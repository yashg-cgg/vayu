using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Vayu.LatestConstraintsInformationLibrary;
using Vayu.MarketView.Model;
using Vayu.MarketView.ViewModels;
using GeoLocation = Vayu.MarketView.Model.MapLocation;
using MBrush = Mapsui.Styles.Brush;
using MColor = Mapsui.Styles.Color;
using MPen = Mapsui.Styles.Pen;
using MPoint = Mapsui.MPoint;
using WpfColor = System.Windows.Media.Color;

namespace Vayu.MarketView.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml (Mapsui + OpenStreetMap).
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        // ---- Shared fields ----
        private List<Tuple<string, string>> sourceSinkList = new List<Tuple<string, string>>();
        private MainWindowViewModel viewModel;

        // ---- New Mapsui layers ----
        private MemoryLayer nodesLayer;
        private MemoryLayer constraintLinesLayer;
        private MemoryLayer constraintPointsLayer;
        private MemoryLayer constraintFillLayer;
        private MemoryLayer navigateLmpLayer;
        private MemoryLayer navigateConstraintLayer;
        private MemoryLayer selectedPinLayer;

        // ---- Pin Image & Clear Selected Pin ----
        private static int _selectedPinBitmapId = -1;
        private static double _selectedPinHeightPx;
        private void ClearSelectedPin()
        {
            if (selectedPinLayer == null) return;
            selectedPinLayer.Features = new List<IFeature>();
            selectedPinLayer.DataHasChanged();
            if (LmpMap != null)
                LmpMap.RefreshGraphics();
        }

        // ---- Hover-tooltip infrastructure (new, built in code-behind) ----
        private Popup _tooltipPopup;
        private TextBlock _tooltipText;
        private IFeature _lastHoveredFeature;

        // Market centre lookup now uses Vayu.MarketView.Model.MapLocation
        // (typedef'd as GeoLocation) instead of Bing's Location type.
        private readonly Hashtable mMarketLocation = new Hashtable{
            {0, new GeoLocation(38.462, -81.843)},
            {1, new GeoLocation(38.462, -81.843)},
            {2, new GeoLocation(42.122, -91.775)},
            {3, new GeoLocation(42.71827, -73.93397)},
            {7, new GeoLocation(36.778261, -119.417932)},
            {12,new GeoLocation(34.75324, -92.44964)},
            {9, new GeoLocation(31.19, -98.05)}
        };

        // ERCOT default view
        private const double DefaultErcotLatitude = 31.19;
        private const double DefaultErcotLongitude = -98.05;
        private const double DefaultErcotResolution = 3000; // ERCOT-wide
        private bool _defaultViewApplied;

        // Node navigation medium zoom, neighbors visible
        private const double NodeNavigationResolution = 650;

        // Constraint navigation medium zoom, wider than node
        // Used only as a fallback when the constraint bbox cannot be computed.
        private const double ConstraintNavigationResolution = 3000;

        // Never zoom out farther than ERCOT default when navigating
        private const double MinNavigateResolution = DefaultErcotResolution;

        // Node-symbol sizing (kept as constants so QA can tweak easily).
        private const double NodeSymbolScale = 0.25;
        private const double ZoneSymbolScale = 0.50;
        private const double PinkSymbolScale = 0.40;
        private const double NavigateOverlayScale = 0.40;
        private const int HitTestMarginPx = 8; // hover/click tolerance in device px


        // [OLD - Bing-specific fields, replaced by Mapsui layers above]
        // Note: original mMarketLocation initializer was syntactically broken
        // (ended with ';' instead of '}'). Fixed inline in this reference copy.
        /*
        private MapLayer mZoneLayer = new MapLayer();

        private Hashtable mMarketLocation = new Hashtable(){
            {0, new Location(38.462, -81.843)},
            {1, new Location(38.462, -81.843)},
            {2, new Location(42.122, -91.775)},
            {3, new Location(42.71827,  -73.93397 )},
            {7, new Location(36.778261,  -119.417932 )},
            {12, new Location(34.75324 ,-92.44964 )},
            {9, new Location(31.19, -98.05)}
        };

        System.Timers.Timer tim = new System.Timers.Timer();
        private SynchronizationContext _uiContext = SynchronizationContext.Current;
        double factor = 0.00001;
        */

        #endregion

        // Constructor now also initializes the Mapsui map and the hover tooltip.
        public MainWindow()
        {
            InitializeComponent();

            buttonRoad.Click += buttonRoad_Click;
            buttonAerial.Click += buttonAerial_Click;
            buttonAerialWithLabels.Click += buttonAerialWithLabels_Click;
            ZoomInbutton.Click += ZoomInbutton_Click;
            ZoomOutbutton.Click += ZoomOutbutton_Click;
            LMPCheckBox.Click += CheckBox_Checked;
            LmpMap.MouseLeftButtonDown += LmpMap_MouseLeftButtonDown;
            ConstraintCheckBox.Click += ConstrainsCheckBox_Checked;

            InitializeMap();
            InitializeHoverTooltip();

            Unloaded += MainWindow_Unloaded;
        }

        #region Mapsui setup
        private void SetDefaultErcotView()
        {
            if (LmpMap?.Map == null) return;
            var mercator = SphericalMercator
                .FromLonLat(DefaultErcotLongitude, DefaultErcotLatitude)
                .ToMPoint();
            LmpMap.Map.Navigator.CenterOnAndZoomTo(mercator, DefaultErcotResolution);
        }
        private void InitializeMap()
        {
            var map = new Mapsui.Map { CRS = "EPSG:3857" };

            // 1) Base OSM tile layer
            map.Layers.Add(OpenStreetMap.CreateTileLayer());

            // 2) Feature layers (order matters – top of list draws on top)
            nodesLayer = new MemoryLayer { Name = "Nodes", Style = null, IsMapInfoLayer = true };
            constraintFillLayer = new MemoryLayer { Name = "ConstraintFill", Style = null };
            constraintLinesLayer = new MemoryLayer { Name = "ConstraintLines", Style = null };
            constraintPointsLayer = new MemoryLayer { Name = "ConstraintPoints", Style = null };
            navigateLmpLayer = new MemoryLayer { Name = "NavigateLmp", Style = null };
            navigateConstraintLayer = new MemoryLayer { Name = "NavigateConstraint", Style = null };
            selectedPinLayer = new MemoryLayer { Name = "SelectedPin", Style = null };

            map.Layers.Add(nodesLayer);
            map.Layers.Add(constraintPointsLayer);
            map.Layers.Add(navigateLmpLayer);
            map.Layers.Add(navigateConstraintLayer);
            map.Layers.Add(selectedPinLayer);
            map.Layers.Add(constraintLinesLayer);
            map.Layers.Add(constraintFillLayer);

            LmpMap.Map = map;

            EnsurePinBitmapLoaded();
        }

        #endregion

        #region Hover tooltip  (Mapsui GetMapInfo + WPF Popup)

        private void InitializeHoverTooltip()
        {
            // Build the popup entirely in code-behind so no XAML change is needed.
            _tooltipText = new TextBlock
            {
                Margin = new Thickness(8, 5, 8, 5),
                TextWrapping = TextWrapping.NoWrap,
                Foreground = Brushes.Black
            };

            var border = new Border
            {
                Background = new SolidColorBrush(WpfColor.FromArgb(235, 255, 255, 210)),
                BorderBrush = new SolidColorBrush(WpfColor.FromRgb(120, 120, 120)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                SnapsToDevicePixels = true,
                Child = _tooltipText
            };

            _tooltipPopup = new Popup
            {
                PlacementTarget = LmpMap,
                Placement = PlacementMode.Relative,
                AllowsTransparency = true,
                StaysOpen = true,
                IsHitTestVisible = false, // never steal mouse from the map
                Child = border
            };

            LmpMap.MouseMove += LmpMap_MouseMove;
            LmpMap.MouseLeave += LmpMap_MouseLeave;
        }

        private void LmpMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (nodesLayer == null || LmpMap.Map == null) return;

            var wpfPos = e.GetPosition(LmpMap);
            MapInfo info = null;
            try
            {
                info = LmpMap.GetMapInfo(new MPoint(wpfPos.X, wpfPos.Y), HitTestMarginPx);
            }
            catch
            {
                // Guarantee: hover never crashes the UI thread.
                info = null;
            }

            var feature = info?.Feature;
            bool isNodeFeature = feature != null && info.Layer == nodesLayer;

            if (!isNodeFeature)
            {
                if (_lastHoveredFeature != null)
                {
                    _lastHoveredFeature = null;
                    _tooltipPopup.IsOpen = false;
                }
                return;
            }

            // Only mutate UI when the hovered feature identity changes.
            if (!ReferenceEquals(feature, _lastHoveredFeature))
            {
                _lastHoveredFeature = feature;
                UpdateTooltipContent(feature);
            }

            // Reposition popup near cursor.
            _tooltipPopup.HorizontalOffset = wpfPos.X + 14;
            _tooltipPopup.VerticalOffset = wpfPos.Y + 14;
            if (!_tooltipPopup.IsOpen)
                _tooltipPopup.IsOpen = true;
        }

        private void LmpMap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LmpMap == null || LmpMap.Map == null)
                return;

            var position = e.GetPosition(LmpMap);

            MapInfo mapInfo = null;

            try
            {
                mapInfo = LmpMap.GetMapInfo(
                    new MPoint(position.X, position.Y),
                    HitTestMarginPx);
            }
            catch
            {
                mapInfo = null;
            }

            // No feature exists at the clicked map location.
            if (mapInfo?.Feature == null)
            {
                ClearSelectedPin();
            }
        }

        private void UpdateTooltipContent(IFeature feature)
        {
            string nodeName = SafeString(feature["NodeName"]);
            double lmp = SafeDouble(feature["Lmp"]);
            string zone = SafeString(feature["Zone"]);

            string lmpDisplay = lmp.ToString("C2", CultureInfo.CurrentCulture);

            _tooltipText.Inlines.Clear();
            _tooltipText.Inlines.Add(new Run(nodeName) { FontWeight = FontWeights.Bold });
            _tooltipText.Inlines.Add(new LineBreak());
            _tooltipText.Inlines.Add(new Run("LMP: " + lmpDisplay));
            _tooltipText.Inlines.Add(new LineBreak());
            _tooltipText.Inlines.Add(new Run("Zone: " + (string.IsNullOrEmpty(zone) ? "-" : zone)));
        }

        private static void EnsurePinBitmapLoaded()
        {
            if (_selectedPinBitmapId >= 0) return;
            var uri = new Uri("pack://application:,,,/Vayu.MarketView;component/Resources/pin_red.png",
                              UriKind.Absolute);
            var sri = System.Windows.Application.GetResourceStream(uri);
            if (sri == null) return;

            // Read height for accurate offset; then rewind for registry.
            using (var ms = new System.IO.MemoryStream())
            {
                sri.Stream.CopyTo(ms);
                ms.Position = 0;
                var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(
                    ms, System.Windows.Media.Imaging.BitmapCreateOptions.None,
                    System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
                _selectedPinHeightPx = decoder.Frames[0].PixelHeight;
                ms.Position = 0;
                _selectedPinBitmapId = Mapsui.Styles.BitmapRegistry.Instance.Register(new System.IO.MemoryStream(ms.ToArray()));
            }
        }

        private static string SafeString(object o) => o == null ? "" : o.ToString();

        private static double SafeDouble(object o)
        {
            if (o == null) return 0d;
            if (o is double d) return d;
            double parsed;
            return double.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out parsed)
                ? parsed : 0d;
        }

        private void LmpMap_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _lastHoveredFeature = null;
            if (_tooltipPopup != null) _tooltipPopup.IsOpen = false;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (LmpMap != null)
            {
                LmpMap.MouseMove -= LmpMap_MouseMove;
                LmpMap.MouseLeave -= LmpMap_MouseLeave;
                LmpMap.MouseLeftButtonDown -= LmpMap_MouseLeftButtonDown;
            }
            if (_tooltipPopup != null) _tooltipPopup.IsOpen = false;
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                viewModel.LmpNodeSelected -= ViewModel_LmpNodeSelected;
            }
        }

        #endregion

        #region Data → feature refresh

        // New: rebuild the nodes MemoryLayer as PointFeatures with attached tooltip data. Uses Mapsui symbol styles instead of WPF Shapes.
        public void RefreshMapNew()
        {
            viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null || viewModel.LatestLMPList == null || nodesLayer == null)
                return;

            // Any hover tracking is invalidated by a full rebuild.
            _lastHoveredFeature = null;
            if (_tooltipPopup != null) _tooltipPopup.IsOpen = false;

            var features = new List<IFeature>();
            foreach (var item in viewModel.LatestLMPList)
            {
                var geo = viewModel.NodeLocationHashCache != null
                    ? viewModel.NodeLocationHashCache[item.NodeKey] as NodeGeoDetail
                    : null;
                if (geo == null) continue;

                string zone1 = "";
                if (viewModel.mNodeZoneList != null && viewModel.mNodeZoneList.ContainsKey(item.NodeName))
                    zone1 = viewModel.mNodeZoneList[item.NodeName];

                string description = string.Format("{0}\n{1}\n({2})",
                    item.NodeName, Math.Round(item.LMP, 2).ToString("C2"), zone1);

                var merc = SphericalMercator.FromLonLat(geo.Longitude, geo.Latitude).ToMPoint();
                var pf = new PointFeature(merc);
                pf["Name"] = description;
                pf["NodeName"] = item.NodeName;
                pf["NodeKey"] = item.NodeKey;
                // Tooltip data source (three lines): NodeName / LMP / Zone.
                pf["Lmp"] = item.LMP;
                pf["Zone"] = zone1;
                pf.Styles.Add(BuildNodeStyle(geo.NodeType, item.LMP));
                features.Add(pf);
            }

            nodesLayer.Features = features;
            nodesLayer.DataHasChanged();

            if (viewModel.LatestLMPList != null)
                viewModel.Count = viewModel.LatestLMPList.Count();
        }

        // New: single place mapping node-type → Mapsui SymbolStyle.
        private SymbolStyle BuildNodeStyle(string nodeType, double lmp)
        {
            var fill = ToMapsuiColor(GetColor(lmp));
            var outline = new MPen(MColor.Black, 2);
            var type = (nodeType ?? "").ToUpper();

            SymbolType symbol;
            switch (type)
            {
                case "ZONE": symbol = SymbolType.Triangle; break;
                case "HUB": symbol = SymbolType.Triangle; break;
                case "AGGREGATE": symbol = SymbolType.Rectangle; break; // diamond via SymbolRotation
                case "INTERFACE": symbol = SymbolType.Rectangle; break;
                default: symbol = SymbolType.Ellipse; break;
            }
            var s = new SymbolStyle
            {
                Fill = new MBrush(fill),
                Outline = outline,
                SymbolType = symbol,
                SymbolScale = NodeSymbolScale
            };
            if (type == "AGGREGATE") s.SymbolRotation = 45; // rectangle → diamond
            if (type == "ZONE") s.SymbolScale = ZoneSymbolScale;
            return s;
        }

        // New: rebuild the red constraint polylines + red self-loop points.
        public void RefreshConstraintLayer()
        {
            viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null || constraintLinesLayer == null) return;

            var fillFeatures = new List<IFeature>();
            var lineFeatures = new List<IFeature>();
            if (viewModel.PolyLineConstraints != null)
            {
                var redPen = new MPen(MColor.FromString("Red"), 2f);
                foreach (var seg in viewModel.PolyLineConstraints)
                {
                    if (seg.Locations == null || seg.Locations.Count < 2) continue;
                    var coords = seg.Locations
                       .Select(location =>
                           SphericalMercator
                               .FromLonLat(
                                   location.Longitude,
                                   location.Latitude))
                       .Select(point => new Coordinate(point.x, point.y))
                       .ToList();

                    if (coords.Count < 3)
                        continue;

                    if (!coords.First().Equals2D(coords.Last()))
                        coords.Add(coords.First());

                    var ring = new LinearRing(coords.ToArray());

                    if (!ring.IsValid)
                        continue;

                    var polygon = new Polygon(ring);

                    var polygonFeature = new GeometryFeature
                    {
                        Geometry = polygon
                    };

                    polygonFeature["Name"] = seg.Name;

                    polygonFeature.Styles.Add(new VectorStyle
                    {
                        Fill = new MBrush(
                            MColor.FromArgb(80, 255, 0, 0)),
                        Outline = new MPen(
                            MColor.FromArgb(210, 220, 0, 0),
                            1.5f)
                    });

                    fillFeatures.Add(polygonFeature);

                    var line = new LineString(coords.ToArray());

                    var lineFeature = new GeometryFeature
                    {
                        Geometry = line
                    };

                    lineFeature["Name"] = seg.Name;

                    lineFeature.Styles.Add(new VectorStyle
                    {
                        Line = new MPen(MColor.FromString("Red"), 2f),
                        Outline = new MPen(MColor.FromString("Red"), 2f)
                    });

                    lineFeatures.Add(lineFeature);
                }
            }
            constraintFillLayer.Features = fillFeatures;
            constraintFillLayer.DataHasChanged();
            constraintLinesLayer.Features = lineFeatures;
            constraintLinesLayer.DataHasChanged();

            var pointFeatures = new List<IFeature>();
            if (viewModel.EclipsLineConstraints != null)
            {
                var pinkStyle = new SymbolStyle
                {
                    Fill = new MBrush(MColor.FromString("Red")),
                    Outline = new MPen(MColor.Black, 0.5),
                    SymbolType = SymbolType.Ellipse,
                    SymbolScale = PinkSymbolScale
                };
                foreach (var p in viewModel.EclipsLineConstraints)
                {
                    if (p.MapLocation == null) continue;
                    var merc = SphericalMercator.FromLonLat(p.MapLocation.Longitude, p.MapLocation.Latitude).ToMPoint();
                    var pf = new PointFeature(merc);
                    pf["Name"] = p.Name;
                    pf.Styles.Add(pinkStyle);
                    pointFeatures.Add(pf);
                }
            }
            constraintPointsLayer.Features = pointFeatures;
            constraintPointsLayer.DataHasChanged();
        }

        // New: rebuild the "Navigate LMP" and "Navigate Constraint" overlays.
        public void RefreshNavigateOverlays()
        {
            viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null) return;

            navigateLmpLayer.Features = BuildNavFeatures(viewModel.NavigateLMPLocations);
            navigateConstraintLayer.Features = BuildNavFeatures(viewModel.NavigateConstraintLocations);
            navigateLmpLayer.DataHasChanged();
            navigateConstraintLayer.DataHasChanged();
        }

        private static IEnumerable<IFeature> BuildNavFeatures(IEnumerable<PointMapPath> src)
        {
            var list = new List<IFeature>();
            if (src == null) return list;
            foreach (var p in src)
            {
                if (p == null || p.MapLocation == null) continue;
                var merc = SphericalMercator.FromLonLat(p.MapLocation.Longitude, p.MapLocation.Latitude).ToMPoint();
                var pf = new PointFeature(merc);
                pf["Name"] = p.Name;
                var wpfSolid = p.MyColor as SolidColorBrush;
                var fillColor = wpfSolid != null
                    ? ToMapsuiColor(wpfSolid.Color)
                    : MColor.FromString("OrangeRed");
                pf.Styles.Add(new SymbolStyle
                {
                    Fill = new MBrush(fillColor),
                    Outline = new MPen(MColor.Black, 0.6),
                    SymbolType = SymbolType.Ellipse,
                    SymbolScale = NavigateOverlayScale
                });
                list.Add(pf);
            }
            return list;
        }

        // [OLD - Bing version drew individual Shape objects onto LMPMapLayer]
        /*
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
                if (lmpLocation == null) { continue; }
                try
                {
                    if (viewModel.mNodeZoneList.ContainsKey(item.NodeName))
                        zone1 = viewModel.mNodeZoneList[item.NodeName];
                    string NodeType = null;
                    double lmp = 0;
                    paths.Add(new PointMapPath { NodeTypeName = lmpLocation.NodeType.ToUpper() });
                    Shape myobj;
                    foreach (var itemm in paths)
                    {
                        NodeType = itemm.NodeTypeName;
                        lmp = item.LMP;
                        break;
                    }
                    string description = item.NodeName + "\n" + Math.Round(item.LMP, 2).ToString("C2") + "\n" + "(" + zone1 + ")";
                    Location location = new Location(lmpLocation.Latitude, lmpLocation.Longitude);
                    switch (NodeType)
                    {
                        case "ZONE":       myobj = PlaceStar(location, GetColor(lmp), description); AddObject(myobj); break;
                        case "HUB":        myobj = PlaceTriangle(location, GetColor(lmp), description); AddObject(myobj); break;
                        case "AGGREGATE":  myobj = PlaceDiamond(location, GetColor(lmp), description); AddObject(myobj); break;
                        case "INTERFACE":  myobj = PlaceSquare(location, GetColor(lmp), description); AddObject(myobj); break;
                        case "GENERATOR":
                        case "LOAD":
                        case "BUS":
                        case "EXT":
                        case "EHV":
                        default:
                            myobj = PlaceCircleWithStroke(location, GetColor(lmp), description); AddObject(myobj); break;
                    }
                }
                catch (Exception ex) { }
            }
            if (viewModel.LatestLMPList != null)
                viewModel.Count = viewModel.LatestLMPList.Count();
        }
        */

        #endregion

        #region Table selection → green highlight (2 sec) — new

        private void ViewModel_LmpNodeSelected(object sender, NodeGeoDetail node)
        {
            if (node == null || selectedPinLayer == null || LmpMap?.Map == null) return;

            var merc = SphericalMercator.FromLonLat(node.Longitude, node.Latitude).ToMPoint();

            // 1) NAVIGATE FIRST
            //    Preserve zoom if user is already deeper than medium (edge-case rule).
            double currentRes = LmpMap.Map.Navigator.Viewport.Resolution;
            if (currentRes > NodeNavigationResolution)
                LmpMap.Map.Navigator.CenterOnAndZoomTo(merc, NodeNavigationResolution);
            else
                LmpMap.Map.Navigator.CenterOn(merc);

            // 2) THEN place the pin at the final target coordinate.
            UpdateSelectedPin(merc, node.BusName);
        }

        private void UpdateSelectedPin(MPoint merc, string label)
        {
            if (selectedPinLayer == null) return;

            // Rebuild features so exactly ONE pin exists — handles rapid A→B→C selection.
            var pin = new PointFeature(merc);
            pin["Name"] = label;

            if (_selectedPinBitmapId >= 0)
            {
                pin.Styles.Add(new SymbolStyle
                {
                    BitmapId = _selectedPinBitmapId,
                    SymbolScale = 0.10,  // tune to taste (0.4–0.7 typical for 64×96 asset)
                                         // pin TIP sits exactly on the coordinate. In Mapsui 4.x, positive Y is up.
                    SymbolOffset = new Offset(0, (_selectedPinHeightPx * 0.7) * 0.5)
                    // (offsetY = imageHeight/2 * SymbolScale)
                });
            }
            else
            {
                // Graceful fallback if bitmap failed to load: a solid red drop shape.
                pin.Styles.Add(new SymbolStyle
                {
                    Fill = new MBrush(MColor.FromString("Green")),
                    Outline = new MPen(MColor.Black, 1.5),
                    SymbolType = SymbolType.Ellipse,
                    SymbolScale = 0.55
                });
            }

            selectedPinLayer.Features = new List<IFeature> { pin };
            selectedPinLayer.DataHasChanged();
        }

        #endregion

        #region Colour helpers (unchanged palette)

        public WpfColor GetColor(double lmp) => GetBrush(lmp).Color;

        public SolidColorBrush GetBrush(double lmp)
        {
            if (lmp < -10) return new SolidColorBrush(WpfColor.FromRgb(115, 0, 148));
            if (lmp >= -10 && lmp < 0) return new SolidColorBrush(WpfColor.FromRgb(0, 77, 173));
            if (lmp >= 0 && lmp < 6) return new SolidColorBrush(WpfColor.FromRgb(49, 89, 255));
            if (lmp >= 6 && lmp < 14) return new SolidColorBrush(WpfColor.FromRgb(57, 113, 255));
            if (lmp >= 14 && lmp < 16) return new SolidColorBrush(WpfColor.FromRgb(57, 138, 255));
            if (lmp >= 16 && lmp < 20) return new SolidColorBrush(WpfColor.FromRgb(57, 162, 255));
            if (lmp >= 20 && lmp < 30) return new SolidColorBrush(WpfColor.FromRgb(49, 190, 255));
            if (lmp >= 30 && lmp < 34) return new SolidColorBrush(WpfColor.FromRgb(41, 215, 255));
            if (lmp >= 34 && lmp < 38) return new SolidColorBrush(WpfColor.FromRgb(24, 243, 255));
            if (lmp >= 38 && lmp < 42) return new SolidColorBrush(WpfColor.FromRgb(41, 255, 247));
            if (lmp >= 42 && lmp < 46) return new SolidColorBrush(WpfColor.FromRgb(90, 255, 222));
            if (lmp >= 46 && lmp < 50) return new SolidColorBrush(WpfColor.FromRgb(123, 255, 206));
            if (lmp >= 50 && lmp < 56) return new SolidColorBrush(WpfColor.FromRgb(148, 255, 173));
            if (lmp >= 56 && lmp < 62) return new SolidColorBrush(WpfColor.FromRgb(173, 255, 156));
            if (lmp >= 62 && lmp < 68) return new SolidColorBrush(WpfColor.FromRgb(198, 255, 132));
            if (lmp >= 68 && lmp < 76) return new SolidColorBrush(WpfColor.FromRgb(206, 255, 107));
            if (lmp >= 76 && lmp < 82) return new SolidColorBrush(WpfColor.FromRgb(231, 255, 82));
            if (lmp >= 82 && lmp < 90) return new SolidColorBrush(WpfColor.FromRgb(239, 255, 57));
            if (lmp >= 90 && lmp < 100) return new SolidColorBrush(WpfColor.FromRgb(255, 255, 24));
            if (lmp >= 100 && lmp < 115) return new SolidColorBrush(WpfColor.FromRgb(255, 239, 0));
            if (lmp >= 115 && lmp < 125) return new SolidColorBrush(WpfColor.FromRgb(255, 219, 0));
            if (lmp >= 125 && lmp < 150) return new SolidColorBrush(WpfColor.FromRgb(255, 195, 0));
            if (lmp >= 150 && lmp < 200) return new SolidColorBrush(WpfColor.FromRgb(255, 170, 0));
            if (lmp >= 200 && lmp < 250) return new SolidColorBrush(WpfColor.FromRgb(255, 150, 0));
            if (lmp >= 250 && lmp < 300) return new SolidColorBrush(WpfColor.FromRgb(156, 162, 165));
            if (lmp >= 300 && lmp < 400) return new SolidColorBrush(WpfColor.FromRgb(255, 101, 0));
            if (lmp >= 400 && lmp < 500) return new SolidColorBrush(WpfColor.FromRgb(255, 81, 0));
            if (lmp >= 500 && lmp < 600) return new SolidColorBrush(WpfColor.FromRgb(255, 48, 0));
            if (lmp >= 600) return new SolidColorBrush(WpfColor.FromRgb(255, 48, 0));
            return Brushes.Gold;
        }

        // [OLD - Bing version used explicit if/else return statements with
        //  new SolidColorBrush(). Body is functionally identical; kept for
        //  reference only.]
        /*
        public SolidColorBrush GetBrush(double lmp)
        {
            BrushConverter brush = new BrushConverter();
            if (lmp < -10) return new SolidColorBrush(Color.FromRgb(115, 0, 148));
            // ... (identical palette continues) ...
            else return Brushes.Gold;
        }
        */


        private static MColor ToMapsuiColor(WpfColor c) => new MColor(c.R, c.G, c.B, c.A);
        private static MColor ToMapsuiColor(SolidColorBrush b) => ToMapsuiColor(b.Color);

        #endregion

        #region UI and event handlers

        private void LoadMarketColumns()
        {
            int key = (int)market.SelectedValue;
            if (key == 1)
            {
                LatestConstraintGrid.Columns[1].Visibility = Visibility.Visible;
                LatestConstraintGrid.Columns[5].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[6].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[9].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[10].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[11].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[12].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[13].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[14].Visibility = Visibility.Hidden;
            }
            else
            {
                LatestConstraintGrid.Columns[1].Visibility = Visibility.Hidden;
                LatestConstraintGrid.Columns[5].Visibility = Visibility.Visible;
                LatestConstraintGrid.Columns[9].Visibility = Visibility.Visible;
                LatestConstraintGrid.Columns[10].Visibility = Visibility.Visible;
                LatestConstraintGrid.Columns[11].Visibility = Visibility.Visible;
                LatestConstraintGrid.Columns[12].Visibility = Visibility.Visible;
                LatestConstraintGrid.Columns[13].Visibility = Visibility.Visible;
                LatestConstraintGrid.Columns[14].Visibility = Visibility.Visible;
            }
        }

        public void SetPositions(int p1, int p2, int p3, int p4)
        {
            Height = p1; Width = p2; Top = p3; Left = p4;
        }


        // New: toggle both Mapsui constraint layers via their Enabled flag.
        private void ConstrainsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool enabled = !constraintLinesLayer.Enabled;

            constraintFillLayer.Enabled = enabled;
            constraintLinesLayer.Enabled = enabled;
            constraintPointsLayer.Enabled = enabled;

            LmpMap.RefreshGraphics();
        }

        // [OLD - Bing version toggled ConstrainsLayer + PolyConstrainsLayer visibility]
        /*
        private void ConstrainsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ConstrainsLayer.Visibility == System.Windows.Visibility.Hidden)
                PolyConstrainsLayer.Visibility = ConstrainsLayer.Visibility = System.Windows.Visibility.Visible;
            else
                PolyConstrainsLayer.Visibility = ConstrainsLayer.Visibility = System.Windows.Visibility.Hidden;
        }
        */

        // New: toggle Mapsui nodesLayer via Enabled flag.
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            nodesLayer.Enabled = !nodesLayer.Enabled;
            LmpMap.RefreshGraphics();
        }

        // [OLD - Bing version toggled LMPMapLayer visibility]
        /*
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (LMPMapLayer.Visibility == System.Windows.Visibility.Hidden)
                LMPMapLayer.Visibility = System.Windows.Visibility.Visible;
            else
                LMPMapLayer.Visibility = System.Windows.Visibility.Hidden;
        }
        */

        private void ZoomOutbutton_Click(object sender, RoutedEventArgs e) => LmpMap.Map.Navigator.ZoomOut();
        private void ZoomInbutton_Click(object sender, RoutedEventArgs e) => LmpMap.Map.Navigator.ZoomIn();


        private void buttonRoad_Click(object sender, RoutedEventArgs e) { /* OSM = road */ }

        // [OLD - Bing used LmpMap.ZoomLevel arithmetic; note the original
        //  code incremented on "ZoomOut" and decremented on "ZoomIn" which
        //  was the reverse of the button semantics. New code uses Mapsui
        //  navigator which is correct.]
        /*
        private void ZoomOutbutton_Click(object sender, RoutedEventArgs e)
        { LmpMap.ZoomLevel = LmpMap.ZoomLevel + 0.1; }

        private void ZoomInbutton_Click(object sender, RoutedEventArgs e)
        { LmpMap.ZoomLevel = LmpMap.ZoomLevel - 0.1; }
        */

        private void buttonAerial_Click(object sender, RoutedEventArgs e) { /* OSM = road */ }
        private void buttonAerialWithLabels_Click(object sender, RoutedEventArgs e) { /* OSM = road */ }

        // [OLD - Bing had distinct Road / Aerial / Aerial+Labels tile modes.
        //  Mapsui + OSM has a single road-style tile source, so the three
        //  mode buttons now no-op; kept to preserve XAML wiring.]
        /*
        private void buttonRoad_Click(object sender, RoutedEventArgs e)
        { LmpMap.Mode = new RoadMode(); }

        private void buttonAerial_Click(object sender, RoutedEventArgs e)
        { LmpMap.Mode = new AerialMode(); }

        private void buttonAerialWithLabels_Click(object sender, RoutedEventArgs e)
        {
            AerialMode mymode = new AerialMode();
            mymode.Labels = true;
            LmpMap.Mode = mymode;
        }
        */

        public void NodeAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            if (SPPLocationMarginal.SelectedCells != null && SPPLocationMarginal.SelectedCells.Count >= 1)
            {
                // Old code read the column header into a local ("column") that
                // was never used; dropped in the new version.
                var datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null) datacontext.ShowNodeAnalyzer(sourceSinkList);
            }
        }

        public void LMPGraph_Click(object sender, RoutedEventArgs e)
        {
            if (SPPLocationMarginal.SelectedCells != null && SPPLocationMarginal.SelectedCells.Count > 0)
            {
                var datacontext = DataContext as MainWindowViewModel;
                if (datacontext != null) datacontext.ShowLMPGraphs(sourceSinkList);
            }
        }

        public void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var rowIndex = new List<int>();
            sourceSinkList = new List<Tuple<string, string>>();
            foreach (var cell in SPPLocationMarginal.SelectedCells)
            {
                var cellItem = Vayu.CommonControls.DataGridInfo.GetCell(cell);
                int index = Vayu.CommonControls.DataGridInfo.GetRowIndex(cellItem);
                if (!rowIndex.Contains(index))
                {
                    var node = (LmpData)cell.Item;
                    sourceSinkList.Add(new Tuple<string, string>(node.NodeName, null));
                    rowIndex.Add(index);
                }
            }
        }


        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            LoadMarketColumns();
        }

        // [OLD - Bing version also re-centred the map using mMarketLocation
        //  (all lines were commented out even in original) and called
        //  LoadColorZones(). Centre is now handled by ViewModel_PropertyChanged.]
        /*
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
        */

        // New: subscribes to ViewModel events and does an initial refresh
        // of all Mapsui layers.
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainWindowViewModel;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                viewModel.LmpNodeSelected += ViewModel_LmpNodeSelected;
            }
            RefreshMapNew();
            RefreshConstraintLayer();
            RefreshNavigateOverlays();

            if (!_defaultViewApplied)
            {
                _defaultViewApplied = true;

                Dispatcher.BeginInvoke(
                    new Action(SetDefaultErcotView),
                    DispatcherPriority.Loaded);
            }
        }

        // [OLD - Bing version started the redraw jitter timer (tim) which
        //  Mapsui does not require. Also did not subscribe to VM events.]
        /*
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as MainWindowViewModel;
            // viewModel.Updatemap += viewModel_Updatemap;
            tim.Interval = 3000;
            tim.Elapsed += tim_Elapsed;
            tim.Start();
        }
        */

        private void NavigateToConstraint(IEnumerable<PointMapPath> points)
        {
            if (LmpMap?.Map == null) return;

            // Collect valid Mercator points from the constraint path
            var mercs = new List<MPoint>();
            if (points != null)
            {
                foreach (var p in points)
                {
                    if (p?.MapLocation == null) continue;
                    mercs.Add(SphericalMercator
                        .FromLonLat(p.MapLocation.Longitude, p.MapLocation.Latitude)
                        .ToMPoint());
                }
            }

            // No valid points -> fallback to VM center + medium constraint zoom
            if (mercs.Count == 0)
            {
                var loc = viewModel.MapCenterLocation;
                if (loc == null) return;
                var m = SphericalMercator.FromLonLat(loc.Longitude, loc.Latitude).ToMPoint();
                LmpMap.Map.Navigator.CenterOnAndZoomTo(m, ConstraintNavigationResolution);
                return;
            }

            // Single-node constraint -> treat like a node navigate
            if (mercs.Count == 1)
            {
                LmpMap.Map.Navigator.CenterOnAndZoomTo(mercs[0], NodeNavigationResolution);
                return;
            }

            // Multi-node: fit to bounding box with padding, clamped to ERCOT default
            double minX = mercs.Min(m => m.X), maxX = mercs.Max(m => m.X);
            double minY = mercs.Min(m => m.Y), maxY = mercs.Max(m => m.Y);

            // 0.25% padding so neighboring nodes remain visible
            double padX = Math.Max((maxX - minX) / 0.25, 1);
            double padY = Math.Max((maxY - minY) / 0.25, 1);
            var box = new MRect(minX - padX, minY - padY, maxX + padX, maxY + padY);

            LmpMap.Map.Navigator.ZoomToBox(box);

            // Clamp: don't zoom out beyond ERCOT default (very large constraints)
            if (LmpMap.Map.Navigator.Viewport.Resolution > MinNavigateResolution)
            {
                var center = new MPoint((minX + maxX) / 2.0, (minY + maxY) / 2.0);
                LmpMap.Map.Navigator.CenterOnAndZoomTo(center, MinNavigateResolution);
            }
        }

        // New: dispatch VM property changes to the correct layer refresh.
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "LocationList":
                case "LatestLMPList":
                    Dispatcher.BeginInvoke(new Action(RefreshMapNew));
                    break;
                case "PolyLineConstraints":
                case "EclipsLineConstraints":
                    Dispatcher.BeginInvoke(new Action(RefreshConstraintLayer));
                    break;
                case "NavigateLMPLocations":
                case "NavigateConstraintLocations":
                    Dispatcher.BeginInvoke(new Action(RefreshNavigateOverlays));
                    break;
                case "MapCenterLocation":
                    if (viewModel?.MapCenterLocation == null) break;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        NavigateToConstraint(viewModel.NavigateConstraintLocations);
                    }));
                    break;
            }
        }

        private void Outages_Click(object sender, RoutedEventArgs e)
        {
            DateTime mStartDate = DateTime.Today;
            DateTime mEndDate = DateTime.Today.AddDays(1);
            viewModel = DataContext as MainWindowViewModel;
            string Market = viewModel != null && viewModel.SelectedMarket == MarketsEnum.ERCOT ? "ERCOT" : "";
            if (LatestConstraintGrid.SelectedItems.Count > 0)
            {
                for (int i = 0; i < LatestConstraintGrid.SelectedItems.Count; i++)
                {
                    var selectedFile = (LatestConstraint)LatestConstraintGrid.SelectedItems[i];
                    // downstream call intentionally left as-is (was commented out in original)
                    //Vayu.ConstraintOutageMapping.ViewModels.MainWindowViewModel
                    //    .OpenConstraintOutageMappingScreen(selectedFile.ConstraintText,
                    //    selectedFile.ContigencyText, mStartDate, mEndDate, Market);
                }
            }
        }


        private void txtRefreshMap_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel = DataContext as MainWindowViewModel;
            txtRefreshMap.Clear();
            RefreshMapNew();
            if (viewModel != null) viewModel.RefreshConstraint_RaisePropertyChanged();
        }

        // [OLD - Bing version also cleared LMPMapLayer.Children before rebuild;
        //  Mapsui rebuild handles that internally via nodesLayer.Features assign.]
        /*
        private void txtRefreshMap_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel = this.DataContext as MainWindowViewModel;
            txtRefreshMap.Clear();
            LMPMapLayer.Children.Clear();
            RefreshMapNew();
            viewModel.RefreshConstraint_RaisePropertyChanged();
        }
        */


        public void NodeAnalyzer_click(object sender, RoutedEventArgs e) { throw new NotImplementedException(); }

        #endregion

        #region Old Bing Maps - Reference Only

        // The methods below were part of the Bing implementation and are not
        // needed under Mapsui. They are preserved as block-commented reference
        // so behaviour parity can be verified during code review.

        // [OLD - Bing zone painter used at ComboBox_SelectionChanged_1.
        //  Superseded by VM-driven zone rendering; XAML still uses zoneLayer
        //  which no longer exists in the Mapsui setup.]
        /*
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
                    switch (zoneitem.Name)
                    {
                        case "AEP":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));   break;
                        case "APS":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 219, 133, 108)); break;
                        case "AEC":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 77, 77));   break;
                        case "ATSI":    zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 179, 191, 128)); break;
                        case "BGE":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 204, 204));   break;
                        case "COMED":   zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 153, 0));   break;
                        case "DAYTON":  zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 108, 247, 49));  break;
                        case "DEOK":    zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 51, 153, 255));  break;
                        case "DOM":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 76, 153));    break;
                        case "DPL":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 208, 54, 219));  break;
                        case "DUQ":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(50, 204, 204, 0));    break;
                        case "JCPL":    zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255));   break;
                        case "METED":   zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 49, 252, 35));   break;
                        case "PECO":    zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 153, 76));    break;
                        case "PENELEC": zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 103, 57, 238));  break;
                        case "PEPCO":   zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 0));     break;
                        case "PPL":     zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 194, 126, 231)); break;
                        case "PSEG":    zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 255));   break;
                        case "RECO":    zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 153, 76, 0));    break;
                        case "EKPC":    zonePolygon.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 102, 102));   break;
                        default: break;
                    }
                    zonePolygon.Locations = zoneitem.Locations;
                    zonePolygon.ToolTip = zoneitem.Name;
                    LmpMap.Children.Add(zonePolygon);
                }
            }
        }
        */

        // [OLD - jitter redraw hack forced Bing to reproject after data load.
        //  Not needed under Mapsui.]
        /*
        private void ReDrawMap()
        {
            try
            {
                var c = LmpMap.Center;
                c.Latitude += factor;
                factor *= -1;
                LmpMap.SetView(c, LmpMap.ZoomLevel);
            }
            catch (Exception) { }
        }

        void tim_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_uiContext != null)
                _uiContext.Send((a) => ReDrawMap(), null);
        }

        void viewModel_Updatemap(object sender, EventArgs e)
        {
            if (_uiContext != null)
                _uiContext.Post((a) => ReDrawMap(), null);
        }
        */

        // [OLD - three zone-selection button handlers that painted MapPolygons
        //  onto LmpMap.Children. Replaced by VM-driven Mapsui rendering.]
        /*
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var polygonToDelete = LmpMap.Children.OfType<MapPolygon>()
                    .Where(p => ((MapPolygon)p).Tag == "ZoneMap").ToList();
            foreach (var p in polygonToDelete) LmpMap.Children.Remove(p);

            MainWindowViewModel model = DataContext as MainWindowViewModel;
            foreach (var item in model.ZoneList)
            {
                if (!item.IsSelected) continue;
                foreach (var zoneitem in item.RegionInfoList)
                {
                    MapPolygon zonePolygon = new MapPolygon();
                    // ... same switch (zoneitem.Name) palette as LoadColorZones ...
                    zonePolygon.Tag = "ZoneMap";
                    zonePolygon.Locations = zoneitem.Locations;
                    zonePolygon.ToolTip = zoneitem.Name;
                    LmpMap.Children.Add(zonePolygon);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Variant that force-selected every zone.
            // Body identical in structure to Button_Click above.
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var polygonToDelete = LmpMap.Children.OfType<MapPolygon>()
                    .Where(p => ((MapPolygon)p).Tag == "ZoneMap").ToList();
            foreach (var p in polygonToDelete) LmpMap.Children.Remove(p);
            MainWindowViewModel model = DataContext as MainWindowViewModel;
            foreach (var item in model.ZoneList) item.IsSelected = false;
        }
        */

        // [OLD - shape-placement helpers that returned WPF Shape objects for
        //  Bing MapLayer.SetPosition. Mapsui uses SymbolStyle instead
        //  (see BuildNodeStyle).]
        /*
        public Polygon PlaceStar(Location location, Color color, string text1) { ... }
        public Ellipse PlaceCircle(Location location, Color color, string text1) { ... }
        public Ellipse PlaceCircleWithStroke(Location location, Color color, string text1) { ... }
        public Rectangle PlaceSquare(Location location, Color color, string text1) { ... }
        public Polygon PlaceDiamond(Location location, Color color, string text1) { ... }
        public Polygon PlaceTriangle(Location location, Color color, string text1) { ... }
        public void AddObject(Shape myshape) { LMPMapLayer.Children.Add(myshape); }
        */

        #endregion
    }

    // ============================================================
    // Converters and attached helpers
    // ============================================================

    #region Active Converters and Attached Helpers

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Visibility v && v == Visibility.Visible;
    }

    public class BoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked) return !isChecked;
            return true;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded) return !isExpanded;
            return false;
        }
    }

    public static class HideColByMarket
    {
        public static string GetHideCol(DependencyObject obj) => (string)obj.GetValue(HideColProperty);
        public static void SetHideCol(DependencyObject obj, string value) => obj.SetValue(HideColProperty, value);

        public static readonly DependencyProperty HideColProperty =
            DependencyProperty.RegisterAttached("HideCol", typeof(string), typeof(HideColByMarket),
                new PropertyMetadata(new PropertyChangedCallback(HideCol)));

        private static void HideCol(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if (string.IsNullOrEmpty(e.NewValue as string) || grid == null) return;
            string market = ((string)e.NewValue).ToUpper();
            if (market == "PJM" || market == "MISO")
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
        public static string GetHidelmpCol(DependencyObject obj) => (string)obj.GetValue(HideColProperty);
        public static void SetHidelmpCol(DependencyObject obj, string value) => obj.SetValue(HideColProperty, value);

        public static readonly DependencyProperty HideColProperty =
            DependencyProperty.RegisterAttached("HidelmpCol", typeof(string), typeof(HidelmpColByMarket),
                new PropertyMetadata(new PropertyChangedCallback(HidelmpCol)));

        private static void HidelmpCol(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if (string.IsNullOrEmpty(e.NewValue as string) || grid == null) return;
            string market = ((string)e.NewValue).ToUpper();
            if (market == "PJM" || market == "MISO")
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

    #endregion

    #region Old Bing Maps Converters - Reference Only

    // [OLD - Bing-era converter that always returned a fixed yellow brush.
    //  Not referenced by the new Mapsui code path.]
    /*
    public class StringColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = ("255,255,0").ToString();
            if (string.IsNullOrEmpty(strValue)) return null;
            string[] splits = strValue.Split(',', ' ');
            byte b1, b2, b3;
            byte.TryParse(splits[0], out b1);
            byte.TryParse(splits[1], out b2);
            byte.TryParse(splits[2], out b3);
            SolidColorBrush br = new SolidColorBrush(Color.FromRgb(b1, b2, b3));
            return br;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
    */

    // [OLD - Bing-era converter that parsed the LMP out of a PointMapPath.Name
    //  string. Superseded by BuildNodeStyle which colours features directly.]
    /*
    public class ValueToShapeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Brushes.Gold;
            PointMapPath tempObj = value as PointMapPath;
            if (tempObj == null) return Brushes.Pink;
            String[] vlArray = tempObj.Name.ToString().Split(' ');
            string[] vlarray = vlArray[vlArray.Length - 1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            double lmp;
            try
            {
                if (double.TryParse(vlarray[vlarray.Length - 2].Replace("$", ""), out lmp))
                    return GetBrush(lmp);
                else
                    return Brushes.Salmon;
            }
            catch { return Brushes.Gold; }
        }
        private Brush GetBrush(double lmp) { ...palette identical to MainWindow.GetBrush... }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => null;
    }
    */

    #endregion
}