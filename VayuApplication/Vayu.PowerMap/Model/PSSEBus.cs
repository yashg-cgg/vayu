namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    public class PSSEBus
    {
        /// <summary>
        /// Gets or sets the external identifier.
        /// </summary>
        /// <value>
        /// The external identifier.
        /// </value>
        public int ExternalID { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double longitude { get; set; }
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double latitude { get; set; }
        //public string kV { get; set; }
        //public int marketkey { get; set; }
        /// <summary>
        /// Gets or sets the bus number.
        /// </summary>
        /// <value>
        /// The bus number.
        /// </value>
        public int busNum { get; set; }
    }

    //public class NodeLocationHash : Hashtable
    //{
    //    public int MarketKey { get; set; }

    //    public void Refresh(int marketKey)
    //    {
    //        if (MarketKey == marketKey)
    //            return;

    //        SqlCommand command = Configuration.GetTradingDBCommand();
    //        this.Clear();

    //        try
    //        {
    //            command.CommandText = " select NodeKey,NodeName,ExternalNodeID,Longitude,Latitude from Node where MarketKey = " + marketKey;
    //            command.Connection.Open();
    //            SqlDataReader reader = command.ExecuteReader();

    //            while (reader.Read())
    //            {
    //                PSSEBus bus = new PSSEBus();
    //                bus.busNum = Configuration.GetInt(reader[0]);
    //                bus.name = reader[1].ToString();
    //                bus.ExternalID = Configuration.GetInt(reader[2]);
    //                bus.longitude = Configuration.GetDouble(reader[3]);
    //                bus.latitude = Configuration.GetDouble(reader[4]);
    //                this[bus.busNum] = bus;
    //            }

    //            reader.Close();
    //        }
    //        catch { }
    //        finally { command.Connection.Close(); }
    //    }
    //}
}
