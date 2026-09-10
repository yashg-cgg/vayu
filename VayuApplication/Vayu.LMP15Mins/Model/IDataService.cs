using System;
using System.Collections.Generic;
using Vayu.NodePriceLibrary;

namespace Vayu.LMP15Mins.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        void loadDBCommands();
        /// <summary>
        /// Gets the load graph data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="loadskey">The loadskey.</param>
        void GetLoadGraphData(Action<List<Load>, Exception> callback, DateTime startDate, DateTime endDate, int marketKey, int? loadskey);
        /// <summary>
        /// Gets the load forecast graph data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="loadskey">The loadskey.</param>
        void GetLoadForecastGraphData(Action<List<Load>, Exception> callback, DateTime startDate, DateTime endDate, int marketKey, int? loadskey);
        /// <summary>
        /// Gets the constraint contingency data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="marketKey">The market key.</param>
        void GetLMP15MinDataa(Action<List<LMP15MinSourceSink>, Exception> callback, DateTime startDate, DateTime endDate, int marketKey, SourceSinkData SourceSink);
        /// <summary>
        /// Gets the source sink hourly prices.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="Hour">The hour.</param>
        /// <param name="SourceSink">The source sink.</param>
        void GetSourceSinkHourlyPrices(Action<List<Node>, List<Node>, Exception> callback, DateTime StartDate, int Hour, SourceSinkData SourceSink);
        /// <summary>
        /// Gets the hourly zone loads.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="marketKey">The market key.</param>
        void GetHourlyZoneLoads(Action<List<ZoneLoads>, Exception> callback, DateTime StartDate, int marketKey);
    }

    /// <summary>
    /// 
    /// </summary>
    public class Load
    {
        /// <summary>
        /// Gets or sets the hour.
        /// </summary>
        /// <value>
        /// The hour.
        /// </value>
        public int Hour { get; set; }
        /// <summary>
        /// Gets or sets the mega watts.
        /// </summary>
        /// <value>
        /// The mega watts.
        /// </value>
        public double? MegaWatts { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ZoneLoads
    {
        /// <summary>
        /// Gets or sets the zone load key.
        /// </summary>
        /// <value>
        /// The zone load key.
        /// </value>
        public int ZoneLoadKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the zone.
        /// </summary>
        /// <value>
        /// The name of the zone.
        /// </value>
        public string ZoneName { get; set; }
        /// <summary>
        /// Gets or sets the mega watts.
        /// </summary>
        /// <value>
        /// The mega watts.
        /// </value>
        public double? MegaWatts { get; set; }
        /// <summary>
        /// Gets or sets the type of the zone.
        /// </summary>
        /// <value>
        /// The type of the zone.
        /// </value>
        public string ZoneType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LMP15MinSourceSink
    {
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the Sink .
        /// </summary>
        /// <value>
        /// The Sink .
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the Source15minsLMP price.
        /// </summary>
        /// <value>
        /// The Source15minsLMP
        /// </value>
        public double Source15minsLMP { get; set; }
        public double Sink15minsLMP { get; set; }
        public double Sink_Source { get; set; }
        public DateTime MarketDateTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SourceSinkData
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public PricingNode Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public PricingNode Sink { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PricingNode
    {
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
        /// Gets or sets the external node identifier.
        /// </summary>
        /// <value>
        /// The external node identifier.
        /// </value>
        public long ExternalNodeId { get; set; }
        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }
        /// <summary>
        /// Gets or sets the node type key.
        /// </summary>
        /// <value>
        /// The node type key.
        /// </value>
        public int NodeTypeKey { get; set; }
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return NodeName;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LoadMarket
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
        /// Gets or sets the load key.
        /// </summary>
        /// <value>
        /// The load key.
        /// </value>
        public int? LoadKey { get; set; }
        /// <summary>
        /// Gets or sets the load forecast.
        /// </summary>
        /// <value>
        /// The load forecast.
        /// </value>
        public int? LoadForecast { get; set; }
        /// <summary>
        /// Gets or sets the name of the load.
        /// </summary>
        /// <value>
        /// The name of the load.
        /// </value>
        public string LoadName { get; set; }
    }
}
