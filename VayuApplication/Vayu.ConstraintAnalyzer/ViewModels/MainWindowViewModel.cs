using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vayu.ConstraintAnalyzer.Model;
using Vayu.DBLibrary;

namespace Vayu.ConstraintAnalyzer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly IDataService _dataService;
        /// <summary>
        /// The m constraints list
        /// </summary>
        private List<Constraints> mConstraintsList = new List<Constraints>();

        #region Properties

        #region Relay Command Properties

        /// <summary>
        /// Gets or sets the run retrieve fetch data command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run right click command.
        /// </summary>
        /// <value>
        /// The run right click command.
        /// </value>
        public DelegateCommand RunRightClickCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run right click maximum command.
        /// </summary>
        /// <value>
        /// The run right click maximum command.
        /// </value>
        public DelegateCommand RunRightClickMaxCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run right click up to command.
        /// </summary>
        /// <value>
        /// The run right click up to command.
        /// </value>
        public DelegateCommand RunRightClickUpToCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run right click tag outage command.
        /// </summary>
        /// <value>
        /// The run right click tag outage command.
        /// </value>
        public DelegateCommand RunRightClickTagOutageCommand { private set; get; }

        #endregion


        /// <summary>
        /// The m family CheckBox checked
        /// </summary>
        private bool mFamilyCheckBoxChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [family CheckBox checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [family CheckBox checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FamilyCheckBoxChecked
        {
            get
            {
                return mFamilyCheckBoxChecked;
            }
            set
            {
                mFamilyCheckBoxChecked = value;
                RaisePropertyChanged("FamilyCheckBoxChecked");
            }
        }
        /// <summary>
        /// The m family list
        /// </summary>
        private List<string> mFamilyList;
        /// <summary>
        /// Gets or sets the family list.
        /// </summary>
        /// <value>
        /// The family list.
        /// </value>
        public List<string> FamilyList
        {
            get
            {
                return mFamilyList;
            }
            set
            {
                mFamilyList = value;
                RaisePropertyChanged("FamilyList");
            }
        }
        /// <summary>
        /// The m selected family
        /// </summary>
        private string mSelectedFamily;
        /// <summary>
        /// Gets or sets the selected family.
        /// </summary>
        /// <value>
        /// The selected family.
        /// </value>
        public string SelectedFamily
        {
            get
            {
                return mSelectedFamily;
            }
            set
            {
                mSelectedFamily = value;
                RaisePropertyChanged("SelectedFamily");
            }
        }

        /// <summary>
        /// The m selected constraint
        /// </summary>
        private Constraints mSelectedConstraint;
        /// <summary>
        /// Gets or sets the selected constraint.
        /// </summary>
        /// <value>
        /// The selected constraint.
        /// </value>
        public Constraints SelectedConstraint
        {
            get
            {
                return mSelectedConstraint;
            }
            set
            {
                mSelectedConstraint = value;
                RaisePropertyChanged("SelectedConstraint");
            }
        }

        /// <summary>
        /// Gets or sets the constraints list.
        /// </summary>
        /// <value>
        /// The constraints list.
        /// </value>
        public List<Constraints> ConstraintsList
        {
            get
            {
                return mConstraintsList;
            }
            set
            {
                mConstraintsList = value;
                RaisePropertyChanged("ConstraintsList");
            }
        }
        /// <summary>
        /// The m rt RadioButton checked
        /// </summary>
        private bool mRtRadioButtonChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [rt RadioButton checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [rt RadioButton checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RtRadioButtonChecked
        {
            get
            {
                return mRtRadioButtonChecked;
            }
            set
            {
                mRtRadioButtonChecked = value;
                RaisePropertyChanged("RtRadioButtonChecked");
                if (SelectedMarket == "PJM")
                    grpFamilyContraint = false;
            }
        }
        /// <summary>
        /// The m da RadioButton checked
        /// </summary>
        private bool mDaRadioButtonChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [da RadioButton checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [da RadioButton checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DaRadioButtonChecked
        {
            get
            {
                return mDaRadioButtonChecked;
            }
            set
            {
                mDaRadioButtonChecked = value;
                RaisePropertyChanged("DaRadioButtonChecked");
                if (SelectedMarket == "PJM")
                    grpFamilyContraint = true;
            }
        }
        /// <summary>
        /// The m impact RadioButton checked
        /// </summary>
        private bool mImpactRadioButtonChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [impact RadioButton checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [impact RadioButton checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ImpactRadioButtonChecked
        {
            get
            {
                return mImpactRadioButtonChecked;
            }
            set
            {
                mImpactRadioButtonChecked = value;
                RaisePropertyChanged("ImpactRadioButtonChecked");
            }
        }
        /// <summary>
        /// The m shifted RadioButton checked
        /// </summary>
        private bool mShiftedRadioButtonChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [shifted RadioButton checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [shifted RadioButton checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ShiftedRadioButtonChecked
        {
            get
            {
                return mShiftedRadioButtonChecked;
            }
            set
            {
                mShiftedRadioButtonChecked = value;
                RaisePropertyChanged("ShiftedRadioButtonChecked");
            }
        }
        /// <summary>
        /// The m shadow RadioButton checked
        /// </summary>
        private bool mShadowRadioButtonChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [shadow RadioButton checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [shadow RadioButton checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ShadowRadioButtonChecked
        {
            get
            {
                return mShadowRadioButtonChecked;
            }
            set
            {
                mShadowRadioButtonChecked = value;
                RaisePropertyChanged("ShadowRadioButtonChecked");
            }
        }
        /// <summary>
        /// The m date range CheckBox checked
        /// </summary>
        private bool mDateRangeCheckBoxChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [date range CheckBox checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [date range CheckBox checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DateRangeCheckBoxChecked
        {
            get
            {
                return mDateRangeCheckBoxChecked;
            }
            set
            {
                mDateRangeCheckBoxChecked = value;
                RaisePropertyChanged("DateRangeCheckBoxChecked");
            }
        }
        /// <summary>
        /// The m date picker1 selected
        /// </summary>
        private DateTime mDatePicker1Selected;
        /// <summary>
        /// Gets or sets the date picker1 selected.
        /// </summary>
        /// <value>
        /// The date picker1 selected.
        /// </value>
        public DateTime DatePicker1Selected
        {
            get
            {
                return mDatePicker1Selected;
            }
            set
            {
                mDatePicker1Selected = value;
                RaisePropertyChanged("DatePicker1Selected");
            }
        }
        /// <summary>
        /// The m date picker2 selected
        /// </summary>
        private DateTime mDatePicker2Selected;
        /// <summary>
        /// Gets or sets the date picker2 selected.
        /// </summary>
        /// <value>
        /// The date picker2 selected.
        /// </value>
        public DateTime DatePicker2Selected
        {
            get
            {
                return mDatePicker2Selected;
            }
            set
            {
                mDatePicker2Selected = value;
                RaisePropertyChanged("DatePicker2Selected");
            }
        }

        /// <summary>
        /// The m is refresh enabled
        /// </summary>
        private bool mIsRefreshEnabled;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is refresh enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is refresh enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsRefreshEnabled
        {
            get { return mIsRefreshEnabled; }
            set
            {
                mIsRefreshEnabled = value;
                RaisePropertyChanged("IsRefreshEnabled");

            }
        }
        public string[] Marketlist { get; set; }
        public string mSelectedMarket;
        public string SelectedMarket
        {
            get { return mSelectedMarket; }
            set
            {
                mSelectedMarket = value;
                //RetrieveThreaded();
                RaisePropertyChanged("SelectedMarket");
                if (SelectedMarket == "PJM")
                    grpFamilyContraint = true;
                else
                    grpFamilyContraint = false;
            }
        }

        private bool mgrpFamilyContraint;
        public bool grpFamilyContraint
        {
            get
            {
                return mgrpFamilyContraint;
            }
            set
            {
                mgrpFamilyContraint = value;
                RaisePropertyChanged("grpFamilyContraint");
            }
        }
        #endregion


        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            RunRetrieveFetchDataCommand = new DelegateCommand(RetrieveFetchDataCommand);
            RunRightClickCommand = new DelegateCommand(RunRightClick);
            RunRightClickMaxCommand = new DelegateCommand(RunRightClickMax);
            RunRightClickUpToCommand = new DelegateCommand(RunRightClickUpTo);
            RunRightClickTagOutageCommand = new DelegateCommand(RunRightClickTagOutage);
            Marketlist = new string[] { "ERCOT" };
            SelectedMarket = Marketlist.FirstOrDefault();
            _dataService.loadDBCommands();
            DatePicker1Selected = DateTime.Today;
            DatePicker2Selected = DateTime.Today;
            FamilyList = DBAccess.GetFamilies();
            IsRefreshEnabled = true;
            RtRadioButtonChecked = true;
            ImpactRadioButtonChecked = true;
            _dataService = dataService;
            grpFamilyContraint = true;

        }

        #region Public Methods

        /// <summary>
        /// Retrieves the data.
        /// </summary>
        public void RetrieveFetchDataCommand()
        {
            Task.Factory.StartNew(() => { RetrieveThreaded(); });
        }

        /// <summary>
        /// Gets Exposure Node Min Details when clicked on Nodal Min DrillDown in context menu and shows it in Constraint Exposure Analyzer screen.
        /// </summary>
        public void RunRightClick()
        {
            Constraints constraint = new Constraints();
            constraint = SelectedConstraint;
            DateTime startTime = Convert.ToDateTime(constraint.date);
            List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();

            _dataService.GetExposureNodeMinDetails(
               (item, Error) =>
               {
                   sourceSinkNodeList = item;
               }, constraint);

            PricingNode BestNode = new PricingNode();
            foreach (SourceSinkData it in sourceSinkNodeList)
            {
                BestNode = it.Source;
            }

            Vayu.ConstraintExposure.ViewModels.MainWindowViewModel mainViewModel = new Vayu.ConstraintExposure.ViewModels.MainWindowViewModel(new Vayu.ConstraintExposure.Model.DataService());
            var window = new Vayu.ConstraintExposure.Views.MainWindow();
            window.DataContext = mainViewModel;
            mainViewModel.SetRadio();
            mainViewModel.AddSource(BestNode, startTime);
            mainViewModel.RetrieveFetchDataCommand();
            window.Show();
        }
        /// <summary>
        /// Gets Exposure Node Max Details when clicked on Nodal Max DrillDown in Context Menu and shows it in Constraint Exposure Analyzer screen.
        /// </summary>
        public void RunRightClickMax()
        {
            Constraints constraint = new Constraints();
            constraint = SelectedConstraint;
            DateTime startTime = Convert.ToDateTime(constraint.date);
            List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();

            _dataService.GetExposureNodeMaxDetails(
               (item, Error) =>
               {
                   sourceSinkNodeList = item;
               }, constraint);

            PricingNode BestNode = new PricingNode();
            foreach (SourceSinkData it in sourceSinkNodeList)
            {
                BestNode = it.Sink;
            }

            Vayu.ConstraintExposure.ViewModels.MainWindowViewModel mainViewModel = new Vayu.ConstraintExposure.ViewModels.MainWindowViewModel(new Vayu.ConstraintExposure.Model.DataService());
            var window = new Vayu.ConstraintExposure.Views.MainWindow();
            window.DataContext = mainViewModel;
            mainViewModel.AddSource(BestNode, startTime);
            mainViewModel.SetRadio();
            mainViewModel.RetrieveFetchDataCommand();
            window.Show();
        }
        /// <summary>
        /// Gets Exposure UpTo Min Details when clicked on Up To DrillDown in Context Menu and shows it in Constraint Exposure Analyzer screen.
        /// </summary>
        public void RunRightClickUpTo()
        {
            Constraints constraint = new Constraints();
            constraint = SelectedConstraint;
            DateTime startTime = Convert.ToDateTime(constraint.date);
            List<SourceSinkData> sourceSinkNodeList = new List<SourceSinkData>();

            _dataService.GetExposureUpMinDetails(
               (item, Error) =>
               {
                   sourceSinkNodeList = item;
               }, constraint);

            PricingNode BestNodeMin = new PricingNode();
            foreach (SourceSinkData it in sourceSinkNodeList)
            {
                BestNodeMin = it.Source;
            }

            List<SourceSinkData> sourceSinkNodeListMax = new List<SourceSinkData>();
            _dataService.GetExposureUpMaxDetails(
               (item, Error) =>
               {
                   sourceSinkNodeListMax = item;
               }, constraint);

            PricingNode BestNodeMax = new PricingNode();
            foreach (SourceSinkData it in sourceSinkNodeListMax)
            {
                BestNodeMax = it.Sink;
            }

            Vayu.ConstraintExposure.ViewModels.MainWindowViewModel mainViewModel = new Vayu.ConstraintExposure.ViewModels.MainWindowViewModel(new Vayu.ConstraintExposure.Model.DataService());
            var window = new Vayu.ConstraintExposure.Views.MainWindow();
            mainViewModel.SetRadioUpTo();
            mainViewModel.AddPath(BestNodeMin, BestNodeMax, startTime);
            mainViewModel.RetrieveFetchDataCommand();
            window.DataContext = mainViewModel;
            window.Show();
        }

        /// <summary>
        /// Sets the families.
        /// </summary>
        public void SetFamilies()
        {
            FamilyList = null;
            FamilyList = DBAccess.GetFamilies();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieves the Constraint related data.
        /// </summary>
        private void RetrieveThreaded()
        {
            IsRefreshEnabled = false;
            _dataService.GetConstraints((conConstraintList, error) =>
            {
                if (error != null)
                {
                    return;
                }
                ConstraintsList = conConstraintList.OrderByDescending(t => Convert.ToDouble(t.total)).ToList();
            }, RtRadioButtonChecked, DaRadioButtonChecked, ImpactRadioButtonChecked, ShiftedRadioButtonChecked,
             ShadowRadioButtonChecked, DateRangeCheckBoxChecked, DatePicker1Selected, DatePicker2Selected, SelectedFamily,
             FamilyCheckBoxChecked, GetMarketKey());
            IsRefreshEnabled = true;
        }
        public int GetMarketKey()
        {
            switch (SelectedMarket)
            {
                case "PJM": return 1;
                case "ERCOT": return 9;
                case "CAISO": return 7;
                default: return 0;
            }
        }
        /// <summary>
        /// Opens Relationship Tracker when clicked on Tag Outage Relationship in context menu
        /// Runs the right click .
        /// </summary>
        private void RunRightClickTagOutage()
        {
            Constraints constraint = new Constraints();
            constraint = SelectedConstraint;
            int constraintNum = constraint.constraintNum;
            string monitored = constraint.monitoredName;
            DateTime day = Convert.ToDateTime(constraint.date);


        }

        #endregion
    }
}
