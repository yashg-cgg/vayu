
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// Interaction logic for MappingDialogConstraint.xaml
    /// </summary>
    public partial class MappingDialogConstraint : Window, INotifyPropertyChanged
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
        /// Gets or sets the m update constraint rt geo bus from and bus to.
        /// </summary>
        /// <value>
        /// The m update constraint rt geo bus from and bus to.
        /// </value>
        private SqlCommand mUpdateConstraintRTGeoBusFromAndBusTo { get; set; }
        /// <summary>
        /// Gets or sets the m psse bus hash.
        /// </summary>
        /// <value>
        /// The m psse bus hash.
        /// </value>
        private Dictionary<string, PSSEBus> mPSSEBusHash { get; set; }
        /// <summary>
        /// The m constraint
        /// </summary>
        private Constraintobj mConstraint;
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public Constraintobj Constraint
        {
            get
            {
                return mConstraint;
            }
            set
            {
                mConstraint = value;
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
            get
            {
                return _mylayer;
            }
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
        /// The m type list
        /// </summary>
        private List<string> mTypeList;
        /// <summary>
        /// Gets or sets the type list.
        /// </summary>
        /// <value>
        /// The type list.
        /// </value>
        public List<string> TypeList
        {
            get { return mTypeList; }
            set
            {
                mTypeList = value;
                RaisePropertyChanged("TypeList");
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingDialogConstraint"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        public MappingDialogConstraint(object obj)
        {
            InitializeComponent();
            IntiDB();
            LoadData(obj);
            MapSetup();
            Source_listBox.SelectionChanged += new SelectionChangedEventHandler(Source_listBox_SelectionChanged);
            Sink_listBox.SelectionChanged += new SelectionChangedEventHandler(Sink_listBox_SelectionChanged);
            Type_listBox.SelectionChanged += new SelectionChangedEventHandler(Type_listBox_SelectionChanged);
        }

        #region Public Methods

        /// <summary>
        /// Maps the setup.
        /// </summary>
        public void MapSetup()
        {
            sourcepin.Background = Brushes.MediumPurple;
            sourcepin.Background = Brushes.SpringGreen;
            line.Stroke = Brushes.Red;
            line.StrokeThickness = 4.0;
            string key = Constraint.constraint_stationFrom + " " + Constraint.constraint_KV;
            if (mPSSEBusHash.ContainsKey(key))
            {
                PSSEBus selectedbus = mPSSEBusHash[key];
                sourcepin.Location = new Location(selectedbus.latitude, selectedbus.longitude);
            }
            ToolTip tt = new ToolTip();
            tt.Content = Constraint.constraint_stationFrom;
            sourcepin.ToolTip = tt;
            MapControl1.myMap.Children.Add(sourcepin);
            MapControl1.myMap.Children.Add(sinkpin);
            MapControl1.myMap.Children.Add(line);
        }

        /// <summary>
        /// Intis the database.
        /// </summary>
        public void IntiDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
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
            mSelect_Station_Geo.Connection = VayuConnection;
            mSelect_Station_Geo.CommandTimeout = 30;
            //
            mSelect_PSSE_Coordinates = new SqlCommand();
            mSelect_PSSE_Coordinates.CommandText =
            "select NodeKey, NodeName, Longitude,Latitude,0 as BusNomVolt from Node where MarketKey = @marketkey order by NodeName";
            mSelect_PSSE_Coordinates.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelect_PSSE_Coordinates.Connection = VayuConnection;
            mSelect_PSSE_Coordinates.CommandTimeout = 30;
            //
            mSelect_Zone = new SqlCommand();
            mSelect_Zone.CommandText = "select distinct(Zone) as Zone from PJM_rt_outages";
            mSelect_Zone.Connection = VayuConnection;
            //
            mUpdateConstraintRTGeoBusFromAndBusTo = new SqlCommand();
            mUpdateConstraintRTGeoBusFromAndBusTo.CommandText = "Update ConstraintRTGeo " +
                                           "set ConstraintBusNameFrom = @ConstraintBusNameFrom, ConstraintBusNumFrom = @ConstraintBusNumFrom, ConstraintBusNameTo = @ConstraintBusNameTo, " +
                                           "ConstraintKV = @ConstraintKV, ConstraintBusNumTo = @ConstraintBusNumTo, type = @Type, Editor = @editor, EditTime = @edittime, Comment = @comment " +
                                           "where ConstraintText = @ConstraintText and ContingencyText = @ContingencyText ";
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@ConstraintText", "ConstraintText");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@ConstraintKV", "ConstraintKV");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@ConstraintBusNameFrom", "ConstraintBusNameFrom");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@ConstraintBusNumFrom", "ConstraintBusNumFrom");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@ConstraintBusNameTo", "ConstraintBusNameTo");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@ConstraintBusNumTo", "ConstraintBusNumTo");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@Type", "Type");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@Editor", "Editor");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@EditTime", "EditTime");
            mUpdateConstraintRTGeoBusFromAndBusTo.Parameters.AddWithValue("@Comment", "Comment");
            mUpdateConstraintRTGeoBusFromAndBusTo.Connection = VayuConnection;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void LoadData(object obj)
        {
            if (obj == null)
            {
                return;
            }
            DataTable busgeo = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            mSelect_PSSE_Coordinates.Parameters["@marketkey"].Value = 1;
            dataAdapter.SelectCommand = mSelect_PSSE_Coordinates;
            dataAdapter.Fill(busgeo);
            mPSSEBusHash = new Dictionary<string, PSSEBus>();
            List<string> keyList = new List<string>();
            foreach (DataRow row in busgeo.Rows)
            {
                PSSEBus bus = new PSSEBus();
                string name = row["NodeName"].ToString();
                string kv = string.Empty;
                if (row["BusNomVolt"] != DBNull.Value)
                {
                    kv = Convert.ToDouble(row["BusNomVolt"]).ToString();
                }

                if (kv.StartsWith("34"))
                {
                }
                int busNum = Convert.ToInt32(row["NodeKey"]);
                bus.name = name;
                //bus.kV = kv;
                bus.latitude = Convert.ToDouble(row["Latitude"]);
                bus.longitude = Convert.ToDouble(row["Longitude"]);
                bus.busNum = busNum;
                string key = name + " " + kv;
                if (!mPSSEBusHash.ContainsKey(key))
                {
                    mPSSEBusHash.Add(key, bus);
                }
                if (!keyList.Contains(key))
                {
                    keyList.Add(key);
                }
            }
            DataView busview = busgeo.DefaultView;
            switch (obj.GetType().Name.ToUpper())
            {
                case "CONSTRAINTOBJ":
                    Constraint = (Constraintobj)obj;
                    Constraint_DataGrid.Items.Add(Constraint);
                    originalConstraintText.Text = Constraint.constraint;
                    originalContingencyText.Text = Constraint.contingency;
                    break;
            }
            KeyList = keyList;
            TypeList = new List<string>() { "XFORMER", "LINE", "GC" };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Drawlines this instance.
        /// </summary>
        private void Drawline()
        {
            if (sourcepin.Location == null || sinkpin.Location == null) return;
            if (sinkpin.Visibility == Visibility.Visible)
            {
                LocationCollection coll = new LocationCollection();
                coll.Add(sourcepin.Location);
                coll.Add(sinkpin.Location);
                line.Locations = coll;
                line.Visibility = Visibility.Visible;
                Location zlocation = new Location((sourcepin.Location.Latitude + sinkpin.Location.Latitude) / 2,
                    (sourcepin.Location.Longitude + sinkpin.Location.Longitude) / 2);
                MapControl1.myMap.Center = zlocation;
                double approxdistance = Math.Sqrt(Math.Pow(sourcepin.Location.Latitude - sinkpin.Location.Latitude, 2) +
                                                  Math.Pow(sourcepin.Location.Longitude - sinkpin.Location.Longitude, 2));
                MapControl1.myMap.ZoomLevel = 9 - approxdistance / 2;
            }
            else if (sourcepin.Visibility == Visibility.Visible)
            {
                MapControl1.myMap.Center = sourcepin.Location;
                MapControl1.myMap.ZoomLevel = 9;
            }
            else if (sinkpin.Visibility == Visibility.Visible)
            {
                MapControl1.myMap.Center = sinkpin.Location;
                MapControl1.myMap.ZoomLevel = 9;
            }
        }

        /// <summary>
        /// Updates the outage.
        /// </summary>
        /// <returns></returns>
        private Int32 UpdateOutage()
        {
            string stationFromKey = Constraint.constraint_stationFrom + " " + Constraint.constraint_KV;
            string stationToKey = Constraint.constraint_stationTo + " " + Constraint.constraint_KV;
            int result = 0;
            try
            {
                VayuConnection.Open();
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@ConstraintText"].Value = Constraint.constraint;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@ContingencyText"].Value = Constraint.contingency;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@ConstraintKV"].Value = Constraint.constraint_KV;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@ConstraintBusNameFrom"].Value = Constraint.constraint_stationFrom;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@ConstraintBusNumFrom"].Value = mPSSEBusHash[stationFromKey].busNum;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@ConstraintBusNameTo"].Value = Constraint.constraint_stationTo;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@ConstraintBusNumTo"].Value = mPSSEBusHash[stationToKey].busNum;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@Type"].Value = Constraint.type;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@Editor"].Value = Environment.UserName.ToUpper();
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@EditTime"].Value = DateTime.Now;
                mUpdateConstraintRTGeoBusFromAndBusTo.Parameters["@Comment"].Value = Comment;
                result = mUpdateConstraintRTGeoBusFromAndBusTo.ExecuteNonQuery();
                VayuConnection.Close();
            }
            catch (Exception ex)
            {
                if (VayuConnection.State == ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
                result = -1;
            }
            return result;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the Source_listBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Source_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string item = Source_listBox.SelectedItem.ToString();
            PSSEBus selectedBus = mPSSEBusHash[item];
            ToolTip tt = new ToolTip();
            tt.Content = item;
            sourcepin.Location = new Location(selectedBus.latitude, selectedBus.longitude);
            sourcepin.ToolTip = tt;
            sourcepin.Visibility = Visibility.Visible;
            Constraint.constraint_stationFrom = selectedBus.name;
            //double testDouble;
            //if (double.TryParse(selectedBus.kV, out testDouble))
            //{
            //    Constraint.constraint_KV = testDouble;
            //}
            Drawline();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Sink_listBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Sink_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string item = Sink_listBox.SelectedItem.ToString();
            PSSEBus selectedbus = mPSSEBusHash[item];
            ToolTip tt = new ToolTip();
            tt.Content = item;
            sinkpin.Location = new Location(selectedbus.latitude, selectedbus.longitude);
            sinkpin.ToolTip = tt;
            sinkpin.Visibility = Visibility.Visible;
            Constraint.constraint_stationTo = selectedbus.name;
            Drawline();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Type_listBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Type_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string item = Type_listBox.SelectedItem.ToString();
            Constraint.type = item;
        }

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Format("Constraint StationFrom: {0}\nConstraint StationTo: {1}\nZone: {2}\nVoltage: {3}\nEquipment: {4}", Constraint.constraint_stationFrom,
                                           Constraint.constraint_stationTo, "", Constraint.constraint_KV, Constraint.type);
            MessageBoxResult result = MessageBox.Show("The changes you are about to make: \n" + message, "ComfirmDialog",
                                                      MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Int32 resultCount = UpdateOutage();
                if (resultCount != -1)
                {
                    MessageBox.Show("Change Committed, rows changed: " + resultCount.ToString());
                    this.Close();  // cj todo: restore for production
                }
                else // exception
                {
                    MessageBox.Show("Change not committed; there was an exception saving to database.");
                }
            }
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
    }
}
