using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vayu.Outage_Constraint_History.Model;

namespace Vayu.Outage_Constraint_History.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        private DateTime mStartDate = DateTime.Today.AddMonths(-1);
        public DelegateCommand Retrieve { private set; get; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
        public DelegateCommand WinterBoxesCommand { private set; get; }
        public DelegateCommand SpringBoxesCommand { private set; get; }
        public DelegateCommand SummerBoxesCommand { private set; get; }
        public DelegateCommand FallBoxesCommand { private set; get; }
        public DelegateCommand AllBoxesCommand { private set; get; }
        public DelegateCommand NoneBoxesCommand { private set; get; }
        private Dictionary<int, string> dicSeasons = new Dictionary<int, string>();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                //GetSeasonsGriddata();
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
                //GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
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
                // GetSeasonsGriddata();
                RaisePropertyChanged("JanChecked");
            }
        }
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
                mStartDate = value;
                RaisePropertyChanged("StartDate");
            }
        }

        private DateTime mEndDate = DateTime.Today;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
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
        private IEnumerable<Daily> mSeasonConstraintList;
        /// <summary>
        /// Gets or sets the constraint list.
        /// </summary>
        /// <value>
        /// The constraint list.
        /// </value>
        public IEnumerable<Daily> SeasonConstraintList
        {
            get
            {
                return mSeasonConstraintList;
            }
            set
            {
                mSeasonConstraintList = value;
                RaisePropertyChanged("SeasonConstraintList");
            }
        }
        private IEnumerable<Daily> mConstraintList;
        /// <summary>
        /// Gets or sets the constraint list.
        /// </summary>
        /// <value>
        /// The constraint list.
        /// </value>
        public IEnumerable<Daily> ConstraintList
        {
            get
            {
                return mConstraintList;
            }
            set
            {
                mConstraintList = value;
                RaisePropertyChanged("ConstraintList");
            }
        }

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            WinterBoxesCommand = new DelegateCommand(() => WinterData());
            SpringBoxesCommand = new DelegateCommand(() => SpringData());
            SummerBoxesCommand = new DelegateCommand(() => SummerData());
            FallBoxesCommand = new DelegateCommand(() => FallData());
            AllBoxesCommand = new DelegateCommand(() => AllData());
            NoneBoxesCommand = new DelegateCommand(() => NoneData());
            Retrieve = new DelegateCommand(() => RetrieveData());
            RunExportCSVCommand = new DelegateCommand(ExportToCSVCommand);
        }

        private void NoneData()
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
            ConstraintList = new List<Daily>();
            SeasonConstraintList = new List<Daily>();
        }

        private void AllData()
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
            GetSeasonsGriddata();
        }

        private void FallData()
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
            OctChecked = true;
            NovChecked = true;
            DecChecked = false;
            GetSeasonsGriddata();
        }

        private void SummerData()
        {
            JanChecked = false;
            FebChecked = false;
            MarChecked = false;
            AprChecked = false;
            MayChecked = false;
            JunChecked = true;
            JulChecked = true;
            AugChecked = true;
            SepChecked = true;
            OctChecked = false;
            NovChecked = false;
            DecChecked = false;
            GetSeasonsGriddata();
        }

        private void SpringData()
        {
            JanChecked = false;
            FebChecked = false;
            MarChecked = true;
            AprChecked = true;
            MayChecked = true;
            JunChecked = false;
            JulChecked = false;
            AugChecked = false;
            SepChecked = false;
            OctChecked = false;
            NovChecked = false;
            DecChecked = false;
            GetSeasonsGriddata();
        }

        private void WinterData()
        {
            JanChecked = true;
            FebChecked = true;
            MarChecked = false;
            AprChecked = false;
            MayChecked = false;
            JunChecked = false;
            JulChecked = false;
            AugChecked = false;
            SepChecked = false;
            OctChecked = false;
            NovChecked = false;
            DecChecked = true;
            GetSeasonsGriddata();
        }
        public void ExportToCSVCommand()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreaded(); });
        }
        private void RetrieveData()
        {
            ConstraintList = new List<Daily>();
            GetSeasonInfo();
            List<Daily> testConstraintOutagesList = new List<Daily>();
            testConstraintOutagesList = RetrieveDaily(StartDate.ToShortDateString(), EndDate.ToShortDateString());
            ConstraintList = testConstraintOutagesList.OrderByDescending(x => x.ConstraintNextDay).OrderByDescending(x => x.ConstraintNext10Day).OrderByDescending(x => x.ConstraintNext30Day);
            int currentyear = EndDate.Year;
            int currentmonth = EndDate.Month;
            string currentseasons = dicSeasons[currentmonth];
            GetCheckBoxes(currentseasons);
        }

        private void GetCheckBoxes(string currentseasons)
        {
            if (currentseasons == "Winter")
            {
                JanChecked = true;
                FebChecked = true;
                MarChecked = false;
                AprChecked = false;
                MayChecked = false;
                JunChecked = false;
                JulChecked = false;
                AugChecked = false;
                SepChecked = false;
                OctChecked = false;
                NovChecked = false;
                DecChecked = true;
            }

            if (currentseasons == "Spring")
            {
                JanChecked = false;
                FebChecked = false;
                MarChecked = true;
                AprChecked = true;
                MayChecked = true;
                JunChecked = false;
                JulChecked = false;
                AugChecked = false;
                SepChecked = false;
                OctChecked = false;
                NovChecked = false;
                DecChecked = false;
            }

            if (currentseasons == "Summer")
            {
                JanChecked = false;
                FebChecked = false;
                MarChecked = false;
                AprChecked = false;
                MayChecked = false;
                JunChecked = true;
                JulChecked = true;
                AugChecked = true;
                SepChecked = true;
                OctChecked = false;
                NovChecked = false;
                DecChecked = false;
            }

            if (currentseasons == "Fall")
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
                OctChecked = true;
                NovChecked = true;
                DecChecked = false;
            }
            GetSeasonsGriddata();
        }

        private void GetSeasonsGriddata()
        {
            ConstraintList = new List<Daily>();
            List<Daily> testConstraintOutagesList = new List<Daily>();
            if (ConstraintList.Count() == 0)
            {
                testConstraintOutagesList = RetrieveDaily(StartDate.ToShortDateString(), EndDate.ToShortDateString());
            }
            ConstraintList = testConstraintOutagesList.OrderByDescending(x => x.ConstraintNextDay).OrderByDescending(x => x.ConstraintNext10Day).OrderByDescending(x => x.ConstraintNext30Day);
            int currentyear = EndDate.Year;
            List<Dates> mMonthList = new List<Dates>();
            for (int k = 2013; k <= currentyear; k++)
            {
                if (JanChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 01, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (FebChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 02, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);

                }
                if (MarChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 03, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (AprChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 04, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (MayChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 05, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (JunChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 06, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (JulChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 07, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (AugChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 08, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (SepChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 09, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (OctChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 10, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (NovChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 11, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
                if (DecChecked)
                {
                    Dates mDates = new Dates();
                    mDates.sDateTime = new DateTime(k, 12, 01);
                    mDates.eDateTime = mDates.sDateTime.AddMonths(1).AddDays(-1);
                    mMonthList.Add(mDates);
                }
            }
            List<Daily> testSeasonsConstraintOutagesList = new List<Daily>();
            List<Daily> SeasonsConstraintOutagesList = new List<Daily>();
            foreach (Dates item in mMonthList)
            {
                SeasonsConstraintOutagesList = new List<Daily>();
                SeasonsConstraintOutagesList = RetrieveDaily(item.sDateTime.ToShortDateString(), item.eDateTime.ToShortDateString());

                testSeasonsConstraintOutagesList.AddRange(SeasonsConstraintOutagesList);

            }
            SeasonConstraintList = testSeasonsConstraintOutagesList.GroupBy(f => new { f.Outages, f.Constraint }).Select(x => new Daily
            {
                Constraint = x.Key.Constraint,
                Outages = x.Key.Outages,
                OutageCurrentMonth = x.Sum(y => y.OutageCurrentMonth),
                ConstraintNextDay = x.Sum(y => y.ConstraintNextDay),
                ConstraintNext10Day = x.Sum(y => y.ConstraintNext10Day),
                ConstraintNext30Day = x.Sum(y => y.ConstraintNext30Day)
            }).OrderByDescending(x => x.ConstraintNextDay).OrderByDescending(x => x.ConstraintNext10Day).OrderByDescending(x => x.ConstraintNext30Day);
        }


        private void GetSeasonInfo()
        {
            dicSeasons = new Dictionary<int, string>();
            dicSeasons.Add(1, "Winter");
            dicSeasons.Add(2, "Winter");
            dicSeasons.Add(3, "Spring");
            dicSeasons.Add(4, "Spring");
            dicSeasons.Add(5, "Spring");
            dicSeasons.Add(6, "Summer");
            dicSeasons.Add(7, "Summer");
            dicSeasons.Add(8, "Summer");
            dicSeasons.Add(9, "Summer");
            dicSeasons.Add(10, "Fall");
            dicSeasons.Add(11, "Fall");
            dicSeasons.Add(12, "Winter");
        }
        private List<Daily> RetrieveDaily(string startdate, string enddate)
        {
            _dataService.loadDBCommands(startdate, enddate);
            IEnumerable<ConstraintOutages> constraintsDailylist = new List<ConstraintOutages>();
            _dataService.GetConstraints((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                constraintsDailylist = item1;
            });
            List<string> ConstraintmonthList = constraintsDailylist.Select(x => x.Constraint).Distinct().ToList();
            List<string> OutageshistoryList = constraintsDailylist.Select(x => x.Outages).Distinct().ToList();
            IEnumerable<OutagesDetails> Outagelist = new List<OutagesDetails>();
            _dataService.GetOutagelist((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                Outagelist = item1;
            });
            IEnumerable<ConstraintDetails> Constraintslist = new List<ConstraintDetails>();
            _dataService.GetConstraintlist((item1, error) =>
            {
                if (error != null)
                {
                    return;
                }
                Constraintslist = item1;
            });

            List<Daily> testConstraintOutagesList = new List<Daily>();
            foreach (var item in constraintsDailylist)
            {
                var conlist = Constraintslist.Where(x => x.Constraint == item.Constraint);
                var outagelist = Outagelist.Where(x => x.Outages == item.Outages);
                int outCount = outagelist.Count();
                if (conlist.Count() >= 1)
                {
                    int nextdays = 0;
                    int next10days = 0;
                    int next30days = 0;
                    Daily mDaily = new Daily();
                    foreach (var datescon in conlist)
                    {
                        DateTime date = datescon.MarketDateTime;
                        if (conlist.Count() > 1)
                        {
                            // DateTime conDate = outagelist.FirstOrDefault().MarketDateTime;
                            foreach (DateTime conDate in outagelist.Select(x => x.MarketDateTime))
                            {
                                if (date >= conDate)
                                {
                                    if (((date - conDate).TotalDays >= 0) && ((date - conDate).TotalDays <= 1))
                                    {
                                        nextdays++;
                                    }
                                    if (((date - conDate).TotalDays >= 2) && ((date - conDate).TotalDays <= 10))
                                    {
                                        next10days++;
                                    }
                                    if (((date - conDate).TotalDays >= 11) && ((date - conDate).TotalDays <= 30))
                                    {
                                        next30days++;
                                    }
                                }
                            }

                        }
                    }
                    mDaily.Constraint = item.Constraint;
                    mDaily.Outages = item.Outages;
                    mDaily.OutageCurrentMonth = outCount;
                    mDaily.ConstraintNextDay = nextdays;
                    mDaily.ConstraintNext10Day = next10days + nextdays;
                    mDaily.ConstraintNext30Day = next30days + next10days + nextdays;
                    testConstraintOutagesList.Add(mDaily);
                }
            }
            return testConstraintOutagesList;
        }
        private void ExportToCSVThreaded()
        {
            if (ConstraintList == null)
            {
                Mouse.OverrideCursor = null;
                return;
            }
            if (ConstraintList == null || ConstraintList.Count() == 0)
            {
                System.Windows.MessageBox.Show("No data to export to");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
            dialog.FileName = "OutageConstraintHistory" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.FileName != "")
                {
                    if (ConstraintList != null && ConstraintList.Count() > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (PropertyInfo item in ConstraintList.FirstOrDefault().GetType().GetProperties())
                        {
                            if (item.Name == "Outages" || item.Name == "Constraint" || item.Name == "OutageCurrentMonth" || item.Name == "ConstraintNextDay" || item.Name == "ConstraintNext10Day" ||
                                item.Name == "ConstraintNext30Day")
                            {
                                builder.Append(item.Name + ",");
                            }

                        }
                        builder.AppendLine();
                        foreach (Daily item in ConstraintList)
                        {
                            foreach (PropertyInfo propItem in item.GetType().GetProperties())
                            {
                                if (propItem.Name == "Outages" || propItem.Name == "Constraint" || propItem.Name == "OutageCurrentMonth" || propItem.Name == "ConstraintNextDay" || propItem.Name == "ConstraintNext10Day" ||
                                    propItem.Name == "ConstraintNext30Day")
                                {
                                    builder.Append(propItem.GetValue(item) + ",");
                                }
                            }
                            builder.AppendLine();
                        }
                        builder.AppendLine();
                        if (SeasonConstraintList != null && SeasonConstraintList.Count() > 0)
                        {
                            foreach (PropertyInfo item in SeasonConstraintList.FirstOrDefault().GetType().GetProperties())
                            {
                                if (item.Name == "Outages" || item.Name == "Constraint" || item.Name == "OutageCurrentMonth" || item.Name == "ConstraintNextDay" || item.Name == "ConstraintNext10Day" ||
                                    item.Name == "ConstraintNext30Day")
                                {
                                    builder.Append(item.Name + ",");
                                }

                            }
                            builder.AppendLine();
                            foreach (Daily item in SeasonConstraintList)
                            {
                                foreach (PropertyInfo propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name == "Outages" || propItem.Name == "Constraint" || propItem.Name == "OutageCurrentMonth" || propItem.Name == "ConstraintNextDay" || propItem.Name == "ConstraintNext10Day" ||
                                        propItem.Name == "ConstraintNext30Day")
                                    {
                                        builder.Append(propItem.GetValue(item) + ",");
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
                                    System.Windows.MessageBox.Show("Successfully saved the file " + dialog.FileName);
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Couldnot save the file");
                                }
                            }
                        }

                    }


                }
            }
        }
    }
    class Dates
    {
        public DateTime sDateTime { get; set; }
        public DateTime eDateTime { get; set; }
    }
}
