using System;

namespace Vayu.WorkbookStatistics.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class HourlyPivotData
    {
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
        /// The m total
        /// </summary>
        private double? mTotal;
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
                return mTotal;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    mTotal = null;
                }
                else
                {
                    mTotal = value;
                }
            }
        }
        /// <summary>
        /// The m average
        /// </summary>
        private double? mAverage;
        /// <summary>
        /// Gets or sets the average.
        /// </summary>
        /// <value>
        /// The average.
        /// </value>
        private double? _MaxLoad;

        public double? MaxLoad
        {
            get { return _MaxLoad; }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    _MaxLoad = null;
                }
                else
                    _MaxLoad = value;
            }
        }
        private double? _CLearedMW;

        public double? ClearedMW
        {
            get { return _CLearedMW; }
            set
            {
                _CLearedMW = value;
                if (value == 0 || value.Equals(double.NaN))
                {
                    _CLearedMW = null;
                }
                else
                    _CLearedMW = value;
            }
        }

        private double? _AbsClearedMW;

        public double? AbsClearedMW
        {
            get { return _AbsClearedMW; }
            set
            {
                _AbsClearedMW = value;
                if (value == 0 || value.Equals(double.NaN))
                {
                    _AbsClearedMW = null;
                }
                else
                    _AbsClearedMW = value;
            }
        }



        public double? Average
        {
            get
            {
                return mAverage;
            }
            set
            {
                if (value == 0 || value.Equals(double.NaN))
                {
                    mAverage = null;
                }
                else
                {
                    mAverage = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the h e1.
        /// </summary>
        /// <value>
        /// The h e1.
        /// </value>
        public double? HE1 { get; set; }
        /// <summary>
        /// Gets or sets the h e2.
        /// </summary>
        /// <value>
        /// The h e2.
        /// </value>
        public double? HE2 { get; set; }
        /// <summary>
        /// Gets or sets the h e3.
        /// </summary>
        /// <value>
        /// The h e3.
        /// </value>
        public double? HE3 { get; set; }
        /// <summary>
        /// Gets or sets the h e4.
        /// </summary>
        /// <value>
        /// The h e4.
        /// </value>
        public double? HE4 { get; set; }
        /// <summary>
        /// Gets or sets the h e5.
        /// </summary>
        /// <value>
        /// The h e5.
        /// </value>
        public double? HE5 { get; set; }
        /// <summary>
        /// Gets or sets the h e6.
        /// </summary>
        /// <value>
        /// The h e6.
        /// </value>
        public double? HE6 { get; set; }
        /// <summary>
        /// Gets or sets the h e7.
        /// </summary>
        /// <value>
        /// The h e7.
        /// </value>
        public double? HE7 { get; set; }
        /// <summary>
        /// Gets or sets the h e8.
        /// </summary>
        /// <value>
        /// The h e8.
        /// </value>
        public double? HE8 { get; set; }
        /// <summary>
        /// Gets or sets the h e9.
        /// </summary>
        /// <value>
        /// The h e9.
        /// </value>
        public double? HE9 { get; set; }
        /// <summary>
        /// Gets or sets the h e10.
        /// </summary>
        /// <value>
        /// The h e10.
        /// </value>
        public double? HE10 { get; set; }
        /// <summary>
        /// Gets or sets the h e11.
        /// </summary>
        /// <value>
        /// The h e11.
        /// </value>
        public double? HE11 { get; set; }
        /// <summary>
        /// Gets or sets the h e12.
        /// </summary>
        /// <value>
        /// The h e12.
        /// </value>
        public double? HE12 { get; set; }
        /// <summary>
        /// Gets or sets the h e13.
        /// </summary>
        /// <value>
        /// The h e13.
        /// </value>
        public double? HE13 { get; set; }
        /// <summary>
        /// Gets or sets the h e14.
        /// </summary>
        /// <value>
        /// The h e14.
        /// </value>
        public double? HE14 { get; set; }
        /// <summary>
        /// Gets or sets the h e15.
        /// </summary>
        /// <value>
        /// The h e15.
        /// </value>
        public double? HE15 { get; set; }
        /// <summary>
        /// Gets or sets the h e16.
        /// </summary>
        /// <value>
        /// The h e16.
        /// </value>
        public double? HE16 { get; set; }
        /// <summary>
        /// Gets or sets the h e17.
        /// </summary>
        /// <value>
        /// The h e17.
        /// </value>
        public double? HE17 { get; set; }
        /// <summary>
        /// Gets or sets the h e18.
        /// </summary>
        /// <value>
        /// The h e18.
        /// </value>
        public double? HE18 { get; set; }
        /// <summary>
        /// Gets or sets the h e19.
        /// </summary>
        /// <value>
        /// The h e19.
        /// </value>
        public double? HE19 { get; set; }
        /// <summary>
        /// Gets or sets the h e20.
        /// </summary>
        /// <value>
        /// The h e20.
        /// </value>
        public double? HE20 { get; set; }
        /// <summary>
        /// Gets or sets the h e21.
        /// </summary>
        /// <value>
        /// The h e21.
        /// </value>
        public double? HE21 { get; set; }
        /// <summary>
        /// Gets or sets the h e22.
        /// </summary>
        /// <value>
        /// The h e22.
        /// </value>
        public double? HE22 { get; set; }
        /// <summary>
        /// Gets or sets the h e23.
        /// </summary>
        /// <value>
        /// The h e23.
        /// </value>
        public double? HE23 { get; set; }
        /// <summary>
        /// Gets or sets the h e24.
        /// </summary>
        /// <value>
        /// The h e24.
        /// </value>
        public double? HE24 { get; set; }
    }
}
