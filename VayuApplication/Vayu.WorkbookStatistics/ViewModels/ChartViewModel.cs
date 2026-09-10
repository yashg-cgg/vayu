using OxyPlot;
using Prism.Mvvm;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class ChartViewModel : BindableBase
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
        public ChartViewModel()
        {

        }
    }
}
