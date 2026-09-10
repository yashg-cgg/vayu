
using System.Windows;
using System.Windows.Controls;

namespace Vayu.LTC_Graphs.Views
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
        /// Sets the position.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        public void SetPosition(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }
        ///// <summary>
        ///// Handles the OpenDropDown event of the GotFocus control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GotFocus_OpenDropDown(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }

        ///// <summary>
        ///// Handles the 1 event of the quarterlyCheck_Checked control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void quarterlyCheck_Checked_1(object sender, RoutedEventArgs e)
        {
            SetRoundInfo(marketCombo.Text);
            if (longRadio == null)
                return;

            if (longRadio.IsChecked.GetValueOrDefault())
                overlayGrid.Visibility = System.Windows.Visibility.Visible;
            else
                overlayGrid.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Handles the 1 event of the marketCombo_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void marketCombo_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            string market = e.AddedItems[0] as string;
            SetRoundInfo(market);
        }

        /// <summary>
        /// Sets the round information.
        /// </summary>
        /// <param name="market">The market.</param>
        private void SetRoundInfo(string market)
        {
            if (longRadio == null)
                return;

            if (market == "PJM")
            {
                if (longRadio.IsChecked.GetValueOrDefault()
                    || yearlyRadio.IsChecked.GetValueOrDefault())
                {
                    CRRCol.Visibility = System.Windows.Visibility.Hidden;
                    CRRDAMinusCRR.Visibility = System.Windows.Visibility.Hidden;
                    round1.Visibility = System.Windows.Visibility.Visible;
                    round2.Visibility = System.Windows.Visibility.Visible;
                    round3.Visibility = System.Windows.Visibility.Visible;
                    round4.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    CRRCol.Visibility = System.Windows.Visibility.Visible;
                    CRRDAMinusCRR.Visibility = System.Windows.Visibility.Visible;
                    round1.Visibility = System.Windows.Visibility.Hidden;
                    round2.Visibility = System.Windows.Visibility.Hidden;
                    round3.Visibility = System.Windows.Visibility.Hidden;
                    round4.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                if (quarterlyCheck.IsChecked.GetValueOrDefault())
                {
                    CRRCol.Visibility = System.Windows.Visibility.Hidden;
                    CRRDAMinusCRR.Visibility = System.Windows.Visibility.Hidden;
                    round1.Visibility = System.Windows.Visibility.Visible;
                    round2.Visibility = System.Windows.Visibility.Visible;
                    round3.Visibility = System.Windows.Visibility.Visible;
                    round4.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    CRRCol.Visibility = System.Windows.Visibility.Visible;
                    CRRDAMinusCRR.Visibility = System.Windows.Visibility.Visible;
                    round1.Visibility = System.Windows.Visibility.Hidden;
                    round2.Visibility = System.Windows.Visibility.Hidden;
                    round3.Visibility = System.Windows.Visibility.Hidden;
                    round4.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }
    }
}
