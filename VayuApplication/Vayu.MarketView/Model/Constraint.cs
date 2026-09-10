using System;

namespace Vayu.MarketView.Model
{
    public class Constraint
    {
        /// <summary>
        /// Gets or sets the constraint text.
        /// </summary>
        /// <value>
        /// The constraint text.
        /// </value>
        public string ConstraintText { get; set; }
        /// <summary>
        /// Gets or sets the contingency text.
        /// </summary>
        /// <value>
        /// The contingency text.
        /// </value>
        public string ContingencyText { get; set; }
        /// <summary>
        /// Gets or sets the monitored facility.
        /// </summary>
        /// <value>
        /// The monitored facility.
        /// </value>
        public string MonitoredFacility { get; set; }
        /// <summary>
        /// Gets or sets the constraint date.
        /// </summary>
        /// <value>
        /// The constraint date.
        /// </value>
        public DateTime ConstraintDate { get; set; }
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
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double? Price { get; set; }
        public double? Avg { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [is15 minimum].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is15 minimum]; otherwise, <c>false</c>.
        /// </value>
        public bool Is15Min { get; set; }

        public double? MaxLoad { get; set; }

        public double? MaxShadowPrice { get; set; }

        public double? MaxRTShadowPrice { get; set; }

        public int? SourceNodekey { get; set; }

        public int? SinkNodekey { get; set; }

        public string SourceZone { get; set; }

        public string SinkZone { get; set; }

        /// <summary>
        /// Accepts the specified constraint list.
        /// </summary>
        /// <param name="constraintList">The constraint list.</param>
        /// <returns></returns>
    }
}
