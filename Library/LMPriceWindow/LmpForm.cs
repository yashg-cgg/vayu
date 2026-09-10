using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Vayu.DBLibrary;

namespace Vayu.LMPriceWindow
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public class LmpForm : Window
    {
        #region Declaration

        /// <summary>
        /// The s LMP graph window
        /// </summary>
        private static Vayu.NodePriceGraph.Views.MainWindow sLmpGraphWindow = new NodePriceGraph.Views.MainWindow();
        /// <summary>
        /// The s LMP graph view model
        /// </summary>
        private static Vayu.NodePriceGraph.ViewModels.MainWindowViewModel sLmpGraphViewModel = new Vayu.NodePriceGraph.ViewModels.MainWindowViewModel(new Vayu.NodePriceGraph.Model.DataService());
        //// <summary>
        // The s LMP statistics view model
        // </summary>
        private static Vayu.LMPStatistics.ViewModels.MainWindowViewModel sLMPStatisticsViewModel = new Vayu.LMPStatistics.ViewModels.MainWindowViewModel(new Vayu.LMPStatistics.Model.DataService());
        /// <summary>
        /// The s LMP statistics window
        /// </summary>
        private static Vayu.LMPStatistics.Views.MainWindow sLMPStatisticsWindow = new Vayu.LMPStatistics.Views.MainWindow();
        /// <summary>
        /// The s source sink list
        /// </summary>
        private static List<SourceSinkData> sSourceSinkList;

        #endregion

        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        private static string GetMarket(int marketKey)
        {
            string market = "ERCOT ";
            if (marketKey == 9)
            {
                market = "ERCOT";
            }

            return market;
        }

        #region Public Methods

        /// <summary>
        /// Opens the LMP statistic analyzer.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="sourceSinkList">The source sink list.</param>
        public static void OpenLMPStatisticAnalyzer(int marketKey, List<SourceSinkData> sourceSinkList)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (!sLMPStatisticsWindow.IsVisible)
            {
                sLMPStatisticsViewModel = new Vayu.LMPStatistics.ViewModels.MainWindowViewModel(new Vayu.LMPStatistics.Model.DataService());
                sLMPStatisticsWindow = new Vayu.LMPStatistics.Views.MainWindow();
            }
            sLMPStatisticsWindow.DataContext = sLMPStatisticsViewModel;
            sLMPStatisticsViewModel.MarketComboSelectedValue = GetMarket(marketKey);
            sLMPStatisticsViewModel.UptosChecked = sourceSinkList[0].Sink != null;
            sLMPStatisticsViewModel.SetSourceSinkList(sourceSinkList);
            sLMPStatisticsWindow.Show();
            sLMPStatisticsWindow.Activate();
            Mouse.OverrideCursor = null;
        }
        /// <summary>
        /// Opens the LMP graphs.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="sourceSinkList">The source sink list.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public static void OpenLmpGraphs(int marketKey, List<SourceSinkData> sourceSinkList, DateTime start, DateTime end)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (!sLmpGraphWindow.IsVisible)
            {
                sLmpGraphWindow = new Vayu.NodePriceGraph.Views.MainWindow();
                sLmpGraphViewModel = new Vayu.NodePriceGraph.ViewModels.MainWindowViewModel(new Vayu.NodePriceGraph.Model.DataService());
            }
            sLmpGraphWindow.DataContext = sLmpGraphViewModel;
            sLmpGraphViewModel.AddDates(start, end);
            sLmpGraphViewModel.SetMarket(GetMarket(marketKey));
            sLmpGraphViewModel.AddNodes(sourceSinkList);
            sLmpGraphViewModel.RefreshChart();
            sLmpGraphWindow.Show();
            sLmpGraphWindow.Activate();
            Mouse.OverrideCursor = null;
        }
        /// <summary>
        /// Sets the source sinks.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        /// <param name="marketKey">The market key.</param>
        public static void SetSourceSinks(List<Tuple<string, string>> sourceSinkList, int marketKey)
        {
            sSourceSinkList = new List<SourceSinkData>();
            foreach (Tuple<string, string> tuple in sourceSinkList)
            {
                SourceSinkData sourceSink = new SourceSinkData();
                PricingNode priceNode = DBAccess.GetNodeFromName(tuple.Item1, marketKey);
                if (priceNode != null)
                {
                    sourceSink.Source = new PricingNode();
                    sourceSink.Source.NodeName = tuple.Item1;
                    sourceSink.Source.ExternalNodeId = priceNode.ExternalNodeId;
                    sourceSink.Source.MarketKey = marketKey;
                    sourceSink.Source.NodeKey = priceNode.NodeKey;
                    sourceSink.Source.NodeTypeKey = priceNode.NodeTypeKey;
                    sourceSink.Source.Zone = priceNode.Zone;
                }
                if (tuple.Item2 != null)
                {
                    priceNode = DBAccess.GetNodeFromName(tuple.Item2, marketKey);
                    if (priceNode != null)
                    {
                        sourceSink.Sink = new PricingNode();
                        sourceSink.Sink.NodeName = tuple.Item2;
                        sourceSink.Sink.ExternalNodeId = priceNode.ExternalNodeId;
                        sourceSink.Sink.MarketKey = marketKey;
                        sourceSink.Sink.NodeKey = priceNode.NodeKey;
                        sourceSink.Sink.NodeTypeKey = priceNode.NodeTypeKey;
                        sourceSink.Sink.Zone = priceNode.Zone;
                    }
                }
                sSourceSinkList.Add(sourceSink);
            }
        }
        /// <summary>
        /// Gets the source sinks.
        /// </summary>
        /// <returns></returns>
        public static List<SourceSinkData> GetSourceSinks()
        {
            return sSourceSinkList;
        }

        #endregion
    }
}
