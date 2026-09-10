using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Vayu.UptosPathAnalysisAlgorithm.Model;

namespace Vayu.UptosPathAnalysisAlgorithm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m selected robot type
        /// </summary>
        private AlgoType mSelectedRobotType;
        /// <summary>
        /// The m temporary robot list
        /// </summary>
        private List<RobotTypeList> mTempRobotList;
        /// <summary>
        /// The refresh task
        /// </summary>
        private Task refreshTask;

        #region Properties

        /// <summary>
        /// Gets or sets the run retrieve data command.
        /// </summary>
        /// <value>
        /// The run retrieve data command.
        /// </value>
        public DelegateCommand RunRetrieveDataCommand { private set; get; }
        /// <summary>
        /// Gets or sets the reset robot list command.
        /// </summary>
        /// <value>
        /// The reset robot list command.
        /// </value>
        public DelegateCommand ResetRobotListCommand { private set; get; }
        /// <summary>
        /// Gets or sets the filter robot list command.
        /// </summary>
        /// <value>
        /// The filter robot list command.
        /// </value>
        public DelegateCommand FilterRobotListCommand { private set; get; }
        /// <summary>
        /// Gets or sets the export to excel.
        /// </summary>
        /// <value>
        /// The export to excel.
        /// </value>
        public DelegateCommand ExportToExcel { private set; get; }
        /// <summary>
        /// Gets or sets the autogenerating column.
        /// </summary>
        /// <value>
        /// The autogenerating column.
        /// </value>
        public DelegateCommand AutogeneratingColumn { private set; get; }
        /// <summary>
        /// Gets or sets the load data by date.
        /// </summary>
        /// <value>
        /// The load data by date.
        /// </value>
        public DelegateCommand LoadDataByDate { get; set; }

        /// <summary>
        /// The m algorithm list
        /// </summary>
        private List<classAlgorithmList> mAlgorithmList;
        /// <summary>
        /// Gets or sets the algorithm list.
        /// </summary>
        /// <value>
        /// The algorithm list.
        /// </value>
        public List<classAlgorithmList> AlgorithmList
        {
            get { return mAlgorithmList; }
            set
            {
                mAlgorithmList = value;
                RaisePropertyChanged("AlgorithmList");
            }
        }

        /// <summary>
        /// The m select all sink zone
        /// </summary>
        private bool mSelectAllSinkZone = true;
        /// <summary>
        /// Gets or sets a value indicating whether [select all sink zone].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [select all sink zone]; otherwise, <c>false</c>.
        /// </value>
        public bool SelectAllSinkZone
        {
            get
            {
                return mSelectAllSinkZone;
            }
            set
            {
                mSelectAllSinkZone = value;
                RaisePropertyChanged("SelectAllSinkZone");
            }
        }
        //
        /// <summary>
        /// The m un select all sink zone
        /// </summary>
        private bool mUnSelectAllSinkZone = true;
        /// <summary>
        /// Gets or sets a value indicating whether [un select all sink zone].
        /// </summary>
        /// <value>
        /// <c>true</c> if [un select all sink zone]; otherwise, <c>false</c>.
        /// </value>
        public bool UnSelectAllSinkZone
        {
            get
            {
                return mUnSelectAllSinkZone;
            }
            set
            {
                mUnSelectAllSinkZone = value;
                RaisePropertyChanged("UnSelectAllSinkZone");
            }
        }
        //
        /// <summary>
        /// The m select all sink node
        /// </summary>
        private bool mSelectAllSinkNode = true;
        /// <summary>
        /// Gets or sets a value indicating whether [select all sink node].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [select all sink node]; otherwise, <c>false</c>.
        /// </value>
        public bool SelectAllSinkNode
        {
            get
            {
                return mSelectAllSinkNode;
            }
            set
            {
                mSelectAllSinkNode = value;
                RaisePropertyChanged("SelectAllSinkNode");
            }
        }
        //
        /// <summary>
        /// The m un select all sink node
        /// </summary>
        private bool mUnSelectAllSinkNode = true;
        /// <summary>
        /// Gets or sets a value indicating whether [un select all sink node].
        /// </summary>
        /// <value>
        /// <c>true</c> if [un select all sink node]; otherwise, <c>false</c>.
        /// </value>
        public bool UnSelectAllSinkNode
        {
            get
            {
                return mUnSelectAllSinkNode;
            }
            set
            {
                mUnSelectAllSinkNode = value;
                RaisePropertyChanged("UnSelectAllSinkNode");
            }
        }
        //
        /// <summary>
        /// The m progress bar
        /// </summary>
        private bool mProgressBar;
        /// <summary>
        /// Gets or sets a value indicating whether [progress bar].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [progress bar]; otherwise, <c>false</c>.
        /// </value>
        public bool ProgressBar
        {
            get
            {
                return mProgressBar;
            }
            set
            {
                mProgressBar = value;
                RaisePropertyChanged("ProgressBar");
            }
        }
        /// <summary>
        /// The m source node type list
        /// </summary>
        private List<string> mSourceNodeTypeList;
        /// <summary>
        /// Gets or sets the source node type list.
        /// </summary>
        /// <value>
        /// The source node type list.
        /// </value>
        public List<string> SourceNodeTypeList
        {
            get
            {
                return mSourceNodeTypeList;
            }
            set
            {
                mSourceNodeTypeList = value;
                RaisePropertyChanged("SourceNodeTypeList");
            }
        }
        /// <summary>
        /// The m source node type selected item
        /// </summary>
        private List<string> mSourceNodeTypeSelectedItem;
        /// <summary>
        /// Gets or sets the source node type selected item.
        /// </summary>
        /// <value>
        /// The source node type selected item.
        /// </value>
        public List<string> SourceNodeTypeSelectedItem
        {
            get
            {
                return mSourceNodeTypeSelectedItem;
            }
            set
            {
                mSourceNodeTypeSelectedItem = value;
                RaisePropertyChanged("SourceNodeTypeSelectedItem");
            }
        }
        /// <summary>
        /// The m sink node type list
        /// </summary>
        private List<string> mSinkNodeTypeList;
        /// <summary>
        /// Gets or sets the sink node type list.
        /// </summary>
        /// <value>
        /// The sink node type list.
        /// </value>
        public List<string> SinkNodeTypeList
        {
            get
            {
                return mSinkNodeTypeList;
            }
            set
            {
                mSinkNodeTypeList = value;
                RaisePropertyChanged("SinkNodeTypeList");
            }
        }
        /// <summary>
        /// The m sink node type selected item
        /// </summary>
        private List<string> mSinkNodeTypeSelectedItem;
        /// <summary>
        /// Gets or sets the sink node type selected item.
        /// </summary>
        /// <value>
        /// The sink node type selected item.
        /// </value>
        public List<string> SinkNodeTypeSelectedItem
        {
            get
            {
                return mSinkNodeTypeSelectedItem;
            }
            set
            {
                mSinkNodeTypeSelectedItem = value;
                RaisePropertyChanged("SinkNodeTypeSelectedItem");
            }
        }
        /// <summary>
        /// The m source zone list
        /// </summary>
        private List<string> mSourceZoneList;
        /// <summary>
        /// Gets or sets the source zone list.
        /// </summary>
        /// <value>
        /// The source zone list.
        /// </value>
        public List<string> SourceZoneList
        {
            get
            {
                return mSourceZoneList;
            }
            set
            {
                mSourceZoneList = value;
                RaisePropertyChanged("SourceZoneList");
            }
        }
        /// <summary>
        /// The m source zone selected item
        /// </summary>
        private List<string> mSourceZoneSelectedItem;
        /// <summary>
        /// Gets or sets the source zone selected item.
        /// </summary>
        /// <value>
        /// The source zone selected item.
        /// </value>
        public List<string> SourceZoneSelectedItem
        {
            get
            {
                return mSourceZoneSelectedItem;
            }
            set
            {
                mSourceZoneSelectedItem = value;
                RaisePropertyChanged("SourceZoneSelectedItem");
            }
        }
        /// <summary>
        /// The m sink zone list
        /// </summary>
        private List<string> mSinkZoneList;
        /// <summary>
        /// Gets or sets the sink zone list.
        /// </summary>
        /// <value>
        /// The sink zone list.
        /// </value>
        public List<string> SinkZoneList
        {
            get
            {
                return mSinkZoneList;
            }
            set
            {
                mSinkZoneList = value;
                RaisePropertyChanged("SinkZoneList");
            }
        }
        /// <summary>
        /// The m sink zone selected item
        /// </summary>
        private List<string> mSinkZoneSelectedItem;
        /// <summary>
        /// Gets or sets the sink zone selected item.
        /// </summary>
        /// <value>
        /// The sink zone selected item.
        /// </value>
        public List<string> SinkZoneSelectedItem
        {
            get
            {
                return mSinkZoneSelectedItem;
            }
            set
            {
                mSinkZoneSelectedItem = value;
                RaisePropertyChanged("SinkZoneSelectedItem");
            }
        }
        /// <summary>
        /// The list count
        /// </summary>
        private int listCount;
        /// <summary>
        /// Gets or sets the list count.
        /// </summary>
        /// <value>
        /// The list count.
        /// </value>
        public int ListCount
        {
            get
            {
                return listCount;
            }
            set
            {
                listCount = value;
                RaisePropertyChanged("ListCount");
            }
        }
        /// <summary>
        /// The m robot type list
        /// </summary>
        private List<RobotTypeList> mRobotTypeList;
        /// <summary>
        /// Gets or sets the robot type data list.
        /// </summary>
        /// <value>
        /// The robot type data list.
        /// </value>
        public List<RobotTypeList> RobotTypeDataList
        {
            get
            {
                return mRobotTypeList;
            }
            set
            {
                mRobotTypeList = value;
                RaisePropertyChanged("RobotTypeDataList");
                if (value != null)
                {
                    ListCount = value.Count;
                }
                else
                {
                    ListCount = 0;
                }
            }
        }
        /// <summary>
        /// The m minimum price filter
        /// </summary>
        private double? mMinPriceFilter;
        /// <summary>
        /// Gets or sets the minimum price filter.
        /// </summary>
        /// <value>
        /// The minimum price filter.
        /// </value>
        public double? MinPriceFilter
        {
            get
            {
                return mMinPriceFilter;
            }
            set
            {
                mMinPriceFilter = value;
                RaisePropertyChanged("MinPriceFilter");
            }
        }
        /// <summary>
        /// The m maximum price filter
        /// </summary>
        private double? mMaxPriceFilter;
        /// <summary>
        /// Gets or sets the maximum price filter.
        /// </summary>
        /// <value>
        /// The maximum price filter.
        /// </value>
        public double? MaxPriceFilter
        {
            get
            {
                return mMaxPriceFilter;
            }
            set
            {
                mMaxPriceFilter = value;
                RaisePropertyChanged("MaxPriceFilter");
            }
        }
        /// <summary>
        /// The m sum value minimum filter
        /// </summary>
        private double? mSumValueMinFilter;
        /// <summary>
        /// Gets or sets the sum value minimum filter.
        /// </summary>
        /// <value>
        /// The sum value minimum filter.
        /// </value>
        public double? SumValueMinFilter
        {
            get
            {
                return mSumValueMinFilter;
            }
            set
            {
                mSumValueMinFilter = value;
                RaisePropertyChanged("SumValueMinFilter");
            }
        }
        /// <summary>
        /// The m sum value maximum filter
        /// </summary>
        private double? mSumValueMaxFilter;
        /// <summary>
        /// Gets or sets the sum value maximum filter.
        /// </summary>
        /// <value>
        /// The sum value maximum filter.
        /// </value>
        public double? SumValueMaxFilter
        {
            get
            {
                return mSumValueMaxFilter;
            }
            set
            {
                mSumValueMaxFilter = value;
                RaisePropertyChanged("SumValueMaxFilter");
            }
        }

        /// <summary>
        /// The selected date
        /// </summary>
        private DateTime? selectedDate;
        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public DateTime? SelectedDate
        {
            get { return selectedDate; }
            set { selectedDate = value; RaisePropertyChanged("SelectedDate"); OnLoadDataByDate(); }
        }

        /// <summary>
        /// The combo selected robot type
        /// </summary>
        private classAlgorithmList _ComboSelectedRobotType;
        /// <summary>
        /// Gets or sets the type of the combo selected robot.
        /// </summary>
        /// <value>
        /// The type of the combo selected robot.
        /// </value>
        public classAlgorithmList ComboSelectedRobotType
        {
            get { return _ComboSelectedRobotType; }
            set
            {
                _ComboSelectedRobotType = value;
                RaisePropertyChanged("ComboSelectedRobotType");

                if (ComboSelectedRobotType.AlgorithmList != null)
                {
                    if (ComboSelectedRobotType.AlgorithmList == "Ramp")
                    {
                        SelectedRobotType = AlgoType.Ramp;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "PeakOffPeak")
                    {
                        SelectedRobotType = AlgoType.PeakOffPeak;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "Block_B1")
                    {
                        SelectedRobotType = AlgoType.Block_B1;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "Block_B2")
                    {
                        SelectedRobotType = AlgoType.Block_B2;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "Block_B3")
                    {
                        SelectedRobotType = AlgoType.Block_B3;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "Virtual")
                    {
                        SelectedRobotType = AlgoType.VirtualBlock;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "UptosShort")
                    {
                        SelectedRobotType = AlgoType.UptosShort;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "VirtualShort")
                    {
                        SelectedRobotType = AlgoType.VirtualShort;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "Outage")
                    {
                        SelectedRobotType = AlgoType.Outage;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "PJMPositiveCorrelations")
                    {
                        SelectedRobotType = AlgoType.PJMPositiveCorrelations;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "VirtualPath")
                    {
                        SelectedRobotType = AlgoType.VirtualPath;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "ErcotBlock_B1")
                    {
                        SelectedRobotType = AlgoType.ErcotBlock_B1;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "ErcotBlock_B2")
                    {
                        SelectedRobotType = AlgoType.ErcotBlock_B2;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "ErcotPositiveCorrelations")
                    {
                        SelectedRobotType = AlgoType.ErcotPositiveCorrelations;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "PJMNegativeCorrelations")
                    {
                        SelectedRobotType = AlgoType.PJMNegativeCorrelations;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "ErcotNegativeCorrelations")
                    {
                        SelectedRobotType = AlgoType.ErcotNegativeCorrelations;
                    }
                    else if (ComboSelectedRobotType.AlgorithmList == "ErcotSubmittedBidsAlgo")
                    {
                        SelectedRobotType = AlgoType.ErcotSubmittedBidsAlgo;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the selected robot.
        /// </summary>
        /// <value>
        /// The type of the selected robot.
        /// </value>
        public AlgoType SelectedRobotType
        {
            get
            {
                return mSelectedRobotType;
            }
            set
            {
                //if (value == RobotType.VirtualBlock)
                //{
                //    MessageBox.Show("VirtualBlock is un-available");
                //    return;
                //}

                mSelectedRobotType = value;
                RaisePropertyChanged("SelectedRobotType");
                foreach (PropertyInfo propItem in this.GetType().GetProperties().Where(a => a.PropertyType == typeof(List<string>)))
                {
                    propItem.SetValue(this, null);
                }
                if (SelectedRobotType == AlgoType.VirtualBlock || SelectedRobotType == AlgoType.VirtualShort)
                {
                    SelectAllSinkZone = false;
                    UnSelectAllSinkZone = false;
                    SelectAllSinkNode = false;
                    UnSelectAllSinkNode = false;
                }
                else
                {
                    SelectAllSinkZone = true;
                    UnSelectAllSinkZone = true;
                    SelectAllSinkNode = true;
                    UnSelectAllSinkNode = true;
                }
                if (SelectedRobotType == AlgoType.Block_B1)
                {
                    selectedDate = mDataService.GetBlockMaxDate();
                    RaisePropertyChanged("SelectedDate");
                }
                RefreshData();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            mDataService.loadDBCommands();

            AlgorithmList = new List<classAlgorithmList> {
                                                            new classAlgorithmList("ErcotBlock_B1"),
                                                            new classAlgorithmList("ErcotBlock_B2")
                                                                           };

            _ComboSelectedRobotType = new classAlgorithmList("ErcotBlock_B1"); //original 
            //_ComboSelectedRobotType = new classAlgorithmList("ErcotBlock_B2"); //New line 
            SelectedRobotType = AlgoType.ErcotBlock_B1;
            //SelectedRobotType = AlgoType.ErcotBlock_B2;

            RunRetrieveDataCommand = new DelegateCommand(RefreshData);
            ResetRobotListCommand = new DelegateCommand(() => ResetRobotList());
            FilterRobotListCommand = new DelegateCommand(() => RefreshRobotList());
            ExportToExcel = new DelegateCommand(() => ExportExcel());
            AutogeneratingColumn = new DelegateCommand(() => OnAutoGeneratingColumn());
            LoadDataByDate = new DelegateCommand(() => OnLoadDataByDate());
        }

        #region Public Methods

        /// <summary>
        /// Called when [automatic generating column].
        /// </summary>
        private void OnAutoGeneratingColumn()
        {

        }
        /// <summary>
        /// Refreshes the data thread.
        /// </summary>
        private void RefreshDataThread()
        {
            ProgressBar = true;
            RobotTypeDataList = null;

            #region changes 
            //
            if (SelectedRobotType == AlgoType.ErcotBlock_B1)
            {
                List<string> foundList = new List<string>();
                List<RobotTypeList> robotTypeList = new List<RobotTypeList>();
                List<RobotTypeList> foundRobotTypleList = new List<RobotTypeList>();
                mDataService.GetEESRobotList((item1, error) =>
                {
                    if (error != null)
                    {
                        return;
                    }
                    robotTypeList = item1;
                }, "ErcotBlock_B1", SelectedDate);


                var groupedList = robotTypeList.GroupBy(x => new SourceSink() { Source = x.SourceName, Sink = x.SinkName });
                int count = groupedList.Count();
                foreach (var list in groupedList)
                {
                    List<int> anaList = new List<int>();
                    int innercount = list.OrderBy(x => x.AnalysisTypeInt).Count();
                    foreach (var item in list.OrderBy(x => x.AnalysisTypeInt))
                    {
                        int hour = item.AnalysisTypeInt;

                        if (!anaList.Contains(hour) && hour > 0)
                        {
                            anaList.Add(hour);
                            anaList.Add(hour + 1);
                            anaList.Add(hour + 2);
                        }
                        else
                            continue;

                        string analysisType = hour + "." + (hour + 1) + "." + (hour + 2);
                        RobotTypeList foundRobotType = item;
                        foundRobotType.AnalysisType = analysisType;
                        foundRobotTypleList.Add(foundRobotType);
                    }
                }

                List<RobotTypeList> tempList = foundRobotTypleList.ToList();
                // tempList.RemoveAll(a => a.Price > 50 || a.Price < -50);
                RobotTypeDataList = tempList.ToList();
            }
            if (SelectedRobotType == AlgoType.ErcotBlock_B2)
            {
                List<string> foundList = new List<string>();
                List<RobotTypeList> robotTypeList = new List<RobotTypeList>();
                List<RobotTypeList> foundRobotTypleList = new List<RobotTypeList>();
                mDataService.GetEESRobotList((item1, error) =>
                {
                    if (error != null)
                    {
                        return;
                    }
                    robotTypeList = item1;
                }, "ErcotBlock_B2", SelectedDate);


                var groupedList = robotTypeList.GroupBy(x => new SourceSink() { Source = x.SourceName, Sink = x.SinkName });
                foreach (var list in groupedList)
                {
                    List<int> anaList = new List<int>();
                    foreach (var item in list.OrderBy(x => x.AnalysisTypeInt))
                    {
                        int hour = item.AnalysisTypeInt;

                        if (!anaList.Contains(hour) && hour > 0)
                        {
                            anaList.Add(hour);
                            anaList.Add(hour + 1);
                            anaList.Add(hour + 2);
                        }
                        else
                            continue;

                        string analysisType = hour + "." + (hour + 1) + "." + (hour + 2);
                        RobotTypeList foundRobotType = item;
                        foundRobotType.AnalysisType = analysisType;
                        foundRobotTypleList.Add(foundRobotType);
                    }
                }

                List<RobotTypeList> tempList = foundRobotTypleList.ToList();
                // tempList.RemoveAll(a => a.Price > 50 || a.Price < -50);
                RobotTypeDataList = tempList.ToList();
            }

            #endregion

            ProgressBar = false;
            mTempRobotList = RobotTypeDataList.ToList();

            if (mSelectedRobotType == AlgoType.VirtualShort)
            {
                SourceZoneList = mTempRobotList.Select(a => a.SourceZone.Trim()).Distinct().OrderBy(j => j).ToList();
            }
            else if (mSelectedRobotType == AlgoType.ErcotBlock_B1)
            {
                SourceZoneList = mTempRobotList.Select(a => a.SourceZone.Trim()).Distinct().OrderBy(j => j).ToList();
                SinkZoneList = mTempRobotList.Select(a => a.SinkZone.Trim()).Distinct().OrderBy(j => j).ToList();
            }
            else if (mSelectedRobotType == AlgoType.ErcotBlock_B2)
            {
                SourceZoneList = mTempRobotList.Select(a => a.SourceZone.Trim()).Distinct().OrderBy(j => j).ToList();
                SinkZoneList = mTempRobotList.Select(a => a.SinkZone.Trim()).Distinct().OrderBy(j => j).ToList();
            }
            else if (mSelectedRobotType == AlgoType.ErcotSubmittedBidsAlgo)
            {
                SourceZoneList = mTempRobotList.Select(a => a.SourceZone.Trim()).Distinct().OrderBy(j => j).ToList();
                SinkZoneList = mTempRobotList.Select(a => a.SinkZone.Trim()).Distinct().OrderBy(j => j).ToList();
            }
            else if (mSelectedRobotType != AlgoType.VirtualBlock && mSelectedRobotType != AlgoType.ErcotBlock_B2)
            {
                if (mTempRobotList != null && mTempRobotList.Count > 0)
                {
                    SourceZoneList = mTempRobotList.Select(a => a.SourceZone.Trim()).Distinct().OrderBy(j => j).ToList();
                    SinkZoneList = mTempRobotList.Select(a => a.SinkZone.Trim()).Distinct().OrderBy(j => j).ToList();

                    SourceNodeTypeList = mTempRobotList.Select(a => a.SourceNodeType.Trim()).Distinct().OrderBy(j => j).ToList();
                    SinkNodeTypeList = mTempRobotList.Select(a => a.SinkNodeType.Trim()).Distinct().OrderBy(j => j).ToList();
                }
            }
            else
            {
                if (mTempRobotList != null && mTempRobotList.Count > 0)
                {
                    SourceZoneList = mTempRobotList.Select(a => a.SourceZone.Trim()).Distinct().OrderBy(j => j).ToList();
                    SourceNodeTypeList = mTempRobotList.Select(a => a.SourceNodeType.Trim()).Distinct().OrderBy(j => j).ToList();
                }
            }
        }
        /// <summary>
        /// Resets the robot list.
        /// </summary>
        private void ResetRobotList()
        {
            if (mTempRobotList != null && mTempRobotList.Count > 0)
            {
                SourceNodeTypeSelectedItem = null;
                SinkNodeTypeSelectedItem = null;
                SourceZoneSelectedItem = null;
                SinkZoneSelectedItem = null;
                MaxPriceFilter = null;
                MinPriceFilter = null;
                SumValueMaxFilter = null;
                SumValueMinFilter = null;
                RobotTypeDataList = mTempRobotList.ToList();
            }
        }
        /// <summary>
        /// Refreshes the robot list.
        /// </summary>
        private void RefreshRobotList()
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            List<RobotTypeList> tempList = new List<RobotTypeList>();
            try
            {
                if (SourceZoneSelectedItem != null && SourceZoneSelectedItem.Count != 0)
                {
                    if (mTempRobotList != null)
                    {
                        SourceZoneSelectedItem.ForEach(a =>
                        {
                            tempList.AddRange(mTempRobotList.Where(g => g.SourceZone == a));
                        });
                    }
                }
                if (SinkZoneSelectedItem != null && SinkZoneSelectedItem.Count != 0)
                {
                    if (mTempRobotList != null)
                    {
                        if (tempList.Count > 0)
                        {
                            List<RobotTypeList> tempNewList = new List<RobotTypeList>();
                            SinkZoneSelectedItem.ForEach(a =>
                            {
                                tempNewList.AddRange(tempList.Where(h => h.SinkZone.Equals(a)));
                            });
                            tempList = tempNewList.ToList();
                        }
                        else
                        {
                            SinkZoneSelectedItem.ForEach(a =>
                            {
                                tempList.AddRange(mTempRobotList.Where(g => g.SinkZone == a));
                            });
                        }
                    }
                }
                if (SourceNodeTypeSelectedItem != null && SourceNodeTypeSelectedItem.Count != 0)
                {
                    if (mTempRobotList != null)
                    {
                        if (tempList.Count > 0)
                        {
                            List<RobotTypeList> tempNewList = new List<RobotTypeList>();
                            SourceNodeTypeSelectedItem.ForEach(a =>
                            {
                                tempNewList.AddRange(tempList.Where(h => h.SourceNodeType.Equals(a)));
                            });
                            tempList = tempNewList.ToList();
                        }
                        else
                        {
                            SourceNodeTypeSelectedItem.ForEach(a =>
                            {
                                tempList.AddRange(mTempRobotList.Where(g => g.SourceNodeType == a));
                            });
                        }
                    }
                }
                if (SinkNodeTypeSelectedItem != null && SinkNodeTypeSelectedItem.Count != 0)
                {
                    if (mTempRobotList != null)
                    {
                        if (tempList.Count > 0)
                        {
                            List<RobotTypeList> tempNewList = new List<RobotTypeList>();
                            SinkNodeTypeSelectedItem.ForEach(a =>
                            {
                                tempNewList.AddRange(tempList.Where(h => h.SinkNodeType.Equals(a)));
                            });
                            tempList = tempNewList.ToList();
                        }
                        else
                        {
                            SinkNodeTypeSelectedItem.ForEach(a =>
                            {
                                tempList.AddRange(mTempRobotList.Where(g => g.SinkNodeType == a));
                            });
                        }
                    }
                }
                if (MinPriceFilter != null)
                {
                    if (tempList.Count > 0)
                    {
                        tempList = tempList.Where(a => a.Price >= MinPriceFilter).ToList();
                    }
                    else
                    {
                        tempList = mTempRobotList.Where(a => a.Price >= MinPriceFilter).ToList();
                    }
                }
                if (MaxPriceFilter != null)
                {
                    if (tempList.Count > 0)
                    {
                        tempList = tempList.Where(a => a.Price <= MaxPriceFilter).ToList();
                    }
                    else
                    {
                        tempList = mTempRobotList.Where(a => a.Price <= MaxPriceFilter).ToList();
                    }
                }
                if (SumValueMinFilter != null)
                {
                    if (tempList.Count > 0)
                    {
                        tempList = tempList.Where(a => a.SumValue >= SumValueMinFilter).ToList();
                    }
                    else
                    {
                        tempList = mTempRobotList.Where(a => a.SumValue >= SumValueMinFilter).ToList();
                    }
                }
                if (SumValueMaxFilter != null)
                {
                    if (tempList.Count > 0)
                    {
                        tempList = tempList.Where(a => a.SumValue <= SumValueMaxFilter).ToList();
                    }
                    else
                    {
                        tempList = mTempRobotList.Where(a => a.SumValue <= SumValueMaxFilter).ToList();
                    }
                }
                if (tempList.Count == 0)
                {
                    MessageBox.Show("Nothing to filter");
                    if (mTempRobotList != null)
                    {
                        RobotTypeDataList = mTempRobotList.ToList();
                    }
                }
                else
                {
                    RobotTypeDataList = tempList.ToList();
                }
            }
            catch
            {
                MessageBox.Show("Something went wrong. Couldn't filter the data");
            }
            finally
            {
                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }

        public void ExportExcel()
        {
            List<RobotTypeList> tempDataList = RobotTypeDataList.ToList();
            if (tempDataList.Count > 0)
            {
                ExportToExcelSpread<RobotTypeList, List<RobotTypeList>> objSource = new ExportToExcelSpread<RobotTypeList, List<RobotTypeList>>();
                List<RobotTypeList> itemListSource = new List<RobotTypeList>();
                ICollectionView viewSource = CollectionViewSource.GetDefaultView(tempDataList);
                foreach (var item in viewSource.SourceCollection)
                {
                    itemListSource.Add((RobotTypeList)item);
                }
                objSource.dataToPrint = itemListSource;
                objSource.GenerateReport(SelectedRobotType.ToString());
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Not Found !!!");
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the data.
        /// </summary>
        public void RefreshData()
        {
            if (refreshTask != null && !refreshTask.IsCompleted)
                return;

            if (!SelectedDate.HasValue)
            {
                selectedDate = DateTime.Today;
                RaisePropertyChanged("SelectedDate");
            }
            refreshTask = Task.Factory.StartNew(() =>
            {
                RefreshDataThread();
            });
        }
        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        //public override void Cleanup()
        //{
        //    // Clean up if needed

        //    base.Cleanup();
        //}
        /// <summary>
        /// Sets the source sinks.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        public void SetSourceSinks(List<Tuple<string, string>> sourceSinkList)
        {
            try
            {
                Vayu.LMPriceWindow.LmpForm.SetSourceSinks(sourceSinkList, 1);
            }
            catch { }
        }
        /// <summary>
        /// Shows the LMP graphs.
        /// </summary>
        /// <param name="day">The day.</param>
        public void ShowLMPGraphs(string day)
        {
            //Vayu.LMPriceWindow.LmpForm.OpenLmpGraphs(1, Vayu.LMPriceWindow.LmpForm.GetSourceSinks(), DateTime.Today, DateTime.Today);
        }
        /// <summary>
        /// Shows the node analyzer.
        /// </summary>
        public void ShowNodeAnalyzer()
        {
            // Vayu.LMPriceWindow.LmpForm.OpenLMPStatisticAnalyzer(1, Vayu.LMPriceWindow.LmpForm.GetSourceSinks());
        }
        /// <summary>
        /// Called when [load data by date].
        /// </summary>
        public void OnLoadDataByDate()
        {
            RefreshData();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Selects the nodes zones.
        /// </summary>
        /// <param name="isNode">if set to <c>true</c> [is node].</param>
        /// <param name="toSelect">if set to <c>true</c> [to select].</param>
        internal void SelectNodesZones(bool isNode, bool toSelect)
        {
            if (isNode)
            {
                if (toSelect)
                {
                    SinkNodeTypeSelectedItem = SinkNodeTypeList == null ? SinkNodeTypeList : SinkNodeTypeList.ToList();
                }
                else
                {
                    if (SinkNodeTypeSelectedItem != null)
                    {
                        SinkNodeTypeSelectedItem.Clear();
                    }
                }
            }
            else
            {
                if (toSelect)
                {
                    SourceNodeTypeSelectedItem = SourceNodeTypeList == null ? SourceNodeTypeList : SourceNodeTypeList.ToList();
                }
                else
                {
                    if (SourceNodeTypeSelectedItem != null)
                    {
                        SourceNodeTypeSelectedItem.Clear();
                    }
                }
            }
        }
        /// <summary>
        /// Selects the source sink.
        /// </summary>
        /// <param name="isSource">if set to <c>true</c> [is source].</param>
        /// <param name="isSelect">if set to <c>true</c> [is select].</param>
        internal void SelectSourceSink(bool isSource, bool isSelect)
        {
            if (isSource)
            {
                if (isSelect)
                {
                    if (SourceZoneList != null)
                    {
                        SourceZoneSelectedItem = SourceZoneList.ToList();
                    }
                }
                else
                {
                    if (SourceZoneSelectedItem != null)
                    {
                        SourceZoneSelectedItem.Clear();
                    }
                }
            }
            else
            {
                if (isSelect)
                {
                    if (SinkZoneList != null)
                    {
                        SinkZoneSelectedItem = SinkZoneList.ToList();
                    }
                    else
                    {
                        if (SinkZoneSelectedItem != null)
                        {
                            SinkZoneSelectedItem.Clear();
                        }
                    }
                }
            }
        }

        #endregion
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
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public enum AlgoType
    {
        /// <summary>
        /// The Ramp
        /// </summary>
        Ramp,
        /// <summary>
        /// The PeakOffPeak    
        /// </summary>
        PeakOffPeak,
        /// <summary>
        /// The Block
        /// </summary>
        Block_B1,
        /// <summary>
        /// The Block
        /// </summary>
        Block_B2,
        Block_B3,
        /// <summary>
        /// The virtual Block
        /// </summary>
        VirtualBlock,
        /// <summary>
        /// The uptos short
        /// </summary>
        UptosShort,
        /// <summary>
        /// The virtual short
        /// </summary>
        VirtualShort,
        /// <summary>
        /// The outage
        /// </summary>
        Outage,

        /// <summary>
        /// The Ercot B1
        /// </summary>
        ErcotBlock_B1,
        /// <summary>
        /// The Ercot B2 
        /// </summary>
        ErcotBlock_B2,

        PJMPositiveCorrelations,
        VirtualPath,

        ErcotPositiveCorrelations,
        PJMNegativeCorrelations,
        ErcotNegativeCorrelations,
        ErcotSubmittedBidsAlgo
    }
    /// <summary>
    /// 
    /// </summary>
    public struct SourceSink
    {
        /// <summary>
        /// The source
        /// </summary>
        public string Source;
        /// <summary>
        /// The sink
        /// </summary>
        public string Sink;
    }
    public class IsLesserConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = (value ?? "").ToString();
            double doubValue = 0;
            double.TryParse(strValue, out doubValue);
            if (doubValue < 0)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Activator.CreateInstance(targetType);
        }
    }
}
