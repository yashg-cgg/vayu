using System.Collections.ObjectModel;
using BingLocationCollection = Microsoft.Maps.MapControl.WPF.LocationCollection;
using BingLocation = Microsoft.Maps.MapControl.WPF.Location;

namespace Vayu.MarketView.Model
{
    /// <summary>
    /// Bing-Maps-free replacement for Microsoft.Maps.MapControl.WPF.Location.
    /// Carries geographic WGS-84 coordinates (Latitude, Longitude).
    /// Mapsui/OSM conversion is done at render time via SphericalMercator.
    /// </summary>
    public class MapLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public MapLocation() { }

        public MapLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public MapLocation(double latitude, double longitude, double altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public override bool Equals(object obj)
        {
            var o = obj as MapLocation;
            return o != null && o.Latitude == Latitude && o.Longitude == Longitude;
        }

        public override int GetHashCode() => Latitude.GetHashCode() ^ Longitude.GetHashCode();
    }

    /// <summary>
    /// Bing-Maps-free replacement for Microsoft.Maps.MapControl.WPF.LocationCollection.
    /// Kept as ObservableCollection so existing INotify bindings / Insert / Add calls still work.
    /// </summary>
    public class MapLocationCollection : ObservableCollection<MapLocation>
    {
        public MapLocationCollection() { }
    }

    /// <summary>
    /// Boundary adapter used ONLY where the MarketView code has to consume
    /// legacy shared-model properties that still return
    /// Microsoft.Maps.MapControl.WPF.LocationCollection
    /// (e.g. Vayu.CommonControls.RegionInfo.Locations).
    ///
    /// This is NOT a cast. It performs an explicit, coordinate-preserving copy:
    ///   Bing Location.Latitude  -> MapLocation.Latitude
    ///   Bing Location.Longitude -> MapLocation.Longitude
    /// No latitude/longitude reversal, no reprojection, no data loss.
    ///
    /// When the shared model is eventually migrated to MapLocationCollection,
    /// this adapter can be deleted and callers switched to a direct assignment.
    /// </summary>
    public static class MapLocationCollectionAdapter
    {
        /// <summary>
        /// Converts a legacy Bing LocationCollection to MapLocationCollection.
        /// Null-safe: returns an empty MapLocationCollection when input is null.
        /// </summary>
        public static MapLocationCollection FromBing(BingLocationCollection bingLocations)
        {
            var result = new MapLocationCollection();
            if (bingLocations == null)
                return result;

            foreach (BingLocation bl in bingLocations)
            {
                if (bl == null) continue;
                result.Add(new MapLocation(bl.Latitude, bl.Longitude, bl.Altitude));
            }
            return result;
        }
    }
}