using System.Collections;

namespace Vayu.MarketView.Model
{
    public class DataItem
    {
        public string NodeName { get; set; }
        public int NodeKey { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string NodeType { get; set; }
    }

    public class LocationList : Hashtable
    {
        public int MarketKey { get; set; }
    }
}
