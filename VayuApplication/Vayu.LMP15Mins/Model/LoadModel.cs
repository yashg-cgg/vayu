using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.EnergyLMP;
using Vayu.NodePriceLibrary;

namespace Vayu.LMP15Mins.Model
{
    public class LoadModel : IDataService
    {
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection VayuConnection;

        #region SQL Commands

        /// <summary>
        /// The m select rt forecast command
        /// </summary>
        private SqlCommand mSelectRTForecastCommand;
        /// <summary>
        /// The m select misort forecast command
        /// </summary>
        private SqlCommand mSelectMISORTForecastCommand;
        /// <summary>
        /// The m select constraint command
        /// </summary>
        private SqlCommand mSelectConstraintCommand;
        /// <summary>
        /// The m select load forecast command
        /// </summary>
        private SqlCommand mSelectLoadForecastCommand;
        private SqlCommand mSelectErcotLoadForecastCommand;
        /// <summary>
        /// The m select zone loads command
        /// </summary>
        private SqlCommand mSelectZoneLoadsCommand;
        /// <summary>
        /// The m select miso zone loads command
        /// </summary>
        private SqlCommand mSelectMISOZoneLoadsCommand;
        /// <summary>
        /// The m select rt command
        /// </summary>
        private SqlCommand mSelectRTCommand;

        private SqlCommand mSelectErcotRTCommand;
        /// <summary>
        /// The m select zone loads rt command
        /// </summary>
        private SqlCommand mSelectZoneLoadsRTCommand;
        private SqlCommand mSelectErcotZoneLoadsRTCommand;
        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectRTForecastCommand = new SqlCommand();
            mSelectRTForecastCommand.CommandText = "select DatePart(hour,marketdateTime) hour, mw megawatt from loadrth where marketdatetime Between @START_DATE and @END_DATE " +
                                                "and loadskey = @loadskey order by hour";
            mSelectRTForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            mSelectRTForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            mSelectRTForecastCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            mSelectRTForecastCommand.Connection = VayuConnection;


            mSelectMISORTForecastCommand = new SqlCommand();
            mSelectMISORTForecastCommand.CommandText = "select DatePart(hour,marketdateTime) hour, mw megawatt from loadrt where marketdatetime Between @START_DATE and @END_DATE " +
                                               "and loadskey = @loadskey And datepart(minute, marketdatetime) = 0 order by hour";
            mSelectMISORTForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            mSelectMISORTForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            mSelectMISORTForecastCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            mSelectMISORTForecastCommand.Connection = VayuConnection;

            mSelectConstraintCommand = new SqlCommand();
            mSelectConstraintCommand.CommandText = "Select distinct constrainttext, contingencytext, IsNull(ShadowPrice,0) ShadowPrice from constraintrt " +
                                            "Where MarketKey = @MarketKey And marketdatetime >= @startDate And marketdatetime < @endDate " +
                                            "And Constrainttext <> 'none' ";
            mSelectConstraintCommand.Parameters.AddWithValue("@MarketKey", "marketkey");
            mSelectConstraintCommand.Parameters.AddWithValue("@StartDate", "marketdatetime");
            mSelectConstraintCommand.Parameters.AddWithValue("@EndDate", "marketdatetime");
            mSelectConstraintCommand.Connection = VayuConnection;

            mSelectLoadForecastCommand = new SqlCommand();
            mSelectLoadForecastCommand.CommandText = "select DatePart(hour,marketdateTime) hour, mw megawatt from loadforecasts where marketdatetime Between @START_DATE and @END_DATE " +
                                                "and LoadForecastTypekey = @loadskey and   DatePart(minute,marketdateTime)=0  order by hour";
            mSelectLoadForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            mSelectLoadForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            mSelectLoadForecastCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            mSelectLoadForecastCommand.Connection = VayuConnection;

            //
            mSelectErcotLoadForecastCommand = new SqlCommand();
            mSelectErcotLoadForecastCommand.CommandText = "select DatePart(hour,marketdateTime) hour, mw megawatt from Vayu..loadforecasts where marketdatetime Between @START_DATE and @END_DATE " +
                                                "and LoadForecastTypekey = @loadskey and   DatePart(minute,marketdateTime)=0  order by hour";
            mSelectErcotLoadForecastCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            mSelectErcotLoadForecastCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            mSelectErcotLoadForecastCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            mSelectErcotLoadForecastCommand.Connection = VayuConnection;
            //

