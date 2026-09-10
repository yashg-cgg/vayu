using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.DBLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class Portfolio
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>
        /// The market.
        /// </value>
        public string Market { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is uptos.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is uptos; otherwise, <c>false</c>.
        /// </value>
        public bool IsUptos { get; set; }
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }
        /// <summary>
        /// The trade type
        /// </summary>
        private string tradeType;

        /// <summary>
        /// Gets or sets the type of the trade.
        /// </summary>
        /// <value>
        /// The type of the trade.
        /// </value>
        public string TradeType
        {
            get
            {
                return tradeType;
            }
            set
            {
                tradeType = value;
                IsUptos = tradeType == "EES/PTP";
            }
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name.Trim();
        }
        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(Portfolio other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}
