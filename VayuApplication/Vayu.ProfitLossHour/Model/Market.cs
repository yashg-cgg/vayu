using System.Collections.Generic;

namespace Vayu.ProfitLossHour.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class HourlyDataService
    {
        // public Market CreateMarket() { return new Market(this); }
        /// <summary>
        /// 
        /// </summary>
        class Market
        {
            /// <summary>
            /// The market key
            /// </summary>
            private int marketKey;
            /// <summary>
            /// The label
            /// </summary>
            private string label;
            /// <summary>
            /// The parent
            /// </summary>
            private HourlyDataService parent;

            /// <summary>
            /// Gets or sets the market key.
            /// </summary>
            /// <value>
            /// The market key.
            /// </value>
            public int MarketKey
            {
                get { return marketKey; }
                set { marketKey = value; }
            }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label
            {
                get { return label; }
                set { label = value; }
            }

            /// <summary>
            /// Prevents a default instance of the <see cref="Market"/> class from being created.
            /// </summary>
            private Market() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Market"/> class.
            /// </summary>
            /// <param name="svc">The SVC.</param>
            public Market(HourlyDataService svc)
            {
                parent = svc;
            }

            /// <summary>
            /// Gets the market list.
            /// </summary>
            /// <param name="upTos">if set to <c>true</c> [up tos].</param>
            /// <returns></returns>
            public List<Market> GetMarketList(bool upTos)
            {
                return null;
                /*SqlCommand selectNodeCommand = parent.RiskDataConnection.CreateCommand();
                selectNodeCommand.CommandText = "select * from Market";
                List<Market> nodeList = new List<Market>();

                parent.ExecuteDataReader((reader) =>
                {
                    Market node = new Market();
                    node.marketKey = (int)reader.GetDecimal(reader.GetOrdinal("MarketKey"));
                    node.Label = reader.GetString(reader.GetOrdinal("Label"));

                    if (upTos && (node.Label == "ERCOTTesting" || node.Label == "PJM"))
                        nodeList.Add(node);
                    else if (!upTos && node.Label != "ERCOT" && node.Label != "ICE" && node.Label != "Genscape" && node.Label != "Natural Gas")
                        nodeList.Add(node);

                    if (node.Label == "ERCOTTesting")
                        node.Label = "ERCOT";

                }, selectNodeCommand);

                if (!upTos)
                    nodeList.Add(new Market() { label = "SPP" });

                return nodeList;*/
            }
        }
    }

}
