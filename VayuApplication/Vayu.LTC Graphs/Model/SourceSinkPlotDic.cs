using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vayu.CRRPeriodCongestionLibrary;
using Vayu.DBLibrary;
using Vayu.LTC_Graphs.Design;
using Vayu.LTC_Graphs.ViewModels;

namespace Vayu.LTC_Graphs.Model
{
    public class SourceSinkPlot : IComparer<LmpHelper>
    {
        private Task CRRTask;
        private Task priceTask;
        private IDataService service;
        private Dictionary<Interval, List<LmpHelper>> tempDARTDic;
        private Dictionary<HourType, Dictionary<int, CRR>> tempCRRHash;

        public SourceSinkDetail SourceSink { get; set; }
        public int MarketID { get; set; }
        public PlotModel GraphPlotModel { get; set; }
        public List<StatisticsHelper> Summary { get; set; }
        public List<LmpHelper> CRRDetails { get; set; }

        public SourceSinkPlot(IDataService svc, SourceSinkDetail ss)
        {
            service = svc;
            SourceSink = ss;
            tempDARTDic = new Dictionary<Interval, List<LmpHelper>>();
        }

        private List<StatisticsHelper> FixStats(List<LmpHelper> list)
        {
            double CRRMin = 0.0;
            double CRRMax = 0.0;
            double? dCRRMin = CRRMin as double?;
            double? dCRRMax = CRRMax as double?;

            if (list == null)
                return null;

            List<StatisticsHelper> tempList = new List<StatisticsHelper>();
            StatisticsHelper daStat = new StatisticsHelper();
            daStat.Stats = "DA";
            daStat.Max = list.Max(a => a.DALMP);
            daStat.Min = list.Min(a => a.DALMP);
            daStat.Avg = list.Average(a => a.DALMP);
            tempList.Add(daStat);

            StatisticsHelper rtStat = new StatisticsHelper();
            rtStat.Stats = "RT";
            rtStat.Max = list.Max(a => a.RTLMP);
            rtStat.Min = list.Min(a => a.RTLMP);
            rtStat.Avg = list.Average(a => a.RTLMP);
            tempList.Add(rtStat);

            Action addRoundInfo = () =>
            {
                StatisticsHelper round1Stat = new StatisticsHelper();
                round1Stat.Stats = "Round 1";
                round1Stat.Max = list.Max(a => a.Round1LMP);
                round1Stat.Min = list.Min(a => a.Round1LMP);
                round1Stat.Avg = list.Average(a => a.Round1LMP);
                tempList.Add(round1Stat);

                StatisticsHelper round2Stat = new StatisticsHelper();
                round2Stat.Stats = "Round 2";
                round2Stat.Max = list.Max(a => a.Round2LMP);
                round2Stat.Min = list.Min(a => a.Round2LMP);
                round2Stat.Avg = list.Average(a => a.Round2LMP);
                tempList.Add(round2Stat);

                StatisticsHelper round3Stat = new StatisticsHelper();
                round3Stat.Stats = "Round 3";
                round3Stat.Max = list.Max(a => a.Round3LMP);
                round3Stat.Min = list.Min(a => a.Round3LMP);
                round3Stat.Avg = list.Average(a => a.Round3LMP);
                tempList.Add(round3Stat);

                StatisticsHelper round4Stat = new StatisticsHelper();
                round4Stat.Stats = "Round 4";
                round4Stat.Max = list.Max(a => a.Round4LMP);
                round4Stat.Min = list.Min(a => a.Round4LMP);
                round4Stat.Avg = list.Average(a => a.Round4LMP);
                tempList.Add(round4Stat);
            };


            Action addCRRInfo = () =>
            {
                StatisticsHelper CRRStat = new StatisticsHelper();
                CRRStat.Stats = "CRR";
                CRRStat.Max = list.Max(a => a.CRRLMP);
                CRRStat.Min = list.Min(a => a.CRRLMP);
                CRRStat.Avg = list.Average(a => a.CRRLMP);

                dCRRMin = CRRStat.Min;
                dCRRMax = CRRStat.Max;
                tempList.Add(CRRStat);
            };

            Action addDAMinusCRRInfo = () =>
            {
                StatisticsHelper daCRRStat = new StatisticsHelper();
                daCRRStat.Stats = "DA-CRR";
                daCRRStat.Max = daStat.Max - dCRRMax;
                daCRRStat.Min = daStat.Min - dCRRMin;
                daCRRStat.Avg = list.Average(a => a.DACRR);
                tempList.Add(daCRRStat);
            };

            if (MarketID == 1)
            {
                if (CurrentInterval == Interval.Annually || CurrentInterval == Interval.LongTerm)
                {
                    addRoundInfo();
                }
                else
                {
                    addCRRInfo();
                    addDAMinusCRRInfo();
                }
            }
            else
            {
                if (CurrentInterval == Interval.Quarterly)
                {
                    addRoundInfo();
                }

                else
                {
                    addCRRInfo();
                    addDAMinusCRRInfo();
                }

            }


            return tempList;
        }

