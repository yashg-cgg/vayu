
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Vayu.CommonAccessLibrary;
using Vayu.CommonControls;
using Vayu.DBLibrary;
using Vayu.LMPStatistics.Model;
using Vayu.LMPStatistics.ViewModels;

namespace Vayu.LMPStatistics.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        private string _user = Environment.UserName;

        Vayu.LMPStatistics.ViewModels.MainWindowViewModel viewModel;
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection VayuConnection;
        /// <summary>
        /// The m select name date range command
        /// </summary>
        private SqlCommand mSelectNameDateRangeCommand;
        /// <summary>
        /// The m select date date range command
        /// </summary>
        private SqlCommand mSelectDateDateRangeCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            loadDBCommands();
        }
        public void SetPosition(int height, int width, int top, int left)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }
        //#region Private Methods

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectNameDateRangeCommand = new SqlCommand();
            mSelectNameDateRangeCommand.CommandText = "select distinct datename from daterange where trader = @trader order by datename";
            mSelectNameDateRangeCommand.Parameters.AddWithValue("@trader", "trader");
            mSelectNameDateRangeCommand.Connection = VayuConnection;
            //
            mSelectDateDateRangeCommand = new SqlCommand();
            mSelectDateDateRangeCommand.CommandText = "select marketdate from daterange where trader = @trader and datename = @datename order by marketdate";
            mSelectDateDateRangeCommand.Parameters.AddWithValue("@trader", "trader");
            mSelectDateDateRangeCommand.Parameters.AddWithValue("@datename", "datename");
            mSelectDateDateRangeCommand.Connection = VayuConnection;
        }
        /// <summary>
        /// Handles the Click event of the WeekDaysButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WeekDaysButton_Click(object sender, RoutedEventArgs e)
        {
            MondayCheckBox.IsChecked = true;
            TuesdayCheckBox.IsChecked = true;
            WednesdayCheckBox.IsChecked = true;
            ThursdayCheckBox.IsChecked = true;
            FridayCheckBox.IsChecked = true;
            SaturdayCheckBox.IsChecked = false;
            SundayCheckBox.IsChecked = false;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadColorZones();
            RefreshMap();
        }
        private void AddSourceSink_Click(object sender, RoutedEventArgs e)
        {
            LoadColorZones();
            RefreshMap();
        }
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as Vayu.LMPStatistics.ViewModels.MainWindowViewModel;
            viewModel.Paste();
            LoadColorZones();
            RefreshMap();
        }

        private void RemovePath_Click(object sender, RoutedEventArgs e)
        {
            myMap.Children.Clear();
            PathMapLayer.Children.Clear();
        }
        private void RemoveAllPath_Click(object sender, RoutedEventArgs e)
        {
            myMap.Children.Clear();
            PathMapLayer.Children.Clear();
        }

        public void AddObject(Shape myshape)
        {
            PathMapLayer.Children.Add(myshape);
        }

        private void RefreshMap()
        {
            PathMapLayer.Children.Clear();
            viewModel = this.DataContext as Vayu.LMPStatistics.ViewModels.MainWindowViewModel;
            LMPStatistics.Model.DataService ds = new Model.DataService();
            if (viewModel.SourceSinkList == null || viewModel.SourceSinkList.Count == 0)
            {
                viewModel.AddSourceSink();
            }

            try
            {
                foreach (SourceSinkData sourceSink in viewModel.SourceSinkList)
                {
                    LocationCollection locationCollection = new LocationCollection();
                    Dictionary<int, string> NodeDict = new Dictionary<int, string>();
                    PricingNode source = sourceSink.Source;
                    PricingNode sink = sourceSink.Sink;
                    int sourceNodeKey = source.NodeKey;
                    int sinkNodeKey = sink.NodeKey;
                    NodeDict.Add(sourceNodeKey, source.NodeName);
                    NodeDict.Add(sinkNodeKey, sink.NodeName);
                    List<int> nodeList = new List<int>();
                    nodeList.Add(sourceNodeKey);
                    nodeList.Add(sinkNodeKey);
                    ds.loadDBCommands();
                    ObservableCollection<NodeCoordinate> ncCol = ds.GetNodeCoordinate(nodeList);
                    string NodeType = null;
                    Shape myobj;
                    foreach (NodeCoordinate item in ncCol)
                    {
                        int kv = 0;
                        NodeType = item.TypeName;
                        if (item.NodeName.Contains("765"))
                            kv = 765;
                        if (item.NodeName.Contains("500"))
                            kv = 500;
                        if (item.NodeName.Contains("345"))
                            kv = 345;
                        if (item.NodeName.Contains("230"))
                            kv = 230;
                        if (item.NodeName.Contains("161"))
                            kv = 161;
                        if (item.NodeName.Contains("138"))
                            kv = 138;
                        if (item.NodeName.Contains("115"))
                            kv = 115;
                        if (item.NodeName.Contains("69"))
                            kv = 69;
                        Location location = item.MapLocation;
                        string description = item.NodeName + '\n' + NodeType + '\n' + kv.ToString() + "KV";
                        locationCollection.Add(location);
                        MapPolyline polyline = new MapPolyline();
                        polyline.Stroke = Brushes.Red;
                        polyline.StrokeThickness = 2;
                        polyline.Locations = locationCollection;
                        PathMapLayer.Children.Add(polyline);
                        switch (NodeType)
                        {
                            case "ZONE":
                                myobj = PlaceStar(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "HUB":
                                myobj = PlaceTriangle(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "AGGREGATE":
                                myobj = PlaceDiamond(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "INTERFACE":
                                myobj = PlaceSquare(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "GENERATOR":
                                myobj = PlaceCircleWithStroke(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "LOAD":
                                myobj = PlaceCircleWithStroke(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "BUS":
                                myobj = PlaceCircleWithStroke(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "EXT":
                                myobj = PlaceCircleWithStroke(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            case "EHV":
                                //  myobj = PlaceCircleWithStroke(location, GetColor(0), description);
                                myobj = PlaceSquare(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                            default:
                                myobj = PlaceCircleWithStroke(location, GetColor(kv), description);
                                AddObject(myobj);
                                break;
                        }
                    }

                    viewModel.NodePath = locationCollection;
                    if (myMap.Children.Count == 0)
                    {
                        myMap.Children.Add(zoneLayer);
                        myMap.Children.Add(PathMapLayer);
                        myMap.Children.Add(ThirdLayer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }
        public Color GetColor(double kv)
        {
            SolidColorBrush solidColorBrush = GetBrush(kv);
            return solidColorBrush.Color;
        }
        ///<summary>
        ///Gets the brush.
        ///</summary>
        ///<param name = "lmp" > The LMP.</param>
        ///<returns></returns>
        public SolidColorBrush GetBrush(double kv)
        {
            BrushConverter brush = new BrushConverter();
            if (kv == 765)
                return Brushes.Red;
            else if (kv == 500)
                return Brushes.Brown;
            else if (kv == 345)
                return Brushes.LightBlue;
            else if (kv == 230)
                return Brushes.Black;
            else if (kv == 161)
                return Brushes.YellowGreen;
            else if (kv == 138)
                return Brushes.Yellow;
            else if (kv == 115)
                return Brushes.DarkGreen;
            else if (kv == 69)
                return Brushes.LightGreen;

            else
                return Brushes.Gold;
        }

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
            Point p0 = myMap.LocationToViewportPoint(location);
            Location loc = myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyStar, loc);
            ToolTipService.SetShowDuration(MyStar, 300000);
            return MyStar;
        }
        /// <summary>
        /// Places the circle.
        /// </summary>
        /// <param name = "location" > The location.</param>
        /// <param name = "color" > The color.</param>
        /// <param name = "text1" > The text1.</param>
        /// <returns></returns>
        public Ellipse PlaceCircle(Location location, Color color, string text1)
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = new SolidColorBrush(color);
            double diameter = 10;
            double radius = diameter / 2;
            myEllipse.Width = diameter;
            myEllipse.Height = diameter;
            myEllipse.Margin = new Thickness(-radius / Math.Sqrt(2), -radius / Math.Sqrt(2), 0, 0);
            myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = myMap.LocationToViewportPoint(location);
            Location loc = myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            return myEllipse;
        }
        /// <summary>
        /// Places the circle with stroke.
        /// </summary>
        /// <param name = "location" > The location.</param>
        /// <param name = "color" > The color.</param>
        /// <param name = "text1" > The text1.</param>
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
            myEllipse.Margin = new Thickness(-radius / Math.Sqrt(2), -radius / Math.Sqrt(2), 0, 0);
            myEllipse.Margin = new Thickness(-radius, -radius, 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = text1;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;
            Point p0 = myMap.LocationToViewportPoint(location);
            Location loc = myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            return myEllipse;
        }
        /// <summary>
        /// Places the square.
        /// </summary>
        /// <param name = "location" > The location.</param>
        /// <param name = "color" > The color.</param>
        /// <param name = "text1" > The text1.</param>
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
            Point p0 = myMap.LocationToViewportPoint(location);
            Location loc = myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyRectangle, loc);
            ToolTipService.SetShowDuration(MyRectangle, 300000);
            return MyRectangle;
        }
        /// <summary>
        /// Places the diamond.
        /// </summary>
        /// <param name = "location" > The location.</param>
        /// <param name = "color" > The color.</param>
        /// <param name = "text1" > The text1.</param>
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
            Point p0 = myMap.LocationToViewportPoint(location);
            Location loc = myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyPolygon, loc);
            ToolTipService.SetShowDuration(MyPolygon, 300000);
            return MyPolygon;
        }
        /// <summary>
        /// Places the triangle.
        /// </summary>
        /// <param name = "location" > The location.</param>
        /// <param name = "color" > The color.</param>
        /// <param name = "text1" > The text1.</param>
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
            Point p0 = myMap.LocationToViewportPoint(location);
            Location loc = myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(MyTriangle, loc);
            ToolTipService.SetShowDuration(MyTriangle, 300000);
            return MyTriangle;
        }

        private void LoadColorZones()
        {
            PathMapLayer.Children.Clear();
            //Vayu.LMPStatistics.ViewModels.MainWindowViewModel model = new Vayu.LMPStatistics.ViewModels.MainWindowViewModel();
            int key = 0;
            string market = marketComboBox.SelectedValue.ToString();
            if (market == "PJM")
                key = 1;
            List<ZoneInfo> zlist = ZoneModel.GetZoneList(key);
            // model.ListZone = zlist;
            //  Vayu.MarketViewNameSpace.MainWindow mW = new MainWindow();
            foreach (var item in zlist)
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
        /// <summary>
        /// Handles the Click event of the WeekEndsButton control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WeekEndsButton_Click(object sender, RoutedEventArgs e)
        {
            MondayCheckBox.IsChecked = false;
            TuesdayCheckBox.IsChecked = false;
            WednesdayCheckBox.IsChecked = false;
            ThursdayCheckBox.IsChecked = false;
            FridayCheckBox.IsChecked = false;
            SaturdayCheckBox.IsChecked = true;
            SundayCheckBox.IsChecked = true;
        }
        /// <summary>
        /// Handles the Click event of the AllDaysButton control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AllDaysButton_Click(object sender, RoutedEventArgs e)
        {
            MondayCheckBox.IsChecked = true;
            TuesdayCheckBox.IsChecked = true;
            WednesdayCheckBox.IsChecked = true;
            ThursdayCheckBox.IsChecked = true;
            FridayCheckBox.IsChecked = true;
            SaturdayCheckBox.IsChecked = true;
            SundayCheckBox.IsChecked = true;
        }
        /// <summary>
        /// Handles the CopyingRowClipboardContent event of the DataGrid control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.DataGridRowClipboardEventArgs"/> instance containing the event data.</param>
        private void DataGrid_CopyingRowClipboardContent(object sender, System.Windows.Controls.DataGridRowClipboardEventArgs e)
        {
            int i = 3;
        }
        /// <summary>
        /// Handles the Click event of the NoneDaysButton control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NoneDaysButton_Click(object sender, RoutedEventArgs e)
        {
            MondayCheckBox.IsChecked = false;
            TuesdayCheckBox.IsChecked = false;
            WednesdayCheckBox.IsChecked = false;
            ThursdayCheckBox.IsChecked = false;
            FridayCheckBox.IsChecked = false;
            SaturdayCheckBox.IsChecked = false;
            SundayCheckBox.IsChecked = false;
        }
        /// <summary>
        /// Handles the GotFocus event of the sourceComboBox control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void sourceComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }
        /// <summary>
        /// Handles the GotFocus event of the sinkComboBox control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void sinkComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;
        }
        /// <summary>
        /// Handles the MouseDoubleClick event of the ListView control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListViewItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null)
                return;
            HourlyPivotData selecteditem = (HourlyPivotData)DayComparisonListView.ItemContainerGenerator.ItemFromContainer(dep);
            foreach (var item in DayComparisonListView.Items)
            {
                HourlyPivotData tempitem = (HourlyPivotData)item;
                if (selecteditem.RowName.Equals(tempitem.RowName) && selecteditem.RowType.Equals(tempitem.RowType))
                {
                    DayComparisonListView.SelectedItems.Add(item);
                }
            }
        }
        /// <summary>
        /// Handles the Click event of the WinterButton control.
        /// </summary>
        /// <param name = "sender" > The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void WinterButton_Click(object sender, RoutedEventArgs e)
        {
            DecCheckBox.IsChecked = true;
            JanCheckBox.IsChecked = true;
            FebCheckBox.IsChecked = true;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
        }
        // <summary>
        // Handles the Click event of the SpringButton control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SpringButton_Click(object sender, RoutedEventArgs e)
        {
            MarCheckBox.IsChecked = true;
            AprCheckBox.IsChecked = true;
            MayCheckBox.IsChecked = true;
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
        }
        // <summary>
        // Handles the Click event of the SummerButton control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SummerButton_Click(object sender, RoutedEventArgs e)
        {
            JunCheckBox.IsChecked = true;
            JulCheckBox.IsChecked = true;
            AugCheckBox.IsChecked = true;
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
        }
        // <summary>
        // Handles the Click event of the FallButton control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FallButton_Click(object sender, RoutedEventArgs e)
        {
            SepCheckBox.IsChecked = true;
            OctCheckBox.IsChecked = true;
            NovCheckBox.IsChecked = true;
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
        }
        // <summary>
        // Handles the Click event of the AllMonthButton control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AllMonthButton_Click(object sender, RoutedEventArgs e)
        {
            DecCheckBox.IsChecked = true;
            JanCheckBox.IsChecked = true;
            FebCheckBox.IsChecked = true;
            MarCheckBox.IsChecked = true;
            AprCheckBox.IsChecked = true;
            MayCheckBox.IsChecked = true;
            JunCheckBox.IsChecked = true;
            JulCheckBox.IsChecked = true;
            AugCheckBox.IsChecked = true;
            SepCheckBox.IsChecked = true;
            OctCheckBox.IsChecked = true;
            NovCheckBox.IsChecked = true;
        }
        // <summary>
        // Handles the Click event of the NoneMonthButton control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NoneMonthButton_Click(object sender, RoutedEventArgs e)
        {
            DecCheckBox.IsChecked = false;
            JanCheckBox.IsChecked = false;
            FebCheckBox.IsChecked = false;
            MarCheckBox.IsChecked = false;
            AprCheckBox.IsChecked = false;
            MayCheckBox.IsChecked = false;
            JunCheckBox.IsChecked = false;
            JulCheckBox.IsChecked = false;
            AugCheckBox.IsChecked = false;
            SepCheckBox.IsChecked = false;
            OctCheckBox.IsChecked = false;
            NovCheckBox.IsChecked = false;
        }
        // <summary>
        // Handles the Checked event of the CollectionRadioButton control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CollectionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (CollectionComboBox != null)
            {
                CollectionComboBox.IsEnabled = true;
            }
            CollectionComboBox.Items.Clear();
            CollectionComboBox.Text = "";
            VayuConnection.Open();
            mSelectNameDateRangeCommand.Parameters["@trader"].Value = _user;
            SqlDataReader reader = mSelectNameDateRangeCommand.ExecuteReader();
            while (reader.Read())
            {
                CollectionComboBox.Items.Add(reader.GetString(0));
            }
            VayuConnection.Close();
        }
        // <summary>
        // Handles the Click event of the menuItemAnalysis control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void menuItemAnalysis_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            string name = menu.Name;
            var selectedItem = (HourlyPivotData)DayComparisonListView.SelectedItem;
            DateTime startDate = DateTime.Parse(selectedItem.Date.ToString());
            byte hour = byte.Parse(name.TrimStart('H', 'o', 'u', 'r'));
            string market = marketComboBox.Text;
            if (!startDate.Equals(null))
            {
                Vayu.LoadAnalysis.ViewModels.MainWindowViewModel mainViewModel = new LoadAnalysis.ViewModels.MainWindowViewModel(new LoadAnalysis.Models.LoadModel());
                var window = new LoadAnalysis.Views.MainWindow();
                window.DataContext = mainViewModel;
                if (sourceSinkDataGrid.SelectedItems.Count > 0)
                {
                    var SourceSinkItem = (SourceSinkData)sourceSinkDataGrid.SelectedItems[0];
                    LoadAnalysis.Models.SourceSinkData sourceSink = new LoadAnalysis.Models.SourceSinkData();

                    sourceSink.Source = new LoadAnalysis.Models.PricingNode();
                    sourceSink.Source.NodeKey = SourceSinkItem.Source.NodeKey;
                    sourceSink.Source.NodeName = SourceSinkItem.Source.NodeName;
                    sourceSink.Source.ExternalNodeId = SourceSinkItem.Source.ExternalNodeId;
                    sourceSink.Source.NodeTypeKey = SourceSinkItem.Source.NodeTypeKey;
                    sourceSink.Source.MarketKey = SourceSinkItem.Source.MarketKey;
                    sourceSink.Source.Zone = SourceSinkItem.Source.Zone;
                    if (SourceSinkItem.Sink != null)
                    {
                        sourceSink.Sink = new LoadAnalysis.Models.PricingNode();
                        sourceSink.Sink.NodeKey = SourceSinkItem.Sink.NodeKey;
                        sourceSink.Sink.NodeName = SourceSinkItem.Sink.NodeName;
                        sourceSink.Sink.ExternalNodeId = SourceSinkItem.Sink.ExternalNodeId;
                        sourceSink.Sink.NodeTypeKey = SourceSinkItem.Sink.NodeTypeKey;
                        sourceSink.Sink.MarketKey = SourceSinkItem.Sink.MarketKey;
                        sourceSink.Sink.Zone = SourceSinkItem.Sink.Zone;
                    }

                    DateTime endDate = startDate.AddHours(23);
                    mainViewModel.SetData(startDate, endDate, hour, SourceSinkItem.Source.MarketKey, sourceSink, double.Parse(MaxDart.Content.ToString()), double.Parse(MinDart.Content.ToString()));
                    window.ShowDialog();
                }
            }
        }
        private void LMP_15mins_Prices_Click(object sender, RoutedEventArgs e)
        {

            //Vayu.LMP15Mins.ViewModels.MainWindowViewModel mainViewModel = new LMP15Mins.ViewModels.MainWindowViewModel(new LMP15Mins.Model.LoadModel());
            //var window = new LMP15Mins.Views.MainWindow();
            //window.DataContext = mainViewModel;
            //window.ShowDialog();

            MenuItem menu = sender as MenuItem;
            string name = menu.Name;
            var selectedItem = (HourlyPivotData)DayComparisonListView.SelectedItem;
            DateTime startDate = DateTime.Parse(selectedItem.Date.ToString());
            byte hour = byte.Parse(name.TrimStart('H', 'o', 'u', 'r', 's', '_'));
            string market = marketComboBox.Text;
            if (!startDate.Equals(null))
            {
                Vayu.LMP15Mins.ViewModels.MainWindowViewModel mainViewModel = new LMP15Mins.ViewModels.MainWindowViewModel(new LMP15Mins.Model.LoadModel());
                var window = new LMP15Mins.Views.MainWindow();
                window.DataContext = mainViewModel;
                if (sourceSinkDataGrid.SelectedItems.Count > 0)
                {
                    var SourceSinkItem = (SourceSinkData)sourceSinkDataGrid.SelectedItems[0];
                    LMP15Mins.Model.SourceSinkData sourceSink = new LMP15Mins.Model.SourceSinkData();

                    sourceSink.Source = new LMP15Mins.Model.PricingNode();
                    sourceSink.Source.NodeKey = SourceSinkItem.Source.NodeKey;
                    sourceSink.Source.NodeName = SourceSinkItem.Source.NodeName;
                    sourceSink.Source.ExternalNodeId = SourceSinkItem.Source.ExternalNodeId;
                    sourceSink.Source.NodeTypeKey = SourceSinkItem.Source.NodeTypeKey;
                    sourceSink.Source.MarketKey = SourceSinkItem.Source.MarketKey;
                    sourceSink.Source.Zone = SourceSinkItem.Source.Zone;
                    if (SourceSinkItem.Sink != null)
                    {
                        sourceSink.Sink = new LMP15Mins.Model.PricingNode();
                        sourceSink.Sink.NodeKey = SourceSinkItem.Sink.NodeKey;
                        sourceSink.Sink.NodeName = SourceSinkItem.Sink.NodeName;
                        sourceSink.Sink.ExternalNodeId = SourceSinkItem.Sink.ExternalNodeId;
                        sourceSink.Sink.NodeTypeKey = SourceSinkItem.Sink.NodeTypeKey;
                        sourceSink.Sink.MarketKey = SourceSinkItem.Sink.MarketKey;
                        sourceSink.Sink.Zone = SourceSinkItem.Sink.Zone;
                    }
                    startDate = startDate.AddHours(hour).AddMinutes(-45);
                    DateTime endDate = startDate.AddMinutes(45);
                    mainViewModel.SetData(startDate, endDate, SourceSinkItem.Source.MarketKey, sourceSink);
                    mainViewModel.SetData(startDate, endDate, SourceSinkItem.Source.MarketKey, sourceSink);
                    window.Show();
                }
            }
        }

        // <summary>
        // Handles the Checked event of the NodeCheckBox control.
        // </summary>
        // <param name = "sender" > The source of the event.</param>
        // <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NodeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (UptosCheckBox.IsChecked.Equals(false))
                sinkComboBox.IsEnabled = false;
        }
        ///// <summary>
        ///// Handles the Unchecked event of the NodeCheckBox control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void NodeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            sinkComboBox.IsEnabled = true;
        }
        ///// <summary>
        ///// Handles the Checked event of the UptosCheckBox control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UptosCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            sinkComboBox.IsEnabled = true;
        }
        ///// <summary>
        ///// Handles the Unchecked event of the UptosCheckBox control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UptosCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (rbPath.IsChecked == true)
            {
                sinkComboBox.IsEnabled = true;
            }
            else
            {
                sinkComboBox.IsEnabled = false;
            }
        }
        ///// <summary>
        ///// Handles the SelectionChanged event of the sourceSinkDataGrid control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void sourceSinkDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SourceSinkData sourceSink = (SourceSinkData)e.AddedItems[0];
                if (sourceSink.Sink == null)
                {
                    sourceCheckBox.IsEnabled = false;
                    sinkCheckBox.IsEnabled = false;
                }
                else
                {
                    sourceCheckBox.IsEnabled = true;
                    sinkCheckBox.IsEnabled = true;
                }
            }
        }
        /// <summary>
        /// Handles the Click event of the Export control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>> s =
                  new ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>>();
            ICollectionView view = CollectionViewSource.GetDefaultView(DayComparisonListView.ItemsSource);
            if (DayComparisonListView.SelectedItems.Count > 0)
            {
                view = CollectionViewSource.GetDefaultView(DayComparisonListView.SelectedItems);
            }
            List<HourlyPivotData> itemlist = new List<HourlyPivotData>();
            foreach (var item in view.SourceCollection)
            {
                itemlist.Add((HourlyPivotData)item);
            }
            s.dataToPrint = itemlist;
            s.GenerateReport();
        }
        /// <summary>
        /// Handles the Click event of the ExportAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ExportAll_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>> s =
                new ExportToExcelNodeSpread<HourlyPivotData, List<HourlyPivotData>>();
            ICollectionView view = CollectionViewSource.GetDefaultView(DayComparisonListView.ItemsSource);
            List<HourlyPivotData> itemlist = new List<HourlyPivotData>();
            foreach (var item in view.SourceCollection)
            {
                itemlist.Add((HourlyPivotData)item);
            }
            s.dataToPrint = itemlist;
            s.GenerateReport();
        }
        /// <summary>
        /// Handles the TextChanged event of the MapCenterVal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void MapCenterVal_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!MapCenterVal.Text.Equals(""))
                {
                    var loc = MapCenterVal.Text.Split(':');
                    Location mapCenterLoc = new Location(Convert.ToDouble(loc[0]), Convert.ToDouble(loc[1]));
                    myMap.SetView(mapCenterLoc, 6);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Handles the Click event of the ConstExp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ConstExp_Click(object sender, RoutedEventArgs e)
        {
            Vayu.ConstraintExposure.Views.MainWindow window = new ConstraintExposure.Views.MainWindow();
            var datacontext = new Vayu.ConstraintExposure.ViewModels.MainWindowViewModel(new ConstraintExposure.Model.DataService());
            var SourceSinkItem = (SourceSinkData)sourceSinkDataGrid.SelectedItems[0];
            var selectedHourlyPivotData = (HourlyPivotData)DayComparisonListView.SelectedItem;
            datacontext.SetData(SourceSinkItem.Source.NodeName, SourceSinkItem.Sink.NodeName, Convert.ToDateTime(selectedHourlyPivotData.Date));
            window.DataContext = datacontext;
            window.Show();
        }

        //#endregion

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

    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class DataGridCellForeColorConverterSimple : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
            double number;
            if (value != null)
            {
                Double.TryParse(value.ToString(), out number);
                //Double.TryParse((string)value, out number);
                if (number >= 0)
                {
                    mybrush = new SolidColorBrush(Colors.Black);
                }
                if (number < 0)
                {
                    mybrush = new SolidColorBrush(Colors.Red);
                }
            }

            return mybrush;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class DataGridCellForeColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
            try
            {
                // capture input values to coloring algorithm
                double[] testFactorsAbove = new double[] { };
                if (values[0] != DependencyProperty.UnsetValue && values[0] != null)
                {
                    if (((double[])values[0]).Length >= 10)
                    {
                        testFactorsAbove = (double[])values[0];
                    }
                }
                double[] testFactorsBelow = new double[] { };
                if (values[1] != DependencyProperty.UnsetValue && values[1] != null)
                {
                    if (((double[])values[1]).Length >= 10)
                    {
                        testFactorsBelow = (double[])values[1];
                    }
                }
                DataGridCell cell = null;
                string cellHeader = "";
                int cellHeaderTest = -1;
                if (values[2] != DependencyProperty.UnsetValue) // cell object value to get header names
                {
                    cell = (DataGridCell)values[2];
                    cellHeader = (string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                HourlyPivotData node = null;
                if (values[3] != DependencyProperty.UnsetValue) // itemsource value to get date for row
                {
                    node = (HourlyPivotData)values[3];
                }
                double textValue = 0;
                if (values[4] != DependencyProperty.UnsetValue) // cell text value to get numeric value to test
                {
                    double.TryParse((string)values[4], out textValue);
                    //Trace.WriteLine(values[3]);
                }
                if (cellHeader.Contains("Avg") || cellHeader.Contains("Tot") || cellHeaderTest != -1)
                {
                    if (textValue >= 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Black);
                    }
                    else if (testFactorsAbove.Length >= 10 && node != null && node.Date != null)
                    {
                        if (cellHeader.Contains("Tot") && textValue < testFactorsBelow[7])
                        {
                            mybrush = new SolidColorBrush(Colors.White);
                        }
                        else if (cellHeader.Contains("Tot") && textValue > testFactorsAbove[7] && textValue < testFactorsAbove[9])
                        {
                            mybrush = new SolidColorBrush(Colors.Black);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue < testFactorsBelow[2]) // m*-1
                        {
                            mybrush = new SolidColorBrush(Colors.White);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue > testFactorsBelow[2] && textValue < testFactorsBelow[4]) // between m*-1 and m*-6
                        {
                            mybrush = new SolidColorBrush(Colors.Black);
                        }
                        else if (textValue < 0)
                        {
                            mybrush = new SolidColorBrush(Colors.Red);
                        }
                    }
                    else if (textValue < 0)
                    {
                        mybrush = new SolidColorBrush(Colors.Red);
                    }
                }
                return mybrush;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception in DataGridCellForeColorConverter: " + ex.Message);
                return mybrush;
            }
        }
        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class DataGridCellBackColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.White);
            try
            {
                int sumTotFactorShift = 0;
                // capture input values to coloring algorithm
                // values[0] is factors above numbers
                double[] testFactorsAbove = new double[] { };
                if (values[0] != DependencyProperty.UnsetValue && values[0] != null)
                {
                    if (((double[])values[0]).Length >= 10)
                    {
                        testFactorsAbove = (double[])values[0];
                    }
                }
                // values[1] is factors below numbers
                double[] testFactorsBelow = new double[] { };
                if (values[1] != DependencyProperty.UnsetValue && values[1] != null)
                {
                    if (((double[])values[1]).Length >= 10)
                    {
                        testFactorsBelow = (double[])values[1];
                    }
                }
                DataGridCell cell = null;
                string cellHeader = "";
                Int32 cellHeaderTest = -1;
                if (values[2] != DependencyProperty.UnsetValue) // cell object value
                {
                    cell = (DataGridCell)values[2];
                    cellHeader = (string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                HourlyPivotData node = null;
                if (values[3] != DependencyProperty.UnsetValue) // itemsource value
                {
                    node = (HourlyPivotData)values[3];
                }
                double textValue = 0;
                if (values[4] != DependencyProperty.UnsetValue) // cell text value if any
                {
                    double.TryParse((string)values[4], out textValue);
                    //Trace.WriteLine(values[3]);
                }
                bool isRowWeekend = false;
                if (node != null)
                {
                    if (node.Date != null)
                    {
                        isRowWeekend = (((DateTime)node.Date).DayOfWeek == DayOfWeek.Saturday || ((DateTime)node.Date).DayOfWeek == DayOfWeek.Sunday);
                    }

                    if (isRowWeekend)
                    {
                        mybrush = new SolidColorBrush(Colors.LightGray);
                    }
                    if (node.RowName != "Spread" || node.Date == null) // cj todo: take out node.date == null check. allow spread avg in summary to show heatmap colors, but not spread total in summary.
                    {
                        return mybrush;
                    }
                }
                // 
                if (cellHeader == "")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader == "Day")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader == "Date")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader.Contains("Avg") || cellHeader.Contains("Tot") || cellHeaderTest != -1)
                {
                    if (testFactorsAbove.Length >= 10)
                    {
                        if (cellHeader.Contains("Tot"))
                        {
                            sumTotFactorShift = 5;
                        }
                        if (textValue > testFactorsAbove[sumTotFactorShift + 0]) // *15
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(13, 138, 0));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 1]) // *10
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(50, 158, 38));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 2]) // *6
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(20, 209, 0));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 3]) // *2
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(77, 233, 60));
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 4]) // *1
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(120, 233, 108));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 0]) // *-15
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(165, 0, 8));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 1]) // *-10
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(190, 46, 53));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 2]) // *-6
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(251, 0, 13));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 3]) // *-2
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(253, 65, 75));
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 4]) // *-1
                        {
                            mybrush = new SolidColorBrush(Color.FromRgb(253, 118, 125));
                        }
                        else
                        {
                            // todo: add weekend gray/white logic here
                        }
                    }
                }
                return mybrush;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception in DataGridCellBackColorConverter: " + ex.Message);
                return mybrush;
            }
        }
        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class TextBlockBackColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Color mybrush = Color.FromRgb(255, 255, 255);
            try
            {
                int sumTotFactorShift = 0;
                // capture input values to coloring algorithm
                // values[0] is factors above numbers
                double[] testFactorsAbove = new double[] { };
                if (values[0] != DependencyProperty.UnsetValue && values[0] != null)
                {
                    if (((double[])values[0]).Length >= 10)
                    {
                        testFactorsAbove = (double[])values[0];
                    }
                }
                // values[1] is factors below numbers
                double[] testFactorsBelow = new double[] { };
                if (values[1] != DependencyProperty.UnsetValue && values[1] != null)
                {
                    if (((double[])values[1]).Length >= 10)
                    {
                        testFactorsBelow = (double[])values[1];
                    }
                }
                //DataGridCell cell = null;
                string cellHeader = "";
                Int32 cellHeaderTest = -1;
                if (!values[2].Equals("")) // cell object value
                {
                    //cell = (DataGridCell)values[2];
                    cellHeader = values[2].ToString(); //(string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                HourlyPivotData node = null;
                if (values[3] != DependencyProperty.UnsetValue) // itemsource value
                {
                    node = (HourlyPivotData)values[3];
                }
                double textValue = 0;
                if (values[4] != DependencyProperty.UnsetValue) // cell text value if any
                {
                    double.TryParse((string)values[4], out textValue);
                    //Trace.WriteLine(values[3]);
                }
                bool isRowWeekend = false;
                if (node.Date != null)
                {
                    isRowWeekend = (((DateTime)node.Date).DayOfWeek == DayOfWeek.Saturday || ((DateTime)node.Date).DayOfWeek == DayOfWeek.Sunday);
                }

                if (isRowWeekend)
                {
                    mybrush = Color.FromRgb(211, 211, 211);
                }
                if (node.RowName != "Spread" || node.Date == null) // cj todo: take out node.date == null check. allow spread avg in summary to show heatmap colors, but not spread total in summary.
                {
                    return mybrush;
                }
                // 
                if (cellHeader == "")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader == "Day")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader == "Date")
                {
                    // todo: add weekend gray/white logic here
                }
                else if (cellHeader.Contains("Avg") || cellHeader.Contains("Tot") || cellHeaderTest != -1)
                {
                    if (testFactorsAbove.Length >= 10)
                    {
                        if (cellHeader.Contains("Tot"))
                        {
                            sumTotFactorShift = 5;
                        }
                        if (textValue > testFactorsAbove[sumTotFactorShift + 0]) // *15
                        {
                            mybrush = Color.FromRgb(13, 138, 0);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 1]) // *10
                        {
                            mybrush = Color.FromRgb(50, 158, 38);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 2]) // *6
                        {
                            mybrush = Color.FromRgb(20, 209, 0);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 3]) // *2
                        {
                            mybrush = Color.FromRgb(77, 233, 60);
                        }
                        else if (textValue > testFactorsAbove[sumTotFactorShift + 4]) // *1
                        {
                            mybrush = Color.FromRgb(120, 233, 108);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 0]) // *-15
                        {
                            mybrush = Color.FromRgb(165, 0, 8);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 1]) // *-10
                        {
                            mybrush = Color.FromRgb(190, 46, 53);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 2]) // *-6
                        {
                            mybrush = Color.FromRgb(251, 0, 13);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 3]) // *-2
                        {
                            mybrush = Color.FromRgb(253, 65, 75);
                        }
                        else if (textValue < testFactorsBelow[sumTotFactorShift + 4]) // *-1
                        {
                            mybrush = Color.FromRgb(253, 118, 125);
                        }
                        else
                        {
                            // todo: add weekend gray/white logic here
                        }
                    }
                }
                return mybrush;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception in DataGridCellBackColorConverter: " + ex.Message);
                return mybrush;
            }
        }
        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IMultiValueConverter" />
    public class TextBlockForeColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Color mybrush = Color.FromRgb(0, 0, 0);
            try
            {
                // capture input values to coloring algorithm
                double[] testFactorsAbove = new double[] { };
                if (values[0] != DependencyProperty.UnsetValue && values[0] != null)
                {
                    if (((double[])values[0]).Length >= 10)
                    {
                        testFactorsAbove = (double[])values[0];
                    }
                }
                double[] testFactorsBelow = new double[] { };
                if (values[1] != DependencyProperty.UnsetValue && values[1] != null)
                {
                    if (((double[])values[1]).Length >= 10)
                    {
                        testFactorsBelow = (double[])values[1];
                    }
                }
                //DataGridCell cell = null;
                string cellHeader = "";
                int cellHeaderTest = -1;
                if (values[2] != DependencyProperty.UnsetValue) // cell object value to get header names
                {
                    //cell = (DataGridCell)values[2];
                    cellHeader = values[2].ToString();//(string)cell.Column.Header;
                    Int32.TryParse(cellHeader, out cellHeaderTest);
                }
                HourlyPivotData node = null;
                if (values[3] != DependencyProperty.UnsetValue) // itemsource value to get date for row
                {
                    node = (HourlyPivotData)values[3];
                }
                double textValue = 0;
                if (values[4] != DependencyProperty.UnsetValue) // cell text value to get numeric value to test
                {
                    double.TryParse((string)values[4], out textValue);
                    //Trace.WriteLine(values[3]);
                }
                if (cellHeader.Contains("Avg") || cellHeader.Contains("Tot") || cellHeaderTest != -1)
                {
                    if (textValue >= 0)
                    {
                        mybrush = Color.FromRgb(0, 0, 0);
                    }
                    else if (testFactorsAbove.Length >= 10 && node != null && node.Date != null)
                    {
                        if (cellHeader.Contains("Tot") && textValue < testFactorsBelow[7])
                        {
                            mybrush = Color.FromRgb(255, 255, 255);
                        }
                        else if (cellHeader.Contains("Tot") && textValue > testFactorsAbove[7] && textValue < testFactorsAbove[9])
                        {
                            mybrush = Color.FromRgb(0, 0, 0);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue < testFactorsBelow[2]) // m*-1
                        {
                            mybrush = Color.FromRgb(255, 255, 255);
                        }
                        else if (!cellHeader.Contains("Tot") && textValue > testFactorsBelow[2] && textValue < testFactorsBelow[4]) // between m*-1 and m*-6
                        {
                            mybrush = Color.FromRgb(0, 0, 0);
                        }
                        else if (textValue < 0)
                        {
                            mybrush = Color.FromRgb(255, 0, 0);
                        }
                    }
                    else if (textValue < 0)
                    {
                        mybrush = Color.FromRgb(255, 0, 0);
                    }
                }
                return mybrush;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception in DataGridCellForeColorConverter: " + ex.Message);
                return mybrush;
            }
        }
        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class LocationsViewConverter : IValueConverter
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
            var locs = value as System.Collections.Generic.IEnumerable<Location>;
            if (locs != null && locs.Any())
            {
                return new LocationRect(locs.Max(l => l.Latitude), locs.Min(l => l.Longitude), // NWSE
                                        locs.Min(l => l.Latitude), locs.Max(l => l.Longitude));
            }
            else
            {
                return null;
            }
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
    public class DataGridCellForeColorConverterCorr : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush mybrush = new SolidColorBrush(Colors.Black);
            double number;
            if (value != null)
            {
                Double.TryParse(value.ToString(), out number);
                //Double.TryParse((string)value, out number);
                if (number >= 0 && number <= 1)
                {
                    mybrush = new SolidColorBrush(Colors.LightGreen);
                }
                else if (number > -0.5 && number < 0)
                {
                    mybrush = new SolidColorBrush(Colors.Orange);
                }
                else if (number >= -1 && number <= -0.5)
                {
                    mybrush = new SolidColorBrush(Colors.Red);
                }

            }
            return mybrush;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
