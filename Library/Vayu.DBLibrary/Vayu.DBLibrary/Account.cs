using System.Collections.Generic;

namespace Vayu.DBLibrary
{
    public class Account
    {
        #region Public Properties
        /// <summary>
        /// Get and set trader infromation 
        /// </summary>
        public string Trader { get; set; }

        /// <summary>
        /// Get and set account ID. It is for unique identification of trader
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Get and Set list of portfolio
        /// </summary>
        public List<Portfolio> PortfolioList { get; set; }
        #endregion

        #region Overridable Methods
        /// <summary>
        /// override basic Tostring Function
        /// </summary>
        /// <returns> string : Return ID of trader  </returns>
        public override string ToString()
        {
            return Trader + " (" + ID + ")";
        }
        #endregion
    }
}
