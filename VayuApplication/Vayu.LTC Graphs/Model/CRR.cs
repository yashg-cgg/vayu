using System;
using System.Collections.Generic;

namespace Vayu.LTC_Graphs.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class CRR
    {
        /// <summary>
        /// The latest CRR
        /// </summary>
        public double? latestCRR;

        /// <summary>
        /// The auction date
        /// </summary>
        public DateTime AuctionDate;
        /// <summary>
        /// The node key
        /// </summary>
        public int NodeKey;
        /// <summary>
        /// The type
        /// </summary>
        public string Type;
        /// <summary>
        /// Gets the latest CRR.
        /// </summary>
        /// <value>
        /// The latest CRR.
        /// </value>
        public double? LatestCRR
        {
            get
            {
                if (!latestCRR.HasValue)
                {
                    if (Round1CRR.HasValue)
                        latestCRR = Round1CRR;

                    if (Round2CRR.HasValue)
                        latestCRR = Round2CRR;

                    if (Round3CRR.HasValue)
                        latestCRR = Round3CRR;

                    if (Round4CRR.HasValue)
                        latestCRR = Round4CRR;
                }
                else
                {

                }

                return latestCRR;
            }
        }
        /// <summary>
        /// The round1 CRR
        /// </summary>
        public double? Round1CRR;
        /// <summary>
        /// The round2 CRR
        /// </summary>
        public double? Round2CRR;
        /// <summary>
        /// The round3 CRR
        /// </summary>
        public double? Round3CRR;
        /// <summary>
        /// The round4 CRR
        /// </summary>
        public double? Round4CRR;
        /// <summary>
        /// The period key
        /// </summary>
        public int PeriodKey;

        public int? PeakWDHrs;
        public int? OffPeakHrs;
        public int? PeakWEHrs;
        public int? TotalHrs;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{Vayu.CRRGraphs.Model.CRR}" />
    public class CRRList : List<CRR>
    {
    }
}
