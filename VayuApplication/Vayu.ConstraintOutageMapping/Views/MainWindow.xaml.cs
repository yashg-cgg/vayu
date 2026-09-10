
using System.Windows;
using Vayu.ConstraintOutageMapping.Model;
using Vayu.ConstraintOutageMapping.ViewModels;
namespace Vayu.ConstraintOutageMapping.Views
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
        public void SetPositions(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }
        /// <summary>
        /// Handles the SelectionChanged event of the ConstraintSearchBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ConstraintSearchBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DataService mDataService = new DataService();
            MainWindowViewModel dx = DataContext as MainWindowViewModel;
            if (dx.SelectedConstraintItem != null)
            {
                dx.ContingencySearchList = mDataService.GetContingencyList(dx.StartDate, dx.EndDate, dx.SelectedConstraintItem);
            }
        }

        public void GetData()
        {

        }
    }
}
