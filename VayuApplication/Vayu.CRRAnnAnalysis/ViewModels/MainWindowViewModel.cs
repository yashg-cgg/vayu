using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;
using System.Windows;
using Vayu.CRRAnnAnalysis.Model;
using Vayu.DBLibrary;

namespace Vayu.CRRAnnAnalysis.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Declaration

        private readonly IDataService _dataService;

        #endregion

        #region Delegate Command

        public DelegateCommand ResetCommand { get; }

        public DelegateCommand LoadCommand { get; }

        #endregion


        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;


            int currentYear = DateTime.Now.Year;
            YearList = new ObservableCollection<int>(
                Enumerable.Range(currentYear - 20, 30).Reverse()
                );

            StartYear = currentYear;
            EndYear = currentYear;

            MonthList = new ObservableCollection<string>(
                System.Globalization.CultureInfo.CurrentCulture
                .DateTimeFormat
                .MonthNames
                .Where(m => !string.IsNullOrEmpty(m)));

            SequenceList = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            SelectedSequence = 1;

            BuildLastMonthHeader();
            SelectedSemester = 1;
            ISOMarketList = new List<string> { "ERCOT" };
            MarketComboSelectedValue = "ERCOT";
            SelectedPeakType = "PeakWD";
            mFillSourceSinkHash = new Dictionary<string, SourceSinkState>();
            SetHedgeComboBox();
            ResetCommand = new DelegateCommand(Reset);

            LoadCommand = new DelegateCommand(LoadData, CanLoadData)
                              .ObservesProperty(() => StartYear)
                              .ObservesProperty(() => EndYear)
                              .ObservesProperty(() => SelectedPeakType)
                              .ObservesProperty(() => SelectedSemester)
                              .ObservesProperty(() => HedgeComboSelectedValue)
                              .ObservesProperty(() => SelectedSequence)
                              .ObservesProperty(() => SelectedMonth)
                              //.ObservesProperty(() => JanChecked)
                              //.ObservesProperty(() => FebChecked)
                              //.ObservesProperty(() => MarChecked)
                              //.ObservesProperty(() => AprChecked)
                              //.ObservesProperty(() => MayChecked)
                              //.ObservesProperty(() => JunChecked)
                              //.ObservesProperty(() => JulChecked)
                              //.ObservesProperty(() => AugChecked)
                              //.ObservesProperty(() => SepChecked)
                              //.ObservesProperty(() => OctChecked)
                              //.ObservesProperty(() => NovChecked)
                              //.ObservesProperty(() => DecChecked)


                              ;




        }

        #region Variable Declarations

        public ObservableCollection<int> YearList { get; }

        public ObservableCollection<string> MonthList { get; set; }

        public ObservableCollection<int> SequenceList { get; }

        public ObservableCollection<SequenceTableRow> SequenceTable { get; } = new ObservableCollection<SequenceTableRow>();

        private string _selectedMonth;
        public string SelectedMonth
        {
            get => _selectedMonth;
            set => SetProperty(ref _selectedMonth, value);
           
        }

         
        private int _startYear;
        public int StartYear
        {
            get => _startYear;
            set => SetProperty(ref _startYear, value);
        }

        private int _endYear;
        public int EndYear
        {
            get => _endYear;
            set => SetProperty(ref _endYear, value);
        }

        public int _selectedSequence;
        public int SelectedSequence
        {
            get => _selectedSequence;
            set => SetProperty(ref _selectedSequence, value);
        }

        private decimal _currentMonthValue;
        public decimal CurrentMonthValue
        {
            get => _currentMonthValue;
            set => SetProperty(ref _currentMonthValue, value);
        }

        private decimal _daAvgOblValue;
        public decimal DAAvgOblValue
        {
            get => _daAvgOblValue;
            set => SetProperty(ref _daAvgOblValue, value);
        }

        private decimal _daAvgOptValue;

        public decimal DAAvgOptValue
        {
            get => _daAvgOptValue;
            set => SetProperty(ref _daAvgOptValue, value);
        }

        public bool IsFirstSemester
        {
            get => SelectedSemester == 1;
            set
            {
                if (value)
                {
                    SelectedSemester = 1;
                    RaisePropertyChanged(nameof(IsSecondSemester));
                }
            }
        }
        public bool IsSecondSemester
        {
            get => SelectedSemester == 2;
            set
            {
                if (value)
                {
                    SelectedSemester = 2;
                    RaisePropertyChanged(nameof(IsFirstSemester));
                }
            }
        }

        private int _selectedSemester;
        public int SelectedSemester
        {
            get => _selectedSemester;
            set
            {
                if (SetProperty(ref _selectedSemester, value))
                {
                    ApplySemesterSelection();
                    BuildSemesterHeader();
                    BuildSequenceTable();
                }
            }
        }

        //Datatable classes
        private string _lastMonth1Header;
        public string LastMonth1Header
        {
            get => _lastMonth1Header;
            set => SetProperty(ref _lastMonth1Header, value);
        }

        private string _lastMonth2Header;
        public string LastMonth2Header
        {
            get => _lastMonth2Header;
            set => SetProperty(ref _lastMonth2Header, value);
        }

        private string _lastMonth3Header;
        public string LastMonth3Header
        {
            get => _lastMonth3Header;
            set => SetProperty(ref _lastMonth3Header, value);
        }

        private string _semMonth1Header;
        public string SemMonth1Header
        {
            get => _semMonth1Header;

            set => SetProperty(ref _semMonth1Header, value);
        }
        private string _semMonth2Header;
        public string SemMonth2Header
        {
            get => _semMonth2Header;
            set => SetProperty(ref _semMonth2Header, value);
        }
        private string _semMonth3Header;
        public string SemMonth3Header
        {
            get => _semMonth3Header;
            set => SetProperty(ref _semMonth3Header, value);
        }
        private string _semMonth4Header;
        public string SemMonth4Header
        {
            get => _semMonth4Header;
            set => SetProperty(ref _semMonth4Header, value);
        }

        private string _semMonth5Header;
        public string SemMonth5Header
        {
            get => _semMonth5Header;
            set => SetProperty(ref _semMonth5Header, value);
        }
        private string _semMonth6Header;
        public string SemMonth6Header
        {
            get => _semMonth6Header;
            set => SetProperty(ref _semMonth6Header, value);
        }

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

        private string _selectedPeakType;

        public string SelectedPeakType
        {
            get => _selectedPeakType;
            set
            {
                if (SetProperty(ref _selectedPeakType, value))
                {
                    RaisePropertyChanged(nameof(IsPeakWD));
                    RaisePropertyChanged(nameof(IsOffPeak));
                    RaisePropertyChanged(nameof(IsPeakWe));
                }
            }
        }

        public bool IsPeakWD
        {
            get => SelectedPeakType == "PeakWD";

            set
            {
                if (value)
                {
                    SelectedPeakType = "PeakWD";
                }
            }

        }
        public bool IsOffPeak
        {
            get => SelectedPeakType == "OFFPEAK";

            set
            {
                if (value)
                {
                    SelectedPeakType = "OFFPEAK";
                }
            }

        }
        public bool IsPeakWe
        {
            get => SelectedPeakType == "PEAKWE";

            set
            {
                if (value)
                {
                    SelectedPeakType = "PEAKWE";
                }
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
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
                //InitializeSourceSinkStateAndRetrive(false);
                RaisePropertyChanged("DecChecked");
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
                if (HedgeComboSelectedValue == "OBL")
                {

                }
                else
                {

                }
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
        #endregion
        #region Private methods

        private void BuildLastMonthHeader()
        {
            DateTime now = DateTime.Now;

            LastMonth1Header = now.AddMonths(-2).ToString("MMM");
            LastMonth2Header = now.AddMonths(-1).ToString("MMM");
            LastMonth3Header = now.ToString("MMM");

        }

        private void BuildSemesterHeader()
        {
            string[] sem1 = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
            string[] sem2 = new string[] { "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            string[] months = SelectedSemester == 1 ? sem1 : sem2;

            SemMonth1Header = months[0];
            SemMonth2Header = months[1];
            SemMonth3Header = months[2];
            SemMonth4Header = months[3];
            SemMonth5Header = months[4];
            SemMonth6Header = months[5];

        }

        private void BuildSequenceTable()
        {
            SequenceTable.Clear();

            for (int i = 1; i <= 3; i++)
            {
                SequenceTable.Add(new SequenceTableRow
                {
                    Seq6 = i,
                    Seq5 = i,
                    Seq4 = i,
                    Seq3 = i,
                    Seq2 = i,
                    Seq1 = i,

                });
            }

        }

        private void Reset()
        {
            SelectedPeakType = "PeakWD";
        }
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
            HedgeComboSelectedValue = "OPT";
        }

        private void ApplySemesterSelection()
        {
            if (SelectedSemester == 1)
            {
                JanChecked = true;
                FebChecked = true;
                MarChecked = true;
                AprChecked = true;
                MayChecked = true;
                JunChecked = true;

                JulChecked = false;
                AugChecked = false;
                SepChecked = false;
                OctChecked = false;
                NovChecked = false;
                DecChecked = false;
            }
            else if (SelectedSemester == 2)
            {
                JanChecked = false;
                FebChecked = false;
                MarChecked = false;
                AprChecked = false;
                MayChecked = false;
                JunChecked = false;

                JulChecked = true;
                AugChecked = true;
                SepChecked = true;
                OctChecked = true;
                NovChecked = true;
                DecChecked = true;
            }
        }

        private bool CanLoadData()
        {
            return StartYear > 0
                && EndYear > 0
                && StartYear <= EndYear
                && !string.IsNullOrEmpty(SelectedPeakType)
                && SelectedSemester > 0
                && HedgeComboSelectedValue != null
                && SelectedSequence > 0
                && SourceSinkList != null
                && SourceSinkList.Count > 0
                && SelectedMonth != null
                && IsAnyMonthSelected();
        }

        private bool IsAnyMonthSelected()
        {
            return JanChecked
                || FebChecked
                || MarChecked
                || AprChecked
                || MayChecked
                || JunChecked
                || JulChecked
                || AugChecked
                || SepChecked
                || OctChecked
                || NovChecked
                || DecChecked;
        }
        private List<int> GetSelectedMonth()
        {
            List<int> months = new List<int>();

            if (JanChecked) months.Add(1);
            if (FebChecked) months.Add(2);
            if (MarChecked) months.Add(3);
            if (AprChecked) months.Add(4);
            if (MayChecked) months.Add(5);
            if (JunChecked) months.Add(6);
            if (JulChecked) months.Add(7);
            if (AugChecked) months.Add(8);
            if (SepChecked) months.Add(9);
            if (OctChecked) months.Add(10);
            if (NovChecked) months.Add(11);
            if (DecChecked) months.Add(12);

            return months;
        }

        private void LoadData()
        {
            //var months = GetSelectedMonth();
            int selectedMonth = GetSelectedMonthNumber();

            if (SourceSinkList == null || SourceSinkList.Count == 0)
                return;


            var sourceSink = SourceSinkList[0];   // example first pair

            int sourceKey = sourceSink.Source.NodeKey;
            int sinkKey = sourceSink.Sink.NodeKey;

            string sourceName = sourceSink.Source.NodeName;
            string sinkName = sourceSink.Sink.NodeName;



            // Process rows
            SequenceTable.Clear();

            for(int year = EndYear; year >= StartYear; year--)
            {
                SequenceTable.Add(new SequenceTableRow
                {
                    Year = year
                });
                    
            }



            System.Data.DataTable table = _dataService.GetSequenceYearData(
           StartYear,
           EndYear,
           selectedMonth,
           SelectedPeakType,
           HedgeComboSelectedValue.ToString(),
           sourceKey,
           sinkKey,
            $"%.{SelectedSemester}st6.AnnualAuction.Seq%"
            );

            foreach (System.Data.DataRow row in table.Rows)
            {
                int year = Convert.ToInt32(row["AuctionYear"]);

                var dataRow = SequenceTable.FirstOrDefault(r => r.Year == year);

                if (dataRow == null)
                    continue;


                //Year = Convert.ToInt32(row["AuctionYear"]),
                dataRow.Seq6 = row["Seq6"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Seq6"]);
                dataRow.Seq5 = row["Seq5"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Seq5"]);
                dataRow.Seq4 = row["Seq4"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Seq4"]);
                dataRow.Seq3 = row["Seq3"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Seq3"]);
                dataRow.Seq2 = row["Seq2"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Seq2"]);
                dataRow.Seq1 = row["Seq1"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Seq1"]);
               
            }

            System.Data.DataTable monthlyTable = _dataService.GetMonthlyShadowPrices(
                StartYear, EndYear, SelectedPeakType, HedgeComboSelectedValue.ToString(), sourceName, sinkName);

            var selectedMonths = GetSelectedMonth();

            MessageBox.Show(monthlyTable.Rows.Count.ToString());
            foreach(System.Data.DataRow mRow in monthlyTable.Rows)
            {
                int year = Convert.ToInt32(mRow["AuctionYear"]);
                int month = Convert.ToInt32(mRow["AuctionMonth"]);
                decimal price = Convert.ToDecimal(mRow["ShadowPrice"]);

                if (!selectedMonths.Contains(month))
                    continue;


                var row = SequenceTable.FirstOrDefault(r => r.Year == year);

                if (row == null)
                    continue;

                int displayMonthIndex = SelectedSemester == 1 ? month : month - 6;

                if (displayMonthIndex < 1 || displayMonthIndex > 6)
                    continue;

                switch(displayMonthIndex)
                {
                    case 1: row.SemMonth1 = price; break;
                    case 2: row.SemMonth2 = price; break;
                    case 3: row.SemMonth3 = price; break;
                    case 4: row.SemMonth4 = price; break;
                    case 5: row.SemMonth5 = price; break;
                    case 6: row.SemMonth6 = price; break;
                    //case 7: row.SemMonth7 = price; break;
                    //case 8: row.SemMonth8 = price; break;
                    //case 9: row.SemMonth9 = price; break;
                    //case 10: row.SemMonth10 = price; break;
                    //case 11: row.SemMonth11 = price; break;
                    //case 12: row.SemMonth12 = price; break;

                }
            }





        }

        private int GetSelectedMonthNumber()
        {
            if (string.IsNullOrEmpty(SelectedMonth))
                return DateTime.Now.Month;
            return DateTime.ParseExact(
                SelectedMonth,
                "MMMM",
                System.Globalization.CultureInfo.InvariantCulture).Month;
        }
        #endregion
        #region Public Method
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

        public void AddSourceSink()
        {
            AddPath(false);
            LoadCommand.RaiseCanExecuteChanged();
        }
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
                // InitializeSourceSinkStateAndRetrive(isSwaped);

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
        private Dictionary<string, SourceSinkState> mFillSourceSinkHash;

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
                    //SelectSourceSinkFetchDataUpdateChart();
                    RaisePropertyChanged("SourceSinkDataSelected");
                    //UpdateView();
                }
            }
        }

        #endregion
    }

    public enum PeakType
    {
        PeakWD,
        OffPeak,
        PeakWE
    }
}
