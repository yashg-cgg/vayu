using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using Vayu.WorkbookStatistics.Model;
using Vayu.WorkbookStatistics.Views;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class ConstraintDollarCheckViewModel : BindableBase
    {
        #region Declaration And Properties

        /// <summary>
        /// The m parent model
        /// </summary>
        private MainWindowViewModel mParentModel;
        /// <summary>
        /// The m constraint check
        /// </summary>
        private ConstraintDollarCheck mConstraintCheck;
        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// Gets or sets the run retrieve right click command.
        /// </summary>
        /// <value>
        /// The run retrieve right click command.
        /// </value>
        public DelegateCommand RunRetrieveRightClickCommand { private set; get; }
        /// <summary>
        /// The m exposure constraint list
        /// </summary>
        private List<Exposure> mExposureConstraintList;
        /// <summary>
        /// Gets or sets the exposure constraint list.
        /// </summary>
        /// <value>
        /// The exposure constraint list.
        /// </value>
        public List<Exposure> ExposureConstraintList
        {
            get
            {
                return mExposureConstraintList;
            }
            set
            {
                mExposureConstraintList = value;
                RaisePropertyChanged("ExposureConstraintList");
            }
        }
        /// <summary>
        /// The m selected constraint path value
        /// </summary>
        private Exposure mSelectedConstraintPathValue;
        /// <summary>
        /// Gets or sets the selected constraint path value.
        /// </summary>
        /// <value>
        /// The selected constraint path value.
        /// </value>
        public Exposure SelectedConstraintPathValue
        {
            get
            {
                return mSelectedConstraintPathValue;
            }
            set
            {
                mSelectedConstraintPathValue = value;
                RaisePropertyChanged("SelectedConstraintPathValue");
            }
        }

        #endregion
        public ConstraintDollarCheckViewModel(MainWindowViewModel parentModel, List<Exposure> ConstraintList, ConstraintDollarCheck constraintCheck)
        {
            RunRetrieveRightClickCommand = new DelegateCommand(RightClickCommand);
            mParentModel = parentModel;
            mDataService = new WorkbookStatistics.Model.DataService();

            ExposureConstraintList = ConstraintList;

        }
        public void RightClickCommand()
        {
            Exposure ConstraintPathValue = new Exposure();
            ConstraintPathValue = SelectedConstraintPathValue;
            mParentModel.PathDetailsCommand(ConstraintPathValue.ID, null);
            List<Exposure> mPath = new List<Exposure>();
        }
    }
}
