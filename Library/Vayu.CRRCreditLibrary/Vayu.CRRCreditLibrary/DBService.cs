using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRCreditLibrary
{
    public class DBService
    {
        private static SqlConnection VayuConnection;
        //private static SqlCommand sSelectUserPasswordCommand;
        private static SqlCommand sInsertTransactionCommand;
        private static SqlCommand sDeleteTransactionCommand;
        private static SqlCommand sSelectTransactionCommand;
        private static SqlCommand sSelectPortfolioCommand;
        private static SqlCommand sUpdateValidCRRBidCommand;
        private static SqlCommand sUpdateInvalidCRRBidCommand;
        private static SqlCommand sSelectNetworkCommand;

        private static void InitDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            sInsertTransactionCommand = new SqlCommand();
            sInsertTransactionCommand.CommandText = "insert CRR_transaction values (@transaction_id, @CRR_date, @market, @round, @CRR_type, @marketkey)";
            sInsertTransactionCommand.Parameters.AddWithValue("@transaction_id", "transaction_id");
            sInsertTransactionCommand.Parameters.AddWithValue("@CRR_date", "CRR_date");
            sInsertTransactionCommand.Parameters.AddWithValue("@market", "market");
            sInsertTransactionCommand.Parameters.AddWithValue("@round", "round");
            sInsertTransactionCommand.Parameters.AddWithValue("@CRR_type", "CRR_type");
            sInsertTransactionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            sInsertTransactionCommand.Connection = VayuConnection;
            //
            sSelectTransactionCommand = new SqlCommand();
            sSelectTransactionCommand.CommandText = "select market, CRR_type from CRR_transaction where transaction_id = @transaction_id and round = @round";
            sSelectTransactionCommand.Parameters.AddWithValue("@transaction_id", "transaction_id");
            sSelectTransactionCommand.Parameters.AddWithValue("@round", "round");
            sSelectTransactionCommand.Connection = VayuConnection;
            //
            sDeleteTransactionCommand = new SqlCommand();
            sDeleteTransactionCommand.CommandText = "delete CRR_transaction where transaction_id = @transaction_id and round = @round";
            sDeleteTransactionCommand.Parameters.AddWithValue("@transaction_id", "transaction_id");
            sDeleteTransactionCommand.Parameters.AddWithValue("@round", "round");
            sDeleteTransactionCommand.Connection = VayuConnection;
            //
            sUpdateValidCRRBidCommand = new SqlCommand();
            sUpdateValidCRRBidCommand.CommandText = "update CRRbids set status = @status, transactionid = @transactionid where CRRbidskey = @CRRbidskey";
            sUpdateValidCRRBidCommand.Parameters.AddWithValue("@CRRbidskey", "CRRbidskey");
            sUpdateValidCRRBidCommand.Parameters.AddWithValue("@status", "status");
            sUpdateValidCRRBidCommand.Parameters.AddWithValue("@transactionid", "transactionid");
            sUpdateValidCRRBidCommand.CommandTimeout = 300000;
            sUpdateValidCRRBidCommand.Connection = VayuConnection;
            //
            sUpdateInvalidCRRBidCommand = new SqlCommand();
            sUpdateInvalidCRRBidCommand.CommandText = "update CRRbids set status = 'Invalid' where transactionid = @transactionid";
            sUpdateInvalidCRRBidCommand.Parameters.AddWithValue("@transactionid", "transactionid");
            sUpdateInvalidCRRBidCommand.Connection = VayuConnection;
            //
            sSelectPortfolioCommand = new SqlCommand();
            sSelectPortfolioCommand.CommandText = "select STRIP from Portfolio where PORTFOLIO_ID = @pkey and product='CRR'";
            sSelectPortfolioCommand.Parameters.AddWithValue("@pkey", "PORTFOLIO_ID");
            sSelectPortfolioCommand.Connection = VayuConnection;
            //
            sSelectNetworkCommand = new SqlCommand();
            sSelectNetworkCommand.CommandText = "select pjm_login, pjm_password from account where account_id = (select ACCOUNT from PORTFOLIO where PORTFOLIO_ID = @portfolio_id and product='CRR')";
            sSelectNetworkCommand.Parameters.AddWithValue("@portfolio_id", "portfolio_id");
            sSelectNetworkCommand.Connection = VayuConnection;
        }
        public static NetworkCredential GetNetworkCredentials(int portfolioID)
        {
            InitDB();
            NetworkCredential networkCred = new NetworkCredential();
            sSelectNetworkCommand.Parameters["@portfolio_id"].Value = portfolioID;
            VayuConnection.Open();
            SqlDataReader reader = sSelectNetworkCommand.ExecuteReader();
            while (reader.Read())
            {
                networkCred.UserName = reader.GetString(0);
                networkCred.Password = reader.GetString(1);
            }
            reader.Close();
            VayuConnection.Close();
            return networkCred;
        }
        public static string GetPortfolio(int portfolio)
        {
            InitDB();
            string portfolioname = "";
            VayuConnection.Open();
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
            VayuConnection.Close();
            return portfolioname;
        }
        public static void UpdateInvalidCRRBids(string trasnactionId)
        {
            InitDB();
            VayuConnection.Open();
            sUpdateInvalidCRRBidCommand.Parameters["@transactionid"].Value = trasnactionId;
            sUpdateInvalidCRRBidCommand.ExecuteNonQuery();
            VayuConnection.Close();
        }
        //public static void UpdateValidCRRBids(int CRRBidsKey, string status, string trasnactionId)
        //{
        //    InitDB();
        //    sConnection90.Open();
        //    try
        //    {
        //        sUpdateValidCRRBidCommand.Parameters["@CRRbidskey"].Value = CRRBidsKey;
        //        sUpdateValidCRRBidCommand.Parameters["@status"].Value = status;
        //        sUpdateValidCRRBidCommand.Parameters["@transactionid"].Value = trasnactionId;
        //        sUpdateValidCRRBidCommand.ExecuteNonQuery();
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
            VayuConnection.Open();
            sDeleteTransactionCommand.Parameters["@transaction_id"].Value = transactionId;
            sDeleteTransactionCommand.Parameters["@round"].Value = round;
            sDeleteTransactionCommand.ExecuteNonQuery();
            VayuConnection.Close();
        }
        public static Transaction GetTransaction(string transactionId, int round)
        {
            InitDB();
            Transaction transaction = new Transaction();
            VayuConnection.Open();
            sSelectTransactionCommand.Parameters["@transaction_id"].Value = transactionId;
            sSelectTransactionCommand.Parameters["@round"].Value = round;
            SqlDataReader reader = sSelectTransactionCommand.ExecuteReader();
            while (reader.Read())
            {
                transaction.Market = reader.GetString(0);
                transaction.Period = reader.GetString(1);
            }
            reader.Close();
            VayuConnection.Close();
            return transaction;
        }
        public static void SaveTransaction(string tranId, string marketName, int round, string type, int marketKey)
        {
            InitDB();
            VayuConnection.Open();
            try
            {
                sInsertTransactionCommand.Parameters["@transaction_id"].Value = tranId;
                sInsertTransactionCommand.Parameters["@CRR_date"].Value = DateTime.Today;
                sInsertTransactionCommand.Parameters["@market"].Value = marketName;
                sInsertTransactionCommand.Parameters["@round"].Value = round;
                sInsertTransactionCommand.Parameters["@CRR_type"].Value = type;
                sInsertTransactionCommand.Parameters["@marketkey"].Value = marketKey;
                sInsertTransactionCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            VayuConnection.Close();
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

                        foreach (var itemF in item.CRRs)
                        {
                            DataRow row = dataTable.NewRow();
                            //row["PortfolioKey"] = portKey;
                            //row["Auction"] = auction;
                            row["CRRBidsKey"] = itemF.ID;
                            row["TransactionID"] = item.transactionId;
                            row["Status"] = item.status;
                            //row["MarketKey"] = marketKey;
                            dataTable.Rows.Add(row);
                        }

                        SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection();
                        using (SqlBulkCopy bkLmpH = new SqlBulkCopy(con))
                        {
                            try
                            {
                                con.Open();
                                bkLmpH.DestinationTableName = "CRRBids_temp";
                                //bkLmpH.ColumnMappings.Add("PortfolioKey", "PortfolioKey");
                                //bkLmpH.ColumnMappings.Add("Auction", "Auction");
                                bkLmpH.ColumnMappings.Add("CRRBidsKey", "CRRBidsKey");
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
                        cmd.CommandText = "update CRRBids set Status = ft.Status , TransactionId = ft.TransactionID from CRRBids f join CRRBids_temp ft on f.CRRbidsKey = ft.CRRBidsKey " +
                        " where ft.TransactionID = '" + item.transactionId + "'";
                        cmd.CommandTimeout = 300000;

                        try
                        {
                            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                                cmd.Connection.Open();

                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "delete CRRBids_temp where TransactionID = '" + item.transactionId + "'";
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

        public void BulkUpdateStatus(CRR[] newList, string status, string uniqueTranID)
        {
            try
            {
                if (!updateloop.IsAddingCompleted)
                    updateloop.Add(new UpdateStruct() { CRRs = newList, status = "Valid", transactionId = uniqueTranID });
            }
            catch { }
        }

        private DataTable BuildTable()
        {
            DataTable dataTable = new DataTable();
            //dataTable.Columns.Add("PortfolioKey", typeof(int));
            //dataTable.Columns.Add("Auction", typeof(string));
            dataTable.Columns.Add("CRRBidsKey", typeof(int));
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
        public CRR[] CRRs;
        public string status;
        public string transactionId;
    }

    public class Transaction
    {
        public string Market { get; set; }
        public string Period { get; set; }
    }
}
