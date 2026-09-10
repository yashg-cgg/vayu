using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.MarketView.Model
{

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.MarketViewNameSpace.Model.IDataService" />
    public class DataService : IDataService
    {

        object lockObj = new object();
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection VayuConnection;
        private SqlCommand mSelectErcotDeenergizedNodesCmd;

        private Dictionary<string, Dictionary<DateTime, double>> mAvgHash = null;
        public Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
        //private Dictionary<int, decimal> NodeDACongHash = new Dictionary<int, decimal>();

        #region SQL Commands

        /// <summary>
        /// The m select node zone command
        /// </summary>
        private SqlCommand mSelectNodeZoneCommand;
        private SqlCommand mSelectErcotNodeZoneCommand;
        /// <summary>
        /// The m select SPP settlement locations command
        /// </summary>
        private SqlCommand mSelectSPPSettlementLocationsCommand;
        /// <summary>
        /// The m select settlement locations command
        /// </summary>
        private SqlCommand mSelectSettlementLocationsCommand;
        private SqlCommand mSelectercotSettlementLocationsCommand;
        private SqlCommand mSelectDACongErcotNodeCommand;
        /// <summary>
        /// The s select node command
        /// </summary>
        private static SqlCommand sSelectNodeCommand;
        /// <summary>
        /// The s select node command dictionary
        /// </summary>
        private static SqlCommand sSelectNodeCommandDict;

        #endregion

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        private void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectSettlementLocationsCommand = VayuConnection.CreateCommand();
            mSelectSettlementLocationsCommand.CommandText = "select nodekey from node where marketkey = @marketkey order by NodeKey";
            mSelectSettlementLocationsCommand.Parameters.AddWithValue("@marketkey", "");

            //
            mSelectercotSettlementLocationsCommand = VayuConnection.CreateCommand();
            mSelectercotSettlementLocationsCommand.CommandText = "select nodekey from  Vayu..node where marketkey = @marketkey order by NodeKey";
            mSelectercotSettlementLocationsCommand.Parameters.AddWithValue("@marketkey", "");
            //

            mSelectNodeZoneCommand = VayuConnection.CreateCommand();
            mSelectNodeZoneCommand.CommandText = "select nodename,zone from node where marketkey = @marketkey";
            mSelectNodeZoneCommand.Parameters.AddWithValue("@marketkey", "");

            mSelectErcotNodeZoneCommand = VayuConnection.CreateCommand();
            mSelectErcotNodeZoneCommand.CommandText = "select nodename,zone from Vayu..node where marketkey = @marketkey";
            mSelectErcotNodeZoneCommand.Parameters.AddWithValue("@marketkey", "");

            sSelectNodeCommand = VayuConnection.CreateCommand();
            sSelectNodeCommand.CommandText = "select zone from node where nodekey=@nodekey";
            sSelectNodeCommand.Parameters.AddWithValue("@nodekey", "");


            //mSelectDACongErcotNodeCommand = VayuDbConn.CreateCommand();
            //mSelectDACongErcotNodeCommand.CommandText = " select NodeKey, Congestion from NodeDALMPH where MarketDateTime = '@MarketDateTime' order by NodeKey";

            //mSelectDACongErcotNodeCommand.Parameters.AddWithValue("@MarketDateTime", "");
        }



        /// <summary>
        /// Gets the settlement locations.
        /// </summary>
        /// <param name="marketID">The market identifier.</param>
        /// <returns></returns>
        public Hashtable GetSettlementLocations(int marketID)
        {
            #region Commented by SATEESH 03/29/2024
            //Hashtable SettlmntLocationList = new Hashtable();
            //loadDBCommands();
            //if (VayuConnection.State == ConnectionState.Closed)
            //    VayuConnection.Open();
            //try
            //{ 
            //    SqlCommand command = new SqlCommand();

            //    using (VayuConnection)
            //    {

            //        command = mSelectercotSettlementLocationsCommand;
            //        command.Parameters["@marketkey"].Value = marketID;
            //        command.Connection = VayuConnection;
            //        if (VayuConnection.State == ConnectionState.Closed)
            //            VayuConnection.Open();
            //        using (command)
            //        using (IDataReader reader = command.ExecuteReader())
            //        {
            //            while (reader.Read())
            //                SettlmntLocationList[reader.IsDBNull(0) ? -1 : Convert.ToInt32(reader.GetValue(0))] = "";
            //        }

            //    }

            //    return SettlmntLocationList;
            //}
            //catch (Exception ex)
            //{
            //    if (VayuConnection.State == ConnectionState.Open)
            //        VayuConnection.Close();

            //}

            //return SettlmntLocationList;
            #endregion
            Hashtable SettlmntLocationList = new Hashtable();

            try
            {
                // Load database commands
                loadDBCommands();

                // Open connection if it's closed
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                // Execute SQL command to fetch settlement locations
                using (SqlCommand command = mSelectercotSettlementLocationsCommand)
                {
                    command.Parameters["@marketkey"].Value = marketID;
                    command.Connection = VayuConnection;

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Populate the Hashtable with settlement locations
                            SettlmntLocationList[reader.IsDBNull(0) ? -1 : Convert.ToInt32(reader.GetValue(0))] = "";
                        }
                    }
                }

                return SettlmntLocationList;
            }
            catch (Exception ex)
            {
                // Handle exception and close connection if it's open
                if (VayuConnection.State == ConnectionState.Open)
                    VayuConnection.Close();
                return SettlmntLocationList;
            }
        }

        /// <summary>
        /// Gets the node zone.
        /// </summary>
        /// <param name="marketID">The market identifier.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetNodeZone(int marketID)
        {
            loadDBCommands();
            Dictionary<string, string> GetNodeZoneList = new Dictionary<string, string>();
            SqlCommand command = null;
            try
            {

                command = mSelectErcotNodeZoneCommand;
                command.Parameters["@marketkey"].Value = marketID;
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    if (!GetNodeZoneList.ContainsKey(name))
                        GetNodeZoneList.Add(name, reader.IsDBNull(1) ? "" : reader.GetString(1));
                }
                if (!reader.IsClosed)
                    reader.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                //if (command.Connection.State == ConnectionState.Open)
                //    command.Connection.Close();
            }

            return GetNodeZoneList;
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <param name="externalId">The external identifier.</param>
        /// <returns></returns>
        public LatestConstraint GetNode(int externalId)
        {
            LatestConstraint node = new LatestConstraint();
            try
            {
                int key = externalId;
                loadDBCommands();
                if (externalId != 0)
                {
                    bool close = false;
                    if (VayuConnection.State == System.Data.ConnectionState.Closed)
                    {
                        //close = true;
                        VayuConnection.Open();
                    }
                    SqlCommand command = null;
                    command = sSelectNodeCommand;
                    command.Parameters["@nodekey"].Value = externalId;
                    SqlDataReader reader = sSelectNodeCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        node.SourceZone = reader.IsDBNull(0) ? null : reader.GetString(0);
                    }
                    return node;
                }
            }
            catch (Exception ex)
            {
                if (VayuConnection.State == System.Data.ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
            }
            finally
            {
                if (VayuConnection.State == System.Data.ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
            }
            return node;

        }
        //public LatestConstraint GetNode(int externalId)
        //{
        //    LatestConstraint node = new LatestConstraint();

        //    try
        //    {
        //        if (externalId == 0)
        //            return node;

        //        loadDBCommands();

        //        if (sSelectNodeCommand == null)
        //            throw new NullReferenceException("sSelectNodeCommand is not initialized.");

        //        if (VayuConnection.State == System.Data.ConnectionState.Closed)
        //        {
        //            VayuConnection.Open();
        //        }

        //        sSelectNodeCommand.Parameters["@nodekey"].Value = externalId;

        //        using (SqlDataReader reader = sSelectNodeCommand.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                node.SourceZone = reader.IsDBNull(0) ? null : reader.GetString(0);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optionally log the exception
        //        Console.WriteLine("Error in GetNode: " + ex.Message);
        //    }
        //    finally
        //    {
        //        if (VayuConnection.State == System.Data.ConnectionState.Open)
        //        {
        //            VayuConnection.Close();
        //        }
        //    }

        //    return node;
        //}


        internal List<MarketView.Model.Constraint> GetAllHistoricalConstraintsData(string constraintname, string ContingencyName, bool isRt, int marketkey)
        {
            loadDBCommands();
            DateTime prevDate = DateTime.MinValue;
            List<MarketView.Model.Constraint> HistoricalDataList = new List<MarketView.Model.Constraint>();

            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();

            SqlDataReader rdr = null;
            if (isRt)
            {
                HistoricalDataList = GetHistoricalConstraintsForErcot(constraintname, isRt, marketkey);


                return HistoricalDataList;

                //mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ConstraintText", constraintname);
                ////mSelectERCOTHistoricConstraintData.Parameters.AddWithValue("@ContingencyText", ContingencyName);
                //rdr = mSelectERCOTHistoricConstraintData.ExecuteReader();

            }




            return HistoricalDataList;
        }

        internal List<ViewModels.SensitivityHelper> GetErcotSensitivities(string constraintName, string contingencyName, bool isDA)
        {
            List<ViewModels.SensitivityHelper> senSitivityList = new List<ViewModels.SensitivityHelper>();
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Connection = con;
                        if (isDA)
                        {
                            cmd.CommandText = "select a.ConstraintRTNum , b.MonitoredText , b.ContingencyText , c.NodeName , a.Sensitivity , a.source , a.nodekey , c.Zone  " +
                                             "from DAMasterVector a join DAMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum join Node c on a.NodeKey = c.NodeKey " +
                                             "where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "'";
                        }
                        else
                        {
                            cmd.CommandText = "select a.ConstraintRTNum , b.MonitoredText , b.ContingencyText , c.NodeName , a.Sensitivity , a.source , a.nodekey , c.Zone " +
                                " from RTMasterVector_new a join RTMasterConstraint b on a.ConstraintRTNum = b.ConstraintRTNum " +
                                " join Node c on a.NodeKey = c.NodeKey where b.MonitoredText = '" + constraintName + "' and b.ContingencyText = '" + contingencyName + "'";
                        }
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            ViewModels.SensitivityHelper helper = new ViewModels.SensitivityHelper();
                            helper.ConstraintId = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                            helper.Constraint = rdr.IsDBNull(1) ? "" : rdr.GetValue(1).ToString();
                            helper.Contingency = rdr.IsDBNull(2) ? "" : rdr.GetValue(2).ToString();
                            helper.NodeName = rdr.IsDBNull(3) ? "" : rdr.GetValue(3).ToString();
                            helper.Sensitivity = rdr.IsDBNull(4) ? 0 : Convert.ToDouble(rdr.GetValue(4));
                            helper.Source = rdr.IsDBNull(5) ? "" : rdr.GetValue(5).ToString();
                            helper.NodeKey = rdr.IsDBNull(6) ? 0 : Convert.ToInt32(rdr.GetValue(6));
                            helper.Zone = rdr.IsDBNull(7) ? "" : rdr.GetValue(7).ToString();
                            senSitivityList.Add(helper);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return senSitivityList;
        }

        internal List<MarketView.Model.Constraint> GetHistoricalConstraintsForErcot(string constraintName, bool isRT, int marketKey)
        {
            List<MarketView.Model.Constraint> tempdatalist = null;

            if (constraintName == "")
            {
                return tempdatalist;
            }

            List<MarketView.Model.Constraint> tempList = new List<MarketView.Model.Constraint>();

            if (isRT && marketKey == 9)
            {
                loadDBCommands();


                List<LatestConstraint> latestConstraintList = new List<LatestConstraint>();

                Dictionary<string, DateTime> dictConstraintHistory = new Dictionary<string, DateTime>();

                SqlCommand cmdGetConstraintHistory = VayuConnection.CreateCommand();
                cmdGetConstraintHistory.CommandText = " select distinct ConstraintText, ContingencyText, MarketDateTime, ShadowPrice from Vayu..ConstraintRT "
                    + " where ConstraintText = @ConstraintText order by MarketDateTime desc ";
                cmdGetConstraintHistory.Parameters.AddWithValue("@ConstraintText", "ConstraintText");

                cmdGetConstraintHistory.Parameters["@ConstraintText"].Value = constraintName;

                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                SqlDataReader rdr = cmdGetConstraintHistory.ExecuteReader();

                while (rdr.Read())
                {
                    //string ConstraintName = rdr.GetString(0);
                    //string Contingency = rdr.GetString(1);
                    //DateTime MarketDateTime = rdr.GetDateTime(2);
                    //double ShadowPrice = rdr.GetDouble(3);

                    LatestConstraint latestConstraint = new LatestConstraint();
                    latestConstraint.ConstraintText = rdr.GetValue(0).ToString();
                    latestConstraint.ContigencyText = rdr.GetValue(1).ToString();
                    latestConstraint.MarketDate = Convert.ToDateTime(rdr.GetValue(2));
                    //latestConstraint.hour = Convert.ToInt32(rdr.GetValue(3));
                    latestConstraint.ShadowPrice = rdr.IsDBNull(3) ? 00 : Convert.ToDouble(rdr.GetValue(3));

                    latestConstraintList.Add(latestConstraint);
                }

                rdr.Close();
                VayuConnection.Close();

                mAvgHash = new Dictionary<string, Dictionary<DateTime, double>>();
                foreach (var item in latestConstraintList)
                {
                    string constraint = item.ConstraintText;
                    string contingency = item.ContigencyText;

                    DateTime mktDate;
                    //if (isDa)
                    //    mktDate = item.MarketDate.AddHours(-1);
                    //else

                    mktDate = item.MarketDate;
                    double prc = item.ShadowPrice;
                    string keyText = constraint + "?split?" + contingency;
                    //int sourceNodekey = item.SourceNodeKey;
                    //int sinkNodekey = item.SinkNodeKey;
                    string constName = constraint + "?" + contingency;

                    MarketView.Model.Constraint constraintItem = tempList.Where(a => a.ConstraintText == constraint && a.ContingencyText == contingency && a.ConstraintDate.Date == mktDate.Date).FirstOrDefault();
                    if (constraintItem == null)
                    {
                        constraintItem = new MarketView.Model.Constraint
                        {
                            ConstraintDate = mktDate,
                            ContingencyText = contingency,
                            ConstraintText = constraint
                        };
                    }
                    if (mAvgHash.ContainsKey(keyText))
                    {
                        Dictionary<DateTime, double> hourValues = mAvgHash[keyText];
                        if (hourValues.ContainsKey(mktDate))
                        {
                            if (isRT)
                            {
                                if (!double.IsNaN(item.ShadowPriceNaN))
                                    hourValues[mktDate] += hourValues[mktDate] + Math.Abs(item.ShadowPrice);
                            }
                        }
                        else
                        {
                            hourValues.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        }
                    }
                    else
                    {
                        Dictionary<DateTime, double> tempHash = new Dictionary<DateTime, double>();
                        tempHash.Add(mktDate, double.IsNaN(item.ShadowPriceNaN) ? 0 : Math.Abs(item.ShadowPrice));
                        mAvgHash.Add(keyText, tempHash);
                    }

                    if (tempList.Contains(constraintItem))
                        tempList.Remove(constraintItem);
                    tempList.Add(constraintItem);
                }
            }

            tempdatalist = CalculateShadowPrices(tempList, marketKey, false);

            return tempdatalist;
        }

        List<MarketView.Model.Constraint> CalculateShadowPrices(List<MarketView.Model.Constraint> tempList, int marketKey, bool isDa)
        {
            foreach (var item in mAvgHash)
            {
                try
                {

                    string[] conMon = item.Key.Split(new string[] { "?split?" }, StringSplitOptions.None);
                    var conMonList = tempList.Where(a => a.ConstraintText == conMon[0] && a.ContingencyText == conMon[1]);
                    foreach (DateTime dObj in conMonList.Select(p => p.ConstraintDate.Date).Distinct())
                    {
                        try
                        {
                            List<ConstraintHistoryhelper> lstConstraintHistoryhelpers = new List<ConstraintHistoryhelper>();

                            loadDictHash = new Dictionary<string, double>();
                            var constObjList = conMonList.Where(a => a.ConstraintDate.Date == dObj);

                            if (marketKey == 9)
                            {
                                if (loadDictHash.Count() == 0)
                                {
                                    List<DateTime> datetimeList = constObjList.Select(x => x.ConstraintDate).ToList<DateTime>();
                                    loadDictHash = GetLoadsData(datetimeList, marketKey);
                                }
                            }

                            if (constObjList.Count() == 0)
                                continue;

                            try
                            {
                                MarketView.Model.Constraint constObj = constObjList.First();
                                string dateCounter = string.Empty;  //DateTime.Now.ToString("yyyy-MM-dd");
                                string dt = string.Empty; //int hrs = 0;
                                if (constObjList.FirstOrDefault() != null)
                                {
                                    dt = constObjList.FirstOrDefault().ConstraintDate.ToString("yyyy-MM-dd");
                                    //  hrs=dt.Hour;
                                    dateCounter = dt; //.Date.AddHours(hrs);
                                }
                                if (!isDa)
                                {
                                    if (marketKey == 9)
                                    {
                                        Dictionary<int, double> hourlyValues = new Dictionary<int, double>();
                                        Dictionary<int, Dictionary<int, double>> values = new Dictionary<int, Dictionary<int, double>>();
                                        Dictionary<int, List<ErcotConstraintHelper>> hourMinuteDict = new Dictionary<int, List<ErcotConstraintHelper>>();

                                        foreach (var a in item.Value)
                                        {
                                            {
                                                if (a.Key.Date == dObj)
                                                {
                                                    ErcotConstraintHelper ercotConstraintHelper = new ErcotConstraintHelper();
                                                    ercotConstraintHelper.Minute = a.Key.Minute;
                                                    ercotConstraintHelper.Seconds = a.Key.Second;
                                                    ercotConstraintHelper.ShadowPrice = a.Value;
                                                    ercotConstraintHelper.Hourval = a.Key.Hour;

                                                    List<ErcotConstraintHelper> lstErcotConstraintHelpers = new List<ErcotConstraintHelper>();
                                                    lstErcotConstraintHelpers.Add(ercotConstraintHelper);

                                                    if (!(hourMinuteDict.ContainsKey(a.Key.Hour)))
                                                    {
                                                        hourMinuteDict.Add(a.Key.Hour, lstErcotConstraintHelpers);
                                                    }
                                                    else
                                                    {
                                                        hourMinuteDict[a.Key.Hour].Add(ercotConstraintHelper);
                                                    }
                                                    //}
                                                }
                                            }
                                        }

                                        List<int> hourList = hourMinuteDict.Keys.ToList();


                                        List<string> l1 = new List<string>();
                                        List<string> l2 = new List<string>();
                                        List<string> l3 = new List<string>();
                                        List<string> l4 = new List<string>();

                                        hourList = hourList.OrderByDescending(x => x).ToList();
                                        foreach (var hour in hourList)
                                        {
                                            var hourMinuteCollection = hourMinuteDict[hour];
                                            var hourMinuteCollectionSP = hourMinuteDict[hour];
                                            double interval1, interval2, interval3, interval4;
                                            double spvalue, finalsp;

                                            int currmin, currsec;
                                            string key, allval;

                                            key = allval = "";
                                            spvalue = finalsp = 0;


                                            ErcotConstraintHelper int1Value = new ErcotConstraintHelper();

                                            l1.Clear(); l2.Clear(); l3.Clear(); l4.Clear();
                                            interval1 = interval2 = interval3 = interval4 = 0;

                                            hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                                            int elementcount = hourMinuteCollectionSP.Count;
                                            int lastmin = hourMinuteCollectionSP.ElementAt(elementcount - 1).Minute;
                                            ErcotConstraintHelper tempobj;

                                            if (lastmin < 55)
                                            {
                                                List<string> MissingPrintList = getMissingPrints(constObj.ConstraintDate, hour, lastmin);

                                                int cnt = 0;
                                                while (cnt < MissingPrintList.Count)
                                                {
                                                    string[] valuearray = MissingPrintList.ElementAt(cnt).Split('#');
                                                    tempobj = new ErcotConstraintHelper();
                                                    tempobj.Hourval = hour;
                                                    tempobj.Minute = Int32.Parse(valuearray[0]);
                                                    tempobj.Seconds = Int32.Parse(valuearray[1]);
                                                    tempobj.ShadowPrice = 0;

                                                    hourMinuteCollectionSP.Add(tempobj);
                                                    cnt++;
                                                }

                                            }
                                            hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                                            elementcount = hourMinuteCollectionSP.Count;
                                            lastmin = hourMinuteCollectionSP.ElementAt(elementcount - 1).Minute;

                                            while (lastmin < 55)
                                            {
                                                if (lastmin % 5 == 0)
                                                    lastmin = lastmin + 5;
                                                else
                                                {
                                                    do
                                                        lastmin = lastmin + 1;
                                                    while ((lastmin % 5 != 0));
                                                }

                                                tempobj = new ErcotConstraintHelper();
                                                tempobj.Hourval = hour;
                                                tempobj.Seconds = 0;
                                                tempobj.Minute = lastmin;
                                                tempobj.ShadowPrice = 0;
                                                hourMinuteCollectionSP.Add(tempobj);
                                            }

                                            hourMinuteCollectionSP = hourMinuteCollectionSP.OrderBy(x => x.Minute).ToList();
                                            for (int i = 0; i < hourMinuteCollectionSP.Count; i++)
                                            {
                                                int1Value = new ErcotConstraintHelper();
                                                int1Value.Hourval = hourMinuteCollectionSP.ElementAt(i).Hourval;
                                                int1Value.Minute = hourMinuteCollectionSP.ElementAt(i).Minute;
                                                int1Value.Seconds = hourMinuteCollectionSP.ElementAt(i).Seconds;
                                                int1Value.ShadowPrice = hourMinuteCollectionSP.ElementAt(i).ShadowPrice;

                                                currmin = int1Value.Minute;
                                                currsec = int1Value.Seconds;

                                                key = allval = "";
                                                finalsp = 0;


                                                if (i == 0)
                                                {
                                                    key = constObj.ConstraintText + "?split?" + constObj.ContingencyText;
                                                    spvalue = getPreviousMinSP(hour, currmin, currsec, hourMinuteDict, key, dateCounter);

                                                    finalsp = (currmin * 60 + currsec) * spvalue;
                                                    allval = hour + "#" + currmin + "#" + currsec + "?" + (currmin * 60 + currsec) + "#" + spvalue + "#" + finalsp;
                                                    l1.Add(allval);
                                                    interval1 = interval1 + finalsp;
                                                }

                                                int totalsec1, totalsec2, totaltime;
                                                if (i > 0)
                                                {
                                                    if (currmin > 0 && currmin <= 15)
                                                    {
                                                        if (currmin == 15)
                                                        {
                                                            totalsec1 = currmin * 60;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            totalsec1 = currmin * 60 + currsec;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l1.Add(allval);
                                                        interval1 = interval1 + finalsp;

                                                    }
                                                    if (currmin >= 15 && currmin <= 30)
                                                    {
                                                        if (currmin == 30)
                                                        {
                                                            totalsec1 = currmin * 60;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            if (currmin == 15)
                                                            {

                                                                totalsec2 = totalsec1 = 0;
                                                                totaltime = currsec;
                                                            }
                                                            else
                                                            {
                                                                totalsec1 = currmin * 60 + currsec;
                                                                totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                                totaltime = totalsec1 - totalsec2;
                                                            }
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l2.Add(allval);
                                                        interval2 = interval2 + finalsp;

                                                    }
                                                    if (currmin >= 30 && currmin <= 45)
                                                    {
                                                        if (currmin == 45)
                                                        {
                                                            totalsec1 = currmin * 60;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            if (currmin == 30)
                                                            {
                                                                totalsec2 = totalsec1 = 0;
                                                                totaltime = currsec;
                                                            }
                                                            else
                                                            {
                                                                totalsec1 = currmin * 60 + currsec;
                                                                totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                                totaltime = totalsec1 - totalsec2;
                                                            }
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l3.Add(allval);
                                                        interval3 = interval3 + finalsp;

                                                    }


                                                    if (currmin >= 45 && currmin <= 59)
                                                    {
                                                        if (i == hourMinuteCollectionSP.Count - 1)
                                                        {

                                                            totalsec1 = currmin * 60 + currsec;
                                                            totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                            totaltime = totalsec1 - totalsec2;

                                                        }
                                                        else
                                                        {
                                                            if (currmin == 45)
                                                            {

                                                                totalsec2 = totalsec1 = 0;
                                                                totaltime = currsec;
                                                            }
                                                            else
                                                            {
                                                                totalsec1 = currmin * 60 + currsec;
                                                                totalsec2 = hourMinuteCollectionSP.ElementAt(i - 1).Minute * 60 + hourMinuteCollectionSP.ElementAt(i - 1).Seconds;
                                                                totaltime = totalsec1 - totalsec2;
                                                            }
                                                        }

                                                        spvalue = hourMinuteCollectionSP.ElementAt(i - 1).ShadowPrice;
                                                        finalsp = totaltime * spvalue;
                                                        allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                        l4.Add(allval);
                                                        interval4 = interval4 + finalsp;

                                                        if (i == hourMinuteCollectionSP.Count - 1)
                                                        {

                                                            totalsec1 = currmin * 60 + currsec;
                                                            totalsec2 = 60 * 60;
                                                            totaltime = totalsec2 - totalsec1;

                                                            spvalue = hourMinuteCollectionSP.ElementAt(i).ShadowPrice;
                                                            finalsp = totaltime * spvalue;
                                                            allval = hour + "#" + currmin + "#" + currsec + "?" + totalsec1 + "#" + totalsec2 + "#" + totaltime + "#" + spvalue + "#" + finalsp;
                                                            l4.Add(allval);
                                                            interval4 = interval4 + finalsp;

                                                        }

                                                    }
                                                }

                                            }
                                            interval1 = interval1 / 900;
                                            interval2 = interval2 / 900;
                                            interval3 = interval3 / 900;
                                            interval4 = interval4 / 900;
                                            spvalue = (interval1 + interval2 + interval3 + interval4) / 4;




                                            ConstraintHistoryhelper objConstraintHistoryhelper = new ConstraintHistoryhelper();
                                            objConstraintHistoryhelper.ConstraintName = constObj.ConstraintText;
                                            objConstraintHistoryhelper.ContingencyName = constObj.ContingencyText;
                                            objConstraintHistoryhelper.date = constObj.ConstraintDate;
                                            objConstraintHistoryhelper.hour = hour;
                                            objConstraintHistoryhelper.shadowprice = spvalue;



                                            lstConstraintHistoryhelpers.Add(objConstraintHistoryhelper);
                                        }

                                        foreach (var annItem in lstConstraintHistoryhelpers)
                                        {
                                            try
                                            {
                                                constObj.GetType().GetProperty("HE" + (annItem.hour == 0 ? "1" : (annItem.hour + 1).ToString())).SetValue(constObj, annItem.shadowprice == 0 ? 0 : annItem.shadowprice);
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        constObj.Price = lstConstraintHistoryhelpers.Sum(a => a.shadowprice) / 24;
                                    }
                                    else
                                    {
                                        var data = from a in item.Value
                                                   where isDa ? (a.Key >= dObj && a.Key < dObj.AddDays(1)) : a.Key.Date == dObj
                                                   group a by a.Key.Hour into g
                                                   select new { time = g.Key, val = g.Sum(o => o.Value) / (isDa ? 1 : 12) }; // change the count here

                                        foreach (var annItem in data)
                                        {
                                            try
                                            {
                                                constObj.GetType().GetProperty("HE" + (annItem.time == 0 ? "1" : (annItem.time + 1).ToString())).SetValue(constObj, annItem.val == 0 ? (double?)null : annItem.val);
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        constObj.Price = data.Sum(a => a.val) / 24;
                                    }

                                }
                                else
                                {
                                    //foreach (var a in item.Value)
                                    {
                                        Dictionary<DateTime, double> shadowPriceDict = item.Value;
                                        foreach (var item1 in shadowPriceDict)
                                        {
                                            //if (item1.Key.Hour == 0)
                                            //    constObj.GetType().GetProperty("HE" + (item1.Key.Hour + 24).ToString()).SetValue(constObj, item1.Value == 0 ? (double?)null : item1.Value);
                                            //else
                                            if (dObj == item1.Key.Date)
                                                constObj.GetType().GetProperty("HE" + (item1.Key.Hour + 1).ToString()).SetValue(constObj, item1.Value == 0 ? (double?)null : item1.Value);
                                        }
                                    }
                                    constObj.Price = (item.Value.Sum(a => a.Value)) / 24;
                                }
                                //if (loadDictHash.ContainsKey(dateCounter))
                                //    constObj.MaxLoad = loadDictHash[dateCounter];
                                //constObj.MaxShadowPrice = item.Value;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                catch
                {

                }
            }
            List<MarketView.Model.Constraint> tempdatalist = tempList.Distinct().ToList();

            return tempdatalist;
        }

        List<string> getMissingPrints(DateTime DateVal, int Hourval, int MinVal)
        {
            List<string> MinSecList = new List<string>();
            string MinuteSecond = "";

            try
            {
                SqlCommand mSelectSCEDPrintCommond = new SqlCommand();
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mSelectSCEDPrintCommond.CommandText = "Select distinct MarketMin, Second from Vayu..NodeLMPMin where MarketDate = @DateVal and  Markethour=@Hourval and MarketMin > @MinVal";
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@DateVal", DateVal.Date);
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@Hourval", Hourval);
                mSelectSCEDPrintCommond.Parameters.AddWithValue("@MinVal", MinVal);
                mSelectSCEDPrintCommond.Connection = VayuConnection;
                SqlDataReader reader = mSelectSCEDPrintCommond.ExecuteReader();
                while (reader.Read())
                {
                    MinuteSecond = reader.GetInt32(0) + "#" + reader.GetInt32(1);
                    MinSecList.Add(MinuteSecond);
                }
                reader.Close();

            }
            catch (Exception ae)
            { }
            return MinSecList;

        }

        public Dictionary<string, string> getConstraintByRT(string constraint, string contingency)
        {
            Dictionary<string, string> CTList = new Dictionary<string, string>();
            string ConstraintName = "";
            string ContigencyName = "";
            try
            {
                SqlCommand mSelectConstraintCommond = new SqlCommand();
                VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                mSelectConstraintCommond.CommandText = "select distinct(MonitoredText), ContingencyText from RTMasterConstraint where  ContingencyText like @contingency";//where MonitoredText like @constraint and ContingencyText like @contingency
                //mSelectConstraintCommond.Parameters.AddWithValue("@constraint", constraint);
                mSelectConstraintCommond.Parameters.AddWithValue("@contingency", contingency);
                mSelectConstraintCommond.Connection = VayuConnection;
                SqlDataReader reader = mSelectConstraintCommond.ExecuteReader();
                while (reader.Read())
                {
                    ConstraintName = reader.GetString(0);
                    string[] CTName = ConstraintName.Split(new char[] { '/' });
                    string NameCT = CTName[1];
                    ContigencyName = reader.GetString(1);
                    if (CTList.ContainsKey(NameCT))
                    {

                    }
                    else
                    {
                        CTList.Add(NameCT, ConstraintName);
                    }

                }
            }
            catch (Exception ex)
            {

            }

            return CTList;
        }

        double getPreviousMinSP(int hour, int min, int sec, Dictionary<int, List<ErcotConstraintHelper>> TotalhourMinuteDict, string key, string dateString)
        {
            double sp = 0;
            List<ErcotConstraintHelper> prevHourMinCollectionSP = null;

            try
            {

                if (hour == 0)
                {
                    var item2 = mAvgHash[key];

                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime dateTimeValue = Convert.ToDateTime(dateString);
                    dateTimeValue = dateTimeValue.AddMinutes(-5);
                    List<ErcotConstraintHelper> valueliest = new List<ErcotConstraintHelper>();
                    ErcotConstraintHelper obj;
                    foreach (var keyitem in item2)
                    {

                        if (keyitem.Key.Date.Equals(dateTimeValue.Date))
                        {
                            if (keyitem.Key.Hour == 23 && keyitem.Key.Minute > 54)
                            {
                                obj = new ErcotConstraintHelper();
                                obj.Hourval = 23;
                                obj.Minute = keyitem.Key.Minute;
                                obj.Seconds = keyitem.Key.Second;
                                obj.ShadowPrice = keyitem.Value;
                                valueliest.Add(obj);
                            }
                        }
                    }


                    valueliest = valueliest.OrderByDescending(x => x.Minute).ToList();

                    if (valueliest.Count > 0)
                        sp = valueliest.ElementAt(0).ShadowPrice;

                }
                if (TotalhourMinuteDict.ContainsKey(hour - 1))
                {
                    prevHourMinCollectionSP = TotalhourMinuteDict[hour - 1];
                    prevHourMinCollectionSP = prevHourMinCollectionSP.OrderBy(x => x.Minute).ToList();
                    int lastindex = prevHourMinCollectionSP.Count - 1;

                    sp = prevHourMinCollectionSP.ElementAt(lastindex).ShadowPrice;
                }
            }
            catch (Exception ae)
            { }


            return sp;
        }

        public Tuple<String, Dictionary<int, double>> GetDACongestionByNodeAndHour(DateTime dateTime)
        {
            Dictionary<int, double> NodeDACongHash = new Dictionary<int, double>();
            DateTime dtValue = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlDataReader reader = null;
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            try
            {
                mSelectDACongErcotNodeCommand = VayuConnection.CreateCommand();
                mSelectDACongErcotNodeCommand.CommandText = " select NodeKey, Congestion from NodeDALMPH where MarketDateTime = '" + dtValue + "' order by NodeKey";
                mSelectDACongErcotNodeCommand.Connection = VayuConnection;
                reader = mSelectDACongErcotNodeCommand.ExecuteReader();
                try
                {

                    while (reader.Read())
                    {
                        NodeDACongHash.Add(reader.GetInt32(0), !reader.IsDBNull(1) ? Convert.ToDouble(reader.GetValue(1)) : 0);

                    }
                    if (VayuConnection.State == ConnectionState.Open)
                    {
                        VayuConnection.Close();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    if (VayuConnection.State == ConnectionState.Open)
                    {
                        VayuConnection.Close();
                    }
                    if (reader != null && !reader.IsClosed)
                    {
                        reader.Close();
                    }
                    return Tuple.Create("Error", NodeDACongHash);
                }
            }
            catch (Exception ex)
            {
                if (VayuConnection.State == ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
                return Tuple.Create("Error", NodeDACongHash);
            }


            return Tuple.Create("", NodeDACongHash);
        }

        internal Dictionary<string, double> GetLoadsData(List<DateTime> datetimeList, int Marketkey)
        {
            try
            {
                Dictionary<string, double> loadDictHash = new Dictionary<string, double>();
                using (VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection()) ;
                {
                    //List<DateTime> temiList = datetimeList.GroupBy(x => x.ToString("dd-MM-yyyy"));
                    foreach (DateTime date in datetimeList)
                    {
                        double load = 0; string mdate; string sdate = string.Empty;
                        int hr = 12;
                        if (!loadDictHash.ContainsKey(date.AddDays(-1).Date.ToString("dd-MM-yyyy")))
                        {


                            SqlCommand mSelectloadDailyCommand = new SqlCommand();
                            if (VayuConnection.State == ConnectionState.Closed)
                            {
                                VayuConnection.Open();
                            }
                            mSelectloadDailyCommand.CommandText = "select CAST(MarketDateTime as date) date , max(mw) from Vayu..LoadRT where loadskey=2213 and   MarketDateTime >= @Startdate and MarketDateTime < @endDate " +
                                                                  " group by CAST(MarketDateTime as date) order by date";
                            mSelectloadDailyCommand.Parameters.AddWithValue("@Startdate", date.Date);
                            mSelectloadDailyCommand.Parameters.AddWithValue("@endDate", date.Date.AddDays(1));
                            mSelectloadDailyCommand.Connection = VayuConnection;
                            SqlDataReader reader = mSelectloadDailyCommand.ExecuteReader();
                            while (reader.Read())
                            {
                                load = reader.IsDBNull(1) ? 0.0 : Convert.ToDouble(reader.GetValue(1));
                                // mdate = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                                // loadDictHash.Add(mdate, load);
                            }
                            reader.Close();
                            VayuConnection.Close();
                            #region Load Zero
                            if (load == 0)
                            {
                                SqlCommand mSelectloadHourlyCommand = new SqlCommand();
                                mSelectloadHourlyCommand.CommandText = "select  max(MW) from Vayu..LoadRT where LoadsKey = 2213 and MarketDateTime >=  @StartDate and MarketDateTime < @EndDate";//LoadRTH 
                                mSelectloadHourlyCommand.Parameters.AddWithValue("@StartDate", date.Date.AddDays(-1));
                                mSelectloadHourlyCommand.Parameters.AddWithValue("@EndDate", date.Date);
                                mSelectloadHourlyCommand.Connection = VayuConnection;
                                SqlDataReader reader1 = mSelectloadHourlyCommand.ExecuteReader();
                                while (reader1.Read())
                                {
                                    load = reader1.IsDBNull(0) ? 0.0 : Convert.ToDouble(reader1.GetValue(0));
                                }
                                reader1.Close();
                            }
                            #endregion Load Zero

                            loadDictHash.Add(date.ToString("yyyy-MM-dd"), load);
                        }
                    }
                }

                return loadDictHash;

            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        public List<string> GetDeenergizedNodes()
        {
            loadDBCommands();
            List<string> listNodes = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
                VayuConnection.Open();
            mSelectErcotDeenergizedNodesCmd = new SqlCommand();
            mSelectErcotDeenergizedNodesCmd.CommandText = "Select Name,MarketDate,Hours from Vayu..DeenergizedNodes where MarketDate='" + DateTime.Now.Date + "'";
            mSelectErcotDeenergizedNodesCmd.Connection = VayuConnection;
            SqlDataReader reader = mSelectErcotDeenergizedNodesCmd.ExecuteReader();
            while (reader.Read())
            {
                // NodeDeenergized = new DeenergizedNode();
                string NodeName = reader.GetString(0);
                //NodeDeenergized.MarketDate = reader.GetDateTime(1);
                //  NodeDeenergized.Hours = reader.GetInt32(2);
                listNodes.Add(NodeName);
            }
            reader.Close();
            VayuConnection.Close();
            return listNodes;
        }

        class ConstraintHistoryhelper
        {
            public string ConstraintName { get; set; }
            public string ContingencyName { get; set; }
            public DateTime date { get; set; }
            /// <summary>
            /// Gets or sets the hour.
            /// </summary>
            /// <value>
            /// The hour.
            /// </value>
            public int hour { get; set; }
            /// <summary>
            /// Gets or sets the shadowprice.
            /// </summary>
            /// <value>
            /// The shadowprice.
            /// </value>
            public double shadowprice { get; set; }
        }

    }
    internal class ErcotConstraintHelper
    {
        public int Minute { get; set; }
        public int Seconds { get; set; }
        public double ShadowPrice { get; set; }
        public int Hourval { get; set; }
    }

}