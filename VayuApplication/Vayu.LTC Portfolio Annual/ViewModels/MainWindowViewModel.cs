using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.CRRCalculationLibrary;
using Vayu.CRRCreditLibrary;
using Vayu.CRRSubmissionLibrary;
using Vayu.DBLibrary;
using Vayu.LTC_PortfolioAnnual.Model;
using Vayu.LTC_PortfolioAnnual.Views;
using Vayu.NodePriceLibrary;

namespace Vayu.LTC_PortfolioAnnual.ViewModels
{
    public enum SummaryRowType
    {
        /// <summary>
        /// The total
        /// </summary>
        Total,
        /// <summary>
        /// The maximum
        /// </summary>
        Max,
        /// <summary>
        /// The minimum
        /// </summary>
        Min,
        /// <summary>
        /// The average
        /// </summary>
        Avg,
        /// <summary>
        /// The win
        /// </summary>
        Win
    }

    public class MainWindowViewModel : BindableBase
    {
        #region Declaration
        /// <summary>
        /// The data service
        /// </summary>
        private readonly IDataService _dataService;
        /// <summary>
        /// The m CRR calculation proxy
        /// </summary>
        private ICRRCredit mCRRCalculationCreditProxy = null;
        private IBidCRRSubmit mCRRCalculationProxy = null;
        private string mEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetCreditCRRService();
        private string mEndPoint1 = Vayu.CommonAccessLibrary.ServiceConnections.GetFTRService();
        private string mEndPointCRR = Vayu.CommonAccessLibrary.ServiceConnections.GetCRRSubmissionService();
        /// <summary>
        /// The m fill portfolio list
        /// </summary>
        private List<Portfolio> mFillPortfolioList = new List<Portfolio>();
        /// <summary>
        /// The m portfolio hash
        /// </summary>
        private Dictionary<int, Portfolio> mPortfolioHash = new Dictionary<int, Portfolio>();
        /// <summary>
        /// The m bid identifier
        /// </summary>
        private int mBidId;
        public DelegateCommand RunPathDetailsCommand { private set; get; }
        public DelegateCommand ClickPathMWsCommand { private set; get; }

        public DelegateCommand PieChartCommand { private set; get; }

        List<PathHelper> ValidCRRPathList = new List<PathHelper>();
        Dictionary<long, long> sNodeHash = new Dictionary<long, long>();
        public Dictionary<long, string> validCRRNodeDict = new Dictionary<long, string>();

        List<PathHelper> ValidCrrPathList = new List<PathHelper>();
        Dictionary<string, long> sNodeHashCRR = new Dictionary<string, long>();
        public Dictionary<long, string> validCrrNodeDict = new Dictionary<long, string>();
        Dictionary<DateTime, string> dictPeakYn_datewise;
        // Dictionary<string, string> RTDAMinDates = new Dictionary<string, string>();
        Dictionary<string, string> SourceRTDAMinDates = new Dictionary<string, string>();
        Dictionary<string, string> SinkRTDAMinDates = new Dictionary<string, string>();

        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName;// "user1"; //
        //private string predicted = string.Empty;
        //private string mUser = "user1";
        /// <summary>
        /// The m as bid market date time hash
        /// </summary>
        private Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mAsBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
        /// <summary>
        /// The m must take market date time hash
        /// </summary>
        private Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mMustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
        /// <summary>
        /// The m as bid daily market date time hash
        /// </summary>
        private Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mAsBidDailyMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
        /// <summary>
        /// The m must take daily market date time hash
        /// </summary>
        private Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mMustTakeDailyMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
        /// <summary>
        /// The m hourly pivot hash
        /// </summary>
        private Dictionary<string, List<Node>> mHourlyPivotHash = new Dictionary<string, List<Node>>();
        /// <summary>
        /// The temporary cost hash
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> tempCostHash = new Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>>();
        Dictionary<DateTime, Dictionary<string, Cost>> tempOptionCostHash = new Dictionary<DateTime, Dictionary<string, Cost>>();

        private Dictionary<string, List<Exposure>> mPathDetailHash = new Dictionary<string, List<Exposure>>();

        #endregion

        #region Properties

        #region Relay Command Properties

        /// <summary>
        /// Gets or sets the add portfolio command.
        /// </summary>
        /// <value>
        /// The add portfolio command.
        /// </value>
        public DelegateCommand AddPortfolioCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove portfolio command.
        /// </summary>
        /// <value>
        /// The remove portfolio command.
        /// </value>
        public DelegateCommand RemovePortfolioCommand { private set; get; }
        /// <summary>
        /// Gets or sets the import command.
        /// </summary>
        /// <value>
        /// The import command.
        /// </value>
        public DelegateCommand ImportCommand { private set; get; }
        /// <summary>
        /// Gets or sets the retrieve command.
        /// </summary>
        /// <value>
        /// The retrieve command.
        /// </value>
        public DelegateCommand RetrieveCommand { private set; get; }
        /// <summary>
        /// Gets or sets the submit command.
        /// </summary>
        /// <value>
        /// The submit command.
        /// </value>
        public DelegateCommand SubmitCommand { private set; get; }
        public DelegateCommand SeasonCommand { private set; get; }

        public DelegateCommand RunRetrieveGoCommand { private set; get; }
        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public DelegateCommand CancelCommand { private set; get; }
        /// <summary>
        /// Gets or sets the credit command.
        /// </summary>
        /// <value>
        /// The credit command.
        /// </value>
        public DelegateCommand CreditCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        public DelegateCommand DeleteCommand { private set; get; }
        /// <summary>
        /// Gets or sets the move CRR.
        /// </summary>
        /// <value>
        /// The move CRR.
        /// </value>
        /// 

        public DelegateCommand AllCommand { get; set; }
        /// <summary>
        /// Gets or sets the none click.
        /// </summary>
        /// <value>
        /// The none click.
        /// </value>
        public DelegateCommand NoneClick { get; set; }
        /// <summary>
        /// Gets or sets the winter command.
        /// </summary>
        /// <value>
        /// The winter command.
        /// </value>
        public DelegateCommand WinterCommand { get; set; }
        /// <summary>
        /// Gets or sets the spring command.
        /// </summary>
        /// <value>
        /// The spring command.
        /// </value>
        public DelegateCommand SpringCommand { get; set; }

        public DelegateCommand RunHistoricalConstOpenCmd { private set; get; }
        /// <summary>
        /// Gets or sets the summer command.
        /// </summary>
        /// <value>
        /// The summer command.
        /// </value>
        public DelegateCommand SummerCommand { get; set; }
        /// <summary>
        /// Gets or sets the fall command.
        /// </summary>
        /// <value>
        /// The fall command.
        /// </value>
        public DelegateCommand FallCommand { get; set; }
        /// <summary>
        /// Gets or sets the run retrieve fetch data and update chart command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data and update chart command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataAndUpdateChartCommand { private set; get; }
        /// <summary>
        /// Gets or sets the create submission file command.
        /// </summary>
        /// <value>
        /// The create submission file command.
        /// </value>
        public DelegateCommand CreateSubmissionFileCommand { private set; get; }

        public DelegateCommand CalcluatePathwistStatCmd { private set; get; }


        public DelegateCommand PFAnalyser_CalcluateCommond { private set; get; }


        public DelegateCommand ExportCommand { private set; get; }
        public DelegateCommand ExportButtonCommand { private set; get; }


        public DelegateCommand PFAnalyser_ExportCommand { private set; get; }

        public DelegateCommand MonthlyAnalysis_ExportCommand { private set; get; }

        public DelegateCommand Calculate_MonthlyAnalysis { private set; get; }

        #endregion
        Dictionary<DateTime, PNL> mMonthlyDartHashg = new Dictionary<DateTime, PNL>();
        Dictionary<DateTime, PNL> mdailyDARTHAshg = new Dictionary<DateTime, PNL>();
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
                RaisePropertyChanged("JanChecked");
            }
        }
        private bool mRTExpChecked = true;
        public bool RTExpChecked
        {
            get
            {
                return mRTExpChecked;
            }
            set
            {
                mRTExpChecked = value;
                RaisePropertyChanged("RTExpChecked");
            }
        }
        private bool mDAExpChecked;
        public bool DAExpChecked
        {
            get
            {
                return mDAExpChecked;
            }
            set
            {
                mDAExpChecked = value;
                RaisePropertyChanged("DAExpChecked");
            }
        }
        private bool misDAChecked;

        private string mDaysChanged;

        public string DaysChanged
        {
            get
            {
                return mDaysChanged;
            }
            set
            {
                mDaysChanged = value;
                int days = 0;
                try
                {
                    days = Int16.Parse(mDaysChanged);
                }
                catch (Exception ex)
                {
                }
                EndDate = DateTime.Today;
                StartDate = DateTime.Today.AddDays(-days);
            }
        }
        public bool isDAChecked
        {
            get { return misDAChecked; }
            set
            {
                misDAChecked = value;
                RaisePropertyChanged("isDAChecked");
                PlotAreaGraph();

            }
        }
        private bool misCostChecked;

        public bool isCostChecked
        {
            get { return misCostChecked; }
            set
            {
                misCostChecked = value;
                RaisePropertyChanged("isCostChecked");
                PlotAreaGraph();

            }
        }
        private bool misDARTChecked;
        public bool isDARTChecked
        {
            get { return misDARTChecked; }
            set
            {
                misDARTChecked = value;
                RaisePropertyChanged("isDARTChecked");
                PlotAreaGraph();
            }
        }
        private bool _NextMonthOutageConstraintChk = false;
        public bool NextMonthOutageConstraintChk
        {
            get { return _NextMonthOutageConstraintChk; }
            set
            {
                _NextMonthOutageConstraintChk = value;
                if (value == true)
                {
                    Predicted = "NEXTMONTH";
                    grpMonth = false;
                    OnGoingOutageConstraintChk = false;
                    ExposureMonthChk = false;
                    BindingConstraintsChk = false;
                    GoCommand();
                }
                RaisePropertyChanged("NextMonthOutageConstraintChk");
            }
        }

        private DateTime _StatStartDateSelected;

        public DateTime StatStartDateSelected
        {
            get { return _StatStartDateSelected; }
            set
            {
                _StatStartDateSelected = value;
                RaisePropertyChanged("StatStartDateSelected");
            }
        }



        private DateTime _PFAnalyserStartDateSelected;

        public DateTime PFAnalyserStartDateSelected
        {
            get { return _PFAnalyserStartDateSelected; }
            set
            {
                _PFAnalyserStartDateSelected = value;
                RaisePropertyChanged("PFAnalyserStartDateSelected");
            }
        }



        private DateTime _PFAnalyserEndDateSelected;

        public DateTime PFAnalyserEndDateSelected
        {
            get { return _PFAnalyserEndDateSelected; }
            set
            {
                _PFAnalyserEndDateSelected = value;
                RaisePropertyChanged("PFAnalyserEndDateSelected");
            }
        }



        private List<PathwiseCalculationHelper> _PathwistStatList;

        public List<PathwiseCalculationHelper> PathwistStatList
        {
            get { return _PathwistStatList; }
            set
            {
                _PathwistStatList = value;
                RaisePropertyChanged("PathwistStatList");
            }
        }

        private DateTime _StatEndDateSelected;

        public DateTime StatEndDateSelected
        {
            get { return _StatEndDateSelected; }
            set
            {
                _StatEndDateSelected = value;
                RaisePropertyChanged("StatEndDateSelected");
            }
        }




        private bool _OnGoingOutageConstraintChk = false;
        public bool OnGoingOutageConstraintChk
        {
            get { return _OnGoingOutageConstraintChk; }
            set
            {
                _OnGoingOutageConstraintChk = value;
                if (value == true)
                {
                    Predicted = "ONGOING";
                    grpMonth = false;
                    NextMonthOutageConstraintChk = false;
                    ExposureMonthChk = false;
                    BindingConstraintsChk = false;
                    GoCommand();
                }
                RaisePropertyChanged("OnGoingOutageConstraintChk");
            }
        }


        private bool _ExposureMonthChk = true;
        public bool ExposureMonthChk
        {
            get { return _ExposureMonthChk; }
            set
            {
                _ExposureMonthChk = value;
                if (value == true)
                {
                    Predicted = "MONTH";
                    grpMonth = false;
                    NextMonthOutageConstraintChk = false;
                    OnGoingOutageConstraintChk = false;
                    BindingConstraintsChk = false;
                    GoCommand();
                }
                RaisePropertyChanged("ExposureMonthChk");
            }
        }

        private bool _BindingConstraintsChk;

        public bool BindingConstraintsChk
        {
            get { return _BindingConstraintsChk; }
            set
            {
                _BindingConstraintsChk = value;
                if (value == true)
                {
                    grpMonth = true;
                    Predicted = "BINDING";
                    NextMonthOutageConstraintChk = false;
                    OnGoingOutageConstraintChk = false;
                    ExposureMonthChk = false;
                    GoCommand();
                }
                RaisePropertyChanged("BindingConstraintsChk");
            }
        }


        /// <summary>
        /// The m feb checked
        /// </summary>
        private List<Exposure> mExposureList;
        /// <summary>
        /// Gets or sets the exposure list.
        /// </summary>
        /// <value>
        /// The exposure list.
        /// </value>
        public List<Exposure> ExposureList
        {
            get
            {
                return mExposureList;
            }
            set
            {
                mExposureList = value;
                RaisePropertyChanged("ExposureList");
            }
        }
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
                RaisePropertyChanged("FebChecked");
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
                RaisePropertyChanged("MarChecked");
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
                RaisePropertyChanged("AprChecked");
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
                RaisePropertyChanged("MayChecked");
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
                RaisePropertyChanged("JunChecked");
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
                RaisePropertyChanged("JulChecked");
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
                RaisePropertyChanged("AugChecked");
            }
        }
        private bool _SortMWChecked;

        public bool SortMWChecked
        {
            get { return _SortMWChecked; }
            set
            {
                _SortMWChecked = value;
                RaisePropertyChanged("SortMWChecked");
            }
        }

        private bool _SortDollarChecked;

        public bool SortDollarChecked
        {
            get { return _SortDollarChecked; }
            set
            {
                _SortDollarChecked = value;
                RaisePropertyChanged("SortDollarChecked");
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
                RaisePropertyChanged("SepChecked");
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
                RaisePropertyChanged("OctChecked");
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
                RaisePropertyChanged("NovChecked");
            }
        }
        /// <summary>
        /// The m decimal checked
        /// </summary>
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
                RaisePropertyChanged("DecChecked");
            }
        }

        /// <summary>
        /// The count text
        /// </summary>
        private double? countText;
        /// <summary>
        /// Gets or sets the count text.
        /// </summary>
        /// <value>
        /// The count text.
        /// </value>
        public double? CountText
        {
            get { return countText; }
            set { countText = value; RaisePropertyChanged("CountText"); }
        }

        /// <summary>
        /// The m spread heat map factors above
        /// </summary>
        private double[] mSpreadHeatMapFactorsAbove;
        /// <summary>
        /// Gets or sets the spread heat map factors above.
        /// </summary>
        /// <value>
        /// The spread heat map factors above.
        /// </value>
        public double[] SpreadHeatMapFactorsAbove
        {
            get
            {
                return mSpreadHeatMapFactorsAbove;
            }
            set
            {
                mSpreadHeatMapFactorsAbove = value;
                RaisePropertyChanged("SpreadHeatMapFactorsAbove");
            }
        }
        /// <summary>
        /// The m spread heat map factors below
        /// </summary>
        private double[] mSpreadHeatMapFactorsBelow;
        /// <summary>
        /// Gets or sets the spread heat map factors below.
        /// </summary>
        /// <value>
        /// The spread heat map factors below.
        /// </value>
        public double[] SpreadHeatMapFactorsBelow
        {
            get
            {
                return mSpreadHeatMapFactorsBelow;
            }
            set
            {
                mSpreadHeatMapFactorsBelow = value;
                RaisePropertyChanged("SpreadHeatMapFactorsBelow");
            }
        }

        private HourlyPivotData mSelectedHourlyPivot;

        public HourlyPivotData SelectedHourlyPivot
        {
            get
            {
                return mSelectedHourlyPivot;
            }
            set
            {
                mSelectedHourlyPivot = value;
                FetchAllPortfolioData(true, MustTakeChecked);
                RaisePropertyChanged("SelectedHourlyPivot");
            }
        }

        /// <summary>
        /// The m market combo selected value
        /// </summary>
        private string mMarketComboSelectedValue;
        /// <summary>
        /// Gets or sets the market combo selected value.
        /// </summary>
        /// <value>
        /// The market combo selected value.
        /// </value>
        public string MarketComboSelectedValue
        {
            get
            {
                return mMarketComboSelectedValue;
            }
            set
            {
                mMarketComboSelectedValue = value;
                RemovePortfolio();
                SetAuctionList();
                SetUserPortfolioList();
                RaisePropertyChanged("MarketComboSelectedValue");
            }
        }
        /// <summary>
        /// The m iso market list
        /// </summary>
        private List<string> mISOMarketList;
        /// <summary>
        /// Gets or sets the iso market list.
        /// </summary>
        /// <value>
        /// The iso market list.
        /// </value>
        public List<string> ISOMarketList
        {
            get
            {
                return mISOMarketList;
            }
            set
            {
                mISOMarketList = value;
                RaisePropertyChanged("ISOMarketList");
            }
        }
        /// <summary>
        /// The m show valid checked
        /// </summary>
        private bool mShowValidChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show valid checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show valid checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowValidChecked
        {
            get
            {
                return mShowValidChecked;
            }
            set
            {
                mShowValidChecked = value;
                Retrieve();
                RaisePropertyChanged("ShowValidChecked");
            }
        }
        /// <summary>
        /// The m show new portfolio checked
        /// </summary>
        private bool mShowNewPortfolioChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show new portfolio checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show new portfolio checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowNewPortfolioChecked
        {
            get
            {
                return mShowNewPortfolioChecked;
            }
            set
            {
                mShowNewPortfolioChecked = value;
                if (!ShowNewPortfolioChecked)
                {
                    AuctionPortfolioComboList = null;
                    SetAuctionList();
                }
                if (ShowNewPortfolioChecked)
                {
                    SetNewAuctionPortfolioList();
                }
                RaisePropertyChanged("ShowNewPortfolioChecked");
            }
        }
        private bool mExternalPorfolios;
        public bool ExternalPorfolios
        {
            get
            {
                return mExternalPorfolios;
            }
            set
            {
                mExternalPorfolios = value;
                if (ExternalPorfolios)
                {
                    SetAuctionList();
                    SetExternalPortfolioList();
                }
                if (!ExternalPorfolios)
                {
                    SetAuctionList();
                    PortfolioComboList = null;
                }

                RaisePropertyChanged("ExternalPorfolios");
            }
        }

        /// <summary>
        /// The m filter day comparison source checked
        /// </summary>
        private bool mFilterDayComparisonSourceChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison source checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison source checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonSourceChecked
        {
            get
            {
                return mFilterDayComparisonSourceChecked;
            }
            set
            {
                mFilterDayComparisonSourceChecked = value;
                RaisePropertyChanged("FilterDayComparisonSourceChecked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m filter day comparison sink checked
        /// </summary>
        private bool mFilterDayComparisonSinkChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison sink checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison sink checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonSinkChecked
        {
            get
            {
                return mFilterDayComparisonSinkChecked;
            }
            set
            {
                mFilterDayComparisonSinkChecked = value;
                RaisePropertyChanged("FilterDayComparisonSinkChecked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m spread combo selected value
        /// </summary>
        private string mSpreadComboSelectedValue;
        /// <summary>
        /// Gets or sets the spread combo selected value.
        /// </summary>
        /// <value>
        /// The spread combo selected value.
        /// </value>
        public string SpreadComboSelectedValue
        {
            get
            {
                return mSpreadComboSelectedValue;
            }
            set
            {
                mSpreadComboSelectedValue = value;
                RaisePropertyChanged("SpreadComboSelectedValue");
            }
        }
        /// <summary>
        /// The m filter day comparison da checked
        /// </summary>
        private bool mFilterDayComparisonDAChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison da checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison da checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonDAChecked
        {
            get
            {
                return mFilterDayComparisonDAChecked;
            }
            set
            {
                mFilterDayComparisonDAChecked = value;
                if (value == true && SpreadComboSelectedValue == "Exclusive")
                {
                    FilterDayComparisonDARTChecked = false;
                    FilterDayComparisonRTChecked = false;
                }
                RaisePropertyChanged("FilterDayComparisonDAChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    UpdateChartCommand();
                }
            }
        }
        /// <summary>
        /// The m filter day comparison rt checked
        /// </summary>
        private bool mFilterDayComparisonRTChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison rt checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison rt checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonRTChecked
        {
            get
            {
                return mFilterDayComparisonRTChecked;
            }
            set
            {
                mFilterDayComparisonRTChecked = value;
                if (value == true && SpreadComboSelectedValue == "Exclusive")
                {
                    FilterDayComparisonDAChecked = false;
                    FilterDayComparisonDARTChecked = false;
                }
                RaisePropertyChanged("FilterDayComparisonRTChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    UpdateChartCommand();
                }
            }
        }
        /// <summary>
        /// The m filter day comparison dart checked
        /// </summary>
        private bool mFilterDayComparisonDARTChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison dart checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison dart checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonDARTChecked
        {
            get
            {
                return mFilterDayComparisonDARTChecked;
            }
            set
            {
                mFilterDayComparisonDARTChecked = value;
                if (value == true && SpreadComboSelectedValue == "Exclusive")
                {
                    FilterDayComparisonDAChecked = false;
                    FilterDayComparisonRTChecked = false;
                }
                RaisePropertyChanged("FilterDayComparisonDARTChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    UpdateChartCommand();
                }
            }
        }
        /// <summary>
        /// The m filter day comparison spread checked
        /// </summary>
        private bool mFilterDayComparisonSpreadChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison spread checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison spread checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonSpreadChecked
        {
            get
            {
                return mFilterDayComparisonSpreadChecked;
            }
            set
            {
                mFilterDayComparisonSpreadChecked = value;
                RaisePropertyChanged("FilterDayComparisonSpreadChecked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m filter day comparison totals checked
        /// </summary>
        private bool mFilterDayComparisonTotalsChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison total checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison total checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonTotalChecked
        {
            get
            {
                return mFilterDayComparisonTotalsChecked;
            }
            set
            {
                mFilterDayComparisonTotalsChecked = value;
                RaisePropertyChanged("FilterDayComparisonTotalChecked");
                //if (FilterDayComparisonTotalChecked)
                SetSummary();
            }
        }
        /// <summary>
        /// The m filter day comparison average checked
        /// </summary>
        private bool mFilterDayComparisonAvgChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison average checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison average checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonAvgChecked
        {
            get
            {
                return mFilterDayComparisonAvgChecked;
            }
            set
            {
                mFilterDayComparisonAvgChecked = value;
                RaisePropertyChanged("FilterDayComparisonAvgChecked");
                // if (FilterDayComparisonAvgChecked)
                SetSummary();
            }
        }
        /// <summary>
        /// The m filter day comparison win PCT checked
        /// </summary>
        private bool mFilterDayComparisonWinPctChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison win PCT checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison win PCT checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonWinPctChecked
        {
            get
            {
                return mFilterDayComparisonWinPctChecked;
            }
            set
            {
                mFilterDayComparisonWinPctChecked = value;
                RaisePropertyChanged("FilterDayComparisonWinPctChecked");
                //if (FilterDayComparisonWinPctChecked)
                SetSummary();
            }
        }
        /// <summary>
        /// The m filter day comparison minimum checked
        /// </summary>
        private bool mFilterDayComparisonMinChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison minimum checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison minimum checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonMinChecked
        {
            get
            {
                return mFilterDayComparisonMinChecked;
            }
            set
            {
                mFilterDayComparisonMinChecked = value;
                RaisePropertyChanged("FilterDayComparisonMinChecked");
                // if (FilterDayComparisonMinChecked)
                SetSummary();
            }
        }
        /// <summary>
        /// The m filter day comparison maximum checked
        /// </summary>
        private bool mFilterDayComparisonMaxChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison maximum checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison maximum checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonMaxChecked
        {
            get
            {
                return mFilterDayComparisonMaxChecked;
            }
            set
            {
                mFilterDayComparisonMaxChecked = value;
                RaisePropertyChanged("FilterDayComparisonMaxChecked");
                // if (FilterDayComparisonMaxChecked)
                SetSummary();
            }
        }
        /// <summary>
        /// The m filter day comparison sharpe checked
        /// </summary>
        private bool mFilterDayComparisonSharpeChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison sharpe checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison sharpe checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonSharpeChecked
        {
            get
            {
                return mFilterDayComparisonSharpeChecked;
            }
            set
            {
                mFilterDayComparisonSharpeChecked = value;
                RaisePropertyChanged("FilterDayComparisonSharpeChecked");
            }
        }
        /// <summary>
        /// The m filter day comparison risk reward checked
        /// </summary>
        private bool mFilterDayComparisonRiskRewardChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison risk reward checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison risk reward checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonRiskRewardChecked
        {
            get
            {
                return mFilterDayComparisonRiskRewardChecked;
            }
            set
            {
                mFilterDayComparisonRiskRewardChecked = value;
                if (FilterDayComparisonRiskRewardChecked)
                    SetSummary();
                RaisePropertyChanged("FilterDayComparisonRiskRewardChecked");
            }
        }

        /// <summary>
        /// The filter day comparison risk checked
        /// </summary>
        private bool filterDayComparisonRiskChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison risk checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison risk checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonRiskChecked
        {
            get { return filterDayComparisonRiskChecked; }
            set
            {
                filterDayComparisonRiskChecked = value;
                //if (FilterDayComparisonRiskChecked)
                SetSummary();
                RaisePropertyChanged("FilterDayComparisonRiskChecked");
            }
        }

        /// <summary>
        /// The m mw text
        /// </summary>
        private string mMWText;
        /// <summary>
        /// Gets or sets the mw text.
        /// </summary>
        /// <value>
        /// The mw text.
        /// </value>
        public string MWText
        {
            get
            {
                return mMWText;
            }
            set
            {
                mMWText = value;
                RaisePropertyChanged("MWText");
            }
        }
        /// <summary>
        /// The m cleared text
        /// </summary>
        private string mClearedText;
        /// <summary>
        /// Gets or sets the cleared text.
        /// </summary>
        /// <value>
        /// The cleared text.
        /// </value>
        public string ClearedText
        {
            get
            {
                return mClearedText;
            }
            set
            {
                mClearedText = value;
                RaisePropertyChanged("ClearedText");
            }
        }
        /// <summary>
        /// The m da text
        /// </summary>
        private string mDAText;
        /// <summary>
        /// Gets or sets the da text.
        /// </summary>
        /// <value>
        /// The da text.
        /// </value>
        public string DAText
        {
            get
            {
                return mDAText;
            }
            set
            {
                mDAText = value;
                RaisePropertyChanged("DAText");
            }
        }



        ///<summary>
        ///the m Submitted cleared text
        ///</summary>
        private string mSubmittedMwhText;
        /// <summary>
        /// Gets or sets the Submittedcleared value
        /// /// </summary>
        /// <value>
        /// The Submittedcleared
        /// </value>
        /// 

        public string SubmittedMwhText
        {

            get
            {
                return mSubmittedMwhText;
            }

            set
            {

                mSubmittedMwhText = value;
                RaisePropertyChanged("SubmittedMwhText");


            }
        }


        //clearedmwhText




        ///<summary>
        ///the m Clearedmwh cleared text
        ///</summary>
        //private string mClearedMwhText;
        ///// <summary>
        ///// Gets or sets the Submittedcleared value
        ///// /// </summary>
        ///// <value>
        ///// The Submittedcleared
        ///// </value>
        ///// 

        //public string clearedmwhText
        //{

        //    get
        //    {
        //        return mClearedMwhText;
        //    }

        //    set
        //    {

        //        mClearedMwhText = value;
        //        RaisePropertyChanged("clearedmwhText");


        //    }
        //}



        /// <summary>
        /// Gets or sets the total credit.
        /// </summary>
        /// <value>
        /// The total credit.
        /// </value>

        /// <summary>
        /// The m total credit
        /// </summary>
        private string mTotalCredit;
        /// <summary>
        /// Gets or sets the total credit.
        /// </summary>
        /// <value>
        /// The total credit.
        /// </value>
        public string TotalCredit
        {
            get
            {
                return mTotalCredit;
            }
            set
            {
                mTotalCredit = value;
                RaisePropertyChanged("TotalCredit");
            }
        }
        /// <summary>
        /// The m date collection list
        /// </summary>
        private List<DateTime> mDateCollectionList;
        /// <summary>
        /// Gets or sets the date collection list.
        /// </summary>
        /// <value>
        /// The date collection list.
        /// </value>
        public List<DateTime> DateCollectionList
        {
            get
            {
                return mDateCollectionList;
            }
            set
            {
                mDateCollectionList = value;
                RaisePropertyChanged("DateCollectionList");
            }
        }
        /// <summary>
        /// The m daily checked
        /// </summary>
        private bool mDailyChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [daily checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [daily checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DailyChecked
        {
            get
            {
                return mDailyChecked;
            }
            set
            {
                mDailyChecked = value;
                if (DailyChecked)
                    MonthlyChecked = false;
                UpdateChartCommand();
                RaisePropertyChanged("DailyChecked");
            }
        }
        /// <summary>
        /// The m monthly pivot list
        /// </summary>
        private List<HourlyPivotData> mMonthlyPivotList;
        /// <summary>
        /// Gets or sets the monthly pivot list.
        /// </summary>
        /// <value>
        /// The monthly pivot list.
        /// </value>
        public List<HourlyPivotData> MonthlyPivotList
        {
            get
            {
                return mMonthlyPivotList;
            }
            set
            {
                mMonthlyPivotList = value;
                RaisePropertyChanged("MonthlyPivotList");
            }
        }
        /// <summary>
        /// The m monthly dart hash
        /// </summary>
        private Dictionary<DateTime, PNL> mMonthlyDartHash;
        /// <summary>
        /// Gets or sets the monthly dart hash.
        /// </summary>
        /// <value>
        /// The monthly dart hash.
        /// </value>
        public Dictionary<DateTime, PNL> MonthlyDartHash
        {
            get
            {
                return mMonthlyDartHash;
            }
            set
            {
                mMonthlyDartHash = value;
                RaisePropertyChanged("MonthlyDartHash");
            }
        }

        /// <summary>
        /// The m as bid monthly pivot list
        /// </summary>
        private List<HourlyPivotData> mAsBidMonthlyPivotList;
        /// <summary>
        /// Gets or sets as bid monthly pivot list.
        /// </summary>
        /// <value>
        /// As bid monthly pivot list.
        /// </value>
        public List<HourlyPivotData> AsBidMonthlyPivotList
        {
            get
            {
                return mAsBidMonthlyPivotList;
            }
            set
            {
                mAsBidMonthlyPivotList = value;
                RaisePropertyChanged("AsBidMonthlyPivotList");
            }
        }
        /// <summary>
        /// The m must take monthly pivot list
        /// </summary>
        private List<HourlyPivotData> mMustTakeMonthlyPivotList;
        /// <summary>
        /// Gets or sets the must take monthly pivot list.
        /// </summary>
        /// <value>
        /// The must take monthly pivot list.
        /// </value>
        public List<HourlyPivotData> MustTakeMonthlyPivotList
        {
            get
            {
                return mMustTakeMonthlyPivotList;
            }
            set
            {
                mMustTakeMonthlyPivotList = value;
                RaisePropertyChanged("MustTakeMonthlyPivotList");
            }
        }
        /// <summary>
        /// The m collection list
        /// </summary>
        private List<string> mCollectionList;
        /// <summary>
        /// Gets or sets the collection list.
        /// </summary>
        /// <value>
        /// The collection list.
        /// </value>
        public List<string> CollectionList
        {
            get
            {
                return mCollectionList;
            }
            set
            {
                mCollectionList = value;
                RaisePropertyChanged("CollectionList");
            }
        }
        /// <summary>
        /// The m collection checked
        /// </summary>
        private bool mCollectionChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [collection checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [collection checked]; otherwise, <c>false</c>.
        /// </value>
        public bool CollectionChecked
        {
            get
            {
                return mCollectionChecked;
            }
            set
            {
                mCollectionChecked = value;
                if (value == true)
                {
                    CollectionList = DBAccess.GetDateNames();
                }
                RaisePropertyChanged("CollectionChecked");
            }
        }
        /// <summary>
        /// The m collection combo selected value
        /// </summary>
        private string mCollectionComboSelectedValue;
        /// <summary>
        /// Gets or sets the collection combo selected value.
        /// </summary>
        /// <value>
        /// The collection combo selected value.
        /// </value>
        public string CollectionComboSelectedValue
        {
            get
            {
                return mCollectionComboSelectedValue;
            }
            set
            {
                mCollectionComboSelectedValue = value;
                DateCollectionList = DBAccess.GetDateRange(value);
                RaisePropertyChanged("CollectionComboSelectedValue");
            }
        }
        /// <summary>
        /// The m range checked
        /// </summary>
        private bool mRangeChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [range checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [range checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RangeChecked
        {
            get
            {
                return mRangeChecked;
            }
            set
            {
                mRangeChecked = value;
                if (value == true)
                {
                    DateCollectionList = new List<DateTime>();
                    CollectionList = new List<string>();
                    CollectionComboSelectedValue = "";
                }
                RaisePropertyChanged("RangeChecked");
            }
        }
        /// <summary>
        /// The m selected transaction
        /// </summary>
        private CRRTransaction mSelectedTransaction;
        /// <summary>
        /// Gets or sets the selected transaction.
        /// </summary>
        /// <value>
        /// The selected transaction.
        /// </value>
        public CRRTransaction SelectedTransaction
        {
            get
            {
                return mSelectedTransaction;
            }
            set
            {
                mSelectedTransaction = value;
                RaisePropertyChanged("SelectedTransaction");
            }
        }
        /// <summary>
        /// The m transaction list
        /// </summary>
        private List<CRRTransaction> mTransactionList;
        /// <summary>
        /// Gets or sets the transaction list.
        /// </summary>
        /// <value>
        /// The transaction list.
        /// </value>
        public List<CRRTransaction> TransactionList
        {
            get
            {
                return mTransactionList;
            }
            set
            {
                mTransactionList = value;
                RaisePropertyChanged("TransactionList");
            }
        }
        /// <summary>
        /// The m trader portfolio combo list
        /// </summary>
        private List<CRRAuction> mTraderPortfolioComboList;
        /// <summary>
        /// Gets or sets the trader portfolio combo list.
        /// </summary>
        /// <value>
        /// The trader portfolio combo list.
        /// </value>
        public List<CRRAuction> TraderPortfolioComboList
        {
            get
            {
                return mTraderPortfolioComboList;
            }
            set
            {
                mTraderPortfolioComboList = value;
                RaisePropertyChanged("TraderPortfolioComboList");
            }
        }
        /// <summary>
        /// The m auctiontext
        /// </summary>
        private string mAuctiontext;
        /// <summary>
        /// Gets or sets the auctiontext.
        /// </summary>
        /// <value>
        /// The auctiontext.
        /// </value>
        public string Auctiontext
        {
            get
            {
                return mAuctiontext;
            }
            set
            {
                mAuctiontext = value;
                RaisePropertyChanged("Auctiontext");
            }
        }
        /// <summary>
        /// The m trader portfolio combo selected item
        /// </summary>
        private CRRAuction mTraderPortfolioComboSelectedItem;
        /// <summary>
        /// Gets or sets the trader portfolio combo selected item.
        /// </summary>
        /// <value>
        /// The trader portfolio combo selected item.
        /// </value>
        public CRRAuction TraderPortfolioComboSelectedItem
        {
            get
            {
                return mTraderPortfolioComboSelectedItem;
            }
            set
            {
                mTraderPortfolioComboSelectedItem = value;
                RaisePropertyChanged("TraderPortfolioComboSelectedItem");
            }
        }
        /// <summary>
        /// The m must take checked
        /// </summary>
        private bool mMustTakeChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [must take checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [must take checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MustTakeChecked
        {
            get
            {
                return mMustTakeChecked;
            }
            set
            {
                mMustTakeChecked = value;
                if (mMustTakeChecked == true)
                {
                    AsBidChecked = false;
                    RetrieveFetchDataAndUpdateChartCommand();
                }
                UpdateChartCommand();
                RaisePropertyChanged("MustTakeChecked");
            }
        }
        /// <summary>
        /// The m as bid checked
        /// </summary>
        private bool mAsBidChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [as bid checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [as bid checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AsBidChecked
        {
            get
            {
                return mAsBidChecked;
            }
            set
            {
                mAsBidChecked = value;
                if (mAsBidChecked == true)
                {
                    MustTakeChecked = false;
                    RetrieveFetchDataAndUpdateChartCommand();
                }
                UpdateChartCommand();
                RaisePropertyChanged("AsBidChecked");
            }
        }
        /// <summary>
        /// The m path plot model upper
        /// </summary>
        private PlotModel mPathPlotModelUpper;
        /// <summary>
        /// Gets or sets the path plot model upper.
        /// </summary>
        /// <value>
        /// The path plot model upper.
        /// </value>
        public PlotModel PathPlotModelUpper
        {
            get
            {
                return mPathPlotModelUpper;
            }
            set
            {
                mPathPlotModelUpper = value;
                RaisePropertyChanged("PathPlotModelUpper");
            }
        }
        /// <summary>
        /// The m plot model upper
        /// </summary>
        private PlotModel mPlotModelUpper;
        /// <summary>
        /// Gets or sets the plot model upper.
        /// </summary>
        /// <value>
        /// The plot model upper.
        /// </value>
        public PlotModel PlotModelUpper
        {
            get
            {
                return mPlotModelUpper;
            }
            set
            {
                mPlotModelUpper = value;
                RaisePropertyChanged("PlotModelUpper");
            }
        }
        /// <summary>
        /// The m plot model lower
        /// </summary>
        private PlotModel mPlotModelLower;
        /// <summary>
        /// Gets or sets the plot model lower.
        /// </summary>
        /// <value>
        /// The plot model lower.
        /// </value>
        public PlotModel PlotModelLower
        {
            get
            {
                return mPlotModelLower;
            }
            set
            {
                mPlotModelLower = value;
                RaisePropertyChanged("PlotModelLower");
            }
        }
        /// <summary>
        /// The m monthly checked
        /// </summary>
        private bool mMonthlyChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [monthly checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [monthly checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MonthlyChecked
        {
            get
            {
                return mMonthlyChecked;
            }
            set
            {
                mMonthlyChecked = value;
                // UpdateChartCommand();
                RaisePropertyChanged("MonthlyChecked");
            }
        }
        /// <summary>
        /// The m round combo list
        /// </summary>
        private List<int> mRoundComboList;
        /// <summary>
        /// Gets or sets the round combo list.
        /// </summary>
        /// <value>
        /// The round combo list.
        /// </value>
        public List<int> RoundComboList
        {
            get
            {
                return mRoundComboList;
            }
            set
            {
                mRoundComboList = value;
                RaisePropertyChanged("RoundComboList");
            }
        }
        /// <summary>
        /// The m selected round
        /// </summary>
        private int mSelectedRound;
        /// <summary>
        /// Gets or sets the selected round.
        /// </summary>
        /// <value>
        /// The selected round.
        /// </value>
        public int SelectedRound
        {
            get
            {
                return mSelectedRound;
            }
            set
            {
                mSelectedRound = value;
                RaisePropertyChanged("SelectedRound");
            }
        }
        /// <summary>
        /// The m start date
        /// </summary>
        private DateTime mStartDate;
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
                mStartDate = value.Date;
                RaisePropertyChanged("StartDate");
            }
        }
        /// <summary>
        /// The m end date
        /// </summary>
        private DateTime mEndDate;
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
            }
        }

        /// <summary>
        /// The m type text
        /// </summary>
        private string mTypeText;
        /// <summary>
        /// Gets or sets the type text.
        /// </summary>
        /// <value>
        /// The type text.
        /// </value>
        public string TypeText
        {
            get
            {
                return mTypeText;
            }
            set
            {
                mTypeText = value;
                RaisePropertyChanged("TypeText");
            }
        }
        /// <summary>
        /// The m path list
        /// </summary>
        private List<FTRBid> mPathList;
        /// <summary>
        /// Gets or sets the path list.
        /// </summary>
        /// <value>
        /// The path list.
        /// </value>
        public List<FTRBid> PathList
        {
            get
            {
                return mPathList;
            }
            set
            {
                mPathList = value;
                RaisePropertyChanged("PathList");
            }
        }
        /// <summary>
        /// The m portfolio list
        /// </summary>
        private List<Portfolio> mPortfolioList;
        /// <summary>
        /// Gets or sets the portfolio list.
        /// </summary>
        /// <value>
        /// The portfolio list.
        /// </value>
        public List<Portfolio> PortfolioList
        {
            get
            {
                return mPortfolioList;
            }
            set
            {
                mPortfolioList = value;
                RaisePropertyChanged("PortfolioList");
            }
        }
        /// <summary>
        /// The m auction portfolio combo list
        /// </summary>
        private List<string> mAuctionPortfolioComboList;
        /// <summary>
        /// Gets or sets the auction portfolio combo list.
        /// </summary>
        /// <value>
        /// The auction portfolio combo list.
        /// </value>
        public List<string> AuctionPortfolioComboList
        {
            get
            {
                return mAuctionPortfolioComboList;
            }
            set
            {
                mAuctionPortfolioComboList = value;
                RaisePropertyChanged("AuctionPortfolioComboList");
            }
        }
        /// <summary>
        /// The m portfolio combo selected item
        /// </summary>
        private Portfolio mPortfolioComboSelectedItem;
        /// <summary>
        /// Gets or sets the portfolio combo selected item.
        /// </summary>
        /// <value>
        /// The portfolio combo selected item.
        /// </value>
        public Portfolio PortfolioComboSelectedItem
        {
            get
            {
                return mPortfolioComboSelectedItem;
            }
            set
            {
                mPortfolioComboSelectedItem = value;
                RaisePropertyChanged("PortfolioComboSelectedItem");
            }
        }
        /// <summary>
        /// The m auction selected item
        /// </summary>
        private string mAuctionSelectedItem;
        /// <summary>
        /// Gets or sets the auction selected item.
        /// </summary>
        /// <value>
        /// The auction selected item.
        /// </value>
        public string AuctionSelectedItem
        {
            get
            {
                return mAuctionSelectedItem;
            }
            set
            {
                mAuctionSelectedItem = value;
                if (!ShowNewPortfolioChecked)
                    SetPortfolioList();
                if (ExternalPorfolios)
                    SetExternalPortfolioList();
                if (ShowNewPortfolioChecked)
                    SetPortfolioList();
                RaisePropertyChanged("AuctionSelectedItem");
            }
        }
        /// <summary>
        /// The m portfolio combo list
        /// </summary>
        private List<Portfolio> mPortfolioComboList;
        /// <summary>
        /// Gets or sets the portfolio combo list.
        /// </summary>
        /// <value>
        /// The portfolio combo list.
        /// </value>
        public List<Portfolio> PortfolioComboList
        {
            get
            {
                return mPortfolioComboList;
            }
            set
            {
                mPortfolioComboList = value;
                RaisePropertyChanged("PortfolioComboList");
            }
        }
        /// <summary>
        /// The m hourly pivot list summary
        /// </summary>
        private List<HourlyPivotData> mHourlyPivotListSummary;
        /// <summary>
        /// Gets or sets the hourly pivot list summary.
        /// </summary>
        /// <value>
        /// The hourly pivot list summary.
        /// </value>
        public List<HourlyPivotData> HourlyPivotListSummary
        {
            get
            {
                return mHourlyPivotListSummary;
            }
            set
            {
                mHourlyPivotListSummary = value;
                RaisePropertyChanged("HourlyPivotListSummary");
            }
        }
        /// <summary>
        /// The m as bid win per text
        /// </summary>
        private string mAsBidWinPerText;
        /// <summary>
        /// Gets or sets as bid win per text.
        /// </summary>
        /// <value>
        /// As bid win per text.
        /// </value>
        public string AsBidWinPerText
        {
            get
            {
                return mAsBidWinPerText;
            }
            set
            {
                mAsBidWinPerText = value;
                RaisePropertyChanged("AsBidWinPerText");
            }
        }
        /// <summary>
        /// The m as bid sum text
        /// </summary>
        private string mAsBidSumText;
        /// <summary>
        /// Gets or sets as bid sum text.
        /// </summary>
        /// <value>
        /// As bid sum text.
        /// </value>
        public string AsBidSumText
        {
            get
            {
                return mAsBidSumText;
            }
            set
            {
                mAsBidSumText = value;
                RaisePropertyChanged("AsBidSumText");
            }
        }

        /// <summary>
        /// The m as bid dol mw
        /// </summary>
        private string mAsBidDolMW;
        /// <summary>
        /// Gets or sets as bid dol mw.
        /// </summary>
        /// <value>
        /// As bid dol mw.
        /// </value>
        public string AsBidDolMW
        {
            get
            {
                return mAsBidDolMW;
            }
            set
            {
                mAsBidDolMW = value;
                RaisePropertyChanged("AsBidDolMW");
            }
        }

        /// <summary>
        /// The m as bid risk reward
        /// </summary>
        private string mAsBidRiskReward;
        /// <summary>
        /// Gets or sets as bid risk reward.
        /// </summary>
        /// <value>
        /// As bid risk reward.
        /// </value>
        public string AsBidRiskReward
        {
            get
            {
                return mAsBidRiskReward;
            }
            set
            {
                mAsBidRiskReward = value;
                RaisePropertyChanged("AsBidRiskReward");
            }
        }

        /// <summary>
        /// The m as bid risk
        /// </summary>
        private string mAsBidRisk;
        /// <summary>
        /// Gets or sets as bid risk.
        /// </summary>
        /// <value>
        /// As bid risk.
        /// </value>
        public string AsBidRisk
        {
            get
            {
                return mAsBidRisk;
            }
            set
            {
                mAsBidRisk = value;
                RaisePropertyChanged("AsBidRisk");
            }
        }

        /// <summary>
        /// The m as bid win
        /// </summary>
        private string mAsBidWin;
        /// <summary>
        /// Gets or sets as bid win.
        /// </summary>
        /// <value>
        /// As bid win.
        /// </value>
        public string AsBidWin
        {
            get
            {
                return mAsBidWin;
            }
            set
            {
                mAsBidWin = value;
                RaisePropertyChanged("AsBidWin");
            }
        }

        /// <summary>
        /// The m as bid risk date
        /// </summary>
        private string mAsBidRiskDate;
        /// <summary>
        /// Gets or sets as bid risk date.
        /// </summary>
        /// <value>
        /// As bid risk date.
        /// </value>
        public string AsBidRiskDate
        {
            get
            {
                return mAsBidRiskDate;
            }
            set
            {
                mAsBidRiskDate = value;
                RaisePropertyChanged("AsBidRiskDate");
            }
        }

        /// <summary>
        /// The m as bid win date
        /// </summary>
        private string mAsBidWinDate;
        /// <summary>
        /// Gets or sets as bid win date.
        /// </summary>
        /// <value>
        /// As bid win date.
        /// </value>
        public string AsBidWinDate
        {
            get
            {
                return mAsBidWinDate;
            }
            set
            {
                mAsBidWinDate = value;
                RaisePropertyChanged("AsBidWinDate");
            }
        }

        /// <summary>
        /// The m as bid maximum draw down
        /// </summary>
        private string mAsBidMaxDrawDown;
        /// <summary>
        /// Gets or sets as bid maximum draw down.
        /// </summary>
        /// <value>
        /// As bid maximum draw down.
        /// </value>
        public string AsBidMaxDrawDown
        {
            get
            {
                return mAsBidMaxDrawDown;
            }
            set
            {
                mAsBidMaxDrawDown = value;
                RaisePropertyChanged("AsBidMaxDrawDown");
            }
        }

        /// <summary>
        /// The m as bid maximum draw down date
        /// </summary>
        /// 

        private List<PortfolioAnalyserHelper> mPathList_analyser;
        /// <summary>
        /// Gets or sets the Portfolio Analyser  path list.
        /// </summary>
        /// <value>
        /// The path list.
        /// </value>
        public List<PortfolioAnalyserHelper> Portfolio_analyser_PathList
        {
            get
            {
                return mPathList_analyser;
            }
            set
            {
                mPathList_analyser = value;
                RaisePropertyChanged("Portfolio_analyser_PathList");
            }
        }


        private DateTime _ExposureStartDate;

        public DateTime ExposureStartDate
        {
            get { return _ExposureStartDate; }
            set
            {
                _ExposureStartDate = value;
                RaisePropertyChanged("ExposureStartDate");
            }
        }

        private DateTime _ExposureEndDate;

        public DateTime ExposureEndDate
        {
            get { return _ExposureEndDate; }
            set
            {
                _ExposureEndDate = value;
                RaisePropertyChanged("ExposureEndDate");
            }
        }


        private string mAsBidMaxDrawDownDate;
        /// <summary>
        /// Gets or sets as bid maximum draw down date.
        /// </summary>
        /// <value>
        /// As bid maximum draw down date.
        /// </value>

        public string AsBidMaxDrawDownDate
        {
            get
            {
                return mAsBidMaxDrawDownDate;
            }
            set
            {
                mAsBidMaxDrawDownDate = value;
                RaisePropertyChanged("AsBidMaxDrawDownDate");
            }
        }

        /// <summary>
        /// The m must take win per text
        /// </summary>
        private string mMustTakeWinPerText;
        /// <summary>
        /// Gets or sets the must take win per text.
        /// </summary>
        /// <value>
        /// The must take win per text.
        /// </value>
        public string MustTakeWinPerText
        {
            get
            {
                return mMustTakeWinPerText;
            }
            set
            {
                mMustTakeWinPerText = value;
                RaisePropertyChanged("MustTakeWinPerText");
            }
        }

        /// <summary>
        /// The m must take sum text
        /// </summary>
        private string mMustTakeSumText;
        /// <summary>
        /// Gets or sets the must take sum text.
        /// </summary>
        /// <value>
        /// The must take sum text.
        /// </value>
        public string MustTakeSumText
        {
            get
            {
                return mMustTakeSumText;
            }
            set
            {
                mMustTakeSumText = value;
                RaisePropertyChanged("MustTakeSumText");
            }
        }

        /// <summary>
        /// The m must take dol mw
        /// </summary>
        private string mMustTakeDolMW;
        /// <summary>
        /// Gets or sets the must take dol mw.
        /// </summary>
        /// <value>
        /// The must take dol mw.
        /// </value>
        public string MustTakeDolMW
        {
            get
            {
                return mMustTakeDolMW;
            }
            set
            {
                mMustTakeDolMW = value;
                RaisePropertyChanged("MustTakeDolMW");
            }
        }
        /// <summary>
        /// The m must take risk reward
        /// </summary>
        private string mMustTakeRiskReward;
        /// <summary>
        /// Gets or sets the must take risk reward.
        /// </summary>
        /// <value>
        /// The must take risk reward.
        /// </value>
        public string MustTakeRiskReward
        {
            get
            {
                return mMustTakeRiskReward;
            }
            set
            {
                mMustTakeRiskReward = value;
                RaisePropertyChanged("MustTakeRiskReward");
            }
        }
        /// <summary>
        /// The m must take risk
        /// </summary>
        private string mMustTakeRisk;
        /// <summary>
        /// Gets or sets the must take risk.
        /// </summary>
        /// <value>
        /// The must take risk.
        /// </value>
        public string MustTakeRisk
        {
            get
            {
                return mMustTakeRisk;
            }
            set
            {
                mMustTakeRisk = value;
                RaisePropertyChanged("MustTakeRisk");
            }
        }
        /// <summary>
        /// The m must take win
        /// </summary>
        private string mMustTakeWin;
        /// <summary>
        /// Gets or sets the must take win.
        /// </summary>
        /// <value>
        /// The must take win.
        /// </value>
        public string MustTakeWin
        {
            get
            {
                return mMustTakeWin;
            }
            set
            {
                mMustTakeWin = value;
                RaisePropertyChanged("MustTakeWin");
            }
        }
        /// <summary>
        /// The m must take risk date
        /// </summary>
        private string mMustTakeRiskDate;
        /// <summary>
        /// Gets or sets the must take risk date.
        /// </summary>
        /// <value>
        /// The must take risk date.
        /// </value>
        public string MustTakeRiskDate
        {
            get
            {
                return mMustTakeRiskDate;
            }
            set
            {
                mMustTakeRiskDate = value;
                RaisePropertyChanged("MustTakeRiskDate");
            }
        }
        /// <summary>
        /// The m must take win date
        /// </summary>
        private string mMustTakeWinDate;
        /// <summary>
        /// Gets or sets the must take win date.
        /// </summary>
        /// <value>
        /// The must take win date.
        /// </value>
        public string MustTakeWinDate
        {
            get
            {
                return mMustTakeWinDate;
            }
            set
            {
                mMustTakeWinDate = value;
                RaisePropertyChanged("MustTakeWinDate");
            }
        }
        /// <summary>
        /// The m must take maximum draw down
        /// </summary>
        private string mMustTakeMaxDrawDown;
        /// <summary>
        /// Gets or sets the must take maximum draw down.
        /// </summary>
        /// <value>
        /// The must take maximum draw down.
        /// </value>
        public string MustTakeMaxDrawDown
        {
            get
            {
                return mMustTakeMaxDrawDown;
            }
            set
            {
                mMustTakeMaxDrawDown = value;
                RaisePropertyChanged("MustTakeMaxDrawDown");
            }
        }
        /// <summary>
        /// The m must take maximum draw down date
        /// </summary>
        private string mMustTakeMaxDrawDownDate;
        /// <summary>
        /// Gets or sets the must take maximum draw down date.
        /// </summary>
        /// <value>
        /// The must take maximum draw down date.
        /// </value>
        public string MustTakeMaxDrawDownDate
        {
            get
            {
                return mMustTakeMaxDrawDownDate;
            }
            set
            {
                mMustTakeMaxDrawDownDate = value;
                RaisePropertyChanged("MustTakeMaxDrawDownDate");
            }
        }

        /// <summary>
        /// The m sort dart checked
        /// </summary>
        private bool mSortDartChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [sort dart checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort dart checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SortDartChecked
        {
            get
            {
                return mSortDartChecked;
            }
            set
            {
                mSortDartChecked = value;
                RaisePropertyChanged("SortDartChecked");
                Sort(MonthlyDartHash);
            }
        }
        /// <summary>
        /// The m sort rt checked
        /// </summary>
        private bool mSortRtChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [sort rt checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort rt checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SortRtChecked
        {
            get
            {
                return mSortRtChecked;
            }
            set
            {
                mSortRtChecked = value;
                RaisePropertyChanged("SortRtChecked");
                Sort(MonthlyDartHash);
            }
        }
        /// <summary>
        /// The m sort da checked
        /// </summary>
        private bool mSortDaChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [sort da checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort da checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SortDaChecked
        {
            get
            {
                return mSortDaChecked;
            }
            set
            {
                mSortDaChecked = value;
                RaisePropertyChanged("SortDaChecked");
                Sort(MonthlyDartHash);
            }
        }
        private bool stDARTChecked;
        public bool DARTChecked
        {
            get
            {
                return stDARTChecked;
            }
            set
            {
                stDARTChecked = value;
                // UpdateChartCommand();
                RaisePropertyChanged("DARTChecked");
            }
        }

        private bool stCostPriceChecked;
        public bool CostPriceChecked
        {
            get
            {
                return stCostPriceChecked;
            }
            set
            {
                stCostPriceChecked = value;
                // UpdateChartCommand();
                RaisePropertyChanged("CostPriceChecked");
            }
        }
        private bool stDAPriceChecked;
        public bool DAPriceChecked
        {
            get
            {
                return stDAPriceChecked;
            }
            set
            {
                stDAPriceChecked = value;
                // UpdateChartCommand();
                RaisePropertyChanged("DAPriceChecked");
            }
        }
        private bool stRTPriceChecked;

        public bool RTPriceChecked
        {

            get
            {

                return stRTPriceChecked;

            }

            set
            {
                stRTPriceChecked = value;
                RaisePropertyChanged("RTPriceChecked");
            }


        }
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
        /// <summary>
        /// The m sort date checked
        /// </summary>
        private bool mSortDateChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [sort date checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort date checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SortDateChecked
        {
            get
            {
                return mSortDateChecked;
            }
            set
            {
                mSortDateChecked = value;
                RaisePropertyChanged("SortDateChecked");
                Sort(MonthlyDartHash);
            }
        }
        /// <summary>
        /// The m spread highlight above
        /// </summary>
        private string mSpreadHighlightAbove;
        /// <summary>
        /// Gets or sets the spread highlight above.
        /// </summary>
        /// <value>
        /// The spread highlight above.
        /// </value>
        public string SpreadHighlightAbove
        {
            get
            {
                return mSpreadHighlightAbove;
            }
            set
            {
                double m;
                if (double.TryParse(value, out m))
                {
                    m = Math.Abs(m);
                    double sf = 24; // sum total mult factor
                    // heatmapfactors: 5 factors for positive.  5 pos factors for the Sum/Total data too.
                    double[] htMapFactors = new double[] { 15 * m, 10 * m, 6 * m, 2 * m, 1 * m, 15 * sf * m, 10 * sf * m, 6 * sf * m, 2 * sf * m, 1 * sf * m };
                    SpreadHeatMapFactorsAbove = htMapFactors;
                    mSpreadHighlightAbove = value;
                    RaisePropertyChanged("SpreadHighlightAbove");
                }
                else if (value == "" && !mSpreadHighlightAbove.Equals(value))
                {
                    double[] htMapFactors = new double[] { };
                    SpreadHeatMapFactorsAbove = htMapFactors;
                    mSpreadHighlightAbove = value;
                    RaisePropertyChanged("SpreadHighlightAbove");
                }
            }
        }
        /// <summary>
        /// The m spread highlight below
        /// </summary>
        private string mSpreadHighlightBelow;
        /// <summary>
        /// Gets or sets the spread highlight below.
        /// </summary>
        /// <value>
        /// The spread highlight below.
        /// </value>
        public string SpreadHighlightBelow
        {
            get
            {
                return mSpreadHighlightBelow;
            }
            set
            {
                double m;
                if (double.TryParse(value, out m))
                {
                    m = Math.Abs(m);
                    double sf = 24; // sum total mult factor
                    // heatmapfactors: 5 factors for negative.  5 neg factors for the Sum/Total data too.
                    double[] htMapFactors = new double[] { -15 * m, -10 * m, -6 * m, -2 * m, -1 * m, -15 * sf * m, -10 * sf * m, -6 * sf * m, -2 * sf * m, -1 * sf * m };
                    SpreadHeatMapFactorsBelow = htMapFactors;
                    mSpreadHighlightBelow = value;
                    RaisePropertyChanged("SpreadHighlightBelow");
                }
                else if (value == "" && !mSpreadHighlightAbove.Equals(value))
                {
                    double[] htMapFactors = new double[] { };
                    SpreadHeatMapFactorsBelow = htMapFactors;
                    mSpreadHighlightBelow = value;
                    RaisePropertyChanged("SpreadHighlightBelow");
                }
            }
        }
        /// <summary>
        /// The m source sink highlight threshold
        /// </summary>
        private string mSourceSinkHighlightThreshold;
        /// <summary>
        /// Gets or sets the source sink highlight threshold.
        /// </summary>
        /// <value>
        /// The source sink highlight threshold.
        /// </value>
        public string SourceSinkHighlightThreshold
        {
            get
            {
                return mSourceSinkHighlightThreshold;
            }
            set
            {
                double m;
                if (double.TryParse(value, out m))
                {
                    m = Math.Abs(m);
                    double sf = 10; // sum total mult factor
                    // heatmapfactors: 5 factors for positive, 5 factors for negative.  5 pos and 5 neg factors for the Sum/Total data too.
                    double[] htMapFactors = new double[] { 15*m, 10*m, 6*m, 2*m, 1*m, -15*m, -10*m, -6*m, -2*m, -1*m,
                                                            15*sf*m, 10*sf*m, 6*sf*m, 2*sf*m, 1*sf*m, -15*sf*m, -10*sf*m, -6*sf*m, -2*sf*m, -1*sf*m };
                    SourceSinkHeatMapFactors = htMapFactors;
                    mSourceSinkHighlightThreshold = value;
                    RaisePropertyChanged("SourceSinkHighlightThreshold");
                }
                else if (value == "")
                {
                    double[] htMapFactors = new double[] { };
                    SourceSinkHeatMapFactors = htMapFactors;
                    mSourceSinkHighlightThreshold = value;
                    RaisePropertyChanged("SourceSinkHighlightThreshold");
                }
            }
        }
        /// <summary>
        /// The m source sink heat map factors
        /// </summary>
        private double[] mSourceSinkHeatMapFactors;
        /// <summary>
        /// Gets or sets the source sink heat map factors.
        /// </summary>
        /// <value>
        /// The source sink heat map factors.
        /// </value>
        public double[] SourceSinkHeatMapFactors
        {
            get { return mSourceSinkHeatMapFactors; }
            set
            {
                mSourceSinkHeatMapFactors = value;
                RaisePropertyChanged("SourceSinkHeatMapFactors");
            }
        }
        string tempdate = string.Empty;
        int Month;
        int year;
        private string mPredicted = "MONTH";
        public string Predicted
        {
            get { return mPredicted; }
            set
            {
                mPredicted = value;
                RaisePropertyChanged("Predicted");
            }
        }


        private List<string> mMonthList;
        public List<string> MonthList
        {
            get { return mMonthList; }
            set { mMonthList = value; }
        }


        private List<int> mYearList;
        public List<int> YearList
        {
            get { return mYearList; }
            set { mYearList = value; }
        }


        private string mMonthSelectedValue;
        public string MonthSelectedValue
        {
            get { return mMonthSelectedValue; }
            set { mMonthSelectedValue = value; }
        }

        private int mYearSelectedValue;
        public int YearSelectedValue
        {
            get { return mYearSelectedValue; }
            set { mYearSelectedValue = value; }
        }
        private DateTime mStartDateMonth;
        public DateTime StartDateMonth
        {
            get
            {
                return mStartDateMonth;
            }
            set
            {
                mStartDateMonth = value.Date;
                RaisePropertyChanged("StartDateMonth");
            }
        }
        private DateTime mEndDateMonth;
        public DateTime EndDateMonth
        {
            get
            {
                return mEndDateMonth;
            }
            set
            {
                mEndDateMonth = value.Date;
                RaisePropertyChanged("EndDateMonth");
            }
        }

        private string mgrpDate;
        public string grpDate
        {
            get
            {
                return mgrpDate;
            }
            set
            {
                mgrpDate = value;
                RaisePropertyChanged("grpDate");
            }
        }

        private bool mgrpMonth;
        public bool grpMonth
        {
            get
            {
                return mgrpMonth;
            }
            set
            {
                mgrpMonth = value;
                RaisePropertyChanged("grpMonth");
            }
        }

        private List<FTRBid> mPathMWList;
        public List<FTRBid> PathMWList
        {
            get
            {
                return mPathMWList;
            }
            set
            {
                mPathMWList = value;
                RaisePropertyChanged("PathMWList");
            }
        }
        private List<FTRBid> mFilterPathMWList;
        public List<FTRBid> FilterPathMWList
        {
            get
            {
                return mFilterPathMWList;
            }
            set
            {
                mFilterPathMWList = value;
                RaisePropertyChanged("FilterPathMWList");
            }
        }
        private bool mSourcePathChecked;

        public bool SourcePathChecked
        {
            get
            {
                return mSourcePathChecked;
            }
            set
            {
                mSourcePathChecked = value;
                RaisePropertyChanged("SourcePathChecked");
            }
        }
        private bool mSinkPathChecked;

        public bool SinkPathChecked
        {
            get
            {
                return mSinkPathChecked;
            }
            set
            {
                mSinkPathChecked = value;
                RaisePropertyChanged("SinkPathChecked");
            }
        }
        private bool mNonePathChecked = true;

        public bool NonePathChecked
        {
            get
            {
                return mNonePathChecked;
            }
            set
            {
                mNonePathChecked = value;
                RaisePropertyChanged("NonePathChecked");
            }
        }

        private bool mPeakChecked = true;
        public bool PeakChecked
        {
            get
            {
                return mPeakChecked;
            }
            set
            {
                mPeakChecked = value;
                RaisePropertyChanged("PeakChecked");
            }
        }

        private bool mOffPeakChecked = true;
        public bool OffPeakChecked
        {
            get
            {
                return mOffPeakChecked;
            }
            set
            {
                mOffPeakChecked = value;
                RaisePropertyChanged("OffPeakChecked");
            }
        }

        private bool mPeakWEChecked = true;
        public bool PeakWEChecked
        {
            get
            {
                return mPeakWEChecked;
            }
            set
            {
                mPeakWEChecked = value;
                RaisePropertyChanged("PeakWEChecked");
            }
        }

        private PlotModel mplotDataFirst;
        public PlotModel PlotDataFirst
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



        private ObservableCollection<MonthCheckbox> _months;
        public ObservableCollection<MonthCheckbox> Months
        {
            get => _months;
            set
            {
                _months = value;
                OnPropertyChanged(nameof(Months));
            }
        }

        private ObservableCollection<MonthCheckbox> _selectedMonths;
        public ObservableCollection<MonthCheckbox> SelectedMonths
        {
            get => _selectedMonths;
            private set
            {
                _selectedMonths = value;
                OnPropertyChanged(nameof(SelectedMonths));
            }
        }

        bool allMonthsSelected;

        private ObservableCollection<DailyPivotData> mDailyPivotDataList = new ObservableCollection<DailyPivotData>();
        private ObservableCollection<DailyPivotData> mDailyFinalPivotDataList;

        private bool mPeakWE;
        public bool PeakWE
        {
            get
            {
                return mPeakWE;
            }
            set
            {
                mPeakWE = value;
                if (mPeakWE)
                {
                    mOnPeakChecked = false;
                    mOffPeakChecked = false;
                    mhour24Checked = false;
                    //UpdateView();
                }
                RaisePropertyChanged("PeakWE");
            }
        }

        private bool mFilterDayComparisonCrrChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison Crr checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison Crr checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonCrrChecked
        {
            get
            {
                return mFilterDayComparisonCrrChecked;
            }
            set
            {
                mFilterDayComparisonCrrChecked = value;
                if (value == true && SpreadComboSelectedValue == "Exclusive")
                {
                    FilterDayComparisonRTChecked = false;
                    FilterDayComparisonDAChecked = false;
                    FilterDayComparisonDACrrChecked = false;
                    //UpdateView();
                }
                RaisePropertyChanged("FilterDayComparisonCrrChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    //UpdateView();
                }
            }
        }

        private bool mFilterDayComparisonDACrrChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison daCrr checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison daCrr checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonDACrrChecked
        {
            get
            {
                return mFilterDayComparisonDACrrChecked;
            }
            set
            {
                mFilterDayComparisonDACrrChecked = value;
                if (value == true && SpreadComboSelectedValue == "Exclusive")
                {
                    FilterDayComparisonDAChecked = false;
                    FilterDayComparisonCrrChecked = false;
                    FilterDayComparisonRTChecked = false;
                    //UpdateView();
                }
                RaisePropertyChanged("FilterDayComparisonDACrrChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    //UpdateView();
                }
            }
        }

        private bool mOnPeakChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [on peak checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on peak checked]; otherwise, <c>false</c>.
        /// </value>
        public bool OnPeakChecked
        {
            get
            {
                return mOnPeakChecked;
            }
            set
            {
                mOnPeakChecked = value;
                if (mOnPeakChecked)
                {
                    mOffPeakChecked = false;
                    mhour24Checked = false;
                    mPeakWE = false;
                    //UpdateView();
                }
                RaisePropertyChanged("OnPeakChecked");
            }
        }

        private bool mhour24Checked;
        /// <summary>
        /// Gets or sets a value indicating whether [hour24 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hour24 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool Hour24Checked
        {
            get
            {
                return mhour24Checked;
            }
            set
            {
                mhour24Checked = value;
                if (mhour24Checked)
                {
                    mOnPeakChecked = false;
                    mOffPeakChecked = false;
                    mPeakWE = false;
                    //UpdateView();
                }
                RaisePropertyChanged("Hour24Checked");
            }
        }

        private List<FilterData> mFilterList;
        /// <summary>
        /// Gets or sets the filter list.
        /// </summary>
        /// <value>
        /// The filter list.
        /// </value>
        public List<FilterData> FilterList
        {
            get
            {
                return mFilterList;
            }
            set
            {
                mFilterList = value;
                RaisePropertyChanged("FilterList");
                //UpdateView();
            }
        }

        private ObservableCollection<DailyPivotData> mDailyPivotList;
        /// <summary>
        /// Gets or sets the daily pivot list.
        /// </summary>
        /// <value>
        /// The daily pivot list.
        /// </value>
        public ObservableCollection<DailyPivotData> DailyPivotList
        {
            get
            {
                return mDailyPivotList;
            }
            set
            {
                mDailyPivotList = value;
                RaisePropertyChanged("DailyPivotList");
            }
        }

        private ObservableCollection<FTRMonthlyData> mCRRMonthlyList;

        public ObservableCollection<FTRMonthlyData> CRRPivotList
        {
            get
            {
                return mCRRMonthlyList;
            }
            set
            {
                mCRRMonthlyList = value;
                RaisePropertyChanged("CRRPivotList");

            }
        }



        private bool showSummaryMin = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show summary minimum].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show summary minimum]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSummaryMin
        {
            get { return showSummaryMin; }
            set { showSummaryMin = value; RaisePropertyChanged("ShowSummaryMin"); SetDailySummary(); }
        }

        public DelegateCommand LoadCommand { private set; get; }

        private bool showSummaryTotal = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show summary total].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show summary total]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSummaryTotal
        {
            get { return showSummaryTotal; }
            set
            {
                showSummaryTotal = value; RaisePropertyChanged("ShowSummaryTotal");
                SetDailySummary();
            }
        }

        private bool showSummaryAvg = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show summary average].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show summary average]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSummaryAvg
        {
            get { return showSummaryAvg; }
            set { showSummaryAvg = value; RaisePropertyChanged("ShowSummaryAvg"); SetDailySummary(); }
        }

        /// <summary>
        /// The show summary maximum
        /// </summary>
        private bool showSummaryMax = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show summary maximum].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show summary maximum]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSummaryMax
        {
            get { return showSummaryMax; }
            set { showSummaryMax = value; RaisePropertyChanged("ShowSummaryMax"); SetDailySummary(); }
        }



        /// <summary>
        /// The show summary win
        /// </summary>
        private bool showSummaryWin = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show summary win].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show summary win]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSummaryWin
        {
            get { return showSummaryWin; }
            set { showSummaryWin = value; RaisePropertyChanged("ShowSummaryWin"); SetDailySummary(); }
        }

        /// <summary>
        /// The show summary risk
        /// </summary>
        private bool showSummaryRisk = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show summary risk].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show summary risk]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSummaryRisk
        {
            get { return showSummaryRisk; }
            set { showSummaryRisk = value; RaisePropertyChanged("ShowSummaryRisk"); SetDailySummary(); }
        }

        public ObservableCollection<DailyPivotData> DailySummaryPivotList
        {
            get { return summaryPivotList; }
            set
            {
                summaryPivotList = value;
                RaisePropertyChanged("DailySummaryPivotList");
            }
        }
        /// <summary>
        /// The m daily pivot list
        /// </summary>

        private ObservableCollection<DailyPivotData> summaryPivotList;
        /// <summary>
        /// Gets or sets the daily summary pivot list.
        /// </summary>
        /// <value>
        /// The daily summary pivot list.
        /// </value>

        private bool mQuaterlyChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [Quaterly checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Quaterly checked]; otherwise, <c>false</c>.
        /// </value>
        public bool QuaterlyChecked
        {
            get
            {
                return mQuaterlyChecked;
            }
            set
            {
                mQuaterlyChecked = value;
                RaisePropertyChanged("QuaterlyChecked");
                if (QuaterlyChecked)
                {
                    MessageBox.Show("Please Make sure you select the correct months and Seasons");
                    MonthlyPeriodChecked = false;
                    AnnualyChecked = false;
                    LongtermChecked = false;
                    //UpdateView(true);
                }

            }
        }

        private bool mAnnualyChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [Annualy checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Annualy checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AnnualyChecked
        {
            get
            {
                return mAnnualyChecked;
            }
            set
            {
                mAnnualyChecked = value;
                RaisePropertyChanged("AnnualyChecked");
                if (AnnualyChecked)
                {
                    MonthlyPeriodChecked = false;
                    QuaterlyChecked = false;
                    LongtermChecked = false;
                    //UpdateView(true);
                }
            }
        }

        public bool MonthlyPeriodChecked
        {
            get { return _MonthlyPeriodChecked; }
            set
            {
                _MonthlyPeriodChecked = value;
                // 
                RaisePropertyChanged("MonthlyPeriodChecked");
                if (MonthlyPeriodChecked)
                {
                    QuaterlyChecked = false;
                    //UpdateView(true);
                }


            }
        }

        private bool mLongtermChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [Long Term checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Long Term checked]; otherwise, <c>false</c>.
        /// </value>
        public bool LongtermChecked
        {
            get
            {
                return mLongtermChecked;
            }
            set
            {
                mLongtermChecked = value;
                RaisePropertyChanged("LongtermChecked");
            }
        }

        private string mHedgeComboSelectedValue;
        /// <summary>
        /// Gets or sets the Hedge combo selected value.
        /// </summary>
        /// <value>
        /// The Hedge combo selected value.
        /// </value>
        public string HedgeComboSelectedValue
        {
            get
            {
                return mHedgeComboSelectedValue;
            }
            set
            {

                mHedgeComboSelectedValue = value;
                RaisePropertyChanged("HedgeComboSelectedValue");
                SetSourceSink();
                //UpdateView();
                if (HedgeComboSelectedValue == "OBL")
                {

                }
                else
                {

                }
            }
        }

        //private Microsoft.Maps.MapControl.WPF.LocationCollection mNodePath;
        ///// <summary>
        ///// Gets or sets the node path.
        ///// </summary>
        ///// <value>
        ///// The node path.
        ///// </value>
        //public Microsoft.Maps.MapControl.WPF.LocationCollection NodePath
        //{
        //    get
        //    {
        //        return mNodePath;
        //    }
        //    set
        //    {
        //        if (mNodePath == value)
        //        {
        //            return;
        //        }
        //        mNodePath = value;
        //        RaisePropertyChanged("NodePath");
        //    }
        //}

        private List<PricingNode> mSourceNodeList;
        /// <summary>
        /// Gets or sets the source node list.
        /// </summary>
        /// <value>
        /// The source node list.
        /// </value>

        public List<PricingNode> SourceNodeList
        {
            get
            {
                return mSourceNodeList;
            }
            set
            {
                mSourceNodeList = value;
                RaisePropertyChanged("SourceNodeList");
            }
        }

        private List<PricingNode> mSinkNodeList;
        /// <summary>
        /// Gets or sets the sink node list.
        /// </summary>
        /// <value>
        /// The sink node list.
        /// </value>
        public List<PricingNode> SinkNodeList
        {
            get
            {
                return mSinkNodeList;
            }
            set
            {
                mSinkNodeList = value;
                RaisePropertyChanged("SinkNodeList");
            }
        }

        private string mCenterLoc;
        /// <summary>
        /// Gets or sets the map center.
        /// </summary>
        /// <value>
        /// The map center.
        /// </value>
        public string MapCenter
        {
            get
            {
                return mCenterLoc;
            }
            set
            {
                mCenterLoc = value;
                RaisePropertyChanged("MapCenter");
            }
        }

        private ObservableCollection<NodeCoordinate> mLocations;
        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        public ObservableCollection<NodeCoordinate> Locations
        {
            get
            {
                return mLocations;
            }
            set
            {
                if (mLocations == value) return;
                mLocations = value;
                RaisePropertyChanged("Locations");
            }
        }

        private IDataService myDataService;
        private bool _MonthlyPeriodChecked;
        private ISourceSink CrrCalculationLibrary = null;
        private Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> yearHash = new Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>>();
        private List<ConsolidatedData> mGraphList = new List<ConsolidatedData>();



        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainWindowViewModel(IDataService dataService)
        {
            try
            {
                Month = DateTime.Now.Month;
                year = DateTime.Now.Year;
                StartDate = DateTime.Now.Date.AddYears(-2);
                EndDate = DateTime.Now.Date;
                SpreadHighlightAbove = "100";
                SpreadHighlightBelow = "-100";
                tempdate = Convert.ToString(Month + "/01/" + year);
                StartDateMonth = Convert.ToDateTime(tempdate);
                EndDateMonth = StartDateMonth.AddMonths(1).AddDays(-1);
                StatStartDateSelected = DateTime.Today.AddYears(-1);
                StatEndDateSelected = DateTime.Today.Date;
                PFAnalyserStartDateSelected = DateTime.Today.AddYears(-1);
                PFAnalyserEndDateSelected = DateTime.Today.Date;
                grpMonth = false;
                AddPortfolioCommand = new DelegateCommand(AddPortfolio);
                RemovePortfolioCommand = new DelegateCommand(RemovePortfolio);
                ImportCommand = new DelegateCommand(Import);
                RetrieveCommand = new DelegateCommand(Retrieve);
                SubmitCommand = new DelegateCommand(Submit);
                SeasonCommand = new DelegateCommand(Season);
                CancelCommand = new DelegateCommand(Cancel);
                CreditCommand = new DelegateCommand(Credit);
                DeleteCommand = new DelegateCommand(Delete);
                NoneClick = new DelegateCommand(OnNone);
                AllCommand = new DelegateCommand(OnAllCommand);
                WinterCommand = new DelegateCommand(OnWinterCommand);
                SpringCommand = new DelegateCommand(OnSpringCommand);
                SummerCommand = new DelegateCommand(OnSummerCommand);
                FallCommand = new DelegateCommand(OnFallCommand);
                RunRetrieveGoCommand = new DelegateCommand(GoCommand);
                RunHistoricalConstOpenCmd = new DelegateCommand(OpenHistoricalConstraints);
                ExportButtonCommand = new DelegateCommand(ExportButton);
                //MustTakeChecked = true;
                SortMWChecked = true;

                CreateSubmissionFileCommand = new DelegateCommand(() => CreateSubmissionFile());
                if (_dataService == null)
                {
                    _dataService = new DataService();
                }
                _dataService.LoadDBCommands();
                // SetAuctionList();
                RoundComboList = new List<int> { 0, 1, 2, 3, 4 };
                MonthlyChecked = true;
                RunRetrieveFetchDataAndUpdateChartCommand = new DelegateCommand(RetrieveFetchDataAndUpdateChartCommand);
                CalcluatePathwistStatCmd = new DelegateCommand(CalcluatePathwistStat);

                PFAnalyser_CalcluateCommond = new DelegateCommand(CalcluatePFAnalysis);
                ExportCommand = new DelegateCommand(ExportToExcel);
                PFAnalyser_ExportCommand = new DelegateCommand(ExportToExcel_PFAnalyser);
                MonthlyAnalysis_ExportCommand = new DelegateCommand(ExportToExcel_MonthlyAnalysis);
                RunPathDetailsCommand = new DelegateCommand(PathDetailsCommand);
                ClickPathMWsCommand = new DelegateCommand(() => ShowPathMws());
                PieChartCommand = new DelegateCommand(() => PieChart());
                Months = new ObservableCollection<MonthCheckbox>
        {
            new MonthCheckbox("Jan", this),
            new MonthCheckbox("Feb", this),
            new MonthCheckbox("Mar", this),
            new MonthCheckbox("Apr", this),
            new MonthCheckbox("May", this),
            new MonthCheckbox("Jun", this),
            new MonthCheckbox("Jul", this),
            new MonthCheckbox("Aug", this),
            new MonthCheckbox("Sep", this),
            new MonthCheckbox("Oct", this),
            new MonthCheckbox("Nov", this),
            new MonthCheckbox("Dec", this)
        };
                // Subscribe to PropertyChanged for each MonthCheckbox
                foreach (var month in Months)
                {
                    month.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(MonthCheckbox.IsChecked))
                        {
                            UpdateSelectedMonths();
                        }
                    };
                }

                // Initial population of selected months
                UpdateSelectedMonths();

                Calculate_MonthlyAnalysis = new DelegateCommand(CalculateMonthlyAnalysis);

                List<string> isoMarketList = new List<string> { "ERCOT" };
                ExposureStartDate = DateTime.Today;
                ExposureEndDate = DateTime.Today;
                MarketComboSelectedValue = "ERCOT";
                ISOMarketList = isoMarketList;
                //SetUserPortfolioList();
                SetNewAuctionPortfolioList();
                MonthList = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

                int currentYear = DateTime.Today.Year;
                YearList = new List<int> { currentYear, currentYear - 1, currentYear - 2 };
                DARTChecked = true;
                isDAChecked = true;
                isCostChecked = true;
                isDARTChecked = true;

            }
            catch (Exception ex)
            {

            }
        }
        public void UpdateSelectedMonths()
        {
            // Ensure SelectedMonths is initialized before clearing
            if (SelectedMonths == null)
            {
                SelectedMonths = new ObservableCollection<MonthCheckbox>();
            }

            // Clear the existing items in SelectedMonths
            SelectedMonths.Clear();

            // Add the checked months to SelectedMonths
            foreach (var month in Months.Where(m => m.IsChecked))
            {
                SelectedMonths.Add(month);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }





        public void UpdateView(List<FTRBid> ftrBids, bool shouldLoadDayComparision = true)
        {
            //if (SourceSinkDataSelected == null)
            //    return;
            if (shouldLoadDayComparision)
                SetDailyPivotList(ftrBids);
        }




        private async void SetDailyPivotList(List<FTRBid> ftrBids)
        {
            ObservableCollection<FTRMonthlyData> mCRRMonthlyDataList = await GetCrrServiceData(ftrBids);


            CRRPivotList = new ObservableCollection<FTRMonthlyData>(mCRRMonthlyDataList);
        }


        private void AssignDisplayTypeRowName(List<DailyPivotData> dailyPivotDataList, bool calculateAverage = true)
        {
            dailyPivotDataList.ForEach(x =>
            {
                if (FilterDayComparisonDAChecked)
                    x.RowDisplayType = "DA";
                else if (FilterDayComparisonDACrrChecked)
                    x.RowDisplayType = "DA-Crr";
                else if (FilterDayComparisonCrrChecked)
                    x.RowDisplayType = "Crr";
                else if (FilterDayComparisonRTChecked)
                    x.RowDisplayType = "RT";

                x.RowName = "Spread";
                ////if (calculateAverage)
                ////    x.Average = x.Total / x.ValueDayCount;
            });
        }

        private void SetDailySummary(List<DailyPivotData> dailyPivotDataList = null)
        {
            if (dailyPivotDataList == null)
                dailyPivotDataList = DailyPivotList.ToList();

            Type typ = typeof(DailyPivotData);
            List<DailyPivotData> summaryList = new List<DailyPivotData>();

            DailyPivotData totalRow = new DailyPivotData()
            {
                RowType = "Total",
                Total = dailyPivotDataList.Sum(x => x.Total),
                Average = dailyPivotDataList.Sum(x => x.Average),
                SummaryType = SummaryRowType.Total
            };
            DailyPivotData avgRow = new DailyPivotData() { RowType = "Average", Total = dailyPivotDataList.Average(x => x.Average), SummaryType = SummaryRowType.Avg };
            DailyPivotData minRow = new DailyPivotData() { RowType = "Min", Total = dailyPivotDataList.Min(x => x.Total), SummaryType = SummaryRowType.Min, Average = dailyPivotDataList.Min(x => x.Average) };
            DailyPivotData maxRow = new DailyPivotData() { RowType = "Max", Total = dailyPivotDataList.Max(x => x.Total), SummaryType = SummaryRowType.Max, Average = dailyPivotDataList.Max(x => x.Average), };
            DailyPivotData winRow = new DailyPivotData();

            if (dailyPivotDataList.Count(x => x.Total.HasValue) > 0)
            {
                winRow.RowType = "Win %";
                winRow.SummaryType = SummaryRowType.Win;
                winRow.Total = dailyPivotDataList.Count
                    (x => x.Total.HasValue && x.Total.Value > 0) / dailyPivotDataList.Count(x => x.Total.HasValue);
            }

            double? winVal = dailyPivotDataList.Select(x => x.Total).Where(x => x.HasValue && x.Value > 0).Count();
            double? winVal2 = dailyPivotDataList.Count(x => x.Total.HasValue);
            winVal = (winVal / winVal2) * 100;
            winRow.Total = winVal;
            DailyPivotData riskRow = new DailyPivotData() { RowType = "Risk", Total = GetRisk(minRow.Total, maxRow.Total, winRow.Total / 100) };

            double? Sum_alldayvalue = 0;
            int Count_valuedays = 0;

            for (int i = 1; i <= 31; i++)
            {
                PropertyInfo info = typ.GetProperty("D" + i);
                double? valD = dailyPivotDataList.Sum(x => info.GetValue(x) as double?);
                Sum_alldayvalue += valD;
                Count_valuedays += dailyPivotDataList.Count(x => (info.GetValue(x) as double?).HasValue);
                info.SetValue(totalRow, valD);

                valD = dailyPivotDataList.Average(x => info.GetValue(x) as double?);
                info.SetValue(avgRow, valD);
                //dailyPivotDataList.RemoveAll(x => info.GetValue(x).Equals(double.NaN));

                double? min = dailyPivotDataList.Min(x => info.GetValue(x) as double?);
                info.SetValue(minRow, min);
                minRow.dayCount++;

                double? max = dailyPivotDataList.Max(x => info.GetValue(x) as double?);
                info.SetValue(maxRow, max);
                maxRow.dayCount++;

                double? win = dailyPivotDataList.Select(x => info.GetValue(x) as double?).Where(x => x.HasValue && x.Value > 0).Count();
                double? valD2 = dailyPivotDataList.Count(x => (info.GetValue(x) as double?).HasValue);
                win = (win / valD2) * 100;
                info.SetValue(winRow, win);

                double? risk = GetRisk(min, max, win / 100);
                info.SetValue(riskRow, risk);
                riskRow.dayCount++;
            }

            avgRow.Total = (double)Sum_alldayvalue / Count_valuedays;
            if (ShowSummaryTotal)
                summaryList.Add(totalRow);

            if (ShowSummaryAvg)
                summaryList.Add(avgRow);

            if (ShowSummaryMin)
                summaryList.Add(minRow);
            if (ShowSummaryMax)
                summaryList.Add(maxRow);

            if (ShowSummaryWin)
                summaryList.Add(winRow);

            if (ShowSummaryRisk)
                summaryList.Add(riskRow);
            //if (minRow.dayCount != 0)
            //    minRow.Average = minRow.Total / minRow.dayCount;
            //if (maxRow.dayCount != 0)
            //    maxRow.Average = maxRow.Total / maxRow.dayCount;
            if (riskRow.dayCount != 0)
                riskRow.Average = riskRow.Total / riskRow.dayCount;

            //AssignDisplayTypeRowName(summaryList, false);
            DailySummaryPivotList = new ObservableCollection<DailyPivotData>(summaryList);
        }

        private ObservableCollection<DailyPivotData> Filter(ObservableCollection<DailyPivotData> mDailyFinalNonCrrPivotDataList, ObservableCollection<DailyPivotData> mDailyFinalPivotDataList, string Product, string Type, double? min, double? max)
        {
            ObservableCollection<DailyPivotData> mDailyPivotDataList = new ObservableCollection<DailyPivotData>();
            bool convalue = false;
            foreach (DailyPivotData itemDailyPivotData in mDailyFinalPivotDataList)
            {
                DailyPivotData mDailyPivotData = new DailyPivotData();
                mDailyPivotData.ClassType = "OffPeak";
                Type typ = typeof(DailyPivotData);
                for (int i = 1; i <= 31; i++)
                {
                    PropertyInfo info = typ.GetProperty("D" + i);
                    info.GetValue(itemDailyPivotData);
                    if (info.GetValue(itemDailyPivotData) != null)
                    {
                        double? value = Convert.ToDouble(info.GetValue(itemDailyPivotData));
                        // if (value > max || value < min)
                        double maxi = double.MaxValue;
                        double mini = double.MinValue;
                        if (max != maxi)
                        {
                            if (value < max)
                            {
                                if (value.HasValue)
                                {
                                    info.SetValue(mDailyPivotData, value);
                                    convalue = true;
                                }
                            }
                        }
                        if (min != mini)
                        {
                            if (value > min)
                            {
                                if (value.HasValue)
                                {
                                    info.SetValue(mDailyPivotData, value);
                                    convalue = true;
                                }
                            }
                        }



                    }
                }
                if (convalue)
                {
                    if (mDailyFinalNonCrrPivotDataList.Count > 0)
                    {
                        ObservableCollection<DailyPivotData> mSelectValue = new ObservableCollection<DailyPivotData>(mDailyFinalNonCrrPivotDataList.Where(x => x.DateDisplay == itemDailyPivotData.DateDisplay));
                        foreach (DailyPivotData item in mSelectValue)
                        {
                            DailyPivotData mDailyPivot = new DailyPivotData();
                            Type typ1 = typeof(DailyPivotData);
                            for (int i = 1; i <= 31; i++)
                            {
                                PropertyInfo info = typ1.GetProperty("D" + i);
                                info.GetValue(item);
                                if (info.GetValue(item) != null)
                                {
                                    info.SetValue(mDailyPivot, Convert.ToDouble(info.GetValue(item)));
                                }
                            }
                            mDailyPivot.DateDisplay = item.DateDisplay;
                            mDailyPivot.Average = item.Average;
                            mDailyPivot.RowDisplayType = item.RowDisplayType;
                            mDailyPivot.SummaryType = item.SummaryType;
                            mDailyPivot.Total = item.Total;
                            mDailyPivotDataList.Add(mDailyPivot);
                            convalue = false;
                        }
                    }
                    else
                    {
                        mDailyPivotData.DateDisplay = itemDailyPivotData.DateDisplay;
                        mDailyPivotData.Average = itemDailyPivotData.Average;
                        mDailyPivotData.RowDisplayType = itemDailyPivotData.RowDisplayType;
                        mDailyPivotData.SummaryType = itemDailyPivotData.SummaryType;
                        mDailyPivotData.Total = itemDailyPivotData.Total;
                        mDailyPivotDataList.Add(mDailyPivotData);
                        convalue = false;
                    }
                }
            }

            return mDailyPivotDataList;
        }

        private void Connect()
        {
            TcpTransportBindingElement transport = new TcpTransportBindingElement();
            transport.TransferMode = TransferMode.Streamed;
            BinaryMessageEncodingBindingElement encoder = new BinaryMessageEncodingBindingElement();
            CustomBinding binding = new CustomBinding(encoder, transport);
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.CloseTimeout = new TimeSpan(1, 0, 0);
            myBinding.OpenTimeout = new TimeSpan(1, 0, 0);
            myBinding.SendTimeout = new TimeSpan(1, 0, 0);
            myBinding.ReceiveTimeout = new TimeSpan(1, 0, 0);
            myBinding.TransactionFlow = false;
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.Security.Mode = SecurityMode.None;
            myBinding.TransferMode = TransferMode.Buffered;
            myBinding.ReaderQuotas.MaxArrayLength = 5000000;
            ChannelFactory<ISourceSink> pipeFactory = new ChannelFactory<ISourceSink>(myBinding, new EndpointAddress(mEndPoint1));
            foreach (OperationDescription op in pipeFactory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior =
                            op.Behaviors.Find<DataContractSerializerOperationBehavior>()
                            as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
            try
            {
                CrrCalculationLibrary = pipeFactory.CreateChannel();
            }
            catch (Exception ex)
            {
            }
        }


        public async Task<ObservableCollection<FTRMonthlyData>> GetCrrServiceData(List<FTRBid> ftrBids)
        {
            List<int> selectedMonths = Months
                .Where(month => month.IsChecked)
                .Select(month => DateTime.ParseExact(month.Name, "MMM", CultureInfo.InvariantCulture).Month)
                .ToList();

            Connect();
            ObservableCollection<FTRMonthlyData> mMonthlyDataList = new ObservableCollection<FTRMonthlyData>();
            List<Task> tasks = new List<Task>();

            foreach (var ftrBid in ftrBids)
            {
                long sourceNodeKey = ftrBid.SourceNodekey;
                long sinkNodeKey = ftrBid.SinkNodekey;
                string hedgeType = ftrBid.HedgeType;
                string classType = ftrBid.ClassType;
                string source = ftrBid.Source;
                string sink = ftrBid.Sink;
                string sourcezone = ftrBid.SourceZone;
                string sinkzone = ftrBid.SinkZone;
                double price1 = (double)ftrBid.Price1;
                double mw1 = (double)ftrBid.MW1;


                tasks.Add(Task.Run(() =>
                {
                    List<MonthlyValues> localYearHash;
                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        localYearHash = CrrCalculationLibrary.FillFTRPathData1(9, sourceNodeKey, sinkNodeKey, StartDate, EndDate, selectedMonths, hedgeType, classType);
                    }
                    else
                    {
                        localYearHash = CrrCalculationLibrary.FillFTRPathData1(1, sourceNodeKey, sinkNodeKey, StartDate, EndDate, selectedMonths, hedgeType, classType);
                    }

                    lock (mMonthlyDataList)
                    {
                        foreach (var monthlyValues in localYearHash)
                        {
                            var ftrMonthlyData = new FTRMonthlyData
                            {
                                PricePeakYear1Month1 = monthlyValues.PricePeakYear1Month1 ?? 0,
                                PricePeakYear1Month2 = monthlyValues.PricePeakYear1Month2 ?? 0,
                                PricePeakYear1Month3 = monthlyValues.PricePeakYear1Month3 ?? 0,
                                PricePeakYear2Month1 = monthlyValues.PricePeakYear2Month1 ?? 0,
                                PricePeakYear2Month2 = monthlyValues.PricePeakYear2Month2 ?? 0,
                                PricePeakYear2Month3 = monthlyValues.PricePeakYear2Month3 ?? 0,
                                PricePeakYear3Month1 = monthlyValues.PricePeakYear3Month1 ?? 0,
                                PricePeakYear3Month2 = monthlyValues.PricePeakYear3Month2 ?? 0,
                                PricePeakYear3Month3 = monthlyValues.PricePeakYear3Month3 ?? 0,

                                PriceOffPeakYear1Month1 = monthlyValues.PriceOffPeakYear1Month1 ?? 0,
                                PriceOffPeakYear1Month2 = monthlyValues.PriceOffPeakYear1Month2 ?? 0,
                                PriceOffPeakYear1Month3 = monthlyValues.PriceOffPeakYear1Month3 ?? 0,
                                PriceOffPeakYear2Month1 = monthlyValues.PriceOffPeakYear2Month1 ?? 0,
                                PriceOffPeakYear2Month2 = monthlyValues.PriceOffPeakYear2Month2 ?? 0,
                                PriceOffPeakYear2Month3 = monthlyValues.PriceOffPeakYear2Month3 ?? 0,
                                PriceOffPeakYear3Month1 = monthlyValues.PriceOffPeakYear3Month1 ?? 0,
                                PriceOffPeakYear3Month2 = monthlyValues.PriceOffPeakYear3Month2 ?? 0,
                                PriceOffPeakYear3Month3 = monthlyValues.PriceOffPeakYear3Month3 ?? 0,

                                PriceWEYear1Month1 = monthlyValues.PriceWEYear1Month1 ?? 0,
                                PriceWEYear1Month2 = monthlyValues.PriceWEYear1Month2 ?? 0,
                                PriceWEYear1Month3 = monthlyValues.PriceWEYear1Month3 ?? 0,
                                PriceWEYear2Month1 = monthlyValues.PriceWEYear2Month1 ?? 0,
                                PriceWEYear2Month2 = monthlyValues.PriceWEYear2Month2 ?? 0,
                                PriceWEYear2Month3 = monthlyValues.PriceWEYear2Month3 ?? 0,
                                PriceWEYear3Month1 = monthlyValues.PriceWEYear3Month1 ?? 0,
                                PriceWEYear3Month2 = monthlyValues.PriceWEYear3Month2 ?? 0,
                                PriceWEYear3Month3 = monthlyValues.PriceWEYear3Month3 ?? 0,

                                DAPeakCongYear1Month1 = monthlyValues.DAPeakCongYear1Month1 ?? 0,
                                DAPeakCongYear1Month2 = monthlyValues.DAPeakCongYear1Month2 ?? 0,
                                DAPeakCongYear1Month3 = monthlyValues.DAPeakCongYear1Month3 ?? 0,
                                DAPeakCongYear2Month1 = monthlyValues.DAPeakCongYear2Month1 ?? 0,
                                DAPeakCongYear2Month2 = monthlyValues.DAPeakCongYear2Month2 ?? 0,
                                DAPeakCongYear2Month3 = monthlyValues.DAPeakCongYear2Month3 ?? 0,
                                DAPeakCongYear3Month1 = monthlyValues.DAPeakCongYear3Month1 ?? 0,
                                DAPeakCongYear3Month2 = monthlyValues.DAPeakCongYear3Month2 ?? 0,
                                DAPeakCongYear3Month3 = monthlyValues.DAPeakCongYear3Month3 ?? 0,

                                DAOffPeakCongYear1Month1 = monthlyValues.DAOffPeakCongYear1Month1 ?? 0,
                                DAOffPeakCongYear1Month2 = monthlyValues.DAOffPeakCongYear1Month2 ?? 0,
                                DAOffPeakCongYear1Month3 = monthlyValues.DAOffPeakCongYear1Month3 ?? 0,
                                DAOffPeakCongYear2Month1 = monthlyValues.DAOffPeakCongYear2Month1 ?? 0,
                                DAOffPeakCongYear2Month2 = monthlyValues.DAOffPeakCongYear2Month2 ?? 0,
                                DAOffPeakCongYear2Month3 = monthlyValues.DAOffPeakCongYear2Month3 ?? 0,
                                DAOffPeakCongYear3Month1 = monthlyValues.DAOffPeakCongYear3Month1 ?? 0,
                                DAOffPeakCongYear3Month2 = monthlyValues.DAOffPeakCongYear3Month2 ?? 0,
                                DAOffPeakCongYear3Month3 = monthlyValues.DAOffPeakCongYear3Month3 ?? 0,

                                DAPeakWECongYear1Month1 = monthlyValues.DAPeakWECongYear1Month1 ?? 0,
                                DAPeakWECongYear1Month2 = monthlyValues.DAPeakWECongYear1Month2 ?? 0,
                                DAPeakWECongYear1Month3 = monthlyValues.DAPeakWECongYear1Month3 ?? 0,
                                DAPeakWECongYear2Month1 = monthlyValues.DAPeakWECongYear2Month1 ?? 0,
                                DAPeakWECongYear2Month2 = monthlyValues.DAPeakWECongYear2Month2 ?? 0,
                                DAPeakWECongYear2Month3 = monthlyValues.DAPeakWECongYear2Month3 ?? 0,
                                DAPeakWECongYear3Month1 = monthlyValues.DAPeakWECongYear3Month1 ?? 0,
                                DAPeakWECongYear3Month2 = monthlyValues.DAPeakWECongYear3Month2 ?? 0,
                                DAPeakWECongYear3Month3 = monthlyValues.DAPeakWECongYear3Month3 ?? 0,

                                Source = source,
                                Sink = sink,
                                SourceZone = sourcezone,
                                SinkZone = sinkzone,
                                HedgeType = hedgeType,
                                ClassType = classType,
                                MW1 = mw1,
                                Price1 = price1
                            };

                            // Set CRR and DA based on classType
                            if (classType == "OFF-PEAK")
                            {
                                ftrMonthlyData.CRRYear1Month1 = monthlyValues.PriceOffPeakYear1Month1.HasValue ? (decimal)monthlyValues.PriceOffPeakYear1Month1.Value : 0m;
                                ftrMonthlyData.CRRYear1Month2 = monthlyValues.PriceOffPeakYear1Month2.HasValue ? (decimal)monthlyValues.PriceOffPeakYear1Month2.Value : 0m;
                                ftrMonthlyData.CRRYear1Month3 = monthlyValues.PriceOffPeakYear1Month3.HasValue ? (decimal)monthlyValues.PriceOffPeakYear1Month3.Value : 0m;
                                ftrMonthlyData.CRRYear2Month1 = monthlyValues.PriceOffPeakYear2Month1.HasValue ? (decimal)monthlyValues.PriceOffPeakYear2Month1.Value : 0m;
                                ftrMonthlyData.CRRYear2Month2 = monthlyValues.PriceOffPeakYear2Month2.HasValue ? (decimal)monthlyValues.PriceOffPeakYear2Month2.Value : 0m;
                                ftrMonthlyData.CRRYear2Month3 = monthlyValues.PriceOffPeakYear2Month3.HasValue ? (decimal)monthlyValues.PriceOffPeakYear2Month3.Value : 0m;
                                ftrMonthlyData.CRRYear3Month1 = monthlyValues.PriceOffPeakYear3Month1.HasValue ? (decimal)monthlyValues.PriceOffPeakYear3Month1.Value : 0m;
                                ftrMonthlyData.CRRYear3Month2 = monthlyValues.PriceOffPeakYear3Month2.HasValue ? (decimal)monthlyValues.PriceOffPeakYear3Month2.Value : 0m;
                                ftrMonthlyData.CRRYear3Month3 = monthlyValues.PriceOffPeakYear3Month3.HasValue ? (decimal)monthlyValues.PriceOffPeakYear3Month3.Value : 0m;
                                ftrMonthlyData.DAYear1Month1 = monthlyValues.DAOffPeakCongYear1Month1.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear1Month1.Value : 0m;
                                ftrMonthlyData.DAYear1Month2 = monthlyValues.DAOffPeakCongYear1Month2.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear1Month2.Value : 0m;
                                ftrMonthlyData.DAYear1Month3 = monthlyValues.DAOffPeakCongYear1Month3.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear1Month3.Value : 0m;
                                ftrMonthlyData.DAYear2Month1 = monthlyValues.DAOffPeakCongYear2Month1.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear2Month1.Value : 0m;
                                ftrMonthlyData.DAYear2Month2 = monthlyValues.DAOffPeakCongYear2Month2.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear2Month2.Value : 0m;
                                ftrMonthlyData.DAYear2Month3 = monthlyValues.DAOffPeakCongYear2Month3.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear2Month3.Value : 0m;
                                ftrMonthlyData.DAYear3Month1 = monthlyValues.DAOffPeakCongYear3Month1.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear3Month1.Value : 0m;
                                ftrMonthlyData.DAYear3Month2 = monthlyValues.DAOffPeakCongYear3Month2.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear3Month2.Value : 0m;
                                ftrMonthlyData.DAYear3Month3 = monthlyValues.DAOffPeakCongYear3Month3.HasValue ? (decimal)monthlyValues.DAOffPeakCongYear3Month3.Value : 0m;

                            }
                            else if (classType == "PEAKWD")
                            {
                                //ftrMonthlyData.CRR = monthlyValues.PricePeak.HasValue ? (decimal)monthlyValues.PricePeak.Value : 0m;
                                //ftrMonthlyData.DA = monthlyValues.DAPeakCong.HasValue ? (decimal)monthlyValues.DAPeakCong.Value : 0m;

                                ftrMonthlyData.CRRYear1Month1 = monthlyValues.PricePeakYear1Month1.HasValue ? (decimal)monthlyValues.PricePeakYear1Month1.Value : 0m;
                                ftrMonthlyData.CRRYear1Month2 = monthlyValues.PricePeakYear1Month2.HasValue ? (decimal)monthlyValues.PricePeakYear1Month2.Value : 0m;
                                ftrMonthlyData.CRRYear1Month3 = monthlyValues.PricePeakYear1Month3.HasValue ? (decimal)monthlyValues.PricePeakYear1Month3.Value : 0m;
                                ftrMonthlyData.CRRYear2Month1 = monthlyValues.PricePeakYear2Month1.HasValue ? (decimal)monthlyValues.PricePeakYear2Month1.Value : 0m;
                                ftrMonthlyData.CRRYear2Month2 = monthlyValues.PricePeakYear2Month2.HasValue ? (decimal)monthlyValues.PricePeakYear2Month2.Value : 0m;
                                ftrMonthlyData.CRRYear2Month3 = monthlyValues.PricePeakYear2Month3.HasValue ? (decimal)monthlyValues.PricePeakYear2Month3.Value : 0m;
                                ftrMonthlyData.CRRYear3Month1 = monthlyValues.PricePeakYear3Month1.HasValue ? (decimal)monthlyValues.PricePeakYear3Month1.Value : 0m;
                                ftrMonthlyData.CRRYear3Month2 = monthlyValues.PricePeakYear3Month2.HasValue ? (decimal)monthlyValues.PricePeakYear3Month2.Value : 0m;
                                ftrMonthlyData.CRRYear3Month3 = monthlyValues.PricePeakYear3Month3.HasValue ? (decimal)monthlyValues.PricePeakYear3Month3.Value : 0m;
                                ftrMonthlyData.DAYear1Month1 = monthlyValues.DAPeakCongYear1Month1.HasValue ? (decimal)monthlyValues.DAPeakCongYear1Month1.Value : 0m;
                                ftrMonthlyData.DAYear1Month2 = monthlyValues.DAPeakCongYear1Month2.HasValue ? (decimal)monthlyValues.DAPeakCongYear1Month2.Value : 0m;
                                ftrMonthlyData.DAYear1Month3 = monthlyValues.DAPeakCongYear1Month3.HasValue ? (decimal)monthlyValues.DAPeakCongYear1Month3.Value : 0m;
                                ftrMonthlyData.DAYear2Month1 = monthlyValues.DAPeakCongYear2Month1.HasValue ? (decimal)monthlyValues.DAPeakCongYear2Month1.Value : 0m;
                                ftrMonthlyData.DAYear2Month2 = monthlyValues.DAPeakCongYear2Month2.HasValue ? (decimal)monthlyValues.DAPeakCongYear2Month2.Value : 0m;
                                ftrMonthlyData.DAYear2Month3 = monthlyValues.DAPeakCongYear2Month3.HasValue ? (decimal)monthlyValues.DAPeakCongYear2Month3.Value : 0m;
                                ftrMonthlyData.DAYear3Month1 = monthlyValues.DAPeakCongYear3Month1.HasValue ? (decimal)monthlyValues.DAPeakCongYear3Month1.Value : 0m;
                                ftrMonthlyData.DAYear3Month2 = monthlyValues.DAPeakCongYear3Month2.HasValue ? (decimal)monthlyValues.DAPeakCongYear3Month2.Value : 0m;
                                ftrMonthlyData.DAYear3Month3 = monthlyValues.DAPeakCongYear3Month3.HasValue ? (decimal)monthlyValues.DAPeakCongYear3Month3.Value : 0m;

                            }
                            else if (classType == "PEAKWE")
                            {
                                //ftrMonthlyData.CRR = monthlyValues.PriceWE.HasValue ? (decimal)monthlyValues.PriceWE.Value : 0m;
                                //ftrMonthlyData.DA = monthlyValues.DAPeakWECong.HasValue ? (decimal)monthlyValues.DAPeakWECong.Value : 0m;

                                ftrMonthlyData.CRRYear1Month1 = monthlyValues.PriceWEYear1Month1.HasValue ? (decimal)monthlyValues.PriceWEYear1Month1.Value : 0m;
                                ftrMonthlyData.CRRYear1Month2 = monthlyValues.PriceWEYear1Month2.HasValue ? (decimal)monthlyValues.PriceWEYear1Month2.Value : 0m;
                                ftrMonthlyData.CRRYear1Month3 = monthlyValues.PriceWEYear1Month3.HasValue ? (decimal)monthlyValues.PriceWEYear1Month3.Value : 0m;
                                ftrMonthlyData.CRRYear2Month1 = monthlyValues.PriceWEYear2Month1.HasValue ? (decimal)monthlyValues.PriceWEYear2Month1.Value : 0m;
                                ftrMonthlyData.CRRYear2Month2 = monthlyValues.PriceWEYear2Month2.HasValue ? (decimal)monthlyValues.PriceWEYear2Month2.Value : 0m;
                                ftrMonthlyData.CRRYear2Month3 = monthlyValues.PriceWEYear2Month3.HasValue ? (decimal)monthlyValues.PriceWEYear2Month3.Value : 0m;
                                ftrMonthlyData.CRRYear3Month1 = monthlyValues.PriceWEYear3Month1.HasValue ? (decimal)monthlyValues.PriceWEYear3Month1.Value : 0m;
                                ftrMonthlyData.CRRYear3Month2 = monthlyValues.PriceWEYear3Month2.HasValue ? (decimal)monthlyValues.PriceWEYear3Month2.Value : 0m;
                                ftrMonthlyData.CRRYear3Month3 = monthlyValues.PriceWEYear3Month3.HasValue ? (decimal)monthlyValues.PriceWEYear3Month3.Value : 0m;
                                ftrMonthlyData.DAYear1Month1 = monthlyValues.DAPeakWECongYear1Month1.HasValue ? (decimal)monthlyValues.DAPeakWECongYear1Month1.Value : 0m;
                                ftrMonthlyData.DAYear1Month2 = monthlyValues.DAPeakWECongYear1Month2.HasValue ? (decimal)monthlyValues.DAPeakWECongYear1Month2.Value : 0m;
                                ftrMonthlyData.DAYear1Month3 = monthlyValues.DAPeakWECongYear1Month3.HasValue ? (decimal)monthlyValues.DAPeakWECongYear1Month3.Value : 0m;
                                ftrMonthlyData.DAYear2Month1 = monthlyValues.DAPeakWECongYear2Month1.HasValue ? (decimal)monthlyValues.DAPeakWECongYear2Month1.Value : 0m;
                                ftrMonthlyData.DAYear2Month2 = monthlyValues.DAPeakWECongYear2Month2.HasValue ? (decimal)monthlyValues.DAPeakWECongYear2Month2.Value : 0m;
                                ftrMonthlyData.DAYear2Month3 = monthlyValues.DAPeakWECongYear2Month3.HasValue ? (decimal)monthlyValues.DAPeakWECongYear2Month3.Value : 0m;
                                ftrMonthlyData.DAYear3Month1 = monthlyValues.DAPeakWECongYear3Month1.HasValue ? (decimal)monthlyValues.DAPeakWECongYear3Month1.Value : 0m;
                                ftrMonthlyData.DAYear3Month2 = monthlyValues.DAPeakWECongYear3Month2.HasValue ? (decimal)monthlyValues.DAPeakWECongYear3Month2.Value : 0m;
                                ftrMonthlyData.DAYear3Month3 = monthlyValues.DAPeakWECongYear3Month3.HasValue ? (decimal)monthlyValues.DAPeakWECongYear3Month3.Value : 0m;

                            }

                            mMonthlyDataList.Add(ftrMonthlyData);

                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);


            return mMonthlyDataList;
        }


        private void ExportCSV()
        {
            SaveFileDialog savefiledialog = new SaveFileDialog();
            savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            savefiledialog.FilterIndex = 1;
            savefiledialog.RestoreDirectory = true;
            savefiledialog.FileName = "PathWise_Stat_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            if ((bool)savefiledialog.ShowDialog())
            {
                StringBuilder builder = new StringBuilder();
                if (PathwistStatList.Count > 0)
                {
                    foreach (PropertyInfo item in PathwistStatList[0].GetType().GetProperties())
                    {
                        builder.Append(item.Name + ",");
                    }
                    builder.ToString().Remove(builder.Length - 1, 1);
                    builder.AppendLine();

                    foreach (PathwiseCalculationHelper item in PathwistStatList)
                    {
                        foreach (PropertyInfo propName in item.GetType().GetProperties())
                        {
                            builder.Append((item.GetType().GetProperty(propName.Name).GetValue(item) == null ? "" : item.GetType().GetProperty(propName.Name).GetValue(item).ToString().Replace(',', '-')) + ",");
                        }
                        builder.AppendLine();
                    }
                }
                try
                {
                    TextWriter writer = new StreamWriter(savefiledialog.FileName);
                    writer.Write(builder.ToString());
                    writer.Flush();
                    writer.Close();
                    MessageBox.Show("Successfully created the file");
                }
                catch
                {
                    MessageBox.Show("Failed to do so!!");
                }
            }
        }

        /// <summary>
        /// Seasons this instance.
        /// </summary>
        public void Season()
        {
            StartDate = DateTime.Today.AddYears(-1).AddMonths(-1);
            EndDate = DateTime.Today.AddYears(-1).AddMonths(1);
        }

        private void CalcluatePFAnalysis()
        {
            string s = "";

            if (PathList == null || PathList.Count == 0)
            {
                MessageBox.Show("No Valid paths Selected");
                return;
            }
            DataService ds = new DataService();

            //   Dictionary<int, DateTime> minDaDateDict = ds.GetMinDaDates(GetMarketKey());

            DateTime startDate = PFAnalyserStartDateSelected.AddDays(-(PFAnalyserStartDateSelected.Day - 1));
            DateTime endDate = PFAnalyserEndDateSelected.AddDays(-(PFAnalyserEndDateSelected.Day - 1)); ;

            DateTime enddate2 = PFAnalyserEndDateSelected.AddDays(-(PFAnalyserEndDateSelected.Day - PFAnalyserEndDateSelected.Day));

            List<PortfolioAnalyserHelper> PFApathlist = new List<PortfolioAnalyserHelper>();
            string pathlist_string = null;
            PathHelper pathhelper = new PathHelper();
            PathHelper crrpathhelper = new PathHelper();
            DateTime d1 = new DateTime(2021, 9, 1);

            List<string> sourcenodekeylist = new List<string>();
            List<string> sinknodekeylist = new List<string>();

            foreach (FTRBid FTRBid in PathList)
            {
                FTRBid path = new FTRBid(FTRBid);
                PortfolioAnalyserHelper PFAnalyserobj = new PortfolioAnalyserHelper();
                PFAnalyserobj.Path_analyser_Source = FTRBid.Source;
                PFAnalyserobj.Path_analyser_Sink = FTRBid.Sink;
                PFAnalyserobj.Path_analyser_Class_type = FTRBid.ClassType;
                PFAnalyserobj.Path_analyser_Hedge_type = FTRBid.HedgeType;
                PFAnalyserobj.Path_analyser_Source_Zone = FTRBid.SourceZone;
                PFAnalyserobj.Path_analyser_Sink_Zone = FTRBid.SinkZone;
                pathlist_string = pathlist_string + ",'" + FTRBid.Source + "','" + FTRBid.Sink + "'";

                //if (FTRBid.SourceNodekey == 2967 && FTRBid.SinkNodekey == 96)//sourcematch -ercot
                {
                    if (!sNodeHash.ContainsKey(FTRBid.SourceExternalid))
                        sNodeHash.Add(FTRBid.SourceExternalid, FTRBid.SourceNodekey);

                    if (!sNodeHash.ContainsKey(FTRBid.SinkExternalid))
                        sNodeHash.Add(FTRBid.SinkExternalid, FTRBid.SinkNodekey);

                    if (!validCRRNodeDict.ContainsKey(FTRBid.SourceExternalid))
                        validCRRNodeDict.Add(FTRBid.SourceExternalid, FTRBid.Source);

                    if (!validCRRNodeDict.ContainsKey(FTRBid.SinkExternalid))
                        validCRRNodeDict.Add(FTRBid.SinkExternalid, FTRBid.Sink);




                    if (!sNodeHashCRR.ContainsKey(FTRBid.Source))
                        sNodeHashCRR.Add(FTRBid.Source, FTRBid.SourceNodekey);

                    if (!sNodeHashCRR.ContainsKey(FTRBid.Sink))
                        sNodeHashCRR.Add(FTRBid.Sink, FTRBid.SinkNodekey);


                    if (!validCrrNodeDict.ContainsKey(FTRBid.SourceNodekey))
                        validCrrNodeDict.Add(FTRBid.SourceNodekey, FTRBid.Source);

                    if (!validCrrNodeDict.ContainsKey(FTRBid.SinkNodekey))
                        validCrrNodeDict.Add(FTRBid.SinkNodekey, FTRBid.Sink);


                    if ((!sourcenodekeylist.Contains(FTRBid.SourceNodekey.ToString())))
                        sourcenodekeylist.Add(FTRBid.SourceNodekey.ToString());

                    if ((!sinknodekeylist.Contains(FTRBid.SinkNodekey.ToString())))
                        sinknodekeylist.Add(FTRBid.SinkNodekey.ToString());

                    pathhelper = new PathHelper();
                    pathhelper.SourceKey = FTRBid.SourceExternalid;
                    pathhelper.SinkKey = FTRBid.SinkExternalid;
                    ValidCRRPathList.Add(pathhelper);

                    crrpathhelper = new PathHelper();
                    crrpathhelper.SourceKey = FTRBid.SourceNodekey;
                    crrpathhelper.SinkKey = FTRBid.SinkNodekey;
                    crrpathhelper.HedgeType = FTRBid.HedgeType;
                    ValidCrrPathList.Add(crrpathhelper);

                }
                PFApathlist.Add(PFAnalyserobj);
            }

            Portfolio_analyser_PathList = PFApathlist;



            string srclist, sinklist;

            srclist = string.Join(",", sourcenodekeylist.ToArray());
            sinklist = string.Join(",", sinknodekeylist.ToArray());

            dictPeakYn_datewise = _dataService.GetPeakYN(startDate, enddate2);
            string market = MarketComboSelectedValue; ;
            // RTDAMinDates = _dataService.getRTDAMinDate(market, srclist, sinklist);

            SourceRTDAMinDates = _dataService.getRTDAMinDate(market, srclist, "Source");
            SinkRTDAMinDates = _dataService.getRTDAMinDate(market, sinklist, "Sink");


            List<NodePriceHelper> CostList = _dataService.GetallCosts(endDate, startDate);
            List<int> periodKeyList = CostList.Select(a => a.PeriodKey).ToList();

            List<PathHelper> tempList = null;
            if (MarketComboSelectedValue == "ERCOT")
                tempList = ValidCrrPathList.ToList();

            while (tempList.Count > 0)
            {
                List<CRRAlgoHelper> algoResultList = new List<CRRAlgoHelper>();
                List<PathHelper> tempList1 = tempList.OrderBy(i => i.SourceKey).Take(30).ToList();

                //
                periodKeyList = periodKeyList.Distinct().ToList();


                foreach (PathHelper path in tempList1)
                {
                    try
                    {
                        if (MarketComboSelectedValue == "ERCOT")
                        {

                            long sourceKey = path.SourceKey;
                            long sinkKey = (long)path.SinkKey;
                            CRRAlgoHelper helper = new CRRAlgoHelper();
                            helper.Source = validCrrNodeDict[sourceKey];
                            helper.Sink = validCrrNodeDict[sinkKey];
                            helper.Hedgetype = path.HedgeType;
                            algoResultList.Add(helper);

                        }
                    }
                    catch (Exception ex)
                    {


                    }
                }

                List<CRRAlgoHelper> lst = CalculateHistPer(CostList, algoResultList, periodKeyList);



                foreach (PathHelper path in tempList1)
                {
                    bool isPresent = tempList.Exists(a => a.SourceKey == path.SourceKey && a.SinkKey == path.SinkKey);
                    if (isPresent)
                    {
                        tempList.Remove(path);
                    }
                }

                string datekey = null, sourcekey = null, sinkkey = null, SourceRTdate = null, SourceDAdate = null, SinkRTdate = null, SinkDAdate = null, FinalRTdate = null, FinalDAdate = null;
                string[] datevalues = null;
                int res = 0;
                string monthname = null;
                foreach (PortfolioAnalyserHelper PFAnalyser in Portfolio_analyser_PathList)
                {
                    foreach (CRRAlgoHelper helper in algoResultList)
                    {
                        if ((PFAnalyser.Path_analyser_Source == helper.Source) && (PFAnalyser.Path_analyser_Sink == helper.Sink))
                        {

                            //  datekey = sNodeHashCRR[helper.Source] + "?" + sNodeHashCRR[helper.Sink];
                            sourcekey = sNodeHashCRR[helper.Source].ToString();// + "?" + sNodeHashCRR[helper.Sink];

                            if (SourceRTDAMinDates.ContainsKey(sourcekey))
                            {
                                datevalues = SourceRTDAMinDates[sourcekey].Split('?');
                                SourceRTdate = datevalues[0];
                                SourceDAdate = datevalues[1];

                            }


                            sinkkey = sNodeHashCRR[helper.Sink].ToString();// + "?" + sNodeHashCRR[helper.Sink];

                            if (SinkRTDAMinDates.ContainsKey(sinkkey))
                            {
                                datevalues = SinkRTDAMinDates[sinkkey].Split('?');
                                SinkRTdate = datevalues[0];
                                SinkDAdate = datevalues[1];

                            }
                            if (MarketComboSelectedValue == "ERCOT")
                            {
                                PFAnalyser.Path_analyser_RTMinDate = "";
                                PFAnalyser.Path_analyser_DAMinDate = "";

                            }
                            if ((PFAnalyser.Path_analyser_Class_type == "OFFPEAK") || (PFAnalyser.Path_analyser_Class_type == "OFF-PEAK"))
                            {
                                PFAnalyser.Path_analyser_DA_Max_Per_Day = helper.MaxDAOffPeak;
                                PFAnalyser.Path_analyser_DA_Min_Per_Day = helper.MinDAOffPeak;
                                PFAnalyser.Path_analyser_DA_Max_total = helper.MaxDAOffPeakTotal;
                                PFAnalyser.Path_analyser_DA_Min_total = helper.MinDAOffPeakTotal;

                                PFAnalyser.Path_analyser_RT_Max_Per_Day = helper.MaxRTOffPeak;
                                PFAnalyser.Path_analyser_RT_Min_Per_Day = helper.MinRTOffPeak;
                                PFAnalyser.Path_analyser_RT_Max_total = helper.MaxRTOffPeakTotal;
                                PFAnalyser.Path_analyser_RT_Min_total = helper.MinRTOffPeakTotal;

                                PFAnalyser.Path_analyser_CRR_Max = helper.MaxFRTOffPeakTotal;
                                PFAnalyser.Path_analyser_CRR_Min = helper.MinFRTOffPeakTotal;
                                PFAnalyser.Path_analyser_CRR_Avg = helper.AvgCRROffPeak;

                            }
                            if ((PFAnalyser.Path_analyser_Class_type == "PEAK") || (PFAnalyser.Path_analyser_Class_type == "PEAKWD"))
                            {
                                PFAnalyser.Path_analyser_DA_Max_Per_Day = helper.MaxDAPeak;
                                PFAnalyser.Path_analyser_DA_Min_Per_Day = helper.MinDAPeak;
                                PFAnalyser.Path_analyser_DA_Max_total = helper.MaxDAPeakTotal;
                                PFAnalyser.Path_analyser_DA_Min_total = helper.MinDAPeakTotal;


                                PFAnalyser.Path_analyser_RT_Max_Per_Day = helper.MaxRTPeak;
                                PFAnalyser.Path_analyser_RT_Min_Per_Day = helper.MinRTPeak;
                                PFAnalyser.Path_analyser_RT_Max_total = helper.MaxRTPeakTotal;
                                PFAnalyser.Path_analyser_RT_Min_total = helper.MinRTPeakTotal;

                                PFAnalyser.Path_analyser_CRR_Max = helper.MaxFRTPeakTotal;
                                PFAnalyser.Path_analyser_CRR_Min = helper.MinFRTPeakTotal;
                                PFAnalyser.Path_analyser_CRR_Avg = helper.AvgCRRPeak;


                            }

                            if ((PFAnalyser.Path_analyser_Class_type == "PEAKWE"))
                            {
                                PFAnalyser.Path_analyser_DA_Max_Per_Day = helper.MaxDAPeakWE;
                                PFAnalyser.Path_analyser_DA_Min_Per_Day = helper.MinDAPeakWE;
                                PFAnalyser.Path_analyser_DA_Max_total = helper.MaxDAPeakWETotal;
                                PFAnalyser.Path_analyser_DA_Min_total = helper.MinDAPeakWETotal;


                                PFAnalyser.Path_analyser_RT_Max_Per_Day = helper.MaxRTPeakWE;
                                PFAnalyser.Path_analyser_RT_Min_Per_Day = helper.MinRTPeakWE;
                                PFAnalyser.Path_analyser_RT_Max_total = helper.MaxRTPeakWETotal;
                                PFAnalyser.Path_analyser_RT_Min_total = helper.MinRTPeakWETotal;

                                PFAnalyser.Path_analyser_CRR_Max = helper.MaxFRTPeakWETotal;
                                PFAnalyser.Path_analyser_CRR_Min = helper.MinFRTPeakWETotal;

                                PFAnalyser.Path_analyser_CRR_Avg = helper.AvgCRRPeakWE;




                            }

                        }
                    }
                }
            }

        }
        public double GetMinBidPrice(long sourceKey, long sinkKey, string PeakOffPeak, List<int> periodKeyList, List<NodePriceHelper> costList)
        {
            double bidPrice = -5;
            double finalBidPrice = double.MinValue;
            bool isBreak = false;

            while (bidPrice <= 5)
            {
                int count = 0;
                int countCleared = 0;
#if DEBUGMODE
                foreach (int period in periodKeyList)
#else
                Parallel.ForEach(periodKeyList, new ParallelOptions { MaxDegreeOfParallelism = 15 }, period =>
#endif
                {
                    long sourceNodeKey = sNodeHash[sourceKey];
                    long sinkNodeKey = sNodeHash[sinkKey];
                    NodePriceHelper sourceDetails = costList.Find(a => a.PeriodKey == period && a.NodeKey == sourceNodeKey);
                    NodePriceHelper sinkDetails = costList.Find(a => a.PeriodKey == period && a.NodeKey == sinkNodeKey);
                    if (sinkDetails != null && sourceDetails != null)
                    {
                        double sourcePrice = double.MinValue;
                        double sinkPrice = double.MinValue;
                        //Source
                        try
                        {
                            if (PeakOffPeak == "PEAK")
                                sourcePrice = sourceDetails.PeakPrice / sourceDetails.PeakHrs;
                            else if (PeakOffPeak == "OFFPEAK")
                                sourcePrice = sourceDetails.OffPeakPrice / sourceDetails.OffPeakHrs;
                            //Sink
                            if (PeakOffPeak == "PEAK")
                                sinkPrice = sinkDetails.PeakPrice / sinkDetails.PeakHrs;
                            else if (PeakOffPeak == "OFFPEAK")
                                sinkPrice = sinkDetails.OffPeakPrice / sinkDetails.OffPeakHrs;
                        }
                        catch (Exception ex)
                        {

                            // continue;
                        }
                        //
                        double pathPrice = sinkPrice - sourcePrice;
                        if (pathPrice < bidPrice)
                        {
                            countCleared++;
                        }
                        count++;
                    }

#if DEBUGMODE
                }
#else
                });
#endif

                if (isBreak == true)
                {
                    isBreak = false;
                    break;
                }
                double clearedPct = (double)countCleared / count;
                if (clearedPct >= 0.8)
                {
                    finalBidPrice = bidPrice;
                    break;
                }
                bidPrice = bidPrice + 0.3;
            }
            return finalBidPrice;
        }


        List<CRRAlgoHelper> CalculateHistPer(List<NodePriceHelper> costList, List<CRRAlgoHelper> algoResultList, List<int> PeriodKeyList)
        {
            int count = 0;

            //
            string periodString = string.Empty;
            foreach (int periodkey in PeriodKeyList)
            {
                if (periodString == string.Empty)
                    periodString = periodkey.ToString();
                else
                    periodString = periodString + ',' + periodkey.ToString();
            }
            //
            List<CRRAlgoHelper> tempalgoResultList = new List<CRRAlgoHelper>();
            //   List<CRRAlgoHelper> pathList200 = new List<CRRAlgoHelper>();

#if DEBUGMODE
                        foreach (CRRAlgoHelper path in algoResultList)
#else
            Parallel.ForEach(algoResultList, new ParallelOptions { MaxDegreeOfParallelism = 15 }, path =>
#endif

            //foreach (CRRAlgoHelper path in algoResultList)
            {
                long sourceKey = 0, sinkKey = 0;
                string hedgetype = "";
                if (MarketComboSelectedValue == "ERCOT")
                {
                    sourceKey = validCrrNodeDict.FirstOrDefault(a => a.Value == path.Source).Key;
                    sinkKey = validCrrNodeDict.FirstOrDefault(a => a.Value == path.Sink).Key;
                    hedgetype = path.Hedgetype;

                }

                DateTime startDate = PFAnalyserStartDateSelected.AddDays(-(PFAnalyserStartDateSelected.Day - 1));

                DateTime endDate = PFAnalyserEndDateSelected.AddDays(-(PFAnalyserEndDateSelected.Day - PFAnalyserEndDateSelected.Day));





                DateTime lastHistTime = startDate;
                // DateTime enddatepass = endDate.AddDays(1);
                DateTime enddatepass = endDate;

                hedgetype = path.Hedgetype;
                hedgetype = path.Hedgetype;
                Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> allPriceDict = ConnCRRServer(lastHistTime, enddatepass, sourceKey, sinkKey, hedgetype);
                //  Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> allPriceDict = ConnCRRServer(lastHistTime, enddatepass, sourceKey, sinkKey);
                try
                {
                    if (allPriceDict.Count != 0)
                    {

                    }
                    // Console.WriteLine("Calculating Results for " + path.Source + "\t" + path.Sink);

                    double peakDayCount = 0; double offPeakDayCount = 0;


                    double? peakMinRT = 0; double? offPeakMinRT = 0; double? offPeakMinDa = 0; double? PeakWEMinDa = 0; double? PeakWEMinRT = 0;
                    double? peakMaxRT = 0; double? offPeakMaxRT = 0; double? offPeakMaxDa = 0; double? PeakWEMaxDa = 0; double? PeakWEMaxRT = 0;

                    double? peakMinDa = 0;
                    double? peakMaxDa = 0;

                    double monthlySumDa_max_Peak_total = 0.0;
                    double monthlySumDa_min_Peak_total = 0.0;
                    double monthlySumDa_max_offPeak_total = 0.0;
                    double monthlySumDa_min_offPeak_total = 0.0;
                    double monthlySumDa_max_min_PeakWE_total = 0.0;
                    double monthlySumRT_max_min_PeakWE_total = 0.0;

                    double monthlySumRT_max_Peak_total = 0.0;
                    double monthlySumRT_min_Peak_total = 0.0;
                    double monthlySumRT_max_offPeak_total = 0.0;
                    double monthlySumRT_min_offPeak_total = 0.0;

                    double monthlySumCRR_max_min_offpeak_total = 0.0;
                    double monthlySumCRR_max_min_peak_total = 0.0;

                    //
                    bool isBreak = false;
                    //
                    List<double> peakDaCRRList = new List<double>();
                    List<double> OffPeakDaCRRList = new List<double>();
                    List<double> annPeakDaCRRList = new List<double>();
                    List<double> annOffPeakDaCRRList = new List<double>();

                    Dictionary<DateTime, Double> Max_Min_DATotalOffPeak_Dic = new Dictionary<DateTime, double>();


                    Dictionary<DateTime, Double> Max_Min_DATotalPeak_Dic = new Dictionary<DateTime, double>();
                    Dictionary<DateTime, Double> sorted_Max_Min_DATotalPeak_Dic = new Dictionary<DateTime, double>();

                    Dictionary<DateTime, Double> Max_Min_RTTotalOffPeak_Dic = new Dictionary<DateTime, double>();
                    Dictionary<DateTime, Double> sorted_Max_Min_RTTotalOffPeak_Dic = new Dictionary<DateTime, double>();

                    Dictionary<DateTime, Double> Max_Min_RTTotalPeak_Dic = new Dictionary<DateTime, double>();
                    Dictionary<DateTime, Double> sorted_Max_Min_RTTotalPeak_Dic = new Dictionary<DateTime, double>();

                    Dictionary<DateTime, Double?> Max_Min_FRTTotalPeak_Dic = new Dictionary<DateTime, double?>();
                    Dictionary<DateTime, Double?> sorted_Max_Min_FRTTotalPeak_Dic = new Dictionary<DateTime, double?>();

                    Dictionary<DateTime, Double?> Max_Min_FRTTotalOffPeak_Dic = new Dictionary<DateTime, double?>();
                    Dictionary<DateTime, Double?> sorted_Max_Min_FRTTotalOffPeak_Dic = new Dictionary<DateTime, double?>();


                    Dictionary<DateTime, Double> Max_Min_DATotalPeakWE_Dic = new Dictionary<DateTime, double>();
                    Dictionary<DateTime, Double> Max_Min_RTTotalPeakWE_Dic = new Dictionary<DateTime, double>();

                    Dictionary<DateTime, Double?> Max_Min_FRTTotalPeakWE_Dic = new Dictionary<DateTime, double?>();
                    Dictionary<DateTime, Double?> sorted_Max_Min_FRTTotalPeakWE_Dic = new Dictionary<DateTime, double?>();



                    //
                    foreach (var Yearlyitem in allPriceDict)
                    {
                        int year = Yearlyitem.Key;
                        Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> monthlyPriceDict = Yearlyitem.Value;



                        foreach (var monthlyItem in monthlyPriceDict)
                        {

                            int month = monthlyItem.Key;
                            Tuple<DailyValues, Dictionary<int, DailyValues>> dailyTuple = monthlyItem.Value;
                            double? pathPeakPriceMonthly = dailyTuple.Item1.PricePeak;
                            double? pathOffPeakPriceMonthly = dailyTuple.Item1.PriceOffPeak;
                            double? pathPeakWEPriceMonthly = dailyTuple.Item1.PriceWE;
                            Dictionary<int, DailyValues> dailyDict = dailyTuple.Item2;

                            //  if (pathPeakPriceMonthly.HasValue && pathOffPeakPriceMonthly.HasValue)
                            {
                                // double? pathPeakCost = (double)(pathPeakPriceMonthly / dailyTuple.Item1.PeakHours);
                                //  double? pathOffPeakCost = (double)(pathOffPeakPriceMonthly / dailyTuple.Item1.OffPeakHours);
                                monthlySumDa_max_offPeak_total = monthlySumDa_max_Peak_total = 0;
                                monthlySumRT_max_offPeak_total = monthlySumRT_max_Peak_total = 0;
                                monthlySumCRR_max_min_offpeak_total = monthlySumCRR_max_min_peak_total = 0;
                                monthlySumDa_max_min_PeakWE_total = monthlySumRT_max_min_PeakWE_total = 0;

                                foreach (var dailyItem in dailyDict)
                                {
                                    int monthDay = dailyItem.Key;
                                    DailyValues dailyDet = dailyItem.Value;

                                    double? pathPeakDa = dailyDet.DAPeakCong ?? 0.0;
                                    double? pathOffPeakDa = dailyDet.DAOffPeakCong ?? 0.0;
                                    //
                                    double? pathPeakRt = dailyDet.RTPeakCong ?? 0.0;
                                    double? pathOffPeakRt = dailyDet.RTOffPeakCong ?? 0.0;

                                    double? pathPriceOffPeak = dailyDet.PriceOffPeak ?? 0.0;
                                    double? patPricePeak = dailyDet.PricePeak ?? 0.0;


                                    double? pathPeakWEDa = dailyDet.DAPeakWECong ?? 0.0;
                                    double? pathPeakWERT = dailyDet.RTPeakWECong ?? 0.0;

                                    monthlySumDa_max_offPeak_total += (double)pathOffPeakDa * dailyDet.OffPeakHours;
                                    monthlySumRT_max_offPeak_total += (double)pathOffPeakRt * dailyDet.OffPeakHours;
                                    monthlySumCRR_max_min_offpeak_total += (double)(pathPriceOffPeak / dailyDet.OffPeakHours) * dailyDet.OffPeakHours;





                                    monthlySumDa_max_Peak_total += (double)pathPeakDa * dailyDet.PeakHours;
                                    monthlySumDa_min_Peak_total += (double)pathPeakDa * dailyDet.PeakHours;
                                    monthlySumRT_max_Peak_total += (double)pathPeakRt * dailyDet.PeakHours;
                                    monthlySumRT_min_Peak_total += (double)pathPeakRt * dailyDet.PeakHours;
                                    monthlySumCRR_max_min_peak_total += (double)(patPricePeak / dailyDet.PeakHours) * dailyDet.PeakHours;


                                    DateTime currMonth = DateTime.Today.Date;




                                    #region PeakCalculations
                                    peakDayCount++;
                                    // if (pathPeakDa.HasValue)
                                    {
                                        // double pathPeakDaCRR = (double)(pathPeakDa - pathPeakCost);
                                        //

                                        if (pathPeakDa > peakMaxDa)
                                            peakMaxDa = (double)pathPeakDa;
                                        if (pathPeakDa < peakMinDa)
                                            peakMinDa = (double)pathPeakDa;
                                        //
                                        if (pathPeakRt > peakMaxRT)
                                            peakMaxRT = (double)pathPeakRt;
                                        if (pathPeakRt < peakMinRT)
                                            peakMinRT = (double)pathPeakRt;




                                    }
                                    #endregion

                                    #region OffPeakCalculations
                                    offPeakDayCount++;




                                    // if (pathOffPeakCost.HasValue)
                                    {




                                        if (pathOffPeakDa > offPeakMaxDa)
                                            offPeakMaxDa = (double)pathOffPeakDa;
                                        if (pathOffPeakDa < offPeakMinDa)
                                            offPeakMinDa = (double)pathOffPeakDa;
                                        //
                                        if (pathOffPeakRt > offPeakMaxRT)
                                            offPeakMaxRT = (double)pathOffPeakRt;
                                        if (pathOffPeakRt < offPeakMinRT)
                                            offPeakMinRT = (double)pathOffPeakRt;


                                    }
                                    #endregion




                                    #region PEAKWE
                                    DateTime curdate = Convert.ToDateTime(year + "/" + month + "/" + monthDay);
                                    string dayofweek = curdate.DayOfWeek.ToString();

                                    bool is_holiday = false;
                                    DateTime ncurdate = curdate.AddHours(8);



                                    curdate = curdate.AddHours(8);
                                    if (dictPeakYn_datewise.ContainsKey(curdate))
                                    {
                                        string peakynval = dictPeakYn_datewise[curdate];
                                        if (peakynval.ToLower() == "w")//holiday or weekend
                                        {
                                            is_holiday = true;
                                        }

                                    }


                                    if (dayofweek.ToLower() == Convert.ToString("saturday") || dayofweek.ToLower() == Convert.ToString("sunday") || is_holiday)
                                    {

                                        if (pathPeakWEDa > PeakWEMaxDa)
                                            PeakWEMaxDa = (double)pathPeakWEDa;
                                        if (pathPeakWEDa < PeakWEMinDa)
                                            PeakWEMinDa = (double)pathPeakWEDa;


                                        if (pathPeakWERT > PeakWEMaxRT)
                                            PeakWEMaxRT = (double)pathPeakWERT;
                                        if (pathPeakWEDa < PeakWEMinRT)
                                            PeakWEMaxRT = (double)pathPeakWERT;

                                        monthlySumDa_max_min_PeakWE_total += (double)pathPeakWEDa * dailyDet.PeakWEHours;
                                        monthlySumRT_max_min_PeakWE_total += (double)pathPeakWERT * dailyDet.PeakWEHours;

                                        double? d = pathPeakWEPriceMonthly;

                                    }
                                    #endregion

                                }

                                DateTime monthdate = DateTime.Parse(year.ToString() + '/' + month.ToString() + '/' + '1');

                                Max_Min_DATotalOffPeak_Dic.Add(monthdate, monthlySumDa_max_offPeak_total);
                                Max_Min_DATotalPeak_Dic.Add(monthdate, monthlySumDa_max_Peak_total);
                                Max_Min_RTTotalOffPeak_Dic.Add(monthdate, monthlySumRT_max_offPeak_total);
                                Max_Min_RTTotalPeak_Dic.Add(monthdate, monthlySumRT_max_Peak_total);
                                Max_Min_DATotalPeakWE_Dic.Add(monthdate, monthlySumDa_max_min_PeakWE_total);
                                Max_Min_RTTotalPeakWE_Dic.Add(monthdate, monthlySumRT_max_min_PeakWE_total);

                                if (MarketComboSelectedValue == "ERCOT")
                                {
                                    if (pathOffPeakPriceMonthly.HasValue)
                                    {
                                        if (!Max_Min_FRTTotalOffPeak_Dic.ContainsKey(monthdate))
                                            Max_Min_FRTTotalOffPeak_Dic.Add(monthdate, (double)(pathOffPeakPriceMonthly));
                                    }
                                    if (pathPeakPriceMonthly.HasValue)
                                    {
                                        if (!Max_Min_FRTTotalPeak_Dic.ContainsKey(monthdate))
                                            Max_Min_FRTTotalPeak_Dic.Add(monthdate, (double)(pathPeakPriceMonthly));
                                    }

                                    if (pathPeakWEPriceMonthly.HasValue)
                                    {
                                        if (!Max_Min_FRTTotalPeakWE_Dic.ContainsKey(monthdate))
                                            Max_Min_FRTTotalPeakWE_Dic.Add(monthdate, (double)(pathPeakWEPriceMonthly));
                                    }
                                }




                            }



                        }

                    }


                    if (isBreak == true)
                    {
                        tempalgoResultList.Add(path);
                        // isBreak = false;
                        // continue;
                    }

                    if (!isBreak)
                    {


                        Max_Min_DATotalOffPeak_Dic = Sort_Dictionary(Max_Min_DATotalOffPeak_Dic);
                        Max_Min_RTTotalOffPeak_Dic = Sort_Dictionary(Max_Min_RTTotalOffPeak_Dic);

                        Max_Min_DATotalPeak_Dic = Sort_Dictionary(Max_Min_DATotalPeak_Dic);
                        Max_Min_RTTotalPeak_Dic = Sort_Dictionary(Max_Min_RTTotalPeak_Dic);

                        Max_Min_DATotalPeakWE_Dic = Sort_Dictionary(Max_Min_DATotalPeakWE_Dic);
                        Max_Min_RTTotalPeakWE_Dic = Sort_Dictionary(Max_Min_RTTotalPeakWE_Dic);

                        double? avg_CRRoffpeak = 0, avg_CRRpeak = 0, avg_CRRpeakwe = 0;

                        foreach (KeyValuePair<DateTime, double?> item in Max_Min_FRTTotalOffPeak_Dic.OrderBy(key => key.Value))
                        {
                            sorted_Max_Min_FRTTotalOffPeak_Dic.Add(item.Key, item.Value);//sorted in ascending
                            avg_CRRoffpeak += item.Value;
                        }

                        foreach (KeyValuePair<DateTime, double?> item in Max_Min_FRTTotalPeak_Dic.OrderBy(key => key.Value))
                        {
                            sorted_Max_Min_FRTTotalPeak_Dic.Add(item.Key, item.Value);//sorted in ascending
                            avg_CRRpeak += item.Value;
                        }


                        foreach (KeyValuePair<DateTime, double?> item in Max_Min_FRTTotalPeakWE_Dic.OrderBy(key => key.Value))
                        {
                            sorted_Max_Min_FRTTotalPeakWE_Dic.Add(item.Key, item.Value);//sorted in ascending
                            avg_CRRpeakwe += item.Value;
                        }


                        //PEAK

                        if (peakMaxDa.HasValue)
                            path.MaxDAPeak = Math.Round(peakMaxDa.Value, 2);

                        if (peakMinDa.HasValue)
                            path.MinDAPeak = Math.Round(peakMinDa.Value, 2);

                        if (peakMaxRT.HasValue)
                            path.MaxRTPeak = Math.Round(peakMaxRT.Value, 2);

                        if (peakMinRT.HasValue)
                            path.MinRTPeak = Math.Round(peakMinRT.Value, 2);





                        //OFFPEAK

                        if (offPeakMaxDa.HasValue)
                            path.MaxDAOffPeak = Math.Round(offPeakMaxDa.Value, 2);

                        if (offPeakMinDa.HasValue)
                            path.MinDAOffPeak = Math.Round(offPeakMinDa.Value, 2);


                        if (offPeakMaxRT.HasValue)
                            path.MaxRTOffPeak = Math.Round(offPeakMaxRT.Value, 2);


                        if (offPeakMinRT.HasValue)
                            path.MinRTOffPeak = Math.Round(offPeakMinRT.Value, 2);


                        //PEAKWE

                        if (PeakWEMaxDa.HasValue)
                            path.MaxDAPeakWE = Math.Round(PeakWEMaxDa.Value, 2);

                        if (PeakWEMinDa.HasValue)
                            path.MinDAPeakWE = Math.Round(PeakWEMinDa.Value, 2);


                        if (PeakWEMaxRT.HasValue)
                            path.MaxRTPeakWE = Math.Round(PeakWEMaxRT.Value, 2);


                        if (PeakWEMinRT.HasValue)
                            path.MinRTPeakWE = Math.Round(PeakWEMinRT.Value, 2);



                        DateTime Min_valkey, Max_valkey;
                        if (Max_Min_DATotalOffPeak_Dic.Count > 0)
                        {
                            Min_valkey = Max_Min_DATotalOffPeak_Dic.Last().Key;
                            Max_valkey = Max_Min_DATotalOffPeak_Dic.First().Key;
                            path.MaxDAOffPeakTotal = Math.Round(Max_Min_DATotalOffPeak_Dic[Min_valkey], 2);
                            path.MinDAOffPeakTotal = Math.Round(Max_Min_DATotalOffPeak_Dic[Max_valkey], 2);
                        }


                        if (Max_Min_DATotalPeak_Dic.Count > 0)
                        {
                            Min_valkey = Max_Min_DATotalPeak_Dic.Last().Key;
                            Max_valkey = Max_Min_DATotalPeak_Dic.First().Key;
                            path.MaxDAPeakTotal = Math.Round(Max_Min_DATotalPeak_Dic[Min_valkey], 2);
                            path.MinDAPeakTotal = Math.Round(Max_Min_DATotalPeak_Dic[Max_valkey], 2);
                        }

                        if (Max_Min_RTTotalOffPeak_Dic.Count > 0)
                        {
                            Min_valkey = Max_Min_RTTotalOffPeak_Dic.Last().Key;
                            Max_valkey = Max_Min_RTTotalOffPeak_Dic.First().Key;
                            path.MaxRTOffPeakTotal = Math.Round(Max_Min_RTTotalOffPeak_Dic[Min_valkey], 2);
                            path.MinRTOffPeakTotal = Math.Round(Max_Min_RTTotalOffPeak_Dic[Max_valkey], 2);
                        }


                        if (Max_Min_RTTotalPeak_Dic.Count > 0)
                        {
                            Min_valkey = Max_Min_RTTotalPeak_Dic.Last().Key;
                            Max_valkey = Max_Min_RTTotalPeak_Dic.First().Key;
                            path.MaxRTPeakTotal = Math.Round(Max_Min_RTTotalPeak_Dic[Min_valkey], 2);
                            path.MinRTPeakTotal = Math.Round(Max_Min_RTTotalPeak_Dic[Max_valkey], 2);
                        }

                        if (Max_Min_DATotalPeakWE_Dic.Count > 0)
                        {
                            Min_valkey = Max_Min_DATotalPeakWE_Dic.Last().Key;
                            Max_valkey = Max_Min_DATotalPeakWE_Dic.First().Key;
                            path.MaxDAPeakWETotal = Math.Round(Max_Min_DATotalPeakWE_Dic[Min_valkey], 2);
                            path.MinDAPeakWETotal = Math.Round(Max_Min_DATotalPeakWE_Dic[Max_valkey], 2);
                        }

                        if (Max_Min_RTTotalPeakWE_Dic.Count > 0)
                        {
                            Min_valkey = Max_Min_RTTotalPeakWE_Dic.Last().Key;
                            Max_valkey = Max_Min_RTTotalPeakWE_Dic.First().Key;
                            path.MaxRTPeakWETotal = Math.Round(Max_Min_RTTotalPeakWE_Dic[Min_valkey], 2);
                            path.MinRTPeakWETotal = Math.Round(Max_Min_RTTotalPeakWE_Dic[Max_valkey], 2);
                        }



                        if (sorted_Max_Min_FRTTotalOffPeak_Dic.Count > 0)
                        {
                            Min_valkey = sorted_Max_Min_FRTTotalOffPeak_Dic.Last().Key;
                            Max_valkey = sorted_Max_Min_FRTTotalOffPeak_Dic.First().Key;
                            path.MaxFRTOffPeakTotal = Math.Round(sorted_Max_Min_FRTTotalOffPeak_Dic[Min_valkey].Value, 2);
                            path.MinFRTOffPeakTotal = Math.Round(sorted_Max_Min_FRTTotalOffPeak_Dic[Max_valkey].Value, 2);

                            avg_CRRoffpeak = avg_CRRoffpeak / sorted_Max_Min_FRTTotalOffPeak_Dic.Count;
                            path.AvgCRROffPeak = Math.Round((double)avg_CRRoffpeak, 2);

                        }


                        if (sorted_Max_Min_FRTTotalPeak_Dic.Count > 0)
                        {
                            Min_valkey = sorted_Max_Min_FRTTotalPeak_Dic.Last().Key;
                            Max_valkey = sorted_Max_Min_FRTTotalPeak_Dic.First().Key;
                            path.MaxFRTPeakTotal = Math.Round(sorted_Max_Min_FRTTotalPeak_Dic[Min_valkey].Value, 2);
                            path.MinFRTPeakTotal = Math.Round(sorted_Max_Min_FRTTotalPeak_Dic[Max_valkey].Value, 2);
                            avg_CRRpeak = avg_CRRpeak / sorted_Max_Min_FRTTotalPeak_Dic.Count;
                            path.AvgCRRPeak = Math.Round((double)avg_CRRpeak, 2);
                        }



                        if (sorted_Max_Min_FRTTotalPeakWE_Dic.Count > 0)
                        {
                            Min_valkey = sorted_Max_Min_FRTTotalPeakWE_Dic.Last().Key;
                            Max_valkey = sorted_Max_Min_FRTTotalPeakWE_Dic.First().Key;
                            path.MaxFRTPeakWETotal = Math.Round(sorted_Max_Min_FRTTotalPeakWE_Dic[Min_valkey].Value, 2);
                            path.MinFRTPeakWETotal = Math.Round(sorted_Max_Min_FRTTotalPeakWE_Dic[Max_valkey].Value, 2);
                            avg_CRRpeakwe = avg_CRRpeakwe / sorted_Max_Min_FRTTotalPeakWE_Dic.Count;

                            path.AvgCRRPeakWE = Math.Round((double)avg_CRRpeakwe, 2);
                        }

                    }

                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }



                //  }//newlyadded foreach


#if DEBUGMODE
                         }
#else
            });
#endif
            foreach (CRRAlgoHelper invalidPath in tempalgoResultList)
            {
                if (algoResultList.Exists(a => a.Source == invalidPath.Source && a.Sink == invalidPath.Sink))
                    algoResultList.Remove(invalidPath);
                Console.WriteLine("Removing Invalid paths");
            }

            return tempalgoResultList;
        }


        private Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> ConnCRRServer(DateTime startDate, DateTime endDate, long sourceExtId, long sinkExtId, string hedgetype)

        {
            Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> allPriceDict = new Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>>();
            try
            {

                TcpTransportBindingElement transport = new TcpTransportBindingElement();
                transport.TransferMode = TransferMode.Streamed;
                BinaryMessageEncodingBindingElement encoder = new BinaryMessageEncodingBindingElement();
                CustomBinding binding = new CustomBinding(encoder, transport);
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
                myBinding.OpenTimeout = new TimeSpan(0, 12, 0);
                myBinding.SendTimeout = new TimeSpan(0, 12, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                myBinding.TransactionFlow = false;
                myBinding.MaxReceivedMessageSize = int.MaxValue;
                myBinding.MaxBufferPoolSize = int.MaxValue;
                myBinding.MaxBufferSize = int.MaxValue;
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.TransferMode = TransferMode.Buffered;
                myBinding.ReaderQuotas.MaxArrayLength = 5000000;
                ChannelFactory<ISourceSink> pipeFactory = new ChannelFactory<ISourceSink>(myBinding, new EndpointAddress(CommonAccessLibrary.ServiceConnections.GetFTRService()));
                foreach (OperationDescription op in pipeFactory.Endpoint.Contract.Operations)
                {
                    DataContractSerializerOperationBehavior dataContractBehavior =
                                op.Behaviors.Find<DataContractSerializerOperationBehavior>()
                                as DataContractSerializerOperationBehavior;
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    }
                }
                try
                {
                    ISourceSink CRRCalculationLibrary = pipeFactory.CreateChannel();
                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        long srcnodekey = sourceExtId;
                        long sinknodekey = sinkExtId;
                        // endDate = endDate.AddDays(-1);
                        allPriceDict = CRRCalculationLibrary.FillFTRPathData(9, srcnodekey, sinknodekey, startDate, endDate, hedgetype, hedgetype, 0, "Monthly");

                    }

                }
                catch (Exception ex)
                {

                }

                return allPriceDict;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private Dictionary<DateTime, Double> Sort_Dictionary(Dictionary<DateTime, double> toSort_Dic)
        {
            Dictionary<DateTime, Double> sorted_Dictionary = new Dictionary<DateTime, double>();

            foreach (KeyValuePair<DateTime, double> item in toSort_Dic.OrderBy(key => key.Value))
            {
                sorted_Dictionary.Add(item.Key, item.Value);//sorted in ascending
            }


            return sorted_Dictionary;

        }


        private void CalcluatePathwistStat()
        {
            if (PathList == null || PathList.Count == 0)
            {
                MessageBox.Show("No Valid paths Selected");
                return;
            }
            DataService ds = new DataService();
            Dictionary<int, DateTime> minDaDateDict = ds.GetMinDaDates(GetMarketKey());

            List<PathwiseCalculationHelper> tempPathwistStatList = new List<PathwiseCalculationHelper>();
            DateTime startDate = StatStartDateSelected.AddDays(-(StatStartDateSelected.Day - 1));
            DateTime endDate = StatEndDateSelected.AddDays(-(StatEndDateSelected.Day - 1)); ;
            Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> costDict = _dataService.GetCost(GetMarketKey(), StartDate, EndDate, true, PathList, "ALL", true);
            Dictionary<string, Dictionary<DateTime, Cost>> monthlyCostDict = costDict["Monthly"];
            Dictionary<string, Dictionary<DateTime, Cost>> Q3CostDict = new Dictionary<string, Dictionary<DateTime, Cost>>();
            Dictionary<string, Dictionary<DateTime, Cost>> Q4CostDict = new Dictionary<string, Dictionary<DateTime, Cost>>();
            Dictionary<string, Dictionary<DateTime, Cost>> Q2CostDict = new Dictionary<string, Dictionary<DateTime, Cost>>();
            Dictionary<string, Dictionary<DateTime, Cost>> Q1CostDict = new Dictionary<string, Dictionary<DateTime, Cost>>();
            if (costDict.ContainsKey("Q1"))
                Q1CostDict = costDict["Q1"];
            if (costDict.ContainsKey("Q2"))
                Q2CostDict = costDict["Q2"];
            if (costDict.ContainsKey("Q3"))
                Q3CostDict = costDict["Q3"];
            if (costDict.ContainsKey("Q4"))
                Q4CostDict = costDict["Q4"];
            foreach (FTRBid path in PathList)
            {
                try
                {
                    DateTime sourceMinDate = minDaDateDict[path.SourceKey];
                    DateTime sinkMinDate = minDaDateDict[path.SinkKey];
                    DateTime pathMindate = DateTime.MinValue;
                    if (sourceMinDate >= sinkMinDate)
                        pathMindate = sourceMinDate;
                    else
                        pathMindate = sinkMinDate;
                    DateTime tempStartDate = startDate;
                    Dictionary<DateTime, Cost> tempPathDict = null;

                    if (path.PeriodName.Contains("Q1"))
                        tempPathDict = monthlyCostDict[path.SourceKey.ToString() + path.SinkKey.ToString()];
                    else if (path.PeriodName.Contains("Q1"))
                        tempPathDict = monthlyCostDict[path.SourceKey.ToString() + path.SinkKey.ToString()];
                    else if (path.PeriodName.Contains("Q1"))
                        tempPathDict = monthlyCostDict[path.SourceKey.ToString() + path.SinkKey.ToString()];
                    else if (path.PeriodName.Contains("Q1"))
                        tempPathDict = monthlyCostDict[path.SourceKey.ToString() + path.SinkKey.ToString()];
                    else
                        tempPathDict = monthlyCostDict[path.SourceKey.ToString() + path.SinkKey.ToString()];
                    double maxDA = double.MinValue; double minDA = double.MaxValue; double avgDa = 0.0; double totalDa = 0.0; double riskDa = 0.0; double pctWinDa = 0; int positiveDaCount = 0; double maxMonthlyDa = double.MinValue; double minMonthlyDa = double.MaxValue; int positiveDaMonthsCount = 0;
                    double maxRT = double.MinValue; double minRT = double.MaxValue; double avgRT = 0.0; double totalRT = 0.0; double riskRT = 0.0; double pctWinRt = 0; int positiveRtCount = 0; double maxMonthlyRT = double.MinValue; double minMonthlyRT = double.MaxValue; int positiveRtMonthsCount = 0;
                    double maxCRR = double.MinValue; double minCRR = double.MaxValue; double avgCRR = 0.0; double totalCRR = 0.0; double riskCRR = 0.0; double pctWinCRR = 0; int positiveCRRCount = 0;
                    double maxDACRR = double.MinValue; double minDACRR = double.MaxValue; double avgDACRR = 0.0; double totalDACRR = 0.0; double riskDACRR = 0.0; double pctWinDaCRR = 0; int positiveDaCRRCount = 0; double maxMonthlyDaCRR = double.MinValue; double minMonthlyDaCRR = double.MaxValue; int positiveDaCRRMonthsCount = 0;
                    List<double> daList = new List<double>();
                    List<double> rtList = new List<double>();
                    List<double> CRRList = new List<double>();
                    List<double> daCRRList = new List<double>();
                    int daysCount = 0; int monthsCount = 0;
                    PathwiseCalculationHelper helper = new PathwiseCalculationHelper();
                    while (tempStartDate <= endDate)
                    {
                        if (!tempPathDict.ContainsKey(tempStartDate))
                        {
                            tempStartDate = tempStartDate.AddMonths(1);
                            continue;
                        }
                        Cost pathCost = tempPathDict[tempStartDate];
                        if (pathCost.DACongestionList == null || pathCost.DACongestionList.Count == 0)
                        {
                            tempStartDate = tempStartDate.AddMonths(1);
                            continue;
                        }
                        double CRR = 0;
                        if (MarketComboSelectedValue == "ERCOT")
                        {
                            if (path.ClassType.ToLower() == "peak")
                                CRR = pathCost.Peak;
                            else
                                CRR = pathCost.OffPeak;
                        }
                        CRRList.Add(CRR);
                        double monthlyDaTotal = 0; double monthlyRtTotal = 0; double monthlyDaCRRTotal = 0;
                        foreach (DACongestion cong in pathCost.DACongestionList)
                        {
                            // DACongestion rtCongestion = pathCost.RTCongestionList.First(a => a.MarketDateTime == cong.MarketDateTime);

                            DACongestion rtCongestion = new DACongestion();
                            if (pathCost.RTCongestionList.Exists(a => a.MarketDateTime == cong.MarketDateTime))
                            {
                                rtCongestion = pathCost.RTCongestionList.First(a => a.MarketDateTime == cong.MarketDateTime);
                            }

                            int hours = 0;
                            double da = 0;
                            double rt = 0;
                            if (path.ClassType.ToLower() == "peak")
                            {
                                da = cong.Peak;
                                rt = rtCongestion.Peak;
                                hours = cong.PeakHours;
                            }

                            else
                            {
                                da = cong.OffPeak;
                                rt = rtCongestion.OffPeak;
                                hours = cong.OffPeakHours;
                            }

                            double daCRR = (da - CRR);
                            totalDa += da * hours;
                            totalRT += rt * hours;
                            totalDACRR += (da - CRR) * hours;
                            monthlyDaTotal += da * hours;
                            monthlyRtTotal += rt * hours;
                            monthlyDaCRRTotal += (da - CRR) * hours;
                            totalCRR += CRR * hours;
                            if (da > maxDA)
                                maxDA = da;
                            if (da < minDA)
                                minDA = da;
                            if (daCRR > maxDACRR)
                                maxDACRR = daCRR;
                            if (daCRR < minDACRR)
                                minDACRR = daCRR;
                            if (rt > maxRT)
                                maxRT = rt;
                            if (rt < minRT)
                                minRT = rt;
                            if (rt > 0)
                                positiveRtCount++;
                            if (da > 0)
                                positiveDaCount++;
                            if (daCRR > 0)
                                positiveDaCRRCount++;
                            daysCount++;
                        }
                        if (monthlyDaTotal > maxMonthlyDa)
                            maxMonthlyDa = monthlyDaTotal;
                        if (monthlyDaTotal < minMonthlyDa)
                            minMonthlyDa = monthlyDaTotal;
                        //
                        if (monthlyRtTotal > maxMonthlyRT)
                            maxMonthlyRT = monthlyRtTotal;
                        if (monthlyRtTotal < minMonthlyRT)
                            minMonthlyRT = monthlyRtTotal;
                        //
                        if (monthlyDaCRRTotal > maxMonthlyDaCRR)
                            maxMonthlyDaCRR = monthlyDaCRRTotal;
                        if (monthlyDaCRRTotal < minMonthlyDaCRR)
                            minMonthlyDaCRR = monthlyDaCRRTotal;
                        //
                        if (CRR > 0)
                            positiveCRRCount++;
                        if (CRR > maxCRR)
                            maxCRR = CRR;
                        if (CRR < minCRR)
                            minCRR = CRR;
                        if (monthlyDaTotal > 0)
                            positiveDaMonthsCount++;
                        if (monthlyRtTotal > 0)
                            positiveRtMonthsCount++;
                        if (monthlyDaCRRTotal > 0)
                            positiveDaCRRMonthsCount++;
                        monthsCount++;
                        //  totalCRR += CRR;
                        tempStartDate = tempStartDate.AddMonths(1);
                    }
                    helper.SourceName = path.Source;
                    helper.SinkName = path.Sink;
                    helper.SourceZone = path.SourceZone;
                    helper.SinkZone = path.SinkZone;
                    helper.ClassTYpe = path.ClassType;
                    double mw1 = Convert.ToDouble(path.MW1 == null ? 0 : path.MW1);
                    double mw2 = Convert.ToDouble(path.MW2 == null ? 0 : path.MW2);
                    double mw3 = Convert.ToDouble(path.MW3 == null ? 0 : path.MW3);
                    double mw4 = Convert.ToDouble(path.MW4 == null ? 0 : path.MW4);
                    double mw5 = Convert.ToDouble(path.MW5 == null ? 0 : path.MW5);
                    double mw6 = Convert.ToDouble(path.MW6 == null ? 0 : path.MW6);
                    double totalMw = mw1 + mw2 + mw3 + mw4 + mw5 + mw6;
                    helper.MW = (double)totalMw;
                    helper.DAHistory = pathMindate;


                    if (daysCount > 0 && monthsCount > 0)
                    {
                        helper.TotalDA = totalDa * totalMw;
                        helper.WinPCtDa = (double)positiveDaCount / daysCount;
                        double DaRisk = Math.Abs(((1 - ((double)positiveDaCount / daysCount)) * minDA)) == 0 ? 0 : (((double)positiveDaCount / daysCount) * maxDA) / Math.Abs(((1 - ((double)positiveDaCount / daysCount)) * minDA));
                        helper.RiskDa = DaRisk * (double)totalMw;
                        helper.WinPCMonthlytDa = (double)positiveDaMonthsCount / monthsCount;
                        helper.WinPCtCRR = (double)positiveCRRCount / monthsCount;
                        helper.AverageDA = (totalDa / daysCount) * totalMw;
                        helper.AverageMonthlyDA = (totalDa / monthsCount) * totalMw;
                        helper.AverageCRR = (totalCRR / monthsCount) * totalMw;
                        double CRRRIsk = Math.Abs(((1 - (double)(positiveCRRCount / monthsCount)) * minCRR)) == 0 ? 0 : (((double)positiveCRRCount / monthsCount) * maxCRR) / Math.Abs(((1 - (double)(positiveCRRCount / monthsCount)) * minCRR));
                        helper.RiskCRR = CRRRIsk * (double)totalMw;
                        helper.WinPCtDACRR = (double)positiveDaCRRCount / daysCount;
                        double DACRRRisk = Math.Abs(((1 - (double)(positiveDaCRRCount / daysCount)) * minDACRR)) == 0 ? 0 : (((double)positiveDaCRRCount / daysCount) * maxDACRR) / Math.Abs(((1 - (double)(positiveDaCRRCount / daysCount)) * minDACRR));
                        helper.RiskDACRR = DACRRRisk * (double)totalMw;
                        helper.AverageMonthlyDACRR = (totalDACRR / monthsCount) * totalMw;
                        helper.WinPCtMonthlyDACRR = ((double)positiveDaCRRMonthsCount / monthsCount) * totalMw;
                        helper.WinPCtRT = (double)positiveRtCount / daysCount;
                        double rtRisk = Math.Abs(((1 - (double)(positiveRtCount / daysCount)) * minRT)) == 0 ? 0 : (((double)positiveRtCount / daysCount) * maxRT) / Math.Abs(((1 - (double)(positiveRtCount / daysCount)) * minRT));
                        helper.RiskRT = rtRisk * (double)totalMw;
                        helper.WinPCtMonthlyRT = (double)positiveRtMonthsCount / monthsCount;
                        helper.AverageDACRR = (totalDACRR / daysCount) * totalMw;
                        helper.AverageRT = (totalRT / daysCount) * totalMw;
                        helper.AverageMonthlyRT = (totalRT / monthsCount) * totalMw;

                        helper.MaxDa = maxDA * totalMw;
                        helper.MinDa = minDA * totalMw;
                        helper.MaxMonthlyDa = maxMonthlyDa * totalMw;
                        helper.MinMonthlyDa = minMonthlyDa * totalMw;
                        //
                        helper.TotalCRR = totalCRR * totalMw;
                        helper.MaxCRR = maxCRR * totalMw;
                        helper.MinCRR = minCRR * totalMw;
                        //
                        helper.TotalDACRR = totalDACRR * totalMw;
                        helper.MaxDACRR = maxDACRR * totalMw;
                        helper.MinDACRR = minDACRR * totalMw;
                        helper.MaxMonthlyDACRR = maxMonthlyDaCRR * totalMw;
                        helper.MinMonthlyDACRR = minMonthlyDaCRR * totalMw;
                        //
                        helper.TotalRT = totalRT * totalMw;
                        helper.MaxRT = maxRT * totalMw;
                        helper.MinRT = minRT * totalMw;
                        helper.MaxMonthlyRT = maxMonthlyRT * totalMw;
                        helper.MinMonthlyRT = minMonthlyRT * totalMw;

                        tempPathwistStatList.Add(helper);

                    }


                }
                catch (Exception ex)
                {

                    continue;
                }

            }
            PathwistStatList = tempPathwistStatList.ToList();
        }

        public void SetSourceSink()
        {
            {
                DBAccess.GetSourceSinkNodeList(
                    (item1, error) =>
                    {
                        SourceNodeList = item1.Item1;
                        SinkNodeList = item1.Item1;
                    }, MarketComboSelectedValue, "Crr");
            }
        }

        private void ExportToExcel_PFAnalyser()
        {
            if (Portfolio_analyser_PathList.Count > 0)
            {
                try
                {
                    Vayu.CommonControls.ExportToExcelNodeSpread<PortfolioAnalyserHelper, List<PortfolioAnalyserHelper>> objSource = new Vayu.CommonControls.ExportToExcelNodeSpread<PortfolioAnalyserHelper, List<PortfolioAnalyserHelper>>();
                    List<PortfolioAnalyserHelper> lstPFAPathwise = new List<PortfolioAnalyserHelper>();
                    ICollectionView viewSource = CollectionViewSource.GetDefaultView(Portfolio_analyser_PathList);
                    foreach (var item in viewSource.SourceCollection)
                    {
                        lstPFAPathwise.Add((PortfolioAnalyserHelper)item);
                    }
                    objSource.dataToPrint = lstPFAPathwise;
                    objSource.GenerateReport();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //private void ExportToExcel_MonthlyAnalysis()
        //{
        //    if (CRRPivotList.Count > 0)
        //    {
        //        try
        //        {
        //            Vayu.CommonControls.ExportToExcelNodeSpread<FTRMonthlyData, List<FTRMonthlyData>> objSource = new Vayu.CommonControls.ExportToExcelNodeSpread<FTRMonthlyData, List<FTRMonthlyData>>();
        //            List<FTRMonthlyData> lstPFAPathwise = new List<FTRMonthlyData>();
        //            ICollectionView viewSource = CollectionViewSource.GetDefaultView(CRRPivotList);
        //            foreach (var item in viewSource.SourceCollection)
        //            {
        //                lstPFAPathwise.Add((FTRMonthlyData)item);
        //            }
        //            objSource.dataToPrint = lstPFAPathwise;
        //            objSource.GenerateReport();
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }
        //    }
        //}

        public class SelectedFTRMonthlyData
        {
            public string Source { get; set; }
            public string Sink { get; set; }
            public string ClassType { get; set; }
            public string HedgeType { get; set; }
            public string SourceZone { get; set; }
            public string SinkZone { get; set; }
            public decimal CRRYear1Month1 { get; set; }
            public decimal DA_CRR_DifferenceYear1Month1 { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
        }


        private void ExportToExcel_MonthlyAnalysis()
        {
            if (CRRPivotList.Count > 0)
            {
                try
                {
                    string filePath = "MonthlyAnalysisExport.csv";

                    StringBuilder csvContent = new StringBuilder();

                    csvContent.AppendLine("Source,Sink,Source Zone,Sink Zone,Class Type,Hedge Type,CRR Year 1 Month 1,DA-CRR Year 1 Month 1,CRR Year 1 Month 2,DA-CRR Year 1 Month 2,CRR Year 1 Month 3,DA-CRR Year 1 Month 3,CRR Year 2 Month 1,DA-CRR Year 2 Month 1,CRR Year 2 Month 2,DA-CRR Year 2 Month 2,CRR Year 2 Month 3,DA-CRR Year 2 Month 3,CRR Year 3 Month 1,DA-CRR Year 3 Month 1,CRR Year 3 Month 2,DA-CRR Year 3 Month 2,CRR Year 3 Month 3,DA-CRR Year 3 Month 3");

                    foreach (var item in CRRPivotList)
                    {
                        // Create a new row for each item
                        string row = $"{item.Source}," +
                                     $"{item.Sink}," +
                                     $"{item.SourceZone}," +
                                     $"{item.SinkZone}," +
                                     $"{item.ClassType}," +
                                     $"{item.HedgeType}," +
                                     $"{item.CRRYear1Month1}," +
                                     $"{item.DA_CRR_DifferenceYear1Month1}," +
                                     $"{item.CRRYear1Month2}," +
                                     $"{item.DA_CRR_DifferenceYear1Month2}," +
                                     $"{item.CRRYear1Month3}," +
                                     $"{item.DA_CRR_DifferenceYear1Month3}," +
                                     $"{item.CRRYear2Month1}," +
                                     $"{item.DA_CRR_DifferenceYear2Month1}," +
                                     $"{item.CRRYear2Month2}," +
                                     $"{item.DA_CRR_DifferenceYear2Month2}," +
                                     $"{item.CRRYear2Month3}," +
                                     $"{item.DA_CRR_DifferenceYear2Month3}" +
                                     $"{item.CRRYear3Month1}," +
                                     $"{item.DA_CRR_DifferenceYear3Month1}," +
                                     $"{item.CRRYear3Month2}," +
                                     $"{item.DA_CRR_DifferenceYear3Month2}," +
                                     $"{item.CRRYear3Month3}," +
                                     $"{item.DA_CRR_DifferenceYear3Month3}";


                        csvContent.AppendLine(row);
                    }

                    File.WriteAllText(filePath, csvContent.ToString());

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void ExportToExcel()
        {
            if (PathwistStatList.Count > 0)
            {
                try
                {
                    Vayu.CommonControls.ExportToExcelNodeSpread<PathwiseCalculationHelper, List<PathwiseCalculationHelper>> objSource = new Vayu.CommonControls.ExportToExcelNodeSpread<PathwiseCalculationHelper, List<PathwiseCalculationHelper>>();
                    List<PathwiseCalculationHelper> lstPathwise = new List<PathwiseCalculationHelper>();
                    ICollectionView viewSource = CollectionViewSource.GetDefaultView(PathwistStatList);
                    foreach (var item in viewSource.SourceCollection)
                    {
                        lstPathwise.Add((PathwiseCalculationHelper)item);
                    }
                    objSource.dataToPrint = lstPathwise;
                    objSource.GenerateReport();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void ExportButton()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        }
        private void ExportToCSVThreaded()
        {
            if (MonthlyPivotList == null)
            {
                Mouse.OverrideCursor = null;
                return;
            }
            if (MonthlyPivotList == null || MonthlyPivotList.Count == 0)
            {
                System.Windows.MessageBox.Show("No data to export to");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
            dialog.FileName = "CRR_Portfolio_Moduling_MonthlyData_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    if (MonthlyPivotList != null && MonthlyPivotList.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in MonthlyPivotList[0].GetType().GetProperties())
                        {
                            if (item.Name == "Date" || item.Name == "RowDisplayType" || item.Name == "Total" || item.Name == "Average"
                                 || item.Name == "HE1" || item.Name == "HE2" || item.Name == "HE3" || item.Name == "HE4" || item.Name == "HE5"
                                || item.Name == "HE6" || item.Name == "HE7" || item.Name == "HE8" || item.Name == "HE9" || item.Name == "HE10" || item.Name == "HE11"
                                || item.Name == "HE12" || item.Name == "HE13" || item.Name == "HE14" || item.Name == "HE15" || item.Name == "HE16" || item.Name == "HE17"
                                || item.Name == "HE18" || item.Name == "HE19" || item.Name == "HE20" || item.Name == "HE21" || item.Name == "HE22" || item.Name == "HE23" || item.Name == "HE24"
                                || item.Name == "HE25" || item.Name == "HE26" || item.Name == "HE27" || item.Name == "HE28" || item.Name == "HE29" || item.Name == "HE30" || item.Name == "HE31")
                            {
                                builder.Append(item.Name + ",");
                            }
                        }
                        builder.AppendLine();
                        foreach (var item in MonthlyPivotList)
                        {
                            foreach (var propItem in item.GetType().GetProperties())
                            {
                                if (propItem.Name == "Date" || propItem.Name == "RowDisplayType" || propItem.Name == "Total" || propItem.Name == "Average"
                                || propItem.Name == "HE1" || propItem.Name == "HE2" || propItem.Name == "HE3" || propItem.Name == "HE4" || propItem.Name == "HE5"
                                || propItem.Name == "HE6" || propItem.Name == "HE7" || propItem.Name == "HE8" || propItem.Name == "HE9" || propItem.Name == "HE10" || propItem.Name == "HE11"
                                || propItem.Name == "HE12" || propItem.Name == "HE13" || propItem.Name == "HE14" || propItem.Name == "HE15" || propItem.Name == "HE16" || propItem.Name == "HE17"
                                || propItem.Name == "HE18" || propItem.Name == "HE19" || propItem.Name == "HE20" || propItem.Name == "HE21" || propItem.Name == "HE22" || propItem.Name == "HE23" || propItem.Name == "HE24"
                                || propItem.Name == "HE25" || propItem.Name == "HE26" || propItem.Name == "HE27" || propItem.Name == "HE28" || propItem.Name == "HE29" || propItem.Name == "HE30" || propItem.Name == "HE31")
                                {
                                    if (propItem.Name == "HE1" || propItem.Name == "HE2" || propItem.Name == "HE3" || propItem.Name == "HE4" || propItem.Name == "HE5"
                                    || propItem.Name == "HE6" || propItem.Name == "HE7" || propItem.Name == "HE8" || propItem.Name == "HE9" || propItem.Name == "HE10" || propItem.Name == "HE11"
                                    || propItem.Name == "HE12" || propItem.Name == "HE13" || propItem.Name == "HE14" || propItem.Name == "HE15" || propItem.Name == "HE16" || propItem.Name == "HE17"
                                    || propItem.Name == "HE18" || propItem.Name == "HE19" || propItem.Name == "HE20" || propItem.Name == "HE21" || propItem.Name == "HE22" || propItem.Name == "HE23" || propItem.Name == "HE24"
                                    || propItem.Name == "HE25" || propItem.Name == "HE26" || propItem.Name == "HE27" || propItem.Name == "HE28" || propItem.Name == "HE29" || propItem.Name == "HE30" || propItem.Name == "HE31")
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

        private void OpenHistoricalConstraints()
        {
            Vayu.ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            var datacontext = new Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new Vayu.ConstraintContingencyHistory.Model.DataService());
            window.DataContext = datacontext;
            if (MarketComboSelectedValue == "ERCOT")
                datacontext.ShowHistoricalConstraintForExposure(SelectedConstraintPathValue.Constraint, SelectedConstraintPathValue.Contingency, SelectedConstraintPathValue.ID, 9);

            window.Show();
        }

        private void PathDetailsCommand()
        {
            List<Exposure> pathList = mPathDetailHash[SelectedConstraintPathValue.ConstId];

            PathDetail pathDetail = new PathDetail();
            PathDetailViewModel pathDetailViewModel = new PathDetailViewModel(this, pathList);
            pathDetail.DataContext = pathDetailViewModel;
            pathDetail.ResizeMode = ResizeMode.CanResize;
            pathDetail.Show();// .ShowDialog();
        }



        private void GoCommand()
        {
            try
            {
                //DateTime startDate = DateTime.Today;
                //DateTime endDate = DateTime.Today;
                DateTime startDate;
                DateTime endDate;
                if (BindingConstraintsChk)
                {
                    startDate = StartDateMonth;
                    endDate = EndDateMonth;
                }
                else
                {
                    startDate = new DateTime(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month, 1);
                    endDate = new DateTime(DateTime.Today.AddMonths(2).Year, DateTime.Today.AddMonths(2).Month, 1);
                }

                if (Predicted == "MONTH")
                {
                    if (MonthSelectedValue != null && MonthSelectedValue != "" && YearSelectedValue != 0)
                    {
                        int month = 0;

                        #region months

                        if (MonthSelectedValue.ToUpper() == "JANUARY")
                            month = 1;
                        else if (MonthSelectedValue.ToUpper() == "FEBRUARY")
                            month = 2;
                        else if (MonthSelectedValue.ToUpper() == "MARCH")
                            month = 3;
                        else if (MonthSelectedValue.ToUpper() == "APRIL")
                            month = 4;
                        else if (MonthSelectedValue.ToUpper() == "MAY")
                            month = 5;
                        else if (MonthSelectedValue.ToUpper() == "JUNE")
                            month = 6;
                        else if (MonthSelectedValue.ToUpper() == "JULY")
                            month = 7;
                        else if (MonthSelectedValue.ToUpper() == "AUGUST")
                            month = 8;
                        else if (MonthSelectedValue.ToUpper() == "SEPTEMBER")
                            month = 9;
                        else if (MonthSelectedValue.ToUpper() == "OCTOBER")
                            month = 10;
                        else if (MonthSelectedValue.ToUpper() == "NOVEMBER")
                            month = 11;
                        else if (MonthSelectedValue.ToUpper() == "DECEMBER")
                            month = 12;

                        #endregion

                        startDate = new DateTime(YearSelectedValue, month, 01);

                        endDate = startDate.AddMonths(1);

                        //if (MonthSelectedValue.ToUpper() != "DECEMBER")
                        //{
                        //    endDate = new DateTime(YearSelectedValue, month + 1, 01);
                        //}
                        //else
                        //{

                        //}
                    }
                    else
                    {
                        MessageBox.Show("Please select Month & Year");
                        return;
                    }
                }
                else
                {
                    //startDate = new DateTime(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month, 1);
                    //endDate = new DateTime(DateTime.Today.AddMonths(2).Year, DateTime.Today.AddMonths(2).Month,
                }

                string rtorda = "";
                if (RTExpChecked == true)
                {
                    rtorda = "RT";

                }
                else
                {
                    rtorda = "DA";
                }
                int market = 1;
                if (MarketComboSelectedValue == "ERCOT")
                {
                    market = 9;

                }

                List<ConstraintContingency> lstConstraintContingency = _dataService.GetConstraintContingencyList(startDate, endDate, Predicted, MarketComboSelectedValue, rtorda);
                Dictionary<int, Dictionary<int, double>> conHash = _dataService.GetSensitivityInfo(startDate, endDate, Predicted, MarketComboSelectedValue, rtorda);
                string date = DateTime.Now.Year.ToString() + '-' + DateTime.Now.Month.ToString() + '-' + "1";
                DateTime costDate = DateTime.Parse(date);
                if (MarketComboSelectedValue == "ERCOT")
                {
                    Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> dic = _dataService.GetCost(market, costDate, costDate.AddMonths(1).AddDays(-1), true, PathList, "Monthly");
                    //Dictionary<DateTime, Dictionary<string, Cost>> optionprice = GetOptionPrices(market, costDate, costDate.AddMonths(1).AddDays(-1),true, PathList, bool isAsbid, "Monthly", int ID)
                    Dictionary<DateTime, Dictionary<string, Cost>> dicOptionCost = _dataService.GetOptionPrices(market, costDate, costDate.AddMonths(1).AddDays(-1), PathList, AsBidChecked, AuctionSelectedItem, PortfolioComboSelectedItem.ID);
                    mPathDetailHash = new Dictionary<string, List<Exposure>>();
                    List<Exposure> lstExposure = new List<Exposure>();
                    foreach (var item in lstConstraintContingency)
                    {
                        if (!conHash.ContainsKey(item.ConstraintRTNum))
                        {
                            continue;
                        }
                        if (item.ShiftFactor > 100)
                        {
                            continue;
                        }
                        Exposure objExposure = new Exposure();
                        objExposure.ID = item.ConstraintRTNum;
                        objExposure.ConstId = item.ConstraintRTNum.ToString() + item.Month;
                        objExposure.Constraint = item.Constraint;
                        objExposure.Contingency = item.Contingency;
                        objExposure.Shift = item.ShiftFactor;
                        objExposure.Date = item.Date;
                        if (Predicted == "BINDING")
                        {
                            objExposure.Month = item.Month;
                        }

                        Dictionary<int, double> sensHash = conHash[item.ConstraintRTNum];
                        double sensSum = 0, peak = 0, offpeak = 0, peakWE = 0;
                        List<Exposure> exposureList = new List<Exposure>();
                        foreach (FTRBid path in PathList)
                        {
                            try
                            {
                                if (Predicted == "BINDING")
                                {
                                    if (path.PeriodName == "Q1")
                                    {
                                        if (!(objExposure.Month == "JUNE" || objExposure.Month == "JULY" || objExposure.Month == "AUGUST"))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (path.PeriodName == "Q2")
                                    {
                                        if (!(objExposure.Month == "SEPTEMBER" || objExposure.Month == "OCTOBER" || objExposure.Month == "NOVEMBER"))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (path.PeriodName == "Q3")
                                    {
                                        if (!(objExposure.Month == "DECEMBER" || objExposure.Month == "JANUARY" || objExposure.Month == "FEBRUARY"))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (path.PeriodName == "Q4")
                                    {
                                        if (!(objExposure.Month == "MARCH" || objExposure.Month == "APRIL" || objExposure.Month == "MAY"))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (path.PeriodKey != item.PeriodKey)
                                    {
                                        continue;
                                    }
                                }
                                Exposure pathExposure = new Exposure();
                                pathExposure.ID = item.ConstraintRTNum;
                                pathExposure.Source = path.Source;
                                pathExposure.Sink = path.Sink;
                                pathExposure.ClassType = path.ClassType;
                                pathExposure.Date = item.Date;
                                int sourceKey = DBAccess.GetNodeFromName(path.Source, market).NodeKey;
                                int sinkKey = DBAccess.GetNodeFromName(path.Sink, market).NodeKey;
                                if (!sensHash.ContainsKey(sourceKey) || !sensHash.ContainsKey(sinkKey))
                                {
                                    continue;
                                }
                                double sensSource = sensHash[sourceKey];
                                double sensSink = sensHash[sinkKey];
                                double senFinal = sensSink - sensSource;
                                double sumMW = 0;
                                double cost = double.MinValue;
                                //if (path.MW1 != null)
                                //{
                                //    sumMW += path.MW1.Value;
                                //}
                                string pathkey = path.SourceKey + "?" + path.SinkKey;
                                if (path.HedgeType.ToUpper() == "OBLIGATION" || path.HedgeType.ToUpper() == "OBL")
                                {
                                    if (dic.ContainsKey("Monthly"))
                                    {
                                        Dictionary<string, Dictionary<DateTime, Cost>> tempCostDict = dic["Monthly"];
                                        string strdate = startDate.ToString();
                                        if (tempCostDict.ContainsKey(sourceKey.ToString() + sinkKey.ToString()))
                                        {
                                            Dictionary<DateTime, Cost> tempDailyCostDict = tempCostDict[sourceKey.ToString() + sinkKey.ToString()];
                                            if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD" || path.ClassType.ToUpper() == "PEAKWE")
                                            {
                                                cost = tempDailyCostDict[DateTime.Parse(date)].Peak;
                                            }
                                            else if (path.ClassType.ToUpper() == "PEAKWE")
                                            {
                                                cost = tempDailyCostDict[DateTime.Parse(date)].PeakWE;
                                            }
                                            else
                                            {
                                                cost = tempDailyCostDict[DateTime.Parse(date)].OffPeak;
                                            }

                                        }

                                    }
                                }
                                else
                                {
                                    //cost = costHelper;
                                    if (dicOptionCost.ContainsKey(DateTime.Parse(date)))
                                    {
                                        Dictionary<string, Cost> tempOptionHash = dicOptionCost[DateTime.Parse(date)];
                                        if (tempOptionHash.ContainsKey(pathkey))
                                        {
                                            Cost objcost = tempOptionHash[pathkey];
                                            if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                                            {
                                                if (objcost.Peak != 0)
                                                    cost = objcost.Peak;//cost.Peak;
                                                else
                                                    continue;
                                            }
                                            else if (path.ClassType.ToUpper() == "PEAKWE")//Work pending
                                            {
                                                if (objcost.PeakWE != 0)
                                                    cost = objcost.PeakWE;
                                                else
                                                    continue;
                                            }
                                            else
                                            {
                                                if (objcost.OffPeak != 0)
                                                    cost = objcost.OffPeak;
                                                else
                                                    continue;
                                            }
                                        }
                                        else
                                            continue;
                                    }
                                    else
                                        continue;
                                }
                                if (double.IsNaN(cost) || double.IsInfinity(cost))
                                {
                                    continue;
                                }
                                if (MustTakeChecked)
                                {
                                    if (path.MW1 != null)
                                    {
                                        sumMW += path.MW1.Value;
                                    }
                                    if (path.MW2 != null)
                                    {
                                        sumMW += path.MW2.Value;
                                    }
                                    if (path.MW3 != null)
                                    {
                                        sumMW += path.MW3.Value;
                                    }
                                    if (path.MW4 != null)
                                    {
                                        sumMW += path.MW4.Value;
                                    }
                                    if (path.MW5 != null)
                                    {
                                        sumMW += path.MW5.Value;
                                    }
                                    if (path.MW6 != null)
                                    {
                                        sumMW += path.MW6.Value;
                                    }
                                }
                                else if (AsBidChecked)
                                {
                                    if (path.TradeType.ToLower() == "buy")
                                    {
                                        if (path.MW1 != null && cost < path.Price1 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW1.Value;
                                        }

                                        if (path.MW2 != null && cost < path.Price2 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW2.Value;
                                        }
                                        if (path.MW3 != null && cost < path.Price3 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW3.Value;
                                        }
                                        if (path.MW4 != null && cost < path.Price4 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW4.Value;
                                        }
                                        if (path.MW5 != null && cost < path.Price5 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW5.Value;
                                        }
                                        if (path.MW6 != null && cost < path.Price6 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW6.Value;
                                        }
                                    }
                                    else if (path.TradeType.ToLower() == "sell")
                                    {
                                        if (path.MW1 != null && cost > path.Price1 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW1.Value;
                                        }

                                        if (path.MW2 != null && cost > path.Price2 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW2.Value;
                                        }
                                        if (path.MW3 != null && cost > path.Price3 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW3.Value;
                                        }
                                        if (path.MW4 != null && cost > path.Price4 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW4.Value;
                                        }
                                        if (path.MW5 != null && cost > path.Price5 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW5.Value;
                                        }
                                        if (path.MW6 != null && cost > path.Price6 && cost != double.MinValue)
                                        {
                                            sumMW += path.MW6.Value;
                                        }
                                    }
                                }
                                if (Predicted == "BINDING")
                                {
                                    if (path.ClassType == "PEAK")
                                    {
                                        if (SortMWChecked)
                                        {
                                            pathExposure.Sum = (sumMW * senFinal);
                                            pathExposure.PeakSum = (sumMW * senFinal);
                                            pathExposure.PeakMW = sumMW;
                                            pathExposure.PeakMWhExposure = sumMW * senFinal;
                                        }
                                        else
                                        {
                                            pathExposure.Sum = (sumMW * senFinal * item.PeakHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.PeakSum = (sumMW * senFinal * item.PeakHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.PeakMW = sumMW;
                                            pathExposure.PeakMWhExposure = (sumMW * senFinal * item.PeakHours * item.DollarImpact * item.ShiftFactor);
                                        }
                                    }
                                    else if (path.ClassType == "OFFPEAK")
                                    {
                                        if (SortMWChecked)
                                        {
                                            pathExposure.Sum = (sumMW * senFinal);
                                            pathExposure.OffPeakSum = (sumMW * senFinal);
                                            pathExposure.OffPeakMW = sumMW;
                                            pathExposure.OffPeakMWhExposure = sumMW * senFinal;
                                        }
                                        else
                                        {
                                            pathExposure.Sum = (sumMW * senFinal * item.OffPeakHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.OffPeakSum = (sumMW * senFinal * item.OffPeakHours * item.DollarImpact * item.ShiftFactor);

                                            pathExposure.OffPeakMW = sumMW;
                                            pathExposure.OffPeakMWhExposure = (sumMW * senFinal * item.OffPeakHours * item.DollarImpact * item.ShiftFactor);
                                        }
                                    }
                                    pathExposure.SensSource = sensSource;
                                    pathExposure.SensSink = sensSink;
                                    pathExposure.Delta = sensSink - sensSource;
                                    pathExposure.Month = path.PeriodName;
                                }
                                if (Predicted == "NEXTMONTH" || Predicted == "ONGOING")
                                {
                                    pathExposure.Sum = (sumMW * senFinal * path.PeriodHours * item.DollarImpact * item.ShiftFactor);
                                }
                                if (Predicted == "MONTH")
                                {

                                    if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                                    {
                                        if (SortMWChecked)
                                        {
                                            pathExposure.Sum = (sumMW * senFinal);
                                            pathExposure.PeakSum = (sumMW * senFinal);
                                            pathExposure.PeakMW = sumMW;
                                            pathExposure.PeakMWhExposure = sumMW * senFinal;
                                        }
                                        else
                                        {
                                            pathExposure.Sum = (sumMW * senFinal * path.PeriodHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.PeakSum = (sumMW * senFinal * item.PeakHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.PeakMW = sumMW;
                                            pathExposure.PeakMWhExposure = (sumMW * senFinal * item.PeakHours * item.DollarImpact * item.ShiftFactor);
                                        }
                                        // pathExposure.MWhExposure = sumMW;//
                                    }
                                    else if (path.ClassType.ToUpper() == "PEAKWE")
                                    {
                                        if (SortMWChecked)
                                        {
                                            pathExposure.Sum = (sumMW * senFinal);
                                            pathExposure.PeakWESum = (sumMW * senFinal);
                                            pathExposure.PeakWEMW = sumMW;
                                            pathExposure.PeakWEMWhExposure = sumMW * senFinal;
                                        }
                                        else
                                        {
                                            pathExposure.Sum = (sumMW * senFinal * path.PeriodHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.PeakWESum = (sumMW * senFinal * item.PeakHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.PeakWEMW = sumMW;
                                            pathExposure.PeakWEMWhExposure = (sumMW * senFinal * item.PeakHours * item.DollarImpact * item.ShiftFactor);
                                        }
                                        // pathExposure.MWhExposure = sumMW;//
                                    }
                                    else
                                    {
                                        if (SortMWChecked)
                                        {
                                            pathExposure.Sum = (sumMW * senFinal);
                                            pathExposure.OffPeakSum = (sumMW * senFinal);
                                            pathExposure.OffPeakMW = sumMW;
                                            pathExposure.OffPeakMWhExposure = sumMW * senFinal;
                                        }
                                        else
                                        {
                                            pathExposure.Sum = (sumMW * senFinal * path.PeriodHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.OffPeakSum = (sumMW * senFinal * item.OffPeakHours * item.DollarImpact * item.ShiftFactor);
                                            pathExposure.OffPeakMW = sumMW;
                                            pathExposure.OffPeakMWhExposure = (sumMW * senFinal * item.OffPeakHours * item.DollarImpact * item.ShiftFactor);
                                        }
                                        // pathExposure.MWhExposure = sumMW;
                                    }
                                    pathExposure.SensSource = sensSource;
                                    pathExposure.SensSink = sensSink;
                                    pathExposure.Delta = sensSink - sensSource;
                                    pathExposure.Month = path.PeriodName;
                                }
                                sensSum += (double)pathExposure.Sum;
                                double temppeak = (pathExposure.PeakSum == null) ? 0 : (double)pathExposure.PeakSum;
                                peak += temppeak;
                                double tempoffpeak = (pathExposure.OffPeakSum == null) ? 0 : (double)pathExposure.OffPeakSum;
                                offpeak += tempoffpeak;
                                double temppeakwe = (pathExposure.PeakWESum == null) ? 0 : (double)pathExposure.PeakWESum;
                                peakWE += temppeakwe;
                                exposureList.Add(pathExposure);
                            }
                            catch (Exception ex)
                            {


                            }
                        }
                        if (!mPathDetailHash.ContainsKey(item.ConstraintRTNum + item.Month))
                            mPathDetailHash.Add(item.ConstraintRTNum + item.Month, exposureList);
                        else
                            mPathDetailHash[item.ConstraintRTNum + item.Month] = exposureList;
                        objExposure.Sum = sensSum;
                        objExposure.PeakSum = peak;
                        objExposure.OffPeakSum = offpeak;
                        objExposure.PeakWESum = peakWE;
                        lstExposure.Add(objExposure);
                    }
                    ExposureList = lstExposure;
                }

            }
            catch (Exception ex)
            {

            }
        }

        public void ShowPathMws()
        {
            if (PortfolioList != null)
            {
                Dictionary<string, List<Bid>> sourceSinkHash = new Dictionary<string, List<Bid>>();
                List<FTRBid> FTRBidList = new List<FTRBid>();
                List<FTRBid> FTRBidtemp = new List<FTRBid>();
                string portfiloName = string.Empty;
                double peaksum = 0, offpeaksum = 0, peakwesum = 0, total = 0;
                foreach (Portfolio portfolio in PortfolioList)
                {
                    portfiloName = portfolio.Name;
                    List<FTRBid> tempFTRBid = new List<FTRBid>();
                    tempFTRBid = DBAccess.GetFtrBids(portfolio.ID, AuctionSelectedItem, GetMarketKey());
                    FTRBidList.AddRange(tempFTRBid);
                }
                double mw1, mw2, mw3, mw4, mw5, mw6;
                List<FTRBid> tempFTRBidList = new List<FTRBid>();
                FTRBidtemp = FTRBidList;
                List<FTRBid> tempBidList = null;
                foreach (FTRBid item in FTRBidtemp.ToList())
                {
                    FTRBid FTRBid = new FTRBid();
                    FTRBid.Source = item.Source;
                    FTRBid.Sink = item.Sink;
                    FTRBid.ClassType = item.ClassType;
                    FTRBid.PortfolioName = item.PortfolioName;//PortfolioComboSelectedItem.ToString();
                    FTRBid.SourceZone = item.SourceZone;
                    FTRBid.SinkZone = item.SinkZone;
                    peaksum = new double(); offpeaksum = new double();
                    if (NonePathChecked)
                        tempBidList = FTRBidList.Where(a => a.Source == item.Source && a.Sink == item.Sink).ToList();
                    if (SourcePathChecked)
                        tempBidList = FTRBidList.Where(a => a.Source == item.Source).ToList();
                    if (SinkPathChecked)
                        tempBidList = FTRBidList.Where(a => a.Sink == item.Sink).ToList();
                    foreach (FTRBid item2 in tempBidList)
                    {
                        if (PeakChecked && !OffPeakChecked)
                        {
                            if (item2.ClassType == "PEAK" || item2.ClassType.ToUpper() == "PEAKWD")
                            {
                                mw1 = (double)item2.MW1;
                                mw2 = (double)item2.MW2;
                                mw3 = (double)item2.MW3;
                                mw4 = (double)item2.MW4;
                                mw5 = (double)item2.MW5;
                                mw6 = (double)item2.MW6;
                                peaksum += (mw1 + mw2 + mw3 + mw4 + mw5 + mw6);
                                FTRBid.Peak = peaksum;
                            }
                            if (item2.ClassType.ToUpper() == "PEAKWE")
                            {
                                mw1 = (double)item2.MW1;
                                mw2 = (double)item2.MW2;
                                mw3 = (double)item2.MW3;
                                mw4 = (double)item2.MW4;
                                mw5 = (double)item2.MW5;
                                mw6 = (double)item2.MW6;
                                peakwesum += (mw1 + mw2 + mw3 + mw4 + mw5 + mw6);
                                FTRBid.PeakWE = peakwesum;
                            }
                        }
                        if (OffPeakChecked && !PeakChecked)
                        {
                            if (item2.ClassType == "OFFPEAK" || item2.ClassType == "OFF-PEAK")
                            {
                                mw1 = (double)item2.MW1;
                                mw2 = (double)item2.MW2;
                                mw3 = (double)item2.MW3;
                                mw4 = (double)item2.MW4;
                                mw5 = (double)item2.MW5;
                                mw6 = (double)item2.MW6;
                                offpeaksum += (mw1 + mw2 + mw3 + mw4 + mw5 + mw6);
                                FTRBid.OffPeak = offpeaksum;
                            }
                        }
                        if (PeakChecked && OffPeakChecked)
                        {
                            if (item2.ClassType.ToUpper() == "PEAKWE")
                            {
                                mw1 = (double)item2.MW1;
                                mw2 = (double)item2.MW2;
                                mw3 = (double)item2.MW3;
                                mw4 = (double)item2.MW4;
                                mw5 = (double)item2.MW5;
                                mw6 = (double)item2.MW6;
                                peakwesum += (mw1 + mw2 + mw3 + mw4 + mw5 + mw6);
                                FTRBid.PeakWE = peakwesum;
                            }
                            if (item2.ClassType == "PEAK" || item2.ClassType.ToUpper() == "PEAKWD")
                            {
                                mw1 = (double)item2.MW1;
                                mw2 = (double)item2.MW2;
                                mw3 = (double)item2.MW3;
                                mw4 = (double)item2.MW4;
                                mw5 = (double)item2.MW5;
                                mw6 = (double)item2.MW6;
                                peaksum += (mw1 + mw2 + mw3 + mw4 + mw5 + mw6);
                                FTRBid.Peak = peaksum;
                            }

                            else
                            {
                                mw1 = (double)item2.MW1;
                                mw2 = (double)item2.MW2;
                                mw3 = (double)item2.MW3;
                                mw4 = (double)item2.MW4;
                                mw5 = (double)item2.MW5;
                                mw6 = (double)item2.MW6;
                                offpeaksum += (mw1 + mw2 + mw3 + mw4 + mw5 + mw6);
                                FTRBid.OffPeak = offpeaksum;
                            }
                        }
                    }
                    FTRBid.Total = (FTRBid.Peak == null ? 0.0 : FTRBid.Peak) + (FTRBid.OffPeak == null ? 0.0 : FTRBid.OffPeak) + (FTRBid.PeakWE == null ? 0.0 : FTRBid.PeakWE);
                    if (NonePathChecked)
                        FTRBidList.RemoveAll(a => a.Source == item.Source && a.Sink == item.Sink);
                    if (SourcePathChecked)
                        FTRBidList.RemoveAll(a => a.Source == item.Source);
                    if (SinkPathChecked)
                        FTRBidList.RemoveAll(a => a.Sink == item.Sink);
                    FTRBidtemp = FTRBidList.ToList();
                    tempFTRBidList.Add(FTRBid);
                }
                tempFTRBidList.RemoveAll(a => a.Total == 0);
                FilterPathMWList = tempFTRBidList.ToList();
                PathMWList = tempFTRBidList.ToList();
            }
        }
        private void PieChart()
        {
            ShowPathMws();
            PieChart chart = new PieChart();
            List<FTRBid> tempFilterPathMWList1 = FilterPathMWList;
            PlotDataFirst = DrawFirstGraph(tempFilterPathMWList1);
            chart.DataContext = new PieChartViewModel { PlotDataFirstNew = PlotDataFirst };
            chart.Title = "Pie-Chart";
            chart.Show();
        }

        #region Private Methods
        public PlotModel DrawFirstGraph(List<FTRBid> ListPathMW)
        {
            List<FTRBid> filteredFTRBid = new List<FTRBid>();
            PlotModel plotModel = new PlotModel();
            if (ListPathMW == null)
            {
                System.Windows.MessageBox.Show("Please Check source OR Sink Radio Button");
                return null;
            }
            else
            {
                List<FTRBid> tempFTRBidList = new List<FTRBid>();
                int count = ListPathMW.Distinct().Count();
                if (ListPathMW.Count > 0)
                {
                    string PortfolioName = string.Empty;
                    foreach (var item in PortfolioList)
                    {
                        PortfolioName += item.Name.ToString() + " ,";
                    }
                    filteredFTRBid = ListPathMW;
                    plotModel = new PlotModel
                    {
                        Title = "Pie-Chart for " + PortfolioName.Remove(PortfolioName.Length - 1) + " Portfolio",//PortfolioComboSelectedItem.ToString()
                        TitleFontSize = 15,
                        DefaultFont = "Arial Black",
                        DefaultFontSize = 10
                    };
                    //plotModel.Series = new Collection<Series>();
                    var ps = new PieSeries
                    {
                        InsideLabelFormat = "{1}",
                        AreInsideLabelsAngled = true,
                        AngleSpan = 360,
                        InsideLabelPosition = .70,
                        StrokeThickness = .90,
                        StartAngle = 0,
                        TextColor = OxyColors.Black
                    };
                    tempFTRBidList = filteredFTRBid;
                    foreach (FTRBid item in tempFTRBidList.OrderByDescending(a => a.Source).ToList())
                    {
                        string Name = string.Empty, Name2 = string.Empty;
                        double total = 0.0;
                        if (SourcePathChecked)
                        {
                            Name = item.SourceZone.ToString();
                            total = (double)filteredFTRBid.Where(a => a.SourceZone == Name).Sum(p => p.Total);
                        }
                        if (SinkPathChecked)
                        {
                            Name = item.SinkZone.ToString();
                            total = (double)filteredFTRBid.Where(a => a.SourceZone == Name).Sum(p => p.Total);
                        }
                        if (NonePathChecked)
                        {
                            Name = item.SourceZone.ToString();
                            Name2 = item.SinkZone.ToString();
                            total = (double)filteredFTRBid.Where(a => a.SourceZone == Name && a.SinkZone == Name2).Sum(p => p.Total);
                        }
                        if (total != 0.0)
                        {
                            if (Name == "BC")
                            {

                            }
                            ps.Slices.Add(new PieSlice(
                                Name + "(" + total + ")", Convert.ToDouble(total))
                            { IsExploded = true });
                        }
                        if (SourcePathChecked)
                            filteredFTRBid.RemoveAll(a => a.SourceZone == Name);
                        if (SinkPathChecked)
                            filteredFTRBid.RemoveAll(a => a.SinkZone == Name);
                        if (NonePathChecked)
                            filteredFTRBid.RemoveAll(a => a.SourceZone == Name && a.SinkZone == Name2);
                        tempFTRBidList = filteredFTRBid.ToList();
                    }
                    plotModel.Series.Add(ps);
                }
            }
            PlotDataFirst = plotModel;
            return PlotDataFirst;
        }
        /// <summary>
        /// Called when [winter command].
        /// </summary>
        private void OnWinterCommand()
        {
            ToggleMonth();
            DecChecked = true;
            JanChecked = true;
            FebChecked = true;
            RetrieveFetchDataAndUpdateChartCommand();
        }
        /// <summary>
        /// Called when [spring command].
        /// </summary>
        private void OnSpringCommand()
        {
            ToggleMonth();
            MarChecked = true;
            AprChecked = true;
            MayChecked = true;
            RetrieveFetchDataAndUpdateChartCommand();
        }
        /// <summary>
        /// Called when [summer command].
        /// </summary>
        private void OnSummerCommand()
        {
            ToggleMonth();
            JunChecked = true;
            JulChecked = true;
            AugChecked = true;
            RetrieveFetchDataAndUpdateChartCommand();
        }
        /// <summary>
        /// Called when [fall command].
        /// </summary>
        private void OnFallCommand()
        {
            ToggleMonth();
            SepChecked = true;
            OctChecked = true;
            NovChecked = true;
            RetrieveFetchDataAndUpdateChartCommand();
        }
        /// <summary>
        /// Toggles the month.
        /// </summary>
        /// <param name="toggle">if set to <c>true</c> [toggle].</param>
        private void ToggleMonth(bool toggle = false)
        {
            JanChecked = toggle;
            FebChecked = toggle;
            MarChecked = toggle;
            AprChecked = toggle;
            MayChecked = toggle;
            JunChecked = toggle;
            JulChecked = toggle;
            AugChecked = toggle;
            SepChecked = toggle;
            OctChecked = toggle;
            NovChecked = toggle;
            DecChecked = toggle;
        }



        /// <summary>
        /// Called when [none].
        /// </summary>
        private void OnNone()
        {
            ToggleMonth();
            RetrieveFetchDataAndUpdateChartCommand();
        }
        /// <summary>
        /// Called when [all command].
        /// </summary>
        private void OnAllCommand()
        {
            ToggleMonth(true);
            RetrieveFetchDataAndUpdateChartCommand();
        }
        /// <summary>
        /// Gets the portfolio auction detail.
        /// </summary>
        /// <returns></returns>



        private Tuple<int, string> GetPortfolioAuctionDetail()
        {
            string name = string.Empty;
            int portKey = 727;
            int marketKey = GetMarketKey();
            List<Tuple<int, string>> sppPortfolioList = _dataService.GetSppMisoPortfolioList(MarketComboSelectedValue);
            foreach (Tuple<int, string> misoSppPortfolio in sppPortfolioList)
            {
                if (TraderPortfolioComboSelectedItem.Name.IndexOf("-") != -1)
                {
                    string temp = TraderPortfolioComboSelectedItem.Name.Substring(TraderPortfolioComboSelectedItem.Name.IndexOf("-") + 1);
                    if (temp == misoSppPortfolio.Item2)
                    {
                        name = TraderPortfolioComboSelectedItem.Name.Replace("-" + misoSppPortfolio.Item2, "");
                        portKey = misoSppPortfolio.Item1;
                        break;
                    }
                }
            }

            return new Tuple<int, string>(portKey, name);
        }
        /// <summary>
        /// Deletes this instance.
        /// </summary>
        private void Delete()
        {
            _dataService.DeleteCRR(PortfolioComboSelectedItem.ID, GetMarketKey());
            AuctionSelectedItem = null;
            PortfolioComboSelectedItem = null;
            PortfolioComboList = null;
            RemovePortfolio();
            Clear();
        }
        /// <summary>
        /// Creates the submission file.
        /// </summary>
        private void CreateSubmissionFile()
        {
            FinalSubmit(true);
        }
        /// <summary>
        /// Connects this instance.
        /// </summary>
        //private void Connect()
        //{
        //    TcpTransportBindingElement transport = new TcpTransportBindingElement();
        //    transport.TransferMode = TransferMode.Streamed;
        //    BinaryMessageEncodingBindingElement encoder = new BinaryMessageEncodingBindingElement();
        //    CustomBinding binding = new CustomBinding(encoder, transport);
        //    NetTcpBinding myBinding = new NetTcpBinding();
        //    myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
        //    myBinding.OpenTimeout = new TimeSpan(0, 12, 0);
        //    myBinding.SendTimeout = new TimeSpan(0, 12, 0);
        //    myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
        //    myBinding.TransactionFlow = false;
        //    myBinding.MaxReceivedMessageSize = int.MaxValue;
        //    myBinding.MaxBufferPoolSize = int.MaxValue;
        //    myBinding.MaxBufferSize = int.MaxValue;
        //    myBinding.TransferMode = TransferMode.Streamed;
        //    myBinding.ReaderQuotas.MaxArrayLength = 5000000;
        //    ChannelFactory<ISourceSink> pipeFactory = new ChannelFactory<ISourceSink>(myBinding, new EndpointAddress(Vayu.CommonAccessLibrary.ServiceConnections.GetFTRService()));
        //    foreach (OperationDescription op in pipeFactory.Endpoint.Contract.Operations)
        //    {
        //        DataContractSerializerOperationBehavior dataContractBehavior =
        //                    op.Behaviors.Find<DataContractSerializerOperationBehavior>()
        //                    as DataContractSerializerOperationBehavior;
        //        if (dataContractBehavior != null)
        //        {
        //            dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
        //        }
        //    }
        //    try
        //    {
        //        mCRRCalculationProxy = pipeFactory.CreateChannel();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        /// <summary>
        /// Sets as bid must take hash.
        /// </summary>
        /// <param name="mustTakePnl">The must take PNL.</param>
        /// <param name="sourceSink">The source sink.</param>
        /// <param name="sourceSinkObj">The source sink object.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="pathHash">The path hash.</param>
        /// <param name="isDaily">if set to <c>true</c> [is daily].</param>
        private void SetAsBidMustTakeHash(PNL mustTakePnl, string sourceSink, SourceSink sourceSinkObj, DateTime startDate, int id, Dictionary<string, FTRBid> pathHash, bool isDaily)
        {
            Dictionary<string, Tuple<FTRBid, PNL>> mustTakePathHash = new Dictionary<string, Tuple<FTRBid, PNL>>();
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mustTakeMarketDateTimeHash = isDaily ? mMustTakeDailyMarketDateTimeHash : mMustTakeMarketDateTimeHash;
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> asBidMarketDateTimeHash = isDaily ? mAsBidDailyMarketDateTimeHash : mAsBidMarketDateTimeHash;
            if (mustTakeMarketDateTimeHash.ContainsKey(startDate))
            {
                mustTakePathHash = mustTakeMarketDateTimeHash[startDate];
            }
            else
            {
                mustTakeMarketDateTimeHash.Add(startDate, mustTakePathHash);
            }
            FTRBid mustTakeFTRBid = new FTRBid();
            mustTakeFTRBid.Source = sourceSinkObj.Source;
            mustTakeFTRBid.Sink = sourceSinkObj.Sink;
            Tuple<FTRBid, PNL> mustTakeTuple = new Tuple<FTRBid, PNL>(mustTakeFTRBid, mustTakePnl);
            if (mustTakePathHash.ContainsKey(id.ToString()))
            {
                mustTakePathHash.Remove(id.ToString());
            }
            mustTakePathHash.Add(id.ToString(), mustTakeTuple);
            string pathKey = sourceSinkObj.Source + sourceSinkObj.Sink + sourceSinkObj.ClassType + sourceSinkObj.HedgeType + sourceSinkObj.PeriodType + sourceSinkObj.TradeType;
            FTRBid path = pathHash[pathKey];
            PNL asBidPnl = new PNL();
            asBidPnl.SourceSink = sourceSink;
            asBidPnl.DA = mustTakePnl.DA;
            asBidPnl.Cost = mustTakePnl.Cost;
            asBidPnl.DART = mustTakePnl.DART;
            if (path.MW1 != null)
            {
                double cost = sourceSinkObj.Costmonthlytotal / sourceSinkObj.MW;
                cost = cost / (double)sourceSinkObj.DAPrice.Count;
                double rt = sourceSinkObj.DAmonthlytotal / sourceSinkObj.MW;
                rt = rt / (double)sourceSinkObj.DAPrice.Count;
                double pnl = sourceSinkObj.PNLmonthlytotal / sourceSinkObj.MW;
                pnl = pnl / (double)sourceSinkObj.DAPrice.Count;
                if (path.Price1 > cost)
                {
                    asBidPnl.DA = (double)(sourceSinkObj.Costmonthlytotal - (cost * (double)sourceSinkObj.DAPrice.Count * path.MW1));
                    asBidPnl.Cost = (double)(sourceSinkObj.DAmonthlytotal - (rt * (double)sourceSinkObj.DAPrice.Count * path.MW1));
                    asBidPnl.DART = (double)(sourceSinkObj.PNLmonthlytotal - (pnl * (double)sourceSinkObj.DAPrice.Count * path.MW1));
                }
                if (path.Price2 > cost)
                {
                    asBidPnl.DA = (double)(asBidPnl.DA - (cost * (double)sourceSinkObj.DAPrice.Count * path.MW2));
                    asBidPnl.Cost = (double)(asBidPnl.Cost - (rt * (double)sourceSinkObj.DAPrice.Count * path.MW2));
                    asBidPnl.DART = (double)(asBidPnl.DART - (pnl * (double)sourceSinkObj.DAPrice.Count * path.MW2));
                }
                if (path.Price3 > cost)
                {
                    asBidPnl.DA = (double)(asBidPnl.DA - (cost * (double)sourceSinkObj.DAPrice.Count * path.MW3));
                    asBidPnl.Cost = (double)(asBidPnl.Cost - (rt * (double)sourceSinkObj.DAPrice.Count * path.MW3));
                    asBidPnl.DART = (double)(asBidPnl.DART - (pnl * (double)sourceSinkObj.DAPrice.Count * path.MW3));
                }
                if (path.Price4 > cost)
                {
                    asBidPnl.DA = (double)(asBidPnl.DA - (cost * (double)sourceSinkObj.DAPrice.Count * path.MW4));
                    asBidPnl.Cost = (double)(asBidPnl.Cost - (rt * (double)sourceSinkObj.DAPrice.Count * path.MW4));
                    asBidPnl.DART = (double)(asBidPnl.DART - (pnl * (double)sourceSinkObj.DAPrice.Count * path.MW4));
                }
                if (path.Price5 > cost)
                {
                    asBidPnl.DA = (double)(asBidPnl.DA - (cost * (double)sourceSinkObj.DAPrice.Count * path.MW5));
                    asBidPnl.Cost = (double)(asBidPnl.Cost - (rt * (double)sourceSinkObj.DAPrice.Count * path.MW5));
                    asBidPnl.DART = (double)(asBidPnl.DART - (pnl * (double)sourceSinkObj.DAPrice.Count * path.MW5));
                }
                if (path.Price6 > cost)
                {
                    asBidPnl.DA = (double)(asBidPnl.DA - (cost * (double)sourceSinkObj.DAPrice.Count * path.MW6));
                    asBidPnl.Cost = (double)(asBidPnl.Cost - (rt * (double)sourceSinkObj.DAPrice.Count * path.MW6));
                    asBidPnl.DART = (double)(asBidPnl.DART - (pnl * (double)sourceSinkObj.DAPrice.Count * path.MW6));
                }
                if (Math.Abs(asBidPnl.DA) > 1)
                {
                    Dictionary<string, Tuple<FTRBid, PNL>> asBidPathHash = new Dictionary<string, Tuple<FTRBid, PNL>>();
                    if (asBidMarketDateTimeHash.ContainsKey(startDate))
                    {
                        asBidPathHash = asBidMarketDateTimeHash[startDate];
                    }
                    else
                    {
                        asBidMarketDateTimeHash.Add(startDate, asBidPathHash);
                    }
                    FTRBid asBidFTRBid = new FTRBid();
                    asBidFTRBid.Source = sourceSinkObj.Source;
                    asBidFTRBid.Sink = sourceSinkObj.Sink;
                    Tuple<FTRBid, PNL> asBidTuple = new Tuple<FTRBid, PNL>(asBidFTRBid, asBidPnl);
                    if (asBidPathHash.ContainsKey(id.ToString()))
                    {
                        asBidPathHash.Remove(id.ToString());
                    }
                    asBidPathHash.Add(id.ToString(), asBidTuple);
                }
            }
            id++;
        }
        /// <summary>
        /// Sets the summary.
        /// </summary>
        private void SetSummary()
        {
            DateTime bidRiskDate = DateTime.Today;
            DateTime bidWinDate = DateTime.Today;
            bool setBidEndDate = false;
            double tempBidDrawDown = 0;
            double bidDrawDown = double.MaxValue;
            DateTime startBidDrawdownDate = DateTime.Today;
            DateTime endBidDrawdownDate = DateTime.Today;
            DateTime tempBidDrawDownDate = DateTime.Today;
            List<int> valCount = new List<int>();
            for (int i = 0; i < 31; i++)
                valCount.Add(0);
            if (DARTChecked == true)
            {
                HourlyPivotData total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "DART",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault()),
                    monthlyClearedmwhText = Math.Round(MonthlyPivotList.Where(x => x.ClearedMWHmonth.HasValue).Sum(x => x.ClearedMWHmonth).GetValueOrDefault(), 2),
                };
                HourlyPivotData max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "DART",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "DART",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "DART",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                HourlyPivotData win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "DART" };
                HourlyPivotData risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DART" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DART" };
                Type hourlyType = typeof(HourlyPivotData);

                foreach (var item in MonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        // double? value = inf.GetValue(item) as double?;
                        double? value = MonthlyPivotList.Sum(x => inf.GetValue(x) as double?);
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);

                }

                if (MonthlyPivotList.Count != 0)
                {
                    double wCo = MonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = MonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }

                List<HourlyPivotData> summaryList = new List<HourlyPivotData>();


                if (FilterDayComparisonTotalChecked)
                    summaryList.Add(total);

                if (FilterDayComparisonAvgChecked)
                    summaryList.Add(avg);

                if (FilterDayComparisonMaxChecked)
                    summaryList.Add(max);

                if (FilterDayComparisonMinChecked)
                    summaryList.Add(min);

                if (FilterDayComparisonWinPctChecked)
                    summaryList.Add(win);

                if (FilterDayComparisonRiskChecked)
                    summaryList.Add(risk);
                HourlyPivotListSummary = summaryList;
            }
            else if (CostPriceChecked == true)
            {
                HourlyPivotData total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "COST",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault()),
                    monthlyClearedmwhText = Math.Round(MonthlyPivotList.Where(x => x.ClearedMWHmonth.HasValue).Sum(x => x.ClearedMWHmonth).GetValueOrDefault(), 2),


                };
                HourlyPivotData max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "COST",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "COST",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "COST",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                HourlyPivotData win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "COST" };
                HourlyPivotData risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "COST" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "COST" };
                Type hourlyType = typeof(HourlyPivotData);

                foreach (var item in MonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        //  double? value = inf.GetValue(item) as double?;
                        double? value = MonthlyPivotList.Sum(x => inf.GetValue(x) as double?);
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);

                }

                if (MonthlyPivotList.Count != 0)
                {
                    double wCo = MonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = MonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }

                List<HourlyPivotData> summaryList = new List<HourlyPivotData>();


                if (FilterDayComparisonTotalChecked)
                    summaryList.Add(total);

                if (FilterDayComparisonAvgChecked)
                    summaryList.Add(avg);

                if (FilterDayComparisonMaxChecked)
                    summaryList.Add(max);

                if (FilterDayComparisonMinChecked)
                    summaryList.Add(min);

                if (FilterDayComparisonWinPctChecked)
                    summaryList.Add(win);

                if (FilterDayComparisonRiskChecked)
                    summaryList.Add(risk);
                HourlyPivotListSummary = summaryList;
            }
            else if (DAPriceChecked == true)
            {
                HourlyPivotData total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "DA",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault()),
                    monthlyClearedmwhText = Math.Round(MonthlyPivotList.Where(x => x.ClearedMWHmonth.HasValue).Sum(x => x.ClearedMWHmonth).GetValueOrDefault(), 2),


                };
                HourlyPivotData max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "DA",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "DA",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "DA",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                HourlyPivotData win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "DA" };
                HourlyPivotData risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DA" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DA" };
                Type hourlyType = typeof(HourlyPivotData);

                foreach (var item in MonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        // double? value = inf.GetValue(item) as double?;
                        double? value = MonthlyPivotList.Sum(x => inf.GetValue(x) as double?);
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);

                }

                if (MonthlyPivotList.Count != 0)
                {
                    double wCo = MonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = MonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }

                List<HourlyPivotData> summaryList = new List<HourlyPivotData>();


                if (FilterDayComparisonTotalChecked)
                    summaryList.Add(total);

                if (FilterDayComparisonAvgChecked)
                    summaryList.Add(avg);

                if (FilterDayComparisonMaxChecked)
                    summaryList.Add(max);

                if (FilterDayComparisonMinChecked)
                    summaryList.Add(min);

                if (FilterDayComparisonWinPctChecked)
                    summaryList.Add(win);

                if (FilterDayComparisonRiskChecked)
                    summaryList.Add(risk);
                HourlyPivotListSummary = summaryList;
            }
            else if (RTPriceChecked == true)
            {
                HourlyPivotData total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "RT",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault()),
                    monthlyClearedmwhText = Math.Round(MonthlyPivotList.Where(x => x.ClearedMWHmonth.HasValue).Sum(x => x.ClearedMWHmonth).GetValueOrDefault(), 2),


                };
                HourlyPivotData max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "RT",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "RT",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "RT",
                    Total = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(MonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                HourlyPivotData win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "RT" };
                HourlyPivotData risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "RT" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "RT" };
                Type hourlyType = typeof(HourlyPivotData);

                foreach (var item in MonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        //  double? value = inf.GetValue(item) as double?;
                        double? value = MonthlyPivotList.Sum(x => inf.GetValue(x) as double?);
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);

                }

                if (MonthlyPivotList.Count != 0)
                {
                    double wCo = MonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = MonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }

                List<HourlyPivotData> summaryList = new List<HourlyPivotData>();


                if (FilterDayComparisonTotalChecked)
                    summaryList.Add(total);

                if (FilterDayComparisonAvgChecked)
                    summaryList.Add(avg);

                if (FilterDayComparisonMaxChecked)
                    summaryList.Add(max);

                if (FilterDayComparisonMinChecked)
                    summaryList.Add(min);

                if (FilterDayComparisonWinPctChecked)
                    summaryList.Add(win);

                if (FilterDayComparisonRiskChecked)
                    summaryList.Add(risk);
                HourlyPivotListSummary = summaryList;
            }


        }

        /// <summary>
        /// Calculates the summary data.
        /// </summary>
        /// <param name="tempMonthlyPivotList">The temporary monthly pivot list.</param>
        /// <param name="total">The total.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="win">The win.</param>
        /// <param name="risk">The risk.</param>
        /// <param name="maxItems">The maximum items.</param>
        /// <param name="minItems">The minimum items.</param>
        /// <param name="bidDrawDown">The bid draw down.</param>
        /// <param name="startBidDrawdownDate">The start bid drawdown date.</param>
        /// <param name="endBidDrawdownDate">The end bid drawdown date.</param>
        private void CalculateSummaryData(List<HourlyPivotData> tempMonthlyPivotList, out HourlyPivotData total, out HourlyPivotData max, out HourlyPivotData min,
                                                                    out HourlyPivotData win, out HourlyPivotData risk, out HourlyPivotData maxItems, out HourlyPivotData minItems,
                                                                    out double bidDrawDown, out DateTime startBidDrawdownDate, out DateTime endBidDrawdownDate)
        {
            bidDrawDown = double.MaxValue;
            bool setBidEndDate = false;
            double tempBidDrawDown = 0;
            Type hourlyType = null;
            total = null;
            max = null;
            min = null;
            win = null;
            maxItems = null;
            minItems = null;
            risk = null;
            double tempBidDrawDownSum = 0;
            startBidDrawdownDate = DateTime.Today;
            DateTime prevDate = DateTime.MinValue;
            endBidDrawdownDate = DateTime.Today;
            DateTime tempBidDrawDownDate = DateTime.Today;
            DateTime lastDate = DateTime.MaxValue;
            int tempcount = 0;
            List<int> valCount = new List<int>();
            for (int i = 0; i < 31; i++)
                valCount.Add(0);
            if (DARTChecked == true)
            {
                total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "DART",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault())
                };
                max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "DART",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "DART",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "DART",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "DART" };
                risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DART" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DART" };
                hourlyType = typeof(HourlyPivotData);
                foreach (var item in tempMonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        double? value = inf.GetValue(item) as double?;
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);
                }

                if (tempMonthlyPivotList.Count != 0)
                {
                    double wCo = tempMonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = tempMonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }
                maxItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Max(a => a.Total));
                minItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Min(a => a.Total));
                Dictionary<DateTime, Dictionary<double, int>> prevData = new Dictionary<DateTime, Dictionary<double, int>>();
                foreach (var item in tempMonthlyPivotList.GroupBy(x => x.DateDisplay))
                {
                    Dictionary<double, int> innerdict = new Dictionary<double, int>();
                    double down = Convert.ToDouble(item.FirstOrDefault().Total);
                    if (item.FirstOrDefault().Total < 0)
                    {
                        double tempBidDrawDown1 = Convert.ToDouble(item.FirstOrDefault().Total);
                        if (prevData != null && prevData.Count > 0)
                        {
                            if (prevDate != DateTime.MinValue)
                            {
                                if (prevDate.AddMonths(-1) == Convert.ToDateTime(item.FirstOrDefault().DateDisplay))
                                {
                                    prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                                    tempBidDrawDownSum += tempBidDrawDown1;
                                    innerdict = new Dictionary<double, int>();
                                    tempcount++;
                                    innerdict.Add(tempBidDrawDownSum, tempcount);
                                    prevData[prevData.Keys.Min()] = innerdict;
                                }
                            }
                            else
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                tempBidDrawDownSum = tempBidDrawDown1;
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }

                        }
                        else
                        {
                            if (tempBidDrawDown1 < 0)
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }
                        }
                    }
                    else
                    {
                        tempBidDrawDownSum = 0;
                        tempcount = 0;
                        prevDate = DateTime.MinValue;
                    }
                }
                double tempmin = 0;
                int monthCount = 0;
                DateTime startdate = DateTime.Today;
                DateTime enddate = DateTime.Today;
                foreach (DateTime date in prevData.Keys)
                {
                    Dictionary<double, int> tempdict = prevData[date];
                    foreach (double drwdown in tempdict.Keys)
                    {
                        if (drwdown < tempmin)
                        {
                            tempmin = drwdown;
                            monthCount = tempdict[drwdown];
                            startdate = date;
                            enddate = date.AddMonths(-monthCount);
                        }
                    }
                }
                bidDrawDown = tempmin;
                startBidDrawdownDate = startdate;
                endBidDrawdownDate = enddate;

            }
            else if (CostPriceChecked == true)
            {
                total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "COST",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault())
                };
                max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "COST",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "COST",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "COST",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "COST" };
                risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "COST" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "COST" };
                hourlyType = typeof(HourlyPivotData);
                foreach (var item in tempMonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        double? value = inf.GetValue(item) as double?;
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);
                }

                if (tempMonthlyPivotList.Count != 0)
                {
                    double wCo = tempMonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = tempMonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }
                maxItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Max(a => a.Total));
                minItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Min(a => a.Total));
                Dictionary<DateTime, Dictionary<double, int>> prevData = new Dictionary<DateTime, Dictionary<double, int>>();
                foreach (var item in tempMonthlyPivotList.GroupBy(x => x.DateDisplay))
                {
                    Dictionary<double, int> innerdict = new Dictionary<double, int>();
                    double down = Convert.ToDouble(item.FirstOrDefault().Total);
                    if (item.FirstOrDefault().Total < 0)
                    {
                        double tempBidDrawDown1 = Convert.ToDouble(item.FirstOrDefault().Total);
                        if (prevData != null && prevData.Count > 0)
                        {
                            if (prevDate != DateTime.MinValue)
                            {
                                if (prevDate.AddMonths(-1) == Convert.ToDateTime(item.FirstOrDefault().DateDisplay))
                                {
                                    prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                                    tempBidDrawDownSum += tempBidDrawDown1;
                                    innerdict = new Dictionary<double, int>();
                                    tempcount++;
                                    innerdict.Add(tempBidDrawDownSum, tempcount);
                                    prevData[prevData.Keys.Min()] = innerdict;
                                }
                            }
                            else
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                tempBidDrawDownSum = tempBidDrawDown1;
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }

                        }
                        else
                        {
                            if (tempBidDrawDown1 < 0)
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }
                        }
                    }
                    else
                    {
                        tempBidDrawDownSum = 0;
                        tempcount = 0;
                        prevDate = DateTime.MinValue;
                    }
                }
                double tempmin = 0;
                int monthCount = 0;
                DateTime startdate = DateTime.Today;
                DateTime enddate = DateTime.Today;
                foreach (DateTime date in prevData.Keys)
                {
                    Dictionary<double, int> tempdict = prevData[date];
                    foreach (double drwdown in tempdict.Keys)
                    {
                        if (drwdown < tempmin)
                        {
                            tempmin = drwdown;
                            monthCount = tempdict[drwdown];
                            startdate = date;
                            enddate = date.AddMonths(-monthCount);
                        }
                    }
                }
                bidDrawDown = tempmin;
                startBidDrawdownDate = startdate;
                endBidDrawdownDate = enddate;

            }
            else if (DAPriceChecked == true)
            {
                total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "DA",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault())
                };
                max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "DA",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "DA",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "DA",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "DA" };
                risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DA" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "DA" };
                hourlyType = typeof(HourlyPivotData);
                foreach (var item in tempMonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        double? value = inf.GetValue(item) as double?;
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);
                }

                if (tempMonthlyPivotList.Count != 0)
                {
                    double wCo = tempMonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = tempMonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }
                maxItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Max(a => a.Total));
                minItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Min(a => a.Total));
                Dictionary<DateTime, Dictionary<double, int>> prevData = new Dictionary<DateTime, Dictionary<double, int>>();
                foreach (var item in tempMonthlyPivotList.GroupBy(x => x.DateDisplay))
                {
                    Dictionary<double, int> innerdict = new Dictionary<double, int>();
                    double down = Convert.ToDouble(item.FirstOrDefault().Total);
                    if (item.FirstOrDefault().Total < 0)
                    {
                        double tempBidDrawDown1 = Convert.ToDouble(item.FirstOrDefault().Total);
                        if (prevData != null && prevData.Count > 0)
                        {
                            if (prevDate != DateTime.MinValue)
                            {
                                if (prevDate.AddMonths(-1) == Convert.ToDateTime(item.FirstOrDefault().DateDisplay))
                                {
                                    prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                                    tempBidDrawDownSum += tempBidDrawDown1;
                                    innerdict = new Dictionary<double, int>();
                                    tempcount++;
                                    innerdict.Add(tempBidDrawDownSum, tempcount);
                                    prevData[prevData.Keys.Min()] = innerdict;
                                }
                            }
                            else
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                tempBidDrawDownSum = tempBidDrawDown1;
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }

                        }
                        else
                        {
                            if (tempBidDrawDown1 < 0)
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }
                        }
                    }
                    else
                    {
                        tempBidDrawDownSum = 0;
                        tempcount = 0;
                        prevDate = DateTime.MinValue;
                    }
                }
                double tempmin = 0;
                int monthCount = 0;
                DateTime startdate = DateTime.Today;
                DateTime enddate = DateTime.Today;
                foreach (DateTime date in prevData.Keys)
                {
                    Dictionary<double, int> tempdict = prevData[date];
                    foreach (double drwdown in tempdict.Keys)
                    {
                        if (drwdown < tempmin)
                        {
                            tempmin = drwdown;
                            monthCount = tempdict[drwdown];
                            startdate = date;
                            enddate = date.AddMonths(-monthCount);
                        }
                    }
                }
                bidDrawDown = tempmin;
                startBidDrawdownDate = startdate;
                endBidDrawdownDate = enddate;

            }
            else if (RTPriceChecked == true)
            {
                total = new HourlyPivotData()
                {
                    RowDisplayType = "Total",
                    RowDay = "RT",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Sum(x => x.Average).GetValueOrDefault())
                };
                max = new HourlyPivotData()
                {
                    RowDisplayType = "Max",
                    RowDay = "RT",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Max(x => x.Average).GetValueOrDefault())
                };
                min = new HourlyPivotData()
                {
                    RowDisplayType = "Min",
                    RowDay = "RT",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Min(x => x.Average).GetValueOrDefault())
                };
                HourlyPivotData avg = new HourlyPivotData()
                {
                    RowDisplayType = "Average",
                    RowDay = "RT",
                    Total = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Total).GetValueOrDefault()),
                    Average = Math.Round(tempMonthlyPivotList.Where(x => x.Total.HasValue).Average(x => x.Average).GetValueOrDefault())
                };

                win = new HourlyPivotData() { RowDisplayType = "Win%", RowDay = "RT" };
                risk = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "RT" };
                HourlyPivotData count = new HourlyPivotData() { RowDisplayType = "Risk", RowDay = "RT" };
                hourlyType = typeof(HourlyPivotData);
                foreach (var item in tempMonthlyPivotList)
                {
                    for (int i = 1, j = 0; i <= 31; i++, j++)
                    {
                        PropertyInfo inf = hourlyType.GetProperty("HE" + i);

                        double? value = inf.GetValue(item) as double?;
                        if (!value.HasValue)
                            continue;

                        valCount[j] += 1;
                        double? tValue = inf.GetValue(total) as double?;
                        double? maxValue = inf.GetValue(max) as double?;
                        double? minValue = inf.GetValue(min) as double?;
                        double? winValue = inf.GetValue(win) as double?;
                        double? countValue = inf.GetValue(count) as double?;
                        // DateTime winDate = inf.GetValue(winDate) as DateTime;
                        if (tValue.HasValue)
                            tValue += value;
                        else
                            tValue = value;

                        if (!maxValue.HasValue || maxValue < value)
                        {
                            maxValue = value;
                        }
                        if (!minValue.HasValue || minValue > value)
                        {
                            minValue = value;
                        }
                        if (value.HasValue)
                        {
                            if (value > 0)
                            {
                                if (winValue.HasValue)
                                    winValue++;
                                else
                                    winValue = 1;
                            }

                            if (countValue.HasValue)
                                countValue++;
                            else
                                countValue = 1;
                        }

                        inf.SetValue(total, Math.Round(value.Value));
                        inf.SetValue(max, Math.Round(maxValue.Value));
                        inf.SetValue(min, Math.Round(minValue.Value));
                        inf.SetValue(win, winValue);
                        inf.SetValue(count, countValue);
                    }
                }

                //Average
                for (int i = 0; i < 31; i++)
                {
                    PropertyInfo inf = hourlyType.GetProperty("HE" + (i + 1));
                    double? value = inf.GetValue(total) as double?;
                    if (!value.HasValue)
                        continue;
                    value = value / valCount[i];
                    inf.SetValue(avg, value);

                    double? minVal = inf.GetValue(min) as double?;
                    double? maxVal = inf.GetValue(max) as double?;
                    double? winVal = inf.GetValue(win) as double?;
                    double? countVal = inf.GetValue(count) as double?;
                    if (winVal.HasValue && countVal.HasValue)
                        winVal = winVal / countVal;

                    double? riskVal = GetRisk(minVal, maxVal, winVal);
                    inf.SetValue(win, winVal);
                    inf.SetValue(risk, riskVal);
                }

                if (tempMonthlyPivotList.Count != 0)
                {
                    double wCo = tempMonthlyPivotList.Where(x => x.Total.HasValue && x.Total > 0).Count();
                    double tco = tempMonthlyPivotList.Where(x => x.Total.HasValue).Count();
                    win.Total = wCo / tco;

                    risk.Total = GetRisk(min.Total, max.Total, win.Total);
                }
                maxItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Max(a => a.Total));
                minItems = tempMonthlyPivotList.First(x => x.Total == tempMonthlyPivotList.Min(a => a.Total));
                Dictionary<DateTime, Dictionary<double, int>> prevData = new Dictionary<DateTime, Dictionary<double, int>>();
                foreach (var item in tempMonthlyPivotList.GroupBy(x => x.DateDisplay))
                {
                    Dictionary<double, int> innerdict = new Dictionary<double, int>();
                    double down = Convert.ToDouble(item.FirstOrDefault().Total);
                    if (item.FirstOrDefault().Total < 0)
                    {
                        double tempBidDrawDown1 = Convert.ToDouble(item.FirstOrDefault().Total);
                        if (prevData != null && prevData.Count > 0)
                        {
                            if (prevDate != DateTime.MinValue)
                            {
                                if (prevDate.AddMonths(-1) == Convert.ToDateTime(item.FirstOrDefault().DateDisplay))
                                {
                                    prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                                    tempBidDrawDownSum += tempBidDrawDown1;
                                    innerdict = new Dictionary<double, int>();
                                    tempcount++;
                                    innerdict.Add(tempBidDrawDownSum, tempcount);
                                    prevData[prevData.Keys.Min()] = innerdict;
                                }
                            }
                            else
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                tempBidDrawDownSum = tempBidDrawDown1;
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }

                        }
                        else
                        {
                            if (tempBidDrawDown1 < 0)
                            {
                                innerdict.Add(tempBidDrawDown1, tempcount);
                                prevData.Add((Convert.ToDateTime(item.FirstOrDefault().DateDisplay)), innerdict);
                                prevDate = Convert.ToDateTime(item.FirstOrDefault().DateDisplay);
                            }
                        }
                    }
                    else
                    {
                        tempBidDrawDownSum = 0;
                        tempcount = 0;
                        prevDate = DateTime.MinValue;
                    }
                }
                double tempmin = 0;
                int monthCount = 0;
                DateTime startdate = DateTime.Today;
                DateTime enddate = DateTime.Today;
                foreach (DateTime date in prevData.Keys)
                {
                    Dictionary<double, int> tempdict = prevData[date];
                    foreach (double drwdown in tempdict.Keys)
                    {
                        if (drwdown < tempmin)
                        {
                            tempmin = drwdown;
                            monthCount = tempdict[drwdown];
                            startdate = date;
                            enddate = date.AddMonths(-monthCount);
                        }
                    }
                }
                bidDrawDown = tempmin;
                startBidDrawdownDate = startdate;
                endBidDrawdownDate = enddate;

            }


        }


        /// <summary>
        /// Gets the summary list.
        /// </summary>
        /// <param name="tempAsBidMonthlyPivotList">The temporary as bid monthly pivot list.</param>
        /// <param name="tempMustTakeMonthlyPivotList">The temporary must take monthly pivot list.</param>
        private void GetSummaryList(List<HourlyPivotData> tempAsBidMonthlyPivotList, List<HourlyPivotData> tempMustTakeMonthlyPivotList)
        {
            HourlyPivotData total = new HourlyPivotData();
            HourlyPivotData max = new HourlyPivotData();
            HourlyPivotData min = new HourlyPivotData();
            HourlyPivotData win = new HourlyPivotData();
            HourlyPivotData risk = new HourlyPivotData();
            HourlyPivotData maxItems = new HourlyPivotData();
            HourlyPivotData minItems = new HourlyPivotData();
            double bidDrawDown = double.MaxValue;
            DateTime startBidDrawdownDate = DateTime.Today;
            DateTime endBidDrawdownDate = DateTime.Today;

            if (tempAsBidMonthlyPivotList != null)
            {
                CalculateSummaryData(tempAsBidMonthlyPivotList, out total, out max, out min, out win, out risk, out maxItems, out minItems,
                                                           out bidDrawDown, out startBidDrawdownDate, out endBidDrawdownDate);
            }




            AsBidSumText = null;
            AsBidSumText = Convert.ToDouble(total.Total).ToString("#,##0;(#,##0)");
            AsBidWinPerText = null;
            AsBidWinPerText = Convert.ToDouble(win.Total).ToString("0.00%");
            AsBidDolMW = null;
            AsBidDolMW = Convert.ToDouble(total.Total / Convert.ToDouble(MWText)).ToString("#,##0.0");
            AsBidRiskReward = null;
            AsBidRiskReward = Convert.ToDouble(max.Total) == double.MinValue || Convert.ToDouble(min.Total) == double.MaxValue || Convert.ToDouble(min.Total) == 0 ? "0" :
                                    (Convert.ToDouble(max.Total) / Math.Abs(Convert.ToDouble(min.Total))).ToString("#,##0.00;(#,##0.00)");
            AsBidRisk = null;
            AsBidRisk = Convert.ToDouble(min.Total) == double.MaxValue ? "0" : Convert.ToDouble(min.Total).ToString("#,##0;(#,##0)");
            AsBidRiskDate = null;
            AsBidRiskDate = Convert.ToDateTime(minItems.DateDisplay) == DateTime.Today ? "" : Convert.ToDateTime(minItems.DateDisplay).ToString("MM/yyyy");
            AsBidWin = null;
            AsBidWin = Convert.ToDouble(max.Total) == double.MinValue ? "0" : Convert.ToDouble(max.Total).ToString("#,##0;(#,##0)");
            AsBidWinDate = null;
            AsBidWinDate = Convert.ToDateTime(maxItems.DateDisplay) == DateTime.Today ? "" : Convert.ToDateTime(maxItems.DateDisplay).ToString("MM/yyyy");
            AsBidMaxDrawDown = null;
            AsBidMaxDrawDown = bidDrawDown == double.MaxValue ? "0" : bidDrawDown.ToString("#,##0;(#,##0)");
            AsBidMaxDrawDownDate = null;
            if (startBidDrawdownDate == endBidDrawdownDate)
            {
                AsBidMaxDrawDownDate = startBidDrawdownDate == DateTime.Today && endBidDrawdownDate == DateTime.Today ? "" :
                   endBidDrawdownDate.ToString("MM/yyyy");
            }
            else
            {
                AsBidMaxDrawDownDate = startBidDrawdownDate == DateTime.Today && endBidDrawdownDate == DateTime.Today ? "" :
                      endBidDrawdownDate.ToString("MM/yyyy") + " - " + startBidDrawdownDate.ToString("MM/yyyy");
            }
            // if (MustTakeChecked)
            {
                if (tempMustTakeMonthlyPivotList != null)
                    CalculateSummaryData(tempMustTakeMonthlyPivotList, out total, out max, out min, out win, out risk, out maxItems, out minItems,
                                                                    out bidDrawDown, out startBidDrawdownDate, out endBidDrawdownDate);
                MustTakeSumText = null;
                MustTakeSumText = Convert.ToDouble(total.Total).ToString("#,##0;(#,##0)");
                MustTakeWinPerText = null;
                MustTakeWinPerText = Convert.ToDouble(win.Total).ToString("0.00%");
                MustTakeDolMW = null;
                MustTakeDolMW = Convert.ToDouble(total.Total / Convert.ToDouble(MWText)).ToString("#,##0.0");
                MustTakeRiskReward = null;
                MustTakeRiskReward = Convert.ToDouble(max.Total) == double.MinValue || Convert.ToDouble(min.Total) == double.MaxValue || Convert.ToDouble(min.Total) == 0 ? "0" :
                                        (Convert.ToDouble(max.Total) / Math.Abs(Convert.ToDouble(min.Total))).ToString("#,##0.00;(#,##0.00)");
                MustTakeRisk = null;
                MustTakeRisk = Convert.ToDouble(min.Total) == double.MaxValue ? "0" : Convert.ToDouble(min.Total).ToString("#,##0;(#,##0)");
                MustTakeRiskDate = null;
                MustTakeRiskDate = Convert.ToDateTime(minItems.DateDisplay) == DateTime.Today ? "" : Convert.ToDateTime(minItems.DateDisplay).ToString("MM/yyyy");
                MustTakeWin = null;
                MustTakeWin = Convert.ToDouble(max.Total) == double.MinValue ? "0" : Convert.ToDouble(max.Total).ToString("#,##0;(#,##0)");
                MustTakeWinDate = null;
                MustTakeWinDate = Convert.ToDateTime(maxItems.DateDisplay) == DateTime.Today ? "" : Convert.ToDateTime(maxItems.DateDisplay).ToString("MM/yyyy");
                MustTakeMaxDrawDown = bidDrawDown == double.MaxValue ? "0" : bidDrawDown.ToString("#,##0;(#,##0)");
                MustTakeMaxDrawDownDate = null;
                if (startBidDrawdownDate == endBidDrawdownDate)
                {
                    MustTakeMaxDrawDownDate = startBidDrawdownDate == DateTime.Today && endBidDrawdownDate == DateTime.Today ? "" :
                        endBidDrawdownDate.ToString("MM/yyyy");
                }
                else
                {
                    MustTakeMaxDrawDownDate = startBidDrawdownDate == DateTime.Today && endBidDrawdownDate == DateTime.Today ? "" :
                        endBidDrawdownDate.ToString("MM/yyyy") + " - " + startBidDrawdownDate.ToString("MM/yyyy");
                }
            }
        }

        /// <summary>
        /// Gets the risk.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="win">The win.</param>
        /// <returns></returns>
        private double? GetRisk(double? min, double? max, double? win)
        {
            double? risk = (win * max) / ((1 - win) * Math.Abs(min.GetValueOrDefault()));
            if (risk.HasValue && double.IsInfinity(risk.Value))
                risk = 100;
            return risk;
        }

        /// <summary>
        /// Creates the cumiliative dart.
        /// </summary>
        /// <param name="monthlyDartHash">The monthly dart hash.</param>
        /// <returns></returns>
        private PlotModel CreateCumiliativeDart(Dictionary<DateTime, PNL> monthlyDartHash)
        {
            Dictionary<DateTime, double> cumiliativePnlHash = new Dictionary<DateTime, double>();
            var plotModel2 = new PlotModel();
            var c = OxyColors.DarkBlue;
            LinearAxis lAxs = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Key = "Y3Axis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.05,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 0.995,
                StringFormat = "$0,00",
                IntervalLength = 40
            };
            LinearAxis lAxs1 = new LinearAxis()
            {
                Position = AxisPosition.Right,
                Key = "Y4Axis",
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.05,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 0.995,
                StringFormat = "$0,00",
                IntervalLength = 40,
                TextColor = OxyColors.Transparent
            };
            plotModel2.Axes.Add(lAxs);
            plotModel2.Axes.Add(lAxs1);
            // X Axis
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 0,
                StringFormat = "0",
                MajorStep = 1,
                IsPanEnabled = true,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                TickStyle = TickStyle.None,
                FontSize = 10,
                IsTickCentered = true,
                Key = "XAxisBCategory",
                GapWidth = 0.05,
                AxisTitleDistance = 2,
                AxisTickToLabelDistance = 0,
                // TextColor = OxyColors.Transparent
            };
            plotModel2.Axes.Add(categoryAxis);
            var areaSeries1 = new AreaSeries()
            {
                Fill = OxyColors.LightBlue,
                DataFieldX2 = "Time",
                DataFieldY2 = "Minimum",
                Color = OxyColors.Black,
                StrokeThickness = 1,
                MarkerFill = OxyColors.Transparent,
                DataFieldX = "Time",
                DataFieldY = "Maximum",
                LineStyle = LineStyle.Solid,
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
                XAxisKey = "XAxisBCategory",
                YAxisKey = "Y4Axis"
            };
            double cumiliativeDart = 0.0;
            int index = 0;
            foreach (var item in monthlyDartHash)
            {
                DateTime date = item.Key;
                double dart = (double)item.Value.DART;
                cumiliativeDart += dart;
                cumiliativePnlHash.Add(date, cumiliativeDart);
            }
            foreach (var item in cumiliativePnlHash)
            {
                string date = item.Key.ToString("yyyy-MM");
                categoryAxis.Labels.Add(date);
                double dart = (double)item.Value;
                areaSeries1.Points.Add(new DataPoint(index, dart));
                areaSeries1.Points2.Add(new DataPoint(index, 0));
                index++;
            }
            if (cumiliativePnlHash.Count > 0)
            {
                string startDate = cumiliativePnlHash.Keys.Min().ToString("yyyy-MM");
                string endDate = cumiliativePnlHash.Keys.Max().ToString("yyyy-MM");
                areaSeries1.Title = "Cumiliative DART" + ' ' + startDate + '-' + endDate;
                plotModel2.Series.Add(areaSeries1);
                double max = cumiliativePnlHash.Values.Max();
                double min = cumiliativePnlHash.Values.Min();
                lAxs1.Maximum = max + (0.01 * max);
                lAxs1.Minimum = min + (0.01 * min);
            }
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
            };
            plotModel2.Legends.Add(l);

            plotModel2.IsLegendVisible = false;
            return plotModel2;

        }

        /// <summary>
        /// Creates the monthly dart plot.
        /// </summary>
        /// <param name="monthlyDartHash">The monthly dart hash.</param>
        /// <returns></returns>
        private PlotModel CreateMonthlyDARTPlot(Dictionary<DateTime, PNL> monthlyDartHash)
        {
            string dart = "", Datext = "", costtext = "";
            if (isDARTChecked)
            {
                dart = "DART";
            }
            if (isDAChecked)
            {
                Datext = "DA";
            }
            if (isCostChecked)
            {
                costtext = "Cost";

            }
            mBidId = 0;
            string title = "Portfolio";
            var plotModel1 = new PlotModel { Title = title };
            var c = OxyColors.DarkBlue;
            List<double> dartList = new List<double>();
            #region Y Axis
            LinearAxis lAxs = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Key = "Y1Axis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                StringFormat = "$0,00",
                EndPosition = 1

            };
            LinearAxis lAxs1 = new LinearAxis()
            {
                Position = AxisPosition.Right,
                Key = "YAxis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                StringFormat = "$0,00",
                EndPosition = 1

            };
            plotModel1.Axes.Add(lAxs);
            plotModel1.Axes.Add(lAxs1);
            #endregion

            var dataItemValues = new Collection<Item>();
            var dataItemValues1 = new Collection<Item>();
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 90,
                StringFormat = "0",
                MajorStep = 1,
                IsPanEnabled = true,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                TickStyle = TickStyle.Outside,
                FontSize = 10,
                IsTickCentered = true,
                Key = "XAxisCategory",
                GapWidth = 0.05,
                AxisTitleDistance = 30
            };
            plotModel1.Axes.Add(categoryAxis);

            // plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 70);
            var colSeries1 = new BarSeries();
            // string colTitle = dart;
            colSeries1 = new BarSeries()
            {
                Title = dart,
                YAxisKey = "XAxisCategory",
                XAxisKey = "YAxis",
                TrackerFormatString = "{0}\n{1:M/yy}\n{2:$0,00}",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,
            };

            foreach (var item in monthlyDartHash.OrderBy(key => key.Key))
            {
                string date = item.Key.ToString("yyyy-MM-dd");
                PNL monthlyPnl = item.Value;
                dartList.Add(monthlyPnl.DART);
                categoryAxis.Labels.Add(date);
                double finalDart = Math.Round(monthlyPnl.DART, 1);
                var colItem1 = new BarItem(finalDart, mBidId);
                colSeries1.Items.Add(colItem1);
                dataItemValues.Add(new Item() { X = mBidId, Y = monthlyPnl.DA });
                dataItemValues1.Add(new Item() { X = mBidId, Y = monthlyPnl.Cost });
                mBidId++;
            }
            if (isDARTChecked)
            {
                plotModel1.Series.Add(colSeries1);
            }
            else
            {

            }

            if (isDAChecked)
            {
                LineSeries lineSeries1 = new LineSeries();
                lineSeries1 = new LineSeries()
                {
                    CanTrackerInterpolatePoints = false,
                    DataFieldX = "X",
                    DataFieldY = "Y",
                    ItemsSource = dataItemValues,
                    TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
                    YAxisKey = "Y1Axis",
                    XAxisKey = "XAxisCategory",
                    MarkerType = MarkerType.Diamond,
                    MarkerSize = 2,
                    MarkerStrokeThickness = 0
                };
                lineSeries1.Title = Datext;
                lineSeries1.Color = OxyColors.Green;
                lineSeries1.MarkerFill = OxyColors.Green;
                plotModel1.Series.Add(lineSeries1);

            }
            else
            {

            }

            //
            if (isCostChecked)
            {
                LineSeries lineSeries2 = new LineSeries();
                lineSeries2 = new LineSeries()
                {
                    CanTrackerInterpolatePoints = false,
                    DataFieldX = "X",
                    DataFieldY = "Y",
                    ItemsSource = dataItemValues1,
                    TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
                    YAxisKey = "Y1Axis",
                    XAxisKey = "XAxisCategory",
                    MarkerType = MarkerType.Diamond,
                    MarkerSize = 2,
                    MarkerStrokeThickness = 0

                };
                lineSeries2.Title = costtext;
                lineSeries2.Color = OxyColors.Orange;
                lineSeries2.MarkerFill = OxyColors.Orange;
                plotModel1.Series.Add(lineSeries2);
            }
            else
            {

            }


            if (dataItemValues.Count > 0 && dataItemValues1.Count > 0)
            {
                double lineMaxLarge = Math.Max(Math.Abs(dataItemValues.Max(x => x.Y)), Math.Abs(dataItemValues1.Max(x => x.Y)));
                double lineMaxSmall = Math.Min(Math.Abs(dataItemValues.Min(x => x.Y)), Math.Abs(dataItemValues1.Min(x => x.Y)));
                double lineSmall = Math.Abs(lineMaxLarge) > Math.Abs(lineMaxSmall) ? Math.Abs(lineMaxLarge) : Math.Abs(lineMaxSmall);
                double maxDartLarge = Math.Abs(dartList.Max());
                double maxDartSmall = Math.Abs(dartList.Min());
                double dartSmall = Math.Abs(maxDartLarge) > Math.Abs(maxDartSmall) ? Math.Abs(maxDartLarge) : Math.Abs(maxDartSmall);
                lAxs.Maximum = lineSmall * 2;
                lAxs.Minimum = (lineSmall * -1) * 2;

                lAxs1.Maximum = dartSmall + (0.3 * dartSmall); //* 2;
                lAxs1.Minimum = (dartSmall * -1) + (0.3 * (dartSmall * -1));// *2; 
            }
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
            };

            plotModel1.Legends.Add(l);

            return plotModel1;
        }


        /// <summary>
        /// Finals the submit.
        /// </summary>
        /// <param name="isXml">if set to <c>true</c> [is XML].</param>
        private void FinalSubmit(bool isXml)
        {
            if (PathList == null || MarketComboSelectedValue == null)
            {
                return;
            }
            if (MarketComboSelectedValue == "SPP" && !ValidateCRRs())
            {
                return;
            }
            if (GetMarketKey() == 9)
            {
                List<CRRBid> crrList = GetCRRList();
                #region CRR
                string auctionName = AuctionSelectedItem;
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.OpenTimeout = new TimeSpan(0, 360, 0);
                myBinding.SendTimeout = new TimeSpan(0, 360, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 360, 0);
                myBinding.CloseTimeout = new TimeSpan(0, 360, 0);
                myBinding.TransactionFlow = false;
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.MaxReceivedMessageSize = 5242880;// int.MaxValue;
                myBinding.MaxBufferPoolSize = 524288; //int.MaxValue;
                myBinding.MaxBufferSize = 524288;//int.MaxValue;
                myBinding.TransferMode = TransferMode.Streamed;
                myBinding.ReaderQuotas.MaxArrayLength = 16384;// 5000000;
                myBinding.ReaderQuotas.MaxStringContentLength = 5242880;
                myBinding.ReaderQuotas.MaxBytesPerRead = 4096;
                myBinding.ReaderQuotas.MaxNameTableCharCount = 16384;
                ChannelFactory<IBidCRRSubmit> pipeFactory = new ChannelFactory<IBidCRRSubmit>(myBinding, new EndpointAddress(mEndPointCRR));
                foreach (OperationDescription op in pipeFactory.Endpoint.Contract.Operations)
                {
                    DataContractSerializerOperationBehavior dataContractBehavior =
                                op.Behaviors.Find<DataContractSerializerOperationBehavior>()
                                as DataContractSerializerOperationBehavior;
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                    }
                }
                try
                {
                    mCRRCalculationProxy = pipeFactory.CreateChannel();
                }
                catch (Exception ex)
                {
                }
                CRRBid[] crrs = crrList.ToArray();
                string resultString = string.Empty; StringBuilder buildHelper = new StringBuilder();
                if (auctionName.ToLower().Contains("round"))
                {
                    auctionName = auctionName.Substring(0, 20);
                }
                if (!isXml)
                {
                    resultString = mCRRCalculationProxy.Post(crrs, DateTime.Today, PortfolioComboSelectedItem.ID, auctionName, auctionName, SelectedRound, TypeText);
                }
                else
                {
                    
                    //buildHelper = mCRRCalculationProxy.CreateSubmissionFile(crrs, DateTime.Today, PortfolioComboSelectedItem.Name, (AuctionSelectedItem ?? ""), SelectedRound, mUser, TypeText);
                    buildHelper = mCRRCalculationProxy.CreateAnnualSubmissionFile(crrs, DateTime.Today, PortfolioComboSelectedItem.Name, (AuctionSelectedItem ?? ""), SelectedRound, mUser, TypeText);
                    if (buildHelper.ToString().Length > 0)
                    {
                        if (!Directory.Exists(@"D:\CRRAnnual\CRRSubmission\"))
                        {
                            Directory.CreateDirectory(@"D:\CRRAnnual\CRRSubmission\");
                        }
                        TextWriter writer = new StreamWriter(@"D:\CRRAnnual\CRRSubmission\" + PortfolioComboSelectedItem.Name + " " + auctionName + ".xml");
                        writer.Write(buildHelper.ToString());
                        writer.Flush();
                        writer.Close();
                        resultString = "Successfully created the file at " + @"D:\CRRAnnual\CRRSubmission\" + PortfolioComboSelectedItem.Name + " " + AuctionSelectedItem + ".xml";
                    }
                }
                pipeFactory.Close();
                MessageBox.Show(resultString);
                #endregion CRR
            }
            Clear();
        }

        private List<CRRBid> GetCRRList()
        {
            int i = 0;
            List<int> bidIdList = new List<int>();
            List<CRRBid> crrList = new List<CRRBid>();
            foreach (FTRBid FTRBid in PathList)
            {
                if (FTRBid.Status.ToUpper() == "VALID")
                {
                    continue;
                }
                bidIdList.Add(FTRBid.ID);
                if (FTRBid.MW1 != null && FTRBid.Price1 != null)
                {
                    CRRBid crr = new CRRBid();
                    BidValues values = new BidValues(); List<BidValues> valuesList = new List<BidValues>();
                    crr.ID = FTRBid.ID;
                    crr.TradeType = FTRBid.TradeType;
                    crr.Trade = FTRBid.HedgeType;
                    crr.PathSource = FTRBid.Source;
                    crr.PathSink = FTRBid.Sink;
                    crr.Class = FTRBid.ClassType;
                    crr.Period = FTRBid.PeriodName;
                    crr.Hedge = FTRBid.HedgeType;

                    values.MW = (double)FTRBid.MW1;
                    values.Price = (double)FTRBid.Price1;// *FTRBid.PeriodHours;                    
                    values.Hour = 1;
                    valuesList.Add(values);
                    crr.Bidvals = valuesList.ToArray();
                    crrList.Add(crr);
                }
                if (FTRBid.MW2 != null && FTRBid.Price2 != null)
                {
                    CRRBid crr = new CRRBid();
                    BidValues values = new BidValues(); List<BidValues> valuesList = new List<BidValues>();
                    crr.ID = FTRBid.ID;
                    crr.TradeType = FTRBid.TradeType;
                    crr.Trade = FTRBid.HedgeType;
                    crr.PathSource = FTRBid.Source;
                    crr.PathSink = FTRBid.Sink;
                    crr.Class = FTRBid.ClassType;
                    crr.Period = FTRBid.PeriodName;
                    crr.Hedge = FTRBid.HedgeType;

                    values.MW = (double)FTRBid.MW2;
                    values.Price = (double)FTRBid.Price2;// *FTRBid.PeriodHours;                    
                    values.Hour = 1;
                    valuesList.Add(values);
                    crr.Bidvals = valuesList.ToArray();
                    crrList.Add(crr);
                }
                if (FTRBid.MW3 != null && FTRBid.Price3 != null)
                {
                    CRRBid crr = new CRRBid();
                    BidValues values = new BidValues(); List<BidValues> valuesList = new List<BidValues>();
                    crr.ID = FTRBid.ID;
                    crr.TradeType = FTRBid.TradeType;
                    crr.Trade = FTRBid.HedgeType;
                    crr.PathSource = FTRBid.Source;
                    crr.PathSink = FTRBid.Sink;
                    crr.Class = FTRBid.ClassType;
                    crr.Period = FTRBid.PeriodName;
                    crr.Hedge = FTRBid.HedgeType;
                    values.MW = (double)FTRBid.MW3;
                    values.Price = (double)FTRBid.Price3;// *FTRBid.PeriodHours;                    
                    values.Hour = 1;
                    valuesList.Add(values);
                    crr.Bidvals = valuesList.ToArray();
                    crrList.Add(crr);
                }
                if (FTRBid.MW4 != null && FTRBid.Price4 != null)
                {
                    CRRBid crr = new CRRBid();
                    BidValues values = new BidValues(); List<BidValues> valuesList = new List<BidValues>();
                    crr.ID = FTRBid.ID;
                    crr.TradeType = FTRBid.TradeType;
                    crr.Trade = FTRBid.HedgeType;
                    crr.PathSource = FTRBid.Source;
                    crr.PathSink = FTRBid.Sink;
                    crr.Class = FTRBid.ClassType;
                    crr.Period = FTRBid.PeriodName;
                    crr.Hedge = FTRBid.HedgeType;
                    values.MW = (double)FTRBid.MW4;
                    values.Price = (double)FTRBid.Price4;// *FTRBid.PeriodHours;                    
                    values.Hour = 1;
                    valuesList.Add(values);
                    crr.Bidvals = valuesList.ToArray();
                    crrList.Add(crr);
                }
                if (FTRBid.MW5 != null && FTRBid.Price5 != null)
                {
                    CRRBid crr = new CRRBid();
                    BidValues values = new BidValues(); List<BidValues> valuesList = new List<BidValues>();
                    crr.ID = FTRBid.ID;
                    crr.TradeType = FTRBid.TradeType;
                    crr.Trade = FTRBid.HedgeType;
                    crr.PathSource = FTRBid.Source;
                    crr.PathSink = FTRBid.Sink;
                    crr.Class = FTRBid.ClassType;
                    crr.Period = FTRBid.PeriodName;
                    crr.Hedge = FTRBid.HedgeType;
                    values.MW = (double)FTRBid.MW5;
                    values.Price = (double)FTRBid.Price5;// *FTRBid.PeriodHours;                    
                    values.Hour = 1;
                    valuesList.Add(values);
                    crr.Bidvals = valuesList.ToArray();
                    crrList.Add(crr);
                }
                if (FTRBid.MW6 != null && FTRBid.Price6 != null)
                {
                    CRRBid crr = new CRRBid();
                    BidValues values = new BidValues(); List<BidValues> valuesList = new List<BidValues>();
                    crr.ID = FTRBid.ID;
                    crr.TradeType = FTRBid.TradeType;
                    crr.Trade = FTRBid.HedgeType;
                    crr.PathSource = FTRBid.Source;
                    crr.PathSink = FTRBid.Sink;
                    crr.Class = FTRBid.ClassType;
                    crr.Period = FTRBid.PeriodName;
                    crr.Hedge = FTRBid.HedgeType;
                    values.MW = (double)FTRBid.MW6;
                    values.Price = (double)FTRBid.Price6;// *FTRBid.PeriodHours;                    
                    values.Hour = 1;
                    valuesList.Add(values);
                    crr.Bidvals = valuesList.ToArray();
                    crrList.Add(crr);
                }
                i++;
            }
            return crrList;
        }
        /// <summary>
        /// Validates the CRRS.
        /// </summary>
        /// <returns></returns>
        private bool ValidateCRRs()
        {
            if (PathList == null)
            {
                return false;
            }
            bool isValid = true;
            foreach (FTRBid item in PathList)
            {
                try
                {
                    if (item.MW1 > item.MW2)
                    {
                        isValid = false;
                        MessageBox.Show("Mw value : " + item.MW1 + "is > its next segment value: " + item.MW2 +
                            " for source: " + item.Source + "  sink : " + item.Sink + "\n Please fix the error and then submit");
                        break;
                    }
                    if (item.MW2 > item.MW3)
                    {
                        isValid = false;
                        MessageBox.Show("Mw value : " + item.MW2 + "is > its next segment value: " + item.MW3 +
                           " for source: " + item.Source + "  sink : " + item.Sink + "\n Please fix the error and then submit");
                        break;
                    }
                    if (item.MW3 > item.MW4)
                    {
                        isValid = false;
                        MessageBox.Show("Mw value : " + item.MW3 + "is > its next segment value: " + item.MW4 +
                           " for source: " + item.Source + "  sink : " + item.Sink + "\n Please fix the error and then submit");
                        break;
                    }
                    if (item.MW4 > item.MW5)
                    {
                        isValid = false;
                        MessageBox.Show("Mw value : " + item.MW4 + "is > its next segment value: " + item.MW5 +
                           " for source: " + item.Source + "  sink : " + item.Sink + "\n Please fix the error and then submit");
                        break;
                    }
                    if (item.MW5 > item.MW6)
                    {
                        isValid = false;
                        MessageBox.Show("Mw value : " + item.MW5 + "is > its next segment value: " + item.MW6 +
                           " for source: " + item.Source + "  sink : " + item.Sink + "\n Please fix the error and then submit");
                        break;
                    }
                    if (item.Price1 != item.Price2)
                    {
                        MessageBox.Show("Price1 should equal Price2 of source : " + item.Source + " sink : " + item.Sink);
                        isValid = false;
                        break;
                    }
                    if (item.Price2 != null && item.Price3 != null)
                    {
                        isValid = item.TradeType.ToUpper().StartsWith("BUY") ? item.Price2 > item.Price3 : item.Price2 < item.Price3;
                        if (!isValid)
                        {
                            MessageBox.Show("Error with Price2 and Price3 of source : " + item.Source + " sink : " + item.Sink);
                            break;
                        }
                    }
                    if (item.Price3 != null && item.Price4 != null)
                    {
                        isValid = item.TradeType.ToUpper().StartsWith("BUY") ? item.Price3 > item.Price4 : item.Price3 < item.Price4;
                        if (!isValid)
                        {
                            MessageBox.Show("Error with Price3 and Price4 of source : " + item.Source + " sink : " + item.Sink);
                            break;
                        }
                    }
                    if (item.Price4 != null && item.Price5 != null)
                    {
                        isValid = item.TradeType.ToUpper().StartsWith("BUY") ? item.Price4 > item.Price5 : item.Price4 < item.Price5;
                        if (!isValid)
                        {
                            MessageBox.Show("Error with Price4 and Price5 of source : " + item.Source + " sink : " + item.Sink);
                            break;
                        }
                    }
                    if (item.Price5 != null && item.Price6 != null)
                    {
                        isValid = item.TradeType.ToUpper().StartsWith("BUY") ? item.Price5 > item.Price6 : item.Price5 < item.Price6;
                        if (!isValid)
                        {
                            MessageBox.Show("Error with Price5 and Price6 of source : " + item.Source + " sink : " + item.Sink);
                            break;
                        }
                    }
                }
                catch
                {
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }

        /// <summary>
        /// Sets the user portfolio list.
        /// </summary>
        private void SetUserPortfolioList()
        {
            TraderPortfolioComboList = null;
            List<CRRAuction> auctionList = _dataService.GetFtrAuctions(MarketComboSelectedValue);
            List<CRRAuction> addPortfolioList = new List<CRRAuction>();
            int marketKey = GetMarketKey();
            List<Tuple<int, string>> portfolioList = _dataService.GetSppMisoPortfolioList(MarketComboSelectedValue);
            if (!((mUser == "sangramp") || (mUser == "anitad" || mUser == "abhishekh")))
            {
                portfolioList.RemoveAll(x => x.Item2 == "SIGMA_CRR_STRAT");
                portfolioList.RemoveAll(x => x.Item2 == "RISK_SIGMA_CRR");
            }
            foreach (CRRAuction auction in auctionList)
            {
                foreach (Tuple<int, string> sppPortfolio in portfolioList)
                {
                    CRRAuction addPortfolio = new CRRAuction();
                    addPortfolio.Key = auction.Key;
                    addPortfolio.Name = auction.Name + "-" + sppPortfolio.Item2;
                    addPortfolioList.Add(addPortfolio);
                }
            }
            TraderPortfolioComboList = null;
            TraderPortfolioComboList = addPortfolioList;
        }
        /// <summary>
        /// Filters all.
        /// </summary>
        /// <param name="asBidMarketDateTimeHash">As bid market date time hash.</param>
        /// <param name="mustTakeMarketDateTimeHash">The must take market date time hash.</param>
        private void FilterAll(out Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> asBidMarketDateTimeHash,
                                out Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mustTakeMarketDateTimeHash)
        {
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mustTakeSendMarketDateTimeHash = DailyChecked ? mMustTakeDailyMarketDateTimeHash : mMustTakeMarketDateTimeHash;
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> asBidSendMarketDateTimeHash = DailyChecked ? mAsBidDailyMarketDateTimeHash : mAsBidMarketDateTimeHash;
            asBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            mustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            for (int i = 0; i < 2; i++)
            {

                List<DateTime> marketDateTimeList = i == 0 ? asBidSendMarketDateTimeHash.Keys.ToList<DateTime>() : mustTakeSendMarketDateTimeHash.Keys.ToList<DateTime>();
                foreach (DateTime marketDateTime in marketDateTimeList)
                {
                    //if (IsValid(marketDateTime))
                    {
                        Dictionary<string, Tuple<FTRBid, PNL>> copyBidHash = new Dictionary<string, Tuple<FTRBid, PNL>>();
                        Dictionary<string, Tuple<FTRBid, PNL>> bidHash = i == 0 ? asBidSendMarketDateTimeHash[marketDateTime] : mustTakeSendMarketDateTimeHash[marketDateTime];
                        List<string> bidList = bidHash.Keys.ToList<string>();
                        foreach (string bid in bidList)
                        {
                            Tuple<FTRBid, PNL> tuple = bidHash[bid];
                            Tuple<FTRBid, PNL> copyTuple = new Tuple<FTRBid, PNL>(new FTRBid(tuple.Item1), new PNL(tuple.Item2));
                            copyBidHash.Add(bid, copyTuple);
                        }
                        if (i == 0)
                        {
                            asBidMarketDateTimeHash.Add(marketDateTime, copyBidHash);
                        }
                        else
                        {
                            mustTakeMarketDateTimeHash.Add(marketDateTime, copyBidHash);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total PNL hash.
        /// </summary>
        /// <param name="marketDateTimeHash">The market date time hash.</param>
        /// <param name="totalBidPnl">The total bid PNL.</param>
        /// <param name="totalBidWin">The total bid win.</param>
        /// <param name="totalBidMW">The total bid mw.</param>
        /// <param name="bidMax">The bid maximum.</param>
        /// <param name="bidMin">The bid minimum.</param>
        /// <param name="bidWinDate">The bid win date.</param>
        /// <param name="bidDrawDown">The bid draw down.</param>
        /// <param name="bidRiskDate">The bid risk date.</param>
        /// <param name="startBidDrawdownDate">The start bid drawdown date.</param>
        /// <param name="endBidDrawdownDate">The end bid drawdown date.</param>
        /// <param name="totalPnlHash">The total PNL hash.</param>
        /// <param name="totalDailyPnlHash">The total daily PNL hash.</param>
        private void GetTotalPnlHash(Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> marketDateTimeHash, out double totalBidPnl, out double totalBidWin, out double totalBidMW,
                                                           out double bidMax, out double bidMin, out DateTime bidWinDate, out double bidDrawDown, out DateTime bidRiskDate,
                                                           out DateTime startBidDrawdownDate, out DateTime endBidDrawdownDate, out Dictionary<DateTime, PNL> totalPnlHash,
                                                           out Dictionary<DateTime, PNL> totalDailyPnlHash)
        {
            totalBidPnl = 0;
            totalBidWin = 0;
            totalBidMW = 0;
            bidMax = double.MinValue;
            bidMin = double.MaxValue;
            List<DateTime> marketDateTimeKeys = marketDateTimeHash.Keys.ToList<DateTime>();
            totalDailyPnlHash = new Dictionary<DateTime, PNL>();
            totalPnlHash = new Dictionary<DateTime, PNL>();
            bidDrawDown = double.MaxValue;
            bidRiskDate = DateTime.Today;
            bidWinDate = DateTime.Today;
            bool setBidEndDate = false;
            double tempBidDrawDown = 0;
            startBidDrawdownDate = DateTime.Today;
            endBidDrawdownDate = DateTime.Today;
            DateTime tempBidDrawDownDate = DateTime.Today;
            marketDateTimeKeys.Sort();
            DateTime lastDate = DateTime.MaxValue;
            PNL totalDailyPnl = new PNL();
            PNL totalPnl = new PNL();
            foreach (DateTime marketDateTime in marketDateTimeKeys)
            {
                DateTime compareDate = marketDateTime.Hour == 0 ? marketDateTime.Date.AddDays(-1).Date : marketDateTime.Date;
                Dictionary<string, Tuple<FTRBid, PNL>> pathHash = marketDateTimeHash[marketDateTime];
                totalPnl = new PNL();
                totalPnlHash.Add(marketDateTime, totalPnl);
                if (lastDate.Date != compareDate)
                {
                    totalDailyPnl = new PNL();
                    lastDate = compareDate;
                    totalDailyPnlHash.Add(compareDate, totalDailyPnl);
                }
                foreach (Tuple<FTRBid, PNL> tuple in pathHash.Values)
                {
                    FTRBid path = tuple.Item1;
                    PNL pnl = tuple.Item2;
                    totalPnl.DA += pnl.DA;
                    totalPnl.Cost += pnl.Cost;
                    totalPnl.DART += pnl.DART;
                    totalDailyPnl.DA += pnl.DA;
                    totalDailyPnl.Cost += pnl.Cost;
                    totalDailyPnl.DART += pnl.DART;
                    //totalBidMW += path.MW;
                    //totalBidPnl += pnl.DART * path.MW;
                }
                /*if (DailyChecked && lastDate != compareDate)
                {
                    continue;
                }*/
                if (bidMax < totalPnl.DART)
                {
                    bidMax = totalPnl.DART;
                    bidWinDate = marketDateTime;
                }
                if (bidMin > totalPnl.DART)
                {
                    bidMin = totalPnl.DART;
                    bidRiskDate = marketDateTime;
                }
                if (totalPnl.DART > 0)
                {
                    if (setBidEndDate)
                    {
                        endBidDrawdownDate = marketDateTime;
                        setBidEndDate = false;
                    }
                    tempBidDrawDown = 0;
                    setBidEndDate = false;
                    totalBidWin++;
                }
                else
                {
                    if (tempBidDrawDown == 0)
                    {
                        tempBidDrawDownDate = marketDateTime;
                    }
                    tempBidDrawDown += totalPnl.DART;
                }
                if (tempBidDrawDown < bidDrawDown)
                {
                    bidDrawDown = tempBidDrawDown;
                    startBidDrawdownDate = tempBidDrawDownDate;
                    setBidEndDate = true;
                }
            }
        }
        /// <summary>
        /// Gets the summary node list.
        /// </summary>
        private void GetSummaryNodeList()
        {
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> asBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            FilterAll(out asBidMarketDateTimeHash, out mustTakeMarketDateTimeHash);
            //bool isPath = path != null;
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> marketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> market1DateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            // asBidTotalNodeList = new List<Node>();
            // mustTakeTotalNodeList = new List<Node>();
            if (MustTakeChecked)
            {
                marketDateTimeHash = mustTakeMarketDateTimeHash;
                market1DateTimeHash = asBidMarketDateTimeHash;
            }
            else
            {
                marketDateTimeHash = asBidMarketDateTimeHash;
                market1DateTimeHash = mustTakeMarketDateTimeHash;
            }
            double totalBidPnl = 0;
            double totalBidWin = 0;
            double totalBidMW = 0;
            double bidMax = double.MinValue;
            double bidMin = double.MaxValue;
            DateTime bidRiskDate = DateTime.Today;
            DateTime bidWinDate = DateTime.Today;
            double bidDrawDown = double.MaxValue;
            DateTime startBidDrawdownDate = DateTime.Today;
            DateTime endBidDrawdownDate = DateTime.Today;
            Dictionary<DateTime, PNL> asBidTotalPnlHash = new Dictionary<DateTime, PNL>();
            Dictionary<DateTime, PNL> asBidDailyTotalPnlHash = new Dictionary<DateTime, PNL>();
            GetTotalPnlHash(asBidMarketDateTimeHash, out totalBidPnl, out totalBidWin, out totalBidMW, out bidMax, out bidMin, out bidWinDate,
                                                        out bidDrawDown, out bidRiskDate, out startBidDrawdownDate, out endBidDrawdownDate,
                                                        out asBidTotalPnlHash, out asBidDailyTotalPnlHash);

            AsBidSumText = null;
            AsBidSumText = totalBidPnl.ToString("#,##0;(#,##0)");
            AsBidWinPerText = null;
            AsBidWinPerText = mAsBidMarketDateTimeHash.Count == 0 ? "0.00%" : (totalBidWin / asBidMarketDateTimeHash.Count).ToString("0.00%");
            AsBidDolMW = null;
            AsBidDolMW = (totalBidPnl / totalBidMW).ToString("#,##0.00");
            AsBidRiskReward = null;
            AsBidRiskReward = bidMax == double.MinValue || bidMin == double.MaxValue || bidMin == 0 ? "0" :
                                (bidMax / Math.Abs(bidMin)).ToString("#,##0.00;(#,##0.00)");
            AsBidRisk = null;
            AsBidRisk = bidMin == double.MaxValue ? "0" : bidMin.ToString("#,##0;(#,##0)");
            AsBidRiskDate = null;
            AsBidRiskDate = bidRiskDate == DateTime.Today ? "" : bidRiskDate.ToString("MM/dd/yy HH");
            AsBidWin = null;
            AsBidWin = bidMax == double.MinValue ? "0" : bidMax.ToString("#,##0;(#,##0)");
            AsBidWinDate = null;
            AsBidWinDate = bidWinDate == DateTime.Today ? "" : bidWinDate.ToString("MM/dd/yy HH");
            AsBidMaxDrawDown = null;
            AsBidMaxDrawDown = bidDrawDown == double.MaxValue ? "0" : bidDrawDown.ToString("#,##0;(#,##0)");
            AsBidMaxDrawDownDate = null;
            AsBidMaxDrawDownDate = startBidDrawdownDate == DateTime.Today && endBidDrawdownDate == DateTime.Today ? "" :
                                            startBidDrawdownDate.ToString("MM/dd/yy HH") + " - " + endBidDrawdownDate.ToString("MM/dd/yy HH");

            Dictionary<DateTime, PNL> mustTakeTotalPnlHash = new Dictionary<DateTime, PNL>();
            Dictionary<DateTime, PNL> mustTakeTotalDailyPnlHash = new Dictionary<DateTime, PNL>();
            GetTotalPnlHash(mustTakeMarketDateTimeHash, out totalBidPnl, out totalBidWin, out totalBidMW, out bidMax, out bidMin, out bidWinDate,
                                                        out bidDrawDown, out bidRiskDate, out startBidDrawdownDate, out endBidDrawdownDate, out mustTakeTotalPnlHash,
                                                        out mustTakeTotalDailyPnlHash);

            MustTakeSumText = null;
            MustTakeSumText = totalBidPnl.ToString("#,##0;(#,##0)");
            MustTakeWinPerText = null;
            MustTakeWinPerText = mAsBidMarketDateTimeHash.Count == 0 ? "0.00%" : (totalBidWin / mustTakeMarketDateTimeHash.Count).ToString("0.00%");
            MustTakeDolMW = null;
            MustTakeDolMW = (totalBidPnl / totalBidMW).ToString("#,##0.00");
            MustTakeRiskReward = null;
            MustTakeRiskReward = bidMax == double.MinValue || bidMin == double.MaxValue || bidMin == 0 ? "0" :
                                (bidMax / Math.Abs(bidMin)).ToString("#,##0.00;(#,##0.00)");
            MustTakeRisk = null;
            MustTakeRisk = bidMin == double.MaxValue ? "0" : bidMin.ToString("#,##0;(#,##0)");
            MustTakeRiskDate = null;
            MustTakeRiskDate = bidRiskDate == DateTime.Today ? "" : bidRiskDate.ToString("MM/dd/yy HH");
            MustTakeWin = null;
            MustTakeWin = bidMax == double.MinValue ? "0" : bidMax.ToString("#,##0;(#,##0)");
            MustTakeWinDate = null;
            MustTakeWinDate = bidWinDate == DateTime.Today ? "" : bidWinDate.ToString("MM/dd/yy HH");
            MustTakeMaxDrawDown = bidDrawDown == double.MaxValue ? "0" : bidDrawDown.ToString("#,##0;(#,##0)");
            MustTakeMaxDrawDownDate = null;
            MustTakeMaxDrawDownDate = startBidDrawdownDate == DateTime.Today && endBidDrawdownDate == DateTime.Today ? "" :
                startBidDrawdownDate.ToString("MM/dd/yy HH") + " - " + endBidDrawdownDate.ToString("MM/dd/yy HH");


            //for (int i = 0; i < 2; i++)
            //{
            //    Dictionary<DateTime, PNL> totalPnlHash = i == 0 ? asBidTotalPnlHash : mustTakeTotalPnlHash;
            //    List<DateTime> totalPnlDateTimeKeys = totalPnlHash.Keys.ToList<DateTime>();
            //    totalPnlDateTimeKeys.Sort();
            //    Node daNode = new Node();
            //    daNode.Market = 1;
            //    daNode.NodeName = "DA";
            //    daNode.TimePriceList = new List<TimePrice>();
            //    Node rtNode = new Node();
            //    rtNode.Market = 1;
            //    rtNode.NodeName = "RT";
            //    rtNode.TimePriceList = new List<TimePrice>();
            //    Node dartNode = new Node();
            //    dartNode.Market = 1;
            //    dartNode.NodeName = "DART";
            //    dartNode.TimePriceList = new List<TimePrice>();
            //    Dictionary<DateTime, double> datePnlHash = new Dictionary<DateTime, double>();
            //    foreach (DateTime marketDateTime in totalPnlDateTimeKeys)
            //    {
            //        PNL pnl = totalPnlHash[marketDateTime];
            //        TimePrice daTimePrice = new TimePrice();
            //        daTimePrice.MarketTime = marketDateTime;
            //        daTimePrice.Price = pnl.DA;
            //        daNode.TimePriceList.Add(daTimePrice);
            //        TimePrice rtTimePrice = new TimePrice();
            //        rtTimePrice.MarketTime = marketDateTime;
            //        rtTimePrice.Price = pnl.Cost;
            //        rtNode.TimePriceList.Add(rtTimePrice);
            //        TimePrice dartTimePrice = new TimePrice();
            //        dartTimePrice.MarketTime = marketDateTime;
            //        dartTimePrice.Price = pnl.DART;
            //        dartNode.TimePriceList.Add(dartTimePrice);
            //    }
            //    if (i == 0)
            //    {
            //        asBidTotalNodeList.Add(daNode);
            //        asBidTotalNodeList.Add(rtNode);
            //        asBidTotalNodeList.Add(dartNode);
            //    }
            //    else
            //    {
            //        mustTakeTotalNodeList.Add(daNode);
            //        mustTakeTotalNodeList.Add(rtNode);
            //        mustTakeTotalNodeList.Add(dartNode);
            //    }
            //}
        }

        /// <summary>
        /// Determines whether [is date valid] [the specified month].
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns>
        ///   <c>true</c> if [is date valid] [the specified month]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDateValid(int month)
        {
            if (JanChecked && (month == 12 || month == 1 || month == 2))
            {
                return true;
            }
            if (FebChecked && (month == 12 || month == 1 || month == 2))
            {
                return true;
            }
            if (MarChecked && (month == 3 || month == 4 || month == 5))
            {
                return true;
            }
            if (AprChecked && (month == 3 || month == 4 || month == 5))
            {
                return true;
            }
            if (MayChecked && (month == 3 || month == 4 || month == 5))
            {
                return true;
            }
            if (JunChecked && (month == 6 || month == 7 || month == 8))
            {
                return true;
            }
            if (JulChecked && (month == 6 || month == 7 || month == 8))
            {
                return true;
            }
            if (AugChecked && (month == 6 || month == 7 || month == 8))
            {
                return true;
            }
            if (SepChecked && (month == 9 || month == 10 || month == 11))
            {
                return true;
            }
            if (OctChecked && (month == 9 || month == 10 || month == 11))
            {
                return true;
            }
            if (NovChecked && (month == 9 || month == 10 || month == 11))
            {
                return true;
            }
            if (DecChecked && (month == 12 || month == 1 || month == 2))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Sets the hourly pivot list.
        /// </summary>
        private void SetHourlyPivotList()
        {
            if (mHourlyPivotHash == null)
            {
                mHourlyPivotHash = null;
                return;
            }
            Dictionary<DateTime, Dictionary<int, PNL>> dailyHash = new Dictionary<DateTime, Dictionary<int, PNL>>();
            List<DateTime> dateList = mAsBidDailyMarketDateTimeHash.Keys.ToList<DateTime>();
            foreach (DateTime date in dateList)
            {
                DateTime dateKey = DateTime.Parse(date.Month + "/1/" + date.Year);
                Dictionary<int, PNL> pnlHash = new Dictionary<int, PNL>();
                if (dailyHash.ContainsKey(dateKey))
                {
                    pnlHash = dailyHash[dateKey];
                    dailyHash.Remove(dateKey);
                }
                Dictionary<string, Tuple<FTRBid, PNL>> secHash = mMustTakeDailyMarketDateTimeHash[date];
            }
            List<HourlyPivotData> hourlyPivotData = new List<HourlyPivotData>();
            string rowType;
            foreach (var nodeList in mHourlyPivotHash)
            {

                rowType = nodeList.Key;
                int counter = 0;
                foreach (var item in nodeList.Value)
                {
                    int compareCounter = nodeList.Value.Count == 3 ? 2 : 0;
                    if (!FilterDayComparisonSourceChecked && nodeList.Key != "DART" && counter == 0 && compareCounter == 2)
                    {
                        counter++;
                        continue;
                    }
                    if (!FilterDayComparisonSinkChecked && nodeList.Key != "DART" && counter == 1 && compareCounter == 2)
                    {
                        counter++;
                        continue;
                    }
                    if (!FilterDayComparisonDAChecked && nodeList.Key == "DA" && counter == compareCounter)
                    {
                        counter++;
                        continue;
                    }
                    if (!FilterDayComparisonRTChecked && nodeList.Key == "RT" && counter == compareCounter)
                    {
                        counter++;
                        continue;
                    }
                    if (!FilterDayComparisonDARTChecked && nodeList.Key == "DART" && counter == compareCounter)
                    {
                        counter++;
                        continue;
                    }
                    //
                    var itemlist = item.TimePriceList.OrderBy(i => i.MarketTime).ToArray();
                    DateTime dateCounter = DateTime.Now.Date;
                    if (itemlist.FirstOrDefault() != null)
                    {
                        dateCounter = itemlist.FirstOrDefault().MarketTime.Date;
                    }
                    double total = 0, avgcounter = 0;
                    HourlyPivotData hpdata = new HourlyPivotData();
                    foreach (var hourPrice in itemlist)
                    {
                        if (FilterDayComparisonSourceChecked.Equals(false) && (counter == 0 && nodeList.Value.Count > 1))
                        {
                            break;
                        }
                        if (FilterDayComparisonSinkChecked.Equals(false) && (counter == 1 && nodeList.Value.Count > 1))
                        {
                            break;
                        }
                        if (rowType.Equals("DART") && (counter < 2 && nodeList.Value.Count > 1))
                        {
                            break;
                        }
                        if (hourPrice.MarketTime.AddMinutes(-1).Date > dateCounter)
                        {
                            hpdata.Total = total;
                            hpdata.Average = total / avgcounter;
                            avgcounter = total = 0;
                            hpdata.Date = dateCounter;
                            hpdata.DateDisplay = dateCounter;
                            hpdata.RowDay = dateCounter.Date.ToString("ddd");
                            hourlyPivotData.Add(hpdata);
                            hpdata = new HourlyPivotData();
                            dateCounter = hourPrice.MarketTime.AddMinutes(-1).Date;
                        }
                        if (hourPrice.MarketTime.AddMinutes(-1).Date == dateCounter)
                        {
                            switch (rowType)
                            {
                                case "DA":
                                case "RT":
                                    if (counter == 0 && nodeList.Value.Count == 3)
                                    {
                                        hpdata.RowName = "Source";
                                    }
                                    if (counter == 1 && nodeList.Value.Count == 3)
                                    {
                                        hpdata.RowName = "Sink";
                                    }
                                    if (counter == 2 || nodeList.Value.Count != 3)
                                    {
                                        hpdata.RowName = "Spread";
                                    }
                                    break;
                                case "DART":
                                    if (counter == 0)
                                    {
                                        hpdata.RowName = "DA Spread";
                                    }
                                    if (counter == 1)
                                    {
                                        hpdata.RowName = "RT Spread";
                                    }
                                    if (counter == 2)
                                    {
                                        hpdata.RowName = "Spread";
                                    }
                                    break;
                            }
                            hpdata.Date = hourPrice.MarketTime.AddMinutes(-1).Date;
                            hpdata.DateDisplay = hourPrice.MarketTime.AddMinutes(-1).Date;
                            hpdata.RowDay = dateCounter.Date.ToString("ddd");
                            hpdata.RowType = rowType;
                            hpdata.RowDisplayType = rowType;
                        }
                        string hour = "HE" + hourPrice.MarketTime.Hour.ToString();
                        double? price = null;
                        if (!hourPrice.Price.Equals(double.NaN))
                        {
                            total += hourPrice.Price;
                            price = hourPrice.Price;
                            avgcounter++;
                        }
                        hpdata.GetType().GetProperty(hour).SetValue(hpdata, price, null);
                    }
                    counter++;
                    if (hpdata != null)
                    {
                        hpdata.Total = total;
                        hpdata.Average = total / avgcounter;
                        avgcounter = total = 0;
                        hourlyPivotData.Add(hpdata);
                        hpdata = null;
                    }
                }
                hourlyPivotData.RemoveAll(item => (item.Total == null && item.Average == null));
            }
            /*List<HourlyPivotData> sorttempHourlyPivotList = hourlyPivotData.Count == 0 ? hourlyPivotData : (from t in hourlyPivotData
                                                                                                            orderby t.Date descending, t.RowType.Length descending, t.RowType ascending, t.RowName descending
                                                                                                            select t).ToList();
            if (sorttempHourlyPivotList.Count > 0)
            {
                DateTime? date = sorttempHourlyPivotList[0].Date;
                string rowtype = sorttempHourlyPivotList[0].RowDisplayType;
                sorttempHourlyPivotList[0].RowDay = date.Value.ToString("ddd");
                for (int i = 1; i < sorttempHourlyPivotList.Count; i++)
                {
                    if (sorttempHourlyPivotList[i].RowDisplayType == rowtype && (date == sorttempHourlyPivotList[i].Date || sorttempHourlyPivotList[i].Date == null))
                    {
                        sorttempHourlyPivotList[i].RowDisplayType = string.Empty;
                    }
                    else
                    {
                        rowtype = sorttempHourlyPivotList[i].RowDisplayType;
                    }
                    if (sorttempHourlyPivotList[i].Date == date)
                    {
                        sorttempHourlyPivotList[i].RowDay = date.Value.ToString("ddd");
                        sorttempHourlyPivotList[i].DateDisplay = null;
                    }
                    else
                    {
                        date = sorttempHourlyPivotList[i].Date;
                        sorttempHourlyPivotList[i].RowDay = date.Value.ToString("ddd");
                    }
                }
            }
            HourlyPivotList = sorttempHourlyPivotList;*/
        }
        /// <summary>
        /// Creates the path plot model upper.
        /// </summary>
        /// <param name="pnlHash">The PNL hash.</param>
        /// <returns></returns>
        private PlotModel CreatePathPlotModelUpper(Dictionary<string, double> pnlHash)
        {
            mBidId = 0;
            string title = ((DateTime)SelectedHourlyPivot.Date).ToShortDateString();
            var plotModel1 = new PlotModel { Title = title };
            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Key = "Y1Axis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 1
            });
            plotModel1.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Right,
                Key = "Y2Axis",
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 1
            });
            // X axis
            var dataItemValues = new Collection<Item>();
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 90,
                StringFormat = "0",
                MajorStep = 1,
                IsPanEnabled = true,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                TickStyle = TickStyle.Outside,
                FontSize = 10,
                IsTickCentered = true,
                Key = "XAxisCategory",
                GapWidth = 0.05,
                AxisTitleDistance = 30
            };

            plotModel1.Axes.Add(categoryAxis);
            // plotModel1.PlotMargins = new OxyThickness(20, 0, 20, 70);
            var colSeries1 = new BarSeries();
            string colTitle = "DART";
            if (DARTChecked == true)
            {
                colTitle = "DART";
            }
            else if (CostPriceChecked == true)
            {
                colTitle = "COST";
            }
            else if (DAPriceChecked == true)
            {
                colTitle = "DA";
            }

            colSeries1 = new BarSeries()
            {
                Title = colTitle,
                YAxisKey = "XAxisCategory",
                XAxisKey = "Y2Axis",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,
            };
            List<string> pnlKeys = pnlHash.Keys.ToList<string>();
            Dictionary<double, List<string>> sortPnlHash = new Dictionary<double, List<string>>();
            foreach (string pnlKey in pnlKeys)
            {
                double sortPnl = pnlHash[pnlKey];
                List<string> sortList = new List<string>();
                if (sortPnlHash.ContainsKey(sortPnl))
                {
                    sortList = sortPnlHash[sortPnl];
                    sortPnlHash.Remove(sortPnl);
                }

                sortList.Add(pnlKey);
                sortPnlHash.Add(sortPnl, sortList);
            }
            List<double> sortPnlKeys = sortPnlHash.Keys.ToList<double>();
            sortPnlKeys.Sort();
            foreach (double pnlKey in sortPnlKeys)
            {
                List<string> nameList = sortPnlHash[pnlKey];
                //  categoryAxis.Labels.Add("");

                foreach (string name in nameList)
                {
                    //  categoryAxis.ca
                    categoryAxis.Labels.Add(name);

                    var colItem1 = new BarItem(Math.Round(pnlHash[name], 0), mBidId);

                    colSeries1.Items.Add(colItem1);
                    mBidId++;
                }
            }


            plotModel1.Series.Add(colSeries1);
            int i = 0;

            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,

            };

            plotModel1.Legends.Add(l);

            return plotModel1;
        }
        /// <summary>
        /// Creates the plot model upper.
        /// </summary>
        /// <param name="nodeList">The node list.</param>
        /// <returns></returns>
        private PlotModel CreatePlotModelUpper(List<Node> nodeList)
        {
            string title = "Portfolio";
            var plotModel1 = new PlotModel { Title = title };
            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Key = "Y1Axis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 1
            });
            plotModel1.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Right,
                Key = "Y2Axis",
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 1
            });
            // X axis
            var dataItemValues = new Collection<Item>(); // use with non DateTime x axis
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 90,
                StringFormat = "0",
                MajorStep = 1,
                IsPanEnabled = true,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                TickStyle = TickStyle.Outside,
                FontSize = 10,
                IsTickCentered = true,
                Key = "XAxisCategory",
                GapWidth = 0.05,
                AxisTitleDistance = 30
            };
            if (nodeList[nodeList.Count - 1].TimePriceList.Count > 96 && nodeList[nodeList.Count - 1].TimePriceList.Count < 720)
            {
                categoryAxis.MajorStep = 8;
            }
            else if (nodeList[nodeList.Count - 1].TimePriceList.Count > 720)
            {
                categoryAxis.MajorStep = nodeList[nodeList.Count - 1].TimePriceList.Count / 48;
            }
            plotModel1.Axes.Add(categoryAxis);
            plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 70);
            var colSeries1 = new BarSeries();
            string colTitle = nodeList[nodeList.Count - 1].NodeName;
            colSeries1 = new BarSeries()
            {
                Title = colTitle,
                YAxisKey = "Y2Axis",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,
            };
            for (int i = 0; i < nodeList[nodeList.Count - 1].TimePriceList.Count; i++)
            {
                categoryAxis.Labels.Add(nodeList[nodeList.Count - 1].TimePriceList[i].MarketTime.ToString("M/d/yy"));
                var colItem1 = new BarItem(nodeList[nodeList.Count - 1].TimePriceList[i].Price, i);
                colSeries1.Items.Add(colItem1);
            }
            plotModel1.Series.Add(colSeries1);
            for (int j = 0; j < nodeList.Count - 1; j++)
            {
                var lineSeries1 = new LineSeries();
                dataItemValues = new Collection<Item>();
                for (int i = 0; i < nodeList[j].TimePriceList.Count; i++)
                {
                    dataItemValues.Add(new Item() { X = i, Y = nodeList[j].TimePriceList[i].Price });
                }
                lineSeries1 = new LineSeries()
                {
                    CanTrackerInterpolatePoints = false,
                    DataFieldX = "X",
                    DataFieldY = "Y",
                    ItemsSource = dataItemValues,
                    TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0}",
                    XAxisKey = "XAxisCategory",
                    MarkerType = MarkerType.Diamond,
                    MarkerSize = 2,
                    MarkerStrokeThickness = 0
                };
                if (j == 0)
                {
                    lineSeries1.Title = "DA";
                }
                else if (j == 1)
                {
                    lineSeries1.Title = "RT";
                }
                plotModel1.Series.Add(lineSeries1);
            }
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
            };
            plotModel1.Legends.Add(l);

            if (nodeList[nodeList.Count - 1].TimePriceList.Count > 0)
            {
                double columnValuesMax = nodeList[nodeList.Count - 1].TimePriceList.Select(d => double.IsNaN(d.Price) ? double.MinValue : d.Price).Max();
                double columnValuesMin = nodeList[nodeList.Count - 1].TimePriceList.Select(d => double.IsNaN(d.Price) ? double.MaxValue : d.Price).Min();
                double columnAbsMaxMin = Math.Max(Math.Abs(columnValuesMax), Math.Abs(columnValuesMin));
                double seriesSrcValuesMax = nodeList[0].TimePriceList.Select(d => double.IsNaN(d.Price) ? double.MinValue : d.Price).Max();
                double seriesSinkValuesMax = nodeList.Count < 2 ? double.MinValue : nodeList[1].TimePriceList.Select(d => double.IsNaN(d.Price) ? double.MinValue : d.Price).Max();
                double seriesValuesMax = Math.Max(seriesSrcValuesMax, seriesSinkValuesMax);
                double seriesSrcValuesMin = nodeList[0].TimePriceList.Select(d => double.IsNaN(d.Price) ? double.MaxValue : d.Price).Min();
                double seriesSinkValuesMin = nodeList.Count < 2 ? double.MaxValue : nodeList[1].TimePriceList.Select(d => double.IsNaN(d.Price) ? double.MaxValue : d.Price).Min();
                double seriesValuesMin = Math.Min(seriesSrcValuesMin, seriesSinkValuesMin);
                double seriesAbsMaxMin = Math.Max(Math.Abs(seriesValuesMax), Math.Abs(seriesValuesMin));
                double columnMinRatio = (columnValuesMax < 0 || columnValuesMin > 0) ? 0 : Math.Min(Math.Abs(columnValuesMax / columnValuesMin), Math.Abs(columnValuesMin / columnValuesMax));
                double seriesMinRatio = (seriesValuesMax < 0 || seriesValuesMin > 0) ? 0 : Math.Min(Math.Abs(seriesValuesMax / seriesValuesMin), Math.Abs(seriesValuesMin / seriesValuesMax));
                double negativeScaleFactor = Math.Max(Math.Max(columnMinRatio, seriesMinRatio), 0.5);
                double lineAxisMin = seriesValuesMin < 0 ? (0 - Math.Max(Math.Abs(seriesValuesMin), Math.Abs(seriesValuesMax) * negativeScaleFactor)) : (0 - Math.Abs(seriesValuesMax) * negativeScaleFactor);
                double lineAxisMax = seriesValuesMax > 0 ? Math.Max(seriesValuesMax, Math.Abs(seriesValuesMin) * negativeScaleFactor) : Math.Abs(seriesValuesMin) * negativeScaleFactor;
                double PaddingFactorYAxis = 1.05;
                plotModel1.Axes[0].Minimum = lineAxisMin * PaddingFactorYAxis;
                plotModel1.Axes[0].Maximum = lineAxisMax * PaddingFactorYAxis;
                plotModel1.Axes[1].Minimum = (0 - Math.Max(Math.Abs(columnValuesMin), Math.Abs(columnValuesMax) * Math.Abs(lineAxisMin) / lineAxisMax)) * PaddingFactorYAxis;
                plotModel1.Axes[1].Maximum = Math.Max(Math.Abs(columnValuesMax), Math.Abs(columnValuesMin) * Math.Abs(lineAxisMax) / Math.Abs(lineAxisMin)) * PaddingFactorYAxis;
            }
            return plotModel1;
        }
        /// <summary>
        /// Creates the plot model lower.
        /// </summary>
        /// <param name="nodeList">The node list.</param>
        /// <returns></returns>
        private PlotModel CreatePlotModelLower(List<Node> nodeList)
        {
            var plotModel1 = new PlotModel();
            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Key = "Y1AxisB",
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.05,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 0.995,
                TextColor = OxyColors.Transparent
            });
            // this Y2 axis (which is hidden, transparent) is only included so that the top and bottom charts line up nicely
            plotModel1.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Right,
                Key = "Y2AxisB",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(40, c),
                MinorGridlineColor = OxyColor.FromAColor(20, c),
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.05,
                MinimumPadding = 0,
                StartPosition = 0,
                EndPosition = 0.995,
                IntervalLength = 40
            });
            // X axis
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 0,
                StringFormat = "0",
                MajorStep = 1,
                IsPanEnabled = true,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                TickStyle = TickStyle.None,
                FontSize = 10,
                IsTickCentered = true,
                Key = "XAxisBCategory",
                GapWidth = 0.05,
                AxisTitleDistance = 2,
                AxisTickToLabelDistance = 0,
                TextColor = OxyColors.Transparent
            };
            if (nodeList[nodeList.Count - 1].TimePriceList.Count > 96 && nodeList[nodeList.Count - 1].TimePriceList.Count < 720)
            {
                categoryAxis.MajorStep = 8;
            }
            else if (nodeList[nodeList.Count - 1].TimePriceList.Count > 720)
            {
                categoryAxis.MajorStep = nodeList[nodeList.Count - 1].TimePriceList.Count / 48;
            }
            plotModel1.Axes.Add(categoryAxis);
            plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 2);
            for (int i = 0; i < nodeList[nodeList.Count - 1].TimePriceList.Count; i++)
            {
                categoryAxis.Labels.Add(nodeList[nodeList.Count - 1].TimePriceList[i].MarketTime.ToString("M/d/yy 'HE'H"));
            }
            var areaSeries1 = new AreaSeries()
            {
                Fill = OxyColors.LightBlue,
                DataFieldX2 = "Time",
                DataFieldY2 = "Minimum",
                Color = OxyColors.Black,
                StrokeThickness = 1,
                MarkerFill = OxyColors.Transparent,
                DataFieldX = "Time",
                DataFieldY = "Maximum",
                LineStyle = LineStyle.Solid,
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0}",
                XAxisKey = "XAxisBCategory",
                YAxisKey = "Y2AxisB"
            };
            var data = new Collection<DateValue>();
            double accumulator = 0;
            for (int i = 0; i < nodeList[nodeList.Count - 1].TimePriceList.Count; i++)
            {
                if (!nodeList[nodeList.Count - 1].TimePriceList[i].Price.Equals(double.NaN))
                {
                    accumulator += nodeList[nodeList.Count - 1].TimePriceList[i].Price;
                }
                areaSeries1.Points.Add(new DataPoint(i, accumulator));
                areaSeries1.Points2.Add(new DataPoint(i, 0));
            }
            areaSeries1.Title = nodeList[nodeList.Count - 1].NodeName;
            plotModel1.Series.Add(areaSeries1);
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
            };

            plotModel1.Legends.Add(l);

            plotModel1.IsLegendVisible = false;
            return plotModel1;
        }
        /// <summary>
        /// Creates the date time axis.
        /// </summary>
        /// <param name="dtAxis">The dt axis.</param>
        /// <param name="columnWidthFromDateTime">The column width from date time.</param>
        private void CreateDateTimeAxis(out DateTimeAxis dtAxis, out double columnWidthFromDateTime)
        {
            dtAxis = new DateTimeAxis();
            columnWidthFromDateTime = 0;
            var c = OxyColors.DarkBlue;
            dtAxis = new DateTimeAxis() //StartDate.AddDays(-0.5), EndDate.AddDays(0.5), null, null, DateTimeIntervalType.Days
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 90,
                StringFormat = "M/d/yy",
                IntervalType = DateTimeIntervalType.Days,
                MajorStep = 1,
                IsZoomEnabled = true,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0.005,
                EndPosition = 0.995,
                TickStyle = TickStyle.Outside,
                AxisTickToLabelDistance = 2,
                FontSize = 10
            };
            columnWidthFromDateTime = (DateTimeAxis.ToDouble(DateTime.Now.AddDays(1).Date) - DateTimeAxis.ToDouble(DateTime.Now.Date)) * 0.3;

        }
        /// <summary>
        /// Clears this instance.
        /// </summary>
        private void Clear()
        {
            PathList = null;
            TransactionList = null;
        }
        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="bid">The bid.</param>
        /// <returns>
        ///   <c>true</c> if the specified bid is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValid(FTRBid bid)
        {
            if (JanChecked && (bid.PeriodName.ToUpper().Contains("JAN") || bid.PeriodName.ToUpper().Contains("WIN") || bid.PeriodName.ToUpper().Contains("Q3")))
            {
                return true;
            }
            if (FebChecked && (bid.PeriodName.ToUpper().Contains("FEB") || bid.PeriodName.ToUpper().Contains("WIN") || bid.PeriodName.ToUpper().Contains("Q3")))
            {
                return true;
            }
            if (MarChecked && (bid.PeriodName.ToUpper().Contains("MAR") || bid.PeriodName.ToUpper().Contains("SPR") || bid.PeriodName.ToUpper().Contains("Q4")))
            {
                return true;
            }
            if (AprChecked && (bid.PeriodName.ToUpper().Contains("APR") || bid.PeriodName.ToUpper().Contains("SPR") || bid.PeriodName.ToUpper().Contains("Q4")))
            {
                return true;
            }
            if (MayChecked && (bid.PeriodName.ToUpper().Contains("MAY") || bid.PeriodName.ToUpper().Contains("SPR") || bid.PeriodName.ToUpper().Contains("Q4")))
            {
                return true;
            }
            if (JunChecked && (bid.PeriodName.ToUpper().Contains("JUN") || bid.PeriodName.ToUpper().Contains("SUM") || bid.PeriodName.ToUpper().Contains("Q1")))
            {
                return true;
            }
            if (JulChecked && (bid.PeriodName.ToUpper().Contains("JUL") || bid.PeriodName.ToUpper().Contains("SUM") || bid.PeriodName.ToUpper().Contains("Q1")))
            {
                return true;
            }
            if (AugChecked && (bid.PeriodName.ToUpper().Contains("AUG") || bid.PeriodName.ToUpper().Contains("SUM") || bid.PeriodName.ToUpper().Contains("Q1")))
            {
                return true;
            }
            if (SepChecked && (bid.PeriodName.ToUpper().Contains("SEP") || bid.PeriodName.ToUpper().Contains("FAL") || bid.PeriodName.ToUpper().Contains("Q2")))
            {
                return true;
            }
            if (OctChecked && (bid.PeriodName.ToUpper().Contains("OCT") || bid.PeriodName.ToUpper().Contains("FAL") || bid.PeriodName.ToUpper().Contains("Q2")))
            {
                return true;
            }
            if (NovChecked && (bid.PeriodName.ToUpper().Contains("NOV") || bid.PeriodName.ToUpper().Contains("FAL") || bid.PeriodName.ToUpper().Contains("Q2")))
            {
                return true;
            }
            if (DecChecked && (bid.PeriodName.ToUpper().Contains("DEC") || bid.PeriodName.ToUpper().Contains("WIN") || bid.PeriodName.ToUpper().Contains("Q3")))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Sets the transaction.
        /// </summary>
        private void SetTransaction()
        {
            TransactionList = null;
            string auctionName = string.Empty;
            if (AuctionSelectedItem.ToLower().Contains("round"))
            {
                auctionName = AuctionSelectedItem.Substring(0, 20);
            }
            else
            {
                auctionName = AuctionSelectedItem;
            }
            TransactionList = DBAccess.GetFtrTransactions(auctionName, GetMarketKey());
        }
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <returns></returns>
        private int GetMarketKey()
        {
            int marketKey = 9;
            if (MarketComboSelectedValue == "ERCOT")
            {
                marketKey = 9;
            }
            return marketKey;
        }
        /// <summary>
        /// Sets the auction list.
        /// </summary>
        private void SetAuctionList()
        {
            AuctionPortfolioComboList = _dataService.GetAuctionList(GetMarketKey(), false);
        }
        /// <summary>
        /// Sets the portfolio list.
        /// </summary>
        private void SetPortfolioList()
        {
            //PortfolioComboList = null;

            ////PortfolioComboList = _dataService.GetCRRPortfolioList(AuctionSelectedItem, GetMarketKey());
            //PortfolioComboList = DBAccess.GetCRRPortfolio(StartDate, mUser, "CRR", MarketComboSelectedValue, AuctionSelectedItem);

            try
            {
                DateTime sendDate = DateTime.Parse(StartDate.ToShortDateString());
                string product = "CRR";
                List<Portfolio> portfolioList = null;
                portfolioList = DBAccess.GetFTRPortfolio(sendDate, mUser, product, MarketComboSelectedValue, AuctionSelectedItem);
                PortfolioComboList = null;
                if (portfolioList.Count > 0)
                {
                    if (!((mUser == "sangramp") || (mUser == "anitad" || mUser == "abhishekh")))
                    {
                        portfolioList.RemoveAll(x => x.Name == "VAYU_CRR_STRAT");
                        portfolioList.RemoveAll(x => x.Name == "RISK_SIGMA_CRR");
                    }
                    PortfolioComboList = portfolioList;
                }
                else
                {
                    Portfolio objPortfolio = new Portfolio();
                    objPortfolio.Name = "File Not Present";
                    portfolioList.Add(objPortfolio);
                    PortfolioComboSelectedItem = objPortfolio;
                    PortfolioComboList = portfolioList;
                }
            }
            catch (Exception)
            {
            }
        }

        private void SetNewAuctionPortfolioList()
        {
            AuctionPortfolioComboList = _dataService.GetNewAuctionList(GetMarketKey(), true);

        }
        private void SetExternalPortfolioList()
        {
            PortfolioComboList = null;
            PortfolioComboList = DBAccess.GetExterFTRPortfolio(StartDate, mUser, "CRR", MarketComboSelectedValue, AuctionSelectedItem);
        }

        /// <summary>
        /// Shows the error MSG.
        /// </summary>
        /// <param name="ErrorResult">The error result.</param>
        private void ShowErrorMsg(string ErrorResult)
        {
            ErrorDialog d = new ErrorDialog();
            //d.LongTextMessage = ErrorResult;
            //d.ShowDialog();
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Fetches all source sink data.
        /// </summary>
        /// <param name="refreshData">if set to <c>true</c> [refresh data].</param>

        /// <summary>
        /// Moves the CRR trading.
        /// </summary>
        public void RetrieveFetchDataAndUpdateChartCommand()
        {
            mMonthlyDartHashg = new Dictionary<DateTime, PNL>();
            mdailyDARTHAshg = new Dictionary<DateTime, PNL>();
            MonthlyPivotList = null;
            PathPlotModelUpper = null;
            if (MustTakeChecked == true)
            {
                AsBidChecked = false;
                FetchAllPortfolioData(true, true);
            }
            else if (AsBidChecked == true)
            {
                MustTakeChecked = false;
                FetchAllPortfolioData(true, false);
            }
            if (!MustTakeChecked)
            {
                FetchAllPortfolioDataForSummary(true, true);
            }
            else if (!AsBidChecked)
            {
                FetchAllPortfolioDataForSummary(true, false);
            }
        }


        /// <summary>
        /// Fetches all portfolio data.
        /// </summary>
        /// <param name="refreshData">if set to <c>true</c> [refresh data].</param>
        /// <param name="isMustTake">if set to <c>true</c> [is must take].</param>
        public void FetchAllPortfolioData(bool refreshData, bool isMustTake)
        {
            Dictionary<string, double> MonthlyClearedMwh = new Dictionary<string, double>();
            double cleared_totalMw_all = 0;
            PathPlotModelUpper = null;
            PlotModelUpper = null;
            PlotModelLower = null;
            Dictionary<DateTime, Dictionary<string, double>> pathPnlDict = new Dictionary<DateTime, Dictionary<string, double>>();
            int marketKey = 9;
            if (MarketComboSelectedValue == "ERCOT")
            {
                marketKey = 9;
            }
            TotalCredit = null;
            double totalMw = 0;
            double totalDa = 0;
            double totalMw_all = 0;
            if (PathList == null)
            {
                return;
            }
            List<HourlyPivotData> monthlyPivotDataList = new List<HourlyPivotData>();
            DateTime startDate = DateTime.Parse(StartDate.Month + "/1/" + StartDate.Year);
            Dictionary<DateTime, Dictionary<int, PNL>> monthHash = new Dictionary<DateTime, Dictionary<int, PNL>>();
            DateTime nEndDate = EndDate.AddDays(1);
            Dictionary<DateTime, string> dictPeakYn = _dataService.GetPeakYN(startDate, nEndDate);
            // Dictionary<DateTime, string> dictPeakYn = _dataService.GetPeakYN(startDate, startDate.AddMonths(1).AddDays(1));
            int count = 0;
#if OldVersion
#else
            Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> dic = _dataService.GetCost(marketKey, StartDate, EndDate, true, PathList);
            //Dictionary<DateTime, Dictionary<string, Cost>> dicOptionCost = _dataService.GetOptionPrices(marketKey, startDate, EndDate, PathList, AsBidChecked);
            Dictionary<DateTime, Dictionary<string, Cost>> dicOptionCost = _dataService.GetOptionPrices(marketKey, startDate, EndDate, PathList, AsBidChecked, AuctionSelectedItem, PortfolioComboSelectedItem.ID);
#endif
            Dictionary<string, double> dic_dbclearedmwh = _dataService.GetDBclrmwh(marketKey, AuctionSelectedItem, PortfolioComboSelectedItem.Name);
            tempCostHash = null; tempOptionCostHash = null;
            tempCostHash = dic;
            tempOptionCostHash = dicOptionCost;
            string[] srcsink = new string[3];
            string combpath = null;
            //string dickey = null;
            double finalcost = 0;
            foreach (FTRBid path in PathList)
            {
                double mw_all = 0;
                string tradeType = path.TradeType;
                if (tradeType.ToUpper() == "BUY")
                {
                    if (path.MW1 != null)
                        mw_all += (double)path.MW1;
                    if (path.MW2 != null)
                        mw_all += (double)path.MW2;
                    if (path.MW3 != null)
                        mw_all += (double)path.MW3;
                    if (path.MW4 != null)
                        mw_all += (double)path.MW4;
                    if (path.MW5 != null)
                        mw_all += (double)path.MW5;
                    if (path.MW6 != null)
                        mw_all += (double)path.MW6;
                }
                else
                {
                    if (path.MW1 != null)
                        mw_all += (double)path.MW1;
                    if (path.MW2 != null)
                        mw_all += (double)path.MW2;
                    if (path.MW3 != null)
                        mw_all += (double)path.MW3;
                    if (path.MW4 != null)
                        mw_all += (double)path.MW4;
                    if (path.MW5 != null)
                        mw_all += (double)path.MW5;
                    if (path.MW6 != null)
                        mw_all += (double)path.MW6;

                    mw_all = mw_all * -1;
                }
                totalMw_all += mw_all * path.PeriodHours; ;
                Dictionary<string, Dictionary<DateTime, Cost>> CRRCostHash = null;
                string pathkey = path.SourceKey + "?" + path.SinkKey;

                if (path.PeriodName.ToUpper() == "Q1")
                    CRRCostHash = tempCostHash["Q1"];
                else if (path.PeriodName.ToUpper() == "Q2")
                    CRRCostHash = tempCostHash["Q2"];
                else if (path.PeriodName.ToUpper() == "Q3")
                    CRRCostHash = tempCostHash["Q3"];
                else if (path.PeriodName.ToUpper() == "Q4")
                    CRRCostHash = tempCostHash["Q4"];
                else
                    if (tempCostHash.ContainsKey("Monthly"))
                    CRRCostHash = tempCostHash["Monthly"];
                if (!IsValid(path))
                {
                    // continue;
                }
                PricingNode sourceNode = DBAccess.GetNodeFromName(path.Source, marketKey);
                PricingNode sinkNode = DBAccess.GetNodeFromName(path.Sink, marketKey);
                string pathkeyext = sourceNode.ExternalNodeId + "?" + sinkNode.ExternalNodeId;
                if (sourceNode == null || sinkNode == null)
                {
                    continue;
                }
                int sourceNodeKey = sourceNode.NodeKey;
                int sinkNodeKey = sinkNode.NodeKey;

#if OldVersion
                            Dictionary<DateTime, Cost> costHash;
                            if (MustTakeChecked)
                                costHash = mDataService.GetCost(marketKey, StartDate, EndDate, sourceNodeKey, sinkNodeKey, true);
                            else
                                costHash = mDataService.GetCost(marketKey, StartDate, EndDate, sourceNodeKey, sinkNodeKey, false);
#else
                string strKey = sourceNodeKey.ToString() + sinkNodeKey.ToString();
                Dictionary<DateTime, Cost> costHash = null;
                if (!CRRCostHash.ContainsKey(strKey))
                {
                    //costHash = mDataService.GetCost(marketKey, StartDate, EndDate, sourceNodeKey, sinkNodeKey, false);

                    continue;
                }
                else
                    costHash = CRRCostHash[strKey];
#endif
                if (costHash == null)
                {
                    continue;
                }
                SourceSink sourceSink = new SourceSink();
                double mw = 0;
                double cleared_mw_all = 0;
                if (MustTakeChecked == true)
                {
                    sourceSink.ClassType = path.ClassType;
                    sourceSink.HedgeType = path.HedgeType;
                    if (marketKey == 9)
                    {
                        if (path.MW6 != null)
                        {
                            mw += (double)path.MW6;
                        }
                        if (path.MW5 != null)
                        {
                            mw += (double)path.MW5;
                        }
                        if (path.MW4 != null)
                        {
                            mw += (double)path.MW4;
                        }
                        if (path.MW3 != null)
                        {
                            mw += (double)path.MW3;
                        }
                        if (path.MW2 != null)
                        {
                            mw += (double)path.MW2;
                        }
                        if (path.MW1 != null)
                        {
                            mw += (double)path.MW1;
                        }
                    }
                    if (path.TradeType.ToUpper() == "SELL")
                        mw = mw * -1;
                    sourceSink.MW = mw;
                    totalMw += mw;
                    sourceSink.PeriodKey = path.PeriodKey;
                    sourceSink.Source = path.Source;
                    sourceSink.Sink = path.Sink;
                    //
                    double CRRCost = 0;
                    List<DateTime> marketDateList = costHash.Keys.ToList<DateTime>();
                    Cost cost = new Cost();
                    Dictionary<string, Cost> optioncost = new Dictionary<string, Cost>(); Cost costHelper = new Cost();
                    //Adding dates to dictonary
                    foreach (DateTime marketDate in marketDateList)
                    {
                        double monthlyPnl = 0.0;
                        double monthlyCost = 0.0;
                        double monthlyDA = 0.0;
                        double monthlyRT = 0.0;

                        if (!MonthlyClearedMwh.ContainsKey(marketDate.ToShortDateString()))
                        {
                            MonthlyClearedMwh.Add(marketDate.ToShortDateString(), 0);
                        }

                        if (path.HedgeType.ToUpper() == "OPT")
                        {
                            if (tempOptionCostHash.ContainsKey(marketDate))
                                optioncost = tempOptionCostHash[marketDate];
                            if (optioncost.ContainsKey(pathkey))
                                costHelper = optioncost[pathkey];
                        }
                        else
                        {
                            //   if (tempCostHash.ContainsKey(marketDate))
                            //    optioncost = tempCostHash[marketDate];
                            //if (optioncost.ContainsKey(pathkey))
                            //   costHelper = optioncost[pathkey];
                        }
                        if (path.HedgeType.ToUpper() == "OBLIGATION" || path.HedgeType.ToUpper() == "OBL")
                        {
                            cost = costHash[marketDate];
                            if (MarketComboSelectedValue == "ERCOT")
                            {
                                if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD" || path.ClassType.ToUpper() == "PEAKWE")
                                {
                                    CRRCost = cost.Peak;
                                }
                                else
                                {
                                    CRRCost = cost.OffPeak;
                                }
                            }
                        }
                        else
                        {
                            if (MarketComboSelectedValue == "ERCOT")
                            {
                                //option
                                cost = costHelper;
                                if (dicOptionCost.ContainsKey(marketDate))
                                {
                                    Dictionary<string, Cost> tempOptionHash = dicOptionCost[marketDate];
                                    if (tempOptionHash.ContainsKey(pathkey))
                                    {
                                        Cost objcost = tempOptionHash[pathkey];
                                        if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                                        {
                                            if (objcost.Peak != 0)
                                                CRRCost = objcost.Peak;//cost.Peak;
                                            else
                                                continue;
                                        }
                                        else if (path.ClassType.ToUpper() == "PEAKWE")//Work pending
                                        {
                                            if (objcost.PeakWE != 0)
                                                CRRCost = objcost.PeakWE;
                                            else
                                                continue;
                                        }
                                        else
                                        {
                                            if (objcost.OffPeak != 0)
                                                CRRCost = objcost.OffPeak;
                                            else
                                                continue;
                                        }
                                    }
                                    else
                                        continue;
                                }
                                else
                                    continue;
                            }
                        }
                        if (double.IsNaN(CRRCost) || double.IsInfinity(CRRCost))
                        {
                            continue;
                        }
                        Dictionary<int, PNL> dayHash = new Dictionary<int, PNL>();
                        if (cost.DACongestionList == null)
                            continue;
                        if (monthHash.ContainsKey(marketDate))
                        {
                            dayHash = monthHash[marketDate];
                            monthHash.Remove(marketDate);
                        }
                        cost.DACongestionList.RemoveAll(a => a == null);
                        DateTime? maxDate = null;
                        if (cost.DACongestionList[0].MarketDateTime.Month == 10)
                        {
                            maxDate = cost.DACongestionList.Max(x => x.MarketDateTime.Date);
                        }
                        foreach (DACongestion daCongestion in cost.DACongestionList)
                        {
                            Dictionary<DateTime, Cost> rtcosthash = CRRCostHash[strKey];
                            DateTime dt = Convert.ToDateTime(daCongestion.MarketDateTime.Month + "/1/" + daCongestion.MarketDateTime.Year);
                            // DACongestion rtCongestion = rtcosthash[dt].RTCongestionList.First(a => a.MarketDateTime == daCongestion.MarketDateTime);

                            DACongestion rtCongestion = new DACongestion();
                            if (rtcosthash[dt].RTCongestionList.Exists(a => a.MarketDateTime == daCongestion.MarketDateTime))
                            {
                                rtCongestion = rtcosthash[dt].RTCongestionList.First(a => a.MarketDateTime == daCongestion.MarketDateTime);
                            }

                            double daPrice = 0;
                            double CRRHours = 0;
                            double rtPrice = 0;


                            if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                            {
                                if (marketKey == 9)
                                {
                                    DateTime curdate = Convert.ToDateTime(daCongestion.MarketDateTime.Year + "/" + daCongestion.MarketDateTime.Month + "/" + daCongestion.MarketDateTime.Day);
                                    DayOfWeek satsunday11 = curdate.DayOfWeek;
                                    string Weekendays = satsunday11.ToString();
                                    if (Weekendays.ToLower() == Convert.ToString("saturday") || Weekendays.ToLower() == Convert.ToString("sunday"))
                                    {
                                        continue;
                                    }
                                }
                                daPrice = daCongestion.Peak;
                                CRRHours = daCongestion.PeakHours;
                                rtPrice = rtCongestion.Peak;
                            }
                            else if (path.ClassType.ToUpper() == "PEAKWE")
                            {
                                if (marketKey == 9)
                                {
                                    DateTime curdate = Convert.ToDateTime(daCongestion.MarketDateTime.Year + "/" + daCongestion.MarketDateTime.Month + "/" + daCongestion.MarketDateTime.Day);
                                    DayOfWeek satsunday11 = curdate.DayOfWeek;
                                    string Weekendays = satsunday11.ToString();
                                    if ((satsunday11 == DayOfWeek.Saturday) || (satsunday11 == DayOfWeek.Sunday))
                                    {
                                        daPrice = daCongestion.PeakWE;
                                        CRRHours = daCongestion.PeakHours;
                                        rtPrice = rtCongestion.PeakWE;

                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    daPrice = daCongestion.PeakWE;
                                    CRRHours = daCongestion.PeakHours;
                                    rtPrice = rtCongestion.PeakWE;
                                }
                            }
                            else
                            {
                                daPrice = daCongestion.OffPeak;
                                CRRHours = daCongestion.OffPeakHours;
                                rtPrice = rtCongestion.OffPeak;
                            }


                            if (CRRHours != 0)
                            {
                                if (!dayHash.ContainsKey(daCongestion.MarketDateTime.Day))
                                    dayHash.Add(daCongestion.MarketDateTime.Day, new PNL() { DART = 0, DA = 0, Cost = 0, RT = 0 });
                                double dart = 00;
                                if (marketKey == 9 && path.HedgeType == "OPT")
                                {
                                    dart = (daPrice - CRRCost) * mw * CRRHours;
                                }

                                else
                                {

                                    dart = (daPrice - CRRCost) * CRRHours * mw;


                                }

                                //
                                PNL pnl = dayHash[daCongestion.MarketDateTime.Day];
                                if (daCongestion.MarketDateTime.Day == 1)
                                {

                                }
                                pnl.DART += dart;

                                if (marketKey == 9)//both opt and obl
                                {

                                    pnl.DA += daPrice * mw * CRRHours;
                                    monthlyDA += daPrice * mw * CRRHours;

                                    pnl.RT += rtPrice * mw * CRRHours;
                                    monthlyRT += rtPrice * mw * CRRHours;


                                }
                                pnl.Cost += CRRCost * mw * CRRHours;


                                // else
                                //   pnl.Cost = CRRCost * mw * CRRHours;
                                //
                                double tempcost = CRRCost * mw * CRRHours;
                                monthlyPnl += dart;
                                monthlyCost += CRRCost * mw * CRRHours;
                                //monthlyDA += pnl.DA;
                                finalcost = monthlyCost;

                            }

                            if (maxDate.HasValue && maxDate == daCongestion.MarketDateTime.Date)
                            {
                                System.Diagnostics.Debug.WriteLine(maxDate + "," + path.SourceKey + "," + path.SinkKey + "," +
                                    daPrice + "," +
                                   CRRCost + ",");
                            }
                        }
                        monthHash.Add(marketDate, dayHash);
                        if (DARTChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyPnl);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyPnl);
                            }
                        }
                        else if (CostPriceChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyCost);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyCost);
                            }
                        }
                        else if (DAPriceChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyDA);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyDA);
                            }
                        }
                        else if (RTPriceChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyRT);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyRT);
                            }
                        }
                    }
                }
                else
                {
                    double CRRCost = 0;
                    List<DateTime> marketDateList = costHash.Keys.ToList<DateTime>();
                    sourceSink.ClassType = path.ClassType;
                    sourceSink.HedgeType = path.HedgeType;
                    double nodePrice = double.NaN;
                    Dictionary<string, Cost> optioncost = new Dictionary<string, Cost>(); Cost costHelper = new Cost();
                    foreach (DateTime marketDate in marketDateList)
                    {
                        double monthlyPnl = 0.0;
                        double monthlyCost = 0.0;
                        double monthlyDA = 0.0;
                        double monthlyRT = 0.0;
                        mw = 0;
                        mw_all = 0;
                        cleared_mw_all = 0;
                        string[] value_marketdate = marketDate.ToShortDateString().Split('/');
                        DateTime datevalue = new DateTime(Int16.Parse(value_marketdate[2]), Int16.Parse(value_marketdate[0]), 1);
                        string monthval = datevalue.ToString("MMM");

                        //string yearval = value_marketdate[2].Substring(0, 4);
                        string yearval = datevalue.Year.ToString();

                        if (!MonthlyClearedMwh.ContainsKey(marketDate.ToShortDateString()))
                        {
                            MonthlyClearedMwh.Add(marketDate.ToShortDateString(), 0);
                        }
                        if (tempOptionCostHash.ContainsKey(marketDate))
                            optioncost = tempOptionCostHash[marketDate];
                        if (optioncost.ContainsKey(pathkey))
                            costHelper = optioncost[pathkey];
                        if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                        {
                            if (costHash.Count > 0)
                            {
                                if (MarketComboSelectedValue == "ERCOT")
                                {
                                    if (path.HedgeType == "OBL")
                                        nodePrice = costHash[marketDate].Peak;
                                    else
                                        nodePrice = costHelper.Peak;
                                }
                            }
                        }
                        else if (path.ClassType.ToUpper() == "PEAKWE")
                        {
                            if (costHash.Count > 0)
                            {
                                if (MarketComboSelectedValue == "ERCOT")
                                {
                                    if (path.HedgeType == "OBL")
                                        nodePrice = costHash[marketDate].PeakWE;
                                    else
                                        nodePrice = costHelper.PeakWE;
                                }
                            }
                        }
                        else
                        {
                            if (costHash.Count > 0)
                            {
                                if (MarketComboSelectedValue == "ERCOT")
                                {
                                    if (path.HedgeType == "OBL")
                                        nodePrice = costHash[marketDate].OffPeak;
                                    else
                                        nodePrice = costHelper.OffPeak;
                                }
                            }
                        }
                        if (marketKey == 9)
                        {
                            if (tradeType.ToUpper() == "BUY")
                            {
                                if (path.MW6 != null && path.Price6 >= nodePrice)
                                    mw += (double)path.MW6;
                                if (path.MW5 != null && path.Price5 >= nodePrice)
                                    mw += (double)path.MW5;
                                if (path.MW4 != null && path.Price4 >= nodePrice)
                                    mw += (double)path.MW4;
                                if (path.MW3 != null && path.Price3 >= nodePrice)
                                    mw += (double)path.MW3;
                                if (path.MW2 != null && path.Price2 >= nodePrice)
                                    mw += (double)path.MW2;
                                if (path.MW1 != null && path.Price1 >= nodePrice)
                                    mw += (double)path.MW1;
                            }
                            else
                            {
                                if (path.MW6 != null && path.Price6 < nodePrice)
                                    mw += (double)path.MW6;
                                if (path.MW5 != null && path.Price5 < nodePrice)
                                    mw += (double)path.MW5;
                                if (path.MW4 != null && path.Price4 < nodePrice)
                                    mw += (double)path.MW4;
                                if (path.MW3 != null && path.Price3 < nodePrice)
                                    mw += (double)path.MW3;
                                if (path.MW2 != null && path.Price2 < nodePrice)
                                    mw += (double)path.MW2;
                                if (path.MW1 != null && path.Price1 < nodePrice)
                                    mw += (double)path.MW1;

                                mw = mw * -1;
                            }
                            totalMw += mw;
                            double mtply_clrmwh = 0;
                            if (mw > 0)
                            {
                                if ((path.ClassType.ToUpper() == "PEAK") || (path.ClassType.ToUpper() == "ONPEAK"))
                                    mtply_clrmwh += mw * 116;// 16;

                                if (path.ClassType.ToUpper() == "OFF-PEAK")
                                    mtply_clrmwh += mw * 248;// 8;

                                if (path.ClassType.ToUpper() == "PEAKWD")
                                    mtply_clrmwh += mw * 336;// 16 ;
                            }
                            cleared_totalMw_all += mtply_clrmwh;
                            string ky = dic_dbclearedmwh.ElementAt(0).Key.Substring(0, 3).ToUpper();
                            string yr = dic_dbclearedmwh.ElementAt(0).Key.Substring(4, 4);
                            if (monthval.ToUpper() == ky && yr == yearval)
                                MonthlyClearedMwh[marketDate.ToShortDateString()] = dic_dbclearedmwh.ElementAt(0).Value;
                            else
                                MonthlyClearedMwh[marketDate.ToShortDateString()] += Math.Round(mtply_clrmwh, 2);
                        }
                        //
                        if (mw == 0.0)
                            continue;
                        //
                        Cost cost = new Cost();
                        cost = costHash[marketDate];
                        if (path.HedgeType.ToUpper() == "OBLIGATION" || path.HedgeType.ToUpper() == "OBL")
                        {
                            if (MarketComboSelectedValue == "ERCOT")
                            {
                                if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                                {
                                    CRRCost = cost.Peak;
                                }
                                else if (path.ClassType.ToUpper() == "PEAKWE")
                                {
                                    CRRCost = cost.PeakWE;
                                }
                                else
                                {
                                    CRRCost = cost.OffPeak;
                                }
                            }
                        }
                        else
                        {
                            if (MarketComboSelectedValue == "ERCOT")
                            {
                                cost = costHelper;
                                if (dicOptionCost.ContainsKey(marketDate))
                                {
                                    Dictionary<string, Cost> tempOptionHash = dicOptionCost[marketDate];
                                    if (tempOptionHash.ContainsKey(pathkey))
                                    {
                                        Cost objcost = tempOptionHash[pathkey];
                                        if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                                        {
                                            if (objcost.Peak != 0)
                                                CRRCost = objcost.Peak;//cost.Peak;
                                            else
                                                continue;
                                        }
                                        else if (path.ClassType.ToUpper() == "PEAKWE")
                                        {
                                            if (objcost.PeakWE != 0)
                                                CRRCost = objcost.PeakWE; //cost.PeakWE;}
                                            else
                                                continue;
                                        }
                                        else
                                        {
                                            if (objcost.OffPeak != 0)
                                                CRRCost = objcost.OffPeak; //cost.OffPeak;
                                            else
                                                continue;
                                        }
                                    }
                                    else
                                        continue;
                                }

                                else
                                    continue;
                            }
                        }
                        if (double.IsNaN(CRRCost) || double.IsInfinity(CRRCost))
                        {
                            continue;
                        }
                        Dictionary<int, PNL> dayHash = new Dictionary<int, PNL>();
                        if (cost.DACongestionList == null)
                            continue;
                        if (monthHash.ContainsKey(marketDate))
                        {
                            dayHash = monthHash[marketDate];
                            monthHash.Remove(marketDate);
                        }
                        cost.DACongestionList.RemoveAll(a => a == null);
                        DateTime? maxDate = null;
                        if (cost.DACongestionList[0].MarketDateTime.Month == 10)
                        {
                            maxDate = cost.DACongestionList.Max(x => x.MarketDateTime.Date);
                            //System.Diagnostics.Debug.WriteLine(maxDate + "," + path.SourceKey + "," + path.SinkKey + "," + cost.DACongestionList.Count );
                        }
                        //OPT 
                        foreach (DACongestion daCongestion in cost.DACongestionList)
                        {

                            Dictionary<DateTime, Cost> rtcosthash = CRRCostHash[strKey];
                            DateTime dt = Convert.ToDateTime(daCongestion.MarketDateTime.Month + "/1/" + daCongestion.MarketDateTime.Year);

                            //  DACongestion rtCongestion = rtcosthash[dt].RTCongestionList.FirstOrDefault(a => a.MarketDateTime == daCongestion.MarketDateTime);
                            DACongestion rtCongestion = new DACongestion();
                            if (rtcosthash[dt].RTCongestionList.Exists(a => a.MarketDateTime == daCongestion.MarketDateTime))
                            {
                                rtCongestion = rtcosthash[dt].RTCongestionList.First(a => a.MarketDateTime == daCongestion.MarketDateTime);
                            }

                            double daPrice = 0;
                            double CRRHours = 0;
                            double rtPrice = 0;

                            if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD")
                            {
                                if (marketKey == 9)
                                {
                                    DateTime curdate = Convert.ToDateTime(daCongestion.MarketDateTime.Year + "/" + daCongestion.MarketDateTime.Month + "/" + daCongestion.MarketDateTime.Day);
                                    DayOfWeek satsunday11 = curdate.DayOfWeek;
                                    string Weekendays = satsunday11.ToString();
                                    if (Weekendays.ToLower() == Convert.ToString("saturday") || Weekendays.ToLower() == Convert.ToString("sunday"))
                                    {
                                        continue;
                                    }
                                    curdate = curdate.AddHours(8);
                                    if (dictPeakYn.ContainsKey(curdate))
                                    {
                                        string peakoffpeak = dictPeakYn[curdate];
                                        if (peakoffpeak.ToLower() == "w")//holiday or weekend
                                        {
                                            continue;
                                        }

                                    }
                                }
                                daPrice = daCongestion.Peak;
                                CRRHours = daCongestion.PeakHours;
                            }
                            else if (path.ClassType.ToUpper() == "PEAKWE")
                            {
                                if (marketKey == 9)
                                {

                                    if (daCongestion.MarketDateTime.Month == 5)
                                    {
                                    }
                                    DateTime curdate = Convert.ToDateTime(daCongestion.MarketDateTime.Year + "/" + daCongestion.MarketDateTime.Month + "/" + daCongestion.MarketDateTime.Day);
                                    DayOfWeek satsunday11 = curdate.DayOfWeek;
                                    string Weekendays = satsunday11.ToString();
                                    if ((satsunday11 == DayOfWeek.Saturday) || (satsunday11 == DayOfWeek.Sunday))
                                    {
                                        daPrice = daCongestion.PeakWE;
                                        CRRHours = daCongestion.PeakHours;
                                        rtPrice = rtCongestion.PeakWE;
                                    }

                                    curdate = curdate.AddHours(8);
                                    if (dictPeakYn.ContainsKey(curdate))
                                    {
                                        string peakoffpeak = dictPeakYn[curdate];
                                        if (peakoffpeak.ToLower() == "w")//holiday or weekend
                                        {
                                            daPrice = daCongestion.PeakWE;
                                            CRRHours = daCongestion.PeakHours;
                                            rtPrice = rtCongestion.PeakWE;
                                        }

                                    }

                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    daPrice = daCongestion.PeakWE;
                                    CRRHours = daCongestion.PeakHours;
                                    rtPrice = rtCongestion.PeakWE;
                                }
                            }
                            else
                            {
                                daPrice = daCongestion.OffPeak;
                                CRRHours = daCongestion.OffPeakHours;
                                rtPrice = rtCongestion.OffPeak;
                            }
                            if (CRRHours != 0)
                            {
                                if (!dayHash.ContainsKey(daCongestion.MarketDateTime.Day))
                                    dayHash.Add(daCongestion.MarketDateTime.Day, new PNL() { DART = 0, DA = 0, Cost = 0, RT = 0 });
                                double dart = 00;
                                if (marketKey == 9 && path.HedgeType == "OPT")
                                {

                                    dart = (daPrice - CRRCost) * mw * CRRHours;

                                }

                                else
                                {
                                    dart = (daPrice - CRRCost) * CRRHours * mw;


                                }

                                //
                                PNL pnl = dayHash[daCongestion.MarketDateTime.Day];
                                if (daCongestion.MarketDateTime.Day == 1)
                                {

                                }
                                pnl.DART += dart;
                                //pnl.DART = dart;

                                if (marketKey == 9)//both opt and obl
                                {

                                    pnl.DA += daPrice * mw * CRRHours;
                                    monthlyDA += daPrice * mw * CRRHours;

                                    pnl.RT += rtPrice * mw * CRRHours;
                                    monthlyRT += rtPrice * mw * CRRHours;

                                    if (rtPrice != 0)
                                    { }


                                }
                                pnl.Cost += CRRCost * mw * CRRHours;
                                double tempcost = CRRCost * mw * CRRHours;
                                monthlyPnl += dart;
                                monthlyCost += CRRCost * mw * CRRHours;
                                //monthlyDA += pnl.DA;
                                finalcost = monthlyCost;

                            }

                            if (maxDate.HasValue && maxDate == daCongestion.MarketDateTime.Date)
                            {
                                System.Diagnostics.Debug.WriteLine(maxDate + "," + path.SourceKey + "," + path.SinkKey + "," +
                                    daPrice + "," +
                                   CRRCost + ",");
                            }
                        }
                        monthHash.Add(marketDate, dayHash);
                        if (DARTChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyPnl);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyPnl);
                            }
                        }
                        else if (CostPriceChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyCost);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyCost);
                            }
                        }
                        else if (DAPriceChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyDA);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyDA);
                            }
                        }

                        else if (RTPriceChecked == true)
                        {
                            if (!pathPnlDict.ContainsKey(marketDate))
                            {
                                Dictionary<string, double> tempPnlDict = new Dictionary<string, double>();
                                tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyRT);
                                pathPnlDict.Add(marketDate, tempPnlDict);
                            }
                            else
                            {
                                Dictionary<string, double> tempPnlDict = pathPnlDict[marketDate];
                                if (!tempPnlDict.ContainsKey(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType))
                                    tempPnlDict.Add(path.Source + '?' + path.Sink + '?' + path.ClassType + '?' + path.HedgeType, monthlyRT);
                            }
                        }

                    }

                }
            }


            ClearedText = Math.Round(totalMw, 2).ToString();
            SubmittedMwhText = Math.Round(totalMw_all, 2).ToString();
            // clearedmwhText = Math.Round(cleared_totalMw_all, 2).ToString();
            //monthlyClearedmwhText = Math.Round(cleared_totalMw_all, 2).ToString();


            List<DateTime> monthList = monthHash.Keys.ToList<DateTime>();
            Dictionary<DateTime, PNL> monthlyDartHash = new Dictionary<DateTime, PNL>();
            Dictionary<DateTime, PNL> dailyDARTHAsh = new Dictionary<DateTime, PNL>();
            foreach (DateTime monthDate in monthList)
            {
                if (!IsDateValid(monthDate.Month))
                {
                    continue;
                }

                int days = 0;
                double monthlyTotalDart = 0.0; ;
                double monthlyTotalDA = 0.0;
                double monthlyTotalCost = 0.0;
                double monthlyTotalRT = 0.0;
                Dictionary<int, PNL> pnlHash = monthHash[monthDate];
                HourlyPivotData monthlyPivotData = new HourlyPivotData();
                monthlyPivotData.DateDisplay = monthDate;
                monthlyPivotData.Date = monthDate;
                monthlyPivotData.RowDay = "";
                monthlyPivotData.RowName = "";
                monthlyPivotData.RowType = "";
                monthlyPivotData.RowDay = "";


                monthlyPivotData.ClearedMWHmonth = MonthlyClearedMwh[monthDate.ToShortDateString()];

                if (DARTChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;

                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE27 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        PNL dailyPnl = new PNL();
                        if (pnlHash.ContainsKey(i))
                        {
                            dailyPnl.DART = pnlHash[i].DART;
                            dailyPnl.DA = pnlHash[i].DA;
                            dailyPnl.Cost = pnlHash[i].Cost;
                            dailyPnl.RT = pnlHash[i].RT;
                            if (!dailyDARTHAsh.ContainsKey(monthDate.AddDays(i)))
                                dailyDARTHAsh.Add(monthDate.AddDays(i), dailyPnl);
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalDart;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalDart / days;
                        // monthlyPivotData.
                    }

                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "DART";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }
                else if (CostPriceChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;

                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE27 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        PNL dailyPnl = new PNL();
                        if (pnlHash.ContainsKey(i))
                        {
                            dailyPnl.DART = pnlHash[i].DART;
                            dailyPnl.DA = pnlHash[i].DA;
                            dailyPnl.Cost = pnlHash[i].Cost;
                            dailyPnl.RT = pnlHash[i].RT;
                            if (!dailyDARTHAsh.ContainsKey(monthDate.AddDays(i)))
                                dailyDARTHAsh.Add(monthDate.AddDays(i), dailyPnl);
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalCost;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalCost / days;
                        // monthlyPivotData.
                    }

                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "COST";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }
                else if (DAPriceChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;

                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE27 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        PNL dailyPnl = new PNL();
                        if (pnlHash.ContainsKey(i))
                        {
                            dailyPnl.DART = pnlHash[i].DART;
                            dailyPnl.DA = pnlHash[i].DA;
                            dailyPnl.Cost = pnlHash[i].Cost;
                            dailyPnl.RT = pnlHash[i].RT;
                            if (!dailyDARTHAsh.ContainsKey(monthDate.AddDays(i)))
                                dailyDARTHAsh.Add(monthDate.AddDays(i), dailyPnl);
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalDA;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalDA / days;
                        // monthlyPivotData.
                    }

                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "DA";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }

                else if (RTPriceChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;

                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE27 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        PNL dailyPnl = new PNL();
                        if (pnlHash.ContainsKey(i))
                        {
                            dailyPnl.DART = pnlHash[i].DART;
                            dailyPnl.DA = pnlHash[i].DA;
                            dailyPnl.Cost = pnlHash[i].Cost;
                            dailyPnl.RT = pnlHash[i].RT;
                            if (!dailyDARTHAsh.ContainsKey(monthDate.AddDays(i)))
                                dailyDARTHAsh.Add(monthDate.AddDays(i), dailyPnl);
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalRT;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalRT / days;
                        // monthlyPivotData.
                    }

                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "RT";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }

            }

            List<int> yearList = new List<int>();

            foreach (var item in monthlyDartHash)
            {
                if (!yearList.Contains(item.Key.Year))
                {
                    yearList.Add(item.Key.Year);
                }
            }

            monthlyPivotDataList.Reverse();
            monthlyPivotDataList = monthlyPivotDataList.OrderByDescending(a => a.Date).ToList();
            //  List<int> monthlyList = new List<int>();
            //  List<int> listremove = Enumerable.Range(1, 12).ToList();

            //  List<DateTime> dateMonthList = new List<DateTime>();

            //foreach (FTRBid itemFTRBid in PathList)
            //{
            //    if (itemFTRBid.PeriodName == "Q1")
            //    {
            //        monthlyList.Add(6);
            //        monthlyList.Add(7);
            //        monthlyList.Add(8);
            //    }
            //    if (itemFTRBid.PeriodName == "Q2")
            //    {
            //        monthlyList.Add(9);
            //        monthlyList.Add(10);
            //        monthlyList.Add(11);
            //    }
            //    if (itemFTRBid.PeriodName == "Q3")
            //    {
            //        monthlyList.Add(12);
            //        monthlyList.Add(1);
            //        monthlyList.Add(2);
            //    }
            //    if (itemFTRBid.PeriodName == "Q4")
            //    {
            //        monthlyList.Add(3);
            //        monthlyList.Add(4);
            //        monthlyList.Add(5);
            //    }
            //}
            //var listfinal = listremove.Except(monthlyList);
            //foreach (var item in listfinal)
            //{
            //    monthlyPivotDataList = monthlyPivotDataList.Where(x => x.Date.Value.Month != item).ToList();

            //    foreach (var years in yearList)
            //    {
            //        DateTime tempDate = DateTime.MinValue;

            //        tempDate = tempDate.AddMonths(item - 1).AddYears(years - 1);

            //        if (monthlyDartHash.ContainsKey(tempDate))
            //        {
            //            monthlyDartHash.Remove(tempDate);
            //        }
            //    }

            //    //monthlyDartHash = monthlyDartHash.Select(x => x.Key.Month != item).ToDictionary();
            //}

            MonthlyPivotList = monthlyPivotDataList;
            MonthlyDartHash = monthlyDartHash;
            mdailyDARTHAshg = dailyDARTHAsh;
            mMonthlyDartHashg = monthlyDartHash;
            PlotAreaGraph();


            //double TotalDA = 0;

            foreach (var item in monthlyDartHash)
            {
                totalDa += item.Value.DA;
            }

            DAText = Math.Round(totalDa, 2).ToString();

            if (SelectedHourlyPivot != null && (SelectedHourlyPivot.Date >= startDate && SelectedHourlyPivot.Date <= EndDate))
            {
                if (pathPnlDict.ContainsKey(Convert.ToDateTime(SelectedHourlyPivot.Date)))
                {


                    Dictionary<string, double> pnlDict = pathPnlDict[Convert.ToDateTime(SelectedHourlyPivot.Date)];
                    PathPlotModelUpper = CreatePathPlotModelUpper(pnlDict);
                }
            }
            if (isMustTake)
            {
                MustTakeMonthlyPivotList = null;
                MustTakeMonthlyPivotList = MonthlyPivotList;
            }
            else
            {
                AsBidMonthlyPivotList = null;
                AsBidMonthlyPivotList = MonthlyPivotList;
            }
            SetSummary();
        }

        private void PlotAreaGraph()
        {
            if (MonthlyChecked)
            {
                PlotModelUpper = CreateMonthlyDARTPlot(mMonthlyDartHashg);
                PlotModelLower = CreateCumiliativeDart(mMonthlyDartHashg);
            }
            else if (DailyChecked)
            {
                PlotModelUpper = CreateMonthlyDARTPlot(mdailyDARTHAshg);
                PlotModelLower = CreateCumiliativeDart(mdailyDARTHAshg);
            }
        }

        /// <summary>
        /// Fetches all portfolio data for summary.
        /// </summary>
        /// <param name="refreshData">if set to <c>true</c> [refresh data].</param>
        /// <param name="isMustTake">if set to <c>true</c> [is must take].</param>
        public void FetchAllPortfolioDataForSummary(bool refreshData, bool isMustTake)
        {
            int marketKey = 9;
            if (MarketComboSelectedValue == "ERCOT")
            {
                marketKey = 9;
            }
            TotalCredit = null;
            double totalMw = 0;
            double totalDa = 0;
            if (PathList == null)
            {
                return;
            }

            List<HourlyPivotData> monthlyPivotDataList = new List<HourlyPivotData>();
            DateTime startDate = DateTime.Parse(StartDate.Month + "/1/" + StartDate.Year);
            Dictionary<DateTime, Dictionary<int, PNL>> monthHash = new Dictionary<DateTime, Dictionary<int, PNL>>();
            int count = 0;
            //List<FTRBid> tempPathList = new List<FTRBid>();

#if OldVersion
#else
            Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> tempPeriodCostHash = tempCostHash;
#endif

            foreach (var item in tempPeriodCostHash)
            {

                Dictionary<string, Dictionary<DateTime, Cost>> dic = item.Value;
                foreach (FTRBid path in PathList)
                {
                    if (!IsValid(path))
                    {
                        //  continue;
                    }

                    PricingNode sourceNode = DBAccess.GetNodeFromName(path.Source, marketKey);
                    PricingNode sinkNode = DBAccess.GetNodeFromName(path.Sink, marketKey);
                    if (sourceNode == null || sinkNode == null)
                    {
                        continue;
                    }
                    int sourceNodeKey = sourceNode.NodeKey;
                    int sinkNodeKey = sinkNode.NodeKey;

#if OldVersion
                    Dictionary<DateTime, Cost> costHash;
                    if (MustTakeChecked)
                        costHash = mDataService.GetCost(marketKey, StartDate, EndDate, sourceNodeKey, sinkNodeKey, true);
                    else
                        costHash = mDataService.GetCost(marketKey, StartDate, EndDate, sourceNodeKey, sinkNodeKey, false);
               
#else
                    string strKey = sourceNodeKey.ToString() + sinkNodeKey.ToString();
                    Dictionary<DateTime, Cost> costHash = null;

                    if (!dic.ContainsKey(strKey))
                    {
                        //costHash = mDataService.GetCost(marketKey, StartDate, EndDate, sourceNodeKey, sinkNodeKey, false);

                        continue;
                    }
                    else
                        costHash = dic[strKey];
#endif

                    if (costHash == null)
                    {
                        continue;
                    }

                    SourceSink sourceSink = new SourceSink();
                    double mw = 0;
                    if (isMustTake == true)
                    {
                        sourceSink.ClassType = path.ClassType;
                        sourceSink.HedgeType = path.HedgeType;
                        if (marketKey == 9)
                        {
                            if (path.MW6 != null)
                            {
                                mw = (double)path.MW6;
                            }
                            else if (path.MW5 != null)
                            {
                                mw = (double)path.MW5;
                            }
                            else if (path.MW4 != null)
                            {
                                mw = (double)path.MW4;
                            }
                            else if (path.MW3 != null)
                            {
                                mw = (double)path.MW3;
                            }
                            else if (path.MW2 != null)
                            {
                                mw = (double)path.MW2;
                            }
                            else if (path.MW1 != null)
                            {
                                mw = (double)path.MW1;
                            }
                        }

                        if (path.TradeType.ToUpper() == "SELL")
                            mw = mw * -1;
                        sourceSink.MW = mw;
                        totalMw += mw;
                        sourceSink.PeriodKey = path.PeriodKey;
                        sourceSink.Source = path.Source;
                        sourceSink.Sink = path.Sink;
                    }
                    else
                    {
                        sourceSink.ClassType = path.ClassType;
                        sourceSink.HedgeType = path.HedgeType;
                        string tradeType = path.TradeType;
                        double nodePrice = double.NaN;

                        if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD" || path.ClassType.ToUpper() == "PEAKWE")
                        {
                            if (costHash.Count > 0)
                            {
                                DateTime latestAuctionDate = costHash.Keys.Max();
                                nodePrice = costHash[latestAuctionDate].Peak / costHash[latestAuctionDate].PeakHours;
                            }
                        }
                        else
                        {
                            if (costHash.Count > 0)
                            {
                                DateTime latestAuctionDate = costHash.Keys.Max();
                                nodePrice = costHash[latestAuctionDate].OffPeak / costHash[latestAuctionDate].OffPeakHours;
                            }
                        }
                        if (marketKey == 9)
                        {
                            if (tradeType.ToUpper() == "BUY")
                            {
                                if (path.MW6 != null && path.Price6 > nodePrice)
                                    mw = (double)path.MW6;
                                else if (path.MW5 != null && path.Price5 >= nodePrice)
                                    mw = (double)path.MW5;
                                else if (path.MW4 != null && path.Price4 >= nodePrice)
                                    mw = (double)path.MW4;
                                else if (path.MW3 != null && path.Price3 >= nodePrice)
                                    mw = (double)path.MW3;
                                else if (path.MW2 != null && path.Price2 >= nodePrice)
                                    mw = (double)path.MW2;
                                else if (path.MW1 != null && path.Price1 >= nodePrice)
                                    mw = (double)path.MW1;
                            }
                            else
                            {
                                if (path.MW6 != null && path.Price6 < nodePrice)
                                    mw = (double)path.MW6;
                                else if (path.MW5 != null && path.Price5 < nodePrice)
                                    mw = (double)path.MW5;
                                else if (path.MW4 != null && path.Price4 < nodePrice)
                                    mw = (double)path.MW4;
                                else if (path.MW3 != null && path.Price3 < nodePrice)
                                    mw = (double)path.MW3;
                                else if (path.MW2 != null && path.Price2 < nodePrice)
                                    mw = (double)path.MW2;
                                else if (path.MW1 != null && path.Price1 < nodePrice)
                                    mw = (double)path.MW1;

                                mw = mw * -1;
                            }
                            totalMw += mw;
                        }

                    }
                    double CRRCost = 0;
                    List<DateTime> marketDateList = costHash.Keys.ToList<DateTime>();
                    foreach (DateTime marketDate in marketDateList)
                    {
                        Cost cost = costHash[marketDate];
                        if (path.HedgeType.ToUpper() == "OBLIGATION" || path.HedgeType.ToUpper() == "OBL")
                        {
                            if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD" || path.ClassType.ToUpper() == "PEAKWE")
                            {
                                CRRCost = cost.Peak / cost.PeakHours;
                            }
                            else
                            {
                                CRRCost = cost.OffPeak / cost.OffPeakHours;
                            }
                        }
                        else
                        {
                            if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD" || path.ClassType.ToUpper() == "PEAKWE")
                            {
                                sourceSink.Option = cost.Peak;
                            }
                            else
                            {
                                sourceSink.Option = cost.OffPeak;
                            }
                        }
                        if (double.IsNaN(CRRCost) || double.IsInfinity(CRRCost))
                        {
                            continue;
                        }
                        Dictionary<int, PNL> dayHash = new Dictionary<int, PNL>();
                        if (monthHash.ContainsKey(marketDate))
                        {
                            dayHash = monthHash[marketDate];
                            monthHash.Remove(marketDate);
                        }
                        if (cost.DACongestionList == null)
                            continue;
                        cost.DACongestionList.RemoveAll(a => a == null);
                        DateTime? maxDate = null;
                        if (cost.DACongestionList[0].MarketDateTime.Month == 10)
                        {
                            maxDate = cost.DACongestionList.Max(x => x.MarketDateTime.Date);
                            //System.Diagnostics.Debug.WriteLine(maxDate + "," + path.SourceKey + "," + path.SinkKey + "," + cost.DACongestionList.Count );
                        }

                        foreach (DACongestion daCongestion in cost.DACongestionList)
                        {
                            Dictionary<DateTime, Cost> rtcosthash = dic[strKey];// CRRCostHash[strKey];
                            DateTime dt = Convert.ToDateTime(daCongestion.MarketDateTime.Month + "/1/" + daCongestion.MarketDateTime.Year);
                            // DACongestion rtCongestion = rtcosthash[dt].RTCongestionList.First(a => a.MarketDateTime == daCongestion.MarketDateTime);
                            DACongestion rtCongestion = new DACongestion();
                            if (rtcosthash[dt].RTCongestionList.Exists(a => a.MarketDateTime == daCongestion.MarketDateTime))
                            {
                                rtCongestion = rtcosthash[dt].RTCongestionList.First(a => a.MarketDateTime == daCongestion.MarketDateTime);
                            }

                            double rtPrice = 0;
                            double daPrice = 0;
                            double CRRHours = 0;
                            if (path.ClassType.ToUpper() == "PEAK" || path.ClassType.ToUpper() == "ONPEAK" || path.ClassType.ToUpper() == "PEAKWD" || path.ClassType.ToUpper() == "PEAKWE")
                            {
                                daPrice = daCongestion.Peak;
                                CRRHours = daCongestion.PeakHours;
                                rtPrice = rtCongestion.Peak;
                            }
                            else
                            {
                                daPrice = daCongestion.OffPeak;
                                CRRHours = daCongestion.OffPeakHours;
                                rtPrice = rtCongestion.OffPeak;
                            }
                            if (CRRHours != 0)
                            {
                                if (!dayHash.ContainsKey(daCongestion.MarketDateTime.Day))
                                    dayHash.Add(daCongestion.MarketDateTime.Day, new PNL() { DART = 0, DA = 0, Cost = 0, RT = 0 });

                                double dart = (daPrice - CRRCost) * CRRHours * mw;
                                PNL pnl = dayHash[daCongestion.MarketDateTime.Day];
                                pnl.DART += dart;
                                pnl.DA += daPrice;
                                pnl.Cost += CRRCost;
                                pnl.RT += rtPrice;
                            }

                            if (maxDate.HasValue && maxDate == daCongestion.MarketDateTime.Date)
                            {
                                System.Diagnostics.Debug.WriteLine(maxDate + "," + path.SourceKey + "," + path.SinkKey + "," +
                                    daPrice + "," +
                                   CRRCost + ",");
                            }
                        }
                        monthHash.Add(marketDate, dayHash);
                    }
                }
            }
            List<DateTime> monthList = monthHash.Keys.ToList<DateTime>();
            Dictionary<DateTime, PNL> monthlyDartHash = new Dictionary<DateTime, PNL>();
            foreach (DateTime monthDate in monthList)
            {
                if (!IsDateValid(monthDate.Month))
                {
                    continue;
                }

                int days = 0;
                double monthlyTotalDart = 0.0;
                double monthlyTotalDA = 0.0;
                double monthlyTotalCost = 0.0;
                double monthlyTotalRT = 0.0;

                Dictionary<int, PNL> pnlHash = monthHash[monthDate];
                HourlyPivotData monthlyPivotData = new HourlyPivotData();
                monthlyPivotData.DateDisplay = monthDate;
                monthlyPivotData.Date = monthDate;
                monthlyPivotData.RowDay = "";
                monthlyPivotData.RowName = "";
                monthlyPivotData.RowType = "";
                monthlyPivotData.RowDay = "";
                if (DARTChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE27 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].DART;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalDart;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalDart / days;
                    }
                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "DART";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }
                else if (CostPriceChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE27 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].Cost;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalDart;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalDart / days;
                    }
                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "COST";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }
                else if (DAPriceChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))

                        {
                            monthlyPivotData.HE27 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].DA;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalDart;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalDart / days;
                    }
                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "DA";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }
                else if (RTPriceChecked == true)
                {
                    for (int i = 1; i < 32; i++)
                    {
                        #region daysCalculation
                        if (i == 1 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE1 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 2 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE2 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 3 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE3 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 4 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE4 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 5 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE5 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 6 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE6 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 7 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE7 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 8 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE8 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 9 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE9 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 10 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE10 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 11 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE11 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 12 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE12 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 13 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE13 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 14 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE14 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 15 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE15 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 16 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE16 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 17 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE17 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 18 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE18 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 19 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE19 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 20 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE20 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 21 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE21 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 22 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE22 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 23 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE23 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 24 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE24 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 25 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE25 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 26 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE26 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 27 && pnlHash.ContainsKey(i))

                        {
                            monthlyPivotData.HE27 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 28 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE28 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 29 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE29 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 30 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE30 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        if (i == 31 && pnlHash.ContainsKey(i))
                        {
                            monthlyPivotData.HE31 = pnlHash[i].RT;
                            monthlyTotalDart += pnlHash[i].DART;
                            monthlyTotalDA += pnlHash[i].DA;
                            monthlyTotalCost += pnlHash[i].Cost;
                            monthlyTotalRT += pnlHash[i].RT;
                            days++;
                        }
                        #endregion
                        monthlyPivotData.Total = monthlyTotalRT;
                        if (days > 0)
                            monthlyPivotData.Average = monthlyTotalRT / days;
                    }
                    PNL monthlyPnl = new PNL();
                    monthlyPnl.DART = monthlyTotalDart;
                    monthlyPnl.Cost = monthlyTotalCost;
                    monthlyPnl.DA = monthlyTotalDA;
                    monthlyPnl.RT = monthlyTotalRT;
                    monthlyPivotData.RowDisplayType = "RT";
                    monthlyPivotDataList.Add(monthlyPivotData);
                    monthlyDartHash.Add(monthDate, monthlyPnl);
                }

                //MonthlyDartHash = monthlyDartHash;
            }
            monthlyPivotDataList.Reverse();
            if (isMustTake)
            {
                MustTakeMonthlyPivotList = null;
                MustTakeMonthlyPivotList = monthlyPivotDataList;
            }
            else
            {
                AsBidMonthlyPivotList = null;
                AsBidMonthlyPivotList = monthlyPivotDataList;
            }
            if (AsBidMonthlyPivotList.Count != 0)
                GetSummaryList(AsBidMonthlyPivotList, MustTakeMonthlyPivotList);
        }
        /// <summary>
        /// Credits this instance.
        /// </summary>
        private void Credit()
        {
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.OpenTimeout = new TimeSpan(0, 360, 0);
            myBinding.SendTimeout = new TimeSpan(0, 360, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 360, 0);
            myBinding.CloseTimeout = new TimeSpan(0, 360, 0);
            myBinding.TransactionFlow = false;
            myBinding.Security.Mode = SecurityMode.None;
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.TransferMode = TransferMode.Streamed;
            myBinding.ReaderQuotas.MaxArrayLength = 5000000;
            ChannelFactory<ICRRCredit> pipeFactory = new ChannelFactory<ICRRCredit>(myBinding, new EndpointAddress(mEndPoint));
            foreach (OperationDescription op in pipeFactory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior =
                            op.Behaviors.Find<DataContractSerializerOperationBehavior>()
                            as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
            try
            {
                mCRRCalculationCreditProxy = pipeFactory.CreateChannel();
            }
            catch (Exception ex)
            {
            }
            List<Credit> creditList = new List<Vayu.CRRCreditLibrary.Credit>();
            int marketKey = GetMarketKey();
            double totalMW = 0;

            double total_allMW = 0;
            foreach (FTRBid FTRBid in PathList)
            {
                Credit credit = new Credit();
                credit.ID = FTRBid.ID;
                credit.PeriodName = FTRBid.PeriodName;
                credit.PeriodKey = FTRBid.PeriodKey;
                credit.PeriodHours = FTRBid.PeriodHours;
                credit.Type = FTRBid.TradeType;
                credit.HedgeType = FTRBid.HedgeType;
                credit.ClassType = FTRBid.ClassType;
                PricingNode pNodeSource = DBAccess.GetNodeFromName(FTRBid.Source, marketKey);
                PricingNode pNodeSink = DBAccess.GetNodeFromName(FTRBid.Sink, marketKey);
                if (pNodeSource == null || pNodeSink == null)
                {
                    continue;
                }
                credit.SourceNodeKey = pNodeSource.NodeKey;
                credit.SinkNodeKey = pNodeSink.NodeKey;
                credit.PriceMWList = new List<CreditPriceMW>();
                if (FTRBid.MW1 != null && FTRBid.Price1 != null)
                {
                    CreditPriceMW creditPriceMW = new CreditPriceMW();
                    creditPriceMW.MW = (double)FTRBid.MW1;
                    creditPriceMW.Price = (double)FTRBid.Price1;
                    credit.PriceMWList.Add(creditPriceMW);
                    totalMW += Math.Round(creditPriceMW.MW, 1);
                }
                if (FTRBid.MW2 != null && FTRBid.Price2 != null)
                {
                    CreditPriceMW creditPriceMW = new CreditPriceMW();
                    creditPriceMW.MW = (double)FTRBid.MW2;
                    creditPriceMW.Price = (double)FTRBid.Price2;
                    credit.PriceMWList.Add(creditPriceMW);
                    totalMW += Math.Round(creditPriceMW.MW, 1);
                }
                if (FTRBid.MW3 != null && FTRBid.Price3 != null)
                {
                    CreditPriceMW creditPriceMW = new CreditPriceMW();
                    creditPriceMW.MW = (double)FTRBid.MW3;
                    creditPriceMW.Price = (double)FTRBid.Price3;
                    credit.PriceMWList.Add(creditPriceMW);
                    totalMW += Math.Round(creditPriceMW.MW, 1);
                }
                if (FTRBid.MW4 != null && FTRBid.Price4 != null)
                {
                    CreditPriceMW creditPriceMW = new CreditPriceMW();
                    creditPriceMW.MW = (double)FTRBid.MW4;
                    creditPriceMW.Price = (double)FTRBid.Price4;
                    credit.PriceMWList.Add(creditPriceMW);
                    totalMW += Math.Round(creditPriceMW.MW, 1);
                }
                if (FTRBid.MW5 != null && FTRBid.Price5 != null)
                {
                    CreditPriceMW creditPriceMW = new CreditPriceMW();
                    creditPriceMW.MW = (double)FTRBid.MW5;
                    creditPriceMW.Price = (double)FTRBid.Price5;
                    credit.PriceMWList.Add(creditPriceMW);
                    totalMW += Math.Round(creditPriceMW.MW, 1);
                }
                if (FTRBid.MW6 != null && FTRBid.Price6 != null)
                {
                    CreditPriceMW creditPriceMW = new CreditPriceMW();
                    creditPriceMW.MW = (double)FTRBid.MW6;
                    creditPriceMW.Price = (double)FTRBid.Price6;
                    credit.PriceMWList.Add(creditPriceMW);
                    totalMW += Math.Round(creditPriceMW.MW, 1);
                }
                creditList.Add(credit);
            }
            Dictionary<long, CreditResult> creditHash = mCRRCalculationCreditProxy.GetCredit(marketKey, creditList);
            pipeFactory.Close();
            double totalCredit = 0;
            List<FTRBid> pathList = new List<FTRBid>();
            foreach (FTRBid FTRBid in PathList)
            {
                FTRBid path = new FTRBid(FTRBid);
                if (creditHash.ContainsKey(FTRBid.ID))
                {
                    double credit = creditHash[FTRBid.ID].Credit;
                    totalCredit += credit;
                    path.Credit = credit;
                    if (creditHash[FTRBid.ID].RefPrice != 0)
                    {
                        path.RefPrice = creditHash[FTRBid.ID].RefPrice;
                    }
                }
                pathList.Add(path);
            }
            PathList = null;
            PathList = pathList;
            if (PathList != null)
                CountText = PathList.Count;
            TotalCredit = totalCredit.ToString("$#,##0;(#,##0)");
            MWText = totalMW.ToString("#,##0.0");
            //creditProxy.Close();
            pipeFactory.Close();
        }
        /// <summary>
        /// Cancels this instance.
        /// </summary>
        public void Cancel()
        {
            if (SelectedTransaction == null)
            {
                MessageBox.Show("Please select transaction");
                return;
            }
            // ChannelFactory<ICRRCredit> pipeFactory = GetCRRFactoryObject();
            #region ServiceBinding
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.OpenTimeout = new TimeSpan(0, 360, 0);
            myBinding.SendTimeout = new TimeSpan(0, 360, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 360, 0);
            myBinding.CloseTimeout = new TimeSpan(0, 360, 0);
            myBinding.TransactionFlow = false;
            myBinding.Security.Mode = SecurityMode.None;
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.TransferMode = TransferMode.Streamed;
            myBinding.ReaderQuotas.MaxArrayLength = 5000000;
            ChannelFactory<ICRRCredit> pipeFactory = new ChannelFactory<ICRRCredit>(myBinding, new EndpointAddress(""));
            foreach (OperationDescription op in pipeFactory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior =
                            op.Behaviors.Find<DataContractSerializerOperationBehavior>()
                            as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
            try
            {
                mCRRCalculationCreditProxy = pipeFactory.CreateChannel();
            }
            catch (Exception ex)
            {
            }
            #endregion
            ICRRCredit submitProxy = pipeFactory.CreateChannel();
            string resultString = submitProxy.Cancel(PortfolioComboSelectedItem.ID, SelectedTransaction.ID, SelectedRound);
            pipeFactory.Close();
            //     ShowErrorMsg(resultString);
            MessageBox.Show(resultString);
            Clear();
            SetTransaction();
        }
        /// <summary>
        /// Submits this instance.
        /// </summary>
        public void Submit()
        {
            FinalSubmit(false);
        }
        /// <summary>
        /// Refreshes the filter data.
        /// </summary>
        /// <param name="nodeList">The node list.</param>
        /// <param name="daFilteredSortedList">The da filtered sorted list.</param>
        /// <param name="rtFilteredSortedList">The rt filtered sorted list.</param>
        /// <param name="dartFilteredSortedList">The dart filtered sorted list.</param>
        public void RefreshFilterData(List<Node> nodeList, out List<Node> daFilteredSortedList, out List<Node> rtFilteredSortedList, out List<Node> dartFilteredSortedList)
        {
            daFilteredSortedList = new List<Node>();
            rtFilteredSortedList = new List<Node>();
            dartFilteredSortedList = new List<Node>();
            if (nodeList == null)
            {
                return;
            }
            foreach (Node node in nodeList)
            {
                if (node.NodeName == "DA")
                {
                    daFilteredSortedList.Add(new Node(node));
                }
                if (node.NodeName == "RT")
                {
                    rtFilteredSortedList.Add(new Node(node));
                }
                if (node.NodeName == "DART")
                {
                    dartFilteredSortedList.Add(new Node(node));
                }
            }
        }

        /// <summary>
        /// Updates the chart command.
        /// </summary>
        public void UpdateChartCommand()
        {
            #region OldCOde
            PathPlotModelUpper = null;
            //List<Node> asBidSummaryNodeList = new List<Node>();
            //List<Node> mustTakeSummaryList = new List<Node>();
            ////   mHourList = new List<DateTime>();
            ////if (FilterList != null)
            ////{
            ////    foreach (FilterData filterData in FilterList)
            ////    {
            ////        double? min = filterData.Min == null ? double.MinValue : filterData.Min;
            ////        double? max = filterData.Max == null ? double.MaxValue : filterData.Max;
            ////        SetAllInvalidHours(filterData.Product, filterData.Type, min, max);
            ////    }
            ////}
            //Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> asBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            //Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>> mustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<FTRBid, PNL>>>();
            //FilterAll(out asBidMarketDateTimeHash, out mustTakeMarketDateTimeHash);
            ////
            ////    GetSummaryNodeList(asBidMarketDateTimeHash, mustTakeMarketDateTimeHash, out asBidSummaryNodeList, out mustTakeSummaryList);
            //GetSummaryNodeList();
            ////
            //if (asBidSummaryNodeList == null || mustTakeSummaryList == null)
            //{
            //    return;
            //}
            //List<Node> asBidNodeList = new List<Node>();
            //List<Node> mustTakeNodeList = new List<Node>();
            //List<Node> asBidDaSendFilteredSortedList = new List<Node>();
            //List<Node> asBidRtSendFilteredSortedList = new List<Node>();
            //List<Node> asBidDartSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeDaSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeRtSendFilteredSortedList = new List<Node>();
            //List<Node> mustTakeDartSendFilteredSortedList = new List<Node>();
            //for (int i = 0; i < 2; i++)
            //{
            //    List<Node> daFilteredSortedList = null;
            //    List<Node> rtFilteredSortedList = null;
            //    List<Node> dartFilteredSortedList = null;
            //    List<Node> sendNodeList = i == 0 ? asBidSummaryNodeList : mustTakeSummaryList;
            //    RefreshFilterData(sendNodeList, out daFilteredSortedList, out rtFilteredSortedList, out dartFilteredSortedList);
            //    List<Node> node1List = new List<Node>();
            //    if (i == 0)
            //    {
            //        asBidDaSendFilteredSortedList = daFilteredSortedList;
            //        asBidRtSendFilteredSortedList = rtFilteredSortedList;
            //        asBidDartSendFilteredSortedList = dartFilteredSortedList;
            //        node1List = asBidNodeList;
            //    }
            //    else
            //    {
            //        mustTakeDaSendFilteredSortedList = daFilteredSortedList;
            //        mustTakeRtSendFilteredSortedList = rtFilteredSortedList;
            //        mustTakeDartSendFilteredSortedList = dartFilteredSortedList;
            //        node1List = mustTakeNodeList;
            //    }
            //    foreach (Node node in daFilteredSortedList)
            //    {
            //        node1List.Add(node);
            //    }
            //    foreach (Node node in rtFilteredSortedList)
            //    {
            //        node1List.Add(node);
            //    }
            //    foreach (Node node in dartFilteredSortedList)
            //    {
            //        node1List.Add(node);
            //    }
            //}
            //List<Node> nodeList = AsBidChecked ? asBidNodeList : mustTakeNodeList;
            ///*if (DailyChecked)
            //{
            //    List<Node> asBidSendNodeList = new List<Node>();
            //    List<Node> mustTakeSendNodeList = new List<Node>();
            //    ConvertDailyValues(asBidNodeList, mustTakeNodeList, out asBidSendNodeList, out mustTakeSendNodeList);
            //    nodeList = AsBidChecked ? asBidSendNodeList : mustTakeSendNodeList;
            //}
            //Sort(nodeList);*/
            //PlotModelUpper = CreatePlotModelUpper(nodeList);
            //PlotModelLower = CreatePlotModelLower(nodeList);
            //List<Node> daSendFilteredSortedList;
            //List<Node> rtSendFilteredSortedList;
            //List<Node> dartSendFilteredSortedList;
            //if (AsBidChecked)
            //{
            //    daSendFilteredSortedList = asBidDaSendFilteredSortedList;
            //    rtSendFilteredSortedList = asBidRtSendFilteredSortedList;
            //    dartSendFilteredSortedList = asBidDartSendFilteredSortedList;
            //}
            //else
            //{
            //    daSendFilteredSortedList = mustTakeDaSendFilteredSortedList;
            //    rtSendFilteredSortedList = mustTakeRtSendFilteredSortedList;
            //    dartSendFilteredSortedList = mustTakeDartSendFilteredSortedList;
            //}
            //    SetHourlyPivotListSummary(daSendFilteredSortedList, rtSendFilteredSortedList, dartSendFilteredSortedList);
            //   RefreshDayComparisonGridCommand(daSendFilteredSortedList, rtSendFilteredSortedList, dartSendFilteredSortedList);
            //     SetPathRisk(); 
            #endregion
            PlotModelUpper = null;
            PlotModelLower = null;
            Retrieve();
            //MonthlyPivotList.Clear();
            FetchAllPortfolioData(true, MustTakeChecked);
        }

        /// <summary>
        /// Updates the chart commandfor selection.
        /// </summary>
        /// <param name="monthlyDartHash">The monthly dart hash.</param>
        public void UpdateChartCommandforSelection(Dictionary<DateTime, PNL> monthlyDartHash)
        {

            PlotModelUpper = null;
            PlotModelLower = null;
            PlotModelUpper = CreateMonthlyDARTPlot(monthlyDartHash);
            PlotModelLower = CreateCumiliativeDart(monthlyDartHash);
            // Retrieve();
            //MonthlyPivotList.Clear();
            // FetchAllPortfolioData(true)
        }

        /// <summary>
        /// Refreshes the day comparison grid command.
        /// </summary>
        /// <param name="daFilteredSortedList">The da filtered sorted list.</param>
        /// <param name="rtFilteredSortedList">The rt filtered sorted list.</param>
        /// <param name="dartFilteredSortedList">The dart filtered sorted list.</param>
        public void RefreshDayComparisonGridCommand(List<Node> daFilteredSortedList, List<Node> rtFilteredSortedList, List<Node> dartFilteredSortedList)
        {
            if (mHourlyPivotHash.Keys.Contains("DART"))
            {
                mHourlyPivotHash.Remove("DART");
            }
            mHourlyPivotHash.Add("DART", dartFilteredSortedList);
            if (mHourlyPivotHash.ContainsKey("DA"))
            {
                mHourlyPivotHash.Remove("DA");
            }
            mHourlyPivotHash.Add("DA", daFilteredSortedList);
            if (mHourlyPivotHash.Keys.Contains("RT"))
            {
                mHourlyPivotHash.Remove("RT");
            }
            mHourlyPivotHash.Add("RT", rtFilteredSortedList);
            SetHourlyPivotList();
        }

        /// <summary>
        ///// Retrieves this instance.
        /// </summary>
        public void Retrieve()
        {
            if (PortfolioList == null || PortfolioList.Count == 0)
            {
                return;
            }
            List<string> ids = new List<string>();
            if (TransactionList != null && TransactionList.Count != 0)
            {
                foreach (var item in TransactionList)
                {
                    if (item.IsSelected)
                        ids.Add(item.ID);
                }
            }
            double? totalMW = 0;
            PathList = null;
            List<FTRBid> pathsList = new List<FTRBid>();
            foreach (Portfolio portfolio in PortfolioList)
            {
                //List<FTRBid> pathList = DBAccess.GetFtrBidList(portfolio.ID, AuctionSelectedItem, ShowValidChecked, GetMarketKey(), ids);
                List<FTRBid> pathList = DBAccess.GetAnnualFtrBidList(portfolio.ID, AuctionSelectedItem, ShowValidChecked, GetMarketKey(), ids);
                pathsList.AddRange(pathList);
            }
            PathList = pathsList;

            //SetTransaction();
            //Credit();
        }

        public void CalculateMonthlyAnalysis()
        {
            if (PortfolioList == null || PortfolioList.Count == 0)
            {
                return;
            }

            List<string> ids = new List<string>();
            if (TransactionList != null && TransactionList.Count != 0)
            {
                foreach (var item in TransactionList)
                {
                    if (item.IsSelected)
                        ids.Add(item.ID);
                }
            }
            double? totalMW = 0;
            PathList = null;
            List<FTRBid> pathsList = new List<FTRBid>();
            foreach (Portfolio portfolio in PortfolioList)
            {
                List<FTRBid> pathList = DBAccess.GetFtrBidList(portfolio.ID, AuctionSelectedItem, ShowValidChecked, GetMarketKey(), ids);
                pathsList.AddRange(pathList);
            }
            PathList = pathsList;

            SetTransaction();
            Credit();
            UpdateView(PathList);
        }


        /// <summary>
        /// Imports this instance.
        /// </summary>
        public void Import()
        {
            Func<string, double?> func = (str) =>
            {
                if (str == null || str.Trim().Length == 0)
                {
                    return null;
                }
                return double.Parse(str);
            };
            if (TraderPortfolioComboSelectedItem == null)
            {
                MessageBox.Show("Please select portfolio");
                return;
            }
            int marketKey = GetMarketKey();
            if (TraderPortfolioComboSelectedItem.Name.Contains("Annual"))
            {
                CRRAuction FtrAuction = new CRRAuction();
                FtrAuction.Name = TraderPortfolioComboSelectedItem.Name;
                FtrAuction.Key = TraderPortfolioComboSelectedItem.Key;
                Tuple<int, string> detail = GetPortfolioAuctionDetail();

                int portfolioKey = detail.Item1;
                FtrAuction.Name = detail.Item2;
                string periodError = string.Empty;
                DateTime CRRMonth = _dataService.GetAuctionStartDate(GetMarketKey(), FtrAuction.Name);
                ImportHelper import = ImportHelper.ConstructByMarket(MarketComboSelectedValue, CRRMonth);
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = fileDialog.ShowDialog();
                if (result != true)
                {
                    return;
                }
                List<Bid> bidList = new List<Bid>();
                using (FileStream stream = File.Open(fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StreamReader fs = new StreamReader(stream);
                    string line = fs.ReadLine();
                    line = fs.ReadLine();
                    List<FTRBid> FTRBidList = new List<FTRBid>();
                    while (line != null)
                    {
                        try
                        {
                            string[] tokens = line.Split(',');
                            FTRBid bid = new FTRBid();
                            bid.mParticipant = import.Participant;
                            bid.mMarketKey = import.MarketKey;

                            if (marketKey == 9)
                                bid.HedgeType = tokens[6].ToString().ToLower() == "obl" ? "OBL" : tokens[6].ToString().ToLower() == "opt" ? "OPT" : tokens[6].ToString();//"OBL";
                           
                            string tempSource = tokens[0].Replace("?", " ");
                            bid.Source = tempSource.Replace("�", " ");
                            string tempSink = tokens[1].Replace("?", " ");
                            bid.Sink = tempSink.Replace("�", " ");
                            if (bid.Source.Trim().Length == 0 || bid.Sink.Trim().Length == 0)
                            {
                                break;
                            }
                            bid.SourceExt = import.GetExternalID(bid.Source);
                            bid.SinkExt = import.GetExternalID(bid.Sink);
                            bid.TradeType = tokens[2].ToUpper();
                            bid.ClassType = tokens[3].ToUpper();
                           // bid.PeriodName = tokens[4];
                            Period p = new Period();
                            //if (marketKey == 9)
                            //{
                            //    p = import.GetPeriodErcotByName(bid.PeriodName);
                            //}

                            if (p == null)
                            {
                                if (string.IsNullOrEmpty(periodError))
                                {
                                    periodError = "Errors in reading Period Name, All records are not imported.";
                                }
                                continue;
                            }
                           // bid.PeriodKey = p.PeriodKey;
                            bid.month = CRRMonth;
                            bid.mPeriodStartDate = p.Date;
                            //if (bid.ClassType.ToUpper() == "PEAKWE")
                            //{
                            //    bid.PeriodHours = p.PeakWEHours;
                            //}
                            //else if (bid.ClassType.ToUpper() == "PEAK" || bid.ClassType.ToUpper() == "ONPEAK" || bid.ClassType.ToUpper() == "PEAKWD")
                            //{
                            //    bid.PeriodHours = p.PeakHours;
                            //}
                            //else
                            //{
                            //    bid.PeriodHours = p.OffPeakHours;
                            //    if (marketKey == 9)
                            //    {
                            //        bid.ClassType = "OFF-PEAK";
                            //    }
                            //}

                            bid.ClassType = tokens[5];

                            if (marketKey == 9)
                            {
                                bid.TCRID = import.GetTCRErcotID(portfolioKey);
                            }
                            bid.StartDate = Convert.ToDateTime(tokens[7]);
                            bid.EndDate = Convert.ToDateTime(tokens[8]);
                            bid.MW1 = func(tokens[9]);
                            bid.Price1 = func(tokens[10]);
                            bid.MW2 = func(tokens[11]);
                            bid.Price2 = func(tokens[12]);
                            bid.MW3 = func(tokens[13]);
                            bid.Price3 = func(tokens[14]);
                            bid.MW4 = func(tokens[15]);
                            bid.Price4 = func(tokens[16]);
                            bid.MW5 = func(tokens[17]);
                            bid.Price5 = func(tokens[18]);
                            bid.MW6 = func(tokens[19]);
                            bid.Price6 = func(tokens[20]);
                            FTRBidList.Add(bid);
                        }
                        catch (Exception ex)
                        {

                        }
                        line = fs.ReadLine();
                    }

                    fs.Close();
                    if (FTRBidList.Count != 0)
                    {
                        if (marketKey == 9)
                        {
                            //DBAccess.SaveFtrErcotBids(portfolioKey, FtrAuction.Name, FTRBidList);
                            DBAccess.SaveFtrErcotBidsAnnual(portfolioKey, FtrAuction.Name, FTRBidList);
                        }
                    }
                }
                SetPortfolioList();
                // this.PortfolioComboSelectedItem = PortfolioSelectedValue;
                // CurrentPortfolio = PortfolioSelectedValue;
                AddPortfolio();
                Retrieve();
                SetAuctionList();
                if (string.IsNullOrEmpty(periodError))
                {
                    MessageBox.Show("Import Done.");
                }
                else
                {
                    MessageBox.Show(periodError);
                }

            }
            else
            {
                CRRAuction FtrAuction = new CRRAuction();
                FtrAuction.Name = TraderPortfolioComboSelectedItem.Name;
                FtrAuction.Key = TraderPortfolioComboSelectedItem.Key;
                Tuple<int, string> detail = GetPortfolioAuctionDetail();

                int portfolioKey = detail.Item1;
                FtrAuction.Name = detail.Item2;
                string periodError = string.Empty;
                DateTime CRRMonth = _dataService.GetAuctionStartDate(GetMarketKey(), FtrAuction.Name);
                ImportHelper import = ImportHelper.ConstructByMarket(MarketComboSelectedValue, CRRMonth);
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
                Nullable<bool> result = fileDialog.ShowDialog();
                if (result != true)
                {
                    return;
                }
                List<Bid> bidList = new List<Bid>();
                using (FileStream stream = File.Open(fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StreamReader fs = new StreamReader(stream);
                    string line = fs.ReadLine();
                    line = fs.ReadLine();
                    List<FTRBid> FTRBidList = new List<FTRBid>();
                    while (line != null)
                    {
                        try
                        {
                            string[] tokens = line.Split(',');
                            FTRBid bid = new FTRBid();
                            bid.mParticipant = import.Participant;
                            bid.mMarketKey = import.MarketKey;
                            if (marketKey == 9)
                                bid.HedgeType = tokens[17].ToString().ToLower() == "obl" ? "OBL" : tokens[17].ToString().ToLower() == "opt" ? "OPT" : tokens[17].ToString();//"OBL";
                            string tempSource = tokens[0].Replace("?", " ");
                            bid.Source = tempSource.Replace("�", " ");
                            string tempSink = tokens[1].Replace("?", " ");
                            bid.Sink = tempSink.Replace("�", " ");
                            if (bid.Source.Trim().Length == 0 || bid.Sink.Trim().Length == 0)
                            {
                                break;
                            }
                            bid.SourceExt = import.GetExternalID(bid.Source);
                            bid.SinkExt = import.GetExternalID(bid.Sink);
                            bid.TradeType = tokens[2].ToUpper();
                            bid.ClassType = tokens[3].ToUpper();
                            bid.PeriodName = tokens[4];
                            Period p = new Period();
                            if (marketKey == 9)
                            {
                                p = import.GetPeriodErcotByName(bid.PeriodName);
                            }

                            if (p == null)
                            {
                                if (string.IsNullOrEmpty(periodError))
                                {
                                    periodError = "Errors in reading Period Name, All records are not imported.";
                                }
                                continue;
                            }
                            bid.PeriodKey = p.PeriodKey;
                            bid.month = CRRMonth;
                            bid.mPeriodStartDate = p.Date;
                            if (bid.ClassType.ToUpper() == "PEAKWE")
                            {
                                bid.PeriodHours = p.PeakWEHours;
                            }
                            else

                                if (bid.ClassType.ToUpper() == "PEAK" || bid.ClassType.ToUpper() == "ONPEAK" || bid.ClassType.ToUpper() == "PEAKWD")
                            {
                                bid.PeriodHours = p.PeakHours;
                            }

                            else
                            {
                                bid.PeriodHours = p.OffPeakHours;
                                if (marketKey == 9)
                                {
                                    bid.ClassType = "OFF-PEAK";
                                }
                            }
                            if (marketKey == 9)
                            {
                                bid.TCRID = import.GetTCRErcotID(portfolioKey);
                            }
                            bid.MW1 = func(tokens[5]);
                            bid.Price1 = func(tokens[6]);
                            bid.MW2 = func(tokens[7]);
                            bid.Price2 = func(tokens[8]);
                            bid.MW3 = func(tokens[9]);
                            bid.Price3 = func(tokens[10]);
                            bid.MW4 = func(tokens[11]);
                            bid.Price4 = func(tokens[12]);
                            bid.MW5 = func(tokens[13]);
                            bid.Price5 = func(tokens[14]);
                            bid.MW6 = func(tokens[15]);
                            bid.Price6 = func(tokens[16]);
                            FTRBidList.Add(bid);
                        }
                        catch (Exception ex)
                        {

                        }
                        line = fs.ReadLine();
                    }

                    fs.Close();
                    if (FTRBidList.Count != 0)
                    {
                        if (marketKey == 9)
                        {
                            DBAccess.SaveFtrErcotBids(portfolioKey, FtrAuction.Name, FTRBidList);
                        }
                    }
                }
                SetPortfolioList();
                // this.PortfolioComboSelectedItem = PortfolioSelectedValue;
                // CurrentPortfolio = PortfolioSelectedValue;
                AddPortfolio();
                Retrieve();
                SetAuctionList();
                if (string.IsNullOrEmpty(periodError))
                {
                    MessageBox.Show("Import Done.");
                }
                else
                {
                    MessageBox.Show(periodError);
                }

            }
            
        }


        public void Import1()
        {
            Func<string, double?> func = (str) =>
            {
                if (str == null || str.Trim().Length == 0)
                {
                    return null;
                }
                return double.Parse(str);
            };
            if (TraderPortfolioComboSelectedItem == null)
            {
                MessageBox.Show("Please select portfolio");
                return;
            }
            int marketKey = GetMarketKey();
            CRRAuction FtrAuction = new CRRAuction();
            FtrAuction.Name = TraderPortfolioComboSelectedItem.Name;
            FtrAuction.Key = TraderPortfolioComboSelectedItem.Key;
            Tuple<int, string> detail = GetPortfolioAuctionDetail();

            int portfolioKey = detail.Item1;
            FtrAuction.Name = detail.Item2;
            string periodError = string.Empty;
            DateTime CRRMonth = _dataService.GetAuctionStartDate(GetMarketKey(), FtrAuction.Name);
            ImportHelper import = ImportHelper.ConstructByMarket(MarketComboSelectedValue, CRRMonth);
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = fileDialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            List<Bid> bidList = new List<Bid>();
            using (FileStream stream = File.Open(fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader fs = new StreamReader(stream);
                string line = fs.ReadLine();
                line = fs.ReadLine();
                List<FTRBid> FTRBidList = new List<FTRBid>();
                while (line != null)
                {
                    try
                    {
                        string[] tokens = line.Split(',');
                        FTRBid bid = new FTRBid();
                        bid.mParticipant = import.Participant;
                        bid.mMarketKey = import.MarketKey;
                        if (marketKey == 9)
                            bid.HedgeType = tokens[17].ToString().ToLower() == "obl" ? "OBL" : tokens[17].ToString().ToLower() == "opt" ? "OPT" : tokens[17].ToString();//"OBL";
                        string tempSource = tokens[0].Replace("?", " ");
                        bid.Source = tempSource.Replace("�", " ");
                        string tempSink = tokens[1].Replace("?", " ");
                        bid.Sink = tempSink.Replace("�", " ");
                        if (bid.Source.Trim().Length == 0 || bid.Sink.Trim().Length == 0)
                        {
                            break;
                        }
                        bid.SourceExt = import.GetExternalID(bid.Source);
                        bid.SinkExt = import.GetExternalID(bid.Sink);
                        bid.TradeType = tokens[2].ToUpper();
                        bid.ClassType = tokens[3].ToUpper();
                        bid.PeriodName = tokens[4];
                        Period p = new Period();
                        if (marketKey == 9)
                        {
                            p = import.GetPeriodErcotByName(bid.PeriodName);
                        }

                        if (p == null)
                        {
                            if (string.IsNullOrEmpty(periodError))
                            {
                                periodError = "Errors in reading Period Name, All records are not imported.";
                            }
                            continue;
                        }
                        bid.PeriodKey = p.PeriodKey;
                        bid.month = CRRMonth;
                        bid.mPeriodStartDate = p.Date;
                        if (bid.ClassType.ToUpper() == "PEAKWE")
                        {
                            bid.PeriodHours = p.PeakWEHours;
                        }
                        else

                            if (bid.ClassType.ToUpper() == "PEAK" || bid.ClassType.ToUpper() == "ONPEAK" || bid.ClassType.ToUpper() == "PEAKWD")
                        {
                            bid.PeriodHours = p.PeakHours;
                        }

                        else
                        {
                            bid.PeriodHours = p.OffPeakHours;
                            if (marketKey == 9)
                            {
                                bid.ClassType = "OFF-PEAK";
                            }
                        }
                        if (marketKey == 9)
                        {
                            bid.TCRID = import.GetTCRErcotID(portfolioKey);
                        }
                        bid.MW1 = func(tokens[5]);
                        bid.Price1 = func(tokens[6]);
                        bid.MW2 = func(tokens[7]);
                        bid.Price2 = func(tokens[8]);
                        bid.MW3 = func(tokens[9]);
                        bid.Price3 = func(tokens[10]);
                        bid.MW4 = func(tokens[11]);
                        bid.Price4 = func(tokens[12]);
                        bid.MW5 = func(tokens[13]);
                        bid.Price5 = func(tokens[14]);
                        bid.MW6 = func(tokens[15]);
                        bid.Price6 = func(tokens[16]);
                        FTRBidList.Add(bid);
                    }
                    catch (Exception ex)
                    {

                    }
                    line = fs.ReadLine();
                }

                fs.Close();
                if (FTRBidList.Count != 0)
                {
                    if (marketKey == 9)
                    {
                        DBAccess.SaveFtrErcotBids(portfolioKey, FtrAuction.Name, FTRBidList);
                    }
                }
            }
            SetPortfolioList();
            // this.PortfolioComboSelectedItem = PortfolioSelectedValue;
            // CurrentPortfolio = PortfolioSelectedValue;
            AddPortfolio();
            Retrieve();
            SetAuctionList();
            if (string.IsNullOrEmpty(periodError))
            {
                MessageBox.Show("Import Done.");
            }
            else
            {
                MessageBox.Show(periodError);
            }
        }

        /// <summary>
        /// Removes the portfolio.
        /// </summary>
        public void RemovePortfolio()
        {
            if (ExposureList != null)
            {
                ExposureList.Clear();
            }
            PortfolioList = null;
            mFillPortfolioList = new List<Portfolio>();
            PathList = null;
            TotalCredit = null;

            MWText = null;
            DAText = null;
            CountText = null;
            ClearedText = null;
            SubmittedMwhText = null;

            PortfolioComboSelectedItem = null;
            HourlyPivotListSummary = null;
            TransactionList = null;
            Portfolio_analyser_PathList = null;
        }
        /// <summary>
        /// Adds the portfolio.
        /// </summary>
        public void AddPortfolio()
        {
            if (PortfolioComboSelectedItem != null)
            {
                if (!mFillPortfolioList.Contains(PortfolioComboSelectedItem))
                {
                    PortfolioList = null;
                    mFillPortfolioList.Add(PortfolioComboSelectedItem);
                    PortfolioList = mFillPortfolioList;
                }
                if (!mPortfolioHash.ContainsKey(PortfolioComboSelectedItem.ID))
                {
                    mPortfolioHash.Add(PortfolioComboSelectedItem.ID, PortfolioComboSelectedItem);
                }
            }
        }

        /// <summary>
        /// Sorts the specified monthly dart hash.
        /// </summary>
        /// <param name="monthlyDartHash">The monthly dart hash.</param>
        public void Sort(Dictionary<DateTime, PNL> monthlyDartHash)
        {
            if (SortDaChecked)
            {
                Dictionary<DateTime, PNL> daMonthlyDartHash = monthlyDartHash.OrderBy(x => x.Value.DA).ToDictionary(x => x.Key, x => x.Value);
                UpdateChartCommandforSelection(daMonthlyDartHash);
            }
            else if (SortDartChecked)
            {
                Dictionary<DateTime, PNL> dartMonthlyDartHash = monthlyDartHash.OrderBy(x => x.Value.DART).ToDictionary(x => x.Key, x => x.Value);
                UpdateChartCommandforSelection(dartMonthlyDartHash);
            }
            else if (SortRtChecked)
            {
                Dictionary<DateTime, PNL> costMonthlyDartHash = monthlyDartHash.OrderBy(x => x.Value.Cost).ToDictionary(x => x.Key, x => x.Value);
                UpdateChartCommandforSelection(costMonthlyDartHash);
            }
            else if (SortDateChecked)
            {
                UpdateChartCommandforSelection(MonthlyDartHash);
            }
        }
        #endregion
    }
    public class PortfolioAnalyserHelper
    {
        public string Path_analyser_Source { get; set; }
        public string Path_analyser_Sink { get; set; }

        public string Path_analyser_Source_Zone { get; set; }
        public string Path_analyser_Sink_Zone { get; set; }

        public string Path_analyser_Class_type { get; set; }
        public string Path_analyser_Hedge_type { get; set; }
        public string Path_analyser_DAMinDate { get; set; }
        public string Path_analyser_RTMinDate { get; set; }
        public double Path_analyser_DA_Max_total { get; set; }
        public double Path_analyser_DA_Min_total { get; set; }
        public double? Path_analyser_DA_Max_Per_Day { get; set; }
        public double? Path_analyser_DA_Min_Per_Day { get; set; }

        public double Path_analyser_RT_Max_total { get; set; }
        public double Path_analyser_RT_Min_total { get; set; }
        public double? Path_analyser_RT_Max_Per_Day { get; set; }
        public double? Path_analyser_RT_Min_Per_Day { get; set; }

        public double? Path_analyser_CRR_Max { get; set; }
        public double? Path_analyser_CRR_Min { get; set; }

        public double? Path_analyser_CRR_Avg { get; set; }



    }

    public class PathwiseCalculationHelper
    {
        public string SourceName { get; set; }
        public string SinkName { get; set; }
        public string SourceZone { get; set; }
        public string SinkZone { get; set; }
        public string ClassTYpe { get; set; }
        public double MW { get; set; }
        public DateTime DAHistory { get; set; }
        //
        public double TotalDA { get; set; }
        public double AverageDA { get; set; }
        public double MinDa { get; set; }
        public double MaxDa { get; set; }
        public double WinPCtDa { get; set; }

        public double AverageMonthlyDA { get; set; }
        public double MinMonthlyDa { get; set; }
        public double MaxMonthlyDa { get; set; }
        public double WinPCMonthlytDa { get; set; }
        public double RiskDa { get; set; }
        //
        public double TotalRT { get; set; }
        public double AverageRT { get; set; }
        public double MinRT { get; set; }
        public double MaxRT { get; set; }
        public double WinPCtRT { get; set; }

        public double AverageMonthlyRT { get; set; }
        public double MinMonthlyRT { get; set; }
        public double MaxMonthlyRT { get; set; }
        public double WinPCtMonthlyRT { get; set; }
        public double RiskRT { get; set; }
        //
        public double TotalCRR { get; set; }
        public double AverageCRR { get; set; }
        public double MinCRR { get; set; }
        public double MaxCRR { get; set; }
        public double WinPCtCRR { get; set; }
        public double RiskCRR { get; set; }
        //
        public double TotalDACRR { get; set; }
        public double AverageDACRR { get; set; }
        public double MinDACRR { get; set; }
        public double MaxDACRR { get; set; }
        public double WinPCtDACRR { get; set; }

        public double AverageMonthlyDACRR { get; set; }
        public double MinMonthlyDACRR { get; set; }
        public double MaxMonthlyDACRR { get; set; }
        public double WinPCtMonthlyDACRR { get; set; }
        public double RiskDACRR { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DateValue
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public double Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int X { get; set; }
        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public double Y { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class BackgroundConverter : IValueConverter
    {
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
            ListViewItem item = (ListViewItem)value;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);
            if (index % 2 != 0)
            {
                return Brushes.LightGray;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class MonthCheckbox : INotifyPropertyChanged
    {
        private bool _isChecked;
        private readonly MainWindowViewModel _viewModel;

        public string Name { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    // Check how many months are currently selected
                    int selectedCount = _viewModel.Months.Count(m => m.IsChecked);

                    if (value) // Trying to check this month
                    {
                        if (selectedCount >= 3)
                        {
                            // Do not allow checking if already 3 are selected
                            return;
                        }
                    }

                    // Set the value after all checks
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                    _viewModel.UpdateSelectedMonths(); // Notify the ViewModel
                }
            }
        }

        public MonthCheckbox(string name, MainWindowViewModel viewModel)
        {
            Name = name;
            _viewModel = viewModel; // Initialize the ViewModel reference
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class DailyPivotData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DailyPivotData"/> class.
        /// </summary>
        public DailyPivotData() { }

        /// <summary>
        /// Gets or sets the type of the summary.
        /// </summary>
        /// <value>
        /// The type of the summary.
        /// </value>
        public SummaryRowType SummaryType { get; set; }

        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime? Date { get; set; }
        /// <summary>
        /// Gets or sets the date display.
        /// </summary>
        /// <value>
        /// The date display.
        /// </value>
        public DateTime? DateDisplay { get; set; }
        /// <summary>
        /// Gets or sets the type of the row.
        /// </summary>
        /// <value>
        /// The type of the row.
        /// </value>
        public string RowType { get; set; }
        /// <summary>
        /// Gets or sets the display type of the row.
        /// </summary>
        /// <value>
        /// The display type of the row.
        /// </value>
        public string RowDisplayType { get; set; }
        /// <summary>
        /// Gets or sets the name of the row.
        /// </summary>
        /// <value>
        /// The name of the row.
        /// </value>
        public string RowName { get; set; }
        /// <summary>
        /// Gets or sets the row day.
        /// </summary>
        /// <value>
        /// The row day.
        /// </value>
        public string RowDay { get; set; }
        /// <summary>
        /// Gets or sets the value day count.
        /// </summary>
        /// <value>
        /// The value day count.
        /// </value>
        public int ValueDayCount { get; set; }
        /// <summary>
        /// The day count
        /// </summary>
        public int dayCount;

        /// <summary>
        /// The total
        /// </summary>
        private double? total;
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public double? Total
        {
            get
            {
                return total;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    total = null;
                }
                else
                {
                    total = value;
                }
            }
        }
        /// <summary>
        /// The average
        /// </summary>
        private double? average;
        /// <summary>
        /// Gets or sets the average.
        /// </summary>
        /// <value>
        /// The average.
        /// </value>
        public double? Average
        {
            get
            {
                return average;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    average = null;
                }
                else
                {
                    average = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the d1.
        /// </summary>
        /// <value>
        /// The d1.
        /// </value>
        public double? D1 { get; set; }
        /// <summary>
        /// Gets or sets the d2.
        /// </summary>
        /// <value>
        /// The d2.
        /// </value>
        public double? D2 { get; set; }
        /// <summary>
        /// Gets or sets the d3.
        /// </summary>
        /// <value>
        /// The d3.
        /// </value>
        public double? D3 { get; set; }
        /// <summary>
        /// Gets or sets the d4.
        /// </summary>
        /// <value>
        /// The d4.
        /// </value>
        public double? D4 { get; set; }
        /// <summary>
        /// Gets or sets the d5.
        /// </summary>
        /// <value>
        /// The d5.
        /// </value>
        public double? D5 { get; set; }
        /// <summary>
        /// Gets or sets the d6.
        /// </summary>
        /// <value>
        /// The d6.
        /// </value>
        public double? D6 { get; set; }
        /// <summary>
        /// Gets or sets the d7.
        /// </summary>
        /// <value>
        /// The d7.
        /// </value>
        public double? D7 { get; set; }
        /// <summary>
        /// Gets or sets the d8.
        /// </summary>
        /// <value>
        /// The d8.
        /// </value>
        public double? D8 { get; set; }
        /// <summary>
        /// Gets or sets the d9.
        /// </summary>
        /// <value>
        /// The d9.
        /// </value>
        public double? D9 { get; set; }
        /// <summary>
        /// Gets or sets the D10.
        /// </summary>
        /// <value>
        /// The D10.
        /// </value>
        public double? D10 { get; set; }
        /// <summary>
        /// Gets or sets the D11.
        /// </summary>
        /// <value>
        /// The D11.
        /// </value>
        public double? D11 { get; set; }
        /// <summary>
        /// Gets or sets the D12.
        /// </summary>
        /// <value>
        /// The D12.
        /// </value>
        public double? D12 { get; set; }
        /// <summary>
        /// Gets or sets the D13.
        /// </summary>
        /// <value>
        /// The D13.
        /// </value>
        public double? D13 { get; set; }
        /// <summary>
        /// Gets or sets the D14.
        /// </summary>
        /// <value>
        /// The D14.
        /// </value>
        public double? D14 { get; set; }
        /// <summary>
        /// Gets or sets the D15.
        /// </summary>
        /// <value>
        /// The D15.
        /// </value>
        public double? D15 { get; set; }
        /// <summary>
        /// Gets or sets the D16.
        /// </summary>
        /// <value>
        /// The D16.
        /// </value>
        public double? D16 { get; set; }
        /// <summary>
        /// Gets or sets the D17.
        /// </summary>
        /// <value>
        /// The D17.
        /// </value>
        public double? D17 { get; set; }
        /// <summary>
        /// Gets or sets the D18.
        /// </summary>
        /// <value>
        /// The D18.
        /// </value>
        public double? D18 { get; set; }
        /// <summary>
        /// Gets or sets the D19.
        /// </summary>
        /// <value>
        /// The D19.
        /// </value>
        public double? D19 { get; set; }
        /// <summary>
        /// Gets or sets the D20.
        /// </summary>
        /// <value>
        /// The D20.
        /// </value>
        public double? D20 { get; set; }
        /// <summary>
        /// Gets or sets the D21.
        /// </summary>
        /// <value>
        /// The D21.
        /// </value>
        public double? D21 { get; set; }
        /// <summary>
        /// Gets or sets the D22.
        /// </summary>
        /// <value>
        /// The D22.
        /// </value>
        public double? D22 { get; set; }
        /// <summary>
        /// Gets or sets the D23.
        /// </summary>
        /// <value>
        /// The D23.
        /// </value>
        public double? D23 { get; set; }
        /// <summary>
        /// Gets or sets the D24.
        /// </summary>
        /// <value>
        /// The D24.
        /// </value>
        public double? D24 { get; set; }
        /// <summary>
        /// Gets or sets the D25.
        /// </summary>
        /// <value>
        /// The D25.
        /// </value>
        public double? D25 { get; set; }
        /// <summary>
        /// Gets or sets the D26.
        /// </summary>
        /// <value>
        /// The D26.
        /// </value>
        public double? D26 { get; set; }
        /// <summary>
        /// Gets or sets the D27.
        /// </summary>
        /// <value>
        /// The D27.
        /// </value>
        public double? D27 { get; set; }
        /// <summary>
        /// Gets or sets the D28.
        /// </summary>
        /// <value>
        /// The D28.
        /// </value>
        public double? D28 { get; set; }
        /// <summary>
        /// Gets or sets the D29.
        /// </summary>
        /// <value>
        /// The D29.
        /// </value>
        public double? D29 { get; set; }
        /// <summary>
        /// Gets or sets the D30.
        /// </summary>
        /// <value>
        /// The D30.
        /// </value>
        public double? D30 { get; set; }
        /// <summary>
        /// Gets or sets the D31.
        /// </summary>
        /// <value>
        /// The D31.
        /// </value>
        public double? D31 { get; set; }

        public string ClassType { get; set; }
    }

    public class ConsolidatedData
    {
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public int Hours { get; set; }
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Gets or sets the da value.
        /// </summary>
        /// <value>
        /// The da value.
        /// </value>
        public double? DAValue { get; set; }
        /// <summary>
        /// Gets or sets the rt value.
        /// </summary>
        /// <value>
        /// The rt value.
        /// </value>
        public double? RTValue { get; set; }
        /// <summary>
        /// Gets or sets the Crr value.
        /// </summary>
        /// <value>
        /// The Crr value.
        /// </value>
        public double? CrrValue { get; set; }
        /// <summary>
        /// Gets or sets the daCrr value.
        /// </summary>
        /// <value>
        /// The daCrr value.
        /// </value>
        public double? DACrrValue { get; set; }
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }
        public string ClassType;

    }

    public class FilterData
    {
        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        /// <value>
        /// The product.
        /// </value>
        public string Product { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public double? Min { get; set; }
        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public double? Max { get; set; }
    }

    public class FTRMonthlyData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double? DAPeakCong { get; set; }
        public double? DAOffPeakCong { get; set; }
        public double? DA24Cong { get; set; }
        public double? DAPeakWECong { get; set; }
        public double? PricePeak { get; set; }
        public double? PriceOffPeak { get; set; }
        public int PeakHours { get; set; }
        public int OffPeakHours { get; set; }
        public int PeriodKey { get; set; }
        public double? PriceWE { get; set; }
        public int PeakWE { get; set; }
        public decimal CRR { get; set; }
        public decimal DA { get; set; }
        public string Source { get; set; }
        public string Sink { get; set; }
        public string SourceZone { get; set; }
        public string SinkZone { get; set; }
        public string HedgeType { get; set; }
        public string ClassType { get; set; }
        public double? MW1 { get; set; }
        public double? Price1 { get; set; }
        public double? PricePeakYear1Month1 { get; set; }
        public double? PricePeakYear1Month2 { get; set; }
        public double? PricePeakYear1Month3 { get; set; }
        public double? PricePeakYear2Month1 { get; set; }
        public double? PricePeakYear2Month2 { get; set; }
        public double? PricePeakYear2Month3 { get; set; }
        public double? PricePeakYear3Month1 { get; set; }
        public double? PricePeakYear3Month2 { get; set; }
        public double? PricePeakYear3Month3 { get; set; }

        public double? PriceOffPeakYear1Month1 { get; set; }
        public double? PriceOffPeakYear1Month2 { get; set; }
        public double? PriceOffPeakYear1Month3 { get; set; }
        public double? PriceOffPeakYear2Month1 { get; set; }
        public double? PriceOffPeakYear2Month2 { get; set; }
        public double? PriceOffPeakYear2Month3 { get; set; }
        public double? PriceOffPeakYear3Month1 { get; set; }
        public double? PriceOffPeakYear3Month2 { get; set; }
        public double? PriceOffPeakYear3Month3 { get; set; }

        public double? PriceWEYear1Month1 { get; set; }
        public double? PriceWEYear1Month2 { get; set; }
        public double? PriceWEYear1Month3 { get; set; }
        public double? PriceWEYear2Month1 { get; set; }
        public double? PriceWEYear2Month2 { get; set; }
        public double? PriceWEYear2Month3 { get; set; }
        public double? PriceWEYear3Month1 { get; set; }
        public double? PriceWEYear3Month2 { get; set; }
        public double? PriceWEYear3Month3 { get; set; }

        public double? DAPeakCongYear1Month1 { get; set; }
        public double? DAPeakCongYear1Month2 { get; set; }
        public double? DAPeakCongYear1Month3 { get; set; }
        public double? DAPeakCongYear2Month1 { get; set; }
        public double? DAPeakCongYear2Month2 { get; set; }
        public double? DAPeakCongYear2Month3 { get; set; }
        public double? DAPeakCongYear3Month1 { get; set; }
        public double? DAPeakCongYear3Month2 { get; set; }
        public double? DAPeakCongYear3Month3 { get; set; }

        public double? DAOffPeakCongYear1Month1 { get; set; }
        public double? DAOffPeakCongYear1Month2 { get; set; }
        public double? DAOffPeakCongYear1Month3 { get; set; }
        public double? DAOffPeakCongYear2Month1 { get; set; }
        public double? DAOffPeakCongYear2Month2 { get; set; }
        public double? DAOffPeakCongYear2Month3 { get; set; }
        public double? DAOffPeakCongYear3Month1 { get; set; }
        public double? DAOffPeakCongYear3Month2 { get; set; }
        public double? DAOffPeakCongYear3Month3 { get; set; }

        public double? DAPeakWECongYear1Month1 { get; set; }
        public double? DAPeakWECongYear1Month2 { get; set; }
        public double? DAPeakWECongYear1Month3 { get; set; }
        public double? DAPeakWECongYear2Month1 { get; set; }
        public double? DAPeakWECongYear2Month2 { get; set; }
        public double? DAPeakWECongYear2Month3 { get; set; }
        public double? DAPeakWECongYear3Month1 { get; set; }
        public double? DAPeakWECongYear3Month2 { get; set; }
        public double? DAPeakWECongYear3Month3 { get; set; }

        public decimal CRRYear1Month1 { get; set; }
        public decimal CRRYear1Month2 { get; set; }
        public decimal CRRYear1Month3 { get; set; }
        public decimal CRRYear2Month1 { get; set; }
        public decimal CRRYear2Month2 { get; set; }
        public decimal CRRYear2Month3 { get; set; }
        public decimal CRRYear3Month1 { get; set; }
        public decimal CRRYear3Month2 { get; set; }
        public decimal CRRYear3Month3 { get; set; }

        public decimal DAYear1Month1 { get; set; }
        public decimal DAYear1Month2 { get; set; }
        public decimal DAYear1Month3 { get; set; }
        public decimal DAYear2Month1 { get; set; }
        public decimal DAYear2Month2 { get; set; }
        public decimal DAYear2Month3 { get; set; }
        public decimal DAYear3Month1 { get; set; }
        public decimal DAYear3Month2 { get; set; }
        public decimal DAYear3Month3 { get; set; }

        public decimal DA_CRR_DifferenceYear1Month1 => DAYear1Month1 - CRRYear1Month1;
        public decimal DA_CRR_DifferenceYear1Month2 => DAYear1Month2 - CRRYear1Month2;
        public decimal DA_CRR_DifferenceYear1Month3 => DAYear1Month3 - CRRYear1Month3;
        public decimal DA_CRR_DifferenceYear2Month1 => DAYear2Month1 - CRRYear2Month1;
        public decimal DA_CRR_DifferenceYear2Month2 => DAYear2Month2 - CRRYear2Month2;
        public decimal DA_CRR_DifferenceYear2Month3 => DAYear2Month3 - CRRYear2Month3;
        public decimal DA_CRR_DifferenceYear3Month1 => DAYear3Month1 - CRRYear3Month1;
        public decimal DA_CRR_DifferenceYear3Month2 => DAYear3Month2 - CRRYear3Month2;
        public decimal DA_CRR_DifferenceYear3Month3 => DAYear3Month3 - CRRYear3Month3;
    }



}