
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Vayu.OutageConstraintMapping.ViewModels;

namespace Vayu.OutageConstraintMapping.Views
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

        /// <summary>
        /// Handles the SelectionChanged event of the Outage_AutoCompleteBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Outage_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                e.Handled = true;
                AutoCompleteBox auto = (AutoCompleteBox)sender;
                if (auto.SelectedItem != null)
                {
                    string selectedItem = auto.SelectedItem.ToString();
                    List<OutageData> pList = System.Linq.Enumerable.Cast<OutageData>(OutageDataGrid.Items).ToList();
                    OutageDataGrid.SelectedItem = pList.Find(p => p.DriverName == selectedItem);
                    OutageDataGrid.ScrollIntoView(OutageDataGrid.SelectedItem);
                    // OutageDataGrid.SelectedItem = (DataContext as MainViewModel).OutageList.Find(a => a.DriverName == selectedItem);
                    // OutageDataGrid.ScrollIntoView(selectedItem);
                }
            }
            catch (System.Exception)
            {
            }
        }
        /// <summary>
        /// Handles the SelectionChanged event of the OutageDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void OutageDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutageDataGrid.SelectedItems != null)
            {
                try
                {
                    int shadowPrice = int.Parse(ShadowPriceTextBox.Text);
                    List<string> selectedOutageList = new List<string>();
                    Vayu.OutageConstraintMapping.ViewModels.MainWindowViewModel model = DataContext as MainWindowViewModel;
                    foreach (var item in OutageDataGrid.SelectedItems)
                    {
                        OutageData outage = (OutageData)item;
                        selectedOutageList.Add(outage.DriverName);
                    }
                    Dictionary<string, List<string>> dictCons = model.mDataService.GetConstraintData(selectedOutageList, shadowPrice);
                    model.showConstraintFamily(dictCons);
                }
                catch (System.Exception)
                {
                    //throw;
                }
            }
        }
        /// <summary>
        /// Handles the 1 event of the SelectAllButton_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectAllButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (OutageDataGrid.Items != null)
            {
                try
                {
                    int shadowPrice = int.Parse(ShadowPriceTextBox.Text);
                    List<string> selectedOutageList = new List<string>();
                    OutageDataGrid.SelectAll();
                    Vayu.OutageConstraintMapping.ViewModels.MainWindowViewModel model = DataContext as MainWindowViewModel;
                    foreach (var item in OutageDataGrid.SelectedItems)
                    {
                        OutageData outage = (OutageData)item;
                        // List<Constraint> tempConstraintList = (model.mDataService.GetConstraintData(outage.DriverName));
                        //constraintList.AddRange(tempConstraintList);
                        selectedOutageList.Add(outage.DriverName);
                    }
                    Dictionary<string, List<string>> consdict = model.mDataService.GetConstraintData(selectedOutageList, shadowPrice);
                    model.showConstraintFamily(consdict);
                }
                catch (System.Exception ex)
                {
                    //throw;
                }
            }
        }

        /// <summary>
        /// Handles the 2 event of the UnSelectallButton_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UnSelectallButton_Click_2(object sender, RoutedEventArgs e)
        {
            if (OutageDataGrid.Items != null)
            {
                try
                {
                    OutageDataGrid.UnselectAll();
                    Vayu.OutageConstraintMapping.ViewModels.MainWindowViewModel model = DataContext as MainWindowViewModel;
                    model.ConstraintList = null;
                }
                catch (System.Exception)
                {
                    //throw;
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ConstraintDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ConstraintDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConstraintDataGrid.SelectedItem != null)
            {
                Vayu.OutageConstraintMapping.ViewModels.MainWindowViewModel model = DataContext as MainWindowViewModel;
                foreach (Constraint constraint in ConstraintDataGrid.SelectedItems)
                {
                    List<Contingency> contigencyList = model.mDataService.GetContigencyData(constraint.ConstraintName);
                    model.ContingencyList = contigencyList;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the ConstraintSesitivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ConstraintSesitivity_Click(object sender, RoutedEventArgs e)
        {
            if (ConstraintDataGrid.SelectedCells != null && ConstraintDataGrid.SelectedCells.Count >= 1)
            {
                string column = ConstraintDataGrid.SelectedCells[0].Column.Header.ToString();
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    foreach (Constraint constraint in ConstraintDataGrid.SelectedItems)
                    {
                        datacontext.ShowConstraintSesitivityWindow(constraint.ConstraintName.Replace("COMPANY", "").Replace("Interface", "").Replace("Monitor", "").Replace("Actual", "").Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Constraint_AutoCompleteBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Constraint_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                e.Handled = true;
                AutoCompleteBox auto = (AutoCompleteBox)sender;
                if (auto.SelectedItem != null)
                {
                    string selectedItem = auto.SelectedItem.ToString();
                    List<Constraint> pList = System.Linq.Enumerable.Cast<Constraint>(ConstraintDataGrid.Items).ToList();
                    ConstraintDataGrid.SelectedItem = pList.Find(p => p.ConstraintName == selectedItem);
                    ConstraintDataGrid.ScrollIntoView(ConstraintDataGrid.SelectedItem);
                    // OutageDataGrid.SelectedItem = (DataContext as MainViewModel).OutageList.Find(a => a.DriverName == selectedItem);
                    // OutageDataGrid.ScrollIntoView(selectedItem);
                }
            }
            catch (System.Exception)
            {
            }
        }
        /// <summary>
        /// Handles the SelectionChanged event of the FamilyDataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void FamilyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FamilyDataGrid.SelectedItems != null)
            {
                List<string> tempFamilyitems = new List<string>();
                Vayu.OutageConstraintMapping.ViewModels.MainWindowViewModel model = DataContext as MainWindowViewModel;
                foreach (Family consfamily in FamilyDataGrid.SelectedItems)
                {
                    tempFamilyitems.Add(consfamily.ConstraintFamily);
                }
                model.FilterConstraintdata(tempFamilyitems);
            }
        }

        /// <summary>
        /// Handles the Click event of the SelectFamilyButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FamilyDataGrid.Items != null)
                {
                    FamilyDataGrid.SelectAll();
                    List<string> tempFamilyList = new List<string>();
                    Vayu.OutageConstraintMapping.ViewModels.MainWindowViewModel model = DataContext as MainWindowViewModel;
                    foreach (var item in FamilyDataGrid.SelectedItems)
                    {
                        Family family = (Family)item;
                        tempFamilyList.Add(family.ConstraintFamily);
                    }
                    model.FilterConstraintdata(tempFamilyList);
                }

            }
            catch (System.Exception)
            {
                //throw;
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Family_AutoCompleteBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Family_AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                e.Handled = true;
                AutoCompleteBox auto = (AutoCompleteBox)sender;
                if (auto.SelectedItem != null)
                {
                    string selectedItem = auto.SelectedItem.ToString();
                    List<Family> familyList = System.Linq.Enumerable.Cast<Family>(FamilyDataGrid.Items).ToList();
                    FamilyDataGrid.SelectedItem = familyList.Find(a => a.ConstraintFamily == selectedItem);
                    FamilyDataGrid.ScrollIntoView(FamilyDataGrid.SelectedItem);
                }
            }
            catch (System.Exception)
            {
                // throw;
            }
        }

    }
}
