using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Vayu.DBLibrary;
using Vayu.NodeSensitivityAnalysis.Model;

namespace Vayu.NodeSensitivityAnalysis.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {


        #region Relay Command Properties  
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand ClearCommand { get; private set; }
        public DelegateCommand AllErcotShiftFactors { get; private set; }
        public DelegateCommand RunPasteCommand { private set; get; }
        #endregion

        #region Properties

        public Dictionary<string, double> ploadDictHash = new Dictionary<string, double>();

        public Dictionary<string, double> eloadDictHash = new Dictionary<string, double>();

        private Dictionary<string, SourceSinkData> mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
        private IDataService dataService;
        public string[] MarketList { get; set; }

        private bool msinkSeachEnable;

        public bool sinkSeachEnable
        {
            get { return msinkSeachEnable; }
            set
            {
                msinkSeachEnable = value;
                RaisePropertyChanged("sinkSeachEnable");
            }
        }

        private bool mSelectedPath;

        public bool SelectedPath
        {
            get
            {
                return mSelectedPath;
            }
            set
            {
                mSelectedPath = value;
                if (value)
                {
                    sinkSeachEnable = true;

                }
                else
                {
                    sinkSeachEnable = false;

                }
                if (!UptosChecked)
                {
                    SetSourceSink();
                }
                RaisePropertyChanged("SelectedPath");
            }
        }
        private bool mSelectedEnablePath;

        public bool SelectedEnablePath
        {
            get { return mSelectedEnablePath; }
            set
            {
                mSelectedEnablePath = value;

                RaisePropertyChanged("SelectedEnablePath");
            }
        }

        private bool mSelectedNode = true;

        public bool SelectedNode
        {
            get
            {
                return mSelectedNode;
            }
            set
            {
                mSelectedNode = value;
                if (value)
                {
                    sinkSeachEnable = true;
                }
                else
                {
                    sinkSeachEnable = false;
                }
                if (!UptosChecked)
                {
                    SetSourceSink();
                }
                RaisePropertyChanged("SelectedNode");
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
                // SetSourceSink(); 
                RaisePropertyChanged("UptosChecked");
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
        public Constraint SelectedConstraint { get; set; }

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

        public System.Reflection.PropertyInfo ColumnSelected { get; set; }

        private List<PricingNode> mSourceSearchList;

        public List<PricingNode> SourceSearchList
        {
            get { return mSourceSearchList; }
            set
            {
                mSourceSearchList = value;
                // SetSourceSink();
                RaisePropertyChanged("SourceSearchList");

            }

        }


        private string _SearchTypedText;
        public string SearchTypedText
        {
            get { return _SearchTypedText; }
            set
            {
                _SearchTypedText = value;
                RaisePropertyChanged("SearchTypedText");
            }
        }

        private PricingNode mSelectedSinkItem;

        public PricingNode SelectedSinkItem
        {
            get { return mSelectedSinkItem; }
            set
            {
                mSelectedSinkItem = value;
                RaisePropertyChanged("SelectedSinkItem");
            }
        }


        private PricingNode _SelectedSourceItem;

        public PricingNode SelectedSourceItem
        {
            get { return _SelectedSourceItem; }
            set
            {
                _SelectedSourceItem = value;

                RaisePropertyChanged("SelectedSourceItem");
            }
        }

        private List<PricingNode> _SinkSearchList;

        public List<PricingNode> SinkSearchList
        {
            get { return _SinkSearchList; }
            set
            {
                _SinkSearchList = value;
                RaisePropertyChanged("SinkSearchList");
            }
        }

        private string mMarketSelected;
        public string MarketSelected
        {
            get { return mMarketSelected; }
            set
            {
                mMarketSelected = value;
                RaisePropertyChanged("MarketSelected");
                mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
                if (MarketSelected.ToUpper() == "ERCOT")
                {
                    isErcotEnabled = true;
                }
                else
                {
                    isErcotEnabled = false;
                }
                fillConstraintSearchList(RTChecked);
                if (value != null)
                {
                    IsSpp = value.ToUpper() == "SPP";
                    ConstraintList = new List<Constraint>();
                }
            }
        }

        private DateTime mFromDate = DateTime.Today;

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

                mFromDate = value.Date;
                RaisePropertyChanged("StartDate");
                RaisePropertyChanged("FromDate");
            }
        }

