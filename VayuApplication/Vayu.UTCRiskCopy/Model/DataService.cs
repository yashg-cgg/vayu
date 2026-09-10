using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows;
using Vayu.CommonAccessLibrary;

namespace Vayu.UTCRiskCopy.Model
{
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;
        private SqlCommand mSelectEESBidsCommand;
        private SqlCommand mSelectErcotPTPBidsCommand;
        private SqlCommand mDeleteEESBidsCommand;
        private SqlCommand mDeleteErcotPTPBidsCommand;
        private DataTable mDatatableBids;
        private SqlCommand mSelectMultiplierCommand;
        private SqlCommand mDeleteEESBidsTestommand;
        private Dictionary<int, double> mMultiplierHash = new Dictionary<int, double>();

        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectEESBidsCommand = new SqlCommand();
            mSelectEESBidsCommand.CommandText = "select ScheduleID,BidStatus,OasisID,EndMarketDateTime,RequestedMW,ClearedMW,EndUserKey,SourceNodeKey,SinkNodeKey,POR,POD,Source,Sink,Price,PortfolioKey,SubmittedDateTime,Comments from eesbids where PortfolioKey in (@PortfolioKey) and EndMarketDateTime>@startdate and EndMarketDateTime<=@enddate order by EndMarketDateTime"; //eesbidstemp
            mSelectEESBidsCommand.Parameters.AddWithValue("@startdate", "EndMarketDateTime");
            mSelectEESBidsCommand.Parameters.AddWithValue("@enddate", "EndMarketDateTime");
            mSelectEESBidsCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mSelectEESBidsCommand.Connection = VayuConnection;
            //
            mSelectErcotPTPBidsCommand = new SqlCommand();
            mSelectErcotPTPBidsCommand.CommandText = "select ScheduleID,BidStatus,OasisID,EndMarketDateTime,RequestedMW,ClearedMW,EndUserKey,SourceNodeKey,SinkNodeKey,POR,POD,Source,Sink,Price,PortfolioKey,SubmittedDateTime,Comments from Vayu..ErcotPTPBids  where PortfolioKey in (@PortfolioKey) and EndMarketDateTime>@startdate and EndMarketDateTime<=@enddate order by EndMarketDateTime"; //ErcotPTPBidstemp
            mSelectErcotPTPBidsCommand.Parameters.AddWithValue("@startdate", "EndMarketDateTime");
            mSelectErcotPTPBidsCommand.Parameters.AddWithValue("@enddate", "EndMarketDateTime");
            mSelectErcotPTPBidsCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mSelectErcotPTPBidsCommand.Connection = VayuConnection;

            //

            mDeleteEESBidsCommand = new SqlCommand();
            mDeleteEESBidsCommand.CommandText = "delete from eesbids where PortfolioKey in (@PortfolioKey) and EndMarketDateTime>@startdate and EndMarketDateTime<=@enddate";//eesbidstemp
            mDeleteEESBidsCommand.Parameters.AddWithValue("@startdate", "EndMarketDateTime");
            mDeleteEESBidsCommand.Parameters.AddWithValue("@enddate", "EndMarketDateTime");
            mDeleteEESBidsCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mDeleteEESBidsCommand.Connection = VayuConnection;
            // ERCOT 
            mDeleteErcotPTPBidsCommand = new SqlCommand();
            mDeleteErcotPTPBidsCommand.CommandText = "delete from Vayu..ErcotPTPBids where PortfolioKey in (@PortfolioKey) and EndMarketDateTime>@startdate and EndMarketDateTime<=@enddate";//ErcotPTPBidstemp 
            mDeleteErcotPTPBidsCommand.Parameters.AddWithValue("@startdate", "EndMarketDateTime");
            mDeleteErcotPTPBidsCommand.Parameters.AddWithValue("@enddate", "EndMarketDateTime");
            mDeleteErcotPTPBidsCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mDeleteErcotPTPBidsCommand.Connection = VayuConnection;
            //
            mSelectMultiplierCommand = new SqlCommand();
            mSelectMultiplierCommand.CommandText = "select account, multiplier from firmaccount";
            mSelectMultiplierCommand.Connection = VayuConnection;

