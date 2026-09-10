using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Vayu.CommonControls;
using Vayu.ConstraintOutageMapping.Model;

namespace Vayu.ConstraintOutageMapping.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Declaration

        /// <summary>
        /// The m data service
        /// </summary>
        public readonly IDataService mDataService;
        /// <summary>
        /// Gets or sets the retrieve.
        /// </summary>
        /// <value>
        /// The retrieve.
        /// </value>
        public DelegateCommand Retrieve { private set; get; }

        public DelegateCommand ExportCommand { private set; get; }

        /// <summary>
        /// The temporary constraint search list
        /// </summary>
        private List<Constraints> TempConstraintSearchList = new List<Constraints>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            CurrentOutagesChecked = true;
            mDataService = dataService;
            Retrieve = new DelegateCommand(() => fillConstraintSearchList());
            ExportCommand = new DelegateCommand(() => Export());
            TempConstraintSearchList = mDataService.GetConstraintList(mStartDate, EndDate);
            ConstraintSearchList = TempConstraintSearchList.Select(x => x.ConstraintName).ToList();
        }

        private void Export()
        {
            try
            {
                ExportToExcelNodeSpread<ConstraintOutages, List<ConstraintOutages>> obj = new ExportToExcelNodeSpread<ConstraintOutages, List<ConstraintOutages>>();

                obj.dataToPrint = OutageList;
                obj.GenerateReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Properties

        /// <summary>
        /// The m start date
        /// </summary>
        private DateTime mStartDate = DateTime.Today;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate
        {
            get
            {
                return mStartDate;
            }
            set
            {
                mStartDate = value;
                RaisePropertyChanged("StartDate");
                ConstraintSearchList = null;
                ContingencySearchList = null;
                OutageList = null;
                TempConstraintSearchList = mDataService.GetConstraintList(mStartDate, EndDate);
                ConstraintSearchList = TempConstraintSearchList.Select(x => x.ConstraintName).ToList();
            }
        }
        /// <summary>
        /// The m end date
        /// </summary>
        private DateTime mEndDate = DateTime.Today.AddDays(1);
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate
        {
            get
            {
                return mEndDate;
            }
            set
            {
                mEndDate = value;
                RaisePropertyChanged("EndDate");
                ConstraintSearchList = null;
                ContingencySearchList = null;
                OutageList = null;
                TempConstraintSearchList = mDataService.GetConstraintList(mStartDate, EndDate);
                ConstraintSearchList = TempConstraintSearchList.Select(x => x.ConstraintName).ToList();
            }
        }
        /// <summary>
        /// The m outage list
        /// </summary>
        private List<ConstraintOutages> mOutageList;
        /// <summary>
        /// Gets or sets the outage list.
        /// </summary>
        /// <value>
        /// The outage list.
        /// </value>
        public List<ConstraintOutages> OutageList
        {
            get
            {
                return mOutageList;
            }
            set
            {
                mOutageList = value;
                RaisePropertyChanged("OutageList");
            }
        }
        /// <summary>
        /// The m constraint search list
        /// </summary>
        private List<string> mConstraintSearchList;
        /// <summary>
        /// Gets or sets the constraint search list.
        /// </summary>
        /// <value>
        /// The constraint search list.
        /// </value>
        public List<string> ConstraintSearchList
        {
            get { return mConstraintSearchList; }
            set
            {
                mConstraintSearchList = value;
                RaisePropertyChanged("ConstraintSearchList");
            }
        }
        private bool mgrpConstContig = true;
        public bool grpConstContig
        {
            get
            {
                return mgrpConstContig;
            }
            set
            {
                mgrpConstContig = value;
                RaisePropertyChanged("grpConstContig");
            }
        }
        private bool misAllConstaintchecked;
        public bool isAllConstaintchecked
        {
            get
            {
                return misAllConstaintchecked;
            }
            set
            {
                misAllConstaintchecked = value;
                RaisePropertyChanged("isAllConstaintchecked");
                if (isAllConstaintchecked)
                {
                    grpConstContig = false;
                    ConstraintSearchList = null;
                    ContingencySearchList = null;
                    OutageList = null;
                }
                else
                {
                    grpConstContig = true;
                    TempConstraintSearchList = null;
                    ConstraintSearchList = null;
                    TempConstraintSearchList = mDataService.GetConstraintList(mStartDate, EndDate);
                    ConstraintSearchList = TempConstraintSearchList.Select(x => x.ConstraintName).ToList();
                    OutageList = null;
                }
            }
        }

        private bool mIsRefreshEnabled = true;
        public bool IsRefreshEnabled
        {
            get
            {
                return mIsRefreshEnabled;
            }
            set
            {
                mIsRefreshEnabled = value;
                RaisePropertyChanged("IsRefreshEnabled");
            }
        }
        /// <summary>
        /// The contingency search list
        /// </summary>
        private List<string> _ContingencySearchList;
        /// <summary>
        /// Gets or sets the contingency search list.
        /// </summary>
        /// <value>
        /// The contingency search list.
        /// </value>
        public List<string> ContingencySearchList
        {
            get { return _ContingencySearchList; }
            set
            {
                _ContingencySearchList = value;
                RaisePropertyChanged("ContingencySearchList");
            }
        }
        /// <summary>
        /// Gets or sets the selected constraint.
        /// </summary>
        /// <value>
        /// The selected constraint.
        /// </value>
        public Constraints SelectedConstraint { get; set; }
        /// <summary>
        /// The selected constraint item
        /// </summary>
        private string _SelectedConstraintItem;
        /// <summary>
        /// Gets or sets the selected constraint item.
        /// </summary>
        /// <value>
        /// The selected constraint item.
        /// </value>
        public string SelectedConstraintItem
        {
            get { return _SelectedConstraintItem; }
            set
            {
                _SelectedConstraintItem = value;
                RaisePropertyChanged("SelectedConstraintItem");
                //if (SelectedConstraintItem != null)
                //{
                //    if (RTChecked)
                //        GetAllContingencies(SelectedConstraintItem, true);
                //    else if (DAChecked)
                //        GetAllContingencies(SelectedConstraintItem, false);
                //}
            }
        }
        /// <summary>
        /// The selected contengency item
        /// </summary>
        private string _SelectedContengencyItem;
        /// <summary>
        /// Gets or sets the selected contengency item.
        /// </summary>
        /// <value>
        /// The selected contengency item.
        /// </value>
        public string SelectedContengencyItem
        {
            get { return _SelectedContengencyItem; }
            set
            {
                _SelectedContengencyItem = value;
                RaisePropertyChanged("SelectedContengencyItem");
            }
        }
        /// <summary>
        /// Fills the constraint search list.
        /// </summary>
        public void fillConstraintSearchList()
        {
            this.IsRefreshEnabled = false;
            bool isnull = false;
            OutageList = new List<ConstraintOutages>();
            if (!isAllConstaintchecked)
            {
                if (SelectedConstraintItem == null || SelectedContengencyItem == null)
                {
                    MessageBox.Show("Please Select Constriant and Contigency");
                    isnull = true;
                }
            }
            if (!isnull)
            {
                if (CurrentOutagesChecked == true)
                {
                    OutageList = mDataService.GetOutageData(SelectedConstraintItem, SelectedContengencyItem, StartDate, EndDate, true, PlannedOutagesChecked, isAllConstaintchecked);
                }
                else if (PlannedOutagesChecked == true)
                {
                    OutageList = mDataService.GetOutageData(SelectedConstraintItem, SelectedContengencyItem, StartDate, EndDate, false, PlannedOutagesChecked, isAllConstaintchecked);
                }
                else
                {
                    OutageList = mDataService.GetAllOutageData(SelectedConstraintItem, SelectedContengencyItem, isAllConstaintchecked, StartDate, EndDate);
                }
            }
            this.IsRefreshEnabled = true;
        }
        /// <summary>
        /// The m check all constraint outages
        /// </summary>
        private bool mCheckAllConstraintOutages;
        /// <summary>
        /// Gets or sets a value indicating whether [check all constraint outages].
        /// </summary>
        /// <value>
        /// <c>true</c> if [check all constraint outages]; otherwise, <c>false</c>.
        /// </value>
        public bool CheckAllConstraintOutages
        {
            get
            {
                return mCheckAllConstraintOutages;
            }
            set
            {
                mCheckAllConstraintOutages = value;

                RaisePropertyChanged("DateRangeCheckBoxChecked");

            }
        }
        /// <summary>
        /// The m planned checked
        /// </summary>
        private bool mPlannedChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [planned outages checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [planned outages checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PlannedOutagesChecked
        {
            get { return mPlannedChecked; }
            set
            {
                mPlannedChecked = value;
                RaisePropertyChanged("PlannedOutagesChecked");
                if (PlannedOutagesChecked == true)
                {
                    CheckAllConstraintOutages = false;
                }
            }
        }
        /// <summary>
        /// The m current checked
        /// </summary>
        private bool mCurrentChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [current outages checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [current outages checked]; otherwise, <c>false</c>.
        /// </value>
        public bool CurrentOutagesChecked
        {
            get { return mCurrentChecked; }
            set
            {
                mCurrentChecked = value;
                RaisePropertyChanged("CurrentOutagesChecked");
                CheckAllConstraintOutages = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calls MainViewModel of ConstraintOutageMapping.
        /// </summary>
        private static ConstraintOutageMapping.ViewModels.MainWindowViewModel sProfileViewModel = new ConstraintOutageMapping.ViewModels.MainWindowViewModel(new ConstraintOutageMapping.Model.DataService());
        /// <summary>
        /// The constraint outage mapping
        /// </summary>
        private static ConstraintOutageMapping.Views.MainWindow sConstraintOutageMapping = new ConstraintOutageMapping.Views.MainWindow();

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the outage data.
        /// </summary>
        /// <param name="SelectedConstraintItemm">The selected constraint itemm.</param>
        /// <param name="SelectedContengencyItemm">The selected contengency itemm.</param>
        /// <param name="StartDatee">The start datee.</param>
        /// <param name="EndDatee">The end datee.</param>
        public void GetOutageData(string SelectedConstraintItemm, string SelectedContengencyItemm, DateTime StartDatee, DateTime EndDatee)
        {
            mStartDate = StartDatee;
            EndDate = EndDatee;
            CurrentOutagesChecked = true;
            TempConstraintSearchList = mDataService.GetConstraintList(mStartDate, EndDate);
            ConstraintSearchList = TempConstraintSearchList.Select(x => x.ConstraintName).ToList();
            Retrieve = new DelegateCommand(() => fillConstraintSearchList());
        }

        /// <summary>
        /// Opens the constraint outage mapping screen.
        /// </summary>
        /// <param name="sSelectedConstraintItem">The s selected constraint item.</param>
        /// <param name="sSelectedContengencyItem">The s selected contengency item.</param>
        /// <param name="sStartDate">The s start date.</param>
        /// <param name="sEndDate">The s end date.</param>
        public static void OpenConstraintOutageMappingScreen(string sSelectedConstraintItem, string sSelectedContengencyItem, DateTime sStartDate, DateTime sEndDate)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (!sConstraintOutageMapping.IsVisible)
            {
                sConstraintOutageMapping = new ConstraintOutageMapping.Views.MainWindow();
            }
            sConstraintOutageMapping.DataContext = sProfileViewModel;
            sProfileViewModel.mStartDate = sStartDate;
            sProfileViewModel.CurrentOutagesChecked = true;
            sProfileViewModel.EndDate = sEndDate;
            sProfileViewModel.TempConstraintSearchList = sProfileViewModel.mDataService.GetConstraintList(sProfileViewModel.mStartDate, sProfileViewModel.EndDate);
            sProfileViewModel.ConstraintSearchList = sProfileViewModel.TempConstraintSearchList.Select(x => x.ConstraintName).ToList();
            sProfileViewModel.SelectedConstraintItem = sSelectedConstraintItem;
            sProfileViewModel.SelectedContengencyItem = sSelectedContengencyItem;
            sProfileViewModel.ContingencySearchList = sProfileViewModel.mDataService.GetContingencyList(sProfileViewModel.StartDate, sProfileViewModel.EndDate, sProfileViewModel.SelectedConstraintItem);
            sProfileViewModel.OutageList = sProfileViewModel.mDataService.GetOutageData(sProfileViewModel.SelectedConstraintItem, sProfileViewModel.SelectedContengencyItem, sProfileViewModel.StartDate, sProfileViewModel.EndDate, true, sProfileViewModel.PlannedOutagesChecked, false);
            sConstraintOutageMapping.Show();
            sConstraintOutageMapping.Activate();
            Mouse.OverrideCursor = null;
        }

        #endregion
    }
}
