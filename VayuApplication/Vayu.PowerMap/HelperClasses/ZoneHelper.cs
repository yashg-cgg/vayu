using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.PowerMap.HelperClasses
{
    /// <summary>
    /// 
    /// </summary>
    public class ZoneHelper
    {
        /// <summary>
        /// Gets the zones.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public List<string> GetZones(int marketKey)
        {
            if (!zoneHash.ContainsKey(marketKey))
                LoadZones(marketKey);

            List<string> zoneLst = zoneHash[marketKey] as List<string>;
            return zoneLst;
        }

        //Instance
        /// <summary>
        /// The zone hash
        /// </summary>
        private Hashtable zoneHash;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneHelper"/> class.
        /// </summary>
        public ZoneHelper()
        {
            zoneHash = new Hashtable();
        }

        /// <summary>
        /// Loads the zones.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        public void LoadZones(int marketKey)
        {
            List<string> zoneLst = new List<string>();
            try
            {
                SqlCommand cmd = Configuration.GetTradingDBCommand();
                cmd.CommandText = "select distinct zone from Node where MarketKey = " + marketKey + "order by zone";

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string zone = reader[0].ToString();
                    if (!string.IsNullOrEmpty(zone))
                        zoneLst.Add(zone);
                }
                reader.Close();
                cmd.Connection.Close();
            }
            catch { }
            zoneHash[marketKey] = zoneLst;
        }
    }
}
