//#define TEST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Net;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Xml;
using Vayu.CommonAccessLibrary;

namespace Vayu.PNLDownload
{
    /// <summary>
    /// Downlload the PJM Reserve Rates
    /// </summary>
    class PJMReserveRatesDownload
    {
        #region Private Members
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuDbConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
        /// <summary>
        /// The connection
        /// </summary>
        private SqlConnection mConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
        /// <summary>
        /// The Vayu database connection1
        /// </summary>
        private SqlConnection VayuDbConnection1 = new VayuDBConnection().GetInstance().GetSqlConnection();
        /// <summary>
        /// The select cleared bids command
        /// </summary>
        private SqlCommand mSelectClearedBidsCommand;
        /// <summary>
        /// The update virtual PNL command
        /// </summary>
        private SqlCommand mUpdateVirtualPnlCommand;
        /// <summary>
        /// The select portfolio command
        /// </summary>
        private SqlCommand mSelectPortfolioCommand;
        /// <summary>
        /// The select user details command
        /// </summary>
        private SqlCommand mSelectUserDetailsCommand;
        /// <summary>
        /// The select woe user details command
        /// </summary>
        private SqlCommand mSelectWOEUserDetailsCommand;
        /// <summary>
        /// The username
        /// </summary>
        private string mUsername;
        /// <summary>
        /// The password
        /// </summary>
        private string mPassword; 
        #endregion

        /// <summary>
        /// The datatable user details
        /// </summary>
        DataTable dtUserDetails = new DataTable();

        #region Public Method
        /// <summary>
        /// Updates the fees.
        /// </summary>
        public void UpdateFees()
        {
            InitDB();
            GetUserDetails();
            GetReserveRatesDailyDownload();
        } 
        #endregion
        #region Commented Code
        //public void UpdateWOEFees()
        //{
        //    InitDB();
        //    GetWOEUserDetails();
        //    GetReserveRatesMonthlyDownload();
        //    GetReserveRatesDailyDownload();
        //} 
        #endregion

        #region Private Method
        /// <summary>
        /// Initializes the database related object.
        /// </summary>
        private void InitDB()
        {
            mSelectPortfolioCommand = new SqlCommand();
            mSelectPortfolioCommand.CommandText = "SELECT PORTFOLIO_ID FROM PORTFOLIO where HUB = 'PJM' and PRODUCT = 'VIRTUAL'";
            mSelectPortfolioCommand.Connection = VayuDbConnection1;
            //
            mSelectClearedBidsCommand = new SqlCommand();
            mSelectClearedBidsCommand.CommandText = "select PortfolioKey, SUM(clearedmw) from ClearedEES where MarketDateTime > @startDateTime and MarketDateTime <= @endDateTime group by PortfolioKey";
            mSelectClearedBidsCommand.Parameters.AddWithValue("@startDateTime", "MarketDateTime");
            mSelectClearedBidsCommand.Parameters.AddWithValue("@endDateTime", "MarketDateTime");
            mSelectClearedBidsCommand.Connection = VayuDbConnection;
            //
            mUpdateVirtualPnlCommand = new SqlCommand();
            mUpdateVirtualPnlCommand.CommandText = "update VIRTUAL_PNL set Fee=@Fee, feeUpdateTime=@feeUpdateTime where PortfolioKey=@PortfolioKey and PnlDate=@PnlDate";
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@Fee", "Fee");
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@PnlDate", "PnlDate");
            mUpdateVirtualPnlCommand.Parameters.AddWithValue("@feeUpdateTime", "feeUpdateTime");
            mUpdateVirtualPnlCommand.Connection = mConnection;
            //
            mSelectUserDetailsCommand = new SqlCommand();
            mSelectUserDetailsCommand.CommandText = "select distinct PJM_UserDetails.MarketKey, PJM_UserDetails.Name, PJM_UserDetails.PortfolioKey, PJM_UserDetails.Username, ISOUserData.[Password], PJM_UserDetails.Virtual_EES FROM PJM_UserDetails inner join ISOUserData on PJM_UserDetails.Username=ISOUserData.UserID";
            mSelectUserDetailsCommand.Connection = VayuDbConnection;
            //
            mSelectWOEUserDetailsCommand = new SqlCommand();
            mSelectWOEUserDetailsCommand.CommandText = "select MarketKey,AccountDesc as Name, AccountID as PortfolioKey,UserID as Username,Password,MarketKey as Virtual_EES from ISOUserData  where Active=1 and MarketKey=1";
            mSelectWOEUserDetailsCommand.Connection = VayuDbConnection;
        }

