using OxyPlot;
using System;
using System.Collections.Generic;
using Vayu.LTC_Graphs.Model;

namespace Vayu.LTC_Graphs.Design
{
    public enum Interval
    {
        None = 0,
        Monthly,
        Quarterly,
        Annually,
        LongTerm
    }

    public enum SubInterval
    {
        YR1,
        YR2,
        YR3,
        YRAll
    }

    public enum HourType
    {
        None = 0,
        Peak,
        OffPeak,
        PeakWE,
        Day
    }

    [Flags]
    public enum PriceType
    {
        None = 0,
        DA = 1,
        RT = 2,
        CRR = 4,
        DACRR = 8,
    }

    [Flags]
    public enum ECollection
    {
        None = 0,
        DA = 1,
        RT = 2,
        CRR = 4,
        Peak = 8,
        OffPeak = 16,
        Day = 32,
        Monthly = 64,
        Quarterly = 128,
        Annually = 256
    }

    public class ValueStore
    {
        public PlotModel GraphPlotModel { get; set; }
        public List<StatisticsHelper> Summary { get; set; }
        public List<LmpHelper> CRRDetails { get; set; }
    }

}
