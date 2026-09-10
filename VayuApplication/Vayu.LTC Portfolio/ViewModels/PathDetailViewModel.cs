using Prism.Mvvm;
using System.Collections.Generic;
using Vayu.LTC_Portfolio.Model;

namespace Vayu.LTC_Portfolio.ViewModels
{
    public class PathDetailViewModel : BindableBase
    {
        private MainWindowViewModel mParentModel;

        private List<Exposure> mPathDetailList;
        /// <summary>
        /// Gets or sets the path detail list.
        /// </summary>
        /// <value>
        /// The path detail list.
        /// </value>
        public List<Exposure> PathDetailList
        {
            get
            {
                return mPathDetailList;
            }
            set
            {
                mPathDetailList = value;
                RaisePropertyChanged("PathDetailList");
            }
        }
        public PathDetailViewModel(MainWindowViewModel parentModel, List<Exposure> pathList)
        {
            mParentModel = parentModel;
            PathDetailList = pathList;
        }
    }
}
