using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Vayu.ConstraintExposure.Model;
using Vayu.DBLibrary;

namespace Vayu.ConstraintExposure.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Constant Variables

        /// <summary>
        /// Up tos
        /// </summary>
        const string UPTos = "UPTO";
        /// <summary>
        /// The nodal
        /// </summary>
        const string Nodal = "Nodal";
        /// <summary>
        /// The dollars
        /// </summary>
        const string Dollars = "Dollars";
        /// <summary>
        /// The inc
        /// </summary>
        const string INC = "INC";
        /// <summary>
        /// The virtuals
        /// </summary>
        const string Virtuals = "VIRTUAL";
        /// <summary>
        /// The path
        /// </summary>
        const string Path = "Path";
        /// <summary>
        /// The decimal
        /// </summary>
        const string DEC = "DEC";
        /// <summary>
        /// The m ws
        /// </summary>
        const string MWs = "MWs";

        #endregion

        #region Private Variables

        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;
        /// <summary>
        /// The m constraints list
        /// </summary>
        private List<Constraints> mConstraintsList;
        /// <summary>
        /// The source nodal value
        /// </summary>
        private PricingNode sourceNodalValue;
        /// <summary>
        /// The source data
        /// </summary>
        private PricingNode sourceData;
        /// <summary>
        /// The sink data
        /// </summary>
        private PricingNode sinkData;
        /// <summary>
        /// The m is swap
        /// </summary>
        private bool mIsSwap = false;
        /// <summary>
        /// The m fill source sink hash
        /// </summary>
        private Dictionary<string, SourceSinkData> mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
        /// <summary>
        /// The LST constraint hash
        /// </summary>
        private Dictionary<string, List<Constraints>> lstConstraintHash = new Dictionary<string, List<Constraints>>();
        /// <summary>
        /// The pnode identifier hash
        /// </summary>
        Dictionary<int, List<string>> pnodeIdHash = new Dictionary<int, List<string>>();

        #endregion

        #region Private Variables

        /// <summary>
        /// The lockobj
        /// </summary>
        public static readonly object lockobj = new object();
        /// <summary>
        /// The m sort order
        /// </summary>
        public bool mSortOrder = false;

        #endregion

        #region Properties

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
        private string mSelectedMarket;
        public string SelectedMarket
        {
            get
            {
                return mSelectedMarket;
            }
            set
            {
                mSelectedMarket = value;
                RaisePropertyChanged("SelectedMarket");
                if (SelectedMode == UPTos)
                    SetSourceSink();
                else
                    SetVirtual();

                ConstraintsList = null;
                SourceSinkList = null;
                mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
            }
        }

        private string mSelectedRTDA;
        public string SelectedRTDA
        {
            get
            {
                return mSelectedRTDA;
            }
            set
            {
                mSelectedRTDA = value;
                RaisePropertyChanged("SelectedRTDA");

            }
        }


        /// <summary>
        /// The m is go enabled
        /// </summary>
        private bool mIsGoEnabled;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is go enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is go enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsGoEnabled
        {
            get
            {
                return mIsGoEnabled;
            }
            set
            {
                mIsGoEnabled = value;
                RaisePropertyChanged("IsGoEnabled");
            }
        }
        /// <summary>
        /// The m visible up to true
        /// </summary>
        private bool mVisibleUpToTrue = true;
        /// <summary>
        /// Gets or sets a value indicating whether [visible up to true].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [visible up to true]; otherwise, <c>false</c>.
        /// </value>
        public bool VisibleUpToTrue
        {
            get
            {
                return mVisibleUpToTrue;
            }
            set
            {
                mVisibleUpToTrue = value;
                RaisePropertyChanged("VisibleUpToTrue");
            }
        }
        /// <summary>
        /// The m visible nodal
        /// </summary>
        private bool mVisibleNodal = true;
        /// <summary>
        /// Gets or sets a value indicating whether [visible nodal].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [visible nodal]; otherwise, <c>false</c>.
        /// </value>
        public bool VisibleNodal
        {
            get
            {
                return mVisibleNodal;
            }
            set
            {
                mVisibleNodal = value;
                RaisePropertyChanged("VisibleNodal");
            }
        }
        /// <summary>
        /// The m visible nodal group box
        /// </summary>
        private bool mVisibleNodalGroupBox = true;
        /// <summary>
        /// Gets or sets a value indicating whether [visible nodal group box].
        /// </summary>
        /// <value>
        /// <c>true</c> if [visible nodal group box]; otherwise, <c>false</c>.
        /// </value>
        public bool VisibleNodalGroupBox
        {
            get
            {
                return mVisibleNodalGroupBox;
            }
            set
            {
                mVisibleNodalGroupBox = value;
                RaisePropertyChanged("VisibleNodalGroupBox");
            }
        }
        /// <summary>
        /// The m visible nodal label
        /// </summary>
        private bool mVisibleNodalLabel = true;
        /// <summary>
        /// Gets or sets a value indicating whether [visible nodal label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [visible nodal label]; otherwise, <c>false</c>.
        /// </value>
        public bool VisibleNodalLabel
        {
            get
            {
                return mVisibleNodalLabel;
            }
            set
            {
                mVisibleNodalLabel = value;
                RaisePropertyChanged("VisibleNodalLabel");
            }
        }
        /// <summary>
        /// The m visible nodal combo
        /// </summary>
        private bool mVisibleNodalCombo = true;
        /// <summary>
        /// Gets or sets a value indicating whether [visible nodal combo].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [visible nodal combo]; otherwise, <c>false</c>.
        /// </value>
        public bool VisibleNodalCombo
        {
            get
            {
                return mVisibleNodalCombo;
            }
            set
            {
                mVisibleNodalCombo = value;
                RaisePropertyChanged("VisibleNodalCombo");
            }
        }
        /// <summary>
        /// The m visible path
        /// </summary>
        private bool mVisiblePath = true;
        /// <summary>
        /// Gets or sets a value indicating whether [visible path].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [visible path]; otherwise, <c>false</c>.
        /// </value>
        public bool VisiblePath
        {
            get
            {
                return mVisiblePath;
            }
            set
            {
                mVisiblePath = value;
                RaisePropertyChanged("VisiblePath");
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
        /// The m selected exposure item
        /// </summary>
        private Constraints mSelectedExposureItem;
        /// <summary>
        /// Gets or sets the selected exposure item.
        /// </summary>
        /// <value>
        /// The selected exposure item.
        /// </value>
        public Constraints SelectedExposureItem
        {
            get
            {
                return mSelectedExposureItem;
            }
            set
            {
                mSelectedExposureItem = value;
                RaisePropertyChanged("SelectedExposureItem");
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
        /// Gets or sets the source sink data selected.
        /// </summary>
        /// <value>
        /// The source sink data selected.
        /// </value>
        public SourceSinkData SourceSinkDataSelected
        {
            get
            {
                return mSrcSnkDataSelected;
            }
            set
            {
                try
                {
                    if (mSrcSnkDataSelected == null || !mSrcSnkDataSelected.Equals(value))
                    {
                        mSrcSnkDataSelected = value;
                        string hashKey = "";
                        if (mSrcSnkDataSelected != null)
                        {
                            hashKey = mSrcSnkDataSelected.Source.NodeKey.ToString() + ":" + mSrcSnkDataSelected.Sink.NodeKey.ToString();
                        }
                        if (!lstConstraintHash.ContainsKey(hashKey))
                        {

                            RetrieveFetchData(hashKey);
                            //lstConstraintHash.Add(hashKey, ConstraintsList);
                        }
                        else
                        {
                            ConstraintsList = lstConstraintHash[hashKey];
                        }

                        RaisePropertyChanged("SourceSinkDataSelected");

                    }
                }
                catch (Exception ex)
                {
                    if (SourceSinkList == null)
                    {
                        ConstraintsList = null;
                    }
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
        /// The m source node combo selected item
        /// </summary>
        private PricingNode mSourceNodeComboSelectedItem;
        /// <summary>
        /// Gets or sets the source node combo selected item.
        /// </summary>
        /// <value>
        /// The source node combo selected item.
        /// </value>
        public PricingNode SourceNodeComboSelectedItem
        {
            get
            {
                return mSourceNodeComboSelectedItem;
            }
            set
            {
                mSourceNodeComboSelectedItem = value;
                RaisePropertyChanged("SourceNodeComboSelectedItem");

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
        /// The m look back checked
        /// </summary>
        private bool mLookBackChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [look back checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [look back checked]; otherwise, <c>false</c>.
        /// </value>
        public bool LookBackChecked
        {
            get
            {
                return mLookBackChecked;
            }
            set
            {
                mLookBackChecked = value;
                RaisePropertyChanged("LookBackChecked");
            }
        }

        /// <summary>
        /// The m family checked
        /// </summary>
        private bool mFamilyChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [family checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [family checked]; otherwise, <c>false</c>.
        /// </value>
        public bool FamilyChecked
        {
            get
            {
                return mFamilyChecked;
            }
            set
            {
                mFamilyChecked = value;
                RaisePropertyChanged("FamilyChecked");
                if (FamilyChecked == true)
                {
                    SetFamilies();
                }
            }
        }

        /// <summary>
        /// The m best checked
        /// </summary>
        private bool mBestChecked = false;
        /// <summary>
        /// Gets or sets a value indicating whether [best checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [best checked]; otherwise, <c>false</c>.
        /// </value>
        public bool BestChecked
        {
            get
            {
                return mBestChecked;
            }
            set
            {
                mBestChecked = value;
                RaisePropertyChanged("BestChecked");
            }
        }
        /// <summary>
        /// The m start checked
        /// </summary>
        private bool mStartChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [start checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start checked]; otherwise, <c>false</c>.
        /// </value>
        public bool StartChecked
        {
            get
            {
                return mStartChecked;
            }
            set
            {
                mStartChecked = value;
                //RetrieveFetchDataCommand();
                RaisePropertyChanged("StartChecked");
            }
        }

        /// <summary>
        /// The m start date enabled
        /// </summary>
        private bool mStartDateEnabled;
        /// <summary>
        /// Gets or sets a value indicating whether [start date enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start date enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool StartDateEnabled
        {
            get
            {
                return mStartDateEnabled;
            }
            set
            {
                mStartDateEnabled = value;
                RaisePropertyChanged("StartDateEnabled");
            }
        }

        /// <summary>
        /// The m shadow price value
        /// </summary>
        private string mShadowPriceValue = "500";
        /// <summary>
        /// Gets or sets the shadow price value.
        /// </summary>
        /// <value>
        /// The shadow price value.
        /// </value>
        public string ShadowPriceValue
        {
            get
            {
                return mShadowPriceValue;
            }
            set
            {
                mShadowPriceValue = value;
                RaisePropertyChanged("ShadowPriceValue");
            }
        }

        /// <summary>
        /// The m outage checked
        /// </summary>
        private bool mOutageChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [outage checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [outage checked]; otherwise, <c>false</c>.
        /// </value>
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
                    StartDateEnabled = true;
                }
                else
                {
                    StartDateEnabled = false;
                }
                RaisePropertyChanged("OutageChecked");
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
            get
            {
                return mConstraintChecked;
            }
            set
            {
                mConstraintChecked = value;
                RaisePropertyChanged("ConstraintChecked");
            }
        }

        /// <summary>
        /// The m days value
        /// </summary>
        private string mDaysVal = "";
        /// <summary>
        /// Gets or sets the days value.
        /// </summary>
        /// <value>
        /// The days value.
        /// </value>
        public string DaysVal
        {
            get
            {
                return mDaysVal;
            }
            set
            {
                mDaysVal = value;
                RaisePropertyChanged("DaysVal");
            }
        }

        /// <summary>
        /// The m rt settle selected
        /// </summary>
        private DateTime mRTSettleSelected;
        /// <summary>
        /// Gets or sets the rt settle selected.
        /// </summary>
        /// <value>
        /// The rt settle selected.
        /// </value>
        public DateTime RTSettleSelected
        {
            get
            {
                return mRTSettleSelected;
            }
            set
            {
                mRTSettleSelected = value;
                RaisePropertyChanged("RTSettleSelected");
            }
        }

        /// <summary>
        /// The m da settle selected
        /// </summary>
        private DateTime mDASettleSelected;
        /// <summary>
        /// Gets or sets the da settle selected.
        /// </summary>
        /// <value>
        /// The da settle selected.
        /// </value>
        public DateTime DASettleSelected
        {
            get
            {
                return mDASettleSelected;
            }
            set
            {
                mDASettleSelected = value;
                RaisePropertyChanged("DASettleSelected");
            }
        }

        /// <summary>
        /// The mode list
        /// </summary>
        private List<string> modeList;
        /// <summary>
        /// Gets or sets the mode list.
        /// </summary>
        /// <value>
        /// The mode list.
        /// </value>
        public List<string> ModeList
        {
            get { return modeList; }
            set
            {
                modeList = value;
                RaisePropertyChanged("ModeList");
            }
        }

        /// <summary>
        /// The selected mode
        /// </summary>
        private string selectedMode;
        /// <summary>
        /// Gets or sets the selected mode.
        /// </summary>
        /// <value>
        /// The selected mode.
        /// </value>
        public string SelectedMode
        {
            get { return selectedMode; }
            set
            {
                selectedMode = value;
                RaisePropertyChanged("SelectedMode");

                if (SelectedMode == UPTos)
                    SetSourceSink();
                else
                    SetVirtual();

                ConstraintsList = null;
                SourceSinkList = null;
                mFillSourceSinkHash = new Dictionary<string, SourceSinkData>();
            }
        }

        /// <summary>
        /// The virtual type list
        /// </summary>
        private List<string> virtualTypeList;
        /// <summary>
        /// Gets or sets the virtual type list.
        /// </summary>
        /// <value>
        /// The virtual type list.
        /// </value>
        public List<string> VirtualTypeList
        {
            get { return virtualTypeList; }
            set { virtualTypeList = value; RaisePropertyChanged("VirtualTypeList"); }
        }

        /// <summary>
        /// The selected virtual
        /// </summary>
        private string selectedVirtual;
        /// <summary>
        /// Gets or sets the selected virtual.
        /// </summary>
        /// <value>
        /// The selected virtual.
        /// </value>
        public string SelectedVirtual
        {
            get { return selectedVirtual; }
            set { selectedVirtual = value; RaisePropertyChanged("SelectedVirtual"); }
        }

        /// <summary>
        /// The capture list
        /// </summary>
        private List<string> captureList;
        /// <summary>
        /// Gets or sets the capture list.
        /// </summary>
        /// <value>
        /// The capture list.
        /// </value>
        public List<string> CaptureList
        {
            get { return captureList; }
            set { captureList = value; RaisePropertyChanged("CaptureList"); }
        }

        /// <summary>
        /// The selected capture
        /// </summary>
        private string selectedCapture;
        /// <summary>
        /// Gets or sets the selected capture.
        /// </summary>
        /// <value>
        /// The selected capture.
        /// </value>
        public string SelectedCapture
        {
            get { return selectedCapture; }
            set { selectedCapture = value; RaisePropertyChanged("SelectedCapture"); lstConstraintHash = new Dictionary<string, List<Constraints>>(); }
        }
        /// <summary>
        /// The selected inc decimal
        /// </summary>
        private string selectedIncDec;
        /// <summary>
        /// Gets or sets the selected inc decimal.
        /// </summary>
        /// <value>
        /// The selected inc decimal.
        /// </value>
        public string SelectedIncDec
        {
            get { return selectedIncDec; }
            set { selectedIncDec = value; RaisePropertyChanged("SelectedIncDec"); }
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
        /// The m inc decimal list
        /// </summary>
        private List<string> mIncDecList;

        /// <summary>
        /// Gets or sets the inc decimal list.
        /// </summary>
        /// <value>
        /// The inc decimal list.
        /// </value>
        public List<string> IncDecList
        {
            get
            {
                return mIncDecList;
            }
            set
            {
                mIncDecList = value;
                RaisePropertyChanged("IncDecList");
            }
        }


        /// <summary>
        /// The RT or DA
        /// </summary>
        private List<string> mRTDAList;

        /// <summary>
        /// Gets or sets the inc decimal list.
        /// </summary>
        /// <value>
        /// The inc decimal list.
        /// </value>
        public List<string> RTDAList
        {
            get
            {
                return mRTDAList;
            }
            set
            {
                mRTDAList = value;
                RaisePropertyChanged("RTDAList");
            }
        }


        /// <summary>
        /// Gets a value indicating whether [uptos checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [uptos checked]; otherwise, <c>false</c>.
        /// </value>
        public bool UptosChecked
        {
            get
            {
                return (SelectedMode == UPTos);
            }
        }
        /// <summary>
        /// Gets a value indicating whether [virtuals checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [virtuals checked]; otherwise, <c>false</c>.
        /// </value>
        public bool VirtualsChecked
        {
            get
            {
                return (SelectedMode == Virtuals);
            }
        }
        /// <summary>
        /// Gets a value indicating whether [nodal checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [nodal checked]; otherwise, <c>false</c>.
        /// </value>
        public bool NodalChecked
        {
            get
            {
                return (SelectedVirtual == Nodal);
            }
        }
        /// <summary>
        /// Gets a value indicating whether [path checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [path checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PathChecked
        {
            get
            {
                return (SelectedVirtual == Path);
            }
        }
        /// <summary>
        /// Gets a value indicating whether [mw checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [mw checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MWChecked
        {
            get
            {
                return (SelectedCapture == MWs);
            }
        }
        /// <summary>
        /// Gets a value indicating whether [dollars checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dollars checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DollarsChecked
        {
            get
            {
                return (SelectedCapture == Dollars);
            }
        }
        /// <summary>
        /// Gets a value indicating whether [inc checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [inc checked]; otherwise, <c>false</c>.
        /// </value>
        public bool IncChecked
        {
            get
            {
                return (SelectedIncDec == INC);
            }
        }
        /// <summary>
        /// Gets a value indicating whether [decimal checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [decimal checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DecChecked
        {
            get
            {
                return (SelectedIncDec == DEC);
            }
        }
        private DateTime stThroDADate = DateTime.Today;
        public DateTime ThroDADate
        {
            get { return stThroDADate; }
            set
            {
                if (value != null && value < DASettleSelected)
                {
                    System.Windows.MessageBox.Show("thro' date should be greater than from date");
                }
                else
                    stThroDADate = value;

                RaisePropertyChanged("ThroDADate");
            }
        }
        private DateTime stThroRTDate = DateTime.Today;
        public DateTime ThroRTDate
        {
            get { return stThroRTDate; }
            set
            {
                if (value != null && value < RTSettleSelected)
                {
                    System.Windows.MessageBox.Show("thro' date should be greater than from date");
                }
                else
                    stThroRTDate = value;

                RaisePropertyChanged("ThroRTDate");
            }
        }

        private bool stDateRangeRTCheckBoxChecked = false;
        public bool DateRangeRTCheckBoxChecked
        {
            get { return stDateRangeRTCheckBoxChecked; }
            set
            {
                stDateRangeRTCheckBoxChecked = value;
                if (RTSettleSelected > ThroRTDate)
                {
                    //System.Windows.MessageBox.Show("Resetting the from date");
                    RTSettleSelected = ThroRTDate.AddDays(-1);
                }
                RaisePropertyChanged("DateRangeRTCheckBoxChecked");
            }
        }
        private bool stDateRangeDACheckBoxChecked = false;
        public bool DateRangeDACheckBoxChecked
        {
            get { return stDateRangeDACheckBoxChecked; }
            set
            {
                stDateRangeDACheckBoxChecked = value;
                if (DASettleSelected > ThroDADate)
                {
                    //System.Windows.MessageBox.Show("Resetting the from date");
                    DASettleSelected = ThroDADate.AddDays(-1);
                }
                RaisePropertyChanged("DateRangeDACheckBoxChecked");
            }
        }


        #region Relay Command Properties

        /// <summary>
        /// Gets or sets the run retrieve fetch data command.
        /// </summary>
        /// <value>
        /// The run retrieve fetch data command.
        /// </value>
        public DelegateCommand RunRetrieveFetchDataCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run paste command.
        /// </summary>
        /// <value>
        /// The run paste command.
        /// </value>
        public DelegateCommand RunPasteCommand { private set; get; }
        /// <summary>
        /// Gets or sets the add source sink command.
        /// </summary>
        /// <value>
        /// The add source sink command.
        /// </value>
        public DelegateCommand AddSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the remove source sink command.
        /// </summary>
        /// <value>
        /// The remove source sink command.
        /// </value>
        public DelegateCommand RemoveSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the swap source sink command.
        /// </summary>
        /// <value>
        /// The swap source sink command.
        /// </value>
        public DelegateCommand SwapSourceSinkCommand { private set; get; }
        /// <summary>
        /// Gets or sets the run historical constraints command.
        /// </summary>
        /// <value>
        /// The run historical constraints command.
        /// </value>
        public DelegateCommand RunHistoricalConstraintsCommand { private set; get; }

        #endregion

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {
            mConstraintsList = new List<Constraints>();
            mDataService = dataService;
            mDataService.loadDBCommands();
            FamilyList = DBAccess.GetERCOTFamilies("RT");
            DASettleSelected = DateTime.Today;
            RTSettleSelected = DateTime.Today;
            RunRetrieveFetchDataCommand = new DelegateCommand(RetrieveFetchDataCommand);
            RunPasteCommand = new DelegateCommand(() => Paste());
            AddSourceSinkCommand = new DelegateCommand(() => AddSourceSink());
            RemoveSourceSinkCommand = new DelegateCommand(() => RemoveSourceSink());
            SwapSourceSinkCommand = new DelegateCommand(() => SwapSourceSink());
            RunHistoricalConstraintsCommand = new DelegateCommand(() => ShowHistoricalConstraints());
            IsGoEnabled = true;
            VirtualTypeList = new List<string>() { Nodal, Path };
            CaptureList = new List<string>() { MWs, Dollars };
            IncDecList = new List<string>() { INC, DEC };
            SelectedVirtual = "";
            SelectedCapture = Dollars;
            SelectedIncDec = "";
            MarketList = new List<string>() { "ERCOT" };
            SelectedMarket = "ERCOT";
            SelectedMode = UPTos;
            ModeList = new List<string>() { UPTos };
            RTDAList = new List<string>() { "RT", "DA" };
            SelectedRTDA = "RT";

        }
        #region Private Methods

        /// <summary>
        /// Used to Paste the data into controls.
        /// </summary>
        private void Paste()
        {
            try
            {
                List<PasteHelp> lstDataHelper = new List<PasteHelp>();
                if (SelectedMode == "VIRTUAL" && SelectedVirtual == "Nodal")
                {
                    string ivData = SourceNodeComboSelectedItem.NodeName;
                    if (ivData == null)
                    {
                        return;
                    }
                    string[] rowData = ivData.Split('\n');
                    List<string> mSourceList = new List<string>();
                    foreach (string rowItem in rowData)
                    {
                        if (rowItem != string.Empty)
                        {
                            string[] cellData = rowItem.Split('\t');
                            if (cellData.Length == 1)
                            {
                                lstDataHelper.Add(new PasteHelp
                                {
                                    Source = cellData[0].Replace("\r", ""),
                                    Sink = ""

                                });
                            }
                        }
                        if (lstDataHelper.Count == 0)
                            break;
                    }
                    try
                    {
                        if (lstDataHelper.Count > 0)
                        {
                            PasteSourceSinkData(lstDataHelper[0], "VIRTUAL");
                        }
                        else
                        {
                            MessageBox.Show("No data to paste");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                else
                {
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
                                    Source = cellData[0],
                                    Sink = cellData[1].Replace("\r", ""),

                                });
                            }
                        }
                        if (lstDataHelper.Count == 1)
                            break;
                    }
                    try
                    {
                        if (lstDataHelper.Count > 0)
                        {
                            PasteSourceSinkData(lstDataHelper[0], "UPTO");
                        }
                        else
                        {
                            MessageBox.Show("No data to paste");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(System.Reflection.MethodInfo.GetCurrentMethod() + "\t" + ex.Message);
            }
        }
        /// <summary>
        /// Adds the source sink.
        /// </summary>
        private void AddSourceSink()
        {
            if (SourceComboSelectedItem != null && SinkComboSelectedItem != null)
            {
                if (SourceComboSelectedItem != null && (!UptosChecked || SinkComboSelectedItem != null))
                {
                    SourceSinkData sourceSinkData = new SourceSinkData();
                    sourceSinkData.Source = SourceComboSelectedItem;
                    sourceSinkData.Sink = SinkComboSelectedItem;
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
            }
            if (selectedMode == "VIRTUAL" && SourceComboSelectedItem != null)
            {
                SourceSinkData sourceSinkData = new SourceSinkData();
                sourceSinkData.Source = SourceComboSelectedItem;
                sourceSinkData.Sink = SinkComboSelectedItem;
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
        }
        /// <summary>
        /// Removes the source sink.
        /// </summary>
        private void RemoveSourceSink()
        {
            SourceSinkData sourceSinkData = SourceSinkDataSelected;
            string sourceSinkKey = "";
            if (sourceSinkData == null)
            {
                return;
            }
            int index = 0;
            if (SelectedMode != UPTos)
            {
                //sourceSinkKey = sourceSinkData.Source.NodeKey.ToString();
                sourceSinkKey = sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();
                foreach (SourceSinkData sourceSink in mSourceSinkList)
                {
                    if (sourceSink.Source.NodeKey == sourceSinkData.Source.NodeKey)
                    {
                        break;
                    }
                    index++;
                }
            }
            else
            {
                sourceSinkKey = sourceSinkData.Source.NodeKey.ToString() + ":" + sourceSinkData.Sink.NodeKey.ToString();

                foreach (SourceSinkData sourceSink in mSourceSinkList)
                {
                    if (sourceSink.Source.NodeKey == sourceSinkData.Source.NodeKey && sourceSink.Sink.NodeKey == sourceSinkData.Sink.NodeKey)
                    {
                        break;
                    }
                    index++;
                }
            }
            if (mSourceNodeList.Count > 0 && index < mSourceNodeList.Count)
            {
                mSourceSinkList.RemoveAt(index);
            }
            if (mFillSourceSinkHash.ContainsKey(sourceSinkKey))
            {
                mFillSourceSinkHash.Remove(sourceSinkKey);
            }
            SourceSinkList = null;
            SourceSinkList = mFillSourceSinkHash.Values.ToList<SourceSinkData>();
        }
        /// <summary>
        /// Swaps source and sink.
        /// </summary>
        private void SwapSourceSink()
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
                mIsSwap = false;
            }
        }
        /// <summary>
        /// Shows the historical constraints.
        /// </summary>
        private void ShowHistoricalConstraints()
        {
            Vayu.ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            var datacontext = new Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new Vayu.ConstraintContingencyHistory.Model.DataService());
            window.DataContext = datacontext;
            if (SelectedMarket == "ERCOT")
            {
                bool isDA = false;

                if (SelectedRTDA == "DA")
                    isDA = true;
                else
                    isDA = false;

                datacontext.ShowHistoricalConstraintForExposure(SelectedExposureItem.monitoredName.ToString(), SelectedExposureItem.contingName.ToString(), SelectedExposureItem.constraintNum, 9, isDA);


            }
            window.Show();
        }
        /// <summary>
        /// Gets Input from controls.
        /// </summary>
        private void InputData()
        {
            if (SourceNodeComboSelectedItem != null && SourceComboSelectedItem == null && SinkComboSelectedItem == null)
            {
                sourceData = SourceNodeComboSelectedItem;
            }
            else
            {
                if (SourceSinkDataSelected != null)
                {
                    sourceData = SourceSinkDataSelected.Source;
                    sinkData = SourceSinkDataSelected.Sink;
                }
                else
                {
                    sourceData = SourceComboSelectedItem;
                    sinkData = SinkComboSelectedItem;
                }
            }
        }
        /// <summary>
        /// Pastes the source sink data.
        /// </summary>
        /// <param name="lstDataHelp">The LST data help.</param>
        /// <param name="type">The type.</param>
        private void PasteSourceSinkData(PasteHelp lstDataHelp, string type)
        {
            DataService mds = new DataService();
            var tempSrcSnkDataList = new List<SourceSinkData>();
            var sourceData = SourceNodeList.Where(a => a.NodeName.ToLower() == lstDataHelp.Source.ToLower()).FirstOrDefault();
            var sinkData = SinkNodeList.Where(a => a.NodeName.ToLower() == lstDataHelp.Sink.Trim().ToLower()).FirstOrDefault();
            string sourceName = lstDataHelp.Source;
            string sinkName = lstDataHelp.Sink;
            if ((sourceData == null || sinkData == null) && lstDataHelp.Source != null && lstDataHelp.Sink != null)
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
            tempSrcSnkDataList.Add(new SourceSinkData
            {
                Source = sourceData,
                Sink = sinkData
            });
            SourceSinkList = tempSrcSnkDataList.ToList();
            if (type == "VIRTUAL")
            {
                SourceSinkDataSelected = null;
            }
            else
            {
                SourceSinkDataSelected = SourceSinkList[0];
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the data.
        /// </summary>
        /// <param name="hashKey">The hash key.</param>
        public void RetrieveFetchData(string hashKey)
        {
            try
            {
                // Task updatetask = null;
                IsGoEnabled = false;
                RetrieveFetchDataThreaded();
                //updatetask = Task.Factory.StartNew(() => { RetrieveFetchDataThreaded(); });
                //await Task.WhenAll(updatetask);
                lstConstraintHash.Add(hashKey, ConstraintsList);

            }
            catch (Exception)
            {
                //throw;
            }
        }
        /// <summary>
        /// Calls RetrieveFetchDataThreaded method.
        /// </summary>
        public void RetrieveFetchDataCommand()
        {
            try
            {
                RetrieveFetchDataThreaded();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Retrieves Constraints List.
        /// </summary>
        public void RetrieveFetchDataThreaded()
        {
            try
            {
                int marketKey = 0;
                InputData();
                ConstraintsList = new List<Constraints>();
                if (SelectedMarket == "ERCOT")
                    marketKey = 9;
                if (MWChecked)
                {
                    if (marketKey != 0)
                    {
                        mDataService.GetConstraints((conConstraintList, error) =>
                        {
                            if (error != null)
                            {
                                return;
                            }
                            ConstraintsList = conConstraintList.OrderByDescending(t => Convert.ToDouble(t.he1Value)).ToList();
                        }, UptosChecked, VirtualsChecked, NodalChecked, PathChecked,
                                  IncChecked, DecChecked, LookBackChecked, sinkData, sourceData, MWChecked,
                                   DollarsChecked, DaysVal, DASettleSelected, RTSettleSelected, FamilyChecked, SelectedFamily,
                                   BestChecked, StartChecked, OutageChecked, ConstraintChecked, ShadowPriceValue, ConstraintsList, marketKey, SelectedRTDA);
                    }
                }
                else if (DollarsChecked)
                {
                    if (marketKey != 0)
                    {
                        mDataService.GetConstraints((conConstraintList, error) =>
                        {
                            if (error != null)
                            {
                                return;
                            }
                            ConstraintsList = conConstraintList.OrderByDescending(t => t.constraintNum).ToList();
                        }, UptosChecked, VirtualsChecked, NodalChecked, PathChecked,
                                   IncChecked, DecChecked, LookBackChecked, sinkData, sourceData, MWChecked,
                                    DollarsChecked, DaysVal, DASettleSelected, RTSettleSelected, FamilyChecked, SelectedFamily,
                                    BestChecked, StartChecked, OutageChecked, ConstraintChecked, ShadowPriceValue, ConstraintsList, marketKey, SelectedRTDA);
                    }
                }
                List<Constraints> tempConstraintList = new List<Constraints>();
                foreach (Constraints cons in ConstraintsList)
                {
                    cons.TotalHourlyValue = Convert.ToDouble(cons.he1Value == null ? 0 : cons.he1Value) + Convert.ToDouble(cons.he2Value == null ? 0 : cons.he2Value) + Convert.ToDouble(cons.he3Value == null ? 0 : cons.he3Value)
                                            + Convert.ToDouble(cons.he4Value == null ? 0 : cons.he4Value) + Convert.ToDouble(cons.he5Value == null ? 0 : cons.he5Value) + Convert.ToDouble(cons.he6Value == null ? 0 : cons.he6Value) + Convert.ToDouble(cons.he7Value == null ? 0 : cons.he7Value) + Convert.ToDouble(cons.he8Value == null ? 0 : cons.he8Value)
                                            + Convert.ToDouble(cons.he9Value == null ? 0 : cons.he9Value) + Convert.ToDouble(cons.he10Value == null ? 0 : cons.he10Value) + Convert.ToDouble(cons.he11Value == null ? 0 : cons.he11Value) + Convert.ToDouble(cons.he12Value == null ? 0 : cons.he12Value)
                                            + Convert.ToDouble(cons.he13Value == null ? 0 : cons.he13Value) + Convert.ToDouble(cons.he14Value == null ? 0 : cons.he14Value) + Convert.ToDouble(cons.he15Value == null ? 0 : cons.he15Value) + Convert.ToDouble(cons.he16Value == null ? 0 : cons.he16Value)
                                            + Convert.ToDouble(cons.he17Value == null ? 0 : cons.he17Value) + Convert.ToDouble(cons.he18Value == null ? 0 : cons.he18Value) + Convert.ToDouble(cons.he19Value == null ? 0 : cons.he19Value) + Convert.ToDouble(cons.he20Value == null ? 0 : cons.he20Value)
                                            + Convert.ToDouble(cons.he21Value == null ? 0 : cons.he21Value) + Convert.ToDouble(cons.he22Value == null ? 0 : cons.he22Value) + Convert.ToDouble(cons.he23Value == null ? 0 : cons.he23Value) + Convert.ToDouble(cons.he24Value == null ? 0 : cons.he24Value);
                    tempConstraintList.Add(cons);
                }
                ConstraintsList = null;
                ConstraintsList = tempConstraintList;
                if (ConstraintsList.Count == 0)
                {
                    MessageBox.Show("No data found for this criteria");
                }
                else
                {
                    if (mSrcSnkDataSelected.Source != null && mSrcSnkDataSelected.Sink != null)
                    {
                        string hashKey = mSrcSnkDataSelected.Source.NodeKey.ToString() + ":" + mSrcSnkDataSelected.Sink.NodeKey.ToString();
                        if (lstConstraintHash.ContainsKey(hashKey))
                        {
                            lstConstraintHash.Remove(hashKey);
                        }
                    }
                    else if (mSrcSnkDataSelected.Source != null && mSrcSnkDataSelected.Sink == null)
                    {
                        string hashKey = mSrcSnkDataSelected.Source.NodeKey.ToString();
                        if (lstConstraintHash.ContainsKey(hashKey))
                        {
                            lstConstraintHash.Remove(hashKey);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(System.Reflection.MethodInfo.GetCurrentMethod() + "\t" + ex.Message);
            }
            IsGoEnabled = true;
        }
        /// <summary>
        /// Sets the radio.
        /// </summary>
        public void SetRadio()
        {
            SelectedCapture = Dollars;
            SelectedMode = "VIRTUAL";
            SelectedVirtual = Nodal;
            //DollarsChecked = true;
            //NodalChecked = true;
            //VirtualsChecked = true;
        }

        /// <summary>
        /// Sets the radio up to.
        /// </summary>
        public void SetRadioUpTo()
        {
            SelectedCapture = Dollars;
            SelectedMode = UPTos;
            //DollarsChecked = true;
            //UptosChecked = true;
        }
        /// <summary>
        /// Handles the sort.
        /// </summary>
        /// <param name="constraintList">The constraint list.</param>
        /// <param name="listSortDirection">The list sort direction.</param>
        public void HandleSort(List<Constraints> constraintList, ListSortDirection listSortDirection)
        {
            try
            {
                // Constraints calculatedata = constraintList.Where(a => a.contingName.ToLower().Contains("calculated")).FirstOrDefault();
                //  Constraints rtCongestionData = constraintList.Where(a => a.contingName.ToLower().Contains("rt congestion")).FirstOrDefault();
                //   Constraints daCongestionData = constraintList.Where(a => a.contingName.ToLower().Contains("da congestion")).FirstOrDefault();
                //  Constraints dartCongestionData = constraintList.Where(a => a.contingName.ToLower().Contains("dart congestion")).FirstOrDefault();

                Constraints calculatedata = null;
                Constraints rtCongestionData = null;
                Constraints daCongestionData = null;
                Constraints dartCongestionData = null;

                if (constraintList.Where(a => a.contingName.ToLower().Contains("calculated")) != null)
                {
                    calculatedata = constraintList.Where(a => a.contingName.ToLower().Contains("calculated")).FirstOrDefault();
                }

                if (constraintList.Where(a => a.contingName.ToLower().Contains("rt congestion")) != null)
                    rtCongestionData = constraintList.Where(a => a.contingName.ToLower().Contains("rt congestion")).FirstOrDefault();

                if (constraintList.Where(a => a.contingName.ToLower().Contains("da congestion")) != null)
                    daCongestionData = constraintList.Where(a => a.contingName.ToLower().Contains("da congestion")).FirstOrDefault();

                if (constraintList.Where(a => a.contingName.ToLower().Contains("dart congestion")) != null)
                    dartCongestionData = constraintList.Where(a => a.contingName.ToLower().Contains("dart congestion")).FirstOrDefault();

                if (calculatedata != null)
                {
                    constraintList.Remove(calculatedata);
                    if (!StartChecked && !OutageChecked && !ConstraintChecked)
                    {
                        constraintList.Remove(rtCongestionData);
                        constraintList.Remove(daCongestionData);
                        constraintList.Remove(dartCongestionData);
                    }
                }
                var secondHalf = constraintList.Where(m => m.GetType().GetProperty(sortCondition).GetValue(m) == null).ToList();
                var firstHalf = constraintList.Where(m => m.GetType().GetProperty(sortCondition).GetValue(m) != null).ToList();
                List<Constraints> tempConstraintList = new List<Constraints>();
                if (listSortDirection == ListSortDirection.Ascending)
                {
                    firstHalf = firstHalf.OrderBy(m => m.GetType().GetProperty(sortCondition).GetValue(m)).ToList();
                }
                else
                {
                    firstHalf = firstHalf.OrderByDescending(m => m.GetType().GetProperty(sortCondition).GetValue(m)).ToList();
                }
                if (!StartChecked && !OutageChecked && !ConstraintChecked)
                {
                    tempConstraintList = firstHalf.Concat(secondHalf).ToList();

                    if (calculatedata != null)
                        tempConstraintList.Add(calculatedata);

                    if (rtCongestionData != null)
                        tempConstraintList.Add(rtCongestionData);

                    if (daCongestionData != null)
                        tempConstraintList.Add(daCongestionData);

                    if (dartCongestionData != null)
                        tempConstraintList.Add(dartCongestionData);
                }
                else
                {
                    tempConstraintList = firstHalf.Concat(secondHalf).ToList();
                    tempConstraintList.Add(calculatedata);
                }
                ConstraintsList = tempConstraintList;
            }
            catch (Exception ex)
            {
                //throw;
            }
        }

        /// <summary>
        /// Adds the source.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="SelectedDate">The selected date.</param>
        public void AddSource(PricingNode Source, DateTime SelectedDate)
        {
            SourceComboSelectedItem = Source;
            DASettleSelected = SelectedDate;
            RTSettleSelected = SelectedDate;
            VisibleNodal = true;
            VisiblePath = true;
            VisibleNodalLabel = true;
            VisibleNodalCombo = true;
            VisibleNodalGroupBox = true;
        }
        /// <summary>
        /// Adds the path.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="Sink">The sink.</param>
        /// <param name="SelectedDate">The selected date.</param>
        public void AddPath(PricingNode Source, PricingNode Sink, DateTime SelectedDate)
        {
            VisibleNodal = false;
            VisiblePath = false;
            VisibleNodalCombo = false;
            VisibleNodalLabel = false;
            VisibleUpToTrue = true;
            VisibleNodalGroupBox = false;
            SourceComboSelectedItem = SourceNodeList.Where(a => a.NodeName == Source.NodeName).FirstOrDefault();
            SinkComboSelectedItem = SinkNodeList.Where(a => a.NodeName == Sink.NodeName).FirstOrDefault();
            DASettleSelected = SelectedDate;
            RTSettleSelected = SelectedDate;
        }

        /// <summary>
        /// Sets the data.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <param name="Sink">The sink.</param>
        /// <param name="date">The date.</param>
        public void SetData(string Source, string Sink, DateTime date)
        {
            SourceComboSelectedItem = SourceNodeList.Where(p => p.NodeName == Source).FirstOrDefault();
            SinkComboSelectedItem = SinkNodeList.Where(p => p.NodeName == Sink).FirstOrDefault();
            RTSettleSelected = date;
            AddSourceSink();
        }
        /// <summary>
        /// Sets the source sink.
        /// </summary>
        public void SetSourceSink()
        {
            VisibleUpToTrue = true;
            SourceNodeList = null;
            DBAccess.GetSourceSinkNodeList(
                    (item1, error) =>
                    {
                        SourceNodeList = item1.Item1;
                        SinkNodeList = item1.Item2;
                    }, SelectedMarket, SelectedMode);//"PJM", "UPTO");
        }
        /// <summary>
        /// Sets the families.
        /// </summary>
        public void SetFamilies()
        {
            FamilyList = null;

            if (SelectedMarket == "ERCOT")
                FamilyList = DBAccess.GetERCOTFamilies(SelectedRTDA);
        }
        /// <summary>
        /// Sets the virtual.
        /// </summary>
        public void SetVirtual()
        {
            VisibleUpToTrue = true; ;
            if (VirtualsChecked == true)
            {
                SourceNodeList = null;
                DBAccess.GetSourceSinkNodeList(
                        (item1, error) =>
                        {
                            SourceNodeList = item1.Item1;
                            SinkNodeList = item1.Item1;
                        }, SelectedMarket, "VIRTUAL");//"PJM", "VIRTUAL");
            }
        }



        #endregion
    }
}
/// <summary>
/// 
/// </summary>
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
