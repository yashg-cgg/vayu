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
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Vayu.CRRCalculationLibrary;
using Vayu.CRRPathAnalyzer.Model;
using Vayu.DBLibrary;
using Vayu.NodePriceLibrary;

namespace Vayu.CRRPathAnalyzer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private ObservableCollection<DailyPivotData> mDailyFinalPivotDataList;
        #region Private Variables

        /// <summary>
        /// My data service object
        /// </summary>
        private IDataService myDataService;
        /// <summary>
        /// The m daily pivot hash
        /// </summary>
        private Dictionary<string, List<Node>> mDailyPivotHash = new Dictionary<string, List<Node>>();
        /// <summary>
        /// The m dart filtered sorted list
        /// </summary>
        private List<Node> mDartFilteredSortedList = new List<Node>();
        /// <summary>
        /// The m dart graph list
        /// </summary>
        private List<Node> mDartGraphList = new List<Node>();
        /// <summary>
        /// The m daily node hash
        /// </summary>
        private Dictionary<string, List<Node>> mDailyNodeHash = new Dictionary<string, List<Node>>();
        /// <summary>
        /// The m graph node hash
        /// </summary>
        private Dictionary<string, List<Node>> mGraphNodeHash = new Dictionary<string, List<Node>>();
        /// <summary>
        /// The m source sink data list
        /// </summary>
        private List<SourceSinkData> mSourceSinkDataList = new List<SourceSinkData>();
        /// <summary>
        /// All months selected
        /// </summary>
        bool allMonthsSelected;

        /// <summary>
        /// The m fill source sink hash
        /// </summary>
        private Dictionary<string, SourceSinkState> mFillSourceSinkHash;
        /// <summary>
        /// The m end point
        /// </summary>
        private ISourceSink CrrCalculationLibrary = null;
        private ObservableCollection<DailyPivotData> mDailyPivotDataList = new ObservableCollection<DailyPivotData>();
        private List<ConsolidatedData> mGraphList = new List<ConsolidatedData>();
        Dictionary<int, List<string>> pnodeIdHash = new Dictionary<int, List<string>>();
        private Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> yearHash = new Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>>();
        private string mEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetFTRService();
        private BarSeries colSeries1 = new BarSeries();
        private LineSeries CrrLineSeries = new LineSeries();
        private LineSeries daLineSeries = new LineSeries();
        private LineSeries rtLineSeries = new LineSeries();
        #endregion



        #region Properties
        private List<FilterData> mFillFilterList = new List<FilterData>();
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
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
        /// The m daily checked
        /// </summary>
        private bool mDailyChecked;
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
                if (mDailyChecked)
                {
                    mMonthlyChecked = false;
                    UpdateView(true);
                }
                RaisePropertyChanged("DailyChecked");
            }
        }
        /// <summary>
        /// The m jan checked
        /// </summary>
        private bool mJanChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("JanChecked");
            }
        }
        /// <summary>
        /// The m feb checked
        /// </summary>
        private bool mFebChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("FebChecked");
            }
        }
        /// <summary>
        /// The m mar checked
        /// </summary>
        private bool mMarChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("MarChecked");
            }
        }
        /// <summary>
        /// The m apr checked
        /// </summary>
        private bool mAprChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("AprChecked");
            }
        }
        /// <summary>
        /// The m may checked
        /// </summary>
        private bool mMayChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("MayChecked");
            }
        }
        /// <summary>
        /// The m jun checked
        /// </summary>
        private bool mJunChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("JunChecked");
            }
        }
        /// <summary>
        /// The m jul checked
        /// </summary>
        private bool mJulChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("JulChecked");
            }
        }
        /// <summary>
        /// The m aug checked
        /// </summary>
        private bool mAugChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("AugChecked");
            }
        }
        /// <summary>
        /// The m sep checked
        /// </summary>
        private bool mSepChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("SepChecked");
            }
        }
        /// <summary>
        /// The m oct checked
        /// </summary>
        private bool mOctChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("OctChecked");
            }
        }
        /// <summary>
        /// The m nov checked
        /// </summary>
        private bool mNovChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("NovChecked");
            }
        }
        /// <summary>
        /// The m decimal checked
        /// </summary>
        private bool mDecChecked;
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("DecChecked");
            }
        }

        /// <summary>
        /// The m Hedge combo list
        /// </summary>
        /// <summary>
        /// The m Hedge combo list
        /// </summary>
        private List<string> mHedgeComboList;
        /// <summary>
        /// Gets or sets the Hedge combo list.
        /// </summary>
        /// <value>
        /// The Hedge combo list.
        /// </value>
        public List<string> HedgeComboList
        {
            get
            {
                return mHedgeComboList;
            }
            set
            {
                mHedgeComboList = value;
                RaisePropertyChanged("HedgeComboList");
            }
        }


        /// <summary>
        /// The m Spread combo list
        /// </summary>
        /// <summary>
        /// The m Spread combo list
        /// </summary>
        private List<string> mSpreadComboList;
        /// <summary>
        /// Gets or sets the Spread combo list.
        /// </summary>
        /// <value>
        /// The Spread combo list.
        /// </value>
        public List<string> SpreadComboList
        {
            get
            {
                return mSpreadComboList;
            }
            set
            {
                mSpreadComboList = value;
                RaisePropertyChanged("SpreadComboList");
            }
        }

        /// <summary>
        /// The m monthly checked
        /// </summary>
        private bool _MonthlyPeriodChecked;

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
                    UpdateView(true);
                }


            }
        }


        private bool mMonthlyChecked = true;
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
                if (mMonthlyChecked)
                {
                    mDailyChecked = false;
                    UpdateView(true);
                }
                RaisePropertyChanged("MonthlyChecked");
            }
        }

        /// <summary>
        /// The m Quaterly checked
        /// </summary>
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
                    UpdateView(true);
                }

            }
        }

        /// <summary>
        /// The m Annualy checked
        /// </summary>
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
                    UpdateView(true);
                }
            }
        }

        /// <summary>
        /// The m Longterm checked
        /// </summary>
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

        /// <summary>
        /// The m maximum dart
        /// </summary>
        private double? mMaxDart;
        /// <summary>
        /// Gets or sets the maximum dart.
        /// </summary>
        /// <value>
        /// The maximum dart.
        /// </value>
        public double? MaxDart
        {
            get
            {
                return mMaxDart;
            }
            set
            {
                mMaxDart = value;
                RaisePropertyChanged("MaxDart");
            }
        }

        /// <summary>
        /// The m minimum dart
        /// </summary>
        private double? mMinDart;
        /// <summary>
        /// Gets or sets the minimum dart.
        /// </summary>
        /// <value>
        /// The minimum dart.
        /// </value>
        public double? MinDart
        {
            get
            {
                return mMinDart;
            }
            set
            {
                mMinDart = value;
                RaisePropertyChanged("MinDart");
            }
        }

        /// <summary>
        /// The m Hedge combo selected value
        /// </summary>
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
                UpdateView();
                if (HedgeComboSelectedValue == "OBL")
                {

                }
                else
                {

                }
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
        /// The m filter day comparison rt checked
        /// </summary>
        private bool mFilterDayComparisonRTChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison rt checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison rt checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonRTChecked
        {
            get { return mFilterDayComparisonRTChecked; }
            set
            {
                mFilterDayComparisonRTChecked = value;
                if (value == true && SpreadComboSelectedValue == "Exclusive")
                {
                    FilterDayComparisonDAChecked = false;
                    FilterDayComparisonCrrChecked = false;
                    FilterDayComparisonDACrrChecked = false;
                    UpdateView();
                }
                RaisePropertyChanged("FilterDayComparisonRTChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    UpdateView();
                }
            }
        }

        /// <summary>
        /// The m filter day comparison da checked
        /// </summary>
        private bool mFilterDayComparisonDAChecked;
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
                    FilterDayComparisonRTChecked = false;
                    FilterDayComparisonCrrChecked = false;
                    FilterDayComparisonDACrrChecked = false;
                    UpdateView();
                }
                RaisePropertyChanged("FilterDayComparisonDAChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    UpdateView();
                }
            }
        }

        /// <summary>
        /// The m filter day comparison Crr checked
        /// </summary>
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
                    UpdateView();
                }
                RaisePropertyChanged("FilterDayComparisonCrrChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    UpdateView();
                }
            }
        }

        /// <summary>
        /// The m filter day comparison daCrr checked
        /// </summary>
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
                    UpdateView();
                }
                RaisePropertyChanged("FilterDayComparisonDACrrChecked");
                if (value == true || SpreadComboSelectedValue != "Exclusive")
                {
                    UpdateView();
                }
            }
        }

        /// <summary>
        /// The show summary total
        /// </summary>
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

        /// <summary>
        /// The show summary average
        /// </summary>
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
        /// The show summary minimum
        /// </summary>
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

        /// <summary>
        /// The m daily pivot list
        /// </summary>
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

        /// <summary>
        /// The summary pivot list
        /// </summary>
        private ObservableCollection<DailyPivotData> summaryPivotList;
        /// <summary>
        /// Gets or sets the daily summary pivot list.
        /// </summary>
        /// <value>
        /// The daily summary pivot list.
        /// </value>
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
                InitializeSourceSinkStateAndRetrive(false);
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
                InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("EndDate");
            }
        }
        /// <summary>
        /// The m source node list
        /// </summary>
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
                //isTF = true;
                conten = "PEAKWD";

                SetSourceSink();
                RaisePropertyChanged("MarketComboSelectedValue");
            }
        }
        private string mconten;
        public string conten
        {
            get
            {
                return mconten;
            }
            set
            {

                mconten = value;
                RaisePropertyChanged("conten");
            }
        }


        /// <summary>
        /// The m sink node list
        /// </summary>
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
        /// <summary>
        /// The m source combo selected item
        /// </summary>
        private PricingNode mSourceComboSelectedItem;
        /// <summary>
        /// Gets or sets the source combo selected item.
        /// </summary>
        /// <value>
        /// The source combo selected item.
        /// </value>
        public PricingNode SourceComboSelectedItem
        {
            get
            {
                return mSourceComboSelectedItem;
            }
            set
            {
                mSourceComboSelectedItem = value;
                RaisePropertyChanged("SourceComboSelectedItem");
            }
        }
        /// <summary>
        /// The m source sink list
        /// </summary>
        private List<SourceSinkState> mSourceSinkList;
        /// <summary>
        /// Gets or sets the source sink list.
        /// </summary>
        /// <value>
        /// The source sink list.
        /// </value>
        public List<SourceSinkState> SourceSinkList
        {
            get
            {
                return mSourceSinkList;
            }
            set
            {
                mSourceSinkList = value;
                RaisePropertyChanged("SourceSinkList");
            }
        }
        /// <summary>
        /// The m source sink data selected
        /// </summary>
        private SourceSinkState mSourceSinkDataSelected;
        /// <summary>
        /// Gets or sets the source sink data selected.
        /// </summary>
        /// <value>
        /// The source sink data selected.
        /// </value>
        public SourceSinkState SourceSinkDataSelected
        {
            get
            {
                return mSourceSinkDataSelected;
            }
            set
            {
                if (mSourceSinkDataSelected == null || !mSourceSinkDataSelected.Equals(value))
                {
                    mSourceSinkDataSelected = value;
                    SelectSourceSinkFetchDataUpdateChart();
                    RaisePropertyChanged("SourceSinkDataSelected");
                    UpdateView();
                }
            }
        }
        /// <summary>
        /// The m sink combo selected item
        /// </summary>
        private PricingNode mSinkComboSelectedItem;
        /// <summary>
        /// Gets or sets the sink combo selected item.
        /// </summary>
        /// <value>
        /// The sink combo selected item.
        /// </value>
        public PricingNode SinkComboSelectedItem
        {
            get
            {
                return mSinkComboSelectedItem;
            }
            set
            {
                mSinkComboSelectedItem = value;
                RaisePropertyChanged("SinkComboSelectedItem");
            }
        }

        /// <summary>
        /// The isTF checked
        /// </summary>
        //private bool misTF = false;
        ///// <summary>
        ///// Gets or sets a value indicating whether [isTF checked].
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if [isTF checked]; otherwise, <c>false</c>.
        ///// </value>
        //public bool isTF
        //{
        //    get
        //    {
        //        return misTF;
        //    }
        //    set
        //    {
        //        misTF = value;
        //        RaisePropertyChanged("isTF");
        //    }
        //}
        /// <summary>
        /// The m on peak checked
        /// </summary>
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
                    UpdateView();
                }
                RaisePropertyChanged("OnPeakChecked");
            }
        }
        /// <summary>
        /// The m off peak checked
        /// </summary>
        private bool mOffPeakChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [off peak checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [off peak checked]; otherwise, <c>false</c>.
        /// </value>
        public bool OffPeakChecked
        {
            get
            {
                return mOffPeakChecked;
            }
            set
            {
                mOffPeakChecked = value;
                if (mOffPeakChecked)
                {
                    mOnPeakChecked = false;
                    mhour24Checked = false;
                    mPeakWE = false;
                    UpdateView();
                }
                RaisePropertyChanged("OffPeakChecked");
            }
        }
        /// <summary>
        /// The mhour24 checked
        /// </summary>
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
                    UpdateView();
                }
                RaisePropertyChanged("Hour24Checked");
            }
        }

        /// <summary>
        /// The m peak we checked
        /// </summary>
        private bool mPeakWE;
        /// <summary>
        /// Gets or sets a value indicating whether [peak We checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [peak We checked]; otherwise, <c>false</c>.
        /// </value>
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
                    UpdateView();
                }
                RaisePropertyChanged("PeakWE");
            }
        }

        /// <summary>
        /// The m locations
        /// </summary>
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
        /// <summary>
        /// The m center loc
        /// </summary>
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
        /// <summary>
        /// The m node path
        /// </summary>
        private Microsoft.Maps.MapControl.WPF.LocationCollection mNodePath;
        /// <summary>
        /// Gets or sets the node path.
        /// </summary>
        /// <value>
        /// The node path.
        /// </value>
        public Microsoft.Maps.MapControl.WPF.LocationCollection NodePath
        {
            get
            {
                return mNodePath;
            }
            set
            {
                if (mNodePath == value)
                {
                    return;
                }
                mNodePath = value;
                RaisePropertyChanged("NodePath");
            }
        }
        private List<string> mProductList;
        /// <summary>
        /// Gets or sets the product list.
        /// </summary>
        /// <value>
        /// The product list.
        /// </value>
        public List<string> ProductList
        {
            get
            {
                return mProductList;
            }
            set
            {
                mProductList = value;
                RaisePropertyChanged("ProductList");
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
                UpdateView();
            }
        }
        private string mTypeComboSelectedValue;
        /// <summary>
        /// Gets or sets the type combo selected value.
        /// </summary>
        /// <value>
        /// The type combo selected value.
        /// </value>
        public string TypeComboSelectedValue
        {
            get
            {
                return mTypeComboSelectedValue;
            }
            set
            {
                mTypeComboSelectedValue = value;
                RaisePropertyChanged("TypeComboSelectedValue");
            }
        }
        private string mMaxText;
        /// <summary>
        /// Gets or sets the maximum text.
        /// </summary>
        /// <value>
        /// The maximum text.
        /// </value>
        public string MaxText
        {
            get
            {
                return mMaxText;
            }
            set
            {
                mMaxText = value;
                RaisePropertyChanged("MaxText");
            }
        }
        /// <summary>
        /// The m minimum text
        /// </summary>
        private string mMinText;
        /// <summary>
        /// Gets or sets the minimum text.
        /// </summary>
        /// <value>
        /// The minimum text.
        /// </value>
        public string MinText
        {
            get
            {
                return mMinText;
            }
            set
            {
                mMinText = value;
                RaisePropertyChanged("MinText");
            }
        }
        private string mProductComboSelectedValue;
        /// <summary>
        /// Gets or sets the product combo selected value.
        /// </summary>
        /// <value>
        /// The product combo selected value.
        /// </value>
        public string ProductComboSelectedValue
        {
            get
            {
                return mProductComboSelectedValue;
            }
            set
            {
                mProductComboSelectedValue = value;
                RaisePropertyChanged("ProductComboSelectedValue");
                SetTypes();
            }
        }
        private List<string> mTypeList;
        /// <summary>
        /// Gets or sets the type list.
        /// </summary>
        /// <value>
        /// The type list.
        /// </value>
        public List<string> TypeList
        {
            get
            {
                return mTypeList;
            }
            set
            {
                mTypeList = value;
                RaisePropertyChanged("TypeList");
            }
        }

        private FilterData mFilterDataSelected;
        /// <summary>
        /// Gets or sets the filter data selected.
        /// </summary>
        /// <value>
        /// The filter data selected.
        /// </value>
        public FilterData FilterDataSelected
        {
            get
            {
                return mFilterDataSelected;
            }
            set
            {
                mFilterDataSelected = value;
            }
        }

        #region Relay Command Properties

        /// <summary>
        /// Gets or sets the add source sink command.
        /// </summary>
        /// <value>
        /// The add source sink command.
        /// </value>
        public DelegateCommand AddSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the clear all months command.
        /// </summary>
        /// <value>
        /// The clear all months command.
        /// </value>
        public DelegateCommand ClearAllMonthsCommand { private set; get; }
        /// <summary>
        /// Gets or sets the refresh command.
        /// </summary>
        /// <value>
        /// The refresh command.
        /// </value>
        public DelegateCommand RefreshCmd { private set; get; }
        /// <summary>
        /// Gets or sets all command.
        /// </summary>
        /// <value>
        /// All command.
        /// </value>
        public DelegateCommand AllCmd { private set; get; }
        /// <summary>
        /// Gets or sets the spring click command.
        /// </summary>
        /// <value>
        /// The spring click command.
        /// </value>
        public DelegateCommand SpringClickCommand { private set; get; }
        /// <summary>
        /// Gets or sets the summer click command.
        /// </summary>
        /// <value>
        /// The summer click command.
        /// </value>
        public DelegateCommand SummerClickCommand { private set; get; }
        /// <summary>
        /// Gets or sets the winter click command.
        /// </summary>
        /// <value>
        /// The winter click command.
        /// </value>
        public DelegateCommand WinterClickCommand { private set; get; }
        /// <summary>
        /// Gets or sets the fall click command.
        /// </summary>
        /// <value>
        /// The fall click command.
        /// </value>
        public DelegateCommand FallClickCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run retrieve fetch data and update chart command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data and update chart command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataAndUpdateChartCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove source sink command.
        /// </summary>
        /// <value>
        /// The remove source sink command.
        /// </value>
        public DelegateCommand RemoveSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove all source sink command.
        /// </summary>
        /// <value>
        /// The remove all source sink command.
        /// </value>
        public DelegateCommand RemoveAllSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public DelegateCommand ResetCommand { private set; get; }
        /// <summary>
        /// Gets or sets the swap command.
        /// </summary>
        /// <value>
        /// The swap command.
        /// </value>
        public DelegateCommand SwapCommand { private set; get; }


        public DelegateCommand PasteCommand { private set; get; }
        public DelegateCommand LoadCommand { private set; get; }

        public DelegateCommand AddFilterCommand { private set; get; }
        public DelegateCommand RemoveFilterCommand { private set; get; }
        public DelegateCommand RemoveAllFilterCommand { private set; get; }
        public DelegateCommand ExportButtonCommand { private set; get; }

        private bool misPNLChecked;

        public bool isPNLChecked
        {
            get { return misPNLChecked; }
            set
            {
                misPNLChecked = value;
                RetrieveFetchDataAndUpdateChartCommand();
                RaisePropertyChanged("isPNLChecked");


            }
        }
        private bool misCrrChecked;

        public bool isCrrChecked
        {
            get { return misCrrChecked; }
            set
            {
                misCrrChecked = value;
                RetrieveFetchDataAndUpdateChartCommand();

                RaisePropertyChanged("isCrrChecked");

            }
        }
        private bool misDAChecked;

        public bool isDAChecked
        {
            get { return misDAChecked; }
            set
            {
                misDAChecked = value;
                RetrieveFetchDataAndUpdateChartCommand();
                RaisePropertyChanged("isDAChecked");


            }
        }
        private bool misRTChecked;

        public bool isRTChecked
        {
            get { return misRTChecked; }
            set
            {
                misRTChecked = value;
                RetrieveFetchDataAndUpdateChartCommand();
                RaisePropertyChanged("isRTChecked");
            }
        }



        #endregion

        #endregion


        public MainWindowViewModel(IDataService dataService)
        {
            myDataService = dataService;
            myDataService.LoadDBCommands();
            MonthlyPeriodChecked = true;
            mFillSourceSinkHash = new Dictionary<string, SourceSinkState>();
            AddSourceSinkCommand = new DelegateCommand(AddSourceSink);
            ClearAllMonthsCommand = new DelegateCommand(ClearAllMonths);
            RefreshCmd = new DelegateCommand(RefreshMonths);
            FallClickCommand = new DelegateCommand(EnableFallMonths);
            SummerClickCommand = new DelegateCommand(EnableSummerMonths);
            WinterClickCommand = new DelegateCommand(EnableWinterMonths);
            SpringClickCommand = new DelegateCommand(EnableSpringMonths);
            AllCmd = new DelegateCommand(EnableAllMonths);
            ExportButtonCommand = new DelegateCommand(ExportButton);
            RunRetrieveFetchDataAndUpdateChartCommand = new DelegateCommand(RetrieveFetchDataAndUpdateChartCommand);
            RemoveSourceSinkCommand = new DelegateCommand(RemoveSourceSink);
            RemoveAllSourceSinkCommand = new DelegateCommand(RemoveAllSourceSink);
            ResetCommand = new DelegateCommand(RemoveAllSourceSink);
            SwapCommand = new DelegateCommand(Swap);

            PasteCommand = new DelegateCommand(Paste);
            LoadCommand = new DelegateCommand(LoadCrrData);

            // ISOMarketList = new List<string> { "PJM", "MISO", "CAISO", "SPP" };
            ISOMarketList = new List<string> { "ERCOT" };
            MarketComboSelectedValue = "ERCOT";

            DateTime tempDate = DateTime.Today.AddYears(-1).AddMonths(1);
            StartDate = DateTime.Parse(tempDate.Month + "/1/" + tempDate.Year);
            tempDate = DateTime.Today;
            //EndDate = DateTime.Parse(tempDate.Month + "/1/" + tempDate.Year);
            EndDate = tempDate;
            List<string> productList = new List<string> { "Price" };
            ProductList = productList;
            SetSpreadComboBox();
            SetHedgeComboBox();
            EnableAllMonths();
            allMonthsSelected = true;
            if (System.Diagnostics.Debugger.IsAttached) // for quicker testing in debug
            {
                SourceComboSelectedItem = SourceNodeList.FirstOrDefault(x => x.NodeName == "GRDA_HUB");
                SinkComboSelectedItem = SinkNodeList.FirstOrDefault(x => x.NodeName == "KMEA_EMP1_KCPL");
            }
            AddFilterCommand = new DelegateCommand(AddFilters);
            RemoveAllFilterCommand = new DelegateCommand(RemoveAllFilters);
            RemoveFilterCommand = new DelegateCommand(RemoveFilters);
            colSeries1 = new BarSeries();
            CrrLineSeries = new LineSeries();
            daLineSeries = new LineSeries();
            rtLineSeries = new LineSeries();

            isDAChecked = true;
            isRTChecked = true;
            isPNLChecked = true;
            isCrrChecked = true;

        }
        private void LoadCrrData()
        {
            if ((SourceSinkList == null || SourceSinkList.Count == 0))
            {
                MessageBox.Show("Please paste paths.", "Paste Paths");
            }
            else
            {
                foreach (SourceSinkState itemsourcesink in SourceSinkList)
                {
                    SourceSinkState sourcesink = new SourceSinkState();
                    sourcesink.Source = itemsourcesink.Source;
                    sourcesink.Sink = itemsourcesink.Sink;
                    SourceSinkDataSelected = sourcesink;
                    UpdateView();
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Swaps Sink and Source.
        /// </summary>
        private void Swap()
        {
            SourceComboSelectedItem = SourceSinkDataSelected.Source;
            SinkComboSelectedItem = SourceSinkDataSelected.Sink;
            AddPath(true);

        }

        /// <summary>
        /// Fills Hedge ComboBox.
        /// </summary>
        private void SetHedgeComboBox()
        {
            if (HedgeComboList != null)
            {
                return;
            }
            HedgeComboList = null;
            HedgeComboList = new List<string>();
            HedgeComboList.Add("OBL");
            HedgeComboList.Add("OPT");
            HedgeComboSelectedValue = null;
            HedgeComboSelectedValue = "OBL";
        }

        /// <summary>
        /// Fills spread ComboBox.
        /// </summary>
        private void SetSpreadComboBox()
        {
            if (SpreadComboList != null)
            {
                return;
            }
            SpreadComboList = null;
            SpreadComboList = new List<string>();
            SpreadComboList.Add("Exclusive");
            SpreadComboList.Add("Inclusive");
            SpreadComboSelectedValue = null;
            SpreadComboSelectedValue = "Exclusive";
        }

        /// <summary>
        /// Clears some Properties.
        /// </summary>
        private void Clear()
        {
            DailyPivotList = null;
            PlotModelLower = null;
            PlotModelUpper = null;
            DailySummaryPivotList = null;
        }

        /// <summary>
        /// Enables spring months.
        /// </summary>
        private void EnableSpringMonths()
        {
            MarChecked = true;
            AprChecked = true;
            MayChecked = true;
            //
            DecChecked = false;
            JanChecked = false;
            FebChecked = false;
            JunChecked = false;
            JulChecked = false;
            AugChecked = false;
            SepChecked = false;
            OctChecked = false;
            NovChecked = false;
            RefreshMonths();
        }

        /// <summary>
        /// Enables the winter months.
        /// </summary>
        private void EnableWinterMonths()
        {
            DecChecked = true;
            JanChecked = true;
            FebChecked = true;
            //
            MarChecked = false;
            AprChecked = false;
            MayChecked = false;
            JunChecked = false;
            JulChecked = false;
            AugChecked = false;
            SepChecked = false;
            OctChecked = false;
            NovChecked = false;
            RefreshMonths();
        }

        /// <summary>
        /// Enables the summer months.
        /// </summary>
        private void EnableSummerMonths()
        {
            JunChecked = true;
            JulChecked = true;
            AugChecked = true;
            //
            MarChecked = false;
            AprChecked = false;
            MayChecked = false;
            JanChecked = false;
            FebChecked = false;
            DecChecked = false;
            SepChecked = false;
            OctChecked = false;
            NovChecked = false;
            RefreshMonths();
        }

        /// <summary>
        /// Enables the fall months.
        /// </summary>
        private void EnableFallMonths()
        {
            SepChecked = true;
            OctChecked = true;
            NovChecked = true;
            //
            MarChecked = false;
            AprChecked = false;
            MayChecked = false;
            JanChecked = false;
            FebChecked = false;
            DecChecked = false;
            JunChecked = false;
            JulChecked = false;
            AugChecked = false;
            RefreshMonths();
        }

        /// <summary>
        /// Refreshes the months.
        /// </summary>
        private void RefreshMonths()
        {
            if (SourceSinkDataSelected != null)
            {
                allMonthsSelected = false;
                SetDailyPivotList();
            }
        }

        /// <summary>
        /// Enables all months.
        /// </summary>
        /// 
        public void ExportButton()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        }
        private void ExportToCSVThreaded()
        {
            if (DailyPivotList == null)
            {
                Mouse.OverrideCursor = null;
                return;
            }
            if (DailyPivotList == null || DailyPivotList.Count == 0)
            {
                System.Windows.MessageBox.Show("No data to export to");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
            dialog.FileName = "Crr_Path_Analyzer_DayComparison_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    if (DailyPivotList != null && DailyPivotList.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (var item in DailyPivotList[0].GetType().GetProperties())
                        {
                            if (item.Name == "DateDisplay" || item.Name == "Month" || item.Name == "RowDisplayType" || item.Name == "RowName" || item.Name == "Average" || item.Name == "Total"
                                || item.Name == "D1" || item.Name == "D2" || item.Name == "D3" || item.Name == "D4" || item.Name == "D5"
                                || item.Name == "D6" || item.Name == "D7" || item.Name == "D8" || item.Name == "D9" || item.Name == "D10" || item.Name == "D11"
                                || item.Name == "D12" || item.Name == "D13" || item.Name == "D14" || item.Name == "D15" || item.Name == "D16" || item.Name == "D17"
                                || item.Name == "D18" || item.Name == "D19" || item.Name == "D20" || item.Name == "D21" || item.Name == "D22" || item.Name == "D23" || item.Name == "D24"
                                || item.Name == "D25" || item.Name == "D26" || item.Name == "D27" || item.Name == "D28" || item.Name == "D29" || item.Name == "D30" || item.Name == "D31")
                            {
                                builder.Append(item.Name + ",");
                            }
                        }
                        builder.AppendLine();
                        foreach (var item in DailyPivotList)
                        {
                            foreach (var propItem in item.GetType().GetProperties())
                            {
                                if (propItem.Name == "DateDisplay" || propItem.Name == "DateDisplay.Month" || propItem.Name == "RowDisplayType" || propItem.Name == "RowName" || propItem.Name == "Average" || propItem.Name == "Total"
                                || propItem.Name == "D1" || propItem.Name == "D2" || propItem.Name == "D3" || propItem.Name == "D4" || propItem.Name == "D5"
                                || propItem.Name == "D6" || propItem.Name == "D7" || propItem.Name == "D8" || propItem.Name == "D9" || propItem.Name == "D10" || propItem.Name == "D11"
                                || propItem.Name == "D12" || propItem.Name == "D13" || propItem.Name == "D14" || propItem.Name == "D15" || propItem.Name == "D16" || propItem.Name == "D17"
                                || propItem.Name == "D18" || propItem.Name == "D19" || propItem.Name == "D20" || propItem.Name == "D21" || propItem.Name == "D22" || propItem.Name == "D23" || propItem.Name == "D24"
                                || propItem.Name == "D25" || propItem.Name == "D26" || propItem.Name == "D27" || propItem.Name == "D28" || propItem.Name == "D29" || propItem.Name == "D30" || propItem.Name == "D31")

                                {
                                    if (propItem.Name == "D1" || propItem.Name == "D2" || propItem.Name == "D3" || propItem.Name == "D4" || propItem.Name == "D5"
                                    || propItem.Name == "D6" || propItem.Name == "D7" || propItem.Name == "D8" || propItem.Name == "D9" || propItem.Name == "D10" || propItem.Name == "D11"
                                    || propItem.Name == "D12" || propItem.Name == "D13" || propItem.Name == "D14" || propItem.Name == "D15" || propItem.Name == "D16" || propItem.Name == "D17"
                                    || propItem.Name == "D18" || propItem.Name == "D19" || propItem.Name == "D20" || propItem.Name == "D21" || propItem.Name == "D22" || propItem.Name == "D23" || propItem.Name == "D24"
                                    || propItem.Name == "D25" || propItem.Name == "D26" || propItem.Name == "D27" || propItem.Name == "D28" || propItem.Name == "D29" || propItem.Name == "D30" || propItem.Name == "D31")

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
        private void EnableAllMonths()
        {
            JanChecked = true;
            FebChecked = true;
            MarChecked = true;
            AprChecked = true;
            MayChecked = true;
            JunChecked = true;
            JulChecked = true;
            AugChecked = true;
            SepChecked = true;
            OctChecked = true;
            NovChecked = true;
            DecChecked = true;
            allMonthsSelected = true;
            RefreshMonths();
        }

        /// <summary>
        /// Clears all months.
        /// </summary>
        private void ClearAllMonths()
        {
            JanChecked = false;
            FebChecked = false;
            MarChecked = false;
            AprChecked = false;
            MayChecked = false;
            JunChecked = false;
            JulChecked = false;
            AugChecked = false;
            SepChecked = false;
            OctChecked = false;
            NovChecked = false;
            DecChecked = false;
            allMonthsSelected = false;
            RefreshMonths();
        }

        /// <summary>
        /// Removes the source sink.
        /// </summary>
        public void RemoveSourceSink()
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            string sourceSinkKey = "";
            if (sourceSinkData == null)
            {
                return;
            }
            sourceSinkKey = sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
            int index = 0;
            foreach (SourceSinkData sourceSink in mSourceSinkList)
            {
                if (sourceSink.Source.NodeKey == sourceSinkData.Source.NodeKey && sourceSink.Sink.NodeKey == sourceSinkData.Sink.NodeKey)
                {
                    break;
                }
                index++;
            }
            if (mSourceSinkList.Count > 0 && index < mSourceSinkList.Count)
            {
                mSourceSinkList.RemoveAt(index);
            }
            if (mFillSourceSinkHash.ContainsKey(sourceSinkKey))
            {
                mFillSourceSinkHash.Remove(sourceSinkKey);
            }
            SourceSinkList = null;
            SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkState>();
            Clear();
            RemoveAllFilters();
        }

        /// <summary>
        /// Removes all source sink.
        /// </summary>
        public void RemoveAllSourceSink()
        {
            mSourceSinkDataList = new List<SourceSinkData>();
            mFillSourceSinkHash = new Dictionary<string, SourceSinkState>();
            SourceSinkList = null;
            Clear();
            RemoveAllFilters();
        }


        /// <summary>
        /// Gets the monthly priod key.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        private int GetMonthlyPriodKey(DateTime period)
        {
            //DateTime start = new DateTime(period.Year, period.Month, 1);
            //DateTime end = start.AddMonths(1).AddDays(-1);
            //CRRGraphs.Model.DataService service = new CRRGraphs.Model.DataService();
            //SqlCommand cmd = service.GetCommand(CRRGraphs.Model.DB.TradingData);
            //cmd.CommandText = "select top 1 PeriodKey from Period where MarketKey = 1 and PeriodType = 'Monthly' and StartDate = '" + start.ToString("yyyy-MM-dd") + "' and EndDate = '" + end.ToString("yyyy-MM-dd") + "'";
            //cmd.Connection.Open();
            //object id = cmd.ExecuteScalar();
            //cmd.Connection.Close();
            //int intID = (int)id;
            //return intID
            return 0;
        }

        /// <summary>
        /// Creates lower model.
        /// </summary>
        /// <param name="consolidatedList">The consolidated list.</param>
        /// <returns></returns>
        private PlotModel CreateModelLower(List<ConsolidatedData> consolidatedList)
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
                Angle = 0, /* 90 */
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
            categoryAxis.MajorStep = 8;
            plotModel1.Axes.Add(categoryAxis);
            //plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 2);
            if (DailyChecked)
            {
                foreach (var item in consolidatedList)
                {
                    categoryAxis.Labels.Add(item.StartDate.ToString("M/d/yy"));
                }
            }
            else

                foreach (var item in consolidatedList)
                {
                    categoryAxis.Labels.Add(item.StartDate.ToString("M/d/yy"));
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
                TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}",
                XAxisKey = "XAxisBCategory",
                YAxisKey = "Y2AxisB",
            };
            var data = new Collection<DateValue>();
            double? accumulator = 0;
            foreach (var item in consolidatedList.OrderByDescending(x => x.StartDate).ToList())
            {
                try
                {
                    if (item.DACrrValue != null)
                    {
                        accumulator += item.DACrrValue.Value;
                    }
                    areaSeries1.Points.Add(new DataPoint(item.Index, accumulator.Value));
                    areaSeries1.Points2.Add(new DataPoint(item.Index, 0));
                }
                catch (Exception)
                {

                }
            }
            string title = SourceSinkDataSelected.Source.NodeName + " -> " + SourceSinkDataSelected.Sink.NodeName;
            areaSeries1.Title = title + "  sum";
            plotModel1.Series.Add(areaSeries1);
            var l = new Legend
            {
                LegendPosition = LegendPosition.TopLeft,
                LegendPlacement = LegendPlacement.Inside

            };
            plotModel1.Legends.Add(l);
            plotModel1.IsLegendVisible = true;


            return plotModel1;
        }

        /// <summary>
        /// Adds the path.
        /// </summary>
        /// <param name="isSwaped">if set to <c>true</c> [is swaped].</param>
        private void AddPath(bool isSwaped)
        {
            if (SourceComboSelectedItem != null && SinkComboSelectedItem != null)
            {
                SourceSinkState sourceSinkData = new SourceSinkState();
                sourceSinkData.Source = SourceComboSelectedItem;
                sourceSinkData.Sink = SinkComboSelectedItem;
                string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                            sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();

                if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                    SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkState>();
                    mSourceSinkDataSelected = sourceSinkData;
                    RaisePropertyChanged("SourceSinkDataSelected");
                }

                SourceComboSelectedItem = null;
                SinkComboSelectedItem = null;
                InitializeSourceSinkStateAndRetrive(isSwaped);

            }
        }

        /// <summary>
        /// Sets the daily pivot list.
        /// </summary>
        public ObservableCollection<DailyPivotData> GetCrrServiceData()
        {
            Connect();

            ObservableCollection<DailyPivotData> mDailyPivotDataList = new ObservableCollection<DailyPivotData>();

            if (MarketComboSelectedValue == "ERCOT")
            {
                if (QuaterlyChecked)
                {
                    if (JunChecked && JulChecked && AugChecked && !JanChecked && !FebChecked && !MarChecked && !AprChecked && !MayChecked && !SepChecked && !OctChecked && !NovChecked && !DecChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(9, SourceSinkDataSelected.Source.NodeKey, SourceSinkDataSelected.Sink.NodeKey, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 6, "Quaterly");
                    else if (SepChecked && OctChecked && NovChecked && !JunChecked && !JulChecked && !AugChecked && !DecChecked && !JanChecked && !FebChecked && !MarChecked && !AprChecked && !MayChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(9, SourceSinkDataSelected.Source.NodeKey, SourceSinkDataSelected.Sink.NodeKey, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 9, "Quaterly");
                    else if (DecChecked && JanChecked && FebChecked && !SepChecked && !OctChecked && !NovChecked && !JunChecked && !JulChecked && !AugChecked && !MarChecked && !AprChecked && !MayChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(9, SourceSinkDataSelected.Source.NodeKey, SourceSinkDataSelected.Sink.NodeKey, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 12, "Quaterly");
                    else if (MarChecked && AprChecked && MayChecked && !SepChecked && !OctChecked && !NovChecked && !JunChecked && !JulChecked && !AugChecked && !DecChecked && !JanChecked && !FebChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(9, SourceSinkDataSelected.Source.NodeKey, SourceSinkDataSelected.Sink.NodeKey, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 3, "Quaterly");
                    //  else
                    //    MessageBox.Show("Please seelect Particular Seasom To View Quarterly Results");
                }
                else if (AnnualyChecked)
                {
                    yearHash = CrrCalculationLibrary.FillFTRPathData(9, SourceSinkDataSelected.Source.NodeKey, SourceSinkDataSelected.Sink.NodeKey, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 0, "Annual");
                }
                else
                {

                    yearHash = CrrCalculationLibrary.FillFTRPathData(9, (long)SourceSinkDataSelected.Source.NodeKey, (long)SourceSinkDataSelected.Sink.NodeKey, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 0, "Monthly");
                }

            }
            else
            {
                if (QuaterlyChecked)
                {
                    if (JunChecked && JulChecked && AugChecked && !JanChecked && !FebChecked && !MarChecked && !AprChecked && !MayChecked && !SepChecked && !OctChecked && !NovChecked && !DecChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(1, SourceSinkDataSelected.Source.ExternalNodeId, SourceSinkDataSelected.Sink.ExternalNodeId, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 6, "Quaterly");
                    else if (SepChecked && OctChecked && NovChecked && !JunChecked && !JulChecked && !AugChecked && !DecChecked && !JanChecked && !FebChecked && !MarChecked && !AprChecked && !MayChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(1, SourceSinkDataSelected.Source.ExternalNodeId, SourceSinkDataSelected.Sink.ExternalNodeId, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 9, "Quaterly");
                    else if (DecChecked && JanChecked && FebChecked && !SepChecked && !OctChecked && !NovChecked && !JunChecked && !JulChecked && !AugChecked && !MarChecked && !AprChecked && !MayChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(1, SourceSinkDataSelected.Source.ExternalNodeId, SourceSinkDataSelected.Sink.ExternalNodeId, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 12, "Quaterly");
                    else if (MarChecked && AprChecked && MayChecked && !SepChecked && !OctChecked && !NovChecked && !JunChecked && !JulChecked && !AugChecked && !DecChecked && !JanChecked && !FebChecked)
                        yearHash = CrrCalculationLibrary.FillFTRPathData(1, SourceSinkDataSelected.Source.ExternalNodeId, SourceSinkDataSelected.Sink.ExternalNodeId, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 3, "Quaterly");
                    //  else
                    //    MessageBox.Show("Please seelect Particular Seasom To View Quarterly Results");
                }
                else if (AnnualyChecked)
                {
                    yearHash = CrrCalculationLibrary.FillFTRPathData(1, SourceSinkDataSelected.Source.ExternalNodeId, SourceSinkDataSelected.Sink.ExternalNodeId, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 0, "Annual");
                }
                else
                {//                                                 /int Marketkey, long SourceId, long SinkId,                                                            DateTime startDate, DateTime endDate,string periodtype, string hedgetype,  int CrrQuarterMonth = 0, string auctionType = "Monthly"
                    yearHash = CrrCalculationLibrary.FillFTRPathData(1, (long)SourceSinkDataSelected.Source.ExternalNodeId, (long)SourceSinkDataSelected.Sink.ExternalNodeId, StartDate, EndDate, HedgeComboSelectedValue, HedgeComboSelectedValue, 0, "Monthly");
                }
            }

            List<int> yearlist = yearHash.Keys.ToList();
            Type typ = typeof(DailyPivotData);
            mGraphList = new List<ConsolidatedData>();
            int i = 0;
            Dictionary<DateTime, string> dictPeakYn = CrrCalculationLibrary.GetPeakYN_daterange(StartDate, EndDate);
            foreach (int year in yearlist)
            {
                Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>> monthHash = yearHash[year];

                #region MonthHash
                if (!JanChecked)
                {
                    if (monthHash.ContainsKey(1))
                        monthHash.Remove(1);
                }
                if (!FebChecked)
                {
                    if (monthHash.ContainsKey(2))
                        monthHash.Remove(2);
                }
                if (!MarChecked)
                {
                    if (monthHash.ContainsKey(3))
                        monthHash.Remove(3);
                }
                if (!AprChecked)
                {
                    if (monthHash.ContainsKey(4))
                        monthHash.Remove(4);
                }

                if (!MayChecked)
                {
                    if (monthHash.ContainsKey(5))
                        monthHash.Remove(5);
                }
                if (!JunChecked)
                {
                    if (monthHash.ContainsKey(6))
                        monthHash.Remove(6);
                }
                if (!JulChecked)
                {
                    if (monthHash.ContainsKey(7))
                        monthHash.Remove(7);
                }
                if (!AugChecked)
                {
                    if (monthHash.ContainsKey(8))
                        monthHash.Remove(8);
                }

                if (!SepChecked)
                {
                    if (monthHash.ContainsKey(9))
                        monthHash.Remove(9);
                }
                if (!OctChecked)
                {
                    if (monthHash.ContainsKey(10))
                        monthHash.Remove(10);
                }
                if (!NovChecked)
                {
                    if (monthHash.ContainsKey(11))
                        monthHash.Remove(11);
                }
                if (!DecChecked)
                {
                    if (monthHash.ContainsKey(12))
                        monthHash.Remove(12);
                }
                List<int> monthlist = monthHash.Keys.ToList();

                #endregion

                foreach (int month in monthlist)
                {
                    DailyPivotData mPeakDailyPivotDataRT = new DailyPivotData();
                    DailyPivotData mPeakDailyPivotDataDA = new DailyPivotData();
                    DailyPivotData mPeakDailyPivotDataCrr = new DailyPivotData();
                    DailyPivotData mPeakDailyPivotDataDACrr = new DailyPivotData();

                    DailyPivotData mOffPeakDailyPivotDataRT = new DailyPivotData();
                    DailyPivotData mOffPeakDailyPivotDataDA = new DailyPivotData();
                    DailyPivotData mOffPeakDailyPivotDataCrr = new DailyPivotData();
                    DailyPivotData mOffPeakDailyPivotDataDACrr = new DailyPivotData();

                    DailyPivotData m24DailyPivotDataRT = new DailyPivotData();
                    DailyPivotData m24DailyPivotDataDA = new DailyPivotData();
                    DailyPivotData m24DailyPivotDataCrr = new DailyPivotData();
                    DailyPivotData m24DailyPivotDataDACrr = new DailyPivotData();

                    DailyPivotData mPeakWEDailyPivotDataRT = new DailyPivotData();
                    DailyPivotData mPeakWEDailyPivotDataDA = new DailyPivotData();
                    DailyPivotData mPeakWEDailyPivotDataCrr = new DailyPivotData();
                    DailyPivotData mPeakWEDailyPivotDataDACrr = new DailyPivotData();

                    DailyValues priceValues = null;
                    if (monthHash.ContainsKey(month))
                        priceValues = monthHash[month].Item1;
                    Dictionary<int, DailyValues> dayHash = monthHash[month].Item2;
                    mPeakDailyPivotDataRT.DateDisplay = new DateTime(year, month, 01);
                    mPeakDailyPivotDataDA.DateDisplay = new DateTime(year, month, 01);
                    mPeakDailyPivotDataCrr.DateDisplay = new DateTime(year, month, 01);
                    mPeakDailyPivotDataDACrr.DateDisplay = new DateTime(year, month, 01);

                    mOffPeakDailyPivotDataRT.DateDisplay = new DateTime(year, month, 01);
                    mOffPeakDailyPivotDataDA.DateDisplay = new DateTime(year, month, 01);
                    mOffPeakDailyPivotDataCrr.DateDisplay = new DateTime(year, month, 01);
                    mOffPeakDailyPivotDataDACrr.DateDisplay = new DateTime(year, month, 01);

                    m24DailyPivotDataRT.DateDisplay = new DateTime(year, month, 01);
                    m24DailyPivotDataDA.DateDisplay = new DateTime(year, month, 01);
                    m24DailyPivotDataCrr.DateDisplay = new DateTime(year, month, 01);
                    m24DailyPivotDataDACrr.DateDisplay = new DateTime(year, month, 01);

                    mPeakWEDailyPivotDataRT.DateDisplay = new DateTime(year, month, 01);
                    mPeakWEDailyPivotDataDA.DateDisplay = new DateTime(year, month, 01);
                    mPeakWEDailyPivotDataCrr.DateDisplay = new DateTime(year, month, 01);
                    mPeakWEDailyPivotDataDACrr.DateDisplay = new DateTime(year, month, 01);

                    List<int> dayslist = dayHash.Keys.ToList();
                    double? totalPeakrt = 0;
                    double? totalPeakda = 0;
                    double? totalPeakCrr = 0;
                    double? totalPeakdaCrr = 0;

                    double? totalOffPeakkrt = 0;
                    double? totalOffPeakda = 0;
                    double? totalOffPeakCrr = 0;
                    double? totalOffPeakdaCrr = 0;

                    double? total24krt = 0;
                    double? total24da = 0;
                    double? total24Crr = 0;
                    double? total24daCrr = 0;

                    double? dRTsumValue = null;
                    double? dDAsumValue = null;
                    double? dCrrsumValue = null;
                    double? dDACrrsumValue = null;

                    double? doffRTsumValue = null;
                    double? doffDAsumValue = null;
                    double? doffCrrsumValue = null;
                    double? doffDACrrsumValue = null;

                    double? d24RTsumValue = null;
                    double? d24DAsumValue = null;
                    double? d24CrrsumValue = null;
                    double? d24DACrrsumValue = null;
                    int peakhrs = 0;
                    int offpeak = 0;
                    int hr24 = 0;
                    double davalue = 0;
                    double rtvalue = 0;
                    double Crrvalue = 0;
                    double daCrrvalue = 0;
                    ConsolidatedData mConsolidatedData = new ConsolidatedData();
                    mConsolidatedData.StartDate = new DateTime(year, month, 01);
                    foreach (int days in dayslist)
                    {

                        int d = 0;
                        if (days == 0)
                        {
                            d = days + 1;
                        }
                        else
                        {
                            d = days;
                        }

                        DateTime curdate = Convert.ToDateTime(year + "/" + month + "/" + d);
                        DayOfWeek satsunday11 = curdate.DayOfWeek;
                        string Weekendays = satsunday11.ToString();

                        double? dRTValue = null;
                        double? dDAValue = null;
                        double? dCrrValue = null;
                        double? dDACrrValue = null;

                        double? doffRTValue = null;
                        double? doffDAValue = null;
                        double? doffCrrValue = null;
                        double? doffDACrrValue = null;

                        double? d24RTValue = null;
                        double? d24DAValue = null;
                        double? d24CrrValue = null;
                        double? d24DACrrValue = null;
                        DailyValues congValues = dayHash[days];
                        PropertyInfo infoPeakRT = typ.GetProperty("D" + days);
                        PropertyInfo infoPeakDA = typ.GetProperty("D" + days);
                        PropertyInfo infoPeakCrr = typ.GetProperty("D" + days);
                        PropertyInfo infoPeakDACrr = typ.GetProperty("D" + days);

                        PropertyInfo infoOffPeakRT = typ.GetProperty("D" + days);
                        PropertyInfo infoOffPeakDA = typ.GetProperty("D" + days);
                        PropertyInfo infoOffPeakCrr = typ.GetProperty("D" + days);
                        PropertyInfo infoOffPeakDACrr = typ.GetProperty("D" + days);

                        PropertyInfo info24RT = typ.GetProperty("D" + days);
                        PropertyInfo info24DA = typ.GetProperty("D" + days);
                        PropertyInfo info24Crr = typ.GetProperty("D" + days);
                        PropertyInfo info24DACrr = typ.GetProperty("D" + days);

                        if (MarketComboSelectedValue == "ERCOT")
                        {
                            if (HedgeComboSelectedValue == "OBL")
                            {
                                if (OnPeakChecked == true)
                                {
                                    if (Weekendays.ToLower() == Convert.ToString("saturday") || Weekendays.ToLower() == Convert.ToString("sunday"))
                                    {

                                    }
                                    else
                                    {
                                        try
                                        {
                                            peakhrs = peakhrs + congValues.PeakHours;

                                            dDAValue = congValues.DAPeakCong;
                                            dDAsumValue = dDAValue * congValues.PeakHours;
                                            infoPeakDA.SetValue(mPeakDailyPivotDataDA, dDAValue);

                                            dRTValue = congValues.RTPeakCong;
                                            dRTsumValue = dRTValue * congValues.PeakHours;
                                            infoPeakRT.SetValue(mPeakDailyPivotDataRT, dRTValue);

                                            dCrrValue = priceValues.PricePeak;

                                            dCrrsumValue = dCrrValue * congValues.PeakHours;
                                            infoPeakCrr.SetValue(mPeakDailyPivotDataCrr, dCrrValue);

                                            dDACrrValue = (congValues.DAPeakCong) - (dCrrValue);
                                            dDACrrsumValue = dDACrrValue * congValues.PeakHours;
                                            infoPeakDACrr.SetValue(mPeakDailyPivotDataDACrr, dDACrrValue);
                                            //
                                            if (OnPeakChecked)
                                            {
                                                if (DailyChecked)
                                                {
                                                    ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                    mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                    mConsolidatedDataDaily.Index = i++;
                                                    mConsolidatedDataDaily.DAValue = congValues.DAPeakCong;
                                                    mConsolidatedDataDaily.RTValue = congValues.RTPeakCong;
                                                    mConsolidatedDataDaily.CrrValue = dCrrValue;
                                                    mConsolidatedDataDaily.DACrrValue = (congValues.DAPeakCong) - (dCrrValue);
                                                    mGraphList.Add(mConsolidatedDataDaily);
                                                }
                                            }
                                            if (congValues.DAPeakCong == null)
                                                davalue = davalue + 0;
                                            else
                                                davalue = davalue + ((congValues.DAPeakCong.Value) * congValues.PeakHours);

                                            if (congValues.RTPeakCong == null)
                                                rtvalue = rtvalue + 0;
                                            else
                                                rtvalue = rtvalue + ((congValues.RTPeakCong.Value) * congValues.PeakHours);

                                            if (priceValues.PricePeak == null)
                                                Crrvalue = Crrvalue + 0;
                                            else
                                                Crrvalue = Crrvalue + ((priceValues.PricePeak.Value) * congValues.PeakHours);

                                            if (congValues.DAPeakCong == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (priceValues.PricePeak == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (!((congValues.DAPeakCong == null) || (priceValues.PricePeak == null)))
                                            {
                                                daCrrvalue = daCrrvalue + ((congValues.DAPeakCong.Value) - (priceValues.PricePeak.Value * congValues.PeakHours));
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        if (dRTsumValue == null)
                                        {
                                            dRTsumValue = 0;
                                        }
                                        totalPeakrt = totalPeakrt + dRTsumValue;

                                        if (dDAsumValue == null)
                                        {
                                            dDAsumValue = 0;
                                        }
                                        totalPeakda = totalPeakda + dDAsumValue;

                                        if (dCrrsumValue == null)
                                        {
                                            dCrrsumValue = 0;
                                        }
                                        totalPeakCrr = totalPeakCrr + dCrrsumValue;
                                        if (dDACrrsumValue == null)
                                        {
                                            dDACrrsumValue = 0;
                                        }
                                        totalPeakdaCrr = totalPeakdaCrr + dDACrrsumValue;
                                        if (doffRTsumValue == null)
                                        {
                                            doffRTsumValue = 0;
                                        }


                                    }

                                }
                                else if (OffPeakChecked == true)
                                {

                                    try
                                    {
                                        offpeak = offpeak + congValues.OffPeakHours;

                                        doffDAValue = congValues.DAOffPeakCong;
                                        doffDAsumValue = doffDAValue * congValues.OffPeakHours;
                                        infoOffPeakDA.SetValue(mOffPeakDailyPivotDataDA, doffDAValue);

                                        doffRTValue = congValues.RTOffPeakCong;
                                        doffRTsumValue = doffRTValue * congValues.OffPeakHours;
                                        infoOffPeakRT.SetValue(mOffPeakDailyPivotDataRT, doffRTValue);

                                        doffCrrValue = priceValues.PriceOffPeak;
                                        doffCrrsumValue = doffCrrValue * congValues.OffPeakHours;
                                        infoOffPeakCrr.SetValue(mOffPeakDailyPivotDataCrr, doffCrrValue);

                                        doffDACrrValue = (congValues.DAOffPeakCong) - (priceValues.PriceOffPeak);
                                        doffDACrrsumValue = doffDACrrValue * congValues.OffPeakHours;
                                        infoOffPeakDACrr.SetValue(mOffPeakDailyPivotDataDACrr, doffDACrrValue);
                                        if (OffPeakChecked)
                                        {
                                            if (DailyChecked)
                                            {
                                                ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                mConsolidatedDataDaily.Index = i++;
                                                mConsolidatedDataDaily.DAValue = congValues.DAOffPeakCong;
                                                mConsolidatedDataDaily.RTValue = congValues.RTOffPeakCong;
                                                mConsolidatedDataDaily.CrrValue = priceValues.PriceOffPeak;
                                                mConsolidatedDataDaily.DACrrValue = (congValues.DAOffPeakCong) - (priceValues.PriceOffPeak); ;
                                                mGraphList.Add(mConsolidatedDataDaily);
                                            }
                                        }
                                        if (congValues.DAOffPeakCong == null)
                                            davalue = davalue + 0;
                                        else
                                            davalue = davalue + ((congValues.DAOffPeakCong.Value) * congValues.OffPeakHours);
                                        if (congValues.RTOffPeakCong == null)
                                            rtvalue = rtvalue + 0;
                                        else
                                            rtvalue = rtvalue + ((congValues.RTOffPeakCong.Value) * congValues.PeakHours);

                                        if (priceValues.PriceOffPeak == null)
                                            Crrvalue = Crrvalue + 0;
                                        else
                                            Crrvalue = Crrvalue + ((priceValues.PriceOffPeak.Value) * congValues.OffPeakHours);

                                        if (congValues.DAOffPeakCong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (congValues.DAOffPeakCong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (!((congValues.DAOffPeakCong == null) || (priceValues.PriceOffPeak == null)))
                                        {
                                            daCrrvalue = daCrrvalue + ((((congValues.DAOffPeakCong.Value) - (priceValues.PriceOffPeak.Value))) * congValues.OffPeakHours);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    totalOffPeakkrt = totalOffPeakkrt + doffRTsumValue;

                                    if (doffDAsumValue == null)
                                    {
                                        doffDAsumValue = 0;
                                    }
                                    totalOffPeakda = totalOffPeakda + doffDAsumValue;

                                    if (doffCrrsumValue == null)
                                    {
                                        doffCrrsumValue = 0;
                                    }
                                    totalOffPeakCrr = totalOffPeakCrr + doffCrrsumValue;

                                    if (doffDACrrsumValue == null)
                                    {
                                        doffDACrrsumValue = 0;
                                    }
                                    totalOffPeakdaCrr = totalOffPeakdaCrr + doffDACrrsumValue;

                                }
                                else if (PeakWE == true)
                                {
                                    bool is_holiday = false;
                                    DateTime ncurdate = curdate.AddHours(8);

                                    if (dictPeakYn.ContainsKey(ncurdate))
                                        is_holiday = true;

                                    if (Weekendays.ToLower() == Convert.ToString("saturday") || Weekendays.ToLower() == Convert.ToString("sunday") || is_holiday)
                                    {
                                        try
                                        {
                                            peakhrs = peakhrs + congValues.PeakWEHours;

                                            dDAValue = congValues.DAPeakWECong;
                                            dDAsumValue = dDAValue * congValues.PeakWEHours;
                                            infoPeakDA.SetValue(mPeakWEDailyPivotDataDA, dDAValue);

                                            dRTValue = congValues.RTPeakWECong;
                                            dRTsumValue = dRTValue * congValues.PeakWEHours;
                                            infoPeakRT.SetValue(mPeakWEDailyPivotDataRT, dRTValue);

                                            dCrrValue = priceValues.PriceWE;

                                            dCrrsumValue = dCrrValue * congValues.PeakWEHours;
                                            infoPeakCrr.SetValue(mPeakWEDailyPivotDataCrr, dCrrValue);

                                            dDACrrValue = (congValues.DAPeakWECong) - (dCrrValue);
                                            dDACrrsumValue = dDACrrValue * congValues.PeakWEHours;
                                            infoPeakDACrr.SetValue(mPeakWEDailyPivotDataDACrr, dDACrrValue);
                                            //
                                            if (PeakWE)
                                            {
                                                if (DailyChecked)
                                                {
                                                    ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                    mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                    mConsolidatedDataDaily.Index = i++;
                                                    mConsolidatedDataDaily.DAValue = congValues.DAPeakWECong;
                                                    mConsolidatedDataDaily.RTValue = congValues.RTPeakWECong;
                                                    mConsolidatedDataDaily.CrrValue = dCrrValue;
                                                    mConsolidatedDataDaily.DACrrValue = (congValues.DAPeakWECong) - (dCrrValue);
                                                    mGraphList.Add(mConsolidatedDataDaily);
                                                }
                                            }
                                            if (congValues.DAPeakWECong == null)
                                                davalue = davalue + 0;
                                            else
                                                davalue = davalue + ((congValues.DAPeakWECong.Value) * congValues.PeakWEHours);

                                            if (congValues.RTPeakWECong == null)
                                                rtvalue = rtvalue + 0;
                                            else
                                                rtvalue = rtvalue + ((congValues.RTPeakWECong.Value) * congValues.PeakWEHours);

                                            if (priceValues.PriceWE == null)
                                                Crrvalue = Crrvalue + 0;
                                            else
                                                Crrvalue = Crrvalue + ((priceValues.PriceWE.Value) * congValues.PeakWEHours);

                                            if (congValues.DAPeakWECong == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (priceValues.PriceWE == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (!((congValues.DAPeakWECong == null) || (priceValues.PriceWE == null)))
                                            {
                                                daCrrvalue = daCrrvalue + ((congValues.DAPeakWECong.Value) - (priceValues.PriceWE.Value * congValues.PeakWEHours));
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                        if (dRTsumValue == null)
                                        {
                                            dRTsumValue = 0;
                                        }
                                        totalPeakrt = totalPeakrt + dRTsumValue;

                                        if (dDAsumValue == null)
                                        {
                                            dDAsumValue = 0;
                                        }
                                        totalPeakda = totalPeakda + dDAsumValue;

                                        if (dCrrsumValue == null)
                                        {
                                            dCrrsumValue = 0;
                                        }
                                        totalPeakCrr = totalPeakCrr + dCrrsumValue;
                                        if (dDACrrsumValue == null)
                                        {
                                            dDACrrsumValue = 0;
                                        }
                                        totalPeakdaCrr = totalPeakdaCrr + dDACrrsumValue;
                                        if (doffRTsumValue == null)
                                        {
                                            doffRTsumValue = 0;
                                        }
                                    }

                                }
                                else if (Hour24Checked == true)
                                {

                                    try
                                    {
                                        hr24 = hr24 + (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours);

                                        d24DAValue = congValues.DA24Cong;
                                        d24DAsumValue = d24DAValue * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours);
                                        info24DA.SetValue(m24DailyPivotDataDA, d24DAValue);

                                        d24RTValue = congValues.RT24Cong;
                                        d24RTsumValue = d24RTValue * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours);
                                        info24RT.SetValue(m24DailyPivotDataRT, d24RTValue);
                                        int total24hours = priceValues.PeakHours + priceValues.OffPeakHours + priceValues.PeakWE;
                                        d24CrrValue = ((priceValues.PricePeak * priceValues.PeakHours) + (priceValues.PriceOffPeak * priceValues.OffPeakHours) + (priceValues.PriceWE * priceValues.PeakWE)) / total24hours;
                                        d24CrrsumValue = (d24DAValue - d24CrrValue) * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours);
                                        // d24CrrsumValue = d24CrrValue * total24hours;
                                        info24Crr.SetValue(m24DailyPivotDataCrr, d24CrrValue);
                                        d24DACrrValue = (congValues.DA24Cong) - (priceValues.PricePeak + priceValues.PriceOffPeak + priceValues.PriceWE);
                                        d24DACrrsumValue = d24DACrrValue * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours);
                                        info24DACrr.SetValue(m24DailyPivotDataDACrr, d24DACrrValue);
                                        if (Hour24Checked)
                                        {
                                            if (DailyChecked)
                                            {
                                                ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                mConsolidatedDataDaily.Index = i++;
                                                mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                mConsolidatedDataDaily.DAValue = congValues.DA24Cong;
                                                mConsolidatedDataDaily.RTValue = congValues.RT24Cong;
                                                mConsolidatedDataDaily.CrrValue = (priceValues.PricePeak + priceValues.PriceOffPeak + priceValues.PriceWE);
                                                mConsolidatedDataDaily.DACrrValue = (congValues.DA24Cong) - (priceValues.PricePeak + priceValues.PriceOffPeak + priceValues.PriceWE);
                                                mGraphList.Add(mConsolidatedDataDaily);
                                            }
                                        }

                                        if (congValues.DA24Cong == null)
                                            davalue = davalue + 0;
                                        else
                                            davalue = davalue + ((congValues.DA24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours));
                                        if (congValues.RT24Cong == null)
                                            rtvalue = rtvalue + 0;
                                        else
                                            rtvalue = rtvalue + ((congValues.RT24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours));
                                        if (priceValues.PricePeak == null)
                                            Crrvalue = Crrvalue + 0;
                                        else
                                            Crrvalue = Crrvalue + (((priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value + priceValues.PriceWE.Value) * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours)));

                                        if (congValues.DA24Cong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (!(priceValues.PricePeak == null || priceValues.OffPeakHours == null))
                                        {
                                            if (congValues.DA24Cong != null)
                                            {
                                                daCrrvalue = daCrrvalue + ((((congValues.DA24Cong.Value) - (priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value + priceValues.PriceWE.Value))) * (congValues.PeakHours + congValues.OffPeakHours + congValues.PeakWEHours));
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    if (d24RTsumValue == null)
                                    {
                                        d24RTsumValue = 0;
                                    }
                                    total24krt = total24krt + d24RTsumValue;

                                    if (d24DAsumValue == null)
                                    {
                                        d24DAsumValue = 0;
                                    }
                                    total24da = total24da + d24DAsumValue;

                                    if (d24CrrsumValue == null)
                                    {
                                        d24CrrsumValue = 0;
                                    }
                                    total24Crr = total24Crr + d24CrrsumValue;

                                    if (d24DACrrsumValue == null)
                                    {
                                        d24DACrrsumValue = 0;
                                    }
                                    total24daCrr = total24daCrr + d24DACrrsumValue;

                                }

                            }
                            else
                            {
                                if (OnPeakChecked == true)
                                {
                                    bool is_holiday = false;
                                    DateTime ncurdate = curdate.AddHours(8);

                                    if (dictPeakYn.ContainsKey(ncurdate))
                                        is_holiday = true;

                                    if (Weekendays.ToLower() == Convert.ToString("saturday") || Weekendays.ToLower() == Convert.ToString("sunday") || is_holiday)
                                    {

                                    }
                                    else
                                    {
                                        try
                                        {
                                            peakhrs = peakhrs + congValues.PeakHours;

                                            //dDAValue = Math.Max(0, Convert.ToDouble(congValues.DAPeakCong));
                                            dDAValue = congValues.DAPeakCong;
                                            dDAsumValue = dDAValue * congValues.PeakHours;
                                            infoPeakDA.SetValue(mPeakDailyPivotDataDA, dDAValue);

                                            dRTValue = congValues.RTPeakCong;
                                            dRTsumValue = dRTValue * congValues.PeakHours;
                                            infoPeakRT.SetValue(mPeakDailyPivotDataRT, dRTValue);

                                            dCrrValue = priceValues.PricePeak;

                                            dCrrsumValue = dCrrValue * congValues.PeakHours;
                                            infoPeakCrr.SetValue(mPeakDailyPivotDataCrr, dCrrValue);

                                            dDACrrValue = (congValues.DAPeakCong) - (dCrrValue);
                                            dDACrrsumValue = dDACrrValue * congValues.PeakHours;
                                            infoPeakDACrr.SetValue(mPeakDailyPivotDataDACrr, dDACrrValue);
                                            //
                                            if (OnPeakChecked)
                                            {
                                                if (DailyChecked)
                                                {
                                                    ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                    mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                    mConsolidatedDataDaily.Index = i++;
                                                    mConsolidatedDataDaily.DAValue = congValues.DAPeakCong;
                                                    mConsolidatedDataDaily.RTValue = congValues.RTPeakCong;
                                                    mConsolidatedDataDaily.CrrValue = dCrrValue;
                                                    mConsolidatedDataDaily.DACrrValue = (congValues.DAPeakCong) - (dCrrValue);
                                                    mGraphList.Add(mConsolidatedDataDaily);
                                                }
                                            }
                                            if (congValues.DAPeakCong == null)
                                                davalue = davalue + 0;
                                            else
                                                davalue = davalue + ((congValues.DAPeakCong.Value) * congValues.PeakHours);

                                            if (congValues.RTPeakCong == null)
                                                rtvalue = rtvalue + 0;
                                            else
                                                rtvalue = rtvalue + ((congValues.RTPeakCong.Value) * congValues.PeakHours);

                                            if (priceValues.PricePeak == null)
                                                Crrvalue = Crrvalue + 0;
                                            else
                                                Crrvalue = Crrvalue + ((priceValues.PricePeak.Value) * congValues.PeakHours);

                                            if (congValues.DAPeakCong == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (priceValues.PricePeak == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (!((congValues.DAPeakCong == null) || (priceValues.PricePeak == null)))
                                            {
                                                daCrrvalue = daCrrvalue + ((congValues.DAPeakCong.Value) - (priceValues.PricePeak.Value * congValues.PeakHours));
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        if (dRTsumValue == null)
                                        {
                                            dRTsumValue = 0;
                                        }
                                        totalPeakrt = totalPeakrt + dRTsumValue;

                                        if (dDAsumValue == null)
                                        {
                                            dDAsumValue = 0;
                                        }
                                        totalPeakda = totalPeakda + dDAsumValue;

                                        if (dCrrsumValue == null)
                                        {
                                            dCrrsumValue = 0;
                                        }
                                        totalPeakCrr = totalPeakCrr + dCrrsumValue;
                                        if (dDACrrsumValue == null)
                                        {
                                            dDACrrsumValue = 0;
                                        }
                                        totalPeakdaCrr = totalPeakdaCrr + dDACrrsumValue;
                                        if (doffRTsumValue == null)
                                        {
                                            doffRTsumValue = 0;
                                        }


                                    }

                                }
                                else if (OffPeakChecked == true)
                                {

                                    try
                                    {
                                        offpeak = offpeak + congValues.OffPeakHours;

                                        doffDAValue = congValues.DAOffPeakCong;
                                        doffDAsumValue = doffDAValue * congValues.OffPeakHours;
                                        infoOffPeakDA.SetValue(mOffPeakDailyPivotDataDA, doffDAValue);

                                        doffRTValue = congValues.RTOffPeakCong;
                                        doffRTsumValue = doffRTValue * congValues.OffPeakHours;
                                        infoOffPeakRT.SetValue(mOffPeakDailyPivotDataRT, doffRTValue);

                                        doffCrrValue = priceValues.PriceOffPeak;
                                        doffCrrsumValue = doffCrrValue * congValues.OffPeakHours;
                                        infoOffPeakCrr.SetValue(mOffPeakDailyPivotDataCrr, doffCrrValue);

                                        doffDACrrValue = (congValues.DAOffPeakCong) - (priceValues.PriceOffPeak);
                                        doffDACrrsumValue = doffDACrrValue * congValues.OffPeakHours;
                                        infoOffPeakDACrr.SetValue(mOffPeakDailyPivotDataDACrr, doffDACrrValue);
                                        if (OffPeakChecked)
                                        {
                                            if (DailyChecked)
                                            {
                                                ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                mConsolidatedDataDaily.Index = i++;
                                                mConsolidatedDataDaily.DAValue = congValues.DAOffPeakCong;
                                                mConsolidatedDataDaily.RTValue = congValues.RTOffPeakCong;
                                                mConsolidatedDataDaily.CrrValue = priceValues.PriceOffPeak;
                                                mConsolidatedDataDaily.DACrrValue = (congValues.DAOffPeakCong) - (priceValues.PriceOffPeak); ;
                                                mGraphList.Add(mConsolidatedDataDaily);
                                            }
                                        }
                                        if (congValues.DAOffPeakCong == null)
                                            davalue = davalue + 0;
                                        else
                                            davalue = davalue + ((congValues.DAOffPeakCong.Value) * congValues.OffPeakHours);
                                        if (congValues.RTOffPeakCong == null)
                                            rtvalue = rtvalue + 0;
                                        else
                                            rtvalue = rtvalue + ((congValues.RTOffPeakCong.Value) * congValues.PeakHours);

                                        if (priceValues.PriceOffPeak == null)
                                            Crrvalue = Crrvalue + 0;
                                        else
                                            Crrvalue = Crrvalue + ((priceValues.PriceOffPeak.Value) * congValues.OffPeakHours);

                                        if (congValues.DAOffPeakCong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (congValues.DAOffPeakCong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (!((congValues.DAOffPeakCong == null) || (priceValues.PriceOffPeak == null)))
                                        {
                                            daCrrvalue = daCrrvalue + ((((congValues.DAOffPeakCong.Value) - (priceValues.PriceOffPeak.Value))) * congValues.OffPeakHours);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    totalOffPeakkrt = totalOffPeakkrt + doffRTsumValue;

                                    if (doffDAsumValue == null)
                                    {
                                        doffDAsumValue = 0;
                                    }
                                    totalOffPeakda = totalOffPeakda + doffDAsumValue;

                                    if (doffCrrsumValue == null)
                                    {
                                        doffCrrsumValue = 0;
                                    }
                                    totalOffPeakCrr = totalOffPeakCrr + doffCrrsumValue;

                                    if (doffDACrrsumValue == null)
                                    {
                                        doffDACrrsumValue = 0;
                                    }
                                    totalOffPeakdaCrr = totalOffPeakdaCrr + doffDACrrsumValue;

                                }
                                else if (PeakWE == true)
                                {
                                    bool is_holiday = false;
                                    DateTime ncurdate = curdate.AddHours(8);


                                    if (dictPeakYn.ContainsKey(ncurdate))
                                        is_holiday = true;

                                    if (Weekendays.ToLower() == Convert.ToString("saturday") || Weekendays.ToLower() == Convert.ToString("sunday") || is_holiday)
                                    {
                                        try
                                        {
                                            peakhrs = peakhrs + congValues.PeakWEHours;

                                            dDAValue = congValues.DAPeakWECong;
                                            dDAsumValue = dDAValue * congValues.PeakWEHours;
                                            infoPeakDA.SetValue(mPeakWEDailyPivotDataDA, dDAValue);

                                            dRTValue = congValues.RTPeakWECong;
                                            dRTsumValue = dRTValue * congValues.PeakWEHours;
                                            infoPeakRT.SetValue(mPeakWEDailyPivotDataRT, dRTValue);

                                            dCrrValue = priceValues.PriceWE;

                                            dCrrsumValue = dCrrValue * congValues.PeakWEHours;
                                            infoPeakCrr.SetValue(mPeakWEDailyPivotDataCrr, dCrrValue);

                                            dDACrrValue = (congValues.DAPeakWECong) - (dCrrValue);
                                            dDACrrsumValue = dDACrrValue * congValues.PeakWEHours;
                                            infoPeakDACrr.SetValue(mPeakWEDailyPivotDataDACrr, dDACrrValue);
                                            //
                                            if (PeakWE)
                                            {
                                                if (DailyChecked)
                                                {
                                                    ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                    mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                    mConsolidatedDataDaily.Index = i++;
                                                    mConsolidatedDataDaily.DAValue = congValues.DAPeakWECong;
                                                    mConsolidatedDataDaily.RTValue = congValues.RTPeakWECong;
                                                    mConsolidatedDataDaily.CrrValue = dCrrValue;
                                                    mConsolidatedDataDaily.DACrrValue = (congValues.DAPeakWECong) - (dCrrValue);
                                                    mGraphList.Add(mConsolidatedDataDaily);
                                                }
                                            }
                                            if (congValues.DAPeakWECong == null)
                                                davalue = davalue + 0;
                                            else
                                                davalue = davalue + ((congValues.DAPeakWECong.Value) * congValues.PeakWEHours);

                                            if (congValues.RTPeakWECong == null)
                                                rtvalue = rtvalue + 0;
                                            else
                                                rtvalue = rtvalue + ((congValues.RTPeakWECong.Value) * congValues.PeakWEHours);

                                            if (priceValues.PriceWE == null)
                                                Crrvalue = Crrvalue + 0;
                                            else
                                                Crrvalue = Crrvalue + ((priceValues.PriceWE.Value) * congValues.PeakWEHours);

                                            if (congValues.DAPeakWECong == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (priceValues.PriceWE == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (!((congValues.DAPeakWECong == null) || (priceValues.PriceWE == null)))
                                            {
                                                daCrrvalue = daCrrvalue + ((congValues.DAPeakWECong.Value) - (priceValues.PriceWE.Value * congValues.PeakWEHours));
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }

                                        if (dRTsumValue == null)
                                        {
                                            dRTsumValue = 0;
                                        }
                                        totalPeakrt = totalPeakrt + dRTsumValue;

                                        if (dDAsumValue == null)
                                        {
                                            dDAsumValue = 0;
                                        }
                                        totalPeakda = totalPeakda + dDAsumValue;

                                        if (dCrrsumValue == null)
                                        {
                                            dCrrsumValue = 0;
                                        }
                                        totalPeakCrr = totalPeakCrr + dCrrsumValue;
                                        if (dDACrrsumValue == null)
                                        {
                                            dDACrrsumValue = 0;
                                        }
                                        totalPeakdaCrr = totalPeakdaCrr + dDACrrsumValue;
                                        if (doffRTsumValue == null)
                                        {
                                            doffRTsumValue = 0;
                                        }
                                    }

                                }
                                else if (Hour24Checked == true)
                                {

                                    try
                                    {
                                        hr24 = hr24 + (congValues.PeakHours + congValues.OffPeakHours);

                                        d24DAValue = congValues.DA24Cong;
                                        d24DAsumValue = d24DAValue * (congValues.PeakHours + congValues.OffPeakHours);
                                        info24DA.SetValue(m24DailyPivotDataDA, d24DAValue);

                                        d24RTValue = congValues.RT24Cong;
                                        d24RTsumValue = d24RTValue * (congValues.PeakHours + congValues.OffPeakHours);
                                        info24RT.SetValue(m24DailyPivotDataRT, d24RTValue);
                                        int total24hours = priceValues.PeakHours + priceValues.OffPeakHours + priceValues.PeakWE;
                                        d24CrrValue = ((priceValues.PricePeak * priceValues.PeakHours) + (priceValues.PriceOffPeak * priceValues.OffPeakHours) + (priceValues.PriceWE * priceValues.PeakWE)) / total24hours;
                                        d24CrrsumValue = (d24DAValue - d24CrrValue) * (congValues.PeakHours + congValues.OffPeakHours);

                                        // d24CrrsumValue = d24CrrValue * total24hours;
                                        info24Crr.SetValue(m24DailyPivotDataCrr, d24CrrValue);

                                        d24DACrrValue = (congValues.DA24Cong) - (priceValues.PricePeak + priceValues.PriceOffPeak + priceValues.PriceWE);
                                        d24DACrrsumValue = d24DACrrValue * (congValues.PeakHours + congValues.OffPeakHours);
                                        info24DACrr.SetValue(m24DailyPivotDataDACrr, d24DACrrValue);
                                        if (Hour24Checked)
                                        {
                                            if (DailyChecked)
                                            {
                                                ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                mConsolidatedDataDaily.Index = i++;
                                                mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                mConsolidatedDataDaily.DAValue = congValues.DA24Cong;
                                                mConsolidatedDataDaily.RTValue = congValues.RT24Cong;
                                                mConsolidatedDataDaily.CrrValue = (priceValues.PricePeak + priceValues.PriceOffPeak + priceValues.PriceWE);
                                                mConsolidatedDataDaily.DACrrValue = (congValues.DA24Cong) - (priceValues.PricePeak + priceValues.PriceOffPeak + priceValues.PriceWE);
                                                mGraphList.Add(mConsolidatedDataDaily);
                                            }
                                        }

                                        if (congValues.DA24Cong == null)
                                            davalue = davalue + 0;
                                        else
                                            davalue = davalue + ((congValues.DA24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours));
                                        if (congValues.RT24Cong == null)
                                            rtvalue = rtvalue + 0;
                                        else
                                            rtvalue = rtvalue + ((congValues.RT24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours));
                                        if (priceValues.PricePeak == null)
                                            Crrvalue = Crrvalue + 0;
                                        else
                                            Crrvalue = Crrvalue + (((priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value + priceValues.PriceWE.Value) * (congValues.PeakHours + congValues.OffPeakHours)));

                                        if (congValues.DA24Cong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (!(priceValues.PricePeak == null || priceValues.OffPeakHours == null))
                                        {
                                            if (congValues.DA24Cong != null)
                                            {
                                                daCrrvalue = daCrrvalue + ((((congValues.DA24Cong.Value) - (priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value + priceValues.PriceWE.Value))) * (congValues.PeakHours + congValues.OffPeakHours));
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    if (d24RTsumValue == null)
                                    {
                                        d24RTsumValue = 0;
                                    }
                                    total24krt = total24krt + d24RTsumValue;

                                    if (d24DAsumValue == null)
                                    {
                                        d24DAsumValue = 0;
                                    }
                                    total24da = total24da + d24DAsumValue;

                                    if (d24CrrsumValue == null)
                                    {
                                        d24CrrsumValue = 0;
                                    }
                                    total24Crr = total24Crr + d24CrrsumValue;

                                    if (d24DACrrsumValue == null)
                                    {
                                        d24DACrrsumValue = 0;
                                    }
                                    total24daCrr = total24daCrr + d24DACrrsumValue;

                                }

                            }


                        }
                        else
                        {
                            if (HedgeComboSelectedValue == "OBL")
                            {
                                try
                                {
                                    peakhrs = peakhrs + congValues.PeakHours;

                                    dDAValue = congValues.DAPeakCong;
                                    dDAsumValue = dDAValue * congValues.PeakHours;
                                    infoPeakDA.SetValue(mPeakDailyPivotDataDA, dDAValue);

                                    dRTValue = congValues.RTPeakCong;
                                    dRTsumValue = dRTValue * congValues.PeakHours;
                                    infoPeakRT.SetValue(mPeakDailyPivotDataRT, dRTValue);

                                    dCrrValue = (priceValues.PricePeak) / priceValues.PeakHours;

                                    dCrrsumValue = dCrrValue * congValues.PeakHours;
                                    infoPeakCrr.SetValue(mPeakDailyPivotDataCrr, dCrrValue);

                                    dDACrrValue = (congValues.DAPeakCong) - ((priceValues.PricePeak) / priceValues.PeakHours);
                                    dDACrrsumValue = dDACrrValue * congValues.PeakHours;
                                    infoPeakDACrr.SetValue(mPeakDailyPivotDataDACrr, dDACrrValue);
                                    //
                                    if (OnPeakChecked)
                                    {
                                        if (DailyChecked)
                                        {
                                            ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                            mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                            mConsolidatedDataDaily.Index = i++;
                                            mConsolidatedDataDaily.DAValue = congValues.DAPeakCong;
                                            mConsolidatedDataDaily.RTValue = congValues.RTPeakCong;
                                            mConsolidatedDataDaily.CrrValue = (priceValues.PricePeak) / priceValues.PeakHours;
                                            mConsolidatedDataDaily.DACrrValue = (congValues.DAPeakCong) - ((priceValues.PricePeak) / priceValues.PeakHours);
                                            mGraphList.Add(mConsolidatedDataDaily);
                                        }
                                    }
                                    if (congValues.DAPeakCong == null)
                                        davalue = davalue + 0;
                                    else
                                        davalue = davalue + ((congValues.DAPeakCong.Value) * congValues.PeakHours);

                                    if (congValues.RTPeakCong == null)
                                        rtvalue = rtvalue + 0;
                                    else
                                        rtvalue = rtvalue + ((congValues.RTPeakCong.Value) * congValues.PeakHours);

                                    if (priceValues.PricePeak == null)
                                        Crrvalue = Crrvalue + 0;
                                    else
                                        Crrvalue = Crrvalue + (((priceValues.PricePeak.Value) / priceValues.PeakHours) * congValues.PeakHours);

                                    if (congValues.DAPeakCong == null)
                                        daCrrvalue = daCrrvalue + 0;
                                    if (priceValues.PricePeak == null)
                                        daCrrvalue = daCrrvalue + 0;
                                    if (!((congValues.DAPeakCong == null) || (priceValues.PricePeak == null)))
                                    {
                                        daCrrvalue = daCrrvalue + (((congValues.DAPeakCong.Value) - ((priceValues.PricePeak.Value) / priceValues.PeakHours)) * congValues.PeakHours);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                try
                                {
                                    offpeak = offpeak + congValues.OffPeakHours;

                                    doffDAValue = congValues.DAOffPeakCong;
                                    doffDAsumValue = doffDAValue * congValues.OffPeakHours;
                                    infoOffPeakDA.SetValue(mOffPeakDailyPivotDataDA, doffDAValue);

                                    doffRTValue = congValues.RTOffPeakCong;
                                    doffRTsumValue = doffRTValue * congValues.OffPeakHours;
                                    infoOffPeakRT.SetValue(mOffPeakDailyPivotDataRT, doffRTValue);

                                    doffCrrValue = (priceValues.PriceOffPeak) / priceValues.OffPeakHours;
                                    doffCrrsumValue = doffCrrValue * congValues.OffPeakHours;
                                    infoOffPeakCrr.SetValue(mOffPeakDailyPivotDataCrr, doffCrrValue);

                                    doffDACrrValue = (congValues.DAOffPeakCong) - ((priceValues.PriceOffPeak) / priceValues.OffPeakHours);
                                    doffDACrrsumValue = doffDACrrValue * congValues.OffPeakHours;
                                    infoOffPeakDACrr.SetValue(mOffPeakDailyPivotDataDACrr, doffDACrrValue);
                                    if (OffPeakChecked)
                                    {
                                        if (DailyChecked)
                                        {
                                            ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                            mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                            mConsolidatedDataDaily.Index = i++;
                                            mConsolidatedDataDaily.DAValue = congValues.DAOffPeakCong;
                                            mConsolidatedDataDaily.RTValue = congValues.RTOffPeakCong;
                                            mConsolidatedDataDaily.CrrValue = (priceValues.PriceOffPeak) / priceValues.OffPeakHours;
                                            mConsolidatedDataDaily.DACrrValue = (congValues.DAOffPeakCong) - ((priceValues.PriceOffPeak) / priceValues.OffPeakHours); ;
                                            mGraphList.Add(mConsolidatedDataDaily);
                                        }
                                    }
                                    if (congValues.DAOffPeakCong == null)
                                        davalue = davalue + 0;
                                    else
                                        davalue = davalue + ((congValues.DAOffPeakCong.Value) * congValues.OffPeakHours);
                                    if (congValues.RTOffPeakCong == null)
                                        rtvalue = rtvalue + 0;
                                    else
                                        rtvalue = rtvalue + ((congValues.RTOffPeakCong.Value) * congValues.PeakHours);

                                    if (priceValues.PriceOffPeak == null)
                                        Crrvalue = Crrvalue + 0;
                                    else
                                        Crrvalue = Crrvalue + ((((priceValues.PriceOffPeak.Value) / priceValues.OffPeakHours) * congValues.OffPeakHours));

                                    if (congValues.DAOffPeakCong == null)
                                        daCrrvalue = daCrrvalue + 0;
                                    if (congValues.DAOffPeakCong == null)
                                        daCrrvalue = daCrrvalue + 0;
                                    if (!((congValues.DAOffPeakCong == null) || (priceValues.PriceOffPeak == null)))
                                    {
                                        daCrrvalue = daCrrvalue + ((((congValues.DAOffPeakCong.Value) - ((priceValues.PriceOffPeak.Value) / priceValues.OffPeakHours))) * congValues.OffPeakHours);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                try
                                {
                                    hr24 = hr24 + (congValues.PeakHours + congValues.OffPeakHours);

                                    d24DAValue = congValues.DA24Cong;
                                    d24DAsumValue = d24DAValue * (congValues.PeakHours + congValues.OffPeakHours);
                                    info24DA.SetValue(m24DailyPivotDataDA, d24DAValue);

                                    d24RTValue = congValues.RT24Cong;
                                    d24RTsumValue = d24RTValue * (congValues.PeakHours + congValues.OffPeakHours);
                                    info24RT.SetValue(m24DailyPivotDataRT, d24RTValue);

                                    d24CrrValue = (priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours);
                                    d24CrrsumValue = d24CrrValue * (congValues.PeakHours + congValues.OffPeakHours);
                                    info24Crr.SetValue(m24DailyPivotDataCrr, d24CrrValue);

                                    d24DACrrValue = (congValues.DA24Cong) - ((priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours));
                                    d24DACrrsumValue = d24DACrrValue * (congValues.PeakHours + congValues.OffPeakHours);
                                    info24DACrr.SetValue(m24DailyPivotDataDACrr, d24DACrrValue);
                                    if (Hour24Checked)
                                    {
                                        if (DailyChecked)
                                        {
                                            ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                            mConsolidatedDataDaily.Index = i++;
                                            mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                            mConsolidatedDataDaily.DAValue = congValues.DA24Cong;
                                            mConsolidatedDataDaily.RTValue = congValues.RT24Cong;
                                            mConsolidatedDataDaily.CrrValue = (priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours);
                                            mConsolidatedDataDaily.DACrrValue = (congValues.DA24Cong) - ((priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours));
                                            mGraphList.Add(mConsolidatedDataDaily);
                                        }
                                    }

                                    if (congValues.DA24Cong == null)
                                        davalue = davalue + 0;
                                    else
                                        davalue = davalue + ((congValues.DA24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours));
                                    if (congValues.RT24Cong == null)
                                        rtvalue = rtvalue + 0;
                                    else
                                        rtvalue = rtvalue + ((congValues.RT24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours));
                                    if (priceValues.PricePeak == null)
                                        Crrvalue = Crrvalue + 0;
                                    else
                                        Crrvalue = Crrvalue + ((((priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value) / (priceValues.PeakHours + priceValues.OffPeakHours)) * (congValues.PeakHours + congValues.OffPeakHours)));

                                    if (congValues.DA24Cong == null)
                                        daCrrvalue = daCrrvalue + 0;
                                    if (!(priceValues.PricePeak == null || priceValues.OffPeakHours == null))
                                    {
                                        if (congValues.DA24Cong != null)
                                        {
                                            daCrrvalue = daCrrvalue + ((((congValues.DA24Cong.Value) - ((priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value) / (priceValues.PeakHours + priceValues.OffPeakHours)))) * (congValues.PeakHours + congValues.OffPeakHours));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                if (dRTsumValue == null)
                                {
                                    dRTsumValue = 0;
                                }
                                totalPeakrt = totalPeakrt + dRTsumValue;

                                if (dDAsumValue == null)
                                {
                                    dDAsumValue = 0;
                                }
                                totalPeakda = totalPeakda + dDAsumValue;

                                if (dCrrsumValue == null)
                                {
                                    dCrrsumValue = 0;
                                }
                                totalPeakCrr = totalPeakCrr + dCrrsumValue;
                                if (dDACrrsumValue == null)
                                {
                                    dDACrrsumValue = 0;
                                }
                                totalPeakdaCrr = totalPeakdaCrr + dDACrrsumValue;
                                if (doffRTsumValue == null)
                                {
                                    doffRTsumValue = 0;
                                }
                                totalOffPeakkrt = totalOffPeakkrt + doffRTsumValue;

                                if (doffDAsumValue == null)
                                {
                                    doffDAsumValue = 0;
                                }
                                totalOffPeakda = totalOffPeakda + doffDAsumValue;

                                if (doffCrrsumValue == null)
                                {
                                    doffCrrsumValue = 0;
                                }
                                totalOffPeakCrr = totalOffPeakCrr + doffCrrsumValue;

                                if (doffDACrrsumValue == null)
                                {
                                    doffDACrrsumValue = 0;
                                }
                                totalOffPeakdaCrr = totalOffPeakdaCrr + doffDACrrsumValue;

                                if (d24RTsumValue == null)
                                {
                                    d24RTsumValue = 0;
                                }
                                total24krt = total24krt + d24RTsumValue;

                                if (d24DAsumValue == null)
                                {
                                    d24DAsumValue = 0;
                                }
                                total24da = total24da + d24DAsumValue;

                                if (d24CrrsumValue == null)
                                {
                                    d24CrrsumValue = 0;
                                }
                                total24Crr = total24Crr + d24CrrsumValue;

                                if (d24DACrrsumValue == null)
                                {
                                    d24DACrrsumValue = 0;
                                }
                                total24daCrr = total24daCrr + d24DACrrsumValue;

                            }
                            else
                            {
                                if (OnPeakChecked == true)
                                {
                                    if (Weekendays.ToLower() == Convert.ToString("saturday") || Weekendays.ToLower() == Convert.ToString("sunday"))
                                    {

                                    }
                                    else
                                    {
                                        try
                                        {
                                            peakhrs = peakhrs + congValues.PeakHours;

                                            dDAValue = congValues.DAPeakCong;
                                            dDAsumValue = dDAValue * congValues.PeakHours;
                                            infoPeakDA.SetValue(mPeakDailyPivotDataDA, dDAValue);

                                            dRTValue = congValues.RTPeakCong;
                                            dRTsumValue = dRTValue * congValues.PeakHours;
                                            infoPeakRT.SetValue(mPeakDailyPivotDataRT, dRTValue);

                                            dCrrValue = (priceValues.PricePeak) / priceValues.PeakHours;

                                            dCrrsumValue = dCrrValue * congValues.PeakHours;
                                            infoPeakCrr.SetValue(mPeakDailyPivotDataCrr, dCrrValue);

                                            dDACrrValue = (congValues.DAPeakCong) - ((priceValues.PricePeak) / priceValues.PeakHours);
                                            dDACrrsumValue = dDACrrValue * congValues.PeakHours;
                                            infoPeakDACrr.SetValue(mPeakDailyPivotDataDACrr, dDACrrValue);
                                            //
                                            if (OnPeakChecked)
                                            {
                                                if (DailyChecked)
                                                {
                                                    ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                    mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                    mConsolidatedDataDaily.Index = i++;
                                                    mConsolidatedDataDaily.DAValue = congValues.DAPeakCong;
                                                    mConsolidatedDataDaily.RTValue = congValues.RTPeakCong;
                                                    mConsolidatedDataDaily.CrrValue = (priceValues.PricePeak) / priceValues.PeakHours;
                                                    mConsolidatedDataDaily.DACrrValue = (congValues.DAPeakCong) - ((priceValues.PricePeak) / priceValues.PeakHours);
                                                    mGraphList.Add(mConsolidatedDataDaily);
                                                }
                                            }
                                            if (congValues.DAPeakCong == null)
                                                davalue = davalue + 0;
                                            else
                                                davalue = davalue + ((congValues.DAPeakCong.Value) * congValues.PeakHours);

                                            if (congValues.RTPeakCong == null)
                                                rtvalue = rtvalue + 0;
                                            else
                                                rtvalue = rtvalue + ((congValues.RTPeakCong.Value) * congValues.PeakHours);

                                            if (priceValues.PricePeak == null)
                                                Crrvalue = Crrvalue + 0;
                                            else
                                                Crrvalue = Crrvalue + (((priceValues.PricePeak.Value) / priceValues.PeakHours) * congValues.PeakHours);

                                            if (congValues.DAPeakCong == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (priceValues.PricePeak == null)
                                                daCrrvalue = daCrrvalue + 0;
                                            if (!((congValues.DAPeakCong == null) || (priceValues.PricePeak == null)))
                                            {
                                                daCrrvalue = daCrrvalue + (((congValues.DAPeakCong.Value) - ((priceValues.PricePeak.Value) / priceValues.PeakHours)) * congValues.PeakHours);
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                }

                                if (OffPeakChecked == true)
                                {
                                    try
                                    {
                                        offpeak = offpeak + congValues.OffPeakHours;

                                        doffDAValue = congValues.DAOffPeakCong;
                                        doffDAsumValue = doffDAValue * congValues.OffPeakHours;
                                        infoOffPeakDA.SetValue(mOffPeakDailyPivotDataDA, doffDAValue);

                                        doffRTValue = congValues.RTOffPeakCong;
                                        doffRTsumValue = doffRTValue * congValues.OffPeakHours;
                                        infoOffPeakRT.SetValue(mOffPeakDailyPivotDataRT, doffRTValue);

                                        doffCrrValue = (priceValues.PriceOffPeak) / priceValues.OffPeakHours;
                                        doffCrrsumValue = doffCrrValue * congValues.OffPeakHours;
                                        infoOffPeakCrr.SetValue(mOffPeakDailyPivotDataCrr, doffCrrValue);

                                        doffDACrrValue = (congValues.DAOffPeakCong) - ((priceValues.PriceOffPeak) / priceValues.OffPeakHours);
                                        doffDACrrsumValue = doffDACrrValue * congValues.OffPeakHours;
                                        infoOffPeakDACrr.SetValue(mOffPeakDailyPivotDataDACrr, doffDACrrValue);
                                        if (OffPeakChecked)
                                        {
                                            if (DailyChecked)
                                            {
                                                ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                mConsolidatedDataDaily.Index = i++;
                                                mConsolidatedDataDaily.DAValue = congValues.DAOffPeakCong;
                                                mConsolidatedDataDaily.RTValue = congValues.RTOffPeakCong;
                                                mConsolidatedDataDaily.CrrValue = (priceValues.PriceOffPeak) / priceValues.OffPeakHours;
                                                mConsolidatedDataDaily.DACrrValue = (congValues.DAOffPeakCong) - ((priceValues.PriceOffPeak) / priceValues.OffPeakHours); ;
                                                mGraphList.Add(mConsolidatedDataDaily);
                                            }
                                        }
                                        if (congValues.DAOffPeakCong == null)
                                            davalue = davalue + 0;
                                        else
                                            davalue = davalue + ((congValues.DAOffPeakCong.Value) * congValues.OffPeakHours);
                                        if (congValues.RTOffPeakCong == null)
                                            rtvalue = rtvalue + 0;
                                        else
                                            rtvalue = rtvalue + ((congValues.RTOffPeakCong.Value) * congValues.PeakHours);

                                        if (priceValues.PriceOffPeak == null)
                                            Crrvalue = Crrvalue + 0;
                                        else
                                            Crrvalue = Crrvalue + ((((priceValues.PriceOffPeak.Value) / priceValues.OffPeakHours) * congValues.OffPeakHours));

                                        if (congValues.DAOffPeakCong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (congValues.DAOffPeakCong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (!((congValues.DAOffPeakCong == null) || (priceValues.PriceOffPeak == null)))
                                        {
                                            daCrrvalue = daCrrvalue + ((((congValues.DAOffPeakCong.Value) - ((priceValues.PriceOffPeak.Value) / priceValues.OffPeakHours))) * congValues.OffPeakHours);
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                                if (Hour24Checked == true)
                                {
                                    try
                                    {
                                        hr24 = hr24 + (congValues.PeakHours + congValues.OffPeakHours);

                                        d24DAValue = congValues.DA24Cong;
                                        d24DAsumValue = d24DAValue * (congValues.PeakHours + congValues.OffPeakHours);
                                        info24DA.SetValue(m24DailyPivotDataDA, d24DAValue);

                                        d24RTValue = congValues.RT24Cong;
                                        d24RTsumValue = d24RTValue * (congValues.PeakHours + congValues.OffPeakHours);
                                        info24RT.SetValue(m24DailyPivotDataRT, d24RTValue);

                                        d24CrrValue = (priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours);
                                        d24CrrsumValue = d24CrrValue * (congValues.PeakHours + congValues.OffPeakHours);
                                        info24Crr.SetValue(m24DailyPivotDataCrr, d24CrrValue);

                                        d24DACrrValue = (congValues.DA24Cong) - ((priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours));
                                        d24DACrrsumValue = d24DACrrValue * (congValues.PeakHours + congValues.OffPeakHours);
                                        info24DACrr.SetValue(m24DailyPivotDataDACrr, d24DACrrValue);
                                        if (Hour24Checked)
                                        {
                                            if (DailyChecked)
                                            {
                                                ConsolidatedData mConsolidatedDataDaily = new ConsolidatedData();
                                                mConsolidatedDataDaily.Index = i++;
                                                mConsolidatedDataDaily.StartDate = new DateTime(year, month, days);
                                                mConsolidatedDataDaily.DAValue = congValues.DA24Cong;
                                                mConsolidatedDataDaily.RTValue = congValues.RT24Cong;
                                                mConsolidatedDataDaily.CrrValue = (priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours);
                                                mConsolidatedDataDaily.DACrrValue = (congValues.DA24Cong) - ((priceValues.PricePeak + priceValues.PriceOffPeak) / (priceValues.PeakHours + priceValues.OffPeakHours));
                                                mGraphList.Add(mConsolidatedDataDaily);
                                            }
                                        }

                                        if (congValues.DA24Cong == null)
                                            davalue = davalue + 0;
                                        else
                                            davalue = davalue + ((congValues.DA24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours));
                                        if (congValues.RT24Cong == null)
                                            rtvalue = rtvalue + 0;
                                        else
                                            rtvalue = rtvalue + ((congValues.RT24Cong.Value) * (congValues.PeakHours + congValues.OffPeakHours));
                                        if (priceValues.PricePeak == null)
                                            Crrvalue = Crrvalue + 0;
                                        else
                                            Crrvalue = Crrvalue + ((((priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value) / (priceValues.PeakHours + priceValues.OffPeakHours)) * (congValues.PeakHours + congValues.OffPeakHours)));

                                        if (congValues.DA24Cong == null)
                                            daCrrvalue = daCrrvalue + 0;
                                        if (!(priceValues.PricePeak == null || priceValues.OffPeakHours == null))
                                        {
                                            if (congValues.DA24Cong != null)
                                            {
                                                daCrrvalue = daCrrvalue + ((((congValues.DA24Cong.Value) - ((priceValues.PricePeak.Value + priceValues.PriceOffPeak.Value) / (priceValues.PeakHours + priceValues.OffPeakHours)))) * (congValues.PeakHours + congValues.OffPeakHours));
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                }


                                if (dRTsumValue == null)
                                {
                                    dRTsumValue = 0;
                                }
                                totalPeakrt = totalPeakrt + dRTsumValue;

                                if (dDAsumValue == null)
                                {
                                    dDAsumValue = 0;
                                }
                                totalPeakda = totalPeakda + dDAsumValue;

                                if (dCrrsumValue == null)
                                {
                                    dCrrsumValue = 0;
                                }
                                totalPeakCrr = totalPeakCrr + dCrrsumValue;
                                if (dDACrrsumValue == null)
                                {
                                    dDACrrsumValue = 0;
                                }
                                totalPeakdaCrr = totalPeakdaCrr + dDACrrsumValue;
                                if (doffRTsumValue == null)
                                {
                                    doffRTsumValue = 0;
                                }
                                totalOffPeakkrt = totalOffPeakkrt + doffRTsumValue;

                                if (doffDAsumValue == null)
                                {
                                    doffDAsumValue = 0;
                                }
                                totalOffPeakda = totalOffPeakda + doffDAsumValue;

                                if (doffCrrsumValue == null)
                                {
                                    doffCrrsumValue = 0;
                                }
                                totalOffPeakCrr = totalOffPeakCrr + doffCrrsumValue;

                                if (doffDACrrsumValue == null)
                                {
                                    doffDACrrsumValue = 0;
                                }
                                totalOffPeakdaCrr = totalOffPeakdaCrr + doffDACrrsumValue;

                                if (d24RTsumValue == null)
                                {
                                    d24RTsumValue = 0;
                                }
                                total24krt = total24krt + d24RTsumValue;

                                if (d24DAsumValue == null)
                                {
                                    d24DAsumValue = 0;
                                }
                                total24da = total24da + d24DAsumValue;

                                if (d24CrrsumValue == null)
                                {
                                    d24CrrsumValue = 0;
                                }
                                total24Crr = total24Crr + d24CrrsumValue;

                                if (d24DACrrsumValue == null)
                                {
                                    d24DACrrsumValue = 0;
                                }
                                total24daCrr = total24daCrr + d24DACrrsumValue;
                            }



                        }
                    }
                    #region OnPeak
                    mPeakDailyPivotDataRT.RowDisplayType = "RT";
                    mPeakDailyPivotDataRT.RowName = "";
                    mPeakDailyPivotDataRT.Total = totalPeakrt;
                    mPeakDailyPivotDataRT.Average = totalPeakrt / peakhrs;
                    mPeakDailyPivotDataRT.ClassType = "Peak";
                    mDailyPivotDataList.Add(mPeakDailyPivotDataRT);


                    mPeakDailyPivotDataDA.RowDisplayType = "DA";
                    mPeakDailyPivotDataDA.Total = totalPeakda;
                    mPeakDailyPivotDataDA.Average = totalPeakda / peakhrs;
                    mPeakDailyPivotDataDA.ClassType = "Peak";
                    mDailyPivotDataList.Add(mPeakDailyPivotDataDA);


                    mPeakDailyPivotDataCrr.RowDisplayType = "Crr";
                    mPeakDailyPivotDataCrr.Total = totalPeakCrr;
                    mPeakDailyPivotDataCrr.Average = totalPeakCrr / peakhrs;
                    mPeakDailyPivotDataCrr.ClassType = "Peak";
                    mDailyPivotDataList.Add(mPeakDailyPivotDataCrr);


                    mPeakDailyPivotDataDACrr.RowDisplayType = "DA-Crr";
                    mPeakDailyPivotDataDACrr.Total = totalPeakdaCrr;
                    mPeakDailyPivotDataDACrr.Average = totalPeakdaCrr / peakhrs;
                    mPeakDailyPivotDataDACrr.ClassType = "Peak";
                    mDailyPivotDataList.Add(mPeakDailyPivotDataDACrr);

                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        if (OnPeakChecked)
                        {
                            mConsolidatedData.DAValue = totalPeakda;
                            mConsolidatedData.RTValue = totalPeakrt;
                            mConsolidatedData.CrrValue = totalPeakCrr;
                            mConsolidatedData.DACrrValue = totalPeakdaCrr;
                            if (MonthlyChecked)
                                mGraphList.Add(mConsolidatedData);
                        }
                    }
                    else
                    {
                        if (OnPeakChecked)
                        {
                            mConsolidatedData.DAValue = totalPeakda;
                            mConsolidatedData.RTValue = totalPeakrt;
                            mConsolidatedData.CrrValue = totalPeakCrr;
                            mConsolidatedData.DACrrValue = totalPeakdaCrr;
                            if (MonthlyChecked)
                                mGraphList.Add(mConsolidatedData);
                        }
                    }
                    #endregion

                    #region OffPeak
                    mOffPeakDailyPivotDataRT.RowDisplayType = "RT";
                    mOffPeakDailyPivotDataRT.Total = totalOffPeakkrt;
                    mOffPeakDailyPivotDataRT.Average = totalOffPeakkrt / offpeak;
                    mOffPeakDailyPivotDataRT.ClassType = "OffPeak";
                    mDailyPivotDataList.Add(mOffPeakDailyPivotDataRT);


                    mOffPeakDailyPivotDataDA.RowDisplayType = "DA";
                    mOffPeakDailyPivotDataDA.Total = totalOffPeakda;
                    mOffPeakDailyPivotDataDA.Average = totalOffPeakda / offpeak;
                    mOffPeakDailyPivotDataDA.ClassType = "OffPeak";
                    mDailyPivotDataList.Add(mOffPeakDailyPivotDataDA);


                    mOffPeakDailyPivotDataCrr.RowDisplayType = "Crr";
                    mOffPeakDailyPivotDataCrr.Total = totalOffPeakCrr;
                    mOffPeakDailyPivotDataCrr.Average = totalOffPeakCrr / offpeak;
                    mOffPeakDailyPivotDataCrr.ClassType = "OffPeak";
                    mDailyPivotDataList.Add(mOffPeakDailyPivotDataCrr);


                    mOffPeakDailyPivotDataDACrr.RowDisplayType = "DA-Crr";
                    mOffPeakDailyPivotDataDACrr.Total = totalOffPeakdaCrr;
                    mOffPeakDailyPivotDataDACrr.Average = totalOffPeakdaCrr / offpeak;
                    mOffPeakDailyPivotDataDACrr.ClassType = "OffPeak";
                    mDailyPivotDataList.Add(mOffPeakDailyPivotDataDACrr);
                    mOffPeakDailyPivotDataDACrr.Average = totalOffPeakdaCrr / offpeak;

                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        if (OffPeakChecked)
                        {

                            mConsolidatedData.DAValue = totalOffPeakda;
                            mConsolidatedData.RTValue = totalOffPeakkrt;
                            mConsolidatedData.CrrValue = totalOffPeakCrr;
                            mConsolidatedData.DACrrValue = totalOffPeakdaCrr;
                            if (MonthlyChecked)
                                mGraphList.Add(mConsolidatedData);
                        }
                    }
                    else
                    {
                        if (OffPeakChecked)
                        {

                            mConsolidatedData.DAValue = totalOffPeakda;
                            mConsolidatedData.RTValue = totalOffPeakkrt;
                            mConsolidatedData.CrrValue = totalOffPeakCrr;
                            mConsolidatedData.DACrrValue = totalOffPeakdaCrr;
                            if (MonthlyChecked)
                                mGraphList.Add(mConsolidatedData);
                        }
                    }

                    #endregion

                    #region 24hrs
                    m24DailyPivotDataRT.RowDisplayType = "RT";
                    m24DailyPivotDataRT.Total = total24krt;
                    m24DailyPivotDataRT.Average = total24krt / hr24;
                    m24DailyPivotDataRT.ClassType = "24H";
                    mDailyPivotDataList.Add(m24DailyPivotDataRT);


                    m24DailyPivotDataDA.RowDisplayType = "DA";
                    m24DailyPivotDataDA.Total = total24da;
                    m24DailyPivotDataDA.Average = total24da / hr24;
                    m24DailyPivotDataDA.ClassType = "24H";
                    mDailyPivotDataList.Add(m24DailyPivotDataDA);


                    m24DailyPivotDataCrr.RowDisplayType = "Crr";
                    m24DailyPivotDataCrr.Total = total24Crr;
                    m24DailyPivotDataCrr.Average = total24Crr / hr24;
                    m24DailyPivotDataCrr.ClassType = "24H";
                    mDailyPivotDataList.Add(m24DailyPivotDataCrr);


                    m24DailyPivotDataDACrr.RowDisplayType = "DA-Crr";
                    m24DailyPivotDataDACrr.Total = total24daCrr;
                    m24DailyPivotDataDACrr.Average = total24daCrr / hr24;
                    m24DailyPivotDataDACrr.ClassType = "24H";
                    mDailyPivotDataList.Add(m24DailyPivotDataDACrr);
                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        if (Hour24Checked)
                        {
                            mConsolidatedData.DAValue = total24da;
                            mConsolidatedData.RTValue = total24krt;
                            mConsolidatedData.CrrValue = total24Crr;
                            mConsolidatedData.DACrrValue = total24daCrr;
                            if (MonthlyChecked)
                                mGraphList.Add(mConsolidatedData);
                        }
                    }
                    else
                    {
                        if (Hour24Checked)
                        {
                            mConsolidatedData.DAValue = total24da;
                            mConsolidatedData.RTValue = total24krt;
                            mConsolidatedData.CrrValue = total24Crr;
                            mConsolidatedData.DACrrValue = total24daCrr;
                            if (MonthlyChecked)
                                mGraphList.Add(mConsolidatedData);
                        }
                    }
                    #endregion

                    #region PeakWE
                    mPeakWEDailyPivotDataRT.RowDisplayType = "RT";
                    mPeakWEDailyPivotDataRT.RowName = "";
                    mPeakWEDailyPivotDataRT.Total = totalPeakrt;
                    mPeakWEDailyPivotDataRT.Average = totalPeakrt / peakhrs;
                    mPeakWEDailyPivotDataRT.ClassType = "PeakWE";
                    mDailyPivotDataList.Add(mPeakWEDailyPivotDataRT);


                    mPeakWEDailyPivotDataDA.RowDisplayType = "DA";
                    mPeakWEDailyPivotDataDA.Total = totalPeakda;
                    mPeakWEDailyPivotDataDA.Average = totalPeakda / peakhrs;
                    mPeakWEDailyPivotDataDA.ClassType = "PeakWE";
                    mDailyPivotDataList.Add(mPeakWEDailyPivotDataDA);


                    mPeakWEDailyPivotDataCrr.RowDisplayType = "Crr";
                    mPeakWEDailyPivotDataCrr.Total = totalPeakCrr;
                    mPeakWEDailyPivotDataCrr.Average = totalPeakCrr / peakhrs;
                    mPeakWEDailyPivotDataCrr.ClassType = "PeakWE";
                    mDailyPivotDataList.Add(mPeakWEDailyPivotDataCrr);


                    mPeakWEDailyPivotDataDACrr.RowDisplayType = "DA-Crr";
                    mPeakWEDailyPivotDataDACrr.Total = totalPeakdaCrr;
                    mPeakWEDailyPivotDataDACrr.Average = totalPeakdaCrr / peakhrs;
                    mPeakWEDailyPivotDataDACrr.ClassType = "PeakWE";
                    mDailyPivotDataList.Add(mPeakWEDailyPivotDataDACrr);
                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        if (PeakWE)
                        {
                            mConsolidatedData.DAValue = totalPeakda;
                            mConsolidatedData.RTValue = totalPeakrt;
                            mConsolidatedData.CrrValue = totalPeakCrr;
                            mConsolidatedData.DACrrValue = totalPeakdaCrr * 1;
                            if (MonthlyChecked)
                                mGraphList.Add(mConsolidatedData);
                        }
                    }
                    #endregion



                }
            }
            return mDailyPivotDataList;
        }
        private void SetDailyPivotList()
        {
            mDailyPivotDataList = GetCrrServiceData();
            mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>();
            ConsolidatedData mConsolidatedData = new ConsolidatedData();
            if (mDailyPivotDataList.Count > 0)
            {
                if (PeakWE)
                {
                    if (FilterDayComparisonDAChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "DA"));
                    }
                    if (FilterDayComparisonRTChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "RT"));
                    }
                    if (FilterDayComparisonCrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "Crr"));
                    }
                    if (FilterDayComparisonDACrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "DA-Crr"));
                    }
                }
                if (OnPeakChecked)
                {
                    if (FilterDayComparisonDAChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "DA"));
                    }
                    if (FilterDayComparisonRTChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "RT"));
                    }
                    if (FilterDayComparisonCrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "Crr"));
                    }
                    if (FilterDayComparisonDACrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "DA-Crr"));
                    }
                }
                if (OffPeakChecked)
                {
                    if (FilterDayComparisonDAChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "DA"));
                    }
                    if (FilterDayComparisonRTChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "RT"));
                    }
                    if (FilterDayComparisonCrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "Crr"));
                    }
                    if (FilterDayComparisonDACrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "DA-Crr"));
                    }

                }
                if (Hour24Checked)
                {

                    if (FilterDayComparisonDAChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "DA"));
                    }
                    if (FilterDayComparisonRTChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "RT"));
                    }
                    if (FilterDayComparisonCrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "Crr"));
                    }
                    if (FilterDayComparisonDACrrChecked)
                    {
                        mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "DA-Crr"));
                    }


                }
            }
            if (FilterList != null)
            {
                foreach (FilterData filterData in FilterList)
                {
                    ObservableCollection<DailyPivotData> mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>();
                    if (!FilterDayComparisonCrrChecked)
                    {
                        if (PeakWE)
                        {
                            if (FilterDayComparisonDAChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "DA"));
                            }
                            if (FilterDayComparisonRTChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "RT"));
                            }
                            if (FilterDayComparisonDACrrChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "DA-Crr"));
                            }
                            mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "PeakWE" && x.RowDisplayType == "Crr"));
                        }

                        if (OnPeakChecked)
                        {
                            if (FilterDayComparisonDAChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "DA"));
                            }
                            if (FilterDayComparisonRTChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "RT"));
                            }
                            if (FilterDayComparisonDACrrChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "DA-Crr"));
                            }
                            mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "Peak" && x.RowDisplayType == "Crr"));
                        }
                        if (OffPeakChecked)
                        {
                            if (FilterDayComparisonDAChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "DA"));
                            }
                            if (FilterDayComparisonRTChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "RT"));
                            }
                            if (FilterDayComparisonDACrrChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "DA-Crr"));
                            }
                            mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "OffPeak" && x.RowDisplayType == "Crr"));
                        }
                        if (Hour24Checked)
                        {
                            if (FilterDayComparisonDAChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "DA"));
                            }
                            if (FilterDayComparisonRTChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "RT"));
                            }
                            if (FilterDayComparisonDACrrChecked)
                            {
                                mDailyFinalNonCrrPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "DA-Crr"));
                            }
                            mDailyFinalPivotDataList = new ObservableCollection<DailyPivotData>(mDailyPivotDataList.Where(x => x.ClassType == "24H" && x.RowDisplayType == "Crr"));
                        }
                        double? min = filterData.Min == null ? double.MinValue : filterData.Min;
                        double? max = filterData.Max == null ? double.MaxValue : filterData.Max;
                        mDailyFinalPivotDataList = Filter(mDailyFinalNonCrrPivotDataList, mDailyFinalPivotDataList, filterData.Product, filterData.Type, min, max);
                    }
                    else
                    {
                        double? min = filterData.Min == null ? double.MinValue : filterData.Min;
                        double? max = filterData.Max == null ? double.MaxValue : filterData.Max;
                        //double? min = filterData.Min == null ? 0 : filterData.Min;
                        //double? max = filterData.Max == null ? 0 : filterData.Max;
                        mDailyFinalPivotDataList = Filter(mDailyFinalNonCrrPivotDataList, mDailyFinalPivotDataList, filterData.Product, filterData.Type, min, max);
                    }
                }
            }
            DailyPivotList = mDailyFinalPivotDataList;
            List<DailyPivotData> dailyPivotDataList = mDailyFinalPivotDataList.ToList();
            AssignDisplayTypeRowName(dailyPivotDataList);
            DailyPivotList = new ObservableCollection<DailyPivotData>(dailyPivotDataList);
            SetDailySummary(dailyPivotDataList);
            RetrieveFetchDataAndUpdateChartCommand();
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

        /// <summary>
        /// Gets the selected dates.
        /// </summary>
        /// <returns></returns>
        private List<DateTime> GetSelectedDates()
        {
            List<DateTime> dateList = new List<DateTime>();
            if (JanChecked)
            {
                int month = 1;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (FebChecked)
            {
                int month = 2;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (MarChecked)
            {
                int month = 3;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (AprChecked)
            {
                int month = 4;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (MayChecked)
            {
                int month = 5;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (JunChecked)
            {
                int month = 6;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (JulChecked)
            {
                int month = 7;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (AugChecked)
            {
                int month = 8;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (SepChecked)
            {
                int month = 9;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (OctChecked)
            {
                int month = 10;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (NovChecked)
            {
                int month = 11;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            if (DecChecked)
            {
                int month = 12;
                DateTime tempDt = DateTime.Now;
                DateTime date;
                if (month <= tempDt.Month)
                    date = DateTime.Parse(month + "/1/" + tempDt.Year);
                else
                    date = DateTime.Parse(month + "/1/" + tempDt.AddYears(-1).Year);
                dateList.Add(date);
            }
            return dateList;
        }

        /// <summary>
        /// Assigns the display name of the row type.
        /// </summary>
        /// <param name="dailyPivotDataList">The daily pivot data list.</param>
        /// <param name="calculateAverage">if set to <c>true</c> [calculate average].</param>
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

        /// <summary>
        /// Sets the daily summary.
        /// </summary>
        /// <param name="dailyPivotDataList">The daily pivot data list.</param>
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
            //DailyPivotData minRow = new DailyPivotData() { RowType = "Min", Total = dailyPivotDataList.Min(x => x.Total), Average = dailyPivotDataList.Sum(x => x.Average), SummaryType = SummaryRowType.Min };
            //DailyPivotData maxRow = new DailyPivotData() { RowType = "Max", Total = dailyPivotDataList.Max(x => x.Total), Average = dailyPivotDataList.Sum(x => x.Average), SummaryType = SummaryRowType.Max };
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

            AssignDisplayTypeRowName(summaryList, false);
            DailySummaryPivotList = new ObservableCollection<DailyPivotData>(summaryList);
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

        internal void Paste()
        {
            try
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                DataService objDataService = new DataService();
                IDataObject iData = Clipboard.GetDataObject();
                if (!iData.GetDataPresent(DataFormats.Text))
                {
                    return;
                }
                string text = (string)Clipboard.GetData(DataFormats.Text);
                if (text.Contains("\r"))
                {
                    text = text.Replace("\r", "");
                }
                int length = text.Length;
                if (text.Contains("\r\n"))
                {
                    text = text.Replace("\r\n", "");
                }
                if (text == null)
                {
                    return;
                }
                string[] rowData = text.Split('\n');
                if (rowData.Count() == 2)
                {
                    List<PasteHelper> helperDataList = new List<PasteHelper>();
                    string[] cellData = null;
                    if (rowData[0].ToString() != string.Empty)
                    {
                        cellData = rowData[0].ToString().Split('\t');
                        if (cellData.Length == 7)
                        {
                            helperDataList.Add(new PasteHelper
                            {
                                Source = cellData[0].Trim(),
                                Sink = cellData[1].Trim(),
                                Class = cellData[3].Trim(),
                                Price = Convert.ToDouble(cellData[6].Trim())
                            });
                        }
                        else if (cellData.Length == 4)
                        {
                            helperDataList.Add(new PasteHelper
                            {
                                Source = cellData[0].Trim(),
                                Sink = cellData[1].Trim(),
                                Class = cellData[3].Trim(),
                            });
                        }
                        else if (cellData.Length == 3)
                        {
                            helperDataList.Add(new PasteHelper
                            {
                                Source = cellData[0].Trim(),
                                Sink = cellData[1].Trim(),
                                Class = cellData[2].Trim()
                            });
                        }
                        else if (cellData.Length == 2)
                        {
                            helperDataList.Add(new PasteHelper
                            {
                                Source = cellData[0].Trim(),
                                Sink = cellData[1].Trim(),
                            });
                        }
                    }
                    if (helperDataList.Count > 0)
                    {
                        FixSelectedSourceSinks(helperDataList[0]);
                    }

                }
                if (rowData.Count() < 0)
                {
                    MessageBox.Show("No data to paste");
                    return;
                }
                List<PasteHelper> helperDataList1 = new List<PasteHelper>();
                if (rowData.Count() >= 2)
                {
                    for (int i = 0; i < rowData.Count(); i++)
                    {
                        string[] cellData = null;
                        if (rowData[i].ToString() != string.Empty)
                        {
                            cellData = rowData[i].ToString().Split('\t');
                            if ((((cellData.Length == 2) || (cellData.Length == 3)) || (((cellData.Length == 4) || (cellData.Length == 5)))) || ((cellData.Length == 6) || (cellData.Length == 7)))
                            {
                                helperDataList1.Add(new PasteHelper
                                {
                                    Source = cellData[0].Trim(),
                                    Sink = cellData[1].Trim(),
                                });
                            }
                        }
                    }
                    for (int k = 0; k < helperDataList1.Count; k++)
                    {
                        if (helperDataList1.Count != 1)
                        {
                            var sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == helperDataList1[k].Source.ToLower()).FirstOrDefault();
                            var sinkData = SinkNodeList.Where(a => a.NodeName.ToLower() == helperDataList1[k].Sink.ToLower()).FirstOrDefault();
                            SourceComboSelectedItem = sourceData;
                            SinkComboSelectedItem = sinkData;
                            string sourceName = helperDataList1[k].Source;
                            string sinkName = helperDataList1[k].Sink;
                            if (sourceData == null)
                            {
                                string[] sArray = helperDataList1[k].Source.Split(' ');

                                string tempSourceName = string.Empty;

                                foreach (string tempString in sArray)
                                {
                                    tempSourceName += "%" + tempString.Trim() + "%";
                                }

                                helperDataList1[k].Source = objDataService.GetSpecificNodeName(tempSourceName);

                                sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == helperDataList1[k].Source.ToLower()).FirstOrDefault();
                            }

                            if (sinkData == null)
                            {
                                string[] sArray = helperDataList1[k].Sink.Split(' ');

                                string tempSinkName = string.Empty;

                                foreach (string tempString in sArray)
                                {
                                    tempSinkName += "%" + tempString.Trim() + "%";
                                }

                                helperDataList1[k].Sink = objDataService.GetSpecificNodeName(tempSinkName);

                                sinkData = SinkNodeList.Where(a => a.NodeName.ToLower() == helperDataList1[k].Sink.ToLower()).FirstOrDefault();
                            }


                            if ((sourceData == null || sinkData == null) && helperDataList1[k].Source != null && helperDataList1[k].Sink != null)
                            {
                                pnodeIdHash = objDataService.FillPnodeHash();
                                if (sourceData == null)
                                {
                                    int externalNodeId = pnodeIdHash.FirstOrDefault(x => x.Value.ToList().Contains(sourceName)).Key;
                                    List<string> NodeNameList = pnodeIdHash[externalNodeId];
                                    foreach (string nodename in NodeNameList)
                                    {
                                        sourceData = SourceNodeList.Where(a => a.NodeName == nodename).FirstOrDefault();
                                        if (sourceData != null)
                                            break;
                                    }
                                }
                                if (sinkData == null)
                                {
                                    int externalNodeId = pnodeIdHash.FirstOrDefault(x => x.Value.ToList().Contains(sinkName)).Key;
                                    List<string> NodeNameList = pnodeIdHash[externalNodeId];
                                    foreach (string nodename in NodeNameList)
                                    {
                                        sinkData = SinkNodeList.Where(a => a.NodeName == nodename).FirstOrDefault();
                                        if (sinkData != null)
                                            break;
                                    }
                                }
                            }
                            SourceComboSelectedItem = sourceData;
                            SinkComboSelectedItem = sinkData;
                            if (SourceComboSelectedItem != null && SinkComboSelectedItem != null)
                            {
                                SourceSinkState sourceSinkData = new SourceSinkState();
                                sourceSinkData.Source = SourceComboSelectedItem;
                                sourceSinkData.Sink = SinkComboSelectedItem;
                                string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                                            sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();

                                if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                                {
                                    mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                                    SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkState>();
                                    mSourceSinkDataSelected = sourceSinkData;
                                    RaisePropertyChanged("SourceSinkDataSelected");
                                }

                                SourceComboSelectedItem = null;
                                SinkComboSelectedItem = null;
                                //InitializeSourceSinkStateAndRetrive(false);
                            }
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }

        private void FixSelectedSourceSinks(PasteHelper helperDataList)
        {
            try
            {
                DataService objDataService = new DataService();

                string sourceName = helperDataList.Source;
                string sinkName = helperDataList.Sink;

                List<SourceSinkState> tempSrcSnkList = new List<SourceSinkState>();
                var sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == helperDataList.Source.ToLower()).FirstOrDefault();
                var sinkData = SinkNodeList.Where(a => a.NodeName.ToLower() == helperDataList.Sink.ToLower()).FirstOrDefault();

                if (sourceData == null)
                {
                    string[] sArray = helperDataList.Source.Split(' ');

                    string tempSourceName = string.Empty;

                    foreach (string tempString in sArray)
                    {
                        tempSourceName += "%" + tempString.Trim() + "%";
                    }

                    helperDataList.Source = objDataService.GetSpecificNodeName(tempSourceName);

                    sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == helperDataList.Source.ToLower()).FirstOrDefault();
                }

                if (sinkData == null)
                {
                    string[] sArray = helperDataList.Sink.Split(' ');

                    string tempSinkName = string.Empty;

                    foreach (string tempString in sArray)
                    {
                        tempSinkName += "%" + tempString.Trim() + "%";
                    }

                    helperDataList.Sink = objDataService.GetSpecificNodeName(tempSinkName);

                    sinkData = SinkNodeList.Where(a => a.NodeName.ToLower() == helperDataList.Sink.ToLower()).FirstOrDefault();
                }


                if ((sourceData == null || sinkData == null) && helperDataList.Source != null && helperDataList.Sink != null)
                {
                    pnodeIdHash = objDataService.FillPnodeHash();
                    if (sourceData == null)
                    {
                        int externalNodeId = pnodeIdHash.FirstOrDefault(x => x.Value.ToList().Contains(sourceName)).Key;
                        List<string> NodeNameList = pnodeIdHash[externalNodeId];
                        foreach (string nodename in NodeNameList)
                        {
                            sourceData = SourceNodeList.Where(a => a.NodeName == nodename).FirstOrDefault();
                            if (sourceData != null)
                                break;
                        }
                    }
                    if (sinkData == null)
                    {
                        int externalNodeId = pnodeIdHash.FirstOrDefault(x => x.Value.ToList().Contains(sinkName)).Key;
                        if (pnodeIdHash.ContainsKey(externalNodeId))
                        {
                            List<string> NodeNameList = pnodeIdHash[externalNodeId];
                            foreach (string nodename in NodeNameList)
                            {
                                sinkData = SinkNodeList.Where(a => a.NodeName == nodename).FirstOrDefault();
                                if (sinkData != null)
                                    break;
                            }
                        }
                    }
                }
                RemoveAllFilters();
                tempSrcSnkList.Add(new SourceSinkState()
                {
                    Source = sourceData,
                    Sink = sinkData
                });
                SourceComboSelectedItem = sourceData;
                SinkComboSelectedItem = sinkData;
                ProductComboSelectedValue = ProductList.Where(a => a.ToLower().Equals("price")).FirstOrDefault();
                TypeComboSelectedValue = TypeList.Where(a => a.ToLower().Equals("Crr")).FirstOrDefault();
                MaxText = helperDataList.Price.ToString();
                if (MaxText != "")
                {
                    FilterList = new List<FilterData>
            {
                new FilterData{
                 Max=helperDataList.Price,
                 Product=ProductComboSelectedValue,
                 Type=TypeComboSelectedValue
                }
            };
                }
                if (helperDataList.Class != null)
                {
                    if (helperDataList.Class.ToUpper() == "ONPEAK" || helperDataList.Class.ToUpper() == "ON-PEAK" || helperDataList.Class.ToUpper() == "ON PEAK" || helperDataList.Class.ToUpper() == "PEAK")
                    {
                        OnPeakChecked = true;
                        //FilterDayComparisonCrrChecked = true;
                    }
                    else if (helperDataList.Class.ToUpper() == "OFFPEAK" || helperDataList.Class.ToUpper() == "OFF-PEAK" || helperDataList.Class.ToUpper() == "OFF PEAK")
                    {
                        OffPeakChecked = true;
                        //FilterDayComparisonCrrChecked = true;
                    }
                    else if (helperDataList.Class.ToUpper() == "24H" || helperDataList.Class.ToUpper() == "24-H" || helperDataList.Class.ToUpper() == "24 H")
                    {
                        Hour24Checked = true;
                        //FilterDayComparisonCrrChecked = true;
                    }
                }

                if (SourceComboSelectedItem != null && SinkComboSelectedItem != null)
                {
                    SourceSinkState sourceSinkData = new SourceSinkState();
                    sourceSinkData.Source = SourceComboSelectedItem;
                    sourceSinkData.Sink = SinkComboSelectedItem;
                    string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                                sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();

                    if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                    {
                        mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                        SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkState>();
                        mSourceSinkDataSelected = sourceSinkData;
                        RaisePropertyChanged("SourceSinkDataSelected");
                    }

                    SourceComboSelectedItem = null;
                    SinkComboSelectedItem = null;
                    InitializeSourceSinkStateAndRetrive(false);
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Selects the source sink data and updates the chart.
        /// </summary>
        public void SelectSourceSinkFetchDataUpdateChart()
        {
            bool refreshData = false;
            FetchAllSourceSinkData(refreshData);
        }
        /// <summary>
        /// Fetches all source sink data.
        /// </summary>
        /// <param name="refreshData">if set to <c>true</c> [refresh data].</param>
        public void FetchAllSourceSinkData(bool refreshData)
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            if (sourceSinkData == null)
            {
                return;
            }
            List<int> sourcesinkList = new List<int>();
            sourcesinkList.Add(sourceSinkData.Source.NodeKey);
            if (sourceSinkData.Sink != null)
            {
                sourcesinkList.Add(sourceSinkData.Sink.NodeKey);
            }

            myDataService.GetNodeCoordinate((nodesLocation, error) =>
            {
                if (error != null)
                {
                    return;
                }
                Locations = nodesLocation;
            }, sourcesinkList);
            Microsoft.Maps.MapControl.WPF.LocationCollection nodeloc = new Microsoft.Maps.MapControl.WPF.LocationCollection();
            foreach (var item in Locations)
            {
                nodeloc.Add(item.MapLocation);
            }
            if (nodeloc.Count > 0)
            {
                mCenterLoc = nodeloc[0].Latitude.ToString() + ":" + nodeloc[0].Longitude.ToString();
                MapCenter = mCenterLoc;
            }
            NodePath = nodeloc;
        }
        /// <summary>
        /// Retrieves the data and update chart command.
        /// </summary>
        /// 
        public void RetrieveFetchDataAndUpdateChartCommand()
        {
            LineSeries CrrLineSeries = new LineSeries();
            LineSeries daLineSeries = new LineSeries();
            LineSeries rtLineSeries = new LineSeries();

            bool refreshData = true;
            FetchAllSourceSinkData(refreshData);
            AddSourceSink();
            //yearHash
            // Task.Factory.StartNew(() => { FetchAllSourceSinkData(refreshData); });

            if (SourceSinkDataSelected == null)
                return;
            List<ConsolidatedData> consolidatedList = new List<ConsolidatedData>();
            List<ConsolidatedData> tempConsolidatedList = new List<ConsolidatedData>();
            tempConsolidatedList = mGraphList.OrderBy(x => x.StartDate).ToList();
            //if (OnPeakChecked)
            //    tempConsolidatedList = myDataService.GetConsolidatedList(SourceSinkDataSelected, "PEAK", "Daily");
            //else if (OffPeakChecked)
            //    tempConsolidatedList = myDataService.GetConsolidatedList(SourceSinkDataSelected, "OFFPEAK", "Daily");
            //else if (Hour24Checked)
            //    tempConsolidatedList = myDataService.GetConsolidatedList(SourceSinkDataSelected, "24HR", "Daily");
            List<DateTime> dateList = new List<DateTime>();
            List<ConsolidatedData> tempConsolidatedList1 = new List<ConsolidatedData>();
            if (!allMonthsSelected)
            {
                dateList = GetSelectedDates();
                foreach (var item in tempConsolidatedList)
                {
                    DateTime tempDt = item.StartDate;
                    foreach (DateTime date in dateList)
                    {
                        if (tempDt.Month == date.Month)
                            tempConsolidatedList1.Add(item);
                    }
                }
                tempConsolidatedList = tempConsolidatedList1;
            }
            if (tempConsolidatedList == null || tempConsolidatedList.Count == 0)
            {
                PlotModelUpper = null;
                PlotModelLower = null;
                return;
            }

            if (DailyChecked)
                consolidatedList = tempConsolidatedList;
            else
            {
                int j = 0;
                foreach (var item in tempConsolidatedList.GroupBy(x => x.StartDate.Year.ToString() + x.StartDate.Month))
                {

                    int count = 0;
                    double? daCrrvalue = 0.0;
                    double? daValue = 0.0;
                    double? CrrValue = 0.0;
                    double? rtValue = 0.0;
                    ConsolidatedData data = new ConsolidatedData();
                    foreach (var itemData in item)
                    {
                        data.StartDate = new DateTime(itemData.StartDate.Year, itemData.StartDate.Month, 1);
                        if (itemData.DACrrValue == null)
                        {
                            itemData.DACrrValue = 0;
                            daCrrvalue += (itemData.DACrrValue);
                        }
                        else
                            daCrrvalue += (itemData.DACrrValue);

                        if (itemData.DAValue == null)
                        {
                            itemData.DAValue = 0;
                            daValue += (itemData.DAValue);
                        }
                        else
                            daValue += (itemData.DAValue);


                        if (itemData.CrrValue == null)
                        {
                            itemData.CrrValue = 0;
                            CrrValue += (itemData.CrrValue);
                        }
                        else
                            CrrValue += (itemData.CrrValue);

                        if (itemData.RTValue == null)
                        {
                            itemData.RTValue = 0;
                            rtValue += (itemData.RTValue);
                        }
                        else
                            rtValue += (itemData.RTValue);
                        data.Index = j;
                        count++;
                    }
                    if (count == 0)
                        continue;

                    data.DACrrValue = Math.Round((daCrrvalue.Value / count), 2);
                    data.DAValue = Math.Round(daValue.Value / count, 2);
                    data.RTValue = Math.Round(rtValue.Value / count, 2);
                    data.CrrValue = Math.Round(CrrValue.Value / count, 2);
                    consolidatedList.Add(data);
                    j++;
                }
            }
            string title = SourceSinkDataSelected.Source.NodeName + "(" + SourceSinkDataSelected.Source.Zone + ")" + " -> " + SourceSinkDataSelected.Sink.NodeName + "(" + SourceSinkDataSelected.Sink.Zone + ")";
            var plotModel1 = new PlotModel { Title = title };

            var c = OxyColors.DarkBlue;


            LinearAxis YAxisL = new LinearAxis()
            {
                Key = "YAxisL",
                Position = AxisPosition.Left,
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
            };
            plotModel1.Axes.Add(YAxisL);

            LinearAxis YAxisR = new LinearAxis()
            {
                Key = "YAxisR",
                Position = AxisPosition.Right,
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
            };
            plotModel1.Axes.Add(YAxisR);

            CategoryAxis catAxis =
            new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 90,
                StringFormat = "0",

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
            catAxis.MajorStep = 1;
            //if (DailyChecked)
            //{
            //    catAxis.MajorStep = 25;
            //}
            //else
            //{
            //    catAxis.MajorStep = 1;
            //}
            plotModel1.Axes.Add(catAxis);


            colSeries1 = new BarSeries()
            {
                Title = "PNL",
                YAxisKey = "XAxisCategory",
                XAxisKey = "YAxisL",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,



            };
            var CrrSeries = new List<Item>();
            //LineSeries CrrLineSeries = new LineSeries();

            var daSeries = new List<Item>();
            //LineSeries daLineSeries = new LineSeries();

            var rtSeries = new List<Item>();
            //LineSeries rtLineSeries = new LineSeries();

            CrrLineSeries.CanTrackerInterpolatePoints = false;
            CrrLineSeries.Title = "Crr";
            CrrLineSeries.DataFieldX = "X";
            CrrLineSeries.DataFieldY = "Y";
            //CrrLineSeries.//TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}",
            CrrLineSeries.TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}";
            CrrLineSeries.XAxisKey = "XAxisCategory";
            CrrLineSeries.YAxisKey = "YAxisR";
            CrrLineSeries.MarkerType = MarkerType.Diamond;
            CrrLineSeries.MarkerSize = 2;
            CrrLineSeries.MarkerStrokeThickness = 0;

            daLineSeries.CanTrackerInterpolatePoints = false;
            daLineSeries.Title = "DA";
            daLineSeries.DataFieldX = "X";
            daLineSeries.DataFieldY = "Y";
            daLineSeries.TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}";
            daLineSeries.XAxisKey = "XAxisCategory";
            daLineSeries.YAxisKey = "YAxisL";
            daLineSeries.MarkerType = MarkerType.Diamond;
            daLineSeries.MarkerSize = 2;
            daLineSeries.MarkerStrokeThickness = 0;

            rtLineSeries.CanTrackerInterpolatePoints = false;
            rtLineSeries.Title = "RT";
            rtLineSeries.DataFieldX = "X";
            rtLineSeries.DataFieldY = "Y";
            rtLineSeries.TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}";
            rtLineSeries.XAxisKey = "XAxisCategory";
            rtLineSeries.YAxisKey = "YAxisL";
            rtLineSeries.MarkerType = MarkerType.Diamond;
            rtLineSeries.MarkerSize = 2;
            rtLineSeries.MarkerStrokeThickness = 0;

            for (int i = 0; i < consolidatedList.Count; i++)
            {
                if (DailyChecked)
                    catAxis.Labels.Add(consolidatedList[i].StartDate.ToString("yyyy-MM-dd"));
                else
                    catAxis.Labels.Add(consolidatedList[i].StartDate.ToString("yyyy-MM"));
                if (DailyChecked)
                {

                    try
                    {
                        if (consolidatedList[i].DACrrValue != null)
                        {
                            var colItem1 = new BarItem(consolidatedList[i].DACrrValue.Value, consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index);
                            colSeries1.Items.Add(colItem1);
                        }
                        if (consolidatedList[i].CrrValue != null)
                        {
                            Item mItem = new Item();
                            mItem.X = consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index;
                            mItem.Y = consolidatedList[i].CrrValue.Value;
                            CrrSeries.Add(mItem);
                        }

                        if (consolidatedList[i].DAValue != null)
                        {
                            Item mItemda = new Item();
                            mItemda.X = consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index;
                            mItemda.Y = consolidatedList[i].DAValue.Value;
                            daSeries.Add(mItemda);
                        }
                        if (consolidatedList[i].RTValue != null)
                        {
                            Item mItemrt = new Item();
                            mItemrt.X = consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index;
                            mItemrt.Y = consolidatedList[i].RTValue.Value;
                            rtSeries.Add(mItemrt);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    if (consolidatedList[i].DACrrValue != null)
                    {
                        var colItem1 = new BarItem(consolidatedList[i].DACrrValue.Value, consolidatedList[i].Index);
                        colSeries1.Items.Add(colItem1);

                        Item mItem = new Item();
                        mItem.X = consolidatedList.OrderBy(x => x.StartDate).ToList()[i].Index;
                        mItem.Y = consolidatedList[i].CrrValue.Value;
                        CrrSeries.Add(mItem);

                        Item mItemda = new Item();
                        mItemda.X = consolidatedList.OrderBy(x => x.StartDate).ToList()[i].Index;
                        mItemda.Y = consolidatedList[i].DAValue.Value;
                        daSeries.Add(mItemda);

                        Item mItemrt = new Item();
                        mItemrt.X = consolidatedList.OrderBy(x => x.StartDate).ToList()[i].Index;
                        mItemrt.Y = consolidatedList[i].RTValue.Value;
                        rtSeries.Add(mItemrt);
                    }
                    else
                    {

                    }
                }
            }

            CrrLineSeries.ItemsSource = CrrSeries;
            rtLineSeries.ItemsSource = rtSeries;
            daLineSeries.ItemsSource = daSeries;
            if (isPNLChecked)
            {
                plotModel1.Series.Add(colSeries1);
            }
            if (isCrrChecked)
            {
                plotModel1.Series.Add(CrrLineSeries);
            }
            if (isDAChecked)
            {
                plotModel1.Series.Add(daLineSeries);
            }
            if (isRTChecked)
            {
                plotModel1.Series.Add(rtLineSeries);
            }

            //plotModel1.Series.OrderByDescending(x => x.PlotModel);
            double CrrMax = Math.Max(CrrSeries.Max(x => x.Y).Value, Math.Abs(CrrSeries.Min(x => x.Y).Value));
            double rtMax = Math.Max(rtSeries.Max(x => x.Y).Value, Math.Abs(CrrSeries.Min(x => x.Y).Value));
            double daMax = Math.Max(daSeries.Max(x => x.Y).Value, Math.Abs(CrrSeries.Min(x => x.Y.Value)));
            double daCrrMax = Math.Max(colSeries1.Items.Max(x => x.Value), Math.Abs(CrrSeries.Min(x => x.Y.Value)));

            daMax = Math.Max(Math.Max(CrrMax, Math.Max(daMax, rtMax)), daCrrMax);

            YAxisL.Maximum = daMax * 1.3;
            YAxisL.Minimum = daMax * 1.3 * -1;
            YAxisR.Maximum = daMax * 1.3;
            YAxisR.Minimum = daMax * 1.3 * -1;
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight,
                LegendOrientation = LegendOrientation.Horizontal

            };

            plotModel1.Legends.Add(l);

            PlotModelUpper = plotModel1;
            PlotModelLower = CreateModelLower(consolidatedList);
        }
        public void RetrieveFetchDataAndUpdateChartCommand1()
        {
            bool refreshData = true;
            FetchAllSourceSinkData(refreshData);
            AddSourceSink();
            //yearHash
            // Task.Factory.StartNew(() => { FetchAllSourceSinkData(refreshData); });

            if (SourceSinkDataSelected == null)
                return;
            List<ConsolidatedData> consolidatedList = new List<ConsolidatedData>();
            List<ConsolidatedData> tempConsolidatedList = new List<ConsolidatedData>();
            tempConsolidatedList = mGraphList.OrderBy(x => x.StartDate).ToList();
            //if (OnPeakChecked)
            //    tempConsolidatedList = myDataService.GetConsolidatedList(SourceSinkDataSelected, "PEAK", "Daily");
            //else if (OffPeakChecked)
            //    tempConsolidatedList = myDataService.GetConsolidatedList(SourceSinkDataSelected, "OFFPEAK", "Daily");
            //else if (Hour24Checked)
            //    tempConsolidatedList = myDataService.GetConsolidatedList(SourceSinkDataSelected, "24HR", "Daily");
            List<DateTime> dateList = new List<DateTime>();
            List<ConsolidatedData> tempConsolidatedList1 = new List<ConsolidatedData>();
            if (!allMonthsSelected)
            {
                dateList = GetSelectedDates();
                foreach (var item in tempConsolidatedList)
                {
                    DateTime tempDt = item.StartDate;
                    foreach (DateTime date in dateList)
                    {
                        if (tempDt.Month == date.Month)
                            tempConsolidatedList1.Add(item);
                    }
                }
                tempConsolidatedList = tempConsolidatedList1;
            }
            if (tempConsolidatedList == null || tempConsolidatedList.Count == 0)
            {
                PlotModelUpper = null;
                PlotModelLower = null;
                return;
            }

            if (DailyChecked)
                consolidatedList = tempConsolidatedList;
            else
            {
                int j = 0;
                foreach (var item in tempConsolidatedList.GroupBy(x => x.StartDate.Year.ToString() + x.StartDate.Month))
                {

                    int count = 0;
                    double? daCrrvalue = 0.0;
                    double? daValue = 0.0;
                    double? CrrValue = 0.0;
                    double? rtValue = 0.0;
                    ConsolidatedData data = new ConsolidatedData();
                    foreach (var itemData in item)
                    {
                        data.StartDate = new DateTime(itemData.StartDate.Year, itemData.StartDate.Month, 1);
                        if (itemData.DACrrValue == null)
                        {
                            itemData.DACrrValue = 0;
                            daCrrvalue += (itemData.DACrrValue);
                        }
                        else
                            daCrrvalue += (itemData.DACrrValue);

                        if (itemData.DAValue == null)
                        {
                            itemData.DAValue = 0;
                            daValue += (itemData.DAValue);
                        }
                        else
                            daValue += (itemData.DAValue);


                        if (itemData.CrrValue == null)
                        {
                            itemData.CrrValue = 0;
                            CrrValue += (itemData.CrrValue);
                        }
                        else
                            CrrValue += (itemData.CrrValue);

                        if (itemData.RTValue == null)
                        {
                            itemData.RTValue = 0;
                            rtValue += (itemData.RTValue);
                        }
                        else
                            rtValue += (itemData.RTValue);
                        data.Index = j;
                        count++;
                    }
                    if (count == 0)
                        continue;

                    data.DACrrValue = Math.Round((daCrrvalue.Value / count), 2);
                    data.DAValue = Math.Round(daValue.Value / count, 2);
                    data.RTValue = Math.Round(rtValue.Value / count, 2);
                    data.CrrValue = Math.Round(CrrValue.Value / count, 2);
                    consolidatedList.Add(data);
                    j++;
                }
            }
            string title = SourceSinkDataSelected.Source.NodeName + "(" + SourceSinkDataSelected.Source.Zone + ")" + " -> " + SourceSinkDataSelected.Sink.NodeName + "(" + SourceSinkDataSelected.Sink.Zone + ")";
            var plotModel1 = new PlotModel { Title = title };

            var c = OxyColors.DarkBlue;


            LinearAxis YAxisL = new LinearAxis()
            {
                Key = "YAxisL",
                Position = AxisPosition.Left,
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
            };
            plotModel1.Axes.Add(YAxisL);

            LinearAxis YAxisR = new LinearAxis()
            {
                Key = "YAxisR",
                Position = AxisPosition.Right,
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
            };
            plotModel1.Axes.Add(YAxisR);

            CategoryAxis catAxis =
            new CategoryAxis()
            {
                MajorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Bottom,
                MajorGridlineColor = OxyColor.FromAColor(20, c),
                Angle = 90,
                StringFormat = "0",

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
            catAxis.MajorStep = 1;
            plotModel1.Axes.Add(catAxis);
            colSeries1 = new BarSeries()
            {
                Title = "PNL",
                YAxisKey = "XAxisCategory",
                XAxisKey = "YAxisL",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,
            };

            var CrrSeries = new List<Item>();
            //LineSeries CrrLineSeries = new LineSeries();

            var daSeries = new List<Item>();
            //LineSeries daLineSeries = new LineSeries();

            var rtSeries = new List<Item>();
            //LineSeries rtLineSeries = new LineSeries();

            CrrLineSeries.CanTrackerInterpolatePoints = false;
            CrrLineSeries.Title = "Crr";
            CrrLineSeries.DataFieldX = "X";
            CrrLineSeries.DataFieldY = "Y";
            //CrrLineSeries.//TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}",
            CrrLineSeries.TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}";
            CrrLineSeries.XAxisKey = "XAxisCategory";
            CrrLineSeries.YAxisKey = "YAxisL";
            CrrLineSeries.MarkerType = MarkerType.Diamond;
            CrrLineSeries.MarkerSize = 2;
            CrrLineSeries.MarkerStrokeThickness = 0;

            daLineSeries.CanTrackerInterpolatePoints = false;
            daLineSeries.Title = "DA";
            daLineSeries.DataFieldX = "X";
            daLineSeries.DataFieldY = "Y";
            daLineSeries.TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}";
            daLineSeries.XAxisKey = "XAxisCategory";
            daLineSeries.YAxisKey = "YAxisL";
            daLineSeries.MarkerType = MarkerType.Diamond;
            daLineSeries.MarkerSize = 2;
            daLineSeries.MarkerStrokeThickness = 0;

            rtLineSeries.CanTrackerInterpolatePoints = false;
            rtLineSeries.Title = "RT";
            rtLineSeries.DataFieldX = "X";
            rtLineSeries.DataFieldY = "Y";
            rtLineSeries.TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0.00}";
            rtLineSeries.XAxisKey = "XAxisCategory";
            rtLineSeries.YAxisKey = "YAxisL";
            rtLineSeries.MarkerType = MarkerType.Diamond;
            rtLineSeries.MarkerSize = 2;
            rtLineSeries.MarkerStrokeThickness = 0;

            for (int i = 0; i < consolidatedList.Count; i++)
            {
                if (DailyChecked)
                    catAxis.Labels.Add(consolidatedList[i].StartDate.ToString("yyyy-MM-dd"));
                else
                    catAxis.Labels.Add(consolidatedList[i].StartDate.ToString("yyyy-MM"));
                if (DailyChecked)
                {

                    try
                    {
                        if (consolidatedList[i].DACrrValue != null)
                        {
                            var colItem1 = new BarItem(consolidatedList[i].DACrrValue.Value, consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index);
                            colSeries1.Items.Add(colItem1);
                        }
                        if (consolidatedList[i].CrrValue != null)
                        {
                            Item mItem = new Item();
                            mItem.X = consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index;
                            mItem.Y = consolidatedList[i].CrrValue.Value;
                            CrrSeries.Add(mItem);
                        }

                        if (consolidatedList[i].DAValue != null)
                        {
                            Item mItemda = new Item();
                            mItemda.X = consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index;
                            mItemda.Y = consolidatedList[i].DAValue.Value;
                            daSeries.Add(mItemda);
                        }
                        if (consolidatedList[i].RTValue != null)
                        {
                            Item mItemrt = new Item();
                            mItemrt.X = consolidatedList.OrderByDescending(x => x.StartDate).ToList()[i].Index;
                            mItemrt.Y = consolidatedList[i].RTValue.Value;
                            rtSeries.Add(mItemrt);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    if (consolidatedList[i].DACrrValue != null)
                    {
                        var colItem1 = new BarItem(consolidatedList[i].DACrrValue.Value, consolidatedList[i].Index);
                        colSeries1.Items.Add(colItem1);

                        Item mItem = new Item();
                        mItem.X = consolidatedList.OrderBy(x => x.StartDate).ToList()[i].Index;
                        mItem.Y = consolidatedList[i].CrrValue.Value;
                        CrrSeries.Add(mItem);

                        Item mItemda = new Item();
                        mItemda.X = consolidatedList.OrderBy(x => x.StartDate).ToList()[i].Index;
                        mItemda.Y = consolidatedList[i].DAValue.Value;
                        daSeries.Add(mItemda);

                        Item mItemrt = new Item();
                        mItemrt.X = consolidatedList.OrderBy(x => x.StartDate).ToList()[i].Index;
                        mItemrt.Y = consolidatedList[i].RTValue.Value;
                        rtSeries.Add(mItemrt);
                    }
                    else
                    {

                    }
                }
            }

            CrrLineSeries.ItemsSource = CrrSeries;
            rtLineSeries.ItemsSource = rtSeries;
            daLineSeries.ItemsSource = daSeries;
            if (isPNLChecked)
            {
                plotModel1.Series.Add(colSeries1);
            }
            if (isCrrChecked)
            {
                plotModel1.Series.Add(CrrLineSeries);
            }
            if (isDAChecked)
            {
                plotModel1.Series.Add(daLineSeries);
            }
            if (isRTChecked)
            {
                plotModel1.Series.Add(rtLineSeries);
            }

            //plotModel1.Series.OrderByDescending(x => x.PlotModel);
            double CrrMax = Math.Max(CrrSeries.Max(x => x.Y).Value, Math.Abs(CrrSeries.Min(x => x.Y).Value));
            double rtMax = Math.Max(rtSeries.Max(x => x.Y).Value, Math.Abs(CrrSeries.Min(x => x.Y).Value));
            double daMax = Math.Max(daSeries.Max(x => x.Y).Value, Math.Abs(CrrSeries.Min(x => x.Y.Value)));
            double daCrrMax = Math.Max(colSeries1.Items.Max(x => x.Value), Math.Abs(CrrSeries.Min(x => x.Y.Value)));

            daMax = Math.Max(Math.Max(CrrMax, Math.Max(daMax, rtMax)), daCrrMax);

            YAxisL.Maximum = daMax * 1.3;
            YAxisL.Minimum = daMax * 1.3 * -1;
            YAxisR.Maximum = daMax * 1.3;
            YAxisR.Minimum = daMax * 1.3 * -1;

            PlotModelUpper = plotModel1;
            PlotModelLower = CreateModelLower(consolidatedList);
        }
        /// <summary>
        /// Updates the view.
        /// </summary>
        /// <param name="shouldLoadDayComparision">if set to <c>true</c> [should load day comparision].</param>
        public void UpdateView(bool shouldLoadDayComparision = true)
        {
            if (SourceSinkDataSelected == null)
                return;

            if (shouldLoadDayComparision)
                SetDailyPivotList();
        }

        /// <summary>
        /// Initializes the source sink state and retrive.
        /// </summary>
        /// <param name="isSwaped">if set to <c>true</c> [is swaped].</param>
        /// <param name="sourceSinkst">The source sinkst.</param>
        public void InitializeSourceSinkStateAndRetrive(bool isSwaped, SourceSinkState sourceSinkst = null)
        {
            if (isSwaped)
            {
                PricingNode source = SourceSinkDataSelected.Sink;
                PricingNode sink = SourceSinkDataSelected.Source;

                SourceSinkDataSelected.Source = source;
                SourceSinkDataSelected.Sink = sink;
                RaisePropertyChanged("SourceSinkDataSelected");
            }
            if (sourceSinkst == null)
                sourceSinkst = SourceSinkDataSelected;

            if (sourceSinkst == null)
                return;

            UpdateView();
        }

        /// <summary>
        /// Adds the source sink.
        /// </summary>
        public void AddSourceSink()
        {
            AddPath(false);
        }

        /// <summary>
        /// Sets the source sink.
        /// </summary>
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

        /// <summary>
        /// Connects to Service.
        /// </summary>
        private void Connect()
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
            ChannelFactory<ISourceSink> pipeFactory = new ChannelFactory<ISourceSink>(myBinding, new EndpointAddress(mEndPoint));
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
        public void AddFilters()
        {
            FilterList = null;
            string filterType = TypeComboSelectedValue;
            double? max = null;
            try
            {
                max = double.Parse(MaxText);
            }
            catch (Exception ex)
            {
            }
            double? min = null;
            try
            {
                min = double.Parse(MinText);
            }
            catch (Exception ex)
            {
            }
            FilterData filterData = new FilterData();
            filterData.Product = ProductComboSelectedValue;
            filterData.Type = filterType;
            filterData.Min = min;
            filterData.Max = max;
            mFillFilterList.Add(filterData);
            FilterList = mFillFilterList;
        }
        private void SetTypes()
        {
            if (ProductComboSelectedValue == null || ProductComboSelectedValue.Length == 0)
            {
                return;
            }
            List<string> typeList = new List<string>();
            if (ProductComboSelectedValue == "Price")
            {
                typeList.Add("RT");
                typeList.Add("DA");
                typeList.Add("Crr");
                //typeList.Add("RT");
                //typeList.Add("RT Cong.");
            }
            TypeList = null;
            TypeList = typeList;
        }
        public void RemoveFilters()
        {
            FilterData filterData = FilterDataSelected;
            mFillFilterList.Remove(filterData);
            FilterList = null;
            FilterList = mFillFilterList;
        }
        public void RemoveAllFilters()
        {
            mFillFilterList = new List<FilterData>();
            FilterList = null;
        }
        #endregion
    }
    public class PasteHelper
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }

        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>        
        //public string Hours { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double? Price { get; set; }
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

}
