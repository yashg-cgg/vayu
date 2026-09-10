using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.NodePriceLibrary;

namespace Vayu.NodePriceGraph.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Vayu database connection
        /// </summary>
        public SqlConnection VayuConnection;
        /// <summary>
        /// The mselect iso market command
        /// </summary>
        private SqlCommand mselectISOMarketCommand;
        /// <summary>
        /// The m selectl nodes coordinate command
        /// </summary>
        private SqlCommand mSelectlNodesCoordinateCommand;
        /// <summary>
        /// The node identifier LMPH hash
        /// </summary>
        private Dictionary<string, Vayu.NodePriceLibrary.LMP> nodeIdLMPHHash = new Dictionary<string, Vayu.NodePriceLibrary.LMP>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mselectISOMarketCommand = new SqlCommand();
            mselectISOMarketCommand.CommandText = "Select MarketKey, Label, TimeZone, MarketTimeObservesDST, HEIntervalMinute, MinutesInInterval from Market (nolock)";
            mselectISOMarketCommand.Connection = VayuConnection;
            //
            mSelectlNodesCoordinateCommand = new SqlCommand();
            mSelectlNodesCoordinateCommand.Connection = VayuConnection;
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
                        }
                        else
                        {
                            tpSpread.Lmp.Price = double.NaN;
                            tpSpread.Lmp.Congestion = double.NaN;
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
        /// Gets the  market list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetISOMarketList(Action<List<string>, Exception> callback)
        {
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
            mSelectlNodesCoordinateCommand.CommandText = "SELECT nodekey, NodeName, null, Longitude, 0, NodeName FROM dbo.Node (nolock) WHERE NodeKey In (" + string.Join(",", sourcesinkNode.ToArray()) + ")";
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

        #endregion
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
}
