
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Vayu.LatestConstraintsInformationLibrary;
using Vayu.MarketView.Model;
using Vayu.MarketView.ViewModels;



namespace Vayu.MarketView.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declaration

        /// <summary>
        /// The source sink list
        /// </summary>
        List<Tuple<string, string>> sourceSinkList = new List<Tuple<string, string>>();
        /// <summary>
        /// The view model
        /// </summary>
        private MainWindowViewModel viewModel;
        /// <summary>
        /// The m zone layer
        /// </summary>
        private MapLayer mZoneLayer = new MapLayer();
        /// <summary>
        /// The m market location
        /// </summary>
        private Hashtable mMarketLocation = new Hashtable(){
            {0, new Location(38.462, -81.843)},
            {1, new Location(38.462, -81.843)},
            {2, new Location(42.122, -91.775)},
            {3, new Location(42.71827,  -73.93397 )},
            {7, new Location(36.778261,  -119.417932 )},
            {12, new Location(34.75324 ,-92.44964 )},
            {9, new Location(31.19, -98.05)}};

        /// <summary>
        /// The tim
        /// </summary>
        System.Timers.Timer tim = new System.Timers.Timer();
        /// <summary>
        /// The UI context
        /// </summary>
        private SynchronizationContext _uiContext = SynchronizationContext.Current;
        /// <summary>
        /// The factor
        /// </summary>
        double factor = 0.00001;

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            buttonRoad.Click += new RoutedEventHandler(buttonRoad_Click);
            buttonAerial.Click += new RoutedEventHandler(buttonAerial_Click);
            buttonAerialWithLabels.Click += new RoutedEventHandler(buttonAerialWithLabels_Click);
            ZoomInbutton.Click += new RoutedEventHandler(ZoomInbutton_Click);
            ZoomOutbutton.Click += new RoutedEventHandler(ZoomOutbutton_Click);
            LMPCheckBox.Click += new RoutedEventHandler(CheckBox_Checked);
            ConstraintCheckBox.Click += new RoutedEventHandler(ConstrainsCheckBox_Checked);
        }
        #region Methods

        /// <summary>
        /// Loads the color zones.
        /// </summary>
        private void LoadColorZones()
        {
            MainWindowViewModel model = new MainWindowViewModel();
            int key = (int)market.SelectedValue;
            //List<ZoneInfo> zlist = ZoneModel.GetZoneList(key);
            //model.ListZone = zlist;
            MainWindow mW = new MainWindow();
            foreach (var item in model.ListZone)
            {
                foreach (var zoneitem in item.RegionInfoList)
                {
                    MapPolygon zonePolygon = new MapPolygon();

                    #region ZoneColor
                    switch (zoneitem.Name)
                    {
                        case "AEP":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
                            break;
                        case "APS":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 219, 133, 108));
                            break;
                        case "AEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 77, 77));
                            break;
                        case "ATSI":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 179, 191, 128));
                            break;
                        case "BGE":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 204, 204));
                            break;
                        case "COMED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 153, 0));
                            break;
                        case "DAYTON":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 108, 247, 49));
                            break;
                        case "DEOK":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 51, 153, 255));
                            break;
                        case "DOM":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 76, 153));
                            break;
                        case "DPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 54, 219));
                            break;
                        case "DUQ":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(50, 204, 204, 0));
                            break;
                        case "JCPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255));
                            break;
                        case "METED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 49, 252, 35));
                            break;
                        case "PECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 153, 76));
                            break;
                        case "PENELEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 103, 57, 238));
                            break;
                        case "PEPCO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 0));
                            break;
                        case "PPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 194, 126, 231));
                            break;
                        case "PSEG":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 255));
                            break;
                        case "RECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 76, 0));
                            break;
                        case "EKPC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 102));
                            break;
                        default:
                            break;
                    }
                    #endregion ZoneColor

                    zonePolygon.Locations = zoneitem.Locations;
                    zonePolygon.ToolTip = zoneitem.Name;
                    zoneLayer.Children.Add(zonePolygon);
                }
            }
        }
        private void LoadMarketColumns()
        {
            int key = (int)market.SelectedValue;
            if (key == 1)
            {
                LatestConstraintGrid.Columns[1].Visibility = System.Windows.Visibility.Visible;
                LatestConstraintGrid.Columns[5].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[6].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[9].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[10].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[11].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[12].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[13].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[14].Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                LatestConstraintGrid.Columns[1].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[5].Visibility = System.Windows.Visibility.Visible;
                //LatestConstraintGrid.Columns[6].Visibility = System.Windows.Visibility.Hidden;
                LatestConstraintGrid.Columns[9].Visibility = System.Windows.Visibility.Visible;
                LatestConstraintGrid.Columns[10].Visibility = System.Windows.Visibility.Visible;
                LatestConstraintGrid.Columns[11].Visibility = System.Windows.Visibility.Visible;
                LatestConstraintGrid.Columns[12].Visibility = System.Windows.Visibility.Visible;
                LatestConstraintGrid.Columns[13].Visibility = System.Windows.Visibility.Visible;
                LatestConstraintGrid.Columns[14].Visibility = System.Windows.Visibility.Visible;
            }
        }
        /// <summary>
        /// Sets the positions.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        public void SetPositions(int p1, int p2, int p3, int p4)
        {
            Height = p1;
            Width = p2;
            Top = p3;
            Left = p4;
        }

        /// <summary>
        /// Res the draw map.
        /// </summary>
        private void ReDrawMap()
        {
            try
            {
                var c = LmpMap.Center;
                c.Latitude += factor;
                factor *= -1;
                LmpMap.SetView(c, LmpMap.ZoomLevel);
            }
            catch (Exception)
            {
            }
        }

        #endregion
        #region RefreshLmpMapMethod
        /// <summary>
        /// Refreshes the map new.
        /// </summary>
        public void RefreshMapNew()
        {
            viewModel = this.DataContext as MainWindowViewModel;
            if (viewModel.LatestLMPList == null)
            {
                return;
            }
            foreach (var item in viewModel.LatestLMPList)
            {
                ObservableCollection<PointMapPath> paths = new ObservableCollection<PointMapPath>();
                NodeGeoDetail lmpLocation = viewModel.NodeLocationHashCache[item.NodeKey] as NodeGeoDetail;
                string zone1 = "";
                if (lmpLocation == null)
                {
                    continue;
                }
                try
                {
                    if (viewModel.mNodeZoneList.ContainsKey(item.NodeName))
                        zone1 = viewModel.mNodeZoneList[item.NodeName];
                    string NodeType = null;
                    double lmp = 0;
                    paths.Add(new PointMapPath
                    {
                        NodeTypeName = lmpLocation.NodeType.ToUpper()
                    });
                    Shape myobj;
                    foreach (var itemm in paths)
                    {
                        NodeType = itemm.NodeTypeName;
                        lmp = item.LMP;
                        break;
                    }
                    string description = "";
                    description = item.NodeName + "\n" + Math.Round(item.LMP, 2).ToString("C2") + "\n" + "(" + zone1 + ")";
                    Location location = new Location(lmpLocation.Latitude, lmpLocation.Longitude);
                    switch (NodeType)
                    {
                        case "ZONE":
                            myobj = PlaceStar(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "HUB":
                            myobj = PlaceTriangle(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "AGGREGATE":
                            myobj = PlaceDiamond(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "INTERFACE":
                            myobj = PlaceSquare(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "GENERATOR":
                            myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "LOAD":
                            myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "BUS":
                            myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "EXT":
                            myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        case "EHV":
                            myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                        default:
                            myobj = PlaceCircleWithStroke(location, GetColor(lmp), description);
                            AddObject(myobj);
                            break;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (viewModel.LatestLMPList != null)
            {
                viewModel.Count = viewModel.LatestLMPList.Count();
            }
        }
        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <param name="lmp">The LMP.</param>
        /// <returns></returns>
        public Color GetColor(double lmp)
        {
            SolidColorBrush solidColorBrush = GetBrush(lmp);
            return solidColorBrush.Color;
        }
        /// <summary>
        /// Gets the brush.
        /// </summary>
        /// <param name="lmp">The LMP.</param>
        /// <returns></returns>
        public SolidColorBrush GetBrush(double lmp)
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
        /// Places the star.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        public Polygon PlaceStar(Location location, Color color, string text1)
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
            Point p0 = LmpMap.LocationToViewportPoint(location);
            Location loc = LmpMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyStar, loc);
            ToolTipService.SetShowDuration(MyStar, 300000);
            return MyStar;
        }
        /// <summary>
        /// Places the circle.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="color">The color.</param>
        /// <param name="text1">The text1.</param>
        /// <returns></returns>
        public Ellipse PlaceCircle(Location location, Color color, string text1)
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
            Point p0 = LmpMap.LocationToViewportPoint(location);
            Location loc = LmpMap.ViewportPointToLocation(p0);
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
        public Ellipse PlaceCircleWithStroke(Location location, Color color, string text1)
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
            Point p0 = LmpMap.LocationToViewportPoint(location);
            Location loc = LmpMap.ViewportPointToLocation(p0);
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
        public Rectangle PlaceSquare(Location location, Color color, string text1)
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
            Point p0 = LmpMap.LocationToViewportPoint(location);
            Location loc = LmpMap.ViewportPointToLocation(p0);
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
        public Polygon PlaceDiamond(Location location, Color color, string text1)
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
            Point p0 = LmpMap.LocationToViewportPoint(location);
            Location loc = LmpMap.ViewportPointToLocation(p0);
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
        public Polygon PlaceTriangle(Location location, Color color, string text1)
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
            Point p0 = LmpMap.LocationToViewportPoint(location);
            Location loc = LmpMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyTriangle, loc);
            ToolTipService.SetShowDuration(MyTriangle, 300000);
            return MyTriangle;
        }
        /// <summary>
        /// Adds the object.
        /// </summary>
        /// <param name="myshape">The myshape.</param>
        /// <param name="description">The description.</param>
        public void AddObject(Shape myshape)
        {
            LMPMapLayer.Children.Add(myshape);
        }
        #endregion
        #region Events

        /// <summary>
        /// Handles the Checked event of the ConstrainsCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ConstrainsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ConstrainsLayer.Visibility == System.Windows.Visibility.Hidden)
                PolyConstrainsLayer.Visibility = ConstrainsLayer.Visibility = System.Windows.Visibility.Visible;
            else
                PolyConstrainsLayer.Visibility = ConstrainsLayer.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Handles the Checked event of the CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (LMPMapLayer.Visibility == System.Windows.Visibility.Hidden)
                LMPMapLayer.Visibility = System.Windows.Visibility.Visible;
            else
                LMPMapLayer.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Handles the Click event of the ZoomOutbutton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ZoomOutbutton_Click(object sender, RoutedEventArgs e)
        {
            LmpMap.ZoomLevel = LmpMap.ZoomLevel + 0.1;
        }

        /// <summary>
        /// Handles the Click event of the ZoomInbutton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ZoomInbutton_Click(object sender, RoutedEventArgs e)
        {
            LmpMap.ZoomLevel = LmpMap.ZoomLevel - 0.1;
        }

        /// <summary>
        /// Handles the Click event of the buttonRoad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void buttonRoad_Click(object sender, RoutedEventArgs e)
        {
            LmpMap.Mode = new RoadMode();
        }

        /// <summary>
        /// Handles the Click event of the buttonAerial control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void buttonAerial_Click(object sender, RoutedEventArgs e)
        {
            LmpMap.Mode = new AerialMode();
        }

        /// <summary>
        /// Handles the Click event of the buttonAerialWithLabels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void buttonAerialWithLabels_Click(object sender, RoutedEventArgs e)
        {
            AerialMode mymode = new AerialMode();
            mymode.Labels = true;
            LmpMap.Mode = mymode;
        }

        /// <summary>
        /// Handles the Click event of the NodeAnalyzer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void NodeAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            if (SPPLocationMarginal.SelectedCells != null && SPPLocationMarginal.SelectedCells.Count >= 1)
            {
                string column = SPPLocationMarginal.SelectedCells[0].Column.Header.ToString();
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowNodeAnalyzer(sourceSinkList);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the LMPGraph control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        public void LMPGraph_Click(object sender, RoutedEventArgs e)
        {
            if (SPPLocationMarginal.SelectedCells != null && SPPLocationMarginal.SelectedCells.Count > 0)
            {
                MainWindowViewModel datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null)
                {
                    datacontext.ShowLMPGraphs(sourceSinkList);
                }
            }
        }

        /// <summary>
        /// Handles the SelectedCellsChanged event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectedCellsChangedEventArgs"/> instance containing the event data.</param>
        public void DataGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            List<int> rowIndex = new List<int>();
            MainWindowViewModel dataContext = DataContext as MainWindowViewModel;
            sourceSinkList = new List<Tuple<string, string>>();
            List<LmpData> selectedItems = new List<LmpData>();
            foreach (var cell in SPPLocationMarginal.SelectedCells)
            {
                DataGridCell cellItem = Vayu.CommonControls.DataGridInfo.GetCell(cell);
                int index = Vayu.CommonControls.DataGridInfo.GetRowIndex(cellItem);
                if (!rowIndex.Contains(index))
                {
                    LmpData node = (LmpData)cell.Item;
                    sourceSinkList.Add(new Tuple<string, string>(node.NodeName, null));
                    rowIndex.Add(index);
                }
            }
        }

        /// <summary>
        /// Handles the 1 event of the ComboBox_SelectionChanged control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int key = (int)market.SelectedValue;
            //if (mMarketLocation.ContainsKey(key))
            //    LmpMap.Center = mMarketLocation[key] as Location;
            //else
            //    LmpMap.Center = mMarketLocation[0] as Location;
            //LoadColorZones();
            LoadMarketColumns();
        }

        /// <summary>
        /// Handles the 1 event of the Window_Loaded control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as MainWindowViewModel;
            // viewModel.Updatemap += viewModel_Updatemap;
            tim.Interval = 3000;
            tim.Elapsed += tim_Elapsed;
            tim.Start();
        }

        /// <summary>
        /// Handles the Elapsed event of the tim control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        void tim_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_uiContext != null)
                _uiContext.Send((a) => ReDrawMap(), null);
        }

        /// <summary>
        /// Handles the Updatemap event of the viewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void viewModel_Updatemap(object sender, EventArgs e)
        {
            if (_uiContext != null)
                _uiContext.Post((a) => ReDrawMap(), null);
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var polygonToDelete = LmpMap.Children.OfType<MapPolygon>()
                    .Where(p => ((MapPolygon)p).Tag == "ZoneMap").ToList();

            foreach (var p in polygonToDelete)
            {
                LmpMap.Children.Remove(p);
            }
            MainWindowViewModel model = DataContext as MainWindowViewModel;
            foreach (var item in model.ZoneList)
            {
                if (!item.IsSelected)
                    continue;
                foreach (var zoneitem in item.RegionInfoList)
                {
                    MapPolygon zonePolygon = new MapPolygon();

                    #region ZoneColor
                    switch (zoneitem.Name)
                    {
                        case "AEP":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
                            break;
                        case "APS":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 219, 133, 108));
                            break;
                        case "AEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 77, 77));
                            break;
                        case "ATSI":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 179, 191, 128));
                            break;
                        case "BGE":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 204, 204));
                            break;
                        case "COMED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 153, 0));
                            break;
                        case "DAYTON":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 108, 247, 49));
                            break;
                        case "DEOK":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 51, 153, 255));
                            break;
                        case "DOM":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 76, 153));
                            break;
                        case "DPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 54, 219));
                            break;
                        case "DUQ":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 51, 255, 153));
                            break;
                        case "JCPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255));
                            break;
                        case "METED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 49, 252, 35));
                            break;
                        case "PECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 153, 76));
                            break;
                        case "PENELEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 103, 57, 238));
                            break;
                        case "PEPCO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 0));
                            break;
                        case "PPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 194, 126, 231));
                            break;
                        case "PSEG":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 255));
                            break;
                        case "RECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 76, 0));
                            break;
                        case "EKPC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 102));
                            break;
                        default:
                            break;
                    }
                    #endregion ZoneColor

                    zonePolygon.Tag = "ZoneMap";
                    zonePolygon.Locations = zoneitem.Locations;
                    zonePolygon.ToolTip = zoneitem.Name;
                    LmpMap.Children.Add(zonePolygon);
                }

            }

        }

        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var polygonToDelete = LmpMap.Children.OfType<MapPolygon>()
                   .Where(p => ((MapPolygon)p).Tag == "ZoneMap").ToList();

            foreach (var p in polygonToDelete)
            {
                LmpMap.Children.Remove(p);
            }
            MainWindowViewModel model = DataContext as MainWindowViewModel;
            foreach (var item in model.ZoneList)
                //{
                //    if (!item.IsSelected)
                //        item.IsSelected = true;
                foreach (var zoneitem in item.RegionInfoList)
                {
                    MapPolygon zonePolygon = new MapPolygon();

                    #region ZoneColor
                    switch (zoneitem.Name)
                    {
                        case "AEP":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
                            break;
                        case "APS":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 219, 133, 108));
                            break;
                        case "AEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 77, 77));
                            break;
                        case "ATSI":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 179, 191, 128));
                            break;
                        case "BGE":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 204, 204));
                            break;
                        case "COMED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 153, 0));
                            break;
                        case "DAYTON":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 108, 247, 49));
                            break;
                        case "DEOK":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 204));
                            break;
                        case "DOM":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 76, 153));
                            break;
                        case "DPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 54, 219));
                            break;
                        case "JCPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255));
                            break;
                        case "METED":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 49, 252, 35));
                            break;
                        case "PECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 153, 76));
                            break;
                        case "PENELEC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 103, 57, 238));
                            break;
                        case "PEPCO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 0));
                            break;
                        case "PPL":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 194, 126, 231));
                            break;
                        case "PSEG":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 255));
                            break;
                        case "RECO":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 76, 0));
                            break;
                        case "EKPC":
                            zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 102));
                            break;
                        default:
                            break;
                    }
                    #endregion ZoneColor

                    zonePolygon.Tag = "ZoneMap";
                    zonePolygon.Locations = zoneitem.Locations;
                    zonePolygon.ToolTip = zoneitem.Name;
                    LmpMap.Children.Add(zonePolygon);
                }
            //}
        }

        /// <summary>
        /// Handles the 2 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var polygonToDelete = LmpMap.Children.OfType<MapPolygon>()
                    .Where(p => ((MapPolygon)p).Tag == "ZoneMap").ToList();

            foreach (var p in polygonToDelete)
            {
                LmpMap.Children.Remove(p);
            }
            MainWindowViewModel model = DataContext as MainWindowViewModel;
            foreach (var item in model.ZoneList)
            {
                item.IsSelected = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the Outages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Outages_Click(object sender, RoutedEventArgs e)
        {
            //Vayu.ConstraintOutageMapping.ViewModels.MainWindowViewModel dx = DataContext as Vayu.ConstraintOutageMapping.ViewModels.MainWindowViewModel;
            //Vayu.ConstraintOutageMapping.ViewModels.MainWindowViewModel dx = new Vayu.ConstraintOutageMapping.ViewModels.MainWindowViewModel(new Vayu.ConstraintOutageMapping.Model.DataService());
            DateTime mStartDate = DateTime.Today;
            DateTime mEndDate = DateTime.Today.AddDays(1);
            viewModel = this.DataContext as MainWindowViewModel;
            string Market = "";
            if (viewModel.SelectedMarket == MarketsEnum.ERCOT)
            {
                Market = "ERCOT";
            }
            if (LatestConstraintGrid.SelectedItems.Count > 0)
            {
                for (int i = 0; i < LatestConstraintGrid.SelectedItems.Count; i++)
                {
                    LatestConstraint selectedFile = (LatestConstraint)LatestConstraintGrid.SelectedItems[i];
                    //Vayu.ConstraintOutageMapping.ViewModels.MainWindowViewModel.OpenConstraintOutageMappingScreen(selectedFile.ConstraintText, selectedFile.ContigencyText, mStartDate, mEndDate, Market);
                }
            }
        }
        /// <summary>
        /// Handles the TextChanged event of the txtRefreshMap control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtRefreshMap_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel = this.DataContext as MainWindowViewModel;
            txtRefreshMap.Clear();
            LMPMapLayer.Children.Clear();
            RefreshMapNew();
            viewModel.RefreshConstraint_RaisePropertyChanged();
        }

        public void NodeAnalyzer_click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
    /// <summary>
    /// main
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class StringColorConverter : IValueConverter
    {

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = ("255,255,0").ToString();

            if (string.IsNullOrEmpty(strValue))
                return null;

            string[] splits = strValue.Split(',', ' ');
            byte b1, b2, b3;
            byte.TryParse(splits[0], out b1);
            byte.TryParse(splits[1], out b2);
            byte.TryParse(splits[2], out b3);

            SolidColorBrush br = new SolidColorBrush(Color.FromRgb(b1, b2, b3));
            return br;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ValueToShapeColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Brushes.Gold;
            }
            else
            {
                PointMapPath tempObj = value as PointMapPath;
                if (tempObj == null)
                {
                    return Brushes.Pink;
                }
                String[] vlArray = tempObj.Name.ToString().Split(' ');
                string[] vlarray = vlArray[vlArray.Length - 1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                double lmp;
                try
                {
                    if (double.TryParse(vlarray[vlarray.Length - 2].Replace("$", ""), out lmp))
                    {
                        return GetBrush(lmp);
                    }
                    else
                    {
                        return Brushes.Salmon;
                    }
                }
                catch
                {
                    return Brushes.Gold;
                }
            }

        }

        /// <summary>
        /// Gets the brush.
        /// </summary>
        /// <param name="lmp">The LMP.</param>
        /// <returns></returns>
        private Brush GetBrush(double lmp)
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
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    public class BoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked)
            {
                return !isChecked; // Invert the boolean value
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return !isExpanded;
            }
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class HideColByMarket
    {
        /// <summary>
        /// Gets the hide col.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string GetHideCol(DependencyObject obj)
        {
            return (string)obj.GetValue(HideColProperty);
        }

        /// <summary>
        /// Sets the hide col.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetHideCol(DependencyObject obj, string value)
        {
            obj.SetValue(HideColProperty, value);
        }

        // Using a DependencyProperty as the backing store for HideCol.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The hide col property
        /// </summary>
        public static readonly DependencyProperty HideColProperty =
            DependencyProperty.RegisterAttached("HideCol", typeof(string), typeof(HideColByMarket),
                new PropertyMetadata(new PropertyChangedCallback(HideCol)));

        /// <summary>
        /// Hides the col.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void HideCol(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid grid = d as DataGrid;

            if (string.IsNullOrEmpty(e.NewValue as string) || grid == null)
                return;

            string market = e.NewValue as string;
            if (market.ToUpper() == "PJM" || market.ToUpper() == "MISO")
            {
                grid.Columns[3].Visibility = Visibility.Hidden;
                grid.Columns[5].Visibility = Visibility.Hidden;
                grid.Columns[6].Visibility = Visibility.Hidden;
            }
            else
            {
                grid.Columns[3].Visibility = Visibility.Visible;
                grid.Columns[5].Visibility = Visibility.Visible;
                grid.Columns[6].Visibility = Visibility.Visible;

            }
        }
    }

    public static class HidelmpColByMarket
    {
        /// <summary>
        /// Gets the hide col.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string GetHidelmpCol(DependencyObject obj)
        {
            return (string)obj.GetValue(HideColProperty);
        }

        /// <summary>
        /// Sets the hide col.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetHidelmpCol(DependencyObject obj, string value)
        {
            obj.SetValue(HideColProperty, value);
        }

        // Using a DependencyProperty as the backing store for HideCol.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The hide col property
        /// </summary>
        public static readonly DependencyProperty HideColProperty =
            DependencyProperty.RegisterAttached("HidelmpCol", typeof(string), typeof(HidelmpColByMarket),
                new PropertyMetadata(new PropertyChangedCallback(HidelmpCol)));

        /// <summary>
        /// Hides the col.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void HidelmpCol(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid grid = d as DataGrid;

            if (string.IsNullOrEmpty(e.NewValue as string) || grid == null)
                return;

            string market = e.NewValue as string;
            if (market.ToUpper() == "PJM" || market.ToUpper() == "MISO")
            {
                grid.Columns[3].Visibility = Visibility.Hidden;
                grid.Columns[5].Visibility = Visibility.Visible;

            }
            else
            {
                grid.Columns[3].Visibility = Visibility.Visible;
                grid.Columns[5].Visibility = Visibility.Hidden;

            }
        }
    }
}