        private Series CreateSeries(IEnumerable<LmpHelper> enumerable,
            Func<LmpHelper, double?> valueSelector, OxyColor ocolor, string name)
        {
            List<GraphItem> grpList = new List<GraphItem>();
            foreach (var item1 in enumerable)
            {
                grpList.Add(new GraphItem
                {
                    X = item1.StartDate,
                    Y = GetGraphValue(valueSelector(item1))
                    //Y = GetGraphValue(item == 0 ? item1.DALMP : (item == 1 ? item1.RTLMP : item1.CRRLMP))
                });
            }

            if (grpList.Where(a => a.Y != 0).Count() <= 0)
                return new LineSeries();
            return new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:MM-dd}\n{Y:#.###}",
                MarkerSize = 1,
                StrokeThickness = 3,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 1,
                Color = ocolor,//item == 0 ? OxyColors.Blue : (item == 1 ? OxyColors.Green : OxyColors.Red),
                LineStyle = LineStyle.Solid,
                MarkerStroke = OxyColors.Black,
                Title = name.ToString()//Enum.GetName(typeof(StatType), item)
            };
        }

        private PlotModel DrawPlot(List<LmpHelper> orderedList)
        {
            PlotModel tempModel = new PlotModel();

            tempModel.PlotAreaBackground = OxyColors.Black;
            tempModel.TextColor = OxyColors.Black;


            tempModel.Axes.Add(new LinearAxis
            {
                Key = "YAxis",
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 120, 120, 120),
                MinorGridlineColor = OxyColor.FromArgb(20, 120, 120, 120),
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                TextColor = OxyColors.Black,
                TitleColor = OxyColors.Black,
                EndPosition = 1,
                Position = AxisPosition.Left,
                Title = "Price ----->",
                AxisTitleDistance = 0,

            });

            DateTimeAxis xAxis = new DateTimeAxis();
            xAxis.Title = "Market Date ---->";
            xAxis.Position = AxisPosition.Bottom;
            xAxis.TextColor = OxyColors.Black;
            xAxis.TitleColor = OxyColors.Black;
            xAxis.AxisTitleDistance = 0;
            xAxis.StringFormat = "MMM-yy";
            xAxis.MajorGridlineStyle = LineStyle.Dot;
            xAxis.MinorGridlineStyle = LineStyle.Dot;
            xAxis.AxislineThickness = 3;
            xAxis.MajorGridlineColor = OxyColor.FromArgb(40, 120, 120, 120);
            xAxis.MinorGridlineColor = OxyColor.FromArgb(20, 120, 120, 120);
            xAxis.IntervalType = DateTimeIntervalType.Months;

            tempModel.Axes.Add(xAxis);