        private DateTime mThroDate = DateTime.Today;

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
        private List<Constraint> mConstraintList;
        public List<Constraint> ConstraintList
        {
            get { return mConstraintList; }
            set
            {
                mConstraintList = value;
                RaisePropertyChanged("ConstraintList");
                IsRefreshEnabled = true;
            }
        }
        private bool mDAChecked;
        public bool DAChecked
        {
            get { return mDAChecked; }
            set
            {
                mDAChecked = value;
                SetSourceSink();
                RaisePropertyChanged("DAChecked");
            }
        }
        private bool mRTChecked;
        public bool RTChecked
        {
            get { return mRTChecked; }
            set
            {
                mRTChecked = value;
                SetSourceSink();
                RaisePropertyChanged("RTChecked");

            }
        }

        private bool mDateRangeCheckBoxChecked;
        public bool DateRangeCheckBoxChecked
        {
            get { return mDateRangeCheckBoxChecked; }
            set
            {
                mDateRangeCheckBoxChecked = value;
                if (FromDate > ThroDate)
                {
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


        private bool mIsSpp = false;

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
        /// The m source SNK data selected
        /// </summary>
        private SourceSinkData mSrcSnkDataSelected;
        /// <summary>
        /// Gets or sets the source or sink data selected.
        /// </summary>
        /// <value>
        /// The source or sink data selected.
        /// </value>
        public SourceSinkData SourceSinkDataSelected
        {
            get
            {
                return mSrcSnkDataSelected;
            }
            set
            {
                mSrcSnkDataSelected = value;

                RaisePropertyChanged("SourceSinkDataSelected");
                GetSensitivityData(GetMarketKey());
            }
        }
        #endregion

        #region Main method
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="_dataService">The data service.</param>
        public MainWindowViewModel(IDataService _dataService = null)
        {
            MarketList = new string[] { "ERCOT" };
            MarketSelected = MarketList.FirstOrDefault();
            dataService = _dataService ?? new DataService();
            IsRefreshEnabled = true;
            RTChecked = true;
            SetSourceSink();
            RefreshCommand = new DelegateCommand(() => RefreshList());
            IsRefreshEnabled = true;
            ClearCommand = new DelegateCommand(() => ClearListBox());
            RunPasteCommand = new DelegateCommand(() => Paste());
        }
        #endregion

        #region Sensitivity Code
        /// <summary>
        /// For clear Data from Gird table
        /// </summary>
        public void ClearListBox()
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            if (sourceSinkData != null && (sourceSinkData.Source != null && sourceSinkData.Sink != null))
            {
                string sourceSinkKey = sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                if (mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Remove(sourceSinkKey);
                }
            }
            if (sourceSinkData != null && sourceSinkData.Source != null)
            {
                string sourceSinkKey = sourceSinkData.Source.NodeKey.ToString();
                if (mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Remove(sourceSinkKey);
                }
            }
            ConstraintList.Clear();

            this.IsRefreshEnabled = true;
            SourceSinkList = null;
            SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
            SourceSinkDataSelected = null;
            RefreshList();
        }

        /// <summary>
        /// To paste Source/Sink in dropdown
        /// </summary>
        private void Paste()
        {
            try
            {
                List<PasteHelp> lstDataHelper = new List<PasteHelp>();
                IDataObject iData = Clipboard.GetDataObject();
                if (!iData.GetDataPresent(DataFormats.Text))
                {
                    return;
                }
                string text = (string)Clipboard.GetData(DataFormats.Text);
                string[] rowData = text.Split('\n');
                foreach (string rowItem in rowData)
                {
                    if (rowItem != string.Empty)
                    {
                        string[] cellData = rowItem.Split('\t');
                        if (cellData.Length == 2)
                        {

                            lstDataHelper.Add(new PasteHelp
                            {
                                Source = cellData[0].Replace("\r", ""),
                                Sink = cellData[1].Replace("\r", "")
                            });
                            SelectedPath = true;

                        }
                        if (cellData.Length == 1)
                        {
                            lstDataHelper.Add(new PasteHelp
                            {
                                Source = cellData[0].Replace("\r", ""),
                            });
                        }
                    }
                }
                if (lstDataHelper.Count > 0)
                {
                    PasteSourceSinkData(lstDataHelper);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(System.Reflection.MethodInfo.GetCurrentMethod() + "\t" + ex.Message);
            }
        }

        private void PasteSourceSinkData(List<PasteHelp> lstDataHelp)
        {
            DataService mds = new DataService();
            var tempSrcSnkDataList = new List<SourceSinkData>();
            foreach (PasteHelp pdata in lstDataHelp)
            {
                var sourceData = SourceSearchList.Where(a => a.NodeName.ToLower() == pdata.Source.ToLower()).FirstOrDefault();
                var sinkData = SinkSearchList.Where(a => a.NodeName.ToLower() == pdata.Sink.Trim().ToLower()).FirstOrDefault();
                tempSrcSnkDataList.Add(new SourceSinkData
                {
                    Source = sourceData,
                    Sink = sinkData
                });
            }
            SourceSinkList = tempSrcSnkDataList.ToList();
            SourceSinkDataSelected = SourceSinkList[0];
            RefreshList();

        }

        /// <summary>
        /// To retrive Source/Sink Sensitivity
        /// </summary>
        private void RefreshList()
        {
            if (SelectedSourceItem != null || SelectedSinkItem != null)
            {
                SourceSinkData sourceSinkData = new SourceSinkData();
                sourceSinkData.Source = SelectedSourceItem;
                sourceSinkData.Sink = SelectedSinkItem;
                string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                                sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                {
                    mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                    SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                    SourceSinkDataSelected = sourceSinkData;
                }
            }
            if (SourceSinkList != null && SourceSinkList.Count > 0)
            {
                foreach (SourceSinkData srcSinkData in SourceSinkList)
                {
                    SourceSinkData sourceSinkData = new SourceSinkData();
                    sourceSinkData.Source = srcSinkData.Source;
                    sourceSinkData.Sink = srcSinkData.Sink;
                    string sourceSinkKey = sourceSinkData.Sink == null ? sourceSinkData.Source.NodeKey.ToString() :
                                                    sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                    if (!mFillSourceSinkHash.ContainsKey(sourceSinkKey))
                    {
                        mFillSourceSinkHash.Add(sourceSinkKey, sourceSinkData);
                        SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
                        SourceSinkDataSelected = sourceSinkData;
                    }
                }
            }
            SelectedSourceItem = null;
            SelectedSinkItem = null;
            ConstraintList.Clear();
            //RefreshList();
            GetSensitivityData(GetMarketKey());
            IsRefreshEnabled = true;
        }

        public void GetSensitivityData(int p)
        {
            if (SelectedSourceItem != null || SelectedSinkItem != null || (SourceSinkList != null && SourceSinkList.Count() > 0))
            {
                List<Constraint> sdt = new List<Constraint>();
                ConstraintList = new List<Constraint>();
                dataService = dataService ?? new DataService();
                List<Constraint> sensitivityList = new List<Constraint>();

                dataService.GetConstraintData((h, e) =>
                {
                    bool isDA = false;
                    if (DAChecked)
                        isDA = true;
                    else
                        isDA = false;

                    DataService ds = new DataService();
                    foreach (Constraint c in h)
                    {
                        if (SourceSinkDataSelected != null && SourceSinkDataSelected.Sink == null)
                        {
                            sensitivityList.AddRange(ds.GetErcotSensitivitiesData(isDA, c.ConstraintText, c.ContingencyText, SourceSinkDataSelected.Source.ToString(), null, c.ConstraintDate));
                        }
                        else if (SourceSinkDataSelected != null)
                        {
                            sensitivityList.AddRange(ds.GetErcotSensitivitiesData(isDA, c.ConstraintText, c.ContingencyText, SourceSinkDataSelected.Source.ToString(), SourceSinkDataSelected.Sink.ToString(), c.ConstraintDate));
                        }
                    }
                    ConstraintList = sensitivityList;
                    this.IsRefreshEnabled = true;
                }, p, DAChecked, FromDate, DateRangeCheckBoxChecked ? ThroDate : (DateTime?)null);
                SelectedSourceItem = null;
                SelectedSinkItem = null;
            }
            else
            {
                //MessageBox.Show("Please Select Source/Sink");
            }
        }

        private void fillConstraintSearchList(bool isRt)
        {
            DataService ds = new DataService();
            SearchTypedText = "";
            if (RTChecked)
            {
                // SourceSearchList = ds.FillConstraintList(true, GetMarketKey());
            }
            else if (DAChecked)
            {
                //SinkSearchList = ds.FillConstraintList(false, GetMarketKey());
            }

        }

        public int GetMarketKey()
        {
            switch (MarketSelected)
            {


                case "ERCOT": return 9;

                default: return 0;
            }
        }

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
                        SourceSearchList = item1.Item1;
                        SinkSearchList = item1.Item2;

                    }, MarketSelected, product);
        }
        #endregion
    }
    /// <seealso cref="System.Windows.Data.IValueConverter" />
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
    public class PasteHelp
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
    }
}
