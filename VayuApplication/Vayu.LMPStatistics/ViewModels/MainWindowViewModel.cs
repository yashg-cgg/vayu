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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Vayu.CommonControls;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.LMPStatistics.Model;
using Vayu.NodePriceLibrary;

namespace Vayu.LMPStatistics.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Declaration
        /// <summary>
        /// The data service
        /// </summary>
        private readonly IDataService _dataService;
        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m fill filter list
        /// </summary>
        private List<FilterData> mFillFilterList = new List<FilterData>();
        /// <summary>
        /// The m weather list
        /// </summary>
        private List<string> mWeatherList = new List<string>();
        /// <summary>
        /// The m fill source sink hash
        /// </summary>
        private Dictionary<string, SourceSinkData> mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
        /// <summary>
        /// The m load hash
        /// </summary>
        private Dictionary<string, Dictionary<string, Load>> mLoadHash = new Dictionary<string, Dictionary<string, Load>>();
        /// <summary>
        /// The m is swap
        /// </summary>
        private bool mIsSwap = false;
        /// <summary>
        /// The m source sink data list
        /// </summary>
        private List<SourceSinkData> mSourceSinkDataList = new List<SourceSinkData>();
        /// <summary>
        /// The plot model
        /// </summary>
        private PlotModel plotModel;
        /// <summary>
        /// The m oxy color list
        /// </summary>
        private Dictionary<string, OxyColor> mOxyColorList;
        /// <summary>
        /// The pnode identifier hash
        /// </summary>
        Dictionary<int, List<string>> pnodeIdHash = new Dictionary<int, List<string>>();
        /// <summary>
        /// The m cached node list hash
        /// </summary>
        private Dictionary<string, List<Node>[,,]> mCachedNodeListHash = new Dictionary<string, List<Node>[,,]>();
        /// <summary>
        /// The m da filtered sorted list
        /// </summary>
        private List<Node> mDaFilteredSortedList = new List<Node>();
        /// <summary>
        /// The m rt filtered sorted list
        /// </summary>
        private List<Node> mRTFilteredSortedList = new List<Node>();
        /// <summary>
        /// The m dart filtered sorted list
        /// </summary>
        private List<Node> mDartFilteredSortedList = new List<Node>();
        /// <summary>
        /// The m hourly pivot hash
        /// </summary>
        private Dictionary<string, List<Node>> mHourlyPivotHash = new Dictionary<string, List<Node>>();
        private Dictionary<string, List<Bollinger>> listBollinger = new Dictionary<string, List<Bollinger>>();

        /// <summary>
        /// The m rt hash
        /// </summary>
        private Dictionary<int, Node> mRTHash = new Dictionary<int, Node>();
        /// <summary>
        /// The m da hash
        /// </summary>
        private Dictionary<int, Node> mDAHash = new Dictionary<int, Node>();
        #endregion

        public Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
        ILMPEnergyPrice mLMPEnergyPriceProxy;

        public DelegateCommand DeleteDateCommand { private set; get; }

        public DelegateCommand OpenConstraints { get; private set; }
        /// <summary>
        /// Gets or sets the run retrieve fetch data and update chart command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data and update chart command.
        /// </value>

        private string mEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.EnergyPriceService();
        #region Properties

        private PlotModel mPlotModelEnergyPrice;
        public PlotModel PlotModelEnergyPrice
        {
            get
            {
                return mPlotModelEnergyPrice;
            }
            set
            {
                mPlotModelEnergyPrice = value;
                RaisePropertyChanged("PlotModelEnergyPrice");
            }
        }

        private int _rtPrints;

        public int rtprints
        {
            get
            {
                return _rtPrints;
            }
            set
            {
                _rtPrints = value;
                RaisePropertyChanged(nameof(rtprints));
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
        /// The m plot simulation
        /// </summary>
        private PlotModel mPlotSimulation;
        /// <summary>
        /// Gets or sets the plot simulation.
        /// </summary>
        /// <value>
        /// The plot simulation.
        /// </value>
        public PlotModel PlotSimulation
        {
            get
            {
                return mPlotSimulation;
            }
            set
            {
                mPlotSimulation = value;
                RaisePropertyChanged("PlotSimulation");
            }
        }
        /// <summary>
        /// The m filter data selected
        /// </summary>
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
        /// <summary>
        /// The m source sink data selected
        /// </summary>
        private SourceSinkData mSourceSinkDataSelected;
        /// <summary>
        /// Gets or sets the source sink data selected.
        /// </summary>
        /// <value>
        /// The source sink data selected.
        /// </value>
        public SourceSinkData SourceSinkDataSelected
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
                    //DeenergizedNodes
                    if (mSourceSinkDataSelected != null)
                    {
                        if (MarketComboSelectedValue == "ERCOT")
                        {
                            string Source = SourceSinkDataSelected.Source.ToString();
                            string Sink = SourceSinkDataSelected.Sink.ToString();
                            GetDeenergizedNodes(Source, Sink);
                            GetFuelTypes(Source, Sink);
                        }
                    }
                    else
                    {
                        DeenergizedSourceChecked = false;
                        DeenergizedSinkChecked = false;
                    }

                    RaisePropertyChanged("SourceSinkDataSelected");
                }
            }
        }
        /// <summary>
        /// The selected sort type
        /// </summary>
        private SortType selectedSortType;
        /// <summary>
        /// Gets or sets the type of the selected sort.
        /// </summary>
        /// <value>
        /// The type of the selected sort.
        /// </value>
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
                Sort(mDaFilteredSortedList, mRTFilteredSortedList, mDartFilteredSortedList);
                PlotUpperAndLowerChartsAndSetHourlyList();
            }
        }
        /// <summary>
        /// The m selected period type
        /// </summary>
        private PeriodType mSelectedPeriodType;
        /// <summary>
        /// Gets or sets the type of the selected period.
        /// </summary>
        /// <value>
        /// The type of the selected period.
        /// </value>
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
                PlotUpperAndLowerChartsAndSetHourlyList();
            }
        }
        /// <summary>
        /// The m selected node
        /// </summary>
        private bool mSelectedNode = true;
        /// <summary>
        /// Gets or sets a value indicating whether [selected node].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [selected node]; otherwise, <c>false</c>.
        /// </value>
        public bool SelectedNode
        {
            get
            {
                return mSelectedNode;
            }
            set
            {
                mSelectedNode = value;
                if (!UptosChecked)
                {
                    SetSourceSink();
                }
                RaisePropertyChanged("SelectedNode");
            }
        }
        /// <summary>
        /// The m selected path
        /// </summary>
        private bool mSelectedPath;
        /// <summary>
        /// Gets or sets a value indicating whether [selected path].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [selected path]; otherwise, <c>false</c>.
        /// </value>
        public bool SelectedPath
        {
            get
            {
                return mSelectedPath;
            }
            set
            {
                mSelectedPath = value;
                if (!UptosChecked)
                {
                    SetSourceSink();
                }
                RaisePropertyChanged("SelectedPath");
            }
        }
        /// <summary>
        /// The m deenergized nodes list
        /// </summary>
        private ObservableCollection<string> mDeenergizedNodesList;
        /// <summary>
        /// Gets or sets the deenergized nodes list.
        /// </summary>
        /// <value>
        /// The deenergized nodes list.
        /// </value>
        public ObservableCollection<string> DeenergizedNodesList
        {
            get
            {
                return mDeenergizedNodesList;
            }
            set
            {
                mDeenergizedNodesList = value;
                //if (!UptosChecked)
                //{
                //    SetSourceSink();
                //}
                RaisePropertyChanged("DeenergizedNodesList");
            }
        }
        private List<SourceSink> mInValidPathNodesList;
        /// <summary>
        /// Gets or sets the mInValidSinkNodesList.
        /// </summary>
        /// <value>
        public List<SourceSink> InValidPathNodesList
        {
            get
            {
                return mInValidPathNodesList;
            }
            set
            {
                mInValidPathNodesList = value;
                RaisePropertyChanged("InValidPathNodesList");
            }
        }
        /// <summary>
        /// The m uptos checked
        /// </summary>
        private bool mUptosChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [uptos checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [uptos checked]; otherwise, <c>false</c>.
        /// </value>
        public bool UptosChecked
        {
            get
            {
                return mUptosChecked;
            }
            set
            {
                mUptosChecked = value;
                SetSourceSink();
                SetSimMinMax();
                RaisePropertyChanged("UptosChecked");
            }
        }
        /// <summary>
        /// The m type list
        /// </summary>
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
        /// <summary>
        /// The m product list
        /// </summary>
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
        /// <summary>
        /// The m inc checked
        /// </summary>
        private bool mIncChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [inc checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [inc checked]; otherwise, <c>false</c>.
        /// </value>
        public bool IncChecked
        {
            get
            {
                return mIncChecked;
            }
            set
            {
                mIncChecked = value;
                SelectSourceSinkFetchDataUpdateChart();
                RaisePropertyChanged("IncChecked");
            }
        }
        private bool mFiftyChecked = true;
        public bool FiftyChecked
        {
            get
            {
                return mFiftyChecked;
            }
            set
            {
                mFiftyChecked = value;
                // Bollinger();
                RaisePropertyChanged("FiftyChecked");
            }
        }

        private bool mTwenty6Checked;
        public bool Twenty6Checked
        {
            get
            {
                return mTwenty6Checked;
            }
            set
            {
                mTwenty6Checked = value;
                // Bollinger();
                RaisePropertyChanged("Twenty6Checked");
            }
        }
        private bool mTwelveChecked;
        public bool TwelveChecked
        {
            get
            {
                return mTwelveChecked;
            }
            set
            {
                mTwelveChecked = value;
                //  Bollinger();
                RaisePropertyChanged("TwelveChecked");
            }
        }
        private OxyPlot.PlotModel mPlotDataModel;
        public OxyPlot.PlotModel PlotDataModel
        {
            get
            {
                return mPlotDataModel;
            }
            set
            {
                mPlotDataModel = value; RaisePropertyChanged("PlotDataModel");
            }
        }
        /// <summary>
        /// The m monday checked
        /// </summary>
        private bool mMondayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [monday checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [monday checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MondayChecked
        {
            get
            {
                return mMondayChecked;
            }
            set
            {
                mMondayChecked = value;
                UpdateChartCommand();
                RaisePropertyChanged("MondayChecked");
            }
        }
        /// <summary>
        /// The m tuesday checked
        /// </summary>
        private bool mTuesdayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [tuesday checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tuesday checked]; otherwise, <c>false</c>.
        /// </value>
        public bool TuesdayChecked
        {
            get
            {
                return mTuesdayChecked;
            }
            set
            {
                mTuesdayChecked = value;
                UpdateChartCommand();
                RaisePropertyChanged("TuesdayChecked");
            }
        }
        /// <summary>
        /// The m wednesday checked
        /// </summary>
        private bool mWednesdayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [wednesday checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wednesday checked]; otherwise, <c>false</c>.
        /// </value>
        public bool WednesdayChecked
        {
            get
            {
                return mWednesdayChecked;
            }
            set
            {
                mWednesdayChecked = value;
                UpdateChartCommand();
                RaisePropertyChanged("WednesdayChecked");
            }
        }
        /// <summary>
        /// The m thursday checked
        /// </summary>
        private bool mThursdayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [thursday checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [thursday checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ThursdayChecked
        {
            get
            {
                return mThursdayChecked;
            }
            set
            {
                mThursdayChecked = value;
                UpdateChartCommand();
                RaisePropertyChanged("ThursdayChecked");
            }
        }
        /// <summary>
        /// The m friday checked
        /// </summary>
        private bool mFridayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [friday checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [friday checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FridayChecked
        {
            get
            {
                return mFridayChecked;
            }
            set
            {
                mFridayChecked = value;
                UpdateChartCommand();
                RaisePropertyChanged("FridayChecked");
            }
        }
        /// <summary>
        /// The m saturday checked
        /// </summary>
        private bool mSaturdayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [saturday checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [saturday checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SaturdayChecked
        {
            get
            {
                return mSaturdayChecked;
            }
            set
            {
                mSaturdayChecked = value;
                UpdateChartCommand();
                RaisePropertyChanged("SaturdayChecked");
            }
        }
        /// <summary>
        /// The m sunday checked
        /// </summary>
        private bool mSundayChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [sunday checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sunday checked]; otherwise, <c>false</c>.
        /// </value>
        public bool SundayChecked
        {
            get
            {
                return mSundayChecked;
            }
            set
            {
                mSundayChecked = value;
                UpdateChartCommand();
                RaisePropertyChanged("SundayChecked");
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                UpdateChartCommand();
                RaisePropertyChanged("JanChecked");
            }
        }
        /// <summary>
        /// The m sim maximum text
        /// </summary>
        private string mSimMaxText;
        /// <summary>
        /// Gets or sets the sim maximum text.
        /// </summary>
        /// <value>
        /// The sim maximum text.
        /// </value>
        public string SimMaxText
        {
            get
            {
                return mSimMaxText;
            }
            set
            {
                mSimMaxText = value;
                RaisePropertyChanged("SimMaxText");
            }
        }
        /// <summary>
        /// The m sim minimum text
        /// </summary>
        private string mSimMinText;
        /// <summary>
        /// Gets or sets the sim minimum text.
        /// </summary>
        /// <value>
        /// The sim minimum text.
        /// </value>
        public string SimMinText
        {
            get
            {
                return mSimMinText;
            }
            set
            {
                mSimMinText = value;
                RaisePropertyChanged("SimMinText");
            }
        }
        /// <summary>
        /// The m maximum text
        /// </summary>
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
        /// The m type combo selected value
        /// </summary>
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
            get
            {
                return mSourceSinkHeatMapFactors;
            }
            set
            {
                mSourceSinkHeatMapFactors = value;
                RaisePropertyChanged("SourceSinkHeatMapFactors");
            }
        }
        /// <summary>
        /// The m maximum price text
        /// </summary>
        private string mMaxPriceText;
        /// <summary>
        /// Gets or sets the maximum price text.
        /// </summary>
        /// <value>
        /// The maximum price text.
        /// </value>
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
        /// <summary>
        /// The m minimum price text
        /// </summary>
        private string mMinPriceText;
        /// <summary>
        /// Gets or sets the minimum price text.
        /// </summary>
        /// <value>
        /// The minimum price text.
        /// </value>
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
        /// The m h e1 checked
        /// </summary>
        private bool mHE1Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e1 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e1 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE1Checked
        {
            get
            {
                return mHE1Checked;
            }
            set
            {
                mHE1Checked = value;
                RaisePropertyChanged("HE1Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e2 checked
        /// </summary>
        private bool mHE2Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e2 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e2 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE2Checked
        {
            get
            {
                return mHE2Checked;
            }
            set
            {
                mHE2Checked = value;
                RaisePropertyChanged("HE2Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e3 checked
        /// </summary>
        private bool mHE3Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e3 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e3 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE3Checked
        {
            get
            {
                return mHE3Checked;
            }
            set
            {
                mHE3Checked = value;
                RaisePropertyChanged("HE3Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e4 checked
        /// </summary>
        private bool mHE4Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e4 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e4 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE4Checked
        {
            get
            {
                return mHE4Checked;
            }
            set
            {
                mHE4Checked = value;
                RaisePropertyChanged("HE4Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e5 checked
        /// </summary>
        private bool mHE5Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e5 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e5 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE5Checked
        {
            get
            {
                return mHE5Checked;
            }
            set
            {
                mHE5Checked = value;
                RaisePropertyChanged("HE5Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e6 checked
        /// </summary>
        private bool mHE6Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e6 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e6 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE6Checked
        {
            get
            {
                return mHE6Checked;
            }
            set
            {
                mHE6Checked = value;
                RaisePropertyChanged("HE6Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e7 checked
        /// </summary>
        private bool mHE7Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e7 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e7 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE7Checked
        {
            get
            {
                return mHE7Checked;
            }
            set
            {
                mHE7Checked = value;
                RaisePropertyChanged("HE7Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e8 checked
        /// </summary>
        private bool mHE8Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e8 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e8 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE8Checked
        {
            get
            {
                return mHE8Checked;
            }
            set
            {
                mHE8Checked = value;
                RaisePropertyChanged("HE8Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e9 checked
        /// </summary>
        private bool mHE9Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e9 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e9 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE9Checked
        {
            get
            {
                return mHE9Checked;
            }
            set
            {
                mHE9Checked = value;
                RaisePropertyChanged("HE9Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e10 checked
        /// </summary>
        private bool mHE10Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e10 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e10 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE10Checked
        {
            get
            {
                return mHE10Checked;
            }
            set
            {
                mHE10Checked = value;
                RaisePropertyChanged("HE10Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e11 checked
        /// </summary>
        private bool mHE11Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e11 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e11 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE11Checked
        {
            get
            {
                return mHE11Checked;
            }
            set
            {
                mHE11Checked = value;
                RaisePropertyChanged("HE11Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e12 checked
        /// </summary>
        private bool mHE12Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e12 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e12 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE12Checked
        {
            get
            {
                return mHE12Checked;
            }
            set
            {
                mHE12Checked = value;
                RaisePropertyChanged("HE12Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e13 checked
        /// </summary>
        private bool mHE13Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e13 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e13 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE13Checked
        {
            get
            {
                return mHE13Checked;
            }
            set
            {
                mHE13Checked = value;
                RaisePropertyChanged("HE13Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e14 checked
        /// </summary>
        private bool mHE14Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e14 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e14 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE14Checked
        {
            get
            {
                return mHE14Checked;
            }
            set
            {
                mHE14Checked = value;
                RaisePropertyChanged("HE14Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e15 checked
        /// </summary>
        private bool mHE15Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e15 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e15 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE15Checked
        {
            get
            {
                return mHE15Checked;
            }
            set
            {
                mHE15Checked = value;
                RaisePropertyChanged("HE15Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e16 checked
        /// </summary>
        private bool mHE16Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e16 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e16 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE16Checked
        {
            get
            {
                return mHE16Checked;
            }
            set
            {
                mHE16Checked = value;
                RaisePropertyChanged("HE16Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e17 checked
        /// </summary>
        private bool mHE17Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e17 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e17 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE17Checked
        {
            get
            {
                return mHE17Checked;
            }
            set
            {
                mHE17Checked = value;
                RaisePropertyChanged("HE17Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e18 checked
        /// </summary>
        private bool mHE18Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e18 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e18 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE18Checked
        {
            get
            {
                return mHE18Checked;
            }
            set
            {
                mHE18Checked = value;
                RaisePropertyChanged("HE18Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e19 checked
        /// </summary>
        private bool mHE19Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e19 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e19 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE19Checked
        {
            get
            {
                return mHE19Checked;
            }
            set
            {
                mHE19Checked = value;
                RaisePropertyChanged("HE19Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e20 checked
        /// </summary>
        private bool mHE20Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e20 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e20 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE20Checked
        {
            get { return mHE20Checked; }
            set
            {
                mHE20Checked = value;
                RaisePropertyChanged("HE20Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e21 checked
        /// </summary>
        private bool mHE21Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e21 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e21 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE21Checked
        {
            get { return mHE21Checked; }
            set
            {
                mHE21Checked = value;
                RaisePropertyChanged("HE21Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e22 checked
        /// </summary>
        private bool mHE22Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e22 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e22 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE22Checked
        {
            get
            {
                return mHE22Checked;
            }
            set
            {
                mHE22Checked = value;
                RaisePropertyChanged("HE22Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e23 checked
        /// </summary>
        private bool mHE23Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e23 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e23 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE23Checked
        {
            get
            {
                return mHE23Checked;
            }
            set
            {
                mHE23Checked = value;
                RaisePropertyChanged("HE23Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m h e24 checked
        /// </summary>
        private bool mHE24Checked = true;
        /// <summary>
        /// Gets or sets a value indicating whether [h e24 checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [h e24 checked]; otherwise, <c>false</c>.
        /// </value>
        public bool HE24Checked
        {
            get
            {
                return mHE24Checked;
            }
            set
            {
                mHE24Checked = value;
                RaisePropertyChanged("HE24Checked");
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m filter day comparison totals checked
        /// </summary>
        private bool mFilterDayComparisonTotalsChecked;
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
                UpdateChartCommand();
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
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m filter day comparison win PCT checked
        /// </summary>
        private bool mFilterDayComparisonWinPctChecked;
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
                UpdateChartCommand();
            }
        }
        /// <summary>
        /// The m filter day comparison risk checked
        /// </summary>
        private bool mFilterDayComparisonRiskChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison risk checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison risk checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FilterDayComparisonRiskChecked
        {
            get { return mFilterDayComparisonRiskChecked; }
            set
            {
                mFilterDayComparisonRiskChecked = value;
                RaisePropertyChanged("FilterDayComparisonRiskChecked");
                UpdateChartCommand();
            }
        }
        private bool mFilterDayComparisonMedianChecked;
        public bool FilterDayComparisonMedianChecked
        {
            get { return mFilterDayComparisonMedianChecked; }
            set
            {
                mFilterDayComparisonMedianChecked = value;
                RaisePropertyChanged("FilterDayComparisonMedianChecked");
                UpdateChartCommand();
            }
        }

        private bool mFilterDayComparisonStdDevChecked;
        public bool FilterDayComparisonStdDevChecked
        {
            get { return mFilterDayComparisonStdDevChecked; }
            set
            {
                mFilterDayComparisonStdDevChecked = value;
                RaisePropertyChanged("FilterDayComparisonStdDevChecked");
                UpdateChartCommand();
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
            get { return mFilterDayComparisonMinChecked; }
            set
            {
                mFilterDayComparisonMinChecked = value;
                RaisePropertyChanged("FilterDayComparisonMinChecked");
                UpdateChartCommand();
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
                UpdateChartCommand();
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
                RaisePropertyChanged("FilterDayComparisonRiskRewardChecked");
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
        /// The m product combo selected value
        /// </summary>
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
                RaisePropertyChanged("MarketComboSelectedValue");
                if (mMarketComboSelectedValue == "PJM")
                {
                    List<string> tList = new List<string>();
                    tList.Add("LMP");
                    tList.Add("Energy");
                    tList.Add("Congestion");
                    tList.Add("Loss");
                    PriceTypeList = tList;
                    SelectedType = "LMP";
                }
                else
                {
                    List<string> tList = new List<string>();
                    tList.Add("LMP");
                    PriceTypeList = tList;
                    SelectedType = "LMP";
                }
                if (mMarketComboSelectedValue == "MISO" || mMarketComboSelectedValue == "CAISO" || mMarketComboSelectedValue == "NYISO" || mMarketComboSelectedValue == "SPP")
                {
                    UptosChecked = false;
                }
                else
                {
                    UptosChecked = true;
                }
                //SetSourceSink();
                SetSpreadComboBox();
                SetTypes();
            }
        }
        /// <summary>
        /// Sets the spread ComboBox.
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
        /// The m filter temperature combo selected value
        /// </summary>
        private string mFilterTemperatureComboSelectedValue;
        /// <summary>
        /// Gets or sets the filter temperature combo selected value.
        /// </summary>
        /// <value>
        /// The filter temperature combo selected value.
        /// </value>
        public string FilterTemperatureComboSelectedValue
        {
            get
            {
                return mFilterTemperatureComboSelectedValue;
            }
            set
            {
                mFilterTemperatureComboSelectedValue = value;
                RaisePropertyChanged("FilterTemperatureComboSelectedValue");
            }
        }
        /// <summary>
        /// The m filter load combo selected value
        /// </summary>
        private string mFilterLoadComboSelectedValue;
        /// <summary>
        /// Gets or sets the filter load combo selected value.
        /// </summary>
        /// <value>
        /// The filter load combo selected value.
        /// </value>
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
        /// <summary>
        /// The m filter price combo selected value
        /// </summary>
        private string mFilterPriceComboSelectedValue;
        /// <summary>
        /// Gets or sets the filter price combo selected value.
        /// </summary>
        /// <value>
        /// The filter price combo selected value.
        /// </value>
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
                    // RetrieveFetchDataAndUpdateChartCommand();
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

        private bool mDeenergizedSourceChecked;
        public bool DeenergizedSourceChecked
        {
            get
            {
                return mDeenergizedSourceChecked;
            }
            set
            {
                mDeenergizedSourceChecked = value;
                RaisePropertyChanged("DeenergizedSourceChecked");
            }
        }
        private bool mDeenergizedSinkChecked;
        public bool DeenergizedSinkChecked
        {
            get
            {
                return mDeenergizedSinkChecked;
            }
            set
            {
                mDeenergizedSinkChecked = value;
                RaisePropertyChanged("DeenergizedSinkChecked");
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
        /// 
        private DateTime mBStartDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime BStartDate
        {
            get
            {
                return mBStartDate;
            }
            set
            {
                mBStartDate = value.Date;                            //.AddHours(1);
                RaisePropertyChanged("BStartDate");
            }
        }

        private DateTime mBEndDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime BEndDate
        {
            get
            {
                return mBEndDate;
            }
            set
            {
                mBEndDate = value.Date;                            //.AddHours(1);
                RaisePropertyChanged("BEndDate");
            }
        }
        private DateTime mCStartDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime CStartDate
        {
            get
            {
                return mCStartDate;
            }
            set
            {
                mCStartDate = value.Date;                            //.AddHours(1);
                RaisePropertyChanged("CStartDate");
            }
        }

        private DateTime mCEndDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime CEndDate
        {
            get
            {
                return mCEndDate;
            }
            set
            {
                mCEndDate = value.Date;                            //.AddHours(1);
                RaisePropertyChanged("CEndDate");
            }
        }
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
                mStartDate = value.Date;                            //.AddHours(1);
                RaisePropertyChanged("StartDate");
                if (mStartDate < mEndDate)
                {
                    DataService ds = new DataService();
                    ploadDictHash = ds.PGetLoadsData(mStartDate, mEndDate);
                    eloadDictHash = ds.EGetLoadsData(mStartDate, mEndDate);
                }
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
                if (mStartDate < mEndDate)
                {
                    DataService ds = new DataService();
                    ploadDictHash = ds.PGetLoadsData(mStartDate, mEndDate);
                    eloadDictHash = ds.EGetLoadsData(mStartDate, mEndDate);
                }
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
        /// Sets the source sink.
        /// </summary>
        public void SetSourceSink()
        {
            string product = "UPTO";
            if (!UptosChecked)
            {
                if (SelectedPath)
                {
                    product = "BOTH";
                }
                else
                {
                    product = "VIRTUAL";
                }
            }
            DBAccess.GetSourceSinkNodeList(
                    (item1, error) =>
                    {
                        SourceNodeList = item1.Item1;
                        SinkNodeList = item1.Item2;
                    }, MarketComboSelectedValue, product);
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
        /// The m filter price combo list
        /// </summary>
        private List<String> mFilterPriceComboList;
        /// <summary>
        /// Gets or sets the filter price combo list.
        /// </summary>
        /// <value>
        /// The filter price combo list.
        /// </value>
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
        /// <summary>
        /// The m filter list
        /// </summary>
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
                UpdateChartCommand();
            }
        }

        private List<FuelTypeList> sListFuelTypes;
        public List<FuelTypeList> ListFuelTypes
        {
            get
            {
                return sListFuelTypes;
            }
            set
            {
                sListFuelTypes = value;
                RaisePropertyChanged("ListFuelTypes");
            }
        }
        /// <summary>
        /// The m source sink list
        /// </summary>
        private List<SourceSinkData> mSourceSinkList;
        /// <summary>
        /// Gets or sets the source sink list.
        /// </summary>
        /// <value>
        /// The source sink list.
        /// </value>
        public List<SourceSinkData> SourceSinkList
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
        /// The m hourly list
        /// </summary>
        private List<HourlyData> mHourlyList;
        /// <summary>
        /// Gets or sets the hourly list.
        /// </summary>
        /// <value>
        /// The hourly list.
        /// </value>
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
        /// <summary>
        /// The m hourly pivot list
        /// </summary>
        private List<HourlyPivotData> mHourlyPivotList;
        /// <summary>
        /// Gets or sets the hourly pivot list.
        /// </summary>
        /// <value>
        /// The hourly pivot list.
        /// </value>
        public List<HourlyPivotData> HourlyPivotList
        {
            get
            {
                return mHourlyPivotList;
            }
            set
            {
                mHourlyPivotList = value;
                RaisePropertyChanged("HourlyPivotList");
            }
        }

        private HourlyPivotData mSelectedItemDayComp;
        /// <summary>
        /// Gets or sets the hourly pivot list.
        /// </summary>
        /// <value>
        /// The hourly pivot list.
        /// </value>
        public HourlyPivotData SelectedItemDayComp
        {
            get
            {
                return mSelectedItemDayComp;
            }
            set
            {
                mSelectedItemDayComp = value;
                RaisePropertyChanged("SelectedItemDayComp");
            }
        }

        private List<HourlyPivotDataCongestion> mHourlyPivotListCongestion;
        public List<HourlyPivotDataCongestion> HourlyPivotListCongestionSource
        {
            get
            {
                return mHourlyPivotListCongestion;
            }
            set
            {
                mHourlyPivotListCongestion = value;
                RaisePropertyChanged("HourlyPivotListCongestionSource");
            }
        }

        private List<HourlyPivotDataCongestion> mHourlyPivotListCongestionSink;
        public List<HourlyPivotDataCongestion> HourlyPivotListCongestionSink
        {
            get
            {
                return mHourlyPivotListCongestionSink;
            }
            set
            {
                mHourlyPivotListCongestionSink = value;
                RaisePropertyChanged("HourlyPivotListCongestionSink");
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
                PlotUpperAndLowerChartsAndSetHourlyList();
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
        /// The m fee
        /// </summary>
        private double mFee;
        /// <summary>
        /// Gets or sets the fee.
        /// </summary>
        /// <value>
        /// The fee.
        /// </value>
        public double Fee
        {
            get
            {
                return mFee;
            }
            set
            {
                mFee = value;
                RaisePropertyChanged("Fee");
            }
        }

        /// <summary>
        /// The m PNL sum checked
        /// </summary>
        private bool mPnlSumChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [PNL sum checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [PNL sum checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PnlSumChecked
        {
            get
            {
                return mPnlSumChecked;
            }
            set
            {
                mPnlSumChecked = value;
                Simulate();
                RaisePropertyChanged("PnlSumChecked");
            }
        }

        /// <summary>
        /// The m win checked
        /// </summary>
        private bool mWinChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [win checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [win checked]; otherwise, <c>false</c>.
        /// </value>
        public bool WinChecked
        {
            get
            {
                return mWinChecked;
            }
            set
            {
                mWinChecked = value;
                Simulate();
                RaisePropertyChanged("WinChecked");
            }
        }

        /// <summary>
        /// The m risk checked
        /// </summary>
        private bool mRiskChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [risk checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [risk checked]; otherwise, <c>false</c>.
        /// </value>
        public bool RiskChecked
        {
            get
            {
                return mRiskChecked;
            }
            set
            {
                mRiskChecked = value;
                Simulate();
                RaisePropertyChanged("RiskChecked");
            }
        }
        /// <summary>
        /// The m cleared checked
        /// </summary>
        private bool mClearedChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [cleared checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cleared checked]; otherwise, <c>false</c>.
        /// </value>
        public bool ClearedChecked
        {
            get
            {
                return mClearedChecked;
            }
            set
            {
                mClearedChecked = value;
                Simulate();
                RaisePropertyChanged("ClearedChecked");
            }
        }
        /// <summary>
        /// The price type list
        /// </summary>
        private List<string> priceTypeList;
        /// <summary>
        /// Gets or sets the price type list.
        /// </summary>
        /// <value>
        /// The price type list.
        /// </value>
        public List<string> PriceTypeList
        {
            get
            {
                return priceTypeList;
            }
            set
            {
                priceTypeList = value; RaisePropertyChanged("PriceTypeList");
            }
        }
        /// <summary>
        /// The selected type
        /// </summary>
        private string selectedType;
        /// <summary>
        /// Gets or sets the type of the selected.
        /// </summary>
        /// <value>
        /// The type of the selected.
        /// </value>
        public string SelectedType
        {
            get
            {
                return selectedType;
            }
            set
            {
                selectedType = value;
                SetSourceSink();
                SetSimMinMax();
                PlotUpperAndLowerChartsAndSetHourlyList();
                RetrieveFetchDataAndUpdateChartCommand();
                RaisePropertyChanged("SelectedType");
            }
        }
        private List<LmpCorelation> mDetailList;
        /// <summary>
        /// Gets or sets the PNL list.
        /// </summary>
        /// <value>
        /// The PNL list.
        /// </value>
        public List<LmpCorelation> DetailList
        {
            get
            {
                return mDetailList;
            }
            set
            {
                mDetailList = value;
                RaisePropertyChanged("DetailList");
            }
        }
        private DateTime mEPriceStartDate = DateTime.Now.AddDays(1 - (DateTime.Now.Day));
        public DateTime EPriceStartDate
        {
            get
            {
                return mEPriceStartDate;
            }
            set
            {
                mEPriceStartDate = value;
                RaisePropertyChanged("EPriceStartDate");
                SourceSinkEnergyPriceList = null;

            }
        }
        DateTime mEPriceEndDate = DateTime.Now;
        public DateTime EPriceEndDate
        {
            get
            {
                return mEPriceEndDate;
            }
            set
            {
                if (value != new DateTime() && value > DateTime.Today)
                {
                    mEPriceEndDate = DateTime.Today;
                }
                else
                {
                    mEPriceEndDate = value;
                }
                RaisePropertyChanged("EPriceEndDate");
                SourceSinkEnergyPriceList = null;

            }
        }

        private DateTime mTdStartDate = DateTime.Now.AddDays(1 - (DateTime.Now.Day));
        public DateTime TdStartDate
        {
            get
            {
                return mTdStartDate;
            }
            set
            {
                mTdStartDate = value;
                RaisePropertyChanged("TdStartDate");
                SourceTradedVolumesList = null;
                SinkTradedVolumesList = null;
            }
        }
        private DateTime mTdEndDate = DateTime.Now;
        public DateTime TdEndDate
        {
            get
            {
                return mTdEndDate;
            }
            set
            {
                if (value != new DateTime() && value > DateTime.Today)
                {
                    mTdEndDate = DateTime.Today;
                }
                else
                {
                    mTdEndDate = value;
                }
                RaisePropertyChanged("TdEndDate");
                SourceTradedVolumesList = null;
                SinkTradedVolumesList = null;
            }
        }
        private bool mTrededSourceChecked;
        public bool TrededSourceChecked
        {
            get
            {
                return mTrededSourceChecked;
            }
            set
            {
                mTrededSourceChecked = value;
                RaisePropertyChanged("TrededSourceChecked");
                if (TrededSourceChecked)
                {
                    TrededSinkChecked = false;
                    SourceTradedVolumesList = null;
                    SinkTradedVolumesList = null;
                }
            }
        }
        private bool mTrededSinkChecked;
        public bool TrededSinkChecked
        {
            get
            {
                return mTrededSinkChecked;
            }
            set
            {
                mTrededSinkChecked = value;
                RaisePropertyChanged("TrededSinkChecked");
                if (TrededSinkChecked)
                {
                    TrededSourceChecked = false;
                    SourceTradedVolumesList = null;
                    SinkTradedVolumesList = null;
                }
            }
        }

        List<DeenergizedNode> ListDeenergizedNodes = new List<DeenergizedNode>();

        private List<TradedVolumesData> mSourceTradedVolumesList;
        public List<TradedVolumesData> SourceTradedVolumesList
        {
            get { return mSourceTradedVolumesList; }
            set
            {
                mSourceTradedVolumesList = value;
                RaisePropertyChanged("SourceTradedVolumesList");
            }
        }
        //
        private List<TradedVolumesData> mSinkTradedVolumesList;
        public List<TradedVolumesData> SinkTradedVolumesList
        {
            get { return mSinkTradedVolumesList; }
            set
            {
                mSinkTradedVolumesList = value;
                RaisePropertyChanged("SinkTradedVolumesList");
            }
        }
        //
        private List<TradedVolumesData> mTradedVolumesList;
        public List<TradedVolumesData> TradedVolumesList
        {
            get { return mTradedVolumesList; }
            set
            {
                mTradedVolumesList = value;
                RaisePropertyChanged("TradedVolumesList");
            }
        }
        private List<TradedVolumesData> mSinkVolumesList;
        public List<TradedVolumesData> SinkVolumesList
        {
            get { return mSinkVolumesList; }
            set
            {
                mSinkVolumesList = value;
                RaisePropertyChanged("SinkVolumesList");
            }
        }

        private List<TradedVolumesData> mSourceSinkEnergyPriceList;
        public List<TradedVolumesData> SourceSinkEnergyPriceList
        {
            get { return mSourceSinkEnergyPriceList; }
            set
            {
                mSourceSinkEnergyPriceList = value;
                RaisePropertyChanged("SourceSinkEnergyPriceList");
            }
        }
        public List<string> deleteddatelist = new List<string>();
        #region Relay Command Properties

        /// <summary>
        /// Gets or sets the run retrieve fetch data and update chart command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data and update chart command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataAndUpdateChartCommand { private set; get; }
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
        public DelegateCommand AddLoadCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add temperature command.
        /// </summary>
        /// <value>
        /// The add temperature command.
        /// </value>
        public DelegateCommand AddTemperatureCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add source sink command.
        /// </summary>
        /// <value>
        /// The add source sink command.
        /// </value>
        public DelegateCommand AddSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public DelegateCommand ResetCommand { private set; get; }
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
        /// Gets or sets the swap command.
        /// </summary>
        /// <value>
        /// The swap command.
        /// </value>
        public DelegateCommand SwapCommand { private set; get; }
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
        /// Gets or sets the LMP command.
        /// </summary>
        /// <value>
        /// The LMP command.
        /// </value>
        public DelegateCommand LmpCommand { private set; get; }
        /// <summary>
        /// Gets or sets the clear check boxes command.
        /// </summary>
        /// <value>
        /// The clear check boxes command.
        /// </value>
        public DelegateCommand ClearCheckBoxesCommand { private set; get; }
        /// <summary>
        /// Gets or sets the check all check boxes command.
        /// </summary>
        /// <value>
        /// The check all check boxes command.
        /// </value>
        public DelegateCommand CheckAllCheckBoxesCommand { private set; get; }
        /// <summary>
        /// Gets or sets the check peak check boxes command.
        /// </summary>
        /// <value>
        /// The check peak check boxes command.
        /// </value>
        public DelegateCommand CheckPeakCheckBoxesCommand { private set; get; }
        /// <summary>
        /// Gets or sets the check off peak check boxes command.
        /// </summary>
        /// <value>
        /// The check off peak check boxes command.
        /// </value>
        public DelegateCommand CheckOffPeakCheckBoxesCommand { private set; get; }
        /// <summary>
        /// Gets or sets the paste command.
        /// </summary>
        /// <value>
        /// The paste command.
        /// </value>
        public DelegateCommand PasteCommand { private set; get; }
        public DelegateCommand LoadCommand { private set; get; }
        /// <summary>
        /// Gets or sets the simulate command.
        /// </summary>
        /// <value>
        /// The simulate command.
        /// </value>
        public DelegateCommand SimulateCommand { private set; get; }
        public DelegateCommand BollingerCommand { private set; get; }
        public DelegateCommand CorrelationsCommand { private set; get; }

        public DelegateCommand RunRefreshCommand { private set; get; }

        public DelegateCommand RunRefreshEPriceCommand { private set; get; }
        //  public DelegateCommand PasteCommand { private set; get; }
        public DelegateCommand ExportCommand { private set; get; }
        public DelegateCommand ExportEnergyPricesCommand { private set; get; }
        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            HourlyPivotListSummary = null;
            HourlyPivotList = null;
            HourlyList = null;
            PlotSimulation = null;
            DetailList = null;
        }

        private string mDeenergizedHourlist;
        public string DeenergizedHourList
        {
            get
            {
                return mDeenergizedHourlist;
            }
            set
            {
                mDeenergizedHourlist = value;

                RaisePropertyChanged("DeenergizedHourList");
            }
        }


        /// <summary>
        /// Sorts the specified da list.
        /// </summary>
        /// <param name="daList">The da list.</param>
        /// <param name="rtList">The rt list.</param>
        /// <param name="dartList">The dart list.</param>
        public void Sort(List<Node> daList, List<Node> rtList, List<Node> dartList)
        {
            List<Node> sortList = null;
            switch (SelectedSortType)
            {
                case SortType.DA:
                    sortList = daList;
                    break;
                case SortType.RT:
                    sortList = rtList;
                    break;
                case SortType.DART:
                    sortList = dartList;
                    break;
                case SortType.Date:
                    if (SelectedSpreadType == SpreadType.DA)
                    {
                        sortList = daList;
                    }
                    else
                    {
                        sortList = dartList;
                    }
                    break;
            }
            if (sortList.Count == 0)
            {
                return;
            }
            if (SelectedSortType == SortType.Date)
            {
                foreach (List<Node> nodelist in new List<List<Node>>() { daList, rtList, dartList })
                {
                    foreach (Node node in nodelist)
                    {
                        List<LmpTimePrice> timePriceList = node.LmpTimePriceList.OrderBy(x => x.MarketTime).ToList<LmpTimePrice>();
                        node.LmpTimePriceList = timePriceList;
                    }
                }
            }
            else if (SelectedSortType != SortType.Date)
            {
                Dictionary<double, List<DateTime>> sortHash = new Dictionary<double, List<DateTime>>();
                foreach (LmpTimePrice timePrice in sortList[sortList.Count - 1].LmpTimePriceList)
                {
                    List<DateTime> dateList = new List<DateTime>();
                    if (sortHash.ContainsKey(timePrice.Lmp.Price))
                    {
                        dateList = sortHash[timePrice.Lmp.Price];
                        sortHash.Remove(timePrice.Lmp.Price);
                    }
                    dateList.Add(timePrice.MarketTime);
                    sortHash.Add(timePrice.Lmp.Price, dateList);
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
                        if (SelectedSpreadType != SpreadType.RT
                            || (SelectedSpreadType == SpreadType.RT && rtList[rtList.Count - 1].LmpTimePriceList.Any(x => x.MarketTime == date)
                            && !rtList[rtList.Count - 1].LmpTimePriceList.First(x => x.MarketTime == date).Lmp.Price.Equals(double.NaN)))
                        {
                            sortDateList.Add(date);
                        }
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    List<Node> nodeList = new List<Node>();
                    if (i == 1)
                    {
                        nodeList = rtList;
                    }
                    if (i == 2)
                    {
                        nodeList = dartList;
                    }
                    if (i == 0)
                    {
                        nodeList = daList;
                    }
                    foreach (Node node in nodeList)
                    {
                        List<LmpTimePrice> timePriceList = node.LmpTimePriceList;
                        Dictionary<DateTime, LmpTimePrice> dateHash = new Dictionary<DateTime, LmpTimePrice>();
                        foreach (LmpTimePrice timePrice in timePriceList)
                        {
                            dateHash.Add(timePrice.MarketTime, timePrice);
                        }
                        List<LmpTimePrice> tempPriceList = new List<LmpTimePrice>();
                        foreach (DateTime date in sortDateList)
                        {
                            if (dateHash.ContainsKey(date))
                            {
                                tempPriceList.Add(dateHash[date]);
                            }
                        }
                        node.LmpTimePriceList = tempPriceList;
                    }
                }
            }
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
            if (sourceSinkData.Sink == null)
            {
                sourceSinkKey = sourceSinkData.Source.NodeKey.ToString();
                int index = 0;
                foreach (SourceSinkData sourceSink in mSourceSinkList)
                {
                    if (sourceSink.Source.NodeKey == sourceSinkData.Source.NodeKey && sourceSink.Sink == null)
                    {
                        break;
                    }
                    index++;
                }
                if (mSourceSinkList.Count > 0 && index < mSourceSinkList.Count)
                {
                    mSourceSinkList.RemoveAt(index);
                }
            }
            else
            {
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
            }
            if (mFillSourceSinkHash.ContainsKey(sourceSinkKey))
            {
                mFillSourceSinkHash.Remove(sourceSinkKey);
            }
            SourceSinkList = null;
            SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();

            TransposedFuelTypes.Clear();
            Clear();
        }
        /// <summary>
        /// Removes all source sink.
        /// </summary>
        public void RemoveAllSourceSink()
        {
            mSourceSinkDataList = new List<SourceSinkData>();
            mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
            DeenergizedHourList = null;
            SourceSinkList = null;
            TransposedFuelTypes.Clear();

            Clear();
        }
        /// <summary>
        /// Swaps this instance.
        /// </summary>
        public void Swap()
        {
            try
            {
                SourceSinkData sourceSinkData = SourceSinkDataSelected;
                if (sourceSinkData == null)
                {
                    return;
                }
                if (SourceSinkDataSelected.Sink != null)
                {
                    mIsSwap = true;
                    PricingNode source = sourceSinkData.Source;
                    PricingNode sink = sourceSinkData.Sink;
                    RemoveSourceSink();
                    SourceComboSelectedItem = sink;
                    SinkComboSelectedItem = source;
                    AddSourceSink();
                    RetrieveFetchDataAndUpdateChartCommand();
                    mIsSwap = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            RemoveAllSourceSink();
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
        /// <summary>
        /// Adds the source sink.
        /// </summary>
        public void AddSourceSink()
        {
            try
            {

                if (SourceComboSelectedItem != null && (!UptosChecked || SinkComboSelectedItem != null))
                {
                    SourceSinkData sourceSinkData = new SourceSinkData();
                    sourceSinkData.Source = SourceComboSelectedItem;
                    sourceSinkData.Sink = (!UptosChecked && !mIsSwap && !SelectedPath) ? null : SinkComboSelectedItem;

                    mDataService.GetDeenergizedNodesList((a, e) =>
                    {
                        if (e == null)
                        {
                            if (a != null && a.Count > 0)
                            {
                                DeenergizedNodesList = a;
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(e.Message);
                        }
                    });

                    rtprints = mDataService.GetRTPrints(StartDate, EndDate.AddDays(1), sourceSinkData);
                    //mDataService.GetInValidPathNodesList((a, e) =>
                    //{
                    //    if (e == null)
                    //    {
                    //        if (a != null && a.Count > 0)
                    //        {
                    //            InValidPathNodesList = a;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        System.Windows.MessageBox.Show(e.Message);
                    //    }
                    //});
                    // if (DeenergizedNodesList.Contains(sourceSinkData.Source.NodeName))
                    {
                        string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                               sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                        if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                        {
                            mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                            SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                            SourceSinkDataSelected = sourceSinkData;
                        }
                        SourceComboSelectedItem = null;
                        SinkComboSelectedItem = null;
                    }
                    //else
                    //{
                    //    MessageBox.Show("You have selected DeenergizedNodes as Source", "Deenergized Source");
                    //    string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                    //                           sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                    //    if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                    //    {
                    //        mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                    //        SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                    //        SourceSinkDataSelected = sourceSinkData;
                    //    }
                    //    SourceComboSelectedItem = null;
                    //    SinkComboSelectedItem = null;
                    //}
                    if (sourceSinkData.Sink != null)
                    {
                        //if (DeenergizedNodesList.Contains(sourceSinkData.Sink.NodeName))
                        {
                            string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                                   sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                            if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                            {
                                mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                                SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                                SourceSinkDataSelected = sourceSinkData;
                            }
                            SourceComboSelectedItem = null;
                            SinkComboSelectedItem = null;
                        }
                        //else
                        //{
                        //    MessageBox.Show("You have selected DeenergizedNodes as Sink", "Deenergized Sink");
                        //    string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                        //                           sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                        //    if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                        //    {
                        //        mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                        //        SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                        //        SourceSinkDataSelected = sourceSinkData;
                        //    }
                        //    SourceComboSelectedItem = null;
                        //    SinkComboSelectedItem = null;
                        //}
                    }

                    //if (sourceSinkData.Sink != null)
                    //{
                    //    if ((InValidPathNodesList.Exists(a => a.Source == sourceSinkData.Source.NodeName && a.Sink == sourceSinkData.Sink.NodeName)))
                    //    {
                    //        MessageBox.Show(String.Format("You have selected InValid Path as Source : {0} and Sink : {1}.", sourceSinkData.Source.NodeName.ToString(), sourceSinkData.Sink.NodeName).ToString(), "InValid Path");
                    //    }
                    //}
                    //else
                    //{
                    //    if ((InValidPathNodesList.Exists(a => a.Source == sourceSinkData.Source.NodeName)))
                    //    {
                    //        MessageBox.Show(String.Format("You have selected InValid Source : {0}.", sourceSinkData.Source.NodeName.ToString(), "InValid Source"));
                    //    }
                    //}

                    string tempSourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                               sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                    if (!mFillSourceSinkHash.ContainsKey(tempSourceSinkKey))
                    {
                        mFillSourceSinkHash.Add(tempSourceSinkKey, sourceSinkData);
                        SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                        SourceSinkDataSelected = sourceSinkData;
                    }

                    FetchDeEnergeziedHourList();
                    SourceComboSelectedItem = null;
                    SinkComboSelectedItem = null;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void GetDeenergizedNodes(string Source, string Sink)
        {
            try
            {
                List<DeenergizedNode> filterSourceNodes = new List<DeenergizedNode>();
                List<DeenergizedNode> filterSinkNodes = new List<DeenergizedNode>();
                if (ListDeenergizedNodes.Count > 0)
                {
                    filterSourceNodes = ListDeenergizedNodes.Where(a => a.Name == Source).ToList();
                    filterSinkNodes = ListDeenergizedNodes.Where(a => a.Name == Sink).ToList();
                    if (filterSourceNodes.Count > 0)
                        DeenergizedSourceChecked = true;
                    else
                        DeenergizedSourceChecked = false;
                    if (filterSinkNodes.Count > 0)
                        DeenergizedSinkChecked = true;
                    else
                        DeenergizedSinkChecked = false;
                }
                else
                {
                    ListDeenergizedNodes = mDataService.GetDeenergizedNodes();
                    filterSourceNodes = ListDeenergizedNodes.Where(a => a.Name == Source).ToList();
                    filterSinkNodes = ListDeenergizedNodes.Where(a => a.Name == Sink).ToList();
                    if (filterSourceNodes.Count > 0)
                        DeenergizedSourceChecked = true;
                    else
                        DeenergizedSourceChecked = false;
                    if (filterSinkNodes.Count > 0)
                        DeenergizedSinkChecked = true;
                    else
                        DeenergizedSinkChecked = false;
                }
            }
            catch
            {

            }
        }
        public class PropertyValue
        {
            public string Node { get; set; }      
            public string Zone { get; set; }       
            public string FuelType { get; set; }   
        }

        public ObservableCollection<PropertyValue> TransposedFuelTypes { get; set; } = new ObservableCollection<PropertyValue>();
        public void GetFuelTypes(string Source, string Sink)
        {
            try
            {
                string sourceFuel = null;
                string sinkFuel = null;

                var fTypes = mDataService.GetSourceSinkFuelTypes();

                foreach (FuelType item in fTypes)
                {
                    if (item.Name == Source)
                    {
                        sourceFuel = item.FuelTypes;
                    }
                    if (item.Name == Sink)
                    {
                        sinkFuel = item.FuelTypes;
                    }
                }

                // Get zone and node info from the selected source/sink
                SourceSinkData sourceSinkData = SourceSinkDataSelected;

                string sourceNode = sourceSinkData.Source?.ToString();
                string sinkNode = sourceSinkData.Sink?.ToString();

                string sourceZone = sourceSinkData.Source?.Zone;
                string sinkZone = sourceSinkData.Sink?.Zone;

                // Clear and update TransposedFuelTypes collection
                TransposedFuelTypes.Clear();
                TransposedFuelTypes.Add(new PropertyValue
                {
                    Node = sourceNode,
                    Zone = sourceZone,
                    FuelType = sourceFuel
                });
                TransposedFuelTypes.Add(new PropertyValue
                {
                    Node = sinkNode,
                    Zone = sinkZone,
                    FuelType = sinkFuel
                });

                // Optional: clear ListFuelTypes if no longer needed
                ListFuelTypes = null;
            }
            catch (Exception ex)
            {
                // Log or handle error appropriately
            }
        }

        /// <summary>
        /// Adds the filters.
        /// </summary>
        public void AddFilters()
        {
            try
            {
                FilterList = null;
                string filterType = TypeComboSelectedValue;
                double? max = null;
                try
                {
                    max = double.Parse(MaxText);
                }
                catch (Exception ex)
                { }
                double? min = null;
                try
                {
                    min = double.Parse(MinText);
                }
                catch (Exception ex)
                { }

                FilterData filterData = new FilterData();
                filterData.Product = ProductComboSelectedValue;
                filterData.Type = filterType;
                filterData.Min = min;
                filterData.Max = max;
                mFillFilterList.Add(filterData);
                FilterList = mFillFilterList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Removes the filters.
        /// </summary>
        public void RemoveFilters()
        {
            try
            {
                FilterData filterData = FilterDataSelected;
                mFillFilterList.Remove(filterData);
                FilterList = null;
                FilterList = mFillFilterList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Removes all filters.
        /// </summary>
        public void RemoveAllFilters()
        {
            mFillFilterList = new List<FilterData>();
            FilterList = null;
        }
        /// <summary>
        /// Selects the source sink data & updates the chart.
        /// </summary>
        public void SelectSourceSinkFetchDataUpdateChart()
        {
            try
            {
                bool refreshData = false;
                FetchAllSourceSinkData(refreshData);
                UpdateChartCommand();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
            }
        }

        public void DeleteDateAndUpdateCommand()
        {
            List<HourlyPivotData> TempList = HourlyPivotList.ToList();
            if (SelectedItemDayComp != null)
            {
                deleteddatelist.Add(SelectedItemDayComp.Date.ToString());
                TempList.Remove(SelectedItemDayComp);
                HourlyPivotList = null;
                HourlyPivotList = TempList;
                PlotUpperAndLowerChartsAndSetHourlyList();
                SetHourlyPivotList(mHourlyPivotHash);
            }
            if (TempList != null)
            {

            }
        }

        public void OpenConstraintsWindow()
        {
            DataService ds = new DataService();
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            if (sourceSinkData == null)
            {
                MessageBox.Show("Please select Source and Sink First");
                return;
            }
            LMPStatistics.Views.Constraint window = new LMPStatistics.Views.Constraint();
            window.DataContext = new LMPStatistics.ViewModels.ConstraintViewModel(sourceSinkData, StartDate, EndDate);
            window.Show();
        }

        /// <summary>
        /// Retrieves the data and updates chart.
        /// </summary>
        public void RetrieveFetchDataAndUpdateChartCommand()
        {
            bool refreshData = true;
            deleteddatelist.Clear();
            Task.Factory.StartNew(() => { FetchAllSourceSinkData(refreshData); });
            Task.Factory.StartNew(() => { UpdateChartCommand(); });
        }
        //Fetch Today DeEnergized Hours for  source and sink
        public void FetchDeEnergeziedHourList()
        {
            int cnt = SourceSinkList.Count();
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            if (sourceSinkData == null)
            {
                return;
            }
            else
            {

                DeenergizedHourList = mDataService.GetDeenergizedHourList(sourceSinkData.Source.ToString(), sourceSinkData.Sink.ToString());

            }
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
            rtprints = mDataService.GetRTPrints(StartDate, EndDate.AddDays(1), sourceSinkData);
            string existingPathKey = "";
            if (CachedNodeListHashCheckKeyExists(sourceSinkData, out existingPathKey))
            {
                try
                {
                    DateTime cachedEndDateDA = mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.DA, (int)DataFilteredType.Orig][0].LmpTimePriceList.Max(x => x.MarketTime);
                    DateTime cachedEndDateRT = mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.RT, (int)DataFilteredType.Orig][0].LmpTimePriceList.Max(x => x.MarketTime);
                    DateTime cachedEndDateDART = mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.DART, (int)DataFilteredType.Orig][0].LmpTimePriceList.Max(x => x.MarketTime);
                    DateTime cachedStartDateDA = mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.DA, (int)DataFilteredType.Orig][0].LmpTimePriceList.Min(x => x.MarketTime);
                    DateTime cachedStartDateRT = mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.RT, (int)DataFilteredType.Orig][0].LmpTimePriceList.Min(x => x.MarketTime);
                    DateTime cachedStartDateDART = mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.DART, (int)DataFilteredType.Orig][0].LmpTimePriceList.Min(x => x.MarketTime);
                    if (EndDate <= cachedEndDateDA && StartDate >= cachedStartDateDA)
                    {
                        if (!refreshData)
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            Node currentNode = new Node();
            LMPList lmpList = GetDarts();
            if (lmpList == null)
            {
                return;
            }
            List<Node> daList = lmpList.DAList;
            List<Node> rtList = lmpList.RTList;
            List<Node> dartList = lmpList.DARTList;
            if (!CachedNodeListHashCheckKeyExists(sourceSinkData, out existingPathKey))
            {
                mCachedNodeListHash.Add(existingPathKey, new List<Node>[2, 3, 2] { { { daList, null }, { rtList, null }, { dartList, null } },
                        { { null, null }, { null, null }, { null, null } } });
            }
            else // already exists in cache, update it
            {
                mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.DA, (int)DataFilteredType.Orig] = daList;
                mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.RT, (int)DataFilteredType.Orig] = rtList;
                mCachedNodeListHash[existingPathKey][(int)PeriodType.Hourly, (int)SpreadType.DART, (int)DataFilteredType.Orig] = dartList;
            }
            List<int> sourcesinkList = new List<int>();
            sourcesinkList.Add(sourceSinkData.Source.NodeKey);
            if (sourceSinkData.Sink != null)
            {
                sourcesinkList.Add(sourceSinkData.Sink.NodeKey);
            }
            mDataService.GetNodeCoordinate((nodesLocation, error) =>
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
        /// Cacheds the node list hash check key exists.
        /// </summary>
        /// <param name="srcSinkData">The source sink data.</param>
        /// <param name="existingPathKey">The existing path key.</param>
        /// <returns></returns>
        public bool CachedNodeListHashCheckKeyExists(SourceSinkData srcSinkData, out string existingPathKey)
        {
            bool keyExists = false;
            string testKey1 = srcSinkData.Sink == null ? srcSinkData.Source.NodeKey.ToString() : srcSinkData.Source.NodeKey.ToString() + ":" + srcSinkData.Sink.NodeKey.ToString();
            string testKey2 = srcSinkData.Sink == null ? srcSinkData.Source.NodeKey.ToString() : srcSinkData.Sink.NodeKey.ToString() + ":" + srcSinkData.Source.NodeKey.ToString();
            existingPathKey = testKey1;
            if (mCachedNodeListHash.ContainsKey(testKey1))
            {
                keyExists = true;
                existingPathKey = testKey1;
            }
            else if (mCachedNodeListHash.ContainsKey(testKey2))
            {
                keyExists = true;
                existingPathKey = testKey2;
            }
            return keyExists;
        }
        /// <summary>
        /// Refreshes the day comparison in grid.
        /// </summary>
        public void RefreshDayComparisonGridCommand()
        {
            try
            {
                // if no source/sink item is selected, return
                if (SourceSinkDataSelected == null)
                {
                    mHourlyPivotHash = new Dictionary<string, List<Node>>();
                    SetHourlyPivotList(mHourlyPivotHash);
                    return;
                }
                else
                {
                    if (mHourlyPivotHash.Keys.Contains("DART"))
                    {
                        mHourlyPivotHash.Remove("DART");
                    }
                    mHourlyPivotHash.Add("DART", mDartFilteredSortedList);
                    if (mHourlyPivotHash.ContainsKey("DA"))
                    {
                        mHourlyPivotHash.Remove("DA");
                    }
                    mHourlyPivotHash.Add("DA", mDaFilteredSortedList);
                    if (mHourlyPivotHash.Keys.Contains("RT"))
                    {
                        mHourlyPivotHash.Remove("RT");
                    }
                    mHourlyPivotHash.Add("RT", mRTFilteredSortedList);
                    // display day comparison hourly table
                    SetHourlyPivotList(mHourlyPivotHash);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Updates the chart.
        /// </summary>
        public void UpdateChartCommand()
        {
            try
            {
                // if no source/sink item is selected, return
                if (SourceSinkDataSelected == null)
                {
                    PlotModelUpper = null;
                    PlotModelLower = null;
                    return;
                }
                PlotSimulation = null;
                string existingPathKey = "";
                //if (SourceSinkDataSelected.Source == null && SourceSinkDataSelected.Sink==null)
                //{
                //    return;
                //}
                if (!CachedNodeListHashCheckKeyExists(SourceSinkDataSelected, out existingPathKey))
                {
                    return;
                }
                // (re)filter
                RefreshFilterData(existingPathKey);
                // (re)sort new lists
                Sort(mDaFilteredSortedList, mRTFilteredSortedList, mDartFilteredSortedList);
                // display day comparison hourly table
                RefreshDayComparisonGridCommand();
                // plot the sorted and filtered lists in upper and lower charts
                PlotUpperAndLowerChartsAndSetHourlyList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Refreshes the filter data.
        /// </summary>
        /// <param name="sourceSinkKey">The source sink key.</param>
        public void RefreshFilterData(string sourceSinkKey)
        {
            try
            {
                // preserve global original lists by creating new lists to filter and sort
                mDaFilteredSortedList = new List<Node>();
                mRTFilteredSortedList = new List<Node>();
                mDartFilteredSortedList = new List<Node>();

                if (mCachedNodeListHash.ContainsKey(sourceSinkKey))
                {
                    var a = mCachedNodeListHash[sourceSinkKey];
                    var b = mCachedNodeListHash[sourceSinkKey][(int)PeriodType.Hourly, (int)SpreadType.DA, (int)DataFilteredType.Orig];
                }
                foreach (Node node in mCachedNodeListHash[sourceSinkKey][(int)PeriodType.Hourly, (int)SpreadType.DA, (int)DataFilteredType.Orig])
                {
                    mDaFilteredSortedList.Add(new Node(node)); // new Node(node) is a deep copy of node object.
                }
                foreach (Node node in mCachedNodeListHash[sourceSinkKey][(int)PeriodType.Hourly, (int)SpreadType.RT, (int)DataFilteredType.Orig])
                {
                    mRTFilteredSortedList.Add(new Node(node)); // new Node(node) is a deep copy of node object.
                }
                foreach (Node node in mCachedNodeListHash[sourceSinkKey][(int)PeriodType.Hourly, (int)SpreadType.DART, (int)DataFilteredType.Orig])
                {
                    mDartFilteredSortedList.Add(new Node(node)); // new Node(node) is a deep copy of node object.
                }
                // remove missing values first 
                Filter(mDaFilteredSortedList, mRTFilteredSortedList, mDartFilteredSortedList, "Price", SelectedSpreadType.ToString(), double.MinValue, double.MaxValue);
                // remove TimePrice with date outside selected StartDate and EndDate that are in cached object
                if (CollectionChecked == false)
                {
                    FilterDatesOut(mDaFilteredSortedList, mRTFilteredSortedList, mDartFilteredSortedList, StartDate, EndDate);
                }
                // filter lists using filters
                if (FilterList != null)
                {
                    foreach (FilterData filterData in FilterList)
                    {
                        double? min = filterData.Min == null ? double.MinValue : filterData.Min;
                        double? max = filterData.Max == null ? double.MaxValue : filterData.Max;
                        Filter(mDaFilteredSortedList, mRTFilteredSortedList, mDartFilteredSortedList, filterData.Product, filterData.Type, min, max);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the darts.
        /// </summary>
        /// <returns></returns>
        private LMPList GetDarts()
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            List<Node> daList = new List<Node>();
            List<Node> rtList = new List<Node>();
            List<Node> dartList = new List<Node>();
            LMPList lmpList = new LMPList();
            try
            {
                if (CollectionChecked == true)
                {
                    if (DateCollectionList == null)
                    {
                        return null;
                    }
                    foreach (DateTime startDate in DateCollectionList)
                    {
                        mDataService.GetChartData((daNodeList, rtNodeList, error) =>
                        {
                            if (error != null)
                            {
                                return;
                            }
                            List<LmpTimePrice>[] daListtemp = new List<LmpTimePrice>[3];
                            List<LmpTimePrice>[] rtListtemp = new List<LmpTimePrice>[3];
                            if (DateCollectionList.Min().Equals(startDate))
                            {
                                foreach (var node in daNodeList)
                                {
                                    List<LmpTimePrice> list = node.LmpTimePriceList.Take(24).ToList();
                                    node.LmpTimePriceList = list;
                                }
                                foreach (var node in rtNodeList)
                                {
                                    List<LmpTimePrice> list = node.LmpTimePriceList.Take(24).ToList();
                                    node.LmpTimePriceList = list;
                                }
                                dartList.Add(new Node(daNodeList[2]));
                                dartList.Add(new Node(rtNodeList[2]));
                                daList = daNodeList;
                                rtList = rtNodeList;
                            }
                            else
                            {
                                int counter = 0;
                                foreach (var node in daNodeList)
                                {
                                    List<LmpTimePrice> list = node.LmpTimePriceList.Take(24).ToList();
                                    daListtemp[counter] = new List<LmpTimePrice>();
                                    daListtemp[counter].AddRange(list);
                                    counter++;
                                }
                                counter = 0;
                                foreach (var node in rtNodeList)
                                {
                                    List<LmpTimePrice> list = node.LmpTimePriceList.Take(24).ToList();
                                    rtListtemp[counter] = new List<LmpTimePrice>();
                                    rtListtemp[counter].AddRange(list);
                                    counter++;
                                }
                                dartList[0].LmpTimePriceList.AddRange(daListtemp[2]);
                                dartList[1].LmpTimePriceList.AddRange(rtListtemp[2]);
                                for (int i = 0; i < 3; i++)
                                {
                                    daList[i].LmpTimePriceList.AddRange(daListtemp[i]);
                                    rtList[i].LmpTimePriceList.AddRange(rtListtemp[i]);
                                }
                            }
                        }, startDate, startDate, sourceSinkData.Source, sourceSinkData.Sink);
                    }
                    dartList.Add(CalculateDART(daList[2], rtList[2], false));
                }
                else
                {
                    mDataService.GetChartData((daNodeList, rtNodeList, error) =>
                    {
                        if (error != null)
                        {
                            return;
                        }
                        dartList.Add(new Node(daNodeList[daNodeList.Count - 1]));
                        dartList.Add(new Node(rtNodeList[rtNodeList.Count - 1]));
                        daList = daNodeList;
                        rtList = rtNodeList;
                    }, StartDate, EndDate, sourceSinkData.Source, sourceSinkData.Sink);
                    dartList.Add(CalculateDART(daList[daList.Count - 1], rtList[rtList.Count - 1], daList.Count - 1 == 0));
                }
                lmpList.DAList = daList;
                lmpList.DARTList = dartList;
                lmpList.RTList = rtList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return lmpList;
        }
        /// <summary>
        /// Sets the sim minimum maximum.
        /// </summary>
        private void SetSimMinMax()
        {
            if (UptosChecked)
            {
                if (SelectedType == "LMP" || SelectedType == "Energy")
                {
                    SimMinText = "-50";
                    SimMaxText = "50";
                }
                else
                {
                    SimMinText = "-5";
                    SimMaxText = "5";
                }
                IncChecked = false;
            }
            else
            {
                if (SelectedType == "LMP" || SelectedType == "Energy")
                {
                    SimMinText = "10";
                    SimMaxText = "50";
                }
                else
                {
                    SimMinText = "-5";
                    SimMaxText = "5";
                }
            }
        }

        /// <summary>
        /// Plots the upper and lower charts and set hourly list.
        /// </summary>
        private void PlotUpperAndLowerChartsAndSetHourlyList()
        {
            try
            {
                if (mDaFilteredSortedList == null || SourceSinkDataSelected == null)
                {
                    return;
                }
                if (mSourceSinkDataSelected.Sink == null && (FilterDayComparisonDAChecked == true || FilterDayComparisonRTChecked == true))
                {
                    SpreadHighlightAbove = "30";
                    SpreadHighlightBelow = "60";
                }
                else
                {
                    if (SpreadHighlightAbove == "30")
                    {
                        SpreadHighlightAbove = "-2";
                        SpreadHighlightBelow = "2";
                    }
                }
                List<Node> plotList = new List<Node>();
                PlotModelUpper = null;
                PlotModelLower = null;
                List<Node> dailyNodeList = new List<Node>();
                List<Node> hourlyNodeList = new List<Node>();
                if (SelectedSpreadType == SpreadType.DA)
                {
                    hourlyNodeList = mDaFilteredSortedList;
                }
                else if (SelectedSpreadType == SpreadType.RT)
                {
                    hourlyNodeList = mRTFilteredSortedList;
                }
                else if (SelectedSpreadType == SpreadType.DART)
                {
                    hourlyNodeList = mDartFilteredSortedList;
                }
                //if (SelectedSortType == SortType.DA)
                //{
                //    hourlyNodeList = mDaFilteredSortedList;
                //}
                //else if (SelectedSortType == SortType.RT)
                //{
                //    hourlyNodeList = mRTFilteredSortedList;
                //}


                if (SelectedPeriodType == PeriodType.Daily)
                {
                    if (SelectedSortType == SortType.DART)
                        plotList = ConvertDailyValues(hourlyNodeList);
                    if (selectedSortType == SortType.DA)
                        plotList = ConvertDailyValues(mDaFilteredSortedList);
                    if (selectedSortType == SortType.RT)
                        plotList = ConvertDailyValues(mRTFilteredSortedList);
                    if (selectedSortType == SortType.Date)
                    {
                        if (SelectedSpreadType == SpreadType.DA)
                            plotList = ConvertDailyValues(mDaFilteredSortedList);
                        else
                            plotList = ConvertDailyValues(mDartFilteredSortedList);
                        //plotList = ConvertDailyValues(mRTFilteredSortedList);
                    }
                    //  plotList = mDartFilteredSortedList;
                }
                else
                {
                    plotList = hourlyNodeList;
                }
                if (deleteddatelist != null)
                {
                    foreach (var item in deleteddatelist.ToList())
                    {
                        string[] deldate = item.Split(' '); ;
                        foreach (var plot in plotList.ToList())
                        {
                            foreach (var lmpprice in plot.LmpTimePriceList.ToList())
                            {
                                string[] lmpdate = lmpprice.MarketTime.ToString().Split(' ');
                                if (deldate[0] == lmpdate[0])
                                {
                                    plot.LmpTimePriceList.Remove(lmpprice);
                                }
                            }
                        }
                    }
                }
                // chart upper and lower charts with selected plotList data
                PlotModelUpper = CreatePlotModelUpper(plotList);
                PlotModelLower = CreatePlotModelLower(plotList);
                // update hourly price grid (2nd tab) with selected plotList data
                HourlyList = SetHourlyList(plotList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Filters the dates out.
        /// </summary>
        /// <param name="daNodeList">The da node list.</param>
        /// <param name="rtNodeList">The rt node list.</param>
        /// <param name="dartNodeList">The dart node list.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        private void FilterDatesOut(List<Node> daNodeList, List<Node> rtNodeList, List<Node> dartNodeList, DateTime start, DateTime end)
        {
            end = end.AddDays(1);
            foreach (List<Node> nodelist in new List<List<Node>>() { daNodeList, rtNodeList, dartNodeList })
            {
                foreach (Node node in nodelist)
                {
                    List<LmpTimePrice> tpToRemoveList = new List<LmpTimePrice>();
                    foreach (LmpTimePrice tp in node.LmpTimePriceList)
                    {
                        if (tp.MarketTime < start || tp.MarketTime > end)
                        {
                            tpToRemoveList.Add(tp);
                        }
                    }
                    foreach (LmpTimePrice tp in tpToRemoveList)
                    {
                        node.LmpTimePriceList.Remove(tp);
                    }
                }
            }
        }
        /// <summary>
        /// Sets the hourly list.
        /// </summary>
        /// <param name="nodeList">The node list.</param>
        /// <returns></returns>
        private List<HourlyData> SetHourlyList(List<Node> nodeList)
        {
            List<HourlyData> hourlyData = new List<HourlyData>();

            try
            {
                var query = (dynamic)null;
                bool hasSink = true;
                if (nodeList.Count > 1)
                {
                    if (selectedType == "Congestion")
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                join snk in nodeList[1].LmpTimePriceList on src.MarketTime equals snk.MarketTime
                                join spd in nodeList[2].LmpTimePriceList on snk.MarketTime equals spd.MarketTime
                                select new { Date = spd.MarketTime.Date, Hour = spd.MarketTime.Hour, Source = src.Lmp.Congestion, Sink = snk.Lmp.Congestion, Spread = spd.Lmp.Price };
                    }
                    else if (selectedType == "Loss")
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                join snk in nodeList[1].LmpTimePriceList on src.MarketTime equals snk.MarketTime
                                join spd in nodeList[2].LmpTimePriceList on snk.MarketTime equals spd.MarketTime
                                select new { Date = spd.MarketTime.Date, Hour = spd.MarketTime.Hour, Source = src.Lmp.Loss, Sink = snk.Lmp.Loss, Spread = spd.Lmp.Price };
                    }
                    else if (selectedType == "Energy")
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                join snk in nodeList[1].LmpTimePriceList on src.MarketTime equals snk.MarketTime
                                join spd in nodeList[2].LmpTimePriceList on snk.MarketTime equals spd.MarketTime
                                select new
                                {
                                    Date = spd.MarketTime.Date,
                                    Hour = spd.MarketTime.Hour,
                                    Source = src.Lmp.Price - src.Lmp.Congestion - src.Lmp.Loss,
                                    Sink = src.Lmp.Price - src.Lmp.Congestion - snk.Lmp.Loss,
                                    Spread = spd.Lmp.Price
                                };
                    }
                    else
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                join snk in nodeList[1].LmpTimePriceList on src.MarketTime equals snk.MarketTime
                                join spd in nodeList[2].LmpTimePriceList on snk.MarketTime equals spd.MarketTime
                                select new { Date = spd.MarketTime.Date, Hour = spd.MarketTime.Hour, Source = src.Lmp.Price, Sink = snk.Lmp.Price, Spread = spd.Lmp.Price };
                    }
                }
                else
                {
                    if (selectedType == "Congestion")
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                select new { Date = src.MarketTime.Date, Hour = src.MarketTime.Hour, Source = src.Lmp.Congestion };
                    }
                    else if (selectedType == "Loss")
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                select new { Date = src.MarketTime.Date, Hour = src.MarketTime.Hour, Source = src.Lmp.Loss };
                    }
                    else if (selectedType == "Energy")
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                select new { Date = src.MarketTime.Date, Hour = src.MarketTime.Hour, Source = src.Lmp.Price - src.Lmp.Congestion - src.Lmp.Loss };
                    }
                    else
                    {
                        query = from src in nodeList[0].LmpTimePriceList
                                select new { Date = src.MarketTime.Date, Hour = src.MarketTime.Hour, Source = src.Lmp.Price };
                    }
                    hasSink = false;
                }

                foreach (var item in query)
                {
                    HourlyData hourItem = new HourlyData();
                    if (item.Hour == 0)
                    {
                        hourItem.Date = item.Date.AddDays(-1);
                        hourItem.Hour = 24;
                    }
                    else
                    {
                        hourItem.Date = item.Date;
                        hourItem.Hour = item.Hour;
                    }
                    if (!item.Source.Equals(double.NaN))
                    {
                        hourItem.Source = item.Source;
                    }
                    else
                    {
                        hourItem.Source = null;
                    }
                    if (hasSink && !item.Sink.Equals(double.NaN))
                    {
                        hourItem.Sink = item.Sink;
                    }
                    else
                    {
                        hourItem.Sink = null;
                    }
                    if (hasSink && !item.Spread.Equals(double.NaN))
                    {
                        hourItem.Spread = item.Spread - Fee;
                    }
                    else
                    {
                        hourItem.Spread = null;
                    }
                    hourlyData.Add(hourItem);
                }
                hourlyData = hourlyData.OrderBy(x => x.Date.AddHours(x.Hour)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return hourlyData;
        }
        /// <summary>
        /// Sets the hourly pivot list.
        /// </summary>
        /// <param name="hourlyPivotList">The hourly pivot list.</param>
        private void SetHourlyPivotList(Dictionary<string, List<Node>> hourlyPivotList)
        {
            if (hourlyPivotList == null)
            {
                HourlyPivotList = null;
                return;
            }


            List<HourlyPivotData> hourlyPivotData = new List<HourlyPivotData>();
            //Dictionary<DateTime, double> clearedMsHash = CalculateDailyClearedMWs(hourlyPivotList, StartDate, EndDate);
            string rowType;

            DateTime LoadDate = DateTime.MinValue;

            try
            {

                foreach (var nodeList in hourlyPivotList)
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
                        var itemlist = item.LmpTimePriceList.OrderBy(i => i.MarketTime).ToArray();
                        DateTime dateCounter = DateTime.Now.Date;
                        if (itemlist.FirstOrDefault() != null)
                        {
                            dateCounter = itemlist.FirstOrDefault().MarketTime.Date;
                        }
                        double total = 0;
                        double avgcounter = 0;
                        HourlyPivotData hpdata = new HourlyPivotData();
                        #region OLD
                        //if (MarketComboSelectedValue == "PJM")
                        //{
                        //    // if (loadDictHash.Count() == 0)
                        //    {
                        //        DataService ds = new DataService();
                        //        List<DateTime> datetimeList = itemlist.Select(x => x.MarketTime).ToList<DateTime>();
                        //        // loadDictHash = ds.GetLoadsData(datetimeList, MarketComboSelectedValue);
                        //    }
                        //}
                        //if (MarketComboSelectedValue == "ERCOT")
                        //{
                        //    //if (loadDictHash.Count() == 0)
                        //    {
                        //        DataService ds = new DataService();
                        //        List<DateTime> datetimeList = itemlist.Select(x => x.MarketTime).ToList<DateTime>();
                        //        // loadDictHash = ds.GetLoadsData(datetimeList, MarketComboSelectedValue);
                        //    }
                        //}
                        #endregion OLD
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
                                hpdata.Total = (total - Fee);
                                hpdata.Average = (total - Fee) / avgcounter;
                                avgcounter = total = 0;
                                hpdata.Date = dateCounter;
                                hpdata.DateDisplay = dateCounter;
                                hpdata.RowDay = dateCounter.Date.ToString("ddd");
                                hpdata.RowType = rowType;
                                hpdata.RowDisplayType = rowType;
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

                                //if (hpdata.MaxLoad == null)
                                //{
                                //    if (LoadDate == DateTime.MinValue)
                                //    {
                                //        LoadDate = hourPrice.MarketTime.Date;
                                //        if (LoadHash.ContainsKey(LoadDate))
                                //        {
                                //            hpdata.MaxLoad = LoadHash[LoadDate];
                                //        }
                                //    }
                                //    else if (LoadDate != hourPrice.MarketTime.Date)
                                //    {
                                //        LoadDate = hourPrice.MarketTime.Date;
                                //        if (LoadHash.ContainsKey(LoadDate))
                                //            hpdata.MaxLoad = LoadHash[LoadDate];
                                //    }
                                //}

                                if (MarketComboSelectedValue == "PJM")
                                {
                                    if (ploadDictHash.ContainsKey(dateCounter.ToString("dd-MM-yyyy")))
                                    {
                                        hpdata.MaxLoad = ploadDictHash[dateCounter.ToString("dd-MM-yyyy")];
                                    }
                                }
                                if (MarketComboSelectedValue == "ERCOT")
                                {
                                    if (eloadDictHash.ContainsKey(dateCounter.ToString("dd-MM-yyyy")))
                                    {
                                        hpdata.MaxLoad = eloadDictHash[dateCounter.ToString("dd-MM-yyyy")];
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
                            if (!hourPrice.Lmp.Price.Equals(double.NaN))
                            {
                                if (selectedType == "Congestion")//&& rowType != "DART"
                                {
                                    price = hourPrice.Lmp.Congestion - Fee;
                                }
                                else if (selectedType == "Loss")// && rowType != "DART"
                                {
                                    price = hourPrice.Lmp.Loss - Fee;
                                }
                                else if (selectedType == "Energy" && rowType != "DART")
                                {
                                    price = hourPrice.Lmp.Price - hourPrice.Lmp.Congestion - hourPrice.Lmp.Loss - Fee;
                                }
                                else
                                {
                                    price = hourPrice.Lmp.Price - Fee;
                                }
                                total += (double)price - Fee;
                                if (rowType == "DART")
                                {
                                    if (mMaxDart == null || mMaxDart < price)
                                    {
                                        mMaxDart = price;
                                    }
                                    if (mMinDart == null || mMinDart > price)
                                    {
                                        mMinDart = price;
                                    }
                                }
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
                            hpdata.RowType = rowType;
                            hpdata.RowDisplayType = rowType;
                            hourlyPivotData.Add(hpdata);
                            hpdata = null;
                        }
                    }
                    hourlyPivotData.RemoveAll(item => (item.Total == null && item.Average == null));
                    int n = hourlyPivotData.Where(a => a.RowType == null).Count();
                }
                HourlyPivotListSummary = null;
                if (hourlyPivotData.Where(a => a.RowType == null).Count() <= 0)
                {
                    List<HourlyPivotData> hourlyPivotListSummaryList = new List<HourlyPivotData>();
                    List<HourlyPivotData> sortTempHourlyPivotList = (from t in hourlyPivotData
                                                                     orderby t.Date descending, t.RowType.Length descending, t.RowType ascending, t.RowName descending
                                                                     select t).ToList();

                    if (deleteddatelist.Count() != 0)
                    {
                        foreach (var item in deleteddatelist.ToList())
                        {
                            string[] deldate = item.Split(' ');
                            foreach (var lmpTimePrice in sortTempHourlyPivotList.ToList())
                            {
                                string[] dtime = lmpTimePrice.Date.ToString().Split(' ');
                                if (deldate[0] == dtime[0])
                                {
                                    sortTempHourlyPivotList.Remove(lmpTimePrice);
                                }
                            }
                        }
                    }

                    if (sortTempHourlyPivotList.Count > 0)
                    {
                        double[] hourCountList = new double[24];
                        HourlyPivotData totalHourlySummaryPivot = new HourlyPivotData();
                        totalHourlySummaryPivot.RowDisplayType = "Total";
                        totalHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        totalHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;
                        HourlyPivotData averageHourlySummaryPivot = new HourlyPivotData();
                        averageHourlySummaryPivot.RowDisplayType = "Average";
                        averageHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        averageHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;

                        HourlyPivotData winPerHourlySummaryPivot = new HourlyPivotData();
                        winPerHourlySummaryPivot.RowDisplayType = "Win %";
                        winPerHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        winPerHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;

                        DateTime? date = sortTempHourlyPivotList[0].Date;
                        string rowtype = sortTempHourlyPivotList[0].RowDisplayType;
                        sortTempHourlyPivotList[0].RowDay = date.Value.ToString("ddd");
                        double? sumWin = null;
                        double? sumHourCount = null;
                        double? sumHourTotal = null;

                        HourlyPivotData minHourlySummaryPivot = new HourlyPivotData();
                        minHourlySummaryPivot.RowDisplayType = "Min";
                        minHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        minHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;
                        HourlyPivotData maxHourlySummaryPivot = new HourlyPivotData();
                        maxHourlySummaryPivot.RowDisplayType = "Max";
                        maxHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        maxHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;
                        HourlyPivotData riskPerHourlySummaryPivot = new HourlyPivotData();
                        riskPerHourlySummaryPivot.RowDisplayType = "Risk";
                        riskPerHourlySummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        riskPerHourlySummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;

                        HourlyPivotData riskMedianSummaryPivot = new HourlyPivotData();
                        riskMedianSummaryPivot.RowDisplayType = "Median";
                        riskMedianSummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        riskMedianSummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;

                        HourlyPivotData riskstddevSummaryPivot = new HourlyPivotData();
                        riskstddevSummaryPivot.RowDisplayType = "StdDev";
                        riskstddevSummaryPivot.RowDay = sortTempHourlyPivotList[0].RowDisplayType;
                        riskstddevSummaryPivot.RowName = sortTempHourlyPivotList[0].RowName;

                        var mintotal = sortTempHourlyPivotList.Min(p => p.Total.GetValueOrDefault());
                        var maxtotal = sortTempHourlyPivotList.Max(p => p.Total.GetValueOrDefault());
                        minHourlySummaryPivot.Total = mintotal;
                        maxHourlySummaryPivot.Total = maxtotal;
                        var list1 = sortTempHourlyPivotList.Where(e => e.Total == mintotal);
                        for (int i = 1; i < sortTempHourlyPivotList.Count + 1; i++)
                        {
                            if (i < sortTempHourlyPivotList.Count)
                            {
                                if (sortTempHourlyPivotList[i].RowDisplayType == rowtype && (date == sortTempHourlyPivotList[i].Date || sortTempHourlyPivotList[i].Date == null))
                                {
                                    sortTempHourlyPivotList[i].RowDisplayType = string.Empty;
                                }
                                else
                                {
                                    rowtype = sortTempHourlyPivotList[i].RowDisplayType;
                                }
                                if (sortTempHourlyPivotList[i].Date == date)
                                {
                                    sortTempHourlyPivotList[i].RowDay = date.Value.ToString("ddd");
                                    sortTempHourlyPivotList[i].DateDisplay = null;
                                }
                                else
                                {
                                    date = sortTempHourlyPivotList[i].Date;
                                    sortTempHourlyPivotList[i].RowDay = date.Value.ToString("ddd");
                                }
                            }
                            Action<int> winRow = (hour) =>
                            {
                                if (sumWin == null && sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    if (sortTempHourlyPivotList[i - 1].HourDataList[hour] > 0)
                                    {
                                        sumWin = 1;
                                    }
                                }
                                else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    if (sortTempHourlyPivotList[i - 1].HourDataList[hour] > 0)
                                    {
                                        sumWin++;
                                    }
                                }
                                if (sumHourTotal == null && sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    sumHourTotal = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    sumHourCount = 1;
                                }
                                else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    sumHourTotal += sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    sumHourCount++;
                                }
                                if (winPerHourlySummaryPivot.HourDataList[hour] == null && sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    if (sortTempHourlyPivotList[i - 1].HourDataList[hour] > 0)
                                    {
                                        winPerHourlySummaryPivot.HourDataList[hour] = 1;
                                    }
                                }
                                else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    if (sortTempHourlyPivotList[i - 1].HourDataList[hour] > 0)
                                    {
                                        winPerHourlySummaryPivot.HourDataList[hour]++;
                                    }
                                }
                                if (totalHourlySummaryPivot.HourDataList[hour] == null && sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    totalHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    minHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    maxHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    hourCountList[hour] = 1;
                                }
                                else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                {
                                    totalHourlySummaryPivot.HourDataList[hour] += sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    if (minHourlySummaryPivot.HourDataList[hour] > sortTempHourlyPivotList[i - 1].HourDataList[hour])
                                    {
                                        minHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    }
                                    if (maxHourlySummaryPivot.HourDataList[hour] < sortTempHourlyPivotList[i - 1].HourDataList[hour])
                                    {
                                        maxHourlySummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[i - 1].HourDataList[hour];
                                    }
                                    hourCountList[hour]++;
                                }
                                //if (riskMedianSummaryPivot.HourDataList[hour] == null && sortTempHourlyPivotList[i - 1] != null)
                                //{
                                //    riskMedianSummaryPivot.HourDataList[hour] = sortTempHourlyPivotList[hour].HourDataList[i - 1];
                                //}
                                //else if (sortTempHourlyPivotList[i - 1].HourDataList[hour] != null)
                                //{
                                //    if (sortTempHourlyPivotList[i - 1].HourDataList[hour] > 0)
                                //    {
                                //        riskMedianSummaryPivot.HourDataList[hour]++;
                                //    }
                                //}

                            };
                            for (int j = 0; j < 24; j++)
                            {
                                winRow(j);
                            }
                        } //for loop
                        double sumTotal = 0;
                        double sumCount = 0;
                        double sumMax = -1000;
                        double sumMin = 1000;

                        Action<int> hourlySumm = (hour) =>
                        {
                            if (totalHourlySummaryPivot.HourDataList[hour] != null)
                            {
                                sumTotal += (double)totalHourlySummaryPivot.HourDataList[hour];
                                sumCount++;
                                if (sumMax < (double)maxHourlySummaryPivot.HourDataList[hour])
                                {
                                    sumMax = (double)maxHourlySummaryPivot.HourDataList[hour];
                                }
                                if (minHourlySummaryPivot.HourDataList[hour].HasValue)
                                {
                                    if (sumMin > (double)minHourlySummaryPivot.HourDataList[hour])
                                    {
                                        sumMin = (double)minHourlySummaryPivot.HourDataList[hour];
                                    }
                                }
                                averageHourlySummaryPivot.HourDataList[hour] = totalHourlySummaryPivot.HourDataList[hour] / hourCountList[hour];
                                winPerHourlySummaryPivot.HourDataList[hour] = (winPerHourlySummaryPivot.HourDataList[hour] / hourCountList[hour]) * 100;
                                riskPerHourlySummaryPivot.HourDataList[hour] = GetRisk(minHourlySummaryPivot.HourDataList[hour], maxHourlySummaryPivot.HourDataList[hour],
                                winPerHourlySummaryPivot.HourDataList[hour] / 100);


                            }
                        };
                        for (int k = 0; k < 24; k++)
                        {
                            hourlySumm(k);
                        }
                        #region ADDCheckedHrs
                        List<int> hourList = new List<int>();
                        for (int i = 0; i < 24; i++)
                        {
                            for (int j = 0; j < sortTempHourlyPivotList.Count; j++)
                            {
                                if (sortTempHourlyPivotList[j].HourDataList[i] != null)
                                {
                                    if (!hourList.Contains(i))
                                    {
                                        hourList.Add(i);
                                    }
                                }
                            }
                        }
                        #endregion
                        double?[] riskMedianSummaryPivottemp = new double?[7500];
                        // double?[] riskstddevSummaryPivottemp = new double?[1500];
                        for (int i = 0; i < 24; i++)
                        {
                            if (!hourList.Contains(i))
                                continue;
                            for (int j = 0; j < sortTempHourlyPivotList.Count; j++)
                            {
                                if (i < sortTempHourlyPivotList.Count)
                                {
                                    if (sortTempHourlyPivotList[i] != null && sortTempHourlyPivotList[j].HourDataList[i] != null)
                                    {

                                        riskMedianSummaryPivottemp[j] = sortTempHourlyPivotList[j].HourDataList[i];
                                        //riskstddevSummaryPivottemp[j] = sortTempHourlyPivotList[j].HourDataList[i];
                                    }
                                    else
                                    {
                                        riskMedianSummaryPivottemp[j] = sortTempHourlyPivotList[j].HourDataList[i];
                                        // riskstddevSummaryPivottemp[j] = sortTempHourlyPivotList[j].HourDataList[i]; ;
                                    }
                                }
                                else
                                {
                                    riskMedianSummaryPivottemp[j] = sortTempHourlyPivotList[j].HourDataList[i];
                                    // riskstddevSummaryPivottemp[j] = sortTempHourlyPivotList[j].HourDataList[i]; ;
                                }
                            }
                            int count = 0; int countstd = 0;
                            List<double?> sort = new List<double?>();
                            List<double?> sortstd = new List<double?>();
                            foreach (var item in riskMedianSummaryPivottemp)
                            {
                                if (item.HasValue)
                                    sort.Add(item);
                            }
                            sort.Sort();
                            count = sort.Count;
                            foreach (var item in riskMedianSummaryPivottemp)
                            {
                                if (item.HasValue)
                                    sortstd.Add(item);
                            }
                            if (sortstd.Count > 0)
                            {
                                riskstddevSummaryPivot.HourDataList[i] = GetSTDDev(sortstd);
                            }
                            if (count > 0)
                                riskMedianSummaryPivot.HourDataList[i] = GetMedian(riskMedianSummaryPivottemp);

                        }
                        if (totalHourlySummaryPivot.HourDataList.Any(x => x.HasValue))
                        {
                            totalHourlySummaryPivot.Total = sumTotal;
                            totalHourlySummaryPivot.Average = sumTotal / sumCount;
                            averageHourlySummaryPivot.Total = sumHourTotal / sumHourCount;
                            winPerHourlySummaryPivot.Total = (sumWin / sumHourCount) * 100;
                            riskPerHourlySummaryPivot.Total = GetRisk(mintotal, maxtotal, (sumWin / sumHourCount));
                            riskMedianSummaryPivot.Total = riskMedianSummaryPivot.HourDataList.Sum();
                            if (FilterDayComparisonTotalChecked)
                                hourlyPivotListSummaryList.Add(totalHourlySummaryPivot);
                            else
                                hourlyPivotListSummaryList.Remove(totalHourlySummaryPivot);
                            if (FilterDayComparisonAvgChecked)
                                hourlyPivotListSummaryList.Add(averageHourlySummaryPivot);
                            else
                                hourlyPivotListSummaryList.Remove(averageHourlySummaryPivot);
                            if (FilterDayComparisonMinChecked)
                                hourlyPivotListSummaryList.Add(minHourlySummaryPivot);
                            else
                                hourlyPivotListSummaryList.Remove(minHourlySummaryPivot);
                            if (FilterDayComparisonMaxChecked)
                                hourlyPivotListSummaryList.Add(maxHourlySummaryPivot);
                            else
                                hourlyPivotListSummaryList.Remove(maxHourlySummaryPivot);
                            if (FilterDayComparisonMedianChecked)
                                hourlyPivotListSummaryList.Add(riskMedianSummaryPivot);
                            else
                                hourlyPivotListSummaryList.Remove(riskMedianSummaryPivot);
                            if (FilterDayComparisonWinPctChecked)
                                hourlyPivotListSummaryList.Add(winPerHourlySummaryPivot);
                            else
                                hourlyPivotListSummaryList.Remove(winPerHourlySummaryPivot);
                            if (FilterDayComparisonStdDevChecked)
                                hourlyPivotListSummaryList.Add(riskstddevSummaryPivot);
                            else
                                hourlyPivotListSummaryList.Remove(riskstddevSummaryPivot);
                        }
                        //for (int hour = 0; hour < 24; hour++)
                        //{
                        //    riskPerHourlySummaryPivot.HourDataList[hour] = GetRisk(minHourlySummaryPivot.HourDataList[hour], maxHourlySummaryPivot.HourDataList[hour],
                        //    winPerHourlySummaryPivot.HourDataList[hour] / 100);
                        //}
                        if (FilterDayComparisonRiskChecked)
                            hourlyPivotListSummaryList.Add(riskPerHourlySummaryPivot);
                        else
                            hourlyPivotListSummaryList.Remove(riskPerHourlySummaryPivot);
                    }


                    // this populates the datagrid in gui
                    MaxDart = mMaxDart;
                    MinDart = mMinDart;
                    HourlyPivotList = sortTempHourlyPivotList;
                    HourlyPivotListSummary = hourlyPivotListSummaryList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private double? GetSTDDev(List<double?> riskstddevSummaryPivottemp)
        {
            double? average = riskstddevSummaryPivottemp.Average();
            double? sumOfSquaresOfDifferences = riskstddevSummaryPivottemp.Select(val => (val - average) * (val - average)).Sum();
            double? sd = Math.Sqrt(sumOfSquaresOfDifferences.Value / (riskstddevSummaryPivottemp.Count - 1));
            return sd;
        }

        //private Dictionary<DateTime, double> CalculateDailyClearedMWs(Dictionary<string, List<Node>> PathList, DateTime startDate, DateTime endDate)
        //{
        //    Dictionary<DateTime, double> clearedMsHash = new Dictionary<DateTime, double>();
        //    if (PathList != null && PathList.Count > 0)
        //    {
        //        for (DateTime tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
        //        {
        //            double mw = 0.0;
        //            foreach (var path in PathList)
        //            {
        //                string[] hours = path.AnalysisType.Split('.');
        //                foreach (string hour in hours)
        //                {
        //                    DateTime sendDate = tempDate.AddHours(Int32.Parse(hour));
        //                    string sourceKey = sendDate.ToString() + DBAccess.GetNodeFromName(path.Source, path.Market).NodeKey; ;
        //                    string sinkKey = path.Sink == "" ? null : sendDate.ToString() + DBAccess.GetNodeFromName(path.Sink, path.Market).NodeKey; ;
        //                    double da = double.NaN;
        //                    double rt = double.NaN;
        //                    double dart = double.NaN;
        //                    if (sinkKey != null)
        //                    {
        //                        if (DARTNode.sDAHash.ContainsKey(sourceKey) && DARTNode.sDAHash.ContainsKey(sinkKey) &&
        //                            !double.IsNaN(DARTNode.sDAHash[sourceKey]) && !double.IsNaN(DARTNode.sDAHash[sinkKey]))
        //                        {
        //                            da = (DARTNode.sDAHash[sinkKey] - DARTNode.sDAHash[sourceKey]);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (DARTNode.sDAHash.ContainsKey(sourceKey) &&
        //                            !double.IsNaN(DARTNode.sDAHash[sourceKey]))
        //                        {
        //                            da = DARTNode.sDAHash[sourceKey];
        //                        }
        //                    }
        //                    if (sinkKey != null)
        //                    {
        //                        if (DARTNode.sRTHash.ContainsKey(sourceKey) && DARTNode.sRTHash.ContainsKey(sinkKey) &&
        //                            !double.IsNaN(DARTNode.sRTHash[sourceKey]) && !double.IsNaN(DARTNode.sRTHash[sinkKey]))
        //                        {
        //                            rt = (DARTNode.sRTHash[sinkKey] - DARTNode.sRTHash[sourceKey]);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (DARTNode.sRTHash.ContainsKey(sourceKey) &&
        //                            !double.IsNaN(DARTNode.sRTHash[sourceKey]))
        //                        {
        //                            rt = DARTNode.sRTHash[sourceKey];
        //                        }
        //                    }
        //                    if (!double.IsNaN(da) && !double.IsNaN(rt))
        //                    {
        //                        dart = rt - da;
        //                    }
        //                    string sourceSink = path.Sink == null || path.Sink.Length == 0 ? path.Source : path.Source + "->" + path.Sink;

        //                    if ((da <= path.Price && path.MW > 0) || (da >= path.Price && path.MW < 0))
        //                    {
        //                        mw += path.MW;
        //                    }
        //                }
        //            }
        //            clearedMsHash.Add(tempDate, mw);
        //        }
        //    }
        //    return clearedMsHash;
        //}


        private double? GetMedian(double?[] hourlyPivotData)
        {
            double median = 0.0;
            int count = 0;
            List<double?> sort = new List<double?>();
            foreach (var item in hourlyPivotData)
            {
                if (item.HasValue)
                    sort.Add(item);
            }
            sort.Sort();
            count = sort.Count;
            if (count > 0)
            {
                sort.OrderBy(order => order.Value);
                if (count % 2 == 0)
                {
                    median = (sort[(count / 2) - 1].Value + sort[(count / 2)].Value) / 2;
                }
                else
                {
                    median = sort[(count / 2)].Value;
                }
            }
            else
            {
                return null;
            }
            return median;
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
        /// Calculates the type of the row.
        /// </summary>
        /// <param name="aList">a list.</param>
        /// <param name="type">The type.</param>
        /// <param name="nameId">The name identifier.</param>
        /// <returns></returns>
        private List<HourlyPivotData> CalculateRowType(List<TimePrice> aList, string type, int nameId)
        {
            int totalday = aList.Count();
            List<HourlyPivotData> hourlyPivotData = new List<HourlyPivotData>();
            int hrs = totalday % 24;
            int[] avgcounter = new int[24];
            totalday /= 24;
            string rowname = "";
            int averageCounter = 0;
            switch (nameId)
            {
                case 0:
                    rowname = "Source";
                    break;
                case 1:
                    rowname = "Sink";
                    break;
                case 2:
                    rowname = "Spread";
                    break;
            }
            if (hrs > 0 || totalday > 0)
            {
                /*averageCounter =*/
                PreparePivotData(aList, type, totalday, hourlyPivotData, hrs, avgcounter, rowname, averageCounter);
                return hourlyPivotData;
            }
            else
            {
                return hourlyPivotData;
            }
        }
        /// <summary>
        /// Prepares the pivot data.
        /// </summary>
        /// <param name="aList">a list.</param>
        /// <param name="type">The type.</param>
        /// <param name="totalday">The totalday.</param>
        /// <param name="hourlyPivotData">The hourly pivot data.</param>
        /// <param name="hrs">The HRS.</param>
        /// <param name="avgcounter">The avgcounter.</param>
        /// <param name="rowname">The rowname.</param>
        /// <param name="averageCounter">The average counter.</param>
        private void PreparePivotData(List<TimePrice> aList, string type, int totalday, List<HourlyPivotData> hourlyPivotData, int hrs, int[] avgcounter, string rowname, int averageCounter)
        {
            HourlyPivotData hpdata = new HourlyPivotData();
            HourlyPivotData maxDayDart = new HourlyPivotData();
            HourlyPivotData minDayDart = new HourlyPivotData();
            HourlyPivotData winDayDart = new HourlyPivotData();
            double? price = null;
            double? total = null;
            double? dayMin = null;
            double? dayMax = null;
            double dayWin = 0;
            string hour = "";
            hpdata.RowType = "Total";
            hpdata.RowDisplayType = "Total";
            hpdata.RowDay = type;
            hpdata.RowName = rowname;
            maxDayDart.RowType = "Max";
            maxDayDart.RowDisplayType = "Max";
            maxDayDart.RowDay = type;
            maxDayDart.RowName = rowname;
            minDayDart.RowType = "Min";
            minDayDart.RowDisplayType = "Min";
            minDayDart.RowDay = type;
            minDayDart.RowName = rowname;

            if (totalday == 0)
            {
                for (int i = 0; i < aList.Count; i++)
                {
                    try
                    {
                        double? daprice = null;
                        hour = "";
                        hour = "HE" + aList[i].MarketTime.Hour.ToString();
                        if (hour.Equals("HE0"))
                        {
                            hour = "HE24";
                        }
                        var tempprice = hpdata.GetType().GetProperty(hour).GetValue(hpdata, null);
                        price = tempprice == null ? double.NaN : Convert.ToDouble(tempprice);
                        tempprice = hpdata.GetType().GetProperty("Total").GetValue(hpdata, null);
                        total = tempprice == null ? double.NaN : Convert.ToDouble(tempprice);
                        if (!aList[i].Price.Equals(double.NaN))
                        {
                            if (dayMax == null || dayMax < aList[i].Price)
                            {
                                dayMax = aList[i].Price;
                            }
                            if (aList[i].Price > 0)
                            {
                                dayWin++;
                            }
                            if (dayMin == null || dayMin > aList[i].Price)
                            {
                                dayMin = aList[i].Price;
                            }
                            if (total.Equals(double.NaN))
                            {
                                total = aList[i].Price;
                            }
                            else
                            {
                                total += aList[i].Price;
                            }
                            daprice = aList[i].Price;
                            int hr = int.Parse(hour.TrimStart('H', 'E'));
                            avgcounter[hr == 24 ? 0 : hr] = avgcounter[hr == 24 ? 0 : hr] + 1;
                            if (!price.Equals(double.NaN))
                            {
                                price += (daprice != null ? daprice : 0);
                            }
                            else
                            {
                                price = (daprice != null ? daprice : 0);
                            }
                            var max = maxDayDart.GetType().GetProperty(hour).GetValue(maxDayDart, null);
                            if (max != null && Convert.ToDouble(max) < daprice)
                            {
                                maxDayDart.GetType().GetProperty(hour).SetValue(maxDayDart, daprice, null);
                            }
                            else if (max == null)
                            {
                                maxDayDart.GetType().GetProperty(hour).SetValue(maxDayDart, daprice, null);
                            }
                            var min = minDayDart.GetType().GetProperty(hour).GetValue(minDayDart, null);
                            if (min != null && Convert.ToDouble(min) > daprice)
                            {
                                minDayDart.GetType().GetProperty(hour).SetValue(minDayDart, daprice, null);
                            }
                            else if (min == null)
                            {
                                minDayDart.GetType().GetProperty(hour).SetValue(minDayDart, daprice, null);
                            }
                            if (type.Equals("DART"))
                            {
                                if (daprice > 0)
                                {
                                    var win = winDayDart.GetType().GetProperty(hour).GetValue(winDayDart, null);
                                    if (win != null)
                                    {
                                        winDayDart.GetType().GetProperty(hour).SetValue(winDayDart, Convert.ToDouble(win) + 1, null);
                                    }
                                    else if (win == null)
                                    {
                                        winDayDart.GetType().GetProperty(hour).SetValue(winDayDart, 1d, null);
                                    }
                                }
                            }
                            hpdata.GetType().GetProperty(hour).SetValue(hpdata, price, null);
                            hpdata.GetType().GetProperty("Total").SetValue(hpdata, total, null);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else
            {
                for (int k = 0; k < totalday; k++)
                {
                    int j = 0;
                    j = k == 0 ? 0 : k * 24;
                    for (int i = 0; i < 24; i++)
                    {
                        double? daprice = null;
                        hour = "";
                        hour = "HE" + aList[i + j].MarketTime.Hour.ToString();
                        if (hour.Equals("HE0"))
                        {
                            hour = "HE24";
                        }
                        var tempprice = hpdata.GetType().GetProperty(hour).GetValue(hpdata, null);
                        price = tempprice == null ? double.NaN : Convert.ToDouble(tempprice);
                        tempprice = hpdata.GetType().GetProperty("Total").GetValue(hpdata, null);
                        total = tempprice == null ? double.NaN : Convert.ToDouble(tempprice);
                        if (!aList[i + j].Price.Equals(double.NaN))
                        {
                            if (dayMax == null || dayMax < aList[i + j].Price)
                            {
                                dayMax = aList[i + j].Price;
                            }
                            if (aList[i + j].Price > 0)
                            {
                                dayWin++;
                            }
                            if (dayMin == null || dayMin > aList[i + j].Price)
                            {
                                dayMin = aList[i + j].Price;
                            }
                            if (total.Equals(double.NaN))
                            {
                                total = aList[i + j].Price;
                            }
                            else
                            {
                                total += aList[i + j].Price;
                            }
                            daprice = aList[i + j].Price;
                            int hr = int.Parse(hour.TrimStart('H', 'E'));
                            avgcounter[hr == 24 ? 0 : hr] = avgcounter[hr == 24 ? 0 : hr] + 1;
                            if (!price.Equals(double.NaN))
                            {
                                price += (daprice != null ? daprice : 0);
                            }
                            else
                            {
                                price = (daprice != null ? daprice : 0);
                            }
                            var max = maxDayDart.GetType().GetProperty(hour).GetValue(maxDayDart, null);
                            if (max != null && Convert.ToDouble(max) < daprice)
                            {
                                maxDayDart.GetType().GetProperty(hour).SetValue(maxDayDart, daprice, null);
                            }
                            else if (max == null)
                            {
                                maxDayDart.GetType().GetProperty(hour).SetValue(maxDayDart, daprice, null);
                            }
                            var min = minDayDart.GetType().GetProperty(hour).GetValue(minDayDart, null);
                            if (min != null && Convert.ToDouble(min) > daprice)
                            {
                                minDayDart.GetType().GetProperty(hour).SetValue(minDayDart, daprice, null);
                            }
                            else if (min == null)
                            {
                                minDayDart.GetType().GetProperty(hour).SetValue(minDayDart, daprice, null);
                            }
                            if (type.Equals("DART"))
                            {
                                if (daprice > 0)
                                {
                                    var win = winDayDart.GetType().GetProperty(hour).GetValue(winDayDart, null);
                                    if (win != null)
                                    {
                                        winDayDart.GetType().GetProperty(hour).SetValue(winDayDart, Convert.ToDouble(win) + 1, null);
                                    }
                                    else if (win == null)
                                    {
                                        winDayDart.GetType().GetProperty(hour).SetValue(winDayDart, 1d, null);
                                    }
                                }
                            }
                            hpdata.GetType().GetProperty(hour).SetValue(hpdata, price, null);
                            hpdata.GetType().GetProperty("Total").SetValue(hpdata, total, null);
                        }
                    }
                }
            }
            for (int i = 0; i < hrs; i++)
            {
                double? daprice = null;
                hour = "";
                int index = totalday * 24;
                hour = "HE" + aList[i + index].MarketTime.Hour.ToString();
                if (hour.Equals("HE0"))
                {
                    hour = "HE24";
                }
                var tempprice = hpdata.GetType().GetProperty(hour).GetValue(hpdata, null);
                price = tempprice == null ? double.NaN : Convert.ToDouble(tempprice);
                tempprice = hpdata.GetType().GetProperty("Total").GetValue(hpdata, null);
                total = tempprice == null ? double.NaN : Convert.ToDouble(tempprice);
                if (!aList[i + index].Price.Equals(double.NaN))
                {
                    if (total.Equals(double.NaN))
                    {
                        total = aList[i + index].Price;
                    }
                    else
                    {
                        total += aList[i + index].Price;
                    }
                    var max = maxDayDart.GetType().GetProperty(hour).GetValue(maxDayDart, null);
                    if (max != null && Convert.ToDouble(max) < aList[i + index].Price)
                    {
                        maxDayDart.GetType().GetProperty(hour).SetValue(maxDayDart, aList[i + index].Price, null);
                    }
                    var min = minDayDart.GetType().GetProperty(hour).GetValue(minDayDart, null);
                    if (min != null && Convert.ToDouble(min) > aList[i + index].Price)
                    {
                        minDayDart.GetType().GetProperty(hour).SetValue(minDayDart, aList[i + index].Price, null);
                    }
                    if (type.Equals("DART"))
                    {
                        if (price > 0)
                        {
                            var win = winDayDart.GetType().GetProperty(hour).GetValue(winDayDart, null);
                            if (win != null)
                            {
                                winDayDart.GetType().GetProperty(hour).SetValue(winDayDart, Convert.ToDouble(win) + 1, null);
                            }
                            else if (win == null)
                            {
                                winDayDart.GetType().GetProperty(hour).SetValue(winDayDart, 1d, null);
                            }
                        }
                    }
                    daprice = aList[i + index].Price;
                    int hr = int.Parse(hour.TrimStart('H', 'E'));
                    avgcounter[hr == 24 ? 0 : hr] = avgcounter[hr == 24 ? 0 : hr] + 1;
                    if (!price.Equals(double.NaN))
                    {
                        price += (daprice != null ? daprice : 0);
                    }
                    else
                    {
                        price = (daprice != null ? daprice : 0);
                    }
                    hpdata.GetType().GetProperty(hour).SetValue(hpdata, price, null);
                    hpdata.GetType().GetProperty("Total").SetValue(hpdata, total, null);
                }
            }
            averageCounter = avgcounter.Count(t => t > 0);
            hpdata.Average = total / averageCounter;
            if (FilterDayComparisonTotalChecked)
            {
                hourlyPivotData.Add(hpdata);
            }
            double? sumTotal = total;
            HourlyPivotData hpdataavg = new HourlyPivotData();
            total = 0.0;
            int upper = 0;
            if (totalday > 0)
            {
                upper = 24;
            }
            else
            {
                upper = hrs;
            }
            for (int i = 0; i < upper; i++)
            {
                price = null;
                hour = "";
                if (i == 0)
                {
                    hour = "HE24";
                }
                else
                {
                    hour = "HE" + i;
                }
                var tempprice = hpdata.GetType().GetProperty(hour).GetValue(hpdata, null);
                price = tempprice == null ? double.NaN : Convert.ToDouble(tempprice);
                if (avgcounter[i] > 0)
                {
                    if (price.Equals(double.NaN))
                    {
                        hpdataavg.GetType().GetProperty(hour).SetValue(hpdataavg, null, null);
                    }
                    else
                    {
                        price /= avgcounter[i];
                        hpdataavg.GetType().GetProperty(hour).SetValue(hpdataavg, price, null);
                        total += price;
                    }
                    var win = winDayDart.GetType().GetProperty(hour).GetValue(winDayDart, null);
                    if (Convert.ToInt16(win) > 0)
                    {
                        double winP = (Convert.ToDouble(win) * 100 / avgcounter[i]);
                        winDayDart.GetType().GetProperty(hour).SetValue(winDayDart, winP, null);
                    }
                }
            }
            if (FilterDayComparisonMaxChecked)
            {
                maxDayDart.RowType = "Max";
                maxDayDart.RowDisplayType = "Max";
                maxDayDart.RowDay = type;
                maxDayDart.RowName = rowname;
                hourlyPivotData.Add(maxDayDart);
            }
            if (FilterDayComparisonMinChecked)
            {
                minDayDart.RowType = "Min";
                minDayDart.RowDisplayType = "Min";
                minDayDart.RowDay = type;
                minDayDart.RowName = rowname;
                hourlyPivotData.Add(minDayDart);
            }
            if (FilterDayComparisonWinPctChecked)
            {
                winDayDart.RowType = "Win %";
                winDayDart.RowDisplayType = "Win %";
                winDayDart.RowDay = type;
                winDayDart.RowName = rowname;
                hourlyPivotData.Add(winDayDart);
            }
            hpdataavg.RowName = hpdata.GetType().GetProperty("RowName").GetValue(hpdata, null).ToString();
            hpdataavg.RowType = "Average";
            hpdataavg.RowDisplayType = "Average";
            hpdataavg.RowDay = type;
            hpdataavg.Total = total;
            maxDayDart.Average = dayMax;
            minDayDart.Average = dayMin;
            if (FilterDayComparisonDARTChecked && dayWin > 0 && aList.Count > 0)
            {
                winDayDart.Average = Math.Round((dayWin / aList.Count) * 100, 2);
            }
            if (sumTotal != null && aList.Count > 0)
            {
                hpdataavg.Average = sumTotal / aList.Count;
            }
            if (mFilterDayComparisonAvgChecked)
            {
                hourlyPivotData.Add(hpdataavg);
            }
        }
        /// <summary>
        /// Creates the plot model upper.
        /// </summary>
        /// <param name="nodeList">The node list.</param>
        /// <returns></returns>
        private PlotModel CreatePlotModelUpper(List<Node> nodeList)
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            /// string title = sourceSinkData.Sink == null ? (sourceSinkData.Source.ToString() + "(" + sourceSinkData.Source.Zone + ")") : sourceSinkData.Source + "(" + sourceSinkData.Source.Zone + ")" + " -> " + sourceSinkData.Sink + "(" + sourceSinkData.Sink.Zone + ")";
            /// // Construct the title with zones included
            string title = sourceSinkData.Sink == null
                ? $"{sourceSinkData.Source} ({sourceSinkData.Source.Zone})"
                : $"{sourceSinkData.Source} ({sourceSinkData.Source.Zone}) -> {sourceSinkData.Sink} ({sourceSinkData.Sink.Zone})";
            var plotModel1 = new PlotModel()
            {
                //Title = title
            };
            var c = OxyColors.DarkBlue;
            plotModel1.Axes.Add(new LinearAxis()
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
                EndPosition = 1
            });
            plotModel1.Axes.Add(new LinearAxis()
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
                StartPosition = 0,
                EndPosition = 1,
                TickStyle = TickStyle.Outside,
                FontSize = 10,
                IsTickCentered = true,
                Key = "XAxisCategory",
                GapWidth = 0.05,
                AxisTitleDistance = 30
            };
            if (nodeList[nodeList.Count - 1].LmpTimePriceList.Count > 96 && nodeList[nodeList.Count - 1].LmpTimePriceList.Count < 720)
            {
                categoryAxis.MajorStep = 8;
            }
            else if (nodeList[nodeList.Count - 1].LmpTimePriceList.Count > 720)
            {
                categoryAxis.MajorStep = nodeList[nodeList.Count - 1].LmpTimePriceList.Count / 48;
            }
            plotModel1.Axes.Add(categoryAxis);
            // plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 70);
            var colSeries1 = new BarSeries();
            //string colTitle = sourceSinkData.Sink == null ? nodeList[nodeList.Count - 1].NodeName : nodeList[nodeList.Count - 1].NodeName + " spread";
            string colTitle = sourceSinkData.Sink == null
                ? $"{sourceSinkData.Source} ({sourceSinkData.Source.Zone})"
                : $"{sourceSinkData.Source} ({sourceSinkData.Source.Zone}) -> {sourceSinkData.Sink} ({sourceSinkData.Sink.Zone})" + " spread";
            colSeries1 = new BarSeries()
            {
                Title = colTitle,
                YAxisKey = "XAxisCategory",
                NegativeFillColor = OxyColors.MediumBlue,
                FillColor = OxyColors.IndianRed,
                XAxisKey = "Y2Axis",

            };
            for (int i = 0; i < nodeList[nodeList.Count - 1].LmpTimePriceList.Count; i++)
            {
                if (SelectedPeriodType == PeriodType.Daily)
                {
                    categoryAxis.Labels.Add(nodeList[nodeList.Count - 1].LmpTimePriceList[i].MarketTime.ToString("M/d/yy"));
                }
                else
                {
                    categoryAxis.Labels.Add(nodeList[nodeList.Count - 1].LmpTimePriceList[i].MarketTime.ToString("M/d/yy 'HE'H"));
                }
                var colItem1 = new BarItem(Math.Round(nodeList[nodeList.Count - 1].LmpTimePriceList[i].Lmp.Price, 2), i);
                colSeries1.Items.Add(colItem1);
            }
            plotModel1.Series.Add(colSeries1);
            for (int j = 0; j < nodeList.Count - 1; j++)
            {
                var lineSeries1 = new LineSeries();
                dataItemValues = new Collection<Item>();
                for (int i = 0; i < nodeList[j].LmpTimePriceList.Count; i++)
                {
                    if (selectedType == "Congestion" && j < 2)
                    {
                        dataItemValues.Add(new Item() { X = i, Y = Math.Round(nodeList[j].LmpTimePriceList[i].Lmp.Congestion, 2) });
                    }
                    else if (selectedType == "Loss" && j < 2)
                    {
                        dataItemValues.Add(new Item() { X = i, Y = Math.Round(nodeList[j].LmpTimePriceList[i].Lmp.Loss, 2) });
                    }
                    else if (selectedType == "Energy" && j < 2)
                    {
                        if (!double.IsNaN(nodeList[j].LmpTimePriceList[i].Lmp.Energy))
                        {
                            dataItemValues.Add(new Item() { X = i, Y = Math.Round(nodeList[j].LmpTimePriceList[i].Lmp.Energy, 2) });
                        }
                        else
                        {
                            dataItemValues.Add(new Item() { X = i, Y = Math.Round(nodeList[j].LmpTimePriceList[i].Lmp.Price - nodeList[j].LmpTimePriceList[i].Lmp.Congestion - nodeList[j].LmpTimePriceList[i].Lmp.Loss, 2) });
                        }
                    }
                    else
                    {
                        dataItemValues.Add(new Item() { X = i, Y = Math.Round(nodeList[j].LmpTimePriceList[i].Lmp.Price, 2) });
                    }
                }
                lineSeries1 = new LineSeries()
                {
                    CanTrackerInterpolatePoints = false,
                    DataFieldX = "X",
                    DataFieldY = "Y",
                    ItemsSource = dataItemValues,
                    TrackerFormatString = "{0}\n{2:M/d/yy}\n{4:$0.00}",
                    XAxisKey = "XAxisCategory",
                    MarkerType = MarkerType.Diamond,
                    MarkerSize = 2,
                    MarkerStrokeThickness = 0
                };
                if (SelectedSpreadType == SpreadType.DART)
                {
                    string add = sourceSinkData.Sink == null ? "" : " Spread";
                    if (j == 0)
                    {
                        lineSeries1.Title = "DA" + add;
                    }
                    else if (j == 1)
                    {
                        lineSeries1.Title = "RT" + add;
                    }
                }
                else
                {
                    if (j == 0)
                    {
                        lineSeries1.Title = nodeList[j].NodeName + " (source)";
                        lineSeries1.Color = OxyColors.Goldenrod;
                        lineSeries1.MarkerFill = OxyColors.Goldenrod;
                    }
                    else if (j == 1)
                    {
                        lineSeries1.Title = nodeList[j].NodeName + " (sink)";
                        lineSeries1.Color = OxyColors.Green;
                        lineSeries1.MarkerFill = OxyColors.Green;
                    }
                }
                plotModel1.Series.Add(lineSeries1);
            }
            var l = new Legend
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight
            };
            plotModel1.Legends.Add(l);
            if (nodeList[nodeList.Count - 1].LmpTimePriceList.Count > 0)
            {
                double columnValuesMax = nodeList[nodeList.Count - 1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price) ? double.MinValue : d.Lmp.Price).Max();
                double columnValuesMin = nodeList[nodeList.Count - 1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price) ? double.MaxValue : d.Lmp.Price).Min();
                double columnAbsMaxMin = Math.Max(Math.Abs(columnValuesMax), Math.Abs(columnValuesMin));
                double seriesSrcValuesMax = 0;
                if (selectedType == "Congestion")
                {
                    seriesSrcValuesMax = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Congestion) ? double.MinValue : d.Lmp.Congestion).Max();
                }
                else if (selectedType == "Loss")
                {
                    seriesSrcValuesMax = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Loss) ? double.MinValue : d.Lmp.Loss).Max();
                }
                else if (selectedType == "Energy")
                {
                    if (!double.IsNaN(nodeList[0].LmpTimePriceList[0].Lmp.Energy))
                    {
                        seriesSrcValuesMax = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Energy) ? double.MinValue : d.Lmp.Energy).Max();
                    }
                    else
                    {
                        seriesSrcValuesMax = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss) ? double.MinValue : d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss).Max();
                    }
                }
                else
                {
                    seriesSrcValuesMax = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price) ? double.MinValue : d.Lmp.Price).Max();
                }
                double seriesSinkValuesMax = 0;
                if (selectedType == "Congestion")
                {
                    seriesSinkValuesMax = nodeList.Count < 2 ? double.MinValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Congestion) ? double.MinValue : d.Lmp.Congestion).Max();
                }
                else if (selectedType == "Loss")
                {
                    seriesSinkValuesMax = nodeList.Count < 2 ? double.MinValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Loss) ? double.MinValue : d.Lmp.Loss).Max();
                }
                else if (selectedType == "Energy")
                {
                    if (!double.IsNaN(nodeList[0].LmpTimePriceList[0].Lmp.Energy))
                    {
                        seriesSinkValuesMax = nodeList.Count < 2 ? double.MinValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Energy) ? double.MinValue : d.Lmp.Energy).Max();
                    }
                    else
                    {
                        seriesSinkValuesMax = nodeList.Count < 2 ? double.MinValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss) ?
                        double.MinValue : d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss).Max();
                    }
                }
                else
                {
                    seriesSinkValuesMax = nodeList.Count < 2 ? double.MinValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price) ? double.MinValue : d.Lmp.Price).Max();
                }
                double seriesValuesMax = Math.Max(seriesSrcValuesMax, seriesSinkValuesMax);
                double seriesSrcValuesMin = 0;
                if (selectedType == "Congestion")
                {
                    seriesSrcValuesMin = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Congestion) ? double.MaxValue : d.Lmp.Congestion).Min();
                }
                else if (selectedType == "Loss")
                {
                    seriesSrcValuesMin = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Loss) ? double.MaxValue : d.Lmp.Loss).Min();
                }
                else if (selectedType == "Energy")
                {
                    if (!double.IsNaN(nodeList[0].LmpTimePriceList[0].Lmp.Energy))
                    {
                        seriesSrcValuesMin = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Energy) ? double.MaxValue : d.Lmp.Energy).Min();
                    }
                    else
                    {
                        seriesSrcValuesMin = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss) ? double.MaxValue : d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss).Min();
                    }
                }
                else
                {
                    seriesSrcValuesMin = nodeList[0].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price) ? double.MaxValue : d.Lmp.Price).Min();
                }
                double seriesSinkValuesMin = 0;
                if (selectedType == "Congestion")
                {
                    seriesSinkValuesMin = nodeList.Count < 2 ? double.MaxValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Congestion) ? double.MaxValue : d.Lmp.Congestion).Min();
                }
                else if (selectedType == "Loss")
                {
                    seriesSinkValuesMin = nodeList.Count < 2 ? double.MaxValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Loss) ? double.MaxValue : d.Lmp.Loss).Min();
                }
                else if (selectedType == "Energy")
                {
                    if (!double.IsNaN(nodeList[0].LmpTimePriceList[0].Lmp.Energy))
                    {
                        seriesSinkValuesMin = nodeList.Count < 2 ? double.MaxValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Energy) ?
                            double.MaxValue : d.Lmp.Energy).Min();
                    }
                    else
                    {
                        seriesSinkValuesMin = nodeList.Count < 2 ? double.MaxValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss) ?
                            double.MaxValue : d.Lmp.Price - d.Lmp.Congestion - d.Lmp.Loss).Min();
                    }
                }
                else
                {
                    seriesSinkValuesMin = nodeList.Count < 2 ? double.MaxValue : nodeList[1].LmpTimePriceList.Select(d => double.IsNaN(d.Lmp.Price) ? double.MaxValue : d.Lmp.Price).Min();
                }
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
                Position = AxisPosition.Right,
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
            if (nodeList[nodeList.Count - 1].LmpTimePriceList.Count > 96 && nodeList[nodeList.Count - 1].LmpTimePriceList.Count < 720)
            {
                categoryAxis.MajorStep = 8;
            }
            else if (nodeList[nodeList.Count - 1].LmpTimePriceList.Count > 720)
            {
                categoryAxis.MajorStep = nodeList[nodeList.Count - 1].LmpTimePriceList.Count / 48;
            }
            plotModel1.Axes.Add(categoryAxis);
            // plotModel1.PlotMargins = new OxyThickness(20, 4, 20, 2);
            for (int i = 0; i < nodeList[nodeList.Count - 1].LmpTimePriceList.Count; i++)
            {
                if (SelectedPeriodType == PeriodType.Daily)
                {
                    categoryAxis.Labels.Add(nodeList[nodeList.Count - 1].LmpTimePriceList[i].MarketTime.ToString("M/d/yy"));
                }
                else
                {
                    categoryAxis.Labels.Add(nodeList[nodeList.Count - 1].LmpTimePriceList[i].MarketTime.ToString("M/d/yy 'HE'H"));
                }
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
                YAxisKey = "Y2AxisB"
            };
            var data = new Collection<DateValue>();
            double accumulator = 0;
            for (int i = 0; i < nodeList[nodeList.Count - 1].LmpTimePriceList.Count; i++)
            {
                if (!nodeList[nodeList.Count - 1].LmpTimePriceList[i].Lmp.Price.Equals(double.NaN))
                {
                    accumulator += nodeList[nodeList.Count - 1].LmpTimePriceList[i].Lmp.Price;
                }
                areaSeries1.Points.Add(new DataPoint(i, accumulator));
                areaSeries1.Points2.Add(new DataPoint(i, 0));
            }
            areaSeries1.Title = nodeList[nodeList.Count - 1].NodeName + " spread sum";
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
            if (SelectedPeriodType == PeriodType.Daily)
            {
                dtAxis = new DateTimeAxis()//StartDate.AddDays(-0.5), EndDate.AddDays(0.5), AxisPosition.Bottom, null, null, DateTimeIntervalType.Days
                {
                    Position = AxisPosition.Bottom,
                    Minimum = Convert.ToDouble(StartDate.AddDays(-0.5)),
                    Maximum = Convert.ToDouble(EndDate.AddDays(0.5)),
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
            else
            {
                dtAxis = new DateTimeAxis()//StartDate, EndDate.Date.AddDays(1), AxisPosition.Bottom, null, null, DateTimeIntervalType.Hours
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromAColor(20, c),
                    Angle = 90,
                    StringFormat = "M/d/yy H:mm",
                    IntervalType = DateTimeIntervalType.Hours,
                    MinorIntervalType = DateTimeIntervalType.Hours,
                    IsZoomEnabled = true,
                    Position = AxisPosition.Bottom,
                    Minimum = Convert.ToDouble(StartDate),
                    Maximum = Convert.ToDouble(EndDate.Date.AddDays(1)),
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
        /// <summary>
        /// Converts the daily values.
        /// </summary>
        /// <param name="hourlyNodeList">The hourly node list.</param>
        /// <returns></returns>
        private List<Node> ConvertDailyValues(List<Node> hourlyNodeList)
        {
            List<Node> dailyNodeList = new List<Node>();
            int i = 0;
            foreach (Node node in hourlyNodeList)
            {
                Dictionary<DateTime, double> dailyPriceHash = new Dictionary<DateTime, double>();
                foreach (LmpTimePrice timePrice in node.LmpTimePriceList)
                {
                    DateTime date = timePrice.MarketTime.Hour == 0 ? timePrice.MarketTime.Date.AddDays(-1) : timePrice.MarketTime.Date;
                    if (!timePrice.Lmp.Price.Equals(double.NaN))
                    {
                        double price = 0;
                        if (selectedType == "Congestion" && i < 2)
                        {
                            price = timePrice.Lmp.Congestion - Fee;
                        }
                        else if (selectedType == "Loss" && i < 2)
                        {
                            price = timePrice.Lmp.Loss - Fee;
                        }
                        else if (selectedType == "Energy" && i < 2)
                        {
                            price = timePrice.Lmp.Price - timePrice.Lmp.Congestion - timePrice.Lmp.Loss - Fee;
                        }
                        else
                        {
                            price = timePrice.Lmp.Price - Fee;
                        }
                        if (dailyPriceHash.ContainsKey(date))
                        {
                            price += dailyPriceHash[date];
                            dailyPriceHash.Remove(date);
                        }
                        dailyPriceHash.Add(date, Math.Round(price, 2));
                    }
                }
                i++;
                List<LmpTimePrice> dailyPriceList = new List<LmpTimePrice>();
                List<DateTime> dateKeys = dailyPriceHash.Keys.ToList<DateTime>();
                foreach (DateTime date in dateKeys)
                {
                    Vayu.NodePriceLibrary.LMP passedLmp = new Vayu.NodePriceLibrary.LMP();
                    if (selectedType == "Congestion")
                    {
                        passedLmp.Congestion = dailyPriceHash[date];
                    }
                    if (selectedType == "Loss")
                    {
                        passedLmp.Loss = dailyPriceHash[date];
                    }
                    if (selectedType == "Energy")
                    {
                        passedLmp.Energy = dailyPriceHash[date];
                    }
                    passedLmp.Price = dailyPriceHash[date];
                    dailyPriceList.Add(new LmpTimePrice() { MarketTime = date, Lmp = passedLmp });
                }
                if (SelectedSortType == SortType.Date)
                {
                    dailyPriceList = dailyPriceList.OrderBy(x => x.MarketTime).ToList();
                }
                else
                {
                    dailyPriceList = dailyPriceList.OrderBy(x => x.Lmp.Price).ToList();
                }
                Node dailyNode = new Node(node);
                dailyNode.LmpTimePriceList = dailyPriceList;
                dailyNodeList.Add(dailyNode);
            }
            return dailyNodeList;
        }
        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="timePrice">The time price.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="mHourList">The m hour list.</param>
        /// <returns>
        ///   <c>true</c> if the specified time price is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValid(LmpTimePrice timePrice, string nodeName, List<DateTime> mHourList)
        {
            if (mHourList.Contains(timePrice.MarketTime))
            {
                return false;
            }
            if ((!mHE1Checked && timePrice.MarketTime.Hour == 1) || (!mHE24Checked && timePrice.MarketTime.Hour == 0) ||
                (!mHE2Checked && timePrice.MarketTime.Hour == 2) || (!mHE3Checked && timePrice.MarketTime.Hour == 3) ||
                (!mHE4Checked && timePrice.MarketTime.Hour == 4) || (!mHE5Checked && timePrice.MarketTime.Hour == 5) ||
                (!mHE6Checked && timePrice.MarketTime.Hour == 6) || (!mHE7Checked && timePrice.MarketTime.Hour == 7) ||
                (!mHE8Checked && timePrice.MarketTime.Hour == 8) || (!mHE9Checked && timePrice.MarketTime.Hour == 9) ||
                (!mHE10Checked && timePrice.MarketTime.Hour == 10) || (!mHE11Checked && timePrice.MarketTime.Hour == 11) ||
                (!mHE12Checked && timePrice.MarketTime.Hour == 12) || (!mHE13Checked && timePrice.MarketTime.Hour == 13) ||
                (!mHE14Checked && timePrice.MarketTime.Hour == 14) || (!mHE15Checked && timePrice.MarketTime.Hour == 15) ||
                (!mHE16Checked && timePrice.MarketTime.Hour == 16) || (!mHE17Checked && timePrice.MarketTime.Hour == 17) ||
                (!mHE18Checked && timePrice.MarketTime.Hour == 18) || (!mHE19Checked && timePrice.MarketTime.Hour == 19) ||
                (!mHE20Checked && timePrice.MarketTime.Hour == 20) || (!mHE21Checked && timePrice.MarketTime.Hour == 21) ||
                (!mHE22Checked && timePrice.MarketTime.Hour == 22) || (!mHE23Checked && timePrice.MarketTime.Hour == 23))
            {
                return false;
            }
            DateTime checkDate = timePrice.MarketTime.Hour == 0 ? timePrice.MarketTime.AddDays(-1) : timePrice.MarketTime;
            if ((!mMondayChecked && checkDate.DayOfWeek == DayOfWeek.Monday) || (!mTuesdayChecked && checkDate.DayOfWeek == DayOfWeek.Tuesday) ||
                (!mWednesdayChecked && checkDate.DayOfWeek == DayOfWeek.Wednesday) || (!mThursdayChecked && checkDate.DayOfWeek == DayOfWeek.Thursday) ||
               (!mFridayChecked && checkDate.DayOfWeek == DayOfWeek.Friday) || (!mSaturdayChecked && checkDate.DayOfWeek == DayOfWeek.Saturday) ||
                (!mSundayChecked && checkDate.DayOfWeek == DayOfWeek.Sunday))
            {
                return false;
            }
            if ((!mJanChecked && checkDate.Month == 1) || (!mFebChecked && checkDate.Month == 2) ||
                (!mMarChecked && checkDate.Month == 3) || (!mAprChecked && checkDate.Month == 4) ||
                (!mMayChecked && checkDate.Month == 5) || (!mJunChecked && checkDate.Month == 6) ||
                (!mJulChecked && checkDate.Month == 7) || (!mAugChecked && checkDate.Month == 8) ||
                (!mSepChecked && checkDate.Month == 9) || (!mOctChecked && checkDate.Month == 10) ||
                (!mNovChecked && checkDate.Month == 11) || (!mDecChecked && checkDate.Month == 12))
            {
                return false;
            }
            return true;
        }
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
                typeList.Add("DA Cong.");
                typeList.Add("RT");
                typeList.Add("RT Cong.");
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
        /// Sets the invalid hours.
        /// </summary>
        /// <param name="daList">The da list.</param>
        /// <param name="rtList">The rt list.</param>
        /// <param name="dartList">The dart list.</param>
        /// <param name="product">The product.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        private List<DateTime> SetInvalidHours(List<Node> daList, List<Node> rtList, List<Node> dartList, string product, string filterType, double? min, double? max)
        {
            List<DateTime> mHourList = new List<DateTime>();
            if (product == "Price")
            {
                List<Node> filterList = new List<Node>();
                if (filterType != null)
                {
                    if (filterType.IndexOf("DART") != -1)
                    {
                        filterList = dartList;
                    }
                    else if (filterType.IndexOf("RT") != -1)
                    {
                        filterList = rtList;
                    }
                    else if (filterType.IndexOf("DA") != -1)
                    {
                        filterList = daList;
                    }
                }
                List<LmpTimePrice> timePriceList = filterList[filterList.Count - 1].LmpTimePriceList;
                foreach (LmpTimePrice timePrice in timePriceList)
                {
                    if (filterType.IndexOf("Con") != -1)
                    {
                        if (timePrice.Lmp.Congestion > max || timePrice.Lmp.Congestion < min)
                        {
                            mHourList.Add(timePrice.MarketTime);
                        }
                    }
                    else
                    {
                        if (timePrice.Lmp.Price > max || timePrice.Lmp.Price < min)
                        {
                            mHourList.Add(timePrice.MarketTime);
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
            return mHourList;
        }
        /// <summary>
        /// Filters the specified da list.
        /// </summary>
        /// <param name="daList">The da list.</param>
        /// <param name="rtList">The rt list.</param>
        /// <param name="dartList">The dart list.</param>
        /// <param name="product">The product.</param>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        private void Filter(List<Node> daList, List<Node> rtList, List<Node> dartList, string product, string filterType, double? min, double? max)
        {
            List<DateTime> hourList = SetInvalidHours(daList, rtList, dartList, product, filterType, min, max);
            for (int i = 0; i < 3; i++)
            {
                List<Node> tempList = daList;
                if (i == 1)
                {
                    tempList = rtList;
                }
                if (i == 2)
                {
                    tempList = dartList;
                }
                foreach (Node node in tempList)
                {
                    for (int j = node.LmpTimePriceList.Count - 1; j >= 0; j--)
                    {
                        try
                        {
                            if (!IsValid(node.LmpTimePriceList[j], node.NodeName, hourList))
                            {
                                node.LmpTimePriceList.RemoveAt(j);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Simulates this instance.
        /// </summary>
        private void Simulate()
        {
            try
            {
                double min = double.Parse(SimMinText);
                double max = double.Parse(SimMaxText);
                Dictionary<double, double> clearedHash = new Dictionary<double, double>();
                Dictionary<double, double> winHash = new Dictionary<double, double>();
                Dictionary<double, double> pnlHash = new Dictionary<double, double>();
                Dictionary<double, double> posPnlHash = new Dictionary<double, double>();
                Dictionary<double, double> negPnlHash = new Dictionary<double, double>();
                Node daNode = UptosChecked ? mDaFilteredSortedList[2] : mDaFilteredSortedList[0];
                Node dartNode = mDartFilteredSortedList[2];
                double totalCleared = 0;
                for (; min < max;)
                {
                    clearedHash.Add(min, 0);
                    winHash.Add(min, 0);
                    pnlHash.Add(min, 0);
                    for (int i = 0; i < daNode.LmpTimePriceList.Count; i++)
                    {
                        bool cleared = false;
                        if (selectedType == "Congestion")
                        {
                            cleared = ((UptosChecked || !IncChecked) && daNode.LmpTimePriceList[i].Lmp.Congestion < min) ||
                                            (!UptosChecked && IncChecked && daNode.LmpTimePriceList[i].Lmp.Congestion > min);
                        }
                        else if (selectedType == "Energy")
                        {
                            cleared = ((UptosChecked || !IncChecked) && daNode.LmpTimePriceList[i].Lmp.Energy < min) ||
                                            (!UptosChecked && IncChecked && daNode.LmpTimePriceList[i].Lmp.Energy > min);
                        }
                        else if (selectedType == "Loss")
                        {
                            cleared = ((UptosChecked || !IncChecked) && daNode.LmpTimePriceList[i].Lmp.Loss < min) ||
                                            (!UptosChecked && IncChecked && daNode.LmpTimePriceList[i].Lmp.Loss > min);
                        }
                        else
                        {
                            cleared = ((UptosChecked || !IncChecked) && daNode.LmpTimePriceList[i].Lmp.Price < min) ||
                                            (!UptosChecked && IncChecked && daNode.LmpTimePriceList[i].Lmp.Price > min);
                        }
                        if (cleared)
                        {
                            clearedHash[min]++;
                            if (dartNode.LmpTimePriceList[i].Lmp.Price > 0)
                            {
                                winHash[min]++;
                                if (!double.IsNaN(dartNode.LmpTimePriceList[i].Lmp.Price))
                                {
                                    if (posPnlHash.ContainsKey(min))
                                    {
                                        posPnlHash[min] += dartNode.LmpTimePriceList[i].Lmp.Price;
                                    }
                                    else
                                    {
                                        posPnlHash.Add(min, dartNode.LmpTimePriceList[i].Lmp.Price);
                                    }
                                }
                            }
                            else
                            {
                                if (!double.IsNaN(dartNode.LmpTimePriceList[i].Lmp.Price))
                                {
                                    if (negPnlHash.ContainsKey(min))
                                    {
                                        negPnlHash[min] += dartNode.LmpTimePriceList[i].Lmp.Price;
                                    }
                                    else
                                    {
                                        negPnlHash.Add(min, dartNode.LmpTimePriceList[i].Lmp.Price);
                                    }
                                }
                            }
                            if (!double.IsNaN(dartNode.LmpTimePriceList[i].Lmp.Price))
                            {
                                pnlHash[min] += dartNode.LmpTimePriceList[i].Lmp.Price;
                            }
                            totalCleared++;
                        }
                    }
                    min += 0.01;
                }
                List<GraphCoOrdinates> graphCoOrdinates = new List<GraphCoOrdinates>();
                if (daNode.LmpTimePriceList.Count > 0)
                {
                    if (ClearedChecked)
                    {
                        List<double> keyList = clearedHash.Keys.ToList<double>();
                        foreach (double key in keyList)
                        {
                            if (clearedHash[key] > 0)
                            {
                                GraphCoOrdinates graphOrdinate = new GraphCoOrdinates();
                                graphOrdinate.Type = "Cleared";
                                graphOrdinate.XValue = Math.Round(key, 2);
                                graphOrdinate.YValue = (clearedHash[key] / daNode.LmpTimePriceList.Count) * 100;
                                graphCoOrdinates.Add(graphOrdinate);
                            }
                        }
                    }
                    if (WinChecked)
                    {
                        List<double> keyList = clearedHash.Keys.ToList<double>();
                        foreach (double key in keyList)
                        {
                            if (winHash.ContainsKey(key))
                            {
                                if (winHash[key] > 0)
                                {
                                    GraphCoOrdinates graphOrdinate = new GraphCoOrdinates();
                                    graphOrdinate.Type = "Win";
                                    graphOrdinate.XValue = Math.Round(key, 2);
                                    graphOrdinate.YValue = (winHash[key] / clearedHash[key]) * 100;
                                    graphCoOrdinates.Add(graphOrdinate);
                                }
                            }
                        }
                    }
                    if (PnlSumChecked)
                    {
                        List<double> keyList = pnlHash.Keys.ToList<double>();
                        foreach (double key in keyList)
                        {
                            if (pnlHash[key] != 0)
                            {
                                GraphCoOrdinates graphOrdinate = new GraphCoOrdinates();
                                graphOrdinate.Type = "PnlSum";
                                graphOrdinate.XValue = Math.Round(key, 2);
                                graphOrdinate.YValue = Math.Round(pnlHash[key], 2);
                                graphCoOrdinates.Add(graphOrdinate);
                            }
                        }
                    }
                    if (RiskChecked)
                    {
                        List<double> keyList = clearedHash.Keys.ToList<double>();
                        foreach (double key in keyList)
                        {
                            double win = posPnlHash.ContainsKey(key) ? posPnlHash[key] : 0;
                            if (win != 0)
                            {
                                GraphCoOrdinates graphOrdinate = new GraphCoOrdinates();
                                graphOrdinate.Type = "Risk";
                                graphOrdinate.XValue = Math.Round(key, 2);
                                double loss = negPnlHash.ContainsKey(key) ? negPnlHash[key] : 1;
                                graphOrdinate.YValue = win / Math.Abs(loss);
                                graphCoOrdinates.Add(graphOrdinate);
                            }
                        }
                    }
                }
                DrawSimulationGraph(graphCoOrdinates);
            }
            catch (Exception ex)
            {
            }
        }
        private LineSeries CreateBSeries(List<Bollinger> list, string key)
        {
            List<GraphItem> grpList = new List<GraphItem>();
            try
            {
                list.ForEach(dItem =>
                {
                    if (key == "LowerLimit")
                        grpList.Add(new GraphItem
                        {
                            X = dItem.MarketDateTime,
                            Y = dItem.LowerLimit
                        });
                    else if (key == "STDDev")
                        grpList.Add(new GraphItem
                        {
                            X = dItem.MarketDateTime,
                            Y = dItem.StdDev
                        });
                    else if (key == "UpperLimit")
                        grpList.Add(new GraphItem
                        {
                            X = dItem.MarketDateTime,
                            Y = dItem.UpperLimit
                        });
                    else if (key == "DARTTotal")
                        grpList.Add(new GraphItem
                        {
                            X = dItem.MarketDateTime,
                            Y = dItem.DARTTotal
                        });
                    else if (key == "Average")
                        grpList.Add(new GraphItem
                        {
                            X = dItem.MarketDateTime,
                            Y = dItem.Average
                        });
                });
            }
            catch (Exception ex)
            {
            }

            return new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " DARTTotal",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0.5,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = key.ToUpper()
            };
        }

        private LineStyle GetLineStyle(string key)
        {
            if (key.StartsWith("LowerLimit"))
            {
                return LineStyle.Dot;
            }
            else if (key.StartsWith("UpperLimit"))
            {
                return LineStyle.Solid;
            }
            else
            {
                return LineStyle.LongDash;
            }
        }
        /// <summary>
        /// Draws the simulation graph.
        /// </summary>
        /// <param name="graphCoOrdinates">The graph co ordinates.</param>
        private void DrawSimulationGraph(List<GraphCoOrdinates> graphCoOrdinates)
        {
            try
            {
                if (mOxyColorList == null)
                {
                    FillColorList();
                }
                if (plotModel == null)
                {
                    plotModel = new PlotModel();
                    CreateAxes(plotModel);
                }
                if (ClearedChecked)
                {
                    DrawCleared(graphCoOrdinates.Where(a => a.Type.ToLower().Equals("cleared")).ToList());
                }
                else
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("cleared")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("cleared")).FirstOrDefault());
                    }
                }
                if (WinChecked)
                {
                    DrawWin(graphCoOrdinates.Where(a => a.Type.ToLower().Equals("win")).ToList());
                }
                else
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("win")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("win")).FirstOrDefault());
                    }
                }
                if (PnlSumChecked)
                {
                    DrawPNL(graphCoOrdinates.Where(a => a.Type.ToLower().Equals("pnlsum")).ToList());
                }
                else
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("pnlsum")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("pnlsum")).FirstOrDefault());
                    }
                }
                if (RiskChecked)
                {
                    DrawRisk(graphCoOrdinates.Where(a => a.Type.ToLower().Equals("risk")).ToList());
                }
                else
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("risk")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("risk")).FirstOrDefault());
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            PlotSimulation = null;
            PlotSimulation = plotModel as PlotModel;
        }
        /// <summary>
        /// Fills the color list.
        /// </summary>
        private void FillColorListbands()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("LowerLimit", OxyColors.Blue);
            mOxyColorList.Add("STDDev", OxyColors.Red);
            mOxyColorList.Add("UpperLimit", OxyColors.Green);
            mOxyColorList.Add("DARTTotal", OxyColors.DarkGoldenrod);
        }
        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("cleared", OxyColors.Blue);
            mOxyColorList.Add("win", OxyColors.Red);
            mOxyColorList.Add("pnlsum", OxyColors.Green);
            mOxyColorList.Add("risk", OxyColors.Black);
        }
        /// <summary>
        /// Draws the cleared.
        /// </summary>
        /// <param name="list">The list.</param>
        private void DrawCleared(List<GraphCoOrdinates> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("cleared")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("cleared")).FirstOrDefault());
                    }
                    plotModel.Series.Add(CreateSeries(list, "cleared"));
                }
                catch (Exception ex)
                {
                }
            }
        }
        /// <summary>
        /// Draws the win.
        /// </summary>
        /// <param name="list">The list.</param>
        private void DrawWin(List<GraphCoOrdinates> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("win")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("win")).FirstOrDefault());
                    }
                    plotModel.Series.Add(CreateSeries(list, "win"));
                }
                catch (Exception ex)
                {
                }
            }
        }
        /// <summary>
        /// Draws the PNL.
        /// </summary>
        /// <param name="list">The list.</param>
        private void DrawPNL(List<GraphCoOrdinates> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("pnlsum")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("pnlsum")).FirstOrDefault());
                    }
                    plotModel.Series.Add(CreateSeries(list, "pnlsum"));
                }
                catch (Exception ex)
                {
                }
            }
        }
        /// <summary>
        /// Draws the risk.
        /// </summary>
        /// <param name="list">The list.</param>
        private void DrawRisk(List<GraphCoOrdinates> list)
        {
            if (list.Count > 0)
            {
                try
                {
                    if (plotModel.Series.Where(a => a.Title.ToLower().Equals("risk")).Count() > 0)
                    {
                        plotModel.Series.Remove(plotModel.Series.Where(a => a.Title.ToLower().Equals("risk")).FirstOrDefault());
                    }
                    plotModel.Series.Add(CreateSeries(list, "risk"));
                }
                catch (Exception ex)
                {
                }
            }
        }
        /// <summary>
        /// Creates the series.
        /// </summary>
        /// <param name="graphCoOrdinates">The graph co ordinates.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private LineSeries CreateSeries(List<GraphCoOrdinates> graphCoOrdinates, string type)
        {

            List<GraphCoOrdinates> grpList = new List<GraphCoOrdinates>();
            try
            {
                graphCoOrdinates.ForEach(dItem =>
                {
                    grpList.Add(new GraphCoOrdinates
                    {
                        XValue = dItem.XValue,
                        YValue = dItem.YValue
                    });
                });
            }
            catch (Exception ex)
            {
            }
            return new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "XValue",
                DataFieldY = "YValue",
                YAxisKey = type,
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                MarkerSize = 2,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 0,
                Color = mOxyColorList.ContainsKey(type) ? mOxyColorList[type] : OxyColors.MediumPurple,
                LineStyle = LineStyle.Solid,
                Title = type.ToUpper()
            };
        }
        /// <summary>
        /// Creates the axes.
        /// </summary>
        /// <param name="plotModel">The plot model.</param>
        private static void CreateAxes(PlotModel plotModel)
        {
            plotModel.Axes.Add(new LinearAxis
            {
                Key = "cleared",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                TextColor = OxyColors.Blue,
                Position = AxisPosition.Left,
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Key = "win",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                TextColor = OxyColors.Red,
                Position = AxisPosition.Right,
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Key = "pnlsum",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                TextColor = OxyColors.Green,
                Position = AxisPosition.Left,
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Key = "risk",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Right,
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                TextColor = OxyColors.Black,
                TitleColor = OxyColors.Green,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                AxislineThickness = 3,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
            });
            var l = new Legend
            {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                LegendTextColor = OxyColors.Black

            };
            plotModel.Legends.Add(l);
            //plotModel.LegendOrientation = LegendOrientation.Horizontal;
            //plotModel.LegendPlacement = LegendPlacement.Outside;
            //plotModel.LegendPosition = LegendPosition.RightTop;
            plotModel.TitlePadding = 5;
        }
        /// <summary>
        /// Pastes this instance.
        /// </summary>
        public void Paste()
        {
            try
            {
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
                var tempSrcSnkList = new List<SourceSinkData>();
                if (rowData.Count() > 2)
                {
                    foreach (string rowDataitem in rowData)
                    {
                        if (rowDataitem != string.Empty)
                        {
                            string[] rownodeData = rowDataitem.Split('\t');

                            var Sourcedata = SourceNodeList.Where(a => a.NodeName.ToLower() == rownodeData[0].ToLower());
                            var Sinkdata = SinkNodeList.Where(a => a.NodeName.ToLower() == rownodeData[1].ToLower());
                            tempSrcSnkList.Add(new SourceSinkData()
                            {

                                Source = Sourcedata.FirstOrDefault(),
                                Sink = Sinkdata.FirstOrDefault()
                            });
                            SourceSinkList = tempSrcSnkList.ToList();
                            //SourceSinkDataSelected = SourceSinkList[0];
                            //RefreshFilterData(tempSrcSnkList[0].Source.NodeKey + ":" + tempSrcSnkList[0].Sink.NodeKey);
                        }
                    }
                }
                else
                {
                    string[] rownodeData = text.Split('\t');
                    mDataService.GetDeenergizedNodesList((a, e) =>
                    {
                        if (e == null)
                        {
                            if (a != null && a.Count > 0)
                            {
                                DeenergizedNodesList = a;
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(e.Message);
                        }
                    });
                    mDataService.GetInValidPathNodesList((a, e) =>
                    {
                        if (e == null)
                        {
                            if (a != null && a.Count > 0)
                            {
                                InValidPathNodesList = a;
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(e.Message);
                        }
                    });
                    if (rownodeData[0].Replace("\r", "").Replace("\n", "") != null)
                    {
                        if (DeenergizedNodesList.Contains(rownodeData[0]))
                        {
                            //  MessageBox.Show("You have selected DeenergizedNodes as Source", "Deenergized Source");
                        }
                    }
                    if (rownodeData[1].Replace("\r", "").Replace("\n", "") != null)
                    {
                        if (DeenergizedNodesList.Contains(rownodeData[1]))
                        {
                            // MessageBox.Show("You have selected DeenergizedNodes as Sink", "Deenergized Sink");
                        }
                    }
                    string source = rownodeData[0].Replace("\r", "").Replace("\n", "");
                    string sink = rownodeData[1].Replace("\r", "").Replace("\n", "");
                    if ((InValidPathNodesList.Exists(a => a.Source == source && a.Sink == sink)))
                    {
                        MessageBox.Show(String.Format("You have selected InValid Path as Source : {0} and Sink : {1}.", rownodeData[0], rownodeData[1]), "InValid Path");
                    }
                    List<PasteHelper> helperDataList = new List<PasteHelper>();
                    List<PasteHelperVirtual> helperVirtualDataList = new List<PasteHelperVirtual>();
                    string[] cellData = null;
                    foreach (string rowItem in rowData)
                    {
                        if (rowItem != string.Empty)
                        {
                            cellData = rowItem.Split('\t');
                            if (cellData.Length == 4)
                            {
                                helperDataList.Add(new PasteHelper
                                {
                                    Source = cellData[0].Trim(),
                                    Sink = cellData[1].Trim(),
                                    Hours = cellData[2],
                                    Price = cellData[3] == "" ? 0 : Convert.ToDouble(cellData[3])
                                });
                            }
                            else if (cellData.Length == 3)
                            {
                                helperDataList.Add(new PasteHelper
                                {
                                    Source = cellData[0].Trim(),
                                    Sink = cellData[1].Trim(),
                                    Hours = cellData[2],

                                });
                            }
                            else if (cellData.Length == 5)
                            {
                                if (!SelectedPath)
                                {
                                    helperVirtualDataList.Add(new PasteHelperVirtual
                                    {
                                        SourceNode = cellData[0].Trim(),
                                        INC_DEC = cellData[1].Trim(),
                                        Hours = cellData[2],
                                        MW = cellData[3] == "" ? 0 : Convert.ToDouble(cellData[3]),
                                        Price = cellData[4] == "" ? 0 : Convert.ToDouble(cellData[4])
                                    });
                                }
                                else
                                {
                                    helperVirtualDataList.Add(new PasteHelperVirtual
                                    {
                                        SourceNode = cellData[0].Trim(),
                                        SinkNode = cellData[0].Trim(),
                                        INC_DEC = cellData[1].Trim(),
                                        Hours = cellData[2],
                                        MW = cellData[3] == "" ? 0 : Convert.ToDouble(cellData[3]),
                                        Price = cellData[4] == "" ? 0 : Convert.ToDouble(cellData[4])
                                    });
                                }
                            }
                            else if (cellData.Length == 2 && UptosChecked)
                            {
                                helperDataList.Add(new PasteHelper
                                {
                                    Source = cellData[0].Trim(),
                                    Sink = cellData[1].Trim(),
                                });
                            }
                            else if (cellData.Length == 2 && !UptosChecked)
                            {
                                if (!SelectedPath)
                                {
                                    helperVirtualDataList.Add(new PasteHelperVirtual
                                    {
                                        SourceNode = cellData[0].Trim(),
                                        INC_DEC = cellData[1].Trim(),
                                    });
                                }
                                else
                                {
                                    helperVirtualDataList.Add(new PasteHelperVirtual
                                    {
                                        SourceNode = cellData[0].Trim(),
                                        SinkNode = cellData[1].Trim(),
                                        INC_DEC = cellData[1].Trim(),
                                    });
                                }
                            }
                        }
                        if (helperDataList.Count == 1)
                        {
                            break;
                        }
                        else if (helperVirtualDataList.Count == 1)
                        {
                            break;
                        }
                    }
                    if (helperDataList.Count > 0)
                    {
                        if (cellData.Length == 2)
                        {
                            FixSelectedSourceSinks(helperDataList[0], true);
                        }
                        else
                        {
                            FixSelectedSourceSinks(helperDataList[0], false);
                        }
                    }
                    else if (helperVirtualDataList.Count > 0)
                    {
                        if (cellData.Length == 2)
                        {
                            FixSelectedVirtualNode(helperVirtualDataList[0], true);
                        }
                        else
                        {
                            FixSelectedVirtualNode(helperVirtualDataList[0], false);
                        }

                    }
                    else
                    {
                        MessageBox.Show("No data to paste");
                        return;
                    }
                    //if (MarketComboSelectedValue == "ERCOT")
                    //{
                    //    GetDeenergizedNodes(source, sink);

                    //}
                    //else
                    //{
                    //    DeenergizedSourceChecked = false;
                    //    DeenergizedSinkChecked = false;
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        /// <summary>
        /// Fixes the selected source sinks.
        /// </summary>
        /// <param name="helperDataList">The helper data list.</param>
        /// <param name="checkAllData">if set to <c>true</c> [check all data].</param>
        private void FixSelectedSourceSinks(PasteHelper helperDataList, bool checkAllData)
        {
            try
            {
                DataService mds = new DataService();
                var tempSrcSnkList = new List<SourceSinkData>();
                string sourceName = helperDataList.Source;
                string sinkName = helperDataList.Sink;
                var sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == helperDataList.Source.ToLower()).FirstOrDefault();
                var sinkData = SinkNodeList.Where(a => a.NodeName.ToLower() == helperDataList.Sink.ToLower()).FirstOrDefault();
                if ((sourceData == null || sinkData == null) && helperDataList.Source != null && helperDataList.Sink != null)
                {
                    pnodeIdHash = mds.FillPnodeHash();
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

                tempSrcSnkList.Add(new SourceSinkData()
                {
                    Source = sourceData,
                    Sink = sinkData
                });

                RemoveAllFilters();
                if (!checkAllData)
                {
                    // RemoveAllFilters();
                    ClearCheckBoxes();
                    foreach (string hItem in helperDataList.Hours.Split('.'))
                    {
                        int hour = -1;
                        if (int.TryParse(hItem, out hour))
                        {
                            foreach (var propItem in this.GetType().GetProperties().Where(a => a.Name.StartsWith("HE")))
                            {
                                var val = Regex.Match(propItem.Name.Split('E')[propItem.Name.Split('E').Length - 1], @"\d+").Value;
                                if (Convert.ToInt32(val) == hour)
                                {
                                    this.GetType().GetProperty(propItem.Name).SetValue(this, true);
                                }
                            }
                        }
                    }

                    ProductComboSelectedValue = ProductList.Where(a => a.ToLower().Equals("price")).FirstOrDefault();
                    TypeComboSelectedValue = TypeList.Where(a => a.ToLower().Equals("da")).FirstOrDefault();
                    MaxText = helperDataList.Price.ToString();
                    FilterList = new List<FilterData>
                    {
                        new FilterData{
                         Max=helperDataList.Price,
                         Product=ProductComboSelectedValue,
                         Type=TypeComboSelectedValue
                        }
                    };
                }
                else
                {
                    CheckCheckBoxes();
                }
                SourceSinkList = tempSrcSnkList.ToList();
                SourceSinkDataSelected = SourceSinkList[0];
                RefreshFilterData(tempSrcSnkList[0].Source.NodeKey + ":" + tempSrcSnkList[0].Sink.NodeKey);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Fixes the selected virtual node.
        /// </summary>
        /// <param name="helperVirtualDataList">The helper virtual data list.</param>
        /// <param name="checkAllData">if set to <c>true</c> [check all data].</param>
        private void FixSelectedVirtualNode(PasteHelperVirtual helperVirtualDataList, bool checkAllData)
        {
            try
            {
                var tempSrcSnkList = new List<SourceSinkData>();
                if (helperVirtualDataList.INC_DEC.ToUpper() == "I")
                {
                    IncChecked = true;
                }
                else
                {
                    IncChecked = false;
                }
                if (!SelectedPath)
                {
                    var sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == helperVirtualDataList.SourceNode.Trim().ToLower()).FirstOrDefault();
                    tempSrcSnkList.Add(new SourceSinkData()
                    {
                        Source = sourceData,
                    });
                }
                else
                {
                    var sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == helperVirtualDataList.SourceNode.ToLower()).FirstOrDefault();
                    var sinkData = SinkNodeList.Where(a => a.NodeName.ToLower() == helperVirtualDataList.SinkNode.ToLower()).FirstOrDefault();
                    tempSrcSnkList.Add(new SourceSinkData()
                    {
                        Source = sourceData,
                        Sink = sinkData
                    });
                }
                RemoveAllFilters();
                if (!checkAllData)
                {
                    ClearCheckBoxes();
                    foreach (string hItem in helperVirtualDataList.Hours.Split('.'))
                    {
                        int hour = -1;
                        if (int.TryParse(hItem, out hour))
                        {
                            foreach (var propItem in this.GetType().GetProperties().Where(a => a.Name.StartsWith("HE")))
                            {
                                var val = Regex.Match(propItem.Name.Split('E')[propItem.Name.Split('E').Length - 1], @"\d+").Value;
                                if (Convert.ToInt32(val) == hour)
                                {
                                    this.GetType().GetProperty(propItem.Name).SetValue(this, true);
                                }
                            }
                        }
                    }
                    ProductComboSelectedValue = ProductList.Where(a => a.ToLower().Equals("price")).FirstOrDefault();
                    TypeComboSelectedValue = TypeList.Where(a => a.ToLower().Equals("da")).FirstOrDefault();
                    if (helperVirtualDataList.INC_DEC.ToUpper() == "I")
                    {
                        MinText = helperVirtualDataList.Price.ToString();
                        MaxText = "";
                    }
                    else
                    {
                        MaxText = helperVirtualDataList.Price.ToString();
                        MinText = "";
                    }
                    if (helperVirtualDataList.INC_DEC.ToUpper() == "I")
                    {
                        FilterList = new List<FilterData>
                        {
                            new FilterData{
                             Min=helperVirtualDataList.Price,
                             Product=ProductComboSelectedValue,
                             Type=TypeComboSelectedValue
                            }
                        };
                    }
                    else
                    {
                        FilterList = new List<FilterData>
                         {
                            new FilterData{
                             Max=helperVirtualDataList.Price,
                             Product=ProductComboSelectedValue,
                             Type=TypeComboSelectedValue
                            }
                        };
                    }
                }
                else
                {
                    CheckCheckBoxes();
                }

                SourceSinkList = tempSrcSnkList.ToList();
                SourceSinkDataSelected = SourceSinkList[0];
                if (!SelectedPath)
                {
                    RefreshFilterData(tempSrcSnkList[0].Source.NodeKey.ToString());
                }
                else
                {
                    RefreshFilterData(tempSrcSnkList[0].Source.NodeKey.ToString() + ":" + tempSrcSnkList[0].Sink.NodeKey);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Checks the off peak check boxes.
        /// </summary>
        private void CheckOffPeakCheckBoxes()
        {
            SetCheckBoxes("OFFPEAK");
        }

        /// <summary>
        /// Checks the peak check boxes.
        /// </summary>
        private void CheckPeakCheckBoxes()
        {
            SetCheckBoxes("PEAK");
        }

        /// <summary>
        /// Sets the check boxes.
        /// </summary>
        /// <param name="hourType">Type of the hour.</param>
        private void SetCheckBoxes(string hourType)
        {
            if (hourType == "ALL" || hourType == "OFFPEAK")
            {
                HE1Checked = true;
                HE2Checked = true;
                HE3Checked = true;
                HE4Checked = true;
                HE5Checked = true;
                HE6Checked = true;
                if (MarketComboSelectedValue == "PJM" || hourType == "ALL")
                {
                    HE7Checked = true;
                }
                else
                {
                    HE7Checked = false;
                }
                HE24Checked = true;
            }
            else
            {
                HE1Checked = false;
                HE2Checked = false;
                HE3Checked = false;
                HE4Checked = false;
                HE5Checked = false;
                HE6Checked = false;
                if (MarketComboSelectedValue == "PJM")
                {
                    HE7Checked = false;
                }
                else
                {
                    HE7Checked = true;
                }
                HE24Checked = false;
            }
            if (hourType == "ALL" || hourType == "PEAK")
            {
                HE8Checked = true;
                HE9Checked = true;
                HE10Checked = true;
                HE11Checked = true;
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
                if (MarketComboSelectedValue == "PJM" || hourType == "ALL")
                {
                    HE23Checked = true;
                }
                else
                {
                    HE23Checked = false;
                }
            }
            else
            {
                HE8Checked = false;
                HE9Checked = false;
                HE10Checked = false;
                HE11Checked = false;
                HE12Checked = false;
                HE13Checked = false;
                HE14Checked = false;
                HE15Checked = false;
                HE16Checked = false;
                HE17Checked = false;
                HE18Checked = false;
                HE19Checked = false;
                HE20Checked = false;
                HE21Checked = false;
                HE22Checked = false;
                if (MarketComboSelectedValue == "PJM")
                {
                    HE23Checked = false;
                }
                else
                {
                    HE23Checked = true;
                }
            }
        }

        /// <summary>
        /// Checks the check boxes.
        /// </summary>
        private void CheckCheckBoxes()
        {
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
            HE11Checked = true;
            HE12Checked = true;
            HE13Checked = true;
            HE14Checked = true;
            HE15Checked = true;
            HE16Checked = true;
            HE17Checked = true;
            HE18Checked = true;
            HE19Checked = true;
            HE24Checked = true;
            HE20Checked = true;
            HE21Checked = true;
            HE22Checked = true;
            HE23Checked = true;
        }

        /// <summary>
        /// Clears the check boxes.
        /// </summary>
        private void ClearCheckBoxes()
        {
            HE1Checked = false;
            HE2Checked = false;
            HE3Checked = false;
            HE4Checked = false;
            HE5Checked = false;
            HE6Checked = false;
            HE7Checked = false;
            HE8Checked = false;
            HE9Checked = false;
            HE10Checked = false;
            HE11Checked = false;
            HE12Checked = false;
            HE13Checked = false;
            HE14Checked = false;
            HE15Checked = false;
            HE16Checked = false;
            HE17Checked = false;
            HE18Checked = false;
            HE19Checked = false;
            HE24Checked = false;
            HE20Checked = false;
            HE21Checked = false;
            HE22Checked = false;
            HE23Checked = false;
            HourlyPivotList = new List<HourlyPivotData>();
            HourlyPivotListSummary = new List<HourlyPivotData>();
        }
        #endregion
        public Dictionary<string, double> ploadDictHash = new Dictionary<string, double>();
        public Dictionary<string, double> eloadDictHash = new Dictionary<string, double>();
        /// <summary>
        /// Initializes a new instance of the <see cref="LMPStatisticsViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            try
            {
                mDataService = dataService;
                mDataService.loadDBCommands();
                StartDate = DateTime.Now.Date.AddDays(-17);
                EndDate = DateTime.Now.Date;
                BStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 01);
                BEndDate = DateTime.Today.AddDays(0);
                CStartDate = DateTime.Now.Date.AddDays(-17);
                CEndDate = DateTime.Now.Date;
                SelectedSpreadType = SpreadType.DART;
                SelectedSortType = SortType.Date;
                SelectedPeriodType = PeriodType.Hourly;
                MarketComboSelectedValue = "ERCOT";
                FilterPriceComboList = new List<string>();
                FilterPriceComboList.Add("DA");
                FilterPriceComboList.Add("RT");
                FilterPriceComboList.Add("DART");
                FilterPriceComboSelectedValue = "DA";
                FilterDayComparisonSourceChecked = false;
                FilterDayComparisonSinkChecked = false;
                SpreadHighlightAbove = "2";
                SpreadHighlightBelow = "-2";
                FilterDayComparisonDAChecked = false;
                FilterDayComparisonRTChecked = false;
                FilterDayComparisonDARTChecked = true;
                FilterDayComparisonTotalChecked = true;
                FilterDayComparisonAvgChecked = true;
                FilterDayComparisonWinPctChecked = true;
                FilterDayComparisonRiskChecked = true;
                FilterDayComparisonMinChecked = true;
                FilterDayComparisonMaxChecked = true;
                FilterDayComparisonSharpeChecked = false;
                FilterDayComparisonRiskRewardChecked = false;
                FilterDayComparisonMedianChecked = false;
                FilterDayComparisonStdDevChecked = false;
                ClearedChecked = true;
                DeleteDateCommand = new DelegateCommand(DeleteDateAndUpdateCommand);
                OpenConstraints = new DelegateCommand(OpenConstraintsWindow);
                RunRetrieveFetchDataAndUpdateChartCommand = new DelegateCommand(RetrieveFetchDataAndUpdateChartCommand);
                AddSourceSinkCommand = new DelegateCommand(AddSourceSink);
                ResetCommand = new DelegateCommand(Reset);
                RemoveSourceSinkCommand = new DelegateCommand(RemoveSourceSink);
                RemoveAllSourceSinkCommand = new DelegateCommand(RemoveAllSourceSink);
                SwapCommand = new DelegateCommand(Swap);
                RemoveFilterCommand = new DelegateCommand(RemoveFilters);
                AddFilterCommand = new DelegateCommand(AddFilters);
                RemoveAllFilterCommand = new DelegateCommand(RemoveAllFilters);
                PasteCommand = new DelegateCommand(() => Paste());
                SimulateCommand = new DelegateCommand(() => Simulate());
                BollingerCommand = new DelegateCommand(() => Bollinger());
                CorrelationsCommand = new DelegateCommand(() => Correlations());
                LoadCommand = new DelegateCommand(() => RunLoadCommand());
                RunRefreshCommand = new DelegateCommand(() => RefreshTradedVolume());
                RunRefreshEPriceCommand = new DelegateCommand(() => RefreshEnergyPrice());
                ExportEnergyPricesCommand = new DelegateCommand(() => ExportEnergyPrices());
                ExportCommand = new DelegateCommand(() => Export());
                SpreadComboSelectedValue = null;
                SpreadComboSelectedValue = "Exclusive";
                mDataService.GetISOMarketList(
                        (item1, error) =>
                        {
                            ISOMarketList = item1;
                        });
                if (System.Diagnostics.Debugger.IsAttached) // for quicker testing in debug
                {
                    SourceComboSelectedItem = SourceNodeList.FirstOrDefault(x => x.NodeName == "WESTERN HUB");
                    SinkComboSelectedItem = SinkNodeList.FirstOrDefault(x => x.NodeName == "KAMMER");
                }
                List<string> productList = new List<string> { "Price", "Load", "Temp", "Cloud Cover", "Dew Point", "Precip", "Wind Speed", "Rel. Hum.", "Wind Dir." };
                ProductList = productList;
                ClearCheckBoxesCommand = new DelegateCommand(() => ClearCheckBoxes());
                CheckAllCheckBoxesCommand = new DelegateCommand(() => CheckCheckBoxes());
                CheckPeakCheckBoxesCommand = new DelegateCommand(() => CheckPeakCheckBoxes());
                CheckOffPeakCheckBoxesCommand = new DelegateCommand(() => CheckOffPeakCheckBoxes());
                SimMinText = "-50";
                SimMaxText = "50";
                //List<string> tList = new List<string>();
                //tList.Add("LMP");
                //tList.Add("Energy");
                //tList.Add("Congestion");
                //tList.Add("Loss");
                //PriceTypeList = tList;
                SelectedType = "LMP";
                ListDeenergizedNodes = mDataService.GetDeenergizedNodes();
                TrededSourceChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void RunLoadCommand()
        {
            if ((SourceSinkList == null || SourceSinkList.Count == 0))
            {
                MessageBox.Show("Please paste paths.", "Paste Paths");
            }
            else
            {
                foreach (SourceSinkData itemsourcesink in SourceSinkList)
                {
                    if (itemsourcesink.Source != null)
                    {
                        if (itemsourcesink.Sink != null)
                        {
                            SourceSinkData sourcesink = new SourceSinkData();
                            sourcesink.Source = itemsourcesink.Source;
                            sourcesink.Sink = itemsourcesink.Sink;

                            SourceSinkDataSelected = sourcesink;
                            UpdateChartCommand();
                        }
                        else
                        {
                            //MessageBox.Show(itemsourcesink.Source + "," + itemsourcesink.Sink + "this Path is Not Valid !!");
                        }
                    }
                    else
                    {
                        //MessageBox.Show(itemsourcesink.Source + "," + itemsourcesink.Sink + "this Path is Not Valid !!");
                    }
                }
            }
        }

        private void Correlations()
        {
            try
            {
                PlotDataModel = new PlotModel();
                SourceSinkData sourceSinkData = SourceSinkDataSelected;
                if (sourceSinkData == null)
                {
                    return;
                }
                sourceSinkData.Source = sourceSinkData.Source;
                sourceSinkData.Sink = !UptosChecked ? null : sourceSinkData.Sink;
                string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                            sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                    SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                }
                if (sourceSinkData.Sink != null)
                {
                    DateTime startdate = CStartDate;
                    DateTime enddate = CEndDate;
                    int n = (CEndDate - CStartDate).Days;
                    List<Node> rtList = new List<Node>();
                    List<Node> daList = new List<Node>();
                    List<PricingNode> sourceSinkNodeList = new List<PricingNode>();
                    foreach (SourceSinkData sourceSinkData1 in SourceSinkList)
                    {
                        if (!sourceSinkNodeList.Contains(sourceSinkData1.Source))
                        {
                            sourceSinkNodeList.Add(sourceSinkData1.Source);
                        }
                        if (sourceSinkData1.Sink != null && !sourceSinkNodeList.Contains(sourceSinkData1.Sink))
                        {
                            sourceSinkNodeList.Add(sourceSinkData1.Sink);
                        }
                    }
                    foreach (PricingNode priceNode in sourceSinkNodeList)
                    {
                        Node node = new Node();
                        node.Market = priceNode.MarketKey;
                        node.NodeId = priceNode.NodeKey;
                        node.NodeName = priceNode.NodeName;
                        node.PNodeId = priceNode.ExternalNodeId;
                        rtList.Add(node);
                        Node node1 = new Node();
                        node1.Market = node.Market;
                        node1.NodeId = node.NodeId;
                        node1.NodeName = node.NodeName;
                        node1.PNodeId = node.PNodeId;
                        daList.Add(node1);
                    }
                    List<Bollinger> listmBollingerfinal = new List<Bollinger>();
                    //while (startdate <= enddate)
                    //{
                    Bollinger mBollinger = new Bollinger();
                    DARTNode.GetDART(rtList, daList, startdate, n, false, true);
                    mRTHash = new Dictionary<int, Node>();
                    mDAHash = new Dictionary<int, Node>();
                    foreach (Node node in rtList)
                    {
                        if (!mRTHash.ContainsKey(node.NodeId))
                        {
                            mRTHash.Add(node.NodeId, node);
                        }
                    }
                    foreach (Node node in daList)
                    {
                        if (!mDAHash.ContainsKey(node.NodeId))
                        {
                            mDAHash.Add(node.NodeId, node);
                        }
                    }
                    Node daSourceNode = mDAHash[sourceSinkData.Source.NodeKey];
                    Node rtSourceNode = mRTHash[sourceSinkData.Source.NodeKey];
                    Node daSinkNode = sourceSinkData.Sink == null ? null : mDAHash[sourceSinkData.Sink.NodeKey];
                    Node rtSinkNode = sourceSinkData.Sink == null ? null : mRTHash[sourceSinkData.Sink.NodeKey];
                    LmpCorrelate danrtlist = new LmpCorrelate();

                    List<double> rt1 = new List<double>();
                    List<double> da1 = new List<double>();

                    List<double> rt2 = new List<double>();
                    List<double> da2 = new List<double>();

                    List<double> rt3 = new List<double>();
                    List<double> da3 = new List<double>();

                    List<double> rt4 = new List<double>();
                    List<double> da4 = new List<double>();

                    List<double> rt5 = new List<double>();
                    List<double> da5 = new List<double>();

                    List<double> rt6 = new List<double>();
                    List<double> da6 = new List<double>();

                    List<double> rt7 = new List<double>();
                    List<double> da7 = new List<double>();

                    List<double> rt8 = new List<double>();
                    List<double> da8 = new List<double>();

                    List<double> rt9 = new List<double>();
                    List<double> da9 = new List<double>();

                    List<double> rt10 = new List<double>();
                    List<double> da10 = new List<double>();

                    List<double> rt11 = new List<double>();
                    List<double> da11 = new List<double>();

                    List<double> rt12 = new List<double>();
                    List<double> da12 = new List<double>();

                    List<double> rt13 = new List<double>();
                    List<double> da13 = new List<double>();

                    List<double> rt14 = new List<double>();
                    List<double> da14 = new List<double>();

                    List<double> rt15 = new List<double>();
                    List<double> da15 = new List<double>();

                    List<double> rt16 = new List<double>();
                    List<double> da16 = new List<double>();

                    List<double> rt17 = new List<double>();
                    List<double> da17 = new List<double>();

                    List<double> rt18 = new List<double>();
                    List<double> da18 = new List<double>();

                    List<double> rt19 = new List<double>();
                    List<double> da19 = new List<double>();

                    List<double> rt20 = new List<double>();
                    List<double> da20 = new List<double>();

                    List<double> rt21 = new List<double>();
                    List<double> da21 = new List<double>();

                    List<double> rt22 = new List<double>();
                    List<double> da22 = new List<double>();

                    List<double> rt23 = new List<double>();
                    List<double> da23 = new List<double>();

                    List<double> rt24 = new List<double>();
                    List<double> da24 = new List<double>();
                    for (int j = 1; j <= n; j++)
                    {
                        if (j == 2)
                        {

                        }
                        for (int i = ((24 * j) - 24); i < 24 * j; i++)
                        {
                            if (daSourceNode.LmpTimePriceList[i].Lmp != null && rtSourceNode.LmpTimePriceList[i].Lmp != null)
                            {
                                if (daSinkNode == null)
                                {
                                    //rt.Add(j, rtSourceNode.LmpTimePriceList[i].Lmp.Price);
                                    //da.Add(j, daSourceNode.LmpTimePriceList[i].Lmp.Price);
                                }
                                else
                                {
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 1)
                                    {
                                        rt1.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da1.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 2)
                                    {
                                        rt2.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da2.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 3)
                                    {
                                        rt3.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da3.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 4)
                                    {
                                        rt4.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da4.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 5)
                                    {
                                        rt5.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da5.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 6)
                                    {
                                        rt6.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da6.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 7)
                                    {
                                        rt7.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da7.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 8)
                                    {
                                        rt8.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da8.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 9)
                                    {
                                        rt9.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da9.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 10)
                                    {
                                        rt10.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da10.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 11)
                                    {
                                        rt11.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da11.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 12)
                                    {
                                        rt12.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da12.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 13)
                                    {
                                        rt13.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da13.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 14)
                                    {
                                        rt14.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da14.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 15)
                                    {
                                        rt15.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da15.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 16)
                                    {
                                        rt16.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da16.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 17)
                                    {
                                        rt17.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da17.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 18)
                                    {
                                        rt18.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da18.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 19)
                                    {
                                        rt19.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da19.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 20)
                                    {
                                        rt20.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da20.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 21)
                                    {
                                        rt21.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da21.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 22)
                                    {
                                        rt22.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da22.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 23)
                                    {
                                        rt23.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da23.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                    if (rtSourceNode.LmpTimePriceList[i].MarketTime.Hour == 0)
                                    {
                                        rt24.Add(Math.Round((rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                        da24.Add(Math.Round((daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price), 2));
                                    }
                                }
                            }
                        }

                    }
                    DARTNode.sDALmpHash.Clear();
                    DARTNode.sRTLmpHash.Clear();
                    List<LmpCorrelate> mListLmpCorrelate = new List<LmpCorrelate>();
                    for (int i = 1; i <= 24; i++)
                    {
                        LmpCorrelate mLmpCorrelate = new LmpCorrelate();
                        mLmpCorrelate.Hour = i;
                        if (i == 1)
                        {
                            mLmpCorrelate.RTList = rt1;
                            mLmpCorrelate.DAList = da1;
                        }
                        if (i == 2)
                        {
                            mLmpCorrelate.RTList = rt2;
                            mLmpCorrelate.DAList = da2;
                        }
                        if (i == 3)
                        {
                            mLmpCorrelate.RTList = rt3;
                            mLmpCorrelate.DAList = da3;
                        }
                        if (i == 4)
                        {
                            mLmpCorrelate.RTList = rt4;
                            mLmpCorrelate.DAList = da4;
                        }
                        if (i == 5)
                        {
                            mLmpCorrelate.RTList = rt5;
                            mLmpCorrelate.DAList = da5;
                        }
                        if (i == 6)
                        {
                            mLmpCorrelate.RTList = rt6;
                            mLmpCorrelate.DAList = da6;
                        }
                        if (i == 7)
                        {
                            mLmpCorrelate.RTList = rt7;
                            mLmpCorrelate.DAList = da7;
                        }
                        if (i == 8)
                        {
                            mLmpCorrelate.RTList = rt8;
                            mLmpCorrelate.DAList = da8;
                        }
                        if (i == 9)
                        {
                            mLmpCorrelate.RTList = rt9;
                            mLmpCorrelate.DAList = da9;
                        }
                        if (i == 10)
                        {
                            mLmpCorrelate.RTList = rt10;
                            mLmpCorrelate.DAList = da10;
                        }
                        if (i == 11)
                        {
                            mLmpCorrelate.RTList = rt11;
                            mLmpCorrelate.DAList = da11;
                        }
                        if (i == 12)
                        {
                            mLmpCorrelate.RTList = rt12;
                            mLmpCorrelate.DAList = da12;
                        }
                        if (i == 13)
                        {
                            mLmpCorrelate.RTList = rt13;
                            mLmpCorrelate.DAList = da13;
                        }
                        if (i == 14)
                        {
                            mLmpCorrelate.RTList = rt14;
                            mLmpCorrelate.DAList = da14;
                        }
                        if (i == 15)
                        {
                            mLmpCorrelate.RTList = rt15;
                            mLmpCorrelate.DAList = da15;
                        }
                        if (i == 16)
                        {
                            mLmpCorrelate.RTList = rt16;
                            mLmpCorrelate.DAList = da16;
                        }
                        if (i == 17)
                        {
                            mLmpCorrelate.RTList = rt17;
                            mLmpCorrelate.DAList = da17;
                        }
                        if (i == 18)
                        {
                            mLmpCorrelate.RTList = rt18;
                            mLmpCorrelate.DAList = da18;
                        }
                        if (i == 19)
                        {
                            mLmpCorrelate.RTList = rt19;
                            mLmpCorrelate.DAList = da19;
                        }

                        if (i == 20)
                        {
                            mLmpCorrelate.RTList = rt20;
                            mLmpCorrelate.DAList = da20;
                        }
                        if (i == 21)
                        {
                            mLmpCorrelate.RTList = rt21;
                            mLmpCorrelate.DAList = da21;
                        }
                        if (i == 22)
                        {
                            mLmpCorrelate.RTList = rt22;
                            mLmpCorrelate.DAList = da22;
                        }
                        if (i == 23)
                        {
                            mLmpCorrelate.RTList = rt23;
                            mLmpCorrelate.DAList = da23;
                        }
                        if (i == 24)
                        {
                            mLmpCorrelate.RTList = rt24;
                            mLmpCorrelate.DAList = da24;
                        }
                        mListLmpCorrelate.Add(mLmpCorrelate);
                    }

                    List<double> CorList = new List<double>();
                    foreach (LmpCorrelate item in mListLmpCorrelate)
                    {
                        CorList.Add(ComputeCoeff(item.RTList.ToArray(), item.DAList.ToArray()));
                    }

                    LmpCorelation mLmpCorelation = new LmpCorelation();
                    mLmpCorelation.HE1 = CorList[0];
                    mLmpCorelation.HE2 = CorList[1];
                    mLmpCorelation.HE3 = CorList[2];
                    mLmpCorelation.HE4 = CorList[3];
                    mLmpCorelation.HE5 = CorList[4];
                    mLmpCorelation.HE6 = CorList[5];
                    mLmpCorelation.HE7 = CorList[6];
                    mLmpCorelation.HE8 = CorList[7];
                    mLmpCorelation.HE9 = CorList[8];
                    mLmpCorelation.HE10 = CorList[9];
                    mLmpCorelation.HE11 = CorList[10];
                    mLmpCorelation.HE12 = CorList[11];
                    mLmpCorelation.HE13 = CorList[12];
                    mLmpCorelation.HE14 = CorList[13];
                    mLmpCorelation.HE15 = CorList[14];
                    mLmpCorelation.HE16 = CorList[15];
                    mLmpCorelation.HE17 = CorList[16];
                    mLmpCorelation.HE18 = CorList[17];
                    mLmpCorelation.HE19 = CorList[18];
                    mLmpCorelation.HE20 = CorList[19];
                    mLmpCorelation.HE21 = CorList[20];
                    mLmpCorelation.HE22 = CorList[21];
                    mLmpCorelation.HE23 = CorList[22];
                    mLmpCorelation.HE24 = CorList[23];
                    List<LmpCorelation> mLmpCorelationList = new List<LmpCorelation>();
                    mLmpCorelationList.Add(mLmpCorelation);
                    DetailList = mLmpCorelationList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public double ComputeCoeff(double[] values1, double[] values2)
        {
            if (values1.Length != values2.Length)
                throw new ArgumentException("values must be the same length");

            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return result;
        }
        private void CorrelationsArray(LmpCorrelate item)
        {
            double[] array1 = item.RTList.ToArray();
            double[] array2 = item.DAList.ToArray();
            double[] array_xy = new double[array1.Length - 1];
            double[] array_xp2 = new double[array1.Length - 1];
            double[] array_yp2 = new double[array1.Length - 1];
            for (int i = 0; i <= array1.Length; i++)
                array_xy[i] = array1[i] * array2[i];
            for (int i = 0; i <= array1.Length; i++)
                array_xp2[i] = Math.Pow(array1[i], 2.0);
            for (int i = 0; i <= array1.Length; i++)
                array_yp2[i] = Math.Pow(array2[i], 2.0);
            double sum_x = 0;
            double sum_y = 0;
            foreach (double n in array1)
                sum_x += n;
            foreach (double n in array2)
                sum_y += n;
            double sum_xy = 0;
            foreach (double n in array_xy)
                sum_xy += n;
            double sum_xpow2 = 0;
            foreach (double n in array_xp2)
                sum_xpow2 += n;
            double sum_ypow2 = 0;
            foreach (double n in array_yp2)
                sum_ypow2 += n;
            double Ex2 = Math.Pow(sum_x, 2.00);
            double Ey2 = Math.Pow(sum_y, 2.00);

            double Correl =
            (array1.Length * sum_xy - sum_x * sum_y) /
            Math.Sqrt((array1.Length * sum_xpow2 - Ex2) * (array1.Length * sum_ypow2 - Ey2));
        }

        private void Bollinger()
        {
            // if (mLoadList != null && mLoadList.Count > 0 && PlotDataModel != null)
            {
                //lock (lockObj)
                {
                    //try
                    //{
                    FixData();
                    //}
                    //catch (Exception ex)
                    //{
                    //}
                }
            }
        }
        private double Median(List<double> source)
        {
            //List<double> temp = source.ToArray();
            //Array.Sort(temp);
            source.Sort();
            int count = source.Count;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                double a = source[count / 2 - 1];
                double b = source[count / 2];
                return ((a + b) / 2);
            }
            else
            {
                // count is odd, return the middle element
                return source[count / 2];
            }
        }
        private double Calculate(List<double> std)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in std)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 2));
        }
        private void FixData()
        {
            try
            {
                //FiftyChecked = false;
                PlotDataModel = new PlotModel();
                int n = 0;
                SourceSinkData sourceSinkData = SourceSinkDataSelected;
                if (sourceSinkData == null)
                {
                    return;
                }
                sourceSinkData.Source = sourceSinkData.Source;
                sourceSinkData.Sink = !UptosChecked ? null : sourceSinkData.Sink;
                string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                            sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                    SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                }
                if (sourceSinkData.Sink != null)
                {
                    listBollinger = null;
                    DataService ds = new DataService();
                    if (FiftyChecked == true)
                    {
                        n = 50;
                        // listBollinger = ds.GetBollingerData(BStartDate, BEndDate, sourceSinkData.Source.NodeKey, sourceSinkData.Sink.NodeKey, n);
                    }
                    if (Twenty6Checked == true)
                    {
                        n = 26;
                        // listBollinger = ds.GetBollingerData(BStartDate, BEndDate, sourceSinkData.Source.NodeKey, sourceSinkData.Sink.NodeKey, n);
                    }
                    if (TwelveChecked == true)
                    {
                        n = 12;
                        //listBollinger = ds.GetBollingerData(BStartDate, BEndDate, sourceSinkData.Source.NodeKey, sourceSinkData.Sink.NodeKey, n);
                    }
                    DateTime startdate = BStartDate.AddDays(-n);
                    DateTime enddate = BEndDate.AddDays(-n);
                    List<Node> rtList = new List<Node>();
                    List<Node> daList = new List<Node>();
                    List<PricingNode> sourceSinkNodeList = new List<PricingNode>();
                    foreach (SourceSinkData sourceSinkData1 in SourceSinkList)
                    {
                        if (!sourceSinkNodeList.Contains(sourceSinkData1.Source))
                        {
                            sourceSinkNodeList.Add(sourceSinkData1.Source);
                        }
                        if (sourceSinkData1.Sink != null && !sourceSinkNodeList.Contains(sourceSinkData1.Sink))
                        {
                            sourceSinkNodeList.Add(sourceSinkData1.Sink);
                        }
                    }
                    foreach (PricingNode priceNode in sourceSinkNodeList)
                    {
                        Node node = new Node();
                        node.Market = priceNode.MarketKey;
                        node.NodeId = priceNode.NodeKey;
                        node.NodeName = priceNode.NodeName;
                        node.PNodeId = priceNode.ExternalNodeId;
                        rtList.Add(node);
                        Node node1 = new Node();
                        node1.Market = node.Market;
                        node1.NodeId = node.NodeId;
                        node1.NodeName = node.NodeName;
                        node1.PNodeId = node.PNodeId;
                        daList.Add(node1);
                    }
                    List<Bollinger> listmBollingerfinal = new List<Bollinger>();
                    while (startdate <= enddate)
                    {
                        Bollinger mBollinger = new Bollinger();
                        DARTNode.GetDART(rtList, daList, startdate, n, false, true);
                        mRTHash = new Dictionary<int, Node>();
                        mDAHash = new Dictionary<int, Node>();
                        foreach (Node node in rtList)
                        {
                            if (!mRTHash.ContainsKey(node.NodeId))
                            {
                                mRTHash.Add(node.NodeId, node);
                            }
                        }
                        foreach (Node node in daList)
                        {
                            if (!mDAHash.ContainsKey(node.NodeId))
                            {
                                mDAHash.Add(node.NodeId, node);
                            }
                        }
                        Node daSourceNode = mDAHash[sourceSinkData.Source.NodeKey];
                        Node rtSourceNode = mRTHash[sourceSinkData.Source.NodeKey];
                        Node daSinkNode = sourceSinkData.Sink == null ? null : mDAHash[sourceSinkData.Sink.NodeKey];
                        Node rtSinkNode = sourceSinkData.Sink == null ? null : mRTHash[sourceSinkData.Sink.NodeKey];
                        List<double> dartlist = new List<double>();
                        for (int j = 1; j <= n; j++)
                        {
                            double price = 0.0;
                            for (int i = ((24 * j) - 24); i < 24 * j; i++)
                            {
                                if (daSourceNode.LmpTimePriceList[i].Lmp != null && rtSourceNode.LmpTimePriceList[i].Lmp != null)
                                {
                                    price = price + (daSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price :
                                   (rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price) -
                                     (daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price));
                                }
                            }
                            dartlist.Add(price);
                        }
                        //double med = Median(dartlist);
                        double avg = dartlist.Average();
                        double stddev = Calculate(dartlist);
                        mBollinger.UpperLimit = avg + (2 * stddev);
                        mBollinger.LowerLimit = avg - (2 * stddev);
                        mBollinger.StdDev = stddev;
                        mBollinger.MarketDateTime = startdate.AddDays(n);
                        DARTNode.sDALmpHash.Clear();
                        DARTNode.sRTLmpHash.Clear();
                        DARTNode.GetDART(rtList, daList, startdate.AddDays(n), 1, false, true);
                        double darttotal = 0.0;
                        for (int i = 0; i < 24; i++)
                        {
                            if (daSourceNode.LmpTimePriceList[i].Lmp != null && rtSourceNode.LmpTimePriceList[i].Lmp != null)
                            {
                                darttotal = darttotal + (daSinkNode == null ? rtSourceNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price :
                               (rtSinkNode.LmpTimePriceList[i].Lmp.Price - rtSourceNode.LmpTimePriceList[i].Lmp.Price) -
                                 (daSinkNode.LmpTimePriceList[i].Lmp.Price - daSourceNode.LmpTimePriceList[i].Lmp.Price));
                            }
                        }
                        mBollinger.DARTTotal = darttotal;
                        mBollinger.Average = dartlist.Average();
                        listmBollingerfinal.Add(mBollinger);
                        startdate = startdate.AddDays(1);
                    }
                    listBollinger = new Dictionary<string, List<Bollinger>>();
                    listBollinger.Add("n", listmBollingerfinal);
                    PlotModel tempModel = new PlotModel();
                    if (listBollinger == null || listBollinger.Count == 0)
                    {
                        return;
                    }
                    tempModel.Axes.Add(new LinearAxis
                    {
                        Key = "YAxis",
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                        MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                        IsPanEnabled = false,
                        IsZoomEnabled = true,
                        MaximumPadding = 0.5,
                        MinimumPadding = 0.1,
                        TextColor = OxyColors.White,
                        TitleColor = OxyColors.WhiteSmoke,
                        EndPosition = 1,
                        Position = AxisPosition.Left,
                        Title = "DARTTotal ----->",
                        AxisTitleDistance = 0
                    });
                    tempModel.Axes.Add(new DateTimeAxis
                    {
                        Title = "Market Date ---->",
                        Position = AxisPosition.Bottom,
                        TextColor = OxyColors.White,
                        TitleColor = OxyColors.WhiteSmoke,
                        AxisTitleDistance = 0,
                        //StringFormat = "HH:mm\n MMM-dd",
                        StringFormat = "MMM-dd",
                        MajorGridlineStyle = LineStyle.Solid,
                        AxislineThickness = 3,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                        MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                    });

                    FillColorListbands();
                    foreach (var item in listBollinger)
                    {
                        tempModel.Series.Add(CreateBSeries(item.Value, "LowerLimit"));
                        tempModel.Series.Add(CreateBSeries(item.Value, "STDDev"));
                        tempModel.Series.Add(CreateBSeries(item.Value, "UpperLimit"));
                        tempModel.Series.Add(CreateBSeries(item.Value, "DARTTotal"));
                        tempModel.Series.Add(CreateBSeries(item.Value, "Average"));
                    }
                    //tempModel.LegendOrientation = LegendOrientation.Horizontal;
                    //tempModel.LegendPlacement = LegendPlacement.Outside;
                    //tempModel.LegendPosition = LegendPosition.RightTop;
                    //tempModel.LegendTextColor = OxyColors.White;
                    tempModel.TitlePadding = 3;
                    PlotDataModel = tempModel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshEnergyPrice()
        {
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> RTEnergyPriceDic;
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> DAEnergyPriceDic;
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> FinalEnergyPriceDic;
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> DA_DaywiseEnergyPrice;
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> RT_DaywiseEnergyPrice;//dd
            Dictionary<double, Dictionary<int, double>> DA_HourwiseEnergyPrice;
            Dictionary<double, Dictionary<int, double>> RT_HourwiseEnergyPrice;
            Dictionary<string, Dictionary<int, double>> LMPPriceDic = new Dictionary<string, Dictionary<int, double>>();
            Dictionary<string, Dictionary<int, double>> LMPPriceDicSource = new Dictionary<string, Dictionary<int, double>>();
            Dictionary<string, Dictionary<int, double>> LMPPriceDicSink = new Dictionary<string, Dictionary<int, double>>();

            Dictionary<string, Dictionary<int, double>> DALMPPriceDicSource = new Dictionary<string, Dictionary<int, double>>();
            Dictionary<string, Dictionary<int, double>> DALMPPriceDicSink = new Dictionary<string, Dictionary<int, double>>();

            Dictionary<string, Dictionary<int, double>> RTLMPPriceDicSource = new Dictionary<string, Dictionary<int, double>>();
            Dictionary<string, Dictionary<int, double>> RTLMPPriceDicSink = new Dictionary<string, Dictionary<int, double>>();

            Dictionary<string, Dictionary<int, double>> DARTLMPPriceDicSource = new Dictionary<string, Dictionary<int, double>>();
            Dictionary<string, Dictionary<int, double>> DARTLMPPriceDicSink = new Dictionary<string, Dictionary<int, double>>();

            Dictionary<string, double> GraphEPdataSource = new Dictionary<string, double>();
            Dictionary<string, double> GraphEPdataSink = new Dictionary<string, double>();
            string SourceName;
            string SinkName;

            RTEnergyPriceDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            DAEnergyPriceDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();
            FinalEnergyPriceDic = new Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>>();

            List<Node> mDaFilteredSortedListEP = new List<Node>();
            LmpTimePrice LmpTimePriceHelper = new LmpTimePrice();
            List<LmpTimePrice> LmpTimePriceListSource = new List<LmpTimePrice>();
            List<LmpTimePrice> LmpTimePriceListSink = new List<LmpTimePrice>();
            List<LmpTimePrice> DARTLMPHelper = new List<LmpTimePrice>();




            try
            {
                SourceTradedVolumesList = null;
                if (MarketComboSelectedValue != "ERCOT")
                {
                    System.Windows.MessageBox.Show("Please Select Ercot Market !!!");
                    return;
                }
                System.TimeSpan datediff = TdStartDate.Subtract(TdEndDate);
                int totalday = Convert.ToInt32(datediff.TotalDays);
                if (totalday <= -32)
                {
                    System.Windows.MessageBox.Show("Select Date Differnce should be one Months !!!");
                    return;
                }
                if (TdStartDate > TdEndDate)
                {
                    System.Windows.MessageBox.Show("Start Date should not be greater than End Date");
                    return;
                }
                if (SourceSinkDataSelected.Source == null & SourceSinkDataSelected.Sink == null)
                {
                    System.Windows.MessageBox.Show("Please select Node Name");
                    return;
                }
                string Source = SourceSinkDataSelected.Source.ToString();
                string Sink = SourceSinkDataSelected.Sink.ToString();

            }
            catch
            {

            }


            try
            {

                ConnectEPService();
                SourceName = SourceSinkDataSelected.Source.ToString();
                SinkName = SourceSinkDataSelected.Sink.ToString();

                LMPPriceDicSource = mDataService.Get15MinLMP(SourceName, EPriceStartDate, EPriceEndDate, SelectedSpreadType.ToString());
                LMPPriceDicSink = mDataService.Get15MinLMP(SinkName, EPriceStartDate, EPriceEndDate, SelectedSpreadType.ToString());

                /*  DALMPPriceDicSource = mDataService.Get15MinLMP(SourceName, EPriceStartDate, EPriceEndDate, "DA");
                   DALMPPriceDicSink = mDataService.Get15MinLMP(SinkName, EPriceStartDate, EPriceEndDate, "DA");

                   RTLMPPriceDicSource = mDataService.Get15MinLMP(SourceName, EPriceStartDate, EPriceEndDate, "RT");
                   RTLMPPriceDicSink = mDataService.Get15MinLMP(SinkName, EPriceStartDate, EPriceEndDate, "RT");
                  */
                // if (SelectedSpreadType.ToString()=="DART")
                {
                    DALMPPriceDicSource = mDataService.Get15MinLMP(SourceName, EPriceStartDate, EPriceEndDate, "DA");
                    DALMPPriceDicSink = mDataService.Get15MinLMP(SinkName, EPriceStartDate, EPriceEndDate, "DA");

                    RTLMPPriceDicSource = mDataService.Get15MinLMP(SourceName, EPriceStartDate, EPriceEndDate, "RT");
                    RTLMPPriceDicSink = mDataService.Get15MinLMP(SinkName, EPriceStartDate, EPriceEndDate, "RT");

                    DARTLMPPriceDicSource = mDataService.Get15MinLMP(SourceName, EPriceStartDate, EPriceEndDate, "DART");
                    DARTLMPPriceDicSink = mDataService.Get15MinLMP(SinkName, EPriceStartDate, EPriceEndDate, "DART");


                }
                if (SelectedSpreadType.ToString() == "RT")
                {
                    RTEnergyPriceDic = mLMPEnergyPriceProxy.RTEnergyPrices(EPriceStartDate, EPriceEndDate);
                    FinalEnergyPriceDic = RTEnergyPriceDic;

                }

                if (SelectedSpreadType.ToString() == "DA")
                {
                    DAEnergyPriceDic = mLMPEnergyPriceProxy.DAEnergyPrices(EPriceStartDate, EPriceEndDate);
                    FinalEnergyPriceDic = DAEnergyPriceDic;
                }

                if (SelectedSpreadType.ToString() == "DART")
                {
                    FinalEnergyPriceDic = mLMPEnergyPriceProxy.DARTEnergyPrices(EPriceStartDate, EPriceEndDate);
                    RTEnergyPriceDic = mLMPEnergyPriceProxy.RTEnergyPrices(EPriceStartDate, EPriceEndDate);
                    DAEnergyPriceDic = mLMPEnergyPriceProxy.DAEnergyPrices(EPriceStartDate, EPriceEndDate);

                }
            }
            catch (FaultException fe)
            {
                MessageBox.Show(fe.Message);
            }


            TradedVolumesData objEnergyPriceData = new TradedVolumesData();
            List<TradedVolumesData> lstEnergyPriceData = new List<TradedVolumesData>();


            string dicdatekey;
            int dichourkey;
            double EPValue = 0, LMPValueSource = 0, LMPValueSink = 0;
            Dictionary<int, Dictionary<double, Dictionary<int, double>>> FinalDaywiseEnergyPrice;                                //dd
            Dictionary<double, Dictionary<int, double>> FinalHourwiseEnergyPrice;

            SourceName = SourceSinkDataSelected.Source.ToString();
            SinkName = SourceSinkDataSelected.Sink.ToString();

            int SourceKey = mDataService.GetNodeKey(SourceName);
            int SinkKey = mDataService.GetNodeKey(SinkName);
            List<HourlyPivotDataCongestion> hourlyPivotDataSource = new List<HourlyPivotDataCongestion>();
            List<HourlyPivotDataCongestion> hourlyPivotDataSink = new List<HourlyPivotDataCongestion>();





            List<HourlyPivotDataCongestion> RTCongestionListSourceValue = CalculateCongestion(RTLMPPriceDicSource, RTEnergyPriceDic);
            List<HourlyPivotDataCongestion> RTCongestionListSinkValue = CalculateCongestion(RTLMPPriceDicSink, RTEnergyPriceDic);


            List<HourlyPivotDataCongestion> DACongestionListSourceValue = CalculateCongestion(DALMPPriceDicSource, DAEnergyPriceDic);
            List<HourlyPivotDataCongestion> DACongestionListSinkValue = CalculateCongestion(DALMPPriceDicSink, DAEnergyPriceDic);





            //Source congetion = Source DA Congetion- Source RT congetion
            if (SelectedSpreadType.ToString() == "RT")
            {
                hourlyPivotDataSource = RTCongestionListSourceValue;
                hourlyPivotDataSink = RTCongestionListSinkValue;
                LmpTimePriceListSource = CalculateLMPHelper(RTLMPPriceDicSource, RTEnergyPriceDic);
                LmpTimePriceListSink = CalculateLMPHelper(RTLMPPriceDicSink, RTEnergyPriceDic);

                GraphEPdataSource = CalculateGraphData(RTLMPPriceDicSource, RTEnergyPriceDic);
                GraphEPdataSink = CalculateGraphData(RTLMPPriceDicSink, RTEnergyPriceDic);
            }
            if (SelectedSpreadType.ToString() == "DA")
            {
                hourlyPivotDataSource = DACongestionListSourceValue;
                hourlyPivotDataSink = DACongestionListSinkValue;
                LmpTimePriceListSource = CalculateLMPHelper(DALMPPriceDicSource, DAEnergyPriceDic);
                LmpTimePriceListSink = CalculateLMPHelper(DALMPPriceDicSink, DAEnergyPriceDic);
                GraphEPdataSource = CalculateGraphData(DALMPPriceDicSource, DAEnergyPriceDic);
                GraphEPdataSink = CalculateGraphData(DALMPPriceDicSink, DAEnergyPriceDic);

            }

            if (SelectedSpreadType.ToString() == "DART")
            {
                // change
                List<HourlyPivotDataCongestion> DARTCongestionListSourceValue = CalculateDARTCongestion(RTCongestionListSourceValue, DACongestionListSourceValue);
                List<HourlyPivotDataCongestion> DARTCongestionListSinkValue = CalculateDARTCongestion(RTCongestionListSinkValue, DACongestionListSinkValue);

                hourlyPivotDataSource = DARTCongestionListSourceValue;
                hourlyPivotDataSink = DARTCongestionListSinkValue;
                // LmpTimePriceListSource = CalculateLMPHelper(RTCongestionListSourceValue, DACongestionListSourceValue); //Source congetion = Source DA Congetion- Source RT congetion
                LmpTimePriceListSource = CalculateLMPHelper(DARTLMPPriceDicSource, FinalEnergyPriceDic);
                LmpTimePriceListSink = CalculateLMPHelper(DARTLMPPriceDicSink, FinalEnergyPriceDic);
                GraphEPdataSource = CalculateGraphData(DARTLMPPriceDicSource, FinalEnergyPriceDic);
                GraphEPdataSink = CalculateGraphData(DARTLMPPriceDicSink, FinalEnergyPriceDic);

            }


            /* GraphEPdataSource.Clear();
             GraphEPdataSink.Clear();
           if (FinalEnergyPriceDic.Count > 0)
             {
                 DateTime dt = EPriceStartDate;

                 while (dt.Date <= EPriceEndDate.Date)
                 {
                     HourlyPivotDataCongestion hpdataSource = new HourlyPivotDataCongestion();
                     HourlyPivotDataCongestion hpdataSink = new HourlyPivotDataCongestion();

                     // for (int i = 0, m = 0; i < 24; i++, m++)
                     for (int i = 0, m = 0; i < 24; i++, m++)
                     {
                         EPValue = 0;
                         dichourkey = m;
                         dicdatekey = dt.ToString("yyyy-MM-dd");
                         string lmpdicdatekey = dt.ToString("M/d/yyyy");

                         if (FinalEnergyPriceDic.ContainsKey(dicdatekey))
                         {

                             if (LMPPriceDicSource.ContainsKey(lmpdicdatekey))
                             {
                                 var TempLMPPricedic = LMPPriceDicSource[lmpdicdatekey];
                                 if (TempLMPPricedic.ContainsKey(dichourkey))
                                 {
                                     // foreach (var dictempItem in TempLMPPricedic)
                                     LMPValueSource = TempLMPPricedic[dichourkey];// dictempItem.Value;
                                 }

                             }

                             if (LMPPriceDicSink.ContainsKey(lmpdicdatekey))
                             {
                                 var TempLMPPricedic = LMPPriceDicSink[lmpdicdatekey];

                                 if (TempLMPPricedic.ContainsKey(dichourkey))
                                     LMPValueSink = TempLMPPricedic[dichourkey];// dictempItem.Value;

                             }
                             objEnergyPriceData = new TradedVolumesData();
                             FinalDaywiseEnergyPrice = FinalEnergyPriceDic[dicdatekey];//2022-01-10  
                             if (FinalDaywiseEnergyPrice.ContainsKey(dichourkey))//dd
                             {
                                 FinalHourwiseEnergyPrice = FinalDaywiseEnergyPrice[dichourkey];
                                 foreach (var dicItem in FinalHourwiseEnergyPrice)
                                     EPValue = dicItem.Key;
                             }

                             dt = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0);
                             dt.AddMinutes(-dt.Minute);
                             dt.AddHours(m);
                             objEnergyPriceData.MarketDateTime = dt;
                             objEnergyPriceData.Hour = i.ToString();
                             objEnergyPriceData.SourceVolume = LMPValueSource - EPValue;//EPValue;
                                                                                        // objEnergyPriceData.SourceVolume =  EPValue;//EPValue;
                             objEnergyPriceData.NodeKey = SourceKey;


                             hpdataSource.Date = dt;
                             hpdataSource.DateDisplay = dt;
                             hpdataSource.RowDay = dt.ToString("ddd");
                             hpdataSource.RowType = SelectedSpreadType.ToString();
                             hpdataSource.RowDisplayType = SelectedSpreadType.ToString();
                             string hour = "HE" + dichourkey;
                             double? price = null;
                             if (hour.Equals("HE0"))
                             {
                                 hour = "HE24";
                             }
                             hour = hour + "_Source_Cong";
                             price = LMPValueSource - EPValue;
                             hpdataSource.GetType().GetProperty(hour).SetValue(hpdataSource, price, null);

                             Node n = new Node();

                             LMP SourceLMPObj = new LMP();
                              LmpTimePriceHelper = new LmpTimePrice();
                             SourceLMPObj.Price = LMPValueSource;
                             SourceLMPObj.Energyprice = EPValue;
                             SourceLMPObj.Congestion= LMPValueSource - EPValue;
                             LmpTimePriceHelper.MarketTime = dt;
                             LmpTimePriceHelper.Lmp= SourceLMPObj;

                             LmpTimePriceListSource.Add(LmpTimePriceHelper);

                             hpdataSink.MarketDate = dt;
                             hpdataSink.Date = dt;
                             hpdataSink.DateDisplay = dt;
                             hpdataSink.RowDay = dt.ToString("ddd");
                             hpdataSink.RowType = SelectedSpreadType.ToString();
                             hpdataSink.RowDisplayType = SelectedSpreadType.ToString();
                             hour = "HE" + dichourkey;
                             price = null;
                             if (hour.Equals("HE0"))
                             {
                                 hour = "HE24";
                             }
                             hour = hour + "_Source_Cong";
                             price = LMPValueSink - EPValue; //HE24_Sink_Cong
                             hpdataSink.GetType().GetProperty(hour).SetValue(hpdataSink, price, null);

                             LmpTimePriceHelper = new LmpTimePrice();
                             LMP SinkLMPObj = new LMP();
                             SinkLMPObj.Price = LMPValueSink;
                             SinkLMPObj.Energyprice = EPValue;
                             SinkLMPObj.Congestion = LMPValueSink - EPValue;
                             LmpTimePriceHelper.MarketTime = dt;
                             LmpTimePriceHelper.Lmp = SinkLMPObj;
                             LmpTimePriceListSink.Add(LmpTimePriceHelper);


                             string key = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0).ToString("M/d/yy");
                             key = key + " H" + (i + 1).ToString();


                             double SourceValue = Math.Round(LMPValueSource - EPValue, 2);
                             double SinkValue = Math.Round(LMPValueSink - EPValue, 2);

                             GraphEPdataSource.Add(key, SourceValue);
                             GraphEPdataSink.Add(key, SinkValue);

                             lstEnergyPriceData.Add(objEnergyPriceData);

                         }

                     }
                     hourlyPivotDataSource.Add(hpdataSource);
                     hourlyPivotDataSink.Add(hpdataSink);
                     dt = dt.AddDays(1);
                 }
             }

        */




            if (hourlyPivotDataSource.Where(a => a.RowType == null).Count() <= 0)
            {

                List<HourlyPivotDataCongestion> sortTempHourlyPivotList = (from t in hourlyPivotDataSource
                                                                           orderby t.Date descending, t.RowType.Length descending, t.RowType ascending, t.RowName descending
                                                                           select t).ToList();

                HourlyPivotListCongestionSource = sortTempHourlyPivotList;
            }



            if (hourlyPivotDataSink.Where(a => a.RowType == null).Count() <= 0)
            {

                List<HourlyPivotDataCongestion> sortTempHourlyPivotListSink = (from t in hourlyPivotDataSink
                                                                               orderby t.Date descending, t.RowType.Length descending, t.RowType ascending, t.RowName descending
                                                                               select t).ToList();

                HourlyPivotListCongestionSink = sortTempHourlyPivotListSink;
            }




            List<Dictionary<string, double>> nodelistall = new List<Dictionary<string, double>>();
            nodelistall.Add(GraphEPdataSource);
            nodelistall.Add(GraphEPdataSink);


            //  if (GraphEPdataSource.Count > 0)
            //  PlotModelEnergyPrice = CreatePlotModelUpperEP(nodelistall);

            // if (GraphEPdataSink.Count > 0)
            //  PlotModelEnergyPrice = CreatePlotModelEnergyPrice(GraphEPdataSink, "DA");

            Node SourceNode = new Node();
            SourceNode.LmpTimePriceList = LmpTimePriceListSource;
            SourceNode.Market = 9;
            SourceNode.NodeId = SourceKey;
            SourceNode.NodeName = SourceName;



            Node SinkNode = new Node();
            SinkNode.LmpTimePriceList = LmpTimePriceListSink;
            SinkNode.Market = 9;
            SinkNode.NodeId = SinkKey;
            SinkNode.NodeName = SinkName;

            List<Node> NodeList = new List<Node>();
            NodeList.Add(SourceNode);
            NodeList.Add(SinkNode);


            Node SinkNode1 = new Node();


            DARTLMPHelper = CalculateDARTLMPHelper(LmpTimePriceListSource, LmpTimePriceListSink);

            // --SinkNode1.LmpTimePriceList = LmpTimePriceListSink;
            SinkNode1.LmpTimePriceList = DARTLMPHelper;

            SinkNode1.Market = 9;
            SinkNode1.NodeId = SinkKey;
            SinkNode1.NodeName = SinkName;

            NodeList.Add(SinkNode1);




            List<Node> hourlyNodeList = new List<Node>();

            hourlyNodeList = NodeList;



            List<Node> plotList = new List<Node>();

            if (SelectedPeriodType == PeriodType.Daily)
            {
                if (SelectedSortType == SortType.DART)
                    plotList = ConvertDailyValues(hourlyNodeList);
                if (selectedSortType == SortType.DA)
                    plotList = ConvertDailyValues(NodeList);
                if (selectedSortType == SortType.RT)
                    plotList = ConvertDailyValues(NodeList);

                if (selectedSortType == SortType.Date)
                {
                    if (SelectedSpreadType == SpreadType.DA)
                        plotList = ConvertDailyValues(NodeList);
                    else
                        plotList = ConvertDailyValues(NodeList);
                    //plotList = ConvertDailyValues(mRTFilteredSortedList);
                }
                //  plotList = mDartFilteredSortedList;
            }
            else
            {
                plotList = hourlyNodeList;
            }

            PlotModelEnergyPrice = CreatePlotModelUpper(plotList);

        }

        private List<HourlyPivotDataCongestion> CalculateCongestion(Dictionary<string, Dictionary<int, double>> LMPValuesDic, Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> EnergyPriceValues)
        {
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> FinalEnergyPriceDic;

            FinalEnergyPriceDic = EnergyPriceValues;

            string dicdatekey;
            int dichourkey;
            double EPValue = 0, LMPValueSource = 0, LMPValueSink = 0;

            Dictionary<int, Dictionary<double, Dictionary<int, double>>> FinalDaywiseEnergyPrice;                                //dd
            Dictionary<double, Dictionary<int, double>> FinalHourwiseEnergyPrice;
            List<LmpTimePrice> LmpTimePriceListSource = new List<LmpTimePrice>();
            Dictionary<string, double> GraphEPdataSource = new Dictionary<string, double>();
            Dictionary<string, double> GraphEPdataSink = new Dictionary<string, double>();
            List<HourlyPivotDataCongestion> hourlyPivotDataSource = new List<HourlyPivotDataCongestion>();


            int SourceKey = mDataService.GetNodeKey(SourceSinkDataSelected.Source.ToString());
            int SinkKey = mDataService.GetNodeKey(SourceSinkDataSelected.Sink.ToString());

            if (FinalEnergyPriceDic.Count > 0)
            {
                DateTime dt = EPriceStartDate;

                while (dt.Date <= EPriceEndDate.Date)
                {
                    HourlyPivotDataCongestion hpdataSource = new HourlyPivotDataCongestion();
                    HourlyPivotDataCongestion hpdataSink = new HourlyPivotDataCongestion();

                    // for (int i = 0, m = 0; i < 24; i++, m++)
                    for (int i = 0, m = 0; i < 24; i++, m++)
                    {
                        EPValue = 0;
                        dichourkey = m;
                        dicdatekey = dt.ToString("yyyy-MM-dd");
                        string lmpdicdatekey = dt.ToString("M/d/yyyy");

                        if (FinalEnergyPriceDic.ContainsKey(dicdatekey))
                        {

                            if (LMPValuesDic.ContainsKey(lmpdicdatekey))
                            {
                                var TempLMPPricedic = LMPValuesDic[lmpdicdatekey];
                                if (TempLMPPricedic.ContainsKey(dichourkey))
                                {
                                    // foreach (var dictempItem in TempLMPPricedic)
                                    LMPValueSource = TempLMPPricedic[dichourkey];// dictempItem.Value;
                                }

                            }
                            FinalDaywiseEnergyPrice = FinalEnergyPriceDic[dicdatekey];//2022-01-10  

                            if (FinalDaywiseEnergyPrice.ContainsKey(dichourkey))//dd
                            {
                                FinalHourwiseEnergyPrice = FinalDaywiseEnergyPrice[dichourkey];
                                foreach (var dicItem in FinalHourwiseEnergyPrice)
                                    EPValue = dicItem.Key;
                            }

                            dt = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0);
                            dt.AddMinutes(-dt.Minute);
                            dt.AddHours(m);

                            hpdataSource.Date = dt;
                            hpdataSource.DateDisplay = dt;
                            hpdataSource.RowDay = dt.ToString("ddd");
                            hpdataSource.RowType = SelectedSpreadType.ToString();
                            hpdataSource.RowDisplayType = SelectedSpreadType.ToString();
                            string hour = "HE" + dichourkey;
                            double? price = null;
                            if (hour.Equals("HE0"))
                            {
                                hour = "HE24";
                            }
                            hour = hour + "_Source_Cong";
                            price = LMPValueSource - EPValue;
                            hpdataSource.GetType().GetProperty(hour).SetValue(hpdataSource, price, null);

                            Node n = new Node();

                            LMP SourceLMPObj = new LMP();
                            LmpTimePrice LmpTimePriceHelper = new LmpTimePrice();
                            SourceLMPObj.Price = LMPValueSource;
                            SourceLMPObj.EnergyPrice = EPValue;
                            SourceLMPObj.Congestion = LMPValueSource - EPValue;
                            LmpTimePriceHelper.MarketTime = dt;
                            LmpTimePriceHelper.Lmp = SourceLMPObj;

                            LmpTimePriceListSource.Add(LmpTimePriceHelper);





                            string key = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0).ToString("M/d/yy");
                            key = key + " H" + (i + 1).ToString();


                            double SourceValue = Math.Round(LMPValueSource - EPValue, 2);
                            double SinkValue = Math.Round(LMPValueSink - EPValue, 2);

                            GraphEPdataSource.Add(key, SourceValue);
                            GraphEPdataSink.Add(key, SinkValue);

                            // lstEnergyPriceData.Add(objEnergyPriceData);

                        }

                    }
                    hourlyPivotDataSource.Add(hpdataSource);

                    dt = dt.AddDays(1);
                }
            }

            return hourlyPivotDataSource;

        }

        private List<HourlyPivotDataCongestion> CalculateDARTCongestion(List<HourlyPivotDataCongestion> RTValue, List<HourlyPivotDataCongestion> DAValue)
        {
            List<HourlyPivotDataCongestion> hourlyPivotDataSource = new List<HourlyPivotDataCongestion>();
            List<double?> RTData = new List<double?>();
            List<double?> DAData = new List<double?>();
            if (RTValue.Count > 0)
            {
                int ListCount = RTValue.Count;
                string dicdatekey;
                int dichourkey;
                double DAValues = 0, LMPValueSource = 0, LMPValueSink = 0;
                DateTime dt = EPriceStartDate;
                // while (dt.Date <= EPriceEndDate.Date)
                {
                    for (int count = 0; count < ListCount; count++)
                    {
                        HourlyPivotDataCongestion hpdataSource = new HourlyPivotDataCongestion();
                        RTData = RTValue[count].HourDataList;
                        DAData = DAValue[count].HourDataList;
                        for (int i = 0, m = 0; i < 24; i++, m++)
                        {
                            DAValues = 0;
                            dichourkey = m;
                            dicdatekey = dt.ToString("yyyy-MM-dd");
                            string lmpdicdatekey = dt.ToString("M/d/yyyy");

                            hpdataSource.Date = dt;
                            hpdataSource.DateDisplay = dt;
                            hpdataSource.RowDay = dt.ToString("ddd");
                            hpdataSource.RowType = SelectedSpreadType.ToString();
                            hpdataSource.RowDisplayType = SelectedSpreadType.ToString();
                            string hour = "HE" + (dichourkey + 1);
                            double? price = null;
                            //if (hour.Equals("HE0"))
                            //{
                            //    hour = "HE24";
                            //}
                            hour = hour + "_Source_Cong";
                            price = RTData[m] - DAData[m];
                            hpdataSource.GetType().GetProperty(hour).SetValue(hpdataSource, price, null);


                        }
                        hourlyPivotDataSource.Add(hpdataSource);
                        dt = dt.AddDays(1);

                    }


                }
            }

            return hourlyPivotDataSource;

        }

        private List<LmpTimePrice> CalculateLMPHelper(Dictionary<string, Dictionary<int, double>> LMPValuesDic, Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> EnergyPriceValues)
        {
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> FinalEnergyPriceDic;

            FinalEnergyPriceDic = EnergyPriceValues;

            string dicdatekey;
            int dichourkey;
            double EPValue = 0, LMPValueSource = 0;

            Dictionary<int, Dictionary<double, Dictionary<int, double>>> FinalDaywiseEnergyPrice;                                //dd
            Dictionary<double, Dictionary<int, double>> FinalHourwiseEnergyPrice;
            List<LmpTimePrice> LmpTimePriceList = new List<LmpTimePrice>();
            if (FinalEnergyPriceDic.Count > 0)
            {
                DateTime dt = EPriceStartDate;

                while (dt.Date <= EPriceEndDate.Date)
                {
                    HourlyPivotDataCongestion hpdataSource = new HourlyPivotDataCongestion();
                    HourlyPivotDataCongestion hpdataSink = new HourlyPivotDataCongestion();


                    for (int i = 0, m = 0; i < 24; i++, m++)
                    {
                        EPValue = 0;
                        dichourkey = m;
                        dicdatekey = dt.ToString("yyyy-MM-dd");
                        string lmpdicdatekey = dt.ToString("M/d/yyyy");

                        if (FinalEnergyPriceDic.ContainsKey(dicdatekey))
                        {

                            if (LMPValuesDic.ContainsKey(lmpdicdatekey))
                            {
                                var TempLMPPricedic = LMPValuesDic[lmpdicdatekey];
                                if (TempLMPPricedic.ContainsKey(dichourkey))
                                    LMPValueSource = TempLMPPricedic[dichourkey];// dictempItem.Value;

                            }


                            FinalDaywiseEnergyPrice = FinalEnergyPriceDic[dicdatekey];//2022-01-10  

                            if (FinalDaywiseEnergyPrice.ContainsKey(dichourkey))//dd
                            {
                                FinalHourwiseEnergyPrice = FinalDaywiseEnergyPrice[dichourkey];
                                foreach (var dicItem in FinalHourwiseEnergyPrice)
                                    EPValue = dicItem.Key;
                            }

                            dt = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0);
                            dt.AddMinutes(-dt.Minute);
                            dt.AddHours(m);

                            Node n = new Node();

                            LMP LMPObj = new LMP();
                            LmpTimePrice LmpTimePriceHelper = new LmpTimePrice();
                            LMPObj.Price = LMPValueSource;
                            LMPObj.EnergyPrice = EPValue;
                            LMPObj.Congestion = LMPValueSource - EPValue;
                            LmpTimePriceHelper.MarketTime = dt;
                            LmpTimePriceHelper.Lmp = LMPObj;
                            LmpTimePriceList.Add(LmpTimePriceHelper);

                        }

                    }

                    dt = dt.AddDays(1);
                }
            }

            return LmpTimePriceList;
        }
        private Dictionary<string, double> CalculateGraphData(Dictionary<string, Dictionary<int, double>> LMPValuesDic, Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> EnergyPriceValues)
        {
            Dictionary<string, Dictionary<int, Dictionary<double, Dictionary<int, double>>>> FinalEnergyPriceDic;

            FinalEnergyPriceDic = EnergyPriceValues;
            string dicdatekey, key, lmpdicdatekey;
            int dichourkey;
            double LMPValueSource = 0, EPValue = 0, CongestionValue;

            Dictionary<int, Dictionary<double, Dictionary<int, double>>> FinalDaywiseEnergyPrice;                                //dd
            Dictionary<double, Dictionary<int, double>> FinalHourwiseEnergyPrice;
            Dictionary<string, double> GraphEPdata = new Dictionary<string, double>();
            int SourceKey = mDataService.GetNodeKey(SourceSinkDataSelected.Source.ToString());
            int SinkKey = mDataService.GetNodeKey(SourceSinkDataSelected.Sink.ToString());

            if (FinalEnergyPriceDic.Count > 0)
            {
                DateTime dt = EPriceStartDate;

                while (dt.Date <= EPriceEndDate.Date)
                {

                    for (int i = 0, m = 0; i < 24; i++, m++)
                    {
                        EPValue = 0;
                        dichourkey = m;
                        dicdatekey = dt.ToString("yyyy-MM-dd");
                        lmpdicdatekey = dt.ToString("M/d/yyyy");

                        if (FinalEnergyPriceDic.ContainsKey(dicdatekey))
                        {

                            if (LMPValuesDic.ContainsKey(lmpdicdatekey))
                            {
                                var TempLMPPricedic = LMPValuesDic[lmpdicdatekey];
                                if (TempLMPPricedic.ContainsKey(dichourkey))
                                    LMPValueSource = TempLMPPricedic[dichourkey];// dictempItem.Value;

                            }

                            FinalDaywiseEnergyPrice = FinalEnergyPriceDic[dicdatekey];//2022-01-10  

                            if (FinalDaywiseEnergyPrice.ContainsKey(dichourkey))//dd
                            {
                                FinalHourwiseEnergyPrice = FinalDaywiseEnergyPrice[dichourkey];
                                foreach (var dicItem in FinalHourwiseEnergyPrice)
                                    EPValue = dicItem.Key;
                            }

                            dt = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0);
                            dt.AddMinutes(-dt.Minute);
                            dt.AddHours(m);
                            key = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0).ToString("M/d/yy");
                            key = key + " H" + (i + 1).ToString();
                            CongestionValue = Math.Round(LMPValueSource - EPValue, 2);

                            GraphEPdata.Add(key, CongestionValue);
                        }

                    }

                    dt = dt.AddDays(1);
                }
            }

            return GraphEPdata;
        }

        private List<LmpTimePrice> CalculateDARTLMPHelper(List<LmpTimePrice> LMPValuesDicSource, List<LmpTimePrice> LMPValuesDicSink)
        {

            string dicdatekey;
            int dichourkey;
            double CongestionValueSource = 0, CongestionValueSink = 0;


            List<LmpTimePrice> LmpTimePriceListDart = new List<LmpTimePrice>();
            double dartCongestion;

            foreach (var sourcelist in LMPValuesDicSource)
            {
                DateTime dt1 = sourcelist.MarketTime;

                foreach (var sinkList in LMPValuesDicSink)
                {

                    DateTime dt2 = sinkList.MarketTime;
                    if (dt1.Equals(dt2))
                    {

                        dartCongestion = sinkList.Lmp.Congestion - sourcelist.Lmp.Congestion;


                        LmpTimePrice LmpTimePriceHelper = new LmpTimePrice();
                        LMP LMPObj = new LMP();
                        LMPObj.Price = dartCongestion;
                        LMPObj.EnergyPrice = dartCongestion;
                        LMPObj.Congestion = dartCongestion;
                        LmpTimePriceHelper.MarketTime = dt1;
                        LmpTimePriceHelper.Lmp = LMPObj;
                        LmpTimePriceListDart.Add(LmpTimePriceHelper);
                    }
                }
            }




            return LmpTimePriceListDart;

            //if (LMPValuesDicSource.Count > 0 && LMPValuesDicSink.Count>0)
            //{
            //    DateTime dt = EPriceStartDate;

            //    while (dt.Date <= EPriceEndDate.Date)
            //    {

            //        for (int i = 0, m = 0; i < 24; i++, m++)
            //        {

            //            dichourkey = m;
            //            dicdatekey = dt.ToString("yyyy-MM-dd");
            //            string lmpdicdatekey = dt.ToString("M/d/yyyy");

            //            if (LMPValuesDicSource..ContainsKey(dicdatekey))
            //            {

            //                if (LMPValuesDicSource.ContainsKey(lmpdicdatekey))
            //                {
            //                    var TempLMPPricedicSource = LMPValuesDicSource[lmpdicdatekey];
            //                    if (TempLMPPricedicSource.ContainsKey(dichourkey))
            //                        CongestionValueSource = TempLMPPricedicSource[dichourkey];// dictempItem.Value;

            //                }

            //                if (LMPValuesDicSink.ContainsKey(lmpdicdatekey))
            //                {
            //                    var TempLMPPricedicSink = LMPValuesDicSink[lmpdicdatekey];
            //                    if (TempLMPPricedicSink.ContainsKey(dichourkey))
            //                        CongestionValueSink = TempLMPPricedicSink[dichourkey];// dictempItem.Value;

            //                }



            //                dt = new DateTime(dt.Year, dt.Month, dt.Day, i, 0, 0);
            //                dt.AddMinutes(-dt.Minute);
            //                dt.AddHours(m);

            //                Node n = new Node();

            //                LMP LMPObj = new LMP();
            //                LmpTimePrice LmpTimePriceHelper = new LmpTimePrice();
            //                LMPObj.Price = CongestionValueSource- CongestionValueSink;
            //               // LMPObj.Energyprice = EPValue;
            //                LMPObj.Congestion = CongestionValueSource - CongestionValueSink;
            //                LmpTimePriceHelper.MarketTime = dt;
            //                LmpTimePriceHelper.Lmp = LMPObj;
            //                LmpTimePriceList.Add(LmpTimePriceHelper);

            //            }

            //        }

            //        dt = dt.AddDays(1);
            //    }
            //}

            // return LmpTimePriceListnew;
        }

        private void RefreshTradedVolume()
        {
            try
            {
                SourceTradedVolumesList = null;
                if (MarketComboSelectedValue != "ERCOT")
                {
                    System.Windows.MessageBox.Show("Please Select Ercot Market !!!");
                    return;
                }
                System.TimeSpan datediff = TdStartDate.Subtract(TdEndDate);
                int totalday = Convert.ToInt32(datediff.TotalDays);
                if (totalday <= -32)
                {
                    System.Windows.MessageBox.Show("Select Date Differnce should be one Months !!!");
                    return;
                }
                if (TdStartDate > TdEndDate)
                {
                    System.Windows.MessageBox.Show("Start Date should not be greater than End Date");
                    return;
                }
                if (SourceSinkDataSelected.Source == null & SourceSinkDataSelected.Sink == null)
                {
                    System.Windows.MessageBox.Show("Please select Node Name");
                    return;
                }
                string Source = SourceSinkDataSelected.Source.ToString();
                string Sink = SourceSinkDataSelected.Sink.ToString();
                if (TrededSourceChecked)
                {
                    if (Source != string.Empty)
                    {
                        TradedVolumesList = mDataService.GetTradedVolumesData(Source.ToString(), TdStartDate, TdEndDate);
                        TradedVolumesList = TradedVolumesList.OrderBy(p => p.MarketDateTime).ToList();
                        if (TradedVolumesList.Count == 0)
                        {
                            System.Windows.MessageBox.Show("Not Found !!!");
                            return;
                        }
                        int Node = TradedVolumesList[0].NodeKey;
                        List<TradedVolumesData> templist = SourceTrededData(TradedVolumesList, Source, Sink);
                        SourceTradedVolumesList = templist.OrderBy(a => a.RowNumber).ToList();

                        List<TradedVolumesData> tempsinklist = SinkTrededData(TradedVolumesList, Source, Sink); // SinkTradedVolumesList
                        SinkTradedVolumesList = tempsinklist.OrderBy(a => a.RowNumber).ToList();

                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please select Source Name");
                        return;
                    }
                }
                else
                {
                    if (Sink != string.Empty)
                    {
                        SinkVolumesList = mDataService.GetTradedVolumesData(Sink.ToString(), TdStartDate, TdEndDate);
                        SinkVolumesList = SinkVolumesList.OrderBy(p => p.MarketDateTime).ToList();
                        if (SinkVolumesList.Count == 0)
                        {
                            System.Windows.MessageBox.Show("Not Found !!!");
                            return;
                        }
                        int Node = SinkVolumesList[0].NodeKey;

                        List<TradedVolumesData> templist = SourceTrededData(SinkVolumesList, Source, Sink);
                        SourceTradedVolumesList = templist.OrderBy(a => a.RowNumber).ToList();

                        List<TradedVolumesData> tempSinklist = SinkTrededData(SinkVolumesList, Source, Sink); // SinkTradedVolumesList
                        SinkTradedVolumesList = tempSinklist.OrderBy(a => a.RowNumber).ToList();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please select Sink Name");
                        return;
                    }
                }
            }
            catch
            {

            }
        }

        void ConnectEPService()
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
            ChannelFactory<ILMPEnergyPrice> pipeFactory = new ChannelFactory<ILMPEnergyPrice>(myBinding, new EndpointAddress(mEndPoint));
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
                mLMPEnergyPriceProxy = pipeFactory.CreateChannel();
            }
            catch (Exception ex)
            {
            }



        }

        public List<TradedVolumesData> SourceTrededData(List<TradedVolumesData> lstTrededData, string SourceName, string SinkName)
        {
            List<TradedVolumesData> tempVolumesList = new List<TradedVolumesData>();

            List<TradedVolumesData> lstSourceTradedVolumes = new List<TradedVolumesData>();

            TradedVolumesData objSourceTradedVolumes = new TradedVolumesData();

            double? totalSourceD1 = 0, totalSourceD2 = 0, totalSourceD3 = 0, totalSourceD4 = 0, totalSourceD5 = 0, totalSourceD6 = 0, totalSourceD7 = 0, totalSourceD8 = 0, totalSourceD9 = 0, totalSourceD10 = 0
                , totalSourceD11 = 0, totalSourceD12 = 0, totalSourceD13 = 0, totalSourceD14 = 0, totalSourceD15 = 0, totalSourceD16 = 0, totalSourceD17 = 0, totalSourceD18 = 0, totalSourceD19 = 0, totalSourceD20 = 0
                , totalSourceD21 = 0, totalSourceD22 = 0, totalSourceD23 = 0, totalSourceD24 = 0, totalSourceD25 = 0, totalSourceD26 = 0, totalSourceD27 = 0, totalSourceD28 = 0, totalSourceD29 = 0, totalSourceD30 = 0, totalSourceD31 = 0;
            try
            {
                for (int i = 0; i <= 25; i++)
                {
                    tempVolumesList = lstTrededData.Where(a => a.MarketDateTime.Hour == i).ToList();
                    int count = tempVolumesList.Count;
                    for (int j = 0; j < tempVolumesList.Count; j++)
                    {
                        objSourceTradedVolumes = new TradedVolumesData();
                        objSourceTradedVolumes.NodeKey = lstTrededData[0].NodeKey;
                        objSourceTradedVolumes.NodeName = SourceName;
                        objSourceTradedVolumes.MarketDateTime = tempVolumesList[j].MarketDateTime.Date;
                        double? tempVol = null;

                        #region 31 Days

                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 1))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 1)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D1 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D1 == null)
                                        objSourceTradedVolumes.D1 = 0;
                                    else
                                        totalSourceD1 += objSourceTradedVolumes.D1;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D1 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D1 == null)
                                        objSourceTradedVolumes.D1 = 0;
                                    else
                                        totalSourceD1 += objSourceTradedVolumes.D1;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 2))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 2)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D2 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D2 == null)
                                        objSourceTradedVolumes.D2 = 0;
                                    else
                                        totalSourceD2 += objSourceTradedVolumes.D2;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D2 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D2 == null)
                                        objSourceTradedVolumes.D2 = 0;
                                    else
                                        totalSourceD2 += objSourceTradedVolumes.D2;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 3))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 3)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D3 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D3 == null)
                                        objSourceTradedVolumes.D3 = 0;
                                    else
                                        totalSourceD3 += objSourceTradedVolumes.D3;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D3 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D3 == null)
                                        objSourceTradedVolumes.D3 = 0;
                                    else
                                        totalSourceD3 += objSourceTradedVolumes.D3;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 4))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 4)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D4 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D4 == null)
                                        objSourceTradedVolumes.D4 = 0;
                                    else
                                        totalSourceD4 += objSourceTradedVolumes.D4;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D4 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D4 == null)
                                        objSourceTradedVolumes.D4 = 0;
                                    else
                                        totalSourceD4 += objSourceTradedVolumes.D4;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 5))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 5)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D5 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D5 == null)
                                        objSourceTradedVolumes.D5 = 0;
                                    else
                                        totalSourceD5 += objSourceTradedVolumes.D5;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D5 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D5 == null)
                                        objSourceTradedVolumes.D5 = 0;
                                    else
                                        totalSourceD5 += objSourceTradedVolumes.D5;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 6))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 6)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D6 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D6 == null)
                                        objSourceTradedVolumes.D6 = 0;
                                    else
                                        totalSourceD6 += objSourceTradedVolumes.D6;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D6 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D6 == null)
                                        objSourceTradedVolumes.D6 = 0;
                                    else
                                        totalSourceD6 += objSourceTradedVolumes.D6;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 7))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 7)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D7 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D7 == null)
                                        objSourceTradedVolumes.D7 = 0;
                                    else
                                        totalSourceD7 += objSourceTradedVolumes.D7;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D7 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D7 == null)
                                        objSourceTradedVolumes.D7 = 0;
                                    else
                                        totalSourceD7 += objSourceTradedVolumes.D7;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 8))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 8)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D8 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D8 == null)
                                        objSourceTradedVolumes.D8 = 0;
                                    else
                                        totalSourceD8 += objSourceTradedVolumes.D8;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D8 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D8 == null)
                                        objSourceTradedVolumes.D8 = 0;
                                    else
                                        totalSourceD8 += objSourceTradedVolumes.D8;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 9))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 9)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D9 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D9 == null)
                                        objSourceTradedVolumes.D9 = 0;
                                    else
                                        totalSourceD9 += objSourceTradedVolumes.D9;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D9 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D9 == null)
                                        objSourceTradedVolumes.D9 = 0;
                                    else
                                        totalSourceD9 += objSourceTradedVolumes.D9;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 10))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 10)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D10 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D10 == null)
                                        objSourceTradedVolumes.D10 = 0;
                                    else
                                        totalSourceD10 += objSourceTradedVolumes.D10;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D10 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D10 == null)
                                        objSourceTradedVolumes.D10 = 0;
                                    else
                                        totalSourceD10 += objSourceTradedVolumes.D10;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 11))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 11)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D11 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D11 == null)
                                        objSourceTradedVolumes.D11 = 0;
                                    else
                                        totalSourceD11 += objSourceTradedVolumes.D11;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D11 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D11 == null)
                                        objSourceTradedVolumes.D11 = 0;
                                    else
                                        totalSourceD11 += objSourceTradedVolumes.D11;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 12))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 12)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D12 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D12 == null)
                                        objSourceTradedVolumes.D12 = 0;
                                    else
                                        totalSourceD12 += objSourceTradedVolumes.D12;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D12 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D12 == null)
                                        objSourceTradedVolumes.D12 = 0;
                                    else
                                        totalSourceD12 += objSourceTradedVolumes.D12;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 13))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 13)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D13 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D13 == null)
                                        objSourceTradedVolumes.D13 = 0;
                                    else
                                        totalSourceD13 += objSourceTradedVolumes.D13;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D13 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D13 == null)
                                        objSourceTradedVolumes.D13 = 0;
                                    else
                                        totalSourceD13 += objSourceTradedVolumes.D13;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 14))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 14)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D14 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D14 == null)
                                        objSourceTradedVolumes.D14 = 0;
                                    else
                                        totalSourceD14 += objSourceTradedVolumes.D14;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D14 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D14 == null)
                                        objSourceTradedVolumes.D14 = 0;
                                    else
                                        totalSourceD14 += objSourceTradedVolumes.D14;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 15))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 15)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D15 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D15 == null)
                                        objSourceTradedVolumes.D15 = 0;
                                    else
                                        totalSourceD15 += objSourceTradedVolumes.D15;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D15 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D15 == null)
                                        objSourceTradedVolumes.D15 = 0;
                                    else
                                        totalSourceD15 += objSourceTradedVolumes.D15;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 16))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 16)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D16 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D16 == null)
                                        objSourceTradedVolumes.D16 = 0;
                                    else
                                        totalSourceD16 += objSourceTradedVolumes.D16;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D16 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D16 == null)
                                        objSourceTradedVolumes.D16 = 0;
                                    else
                                        totalSourceD16 += objSourceTradedVolumes.D16;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 17))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 17)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D17 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D17 == null)
                                        objSourceTradedVolumes.D17 = 0;
                                    else
                                        totalSourceD17 += objSourceTradedVolumes.D17;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D17 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D17 == null)
                                        objSourceTradedVolumes.D17 = 0;
                                    else
                                        totalSourceD17 += objSourceTradedVolumes.D17;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 18))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 18)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D18 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D18 == null)
                                        objSourceTradedVolumes.D18 = 0;
                                    else
                                        totalSourceD18 += objSourceTradedVolumes.D18;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D18 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D18 == null)
                                        objSourceTradedVolumes.D18 = 0;
                                    else
                                        totalSourceD18 += objSourceTradedVolumes.D18;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 19))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 19)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D19 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D19 == null)
                                        objSourceTradedVolumes.D19 = 0;
                                    else
                                        totalSourceD19 += objSourceTradedVolumes.D19;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D19 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D19 == null)
                                        objSourceTradedVolumes.D19 = 0;
                                    else
                                        totalSourceD19 += objSourceTradedVolumes.D19;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 20))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 20)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D20 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D20 == null)
                                        objSourceTradedVolumes.D20 = 0;
                                    else
                                        totalSourceD20 += objSourceTradedVolumes.D20;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D20 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D20 == null)
                                        objSourceTradedVolumes.D20 = 0;
                                    else
                                        totalSourceD20 += objSourceTradedVolumes.D20;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 21))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 21)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D21 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D21 == null)
                                        objSourceTradedVolumes.D21 = 0;
                                    else
                                        totalSourceD21 += objSourceTradedVolumes.D21;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D21 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D21 == null)
                                        objSourceTradedVolumes.D21 = 0;
                                    else
                                        totalSourceD21 += objSourceTradedVolumes.D21;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 22))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 22)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D22 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D22 == null)
                                        objSourceTradedVolumes.D22 = 0;
                                    else
                                        totalSourceD22 += objSourceTradedVolumes.D22;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D22 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D22 == null)
                                        objSourceTradedVolumes.D22 = 0;
                                    else
                                        totalSourceD22 += objSourceTradedVolumes.D22;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 23))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 23)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D23 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D23 == null)
                                        objSourceTradedVolumes.D23 = 0;
                                    else
                                        totalSourceD23 += objSourceTradedVolumes.D23;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D23 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D23 == null)
                                        objSourceTradedVolumes.D23 = 0;
                                    else
                                        totalSourceD23 += objSourceTradedVolumes.D23;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 24))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 24)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D24 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D24 == null)
                                        objSourceTradedVolumes.D24 = 0;
                                    else
                                        totalSourceD24 += objSourceTradedVolumes.D24;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D24 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D24 == null)
                                        objSourceTradedVolumes.D24 = 0;
                                    else
                                        totalSourceD24 += objSourceTradedVolumes.D24;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 25))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 25)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D25 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D25 == null)
                                        objSourceTradedVolumes.D25 = 0;
                                    else
                                        totalSourceD25 += objSourceTradedVolumes.D25;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D25 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D25 == null)
                                        objSourceTradedVolumes.D25 = 0;
                                    else
                                        totalSourceD25 += objSourceTradedVolumes.D25;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 26))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 26)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D26 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 4.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D26 == null)
                                        objSourceTradedVolumes.D26 = 0;
                                    else
                                        totalSourceD26 += objSourceTradedVolumes.D26;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D26 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D26 == null)
                                        objSourceTradedVolumes.D26 = 0;
                                    else
                                        totalSourceD26 += objSourceTradedVolumes.D26;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 27))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 27)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D27 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D27 == null)
                                        objSourceTradedVolumes.D27 = 0;
                                    else
                                        totalSourceD27 += objSourceTradedVolumes.D27;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D27 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D27 == null)
                                        objSourceTradedVolumes.D27 = 0;
                                    else
                                        totalSourceD27 += objSourceTradedVolumes.D27;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 28))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 28)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D28 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D28 == null)
                                        objSourceTradedVolumes.D28 = 0;
                                    else
                                        totalSourceD28 += objSourceTradedVolumes.D28;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D28 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D28 == null)
                                        objSourceTradedVolumes.D28 = 0;
                                    else
                                        totalSourceD28 += objSourceTradedVolumes.D28;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 29))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 29)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D29 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D29 == null)
                                        objSourceTradedVolumes.D29 = 0;
                                    else
                                        totalSourceD29 += objSourceTradedVolumes.D29;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D29 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D29 == null)
                                        objSourceTradedVolumes.D29 = 0;
                                    else
                                        totalSourceD29 += objSourceTradedVolumes.D29;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 30))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 30)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D30 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D30 == null)
                                        objSourceTradedVolumes.D30 = 0;
                                    else
                                        totalSourceD30 += objSourceTradedVolumes.D30;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D30 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D30 == null)
                                        objSourceTradedVolumes.D30 = 0;
                                    else
                                        totalSourceD30 += objSourceTradedVolumes.D30;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 31))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 31)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D31 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = 24.ToString();
                                    objSourceTradedVolumes.RowNumber = 24;
                                    if (objSourceTradedVolumes.D31 == null)
                                        objSourceTradedVolumes.D31 = 0;
                                    else
                                        totalSourceD31 += objSourceTradedVolumes.D31;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SourceVolume;
                                    objSourceTradedVolumes.D31 = tempVolumesList[j].SourceVolume == 0 ? null : tempVol;
                                    objSourceTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSourceTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSourceTradedVolumes.D31 == null)
                                        objSourceTradedVolumes.D31 = 0;
                                    else
                                        totalSourceD31 += objSourceTradedVolumes.D31;
                                }
                            }
                        }

                        #endregion

                        lstSourceTradedVolumes.Add(objSourceTradedVolumes);
                    }

                    if (i == 25 && count == 0)
                    {
                        objSourceTradedVolumes = new TradedVolumesData();
                        objSourceTradedVolumes.Hour = "Total";
                        objSourceTradedVolumes.D1 = totalSourceD1;
                        objSourceTradedVolumes.D2 = totalSourceD2;
                        objSourceTradedVolumes.D3 = totalSourceD3;

                        objSourceTradedVolumes.D4 = totalSourceD4;
                        objSourceTradedVolumes.D5 = totalSourceD5;
                        objSourceTradedVolumes.D6 = totalSourceD6;

                        objSourceTradedVolumes.D7 = totalSourceD7;
                        objSourceTradedVolumes.D8 = totalSourceD8;
                        objSourceTradedVolumes.D9 = totalSourceD9;

                        objSourceTradedVolumes.D10 = totalSourceD10;
                        objSourceTradedVolumes.D11 = totalSourceD11;
                        objSourceTradedVolumes.D12 = totalSourceD12;

                        objSourceTradedVolumes.D13 = totalSourceD13;
                        objSourceTradedVolumes.D14 = totalSourceD14;
                        objSourceTradedVolumes.D15 = totalSourceD15;

                        objSourceTradedVolumes.D16 = totalSourceD16;
                        objSourceTradedVolumes.D17 = totalSourceD17;
                        objSourceTradedVolumes.D18 = totalSourceD18;

                        objSourceTradedVolumes.D19 = totalSourceD19;
                        objSourceTradedVolumes.D20 = totalSourceD20;
                        objSourceTradedVolumes.D21 = totalSourceD21;

                        objSourceTradedVolumes.D22 = totalSourceD22;
                        objSourceTradedVolumes.D23 = totalSourceD23;
                        objSourceTradedVolumes.D24 = totalSourceD24;

                        objSourceTradedVolumes.D25 = totalSourceD25;
                        objSourceTradedVolumes.D26 = totalSourceD26;
                        objSourceTradedVolumes.D27 = totalSourceD27;

                        objSourceTradedVolumes.D28 = totalSourceD28;
                        objSourceTradedVolumes.D29 = totalSourceD29;
                        objSourceTradedVolumes.D30 = totalSourceD30;
                        objSourceTradedVolumes.D31 = totalSourceD31;
                        objSourceTradedVolumes.RowNumber = 25;
                        lstSourceTradedVolumes.Add(objSourceTradedVolumes);
                    }
                }
            }
            catch
            {

            }
            return lstSourceTradedVolumes.OrderBy(a => a.RowNumber).ToList();
        }
        public List<TradedVolumesData> SinkTrededData(List<TradedVolumesData> lstSinkTrededData, string SourceName, string SinkName)
        {
            List<TradedVolumesData> tempVolumesList = new List<TradedVolumesData>();
            List<TradedVolumesData> lstSinkTradedVolumes = new List<TradedVolumesData>();
            TradedVolumesData objSinkTradedVolumes = new TradedVolumesData();
            double? totalSinkD1 = 0, totalSinkD2 = 0, totalSinkD3 = 0, totalSinkD4 = 0, totalSinkD5 = 0, totalSinkD6 = 0, totalSinkD7 = 0, totalSinkD8 = 0, totalSinkD9 = 0, totalSinkD10 = 0
                , totalSinkD11 = 0, totalSinkD12 = 0, totalSinkD13 = 0, totalSinkD14 = 0, totalSinkD15 = 0, totalSinkD16 = 0, totalSinkD17 = 0, totalSinkD18 = 0, totalSinkD19 = 0, totalSinkD20 = 0
                , totalSinkD21 = 0, totalSinkD22 = 0, totalSinkD23 = 0, totalSinkD24 = 0, totalSinkD25 = 0, totalSinkD26 = 0, totalSinkD27 = 0, totalSinkD28 = 0, totalSinkD29 = 0, totalSinkD30 = 0, totalSinkD31 = 0;

            try
            {
                for (int i = 0; i <= 25; i++)
                {
                    tempVolumesList = lstSinkTrededData.Where(a => a.MarketDateTime.Hour == i).ToList();
                    int count = tempVolumesList.Count;
                    for (int j = 0; j < tempVolumesList.Count; j++)
                    {
                        objSinkTradedVolumes = new TradedVolumesData();
                        objSinkTradedVolumes.NodeKey = lstSinkTrededData[0].NodeKey;
                        objSinkTradedVolumes.NodeName = SinkName;
                        objSinkTradedVolumes.MarketDateTime = tempVolumesList[j].MarketDateTime.Date;
                        double? tempVol = null;
                        #region 31 Days

                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 1))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 1)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D1 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D1 == null)
                                        objSinkTradedVolumes.D1 = 0;
                                    else
                                        totalSinkD1 += objSinkTradedVolumes.D1;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D1 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D1 == null)
                                        objSinkTradedVolumes.D1 = 0;
                                    else
                                        totalSinkD1 += objSinkTradedVolumes.D1;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 2))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 2)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D2 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D2 == null)
                                        objSinkTradedVolumes.D2 = 0;
                                    else
                                        totalSinkD2 += objSinkTradedVolumes.D2;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D2 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D2 == null)
                                        objSinkTradedVolumes.D2 = 0;
                                    else
                                        totalSinkD2 += objSinkTradedVolumes.D2;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 3))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 3)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D3 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D3 == null)
                                        objSinkTradedVolumes.D3 = 0;
                                    else
                                        totalSinkD3 += objSinkTradedVolumes.D3;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D3 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D3 == null)
                                        objSinkTradedVolumes.D3 = 0;
                                    else
                                        totalSinkD3 += objSinkTradedVolumes.D3;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 4))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 4)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D4 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D4 == null)
                                        objSinkTradedVolumes.D4 = 0;
                                    else
                                        totalSinkD4 += objSinkTradedVolumes.D4;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D4 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D4 == null)
                                        objSinkTradedVolumes.D4 = 0;
                                    else
                                        totalSinkD4 += objSinkTradedVolumes.D4;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 5))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 5)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D5 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D5 == null)
                                        objSinkTradedVolumes.D5 = 0;
                                    else
                                        totalSinkD5 += objSinkTradedVolumes.D5;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D5 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D5 == null)
                                        objSinkTradedVolumes.D5 = 0;
                                    else
                                        totalSinkD5 += objSinkTradedVolumes.D5;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 6))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 6)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D6 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D6 == null)
                                        objSinkTradedVolumes.D6 = 0;
                                    else
                                        totalSinkD6 += objSinkTradedVolumes.D6;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D6 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D6 == null)
                                        objSinkTradedVolumes.D6 = 0;
                                    else
                                        totalSinkD6 += objSinkTradedVolumes.D6;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 7))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 7)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D7 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D7 == null)
                                        objSinkTradedVolumes.D7 = 0;
                                    else
                                        totalSinkD7 += objSinkTradedVolumes.D7;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D7 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D7 == null)
                                        objSinkTradedVolumes.D7 = 0;
                                    else
                                        totalSinkD7 += objSinkTradedVolumes.D7;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 8))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 8)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D8 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D8 == null)
                                        objSinkTradedVolumes.D8 = 0;
                                    else
                                        totalSinkD8 += objSinkTradedVolumes.D8;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D8 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D8 == null)
                                        objSinkTradedVolumes.D8 = 0;
                                    else
                                        totalSinkD8 += objSinkTradedVolumes.D8;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 9))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 9)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D9 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D9 == null)
                                        objSinkTradedVolumes.D9 = 0;
                                    else
                                        totalSinkD9 += objSinkTradedVolumes.D9;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D9 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D9 == null)
                                        objSinkTradedVolumes.D9 = 0;
                                    else
                                        totalSinkD9 += objSinkTradedVolumes.D9;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 10))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 10)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D10 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D10 == null)
                                        objSinkTradedVolumes.D10 = 0;
                                    else
                                        totalSinkD10 += objSinkTradedVolumes.D10;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D10 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D10 == null)
                                        objSinkTradedVolumes.D10 = 0;
                                    else
                                        totalSinkD10 += objSinkTradedVolumes.D10;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 11))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 11)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D11 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D11 == null)
                                        objSinkTradedVolumes.D11 = 0;
                                    else
                                        totalSinkD11 += objSinkTradedVolumes.D11;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D11 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D11 == null)
                                        objSinkTradedVolumes.D11 = 0;
                                    else
                                        totalSinkD11 += objSinkTradedVolumes.D11;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 12))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 12)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D12 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D12 == null)
                                        objSinkTradedVolumes.D12 = 0;
                                    else
                                        totalSinkD12 += objSinkTradedVolumes.D12;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D12 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D12 == null)
                                        objSinkTradedVolumes.D12 = 0;
                                    else
                                        totalSinkD12 += objSinkTradedVolumes.D12;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 13))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 13)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D13 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D13 == null)
                                        objSinkTradedVolumes.D13 = 0;
                                    else
                                        totalSinkD13 += objSinkTradedVolumes.D13;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D13 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D13 == null)
                                        objSinkTradedVolumes.D13 = 0;
                                    else
                                        totalSinkD13 += objSinkTradedVolumes.D13;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 14))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 14)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D14 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D14 == null)
                                        objSinkTradedVolumes.D14 = 0;
                                    else
                                        totalSinkD14 += objSinkTradedVolumes.D14;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D14 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D14 == null)
                                        objSinkTradedVolumes.D14 = 0;
                                    else
                                        totalSinkD14 += objSinkTradedVolumes.D14;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 15))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 15)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D15 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D15 == null)
                                        objSinkTradedVolumes.D15 = 0;
                                    else
                                        totalSinkD15 += objSinkTradedVolumes.D15;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D15 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D15 == null)
                                        objSinkTradedVolumes.D15 = 0;
                                    else
                                        totalSinkD15 += objSinkTradedVolumes.D15;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 16))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 16)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D16 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D16 == null)
                                        objSinkTradedVolumes.D16 = 0;
                                    else
                                        totalSinkD16 += objSinkTradedVolumes.D16;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D16 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D16 == null)
                                        objSinkTradedVolumes.D16 = 0;
                                    else
                                        totalSinkD16 += objSinkTradedVolumes.D16;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 17))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 17)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D17 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D17 == null)
                                        objSinkTradedVolumes.D17 = 0;
                                    else
                                        totalSinkD17 += objSinkTradedVolumes.D17;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D17 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D17 == null)
                                        objSinkTradedVolumes.D17 = 0;
                                    else
                                        totalSinkD17 += objSinkTradedVolumes.D17;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 18))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 18)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D18 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D18 == null)
                                        objSinkTradedVolumes.D18 = 0;
                                    else
                                        totalSinkD18 += objSinkTradedVolumes.D18;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D18 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D18 == null)
                                        objSinkTradedVolumes.D18 = 0;
                                    else
                                        totalSinkD18 += objSinkTradedVolumes.D18;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 19))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 19)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D19 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D19 == null)
                                        objSinkTradedVolumes.D19 = 0;
                                    else
                                        totalSinkD19 += objSinkTradedVolumes.D19;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D19 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D19 == null)
                                        objSinkTradedVolumes.D19 = 0;
                                    else
                                        totalSinkD19 += objSinkTradedVolumes.D19;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 20))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 20)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D20 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D20 == null)
                                        objSinkTradedVolumes.D20 = 0;
                                    else
                                        totalSinkD20 += objSinkTradedVolumes.D20;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D20 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D20 == null)
                                        objSinkTradedVolumes.D20 = 0;
                                    else
                                        totalSinkD20 += objSinkTradedVolumes.D20;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 21))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 21)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D21 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D21 == null)
                                        objSinkTradedVolumes.D21 = 0;
                                    else
                                        totalSinkD21 += objSinkTradedVolumes.D21;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D21 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D21 == null)
                                        objSinkTradedVolumes.D21 = 0;
                                    else
                                        totalSinkD21 += objSinkTradedVolumes.D21;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 22))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 22)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D22 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D22 == null)
                                        objSinkTradedVolumes.D22 = 0;
                                    else
                                        totalSinkD22 += objSinkTradedVolumes.D22;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D22 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D22 == null)
                                        objSinkTradedVolumes.D22 = 0;
                                    else
                                        totalSinkD22 += objSinkTradedVolumes.D22;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 23))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 23)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D23 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D23 == null)
                                        objSinkTradedVolumes.D23 = 0;
                                    else
                                        totalSinkD23 += objSinkTradedVolumes.D23;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D23 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D23 == null)
                                        objSinkTradedVolumes.D23 = 0;
                                    else
                                        totalSinkD23 += objSinkTradedVolumes.D23;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 24))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 24)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D24 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D24 == null)
                                        objSinkTradedVolumes.D24 = 0;
                                    else
                                        totalSinkD24 += objSinkTradedVolumes.D24;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D24 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D24 == null)
                                        objSinkTradedVolumes.D24 = 0;
                                    else
                                        totalSinkD24 += objSinkTradedVolumes.D24;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 25))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 25)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D25 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D25 == null)
                                        objSinkTradedVolumes.D25 = 0;
                                    else
                                        totalSinkD25 += objSinkTradedVolumes.D25;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D25 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D25 == null)
                                        objSinkTradedVolumes.D25 = 0;
                                    else
                                        totalSinkD25 += objSinkTradedVolumes.D25;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 26))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 26)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D26 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D26 == null)
                                        objSinkTradedVolumes.D26 = 0;
                                    else
                                        totalSinkD26 += objSinkTradedVolumes.D26;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D26 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D26 == null)
                                        objSinkTradedVolumes.D26 = 0;
                                    else
                                        totalSinkD26 += objSinkTradedVolumes.D26;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 27))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 27)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D27 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D27 == null)
                                        objSinkTradedVolumes.D27 = 0;
                                    else
                                        totalSinkD27 += objSinkTradedVolumes.D27;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D27 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D27 == null)
                                        objSinkTradedVolumes.D27 = 0;
                                    else
                                        totalSinkD27 += objSinkTradedVolumes.D27;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 28))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 28)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D28 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D28 == null)
                                        objSinkTradedVolumes.D28 = 0;
                                    else
                                        totalSinkD28 += objSinkTradedVolumes.D28;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D28 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D28 == null)
                                        objSinkTradedVolumes.D28 = 0;
                                    else
                                        totalSinkD28 += objSinkTradedVolumes.D28;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 29))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 29)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D29 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D29 == null)
                                        objSinkTradedVolumes.D29 = 0;
                                    else
                                        totalSinkD29 += objSinkTradedVolumes.D29;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D29 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D29 == null)
                                        objSinkTradedVolumes.D29 = 0;
                                    else
                                        totalSinkD29 += objSinkTradedVolumes.D29;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 30))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 30)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D30 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D30 == null)
                                        objSinkTradedVolumes.D30 = 0;
                                    else
                                        totalSinkD30 += objSinkTradedVolumes.D30;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D30 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D30 == null)
                                        objSinkTradedVolumes.D30 = 0;
                                    else
                                        totalSinkD30 += objSinkTradedVolumes.D30;
                                }
                                j++;
                            }
                        }
                        if (tempVolumesList.Exists(a => a.MarketDateTime.Day == 31))
                        {
                            if (tempVolumesList[j].MarketDateTime.Day == 31)
                            {
                                if (tempVolumesList[j].Hour == 0.ToString())
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D31 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = 24.ToString();
                                    objSinkTradedVolumes.RowNumber = 24;
                                    if (objSinkTradedVolumes.D31 == null)
                                        objSinkTradedVolumes.D31 = 0;
                                    else
                                        totalSinkD31 += objSinkTradedVolumes.D31;
                                }
                                else
                                {
                                    tempVol = tempVolumesList[j].SinkVolume;
                                    objSinkTradedVolumes.D31 = tempVolumesList[j].SinkVolume == 0 ? null : tempVol;
                                    objSinkTradedVolumes.Hour = tempVolumesList[j].Hour;
                                    objSinkTradedVolumes.RowNumber = Convert.ToInt32(tempVolumesList[j].Hour);
                                    if (objSinkTradedVolumes.D31 == null)
                                        objSinkTradedVolumes.D31 = 0;
                                    else
                                        totalSinkD31 += objSinkTradedVolumes.D31;
                                }
                            }
                        }

                        #endregion

                        lstSinkTradedVolumes.Add(objSinkTradedVolumes);
                    }
                    if (i == 25 && count == 0)
                    {
                        objSinkTradedVolumes = new TradedVolumesData();
                        objSinkTradedVolumes.Hour = "Total";
                        objSinkTradedVolumes.D1 = totalSinkD1;
                        objSinkTradedVolumes.D2 = totalSinkD2;
                        objSinkTradedVolumes.D3 = totalSinkD3;

                        objSinkTradedVolumes.D4 = totalSinkD4;
                        objSinkTradedVolumes.D5 = totalSinkD5;
                        objSinkTradedVolumes.D6 = totalSinkD6;

                        objSinkTradedVolumes.D7 = totalSinkD7;
                        objSinkTradedVolumes.D8 = totalSinkD8;
                        objSinkTradedVolumes.D9 = totalSinkD9;

                        objSinkTradedVolumes.D10 = totalSinkD10;
                        objSinkTradedVolumes.D11 = totalSinkD11;
                        objSinkTradedVolumes.D12 = totalSinkD12;

                        objSinkTradedVolumes.D13 = totalSinkD13;
                        objSinkTradedVolumes.D14 = totalSinkD14;
                        objSinkTradedVolumes.D15 = totalSinkD15;

                        objSinkTradedVolumes.D16 = totalSinkD16;
                        objSinkTradedVolumes.D17 = totalSinkD17;
                        objSinkTradedVolumes.D18 = totalSinkD18;

                        objSinkTradedVolumes.D19 = totalSinkD19;
                        objSinkTradedVolumes.D20 = totalSinkD20;
                        objSinkTradedVolumes.D21 = totalSinkD21;

                        objSinkTradedVolumes.D22 = totalSinkD22;
                        objSinkTradedVolumes.D23 = totalSinkD23;
                        objSinkTradedVolumes.D24 = totalSinkD24;

                        objSinkTradedVolumes.D25 = totalSinkD25;
                        objSinkTradedVolumes.D26 = totalSinkD26;
                        objSinkTradedVolumes.D27 = totalSinkD27;

                        objSinkTradedVolumes.D28 = totalSinkD28;
                        objSinkTradedVolumes.D29 = totalSinkD29;
                        objSinkTradedVolumes.D30 = totalSinkD30;
                        objSinkTradedVolumes.D31 = totalSinkD31;
                        objSinkTradedVolumes.RowNumber = 25;
                        lstSinkTradedVolumes.Add(objSinkTradedVolumes);
                    }
                }
            }
            catch
            {

            }
            return lstSinkTradedVolumes.OrderBy(a => a.RowNumber).ToList();
        }

        public void ExportEnergyPrices()
        {
            // SourceSinkEnergyPriceList
            try
            {
                if (SourceSinkEnergyPriceList != null)
                {
                    if (SourceSinkEnergyPriceList.Count > 0)
                    {
                        ExportToExcelNodeSpread<TradedVolumesData, List<TradedVolumesData>> objSource = new ExportToExcelNodeSpread<TradedVolumesData, List<TradedVolumesData>>();
                        List<TradedVolumesData> itemListSource = new List<TradedVolumesData>();
                        ICollectionView viewSource = CollectionViewSource.GetDefaultView(SourceSinkEnergyPriceList);
                        foreach (var item in viewSource.SourceCollection)
                        {
                            itemListSource.Add((TradedVolumesData)item);
                        }
                        objSource.dataToPrint = itemListSource;
                        objSource.GenerateReport();
                    }
                    else
                    {
                        MessageBox.Show("Not Found !!!");
                    }

                }
                else
                {
                    MessageBox.Show("Not Found !!!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        public void Export()
        {
            try
            {
                if (SourceTradedVolumesList != null || SinkTradedVolumesList != null)
                {
                    if (SourceTradedVolumesList.Count > 0)
                    {
                        ExportToExcelNodeSpread<TradedVolumesData, List<TradedVolumesData>> objSource = new ExportToExcelNodeSpread<TradedVolumesData, List<TradedVolumesData>>();
                        List<TradedVolumesData> itemListSource = new List<TradedVolumesData>();
                        ICollectionView viewSource = CollectionViewSource.GetDefaultView(SourceTradedVolumesList);
                        foreach (var item in viewSource.SourceCollection)
                        {
                            itemListSource.Add((TradedVolumesData)item);
                        }
                        objSource.dataToPrint = itemListSource;
                        objSource.GenerateReport();
                    }
                    else
                    {
                        MessageBox.Show("Not Found !!!");
                    }
                    if (SinkTradedVolumesList.Count > 0)
                    {
                        #region Sink
                        ExportToExcelNodeSpread<TradedVolumesData, List<TradedVolumesData>> objSink = new ExportToExcelNodeSpread<TradedVolumesData, List<TradedVolumesData>>();
                        List<TradedVolumesData> itemListSink = new List<TradedVolumesData>();
                        ICollectionView viewSink = CollectionViewSource.GetDefaultView(SinkTradedVolumesList);
                        foreach (var item in viewSink.SourceCollection)
                        {
                            itemListSink.Add((TradedVolumesData)item);
                        }
                        objSink.dataToPrint = itemListSink;
                        objSink.GenerateReport();
                        #endregion Sink
                    }
                    else
                    {
                        MessageBox.Show("Not Found !!!");
                    }
                }
                else
                {
                    MessageBox.Show("Not Found !!!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public class HourlyData
        {
            /// <summary>
            /// Gets or sets the date.
            /// </summary>
            /// <value>
            /// The date.
            /// </value>
            public DateTime Date { get; set; }
            /// <summary>
            /// Gets or sets the hour.
            /// </summary>
            /// <value>
            /// The hour.
            /// </value>
            public int Hour { get; set; }
            /// <summary>
            /// Gets or sets the source.
            /// </summary>
            /// <value>
            /// The source.
            /// </value>
            public double? Source { get; set; }
            /// <summary>
            /// Gets or sets the sink.
            /// </summary>
            /// <value>
            /// The sink.
            /// </value>
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
        /// Calculates the dart.
        /// </summary>
        /// <param name="daList">The da list.</param>
        /// <param name="rtList">The rt list.</param>
        /// <param name="isVirtual">if set to <c>true</c> [is virtual].</param>
        /// <returns></returns>
        private Node CalculateDART(Node daList, Node rtList, bool isVirtual)
        {
            List<LmpTimePrice> dartList = new List<LmpTimePrice>();
            Node currentNode = new Node();
            try
            {
                for (int i = 0; i < daList.LmpTimePriceList.Count; i++)
                {
                    if (daList.LmpTimePriceList[i].MarketTime == rtList.LmpTimePriceList[i].MarketTime)
                    {
                        LmpTimePrice dart = new LmpTimePrice();
                        dart.Lmp = new Vayu.NodePriceLibrary.LMP();
                        dart.MarketTime = daList.LmpTimePriceList[i].MarketTime;
                        if (daList.LmpTimePriceList[i].Lmp.Price.Equals(double.NaN) || rtList.LmpTimePriceList[i].Lmp.Price.Equals(double.NaN))
                        {
                            dart.Lmp.Price = double.NaN;
                        }
                        else
                        {
                            double da = daList.LmpTimePriceList[i].Lmp.Price;
                            double rt = 0;
                            double daCong = daList.LmpTimePriceList[i].Lmp.Congestion;
                            double daLoss = daList.LmpTimePriceList[i].Lmp.Loss;
                            double daEnergy = daList.LmpTimePriceList[i].Lmp.Energy;
                            double rtCong = 0; double rtLoss = 0; double rtEnergy = 0;
                            if (!rtList.LmpTimePriceList[i].Lmp.Price.Equals(double.NaN))
                            {
                                rt = rtList.LmpTimePriceList[i].Lmp.Price;
                                if (mMarketComboSelectedValue == "PJM")
                                {
                                    rtCong = rtList.LmpTimePriceList[i].Lmp.Congestion;
                                    rtLoss = rtList.LmpTimePriceList[i].Lmp.Loss;
                                    rtEnergy = rtList.LmpTimePriceList[i].Lmp.Energy;
                                }
                            }
                            if (IncChecked && isVirtual)
                            {
                                dart.Lmp.Price = (da - rt) - Fee;
                            }
                            else
                            {
                                dart.Lmp.Price = (rt - da) - Fee;
                                if (mMarketComboSelectedValue == "PJM")
                                {
                                    dart.Lmp.Congestion = (rtCong - daCong) - Fee;
                                    dart.Lmp.Loss = (rtLoss - daLoss) - Fee;
                                    dart.Lmp.Energy = (rtEnergy - daEnergy) - Fee;
                                }
                            }
                        }
                        dartList.Add(dart);
                    }
                }
                currentNode.Market = daList.Market;
                currentNode.NodeId = daList.NodeId;
                currentNode.NodeName = daList.NodeName;
                currentNode.PNodeId = daList.PNodeId;
                currentNode.LmpTimePriceList = dartList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return currentNode;
        }
        /// <summary>
        /// Sets the source sink list.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        public void SetSourceSinkList(List<SourceSinkData> sourceSinkList)
        {
            try
            {
                foreach (SourceSinkData sourceSink in sourceSinkList)
                {
                    if (sourceSink.Sink == null)
                    {
                        if (!mSourceSinkDataList.Exists(compareSourceSink => (compareSourceSink.Source.NodeKey == sourceSink.Source.NodeKey)))
                        {
                            mSourceSinkDataList.Add(sourceSink);
                        }
                    }
                    else
                    {
                        if (!mSourceSinkDataList.Exists(compareSourceSink => (compareSourceSink.Source.NodeKey == sourceSink.Source.NodeKey && compareSourceSink.Sink.NodeKey == sourceSink.Sink.NodeKey)))
                        {
                            mSourceSinkDataList.Add(sourceSink);
                        }
                    }
                }
                mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
                foreach (SourceSinkData sourceSinkData in sourceSinkList)
                {
                    string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() : sourceSinkData.Source.NodeKey.ToString() + ":" +
                                                    sourceSinkData.Sink.NodeKey.ToString();
                    if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                    {
                        mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                    }
                }
                SourceSinkList = null;
                SourceSinkList = mSourceSinkDataList;

                SourceSinkDataSelected = SourceSinkList[mSourceSinkDataList.Count - 1];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Sets the values from portfolio.
        /// </summary>
        /// <param name="values">The values.</param>
        public void SetValuesFromPortfolio(List<string> values)
        {
            try
            {
                HE1Checked = false;
                HE2Checked = false;
                HE3Checked = false;
                HE4Checked = false;
                HE5Checked = false;
                HE6Checked = false;
                HE7Checked = false;
                HE8Checked = false;
                HE9Checked = false;
                HE10Checked = false;
                HE11Checked = false;
                HE12Checked = false;
                HE13Checked = false;
                HE14Checked = false;
                HE15Checked = false;
                HE16Checked = false;
                HE17Checked = false;
                HE18Checked = false;
                HE19Checked = false;
                HE24Checked = false;
                HE20Checked = false;
                HE21Checked = false;
                HE22Checked = false;
                HE23Checked = false;
                StartDate = DateTime.Parse(values[1]);
                EndDate = DateTime.Parse(values[2]);
                UptosChecked = bool.Parse(values[3]);
                string[] hourValues = values[0].Split('.');
                foreach (string hour in hourValues)
                {
                    int eachHour = int.Parse(hour);
                    switch (eachHour)
                    {
                        case 1:
                            HE1Checked = true;
                            break;
                        case 2:
                            HE2Checked = true;
                            break;
                        case 3:
                            HE3Checked = true;
                            break;
                        case 4:
                            HE4Checked = true;
                            break;
                        case 5:
                            HE5Checked = true;
                            break;
                        case 6:
                            HE6Checked = true;
                            break;
                        case 7:
                            HE7Checked = true;
                            break;
                        case 8:
                            HE8Checked = true;
                            break;
                        case 9:
                            HE9Checked = true;
                            break;
                        case 10:
                            HE10Checked = true;
                            break;
                        case 11:
                            HE11Checked = true;
                            break;
                        case 12:
                            HE12Checked = true;
                            break;
                        case 13:
                            HE13Checked = true;
                            break;
                        case 14:
                            HE14Checked = true;
                            break;
                        case 15:
                            HE15Checked = true;
                            break;
                        case 16:
                            HE16Checked = true;
                            break;
                        case 17:
                            HE17Checked = true;
                            break;
                        case 18:
                            HE18Checked = true;
                            break;
                        case 19:
                            HE19Checked = true;
                            break;
                        case 20:
                            HE20Checked = true;
                            break;
                        case 21:
                            HE21Checked = true;
                            break;
                        case 22:
                            HE22Checked = true;
                            break;
                        case 23:
                            HE23Checked = true;
                            break;
                        case 24:
                            HE24Checked = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// Calculates the spread.
        /// </summary>
        /// <param name="node1Values">The node1 values.</param>
        /// <param name="node2Values">The node2 values.</param>
        /// <returns></returns>
        private Node CalculateSpread(Node node1Values, Node node2Values)
        {
            List<LmpTimePrice> spreadPriceList = new List<LmpTimePrice>();
            Node currentNode = new Node();

            try
            {
                for (int i = 0; i < node1Values.LmpTimePriceList.Count; i++)
                {
                    if (node1Values.LmpTimePriceList[i].MarketTime == node2Values.LmpTimePriceList[i].MarketTime)
                    {
                        LmpTimePrice spreadPrice = new LmpTimePrice();
                        spreadPrice.MarketTime = node1Values.LmpTimePriceList[i].MarketTime;
                        if (node1Values.LmpTimePriceList[i].Lmp.Price.Equals(double.NaN) || node2Values.LmpTimePriceList[i].Lmp.Price.Equals(double.NaN))
                        {
                            spreadPrice.Lmp.Price = double.NaN;
                        }
                        else
                        {
                            double node1Price = node1Values.LmpTimePriceList[i].Lmp.Price;
                            double node2Price = 0;
                            if (!node2Values.LmpTimePriceList[i].Lmp.Price.Equals(double.NaN))
                            {
                                node2Price = node2Values.LmpTimePriceList[i].Lmp.Price;
                            }
                            spreadPrice.Lmp.Price = node1Price - node2Price;
                        }
                        spreadPriceList.Add(spreadPrice);
                    }
                }
                currentNode.Market = node1Values.Market;
                currentNode.NodeId = node1Values.NodeId;
                currentNode.NodeName = node1Values.NodeName;
                currentNode.PNodeId = node1Values.PNodeId;
                currentNode.LmpTimePriceList = spreadPriceList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return currentNode;
        }


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
        /// <summary>
        /// The hourly
        /// </summary>
        Hourly,
        /// <summary>
        /// The daily
        /// </summary>
        Daily
    }
    /// <summary>
    /// 
    /// </summary>
    public enum DataFilteredType
    {
        /// <summary>
        /// The original
        /// </summary>
        Orig,
        /// <summary>
        /// The filtered sorted
        /// </summary>
        FilteredSorted
    }
    /// <summary>
    /// 
    /// </summary>

    public class HourlyPivotData
    {
        /// <summary>
        /// The m hour data list
        /// </summary>
        private List<double?> mHourDataList;
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
        /// The m total
        /// </summary>
        private double? mTotal;
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
                return mTotal;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    mTotal = null;
                }
                else
                {
                    mTotal = value;
                }
            }
        }
        private double? _MaxLoad;

        public double? MaxLoad
        {
            get { return _MaxLoad; }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    _MaxLoad = null;
                }
                else
                    _MaxLoad = value;
            }
        }
        /// <summary>
        /// The m average
        /// </summary>
        private double? mAverage;
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
                return mAverage;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    mAverage = null;
                }
                else
                {
                    mAverage = value;
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HourlyPivotData"/> class.
        /// </summary>
        public HourlyPivotData()
        {
            mHourDataList = new List<double?>();
            for (int i = 0; i < 24; i++)
            {
                mHourDataList.Add(null);
            }
        }
        /// <summary>
        /// Gets or sets the hour data list.
        /// </summary>
        /// <value>
        /// The hour data list.
        /// </value>
        public List<double?> HourDataList
        {
            get
            {
                return mHourDataList;
            }
            set
            {
                mHourDataList = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e1.
        /// </summary>
        /// <value>
        /// The h e1.
        /// </value>
        public double? HE1
        {
            get
            {
                return mHourDataList[0];
            }
            set
            {
                mHourDataList[0] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e2.
        /// </summary>
        /// <value>
        /// The h e2.
        /// </value>
        public double? HE2
        {
            get
            {
                return mHourDataList[1];
            }
            set
            {
                mHourDataList[1] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e3.
        /// </summary>
        /// <value>
        /// The h e3.
        /// </value>
        public double? HE3
        {
            get
            {
                return mHourDataList[2];
            }
            set
            {
                mHourDataList[2] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e4.
        /// </summary>
        /// <value>
        /// The h e4.
        /// </value>
        public double? HE4
        {
            get
            {
                return mHourDataList[3];
            }
            set
            {
                mHourDataList[3] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e5.
        /// </summary>
        /// <value>
        /// The h e5.
        /// </value>
        public double? HE5
        {
            get
            {
                return mHourDataList[4];
            }
            set
            {
                mHourDataList[4] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e6.
        /// </summary>
        /// <value>
        /// The h e6.
        /// </value>
        public double? HE6
        {
            get
            {
                return mHourDataList[5];
            }
            set
            {
                mHourDataList[5] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e7.
        /// </summary>
        /// <value>
        /// The h e7.
        /// </value>
        public double? HE7
        {
            get
            {
                return mHourDataList[6];
            }
            set
            {
                mHourDataList[6] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e8.
        /// </summary>
        /// <value>
        /// The h e8.
        /// </value>
        public double? HE8
        {
            get
            {
                return mHourDataList[7];
            }
            set
            {
                mHourDataList[7] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e9.
        /// </summary>
        /// <value>
        /// The h e9.
        /// </value>
        public double? HE9
        {
            get
            {
                return mHourDataList[8];
            }
            set
            {
                mHourDataList[8] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e10.
        /// </summary>
        /// <value>
        /// The h e10.
        /// </value>
        public double? HE10
        {
            get
            {
                return mHourDataList[9];
            }
            set
            {
                mHourDataList[9] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e11.
        /// </summary>
        /// <value>
        /// The h e11.
        /// </value>
        public double? HE11
        {
            get
            {
                return mHourDataList[10];
            }
            set
            {
                mHourDataList[10] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e12.
        /// </summary>
        /// <value>
        /// The h e12.
        /// </value>
        public double? HE12
        {
            get
            {
                return mHourDataList[11];
            }
            set
            {
                mHourDataList[11] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e13.
        /// </summary>
        /// <value>
        /// The h e13.
        /// </value>
        public double? HE13
        {
            get
            {
                return mHourDataList[12];
            }
            set
            {
                mHourDataList[12] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e14.
        /// </summary>
        /// <value>
        /// The h e14.
        /// </value>
        public double? HE14
        {
            get
            {
                return mHourDataList[13];
            }
            set
            {
                mHourDataList[13] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e15.
        /// </summary>
        /// <value>
        /// The h e15.
        /// </value>
        public double? HE15
        {
            get
            {
                return mHourDataList[14];
            }
            set
            {
                mHourDataList[14] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e16.
        /// </summary>
        /// <value>
        /// The h e16.
        /// </value>
        public double? HE16
        {
            get
            {
                return mHourDataList[15];
            }
            set
            {
                mHourDataList[15] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e17.
        /// </summary>
        /// <value>
        /// The h e17.
        /// </value>
        public double? HE17
        {
            get
            {
                return mHourDataList[16];
            }
            set
            {
                mHourDataList[16] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e18.
        /// </summary>
        /// <value>
        /// The h e18.
        /// </value>
        public double? HE18
        {
            get
            {
                return mHourDataList[17];
            }
            set
            {
                mHourDataList[17] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e19.
        /// </summary>
        /// <value>
        /// The h e19.
        /// </value>
        public double? HE19
        {
            get
            {
                return mHourDataList[18];
            }
            set
            {
                mHourDataList[18] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e20.
        /// </summary>
        /// <value>
        /// The h e20.
        /// </value>
        public double? HE20
        {
            get
            {
                return mHourDataList[19];
            }
            set
            {
                mHourDataList[19] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e21.
        /// </summary>
        /// <value>
        /// The h e21.
        /// </value>
        public double? HE21
        {
            get
            {
                return mHourDataList[20];
            }
            set
            {
                mHourDataList[20] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e22.
        /// </summary>
        /// <value>
        /// The h e22.
        /// </value>
        public double? HE22
        {
            get
            {
                return mHourDataList[21];
            }
            set
            {
                mHourDataList[21] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e23.
        /// </summary>
        /// <value>
        /// The h e23.
        /// </value>
        public double? HE23
        {
            get
            {
                return mHourDataList[22];
            }
            set
            {
                mHourDataList[22] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e24.
        /// </summary>
        /// <value>
        /// The h e24.
        /// </value>
        public double? HE24
        {
            get
            {
                return mHourDataList[23];
            }
            set
            {
                mHourDataList[23] = value;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// 
    public class HourlyPivotDataCongestion
    {
        /// <summary>
        /// The m hour data list
        /// </summary>
        private List<double?> mHourDataList;
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime? MarketDate { get; set; }
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
        /// The m total
        /// </summary>
        private double? mTotal;
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
                return mTotal;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    mTotal = null;
                }
                else
                {
                    mTotal = value;
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HourlyPivotData"/> class.
        /// </summary>
        public HourlyPivotDataCongestion()
        {
            mHourDataList = new List<double?>();
            for (int i = 0; i < 24; i++)
            {
                mHourDataList.Add(null);
            }
        }
        /// <summary>
        /// Gets or sets the hour data list.
        /// </summary>
        /// <value>
        /// The hour data list.
        /// </value>
        public List<double?> HourDataList
        {
            get
            {
                return mHourDataList;
            }
            set
            {
                mHourDataList = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e1.
        /// </summary>
        /// <value>
        /// The h e1.
        /// </value>
        public double? HE1_Source_Cong
        {
            get
            {
                return mHourDataList[0];
            }
            set
            {
                mHourDataList[0] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e2.
        /// </summary>
        /// <value>
        /// The h e2.
        /// </value>
        public double? HE2_Source_Cong
        {
            get
            {
                return mHourDataList[1];
            }
            set
            {
                mHourDataList[1] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e3.
        /// </summary>
        /// <value>
        /// The h e3.
        /// </value>
        public double? HE3_Source_Cong
        {
            get
            {
                return mHourDataList[2];
            }
            set
            {
                mHourDataList[2] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e4.
        /// </summary>
        /// <value>
        /// The h e4.
        /// </value>
        public double? HE4_Source_Cong
        {
            get
            {
                return mHourDataList[3];
            }
            set
            {
                mHourDataList[3] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e5.
        /// </summary>
        /// <value>
        /// The h e5.
        /// </value>
        public double? HE5_Source_Cong
        {
            get
            {
                return mHourDataList[4];
            }
            set
            {
                mHourDataList[4] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e6.
        /// </summary>
        /// <value>
        /// The h e6.
        /// </value>
        public double? HE6_Source_Cong
        {
            get
            {
                return mHourDataList[5];
            }
            set
            {
                mHourDataList[5] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e7.
        /// </summary>
        /// <value>
        /// The h e7.
        /// </value>
        public double? HE7_Source_Cong
        {
            get
            {
                return mHourDataList[6];
            }
            set
            {
                mHourDataList[6] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e8.
        /// </summary>
        /// <value>
        /// The h e8.
        /// </value>
        public double? HE8_Source_Cong
        {
            get
            {
                return mHourDataList[7];
            }
            set
            {
                mHourDataList[7] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e9.
        /// </summary>
        /// <value>
        /// The h e9.
        /// </value>
        public double? HE9_Source_Cong
        {
            get
            {
                return mHourDataList[8];
            }
            set
            {
                mHourDataList[8] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e10.
        /// </summary>
        /// <value>
        /// The h e10.
        /// </value>
        public double? HE10_Source_Cong
        {
            get
            {
                return mHourDataList[9];
            }
            set
            {
                mHourDataList[9] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e11.
        /// </summary>
        /// <value>
        /// The h e11.
        /// </value>
        public double? HE11_Source_Cong
        {
            get
            {
                return mHourDataList[10];
            }
            set
            {
                mHourDataList[10] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e12.
        /// </summary>
        /// <value>
        /// The h e12.
        /// </value>
        public double? HE12_Source_Cong
        {
            get
            {
                return mHourDataList[11];
            }
            set
            {
                mHourDataList[11] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e13.
        /// </summary>
        /// <value>
        /// The h e13.
        /// </value>
        public double? HE13_Source_Cong
        {
            get
            {
                return mHourDataList[12];
            }
            set
            {
                mHourDataList[12] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e14.
        /// </summary>
        /// <value>
        /// The h e14.
        /// </value>
        public double? HE14_Source_Cong
        {
            get
            {
                return mHourDataList[13];
            }
            set
            {
                mHourDataList[13] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e15.
        /// </summary>
        /// <value>
        /// The h e15.
        /// </value>
        public double? HE15_Source_Cong
        {
            get
            {
                return mHourDataList[14];
            }
            set
            {
                mHourDataList[14] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e16.
        /// </summary>
        /// <value>
        /// The h e16.
        /// </value>
        public double? HE16_Source_Cong
        {
            get
            {
                return mHourDataList[15];
            }
            set
            {
                mHourDataList[15] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e17.
        /// </summary>
        /// <value>
        /// The h e17.
        /// </value>
        public double? HE17_Source_Cong
        {
            get
            {
                return mHourDataList[16];
            }
            set
            {
                mHourDataList[16] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e18.
        /// </summary>
        /// <value>
        /// The h e18.
        /// </value>
        public double? HE18_Source_Cong
        {
            get
            {
                return mHourDataList[17];
            }
            set
            {
                mHourDataList[17] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e19.
        /// </summary>
        /// <value>
        /// The h e19.
        /// </value>
        public double? HE19_Source_Cong
        {
            get
            {
                return mHourDataList[18];
            }
            set
            {
                mHourDataList[18] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e20.
        /// </summary>
        /// <value>
        /// The h e20.
        /// </value>
        public double? HE20_Source_Cong
        {
            get
            {
                return mHourDataList[19];
            }
            set
            {
                mHourDataList[19] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e21.
        /// </summary>
        /// <value>
        /// The h e21.
        /// </value>
        public double? HE21_Source_Cong
        {
            get
            {
                return mHourDataList[20];
            }
            set
            {
                mHourDataList[20] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e22.
        /// </summary>
        /// <value>
        /// The h e22.
        /// </value>
        public double? HE22_Source_Cong
        {
            get
            {
                return mHourDataList[21];
            }
            set
            {
                mHourDataList[21] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e23.
        /// </summary>
        /// <value>
        /// The h e23.
        /// </value>
        public double? HE23_Source_Cong
        {
            get
            {
                return mHourDataList[22];
            }
            set
            {
                mHourDataList[22] = value;
            }
        }
        /// <summary>
        /// Gets or sets the h e24.
        /// </summary>
        /// <value>
        /// The h e24.
        /// </value>
        public double? HE24_Source_Cong
        {
            get
            {
                return mHourDataList[23];
            }
            set
            {
                mHourDataList[23] = value;
            }
        }



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
        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public string Hours { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double? Price { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class PasteHelperVirtual
    {
        /// <summary>
        /// Gets or sets the source node.
        /// </summary>
        /// <value>
        /// The source node.
        /// </value>
        public string SourceNode { get; set; }
        /// <summary>
        /// Gets or sets the sink node.
        /// </summary>
        /// <value>
        /// The sink node.
        /// </value>
        public string SinkNode { get; set; }
        /// <summary>
        /// Gets or sets the inc decimal.
        /// </summary>
        /// <value>
        /// The inc decimal.
        /// </value>
        public string INC_DEC { get; set; }
        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public string Hours { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double? MW { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double? Price { get; set; }
    }
    public class Bollinger
    {
        public double UpperLimit { get; set; }
        public double LowerLimit { get; set; }
        public double StdDev { get; set; }
        public DateTime MarketDateTime { get; set; }
        public double DARTTotal { get; set; }
        public double Average { get; set; }
    }
    public class GraphItem
    {
        public DateTime X { get; set; }
        public double? Y { get; set; }
    }
    public class LmpCorrelate
    {
        public int Hour { get; set; }
        public List<double> RTList { get; set; }
        public List<double> DAList { get; set; }
    }

}
