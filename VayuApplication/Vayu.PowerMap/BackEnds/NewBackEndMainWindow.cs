using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Vayu.LatestConstraintsInformationLibrary;
using Vayu.PowerMap.Controls;
using Vayu.PowerMap.HelperClasses;
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
        /// The node task
        /// </summary>
        private Task nodeTask;
        /// <summary>
        /// The enable events
        /// </summary>
        private bool EnableEvents;
        /// <summary>
        /// Gets or sets the current zone helper.
        /// </summary>
        /// <value>
        /// The current zone helper.
        /// </value>
        public ZoneHelper CurrentZoneHelper { get; set; }

        /// <summary>
        /// The m zone list
        /// </summary>
        private ObservableCollection<string> mZoneList;
        /// <summary>
        /// Gets or sets the zone list.
        /// </summary>
        /// <value>
        /// The zone list.
        /// </value>
        public ObservableCollection<string> ZoneList
        {
            get
            {
                return mZoneList;
            }
            set
            {
                mZoneList = value;
                RaisePropertyChanged("ZoneList");
            }
        }

        /// <summary>
        /// The custom zoom level
        /// </summary>
        private double customZoomLevel;
        /// <summary>
        /// Gets or sets the custom zoom level.
        /// </summary>
        /// <value>
        /// The custom zoom level.
        /// </value>
        public double CustomZoomLevel
        {
            get { return customZoomLevel; }
            set { customZoomLevel = value; RaisePropertyChanged("CustomZoomLevel"); }
        }

        /// <summary>
        /// The click location
        /// </summary>
        private Location clickLocation;
        /// <summary>
        /// Gets or sets the click location.
        /// </summary>
        /// <value>
        /// The click location.
        /// </value>
        public Location ClickLocation
        {
            get { return clickLocation; }
            set { clickLocation = value; RaisePropertyChanged("ClickLocation"); }
        }

        /// <summary>
        /// The center location
        /// </summary>
        private Location centerLocation;
        /// <summary>
        /// Gets or sets the center location.
        /// </summary>
        /// <value>
        /// The center location.
        /// </value>
        public Location CenterLocation
        {
            get { return centerLocation; }
            set { centerLocation = value; ClickLocation = value; RaisePropertyChanged("CenterLocation"); }
        }

        /// <summary>
        /// The map zoom
        /// </summary>
        private double mapZoom;
        /// <summary>
        /// Gets or sets the map zoom.
        /// </summary>
        /// <value>
        /// The map zoom.
        /// </value>
        public double MapZoom
        {
            get { return mapZoom; }
            set { mapZoom = value; RaisePropertyChanged("MapZoom"); if (CustomZoomLevel != value) CustomZoomLevel = value; }
        }

        /// <summary>
        /// The m node geo hash
        /// </summary>
        private ConcurrentDictionary<string, Node_Geo> mNodeGeoHash = new ConcurrentDictionary<string, Node_Geo>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            uptoTask = EnergyMapHelper.RunActionInThread(() => { FillUpto(); BuildOutageZoneHash(); });
            branchTask = EnergyMapHelper.RunActionInThread(() => { BuildBranchHash(); });
            nodeTask = EnergyMapHelper.RunActionInThread(() => { BuildNodeHash(); LoadSPPSettlementHash(); });

            InitializeComponent();
            CurrentZoneHelper = new ZoneHelper();

            loadDBCommands();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            InitReactiveEvents();

            sReconnectTimer = new System.Timers.Timer(3000);
            sHeartBeatTimer = new System.Timers.Timer(300000);
            sReconnectTimer.Elapsed += new ElapsedEventHandler(ReconnectToProxy);
            sHeartBeatTimer.Elapsed += new ElapsedEventHandler(HeartBeat);

            Loaded += MainWindow_Loaded;
            //Closing += (s, e) => ViewModelLocator.Cleanup();
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            Closed += MainWindow_Closed;

            //RunExportCSVCommand = new GalaSoft.MvvmLight.Command.RelayCommand(ExportToCSVCommand);
        }

        #region Private Methods

        /// <summary>
        /// Initializes the parameter.
        /// </summary>
        private void InitParameter()

        {
            if ((bool)MainCalendarSilder.IsRange)
            {
                startTime = MainCalendarSilder.from_date;
                endTime = MainCalendarSilder.to_date.AddDays(1);
                exactDate = MainCalendarSilder.exact_date;
                exactTime = MainCalendarSilder.exact_date.AddHours(MainCalendarSilder.HE);
                Temp_Avg_radioButton.IsEnabled = true;
                Temp_Max_radioButton.IsEnabled = true;
                Temp_Min_radioButton.IsEnabled = true;
            }
            else if (!(bool)MainCalendarSilder.IsRange)
            {
                exactTime = MainCalendarSilder.exact_date.AddHours(MainCalendarSilder.HE);
                startTime = exactTime;// MainCalendarSilder.exact_date;
                endTime = exactTime.AddHours(1);// MainCalendarSilder.exact_date.AddDays(1);
                exactDate = MainCalendarSilder.exact_date;
                Temp_Avg_radioButton.IsEnabled = false;
                Temp_Max_radioButton.IsEnabled = false;
                Temp_Min_radioButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Initializes the parameter.
        /// </summary>
        /// <param name="hour">The hour.</param>
        private void InitParameter(int hour)
        {
            if ((bool)MainCalendarSilder.IsRange)
            {
                startTime = MainCalendarSilder.from_date;
                endTime = MainCalendarSilder.to_date.AddDays(1);
                exactDate = MainCalendarSilder.exact_date;
                exactTime = MainCalendarSilder.exact_date.AddHours(hour);
                Temp_Avg_radioButton.IsEnabled = true;
                Temp_Max_radioButton.IsEnabled = true;
                Temp_Min_radioButton.IsEnabled = true;
            }
            else if (!(bool)MainCalendarSilder.IsRange)
            {
                exactTime = MainCalendarSilder.exact_date.AddHours(hour);
                startTime = exactTime;// MainCalendarSilder.exact_date;
                endTime = exactTime.AddHours(1);// MainCalendarSilder.exact_date.AddDays(1);
                exactDate = MainCalendarSilder.exact_date;
                Temp_Avg_radioButton.IsEnabled = false;
                Temp_Max_radioButton.IsEnabled = false;
                Temp_Min_radioButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Resets the GUI.
        /// </summary>
        private void ResetGui()
        {
            MapControl.myMap.Children.Remove(mNavigationMapLayer);
            MapControl.myMap.Children.Remove(mNavigationLMPLayer);
            MapControl.myMap.Children.Remove(mConstraintMapLayer);
            MapControl.myMap.Children.Remove(mOutageMapLayer);
            MapControl.myMap.Children.Remove(mLMPMapLayer);
            MapControl.myMap.Children.Remove(mOutageMapLayer);
            MapControl.myMap.Children.Remove(mConstraintMapLayer);
            MapControl.myMap.Children.Remove(mTransmissionMapLayer);
            MapControl.myMap.Children.Remove(mVirtualMapLayer);
            MapControl.myMap.Children.Remove(mWeatherMapLayer);
            MapControl.myMap.Children.Remove(mIIROutageMapLayer);

            mConstraintMapLayer.Children.Clear();
            mNavigationMapLayer.Children.Clear();
            mNavigationLMPLayer.Children.Clear();
            mOutageMapLayer.Children.Clear();
            mWeatherMapLayer.Children.Clear();
            mIIROutageMapLayer.Children.Clear();

            ZoneList = new ObservableCollection<string>();
            ConstraintDetailList = new ObservableCollection<Constraintobj>();
            TransmissionList = new ObservableCollection<BranchModel>();

            GeoConstraintList = new ObservableCollection<Constraintobj>();
            NonGeoConstraintList = new ObservableCollection<Constraintobj>();
            ConstraintEquipmentTypeList = new ObservableCollection<string>();

            LMPList = new ObservableCollection<point>();
            LmpEquipmentList = new ObservableCollection<string>();

            GeoOutageList = new ObservableCollection<Outage>();
            NonGeoOutageList = new ObservableCollection<Outage>();
            OutageEquipmentList = new ObservableCollection<string>();
        }

        /// <summary>
        /// Fills the upto.
        /// </summary>
        private void FillUpto()
        {
            SqlCommand mSelectSourceSinkNodeCommand = EnergyMapHelper.GetTradingDBCommand();
            mSelectSourceSinkNodeCommand.CommandText = " select src.SourceNodeName, sink.SinkNodeName from EESPathList src inner join Node n on n.NodeKey = src.SourceNodeKey " +
                " inner join EESPathList sink on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey inner join Node n2 on n2.NodeKey = sink.SinkNodeKey " +
                " where n.MarketKey = @MarketKey and n2.MarketKey = @MarketKey and src.MarketKey = @MarketKey and sink.MarketKey = @MarketKey order by n.NodeName ";
            mSelectSourceSinkNodeCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectSourceSinkNodeCommand.Connection.Open();
            mSelectSourceSinkNodeCommand.Parameters["@MarketKey"].Value = 1;
            SqlDataReader reader = mSelectSourceSinkNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                string str1 = reader[0].ToString();
                string str2 = reader[1].ToString();
                sourcesinkUpToList.Add(str1);
                sourcesinkUpToList.Add(str2);
            }
            mSelectSourceSinkNodeCommand.Connection.Close();
        }

        /// <summary>
        /// Gets the geo key.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        private string GetGeoKey(string nodeName, int marketKey)
        {
            string geoKey = string.Format("{0}!{1}", nodeName, marketKey);
            return geoKey;
        }

        /// <summary>
        /// Builds the node hash.
        /// </summary>
        private void BuildNodeHash()
        {
            try
            {
                SqlCommand mSelectStationImportGeoCommand = EnergyMapHelper.GetTradingDBCommand();
                //  mSelectStationImportGeoCommand.CommandText = "select n.NodeName, nt.Label , n.Longitude, n.Latitude, n.ExternalNodeId, n.MarketKey, n.NodeKey, " +
                // " Case When n.Zone is null Then 'NONE' When RTrim(LTrim(n.Zone)) = '' Then 'NONE' When RTrim(LTrim(n.Zone)) = 'N/A' Then 'NONE' Else n.Zone End Zone ," +
                // " Case When b.FuelSource is null then '' when b.FuelSource is not null then b.FuelSource End
                // from  Vayu..node n join Vayu..NodeTypeGeo nt on n.NodeTypeKey = nt.NodeTypeKey inner join ERCOT..ErcotNodeFuelSource b ON n.NodeKey=b.NodeKey where marketkey in (1,9) ";

                mSelectStationImportGeoCommand.CommandText = @"select n.NodeName, nt.Label , n.Longitude, n.Latitude, n.ExternalNodeId, n.MarketKey, n.NodeKey, 
                                                        Case When n.Zone is null Then 'NONE' When RTrim(LTrim(n.Zone)) = '' Then 'NONE' When RTrim(LTrim(n.Zone)) = 'N/A' Then 'NONE' Else n.Zone End Zone ,
                                                        Case When b.FuelSource is null then '' 
                                                        when b.FuelSource is not null then b.FuelSource End FuelType 
                                                        from Vayu..node n join Vayu..NodeTypeGeo nt on n.NodeTypeKey = nt.NodeTypeKey 
                                                        left join Vayu..ErcotNodeFuelSource b ON n.NodeKey=b.NodeKey where marketkey in (1,9)";

                if (mSelectStationImportGeoCommand.Connection.State == ConnectionState.Closed)
                {
                    mSelectStationImportGeoCommand.Connection.Open();
                }
                mNodeList = new List<Node_Geo>();
                SqlDataReader reader = mSelectStationImportGeoCommand.ExecuteReader();
                while (reader.Read())
                {
                    Node_Geo station = new Node_Geo();
                    int market = Configuration.GetInt(reader[5]);
                    station.NodeName = reader.GetString(0);
                    station.Type = reader.GetString(1);

                    station.FuelType = reader.GetString(8);

                    station.Longitude = Configuration.GetDoubleNaN(reader.GetValue(2));
                    station.Latitude = Configuration.GetDoubleNaN(reader.GetValue(3));

                    if (market == 1)
                        station.PnodeID = Configuration.GetInt(reader[4]);

                    station.Marketkey = market;
                    station.NodeKey = Configuration.GetNullInt(reader[6]);
                    station.Zone = reader[7].ToString();
                    mNodeList.Add(station);

                    string key = GetGeoKey(station.NodeName, market);
                    if (!mNodeGeoHash.ContainsKey(key))
                        mNodeGeoHash.TryAdd(key, station);
                }
                reader.Close();
                mSelectStationImportGeoCommand.Connection.Close();

                SqlCommand mSelectBusGeoCommand = EnergyMapHelper.GetTradingDBCommand();
                mSelectBusGeoCommand.CommandText = " select BusName,'',Longitude,Latitude,0,MarketKey,BusNum,Case When ZoneName is null " +
                " Then 'NONE' When RTrim(LTrim(ZoneName)) = '' Then 'NONE' When " +
                " RTrim(LTrim(ZoneName)) = 'N/A' Then 'NONE' Else ZoneName End Zone  from PSSEBusgeo where marketkey in (1,9) ";
                if (mSelectBusGeoCommand.Connection.State == ConnectionState.Closed)
                {
                    mSelectBusGeoCommand.Connection.Open();
                }

                reader = mSelectBusGeoCommand.ExecuteReader();
                while (reader.Read())
                {
                    Node_Geo station = new Node_Geo();
                    int market = Configuration.GetInt(reader[5]);
                    station.NodeName = reader.GetString(0);
                    station.Type = reader.GetString(1);
                    station.Longitude = Configuration.GetDoubleNaN(reader.GetValue(2));
                    station.Latitude = Configuration.GetDoubleNaN(reader.GetValue(3));

                    if (market == 1)
                        station.PnodeID = Configuration.GetInt(reader[4]);

                    station.Marketkey = market;
                    station.NodeKey = Configuration.GetNullInt(reader[6]);
                    station.Zone = reader[7].ToString();

                    string key = GetGeoKey(station.NodeName, market);
                    if (!mNodeGeoHash.ContainsKey(key))
                        mNodeGeoHash.TryAdd(key, station);
                }
                reader.Close();
                mSelectBusGeoCommand.Connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Loading Geo-Location Error: " + e.Message);
            }
        }

        /// <summary>
        /// Builds the branch hash.
        /// </summary>
        private void BuildBranchHash()
        {
            try
            {
                branchList = new List<BranchModel>();
                SqlCommand mSelectPSSEBranchCommad = EnergyMapHelper.GetTradingDBCommand();
                mSelectPSSEBranchCommad.CommandText =
                    "select BusNameFrom, LatitudeFrom, LongitudeFrom, BusNomVoltFrom, BusNameTo, LatitudeTo, LongitudeTo, BusNomVoltTo, BranchDeviceType, LineXfmr , MarketKey from pssebranchgeo";
                if (mSelectPSSEBranchCommad.Connection.State == ConnectionState.Closed)
                {
                    mSelectPSSEBranchCommad.Connection.Open();
                }
                mSelectPSSEBranchCommad.CommandTimeout = 30;
                SqlDataReader reader = mSelectPSSEBranchCommad.ExecuteReader();
                while (reader.Read())
                {
                    BranchModel br = new BranchModel();
                    br.branch = reader[0].ToString();
                    br.branch_location = new Location(Configuration.GetDouble(reader[1]), Configuration.GetDouble(reader[2]));
                    br.kv = Configuration.GetInt(reader[3]);
                    br.tobranch = reader[4].ToString();
                    br.tobranch_location = new Location(Configuration.GetDouble(reader[5]), Configuration.GetDouble(reader[6]));
                    br.tokv = Configuration.GetInt(reader[7]);
                    br.devicetype = reader[8].ToString();
                    br.marketkey = 1;
                    br.branchname = br.branch + "-" + br.tobranch;
                    branchList.Add(br);

                    int mktKey = Configuration.GetInt(reader[10]);
                    string key = GetGeoKey(br.branch, mktKey);
                    if (!mNodeGeoHash.ContainsKey(key))
                    {
                        Node_Geo station = new Node_Geo();
                        station.NodeName = br.branch;
                        station.Longitude = Configuration.GetDoubleNaN(reader.GetValue(2));
                        station.Latitude = Configuration.GetDoubleNaN(reader.GetValue(1));
                        station.Marketkey = mktKey;
                        mNodeGeoHash.TryAdd(key, station);
                    }

                    key = GetGeoKey(br.tobranch, mktKey);
                    if (!mNodeGeoHash.ContainsKey(key))
                    {
                        Node_Geo station = new Node_Geo();
                        station.NodeName = br.tobranch;
                        station.Longitude = Configuration.GetDoubleNaN(reader.GetValue(6));
                        station.Latitude = Configuration.GetDoubleNaN(reader.GetValue(5));
                        station.Marketkey = mktKey;
                        mNodeGeoHash.TryAdd(key, station);
                    }
                }
                reader.Close();
                reader.Dispose();

                SqlCommand mSelectYesBranchGeoCommand = new SqlCommand();
                mSelectYesBranchGeoCommand.Connection = mSelectPSSEBranchCommad.Connection;
                mSelectYesBranchGeoCommand.CommandText = "select BusNameFrom, LatitudeFrom,LongitudeFrom, ZoneFrom, BusNameTo, LatitudeTo, LongitudeTo, " +
                                                     "ZoneTo, Voltage, Type, MarketKey from Branchgeo where marketkey <> 1";
                mSelectYesBranchGeoCommand.CommandTimeout = 30;
                reader = mSelectYesBranchGeoCommand.ExecuteReader();
                while (reader.Read())
                {
                    BranchModel br = new BranchModel();
                    br.branch = reader.GetString(0);
                    br.branch_location = new Location(Convert.ToDouble(reader.GetValue(1)), Convert.ToDouble(reader.GetValue(2)));
                    br.branchzone = reader.GetString(3);
                    br.tobranch = reader.GetString(4);
                    br.tobranch_location = new Location(Convert.ToDouble(reader.GetValue(5)), Convert.ToDouble(reader.GetValue(6)));
                    br.tobranchzone = reader.GetString(7);
                    br.kv = Convert.ToInt16(reader.GetValue(8));
                    br.devicetype = reader.GetString(9);
                    br.marketkey = Convert.ToInt16(reader.GetValue(10));
                    br.branchname = br.branch + "-" + br.tobranch;
                    branchList.Add(br);

                    int mktKey = br.marketkey;
                    string key = GetGeoKey(br.branch, mktKey);
                    if (!mNodeGeoHash.ContainsKey(key))
                    {
                        Node_Geo station = new Node_Geo();
                        station.NodeName = br.branch;
                        station.Longitude = Configuration.GetDoubleNaN(reader.GetValue(2));
                        station.Latitude = Configuration.GetDoubleNaN(reader.GetValue(1));
                        station.Marketkey = mktKey;
                        mNodeGeoHash.TryAdd(key, station);
                    }

                    key = GetGeoKey(br.tobranch, mktKey);
                    if (!mNodeGeoHash.ContainsKey(key))
                    {
                        Node_Geo station = new Node_Geo();
                        station.NodeName = br.tobranch;
                        station.Longitude = Configuration.GetDoubleNaN(reader.GetValue(6));
                        station.Latitude = Configuration.GetDoubleNaN(reader.GetValue(5));
                        station.Marketkey = mktKey;
                        mNodeGeoHash.TryAdd(key, station);
                    }
                }
                reader.Close();
                mSelectPSSEBranchCommad.Connection.Close();
            }
            catch (Exception e)
            {
                // MessageBox.Show("Download Branch Error: " + e.Message);
            }
        }

        /// <summary>
        /// Finds the coordinate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private Location FindCoordinate(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string stationkey = GetGeoKey(name, mMarketInContext);

            if (mNodeGeoHash.ContainsKey(stationkey))
            {
                Location stationcoordinate = new Location(mNodeGeoHash[stationkey].Latitude, mNodeGeoHash[stationkey].Longitude);
                return stationcoordinate;
            }
            //else
            //{
            //    stationkey = mNodeGeoHash.Keys.FirstOrDefault(x => x.StartsWith(name, StringComparison.InvariantCultureIgnoreCase));
            //    if (!string.IsNullOrEmpty(stationkey) && mNodeGeoHash.ContainsKey(stationkey))
            //    {
            //        Location stationcoordinate = new Location(mNodeGeoHash[stationkey].Latitude, mNodeGeoHash[stationkey].Longitude);
            //        return stationcoordinate;
            //    }
            //}

            return null;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Loaded event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Task.WaitAll(nodeTask);
            InitParameter();
            ResetGui();
            LMPCheckBox.IsChecked = false;
            ConstraintCheckBox.IsChecked = false;
            OutageCheckBox.IsChecked = false;
            SelectedLMPPriceType = LMPPriceType.RT;
            SelectedOutagesSourceType = OutagesSourceType.Actual;
            OutagesStatusForced = true;
            OutagesStatusPlanned = true;
            OutagesIncludeComplete = false;
            SelectedOutagesScheduleType = OutagesScheduleType.Active;
            MarketlistBox.SelectedIndex = 0;
            //IIROutageCheckBox.IsChecked = true;
            Transmisson_MinKV_Textbox.Text = "345";
            ConnectToProxy();
            Initiate();
        }

        /// <summary>
        /// Mains the calendar silder property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void MainCalendarSilderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EnableEvents = true;

            if (mRefresh)
            {
                InitParameter();

                if (mMarketInContext == 0)
                    return;

                if (MainCalendarSilder.IsRange.GetValueOrDefault())
                {
                    LMPCheckBox.IsEnabled = false;
                    LMPCheckBox.IsChecked = false;
                }
                else
                    LMPCheckBox.IsEnabled = true;

                if (LMPCheckBox.IsChecked.GetValueOrDefault() && !MainCalendarSilder.IsRange.GetValueOrDefault())
                    LMPMain();
                else
                    mLMPMapLayer.Children.Clear();

                if (OutageCheckBox.IsChecked.GetValueOrDefault())
                    OutageMain();
                else
                    mOutageMapLayer.Children.Clear();

                if (ConstraintCheckBox.IsChecked.GetValueOrDefault())
                    FetchConstraintData();
                else
                    mConstraintMapLayer.Children.Clear();

                if (TransmissionCheckBox.IsChecked.GetValueOrDefault())
                    TransmissionMain();
                else
                    mTransmissionMapLayer.Children.Clear();

                if (TemperatureCheckBox.IsChecked.GetValueOrDefault())
                    TemperaturePreLoad();
                else
                    mWeatherMapLayer.Children.Clear();
                //if (IIROutageCheckBox.IsChecked.GetValueOrDefault())
                //    IIROutageMain();
                //else
                //    mIIROutageMapLayer.Children.Clear();

                CheckBox_Checked(null, null);
                SetPortfolio();
            }
        }

        /// <summary>
        /// Handles the Checked event of the CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!EnableEvents)
                return;

            if (MainCalendarSilder.IsRange.GetValueOrDefault() || !LMPCheckBox.IsChecked.GetValueOrDefault())
                MapControl.myMap.Children.Remove(mLMPMapLayer);
            else if (!MapControl.myMap.Children.Contains(mLMPMapLayer))
                MapControl.myMap.Children.Add(mLMPMapLayer);

            if (!OutageCheckBox.IsChecked.GetValueOrDefault())
                MapControl.myMap.Children.Remove(mOutageMapLayer);
            else if (!MapControl.myMap.Children.Contains(mOutageMapLayer))
                MapControl.myMap.Children.Add(mOutageMapLayer);

            if (!ConstraintCheckBox.IsChecked.GetValueOrDefault())
                MapControl.myMap.Children.Remove(mConstraintMapLayer);
            else if (!MapControl.myMap.Children.Contains(mConstraintMapLayer))
                MapControl.myMap.Children.Add(mConstraintMapLayer);

            if (!TransmissionCheckBox.IsChecked.GetValueOrDefault())
                MapControl.myMap.Children.Remove(mTransmissionMapLayer);
            else if (!MapControl.myMap.Children.Contains(mTransmissionMapLayer))
                MapControl.myMap.Children.Add(mTransmissionMapLayer);

            if (!TemperatureCheckBox.IsChecked.GetValueOrDefault())
                MapControl.myMap.Children.Remove(mWeatherMapLayer);
            else if (!MapControl.myMap.Children.Contains(mWeatherMapLayer))
                MapControl.myMap.Children.Insert(0, mWeatherMapLayer);

            //if (!IIROutageCheckBox.IsChecked.GetValueOrDefault())
            //    MapControl.myMap.Children.Remove(mIIROutageMapLayer);
            //else if (!MapControl.myMap.Children.Contains(mIIROutageMapLayer))
            //    MapControl.myMap.Children.Add(mIIROutageMapLayer);

            if (!ZoneCheckBox.IsChecked.GetValueOrDefault())
                MapControl.myMap.Children.Remove(mZoneMapLayer);
            else if (!MapControl.myMap.Children.Contains(mZoneMapLayer))
                MapControl.myMap.Children.Insert(0, mZoneMapLayer);
        }

        /// <summary>
        /// Handles the Checked event of the LiveCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LiveCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (LiveCheckBox.IsChecked.GetValueOrDefault())
                SubscribeToLiveData();
            else if (mNodeProxy != null)
                mNodeProxy.Unsubscribe();

            e.Handled = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the MarketlistBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void MarketlistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarketlistBox.SelectedItem != null)
            {
                EnableEvents = false;
                ResetGui();
                LiveCheckBox.IsChecked = false;
                mMarketInContext = mMarkets[MarketlistBox.SelectedItem.ToString()];

                if (mMarketInContext == 1)
                    LMPFilterToggle = "Upto Nodes";
                else
                    LMPFilterToggle = "Settlements";

                ZoneList = new ObservableCollection<string>(CurrentZoneHelper.GetZones(mMarketInContext));
                ZoneListBox.SelectAll();
                LoadLMPEquipments();
            }

            if (e != null)
                e.Handled = true;
            SetPortfolio();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the LMPEquipmentTypeListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void LMPEquipmentTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!EnableEvents)
                return;

            LMPMain();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ZoneListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ZoneListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!EnableEvents)
                return;

            LMPMain();

            if (e != null)
                e.Handled = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the OutageEquipmentTypeListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void OutageEquipmentTypeListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!EnableEvents)
                return;

            OutageMain();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ConstraintEquipmentTypeListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ConstraintEquipmentTypeListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!EnableEvents)
                return;

            ConstraintMain();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the ZoneUnSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ZoneUnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ZoneListBox.UnselectAll();
            e.Handled = true;
        }
        /// <summary>
        /// Handles the Click event of the ZoneSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ZoneSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ZoneListBox.SelectAll();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the LMPEquipmentTypeSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LMPEquipmentTypeSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            LMPEquipmentTypeListBox.SelectAll();
            e.Handled = true;
        }
        /// <summary>
        /// Handles the Click event of the LMPEquipmentTypeUnSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LMPEquipmentTypeUnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            LMPEquipmentTypeListBox.UnselectAll();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the OutageEquipmentTypeSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OutageEquipmentTypeSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            OutageEquipmentTypeListBox.SelectAll();
            e.Handled = true;
        }
        /// <summary>
        /// Handles the Click event of the OutageEquipmentTypeUnSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OutageEquipmentTypeUnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            OutageEquipmentTypeListBox.UnselectAll();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the ConstraintEquipmentTypeSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ConstraintEquipmentTypeSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ConstraintEquipmentTypeListBox.SelectAll();
            e.Handled = true;
        }
        /// <summary>
        /// Handles the Click event of the ConstraintEquipmentTypeUnSelectAllButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ConstraintEquipmentTypeUnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ConstraintEquipmentTypeListBox.UnselectAll();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the ZoomToLevel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ZoomToLevel_Click(object sender, RoutedEventArgs e)
        {
            FlyTo(ClickLocation, CustomZoomLevel);
        }
        #endregion

    }
}
