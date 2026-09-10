using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vayu.PowerMap.Controls;
using Vayu.PowerMap.Model;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public partial class MainWindow
    {
        #region Declaration

        /// <summary>
        /// The outage list
        /// </summary>
        private ObservableCollection<Outage> outageList;
        /// <summary>
        /// Gets or sets the geo outage list.
        /// </summary>
        /// <value>
        /// The geo outage list.
        /// </value>
        public ObservableCollection<Outage> GeoOutageList
        {
            get { return outageList; }
            set { outageList = value; RaisePropertyChanged("GeoOutageList"); }
        }

        /// <summary>
        /// The nonoutage list
        /// </summary>
        private ObservableCollection<Outage> nonoutageList;
        /// <summary>
        /// Gets or sets the non geo outage list.
        /// </summary>
        /// <value>
        /// The non geo outage list.
        /// </value>
        public ObservableCollection<Outage> NonGeoOutageList
        {
            get { return nonoutageList; }
            set { nonoutageList = value; RaisePropertyChanged("NonGeoOutageList"); }
        }

        /// <summary>
        /// The out equipment list
        /// </summary>
        private ObservableCollection<string> outEquipmentList;
        /// <summary>
        /// Gets or sets the outage equipment list.
        /// </summary>
        /// <value>
        /// The outage equipment list.
        /// </value>
        public ObservableCollection<string> OutageEquipmentList
        {
            get { return outEquipmentList; }
            set { outEquipmentList = value; RaisePropertyChanged("OutageEquipmentList"); }
        }
        /// <summary>
        /// The iir outage list
        /// </summary>
        private ObservableCollection<IIROutages> iirOutageList;
        /// <summary>
        /// Gets or sets the geo iir outage list.
        /// </summary>
        /// <value>
        /// The geo iir outage list.
        /// </value>
        public ObservableCollection<IIROutages> GeoIIROutageList
        {
            get
            {
                return iirOutageList;
            }
            set
            {
                iirOutageList = value;
                RaisePropertyChanged("GeoIIROutageList");
            }
        }
        /// <summary>
        /// The iir non outage list
        /// </summary>
        private ObservableCollection<IIROutages> iirNonOutageList;
        /// <summary>
        /// Gets or sets the geo iir non outage list.
        /// </summary>
        /// <value>
        /// The geo iir non outage list.
        /// </value>
        public ObservableCollection<IIROutages> GeoIIRNonOutageList
        {
            get
            {
                return iirNonOutageList;
            }
            set
            {
                iirNonOutageList = value;
                RaisePropertyChanged("GeoIIRNonOutageList");
            }
        }

        #endregion

        /// <summary>
        /// Previouses the businessdate.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <returns></returns>
        public static DateTime PrevBusinessdate(DateTime startDate)
        {
            int addDays = -1;
            if (startDate.DayOfWeek == DayOfWeek.Monday)
            {
                addDays = -3;
            }
            else if (startDate.DayOfWeek == DayOfWeek.Sunday)
            {
                addDays = -2;
            }
            return startDate.AddDays(addDays);
        }

        #region Private Methods

        /// <summary>
        /// Ellipses the shape.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="desc">The desc.</param>
        /// <returns></returns>
        private Ellipse EllipseShape(Location location, Color color, string desc)
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = new SolidColorBrush(color);
            myEllipse.Stroke = new SolidColorBrush(Colors.Black);
            double diameter = 20;
            double radius = diameter / 2;
            myEllipse.Width = 30;
            myEllipse.Height = 15;
            // myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = desc;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            return myEllipse;
        }
        /// <summary>
        /// Rectangles the shape.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="desc">The desc.</param>
        /// <returns></returns>
        private Rectangle RectangleShape(Location location, Color color, string desc)
        {
            Rectangle myRectangle = new Rectangle();
            myRectangle.Fill = new SolidColorBrush(color);
            myRectangle.Stroke = new SolidColorBrush(Colors.Black);
            myRectangle.StrokeThickness = 0.15;
            myRectangle.Width = 23;
            myRectangle.Height = 13;
            myRectangle.RadiusX = 4;
            myRectangle.RadiusY = 4;
            //myRectangle.Margin = new Thickness(-length / 2, -length / 2, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = desc;
            tt.FontWeight = FontWeights.Bold;
            myRectangle.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myRectangle, loc);
            ToolTipService.SetShowDuration(myRectangle, 300000);
            return myRectangle;
        }
        /// <summary>
        /// Polygons the shape.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="desc">The desc.</param>
        /// <returns></returns>
        private Polygon PolygonShape(Location location, Color color, string desc)
        {

            Polygon MyTriangle = new Polygon();
            MyTriangle.Fill = new SolidColorBrush(color);
            MyTriangle.Stroke = new SolidColorBrush(Colors.Black);
            MyTriangle.StrokeThickness = 1;
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(10, 20));
            myPointCollection.Add(new Point(40, 20));
            myPointCollection.Add(new Point(40, 40));
            myPointCollection.Add(new Point(60, 6));
            MyTriangle.Points = myPointCollection;
            ToolTip tt = new ToolTip();
            tt.Content = desc;
            tt.FontWeight = FontWeights.Bold;
            MyTriangle.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyTriangle, loc);
            ToolTipService.SetShowDuration(MyTriangle, 300000);
            return MyTriangle;
        }
        /// <summary>
        /// Stars the shape.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="desc">The desc.</param>
        /// <returns></returns>
        private Polygon StarShape(Location location, Color color, string desc)
        {
            Polygon MyStar = new Polygon();
            MyStar.Fill = new SolidColorBrush(color);
            MyStar.Stroke = new SolidColorBrush(Colors.Black);
            MyStar.StrokeThickness = 0.8;
            PointCollection myPointCollection = new PointCollection();
            double outter_radius = 12;
            double inner_radius = 6;

            for (int i = 36; i <= 324; i = i + 72)
            {
                myPointCollection.Add(new Point(outter_radius * Math.Sin(i * Math.PI / 180),
                                                outter_radius * Math.Cos(i * Math.PI / 180)));
                myPointCollection.Add(new Point(inner_radius * Math.Sin((i + 36) * Math.PI / 180),
                                                inner_radius * Math.Cos((i + 36) * Math.PI / 180)));
            }
            MyStar.Points = myPointCollection;
            ToolTip tt = new ToolTip();
            tt.Content = desc;
            tt.FontWeight = FontWeights.Bold;
            MyStar.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyStar, loc);
            ToolTipService.SetShowDuration(MyStar, 300000);
            return MyStar;
        }
        /// <summary>
        /// Diamonds the shape.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="desc">The desc.</param>
        /// <returns></returns>
        private Polygon DiamondShape(Location location, Color color, string desc)
        {
            Polygon MyPolygon = new Polygon();
            MyPolygon.Fill = new SolidColorBrush(color);
            MyPolygon.Stroke = new SolidColorBrush(Colors.Black);
            MyPolygon.StrokeThickness = 1.5;
            PointCollection myPointCollection = new PointCollection();
            double unitlength = 10;
            myPointCollection.Add(new Point(0, unitlength));
            myPointCollection.Add(new Point(unitlength, 0));
            myPointCollection.Add(new Point(0, -unitlength));
            myPointCollection.Add(new Point(-unitlength, 0));
            MyPolygon.Points = myPointCollection;
            ToolTip tt = new ToolTip();
            tt.Content = desc;
            tt.FontWeight = FontWeights.Bold;
            MyPolygon.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyPolygon, loc);
            ToolTipService.SetShowDuration(MyPolygon, 300000);
            return MyPolygon;
        }
        /// <summary>
        /// Circles the shape.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="desc">The desc.</param>
        /// <returns></returns>
        private Ellipse CircleShape(Location location, Color color, string desc)
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = new SolidColorBrush(color);
            double diameter = 20;
            double radius = diameter / 2;
            myEllipse.Width = diameter;
            myEllipse.Height = diameter;
            //myEllipse.Margin = new Thickness(-radius / Math.Sqrt(2), -radius / Math.Sqrt(2), 0, 0);
            myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = desc;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            return myEllipse;
        }
        /// <summary>
        /// Iirs the outage build map.
        /// </summary>
        /// <param name="iirOutagelist">The iir outagelist.</param>
        private void IIROutageBuildMap(ObservableCollection<IIROutages> iirOutagelist)
        {
            List<string> pUnitList = new List<string>();
            foreach (IIROutages iiroutages in iirOutagelist)
            {
                Shape myShape;
                IIROutage_DataGrid.Items.Add(iiroutages);
                string fuelGroup = iiroutages.FuelGroup;
                pUnitList.Add(iiroutages.PlantUnitName);
                Location location = new Location(iiroutages.Lattitude, iiroutages.Longitude);
                string description = "Unit Name :" + iiroutages.UnitName + "\n" +
                                    "Plant Name :" + iiroutages.PlantName + "\n" +
                                    "Primary Fuel :" + iiroutages.PrimaryFuel + "\n" +
                                    "Secondary Fuel :" + iiroutages.SecondaryFuel + "\n" +
                                    "Output Capacity :" + iiroutages.OutputCapacity + "\n" +
                                    "Status :" + iiroutages.Status + "\n" +
                                    "Heat Rate :" + iiroutages.HeatRate + "\n" +
                                    "Cap Offline :" + iiroutages.CapOffline + "\n";
                switch (fuelGroup)
                {
                    case "Fuel Oil":
                        myShape = CircleShape(location, Colors.BlueViolet, description);
                        mIIROutageMapLayer.Children.Add(myShape);
                        break;
                    case "Renewable Energy":
                        myShape = DiamondShape(location, Colors.BlueViolet, description);
                        mIIROutageMapLayer.Children.Add(myShape);
                        break;
                    case "Nuclear":
                        myShape = RectangleShape(location, Colors.BlueViolet, description);
                        mIIROutageMapLayer.Children.Add(myShape);
                        break;
                    case "Coal":
                        myShape = EllipseShape(location, Colors.BlueViolet, description);
                        mIIROutageMapLayer.Children.Add(myShape);
                        break;
                    case "Natural Gas":
                        myShape = StarShape(location, Colors.BlueViolet, description);
                        mIIROutageMapLayer.Children.Add(myShape);
                        break;
                    default:
                        break;
                }

            }
            IIROutage_AutoCompleteBox.ItemsSource = pUnitList;
        }

        /// <summary>
        /// Outages the build map.
        /// </summary>
        /// <param name="outagelist">The outagelist.</param>
        private void OutageBuildMap(List<Outage> outagelist)
        {
            List<Outage> geoList = new List<Outage>();
            List<Outage> nongeoList = new List<Outage>();

            string descripton = "";
            List<string> names = new List<string>();  //Auto_completioBox
            foreach (Outage _outage in outagelist)
            {
                Outage_dataGrid.Items.Add(_outage);
                descripton = "From Station: " + _outage.branch + "\n" +
                             "To Station: " + _outage.tobranch + "\n" +
                             "Equipment: " + _outage.equipment + "\n" +
                             "Equipment Type: " + _outage.equipmentType + "\n" +
                             "Equipment KV: " + _outage.voltage + "\n" +
                             "Status: " + _outage.status + "\n" +
                             "Start: " + _outage.startdate + "\n" +
                             "End: " + _outage.enddate + "\n" +
                             "Planned Start: " + _outage.plannedstart + "\n" +
                             "Planned End: " + _outage.plannedend + "\n" +
                             "Outage Type: " + _outage.type + "\n" +
                             "Outage Status: " + _outage.outagestatus + "\n" +
                             "Causes: " + _outage.causes + "\n";
                if (_outage.equipmentType == "LINE" || _outage.equipmentType == "LN" || _outage.equipmentType == "Line")
                {
                    if (_outage.branch_location != null && _outage.tobranch_location != null)
                    {
                        DrawLine(_outage.branch_location, _outage.tobranch_location, _outage.branch, _outage.tobranch, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.branch_location != null)
                    {
                        DrawXFMR(_outage.branch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.tobranch_location != null)
                    {
                        DrawXFMR(_outage.tobranch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
                else if ((_outage.equipmentType == "CAP") || (_outage.equipmentType == "SVC") || (_outage.equipmentType == "DSC") || (_outage.equipmentType == "CB") || (_outage.equipmentType == "LD") || (_outage.equipmentType == "SC"))
                {
                    if (_outage.branch_location != null && _outage.tobranch_location != null)
                    {
                        DrawLine(_outage.branch_location, _outage.tobranch_location, _outage.branch, _outage.tobranch, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.branch_location != null)
                    {
                        DrawXFMR(_outage.branch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.tobranch_location != null)
                    {
                        DrawXFMR(_outage.tobranch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
                else if ((_outage.equipmentType == "BRKR"))
                {
                    if (_outage.branch_location != null && _outage.tobranch_location != null)
                    {
                        DrawLine(_outage.branch_location, _outage.tobranch_location, _outage.branch, _outage.tobranch, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.branch_location != null)
                    {
                        DrawXFMR(_outage.branch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.tobranch_location != null)
                    {
                        DrawXFMR(_outage.tobranch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
                else if ((_outage.equipmentType == "PS"))
                {
                    if (_outage.branch_location != null && _outage.tobranch_location != null)
                    {
                        DrawLine(_outage.branch_location, _outage.tobranch_location, _outage.branch, _outage.tobranch, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.branch_location != null)
                    {
                        DrawXFMR(_outage.branch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.tobranch_location != null)
                    {
                        DrawXFMR(_outage.tobranch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
                else if ((_outage.equipmentType == "SD"))
                {
                    if (_outage.branch_location != null && _outage.tobranch_location != null)
                    {
                        DrawLine(_outage.branch_location, _outage.tobranch_location, _outage.branch, _outage.tobranch, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.branch_location != null)
                    {
                        DrawXFMR(_outage.branch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else if (_outage.tobranch_location != null)
                    {
                        DrawXFMR(_outage.tobranch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
                else if (_outage.equipmentType == "XFMR" || _outage.equipmentType == "XF" || _outage.equipmentType == "Transformer")
                {
                    if (_outage.branch_location != null)
                    {
                        DrawXFMR(_outage.branch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
                else if (_outage.equipmentType == "RT")
                {
                    if (_outage.branch_location != null && _outage.tobranch_location != null)
                    {
                        DrawLine(_outage.branch_location, _outage.tobranch_location, _outage.branch, _outage.tobranch, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
                else if (_outage.equipmentType == "Planned")
                {
                    if (_outage.branch_location != null)
                    {
                        DrawXFMR(_outage.branch_location, _outage, descripton);
                        geoList.Add(_outage);
                        names.Add(_outage.equipment);
                    }
                    else
                    {
                        nongeoList.Add(_outage);
                    }
                }
            }

            GeoOutageList = new ObservableCollection<Outage>(geoList);
            NonGeoOutageList = new ObservableCollection<Outage>(nongeoList);
            Outage_AutoCompleteBox.ItemsSource = names;
            OutageGridCount = Outage_dataGrid.Items.Count;
        }
        /// <summary>
        /// Iirs the outage pre load.
        /// </summary>
        private void IIROutagePreLoad()
        {
            return;

            if (!mMarkets.Values.Contains(mMarketInContext))
                return;
            if (VayuConnection == null)
            {
                return;
            }
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Close();
            }
            DateTime startDate = PrevBusinessdate(startTime);//.ToString("yyyy-MM-dd");
            DateTime endDate = endTime;//.ToString("yyyy-MM-dd");
            SqlDataReader reader = null;
            GeoIIROutageList = new ObservableCollection<IIROutages>();
            if (mMarketInContext == Markets["ERCOT"])
            {
                if (!(bool)MainCalendarSilder.IsRange)
                {
                    mSelectErcotIIROutageCommand.Parameters["@StartDate"].Value = startDate.ToString("yyyy-MM-dd");
                    mSelectErcotIIROutageCommand.Parameters["@EndDate"].Value = startDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    mSelectErcotIIROutageCommand.Parameters["@StartDate"].Value = startDate.ToString("yyyy-MM-dd");
                    mSelectErcotIIROutageCommand.Parameters["@EndDate"].Value = endDate.ToString("yyyy-MM-dd");
                }
                reader = mSelectErcotIIROutageCommand.ExecuteReader();
            }


            while (reader.Read())
            {
                //GeoIIROutageList.Add(new IIROutages
                //{
                IIROutages obj = new IIROutages();
                obj.OutageID = reader.IsDBNull(0) ? "" : Convert.ToString(reader.GetValue(0));
                obj.UnitName = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1));
                obj.UnitID = reader.IsDBNull(2) ? "" : Convert.ToString(reader.GetValue(2));
                obj.OwnerName = reader.IsDBNull(3) ? "" : Convert.ToString(reader.GetValue(3));
                obj.PlantID = reader.IsDBNull(4) ? "" : Convert.ToString(reader.GetValue(4));
                obj.PlantName = reader.IsDBNull(5) ? "" : Convert.ToString(reader.GetValue(5));
                obj.NERCRRegion = reader.IsDBNull(9) ? "" : Convert.ToString(reader.GetValue(9));
                obj.PrimaryFuel = reader.IsDBNull(10) ? "" : Convert.ToString(reader.GetValue(10));
                obj.SecondaryFuel = reader.IsDBNull(11) ? "" : Convert.ToString(reader.GetValue(11));
                obj.FuelGroup = reader.IsDBNull(12) ? "" : Convert.ToString(reader.GetValue(12));
                obj.OutputCapacity = reader.IsDBNull(13) ? "" : Convert.ToString(reader.GetValue(13));
                obj.PowerUsage = reader.IsDBNull(14) ? "" : Convert.ToString(reader.GetValue(14));
                obj.StartDate = reader.IsDBNull(15) ? new DateTime() : Convert.ToDateTime(reader.GetValue(15));
                obj.EndDate = reader.IsDBNull(16) ? new DateTime() : Convert.ToDateTime(reader.GetValue(16));
                obj.ODuration = reader.IsDBNull(17) ? 0 : Convert.ToInt32(reader.GetValue(17));
                obj.Precision = reader.IsDBNull(18) ? "" : Convert.ToString(reader.GetValue(18));
                obj.Type = reader.IsDBNull(19) ? "" : Convert.ToString(reader.GetValue(19));
                obj.Status = reader.IsDBNull(20) ? "" : Convert.ToString(reader.GetValue(20));
                obj.DeliveryDate = reader.IsDBNull(21) ? new DateTime() : Convert.ToDateTime(reader.GetValue(21));
                obj.NERCSubRegion = reader.IsDBNull(22) ? "" : Convert.ToString(reader.GetValue(22));
                obj.UnitType = reader.IsDBNull(24) ? "" : Convert.ToString(reader.GetValue(24));
                obj.HeatRate = reader.IsDBNull(25) ? "" : Convert.ToString(reader.GetValue(25));
                obj.UltimateOwnerID = reader.IsDBNull(26) ? "" : Convert.ToString(reader.GetValue(26));
                obj.UltimateOwner = reader.IsDBNull(27) ? "" : Convert.ToString(reader.GetValue(27));
                obj.ElectricConnection = reader.IsDBNull(30) ? "" : Convert.ToString(reader.GetValue(30));
                obj.CapOffline = reader.IsDBNull(32) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(32)));
                obj.TradeRegion = reader.IsDBNull(34) ? "" : Convert.ToString(reader.GetValue(34));
                obj.Lattitude = reader.IsDBNull(35) ? 0.0 : Convert.ToDouble(reader.GetValue(35));
                obj.Longitude = reader.IsDBNull(36) ? 0.0 : Convert.ToDouble(reader.GetValue(36));
                obj.OutageCause = reader.IsDBNull(37) ? "" : Convert.ToString(reader.GetValue(37));
                obj.PlantUnitName = obj.PlantName + " - " + obj.UnitName;
                GeoIIROutageList.Add(obj);
                //});
            }
            reader.Close();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            IIROutageGridCount = GeoIIROutageList.Count;
            IIROutageMain();
        }
        /// <summary>
        /// Outages the pre load.
        /// </summary>
        private void OutagePreLoad()
        {
            if (!mMarkets.Values.Contains(mMarketInContext))
                return;

            HashSet<string> eqipList = new HashSet<string>();
            if (VayuConnection == null)
            {
                return;
            }
            SqlDataAdapter adapter = new SqlDataAdapter();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();

            DataTable tempTableActual = new DataTable();
            DataTable tempTablePlanned = new DataTable();
            if (mMarketInContext == mMarkets["ERCOT"])
            {
                DateTime Eddt = endTime;
                DateTime ed = Eddt.AddDays(1).AddHours(0).AddMinutes(0);
                mERCOTOutagePlannedTable = new DataTable();
                mSelectERCOTOutagePlannedCommand.Parameters["@startdate"].Value = startTime.ToString("yyyy-MM-dd");
                mSelectERCOTOutagePlannedCommand.Parameters["@enddate"].Value = ed.ToString("yyyy-MM-dd");
                adapter.SelectCommand = mSelectERCOTOutagePlannedCommand;
                adapter.Fill(mERCOTOutagePlannedTable);
                tempTablePlanned = mERCOTOutagePlannedTable;
                adapter = new SqlDataAdapter();
                mERCOTOutageActualTable = new DataTable();
                mSelectERCOTOutageCommand.Parameters["@startdate"].Value = startTime;
                mSelectERCOTOutageCommand.Parameters["@enddate"].Value = endTime;
                adapter.SelectCommand = mSelectERCOTOutageCommand;
                adapter.Fill(mERCOTOutageActualTable);
                tempTableActual = mERCOTOutageActualTable;
            }

            VayuConnection.Close();

            if (mMarketInContext == mMarkets["ERCOT"])
            {
                if (tempTableActual.Rows.Count > 0)
                {
                    DataTable tmpTableViewColumns = tempTableActual.DefaultView.ToTable(true, "EquipmentType");
                    DataTable tmpTableViewColumns2 = tempTablePlanned.DefaultView.ToTable(true, "EquipmentType");

                    foreach (DataRow item in tmpTableViewColumns2.Rows)
                        eqipList.Add(item["EquipmentType"].ToString());

                    foreach (DataRow item in tmpTableViewColumns.Rows)
                        eqipList.Add(item["EquipmentType"].ToString());
                }
            }

            OutageEquipmentList = new ObservableCollection<string>(eqipList.ToList());
            OutageEquipmentTypeListBox.UnselectAll();
            foreach (var item in OutageEquipmentTypeListBox.Items)
            {
                if (item.ToString() == "LINE" || item.ToString() == "XFMR" || item.ToString() == "LN" || item.ToString() == "XF" || item.ToString() == "SVC" || item.ToString() == "DSC" || item.ToString() == "CB" || item.ToString() == "LD" || item.ToString() == "SC")
                {
                    OutageEquipmentTypeListBox.SelectedItems.Add(item);
                }
                //OutageEquipmentTypeListBox.SelectedItems.Add(item);
            }
            //OutageEquipmentTypeListBox.SelectedItems.Add(new Outage() {equipmentType="LINE"});
            OutageMain();
        }
        /// <summary>
        /// Iirs the outage main.
        /// </summary>
        private void IIROutageMain()
        {
            //IIROutagePreLoad();
            IIROutage_DataGrid.Items.Clear();
            if (GeoIIROutageList != null && GeoIIROutageList.Count != 0)
            {
                IIROutageBuildMap(GeoIIROutageList);
            }
        }
        /// <summary>
        /// Outages the main.
        /// </summary>
        /// <exception cref="System.Exception">Outage Exception" + e.Message</exception>
        private void OutageMain()
        {
            try
            {
                DataTable outputTable = new DataTable();
                List<Outage> outagelist = new List<Outage>();
                DataTable mOutputTable = new DataTable();
                Outage_dataGrid.Items.Clear();
                if (MarketlistBox.SelectedItem == null)
                {
                    return;
                }
                switch (MarketlistBox.SelectedItem.ToString())
                {

                    case "ERCOT":
                        // marketkey = mMarkets["ERCOT"];
                        if (SelectedOutagesSourceType == OutagesSourceType.Actual)
                        {
                            mOutputTable = mERCOTOutageActualTable;
                        }
                        else if (SelectedOutagesSourceType == OutagesSourceType.Planned)
                        {
                            mOutputTable = mERCOTOutagePlannedTable;
                        }
                        break;
                }
                if (mOutputTable == null)
                {
                    return;
                }

                MapControl.myMap.Children.Remove(mOutageMapLayer);
                mOutageMapLayer.Children.Clear();
                using (new WaitCursor())
                {
                    outputTable = mOutputTable;
                    outagelist = OutageBuildObject(outputTable);
                    OutageBuildMap(outagelist);
                }
                if ((bool)OutageCheckBox.IsChecked)
                {
                    MapControl.myMap.Children.Add(mOutageMapLayer);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Outage Exception" + e.Message);
            }
        }

        /// <summary>
        /// Outages the build object.
        /// </summary>
        /// <param name="mytable">The mytable.</param>
        /// <returns></returns>
        private List<Outage> OutageBuildObject(DataTable mytable)
        {
            mOutagelist = new List<Outage>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mAddedRevisedList = new List<string>();
            foreach (DataRow mydatarow in mytable.Rows)
            {
                Outage outage = new Outage();
                try
                {
                    if ((mMarketInContext == mMarkets["PJM"]) || (mMarketInContext == mMarkets["ERCOT"]))
                    {
                        outage.ticketId = mydatarow["TicketID"].ToString().Trim();
                        outage.branch = mydatarow["FromSub"].ToString().Trim();
                        outage.tobranch = mydatarow["ToSub"].ToString().Trim();
                        outage.zone = mydatarow["Zone"].ToString().Trim();
                        outage.equipmentType = mydatarow["EquipmentType"].ToString().Trim();
                        outage.voltage = Convert.ToInt32(Math.Round(Convert.ToDouble(mydatarow["Voltage"]), 0));
                        outage.equipment = mydatarow["Equipment"].ToString().Trim();
                        outage.status = mydatarow["Status"].ToString().Trim();
                        outage.type = mydatarow["Type"].ToString();
                        if (mytable.Columns.Contains("ActualStart"))
                        {
                            if (!(mydatarow["ActualStart"] is DBNull))
                            {
                                outage.startdate = Convert.ToDateTime(mydatarow["ActualStart"]);
                            }
                            if (!(mydatarow["ActualEnd"] is DBNull))
                            {
                                outage.enddate = Convert.ToDateTime(mydatarow["ActualEnd"]);
                            }
                        }
                        if (!(mydatarow["PlannedStart"] is DBNull))
                        {
                            outage.plannedstart = Convert.ToDateTime(mydatarow["PlannedStart"]);
                        }
                        if (!(mydatarow["PlannedEnd"] is DBNull))
                        {
                            outage.plannedend = Convert.ToDateTime(mydatarow["PlannedEnd"]);
                        }
                        if (!(mydatarow["PlannedStart"] is DBNull) && !(mydatarow["PlannedEnd"] is DBNull))
                        {
                            outage.plannedduration = Convert.ToInt32(mydatarow["PlannedDuration"]);
                        }
                        if (mytable.Columns.Contains("OpenClose") && !(mydatarow["OpenClose"] is DBNull))
                        {
                            outage.openclose = Convert.ToInt32(mydatarow["OpenClose"]);
                        }
                        if (mytable.Columns.Contains("OutageStatus") && !(mydatarow["OutageStatus"] is DBNull))
                        {
                            outage.outagestatus = mydatarow["OutageStatus"].ToString();
                        }
                        if (mytable.Columns.Contains("OutageType") && !(mydatarow["OutageType"] is DBNull))
                        {
                            outage.outageType = mydatarow["OutageType"].ToString();
                        }
                        if (mytable.Columns.Contains("LastRevised") && !(mydatarow["LastRevised"] is DBNull))
                        {
                            outage.lastrevised = Convert.ToDateTime(mydatarow["LastRevised"]);
                        }
                        if (mytable.Columns.Contains("Causes") && !(mydatarow["Causes"] is DBNull))
                        {
                            outage.causes = mydatarow["Causes"].ToString();
                        }
                        if (mytable.Columns.Contains("RemovedDate") && !(mydatarow["RemovedDate"] is DBNull))
                        {
                            outage.removeddate = Convert.ToDateTime(mydatarow["RemovedDate"]);
                        }
                    }

                    outage.branch_location = FindCoordinate(outage.branch);
                    outage.tobranch_location = FindCoordinate(outage.tobranch);

                    if (SelectedOutagesSourceType == OutagesSourceType.Actual && OutagesStatusForced == false && outage.ticketId == "0")
                    {
                        continue;
                    }
                    if (SelectedOutagesSourceType == OutagesSourceType.Actual && OutagesStatusPlanned == false && outage.ticketId != "0")
                    {
                        continue;
                    }
                    if (OutagesIncludeComplete == false && (outage.status == "Complete" || outage.status == "Completed"))
                    {
                        continue;
                    }
                    if (SelectedOutagesSourceType == OutagesSourceType.Planned && OutageClose == false && outage.openclose == 1)
                    {
                        continue;
                    }
                    if (SelectedOutagesSourceType == OutagesSourceType.Planned && OutageOpen == false && outage.openclose == 0)
                    {
                        continue;
                    }
                    if (SelectedOutagesSourceType == OutagesSourceType.Planned && (bool)Outage_Start_radioButton.IsChecked &&
                                               (outage.plannedstart == null || ((DateTime)outage.plannedstart).Date != startTime.Date))
                    {
                        continue;
                    }
                    if (MinText != null && outage.plannedduration < Int32.Parse(MinText))
                    {
                        continue;
                    }
                    if (MaxText != null && outage.plannedduration > Int32.Parse(MaxText))
                    {
                        continue;
                    }
                    if (MinVoltText != null && outage.voltage < Int32.Parse(MinVoltText))
                    {
                        continue;
                    }
                    if (MaxVoltText != null && outage.voltage > Int32.Parse(MaxVoltText))
                    {
                        continue;
                    }
                    if (outage.zone != null)
                    {
                        string zone = outage.zone;
                        if (mOutageZoneHash.ContainsKey(zone))
                        {
                            zone = mOutageZoneHash[zone];
                        }
                        if (outage.zone.Trim().Length > 0 && ZoneListBox.Items.Contains(zone) && !ZoneListBox.SelectedItems.Contains(zone))
                        {
                            continue;
                        }
                    }


                    //if (OutageEquipmentTypeListBox.Items.Contains(outage.equipmentType) &&
                    //    !OutageEquipmentTypeListBox.SelectedItems.Contains(outage.equipmentType))
                    //{
                    //    continue;
                    //}
                }
                catch (Exception ex)
                {

                }
                mOutagelist.Add(outage);
            }
            VayuConnection.Close();
            return mOutagelist;
        }

        /// <summary>
        /// Draws the line.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="finish">The finish.</param>
        /// <param name="source">The source.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="myoutage">The myoutage.</param>
        /// <param name="description">The description.</param>
        private void DrawLine(Location start, Location finish, string source, string sink, Outage myoutage, string description)
        {
            Polygon soucePolygon = new Polygon();
            Polygon sinkPolygon = new Polygon();
            mOutageLineColor = GetOutageColor(myoutage.voltage);
            soucePolygon.Fill = new SolidColorBrush(mOutageLineColor);
            soucePolygon.Fill.Opacity = 0.5;
            sinkPolygon.Fill = new SolidColorBrush(mOutageLineColor);
            sinkPolygon.Fill.Opacity = 0.5;
            PointCollection myPointCollection = new PointCollection();
            double unitlength = 5.5;
            myPointCollection.Add(new Point(0, unitlength));
            myPointCollection.Add(new Point(unitlength, 0));
            myPointCollection.Add(new Point(0, -unitlength));
            myPointCollection.Add(new Point(-unitlength, 0));
            soucePolygon.Points = myPointCollection;
            sinkPolygon.Points = myPointCollection;
            ToolTip sourcett = new ToolTip();
            ToolTip sinktt = new ToolTip();
            sourcett.Content = source;
            sinktt.Content = sink;
            sourcett.FontWeight = FontWeights.Bold;
            sinktt.FontWeight = FontWeights.Bold;
            soucePolygon.ToolTip = sourcett;
            sinkPolygon.ToolTip = sinktt;
            MapLayer.SetPosition(soucePolygon, start);
            ToolTipService.SetShowDuration(soucePolygon, 300000);
            mOutageMapLayer.Children.Add(soucePolygon);
            MapLayer.SetPosition(sinkPolygon, finish);
            ToolTipService.SetShowDuration(sinkPolygon, 300000);
            mOutageMapLayer.Children.Add(sinkPolygon);
            LocationCollection coll = new LocationCollection();
            coll.Add(start);
            coll.Add(finish);
            MapPolyline line = new MapPolyline();
            line.Stroke = new SolidColorBrush(mOutageLineColor);
            line.StrokeThickness = 4.0;
            line.Opacity = 0.6;
            line.Locations = coll;
            ToolTip tt = new ToolTip();
            tt.Content = description;
            tt.FontWeight = FontWeights.Bold;
            line.ToolTip = tt;
            ToolTipService.SetShowDuration(line, 300000);
            mOutageMapLayer.Children.Add(line);
            string primarykey = string.Format("{0}!{1}!{2}!{3}", myoutage.equipmentType, myoutage.branch,
                                              myoutage.voltage, myoutage.equipment);
            ContextMenu mymenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Uid = primarykey;
            menuItem.Header = "ShowHistory";
            menuItem.Click += new RoutedEventHandler(menuItem_Click);
            //menuItem.Click += new LineDelegate(this.print_line(line));
            mymenu.Items.Add(menuItem);
            line.ContextMenu = mymenu;

        }

        /// <summary>
        /// Draws the XFMR.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="myoutage">The myoutage.</param>
        /// <param name="description">The description.</param>
        private void DrawXFMR(Location start, Outage myoutage, string description)
        {
            Ellipse myEllipse = new Ellipse();
            mOutageLineColor = GetOutageColor(myoutage.voltage);
            myEllipse.Fill = new SolidColorBrush(mOutageLineColor);
            myEllipse.Fill.Opacity = 0.3;
            //myEllipse.Fill = new SolidColorBrush(Get_Color());
            myEllipse.Stroke = new SolidColorBrush(mOutageLineColor);
            myEllipse.StrokeThickness = 0.5;
            double diameter = 20;
            double radius = diameter / 2;
            myEllipse.Width = diameter;
            myEllipse.Height = diameter;
            myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = description;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(start);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            mOutageMapLayer.Children.Add(myEllipse);

            string primarykey = string.Format("{0}!{1}!{2}!{3}", myoutage.equipmentType, myoutage.branch,
                                              myoutage.voltage, myoutage.equipment);

            ContextMenu mymenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Uid = primarykey;
            menuItem.Header = "Show History";
            menuItem.Click += new RoutedEventHandler(menuItem_Click);
            mymenu.Items.Add(menuItem);
            myEllipse.ContextMenu = mymenu;
        }

        /// <summary>
        /// Gets the color of the outage.
        /// </summary>
        /// <param name="kvlevel">The kvlevel.</param>
        /// <returns></returns>
        private Color GetOutageColor(int kvlevel)
        {
            int[] colorarray = new int[2] { 230, 345 };
            if (kvlevel < colorarray[0])
            {
                return Colors.Purple;
            }
            else if (colorarray[0] <= kvlevel && kvlevel < colorarray[1])
            {
                return Colors.DarkGreen;
            }
            else
            {
                return Colors.Blue;
            }
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the menuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void menuItem_Click(object sender, RoutedEventArgs e)
        {
            DataTable temptable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            MenuItem menu = (MenuItem)sender;
            ContextMenu contextMenu = menu.Parent as ContextMenu;

            string[] KeyArray = (menu.Uid).Split('!');
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            if (mMarketInContext == mMarkets["ERCOT"])
            {
                mSelectERCOTHistoricalOutageCommand.Parameters["@equipmenttype"].Value = KeyArray[0];
                mSelectERCOTHistoricalOutageCommand.Parameters["@branch"].Value = KeyArray[1];
                mSelectERCOTHistoricalOutageCommand.Parameters["@equipment"].Value = KeyArray[3];
                adapter.SelectCommand = mSelectERCOTHistoricalOutageCommand;
            }

            adapter.Fill(temptable);
            VayuConnection.Close();
            HistorydataGrid.DataContext = temptable;
            Historytab.Focus();
        }
    }
}