        /// <summary>
        /// Gets the reserve rates daily download.
        /// </summary>
        private void GetReserveRatesDailyDownload()
        {
            try
            {
                int days = DateTime.Today.Day;
#if TEST
                DateTime startDate = DateTime.Parse("02/01/2015");
#else
                DateTime startDate = DateTime.Today.AddMonths(-3).AddDays(-(days - 1));
#endif
#if TEST
                DateTime endDate = DateTime.Parse("03/31/2015");
#else
                DateTime endDate = DateTime.Today;
#endif
                VayuDbConnection.Open();
                mConnection.Open();
                while (startDate <= endDate)
                {
                    DateTime feeDate = startDate;
                    try
                    {
                        FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(@"ftp://ftp.pjm.com/account/oper-reserve-rates/monthly/" + startDate.ToString("yyyyMM") + ".csv"));
                        reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                        reqFTP.UseBinary = true;
                        reqFTP.Credentials = new NetworkCredential("anonymous", "mg@westoaksenergy.com");
                        FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                        DateTime FtpFileLastModified = response.LastModified;
                        Stream ftpStream = response.GetResponseStream();
                        StreamReader sr = new StreamReader(ftpStream);
                        string line = sr.ReadLine();
                        line = sr.ReadLine();
                        line = sr.ReadLine();
                        line = sr.ReadLine();
                        line = sr.ReadLine();
                        while (line != null)
                        {
                            string[] tokens = line.Split(',');
                            double or = double.MinValue;
                            try
                            {
                                DateTime date = DateTime.Parse(tokens[0]);
                                or = double.Parse(tokens[1]) + double.Parse(tokens[3]) + Math.Max(double.Parse(tokens[5]), double.Parse(tokens[7]));
                                mSelectClearedBidsCommand.Parameters["@startDateTime"].Value = feeDate;
                                mSelectClearedBidsCommand.Parameters["@endDateTime"].Value = feeDate.AddDays(1);
                                SqlDataReader reader = mSelectClearedBidsCommand.ExecuteReader();
                                while (reader.Read())
                                {
                                    int portfolioId = (int)reader.GetDecimal(0);
                                    double mw = (double)reader.GetDecimal(1);
                                    double fee = -mw * or;
                                    mUpdateVirtualPnlCommand.Parameters["@Fee"].Value = fee;
                                    mUpdateVirtualPnlCommand.Parameters["@PortfolioKey"].Value = portfolioId;
                                    mUpdateVirtualPnlCommand.Parameters["@PnlDate"].Value = feeDate;
                                    mUpdateVirtualPnlCommand.Parameters["@feeUpdateTime"].Value = DateTime.Now;
                                    mUpdateVirtualPnlCommand.ExecuteNonQuery();
                                }
                                reader.Close();
                                line = sr.ReadLine();
                                feeDate = feeDate.AddDays(1);
                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                        }
                        sr.Close();
                        ftpStream.Close();
                        response.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    startDate = startDate.AddMonths(1);
                    while (feeDate < startDate)
                    {
                        mSelectClearedBidsCommand.Parameters["@startDateTime"].Value = feeDate;
                        mSelectClearedBidsCommand.Parameters["@endDateTime"].Value = feeDate.AddDays(1);
                        SqlDataReader reader = mSelectClearedBidsCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            int portfolioId = (int)reader.GetDecimal(0);
                            double mw = (double)reader.GetDecimal(1);
                            double fee = -mw * 0.04;
                            mUpdateVirtualPnlCommand.Parameters["@Fee"].Value = fee;
                            mUpdateVirtualPnlCommand.Parameters["@PortfolioKey"].Value = portfolioId;
                            mUpdateVirtualPnlCommand.Parameters["@PnlDate"].Value = feeDate;
                            mUpdateVirtualPnlCommand.Parameters["@feeUpdateTime"].Value = DateTime.Now;
                            mUpdateVirtualPnlCommand.ExecuteNonQuery();
                        }
                        reader.Close();
                        feeDate = feeDate.AddDays(1);
                    }
                }
                VayuDbConnection.Close();
                mConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Gets the reserve rates monthly download.
        /// </summary>
        private void GetReserveRatesMonthlyDownload()
        {
            int EndMonth = Convert.ToDateTime("12/31/2011").Month - Convert.ToDateTime("06/01/2011").Month;
            EndMonth = EndMonth + DateTime.Now.Month;
            for (int i = 0; i < dtUserDetails.Rows.Count; i++)
            {
                try
                {
                    DateTime StartDayOfTheMonth = Convert.ToDateTime("05/01/2011");
                    for (int j = 1; j <= EndMonth; j++)
                    {
                        DateTime EndDayOfTheMonth = DateTime.Now;
                        StartDayOfTheMonth = StartDayOfTheMonth.AddMonths(1);

                        EndDayOfTheMonth = StartDayOfTheMonth.AddMonths(1).AddDays(-1);

                        string startdate = StartDayOfTheMonth.ToString("MM/dd/yyyy");  //"07/01/2011"; //
                        string enddate = EndDayOfTheMonth.ToString("MM/dd/yyyy"); //"07/31/2011"; //        

                        mUsername = dtUserDetails.Rows[i]["Username"].ToString();
                        mPassword = dtUserDetails.Rows[i]["Password"].ToString();

                        string SourcePath = "https://msrs.pjm.com/msrs/browserless.do?report=monthlybillingstatement-csvandxml&version=L&format=X&start=" + startdate + "&stop=" + enddate + "&username=" + mUsername + "&password=" + mPassword;
                        HttpWebRequest wrq = (HttpWebRequest)HttpWebRequest.Create(SourcePath);
                        HttpWebResponse wrp = (HttpWebResponse)wrq.GetResponse();
                        StreamReader sr = new StreamReader(wrp.GetResponseStream());
                        string strXML = sr.ReadToEnd();
                        XmlDataDocument xmlDocument = new XmlDataDocument();
                        xmlDocument.LoadXml(strXML);
                        double Fee = 0;
                        foreach (XmlNode Rowset in xmlDocument.GetElementsByTagName("ROWSET"))
                        {
                            foreach (XmlNode row in Rowset.ChildNodes)
                            {
                                foreach (XmlNode rowChildNodes in row.ChildNodes)
                                {

                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Day-ahead Spot Market Energy")
                                        {
                                            if (Convert.ToInt16(row.FirstChild.InnerText) == 1200)
                                            {

                                                if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                                {
                                                    Fee = Convert.ToDouble(row.LastChild.InnerText);
                                                }
                                            }
                                        }
                                    }
                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Balancing Spot Market Energy")
                                        {
                                            if (Convert.ToInt16(row.FirstChild.InnerText) == 1205)
                                            {

                                                if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                                {

                                                    Fee += Convert.ToDouble(row.LastChild.InnerText);
                                                }
                                            }
                                        }
                                    }
                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Day-ahead Transmission Congestion")
                                        {
                                            if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                            {
                                                Fee += Convert.ToDouble(row.LastChild.InnerText);
                                            }
                                        }
                                    }
                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Balancing Transmission Congestion")
                                        {
                                            if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                            {
                                                Fee += Convert.ToDouble(row.LastChild.InnerText);
                                            }
                                        }
                                    }
                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Day-ahead Transmission Losses")
                                        {
                                            if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                            {
                                                Fee += Convert.ToDouble(row.LastChild.InnerText);
                                            }
                                        }
                                    }
                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Balancing Transmission Losses")
                                        {
                                            if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                            {
                                                Fee += Convert.ToDouble(row.LastChild.InnerText);
                                            }
                                        }
                                    }
                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Financial Transmission Rights Auction")
                                        {
                                            if (Convert.ToInt16(row.FirstChild.InnerText) == 1500)
                                            {
                                                if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                                {
                                                    Fee += Convert.ToDouble(row.LastChild.InnerText);
                                                }
                                            }
                                        }
                                    }
                                    if (rowChildNodes.Name == "BILLING_LINE_ITEM_NAME")
                                    {
                                        if (rowChildNodes.InnerText == "Planning Period Congestion Uplift")
                                        {
                                            if (Convert.ToInt16(row.FirstChild.InnerText) == 1218)
                                            {
                                                if (!(Convert.ToDouble(row.LastChild.InnerText) == null))
                                                {
                                                    Fee += Convert.ToDouble(row.LastChild.InnerText);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        string strTOTAL_CHARGES = null;
                        double dTOTAL_CHARGES = 0;
                        foreach (XmlNode xmlNode in xmlDocument.GetElementsByTagName("TOTAL_CHARGES"))
                        {
                            dTOTAL_CHARGES = Convert.ToDouble(xmlNode.InnerText) * (-1);
                        }
                        dTOTAL_CHARGES = dTOTAL_CHARGES + Fee;

                        if (j == DateTime.Now.Month)
                        {
                            SqlDataReader readerClearedBids = null;
                            VayuDbConnection.Open();
                            mSelectClearedBidsCommand.Parameters["@startDateTime"].Value = StartDayOfTheMonth.ToString("MM/dd/yyyy");
                            mSelectClearedBidsCommand.Parameters["@endDateTime"].Value = DateTime.Now.ToString("MM-dd-yyyy");
                            mSelectClearedBidsCommand.Parameters["@Portfoliokey"].Value = dtUserDetails.Rows[i]["PortfolioKey"].ToString();
                            readerClearedBids = mSelectClearedBidsCommand.ExecuteReader();
                            while (readerClearedBids.Read())
                            {
                                if (readerClearedBids[0].ToString() != "")
                                {
                                    double fee = ((Convert.ToDouble(readerClearedBids[0])) * (2.25) * (-1));
                                    mConnection.Open();
                                    mUpdateVirtualPnlCommand.Parameters["@PortfolioKey"].Value = dtUserDetails.Rows[i]["PortfolioKey"].ToString();
                                    mUpdateVirtualPnlCommand.Parameters["@Fee"].Value = fee;
                                    mUpdateVirtualPnlCommand.Parameters["@PnlDate"].Value = DateTime.Now.ToString("MM-dd-yyyy"); //EndDayOfTheMonth.ToString("MM-dd-yyyy");
                                    mUpdateVirtualPnlCommand.Parameters["@feeUpdateTime"].Value = DateTime.Now;
                                    mUpdateVirtualPnlCommand.ExecuteNonQuery();
                                }
                            }
                            mConnection.Close();
                            VayuDbConnection.Close();
                        }
                        strTOTAL_CHARGES = dTOTAL_CHARGES.ToString();
                        mConnection.Open();
                        mUpdateVirtualPnlCommand.Parameters["@PortfolioKey"].Value = dtUserDetails.Rows[i]["PortfolioKey"].ToString();
                        mUpdateVirtualPnlCommand.Parameters["@Fee"].Value = strTOTAL_CHARGES;
                        mUpdateVirtualPnlCommand.Parameters["@PnlDate"].Value = enddate;
                        mUpdateVirtualPnlCommand.Parameters["@feeUpdateTime"].Value = DateTime.Now;
                        mUpdateVirtualPnlCommand.ExecuteNonQuery();
                        mConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                }

            }
        }
        /// <summary>
        /// Gets the user details.
        /// </summary>
        private void GetUserDetails()
        {
            try
            {
                VayuDbConnection.Open();
                dtUserDetails.Load(mSelectUserDetailsCommand.ExecuteReader());
                VayuDbConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Gets the woe user details.
        /// </summary>
        private void GetWOEUserDetails()
        {
            try
            {
                VayuDbConnection.Open();
                dtUserDetails.Load(mSelectWOEUserDetailsCommand.ExecuteReader());
                VayuDbConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        } 
        #endregion

    }
}
