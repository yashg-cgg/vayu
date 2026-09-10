using Prism.Mvvm;

namespace Vayu.LTC_PortfolioAnnual.ViewModels
{
    public class ErrorDialogViewModel : BindableBase
    {

        /// <summary>
        /// The display text
        /// </summary>
        private string displayText;
        /// <summary>
        /// Gets or sets the display text.
        /// </summary>
        /// <value>
        /// The display text.
        /// </value>
        public string DisplayText
        {
            get { return displayText; }
            set { displayText = value; RaisePropertyChanged("DisplayText"); }
        }
    }
}
