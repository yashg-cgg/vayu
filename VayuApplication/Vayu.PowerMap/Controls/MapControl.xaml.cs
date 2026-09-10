using Microsoft.Maps.MapControl.WPF;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Vayu.PowerMap.Controls
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        public MapControl()
        {
            InitializeComponent();
            myMap.ViewChangeOnFrame += new EventHandler<MapEventArgs>(myMap_ViewChangeOnFrame);
            myMap.MouseMove += new MouseEventHandler(myMap_MouseMove);
            buttonRoad.Click += new RoutedEventHandler(buttonRoad_Click);
            buttonAerial.Click += new RoutedEventHandler(buttonAerial_Click);
            buttonAerialWithLabels.Click += new RoutedEventHandler(buttonAerialWithLabels_Click);
            ZoomInbutton.Click += new RoutedEventHandler(ZoomInbutton_Click);
            ZoomOutbutton.Click += new RoutedEventHandler(ZoomOutbutton_Click);
            InternalZoomLevel = 4;
            InternalMapCenter = new Location(39.3683, -95.2734);
        }
        private double internalZoomLevel;
        public double InternalZoomLevel
        {
            get { return internalZoomLevel; }
            set { internalZoomLevel = value; RaisePropertyChanged(); MapZoomLevel = value; }
        }

        private Location internalMapCenter;
        public Location InternalMapCenter
        {
            get
            {
                if (internalMapCenter == null)
                    internalMapCenter = new Location(39.3683, -95.2734);
                return internalMapCenter;
            }
            set { internalMapCenter = value; RaisePropertyChanged(); SetStringLocation(value); }
        }
        private void myMap_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            if (BoundingtextBlock != null)
            {
                //Gets the map that raised this event
                Map map = (Map)sender;
                //Gets the bounded rectangle for the current frame
                LocationRect bounds = map.BoundingRectangle;
                //Update the current latitude and longitude
                BoundingtextBlock.TextWrapping = TextWrapping.Wrap;
                BoundingtextBlock.Text = String.Format("(Current Bounding) \nNorthwest: {0:F3}  \nNortheast: {1:F3} \nSoutheast: {2:F3}  \nSouthwest: {3:F3}  \nZoom Level: {4:f2}",
                           bounds.Northwest, bounds.Northeast, bounds.Southeast, bounds.Southwest, myMap.ZoomLevel);
            }
        }

        private void myMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point viewportpoint = e.GetPosition(myMap);
            Location location;
            if (myMap.TryViewportPointToLocation(viewportpoint, out location))
            {
                LocationtextBlock.Text = String.Format("Lat: {0:f3}, Lon: {1:f3}", location.Latitude, location.Longitude);
            }
        }

        #region Choose map styles

        private void buttonRoad_Click(object sender, RoutedEventArgs e)
        {
            myMap.Mode = new RoadMode();
        }

        private void buttonAerial_Click(object sender, RoutedEventArgs e)
        {
            myMap.Mode = new AerialMode();
        }

        private void buttonAerialWithLabels_Click(object sender, RoutedEventArgs e)
        {
            AerialMode mymode = new AerialMode();
            mymode.Labels = true;
            myMap.Mode = mymode;
        }

        #endregion

        private void ZoomInbutton_Click(object sender, RoutedEventArgs e)
        {
            myMap.ZoomLevel = myMap.ZoomLevel - 0.1;
        }

        private void ZoomOutbutton_Click(object sender, RoutedEventArgs e)
        {
            myMap.ZoomLevel = myMap.ZoomLevel + 0.1;
        }

        private void myMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            Location loc = myMap.ViewportPointToLocation(p);
            LastClickCoOrdinates = loc;
            SetStringLocation(loc);
        }

        private void SetStringLocation(Location loc)
        {
            if (loc != null)
                StringLocation = String.Format("Lat: {0:N2} Log: {1:N2}", loc.Latitude, loc.Longitude);
        }

        public Location LastClickCoOrdinates
        {
            get { return (Location)GetValue(LastClickCoOrdinatesProperty); }
            set { SetValue(LastClickCoOrdinatesProperty, value); }
        }
        // Using a DependencyProperty as the backing store for LastClickCoOrdinates.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastClickCoOrdinatesProperty =
            DependencyProperty.Register("LastClickCoOrdinates", typeof(Location), typeof(MapControl), new PropertyMetadata(null));

        public string StringLocation
        {
            get { return (string)GetValue(StringLocationProperty); }
            set { SetValue(StringLocationProperty, value); }
        }
        // Using a DependencyProperty as the backing store for StringLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringLocationProperty =
            DependencyProperty.Register("StringLocation", typeof(string), typeof(MapControl), new PropertyMetadata(null));

        public double MapZoomLevel
        {
            get { return (double)GetValue(MapZoomLevelProperty); }
            set { SetValue(MapZoomLevelProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MapZoomLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapZoomLevelProperty =
            DependencyProperty.Register("MapZoomLevel", typeof(double), typeof(MapControl), new PropertyMetadata(0.75));

        private void RaisePropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
