
using System.Collections.Generic;
using System.Windows;
using Vayu.DBLibrary;

namespace Vayu.UTCRiskCopy.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = new MainWindowViewModel();
        }
        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }
        private void AccountListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            List<Portfolio> portfolioList = new List<Portfolio>();
            foreach (Account item in accountListBox.SelectedItems)
            {
                foreach (Portfolio portfolio in item.PortfolioList)
                {
                    portfolioList.Add(portfolio);
                }
            }
            Vayu.UTCRiskCopy.ViewModels.MainWindowViewModel utcmainwin = DataContext as Vayu.UTCRiskCopy.ViewModels.MainWindowViewModel;
            utcmainwin.SetPortfolioList(portfolioList);
        }

        private void PortfolioListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            List<Portfolio> portfolioList = new List<Portfolio>();
            foreach (Portfolio item in portfolioListBox.SelectedItems)
            {
                portfolioList.Add(item);
            }
            ViewModels.MainWindowViewModel utcmainwin = DataContext as ViewModels.MainWindowViewModel;
            utcmainwin.SelectedPortfolioList = portfolioList;
        }

        private void portfolioSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            portfolioListBox.SelectAll();
        }

        private void portfolioUnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            portfolioListBox.UnselectAll();

        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            portfolioListBox.UnselectAll();
            marketListBox.SelectedIndex = -1;
            productlist.SelectedIndex = -1;
        }

        private void startDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (portfolioListBox != null)
                portfolioListBox.UnselectAll();
            marketListBox.SelectedIndex = -1;
            productlist.SelectedIndex = -1;
        }
    }
}
