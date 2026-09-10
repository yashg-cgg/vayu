using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.PowerMap.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string selMarket;
        /// <summary>
        /// Gets or sets the selected market.
        /// </summary>
        /// <value>
        /// The selected market.
        /// </value>
        public string SelectedMarket
        {
            get { return selMarket; }
            set { selMarket = value; RaisePropertyChanged("SelectedMarket"); }
        }

        public Dictionary<string, int> Markets { get; set; }

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public DelegateCommand RefreshCommand { get; private set; }

        public DelegateCommand RunExportCSVCommand { private set; get; }

        public MainWindowViewModel()
        {
            RefreshCommand = new DelegateCommand(() => OnRefreshCommand());

            Markets = new Dictionary<string, int>() { { "ERCOT", 9 } };
        }
        /// <summary>
        /// Called when [refresh command].
        /// </summary>
        private void OnRefreshCommand()
        {
            //MarketType mkType = GetMarketType(SelectedMarket);
            //Hashtable hash = NodeInfo.GetHash(mkType);
            //ConstraintList = ConstraintHelper.GetLatestConstraintInfo(mkType, hash);
            //MapConstraintList = ConstraintList.Where(x => x.Fromlocation != null || x.ToLocation != null).ToList();
            //NonMapConstraintList = ConstraintList.Where(x => x.Fromlocation == null && x.ToLocation == null).ToList();
        }

        /// <summary>
        /// Gets the type of the market.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private MarketType GetMarketType(string name)
        {
            if (name == "PJM")
                return MarketType.PJM;
            else if (name == "ERCOT")
                return MarketType.ERCOT;
            else
                return MarketType.NONE;
        }
    }
}
