using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using Vayu.WorkbookStatistics.Model;
using Vayu.WorkbookStatistics.Views;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class PathDetailViewModel : BindableBase
    {
        #region Declaration

        /// <summary>
        /// The m parent model
        /// </summary>
        public MainWindowViewModel mParentModel;
        /// <summary>
        /// The m constraint parent model
        /// </summary>
        private ConstraintCheckViewModel mConstraintParentModel;
        /// <summary>
        /// Gets the show path constraint dialog command.
        /// </summary>
        /// <value>
        /// The show path constraint dialog command.
        /// </value>
        public DelegateCommand ShowPathConstraintDialogCommand { get; private set; }
        /// <summary>
        /// The selected constraint path value
        /// </summary>
        private Exposure selectedConstraintPathValue;
        /// <summary>
        /// Gets or sets the selected constraint path value.
        /// </summary>
        /// <value>
        /// The selected constraint path value.
        /// </value>
        public Exposure SelectedConstraintPathValue
        {
            get { return selectedConstraintPathValue; }
            set
            {
                selectedConstraintPathValue = value;
                RaisePropertyChanged("SelectedConstraintPathValue");
            }
        }
        /// <summary>
        /// The m path detail list
        /// </summary>
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
        private string stTextScale1;
        public string TextScale1
        {
            get
            {
                return stTextScale1;
            }
            set
            {
                stTextScale1 = value;
                RaisePropertyChanged("TextScale1");
            }
        }
        private string mVisibleText1;

        public string VisibleText1
        {
            get
            {
                return mVisibleText1;
            }
            set
            {
                mVisibleText1 = value;
                RaisePropertyChanged("VisibleText1");
            }
        }
        private string mVisibleScale1;

        public string VisibleScale1
        {
            get
            {
                return mVisibleScale1;
            }
            set
            {
                mVisibleScale1 = value;
                RaisePropertyChanged("VisibleScale1");
            }
        }

        private string mVisibileGroup;

        public string VisibileGroup
        {
            get
            {
                return mVisibileGroup;
            }
            set
            {
                mVisibileGroup = value;
                RaisePropertyChanged("VisibileGroup");
            }
        }

        #endregion
        public PathDetailViewModel(MainWindowViewModel parentModel, List<Exposure> pathList, ConstraintCheckViewModel constraintcheckViewModel)
        {
            ShowPathConstraintDialogCommand = new DelegateCommand(() => DisplayPathConstraintDetails());
            DisplayPathConstraintDetails();
            mParentModel = parentModel;
            mConstraintParentModel = constraintcheckViewModel;
            PathDetailList = pathList;
        }
        private void DisplayPathConstraintDetails()
        {
            if (SelectedConstraintPathValue != null)
            {
                ConstraintPathDetailViewModel constVm = new ConstraintPathDetailViewModel(mParentModel, mConstraintParentModel, this, mParentModel.PathList, SelectedConstraintPathValue);
                new ConstraintPathDetail { DataContext = constVm }.Show();
            }
        }

        public PathDetailViewModel(ConstraintCheckViewModel constraintParentModel, List<Exposure> mPathList)
        {
            ShowPathConstraintDialogCommand = new DelegateCommand(() => DisplayPathConstraintDetails());
            mConstraintParentModel = constraintParentModel;
            PathDetailList = mPathList;
        }
    }
}
