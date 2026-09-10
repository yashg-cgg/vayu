using OxyPlot;
using Prism.Mvvm;

namespace Vayu.LTC_PortfolioAnnual.ViewModels
{
    public class PieChartViewModel : BindableBase
    {
        private PlotModel mplotDataFirst;
        public PlotModel PlotDataFirstNew
        {
            get
            {
                return mplotDataFirst;
            }
            set
            {
                mplotDataFirst = value;
                RaisePropertyChanged("PlotDataFirst");
            }
        }
        public PieChartViewModel()
        {

        }
    }
}
