
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Vayu.CityTemperatureServiceLibrary;
using Vayu.CommonControls;

namespace Vayu.TemperatureHistoryDetails.Views
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
        private void cityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> onlyCitylist = new List<string>();
            foreach (string cityname in cityListBox.SelectedItems)
            {
                onlyCitylist.Add(cityname);
            }
            TemperatureHistoryDetails.ViewModels.MainWindowViewModel virtualModel = DataContext as TemperatureHistoryDetails.ViewModels.MainWindowViewModel;
            virtualModel.SelectedCityList = onlyCitylist;
        }
        private void CityAllSelect_Click(object sender, RoutedEventArgs e)
        {
            cityListBox.SelectAll();
        }
        private void CityUnSelect_Click(object sender, RoutedEventArgs e)
        {
            cityListBox.UnselectAll();

        }
        private void ExportToCSV(object sender, RoutedEventArgs e)
        {
            ExportToExcelNodeSpread<Temperature, List<Temperature>> p = new ExportToExcelNodeSpread<Temperature, List<Temperature>>();
            ICollectionView view = CollectionViewSource.GetDefaultView(TemperatureListMinMax1.ItemsSource);
            if (TemperatureListMinMax1.Items.Count > 0)
            {
                view = CollectionViewSource.GetDefaultView(TemperatureListMinMax1.ItemsSource);
            }
            List<Temperature> itemList = new List<Temperature>();
            foreach (var item in view.SourceCollection)
            {
                itemList.Add((Temperature)item);
            }
            p.dataToPrint = itemList;
            p.GenerateReport();
        }
    }
}
