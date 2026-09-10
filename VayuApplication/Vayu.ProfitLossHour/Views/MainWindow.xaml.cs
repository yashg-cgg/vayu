
using System.ComponentModel;
using System.Windows;
using Vayu.ProfitLossHour.ViewModels;

namespace Vayu.ProfitLossHour.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        private ListSortDirection orderDirection;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }

        private void pnlGrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            e.Handled = true;
            var DataContext = this.DataContext as MainWindowViewModel;
            if (DataContext != null)
            {
                DataContext.mSortOrder = !DataContext.mSortOrder;
                if (DataContext.mSortOrder)
                {
                    orderDirection = ListSortDirection.Ascending;
                }
                else
                {
                    orderDirection = ListSortDirection.Descending;
                }
                DataContext.SortCondition = e.Column.SortMemberPath;
                DataContext.HandleSort(DataContext.HourPNLList, orderDirection);
            }
        }
    }
}
