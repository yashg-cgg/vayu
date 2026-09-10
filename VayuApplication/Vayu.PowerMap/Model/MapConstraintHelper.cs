using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.PowerMap.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.ConstraintHelper" />
    public class MapConstraintHelper : ConstraintHelper
    {
        /// <summary>
        /// Gets the latest constraint information.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="nodeHash">The node hash.</param>
        /// <param name="isAllDay">if set to <c>true</c> [is all day].</param>
        /// <returns></returns>
        public static List<ConstraintInfo> GetLatestConstraintInfo(MarketType market, Hashtable nodeHash, bool isAllDay = false)
        {
            List<ConstraintInfo> constraintInfoList = new List<ConstraintInfo>();
            List<LatestConstraint> constraintList = GetLatestConstraint(market, DateTime.Now, isAllDay);
            foreach (var item in constraintList)
            {
                ConstraintInfo info = new ConstraintInfo(item);
                NodeInfo sourceInfo = nodeHash[item.SourceNodeKey] as NodeInfo;
                NodeInfo sinkInfo = nodeHash[item.SinkNodeKey] as NodeInfo;

                if (sourceInfo != null)
                {
                    info.StationFrom = sourceInfo.NodeName;
                    if (sourceInfo.Latitude.HasValue && sinkInfo.Longitude.HasValue)
                        info.Fromlocation = new Location(sourceInfo.Latitude.Value, sinkInfo.Longitude.Value);
                }

                if (sinkInfo != null)
                {
                    info.StationTo = sinkInfo.NodeName;
                    if (sinkInfo.Latitude.HasValue && sinkInfo.Longitude.HasValue)
                        info.ToLocation = new Location(sinkInfo.Latitude.Value, sinkInfo.Longitude.Value);
                }

                constraintInfoList.Add(info);
            }

            return constraintInfoList;
        }

        /// <summary>
        /// Gets the constraint object rt.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="nodeHash">The node hash.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="hourly">if set to <c>true</c> [hourly].</param>
        /// <returns></returns>
        public static List<Constraintobj> GetConstraintObjRT(int market, Hashtable nodeHash, DateTime startDate, DateTime endDate, bool hourly = false)
        {
            if (nodeHash == null)
            {
                nodeHash = NodeInfo.GetHash(market);
            }

            List<Constraintobj> constraintInfoList = new List<Constraintobj>();
            MapConstraintHelper helper = new MapConstraintHelper();
            IConstraintInfoProvider infoProvider = helper.GetInstance();
            List<LatestConstraint> constraintList = infoProvider.GetConstraintRT(market, Configuration.GetAbsoluteDate(startDate),
                Configuration.GetAbsoluteDate(endDate), hourly);

            foreach (var item in constraintList)
            {
                Constraintobj info = new Constraintobj(item);
                info.MarketKey = market;
                NodeInfo sourceInfo = nodeHash[item.SourceNodeKey] as NodeInfo;
                NodeInfo sinkInfo = nodeHash[item.SinkNodeKey] as NodeInfo;

                if (sourceInfo != null)
                {
                    info.constraint_stationFrom = sourceInfo.NodeName;
                    if (sourceInfo.Latitude.HasValue && sourceInfo.Longitude.HasValue)
                        info.branch_location = new Location(sourceInfo.Latitude.Value, sourceInfo.Longitude.Value);
                }

                if (sinkInfo != null)
                {
                    info.constraint_stationTo = sinkInfo.NodeName;
                    if (sinkInfo.Latitude.HasValue && sinkInfo.Longitude.HasValue)
                        info.tobranch_location = new Location(sinkInfo.Latitude.Value, sinkInfo.Longitude.Value);
                }

                constraintInfoList.Add(info);
            }

            return constraintInfoList;
        }

        /// <summary>
        /// Gets the constraint object da.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="nodeHash">The node hash.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="hourly">if set to <c>true</c> [hourly].</param>
        /// <returns></returns>
        public static List<Constraintobj> GetConstraintObjDA(int market, Hashtable nodeHash, DateTime startDate, DateTime endDate, bool hourly = false)
        {
            if (nodeHash == null)
            {
                nodeHash = NodeInfo.GetHash(market);
            }

            List<Constraintobj> constraintInfoList = new List<Constraintobj>();
            MapConstraintHelper helper = new MapConstraintHelper();
            IConstraintInfoProvider infoProvider = helper.GetInstance();
            List<LatestConstraint> constraintList = infoProvider.GetConstraintDA(market, Configuration.GetAbsoluteDate(startDate),
                Configuration.GetAbsoluteDate(endDate));
            foreach (var item in constraintList)
            {
                Constraintobj info = new Constraintobj(item);
                NodeInfo sourceInfo = nodeHash[item.SourceNodeKey] as NodeInfo;
                NodeInfo sinkInfo = nodeHash[item.SinkNodeKey] as NodeInfo;

                if (sourceInfo != null)
                {
                    info.constraint_stationFrom = sourceInfo.NodeName;
                    if (sourceInfo.Latitude.HasValue && sourceInfo.Longitude.HasValue)
                        info.branch_location = new Location(sourceInfo.Latitude.Value, sourceInfo.Longitude.Value);
                }

                if (sinkInfo != null)
                {
                    info.constraint_stationTo = sinkInfo.NodeName;
                    if (sinkInfo.Latitude.HasValue && sinkInfo.Longitude.HasValue)
                        info.tobranch_location = new Location(sinkInfo.Latitude.Value, sinkInfo.Longitude.Value);
                }

                constraintInfoList.Add(info);
            }

            return constraintInfoList;
        }
    }
}