            mSelectZoneLoadsCommand = new SqlCommand();
            mSelectZoneLoadsCommand.CommandText = "Select l.LoadsKey, LoadsName, MW From LoadRTH r Join Loads l On l.LoadsKey = r.LoadsKey Where marketdatetime = @START_DATE " +
                                                "and MarketKey = @MarketKey order by l.LoadsKey";
            mSelectZoneLoadsCommand.Parameters.AddWithValue("@START_DATE", "MarketDateTime");
            mSelectZoneLoadsCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectZoneLoadsCommand.Connection = VayuConnection;

            mSelectZoneLoadsRTCommand = new SqlCommand();
            mSelectZoneLoadsRTCommand.CommandText = "Select l.LoadsKey, LoadsName, MW From LoadRT r Join Loads l On l.LoadsKey = r.LoadsKey Where marketdatetime = @START_DATE " +
                                                "and MarketKey = @MarketKey order by l.LoadsKey";
            mSelectZoneLoadsRTCommand.Parameters.AddWithValue("@START_DATE", "MarketDateTime");
            mSelectZoneLoadsRTCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectZoneLoadsRTCommand.Connection = VayuConnection;

            //
            mSelectErcotZoneLoadsRTCommand = new SqlCommand();
            mSelectErcotZoneLoadsRTCommand.CommandText = "Select l.LoadsKey, LoadsName, MW From Vayu..LoadRT r Join Loads l On l.LoadsKey = r.LoadsKey Where marketdatetime = @START_DATE " +
                                                "and MarketKey = @MarketKey order by l.LoadsKey";
            mSelectErcotZoneLoadsRTCommand.Parameters.AddWithValue("@START_DATE", "MarketDateTime");
            mSelectErcotZoneLoadsRTCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectErcotZoneLoadsRTCommand.Connection = VayuConnection;
            //
            mSelectMISOZoneLoadsCommand = new SqlCommand();
            mSelectMISOZoneLoadsCommand.CommandText = "Select l.LoadsKey, LoadsName, MW From LoadRT r Join Loads l On l.LoadsKey = r.LoadsKey Where marketdatetime = @START_DATE " +
                                                "and MarketKey = @MarketKey order by l.LoadsKey";
            mSelectMISOZoneLoadsCommand.Parameters.AddWithValue("@START_DATE", "MarketDateTime");
            mSelectMISOZoneLoadsCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
            mSelectMISOZoneLoadsCommand.Connection = VayuConnection;

            mSelectRTCommand = new SqlCommand();
            mSelectRTCommand.CommandText = "select DatePart(hour,marketdateTime) hour, mw megawatt from loadrt where marketdatetime Between @START_DATE and @END_DATE " +
                                                "and loadskey = @loadskey and   DatePart(minute,marketdateTime)=0 ";
            mSelectRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            mSelectRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            mSelectRTCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            mSelectRTCommand.Connection = VayuConnection;

            mSelectErcotRTCommand = new SqlCommand();
            mSelectErcotRTCommand.CommandText = "select DatePart(hour,marketdateTime) hour, mw megawatt from Vayu..loadrt where marketdatetime Between @START_DATE and @END_DATE " +
                                                "and loadskey = @loadskey and   DatePart(minute,marketdateTime)=0 ";
            mSelectErcotRTCommand.Parameters.AddWithValue("@START_DATE", "MARKETDATETIME");
            mSelectErcotRTCommand.Parameters.AddWithValue("@END_DATE", "MARKETDATETIME");
            mSelectErcotRTCommand.Parameters.AddWithValue("@loadskey", "loadskey");
            mSelectErcotRTCommand.Connection = VayuConnection;
        }

