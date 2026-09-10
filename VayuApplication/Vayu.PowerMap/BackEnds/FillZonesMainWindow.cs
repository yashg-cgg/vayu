using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;
using Vayu.CommonControls;
using Vayu.LatestConstraintsInformationLibrary;

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
        /// The used color
        /// </summary>
        List<Color> usedColor;
        /// <summary>
        /// The m zone map layer
        /// </summary>
        private MapLayer mZoneMapLayer;
        /// <summary>
        /// The zone poylgon line
        /// </summary>
        private MapPolyline zonePoylgonLine;
        /// <summary>
        /// The zone list
        /// </summary>
        private ObservableCollection<ZoneInfo> zoneList;

        /// <summary>
        /// The poly line loc
        /// </summary>
        private LocationCollection polyLineLoc;
        /// <summary>
        /// The selected pushpin
        /// </summary>
        private Pushpin SelectedPushpin;
        /// <summary>
        /// The drag pin
        /// </summary>
        private bool _dragPin;
        /// <summary>
        /// The mouse to marker
        /// </summary>
        private Vector _mouseToMarker;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets or sets the map zone list.
        /// </summary>
        /// <value>
        /// The map zone list.
        /// </value>
        public ObservableCollection<ZoneInfo> MapZoneList
        {
            get { return zoneList; }
            set { zoneList = value; RaisePropertyChanged("MapZoneList"); }
        }

        /// <summary>
        /// Adds the poly line.
        /// </summary>
        /// <param name="zoneName">Name of the zone.</param>
        public void AddPolyLine(ZoneInfo zoneName)
        {
            List<LocationCollection> locList = GetRegionList(1, zoneName.Name);
            string[] cols = zoneName.StringColor.Split(',');
            byte r, g, b;
            byte.TryParse(cols[0], out r);
            byte.TryParse(cols[1], out g);
            byte.TryParse(cols[2], out b);
            Color col = Color.FromRgb(r, g, b);
            usedColor.Add(col);

            foreach (var item in locList)
            {
                MapPolyline zonePoylgon = new MapPolyline();
                mZoneMapLayer.Children.Insert(0, zonePoylgon);
                zonePoylgon.Fill = new SolidColorBrush(col);
                zonePoylgon.Opacity = 0.4;
                zonePoylgon.Locations = item;
                zonePoylgon.ToolTip = zoneName.Name;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Fills the zone list.
        /// </summary>
        private void FillZoneList()
        {
            MarketType mkTyp = GetMarketType(MarketlistBox.SelectedItem as string);
            List<ZoneInfo> zList = ZoneModel.GetZoneList((int)mkTyp);
            MapZoneList = new ObservableCollection<ZoneInfo>(zList);
        }

        /// <summary>
        /// Initiates this instance.
        /// </summary>
        private void Initiate()
        {
            if (mZoneMapLayer != null)
                MapControl.myMap.Children.Remove(mZoneMapLayer);

            mZoneMapLayer = new MapLayer();
            FillZoneList();
            MapControl.myMap.Children.Add(mZoneMapLayer);
            polyLineLoc = new LocationCollection();

            zonePoylgonLine = new MapPolyline();
            mZoneMapLayer.Children.Add(zonePoylgonLine);
            Color col = GetRandomColor();
            zonePoylgonLine.Fill = new SolidColorBrush(col);
            usedColor = new List<Color>();
        }

        /// <summary>
        /// Gets the type of the market.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private MarketType GetMarketType(string name)
        {
            if (name == "PJM")
                return MarketType.PJM;
            else if (name == "ERCOT")
                return MarketType.ERCOT;
            else
                return MarketType.NONE;
        }

        /// <summary>
        /// Gets the co ordinates.
        /// </summary>
        /// <param name="nodeIds">The node ids.</param>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        private LocationCollection GetCoOrdinates(string nodeIds, int i)
        {
            LocationCollection collection = new LocationCollection();
            SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand cmd = con.CreateCommand();


            cmd.CommandText = "select distinct Latitude,Longitude, NodeKey from Node where NodeKey in (" + nodeIds + ") and Longitude is not null and Latitude is not null ";
            if (i == 0)
                cmd.CommandText += " order by Latitude";
            else if (i == 1)
                cmd.CommandText += " order by Longitude";
            else if (i == 2)
                cmd.CommandText += " order by NodeKey";
            else //if (i == 3)
                cmd.CommandText += " order by NodeKey desc";

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                double? latitude = GetNullDouble(reader[0]);
                double? longitude = GetNullDouble(reader[1]);

                if (latitude.HasValue && longitude.HasValue)
                    collection.Add(new Location(latitude.Value, longitude.Value));
            }

            reader.Close();
            con.Close();
            return collection;
        }

        /// <summary>
        /// Gets the null double.
        /// </summary>
        /// <param name="objVal">The object value.</param>
        /// <returns></returns>
        private double? GetNullDouble(object objVal)
        {
            double doub;
            if (double.TryParse((objVal ?? "").ToString(), out doub))
                return doub;

            return null;
        }

        /// <summary>
        /// Gets the region list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <returns></returns>
        private List<LocationCollection> GetRegionList(int marketKey, string zoneName)
        {
            List<LocationCollection> list = new List<LocationCollection>();
            SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand cmd = con.CreateCommand();

            cmd.CommandText = "select Latitude ,Longitude , Sequence , regionID from ZoneInfo where Zone = '" + zoneName + "' and MarketKey = " + marketKey + " order by Sequence";
            SqlDataReader reader = cmd.ExecuteReader();
            List<ZoneInfo> infList = new List<ZoneInfo>();
            while (reader.Read())
            {
                ZoneInfo inf = new ZoneInfo();

                double.TryParse(reader[0].ToString(), out inf.latitude);
                double.TryParse(reader[1].ToString(), out inf.longitude);
                int.TryParse(reader[2].ToString(), out inf.sequence);
                int.TryParse(reader[3].ToString(), out inf.regionID);

                infList.Add(inf);
            }

            reader.Close();
            con.Close();
            foreach (var item in infList.GroupBy(x => x.regionID))
            {
                LocationCollection col = new LocationCollection();
                foreach (var itemF in item.OrderBy(x => x.sequence))
                    col.Add(new Location(itemF.latitude, itemF.longitude));

                list.Add(col);
            }
            return list;
        }

        /// <summary>
        /// Gets the random color.
        /// </summary>
        /// <returns></returns>
        private Color GetRandomColor()
        {
            //Color c = Color.
            Color[] ColorList =
             typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Select(propInfo => propInfo.GetValue(null, null))
            .Cast<Color>()
            .ToArray();
            Random rand = new Random();
            int index = rand.Next(0, ColorList.Length - 1);
            return ColorList[index];
        }

        #endregion

        #region Events

        /// <summary>
        /// Alls the checked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AllChecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = e.OriginalSource as System.Windows.Controls.CheckBox;

            foreach (var item in MapZoneList)
            {
                item.IsSelected = checkBox.IsChecked.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Shows the un show.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShowUnShow(object sender, RoutedEventArgs e)
        {
            mZoneMapLayer.Children.Clear();
            foreach (var item in MapZoneList)
            {
                if (item.IsSelected)
                    AddPolyLine(item);
            }
        }

        /// <summary>
        /// Handles the Click event of the SetPin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SetPin_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPushpin != null)
                mZoneMapLayer.Children.Remove(SelectedPushpin);

            SelectedPushpin = new Pushpin();
            SelectedPushpin.AllowDrop = true;
            SelectedPushpin.MouseDown += Pin_MouseDown;
            SelectedPushpin.MouseMove += Pin_MouseMove;
            mZoneMapLayer.Children.Add(SelectedPushpin);
            SelectedPushpin.Location = MapControl.InternalMapCenter;
            SelectedPushpin.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the MouseMove event of the Pin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void Pin_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_dragPin && SelectedPushpin != null)
                {
                    SelectedPushpin.Location = MapControl.myMap.ViewportPointToLocation(
                      Point.Add(e.GetPosition(MapControl.myMap), _mouseToMarker));
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the Pin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Pin_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            SelectedPushpin = sender as Pushpin;
            _dragPin = true;
            _mouseToMarker = Point.Subtract(
            MapControl.myMap.LocationToViewportPoint(SelectedPushpin.Location),
              e.GetPosition(MapControl.myMap));
        }

        /// <summary>
        /// Handles the Click event of the SetLoc control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SetLoc_Click(object sender, RoutedEventArgs e)
        {
            polyLineLoc.Add(SelectedPushpin.Location);
            //zonePoylgonLine.Opacity = .3;
            zonePoylgonLine.Locations = polyLineLoc;// GetCoOrdinates(nodeIds, i);
        }

        /// <summary>
        /// Handles the Click event of the Reset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Initiate();
            ShowUnShow(null, null);
            SetPin_Click(null, null);
        }

        /// <summary>
        /// Handles the Click event of the Save control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string zoneName = "ATSI";
            SqlCommand cmd = EnergyMapHelper.GetTradingDBCommand();

            for (int i = 0; i < polyLineLoc.Count; i++)
            {
                cmd.CommandText = " insert Zoneinfo ( MarketKey , Zone , Latitude , Longitude , RegionID , Sequence ) values (1, '" + zoneName +
                    "' , " + polyLineLoc[i].Latitude + "," + polyLineLoc[i].Longitude + " , 3 , " + i + ")";
                cmd.ExecuteNonQuery();
            }
            cmd.Connection.Close();
            FillZoneList();
        }

        #endregion


        //private List<int> GetRegionList(int marketKey,string zoneName)
        //{
        //    List<int> list = new List<int>();
        //    SqlCommand cmd = con.CreateCommand();
        //    con.Open();
        //    cmd.CommandText = "select NodeKey from ZoneInfo where Zone = '" + zoneName + "' and MarketKey = " + marketKey; // "select distinct top 6 MIN(NodeKey) from Node where Zone = '" + zoneName + "' and MarketKey = " + marketKey + " group by Latitude,Longitude" ;
        //    SqlDataReader reader = cmd.ExecuteReader();

        //    while (reader.Read())
        //    {
        //        int id = 0;
        //        int.TryParse(reader[0].ToString(), out id);
        //        list.Add(id);
        //    }

        //    reader.Close();
        //    con.Close();
        //    return list;
        //}

    }
}