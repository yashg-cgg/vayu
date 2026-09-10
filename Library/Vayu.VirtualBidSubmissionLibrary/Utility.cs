using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.ServiceModel;
using Vayu.CommonAccessLibrary;

namespace Vayu.VirtualBidSubmissionLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// The s select user password command
        /// </summary>
        private static SqlCommand sSelectUserPasswordCommand;
        /// <summary>
        /// The s connection90
        /// </summary>
        private static SqlConnection sConnection90;
        /// <summary>
        /// The s connection90r
        /// </summary>
        private static SqlConnection sConnection90r;
        /// <summary>
        /// The s update submit virtual portfolio command
        /// </summary>
        private static SqlCommand sUpdateSubmitVirtualPortfolioCommand;
        /// <summary>
        /// The s delete virtual bids holder command
        /// </summary>
        private static SqlCommand sDeleteVirtualBidsHolderCommand;
        /// <summary>
        /// The s select node from ext command
        /// </summary>
        private static SqlCommand sSelectNodeFromExtCommand;
        /// <summary>
        /// The s select firm command
        /// </summary>
        private static SqlCommand sSelectFirmCommand;
        /// <summary>
        /// The s select multiplier command
        /// </summary>
        private static SqlCommand sSelectMultiplierCommand;

        /// <summary>
        /// Initializes the database.
        /// </summary>
        private static void LoadDB()
        {
            /*sConnection90 = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());*/
            sConnection90 = new VayuDBConnection().GetInstance().GetSqlConnection();
            /*sConnection90r = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());*/
            //
            sSelectUserPasswordCommand = new SqlCommand();
            sSelectUserPasswordCommand.CommandText = "select pjm_login, pjm_password from account where account_id = (select ACCOUNT from PORTFOLIO where PORTFOLIO_ID = @portfolio_id)";
            sSelectUserPasswordCommand.Parameters.AddWithValue("@portfolio_id", "portfolio_id");
            sSelectUserPasswordCommand.Connection = sConnection90r;
            //
            sSelectNodeFromExtCommand = new SqlCommand();
            sSelectNodeFromExtCommand.CommandText = "select nodekey from node where marketkey = 1 and externalnodeid = @externalnodeid";
            sSelectNodeFromExtCommand.Parameters.AddWithValue("@externalnodeid", "externalnodeid");
            sSelectNodeFromExtCommand.Connection = sConnection90;
            //
            sUpdateSubmitVirtualPortfolioCommand = new SqlCommand();
            sUpdateSubmitVirtualPortfolioCommand.CommandText = "update virtualbids set marketdate = @newmarketdate, trantime = @trantime, status = @status, transactionid=@transactionid where nodekey in " +
                                                            "(select nodekey from node where marketkey = 1 and externalnodeid = @externalnodeid) and he = @he and price = @price " +
                                                        "and mw = @mw and marketdate = @marketdate and portfoliokey = @portfoliokey and incdec = @incdec";
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@newmarketdate", "marketdate");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@trantime", "trantime");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@externalnodeid", "externalnodeid");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@he", "he");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@price", "price");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@mw", "mw");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@marketdate", "marketdate");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@incdec", "incdec");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@status", "status");
            sUpdateSubmitVirtualPortfolioCommand.Parameters.AddWithValue("@transactionid", "transactionid");
            sUpdateSubmitVirtualPortfolioCommand.Connection = sConnection90;
            //
            sDeleteVirtualBidsHolderCommand = new SqlCommand();
            sDeleteVirtualBidsHolderCommand.CommandText = "delete virtualbidsHolder where portfoliokey = @portfoliokey and marketdate = @marketdate and nodekey = @nodekey and incdec = @incdec";
            sDeleteVirtualBidsHolderCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            sDeleteVirtualBidsHolderCommand.Parameters.AddWithValue("@marketdate", "marketdate");
            sDeleteVirtualBidsHolderCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            sDeleteVirtualBidsHolderCommand.Parameters.AddWithValue("@incdec", "incdec");
            sDeleteVirtualBidsHolderCommand.Connection = sConnection90;
            //
            sSelectFirmCommand = new SqlCommand();
            sSelectFirmCommand.CommandText = "select PORTFOLIO_ID from PORTFOLIO where ACTIVE = 'y' and PRODUCT = 'virtual' and HUB = 'pjm' and ACCOUNT in (select ACCOUNT from firmaccount)";
            sSelectFirmCommand.Connection = sConnection90r;
            //
            sSelectMultiplierCommand = new SqlCommand();
            sSelectMultiplierCommand.CommandText = "select virtual_multiplier from firmaccount where account = (select account from portfolio where portfolio_id = @portfolio_id) and multiplier > 0";
            sSelectMultiplierCommand.Parameters.AddWithValue("@portfolio_id", "portfolio_id");
            sSelectMultiplierCommand.Connection = sConnection90r;
        }
        /// <summary>
        /// Sets the inc decimal hash.
        /// </summary>
        /// <param name="virtuals">The virtuals.</param>
        /// <param name="incHash">The inc hash.</param>
        /// <param name="decHash">The decimal hash.</param>
        public static void SetIncDecHash(VirtualBid[] virtuals, Dictionary<string, Dictionary<int, Dictionary<int, Tuple<double, double>>>> incHash,
                                            Dictionary<string, Dictionary<int, Dictionary<int, Tuple<double, double>>>> decHash)
        {
            foreach (VirtualBid virtualBid in virtuals)
            {
                Dictionary<string, Dictionary<int, Dictionary<int, Tuple<double, double>>>> incDecHash = virtualBid.IsInc ? incHash : decHash;
                Dictionary<int, Dictionary<int, Tuple<double, double>>> hourHash = new Dictionary<int, Dictionary<int, Tuple<double, double>>>();
                if (incDecHash.ContainsKey(virtualBid.Source))
                {
                    hourHash = incDecHash[virtualBid.Source];
                    incDecHash.Remove(virtualBid.Source);
                }
                Dictionary<int, Tuple<double, double>> segmentHash = new Dictionary<int, Tuple<double, double>>();
                if (hourHash.ContainsKey(virtualBid.Hour))
                {
                    segmentHash = hourHash[virtualBid.Hour];
                    hourHash.Remove(virtualBid.Hour);
                }
                Tuple<double, double> mwPriceTuple = new Tuple<double, double>(virtualBid.MW, virtualBid.Price);
                segmentHash.Add(virtualBid.Segment, mwPriceTuple);
                hourHash.Add(virtualBid.Hour, segmentHash);
                incDecHash.Add(virtualBid.Source, hourHash);
            }
        }
        /// <summary>
        /// Determines whether [is valid portfolio] [the specified portfolio].
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        /// <returns></returns>
        public static double IsValidPortfolio(int portfolio)
        {
            double multiplier = 0;
            try
            {
                LoadDB();
                sConnection90r.Open();
                sSelectMultiplierCommand.Parameters["@portfolio_id"].Value = portfolio;
                SqlDataReader reader = sSelectMultiplierCommand.ExecuteReader();
                while (reader.Read())
                {
                    multiplier = Convert.ToDouble(reader.GetValue(0));
                }
                reader.Close();
                sConnection90r.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return multiplier;
        }
        /// <summary>
        /// Gets the user password.
        /// </summary>
        /// <param name="portfolioId">The portfolio identifier.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public static Tuple<string, string> GetUserPassword(int portfolioId, int marketKey)
        {
            LoadDB();
            sSelectUserPasswordCommand.CommandText = marketKey == 2 ? "select miso_cert, miso_password from account where account_id = (select ACCOUNT from PORTFOLIO where PORTFOLIO_ID = @portfolio_id)" :
                                                                "select spp_cert, spp_password from account where account_id = (select ACCOUNT from PORTFOLIO where PORTFOLIO_ID = @portfolio_id)";
            string certFileName = marketKey == 2 ? "C:\\Certificates\\MISO\\" : "C:\\Certificates\\SPP\\";
            string certPasswd = "";
            sConnection90r.Open();
            sSelectUserPasswordCommand.Parameters["@portfolio_id"].Value = portfolioId;
            SqlDataReader reader = sSelectUserPasswordCommand.ExecuteReader();
            while (reader.Read())
            {
                certFileName = certFileName + reader.GetString(0);
                certPasswd = reader.GetString(1);
            }
            reader.Close();
            sConnection90r.Close();
            if (certPasswd == "")
            {
                return null;
            }
            return new Tuple<string, string>(certFileName, certPasswd);
        }
        /// <summary>
        /// Gets the network credentials.
        /// </summary>
        /// <param name="portfolioID">The portfolio identifier.</param>
        /// <returns></returns>
        public static NetworkCredential GetNetworkCredentials(int portfolioID)
        {
            LoadDB();
            NetworkCredential networkCred = new NetworkCredential();
            sSelectUserPasswordCommand.Connection.Open();
            sSelectUserPasswordCommand.Parameters["@portfolio_id"].Value = portfolioID;
            SqlDataReader reader = sSelectUserPasswordCommand.ExecuteReader();
            while (reader.Read())
            {
                networkCred.UserName = reader.GetString(0);
                networkCred.Password = reader.GetString(1);
            }
            reader.Close();
            sSelectUserPasswordCommand.Connection.Close();
            return networkCred;
        }
        /// <summary>
        /// Updates the virtual bids.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="portfoliokey">The portfoliokey.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="bids">The bids.</param>
        /// <param name="status">The status.</param>
        /// <param name="tranId">The tran identifier.</param>
        public static void UpdateVirtualBids(int marketKey, int portfoliokey, DateTime toDate, VirtualBid[] bids, string status, string tranId)
        {
            LoadDB();
            sConnection90.Open();
            try
            {
                List<string> holderList = new List<string>();
                if (marketKey == 2)
                {
                    using (SqlConnection con = new SqlConnection(sConnection90.ConnectionString))
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "update virtualbids set transactionid=@transactionid,trantime=getdate(),status=@status where portfoliokey=@portfoliokey and marketdate=@marketdate and market=2";
                            try
                            {
                                cmd.Connection.Open();
                                cmd.Parameters.AddWithValue("@transactionid", tranId == null ? "" : tranId);
                                cmd.Parameters.AddWithValue("@status", status);
                                cmd.Parameters.AddWithValue("@portfoliokey", portfoliokey);
                                cmd.Parameters.AddWithValue("@marketdate", toDate.Date);
                                cmd.ExecuteNonQuery();
                                cmd.Connection.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
                else if (marketKey == 12)
                {
                    string[] transIds = tranId.Split('\t');
                    if (transIds.Length == 2)
                    {
                        bool isInc = true;
                        string bidsId = "";// transIds[0].Replace("bids=>", "");
                        string offerId = "";// transIds[0].Replace("offers=>", "");
                        foreach (var item in transIds)
                        {
                            if (item.Contains("bids=>"))
                            {
                                isInc = false;
                                bidsId = item.Replace("bids=>", "");
                            }
                            else
                            {
                                isInc = true;
                                offerId = item.Replace("offers=>", "");
                            }
                            using (SqlConnection con = new SqlConnection(sConnection90.ConnectionString))
                            {
                                using (SqlCommand cmd = con.CreateCommand())
                                {
                                    cmd.CommandText = "update virtualbids set transactionid=@transactionid,trantime=getdate(),status=@status where portfoliokey=@portfoliokey and marketdate=@marketdate  and market=12";
                                    try
                                    {
                                        cmd.Connection.Open();
                                        cmd.Parameters.AddWithValue("@transactionid", isInc ? offerId : bidsId);
                                        cmd.Parameters.AddWithValue("@status", status);
                                        cmd.Parameters.AddWithValue("@portfoliokey", portfoliokey);
                                        cmd.Parameters.AddWithValue("@marketdate", toDate.Date);
                                        cmd.Parameters.AddWithValue("@incdec", isInc ? "I" : "D");
                                        cmd.ExecuteNonQuery();
                                        cmd.Connection.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (VirtualBid bid in bids)
                    {
                        string incDec = bid.IsInc ? "I" : "D";
                        if (marketKey != 1)
                        {
                            sUpdateSubmitVirtualPortfolioCommand.CommandText = "update virtualbids set marketdate = @newmarketdate, trantime = @trantime, status = @status,transactionid=@transactionid where nodekey in " +
                                                                "(select nodekey from node where marketkey = " + marketKey + " and nodename = @externalnodeid) and he = @he and price = @price " +
                                                            "and mw = @mw and marketdate = @marketdate and portfoliokey = @portfoliokey and incdec = @incdec";
                            sUpdateSubmitVirtualPortfolioCommand.Parameters["@externalnodeid"].Value = bid.Source;
                        }

                        else
                        {
                            sUpdateSubmitVirtualPortfolioCommand.Parameters["@externalnodeid"].Value = Int32.Parse(bid.Source);
                        }
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@newmarketdate"].Value = toDate;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@trantime"].Value = DateTime.Now;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@he"].Value = bid.Hour;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@price"].Value = bid.Price;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@mw"].Value = Math.Abs(bid.MW);
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@marketdate"].Value = toDate;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@portfoliokey"].Value = portfoliokey;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@incdec"].Value = incDec;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@status"].Value = status;
                        sUpdateSubmitVirtualPortfolioCommand.Parameters["@transactionid"].Value = tranId;
                        int count = sUpdateSubmitVirtualPortfolioCommand.ExecuteNonQuery();
                        if (status.ToUpper() == "VALID")
                        {
                            string key = bid.Source + "?" + incDec;
                            if (!holderList.Contains(key))
                            {
                                holderList.Add(key);
                            }
                        }
                    }
                    foreach (string holder in holderList)
                    {
                        string[] tokens = holder.Split('?');
                        int nodeKey = 0;
                        if (marketKey != 1)
                        {
                            sSelectNodeFromExtCommand.CommandText = "select nodekey from node where marketkey = " + marketKey + " and nodename = @externalnodeid";
                            sSelectNodeFromExtCommand.Parameters["@externalnodeid"].Value = tokens[0];
                        }
                        else
                        {
                            sSelectNodeFromExtCommand.Parameters["@externalnodeid"].Value = Int32.Parse(tokens[0]);
                        }
                        SqlDataReader reader = sSelectNodeFromExtCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            nodeKey = Convert.ToInt32(reader.GetValue(0));
                        }
                        reader.Close();
                        sDeleteVirtualBidsHolderCommand.Parameters["@portfoliokey"].Value = portfoliokey;
                        sDeleteVirtualBidsHolderCommand.Parameters["@nodekey"].Value = nodeKey;
                        sDeleteVirtualBidsHolderCommand.Parameters["@incdec"].Value = tokens[1];
                        sDeleteVirtualBidsHolderCommand.Parameters["@marketdate"].Value = toDate;
                        sDeleteVirtualBidsHolderCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            sConnection90.Close();
        }
        /// <summary>
        /// Connects the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="type">The type.</param>
        public static void Connect(int port, Type type)
        {
            using (ServiceHost host = new ServiceHost(type, new Uri("net.tcp://localhost:" + port)))
            {
                NetTcpBinding myBinding = new NetTcpBinding();
                myBinding.Security.Mode = SecurityMode.None;
                myBinding.MaxReceivedMessageSize = 10485760;
                myBinding.OpenTimeout = new TimeSpan(0, 12, 0);
                myBinding.SendTimeout = new TimeSpan(0, 12, 0);
                myBinding.ReceiveTimeout = new TimeSpan(0, 12, 0);
                myBinding.CloseTimeout = new TimeSpan(0, 12, 0);
                host.AddServiceEndpoint(typeof(IVirtual), myBinding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine("Successfully opened port " + port);
                    Console.ReadLine();
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
