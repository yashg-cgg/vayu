using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Vayu.CommonAccessLibrary;
using Vayu.CRRPeriodCongestionLibrary;
using Vayu.DBLibrary;
using Vayu.LTC_Graphs.Design;

namespace Vayu.LTC_Graphs.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class DataService : IDataService
    {
        private double? CRRTotal = double.MinValue;
        #region DART Price
        /// <summary>
        /// Gets the float.
        /// </summary>
        /// <param name="objValue">The object value.</param>
        /// <returns></returns>
        private float? GetFloat(object objValue)
        {
            string strValue = (objValue ?? "").ToString();
            float fValue;
            if (float.TryParse(strValue, out fValue))
                return fValue;

            return null;
        }

        /// <summary>
        /// Appends the log.
        /// </summary>
        /// <param name="text">The text.</param>
        private void AppendLog(string text)
        {
            string fileName = @"C:\temp\graph.csv";
            StreamWriter writer = new StreamWriter(fileName, true);
            writer.WriteLine(text);
            writer.Close();
        }

        /// <summary>
        /// Gets the dart prices.
        /// </summary>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="from">From.</param>
        /// <param name="thro">The thro.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public Dictionary<Interval, List<LmpHelper>> GetDARTPrices(SourceSinkDetail sourceSink, DateTime from, DateTime thro, int marketKey, HourType CurrentHourType)
        {
            Strategy stra = Strategy.GetStrategy(marketKey);
            return LmpHelperList.GetLmpDic(from, thro, stra, sourceSink, CurrentHourType);
        }
        #endregion

        #region CRR Price

        /// <summary>
        /// Gets all CRR data.
        /// </summary>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public Dictionary<HourType, Dictionary<int, CRR>> GetAllCRRData(SourceSinkDetail sourceSink, DateTime start, DateTime end, int market)
        {
            Dictionary<HourType, Dictionary<int, CRR>> results = new Dictionary<HourType, Dictionary<int, CRR>>();

            if (market != 2 && market != 7 && market != 1 && market != 12 && market != 9)
                return results;

            int sourceID = -1;
            int sinkID = -1;

            HashSet<int> periodKeyHash = new HashSet<int>();
            Dictionary<string, CRR> dayDic = new Dictionary<string, CRR>();
            Dictionary<string, CRR> peakDic = new Dictionary<string, CRR>();
            Dictionary<string, CRR> offDic = new Dictionary<string, CRR>();
            Dictionary<string, CRR> peakWEDic = new Dictionary<string, CRR>();
            #region Functions
            Func<int, int, string> GetKey = (pKey, nodeID) =>
            {
                string key = pKey.ToString() + nodeID.ToString();
                return key;
            };

            Action<Dictionary<string, CRR>, string, int, double?, int, DateTime, int, HourType, int> appendValues = (dic, stringKey, roundID, doubleValue, periodKey, date, hourCount, hourType, TotalHr) =>
            {
                if (!dic.ContainsKey(stringKey))
                    dic.Add(stringKey, new CRR());
                else
                {
                    if (dic[stringKey].AuctionDate > date)
                        return;
                }

                dic[stringKey].PeriodKey = periodKey;
                dic[stringKey].AuctionDate = date;
                dic[stringKey].TotalHrs = TotalHr;
                //
                if (HourType.Peak == hourType)
                    dic[stringKey].PeakWDHrs = hourCount;
                else if (HourType.OffPeak == hourType)
                    dic[stringKey].OffPeakHrs = hourCount;
                else if (HourType.PeakWE == hourType)
                    dic[stringKey].PeakWEHrs = hourCount;
                else
                    dic[stringKey].TotalHrs = hourCount;
                //
                if (roundID == 1)
                    dic[stringKey].Round1CRR = doubleValue;
                else if (roundID == 2)
                    dic[stringKey].Round2CRR = doubleValue;
                else if (roundID == 3)
                    dic[stringKey].Round3CRR = doubleValue;
                else
                    dic[stringKey].Round4CRR = doubleValue;
            };

            Func<Dictionary<string, CRR>, Dictionary<int, CRR>> process = (dic) =>
            {
                Dictionary<int, CRR> periodHash = new Dictionary<int, CRR>();
                foreach (var pKey in periodKeyHash)
                {
                    CRR CRR = new CRR();

                    if (dic.ContainsKey(GetKey(pKey, sinkID)))
                    {
                        CRR sinkCRR = dic[GetKey(pKey, sinkID)];
                        CRR.Round1CRR = sinkCRR.Round1CRR;
                        CRR.Round2CRR = sinkCRR.Round2CRR;
                        CRR.Round3CRR = sinkCRR.Round3CRR;
                        CRR.Round4CRR = sinkCRR.Round4CRR;

                        CRR.PeakWDHrs = sinkCRR.PeakWDHrs;
                        CRR.OffPeakHrs = sinkCRR.OffPeakHrs;
                        CRR.PeakWEHrs = sinkCRR.PeakWEHrs;
                        CRR.TotalHrs = sinkCRR.TotalHrs;
                    }

                    if (dic.ContainsKey(GetKey(pKey, sourceID)))
                    {
                        CRR sourceCRR = dic[GetKey(pKey, sourceID)];
                        CRR.Round1CRR = -(CRR.Round1CRR.HasValue ? CRR.Round1CRR - sourceCRR.Round1CRR : sourceCRR.Round1CRR);
                        CRR.Round2CRR = -(CRR.Round2CRR.HasValue ? CRR.Round2CRR - sourceCRR.Round2CRR : sourceCRR.Round2CRR);
                        CRR.Round3CRR = -(CRR.Round3CRR.HasValue ? CRR.Round3CRR - sourceCRR.Round3CRR : sourceCRR.Round3CRR);
                        CRR.Round4CRR = -(CRR.Round4CRR.HasValue ? CRR.Round4CRR - sourceCRR.Round4CRR : sourceCRR.Round4CRR);
                    }

                    if (market == 2 || market == 12)
                    {
                        CRR.Round1CRR *= -1;
                        CRR.Round2CRR *= -1;
                        CRR.Round3CRR *= -1;
                        CRR.Round4CRR *= -1;
                    }

                    if (CRR.Round1CRR.HasValue || CRR.Round2CRR.HasValue
                        || CRR.Round3CRR.HasValue || CRR.Round4CRR.HasValue)
                        periodHash[pKey] = CRR;
                }
                return periodHash;
            };
            Func<Dictionary<HourType, Dictionary<int, CRR>>, Dictionary<int, CRR>> process24Hr = (dicResult) =>
            {
                Dictionary<int, CRR> periodHash = new Dictionary<int, CRR>();
                double? latestCRR; int? totalhr;
                //foreach (var item in dicResult)
                {
                    Dictionary<int, CRR> DicPeak = dicResult[HourType.Peak];
                    Dictionary<int, CRR> DicOffpeak = dicResult[HourType.OffPeak];
                    Dictionary<int, CRR> DicPeakWE = dicResult[HourType.PeakWE];
                    Dictionary<int, CRR> DicDay = new Dictionary<int, CRR>();
                    foreach (int pKey in periodKeyHash)
                    {
                        CRR CRR = new CRR();

                        CRR = DicPeak[pKey];
                        latestCRR = CRR.LatestCRR;
                        totalhr = CRR.TotalHrs;
                        CRRTotal = latestCRR * CRR.PeakWDHrs;
                        CRR = new CRR();
                        CRR = DicOffpeak[pKey];
                        latestCRR = CRR.LatestCRR;
                        CRRTotal += latestCRR * CRR.OffPeakHrs;
                        CRR = new CRR();
                        CRR = DicPeakWE[pKey];
                        latestCRR = CRR.LatestCRR;
                        CRRTotal += latestCRR * CRR.PeakWEHrs;
                        CRR = new CRR();
                        CRR.latestCRR = (CRRTotal / totalhr);
                        CRR.Round1CRR = (CRRTotal / totalhr);
                        periodHash[pKey] = CRR;
                    }
                }
                return periodHash;
            };
            #endregion

            #region Logic
            DateTime endDt = new DateTime(end.AddMonths(1).Year, end.AddMonths(1).Month, 1);
            SqlCommand command = GetCommand(DB.TradingData);
            Func<string, bool> GetHourType = null;

            sourceID = sourceSink.Source.ID;
            sinkID = (sourceSink.Sink == null ? -1 : sourceSink.Sink.ID);
            GetHourType = (str) => { return str == "ON" || str == "Peak" || str == "OnPeak"; };
            command.CommandText = "exec GetAllCRRData '" + start.ToString("yyyy-MM-dd") + "' , '" + endDt.ToString("yyyy-MM-dd") + "' , "
                + sourceID + " , " + sinkID + " , " + market;
            if (command.Connection.State != ConnectionState.Open)
                command.Connection.Open();

            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime date = Utility.GetDateTime(reader[0]);
                int nodeID = Utility.GetInt(reader[1]);
                string type = reader[2].ToString();
                double value = Utility.GetDouble(reader[3]);
                int roundID = Utility.GetInt(reader[4]);
                int periodKey = Utility.GetInt(reader[5]);
                string key = GetKey(periodKey, nodeID);
                double offValue = Utility.GetDouble(reader[10]);
                periodKeyHash.Add(periodKey);
                if (market == 1)
                {
                    appendValues(peakDic, key, roundID, value, periodKey, date, 0, HourType.Peak, 0);
                    appendValues(offDic, key, roundID, offValue, periodKey, date, 0, HourType.OffPeak, 0);

                    appendValues(dayDic, key, roundID, value, periodKey, date, 0, HourType.Day, 0);
                    appendValues(dayDic, key, roundID, offValue, periodKey, date, 0, HourType.Day, 0);
                }
                else if (market == 9)
                {
                    double peakwevalue = Utility.GetDouble(reader[11]);
                    int PeakWDHr = Utility.GetInt(reader[8]);
                    int OffPeakHr = Utility.GetInt(reader[9]);
                    int PeakWEHr = Utility.GetInt(reader[12]);
                    int TotalHr = Utility.GetInt(reader[13]);
                    appendValues(peakDic, key, roundID, value, periodKey, date, PeakWDHr, HourType.Peak, TotalHr);
                    appendValues(offDic, key, roundID, offValue, periodKey, date, OffPeakHr, HourType.OffPeak, TotalHr);
                    appendValues(peakWEDic, key, roundID, peakwevalue, periodKey, date, PeakWEHr, HourType.PeakWE, TotalHr);

                    //appendValues(dayDic, key, roundID, value, periodKey, date);
                    //appendValues(dayDic, key, roundID, offValue, periodKey, date);
                    //appendValues(dayDic, key, roundID, peakwevalue, periodKey, date);
                }
                else
                {
                    if (GetHourType(type))
                        appendValues(peakDic, key, roundID, value, periodKey, date, 0, HourType.Peak, 0);
                    else
                        appendValues(offDic, key, roundID, value, periodKey, date, 0, HourType.OffPeak, 0);

                    appendValues(dayDic, key, roundID, value, periodKey, date, 0, HourType.Day, 0);
                }
            }

            if (command.Connection.State != ConnectionState.Closed)
                command.Connection.Close();

            foreach (HourType item in Enum.GetValues(typeof(HourType)))
            {
                switch (item)
                {
                    case HourType.Peak:
                        results[HourType.Peak] = process(peakDic);
                        break;
                    case HourType.OffPeak:
                        results[HourType.OffPeak] = process(offDic);
                        break;
                    case HourType.PeakWE:
                        results[HourType.PeakWE] = process(peakWEDic);
                        break;
                    case HourType.Day:
                        if (market == 9)
                            results[HourType.Day] = process24Hr(results);
                        else
                            results[HourType.Day] = process(dayDic);
                        break;
                }
            }
            #endregion

            return results;
        }

        #endregion

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="dbToUse">The database to use.</param>
        /// <returns></returns>
        public SqlCommand GetCommand(DB dbToUse)
        {
            SqlCommand command = null;
            switch (dbToUse)
            {
                case DB.TradingData:
                    SqlConnection trading = new VayuDBConnection().GetInstance().GetSqlConnection();
                    command = trading.CreateCommand();
                    break;
                case DB.RiskData:
                    SqlConnection risk = new VayuDBConnection().GetInstance().GetSqlConnection();
                    command = risk.CreateCommand();
                    break;
                default:
                    break;
            }

            return command;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DB
    {
        /// <summary>
        /// The trading data
        /// </summary>
        TradingData,
        /// <summary>
        /// The risk data
        /// </summary>
        RiskData
    }
}
