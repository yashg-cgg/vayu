using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Vayu.PNLCalculationLibrary;
using System.ServiceModel;
using System.Data.SqlClient;
using Vayu.NodePriceLibrary;
using System.Threading;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Collections.Concurrent;

namespace Vayu.PNLDownload
{
    /// <summary>
    /// download the Virtual Pnl
    /// </summary>
    public class VirtualPnlDownload
    {
        #region Private Members
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuDbConnection;
        /// <summary>
        /// The Vayu database connectionb
        /// </summary>
       
        /// <summary>
        /// The Vayu database connectionc
        /// </summary>
       
        /// <summary>
        /// The select portfolio command
        /// </summary>
        private SqlCommand mSelectPortfolioCommand;
        /// <summary>
        /// The select portfolio detail command
        /// </summary>
        private SqlCommand mSelectPortfolioDetailCommand;
        /// <summary>
        /// The select rt command
        /// </summary>
        private SqlCommand mSelectRTCommand;
        /// <summary>
        /// The select da command
        /// </summary>
        private SqlCommand mSelectDACommand;
        /// <summary>
        /// The select caiso rt command
        /// </summary>
        private SqlCommand mSelectCaisoRTCommand;
        /// <summary>
        /// The select caiso hart command
        /// </summary>
        private SqlCommand mSelectCaisoHARTCommand;
        /// <summary>
        /// The select caiso da command
        /// </summary>
        private SqlCommand mSelectCaisoDACommand;
        /// <summary>
        /// The insert virtaul PNL command
        /// </summary>
        private SqlCommand mInsertVirtaulPnlCommand;
        /// <summary>
        /// The delete virtaul PNL command
        /// </summary>
        private SqlCommand mDeleteVirtaulPnlCommand;

        private SqlCommand mUpdateVirtaulPnlCommand;
        /// <summary>
        /// The select virt portfolio command
        /// </summary>
        private SqlCommand mSelectVirtPortfolioCommand;
        /// <summary>
        /// The select virt portfolio detail command
        /// </summary>
        private SqlCommand mSelectVirtPortfolioDetailCommand;
        /// <summary>
        /// The select caiso port folio command
        /// </summary>
        private SqlCommand mSelectCaisoPortfolioCommand;
        /// <summary>
        /// The select node type command
        /// </summary>
        private SqlCommand mSelectNodeTypeCommand;
        /// <summary>
        /// The select market command
        /// </summary>
        private SqlCommand mSelectMarketCommand;
        /// <summary>
        /// The start date
        /// </summary>
        //private DateTime mStartDate = DateTime.Today.AddMonths(-3);
        private DateTime mStartDate = new DateTime(2023, 8, 01);
        /// <summary>
        /// The realtime hash
        /// </summary>
        private static Dictionary<string, double> sRTHash = new Dictionary<string, double>();
        /// <summary>
        /// The dayahead hash
        /// </summary>
        private static Dictionary<string, double> sDAHash = new Dictionary<string, double>();
        private string pnlTableName;

        double DollerCleared = 0;
        #endregion

