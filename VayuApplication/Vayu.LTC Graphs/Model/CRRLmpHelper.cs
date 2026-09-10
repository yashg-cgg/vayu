using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CRRPeriodCongestionLibrary;
using Vayu.DBLibrary;
using Vayu.LTC_Graphs.Design;

namespace Vayu.LTC_Graphs.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.PeriodCongession" />
    public class LmpHelper : PeriodCongession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LmpHelper"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="k">The k.</param>
        public LmpHelper(string key, IEnumerable<LmpHelper> k)
        {
            StartDate = k.Min(x => x.StartDate);
            PeriodYear = StartDate.Year;
            PeriodName = "Year";
            seasonCount = k.Sum(x => x.Count);

            DALMP = k.Average(x => x.DALMP);
            RTLMP = k.Average(x => x.RTLMP);
            CRRLMP = k.Average(x => x.CRRLMP);
            DACRR = k.Average(x => x.DACRR);

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LmpHelper() { }

        #region Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get { return PeriodName + "-" + PeriodYear; } }
        /// <summary>
        /// Gets or sets the dalmp.
        /// </summary>
        /// <value>
        /// The dalmp.
        /// </value>
        public double? DALMP { get; set; }
        /// <summary>
        /// Gets or sets the RTLMP.
        /// </summary>
        /// <value>
        /// The RTLMP.
        /// </value>
        public double? RTLMP { get; set; }
        /// <summary>
        /// Gets or sets the CRRLMP.
        /// </summary>
        /// <value>
        /// The CRRLMP.
        /// </value>
        public double? CRRLMP { get; set; }
        /// <summary>
        /// Gets or sets the daCRR.
        /// </summary>
        /// <value>
        /// The daCRR.
        /// </value>
        public double? DACRR { get; set; }
        /// <summary>
        /// Gets or sets the round1 LMP.
        /// </summary>
        /// <value>
        /// The round1 LMP.
        /// </value>
        public double? Round1LMP { get; set; }
        /// <summary>
        /// Gets or sets the round2 LMP.
        /// </summary>
        /// <value>
        /// The round2 LMP.
        /// </value>
        public double? Round2LMP { get; set; }
        /// <summary>
        /// Gets or sets the round3 LMP.
        /// </summary>
        /// <value>
        /// The round3 LMP.
        /// </value>
        public double? Round3LMP { get; set; }
        /// <summary>
        /// Gets or sets the round4 LMP.
        /// </summary>
        /// <value>
        /// The round4 LMP.
        /// </value>
        public double? Round4LMP { get; set; }
        /// <summary>
        /// The season count
        /// </summary>
        public int seasonCount;
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets all dalmp.
        /// </summary>
        /// <value>
        /// All dalmp.
        /// </value>
        public double? AllDALMP { get; set; }
        /// <summary>
        /// Gets or sets all RTLMP.
        /// </summary>
        /// <value>
        /// All RTLMP.
        /// </value>
        public double? AllRTLMP { get; set; }

        #endregion

        #region Public Variables

        /// <summary>
        /// The start date
        /// </summary>
        public DateTime StartDate;
        /// <summary>
        /// The end date
        /// </summary>
        public DateTime EndDate;
        /// <summary>
        /// The period name
        /// </summary>
        public string PeriodName;
        /// <summary>
        /// The period year
        /// </summary>
        public int PeriodYear;
        /// <summary>
        /// The period type
        /// </summary>
        public string PeriodType;

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the peak.
        /// </summary>
        internal void SetPeak()
        {
            DALMP = PeakDALMP;
            RTLMP = PeakRTLMP;
            Count = PeakDACount;
        }
        /// <summary>
        /// Sets the off peak.
        /// </summary>
        internal void SetOffPeak()
        {
            DALMP = OffPeakDALMP;
            RTLMP = OffPeakRTLMP;
            Count = OffPeakDACount;
        }
        internal void SetPeakWE()
        {
            DALMP = PeakWEDALMP;
            RTLMP = PeakWERTLMP;
            Count = PeakWEDACount;
        }
        /// <summary>
        /// Sets all.
        /// </summary>
        internal void SetAll()
        {
            DALMP = AllDALMP;
            RTLMP = AllRTLMP;
            Count = PeakDACount + OffPeakDACount;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{Vayu.CRRGraphs.Model.LmpHelper}" />
    public class LmpHelperList : List<LmpHelper>
    {
        /// <summary>
        /// Fills the Path List.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        public void Fill(DateTime startDate, DateTime endDate, Strategy strategy, SourceSinkDetail sourceSink, HourType CurrentHourType, bool append = false)
        {
            if (!append)
                this.Clear();

            if (sourceSink == null || strategy == null)
                return;

            #region DB Part
            int sourceID = (strategy.MarketID == 1 ? sourceSink.Source.ID : sourceSink.Source.ID);
            int sinkID = (sourceSink.Sink == null ? -1 : (strategy.MarketID == 1 ? sourceSink.Sink.ID : sourceSink.Sink.ID));
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            endDate = (new DateTime(endDate.Year, endDate.Month, 1)).AddMonths(1).AddDays(-1);

            SqlCommand cmd = Utility.GetCommand(Vayu.CRRPeriodCongestionLibrary.DB.TradingData);
            if (strategy.MarketID == 9)//if (CurrentHourType == HourType.PeakWE)
            {
                cmd.CommandText = "exec GetPeriodicDART '" + startDate.ToString("yyyy-MM-dd") + "' , '" +
                    endDate.ToString("yyyy-MM-dd") + "' , " + sourceID + "," + sinkID + " , " + strategy.MarketID;
            }
            else
            {
                cmd.CommandText = " exec GetPeriodicDART '" + startDate.ToString("yyyy-MM-dd") + "' , '" +
                    endDate.ToString("yyyy-MM-dd") + "' , " + sourceID + "," + sinkID + " , " + strategy.MarketID;
            }
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();

            Hashtable sourceHash = new Hashtable();
            Hashtable sinkeHash = new Hashtable();
            HashSet<int> uniquePeriodKey = new HashSet<int>();

            Action<LmpHelper, SqlDataReader> readCommon = (lmp, dbReader) =>
            {
                lmp.StartDate = Utility.GetDateTime(dbReader[4]);
                lmp.EndDate = Utility.GetDateTime(dbReader[5]);
                lmp.PeriodName = dbReader[8].ToString();
                lmp.PeriodYear = Utility.GetInt(dbReader[9]);
                lmp.PeriodKey = Utility.GetInt(dbReader[10]);
                lmp.PeriodType = dbReader[11].ToString();
                if (lmp.PeriodType == "Monthly")
                    lmp.PeriodYear = lmp.StartDate.Year;
            };

            Func<int, int, LmpHelper> getcongestion = (nodeKey, periodKey) =>
            {
                LmpHelper congession = null;

                if (nodeKey == sourceID && sourceHash.ContainsKey(periodKey))
                    congession = sourceHash[periodKey] as LmpHelper;
                else if (nodeKey == sinkID && sinkeHash.ContainsKey(periodKey))
                    congession = sinkeHash[periodKey] as LmpHelper;
                else
                {
                    congession = new LmpHelper();
                    congession.NodeKey = nodeKey;
                    congession.PeriodKey = periodKey;

                    if (nodeKey == sourceID)
                        sourceHash[periodKey] = congession;
                    else
                        sinkeHash[periodKey] = congession;
                }

                return congession;
            };

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int periodKey = Utility.GetInt(reader[10]);
                    int nodeKey = Utility.GetInt(reader[1]);

                    uniquePeriodKey.Add(periodKey);
                    LmpHelper congession = getcongestion(nodeKey, periodKey);

                    if (congession == null)
                        continue;

                    readCommon(congession, reader);

                    if ("DA".Equals(reader[0].ToString()))
                    {
                        congession.PeakDALMP = Utility.GetDouble(reader[2]);
                        congession.OffPeakDALMP = Utility.GetDouble(reader[3]);
                        congession.PeakDACount = Utility.GetInt(reader[6]);
                        congession.OffPeakDACount = Utility.GetInt(reader[7]);
                        if (strategy.MarketID == 9)
                        {
                            congession.PeakWEDALMP = Utility.GetDouble(reader[12]);
                            congession.PeakWEDACount = Utility.GetInt(reader[13]);
                            congession.AllDALMP = (congession.OffPeakDALMP + congession.PeakDALMP + congession.PeakWEDALMP);
                        }
                        else
                            congession.AllDALMP = congession.OffPeakDALMP + congession.PeakDALMP;
                    }
                    else
                    {
                        congession.PeakRTLMP = Utility.GetDouble(reader[2]);
                        congession.OffPeakRTLMP = Utility.GetDouble(reader[3]);
                        congession.PeakRTCount = Utility.GetInt(reader[6]);
                        congession.OffPeakRTCount = Utility.GetInt(reader[7]);
                        if (strategy.MarketID == 9)
                        {
                            congession.PeakWERTLMP = Utility.GetDouble(reader[12]);
                            congession.PeakWERTCount = Utility.GetInt(reader[13]);
                            congession.AllRTLMP = (congession.OffPeakRTLMP + congession.PeakRTLMP + congession.PeakWERTLMP);
                        }
                        else
                            congession.AllRTLMP = congession.OffPeakRTLMP + congession.PeakRTLMP;
                    }
                }

                reader.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                if (cmd.Connection.State != ConnectionState.Closed)
                    cmd.Connection.Close();
            }

            #endregion

            #region GetPath List
            foreach (var item in uniquePeriodKey)
            {
                if (!sourceHash.ContainsKey(item))
                    continue;

                LmpHelper source = sourceHash[item] as LmpHelper;

                if (!sinkeHash.ContainsKey(item))
                {
                    this.Add(source);
                    continue;
                }

                LmpHelper sink = sinkeHash[item] as LmpHelper;
                sink.CRRLMP -= source.CRRLMP;
                sink.PeakDALMP -= source.PeakDALMP;
                sink.PeakRTLMP -= source.PeakRTLMP;
                sink.OffPeakDALMP -= source.OffPeakDALMP;
                sink.DACRR -= source.DACRR;
                sink.OffPeakRTLMP -= source.OffPeakRTLMP;
                sink.AllDALMP -= source.AllDALMP;
                sink.AllRTLMP -= source.AllRTLMP;
                sink.PeakWEDALMP -= source.PeakWEDALMP;
                sink.PeakWERTLMP -= source.PeakWERTLMP;
                this.Add(sink);
            }
            #endregion
        }

        /// <summary>
        /// Gets the LMPs.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="strategy">The strategy.</param>
        /// <param name="sourceSink">The source sink.</param>
        /// <returns></returns>
        public static Dictionary<Interval, List<LmpHelper>> GetLmpDic(DateTime startDate, DateTime endDate, Strategy strategy, SourceSinkDetail sourceSink, HourType CurrentHourType)
        {
            Dictionary<Interval, List<LmpHelper>> dic = new Dictionary<Interval, List<LmpHelper>>();
            LmpHelperList helperList = new LmpHelperList();
            helperList.Fill(startDate, endDate, strategy, sourceSink, CurrentHourType);
            dic.Add(Interval.Monthly, helperList.Where(x => x.PeriodType == "Monthly").ToList());
            dic.Add(Interval.Quarterly, helperList.Where(x => x.PeriodType == "Seasonal").ToList());

            if (strategy.MarketID == 1)
                dic.Add(Interval.Annually, helperList.Where(x => x.PeriodType == "Annual").ToList());
            else
                dic.Add(Interval.Annually, helperList.Where(x => x.PeriodType == "Monthly").ToList());

            if (strategy.MarketID == 1)
                dic.Add(Interval.LongTerm, helperList.Where(x => x.PeriodType == "Long Term").ToList());
            return dic;
        }

        //public void PeriodicFill(DateTime startDate, DateTime endDate, 
        //    Strategy strategy, SourceSinkNode sourceSink, bool append = false)
        //{
        //    if (!append)
        //        this.Clear();

        //    if (sourceSink == null || strategy == null)
        //        return;

        //    #region DB Part
        //    int sourceID = (strategy.MarketID == 1 ? sourceSink.Source.ID : sourceSink.Source.ID);
        //    int sinkID = (sourceSink.Sink == null ? -1 : (strategy.MarketID == 1 ? sourceSink.Sink.ID : sourceSink.Sink.ID));
        //    startDate = new DateTime(startDate.Year, startDate.Month, 1);
        //    endDate = (new DateTime(endDate.Year, endDate.Month, 1)).AddMonths(1).AddDays(-1);

        //    SqlCommand cmd = Utility.GetCommand(CRRPeriodCongestionLibrary.DB.TradingData);
        //    cmd.CommandText = "select m.NodeKey, m.PeriodKey ";

        //    if (strategy.MarketID == 9)
        //        cmd.CommandText += "  , m.PeakDALMP , m.PeakRTLMP , m.OffPeakDALMP, m.OffPeakRTLMP ";
        //    else
        //        cmd.CommandText += " , m.PeakDACongestion, m.PeakRTCongestion, m.OffPeakDACongestion, m.OffPeakRTCongestion ";

        //    cmd.CommandText += " , p.StartDate,p.EndDate , p.PeakHrs , p.OffPeakHrs , p.PeriodName , p.PeriodYear from " + strategy.TableName +
        //     " m join Period p on m.periodkey = p.PeriodKey where " + " p.StartDate >= '" + startDate.ToString("yyyy-MM-dd") +
        //     "' and p.StartDate <= '" + endDate.ToString("yyyy-MM-dd") + "' and NodeKey in (" + sourceID + "," + sinkID + ") ";

        //    if (cmd.Connection.State != ConnectionState.Open)
        //        cmd.Connection.Open();

        //    try
        //    {
        //        SqlDataReader reader = cmd.ExecuteReader();

        //        while (reader.Read())
        //        {
        //            CRRLmpHelper congession = new CRRLmpHelper();
        //            congession.NodeKey = Utility.GetInt(reader[0]);
        //            congession.PeriodKey = Utility.GetInt(reader[1]);

        //            congession.PeakDALMP = Utility.GetDouble(reader[2]);
        //            congession.PeakRTLMP = Utility.GetDouble(reader[3]);
        //            congession.OffDALMP = Utility.GetDouble(reader[4]);
        //            congession.OffRTLMP = Utility.GetDouble(reader[5]);

        //            congession.StartDate = Utility.GetDateTime(reader[6]);
        //            congession.EndDate = Utility.GetDateTime(reader[7]);
        //            congession.PeakHours = Utility.GetInt(reader[8]);
        //            congession.OffPeakHours = Utility.GetInt(reader[9]);

        //            congession.PeriodName = reader[10].ToString();
        //            congession.PeriodYear = Utility.GetInt(reader[11]);

        //            congession.AllDALMP = congession.OffDALMP + congession.PeakDALMP;
        //            congession.AllRTLMP = congession.OffRTLMP + congession.PeakRTLMP;

        //            this.Add(congession);
        //        }

        //        reader.Close();
        //    }
        //    catch { }
        //    finally
        //    {
        //        if (cmd.Connection.State != ConnectionState.Closed)
        //            cmd.Connection.Close();
        //    }

        //    #endregion

        //    #region GetPath List
        //    Hashtable sourceHash = new Hashtable();
        //    Hashtable sinkeHash = new Hashtable();
        //    HashSet<int> uniquePeriodKey = new HashSet<int>();

        //    foreach (var item in this)
        //    {
        //        uniquePeriodKey.Add(item.PeriodKey);

        //        if (item.NodeKey == sourceID)
        //            sourceHash[item.PeriodKey] = item;
        //        else
        //            sinkeHash[item.PeriodKey] = item;
        //    }

        //    this.Clear();

        //    foreach (var item in uniquePeriodKey)
        //    {
        //        if (!sourceHash.ContainsKey(item))
        //            continue;

        //        CRRLmpHelper source = sourceHash[item] as CRRLmpHelper;

        //        if (!sinkeHash.ContainsKey(item))
        //        {
        //            this.Add(source);
        //            continue;
        //        }

        //        CRRLmpHelper sink = sinkeHash[item] as CRRLmpHelper;
        //        sink.PeakDALMP -= source.PeakDALMP;
        //        sink.PeakRTLMP -= source.PeakRTLMP;
        //        sink.OffDALMP -= source.OffDALMP;
        //        sink.OffRTLMP -= source.OffRTLMP;
        //        sink.AllDALMP -= source.AllDALMP;
        //        sink.AllRTLMP -= source.AllRTLMP;
        //        this.Add(sink);
        //    }
        //    #endregion
        //}
    }
}
