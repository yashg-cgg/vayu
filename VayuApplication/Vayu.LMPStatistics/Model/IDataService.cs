using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vayu.DBLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.LMPStatistics.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        void loadDBCommands();
        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="callbackDateTime">The callback date time.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="sinkNode">The sink node.</param>
        void GetChartData(Action<List<Node>, List<Node>, Exception> callbackDateTime, DateTime startDate, DateTime endDate, PricingNode sourceNode, PricingNode sinkNode);
        /// <summary>
        /// Gets the iso market list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetISOMarketList(Action<List<string>, Exception> callback);
        /// <summary>
        /// Gets the deenergized nodes list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetDeenergizedNodesList(Action<ObservableCollection<string>, Exception> callback);

        string GetDeenergizedHourList(string Source, string Sink);
        int GetRTPrints(DateTime sdate, DateTime edate, SourceSinkData sourceSink);
        List<DeenergizedNode> GetDeenergizedNodes();

        List<FuelType> GetSourceSinkFuelTypes();
        /// <summary>
        /// Gets the node coordinate.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="sourcesinkNode">The sourcesink node.</param>
        void GetNodeCoordinate(Action<ObservableCollection<NodeCoordinate>, Exception> callback, List<int> sourcesinkNode);
        void GetInValidPathNodesList(Action<List<SourceSink>, Exception> callback);
        List<string> GetSouceSink();

        List<TradedVolumesData> GetTradedVolumesData(string NodeName, DateTime StartDate, DateTime EndDate);

        int GetNodeKey(string Nodename);

        Dictionary<string, Dictionary<int, double>> Get15MinLMP(string NodeName, DateTime StartDate, DateTime EndDate, string Spreadtype);

        void GetConstraintData(Action<System.Collections.Generic.List<Constraint>, Exception> callback, string Source, string Sink, bool isRT, DateTime fromDate, DateTime throDate);
    }
}