        #region Public Member
        /// <summary>
        /// The node detail hash
        /// </summary>
        public static Dictionary<int, NodeDetail> sNodeDetailHash = new Dictionary<int, NodeDetail>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the database related object.
        /// </summary>
        public void InitDB()
        {

            //
            mSelectVirtPortfolioCommand = new SqlCommand();
            mSelectVirtPortfolioCommand.CommandText = "select distinct portfoliokey from ClearedBids where MarketDateTime > @startDate and MarketDateTime <= @endDate and PortfolioKey in(select distinct PORTFOLIO_ID from PORTFOLIO where ACCOUNT in(select account_id from account where PJM_LOGIN is not null and account_id in(select distinct account from portfolio where product='VIRTUAL' and hub='PJM')  ))";
            mSelectVirtPortfolioCommand.Parameters.AddWithValue("@startDate", "MarketDateTime");
            mSelectVirtPortfolioCommand.Parameters.AddWithValue("@endDate", "MarketDateTime");
            //
            mSelectVirtPortfolioDetailCommand = new SqlCommand();
            mSelectVirtPortfolioDetailCommand.CommandText = "select marketDateTime, NodeKey, clearedmw, incdec from ClearedBids where MarketDateTime > @startDate and MarketDateTime <= @endDate and portfolioKey = @portfolioKey and PortfolioKey in(select distinct PORTFOLIO_ID from PORTFOLIO where ACCOUNT in (select account_id from account where PJM_LOGIN is not null and account_id in(select distinct account from portfolio where product='VIRTUAL' and hub='PJM')  ))";
            mSelectVirtPortfolioDetailCommand.Parameters.AddWithValue("@startDate", "MarketDateTime");
            mSelectVirtPortfolioDetailCommand.Parameters.AddWithValue("@endDate", "MarketDateTime");
            mSelectVirtPortfolioDetailCommand.Parameters.AddWithValue("@portfolioKey", "portfolioKey");
            //
            mSelectPortfolioCommand = new SqlCommand();
            mSelectPortfolioCommand.CommandText = "select distinct portfoliokey from ClearedEES where MarketDateTime > @startDate and MarketDateTime <= @endDate and PortfolioKey in(select distinct PORTFOLIO_ID from PORTFOLIO where ACCOUNT in(select account_id from account where PJM_LOGIN is not null and account_id in(select distinct account from portfolio where product='EES/PTP' and hub='PJM')  ))";
            mSelectPortfolioCommand.Parameters.AddWithValue("@startDate", "MarketDateTime");
            mSelectPortfolioCommand.Parameters.AddWithValue("@endDate", "MarketDateTime");
            //
            mSelectPortfolioDetailCommand = new SqlCommand();
            mSelectPortfolioDetailCommand.CommandText = "select marketDateTime, sourcenodekey, sinknodekey, clearedmw from ClearedEES where MarketDateTime > @startDate and MarketDateTime <= @endDate and portfolioKey = @portfolioKey and PortfolioKey in(select distinct PORTFOLIO_ID from PORTFOLIO where ACCOUNT in(select account_id from account where PJM_LOGIN is not null and account_id in(select distinct account from portfolio where product='EES/PTP' and hub='PJM')  ))";
            mSelectPortfolioDetailCommand.Parameters.AddWithValue("@startDate", "MarketDateTime");
            mSelectPortfolioDetailCommand.Parameters.AddWithValue("@endDate", "MarketDateTime");
            mSelectPortfolioDetailCommand.Parameters.AddWithValue("@portfolioKey", "portfolioKey");
            //
            mSelectRTCommand = new SqlCommand();
            mSelectRTCommand.CommandText = "select lmp from nodelmph (NOLOCK) where nodekey = @nodekey and marketdatetime = @marketdatetime";
            mSelectRTCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectRTCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            //
            mSelectCaisoRTCommand = new SqlCommand();
            mSelectCaisoRTCommand.CommandText = "select lmp from CAISO.nodelmph (NOLOCK) where nodekey = @nodekey and marketdatetime = @marketdatetime";
            mSelectCaisoRTCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectCaisoRTCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            //
            mSelectCaisoHARTCommand = new SqlCommand();
            mSelectCaisoHARTCommand.CommandText = "select lmp from CAISO.nodehalmph (NOLOCK) where nodekey = @nodekey and marketdatetime = @marketdatetime";
            mSelectCaisoHARTCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectCaisoHARTCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            //
            mSelectDACommand = new SqlCommand();
            mSelectDACommand.CommandText = "select lmp from NodeDALMPH (NOLOCK) where nodekey = @nodekey and marketdatetime = @marketdatetime";
            mSelectDACommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectDACommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            //
            mSelectCaisoDACommand = new SqlCommand();
            mSelectCaisoDACommand.CommandText = "select lmp from CAISO.NodeDALMPH (NOLOCK) where nodekey = @nodekey and marketdatetime = @marketdatetime";
            mSelectCaisoDACommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mSelectCaisoDACommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            //
            mInsertVirtaulPnlCommand = new SqlCommand();
            mInsertVirtaulPnlCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mInsertVirtaulPnlCommand.Parameters.AddWithValue("@pnldate", "pnldate");
            mInsertVirtaulPnlCommand.Parameters.AddWithValue("@pnl", "pnl");
            mInsertVirtaulPnlCommand.Parameters.AddWithValue("@pnlUpdateTime", "pnlUpdateTime");
            mInsertVirtaulPnlCommand.Parameters.AddWithValue("@MW", "MW");
            mInsertVirtaulPnlCommand.Parameters.AddWithValue("@DollarCleared", "DollarCleared");
            //
            mDeleteVirtaulPnlCommand = new SqlCommand();
            mDeleteVirtaulPnlCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mDeleteVirtaulPnlCommand.Parameters.AddWithValue("@pnldate", "pnldate");
            //
            mSelectCaisoPortfolioCommand = new SqlCommand();
            mSelectCaisoPortfolioCommand.CommandText = "select portfoliokey from Portfolio where Market = 7";
            //
            mSelectNodeTypeCommand = new SqlCommand();
            mSelectNodeTypeCommand.CommandText = "select nodetypekey from node where nodekey = @nodekey";
            mSelectNodeTypeCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            //
            mSelectMarketCommand = new SqlCommand();
            mSelectMarketCommand.CommandText = "select market from portfolio where portfoliokey = @portfoliokey";
            mSelectMarketCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            //

        }
        /// <summary>
        /// Gets the uptos PNL.
        /// </summary>
        public void GetUptosPnl(string iso)
        {
            InitDB();
              if (iso == "ERCOT")
                pnlTableName = "Vayu_ercot..DailyPnl";
            try
            {
                VayuDbConnection = new SqlConnection(CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
                
                DateTime startDate = mStartDate;
                //  DateTime endDate = DateTime.Parse(DateTime.Today.AddDays(-1).ToShortDateString());
                DateTime endDate = DateTime.Parse(DateTime.Today.ToShortDateString());
               // DateTime endDate = new DateTime(2019, 4, 21);

                while (startDate <= endDate)
                {
                  
                    if (VayuDbConnection.State == ConnectionState.Open)
                    {
                        VayuDbConnection.Close();
                    }
                    VayuDbConnection.Open();
                    mSelectPortfolioCommand.Connection = VayuDbConnection;
                    if (iso == "ERCOT")
                    {
                        mSelectPortfolioCommand.CommandText = "select distinct portfoliokey from Vayu_ercot..ClearedEES where MarketDateTime > @startDate and MarketDateTime <= @endDate and PortfolioKey in(select distinct PORTFOLIO_ID from PORTFOLIO where ACCOUNT in(select account_id from account where  account_id in(select distinct account from portfolio where product='EES/PTP' and hub='ERCOT')  ))";
                    }
                    mSelectPortfolioCommand.Parameters["@startDate"].Value = startDate;
                    DateTime sendEndDate = startDate.AddDays(1);
                    mSelectPortfolioCommand.Parameters["@endDate"].Value = sendEndDate;
                    SqlDataReader reader = mSelectPortfolioCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        double totalPnl = 0;
                        DollerCleared = 0;
                        int portfolioId = Convert.ToInt32(reader.GetValue(0));
                        mSelectPortfolioDetailCommand.Connection = VayuDbConnection;
                        if (iso == "ERCOT")
                        {
                            mSelectPortfolioDetailCommand.CommandText = "select marketDateTime, sourcenodekey, sinknodekey, clearedmw from Vayu_ercot..ClearedEES where MarketDateTime > @startDate and MarketDateTime <= @endDate and portfolioKey = @portfolioKey and PortfolioKey in(select distinct PORTFOLIO_ID from PORTFOLIO where ACCOUNT in(select account_id from account where  account_id in(select distinct account from portfolio where product='EES/PTP' and hub='ERCOT')  ))";
                        }
                        mSelectPortfolioDetailCommand.Parameters["@startDate"].Value = startDate;
                        mSelectPortfolioDetailCommand.Parameters["@endDate"].Value = sendEndDate;
                        mSelectPortfolioDetailCommand.Parameters["@portfolioKey"].Value = portfolioId;
                        SqlDataReader reader1 = mSelectPortfolioDetailCommand.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(reader1);
                        reader1.Close();
                        int market = 0;
                        mSelectMarketCommand.Connection = VayuDbConnection;
                        //  mSelectMarketCommand.Parameters["@portfoliokey"].Value = portfolioId;
                        //  reader1 = mSelectMarketCommand.ExecuteReader();
                        // while (reader1.Read())
                        {
                             if (iso == "ERCOT")
                                market = 9;
                        }
                        //    reader1.Close();
                        Dictionary<int, List<DateTime>> nodeHash = new Dictionary<int, List<DateTime>>();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DateTime marketDate = Convert.ToDateTime(dt.Rows[i][0]);
                            Int32 sourceKey = Convert.ToInt32(dt.Rows[i][1]);
                            Int32 sinkKey = Convert.ToInt32(dt.Rows[i][2]);
                            List<DateTime> sourceMarketDateTimeList = new List<DateTime>();
                            if (nodeHash.ContainsKey(sourceKey))
                            {
                                sourceMarketDateTimeList = nodeHash[sourceKey];
                                nodeHash.Remove(sourceKey);
                            }
                            if (!sourceMarketDateTimeList.Contains(marketDate))
                            {
                                sourceMarketDateTimeList.Add(marketDate);
                            }
                            nodeHash.Add(sourceKey, sourceMarketDateTimeList);
                            List<DateTime> sinkMarketDateTimeList = new List<DateTime>();
                            if (nodeHash.ContainsKey(sinkKey))
                            {
                                sinkMarketDateTimeList = nodeHash[sinkKey];
                                nodeHash.Remove(sinkKey);
                            }
                            if (!sinkMarketDateTimeList.Contains(marketDate))
                            {
                                sinkMarketDateTimeList.Add(marketDate);
                            }
                            nodeHash.Add(sinkKey, sinkMarketDateTimeList);
                        }
                        SetDARTHash(nodeHash, market);
                        double totalmw = 0;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DateTime marketDate = Convert.ToDateTime(dt.Rows[i][0]);
                            Int32 sourceKey = Convert.ToInt32(dt.Rows[i][1]);
                            Int32 sinkKey = Convert.ToInt32(dt.Rows[i][2]);
                            double mw = Convert.ToDouble(dt.Rows[i][3]);
                            totalmw += mw;
                            totalPnl += CalculatePnl(marketDate, sourceKey, sinkKey, mw);
                            Logeriter.writeLog("calculating pnl " + totalPnl.ToString() + "\t date" + marketDate + "\t MW" + mw);
                        }
                        try
                        {
                            if (totalPnl != 0)
                            {
                                //if (VayuDbConnection.State == ConnectionState.Open)
                                //{
                                //   // VayuDbConnection.Close();
                                //}
                                //VayuDbConnection.Open();
                                mUpdateVirtaulPnlCommand = VayuDbConnection.CreateCommand();
                                mUpdateVirtaulPnlCommand.CommandText = "update " + pnlTableName + " set pnl = " + totalPnl 
                                    + "  , pnlUpdateTime = '" + DateTime.Now.ToString() + "' , MW = '" + totalmw + "' , DollarsCleared = '" + DollerCleared
                                    + "' where PnlDate = '" + startDate.ToString() 
                                    + "' and PortfolioKey =  " + portfolioId;
                                int count = mUpdateVirtaulPnlCommand.ExecuteNonQuery();

                                //mDeleteVirtaulPnlCommand.CommandText = "delete from  " + pnlTableName + " where portfoliokey = @portfoliokey and pnldate = @pnldate";
                                //mDeleteVirtaulPnlCommand.Connection = VayuDbConnectionc;
                                //mDeleteVirtaulPnlCommand.Parameters["@portfoliokey"].Value = portfolioId;
                                //mDeleteVirtaulPnlCommand.Parameters["@pnldate"].Value = startDate;
                                //mDeleteVirtaulPnlCommand.ExecuteNonQuery();
                                if (count == 0)
                                {
                                    mInsertVirtaulPnlCommand.CommandText = "insert into " + pnlTableName + " values (@portfoliokey, @pnldate, @pnl, 0, @pnlUpdateTime, null, @MW, @DollarCleared)";
                                    mInsertVirtaulPnlCommand.Connection = VayuDbConnection;
                                    mInsertVirtaulPnlCommand.Parameters["@portfoliokey"].Value = portfolioId;
                                    mInsertVirtaulPnlCommand.Parameters["@pnldate"].Value = startDate;
                                    mInsertVirtaulPnlCommand.Parameters["@pnl"].Value = totalPnl;
                                    mInsertVirtaulPnlCommand.Parameters["@pnlUpdateTime"].Value = DateTime.Now;
                                    mInsertVirtaulPnlCommand.Parameters["@MW"].Value = totalmw;
                                    mInsertVirtaulPnlCommand.Parameters["@DollarCleared"].Value = DollerCleared;
                                    mInsertVirtaulPnlCommand.ExecuteNonQuery();
                                   // VayuDbConnection.Close();
                                }
                                
                                Logeriter.writeLog("inserting virtual pnl" + "portfoliokey" + portfolioId + "\tStartDate" + startDate + "\t totalPnl" + totalPnl);
                                Console.WriteLine("Inserting inserting virtual pnl");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logeriter.writeLog(ex.Message);
                        }
                    }
                    startDate = startDate.AddDays(1);
                    reader.Close();
                    VayuDbConnection.Close();
                     
                }
            }
            catch (Exception ex)
            {
                Logeriter.writeLog(ex.Message);
            }
        }
        /// <summary>
        /// Sets the dart hash.
        /// </summary>
        /// <param name="nodeHash">The node hash.</param>
        /// <param name="market">The market.</param>
        public static void SetDARTHash(Dictionary<int, List<DateTime>> nodeHash, int market)
        {
            List<int> nodeKeyList = nodeHash.Keys.ToList<int>();
            List<Node> daNodeList = new List<Node>();
            List<Node> rtNodeList = new List<Node>();
            foreach (int nodeKey in nodeKeyList)
            {
                List<DateTime> marketDateList = nodeHash[nodeKey];
                if (!sNodeDetailHash.ContainsKey(nodeKey))
                {

                    SqlConnection connection90 = new SqlConnection(CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
                    if (connection90.State == ConnectionState.Open)
                    {
                        connection90.Close();
                    }
                    connection90.Open();
                    SqlCommand selectNodeCommand = new SqlCommand();
                    selectNodeCommand = new SqlCommand();
                    selectNodeCommand.CommandText = "select nodename, externalnodeid from Node where NodeKey=@Nodekey";
                    selectNodeCommand.Parameters.AddWithValue("@Nodekey", "Nodekey");
                    selectNodeCommand.Connection = connection90;
                    selectNodeCommand.Parameters["@Nodekey"].Value = nodeKey;
                    SqlDataReader reader = selectNodeCommand.ExecuteReader();
                    string name = "";
                    long pNode = 0;
                    while (reader.Read())
                    {
                        name = reader.GetString(0);
                        if (market == 1)
                        {
                            pNode = (long)reader.GetDecimal(1);
                        }
                    }
                    reader.Close();
                    connection90.Close();
                    NodeDetail nodeDetail1 = new NodeDetail();
                    nodeDetail1.MarketKey = market;
                    nodeDetail1.NodeName = name;
                    
                        nodeDetail1.PNode = nodeKey;
                    
                    sNodeDetailHash.Add(nodeKey, nodeDetail1);
                }
                NodeDetail nodeDetail = sNodeDetailHash[nodeKey];
                Node daNode = new Node();
                daNode.NodeId = nodeKey;
                daNode.PNodeId = nodeDetail.PNode;
                daNode.Market = nodeDetail.MarketKey;
                Node rtNode = new Node();
                rtNode.NodeId = nodeKey;
                rtNode.PNodeId = nodeDetail.PNode;
                rtNode.Market = nodeDetail.MarketKey;
                List<TimePrice> daTimeList = new List<TimePrice>();
                List<TimePrice> rtTimeList = new List<TimePrice>();
                foreach (DateTime marketDate in marketDateList)
                {
                    string hourKey = marketDate.ToString() + nodeKey.ToString();
                    if (!sDAHash.ContainsKey(hourKey))
                    {
                        TimePrice time = new TimePrice();
                        time.Price = double.NaN;
                        time.MarketTime = marketDate;
                        daTimeList.Add(time);
                    }
                    if (DateTime.Today < marketDate)
                    {
                        sRTHash.Remove(hourKey);
                    }
                    if (!sRTHash.ContainsKey(hourKey))
                    {
                        TimePrice time = new TimePrice();
                        time.Price = double.NaN;
                        time.MarketTime = marketDate;
                        rtTimeList.Add(time);
                    }
                }
                if (daTimeList.Count > 0)
                {
                    daNode.TimePriceList = daTimeList;
                    daNodeList.Add(daNode);
                }
                if (rtTimeList.Count > 0)
                {
                    rtNode.TimePriceList = rtTimeList;
                    rtNodeList.Add(rtNode);
                }
            }
            Thread thread = null;
            Thread thread1 = null;
            if (daNodeList.Count > 0)
            {
                DARTThread range = new DARTThread(daNodeList, sDAHash, true);
                thread = new Thread(new ThreadStart(range.Run));
                thread.Start();
            }
            if (rtNodeList.Count > 0)
            {
                DARTThread range = new DARTThread(rtNodeList, sRTHash, false);
                thread1 = new Thread(new ThreadStart(range.Run));
                thread1.Start();
            }
            if (thread != null)
            {
                thread.Join();
            }
            if (thread1 != null)
            {
                thread1.Join();
            }
        }
        /// <summary>
        /// Gets the virtual PNL.
        /// </summary>
        public void GetVirtualPnl()
        {
            DateTime startDate = mStartDate;
            DateTime endDate = DateTime.Parse(DateTime.Today.AddDays(-1).ToShortDateString());

           
            VayuDbConnection = new SqlConnection(CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            if (VayuDbConnection.State == ConnectionState.Open)
            {
                VayuDbConnection.Close();
            }
            VayuDbConnection.Open();

            if (VayuDbConnection.State == ConnectionState.Open)
            {
                VayuDbConnection.Close();
            }
            VayuDbConnection.Open();
            while (startDate <= endDate)
            {
                mSelectVirtPortfolioCommand.Connection = VayuDbConnection;
                mSelectVirtPortfolioCommand.Parameters["@startDate"].Value = startDate;
                DateTime sendEndDate = startDate.AddDays(1);
                mSelectVirtPortfolioCommand.Parameters["@endDate"].Value = sendEndDate;
                SqlDataReader reader = mSelectVirtPortfolioCommand.ExecuteReader();
                while (reader.Read())
                {
                    double totalPnl = 0;
                    int portfolioId = Convert.ToInt32(reader.GetValue(0));
                    int market = 0;
                    //mSelectMarketCommand.Connection = VayuDbConnectionb;
                    //mSelectMarketCommand.Parameters["@portfoliokey"].Value = portfolioId;
                    // SqlDataReader reader1 = mSelectMarketCommand.ExecuteReader();
                    market = 1;
                    ;
                    mSelectVirtPortfolioDetailCommand.Connection = VayuDbConnection;
                    mSelectVirtPortfolioDetailCommand.Parameters["@startDate"].Value = startDate;
                    mSelectVirtPortfolioDetailCommand.Parameters["@endDate"].Value = sendEndDate;
                    mSelectVirtPortfolioDetailCommand.Parameters["@portfolioKey"].Value = portfolioId;
                    SqlDataReader reader1 = mSelectVirtPortfolioDetailCommand.ExecuteReader();
                    Dictionary<int, List<DateTime>> nodeHash = new Dictionary<int, List<DateTime>>();
                    while (reader1.Read())
                    {
                        DateTime marketDate = reader1.GetDateTime(0);
                        int sourceKey = (int)reader1.GetDecimal(1);
                        double mw = (double)reader1.GetDecimal(2);
                        bool isInc = "INC".Equals(reader1.GetString(3).Trim().ToUpper());
                        List<DateTime> marketDateTimeList = new List<DateTime>();
                        if (nodeHash.ContainsKey(sourceKey))
                        {
                            marketDateTimeList = nodeHash[sourceKey];
                            nodeHash.Remove(sourceKey);
                        }
                        if (!marketDateTimeList.Contains(marketDate))
                        {
                            marketDateTimeList.Add(marketDate);
                        }
                        nodeHash.Add(sourceKey, marketDateTimeList);
                    }
                    reader1.Close();
                    reader1 = mSelectVirtPortfolioDetailCommand.ExecuteReader();
                    SetDARTHash(nodeHash, market);
                    while (reader1.Read())
                    {
                        DateTime marketDate = reader1.GetDateTime(0);
                        int sourceKey = (int)reader1.GetDecimal(1);
                        double mw = (double)reader1.GetDecimal(2);
                        string incdec = reader1.GetString(3).Trim().ToUpper();
                        totalPnl += CalculateVirtualPnl(marketDate, sourceKey, mw, incdec.StartsWith("I"));
                        Logeriter.writeLog("total pnl " + totalPnl + "\t MW" + mw + "\tIsInc" + incdec.StartsWith("I"));
                    }
                    reader1.Close();
                    try
                    {
                        if (totalPnl != 0)
                        {
                            VayuDbConnection = new SqlConnection(CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
                            if (VayuDbConnection.State == ConnectionState.Open)
                            {
                                VayuDbConnection.Close();
                            }
                            VayuDbConnection.Open();
                            mUpdateVirtaulPnlCommand = VayuDbConnection.CreateCommand();
                            mUpdateVirtaulPnlCommand.CommandText = "update NewTrading..VIRTUAL_PNL set pnl = " + totalPnl + "  , pnlUpdateTime = '" + DateTime.Now.ToString() +"' where PnlDate = '" + startDate.ToString()  +"' and PortfolioKey =  " + portfolioId;
                            int count = mUpdateVirtaulPnlCommand.ExecuteNonQuery();
                            if (count == 0 )
                            {
                                mInsertVirtaulPnlCommand.Connection = VayuDbConnection;
                                mInsertVirtaulPnlCommand.Parameters["@portfoliokey"].Value = portfolioId;
                                mInsertVirtaulPnlCommand.Parameters["@pnldate"].Value = startDate;
                                mInsertVirtaulPnlCommand.Parameters["@pnl"].Value = totalPnl;
                                mInsertVirtaulPnlCommand.Parameters["@pnlUpdateTime"].Value = DateTime.Now;
                                mInsertVirtaulPnlCommand.ExecuteNonQuery(); 
                            }
                            VayuDbConnection.Close();
                            Logeriter.writeLog("portfoliokey=" + portfolioId + "\t date" + startDate + "totalPnl" + totalPnl);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logeriter.writeLog(ex.Message);
                    }
                }
                reader.Close();
                startDate = startDate.AddDays(1);
            }
            VayuDbConnection.Close();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Calculates the PNL.
        /// </summary>
        /// <param name="marketDate">The market date.</param>
        /// <param name="source">The source.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="mw">The mw.</param>
        /// <returns>PNL</returns>
        private double CalculatePnl(DateTime marketDate, int source, int sink, double mw)
        {
            Console.WriteLine("calculating pnl marketDate = " + marketDate);
            double pnl = 0;
            double sourceRT = 0;
            double sourceDA = 0;
            double sinkRT = 0;
            double sinkDA = 0;
            double DCleared = 0;
            string sourceKey = marketDate.ToString() + source.ToString();
            string sinkKey = marketDate.ToString() + sink.ToString();
            if (sRTHash.ContainsKey(sourceKey) && sRTHash.ContainsKey(sinkKey) && sDAHash.ContainsKey(sourceKey) && sDAHash.ContainsKey(sinkKey))
            {
                sourceRT = sRTHash[sourceKey];
                sinkRT = sRTHash[sinkKey];
                sourceDA = sDAHash[sourceKey];
                sinkDA = sDAHash[sinkKey];
                double da = sinkDA - sourceDA;
                double rt = sinkRT - sourceRT;
                pnl = (rt - da) * mw;
            }
            if (sDAHash.ContainsKey(sourceKey) && sDAHash.ContainsKey(sinkKey))
            {
                sourceDA = sDAHash[sourceKey];
                sinkDA = sDAHash[sinkKey];
                double da = sinkDA - sourceDA;
                DCleared = da * mw;
                DollerCleared += DCleared;
            }
            Logeriter.writeLog("calculating pnl " + pnl.ToString() + "\t date" + marketDate);
            return pnl;
        }
        /// <summary>
        /// Calculates the virtual PNL.
        /// </summary>
        /// <param name="marketDate">The market date.</param>
        /// <param name="node">The node.</param>
        /// <param name="mw">The mw.</param>
        /// <param name="isInc">if set to <c>true</c> [is inc].</param>
        /// <returns>Virtual PNL</returns>
        private double CalculateVirtualPnl(DateTime marketDate, int node, double mw, bool isInc)
        {
            double pnl = 0;
            string nodeKey = marketDate.ToString() + node.ToString();
            if (sRTHash.ContainsKey(nodeKey) && sDAHash.ContainsKey(nodeKey))
            {
                if (isInc)
                {
                    pnl = (sDAHash[nodeKey] - sRTHash[nodeKey]) * mw;
                }
                else
                {
                    pnl = (sRTHash[nodeKey] - sDAHash[nodeKey]) * mw;
                }
            }
            Logeriter.writeLog("pnl" + pnl + "\tDate" + marketDate);
            return pnl;
        }
        #endregion

    }

}

