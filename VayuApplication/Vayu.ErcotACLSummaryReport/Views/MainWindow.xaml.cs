
using System.Windows;
using System.Windows.Controls;
using Vayu.ErcotACLSummaryReport.ViewModel;

namespace Vayu.ErcotACLSummaryReport.Views
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
        private void All_checked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
            {
                accDataDataGrid.Columns[0].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[1].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[2].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[3].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[4].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[5].Visibility = Visibility.Visible;
            }
        }

        private void Al_unchecked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
            {
                accDataDataGrid.Columns[0].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[1].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[2].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[3].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[4].Visibility = Visibility.Visible;
                accDataDataGrid.Columns[5].Visibility = Visibility.Visible;
            }
        }

        private void LetterOfCredit_checked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[6].Visibility = Visibility.Visible;
        }

        private void LetterOfCredit_unchecked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[6].Visibility = Visibility.Collapsed;
        }

        private void SuretyBond_checked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[7].Visibility = Visibility.Visible;
        }

        private void SuretyBond_unchecked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[7].Visibility = Visibility.Collapsed;
        }


        private void ApprovedCRRBilateralTrades_checked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[8].Visibility = Visibility.Visible;
        }

        private void ApprovedCRRBilateralTrades_unchecked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[8].Visibility = Visibility.Collapsed;
        }

        private void CRRLockedACL_checked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[9].Visibility = Visibility.Visible;
        }

        private void CRRLockedACL_unchecked(object sender, RoutedEventArgs e)
        {
            if (accDataDataGrid != null)
                accDataDataGrid.Columns[9].Visibility = Visibility.Collapsed;
        }

        private void ApprovedBilateralTrades_checked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[7].Visibility = Visibility.Visible;
        }

        private void ApprovedBilateralTrades_unchecked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[7].Visibility = Visibility.Collapsed;
        }

        private void ACLLockedForCRR_checked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[8].Visibility = Visibility.Visible;
        }

        private void ACLLockedForCRR_unchecked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[8].Visibility = Visibility.Collapsed;
        }

        private void TPESInExcessOfSecuredCollateral_checked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[9].Visibility = Visibility.Visible;
        }
        private void TPESInExcessOfSecuredCollateral_unchecked(object sender, RoutedEventArgs e)
        {
            crrDataDataGrid.Columns[9].Visibility = Visibility.Collapsed;
        }

        private void OutstandingSecuredCollateralRequest_checked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[10].Visibility = Visibility.Visible;
        }

        private void OutstandingSecuredCollateralRequest_unchecked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[10].Visibility = Visibility.Collapsed;
        }

        private void AdditionalSecuredCollateralRequired_checked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[11].Visibility = Visibility.Visible;
        }

        private void AdditionalSecuredCollateralRequired_unchecked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
                crrDataDataGrid.Columns[11].Visibility = Visibility.Collapsed;
        }

        private void CheckedAllcrr_checked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
            {
                crrDataDataGrid.Columns[0].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[1].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[2].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[3].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[4].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[5].Visibility = Visibility.Visible;
            }
        }

        private void CheckedAllcrr_unchecked(object sender, RoutedEventArgs e)
        {
            if (crrDataDataGrid != null)
            {
                crrDataDataGrid.Columns[0].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[1].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[2].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[3].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[4].Visibility = Visibility.Visible;
                crrDataDataGrid.Columns[5].Visibility = Visibility.Visible;
            }
        }

        private void CheckedAlldam_checked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
            {
                damDataDataGrid.Columns[0].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[1].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[2].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[3].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[4].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[5].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[6].Visibility = Visibility.Visible;
            }
        }

        private void CheckedAlldam_unchecked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
            {
                damDataDataGrid.Columns[0].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[1].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[2].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[3].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[4].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[5].Visibility = Visibility.Visible;
                damDataDataGrid.Columns[6].Visibility = Visibility.Visible;
            }
        }

        private void TPEAInExcessOfRemainderCollateralAndUnsecuredCredit_checked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
                damDataDataGrid.Columns[7].Visibility = Visibility.Visible;
        }

        private void TPEAInExcessOfRemainderCollateralAndUnsecuredCredit_unchecked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
                damDataDataGrid.Columns[7].Visibility = Visibility.Collapsed;
        }
        private void OutstandingAnyCollateralRequest_checked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
                damDataDataGrid.Columns[8].Visibility = Visibility.Visible;
        }

        private void OutstandingAnyCollateralRequest_unchecked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
                damDataDataGrid.Columns[8].Visibility = Visibility.Collapsed;
        }
        private void AdditionalAnyCollateralRd_checked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
                damDataDataGrid.Columns[9].Visibility = Visibility.Visible;
        }

        private void AdditionalAnyCollateralRd_unchecked(object sender, RoutedEventArgs e)
        {
            if (damDataDataGrid != null)
                damDataDataGrid.Columns[9].Visibility = Visibility.Collapsed;
        }

        private void Showdesc_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            string name = menu.Name;
            var selectedItem = (ClTranACLData)cltranDataDataGrid.SelectedItem;
            string desc = selectedItem.Description.ToString();
            if (!desc.Equals(""))
            {
                //Vayu.BidsEvaluation.Views.MainWindow vm = new Vayu.BidsEvaluation.Views.MainWindow();
                //var dataContext = new Vayu.BidsEvaluation.ViewModels.MainWindowViewModel(new Vayu.BidsEvaluation.Model.DataService());
                //vm.DataContext = dataContext;
                //vm.Show();
                Vayu.Invoice.ViewModels.MainWindowViewModel mainViewModel = new Vayu.Invoice.ViewModels.MainWindowViewModel(new Vayu.Invoice.Models.LoadModel());
                var window = new Vayu.Invoice.Views.MainWindow();
                window.DataContext = mainViewModel;
                if (cltranDataDataGrid.SelectedItems.Count > 0)
                {
                    mainViewModel.SetData(desc);
                    window.Show();
                }
                //Vayu.Invoice.Views.MainWindow mainViewModel = new Vayu.Invoice.Views.MainWindow();
                //var datacontext = new Vayu.Invoice.ViewModels.MainWindowViewModel(null);
                //mainViewModel.da
                //if (cltranDataDataGrid.SelectedItems.Count > 0)
                //{
                //    mainViewModel.SetData(desc);
                //    mainViewModel.Show();
                //}
            }
            else
            {
                MessageBox.Show("No Invoice number or description provided.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
