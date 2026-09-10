using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vayu.DBLibrary;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public partial class MainWindow
    {
        #region Declaration

        /// <summary>
        /// The portfolio list
        /// </summary>
        private ObservableCollection<Portfolio> portfolioList;
        /// <summary>
        /// Gets or sets the trader portfolio combo list.
        /// </summary>
        /// <value>
        /// The trader portfolio combo list.
        /// </value>
        public ObservableCollection<Portfolio> TraderPortfolioComboList
        {
            get { return portfolioList; }
            set { portfolioList = value; RaisePropertyChanged("TraderPortfolioComboList"); }
        }

        /// <summary>
        /// The selected portfolio
        /// </summary>
        private Portfolio selectedPortfolio;
        /// <summary>
        /// Gets or sets the trader portfolio combo selected item.
        /// </summary>
        /// <value>
        /// The trader portfolio combo selected item.
        /// </value>
        public Portfolio TraderPortfolioComboSelectedItem
        {
            get { return selectedPortfolio; }
            set { selectedPortfolio = value; RaisePropertyChanged("TraderPortfolioComboSelectedItem"); }
        }

        /// <summary>
        /// The portfolio bid list
        /// </summary>
        private ObservableCollection<PortfolioBid> portfolioBidList;
        /// <summary>
        /// Gets or sets the portfolio bid list.
        /// </summary>
        /// <value>
        /// The portfolio bid list.
        /// </value>
        public ObservableCollection<PortfolioBid> PortfolioBidList
        {
            get { return portfolioBidList; }
            set { portfolioBidList = value; RaisePropertyChanged("PortfolioBidList"); }
        }

        #endregion

        /// <summary>
        /// Sets the portfolio.
        /// </summary>
        private void SetPortfolio()
        {
            //if (!mMarkets.Values.Contains(mMarketInContext))
            //    return;

            List<Portfolio> portfolioList = new System.Collections.Generic.List<Portfolio>();
            if (!StartDatePicker.SelectedDate.HasValue)
                return;

            bool isUpto = UptosCheckBox.IsChecked.GetValueOrDefault();
            string product = isUpto ? "EES/PTP" : "Virtual";
            if (MarketlistBox.SelectedIndex == 0 || MarketlistBox.SelectedIndex == -1)
                portfolioList = DBAccess.GetPortfolio(StartDatePicker.SelectedDate.Value, mUser, product, "PJM");
            if (MarketlistBox.SelectedIndex == 2)
                portfolioList = DBAccess.GetPortfolio(StartDatePicker.SelectedDate.Value, mUser, product, "CAISO");

            foreach (Portfolio portfolio in portfolioList)
                portfolio.IsUptos = isUpto;

            TraderPortfolioComboList = new System.Collections.ObjectModel.ObservableCollection<Portfolio>(portfolioList);
        }

        /// <summary>
        /// Resets the portfolio view.
        /// </summary>
        private void ResetPortfolioView()
        {
            PortfolioBidList = new ObservableCollection<PortfolioBid>();
            MapControl.myMap.Children.Remove(mPortfolioMapLayer);
            mPortfolioMapLayer.Children.Clear();
        }

        #region Events

        /// <summary>
        /// Handles the ValueChanged event of the StartDatePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void StartDatePicker_ValueChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetPortfolioView();
            SetPortfolio();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the changes event of the Portfolio_Auto control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Portfolio_Auto_changes(object sender, SelectionChangedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            if (TraderPortfolioComboSelectedItem == null)
                return;

            if (UptosCheckBox.IsChecked.Equals(true))
            {
                var columns = dgPortfolio.Columns;
                foreach (DataGridColumn item in columns)
                {
                    if (item.Header.ToString().Equals("Source") || item.Header.ToString().Equals("Sink") || item.Header.ToString().Equals("MW") || item.Header.ToString().Equals("Price"))
                    {
                        item.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        item.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
            else
            {
                var columsn = dgPortfolio.Columns;
                foreach (DataGridColumn item in columsn)
                {
                    if (item.Header.ToString().Equals("Source") || item.Header.ToString().Equals("MW"))
                    {
                        item.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        item.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }

            List<PortfolioBid> bids = new List<PortfolioBid>();
            string savedName = TraderPortfolioComboSelectedItem.IsUptos ? null : TraderPortfolioComboSelectedItem.Name;
            List<Bid> bidList = DBAccess.GetBids("PJM", TraderPortfolioComboSelectedItem.ID, savedName, (DateTime)StartDatePicker.SelectedDate, ((DateTime)StartDatePicker.SelectedDate).AddDays(1),
                                    TraderPortfolioComboSelectedItem.IsUptos, "MOVED");
            ResetPortfolioView();
            PointCollection myPointCollection = new PointCollection();
            double unitlength = 10.5;
            myPointCollection.Add(new Point(0, unitlength));
            myPointCollection.Add(new Point(unitlength, 0));
            myPointCollection.Add(new Point(0, -unitlength));
            myPointCollection.Add(new Point(-unitlength, 0));

            foreach (Bid bid in bidList)
            {
                int hour = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                PricingNode sourceNode = DBAccess.GetNode(bid.Source, bid.Market);
                PricingNode sinkNode = DBAccess.GetNode(bid.Sink, bid.Market);
                LocationCollection coll = new LocationCollection();

                if (!IsValidHour(hour) || sourceNode == null)
                {
                    continue;
                }

                Microsoft.Maps.MapControl.WPF.Location locationSource = FindCoordinate(sourceNode.NodeName);
                if (locationSource == null)
                    continue;
                Polygon soucePolygon = new Polygon();
                soucePolygon.Fill = new SolidColorBrush(Colors.DarkGreen);
                soucePolygon.Fill.Opacity = 0.8;
                soucePolygon.Points = myPointCollection;

                ToolTip sourcett = new ToolTip();
                sourcett.Content = DBAccess.GetNode(bid.Source, bid.Market).NodeName;
                sourcett.FontWeight = FontWeights.Bold;
                soucePolygon.ToolTip = sourcett;

                MapLayer.SetPosition(soucePolygon, locationSource);
                ToolTipService.SetShowDuration(soucePolygon, 300000);
                mPortfolioMapLayer.Children.Add(soucePolygon);
                coll.Add(locationSource);

                PortfolioBid portfolioBid = new PortfolioBid();
                portfolioBid.Source = (string)sourcett.Content;

                if (UptosCheckBox.IsChecked.Equals(true) && sinkNode != null)
                {
                    Microsoft.Maps.MapControl.WPF.Location locationSink = FindCoordinate(sinkNode.NodeName);
                    if (locationSink == null)
                        continue;
                    Polygon sinkPolygon = new Polygon();
                    sinkPolygon.Fill = new SolidColorBrush(Colors.DarkRed);
                    sinkPolygon.Fill.Opacity = 0.5;
                    sinkPolygon.Points = myPointCollection;

                    ToolTip sinktt = new ToolTip();
                    sinktt.Content = DBAccess.GetNode(bid.Sink, bid.Market).NodeName;
                    sinktt.FontWeight = FontWeights.Bold;
                    sinkPolygon.ToolTip = sinktt;

                    MapLayer.SetPosition(sinkPolygon, locationSink);
                    ToolTipService.SetShowDuration(sinkPolygon, 300000);
                    mPortfolioMapLayer.Children.Add(sinkPolygon);
                    coll.Add(locationSink);

                    MapPolyline line = new MapPolyline();
                    line.Stroke = new SolidColorBrush(Colors.Black);
                    line.StrokeThickness = 1.0;
                    line.Opacity = 0.6;
                    line.Locations = coll;
                    ToolTip tt = new ToolTip();
                    tt.FontWeight = FontWeights.Bold;
                    line.ToolTip = tt;
                    ToolTipService.SetShowDuration(line, 300000);
                    mPortfolioMapLayer.Children.Add(line);
                    portfolioBid.Sink = (string)sinktt.Content;
                }

                portfolioBid.MW = bid.MW;
                portfolioBid.Price = bid.Price;
                bids.Add(portfolioBid);
            }

            PortfolioBidList = new ObservableCollection<PortfolioBid>(bids);
            MapControl.myMap.Children.Add(mPortfolioMapLayer);
        }

        /// <summary>
        /// Handles the Click event of the UptosCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UptosCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ResetPortfolioView();
            SetPortfolio();
        }

        #endregion
    }
}
