using System.Collections.Generic;

namespace Vayu.CRRCreditLibrary
{

    /// <summary>
    /// 
    /// </summary>
    public class Credit
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long ID { get; set; }
        /// <summary>
        /// Gets or sets the source node key.
        /// </summary>
        /// <value>
        /// The source node key.
        /// </value>
        public int SourceNodeKey { get; set; }
        /// <summary>
        /// Gets or sets the sink node key.
        /// </summary>
        /// <value>
        /// The sink node key.
        /// </value>
        public int SinkNodeKey { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the type of the hedge.
        /// </summary>
        /// <value>
        /// The type of the hedge.
        /// </value>
        public string HedgeType { get; set; }
        /// <summary>
        /// Gets or sets the name of the period.
        /// </summary>
        /// <value>
        /// The name of the period.
        /// </value>
        public string PeriodName { get; set; }
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the period hours.
        /// </summary>
        /// <value>
        /// The period hours.
        /// </value>
        public int PeriodHours { get; set; }
        /// <summary>
        /// Gets or sets the type of the class.
        /// </summary>
        /// <value>
        /// The type of the class.
        /// </value>
        public string ClassType { get; set; }
        /// <summary>
        /// Gets or sets the price mw list.
        /// </summary>
        /// <value>
        /// The price mw list.
        /// </value>
        public List<CreditPriceMW> PriceMWList { get; set; }
    }
}
