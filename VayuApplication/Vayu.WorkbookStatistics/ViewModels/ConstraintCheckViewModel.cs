using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using Vayu.WorkbookStatistics.Model;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class ConstraintCheckViewModel : BindableBase
    {

        #region Declaration & Properties

        /// <summary>
        /// The m parent model
        /// </summary>
        private MainWindowViewModel mParentModel;
        /// <summary>
        /// The m constraint check
        /// </summary>
        private Views.ConstraintCheck mConstraintCheck;
        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;

        /// <summary>
        /// The m message display
        /// </summary>
        private string mMessageDisplay;
        /// <summary>
        /// Gets or sets the message display.
        /// </summary>
        /// <value>
        /// The message display.
        /// </value>
        public string MessageDisplay
        {
            get
            {
                return mMessageDisplay;
            }
            set
            {
                mMessageDisplay = value;
                RaisePropertyChanged("MessageDisplay");
            }
        }

        /// <summary>
        /// The m is XML
        /// </summary>
        private bool mIsXml;
        /// <summary>
        /// Gets or sets the override command.
        /// </summary>
        /// <value>
        /// The override command.
        /// </value>
        public DelegateCommand OverrideCommand { private set; get; }
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

        public ConstraintCheckViewModel(MainWindowViewModel parentModel, List<Exposure> ConstraintList,
            Views.ConstraintCheck constraintCheck, bool isXml)
        {
            RunRetrieveRightClickCommand = new DelegateCommand(RightClickCommand);
            OverrideCommand = new DelegateCommand(Override);
            mParentModel = parentModel;
            mConstraintCheck = constraintCheck;
            mDataService = new WorkbookStatistics.Model.DataService();
            ExposureConstraintList = ConstraintList;
            if (ExposureConstraintList.Count == 0)
            {
                MessageDisplay = "Risk Constraints information not available. You are submitting bids without this information.";
            }
            mIsXml = isXml;
        }

        public void Override()
        {
            mParentModel.FinalSubmit(mIsXml);
            mConstraintCheck.Close();
        }

        public void RightClickCommand()
        {
            Exposure ConstraintPathValue = new Exposure();
            ConstraintPathValue = SelectedConstraintPathValue;
            mParentModel.PathDetailsCommand(ConstraintPathValue.ID, this);
            List<Exposure> mPath = new List<Exposure>();
        }
    }
}
