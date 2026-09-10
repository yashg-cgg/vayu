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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Vayu.CircularDependancy;
using Vayu.CommonControls;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.ErcotSubmissionLibrary;
using Vayu.NodePriceLibrary;
using Vayu.VirtualBidSubmissionLibrary;
using Vayu.WorkbookStatistics.Model;
using Vayu.WorkbookStatistics.Views;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class MainWindowViewModel : BindableBase, IVirtualCallBack, ISubmitResultCallback
    {

        #region Declaration
        private readonly IDataService _dataService;

        private string _User = Environment.UserName;

        public bool mSortOrder = false;

        private bool mRefreshGraphs = false;

        private List<FilterData> mFillFilterList = new List<FilterData>();

        private List<Portfolio> mFillPortfolioList = new List<Portfolio>();

        private List<DateTime> mHourList = new List<DateTime>();

        private Dictionary<int, Portfolio> mPortfolioHash = new Dictionary<int, Portfolio>();

        private Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> mAsBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();

        private Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> mMustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();

        private Dictionary<string, Dictionary<string, Tuple<DateTime, double>>> mPathRiskHash = new Dictionary<string, Dictionary<string, Tuple<DateTime, double>>>();

        private List<string> mOverrideList = new List<string>();

        private List<int> mCancelNodeList = new List<int>();

        private int mBidId = 0;

        private bool mRefresh = true;

        private bool mRefreshPath = true;

        private DateTime date;

        public static Portfolio CurrentPortfolio;

        private Dictionary<int, List<Exposure>> mPathDetailHash = new Dictionary<int, List<Exposure>>();

        private WorkbookStatistics.Views.UptosPTPBidEntry mPathWindow = null;

        private Vayu.CircularDependancy.IMainApp mainApp;

        private Vayu.WorkbookStatistics.ViewModels.UptosPTPBidEntryViewModel mVwModel = null;

        private Dictionary<string, Dictionary<string, Load>> mLoadHash = new Dictionary<string, Dictionary<string, Load>>();

        private List<string> mWeatherList = new List<string>();

        private Dictionary<string, HourlyPivotData> mHourlySummaryHash = new Dictionary<string, HourlyPivotData>();

        private Dictionary<string, List<Node>> mHourlyPivotHash = new Dictionary<string, List<Node>>();

        Dictionary<DateTime, double> clearedMsHash = new Dictionary<DateTime, double>();

        Dictionary<DateTime, double> absClearedMwDicttemp = new Dictionary<DateTime, double>();

        List<DeenergizedNode> ListDeenergizedNodes = new List<DeenergizedNode>();

        Dictionary<int, List<DateTime>> nodeHashtemp = new Dictionary<int, List<DateTime>>();
        #endregion

        #region Properties

        private string GetPreference()
        {
            string preference = "";
            if ((bool)AsBidRiskChecked == true)
            {
                preference = preference + "asBidRisk?";
            }
            if ((bool)MustTakeRiskChecked == true)
            {
                preference = preference + "mustTakeRisk?";
            }
            if ((bool)AsBidMaxWinChecked == true)
            {
                preference = preference + "asBidMaxWin?";
            }
            if ((bool)MustTakeMaxWinChecked == true)
            {
                preference = preference + "mustTakeMaxWin?";
            }
            if ((bool)AsBidRiskRwdChecked == true)
            {
                preference = preference + "asBidRiskReward?";
            }
            if ((bool)MustTakeRiskRwdChecked == true)
            {
                preference = preference + "mustTakeRiskReward?";
            }
            if ((bool)AsBidSumChecked == true)
            {
                preference = preference + "asBidSum?";
            }
            if ((bool)MustTakeSumChecked == true)
            {
                preference = preference + "mustTakeSum?";
            }
            if ((bool)AsBidWinPctChecked == true)
            {
                preference = preference + "asBidWinPer?";
            }
            if ((bool)MustTakeWinPctChecked == true)
            {
                preference = preference + "mustTakeWinPer?";
            }
            if ((bool)AvgDAChecked == true)
            {
                preference = preference + "avgDaSpread?";
            }
            if ((bool)AvgRTChecked == true)
            {
                preference = preference + "avgRtSpread?";
            }
            if ((bool)DARTChecked == true)
            {
                preference = preference + "avgDart?";
            }
            if ((bool)MinRTChecked == true)
            {
                preference = preference + "minRt?";
            }
            if ((bool)DAMinChecked == true)
            {
                preference = preference + "minDa?";
            }
            if ((bool)MinDARTChecked == true)
            {
                preference = preference + "minDart?";
            }
            if ((bool)MaxDAChecked == true)
            {
                preference = preference + "maxDA?";
            }
            if ((bool)MaxRTChecked == true)
            {
                preference = preference + "maxRT?";
            }
            if ((bool)MaxDARTChecked == true)
            {
                preference = preference + "maxDart?";
            }
            return preference;
        }

        private bool mClearedDateEnable = true;

        private List<ClearedPathsHelper> _ClearedPathList;
        public List<ClearedPathsHelper> ClearedPathList
        {
            get { return _ClearedPathList; }
            set
            {
                _ClearedPathList = value;
                RaisePropertyChanged("ClearedPathList");
            }
        }

        private DateTime _ClearingStartDate;
        public DateTime ClearingStartDate
        {
            get { return _ClearingStartDate; }
            set
            {
                _ClearingStartDate = value;
                RaisePropertyChanged("ClearingStartDate");
            }
        }

        private DateTime _ClearingEndDate;
        public DateTime ClearingEndDate
        {
            get { return _ClearingEndDate; }
            set
            {
                _ClearingEndDate = value;
                RaisePropertyChanged("ClearingEndDate");
            }
        }

        public bool ClearedDateEnable
        {
            get
            {
                return mClearedDateEnable;
            }
            set
            {
                if (mClearedDateEnable != value)
                {
                    mClearedDateEnable = value;
                    RaisePropertyChanged("ClearedDateEnable");
                }
            }
        }

        private DateTime _RiskConstraintDateSelected;
        public DateTime RiskConstraintDateSelected
        {
            get { return _RiskConstraintDateSelected; }
            set
            {
                _RiskConstraintDateSelected = value;
                RaisePropertyChanged("RiskConstraintDateSelected");
            }
        }

        private bool _RiskConstraintDateEnabled;

        public bool RiskConstraintDateEnabled
        {
            get { return _RiskConstraintDateEnabled; }
            set
            {
                _RiskConstraintDateEnabled = value;
                RaisePropertyChanged("RiskConstraintDateEnabled");
            }
        }


        private bool mEndClearedDateEnable = true;

        public bool EndClearedDateEnable
        {
            get
            {
                return mEndClearedDateEnable;
            }
            set
            {
                mEndClearedDateEnable = value;
                RaisePropertyChanged("EndClearedDateEnable");
            }
        }


        private bool mSortMWChecked = true;

        public bool SortMWChecked
        {
            get
            {
                return mSortMWChecked;
            }
            set
            {
                if (mSortMWChecked == false)
                {
                    if (RiskConstraintsChecked)
                    {
                        ClearedDateEnable = false;
                        EndClearedDateEnable = false;
                        AsBidClearedDateEnable = false;
                    }
                    else
                    {
                        ClearedDateEnable = true;
                        EndClearedDateEnable = true;
                        AsBidClearedDateEnable = true;
                    }
                }
                else
                {
                    if (RiskConstraintsChecked)
                    {
                        ClearedDateEnable = false;
                        EndClearedDateEnable = false;
                        AsBidClearedDateEnable = false;
                    }
                    else
                    {
                        ClearedDateEnable = true;
                        EndClearedDateEnable = true;
                        AsBidClearedDateEnable = true;
                    }
                }
                mSortMWChecked = value;
                RaisePropertyChanged("SortMWChecked");
            }
        }

        private bool mSortDollarChecked = false;

        public bool SortDollarChecked
        {
            get
            {
                return mSortDollarChecked;
            }
            set
            {
                if (mSortDollarChecked == false)
                {
                    ClearedDateEnable = false;
                    EndClearedDateEnable = false;
                    AsBidClearedDateEnable = false;
                }
                else
                {
                    if (RiskConstraintsChecked)
                    {
                        ClearedDateEnable = false;
                        EndClearedDateEnable = false;
                        AsBidClearedDateEnable = false;
                    }
                    else
                    {
                        ClearedDateEnable = true;
                        EndClearedDateEnable = true;
                        AsBidClearedDateEnable = true;
                    }
                }
                mSortDollarChecked = value;
                RaisePropertyChanged("SortDollarChecked");
            }
        }

        private Exposure mSelectedConstraintPathValue;

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

        private bool mRiskConstraintsChecked = false;

        public bool RiskConstraintsChecked
        {
            get
            {
                return mRiskConstraintsChecked;
            }
            set
            {
                if (value == false)
                {
                    RiskConstraintDateEnabled = false;
                    if (SortDollarChecked)
                    {
                        ClearedDateEnable = true;
                        EndClearedDateEnable = true;
                        AsBidClearedDateEnable = true;
                    }
                    else
                    {
                        ClearedDateEnable = false;
                        EndClearedDateEnable = false;
                        AsBidClearedDateEnable = false;
                    }
                }
                else
                {
                    ClearedDateEnable = false;
                    EndClearedDateEnable = false;
                    AsBidClearedDateEnable = false;
                    RiskConstraintDateEnabled = true;
                }
                mRiskConstraintsChecked = value;
                RaisePropertyChanged("RiskConstraintsChecked");
            }
        }

        private bool mNewConstraintChecked;
        public bool NewConstraintChecked
        {
            get { return mNewConstraintChecked; }
            set
            {
                if (value == false)
                {
                    if (SortDollarChecked)
                    {
                        ClearedDateEnable = true;
                        EndClearedDateEnable = true;
                        AsBidClearedDateEnable = true;
                    }
                    else
                    {
                        ClearedDateEnable = false;
                        EndClearedDateEnable = false;
                        AsBidClearedDateEnable = false;
                    }
                }
                else
                {
                    ClearedDateEnable = false;
                    EndClearedDateEnable = false;
                    AsBidClearedDateEnable = false;
                }

                mNewConstraintChecked = value;
                RaisePropertyChanged("NewConstraintChecked");
            }
        }

        private bool mStartDateChecked;


        public bool StartDateChecked
        {
            get
            {
                return mStartDateChecked;
            }
            set
            {
                mStartDateChecked = value;
                RaisePropertyChanged("StartDateChecked");
            }
        }


        private bool mOutageChecked;

        public bool OutageChecked
        {
            get
            {
                return mOutageChecked;
            }
            set
            {
                mOutageChecked = value;
                if (mOutageChecked)
                {
                    EnableStartDate = true;
                }
                else
                {
                    EnableStartDate = false;
                }
                RaisePropertyChanged("OutageChecked");
            }
        }

        private string mShadowPrice;

        public string ShadowPrice
        {
            get
            {
                return mShadowPrice;
            }
            set
            {
                mShadowPrice = value;
                RaisePropertyChanged("ShadowPrice");
            }
        }

        private bool mEnableStartDate;

        public bool EnableStartDate
        {
            get
            {
                return mEnableStartDate;
            }
            set
            {
                mEnableStartDate = value;
                RaisePropertyChanged("EnableStartDate");
            }
        }

        private bool isPjmUpto;

        public bool IsPjmUpto
        {
            get { return isPjmUpto; }
            set
            {
                isPjmUpto = value;
                RaisePropertyChanged("IsPjmUpto");
            }
        }

        private bool submissionFileEnabled;

        public bool SubmissionFileEnabled
        {
            get { return submissionFileEnabled; }
            set { submissionFileEnabled = value; RaisePropertyChanged("submissionFileEnabled"); }
        }

        private bool isPjmUptoSink;

        public bool IsPjmUptoSink
        {
            get { return isPjmUptoSink; }
            set
            {
                isPjmUptoSink = value;
                RaisePropertyChanged("IsPjmUptoSink");
            }
        }


        private List<Exposure> mExposureList;

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

        private string mMinConstraintText;

        public string MinConstraintText
        {
            get
            {
                return mMinConstraintText;
            }
            set
            {
                mMinConstraintText = value;
                RaisePropertyChanged("MinConstraintText");
            }
        }

        private string mMaxConstraintText;

        public string MaxConstraintText
        {
            get
            {
                return mMaxConstraintText;
            }
            set
            {
                mMaxConstraintText = value;
                RaisePropertyChanged("MaxConstraintText");
            }
        }

        private List<string> mOutageStartConstraintList;

        public List<string> OutageStartConstraintList
        {
            get
            {
                return
                  mOutageStartConstraintList;
            }
            set
            {
                mOutageStartConstraintList = value;
                RaisePropertyChanged("OutageStartConstraintList");
            }
        }

        private int mOutageStartConstraintCount;

        public int OutageStartConstraintCount
        {
            get
            {
                return mOutageStartConstraintCount;
            }
            set
            {
                mOutageStartConstraintCount = value;
                RaisePropertyChanged("OutageStartConstraintCount");
            }
        }

        private List<SourceSinkData> mSourceSinkList;

        public void SetSourceSinks(List<Tuple<string, string>> sourceSinkList)
        {
            try
            {
                //Vayu.LMPriceWindow.LmpForm.SetSourceSinks(sourceSinkList, 1);
            }
            catch { }
        }

        public void ShowLMPGraphs(string day)
        {
            //Vayu.LMPriceWindow.LmpForm.OpenLmpGraphs(1, Vayu.LMPriceWindow.LmpForm.GetSourceSinks(), DateTime.Today, DateTime.Today);
        }

        public void ShowLMPStatistics()
        {
            // Vayu.LMPriceWindow.LmpForm.OpenLMPStatisticAnalyzer(1, Vayu.LMPriceWindow.LmpForm.GetSourceSinks());
        }

        public bool SelectChange { get; set; }

        private string mMaxText;

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

        private string mMinText;

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

        private string mSubmitVisible;

        public string SubmitVisible
        {
            get
            {
                return mSubmitVisible;
            }
            set
            {
                mSubmitVisible = value;
                RaisePropertyChanged("SubmitVisible");
            }
        }

        private string mCancelVisible;

        public string CancelVisible
        {
            get
            {
                return mCancelVisible;
            }
            set
            {
                mCancelVisible = value;
                RaisePropertyChanged("CancelVisible");
            }
        }

        private string mCreateRequestFileVisible;

        public string CreateRequestFileVisible
        {
            get
            {
                return mCreateRequestFileVisible;
            }
            set
            {
                mCreateRequestFileVisible = value;
                RaisePropertyChanged("CreateRequestFileVisible");
            }
        }

        private string mTypeComboSelectedValue;

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

        private List<string> mTypeList;

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

        private List<string> mProductList;

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

        private string mProductComboSelectedValue;

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

        private string mPathComboSelectedValue;

        public string PathComboSelectedValue
        {
            get
            {
                return mPathComboSelectedValue;
            }
            set
            {
                mPathComboSelectedValue = value;
                SetPathChart();
                RaisePropertyChanged("PathComboSelectedValue");
            }
        }

        private List<string> mCancelList;

        public List<string> CancelList
        {
            get
            {
                return mCancelList;
            }
            set
            {
                mCancelList = value;
                RaisePropertyChanged("CancelList");
            }
        }

        private List<string> mPathComboList;

        public List<string> PathComboList
        {
            get
            {
                return mPathComboList;
            }
            set
            {
                mPathComboList = value;
                RaisePropertyChanged("PathComboList");
            }
        }

        private PlotModel mPathDayPlotModelUpper;

        public PlotModel PathDayPlotModelUpper
        {
            get
            {
                return mPathDayPlotModelUpper;
            }
            set
            {
                mPathDayPlotModelUpper = value;
                RaisePropertyChanged("PathDayPlotModelUpper");
            }
        }

        private PlotModel mPathPlotModelUpper;

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

        private PlotModel mPathPlotModelLower;

        public PlotModel PathPlotModelLower
        {
            get
            {
                return mPathPlotModelLower;
            }
            set
            {
                mPathPlotModelLower = value;
                RaisePropertyChanged("PathPlotModelLower");
            }
        }

        private PlotModel mPlotModelUpper;

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

        private PlotModel mPlotModelLower;

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

        private FilterData mFilterDataSelected;

        public FilterData FilterDataSelected
        {
            get
            {
                return mFilterDataSelected;
            }
            set
            {
                mFilterDataSelected = value;
                RaisePropertyChanged("FilterDataSelected");
            }
        }

        private string mMWText;

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

        private string mINCText;

        public string INCText
        {
            get
            {
                return mINCText;
            }
            set
            {
                mINCText = value;
                RaisePropertyChanged("INCText");
            }
        }

        private string mDECText;

        public string DECText
        {
            get
            {
                return mDECText;
            }
            set
            {
                mDECText = value;
                RaisePropertyChanged("DECText");
            }
        }

        private string mAsBidWinPerText;

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

        private string mMustTakeWinPerText;

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

        private DateTime mSubmitDate;

        public DateTime SubmitDate
        {
            get
            {
                return mSubmitDate;
            }
            set
            {
                mSubmitDate = value;
                RaisePropertyChanged("SubmitDate");
            }
        }

        private string mDAText;

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

        private string mAsBidDolMW;

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

        private string mMustTakeDolMW;

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

        private string mMustTakeRiskReward;

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

        private string mAsBidRiskReward;

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

        private string mCountText;

        public string CountText
        {
            get
            {
                return mCountText;
            }
            set
            {
                mCountText = value;
                RaisePropertyChanged("CountText");
            }
        }

        private string mAsBidRisk;

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

        private string mMustTakeRisk;

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

        private string mAsBidRiskDate;

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

        private string mMustTakeRiskDate;

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

        private string mAsBidMaxDrawDown;

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

        private string mAsBidMaxDrawDownDate;

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

        private string mMustTakeMaxDrawDown;

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

        private string mMustTakeMaxDrawDownDate;

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

        private string mMustTakeWin;

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

        private string mAsBidWin;

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

        private string mMustTakeWinDate;

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

        private string mAsBidWinDate;

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

        private string mMustTakeSumText;

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

        private string mSelectedCancel;

        public string SelectedCancel
        {
            get
            {
                return mSelectedCancel;
            }
            set
            {
                mSelectedCancel = value;
                RaisePropertyChanged("SelectedCancel");
            }
        }

        private string mAsBidSumText;

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

        private string mClearedText;

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

        private string mSegext;
        private string twomSegext;

        public string TwoSegext
        {
            get
            {
                return twomSegext;
            }
            set
            {
                twomSegext = value;
                RaisePropertyChanged("TwoSegext");
            }
        }
        public string Segext
        {
            get
            {
                return mSegext;
            }
            set
            {
                mSegext = value;
                RaisePropertyChanged("Segext");
            }
        }

        private Portfolio mTraderPortfolioComboSelectedItem;

        public Portfolio TraderPortfolioComboSelectedItem
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

        private Portfolio mPortfolioComboSelectedItem;

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

        private SortType selectedSortType;

        public SortType SelectedSortType
        {
            get
            {
                return selectedSortType;
            }
            set
            {
                selectedSortType = value;
                RaisePropertyChanged("SelectedSortType");
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
                RaisePropertyChanged("SelectedHourlyPivot");
            }
        }

        private PeriodType mSelectedPeriodType;

        public PeriodType SelectedPeriodType
        {
            get
            {
                return mSelectedPeriodType;
            }
            set
            {
                mSelectedPeriodType = value;
                RaisePropertyChanged("SelectedPeriodType");
            }
        }

        private bool mPathsGraphSelected;

        public bool PathsGraphSelected
        {
            get
            {
                return mPathsGraphSelected;
            }
            set
            {
                SetPathGraphs();
                mPathsGraphSelected = value;
                RaisePropertyChanged("PathsGraphSelected");
            }
        }

        private bool mMustTakeChecked = false;

        public bool MustTakeChecked
        {
            get
            {
                return mMustTakeChecked;
            }
            set
            {
                mMustTakeChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("MustTakeChecked");
            }
        }

        private bool mAsBidChecked = true;

        public bool AsBidChecked
        {
            get
            {
                return mAsBidChecked;
            }
            set
            {
                mAsBidChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("AsBidChecked");
            }
        }

        private bool mUptosChecked = true;

        public bool UptosChecked
        {
            get
            {
                return mUptosChecked;
            }
            set
            {
                mUptosChecked = value;
                /*if (!mUptosChecked)
                {
                    TraderPortfolioComboList = null;
                }
                else*/
                {
                    SetUserPortfolioList();
                }
                SetPortfolioList();
                RaisePropertyChanged("UptosChecked");
                if (UptosChecked && MarketComboSelectedValue == "ERDOT")
                    isPjmUpto = true;
                else IsPjmUpto = false;
                if (IsPjmUpto)
                    IsPjmUptoSink = true;
                else
                    IsPjmUptoSink = false;
            }
        }

        private List<Portfolio> mPortfolioList;

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

        private bool mMondayChecked = true;

        public bool MondayChecked
        {
            get
            {
                return mMondayChecked;
            }
            set
            {
                mMondayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("MondayChecked");
            }
        }

        private bool mTuesdayChecked = true;

        public bool TuesdayChecked
        {
            get
            {
                return mTuesdayChecked;
            }
            set
            {
                mTuesdayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("TuesdayChecked");
            }
        }

        private bool mWednesdayChecked = true;

        public bool WednesdayChecked
        {
            get
            {
                return mWednesdayChecked;
            }
            set
            {
                mWednesdayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("WednesdayChecked");
            }
        }

        private bool mThursdayChecked = true;

        public bool ThursdayChecked
        {
            get
            {
                return mThursdayChecked;
            }
            set
            {
                mThursdayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("ThursdayChecked");
            }
        }

        private bool mFridayChecked = true;

        public bool FridayChecked
        {
            get
            {
                return mFridayChecked;
            }
            set
            {
                mFridayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("FridayChecked");
            }
        }

        private bool mSaturdayChecked = true;

        public bool SaturdayChecked
        {
            get
            {
                return mSaturdayChecked;
            }
            set
            {
                mSaturdayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("SaturdayChecked");
            }
        }

        private bool mSundayChecked = true;

        public bool SundayChecked
        {
            get
            {
                return mSundayChecked;
            }
            set
            {
                mSundayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("SundayChecked");
            }
        }

        private bool mDecChecked = true;

        public bool DecChecked
        {
            get
            {
                return mDecChecked;
            }
            set
            {
                mDecChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("DecChecked");
            }
        }

        private bool mNovChecked = true;

        public bool NovChecked
        {
            get
            {
                return mNovChecked;
            }
            set
            {
                mNovChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("NovChecked");
            }
        }

        private bool mOctChecked = true;

        public bool OctChecked
        {
            get
            {
                return mOctChecked;
            }
            set
            {
                mOctChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("OctChecked");
            }
        }

        private bool mSepChecked = true;

        public bool SepChecked
        {
            get
            {
                return mSepChecked;
            }
            set
            {
                mSepChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("SepChecked");
            }
        }

        private bool mAugChecked = true;

        public bool AugChecked
        {
            get
            {
                return mAugChecked;
            }
            set
            {
                mAugChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("AugChecked");
            }
        }

        private bool mJulChecked = true;

        public bool JulChecked
        {
            get
            {
                return mJulChecked;
            }
            set
            {
                mJulChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("JulChecked");
            }
        }

        private bool mJunChecked = true;

        public bool JunChecked
        {
            get
            {
                return mJunChecked;
            }
            set
            {
                mJunChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("JunChecked");
            }
        }

        private bool mMayChecked = true;

        public bool MayChecked
        {
            get
            {
                return mMayChecked;
            }
            set
            {
                mMayChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("MayChecked");
            }
        }

        private bool mAprChecked = true;

        public bool AprChecked
        {
            get
            {
                return mAprChecked;
            }
            set
            {
                mAprChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("AprChecked");
            }
        }

        private bool mMarChecked = true;

        public bool MarChecked
        {
            get
            {
                return mMarChecked;
            }
            set
            {
                mMarChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("MarChecked");
            }
        }

        private bool mFebChecked = true;

        public bool FebChecked
        {
            get
            {
                return mFebChecked;
            }
            set
            {
                mFebChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("FebChecked");
            }
        }

        private bool mJanChecked = true;

        public bool JanChecked
        {
            get
            {
                return mJanChecked;
            }
            set
            {
                mJanChecked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("JanChecked");
            }
        }

        private string mMaxLoadText;

        public string MaxLoadText
        {
            get
            {
                return mMaxLoadText;
            }
            set
            {
                mMaxLoadText = value;
                RaisePropertyChanged("MaxLoadText");
            }
        }

        private string mMinLoadText;

        public string MinLoadText
        {
            get
            {
                return mMinLoadText;
            }
            set
            {
                mMinLoadText = value;
                RaisePropertyChanged("MinLoadText");
            }
        }

        private Portfolio mPortfolioListSelected;

        public Portfolio PortfolioListSelected
        {
            get
            {
                return mPortfolioListSelected;
            }
            set
            {
                mPortfolioListSelected = value;
                if (PathList != null)
                {
                    UpdateChartCommand(false);
                    SetPathChart();
                }
                CurrentPortfolio = mPortfolioListSelected;
                RaisePropertyChanged("PortfolioListSelected");
            }
        }

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

        private string mSpreadHighlightAbove;

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

        private string mSpreadHighlightBelow;

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

        private string mSourceSinkHighlightThreshold;

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

        private double[] mSpreadHeatMapFactorsAbove;

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

        private double[] mSpreadHeatMapFactorsBelow;

        public double[] SpreadHeatMapFactorsBelow
        {
            get { return mSpreadHeatMapFactorsBelow; }
            set
            {
                mSpreadHeatMapFactorsBelow = value;
                RaisePropertyChanged("SpreadHeatMapFactorsBelow");
            }
        }

        private double[] mSourceSinkHeatMapFactors;

        public double[] SourceSinkHeatMapFactors
        {
            get { return mSourceSinkHeatMapFactors; }
            set
            {
                mSourceSinkHeatMapFactors = value;
                RaisePropertyChanged("SourceSinkHeatMapFactors");
            }
        }

        private string mMaxPriceText;

        public string MaxPriceText
        {
            get
            {
                return mMaxPriceText;
            }
            set
            {
                mMaxPriceText = value;
                RaisePropertyChanged("MaxPriceText");
            }
        }

        private string mMinPriceText;

        public string MinPriceText
        {
            get
            {
                return mMinPriceText;
            }
            set
            {
                mMinPriceText = value;
                RaisePropertyChanged("MinPriceText");
            }
        }

        private bool mHourlyChecked = true;

        public bool HourlyChecked
        {
            get
            {
                return mHourlyChecked;
            }
            set
            {
                mHourlyChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("HourlyChecked");
            }
        }

        private bool mDailyChecked = false;

        public bool DailyChecked
        {
            get
            {
                return mDailyChecked;
            }
            set
            {
                mDailyChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("DailyChecked");
            }
        }

        private bool mRangeChecked = true;

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

        private bool mCollectionChecked = false;

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
                    List<string> collectionList = DBAccess.GetDateNames();
                    CollectionList = null;
                    CollectionList = collectionList;
                }
                RaisePropertyChanged("CollectionChecked");
            }
        }

        private bool mSortDartChecked = false;

        public bool SortDartChecked
        {
            get
            {
                return mSortDartChecked;
            }
            set
            {
                mSortDartChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("SortDartChecked");
            }
        }

        private bool mSortRtChecked = false;

        public bool SortRtChecked
        {
            get
            {
                return mSortRtChecked;
            }
            set
            {
                mSortRtChecked = value;
                UpdateChartCommand(false);
                SetPathChart();
                RaisePropertyChanged("SortRtChecked");
            }
        }

        private bool mSortDaChecked = false;

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
            }
        }

        private bool mSortDateChecked = true;

        public bool SortDateChecked
        {
            get
            {
                return mSortDateChecked;
            }
            set
            {
                mSortDateChecked = value;
                UpdateChartCommand(false);
                RaisePropertyChanged("SortDateChecked");
            }
        }

        private bool mHE1Checked = true;

        public bool HE1Checked
        {
            get
            {
                return mHE1Checked;
            }
            set
            {
                mHE1Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE1Checked");
            }
        }

        private bool mHE2Checked = true;

        public bool HE2Checked
        {
            get
            {
                return mHE2Checked;
            }
            set
            {
                mHE2Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE2Checked");
            }
        }

        private bool mHE3Checked = true;

        public bool HE3Checked
        {
            get
            {
                return mHE3Checked;
            }
            set
            {
                mHE3Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE3Checked");
            }
        }

        private bool mHE4Checked = true;

        public bool HE4Checked
        {
            get
            {
                return mHE4Checked;
            }
            set
            {
                mHE4Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE4Checked");
            }
        }

        private bool mHE5Checked = true;

        public bool HE5Checked
        {
            get
            {
                return mHE5Checked;
            }
            set
            {
                mHE5Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE5Checked");
            }
        }

        private bool mHE6Checked = true;

        public bool HE6Checked
        {
            get
            {
                return mHE6Checked;
            }
            set
            {
                mHE6Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE6Checked");
            }
        }

        private bool mHE7Checked = true;

        public bool HE7Checked
        {
            get
            {
                return mHE7Checked;
            }
            set
            {
                mHE7Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE7Checked");
            }
        }

        private bool mHE8Checked = true;

        public bool HE8Checked
        {
            get
            {
                return mHE8Checked;
            }
            set
            {
                mHE8Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE8Checked");
            }
        }

        private bool mHE9Checked = true;

        public bool HE9Checked
        {
            get
            {
                return mHE9Checked;
            }
            set
            {
                mHE9Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE9Checked");
            }
        }

        private bool mHE10Checked = true;

        public bool HE10Checked
        {
            get
            {
                return mHE10Checked;
            }
            set
            {
                mHE10Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE10Checked");
            }
        }

        private bool mHE11Checked = true;

        public bool HE11Checked
        {
            get
            {
                return mHE11Checked;
            }
            set
            {
                mHE11Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE11Checked");
            }
        }

        private bool mHE12Checked = true;

        public bool HE12Checked
        {
            get
            {
                return mHE12Checked;
            }
            set
            {
                mHE12Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE12Checked");
            }
        }

        private bool mHE13Checked = true;

        public bool HE13Checked
        {
            get
            {
                return mHE13Checked;
            }
            set
            {
                mHE13Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE13Checked");
            }
        }

        private bool mHE14Checked = true;

        public bool HE14Checked
        {
            get
            {
                return mHE14Checked;
            }
            set
            {
                mHE14Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE14Checked");
            }
        }

        private bool mHE15Checked = true;

        public bool HE15Checked
        {
            get
            {
                return mHE15Checked;
            }
            set
            {
                mHE15Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE15Checked");
            }
        }

        private bool mHE16Checked = true;

        public bool HE16Checked
        {
            get
            {
                return mHE16Checked;
            }
            set
            {
                mHE16Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE16Checked");
            }
        }

        private bool mHE17Checked = true;

        public bool HE17Checked
        {
            get
            {
                return mHE17Checked;
            }
            set
            {
                mHE17Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE17Checked");
            }
        }

        private bool mHE18Checked = true;

        public bool HE18Checked
        {
            get
            {
                return mHE18Checked;
            }
            set
            {
                mHE18Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE18Checked");
            }
        }

        private bool mHE19Checked = true;

        public bool HE19Checked
        {
            get
            {
                return mHE19Checked;
            }
            set
            {
                mHE19Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE19Checked");
            }
        }

        private bool mHE20Checked = true;

        public bool HE20Checked
        {
            get
            {
                return mHE20Checked;
            }
            set
            {
                mHE20Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE20Checked");
            }
        }

        private bool mHE21Checked = true;

        public bool HE21Checked
        {
            get
            {
                return mHE21Checked;
            }
            set
            {
                mHE21Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE21Checked");
            }
        }

        private bool mHE22Checked = true;

        public bool HE22Checked
        {
            get
            {
                return mHE22Checked;
            }
            set
            {
                mHE22Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE22Checked");
            }
        }

        private bool mHE23Checked = true;

        public bool HE23Checked
        {
            get
            {
                return mHE23Checked;
            }
            set
            {
                mHE23Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE23Checked");
            }
        }

        private bool mHE24Checked = true;

        public bool HE24Checked
        {
            get
            {
                return mHE24Checked;
            }
            set
            {
                mHE24Checked = value;
                if (mRefresh)
                {
                    UpdateChartCommand(false);
                }
                if (mRefreshPath)
                {
                    SetPathChart();
                }
                RaisePropertyChanged("HE24Checked");
            }
        }

        private bool mAsBidRiskChecked;

        public bool AsBidRiskChecked
        {
            get
            {
                return mAsBidRiskChecked;
            }
            set
            {
                mAsBidRiskChecked = value;
                RaisePropertyChanged("AsBidRiskChecked");
            }
        }

        private bool mMustTakeRiskChecked;

        public bool MustTakeRiskChecked
        {
            get
            {
                return mMustTakeRiskChecked;
            }
            set
            {
                mMustTakeRiskChecked = value;
                RaisePropertyChanged("MustTakeRiskChecked");
            }
        }

        private bool mAsBidMaxWinChecked;

        public bool AsBidMaxWinChecked
        {
            get
            {
                return mAsBidMaxWinChecked;
            }
            set
            {
                mAsBidMaxWinChecked = value;
                RaisePropertyChanged("AsBidMaxWinChecked");
            }
        }

        private bool mMustTakeMaxWinChecked;

        public bool MustTakeMaxWinChecked
        {
            get
            {
                return mMustTakeMaxWinChecked;
            }
            set
            {
                mMustTakeMaxWinChecked = value;
                RaisePropertyChanged("MustTakeMaxWinChecked");
            }
        }

        private bool mAsBidRiskRwdChecked;

        public bool AsBidRiskRwdChecked
        {
            get
            {
                return mAsBidRiskRwdChecked;
            }
            set
            {
                mAsBidRiskRwdChecked = value;
                RaisePropertyChanged("AsBidRiskRwdChecked");

            }
        }

        private bool mMustTakeRiskRwdChecked;

        public bool MustTakeRiskRwdChecked
        {
            get
            {
                return mMustTakeRiskRwdChecked;
            }
            set
            {
                mMustTakeRiskRwdChecked = value;
                RaisePropertyChanged("MustTakeRiskRwdChecked");

            }
        }

        private bool mAsBidSumChecked;

        public bool AsBidSumChecked
        {
            get
            {
                return mAsBidSumChecked;
            }
            set
            {
                mAsBidSumChecked = value;
                RaisePropertyChanged("AsBidSumChecked");

            }
        }


        private bool mMustTakeSumChecked;

        public bool MustTakeSumChecked
        {
            get
            {
                return mMustTakeSumChecked;
            }
            set
            {
                mMustTakeSumChecked = value;
                RaisePropertyChanged("MustTakeSumChecked");

            }
        }

        private bool mAsBidWinPctChecked;

        public bool AsBidWinPctChecked
        {
            get
            {
                return mAsBidWinPctChecked;
            }
            set
            {
                mAsBidWinPctChecked = value;
                RaisePropertyChanged("AsBidWinPctChecked");
            }
        }


        private bool mAsBidDARTChecked;

        public bool AsBidDARTChecked
        {
            get { return mAsBidDARTChecked; }
            set
            {
                mAsBidDARTChecked = value;
                RaisePropertyChanged("AsBidDARTChecked");
            }
        }


        private bool mMustTakeWinPctChecked;

        public bool MustTakeWinPctChecked
        {
            get
            {
                return mMustTakeWinPctChecked;
            }
            set
            {
                mMustTakeWinPctChecked = value;
                RaisePropertyChanged("MustTakeWinPctChecked");

            }
        }

        private bool mAvgDAChecked;

        public bool AvgDAChecked
        {
            get
            {
                return mAvgDAChecked;
            }
            set
            {
                mAvgDAChecked = value;
                RaisePropertyChanged("AvgDAChecked");

            }
        }

        private bool mAvgRTChecked;

        public bool AvgRTChecked
        {
            get
            {
                return mAvgRTChecked;
            }
            set
            {
                mAvgRTChecked = value;
                RaisePropertyChanged("AvgRTChecked");

            }
        }

        private bool mDARTChecked;

        public bool DARTChecked
        {
            get
            {
                return mDARTChecked;
            }
            set
            {
                mDARTChecked = value;
                RaisePropertyChanged("DARTChecked");
            }
        }

        private bool mMinRTChecked;

        public bool MinRTChecked
        {
            get
            {
                return mMinRTChecked;
            }
            set
            {
                mMinRTChecked = value;
                RaisePropertyChanged("mMinRTChecked");

            }
        }

        private bool mDAMinChecked;

        public bool DAMinChecked
        {
            get
            {
                return mDAMinChecked;
            }
            set
            {
                mDAMinChecked = value;
                RaisePropertyChanged("DAMinChecked");

            }
        }

        private bool mMinDARTChecked;

        public bool MinDARTChecked
        {
            get
            {
                return mMinDARTChecked;
            }
            set
            {
                mMinDARTChecked = value;
                RaisePropertyChanged("MinDARTChecked");

            }
        }

        private bool mMaxDAChecked;

        public bool MaxDAChecked
        {
            get
            {
                return mMaxDAChecked;
            }
            set
            {
                mMaxDAChecked = value;
                RaisePropertyChanged("MaxDAChecked");

            }
        }

        private bool mMaxRTChecked;

        public bool MaxRTChecked
        {
            get
            {
                return mMaxRTChecked;
            }
            set
            {
                mMaxRTChecked = value;
                RaisePropertyChanged("mMaxRTChecked");

            }
        }

        private bool mMaxDARTChecked;

        public bool MaxDARTChecked
        {
            get
            {
                return mMaxDARTChecked;
            }
            set
            {
                mMaxDARTChecked = value;
                RaisePropertyChanged("MaxDARTChecked");
            }
        }

        private bool notionalChecked;

        public bool NotionalChecked
        {
            get
            {
                return notionalChecked;
            }
            set
            {
                notionalChecked = value;
                RaisePropertyChanged("NotionalChecked");
            }
        }

        private bool clearedChecked;

        public bool ClearedChecked
        {
            get
            {
                return clearedChecked;
            }
            set
            {
                clearedChecked = value;
                RaisePropertyChanged("ClearedChecked");
            }
        }

        private bool mMustTakeDARTChecked;


        public bool MustTakeDARTChecked
        {
            get { return mMustTakeDARTChecked; }
            set
            {
                mMustTakeDARTChecked = value;
                RaisePropertyChanged("MustTakeDARTChecked");
            }
        }


        private double mFee;

        public double Fee
        {
            get
            {
                return mFee;
            }
            set
            {
                mFee = value; RaisePropertyChanged("Fee");
            }
        }


        private bool mFilterDayComparisonTotalsChecked;

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
                DisplayHourlySummary();
            }
        }

        private bool mFilterDayComparisonAvgChecked;

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
                DisplayHourlySummary();
            }
        }

        private bool mFilterDayComparisonWinPctChecked;

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
                DisplayHourlySummary();
            }
        }

        private bool mFilterDayComparisonMinChecked;

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
                DisplayHourlySummary();
            }
        }

        private bool mFilterDayComparisonMaxChecked;

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
                DisplayHourlySummary();
            }
        }

        private bool mFilterDayComparisonSharpeChecked;

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

        private bool mFilterDayComparisonRiskRewardChecked;

        public bool FilterDayComparisonRiskRewardChecked
        {
            get
            {
                return mFilterDayComparisonRiskRewardChecked;
            }
            set
            {
                mFilterDayComparisonRiskRewardChecked = value;
                RaisePropertyChanged("FilterDayComparisonRiskRewardChecked");
            }
        }

        private string mCollectionComboSelectedValue;

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

        private string mSpreadComboSelectedValue;

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

        private string mMarketComboSelectedValue;

        public string MarketComboSelectedValue
        {
            get
            {
                return mMarketComboSelectedValue;
            }
            set
            {
                mMarketComboSelectedValue = value;
                RaisePropertyChanged("MarketComboSelectedValue");

                if (MarketComboSelectedValue == "NYISO")
                {
                    //IsSubmitEnabled = false;
                    SubmissionFileEnabled = true;
                }
                else
                {
                    IsSubmitEnabled = true;
                    SubmissionFileEnabled = false;
                }
                if (mMarketComboSelectedValue == "MISO" || mMarketComboSelectedValue == "CAISO" || mMarketComboSelectedValue == "NYISO" || mMarketComboSelectedValue == "SPP")
                {
                    UptosChecked = false;
                    IsPnodeMarket = false;
                    IsPjmUptoSink = false;
                }
                else
                {
                    UptosChecked = true;
                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        IsPjmUpto = true;
                        IsPnodeMarket = true;
                        if (IsPjmUpto)
                        {
                            IsPjmUptoSink = true;
                            SubmissionFileEnabled = true;
                        }
                        else
                            IsPjmUptoSink = false;
                        Vcong = false;
                        VLoss = false;
                        DAExpVisible = true;
                    }
                    else if (MarketComboSelectedValue == "ERCOT External")
                    {
                        IsPjmUpto = true;
                        IsPnodeMarket = true;
                        PortfolioDate = DateTime.Today.AddDays(-61);
                        if (IsPjmUpto)
                        {
                            IsPjmUptoSink = true;
                            SubmissionFileEnabled = true;
                        }
                        else
                            IsPjmUptoSink = false;
                        Vcong = false;
                        VLoss = false;
                        DAExpVisible = true;
                    }
                    else
                    {
                        IsPnodeMarket = false;
                        IsPjmUpto = false;
                        IsPjmUptoSink = false;
                    }
                }
                //SetLoads();
                //SetUserPortfolioList();
                //SetPortfolioList();
            }
        }

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

        private string mFilterLoadComboSelectedValue;

        public string FilterLoadComboSelectedValue
        {
            get
            {
                return mFilterLoadComboSelectedValue;
            }
            set
            {
                mFilterLoadComboSelectedValue = value;
                RaisePropertyChanged("FilterLoadComboSelectedValue");
            }
        }

        private string mFilterPriceComboSelectedValue;

        public string FilterPriceComboSelectedValue
        {
            get
            {
                return mFilterPriceComboSelectedValue;
            }
            set
            {
                mFilterPriceComboSelectedValue = value;
                RaisePropertyChanged("FilterPriceComboSelectedValue");
            }
        }

        private bool mFilterDayComparisonDAChecked = true;

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
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }

        private bool mFilterDayComparisonRTChecked = true;

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
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }

        private bool mFilterDayComparisonDARTChecked = true;

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
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }

        private bool mFilterDayComparisonSourceChecked = true;

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
                if (PathList != null)
                {
                    UpdateChartCommand(false);
                    SetPathChart();
                }
            }
        }

        private bool mFilterDayComparisonSinkChecked = true;

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
                if (PathList != null)
                {
                    UpdateChartCommand(true);
                    SetPathChart();
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
                if (PathList != null)
                {
                    UpdateChartCommand(true);
                    SetPathChart();
                }
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
        /// The m portfolio date
        /// </summary>
        private DateTime mPortfolioDate;
        /// <summary>
        /// Gets or sets the portfolio date.
        /// </summary>
        /// <value>
        /// The portfolio date.
        /// </value>
        public DateTime PortfolioDate
        {
            get
            {
                return mPortfolioDate;
            }
            set
            {
                mPortfolioDate = value.Date;
                SetPortfolioList();
                RaisePropertyChanged("PortfolioDate");
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
        /// The m spread combo list
        /// </summary>
        private List<string> mSpreadComboList;
        /// <summary>
        /// Gets or sets the spread combo list.
        /// </summary>
        /// <value>
        /// The spread combo list.
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
        /// The m filter load combo list
        /// </summary>
        private List<Vayu.DBLibrary.Load> mFilterLoadComboList;
        /// <summary>
        /// Gets or sets the filter load combo list.
        /// </summary>
        /// <value>
        /// The filter load combo list.
        /// </value>
        public List<Vayu.DBLibrary.Load> FilterLoadComboList
        {
            get
            {
                return mFilterLoadComboList;
            }
            set
            {
                mFilterLoadComboList = value;
                RaisePropertyChanged("FilterLoadComboList");
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
        /// The m trader portfolio combo list
        /// </summary>
        private List<Portfolio> mTraderPortfolioComboList;
        /// <summary>
        /// Gets or sets the trader portfolio combo list.
        /// </summary>
        /// <value>
        /// The trader portfolio combo list.
        /// </value>
        public List<Portfolio> TraderPortfolioComboList
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
        /// Sets the loads.
        /// </summary>
        public void SetLoads()
        {
            FilterLoadComboList = DBAccess.GetLoads(MarketComboSelectedValue);
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
        /// The m iso market list
        /// </summary>
        private List<string> mISOMarketList;

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

        private List<String> mFilterPriceComboList;

        public List<String> FilterPriceComboList
        {
            get
            {
                return mFilterPriceComboList;
            }
            set
            {
                mFilterPriceComboList = value;
                RaisePropertyChanged("FilterPriceComboList");
            }
        }

        private List<FilterData> mFilterList;

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
                if (FilterList != null)
                {
                    UpdateChartCommand(true);
                }
            }
        }

        private List<HourlyData> mHourlyList;

        public List<HourlyData> HourlyList
        {
            get
            {
                return mHourlyList;
            }
            set
            {
                mHourlyList = value;
                RaisePropertyChanged("HourlyList");
            }
        }

        private bool mExportDayComparisonEnable;
        public bool ExportDayComparisonEnable
        {
            get { return mExportDayComparisonEnable; }
            set
            {
                mExportDayComparisonEnable = value;
                RaisePropertyChanged("ExportDayComparisonEnable");
            }
        }

        private List<HourlyPivotData> mHourlyPivotList;

        public List<HourlyPivotData> HourlyPivotList
        {
            get
            {
                return mHourlyPivotList;
            }
            set
            {
                mHourlyPivotList = value;

                if (HourlyPivotList != null)
                {
                    ExportDayComparisonEnable = true;
                }
                else
                {
                    ExportDayComparisonEnable = false;
                }

                RaisePropertyChanged("HourlyPivotList");
            }
        }

        private List<Path> mPathList;

        public List<Path> PathList
        {
            get
            {
                return mPathList;
            }
            set
            {
                SelectChange = false;
                mPathList = value;
                RaisePropertyChanged("PathList");
                SelectChange = true;
            }
        }

        private List<string> mSelectedSourceListItem;

        private Path mPathSelectedItem;
        /// <summary>
        /// Gets or sets the path selected item.
        /// </summary>
        /// <value>
        /// The path selected item.
        /// </value>
        public Path PathSelectedItem
        {
            get
            {
                return mPathSelectedItem;
            }
            set
            {
                mPathSelectedItem = value;
                RaisePropertyChanged("PathSelectedItem");
            }
        }


        private List<HourlyPivotData> mHourlyPivotListSummary;

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
        /// The m selected spread type
        /// </summary>
        private SpreadType mSelectedSpreadType;
        /// <summary>
        /// Gets or sets the type of the selected spread.
        /// </summary>
        /// <value>
        /// The type of the selected spread.
        /// </value>
        public SpreadType SelectedSpreadType
        {
            get
            {
                return mSelectedSpreadType;
            }
            set
            {
                mSelectedSpreadType = value;
                RaisePropertyChanged("SelectedSpreadType");
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
                if (mLocations == value)
                {
                    return;
                }
                mLocations = value;
                RaisePropertyChanged("Locations");
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
        /// <summary>
        /// Gets or sets the selected path by cell.
        /// </summary>
        /// <value>
        /// The selected path by cell.
        /// </value>
        public List<Path> SelectedPathByCell { get; set; }
        /// <summary>
        /// The m show hide summary
        /// </summary>
        private Visibility mShowHideSummary;
        /// <summary>
        /// Gets or sets the show hide summary.
        /// </summary>
        /// <value>
        /// The show hide summary.
        /// </value>
        public Visibility ShowHideSummary
        {
            get
            {
                return mShowHideSummary;
            }
            set
            {
                if (mShowHideSummary != value)
                {
                    mShowHideSummary = value;
                    RaisePropertyChanged("ShowHideSummary");
                }
            }
        }
        /// <summary>
        /// The sort condition
        /// </summary>
        private string sortCondition;
        /// <summary>
        /// Gets or sets the sort condition.
        /// </summary>
        /// <value>
        /// The sort condition.
        /// </value>
        public string SortCondition
        {
            get
            {
                return sortCondition;
            }
            set
            {
                sortCondition = value;
                RaisePropertyChanged("SortCondition");
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [sort order].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort order]; otherwise, <c>false</c>.
        /// </value>
        public bool SortOrder { get; set; }
        /// <summary>
        /// The m selected tab index
        /// </summary>
        private int mSelectedTabIndex;
        /// <summary>
        /// Gets or sets the index of the selected tab.
        /// </summary>
        /// <value>
        /// The index of the selected tab.
        /// </value>
        public int SelectedTabIndex
        {
            get
            {
                return mSelectedTabIndex;
            }
            set
            {
                if (mSelectedTabIndex != value)
                {
                    mSelectedTabIndex = value;
                    DetailsTabSelectionChanged();
                    RaisePropertyChanged("SelectedTabIndex");
                }
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
        /// The m is pnode market
        /// </summary>
        private bool mIsPnodeMarket;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is pnode market.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is pnode market; otherwise, <c>false</c>.
        /// </value>
        public bool IsPnodeMarket
        {
            get
            {
                return mIsPnodeMarket;
            }
            set
            {
                mIsPnodeMarket = value;
                RaisePropertyChanged("IsPnodeMarket");
            }
        }
        /// <summary>
        /// The m is submit enabled
        /// </summary>
        private bool mIsSubmitEnabled;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is submit enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is submit enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsSubmitEnabled
        {
            get
            {
                return mIsSubmitEnabled;
            }
            set
            {
                mIsSubmitEnabled = value;
                RaisePropertyChanged("IsSubmitEnabled");
            }
        }
        /// <summary>
        /// The mw multiplier value
        /// </summary>
        private double mwMultiplierValue;
        /// <summary>
        /// Gets or sets the mw multiplier value.
        /// </summary>
        /// <value>
        /// The mw multiplier value.
        /// </value>
        public double MwMultiplierValue
        {
            get { return mwMultiplierValue; }
            set
            {
                double val;
                if (double.TryParse(value.ToString(), out val))
                {
                    if (val <= 0 || val > 2)
                    {
                        MessageBox.Show("Multiplier cannot be more than 2 and less than or equal to zero.",
                        "Invalid Multiplier", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (Math.Abs(val) <= 2)
                    {
                        mwMultiplierValue = value;
                        RaisePropertyChanged("MwMultiplierValue");
                    }
                }
            }
        }
        /// <summary>
        /// The m user list
        /// </summary>
        private ObservableCollection<string> mUserList;
        /// <summary>
        /// Gets or sets the user list.
        /// </summary>
        /// <value>
        /// The user list.
        /// </value>
        public ObservableCollection<string> UserList
        {
            get
            {
                return mUserList;
            }
            set
            {
                mUserList = value;
                RaisePropertyChanged("UserList");
            }
        }
        /// <summary>
        /// The m PJM user list
        /// </summary>
        private ObservableCollection<string> mPJMUserList;
        /// <summary>
        /// Gets or sets the PJM user list.
        /// </summary>
        /// <value>
        /// The PJM user list.
        /// </value>
        public ObservableCollection<string> PJMUserList
        {
            get
            {
                return mPJMUserList;
            }
            set
            {
                mPJMUserList = value;
                RaisePropertyChanged("PJMUserList");
            }
        }
        /// <summary>
        /// The m caiso user list
        /// </summary>
        private ObservableCollection<string> mCAISOUserList;
        /// <summary>
        /// Gets or sets the caiso user list.
        /// </summary>
        /// <value>
        /// The caiso user list.
        /// </value>
        public ObservableCollection<string> CAISOUserList
        {
            get
            {
                return mCAISOUserList;
            }
            set
            {
                mCAISOUserList = value;
                RaisePropertyChanged("CAISOUserList");
            }
        }

        /// <summary>
        /// The m date picker enable
        /// </summary>
        private bool mDatePickerEnable;
        /// <summary>
        /// Gets or sets a value indicating whether [date picker enable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [date picker enable]; otherwise, <c>false</c>.
        /// </value>
        public bool DatePickerEnable
        {
            get
            {
                return mDatePickerEnable;
            }
            set
            {
                mDatePickerEnable = value;
                RaisePropertyChanged("DatePickerEnable");
            }
        }
        /// <summary>
        /// The m clearing date
        /// </summary>
        private DateTime mClearingDate;
        /// <summary>
        /// Gets or sets the clearing date.
        /// </summary>
        /// <value>
        /// The clearing date.
        /// </value>
        public DateTime ClearingDate
        {
            get
            {
                return mClearingDate;
            }
            set
            {
                mClearingDate = value.Date;
                RaisePropertyChanged("ClearingDate");
            }
        }
        private DateTime mEndClearedDate;

        public DateTime EndClearedDate
        {
            get
            {
                return mEndClearedDate;
            }
            set
            {
                mEndClearedDate = value;
                RaisePropertyChanged("EndClearedDate");
            }
        }

        private DateTime mAsBidClearedDate;

        public DateTime AsBidClearedDate
        {
            get
            {
                return mAsBidClearedDate;
            }
            set
            {
                mAsBidClearedDate = value;
                RaisePropertyChanged("AsBidClearedDate");
            }
        }

        private bool mAsBidClearedDateEnable = true;

        public bool AsBidClearedDateEnable
        {
            get
            {
                return mAsBidClearedDateEnable;
            }
            set
            {
                mAsBidClearedDateEnable = value;
                RaisePropertyChanged("AsBidClearedDateEnable");
            }
        }


        private bool mPathMWTabSelect;

        public bool PathMWTabSelect
        {
            get
            {
                return mPathMWTabSelect;
            }
            set
            {
                mPathMWTabSelect = value;
                RaisePropertyChanged("PathMWTabSelect");
            }
        }

        private List<HourlyPathMWs> mPathMWList;

        public List<HourlyPathMWs> PathMWList
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
        private bool mPortfolioChecked;

        public bool PortfolioChecked
        {
            get
            {
                return mPortfolioChecked;
            }
            set
            {
                mPortfolioChecked = value;
                RaisePropertyChanged("PortfolioChecked");
            }
        }
        private bool mFuelChecked;
        public bool FuelChecked
        {
            get { return mFuelChecked; }
            set
            {
                mFuelChecked = value;
                RaisePropertyChanged("FuelChecked");
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

        private bool mDaCheckedcomp = false;
        public bool DaCheckedcomp
        {
            get
            {
                return mDaCheckedcomp;
            }
            set
            {
                mDaCheckedcomp = value;
                if (DaCheckedcomp)
                {
                    RtCheckedcomp = false;
                    DartCheckedcomp = false;
                    FilterDayComparisonDARTChecked = false;
                    clearedMsHash = null;
                }
                RaisePropertyChanged("DaCheckedcomp");
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    FetchComponentwise();
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }

        private bool mRtCheckedcomp = false;
        public bool RtCheckedcomp
        {
            get
            {
                return mRtCheckedcomp;
            }
            set
            {
                mRtCheckedcomp = value;
                if (RtCheckedcomp)
                {
                    DaCheckedcomp = false;
                    DartCheckedcomp = false;
                    FilterDayComparisonDARTChecked = false;
                    clearedMsHash = null;
                }
                RaisePropertyChanged("RtCheckedcomp");
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    FetchComponentwise();
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }

        private bool mDartCheckedcomp = true;
        public bool DartCheckedcomp
        {
            get
            {
                return mDartCheckedcomp;
            }
            set
            {
                mDartCheckedcomp = value;
                RaisePropertyChanged("DartCheckedcomp");
                if (DartCheckedcomp)
                {
                    RtCheckedcomp = false;
                    DaCheckedcomp = false;
                    clearedMsHash = null;
                    FilterDayComparisonDARTChecked = true;
                }
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    FetchComponentwise();
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }

        private bool mLmpCheckedcomp;
        public bool LmpCheckedcomp
        {
            get
            {
                return mLmpCheckedcomp;
            }
            set
            {
                mLmpCheckedcomp = value;
                if (LmpCheckedcomp)
                {
                    CongCheckedcomp = false;
                    LossCheckedcomp = false;
                    clearedMsHash = null;
                }
                RaisePropertyChanged("LmpCheckedcomp");
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    FetchComponentwise();
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }

        private bool mCongCheckedcomp;
        public bool CongCheckedcomp
        {
            get
            {
                return mCongCheckedcomp;
            }
            set
            {
                mCongCheckedcomp = value;
                RaisePropertyChanged("CongCheckedcomp");
                if (CongCheckedcomp)
                {
                    LmpCheckedcomp = false;
                    LossCheckedcomp = false;
                    clearedMsHash = null;
                }
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    FetchComponentwise();

                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }
        private bool mLossCheckedcomp;
        public bool LossCheckedcomp
        {
            get
            {
                return mLossCheckedcomp;
            }
            set
            {
                mLossCheckedcomp = value;
                RaisePropertyChanged("CongCheckedcomp");
                if (LossCheckedcomp)
                {
                    LmpCheckedcomp = false;
                    CongCheckedcomp = false;
                    clearedMsHash = null;
                }
                if ((value == true || SpreadComboSelectedValue != "Exclusive") && PathList != null)
                {
                    FetchComponentwise();
                    UpdateChartCommand(true);
                    SetPathChart();
                }
            }
        }
        private bool mVcong;
        public bool Vcong
        {
            get
            {
                return mVcong;
            }
            set
            {
                mVcong = value;
                RaisePropertyChanged("Vcong");
            }
        }
        private bool mVLoss;
        public bool VLoss
        {
            get
            {
                return mVLoss;
            }
            set
            {
                mVLoss = value;
                RaisePropertyChanged("VLoss");
            }
        }
        //private string mGroupBoxVisibility;
        //public string GroupBoxVisibility
        //{
        //    get
        //    {
        //        return mGroupBoxVisibility;
        //    }
        //    set
        //    {
        //        mGroupBoxVisibility = value;
        //        RaisePropertyChanged("GroupBoxVisibility");
        //    }
        //}

        private string stTextScale;
        public string TextScale
        {
            get
            {
                return stTextScale;
            }
            set
            {
                stTextScale = value;
                RaisePropertyChanged("TextScale");
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

        private bool mDAExpVisible;
        public bool DAExpVisible
        {
            get
            {
                return mDAExpVisible;
            }
            set
            {
                mDAExpVisible = value;
                RaisePropertyChanged("DAExpVisible");
            }
        }


        #region Relay Command Properties

        public DelegateCommand ExportDayComparisonCommand { private set; get; }


        public DelegateCommand RunRetrieveFetchDataAndUpdateChartCommand { private set; get; }

        public DelegateCommand GetCleredPathsCmd { get; set; }

        public DelegateCommand SeasonCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add price command.
        /// </summary>
        /// <value>
        /// The add price command.
        /// </value>
        public DelegateCommand AddPriceCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add load command.
        /// </summary>
        /// <value>
        /// The add load command.
        /// </value>
        //public DelegateCommand AddLoadCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add portfolio command.
        /// </summary>
        /// <value>
        /// The add portfolio command.
        /// </value>
        public DelegateCommand AddPortfolioCommand { private set; get; }
        /// <summary>
        /// Gets or sets the refresh portfolio command.
        /// </summary>
        /// <value>
        /// The refresh portfolio command.
        /// </value>
        public DelegateCommand RefreshPortfolioCommand { private set; get; }
        /// <summary>
        /// Gets or sets the retrieve command.
        /// </summary>
        /// <value>
        /// The retrieve command.
        /// </value>
        public DelegateCommand RetrieveCommand { private set; get; }
        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public DelegateCommand ResetCommand { private set; get; }

        public DelegateCommand RemovePortfolioCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove all portfolio command.
        /// </summary>
        /// <value>
        /// The remove all portfolio command.
        /// </value>
        public DelegateCommand RemoveAllPortfolioCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove filter command.
        /// </summary>
        /// <value>
        /// The remove filter command.
        /// </value>
        public DelegateCommand RemoveFilterCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove all filter command.
        /// </summary>
        /// <value>
        /// The remove all filter command.
        /// </value>
        public DelegateCommand RemoveAllFilterCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add filter command.
        /// </summary>
        /// <value>
        /// The add filter command.
        /// </value>
        public DelegateCommand AddFilterCommand { private set; get; }
        /// <summary>
        /// Gets or sets the import command.
        /// </summary>
        /// <value>
        /// The import command.
        /// </value>
        public DelegateCommand ImportCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        public DelegateCommand DeleteCommand { private set; get; }
        /// <summary>
        /// Gets or sets the copy command.
        /// </summary>
        /// <value>
        /// The copy command.
        /// </value>
        public DelegateCommand CopyCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add path command.
        /// </summary>
        /// <value>
        /// The add path command.
        /// </value>
        public DelegateCommand AddPathCommand { private set; get; }
        /// <summary>
        /// Gets or sets the submit command.
        /// </summary>
        /// <value>
        /// The submit command.
        /// </value>
        public DelegateCommand SubmitCommand { private set; get; }
        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public DelegateCommand CancelCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run retrieve go command.
        /// </summary>
        /// <value>
        /// The run retrieve go command.
        /// </value>
        public DelegateCommand RunRetrieveGoCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run path details command.
        /// </summary>
        /// <value>
        /// The run path details command.
        /// </value>
        public DelegateCommand RunHistoricalConstOpenCmd { private set; get; }
        public DelegateCommand RunRTHistoricalConstOpenCmd { private set; get; }
        public DelegateCommand RunDAHistoricalConstOpenCmd { private set; get; }
        public DelegateCommand RunPathDetailsCommand { private set; get; }
        /// <summary>
        /// Gets or sets the cancel add command.
        /// </summary>
        /// <value>
        /// The cancel add command.
        /// </value>
        public DelegateCommand CancelAddCommand { private set; get; }
        /// <summary>
        /// Gets or sets the override command.
        /// </summary>
        /// <value>
        /// The override command.
        /// </value>
        public DelegateCommand OverrideCommand { private set; get; }
        /// <summary>
        /// Gets or sets the node analyzer command.
        /// </summary>
        /// <value>
        /// The node analyzer command.
        /// </value>
        public DelegateCommand NodeAnalyzerCommand { private set; get; }
        /// <summary>
        /// Gets or sets the LMP graphs command.
        /// </summary>
        /// <value>
        /// The LMP graphs command.
        /// </value>
        public DelegateCommand LMPGraphsCommand { private set; get; }
        /// <summary>
        /// Gets or sets all cancel command.
        /// </summary>
        /// <value>
        /// All cancel command.
        /// </value>
        public DelegateCommand AllCancelCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete cancel command.
        /// </summary>
        /// <value>
        /// The delete cancel command.
        /// </value>
        public DelegateCommand DeleteCancelCommand { private set; get; }
        /// <summary>
        /// Gets or sets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public DelegateCommand CloseCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete selected path command.
        /// </summary>
        /// <value>
        /// The delete selected path command.
        /// </value>
        public DelegateCommand DeleteSelectedPathCommand { private set; get; }
        /// <summary>
        /// Gets or sets the show hide command.
        /// </summary>
        /// <value>
        /// The show hide command.
        /// </value>
        public DelegateCommand ShowHideCommand { private set; get; }
        /// <summary>
        /// Gets the create request file command.
        /// </summary>
        /// <value>
        /// The create request file command.
        /// </value>
        public DelegateCommand CreateRequestFileCommand { get; private set; }
        /// <summary>
        /// Gets or sets the update mw for path command.
        /// </summary>
        /// <value>
        /// The update mw for path command.
        /// </value>
        public DelegateCommand UpdateMWForPathCommand { private set; get; }
        public DelegateCommand ClickPathMWsCommand { private set; get; }
        public DelegateCommand PieChartCommand { private set; get; }
        public DelegateCommand ScaleCommand { private set; get; }
        List<string> ListstrDeenergizedNodes = new List<string>();
        #endregion

        #endregion

        public Dictionary<string, double> loadDictHash;
        List<HourlyPivotData> tempHourlyPivotData = new List<HourlyPivotData>();
        List<string> FilterLoadDateList;

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.loadDBCommands();
            StartDate = DateTime.Now.Date.AddDays(-45);
            RiskConstraintDateSelected = DateTime.Today.AddDays(-1 * DateTime.Today.Day - 1);
            EndDate = DateTime.Now.Date;
            SelectedSpreadType = SpreadType.DART;
            SelectedSortType = SortType.Date;
            SelectedPeriodType = PeriodType.Hourly;
            MarketComboSelectedValue = "ERCOT";
            FilterPriceComboList = new List<string>();
            if (_User == "darshand" || _User == "neelams" || _User == "gojira" || _User == "sangramp")
            {
                SubmitVisible = "Visible";
                CancelVisible = "Visible";
                CreateRequestFileVisible = "Visible";

            }
            else
            {
                SubmitVisible = "Hidden";
                CancelVisible = "Hidden";
                CreateRequestFileVisible = "Hidden";

            }
            FilterPriceComboList.Add("DA");
            FilterPriceComboList.Add("RT");
            FilterPriceComboList.Add("DART");
            FilterPriceComboSelectedValue = "DA";
            FilterDayComparisonSourceChecked = false;
            FilterDayComparisonSinkChecked = false;
            SpreadHighlightAbove = "20";
            SpreadHighlightBelow = "-20";
            FilterDayComparisonDAChecked = false;
            FilterDayComparisonRTChecked = false;
            FilterDayComparisonDARTChecked = true;
            FilterDayComparisonTotalChecked = false;
            FilterDayComparisonAvgChecked = true;
            FilterDayComparisonWinPctChecked = true;
            FilterDayComparisonMinChecked = false;
            FilterDayComparisonMaxChecked = false;
            FilterDayComparisonSharpeChecked = false;
            FilterDayComparisonRiskRewardChecked = false;
            AddPortfolioCommand = new DelegateCommand(AddPortfolio);
            RefreshPortfolioCommand = new DelegateCommand(RefreshPortfolio);
            RetrieveCommand = new DelegateCommand(Retrieve);
            RunRetrieveFetchDataAndUpdateChartCommand = new DelegateCommand(RetrieveFetchDataAndUpdateChartCommand);
            GetCleredPathsCmd = new DelegateCommand(GetCleredPaths);
            SeasonCommand = new DelegateCommand(Season);
            RemoveFilterCommand = new DelegateCommand(RemoveFilters);
            AddFilterCommand = new DelegateCommand(AddFilters);
            RemoveAllFilterCommand = new DelegateCommand(RemoveAllFilters);
            RemovePortfolioCommand = new DelegateCommand(RemovePortfolio);
            RemoveAllPortfolioCommand = new DelegateCommand(RemoveAllPortfolio);
            ImportCommand = new DelegateCommand(Import);
            DeleteCommand = new DelegateCommand(Delete);
            CopyCommand = new DelegateCommand(Copy);
            CancelCommand = new DelegateCommand(Cancel);
            RunRetrieveGoCommand = new DelegateCommand(GoCommand);
            RunPathDetailsCommand = new DelegateCommand(PathDetailsCommand);
            RunHistoricalConstOpenCmd = new DelegateCommand(OpenHistoricalConstraints);
            RunRTHistoricalConstOpenCmd = new DelegateCommand(OpenRTHistoricalConstraints);
            RunDAHistoricalConstOpenCmd = new DelegateCommand(OpenDAHistoricalConstraints);
            OverrideCommand = new DelegateCommand(Override);
            CancelAddCommand = new DelegateCommand(CancelAdd);
            AllCancelCommand = new DelegateCommand(AllCancel);
            DeleteCancelCommand = new DelegateCommand(DeleteCancel);
            CloseCommand = new DelegateCommand(Close);
            CreateRequestFileCommand = new DelegateCommand(() => CreateRequestFile());
            UpdateMWForPathCommand = new DelegateCommand(() => UpdateMWForPaths());
            SpreadComboSelectedValue = null;
            SpreadComboSelectedValue = "Exclusive";
            PortfolioDate = DateTime.Today.AddDays(-1);
            UserList = DBAccess.GetUserList();
            SetMarket();
            loadDictHash = new Dictionary<string, double>();
            PortfolioDate = DateTime.Today.AddDays(1);
            SubmitDate = DateTime.Today.AddDays(1);
            ClearingDate = DateTime.Today;
            EndClearedDate = DateTime.Today;
            ClearingStartDate = DateTime.Today;
            _ClearingEndDate = DateTime.Today;
            AsBidClearedDate = DateTime.Today;
            List<string> productList = new List<string> { "Price", "Load", "Temp", "Cloud Cover", "Dew Point", "Precip", "Wind Speed", "Rel. Hum.", "Wind Dir." };
            ProductList = productList;

            ShowHideCommand = new DelegateCommand(ShowHideSummaryGrid);
            ClickPathMWsCommand = new DelegateCommand(() => ShowPathMws());
            PieChartCommand = new DelegateCommand(() => PieChart());
            ExportDayComparisonCommand = new DelegateCommand(() => ExportDayComparison());
            mLmpCheckedcomp = true;
            Vcong = true;
            VLoss = true;
            ScaleCommand = new DelegateCommand(ScaleExecute);
            ListstrDeenergizedNodes = GetDeenergizedNodes();
        }

        private void ExportDayComparison()
        {
            if (HourlyPivotList != null)
            {
                ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>> obj = new ExportToExcelNodeSpread<Model.HourlyPivotData, List<Model.HourlyPivotData>>();

                obj.dataToPrint = HourlyPivotList;
                obj.GenerateReport();
            }
        }

        private void GetCleredPaths()
        {
            if (PathList != null && PathList.Count > 1)
            {
                List<ClearedPathsHelper> LatestCLearedPathsList = new List<ClearedPathsHelper>();
                DataService ds = new DataService();
                int portfolioKey = 0;
                List<ClearedPathsHelper> tempClearedPathList = new List<ClearedPathsHelper>();
                foreach (Portfolio portfolio in PortfolioList)
                {
                    if (MarketComboSelectedValue == "ERCOT")
                        tempClearedPathList = ds.GetClearedPaths(portfolio.Market, PathList, portfolio.ID, ClearingStartDate, _ClearingEndDate, PortfolioDate, 9);
                    if (ClearedPathList == null)
                    {
                        ClearedPathList = tempClearedPathList.ToList();
                    }
                    else
                    {
                        foreach (ClearedPathsHelper clearedPath in tempClearedPathList)
                        {
                            bool exists = ClearedPathList.Exists(a => a.Source == clearedPath.Source && a.Sink == clearedPath.Sink && a.Hours == clearedPath.Hours);
                            if (!exists)
                            {
                                ClearedPathList.Add(clearedPath);
                            }
                        }
                    }
                }
            }
            else
                MessageBox.Show("Please Retrive the data for selected portfolio");
        }

        private void OpenHistoricalConstraints()
        {
            bool isDA = false;
            if (RTExpChecked)
                isDA = false;
            else
                isDA = true;
            Vayu.ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            var datacontext = new Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new Vayu.ConstraintContingencyHistory.Model.DataService());
            window.DataContext = datacontext;
            datacontext.ShowHistoricalConstraintForExposure(SelectedConstraintPathValue.Constraint, SelectedConstraintPathValue.Contingency, SelectedConstraintPathValue.ID, 9, isDA);

            window.Show();
        }

        private void OpenRTHistoricalConstraints()
        {
            Vayu.ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            var datacontext = new Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new Vayu.ConstraintContingencyHistory.Model.DataService());
            window.DataContext = datacontext;
            datacontext.ShowHistoricalConstraintForExposure(SelectedConstraintPathValue.Constraint, SelectedConstraintPathValue.Contingency, SelectedConstraintPathValue.ID, 9, false);
            if(datacontext.ConstraintList==null)
            {
                MessageBox.Show("This Constraint Does not have History");
            }
            else
            window.Show();
        }

        private void OpenDAHistoricalConstraints()
        {
            Vayu.ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            var datacontext = new Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new Vayu.ConstraintContingencyHistory.Model.DataService());
            window.DataContext = datacontext;
            datacontext.ShowHistoricalConstraintForExposure(SelectedConstraintPathValue.Constraint, SelectedConstraintPathValue.Contingency, SelectedConstraintPathValue.ID, 9, true);
            if (datacontext.ConstraintList == null)
            {
                MessageBox.Show("This Constraint Does not have History");
            }
            else
                window.Show();
        }



        #region Public Methods

        public void SetPathChart()
        {
            if (PathComboSelectedValue == null)
            {
                return;
            }
            List<Node> asBidDaSendFilteredSortedList = new List<Node>();
            List<Node> asBidRtSendFilteredSortedList = new List<Node>();
            List<Node> asBidDartSendFilteredSortedList = new List<Node>();
            List<Node> mustTakeDaSendFilteredSortedList = new List<Node>();
            List<Node> mustTakeRtSendFilteredSortedList = new List<Node>();
            List<Node> mustTakeDartSendFilteredSortedList = new List<Node>();
            List<Node> nodeList = GetNodeList(out asBidDaSendFilteredSortedList, out asBidRtSendFilteredSortedList, out asBidDartSendFilteredSortedList, out mustTakeDaSendFilteredSortedList,
                                            out mustTakeRtSendFilteredSortedList, out mustTakeDartSendFilteredSortedList, PathComboSelectedValue);
            if (nodeList == null)
            {
                return;
            }
            PathPlotModelUpper = null;
            PathPlotModelLower = null;
            if (nodeList.Count > 0)
            {
                PathPlotModelUpper = CreatePlotModelUpper(nodeList);
                PathPlotModelLower = CreatePlotModelLower(nodeList);
            }
        }

        private List<string> GetDeenergizedNodes()
        {
            List<string> listDnergizeNode = new List<string>();
            try
            {
                listDnergizeNode = _dataService.GetStrDeenergizedNodes();
            }
            catch (Exception)
            {
            }
            return listDnergizeNode;
        }
        public void Sort(List<Node> sendNodeList)
        {
            List<Node> sortList = new List<Node>();
            if (SortDaChecked)
            {
                sortList.Add(sendNodeList[0]);
            }
            if (SortRtChecked)
            {
                sortList.Add(sendNodeList[1]);
            }
            if (SortDartChecked)
            {
                sortList.Add(sendNodeList[2]);
            }
            if (sortList.Count == 0)
            {
                return;
            }
            if (SortDateChecked)
            {
                List<Node> daList = new List<Node>();
                List<Node> rtList = new List<Node>();
                List<Node> dartList = new List<Node>();
                daList.Add(sendNodeList[0]);
                rtList.Add(sendNodeList[1]);
                dartList.Add(sendNodeList[2]);
                foreach (List<Node> nodelist in new List<List<Node>>() { daList, rtList, dartList })
                {
                    foreach (Node node in nodelist)
                    {
                        List<TimePrice> timePriceList = node.TimePriceList.OrderBy(x => x.MarketTime).ToList<TimePrice>();
                        node.TimePriceList = timePriceList;
                    }
                }
            }
            else
            {
                Dictionary<double, List<DateTime>> sortHash = new Dictionary<double, List<DateTime>>();
                foreach (TimePrice timePrice in sortList[sortList.Count - 1].TimePriceList)
                {
                    List<DateTime> dateList = new List<DateTime>();
                    if (sortHash.ContainsKey(timePrice.Price))
                    {
                        dateList = sortHash[timePrice.Price];
                        sortHash.Remove(timePrice.Price);
                    }
                    dateList.Add(timePrice.MarketTime);
                    sortHash.Add(timePrice.Price, dateList);
                }
                List<DateTime> sortDateList = new List<DateTime>();
                List<double> priceKeyList = sortHash.Keys.ToList<double>();
                priceKeyList.Sort();
                if (priceKeyList.Contains(double.NaN))
                {
                    priceKeyList.Remove(double.NaN);
                }
                foreach (double price in priceKeyList)
                {
                    List<DateTime> tempDateList = sortHash[price];
                    foreach (DateTime date in tempDateList.Distinct<DateTime>().ToList())
                    {
                        sortDateList.Add(date);
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    List<Node> nodeList = new List<Node>();
                    nodeList.Add(sendNodeList[i]);
                    foreach (Node node in nodeList)
                    {
                        List<TimePrice> timePriceList = node.TimePriceList;
                        Dictionary<DateTime, TimePrice> dateHash = new Dictionary<DateTime, TimePrice>();
                        foreach (TimePrice timePrice in timePriceList)
                        {
                            dateHash.Add(timePrice.MarketTime, timePrice);
                        }
                        List<TimePrice> tempPriceList = new List<TimePrice>();
                        foreach (DateTime date in sortDateList)
                        {
                            if (dateHash.ContainsKey(date))
                            {
                                tempPriceList.Add(dateHash[date]);
                            }
                        }
                        node.TimePriceList = tempPriceList;
                    }
                }
            }
        }

        public void SendResults(string[] s, string[] e)
        {
            string message = "";
            foreach (string mesg in s)
            {
                message = message + mesg + "\n";
            }
            foreach (string mesg in e)
            {
                message = message + mesg + "\n";
            }
            MessageBox.Show(message);
            Retrieve();
        }

        public void SendCancelResults(string[] s, string[] e)
        {
        }

        public void RemovePortfolio()
        {
            mFillPortfolioList.Remove(PortfolioListSelected);
            PortfolioList = null;
            PortfolioList = mFillPortfolioList;
            ClearAll();
        }

        public void RemoveAllPortfolio()
        {
            mFillPortfolioList = new List<Portfolio>();
            PortfolioList = null;
            ClearAll();
        }

        public void AddPath(List<Path> pathList)
        {
            if (TraderPortfolioComboSelectedItem == null)
            {
                MessageBox.Show("Please select portfolio");
                return;
            }
            PathList = null;
            PathList = pathList;
            if (UptosChecked == true)
            {
                mVwModel = new Vayu.WorkbookStatistics.ViewModels.UptosPTPBidEntryViewModel(new Vayu.WorkbookStatistics.Model.DataService()) { ParentModel = this };
                mPathWindow = new Vayu.WorkbookStatistics.Views.UptosPTPBidEntry();
                mPathWindow.DataContext = mVwModel;
                mVwModel.SetValues(MarketComboSelectedValue, TraderPortfolioComboSelectedItem, PortfolioDate, SubmitDate);
                mPathWindow.Topmost = true;
                mPathWindow.Show();
            }
            if (UptosChecked == false)
            {
                IVirtualBidEntry bidEntry = mainApp.GetVirtualBidEntryInterface();
                bidEntry.Show(MarketComboSelectedValue, TraderPortfolioComboSelectedItem, PortfolioDate);
            }
        }


        public void Cancel()
        {

            if (CancelList == null || CancelList.Count < 1)
            {
                MessageBox.Show("Please add nodes/paths you would like to cancel");
                return;
            }
            DateTime submitDate = SubmitDate;
            TimeZone timeZone = TimeZone.CurrentTimeZone;
            if (timeZone.StandardName.StartsWith("East"))
            {
                submitDate = submitDate.AddHours(1);
            }
            if (PortfolioListSelected == null)
            {
                MessageBox.Show("Please select Portfolio");
                return;
            }
            List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> virtualList = PortfolioListSelected.IsUptos ? GetUptosList(true) : GetVirtualList(true, GetMarketKey(PortfolioListSelected.Market));
            if (virtualList == null)
            {
                return;
            }
            List<PTPBid> ptpBidList = new List<PTPBid>();
            foreach (Vayu.VirtualBidSubmissionLibrary.VirtualBid oldBid in virtualList)
            {
                List<BidValues> intervalList = new List<BidValues>();
                PTPBid bid = new PTPBid();
                bid.Source = oldBid.Source;
                bid.Sink = oldBid.Sink;
                bid.PortfolioKey = PortfolioListSelected.ID;
                bid.RequestID = "QENJRE." + DateTime.Today.AddDays(1).ToString("yyyyMMdd") + ".PTP." + oldBid.BidId + "." + oldBid.Source + "." + oldBid.Sink;
                ptpBidList.Add(bid);
            }
            PTPBid[] ptpbidArr = ptpBidList.ToArray();

            DuplexChannelFactory<IBidSubmit> pipeFactoryErcot = GetProxyErcot();
            if (pipeFactoryErcot == null)
            {
                return;
            }
            IBidSubmit submitProxy = pipeFactoryErcot.CreateChannel();
            Vayu.VirtualBidSubmissionLibrary.VirtualBid[] virtuals = virtualList.ToArray<Vayu.VirtualBidSubmissionLibrary.VirtualBid>();
            string resultString = "";
            try
            {
                resultString = submitProxy.CancelBids(ptpbidArr, 0);
                pipeFactoryErcot.Close();
            }
            catch (Exception ex)
            {
                resultString = ex.Message;
            }
            MessageBox.Show(resultString);

            Retrieve();
        }

        public void SetValues(string market, Portfolio portfolio, DateTime submitDate, Path path)
        {
            if (MarketComboSelectedValue == null || MarketComboSelectedValue != market)
            {
                MarketComboSelectedValue = market;
            }
            if (PortfolioDate == null || PortfolioDate != submitDate)
            {
                PortfolioDate = submitDate;
            }
            if (SubmitDate == null || SubmitDate != submitDate)
            {
                SubmitDate = submitDate;
            }
            if (UptosChecked == null || UptosChecked != portfolio.IsUptos)
            {
                UptosChecked = portfolio.IsUptos;
            }
            List<Portfolio> portfolioList = new List<Portfolio>();
            portfolioList.Add(portfolio);
            PortfolioList = null;
            if (!mPortfolioHash.ContainsKey(portfolio.ID))
            {
                mPortfolioHash.Add(portfolio.ID, portfolio);
            }
            PortfolioList = portfolioList;
            List<Path> pathList = PathList;
            if (path != null)
            {
                pathList.Insert(0, path);
            }
            PathList = null;
            PathList = pathList;
            if (path == null || PathList == null)
            {
                Retrieve();
            }
            else
            {
                if (PathList[PathList.Count - 1].AvgDa != null)
                {
                    RetrieveFetchDataAndUpdateChartCommand();
                }
            }
        }

        public void CheckExposure(bool IsExposure, bool IsDollar, bool isXml, DateTime startDate)
        {
            mPathDetailHash.Clear();
            DateTime similarDate = DateTime.Today;
            //similarDate = DBAccess.GetSimilarDate(SubmitDate);
            Dictionary<int, List<DateTime>> nodeHash = new Dictionary<int, List<DateTime>>();
            foreach (Path path in PathList)
            {
                if (!path.Submit)
                {
                    continue;
                }
                if (path.Sink == "")
                {
                    path.Sink = null;
                }
                int source = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                int sink = path.Sink == null ? 0 : DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;
                string[] hours = path.AnalysisType.Split('.');
                List<DateTime> marketDateList = new List<DateTime>();
                if (nodeHash.ContainsKey(source))
                {
                    marketDateList = nodeHash[source];
                    nodeHash.Remove(source);
                }
                foreach (string hour in hours)
                {
                    DateTime marketDateTime = similarDate.AddHours(Int32.Parse(hour));
                    if (!marketDateList.Contains(marketDateTime))
                    {
                        marketDateList.Add(marketDateTime);
                    }
                }
                nodeHash.Add(source, marketDateList);
                if (sink != 0)
                {
                    if (nodeHash.ContainsKey(sink))
                    {
                        marketDateList = nodeHash[sink];
                        nodeHash.Remove(sink);
                    }
                    foreach (string hour in hours)
                    {
                        //DateTime marketDateTime = similarDate.AddHours(Int32.Parse(hour));
                        DateTime marketDateTime = ClearingDate.AddHours(Int32.Parse(hour));
                        if (!marketDateList.Contains(marketDateTime))
                        {
                            marketDateList.Add(marketDateTime);
                        }
                    }
                    nodeHash.Add(sink, marketDateList);
                }

                DARTNode.GetDartMarket(nodeHash, "da", 9);



            }

            Dictionary<int, Dictionary<int, Sensitivity>> nodeSensitivityHash;

            if (NewConstraintChecked)
            {
                nodeSensitivityHash = _dataService.GetSensitivityByConstraintIDsForNewConstraints(SubmitDate);
            }
            else
            {
                nodeSensitivityHash = _dataService.GetSensitivityByConstraintIDs(startDate);
            }

            double maxLoad = _dataService.GetMaxLoad(SubmitDate);
            CalculateConstraint(IsExposure, IsDollar, similarDate, nodeSensitivityHash, maxLoad, isXml);
        }

        public void Submit()
        {  
            if (PortfolioListSelected != null)
            {
                

                MessageBoxResult result = MessageBox.Show("Are you sure you want to submit For Portfolio :" + PortfolioListSelected + " ?", "Warning!", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    int count = _dataService.CheckSubmittedPortfolio(SubmitDate, PortfolioListSelected.ID);
                    if (count > 0)
                    {
                        MessageBoxResult result1 = MessageBox.Show("For " + PortfolioListSelected + " already submitted " + count + " Bids. Do you want to submit it Again", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result1 == MessageBoxResult.Yes)
                        {
                            SubmitSaveFile(false);
                        }
                        else
                        {

                        }

                    }

                    else
                    {
                        SubmitSaveFile(false);

                    }
                    
                }
                else
                {

                }

            }
            else
            {
                MessageBox.Show("Please select Portfolio");
            }
        }

        public void FinalSubmit(bool isXml)
        {
            DateTime submitDate = SubmitDate;
            TimeZone timeZone = TimeZone.CurrentTimeZone;
            if (timeZone.StandardName.StartsWith("East"))
            {
                submitDate = submitDate.AddHours(1);
            }
            List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> virtualList = PortfolioListSelected.IsUptos ? GetUptosList(false) : GetVirtualList(false, GetMarketKey(PortfolioListSelected.Market));
            if (virtualList == null)
            {
                return;
            }
            DuplexChannelFactory<IVirtual> pipeFactory = null;
            DuplexChannelFactory<IBidSubmit> pipeFactoryErcot = null;
            if (PortfolioListSelected.Market == "ERCOT")
                pipeFactoryErcot = GetProxyErcot();
            else
                pipeFactory = GetProxy();

            if (pipeFactoryErcot == null && PortfolioListSelected.Market == "ERCOT")
            {
                return;
            }
            IBidSubmit submitProxyErcot = null;
            IVirtual submitProxy = null;
            if (PortfolioListSelected.Market == "ERCOT")
                submitProxyErcot = pipeFactoryErcot.CreateChannel();
            else
                submitProxy = pipeFactory.CreateChannel();
            Vayu.VirtualBidSubmissionLibrary.VirtualBid[] virtuals = virtualList.ToArray<Vayu.VirtualBidSubmissionLibrary.VirtualBid>();
            if (PortfolioListSelected.IsUptos && mCancelNodeList != null && mCancelNodeList.Count > 0)
            {
                List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> cancelBidList = new List<Vayu.VirtualBidSubmissionLibrary.VirtualBid>();
                foreach (int nodeKey in mCancelNodeList)
                {
                    Vayu.VirtualBidSubmissionLibrary.VirtualBid bid = new Vayu.VirtualBidSubmissionLibrary.VirtualBid();
                    string location = DBAccess.GetNode(nodeKey, PortfolioListSelected.MarketKey).NodeName;
                    int market = GetMarketKey(PortfolioListSelected.Market);
                    if (market == 1)
                    {
                        PricingNode node = DBAccess.GetNode(nodeKey);
                        location = node.ExternalNodeId.ToString();
                    }
                    cancelBidList.Add(bid);
                }
                submitProxy.Cancel(cancelBidList.ToArray<Vayu.VirtualBidSubmissionLibrary.VirtualBid>(), submitDate, PortfolioListSelected.ID);
            }
            string resultString = "";

            List<PTPBid> ptpBidList = new List<PTPBid>();
            // List<PTPBid> finalErcotPtpBidList = new List<PTPBid>();
            for (int i = 0; i < virtuals.Length; i++)
            {
                Vayu.VirtualBidSubmissionLibrary.VirtualBid oldBid = virtuals[i];
                List<BidValues> intervalList = new List<BidValues>();
                PTPBid bid = new PTPBid();
                bid.Source = oldBid.Source;
                bid.Sink = oldBid.Sink;
                bid.PortfolioKey = PortfolioListSelected.ID;
                bid.RequestID = "QENJRE." + DateTime.Today.AddDays(1).ToString("yyyyMMdd") + ".PTP." + oldBid.BidId + "." + oldBid.Source + "." + oldBid.Sink;
                BidValues interval = new BidValues();

                interval.Hour = oldBid.Hour;
                interval.MW = oldBid.MW;
                interval.Price = oldBid.Price;
                intervalList.Add(interval);
                bid.Bidvals = intervalList.ToArray();
                bid.BidId = oldBid.BidId;
                ptpBidList.Add(bid);
                // bid.Bidvals
            }
            PTPBid[] ptpbidArr = ptpBidList.ToArray();
            try
            {
                resultString = submitProxyErcot.SubmitBids(ptpbidArr, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                for (int index = 0; index < App.Current.Windows.Count; index++)
                {
                    if (App.Current.Windows[index].Title == "DART Portfolio")
                    {
                        MessageBoxResult result = MessageBox.Show(App.Current.Windows[index], "Submission For Portfolio :" + PortfolioListSelected + " " + resultString, "Success!", MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                        {
                            if (pipeFactory != null)
                            {
                                pipeFactory.Close();
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            Retrieve();
        }

        private DuplexChannelFactory<IVirtual> GetProxy()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.OpenTimeout = new TimeSpan(0, 12, 0);
            binding.SendTimeout = new TimeSpan(0, 12, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 12, 0);
            binding.CloseTimeout = new TimeSpan(0, 12, 0);
            binding.Security.Mode = SecurityMode.None;
            DuplexChannelFactory<IVirtual> pipeFactory = null;

            if (PortfolioListSelected.Market == "ERCOT" && PortfolioListSelected.IsUptos == false)
            {
                pipeFactory = new DuplexChannelFactory<IVirtual>
                            (new InstanceContext(this), binding, new EndpointAddress(""));
            }

            if (PortfolioListSelected.Market == "ERCOT" && PortfolioListSelected.IsUptos == true)
            {
                pipeFactory = new DuplexChannelFactory<IVirtual>
                            (new InstanceContext(this), binding, new EndpointAddress(Vayu.CommonAccessLibrary.ServiceConnections.GetErcotPtpUpload()));
            }
            return pipeFactory;
        }
        private DuplexChannelFactory<IBidSubmit> GetProxyErcot()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.OpenTimeout = new TimeSpan(0, 120, 0);
            binding.SendTimeout = new TimeSpan(0, 120, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 120, 0);
            binding.CloseTimeout = new TimeSpan(0, 120, 0);
            binding.MaxBufferSize = 1500000;
            binding.MaxReceivedMessageSize = 1500000;
            binding.Security.Mode = SecurityMode.None;
            DuplexChannelFactory<IBidSubmit> pipeFactory = null;

            if (PortfolioListSelected.Market == "ERCOT" && PortfolioListSelected.IsUptos == true)
            {
                pipeFactory = new DuplexChannelFactory<IBidSubmit>
                            (new InstanceContext(this), binding, new EndpointAddress(Vayu.CommonAccessLibrary.ServiceConnections.GetErcotPtpUpload()));
            }
            return pipeFactory;

        }

        public void DeletePath(List<Path> pathList, List<Path> DeleteSelectedPathList)
        {
            foreach (Path path in DeleteSelectedPathList)
            {
                if (path.Status.ToUpper() != "VALID")
                {
                    try
                    {
                        if (path.IsUptos)
                            DBAccess.DeletePathAsync(path.PortfolioDate, path.Source, path.Sink, path.Price, path.MW, path.PortfolioKey, UptosChecked, path.Market);
                        else
                            DBAccess.DeletePath(path.Market, path.BidId, PortfolioDate, path.IsUptos);
                    }
                    catch
                    {

                    }

                }
            }
            Retrieve();
            mRefreshGraphs = true;
            CountRows();
        }

        public void Copy()
        {
            if (PathList == null)
            {
                return;
            }
            if (PathList.Count > 0)
            {
                if (PortfolioListSelected == null)
                {
                    MessageBox.Show("Please select source portfolio");
                    return;
                }
                if (TraderPortfolioComboSelectedItem == null)
                {
                    MessageBox.Show("Please select destination portfolio");
                    return;
                }
                CopyWindow copyWindow = new CopyWindow();
                CopyWindowViewModel copyWindowViewModel = new CopyWindowViewModel(this);
                bool isAll = SelectedPathByCell == null || SelectedPathByCell.Count == 0 ? true : false;
                copyWindowViewModel.SetChecked(isAll);
                copyWindow.DataContext = copyWindowViewModel;
                copyWindow.ShowDialog();
            }
        }

        public void CopyPaths(bool isAll, DateTime toDate)
        {
            DBAccess.DeletePortfolio(_User, TraderPortfolioComboSelectedItem, toDate);
            date = toDate;
            if (!mPortfolioHash.ContainsKey(TraderPortfolioComboSelectedItem.ID))
            {
                mPortfolioHash.Add(TraderPortfolioComboSelectedItem.ID, TraderPortfolioComboSelectedItem);
            }
            List<Bid> bidList = new List<Bid>();
            List<ValidateBids> validateBidsList = new List<ValidateBids>();
            Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> decHash = new Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>>();
            Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> incHash = new Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>>();
            Dictionary<string, Bid> bidHash = new Dictionary<string, Bid>();
            List<Path> pathList = new List<Path>();
            int marketKey = TraderPortfolioComboSelectedItem.Market == "PJM" ? 1 : 9;
            if (isAll)
            {
                foreach (Path path in PathList)
                {
                    pathList.Add(path);
                    ValidateBids validateBids = new ValidateBids();
                    PricingNode sourceNode = DBAccess.GetNodeFromName(path.Source, marketKey);
                    PricingNode sinkNode = DBAccess.GetNodeFromName(path.Sink, marketKey);
                    validateBids.Source = sourceNode.NodeKey;
                    validateBids.Sink = sinkNode.NodeKey;
                    validateBids.AnalysisType = path.AnalysisType;
                    validateBids.Price = path.Price;
                    validateBids.MW = path.MW;
                    validateBids.BidID = path.BidId;
                    validateBidsList.Add(validateBids);
                }
            }
            else
            {
                foreach (Path path in SelectedPathByCell)
                {
                    pathList.Add(path);
                    ValidateBids validateBids = new ValidateBids();
                    PricingNode sourceNode = DBAccess.GetNodeFromName(path.Source, marketKey);
                    PricingNode sinkNode = DBAccess.GetNodeFromName(path.Sink, marketKey);
                    validateBids.Source = sourceNode.NodeKey;
                    validateBids.Sink = sinkNode.NodeKey;
                    validateBids.AnalysisType = path.AnalysisType;
                    validateBids.Price = path.Price;
                    validateBids.MW = path.MW;
                    validateBids.BidID = path.BidId;
                    validateBidsList.Add(validateBids);
                }
            }
            foreach (Path path in pathList)
            {
                if (path.PortfolioKey != PortfolioListSelected.ID)
                {
                    continue;
                }
                Thread.Sleep(5);
                var tempBidId = _dataService.GetBidId(TraderPortfolioComboSelectedItem.ID);
                string[] hours = path.AnalysisType.Split('.');
                foreach (string hour in hours)
                {

                    Bid bid = new Bid();
                    bid.PortfolioKey = TraderPortfolioComboSelectedItem.ID;
                    bid.Market = path.Market;
                    bid.MarketDateTime = toDate.AddHours(Int32.Parse(hour));
                    bid.Status = "IMPORTED";
                    bid.IsUptos = path.IsUptos;
                    bid.Source = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                    bid.Price = path.Price;
                    bid.MW = path.MW;
                    bid.Comments = path.Comments;
                    if (path.IsUptos)
                    {
                        if (path.Market == 1)
                        {
                            bid.BidId = tempBidId.ToString() + ".1";
                        }
                        else
                        {
                            bid.BidId = path.BidId;
                        }
                        bid.Sink = DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;
                        bidList.Add(bid);
                    }
                    else
                    {
                        bid.BidId = path.BidId;
                        string key = path.Source + hour + bid.MW + bid.Price;
                        SetIncDecVirtualHash(decHash, incHash, path);
                        bidHash.Add(key, bid);
                    }
                }
            }
            if (bidList.Count == 0)
            {
                List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> virtualList = new List<Vayu.VirtualBidSubmissionLibrary.VirtualBid>();
                for (int i = 0; i < 2; i++)
                {
                    Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> incDecHash = i == 0 ? decHash : incHash;
                    List<string> keyList = incDecHash.Keys.ToList<string>();
                    foreach (string key in keyList)
                    {
                        Dictionary<int, Dictionary<double, List<double>>> hourHash = incDecHash[key];
                        List<int> hourKeyList = hourHash.Keys.ToList<int>();
                        hourKeyList.Sort();
                        foreach (int hour in hourKeyList)
                        {
                            Dictionary<double, List<double>> priceHash = hourHash[hour];
                            List<double> priceList = priceHash.Keys.ToList<double>();
                            priceList.Sort();
                            if (i == 0)
                            {
                                priceList.Reverse();
                            }
                            int segment = 1;
                            foreach (double price in priceList)
                            {
                                List<double> mwList = priceHash[price];
                                mwList.Sort();
                                foreach (double mw in mwList)
                                {
                                    double bidMW = i == 1 ? mw * -1 : mw;
                                    string bidKey = key + hour + bidMW + price;
                                    Bid bid = bidHash[bidKey];
                                    bid.Segment = segment;
                                    segment++;
                                    bidList.Add(bid);
                                }
                            }
                        }
                    }
                }
            }
            if (bidList.Count > 0)
            {
                if (ValidateBids(validateBidsList))
                {
                    DBAccess.SaveBids(bidList, _User);
                }
                Portfolio portfolio = TraderPortfolioComboSelectedItem;
                RemoveAllPortfolio();
                PortfolioDate = toDate;
                List<Portfolio> portfolioList = new List<Portfolio>();
                portfolioList.Add(portfolio);
                PortfolioList = portfolioList;
                Retrieve();
            }
        }

        public bool ValidateBids(List<ValidateBids> bidList)
        {
            StackTrace stackTrace = new StackTrace();
            int marketKey = 9;
            List<ValidateBids> tempPaths = bidList;
            List<ValidateBids> tempList = tempPaths;
            DateTime date1 = date;
            try
            {
                if (stackTrace.GetFrame(1).GetMethod().Name == "Import")
                {
                    if (tempPaths.Count > 0)
                    {
                        foreach (var item in tempList)
                        {
                            for (int i = tempPaths.Count - 1; i >= 0; i--)
                            {
                                if (item.BidID != tempPaths[i].BidID)
                                {
                                    if (item.Source == tempPaths[i].Source && item.Sink == tempPaths[i].Sink && item.Price == tempPaths[i].Price && item.MW == tempPaths[i].MW &&
                                                                                                                    item.AnalysisType.Equals(tempPaths[i].AnalysisType))
                                    {
                                        string sourceName = string.Empty;
                                        string sinkName = string.Empty;
                                        if (MarketComboSelectedValue == "ERCOT")
                                        {
                                            sourceName = DBAccess.GetNode(item.Source, 9).NodeName;
                                            sinkName = DBAccess.GetNode(item.Sink, 9).NodeName;
                                        }


                                        MessageBox.Show("Source " + sourceName + " Sink " + sinkName + " Price " + item.Price + " MW " + item.MW + " Hours " + item.AnalysisType +
                                            " is duplicated");
                                        return false;
                                    }
                                }
                            }
                        }
                        SetPortfolioList();
                        this.PortfolioComboSelectedItem = TraderPortfolioComboSelectedItem;
                        CurrentPortfolio = TraderPortfolioComboSelectedItem;
                        AddPortfolio();
                        Retrieve();
                        List<Path> tempPath = PathList;
                        foreach (var eachGridValue in PathList)
                        {
                            PricingNode sourceNode = DBAccess.GetNodeFromName(eachGridValue.Source, marketKey);
                            PricingNode sinkNode = DBAccess.GetNodeFromName(eachGridValue.Sink, marketKey);
                            foreach (var eachFileRow in tempPaths)
                            {
                                if (eachGridValue.BidId != eachFileRow.BidID)
                                {
                                    if (sourceNode.NodeKey == eachFileRow.Source && sinkNode.NodeKey == eachFileRow.Sink && eachGridValue.Price == eachFileRow.Price &&
                                        eachGridValue.MW == eachFileRow.MW && eachGridValue.AnalysisType.Equals(eachFileRow.AnalysisType))
                                    {
                                        MessageBox.Show("Source " + sourceNode.NodeName + " Sink " + sourceNode.NodeName + " Price " + eachGridValue.Price + " MW " + eachFileRow.MW +
                                            " Hours " + eachGridValue.AnalysisType + " is duplicated");
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                if (stackTrace.GetFrame(1).GetMethod().Name == "SaveCommand" || stackTrace.GetFrame(1).GetMethod().Name == "RowsUpdated")
                {

                    List<Path> tempPath = new List<Path>();
                    if (stackTrace.GetFrame(1).GetMethod().Name == "RowsUpdated")
                    {
                        foreach (var item in PathList)
                        {
                            foreach (var item1 in tempPaths)
                            {
                                if (item.BidId != item1.BidID)
                                {
                                    tempPath.Add(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        tempPath = PathList;
                    }
                    foreach (var eachGridValue in tempPath)
                    {
                        PricingNode sourceNode = DBAccess.GetNodeFromName(eachGridValue.Source, marketKey);
                        PricingNode sinkNode = DBAccess.GetNodeFromName(eachGridValue.Sink, marketKey);
                        foreach (var item in tempPaths)
                        {
                            if (sourceNode.NodeKey == item.Source && sinkNode.NodeKey == item.Sink && eachGridValue.Price == item.Price &&
                                eachGridValue.MW == item.MW && eachGridValue.AnalysisType.Equals(item.AnalysisType))
                            {
                                MessageBox.Show("Source " + sourceNode.NodeName + " Sink " + sourceNode.NodeName + " Price " + eachGridValue.Price + " MW " + eachGridValue.MW +
                                             " Hours " + eachGridValue.AnalysisType + " is duplicated");
                                return false;
                            }
                        }
                    }
                }
                if (stackTrace.GetFrame(1).GetMethod().Name == "CopyPaths")
                {
                    SetPortfolioList();
                    this.PortfolioComboSelectedItem = TraderPortfolioComboSelectedItem;
                    CurrentPortfolio = TraderPortfolioComboSelectedItem;
                    AddPortfolio();
                    PortfolioDate = date;
                    Retrieve();
                    List<Path> tempPath = PathList;
                    foreach (var eachGridValue in PathList)
                    {
                        PricingNode sourceNode = DBAccess.GetNodeFromName(eachGridValue.Source, marketKey);
                        PricingNode sinkNode = DBAccess.GetNodeFromName(eachGridValue.Sink, marketKey);
                        foreach (var eachFileRow in tempPaths)
                        {
                            if (eachGridValue.BidId != eachFileRow.BidID)
                            {
                                if (sourceNode.NodeKey == eachFileRow.Source && sinkNode.NodeKey == eachFileRow.Sink && eachGridValue.Price == eachFileRow.Price &&
                                    eachGridValue.MW == eachFileRow.MW && eachGridValue.AnalysisType.Equals(eachFileRow.AnalysisType))
                                {
                                    MessageBox.Show("Source " + sourceNode.NodeName + " Sink " + sourceNode.NodeName + " Price " + eachGridValue.Price + " MW " + eachFileRow.MW +
                                            " Hours " + eachGridValue.AnalysisType + " is duplicated");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        public void Delete()
        {
            if (PortfolioListSelected == null)
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to Delete", "Delete", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                DBAccess.DeletePortfolio(_User, PortfolioListSelected, PortfolioDate);
                RemovePortfolio();
                SetPortfolioList();
            }
        }

        static bool CheckDecimalPlaces(double value, bool price)
        {
            decimal decimalValue = (decimal)value;
            decimal fractionalPart = decimalValue - Math.Truncate(decimalValue);
            string fractionalString = fractionalPart.ToString().TrimEnd('0');

            int decimalPlaces = fractionalString.Contains(".")
                ? fractionalString.Split('.')[1].Length
                : 0;
            if(price)
            return decimalPlaces > 2;
            else
            return decimalPlaces > 1;

        }

        static bool AreHoursInOrder(string hourstrs)
        {
            var hours = hourstrs
    .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList();
            for (int i = 1; i < hours.Count; i++)
            {
                if (hours[i] < hours[i - 1])
                {
                    return false;
                }
            }
            return true;
        }

        public void Import()
        {
            RemoveAllPortfolio();
            List<ValidateBids> validateBidsList = new List<ValidateBids>();
            List<string> importList = new List<string>();
            List<SourceSinkData> ListSourceSinkData = new List<SourceSinkData>();
            if (TraderPortfolioComboSelectedItem == null)
            {
                MessageBox.Show("Please select portfolio");
                return;
            }
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = fileDialog.ShowDialog();
            if (result == true)
            {
                MessageBoxResult msgResult = MessageBox.Show("OverWrite?", "Over Write", MessageBoxButton.YesNo);
                if (msgResult == MessageBoxResult.Yes)
                {
                    DBAccess.DeletePortfolio(_User, TraderPortfolioComboSelectedItem, PortfolioDate);
                }
                List<string> ignoreList = new List<string>();
                List<string> forfeitureList = null; //CheckForfeiture(TraderPortfolioComboSelectedItem.ID) ? DBAccess.GetForfeitureUptosList() : null;
                List<Bid> bidList = new List<Bid>();
                using (FileStream stream = File.Open(fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StreamReader fs = new StreamReader(stream);
                    string line = fs.ReadLine();
                    line = fs.ReadLine();
                    DateTime now = DateTime.Now.AddHours(-4);
                    int marketKey = TraderPortfolioComboSelectedItem.Market == "PJM" ? 1 : 9;
                    var tempBidId = _dataService.GetBidId(TraderPortfolioComboSelectedItem.ID);

                    int counter = 1;
                    while (line != null)
                    {
                        try
                        {
                            SourceSinkData sourcesink = new SourceSinkData();
                            string[] tokens = line.Split(',');

                            //for (int i = 0; i < tokens.Length; i++)
                            //{
                            //    tokens[i] = Regex.Replace(tokens[i], "[^\\w\\.]", "");
                            //}

                            string source = TraderPortfolioComboSelectedItem.Market == "PJM" || TraderPortfolioComboSelectedItem.Market == "ERCOT" ? tokens[0] : tokens[2];
                            string sink = TraderPortfolioComboSelectedItem.Market == "PJM" || TraderPortfolioComboSelectedItem.Market == "ERCOT" ? tokens[1] : tokens[3];
                            if (source.Trim().Length == 0 || sink.Trim().Length == 0)
                            {
                                break;
                            }
                            PricingNode sourceNode = DBAccess.GetNodeFromName(source, marketKey);
                            PricingNode sinkNode = DBAccess.GetNodeFromName(sink, marketKey);
                            string hourStrs = TraderPortfolioComboSelectedItem.Market == "PJM" || TraderPortfolioComboSelectedItem.Market == "ERCOT" ? tokens[2] : tokens[4];
                            bool isInOrder = AreHoursInOrder(hourStrs);
                            if(!isInOrder)
                            {
                                MessageBox.Show("Hours are Out of order for Path " + source + " => " + sink + "");
                                return;
                            }
                            double price = TraderPortfolioComboSelectedItem.Market == "PJM" || TraderPortfolioComboSelectedItem.Market == "ERCOT" ? double.Parse(tokens[3].Replace("$", "")) : double.Parse(tokens[6].Replace("$", ""));
                            double mw = TraderPortfolioComboSelectedItem.Market == "PJM" || TraderPortfolioComboSelectedItem.Market == "ERCOT" ? double.Parse(tokens[7]) : double.Parse(tokens[10]);
                            string bidId = TraderPortfolioComboSelectedItem.Market == "PJM" ? tempBidId++ + ".1" : "";
                            if(CheckDecimalPlaces(price, true))
                            {
                                MessageBox.Show("Price has More than 2 Decimal Value for Path " + source + " => " + sink + "");
                                return;
                            }
                            if (CheckDecimalPlaces(mw, false))
                            {
                                MessageBox.Show("MW has More than 1 Decimal Value for Path " + source + " => " + sink + "");
                                return;
                            }
                            if (mw > 0)
                            {
                                if (TraderPortfolioComboSelectedItem.Market == "ERCOT" && UptosChecked)
                                {
                                    try
                                    {
                                        if (bidList.Exists(a => a.Source == sourceNode.NodeKey && a.Sink == sinkNode.NodeKey))
                                        {
                                            List<Bid> tempBidList = bidList.FindAll(a => a.Source == sourceNode.NodeKey && a.Sink == sinkNode.NodeKey).ToList();
                                            bool isBreak = false;
                                            foreach (Bid tempBid in tempBidList)
                                            {
                                                string[] existingHrsArr = hourStrs.Split('.');
                                                foreach (string hour in existingHrsArr)
                                                {
                                                    string existingHour = tempBid.MarketDateTime.Hour.ToString();
                                                    if (hour == existingHour)
                                                    {
                                                        //bool exists = bidList.Exists(a => a.Source == sourceNode.NodeKey && a.Sink == sinkNode.NodeKey && a.MW == mw && a.Price == price);
                                                        //if (exists)
                                                        //    continue;
                                                        bidId = TraderPortfolioComboSelectedItem.ID + "_" + counter;
                                                        isBreak = true;
                                                        break;
                                                    }
                                                }
                                                if (isBreak)
                                                    break;
                                            }
                                            if (!isBreak)
                                                bidId = bidList.FirstOrDefault((a => a.Source == sourceNode.NodeKey && a.Sink == sinkNode.NodeKey)).BidId;

                                        }
                                        else
                                            bidId = TraderPortfolioComboSelectedItem.ID + "_" + counter;
                                    }
                                    catch
                                    {

                                    }
                                }
                                string[] hourTokens = hourStrs.Split('.');
                                ValidateBids validateBids = new ValidateBids();
                                validateBids.Source = sourceNode.NodeKey;
                                validateBids.Sink = sinkNode.NodeKey;
                                validateBids.AnalysisType = hourStrs;
                                validateBids.Price = price;
                                validateBids.MW = mw;
                                validateBids.BidID = bidId;
                                string strImport = source + ":" + sink + ":" + hourStrs + ":" + price + ":" + mw;
                                if (!importList.Contains(strImport))
                                {
                                    importList.Add(strImport);
                                    validateBidsList.Add(validateBids);
                                    foreach (string hour in hourTokens)
                                    {
                                        try
                                        {
                                            Bid bid = new Bid();
                                            bid.Source = sourceNode.NodeKey;
                                            bid.Sink = sinkNode.NodeKey;
                                            bid.MW = mw;
                                            bid.Price = price;
                                            bid.Market = marketKey;
                                            bid.MarketDateTime = PortfolioDate.AddHours(Int16.Parse(hour));
                                            bid.PortfolioKey = TraderPortfolioComboSelectedItem.ID;
                                            bid.BidId = bidId;
                                            bid.IsUptos = true;
                                            bidList.Add(bid);
                                        }
                                        catch (Exception ec)
                                        {
                                            Debug.WriteLine(ec.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("MW less than 0 for path " + source + " => " + sink + "");
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        line = fs.ReadLine();
                        counter += 1;
                    }
                    fs.Close();

                    if (marketKey == 3)
                    {
                        bidList.RemoveAll(a => a.Segment > 3);
                    }
                    if (ValidateBids(validateBidsList))
                    {
                        DBAccess.SaveBids(bidList, _User);
                    }
                    SetPortfolioList();
                    this.PortfolioComboSelectedItem = TraderPortfolioComboSelectedItem;
                    CurrentPortfolio = TraderPortfolioComboSelectedItem;
                    AddPortfolio();
                    Retrieve();
                }
            }
        }

        public string GetDeenergizedNodes(List<SourceSinkData> listSourceSinkData)
        {
            string hours = string.Empty, source = string.Empty, sink = string.Empty;
            try
            {
                List<DeenergizedNode> filterSourceNodes = new List<DeenergizedNode>();
                List<DeenergizedNode> filterSinkNodes = new List<DeenergizedNode>();
                ListDeenergizedNodes = _dataService.GetDeenergizedNodes();
                List<DeenergizedNode> FilterSourcelist = new List<DeenergizedNode>();
                List<DeenergizedNode> FilterSinklist = new List<DeenergizedNode>();
                List<int> listhour = new List<int>();
                foreach (var item in listSourceSinkData)
                {

                    source = item.Source == null ? " " : item.Source.ToString();
                    sink = item.Sink == null ? " " : item.Sink.ToString();
                    FilterSourcelist = ListDeenergizedNodes.Where(a => a.Name == item.Source.ToString() && listhour.Contains(a.Hours)).ToList();
                    FilterSinklist = ListDeenergizedNodes.Where(a => a.Name == item.Sink.ToString() && listhour.Contains(a.Hours)).ToList();
                    filterSourceNodes.AddRange(FilterSourcelist);
                    filterSinkNodes.AddRange(FilterSinklist);
                }
                if (filterSourceNodes.Count() > 0)
                {
                    var list = from element in filterSourceNodes orderby element.Name select element;
                    string Name = string.Empty, str = " is Deenergized Nodes for Hours ";
                    foreach (var item in list.Distinct().ToList())
                    {
                        if (Name != item.Name)
                        {
                            Name = item.Name;
                            hours += "\n" + Name + str + item.Hours.ToString() + ",";
                        }
                        else
                        {
                            hours += item.Hours.ToString() + ",";
                        }
                    }
                    hours = hours.Remove(hours.Length - 1);
                    // hours += " \n";
                    Name = string.Empty;
                }
                if (filterSinkNodes.Count > 0)
                {
                    var list = from element in filterSinkNodes orderby element.Name select element;
                    string Name = string.Empty, str = " is Deenergized Nodes for Hours ";
                    foreach (var item in list.Distinct().ToList())
                    {
                        if (Name != item.Name)
                        {
                            Name = item.Name;
                            hours += "\n" + Name + str + item.Hours.ToString() + ",";
                        }
                        else
                        {
                            hours += item.Hours.ToString() + ",";
                        }
                    }
                    hours = hours.Remove(hours.Length - 1);
                    // hours += " \n";
                }
            }
            catch (Exception)
            {

            }
            return hours.ToString();
        }

        public void Reset()
        {
            RemoveAllPortfolio();
            RemoveAllFilters();
            HE1Checked = true;
            HE2Checked = true;
            HE3Checked = true;
            HE4Checked = true;
            HE5Checked = true;
            HE6Checked = true;
            HE7Checked = true;
            HE8Checked = true;
            HE9Checked = true;
            HE10Checked = true;
            HE12Checked = true;
            HE13Checked = true;
            HE14Checked = true;
            HE15Checked = true;
            HE16Checked = true;
            HE17Checked = true;
            HE18Checked = true;
            HE19Checked = true;
            HE20Checked = true;
            HE21Checked = true;
            HE22Checked = true;
            HE23Checked = true;
            HE24Checked = true;
            SundayChecked = true;
            MondayChecked = true;
            TuesdayChecked = true;
            WednesdayChecked = true;
            ThursdayChecked = true;
            FridayChecked = true;
            SaturdayChecked = true;
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
        }
        #region Commentd by Sateesh

        //public void Retrieve()
        //{
        //    List<Bid> bidsList = new List<Bid>();
        //    if (PortfolioList == null)
        //    {
        //        return;
        //    }
        //    foreach (Portfolio portfolio in PortfolioList)
        //    {
        //        string savedName = portfolio.IsUptos ? null : portfolio.Name;
        //        List<Bid> bidList = DBAccess.GetBids(portfolio.Market, portfolio.ID, savedName, PortfolioDate, PortfolioDate.AddDays(1),
        //                                portfolio.IsUptos, "MOVED");
        //        bidsList.AddRange(bidList);
        //    }
        //    if (PortfolioDate > DateTime.Today)
        //    {
        //        SubmitDate = PortfolioDate;
        //    }
        //    PathList = null;

        //    Dictionary<string, Path> pathHash = new Dictionary<string, Path>();
        //    List<Path> tempPathList = new List<Path>();

        //    foreach (Bid bItem in bidsList)
        //    {
        //        Path path = new Path();
        //        PricingNode sourcePricingNode = DBAccess.GetNode(bItem.Source, bItem.Market);
        //        path.Source = sourcePricingNode.NodeName;
        //        path.SourcePNodeId = sourcePricingNode.ExternalNodeId;
        //        path.SourceZone = sourcePricingNode.Zone;
        //        if (bItem.Sink != 0)
        //        {
        //            PricingNode sinkPricingNode = DBAccess.GetNode(bItem.Sink, bItem.Market);
        //            path.Sink = sinkPricingNode.NodeName;
        //            path.SinkZone = sinkPricingNode.Zone;
        //            path.SinkPNodeId = sinkPricingNode.ExternalNodeId;
        //        }
        //        else
        //        {
        //            path.Sink = "";
        //            path.SinkZone = "";
        //        }

        //        if (bItem.MarketDateTime.Hour == 0)
        //        {
        //            path.AnalysisType = "24";
        //        }
        //        else
        //        {
        //            path.AnalysisType = bItem.MarketDateTime.Hour.ToString();
        //        }
        //        path.Price = bItem.Price;
        //        path.MW = bItem.MW;
        //        path.Status = bItem.Status;
        //        path.BidId = bItem.BidId;
        //        path.IsUptos = bItem.IsUptos;
        //        path.Portfolio = bItem.PortfolioKey == 0 ? bItem.File : mPortfolioHash[bItem.PortfolioKey].Name;
        //        path.PortfolioKey = bItem.PortfolioKey;
        //        path.Submit = true;
        //        path.Market = bItem.Market;
        //        path.RiskPath = false;
        //        path.PortfolioDate = PortfolioDate;
        //        path.Comments = bItem.Comments;
        //        path.MarketDateTime = bItem.MarketDateTime;
        //        if (bItem.Market == 1)
        //        {
        //            path.SourceDeenergized = false;
        //            path.SinkDeenergized = false;
        //        }
        //        else if (bItem.Market == 9)
        //        {
        //            if (ListstrDeenergizedNodes.Contains(path.Source))
        //            {
        //                path.SourceDeenergized = true;
        //            }
        //            else if (ListstrDeenergizedNodes.Contains(path.Sink))
        //            {
        //                path.SinkDeenergized = true;
        //            }
        //            else
        //            {
        //                path.SourceDeenergized = false;
        //                path.SinkDeenergized = false;
        //            }
        //        }
        //        Path tempPath = tempPathList.Where(a => a.Source == path.Source && a.Sink == path.Sink && a.Price == bItem.Price && a.MW == bItem.MW && a.PortfolioKey == bItem.PortfolioKey && a.BidId == bItem.BidId).FirstOrDefault();

        //        if (tempPath != null)
        //        {
        //            bool addType = true;
        //            if (!string.IsNullOrEmpty(tempPath.AnalysisType))
        //            {
        //                string[] hours = tempPath.AnalysisType.Split('.');
        //                if (hours.Any(x => x.Equals(path.AnalysisType.Trim())))
        //                    addType = false;
        //            }

        //            if (addType)
        //            {
        //                tempPath.AnalysisType = tempPath.AnalysisType.Trim() + "." + path.AnalysisType.Trim();
        //            }
        //        }
        //        else
        //        {
        //            tempPathList.Add(path);
        //        }
        //    }


        //    PathList = new List<Path>();
        //    List<Path> _pathList = new List<Path>();
        //    foreach (var tPathList in tempPathList)
        //    {
        //        if (tPathList.PortfolioKey == 999)
        //        {
        //            string[] hours = tPathList.AnalysisType.Split('.');

        //            int oldHour = -1;

        //            string hourString = string.Empty;

        //            for (int i = 0; i < hours.Length; i++)
        //            {
        //                if (oldHour == -1)
        //                {
        //                    oldHour = Convert.ToInt32(hours[i].ToString());

        //                    //tPathList.AnalysisType = oldHour.ToString();
        //                    hourString = oldHour.ToString();
        //                }

        //                int newHour = 0;

        //                //int nextHour = Convert.ToInt32(hours[i + 1]);

        //                if ((i + 1) < hours.Length)
        //                {
        //                    newHour = Convert.ToInt32(hours[i + 1].ToString());
        //                }

        //                if ((oldHour + 1) == newHour)
        //                {
        //                    //tPathList.AnalysisType += "." + newHour.ToString();
        //                    hourString += "." + newHour.ToString();

        //                    //PathList.Add(tPathList);

        //                    oldHour++;
        //                }
        //                else
        //                {
        //                    Path tempObjectPath = new Path(tPathList);

        //                    tempObjectPath.AnalysisType = hourString;

        //                    _pathList.Add(tempObjectPath);

        //                    oldHour = newHour;
        //                    hourString = newHour.ToString();
        //                }
        //            }

        //        }
        //        else
        //        {
        //            _pathList.Add(tPathList);
        //        }
        //    }

        //    PathList = _pathList;
        //    //PathList = tempPathList.ToList();

        //    #region old
        //    //foreach (Bid bid in bidsList)
        //    //{
        //    //    Path path = new Path();
        //    //    PricingNode sourcePricingNode = DBAccess.GetNode(bid.Source);
        //    //    path.Source = sourcePricingNode.NodeName;
        //    //    path.SourcePNodeId = sourcePricingNode.ExternalNodeId;
        //    //    path.SourceZone = sourcePricingNode.Zone;
        //    //    if (bid.Sink != 0)
        //    //    {
        //    //        PricingNode sinkPricingNode = DBAccess.GetNode(bid.Sink);
        //    //        path.Sink = sinkPricingNode.NodeName;
        //    //        path.SinkZone = sinkPricingNode.Zone;
        //    //        path.SinkPNodeId = sinkPricingNode.ExternalNodeId;
        //    //        //pathName = sourcePricingNode.NodeName + "->" + sinkPricingNode.NodeName;
        //    //    }
        //    //    else
        //    //    {
        //    //        path.Sink = "";
        //    //        path.SinkZone = "";
        //    //    }
        //    //    //if (!pathComboList.Contains(pathName))
        //    //    //{
        //    //    //    pathComboList.Add(pathName);
        //    //    //}
        //    //    path.MarketDateTime = bid.MarketDateTime;
        //    //    if (bid.MarketDateTime.Hour == 0)
        //    //    {
        //    //        path.AnalysisType = "24";
        //    //    }
        //    //    else
        //    //    {
        //    //        path.AnalysisType = bid.MarketDateTime.Hour.ToString();
        //    //    }
        //    //    path.Price = bid.Price;
        //    //    path.MW = bid.MW;
        //    //    path.Status = bid.Status;
        //    //    path.BidId = bid.BidId;
        //    //    path.IsUptos = bid.IsUptos;
        //    //    path.Portfolio = bid.PortfolioKey == 0 ? bid.File : mPortfolioHash[bid.PortfolioKey].Name;
        //    //    path.PortfolioKey = bid.PortfolioKey;
        //    //    path.Submit = true;
        //    //    path.Market = bid.Market;
        //    //    path.RiskPath = false;
        //    //    path.PortfolioDate = PortfolioDate;
        //    //    path.Comments = bid.Comments;
        //    //    if (pathHash.ContainsKey(bid.BidId))
        //    //    {
        //    //        Path savedPath = pathHash[bid.BidId];
        //    //        if (MarketComboSelectedValue.ToUpper().Equals("ERCOT") && bid.Price != savedPath.Price)
        //    //        {
        //    //            pathHash.Add(bid.BidId + pathHash.Count(), path);
        //    //        }

        //    //        else
        //    //        {
        //    //            savedPath.AnalysisType = savedPath.AnalysisType + "." + path.AnalysisType;
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        pathHash.Add(bid.BidId, path);
        //    //    }
        //    //}
        //    //PathList = pathHash.Values.ToList<Path>();
        //    #endregion
        //    PathComboList = null;
        //    PathComboList = PathList.Select(a => a.Source + "->" + (a.Sink == null ? "" : a.Sink)).Distinct().ToList();
        //    CurrentPortfolio = PortfolioComboSelectedItem;
        //    CountRows();

        //}
        #endregion
        public void Retrieve()
        {
            List<Bid> bidsList = new List<Bid>();
            if (PortfolioList == null)
            {
                return;
            }
            string duplicateHours = "";
            string sourceNames = "";
            string sinkNames = "";
            bool windowShow = false;
            Dictionary<int, double> sourceTradedVolume = DBAccess.GetNodeTradedVolume(PortfolioDate, true);
            Dictionary<int, double> sinkTradedVolume = DBAccess.GetNodeTradedVolume(PortfolioDate, false);
            foreach (Portfolio portfolio in PortfolioList)
            {
                string savedName = portfolio.IsUptos ? null : portfolio.Name;
                List<Bid> bidList = DBAccess.GetBids(portfolio.Market, portfolio.ID, savedName, PortfolioDate, PortfolioDate.AddDays(1),
                                        portfolio.IsUptos, "MOVED");
                bidsList.AddRange(bidList);
            }
            if (PortfolioDate > DateTime.Today)
            {
                SubmitDate = PortfolioDate;
            }
            PathList = null;

            Dictionary<string, Path> pathHash = new Dictionary<string, Path>();
            List<Path> tempPathList = new List<Path>();
            List<int> tradedVolSourceList = new List<int>();
            List<int> tradedVolSinkList = new List<int>();
            foreach (Bid bItem in bidsList)
            {
                Path path = new Path();
                PricingNode sourcePricingNode = DBAccess.GetNode(bItem.Source, bItem.Market);
                path.Source = sourcePricingNode.NodeName;
                if (!sourceTradedVolume.ContainsKey(bItem.Source))
                {
                    if (tradedVolSourceList.Contains(bItem.Source))
                    { }
                    else
                    {
                        if (sourceNames == "")
                        {
                            sourceNames = path.Source;
                        }
                        else
                        {
                            sourceNames = sourceNames + "," + path.Source;
                        }
                        tradedVolSourceList.Add(bItem.Source);
                        windowShow = true;

                    }

                }
                path.SourcePNodeId = sourcePricingNode.ExternalNodeId;
                path.SourceZone = sourcePricingNode.Zone;
                if (bItem.Sink != 0)
                {
                    PricingNode sinkPricingNode = DBAccess.GetNode(bItem.Sink, bItem.Market);
                    path.Sink = sinkPricingNode.NodeName;
                    path.SinkZone = sinkPricingNode.Zone;
                    path.SinkPNodeId = sinkPricingNode.ExternalNodeId;

                    if (!sinkTradedVolume.ContainsKey(bItem.Sink))
                    {
                        if (tradedVolSinkList.Contains(bItem.Sink))
                        { }
                        else
                        {
                            if (sinkNames == "")
                            {
                                sinkNames = path.Sink;
                            }
                            else
                            {
                                sinkNames = sinkNames + "," + path.Sink;
                            }
                            tradedVolSinkList.Add(bItem.Sink);
                            windowShow = true;

                        }

                    }
                }
                else
                {
                    path.Sink = "";
                    path.SinkZone = "";
                }

                if (bItem.MarketDateTime.Hour == 0)
                {
                    path.AnalysisType = "24";
                }
                else
                {
                    path.AnalysisType = bItem.MarketDateTime.Hour.ToString();
                }
                path.Price = bItem.Price;
                path.MW = bItem.MW;
                path.Status = bItem.Status;
                path.BidId = bItem.BidId;
                path.IsUptos = bItem.IsUptos;
                path.Portfolio = bItem.PortfolioKey == 0 ? bItem.File : mPortfolioHash[bItem.PortfolioKey].Name;
                path.PortfolioKey = bItem.PortfolioKey;
                path.Submit = true;
                path.Market = bItem.Market;
                path.RiskPath = false;
                path.PortfolioDate = PortfolioDate;
                path.Comments = bItem.Comments;
                path.MarketDateTime = bItem.MarketDateTime;
                if (bItem.Market == 1)
                {
                    path.SourceDeenergized = false;
                    path.SinkDeenergized = false;
                }
                else if (bItem.Market == 9)
                {
                    if (ListstrDeenergizedNodes.Contains(path.Source))
                    {
                        path.SourceDeenergized = true;
                    }
                    else if (ListstrDeenergizedNodes.Contains(path.Sink))
                    {
                        path.SinkDeenergized = true;
                    }
                    else
                    {
                        path.SourceDeenergized = false;
                        path.SinkDeenergized = false;
                    }
                }
                Path tempPath = null;
                if (MarketComboSelectedValue == "ERCOT External")
                {
                    tempPath = tempPathList.Where(a => a.Source == path.Source && a.Sink == path.Sink && a.Price == bItem.Price && a.MW == bItem.MW && a.PortfolioKey == bItem.PortfolioKey && a.BidId == bItem.BidId).FirstOrDefault();
                }
                else
                {
                    tempPath = tempPathList.Where(a => a.Source == path.Source && a.Sink == path.Sink && a.Price == bItem.Price && a.MW == bItem.MW && a.PortfolioKey == bItem.PortfolioKey && a.BidId == bItem.BidId).FirstOrDefault();

                }


                if (tempPath != null)
                {
                    bool addType = true;
                    if (!string.IsNullOrEmpty(tempPath.AnalysisType))
                    {
                        string[] hours = tempPath.AnalysisType.Split('.');
                        if (hours.Any(x => x.Equals(path.AnalysisType.Trim())))
                        {
                            addType = false;
                            duplicateHours = duplicateHours+" BidID " + path.BidId + " Hour " + path.AnalysisType;

                        }
                            
                    }

                    if (addType)
                    {
                        tempPath.AnalysisType = tempPath.AnalysisType.Trim() + "." + path.AnalysisType.Trim();
                    }
                }
                else
                {
                    tempPathList.Add(path);
                }
            }
            PathList = new List<Path>();
            List<Path> _pathList = new List<Path>();
            foreach (var tPathList in tempPathList)
            {
                if (tPathList.PortfolioKey == 999)
                {
                    string[] hours = tPathList.AnalysisType.Split('.');

                    int oldHour = -1;

                    string hourString = string.Empty;

                    for (int i = 0; i < hours.Length; i++)
                    {
                        if (oldHour == -1)
                        {
                            oldHour = Convert.ToInt32(hours[i].ToString());

                            //tPathList.AnalysisType = oldHour.ToString();
                            hourString = oldHour.ToString();
                        }

                        int newHour = 0;

                        //int nextHour = Convert.ToInt32(hours[i + 1]);

                        if ((i + 1) < hours.Length)
                        {
                            newHour = Convert.ToInt32(hours[i + 1].ToString());
                        }

                        if ((oldHour + 1) == newHour)
                        {
                            //tPathList.AnalysisType += "." + newHour.ToString();
                            hourString += "." + newHour.ToString();

                            //PathList.Add(tPathList);

                            oldHour++;
                        }
                        else
                        {
                            Path tempObjectPath = new Path(tPathList);

                            tempObjectPath.AnalysisType = hourString;

                            _pathList.Add(tempObjectPath);

                            oldHour = newHour;
                            hourString = newHour.ToString();
                        }
                    }

                }
                else
                {
                    _pathList.Add(tPathList);
                }
            }

            PathList = _pathList;
            PathComboList = null;
            PathComboList = PathList.Select(a => a.Source + "->" + (a.Sink == null ? "" : a.Sink)).Distinct().ToList();
            CurrentPortfolio = PortfolioComboSelectedItem;
            if (windowShow)
            {
                MessageBox.Show("Source - " + sourceNames + "\n" + "Sink -" + sinkNames, "No Volume Traded Alert", MessageBoxButton.OK);
            }
            string duplicateHrsAcrossbids = "";
            var groupsWithOverlappingHours = bidsList
           .GroupBy(x => new { x.PortfolioKey, x.Source, x.Sink, x.Price, Hour= x.MarketDateTime.Hour })
           .Where(g => g.Count() > 1)
           .SelectMany(g => g).ToList(); // Check for any duplicates
            foreach (var group in groupsWithOverlappingHours)
            {
                duplicateHrsAcrossbids = duplicateHrsAcrossbids + " BidID " + group.BidId + " Hour " + group.MarketDateTime.Hour;


            }
            if (duplicateHrsAcrossbids != "")
            {
                MessageBox.Show("Portfolio Has Duplicate Hours For Same " + duplicateHrsAcrossbids);

            }
            if (duplicateHours!="")
            {
                MessageBox.Show("Portfolio Has Duplicate Hours For Same " + duplicateHours);
            
            }

            
            CountRows();

        }
        static bool HasDuplicates(List<int> numbers)
        {
            return numbers.Count != numbers.Distinct().Count();
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
                    PortfolioComboSelectedItem.Market = MarketComboSelectedValue;
                    PortfolioComboSelectedItem.IsUptos = UptosChecked;
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
        /// Refreshes the portfolio.
        /// </summary>
        public void RefreshPortfolio()
        {
            if (MarketComboSelectedValue == "ERCOT External")
            {

                PortfolioDate = DateTime.Today.AddDays(-61);
            }
            else
            {
                PortfolioDate = DateTime.Today.AddDays(1);
            }

            //PortfolioDate = DateTime.Today.AddDays(1);
        }
        /// <summary>
        /// Adds the load filters.
        /// </summary>
        public void AddLoadFilters()
        {
            FilterList = null;
            string filterType = FilterLoadComboSelectedValue;
            double? max = null;
            try
            {
                max = double.Parse(MaxLoadText);
            }
            catch (Exception ex)
            {
            }
            double? min = null;
            try
            {
                min = double.Parse(MinLoadText);
            }
            catch (Exception ex)
            {
            }
            FilterData filterData = new FilterData();
            filterData.Product = "Load";
            filterData.Type = filterType;
            filterData.Min = min;
            filterData.Max = max;
            mFillFilterList.Add(filterData);
            FilterList = mFillFilterList;
        }
        /// <summary>
        /// Adds the price filters.
        /// </summary>
        public void AddPriceFilters()
        {
            //FilterList = null;
            string filterType = FilterPriceComboSelectedValue;
            double? max = null;
            try
            {
                max = double.Parse(MaxPriceText);
            }
            catch (Exception ex)
            {
            }
            double? min = null;
            try
            {
                min = double.Parse(MinPriceText);
            }
            catch (Exception ex)
            {
            }
            FilterData filterData = new FilterData();
            filterData.Product = "Price";
            filterData.Type = filterType;
            filterData.Min = min;
            filterData.Max = max;
            mFillFilterList.Add(filterData);
            FilterList = mFillFilterList;
        }
        /// <summary>
        /// Removes the filters.
        /// </summary>
        public void RemoveFilters()
        {
            FilterData filterData = FilterDataSelected;
            mFillFilterList.Remove(filterData);
            FilterList = null;
            FilterList = mFillFilterList;
        }
        /// <summary>
        /// Removes all filters.
        /// </summary>
        public void RemoveAllFilters()
        {
            mFillFilterList = new List<FilterData>();
            FilterList = null;
            FilterList = mFillFilterList;
        }
        /// <summary>
        /// Seasons this instance.
        /// </summary>
        public void Season()
        {
            StartDate = DateTime.Today.AddYears(-1).AddMonths(-1);
            EndDate = DateTime.Today.AddYears(-1).AddMonths(1);
        }
        /// <summary>
        /// Retrieves the fetch data and update chart command.
        /// </summary>
        public void RetrieveFetchDataAndUpdateChartCommand()
        {
            //Stopwatch stopwatch = Stopwatch.StartNew();
            loadDictHash = new Dictionary<string, double>();
            clearedMsHash = new Dictionary<DateTime, double>();
            FetchAllPortfolioData(true);
            mRefreshGraphs = false;
            //stopwatch.Stop();
            //  Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

        }

        /// <summary>
        /// Updates the refresh.
        /// </summary>
        /// <param name="update">if set to <c>true</c> [update].</param>
        public void UpdateRefresh(bool update)
        {
            mRefresh = update;
        }
        /// <summary>
        /// Updates the refresh path.
        /// </summary>
        /// <param name="update">if set to <c>true</c> [update].</param>
        public void UpdateRefreshPath(bool update)
        {
            mRefreshPath = update;
        }
        /// <summary>
        /// Fetches all portfolio data.
        /// </summary>
        /// <param name="refreshData">if set to <c>true</c> [refresh data].</param>



        public void FetchAllPortfolioData(bool refreshData)
        {
            //Stopwatch stopwatch = Stopwatch.StartNew();
            LogWriter.WriteLog("New Instance " + '\t' + DateTime.Now.ToString());
            LogWriter.WriteLog("Fetching all portfolio data" + '\t' + DateTime.Now.ToString());
            if (PathList == null)
            {
                return;
            }
            mAsBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            mMustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            Dictionary<int, List<DateTime>> nodeHash = new Dictionary<int, List<DateTime>>();
            double totalMw = 0;
            double totalDa = 0;
            double totalinc = 0;
            double totaldec = 0;
            foreach (Path path in PathList)
            {
                LogWriter.WriteLog("Itterating  for " + path.Source + '\t' + path.Sink + '\t' + DateTime.Now.ToString());
                if (path.Submit == false)
                {
                    continue;
                }
                List<DateTime> marketDateTimeList = new List<DateTime>();
                int sourceNodeKey = DBAccess.GetNodeFromName(path.Source, 9).NodeKey;
                if (nodeHash.ContainsKey(sourceNodeKey))
                {
                    marketDateTimeList = nodeHash[sourceNodeKey];
                    nodeHash.Remove(sourceNodeKey);
                }
                DateTime startDate = StartDate;
                string[] hours = path.AnalysisType.Split('.');
                if (path.MW > 0)
                {
                    totalinc += (hours.Length * path.MW);
                }
                else
                {
                    totaldec += (hours.Length * path.MW);

                }
                totalMw += (hours.Length * path.MW);
                totalDa += (hours.Length * path.MW * path.Price);
                if (RangeChecked == true)
                {
                    while (startDate <= EndDate)
                    {
                        foreach (string hour in hours)
                        {
                            DateTime sendDate = startDate.AddHours(Int32.Parse(hour));
                            if (!marketDateTimeList.Contains(sendDate))
                            {
                                marketDateTimeList.Add(sendDate);
                            }
                        }
                        startDate = startDate.AddDays(1);
                    }
                }
                else
                {
                    foreach (DateTime date in DateCollectionList)
                    {
                        foreach (string hour in hours)
                        {
                            DateTime sendDate = date.AddHours(Int32.Parse(hour));
                            if (!marketDateTimeList.Contains(sendDate))
                            {
                                marketDateTimeList.Add(sendDate);
                            }
                        }
                    }
                }
                nodeHash.Add(sourceNodeKey, marketDateTimeList);
                if (path.Sink != null && path.Sink.Length > 0)
                {
                    marketDateTimeList = new List<DateTime>();
                    int sinkNodeKey = DBAccess.GetNodeFromName(path.Sink, 9).NodeKey;
                    if (nodeHash.ContainsKey(sinkNodeKey))
                    {
                        marketDateTimeList = nodeHash[sinkNodeKey];
                        nodeHash.Remove(sinkNodeKey);
                    }
                    startDate = StartDate;
                    if (RangeChecked == true)
                    {
                        while (startDate <= EndDate)
                        {
                            foreach (string hour in hours)
                            {
                                DateTime sendDate = startDate.AddHours(Int32.Parse(hour));
                                if (!marketDateTimeList.Contains(sendDate))
                                {
                                    marketDateTimeList.Add(sendDate);
                                }
                            }
                            startDate = startDate.AddDays(1);
                        }
                    }
                    else
                    {
                        foreach (DateTime date in DateCollectionList)
                        {
                            foreach (string hour in hours)
                            {
                                DateTime sendDate = date.AddHours(Int32.Parse(hour));
                                if (!marketDateTimeList.Contains(sendDate))
                                {
                                    marketDateTimeList.Add(sendDate);
                                }
                            }
                        }
                    }
                    nodeHash.Add(sinkNodeKey, marketDateTimeList);
                }
            }
            MWText = null;
            //MWText = totalMw.ToString("#,##0.00;(#,##0.00)");
            MWText = totalMw.ToString();
            INCText = totalinc.ToString();
            DECText = totaldec.ToString();
            DAText = null;
            DAText = totalDa.ToString("#,##0;(#,##0)");
            LogWriter.WriteLog("Getting DART Values from service " + '\t' + DateTime.Now.ToString());

            DARTNode.GetDartMarket(nodeHash, "both", 9, false);


            LogWriter.WriteLog(" Completed Getting DART Values from service " + '\t' + DateTime.Now.ToString());

            // stopwatch.Stop();

            // Log the elapsed time using Trace
            // Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

            FetchComponentwise();
            UpdateChartCommand(true);
        }



        public void FetchComponentwise()
        {
            // Stopwatch stopwatch = Stopwatch.StartNew();
            mAsBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            mMustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            nodeHashtemp = new Dictionary<int, List<DateTime>>();
            List<DateTime> dateCollection = new List<DateTime>();
            if (RangeChecked == true)
            {
                DateTime startDate = StartDate;
                while (startDate <= EndDate)
                {
                    dateCollection.Add(startDate);
                    startDate = startDate.AddDays(1);
                }
            }
            else
            {
                foreach (DateTime startDate in DateCollectionList)
                {
                    dateCollection.Add(startDate);
                }
            }
            foreach (Path path in PathList)
            {
                if (path.Submit == false)
                {
                    continue;
                }
                foreach (DateTime startDate in dateCollection)
                {
                    LogWriter.WriteLog("Calculating As Bid and Must Take Pnl  " + '\t' + path.Source + '\t' + path.Sink + '\t' + startDate.ToString() + '\t' + DateTime.Now.ToString());
                    string[] hours = path.AnalysisType.Split('.');
                    foreach (string hour in hours)
                    {
                        DateTime sendDate = startDate.AddHours(Int32.Parse(hour));
                        string sourceKey = sendDate.ToString() + DBAccess.GetNodeFromName(path.Source, 9).NodeKey; ;
                        string sinkKey = path.Sink == "" ? null : sendDate.ToString() + DBAccess.GetNodeFromName(path.Sink, 9).NodeKey; ;
                        double da = double.NaN;
                        double daLmp = double.NaN;
                        double rt = double.NaN;
                        double dart = double.NaN;
                        #region Dart
                        if (DartCheckedcomp)
                        {
                            if (sinkKey != null)
                            {
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                                    {
                                        if (LmpCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                            if (sinkKey != null)
                                            {
                                                rt = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sRTLmpHash[sourceKey].Price);
                                            }
                                        }
                                        else if (CongCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                            if (sinkKey != null)
                                            {
                                                rt = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sRTLmpHash[sourceKey].Congestion);

                                            }
                                        }
                                        else if (LossCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                                            if (sinkKey != null)
                                            {
                                                rt = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sRTLmpHash[sourceKey].Loss);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // rt = 0;
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sRTLmpHash[sourceKey] != null)
                                {
                                    if (LmpCheckedcomp)
                                    {
                                        rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                    }
                                    else if (CongCheckedcomp)
                                    {
                                        rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                    }
                                    else if (LossCheckedcomp)
                                    {
                                        rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                                    }
                                }
                                else
                                {
                                    // rt = 0;
                                }
                            }
                            if (sinkKey != null)
                            {
                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sDALmpHash[sourceKey] != null && DARTNode.sDALmpHash[sinkKey] != null)
                                    {
                                        if (LmpCheckedcomp)
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Price;
                                            if (sinkKey != null)
                                            {
                                                da = (DARTNode.sDALmpHash[sinkKey].Price - DARTNode.sDALmpHash[sourceKey].Price);
                                            }

                                        }
                                        else if (CongCheckedcomp)
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Congestion;
                                            daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                            if (sinkKey != null)
                                            {
                                                da = (DARTNode.sDALmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                                                daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                                            }

                                        }
                                        else if (LossCheckedcomp)
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Loss;
                                            daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                            if (sinkKey != null)
                                            {
                                                da = (DARTNode.sDALmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                                                daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                                            }

                                        }
                                    }
                                    else
                                    {
                                        //  da = 0;
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sDALmpHash[sourceKey] != null)
                                {
                                    if (LmpCheckedcomp)
                                    {
                                        da = DARTNode.sDALmpHash[sourceKey].Price;
                                    }
                                    else if (CongCheckedcomp)
                                    {
                                        da = DARTNode.sDALmpHash[sourceKey].Congestion;
                                        daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                    }
                                    else if (LossCheckedcomp)
                                    {
                                        da = DARTNode.sDALmpHash[sourceKey].Loss;
                                        daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                    }
                                }
                                else
                                {
                                    //  da = 0;
                                }
                            }
                        }
                        #endregion Dart
                        #region RT
                        if (RtCheckedcomp)
                        {
                            if (sinkKey != null)
                            {
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                                    {
                                        if (LmpCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                                                rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                                            }
                                        }
                                        else if (CongCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                                                rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion;
                                            }
                                        }
                                        else if (LossCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                                            if (sinkKey != null)
                                            {
                                                double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                                                rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        rt = 0;
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey))
                                {
                                    if (DARTNode.sRTLmpHash[sourceKey] != null)
                                    {
                                        if (LmpCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                        }
                                        else if (CongCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                        }
                                        else if (LossCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                                        }
                                    }
                                    else
                                    {
                                        rt = 0;
                                    }
                                }
                            }
                            if (sinkKey != null)
                            {
                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sDALmpHash[sourceKey] != null && DARTNode.sDALmpHash[sinkKey] != null)
                                    {
                                        if (LmpCheckedcomp)
                                        {
                                            if (double.IsNaN(da))
                                            {
                                                da = DARTNode.sDALmpHash[sourceKey].Price;
                                                if (sinkKey != null)
                                                {
                                                    double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                                                    da = sinkDa - DARTNode.sDALmpHash[sourceKey].Price;
                                                }
                                            }
                                        }
                                        else if (CongCheckedcomp)
                                        {
                                            if (double.IsNaN(da))
                                            {
                                                da = DARTNode.sDALmpHash[sourceKey].Congestion;
                                                daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                                if (sinkKey != null)
                                                {
                                                    double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                                                    da = sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion;
                                                    daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                                                }
                                            }
                                        }
                                        else if (LossCheckedcomp)
                                        {
                                            if (double.IsNaN(da))
                                            {
                                                da = DARTNode.sDALmpHash[sourceKey].Loss;
                                                daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                                if (sinkKey != null)
                                                {
                                                    double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                                                    da = sinkDa - DARTNode.sDALmpHash[sourceKey].Loss;
                                                    daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        da = 0;
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sDALmpHash[sourceKey] != null)
                                {
                                    if (LmpCheckedcomp)
                                    {
                                        if (double.IsNaN(da))
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Price;
                                        }
                                    }
                                    else if (CongCheckedcomp)
                                    {
                                        if (double.IsNaN(da))
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Congestion;
                                            daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                        }
                                    }
                                    else if (LossCheckedcomp)
                                    {
                                        if (double.IsNaN(da))
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Loss;
                                            daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                        }
                                    }
                                }
                                else
                                {
                                    da = 0;
                                }

                            }
                        }
                        #endregion RT
                        #region DA
                        if (DaCheckedcomp)
                        {
                            if (sinkKey != null)
                            {
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                                    {
                                        if (LmpCheckedcomp)
                                        {
                                            if (double.IsNaN(rt))
                                            {
                                                rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                                if (sinkKey != null)
                                                {
                                                    double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                                                    rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                                                }
                                            }
                                        }
                                        else if (CongCheckedcomp)
                                        {
                                            if (double.IsNaN(rt))
                                            {
                                                rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                                if (sinkKey != null)
                                                {
                                                    double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                                                    rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion;
                                                }
                                            }
                                        }
                                        else if (LossCheckedcomp)
                                        {
                                            if (double.IsNaN(rt))
                                            {
                                                rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                                                if (sinkKey != null)
                                                {
                                                    double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                                                    rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //   rt = 0;
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                                {
                                    if (LmpCheckedcomp)
                                    {
                                        if (double.IsNaN(rt))
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                        }
                                    }
                                    else if (CongCheckedcomp)
                                    {
                                        if (double.IsNaN(rt))
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                                        }
                                    }
                                    else if (LossCheckedcomp)
                                    {
                                        if (double.IsNaN(rt))
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                                        }
                                    }
                                }
                                else
                                {
                                    //   rt = 0;
                                }
                            }
                            if (sinkKey != null)
                            {
                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sDALmpHash[sourceKey] != null && DARTNode.sDALmpHash[sinkKey] != null)
                                    {
                                        if (LmpCheckedcomp)
                                        {
                                            if (double.IsNaN(da))
                                            {
                                                da = DARTNode.sDALmpHash[sourceKey].Price;
                                                if (sinkKey != null)
                                                {
                                                    double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                                                    da = sinkDa - DARTNode.sDALmpHash[sourceKey].Price;
                                                }
                                            }
                                        }
                                        else if (CongCheckedcomp)
                                        {
                                            if (double.IsNaN(da))
                                            {
                                                da = DARTNode.sDALmpHash[sourceKey].Congestion;
                                                daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                                if (sinkKey != null)
                                                {
                                                    double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                                                    da = sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion;
                                                    daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;
                                                }
                                            }
                                        }
                                        else if (LossCheckedcomp)
                                        {
                                            if (double.IsNaN(da))
                                            {
                                                da = DARTNode.sDALmpHash[sourceKey].Loss;
                                                daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                                if (sinkKey != null)
                                                {
                                                    double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                                                    da = sinkDa - DARTNode.sDALmpHash[sourceKey].Loss;
                                                    daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // da = 0;
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sDALmpHash[sourceKey] != null)
                                {
                                    if (LmpCheckedcomp)
                                    {
                                        if (double.IsNaN(da))
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Price;
                                        }
                                    }
                                    else if (CongCheckedcomp)
                                    {
                                        if (double.IsNaN(da))
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Congestion;
                                            daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                        }
                                    }
                                    else if (LossCheckedcomp)
                                    {
                                        if (double.IsNaN(da))
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Loss;
                                            daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                                        }
                                    }
                                }
                                else
                                {
                                    // da = 0;
                                }
                            }
                        }
                        #endregion DA

                        #region OLD
                        //if (sinkKey != null)
                        //{
                        //    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey) &&
                        //        !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sDAHash[sinkKey]))
                        //    {
                        //        da = (DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey]);
                        //    }
                        //}
                        //else
                        //{
                        //    if (DARTNode.sDAHash.ContainsKey(sourceKey) &&
                        //        !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                        //    {
                        //        da = DARTNode.sDAHash[sourceKey];
                        //    }
                        //}
                        //if (sinkKey != null)
                        //{
                        //    if (DARTNode.sRTHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                        //        !double.IsNaN(DARTNode.sRTHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                        //    {
                        //        rt = (DARTNode.sRTHash[sinkKey] - DARTNode.sRTHash[sourceKey]);
                        //    }
                        //}
                        //else
                        //{
                        //    if (DARTNode.sRTHash.ContainsKey(sourceKey) &&
                        //        !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                        //    {
                        //        rt = DARTNode.sRTHash[sourceKey];
                        //    }
                        //}
                        #endregion OLD
                        if (!double.IsNaN(da) && !double.IsNaN(rt))
                        {
                            dart = rt - da;
                        }
                        string sourceSink = path.Sink == null || path.Sink.Length == 0 ? path.Source : path.Source + "->" + path.Sink;
                        PNL mustTakePnl = new PNL();
                        mustTakePnl.SourceSink = sourceSink;
                        if (!double.IsNaN(da))
                        {
                            mustTakePnl.DA = da;
                        }
                        if (!double.IsNaN(rt))
                        {
                            mustTakePnl.RT = rt;
                        }
                        if (!double.IsNaN(dart))
                        {
                            mustTakePnl.DART = dart - Fee;
                        }
                        Dictionary<string, Tuple<Path, PNL>> mustTakePathHash = new Dictionary<string, Tuple<Path, PNL>>();
                        if (mMustTakeMarketDateTimeHash.ContainsKey(sendDate))
                        {
                            mustTakePathHash = mMustTakeMarketDateTimeHash[sendDate];
                        }
                        else
                        {
                            mMustTakeMarketDateTimeHash.Add(sendDate, mustTakePathHash);
                        }
                        Tuple<Path, PNL> mustTakeTuple = new Tuple<Path, PNL>(path, mustTakePnl);
                        if (MarketComboSelectedValue != "ERCOT")
                        {
                            if (mustTakePathHash.ContainsKey(path.BidId))
                            {
                                MessageBox.Show(path.BidId + " Bid ID is not unique");
                                return;
                            }
                            mustTakePathHash.Add(path.BidId, mustTakeTuple);
                        }
                        else
                        {
                            if (!mustTakePathHash.ContainsKey(path.BidId))
                                mustTakePathHash.Add(path.BidId, mustTakeTuple);
                            else
                            {
                                Tuple<Path, PNL> tempTuple = mustTakePathHash[path.BidId];
                                tempTuple.Item2.DA += mustTakeTuple.Item2.DA;
                                tempTuple.Item2.RT += mustTakeTuple.Item2.RT;
                                tempTuple.Item2.DART += mustTakeTuple.Item2.DART;
                            }
                        }
                        // if congestion and loss components are checked it should always check lmp prices for asbid CLearing 
                        if (LmpCheckedcomp)
                        {
                            daLmp = da;
                        }
                        if ((daLmp <= path.Price && path.MW > 0) || (daLmp >= path.Price && path.MW < 0))
                        {
                            PNL asBidPnl = new PNL();
                            asBidPnl.SourceSink = sourceSink;
                            if (!double.IsNaN(da))
                            {
                                asBidPnl.DA = da;
                            }
                            if (!double.IsNaN(rt))
                            {
                                asBidPnl.RT = rt;
                            }
                            if (!double.IsNaN(dart))
                            {
                                asBidPnl.DART = dart - Fee;
                            }
                            Dictionary<string, Tuple<Path, PNL>> asBidPathHash = new Dictionary<string, Tuple<Path, PNL>>();
                            if (mAsBidMarketDateTimeHash.ContainsKey(sendDate))
                            {
                                asBidPathHash = mAsBidMarketDateTimeHash[sendDate];
                            }
                            else
                            {
                                mAsBidMarketDateTimeHash.Add(sendDate, asBidPathHash);
                            }
                            Tuple<Path, PNL> asBidTuple = new Tuple<Path, PNL>(path, asBidPnl);
                            if (!asBidPathHash.ContainsKey(path.BidId))
                            {
                                asBidPathHash.Add(path.BidId, asBidTuple);
                            }
                            else
                            {
                                Tuple<Path, PNL> tempTuple = asBidPathHash[path.BidId];
                                tempTuple.Item2.DA += asBidTuple.Item2.DA;
                                tempTuple.Item2.RT += asBidTuple.Item2.RT;
                                tempTuple.Item2.DART += asBidTuple.Item2.DART;
                            }
                        }
                    }
                }
            }

            // stopwatch.Stop();

            // Log the elapsed time using Trace
            // Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

        }


        public void UpdateChartCommand(bool isCalculatable)
        {
            //   Stopwatch stopwatch = Stopwatch.StartNew();
            PathDayPlotModelUpper = null;
            List<Node> asBidDaSendFilteredSortedList = new List<Node>();
            List<Node> asBidRtSendFilteredSortedList = new List<Node>();
            List<Node> asBidDartSendFilteredSortedList = new List<Node>();
            List<Node> mustTakeDaSendFilteredSortedList = new List<Node>();
            List<Node> mustTakeRtSendFilteredSortedList = new List<Node>();
            List<Node> mustTakeDartSendFilteredSortedList = new List<Node>();

            //  Stopwatch stopwatch1 = Stopwatch.StartNew();

            List<Node> nodeList = GetNodeList(out asBidDaSendFilteredSortedList, out asBidRtSendFilteredSortedList, out asBidDartSendFilteredSortedList, out mustTakeDaSendFilteredSortedList,
                                            out mustTakeRtSendFilteredSortedList, out mustTakeDartSendFilteredSortedList, null);

            //   Trace.WriteLine($"SomeMethod execution time: {stopwatch1.ElapsedMilliseconds} ms");


            if (nodeList == null)
            {
                return;
            }
            PlotModelUpper = CreatePlotModelUpper(nodeList);
            PlotModelLower = CreatePlotModelLower(nodeList);
            List<Node> daSendFilteredSortedList;
            List<Node> rtSendFilteredSortedList;
            List<Node> dartSendFilteredSortedList;
            if (AsBidChecked)
            {
                daSendFilteredSortedList = asBidDaSendFilteredSortedList;
                rtSendFilteredSortedList = asBidRtSendFilteredSortedList;
                dartSendFilteredSortedList = asBidDartSendFilteredSortedList;
            }
            else
            {
                daSendFilteredSortedList = mustTakeDaSendFilteredSortedList;
                rtSendFilteredSortedList = mustTakeRtSendFilteredSortedList;
                dartSendFilteredSortedList = mustTakeDartSendFilteredSortedList;
            }
            if (isCalculatable)
            {
                RefreshDayComparisonGridCommand(daSendFilteredSortedList, rtSendFilteredSortedList, dartSendFilteredSortedList);
            }
            SetPathRisk();

            // stopwatch.Stop();

            // Log the elapsed time using Trace
            //  Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

        }

        public void RefreshFilterData(List<Node> nodeList, out List<Node> daFilteredSortedList, out List<Node> rtFilteredSortedList, out List<Node> dartFilteredSortedList)
        {

            //   Stopwatch stopwatch = Stopwatch.StartNew();
            daFilteredSortedList = new List<Node>();
            rtFilteredSortedList = new List<Node>();
            dartFilteredSortedList = new List<Node>();
            List<Node> tempList = new List<Node>();
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
            SetHourlyPivotList(PathList);
        }


        public bool IsValid(DateTime marketDateTime)
        {
            if (mHourList.Contains(marketDateTime))
            {
                return false;
            }
            if ((!mHE1Checked && marketDateTime.Hour == 1) || (!mHE24Checked && marketDateTime.Hour == 0) ||
                (!mHE2Checked && marketDateTime.Hour == 2) || (!mHE3Checked && marketDateTime.Hour == 3) ||
                (!mHE4Checked && marketDateTime.Hour == 4) || (!mHE5Checked && marketDateTime.Hour == 5) ||
                (!mHE6Checked && marketDateTime.Hour == 6) || (!mHE7Checked && marketDateTime.Hour == 7) ||
                (!mHE8Checked && marketDateTime.Hour == 8) || (!mHE9Checked && marketDateTime.Hour == 9) ||
                (!mHE10Checked && marketDateTime.Hour == 10) || (!mHE11Checked && marketDateTime.Hour == 11) ||
                (!mHE12Checked && marketDateTime.Hour == 12) || (!mHE13Checked && marketDateTime.Hour == 13) ||
                (!mHE14Checked && marketDateTime.Hour == 14) || (!mHE15Checked && marketDateTime.Hour == 15) ||
                (!mHE16Checked && marketDateTime.Hour == 16) || (!mHE17Checked && marketDateTime.Hour == 17) ||
                (!mHE18Checked && marketDateTime.Hour == 18) || (!mHE19Checked && marketDateTime.Hour == 19) ||
                (!mHE20Checked && marketDateTime.Hour == 20) || (!mHE21Checked && marketDateTime.Hour == 21) ||
                (!mHE22Checked && marketDateTime.Hour == 22) || (!mHE23Checked && marketDateTime.Hour == 23))
            {
                return false;
            }
            if ((!mMondayChecked && marketDateTime.DayOfWeek == DayOfWeek.Monday) || (!mTuesdayChecked && marketDateTime.DayOfWeek == DayOfWeek.Tuesday) ||
                (!mWednesdayChecked && marketDateTime.DayOfWeek == DayOfWeek.Wednesday) || (!mThursdayChecked && marketDateTime.DayOfWeek == DayOfWeek.Thursday) ||
               (!mFridayChecked && marketDateTime.DayOfWeek == DayOfWeek.Friday) || (!mSaturdayChecked && marketDateTime.DayOfWeek == DayOfWeek.Saturday) ||
                (!mSundayChecked && marketDateTime.DayOfWeek == DayOfWeek.Sunday))
            {
                return false;
            }
            if ((!mJanChecked && marketDateTime.Month == 1) || (!mFebChecked && marketDateTime.Month == 2) ||
                (!mMarChecked && marketDateTime.Month == 3) || (!mAprChecked && marketDateTime.Month == 4) ||
                (!mMayChecked && marketDateTime.Month == 5) || (!mJunChecked && marketDateTime.Month == 6) ||
                (!mJulChecked && marketDateTime.Month == 7) || (!mAugChecked && marketDateTime.Month == 8) ||
                (!mSepChecked && marketDateTime.Month == 9) || (!mOctChecked && marketDateTime.Month == 10) ||
                (!mNovChecked && marketDateTime.Month == 11) || (!mDecChecked && marketDateTime.Month == 12))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Adds the filters.
        /// </summary>
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

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            try
            {
                SavePreferences();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the main application.
        /// </summary>
        /// <param name="baseApp">The base application.</param>
        public void SetMainApp(IMainApp baseApp)
        {
            mainApp = baseApp;
        }
        /// <summary>
        /// Goes the command.
        /// </summary>
        public void GoCommand()
        {
            try
            {
                bool isDA = false;
                if (RTExpChecked)
                    isDA = false;
                else
                    isDA = true;
                if (mRiskConstraintsChecked || NewConstraintChecked)
                {
                    DateTime startDate = RiskConstraintDateSelected;
                    ClearedDateEnable = false;
                    Mouse.OverrideCursor = Cursors.Wait;
                    CheckExposure(true, SortDollarChecked, false, startDate);
                    Mouse.OverrideCursor = null;
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    mPathDetailHash = new Dictionary<int, List<Exposure>>();
                    ExposureList = null;
                    if (PathList == null)
                    {
                        return;
                    }
                    if (AsBidChecked)
                    {
                        Dictionary<int, List<DateTime>> nodeHash = new Dictionary<int, List<DateTime>>();
                        foreach (Path path in PathList)
                        {
                            if (path.Sink == "")
                            {
                                path.Sink = null;
                            }
                            if (!path.Submit)
                            {
                                continue;
                            }
                            int source = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                            int sink = path.Sink == null ? 0 : DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;
                            string[] hours = path.AnalysisType.Split('.');
                            List<DateTime> marketDateList = new List<DateTime>();
                            if (nodeHash.ContainsKey(source))
                            {
                                marketDateList = nodeHash[source];
                                nodeHash.Remove(source);
                            }
                            foreach (string hour in hours)
                            {
                                DateTime marketDateTime = AsBidClearedDate.AddHours(Int32.Parse(hour));
                                if (!marketDateList.Contains(marketDateTime))
                                {
                                    marketDateList.Add(marketDateTime);
                                }
                            }
                            nodeHash.Add(source, marketDateList);
                            if (sink != 0)
                            {
                                if (nodeHash.ContainsKey(sink))
                                {
                                    marketDateList = nodeHash[sink];
                                    nodeHash.Remove(sink);
                                }
                                foreach (string hour in hours)
                                {
                                    DateTime marketDateTime = AsBidClearedDate.AddHours(Int32.Parse(hour));
                                    if (!marketDateList.Contains(marketDateTime))
                                    {
                                        marketDateList.Add(marketDateTime);
                                    }
                                }
                                nodeHash.Add(sink, marketDateList);
                            }
                            if (MarketComboSelectedValue == "PJM")
                            {
                                DARTNode.GetDartMarket(nodeHash, "da", 1);
                            }
                            else
                            {
                                DARTNode.GetDartMarket(nodeHash, "da", 9);

                            }
                        }
                    }
                    DateTime tempStartDate = StartDate;
                    DateTime tempEndDate = EndDate;
                    //    if (SortDollarChecked)
                    {
                        StartDate = ClearingDate;
                        EndDate = ClearingDate;
                    }
                    if (EndClearedDate == ClearingDate)
                    {
                        EndClearedDate = ClearingDate.AddDays(1);
                    }
                    Dictionary<int, Dictionary<int, Sensitivity>> nodeSensitivityHash = _dataService.GetSensitivity(ClearingDate, EndClearedDate, SortDollarChecked, MarketComboSelectedValue, isDA);
                    Dictionary<DateTime, Dictionary<int, double>> hourlyImpactHash = _dataService.GetHourlyImpact(ClearingDate, EndClearedDate, MarketComboSelectedValue, isDA);
                    Dictionary<int, Exposure> exposureHash = new Dictionary<int, Exposure>();
                    foreach (Path path in PathList)
                    {
                        if (!path.Submit)
                        {
                            continue;
                        }
                        int sinkNode = 0;
                        int sourceNode = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                        if (path.Sink == "")
                        {
                            path.Sink = null;
                        }
                        sinkNode = path.Sink == null ? 0 : DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;


                        if (nodeSensitivityHash.ContainsKey(sourceNode) && (sinkNode == 0 || nodeSensitivityHash.ContainsKey(sinkNode)))
                        {
                            Dictionary<int, Sensitivity> sourceSensitivityHash = nodeSensitivityHash[sourceNode];
                            Dictionary<int, Sensitivity> sinkSensitivityHash = sinkNode == 0 ? null : nodeSensitivityHash[sinkNode];
                            List<int> sourceSensitivityKeys = sourceSensitivityHash.Keys.ToList<int>();


                            IEnumerable<int> sourcenotfound = sinkSensitivityHash.Keys.ToList<int>().Except(sourceSensitivityKeys);

                            foreach (int value in sourcenotfound)
                            {
                                Sensitivity ValueSensitivity = new Sensitivity();
                                ValueSensitivity = sinkSensitivityHash[value];

                                Sensitivity NewSensitivity = new Sensitivity();
                                NewSensitivity.ID = ValueSensitivity.ID;
                                NewSensitivity.Constraint = ValueSensitivity.Constraint;
                                NewSensitivity.Contingency = ValueSensitivity.Contingency;
                                NewSensitivity.DollarImpact = ValueSensitivity.DollarImpact;
                                NewSensitivity.RiskType = ValueSensitivity.RiskType;
                                NewSensitivity.ShiftFactor = ValueSensitivity.ShiftFactor;
                                NewSensitivity.SensitivityValue = 0;



                                if (!sourceSensitivityHash.ContainsKey(value))
                                {
                                    sourceSensitivityHash.Add(value, NewSensitivity);
                                    sourceSensitivityHash[value].SensitivityValue = 0;
                                }

                            }


                            sourceSensitivityKeys = sourceSensitivityHash.Keys.ToList<int>();

                            foreach (int sourceSensitivityKey in sourceSensitivityKeys)
                            {
                                try
                                {
                                    {
                                        Sensitivity sinkSensitivity;
                                        double sensityvity = 0;
                                        Sensitivity sourceSensitivity = sourceSensitivityHash[sourceSensitivityKey];
                                        if (sinkSensitivityHash.ContainsKey(sourceSensitivityKey))
                                        {
                                            sinkSensitivity = sinkNode == 0 ? null : sinkSensitivityHash[sourceSensitivityKey];
                                            sensityvity = sinkSensitivity.SensitivityValue;
                                        }
                                        double diff = sinkNode == 0 ? sourceSensitivity.SensitivityValue : sensityvity - sourceSensitivity.SensitivityValue;
                                        if (SortDollarChecked)
                                        {
                                            diff *= sourceSensitivity.DollarImpact;
                                        }
                                        Exposure exposure = new Exposure();
                                        exposure.Constraint = sourceSensitivity.Constraint;
                                        exposure.Contingency = sourceSensitivity.Contingency;
                                        exposure.ID = sourceSensitivity.ID;
                                        if (exposure.ID == 3185)
                                        {

                                        }
                                        exposure.Shift = sourceSensitivity.ShiftFactor;
                                        List<Exposure> pathDetailList = new List<Exposure>();
                                        if (mPathDetailHash.ContainsKey(exposure.ID))
                                        {
                                            pathDetailList = mPathDetailHash[exposure.ID];
                                            mPathDetailHash.Remove(exposure.ID);
                                        }
                                        Exposure pathExposure = null;
                                        if (exposureHash.ContainsKey(sourceSensitivity.ID))
                                        {
                                            exposure = exposureHash[sourceSensitivity.ID];
                                            exposureHash.Remove(sourceSensitivity.ID);
                                        }
                                        string[] tokens = path.AnalysisType.Split('.');
                                        double total = 0;
                                        double pathTotal = 0;
                                        foreach (string token in tokens)
                                        {
                                            int hourValue = Int32.Parse(token);
                                            DateTime tempDate = hourValue == 24 ? AsBidClearedDate.Date.AddDays(1) :
                                                                                       AsBidClearedDate.Date.AddHours(hourValue);
                                            Dictionary<int, double> constraintImpacthash = new Dictionary<int, double>();
                                            if (hourlyImpactHash.ContainsKey(tempDate))
                                            {
                                                constraintImpacthash = hourlyImpactHash[tempDate];
                                            }
                                            if (AsBidChecked)
                                            {

                                                DateTime sendDate = hourValue == 24 ? AsBidClearedDate.Date.AddDays(1) :
                                                                                        AsBidClearedDate.Date.AddHours(hourValue);

                                                string sourceKey = sendDate.ToString() + sourceNode;
                                                string sinkKey = sinkNode == 0 ? null : sendDate.ToString() + sinkNode;
                                                double da = double.NaN;
                                                if (sinkKey != null)
                                                {
                                                    if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey)
                                                        && !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                                                    {
                                                        da = (DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey]);
                                                    }
                                                }
                                                else
                                                {
                                                    if (DARTNode.sDAHash.ContainsKey(sourceKey)
                                                        && !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                                                    {
                                                        da = DARTNode.sDAHash[sourceKey];
                                                    }
                                                }
                                                if ((path.Price < da && path.MW > 0) || (path.MW < 0 && path.Price > da))
                                                {
                                                    continue;
                                                }
                                            }
                                            if (pathExposure == null)
                                            {
                                                pathExposure = new Exposure();
                                                pathExposure.ID = exposure.ID;
                                                pathExposure.Constraint = path.Source;
                                                pathExposure.Contingency = path.Sink;
                                            }
                                            if (token == "1")
                                            {
                                                if (exposure.HE1 == null)
                                                {
                                                    exposure.HE1 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE1 += path.MW * diff;
                                                        pathExposure.HE1 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE1 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE1 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE1)) || double.IsInfinity(Convert.ToDouble(exposure.HE1)))
                                                        {
                                                            exposure.HE1 = 00;
                                                            pathExposure.HE1 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE1 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE1 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE1 += path.MW * diff;
                                                        pathExposure.HE1 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE1;
                                                pathTotal += (double)pathExposure.HE1;
                                            }
                                            if (token == "2")
                                            {
                                                if (exposure.HE2 == null)
                                                {
                                                    exposure.HE2 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE2 += path.MW * diff;
                                                        pathExposure.HE2 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE2 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE2 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE2)) || double.IsInfinity(Convert.ToDouble(exposure.HE2)))
                                                        {
                                                            exposure.HE2 = 00;
                                                            pathExposure.HE2 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE2 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE2 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE2 += path.MW * diff;
                                                        pathExposure.HE2 = path.MW * diff;
                                                    }

                                                }
                                                total += (double)exposure.HE2;
                                                pathTotal += (double)pathExposure.HE2;
                                            }
                                            if (token == "3")
                                            {
                                                if (exposure.HE3 == null)
                                                {
                                                    exposure.HE3 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE3 += path.MW * diff;
                                                        pathExposure.HE3 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE3 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE3 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE3)) || double.IsInfinity(Convert.ToDouble(exposure.HE3)))
                                                        {
                                                            exposure.HE3 = 00;
                                                            pathExposure.HE3 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE3 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE3 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE3 += path.MW * diff;
                                                        pathExposure.HE3 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE3;
                                                pathTotal += (double)pathExposure.HE3;
                                            }
                                            if (token == "4")
                                            {
                                                if (exposure.HE4 == null)
                                                {
                                                    exposure.HE4 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE4 += path.MW * diff;
                                                        pathExposure.HE4 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE4 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE4 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE4)) || double.IsInfinity(Convert.ToDouble(exposure.HE4)))
                                                        {
                                                            exposure.HE4 = 00;
                                                            pathExposure.HE4 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE4 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE4 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE4 += path.MW * diff;
                                                        pathExposure.HE4 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE4;
                                                pathTotal += (double)pathExposure.HE4;
                                            }
                                            if (token == "5")
                                            {
                                                if (exposure.HE5 == null)
                                                {
                                                    exposure.HE5 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE5 += path.MW * diff;
                                                        pathExposure.HE5 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE5 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE5 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE5)) || double.IsInfinity(Convert.ToDouble(exposure.HE5)))
                                                        {
                                                            exposure.HE5 = 00;
                                                            pathExposure.HE5 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE5 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE5 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE5 += path.MW * diff;
                                                        pathExposure.HE5 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE5;
                                                pathTotal += (double)pathExposure.HE5;
                                            }
                                            if (token == "6")
                                            {
                                                if (exposure.HE6 == null)
                                                {
                                                    exposure.HE6 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE6 += path.MW * diff;
                                                        pathExposure.HE6 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE6 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE6 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE6)) || double.IsInfinity(Convert.ToDouble(exposure.HE6)))
                                                        {
                                                            exposure.HE6 = 00;
                                                            pathExposure.HE6 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE6 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE6 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE6 += path.MW * diff;
                                                        pathExposure.HE6 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE6;
                                                pathTotal += (double)pathExposure.HE6;
                                            }
                                            if (token == "7")
                                            {
                                                if (exposure.HE7 == null)
                                                {
                                                    exposure.HE7 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE7 += path.MW * diff;
                                                        pathExposure.HE7 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE7 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE7 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE7)) || double.IsInfinity(Convert.ToDouble(exposure.HE7)))
                                                        {
                                                            exposure.HE7 = 00;
                                                            pathExposure.HE7 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE7 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE7 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE7 += path.MW * diff;
                                                        pathExposure.HE7 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE7;
                                                pathTotal += (double)pathExposure.HE7;
                                            }
                                            if (token == "8")
                                            {
                                                if (exposure.HE8 == null)
                                                {
                                                    exposure.HE8 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE8 += path.MW * diff;
                                                        pathExposure.HE8 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE8 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE8 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE8)) || double.IsInfinity(Convert.ToDouble(exposure.HE8)))
                                                        {
                                                            exposure.HE8 = 00;
                                                            pathExposure.HE8 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE8 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE8 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE8 += path.MW * diff;
                                                        pathExposure.HE8 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE8;
                                                pathTotal += (double)pathExposure.HE8;
                                            }
                                            if (token == "9")
                                            {
                                                if (exposure.HE9 == null)
                                                {
                                                    exposure.HE9 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE9 += path.MW * diff;
                                                        pathExposure.HE9 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE9 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE9 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE9)) || double.IsInfinity(Convert.ToDouble(exposure.HE9)))
                                                        {
                                                            exposure.HE9 = 00;
                                                            pathExposure.HE9 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE9 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE9 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE9 += path.MW * diff;
                                                        pathExposure.HE9 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE9;
                                                pathTotal += (double)pathExposure.HE9;
                                            }
                                            if (token == "10")
                                            {
                                                if (exposure.HE10 == null)
                                                {
                                                    exposure.HE10 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE10 += path.MW * diff;
                                                        pathExposure.HE10 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE10 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE10 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE10)) || double.IsInfinity(Convert.ToDouble(exposure.HE10)))
                                                        {
                                                            exposure.HE10 = 00;
                                                            pathExposure.HE10 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE10 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE10 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE10 += path.MW * diff;
                                                        pathExposure.HE10 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE10;
                                                pathTotal += (double)pathExposure.HE10;
                                            }
                                            if (token == "11")
                                            {
                                                if (exposure.HE11 == null)
                                                {
                                                    exposure.HE11 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE11 += path.MW * diff;
                                                        pathExposure.HE11 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE11 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE11 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE11)) || double.IsInfinity(Convert.ToDouble(exposure.HE11)))
                                                        {
                                                            exposure.HE11 = 00;
                                                            pathExposure.HE11 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE11 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE11 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE11 += path.MW * diff;
                                                        pathExposure.HE11 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE11;
                                                pathTotal += (double)pathExposure.HE11;
                                            }
                                            if (token == "12")
                                            {
                                                if (exposure.HE12 == null)
                                                {
                                                    exposure.HE12 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE12 += path.MW * diff;
                                                        pathExposure.HE12 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE12 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE12 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE12)) || double.IsInfinity(Convert.ToDouble(exposure.HE12)))
                                                        {
                                                            exposure.HE12 = 00;
                                                            pathExposure.HE12 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE12 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE12 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE12 += path.MW * diff;
                                                        pathExposure.HE12 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE12;
                                                pathTotal += (double)pathExposure.HE12;
                                            }
                                            if (token == "13")
                                            {
                                                if (exposure.HE13 == null)
                                                {
                                                    exposure.HE13 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE13 += path.MW * diff;
                                                        pathExposure.HE13 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE13 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE13 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE13)) || double.IsInfinity(Convert.ToDouble(exposure.HE13)))
                                                        {
                                                            exposure.HE13 = 00;
                                                            pathExposure.HE13 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE13 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE13 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE13 += path.MW * diff;
                                                        pathExposure.HE13 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE13;
                                                pathTotal += (double)pathExposure.HE13;
                                            }
                                            if (token == "14")
                                            {
                                                if (exposure.HE14 == null)
                                                {
                                                    exposure.HE14 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE14 += path.MW * diff;
                                                        pathExposure.HE14 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE14 += (double)path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE14 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE14)) || double.IsInfinity(Convert.ToDouble(exposure.HE14)))
                                                        {
                                                            exposure.HE14 = 00;
                                                            pathExposure.HE14 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE14 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE14 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE14 += path.MW * diff;
                                                        pathExposure.HE14 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE14;
                                                pathTotal += (double)pathExposure.HE14;
                                            }
                                            if (token == "15")
                                            {
                                                if (exposure.HE15 == null)
                                                {
                                                    exposure.HE15 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE15 += path.MW * diff;
                                                        pathExposure.HE15 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE15 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE15 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE15)) || double.IsInfinity(Convert.ToDouble(exposure.HE15)))
                                                        {
                                                            exposure.HE15 = 00;
                                                            pathExposure.HE15 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE15 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE15 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE15 += path.MW * diff;
                                                        pathExposure.HE15 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE15;
                                                pathTotal += (double)pathExposure.HE15;
                                            }
                                            if (token == "16")
                                            {
                                                if (exposure.HE16 == null)
                                                {
                                                    exposure.HE16 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE16 += path.MW * diff;
                                                        pathExposure.HE16 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE16 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE16 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE16)) || double.IsInfinity(Convert.ToDouble(exposure.HE16)))
                                                        {
                                                            exposure.HE16 = 00;
                                                            pathExposure.HE16 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE16 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE16 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE16 += path.MW * diff;
                                                        pathExposure.HE16 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE16;
                                                pathTotal += (double)pathExposure.HE16;
                                            }
                                            if (token == "17")
                                            {
                                                if (exposure.HE17 == null)
                                                {
                                                    exposure.HE17 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE17 += path.MW * diff;
                                                        pathExposure.HE17 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE17 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE17 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE17)) || double.IsInfinity(Convert.ToDouble(exposure.HE17)))
                                                        {
                                                            exposure.HE17 = 00;
                                                            pathExposure.HE17 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE17 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE17 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE17 += path.MW * diff;
                                                        pathExposure.HE17 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE17;
                                                pathTotal += (double)pathExposure.HE17;
                                            }
                                            if (token == "18")
                                            {
                                                if (exposure.HE18 == null)
                                                {
                                                    exposure.HE18 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE18 += path.MW * diff;
                                                        pathExposure.HE18 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE18 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE18 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE18)) || double.IsInfinity(Convert.ToDouble(exposure.HE18)))
                                                        {
                                                            exposure.HE18 = 00;
                                                            pathExposure.HE18 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE18 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE18 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE18 += path.MW * diff;
                                                        pathExposure.HE18 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE18;
                                                pathTotal += (double)pathExposure.HE18;
                                            }
                                            if (token == "19")
                                            {
                                                if (exposure.HE19 == null)
                                                {
                                                    exposure.HE19 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE19 += path.MW * diff;
                                                        pathExposure.HE19 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE19 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE19 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE19)) || double.IsInfinity(Convert.ToDouble(exposure.HE19)))
                                                        {
                                                            exposure.HE19 = 00;
                                                            pathExposure.HE19 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE19 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE19 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE19 += path.MW * diff;
                                                        pathExposure.HE19 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE19;
                                                pathTotal += (double)pathExposure.HE19;
                                            }
                                            if (token == "20")
                                            {
                                                if (exposure.HE20 == null)
                                                {
                                                    exposure.HE20 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE20 += path.MW * diff;
                                                        pathExposure.HE20 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE20 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE20 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE20)) || double.IsInfinity(Convert.ToDouble(exposure.HE20)))
                                                        {
                                                            exposure.HE20 = 00;
                                                            pathExposure.HE20 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE20 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE20 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE20 += path.MW * diff;
                                                        pathExposure.HE20 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE20;
                                                pathTotal += (double)pathExposure.HE20;
                                            }
                                            if (token == "21")
                                            {
                                                if (exposure.HE21 == null)
                                                {
                                                    exposure.HE21 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE21 += path.MW * diff;
                                                        pathExposure.HE21 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE21 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE21 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE21)) || double.IsInfinity(Convert.ToDouble(exposure.HE21)))
                                                        {
                                                            exposure.HE21 = 00;
                                                            pathExposure.HE21 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE21 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE21 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE21 += path.MW * diff;
                                                        pathExposure.HE21 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE21;
                                                pathTotal += (double)pathExposure.HE21;
                                            }
                                            if (token == "22")
                                            {
                                                if (exposure.HE22 == null)
                                                {
                                                    exposure.HE22 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE22 += path.MW * diff;
                                                        pathExposure.HE22 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE22 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE22 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE22)) || double.IsInfinity(Convert.ToDouble(exposure.HE22)))
                                                        {
                                                            exposure.HE22 = 00;
                                                            pathExposure.HE22 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE22 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE22 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE22 += path.MW * diff;
                                                        pathExposure.HE22 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE22;
                                                pathTotal += (double)pathExposure.HE22;
                                            }
                                            if (token == "23")
                                            {
                                                if (exposure.HE23 == null)
                                                {
                                                    exposure.HE23 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE23 += path.MW * diff;
                                                        pathExposure.HE23 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE23 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE23 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE23)) || double.IsInfinity(Convert.ToDouble(exposure.HE23)))
                                                        {
                                                            exposure.HE23 = 00;
                                                            pathExposure.HE23 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE23 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE23 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE23 += path.MW * diff;
                                                        pathExposure.HE23 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE23;
                                                pathTotal += (double)pathExposure.HE23;
                                            }
                                            if (token == "24")
                                            {
                                                if (exposure.HE24 == null)
                                                {
                                                    exposure.HE24 = 0;
                                                }

                                                if (SortDollarChecked)
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE24 += path.MW * diff;
                                                        pathExposure.HE24 = path.MW * diff;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE24 += path.MW * (diff / exposure.Shift);
                                                        pathExposure.HE24 = path.MW * (diff / exposure.Shift);
                                                        if (double.IsNaN(Convert.ToDouble(exposure.HE24)) || double.IsInfinity(Convert.ToDouble(exposure.HE24)))
                                                        {
                                                            exposure.HE24 = 00;
                                                            pathExposure.HE24 = 00;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MarketComboSelectedValue != "ERCOT")
                                                    {
                                                        exposure.HE24 += path.MW * diff * exposure.Shift;
                                                        pathExposure.HE24 = path.MW * diff * exposure.Shift;
                                                    }
                                                    else
                                                    {
                                                        exposure.HE24 += path.MW * diff;
                                                        pathExposure.HE24 = path.MW * diff;
                                                    }
                                                }
                                                total += (double)exposure.HE24;
                                                pathTotal += (double)pathExposure.HE24;
                                            }
                                        }
                                        //exposure.Sum = total;
                                        if (total != 0)
                                        {
                                            pathExposure.Sum = (double)pathTotal;


                                        }
                                        exposureHash.Add(sourceSensitivity.ID, exposure);
                                        if (pathExposure != null)
                                        {
                                            pathDetailList.Add(pathExposure);
                                        }
                                        mPathDetailHash.Add(exposure.ID, pathDetailList);
                                    }


                                }
                                catch (Exception ex)
                                {

                                    //throw;
                                }
                                //if (sinkNode == 0 || sinkSensitivityHash.ContainsKey(sourceSensitivityKey) )
                            }
                        }
                    }
                    foreach (var item in mPathDetailHash)
                    {
                        int constraintId = item.Key;
                        List<Exposure> pathDetailList = item.Value;
                        Exposure hourlyPathTot = new Exposure();
                        hourlyPathTot.HE1 = pathDetailList.Sum(a => a.HE1);
                        hourlyPathTot.HE2 = pathDetailList.Sum(a => a.HE2);
                        hourlyPathTot.HE3 = pathDetailList.Sum(a => a.HE3);
                        hourlyPathTot.HE4 = pathDetailList.Sum(a => a.HE4);
                        hourlyPathTot.HE5 = pathDetailList.Sum(a => a.HE5);
                        hourlyPathTot.HE6 = pathDetailList.Sum(a => a.HE6);
                        hourlyPathTot.HE7 = pathDetailList.Sum(a => a.HE7);
                        hourlyPathTot.HE8 = pathDetailList.Sum(a => a.HE8);
                        hourlyPathTot.HE9 = pathDetailList.Sum(a => a.HE9);
                        hourlyPathTot.HE10 = pathDetailList.Sum(a => a.HE10);
                        hourlyPathTot.HE11 = pathDetailList.Sum(a => a.HE11);
                        hourlyPathTot.HE12 = pathDetailList.Sum(a => a.HE12);
                        hourlyPathTot.HE13 = pathDetailList.Sum(a => a.HE13);
                        hourlyPathTot.HE14 = pathDetailList.Sum(a => a.HE14);
                        hourlyPathTot.HE15 = pathDetailList.Sum(a => a.HE15);
                        hourlyPathTot.HE16 = pathDetailList.Sum(a => a.HE16);
                        hourlyPathTot.HE17 = pathDetailList.Sum(a => a.HE17);
                        hourlyPathTot.HE18 = pathDetailList.Sum(a => a.HE18);
                        hourlyPathTot.HE19 = pathDetailList.Sum(a => a.HE19);
                        hourlyPathTot.HE20 = pathDetailList.Sum(a => a.HE20);
                        hourlyPathTot.HE21 = pathDetailList.Sum(a => a.HE21);
                        hourlyPathTot.HE22 = pathDetailList.Sum(a => a.HE22);
                        hourlyPathTot.HE23 = pathDetailList.Sum(a => a.HE23);
                        hourlyPathTot.HE24 = pathDetailList.Sum(a => a.HE24);
                        hourlyPathTot.Constraint = "Total";
                        pathDetailList.Add(hourlyPathTot);
                    }
                    List<Exposure> exposureFinalList = exposureHash.Values.ToList<Exposure>();
                    foreach (Exposure item in exposureFinalList)
                    {
                        if (item.HE1 == null)
                        {
                            item.HE1 = 0;
                        }
                        if (item.HE2 == null)
                        {
                            item.HE2 = 0;
                        }
                        if (item.HE3 == null)
                        {
                            item.HE3 = 0;
                        }
                        if (item.HE4 == null)
                        {
                            item.HE4 = 0;
                        }
                        if (item.HE5 == null)
                        {
                            item.HE5 = 0;
                        }
                        if (item.HE6 == null)
                        {
                            item.HE6 = 0;
                        }
                        if (item.HE7 == null)
                        {
                            item.HE7 = 0;
                        }
                        if (item.HE8 == null)
                        {
                            item.HE8 = 0;
                        }
                        if (item.HE9 == null)
                        {
                            item.HE9 = 0;
                        }
                        if (item.HE10 == null)
                        {
                            item.HE10 = 0;
                        }
                        if (item.HE11 == null)
                        {
                            item.HE11 = 0;
                        }
                        if (item.HE12 == null)
                        {
                            item.HE12 = 0;
                        }
                        if (item.HE13 == null)
                        {
                            item.HE13 = 0;
                        }
                        if (item.HE14 == null)
                        {
                            item.HE14 = 0;
                        }
                        if (item.HE15 == null)
                        {
                            item.HE15 = 0;
                        }
                        if (item.HE16 == null)
                        {
                            item.HE16 = 0;
                        }
                        if (item.HE17 == null)
                        {
                            item.HE17 = 0;
                        }
                        if (item.HE18 == null)
                        {
                            item.HE18 = 0;
                        }
                        if (item.HE19 == null)
                        {
                            item.HE19 = 0;
                        }
                        if (item.HE20 == null)
                        {
                            item.HE20 = 0;
                        }
                        if (item.HE21 == null)
                        {
                            item.HE21 = 0;
                        }
                        if (item.HE22 == null)
                        {
                            item.HE22 = 0;
                        }
                        if (item.HE23 == null)
                        {
                            item.HE23 = 0;
                        }
                        if (item.HE24 == null)
                        {
                            item.HE24 = 0;
                        }
                        item.Sum = item.HE1 + item.HE2 + item.HE3 + item.HE4 + item.HE5 + item.HE6 + item.HE7 + item.HE8 + item.HE9 + item.HE10 + item.HE11 + item.HE12 + item.HE13 +
                                       item.HE14 + item.HE15 + item.HE16 + item.HE17 + item.HE18 + item.HE19 + item.HE20 + item.HE21 + item.HE22 + item.HE23 + item.HE24;
                    }
                    ExposureList = exposureFinalList;
                    Mouse.OverrideCursor = null;
                    StartDate = tempStartDate;
                    EndDate = tempEndDate;
                }
            }
            catch (Exception ex)
            {


            }
        }
        /// <summary>
        /// Pathes the details command.
        /// </summary>
        public void PathDetailsCommand()
        {
            List<Exposure> pathList = mPathDetailHash[SelectedConstraintPathValue.ID];
            PathDetail pathDetail = new PathDetail();
            PathDetailViewModel pathDetailViewModel = new PathDetailViewModel(this, pathList, null);
            pathDetail.DataContext = pathDetailViewModel;
            pathDetail.ResizeMode = ResizeMode.CanResize;
            pathDetail.Show();// .ShowDialog();
        }

        /// <summary>
        /// Pathes the details command.
        /// </summary>
        /// <param name="selectedConstraintID">The selected constraint identifier.</param>
        /// <param name="constraintModel">The constraint model.</param>
        public void PathDetailsCommand(int selectedConstraintID, ConstraintCheckViewModel constraintModel)
        {
            List<Exposure> pathList = new List<Exposure>();
            if (mPathDetailHash.ContainsKey(selectedConstraintID))
            {
                pathList = mPathDetailHash[selectedConstraintID];
                pathList = pathList.OrderBy(t => t.Sum).ToList();
                PathDetail pathDetail = new PathDetail();
                PathDetailViewModel pathDetailViewModel = new PathDetailViewModel(this, pathList, constraintModel);
                pathDetail.DataContext = pathDetailViewModel;
                pathDetail.ResizeMode = ResizeMode.CanResize;
                pathDetail.Show();
            }
        }

        /// <summary>
        /// Gets the exposure checked.
        /// </summary>
        /// <param name="IsDollar">if set to <c>true</c> [is dollar].</param>
        /// <param name="isXml">if set to <c>true</c> [is XML].</param>
        /// <returns></returns>
        public List<Exposure> GetExposureChecked(bool IsDollar, bool isXml)
        {
            mPathDetailHash.Clear();
            DateTime similarDate = DateTime.Today;
            similarDate = DBAccess.GetSimilarDate(SubmitDate);
            Dictionary<int, List<DateTime>> nodeHash = new Dictionary<int, List<DateTime>>();
            foreach (Path path in PathList)
            {
                if (path.Sink == "")
                {
                    path.Sink = null;
                }
                int source = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                int sink = path.Sink == null ? 0 : DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;
                string[] hours = path.AnalysisType.Split('.');
                List<DateTime> marketDateList = new List<DateTime>();
                if (nodeHash.ContainsKey(source))
                {
                    marketDateList = nodeHash[source];
                    nodeHash.Remove(source);
                }
                foreach (string hour in hours)
                {
                    DateTime marketDateTime = similarDate.AddHours(Int32.Parse(hour));
                    if (!marketDateList.Contains(marketDateTime))
                    {
                        marketDateList.Add(marketDateTime);
                    }
                }
                nodeHash.Add(source, marketDateList);
                if (sink != 0)
                {
                    if (nodeHash.ContainsKey(sink))
                    {
                        marketDateList = nodeHash[sink];
                        nodeHash.Remove(sink);
                    }
                    foreach (string hour in hours)
                    {
                        DateTime marketDateTime = similarDate.AddHours(Int32.Parse(hour));
                        if (!marketDateList.Contains(marketDateTime))
                        {
                            marketDateList.Add(marketDateTime);
                        }
                    }
                    nodeHash.Add(sink, marketDateList);
                }
                if (MarketComboSelectedValue == "PJM")
                {
                    DARTNode.GetDartMarket(nodeHash, "da", 1);
                }
                else
                {
                    DARTNode.GetDartMarket(nodeHash, "da", 9);
                }

            }
            Dictionary<int, Dictionary<int, Sensitivity>> nodeSensitivityHash = _dataService.GetSensitivityByConstraintIDs(SubmitDate);
            double maxLoad = 0.0;
            if (MarketComboSelectedValue == "PJM")
                maxLoad = _dataService.GetMaxLoad(SubmitDate);
            return GetCalculatedConstraints(IsDollar, similarDate, nodeSensitivityHash, maxLoad, isXml);
        }


        /// <summary>
        /// Deletes the cancel.
        /// </summary>
        public void DeleteCancel()
        {
            List<string> cancelList = new List<string>();
            if (CancelList != null)
            {
                foreach (string cancel in CancelList)
                {
                    if (SelectedCancel != cancel)
                    {
                        cancelList.Add(cancel);
                    }
                }
            }
            CancelList = null;
            CancelList = cancelList;
        }
        /// <summary>
        /// Alls the cancel.
        /// </summary>
        public void AllCancel()
        {
            List<string> cancelList = new List<string>();
            if (CancelList != null)
            {
                foreach (string cancel in CancelList)
                {
                    cancelList.Add(cancel);
                }
            }
            foreach (Path path in PathList)
            {
                string selectedPath = path.Sink == null || path.Sink.Length == 0 ? path.Source : path.Source + "->" + path.Sink;
                if (!cancelList.Contains(selectedPath))
                {
                    cancelList.Add(selectedPath);
                }
            }
            CancelList = null;
            CancelList = cancelList;
        }
        /// <summary>
        /// Cancels the add.
        /// </summary>
        public void CancelAdd()
        {
            List<string> cancelList = new List<string>();
            if (CancelList != null)
            {
                foreach (string cancel in CancelList)
                {
                    cancelList.Add(cancel);
                }
            }
            foreach (Path path in SelectedPathByCell)
            {
                string selectedPath = path.Sink == null || path.Sink.Length == 0 ? path.Source : path.Source + "->" + path.Sink;
                if (!cancelList.Contains(selectedPath))
                {
                    cancelList.Add(selectedPath);
                }
            }
            CancelList = null;
            CancelList = cancelList;
        }
        /// <summary>
        /// Overrides this instance.
        /// </summary>
        public void Override()
        {
            PathErrorDialog pathErrorDialog = new PathErrorDialog();
            List<Path> pathList = new List<Path>();
            foreach (Path path in PathList)
            {
                if (path.RiskPath == true)
                {
                    pathList.Add(path);
                }
            }
            foreach (Path path in SelectedPathByCell)
            {
                PathErrorDialogViewModel pathErrorViewModel = new PathErrorDialogViewModel(path, mPathRiskHash, mOverrideList, pathList);
                pathErrorDialog.DataContext = pathErrorViewModel;
                pathErrorDialog.ShowDialog();
                List<Path> tempList = PathList;
                PathList = null;
                PathList = tempList;
                break;
            }
        }
        /// <summary>
        /// Sens the genericd cancel results.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SenGenericdCancelResults(string[] s, string[] e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends the generic results.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SendGenericResults(string[] s, string[] e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the preferences.
        /// </summary>
        public void SavePreferences()
        {
            try
            {
                StringBuilder preferenceHelper = new StringBuilder();
                if (DARTChecked)
                {
                    preferenceHelper.Append("avgdart?");
                }
                if (AvgDAChecked)
                {
                    preferenceHelper.Append("avgda?");
                }
                if (AvgRTChecked)
                {
                    preferenceHelper.Append("avgrt?");
                }
                if (DAMinChecked)
                {
                    preferenceHelper.Append("minda?");
                }
                if (MinRTChecked)
                {
                    preferenceHelper.Append("minrt?");
                }
                if (MinDARTChecked)
                {
                    preferenceHelper.Append("mindart?");
                }
                if (MaxDAChecked)
                {
                    preferenceHelper.Append("maxda?");
                }
                if (MaxRTChecked)
                {
                    preferenceHelper.Append("maxrt?");
                }
                if (MaxDARTChecked)
                {
                    preferenceHelper.Append("maxdart?");
                }
                if (NotionalChecked)
                {
                    preferenceHelper.Append("notional?");
                }
                if (ClearedChecked)
                {
                    preferenceHelper.Append("cleared?");
                }
                if (AsBidRiskChecked)
                {
                    preferenceHelper.Append("asbidrisk?");
                }
                if (AsBidMaxWinChecked)
                {
                    preferenceHelper.Append("asbidmaxwin?");
                }
                if (AsBidSumChecked)
                {
                    preferenceHelper.Append("asbidsum?");
                }
                if (AsBidWinPctChecked)
                {
                    preferenceHelper.Append("asbidwinpct?");
                }
                if (AsBidRiskRwdChecked)
                {
                    preferenceHelper.Append("asbidriskrwd?");
                }
                if (AsBidDARTChecked)
                {
                    preferenceHelper.Append("asbidavgdart?");
                }
                if (MustTakeRiskChecked)
                {
                    preferenceHelper.Append("musttakerisk?");
                }
                if (MustTakeMaxWinChecked)
                {
                    preferenceHelper.Append("musttakemaxwin?");
                }
                if (MustTakeSumChecked)
                {
                    preferenceHelper.Append("musttakesum?");
                }
                if (MustTakeWinPctChecked)
                {
                    preferenceHelper.Append("musttakewinpct?");
                }
                if (MustTakeRiskRwdChecked)
                {
                    preferenceHelper.Append("musttakeriskrwd?");
                }
                if (MustTakeDARTChecked)
                {
                    preferenceHelper.Append("musttakeavgdart?");
                }
                _dataService.SavePreference(Environment.UserName, preferenceHelper.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Rowses the updated.
        /// </summary>
        /// <param name="editedRow">The edited row.</param>
        /// <param name="column">The column.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        public bool RowsUpdated(Path editedRow, string column, string newValue)
        {
            try
            {
                string savedName = editedRow.IsUptos ? null : editedRow.Portfolio;
                DateTime? start = editedRow.PortfolioDate;
                PricingNode sourceNode = DBAccess.GetNodeFromName(editedRow.Source, editedRow.Market);
                PricingNode sinkNode = DBAccess.GetNodeFromName(editedRow.Sink, editedRow.Market);
                DBAccess.DeletePath(editedRow.Market, editedRow.BidId, start.Value, editedRow.IsUptos);
                List<Bid> bidList = new List<Bid>();
                List<ValidateBids> validateBidsList = new List<ValidateBids>();
                string[] hourTokens = editedRow.AnalysisType.Split('.');
                double mw = editedRow.MW;
                double price = editedRow.Price;
                bool done = false;
                if (column.Equals("Analysis Type"))
                {
                    hourTokens = newValue.Split('.');
                }
                if (column.Equals("MW"))
                {
                    mw = double.Parse(newValue);
                }
                if (column.Equals("Price"))
                {
                    price = double.Parse(newValue);
                }
                ValidateBids validateBids = new ValidateBids();
                validateBids.Source = sourceNode.NodeKey;
                validateBids.Sink = sinkNode.NodeKey;
                validateBids.AnalysisType = editedRow.AnalysisType;
                validateBids.Price = price;
                validateBids.MW = mw;
                validateBids.BidID = editedRow.BidId;
                validateBidsList.Add(validateBids);
                foreach (string hour in hourTokens)
                {
                    Bid bid = new Bid();
                    bid.Source = sourceNode.NodeKey;
                    bid.Sink = sinkNode == null ? -1 : sinkNode.NodeKey;
                    bid.MW = mw;
                    bid.Price = price;
                    bid.Market = editedRow.Market;
                    bid.MarketDateTime = start.Value.AddHours(Int16.Parse(hour));
                    bid.PortfolioKey = editedRow.PortfolioKey;
                    bid.BidId = editedRow.BidId;
                    bid.IsUptos = editedRow.IsUptos;
                    bid.Status = editedRow.Status;
                    bid.Comments = newValue;
                    bidList.Add(bid);
                }
                if (ValidateBids(validateBidsList))
                {
                    DBAccess.SaveBids(bidList, _User);
                    return true;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }
        /// <summary>
        /// Updates the rows.
        /// </summary>
        /// <param name="pathList">The path list.</param>
        public void UpdateRows(List<Path> pathList)
        {
            PathList = null;
            PathList = pathList;
            if (pathList[0].AvgDa != null)
            {
                RetrieveFetchDataAndUpdateChartCommand();
                if (sortCondition.Equals("AsBidRisk") || sortCondition.Equals("AsBidRiskReward"))
                {
                    SortingNulls();
                }
            }
        }

        /// <summary>
        /// Sortings the nulls.
        /// </summary>
        public void SortingNulls()
        {
            List<Path> temppathnulllist = null;
            List<Path> temppathnotnullList = null;

            if (sortCondition.Equals("AsBidRisk"))
            {
                temppathnulllist = PathList.FindAll(t => t.AsBidRisk == null).ToList();
                temppathnotnullList = PathList.FindAll(t => t.AsBidRisk != null).ToList();
                if (SortOrder == null || SortOrder)
                {
                    temppathnotnullList = temppathnotnullList.OrderBy(t => t.AsBidRisk).ToList();
                }
                else
                {
                    temppathnotnullList = temppathnotnullList.OrderByDescending(t => t.AsBidRisk).ToList();
                }
                temppathnotnullList.AddRange(temppathnulllist);
                PathList = null;
                PathList = temppathnotnullList;
            }
            else if (sortCondition.Equals("AsBidRiskReward"))
            {
                temppathnulllist = PathList.FindAll(t => t.AsBidRiskReward == null).ToList();
                temppathnotnullList = PathList.FindAll(t => t.AsBidRiskReward != null).ToList();
                if (!SortOrder)
                {
                    temppathnotnullList = temppathnotnullList.OrderBy(t => t.AsBidRiskReward).ToList();
                }
                else
                {
                    temppathnotnullList = temppathnotnullList.OrderByDescending(t => t.AsBidRiskReward).ToList();
                }
                temppathnotnullList.AddRange(temppathnulllist);
                PathList = null;
                PathList = temppathnotnullList;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the proxy.
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// Sets the types.
        /// </summary>
        private void SetTypes()
        {
            if (ProductComboSelectedValue == null || ProductComboSelectedValue.Length == 0)
            {
                return;
            }
            List<string> typeList = new List<string>();
            if (ProductComboSelectedValue == "Price")
            {
                typeList.Add("DART");
                typeList.Add("DA");
                typeList.Add("RT");
            }
            else if (ProductComboSelectedValue == "Load")
            {
                if (!mLoadHash.ContainsKey(MarketComboSelectedValue))
                {
                    List<Load> loadList = DBAccess.GetLoads(MarketComboSelectedValue);
                    Dictionary<string, Load> loadHash = new Dictionary<string, Load>();
                    foreach (Load load in loadList)
                    {
                        loadHash.Add(load.Name, load);
                    }
                    mLoadHash.Add(MarketComboSelectedValue, loadHash);
                }
                typeList = mLoadHash[MarketComboSelectedValue].Keys.ToList<string>();
            }
            else
            {
                if (mWeatherList.Count == 0)
                {
                    mWeatherList = DBAccess.GetWeatherCities();
                }
                typeList = mWeatherList;
            }
            TypeList = null;
            TypeList = typeList;
        }

        /// <summary>
        /// Sets the inc decimal virtual hash.
        /// </summary>
        /// <param name="decHash">The decimal hash.</param>
        /// <param name="incHash">The inc hash.</param>
        /// <param name="path">The path.</param>
        private void SetIncDecVirtualHash(Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> decHash,
                                                Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> incHash, Path path)
        {
            Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> incDecHash = path.MW > 0 ? decHash : incHash;
            Dictionary<int, Dictionary<double, List<double>>> hourHash = new Dictionary<int, Dictionary<double, List<double>>>();
            if (incDecHash.ContainsKey(path.Source))
            {
                hourHash = incDecHash[path.Source];
                incDecHash.Remove(path.Source);
            }
            string[] hours = path.AnalysisType.Split('.');
            foreach (string hour in hours)
            {
                Dictionary<double, List<double>> priceHash = new Dictionary<double, List<double>>();
                if (hourHash.ContainsKey(Int32.Parse(hour)))
                {
                    priceHash = hourHash[Int32.Parse(hour)];
                    hourHash.Remove(Int32.Parse(hour));
                }
                List<double> mwList = new List<double>();
                if (priceHash.ContainsKey(path.Price))
                {
                    mwList = priceHash[path.Price];
                    priceHash.Remove(path.Price);
                }
                mwList.Add(Math.Abs(path.MW));
                priceHash.Add(path.Price, mwList);
                hourHash.Add(Int32.Parse(hour), priceHash);
            }
            incDecHash.Add(path.Source, hourHash);
        }
        /// <summary>
        /// Gets the virtual list.
        /// </summary>
        /// <param name="isCancel">if set to <c>true</c> [is cancel].</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        private List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> GetVirtualList(bool isCancel, int market)
        {
            mCancelNodeList = null;
            if (!isCancel)
            {
                mCancelNodeList = DBAccess.GetVirtualBidsHolders(PortfolioListSelected.ID, SubmitDate, _User);
            }
            Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> decHash = new Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>>();
            Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> incHash = new Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>>();
            List<Bid> bidList = new List<Bid>();
            foreach (Path path in PathList)
            {
                if (path.PortfolioKey != PortfolioListSelected.ID)
                {
                    continue;
                }
                if (!isCancel && path.Status.ToLower() == "valid")
                {
                    continue;
                }
                if (isCancel && path.Status.ToLower() != "valid")
                {
                    continue;
                }
                if (!isCancel && path.Submit == false)
                {
                    continue;
                }
                if (isCancel)
                {
                    bool found = false;
                    foreach (string cancel in CancelList)
                    {
                        if (path.Source == cancel)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        continue;
                    }
                }
                market = path.Market;
                Bid bid = new Bid();
                bid.MarketDateTime = path.MarketDateTime;
                bid.MW = path.MW;
                bid.PortfolioKey = path.PortfolioKey;
                bid.Price = path.Price;
                PricingNode node = DBAccess.GetNodeFromName(path.Source, path.Market);
                if (market == 9 || market == 3)
                {
                    bid.BidId = path.BidId;
                }
                if (market == 1)
                    bid.SourcePnodeId = node.ExternalNodeId;
                else
                    bid.Source = node.NodeKey;
                if (mCancelNodeList != null && mCancelNodeList.Contains(bid.Source))
                {
                    mCancelNodeList.Remove(bid.Source);
                }
                bidList.Add(bid);
                SetIncDecVirtualHash(decHash, incHash, path);
            }
            DBAccess.UpdateDateVirtualBids(PortfolioListSelected.ID, SubmitDate, bidList, "IMPORTED", market);

            List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> virtualList = new List<Vayu.VirtualBidSubmissionLibrary.VirtualBid>();
            for (int i = 0; i < 2; i++)
            {
                Dictionary<string, Dictionary<int, Dictionary<double, List<double>>>> incDecHash = i == 0 ? decHash : incHash;
                List<string> keyList = incDecHash.Keys.ToList<string>();
                foreach (string key in keyList)
                {
                    string location = key;
                    if (market == 1)
                    {
                        PricingNode node = DBAccess.GetNodeFromName(key, 1);
                        location = node.ExternalNodeId.ToString();
                    }
                    Dictionary<int, Dictionary<double, List<double>>> hourHash = incDecHash[key];
                    List<int> hourKeyList = hourHash.Keys.ToList<int>();
                    hourKeyList.Sort();
                    foreach (int hour in hourKeyList)
                    {
                        Dictionary<double, List<double>> priceHash = hourHash[hour];
                        List<double> priceList = priceHash.Keys.ToList<double>();
                        priceList.Sort();
                        if (i == 0)
                        {
                            priceList.Reverse();
                        }
                        int segment = 1;
                        foreach (double price in priceList)
                        {
                            List<double> mwList = priceHash[price];
                            mwList.Sort();
                            foreach (double mw in mwList)
                            {
                                Vayu.VirtualBidSubmissionLibrary.VirtualBid virtualBid = new Vayu.VirtualBidSubmissionLibrary.VirtualBid();
                                virtualBid.Source = location;
                                virtualBid.Hour = hour;
                                virtualBid.MW = Math.Abs(mw);
                                virtualBid.Price = price;
                                virtualBid.IsInc = i == 1;
                                virtualBid.Segment = segment;
                                if (market == 9 || market == 3)
                                {
                                    virtualBid.BidId = DBAccess.GetBidId(location, hour, i == 1, segment, SubmitDate, market);
                                }
                                virtualList.Add(virtualBid);
                                segment++;
                            }
                        }
                    }
                }
            }
            return virtualList;
        }
        /// <summary>
        /// Gets the uptos list.
        /// </summary>
        /// <param name="isCancel">if set to <c>true</c> [is cancel].</param>
        /// <returns></returns>
        private List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> GetUptosList(bool isCancel)
        {
            if (PathList == null)
            {
                return null;
            }
            List<Vayu.VirtualBidSubmissionLibrary.VirtualBid> uptosList = new List<Vayu.VirtualBidSubmissionLibrary.VirtualBid>();
            foreach (Path path in PathList)
            {
                if (path.PortfolioKey != PortfolioListSelected.ID)
                {
                    continue;
                }
                if (!isCancel && (path.Status == "Valid" || path.Status == "SUBMITTED"))
                {
                    continue;
                }
                if (!isCancel && path.Submit == false)
                {
                    continue;
                }
                if (isCancel)
                {
                    bool found = false;
                    foreach (string cancel in CancelList)
                    {
                        string[] tokens = cancel.Split('>');
                        if (path.Source == tokens[0].Substring(0, tokens[0].Length - 1) && path.Sink == tokens[1])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        continue;
                    }
                }
                string[] hours = path.AnalysisType.Split('.');
                foreach (string hour in hours)
                {
                    Vayu.VirtualBidSubmissionLibrary.VirtualBid uptos = new Vayu.VirtualBidSubmissionLibrary.VirtualBid();
                    uptos.BidId = path.BidId;
                    uptos.Hour = Int32.Parse(hour);
                    uptos.MW = path.MW;
                    uptos.Price = path.Price;
                    if (path.Market == 1)
                    {
                        PricingNode sourceNode = DBAccess.GetNodeFromName(path.Source, 1);
                        uptos.Source = sourceNode.ExternalNodeId.ToString();
                        PricingNode sinkNode = DBAccess.GetNodeFromName(path.Sink, 1);
                        uptos.Sink = sinkNode.ExternalNodeId.ToString();
                    }
                    else
                    {
                        uptos.Source = path.Source;
                        uptos.Sink = path.Sink;
                    }
                    uptosList.Add(uptos);
                }
            }
            return uptosList;
        }
        /// <summary>
        /// Calculates the constraint.
        /// </summary>
        /// <param name="IsExposure">if set to <c>true</c> [is exposure].</param>
        /// <param name="IsDollar">if set to <c>true</c> [is dollar].</param>
        /// <param name="similarDate">The similar date.</param>
        /// <param name="nodeSensitivityHash">The node sensitivity hash.</param>
        /// <param name="maxLoad">The maximum load.</param>
        /// <param name="isXml">if set to <c>true</c> [is XML].</param>
        private void CalculateConstraint(bool IsExposure, bool IsDollar, DateTime similarDate,
            Dictionary<int, Dictionary<int, Sensitivity>> nodeSensitivityHash, double maxLoad, bool isXml)
        {
            Dictionary<int, Exposure> exposureHash = new Dictionary<int, Exposure>();

            foreach (Path path in PathList)
            {
                if (!path.Submit)
                {
                    continue;
                }
                int sourceNode = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                int sinkNode = path.Sink == null ? 0 : DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;

                if (nodeSensitivityHash.ContainsKey(sourceNode) && (sinkNode == 0 || nodeSensitivityHash.ContainsKey(sinkNode)))
                {
                    Dictionary<int, Sensitivity> sourceSensitivityHash = nodeSensitivityHash[sourceNode];
                    Dictionary<int, Sensitivity> sinkSensitivityHash = sinkNode == 0 ? null : nodeSensitivityHash[sinkNode];
                    List<int> sourceSensitivityKeys = sourceSensitivityHash.Keys.ToList<int>();
                    foreach (int sourceSensitivityKey in sourceSensitivityKeys)
                    {
                        if (sinkNode == 0 || sinkSensitivityHash.ContainsKey(sourceSensitivityKey))
                        {
                            Sensitivity sourceSensitivity = sourceSensitivityHash[sourceSensitivityKey];
                            Sensitivity sinkSensitivity = sinkNode == 0 ? null : sinkSensitivityHash[sourceSensitivityKey];
                            double diff = sinkNode == 0 ? sourceSensitivity.SensitivityValue : sinkSensitivity.SensitivityValue - sourceSensitivity.SensitivityValue;
                            if (IsDollar)
                            {
                                diff *= sourceSensitivity.DollarImpact;
                            }
                            Exposure exposure = new Exposure();
                            exposure.Constraint = sourceSensitivity.Constraint;
                            exposure.Contingency = sourceSensitivity.Contingency;
                            exposure.ID = sourceSensitivity.ID;
                            exposure.Shift = sourceSensitivity.ShiftFactor;
                            exposure.RiskType = sourceSensitivity.RiskType;
                            List<Exposure> pathDetailList = new List<Exposure>();
                            if (mPathDetailHash.ContainsKey(exposure.ID))
                            {
                                pathDetailList = mPathDetailHash[exposure.ID];
                                mPathDetailHash.Remove(exposure.ID);
                            }
                            Exposure pathExposure = null;
                            if (exposureHash.ContainsKey(sourceSensitivity.ID))
                            {
                                exposure = exposureHash[sourceSensitivity.ID];
                                exposureHash.Remove(sourceSensitivity.ID);
                            }
                            string[] tokens = path.AnalysisType.Split('.');
                            double total = 0;
                            double pathTotal = 0;
                            if (DARTNode.sDAHash.Count == 0)
                            {
                                MessageBox.Show("Price Server down.");
                                return;
                            }
                            foreach (string token in tokens)
                            {
                                if (AsBidChecked)
                                {
                                    int hourValue = Int32.Parse(token);
                                    DateTime sendDate = hourValue == 24 ? similarDate.Date.AddDays(1) : similarDate.Date.AddHours(hourValue);
                                    string sourceKey = sendDate.ToString() + sourceNode;
                                    string sinkKey = sinkNode == 0 ? null : sendDate.ToString() + sinkNode;
                                    double da = double.NaN;
                                    if (sinkKey != null)
                                    {
                                        if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey) &&
                                            !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sDAHash[sinkKey]))
                                        {
                                            da = (DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey]);
                                        }
                                    }
                                    else
                                    {
                                        if (DARTNode.sDAHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                                        {
                                            da = DARTNode.sDAHash[sourceKey];
                                        }
                                    }
                                    if (path.Price < da || (path.MW < 0 && path.Price > da))
                                    {
                                        continue;
                                    }
                                }
                                if (pathExposure == null)
                                {
                                    pathExposure = new Exposure();
                                    pathExposure.ID = exposure.ID;
                                    pathExposure.Constraint = path.Source;
                                    pathExposure.Contingency = path.Sink;
                                }
                                if (token == "1")
                                {
                                    if (exposure.HE1 == null)
                                    {
                                        exposure.HE1 = 0;
                                    }
                                    if (pathExposure.HE1 == null)
                                    {
                                        pathExposure.HE1 = 0;
                                    }
                                    pathExposure.HE1 = path.MW * diff;
                                    exposure.HE1 += path.MW * diff;
                                    // hourlyTotalExp.HE1 += path.MW * diff;
                                    total += (double)exposure.HE1;
                                    pathTotal += (double)pathExposure.HE1;
                                }
                                if (token == "2")
                                {
                                    if (exposure.HE2 == null)
                                    {
                                        exposure.HE2 = 0;
                                    }
                                    if (pathExposure.HE2 == null)
                                    {
                                        pathExposure.HE2 = 0;
                                    }
                                    pathExposure.HE2 = path.MW * diff;
                                    exposure.HE2 += path.MW * diff;
                                    //   hourlyTotalExp.HE2 += path.MW * diff;
                                    total += (double)exposure.HE2;
                                    pathTotal += (double)pathExposure.HE2;
                                }
                                if (token == "3")
                                {
                                    if (exposure.HE3 == null)
                                    {
                                        exposure.HE3 = 0;
                                    }
                                    if (pathExposure.HE3 == null)
                                    {
                                        pathExposure.HE3 = 0;
                                    }
                                    pathExposure.HE3 = path.MW * diff;
                                    exposure.HE3 += path.MW * diff;
                                    //    hourlyTotalExp.HE3 += path.MW * diff;
                                    total += (double)exposure.HE3;
                                    pathTotal += (double)pathExposure.HE3;
                                }
                                if (token == "4")
                                {
                                    if (exposure.HE4 == null)
                                    {
                                        exposure.HE4 = 0;
                                    }
                                    if (pathExposure.HE4 == null)
                                    {
                                        pathExposure.HE4 = 0;
                                    }
                                    pathExposure.HE4 = path.MW * diff;
                                    exposure.HE4 += path.MW * diff;
                                    //   hourlyTotalExp.HE4 += path.MW * diff;
                                    total += (double)exposure.HE4;
                                    pathTotal += (double)pathExposure.HE4;
                                }
                                if (token == "5")
                                {
                                    if (exposure.HE5 == null)
                                    {
                                        exposure.HE5 = 0;
                                    }
                                    if (pathExposure.HE5 == null)
                                    {
                                        pathExposure.HE5 = 0;
                                    }
                                    pathExposure.HE5 = path.MW * diff;
                                    exposure.HE5 += path.MW * diff;
                                    //   hourlyTotalExp.HE5 += path.MW * diff;
                                    total += (double)exposure.HE5;
                                    pathTotal += (double)pathExposure.HE5;
                                }
                                if (token == "6")
                                {
                                    if (exposure.HE6 == null)
                                    {
                                        exposure.HE6 = 0;
                                    }
                                    if (pathExposure.HE6 == null)
                                    {
                                        pathExposure.HE6 = 0;
                                    }
                                    pathExposure.HE6 = path.MW * diff;
                                    exposure.HE6 += path.MW * diff;
                                    //  hourlyTotalExp.HE6 += path.MW * diff;
                                    total += (double)exposure.HE6;
                                    pathTotal += (double)pathExposure.HE6;
                                }
                                if (token == "7")
                                {
                                    if (exposure.HE7 == null)
                                    {
                                        exposure.HE7 = 0;
                                    }
                                    if (pathExposure.HE7 == null)
                                    {
                                        pathExposure.HE7 = 0;
                                    }
                                    pathExposure.HE7 = path.MW * diff;
                                    exposure.HE7 += path.MW * diff;
                                    //  hourlyTotalExp.HE7 += path.MW * diff;
                                    total += (double)exposure.HE7;
                                    pathTotal += (double)pathExposure.HE7;
                                }
                                if (token == "8")
                                {
                                    if (exposure.HE8 == null)
                                    {
                                        exposure.HE8 = 0;
                                    }
                                    if (pathExposure.HE8 == null)
                                    {
                                        pathExposure.HE8 = 0;
                                    }
                                    pathExposure.HE8 = path.MW * diff;
                                    exposure.HE8 += path.MW * diff;
                                    //    hourlyTotalExp.HE8 += path.MW * diff;
                                    total += (double)exposure.HE8;
                                    pathTotal += (double)pathExposure.HE8;
                                }
                                if (token == "9")
                                {
                                    if (exposure.HE9 == null)
                                    {
                                        exposure.HE9 = 0;
                                    }
                                    if (pathExposure.HE9 == null)
                                    {
                                        pathExposure.HE9 = 0;
                                    }
                                    pathExposure.HE9 = path.MW * diff;
                                    exposure.HE9 += path.MW * diff;
                                    //   hourlyTotalExp.HE9 += path.MW * diff;
                                    total += (double)exposure.HE9;
                                    pathTotal += (double)pathExposure.HE9;
                                }
                                if (token == "10")
                                {
                                    if (exposure.HE10 == null)
                                    {
                                        exposure.HE10 = 0;
                                    }
                                    if (pathExposure.HE10 == null)
                                    {
                                        pathExposure.HE10 = 0;
                                    }
                                    pathExposure.HE10 = path.MW * diff;
                                    exposure.HE10 += path.MW * diff;
                                    //   hourlyTotalExp.HE10 += path.MW * diff;
                                    total += (double)exposure.HE10;
                                    pathTotal += (double)pathExposure.HE10;
                                }
                                if (token == "11")
                                {
                                    if (exposure.HE11 == null)
                                    {
                                        exposure.HE11 = 0;
                                    }
                                    if (pathExposure.HE11 == null)
                                    {
                                        pathExposure.HE11 = 0;
                                    }
                                    pathExposure.HE11 = path.MW * diff;
                                    exposure.HE11 += path.MW * diff;
                                    //     hourlyTotalExp.HE11 += path.MW * diff;
                                    total += (double)exposure.HE11;
                                    pathTotal += (double)pathExposure.HE11;
                                }
                                if (token == "12")
                                {
                                    if (exposure.HE12 == null)
                                    {
                                        exposure.HE12 = 0;
                                    }
                                    if (pathExposure.HE12 == null)
                                    {
                                        pathExposure.HE12 = 0;
                                    }
                                    pathExposure.HE12 = path.MW * diff;
                                    exposure.HE12 += path.MW * diff;
                                    //     hourlyTotalExp.HE12 += path.MW * diff;
                                    total += (double)exposure.HE12;
                                    pathTotal += (double)pathExposure.HE12;
                                }
                                if (token == "13")
                                {
                                    if (exposure.HE13 == null)
                                    {
                                        exposure.HE13 = 0;
                                    }
                                    if (pathExposure.HE13 == null)
                                    {
                                        pathExposure.HE13 = 0;
                                    }
                                    pathExposure.HE13 = path.MW * diff;
                                    exposure.HE13 += path.MW * diff;
                                    //     hourlyTotalExp.HE13 += path.MW * diff;
                                    total += (double)exposure.HE13;
                                    pathTotal += (double)pathExposure.HE13;
                                }
                                if (token == "14")
                                {
                                    if (exposure.HE14 == null)
                                    {
                                        exposure.HE14 = 0;
                                    }
                                    if (pathExposure.HE14 == null)
                                    {
                                        pathExposure.HE14 = 0;
                                    }
                                    pathExposure.HE14 = path.MW * diff;
                                    exposure.HE14 += path.MW * diff;
                                    //     hourlyTotalExp.HE14 += path.MW * diff;
                                    total += (double)exposure.HE14;
                                    pathTotal += (double)pathExposure.HE14;
                                }
                                if (token == "15")
                                {
                                    if (exposure.HE15 == null)
                                    {
                                        exposure.HE15 = 0;
                                    }
                                    if (pathExposure.HE15 == null)
                                    {
                                        pathExposure.HE15 = 0;
                                    }
                                    pathExposure.HE15 = path.MW * diff;
                                    exposure.HE15 += path.MW * diff;
                                    //     hourlyTotalExp.HE15 += path.MW * diff;
                                    total += (double)exposure.HE15;
                                    pathTotal += (double)pathExposure.HE15;
                                }
                                if (token == "16")
                                {
                                    if (exposure.HE16 == null)
                                    {
                                        exposure.HE16 = 0;
                                    }
                                    if (pathExposure.HE16 == null)
                                    {
                                        pathExposure.HE16 = 0;
                                    }
                                    pathExposure.HE16 = path.MW * diff;
                                    exposure.HE16 += path.MW * diff;
                                    //    hourlyTotalExp.HE16 += path.MW * diff;
                                    total += (double)exposure.HE16;
                                    pathTotal += (double)pathExposure.HE16;
                                }
                                if (token == "17")
                                {
                                    if (exposure.HE17 == null)
                                    {
                                        exposure.HE17 = 0;
                                    }
                                    if (pathExposure.HE17 == null)
                                    {
                                        pathExposure.HE17 = 0;
                                    }
                                    pathExposure.HE17 = path.MW * diff;
                                    exposure.HE17 += path.MW * diff;
                                    //     hourlyTotalExp.HE17 += path.MW * diff;
                                    total += (double)exposure.HE17;
                                    pathTotal += (double)pathExposure.HE17;
                                }
                                if (token == "18")
                                {
                                    if (exposure.HE18 == null)
                                    {
                                        exposure.HE18 = 0;
                                    }
                                    if (pathExposure.HE18 == null)
                                    {
                                        pathExposure.HE18 = 0;
                                    }
                                    pathExposure.HE18 = path.MW * diff;
                                    exposure.HE18 += path.MW * diff;
                                    //    hourlyTotalExp.HE18 += path.MW * diff;
                                    total += (double)exposure.HE18;
                                    pathTotal += (double)pathExposure.HE18;
                                }
                                if (token == "19")
                                {
                                    if (exposure.HE19 == null)
                                    {
                                        exposure.HE19 = 0;
                                    }
                                    if (pathExposure.HE19 == null)
                                    {
                                        pathExposure.HE19 = 0;
                                    }
                                    pathExposure.HE19 = path.MW * diff;
                                    exposure.HE19 += path.MW * diff;
                                    //    hourlyTotalExp.HE19 += path.MW * diff;
                                    total += (double)exposure.HE19;
                                    pathTotal += (double)pathExposure.HE19;
                                }
                                if (token == "20")
                                {
                                    if (exposure.HE20 == null)
                                    {
                                        exposure.HE20 = 0;
                                    }
                                    pathExposure.HE20 = path.MW * diff;
                                    exposure.HE20 += path.MW * diff;
                                    //     hourlyTotalExp.HE20 += path.MW * diff;
                                    total += (double)exposure.HE20;
                                    pathTotal += (double)pathExposure.HE20;
                                }
                                if (token == "21")
                                {
                                    if (exposure.HE21 == null)
                                    {
                                        exposure.HE21 = 0;
                                    }
                                    if (pathExposure.HE21 == null)
                                    {
                                        pathExposure.HE21 = 0;
                                    }
                                    pathExposure.HE21 = path.MW * diff;
                                    exposure.HE21 += path.MW * diff;
                                    //       hourlyTotalExp.HE21 += path.MW * diff;
                                    total += (double)exposure.HE21;
                                    pathTotal += (double)pathExposure.HE21;
                                }
                                if (token == "22")
                                {
                                    if (exposure.HE22 == null)
                                    {
                                        exposure.HE22 = 0;
                                    }
                                    if (pathExposure.HE22 == null)
                                    {
                                        pathExposure.HE22 = 0;
                                    }
                                    pathExposure.HE22 = path.MW * diff;
                                    exposure.HE22 += path.MW * diff;
                                    //       hourlyTotalExp.HE22 += path.MW * diff;
                                    total += (double)exposure.HE22;
                                    pathTotal += (double)pathExposure.HE22;
                                }
                                if (token == "23")
                                {
                                    if (exposure.HE23 == null)
                                    {
                                        exposure.HE23 = 0;
                                    }
                                    if (pathExposure.HE23 == null)
                                    {
                                        pathExposure.HE23 = 0;
                                    }
                                    pathExposure.HE23 = path.MW * diff;
                                    exposure.HE23 += path.MW * diff;
                                    //       hourlyTotalExp.HE23 += path.MW * diff;
                                    total += (double)exposure.HE23;
                                    pathTotal += (double)pathExposure.HE23;
                                }
                                if (token == "24")
                                {
                                    if (exposure.HE24 == null)
                                    {
                                        exposure.HE24 = 0;
                                    }
                                    if (pathExposure.HE24 == null)
                                    {
                                        pathExposure.HE24 = 0;
                                    }
                                    pathExposure.HE24 = path.MW * diff;
                                    exposure.HE24 += path.MW * diff;
                                    //   hourlyTotalExp.HE24 += path.MW * diff;
                                    total += (double)exposure.HE24;
                                    pathTotal += (double)pathExposure.HE24;
                                }
                            }
                            if (pathTotal == 0)
                            {

                            }
                            else
                            {
                                pathExposure.Sum = (double)pathTotal;
                            }
                            exposure.IsShiftEmpty = true;
                            exposureHash.Add(sourceSensitivity.ID, exposure);
                            if (pathExposure != null)
                            {
                                pathDetailList.Add(pathExposure);
                            }
                            mPathDetailHash.Add(exposure.ID, pathDetailList);
                        }
                    }
                }
            }
            foreach (var item in mPathDetailHash)
            {
                int constraintId = item.Key;
                List<Exposure> pathDetailList = item.Value;
                Exposure hourlyPathTot = new Exposure();
                hourlyPathTot.HE1 = pathDetailList.Sum(a => a.HE1);
                hourlyPathTot.HE2 = pathDetailList.Sum(a => a.HE2);
                hourlyPathTot.HE3 = pathDetailList.Sum(a => a.HE3);
                hourlyPathTot.HE4 = pathDetailList.Sum(a => a.HE4);
                hourlyPathTot.HE5 = pathDetailList.Sum(a => a.HE5);
                hourlyPathTot.HE6 = pathDetailList.Sum(a => a.HE6);
                hourlyPathTot.HE7 = pathDetailList.Sum(a => a.HE7);
                hourlyPathTot.HE8 = pathDetailList.Sum(a => a.HE8);
                hourlyPathTot.HE9 = pathDetailList.Sum(a => a.HE9);
                hourlyPathTot.HE10 = pathDetailList.Sum(a => a.HE10);
                hourlyPathTot.HE11 = pathDetailList.Sum(a => a.HE11);
                hourlyPathTot.HE12 = pathDetailList.Sum(a => a.HE12);
                hourlyPathTot.HE13 = pathDetailList.Sum(a => a.HE13);
                hourlyPathTot.HE14 = pathDetailList.Sum(a => a.HE14);
                hourlyPathTot.HE15 = pathDetailList.Sum(a => a.HE15);
                hourlyPathTot.HE16 = pathDetailList.Sum(a => a.HE16);
                hourlyPathTot.HE17 = pathDetailList.Sum(a => a.HE17);
                hourlyPathTot.HE18 = pathDetailList.Sum(a => a.HE18);
                hourlyPathTot.HE19 = pathDetailList.Sum(a => a.HE19);
                hourlyPathTot.HE20 = pathDetailList.Sum(a => a.HE20);
                hourlyPathTot.HE21 = pathDetailList.Sum(a => a.HE21);
                hourlyPathTot.HE22 = pathDetailList.Sum(a => a.HE22);
                hourlyPathTot.HE23 = pathDetailList.Sum(a => a.HE23);
                hourlyPathTot.HE24 = pathDetailList.Sum(a => a.HE24);
                hourlyPathTot.Constraint = "Total";
                pathDetailList.Add(hourlyPathTot);
            }
            List<Sensitivity> ConstraintNotExistList = _dataService.GetConstraintNotExist(SubmitDate);
            foreach (Sensitivity item in ConstraintNotExistList)
            {
                Exposure exposure = new Exposure();
                exposure.Constraint = item.Constraint;
                exposure.Contingency = item.Contingency;
                exposure.ID = item.ID;
                exposure.IsShiftEmpty = false;
                exposureHash.Add(item.ID, exposure);
            }
            List<Exposure> ConstraintExposureList = new List<Exposure>();
            ConstraintExposureList = exposureHash.Values.ToList<Exposure>();
            foreach (Exposure item in ConstraintExposureList)
            {
                if (item.HE1 == null)
                {
                    item.HE1 = 0;
                }
                if (item.HE2 == null)
                {
                    item.HE2 = 0;
                }
                if (item.HE3 == null)
                {
                    item.HE3 = 0;
                }
                if (item.HE4 == null)
                {
                    item.HE4 = 0;
                }
                if (item.HE5 == null)
                {
                    item.HE5 = 0;
                }
                if (item.HE6 == null)
                {
                    item.HE6 = 0;
                }
                if (item.HE7 == null)
                {
                    item.HE7 = 0;
                }
                if (item.HE8 == null)
                {
                    item.HE8 = 0;
                }
                if (item.HE9 == null)
                {
                    item.HE9 = 0;
                }
                if (item.HE10 == null)
                {
                    item.HE10 = 0;
                }
                if (item.HE11 == null)
                {
                    item.HE11 = 0;
                }
                if (item.HE12 == null)
                {
                    item.HE12 = 0;
                }
                if (item.HE13 == null)
                {
                    item.HE13 = 0;
                }
                if (item.HE14 == null)
                {
                    item.HE14 = 0;
                }
                if (item.HE15 == null)
                {
                    item.HE15 = 0;
                }
                if (item.HE16 == null)
                {
                    item.HE16 = 0;
                }
                if (item.HE17 == null)
                {
                    item.HE17 = 0;
                }
                if (item.HE18 == null)
                {
                    item.HE18 = 0;
                }
                if (item.HE19 == null)
                {
                    item.HE19 = 0;
                }
                if (item.HE20 == null)
                {
                    item.HE20 = 0;
                }
                if (item.HE21 == null)
                {
                    item.HE21 = 0;
                }
                if (item.HE22 == null)
                {
                    item.HE22 = 0;
                }
                if (item.HE23 == null)
                {
                    item.HE23 = 0;
                }
                if (item.HE24 == null)
                {
                    item.HE24 = 0;
                }
                item.Sum = item.HE1 + item.HE2 + item.HE3 + item.HE4 + item.HE5 + item.HE6 + item.HE7 + item.HE8 + item.HE9 + item.HE10 + item.HE11 + item.HE12 + item.HE13 +
                           item.HE14 + item.HE15 + item.HE16 + item.HE17 + item.HE18 + item.HE19 + item.HE20 + item.HE21 + item.HE22 + item.HE23 + item.HE24;
            }
            if (IsExposure)
            {
                ExposureList = ConstraintExposureList.OrderBy(t => t.Sum).ToList();
            }
            else
            {
                if (IsDollar)
                {
                    ConstraintExposureList = ConstraintExposureList.OrderBy(t => t.Sum).ToList();
                    ConstraintDollarCheck constraintDollarCheckWindow = new ConstraintDollarCheck();
                    ConstraintDollarCheckViewModel constraintDollarCheckViewModel = new ConstraintDollarCheckViewModel(this, ConstraintExposureList, constraintDollarCheckWindow);
                    constraintDollarCheckWindow.DataContext = constraintDollarCheckViewModel;
                    constraintDollarCheckWindow.ShowDialog();
                }
                else
                {
                    ConstraintExposureList = ConstraintExposureList.OrderBy(t => t.Sum).ToList();
                    ConstraintCheck contraintCheckWindow = new ConstraintCheck();
                    ConstraintCheckViewModel contraintCheckModel =
                        new ConstraintCheckViewModel(this, ConstraintExposureList, contraintCheckWindow, isXml);
                    contraintCheckWindow.DataContext = contraintCheckModel;
                    contraintCheckWindow.ShowDialog();
                }
            }
        }

        private void SubmitSaveFile(bool isXml)
        {
            if (PortfolioListSelected == null)
            {
                MessageBox.Show("Please select Portfolio");
                return;
            }

            FinalSubmit(isXml);

        }

        private int GetMarketKey(string market)
        {
            switch (market.ToLower())
            {

                case "ercot":
                    return 9;
                default:
                    return 0;
            }
        }

        private void ClearAll()
        {
            PathList = null;
            AsBidWinPerText = null;
            MustTakeWinPerText = null;
            AsBidSumText = null;
            MustTakeSumText = null;
            AsBidDolMW = null;
            MustTakeDolMW = null;
            AsBidRiskReward = null;
            MustTakeRiskReward = null;
            AsBidRisk = null;
            MustTakeRisk = null;
            AsBidWin = null;
            MustTakeWin = null;
            AsBidWinDate = null;
            MustTakeWinDate = null;
            AsBidMaxDrawDown = null;
            MustTakeMaxDrawDown = null;
            AsBidMaxDrawDownDate = null;
            MustTakeMaxDrawDownDate = null;
            AsBidRiskDate = null;
            MustTakeRiskDate = null;
            PlotModelUpper = null;
            PlotModelLower = null;
            PathPlotModelUpper = null;
            PathPlotModelLower = null;
            HourlyPivotList = null;
            HourlyPivotListSummary = null;
            MWText = null;
            DAText = null;
            CountText = null;
            ClearedText = null;
            ExposureList = null;
            DECText = null;
            INCText = null;
            PathMWList = null;
            mHourlySummaryHash = new Dictionary<string, HourlyPivotData>();
        }

        private bool IsValidRisk()
        {
            if (PathList == null)
            {
                return false;
            }
            Dictionary<Path, List<DateTime>> virtualNonProfitableNodeHash = new Dictionary<Path, List<DateTime>>();
            mPathRiskHash = new Dictionary<string, Dictionary<string, Tuple<DateTime, double>>>();
            for (int i = 0; i < 2; i++)
            {
                DateTime inStartDate = i == 0 ? DateTime.Today.AddDays(-45) : DateTime.Today.AddYears(-1).AddDays(-45);
                DateTime endDate = i == 0 ? DateTime.Today : DateTime.Today.AddYears(-1).AddDays(45);
                double totalMW = 0;
                double totalCredit = 0;
                Dictionary<string, double> nodeMwHash = new Dictionary<string, double>();
                RiskLimit riskLimit = _dataService.GetRiskLimit(PortfolioListSelected.ID);
                Dictionary<int, List<DateTime>> nodeHash = new Dictionary<int, List<DateTime>>();
                foreach (Path path in PathList)
                {
                    if (path.Submit == false)
                    {
                        continue;
                    }
                    List<DateTime> marketDateTimeList = new List<DateTime>();
                    int sourceNodeKey = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                    if (nodeHash.ContainsKey(sourceNodeKey))
                    {
                        marketDateTimeList = nodeHash[sourceNodeKey];
                        nodeHash.Remove(sourceNodeKey);
                    }
                    string[] hours = path.AnalysisType.Split('.');
                    totalMW += Math.Abs(path.MW * hours.Length);
                    DateTime startDate = inStartDate;
                    while (startDate <= endDate)
                    {
                        foreach (string hour in hours)
                        {
                            DateTime sendDate = startDate.AddHours(Int32.Parse(hour));
                            if (!marketDateTimeList.Contains(sendDate))
                            {
                                marketDateTimeList.Add(sendDate);
                            }
                        }
                        startDate = startDate.AddDays(1);
                    }
                    nodeHash.Add(sourceNodeKey, marketDateTimeList);
                    if (path.Sink != null && path.Sink.Length > 0)
                    {
                        marketDateTimeList = new List<DateTime>();
                        int sinkNodeKey = DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;
                        if (nodeHash.ContainsKey(sinkNodeKey))
                        {
                            marketDateTimeList = nodeHash[sinkNodeKey];
                            nodeHash.Remove(sinkNodeKey);
                        }
                        startDate = inStartDate;
                        while (startDate <= endDate)
                        {
                            foreach (string hour in hours)
                            {
                                DateTime sendDate = startDate.AddHours(Int32.Parse(hour));
                                if (!marketDateTimeList.Contains(sendDate))
                                {
                                    marketDateTimeList.Add(sendDate);
                                }
                            }
                            startDate = startDate.AddDays(1);
                        }
                        nodeHash.Add(sinkNodeKey, marketDateTimeList);
                    }
                    else
                    {
                        if (path.Market == 1 && i == 0)
                        {
                            PricingNode pricingNode = DBAccess.GetNodeFromName(path.Source, 1);
                        }
                    }
                    string sourceSinkKey = path.Sink == null ? path.Source : path.Source + path.Sink;
                    double nodeTotalMW = Math.Abs(path.MW * hours.Length);
                    if (nodeMwHash.ContainsKey(sourceSinkKey))
                    {
                        nodeTotalMW += nodeMwHash[sourceSinkKey];
                        nodeMwHash.Remove(sourceSinkKey);
                    }
                    nodeMwHash.Add(sourceSinkKey, nodeTotalMW);
                }
                if (totalCredit != 0 && riskLimit.Credit < totalCredit)
                {
                    MessageBox.Show("Exceeded Credit, allowed " + riskLimit.Credit + " bid submitted " + totalCredit);
                    return false;
                }
                List<string> nodeMWKeyList = nodeMwHash.Keys.ToList<string>();
                foreach (string nodeMwKey in nodeMWKeyList)
                {
                    double mw = nodeMwHash[nodeMwKey];
                    if (riskLimit.MWPerNode < mw)
                    {
                        MessageBox.Show("Exceeded Node MW, allowed " + riskLimit.MWPerNode + " bid submitted " + mw +
                                        " for node " + nodeMwKey);
                        return false;
                    }
                }
                if (riskLimit.TotalMW < totalMW)
                {
                    MessageBox.Show("Exceeded total MW, allowed " + riskLimit.TotalMW + " bid submitted " + totalMW);
                    return false;
                }

                DARTNode.GetDartMarket(nodeHash, "both", 9);

                List<DateTime> dateCollection = new List<DateTime>();
                DateTime startDate1 = inStartDate;
                while (startDate1 <= endDate)
                {
                    dateCollection.Add(startDate1);
                    startDate1 = startDate1.AddDays(1);
                }
                Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> asBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
                foreach (Path path in PathList)
                {

                    double risk = 0;
                    DateTime riskDate = DateTime.Today;
                    string sourceSink = path.Sink == null || path.Sink.Length == 0 ? path.Source : path.Source + "->" + path.Sink;
                    if (path.Submit == false)
                    {
                        continue;
                    }
                    foreach (DateTime startDate in dateCollection)
                    {
                        string[] hours = path.AnalysisType.Split('.');
                        double pnl = 0;
                        foreach (string hour in hours)
                        {
                            DateTime sendDate = startDate.AddHours(Int32.Parse(hour));
                            string sourceKey = sendDate.ToString() + DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey; ;
                            string sinkKey = string.Empty;
                            if (UptosChecked)
                            {
                                sinkKey = path.Sink == "" ? null : sendDate.ToString() + DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;
                            }
                            double da = double.NaN;
                            double rt = double.NaN;
                            double dart = double.NaN;
                            if (sinkKey != null)
                            {
                                if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey) &&
                                    !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sDAHash[sinkKey]))
                                {
                                    da = (DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey]);
                                }
                            }
                            else
                            {
                                if (DARTNode.sDAHash.ContainsKey(sourceKey) &&
                                    !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                                {
                                    da = DARTNode.sDAHash[sourceKey];
                                }
                            }
                            if (sinkKey != null)
                            {
                                if (DARTNode.sRTHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
                                    !double.IsNaN(DARTNode.sRTHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
                                {
                                    rt = (DARTNode.sRTHash[sinkKey] - DARTNode.sRTHash[sourceKey]);
                                }
                            }
                            else
                            {
                                if (DARTNode.sRTHash.ContainsKey(sourceKey) &&
                                    !double.IsNaN(DARTNode.sRTHash[sourceKey]))
                                {
                                    rt = DARTNode.sRTHash[sourceKey];
                                }
                            }
                            if (!double.IsNaN(da) && !double.IsNaN(rt))
                            {
                                dart = rt - da;
                            }
                            if (UptosChecked == false)
                            {
                                pnl = dart * path.MW;
                                if ((endDate - startDate).Days <= 5)
                                {
                                    if (pnl < 0)
                                    {
                                        List<DateTime> dateList;
                                        if (virtualNonProfitableNodeHash.TryGetValue(path, out dateList))
                                        {
                                            dateList.Add(sendDate);
                                            virtualNonProfitableNodeHash[path] = dateList;
                                        }
                                        else
                                        {
                                            dateList = new List<DateTime>();
                                            dateList.Add(sendDate);
                                            virtualNonProfitableNodeHash.Add(path, dateList);
                                        }
                                    }
                                }
                            }
                            if ((da <= path.Price && path.MW > 0) || (da >= path.Price && path.MW < 0))
                            {
                                PNL asBidPnl = new PNL();
                                asBidPnl.SourceSink = sourceSink;
                                if (!double.IsNaN(da))
                                {
                                    asBidPnl.DA = da;
                                }
                                if (!double.IsNaN(rt))
                                {
                                    asBidPnl.RT = rt;
                                }
                                if (!double.IsNaN(dart))
                                {
                                    asBidPnl.DART = dart;
                                }
                                pnl += (dart * path.MW);
                                Dictionary<string, Tuple<Path, PNL>> asBidPathHash = new Dictionary<string, Tuple<Path, PNL>>();
                                if (asBidMarketDateTimeHash.ContainsKey(sendDate))
                                {
                                    asBidPathHash = asBidMarketDateTimeHash[sendDate];
                                }
                                else
                                {
                                    asBidMarketDateTimeHash.Add(sendDate, asBidPathHash);
                                }
                                Tuple<Path, PNL> asBidTuple = new Tuple<Path, PNL>(path, asBidPnl);
                                if (asBidPathHash.ContainsKey(path.BidId))
                                {
                                    MessageBox.Show("Please check bid id " + path.BidId + " for duplicate hours");
                                    return false;
                                }
                                asBidPathHash.Add(path.BidId, asBidTuple);
                            }
                        }
                        if (pnl < 0 && pnl < risk)
                        {
                            risk = pnl;
                            riskDate = startDate;
                        }
                    }
                    string type = i == 0 ? "45" : "season";
                    string key = path.Sink == null ? path.Source : path.Source + "->" + path.Sink;
                    if (risk < -100000 && !mOverrideList.Contains(key))
                    {
                        Dictionary<string, Tuple<DateTime, double>> pathHash = new Dictionary<string, Tuple<DateTime, double>>();
                        if (MarketComboSelectedValue == "SPP" && type == "season")
                        {
                            continue;
                        }
                        if (mPathRiskHash.ContainsKey(type))
                        {
                            pathHash = mPathRiskHash[type];
                        }
                        else
                        {
                            mPathRiskHash.Add(type, pathHash);
                        }
                        Tuple<DateTime, double> tuple = new Tuple<DateTime, double>(riskDate, risk);
                        if (!pathHash.ContainsKey(key))
                        {
                            pathHash.Add(key, tuple);
                        }
                        path.RiskPath = true;
                    }
                    else
                    {
                        if (i == 1)
                        {
                            if (mPathRiskHash.ContainsKey("45"))
                            {
                                Dictionary<string, Tuple<DateTime, double>> pathHash = mPathRiskHash["45"];
                                if (!pathHash.ContainsKey(key))
                                {
                                    path.RiskPath = false;
                                }
                            }
                        }
                        else
                        {
                            path.RiskPath = false;
                        }
                    }
                }

                virtualNonProfitableNodeHash.Clear();
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
                if (i == 0)
                {
                    if (riskLimit.Risk > bidDrawDown)
                    {
                        MessageBox.Show("Exceeded 45 day drawdown, allowed " + riskLimit.Risk + " bid submitted " + bidDrawDown);
                        return false;
                    }
                }
                else
                {
                    if (riskLimit.SeasonRisk > bidDrawDown)
                    {
                        MessageBox.Show("Exceeded season drawdown, allowed " + riskLimit.SeasonRisk + " bid submitted " + bidDrawDown);
                        return false;
                    }
                }
            }
            if (mPathRiskHash.Count > 0)
            {
                MessageBox.Show("Exceeded Path Risk");
                List<Path> tempPathList = PathList;
                PathList = null;
                PathList = tempPathList.OrderByDescending(t => t.RiskPath).ToList();
                return false;
            }
            return true;
        }


        private void CheckProfitableNodes(Dictionary<Path, List<DateTime>> virtualNonProfitableNodeHash)
        {
            List<Path> mPathList = virtualNonProfitableNodeHash.Keys.ToList();
            List<Path> TempPathList = new List<Path>();
            foreach (var path in mPathList)
            {
                Dictionary<int, List<DateTime>> DateTimeHash = new Dictionary<int, List<DateTime>>();
                List<DateTime> tempDateList = virtualNonProfitableNodeHash[path];
                foreach (DateTime dTime in tempDateList)
                {
                    List<DateTime> dateList;
                    int hour = dTime.Hour;
                    DateTime dateOnly = dTime.Date;
                    if (DateTimeHash.TryGetValue(hour, out dateList))
                    {
                        dateList.Add(dateOnly);
                        DateTimeHash[hour] = dateList;
                    }
                    else
                    {
                        dateList = new List<DateTime>();
                        dateList.Add(dateOnly);
                        DateTimeHash.Add(hour, dateList);
                    }
                }
                foreach (var item in DateTimeHash)
                {
                    int hour = item.Key;
                    if (DateTimeHash[hour].Count == 5)
                    {
                        TempPathList.Add(path);
                    }
                }
            }
            if (TempPathList.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Negetive PNLs for last five days in : ");
                foreach (var item in TempPathList)
                {
                    builder.AppendLine(item.Source + " " + item.MW);
                }
                MessageBox.Show(builder.ToString());
            }
        }

        private void CountRows()
        {
            int count = 0;
            int cleared = 0;
            int segcount = 0;
            int twosegcount = 0;
            int bidcount = 0;
            int[] hrlist = new int[24];
            int listcount = 0;

            var results2 = from p in PathList
                           group p.BidId by new
                           {
                               p.Source,
                               p.Sink,
                               p.AnalysisType,
                               p.Price
                           } into g
                           select new { BidPath = g.Key, UniqueCount = g.ToList() };
            foreach (var item in results2)
            {
                string[] split = item.BidPath.AnalysisType.Split('.').ToArray();
                twosegcount = twosegcount + split.Count();
            }
            // change by SP
            List<string> BididCountList = new List<string>();

            foreach (Path path in PathList)
            {
                if (path.Status.ToUpper() == "SUBMITTED" || path.Status.ToUpper() == "VALID")
                {
                    cleared++;
                }
                if(BididCountList.Contains(path.BidId))
                {

                }
                else
                {
                    BididCountList.Add(path.BidId);
                    bidcount++;
                }
                count++;
                List<int> list = new List<int>();
                var hrs = path.AnalysisType.Split('.');
                foreach (var item in hrs)
                {
                    list.Add(int.Parse(item));
                }
                var result = list.GroupWhile((x, y) => y - x == 1)
                 .Select(x => new { i = x.First(), len = x.Count() })
                 .ToList();
                segcount = segcount + result.Count();

            }
            CountText = null;
            //CountText = count.ToString();
            CountText = bidcount.ToString();
            ClearedText = null;
            ClearedText = cleared.ToString();
            Segext = segcount.ToString();
            TwoSegext = twosegcount.ToString();
        }

        private void GetTotalPnlHash(Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> marketDateTimeHash, out double totalBidPnl, out double totalBidWin, out double totalBidMW,
                                                            out double bidMax, out double bidMin, out DateTime bidWinDate, out double bidDrawDown, out DateTime bidRiskDate,
                                                            out DateTime startBidDrawdownDate, out DateTime endBidDrawdownDate, out Dictionary<DateTime, PNL> totalPnlHash,
                                                            out Dictionary<DateTime, PNL> totalDailyPnlHash)
        {
            //   Stopwatch stopwatch = Stopwatch.StartNew();

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
                Dictionary<string, Tuple<Path, PNL>> pathHash = marketDateTimeHash[marketDateTime];
                totalPnl = new PNL();
                totalPnlHash.Add(marketDateTime, totalPnl);
                if (lastDate.Date != compareDate)
                {
                    totalDailyPnl = new PNL();
                    lastDate = compareDate;
                    totalDailyPnlHash.Add(compareDate, totalDailyPnl);
                }
                foreach (Tuple<Path, PNL> tuple in pathHash.Values)
                {
                    Path path = tuple.Item1;
                    PNL pnl = tuple.Item2;
                    totalPnl.DA += pnl.DA * path.MW;
                    totalPnl.RT += pnl.RT * path.MW;
                    totalPnl.DART += pnl.DART * path.MW;
                    totalDailyPnl.DA += pnl.DA * path.MW;
                    totalDailyPnl.RT += pnl.RT * path.MW;
                    totalDailyPnl.DART += pnl.DART * path.MW;
                    totalBidMW += Math.Abs(path.MW);
                    totalBidPnl += pnl.DART * path.MW;
                }
                if (DailyChecked && lastDate != compareDate)
                {
                    continue;
                }
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

            // Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");
        }//GetTotalPNLHash

        private void GetSummaryNodeList(Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> asBidMarketDateTimeHash, Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> mustTakeMarketDateTimeHash,
                                            out List<Node> asBidTotalNodeList, out List<Node> mustTakeTotalNodeList, bool isPath)
        {

            // Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> marketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> market1DateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            asBidTotalNodeList = new List<Node>();
            mustTakeTotalNodeList = new List<Node>();
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
            if (!isPath)
            {
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
            }
            Dictionary<DateTime, PNL> mustTakeTotalPnlHash = new Dictionary<DateTime, PNL>();
            Dictionary<DateTime, PNL> mustTakeTotalDailyPnlHash = new Dictionary<DateTime, PNL>();
            GetTotalPnlHash(mustTakeMarketDateTimeHash, out totalBidPnl, out totalBidWin, out totalBidMW, out bidMax, out bidMin, out bidWinDate,
                                                        out bidDrawDown, out bidRiskDate, out startBidDrawdownDate, out endBidDrawdownDate, out mustTakeTotalPnlHash,
                                                        out mustTakeTotalDailyPnlHash);
            if (!isPath)
            {
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
            }
            for (int i = 0; i < 2; i++)
            {
                Dictionary<DateTime, PNL> totalPnlHash = i == 0 ? asBidTotalPnlHash : mustTakeTotalPnlHash;
                List<DateTime> totalPnlDateTimeKeys = totalPnlHash.Keys.ToList<DateTime>();
                totalPnlDateTimeKeys.Sort();
                Node daNode = new Node();
                daNode.Market = 1;
                daNode.NodeName = "DA";
                daNode.TimePriceList = new List<TimePrice>();
                Node rtNode = new Node();
                rtNode.Market = 1;
                rtNode.NodeName = "RT";
                rtNode.TimePriceList = new List<TimePrice>();
                Node dartNode = new Node();
                dartNode.Market = 1;
                dartNode.NodeName = "DART";
                dartNode.TimePriceList = new List<TimePrice>();
                Dictionary<DateTime, double> datePnlHash = new Dictionary<DateTime, double>();
                foreach (DateTime marketDateTime in totalPnlDateTimeKeys)
                {
                    PNL pnl = totalPnlHash[marketDateTime];
                    TimePrice daTimePrice = new TimePrice();
                    daTimePrice.MarketTime = marketDateTime;
                    daTimePrice.Price = pnl.DA;
                    daNode.TimePriceList.Add(daTimePrice);
                    TimePrice rtTimePrice = new TimePrice();
                    rtTimePrice.MarketTime = marketDateTime;
                    rtTimePrice.Price = pnl.RT;
                    rtNode.TimePriceList.Add(rtTimePrice);
                    TimePrice dartTimePrice = new TimePrice();
                    dartTimePrice.MarketTime = marketDateTime;
                    dartTimePrice.Price = pnl.DART;
                    dartNode.TimePriceList.Add(dartTimePrice);
                }
                if (i == 0)
                {
                    asBidTotalNodeList.Add(daNode);
                    asBidTotalNodeList.Add(rtNode);
                    asBidTotalNodeList.Add(dartNode);
                }
                else
                {
                    mustTakeTotalNodeList.Add(daNode);
                    mustTakeTotalNodeList.Add(rtNode);
                    mustTakeTotalNodeList.Add(dartNode);
                }
            }
            // Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

        }//GetSumarynodelist

        private void SetAllInvalidHours(string product, string filterType, double? min, double? max)
        {
            if (product == "Price")
            {
                List<DateTime> bidKeys = mMustTakeMarketDateTimeHash.Keys.ToList<DateTime>();
                foreach (DateTime dateTime in bidKeys)
                {
                    Dictionary<string, Tuple<Path, PNL>> bidHash = mMustTakeMarketDateTimeHash[dateTime];
                    List<string> bids = bidHash.Keys.ToList<string>();
                    foreach (string bid in bids)
                    {
                        Tuple<Path, PNL> tuple = bidHash[bid];
                        PNL pnl = tuple.Item2;
                        if (filterType.IndexOf("DART") != -1)
                        {
                            if (pnl.DART > max || pnl.DART < min)
                            {
                                mHourList.Add(dateTime);
                            }
                        }
                        else if (filterType.IndexOf("RT") != -1)
                        {
                            if (pnl.RT > max || pnl.RT < min)
                            {
                                mHourList.Add(dateTime);
                            }
                        }
                        else if (filterType.IndexOf("DA") != -1)
                        {
                            if (pnl.DA > max || pnl.DA < min)
                            {
                                mHourList.Add(dateTime);
                            }
                        }
                    }
                }
            }
            else if (product == "Temp")
            {
                List<DateTime> dateList = new List<DateTime>();
                double sendMax = max == null ? 1000000 : (double)max;
                double sendMin = min == null ? -1000000 : (double)min;
                dateList = DBAccess.GetInvalidWeatherHoursList(product, filterType, StartDate.Date, EndDate.Date, sendMin, sendMax);
                foreach (DateTime dateTime in dateList)
                {
                    if (!mHourList.Contains(dateTime))
                    {
                        mHourList.Add(dateTime);
                    }
                }
            }
            else
            {
                List<string> marketKeys = mLoadHash.Keys.ToList<string>();
                foreach (string marketKey in marketKeys)
                {
                    bool found = false;
                    Dictionary<string, Load> loadHash = mLoadHash[marketKey];
                    List<string> loadKeys = loadHash.Keys.ToList<string>();
                    foreach (string name in loadKeys)
                    {
                        if (name == filterType)
                        {
                            Load load = loadHash[name];
                            List<DateTime> dateList = new List<DateTime>();
                            double sendMax = max == null ? 1000000 : (double)max;
                            double sendMin = min == null ? -1000000 : (double)min;
                            dateList = DBAccess.GetInvalidLoadHoursList(load.Key, StartDate.Date, EndDate.Date, sendMin, sendMax);
                            foreach (DateTime dateTime in dateList)
                            {
                                if (!mHourList.Contains(dateTime))
                                {
                                    mHourList.Add(dateTime);
                                }
                            }
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
        }

        private void FilterAll(out Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> asBidMarketDateTimeHash,
                                out Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> mustTakeMarketDateTimeHash, string path)
        {
            //  Stopwatch stopwatch = new Stopwatch();
            //   stopwatch.Start();
            asBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            mustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            if (ProductComboSelectedValue == "Load" && FilterList != null && TypeComboSelectedValue == "PJM RTO Total")
            {
                if (FilterList.Count > 0)
                {
                    FilterLoadDateList = FilterDatesForLoad(HourlyPivotList, Convert.ToDouble(MinText), Convert.ToDouble(MaxText));
                }
            }
            for (int i = 0; i < 2; i++)
            {
                List<DateTime> marketDateTimeList = i == 0 ? mAsBidMarketDateTimeHash.Keys.ToList<DateTime>() : mMustTakeMarketDateTimeHash.Keys.ToList<DateTime>();
                foreach (DateTime marketDateTime in marketDateTimeList)
                {
                    if (FilterLoadDateList != null && FilterList != null)
                    {
                        if (FilterList.Count > 0)
                        {
                            if (!FilterLoadDateList.Contains(marketDateTime.AddMinutes(-1).Date.ToString("dd-MM-yyyy")))
                            {
                                continue;
                            }
                        }
                    }

                    if (IsValid(marketDateTime))
                    {
                        Dictionary<string, Tuple<Path, PNL>> copyBidHash = new Dictionary<string, Tuple<Path, PNL>>();
                        Dictionary<string, Tuple<Path, PNL>> bidHash = i == 0 ? mAsBidMarketDateTimeHash[marketDateTime] : mMustTakeMarketDateTimeHash[marketDateTime];
                        List<string> bidList = bidHash.Keys.ToList<string>();
                        foreach (string bid in bidList)
                        {
                            Tuple<Path, PNL> tuple = bidHash[bid];
                            string compPath = tuple.Item1.Sink == null || tuple.Item1.Sink.Length == 0 ? tuple.Item1.Source : tuple.Item1.Source + "->" + tuple.Item1.Sink;
                            if (path == null || path == compPath)
                            {
                                Tuple<Path, PNL> copyTuple = new Tuple<Path, PNL>(new Path(tuple.Item1), new PNL(tuple.Item2));
                                copyBidHash.Add(bid, copyTuple);
                            }
                        }
                        if (copyBidHash.Count > 0)
                        {
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
            // Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

        }//FilterAll

        private List<Node> GetNodeList(out List<Node> asBidDaSendFilteredSortedList, out List<Node> asBidRtSendFilteredSortedList, out List<Node> asBidDartSendFilteredSortedList,
                                        out List<Node> mustTakeDaSendFilteredSortedList, out List<Node> mustTakeRtSendFilteredSortedList,
                                        out List<Node> mustTakeDartSendFilteredSortedList, string path)
        {
            // Stopwatch stopwatch = new Stopwatch();
            //  stopwatch.Start();
            asBidDaSendFilteredSortedList = new List<Node>();
            asBidRtSendFilteredSortedList = new List<Node>();
            asBidDartSendFilteredSortedList = new List<Node>();
            mustTakeDaSendFilteredSortedList = new List<Node>();
            mustTakeRtSendFilteredSortedList = new List<Node>();
            mustTakeDartSendFilteredSortedList = new List<Node>();
            List<Node> asBidSummaryNodeList = new List<Node>();
            List<Node> mustTakeSummaryList = new List<Node>();
            mHourList = new List<DateTime>();
            FilterLoadDateList = new List<string>();

            Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> asBidMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> mustTakeMarketDateTimeHash = new Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>>();
            FilterAll(out asBidMarketDateTimeHash, out mustTakeMarketDateTimeHash, path);
            bool isPath = path != null;
            GetSummaryNodeList(asBidMarketDateTimeHash, mustTakeMarketDateTimeHash, out asBidSummaryNodeList, out mustTakeSummaryList, isPath);
            if (asBidSummaryNodeList == null || mustTakeSummaryList == null)
            {
                return null;
            }
            List<Node> asBidNodeList = new List<Node>();
            List<Node> mustTakeNodeList = new List<Node>();
            for (int i = 0; i < 2; i++)
            {
                List<Node> daFilteredSortedList;
                List<Node> rtFilteredSortedList;
                List<Node> dartFilteredSortedList;
                List<Node> sendNodeList = i == 0 ? asBidSummaryNodeList : mustTakeSummaryList;
                RefreshFilterData(sendNodeList, out daFilteredSortedList, out rtFilteredSortedList, out dartFilteredSortedList);
                List<Node> node1List = new List<Node>();
                if (i == 0)
                {
                    asBidDaSendFilteredSortedList = daFilteredSortedList;
                    asBidRtSendFilteredSortedList = rtFilteredSortedList;
                    asBidDartSendFilteredSortedList = dartFilteredSortedList;
                    node1List = asBidNodeList;
                }
                else
                {
                    mustTakeDaSendFilteredSortedList = daFilteredSortedList;
                    mustTakeRtSendFilteredSortedList = rtFilteredSortedList;
                    mustTakeDartSendFilteredSortedList = dartFilteredSortedList;
                    node1List = mustTakeNodeList;
                }
                foreach (Node node in daFilteredSortedList)
                {
                    node1List.Add(node);
                }
                foreach (Node node in rtFilteredSortedList)
                {
                    node1List.Add(node);
                }
                foreach (Node node in dartFilteredSortedList)
                {
                    node1List.Add(node);
                }
            }
            List<Node> nodeList = AsBidChecked ? asBidNodeList : mustTakeNodeList;
            if (DailyChecked)
            {
                List<Node> asBidSendNodeList = new List<Node>();
                List<Node> mustTakeSendNodeList = new List<Node>();
                ConvertDailyValues(asBidNodeList, mustTakeNodeList, out asBidSendNodeList, out mustTakeSendNodeList);
                nodeList = AsBidChecked ? asBidSendNodeList : mustTakeSendNodeList;
            }
            Sort(nodeList);

            // Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

            return nodeList;
        }//GetNodeList

        private void SetPathRisk()
        { //Stopwatch stopwatch = Stopwatch.StartNew();
            if (PathList == null)
            {
                return;
            }
            List<Path> pathList = new List<Path>();
            foreach (Path path in PathList)
            {
                Path clonePath = new Path(path);
                List<DateTime> marketDateList = mMustTakeMarketDateTimeHash.Keys.ToList<DateTime>();
                marketDateList.Sort();
                double? asBidRisk = double.MaxValue;
                double? asBidMaxWin = double.MinValue;
                DateTime lastDate = DateTime.MaxValue;
                double asBidDayTotal = 0;
                double asBidTotal = 0;
                double asBidDays = 0;
                double asBidWins = 0;
                double? mustTakeRisk = double.MaxValue;
                double? mustTakeMaxWin = double.MinValue;
                double mustTakeDayTotal = 0;
                double mustTakeTotalDart = 0;
                double mustTakeTotalDa = 0;
                double mustTakeTotalRt = 0;
                double mustTakeDays = 0;
                double mustTakeWins = 0;
                double? minDa = double.MaxValue;
                double? maxDa = double.MinValue;
                double? minRt = double.MaxValue;
                double? maxRt = double.MinValue;
                double? minDart = double.MaxValue;
                double? maxDart = double.MinValue;
                double totalMw = 0;
                double totalFilteredMW = 0;
                double total = 0;
                double totalCleared = 0;
                foreach (DateTime date in marketDateList)
                {
                    if (lastDate == DateTime.MaxValue)
                    {
                        lastDate = date;
                    }
                    DateTime compareDate = date.Hour == 0 ? date.Date.AddDays(-1) : date;
                    if (compareDate.Date != lastDate.Date)
                    {
                        if (asBidDayTotal < 0 && asBidRisk > asBidDayTotal)
                        {
                            asBidRisk = Math.Round(asBidDayTotal, 0);
                        }
                        if (asBidMaxWin < asBidDayTotal)
                        {
                            asBidMaxWin = Math.Round(asBidDayTotal, 0);
                        }
                        if (asBidDayTotal != 0)
                        {
                            asBidDays++;
                        }
                        if (asBidDayTotal > 0)
                        {
                            asBidWins++;
                        }
                        asBidDayTotal = 0;
                        if (mustTakeDayTotal < 0 && mustTakeRisk > mustTakeDayTotal)
                        {
                            mustTakeRisk = Math.Round(mustTakeDayTotal, 0);
                        }
                        if (mustTakeMaxWin < mustTakeDayTotal)
                        {
                            mustTakeMaxWin = Math.Round(mustTakeDayTotal, 0);
                        }
                        if (mustTakeDayTotal != 0)
                        {
                            mustTakeDays++;
                        }
                        if (mustTakeDayTotal > 0)
                        {
                            mustTakeWins++;
                        }
                        mustTakeDayTotal = 0;
                        lastDate = date;
                    }
                    if (mAsBidMarketDateTimeHash.ContainsKey(date))
                    {
                        Dictionary<string, Tuple<Path, PNL>> asBidHash = mAsBidMarketDateTimeHash[date];
                        if (asBidHash.ContainsKey(path.BidId))
                        {
                            Tuple<Path, PNL> tuple = asBidHash[path.BidId];
                            asBidDayTotal += tuple.Item2.DART * tuple.Item1.MW;
                            if (tuple.Item2.RT != 0 && tuple.Item2.RT != double.NaN)
                            {
                                asBidTotal += tuple.Item2.DART * tuple.Item1.MW;
                                totalFilteredMW += tuple.Item1.MW;
                            }
                            else if (tuple.Item2.RT == 0 && date < DateTime.Now.AddHours(1))
                            {
                                asBidTotal += tuple.Item2.DART * tuple.Item1.MW;
                                totalFilteredMW += tuple.Item1.MW;
                            }
                        }
                    }
                    if (mMustTakeMarketDateTimeHash.ContainsKey(date))
                    {
                        Dictionary<string, Tuple<Path, PNL>> mustTakeHash = mMustTakeMarketDateTimeHash[date];
                        if (mustTakeHash.ContainsKey(path.BidId))
                        {
                            Tuple<Path, PNL> tuple = mustTakeHash[path.BidId];
                            if (minDart > tuple.Item2.DART)
                            {
                                minDart = tuple.Item2.DART;
                            }
                            if (maxDart < tuple.Item2.DART)
                            {
                                maxDart = tuple.Item2.DART;
                            }
                            if (minDa > tuple.Item2.DA)
                            {
                                minDa = tuple.Item2.DA;
                            }
                            if (maxDa < tuple.Item2.DA)
                            {
                                maxDa = tuple.Item2.DA;
                            }
                            if (minRt > tuple.Item2.RT)
                            {
                                minRt = tuple.Item2.RT;
                            }
                            if (maxRt < tuple.Item2.RT)
                            {
                                maxRt = tuple.Item2.RT;
                            }
                            mustTakeDayTotal += tuple.Item2.DART * tuple.Item1.MW;
                            totalMw += tuple.Item1.MW;
                            if (tuple.Item2.RT != 0 && tuple.Item2.RT != double.NaN)
                            {
                                // totalMw += tuple.Item1.MW;
                                mustTakeTotalDart += tuple.Item2.DART * tuple.Item1.MW;
                                mustTakeTotalRt += tuple.Item2.RT * tuple.Item1.MW;
                            }
                            else if (tuple.Item2.RT == 0 && date < DateTime.Now.AddHours(1))
                            {
                                asBidTotal += tuple.Item2.DART * tuple.Item1.MW;
                                totalFilteredMW += tuple.Item1.MW;
                            }
                            mustTakeTotalDa += tuple.Item2.DA * tuple.Item1.MW;
                            //mustTakeTotalRt += tuple.Item2.RT * tuple.Item1.MW;
                            if (path.Price >= tuple.Item2.DA)
                            {
                                totalCleared++;
                            }
                            total++;
                        }
                    }
                }
                if (asBidDayTotal < 0 && asBidRisk > asBidDayTotal)
                {
                    asBidRisk = Math.Round(asBidDayTotal, 0);
                }
                if (asBidMaxWin < asBidDayTotal)
                {
                    asBidMaxWin = Math.Round(asBidDayTotal, 0);
                }
                clonePath.AsBidRisk = asBidRisk == double.MaxValue ? null : asBidRisk;
                clonePath.AsBidMaxWin = asBidMaxWin == double.MinValue ? null : asBidMaxWin;
                double? asBidRiskReward = null;
                if (asBidMaxWin != double.MinValue && asBidRisk != double.MaxValue)
                {
                    asBidRiskReward = Math.Round(Math.Abs((double)asBidMaxWin / (double)asBidRisk), 2);
                }
                clonePath.AsBidRiskReward = asBidRiskReward;
                clonePath.AsBidSum = Math.Round((double)asBidTotal, 0);
                clonePath.AsBidWinPer = asBidDays == 0 ? 0 : asBidWins / asBidDays;

                if (totalFilteredMW != 0 && totalFilteredMW != double.NaN && totalFilteredMW != double.MaxValue && totalFilteredMW != double.MinValue)
                {
                    clonePath.AsBidAvgDart = Math.Round((double)asBidTotal / (double)totalFilteredMW, 2);
                }
                if (mustTakeDayTotal < 0 && mustTakeRisk > mustTakeDayTotal)
                {
                    mustTakeRisk = Math.Round(mustTakeDayTotal, 0);
                }
                if (mustTakeMaxWin < mustTakeDayTotal)
                {
                    mustTakeMaxWin = Math.Round(mustTakeDayTotal, 0);
                }
                clonePath.MustTakeRisk = mustTakeRisk == double.MaxValue ? null : mustTakeRisk;
                clonePath.MustTakeMaxWin = mustTakeMaxWin == double.MinValue ? null : mustTakeMaxWin;
                double? mustTakeRiskReward = null;
                if (mustTakeMaxWin != double.MinValue && mustTakeRisk != double.MaxValue)
                {
                    mustTakeRiskReward = Math.Round(Math.Abs((double)mustTakeMaxWin / (double)mustTakeRisk), 2);
                }
                clonePath.MustTakeRiskReward = mustTakeRiskReward;
                clonePath.MustTakeSum = Math.Round((double)mustTakeTotalDart, 0);
                clonePath.MustTakeWinPer = mustTakeDays == 0 ? 0 : mustTakeWins / mustTakeDays;
                double avgDa = (double)mustTakeTotalDa / (double)totalMw;
                clonePath.AvgDa = Math.Round(avgDa, 2);
                clonePath.MinDa = Math.Round((double)minDa, 2);
                clonePath.MaxDa = Math.Round((double)maxDa, 2);
                clonePath.AvgRt = Math.Round((double)mustTakeTotalRt / (double)totalMw, 2);
                clonePath.MinRt = Math.Round((double)minRt, 2);
                clonePath.MaxRt = Math.Round((double)maxRt, 2);
                clonePath.AvgDart = Math.Round((double)mustTakeTotalDart / (double)totalMw, 2);
                clonePath.MinDart = Math.Round((double)minDart, 2);
                clonePath.MaxDart = Math.Round((double)maxDart, 2);
                double notional = Math.Round((double)mustTakeTotalDa / (double)totalMw, 2);
                clonePath.Notional = avgDa > 0 ? Math.Round(avgDa * path.MW * path.AnalysisType.Split('.').Length, 0) : 0;
                clonePath.ClearedPer = totalCleared / total;
                pathList.Add(clonePath);
            }
            PathList = null;
            PathList = pathList;

            //stopwatch.Stop();

            // Log the elapsed time using Trace
            // Trace.WriteLine($"SomeMethod execution time: {stopwatch.ElapsedMilliseconds} ms");

        }

        private void SetPathGraphs()
        {
            Dictionary<DateTime, Dictionary<string, Tuple<Path, PNL>>> marketDateHash = AsBidChecked ? mAsBidMarketDateTimeHash : mMustTakeMarketDateTimeHash;
            if (SelectedHourlyPivot == null)
            {
                return;
            }
            DateTime? date = SelectedHourlyPivot.Date;
            Dictionary<string, PNL> pnlHash = new Dictionary<string, PNL>();
            for (int i = 0; i < 24; i++)
            {
                DateTime marketDateTime = ((DateTime)date).AddHours(i + 1);
                if (!marketDateHash.ContainsKey(marketDateTime) || !IsValid(marketDateTime))
                {
                    continue;
                }
                Dictionary<string, Tuple<Path, PNL>> pathHash = marketDateHash[marketDateTime];
                foreach (Tuple<Path, PNL> tuple in pathHash.Values.ToList<Tuple<Path, PNL>>())
                {
                    PNL pnl = tuple.Item2;
                    PNL sendPnl = new PNL();
                    sendPnl.SourceSink = pnl.SourceSink;
                    sendPnl.DA = pnl.DA * tuple.Item1.MW;
                    sendPnl.RT = pnl.RT * tuple.Item1.MW;
                    sendPnl.DART = pnl.DART * tuple.Item1.MW;
                    if (pnlHash.ContainsKey(sendPnl.SourceSink))
                    {
                        PNL compPnl = pnlHash[pnl.SourceSink];
                        sendPnl.DA += compPnl.DA;
                        sendPnl.RT += compPnl.RT;
                        sendPnl.DART += compPnl.DART;
                        pnlHash.Remove(sendPnl.SourceSink);
                    }
                    pnlHash.Add(sendPnl.SourceSink, sendPnl);
                }
            }
            PathDayPlotModelUpper = null;
            if (pnlHash != null)
            {
                PathDayPlotModelUpper = CreatePathPlotModelUpper(pnlHash);
            }
        }

        private void SetHourlyPivotList(List<Path> PathList)
        {
            Dictionary<DateTime, double> absClearedMwDict = new Dictionary<DateTime, double>();
            if (mHourlyPivotHash == null)
            {
                mHourlyPivotHash = null;
                return;
            }
            if (PathList != null && PathList.Count > 0)
            {
                if ((DARTNode.sDAHash == null || DARTNode.sDAHash.Count == 0))
                {
                    DARTNode.GetDartsForUptos(null, null, StartDate, EndDate.AddDays(1));
                }
            }
            if (clearedMsHash == null || clearedMsHash.Count == 0)
            {

                clearedMsHash = CalculateDailyClearedMWs(PathList, StartDate, EndDate, out absClearedMwDict);
                absClearedMwDicttemp = absClearedMwDict;
            }
            absClearedMwDict = absClearedMwDicttemp;
            List<HourlyPivotData> hourlyPivotData = new List<HourlyPivotData>();
            string rowType;
            Dictionary<string, Summary> maxHourHash = new Dictionary<string, Summary>();
            foreach (var nodeList in mHourlyPivotHash)
            {
                List<Node> HourlyDAPriceList = mHourlyPivotHash["DA"];
                rowType = nodeList.Key;
                int counter = 0;

                int daCount = DARTNode.sDAHash.Count;
                int rtCount = DARTNode.sRTHash.Count;
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
                        if (!DaCheckedcomp)
                            continue;
                    }
                    if (!FilterDayComparisonRTChecked && nodeList.Key == "RT" && counter == compareCounter)
                    {
                        counter++;
                        if (!RtCheckedcomp)
                            continue;
                    }
                    if (!FilterDayComparisonDARTChecked && nodeList.Key == "DART" && counter == compareCounter)
                    {
                        counter++;
                        if (!DartCheckedcomp)
                            continue;
                    }
                    //
                    var itemlist = item.TimePriceList.OrderBy(i => i.MarketTime).ToArray();
                    DateTime dateCounter = DateTime.Now.Date;
                    if (itemlist.FirstOrDefault() != null)
                    {
                        dateCounter = itemlist.FirstOrDefault().MarketTime.Date;
                    }
                    double total = 0;
                    double avgcounter = 0;
                    HourlyPivotData hpdata = new HourlyPivotData();
                    hpdata.RowType = rowType;

                    if (MarketComboSelectedValue == "ERCOT")
                    {
                        if (loadDictHash.Count() == 0)
                        {
                            DataService ds = new DataService();
                            List<DateTime> datetimeList = itemlist.Select(x => x.MarketTime).ToList<DateTime>();
                            loadDictHash = ds.GetLoadsData(datetimeList, MarketComboSelectedValue);
                        }
                    }
                    foreach (TimePrice hourPrice in itemlist)
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
                            //DataService ds = new DataService();
                            //double? tempMaxLoad = ds.GetMaxHourlyLoad(hourPrice.MarketTime.Date);
                            //if (tempMaxLoad != null)
                            //{
                            //    hpdata.MaxLoad = tempMaxLoad;
                            //}
                            //else
                            //{
                            //    hpdata.MaxLoad = ds.GetMaxDailyLoad(hourPrice.MarketTime.Date);
                            //}
                            if (loadDictHash.ContainsKey(dateCounter.ToString("dd-MM-yyyy")))
                            {
                                hpdata.MaxLoad = loadDictHash[dateCounter.ToString("dd-MM-yyyy")];
                            }
                            if (clearedMsHash.Count > 0 || clearedMsHash != null)
                            {
                                if (clearedMsHash.ContainsKey(hourPrice.MarketTime.Date))
                                {
                                    if (hourPrice.MarketTime.Hour != 0)
                                    {
                                        hpdata.ClearedMW = clearedMsHash[hourPrice.MarketTime.Date];
                                        if (absClearedMwDict != null && absClearedMwDict.Count > 0)
                                            hpdata.AbsClearedMW = absClearedMwDict[hourPrice.MarketTime.Date];
                                    }
                                }
                            }
                            hpdata.Date = hourPrice.MarketTime.AddMinutes(-1).Date;
                            hpdata.DateDisplay = hourPrice.MarketTime.AddMinutes(-1).Date;
                            hpdata.RowDay = dateCounter.Date.ToString("ddd");
                            hpdata.RowType = rowType;
                            hpdata.RowDisplayType = rowType;
                        }
                        string hour = "HE" + hourPrice.MarketTime.Hour.ToString();
                        double? price = null;
                        if (hour.Equals("HE0"))
                        {
                            hour = "HE24";
                        }
                        if (!hourPrice.Price.Equals(double.NaN))
                        {
                            total += hourPrice.Price;
                            price = hourPrice.Price;
                            if (rowType == "DART")
                            {
                                if (MaxDart == null || MaxDart < price)
                                {
                                    MaxDart = price;
                                }
                                if (MinDart == null || MinDart > price)
                                {
                                    MinDart = price;
                                }
                            }
                            avgcounter++;
                        }
                        hpdata.GetType().GetProperty(hour).SetValue(hpdata, price, null);
                        if (price != null)
                        {
                            Summary summary = new Summary();
                            summary.Max = (double)price;
                            summary.Min = (double)price;
                            summary.Total = (double)price;
                            summary.Count = 1;
                            summary.TotalWin = 1;
                            if (maxHourHash.ContainsKey(hour))
                            {
                                Summary tempSummary = maxHourHash[hour];
                                if (tempSummary.Max > price)
                                {
                                    summary.Max = tempSummary.Max;
                                }
                                if (tempSummary.Min < price)
                                {
                                    summary.Min = tempSummary.Min;
                                }
                                summary.Total += tempSummary.Total;
                                summary.Count += tempSummary.Count;
                                if (price > 0)
                                {
                                    summary.TotalWin += tempSummary.TotalWin;
                                }
                                else
                                {
                                    summary.TotalWin = tempSummary.TotalWin;
                                }
                                maxHourHash.Remove(hour);
                            }
                            maxHourHash.Add(hour, summary);
                        }
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

            if (ProductComboSelectedValue == "Load" && FilterList != null && TypeComboSelectedValue == "PJM RTO Total")
            {

            }
            List<HourlyPivotData> sorttempHourlyPivotList = hourlyPivotData.Count == 0 ? hourlyPivotData : (from t in hourlyPivotData
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
            MaxDart = mMaxDart;
            MinDart = mMinDart;
            HourlyPivotList = sorttempHourlyPivotList;
            SetHourlySummary(maxHourHash);
            DisplayHourlySummary();
        }
        private void filterLoadData(List<HourlyPivotData> hourlyPivotList)
        {
            if (ProductComboSelectedValue == "Load" && FilterList.Count() != 0 && TypeComboSelectedValue == "PJM RTO Total")
            {
                tempHourlyPivotData = new List<HourlyPivotData>();
                tempHourlyPivotData = hourlyPivotList;
                hourlyPivotList = filteredData(hourlyPivotList, Convert.ToDouble(MinText), Convert.ToDouble(MaxText));
                //FilterList = null;
            }
            else
            {
                hourlyPivotList = tempHourlyPivotData;
            }

            List<HourlyPivotData> sorttempHourlyPivotList = hourlyPivotList.Count == 0 ? hourlyPivotList : (from t in hourlyPivotList
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
            MaxDart = mMaxDart;
            MinDart = mMinDart;
            HourlyPivotList = sorttempHourlyPivotList;
        }
        private List<HourlyPivotData> filteredData(List<HourlyPivotData> tempPivotList, double minText, double maxText)
        {
            List<HourlyPivotData> filteredHourlyPivotList = new List<HourlyPivotData>();
            foreach (HourlyPivotData pivotdata in tempPivotList)
            {
                if (pivotdata.MaxLoad > minText && pivotdata.MaxLoad < maxText)
                {
                    filteredHourlyPivotList.Add(pivotdata);
                }
            }
            return filteredHourlyPivotList;

        }
        private List<string> FilterDatesForLoad(List<HourlyPivotData> tempPivotList, double minText, double maxText)
        {
            List<string> FilterDateList = new List<string>();
            foreach (HourlyPivotData pivotdata in tempPivotList)
            {
                if (pivotdata.MaxLoad > minText && pivotdata.MaxLoad < maxText)
                {
                    FilterDateList.Add(Convert.ToDateTime(pivotdata.Date).ToString("dd-MM-yyyy"));
                }
            }
            return FilterDateList;
        }

        private Dictionary<DateTime, double> CalculateDailyClearedMWs(List<Path> pathList, DateTime startDate, DateTime endDate, out Dictionary<DateTime, double> absClearedMWDict)
        {
            absClearedMWDict = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> clearedMsHash = new Dictionary<DateTime, double>();
            if (PathList != null && PathList.Count > 0)
            {
                for (DateTime tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
                {
                    double mw = 0.0;
                    double absMw = 0.0;
                    foreach (Path path in PathList)
                    {
                        string[] hours = path.AnalysisType.Split('.');
                        foreach (string hour in hours)
                        {
                            DateTime sendDate = tempDate.AddHours(Int32.Parse(hour));
                            string sourceKey = sendDate.ToString() + DBAccess.GetNodeFromName(path.Source, 9).NodeKey; ;
                            string sinkKey = path.Sink == "" ? null : sendDate.ToString() + DBAccess.GetNodeFromName(path.Sink, 9).NodeKey; ;
                            double da = double.NaN;
                            double rt = double.NaN;
                            double dart = double.NaN;
                            double daLmp = double.NaN;

                            #region All Component

                            //#region Dart
                            //if (DartCheckedcomp)
                            //{
                            //    if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                            //    {
                            //        if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                            //        {
                            //            if (LmpCheckedcomp)
                            //            {
                            //                rt = DARTNode.sRTLmpHash[sourceKey].Price;
                            //                if (sinkKey != null)
                            //                {
                            //                    rt = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sRTLmpHash[sourceKey].Price);
                            //                }
                            //            }
                            //            else if (CongCheckedcomp)
                            //            {
                            //                rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                            //                if (sinkKey != null)
                            //                {
                            //                    rt = (DARTNode.sRTLmpHash[sinkKey].Congestion - DARTNode.sRTLmpHash[sourceKey].Congestion);

                            //                }
                            //            }
                            //            else if (LossCheckedcomp)
                            //            {
                            //                rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                            //                if (sinkKey != null)
                            //                {
                            //                    rt = (DARTNode.sRTLmpHash[sinkKey].Loss - DARTNode.sRTLmpHash[sourceKey].Loss);
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            // rt = 0;
                            //        }
                            //    }
                            //    if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                            //    {
                            //        if (DARTNode.sDALmpHash[sourceKey] != null && DARTNode.sDALmpHash[sinkKey] != null)
                            //        {
                            //            if (LmpCheckedcomp)
                            //            {
                            //                da = DARTNode.sDALmpHash[sourceKey].Price;
                            //                if (sinkKey != null)
                            //                {
                            //                    da = (DARTNode.sDALmpHash[sinkKey].Price - DARTNode.sDALmpHash[sourceKey].Price);
                            //                }

                            //            }
                            //            else if (CongCheckedcomp)
                            //            {
                            //                da = DARTNode.sDALmpHash[sourceKey].Congestion;
                            //                daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                            //                if (sinkKey != null)
                            //                {
                            //                    da = (DARTNode.sDALmpHash[sinkKey].Congestion - DARTNode.sDALmpHash[sourceKey].Congestion);
                            //                    daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                            //                }

                            //            }
                            //            else if (LossCheckedcomp)
                            //            {
                            //                da = DARTNode.sDALmpHash[sourceKey].Loss;
                            //                daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                            //                if (sinkKey != null)
                            //                {
                            //                    da = (DARTNode.sDALmpHash[sinkKey].Loss - DARTNode.sDALmpHash[sourceKey].Loss);
                            //                    daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                            //                }

                            //            }
                            //        }
                            //        else
                            //        {
                            //            //  da = 0;
                            //        }
                            //    }
                            //}
                            //#endregion Dart
                            //#region RT
                            //if (RtCheckedcomp)
                            //{
                            //    if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                            //    {
                            //        if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                            //        {
                            //            if (LmpCheckedcomp)
                            //            {
                            //                rt = DARTNode.sRTLmpHash[sourceKey].Price;
                            //                if (sinkKey != null)
                            //                {
                            //                    double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                            //                    rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                            //                }
                            //            }
                            //            else if (CongCheckedcomp)
                            //            {
                            //                rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                            //                if (sinkKey != null)
                            //                {
                            //                    double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                            //                    rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion;
                            //                }
                            //            }
                            //            else if (LossCheckedcomp)
                            //            {
                            //                rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                            //                if (sinkKey != null)
                            //                {
                            //                    double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                            //                    rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss;
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            rt = 0;
                            //        }
                            //    }
                            //    if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                            //    {
                            //        if (DARTNode.sDALmpHash[sourceKey] != null && DARTNode.sDALmpHash[sinkKey] != null)
                            //        {
                            //            if (LmpCheckedcomp)
                            //            {
                            //                if (double.IsNaN(da))
                            //                {
                            //                    da = DARTNode.sDALmpHash[sourceKey].Price;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                            //                        da = sinkDa - DARTNode.sDALmpHash[sourceKey].Price;
                            //                    }
                            //                }
                            //            }
                            //            else if (CongCheckedcomp)
                            //            {
                            //                if (double.IsNaN(da))
                            //                {
                            //                    da = DARTNode.sDALmpHash[sourceKey].Congestion;
                            //                    daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                            //                        da = sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion;
                            //                        daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                            //                    }
                            //                }
                            //            }
                            //            else if (LossCheckedcomp)
                            //            {
                            //                if (double.IsNaN(da))
                            //                {
                            //                    da = DARTNode.sDALmpHash[sourceKey].Loss;
                            //                    daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                            //                        da = sinkDa - DARTNode.sDALmpHash[sourceKey].Loss;
                            //                        daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                            //                    }
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            da = 0;
                            //        }
                            //    }
                            //}
                            //#endregion RT
                            //#region DA
                            //if (DaCheckedcomp)
                            //{
                            //    if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                            //    {
                            //        if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                            //        {
                            //            if (LmpCheckedcomp)
                            //            {
                            //                if (double.IsNaN(rt))
                            //                {
                            //                    rt = DARTNode.sRTLmpHash[sourceKey].Price;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                            //                        rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                            //                    }
                            //                }
                            //            }
                            //            else if (CongCheckedcomp)
                            //            {
                            //                if (double.IsNaN(rt))
                            //                {
                            //                    rt = DARTNode.sRTLmpHash[sourceKey].Congestion;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Congestion)) ? DARTNode.sRTLmpHash[sinkKey].Congestion : 0;
                            //                        rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Congestion;
                            //                    }
                            //                }
                            //            }
                            //            else if (LossCheckedcomp)
                            //            {
                            //                if (double.IsNaN(rt))
                            //                {
                            //                    rt = DARTNode.sRTLmpHash[sourceKey].Loss;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Loss)) ? DARTNode.sRTLmpHash[sinkKey].Loss : 0;
                            //                        rt = sinkRt - DARTNode.sRTLmpHash[sourceKey].Loss;
                            //                    }
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            //   rt = 0;
                            //        }
                            //    }
                            //    if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                            //    {
                            //        if (DARTNode.sDALmpHash[sourceKey] != null && DARTNode.sDALmpHash[sinkKey] != null)
                            //        {
                            //            if (LmpCheckedcomp)
                            //            {
                            //                if (double.IsNaN(da))
                            //                {
                            //                    da = DARTNode.sDALmpHash[sourceKey].Price;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0;
                            //                        da = sinkDa - DARTNode.sDALmpHash[sourceKey].Price;
                            //                    }
                            //                }
                            //            }
                            //            else if (CongCheckedcomp)
                            //            {
                            //                if (double.IsNaN(da))
                            //                {
                            //                    da = DARTNode.sDALmpHash[sourceKey].Congestion;
                            //                    daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Congestion)) ? DARTNode.sDALmpHash[sinkKey].Congestion : 0;
                            //                        da = sinkDa - DARTNode.sDALmpHash[sourceKey].Congestion;
                            //                        daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;
                            //                    }
                            //                }
                            //            }
                            //            else if (LossCheckedcomp)
                            //            {
                            //                if (double.IsNaN(da))
                            //                {
                            //                    da = DARTNode.sDALmpHash[sourceKey].Loss;
                            //                    daLmp = DARTNode.sDALmpHash[sourceKey].Price;
                            //                    if (sinkKey != null)
                            //                    {
                            //                        double sinkDa = (DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Loss)) ? DARTNode.sDALmpHash[sinkKey].Loss : 0;
                            //                        da = sinkDa - DARTNode.sDALmpHash[sourceKey].Loss;
                            //                        daLmp = ((DARTNode.sDALmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sDALmpHash[sinkKey].Price)) ? DARTNode.sDALmpHash[sinkKey].Price : 0) - daLmp;

                            //                    }
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            // da = 0;
                            //        }
                            //    }
                            //}
                            //#endregion DA
                            #endregion All Component


                            #region LMP
                            if (sinkKey != null)
                            {
                                if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && DARTNode.sRTLmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sRTLmpHash[sourceKey] != null && DARTNode.sRTLmpHash[sinkKey] != null)
                                    {
                                        //if (LmpCheckedcomp)
                                        {
                                            rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                            if (sinkKey != null)
                                            {
                                                rt = (DARTNode.sRTLmpHash[sinkKey].Price - DARTNode.sRTLmpHash[sourceKey].Price);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sRTLmpHash[sourceKey] != null)
                                {
                                    //if (LmpCheckedcomp)
                                    {
                                        rt = DARTNode.sRTLmpHash[sourceKey].Price;
                                    }
                                }
                            }
                            if (sinkKey != null)
                            {
                                if (DARTNode.sDALmpHash.ContainsKey(sourceKey) && DARTNode.sDALmpHash.ContainsKey(sinkKey))
                                {
                                    if (DARTNode.sDALmpHash[sourceKey] != null && DARTNode.sDALmpHash[sinkKey] != null)
                                    {
                                        //  if (LmpCheckedcomp)
                                        {
                                            da = DARTNode.sDALmpHash[sourceKey].Price;
                                            if (sinkKey != null)
                                            {
                                                da = (DARTNode.sDALmpHash[sinkKey].Price - DARTNode.sDALmpHash[sourceKey].Price);
                                            }

                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (DARTNode.sDALmpHash[sourceKey] != null)
                                {
                                    //  if (LmpCheckedcomp)
                                    {
                                        da = DARTNode.sDALmpHash[sourceKey].Price;
                                    }
                                }
                            }

                            #endregion LMP
                            if (!double.IsNaN(da) && !double.IsNaN(rt))
                            {
                                dart = rt - da;
                            }
                            string sourceSink = path.Sink == null || path.Sink.Length == 0 ? path.Source : path.Source + "->" + path.Sink;

                            if ((da <= path.Price && path.MW > 0) || (da >= path.Price && path.MW < 0))
                            {
                                mw += path.MW;
                                absMw += Math.Abs(path.MW);
                            }
                        }
                    }
                    clearedMsHash.Add(tempDate, mw);
                    absClearedMWDict.Add(tempDate, absMw);
                }
            }
            return clearedMsHash;
        }

        private void DisplayHourlySummary()
        {
            List<HourlyPivotData> hourlyPivotDataList = new List<HourlyPivotData>();
            if (FilterDayComparisonMaxChecked == true)
            {
                if (mHourlySummaryHash.ContainsKey("Max"))
                {
                    hourlyPivotDataList.Add(mHourlySummaryHash["Max"]);
                }
            }
            if (FilterDayComparisonMinChecked == true)
            {
                if (mHourlySummaryHash.ContainsKey("Min"))
                {
                    hourlyPivotDataList.Add(mHourlySummaryHash["Min"]);
                }
            }
            if (FilterDayComparisonTotalChecked == true)
            {
                if (mHourlySummaryHash.ContainsKey("Total"))
                {
                    hourlyPivotDataList.Add(mHourlySummaryHash["Total"]);
                }
            }
            if (FilterDayComparisonAvgChecked == true)
            {
                if (mHourlySummaryHash.ContainsKey("Average"))
                {
                    hourlyPivotDataList.Add(mHourlySummaryHash["Average"]);
                }
            }
            if (FilterDayComparisonWinPctChecked == true)
            {
                if (mHourlySummaryHash.ContainsKey("Win%"))
                {
                    hourlyPivotDataList.Add(mHourlySummaryHash["Win%"]);
                }
            }

            if (mHourlySummaryHash.ContainsKey("Risk"))
            {
                hourlyPivotDataList.Add(mHourlySummaryHash["Risk"]);
            }

            if (hourlyPivotDataList.Count > 0)
            {
                HourlyPivotListSummary = null;
                HourlyPivotListSummary = hourlyPivotDataList;
            }
        }


        private double? GetRisk(double? min, double? max, double? win)
        {
            if (!min.HasValue || !max.HasValue || !win.HasValue)
                return null;

            double? risk = (win * max) / ((1 - win) * Math.Abs(min.GetValueOrDefault()));
            if (risk.HasValue && double.IsInfinity(risk.Value))
                risk = 100;
            return risk;
        }


        private void SetHourlySummary(Dictionary<string, Summary> maxHourHash)
        {
            Type pivoteType = typeof(HourlyPivotData);
            mHourlySummaryHash = new Dictionary<string, HourlyPivotData>();
            double? totalMin, totalmax, totalwin = null;
            for (int i = 0; i < 6; i++)
            {
                HourlyPivotData hourlyPivotData = new HourlyPivotData();
                double total = 0;
                int count = 0;
                if (i == 0)
                {
                    hourlyPivotData.RowDisplayType = "Max";
                    mHourlySummaryHash.Add("Max", hourlyPivotData);
                }
                if (i == 1)
                {
                    hourlyPivotData.RowDisplayType = "Min";
                    mHourlySummaryHash.Add("Min", hourlyPivotData);
                }
                if (i == 2)
                {
                    hourlyPivotData.RowDisplayType = "Total";
                    mHourlySummaryHash.Add("Total", hourlyPivotData);
                }
                if (i == 3)
                {
                    hourlyPivotData.RowDisplayType = "Average";
                    mHourlySummaryHash.Add("Average", hourlyPivotData);
                }
                if (i == 4)
                {
                    hourlyPivotData.RowDisplayType = "Win%";
                    mHourlySummaryHash.Add("Win%", hourlyPivotData);
                }
                if (i == 5)
                {
                    totalmax = mHourlySummaryHash["Max"].Total;
                    totalMin = mHourlySummaryHash["Min"].Total;
                    totalwin = mHourlySummaryHash["Win%"].Total / 100;
                    hourlyPivotData.RowDisplayType = "Risk";
                    mHourlySummaryHash.Add("Risk", hourlyPivotData);
                    hourlyPivotData.Total = GetRisk(totalMin, totalmax, totalwin);

                    for (int j = 1; j < 25; j++)
                    {
                        if (!maxHourHash.ContainsKey("HE" + j) || i != 5)
                            continue;

                        Summary summary = maxHourHash["HE" + j];
                        double hourlyRisk = GetRisk(summary.Min, summary.Max, (summary.TotalWin / (double)summary.Count)).GetValueOrDefault();
                        pivoteType.GetProperty("HE" + j).SetValue(hourlyPivotData, hourlyRisk);
                    }
                    continue;
                }

                hourlyPivotData.RowDay = "DART";
                if (maxHourHash.ContainsKey("HE1"))
                {
                    count++;
                    Summary summary = maxHourHash["HE1"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE1 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE1 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE1 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE1 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE1 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE2"))
                {
                    count++;
                    Summary summary = maxHourHash["HE2"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE2 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE2 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE2 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE2 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE2 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE3"))
                {
                    count++;
                    Summary summary = maxHourHash["HE3"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE3 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE3 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE3 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE3 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE3 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE4"))
                {
                    count++;
                    Summary summary = maxHourHash["HE4"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE4 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE4 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE4 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE4 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE4 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE5"))
                {
                    count++;
                    Summary summary = maxHourHash["HE5"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE5 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE5 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE5 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE5 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE5 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE6"))
                {
                    count++;
                    Summary summary = maxHourHash["HE6"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE6 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE6 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE6 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE6 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE6 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE7"))
                {
                    count++;
                    Summary summary = maxHourHash["HE7"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE7 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE7 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE7 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE7 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE7 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE8"))
                {
                    count++;
                    Summary summary = maxHourHash["HE8"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE8 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE8 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE8 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE8 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE8 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE9"))
                {
                    count++;
                    Summary summary = maxHourHash["HE9"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE9 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE9 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE9 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE9 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE9 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE10"))
                {
                    count++;
                    Summary summary = maxHourHash["HE10"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE10 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE10 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE10 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE10 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE10 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE11"))
                {
                    count++;
                    Summary summary = maxHourHash["HE11"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE11 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE11 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE11 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE11 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE11 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE12"))
                {
                    count++;
                    Summary summary = maxHourHash["HE12"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE12 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE12 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE12 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE12 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE12 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE13"))
                {
                    count++;
                    Summary summary = maxHourHash["HE13"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE13 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE13 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE13 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE13 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE13 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE14"))
                {
                    count++;
                    Summary summary = maxHourHash["HE14"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE14 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE14 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE14 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE14 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE14 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE15"))
                {
                    count++;
                    Summary summary = maxHourHash["HE15"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE15 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE15 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE15 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE15 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE15 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE16"))
                {
                    count++;
                    Summary summary = maxHourHash["HE16"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE16 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE16 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE16 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE16 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE16 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE17"))
                {
                    count++;
                    Summary summary = maxHourHash["HE17"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE17 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE17 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE17 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE17 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE17 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE18"))
                {
                    count++;
                    Summary summary = maxHourHash["HE18"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE18 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE18 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE18 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE18 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE18 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE19"))
                {
                    count++;
                    Summary summary = maxHourHash["HE19"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE19 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE19 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE19 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE19 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE19 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE20"))
                {
                    count++;
                    Summary summary = maxHourHash["HE20"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE20 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE20 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE20 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE20 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE20 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE21"))
                {
                    count++;
                    Summary summary = maxHourHash["HE21"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE21 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE21 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE21 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE21 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE21 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE22"))
                {
                    count++;
                    Summary summary = maxHourHash["HE22"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE22 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE22 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE22 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE22 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE22 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE23"))
                {
                    count++;
                    Summary summary = maxHourHash["HE23"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE23 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE23 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE23 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE23 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE23 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (maxHourHash.ContainsKey("HE24"))
                {
                    count++;
                    Summary summary = maxHourHash["HE24"];
                    if (i == 0)
                    {
                        hourlyPivotData.HE24 = summary.Max;
                        total += summary.Max;
                    }
                    if (i == 1)
                    {
                        hourlyPivotData.HE24 = summary.Min;
                        total += summary.Min;
                    }
                    if (i == 2)
                    {
                        hourlyPivotData.HE24 = summary.Total;
                        total += summary.Total;
                    }
                    if (i == 3 && summary.Count > 0)
                    {
                        hourlyPivotData.HE24 = summary.Total / (double)summary.Count;
                        total += (summary.Total / (double)summary.Count);
                    }
                    if (i == 4 && summary.Count > 0)
                    {
                        hourlyPivotData.HE24 = (summary.TotalWin / (double)summary.Count) * 100;
                        total += ((summary.TotalWin / (double)summary.Count) * 100);
                    }
                }
                if (i < 3)
                {
                    hourlyPivotData.Total = total;
                }
                if (count > 0)
                {
                    hourlyPivotData.Average = total / (double)count;
                }
            }
        }

        private PlotModel CreatePathPlotModelUpper(Dictionary<string, PNL> pnlHash)
        {
            mBidId = 0;
            string title = ((DateTime)SelectedHourlyPivot.Date).ToShortDateString();
            string Rowtype = ((string)SelectedHourlyPivot.RowType).ToString();
            var plotModel1 = new PlotModel { Title = title };//title
            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis() //AxisPosition.Left
            {
                Key = "Y1Axis",
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
                StringFormat = "$0,00",
                EndPosition = 1
            });
            plotModel1.Axes.Add(new LinearAxis()//AxisPosition.Right
            {
                Key = "Y2Axis",
                Position = AxisPosition.Right,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StartPosition = 0,
                StringFormat = "$0,00",
                EndPosition = 1
            });
            // X axis
            var dataItemValues = new Collection<Item>();
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()//AxisPosition.Bottom
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
            string colTitle = Rowtype;// "DART";
            colSeries1 = new BarSeries()
            {
                Title = colTitle,
                YAxisKey = "XAxisCategory",
                XAxisKey = "Y1Axis",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,
            };
            List<string> pnlKeys = pnlHash.Keys.ToList<string>();
            Dictionary<double, List<string>> sortPnlHash = new Dictionary<double, List<string>>();
            foreach (string pnlKey in pnlKeys)
            {
                PNL sortPnl = pnlHash[pnlKey];
                List<string> sortList = new List<string>();
                if (Rowtype == "DART")
                {
                    if (sortPnlHash.ContainsKey(sortPnl.DART))
                    {
                        sortList = sortPnlHash[sortPnl.DART];
                        sortPnlHash.Remove(sortPnl.DART);
                    }
                    sortList.Add(sortPnl.SourceSink);
                    sortPnlHash.Add(sortPnl.DART, sortList);
                }
                if (Rowtype == "RT")
                {
                    if (sortPnlHash.ContainsKey(sortPnl.RT))
                    {
                        sortList = sortPnlHash[sortPnl.RT];
                        sortPnlHash.Remove(sortPnl.RT);
                    }
                    sortList.Add(sortPnl.SourceSink);
                    sortPnlHash.Add(sortPnl.RT, sortList);
                }
                if (Rowtype == "DA")
                {
                    if (sortPnlHash.ContainsKey(sortPnl.DA))
                    {
                        sortList = sortPnlHash[sortPnl.DA];
                        sortPnlHash.Remove(sortPnl.DA);
                    }
                    sortList.Add(sortPnl.SourceSink);
                    sortPnlHash.Add(sortPnl.DA, sortList);
                }
            }
            List<double> sortPnlKeys = sortPnlHash.Keys.ToList<double>();
            sortPnlKeys.Sort();
            foreach (double pnlKey in sortPnlKeys)
            {
                List<string> nameList = sortPnlHash[pnlKey];
                foreach (string name in nameList)
                {
                    categoryAxis.Labels.Add(name);
                    var colItem1 = new BarItem(Math.Round(pnlKey, 0), mBidId);
                    colSeries1.Items.Add(colItem1);
                    mBidId++;
                }
            }
            plotModel1.Series.Add(colSeries1);
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,

            };
            plotModel1.Legends.Add(l);

            return plotModel1;
        }

        private PlotModel CreatePlotModelUpper(List<Node> nodeList)
        {

            var plotModel1 = new PlotModel()
            {
                Title = "Portfolio",
            };

            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis()//(AxisPosition.Left
            {
                Key = "Y1Axis",
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
                StringFormat = "$0,00",
                EndPosition = 1
            });
            plotModel1.Axes.Add(new LinearAxis()//AxisPosition.Right
            {
                Key = "Y2Axis",
                Position = AxisPosition.Right,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.Outside,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0,
                MinimumPadding = 0,
                StringFormat = "$0,00",
                StartPosition = 0,
                EndPosition = 1
            });
            // X axis
            var dataItemValues = new Collection<Item>(); // use with non DateTime x axis
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()//AxisPosition.Bottom
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
            //plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 70);
            var colSeries1 = new BarSeries();
            string colTitle = nodeList[nodeList.Count - 1].NodeName;
            colSeries1 = new BarSeries()
            {
                Title = colTitle,

                YAxisKey = "XAxisCategory",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,
                XAxisKey = "Y2Axis",
                //TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
            };
            for (int i = 0; i < nodeList[nodeList.Count - 1].TimePriceList.Count; i++)
            {
                categoryAxis.Labels.Add(nodeList[nodeList.Count - 1].TimePriceList[i].MarketTime.ToString("M/d/yy 'HE'H"));
                var colItem1 = new BarItem(Math.Round(nodeList[nodeList.Count - 1].TimePriceList[i].Price, 0), i);
                colSeries1.Items.Add(colItem1);
            }
            //if (colSeries1.Items.Count > 0)
            //{
            //    colSeries1.TrackerFormatString = "$0,00";
            //}
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
                    TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
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

        private PlotModel CreatePlotModelLower(List<Node> nodeList)
        {
            var plotModel1 = new PlotModel();
            var plotModel12 = new Legend();
            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis()//AxisPosition.Left
            {
                Key = "Y1AxisB",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.None,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MaximumPadding = 0.05,
                MinimumPadding = 0,
                StartPosition = 0,
                StringFormat = "$0,00",
                EndPosition = 0.995,
                TextColor = OxyColors.Transparent
            });
            // this Y2 axis (which is hidden, transparent) is only included so that the top and bottom charts line up nicely
            plotModel1.Axes.Add(new LinearAxis()//AxisPosition.Right
            {
                Key = "Y2AxisB",
                Position = AxisPosition.Right,
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
            });
            // X axis
            CategoryAxis categoryAxis = new CategoryAxis();
            categoryAxis = new CategoryAxis()//AxisPosition.Bottom
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
            //  plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 2);
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
                TrackerFormatString = "{0}\n{2:M/d/yy H:mm}\n{4:$0,00}",
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
                //areaSeries1.Points.Add(new DataPoint() { X = i, Y = accumulator });
                //areaSeries1.Points2.Add(new DataPoint() { X = i, Y = 0 });
                areaSeries1.Points.Add(new DataPoint(i, accumulator));
                areaSeries1.Points2.Add(new DataPoint(i, 0));
            }
            areaSeries1.Title = nodeList[nodeList.Count - 1].NodeName;
            plotModel1.Series.Add(areaSeries1);
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
                IsLegendVisible = false,
            };
            plotModel1.Legends.Add(l);

            return plotModel1;
        }

        private void CreateDateTimeAxis(out DateTimeAxis dtAxis, out double columnWidthFromDateTime)
        {
            dtAxis = new DateTimeAxis();
            columnWidthFromDateTime = 0;
            var c = OxyColors.DarkBlue;
            if (SelectedPeriodType == PeriodType.Daily)
            {
                dtAxis = new DateTimeAxis() //StartDate.AddDays(-0.5), EndDate.AddDays(0.5), AxisPosition.Bottom, null, null, DateTimeIntervalType.Days
                {

                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromAColor(20, c),
                    Angle = 90,
                    StringFormat = "M/d/yy",
                    IntervalType = DateTimeIntervalType.Days,
                    /* MinorIntervalType = DateTimeIntervalType.Auto,  */
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
            else
            {
                dtAxis = new DateTimeAxis()//StartDate, EndDate.Date.AddDays(1), AxisPosition.Bottom, null, null, DateTimeIntervalType.Hours
                {
                    Position = AxisPosition.Bottom,
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromAColor(20, c),
                    Angle = 90,
                    StringFormat = "M/d/yy H:mm",
                    IntervalType = DateTimeIntervalType.Hours,
                    MinorIntervalType = DateTimeIntervalType.Hours,
                    IsZoomEnabled = true,
                    MaximumPadding = 0,
                    MinimumPadding = 0,
                    StartPosition = 0.005,
                    EndPosition = 0.995,
                    TickStyle = TickStyle.Outside,
                    AxisTickToLabelDistance = 2,
                    FontSize = 10
                };
                columnWidthFromDateTime = (DateTimeAxis.ToDouble(DateTime.Now.AddHours(1)) - DateTimeAxis.ToDouble(DateTime.Now)) * 0.3;
            }
        }

        private void ConvertDailyValues(List<Node> asBidHourlyNodeList, List<Node> mustTakeHourylyNodeList,
                                            out List<Node> asBidSendHourlyNodeList, out List<Node> mustTakeSendHourylyNodeList)
        {
            asBidSendHourlyNodeList = new List<Node>();
            mustTakeSendHourylyNodeList = new List<Node>();
            for (int k = 0; k < 2; k++)
            {
                List<Node> hourlyNodeList = k == 0 ? asBidHourlyNodeList : mustTakeHourylyNodeList;
                List<Node> dailyNodeList = new List<Node>();
                foreach (Node node in hourlyNodeList)
                {
                    dailyNodeList.Add(new Node(node));
                }
                List<DateTime> distinctDays = hourlyNodeList[0].TimePriceList.Select(n => n.MarketTime.AddMinutes(-1).Date).Distinct().OrderBy(n => n.Date).ToList();
                int numDays = distinctDays.Count;
                int winner = 0;
                double bidMax = double.MinValue;
                double bidMin = double.MaxValue;
                DateTime bidRiskDate = DateTime.Today;
                DateTime bidWinDate = DateTime.Today;
                double tempDrawDown = 0;
                double drawDown = double.MaxValue;
                bool setEndDate = false;
                DateTime startDrawdownDate = DateTime.Today;
                DateTime endDrawdownDate = DateTime.Today;
                DateTime tempDrawDownDate = DateTime.Today;
                for (int j = 0; j < dailyNodeList.Count; j++)
                {
                    List<TimePrice> dailyPriceList = new List<TimePrice>();
                    for (int i = 0; i < numDays; i++)
                    {
                        double accumulator = 0;
                        foreach (TimePrice tp in dailyNodeList[j].TimePriceList)
                        {
                            DateTime tempDate = tp.MarketTime.Hour == 0 ? tp.MarketTime.AddDays(-1) : tp.MarketTime;
                            if (!tp.Price.Equals(double.NaN) && tempDate.Date == distinctDays[i].Date)
                            {
                                accumulator += tp.Price;
                            }
                        }
                        if (j == dailyNodeList.Count - 1)
                        {
                            if (accumulator > 0)
                            {
                                winner++;
                                if (setEndDate)
                                {
                                    endDrawdownDate = distinctDays[i].Date;
                                    setEndDate = false;
                                }
                                tempDrawDown = 0;
                                setEndDate = false;
                            }
                            else
                            {
                                if (tempDrawDown == 0)
                                {
                                    tempDrawDownDate = distinctDays[i].Date;
                                }
                                tempDrawDown += accumulator;
                            }
                            if (bidMax < accumulator)
                            {
                                bidMax = accumulator;
                                bidWinDate = distinctDays[i].Date;
                            }
                            if (bidMin > accumulator)
                            {
                                bidMin = accumulator;
                                bidRiskDate = distinctDays[i].Date;
                            }
                            if (tempDrawDown < drawDown)
                            {
                                drawDown = tempDrawDown;
                                startDrawdownDate = tempDrawDownDate;
                                setEndDate = true;
                            }
                        }
                        dailyPriceList.Add(new TimePrice() { MarketTime = distinctDays[i].Date, Price = accumulator });
                    }
                    dailyNodeList[j].TimePriceList = dailyPriceList;
                }
                if (k == 0)
                {
                    asBidSendHourlyNodeList = dailyNodeList;
                    AsBidWinPerText = null;
                    AsBidWinPerText = numDays == 0 ? "0.00%" : ((double)winner / (double)numDays).ToString("0.00%");
                    AsBidRiskReward = null;
                    AsBidRiskReward = bidMax == double.MinValue || bidMin == double.MaxValue || bidMin == 0 ? "0" :
                                        (bidMax / Math.Abs(bidMin)).ToString("#,##0.00;(#,##0.00)");
                    AsBidRisk = null;
                    AsBidRisk = bidMin == double.MaxValue ? "0" : bidMin.ToString("#,##0;(#,##0)");
                    AsBidRiskDate = null;
                    AsBidRiskDate = bidRiskDate == DateTime.Today ? "" : bidRiskDate.ToString("MM/dd/yy");
                    AsBidWin = bidMax == double.MinValue ? "0" : bidMax.ToString("#,##0;(#,##0)");
                    AsBidWinDate = null;
                    AsBidWinDate = bidWinDate == DateTime.Today ? "" : bidWinDate.ToString("MM/dd/yy");
                    AsBidMaxDrawDown = null;
                    AsBidMaxDrawDown = drawDown == double.MaxValue ? "0" : drawDown.ToString("#,##0;(#,##0)");
                    if (endDrawdownDate < startDrawdownDate)
                    {
                        endDrawdownDate = DateTime.Today;
                    }
                    AsBidMaxDrawDownDate = startDrawdownDate == DateTime.Today && endDrawdownDate == DateTime.Today ? "" :
                                                    startDrawdownDate.ToString("MM/dd/yy") + " - " + endDrawdownDate.ToString("MM/dd/yy");
                }
                else
                {
                    mustTakeSendHourylyNodeList = dailyNodeList;
                    MustTakeWinPerText = null;
                    MustTakeWinPerText = numDays == 0 ? "0.00%" : ((double)winner / (double)numDays).ToString("0.00%");
                    MustTakeRiskReward = null;
                    MustTakeRiskReward = bidMax == double.MinValue || bidMin == double.MaxValue || bidMin == 0 ? "0" :
                                        (bidMax / Math.Abs(bidMin)).ToString("#,##0.00;(#,##0.00)");
                    MustTakeRisk = null;
                    MustTakeRisk = bidMin == double.MaxValue ? "0" : bidMin.ToString("#,##0;(#,##0)");
                    MustTakeRiskDate = null;
                    MustTakeRiskDate = bidRiskDate == DateTime.Today ? "" : bidRiskDate.ToString("MM/dd/yy");
                    MustTakeWin = bidMax == double.MinValue ? "0" : bidMax.ToString("#,##0;(#,##0)");
                    MustTakeWinDate = null;
                    MustTakeWinDate = bidWinDate == DateTime.Today ? "" : bidWinDate.ToString("MM/dd/yy");
                    MustTakeMaxDrawDown = null;
                    MustTakeMaxDrawDown = drawDown == double.MaxValue ? "0" : drawDown.ToString("#,##0;(#,##0)");
                    if (endDrawdownDate < startDrawdownDate)
                    {
                        endDrawdownDate = DateTime.Today;
                    }
                    MustTakeMaxDrawDownDate = startDrawdownDate == DateTime.Today && endDrawdownDate == DateTime.Today ? "" :
                                                    startDrawdownDate.ToString("MM/dd/yy") + " - " + endDrawdownDate.ToString("MM/dd/yy");
                }
            }
        }

        private void SetMarket()
        {
            List<string> marketList = new List<string>();
            {
                marketList.Add("ERCOT");
                marketList.Add("ERCOT External");
                ISOMarketList = marketList;

            }
            {
                MarketComboSelectedValue = "ERCOT";
                ISOMarketList = marketList;
            }
        }

        private void UpdateMWForPaths()
        {
            if (PathList == null || MwMultiplierValue <= 0 || MwMultiplierValue > 2)
                return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                PathList.ForEach(a =>
                {
                    try
                    {
                        if (a.Status.ToLower() != "valid")
                        {
                            a.MW = a.MW * MwMultiplierValue;
                            RowsUpdated(a, "MW", a.MW.ToString());
                        }
                    }
                    catch
                    {
                    }
                });

                foreach (var item in PathList)
                    item.FireUpdate();

                Mouse.OverrideCursor = Cursors.Arrow;
                //Retrieve();
            }
            catch
            {
            }
        }

        private void CreateRequestFile()
        {
            SubmitSaveFile(true);
        }

        private void SetPortfolioList()
        {
            DateTime sendDate = DateTime.Parse(PortfolioDate.ToShortDateString());
            string product = UptosChecked ? "EES/PTP" : "Virtual";
            List<Portfolio> portfolioList = null;
            //portfolioList = DBAccess.GetPortfolio(sendDate, mUser, product, MarketComboSelectedValue);
            //PortfolioComboList = null;
            //PortfolioComboList = portfolioList;
            if (MarketComboSelectedValue == "ERCOT External")
            {
                portfolioList = _dataService.GetExternalPortfolio(PortfolioDate, PortfolioDate.AddDays(1));
            }
            else
            {
                portfolioList = DBAccess.GetPortfolio(sendDate, _User, product, MarketComboSelectedValue);

            }
            PortfolioComboList = null;
            PortfolioComboList = portfolioList;
        }

        private void SetUserPortfolioList()
        {
            TraderPortfolioComboList = null;
            string product = UptosChecked ? "EES/PTP" : "Virtual";
            List<Portfolio> portfolioList = new List<Portfolio>();
            DBAccess.GetUserPortfolioList(
                (pkList, error) =>
                {
                    portfolioList = pkList;
                },
                _User, product, MarketComboSelectedValue);
            TraderPortfolioComboList = null;
            TraderPortfolioComboList = portfolioList;
        }

        private List<Exposure> GetCalculatedConstraints(bool IsDollar, DateTime similarDate,
          Dictionary<int, Dictionary<int, Sensitivity>> nodeSensitivityHash, double maxLoad, bool isXml)
        {
            Dictionary<int, Exposure> exposureHash = new Dictionary<int, Exposure>();
            foreach (Path path in PathList)
            {
                int sourceNode = DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey;
                int sinkNode = path.Sink == null ? 0 : DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey;

                if (nodeSensitivityHash.ContainsKey(sourceNode) && (sinkNode == 0 || nodeSensitivityHash.ContainsKey(sinkNode)))
                {
                    Dictionary<int, Sensitivity> sourceSensitivityHash = nodeSensitivityHash[sourceNode];
                    Dictionary<int, Sensitivity> sinkSensitivityHash = sinkNode == 0 ? null : nodeSensitivityHash[sinkNode];
                    List<int> sourceSensitivityKeys = sourceSensitivityHash.Keys.ToList<int>();
                    foreach (int sourceSensitivityKey in sourceSensitivityKeys)
                    {
                        if (sinkNode == 0 || sinkSensitivityHash.ContainsKey(sourceSensitivityKey))
                        {
                            Sensitivity sourceSensitivity = sourceSensitivityHash[sourceSensitivityKey];
                            Sensitivity sinkSensitivity = sinkNode == 0 ? null : sinkSensitivityHash[sourceSensitivityKey];
                            double diff = sinkNode == 0 ? sourceSensitivity.SensitivityValue : sinkSensitivity.SensitivityValue - sourceSensitivity.SensitivityValue;
                            if (IsDollar)
                            {
                                diff *= sourceSensitivity.DollarImpact;
                            }
                            Exposure exposure = new Exposure();
                            exposure.Constraint = sourceSensitivity.Constraint;
                            exposure.Contingency = sourceSensitivity.Contingency;
                            exposure.ID = sourceSensitivity.ID;
                            exposure.Shift = sourceSensitivity.ShiftFactor;
                            exposure.RiskType = sourceSensitivity.RiskType;
                            List<Exposure> pathDetailList = new List<Exposure>();
                            if (mPathDetailHash.ContainsKey(exposure.ID))
                            {
                                pathDetailList = mPathDetailHash[exposure.ID];
                                mPathDetailHash.Remove(exposure.ID);
                            }
                            Exposure pathExposure = null;
                            if (exposureHash.ContainsKey(sourceSensitivity.ID))
                            {
                                exposure = exposureHash[sourceSensitivity.ID];
                                exposureHash.Remove(sourceSensitivity.ID);
                            }
                            string[] tokens = path.AnalysisType.Split('.');
                            double total = 0;
                            double pathTotal = 0;
                            if (DARTNode.sDAHash.Count == 0)
                            {
                                MessageBox.Show("Price Server down.");
                                return null;
                            }

                            foreach (string token in tokens)
                            {
                                if (maxLoad < 132000)
                                {
                                    if (AsBidChecked)
                                    {
                                        int hourValue = Int32.Parse(token);
                                        DateTime sendDate = hourValue == 24 ? similarDate.Date.AddDays(1) :
                                                                                similarDate.Date.AddHours(hourValue);
                                        string sourceKey = sendDate.ToString() + sourceNode;
                                        string sinkKey = sinkNode == 0 ? null : sendDate.ToString() + sinkNode;
                                        double da = double.NaN;
                                        if (sinkKey != null)
                                        {
                                            if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey) &&
                                                !double.IsNaN(DARTNode.sDAHash[sinkKey]) && !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                                            {
                                                da = (DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey]);
                                            }
                                        }
                                        else
                                        {
                                            if (DARTNode.sDAHash.ContainsKey(sourceKey) &&
                                                !double.IsNaN(DARTNode.sDAHash[sourceKey]))
                                            {
                                                da = DARTNode.sDAHash[sourceKey];
                                            }
                                        }
                                        if (path.Price < da || (path.MW < 0 && path.Price > da))
                                        {
                                            continue;
                                        }
                                    }
                                }

                                if (pathExposure == null)
                                {
                                    pathExposure = new Exposure();
                                    pathExposure.ID = exposure.ID;
                                    pathExposure.Constraint = path.Source;
                                    pathExposure.Contingency = path.Sink;
                                }
                                if (token == "1")
                                {
                                    if (exposure.HE1 == null)
                                    {
                                        exposure.HE1 = 0;
                                    }
                                    if (pathExposure.HE1 == null)
                                    {
                                        pathExposure.HE1 = 0;
                                    }
                                    pathExposure.HE1 = path.MW * diff;
                                    exposure.HE1 += path.MW * diff;
                                    total += (double)exposure.HE1;
                                    pathTotal += (double)pathExposure.HE1;
                                }
                                if (token == "2")
                                {
                                    if (exposure.HE2 == null)
                                    {
                                        exposure.HE2 = 0;
                                    }
                                    if (pathExposure.HE2 == null)
                                    {
                                        pathExposure.HE2 = 0;
                                    }
                                    pathExposure.HE2 = path.MW * diff;
                                    exposure.HE2 += path.MW * diff;
                                    total += (double)exposure.HE2;
                                    pathTotal += (double)pathExposure.HE2;
                                }
                                if (token == "3")
                                {
                                    if (exposure.HE3 == null)
                                    {
                                        exposure.HE3 = 0;
                                    }
                                    if (pathExposure.HE3 == null)
                                    {
                                        pathExposure.HE3 = 0;
                                    }
                                    pathExposure.HE3 = path.MW * diff;
                                    exposure.HE3 += path.MW * diff;
                                    total += (double)exposure.HE3;
                                    pathTotal += (double)pathExposure.HE3;
                                }
                                if (token == "4")
                                {
                                    if (exposure.HE4 == null)
                                    {
                                        exposure.HE4 = 0;
                                    }
                                    if (pathExposure.HE4 == null)
                                    {
                                        pathExposure.HE4 = 0;
                                    }
                                    pathExposure.HE4 = path.MW * diff;
                                    exposure.HE4 += path.MW * diff;
                                    total += (double)exposure.HE4;
                                    pathTotal += (double)pathExposure.HE4;
                                }
                                if (token == "5")
                                {
                                    if (exposure.HE5 == null)
                                    {
                                        exposure.HE5 = 0;
                                    }
                                    if (pathExposure.HE5 == null)
                                    {
                                        pathExposure.HE5 = 0;
                                    }
                                    pathExposure.HE5 = path.MW * diff;
                                    exposure.HE5 += path.MW * diff;
                                    total += (double)exposure.HE5;
                                    pathTotal += (double)pathExposure.HE5;
                                }
                                if (token == "6")
                                {
                                    if (exposure.HE6 == null)
                                    {
                                        exposure.HE6 = 0;
                                    }
                                    if (pathExposure.HE6 == null)
                                    {
                                        pathExposure.HE6 = 0;
                                    }
                                    pathExposure.HE6 = path.MW * diff;
                                    exposure.HE6 += path.MW * diff;
                                    total += (double)exposure.HE6;
                                    pathTotal += (double)pathExposure.HE6;
                                }
                                if (token == "7")
                                {
                                    if (exposure.HE7 == null)
                                    {
                                        exposure.HE7 = 0;
                                    }
                                    if (pathExposure.HE7 == null)
                                    {
                                        pathExposure.HE7 = 0;
                                    }
                                    pathExposure.HE7 = path.MW * diff;
                                    exposure.HE7 += path.MW * diff;
                                    total += (double)exposure.HE7;
                                    pathTotal += (double)pathExposure.HE7;
                                }
                                if (token == "8")
                                {
                                    if (exposure.HE8 == null)
                                    {
                                        exposure.HE8 = 0;
                                    }
                                    if (pathExposure.HE8 == null)
                                    {
                                        pathExposure.HE8 = 0;
                                    }
                                    pathExposure.HE8 = path.MW * diff;
                                    exposure.HE8 += path.MW * diff;
                                    total += (double)exposure.HE8;
                                    pathTotal += (double)pathExposure.HE8;
                                }
                                if (token == "9")
                                {
                                    if (exposure.HE9 == null)
                                    {
                                        exposure.HE9 = 0;
                                    }
                                    if (pathExposure.HE9 == null)
                                    {
                                        pathExposure.HE9 = 0;
                                    }
                                    pathExposure.HE9 = path.MW * diff;
                                    exposure.HE9 += path.MW * diff;
                                    total += (double)exposure.HE9;
                                    pathTotal += (double)pathExposure.HE9;
                                }
                                if (token == "10")
                                {
                                    if (exposure.HE10 == null)
                                    {
                                        exposure.HE10 = 0;
                                    }
                                    if (pathExposure.HE10 == null)
                                    {
                                        pathExposure.HE10 = 0;
                                    }
                                    pathExposure.HE10 = path.MW * diff;
                                    exposure.HE10 += path.MW * diff;
                                    total += (double)exposure.HE10;
                                    pathTotal += (double)pathExposure.HE10;
                                }
                                if (token == "11")
                                {
                                    if (exposure.HE11 == null)
                                    {
                                        exposure.HE11 = 0;
                                    }
                                    if (pathExposure.HE11 == null)
                                    {
                                        pathExposure.HE11 = 0;
                                    }
                                    pathExposure.HE11 = path.MW * diff;
                                    exposure.HE11 += path.MW * diff;
                                    total += (double)exposure.HE11;
                                    pathTotal += (double)pathExposure.HE11;
                                }
                                if (token == "12")
                                {
                                    if (exposure.HE12 == null)
                                    {
                                        exposure.HE12 = 0;
                                    }
                                    if (pathExposure.HE12 == null)
                                    {
                                        pathExposure.HE12 = 0;
                                    }
                                    pathExposure.HE12 = path.MW * diff;
                                    exposure.HE12 += path.MW * diff;
                                    total += (double)exposure.HE12;
                                    pathTotal += (double)pathExposure.HE12;
                                }
                                if (token == "13")
                                {
                                    if (exposure.HE13 == null)
                                    {
                                        exposure.HE13 = 0;
                                    }
                                    if (pathExposure.HE13 == null)
                                    {
                                        pathExposure.HE13 = 0;
                                    }
                                    pathExposure.HE13 = path.MW * diff;
                                    exposure.HE13 += path.MW * diff;
                                    total += (double)exposure.HE13;
                                    pathTotal += (double)pathExposure.HE13;
                                }
                                if (token == "14")
                                {
                                    if (exposure.HE14 == null)
                                    {
                                        exposure.HE14 = 0;
                                    }
                                    if (pathExposure.HE14 == null)
                                    {
                                        pathExposure.HE14 = 0;
                                    }
                                    pathExposure.HE14 = path.MW * diff;
                                    exposure.HE14 += path.MW * diff;
                                    total += (double)exposure.HE14;
                                    pathTotal += (double)pathExposure.HE14;
                                }
                                if (token == "15")
                                {
                                    if (exposure.HE15 == null)
                                    {
                                        exposure.HE15 = 0;
                                    }
                                    if (pathExposure.HE15 == null)
                                    {
                                        pathExposure.HE15 = 0;
                                    }
                                    pathExposure.HE15 = path.MW * diff;
                                    exposure.HE15 += path.MW * diff;
                                    total += (double)exposure.HE15;
                                    pathTotal += (double)pathExposure.HE15;
                                }
                                if (token == "16")
                                {
                                    if (exposure.HE16 == null)
                                    {
                                        exposure.HE16 = 0;
                                    }
                                    if (pathExposure.HE16 == null)
                                    {
                                        pathExposure.HE16 = 0;
                                    }
                                    pathExposure.HE16 = path.MW * diff;
                                    exposure.HE16 += path.MW * diff;
                                    total += (double)exposure.HE16;
                                    pathTotal += (double)pathExposure.HE16;
                                }
                                if (token == "17")
                                {
                                    if (exposure.HE17 == null)
                                    {
                                        exposure.HE17 = 0;
                                    }
                                    if (pathExposure.HE17 == null)
                                    {
                                        pathExposure.HE17 = 0;
                                    }
                                    pathExposure.HE17 = path.MW * diff;
                                    exposure.HE17 += path.MW * diff;
                                    total += (double)exposure.HE17;
                                    pathTotal += (double)pathExposure.HE17;
                                }
                                if (token == "18")
                                {
                                    if (exposure.HE18 == null)
                                    {
                                        exposure.HE18 = 0;
                                    }
                                    if (pathExposure.HE18 == null)
                                    {
                                        pathExposure.HE18 = 0;
                                    }
                                    pathExposure.HE18 = path.MW * diff;
                                    exposure.HE18 += path.MW * diff;
                                    total += (double)exposure.HE18;
                                    pathTotal += (double)pathExposure.HE18;
                                }
                                if (token == "19")
                                {
                                    if (exposure.HE19 == null)
                                    {
                                        exposure.HE19 = 0;
                                    }
                                    if (pathExposure.HE19 == null)
                                    {
                                        pathExposure.HE19 = 0;
                                    }
                                    pathExposure.HE19 = path.MW * diff;
                                    exposure.HE19 += path.MW * diff;
                                    total += (double)exposure.HE19;
                                    pathTotal += (double)pathExposure.HE19;
                                }
                                if (token == "20")
                                {
                                    if (exposure.HE20 == null)
                                    {
                                        exposure.HE20 = 0;
                                    }
                                    pathExposure.HE20 = path.MW * diff;
                                    exposure.HE20 += path.MW * diff;
                                    total += (double)exposure.HE20;
                                    pathTotal += (double)pathExposure.HE20;
                                }
                                if (token == "21")
                                {
                                    if (exposure.HE21 == null)
                                    {
                                        exposure.HE21 = 0;
                                    }
                                    if (pathExposure.HE21 == null)
                                    {
                                        pathExposure.HE21 = 0;
                                    }
                                    pathExposure.HE21 = path.MW * diff;
                                    exposure.HE21 += path.MW * diff;
                                    total += (double)exposure.HE21;
                                    pathTotal += (double)pathExposure.HE21;
                                }
                                if (token == "22")
                                {
                                    if (exposure.HE22 == null)
                                    {
                                        exposure.HE22 = 0;
                                    }
                                    if (pathExposure.HE22 == null)
                                    {
                                        pathExposure.HE22 = 0;
                                    }
                                    pathExposure.HE22 = path.MW * diff;
                                    exposure.HE22 += path.MW * diff;
                                    total += (double)exposure.HE22;
                                    pathTotal += (double)pathExposure.HE22;
                                }
                                if (token == "23")
                                {
                                    if (exposure.HE23 == null)
                                    {
                                        exposure.HE23 = 0;
                                    }
                                    if (pathExposure.HE23 == null)
                                    {
                                        pathExposure.HE23 = 0;
                                    }
                                    pathExposure.HE23 = path.MW * diff;
                                    exposure.HE23 += path.MW * diff;
                                    total += (double)exposure.HE23;
                                    pathTotal += (double)pathExposure.HE23;
                                }
                                if (token == "24")
                                {
                                    if (exposure.HE24 == null)
                                    {
                                        exposure.HE24 = 0;
                                    }
                                    if (pathExposure.HE24 == null)
                                    {
                                        pathExposure.HE24 = 0;
                                    }
                                    pathExposure.HE24 = path.MW * diff;
                                    exposure.HE24 += path.MW * diff;
                                    total += (double)exposure.HE24;
                                    pathTotal += (double)pathExposure.HE24;
                                }
                            }
                            if (pathTotal != 0)
                            {
                                pathExposure.Sum = (double)pathTotal;
                            }
                            exposure.IsShiftEmpty = true;
                            exposureHash.Add(sourceSensitivity.ID, exposure);
                            if (pathExposure != null)
                            {
                                pathDetailList.Add(pathExposure);
                            }
                            mPathDetailHash.Add(exposure.ID, pathDetailList);
                        }
                    }
                }
            }
            List<Sensitivity> ConstraintNotExistList = _dataService.GetConstraintNotExist(SubmitDate);
            foreach (Sensitivity item in ConstraintNotExistList)
            {
                Exposure exposure = new Exposure();
                exposure.Constraint = item.Constraint;
                exposure.Contingency = item.Contingency;
                exposure.ID = item.ID;
                exposure.IsShiftEmpty = false;
                exposureHash.Add(item.ID, exposure);
            }
            List<Exposure> ConstraintExposureList = new List<Exposure>();
            ConstraintExposureList = exposureHash.Values.ToList<Exposure>();
            Parallel.ForEach(ConstraintExposureList, item =>
            {
                if (item.Sum == null)
                    item.Sum = 0;
                foreach (PropertyInfo propItem in item.GetType().GetProperties().Where(a => a.Name.StartsWith("HE")))
                {
                    try
                    {
                        item.Sum += Convert.ToDouble(propItem.GetValue(item) == null ? 0 : propItem.GetValue(item));
                    }
                    catch
                    {
                    }
                }
            });
            return ConstraintExposureList.OrderBy(t => t.Sum).ToList();
        }

        private void ShowHideSummaryGrid()
        {
            if (ShowHideSummary == Visibility.Visible)
            {
                ShowHideSummary = Visibility.Collapsed;
            }
            else
            {
                ShowHideSummary = Visibility.Visible;
            }
        }

        private void DetailsTabSelectionChanged()
        {
            if (SelectedTabIndex != 0)
            {
                if (mRefreshGraphs)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    RetrieveFetchDataAndUpdateChartCommand();
                    Mouse.OverrideCursor = null;
                }
            }
        }


        //private void OpenLMPStatistics()
        //{
        //    try
        //    {
        //        Mouse.OverrideCursor = Cursors.Wait;
        //        if (mLMPStatisticsWindow == null || mLMPStatisticsWindow.IsVisible == false)
        //        {
        //            mLMPStatisticalViewModel = new Vayu.LMPStatistics.ViewModel.LMPStatisticsViewModel(new LMPStatistics.Model.DataService());
        //            mLMPStatisticsWindow = new LMPStatistics.MainWindow();
        //            mLMPStatisticsWindow.DataContext = mLMPStatisticalViewModel;
        //            mSourceSinkList = new List<SourceSinkData>();
        //        }
        //        List<string> selectedValues = new List<string>();
        //        foreach (Path item in SelectedPathByCell)
        //        {
        //            SourceSinkData sourceSinkData = new SourceSinkData();
        //            PricingNode priceNode = new PricingNode();
        //            priceNode = DBAccess.GetNodeFromName(item.Source, item.Market);
        //            sourceSinkData.Source = priceNode;
        //            if (UptosChecked)
        //            {
        //                PricingNode priceSinkNode = DBAccess.GetNodeFromName(item.Sink, item.Market);
        //                sourceSinkData.Sink = priceSinkNode;
        //            }
        //            mSourceSinkList.Add(sourceSinkData);
        //            selectedValues.Add(item.AnalysisType);
        //        }
        //        selectedValues.Add(StartDate.ToString());
        //        selectedValues.Add(EndDate.ToString());
        //        selectedValues.Add(UptosChecked.ToString());
        //        mLMPStatisticalViewModel.SetValuesFromPortfolio(selectedValues);
        //        mLMPStatisticalViewModel.SetSourceSinkList(mSourceSinkList);
        //        mLMPStatisticsWindow.Show();
        //        Mouse.OverrideCursor = null;
        //    }
        //    catch
        //    {
        //    }

        //}


        //private void OpenNodePriceGraphs()
        //{
        //    try
        //    {
        //        Mouse.OverrideCursor = Cursors.Wait;
        //        if (mNodePriceGraphsWinodw == null || mNodePriceGraphsWinodw.IsVisible == false)
        //        {
        //            mNodePriceGraphViewModel = new NodePriceGraph.ViewModel.MainViewModel(new NodePriceGraph.Model.DataService());
        //            mNodePriceGraphsWinodw = new NodePriceGraph.MainWindow();
        //            mNodePriceGraphsWinodw.DataContext = mNodePriceGraphViewModel;
        //            mSourceSinkList = new List<SourceSinkData>();
        //        }
        //        List<string> selectedValues = new List<string>();
        //        foreach (Path item in SelectedPathByCell)
        //        {
        //            SourceSinkData sourcesSinkData = new SourceSinkData();
        //            PricingNode priceNode = new PricingNode();
        //            priceNode = DBAccess.GetNodeFromName(item.Source, item.Market);
        //            sourcesSinkData.Source = priceNode;
        //            if (UptosChecked)
        //            {
        //                PricingNode priceSinkNode = DBAccess.GetNodeFromName(item.Sink, item.Market);
        //                sourcesSinkData.Sink = priceSinkNode;
        //            }
        //            mSourceSinkList.Add(sourcesSinkData);
        //        }
        //        selectedValues.Add(MarketComboSelectedValue.ToString());
        //        selectedValues.Add(UptosChecked.ToString());
        //        mNodePriceGraphViewModel.SetValuesFromPortfolio(selectedValues);
        //        mNodePriceGraphViewModel.SetSourceSinkListFromPortfolio(mSourceSinkList);
        //        mNodePriceGraphsWinodw.Show();
        //        Mouse.OverrideCursor = null;
        //    }
        //    catch
        //    {


        //    }
        //}

        private void SetPreferences()
        {
            string prefText = _dataService.GetPreference(Environment.UserName);
            string[] preferences = prefText.Split('?');
            foreach (var item in preferences)
            {
                // SetUniquePreference(item);
            }
        }

        private void SetUniquePreference(string item)
        {
            switch (item.ToLower())
            {
                case "avgdart":
                    DARTChecked = true;
                    break;
                case "avgda":
                    AvgDAChecked = true;
                    break;
                case "avgrt":
                    AvgRTChecked = true;
                    break;
                case "minda":
                    DAMinChecked = true;
                    break;
                case "minrt":
                    MinRTChecked = true;
                    break;
                case "mindart":
                    MinDARTChecked = true;
                    break;
                case "maxda":
                    MaxDAChecked = true;
                    break;
                case "maxrt":
                    MaxRTChecked = true;
                    break;
                case "maxdart":
                    MaxDARTChecked = true;
                    break;
                case "notional":
                    NotionalChecked = true;
                    break;
                case "cleared":
                    ClearedChecked = true;
                    break;
                case "asbidrisk":
                    AsBidRiskChecked = true;
                    break;
                case "asbidmaxwin":
                    AsBidMaxWinChecked = true;
                    break;
                case "asbidsum":
                    AsBidSumChecked = true;
                    break;
                case "asbidwinpct":
                    AsBidWinPctChecked = true;
                    break;
                case "asbidriskrwd":
                    AsBidRiskRwdChecked = true;
                    break;
                case "asbidavgdart":
                    AsBidDARTChecked = true;
                    break;
                case "musttakerisk":
                    MustTakeRiskChecked = true;
                    break;
                case "musttakemaxwin":
                    MustTakeMaxWinChecked = true;
                    break;
                case "musttakesum":
                    MustTakeSumChecked = true;
                    break;
                case "musttakewinpct":
                    MustTakeWinPctChecked = true;
                    break;
                case "musttakeriskrwd":
                    MustTakeRiskRwdChecked = true;
                    break;
                case "musttakeavgdart":
                    MustTakeDARTChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void ShowPathMws()
        {
            if (PortfolioDate != DateTime.Now.Date.AddDays(1))
            {
                if (PortfolioList != null)
                {
                    Dictionary<string, List<Bid>> sourceSinkHash = new Dictionary<string, List<Bid>>();
                    List<Bid> bidList = new List<Bid>();
                    foreach (Portfolio portfolio in PortfolioList)
                    {
                        List<Bid> tempBidList = new List<Bid>();
                        string savedName = portfolio.IsUptos ? null : portfolio.Name;
                        if (MustTakeChecked)
                            tempBidList = DBAccess.GetBids(portfolio.Market, portfolio.ID, savedName, PortfolioDate, PortfolioDate.AddDays(1),
                                                portfolio.IsUptos, "");
                        else
                            tempBidList = DBAccess.GetCleareds(portfolio.IsUptos, GetMarketKey(portfolio.Market), portfolio.ID, PortfolioDate, PortfolioDate);
                        bidList.AddRange(tempBidList);
                    }
                    foreach (Bid bid in bidList)
                    {
                        string name = null;
                        if (bid.IsUptos)
                        {
                            PricingNode sourcePriceNode = DBAccess.GetNode(bid.Source, bid.Market);
                            PricingNode sinkPriceNode = DBAccess.GetNode(bid.Sink, bid.Market);
                            // Portfolio portfoliodata = PortfolioList.FirstOrDefault(x => x.ID.Equals(bid.PortfolioKey));
                            if (PortfolioChecked)
                            {
                                name = sourcePriceNode.NodeName + "?" + sinkPriceNode.NodeName + "?" + bid.PortfolioKey;
                            }
                            else
                            {
                                name = sourcePriceNode.NodeName + "?" + sinkPriceNode.NodeName;
                            }

                        }
                        else
                        {
                            PricingNode priceNode = DBAccess.GetNode(bid.Source, bid.Market);
                            name = priceNode.NodeName;
                        }
                        List<Bid> tempBidList = new List<Bid>();
                        if (sourceSinkHash.ContainsKey(name))
                        {
                            tempBidList = sourceSinkHash[name];
                            sourceSinkHash.Remove(name);
                        }
                        tempBidList.Add(bid);
                        sourceSinkHash.Add(name, tempBidList);
                    }
                    List<string> sourceSinkKeyList = sourceSinkHash.Keys.ToList<string>();
                    sourceSinkKeyList.Sort();
                    List<HourlyPathMWs> hourlyPathMWList = new List<HourlyPathMWs>();
                    HourlyPathMWs totalHourlyMW = new HourlyPathMWs();
                    //HourlyPathMWs TotalhourlyMW = new HourlyPathMWs();
                    foreach (string sourceSinkName in sourceSinkKeyList)
                    {
                        List<Bid> tempBidList = sourceSinkHash[sourceSinkName];
                        HourlyPathMWs hourlyMW = new HourlyPathMWs();
                        string[] nodeName = sourceSinkName.Split('?');
                        hourlyMW.SourceName = nodeName[0].ToString();
                        hourlyMW.SinkName = nodeName[1].ToString();
                        if (nodeName.Count() == 3)
                        {
                            Portfolio portfoliodata = PortfolioList.FirstOrDefault(x => x.ID.Equals(Convert.ToInt32(nodeName[2])));
                            hourlyMW.PortfolioName = portfoliodata.Name;
                        }
                        foreach (Bid bid in tempBidList)
                        {
                            int hour = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                            if (hour == 1)
                            {
                                hourlyMW.Hour1 += bid.MW;
                                totalHourlyMW.Hour1 += bid.MW;
                            }
                            else if (hour == 2)
                            {
                                hourlyMW.Hour2 += bid.MW;
                                totalHourlyMW.Hour2 += bid.MW;
                            }
                            else if (hour == 3)
                            {
                                hourlyMW.Hour3 += bid.MW;
                                totalHourlyMW.Hour3 += bid.MW;
                            }
                            else if (hour == 4)
                            {
                                hourlyMW.Hour4 += bid.MW;
                                totalHourlyMW.Hour4 += bid.MW;
                            }
                            else if (hour == 5)
                            {
                                hourlyMW.Hour5 += bid.MW;
                                totalHourlyMW.Hour5 += bid.MW;
                            }
                            else if (hour == 6)
                            {
                                hourlyMW.Hour6 += bid.MW;
                                totalHourlyMW.Hour6 += bid.MW;
                            }
                            else if (hour == 7)
                            {
                                hourlyMW.Hour7 += bid.MW;
                                totalHourlyMW.Hour7 += bid.MW;
                            }
                            else if (hour == 8)
                            {
                                hourlyMW.Hour8 += bid.MW;
                                totalHourlyMW.Hour8 += bid.MW;
                            }
                            else if (hour == 9)
                            {
                                hourlyMW.Hour9 += bid.MW;
                                totalHourlyMW.Hour9 += bid.MW;
                            }
                            else if (hour == 10)
                            {
                                hourlyMW.Hour10 += bid.MW;
                                totalHourlyMW.Hour10 += bid.MW;
                            }
                            else if (hour == 11)
                            {
                                hourlyMW.Hour11 += bid.MW;
                                totalHourlyMW.Hour11 += bid.MW;
                            }
                            else if (hour == 12)
                            {
                                hourlyMW.Hour12 += bid.MW;
                                totalHourlyMW.Hour12 += bid.MW;
                            }
                            else if (hour == 13)
                            {
                                hourlyMW.Hour13 += bid.MW;
                                totalHourlyMW.Hour13 += bid.MW;
                            }
                            else if (hour == 14)
                            {
                                hourlyMW.Hour14 += bid.MW;
                                totalHourlyMW.Hour14 += bid.MW;
                            }
                            else if (hour == 15)
                            {
                                hourlyMW.Hour15 += bid.MW;
                                totalHourlyMW.Hour15 += bid.MW;
                            }
                            else if (hour == 16)
                            {
                                hourlyMW.Hour16 += bid.MW;
                                totalHourlyMW.Hour16 += bid.MW;
                            }
                            else if (hour == 17)
                            {
                                hourlyMW.Hour17 += bid.MW;
                                totalHourlyMW.Hour17 += bid.MW;
                            }
                            else if (hour == 18)
                            {
                                hourlyMW.Hour18 += bid.MW;
                                totalHourlyMW.Hour18 += bid.MW;
                            }
                            else if (hour == 19)
                            {
                                hourlyMW.Hour19 += bid.MW;
                                totalHourlyMW.Hour19 += bid.MW;
                            }
                            else if (hour == 20)
                            {
                                hourlyMW.Hour20 += bid.MW;
                                totalHourlyMW.Hour20 += bid.MW;
                            }
                            else if (hour == 21)
                            {
                                hourlyMW.Hour21 += bid.MW;
                                totalHourlyMW.Hour21 += bid.MW;
                            }
                            else if (hour == 22)
                            {
                                hourlyMW.Hour22 += bid.MW;
                                totalHourlyMW.Hour22 += bid.MW;
                            }
                            else if (hour == 23)
                            {
                                hourlyMW.Hour23 += bid.MW;
                                totalHourlyMW.Hour23 += bid.MW;
                            }
                            else if (hour == 24)
                            {
                                hourlyMW.Hour24 += bid.MW;
                                totalHourlyMW.Hour24 += bid.MW;
                            }
                            hourlyMW.Total += Math.Round(bid.MW, 2);
                            totalHourlyMW.Total += Math.Round(bid.MW, 2);
                        }
                        hourlyPathMWList.Add(hourlyMW);
                    }
                    hourlyPathMWList.Insert(0, totalHourlyMW);
                    PathMWList = hourlyPathMWList;
                    var FilteredPathMWlist = (dynamic)null;
                    if (SourcePathChecked || SinkPathChecked)
                    {
                        List<HourlyPathMWs> filteredSourcePathMWlist = new List<HourlyPathMWs>();
                        if (SourcePathChecked)
                        {
                            FilteredPathMWlist = PathMWList.GroupBy(x => x.SourceName).Select(p => p.ToList()).ToList();
                        }
                        else if (SinkPathChecked)
                        {
                            FilteredPathMWlist = PathMWList.GroupBy(x => x.SinkName).Select(p => p.ToList()).ToList();
                        }
                        foreach (var grp in FilteredPathMWlist)
                        {
                            HourlyPathMWs hourlySourcePath = new HourlyPathMWs();
                            if (grp.Count > 1)
                            {
                                for (int i = 0; i < grp.Count; i++)
                                {
                                    hourlySourcePath.Hour1 += grp[i].Hour1;
                                    hourlySourcePath.Hour2 += grp[i].Hour2;
                                    hourlySourcePath.Hour3 += grp[i].Hour3;
                                    hourlySourcePath.Hour4 += grp[i].Hour4;
                                    hourlySourcePath.Hour5 += grp[i].Hour5;
                                    hourlySourcePath.Hour6 += grp[i].Hour6;
                                    hourlySourcePath.Hour7 += grp[i].Hour7;
                                    hourlySourcePath.Hour8 += grp[i].Hour8;
                                    hourlySourcePath.Hour9 += grp[i].Hour9;
                                    hourlySourcePath.Hour10 += grp[i].Hour10;
                                    hourlySourcePath.Hour11 += grp[i].Hour11;
                                    hourlySourcePath.Hour12 += grp[i].Hour12;
                                    hourlySourcePath.Hour13 += grp[i].Hour13;
                                    hourlySourcePath.Hour14 += grp[i].Hour14;
                                    hourlySourcePath.Hour15 += grp[i].Hour15;
                                    hourlySourcePath.Hour16 += grp[i].Hour16;
                                    hourlySourcePath.Hour17 += grp[i].Hour17;
                                    hourlySourcePath.Hour18 += grp[i].Hour18;
                                    hourlySourcePath.Hour19 += grp[i].Hour19;
                                    hourlySourcePath.Hour20 += grp[i].Hour20;
                                    hourlySourcePath.Hour21 += grp[i].Hour21;
                                    hourlySourcePath.Hour22 += grp[i].Hour22;
                                    hourlySourcePath.Hour23 += grp[i].Hour23;
                                    hourlySourcePath.Hour24 += grp[i].Hour24;
                                    hourlySourcePath.Total += grp[i].Total;
                                    hourlySourcePath.PortfolioName = grp[i].PortfolioName;
                                }
                            }
                            else
                            {
                                hourlySourcePath.Hour1 = grp[0].Hour1;
                                hourlySourcePath.Hour2 = grp[0].Hour2;
                                hourlySourcePath.Hour3 = grp[0].Hour3;
                                hourlySourcePath.Hour4 = grp[0].Hour4;
                                hourlySourcePath.Hour5 = grp[0].Hour5;
                                hourlySourcePath.Hour6 = grp[0].Hour6;
                                hourlySourcePath.Hour7 = grp[0].Hour7;
                                hourlySourcePath.Hour8 = grp[0].Hour8;
                                hourlySourcePath.Hour9 = grp[0].Hour9;
                                hourlySourcePath.Hour10 = grp[0].Hour10;
                                hourlySourcePath.Hour11 = grp[0].Hour11;
                                hourlySourcePath.Hour12 = grp[0].Hour12;
                                hourlySourcePath.Hour13 = grp[0].Hour13;
                                hourlySourcePath.Hour14 = grp[0].Hour14;
                                hourlySourcePath.Hour15 = grp[0].Hour15;
                                hourlySourcePath.Hour16 = grp[0].Hour16;
                                hourlySourcePath.Hour17 = grp[0].Hour17;
                                hourlySourcePath.Hour18 = grp[0].Hour18;
                                hourlySourcePath.Hour19 = grp[0].Hour19;
                                hourlySourcePath.Hour20 = grp[0].Hour20;
                                hourlySourcePath.Hour21 = grp[0].Hour21;
                                hourlySourcePath.Hour22 = grp[0].Hour22;
                                hourlySourcePath.Hour23 = grp[0].Hour23;
                                hourlySourcePath.Hour24 = grp[0].Hour24;
                                hourlySourcePath.Total += grp[0].Total;
                                hourlySourcePath.PortfolioName = grp[0].PortfolioName;
                            }
                            if (SourcePathChecked)
                            {
                                hourlySourcePath.SourceName = grp[0].SourceName;
                            }
                            else if (SinkPathChecked)
                            {
                                hourlySourcePath.SinkName = grp[0].SinkName;
                            }
                            filteredSourcePathMWlist.Add(hourlySourcePath);
                        }
                        PathMWList = filteredSourcePathMWlist;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select Cleared/Submitted file Date!");
            }
            //DrawFirstGraph(PathMWList);
        }

        private void PieChart()
        {

            Chart chart = new Chart();
            PlotDataFirst = DrawFirstGraph();
            chart.DataContext = new ChartViewModel { PlotDataFirstNew = PlotDataFirst };
            chart.Title = "Pie Chart";
            chart.Show();
        }

        public PlotModel DrawFirstGraph()
        {
            PlotModel plotModel = new PlotModel();
            if (PathMWList == null)
            {
                System.Windows.MessageBox.Show("Please select Portfilio ,source OR Sink ");
                return null;
            }
            else
            {
                try
                {
                    int marketkey = GetMarketKey(MarketComboSelectedValue.ToString());
                    List<HourlyPathMWs> PathList = PathMWList.ToList();
                    int count = PathList.Distinct().Count();
                    Dictionary<string, string> tempDict = _dataService.GetFuelSource();
                    List<HourlyPathMWs> temppathList = new List<HourlyPathMWs>();
                    string nodeType = string.Empty;
                    if (PathList.Count > 0)
                    {
                        temppathList = PathList.ToList();
                        if (SourcePathChecked)
                            nodeType = "Pie Chart For Source.";
                        else if (SinkPathChecked)
                            nodeType = "Pie Chart For Sink.";
                        else
                            nodeType = "Pie Chart For Source and Sink.";
                        if (marketkey == 9)
                        {
                            if (FuelChecked)
                            {
                                temppathList.Clear();
                                List<HourlyPathMWs> tempList = new List<HourlyPathMWs>();
                                foreach (HourlyPathMWs item in PathList.OrderByDescending(x => x.SourceName))
                                {
                                    HourlyPathMWs path = new HourlyPathMWs();
                                    path.Total = item.Total;
                                    if (SourcePathChecked)
                                        if (item.SourceName != null)
                                        {
                                            if (tempDict.ContainsKey(item.SourceName))
                                                path.SourceName = tempDict[item.SourceName].ToString();
                                            else
                                                path.SourceName = "Blank";
                                        }
                                        else
                                            continue;
                                    if (SinkPathChecked)
                                        if (item.SinkName != null)
                                        {
                                            if (tempDict.ContainsKey(item.SinkName))
                                                path.SourceName = tempDict[item.SinkName].ToString();
                                            else
                                                path.SourceName = "Blank";
                                        }
                                        else
                                            continue;
                                    if (NonePathChecked)
                                    {
                                        if (item.SourceName != null)
                                        {
                                            if (tempDict.ContainsKey(item.SourceName))
                                                path.SourceName = tempDict[item.SourceName].ToString();
                                            else
                                                path.SourceName = "Blank";
                                            tempList.Add(path);
                                        }
                                        if (item.SinkName != null)
                                        {
                                            if (tempDict.ContainsKey(item.SinkName))
                                                path.SourceName = tempDict[item.SinkName].ToString();
                                            else
                                                path.SourceName = "Blank";
                                        }
                                    }
                                    tempList.Add(path);
                                }
                                foreach (string key in tempDict.Values.Distinct())
                                {
                                    HourlyPathMWs pathvalue = new HourlyPathMWs();
                                    pathvalue.SourceName = key;
                                    pathvalue.Total = tempList.Where(a => a.SourceName == key).Sum(a => a.Total);
                                    if (pathvalue.Total == 0.0)
                                        continue;
                                    temppathList.Add(pathvalue);
                                }
                            }
                        }
                        plotModel = new PlotModel
                        {
                            Title = nodeType,//"Pie Chart",
                            TitleFontSize = 20,
                            DefaultFont = "Arial Black",
                            DefaultFontSize = 10
                        };
                        // plotModel.Series = new Collection<Series>();
                        var ps = new PieSeries
                        {
                            InsideLabelFormat = "{1}",
                            AreInsideLabelsAngled = true,
                            //OutsideLabelFormat = "{0}",
                            StrokeThickness = .80,
                            InsideLabelPosition = .70,
                            AngleSpan = 360,
                            StartAngle = 0,
                            TextColor = OxyColors.Black,
                        };
                        double total1 = temppathList.Sum(a => a.Total);
                        foreach (HourlyPathMWs item in temppathList.OrderByDescending(x => x.SourceName))
                        {
                            if (item.SourceName == null && item.SinkName == null)
                                continue;
                            string Name = string.Empty;
                            if (SourcePathChecked)
                                if (item.SourceName != null)
                                    Name = item.SourceName.ToString();
                                else
                                    continue;
                            if (SinkPathChecked)
                                if (marketkey == 9)
                                {
                                    if (FuelChecked)
                                        Name = item.SourceName;
                                    else
                                        Name = item.SinkName;
                                }
                                else if (item.SinkName != null)
                                    Name = item.SinkName.ToString();
                                else
                                    continue;
                            if (NonePathChecked)
                                if (marketkey == 9)
                                    Name = item.SourceName;
                                else
                                    Name = item.SourceName + ">" + item.SinkName;
                            ps.Slices.Add(new PieSlice(
                                Name,
                                item.Total)
                            { IsExploded = true });
                        }

                        plotModel.Series.Add(ps);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            PlotDataFirst = plotModel;
            return PlotDataFirst;
        }

        public void HandleSort(List<HourlyPathMWs> sortingPathMWList, ListSortDirection listSortDirection)
        {
            try
            {
                if (sortingPathMWList != null)
                {
                    var tempHourlyPath = sortingPathMWList.Find(x => x.SourceName == null && x.SinkName == null);
                    List<HourlyPathMWs> tempPathMWList = new List<HourlyPathMWs>();
                    sortingPathMWList.Remove(tempHourlyPath);
                    if (listSortDirection == ListSortDirection.Ascending)
                    {
                        tempPathMWList = sortingPathMWList.OrderBy(m => m.GetType().GetProperty(sortCondition).GetValue(m)).ToList();
                    }
                    else
                    {
                        tempPathMWList = sortingPathMWList.OrderByDescending(m => m.GetType().GetProperty(sortCondition).GetValue(m)).ToList();
                    }
                    tempPathMWList.Insert(0, tempHourlyPath);
                    PathMWList = tempPathMWList;
                }
            }
            catch (Exception ex)
            {
                //throw;
            }
        }


        #endregion

        #region Internal Methods

        internal void CheckAll(bool p)
        {
            List<Path> tempPathList = PathList as List<Path>;
            if (tempPathList != null)
            {
                Parallel.ForEach(tempPathList, a => a.Submit = p);
                PathList = tempPathList.ToList();
            }
        }

        internal void UpdateRows(string bidid, bool isSubmit)
        {
            if (bidid != "")
            {
                Parallel.ForEach(PathList, a =>
                {
                    if (a.BidId == bidid)
                    {
                        a.Submit = isSubmit;
                    }
                });
            }
        }

        #endregion
        private void ScaleExecute()
        {
            if (PathList != null)
            {
                string status = PathList.FirstOrDefault().Status;
                if (status == "IMPORTED" || status == "Cancelled")
                {
                    if (TextScale != null)
                    {
                        double num = Convert.ToDouble(TextScale);
                        if (num > 0)
                        {

                            if (TraderPortfolioComboSelectedItem == null)
                            {
                                _dataService.UpdateScaleNumber(num, PortfolioComboSelectedItem, PortfolioDate, PortfolioDate.AddDays(1));
                                Retrieve();
                                MessageBox.Show("Updated MWs");
                            }
                            else
                            {
                                _dataService.UpdateScaleNumber(num, TraderPortfolioComboSelectedItem, PortfolioDate, PortfolioDate.AddDays(1));
                                Retrieve();
                                MessageBox.Show("Updated MWs");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Scaling Number should be more than ZERO.");
                        }
                    }
                    else
                        MessageBox.Show("Please enter Scaling Number.");
                }
                else
                {
                    MessageBox.Show("Status of the Portfolio should be IMPORTED.");
                }

            }
            else
            {
                MessageBox.Show("Please Import the file.");
            }
        }

    }


    public class FilterData
    {
        public string Product { get; set; }

        public string Type { get; set; }

        public double? Min { get; set; }

        public double? Max { get; set; }
    }

    public class HourlyData
    {

        public DateTime Date { get; set; }

        public int Hour { get; set; }

        public double? Source { get; set; }

        public double? Sink { get; set; }
        /// <summary>
        /// Gets or sets the spread.
        /// </summary>
        /// <value>
        /// The spread.
        /// </value>
        public double? Spread { get; set; }
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
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ComparisonConverter : IValueConverter
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
            return value.Equals(parameter);
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
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : System.Windows.Data.Binding.DoNothing;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public enum SpreadType
    {
        /// <summary>
        /// The da
        /// </summary>
        DA,
        /// <summary>
        /// The rt
        /// </summary>
        RT,
        /// <summary>
        /// The dart
        /// </summary>
        DART
    }
    /// <summary>
    /// 
    /// </summary>
    public enum SortType
    {
        /// <summary>
        /// The date
        /// </summary>
        Date,
        /// <summary>
        /// The da
        /// </summary>
        DA,
        /// <summary>
        /// The rt
        /// </summary>
        RT,
        /// <summary>
        /// The dart
        /// </summary>
        DART
    }
    /// <summary>
    /// 
    /// </summary>
    public enum PeriodType
    {

        Hourly,

        Daily
    }
    /// <summary>
    /// 
    /// </summary>
    public enum DataFilteredType
    {
        Orig,

        FilteredSorted
    }

    public class ValidateBids
    {

        public int Source { get; set; }

        public int Sink { get; set; }

        public string AnalysisType { get; set; }

        public double Price { get; set; }

        public double MW { get; set; }

        public string BidID { get; set; }
    }

    public sealed class LogWriter
    {
        public static void WriteLog(string message)
        {
            //string fileName = "PortfolioModelingLog" + DateTime.Today.ToString("yyyyMMdd") + ".txt";
            //if (!File.Exists(fileName))
            //{
            //    File.Create(fileName).Close();

            //    File.WriteAllText(fileName, message);
            //}
            //else
            //    File.AppendAllText(fileName,   message + Environment.NewLine);

        }
    }
}
