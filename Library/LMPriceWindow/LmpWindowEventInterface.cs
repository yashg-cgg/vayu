using System.Windows;
using System.Windows.Controls;

namespace Vayu.LMPriceWindow
{
    /// <summary>
    /// 
    /// </summary>
    public interface LmpWindowEventInterface
    {
        /// <summary>
        /// Handles the Click event of the LMPGraph control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void LMPGraph_Click(object sender, RoutedEventArgs e);
        /// <summary>
        /// Handles the Click event of the NodeAnalyzer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void NodeAnalyzer_Click(object sender, RoutedEventArgs e);
        /// <summary>
        /// Handles the SelectedCellsChanged event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectedCellsChangedEventArgs"/> instance containing the event data.</param>
        void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e);
    }
}
