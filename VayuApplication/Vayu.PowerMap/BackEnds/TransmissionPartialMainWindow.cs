using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public partial class MainWindow
    {
        /// <summary>
        /// The transmission list
        /// </summary>
        private ObservableCollection<BranchModel> transmissionList;
        /// <summary>
        /// Gets or sets the transmission list.
        /// </summary>
        /// <value>
        /// The transmission list.
        /// </value>
        public ObservableCollection<BranchModel> TransmissionList
        {
            get { return transmissionList; }
            set { transmissionList = value; RaisePropertyChanged("TransmissionList"); }
        }

        /// <summary>
        /// Transmissions the main.
        /// </summary>
        private void TransmissionMain()
        {
            MapControl.myMap.Children.Remove(mTransmissionMapLayer);
            mTransmissionMapLayer.Children.Clear();
            Transmission_BuildMap();
            if (this.TransmissionCheckBox.IsChecked != null && (bool)this.TransmissionCheckBox.IsChecked)
            {
                MapControl.myMap.Children.Add(mTransmissionMapLayer);
            }
        }

        /// <summary>
        /// Transmissions the build map.
        /// </summary>
        private void Transmission_BuildMap()
        {
            List<BranchModel> brList = new List<BranchModel>();
            List<string> names = new List<string>();
            List<StationModel> stationlist = new List<StationModel>();
            Int16 trans_Max;
            Int16 trans_Min;
            if (!Int16.TryParse(Transmisson_MaxKV_Textbox.Text, out trans_Max))
            {
                trans_Max = Int16.MaxValue;
            }
            if (!Int16.TryParse(Transmisson_MinKV_Textbox.Text, out trans_Min))
            {
                trans_Min = Int16.MinValue;
            }
            List<BranchModel> marketbranchlist = mBranchList.Where(p => p.marketkey == mMarketInContext).ToList();
            foreach (BranchModel br in marketbranchlist)
            {
                if (br.devicetype.ToUpper() == "LINE")
                {
                    if (br.kv >= trans_Min && br.kv <= trans_Max)
                    {
                        DrawBranch(br);
                        brList.Add(br);
                        names.Add(br.branchname);

                        if (!stationlist.Exists(p => p.branch == br.branch))
                        {
                            StationModel st = new StationModel();
                            st.branch = br.branch;
                            st.branch_location = br.branch_location;
                            stationlist.Add(st);
                        }
                        if (!stationlist.Exists(p => p.branch == br.tobranch))
                        {
                            StationModel st = new StationModel();
                            st.branch = br.tobranch;
                            st.branch_location = br.tobranch_location;
                            stationlist.Add(st);
                        }
                    }
                }
            }
            if (Transmissionshowstation)
            {
                foreach (StationModel st in stationlist)
                {
                    DrawStation(st.branch_location, st.branch);
                }
            }
            TransmissionAutoCompleteBox.ItemsSource = names;
            TransmissionList = new ObservableCollection<BranchModel>(brList);
        }

        /// <summary>
        /// Draws the station.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="text1">The text1.</param>
        private void DrawStation(Location location, string text1)
        {
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = new SolidColorBrush(Colors.Green);
            myEllipse.Fill.Opacity = 0.4;
            myEllipse.Stroke = new SolidColorBrush(Colors.Black);
            myEllipse.StrokeThickness = 0.5;
            double diameter = 8;
            double radius = diameter / 2;
            myEllipse.Width = diameter;
            myEllipse.Height = diameter;
            myEllipse.Margin = new Thickness(-radius / Math.Sqrt(2), -radius / Math.Sqrt(2), 0, 0);
            ToolTip tt = new ToolTip();
            tt.Content = "Station: " + text1;
            tt.FontWeight = FontWeights.Bold;
            myEllipse.ToolTip = tt;

            Point p0 = MapControl.myMap.LocationToViewportPoint(location);
            Location loc = MapControl.myMap.ViewportPointToLocation(p0);
            MapLayer.SetPosition(myEllipse, loc);
            ToolTipService.SetShowDuration(myEllipse, 300000);
            mTransmissionMapLayer.Children.Add(myEllipse);
        }
        /// <summary>
        /// Draws the branch.
        /// </summary>
        /// <param name="br">The br.</param>
        private void DrawBranch(BranchModel br)
        {
            MyPolyline line = new MyPolyline();
            line.branch = br;
            string description = br.branchname;
            Location start = br.branch_location;
            Location finish = br.tobranch_location;
            LocationCollection coll = new LocationCollection();
            coll.Add(start);
            coll.Add(finish);
            line.Stroke = new SolidColorBrush(SelectColor(br.kv));
            line.StrokeThickness = 1.0;
            line.Locations = coll;
            ToolTip tt = new ToolTip();
            tt.Content = description;
            tt.FontWeight = FontWeights.Bold;
            line.ToolTip = tt;
            ToolTipService.SetShowDuration(line, 300000);
            mTransmissionMapLayer.Children.Add(line);
        }
        /// <summary>
        /// Selects the color.
        /// </summary>
        /// <param name="kvlevel">The kvlevel.</param>
        /// <returns></returns>
        private Color SelectColor(int kvlevel)
        {
            int[] colorarray = new int[5] { 70, 138, 230, 300, 500 };
            if (kvlevel <= colorarray[0])
            {
                return Colors.Brown;
            }
            else if (colorarray[0] < kvlevel && kvlevel <= colorarray[1])
            {
                return Colors.Magenta;
            }
            else if (colorarray[1] < kvlevel && kvlevel <= colorarray[2])
            {
                return Colors.DarkRed;
            }
            else if (colorarray[2] < kvlevel && kvlevel <= colorarray[3])
            {
                return Colors.DarkBlue;
            }
            else if (colorarray[3] < kvlevel && kvlevel <= colorarray[4])
            {
                return Colors.DarkGreen;
            }
            else
            {
                return Colors.Purple;
            }
        }
    }
}
