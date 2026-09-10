using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Data;
using System.Windows.Media;
using Vayu.CRRCalculationLibrary;
using Vayu.CRRPNLDetails.Model;

namespace Vayu.CRRPNLDetails.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Variables

        /// <summary>
        /// The m end point
        /// </summary>
        Dictionary<int, PeriodDays> PeriodDaysdic = new Dictionary<int, PeriodDays>();
        private string mEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetCRRPNLService();

        /// <summary>
        /// The m FTR calculation proxy
        /// </summary>
       private ISourceSink mFTRCalculationProxy = null;
        /// <summary>
        /// The m accounts
        /// </summary>
        private Dictionary<string, string[]> mAccounts = new Dictionary<string, string[]>();
        /// <summary>
        /// The m PJM account
        /// </summary>
        private string[] mPJMAccount = new string[15];

        /// <summary>
        /// The m markets
        /// </summary>
        private Dictionary<string, int> mMarkets = new Dictionary<string, int> { { "ERCOT", 9 } };
        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName;
        //private string mUser = "sauravv";
        //private string mUser = "mbp";
        /// <summary>
        /// The m data service
        /// </summary>
        private readonly IDataService mDataService;

        /// <summary>
        /// The m sort order
        /// </summary>
        public bool mSortOrder = false;
        /// <summary>
        /// The m ftrfilter window
        /// </summary>
        private Vayu.CRRPNLDetails.Views.FTRDetailsFormFilter mFtrfilterWindow = null;
        /// <summary>
        /// The m FTR filter model
        /// </summary>
        private Vayu.CRRPNLDetails.ViewModels.FTRFilterViewModel mFtrFilterModel = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the m temporary selected.
        /// </summary>
        /// <value>
        /// The m temporary selected.
        /// </value>
        public List<string> mTempSelected { get; set; }
        /// <summary>
        /// Gets or sets the m temporary deleted.
        /// </summary>
        /// <value>
        /// The m temporary deleted.
        /// </value>
        public List<string> mTempDeleted { get; set; }
        /// <summary>
        /// Gets or sets the m temporary date selected.
        /// </summary>
        /// <value>
        /// The m temporary date selected.
        /// </value>
        public List<string> mTempDateSelected { get; set; }
        /// <summary>
        /// Gets or sets the m temporary date deleted.
        /// </summary>
        /// <value>
        /// The m temporary date deleted.
        /// </value>
        public List<string> mTempDateDeleted { get; set; }

        /// <summary>
        /// The m market list
        /// </summary>
        private List<string> mMarketList;
        /// <summary>
        /// Gets or sets the market list.
        /// </summary>
        /// <value>
        /// The market list.
        /// </value>
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
            get
            {
                return mMarketSelected;
            }
            set
            {
                mMarketSelected = value;
                SetListBox(MarketSelected);
                int key = 0;
                if (!string.IsNullOrEmpty(MarketSelected))
                {
                    key = mMarkets[MarketSelected];
                }
                if (key != 0)
                {
                    List<DateTime> monthlist = new List<DateTime>();
                    mDataService.GetDateList((tempList, error) => { if (error != null) { return; } monthlist = tempList; }, key);
                    if (monthlist != null && monthlist.Count > 0)
                    {
                        List<string> tempDateList = new List<string>();
                        DateTime maxyear = monthlist.Max();
                        DateTime minyear = monthlist.Min();
                        if (key == 9)
                        {
                            for (; minyear.Year <= maxyear.Year; minyear = minyear.AddYears(1))
                            {
                                tempDateList.Add("CRR " + (minyear.Year).ToString());
                            }
                        }

                        tempDateList.Sort();
                        DateList = tempDateList.ToList();
                    }
                }
                HideColumns();
                RaisePropertyChanged("MarketSelected");
            }
        }
        /// <summary>
        /// The m int ext list
        /// </summary>
        private List<string> mIntExtList;
        /// <summary>
        /// Gets or sets the int ext list.
        /// </summary>
        /// <value>
        /// The int ext list.
        /// </value>
        public List<string> IntExtList
        {
            get
            {
                return mIntExtList;
            }
            set
            {
                mIntExtList = value;
                RaisePropertyChanged("IntExtList");
            }
        }
        /// <summary>
        /// The m int ext selected
        /// </summary>
        private string mIntExtSelected;
        /// <summary>
        /// Gets or sets the int ext selected.
        /// </summary>
        /// <value>
        /// The int ext selected.
        /// </value>
        public string IntExtSelected
        {
            get
            {
                return mIntExtSelected;
            }
            set
            {
                mIntExtSelected = value;
                SetListBox(IntExtSelected);
                RaisePropertyChanged("IntExtSelected");
            }
        }
        /// <summary>
        /// The m participants list
        /// </summary>
        private List<string> mParticipantsList;
        /// <summary>
        /// Gets or sets the participants list.
        /// </summary>
        /// <value>
        /// The participants list.
        /// </value>
        public List<string> ParticipantsList
        {
            get
            {
                return mParticipantsList;
            }
            set
            {
                mParticipantsList = value;
                RaisePropertyChanged("ParticipantsList");
            }
        }
        /// <summary>
        /// The m participants selected list
        /// </summary>
        private List<string> mParticipantsSelectedList;
        /// <summary>
        /// Gets or sets the participants selected list.
        /// </summary>
        /// <value>
        /// The participants selected list.
        /// </value>
        public List<string> ParticipantsSelectedList
        {
            get
            {
                return mParticipantsSelectedList;
            }
            set
            {
                mParticipantsSelectedList = value;
                RaisePropertyChanged("ParticipantsSelectedList");
            }
        }
        /// <summary>
        /// The m date selected list
        /// </summary>
        private List<string> mDateSelectedList;
        /// <summary>
        /// Gets or sets the date selected list.
        /// </summary>
        /// <value>
        /// The date selected list.
        /// </value>
        public List<string> DateSelectedList
        {
            get
            {
                return mDateSelectedList;
            }
            set
            {
                mDateSelectedList = value;
                RaisePropertyChanged("DateSelectedList");
            }
        }
        /// <summary>
        /// Gets or sets the m temporary selected list.
        /// </summary>
        /// <value>
        /// The m temporary selected list.
        /// </value>
        public List<string> mTempSelectedList { get; set; }
        /// <summary>
        /// Gets or sets the m temporary selected dates.
        /// </summary>
        /// <value>
        /// The m temporary selected dates.
        /// </value>
        public List<string> mTempSelectedDates { get; set; }
        /// <summary>
        /// The m selected participants
        /// </summary>
        private List<string> mSelectedParticipants;
        /// <summary>
        /// Gets or sets the selected participants.
        /// </summary>
        /// <value>
        /// The selected participants.
        /// </value>
        public List<string> SelectedParticipants
        {
            get
            {
                return mSelectedParticipants;
            }
            set
            {
                mSelectedParticipants = value;
                RaisePropertyChanged("SelectedParticipants");
            }
        }
        /// <summary>
        /// The m date list
        /// </summary>
        private List<string> mDateList;
        /// <summary>
        /// Gets or sets the date list.
        /// </summary>
        /// <value>
        /// The date list.
        /// </value>
        public List<string> DateList
        {
            get
            {
                return mDateList;
            }
            set
            {
                mDateList = value;
                RaisePropertyChanged("DateList");
            }
        }
        /// <summary>
        /// The m selected date
        /// </summary>
        private string mSelectedDate;
        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public string SelectedDate
        {
            get
            {
                return mSelectedDate;
            }
            set
            {
                mSelectedDate = value;
                DateChanged();
                RaisePropertyChanged("SelectedDate");
            }
        }
        /// <summary>
        /// The m selected month list
        /// </summary>
        private List<string> mSelectedMonthList;
        /// <summary>
        /// Gets or sets the selected month list.
        /// </summary>
        /// <value>
        /// The selected month list.
        /// </value>
        public List<string> SelectedMonthList
        {
            get
            {
                return mSelectedMonthList;
            }
            set
            {
                mSelectedMonthList = value;
                RaisePropertyChanged("SelectedMonthList");
            }
        }
        /// <summary>
        /// The m source sinks
        /// </summary>
        private List<SourceSink> mSourceSinks;
        // private ObservableCollection<SourceSink> mSourceSinks;
        /// <summary>
        /// Gets or sets the source sinks.
        /// </summary>
        /// <value>
        /// The source sinks.
        /// </value>
        public List<SourceSink> SourceSinks
        {
            get
            {
                return mSourceSinks;
            }
            set
            {
                mSourceSinks = value;
                RaisePropertyChanged("SourceSinks");
            }
        }
        //public ObservableCollection<SourceSink> SourceSinks
        //{
        //    get { return mSourceSinks; }

        //    set
        //    {
        //        mSourceSinks = value;
        //        RaisePropertyChanged("SourceSinks");
        //    }
        //}

        private List<SourceSink> sSourceSinkstemp;
        /// <summary>
        /// Gets or sets the source sinks.
        /// </summary>
        /// <value>
        /// The source sinks.
        /// </value>
        public List<SourceSink> SourceSinksTemp
        {
            get
            {
                return sSourceSinkstemp;
            }
            set
            {
                sSourceSinkstemp = value;
                RaisePropertyChanged("SourceSinksTemp");
            }
        }

        /// <summary>
        /// The m all checked
        /// </summary>
        private bool mAllChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [all checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [all checked]; otherwise, <c>false</c>.
        /// </value>
        public bool AllChecked
        {
            get
            {
                return mAllChecked;
            }
            set
            {
                mAllChecked = value;
                RaisePropertyChanged("AllChecked");
            }
        }
        /// <summary>
        /// The m quarter checked
        /// </summary>
        private bool mQuarterChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [quarter checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [quarter checked]; otherwise, <c>false</c>.
        /// </value>
        public bool QuarterChecked
        {
            get
            {
                return mQuarterChecked;
            }
            set
            {
                mQuarterChecked = value;
                RaisePropertyChanged("QuarterChecked");
            }
        }
        /// <summary>
        /// The m month checked
        /// </summary>
        private bool mMonthChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [month checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [month checked]; otherwise, <c>false</c>.
        /// </value>
        public bool MonthChecked
        {
            get
            {
                return mMonthChecked;
            }
            set
            {
                mMonthChecked = value;
                FilterData();
                RaisePropertyChanged("MonthChecked");
            }
        }

        private bool sSeq1checked;

        public bool Seq1checked
        {
            get { return sSeq1checked; }
            set { sSeq1checked = value; }
        }
        private bool sSeq2checked;

        public bool Seq2checked
        {
            get { return sSeq2checked; }
            set { sSeq2checked = value; }
        }
        private bool sSeq3checked;

        public bool Seq3checked
        {
            get { return sSeq3checked; }
            set { sSeq3checked = value; }
        }
        private bool sSeq4checked;

        public bool Seq4checked
        {
            get { return sSeq4checked; }
            set { sSeq4checked = value; }
        }
        private bool sSeq5checked;

        public bool Seq5checked
        {
            get { return sSeq5checked; }
            set { sSeq5checked = value; }
        }
        private bool sSeq6checked;

        public bool Seq6checked
        {
            get { return sSeq6checked; }
            set { sSeq6checked = value; }
        }

        /// <summary>
        /// The m da price checked
        /// </summary>
        private bool mDAPriceChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [da price checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [da price checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DAPriceChecked
        {
            get
            {
                return mDAPriceChecked;
            }
            set
            {
                if (DAPriceChecked)
                {
                    D1Type = "Hidden";
                    DType = "Visible";
                }
                mDAPriceChecked = value;

                RaisePropertyChanged("DAPriceChecked");

            }
        }

        private bool sRTPriceChecked;

        public bool RTPriceChecked
        {
            get
            {
                return sRTPriceChecked;
            }
            set
            {
                if (RTPriceChecked)
                {
                    D1Type = "Hidden";
                    DType = "Visible";
                }
                sRTPriceChecked = value;

                RaisePropertyChanged("RTPriceChecked");

            }
        }
        /// <summary>
        /// The m PNL checked
        /// </summary>
        private bool mPNLChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [PNL checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [PNL checked]; otherwise, <c>false</c>.
        /// </value>
        public bool PNLChecked
        {
            get
            {
                return mPNLChecked;
            }
            set
            {
                if (PNLChecked)
                {
                    D1Type = "Hidden";
                    DType = "Visible";
                }
                mPNLChecked = value;

                RaisePropertyChanged("PNLChecked");

            }
        }
        private bool mCostChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [PNL checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [PNL checked]; otherwise, <c>false</c>.
        /// </value>
        public bool CostChecked
        {
            get
            {
                return mCostChecked;
            }
            set
            {
                if (CostChecked)
                {
                    D1Type = "Hidden";
                    DType = "Visible";
                }
                mCostChecked = value;

                RaisePropertyChanged("CostChecked");

            }
        }
        private bool mMWhChecked;
        public bool MWhChecked
        {
            get
            {
                return mMWhChecked;
            }
            set
            {
                if (MWhChecked)
                {
                    D1Type = "Visible";
                    DType = "Hidden";
                }
                mMWhChecked = value;

                RaisePropertyChanged("MWhChecked");

            }
        }




        /// <summary>
        /// The m long term checked
        /// </summary>
        private bool mLongTermChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [long term checked].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [long term checked]; otherwise, <c>false</c>.
        /// </value>
        public bool LongTermChecked
        {
            get
            {
                return mLongTermChecked;
            }
            set
            {
                mLongTermChecked = value;
                FilterData();
                RaisePropertyChanged("LongTermChecked");
            }
        }
        /// <summary>
        /// The m FTR details data list
        /// </summary>
        private List<FTRDetailsData> mFTRDetailsDataList;
        /// <summary>
        /// Gets or sets the FTR details data list.
        /// </summary>
        /// <value>
        /// The FTR details data list.
        /// </value>
        public List<FTRDetailsData> FTRDetailsDataList
        {
            get
            {
                return mFTRDetailsDataList;
            }
            set
            {
                mFTRDetailsDataList = value;
                RaisePropertyChanged("FTRDetailsDataList");
            }
        }


        //private List<FTRDetailsData> mFTRDetailsDataListMWH;
        ///// <summary>
        ///// Gets or sets the FTR details data list.
        ///// </summary>
        ///// <value>
        ///// The FTR details data list.
        ///// </value>
        //public List<FTRDetailsData> FTRDetailsDataListMWH
        //{
        //    get
        //    {
        //        return mFTRDetailsDataListMWH;
        //    }
        //    set
        //    {
        //        mFTRDetailsDataListMWH = value;
        //        RaisePropertyChanged("FTRDetailsDataListMWH");
        //    }
        //}
        /// <summary>
        /// The m cell color
        /// </summary>
        private Brush mCellColor;
        /// <summary>
        /// Gets or sets the color of the cell.
        /// </summary>
        /// <value>
        /// The color of the cell.
        /// </value>
        public Brush CellColor
        {
            get
            {
                return mCellColor;
            }
            set
            {
                mCellColor = value;
                RaisePropertyChanged("CellColor");
            }
        }
        /// <summary>
        /// The m cost total
        /// </summary>
        private double mCostTotal;
        /// <summary>
        /// Gets or sets the cost total.
        /// </summary>
        /// <value>
        /// The cost total.
        /// </value>
        public double CostTotal
        {
            get
            {
                return mCostTotal;
            }
            set
            {
                mCostTotal = value;
                RaisePropertyChanged("CostTotal");
            }
        }
        /// <summary>
        /// The m da total
        /// </summary>
        private double mDATotal;
        /// <summary>
        /// Gets or sets the da total.
        /// </summary>
        /// <value>
        /// The da total.
        /// </value>
        public double DATotal
        {
            get
            {
                return mDATotal;
            }
            set
            {
                mDATotal = value;
                RaisePropertyChanged("DATotal");
            }
        }

        private double sRTTotal;

        public double RTTotal
        {
            get
            {
                return sRTTotal;
            }
            set
            {
                sRTTotal = value;
                RaisePropertyChanged("RTTotal");
            }
        }
        /// <summary>
        /// The m PNL total
        /// </summary>
        private double mPNLTotal;
        /// <summary>
        /// Gets or sets the PNL total.
        /// </summary>
        /// <value>
        /// The PNL total.
        /// </value>
        public double PNLTotal
        {
            get
            {
                return mPNLTotal;
            }
            set
            {
                mPNLTotal = value;
                RaisePropertyChanged("PNLTotal");
            }
        }
        /// <summary>
        /// The m mw total
        /// </summary>
        private double mMWTotal;
        /// <summary>
        /// Gets or sets the mw total.
        /// </summary>
        /// <value>
        /// The mw total.
        /// </value>
        public double MWTotal
        {
            get
            {
                return mMWTotal;
            }
            set
            {
                mMWTotal = value;
                RaisePropertyChanged("MWTotal");
            }
        }
        /// <summary>
        /// The m count
        /// </summary>
        private double mCount;
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public double Count
        {
            get
            {
                return mCount;
            }
            set
            {
                mCount = value;
                RaisePropertyChanged("Count");
            }
        }
        /// <summary>
        /// The m period type visible
        /// </summary>
        private Visibility mPeriodTypeVisible;
        /// <summary>
        /// Gets or sets the period type visible.
        /// </summary>
        /// <value>
        /// The period type visible.
        /// </value>
        public Visibility PeriodTypeVisible
        {
            get
            {
                return mPeriodTypeVisible;
            }
            set
            {
                mPeriodTypeVisible = value;
                RaisePropertyChanged("PeriodTypeVisible");
            }
        }
        /// <summary>
        /// The m clearing price visible
        /// </summary>
        private Visibility mClearingPriceVisible;
        /// <summary>
        /// Gets or sets the clearing price visible.
        /// </summary>
        /// <value>
        /// The clearing price visible.
        /// </value>
        public Visibility ClearingPriceVisible
        {
            get
            {
                return mClearingPriceVisible;
            }
            set
            {
                mClearingPriceVisible = value;
                RaisePropertyChanged("ClearingPriceVisible");
            }
        }
        /// <summary>
        /// The m source zone visible
        /// </summary>
        private Visibility mSourceZoneVisible;
        /// <summary>
        /// Gets or sets the source zone visible.
        /// </summary>
        /// <value>
        /// The source zone visible.
        /// </value>
        public Visibility SourceZoneVisible
        {
            get
            {
                return mSourceZoneVisible;
            }
            set
            {
                mSourceZoneVisible = value;
                RaisePropertyChanged("SourceZoneVisible");
            }
        }
        /// <summary>
        /// The m sink zone visible
        /// </summary>
        private Visibility mSinkZoneVisible;
        /// <summary>
        /// Gets or sets the sink zone visible.
        /// </summary>
        /// <value>
        /// The sink zone visible.
        /// </value>
        public Visibility SinkZoneVisible
        {
            get
            {
                return mSinkZoneVisible;
            }
            set
            {
                mSinkZoneVisible = value;
                RaisePropertyChanged("SinkZoneVisible");
            }
        }
        /// <summary>
        /// The m combine enable
        /// </summary>
        private bool mCombineEnable;
        /// <summary>
        /// Gets or sets a value indicating whether [combine enable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [combine enable]; otherwise, <c>false</c>.
        /// </value>
        public bool CombineEnable
        {
            get
            {
                return mCombineEnable;
            }
            set
            {
                mCombineEnable = value;
                RaisePropertyChanged("CombineEnable");
            }
        }
        /// <summary>
        /// The m combine button text
        /// </summary>
        private string mCombineButtonText;
        /// <summary>
        /// Gets or sets the combine button text.
        /// </summary>
        /// <value>
        /// The combine button text.
        /// </value>
        public string CombineButtonText
        {
            get
            {
                return mCombineButtonText;
            }
            set
            {
                mCombineButtonText = value;
                RaisePropertyChanged("CombineButtonText");
            }
        }
        /// <summary>
        /// The m generate enabled
        /// </summary>
        private bool mGenerateEnabled;
        /// <summary>
        /// Gets or sets a value indicating whether [generate enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [generate enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool GenerateEnabled
        {
            get
            {
                return mGenerateEnabled;
            }
            set
            {
                mGenerateEnabled = value;
                RaisePropertyChanged("GenerateEnabled");
            }
        }
        /// <summary>
        /// The m parent model
        /// </summary>
        private Vayu.CRRPNLDetails.ViewModels.MainWindowViewModel mParentModel;
        /// <summary>
        /// Gets or sets the parent model.
        /// </summary>
        /// <value>
        /// The parent model.
        /// </value>
        public Vayu.CRRPNLDetails.ViewModels.MainWindowViewModel ParentModel
        {
            get
            {
                return mParentModel;
            }
            set
            {
                mParentModel = value;
                RaisePropertyChanged("ParentModel");
            }
        }
        /// <summary>
        /// Gets or sets the frozen column count.
        /// </summary>
        /// <value>
        /// The frozen column count.
        /// </value>
        public int FrozenColumnCount { get; set; }
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
        /// The m ro w color
        /// </summary>
        private Brush mRoWColor;
        /// <summary>
        /// Gets or sets the color of the ro w.
        /// </summary>
        /// <value>
        /// The color of the ro w.
        /// </value>
        public Brush RoWColor
        {
            get
            {
                return mRoWColor;
            }
            set
            {
                mRoWColor = value;
                RaisePropertyChanged("RoWColor");
            }
        }
        private string mDType;
        public string DType
        {
            get
            {
                return mDType;
            }
            set
            {
                mDType = value;
                RaisePropertyChanged("DType");
            }
        }
        private string mD1Type;
        public string D1Type
        {
            get
            {
                return mD1Type;
            }
            set
            {
                mD1Type = value;
                RaisePropertyChanged("D1Type");
            }
        }

        #region Relay Command Properties

        /// <summary>
        /// Gets or sets the move paticipant command.
        /// </summary>
        /// <value>
        /// The move paticipant command.
        /// </value>
        public DelegateCommand MovePaticipantCommand { private set; get; }
        /// <summary>
        /// Gets or sets the move date command.
        /// </summary>
        /// <value>
        /// The move date command.
        /// </value>
        public DelegateCommand MoveDateCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete participant command.
        /// </summary>
        /// <value>
        /// The delete participant command.
        /// </value>
        public DelegateCommand DeleteParticipantCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete date command.
        /// </summary>
        /// <value>
        /// The delete date command.
        /// </value>
        public DelegateCommand DeleteDateCommand { private set; get; }
        /// <summary>
        /// Gets or sets the tyd command.
        /// </summary>
        /// <value>
        /// The tyd command.
        /// </value>
        public DelegateCommand TYDCommand { private set; get; }
        /// <summary>
        /// Gets or sets the MTD command.
        /// </summary>
        /// <value>
        /// The MTD command.
        /// </value>
        public DelegateCommand MTDCommand { private set; get; }
        /// <summary>
        /// Gets or sets the generate report command.
        /// </summary>
        /// <value>
        /// The generate report command.
        /// </value>
        public DelegateCommand GenerateReportCommand { private set; get; }

        public DelegateCommand CheckExposureCommand { private set; get; }
        /// <summary>
        /// Gets or sets the export CSV command.
        /// </summary>
        /// <value>
        /// The export CSV command.
        /// </value>
        public DelegateCommand ExportCSVCommand { private set; get; }
        /// <summary>
        /// Gets or sets the combine command.
        /// </summary>
        /// <value>
        /// The combine command.
        /// </value>
        public DelegateCommand CombineCommand { private set; get; }
        /// <summary>
        /// Gets or sets the generate XML.
        /// </summary>
        /// <value>
        /// The generate XML.
        /// </value>
        public DelegateCommand GenerateXML { private set; get; }
        /// <summary>
        /// Gets or sets the selection changed command.
        /// </summary>
        /// <value>
        /// The selection changed command.
        /// </value>
        public DelegateCommand<System.Collections.IList> SelectionChangedCommand { private set; get; }
        MainWindowViewModel ftrobjmainviewmodel;
        #endregion

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        public MainWindowViewModel(IDataService dataService)
        {

            D1Type = "Visible";
            DType = "Hidden";

            CombineEnable = false;
            CombineButtonText = "Combine";
            GenerateEnabled = true;
            mDataService = dataService;
            
            //List<string> tempMarketList = new List<string>();
            //tempMarketList.Add("PJM");
            //tempMarketList.Add("MISO");
            //tempMarketList.Add("CAISO");
            //tempMarketList.Add("SPP");
            //tempMarketList.Add("ERCOT");
            MarketList = null;
            MarketList = mMarkets.Keys.ToList();
            //mPJMAccount = Vayu.DBLibrary.DBAccess.GetFTRPortfolio(DateTime.Today.AddMonths(-1), mUser, "FTR", "PJM", "").Select(x => x.Name).ToArray();
            //mAccounts.Add("PJM", mPJMAccount);
            mPJMAccount = Vayu.DBLibrary.DBAccess.GetFTRPortfolio(DateTime.Today.AddMonths(-1), mUser, "CRR", "ERCOT", "").Select(x => x.Name).ToArray();
            mAccounts.Add("ERCOT", mPJMAccount);

            //mAccounts.Add("MISO", mMISOAccount);
            //mAccounts.Add("CAISO", mCAISOAccount);
            //mAccounts.Add("SPP", mSPPAccount);
            //mAccounts.Add("ERCOT", mERCOTAccount);
            MovePaticipantCommand = new DelegateCommand(MoveParticipants);
            MoveDateCommand = new DelegateCommand(MoveDates);
            DeleteParticipantCommand = new DelegateCommand(DeleteParticipants);
            DeleteDateCommand = new DelegateCommand(DeleteDates);
            TYDCommand = new DelegateCommand(GenerateYTD);
            MTDCommand = new DelegateCommand(GenerateMTD);
            GenerateReportCommand = new DelegateCommand(GenerateReport);
            CheckExposureCommand = new DelegateCommand(CalculateExposure);
            ExportCSVCommand = new DelegateCommand(ExportCSV);
            CombineCommand = new DelegateCommand(Combine);
            GenerateXML = new DelegateCommand(GenerateXMLFile);

            AllChecked = true;
            MonthChecked = true;
            QuarterChecked = true;
            DAPriceChecked = true;
            LongTermChecked = true;

            Seq1checked = true;
            Seq2checked = true;
            Seq3checked = true;
            Seq4checked = true;
            Seq5checked = true;
            Seq6checked = true;

        }

        private void CalculateExposure()
        {
            if (FTRDetailsDataList == null || FTRDetailsDataList.Count == 0)
            {
                MessageBox.Show("Please Generate Report For any Participant");
                return;
            }

            //Vayu.CRRPNLDetails.ExposureDetails view = new ExposureDetails();
            //view.DataContext = new CRRPNLDetails.ViewModel.ExposureDetailsViewModel1(this, FTRDetailsDataList);
            //view.Show();
        }

        #region Private Methods

        /// <summary>
        /// Generates the XML file.
        /// </summary>
        private void GenerateXMLFile()
        {
            FTRDetailsData firstItem = FTRDetailsDataList[1];
            StringBuilder bidText = new StringBuilder();
            bidText.AppendLine("<?xml version=\"1.0\"?>");
            bidText.AppendLine("<env:Envelope xmlns:mkt=\"http://eftr.pjm.com/ftr/xml\" xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            bidText.AppendLine("<env:Body>");
            bidText.AppendLine("<SubmitRequest>");
            bidText.AppendLine("<TradingPost>");
            string tradeType = firstItem.TradeType.ToUpper() == "BUY" ? "Buy" : "Sell";
            int roundStr = 1;
            string marketStr = "SEP 2013 Auction";
            bidText.AppendLine("<FTR trade =\"" + tradeType + "\" market =\"" + marketStr + "\" round=\"" + roundStr + "\">");
            bidText.AppendLine("<Interval start=\"" + "06/01/2013" + "\" end=\"" + "06/30/2013" + "\" />");
            bidText.AppendLine("<Path source=\"" + firstItem.Source + "\" sink=\"" + firstItem.Sink + "\"/>");
            bidText.AppendLine("<Class>" + firstItem.ClassType + "</Class>");
            string period = firstItem.PeriodType == "ALL" ? "All" : firstItem.PeriodType;
            bidText.AppendLine("<Period>" + period + "</Period>");
            bidText.AppendLine("<Hedge>" + firstItem.HedgeType + "</Hedge>");
            bidText.AppendLine("<MW>" + firstItem.MWTotal + "</MW>");
            bidText.AppendLine("<Price>" + Math.Round(firstItem.Cost, 2) + "</Price>");
            bidText.AppendLine("</FTR>");
            bidText.AppendLine("</TradingPost>");
            bidText.AppendLine("</SubmitRequest>");
            bidText.AppendLine("</env:Body>");
            bidText.AppendLine("</env:Envelope>");
            try
            {
                if (bidText.ToString().Length > 0)
                {
                    TextWriter writer = new StreamWriter(@"Y:\PJMFTRTestXML\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xml");
                    writer.Write(bidText.ToString());
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Exports the CSV.
        /// </summary>
        private void ExportCSV()
        {
            SaveFileDialog savefiledialog = new SaveFileDialog();
            savefiledialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            savefiledialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            savefiledialog.FilterIndex = 1;
            savefiledialog.RestoreDirectory = true;
            savefiledialog.FileName = "PNL_Details_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            if ((bool)savefiledialog.ShowDialog())
            {
                StringBuilder builder = new StringBuilder();
                if (FTRDetailsDataList.Count > 0)
                {
                    foreach (PropertyInfo item in FTRDetailsDataList[0].GetType().GetProperties())
                    {
                        builder.Append(item.Name + ",");
                    }
                    builder.ToString().Remove(builder.Length - 1, 1);
                    builder.AppendLine();

                    foreach (FTRDetailsData item in FTRDetailsDataList)
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
        /// Generates the report.
        /// </summary>
        private void GenerateReport()
        {
            if (ValidateEntries())
            {
                Connect();
                GenerateEnabled = false;
                Task.Factory.StartNew(() => { RunReport(); });
            }
        }
        /// <summary>
        /// Validates the entries.
        /// </summary>
        /// <returns></returns>
        private Boolean ValidateEntries()
        {
            if (ParticipantsSelectedList != null)
            {
                if (ParticipantsSelectedList.Count == 0)
                {
                    MessageBox.Show("Please select at least one participant");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (DateSelectedList != null)
            {
                if (DateSelectedList.Count == 0)
                {
                    MessageBox.Show("Please select at least one month");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
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
            //CustomBinding binding = new CustomBinding(encoder, transport);
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.CloseTimeout = new TimeSpan(0, 40, 0);
            myBinding.OpenTimeout = new TimeSpan(0, 40, 0);
            myBinding.SendTimeout = new TimeSpan(0, 40, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 40, 0);
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
                mFTRCalculationProxy = pipeFactory.CreateChannel();
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Runs the report.
        /// </summary>
        private void RunReport()
        {
            if (MWhChecked)
            {
                D1Type = "Visible";
                DType = "Hidden";
            }
            else
            {
                D1Type = "Hidden";
                DType = "Visible";
            }
            FTRDetailsDataList = null;
            CostTotal = 0.0;
            DATotal = 0.0;
            RTTotal = 0.0;
            PNLTotal = 0.0;
            MWTotal = 0.0;
            Count = 0.0;
            RetrieveData();
            FilterData();
            // PopulateReport();
            if (FTRDetailsDataList != null)
            {
                if (FTRDetailsDataList.Count > 0)
                {
                    CombineEnable = true;
                }
            }
            if (GenerateEnabled == false)
                GenerateEnabled = true;
        }
        /// <summary>
        /// Filters the data.
        /// </summary>
        private void FilterData()
        {
            if (GenerateEnabled == true)
            {
                GenerateEnabled = false;
            }
            List<SourceSink> toFilterData = new List<SourceSink>();
            if (SourceSinks != null)
            {
                if (SourceSinks.Count > 0)
                {
                    toFilterData = SourceSinksTemp.ToList();
                    if (!AllChecked)
                    {
                        List<SourceSink> filteredDataList = new List<SourceSink>();
                        foreach (var item in toFilterData)
                        {
                            if (!item.PeriodType.ToLower().Equals("all"))
                            {
                                filteredDataList.Add(item);
                            }
                        }
                        if (filteredDataList.Count > 0)
                        {
                            toFilterData = filteredDataList;
                        }
                        else
                        {
                            toFilterData = null;
                        }
                    }
                    if (!QuarterChecked)
                    {
                        List<SourceSink> filteredDataList = new List<SourceSink>();
                        if (toFilterData != null)
                        {
                            foreach (var item in toFilterData)
                            {
                                if (!item.PeriodType.ToLower().Equals("q1") && !item.PeriodType.ToLower().Equals("q2") && !item.PeriodType.ToLower().Equals("q3") && !item.PeriodType.ToLower().Equals("q4")
                                    && !item.PeriodType.ToLower().Equals("summer") && !item.PeriodType.ToLower().Equals("spring") && !item.PeriodType.ToLower().Equals("fall") && !item.PeriodType.ToLower().Equals("winter"))
                                {
                                    filteredDataList.Add(item);
                                }
                            }
                        }
                        if (filteredDataList.Count > 0)
                        {
                            toFilterData = filteredDataList;
                        }
                        else
                        {
                            toFilterData = null;
                        }
                    }
                    if (!MonthChecked)
                    {
                        List<SourceSink> filteredDataList = new List<SourceSink>();
                        if (toFilterData != null)
                        {
                            foreach (var item in toFilterData)
                            {
                                if (!item.PeriodType.ToLower().Equals("monthly"))
                                {
                                    filteredDataList.Add(item);
                                }
                            }
                        }
                        if (filteredDataList.Count > 0)
                        {
                            toFilterData = filteredDataList;
                        }
                        else
                        {
                            toFilterData = null;
                        }
                    }
                    if (!LongTermChecked)
                    {
                        List<SourceSink> filteredDataList = new List<SourceSink>();
                        if (toFilterData != null)
                        {
                            foreach (var item in toFilterData)
                            {
                                if (!item.PeriodType.ToLower().Equals("annual"))
                                {
                                    filteredDataList.Add(item);
                                }
                            }
                        }
                        if (filteredDataList.Count > 0)
                        {
                            toFilterData = filteredDataList;
                        }
                        else
                        {
                            toFilterData = null;
                        }
                    }
                    SourceSinks = null;
                    if (toFilterData != null)
                    {
                        //SourceSinks = new ObservableCollection<SourceSink>(toFilterData);
                        //Count = toFilterData.Count;
                        SourceSinks = toFilterData.ToList();
                        Count = toFilterData.ToList().Count();
                    }
                    else
                    {
                        MessageBox.Show("No record found");
                    }
                }
            }
            PopulateReport();
            GenerateEnabled = true;
        }
        /// <summary>
        /// Populates the report.
        /// </summary>
        private void PopulateReport()
        {
            try
            {
                List<FTRDetailsData> tempFTRDataList = new List<FTRDetailsData>();
                if (SourceSinks != null)
                {
                    if (SourceSinks.Count > 0)
                    {
                        //DataService ds = new DataService();
                        foreach (SourceSink item in SourceSinks)
                        {


                            FTRDetailsData ftrdataItem = new FTRDetailsData();
                            string participant = item.Participant;
                            //if (participant == "TLLRV4")
                            //    ftrdataItem.Participant = "SIGMA";
                            //else
                            ftrdataItem.Participant = item.Participant;
                            ftrdataItem.Month = item.Month;
                            ftrdataItem.Auction = item.AuctionName;
                            ftrdataItem.PeriodType = item.PeriodType;
                            ftrdataItem.TradeType = item.TradeType;
                            ftrdataItem.HedgeType = item.HedgeType;
                            ftrdataItem.ClassType = item.ClassType;
                            ftrdataItem.Source = item.Source;
                            ftrdataItem.Sink = item.Sink;
                            ftrdataItem.MWTotal = item.MW;
                            ftrdataItem.Cost = item.Costmonthlytotal;
                            ftrdataItem.DAPrice = item.DAmonthlytotal;
                            ftrdataItem.RTPrice = item.RTmonthlytotal;
                            ftrdataItem.PNL = ftrdataItem.PNL = item.PNLmonthlytotal;
                            ftrdataItem.ClearingPrice = item.Costmonthlytotal / item.MW;
                            if (item.HedgeType.ToUpper() == "OBLIGATION" || item.HedgeType.ToUpper() == "OBL")
                            {
                                ftrdataItem.FTRPricePerHour = item.Obligation;
                            }
                            else
                            {
                                ftrdataItem.FTRPricePerHour = item.Option;
                            }
                            ftrdataItem.MTDTotal = item.PNLmonthlytotal;
                            PeriodDays dayobj = new PeriodDays();
                            //DataService ds = new DataService();
                            DateTime startDate;
                            DateTime endDate;
                            int month1 = DateTime.ParseExact(ftrdataItem.Month.Substring(0, 3), "MMM", CultureInfo.CurrentCulture).Month;
                            int year1 = int.Parse(ftrdataItem.Month.Substring(ftrdataItem.Month.Length - 4, 4));
                            startDate = new DateTime(year1, month1, 1);
                            endDate = startDate.AddMonths(1).AddDays(-1);
                            //PeriodHours hours = ds.GetPeriodHours(startDate, endDate, 9);
                            PeriodHours hours = mDataService.GetPeriodHours(startDate, endDate, 9);

                            if (PeriodDaysdic.ContainsKey(item.PeriodKey))
                            {
                                dayobj = PeriodDaysdic[item.PeriodKey];
                            }
                            if (item.ClassType == "PeakWE")
                            {
                                ftrdataItem.MWHTotal = item.MW * hours.peakWEHours;
                            }
                            else if (item.ClassType == "Off-peak" || item.ClassType == "OFF - PEAK" || item.ClassType == "OFFPEAK")
                            {
                                ftrdataItem.MWHTotal = item.MW * hours.offpeakHours;
                            }
                            else if (item.ClassType == "PeakWD" || item.ClassType.ToUpper() == "PEAK" || item.ClassType.ToUpper() == "ONPEAK" || item.ClassType.ToUpper() == "PEAKWD" || item.ClassType.ToUpper() == "PEAKWE")
                            {
                                ftrdataItem.MWHTotal = item.MW * hours.peakHours;
                            }

                            ftrdataItem.SourceZone = item.SourceZone;
                            ftrdataItem.SinkZone = item.SinkZone;
                            if (PNLChecked == true)
                            {
                                foreach (KeyValuePair<DateTime, double> eachdayPNL in item.dailyPNL)
                                {
                                    int day = eachdayPNL.Key.Day;
                                    switch (day)
                                    {
                                        case 1:
                                            ftrdataItem.Day1 = eachdayPNL.Value;
                                            break;
                                        case 2:
                                            ftrdataItem.Day2 = eachdayPNL.Value;
                                            break;
                                        case 3:
                                            ftrdataItem.Day3 = eachdayPNL.Value;
                                            break;
                                        case 4:
                                            ftrdataItem.Day4 = eachdayPNL.Value;
                                            break;
                                        case 5:
                                            ftrdataItem.Day5 = eachdayPNL.Value;
                                            break;
                                        case 6:
                                            ftrdataItem.Day6 = eachdayPNL.Value;
                                            break;
                                        case 7:
                                            ftrdataItem.Day7 = eachdayPNL.Value;
                                            break;
                                        case 8:
                                            ftrdataItem.Day8 = eachdayPNL.Value;
                                            break;
                                        case 9:
                                            ftrdataItem.Day9 = eachdayPNL.Value;
                                            break;
                                        case 10:
                                            ftrdataItem.Day10 = eachdayPNL.Value;
                                            break;
                                        case 11:
                                            ftrdataItem.Day11 = eachdayPNL.Value;
                                            break;
                                        case 12:
                                            ftrdataItem.Day12 = eachdayPNL.Value;
                                            break;
                                        case 13:
                                            ftrdataItem.Day13 = eachdayPNL.Value;
                                            break;
                                        case 14:
                                            ftrdataItem.Day14 = eachdayPNL.Value;
                                            break;
                                        case 15:
                                            ftrdataItem.Day15 = eachdayPNL.Value;
                                            break;
                                        case 16:
                                            ftrdataItem.Day16 = eachdayPNL.Value;
                                            break;
                                        case 17:
                                            ftrdataItem.Day17 = eachdayPNL.Value;
                                            break;
                                        case 18:
                                            ftrdataItem.Day18 = eachdayPNL.Value;
                                            break;
                                        case 19:
                                            ftrdataItem.Day19 = eachdayPNL.Value;
                                            break;
                                        case 20:
                                            ftrdataItem.Day20 = eachdayPNL.Value;
                                            break;
                                        case 21:
                                            ftrdataItem.Day21 = eachdayPNL.Value;
                                            break;
                                        case 22:
                                            ftrdataItem.Day22 = eachdayPNL.Value;
                                            break;
                                        case 23:
                                            ftrdataItem.Day23 = eachdayPNL.Value;
                                            break;
                                        case 24:
                                            ftrdataItem.Day24 = eachdayPNL.Value;
                                            break;
                                        case 25:
                                            ftrdataItem.Day25 = eachdayPNL.Value;
                                            break;
                                        case 26:
                                            ftrdataItem.Day26 = eachdayPNL.Value;
                                            break;
                                        case 27:
                                            ftrdataItem.Day27 = eachdayPNL.Value;
                                            break;
                                        case 28:
                                            ftrdataItem.Day28 = eachdayPNL.Value;
                                            break;
                                        case 29:
                                            ftrdataItem.Day29 = eachdayPNL.Value;
                                            break;
                                        case 30:
                                            ftrdataItem.Day30 = eachdayPNL.Value;
                                            break;
                                        case 31:
                                            ftrdataItem.Day31 = eachdayPNL.Value;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else if (CostChecked == true)
                            {
                                double cost = item.Costmonthlytotal;
                                double daycost = 0;
                                PeriodDays dayobj1 = new PeriodDays();
                                if (PeriodDaysdic.ContainsKey(item.PeriodKey))
                                {
                                    dayobj1 = PeriodDaysdic[item.PeriodKey];
                                }
                                if (item.ClassType == "PeakWE")
                                {
                                    daycost = cost / dayobj1.PeakWEDay;
                                }
                                else if (item.ClassType == "Off-peak" || item.ClassType == "OFF - PEAK" || item.ClassType == "OFFPEAK")
                                {
                                    daycost = cost / dayobj1.OffPeakDay;
                                }
                                else if (item.ClassType == "PeakWD" || item.ClassType.ToUpper() == "PEAK" || item.ClassType.ToUpper() == "ONPEAK" || item.ClassType.ToUpper() == "PEAKWD" || item.ClassType.ToUpper() == "PEAKWE")
                                {
                                    daycost = cost / dayobj1.PeakDay;
                                }
                                foreach (KeyValuePair<DateTime, double> eachdayPNL in item.dailyCost)
                                {
                                    int day = eachdayPNL.Key.Day;
                                    switch (day)
                                    {
                                        case 1:
                                            ftrdataItem.Day1 = daycost;
                                            break;
                                        case 2:
                                            ftrdataItem.Day2 = daycost;
                                            break;
                                        case 3:
                                            ftrdataItem.Day3 = daycost;
                                            break;
                                        case 4:
                                            ftrdataItem.Day4 = daycost;
                                            break;
                                        case 5:
                                            ftrdataItem.Day5 = daycost;
                                            break;
                                        case 6:
                                            ftrdataItem.Day6 = daycost;
                                            break;
                                        case 7:
                                            ftrdataItem.Day7 = daycost;
                                            break;
                                        case 8:
                                            ftrdataItem.Day8 = daycost;
                                            break;
                                        case 9:
                                            ftrdataItem.Day9 = daycost;
                                            break;
                                        case 10:
                                            ftrdataItem.Day10 = daycost;
                                            break;
                                        case 11:
                                            ftrdataItem.Day11 = daycost;
                                            break;
                                        case 12:
                                            ftrdataItem.Day12 = daycost;
                                            break;
                                        case 13:
                                            ftrdataItem.Day13 = daycost;
                                            break;
                                        case 14:
                                            ftrdataItem.Day14 = daycost;
                                            break;
                                        case 15:
                                            ftrdataItem.Day15 = daycost;
                                            break;
                                        case 16:
                                            ftrdataItem.Day16 = daycost;
                                            break;
                                        case 17:
                                            ftrdataItem.Day17 = daycost;
                                            break;
                                        case 18:
                                            ftrdataItem.Day18 = daycost;
                                            break;
                                        case 19:
                                            ftrdataItem.Day19 = daycost;
                                            break;
                                        case 20:
                                            ftrdataItem.Day20 = daycost;
                                            break;
                                        case 21:
                                            ftrdataItem.Day21 = daycost;
                                            break;
                                        case 22:
                                            ftrdataItem.Day22 = daycost;
                                            break;
                                        case 23:
                                            ftrdataItem.Day23 = daycost;
                                            break;
                                        case 24:
                                            ftrdataItem.Day24 = daycost;
                                            break;
                                        case 25:
                                            ftrdataItem.Day25 = daycost;
                                            break;
                                        case 26:
                                            ftrdataItem.Day26 = daycost;
                                            break;
                                        case 27:
                                            ftrdataItem.Day27 = daycost;
                                            break;
                                        case 28:
                                            ftrdataItem.Day28 = daycost;
                                            break;
                                        case 29:
                                            ftrdataItem.Day29 = daycost;
                                            break;
                                        case 30:
                                            ftrdataItem.Day30 = daycost;
                                            break;
                                        case 31:
                                            ftrdataItem.Day31 = daycost;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else if (MWhChecked == true)
                            {

                                double mw = item.MW;
                                double daymw = 0;
                                if (item.ClassType == "PeakWE")
                                {
                                    daymw = mw * 16;
                                }
                                else if (item.ClassType == "Off-peak" || item.ClassType == "OFF - PEAK" || item.ClassType == "OFFPEAK")
                                {
                                    if (hours.peakHours == 247)
                                    {
                                        daymw = mw * 7;
                                    }
                                    else
                                    {
                                        daymw = mw * 8;
                                    }
                                }
                                else if (item.ClassType == "PeakWD" || item.ClassType.ToUpper() == "PEAK" || item.ClassType.ToUpper() == "ONPEAK" || item.ClassType.ToUpper() == "PEAKWD" || item.ClassType.ToUpper() == "PEAKWE")
                                {
                                    daymw = mw * 16;
                                }
                                foreach (KeyValuePair<DateTime, double> eachdayPNL in item.dailyCost)
                                {
                                    int day = eachdayPNL.Key.Day;
                                    switch (day)
                                    {
                                        case 1:
                                            ftrdataItem.Day1 = daymw;
                                            break;
                                        case 2:
                                            ftrdataItem.Day2 = daymw;
                                            break;
                                        case 3:
                                            ftrdataItem.Day3 = daymw;
                                            break;
                                        case 4:
                                            ftrdataItem.Day4 = daymw;
                                            break;
                                        case 5:
                                            ftrdataItem.Day5 = daymw;
                                            break;
                                        case 6:
                                            ftrdataItem.Day6 = daymw;
                                            break;
                                        case 7:
                                            ftrdataItem.Day7 = daymw;
                                            break;
                                        case 8:
                                            ftrdataItem.Day8 = daymw;
                                            break;
                                        case 9:
                                            ftrdataItem.Day9 = daymw;
                                            break;
                                        case 10:
                                            ftrdataItem.Day10 = daymw;
                                            break;
                                        case 11:
                                            ftrdataItem.Day11 = daymw;
                                            break;
                                        case 12:
                                            ftrdataItem.Day12 = daymw;
                                            break;
                                        case 13:
                                            ftrdataItem.Day13 = daymw;
                                            break;
                                        case 14:
                                            ftrdataItem.Day14 = daymw;
                                            break;
                                        case 15:
                                            ftrdataItem.Day15 = daymw;
                                            break;
                                        case 16:
                                            ftrdataItem.Day16 = daymw;
                                            break;
                                        case 17:
                                            ftrdataItem.Day17 = daymw;
                                            break;
                                        case 18:
                                            ftrdataItem.Day18 = daymw;
                                            break;
                                        case 19:
                                            ftrdataItem.Day19 = daymw;
                                            break;
                                        case 20:
                                            ftrdataItem.Day20 = daymw;
                                            break;
                                        case 21:
                                            ftrdataItem.Day21 = daymw;
                                            break;
                                        case 22:
                                            ftrdataItem.Day22 = daymw;
                                            break;
                                        case 23:
                                            ftrdataItem.Day23 = daymw;
                                            break;
                                        case 24:
                                            ftrdataItem.Day24 = daymw;
                                            break;
                                        case 25:
                                            ftrdataItem.Day25 = daymw;
                                            break;
                                        case 26:
                                            ftrdataItem.Day26 = daymw;
                                            break;
                                        case 27:
                                            ftrdataItem.Day27 = daymw;
                                            break;
                                        case 28:
                                            ftrdataItem.Day28 = daymw;
                                            break;
                                        case 29:
                                            ftrdataItem.Day29 = daymw;
                                            break;
                                        case 30:
                                            ftrdataItem.Day30 = daymw;
                                            break;
                                        case 31:
                                            ftrdataItem.Day31 = daymw;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else if (DAPriceChecked == true)
                            {
                                foreach (KeyValuePair<DateTime, double> dayDAPrice in item.dailyDAPrice)
                                {
                                    int day = dayDAPrice.Key.Day;
                                    switch (day)
                                    {
                                        case 1:
                                            ftrdataItem.Day1 = dayDAPrice.Value;
                                            break;
                                        case 2:
                                            ftrdataItem.Day2 = dayDAPrice.Value;
                                            break;
                                        case 3:
                                            ftrdataItem.Day3 = dayDAPrice.Value;
                                            break;
                                        case 4:
                                            ftrdataItem.Day4 = dayDAPrice.Value;
                                            break;
                                        case 5:
                                            ftrdataItem.Day5 = dayDAPrice.Value;
                                            break;
                                        case 6:
                                            ftrdataItem.Day6 = dayDAPrice.Value;
                                            break;
                                        case 7:
                                            ftrdataItem.Day7 = dayDAPrice.Value;
                                            break;
                                        case 8:
                                            ftrdataItem.Day8 = dayDAPrice.Value;
                                            break;
                                        case 9:
                                            ftrdataItem.Day9 = dayDAPrice.Value;
                                            break;
                                        case 10:
                                            ftrdataItem.Day10 = dayDAPrice.Value;
                                            break;
                                        case 11:
                                            ftrdataItem.Day11 = dayDAPrice.Value;
                                            break;
                                        case 12:
                                            ftrdataItem.Day12 = dayDAPrice.Value;
                                            break;
                                        case 13:
                                            ftrdataItem.Day13 = dayDAPrice.Value;
                                            break;
                                        case 14:
                                            ftrdataItem.Day14 = dayDAPrice.Value;
                                            break;
                                        case 15:
                                            ftrdataItem.Day15 = dayDAPrice.Value;
                                            break;
                                        case 16:
                                            ftrdataItem.Day16 = dayDAPrice.Value;
                                            break;
                                        case 17:
                                            ftrdataItem.Day17 = dayDAPrice.Value;
                                            break;
                                        case 18:
                                            ftrdataItem.Day18 = dayDAPrice.Value;
                                            break;
                                        case 19:
                                            ftrdataItem.Day19 = dayDAPrice.Value;
                                            break;
                                        case 20:
                                            ftrdataItem.Day20 = dayDAPrice.Value;
                                            break;
                                        case 21:
                                            ftrdataItem.Day21 = dayDAPrice.Value;
                                            break;
                                        case 22:
                                            ftrdataItem.Day22 = dayDAPrice.Value;
                                            break;
                                        case 23:
                                            ftrdataItem.Day23 = dayDAPrice.Value;
                                            break;
                                        case 24:
                                            ftrdataItem.Day24 = dayDAPrice.Value;
                                            break;
                                        case 25:
                                            ftrdataItem.Day25 = dayDAPrice.Value;
                                            break;
                                        case 26:
                                            ftrdataItem.Day26 = dayDAPrice.Value;
                                            break;
                                        case 27:
                                            ftrdataItem.Day27 = dayDAPrice.Value;
                                            break;
                                        case 28:
                                            ftrdataItem.Day28 = dayDAPrice.Value;
                                            break;
                                        case 29:
                                            ftrdataItem.Day29 = dayDAPrice.Value;
                                            break;
                                        case 30:
                                            ftrdataItem.Day30 = dayDAPrice.Value;
                                            break;
                                        case 31:
                                            ftrdataItem.Day31 = dayDAPrice.Value;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (KeyValuePair<DateTime, double> dayDAPrice in item.dailyRTPrice)
                                {
                                    int day = dayDAPrice.Key.Day;
                                    switch (day)
                                    {
                                        case 1:
                                            ftrdataItem.Day1 = dayDAPrice.Value;
                                            break;
                                        case 2:
                                            ftrdataItem.Day2 = dayDAPrice.Value;
                                            break;
                                        case 3:
                                            ftrdataItem.Day3 = dayDAPrice.Value;
                                            break;
                                        case 4:
                                            ftrdataItem.Day4 = dayDAPrice.Value;
                                            break;
                                        case 5:
                                            ftrdataItem.Day5 = dayDAPrice.Value;
                                            break;
                                        case 6:
                                            ftrdataItem.Day6 = dayDAPrice.Value;
                                            break;
                                        case 7:
                                            ftrdataItem.Day7 = dayDAPrice.Value;
                                            break;
                                        case 8:
                                            ftrdataItem.Day8 = dayDAPrice.Value;
                                            break;
                                        case 9:
                                            ftrdataItem.Day9 = dayDAPrice.Value;
                                            break;
                                        case 10:
                                            ftrdataItem.Day10 = dayDAPrice.Value;
                                            break;
                                        case 11:
                                            ftrdataItem.Day11 = dayDAPrice.Value;
                                            break;
                                        case 12:
                                            ftrdataItem.Day12 = dayDAPrice.Value;
                                            break;
                                        case 13:
                                            ftrdataItem.Day13 = dayDAPrice.Value;
                                            break;
                                        case 14:
                                            ftrdataItem.Day14 = dayDAPrice.Value;
                                            break;
                                        case 15:
                                            ftrdataItem.Day15 = dayDAPrice.Value;
                                            break;
                                        case 16:
                                            ftrdataItem.Day16 = dayDAPrice.Value;
                                            break;
                                        case 17:
                                            ftrdataItem.Day17 = dayDAPrice.Value;
                                            break;
                                        case 18:
                                            ftrdataItem.Day18 = dayDAPrice.Value;
                                            break;
                                        case 19:
                                            ftrdataItem.Day19 = dayDAPrice.Value;
                                            break;
                                        case 20:
                                            ftrdataItem.Day20 = dayDAPrice.Value;
                                            break;
                                        case 21:
                                            ftrdataItem.Day21 = dayDAPrice.Value;
                                            break;
                                        case 22:
                                            ftrdataItem.Day22 = dayDAPrice.Value;
                                            break;
                                        case 23:
                                            ftrdataItem.Day23 = dayDAPrice.Value;
                                            break;
                                        case 24:
                                            ftrdataItem.Day24 = dayDAPrice.Value;
                                            break;
                                        case 25:
                                            ftrdataItem.Day25 = dayDAPrice.Value;
                                            break;
                                        case 26:
                                            ftrdataItem.Day26 = dayDAPrice.Value;
                                            break;
                                        case 27:
                                            ftrdataItem.Day27 = dayDAPrice.Value;
                                            break;
                                        case 28:
                                            ftrdataItem.Day28 = dayDAPrice.Value;
                                            break;
                                        case 29:
                                            ftrdataItem.Day29 = dayDAPrice.Value;
                                            break;
                                        case 30:
                                            ftrdataItem.Day30 = dayDAPrice.Value;
                                            break;
                                        case 31:
                                            ftrdataItem.Day31 = dayDAPrice.Value;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                            }
                            // ftrdataItem.MTDTotal = item.PNLmonthlytotal;
                            ftrdataItem.MTDTotal = (ftrdataItem.Day1 == null ? 0 : ftrdataItem.Day1) + (ftrdataItem.Day2 == null ? 0 : ftrdataItem.Day2) + (ftrdataItem.Day3 == null ? 0 : ftrdataItem.Day3) + (ftrdataItem.Day4 == null ? 0 : ftrdataItem.Day4)
                                                  + (ftrdataItem.Day5 == null ? 0 : ftrdataItem.Day5) + (ftrdataItem.Day6 == null ? 0 : ftrdataItem.Day6) + (ftrdataItem.Day7 == null ? 0 : ftrdataItem.Day7) + (ftrdataItem.Day8 == null ? 0 : ftrdataItem.Day8)
                                                  + (ftrdataItem.Day9 == null ? 0 : ftrdataItem.Day9) + (ftrdataItem.Day10 == null ? 0 : ftrdataItem.Day10) + (ftrdataItem.Day11 == null ? 0 : ftrdataItem.Day11) + (ftrdataItem.Day12 == null ? 0 : ftrdataItem.Day12)
                                                  + (ftrdataItem.Day13 == null ? 0 : ftrdataItem.Day13) + (ftrdataItem.Day14 == null ? 0 : ftrdataItem.Day14) + (ftrdataItem.Day15 == null ? 0 : ftrdataItem.Day15) + (ftrdataItem.Day16 == null ? 0 : ftrdataItem.Day16)
                                                  + (ftrdataItem.Day17 == null ? 0 : ftrdataItem.Day17) + (ftrdataItem.Day18 == null ? 0 : ftrdataItem.Day18) + (ftrdataItem.Day19 == null ? 0 : ftrdataItem.Day19) + (ftrdataItem.Day20 == null ? 0 : ftrdataItem.Day20)
                                                  + (ftrdataItem.Day21 == null ? 0 : ftrdataItem.Day21) + (ftrdataItem.Day22 == null ? 0 : ftrdataItem.Day22) + (ftrdataItem.Day23 == null ? 0 : ftrdataItem.Day23) + (ftrdataItem.Day24 == null ? 0 : ftrdataItem.Day24)
                                                  + (ftrdataItem.Day25 == null ? 0 : ftrdataItem.Day25) + (ftrdataItem.Day26 == null ? 0 : ftrdataItem.Day26) + (ftrdataItem.Day27 == null ? 0 : ftrdataItem.Day27) + (ftrdataItem.Day28 == null ? 0 : ftrdataItem.Day28)
                                                  + (ftrdataItem.Day29 == null ? 0 : ftrdataItem.Day29) + (ftrdataItem.Day30 == null ? 0 : ftrdataItem.Day30) + (ftrdataItem.Day31 == null ? 0 : ftrdataItem.Day31);
                            tempFTRDataList.Add(ftrdataItem);
                        }
                        FTRDetailsDataList = null;
                        FTRDetailsDataList = AddTotalRow(tempFTRDataList).ToList();

                        //if(MWhChecked)
                        //{
                        //    FTRDetailsDataListMWH = null;
                        //    FTRDetailsDataListMWH = AddTotalRow(tempFTRDataList).ToList();
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Adds the total row.
        /// </summary>
        /// <param name="tempFTRDataList">The temporary FTR data list.</param>
        /// <returns></returns>
        private List<FTRDetailsData> AddTotalRow(List<FTRDetailsData> tempFTRDataList)
        {
            List<FTRDetailsData> finalListData = new List<FTRDetailsData>();
            tempFTRDataList.ForEach(m =>
            {
                if (tempFTRDataList.FindIndex(a => a == m) == 0)
                {
                    finalListData.Add(GetTotalValue(tempFTRDataList));
                    finalListData.Add(m);
                }
                else
                {
                    finalListData.Add(m);
                }
            });
            return finalListData;
        }
        /// <summary>
        /// Retrieves the data.
        /// </summary>
        ///

        
        private string BuildAuctionRounds()
        {
            List<string> rounds = new List<string>();

            if (Seq1checked) rounds.Add("1");
            if (Seq2checked) rounds.Add("2");
            if (Seq3checked) rounds.Add("3");
            if (Seq4checked) rounds.Add("4");
            if (Seq5checked) rounds.Add("5");
            if (Seq6checked) rounds.Add("6");

            return string.Join(",", rounds);
        }
        // public ObservableCollection<SourceSink> SourceSinks { get; set; }

        //private async void RetrieveData()
        //{
        //    try
        //    {
        //        if (DateSelectedList != null)
        //        {
        //            List<SourceSink> tempSourceSinkList = new List<SourceSink>();
        //            int marketkey = mMarkets[MarketSelected];

        //            List<string> accounts = new List<string>();
        //            List<SourceSink> sourcesink = new List<SourceSink>();
        //            List<SourceSink> sourcesinkAnnual = new List<SourceSink>();

        //            for (int j = 0; j < this.ParticipantsSelectedList.Count; j++)
        //            {
        //                accounts.Add(ParticipantsSelectedList[j].ToString());
        //            }

        //            // 🔥 batching
        //            int batchSize = 30;

        //            var batches = accounts
        //                .Select((x, i) => new { x, i })
        //                .GroupBy(x => x.i / batchSize)
        //                .Select(g => g.Select(x => x.x).ToList())
        //                .ToList();

        //            // ✅ Ensure ObservableCollection
        //            if (SourceSinks == null)
        //                SourceSinks = new ObservableCollection<SourceSink>();
        //            else
        //                SourceSinks.Clear();

        //            foreach (string dateSelected in DateSelectedList)
        //            {
        //                DateTime period = DateTime.Parse(dateSelected);

        //                foreach (var accountBatch in batches)
        //                {
        //                    await Task.Run(() =>
        //                    {
        //                        try
        //                        {
        //                            // =====================
        //                            // Monthly Data
        //                            // =====================
        //                            sourcesink = mDataService.GetFTRs(marketkey, accountBatch, period);

        //                            lock (tempSourceSinkList)
        //                            {
        //                                tempSourceSinkList.AddRange(sourcesink);
        //                            }

        //                            // 🔥 UI Update (WPF SAFE)
        //                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
        //                            {
        //                                foreach (var item in sourcesink)
        //                                {
        //                                    SourceSinks.Add(item);
        //                                }
        //                            }));

        //                            // =====================
        //                            // Annual Data
        //                            // =====================
        //                            string auctRound = null;

        //                            if (Seq1checked) auctRound = "1";
        //                            if (Seq2checked) auctRound = auctRound + "," + "2";
        //                            if (Seq3checked) auctRound = auctRound + "," + "3";
        //                            if (Seq4checked) auctRound = auctRound + "," + "4";
        //                            if (Seq5checked) auctRound = auctRound + "," + "5";
        //                            if (Seq6checked) auctRound = auctRound + "," + "6";

        //                            if (!string.IsNullOrEmpty(auctRound) && auctRound[0] == ',')
        //                            {
        //                                auctRound = auctRound.Substring(1);
        //                            }

        //                            sourcesinkAnnual = mDataService.Get6MonthsCRRs(9, accountBatch, period, auctRound);

        //                            lock (tempSourceSinkList)
        //                            {
        //                                tempSourceSinkList.AddRange(sourcesinkAnnual);
        //                            }

        //                            // 🔥 UI Update (WPF SAFE)
        //                            App.Current.Dispatcher.BeginInvoke(new Action(() =>
        //                            {
        //                                foreach (var item in sourcesinkAnnual)
        //                                {
        //                                    SourceSinks.Add(item);
        //                                }
        //                            }));
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            // optional: log
        //                        }
        //                    });
        //                }
        //            }

        //            // ✅ Final full data for CSV
        //            SourceSinksTemp = tempSourceSinkList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // optional: log
        //    }
        //}

        /// <summary>
        /// working old code only issue in fetching big data 
        /// </summary>
        /// 
        private void RetrieveData()
        {
            try
            {
                if (DateSelectedList == null || DateSelectedList.Count == 0)
                    return;

                List<SourceSink> tempSourceSinkList = new List<SourceSink>();
                int marketkey = mMarkets[MarketSelected];

                List<string> accounts = new List<string>();
                foreach (var p in ParticipantsSelectedList)
                {
                    accounts.Add(p.ToString());
                }

                // CONNECT ONLY ONCE
                Connect();

                foreach (string dateSelected in DateSelectedList)
                {
                    DateTime period = DateTime.Parse(dateSelected);

                    try
                    {
                        // Monthly
                        var sourcesink = mFTRCalculationProxy.GetFTRs(marketkey, accounts, period);
                        if (sourcesink != null)
                            tempSourceSinkList.AddRange(sourcesink);

                        // Annual rounds
                        List<string> rounds = new List<string>();

                        if (Seq1checked) rounds.Add("1");
                        if (Seq2checked) rounds.Add("2");
                        if (Seq3checked) rounds.Add("3");
                        if (Seq4checked) rounds.Add("4");
                        if (Seq5checked) rounds.Add("5");
                        if (Seq6checked) rounds.Add("6");

                        string auctRound = string.Join(",", rounds);

                        if (!string.IsNullOrEmpty(auctRound))
                        {
                            var sourcesinkAnnual =
                                mFTRCalculationProxy.Get6MonthCRRs(9, accounts, period, auctRound);

                            if (sourcesinkAnnual != null)
                                tempSourceSinkList.AddRange(sourcesinkAnnual);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving data for {period}: {ex}");
                    }
                }

                SourceSinks = tempSourceSinkList.ToList();
                SourceSinksTemp = SourceSinks.ToList();

                // CLOSE CHANNEL PROPERLY
                if (mFTRCalculationProxy != null)
                {
                    var channel = (System.ServiceModel.IClientChannel)mFTRCalculationProxy;

                    if (channel.State == System.ServiceModel.CommunicationState.Opened)
                        channel.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RetrieveData failed: " + ex);
            }
        }
        //private void RetrieveData()
        //{
        //    try
        //    {
        //        if (DateSelectedList != null)
        //        {
        //            List<SourceSink> tempSourceSinkList = new List<SourceSink>();
        //            int marketkey = mMarkets[MarketSelected];
        //            List<string> accounts = new List<string>();
        //            List<SourceSink> sourcesink = new List<SourceSink>();
        //            List<SourceSink> sourcesinkAnnual = new List<SourceSink>();
        //            for (int j = 0; j < this.ParticipantsSelectedList.Count; j++)
        //            {
        //                accounts.Add(ParticipantsSelectedList[j].ToString());
        //            }
        //            //Monthly
        //            foreach (string dateSelected in DateSelectedList)
        //            {
        //                //mFTRCalculationProxy = null;
        //                //Connect();
        //                DateTime period = DateTime.Parse(dateSelected);
        //                try
        //                {
        //                    //sourcesink = mFTRCalculationProxy.GetFTRs(marketkey, accounts, period);
        //                    sourcesink = mDataService.GetFTRs(marketkey, accounts, period);
        //                    tempSourceSinkList.AddRange(sourcesink);
        //                    //mFTRCalculationProxy = null;
        //                    //Connect();
        //                    //Annualsp
        //                    string auctRound = null;
        //                    if (Seq1checked)
        //                        auctRound = "1";
        //                    if (Seq2checked)
        //                        auctRound = auctRound + "," + "2";
        //                    if (Seq3checked)
        //                        auctRound = auctRound + "," + "3";
        //                    if (Seq4checked)
        //                        auctRound = auctRound + "," + "4";
        //                    if (Seq5checked)
        //                        auctRound = auctRound + "," + "5";
        //                    if (Seq6checked)
        //                        auctRound = auctRound + "," + "6";
        //                    if (auctRound[0] == ',')
        //                    {
        //                        auctRound = auctRound.Substring(1);
        //                    }
        //                    List<int> auctionKeyList = new List<int>();
        //                    //sourcesinkAnnual = mFTRCalculationProxy.Get6MonthCRRs(9, accounts, period, auctRound);
        //                    sourcesinkAnnual = mDataService.Get6MonthsCRRs(9, accounts, period, auctRound);
        //                    tempSourceSinkList.AddRange(sourcesinkAnnual);
        //                }
        //                catch (Exception ex)
        //                {
        //                }
        //            }



        //            SourceSinks = tempSourceSinkList.ToList();
        //            SourceSinksTemp = SourceSinks.ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        /// <summary>
        /// Generates Month To Date.
        /// </summary>
        private void GenerateMTD()
        {
            DateSelectedList = null;
            List<string> tempList = new List<string>();
            string temp = DateTime.Now.ToString("M").Substring(0, 3);
            tempList.Add(temp + " " + DateTime.Now.Year.ToString());
            DateSelectedList = tempList.ToList();
            RemoveDuplicates();
        }
        /// <summary>
        /// Generates Year To Date.
        /// </summary>
        private void GenerateYTD()
        {
            DateSelectedList = null;
            List<string> tempList = new List<string>();
            DateTime firstmonth = new DateTime(DateTime.Now.Year, 1, 1);
            for (int i = 0; i < DateTime.Now.Month; i++)
            {
                string temp = firstmonth.AddMonths(i).ToString("M").Substring(0, 3);
                tempList.Add(temp + " " + DateTime.Now.Year.ToString());
            }
            DateSelectedList = tempList.ToList();
            RemoveDuplicates();
        }
        /// <summary>
        /// Removes the duplicates.
        /// </summary>
        private void RemoveDuplicates()
        {
            try
            {
                List<string> tempList = new List<string>();
                List<DateTime> datelist = new List<DateTime>();
                List<string> withdups = DateSelectedList.Cast<string>().ToList();
                DateSelectedList = null;
                withdups = withdups.Distinct().ToList();
                DateTime dateValue;
                foreach (string item in withdups)
                {
                    bool isDatetime = DateTime.TryParse(item, out dateValue);
                    if (isDatetime) datelist.Add(dateValue);
                }
                datelist.Sort();
                foreach (DateTime item in datelist)
                {
                    tempList.Add(item.ToString("M").Substring(0, 3) + " " + item.Year.ToString());
                }
                DateSelectedList = tempList.ToList();
            }
            catch
            {

            }
        }
        /// <summary>
        /// Moves the participants.
        /// </summary>
        private void MoveParticipants()
        {
            if (mTempSelectedList != null)
            {
                ParticipantsSelectedList = mTempSelectedList.ToList();
            }
            if (mTempSelected != null)
            {
                ParticipantsSelectedList = mTempSelected.ToList();
            }
        }
        /// <summary>
        /// Moves the dates.
        /// </summary>
        private void MoveDates()
        {

            if (mTempSelectedDates != null)
            {
                DateSelectedList = mTempSelectedDates.ToList();
            }
            if (mTempDateSelected != null)
            {
                List<string> itemsList = mTempDateSelected.ToList();
                List<string> finalItemList = new List<string>();
                foreach (var item in itemsList)
                {
                    string[] temp = item.ToString().Split(new char[] { ' ', ' ' });
                    switch (temp[0])
                    {
                        case "Q1-SUM":
                            finalItemList.Add("Jun " + temp[1]);
                            finalItemList.Add("Jul " + temp[1]);
                            finalItemList.Add("Aug " + temp[1]);
                            break;
                        case "Q2-FAL":
                            finalItemList.Add("Sep " + temp[1]);
                            finalItemList.Add("Oct " + temp[1]);
                            finalItemList.Add("Nov " + temp[1]);
                            break;
                        case "Q3-WIN":
                            finalItemList.Add("Dec " + temp[1]);
                            finalItemList.Add("Jan " + temp[2]);
                            finalItemList.Add("Feb " + temp[2]);
                            break;
                        case "Q4-SPR":
                            finalItemList.Add("Mar " + temp[2]);
                            finalItemList.Add("Apr " + temp[2]);
                            finalItemList.Add("May " + temp[2]);
                            break;
                        default:
                            finalItemList.Add(item);
                            break;
                    }

                }
                //RemoveDuplicates();
                DateSelectedList = finalItemList.Distinct().ToList();
            }
        }
        /// <summary>
        /// Deletes the participants.
        /// </summary>
        private void DeleteParticipants()
        {
            if (mTempDeleted != null)
            {
                if (mTempDeleted.Count > 0)
                {
                    List<string> tempList = new List<string>();
                    foreach (var item in ParticipantsSelectedList)
                    {
                        tempList.Add(item);
                    }
                    foreach (var item in mTempDeleted)
                    {
                        if (tempList.Contains(item))
                        {
                            tempList.Remove(item);
                        }
                    }

                    ParticipantsSelectedList = tempList.ToList();
                }
                else
                {
                    ParticipantsSelectedList = null;
                }

            }
            else
            {
                ParticipantsSelectedList = null;
            }
        }
        /// <summary>
        /// Deletes the dates.
        /// </summary>
        private void DeleteDates()
        {
            if (mTempDateDeleted != null)
            {
                if (mTempDateDeleted.Count > 0)
                {
                    List<string> tempList = new List<string>();
                    foreach (var item in DateSelectedList)
                    {
                        tempList.Add(item);
                    }
                    foreach (var item in mTempDateDeleted)
                    {
                        if (tempList.Contains(item))
                        {
                            tempList.Remove(item);
                        }
                    }

                    DateSelectedList = tempList.ToList();
                }
                else
                {
                    MessageBox.Show("Please select at least one month");
                    //DateSelectedList = null;
                }
            }
            else
            {
                MessageBox.Show("Please select at least one month");
                //DateSelectedList = null;
            }

        }
        /// <summary>
        /// Fills Participants List.
        /// </summary>
        /// <param name="PropertyChanged">The property changed.</param>
        private void SetListBox(string PropertyChanged)
        {
            //List<string> tempMarketList = new List<string> { "PJM", "MISO", "ERCOT", "SPP", "CAISO" }; 
            //List<string> tempMarketList = new List<string> { "PJM", "CAISO", "MISO" }; 
            List<string> tempMarketList = new List<string> { "ERCOT" };
            if (tempMarketList.Contains(PropertyChanged))
            {
                ParticipantsList = null;
                IntExtList = null;
                ParticipantsSelectedList = null;
                List<string> tempList = new List<string>() { "Internal", "External" };
                //List<string> tempList = new List<string>() { "External" }; 
                IntExtList = null;
                IntExtList = tempList.ToList();
            }
            else
            {
                ParticipantsList = null;
                if (PropertyChanged == "Internal")
                {
                    if (mAccounts != null && mAccounts.Count > 0 && mAccounts.ContainsKey(MarketSelected) && mAccounts[MarketSelected] != null)
                    {
                        List<string> tempParticipantList = new List<string>();
                        foreach (string user in mAccounts[MarketSelected])
                        {
                            tempParticipantList.Add(user);
                        }
                        ParticipantsList = tempParticipantList.ToList();
                    }
                }
                else
                {
                    if (MarketSelected == "ERCOT")
                    {
                        List<string> tempHolderList = new List<string>();
                        mDataService.GetERCOTAccountHolders((tempList, error) =>
                        {
                            if (error != null)
                            {
                                return;
                            }
                            tempHolderList = tempList.ToList();
                        });
                        ParticipantsList = tempHolderList.OrderBy(a => a.ToString()).ToList();
                    }
                    else
                    {
                        List<string> tempParticipantsList = new List<string>();
                        int key = 0;
                        if (!string.IsNullOrEmpty(MarketSelected))
                        {
                            key = mMarkets[MarketSelected];
                        }
                        if (key != 0)
                        {
                            mDataService.GetMarketParticipants((tempList, error) =>
                            {
                                if (error != null)
                                {
                                    return;
                                }
                                tempParticipantsList = tempList;
                            }, key);
                            ParticipantsList = tempParticipantsList.ToList();
                        }
                    }
                }
            }
            if (!((mUser == "Programmer1") || (mUser == "gojira") || (mUser.ToLower() == "sangramp") || (mUser.ToLower() == "Programmer2") || (mUser.ToLower() == "dilsha"))) //|| mUser == "dilsha"
            {
                if (ParticipantsList != null)
                {
                    ParticipantsList.Remove("VAYU_CRR_STRAT");
                    ParticipantsList.Remove("RISK_VAYU_CRR");
                }
            }
            if (ParticipantsList != null)
            {
                if (!((mUser == "Programmer1") || (mUser == "sangramp")))
                {
                    ParticipantsList.RemoveAll(x => x.ToUpper() == "ISO1");
                    ParticipantsList.RemoveAll(x => x.ToUpper() == "XISO2");
                }
            }
            PeriodDaysdic = new Dictionary<int, PeriodDays>();
            PeriodDaysdic = mDataService.GetPeriodDays(MarketSelected);
        }
        /// <summary>
        /// Called when Date is changed.
        /// </summary>
        private void DateChanged()
        {
            SelectedMonthList = null;
            List<string> tempSelectMonthList = new List<string>();
            try
            {
                if (IntExtSelected == null)
                {
                    MessageBox.Show("Please select Market Type");
                    SelectedDate = null;
                }
                else
                {
                    int key = 0;
                    if (!string.IsNullOrEmpty(MarketSelected))
                    {
                        key = mMarkets[MarketSelected];
                    }
                    string[] temp;
                    if (SelectedDate != null)
                    {
                        if (key == 1)
                        {

                            temp = SelectedDate.Split(new char[] { ' ', ' ' });

                        }
                        else
                        {

                            temp = SelectedDate.Split(new char[] { ' ' });
                        }

                        if (temp[0] == "FTR")
                        {
                            tempSelectMonthList.Add("Q1-SUM " + temp[1] + " " + temp[2]);
                            tempSelectMonthList.Add("Q2-FAL " + temp[1] + " " + temp[2]);
                            tempSelectMonthList.Add("Q3-WIN " + temp[1] + " " + temp[2]);
                            tempSelectMonthList.Add("Q4-SPR " + temp[1] + " " + temp[2]);

                            DateTime startmonth = new DateTime(Convert.ToInt32(temp[1]), 6, 1);
                            DateTime endmonth = new DateTime(Convert.ToInt32(temp[2]), 6, 1);

                            for (; startmonth < endmonth; startmonth = startmonth.AddMonths(1))
                            {
                                tempSelectMonthList.Add(startmonth.ToString("MMMM").Substring(0, 3) + " " + startmonth.Year.ToString());
                            }
                        }
                        else
                        {
                            DateTime startmonth = new DateTime(Convert.ToInt32(temp[1]), 1, 1);
                            DateTime endmonth = startmonth.AddYears(1);
                            for (; startmonth < endmonth; startmonth = startmonth.AddMonths(1))
                            {
                                tempSelectMonthList.Add(startmonth.ToString("MMMM").Substring(0, 3) + " " + startmonth.Year.ToString());
                            }
                        }

                    }



                    SelectedMonthList = tempSelectMonthList.ToList();
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Gets the total value.
        /// </summary>
        /// <param name="tempFTRDataList">The temporary FTR data list.</param>
        /// <returns></returns>
        private FTRDetailsData GetTotalValue(List<FTRDetailsData> tempFTRDataList)
        {
            FTRDetailsData totalData = new FTRDetailsData() { Participant = "TOTAL" };
            try
            {
                foreach (var propItem in totalData.GetType().GetProperties())
                {
                    if (propItem.PropertyType == typeof(double?) || propItem.PropertyType == typeof(double))
                    {
                        double totalSum = 0.0;
                        var tempColumnList = (from a in tempFTRDataList select a.GetType().GetProperty(propItem.Name).GetValue(a)).ToList();
                        tempColumnList.RemoveAll(a => a == null);
                        tempColumnList.ForEach(e =>
                        {
                            double outVal;
                            if (double.TryParse(e.ToString(), out outVal))
                                totalSum += outVal;
                        });
                        if (propItem.Name == "MWTotal")
                        {
                            MWTotal = totalSum;
                        }
                        if (propItem.Name == "Cost")
                        {
                            CostTotal = totalSum;
                        }
                        if (propItem.Name == "RTPrice")
                        {
                            RTTotal = totalSum;
                        }
                        if (propItem.Name == "DAPrice")
                        {
                            DATotal = totalSum;
                        }
                        if (propItem.Name == "PNL")
                        {
                            PNLTotal = totalSum;
                        }
                        totalData.GetType().GetProperty(propItem.Name).SetValue(totalData, totalSum);
                    }
                }
            }
            catch
            {
            }
            return totalData;
        }
        /// <summary>
        /// Hides the columns.
        /// </summary>
        private void HideColumns()
        {
            if (MarketSelected == "PJM")
            {
                ClearingPriceVisible = Visibility.Hidden;
            }
            else if (MarketSelected == "MISO")
            {
                SourceZoneVisible = Visibility.Hidden;
                SinkZoneVisible = Visibility.Hidden;
            }
            else
            {
                ClearingPriceVisible = Visibility.Hidden;
                PeriodTypeVisible = Visibility.Hidden;
            }

        }
        /// <summary>
        /// Combines this instance.
        /// </summary>
        private void Combine()
        {
            if (CombineButtonText == "Restore")
            {
                GenerateReport();
                CombineButtonText = "Combine";
            }
            else
            {
                List<string> cols = new List<string>() { "Participant", "Month", "Auction", "PeriodType", "TradeType", "HedgeType", "ClassType", "Source", "Sink" };
                //mFtrFilterModel = new Vayu.CRRPNLDetails.ViewModels.FTRFilterViewModel(new Vayu.CRRPNLDetails.Model.DataService(), cols) { ParentModel = this };
                mFtrFilterModel = new Vayu.CRRPNLDetails.ViewModels.FTRFilterViewModel(mDataService, cols) { ParentModel = this };
                mFtrfilterWindow = new Vayu.CRRPNLDetails.Views.FTRDetailsFormFilter();
                mFtrfilterWindow.DataContext = mFtrFilterModel;
                mFtrfilterWindow.Show();
            }
        }
        /// <summary>
        /// Filters the data list.
        /// </summary>
        /// <param name="cols">The cols.</param>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private List<FTRDetailsData> FilterDataList(List<string> cols, List<FTRDetailsData> list)
        {
            var linq = from row in list
                       group row by new
                       {
                           ParticipantColumn = (cols.Contains("Participant")) ? row.Participant : string.Empty,
                           MonthColumn = (cols.Contains("Month")) ? row.Month : string.Empty,
                           AuctionColumn = (cols.Contains("Auction")) ? row.Auction : string.Empty,
                           PeriodTypeColumn = (cols.Contains("PeriodType")) ? row.PeriodType : string.Empty,
                           TradeTypeColumn = (cols.Contains("TradeType")) ? row.TradeType : string.Empty,
                           HedgeTypeColumn = (cols.Contains("HedgeType")) ? row.HedgeType : string.Empty,
                           ClassColumn = (cols.Contains("ClassType")) ? row.ClassType : string.Empty,
                           SourceColumn = (cols.Contains("Source")) ? row.Source : string.Empty,
                           SinkColumn = (cols.Contains("Sink")) ? row.Sink : string.Empty,
                           //ClearingPriceColumn = (cols.Contains("Clearing")) ? row.ClearingPrice : 0.0
                           //SourceZoneColumn = (cols.Contains("SourceZone")) ? row.SourceZone: string.Empty,
                           //SinkZoneColumn = (cols.Contains("SinkZoneColumn")) ? row.Field<string>("SinkZoneColumn") : string.Empty,
                       } into g
                       select new
                       {
                           ParticipantColumn = g.Key.ParticipantColumn,
                           MonthColumn = g.Key.MonthColumn,
                           AuctionColumn = g.Key.AuctionColumn,
                           PeriodTypeColumn = g.Key.PeriodTypeColumn,
                           TradeTypeColumn = g.Key.TradeTypeColumn,
                           HedgeTypeColumn = g.Key.HedgeTypeColumn,
                           ClassColumn = g.Key.ClassColumn,
                           SourceColumn = g.Key.SourceColumn,
                           //SourceZoneColumn = g.Key.SourceZoneColumn,
                           SinkColumn = g.Key.SinkColumn,
                           //SinkZoneColumn = g.Key.SinkZoneColumn,
                           //HoursColumn = g.Sum(x => x.hour),
                           ClearingPrice = g.Sum(k => k.ClearingPrice),
                           MWColumn = g.Sum(y => y.MWTotal),
                           CostColumn = g.Sum(z => z.Cost),
                           PNLColumn = g.Sum(a => a.PNL),
                           DAColumn = g.Sum(b => b.DAPrice),
                           RTColumn = g.Sum(b => b.RTPrice),
                           MTDColumn = g.Sum(c => c.MTDTotal),
                           data = g
                       };
            List<FTRDetailsData> combineDetails = new List<FTRDetailsData>();
            foreach (var item in linq)
            {
                try
                {
                    FTRDetailsData data = new FTRDetailsData();
                    data.Participant = item.ParticipantColumn;
                    data.Month = item.MonthColumn;
                    data.Auction = item.AuctionColumn;
                    data.PeriodType = item.PeriodTypeColumn;
                    data.HedgeType = item.HedgeTypeColumn;
                    data.TradeType = item.TradeTypeColumn;
                    data.ClassType = item.ClassColumn;
                    data.Source = item.SourceColumn;
                    data.ClearingPrice = item.ClearingPrice;
                    data.Sink = item.SinkColumn;
                    data.MWTotal = item.MWColumn;
                    data.Cost = item.CostColumn;
                    data.DAPrice = item.DAColumn;
                    data.RTPrice = item.RTColumn;
                    data.PNL = item.PNLColumn;
                    data.MTDTotal = item.MTDColumn;
                    FTRDetailsData datanew = GetTotalValue(item.data.ToList());
                    foreach (var propItem in datanew.GetType().GetProperties())
                    {
                        if (propItem.Name.Contains("Day"))
                            data.GetType().GetProperty(propItem.Name).SetValue(data, datanew.GetType().GetProperty(propItem.Name).GetValue(datanew));
                    }
                    combineDetails.Add(data);
                }
                catch
                {
                }
            }
            return combineDetails;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Selects all participants.
        /// </summary>
        public void SelectAllParticipants()
        {
            if (ParticipantsList != null)
            {
                mTempSelectedList = ParticipantsList.ToList();
            }
        }
        /// <summary>
        /// Selects all dates.
        /// </summary>
        public void SelectAllDates()
        {
            if (SelectedMonthList != null)
            {
                mTempSelectedDates = SelectedMonthList.ToList();
            }
        }
        /// <summary>
        /// Selecteds the participant.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        public void SelectedParticipant(List<string> selectedItems)
        {
            mTempSelected = selectedItems.ToList();
        }
        /// <summary>
        /// Selecteds the dates.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        public void SelectedDates(List<string> selectedItems)
        {
            mTempDateSelected = selectedItems.ToList();
        }
        /// <summary>
        /// Deletes the participant.
        /// </summary>
        /// <param name="deleteItems">The delete items.</param>
        public void DeleteParticipant(List<string> deleteItems)
        {
            mTempDeleted = deleteItems.ToList();
        }
        /// <summary>
        /// Deletes the dates.
        /// </summary>
        /// <param name="deleteItems">The delete items.</param>
        public void DeleteDates(List<string> deleteItems)
        {
            mTempDateDeleted = deleteItems.ToList();
        }
        /// <summary>
        /// Combineds the specified cols.
        /// </summary>
        /// <param name="cols">The cols.</param>
        public void Combined(List<string> cols)
        {
            var list = FTRDetailsDataList.Where(m => !m.Participant.ToLower().Contains("total")).ToList();
            List<FTRDetailsData> combineDetails = FilterDataList(cols, list);
            FTRDetailsDataList = AddTotalRow(combineDetails.ToList()).ToList();
            CombineButtonText = "Restore";
        }

        #endregion

        /// <summary>
        /// Sorts FTR Details Data.
        /// </summary>
        /// <param name="tempData">The temporary data.</param>
        /// <param name="orderDirection">The order direction.</param>
        internal void HandleSort(List<FTRDetailsData> tempData, System.ComponentModel.ListSortDirection orderDirection)
        {
            if (tempData != null)
            {
                FTRDetailsData totalItem = tempData.Where(m => m.Participant.ToLower().Contains("total")).FirstOrDefault();
                if (totalItem != null)
                {
                    tempData.Remove(totalItem);
                }
                var secondHalf = tempData.Where(m => m.GetType().GetProperty(SortCondition).GetValue(m) == null).ToList();
                var firstHalf = tempData.Where(m => m.GetType().GetProperty(SortCondition).GetValue(m) != null).ToList();
                List<FTRDetailsData> tempList = new List<FTRDetailsData>();
                if (orderDirection == System.ComponentModel.ListSortDirection.Ascending)
                {
                    firstHalf = firstHalf.OrderBy(m => m.GetType().GetProperty(SortCondition).GetValue(m)).ToList();
                }
                else
                {
                    firstHalf = firstHalf.OrderByDescending(m => m.GetType().GetProperty(SortCondition).GetValue(m)).ToList();
                }
                firstHalf.Insert(0, totalItem);
                tempList = firstHalf.Concat(secondHalf).ToList();
                FTRDetailsDataList = tempList.ToList();

            }

        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class DataGridBackColorConverter : IValueConverter
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
            SolidColorBrush mybrush = new SolidColorBrush();
            double number;

            if (value != null)
            {
                if (value.ToString().Contains("($"))
                {
                    mybrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    double.TryParse(value.ToString(), out number);
                    if (number == 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number > 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (number < 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                }

                if (value.ToString().Contains("SELL"))
                {
                    mybrush = new SolidColorBrush(Colors.Blue);
                }
                if (value.ToString().Contains("BUY"))
                {
                    mybrush = new SolidColorBrush(Colors.Black);
                }

            }
            return mybrush;
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
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ValueToForegroundColorConverter : IValueConverter
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

            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            double doubleValue = 0.0;
            double.TryParse(value.ToString(), out doubleValue);
            if (doubleValue < 0)
            {
                brush = new SolidColorBrush(Colors.Red);
            }
            return brush;
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
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
