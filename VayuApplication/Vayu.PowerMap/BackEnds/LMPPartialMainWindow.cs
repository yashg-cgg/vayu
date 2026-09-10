using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Vayu.EnergyLMP;
using Vayu.LatestConstraintsInformationLibrary;
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
        /// <summary>
        /// The colorhash
        /// </summary>
        private Dictionary<int, Color> colorhash = new Dictionary<int, Color>()
                                                   {
                                                       {-20, Colors.Purple},
                                                       {0, Colors.MediumPurple},
                                                       {10, Colors.DodgerBlue},
                                                       {20, Colors.Green},
                                                       {40, Colors.LimeGreen},
                                                       {60, Colors.LightGreen},
                                                       {80, Colors.YellowGreen},
                                                       {100, Colors.Yellow},
                                                       {150, Colors.Orange},
                                                       {300, Colors.OrangeRed},
                                                       {9999, Colors.Red},
                                                   };

        /// <summary>
        /// The dynamic colorhash
        /// </summary>
        private Dictionary<int, Color> Dynamic_colorhash = new Dictionary<int, Color>()
                                                           {
                                                               {1, Colors.Purple},
                                                               {2, Colors.MediumPurple},
                                                               {3, Colors.DodgerBlue},
                                                               {4, Colors.DarkGreen},
                                                               {5, Colors.Green},
                                                               {6, Colors.LightGreen},
                                                               {7, Colors.YellowGreen},
                                                               {8, Colors.Yellow},
                                                               {9, Colors.Orange},
                                                               {10, Colors.OrangeRed},
                                                               {11, Colors.Red},
                                                           };

        /// <summary>
        /// The SPP hash
        /// </summary>
        private HashSet<int> sppHash;
        /// <summary>
        /// The LMP equipment list
        /// </summary>
        private ObservableCollection<string> lmpEquipmentList;
        /// <summary>
        /// Gets or sets the LMP equipment list.
        /// </summary>
        /// <value>
        /// The LMP equipment list.
        /// </value>
        public ObservableCollection<string> LmpEquipmentList
        {
            get { return lmpEquipmentList; }
            set { lmpEquipmentList = value; RaisePropertyChanged("LmpEquipmentList"); }
        }

        /// <summary>
        /// The lmpmy variable
        /// </summary>
        private ObservableCollection<point> lmpmyVar;
        /// <summary>
        /// Gets or sets the LMP list.
        /// </summary>
        /// <value>
        /// The LMP list.
        /// </value>
        public ObservableCollection<point> LMPList
        {
            get { return lmpmyVar; }
            set { lmpmyVar = value; RaisePropertyChanged("LMPList"); }
        }

        /// <summary>
        /// The LMP filter toggle
        /// </summary>
        private string lmpFilterToggle;
        /// <summary>
        /// Gets or sets the LMP filter toggle.
        /// </summary>
        /// <value>
        /// The LMP filter toggle.
        /// </value>
        public string LMPFilterToggle
        {
            get { return lmpFilterToggle; }
            set { lmpFilterToggle = value; RaisePropertyChanged("LMPFilterToggle"); }
        }

        /// <summary>
        /// Loads the LMP equipments.
        /// </summary>
        public void LoadLMPEquipments()
        {
            try
            {
                using (new WaitCursor())
                {
                    mMarketNodeList = mNodeList.Where(p => p.Marketkey == mMarketInContext).ToList();
                    List<string> nodeTypeList = mMarketNodeList.Select(i => i.Type).Distinct().ToList();
                    nodeTypeList.Sort();
                    LmpEquipmentList = new ObservableCollection<string>(nodeTypeList);
                    LMPEquipmentTypeListBox.SelectAll();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// LMPs the main.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <exception cref="System.Exception">LMP Exception" + e.Message</exception>
        private void LMPMain([CallerMemberName] string memberName = "")
        {
            if (MainCalendarSilder.IsRange.GetValueOrDefault())
                return;

            MapControl.myMap.Children.Remove(mLMPMapLayer);
            mLMPMapLayer.Children.Clear();
            mPlacemarkList.Clear();
            mNodeListMapped.Clear();
            try
            {
                using (new WaitCursor())
                {
                    if (mMarketNodeList == null)
                    {
                        return;
                    }
                    if (!memberName.ToLower().Contains("zone") && !memberName.ToLower().Contains("equipment") &&
                        !memberName.ToLower().Contains("LMPPJMUpTo".ToLower()))
                    {
                        if (SelectedLMPPriceType == LMPPriceType.RT5 || SelectedLMPPriceType == LMPPriceType.RT5Max || SelectedLMPPriceType == LMPPriceType.RT5Min)
                        {
                            DARTNode.GetAllFiveMinDarts(mMarketInContext, exactTime, exactTime.AddHours(1));
                        }
                        else
                        {
                            if (SelectedLMPPriceType == LMPPriceType.DA)
                            {
                                LiveCheckBox.IsChecked = false;
                            }
                            //TimeZone curTimeZone = TimeZone.CurrentTimeZone;
                            //DateTime startdate = curTimeZone.IsDaylightSavingTime(exactDate) ? exactDate : exactDate.AddHours(-3);
                            //DARTNode.GetAllDarts(mMarketInContext, startdate, exactDate.AddDays(1));
                            DARTNode.GetAllDarts(mMarketInContext, exactDate, exactDate.AddDays(1));
                        }
                    }

                    UpdatePrices();
                    BuildLMPMap(mPlacemarkList);
                }

                if (isLMPCheck.GetValueOrDefault())
                {
                    MapControl.myMap.Children.Add(mLMPMapLayer);
                }
            }
            catch (Exception e)
            {
                throw new Exception("LMP Exception" + e.Message);
            }
        }

        /// <summary>
        /// Updates the prices.
        /// </summary>
        private void UpdatePrices()
        {
            if (mMarketNodeList == null)
                return;

            List<string> szList = ZoneListBox.SelectedItems.Cast<string>().ToList();
            List<string> slList = LMPEquipmentTypeListBox.SelectedItems.Cast<string>().ToList();
            Func<int?, string> getKey = null;
            bool bpriceTyp = false;

            if (SelectedLMPPriceType == LMPPriceType.RT5 || SelectedLMPPriceType == LMPPriceType.RT5Max || SelectedLMPPriceType == LMPPriceType.RT5Min
                   || (SelectedLMPPriceType == LMPPriceType.RT && LiveCheckBox.IsChecked == true) || (SelectedLMPPriceType == LMPPriceType.DART && LiveCheckBox.IsChecked == true))
                bpriceTyp = true;

            if (mMarketInContext == mMarkets["PJM"])
            {
                getKey = (nKey) => { return exactTime.ToString() + nKey.ToString(); };
            }
            else if ((mMarketInContext == mMarkets["ERCOT"]) || (mMarketInContext == mMarkets["MISO"]))
            {
                getKey = (nKey) => { return exactTime.AddHours(-1).ToString() + nKey.ToString(); };
            }
            else if (mMarketInContext == mMarkets["NYISO"])
            {
                getKey = (nKey) => { return exactTime.AddHours(1).ToString() + nKey.ToString(); };
            }
            else if (mMarketInContext == mMarkets["SPP"])
            {
                getKey = (nKey) => { return exactTime.ToString() + nKey.ToString(); };
            }
            else if (mMarketInContext == mMarkets["CAISO"])
            {
                getKey = (nKey) => { return exactTime.AddHours(-3).ToString() + nKey.ToString(); };
            }

            IEnumerable<Node_Geo> zoneFilter = mMarketNodeList;
            if (szList.Count != 0)
                zoneFilter = mMarketNodeList.Where(x => szList.Contains(x.Zone));

            IEnumerable<Node_Geo> equipFilter = zoneFilter;
            if (slList.Count != 0)
                equipFilter = zoneFilter.Where(x => slList.Contains(x.Type));

            if (LMPPJMUpTo && mMarketInContext == mMarkets["PJM"])
            {
                foreach (Node_Geo item in equipFilter.Where(x => mPJMSourceSinkUptoList.Contains(x.NodeName)))
                    FillLmpPoints(item, SelectedLMPPriceType, getKey, bpriceTyp);
            }
            else if (LMPPJMUpTo && mMarketInContext == mMarkets["PJM"])
            {
                foreach (var item in equipFilter.Where(x => sppHash.Contains(x.NodeKey.Value)))
                    FillLmpPoints(item, SelectedLMPPriceType, getKey, bpriceTyp);
            }
            else
            {
                foreach (Node_Geo item in equipFilter)
                    FillLmpPoints(item, SelectedLMPPriceType, getKey, bpriceTyp);
            }
        }

        /// <summary>
        /// Fills the LMP points.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="priceType">Type of the price.</param>
        /// <param name="getKey">The get key.</param>
        /// <param name="bpriceTyp">if set to <c>true</c> [bprice typ].</param>
        private void FillLmpPoints(Node_Geo item, LMPPriceType priceType, Func<int?, string> getKey, bool bpriceTyp)
        {
            if (!item.NodeKey.HasValue)
                return;

            string key = getKey(item.NodeKey);
            point p = new point();
            placemark place = new placemark();
            p.NodeName = item.NodeName;
            p.Zone = item.Zone;
            p.Type = item.Type;
            p.FuelType = item.FuelType;
            p.NodeKey = item.NodeKey.Value;
            p.MarketDateTime = startTime;
            p.Parent = place;
            place.Longitude = item.Longitude;
            place.Latitude = item.Latitude;
            place.Station = item.PSSEname;
            place.NodeType = item.NodeType;
            place.Type = item.Type;
            place.points = new List<point>();
            place.marketkey = item.Marketkey;
            place.points.Add(p);
            mPlacemarkList.Add(place);

            if (item.KV.HasValue)
                p.KV = item.KV.Value;

            if (bpriceTyp)
            {
                key = exactTime.ToString() + item.NodeKey.ToString();
                if (DARTNode.sRTFiveMinHash.ContainsKey(key))
                {
                    ConcurrentDictionary<int, double> minuteHash = DARTNode.sRTFiveMinHash[key];
                    List<int> minuteKeys = minuteHash.Keys.ToList<int>();
                    minuteKeys.Sort();
                    if (priceType == LMPPriceType.RT5)
                    {
                        p.LMP = Math.Round(minuteHash[minuteKeys[minuteKeys.Count - 1]], 2);
                    }
                    else if (priceType == LMPPriceType.RT5Max)
                    {
                        double maxMinute = minuteHash.Values.Max<double>();
                        p.LMP = maxMinute;
                    }
                    else if (priceType == LMPPriceType.RT)
                    {
                        double avgMinute = minuteHash.Values.Average();
                        p.LMP = avgMinute;
                    }
                    else if (priceType == LMPPriceType.DART)
                    {
                        if (DARTNode.sDAHash.ContainsKey(key))
                        {
                            double avgMinute = minuteHash.Values.Average() - DARTNode.sDAHash[key];
                            p.LMP = avgMinute;
                        }
                    }
                    else
                    {
                        double minMinute = minuteHash.Values.Min<double>();
                        p.LMP = minMinute;
                    }
                }
            }
            if (priceType == LMPPriceType.RT && LiveCheckBox.IsChecked == false && DARTNode.sRTHash.ContainsKey(key))
            {
                p.LMP = Math.Round(DARTNode.sRTHash[key], 2);
            }
            if (priceType == LMPPriceType.DA && DARTNode.sDAHash.ContainsKey(key))
            {
                p.LMP = Math.Round(DARTNode.sDAHash[key], 2);
            }
            if (priceType == LMPPriceType.DART && LiveCheckBox.IsChecked == false && DARTNode.sDAHash.ContainsKey(key) && DARTNode.sRTHash.ContainsKey(key))
            {
                p.LMP = Math.Round(DARTNode.sRTHash[key] - DARTNode.sDAHash[key], 2);
            }
        }

        /// <summary>
        /// Loads the SPP settlement hash.
        /// </summary>
        private void LoadSPPSettlementHash()
        {
            sppHash = new HashSet<int>();

            try
            {
                SqlCommand cmd = EnergyMapHelper.GetTradingDBCommand();
                cmd.CommandText = "select nodekey from node where nodename in (select distinct SettlementLocationName from SPP.SettlementLocationName) and marketkey=12 order by NodeKey";
                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                    sppHash.Add(Configuration.GetInt(reader[0]));

                reader.Close();
                cmd.Connection.Close();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Builds the LMP map.
        /// </summary>
        /// <param name="placemarklist">The placemarklist.</param>
        private void BuildLMPMap(List<placemark> placemarklist)
        {
            List<string> names = new List<string>();
            List<point> pointList = new List<point>();

            if (placemarklist.Count != 0)
            {
                legendMax = placemarklist.Max(p => p.points.Max(v => v.LMP));
            }
            if (placemarklist.Count != 0)
            {
                legendMin = placemarklist.Min(p => p.points.Min(v => v.LMP));
            }
            foreach (placemark place in placemarklist)
            {
                Shape myobj;
                double MaxLMP = place.points.Max(p => p.LMP);
                string description = "";
                string Type = place.Type.ToUpper();
                double lmp = place.points.Max(p => p.LMP);
                Location location = new Location(place.Latitude, place.Longitude);

                foreach (point p in place.points)
                {
                    description += p.NodeName + "\n" + p.LMP.ToString("C2") + "\n" + ("(" + p.Zone + ")");
                    //description += p.NodeName + "\n" + p.LMP.ToString("C2");
                    pointList.Add(p);
                    names.Add(p.NodeName);
                }
                switch (Type)
                {
                    case "ZONE":
                        myobj = PlaceStar(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "HUB":
                        myobj = PlaceTriangle(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "AGGREGATE":
                        myobj = PlaceDiamond(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "INTERFACE":
                        myobj = PlaceSquare(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "GENERATOR":
                        myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "LOAD":
                        myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "BUS":
                        myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "EXT":
                        myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    case "EHV":
                        myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                    default:
                        myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                        AddObject(myobj, place, description);
                        break;
                }
            }
            LMPList = new ObservableCollection<point>(pointList);
            LMP_AutoCompleteBox.ItemsSource = names;
        }

        /// <summary>
        /// Places the circle.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        private Ellipse PlaceCircle(Location location, Color color, string text1)
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = new SolidColorBrush(color);
            double diameter = 10;
            double radius = diameter / 2;
            myEllipse.Width = diameter;
            myEllipse.Height = diameter;
            //myEllipse.Margin = new Thickness(-radius / Math.Sqrt(2), -radius / Math.Sqrt(2), 0, 0);
            myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            return myEllipse;
        }

        /// <summary>
        /// Places the circle with stroke.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        private Ellipse PlaceCircleWithStroke(Location location, Color color, string text1)
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = new SolidColorBrush(color);
            myEllipse.Stroke = new SolidColorBrush(Colors.Black);
            myEllipse.StrokeThickness = 0.8;
            double diameter = 10;
            double radius = diameter / 2;
            myEllipse.Width = diameter;
            myEllipse.Height = diameter;
            //myEllipse.Margin = new Thickness(-radius / Math.Sqrt(2), -radius / Math.Sqrt(2), 0, 0);
            myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            return myEllipse;
        }

        /// <summary>
        /// Places the square.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        private Rectangle PlaceSquare(Location location, Color color, string text1)
        {
            Rectangle MyRectangle = new Rectangle();
            MyRectangle.Fill = new SolidColorBrush(color);
            MyRectangle.Stroke = new SolidColorBrush(Colors.Black);
            MyRectangle.StrokeThickness = 0.8;
            double length = 10;
            MyRectangle.Width = length;
            MyRectangle.Height = length;
            double diagonal = length * Math.Sqrt(2) / 2;
            MyRectangle.Margin = new Thickness(-length / 2, -length / 2, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            MyRectangle.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyRectangle, loc);
            ToolTipService.SetShowDuration(MyRectangle, 300000);
            return MyRectangle;
        }

        /// <summary>
        /// Places the diamond.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        private Polygon PlaceDiamond(Location location, Color color, string text1)
        {
            Polygon MyPolygon = new Polygon();
            MyPolygon.Fill = new SolidColorBrush(color);
            MyPolygon.Stroke = new SolidColorBrush(Colors.Black);
            MyPolygon.StrokeThickness = 0.8;
            PointCollection myPointCollection = new PointCollection();
            double unitlength = 5.5;
            myPointCollection.Add(new Point(0, unitlength));
            myPointCollection.Add(new Point(unitlength, 0));
            myPointCollection.Add(new Point(0, -unitlength));
            myPointCollection.Add(new Point(-unitlength, 0));
            MyPolygon.Points = myPointCollection;
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            MyPolygon.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyPolygon, loc);
            ToolTipService.SetShowDuration(MyPolygon, 300000);
            return MyPolygon;
        }

        /// <summary>
        /// Places the triangle.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        private Polygon PlaceTriangle(Location location, Color color, string text1)
        {
            Polygon MyTriangle = new Polygon();
            MyTriangle.Fill = new SolidColorBrush(color);
            MyTriangle.Stroke = new SolidColorBrush(Colors.Black);
            MyTriangle.StrokeThickness = 0.8;
            PointCollection myPointCollection = new PointCollection();
            double radius = 6;
            for (int i = 120; i <= 360; i = i + 120)
            {
                myPointCollection.Add(new Point(radius * Math.Sin(i * Math.PI / 180), radius * Math.Cos(i * Math.PI / 180)));
            }
            MyTriangle.Points = myPointCollection;
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            MyTriangle.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyTriangle, loc);
            ToolTipService.SetShowDuration(MyTriangle, 300000);
            return MyTriangle;
        }

        /// <summary>
        /// Places the star.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        private Polygon PlaceStar(Location location, Color color, string text1)
        {
            Polygon MyStar = new Polygon();
            MyStar.Fill = new SolidColorBrush(color);
            MyStar.Stroke = new SolidColorBrush(Colors.Black);
            MyStar.StrokeThickness = 0.8;
            PointCollection myPointCollection = new PointCollection();
            double outter_radius = 6;
            double inner_radius = 3;

            for (int i = 36; i <= 324; i = i + 72)
            {
                myPointCollection.Add(new Point(outter_radius * Math.Sin(i * Math.PI / 180),
                                                outter_radius * Math.Cos(i * Math.PI / 180)));
                myPointCollection.Add(new Point(inner_radius * Math.Sin((i + 36) * Math.PI / 180),
                                                inner_radius * Math.Cos((i + 36) * Math.PI / 180)));
            }
            MyStar.Points = myPointCollection;
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            MyStar.ToolTip = tt;
            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyStar, loc);
            ToolTipService.SetShowDuration(MyStar, 300000);
            return MyStar;
        }

        /// <summary>
        /// Gets the brush.
        /// </summary>
        /// <param name="lmp">The LMP.</param>
        /// <returns></returns>
        private SolidColorBrush GetBrush(double lmp)
        {
            BrushConverter brush = new BrushConverter();
            if (lmp < -10)
                return new SolidColorBrush(Color.FromRgb(115, 0, 148));
            else if (lmp >= -10 && lmp < 0)
                return new SolidColorBrush(Color.FromRgb(0, 77, 173));
            else if (lmp >= 0 && lmp < 6)
                return new SolidColorBrush(Color.FromRgb(49, 89, 255));
            else if (lmp >= 6 && lmp < 14)
                return new SolidColorBrush(Color.FromRgb(57, 113, 255));
            else if (lmp >= 14 && lmp < 16)
                return new SolidColorBrush(Color.FromRgb(57, 138, 255));
            else if (lmp >= 16 && lmp < 20)
                return new SolidColorBrush(Color.FromRgb(57, 162, 255));
            else if (lmp >= 20 && lmp < 30)
                return new SolidColorBrush(Color.FromRgb(49, 190, 255));
            else if (lmp >= 30 && lmp < 34)
                return new SolidColorBrush(Color.FromRgb(41, 215, 255));
            else if (lmp >= 34 && lmp < 38)
                return new SolidColorBrush(Color.FromRgb(24, 243, 255));
            else if (lmp >= 38 && lmp < 42)
                return new SolidColorBrush(Color.FromRgb(41, 255, 247));
            else if (lmp >= 42 && lmp < 46)
                return new SolidColorBrush(Color.FromRgb(90, 255, 222));
            else if (lmp >= 46 && lmp < 50)
                return new SolidColorBrush(Color.FromRgb(123, 255, 206));
            else if (lmp >= 50 && lmp < 56)
                return new SolidColorBrush(Color.FromRgb(148, 255, 173));
            else if (lmp >= 56 && lmp < 62)
                return new SolidColorBrush(Color.FromRgb(173, 255, 156));
            else if (lmp >= 62 && lmp < 68)
                return new SolidColorBrush(Color.FromRgb(198, 255, 132));
            else if (lmp >= 68 && lmp < 76)
                return new SolidColorBrush(Color.FromRgb(206, 255, 107));
            else if (lmp >= 76 && lmp < 82)
                return new SolidColorBrush(Color.FromRgb(231, 255, 82));
            else if (lmp >= 82 && lmp < 90)
                return new SolidColorBrush(Color.FromRgb(239, 255, 57));
            else if (lmp >= 90 && lmp < 100)
                return new SolidColorBrush(Color.FromRgb(255, 255, 24));
            else if (lmp >= 100 && lmp < 115)
                return new SolidColorBrush(Color.FromRgb(255, 239, 0));
            else if (lmp >= 115 && lmp < 125)
                return new SolidColorBrush(Color.FromRgb(255, 219, 0));
            else if (lmp >= 125 && lmp < 150)
                return new SolidColorBrush(Color.FromRgb(255, 195, 0));
            else if (lmp >= 150 && lmp < 200)
                return new SolidColorBrush(Color.FromRgb(255, 170, 0));
            else if (lmp >= 200 && lmp < 250)
                return new SolidColorBrush(Color.FromRgb(255, 150, 0));
            else if (lmp >= 250 && lmp < 300)
                return new SolidColorBrush(Color.FromRgb(156, 162, 165));
            else if (lmp >= 300 && lmp < 400)
                return new SolidColorBrush(Color.FromRgb(255, 101, 0));
            else if (lmp >= 400 && lmp < 500)
                return new SolidColorBrush(Color.FromRgb(255, 81, 0));
            else if (lmp >= 500 && lmp < 600)
                return new SolidColorBrush(Color.FromRgb(255, 48, 0));
            else if (lmp >= 600)
                return new SolidColorBrush(Color.FromRgb(255, 48, 0));
            else
                return Brushes.Gold;
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <param name="lmp">The LMP.</param>
        /// <returns></returns>
        private Color GetColor(double lmp)
        {
            if (!LMPNormColorScaleType)
            {
                SolidColorBrush solidColorBrush = GetBrush(lmp);
                double total_scale = legendMax - legendMin + 1;
                double normalized_lmp = (lmp - legendMin) * 11 / total_scale;
                var color = Dynamic_colorhash.Where(p => p.Key >= normalized_lmp);
                int colorkey = color.Min(d => d.Key);
                return solidColorBrush.Color;// Dynamic_colorhash[colorkey];
            }
            else
            {
                //static scale
                var color = colorhash.Where(p => p.Key >= lmp);
                int colorkey = color.Min(p => p.Key);
                return colorhash[colorkey];
            }
        }

        /// <summary>
        /// Adds the object.
        /// </summary>
        /// <param name="myshape">The myshape.</param>
        /// <param name="place">The place.</param>
        /// <param name="description">The description.</param>
        private void AddObject(Shape myshape, placemark place, string description)
        {
            placemarkbject ojb = new placemarkbject();
            //Add station object for navigation purposes.
            string nodedetail = "";
            foreach (point p in place.points)
            {
                string name = p.NodeName;
                ojb.name = place.Station;
                ojb.descripton = description;
                ojb.location = new Location(place.Latitude, place.Longitude);
                nodedetail = string.Format("{0}@{1}@{2}@{3}@{4}@{5}@{6}", p.NodeKey.ToString(), p.Parent.marketkey, p.NodeName, p.Zone,
                    p.Parent.NodeType, place.Latitude.ToString(), place.Longitude.ToString());
            }
            //Add shape to the map third_maplayer
            mLMPMapLayer.Children.Add(myshape);
            ContextMenu pinMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Uid = nodedetail;
            menuItem.Header = "Node Analyzer";
            menuItem.Click += new RoutedEventHandler(NodeSpreadAnalysis_MenuItem_Click);
            pinMenu.Items.Add(menuItem);
            MenuItem lmpGraphmenuItem = new MenuItem();
            lmpGraphmenuItem.Uid = nodedetail;
            lmpGraphmenuItem.Header = "LMP Graph";
            lmpGraphmenuItem.Click += new RoutedEventHandler(LMPGraph_MenuItem_Click);
            pinMenu.Items.Add(lmpGraphmenuItem);
            myshape.ContextMenu = pinMenu;
            MenuItem latlongmenuItem = new MenuItem();
            latlongmenuItem.Uid = nodedetail;
            latlongmenuItem.Header = "Update Location";
            latlongmenuItem.Click += new RoutedEventHandler(UpdateLocation_MenuItem_Click);
            pinMenu.Items.Add(latlongmenuItem);
            myshape.ContextMenu = pinMenu;
            if (mMarketInContext == mMarkets["PJM"])
            {
                MenuItem drillDownmenuItem = new MenuItem();
                drillDownmenuItem.Uid = nodedetail;
                drillDownmenuItem.Header = "DrillDown Exposure";
                drillDownmenuItem.Click += new RoutedEventHandler(DrillDownExposure_MenuItem_Click);
                pinMenu.Items.Add(drillDownmenuItem);
                myshape.ContextMenu = pinMenu;
            }
        }

        //private ContextMenu pinMenu;

        //private void BuildContextMenu()
        //{
        //    pinMenu = new ContextMenu();
        //    MenuItem menuItem = new MenuItem();
        //    menuItem.Uid = nodedetail;
        //    menuItem.Header = "Node Analyzer";
        //    menuItem.Click += new RoutedEventHandler(NodeSpreadAnalysis_MenuItem_Click);
        //    pinMenu.Items.Add(menuItem);
        //    MenuItem lmpGraphmenuItem = new MenuItem();
        //    lmpGraphmenuItem.Uid = nodedetail;
        //    lmpGraphmenuItem.Header = "LMP Graph";
        //    lmpGraphmenuItem.Click += new RoutedEventHandler(LMPGraph_MenuItem_Click);
        //    pinMenu.Items.Add(lmpGraphmenuItem);
        //    myshape.ContextMenu = pinMenu;
        //    MenuItem latlongmenuItem = new MenuItem();
        //    latlongmenuItem.Uid = nodedetail;
        //    latlongmenuItem.Header = "Update Location";
        //    latlongmenuItem.Click += new RoutedEventHandler(UpdateLocation_MenuItem_Click);
        //    pinMenu.Items.Add(latlongmenuItem);
        //    myshape.ContextMenu = pinMenu;
        //    if (mMarketInContext == mMarkets["PJM"])
        //    {
        //        MenuItem drillDownmenuItem = new MenuItem();
        //        drillDownmenuItem.Uid = nodedetail;
        //        drillDownmenuItem.Header = "DrillDown Exposure";
        //        drillDownmenuItem.Click += new RoutedEventHandler(DrillDownExposure_MenuItem_Click);
        //        pinMenu.Items.Add(drillDownmenuItem);
        //        myshape.ContextMenu = pinMenu;
        //    }
        //}
    }
}
