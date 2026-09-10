//using ICSharpCode.SharpZipLib.Zip;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Vayu.PNLDownload
{
    /// <summary>
    /// Download the Fee
    /// </summary>
    public class FeeDownload
    {
        #region Private Members
        private SqlConnection VayuDbConnection = new SqlConnection(CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
        /// <summary>
        /// The select cleared bids command
        /// </summary>
        private SqlCommand mSelectClearedBidsCommand;
        /// <summary>
        /// The select cleared ees command
        /// </summary>
        private SqlCommand mSelectClearedEESCommand;
        /// <summary>
        /// The select virtual portfolio command
        /// </summary>
        private SqlCommand mSelectVirtualPortfolioCommand;
        /// <summary>
        /// The select upto portfolio command
        /// </summary>
        private SqlCommand mSelectUptoPortfolioCommand;
        /// <summary>
        /// The update virtual PNL command
        /// </summary>
        private SqlCommand mUpdateVirtualPnlCommand;
        /// <summary>
        /// The select user details command
        /// </summary>
        private SqlCommand mSelectUserDetailsCommand;
        /// <summary>
        /// The select under fund fee command
        /// </summary>
        private SqlCommand mSelectUnderFundFeeCommand;
        /// <summary>
        /// The select miso virtual portfolios
        /// </summary>
        private SqlCommand mSelectMisoVirtualPortfolios;
        /// <summary>
        /// The miso start date
        /// </summary>
        private DateTime mMisoStartDate = DateTime.Today.AddMonths(-3);
        /// <summary>
        /// The PJM start date
        /// </summary>
       private DateTime mPjmStartDate = DateTime.Today.AddMonths(-3);
        //  private DateTime mPjmStartDate = DateTime.Parse("2019-04-1 00:00:00.000");
       private DateTime mPjmEndDate = DateTime.Today;
        // private DateTime mPjmEndDate = DateTime.Parse("2019-04-21 00:00:00.000");
        #endregion


        #region Public Method
        /// <summary>
        /// Updates the fees.
        /// </summary>
        public void UpdateFees()
        {
            InitDB();
            SavePJMFees();
        }
        #endregion

        #region Private Method
        /// <summary>
        /// Initializes the database related object.
        /// </summary>
        private void InitDB()
        {
            mSelectClearedBidsCommand = new SqlCommand();
            mSelectClearedBidsCommand.CommandText = "select sum(ClearedMW) from clearedbids where Portfoliokey in (@Portfoliokey) and MarketDateTime > @startDateTime " +
                                                    "and MarketDateTime <= @endDateTime";
            mSelectClearedBidsCommand.Parameters.AddWithValue("@startDateTime", "MarketDateTime");
            mSelectClearedBidsCommand.Parameters.AddWithValue("@endDateTime", "MarketDateTime");
            mSelectClearedBidsCommand.Parameters.AddWithValue("@Portfoliokey", "Portfoliokey");
            mSelectClearedBidsCommand.Connection = VayuDbConnection;
            //
            mSelectClearedEESCommand = new SqlCommand();
            mSelectClearedEESCommand.CommandText = "select sum(ClearedMW) from clearedees where Portfoliokey = @Portfoliokey and MarketDateTime>@startDateTime and MarketDateTime<=@endDateTime";
            mSelectClearedEESCommand.Parameters.AddWithValue("@startDateTime", "MarketDateTime");
            mSelectClearedEESCommand.Parameters.AddWithValue("@endDateTime", "MarketDateTime");
            mSelectClearedEESCommand.Parameters.AddWithValue("@Portfoliokey", "Portfoliokey");
            mSelectClearedEESCommand.Connection = VayuDbConnection;
            //
            mSelectVirtualPortfolioCommand = new SqlCommand();
            mSelectVirtualPortfolioCommand.CommandText = "select PORTFOLIO_ID from portfolio where ACCOUNT = (select account_id from ACCOUNT where account_id = @account_id) " +
                                                    "and HUB = 'PJM' and ACTIVE = 'Y' and product = 'VIRTUAL'";
            mSelectVirtualPortfolioCommand.Parameters.AddWithValue("@account_id", "account_id");
            mSelectVirtualPortfolioCommand.Connection = VayuDbConnection;
            //
            mSelectUptoPortfolioCommand = new SqlCommand();
            mSelectUptoPortfolioCommand.CommandText = "select PORTFOLIO_ID from portfolio where ACCOUNT = (select account_id from ACCOUNT where ACCOUNT_ID = @account_id) " +
                                                "and HUB = 'PJM' and ACTIVE = 'Y' and product = 'EES/PTP'";
            mSelectUptoPortfolioCommand.Parameters.AddWithValue("@account_id", "ACCOUNT_ID");
            mSelectUptoPortfolioCommand.Connection = VayuDbConnection;
            //
            mUpdateVirtualPnlCommand = new SqlCommand();
            mUpdateVirtualPnlCommand.CommandText = "update VIRTUAL_PNL set Fee=@Fee , feeUpdateTime=@feeUpdateTime where PortfolioKey=@PortfolioKey and PnlDate=@PnlDate";
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@Fee", "Fee");
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@PnlDate", "PnlDate");
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@feeUpdateTime", "feeUpdateTime");
            mUpdateVirtualPnlCommand.Connection = VayuDbConnection;
            //
            mSelectUserDetailsCommand = new SqlCommand();
            mSelectUserDetailsCommand.CommandText = "select ACCOUNT_ID, PJM_LOGIN, PJM_PASSWORD from account where PJM_LOGIN is not null and FLAG_DEPRECATED = 0 ";
            mSelectUserDetailsCommand.Connection = VayuDbConnection;
            //
            mSelectUnderFundFeeCommand = new SqlCommand();
            mSelectUnderFundFeeCommand.CommandText = "select Fee from VIRTUAL_PNL where portfoliokey = @portfoliokey and PnlDate = @pnldate";
            mSelectUnderFundFeeCommand.Parameters.AddWithValue("@portfoliokey", "PortfolioKey");
            mSelectUnderFundFeeCommand.Parameters.AddWithValue("@pnldate", "PnlDate");
            mSelectUnderFundFeeCommand.Connection = VayuDbConnection;
            //
            mSelectMisoVirtualPortfolios = new SqlCommand();
            mSelectMisoVirtualPortfolios.CommandText = "select PORTFOLIO_ID from RiskData.dbo.PORTFOLIO  where HUB='MISO' and Active = 'Y' and ACCOUNT in('vs19','vs12')";
            mSelectMisoVirtualPortfolios.Parameters.AddWithValue("@Account", "Account");
            mSelectMisoVirtualPortfolios.Connection = VayuDbConnection;
            //
            
        }
        /// <summary>
        /// Saves the fees.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="virtualRate">The virtual rate.</param>
        /// <param name="uptosRate">The uptos rate.</param>
        private void SaveFees(string account, DateTime startDate, double virtualRate, double uptosRate)
        {
            Logeriter.writeLog("saving fees datetime=" + startDate);
            VayuDbConnection.Open();
           
            mSelectVirtualPortfolioCommand.Parameters["@account_id"].Value = account;
            SqlDataReader reader = mSelectVirtualPortfolioCommand.ExecuteReader();
            while (reader.Read())
            {
                int portfolioKey = reader.GetInt32(0);
                mSelectClearedBidsCommand.Parameters["@startDateTime"].Value = startDate;
                mSelectClearedBidsCommand.Parameters["@endDateTime"].Value = startDate.AddDays(1);
                mSelectClearedBidsCommand.Parameters["@Portfoliokey"].Value = portfolioKey;
                SqlDataReader reader1 = mSelectClearedBidsCommand.ExecuteReader();

                while (reader1.Read())
                {
                    if (!reader1.IsDBNull(0))
                    {
                        double virtualMW = (double)reader1.GetDecimal(0);

                        using (SqlConnection con = new SqlConnection(VayuDbConnection.ConnectionString))
                        {
                            try
                            {
                                using (SqlCommand cmd = con.CreateCommand())
                                {
                                    cmd.Connection.Open();
                                    cmd.CommandText = mUpdateVirtualPnlCommand.CommandText;
                                    cmd.Parameters.AddWithValue("@Fee", -virtualMW * virtualRate);
                                    cmd.Parameters.AddWithValue("@PortfolioKey", portfolioKey);
                                    cmd.Parameters.AddWithValue("@PnlDate", startDate);
                                    cmd.Parameters.AddWithValue("@feeUpdateTime", DateTime.Now);
                                    Logeriter.writeLog("updating virtualfees =" + -virtualMW * virtualRate + "\t portfolioKey" + portfolioKey + "\t pnldate" + startDate);
                                    if (cmd.ExecuteNonQuery() <= 0)
                                    {
                                        cmd.CommandText = "Insert into VIRTUAL_PNL(portfoliokey,pnldate,pnl,fee,pnlupdatetime,feeupdatetime) values(@PortfolioKey,@PnlDate,0,@Fee,getdate(),getdate())";
                                        int c = cmd.ExecuteNonQuery();
                                        Logeriter.writeLog("inserting virtualfees =" + -virtualMW * virtualRate + "\t portfolioKey" + portfolioKey + "\t pnldate" + startDate);
                                        Console.WriteLine("inserting virtualfees =" + -virtualMW * virtualRate + "\t portfolioKey" + portfolioKey + "\t pnldate" + startDate);
                                    }
                                    cmd.Connection.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
                reader1.Close();
            }
            reader.Close();
            mSelectUptoPortfolioCommand.Parameters["@account_id"].Value = account;
            reader = mSelectUptoPortfolioCommand.ExecuteReader();
            while (reader.Read())
            {
                int portfolioKey = reader.GetInt32(0);
                mSelectClearedEESCommand.Parameters["@startDateTime"].Value = startDate;
                mSelectClearedEESCommand.Parameters["@endDateTime"].Value = startDate.AddDays(1);
                mSelectClearedEESCommand.Parameters["@Portfoliokey"].Value = portfolioKey;

                SqlDataReader reader1 = mSelectClearedEESCommand.ExecuteReader();
                while (reader1.Read())
                {
                    if (!reader1.IsDBNull(0))
                    {
                        double virtualMW = (double)reader1.GetDecimal(0);
                        using (SqlConnection con = new SqlConnection(VayuDbConnection.ConnectionString))
                        {
                            try
                            {
                                using (SqlCommand cmd = con.CreateCommand())
                                {
                                    cmd.Connection.Open();
                                    cmd.CommandText = mUpdateVirtualPnlCommand.CommandText;
                                    cmd.Parameters.AddWithValue("@Fee", -virtualMW * uptosRate);
                                    cmd.Parameters.AddWithValue("@PortfolioKey", portfolioKey);
                                    cmd.Parameters.AddWithValue("@PnlDate", startDate);
                                    cmd.Parameters.AddWithValue("@feeUpdateTime", DateTime.Now);
                                    Logeriter.writeLog("updating uptosfees =" + -virtualMW * uptosRate + "\t portfolioKey" + portfolioKey + "\t pnldate" + startDate);
                                    Console.WriteLine("updating uptosfees =" + -virtualMW * uptosRate + "\t portfolioKey" + portfolioKey + "\t pnldate" + startDate);

                                    if (cmd.ExecuteNonQuery() <= 0)
                                    {
                                        cmd.CommandText = "Insert into VIRTUAL_PNL(portfoliokey,pnldate,pnl,fee,pnlupdatetime,feeupdatetime) values(@PortfolioKey,@PnlDate,0,@Fee,getdate(),getdate())";
                                        int c = cmd.ExecuteNonQuery();
                                        Logeriter.writeLog("inserting uptosfees =" + -virtualMW * uptosRate + "\t portfolioKey" + portfolioKey + "\t pnldate" + startDate);
                                    }
                                    cmd.Connection.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
                reader1.Close();
            }
            reader.Close();
            VayuDbConnection.Close();
            
        }
        /// <summary>
        /// Gets the mw.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="lastDate">The last date.</param>
        /// <param name="virtualMW">The virtual mw.</param>
        /// <param name="uptosMW">The uptos mw.</param>
        /// <param name="accounts">The accounts.</param>
        private void GetMW(DateTime startDate, DateTime lastDate, out double virtualMW, out double uptosMW, List<string> accounts)
        {
            Logeriter.writeLog("getting MW" + startDate);
            virtualMW = 0;
            uptosMW = 0;
            foreach (var account in accounts)
            {
                VayuDbConnection.Open();
                VayuDbConnection.Open();
                mSelectVirtualPortfolioCommand.Parameters["@account_id"].Value = account;
                SqlDataReader reader = mSelectVirtualPortfolioCommand.ExecuteReader();
                while (reader.Read())
                {
                    int portfolioKey = reader.GetInt32(0);
                    mSelectClearedBidsCommand.Parameters["@startDateTime"].Value = startDate;
                    mSelectClearedBidsCommand.Parameters["@endDateTime"].Value = lastDate;//.AddDays(1);
                    mSelectClearedBidsCommand.Parameters["@Portfoliokey"].Value = portfolioKey;
                    SqlDataReader reader1 = mSelectClearedBidsCommand.ExecuteReader();
                    while (reader1.Read())
                    {
                        if (!reader1.IsDBNull(0))
                        {
                            virtualMW += (double)reader1.GetDecimal(0);
                            Logeriter.writeLog("getting virtual MW" + startDate + "virtualMW" + virtualMW);
                        }
                    }
                    reader1.Close();
                }
                reader.Close();
                mSelectUptoPortfolioCommand.Parameters["@account_id"].Value = account;
                reader = mSelectUptoPortfolioCommand.ExecuteReader();
                while (reader.Read())
                {
                    int portfolioKey = reader.GetInt32(0);
                    mSelectClearedEESCommand.Parameters["@startDateTime"].Value = startDate;
                    mSelectClearedEESCommand.Parameters["@endDateTime"].Value = lastDate;//.AddDays(1);
                    mSelectClearedEESCommand.Parameters["@Portfoliokey"].Value = portfolioKey;
                    SqlDataReader reader1 = mSelectClearedEESCommand.ExecuteReader();
                    while (reader1.Read())
                    {
                        if (!reader1.IsDBNull(0))
                        {
                            uptosMW += (double)reader1.GetDecimal(0);
                            Logeriter.writeLog("getting uptos MW" + startDate + "uptosMW" + uptosMW);
                        }
                    }
                    reader1.Close();
                }
                reader.Close();
                VayuDbConnection.Close();
                VayuDbConnection.Close();
            }
        }
        /// <summary>
        /// Gets the monthly fees.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="virtualFees">The virtual fees.</param>
        /// <param name="uptosFees">The uptos fees.</param>
        private double GetGrossPnl(DateTime startDate, DateTime endDate, string account)
        {
            try
            {
                SqlCommand getGrossCmd = VayuDbConnection.CreateCommand();
                getGrossCmd.CommandText = "select sum(Pnl) from VIRTUAL_PNL a join portfolio b on a.PortfolioKey = b.PORTFOLIO_ID " +
                                            " where PnlDate >= @startDate and PnlDate < @endDate and account = @account ";
                getGrossCmd.Connection = VayuDbConnection;
                getGrossCmd.Parameters.AddWithValue("@startDate", startDate);
                getGrossCmd.Parameters.AddWithValue("@endDate", endDate);
                getGrossCmd.Parameters.AddWithValue("@account", account);
                if (VayuDbConnection.State == ConnectionState.Closed)
                {
                    VayuDbConnection.Open();
                }
                double grossPnl = Convert.ToDouble(getGrossCmd.ExecuteScalar());
                #region oldCode
                //while (rdr.Read())
                //{
                //    Pnl pnl = new Pnl();
                //    string account = rdr.IsDBNull(0) ? "" : rdr.GetValue(0).ToString();
                //    pnl.PortfolioKey = rdr.IsDBNull(1) ? 0 : Convert.ToInt32(rdr.GetValue(1));
                //    DateTime pnlDate = rdr.IsDBNull(2) ? DateTime.MaxValue : Convert.ToDateTime(rdr.GetValue(2));
                //    pnl.GrossPnl = rdr.IsDBNull(0) ? 0 : Convert.ToDouble(rdr.GetValue(3));
                //    if (!tempgrossPnlHash.ContainsKey(account))
                //    {
                //        Dictionary<int, double> temp1Hash = new Dictionary<int, double>();
                //        Dictionary<DateTime, List<Pnl>> temp2Hash = new Dictionary<DateTime, List<Pnl>>();
                //        List<Pnl> pnlList = new List<Pnl>();
                //        pnlList.Add(pnl);
                //        temp2Hash.Add(pnlDate, pnlList);
                //        tempgrossPnlHash.Add(account, temp2Hash);
                //    }
                //    else
                //    {
                //        Dictionary<DateTime, List<Pnl>> temp1Hash = tempgrossPnlHash[account];
                //        List<Pnl> tempList = temp1Hash[pnlDate];
                //        tempList.Add(pnl);
                //    }
                //}
                //rdr.Close();

                //Dictionary<DateTime, List<Pnl>> grossPnlHash = new Dictionary<DateTime, List<Pnl>>();
                //foreach (string account in accounts)
                //{
                //    if (!tempgrossPnlHash.ContainsKey(account))
                //    {
                //        grossPnlHash = tempgrossPnlHash[account]; 
                //    }
                //    else
                //    {
                //     //   List<Pnl>tempPnl:ist = grossPnlHash[]
                //    }
                //}
                #endregion
                VayuDbConnection.Close();
                return grossPnl;
            }
            catch (Exception ex)
            {

                return 0.0;
            }
        }
        private void GetMonthlyFees(DateTime startDate, string user, string password, out double virtualFees, out double uptosFees, out double netPnl)
        {
            virtualFees = 0;
            uptosFees = 0;
            netPnl = 0;
            try
            {
                // startDate = new DateTime(2014, 09, 01);
                Logeriter.writeLog("Getting monthly fees from monthly fees file" + startDate);

                DateTime lastDate = startDate.AddMonths(1).AddDays(-1);
                string sourcePath = "https://msrs.pjm.com/msrs/browserless.do?report=monthlybillingstatement-csvandxml&version=L&format=X&start=" +
                    startDate + "&stop=" + lastDate + "&username=" + user + "&password=" + password;
                HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create(sourcePath);
                HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
                StreamReader sr = new StreamReader(wrp.GetResponseStream());
                string strXML = sr.ReadToEnd();
                StringReader objsr = new StringReader(strXML);
                DataSet dsXML = new DataSet();
                dsXML.ReadXml(objsr);
                if (dsXML.Tables["ROW"] != null)
                {
                    if (dsXML.Tables["ROW"].Rows.Count > 0)
                    {
                        foreach (DataRow dr in dsXML.Tables["ROW"].Rows)
                        {
                            if (!dr.IsNull("CHARGES"))
                            {
                                int charges = Convert.ToInt32(dr["CHARGES"]);
                                if (charges <= 1314 && charges >= 1300)
                                {
                                    var row = (from r in dsXML.Tables["AMOUNT"].AsEnumerable()
                                               where r.Field<int>("ROW_Id") == Convert.ToInt32(dr["ROW_Id"])
                                               select r).ToArray();
                                    uptosFees += Convert.ToDouble(row[0]["AMOUNT_Text"]);
                                }
                                if (charges > 1314)
                                {
                                    var row = (from r in dsXML.Tables["AMOUNT"].AsEnumerable()
                                               where r.Field<int>("ROW_Id") == Convert.ToInt32(dr["ROW_Id"])
                                               select r).ToArray();
                                    virtualFees += Convert.ToDouble(row[0]["AMOUNT_Text"]);
                                }
                                Logeriter.writeLog("Getting monthly fees from monthly fees file" + startDate + "\tvirtualFees" + virtualFees + "\tuptoFees" + uptosFees);
                            }
                            if (!dr.IsNull("MONTHLY_NET_TOTAL"))
                            {
                                string NEtPnlString = dr["MONTHLY_NET_TOTAL"].ToString();
                                netPnl = Convert.ToDouble(NEtPnlString);
                                netPnl = -1 * netPnl;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logeriter.writeLog(ex.Message);
            }
        }
        /// <summary>
        /// Gets the month to date fees.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="virtualHash">The virtual hash.</param>
        /// <param name="uptosHash">The uptos hash.</param>
        private void GetMonthToDateFees(DateTime startDate, string user, string password, out Dictionary<DateTime, double> virtualHash, out Dictionary<DateTime, double> uptosHash)
        {
            #region OldFeesCalcCode
            Logeriter.writeLog("Getting monthly fees from monthToDate  fees file" + startDate);

            DateTime lastDate = startDate.AddMonths(1).AddDays(-1);
            virtualHash = new Dictionary<DateTime, double>();
            uptosHash = new Dictionary<DateTime, double>();
            string SourcePath = "https://msrs.pjm.com/msrs/browserless.do?report=month-to-datebill-csvandxml&version=L&format=X&start=" + startDate + "&stop=" +
                                                lastDate + "&username=" + user + "&password=" + password;
            HttpWebRequest wrqq = (HttpWebRequest)HttpWebRequest.Create(SourcePath);
            HttpWebResponse wrpp = (HttpWebResponse)wrqq.GetResponse();
            StreamReader srr = new StreamReader(wrpp.GetResponseStream());
            string strXData = srr.ReadToEnd();
            XmlDataDocument xmlDocument = new XmlDataDocument();
            xmlDocument.LoadXml(strXData);
            foreach (XmlNode Rowset in xmlDocument.GetElementsByTagName("CHARGES"))
            {
                foreach (XmlNode row in Rowset.ChildNodes)
                {
                    foreach (XmlNode childnode in row.ChildNodes)
                    {
                        if (childnode.Name.Contains("DAY"))
                        {
                            int day = Int16.Parse(childnode.Name.Replace("DAY", ""));
                            DateTime date = startDate.AddDays(day - 1);
                            double fees = double.Parse(childnode.InnerText);
                            if (Convert.ToInt32(row.FirstChild.InnerText) <= 1314 && Convert.ToInt32(row.FirstChild.InnerText) >= 1301)
                            {
                                if (uptosHash.ContainsKey(date))
                                {
                                    fees += uptosHash[date];
                                    uptosHash.Remove(date);
                                    Logeriter.writeLog("calculating uptos feese" + startDate + "\t fees" + fees);

                                }
                                uptosHash.Add(date, fees);
                            }
                            if (Convert.ToInt32(row.FirstChild.InnerText) > 1314)
                            {
                                if (virtualHash.ContainsKey(date))
                                {
                                    fees += virtualHash[date];
                                    virtualHash.Remove(date);
                                    Logeriter.writeLog("calculating virtuals feese" + startDate + "\t fees" + fees);
                                }
                                virtualHash.Add(date, fees);
                            }
                        }
                    }
                }
            }
            #endregion
        }
        public string GetToken(string user, string password)
        {
            try
            {
               
                Dictionary<string, string> TokenResponseHash = new Dictionary<string, string>();              
                HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create("https://sso.pjm.com/access/authenticate/");

                wrq.ContentType = "application/json";
                wrq.Headers.Set("X-OpenAM-Username", user);
                wrq.Headers.Set("X-OpenAM-Password", password);
                wrq.ContentLength = 0;
                wrq.ProtocolVersion = System.Net.HttpVersion.Version11;
                wrq.Method = "POST";
                wrq.Accept = "*/*";
                wrq.Timeout = 240000;
                //
                //StreamWriter sw = default(StreamWriter);
                //sw = new StreamWriter(wrq.GetRequestStream());
                //sw.Write(sw.ToString());
                //sw.Close();
                HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
                StreamReader sr = new StreamReader(wrp.GetResponseStream());
                string responseData = sr.ReadToEnd();
                string tokenId = string.Empty;
                TokenResponseHash = responseData.ToString().Remove(0, 1).Split(',').Select(value => value.Split(':')).ToDictionary(pair => pair[0].Replace("\"", ""), pair => pair[1].Replace("\"", ""));
                tokenId = TokenResponseHash["tokenId"];
                Console.WriteLine(DateTime.Now + " : " + tokenId);
                sr.Close();
                return tokenId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Unable to Authenticate");
                return null;
            }
        }
        private double GetNetPnlFromMonthToDateStmt(DateTime startDate, string user, string password)
        {
            string tokenId = GetToken(user,password);
            double totalNetPnl = 0;
            DateTime lastDate = startDate.AddMonths(1).AddDays(-1);
            //uptosHash = new Dictionary<DateTime, double>();      
            string SourcePath = "https://msrsapp.pjm.com/msrs/rest/secure/download/reports?version=L&shortName=month-to-datebill-csvandxml&format=X&start=" + startDate.ToString("MM-dd-yyyy") + "&stop=" + lastDate.ToString("MM-dd-yyyy");//+ "&username=" + user + "&password=" + password;
            WebRequest request = WebRequest.Create(SourcePath);
            HttpWebRequest httpreq = (HttpWebRequest)request;
            httpreq.Method = "GET";
            httpreq.ContentType = "text/csv";
            httpreq.ProtocolVersion = HttpVersion.Version11;
            httpreq.KeepAlive = true;
            httpreq.Headers.Add("Cookie", "pjmauth="+tokenId);           
            HttpWebResponse wrpp = (HttpWebResponse)httpreq.GetResponse();
            StreamReader srr = new StreamReader(wrpp.GetResponseStream());
            string strXData = srr.ReadToEnd();
            XmlDataDocument xmlDocument = new XmlDataDocument();
            xmlDocument.LoadXml(strXData);
            foreach (XmlNode Rowset in xmlDocument.GetElementsByTagName("MONTHLY_NET_TOTAL"))
            {
                foreach (XmlNode row in Rowset.ChildNodes)
                {
                    foreach (XmlNode childnode in row.ChildNodes)
                    {
                        if (childnode.Name.Contains("DAY"))
                        {
                            int day = Int16.Parse(childnode.Name.Replace("DAY", ""));
                            int date = startDate.Day;
                            //double fees = double.Parse(childnode.InnerText);
                            if (day == date)
                            {
                                totalNetPnl = double.Parse(childnode.InnerText);
                            }

                        }
                    }
                }
            }
            return totalNetPnl;
        }

       

        /// <summary>
        /// Saves the PJM fees.
        /// </summary>
        private void SavePJMFees()
        {
            try
            {
                Logeriter.writeLog("Saving  Pjm Fees");
                VayuDbConnection.Open();
                VayuDbConnection.Open();
                SqlDataReader reader = mSelectUserDetailsCommand.ExecuteReader();
                double lastVirtualRate = 0;
                double lastUptosRate = 0;
                //DateTime lastDate = DateTime.Today;
                DateTime lastDate = DateTime.Today;
                Dictionary<string, List<string>> accountLoginHash = new Dictionary<string, List<string>>();
                while (reader.Read())
                {
                    string accountOld = reader.GetString(0);
                    string userOld = reader.GetString(1);
                    string passwordOld = reader.GetString(2);

                    try
                    {
                        if (accountLoginHash.ContainsKey(userOld + "\t" + passwordOld))
                        {
                            accountLoginHash[userOld + "\t" + passwordOld].Add(accountOld);
                        }
                        else
                        {
                            accountLoginHash.Add(userOld + "\t" + passwordOld, new List<string> { accountOld });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(MethodInfo.GetCurrentMethod().Name, ex.Message);
                    }
                }
                reader.Close();
                VayuDbConnection.Close();

                foreach (var item in accountLoginHash.Keys)
                {
                    string user = "", password = "";
                    string[] uspass = item.Split('\t');
                    if (uspass.Length == 2)
                    {
                        user = uspass[0];
                        password = uspass[1];
                    }

                    foreach (var account in accountLoginHash[item])
                    {
                        DateTime startDate = mPjmStartDate;
                       while (startDate < mPjmEndDate)
                        {
                            double virtualFees = 0;
                            double uptosFees = 0;
                            double virtualMW = 0;
                            double uptosMW = 0;
                            double netPnl = 0;
                            try
                            {
                                //  GetMonthlyFees(startDate, user, password, out virtualFees, out uptosFees, out netPnl);
                               netPnl =  -1 *  GetNetPnlFromMonthToDateStmt(startDate, user, password);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                startDate = startDate.AddDays(1);
                                continue;
                            }
                            #region OldCode
                            //if (uptosFees == 0 && virtualFees == 0)
                            //{
                            //    Dictionary<DateTime, double> virtualHash = new Dictionary<DateTime, double>();
                            //    Dictionary<DateTime, double> uptosHash = new Dictionary<DateTime, double>();
                            //    try
                            //    {
                            //        GetNetPnlFromMonthToDateStmt(startDate, user, password);
                            //        //  GetMonthToDateFees(startDate, user, password, out virtualHash, out uptosHash);
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        Console.WriteLine(ex.Message);
                            //    }
                            //    DateTime tempDate = startDate;
                            //    while (tempDate < DateTime.Today)
                            //    {
                            //        virtualFees = 0;
                            //        if (virtualHash.ContainsKey(tempDate))
                            //        {
                            //            virtualFees = virtualHash[tempDate];
                            //        }
                            //        uptosFees = 0;
                            //        if (uptosHash.ContainsKey(tempDate))
                            //        {
                            //            uptosFees = uptosHash[tempDate];
                            //        }
                            //        GetMW(tempDate, tempDate.AddDays(1), out virtualMW, out uptosMW, accountLoginHash[item]);
                            //        double virtualRate = 0;
                            //        double uptosRate = 0;
                            //        if (virtualFees != 0 && virtualMW != 0)
                            //        {
                            //            virtualRate = virtualFees / virtualMW;
                            //            lastVirtualRate = virtualRate;
                            //            lastDate = startDate;
                            //        }
                            //        if (uptosFees != 0 && uptosMW != 0)
                            //        {
                            //            uptosRate = uptosFees / uptosMW;
                            //            lastUptosRate = uptosRate;
                            //            lastDate = startDate;
                            //        }
                            //        if (virtualRate == 0 && uptosRate == 0)
                            //        {
                            //            if (lastUptosRate == 0 && lastVirtualRate == 0)
                            //            {
                            //                SaveFees(account, tempDate, 1.5, 0.07);
                            //            }
                            //            else
                            //            {
                            //                SaveFees(account, tempDate, lastVirtualRate, lastUptosRate);
                            //            }
                            //        }
                            //        else
                            //        {
                            //            SaveFees(account, tempDate, virtualRate, uptosRate);
                            //        }
                            //        tempDate = tempDate.AddDays(1);
                            //    }
                            //}
                            // else 
                            #endregion
                            {
                                //  DateTime mwStartdate = startDate.AddDays(-(startDate.Day - 1));
                                DateTime mwStartdate = startDate;
                                double grossPnl = GetGrossPnl(mwStartdate, mwStartdate.AddDays(1), account);
                                double totalMw = GetTotalMw(mwStartdate, mwStartdate.AddDays(1), account);
                                double virtualRate = 0;
                                double uptosRate = (grossPnl - netPnl) / totalMw;
                                if (virtualFees != 0 && virtualMW != 0)
                                {
                                    virtualRate = virtualFees / virtualMW;
                                    lastVirtualRate = virtualRate;
                                    lastDate = startDate;
                                }
                                else
                                {
                                    double totalFeesForAc = grossPnl - netPnl;
                                    uptosRate = totalFeesForAc / totalMw;
                                }

                                if (uptosFees != 0 && uptosMW != 0)
                                {
                                    // uptosRate = uptosFees / uptosMW;
                                    // lastUptosRate = uptosRate;
                                    //  lastDate = startDate;

                                }
                                DateTime tempDate = mwStartdate;
                                DateTime lastTempDate = tempDate.AddMonths(1).AddDays(-1);
                                if (lastTempDate > DateTime.Today)
                                {
                                    lastTempDate = DateTime.Today;
                                }
                           //     while (tempDate <= lastTempDate)
                                {
                                    SaveFees(account, tempDate, virtualRate, uptosRate);
                                  //  tempDate = tempDate.AddDays(1);
                                }
                            }
                            //  startDate = startDate.AddMonths(1);
                            startDate = startDate.AddDays(1);
                        }
                    }
                }
                VayuDbConnection.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private double GetTotalMw(DateTime startDate, DateTime endDate, string account)
        {
            try
            {
                SqlCommand getTotalMwCmd = VayuDbConnection.CreateCommand();
                getTotalMwCmd.CommandText = "select sum(clearedMw) from ClearedEES a join portfolio b on a.PortfolioKey = b.PORTFOLIO_ID " +
                                            " where marketdatetime > @startDate and marketdatetime <= @endDate and account = @account ";
                getTotalMwCmd.Connection = VayuDbConnection;
                getTotalMwCmd.Parameters.AddWithValue("@startDate", startDate);
                getTotalMwCmd.Parameters.AddWithValue("@endDate", endDate);
                getTotalMwCmd.Parameters.AddWithValue("@account", account);
                if (VayuDbConnection.State == ConnectionState.Closed)
                {
                    VayuDbConnection.Open();
                }
                double totalMw = Convert.ToDouble(getTotalMwCmd.ExecuteScalar());
                return totalMw;
            }
            catch (Exception ex)
            {

                return 0.0;
            }
        }
        #endregion
    }
}



