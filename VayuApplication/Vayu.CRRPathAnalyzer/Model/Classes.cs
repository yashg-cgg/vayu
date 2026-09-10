using System;
using System.Collections.Generic;
using Vayu.CRRPeriodCongestionLibrary;
using Vayu.DBLibrary;

namespace Vayu.CRRPathAnalyzer.Model
{
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
        public double? Y { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SummaryRowType
    {
        /// <summary>
        /// The total
        /// </summary>
        Total,
        /// <summary>
        /// The maximum
        /// </summary>
        Max,
        /// <summary>
        /// The minimum
        /// </summary>
        Min,
        /// <summary>
        /// The average
        /// </summary>
        Avg,
        /// <summary>
        /// The win
        /// </summary>
        Win
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
    /// 
    /// </summary>
    public class ConsolidatedDataSet
    {
        /// <summary>
        /// Gets or sets the monthly data.
        /// </summary>
        /// <value>
        /// The monthly data.
        /// </value>
        public ConsolidatedData MonthlyData { get; set; }
        /// <summary>
        /// Gets or sets the daily data.
        /// </summary>
        /// <value>
        /// The daily data.
        /// </value>
        public ConsolidatedData DailyData { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConsolidatedData
    {
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public int Hours { get; set; }
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Gets or sets the da value.
        /// </summary>
        /// <value>
        /// The da value.
        /// </value>
        public double? DAValue { get; set; }
        /// <summary>
        /// Gets or sets the rt value.
        /// </summary>
        /// <value>
        /// The rt value.
        /// </value>
        public double? RTValue { get; set; }
        /// <summary>
        /// Gets or sets the Crr value.
        /// </summary>
        /// <value>
        /// The Crr value.
        /// </value>
        public double? CrrValue { get; set; }
        /// <summary>
        /// Gets or sets the daCrr value.
        /// </summary>
        /// <value>
        /// The daCrr value.
        /// </value>
        public double? DACrrValue { get; set; }
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }
        public string ClassType;
    }

    /// <summary>
    /// 
    /// </summary>
    public class DailyPivotData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DailyPivotData"/> class.
        /// </summary>
        public DailyPivotData() { }

        /// <summary>
        /// Gets or sets the type of the summary.
        /// </summary>
        /// <value>
        /// The type of the summary.
        /// </value>
        public SummaryRowType SummaryType { get; set; }

        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
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
        /// Gets or sets the value day count.
        /// </summary>
        /// <value>
        /// The value day count.
        /// </value>
        public int ValueDayCount { get; set; }
        /// <summary>
        /// The day count
        /// </summary>
        public int dayCount;

        /// <summary>
        /// The total
        /// </summary>
        private double? total;
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
                return total;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    total = null;
                }
                else
                {
                    total = value;
                }
            }
        }
        /// <summary>
        /// The average
        /// </summary>
        private double? average;
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
                return average;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    average = null;
                }
                else
                {
                    average = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the d1.
        /// </summary>
        /// <value>
        /// The d1.
        /// </value>
        public double? D1 { get; set; }
        /// <summary>
        /// Gets or sets the d2.
        /// </summary>
        /// <value>
        /// The d2.
        /// </value>
        public double? D2 { get; set; }
        /// <summary>
        /// Gets or sets the d3.
        /// </summary>
        /// <value>
        /// The d3.
        /// </value>
        public double? D3 { get; set; }
        /// <summary>
        /// Gets or sets the d4.
        /// </summary>
        /// <value>
        /// The d4.
        /// </value>
        public double? D4 { get; set; }
        /// <summary>
        /// Gets or sets the d5.
        /// </summary>
        /// <value>
        /// The d5.
        /// </value>
        public double? D5 { get; set; }
        /// <summary>
        /// Gets or sets the d6.
        /// </summary>
        /// <value>
        /// The d6.
        /// </value>
        public double? D6 { get; set; }
        /// <summary>
        /// Gets or sets the d7.
        /// </summary>
        /// <value>
        /// The d7.
        /// </value>
        public double? D7 { get; set; }
        /// <summary>
        /// Gets or sets the d8.
        /// </summary>
        /// <value>
        /// The d8.
        /// </value>
        public double? D8 { get; set; }
        /// <summary>
        /// Gets or sets the d9.
        /// </summary>
        /// <value>
        /// The d9.
        /// </value>
        public double? D9 { get; set; }
        /// <summary>
        /// Gets or sets the D10.
        /// </summary>
        /// <value>
        /// The D10.
        /// </value>
        public double? D10 { get; set; }
        /// <summary>
        /// Gets or sets the D11.
        /// </summary>
        /// <value>
        /// The D11.
        /// </value>
        public double? D11 { get; set; }
        /// <summary>
        /// Gets or sets the D12.
        /// </summary>
        /// <value>
        /// The D12.
        /// </value>
        public double? D12 { get; set; }
        /// <summary>
        /// Gets or sets the D13.
        /// </summary>
        /// <value>
        /// The D13.
        /// </value>
        public double? D13 { get; set; }
        /// <summary>
        /// Gets or sets the D14.
        /// </summary>
        /// <value>
        /// The D14.
        /// </value>
        public double? D14 { get; set; }
        /// <summary>
        /// Gets or sets the D15.
        /// </summary>
        /// <value>
        /// The D15.
        /// </value>
        public double? D15 { get; set; }
        /// <summary>
        /// Gets or sets the D16.
        /// </summary>
        /// <value>
        /// The D16.
        /// </value>
        public double? D16 { get; set; }
        /// <summary>
        /// Gets or sets the D17.
        /// </summary>
        /// <value>
        /// The D17.
        /// </value>
        public double? D17 { get; set; }
        /// <summary>
        /// Gets or sets the D18.
        /// </summary>
        /// <value>
        /// The D18.
        /// </value>
        public double? D18 { get; set; }
        /// <summary>
        /// Gets or sets the D19.
        /// </summary>
        /// <value>
        /// The D19.
        /// </value>
        public double? D19 { get; set; }
        /// <summary>
        /// Gets or sets the D20.
        /// </summary>
        /// <value>
        /// The D20.
        /// </value>
        public double? D20 { get; set; }
        /// <summary>
        /// Gets or sets the D21.
        /// </summary>
        /// <value>
        /// The D21.
        /// </value>
        public double? D21 { get; set; }
        /// <summary>
        /// Gets or sets the D22.
        /// </summary>
        /// <value>
        /// The D22.
        /// </value>
        public double? D22 { get; set; }
        /// <summary>
        /// Gets or sets the D23.
        /// </summary>
        /// <value>
        /// The D23.
        /// </value>
        public double? D23 { get; set; }
        /// <summary>
        /// Gets or sets the D24.
        /// </summary>
        /// <value>
        /// The D24.
        /// </value>
        public double? D24 { get; set; }
        /// <summary>
        /// Gets or sets the D25.
        /// </summary>
        /// <value>
        /// The D25.
        /// </value>
        public double? D25 { get; set; }
        /// <summary>
        /// Gets or sets the D26.
        /// </summary>
        /// <value>
        /// The D26.
        /// </value>
        public double? D26 { get; set; }
        /// <summary>
        /// Gets or sets the D27.
        /// </summary>
        /// <value>
        /// The D27.
        /// </value>
        public double? D27 { get; set; }
        /// <summary>
        /// Gets or sets the D28.
        /// </summary>
        /// <value>
        /// The D28.
        /// </value>
        public double? D28 { get; set; }
        /// <summary>
        /// Gets or sets the D29.
        /// </summary>
        /// <value>
        /// The D29.
        /// </value>
        public double? D29 { get; set; }
        /// <summary>
        /// Gets or sets the D30.
        /// </summary>
        /// <value>
        /// The D30.
        /// </value>
        public double? D30 { get; set; }
        /// <summary>
        /// Gets or sets the D31.
        /// </summary>
        /// <value>
        /// The D31.
        /// </value>
        public double? D31 { get; set; }

        public string ClassType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.SourceSinkData" />
    public class SourceSinkState : SourceSinkData
    {
        /// <summary>
        /// Gets or sets the proc Crr data.
        /// </summary>
        /// <value>
        /// The proc Crr data.
        /// </value>
        public List<PeriodFTRProc> procCrrData { get; set; }
        /// <summary>
        /// Gets or sets the proc daily LMP data.
        /// </summary>
        /// <value>
        /// The proc daily LMP data.
        /// </value>
        public List<PeriodicLMPProc> procDailyLMPData { get; set; }
        /// <summary>
        /// Gets or sets the proc monthly LMP data.
        /// </summary>
        /// <value>
        /// The proc monthly LMP data.
        /// </value>
        public List<PeriodicLMPProc> procMonthlyLMPData { get; set; }
    }
}