            if (CurrentStatType.HasFlag(PriceType.CRR))
            {
                if (MarketID == 1)
                {
                    if (CurrentInterval == Interval.Annually || CurrentInterval == Interval.LongTerm)
                    {
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round1LMP, OxyColors.Brown, "Round 1"));
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round2LMP, OxyColors.Red, "Round 2"));
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round3LMP, OxyColors.Black, "Round 3"));
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round4LMP, OxyColors.Purple, "Round 4"));
                        if (CurrentStatType.HasFlag(PriceType.DA))
                            tempModel.Series.Add(CreateSeries(orderedList, y => y.DALMP, OxyColors.Blue, "DA"));

                        if (CurrentStatType.HasFlag(PriceType.RT))
                            tempModel.Series.Add(CreateSeries(orderedList, y => y.RTLMP, OxyColors.Green, "RT"));
                    }
                    else if (CurrentInterval == Interval.Monthly || CurrentInterval == Interval.Quarterly)
                    {
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.CRRLMP, OxyColors.Red, "CRR"));
                        if (CurrentStatType.HasFlag(PriceType.DACRR))
                            tempModel.Series.Add(CreateSeries(orderedList, y => y.DACRR, OxyColors.Yellow, "DA-CRR"));
                        if (CurrentHourType == HourType.OffPeak || CurrentHourType == HourType.Peak || CurrentHourType == HourType.Day)
                        {
                            if (CurrentStatType.HasFlag(PriceType.DA))
                                tempModel.Series.Add(CreateSeries(orderedList, y => y.DALMP, OxyColors.Blue, "DA"));

                            if (CurrentStatType.HasFlag(PriceType.RT))
                                tempModel.Series.Add(CreateSeries(orderedList, y => y.RTLMP, OxyColors.Green, "RT"));
                        }
                    }

                }
                else
                {
                    if (CurrentInterval == Interval.Quarterly)
                    {
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round1LMP, OxyColors.Brown, "Round 1"));
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round2LMP, OxyColors.Red, "Round 2"));
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round3LMP, OxyColors.Black, "Round 3"));
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.Round4LMP, OxyColors.Purple, "Round 4"));
                        if (CurrentStatType.HasFlag(PriceType.DA))
                            tempModel.Series.Add(CreateSeries(orderedList, y => y.DALMP, OxyColors.Blue, "DA"));

                        if (CurrentStatType.HasFlag(PriceType.RT))
                            tempModel.Series.Add(CreateSeries(orderedList, y => y.RTLMP, OxyColors.Green, "RT"));
                    }
                    else
                    {
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.CRRLMP, OxyColors.Red, "CRR"));
                        tempModel.Series.Add(CreateSeries(orderedList, y => y.DACRR, OxyColors.Yellow, "DA-CRR"));
                        if (CurrentStatType.HasFlag(PriceType.DA))
                            tempModel.Series.Add(CreateSeries(orderedList, y => y.DALMP, OxyColors.Blue, "DA"));

                        if (CurrentStatType.HasFlag(PriceType.RT))
                            tempModel.Series.Add(CreateSeries(orderedList, y => y.RTLMP, OxyColors.Green, "RT"));
                    }
                }
            }
            else if (!CurrentStatType.HasFlag(PriceType.CRR))
            {
                if (CurrentStatType.HasFlag(PriceType.DA))
                    tempModel.Series.Add(CreateSeries(orderedList, y => y.DALMP, OxyColors.Blue, "DA"));

                if (CurrentStatType.HasFlag(PriceType.RT))
                    tempModel.Series.Add(CreateSeries(orderedList, y => y.RTLMP, OxyColors.Green, "RT"));

                if (CurrentStatType.HasFlag(PriceType.DACRR))
                    tempModel.Series.Add(CreateSeries(orderedList, y => y.DACRR, OxyColors.Yellow, "DA-CRR"));
            }
            else if (!CurrentStatType.HasFlag(PriceType.DACRR))
            {
                if (CurrentStatType.HasFlag(PriceType.DA))
                    tempModel.Series.Add(CreateSeries(orderedList, y => y.DALMP, OxyColors.Blue, "DA"));

                if (CurrentStatType.HasFlag(PriceType.RT))
                    tempModel.Series.Add(CreateSeries(orderedList, y => y.RTLMP, OxyColors.Green, "RT"));
                tempModel.Series.Add(CreateSeries(orderedList, y => y.CRRLMP, OxyColors.Red, "CRR"));
            }
            var l = new Legend
            {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.RightTop,
                LegendTextColor = OxyColors.White,
                LegendPadding = 3

            };

            tempModel.Legends.Add(l);
            return tempModel;
        }

        private double GetGraphValue(double? value)
        {
            if (value.HasValue)
                return value.Value;
            else
                return double.NaN;
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Interval CurrentInterval { get; set; }
        public HourType CurrentHourType { get; set; }
        public PriceType CurrentStatType { get; set; }
        public SubInterval CurrentSubInterval { get; set; }

        public bool SetState(Interval interval, PriceType stat, HourType hourType, int Marketkey, SubInterval subInterval = SubInterval.YR1)
        {
            Summary = null;
            GraphPlotModel = null;
            CRRDetails = null;

            if (interval == Interval.None || stat == PriceType.None || hourType == HourType.None)
                return false;

            CurrentInterval = interval;
            CurrentStatType = stat;
            CurrentHourType = hourType;
            CurrentSubInterval = subInterval;

            Task.WaitAll(CRRTask, priceTask);

            if (!tempDARTDic.ContainsKey(interval))
                return false;

            List<LmpHelper> finalList = tempDARTDic[interval].ToList();

            finalList.ForEach(x => x.SetPeak());
            if (hourType == HourType.Peak)
                finalList.ForEach(x => x.SetPeak());
            else if (hourType == HourType.OffPeak)
                finalList.ForEach(x => x.SetOffPeak());
            else if (hourType == HourType.Day)
                finalList.ForEach(x => x.SetAll());
            else if (hourType == HourType.PeakWE)
                finalList.ForEach(x => x.SetPeakWE());

            if (tempCRRHash != null && tempCRRHash.ContainsKey(hourType) && tempCRRHash[hourType].Count > 0)
            {
                Dictionary<int, CRR> CRRPriceHash = tempCRRHash[hourType];
                foreach (var a in finalList)
                {
                    if (!CRRPriceHash.ContainsKey(a.PeriodKey))
                        continue;
                    CRR CRR = CRRPriceHash[a.PeriodKey];
                    if (Marketkey == 1)
                    {
                        a.Round1LMP = CRR.Round1CRR / a.Count;
                        a.Round2LMP = CRR.Round2CRR / a.Count;
                        a.Round3LMP = CRR.Round3CRR / a.Count;
                        a.Round4LMP = CRR.Round4CRR / a.Count;
                        a.CRRLMP = CRR.LatestCRR / a.Count;
                    }
                    else
                    {
                        a.Round1LMP = CRR.Round1CRR;
                        a.Round2LMP = CRR.Round2CRR;
                        a.Round3LMP = CRR.Round3CRR;
                        a.Round4LMP = CRR.Round4CRR;
                        a.CRRLMP = CRR.LatestCRR;
                    }
                }
            }

            List<LmpHelper> orderedList = null;

            if (CurrentInterval == Interval.LongTerm && MarketID == 1)
            {
                string periodName = string.Empty;

                switch (CurrentSubInterval)
                {
                    case SubInterval.YR1:
                        periodName = "YR1";
                        break;
                    case SubInterval.YR2:
                        periodName = "YR2";
                        break;
                    case SubInterval.YR3:
                        periodName = "YR3";
                        break;
                    case SubInterval.YRAll:
                        periodName = "YRALL";
                        break;
                }


                orderedList = finalList.Where(x => periodName.Equals(x.PeriodName, StringComparison.InvariantCultureIgnoreCase)).OrderBy(a => a.StartDate).ToList();
            }
            else
                orderedList = finalList.OrderBy(a => a.StartDate).ToList();

            List<LmpHelper> mFinalDACRRAddedList = finalList.OrderBy(x => x.StartDate).ToList();
            for (int i = 0; i < mFinalDACRRAddedList.Count; i++)
            {
                mFinalDACRRAddedList[i].DACRR = mFinalDACRRAddedList[i].DALMP - mFinalDACRRAddedList[i].CRRLMP;
            }

            Summary = FixStats(mFinalDACRRAddedList);
            GraphPlotModel = DrawPlot(mFinalDACRRAddedList);
            CRRDetails = mFinalDACRRAddedList;
            CRRDetails.Sort(this);
            return true;
        }

        public async void LoadLMPList()
        {
            CRRTask = Task.Run(() =>
            {
#if PERFORMANCE
                Utility.StartTimer("LoadCRRList");
#endif
                tempDARTDic = service.GetDARTPrices(SourceSink, StartDate, EndDate, MarketID, CurrentHourType);
#if PERFORMANCE
                Utility.StopTimer("LoadCRRList");
#endif
            });

            await CRRTask;
        }

        public async void LoadCRRPriceDic()
        {
            priceTask = Task.Run(() =>
            {
#if PERFORMANCE
                Utility.StartTimer("LoadPriceDic");
#endif
                if (SourceSink.Source == null && SourceSink.Sink == null)
                    return;

                tempCRRHash = service.GetAllCRRData(SourceSink, StartDate, EndDate, MarketID);
#if PERFORMANCE
                Utility.StopTimer("LoadPriceDic");
#endif
            });
            await priceTask;
        }

        public int Compare(LmpHelper x, LmpHelper y)
        {
            return x.StartDate.Date.CompareTo(y.StartDate.Date);
        }

        public void Swap()
        {
            SourceSinkDetail newSS = new SourceSinkDetail();
            newSS.Source = SourceSink.Sink;
            newSS.Sink = SourceSink.Source;
            SourceSink = newSS;

            foreach (var parent in tempDARTDic)
            {
                foreach (var item in parent.Value)
                {
                    item.AllDALMP *= -1;
                    item.AllRTLMP *= -1;

                    item.PeakDALMP *= -1;
                    item.PeakRTLMP *= -1;

                    item.OffPeakDALMP *= -1;
                    item.OffPeakDALMP *= -1;
                }
            }

            Dictionary<HourType, Dictionary<int, CRR>> newHourlyPriceHash = new Dictionary<HourType, Dictionary<int, CRR>>();

            foreach (HourType item in Enum.GetValues(typeof(HourType)))
            {
                if (!tempCRRHash.ContainsKey(item))
                    continue;

                Dictionary<int, CRR> loopDic = new Dictionary<int, CRR>();
                Dictionary<int, CRR> dic = tempCRRHash[item];
                foreach (var item2 in dic)
                {
                    CRR CRR = item2.Value;
                    CRR.Round1CRR *= -1;
                    CRR.Round2CRR *= -1;
                    CRR.Round3CRR *= -1;

                    loopDic[item2.Key] = CRR;
                }

                newHourlyPriceHash[item] = loopDic;
            }
            tempCRRHash = newHourlyPriceHash;
        }
    }

    public class SourceSinkPlotDic : Hashtable//, IPlotContainer
    {
        public SourceSinkPlotDic() { }

        public string GetKey(SourceSinkDetail key)
        {
            if (key == null)
                return string.Empty;

            string strKey = key.Source.ID + (key.Sink != null ? ("-" + key.Sink.ID) : string.Empty);
            return strKey;
        }

        public bool ContainsKey(SourceSinkDetail key)
        {
            string strKey = GetKey(key);
            return base.ContainsKey(strKey);
        }

        public override void Remove(object key)
        {
            string strKey = GetKey(key as SourceSinkDetail);
            base.Remove(strKey);
        }

        public override void Add(object key, object value)
        {
            string strKey = GetKey(key as SourceSinkDetail);
            base.Add(strKey, value);
        }

        public void Add(SourceSinkPlot plot)
        {
            string strKey = GetKey(plot.SourceSink);
            base.Add(strKey, plot);
        }

        public override bool Contains(object key)
        {
            return base.ContainsKey(key as SourceSinkDetail);
        }

        public SourceSinkPlot this[SourceSinkDetail node]
        {
            get
            {
                string strKey = GetKey(node as SourceSinkDetail);
                return base[strKey] as SourceSinkPlot;
            }
            set
            {
                string strKey = GetKey(node as SourceSinkDetail);
                base[strKey] = value;
            }
        }

        public SourceSinkPlot FillOrGet(IDataService service, SourceSinkDetail node, DateTime startDate, DateTime endTime, int MarketID, HourType mCurrentHourType)
        {
            Utility.StartTimer("FillOrGet");
            SourceSinkPlot modelPlot = null;

            if (!ContainsKey(node))
            {
                modelPlot = new SourceSinkPlot(service, node);
                this.Add(node, modelPlot);
            }
            else
                modelPlot = this[node];

            if (modelPlot.StartDate != startDate || modelPlot.EndDate != endTime || modelPlot.MarketID != MarketID)
            {
                modelPlot.StartDate = startDate;
                modelPlot.EndDate = endTime;
                modelPlot.MarketID = MarketID;
                modelPlot.CurrentHourType = mCurrentHourType;
                modelPlot.LoadLMPList();
                modelPlot.LoadCRRPriceDic();
            }

            Utility.StopTimer("FillOrGet");
            return modelPlot;
        }

        public SourceSinkPlot Swap(IDataService dataservice, SourceSinkDetail node)
        {
            SourceSinkDetail old = new SourceSinkDetail();
            old.Source = node.Sink;
            old.Sink = node.Source;

            if (!ContainsKey(old))
                return null;

            SourceSinkPlot modelPlot = this[old];
            this.Remove(old);
            this.Add(node, modelPlot);
            modelPlot.Swap();
            return modelPlot;
        }
    }

    public static class Utils
    {
        public static double? NullSum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            if (source.All(x => !selector(x).HasValue))
                return null;

            return source.Sum(x => selector(x));
        }

        public static string GetQuarterName(this DateTime date)
        {
            string qName = string.Empty;
            int quarterNum = date.GetQuarterNum();
            switch (quarterNum)
            {
                case 2:
                    qName = "Q2";
                    break;
                case 3:
                    qName = "Q3";
                    break;
                case 4:
                    qName = "Q4";
                    break;
                case 1:
                default:
                    qName = "Q1";
                    break;
            }

            qName += "-" + date.Year.ToString();
            return qName;
        }

        public static DateTime GetQuarterStart(this DateTime date)
        {
            int quarterNum = date.GetQuarterNum();
            DateTime qtStart;
            switch (quarterNum)
            {
                case 2:
                    qtStart = new DateTime(date.Year, 1, 1); break;
                case 3:
                    qtStart = new DateTime(date.Year, 4, 1); break;
                case 4:
                    qtStart = new DateTime(date.Year, 7, 1); break;

                case 1:
                default:
                    qtStart = new DateTime(date.Year, 10, 1); break;
                    break;
            }

            return qtStart;
        }

        public static int GetQuarterNum(this DateTime date)
        {
            int quarter = 0;
            switch (date.Month)
            {
                case 1:
                case 2:
                case 3:
                    quarter = 1;
                    break;

                case 4:
                case 5:
                case 6:
                    quarter = 2;
                    break;

                case 7:
                case 8:
                case 9:
                    quarter = 3;
                    break;

                case 10:
                case 11:
                case 12:
                    quarter = 4;
                    break;
            }
            return quarter;
        }
    }
}
