using System;
using System.Collections;
using System.Collections.Generic;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.MarketView.Model
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataService
    {
        List<string> GetDeenergizedNodes();
        //LocationList GetNodeGeoLocations(int marketID);
        /// <summary>
        /// Gets the settlement locations.
        /// </summary>
        /// <param name="marketID">The market identifier.</param>
        /// <returns></returns>
        Hashtable GetSettlementLocations(int marketID);

        /// <summary>
        /// Gets the node zone.
        /// </summary>
        /// <param name="marketID">The market identifier.</param>
        /// <returns></returns>
        Dictionary<string, string> GetNodeZone(int marketID);
        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <param name="externalId">The external identifier.</param>
        /// <returns></returns>
        LatestConstraint GetNode(int externalId);
        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <param name="externalId">The external identifier.</param>
        /// <returns></returns>
        Tuple<String, Dictionary<int, double>> GetDACongestionByNodeAndHour(DateTime dateTime);
    }
}
