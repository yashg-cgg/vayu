using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.CongestionVolatilityIndex.Model
{
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;
        private SqlCommand mSelectSourceSinkNodeCommand;
        private SqlCommand mSelectPJMUptosNodeCommand;
        private SqlCommand mSelectErcotUptosNodeCommand;
        private List<string> mPJMSourceSinkList = new List<string>();
        private List<string> mUptosPathList = new List<string>();

        private Dictionary<string, Dictionary<DateTime, double>> mAvgHash = null;
        List<CongestionVolatility> tempdictdata = null;
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("Welcome to MVVM Light");
            callback(item, null);
        }
        public void GetCongestionVolatility(Action<System.Collections.Generic.List<CongestionVolatility>, Exception> callback, int marketKey, DateTime fromDate, DateTime throDate, string Type, bool HourlyChecked, bool FourhourlyChecked, bool DailyChecked)
        {
            bool isDa;
            if (Type == "RT")
                isDa = false;
            else
                isDa = true;
            int totalhour = 0;
            List<CongestionVolatility> tempList = FillAvgHashByService(marketKey, isDa, fromDate, throDate);
            foreach (var item in mAvgHash)
            {
                try
                {
                    string[] conMon = item.Key.Split(new string[] { "?split?" }, StringSplitOptions.None);
                    var conMonList = tempList.Where(a => a.ConstraintText == conMon[0] && a.ContingencyText == conMon[1]);
                    foreach (DateTime dObj in conMonList.Select(p => p.ConstraintDate.Date).Distinct())
                    {
                        try
                        {
                            var constObjList = conMonList.Where(a => a.ConstraintDate.Date == dObj);
                            if (marketKey == 1)
                            {
                                // if (loadDictHash.Count() == 0)
                                {
                                    List<DateTime> datetimeList = constObjList.Select(x => x.ConstraintDate).ToList<DateTime>();
                                }
                            }

                            if (constObjList.Count() == 0)
                                continue;

                            try
                            {
                                CongestionVolatility constObj = constObjList.First();
                                string dateCounter = string.Empty;
                                string dt = string.Empty; int hrs = 0;
                                if (constObjList.FirstOrDefault() != null)
                                {
                                    dt = constObjList.FirstOrDefault().ConstraintDate.ToString("yyyy-MM-dd");
                                    dateCounter = dt;//.Date.AddHours(hrs);
                                }
                                if (constObj.ConstraintText == "GRACETON230 KV  GRA-SAF")
                                {

                                }
                                if (!isDa)
                                {
                                    var data = from a in item.Value
                                               where isDa ? (a.Key >= dObj && a.Key < dObj.AddDays(1)) : a.Key.Date == dObj
                                               group a by a.Key.Hour into g
                                               select new { time = g.Key, val = g.Sum(o => o.Value) / (isDa ? 1 : 12) };
                                    foreach (var annItem in data)
                                    {
                                        try
                                        {
                                            constObj.GetType().GetProperty("HE" + (annItem.time == 0 ? "1" : (annItem.time + 1).ToString())).SetValue(constObj, annItem.val == 0 ? (double?)null : annItem.val);
                                            totalhour += Convert.ToInt32(annItem.val);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    constObj.ConstraintDate = DateTime.Parse(dt);
                                    constObj.Price = data.Sum(a => a.val) / 24;
                                }
                                else
                                {
                                    //foreach (var a in item.Value)
                                    {
                                        Dictionary<DateTime, double> shadowPriceDict = item.Value;
                                        foreach (var item1 in shadowPriceDict)
                                        {
                                            if (dObj == item1.Key.Date)
                                                constObj.GetType().GetProperty("HE" + (item1.Key.Hour + 1).ToString()).SetValue(constObj, item1.Value == 0 ? (double?)null : item1.Value);
                                        }

                                    }
                                    constObj.Price = (item.Value.Sum(a => a.Value)) / 24;
                                    constObj.ConstraintDate = DateTime.Parse(dt);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                catch
                {

                }
            }
            List<DateTime> dtlist = new List<DateTime>();
            dtlist = tempList.Select(item => item.ConstraintDate).Distinct().ToList();
            tempdictdata = CalculateHour(tempList, dtlist, HourlyChecked, FourhourlyChecked, DailyChecked);
            callback(tempdictdata, null);
        }

        public List<CongestionVolatility> FillAvgHashByService(int marketKey, bool isDa, DateTime fromDate, DateTime? throDate)
        {
            List<CongestionVolatility> tempList = new List<CongestionVolatility>();
            ConstraintHelper helper = new ConstraintHelper();
            IConstraintInfoProvider chelper = helper.GetInstance();
            List<LatestConstraint> constraintList = null;
            Dictionary<int, string> dictZones = new Dictionary<int, string>();
            //throDate = (throDate.HasValue ? throDate.Value.AddDays(1) : fromDate.AddDays(1));
            if (isDa)
                fromDate = fromDate.AddHours(1);

            if (isDa)
                constraintList = chelper.GetConstraintDA(marketKey, Configuration.GetAbsoluteDate(fromDate), Configuration.GetAbsoluteDate(throDate.Value));
            else
                constraintList = chelper.GetConstraintRT(marketKey, Configuration.GetAbsoluteDate(fromDate), Configuration.GetAbsoluteDate(throDate.Value), false);

            mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in constraintList)
            {
                try
                {
                    string SourceZonename = string.Empty, SinkZonename = string.Empty;
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;
                    DateTime mktDate;
                    if (isDa)
                        mktDate = item.MarketDate.AddHours(-1);
                    else
                        mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    int sourceNodekey = item.SourceNodeKey;
                    int sinkNodekey = item.SinkNodeKey;

                    CongestionVolatility constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new CongestionVolatility { ConstraintDate = mktDate, ContingencyText = contingency, ConstraintText = constraint, SourceNodekey = sourceNodekey, SinkNodekey = sinkNodekey, SourceZone = SourceZonename, SinkZone = SinkZonename };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (!isDa)
                            {
                                if (!double.IsNaN(item.ShadowPriceNaN))
                                    hourValues[mktDate] += hourValues[mktDate] + Math.Abs(item.ShadowPrice);
                            }
                        }
                        else
                        {
                            hourValues.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        }
                    }
                    else
                    {
                        Dictionary<DateTime, double> tempHash = new Dictionary<DateTime, double>();
                        tempHash.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        mAvgHash.Add(keyText, tempHash);
                    }
                    if (tempList.Contains(constraintItem))
                        tempList.Remove(constraintItem);
                    tempList.Add(constraintItem);
                }
                catch
                {
                }
            }
            return tempList;
        }

        public List<CongestionVolatility> CalculateHour(List<CongestionVolatility> tempList, List<DateTime> dtList, bool HourlyChecked, bool FourhourlyChecked, bool DailyChecked)
        {
            List<CongestionVolatility> hrdata = new List<CongestionVolatility>();
            foreach (var itemdate in dtList.OrderBy(a => a.Date))
            {
                DateTime date = Convert.ToDateTime(itemdate.ToString("MM-dd-yyyy"));
                CongestionVolatility congestion = new CongestionVolatility();
                #region  HourlyChecked
                if (HourlyChecked)
                {
                    congestion.ConstraintDate = date.AddHours(0);
                    congestion.value = 0;
                    hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE1);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(1);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE2);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(2);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE3);
                    //if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(3);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE4);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(4);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE5);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(5);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE6);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(6);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE7);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(7);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE8);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(8);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE9);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(9);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE10);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(10);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE11);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(11);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE12);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(12);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE13);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(13);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE14);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(14);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE15);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(15);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE16);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(16);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE17);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(17);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE18);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(18);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE19);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(19);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE20);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(20);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE21);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(21);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE22);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(22);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE23);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(23);
                        hrdata.Add(congestion);
                    }
                    congestion = new CongestionVolatility();
                    congestion.value = tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE24);
                    // if (congestion.value != 0.0)
                    {
                        congestion.ConstraintDate = date.AddHours(24);
                        hrdata.Add(congestion);
                    }
                }
                #endregion HourlyChecked
                #region FourhourlyChecked
                if (FourhourlyChecked)
                {
                    double HE1 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE1);
                    double HE2 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE2);
                    double HE3 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE3);
                    double HE4 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE4);
                    double HE5 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE5);
                    double HE6 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE6);
                    double HE7 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE7);
                    double HE8 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE8);
                    double HE9 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE9);
                    double HE10 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE10);
                    double HE11 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE11);
                    double HE12 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE12);
                    double HE13 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE13);
                    double HE14 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE14);
                    double HE15 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE15);
                    double HE16 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE16);
                    double HE17 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE17);
                    double HE18 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE18);
                    double HE19 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE19);
                    double HE20 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE20);
                    double HE21 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE21);
                    double HE22 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE22);
                    double HE23 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE23);
                    double HE24 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE24);

                    //congestion.value = 00;
                    //congestion.ConstraintDate = date.AddHours(0);
                    //hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.value = HE1 + HE2 + HE3 + HE4;
                    congestion.ConstraintDate = date.AddHours(4);
                    hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.value = HE5 + HE6 + HE7 + HE8;
                    congestion.ConstraintDate = date.AddHours(8);
                    hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.value = HE9 + HE10 + HE11 + HE12;
                    congestion.ConstraintDate = date.AddHours(12);
                    hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.value = HE13 + HE14 + HE15 + HE16;
                    congestion.ConstraintDate = date.AddHours(16);
                    hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.value = HE17 + HE18 + HE19 + HE20;
                    congestion.ConstraintDate = date.AddHours(20);
                    hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.value = HE21 + HE22 + HE23 + HE24;
                    congestion.ConstraintDate = date.AddHours(23);
                    hrdata.Add(congestion);
                }
                #endregion FourhourlyChecked
                #region DailyChecked
                if (DailyChecked)
                {
                    double HE1 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE1);
                    double HE2 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE2);
                    double HE3 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE3);
                    double HE4 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE4);
                    double HE5 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE5);
                    double HE6 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE6);
                    double HE7 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE7);
                    double HE8 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE8);
                    double HE9 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE9);
                    double HE10 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE10);
                    double HE11 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE11);
                    double HE12 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE12);
                    double HE13 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE13);
                    double HE14 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE14);
                    double HE15 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE15);
                    double HE16 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE16);
                    double HE17 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE17);
                    double HE18 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE18);
                    double HE19 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE19);
                    double HE20 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE20);
                    double HE21 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE21);
                    double HE22 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE22);
                    double HE23 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE23);
                    double HE24 = (double)tempList.Where(item => item.ConstraintDate == itemdate).Sum(item => item.HE24);
                    //congestion.value = 00;
                    //congestion.ConstraintDate = date.AddHours(0);
                    //hrdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    congestion.ConstraintDate = date.AddHours(23);
                    congestion.value = HE1 + HE2 + HE3 + HE4 + HE5 + HE6 + HE7 + HE8 + HE9 + HE10 + HE11 + HE12 + HE13 + HE14 + HE15 + HE16 + HE17 + HE18 + HE19 + HE20 + HE21 + HE22 + HE23 + HE24;
                    hrdata.Add(congestion);
                }
                #endregion DailyChecked
            }
            return hrdata;
        }

        private void LoadDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            mSelectSourceSinkNodeCommand = new SqlCommand();
            mSelectSourceSinkNodeCommand.CommandText = "select src.SourceNodeName, sink.SinkNodeName from EESPathList (nolock) src inner join Node (nolock) n on n.NodeKey = src.SourceNodeKey inner join EESPathList sink on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey inner join Node n2 on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = @MarketKey and n2.MarketKey = @MarketKey and src.MarketKey = @MarketKey and sink.MarketKey = @MarketKey order by n.NodeName";
            mSelectSourceSinkNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectSourceSinkNodeCommand.Connection = VayuConnection;

            mSelectPJMUptosNodeCommand = new SqlCommand();
            mSelectPJMUptosNodeCommand.CommandText = "Select distinct SourceName from EESPathList ";
            mSelectPJMUptosNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectPJMUptosNodeCommand.Connection = VayuConnection;

            mSelectErcotUptosNodeCommand = new SqlCommand();
            mSelectErcotUptosNodeCommand.CommandText = "Select distinct SourceName from EESPathList ";
            mSelectErcotUptosNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectErcotUptosNodeCommand.Connection = VayuConnection;
        }
        public List<string> FillSourceSinkHash(int Marketkey)
        {
            try
            {
                LoadDB();
                if (Marketkey == 1)
                {
                    if (VayuConnection.State == ConnectionState.Closed)
                        VayuConnection.Open();
                    mSelectSourceSinkNodeCommand.Parameters["@MarketKey"].Value = 1;
                    SqlDataReader reader = mSelectSourceSinkNodeCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string source = reader.GetString(0);
                        if (!mPJMSourceSinkList.Contains(source))
                        {
                            mPJMSourceSinkList.Add(source);
                        }
                        string sink = reader.GetString(1);
                        if (!mPJMSourceSinkList.Contains(sink))
                        {
                            mPJMSourceSinkList.Add(sink);
                        }
                        mUptosPathList.Add(source + "?" + sink);
                    }
                    reader.Close();
                    VayuConnection.Close();
                }
            }
            catch (Exception)
            {

            }
            return mUptosPathList.ToList();
        }

        public List<string> GetUptosNode(int Marketkey)
        {
            List<string> UptosNodeList = new List<string>();
            try
            {
                LoadDB();
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                SqlDataReader reader = null;
                if (Marketkey == 1)
                {
                    mSelectPJMUptosNodeCommand.Parameters["@MarketKey"].Value = Marketkey;
                    reader = mSelectPJMUptosNodeCommand.ExecuteReader();
                }
                else
                {
                    mSelectErcotUptosNodeCommand.Parameters["@MarketKey"].Value = Marketkey;
                    reader = mSelectErcotUptosNodeCommand.ExecuteReader();
                }
                while (reader.Read())
                {
                    string source = reader.GetString(0);
                    if (!UptosNodeList.Contains(source))
                        UptosNodeList.Add(source);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return UptosNodeList.ToList();
        }
    }
}
