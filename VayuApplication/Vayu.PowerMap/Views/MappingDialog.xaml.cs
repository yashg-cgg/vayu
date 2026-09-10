using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// Interaction logic for MappingDialog.xaml
    /// </summary>
    public partial class MappingDialog : Window
    {
        #region Declaration & Properties

        /// <summary>
        /// The sourcepin
        /// </summary>
        private Pushpin sourcepin = new Pushpin();
        /// <summary>
        /// The sinkpin
        /// </summary>
        private Pushpin sinkpin = new Pushpin();
        /// <summary>
        /// The line
        /// </summary>
        private MapPolyline line = new MapPolyline();

        /// <summary>
        /// Gets or sets the Sigma database connection.
        /// </summary>
        /// <value>
        /// The Sigma database connection.
        /// </value>
        private SqlConnection VayuConnection { get; set; }
        /// <summary>
        /// Gets or sets the m select station geo.
        /// </summary>
        /// <value>
        /// The m select station geo.
        /// </value>
        private SqlCommand mSelect_Station_Geo { get; set; }
        /// <summary>
        /// Gets or sets the m select psse coordinates.
        /// </summary>
        /// <value>
        /// The m select psse coordinates.
        /// </value>
        private SqlCommand mSelect_PSSE_Coordinates { get; set; }
        /// <summary>
        /// Gets or sets the m select zone.
        /// </summary>
        /// <value>
        /// The m select zone.
        /// </value>
        private SqlCommand mSelect_Zone { get; set; }
        /// <summary>
        /// Gets or sets the m update to branch actual.
        /// </summary>
        /// <value>
        /// The m update to branch actual.
        /// </value>
        private SqlCommand mUpdate_ToBranch_Actual { get; set; }
        /// <summary>
        /// Gets or sets the m update to branch planned.
        /// </summary>
        /// <value>
        /// The m update to branch planned.
        /// </value>
        private SqlCommand mUpdate_ToBranch_Planned { get; set; }
        /// <summary>
        /// Gets or sets the m psse bus hash.
        /// </summary>
        /// <value>
        /// The m psse bus hash.
        /// </value>
        private Dictionary<string, PSSEBus> mPSSEBusHash { get; set; }

        /// <summary>
        /// The moutage
        /// </summary>
        private Outage _moutage;
        /// <summary>
        /// Gets or sets the moutage.
        /// </summary>
        /// <value>
        /// The moutage.
        /// </value>
        public Outage moutage
        {
            get { return _moutage; }
            set
            {
                _moutage = value;
                RaisePropertyChanged("myoutage");
            }
        }

        /// <summary>
        /// The mylayer
        /// </summary>
        private MapLayer _mylayer;
        /// <summary>
        /// Gets or sets the mylayer.
        /// </summary>
        /// <value>
        /// The mylayer.
        /// </value>
        public MapLayer mylayer
        {
            get { return _mylayer; }
            set
            {
                _mylayer = value;
                RaisePropertyChanged("mylayer");
            }
        }

        /// <summary>
        /// The m key list
        /// </summary>
        private List<string> mKeyList;
        /// <summary>
        /// Gets or sets the key list.
        /// </summary>
        /// <value>
        /// The key list.
        /// </value>
        public List<string> KeyList
        {
            get { return mKeyList; }
            set
            {
                mKeyList = value;
                RaisePropertyChanged("KeyList");
            }
        }

        /// <summary>
        /// The m zone list
        /// </summary>
        private List<string> mZoneList;
        /// <summary>
        /// Gets or sets the zone list.
        /// </summary>
        /// <value>
        /// The zone list.
        /// </value>
        public List<string> ZoneList
        {
            get { return mZoneList; }
            set
            {
                mZoneList = value;
                RaisePropertyChanged("ZoneList");
            }
        }

        /// <summary>
        /// The m comment
        /// </summary>
        private string mComment = "";
        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment
        {
            get { return mComment; }
            set
            {
                mComment = value;
                RaisePropertyChanged("Comment");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingDialog"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        public MappingDialog(object obj)
        {
            InitializeComponent();
            IntiDB();
            LoadData(obj);
            MapSetup();
            //Souce_listBox.SelectionChanged += new SelectionChangedEventHandler(Souce_listBox_SelectionChanged);
            Sink_listBox.SelectionChanged += new SelectionChangedEventHandler(Sink_listBox_SelectionChanged);
            Zone_listBox.SelectionChanged += new SelectionChangedEventHandler(Zone_listBox_SelectionChanged);
        }

        /// <summary>
        /// Maps the setup.
        /// </summary>
        public void MapSetup()
        {
            sourcepin.Background = Brushes.MediumPurple;
            sourcepin.Background = Brushes.SpringGreen;
            line.Stroke = Brushes.Red;
            line.StrokeThickness = 4.0;

            string key = string.Format("{0}!{1}", moutage.branch, moutage.voltage);
            if (!mPSSEBusHash.ContainsKey(key)) return;
            PSSEBus selectedbus = mPSSEBusHash[key];
            ToolTip tt = new ToolTip();
            tt.Content = moutage.branch;
            sourcepin.Location = new Location(selectedbus.latitude, selectedbus.longitude);
            sourcepin.ToolTip = tt;

            sinkpin.Visibility = Visibility.Hidden;
            line.Visibility = Visibility.Hidden;

            MapControl.myMap.Children.Add(sourcepin);
            MapControl.myMap.Children.Add(sinkpin);
            MapControl.myMap.Children.Add(line);
        }

        /// <summary>
        /// Intis the database.
        /// </summary>
        public void IntiDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            mSelect_Station_Geo = new SqlCommand();
            mSelect_Station_Geo.CommandText =
                "select A.NodeName, " +
                "A.NodeKey, " +
                "A.Longitude, " +
                "A.Latitude, " +
                "C.NodeTypeKey, " +
                "A.KV, " +
                "A.PSSENAME, " +
                "A.MarketKey, " +
                "Zone = case when (C.zone is null or C.zone = '') then 'N/A' else C.zone end " +
                "from nodegeo A left join Node C (NOLOCK) on A.NodeKey = C.NodeKey";
            //mSelect_Station_Geo.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelect_Station_Geo.Connection = VayuConnection;
            mSelect_Station_Geo.CommandTimeout = 30;

            mSelect_PSSE_Coordinates = new SqlCommand();
            mSelect_PSSE_Coordinates.CommandText =
                "select BusName, Longitude, Latitude, BusNomVolt from pssebusgeo where MarketKey = 1";
            mSelect_PSSE_Coordinates.Connection = VayuConnection;
            mSelect_PSSE_Coordinates.CommandTimeout = 30;

            mSelect_Zone = new SqlCommand();
            mSelect_Zone.CommandText = "select distinct(Zone) as Zone from PJM_rt_outages";
            mSelect_Zone.Connection = VayuConnection;

            mUpdate_ToBranch_Actual = new SqlCommand();
            mUpdate_ToBranch_Actual.CommandText = "Update PJM_current_rt_outages " +
                                           "set ToBranch = @toBranch, Editor = @editor, EditTime = @edittime, Comment = @comment, Zone = @zone " +
                                           "where Equipment = @equipment and Voltage = @voltage and Branch = @branch and EquipmentType = @equipmenttype ";
            //and (toBranch is null or toBranch = '') ";
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@equipment", "equipment");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@toBranch", "toBranch");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@editor", "editor");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@edittime", "edittime");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@comment", "comment");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@equipmenttype", "equipmenttype");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@voltage", "voltage");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@branch", "branch");
            mUpdate_ToBranch_Actual.Parameters.AddWithValue("@zone", "zone");
            mUpdate_ToBranch_Actual.Connection = VayuConnection;

            mUpdate_ToBranch_Planned = new SqlCommand();
            mUpdate_ToBranch_Planned.CommandText = "Update PJM_rt_outages " +
                                           "set ToBranch = @toBranch, Editor = @editor, EditTime = @edittime, Comment = @comment " +
                                           "where Equipment = @equipment and Voltage = @voltage and Branch = @branch and EquipmentType = @equipmenttype ";
            //and (toBranch is null or toBranch = '') ";
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@equipment", "equipment");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@toBranch", "toBranch");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@editor", "editor");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@edittime", "edittime");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@comment", "comment");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@equipmenttype", "equipmenttype");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@voltage", "voltage");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@branch", "branch");
            mUpdate_ToBranch_Planned.Parameters.AddWithValue("@zone", "zone");
            mUpdate_ToBranch_Planned.Connection = VayuConnection;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void LoadData(object obj)
        {
            if (obj == null) return;
            DataTable busgeo = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = mSelect_PSSE_Coordinates;
            dataAdapter.Fill(busgeo);

            switch (obj.GetType().Name.ToUpper())
            {
                case "OUTAGE":
                    Outage outage = (Outage)obj;
                    moutage = outage;
                    Outage_DataGrid.Items.Add(moutage);
                    //if (outage.equipmentType == "XFMR" || outage.equipmentType == "XF")
                    //{
                    //    Sink_listBox.Visibility = Visibility.Hidden;
                    //    Sink_AutoCompleteBox.Visibility = Visibility.Hidden;
                    //}
                    break;
            }

            mPSSEBusHash = new Dictionary<string, PSSEBus>();
            foreach (DataRow row in busgeo.Rows)
            {
                PSSEBus bus = new PSSEBus();
                string name = row["BusName"].ToString();
                string kv = string.Empty;
                if (row["BusNomVolt"] != DBNull.Value)
                {
                    kv = Math.Round(Convert.ToDouble(row["BusNomVolt"])).ToString();
                }
                // Souce_listBox.Items.Add(string.Format("{0}!{1}", name, kv));
                // Sink_listBox.Items.Add(string.Format("{0}!{1}", name, kv));
                bus.name = name;
                //bus.kV = kv;
                bus.latitude = Convert.ToDouble(row["Latitude"]);
                bus.longitude = Convert.ToDouble(row["Longitude"]);

                string key = string.Format("{0}!{1}", name, kv);
                if (!mPSSEBusHash.ContainsKey(key)) mPSSEBusHash.Add(key, bus);
            }
            DataView busview = busgeo.DefaultView;
            busview.RowFilter = string.Format("BusNomVolt = {0}", moutage.voltage);
            KeyList = (from row in busview.ToTable().AsEnumerable() select row.Field<string>("BusName")).Distinct().ToList();
            //KeyList = mPSSEBusHash.Keys.ToList();

            DataTable zonetable = new DataTable();
            SqlDataAdapter zoneAdapter = new SqlDataAdapter();
            zoneAdapter.SelectCommand = mSelect_Zone;
            zoneAdapter.Fill(zonetable);
            ZoneList = (from row in zonetable.AsEnumerable() select row.Field<string>("Zone")).ToList();

        }

        #endregion

        //private void Souce_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    string item = Souce_listBox.SelectedItem.ToString();
        //    PSSEBus selectedbus = mPSSEBusHash[item];
        //    ToolTip tt = new ToolTip();
        //    tt.Content = item;
        //    sourcepin.Location = new Location(selectedbus.latitude, selectedbus.longitude);
        //    sourcepin.ToolTip = tt;
        //    sourcepin.Visibility = Visibility.Visible;
        //    Drawline();
        //}

        #region Private Methods

        /// <summary>
        /// Drawlines this instance.
        /// </summary>
        private void Drawline()
        {
            if (sourcepin.Location == null) return;
            if (sinkpin.Visibility == Visibility.Visible)
            {
                LocationCollection coll = new LocationCollection();
                coll.Add(sourcepin.Location);
                coll.Add(sinkpin.Location);
                line.Locations = coll;
                line.Visibility = Visibility.Visible;
                Location zlocation = new Location((sourcepin.Location.Latitude + sinkpin.Location.Latitude) / 2,
                    (sourcepin.Location.Longitude + sinkpin.Location.Longitude) / 2);
                MapControl.myMap.Center = zlocation;
                double approxdistance = Math.Sqrt(Math.Pow(sourcepin.Location.Latitude - sinkpin.Location.Latitude, 2) +
                                                  Math.Pow(sourcepin.Location.Longitude - sinkpin.Location.Longitude, 2));
                MapControl.myMap.ZoomLevel = 9 - approxdistance / 2;
            }
            else if (sourcepin.Visibility == Visibility.Visible)
            {
                MapControl.myMap.Center = sourcepin.Location;
                MapControl.myMap.ZoomLevel = 9;
            }
            else if (sinkpin.Visibility == Visibility.Visible)
            {
                MapControl.myMap.Center = sinkpin.Location;
                MapControl.myMap.ZoomLevel = 9;
            }
        }

        /// <summary>
        /// Updates the outage.
        /// </summary>
        private void Update_Outage()
        {
            VayuConnection.Open();
            mUpdate_ToBranch_Actual.Parameters["@Branch"].Value = moutage.branch;
            mUpdate_ToBranch_Actual.Parameters["@toBranch"].Value = moutage.tobranch;
            mUpdate_ToBranch_Actual.Parameters["@equipment"].Value = moutage.equipment;
            mUpdate_ToBranch_Actual.Parameters["@voltage"].Value = moutage.voltage;
            mUpdate_ToBranch_Actual.Parameters["@equipmenttype"].Value = moutage.equipmentType;
            mUpdate_ToBranch_Actual.Parameters["@editor"].Value = Environment.UserName.ToUpper();
            mUpdate_ToBranch_Actual.Parameters["@edittime"].Value = DateTime.Now;
            mUpdate_ToBranch_Actual.Parameters["@comment"].Value = Comment;
            mUpdate_ToBranch_Actual.Parameters["@zone"].Value = moutage.zone;

            mUpdate_ToBranch_Planned.Parameters["@Branch"].Value = moutage.branch;
            mUpdate_ToBranch_Planned.Parameters["@toBranch"].Value = moutage.tobranch;
            mUpdate_ToBranch_Planned.Parameters["@equipment"].Value = moutage.equipment;
            mUpdate_ToBranch_Planned.Parameters["@zone"].Value = moutage.zone;
            mUpdate_ToBranch_Planned.Parameters["@voltage"].Value = moutage.voltage;
            mUpdate_ToBranch_Planned.Parameters["@equipmenttype"].Value = moutage.equipmentType;
            mUpdate_ToBranch_Planned.Parameters["@editor"].Value = Environment.UserName.ToUpper();
            mUpdate_ToBranch_Planned.Parameters["@edittime"].Value = DateTime.Now;
            mUpdate_ToBranch_Planned.Parameters["@comment"].Value = Comment;

            mUpdate_ToBranch_Actual.ExecuteNonQuery();
            mUpdate_ToBranch_Planned.ExecuteNonQuery();
            VayuConnection.Close();
        }

        #endregion

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the Sink_listBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Sink_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string item = Sink_listBox.SelectedItem.ToString();
            string key = string.Format("{0}!{1}", item, moutage.voltage);
            PSSEBus selectedbus = mPSSEBusHash[key];
            ToolTip tt = new ToolTip();
            tt.Content = item;
            sinkpin.Location = new Location(selectedbus.latitude, selectedbus.longitude);
            sinkpin.ToolTip = tt;
            sinkpin.Visibility = Visibility.Visible;
            moutage.tobranch = item;
            Drawline();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Zone_listBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Zone_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string item = Zone_listBox.SelectedItem.ToString();
            moutage.zone = item;
        }

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Format("Branch: {0}\nToBranch: {1}\nZone: {2}\nVoltage: {3}\nEquipment: {4}", moutage.branch,
                                           moutage.tobranch, moutage.zone, moutage.voltage, moutage.equipment);
            MessageBoxResult result = MessageBox.Show("The changes you are about to make: \n" + message, "ComfirmDialog",
                                                      MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Update_Outage();
                MessageBox.Show("Change Committed");
                this.Close();
            }
        }

        #endregion

    }
}
