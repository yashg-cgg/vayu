using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.LMPStatistics.ViewModels;
using Vayu.NodePriceLibrary;

namespace Vayu.LMPStatistics.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.LMPStatistics.Model.IDataService" />
    public class DataService : IDataService
    {
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        public SqlConnection VayuConnection;

        #region SQL Commands

        /// <summary>
        /// The mselect iso market command
        /// </summary>
        private SqlCommand mselectISOMarketCommand;
        /// <summary>
        /// The m selectl nodes coordinate command
        /// </summary>
        private SqlCommand mSelectlNodesCoordinateCommand;
        /// <summary>
        /// The m selectl deenergized nodes command
        /// </summary>
        private SqlCommand mSelectlDeenergizedNodesCommand;
        private SqlCommand mSelectlDeenergizedHoursCommand;
        private SqlCommand mSelectErcotDeenergizedNodesCmd;
        private SqlCommand sSelectFuelTypesCmd;
        private SqlCommand sSelectRTPrintsCmd;
        private SqlCommand mSelectlPathNodesCommand;
        private SqlCommand mSelectBollingerCommand;
        private SqlCommand mSelectloadDailyCommand;
        private SqlCommand mSelectloadHourlyCommand;
        SqlCommand mSelectSourceSinkCommand;
        SqlCommand SelectSumVolumesCommand;

        #endregion

        /// <summary>
        /// The node identifier LMPH hash
        /// </summary>
        private Dictionary<string, Vayu.NodePriceLibrary.LMP> nodeIdLMPHHash = new Dictionary<string, Vayu.NodePriceLibrary.LMP>();
        private Dictionary<string, Dictionary<DateTime, double>> mAvgHash = null;
        List<string> SouceSinkList = new List<string>();
        private Dictionary<string, int> dictNodeHash = new Dictionary<string, int>();
        Dictionary<string, double?> dictSumVolume = new Dictionary<string, double?>();
        #region Public Methods

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mselectISOMarketCommand = new SqlCommand();
            mselectISOMarketCommand.CommandText = "Select MarketKey, Label, TimeZone, MarketTimeObservesDST, HEIntervalMinute, MinutesInInterval from Market";
            mselectISOMarketCommand.Connection = VayuConnection;
            //
            mSelectlNodesCoordinateCommand = new SqlCommand();
            mSelectlNodesCoordinateCommand.Connection = VayuConnection;

            mSelectlDeenergizedNodesCommand = new SqlCommand();
            mSelectlDeenergizedNodesCommand.Connection = VayuConnection;

            mSelectlDeenergizedHoursCommand = new SqlCommand();
            mSelectlDeenergizedHoursCommand.Connection = VayuConnection;

            mSelectlPathNodesCommand = new SqlCommand();
            mSelectlPathNodesCommand.Connection = VayuConnection;

            mSelectBollingerCommand = VayuConnection.CreateCommand();
            mSelectBollingerCommand.CommandText = "select StdDev,LowerLimit, UpperLimit ,MarketDateTime , DARTTotal from  [PJM].[Bollinger] where MarketDateTime>=@SMarketDateTime and MarketDateTime<=@EMarketDateTime and SourceNodeKey=@SourceNodeKey and SinkNodeKey=@SinkNodeKey and Days=@Days";
            mSelectBollingerCommand.Parameters.AddWithValue("@SourceNodeKey", "SourceNodeKey");
            mSelectBollingerCommand.Parameters.AddWithValue("@SinkNodeKey", "SinkNodeKey");
            mSelectBollingerCommand.Parameters.AddWithValue("@SMarketDateTime", "MarketDateTime");
            mSelectBollingerCommand.Parameters.AddWithValue("@EMarketDateTime", "MarketDateTime");
            mSelectBollingerCommand.Parameters.AddWithValue("@Days", "Days");

            //
            mSelectSourceSinkCommand = VayuConnection.CreateCommand();
            mSelectSourceSinkCommand.CommandText = " select distinct SourceName from Vayu..EESPathList where SourceNodeKey is not null " +
                                                   " UNION " +
                                                   " select distinct SinkName from Vayu..EESPathList where SourceNodeKey is not null order by SourceName";

            SelectSumVolumesCommand = VayuConnection.CreateCommand();
            SelectSumVolumesCommand.CommandText = "SELECT  MarketDateTime,sum(SourceVolume) FROM Vayu..TradedVolume WHERE NodeKey =@NodeKey AND MarketDateTime >= @StartDate AND MarketDateTime <= @EndDate group by MarketDateTime, SourceVolume order by MarketDateTime asc ";
            SelectSumVolumesCommand.Parameters.AddWithValue("@NodeKey", "NodeKey");
            SelectSumVolumesCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            SelectSumVolumesCommand.Parameters.AddWithValue("@EndDate", "EndDate");

        }
        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="sourcePricingNode">The source pricing node.</param>
        /// <param name="sinkPricingNode">The sink pricing node.</param>
        public void GetChartData(Action<List<Node>, List<Node>, Exception> callback, DateTime startDate, DateTime endDate, PricingNode sourcePricingNode, PricingNode sinkPricingNode)
        {
            List<Node> rtNodeList = new List<Node>();
            List<Node> daNodeList = new List<Node>();
            Node daSourceNode = new Node();
            daSourceNode.Market = sourcePricingNode.MarketKey;
            daSourceNode.NodeId = sourcePricingNode.NodeKey;
            daSourceNode.PNodeId = sourcePricingNode.ExternalNodeId;
            daSourceNode.NodeName = sourcePricingNode.NodeName;
            List<LmpTimePrice> timePriceList = new List<LmpTimePrice>();
            TimeSpan ts = endDate.AddDays(1) - startDate;
            int daysSpan = (int)Math.Ceiling(ts.TotalDays);
            for (int d = 0; d < daysSpan; d++)
            {
                DateTime tempDate = startDate.Date.AddDays(d);
                for (int h = 1; h < 25; h++)
                {
                    LmpTimePrice tp1 = new LmpTimePrice();
                    tp1.MarketTime = tempDate.AddHours(h);
                    timePriceList.Add(tp1);
                }
            }
            daSourceNode.LmpTimePriceList = timePriceList;
            Node rtSourceNode = new Node();
            rtSourceNode.Market = sourcePricingNode.MarketKey;
            rtSourceNode.NodeId = sourcePricingNode.NodeKey;
            rtSourceNode.PNodeId = sourcePricingNode.ExternalNodeId;
            rtSourceNode.NodeName = sourcePricingNode.NodeName;
            timePriceList = new List<LmpTimePrice>();
            for (int d = 0; d < daysSpan; d++)
            {
                DateTime tempDate = startDate.Date.AddDays(d);
                for (int h = 1; h < 25; h++)
                {
                    LmpTimePrice tp1 = new LmpTimePrice();
                    tp1.MarketTime = tempDate.AddHours(h);
                    timePriceList.Add(tp1);
                }
            }
            rtSourceNode.LmpTimePriceList = timePriceList;
            rtNodeList.Add(rtSourceNode);
            daNodeList.Add(daSourceNode);
            if (sinkPricingNode != null)
            {
                Node daSinkNode = new Node();
                daSinkNode.Market = sinkPricingNode.MarketKey;
                daSinkNode.NodeId = sinkPricingNode.NodeKey;
                daSinkNode.PNodeId = sinkPricingNode.ExternalNodeId;
                daSinkNode.NodeName = sinkPricingNode.NodeName;
                timePriceList = new List<LmpTimePrice>();
                for (int d = 0; d < daysSpan; d++)
                {
                    DateTime tempDate = startDate.Date.AddDays(d);
                    for (int h = 1; h < 25; h++)
                    {
                        LmpTimePrice tp1 = new LmpTimePrice();
                        tp1.MarketTime = tempDate.AddHours(h);
                        timePriceList.Add(tp1);
                    }
                }
                daSinkNode.LmpTimePriceList = timePriceList;
                Node rtSinkNode = new Node();
                rtSinkNode.Market = sinkPricingNode.MarketKey;
                rtSinkNode.NodeId = sinkPricingNode.NodeKey;
                rtSinkNode.PNodeId = sinkPricingNode.ExternalNodeId;
                rtSinkNode.NodeName = sinkPricingNode.NodeName;
                timePriceList = new List<LmpTimePrice>();
                for (int d = 0; d < daysSpan; d++)
                {
                    DateTime tempDate = startDate.Date.AddDays(d);
                    for (int h = 1; h < 25; h++)
                    {
                        LmpTimePrice tp1 = new LmpTimePrice();
                        tp1.MarketTime = tempDate.AddHours(h);
                        timePriceList.Add(tp1);
                    }
                }
                rtSinkNode.LmpTimePriceList = timePriceList;
                rtNodeList.Add(rtSinkNode);
                daNodeList.Add(daSinkNode);
            }
            DARTNode.GetDART(rtNodeList, daNodeList, startDate, ts.Days, false, true);
            try
            {
                for (int j = 0; j < 2; j++)
                {
                    Node spreadNodeTimePrices = new Node();
                    spreadNodeTimePrices.NodeName = sourcePricingNode.NodeName + " -> " + sinkPricingNode.NodeName;
                    spreadNodeTimePrices.LmpTimePriceList = new List<LmpTimePrice>();
                    LmpTimePrice tpSource = new LmpTimePrice();
                    LmpTimePrice tpSink = new LmpTimePrice();
                    LmpTimePrice tpSpread = new LmpTimePrice();
                    List<Node> nodeLMPsList = j == 0 ? daNodeList : rtNodeList;
                    for (int i = 0; i < nodeLMPsList[0].LmpTimePriceList.Count; i++)
                    {
                        tpSpread = new LmpTimePrice();
                        tpSpread.Lmp = new Vayu.NodePriceLibrary.LMP();
                        tpSpread.MarketTime = nodeLMPsList[0].LmpTimePriceList[i].MarketTime;
                        if (nodeLMPsList[0].LmpTimePriceList[i] != null
                            && nodeLMPsList[0].LmpTimePriceList[i].MarketTime != null
                            && nodeLMPsList[1].LmpTimePriceList[i] != null
                            && nodeLMPsList[1].LmpTimePriceList[i].MarketTime != null)
                        {
                            tpSpread.Lmp.Price = nodeLMPsList[1].LmpTimePriceList[i].Lmp.Price - nodeLMPsList[0].LmpTimePriceList[i].Lmp.Price;
                            tpSpread.Lmp.Congestion = nodeLMPsList[1].LmpTimePriceList[i].Lmp.Congestion - nodeLMPsList[0].LmpTimePriceList[i].Lmp.Congestion;
                            tpSpread.Lmp.Loss = nodeLMPsList[1].LmpTimePriceList[i].Lmp.Loss - nodeLMPsList[0].LmpTimePriceList[i].Lmp.Loss;
                        }
                        else
                        {
                            tpSpread.Lmp.Price = double.NaN;
                            tpSpread.Lmp.Congestion = double.NaN;
                            tpSpread.Lmp.Loss = double.NaN;
                        }
                        spreadNodeTimePrices.LmpTimePriceList.Add(tpSpread);
                    }
                    foreach (Node node1LMPs in nodeLMPsList)
                    {
                        foreach (LmpTimePrice time in node1LMPs.LmpTimePriceList)
                        {
                            if (!double.IsNaN(time.Lmp.Price))
                            {
                                string marketDateTimeNodeIdKey = time.MarketTime.ToString() + node1LMPs.NodeId.ToString();
                                if (!nodeIdLMPHHash.ContainsKey(marketDateTimeNodeIdKey))
                                {
                                    nodeIdLMPHHash.Add(marketDateTimeNodeIdKey, time.Lmp);
                                }
                            }
                        }
                    }
                    if (j == 0)
                    {
                        daNodeList = nodeLMPsList.ToList<Node>();
                        daNodeList.Add(spreadNodeTimePrices);
                    }
                    else
                    {
                        rtNodeList = nodeLMPsList.ToList<Node>();
                        rtNodeList.Add(spreadNodeTimePrices);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            callback(daNodeList, rtNodeList, null);
        }

        /// <summary>
        /// Gets the iso market list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetISOMarketList(Action<List<string>, Exception> callback)
        {
            //List<string> marketList = new List<string>() { "PJM", "MISO", "CAISO", "SPP", "ERCOT", "NYISO" };
            List<string> marketList = new List<string>() { "ERCOT" };
            callback(marketList, null);
        }

        /// <summary>
        /// Gets the node coordinate.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="sourcesinkNode">The sourcesink node.</param>
        public void GetNodeCoordinate(Action<ObservableCollection<NodeCoordinate>, Exception> callback, List<int> sourcesinkNode)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectlNodesCoordinateCommand.CommandText = "SELECT nodekey, NodeName, Latitude, Longitude, 0, NodeName , Label  FROM dbo.Node    join NodeType on  Node.NodeTypeKey = NodeType.NodeTypeKey  WHERE NodeKey In (" + string.Join(",", sourcesinkNode.ToArray()) + ")";
            SqlDataReader reader = mSelectlNodesCoordinateCommand.ExecuteReader();
            ObservableCollection<NodeCoordinate> coordinateList = new ObservableCollection<NodeCoordinate>();
            while (reader.Read())
            {
                NodeCoordinate coordinate = new NodeCoordinate();
                coordinate.Nodekey = Convert.ToInt32(reader[0].ToString());

                coordinate.NodeName = reader.GetString(1);
                coordinate.MapLocation = new Location(reader.IsDBNull(2) ? 0 : double.Parse(reader[2].ToString()), reader.IsDBNull(3) ? 0 : double.Parse(reader[3].ToString()));
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
            VayuConnection.Close();
            callback(coordinateList, null);
        }

        public ObservableCollection<NodeCoordinate> GetNodeCoordinate(List<int> sourcesinkNode)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectlNodesCoordinateCommand.CommandText = "SELECT nodekey, NodeName, Latitude, Longitude, 0, NodeName , Label  FROM Vayu..Node    join NodeType on  Node.NodeTypeKey = NodeType.NodeTypeKey  WHERE NodeKey In (" + string.Join(",", sourcesinkNode.ToArray()) + ")";
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
            VayuConnection.Close();
            return coordinateList;
        }


        internal List<Constraint> GetErcotSensitivitiesData(string constraintName, string contingencyName, string sourcenode, string sinknode, DateTime date, double sensetivity, DateTime sDate, DateTime eDate, bool isRT)
        {
            List<Constraint> senSitivityList1 = new List<Constraint>();

            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Connection = con;
                        if (isRT)
                        {
                            cmd.CommandText = "WITH FilteredData AS ( SELECT MarketDateTime, ShadowPrice FROM HourlySPConstraintRT " +
" WHERE ConstraintText = '" + constraintName + "' and ContingencyText = '" + contingencyName + "' and ShadowPrice != 0 " +
" AND MarketDateTime BETWEEN '" + sDate + "' AND '" + eDate + "' ), " +
" RankedShadowPrices AS(SELECT MarketDateTime, ShadowPrice, ROW_NUMBER() OVER (ORDER BY ShadowPrice DESC) AS Rank " +
" FROM FilteredData) SELECT " +
 "   (SELECT MarketDateTime FROM RankedShadowPrices WHERE Rank = 1) AS HighestShadowPriceDate,               " +
 "   (SELECT top 1 MarketDateTime FROM FilteredData ORDER BY MarketDateTime DESC) AS LatestMarketDateTime,   " +
"	(SELECT MAX(ShadowPrice) FROM FilteredData) AS MaxShadowPrice,                                           " +
 "   (SELECT COUNT(*) FROM FilteredData) AS TotalRecords,                                                    " +
 "   (SELECT Datepart(HOUR FROM MarketDateTime) FROM RankedShadowPrices WHERE Rank = 1) AS HourOfTop1,       " +
 "   (SELECT Datepart(HOUR FROM MarketDateTime) FROM RankedShadowPrices WHERE Rank = 2) AS HourOfTop2,       " +
 "   (SELECT Datepart(HOUR FROM MarketDateTime) FROM RankedShadowPrices WHERE Rank = 3) AS HourOfTop3; ";

                        }
                        else
                        {
                            cmd.CommandText = "WITH FilteredData AS ( SELECT MarketDateTime, ShadowPrice FROM HourlySPConstraintDA " +
" WHERE ConstraintText = '" + constraintName + "' and ContingencyText = '" + contingencyName + "' and ShadowPrice != 0 " +
" AND MarketDateTime BETWEEN '" + sDate + "' AND '" + eDate + "' ), " +
" RankedShadowPrices AS(SELECT MarketDateTime, ShadowPrice, ROW_NUMBER() OVER (ORDER BY ShadowPrice DESC) AS Rank " +
" FROM FilteredData) SELECT " +
 "   (SELECT MarketDateTime FROM RankedShadowPrices WHERE Rank = 1) AS HighestShadowPriceDate,               " +
 "   (SELECT top 1 MarketDateTime FROM FilteredData ORDER BY MarketDateTime DESC) AS LatestMarketDateTime,   " +
"	(SELECT MAX(ShadowPrice) FROM FilteredData) AS MaxShadowPrice,                                           " +
 "   (SELECT COUNT(*) FROM FilteredData) AS TotalRecords,                                                    " +
 "   (SELECT Datepart(HOUR FROM MarketDateTime) FROM RankedShadowPrices WHERE Rank = 1) AS HourOfTop1,       " +
 "   (SELECT Datepart(HOUR FROM MarketDateTime) FROM RankedShadowPrices WHERE Rank = 2) AS HourOfTop2,       " +
 "   (SELECT Datepart(HOUR FROM MarketDateTime) FROM RankedShadowPrices WHERE Rank = 3) AS HourOfTop3; ";
                        }
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        SqlDataReader rdr1 = cmd.ExecuteReader();
                        while (rdr1.Read())
                        {
                            double sp = rdr1.IsDBNull(2) ? 0 : Convert.ToDouble(rdr1.GetValue(2));
                            string H1 = rdr1.IsDBNull(4) ? "" : rdr1.GetValue(4).ToString();
                            string H2 = rdr1.IsDBNull(5) ? "" : rdr1.GetValue(5).ToString();
                            string H3 = rdr1.IsDBNull(6) ? "" : rdr1.GetValue(6).ToString();
                            string HH = "";
                            if (H1 != "")
                            {
                                HH = H1 + ",";
                            }
                            if (H2 != "")
                            {
                                HH = HH + H2 + ",";
                            }
                            if (H3 != "")
                            {
                                HH = HH + H3;
                            }
                            Constraint helper = new Constraint();
                            helper.ConstraintDate = rdr1.GetDateTime(1);
                            helper.MaxConstraintDate = rdr1.GetDateTime(0);
                            helper.ConstraintText = constraintName;
                            helper.ContingencyText = contingencyName;
                            helper.Impact = Math.Round(-sensetivity * sp, 0);
                            helper.TopHours = HH;
                            helper.Count = rdr1.GetInt32(3);
                            helper.Sensitivity = Math.Round(-sensetivity, 4);
                            senSitivityList1.Add(helper);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return senSitivityList1;
        }

        /// <summary>
        /// Gets the deenergized nodes list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetDeenergizedNodesList(Action<ObservableCollection<string>, Exception> callback)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            //mSelectlDeenergizedNodesCommand.CommandText = "select name from dbo.DeenergizedNodes";
            mSelectlDeenergizedNodesCommand.CommandText = "select distinct(name) from Vayu..DeenergizedNodes order by name";
            SqlDataReader reader = mSelectlDeenergizedNodesCommand.ExecuteReader();
            ObservableCollection<string> DeenergizedNodesList = new ObservableCollection<string>();
            while (reader.Read())
            {
                DeenergizedNodesList.Add(reader.GetString(0));
            }
            reader.Close();
            VayuConnection.Close();
            callback(DeenergizedNodesList, null);
        }

        public string GetDeenergizedHourList(string Source, string Sink)
        {
            List<int> hourList = new List<int>();
            mSelectlDeenergizedHoursCommand = new SqlCommand();
            mSelectlDeenergizedHoursCommand.Connection = VayuConnection;

            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            //mSelectlDeenergizedNodesCommand.CommandText = "select name from dbo.DeenergizedNodes";
            mSelectlDeenergizedHoursCommand.CommandText = "select distinct hours,MarketDate from DeenergizedNodes " +
                                      " where ( MarketDate = @StartDate ) " +
                                      " and Name in (@Source, @Sink) ";
            mSelectlDeenergizedHoursCommand.Parameters.AddWithValue("@Source", Source);
            mSelectlDeenergizedHoursCommand.Parameters.AddWithValue("@Sink", Sink);

            mSelectlDeenergizedHoursCommand.Parameters.AddWithValue("@StartDate", DateTime.Today.AddDays(0));
            SqlDataReader reader = mSelectlDeenergizedHoursCommand.ExecuteReader();

            while (reader.Read())
            {
                int hour = reader.GetInt32(0);
                if (!hourList.Contains(hour))
                    hourList.Add(hour);
            }
            reader.Close();
            VayuConnection.Close();




            /*
                        using (SqlConnection connection = VayuConnection)
                        {
                            string query = "select distinct hours,MarketDate from DeenergizedNodes " +
                                                  " where ( MarketDate = @StartDate ) " +
                                                  " and Name in (@Source, @Sink) ";



                            mSelectlDeenergizedHoursCommand = new SqlCommand(query, connection);
                            mSelectlDeenergizedHoursCommand.Parameters.AddWithValue("@Source", Source);
                            mSelectlDeenergizedHoursCommand.Parameters.AddWithValue("@Sink", Sink);
                            mSelectlDeenergizedHoursCommand.Parameters.AddWithValue("@StartDate", DateTime.Today.AddDays(0));

                            if (connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (SqlDataReader reader = mSelectlDeenergizedHoursCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int hour = reader.GetInt32(0);
                                    if (!hourList.Contains(hour))
                                        hourList.Add(hour);

                                }
                            }

                        }

                        */

            //mSelectlDeenergizedNodesCommand.CommandText = "select name from dbo.DeenergizedNodes";

            string commaSeparatedString = string.Join(",", hourList);
            return commaSeparatedString;
        }

        public int GetRTPrints(DateTime sdate, DateTime edate, SourceSinkData sourceSink)
        {
            int RtPrints = 0;
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();//sSelectRTPrintsCmd
                sSelectRTPrintsCmd = new SqlCommand();
                sSelectRTPrintsCmd.CommandText = "select count(*) from Vayu..NodeLMP so inner join Vayu..NodeLMP si " +
                    "on so.MarketDateTime = si.MarketDateTime where so.NodeKey = " + sourceSink.Source.NodeKey + " and si.NodeKey = " + sourceSink.Sink.NodeKey + " " +
                    "and so.MarketDateTime > '" + sdate + "' and so.MarketDateTime <= '" + edate + "'";
                sSelectRTPrintsCmd.Connection = VayuConnection;
                SqlDataReader reader = sSelectRTPrintsCmd.ExecuteReader();
                while (reader.Read())
                {
                    RtPrints = int.Parse(reader[0].ToString());
                }

            }
            catch
            {

            }

            return RtPrints;

        }
        public List<DeenergizedNode> GetDeenergizedNodes()
        {
            List<DeenergizedNode> ListDeenergizedNodes = new List<DeenergizedNode>();
            DeenergizedNode NodeDeenergized = new DeenergizedNode();
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                mSelectErcotDeenergizedNodesCmd = new SqlCommand();
                mSelectErcotDeenergizedNodesCmd.CommandText = "Select Name,MarketDate,Hours from Vayu..DeenergizedNodes where MarketDate='" + DateTime.Now.Date + "'";
                mSelectErcotDeenergizedNodesCmd.Connection = VayuConnection;
                SqlDataReader reader = mSelectErcotDeenergizedNodesCmd.ExecuteReader();
                while (reader.Read())
                {
                    NodeDeenergized = new DeenergizedNode();
                    NodeDeenergized.Name = reader.GetString(0);
                    NodeDeenergized.MarketDate = reader.GetDateTime(1);
                    NodeDeenergized.Hours = reader.GetInt32(2);
                    ListDeenergizedNodes.Add(NodeDeenergized);
                }
            }
            catch
            {

            }
            VayuConnection.Close();
            return ListDeenergizedNodes.ToList();
        }

        public List<FuelType> GetSourceSinkFuelTypes()
        {
            List<FuelType> types = new List<FuelType>();
            FuelType objfuelType = new FuelType();
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                sSelectFuelTypesCmd = new SqlCommand();
                sSelectFuelTypesCmd.CommandText = "select b.NodeName, ISNULL(a.FuelSource, 'NULL') as Fuel from ErcotNodeFuelSource a join node b on a.NodeKey=b.NodeKey";
                sSelectFuelTypesCmd.Connection = VayuConnection;
                SqlDataReader reader = sSelectFuelTypesCmd.ExecuteReader();
                while (reader.Read())
                {
                    objfuelType = new FuelType();
                    objfuelType.Name = reader.GetString(0);
                    objfuelType.FuelTypes = reader.GetString(1);
                    types.Add(objfuelType);
                }


            }
            catch
            {

            }
            return types.ToList();
        }
        public void GetInValidPathNodesList(Action<List<SourceSink>, Exception> callback)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectlPathNodesCommand.CommandText = "select SourceNodeName, SinkNodeName from InvalidUptosPaths";
            SqlDataReader reader = mSelectlPathNodesCommand.ExecuteReader();
            List<SourceSink> DicPathNodes = new List<SourceSink>();
            while (reader.Read())
            {
                SourceSink mSourceSink = new SourceSink();
                mSourceSink.Source = reader.GetString(0);
                mSourceSink.Sink = reader.GetString(1);
                DicPathNodes.Add(mSourceSink);
            }
            reader.Close();
            VayuConnection.Close();
            callback(DicPathNodes, null);
        }

        /// <summary>
        /// Fills the pnode hash.
        /// </summary>
        /// <returns></returns>
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

        #endregion

        //internal Dictionary<DateTime, double?> GetMaxDailyLoad(DateTime startDate, DateTime endDate)
        //{
        //    try
        //    {
        //        Dictionary<DateTime, double?> LoadHash = new Dictionary<DateTime, double?>();
        //        loadDBCommands();
        //        double? maxLoad = 0.0;
        //        SqlCommand cmd = SigmaDbConn.CreateCommand();
        //        cmd.CommandText = "select cast(MARKETDATETIME AS DATE) , max(MW) from LoadRT where LoadsKey = 25 and MarketDateTime >=  @StartDate and MarketDateTime <= @EndDate GROUP BY cast(MARKETDATETIME AS DATE) ORDER BY cast(MARKETDATETIME AS DATE)";
        //        cmd.Connection = SigmaDbConn;
        //        cmd.Parameters.AddWithValue("@StartDate", startDate);
        //        cmd.Parameters.AddWithValue("@EndDate", endDate);
        //        if (SigmaDbConn.State == ConnectionState.Closed)
        //            SigmaDbConn.Open();
        //        SqlDataReader rdr = cmd.ExecuteReader();
        //        while (rdr.Read())
        //        {
        //            DateTime mdate = Convert.ToDateTime(rdr.GetValue(0));
        //            maxLoad = CommonAccessLibrary.CommonDataConversions.GetDouble(rdr.IsDBNull(1) ? 0.0 : rdr.GetValue(1));
        //            if (!LoadHash.ContainsKey(mdate))
        //                LoadHash.Add(mdate, maxLoad);
        //        }
        //        rdr.Close();
        //        SigmaDbConn.Close();
        //        return LoadHash;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        public Dictionary<string, List<Bollinger>> GetBollingerData(DateTime fromDate, DateTime toDate, int SourceNodeKey, int SinkNodeKey, int days)
        {
            Dictionary<string, List<Bollinger>> returnvalue = new Dictionary<string, List<Bollinger>>();
            List<Bollinger> itemList = new List<Bollinger>();
            loadDBCommands();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                con.Open();
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = mSelectBollingerCommand.CommandText;
                    cmd.Parameters.Add(new SqlParameter("@SourceNodeKey", SourceNodeKey));
                    cmd.Parameters.Add(new SqlParameter("@SinkNodeKey", SinkNodeKey));
                    cmd.Parameters.Add(new SqlParameter("@sMarketDateTime", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@EMarketDateTime", toDate));
                    cmd.Parameters.Add(new SqlParameter("@Days", days));
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        itemList.Add(new Bollinger
                        {
                            StdDev = reader.IsDBNull(2) ? double.NaN : Convert.ToDouble(reader.GetValue(0)),
                            LowerLimit = reader.IsDBNull(1) ? double.NaN : Convert.ToDouble(reader.GetValue(1)),
                            UpperLimit = reader.IsDBNull(0) ? double.NaN : Convert.ToDouble(reader.GetValue(2)),
                            MarketDateTime = Convert.ToDateTime(reader.GetValue(3)),
                            DARTTotal = reader.IsDBNull(4) ? double.NaN : Convert.ToDouble(reader.GetValue(4)),
                        });
                    }
                    if (!reader.IsClosed)
                        reader.Close();
                }
            }
            returnvalue.Add(days.ToString(), itemList);
            return returnvalue;

        }

        internal Dictionary<string, double> GetLoadsData(List<DateTime> datetimeList, string Market)
        {
            try
            {
                Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    //List<DateTime> temiList = datetimeList.GroupBy(x => x.ToString("dd-MM-yyyy"));
                    foreach (DateTime date in datetimeList)
                    {
                        double load = 0;
                        if (!loadDictHash.ContainsKey(date.AddDays(-1).Date.ToString("dd-MM-yyyy")))
                        { //For Ercot
                            {
                                mSelectloadDailyCommand = new SqlCommand();
                                if (VayuConnection.State == ConnectionState.Closed)
                                {
                                    VayuConnection.Open();
                                }
                                mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from Vayu..LoadRT where loadskey=2213 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate"
                                                                      + " group by CAST(MarketDateTime as date) order by date";
                                mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date.AddDays(-1));
                                mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date);
                                mSelectloadDailyCommand.Connection = VayuConnection;
                                SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                }
                                reader.Close();
                                if (load == 0)
                                {
                                    mSelectloadHourlyCommand = new SqlCommand();
                                    mSelectloadHourlyCommand.CommandText = "select  max(MW) from Vayu..LoadRT where LoadsKey = 2213 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";//LoadRTH 
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@StartDate", date.Date.AddDays(-1));
                                    mSelectloadHourlyCommand.Parameters.AddWithValue("@EndDate", date.Date);
                                    mSelectloadHourlyCommand.Connection = VayuConnection;
                                    SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                    while (reader1.Read())
                                    {
                                        load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                    }
                                    reader1.Close();
                                }
                            }
                            loadDictHash.Add(date.AddDays(-1).Date.ToString("dd-MM-yyyy"), load);
                        }

                    }
                }
                VayuConnection.Close();
                return loadDictHash;

            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        internal Dictionary<string, double> PGetLoadsData(DateTime startdate, DateTime endate)
        {
            try
            {
                Dictionary<string, double> ploadDictHash = new Dictionary<string, double>();
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    mSelectloadDailyCommand = new SqlCommand();
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from LoadRT where loadskey=25 and   MarketDateTime >= @Startdate and MarketDateTime <=@endDate"
                                                          + " group by CAST(MarketDateTime as date) order by date";
                    mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", startdate.Date);
                    mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", endate.Date);
                    mSelectloadDailyCommand.Connection = VayuConnection;
                    SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime date = Convert.ToDateTime(reader.GetValue(0));
                        double load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                        ploadDictHash.Add(date.ToString("dd-MM-yyyy"), load);
                    }
                    reader.Close();
                }
                VayuConnection.Close();
                return ploadDictHash;

            }
            catch (Exception)
            {
                return null;
            }
        }

        internal Dictionary<string, double> EGetLoadsData(DateTime startdate, DateTime endate)
        {
            try
            {
                Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    mSelectloadDailyCommand = new SqlCommand();
                    if (VayuConnection.State == ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from Vayu..LoadRT where loadskey=2213 and   MarketDateTime >= @Startdate and MarketDateTime <= @endDate"
                                                          + " group by CAST(MarketDateTime as date) order by date";
                    mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", startdate.Date);
                    mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", endate.Date);
                    mSelectloadDailyCommand.Connection = VayuConnection;
                    SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        DateTime date = Convert.ToDateTime(reader.GetValue(0));
                        double load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                        loadDictHash.Add(date.ToString("dd-MM-yyyy"), load);
                    }
                    reader.Close();
                }
                VayuConnection.Close();
                return loadDictHash;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GetSouceSink()
        {
            loadDBCommands();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlDataReader reader = mSelectSourceSinkCommand.ExecuteReader();
            while (reader.Read())
            {
                //SouceSinkList = new List<string>();
                string SourceSink = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                SouceSinkList.Add(SourceSink);
            }
            return SouceSinkList;
        }
        private void FillNodeHash()
        {
            dictNodeHash = new Dictionary<string, int>();
            loadDBCommands();

            SqlCommand selectNodeCommand = new SqlCommand();
            selectNodeCommand.CommandText = "select distinct nodekey, nodename from Vayu..node where MarketKey = 9";

            selectNodeCommand.Connection = VayuConnection;
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();

            SqlDataReader reader = selectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                string nodename = reader.GetString(1).ToString();
                if (!dictNodeHash.ContainsKey(nodename))
                    dictNodeHash.Add(nodename, (int)reader.GetInt32(0));
            }
            reader.Close();
            VayuConnection.Close();
        }

        public int GetNodeKey(string Nodename)
        {
            int Nodekey = 0;
            loadDBCommands();

            SqlCommand selectNodeCommand = new SqlCommand();
            selectNodeCommand.CommandText = "select nodekey, nodename from Vayu..node where MarketKey = 9 and nodename='" + Nodename + "'";

            selectNodeCommand.Connection = VayuConnection;
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();

            SqlDataReader reader = selectNodeCommand.ExecuteReader();
            while (reader.Read())
            {

                Nodekey = (int)reader.GetInt32(0);

            }
            reader.Close();
            VayuConnection.Close();

            return Nodekey;
        }

        public List<TradedVolumesData> GetTradedVolumesData(string NodeName, DateTime StartDate, DateTime EndDate)
        {
            FillNodeHash();

            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            TradedVolumesData objTradedVolumesData = new TradedVolumesData();
            List<TradedVolumesData> lstTradedVolumesData = new List<TradedVolumesData>();

            SqlCommand SelectTradedVolumesCommand = VayuConnection.CreateCommand();
            SelectTradedVolumesCommand.CommandText = "SELECT NodeKey, MarketDateTime, SourceVolume, SinkVolume FROM Vayu..TradedVolume WHERE NodeKey = @NodeKey AND MarketDateTime > @StartDate AND MarketDateTime <= @EndDate order by MarketDateTime asc";
            SelectTradedVolumesCommand.Parameters.AddWithValue("@NodeKey", "NodeKey");
            SelectTradedVolumesCommand.Parameters.AddWithValue("@StartDate", "StartDate");
            SelectTradedVolumesCommand.Parameters.AddWithValue("@EndDate", "EndDate");

            int NodeKey = 0;

            if (dictNodeHash.ContainsKey(NodeName))
            {
                NodeKey = dictNodeHash[NodeName];
            }

            if (NodeKey != 0)
            {
                SelectTradedVolumesCommand.Parameters["@NodeKey"].Value = NodeKey;
                SelectTradedVolumesCommand.Parameters["@StartDate"].Value = StartDate.Date;
                SelectTradedVolumesCommand.Parameters["@EndDate"].Value = EndDate.Date.AddDays(1);

                SqlDataReader rdr = SelectTradedVolumesCommand.ExecuteReader();
                while (rdr.Read())
                {
                    DateTime date = new DateTime();
                    objTradedVolumesData = new TradedVolumesData();
                    objTradedVolumesData.NodeKey = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                    DateTime dt = rdr.IsDBNull(1) ? new DateTime() : Convert.ToDateTime(rdr.GetValue(1));
                    if (dt.Hour == 0)
                    {
                        date = dt.AddDays(-2).AddHours(0);
                        objTradedVolumesData.MarketDateTime = date.AddHours(24);
                        objTradedVolumesData.Hour = date.Hour.ToString();
                    }
                    else
                    {
                        objTradedVolumesData.MarketDateTime = dt;
                        objTradedVolumesData.Hour = dt.Hour.ToString();
                    }
                    objTradedVolumesData.SourceVolume = rdr.IsDBNull(2) ? 0 : Convert.ToDouble(rdr.GetValue(2));
                    objTradedVolumesData.SinkVolume = rdr.IsDBNull(3) ? 0 : Convert.ToDouble(rdr.GetValue(3));
                    lstTradedVolumesData.Add(objTradedVolumesData);
                }
                rdr.Close();
            }
            VayuConnection.Close();

            return lstTradedVolumesData;
        }

        internal Dictionary<string, double?> GetSourceSum(int NodeKey, DateTime startdate, DateTime endate)
        {
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SelectSumVolumesCommand.Parameters["@NodeKey"].Value = NodeKey;
            SelectSumVolumesCommand.Parameters["@StartDate"].Value = startdate.Date;
            SelectSumVolumesCommand.Parameters["@EndDate"].Value = endate.Date.AddDays(1);
            SqlDataReader rdr = SelectSumVolumesCommand.ExecuteReader();
            while (rdr.Read())
            {
                string date = Convert.ToString(rdr.GetValue(0));
                double Sum = Convert.ToDouble(rdr.GetValue(1));
                dictSumVolume.Add(date, Sum);
            }
            return dictSumVolume;
        }

        public Dictionary<string, Dictionary<int, double>> Get15MinLMP(string NodeName, DateTime StartDate, DateTime EndDate, string Spreadtype)
        {
            Dictionary<string, Dictionary<int, double>> AllHourLMPDic = new Dictionary<string, Dictionary<int, double>>();

            List<string> abc = new List<string>();

            int NodeKey = GetNodeKey(NodeName);
            List<LMP15MinPriceHelper> LMP15MinPriceList = new List<LMP15MinPriceHelper>();
            Dictionary<string, List<LMP15MinPriceHelper>> LMP15PriceDic = new Dictionary<string, List<LMP15MinPriceHelper>>();
            List<string> DatehourList;
            List<string> DateList = new List<string>();
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand SelectLMP15MinValues = new SqlCommand();

            if (Spreadtype == "RT")
                SelectLMP15MinValues = new SqlCommand("select so.LMP, so.MarketDateTime  from Vayu..NodeLMPH so  where so.NodeKey = @NodeKey and so.MarketDateTime >= @StartDate and so.MarketDateTime <= @EndDate ");


            // if (Spreadtype == "DA" || Spreadtype =="DART" )
            if (Spreadtype == "DA")
                SelectLMP15MinValues = new SqlCommand("select so.LMP, so.MarketDateTime  from Vayu..NodeDALMPH so  where so.NodeKey = @NodeKey and so.MarketDateTime >= @StartDate and so.MarketDateTime <= @EndDate ");

            if (Spreadtype == "DART")
                SelectLMP15MinValues = new SqlCommand("select a.LMP-b.LMP, a.MarketDateTime from Vayu..NodeLMPH a join Vayu..NodeDALMPH b on a.MarketDateTime=b.MarketDateTime   where a.MarketDateTime >= @StartDate and a.MarketDateTime <= @EndDate and a.NodeKey = @NodeKey and  b.NodeKey = @NodeKey");

            SelectLMP15MinValues.Parameters.AddWithValue("@NodeKey", "NodeKey");
            SelectLMP15MinValues.Parameters.AddWithValue("@StartDate", "StartDate");
            SelectLMP15MinValues.Parameters.AddWithValue("@EndDate", "EndDate");

            SelectLMP15MinValues.Parameters["@NodeKey"].Value = NodeKey;
            SelectLMP15MinValues.Parameters["@StartDate"].Value = StartDate.Date;
            SelectLMP15MinValues.Parameters["@EndDate"].Value = EndDate.Date.AddDays(1);

            SelectLMP15MinValues.Connection = VayuConnection;
            SqlDataReader rdr = SelectLMP15MinValues.ExecuteReader();
            while (rdr.Read())
            {
                DateTime dt = rdr.GetDateTime(1);
                int hour = dt.Hour;
                int min = dt.Minute;
                string key = dt.Date.ToShortDateString() + "?" + dt.Hour;
                double LMPValue = (double)Math.Round(rdr.GetDecimal(0), 2);

                LMP15MinPriceHelper helper = new LMP15MinPriceHelper();


                helper.MarketDateTime = dt.Date;
                helper.Minute = min;
                helper.Hour = hour;
                helper.Price = LMPValue;

                LMP15MinPriceList = new List<LMP15MinPriceHelper>();

                LMP15MinPriceList.Add(helper);


                if (!LMP15PriceDic.ContainsKey(key))
                {
                    LMP15PriceDic.Add(key, LMP15MinPriceList);
                }
                else
                    LMP15PriceDic[key].Add(helper);

            }


            DatehourList = LMP15PriceDic.Keys.ToList();

            foreach (string val in DatehourList)
            {
                string[] valarrya = val.Split('?');
                int hr = Convert.ToInt32(valarrya[1]);
                string datevar = valarrya[0];

                if (!DateList.Contains(datevar))
                    DateList.Add(datevar);

            }

            foreach (var Datevalue in DateList)
            {
                List<int> hourList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };



                foreach (var hour in hourList)
                {
                    double TotalLMP = 0;
                    double FinalEnergyPrice = 0;
                    string keyval = Datevalue + "?" + hour;


                    if (LMP15PriceDic.ContainsKey(keyval))
                    {
                        var CurrhourMinCollection = LMP15PriceDic[keyval];
                        CurrhourMinCollection = CurrhourMinCollection.OrderBy(x => x.Minute).ToList();

                        for (int i = 0; i < CurrhourMinCollection.Count; i++)

                            TotalLMP = CurrhourMinCollection.ElementAt(i).Price;

                    }

                    Dictionary<int, double> dic1 = new Dictionary<int, double>();
                    dic1.Add(hour, TotalLMP);

                    if (!AllHourLMPDic.ContainsKey(Datevalue))
                        AllHourLMPDic.Add(Datevalue, dic1);
                    else
                        AllHourLMPDic[Datevalue].Add(hour, TotalLMP);

                }
            }




            return AllHourLMPDic;

        }

        public void GetConstraintData(Action<System.Collections.Generic.List<Constraint>, Exception> callback, string Source, string Sink, bool isRT, DateTime fromDate, DateTime throDate)
        {
            List<Constraint> tempList = FillAvgHash(Source, Sink, isRT, fromDate, throDate);

            callback(tempList.OrderByDescending(a => a.CnstRTNum).ToList(), null);

        }

        private List<Constraint> FillAvgHash(string Source, string Sink, bool isRT, DateTime fromDate, DateTime throDate)
        {

            List<Constraint> tempList = new List<Constraint>();
            List<Constraint> tempList1 = new List<Constraint>();

            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                try
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (isRT)
                        {
                            cmd.CommandText = "with cte1 as(select a.ConstraintRTNum as constrt, b.NodeKey,a.MonitoredText,a.ContingencyText,b.Sensitivity as sensetivity from RTMasterConstraint a join RTMasterVector_New b on a.ConstraintRTNum = b.ConstraintRTNum " +
                "join node n on  b.NodeKey = n.NodeKey where n.NodeName = '" + Source + "' and a.MarketDateTime >= @start and " +
                 " a.marketdatetime < @end), " +
                   " cte2 as (select a.ConstraintRTNum as constrt1, b.NodeKey,a.MonitoredText,a.ContingencyText,b.Sensitivity as sensetivity from RTMasterConstraint a join RTMasterVector_New b on a.ConstraintRTNum = b.ConstraintRTNum " +
                   " join node n on  b.NodeKey = n.NodeKey where n.NodeName = '" + Sink + "' and a.MarketDateTime >= @start and " +
                  " a.marketdatetime < @end) select cte1.constrt,cte1.MonitoredText,cte1.ContingencyText,cte1.sensetivity-cte2.sensetivity from cte1 inner join cte2 on cte1.constrt = cte2.constrt1";
                        }
                        else
                        {
                            cmd.CommandText = "with cte1 as(select a.ConstraintRTNum as constrt, b.NodeKey,a.MonitoredText,a.ContingencyText,b.Sensitivity as sensetivity from DAMasterConstraint a join DAMasterVector b on a.ConstraintRTNum = b.ConstraintRTNum " +
                "join node n on  b.NodeKey = n.NodeKey where n.NodeName = '" + Source + "' and a.MarketDateTime >= @start and " +
                 " a.marketdatetime < @end), " +
                   " cte2 as (select a.ConstraintRTNum as constrt1, b.NodeKey,a.MonitoredText,a.ContingencyText,b.Sensitivity as sensetivity from DAMasterConstraint a join DAMasterVector b on a.ConstraintRTNum = b.ConstraintRTNum " +
                   " join node n on  b.NodeKey = n.NodeKey where n.NodeName = '" + Sink + "' and a.MarketDateTime >= @start and " +
                  " a.marketdatetime < @end) select cte1.constrt,cte1.MonitoredText,cte1.ContingencyText,cte1.sensetivity-cte2.sensetivity from cte1 inner join cte2 on cte1.constrt = cte2.constrt1";
                        }

                        cmd.Parameters.AddWithValue("@start", fromDate);
                        cmd.Parameters.AddWithValue("@end", throDate);
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        IDataReader reader = cmd.ExecuteReader();
                        mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                        while (reader.Read())
                        {
                            try
                            {
                                int constNum = (int)reader.GetDecimal(0);
                                string constraint = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString();
                                string contingency = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString();
                                double sensetivity = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3));
                                string keyText = constraint + "?split?" + contingency;
                                Constraint constraintItem = tempList.Where(a => a.CnstRTNum == constNum && a.ConstraintText == constraint && a.ContingencyText == contingency).FirstOrDefault();
                                if (constraintItem == null)
                                {
                                    constraintItem = new Constraint { CnstRTNum = constNum, ContingencyText = contingency, ConstraintText = constraint, Sensitivity = sensetivity };
                                }
                                if (tempList.Contains(constraintItem))
                                    tempList.Remove(constraintItem);
                                tempList.Add(constraintItem);
                            }
                            catch (Exception e)
                            {
                            }
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                }
                catch
                {
                }
            }

            tempList1 = tempList.Where(c => c.Sensitivity != 0).ToList();


            return tempList1;
        }

    }
    /// <summary>
    /// 
    /// </summary>
    public class ISOMarket
    {
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>
        /// The time zone.
        /// </value>
        public string TimeZone { get; set; }
        /// <summary>
        /// Gets or sets the market time observes DST.
        /// </summary>
        /// <value>
        /// The market time observes DST.
        /// </value>
        public int MarketTimeObservesDST { get; set; }
        /// <summary>
        /// Gets or sets the he interval minute.
        /// </summary>
        /// <value>
        /// The he interval minute.
        /// </value>
        public int HEIntervalMinute { get; set; }
        /// <summary>
        /// Gets or sets the minutes interval.
        /// </summary>
        /// <value>
        /// The minutes interval.
        /// </value>
        public int MinutesInterval { get; set; }
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

        private string toolTipText;

        public string ToolTipText
        {
            get { return toolTipText; }
            set
            {
                toolTipText = value;
            }
        }

        public Brush MyColor { get; set; }

        private string typeName;
        public string NodeTypeName { get; set; }
        public string TypeName
        {
            get { return typeName; }
            set
            {
                typeName = value;

                switch (typeName)
                {
                    case "GENERATOR":
                        UpdateStarPatternPoints();
                        break;

                    case "LOAD":
                        UpdateTrianglePatternPoints();
                        break;

                    case "EHV":
                        UpdateDiamodPoints();
                        break;


                    case "EXT":
                    case "BUS":

                    case "INTERFACE":

                    default:
                        break;
                }
            }
        }

        public PointCollection MyPointCollection { get; set; }

        private void UpdateDiamodPoints()
        {
            MyPointCollection = new PointCollection();
            double unitlength = 5.5;
            MyPointCollection.Add(new Point(0, unitlength));
            MyPointCollection.Add(new Point(unitlength, 0));
            MyPointCollection.Add(new Point(0, -unitlength));
            MyPointCollection.Add(new Point(-unitlength, 0));
        }

        private void UpdateTrianglePatternPoints()
        {
            MyPointCollection = new PointCollection();
            double radius = 6;
            for (int i = 120; i <= 360; i = i + 120)
                MyPointCollection.Add(new Point(radius * Math.Sin(i * Math.PI / 180), radius * Math.Cos(i * Math.PI / 180)));
        }

        private void UpdateStarPatternPoints()
        {
            MyPointCollection = new PointCollection();
            double outter_radius = 6;
            double inner_radius = 3;

            for (int i = 36; i <= 324; i = i + 72)
            {
                MyPointCollection.Add(new Point(outter_radius * Math.Sin(i * Math.PI / 180),
                                                outter_radius * Math.Cos(i * Math.PI / 180)));
                MyPointCollection.Add(new Point(inner_radius * Math.Sin((i + 36) * Math.PI / 180),
                                                inner_radius * Math.Cos((i + 36) * Math.PI / 180)));
            }
        }

        public NodeCoordinate()
        {
            MyColor = Brushes.GreenYellow;
            TypeName = "Default";
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GraphCoOrdinates
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the x value.
        /// </summary>
        /// <value>
        /// The x value.
        /// </value>
        public double XValue { get; set; }
        /// <summary>
        /// Gets or sets the y value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        public double? YValue { get; set; }
    }
    public class SourceSink
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the source value.
        /// </summary>
        /// <value>
        /// The x value.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the sink value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
    }
    public class LMP15MinPriceHelper
    {
        public DateTime MarketDateTime { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        public double Price { get; set; }

    }

}