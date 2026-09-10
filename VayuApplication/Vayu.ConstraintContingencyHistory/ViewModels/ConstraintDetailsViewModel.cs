using Prism.Mvvm;

namespace Vayu.ConstraintContingencyHistory.ViewModels
{
    public class ConstraintDetailsViewModel : BindableBase
    {
        private System.Collections.Generic.List<Model.Constraint> mConstraintList;

        public System.Collections.Generic.List<Model.Constraint> ConstraintList
        {
            get { return mConstraintList; }
            set
            {
                mConstraintList = value;
                RaisePropertyChanged("ConstraintList");
            }
        }
        public ConstraintDetailsViewModel()
        {

        }
    }
}
