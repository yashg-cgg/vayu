
using System.Windows;
using Vayu.LTC_PortfolioAnnual.ViewModels;

namespace Vayu.LTC_PortfolioAnnual.Views
{
    /// <summary>
    /// Interaction logic for ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : Window
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Handles the Close event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Gets or sets the long text message.
        /// </summary>
        /// <value>
        /// The long text message.
        /// </value>
        public string LongTextMessage
        {
            get { return (string)GetValue(LongTextMessageProperty); }
            set
            {
                SetValue(LongTextMessageProperty, value);
                ErrorDialogViewModel viewmodel = this.DataContext as ErrorDialogViewModel;
                viewmodel.DisplayText = value;
            }
        }

        // Using a DependencyProperty as the backing store for LongTextMessage.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The long text message property
        /// </summary>
        public static readonly DependencyProperty LongTextMessageProperty =
            DependencyProperty.Register("LongTextMessage", typeof(string), typeof(ErrorDialog), new PropertyMetadata(null));
    }
}
