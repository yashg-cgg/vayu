using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using Vayu.CRRPNLDetails.Model;

namespace Vayu.CRRPNLDetails.ViewModels
{
    public class PathDetailViewModel : BindableBase
    {
        private List<PathExposure> _PathwiseExposureList;

        public List<PathExposure> PathwiseExposureList
        {
            get { return _PathwiseExposureList; }
            set
            {
                _PathwiseExposureList = value;
                RaisePropertyChanged("PathwiseExposureList");
            }
        }

        public PathDetailViewModel(ExposureDetailsViewModel1 parentViewModel, List<CRRPNLDetails.Model.PathExposure> exposureDetailsList)
        {
            PathwiseExposureList = exposureDetailsList.ToList();
        }
    }
}
