using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Vayu.CRRAnnAnalysis.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Vayu.CRRAnnAnalysis.ViewModels.MainWindowViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as Vayu.CRRAnnAnalysis.ViewModels.MainWindowViewModel;
            viewModel.AddSourceSink();
           
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {

        }
        private void RemoveALl_Click(object sender, RoutedEventArgs e)
        {

        }
        private void sourceComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }
        private void sinkComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }
        private void sourceSinkDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count > 0)
            //{
            //    SourceSinkData sourceSink = (SourceSinkData)e.AddedItems[0];
            //    if (sourceSink.Sink == null)
            //    {
            //        //sourceCheckBox.IsEnabled = false;
            //        //sinkCheckBox.IsEnabled = false;
            //    }
            //    else
            //    {
            //        //sourceCheckBox.IsEnabled = true;
            //        //sinkCheckBox.IsEnabled = true;
            //    }
            //}
            //else
            //    return;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
