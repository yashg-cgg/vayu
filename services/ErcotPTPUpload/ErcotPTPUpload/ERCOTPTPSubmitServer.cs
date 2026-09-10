using System;
using System.Collections.Generic;
using Vayu.ErcotSubmissionLibrary;
using System.ServiceModel;
using System.Data;
using System.Xml;
using System.Data.SqlClient;
using Vayu.ErcotPTPSubmissionLibrary;

namespace Vayu.ErcotPTPUpload
{
    class ERCOTPTPSubmitServer : IBidSubmit
    {
        private static readonly object lockObj = new object();
        private SqlConnection alphaDbConnection = new SqlConnection();
        private static Dictionary<ISubmitResultCallback, int> mSubscriberHash = new Dictionary<ISubmitResultCallback, int>();
        public static List<string> successlist = new List<string>();
        public static List<string> successmsglist = new List<string>();
        public static List<string> errorlist = new List<string>();
        private SqlCommand mSelectMultiplierCommand;
        private SqlCommand mSelectRiskCommand;
        private SqlCommand mUpdateValidRiskCommand;
        public static string dburl = Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection();
        private static  Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        private static Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;

        private void InitDB()
        {
            alphaDbConnection = new SqlConnection(dburl);
            mSelectMultiplierCommand = new SqlCommand();
            mSelectMultiplierCommand.CommandText = "select multiplier, account2 from Vayu..firmaccount where account = @portfolio_id and multiplier > 0";
            mSelectMultiplierCommand.Parameters.AddWithValue("@portfolio_id", "portfolio_id");
            mSelectMultiplierCommand.Connection = alphaDbConnection;
            //
            mSelectRiskCommand = new SqlCommand();
            mSelectRiskCommand.CommandText = "select requestedmw from Vayu..ercotptpbids where portfoliokey = 3012 and EndMarketDateTime = @EndMarketDateTime " +
                                                "and source = @source and sink = @sink and price = @price and BidStatus <> 'valid'";
            mSelectRiskCommand.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
            mSelectRiskCommand.Parameters.AddWithValue("@source", "source");
            mSelectRiskCommand.Parameters.AddWithValue("@sink", "sink");
            mSelectRiskCommand.Parameters.AddWithValue("@price", "price");
            mSelectRiskCommand.Connection = alphaDbConnection;
            //
            mUpdateValidRiskCommand = new SqlCommand();
            mUpdateValidRiskCommand.CommandText = "update Vayu..ercotptpbids set ScheduleID = @ScheduleID, bidstatus = 'Valid' where portfoliokey = 3012 and EndMarketDateTime = @EndMarketDateTime " +
                                                "and source = @source and sink = @sink and price = @price";
            mUpdateValidRiskCommand.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
            mUpdateValidRiskCommand.Parameters.AddWithValue("@source", "source");
            mUpdateValidRiskCommand.Parameters.AddWithValue("@sink", "sink");
            mUpdateValidRiskCommand.Parameters.AddWithValue("@price", "price");
            mUpdateValidRiskCommand.Parameters.AddWithValue("@ScheduleID", "ScheduleID");
            mUpdateValidRiskCommand.Connection = alphaDbConnection;
        }
        public void Connect()
        {
            ServiceHost host = new ServiceHost(typeof(ERCOTPTPSubmitServer), new Uri("net.tcp://localhost:8004"));//  7022
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.OpenTimeout = new TimeSpan(0, 120, 0);
                binding.SendTimeout = new TimeSpan(0, 120, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 120, 0);
                binding.CloseTimeout = new TimeSpan(0, 120, 0);
                binding.MaxBufferSize = 1500000;
                binding.MaxReceivedMessageSize = 1500000;
                binding.Security.Mode = SecurityMode.None;
                host.AddServiceEndpoint(typeof(IBidSubmit), binding, "ISubscribe");
                try
                {
                    host.Open();
                    Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": started ercot submission server");
                    while (true)
                    {
                        Console.ReadLine();
                    }
                    host.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public bool Subscribe(int traderid)
        {
            Console.WriteLine("Connected");
            lock (lockObj)
            {
                try
                {
                    //Get the hashCode of the connecting app and store it as a connection
                    ISubmitResultCallback callback = OperationContext.Current.GetCallbackChannel<ISubmitResultCallback>();
                    if (!mSubscriberHash.ContainsKey(callback))
                    {
                        mSubscriberHash.Add(callback, traderid);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }
        public bool HeartBeat()
        {
            Console.WriteLine(DateTime.Now + " HeartBeat");
            return true;
        }
        public bool Unsubscribe()
        {
            lock (lockObj)
            {
                try
                {
                    //remove any connection that is leaving
                    ISubmitResultCallback callback = OperationContext.Current.GetCallbackChannel<ISubmitResultCallback>();
                    if (mSubscriberHash.ContainsKey(callback))
                    {
                        mSubscriberHash.Remove(callback);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public string SubmitBids(PTPBid[] bids, int submittype)
        {
            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdClientAPI3");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdServerCert");
            Console.WriteLine("Connected to Submit Method");
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": submitbids");
            TimerArgs t = new TimerArgs();
            t.bids = bids;
            t.submittype = submittype;
            string success = "";
            Tuple<double, int> multiplierTuple = IsValidPortfolio(bids[0].PortfolioKey);
            Console.WriteLine("Multiplier is " + multiplierTuple.Item1);
            PTPBid[] returnBids = FillBids(bids, multiplierTuple.Item1, true);
            t.bids = returnBids;
            t.submittype = submittype;
            success = UploadPTP(t);
            return success;
        }
        private Tuple<double, int> IsValidPortfolio(int portfolio)
        {
            double multiplier = 0;
            int portfolio2 = 0;
            try
            {
                InitDB();
                alphaDbConnection.Open();
                mSelectMultiplierCommand.Parameters["@portfolio_id"].Value = portfolio;
                SqlDataReader reader = mSelectMultiplierCommand.ExecuteReader();
                while (reader.Read())
                {
                    multiplier = reader.GetDouble(0);
                    portfolio2 = reader.GetInt32(1);
                }
                reader.Close();
                alphaDbConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new Tuple<double, int>(multiplier, portfolio2);
        }
        public string UploadPTP(Object data)
        {
            string success = "Error";
            try
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": uploadptp");
                ErcotPTP myPTPUpload = new ErcotPTP();
                PTPBid[] bids = ((TimerArgs)data).bids;
                int submittype = ((TimerArgs)data).submittype;
                string resp = myPTPUpload.Upload(bids, submittype, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path, userCertificateDetails.UserName);
              //  string resp = myPTPUpload.Upload(bids, submittype, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path, "API_mhancock2022032");

                success = "Success";
                PopulateTable(bids, ((TimerArgs)data).submittype);
                DataTable dt = CreateDataTable();
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml("<root>" + resp + "</root>");
                string mridstr = string.Empty;
                foreach (XmlNode resultnode in xdoc.GetElementsByTagName("ns1:PTPObligation"))
                {
                    mridstr = resultnode["ns1:mRID"].InnerText;
                    string statusstr = resultnode["ns1:status"].InnerText;
                    Console.WriteLine("mrid: " + mridstr + " statusstr: " + statusstr);
                    string[] elems = mridstr.Split('.');
                    string bidid = elems[3];
                    dt.Rows.Add(bidid, null, null, null, null, null, null, statusstr, null, null, null);
                }

                Tuple<double, int> multiplierTuple = IsValidPortfolio(bids[0].PortfolioKey);
                Console.WriteLine("Multiplier is " + multiplierTuple.Item1);
                UpdateDB(dt, bids[0].PortfolioKey);
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": ex.Message");
            }
            return success;
        }
        private PTPBid[] FillBids(PTPBid[] ptpBids, double multiplier, bool isSubmit)
        {
                alphaDbConnection.Open();


            foreach (PTPBid ptpBid in ptpBids)
            {
                if (isSubmit)
                {
                    mSelectRiskCommand.Parameters["@source"].Value = ptpBid.Source;
                    mSelectRiskCommand.Parameters["@sink"].Value = ptpBid.Sink;
                    mUpdateValidRiskCommand.Parameters["@source"].Value = ptpBid.Source;
                    mUpdateValidRiskCommand.Parameters["@sink"].Value = ptpBid.Sink;
                    mUpdateValidRiskCommand.Parameters["@ScheduleID"].Value = ptpBid.BidId;
                    foreach (BidValues bidValue in ptpBid.Bidvals)
                    {
                        int hr = bidValue.Hour;
                        string endmarketdatetime = DateTime.Today.AddDays(1).AddHours(hr).ToString("yyyy-MM-dd HH:mm:ss");
                        mSelectRiskCommand.Parameters["@EndMarketDateTime"].Value = endmarketdatetime;
                        mSelectRiskCommand.Parameters["@price"].Value = bidValue.Price;
                        mUpdateValidRiskCommand.Parameters["@EndMarketDateTime"].Value = endmarketdatetime;
                        mUpdateValidRiskCommand.Parameters["@price"].Value = bidValue.Price;
                        double totalMW = 0;
                        SqlDataReader reader = mSelectRiskCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            totalMW += (double)reader.GetDecimal(0);
                        }
                        reader.Close();
                        mUpdateValidRiskCommand.ExecuteNonQuery();
                        bidValue.MW = totalMW + bidValue.MW;
                    }
                }
            }
            alphaDbConnection.Close();
            return ptpBids;
        }
        public void PopulateTable(PTPBid[] b, int submittype)
        {
            DataTable d = CreateDataTable();
            for (int i = 0; i < 1; i++)
            {
                string source = b[i].Source;
                string sink = b[i].Sink;
                string bidid = b[i].BidId;
                int portfoliokey = b[i].PortfolioKey;
                int totalhours = b[i].Bidvals.Length;
                for (int j = 0; j < 1; j++)
                {
                    int hr = b[i].Bidvals[j].Hour;
                    string endmarketdatetime = DateTime.Today.AddDays(1).AddHours(hr).ToString("yyyy-MM-dd HH:mm:ss");
                    double price = b[i].Bidvals[j].Price;
                    double mw = b[i].Bidvals[j].MW;
                    d.Rows.Add(bidid, source, sink, price, endmarketdatetime, mw, 0, null, submittype, portfoliokey, DateTime.Now.ToString());
                }
            }
            UpdateDB(d, b[0].PortfolioKey);
        }
        public DataTable CreateDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BidID", typeof(string));
            dt.Columns.Add("Source", typeof(string));
            dt.Columns.Add("Sink", typeof(string));
            dt.Columns.Add("Price", typeof(double));
            dt.Columns.Add("EndMarketDateTime", typeof(DateTime));
            dt.Columns.Add("RequestedMW", typeof(double));
            dt.Columns.Add("ClearedMW", typeof(double));
            dt.Columns.Add("BidStatus", typeof(string));
            dt.Columns.Add("EndUserKey", typeof(int));
            dt.Columns.Add("PortfolioKey", typeof(double));
            dt.Columns.Add("SubmittedDateTime", typeof(DateTime));
            return dt;
        }
        public void UpdateDB(DataTable dt, int portfolioKey)
        {
            SqlConnection con = new SqlConnection(dburl);
            con.Open();
            string status = "Valid";
            try
            {
                string updatequery = "update Vayu..ercotptpbids set BidStatus ='" + status + "' where PortfolioKey = '" + portfolioKey.ToString() + "' and EndMarketDateTime > '" + DateTime.Today.Date.AddDays(1).ToString() + "' and EndMarketDateTime <= '" + DateTime.Today.Date.AddDays(2).ToString() + "'";
                SqlCommand updateCommand = new SqlCommand(updatequery, con);
                int updatedrows = updateCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            con.Close();
        }
        public void UpdateDBCancel(int portfolioKey, PTPBid[] bids)
        {
            SqlConnection con = new SqlConnection(dburl);
            con.Open();
            string status = "Invalid";
            try
            {
                string updatequery = "update Vayu..ercotptpbids set BidStatus ='" + status + "' where PortfolioKey = '" + portfolioKey.ToString() + "' and EndMarketDateTime > '" +
                    DateTime.Today.Date.AddDays(1).ToString() + "' and EndMarketDateTime <= '" + DateTime.Today.Date.AddDays(2).ToString() + "' and BidStatus = 'Valid' ";
                SqlCommand updateCommand = new SqlCommand(updatequery, con);
                int updatedrows = updateCommand.ExecuteNonQuery();
                string updatRiskequery = "update Vayu..ercotptpbids set BidStatus ='" + status + "' where PortfolioKey = '3012' and EndMarketDateTime > '" +
                   DateTime.Today.Date.AddDays(1).ToString() + "' and EndMarketDateTime <= '" + DateTime.Today.Date.AddDays(2).ToString() +
                   "' and BidStatus = 'Valid' and scheduleid like '" + portfolioKey.ToString() + "%'";
                SqlCommand updateRiskCommand = new SqlCommand(updatRiskequery, con);
                int updatedRiskrows = updateRiskCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            con.Close();
        }
        public string CancelBids(PTPBid[] bids, int submittype)
        {

            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdServerCert");

            //userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdClientAPI");
            //serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdServerCert");

            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": submitbids");
            TimerArgs t = new TimerArgs();
            t.bids = bids;
            t.submittype = submittype;
            int portfolioKey = bids[0].PortfolioKey;
            Tuple<double, int> multiplierTuple = IsValidPortfolio(t.bids[0].PortfolioKey);
            PTPBid[] cancelBids = FillBids(t.bids, multiplierTuple.Item1, false);
            t.bids = cancelBids;
            t.submittype = submittype;
            CancelPTP(t);
            UpdateDBCancel(portfolioKey, bids);
            return "Cancelled Successfully" + portfolioKey.ToString();
        }
        private void CancelPTP(object data)
        {
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": cancelptp");
            ErcotPTP myPTPUpload = new ErcotPTP();
            PTPBid[] bids = ((TimerArgs)data).bids;
            int submittype = ((TimerArgs)data).submittype;
            string resp = myPTPUpload.Cancel(bids, submittype, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path);
        }
    }

    public class TimerArgs
    {
        public ErcotSubmissionLibrary.PTPBid[] bids;
        public ISubmitResultCallback callback;
        public int submittype;
    }
}
