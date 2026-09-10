using System.Collections.Generic;
using System.Windows;

namespace Vayu.CRRPNLDetails.Views
{
    /// <summary>
    /// Interaction logic for FTRDetailsFormFilter.xaml
    /// </summary>
    public partial class FTRDetailsFormFilter : Window
    {
        public FTRDetailsFormFilter()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Handles the SelectionChanged event of the ListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as Vayu.CRRPNLDetails.ViewModels.FTRFilterViewModel;
            List<string> selectedItems = new List<string>();
            foreach (var item in CombinedParameterList.SelectedItems)
            {
                selectedItems.Add((string)item);
            }
            if (dataContext != null)
                dataContext.SelectedParticipant(selectedItems);
        }

        /// <summary>
        /// Handles the 1 event of the ListBox_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ListBox_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var dataContext = this.DataContext as Vayu.CRRPNLDetails.ViewModels.FTRFilterViewModel;
            List<string> selectedItems = new List<string>();
            foreach (var item in SelectedParameterList.SelectedItems)
            {
                selectedItems.Add((string)item);
            }
            if (dataContext != null)
                dataContext.DeleteDates(selectedItems);

        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
