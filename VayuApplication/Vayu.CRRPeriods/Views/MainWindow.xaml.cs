
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Vayu.CRRPeriods.ViewModels;
namespace Vayu.CRRPeriods.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ListSortDirection orderDirection;
        public MainWindow()
        {
            InitializeComponent();
        }
        public void SetPosition(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortMemberPath.Equals("StartDate") || e.Column.SortMemberPath.Equals("EndDate"))
            {
                e.Handled = true;
                MainWindowViewModel dataContext = this.DataContext as MainWindowViewModel;
                if (dataContext != null)
                {
                    dataContext.mSortOrder = !dataContext.mSortOrder;
                    if (dataContext.mSortOrder)
                    {
                        orderDirection = ListSortDirection.Ascending;
                    }
                    else
                    {
                        orderDirection = ListSortDirection.Descending;
                    }
                    dataContext.SortCondition = e.Column.SortMemberPath;
                    dataContext.HandleSortedData(dataContext.PeriodList, orderDirection);
                }

            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindowViewModel dataContext = DataContext as MainWindowViewModel;
            if (dataContext.MarketSelectedItem == "PJM")
            {
                periodlist.Columns[8].Visibility = Visibility.Collapsed;
            }
            if (dataContext.MarketSelectedItem == "PJM")
            {
                periodlist.Columns[8].Visibility = Visibility.Visible;
            }
        }
    }
}
