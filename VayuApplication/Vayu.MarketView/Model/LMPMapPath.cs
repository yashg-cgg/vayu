//using Microsoft.Maps.MapControl.WPF;   // Bing Maps replaced by Mapsui/OSM. Kept commented for traceability.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Vayu.CommonControls;
using Location = Vayu.MarketView.Model.MapLocation;           // Mapsui migration: retarget Location alias.
using LocationCollection = Vayu.MarketView.Model.MapLocationCollection; // Mapsui migration: retarget LocationCollection alias.

namespace Vayu.MarketView.Model
{
    public class PointMapPath
    {
        public string Name { get; set; }

        private string toolTipText;

        public string ToolTipText
        {
            get { return toolTipText; }
            set
            {
                toolTipText = value;
            }
        }

        public Location MapLocation { get; set; }

        public Brush MyColor { get; set; }

        private string typeName;
        public string NodeTypeName { get; set; }
        public string TypeName
        {
            get { return typeName; }
            set
            {
                typeName = value;

                //switch (typeName)
                //{
                //    case "ZONE":
                //        UpdateStarPatternPoints();
                //        break;

                //    case "HUB":
                //        UpdateTrianglePatternPoints();
                //        break;

                //    case "AGGREGATE":
                //        UpdateDiamodPoints();
                //        break;

                //    case "EHV":
                //    case "EXT":
                //    case "BUS":
                //    case "LOAD":
                //    case "INTERFACE":
                //    case "GENERATOR":
                //    default:
                //        break;
                //}
            }
        }

        public PointCollection MyPointCollection { get; set; }

        private void UpdateDiamodPoints()
        {
            MyPointCollection = new PointCollection();
            double unitlength = 5.5;
            MyPointCollection.Add(new Point(0, unitlength));
            MyPointCollection.Add(new Point(unitlength, 0));
            MyPointCollection.Add(new Point(0, -unitlength));
            MyPointCollection.Add(new Point(-unitlength, 0));
        }

        private void UpdateTrianglePatternPoints()
        {
            MyPointCollection = new PointCollection();
            double radius = 6;
            for (int i = 120; i <= 360; i = i + 120)
                MyPointCollection.Add(new Point(radius * Math.Sin(i * Math.PI / 180), radius * Math.Cos(i * Math.PI / 180)));
        }

        private void UpdateStarPatternPoints()
        {
            MyPointCollection = new PointCollection();
            double outter_radius = 6;
            double inner_radius = 3;

            for (int i = 36; i <= 324; i = i + 72)
            {
                MyPointCollection.Add(new Point(outter_radius * Math.Sin(i * Math.PI / 180),
                                                outter_radius * Math.Cos(i * Math.PI / 180)));
                MyPointCollection.Add(new Point(inner_radius * Math.Sin((i + 36) * Math.PI / 180),
                                                inner_radius * Math.Cos((i + 36) * Math.PI / 180)));
            }
        }

        public PointMapPath()
        {
            MyColor = Brushes.GreenYellow;
            TypeName = "Default";
        }
    }

    public class PointMapPathLocationList : ObservableCollection<PointMapPath>
    {
        public PointMapPathLocationList() : base() { }

        public PointMapPathLocationList(IEnumerable<PointMapPath> list)
        {
            Hashtable hash = new Hashtable();
            foreach (var item in list)
            {
                if (!hash.ContainsKey(item.Name))
                    hash[item.Name] = item;
                else
                    continue;

                this.Add(item);
            }
        }
    }

    public class MultiMapPath
    {
        public string Name { get; set; }

        public LocationCollection Locations { get; set; }

        public PointCollection ShapePointCollection { get; set; }

        public void AddExtraPoints()
        {
            if (Locations.Count > 2)
                return;

            double magnitude = 0.1;
            double xdistance = Locations[0].Latitude - Locations[1].Latitude;
            double ydistance = Locations[0].Longitude - Locations[1].Longitude;
            double slope = ydistance / xdistance;
            double inverslope = -1 / slope;
            Point midpoint = new Point();
            midpoint.X = Locations[0].Latitude - xdistance / 2;
            midpoint.Y = Locations[0].Longitude - ydistance / 2;
            double distance = Math.Sqrt(Math.Pow(xdistance, 2) + Math.Pow(ydistance, 2));
            Point midpointdown = new Point(midpoint.X + magnitude * ydistance, midpoint.Y - magnitude * xdistance);
            Point midpointup = new Point(midpoint.X - magnitude * ydistance, midpoint.Y + magnitude * xdistance);
            Locations.Insert(1, new Location(midpointdown.X, midpointdown.Y));
            Locations.Add(new Location(midpointup.X, midpointup.Y));

            //double latitude = Math.Abs(Locations[0].Latitude - Locations[1].Latitude) / 2;
            //double longitude = Math.Abs(Locations[0].Longitude - Locations[1].Longitude) / 2;

            //Locations.Insert(1, new Location(Locations[0].Latitude - latitude, Locations[1].Longitude - longitude));
            //Locations.Add(new Location(Locations[2].Latitude + latitude, Locations[0].Longitude + longitude));
        }

        public MultiMapPath()
        {
            Locations = new LocationCollection();
            ShapePointCollection = new PointCollection();
            double unitlength = 10.5;
            ShapePointCollection.Add(new Point(0, unitlength));
            ShapePointCollection.Add(new Point(unitlength, 0));
            ShapePointCollection.Add(new Point(0, -unitlength));
            ShapePointCollection.Add(new Point(-unitlength, 0));
        }
    }

    public class MultiLocationList : ObservableCollection<MultiMapPath>
    {
        public MultiLocationList() { }

        public MultiLocationList(IEnumerable<MultiMapPath> list) : base(list)
        {
            Hashtable hash = new Hashtable();
            foreach (var item in list)
            {
                if (!hash.ContainsKey(item.Name))
                    hash[item.Name] = item;
                else
                    continue;

                this.Add(item);
            }
        }
    }

    public class ZonePolyLineLocation : ZoneInfo
    {
        public LocationCollection ZoneLocations { get; set; }

        public SolidColorBrush FillColor { get; set; }

        public ZonePolyLineLocation()
        {
            ZoneLocations = new LocationCollection();
        }
    }

    public class ZonePolyLineLocationList : ObservableCollection<ZonePolyLineLocation>
    {

    }
}