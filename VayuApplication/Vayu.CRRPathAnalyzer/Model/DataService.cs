using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.CRRCalculationLibrary;
using Vayu.CRRPeriodCongestionLibrary;
using Vayu.DBLibrary;

namespace Vayu.CRRPathAnalyzer.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.CRRPathAnalyzer.Model.IDataService" />
    public class DataService : IDataService
    {

        //private SqlCommand mSelectlNodesCoordinateCommand;
        private SqlCommand mSelectPriceCommand;
        /// <summary>
        /// The trading database string
        /// </summary>
        private SqlConnection VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void LoadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectPriceCommand = new SqlCommand();
            mSelectPriceCommand.CommandText = "select c.StartDate, b.LMPOnPeak - a.LMPOnPeak, b.LMPOffPeak - a.LMPOffPeak , c.PeakHrs, c.OffPeakHrs, c.periodkey   from pjm.CrrAuctionNodePrice a, pjm.CrrAuctionNodePrice b, period c  " +
                                    "where a.nodekey = @source and a.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'monthly' and startdate >= @startdate and enddate <= @enddate) " +
                                    "and b.nodekey = @sink and b.periodkey in (select PeriodKey from period where marketkey = 1 and periodtype = 'monthly' and startdate >= @startdate and enddate <= @enddate) " +
                                    "and a.PeriodKey = b.PeriodKey and a.CrrAuctionKey = b.CrrAuctionKey and a.PeriodKey = c.PeriodKey order by a.periodkey, a.Crrauctionkey";
            mSelectPriceCommand.Parameters.Add("@source", "nodekey");
            mSelectPriceCommand.Parameters.Add("@sink", "sink");
            mSelectPriceCommand.Parameters.Add("@startdate", "startdate");
            mSelectPriceCommand.Parameters.Add("@enddate", "enddate");
            mSelectPriceCommand.Connection = VayuConnection;
            //mSelectlNodesCoordinateCommand = new SqlCommand();
            //mSelectlNodesCoordinateCommand.Connection = VayuDbConnection;
        }
        /// <summary>
        /// Fills Source Sink List.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="period">The period.</param>
        public Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> FillData(SourceSinkState state, DateTime startDate, DateTime endDate, string period)
        {
            LoadDBCommands();
            if (state == null)
            {

            }
            // return;

            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            Dictionary<string, DailyValues> priceHash = new Dictionary<string, DailyValues>();
            mSelectPriceCommand.Parameters["@source"].Value = state.Source.NodeKey;
            mSelectPriceCommand.Parameters["@sink"].Value = state.Sink.NodeKey;
            mSelectPriceCommand.Parameters["@startdate"].Value = startDate;
            mSelectPriceCommand.Parameters["@enddate"].Value = endDate.AddMonths(1);
            SqlDataReader reader = mSelectPriceCommand.ExecuteReader();
            while (reader.Read())
            {
                DateTime marketDateTime = reader.GetDateTime(0);
                DailyValues dailyValues = new DailyValues();
                dailyValues.PricePeak = (double)reader.GetDecimal(1);
                dailyValues.PriceOffPeak = (double)reader.GetDecimal(2);
                dailyValues.PeakHours = Convert.ToInt32(reader.GetValue(3)); ;
                dailyValues.OffPeakHours = Convert.ToInt32(reader.GetValue(4));
                dailyValues.PeriodKey = Convert.ToInt32(reader.GetValue(5));
                string date = marketDateTime.Year + "-" + marketDateTime.Month;
                if (priceHash.ContainsKey(date))
                {
                    priceHash.Remove(date);
                }
                priceHash.Add(date, dailyValues);
            }
            reader.Close();
            VayuConnection.Close();
            Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> yearHash = new Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>>();
            SqlConnection cmd = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectDailyValuesCommand = cmd.CreateCommand();
            if (cmd.State != System.Data.ConnectionState.Open)
            {
                cmd.Open();
            }
            mSelectDailyValuesCommand.CommandText = "select p.marketdate, p1.AvgPeakCong - p.AvgPeakCong,p1.AvgOffpeakCong-p.AvgOffpeakCong,p1.Avg24Cong-p.Avg24Cong,p.PeakHours ,p.OffpeakHours, p.markettypecode " +
                "from PJM.nodelmpdailys p join PJM.nodelmpdailys p1 on p.MarketDate=p1.MarketDate and p.NodeKey= " + state.Source.NodeKey + " and p1.NodeKey= " + state.Sink.NodeKey +
                " and p.MarketDate>='" + startDate + "' and p.MarketDate<='" + endDate.AddMonths(1) + "' and p1.MarketDate>= '" + startDate + "' and p1.MarketDate<=' " + endDate.AddMonths(1) +
                "'and p.markettypecode = p1.markettypecode order by p.markettypecode, p.MarketDate desc";
            reader = mSelectDailyValuesCommand.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> monthHash = new Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>();
                Dictionary<int, DailyValues> daysHash = new Dictionary<int, DailyValues>();
                DailyValues congValues = new DailyValues();
                DateTime marketdate = reader.GetDateTime(0);
                if (yearHash.ContainsKey(marketdate.Year))
                {
                    monthHash = yearHash[marketdate.Year];
                }
                if (monthHash.ContainsKey(marketdate.Month))
                {
                    daysHash = monthHash[marketdate.Month].Item2;
                }
                if (daysHash.ContainsKey(marketdate.Day))
                {
                    congValues = daysHash[marketdate.Day];
                }
                string marketType = reader.GetString(6);
                if (marketType == "DA")
                {
                    congValues.DAPeakCong = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1));
                    congValues.DAOffPeakCong = reader.IsDBNull(2) ? double.NaN : Convert.ToDouble(reader.GetValue(2));
                    congValues.DA24Cong = reader.IsDBNull(3) ? double.NaN : Convert.ToDouble(reader.GetValue(3));
                }
                else
                {
                    congValues.RTPeakCong = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1));
                    congValues.RTOffPeakCong = reader.IsDBNull(2) ? double.NaN : Convert.ToDouble(reader.GetValue(2));
                    congValues.RT24Cong = reader.IsDBNull(3) ? double.NaN : Convert.ToDouble(reader.GetValue(3));
                }
                congValues.PeakHours = Convert.ToInt32(reader.GetValue(4));
                congValues.OffPeakHours = Convert.ToInt32(reader.GetValue(5));
                string date = marketdate.Year + "-" + marketdate.Month;
                DailyValues priceDailyValues = new DailyValues();
                if (priceHash.ContainsKey(date))
                {
                    priceDailyValues = priceHash[date];
                }
                Tuple<DailyValues, Dictionary<int, DailyValues>> monthTuple = new Tuple<DailyValues, Dictionary<int, DailyValues>>(priceDailyValues, daysHash);
                daysHash.Remove(marketdate.Day);
                daysHash.Add(marketdate.Day, congValues);
                monthHash.Remove(marketdate.Month);
                monthHash.Add(marketdate.Month, monthTuple);
                yearHash.Remove(marketdate.Year);
                yearHash.Add(marketdate.Year, monthHash);
            }
            reader.Close();
            return yearHash;
        }

        public Dictionary<int, DailyCrrValues> FillDataCrr(SourceSinkState state, DateTime startDate, DateTime endDate, string period, string Type)
        {
            if (state == null)
            {

            }
            // return;
            Dictionary<int, DailyCrrValues> daHashDaily = new Dictionary<int, DailyCrrValues>();
            List<DailyCrrValues> mdaDailyValues = new List<DailyCrrValues>();
            SqlConnection cmd = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectDailyValuesCommand = cmd.CreateCommand();
            if (cmd.State != System.Data.ConnectionState.Open)
                cmd.Open();

            mSelectDailyValuesCommand.CommandText = "select max(r.CrrAuctionKey) as CrrAuctionKey ,(r1.LMPOnPeak-r.LMPOnPeak) as peak,(r1.LMPOffPeak-r.LMPOffPeak)as offpeak,r.periodkey,a.CrrAuctionStartDate,p.OffPeakHrs,p.PeakHrs,p.[24Hrs] " +
 " from [PJM].[CrrAuctionNodePrice] r join PJM.CrrAuction a on a.CrrAuctionKey=r.CrrAuctionKey join [PJM].[CrrAuctionNodePrice] r1 on r.CrrAuctionKey=r1.CrrAuctionKey and " +
 " r.PeriodKey=r1.PeriodKey join Period p on r.PeriodKey=p.PeriodKey  where r.NodeKey in (31,33) and  r1.NodeKey in (31,33) and r.PeriodKey in (select periodkey from period where StartDate >= '2016-10-01' " +
" and StartDate <= '2017-10-01' and PeriodType='Monthly' and marketkey=1) and (r1.LMPOnPeak-r.LMPOnPeak) !=0.000000 " +
 " group by r.PeriodKey,(r1.LMPOnPeak-r.LMPOnPeak) ,(r1.LMPOffPeak-r.LMPOffPeak),r.periodkey,a.CrrAuctionStartDate,p.OffPeakHrs,p.PeakHrs,p.[24Hrs] order by PeriodKey";
            SqlDataReader reader = mSelectDailyValuesCommand.ExecuteReader();

            while (reader.Read())
            {
                DailyCrrValues daCongValues = new DailyCrrValues();
                DateTime marketdate = reader.GetDateTime(4);
                daCongValues.CrrAuctionKey = Convert.ToInt32(reader.GetValue(0));
                daCongValues.LMPOnPeak = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1));
                daCongValues.LMPOffPeak = reader.IsDBNull(2) ? double.NaN : Convert.ToDouble(reader.GetValue(2));
                daCongValues.Lmp24Hrs = daCongValues.LMPOnPeak + daCongValues.LMPOffPeak;
                daCongValues.PeriodKey = Convert.ToInt32(reader.GetValue(3));
                daCongValues.PeakHours = Convert.ToInt32(reader.GetValue(6));
                daCongValues.OffpeakHours = Convert.ToInt32(reader.GetValue(5));
                daCongValues.Hours24 = Convert.ToInt32(reader.GetValue(7));
                daHashDaily.Remove(marketdate.Month);
                daHashDaily.Add(marketdate.Month, daCongValues);
            }
            reader.Close();
            return daHashDaily;
        }
        /// <summary>
        /// Gets the new consolidated data.
        /// </summary>
        /// <param name="procLMP">The proc LMP.</param>
        /// <param name="isPeakOffPeak">The is peak off peak.</param>
        /// <returns></returns>
        private ConsolidatedData GetNewConsolidatedData(PeriodicLMPProc procLMP, string classType)
        {
            ConsolidatedData cd = new ConsolidatedData();
            cd.StartDate = procLMP.StartDate;
            cd.PeriodKey = procLMP.PeriodKey;
            if (classType == "24HR")
                cd.Hours = procLMP.PeakHours + procLMP.OffpeakHours;
            else if (classType == "PEAK")
                cd.Hours = procLMP.PeakHours;
            else if (classType == "OFFPEAK")
                cd.Hours = procLMP.OffpeakHours;
            return cd;
        }

        /// <summary>
        /// Gets the consolidated list.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="isPeakOffPeak">The is peak off peak.</param>
        /// <param name="periodType">Type of the period.</param>
        /// <returns></returns>
        public List<ConsolidatedData> GetConsolidatedList(SourceSinkState state, string classTYpe, string periodType)
        {
            Dictionary<string, ConsolidatedData> consolatedDic = new Dictionary<string, ConsolidatedData>();
            IEnumerable<PeriodicLMPProc> daList = null;
            if (periodType == "Monthly")
                daList = state.procMonthlyLMPData.Where(x => x.MarketTypeCode == "DA" && x.PeriodType == periodType).OrderBy(x => x.StartDate);
            else
                daList = state.procDailyLMPData.Where(x => x.MarketTypeCode == "DA" && x.PeriodType == periodType).OrderBy(x => x.StartDate);

            foreach (var item in daList)
            {
                string key = item.GetKey(periodType);
                if (!consolatedDic.ContainsKey(key))
                    consolatedDic.Add(key, GetNewConsolidatedData(item, classTYpe));

                ConsolidatedData data = consolatedDic[key];
                if (classTYpe == "24HR")
                {
                    if (item.PeakLMP.HasValue && item.OffPeakLMP.HasValue)
                        data.DAValue = (item.PeakLMP + item.OffPeakLMP).Value;
                    else if (item.PeakLMP.HasValue)
                        data.DAValue = (item.PeakLMP).Value;
                    else if (item.OffPeakLMP.HasValue)
                        data.DAValue = (item.OffPeakLMP).Value;
                }
                else if (classTYpe == "PEAK")
                {
                    if (item.PeakLMP.HasValue)
                        data.DAValue = item.PeakLMP.Value;
                    else
                        data.DAValue = 0;
                }
                else if (classTYpe == "OFFPEAK")
                {
                    if (item.OffPeakLMP.HasValue)
                        data.DAValue = item.OffPeakLMP.Value;
                    else
                        data.DAValue = 0;
                }
            }

            IEnumerable<PeriodicLMPProc> rtList = null;
            if (periodType == "Monthly")
                rtList = state.procMonthlyLMPData.Where(x => x.MarketTypeCode == "RT" && x.PeriodType == periodType).OrderBy(x => x.StartDate);
            else
                rtList = state.procDailyLMPData.Where(x => x.MarketTypeCode == "RT" && x.PeriodType == periodType).OrderBy(x => x.StartDate);

            foreach (var item in rtList)
            {
                string key = item.GetKey(periodType);
                if (!consolatedDic.ContainsKey(key))
                    consolatedDic.Add(key, GetNewConsolidatedData(item, classTYpe));

                ConsolidatedData data = consolatedDic[key];
                if (classTYpe == "24HR")
                {
                    if (item.PeakLMP.HasValue && item.OffPeakLMP.HasValue)
                        data.RTValue = (item.PeakLMP + item.OffPeakLMP).Value;
                    else if (item.PeakLMP.HasValue)
                        data.RTValue = (item.PeakLMP).Value;
                    else if (item.OffPeakLMP.HasValue)
                        data.RTValue = (item.OffPeakLMP).Value;

                }
                else if (classTYpe == "PEAK")
                {
                    if (item.PeakLMP.HasValue)
                        data.RTValue = item.PeakLMP.Value;
                    else
                        data.RTValue = 0;
                }
                else if (classTYpe == "OFFPEAK")
                {
                    if (item.OffPeakLMP.HasValue)
                        data.RTValue = item.OffPeakLMP.Value;
                    else
                        data.RTValue = 0;
                }
            }

            if (periodType == "Monthly")
            {
                IEnumerable<PeriodFTRProc> iCrrList = state.procCrrData.Where(x => x.PeriodType == "Monthly").OrderBy(x => x.StartDate);
                foreach (var item in iCrrList)
                {
                    string key = item.PeriodKey.ToString();
                    if (!consolatedDic.ContainsKey(key))
                        continue;

                    ConsolidatedData data = consolatedDic[key];
                    if (classTYpe == "24HR")
                        data.CrrValue = item.FTROnPeak.GetValueOrDefault() + item.FTROffPeak.GetValueOrDefault();
                    else if (classTYpe == "PEAK")
                        data.CrrValue = item.FTROnPeak.GetValueOrDefault();
                    else if (classTYpe == "OFFPEAK")
                        data.CrrValue = item.FTROffPeak.GetValueOrDefault();
                }
            }
            else
            {
                IEnumerable<PeriodFTRProc> iCrrList = state.procCrrData.Where(x => x.PeriodType == "Monthly").OrderBy(x => x.StartDate);
                Dictionary<int, PeriodFTRProc> consolatedCrrDic = iCrrList.Distinct().ToDictionary(x => x.PeriodKey);
                foreach (var item in consolatedDic.Values)
                {
                    if (!consolatedCrrDic.ContainsKey(item.PeriodKey))
                        continue;

                    PeriodFTRProc period = consolatedCrrDic[item.PeriodKey];

                    if (classTYpe == "24HR")
                        item.CrrValue = period.FTROnPeak.GetValueOrDefault() + period.FTROffPeak.GetValueOrDefault();
                    else if (classTYpe == "PEAK")
                        item.CrrValue = period.FTROnPeak.GetValueOrDefault();
                    else if (classTYpe == "OFFPEAK")
                        item.CrrValue = period.FTROffPeak.GetValueOrDefault();
                }
            }

            List<ConsolidatedData> conList = consolatedDic.Values.OrderBy(x => x.StartDate).ToList();
            for (int i = 0; i < conList.Count; i++)
            {
                conList[i].Index = i;
                conList[i].DACrrValue = conList[i].DAValue - conList[i].CrrValue;
                //conList[i].DAValue /= conList[i].Hours;
                //conList[i].RTValue /= conList[i].Hours;
                //conList[i].CrrValue /= conList[i].Hours;
                //conList[i].DACrrValue /= conList[i].Hours;
            }

            return conList;
        }
        /// <summary>
        /// Gets the node coordinate.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="sourcesinkNode">The sourcesink node.</param>
        public void GetNodeCoordinate(Action<ObservableCollection<NodeCoordinate>, Exception> callback, List<int> sourcesinkNode)
        {
            SqlConnection cmd = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectlNodesCoordinateCommand = cmd.CreateCommand();
            if (cmd.State != System.Data.ConnectionState.Open)
                cmd.Open();

            //mSelectlNodesCoordinateCommand.CommandText = "SELECT nodekey, NodeName, Latitude, Longitude, kv, PSSENAME FROM dbo.NodeGeoImport WHERE NodeKey In (" + string.Join(",", sourcesinkNode.ToArray()) + ")";
            mSelectlNodesCoordinateCommand.CommandText = "SELECT nodekey, NodeName, Latitude, Longitude FROM dbo.Node WHERE NodeKey In (" + string.Join(",", sourcesinkNode.ToArray()) + ")";
            SqlDataReader reader = mSelectlNodesCoordinateCommand.ExecuteReader();
            ObservableCollection<NodeCoordinate> coordinateList = new ObservableCollection<NodeCoordinate>();
            while (reader.Read())
            {
                NodeCoordinate coordinate = new NodeCoordinate();
                coordinate.Nodekey = Convert.ToInt32(reader[0].ToString());
                coordinate.NodeName = reader.GetString(1);
                coordinate.MapLocation = new Microsoft.Maps.MapControl.WPF.Location(reader.IsDBNull(2) ? 0 : double.Parse(reader[2].ToString()), reader.IsDBNull(3) ? 0 : double.Parse(reader[3].ToString()));
                // coordinate.KV = reader.IsDBNull(4) ? 0 : double.Parse(reader[4].ToString());
                //coordinate.PsseName = reader.IsDBNull(5) ? "" : reader.GetString(5);
                coordinateList.Add(coordinate);
            }
            reader.Close();

            if (sourcesinkNode.Count == coordinateList.Count)
            {
                for (int i = 0; i < sourcesinkNode.Count; i++)
                {
                    if (!coordinateList[i].Nodekey.Equals(sourcesinkNode[i]))
                    {
                        NodeCoordinate coordinate = new NodeCoordinate();
                        var index = coordinateList.Where(t => t.Nodekey.Equals(sourcesinkNode[i])).FirstOrDefault();
                        if (coordinateList.Contains(index))
                        {
                            coordinateList.Remove(index);
                        }
                        coordinateList.Insert(i, index);
                    }
                }
            }
            if (cmd.State != System.Data.ConnectionState.Closed)
                cmd.Close();
            callback(coordinateList, null);
        }

        public ObservableCollection<NodeCoordinate> GetNodeCoordinate(List<int> sourcesinkNode)
        {
            SqlConnection cmd = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectlNodesCoordinateCommand = cmd.CreateCommand();
            if (cmd.State != System.Data.ConnectionState.Open)
                cmd.Open();
            mSelectlNodesCoordinateCommand.CommandText = "SELECT nodekey, NodeName, Latitude, Longitude, 0, NodeName , Label  FROM dbo.Node    join NodeType on  Node.NodeTypeKey = NodeType.NodeTypeKey  WHERE NodeKey In (" + string.Join(",", sourcesinkNode.ToArray()) + ")";
            SqlDataReader reader = mSelectlNodesCoordinateCommand.ExecuteReader();
            ObservableCollection<NodeCoordinate> coordinateList = new ObservableCollection<NodeCoordinate>();
            while (reader.Read())
            {
                NodeCoordinate coordinate = new NodeCoordinate();
                coordinate.Nodekey = Convert.ToInt32(reader[0].ToString());

                coordinate.NodeName = reader.GetString(1);
                coordinate.MapLocation = new Microsoft.Maps.MapControl.WPF.Location(reader.IsDBNull(2) ? 0 : double.Parse(reader[2].ToString()), reader.IsDBNull(3) ? 0 : double.Parse(reader[3].ToString()));
                coordinate.KV = reader.IsDBNull(4) ? 0 : double.Parse(reader[4].ToString());
                coordinate.PsseName = reader.IsDBNull(5) ? "" : reader.GetString(5);
                coordinate.TypeName = reader.IsDBNull(6) ? "" : reader.GetString(6);
                coordinateList.Add(coordinate);
            }
            if (sourcesinkNode.Count == coordinateList.Count)
            {
                for (int i = 0; i < sourcesinkNode.Count; i++)
                {
                    if (!coordinateList[i].Nodekey.Equals(sourcesinkNode[i]))
                    {
                        NodeCoordinate coordinate = new NodeCoordinate();
                        var index = coordinateList.Where(t => t.Nodekey.Equals(sourcesinkNode[i])).FirstOrDefault();
                        if (coordinateList.Contains(index))
                        {
                            coordinateList.Remove(index);
                        }
                        coordinateList.Insert(i, index);
                    }
                }
            }
            reader.Close();
            cmd.Close();
            return coordinateList;
        }

        /// <summary>
        /// Gets the caiso nodes.
        /// </summary>
        /// <returns></returns>
        public List<PricingNode> GetCAISONodes()
        {
            List<PricingNode> nodeList = new List<PricingNode>();
            SqlConnection cmd = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectlCAISOCommand = cmd.CreateCommand();
            if (cmd.State != System.Data.ConnectionState.Open)
                cmd.Open();

            mSelectlCAISOCommand.CommandText = "select NodeKey,NodeName,ExternalNodeId,NodeTypeKey ,Zone from Node where NodeKey in " +
            " (select distinct NodeKey from CAISO.[CrrAuctionNodePrice] ) ";
            SqlDataReader reader = mSelectlCAISOCommand.ExecuteReader();

            while (reader.Read())
            {
                PricingNode sourceNode = new PricingNode();
                sourceNode.NodeKey = Convert.ToInt32(reader.GetDecimal(0));
                sourceNode.NodeName = reader.GetString(1);
                sourceNode.ExternalNodeId = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetDecimal(2));
                sourceNode.NodeTypeKey = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetDecimal(3));
                sourceNode.Zone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                sourceNode.MarketKey = 7;
                nodeList.Add(sourceNode);
            }

            return nodeList;
        }

        /// <summary>
        /// Gets the periods.
        /// </summary>
        /// <param name="datelist">The datelist.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public List<int> GetPeriods(List<DateTime> datelist, int marketKey)
        {
            List<int> periodList = new List<int>();
            foreach (DateTime sDate in datelist)
            {
                SqlCommand cmd = VayuConnection.CreateCommand();
                cmd.CommandText = "select PeriodKey from Period where MarketKey = @MarketKey and PeriodType = 'monthly' and StartDate = @StartDate and EndDate = @EndDate";
                cmd.Parameters.AddWithValue("@MarketKey", marketKey);
                cmd.Parameters.AddWithValue("@StartDate", sDate);
                DateTime eDate = sDate.AddMonths(1).AddDays(-1);
                cmd.Parameters.AddWithValue("@EndDate", eDate);
                cmd.Connection.Open();
                int periodKey = (int)cmd.ExecuteScalar();
                cmd.Connection.Close();
                periodList.Add(periodKey);
            }
            return periodList;
        }

        public Dictionary<int, List<string>> FillPnodeHash()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            Dictionary<int, List<string>> tempPnodeHash = new Dictionary<int, List<string>>();
            // List<string> NodeNameList = new List<string>();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "select ExternalNodeID , NodeName from Node where ExternalNodeID in (" +
                                "select ExternalNodeID   from Node where MarketKey = 1 group by externalnodeid   having  COUNT(ExternalNodeID) >1 " +
                                ")";
            cmd.Connection = VayuConnection;
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                int externalNodeId = Convert.ToInt32(rdr.GetValue(0));
                string nodeName = rdr.GetValue(1).ToString();
                if (!tempPnodeHash.ContainsKey(externalNodeId))
                {
                    List<string> NodeNameList = new List<string>();
                    NodeNameList.Add(nodeName);
                    tempPnodeHash.Add(externalNodeId, NodeNameList);
                }
                else
                {
                    List<string> NodeNameList = tempPnodeHash[externalNodeId];
                    NodeNameList.Add(nodeName);
                }
            }
            rdr.Close();
            VayuConnection.Close();
            return tempPnodeHash;
        }

        public string GetSpecificNodeName(string NodeName)
        {
            string exactNodeName = string.Empty;
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "select NodeName from node where MarketKey = 1 " +
                                "and NodeName like '" + NodeName + "'";

            cmd.Connection = VayuConnection;
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();

            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                exactNodeName = rdr.GetValue(0).ToString();
            }
            rdr.Close();
            VayuConnection.Close();

            return exactNodeName;
        }

        public List<PricingNode> GetPJMSourceNodes()
        {
            List<PricingNode> nodeList = new List<PricingNode>();
            SqlConnection cmd = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectlPJMCommand = cmd.CreateCommand();
            if (cmd.State != System.Data.ConnectionState.Open)
                cmd.Open();

            mSelectlPJMCommand.CommandText = "select NodeKey,NodeName,ExternalNodeId,NodeTypeKey ,Zone from Node where NodeKey in " +
            " (select distinct SourceNodeKey from PJM.CrrValidOptionPaths ) ";
            SqlDataReader reader = mSelectlPJMCommand.ExecuteReader();

            while (reader.Read())
            {
                PricingNode sourceNode = new PricingNode();
                sourceNode.NodeKey = Convert.ToInt32(reader.GetValue(0));
                sourceNode.NodeName = reader.GetString(1);
                sourceNode.ExternalNodeId = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[2]).GetValueOrDefault();
                sourceNode.NodeTypeKey = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
                sourceNode.Zone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                sourceNode.MarketKey = 1;
                nodeList.Add(sourceNode);
            }

            return nodeList;
        }

        public List<PricingNode> GetPJMSinkNodes()
        {
            List<PricingNode> nodeList = new List<PricingNode>();
            SqlConnection cmd = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectlPJMCommand = cmd.CreateCommand();
            if (cmd.State != System.Data.ConnectionState.Open)
                cmd.Open();

            mSelectlPJMCommand.CommandText = "select NodeKey,NodeName,ExternalNodeId,NodeTypeKey ,Zone from Node where NodeKey in " +
            " (select distinct SinkNodeKey from PJM.CrrValidOptionPaths ) ";
            SqlDataReader reader = mSelectlPJMCommand.ExecuteReader();

            while (reader.Read())
            {
                PricingNode sourceNode = new PricingNode();
                sourceNode.NodeKey = Convert.ToInt32(reader.GetValue(0));
                sourceNode.NodeName = reader.GetString(1);
                sourceNode.ExternalNodeId = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[2]).GetValueOrDefault();
                sourceNode.NodeTypeKey = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
                sourceNode.Zone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                sourceNode.MarketKey = 1;
                nodeList.Add(sourceNode);
            }

            return nodeList;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class NodeCoordinate
    {
        /// <summary>
        /// Gets or sets the nodekey.
        /// </summary>
        /// <value>
        /// The nodekey.
        /// </value>
        public int Nodekey { get; set; }
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the map location.
        /// </summary>
        /// <value>
        /// The map location.
        /// </value>
        public Microsoft.Maps.MapControl.WPF.Location MapLocation { get; set; }
        /// <summary>
        /// Gets or sets the kv.
        /// </summary>
        /// <value>
        /// The kv.
        /// </value>
        public double? KV { get; set; }
        /// <summary>
        /// Gets or sets the name of the psse.
        /// </summary>
        /// <value>
        /// The name of the psse.
        /// </value>
        public string PsseName { get; set; }

        public string TypeName { get; set; }
    }
}