        /// <summary>
        /// Gets the load graph data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="loadskey">The loadskey.</param>
        public void GetLoadGraphData(Action<List<Load>, Exception> callback, DateTime startDate, DateTime endDate, int marketKey, int? loadskey)
        {
            SqlDataReader reader = null;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            if (marketKey == 2)
            {
                mSelectMISORTForecastCommand.Parameters["@START_DATE"].Value = startDate;
                mSelectMISORTForecastCommand.Parameters["@END_DATE"].Value = endDate;
                mSelectMISORTForecastCommand.Parameters["@loadskey"].Value = loadskey;
                reader = mSelectMISORTForecastCommand.ExecuteReader();
            }
            else
            {
                if (marketKey == 1)
                {
                    mSelectRTForecastCommand.Parameters["@START_DATE"].Value = startDate;
                    mSelectRTForecastCommand.Parameters["@END_DATE"].Value = endDate;
                    if (loadskey == null)
                    {
                        mSelectRTForecastCommand.Parameters["@loadskey"].Value = -100;
                    }
                    else
                    {
                        mSelectRTForecastCommand.Parameters["@loadskey"].Value = loadskey;
                    }
                    reader = mSelectRTForecastCommand.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        mSelectRTCommand.Parameters["@START_DATE"].Value = startDate;
                        mSelectRTCommand.Parameters["@END_DATE"].Value = endDate;
                        if (loadskey == null)
                        {
                            mSelectRTCommand.Parameters["@loadskey"].Value = -100;
                        }
                        else
                        {
                            mSelectRTCommand.Parameters["@loadskey"].Value = loadskey;
                        }
                        reader = mSelectRTCommand.ExecuteReader();
                    }
                }
                else
                {
                    mSelectErcotRTCommand.Parameters["@START_DATE"].Value = startDate;
                    mSelectErcotRTCommand.Parameters["@END_DATE"].Value = endDate;
                    mSelectErcotRTCommand.Parameters["@loadskey"].Value = loadskey;// 2213;
                    reader = mSelectErcotRTCommand.ExecuteReader();
                }
            }