            mDatatableBids = new DataTable();
            mDatatableBids.Columns.Add("ScheduleID", typeof(string));
            mDatatableBids.Columns.Add("BidStatus", typeof(string));
            mDatatableBids.Columns.Add("OasisID", typeof(int));
            mDatatableBids.Columns.Add("EndMarketDateTime", typeof(DateTime));
            mDatatableBids.Columns.Add("RequestedMW", typeof(double));
            mDatatableBids.Columns.Add("ClearedMW", typeof(double));
            mDatatableBids.Columns.Add("EndUserKey", typeof(int));
            mDatatableBids.Columns.Add("SourceNodeKey", typeof(int));
            mDatatableBids.Columns.Add("SinkNodeKey", typeof(int));
            mDatatableBids.Columns.Add("POR", typeof(string));
            mDatatableBids.Columns.Add("POD", typeof(string));
            mDatatableBids.Columns.Add("Source", typeof(string));
            mDatatableBids.Columns.Add("Sink", typeof(string));
            mDatatableBids.Columns.Add("Price", typeof(double));
            mDatatableBids.Columns.Add("PortfolioKey", typeof(int));
            mDatatableBids.Columns.Add("SubmittedDateTime", typeof(DateTime));
            mDatatableBids.Columns.Add("Comments", typeof(string));

