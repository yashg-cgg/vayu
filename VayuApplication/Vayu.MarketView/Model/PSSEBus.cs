using Microsoft.Maps.MapControl.WPF;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.MarketView.Model
{
    public class NodeGeoDetail
    {
        public int BusNumber { get; set; }

        public string BusName { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public Location MapLocation { get; set; }

        public static List<NodeGeoDetail> GetPSSEBusList(int marketKey)
        {
            List<NodeGeoDetail> pssEBusList = new List<NodeGeoDetail>();
            SqlConnection VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand mSelectPSSECoordinatesCommand = new SqlCommand();
            mSelectPSSECoordinatesCommand.CommandText = "select NodeKey,NodeName,Longitude,Latitude, nt.Label as NodeType from Node n left join NodeTypeGeo nt on n.NodeTypeKey = nt.NodeTypeKey " +
            " where n.MarketKey = " + marketKey + " and Latitude is not null and Longitude is not null ";
            mSelectPSSECoordinatesCommand.Connection = VayuDbConn;

            //if (VayuDbConn.State != System.Data.ConnectionState.Open)
            //    VayuDbConn.Open();

            SqlDataReader reader = mSelectPSSECoordinatesCommand.ExecuteReader();
            while (reader.Read())
            {
                NodeGeoDetail bus = new NodeGeoDetail();
                bus.BusNumber = Configuration.GetInt(reader[0]);
                bus.BusName = (reader[1] ?? "").ToString();
                bus.Longitude = Configuration.GetDouble(reader[2]);
                bus.Latitude = Configuration.GetDouble(reader[3]);
                bus.NodeType = (reader[4] ?? "").ToString();
                bus.MapLocation = new Location(bus.Latitude, bus.Longitude);
                pssEBusList.Add(bus);
            }

            reader.Close();

            if (VayuDbConn.State != System.Data.ConnectionState.Closed)
                VayuDbConn.Close();

            return pssEBusList;
        }

        public string NodeType { get; set; }
    }

    public class NodeLocationHash : Hashtable
    {
        public NodeLocationHash() : base() { }

        public NodeLocationHash(IDictionary dic) : base(dic) { }

        public int ContextMarketKey { get; set; }

        public static NodeLocationHash GetLocationHash(int marketKey)
        {
            List<NodeGeoDetail> pssEBusList = NodeGeoDetail.GetPSSEBusList(marketKey);
            NodeLocationHash pssEBusHash = new NodeLocationHash(pssEBusList.ToDictionary(x => x.BusNumber));
            pssEBusHash.ContextMarketKey = marketKey;
            return pssEBusHash;
        }
    }

}