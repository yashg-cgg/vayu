using System;
using System.Collections.Generic;

namespace Vayu.NodePriceMonitor.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Loads the database commands.
        /// </summary>
        void loadDBCommands();
        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <param name="callback">The callback.</param>
        void GetMarket(Action<List<MarketNode>, Exception> callback);
        /// <summary>
        /// Gets the market node.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="MarketKey">The market key.</param>
        void GetMarketNode(Action<List<MarketNode>, Exception> callback, int MarketKey);
        /// <summary>
        /// Gets the da price.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="Date">The date.</param>
        /// <param name="Market">The market.</param>
        /// <param name="Hub">The hub.</param>
        void GetDAPrice(Action<Dictionary<int, double>, Exception> callback, DateTime Date, int Market, int Hub);
        /// <summary>
        /// Gets the rt price.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="Date">The date.</param>
        /// <param name="Market">The market.</param>
        /// <param name="Hub">The hub.</param>
        void GetRTPrice(Action<Dictionary<int, double>, Exception> callback, DateTime Date, int Market, int Hub);
        /// <summary>
        /// Gets the strategy.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="username">The username.</param>
        void GetStrategy(Action<List<Strategy>, Exception> callback, string username);
        /// <summary>
        /// Inserts the strategy.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        void InsertStrategy(Strategy strategy);
        /// <summary>
        /// Deletes the strategy.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="NodePriceStrategyId">The node price strategy identifier.</param>
        void DeleteStrategy(string username, int NodePriceStrategyId);
    }

    /// <summary>
    /// 
    /// </summary>
    public class HourlyLMP
    {
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public int Hour { get; set; }
        /// <summary>
        /// Gets or sets the hour show.
        /// </summary>
        /// <value>
        /// The hour show.
        /// </value>
        public string HourShow { get; set; }
        /// <summary>
        /// Gets or sets the min0.
        /// </summary>
        /// <value>
        /// The min0.
        /// </value>
        public double? Min0 { get; set; }
        /// <summary>
        /// Gets or sets the min5.
        /// </summary>
        /// <value>
        /// The min5.
        /// </value>
        public double? Min5 { get; set; }
        /// <summary>
        /// Gets or sets the min10.
        /// </summary>
        /// <value>
        /// The min10.
        /// </value>
        public double? Min10 { get; set; }
        /// <summary>
        /// Gets or sets the min15.
        /// </summary>
        /// <value>
        /// The min15.
        /// </value>
        public double? Min15 { get; set; }
        /// <summary>
        /// Gets or sets the min20.
        /// </summary>
        /// <value>
        /// The min20.
        /// </value>
        public double? Min20 { get; set; }
        /// <summary>
        /// Gets or sets the min25.
        /// </summary>
        /// <value>
        /// The min25.
        /// </value>
        public double? Min25 { get; set; }
        /// <summary>
        /// Gets or sets the min30.
        /// </summary>
        /// <value>
        /// The min30.
        /// </value>
        public double? Min30 { get; set; }
        /// <summary>
        /// Gets or sets the min35.
        /// </summary>
        /// <value>
        /// The min35.
        /// </value>
        public double? Min35 { get; set; }
        /// <summary>
        /// Gets or sets the min40.
        /// </summary>
        /// <value>
        /// The min40.
        /// </value>
        public double? Min40 { get; set; }
        /// <summary>
        /// Gets or sets the min45.
        /// </summary>
        /// <value>
        /// The min45.
        /// </value>
        public double? Min45 { get; set; }
        /// <summary>
        /// Gets or sets the min50.
        /// </summary>
        /// <value>
        /// The min50.
        /// </value>
        public double? Min50 { get; set; }
        /// <summary>
        /// Gets or sets the min55.
        /// </summary>
        /// <value>
        /// The min55.
        /// </value>
        public double? Min55 { get; set; }
        /// <summary>
        /// Gets or sets the da.
        /// </summary>
        /// <value>
        /// The da.
        /// </value>
        public double? DA { get; set; }
        /// <summary>
        /// Gets or sets the rt.
        /// </summary>
        /// <value>
        /// The rt.
        /// </value>
        public double? RT { get; set; }
        /// <summary>
        /// Gets or sets the difference.
        /// </summary>
        /// <value>
        /// The difference.
        /// </value>
        public double? Diff { get; set; }
        /// <summary>
        /// Gets or sets the compare da.
        /// </summary>
        /// <value>
        /// The compare da.
        /// </value>
        public double? CompareDA { get; set; }
        /// <summary>
        /// Gets or sets the compare rt.
        /// </summary>
        /// <value>
        /// The compare rt.
        /// </value>
        public double? CompareRT { get; set; }
        //public double? Manual { get; set; }
        /// <summary>
        /// Gets or sets the combo.
        /// </summary>
        /// <value>
        /// The combo.
        /// </value>
        public double? Combo { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is peak hour.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is peak hour; otherwise, <c>false</c>.
        /// </value>
        public bool IsPeakHour { get; set; }
        /// <summary>
        /// Gets or sets the index of the hour type.
        /// </summary>
        /// <value>
        /// The index of the hour type.
        /// </value>
        public int HourTypeIndex { get; set; }
        /// <summary>
        /// The exante dispatch
        /// </summary>
        public List<int> ExanteDispatch;
        /// <summary>
        /// The imaginary value
        /// </summary>
        public List<int> ImaginaryValue;
    }

    /// <summary>
    /// 
    /// </summary>
    public class MarketNode
    {
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the market.
        /// </summary>
        /// <value>
        /// The name of the market.
        /// </value>
        public string MarketName { get; set; }
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the temporary price.
        /// </summary>
        /// <value>
        /// The temporary price.
        /// </value>
        public string TempPrice { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Strategy
    {
        /// <summary>
        /// Gets or sets the node price strategy identifier.
        /// </summary>
        /// <value>
        /// The node price strategy identifier.
        /// </value>
        public int NodePriceStrategyId { get; set; }
        /// <summary>
        /// Gets or sets the name of the strategy.
        /// </summary>
        /// <value>
        /// The name of the strategy.
        /// </value>
        public string StrategyName { get; set; }
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the current date.
        /// </summary>
        /// <value>
        /// The current date.
        /// </value>
        public DateTime CurrentDate { get; set; }
        /// <summary>
        /// Gets or sets the compare date.
        /// </summary>
        /// <value>
        /// The compare date.
        /// </value>
        public DateTime CompareDate { get; set; }
        /// <summary>
        /// Gets or sets the type of the hour.
        /// </summary>
        /// <value>
        /// The type of the hour.
        /// </value>
        public char HourType { get; set; }
    }
}
