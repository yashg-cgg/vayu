using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sigma.FTRCreditLibrary
{
    public class DBService
    {
        private static SqlConnection sConnection90;
        private static SqlConnection sConnection90r;
        //private static SqlCommand sSelectUserPasswordCommand;
        private static SqlCommand sInsertTransactionCommand;
        private static SqlCommand sDeleteTransactionCommand;
        private static SqlCommand sSelectTransactionCommand;
        private static SqlCommand sSelectPortfolioCommand;
        private static SqlCommand sUpdateValidFtrBidCommand;
        private static SqlCommand sUpdateInvalidFtrBidCommand;
        private static SqlCommand sSelectNetworkCommand;

        private static void InitDB()
        {
            sConnection90 = new SqlConnection(Sigma.CommonAccessLibrary.DBConnectionCredentials.GetTradingDBConnection());
            sConnection90r = new SqlConnection(Sigma.CommonAccessLibrary.DBConnectionCredentials.GetTradingDBConnection());
            //
            //sSelectUserPasswordCommand = new SqlCommand();
            //sSelectUserPasswordCommand.CommandText = "Select UserID, Password From ISOUserData Where AccountID = @AccountID and LoginType = 'FTR' and active = 1 and marketkey = 1";
            //sSelectUserPasswordCommand.Parameters.AddWithValue("@AccountID", "AccountID");
            //sSelectUserPasswordCommand.Connection = sConnection90;
            //
            sInsertTransactionCommand = new SqlCommand();
            sInsertTransactionCommand.CommandText = "insert ftr_transaction values (@transaction_id, @ftr_date, @market, @round, @ftr_type, @marketkey)";
            sInsertTransactionCommand.Parameters.AddWithValue("@transaction_id", "transaction_id");
            sInsertTransactionCommand.Parameters.AddWithValue("@ftr_date", "ftr_date");
            sInsertTransactionCommand.Parameters.AddWithValue("@market", "market");
            sInsertTransactionCommand.Parameters.AddWithValue("@round", "round");
            sInsertTransactionCommand.Parameters.AddWithValue("@ftr_type", "ftr_type");
            sInsertTransactionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            sInsertTransactionCommand.Connection = sConnection90;
            //
            sSelectTransactionCommand = new SqlCommand();
            sSelectTransactionCommand.CommandText = "select market, ftr_type from ftr_transaction where transaction_id = @transaction_id and round = @round";
            sSelectTransactionCommand.Parameters.AddWithValue("@transaction_id", "transaction_id");
            sSelectTransactionCommand.Parameters.AddWithValue("@round", "round");
            sSelectTransactionCommand.Connection = sConnection90;
            //
            sDeleteTransactionCommand = new SqlCommand();
            sDeleteTransactionCommand.CommandText = "delete ftr_transaction where transaction_id = @transaction_id and round = @round";
            sDeleteTransactionCommand.Parameters.AddWithValue("@transaction_id", "transaction_id");
            sDeleteTransactionCommand.Parameters.AddWithValue("@round", "round");
            sDeleteTransactionCommand.Connection = sConnection90;
            //
            sUpdateValidFtrBidCommand = new SqlCommand();
            sUpdateValidFtrBidCommand.CommandText = "update ftrbids set status = @status, transactionid = @transactionid where ftrbidskey = @ftrbidskey";
            sUpdateValidFtrBidCommand.Parameters.AddWithValue("@ftrbidskey", "ftrbidskey");
            sUpdateValidFtrBidCommand.Parameters.AddWithValue("@status", "status");
            sUpdateValidFtrBidCommand.Parameters.AddWithValue("@transactionid", "transactionid");
            sUpdateValidFtrBidCommand.CommandTimeout = 300000;
            sUpdateValidFtrBidCommand.Connection = sConnection90;
            //
            sUpdateInvalidFtrBidCommand = new SqlCommand();
            sUpdateInvalidFtrBidCommand.CommandText = "update ftrbids set status = 'Invalid' where transactionid = @transactionid";
            sUpdateInvalidFtrBidCommand.Parameters.AddWithValue("@transactionid", "transactionid");
            sUpdateInvalidFtrBidCommand.Connection = sConnection90;
            //
            sSelectPortfolioCommand = new SqlCommand();
            sSelectPortfolioCommand.CommandText = "select STRIP from Portfolio where PORTFOLIO_ID = @pkey and product='FTR'";
            sSelectPortfolioCommand.Parameters.AddWithValue("@pkey", "PORTFOLIO_ID");
            sSelectPortfolioCommand.Connection = sConnection90r;
            //
            sSelectNetworkCommand = new SqlCommand();
            sSelectNetworkCommand.CommandText = "select pjm_login, pjm_password from account where account_id = (select ACCOUNT from PORTFOLIO where PORTFOLIO_ID = @portfolio_id and product='FTR')";
            sSelectNetworkCommand.Parameters.AddWithValue("@portfolio_id", "portfolio_id");
            sSelectNetworkCommand.Connection = sConnection90r;
        }
        public static NetworkCredential GetNetworkCredentials(int portfolioID)
        {
            InitDB();
            NetworkCredential networkCred = new NetworkCredential();
            sSelectNetworkCommand.Parameters["@portfolio_id"].Value = portfolioID;
            sConnection90r.Open();
            SqlDataReader reader = sSelectNetworkCommand.ExecuteReader();
            while (reader.Read())
            {
                networkCred.UserName = reader.GetString(0);
                networkCred.Password = reader.GetString(1);
            }
            reader.Close();
            sConnection90r.Close();
            return networkCred;
        }
        public static string GetPortfolio(int portfolio)
        {
            InitDB();
            string portfolioname = "";
            sConnection90r.Open();
            sSelectPortfolioCommand.Parameters["@pkey"].Value = portfolio;
            SqlDataReader reader = sSelectPortfolioCommand.ExecuteReader();
            while (reader.Read())
            {
                portfolioname = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString();
                if (portfolioname != "")
                {
                    break;
                }
            }
            reader.Close();
            sConnection90r.Close();
            return portfolioname;
        }
        public static void UpdateInvalidFtrBids(string trasnactionId)
        {
            InitDB();
            sConnection90.Open();
            sUpdateInvalidFtrBidCommand.Parameters["@transactionid"].Value = trasnactionId;
            sUpdateInvalidFtrBidCommand.ExecuteNonQuery();
            sConnection90.Close();
        }
        //public static void UpdateValidFtrBids(int ftrBidsKey, string status, string trasnactionId)
        //{
        //    InitDB();
        //    sConnection90.Open();
        //    try
        //    {
        //        sUpdateValidFtrBidCommand.Parameters["@ftrbidskey"].Value = ftrBidsKey;
        //        sUpdateValidFtrBidCommand.Parameters["@status"].Value = status;
        //        sUpdateValidFtrBidCommand.Parameters["@transactionid"].Value = trasnactionId;
        //        sUpdateValidFtrBidCommand.ExecuteNonQuery();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    sConnection90.Close();
        //}
        public static void DeleteTransaction(string transactionId, int round)
        {
            InitDB();
            sConnection90.Open();
            sDeleteTransactionCommand.Parameters["@transaction_id"].Value = transactionId;
            sDeleteTransactionCommand.Parameters["@round"].Value = round;
            sDeleteTransactionCommand.ExecuteNonQuery();
            sConnection90.Close();
        }
        public static Transaction GetTransaction(string transactionId, int round)
        {
            InitDB();
            Transaction transaction = new Transaction();
            sConnection90.Open();
            sSelectTransactionCommand.Parameters["@transaction_id"].Value = transactionId;
            sSelectTransactionCommand.Parameters["@round"].Value = round;
            SqlDataReader reader = sSelectTransactionCommand.ExecuteReader();
            while (reader.Read())
            {
                transaction.Market = reader.GetString(0);
                transaction.Period = reader.GetString(1);
            }
            reader.Close();
            sConnection90.Close();
            return transaction;
        }
        public static void SaveTransaction(string tranId, string marketName, int round, string type, int marketKey)
        {
            InitDB();
            sConnection90.Open();
            try
            {
                sInsertTransactionCommand.Parameters["@transaction_id"].Value = tranId;
                sInsertTransactionCommand.Parameters["@ftr_date"].Value = DateTime.Today;
                sInsertTransactionCommand.Parameters["@market"].Value = marketName;
                sInsertTransactionCommand.Parameters["@round"].Value = round;
                sInsertTransactionCommand.Parameters["@ftr_type"].Value = type;
                sInsertTransactionCommand.Parameters["@marketkey"].Value = marketKey;
                sInsertTransactionCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            sConnection90.Close();
        }

        private BlockingCollection<UpdateStruct> updateloop;

      

        public DBService()
        {
            updateloop = new BlockingCollection<UpdateStruct>();
            RunUpdate();
        }

        private async void RunUpdate()
        {
            Task t1 = Task.Run(() =>
            {
                foreach (var item in updateloop.GetConsumingEnumerable())
                {
                    try
                    {
                        DataTable dataTable = BuildTable();

                        foreach (var itemF in item.ftrs)
                        {
                            DataRow row = dataTable.NewRow();
                            //row["PortfolioKey"] = portKey;
                            //row["Auction"] = auction;
                            row["FtrBidsKey"] = itemF.ID;
                            row["TransactionID"] = item.transactionId;
                            row["Status"] = item.status;
                            //row["MarketKey"] = marketKey;
                            dataTable.Rows.Add(row);
                        }

                        SqlConnection con = new SqlConnection(Sigma.CommonAccessLibrary.DBConnectionCredentials.GetTradingDBConnection());
                        using (SqlBulkCopy bkLmpH = new SqlBulkCopy(con))
                        {
                            try
                            {
                                con.Open();
                                bkLmpH.DestinationTableName = "FtrBids_temp";
                                //bkLmpH.ColumnMappings.Add("PortfolioKey", "PortfolioKey");
                                //bkLmpH.ColumnMappings.Add("Auction", "Auction");
                                bkLmpH.ColumnMappings.Add("FtrBidsKey", "FtrBidsKey");
                                bkLmpH.ColumnMappings.Add("TransactionID", "TransactionID");
                                bkLmpH.ColumnMappings.Add("Status", "Status");
                                //bkLmpH.ColumnMappings.Add("MarketKey", "MarketKey");

                                bkLmpH.WriteToServer(dataTable);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        SqlCommand cmd = con.CreateCommand();
                        cmd.CommandText = "update FtrBids set Status = ft.Status , TransactionId = ft.TransactionID from FtrBids f join FtrBids_temp ft on f.ftrbidsKey = ft.FtrBidsKey " +
                        " where ft.TransactionID = '" + item.transactionId + "'";
                        cmd.CommandTimeout = 300000;

                        try
                        {
                            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                                cmd.Connection.Open();

                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "delete FtrBids_temp where TransactionID = '" + item.transactionId + "'";
                            cmd.ExecuteNonQuery();

                            if (cmd.Connection.State != System.Data.ConnectionState.Closed)
                                cmd.Connection.Close();
                        }
                        catch
                        {
                            if (cmd.Connection.State != System.Data.ConnectionState.Closed)
                                cmd.Connection.Close();
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            });
            await t1;
        }

        public void BulkUpdateStatus(FTR[] newList, string status, string uniqueTranID)
        {
            try
            {
                if (!updateloop.IsAddingCompleted)
                    updateloop.Add(new UpdateStruct() { ftrs = newList, status = "Valid", transactionId = uniqueTranID });
            }
            catch { }
        }

        private DataTable BuildTable()
        {
            DataTable dataTable = new DataTable();
            //dataTable.Columns.Add("PortfolioKey", typeof(int));
            //dataTable.Columns.Add("Auction", typeof(string));
            dataTable.Columns.Add("FtrBidsKey", typeof(int));
            dataTable.Columns.Add("TransactionID", typeof(string));
            dataTable.Columns.Add("Status", typeof(string));
            //dataTable.Columns.Add("MarketKey", typeof(int));
            return dataTable;
        }

        public void OnDispose()
        {
            if (updateloop != null && !updateloop.IsAddingCompleted)
                updateloop.CompleteAdding();
        }
    }

    public struct UpdateStruct
    {
        public FTR[] ftrs;
        public string status;
        public string transactionId;
    }

    public class Transaction
    {
        public string Market { get; set; }
        public string Period { get; set; }
    }
}
