
using System.Windows;
using Vayu.ErcotShiftFactor.Model;
using Vayu.ErcotShiftFactor.ViewModels;

namespace Vayu.ErcotShiftFactor.Views
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

        private void SelectionChanged_Con(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DataService ds = new DataService();
            MainWindowViewModel dx = DataContext as MainWindowViewModel;

            if (((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem != null)
            {
                dx.SelectedConstraintItem = ((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString();
                dx.ContingencySearchList = ds.GetAllContingency(((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString());
                //if (dx.RTChecked)
                //    dx.ContingencySearchList = ds.GetAllContingencies(((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString(), true, 9);
                //else if (dx.DAChecked)
                //    dx.ContingencySearchList = ds.GetAllContingencies(((System.Windows.Controls.AutoCompleteBox)(sender)).SelectedItem.ToString(), false, 9);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
