using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Vayu.CommonControls;
using Vayu.ConstraintSensitivityAlgorithm.Model;
using Vayu.DBLibrary;

namespace Vayu.ConstraintSensitivityAlgorithm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string mUser = Environment.UserName;
        private readonly IDataService mDataService;

        #region Properties

        #region Relay Command Properties

        public DelegateCommand RunAddCommand { private set; get; }
        public DelegateCommand RunAddAllCommand { private set; get; }
        public DelegateCommand RunRemoveCommand { private set; get; }
        public DelegateCommand RunRemoveAllCommand { private set; get; }
        public DelegateCommand RunRunCommand { private set; get; }
        public DelegateCommand RunExportCommand { private set; get; }
        public DelegateCommand RunOpenLMPStatisticAnalyzer { private set; get; }

        #endregion

        public List<ConstraintContingency> SelectConstraintCongingencyList { get; set; }
        public List<ConstraintContingency> SelectAddedConstraintCongingencyList { get; set; }
        private string mSensitivity = "0.01";
        public string Sensitivity
        {
            get
            {
                return mSensitivity;
            }
            set
            {
                mSensitivity = value;
                RaisePropertyChanged("Sensitivity");
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
        private string mProductSelectedItem = "UPTOs";
        public string ProductSelectedItem
        {
            get
            {
                return mProductSelectedItem;
            }
            set
            {
                mProductSelectedItem = value;
                if (ProductSelectedItem.ToUpper().Contains("VIRTUAL"))
                {
                    SourceHeaderName = "Node Name";
                    SourceSensitivityHeaderName = "Node Sensitivity";
                }
                else
                {
                    SourceHeaderName = "Source";
                    SourceSensitivityHeaderName = "Source Sensitivity";
                }

                Sensitivity = mProductSelectedItem == "UPTOs" ? "0.01" : "0.1";
            }
        }
        private string sourceHeaderName = "Source";
        public string SourceHeaderName
        {
            get
            {
                return sourceHeaderName;
            }
            set
            {
                sourceHeaderName = value;
                RaisePropertyChanged("SourceHeaderName");
            }
        }

        private string sourceSensitivityHeaderName = "Source Sensitivity";
        public string SourceSensitivityHeaderName
        {
            get
            {
                return sourceSensitivityHeaderName;
            }
            set
            {
                sourceSensitivityHeaderName = value;
                RaisePropertyChanged("SourceSensitivityHeaderName");
            }
        }


        private List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> mVectorList;
        public List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> VectorList
        {
            get
            {
                return mVectorList;
            }
            set
            {
                mVectorList = value;
                RaisePropertyChanged("VectorList");
            }
        }

        private string mConstraintSelected;
        public string ConstraintSelected
        {
            get
            {
                return mConstraintSelected;
            }
            set
            {
                mConstraintSelected = value;
                RaisePropertyChanged("ConstraintSelected");
                if (value == null || value == "")
                {
                    Parallel.ForEach(ConstraintList, a =>
                    {
                        a.IsSelected = false;
                    });
                    return;
                }
                if (value != null)
                {
                    IEnumerable<ConstraintContingency> templist = ConstraintList.Where(a => a.Constraint.ToLower().Contains(value.ToLower()));
                    foreach (ConstraintContingency item in templist)
                    {
                        Parallel.ForEach(ConstraintList, a =>
                        {
                            if (a.Constraint == item.Constraint)
                            {
                                a.IsSelected = true;
                            }
                        });
                    }
                }
            }
        }
        private string mContigencySelected;
        public string ContigencySelected
        {
            get
            {
                return mContigencySelected;
            }
            set
            {
                mContigencySelected = value;
                RaisePropertyChanged("ContigencySelected");
                if (value == null || value == "")
                {
                    Parallel.ForEach(ConstraintList, a =>
                    {
                        a.IsSelected = false;
                    });
                    return;
                }
                if (value != null)
                {
                    IEnumerable<ConstraintContingency> templist = ConstraintList.Where(a => a.Contingency.ToLower().Contains(value.ToLower()));
                    foreach (ConstraintContingency item in templist)
                    {
                        Parallel.ForEach(ConstraintList, a =>
                        {
                            if (a.Contingency == item.Contingency)
                            {
                                a.IsSelected = true;
                            }
                        });
                    }
                }
            }
        }

        private List<ConstraintContingency> mConstraintList;
        public List<ConstraintContingency> ConstraintList
        {
            get
            {
                return mConstraintList;
            }
            set
            {
                mConstraintList = value;
                RaisePropertyChanged("ConstraintList");
                if (value != null && value.Count > 0)
                {
                    ConstraintListItems = ConstraintList.Select(a => a.Constraint).Distinct().ToList();
                    ContigencyListItems = ConstraintList.Select(a => a.Contingency).Distinct().ToList();
                }
            }
        }

        private List<ConstraintContingency> mAddedConstraintList;
        public List<ConstraintContingency> AddedConstraintList
        {
            get
            {
                return mAddedConstraintList;
            }
            set
            {
                mAddedConstraintList = value;
                RaisePropertyChanged("AddedConstraintList");
            }
        }

        private List<string> mConstraintListItems;
        public List<string> ConstraintListItems
        {
            get
            {
                return mConstraintListItems;
            }
            set
            {
                mConstraintListItems = value;
                RaisePropertyChanged("ConstraintListItems");
            }
        }
        private List<string> mContigencyListItems;
        private List<ConstraintContingency> mTempConstraintContingencyCache;
        public List<string> ContigencyListItems
        {
            get
            {
                return mContigencyListItems;
            }
            set
            {
                mContigencyListItems = value;
                RaisePropertyChanged("ContigencyListItems");
            }
        }
        private List<string> mFamilyListItems;
        public List<string> FamilyListItems
        {
            get
            {
                return mFamilyListItems;
            }
            set
            {
                mFamilyListItems = value;
                RaisePropertyChanged("FamilyListItems");
            }
        }
        private string mFamilySelectedItem;
        public string FamilySelectedItem
        {
            get
            {
                return mFamilySelectedItem;
            }
            set
            {
                mFamilySelectedItem = value;
                RaisePropertyChanged("FamilySelectedItem");
                SetConstraintContingency();
            }
        }

        private string mConstraintTextItem;

        public string ConstraintTextItem
        {
            get
            {
                return mConstraintTextItem;
            }
            set
            {
                mConstraintTextItem = value;
                RaisePropertyChanged("ConstraintTextItem");
            }
        }

        private Visibility mIsDataGridVisible;
        public Visibility IsDataGridVisible
        {
            get
            {
                return mIsDataGridVisible;
            }
            set
            {
                mIsDataGridVisible = value;
                RaisePropertyChanged("IsDataGridVisible");
            }
        }

        private string mSelectedMarket;
        /// <summary>
        /// Gets or sets the selected market.
        /// </summary>
        /// <value>
        /// The selected market.
        /// </value>
        public string SelectedMarket
        {
            get
            {
                return mSelectedMarket;
            }
            set
            {
                mSelectedMarket = value;
                // SetAccountList();
                SetConstraintContingency();
                RaisePropertyChanged("SelectedMarket");

            }
        }

        private List<string> mMarketList;

        public List<string> MarketList
        {
            get
            {
                return mMarketList;
            }
            set
            {
                mMarketList = value;
                RaisePropertyChanged("MarketList");
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>

        public MainWindowViewModel(IDataService dataService)
        {
            mDataService = dataService;
            RunAddCommand = new DelegateCommand(() => AddCommand());
            RunAddAllCommand = new DelegateCommand(() => AddAllCommand());
            RunRemoveCommand = new DelegateCommand(() => RemoveCommand());
            RunRemoveAllCommand = new DelegateCommand(() => RemoveAllCommand());
            RunRunCommand = new DelegateCommand(() => RunCommand());
            RunExportCommand = new DelegateCommand(() => RunExport());
            MarketList = new List<string> { "ERCOT" };
            SelectedMarket = "ERCOT";
            ProductList = new List<string>() { "UPTOs", "Virtual", "FTR" };
            ProductSelectedItem = "UPTOs";
            //SetConstraintContingency();
            FamilyListItems = DBAccess.GetFamilies();
        }

        private int mMarketid;
        public int MarketId
        {
            get { return mMarketid; }
            set
            {
                mMarketid = value;
                RaisePropertyChanged("MarketId");
            }
        }

        #region Private Methods
        private void SetConstraintContingency()
        {
            MarketId = SelectedMarket == "PJM" ? 1 : 9;
            mTempConstraintContingencyCache = mDataService.GetConstraintContingency(DateTime.Today.Date, MarketId);
            ConstraintList = null;
            if (mTempConstraintContingencyCache != null)
            {
                ConstraintList = mTempConstraintContingencyCache.ToList();
            }
            if (!string.IsNullOrEmpty(FamilySelectedItem))
            {
                List<ConstraintContingency> mTempList = ConstraintList.Where(a => a.Family == FamilySelectedItem).ToList();
                if (mTempList != null)
                {
                    ConstraintList = mTempList;
                }
            }
        }
        private void RunCommand()
        {
            if (AddedConstraintList == null)
            {
                return;
            }
            bool isUptos = ProductSelectedItem == "UPTOs";
            bool isFTRs = ProductSelectedItem == "FTR";
            MarketId = SelectedMarket == "PJM" ? 1 : 9;
            List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> vectorList = mDataService.GetVectors(AddedConstraintList, isUptos, MarketId);
            Dictionary<string, List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector>> InvalidSourceSinkHash = mDataService.GetInvalidSourceSink();
            List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> InvalidSourceList = InvalidSourceSinkHash["Source"];
            List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> InvalidSinkList = InvalidSourceSinkHash["Sink"];
            var SourceVectorList = vectorList.Where(x => !InvalidSourceList.Any(p => p.Source == x.Source));
            var SinkVectorList = vectorList.Where(x => !InvalidSinkList.Any(p => p.Sink == x.Sink));
            List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> sendVectorList = new List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector>();
            double sensitivity = Sensitivity == null ? 0.01 : double.Parse(Sensitivity);
            if (vectorList.Count > 50000)
            {
                MessageBox.Show("Too many Paths");
                return;
            }
            List<SourceSink> mSourceSinkHash = mDataService.GetPath(MarketId);
            List<string> validFtrNodeList = new List<string>();
            if (isFTRs)
                validFtrNodeList = DBAccess.GetLatestValidFTRNodes();
            for (int i = 0; i < vectorList.Count; i++)
            //   Parallel.For(0, vectorList.Count, new ParallelOptions { MaxDegreeOfParallelism = 20 }, i =>
            {
                if (isUptos)
                {
                    for (int j = 0; j < vectorList.Count; j++)
                    {
                        if (i != j)
                        {

                            double diff = vectorList[j].SourceSensitivity - vectorList[i].SourceSensitivity;
                            if (Math.Abs(diff) > sensitivity)
                            {
                                SourceSink mSourceSink = new SourceSink();
                                mSourceSink.Source = vectorList[i].Source;
                                mSourceSink.Sink = vectorList[j].Source;
                                Vayu.ConstraintSensitivityAlgorithm.Model.Vector sendVector = new Vayu.ConstraintSensitivityAlgorithm.Model.Vector();
                                sendVector.Source = vectorList[i].Source;
                                sendVector.SourceSensitivity = vectorList[i].SourceSensitivity;
                                sendVector.Sink = vectorList[j].Source;
                                sendVector.SinkSensitivity = vectorList[j].SourceSensitivity;
                                sendVector.Sensitivity = diff;
                                //if ((mSourceSinkHash.Exists(x => x.Source == vectorList[i].Source)) && (mSourceSinkHash.Exists(x => x.Sink == vectorList[j].Source)))
                                //{
                                sendVectorList.Add(sendVector);
                                //}
                            }
                        }
                    }
                }

                else
                {
                    if (!isFTRs)
                    {
                        if (Math.Abs(vectorList[i].SourceSensitivity) > sensitivity)
                        {
                            Vayu.ConstraintSensitivityAlgorithm.Model.Vector sendVector = new Vayu.ConstraintSensitivityAlgorithm.Model.Vector();
                            sendVector.Source = vectorList[i].Source;
                            sendVector.SourceSensitivity = vectorList[i].SourceSensitivity;
                            sendVectorList.Add(sendVector);
                        }
                    }
                }
            }
            // });
            if (isFTRs)
            {
                //  Task.Factory.StartNew(() => GetFtrs(vectorList, validFtrNodeList, sensitivity, sendVectorList));
                GetFtrs(vectorList, validFtrNodeList, sensitivity, sendVectorList);
            }
            VectorList = null;
            VectorList = sendVectorList;
        }



        private void GetFtrs(List<Model.Vector> vectorList, List<string> validFtrNodeList, double sensitivity, List<Model.Vector> sendVectorList)
        {
            List<Model.Vector> vectorFTRList = new List<Model.Vector>();
            foreach (string validFTRNode in validFtrNodeList)
            {
                Model.Vector ftrVector = vectorList.Find(a => a.Source == validFTRNode);
                vectorFTRList.Add(ftrVector);
            }

            // Parallel.For(0, vectorList.Count, new ParallelOptions { MaxDegreeOfParallelism = 20 }, (j, state) =>
            for (int i = 0; i < vectorFTRList.Count; i++)
            {
                for (int j = 0; j < vectorFTRList.Count; j++)
                {
                    if (i != j)
                    {

                        if (vectorFTRList[i] == null)
                        {
                            // state.Break();
                            break;
                        }
                        if (vectorFTRList[j] == null || vectorFTRList[j].Source == null)
                        {
                            continue;
                        }
                        else
                        {
                            double diff = vectorFTRList[j].SourceSensitivity - vectorFTRList[i].SourceSensitivity;
                            if (Math.Abs(diff) > sensitivity)
                            {
                                if (vectorFTRList[j].Source == null)
                                {

                                }
                                SourceSink mSourceSink = new SourceSink();
                                mSourceSink.Source = vectorFTRList[i].Source;
                                mSourceSink.Sink = vectorFTRList[j].Source;
                                Vayu.ConstraintSensitivityAlgorithm.Model.Vector sendVector = new Vayu.ConstraintSensitivityAlgorithm.Model.Vector();
                                sendVector.Source = vectorFTRList[i].Source;
                                sendVector.SourceSensitivity = vectorFTRList[i].SourceSensitivity;
                                sendVector.Sink = vectorFTRList[j].Source;
                                sendVector.SinkSensitivity = vectorFTRList[j].SourceSensitivity;
                                sendVector.Sensitivity = diff;
                                sendVectorList.Add(sendVector);

                            }
                        }
                    }
                }
            }
            // });
        }


        private void AddCommand()
        {
            List<int> constraintIDList = new List<int>();
            List<ConstraintContingency> constraintContingecyList = new List<ConstraintContingency>();
            try
            {

                if (AddedConstraintList != null)
                {
                    foreach (ConstraintContingency constraintContingency in AddedConstraintList)
                    {
                        ConstraintContingency constraint = new ConstraintContingency();
                        constraint.ID = constraintContingency.ID;
                        constraint.Constraint = constraintContingency.Constraint;
                        constraintContingency.Contingency = constraintContingency.Contingency;
                        constraintContingency.Family = constraintContingency.Family;
                        constraintIDList.Add(constraintContingency.ID);
                        constraintContingecyList.Add(constraint);
                    }
                }
                foreach (ConstraintContingency constraintContingency in SelectConstraintCongingencyList)
                {
                    if (!constraintIDList.Contains(constraintContingency.ID))
                    {
                        ConstraintContingency constraint = new ConstraintContingency();
                        constraint.ID = constraintContingency.ID;
                        constraint.Constraint = constraintContingency.Constraint;
                        constraint.Contingency = constraintContingency.Contingency;
                        constraint.Family = constraintContingency.Family;
                        constraintIDList.Add(constraintContingency.ID);
                        constraintContingecyList.Add(constraint);
                    }
                }
            }
            catch
            {
            }
            AddedConstraintList = null;
            AddedConstraintList = constraintContingecyList;
        }
        private void AddAllCommand()
        {
            AddedConstraintList = null;
            AddedConstraintList = ConstraintList;
        }
        private void RemoveCommand()
        {
            List<int> constraintIDList = new List<int>();
            List<ConstraintContingency> constraintContingecyList = new List<ConstraintContingency>();
            if (SelectAddedConstraintCongingencyList != null)
            {
                if (SelectAddedConstraintCongingencyList.Count > 0)
                {
                    try
                    {
                        foreach (ConstraintContingency constraintContingency in SelectAddedConstraintCongingencyList)
                        {
                            constraintIDList.Add(constraintContingency.ID);
                        }
                        if (AddedConstraintList != null)
                        {
                            foreach (ConstraintContingency constraintContingency in AddedConstraintList)
                            {
                                if (!constraintIDList.Contains(constraintContingency.ID))
                                {
                                    ConstraintContingency constraint = new ConstraintContingency();
                                    constraint.Constraint = constraintContingency.Constraint;
                                    constraint.Contingency = constraintContingency.Contingency;
                                    constraint.Family = constraintContingency.Family;
                                    constraint.ID = constraintContingency.ID;
                                    constraintContingecyList.Add(constraint);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    AddedConstraintList = null;
                    AddedConstraintList = constraintContingecyList;
                }
            }
        }
        private void RemoveAllCommand()
        {
            AddedConstraintList = null;
        }
        private void RunExport()
        {
            if (VectorList != null)
            {
                ExportToExcelNodeSpread<Vayu.ConstraintSensitivityAlgorithm.Model.Vector, List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector>> s =
                    new ExportToExcelNodeSpread<Model.Vector, List<Model.Vector>>();
                List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector> itemlist = new List<Vayu.ConstraintSensitivityAlgorithm.Model.Vector>();
                ICollectionView view = CollectionViewSource.GetDefaultView(VectorList);
                foreach (var item in view.SourceCollection)
                {
                    itemlist.Add((Vayu.ConstraintSensitivityAlgorithm.Model.Vector)item);
                }
                s.dataToPrint = itemlist;
                s.GenerateReport();
            }
        }

        #endregion

        #region Public Methods

        public void SetSourceSinks(List<Tuple<string, string>> sourceSinkList)
        {
            try
            {
                Vayu.LMPriceWindow.LmpForm.SetSourceSinks(sourceSinkList, 9);
            }
            catch (Exception ex) { }
        }
        public void ShowLMPGraphs(string day)
        {
            Vayu.LMPriceWindow.LmpForm.OpenLmpGraphs(9, Vayu.LMPriceWindow.LmpForm.GetSourceSinks(), DateTime.Today, DateTime.Today);
        }
        public void ShowNodeAnalyzer()
        {
            Vayu.LMPriceWindow.LmpForm.OpenLMPStatisticAnalyzer(9, Vayu.LMPriceWindow.LmpForm.GetSourceSinks());
        }
        public void OpenOutageConstraint(string constraintText)
        {
            ConstraintTextItem = constraintText;
        }

        #endregion


    }
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        private BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((value ?? "").ToString().ToLower().Contains("virtual"))
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter.ConvertBack(value, targetType, parameter, culture) as bool?;
            return result == true ? false : true;
        }
    }
}
