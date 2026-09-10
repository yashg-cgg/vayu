using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Vayu.ConstraintContingencyHistory.Model;
using Vayu.ConstraintContingencyHistory.Views;

namespace Vayu.ConstraintContingencyHistory.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// The data service object
        /// </summary>
        private IDataService dataService;

        #region Properties

        #region Relay Command Properties

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public DelegateCommand RefreshCommand { get; private set; }
        /// <summary>
        /// Gets the clear command.
        /// </summary>
        /// <value>
        /// The clear command.
        /// </value>
        public DelegateCommand ClearCommand { get; private set; }
        /// <summary>
        /// Gets the paste command.
        /// </summary>
        /// <value>
        /// The paste command.
        /// </value>
        public DelegateCommand PasteCommand { get; private set; }
        public DelegateCommand Export5MinLMPsToExcel { get; set; }
        /// <summary>
        /// Gets the hour details command.
        /// </summary>
        /// <value>
        /// The hour details command.
        /// </value>
        public DelegateCommand HourDetailsCommand { get; private set; }

        public DelegateCommand ExportFiveMinPricesCmd { get; private set; }
        public DelegateCommand<object> CellClickCmd { get; private set; }

        public DelegateCommand AllButtonClickCmd { get; private set; }

        public DelegateCommand RunExportCSVCommand { private set; get; }

        public DelegateCommand FTRFIVEMINPRICE { get; private set; }
        public DelegateCommand ALLFIVEMINPRICE { get; private set; }

        public DelegateCommand AllErcotShiftFactors { get; private set; }

        public DelegateCommand ConstraintHistory { get; private set; }
        public DelegateCommand ConstraintRTHistory { get; private set; }
        public DelegateCommand ConstraintDAHistory { get; private set; }

        #endregion

        public Dictionary<string, double> ploadDictHash = new Dictionary<string, double>();
        public Dictionary<string, double> eloadDictHash = new Dictionary<string, double>();
        /// <summary>
        /// <summary>
        /// Gets or sets the market list.
        /// </summary>
        /// <value>
        /// The market list.
        /// </value>
        public string[] MarketList { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is refresh enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is refresh enabled; otherwise, <c>false</c>.
        /// </value>
        //public bool IsRefreshEnabled { get; set; }


        private bool _isWinterClicked;
        public bool IsWinterClicked
        {
            get { return _isWinterClicked; }
            set
            {
                _isWinterClicked = value;
                if (_isWinterClicked == true)
                {
                    RefreshList();
                }

            }
        }
        private bool _isSpringClicked;
        public bool IsSpringClicked
        {
            get { return _isSpringClicked; }
            set
            {
                _isSpringClicked = value;
                // Call RefreshList method whenever IsWinterClicked changes
                if (_isSpringClicked == true)
                {
                    RefreshList();
                }

            }
        }
        private bool _isSummerClicked;
        public bool IsSummerClicked
        {
            get { return _isSummerClicked; }
            set
            {
                _isSummerClicked = value;
                // Call RefreshList method whenever IsWinterClicked changes
                if (_isSummerClicked == true)
                {
                    RefreshList();
                }
            }
        }
        private bool _isFallClicked;
        public bool IsFallClicked
        {
            get { return _isFallClicked; }
            set
            {
                _isFallClicked = value;
                // Call RefreshList method whenever IsWinterClicked changes
                if (_isFallClicked == true)
                {
                    RefreshList();
                }
            }
        }

        private bool _isNoneClicked;
        public bool IsNoneClicked
        {
            get { return _isNoneClicked; }
            set
            {
                _isNoneClicked = value;
                // Call RefreshList method whenever IsWinterClicked changes
                if (_isNoneClicked)
                {
                    RefreshList();
                }


            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is refresh enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is refresh enabled; otherwise, <c>false</c>.
        /// </value>
        //public bool IsRefreshEnabled { get; set; }
        private bool mDecChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [decimal checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [decimal checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DecChecked
        {
            get
            {
                return mDecChecked;
            }
            set
            {
                mDecChecked = value;
                RefreshList();
                //UpdateChartCommand();
                RaisePropertyChanged("DecChecked");
            }
        }
        /// <summary>
        /// The m nov checked
        /// </summary>
        private bool mNovChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [nov checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [nov checked]; otherwise, <c>false</c>.
        /// </value>
        public bool NovChecked
        {
            get
            {
                return mNovChecked;
            }
            set
            {
                mNovChecked = value;
                RefreshList();
                //UpdateChartCommand();
                RaisePropertyChanged("NovChecked");
            }
        }
        /// <summary>
        /// The m oct checked
        /// </summary>
        private bool mOctChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [oct checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [oct checked]; otherwise, <c>false</c>.
        /// </value>
        public bool OctChecked
        {
            get
            {
                return mOctChecked;
            }
            set
            {
                mOctChecked = value;
                RefreshList();
                //UpdateChartCommand();
                RaisePropertyChanged("OctChecked");
            }
        }
        /// <summary>
        /// The m sep checked
        /// </summary>
        private bool mSepChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [sep checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sep checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SepChecked
        {
            get
            {
                return mSepChecked;
            }
            set
            {
                mSepChecked = value;
                RefreshList();
                //UpdateChartCommand();
                RaisePropertyChanged("SepChecked");
            }
        }
        /// <summary>
        /// The m aug checked
        /// </summary>
        private bool mAugChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [aug checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [aug checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AugChecked
        {
            get
            {
                return mAugChecked;
            }
            set
            {
                mAugChecked = value;
                RefreshList();
                // UpdateChartCommand();
                RaisePropertyChanged("AugChecked");
            }
        }
        /// <summary>
        /// The m jul checked
        /// </summary>
        private bool mJulChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [jul checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [jul checked]; otherwise, <c>false</c>.
        /// </value>
        public bool JulChecked
        {
            get
            {
                return mJulChecked;
            }
            set
            {
                mJulChecked = value;
                RefreshList();
                // UpdateChartCommand();
                RaisePropertyChanged("JulChecked");
            }
        }
        /// <summary>
        /// The m jun checked
        /// </summary>
        private bool mJunChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [jun checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [jun checked]; otherwise, <c>false</c>.
        /// </value>
        public bool JunChecked
        {
            get
            {
                return mJunChecked;
            }
            set
            {
                mJunChecked = value;
                RefreshList();
                // UpdateChartCommand();
                RaisePropertyChanged("JunChecked");
            }
        }
        /// <summary>
        /// The m may checked
        /// </summary>
        private bool mMayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [may checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [may checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MayChecked
        {
            get
            {
                return mMayChecked;
            }
            set
            {
                mMayChecked = value;
                RefreshList();
                // UpdateChartCommand();
                RaisePropertyChanged("MayChecked");
            }
        }
        /// <summary>
        /// The m apr checked
        /// </summary>
        private bool mAprChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [apr checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [apr checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AprChecked
        {
            get
            {
                return mAprChecked;
            }
            set
            {
                mAprChecked = value;
                RefreshList();
                // UpdateChartCommand();
                RaisePropertyChanged("AprChecked");
            }
        }
        /// <summary>
        /// The m mar checked
        /// </summary>
        private bool mMarChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [mar checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [mar checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MarChecked
        {
            get
            {
                return mMarChecked;
            }
            set
            {
                mMarChecked = value;
                RefreshList();
                // UpdateChartCommand();
                RaisePropertyChanged("MarChecked");
            }
        }
        /// <summary>
        /// The m feb checked
        /// </summary>
        private bool mFebChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [feb checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [feb checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FebChecked
        {
            get
            {
                return mFebChecked;
            }
            set
            {
                mFebChecked = value;
                RefreshList();
                //UpdateChartCommand();
                RaisePropertyChanged("FebChecked");
            }
        }
        /// <summary>
        /// The m jan checked
        /// </summary>
        private bool mJanChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [jan checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [jan checked]; otherwise, <c>false</c>.
        /// </value>
        public bool JanChecked
        {
            get
            {
                return mJanChecked;
            }
            set
            {
                mJanChecked = value;

                RefreshList();

                RaisePropertyChanged("JanChecked");
            }
        }





        private bool _isErcotEnabled;

        public bool isErcotEnabled
        {
            get { return _isErcotEnabled; }
            set
            {
                _isErcotEnabled = value;
                RaisePropertyChanged("isErcotEnabled");
            }
        }


        private int _winterFrequency;
        public int WinterFrequency
        {
            get { return _winterFrequency; }
            set
            {
                _winterFrequency = value;
                RaisePropertyChanged(nameof(WinterFrequency));
            }
        }

        private int _springFrequency;
        public int SpringFrequency
        {
            get { return _springFrequency; }
            set
            {
                _springFrequency = value;
                RaisePropertyChanged(nameof(SpringFrequency));
            }
        }

        private int _summerFrequency;
        public int SummerFrequency
        {
            get { return _summerFrequency; }
            set
            {
                _summerFrequency = value;
                RaisePropertyChanged(nameof(SummerFrequency));
            }
        }

        private int _fallFrequency;
        public int FallFrequency
        {
            get { return _fallFrequency; }
            set
            {
                _fallFrequency = value;
                RaisePropertyChanged(nameof(FallFrequency));
            }
        }
        public ICommand CellClickCommand { get; private set; }

        public ICommand AllButtonClickCommand { get; private set; }
        private bool mIsRefreshEnabled;
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
        /// Gets or sets the column selected.
        /// </summary>
        /// <value>
        /// The column selected.
        /// </value>
        public System.Reflection.PropertyInfo ColumnSelected { get; set; }


        /// <summary>
        /// Gets or sets the selected constraint.
        /// </summary>
        /// <value>
        /// The selected constraint.
        /// </value>
        public Constraint SelectedConstraint { get; set; }
        /// <summary>
        /// Gets or sets the detailed constraint list.
        /// </summary>
        /// <value>
        /// The detailed constraint list.
        /// </value>
        public List<Constraint> DetailedConstraintList { get; set; }

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

        /// <summary>
        /// The search typed text
        /// </summary>
        private string _SearchTypedText;
        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text.
        /// </value>
        public string SearchTypedText
        {
            get { return _SearchTypedText; }
            set
            {
                _SearchTypedText = value;
                RaisePropertyChanged("SearchTypedText");
            }
        }

        /// <summary>
        /// The is constraint checked
        /// </summary>
        private bool _isConstraintChecked;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constraint checked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is constraint checked; otherwise, <c>false</c>.
        /// </value>
        public bool isConstraintChecked
        {
            get { return _isConstraintChecked; }
            set
            {
                _isConstraintChecked = value;
                RaisePropertyChanged("isConstraintChecked");
            }
        }

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
                if (SelectedConstraintItem != null)
                {
                    //if (SelectedConstraintItem == null)
                    {
                        if (ConstraintChecked)
                        {
                            if (RTChecked)
                                GetAllContingencies(SelectedConstraintItem, true);
                            else if (DAChecked)
                                GetAllContingencies(SelectedConstraintItem, false);
                        }
                    }
                }
                RaisePropertyChanged("SelectedConstraintItem");
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
                if (SelectedContengencyItem != null)
                {

                    if (ContingencyChecked)
                    {
                        //if (RTChecked)
                        //    GetAllConstraints(SelectedConstraintItem, true);
                        //else if (DAChecked)
                        //    GetAllConstraints(SelectedConstraintItem, false);
                    }

                }
                RaisePropertyChanged("SelectedConstraintItem");

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
        /// The m market selected
        /// </summary>
        private string mMarketSelected;
        /// <summary>
        /// Gets or sets the market selected.
        /// </summary>
        /// <value>
        /// The market selected.
        /// </value>
        public string MarketSelected
        {
            get { return mMarketSelected; }
            set
            {
                mMarketSelected = value;
                RaisePropertyChanged("MarketSelected");
                SelectedConstraintItem = null;
                SelectedContengencyItem = null;
                ConstraintSearchList = null;
                ContingencySearchList = null;
                if (MarketSelected.ToUpper() == "ERCOT")
                {
                    isErcotEnabled = true;
                }
                else
                {
                    isErcotEnabled = false;
                }
                //if(ConstraintSearchList != null && ContingencySearchList != null)
                //{
                //    ConstraintSearchList.Clear();
                //    ContingencySearchList.Clear();
                //}

                fillConstraintSearchList(RTChecked);


                if (value != null)
                {
                    IsSpp = value.ToUpper() == "SPP";//? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    ConstraintList = new List<Constraint>();
                }
            }
        }

        /// <summary>
        /// The m from date
        /// </summary>
        private DateTime mFromDate = DateTime.Today;
        /// <summary>
        /// Gets or sets from date.
        /// </summary>
        /// <value>
        /// From date.
        /// </value>
        public DateTime FromDate
        {
            get { return mFromDate; }
            set
            {
                if (value != null && DateRangeCheckBoxChecked && value > ThroDate)
                {
                    System.Windows.MessageBox.Show("thro' date should be greater than from date");
                    mFromDate = DateTime.Today;
                }
                else
                    mFromDate = value;

                mFromDate = value.Date;                            //.AddHours(1);
                RaisePropertyChanged("StartDate");
                RaisePropertyChanged("FromDate");
            }
        }

        /// <summary>
        /// The m thro date
        /// </summary>
        private DateTime mThroDate = DateTime.Today;
        /// <summary>
        /// Gets or sets the thro date.
        /// </summary>
        /// <value>
        /// The thro date.
        /// </value>
        public DateTime ThroDate
        {
            get { return mThroDate; }
            set
            {
                if (value != null && value < FromDate)
                {
                    System.Windows.MessageBox.Show("thro' date should be greater than from date");
                }
                else
                    mThroDate = value;
                RaisePropertyChanged("ThroDate");
            }
        }


        private bool mThroDateEnable;
        public bool ThroDateEnable
        {
            get { return mThroDateEnable; }
            set
            {
                mThroDateEnable = value;
                RaisePropertyChanged("ThroDateEnable");
            }
        }


        /// <summary>
        /// The m constraint list
        /// </summary>
        private List<Constraint> mConstraintList;
        /// <summary>
        /// Gets or sets the constraint list.
        /// </summary>
        /// <value>
        /// The constraint list.
        /// </value>
        public List<Constraint> ConstraintList
        {
            get { return mConstraintList; }
            set
            {
                mConstraintList = value;
                RaisePropertyChanged("ConstraintList");
            }
        }


        private List<Constraint> mDatedConstraintList;
        /// <summary>
        /// Gets or sets the constraint list.
        /// </summary>
        /// <value>
        /// The constraint list.
        /// </value>
        public List<Constraint> DatedConstraintList
        {
            get { return mDatedConstraintList; }
            set
            {
                mDatedConstraintList = value;
                RaisePropertyChanged("DatedConstraintList");
            }
        }
        //
        public bool ishistoryConstchk { get; set; }

        private List<Constraint> mHistoryConstraintList;
        public List<Constraint> HistoryConstraintList
        {
            get { return mHistoryConstraintList; }
            set
            {
                mHistoryConstraintList = value;
                RaisePropertyChanged("HistoryConstraintList");
            }
        }
        //
        /// <summary>
        /// The m da checked
        /// </summary>
        private bool mDAChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [da checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [da checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DAChecked
        {
            get { return mDAChecked; }
            set
            {
                mDAChecked = value;
                RaisePropertyChanged("DAChecked");
                ConstraintSearchList = null;
                ContingencySearchList = null;
                fillConstraintSearchList(false);
            }
        }

        /// <summary>
        /// The m rt checked
        /// </summary>
        private bool mRTChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [rt checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [rt checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RTChecked
        {
            get { return mRTChecked; }
            set
            {
                mRTChecked = value;
                RaisePropertyChanged("RTChecked");
                SelectedConstraintItem = null;
                SelectedContengencyItem = null;
                ConstraintList = null;
                ConstraintSearchList = null;
                ContingencySearchList = null;
                fillConstraintSearchList(true);
            }
        }

        /// <summary>
        /// The m constraint checked
        /// </summary>
        private bool mConstraintChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [constraint checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [constraint checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ConstraintChecked
        {
            get { return mConstraintChecked; }
            set
            {
                mConstraintChecked = value;
                RaisePropertyChanged("ConstraintChecked");
                ConstraintSearchList = null;
                ContingencySearchList = null;
                fillConstraintSearchList(false);
            }
        }

        /// <summary>
        /// The m contingency checked
        /// </summary>
        private bool mContingencyChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [contingency checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contingency checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ContingencyChecked
        {
            get { return mContingencyChecked; }
            set
            {
                mContingencyChecked = value;
                RaisePropertyChanged("ContingencyChecked");
                ConstraintSearchList = null;
                ContingencySearchList = null;
                fillConstraintSearchList(false);
            }
        }

        /// <summary>
        /// The m date range CheckBox checked
        /// </summary>
        private bool mDateRangeCheckBoxChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [date range CheckBox checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [date range CheckBox checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DateRangeCheckBoxChecked
        {
            get { return mDateRangeCheckBoxChecked; }
            set
            {
                mDateRangeCheckBoxChecked = value;
                if (FromDate > ThroDate)
                {
                    //System.Windows.MessageBox.Show("Resetting the from date");
                    FromDate = ThroDate.AddDays(-1);
                }

                if (value)
                {
                    ThroDateEnable = true;
                }
                else
                {
                    ThroDateEnable = false;
                }

                RaisePropertyChanged("DateRangeCheckBoxChecked");
            }
        }

        /// <summary>
        /// The m is SPP
        /// </summary>
        private bool mIsSpp = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is SPP.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is SPP; otherwise, <c>false</c>.
        /// </value>
        public bool IsSpp
        {
            get
            {
                return mIsSpp;
            }
            set
            {
                mIsSpp = value;
                RaisePropertyChanged("IsSpp");
            }
        }
        List<NodePriceHelper> Lmp5MinsPriceList;

        #endregion

        public MainWindowViewModel(IDataService _dataService = null)
        {
            MarketList = new string[] { "ERCOT", "SystemLambda" };
            MarketSelected = MarketList.FirstOrDefault();
            dataService = _dataService ?? new DataService();
            IsRefreshEnabled = true;
            RTChecked = true;
            ConstraintChecked = true;
            RefreshCommand = new DelegateCommand(() => RefreshList());
            ClearCommand = new DelegateCommand(() => ClearCombos());
            PasteCommand = new DelegateCommand(() => PasteConstraints());
            HourDetailsCommand = new DelegateCommand(() => ConstraintHourDetails());
            ExportFiveMinPricesCmd = new DelegateCommand(() => ExortFiveMinsPrices());
            this.CellClickCmd = new DelegateCommand<object>(this.CellClicked); //RelayCommand(() => CellClicked(SelectedConstraint));

            AllButtonClickCmd = new DelegateCommand(() => AllFrequency());

            RunExportCSVCommand = new DelegateCommand(() => ExportToCSVCommand());

            FTRFIVEMINPRICE = new DelegateCommand(() => GetFTRLMP());
            ALLFIVEMINPRICE = new DelegateCommand(() => GetALLLMP());
            AllErcotShiftFactors = new DelegateCommand(() => GetAllErcotShiftFactors());
            ConstraintHistory = new DelegateCommand(() => GetConstraintHistory());
            ConstraintRTHistory = new DelegateCommand(() => GetConstraintHistory(true, true));
            ConstraintDAHistory = new DelegateCommand(() => GetConstraintHistory(false, true));
        }

        private void GetConstraintHistory(bool rt = false, bool search = false)
        {
            if (MarketSelected == "SystemLambda")
            {
                MessageBox.Show("Not Applicable for System Lambda");
            }

            else
            {
                SelectedConstraintItem = SelectedConstraint.ConstraintText;
                SelectedContengencyItem = SelectedConstraint.ContingencyText;
                ishistoryConstchk = true;
                if(search)
                {
                    if (rt)
                        RefreshList(true, true);
                    else
                        RefreshList(false, true);
                }
                else
                RefreshList();
                ConstraintHistory view = new ConstraintHistory();
                view.DataContext = new ConstraintHistoryViewModel(this, HistoryConstraintList, SelectedConstraint.ConstraintText, SelectedConstraint.ContingencyText);
                view.Title = "Constraint History";
                view.Show();
            }

        }



        private void GetAllErcotShiftFactors()
        {
            if (SelectedConstraint == null)
            {
                MessageBox.Show("Please select a constraint");
                return;
            }
            bool isDA = false;
            if (DAChecked)
                isDA = true;
            else
                isDA = false;
            string constraintName = SelectedConstraint.ConstraintText;
            string contingencyName = SelectedConstraint.ContingencyText;
            DataService ds = new DataService();
            List<SensitivityHelper> tempsensitivityList = ds.GetErcotSensitivities(constraintName, contingencyName, isDA);
            if (tempsensitivityList != null && tempsensitivityList.Count > 0)
            {
                SensitivityHelper firstHelper = tempsensitivityList.First();
                ErcotSensitivities view = new ErcotSensitivities();
                view.DataContext = new ErcotSensitivitiesViewModel(tempsensitivityList, firstHelper.ConstraintId, constraintName, contingencyName, isDA);
                view.Title = "Ercot ShiftFactor";
                view.Show();
            }
            else
            {
                MessageBox.Show("No Sensitivities found for the selected constraint");
            }

        }

        //change by datta
        private void GetFTRLMP()
        {
            try
            {
                DateTime constDate = SelectedConstraint.ConstraintDate.Date;
                List<NodePriceHelper> TempNodePriceList = new List<NodePriceHelper>();
                if (MarketSelected == "ERCOT")
                    TempNodePriceList = dataService.GetFiveMinsPRicesForFTR(constDate, constDate, ColumnSelected.Name.Replace("HE", ""), SelectedConstraint, 9);
                else if (MarketSelected == "SystemLambda")
                {
                    MessageBox.Show("Not Applicable for System Lambda");
                }

                if (TempNodePriceList != null && TempNodePriceList.Count > 0)
                {
                    Lmp5MinsPriceList = TempNodePriceList.ToList();
                    //UptosNodePrices view = new UptosNodePrices();
                    //view.DataContext = new UptosNodePriceViewModel { NodePriceList = TempNodePriceList };
                    //view.Title = "FTR NodePrices";
                    //view.Show();
                }
            }
            catch (Exception ex)
            { }
        }

        private void GetALLLMP()
        {
            try
            {
                DateTime constDate = SelectedConstraint.ConstraintDate.Date;
                List<NodePriceHelper> TempNodePriceList = new List<NodePriceHelper>();
                if (MarketSelected == "ERCOT")
                    TempNodePriceList = dataService.GetFiveMinsPRicesForALL(constDate, constDate, ColumnSelected.Name.Replace("HE", ""), SelectedConstraint, 9);
                else if (MarketSelected == "SystemLambda")
                    MessageBox.Show("Not Applicable for System Lambda");

                if (TempNodePriceList != null && TempNodePriceList.Count > 0)
                {
                    Lmp5MinsPriceList = TempNodePriceList.ToList();
                    //UptosNodePrices view = new UptosNodePrices();
                    //view.DataContext = new UptosNodePriceViewModel { NodePriceList = TempNodePriceList };
                    //view.Title = "Virtual NodePrices";
                    //view.Show();
                }
            }
            catch (Exception ex)
            { }
        }

        //end 

        private void ExortFiveMinsPrices()
        {
            try
            {
                DateTime constDate = SelectedConstraint.ConstraintDate.Date;
                List<NodePriceHelper> TempNodePriceList = new List<NodePriceHelper>();
                if (MarketSelected == "ERCOT")
                    TempNodePriceList = dataService.GetFiveMinsPRicesForUptos(constDate, constDate, ColumnSelected.Name.Replace("HE", ""), SelectedConstraint, 9);
                else if (MarketSelected == "SystemLambda")
                {
                    MessageBox.Show("Not Applicable for System Lambda");
                }


                if (TempNodePriceList != null && TempNodePriceList.Count > 0)
                {
                    Lmp5MinsPriceList = TempNodePriceList.ToList();
                    UptosNodePrice view = new UptosNodePrice();
                    view.DataContext = new UptosNodePriceViewModel { NodePriceList = TempNodePriceList };
                    view.Title = "UPTOS NodePrices";
                    view.Show();
                }
            }
            catch (Exception ex)
            { }
        }




        /// <summary>
        /// Shows the historical constraint for exposure.
        /// </summary>
        /// <param name="ConstraintItem">The constraint item.</param>
        /// <param name="ContingencyItem">The contingency item.</param>
        public void ShowHistoricalConstraintForExposure(string ConstraintItem, string ContingencyItem, int constraintId, int marketKey, bool isDA = false)
        {
            if (isDA) { DAChecked = true; RTChecked = false; }
            else { RTChecked = true; DAChecked = false; }

            bool isResfresh = true;
            if (constraintId != 0)
            {
                DataService ds = new DataService();
                string constraintText = ds.GetConstraintname(constraintId, marketKey, isDA);
                if (constraintText == "")
                {
                    //MessageBox.Show("This constraint dont have History");
                    isResfresh = false;
                }
                else
                {
                    if (marketKey == 9)
                        MarketSelected = "ERCOT";
                    string[] constraintTextArr = constraintText.Split('?');
                    SelectedContengencyItem = constraintTextArr[1];
                    SelectedConstraintItem = constraintTextArr[0];
                }
            }
            else
            {
                SelectedContengencyItem = ConstraintItem;
                SelectedConstraintItem = ContingencyItem;
            }
            if(isResfresh)
            RefreshList();
        }

        public void ShowHistoricalConstraintForMAp(string ConstraintItem, int marketKey, bool isDA = false)
        {
            if (isDA) { DAChecked = true; RTChecked = false; }
            else { RTChecked = true; DAChecked = false; }
            if (ConstraintItem.Count() > 0)
            {
                DataService ds = new DataService();
                List<Constraint> constraintText = ds.GetHistoricalConstraintsForErcotMap(ConstraintItem, true, 9);

                if (marketKey == 9)
                    MarketSelected = "ERCOT";
                ConstraintList = constraintText;
            }
            //RefreshList();
        }

        #region Private Methods

        /// <summary>
        /// Pastes the constraints.
        /// </summary>
        private void PasteConstraints()
        {
            try
            {
                ConstraintPasteHelper PasteConstraint = new ConstraintPasteHelper();

                IDataObject iData = Clipboard.GetDataObject();
                if (!iData.GetDataPresent(DataFormats.Text))
                {
                    return;
                }
                string text = (string)Clipboard.GetData(DataFormats.Text);
                string[] rowData = text.Split('\t');

                PasteConstraint.Constraint = rowData[0];
                SelectedConstraintItem = rowData[0];

                //if (rowData.Length == 1)
                //{
                //PasteConstraint.Constraint = rowData[0];
                //}
                if (rowData.Length > 1)
                {
                    PasteConstraint.Contingency = rowData[1];
                    SelectedContengencyItem = rowData[1];
                }

                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong Data Pasted");
            }
        }
        /// <summary>
        /// Clears the comboboxes.
        /// </summary>
        public void ClearCombos()
        {
            SelectedConstraintItem = null;
            SelectedContengencyItem = null;
            SearchTypedText = string.Empty;
            ConstraintSearchList.Clear();

            if (ContingencySearchList != null)
            {
                ContingencySearchList.Clear();
            }

            RefreshList();

            this.IsRefreshEnabled = true;
            //Mouse.OverrideCursor = Cursors.Arrow;
        }
        /// <summary>
        /// Gets Detailed Constraint Data
        /// </summary>
        private void ConstraintHourDetails()
        {
            if (ColumnSelected != null && SelectedConstraint != null)
            {
                dataService.GetDetailedConstraintData((a, e) =>
                {
                    if (e == null)
                    {
                        DetailedConstraintList = a;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(e.Message);
                    }
                }, ColumnSelected.Name.Replace("HE", ""), SelectedConstraint, GetMarketKey(), MarketSelected);
            }
            if (DetailedConstraintList != null && DetailedConstraintList.Count >= 1)
            {
                ConstraintDetails detailsWindow = new ConstraintDetails();
                detailsWindow.DataContext = new ConstraintDetailsViewModel { ConstraintList = DetailedConstraintList };
                detailsWindow.Show();
            }
            else
            {
                System.Windows.MessageBox.Show("No additional details to show!");
            }
        }
        /// <summary>
        /// Refreshes the list.
        /// </summary>
        private void RefreshList(bool rt= false, bool search=false)
        {
            this.IsRefreshEnabled = false;
            //Mouse.OverrideCursor = Cursors.Wait;
            if ((SelectedConstraintItem == null && SelectedContengencyItem == null) || SelectedConstraintItem == "")
            {
                try
                {

                    bool isDA = false;
                    if (DAChecked)
                        isDA = true;
                    else
                        isDA = false;


                    if (MarketSelected == "SystemLambda")
                    {

                        ConstraintList = dataService.GetEneryPrice(FromDate, ThroDate, isDA);
                        this.IsRefreshEnabled = true;
                    }
                    else
                    {
                        Task.Factory.StartNew(() => GetConstraints(GetMarketKey()));
                        WinterFrequency = 0;
                        SpringFrequency = 0;
                        SummerFrequency = 0;
                        FallFrequency = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                //finally
                //{
                //    Mouse.OverrideCursor = Cursors.Arrow;
                //}

            }
            else
            {
                if (SelectedConstraintItem != null)
                {
                    try
                    {
                        if (SelectedContengencyItem == null)
                        {
                            MessageBox.Show("Please select Contingency");
                            return;
                        }
                        if(search)
                        {
                            if (rt)
                            {
                                GetConstraintHistory(true);
                                GetDateConstraintHistory(FromDate, ThroDate, true);
                            }
                            else
                            {
                                GetConstraintHistory(false);
                                GetDateConstraintHistory(FromDate, ThroDate, false);

                            }

                        }
                        else
                        {
                            if (RTChecked)
                            {
                                GetConstraintHistory(true);
                                GetDateConstraintHistory(FromDate, ThroDate, true);
                            }
                            else if (DAChecked)
                            {
                                GetConstraintHistory(false);
                                GetDateConstraintHistory(FromDate, ThroDate, false);

                            }

                        }
                        //   SelectedConstraintItem = null;
                        //    SelectedContengencyItem = null; 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        this.IsRefreshEnabled = true;
                        //Mouse.OverrideCursor = Cursors.Arrow;
                    }
                }
                else
                {
                    MessageBox.Show("This constraint did not occur. Please check the Constraint name and Contingency name");
                    //return;
                }
            }
        }
        public void ExportToCSVCommand()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        }
        /// <summary>
        /// Exports to CSV threaded.
        /// </summary>
        private void ExportToCSVThreaded()
        {
            if (ConstraintList == null)
            {
                Mouse.OverrideCursor = null;
                return;
            }
            if (ConstraintList == null || ConstraintList.Count == 0)
            {
                System.Windows.MessageBox.Show("No data to export to");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
            dialog.FileName = "HistoricalConstraints_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    if (ConstraintList != null && ConstraintList.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in ConstraintList[0].GetType().GetProperties())
                        {
                            if (item.Name == "ConstraintDate" || item.Name == "ConstraintText" || item.Name == "MonitoredFacility" || item.Name == "ContingencyText"
                                || item.Name == "MaxLoad" || item.Name == "MaxShadowPrice" || item.Name == "MaxRTShadowPrice" || item.Name == "SourceZone" || item.Name == "SinkZone"
                                || item.Name == "Avg" || item.Name == "HE1" || item.Name == "HE2" || item.Name == "HE3" || item.Name == "HE4" || item.Name == "HE5"
                                || item.Name == "HE6" || item.Name == "HE7" || item.Name == "HE8" || item.Name == "HE9" || item.Name == "HE10" || item.Name == "HE11"
                                || item.Name == "HE12" || item.Name == "HE13" || item.Name == "HE14" || item.Name == "HE15" || item.Name == "HE16" || item.Name == "HE17"
                                || item.Name == "HE18" || item.Name == "HE19" || item.Name == "HE20" || item.Name == "HE21" || item.Name == "HE22" || item.Name == "HE23" || item.Name == "HE24")
                            {
                                builder.Append(item.Name + ",");
                            }
                        }
                        builder.AppendLine();
                        foreach (var item in ConstraintList)
                        {
                            foreach (var propItem in item.GetType().GetProperties())
                            {
                                if (propItem.Name == "ConstraintDate" || propItem.Name == "ConstraintText" || propItem.Name == "MonitoredFacility" || propItem.Name == "ContingencyText"
                                || propItem.Name == "MaxLoad" || propItem.Name == "MaxShadowPrice" || propItem.Name == "MaxRTShadowPrice" || propItem.Name == "SourceZone" || propItem.Name == "SinkZone"
                                || propItem.Name == "Price" || propItem.Name == "HE1" || propItem.Name == "HE2" || propItem.Name == "HE3" || propItem.Name == "HE4" || propItem.Name == "HE5"
                                || propItem.Name == "HE6" || propItem.Name == "HE7" || propItem.Name == "HE8" || propItem.Name == "HE9" || propItem.Name == "HE10" || propItem.Name == "HE11"
                                || propItem.Name == "HE12" || propItem.Name == "HE13" || propItem.Name == "HE14" || propItem.Name == "HE15" || propItem.Name == "HE16" || propItem.Name == "HE17"
                                || propItem.Name == "HE18" || propItem.Name == "HE19" || propItem.Name == "HE20" || propItem.Name == "HE21" || propItem.Name == "HE22" || propItem.Name == "HE23" || propItem.Name == "HE24")
                                {
                                    if (propItem.Name == "Price" || propItem.Name == "HE1" || propItem.Name == "HE2" || propItem.Name == "HE3" || propItem.Name == "HE4" || propItem.Name == "HE5"
                                    || propItem.Name == "HE6" || propItem.Name == "HE7" || propItem.Name == "HE8" || propItem.Name == "HE9" || propItem.Name == "HE10" || propItem.Name == "HE11"
                                    || propItem.Name == "HE12" || propItem.Name == "HE13" || propItem.Name == "HE14" || propItem.Name == "HE15" || propItem.Name == "HE16" || propItem.Name == "HE17"
                                    || propItem.Name == "HE18" || propItem.Name == "HE19" || propItem.Name == "HE20" || propItem.Name == "HE21" || propItem.Name == "HE22" || propItem.Name == "HE23" || propItem.Name == "HE24")
                                    {
                                        object st = propItem.GetValue(item);
                                        double s = Convert.ToDouble(st);
                                        double val = Math.Round(s, 2);
                                        string g = "";
                                        if (val == 0.0)
                                        {
                                            g = val.ToString();
                                            g = "";
                                            builder.Append(g + ",");
                                        }
                                        else
                                        {
                                            builder.Append(val + ",");
                                        }

                                    }
                                    else
                                    {
                                        builder.Append(propItem.GetValue(item) + ",");
                                    }
                                }
                            }
                            builder.AppendLine();
                        }
                        if (builder.Length > 0)
                        {
                            using (TextWriter str = new StreamWriter(dialog.FileName, false))
                            {
                                str.Write(builder.ToString());
                                str.Flush();
                                str.Close();
                                str.Dispose();
                            }
                            if (File.Exists(dialog.FileName))
                            {
                                MessageBox.Show("Successfully saved the file " + dialog.FileName);
                            }
                            else
                            {
                                MessageBox.Show("Could not save the file");
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Gets constraints data.
        /// </summary>
        /// <param name="p">The p.</param>
        public void GetConstraints(int p)
        {
            ConstraintList = new List<Constraint>();
            dataService = dataService ?? new DataService();
            dataService.GetConstraintData((a, e) =>
            {
                if (e == null)
                {
                    ConstraintList = a;
                    var seasonConstraintFrequency = ConstraintList
                          .GroupBy(data =>
                          {
                              if (data.ConstraintDate.Month >= 12 || data.ConstraintDate.Month <= 2)
                                  return "Winter";
                              else if (data.ConstraintDate.Month >= 3 && data.ConstraintDate.Month <= 5)
                                  return "Spring";
                              else if (data.ConstraintDate.Month >= 6 && data.ConstraintDate.Month <= 8)
                                  return "Summer";
                              else
                                  return "Fall";
                          })
                          .ToDictionary(
                              group => group.Key,
                              group => group
                                  .GroupBy(constraint => constraint.ConstraintText)
                                  .ToDictionary(constraintGroup => constraintGroup.Key, constraintGroup => constraintGroup.Count()) // Count occurrences of each constraint
                          );


                    foreach (var constraint in ConstraintList)
                    {

                        if (seasonConstraintFrequency.ContainsKey("Winter") && seasonConstraintFrequency["Winter"].ContainsKey(constraint.ConstraintText))
                            constraint.WinterFrequency = seasonConstraintFrequency["Winter"][constraint.ConstraintText];
                        else
                            constraint.WinterFrequency = 0;


                        if (seasonConstraintFrequency.ContainsKey("Spring") && seasonConstraintFrequency["Spring"].ContainsKey(constraint.ConstraintText))
                            constraint.SpringFrequency = seasonConstraintFrequency["Spring"][constraint.ConstraintText];
                        else
                            constraint.SpringFrequency = 0;


                        if (seasonConstraintFrequency.ContainsKey("Summer") && seasonConstraintFrequency["Summer"].ContainsKey(constraint.ConstraintText))
                            constraint.SummerFrequency = seasonConstraintFrequency["Summer"][constraint.ConstraintText];
                        else
                            constraint.SummerFrequency = 0;


                        if (seasonConstraintFrequency.ContainsKey("Fall") && seasonConstraintFrequency["Fall"].ContainsKey(constraint.ConstraintText))
                            constraint.FallFrequency = seasonConstraintFrequency["Fall"][constraint.ConstraintText];
                        else
                            constraint.FallFrequency = 0;
                    }

                }
                else
                {
                    MessageBox.Show(e.Message);
                }
                this.IsRefreshEnabled = true;
            }, p, DAChecked, FromDate, DateRangeCheckBoxChecked ? ThroDate : (DateTime?)null);
        }

        private void fillConstraintSearchList(bool isRt)
        {
            DataService ds = new DataService();
            SearchTypedText = "";

            if (ConstraintChecked)
            {
                if (RTChecked)
                {
                    ConstraintSearchList = ds.FillConstraintList(true, GetMarketKey(), true);
                }
                else if (DAChecked)
                {
                    ConstraintSearchList = ds.FillConstraintList(false, GetMarketKey(), true);
                }
            }
            else if (ContingencyChecked)
            {
                if (RTChecked)
                {
                    ConstraintSearchList = ds.FillConstraintList(true, GetMarketKey(), false);
                }
                else if (DAChecked)
                {
                    ConstraintSearchList = ds.FillConstraintList(false, GetMarketKey(), false);
                }
            }
        }

        private void GetAllContingencies(string constraintname, bool isRt)
        {
            DataService ds = new DataService();
            ContingencySearchList = ds.GetAllContingencies(constraintname, isRt, GetMarketKey());
        }
        private void GetAllConstraints(string contingencyname, bool isRt)
        {
            DataService ds = new DataService();
            ContingencySearchList = ds.GetAllConstraints(contingencyname, isRt, GetMarketKey());
        }

        public void GetDateConstraintHistory(DateTime Fromdate, DateTime Todate, bool isRt)
        {

            DatedConstraintList = new List<Constraint>();


        }
        private void GetConstraintHistory(bool isRt)
        {
            try
            {
                // isConstraintChecked = true;
                DataService ds = new DataService();
                if (ishistoryConstchk)
                {
                    HistoryConstraintList = null;
                    if (ConstraintChecked)
                    {
                        HistoryConstraintList = ds.GetAllHistoricalConstraintsData(SelectedConstraintItem, SelectedContengencyItem, isRt, GetMarketKey());

                    }
                    else if (ContingencyChecked)
                    {
                        HistoryConstraintList = ds.GetAllHistoricalContingencyData(SelectedConstraintItem, SelectedContengencyItem, isRt, GetMarketKey());

                    }
                    if (HistoryConstraintList.Count == 0)
                    {
                        MessageBox.Show("This constraint did not occur. Please check the Constraintname and Contingencyname");
                    }
                    ishistoryConstchk = false;
                }
                else
                {
                    ConstraintList = null;
                    if (ConstraintChecked)
                    {
                        ConstraintList = ds.GetAllHistoricalConstraintsData(SelectedConstraintItem, SelectedContengencyItem, isRt, GetMarketKey());

                    }
                    else if (ContingencyChecked)
                    {
                        ConstraintList = ds.GetAllHistoricalContingencyData(SelectedContengencyItem, SelectedConstraintItem, isRt, GetMarketKey());

                    }
                    if (ThroDateEnable == true)
                    {
                        var seasonConstraintFrequency = ConstraintList
                        .GroupBy(data =>
                        {
                            if (data.ConstraintDate.Month >= 12 || data.ConstraintDate.Month <= 2)
                                return "Winter";
                            else if (data.ConstraintDate.Month >= 3 && data.ConstraintDate.Month <= 5)
                                return "Spring";
                            else if (data.ConstraintDate.Month >= 6 && data.ConstraintDate.Month <= 8)
                                return "Summer";
                            else
                                return "Fall";
                        })
                        .ToDictionary(group => group.Key, group => group.Count());

                        // Assign the season constraint frequency to each constraint
                        foreach (var constraint in ConstraintList)
                        {
                            if (seasonConstraintFrequency.ContainsKey("Winter"))
                                constraint.WinterFrequency = seasonConstraintFrequency["Winter"];
                            if (seasonConstraintFrequency.ContainsKey("Spring"))
                                constraint.SpringFrequency = seasonConstraintFrequency["Spring"];
                            if (seasonConstraintFrequency.ContainsKey("Summer"))
                                constraint.SummerFrequency = seasonConstraintFrequency["Summer"];
                            if (seasonConstraintFrequency.ContainsKey("Fall"))
                                constraint.FallFrequency = seasonConstraintFrequency["Fall"];
                        }
                    }
                    if (ConstraintList.Count == 0)
                    {
                        MessageBox.Show("This constraint did not occur. Please check the Constraintname and Contingencyname");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        //private void myDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    // Ensure a single item is selected
        //    if (constraintDataGrid.SelectedItem != null)
        //    {
        //        // Get the selected item
        //        Constraint selectedEmployee = constraintDataGrid.SelectedItem as Constraint;

        //        // Pass the selected item to YourFunction for processing
        //     //   YourFunction(selectedEmployee);
        //    }
        //}

        public void CellClicked(object parameter)
        {
            if (ThroDateEnable == true)
            {
                if (parameter is Constraint selectedItem)
                {
                    DatedConstraintList = new DataService().GetAllHistoricalConstraintsDataforDateRange(FromDate, ThroDate, selectedItem.ConstraintText, selectedItem.ContingencyText, RTChecked, 9);

                    // Calculate frequency for each season
                    var selectedConstraintFrequency = DatedConstraintList
                   .Where(data => data.ConstraintText == selectedItem.ConstraintText) // Filter by constraint text
                   .GroupBy(data =>
                   {
                       if (data.ConstraintDate.Month >= 12 || data.ConstraintDate.Month <= 2)
                           return "Winter";
                       else if (data.ConstraintDate.Month >= 3 && data.ConstraintDate.Month <= 5)
                           return "Spring";
                       else if (data.ConstraintDate.Month >= 6 && data.ConstraintDate.Month <= 8)
                           return "Summer";
                       else
                           return "Fall";
                   })
                 .ToDictionary(group => group.Key, group => group.Count());

                    // Update the selected item with the calculated frequencies
                    selectedItem.WinterFrequency = selectedConstraintFrequency.ContainsKey("Winter") ? selectedConstraintFrequency["Winter"] : 0;
                    selectedItem.SpringFrequency = selectedConstraintFrequency.ContainsKey("Spring") ? selectedConstraintFrequency["Spring"] : 0;
                    selectedItem.SummerFrequency = selectedConstraintFrequency.ContainsKey("Summer") ? selectedConstraintFrequency["Summer"] : 0;
                    selectedItem.FallFrequency = selectedConstraintFrequency.ContainsKey("Fall") ? selectedConstraintFrequency["Fall"] : 0;

                    // Optionally, update the entire ConstraintList with the calculated frequencies
                    // This assumes that ConstraintList is a property of your ViewModel
                    foreach (var constraint in ConstraintList)
                    {
                        WinterFrequency = selectedConstraintFrequency.ContainsKey("Winter") ? selectedConstraintFrequency["Winter"] : 0;
                        SpringFrequency = selectedConstraintFrequency.ContainsKey("Spring") ? selectedConstraintFrequency["Spring"] : 0;
                        SummerFrequency = selectedConstraintFrequency.ContainsKey("Summer") ? selectedConstraintFrequency["Summer"] : 0;
                        FallFrequency = selectedConstraintFrequency.ContainsKey("Fall") ? selectedConstraintFrequency["Fall"] : 0;
                    }
                }
            }
        }
        private void AllFrequency()
        {
            if (ThroDateEnable == true)
            {
                var selectedConstraintFrequency = ConstraintList
                  // Filter by constraint text
                  .GroupBy(data =>
                  {
                      if (data.ConstraintDate.Month >= 12 || data.ConstraintDate.Month <= 2)
                          return "Winter";
                      else if (data.ConstraintDate.Month >= 3 && data.ConstraintDate.Month <= 5)
                          return "Spring";
                      else if (data.ConstraintDate.Month >= 6 && data.ConstraintDate.Month <= 8)
                          return "Summer";
                      else
                          return "Fall";
                  })
                .ToDictionary(group => group.Key, group => group.Count());

                // Update the selected item with the calculated frequencies
                WinterFrequency = selectedConstraintFrequency.ContainsKey("Winter") ? selectedConstraintFrequency["Winter"] : 0;
                SpringFrequency = selectedConstraintFrequency.ContainsKey("Spring") ? selectedConstraintFrequency["Spring"] : 0;
                SummerFrequency = selectedConstraintFrequency.ContainsKey("Summer") ? selectedConstraintFrequency["Summer"] : 0;
                FallFrequency = selectedConstraintFrequency.ContainsKey("Fall") ? selectedConstraintFrequency["Fall"] : 0;

                // Optionally, update the entire ConstraintList with the calculated frequencies
                // This assumes that ConstraintList is a property of your ViewModel
                foreach (var constraint in ConstraintList)
                {
                    WinterFrequency = selectedConstraintFrequency.ContainsKey("Winter") ? selectedConstraintFrequency["Winter"] : 0;
                    SpringFrequency = selectedConstraintFrequency.ContainsKey("Spring") ? selectedConstraintFrequency["Spring"] : 0;
                    SummerFrequency = selectedConstraintFrequency.ContainsKey("Summer") ? selectedConstraintFrequency["Summer"] : 0;
                    FallFrequency = selectedConstraintFrequency.ContainsKey("Fall") ? selectedConstraintFrequency["Fall"] : 0;
                }
            }
        }


        public int GetMarketKey()
        {
            switch (MarketSelected)
            {

                case "ERCOT": return 9;

                case "SystemLambda": return 9; ;

                default: return 0;
            }
        }

        #endregion
    }
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// The converter
        /// </summary>
        private BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter.Convert(value, targetType, parameter, culture) as Visibility?;
            return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts back the value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter.ConvertBack(value, targetType, parameter, culture) as bool?;
            return result == true ? false : true;
        }
    }
    public class ConstraintPasteHelper
    {
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Constraint { get; set; }
        /// <summary>
        /// Gets or sets the contingency.
        /// </summary>
        /// <value>
        /// The contingency.
        /// </value>
        public string Contingency { get; set; }
    }

    public class SensitivityHelper
    {
        public int ConstraintId { get; set; }
        public string Constraint { get; set; }
        public string Contingency { get; set; }
        public string NodeName { get; set; }

        public int NodeKey { get; set; }
        public string Source { get; set; }
        public double Sensitivity { get; set; }
        public string Zone { get; set; }
    }
}
