using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vayu.CRRCalculationLibrary;
using Vayu.DBLibrary;

namespace Vayu.CRRPathAnalyzer.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        void LoadDBCommands();
        /// <summary>
        /// Fills Source Sink List..
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="period">The period.</param>
        Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> FillData(SourceSinkState state, DateTime startDate, DateTime endDate, string period);
        Dictionary<int, DailyCrrValues> FillDataCrr(SourceSinkState state, DateTime startDate, DateTime endDate, string period, string Type);
        /// <summary>
        /// Gets the consolidated list.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="isPeakOffPeak">The is peak off peak.</param>
        /// <param name="periodType">Type of the period.</param>
        /// <returns></returns>
        List<ConsolidatedData> GetConsolidatedList(SourceSinkState state, string classType, string periodType);
        /// <summary>
        /// Gets the node coordinate.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="sourcesinkNode">The sourcesink node.</param>
        void GetNodeCoordinate(Action<ObservableCollection<NodeCoordinate>, Exception> callback, List<int> sourcesinkNode);
        /// <summary>
        /// Gets the caiso nodes.
        /// </summary>
        /// <returns></returns>
        List<PricingNode> GetCAISONodes();

        List<PricingNode> GetPJMSourceNodes();
        List<PricingNode> GetPJMSinkNodes();

        /// <summary>
        /// Gets the periods.
        /// </summary>
        /// <param name="datelist">The datelist.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        List<int> GetPeriods(List<DateTime> datelist, int marketKey);
    }
}