            List<Load> loaddata = new List<Load>();
            while (reader.Read())
            {
                Load data = new Load();
                data.Hour = int.Parse(reader[0].ToString());
                data.MegaWatts = double.Parse(reader[1].ToString());
                //if (data.MegaWatts != 0.00)
                loaddata.Add(data);
            }
            reader.Close();
            VayuConnection.Close();
            callback(loaddata, null);
        }
        /// <summary>
        /// Gets the load forecast graph data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="loadskey">The loadskey.</param>
        public void GetLoadForecastGraphData(Action<List<Load>, Exception> callback, DateTime startDate, DateTime endDate, int marketKey, int? loadskey)
        {
            try
            {
                SqlDataReader reader = null;
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();
                if (marketKey == 1)
                {
                    mSelectLoadForecastCommand.Parameters["@START_DATE"].Value = startDate;
                    mSelectLoadForecastCommand.Parameters["@END_DATE"].Value = endDate;
                    if (loadskey == null)
                    {
                        mSelectLoadForecastCommand.Parameters["@loadskey"].Value = -100;
                    }
                    else
                    {
                        mSelectLoadForecastCommand.Parameters["@loadskey"].Value = loadskey;
                    }
                    reader = mSelectLoadForecastCommand.ExecuteReader();
                }
                else
                {
                    mSelectErcotLoadForecastCommand.Parameters["@START_DATE"].Value = startDate;
                    mSelectErcotLoadForecastCommand.Parameters["@END_DATE"].Value = endDate;
                    mSelectErcotLoadForecastCommand.Parameters["@loadskey"].Value = loadskey;
                    reader = mSelectErcotLoadForecastCommand.ExecuteReader();
                }
                List<Load> loaddata = new List<Load>();
                while (reader.Read())
                {
                    Load data = new Load();
                    data.Hour = int.Parse(reader[0].ToString());
                    data.MegaWatts = double.Parse(reader[1].ToString());
                    loaddata.Add(data);
                }
                reader.Close();
                VayuConnection.Close();
                callback(loaddata, null);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Gets the constraint contingency data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="marketKey">The market key.</param>
        public void GetLMP15MinDataa(Action<List<LMP15MinSourceSink>, Exception> callback, DateTime startDate, DateTime endDate, int marketKey, SourceSinkData SourceSink)
        {
            List<LMP15MinSourceSink> ccdata = new List<LMP15MinSourceSink>();
            SqlCommand cmd = new SqlCommand();

            try
            {


                if (marketKey == 9)
                {
                    cmd.CommandText = "select so.LMP, si.LMP, si.LMP-so.LMP, so.MarketDateTime from Vayu..NodeLMP so inner join Vayu..NodeLMP si "
                                        + " on so.MarketDateTime = si.MarketDateTime where "
                                        + " so.NodeKey = " + SourceSink.Source.NodeKey + " and si.NodeKey = " + SourceSink.Sink.NodeKey + " and so.MarketDateTime >='" + startDate + "' and so.MarketDateTime <='" + endDate + "'";
                    //cmd.CommandText = "Select distinct constrainttext, contingencytext, IsNull(ShadowPrice,0) ShadowPrice from Vayu.. ConstraintRT " +
                    //                            "Where marketdatetime >= @startDate And marketdatetime < @endDate " +
                    //                            "And Constrainttext <> 'none' ";
                }

                //cmd.Parameters.AddWithValue("@StartDate", "marketdatetime");
                //cmd.Parameters.AddWithValue("@EndDate", "marketdatetime");
                //cmd.Parameters["@startDate"].Value = startDate;
                //cmd.Parameters["@endDate"].Value = endDate;
                cmd.Connection = VayuConnection;

                SqlDataReader reader;
                if (VayuConnection.State != ConnectionState.Open)
                    VayuConnection.Open();

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    LMP15MinSourceSink data = new LMP15MinSourceSink();
                    data.Source = SourceSink.Source.NodeName;
                    data.Sink = SourceSink.Sink.NodeName;
                    data.Source15minsLMP = Math.Round(double.Parse(reader[0].ToString()), 2);
                    data.Sink15minsLMP = Math.Round(double.Parse(reader[1].ToString()), 2);
                    data.Sink_Source = Math.Round(double.Parse(reader[2].ToString()), 2);
                    data.MarketDateTime = Convert.ToDateTime(reader[3]);
                    // data.ShadowPrice = double.Parse(reader[2].ToString());
                    ccdata.Add(data);
                }
                reader.Close();

                if (VayuConnection.State != ConnectionState.Closed)
                    VayuConnection.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                callback(ccdata, null);
            }
        }

        /// <summary>
        /// Gets the source sink hourly prices.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="Hour">The hour.</param>
        /// <param name="SourceSink">The source sink.</param>
        public void GetSourceSinkHourlyPrices(Action<List<Node>, List<Node>, Exception> callback, DateTime StartDate, int Hour, SourceSinkData SourceSink)
        {
            List<Node> rtNodeList = new List<Node>();
            List<Node> daNodeList = new List<Node>();
            Node sourceRtNode = new Node();
            sourceRtNode.Market = SourceSink.Source.MarketKey;
            sourceRtNode.NodeId = SourceSink.Source.NodeKey;
            sourceRtNode.NodeName = SourceSink.Source.NodeName;
            sourceRtNode.PNodeId = SourceSink.Source.ExternalNodeId;
            List<LmpTimePrice> rtSourceTimePriceList = new List<LmpTimePrice>();
            LmpTimePrice rtSourceTimePrice = new LmpTimePrice();
            rtSourceTimePrice.MarketTime = Hour == 24 ? StartDate.AddDays(1) : StartDate.AddHours(Hour);
            rtSourceTimePriceList.Add(rtSourceTimePrice);
            sourceRtNode.LmpTimePriceList = rtSourceTimePriceList;
            rtNodeList.Add(sourceRtNode);
            if (SourceSink.Sink != null)
            {
                Node sinkRtNode = new Node();
                sinkRtNode.Market = SourceSink.Sink.MarketKey;
                sinkRtNode.NodeId = SourceSink.Sink.NodeKey;
                sinkRtNode.NodeName = SourceSink.Sink.NodeName;
                sinkRtNode.PNodeId = SourceSink.Sink.ExternalNodeId;
                List<LmpTimePrice> rtSinkTimePriceList = new List<LmpTimePrice>();
                LmpTimePrice rtSinkTimePrice = new LmpTimePrice();
                rtSinkTimePrice.MarketTime = Hour == 24 ? StartDate.AddDays(1) : StartDate.AddHours(Hour);
                rtSinkTimePriceList.Add(rtSinkTimePrice);
                sinkRtNode.LmpTimePriceList = rtSinkTimePriceList;
                rtNodeList.Add(sinkRtNode);
            }
            Node sourceDaNode = new Node();
            sourceDaNode.Market = SourceSink.Source.MarketKey;
            sourceDaNode.NodeId = SourceSink.Source.NodeKey;
            sourceDaNode.NodeName = SourceSink.Source.NodeName;
            sourceDaNode.PNodeId = SourceSink.Source.ExternalNodeId;
            List<LmpTimePrice> daSourceTimePriceList = new List<LmpTimePrice>();
            LmpTimePrice daSourceTimePrice = new LmpTimePrice();
            daSourceTimePrice.MarketTime = Hour == 24 ? StartDate.AddDays(1) : StartDate.AddHours(Hour);
            daSourceTimePriceList.Add(daSourceTimePrice);
            sourceDaNode.LmpTimePriceList = daSourceTimePriceList;
            daNodeList.Add(sourceDaNode);
            if (SourceSink.Sink != null)
            {
                Node sinkDaNode = new Node();
                sinkDaNode.Market = SourceSink.Sink.MarketKey;
                sinkDaNode.NodeId = SourceSink.Sink.NodeKey;
                sinkDaNode.NodeName = SourceSink.Sink.NodeName;
                sinkDaNode.PNodeId = SourceSink.Sink.ExternalNodeId;
                List<LmpTimePrice> daSinkTimePriceList = new List<LmpTimePrice>();
                LmpTimePrice daSinkTimePrice = new LmpTimePrice();
                daSinkTimePrice.MarketTime = Hour == 24 ? StartDate.AddDays(1) : StartDate.AddHours(Hour);
                daSinkTimePriceList.Add(daSinkTimePrice);
                sinkDaNode.LmpTimePriceList = daSinkTimePriceList;
                daNodeList.Add(sinkDaNode);
            }
            DARTNode.GetDARTForHour(rtNodeList, daNodeList);
            callback(rtNodeList, daNodeList, null);
        }

        /// <summary>
        /// Gets the hourly zone loads.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="marketKey">The market key.</param>
        public void GetHourlyZoneLoads(Action<List<ZoneLoads>, Exception> callback, DateTime StartDate, int marketKey)
        {
            SqlDataReader reader = null;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<ZoneLoads> zoneLoadData = new List<ZoneLoads>();

            if (marketKey == 2)
            {
                mSelectMISOZoneLoadsCommand.Parameters["@START_DATE"].Value = StartDate;
                mSelectMISOZoneLoadsCommand.Parameters["@MarketKey"].Value = marketKey;

                reader = mSelectMISOZoneLoadsCommand.ExecuteReader();

                string type;
                while (reader.Read())
                {
                    ZoneLoads data = new ZoneLoads();
                    data.ZoneLoadKey = int.Parse(reader[0].ToString());
                    type = reader[1].ToString();
                    if (type.Contains("Zone"))
                    {
                        data.ZoneName = type.TrimEnd('Z', 'o', 'n', 'e');
                        data.ZoneType = "Zone";
                    }
                    else if (type.Contains("Region"))
                    {
                        data.ZoneName = type.TrimEnd('R', 'e', 'g', 'i', 'o', 'n');
                        data.ZoneType = "Region";
                    }
                    else
                    {
                        data.ZoneName = type;
                        data.ZoneType = "";
                    }
                    data.MegaWatts = double.Parse(reader[2].ToString());
                    zoneLoadData.Add(data);

                }
                reader.Close();
            }
            else
            {
                try
                {
                    if (marketKey == 1)
                    {
                        mSelectZoneLoadsCommand.Parameters["@START_DATE"].Value = StartDate;
                        mSelectZoneLoadsCommand.Parameters["@MarketKey"].Value = marketKey;
                        reader = mSelectZoneLoadsCommand.ExecuteReader();
                        if (!reader.HasRows)
                        {
                            mSelectZoneLoadsRTCommand.Parameters["@START_DATE"].Value = StartDate;
                            mSelectZoneLoadsRTCommand.Parameters["@MarketKey"].Value = marketKey;
                            reader = mSelectZoneLoadsRTCommand.ExecuteReader();
                        }
                    }
                    else
                    {
                        mSelectErcotZoneLoadsRTCommand.Parameters["@START_DATE"].Value = StartDate;
                        mSelectErcotZoneLoadsRTCommand.Parameters["@MarketKey"].Value = marketKey;
                        reader = mSelectErcotZoneLoadsRTCommand.ExecuteReader();
                    }
                    string type;
                    while (reader.Read())
                    {
                        ZoneLoads data = new ZoneLoads();
                        data.ZoneLoadKey = int.Parse(reader[0].ToString());
                        type = reader[1].ToString();
                        if (type.Contains("Zone"))
                        {
                            data.ZoneName = type.TrimEnd('Z', 'o', 'n', 'e');
                            data.ZoneType = "Zone";
                        }
                        else if (type.Contains("Region"))
                        {
                            data.ZoneName = type.TrimEnd('R', 'e', 'g', 'i', 'o', 'n');
                            data.ZoneType = "Region";
                        }
                        else
                        {
                            data.ZoneName = type;
                            data.ZoneType = "";
                        }
                        data.MegaWatts = double.Parse(reader[2].ToString());
                        zoneLoadData.Add(data);

                    }
                    reader.Close();
                }
                catch
                {

                }
            }

            VayuConnection.Close();
            callback(zoneLoadData, null);
        }

        #endregion
    }
}