            mDeleteEESBidsTestommand = new SqlCommand();
            mDeleteEESBidsTestommand.CommandText = "truncate table eesbids_test";
            mDeleteEESBidsTestommand.Connection = VayuConnection;
        }
        private void FillMultiplierHash()
        {
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            mMultiplierHash = new Dictionary<int, double>();
            SqlDataReader reader = mSelectMultiplierCommand.ExecuteReader();
            while (reader.Read())
            {
                int portfolio = Int32.Parse(reader.GetString(0));
                double multiplier = reader.GetDouble(1);
                mMultiplierHash.Add(portfolio, multiplier);
            }
            reader.Close();
            VayuConnection.Close();
        }

        public void InsertBids(System.Collections.Generic.List<Vayu.DBLibrary.Portfolio> portfoliolist, DateTime sdate, string Market, int portfolioId)
        {
            string TableName = string.Empty;
            if (Market == "PJM")
            {
                string id = "";
                TableName = "[dbo].[eesbids]"; // eesbidstemp
                loadDBCommands();
                FillMultiplierHash();
                if (VayuConnection.State == ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
                VayuConnection.Open();
                mDeleteEESBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                mDeleteEESBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                mDeleteEESBidsCommand.Parameters["@PortfolioKey"].Value = portfolioId;
                int count = mDeleteEESBidsCommand.ExecuteNonQuery();
            }
            else
            {
                string id = "";
                TableName = "Vayu..ErcotPTPBids";// ErcotPTPBidstemp
                loadDBCommands();
                FillMultiplierHash();
                if (VayuConnection.State == ConnectionState.Open)
                {
                    VayuConnection.Close();
                }
                VayuConnection.Open();
                mDeleteErcotPTPBidsCommand.CommandTimeout = 30000;
                mDeleteErcotPTPBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                mDeleteErcotPTPBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                mDeleteErcotPTPBidsCommand.Parameters["@PortfolioKey"].Value = portfolioId;
                int count = mDeleteErcotPTPBidsCommand.ExecuteNonQuery();
            }
            List<Path> tempListPath = new List<Path>();
            List<Path> lstPath = new List<Path>();

            foreach (var item in portfoliolist)
            {
                SqlDataAdapter adapter = new SqlDataAdapter();
                DataSet ds = new DataSet();
                List<string> foundPathList = new List<string>();
                VayuConnection.Close();
                try
                {
                    if (Market == "PJM")
                    {
                        if (VayuConnection.State == ConnectionState.Open)
                        {
                            VayuConnection.Close();
                        }
                        VayuConnection.Open();

                        mSelectEESBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectEESBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectEESBidsCommand.Parameters["@PortfolioKey"].Value = item.ID;
                        adapter.SelectCommand = mSelectEESBidsCommand;
                        ds = new DataSet();
                        adapter.Fill(ds);
                    }
                    else
                    {
                        if (VayuConnection.State == ConnectionState.Open)
                        {
                            VayuConnection.Close();
                        }
                        VayuConnection.Open();
                        mSelectErcotPTPBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectErcotPTPBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectErcotPTPBidsCommand.Parameters["@PortfolioKey"].Value = item.ID;
                        adapter.SelectCommand = mSelectErcotPTPBidsCommand;
                        ds = new DataSet();
                        adapter.Fill(ds);
                    }
                    int mcount = ds.Tables[0].Rows.Count;
                    if (mcount > 0)
                    {

                        for (int i = 0; i < mcount; i++)
                        {
                            int portfolio = Convert.ToInt32(ds.Tables[0].Rows[i].ItemArray[14]);
                            double multiplier = 0;
                            if (mMultiplierHash.ContainsKey(portfolio))
                            {
                                multiplier = mMultiplierHash[portfolio];
                            }
                            string Id = ds.Tables[0].Rows[i].ItemArray[0].ToString();

                            Path path = new Path();
                            path.Source = ds.Tables[0].Rows[i].ItemArray[11].ToString();
                            path.Sink = ds.Tables[0].Rows[i].ItemArray[12].ToString();
                            path.Price = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[13].ToString()), 2);
                            path.EndMarketDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i].ItemArray[3]);

                            if (path.EndMarketDateTime.Hour == 0)
                            {
                                path.Hour = 24;
                            }
                            else
                            {
                                path.Hour = path.EndMarketDateTime.Hour;
                            }
                            double finalMultiplier = multiplier + 1;
                            if (Market == "ERCOT" && portfolioId == 3012)
                            {
                                finalMultiplier = multiplier;
                            }
                            else if (Market == "ERCOT" && portfolioId == 3022)
                            {
                                finalMultiplier = 1;
                            }

                            #region comment
                            //double finalMW = (Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[4].ToString()), 2) * finalMultiplier);
                            //if (Market == "ERCOT")
                            //{
                            //    if (finalMW < 1)
                            //    {
                            //        finalMW = 1 - (Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[4].ToString()), 2));
                            //    }
                            //}


                            //double mw = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[4].ToString()), 1);

                            //if (mw == 0.7)
                            //{

                            //}

                            //double mwWithMultiplier = (mw * finalMultiplier);

                            //decimal finalMW = Convert.ToDecimal(mwWithMultiplier);

                            //decimal decimalMW = Convert.ToDecimal(mw);

                            //if (Market == "ERCOT" && portfolioId == 3012 && finalMW < 1 && (1 - decimalMW >= decimalMW))
                            //{
                            //    //if (finalMW < 1)
                            //    //{
                            //    // finalMW = 1 - (Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[4].ToString()), 2));
                            //    //}
                            //    finalMW = 1 - decimalMW;                                
                            //}
                            #endregion

                            double mw = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[4].ToString()), 1);
                            double mwWithMultiplier = (mw * finalMultiplier);
                            decimal finalMW = Convert.ToDecimal(mwWithMultiplier);
                            decimal decimalMW = Convert.ToDecimal(mw);
                            if (Market == "ERCOT" && portfolioId == 3012 && (finalMW + decimalMW) < 1)
                            {
                                finalMW = 1 - decimalMW;
                            }

                            path.RequestedMW = Convert.ToDouble(finalMW);
                            path.ScheduleID = ds.Tables[0].Rows[i].ItemArray[0].ToString();
                            path.BidStatus = ds.Tables[0].Rows[i].ItemArray[1].ToString();
                            path.OasisID = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[2]);
                            path.EndMarketDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i].ItemArray[3]);
                            path.ClearedMW = (Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[5].ToString()), 2) * finalMultiplier);
                            path.EndUserKey = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[6]);
                            path.SourceNodeKey = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[7]);
                            path.SinkNodeKey = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[8]);
                            if (ds.Tables[0].Rows[i].ItemArray[9] == DBNull.Value)
                            {
                                path.POR = null; //DBNull.Value;
                            }
                            else
                            {
                                path.POR = ds.Tables[0].Rows[i].ItemArray[9].ToString();
                            }
                            if (ds.Tables[0].Rows[i].ItemArray[10] == DBNull.Value)
                            {
                                path.POD = null; //DBNull.Value;
                            }
                            else
                            {
                                path.POD = ds.Tables[0].Rows[i].ItemArray[10].ToString();
                            }
                            path.PortfolioKey = portfolioId;
                            path.SubmittedDateTime = Convert.ToDateTime(ds.Tables[0].Rows[i].ItemArray[15]);
                            if (ds.Tables[0].Rows[i].ItemArray[16] == DBNull.Value)
                            {
                                path.Comments = null; //DBNull.Value;
                            }
                            else
                            {
                                path.Comments = ds.Tables[0].Rows[i].ItemArray[16].ToString();
                            }
                            string key = path.Source + path.Sink + path.Price + path.RequestedMW + path.EndMarketDateTime;
                            //if (foundPathList.Contains(key))
                            //{
                            //    continue;
                            //}
                            //foundPathList.Add(key);
                            //Path tempPath = tempListPath.Where(a => a.SourceNodeKey == path.SourceNodeKey && a.SinkNodeKey == path.SinkNodeKey && a.Hour == path.Hour
                            //    && a.Price == path.Price).FirstOrDefault(); //  && a.RequestedMW == path.RequestedMW
                            //code change by Sangram
                            Path tempPath = tempListPath.Where(a =>a.ScheduleID==path.ScheduleID && a.SourceNodeKey == path.SourceNodeKey && a.SinkNodeKey == path.SinkNodeKey && a.Hour == path.Hour
                                && a.Price == path.Price).FirstOrDefault();
                            if (tempPath != null)
                            {
                                tempListPath.Find(x => x.ScheduleID == path.ScheduleID && x.SourceNodeKey == path.SourceNodeKey && x.SinkNodeKey == path.SinkNodeKey && x.Hour == path.Hour
                                    && x.Price == path.Price).RequestedMW += path.RequestedMW; //  && x.RequestedMW == path.RequestedMW

                            }
                            else
                            {
                                tempListPath.Add(path);
                            }
                        }
                        adapter.Dispose();
                        mSelectEESBidsCommand.Dispose();
                        mSelectErcotPTPBidsCommand.Dispose();
                        VayuConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "Error While Inserting Bids");
                }
                finally
                {
                    VayuConnection.Close();
                }
            }
            mDatatableBids = ToDataTable(tempListPath);
            mDatatableBids.Columns.Remove("Hour");
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            SqlTransaction transaction = VayuConnection.BeginTransaction();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkLmpH.DestinationTableName = TableName;
                    bkLmpH.BulkCopyTimeout = 300000;
                    bkLmpH.BatchSize = 1000;
                    bkLmpH.ColumnMappings.Add("ScheduleID", "ScheduleID");
                    bkLmpH.ColumnMappings.Add("BidStatus", "BidStatus");
                    bkLmpH.ColumnMappings.Add("OasisID", "OasisID");
                    bkLmpH.ColumnMappings.Add("EndMarketDateTime", "EndMarketDateTime");
                    bkLmpH.ColumnMappings.Add("RequestedMW", "RequestedMW");
                    bkLmpH.ColumnMappings.Add("ClearedMW", "ClearedMW");
                    bkLmpH.ColumnMappings.Add("EndUserKey", "EndUserKey");
                    bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                    bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                    bkLmpH.ColumnMappings.Add("POR", "POR");
                    bkLmpH.ColumnMappings.Add("POD", "POD");
                    bkLmpH.ColumnMappings.Add("Source", "Source");
                    bkLmpH.ColumnMappings.Add("Sink", "Sink");
                    bkLmpH.ColumnMappings.Add("Price", "Price");
                    bkLmpH.ColumnMappings.Add("PortfolioKey", "PortfolioKey");
                    bkLmpH.ColumnMappings.Add("SubmittedDateTime", "SubmittedDateTime");
                    bkLmpH.ColumnMappings.Add("Comments", "Comments");
                    bkLmpH.WriteToServer(mDatatableBids);
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            VayuConnection.Close();
            mDatatableBids.Clear();

        }


        public void InsertBidsToTest(System.Collections.Generic.List<Vayu.DBLibrary.Portfolio> portfoliolist, DateTime sdate, string Market)
        {
            string TableName = string.Empty;
            int PortfolioKey; //999;
            string id = "";
            loadDBCommands();
            FillMultiplierHash();
            DataSet ds;
            SqlDataAdapter adapter;
            foreach (var item in portfoliolist)
            {
                if (Market == "PJM")
                {
                    PortfolioKey = 420;
                    if (VayuConnection.State == ConnectionState.Open)
                    {
                        VayuConnection.Close();
                    }
                    VayuConnection.Open();
                    mDeleteEESBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                    mDeleteEESBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                    mDeleteEESBidsCommand.Parameters["@PortfolioKey"].Value = PortfolioKey;
                    int count = mDeleteEESBidsCommand.ExecuteNonQuery();
                    VayuConnection.Close();
                }
                else
                {
                    PortfolioKey = 2001;
                    if (VayuConnection.State == ConnectionState.Open)
                    {
                        VayuConnection.Close();
                    }
                    VayuConnection.Open();
                    mDeleteErcotPTPBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                    mDeleteErcotPTPBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                    mDeleteErcotPTPBidsCommand.Parameters["@PortfolioKey"].Value = PortfolioKey;
                    int count = mDeleteErcotPTPBidsCommand.ExecuteNonQuery();
                    VayuConnection.Close();
                }
                try
                {
                    if (Market == "PJM")
                    {
                        TableName = "[dbo].[eesbids]"; // eesbidstemp
                        if (VayuConnection.State == ConnectionState.Open)
                        {
                            VayuConnection.Close();
                        }
                        VayuConnection.Open();
                        adapter = new SqlDataAdapter();
                        ds = new DataSet();
                        mSelectEESBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectEESBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectEESBidsCommand.Parameters["@PortfolioKey"].Value = item.ID;
                        adapter.SelectCommand = mSelectEESBidsCommand;
                        adapter.Fill(ds);
                        //mDatatableBids.Clear();
                    }
                    else
                    {
                        TableName = "Vayu..ErcotPTPBids";//  ErcotPTPBidstemp
                        if (VayuConnection.State == ConnectionState.Open)
                        {
                            VayuConnection.Close();
                        }
                        VayuConnection.Open();
                        adapter = new SqlDataAdapter();
                        ds = new DataSet();
                        mSelectErcotPTPBidsCommand.Parameters["@startdate"].Value = sdate.ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectErcotPTPBidsCommand.Parameters["@enddate"].Value = sdate.AddDays(1).ToString("yyyy-MM-dd 00:00:00.000");
                        mSelectErcotPTPBidsCommand.Parameters["@PortfolioKey"].Value = item.ID;
                        adapter.SelectCommand = mSelectErcotPTPBidsCommand;
                        adapter.Fill(ds);
                    }
                    int mcount = ds.Tables[0].Rows.Count;
                    if (mcount > 0)
                    {
                        for (int i = 0; i < mcount; i++)
                        {
                            int portfolio = Convert.ToInt32(ds.Tables[0].Rows[i].ItemArray[14]);
                            double multiplier = 0;
                            if (mMultiplierHash.ContainsKey(portfolio))
                            {
                                multiplier = mMultiplierHash[portfolio];
                            }
                            double finalMultiplier = multiplier + 1;
                            if (Market == "ERCOT")
                            {
                                finalMultiplier = multiplier;
                            }
                            DataRow mDatarowBids = mDatatableBids.NewRow();
                            // var tempBidId = GetBidId(999);
                            mDatarowBids["ScheduleID"] = DateTime.Now.Ticks + i + ".1"; ;
                            mDatarowBids["BidStatus"] = ds.Tables[0].Rows[i].ItemArray[1];
                            mDatarowBids["OasisID"] = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[2]);
                            mDatarowBids["EndMarketDateTime"] = Convert.ToDateTime(ds.Tables[0].Rows[i].ItemArray[3]);
                            mDatarowBids["RequestedMW"] = (Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[4].ToString()), 2) * finalMultiplier);
                            mDatarowBids["ClearedMW"] = (Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[5].ToString()), 2) * finalMultiplier);
                            mDatarowBids["EndUserKey"] = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[6]);
                            mDatarowBids["SourceNodeKey"] = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[7]);
                            mDatarowBids["SinkNodeKey"] = (int)Convert.ToDecimal(ds.Tables[0].Rows[i].ItemArray[8]);
                            if (ds.Tables[0].Rows[i].ItemArray[9] == "")
                                mDatarowBids["POR"] = DBNull.Value;
                            else
                                mDatarowBids["POR"] = ds.Tables[0].Rows[i].ItemArray[9];
                            if (ds.Tables[0].Rows[i].ItemArray[10] == "")
                                mDatarowBids["POD"] = DBNull.Value;
                            else
                                mDatarowBids["POD"] = ds.Tables[0].Rows[i].ItemArray[10];
                            mDatarowBids["Source"] = ds.Tables[0].Rows[i].ItemArray[11];
                            mDatarowBids["Sink"] = ds.Tables[0].Rows[i].ItemArray[12];
                            mDatarowBids["Price"] = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i].ItemArray[13].ToString()), 2);
                            mDatarowBids["PortfolioKey"] = PortfolioKey;
                            mDatarowBids["SubmittedDateTime"] = Convert.ToDateTime(ds.Tables[0].Rows[i].ItemArray[15]);
                            if (ds.Tables[0].Rows[i].ItemArray[16] == "")
                                mDatarowBids["Comments"] = DBNull.Value;
                            else
                                mDatarowBids["Comments"] = ds.Tables[0].Rows[i].ItemArray[16];
                            mDatatableBids.Rows.Add(mDatarowBids);
                        }
                        adapter.Dispose();
                        mSelectEESBidsCommand.Dispose();
                        VayuConnection.Close();
                        if (VayuConnection.State == ConnectionState.Open)
                        {
                            VayuConnection.Close();
                        }
                        VayuConnection.Open();
                        //mDeleteEESBidsTestommand.ExecuteNonQuery();
                        SqlTransaction transaction = VayuConnection.BeginTransaction();
                        using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuConnection, SqlBulkCopyOptions.TableLock, transaction))
                        {
                            try
                            {
                                // bkLmpH.DestinationTableName = "[dbo].[eesbids_test]";
                                bkLmpH.DestinationTableName = TableName;
                                bkLmpH.ColumnMappings.Add("ScheduleID", "ScheduleID");
                                bkLmpH.ColumnMappings.Add("BidStatus", "BidStatus");
                                bkLmpH.ColumnMappings.Add("OasisID", "OasisID");
                                bkLmpH.ColumnMappings.Add("EndMarketDateTime", "EndMarketDateTime");
                                bkLmpH.ColumnMappings.Add("RequestedMW", "RequestedMW");
                                bkLmpH.ColumnMappings.Add("ClearedMW", "ClearedMW");
                                bkLmpH.ColumnMappings.Add("EndUserKey", "EndUserKey");
                                bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                                bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                                bkLmpH.ColumnMappings.Add("POR", "POR");
                                bkLmpH.ColumnMappings.Add("POD", "POD");
                                bkLmpH.ColumnMappings.Add("Source", "Source");
                                bkLmpH.ColumnMappings.Add("Sink", "Sink");
                                bkLmpH.ColumnMappings.Add("Price", "Price");
                                bkLmpH.ColumnMappings.Add("PortfolioKey", "PortfolioKey");
                                bkLmpH.ColumnMappings.Add("SubmittedDateTime", "SubmittedDateTime");
                                bkLmpH.ColumnMappings.Add("Comments", "Comments");
                                bkLmpH.WriteToServer(mDatatableBids);
                                //SqlCommand cmdUpdatenodelmph = new SqlCommand("[dbo].[UpMergeesbids]", VayuDbConn, transaction);
                                //cmdUpdatenodelmph.CommandType = CommandType.StoredProcedure;
                                //cmdUpdatenodelmph.CommandTimeout = 30000;
                                //cmdUpdatenodelmph.ExecuteNonQuery();
                                transaction.Commit();

                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                            }
                        }
                        VayuConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    //if (reader != null)
                    //    reader.Close();
                    VayuConnection.Close();
                }
            }

        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        private BigInteger GetBidId(int portfolio)
        {
            DateTime now = DateTime.Now;
            BigInteger tempBidId = BigInteger.Parse(portfolio.ToString() + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() +
            now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + now.Millisecond.ToString());
            return tempBidId;
        }


        #region "getobject filled object with property reconized"

        public List<T> ConvertTo<T>(DataTable datatable) where T : new()
        {
            List<T> Temp = new List<T>();
            try
            {
                List<string> columnsNames = new List<string>();
                foreach (DataColumn DataColumn in datatable.Columns)
                    columnsNames.Add(DataColumn.ColumnName);
                Temp = datatable.AsEnumerable().ToList().ConvertAll<T>(row => getObject<T>(row, columnsNames));
                return Temp;
            }
            catch
            {
                return Temp;
            }

        }
        public T getObject<T>(DataRow row, List<string> columnsName) where T : new()
        {
            T obj = new T();
            try
            {
                string columnname = "";
                string value = "";
                PropertyInfo[] Properties;
                Properties = typeof(T).GetProperties();
                foreach (PropertyInfo objProperty in Properties)
                {
                    columnname = columnsName.Find(name => name.ToLower() == objProperty.Name.ToLower());
                    if (!string.IsNullOrEmpty(columnname))
                    {
                        value = row[columnname].ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
                            {
                                value = row[columnname].ToString().Replace("$", "").Replace(",", "");
                                objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
                            }
                            else
                            {
                                value = row[columnname].ToString().Replace("%", "");
                                objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
                            }
                        }
                    }
                }
                return obj;
            }
            catch
            {
                return obj;
            }
        }

        #endregion


        #region "New DataTable"
        public DataTable ToDataTable<T>(IEnumerable<T> collection)
        {
            DataTable newDataTable = new DataTable();
            Type impliedType = typeof(T);
            PropertyInfo[] _propInfo = impliedType.GetProperties();
            foreach (PropertyInfo pi in _propInfo)
                newDataTable.Columns.Add(pi.Name, pi.PropertyType);

            foreach (T item in collection)
            {
                DataRow newDataRow = newDataTable.NewRow();
                newDataRow.BeginEdit();
                foreach (PropertyInfo pi in _propInfo)
                    newDataRow[pi.Name] = pi.GetValue(item, null);
                newDataRow.EndEdit();
                newDataTable.Rows.Add(newDataRow);
            }
            return newDataTable;
        }
        #endregion
    }
}
