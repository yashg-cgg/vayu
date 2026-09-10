using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vayu.DBLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.NodePriceGraph.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Loads the database commands.
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
        /// Gets the node coordinate.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="sourcesinkNode">The sourcesink node.</param>
        void GetNodeCoordinate(Action<ObservableCollection<NodeCoordinate>, Exception> callback, List<int> sourcesinkNode);

    }
}
