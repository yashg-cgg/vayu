using System;

namespace Vayu.PowerMap.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class IIRReport
    {
        /// <summary>
        /// Gets or sets the report date.
        /// </summary>
        /// <value>
        /// The report date.
        /// </value>
        public DateTime ReportDate { get; set; }
        /// <summary>
        /// Gets or sets the day.
        /// </summary>
        /// <value>
        /// The day.
        /// </value>
        public string Day { get; set; }
        /// <summary>
        /// Gets or sets the days.
        /// </summary>
        /// <value>
        /// The days.
        /// </value>
        public int Days { get; set; }
        /// <summary>
        /// Gets or sets the mw base.
        /// </summary>
        /// <value>
        /// The mw base.
        /// </value>
        public double MWBase { get; set; }
        /// <summary>
        /// Gets or sets the mw intermed.
        /// </summary>
        /// <value>
        /// The mw intermed.
        /// </value>
        public double MWIntermed { get; set; }
        /// <summary>
        /// Gets or sets the mw peak.
        /// </summary>
        /// <value>
        /// The mw peak.
        /// </value>
        public double MWPeak { get; set; }
        /// <summary>
        /// Gets or sets the power usage.
        /// </summary>
        /// <value>
        /// The power usage.
        /// </value>
        public string PowerUsage { get; set; }
        //private DateTime mReportDate;
        //public DateTime ReportDate
        //{
        //   get
        //    {
        //        return mReportDate;
        //    }
        //    set
        //    {
        //        mReportDate = value;
        //    }
        //}
        //private string mDay;
        //public  string Day
        //{
        //    get
        //    {
        //        return mDay;
        //    }
        //    set
        //    {
        //        mDay = value;
        //    }
        //}
        //private int mDays;
        //public int Days
        //{
        //    get
        //    {
        //        return mDays;
        //    }
        //    set
        //    {
        //        mDays = value;
        //    }
        //}
        //private double mMWBase;
        //public double MWBase
        //{
        //    get
        //    {
        //        return mMWBase;
        //    }
        //    set
        //    {
        //        mMWBase = value;
        //    }
        //}
        //private double mMWIntermed;
        //public double MWIntermed
        //{
        //    get
        //    {
        //        return mMWIntermed;
        //    }
        //    set
        //    {
        //        mMWIntermed = value;
        //    }
        //}
        //private double mMWPeak;
        //public double MWPeak
        //{
        //    get
        //    {
        //        return mMWPeak;
        //    }
        //    set
        //    {
        //        mMWPeak = value;
        //    }
        //}
    }
}
