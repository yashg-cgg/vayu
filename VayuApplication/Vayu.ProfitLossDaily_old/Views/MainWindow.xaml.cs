using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Vayu.DBLibrary;

namespace Vayu.ProfitLossDaily.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }

        private void portfolioSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            portfolioListBox.SelectAll();
        }

        private void portfolioUnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            portfolioListBox.UnselectAll();
        }

        private void AccountListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Portfolio> portfolioList = new List<Portfolio>();
            foreach (Account item in accountListBox.SelectedItems)
            {
                foreach (Portfolio portfolio in item.PortfolioList)
                {
                    portfolioList.Add(portfolio);
                }
            }
            //portfolioList.Sort();
            ProfitLossDaily.ViewModels.MainWindowViewModel virtualPnlModel = DataContext as ProfitLossDaily.ViewModels.MainWindowViewModel;
            virtualPnlModel.SetPortfolioList(portfolioList);
        }

        private void PortfolioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Portfolio> portfolioList = new List<Portfolio>();
            foreach (Portfolio item in portfolioListBox.SelectedItems)
            {
                portfolioList.Add(item);
            }
            ProfitLossDaily.ViewModels.MainWindowViewModel virtualPnlModel = DataContext as ProfitLossDaily.ViewModels.MainWindowViewModel;
            virtualPnlModel.SelectedPortfolioList = portfolioList;
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            portfolioListBox.UnselectAll();

        }

        private void BtnAggregate_Click(object sender, RoutedEventArgs e)
        {
            // tabAggregateItem.Visibility = Visibility.Visible;
            AggregateGrid.Visibility = Visibility.Visible;
            //tabAggregateItem.MouseDoubleClick += tabIndividualItem_MouseDoubleClick;
            SummaryGrid.Visibility = Visibility.Collapsed;

        }

        private void BtnIndividual_Click(object sender, RoutedEventArgs e)
        {
            //tabIndividualItem.Visibility = Visibility.Visible;
            // AggregateGrid.Visibility = Visibility.Visible;
        }

        private void calculateButton1_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ForeColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
            double number;
            if (value != null)
            {
                value = value.ToString().Replace("$", "");
                Double.TryParse(value.ToString(), out number);
                //Double.TryParse((string)value, out number);
                if (number >= 0)
                {
                    mybrush = new SolidColorBrush(Colors.Black);
                }
                if (number < 0)
                {
                    mybrush = new SolidColorBrush(Colors.Red);
                }
            }
            return mybrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TextColumnConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridRow row = value as DataGridRow;
            Vayu.ProfitLossDaily.Model.PNLConstraints constraint = row.Item as Vayu.ProfitLossDaily.Model.PNLConstraints;
            double? doub = constraint.GetType().GetProperty(parameter as string).GetValue(constraint) as double?;
            return (doub.HasValue ? string.Format("{0:#,##0.00;-#,##0.00;''}", doub) : "");
            // return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
