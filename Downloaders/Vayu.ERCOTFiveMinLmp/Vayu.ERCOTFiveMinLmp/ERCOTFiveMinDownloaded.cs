using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Vayu.MarketLibraryErcot;
using Vayu.MarketLibraryErcot.ERCOTNodalService;
using System.ServiceModel.Channels;
using System.IO;
using Vayu.CommonAccessLibrary;
using Serilog;

namespace Vayu.ERCOTFiveMinLmp
{
    class ERCOTFiveMinDownloaded
    {
        private static System.Timers.Timer sTimer = null;

        private static System.Timers.Timer sTimer5Min = null;

        private static System.Timers.Timer sTimer15Min = null;

        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;

        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;

        private SqlConnection VayuDbConn;
        private DataTable mDtFiveMin = new DataTable();
        private DataRow mDrFiveMin = null;
        private SqlCommand mSelectToday15MinCommand;
        private SqlCommand mSelectNodeCommand;
        private SqlCommand mSelectNodeHourCommand;
        private SqlCommand mSelectMaxDateMarketMinCommand;
        private SqlCommand mSelectMaxDateMarketLmpCommand;
        private SqlCommand mSelectMarketMinCommand;
        private SqlCommand mSelectMarketMinDateCommand;
        private SqlCommand mDeleteDownloadSspCommand;
        private SqlCommand mDeleteDownloadLMPCommand;
        private SqlCommand mDeleteCal15MinCommand;
        private SqlCommand mDeleteCalHourlyCommand;
        private SqlCommand mSelectNodeTypeCommand;
        private SqlCommand mDeleteLmpCommand;
        private SqlCommand mDeleteLmpTmpCommand;
        private SqlCommand mDeleteLmphTmpCommand;
        private SqlCommand mInsertNodeCommand;
        private SqlCommand mSelectHourCommand;
        private DataTable mDtLmp = new DataTable();
        private DataRow mDrLmp = null;
        private DataTable mDtLmph = new DataTable();
        private Dictionary<string, int> mNodeHash = new Dictionary<string, int>();
        private List<string> mNodeNameList = new List<string>();


        public ERCOTFiveMinDownloaded()
        {
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .WriteTo.Console()
            //    .WriteTo.File("logs\\ercot-lmp.log", rollingInterval: RollingInterval.Day)
            //    .CreateLogger();
            InitDB();
             FillNodeHash();
            StartTimer();
        }
        public void InitDB()
        {
            
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();

            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select nodekey, nodename from Vayu..node where marketkey = 9";
            mSelectNodeCommand.Connection = VayuDbConn;
            //
            mSelectNodeHourCommand = new SqlCommand();
            mSelectNodeHourCommand.CommandText = "select nodekey, lmp from nodelmp where marketdatetime >= @start and marketdatetime < @end";
            mSelectNodeHourCommand.Parameters.AddWithValue("@start", "MarketDateTime");
            mSelectNodeHourCommand.Parameters.AddWithValue("@end", "MarketDateTime");
            mSelectNodeHourCommand.Connection = VayuDbConn;
            //
            mDtFiveMin.Columns.Add("nodeKey", typeof(int));
            mDtFiveMin.Columns.Add("Lmp", typeof(double));
            mDtFiveMin.Columns.Add("MarketDate", typeof(DateTime));
            mDtFiveMin.Columns.Add("MarketHour", typeof(int));
            mDtFiveMin.Columns.Add("MarketMin", typeof(Int32));
            mDtFiveMin.Columns.Add("Second", typeof(Int32));
            //
            mSelectToday15MinCommand = new SqlCommand();
            mSelectToday15MinCommand.CommandText = "select distinct marketdatetime from nodelmp where MarketDateTime >= @start and " +
                                                    "MarketDateTime <= @end and finalyn = 'Y' order by MarketDateTime";
            mSelectToday15MinCommand.Parameters.AddWithValue("@start", "MarketDateTime");
            mSelectToday15MinCommand.Parameters.AddWithValue("@end", "MarketDateTime");
            mSelectToday15MinCommand.Connection = VayuDbConn;
            //
            mDtLmp.Columns.Add("NodeKey", typeof(int));
            mDtLmp.Columns.Add("LMP", typeof(double));
            mDtLmp.Columns.Add("Congestion", typeof(double));
            mDtLmp.Columns.Add("Loss", typeof(double));
            mDtLmp.Columns.Add("MarketDateTime", typeof(DateTime));
            mDtLmp.Columns.Add("FinalYN", typeof(char));
            //
            mDtLmph.Columns.Add("NodeKey", typeof(int));
            mDtLmph.Columns.Add("LMP", typeof(double));
            mDtLmph.Columns.Add("Congestion", typeof(double));
            mDtLmph.Columns.Add("Loss", typeof(double));
            mDtLmph.Columns.Add("MarketDateTime", typeof(DateTime));
            mDtLmph.Columns.Add("LMPCalculated", typeof(double));
            mDtLmph.Columns.Add("CongestionCalculated", typeof(double));
            mDtLmph.Columns.Add("LossCalculated", typeof(double));
            mDtLmph.Columns.Add("FinalYN", typeof(char));
            //
            mSelectMarketMinDateCommand = new SqlCommand();
            mSelectMarketMinDateCommand.CommandText = "select distinct marketdatetime from nodelmp (nolock) where (finalyn = 'N' " +
                "or finalyn is null) and marketdatetime >= @start and marketdatetime < @end";
            mSelectMarketMinDateCommand.Parameters.AddWithValue("@start", "marketdatetime");
            mSelectMarketMinDateCommand.Parameters.AddWithValue("@end", "marketdatetime");
            mSelectMarketMinDateCommand.Connection = VayuDbConn;
            //
            mSelectMaxDateMarketLmpCommand = new SqlCommand();
            mSelectMaxDateMarketLmpCommand.CommandText = "select max(marketdatetime) from nodelmp";
            mSelectMaxDateMarketLmpCommand.Connection = VayuDbConn;
            //
            mSelectMaxDateMarketMinCommand = new SqlCommand();
            mSelectMaxDateMarketMinCommand.CommandText = "select lmp from nodelmpmin (nolock) where marketdate = @marketdate " +
                                                            "and markethour = @markethour and marketmin = @marketmin and nodekey = 57194"; //20144
            mSelectMaxDateMarketMinCommand.Parameters.AddWithValue("@marketdate", "marketdate");
            mSelectMaxDateMarketMinCommand.Parameters.AddWithValue("@markethour", "markethour");
            mSelectMaxDateMarketMinCommand.Parameters.AddWithValue("@marketmin", "marketmin");
            mSelectMaxDateMarketMinCommand.Connection = VayuDbConn;
            //
            mSelectMarketMinCommand = new SqlCommand();
            mSelectMarketMinCommand.CommandText = "select nodekey, lmp from nodelmpmin (nolock) where marketdate = @marketdate and markethour = @markethour and marketmin = @marketmin";
            mSelectMarketMinCommand.Parameters.AddWithValue("@marketdate", "marketdate");
            mSelectMarketMinCommand.Parameters.AddWithValue("@markethour", "markethour");
            mSelectMarketMinCommand.Parameters.AddWithValue("@marketmin", "marketmin");
            mSelectMarketMinCommand.Connection = VayuDbConn;
            //
            mDeleteLmpTmpCommand = new SqlCommand();
            mDeleteLmpTmpCommand.CommandText = "truncate table nodelmptmp";
            mDeleteLmpTmpCommand.Connection = VayuDbConn;
            //
            mDeleteLmphTmpCommand = new SqlCommand();
            mDeleteLmphTmpCommand.CommandText = "truncate table nodelmphtemp";
            mDeleteLmphTmpCommand.Connection = VayuDbConn;
            //
            mDeleteLmpCommand = new SqlCommand();
            mDeleteLmpCommand.CommandText = "delete nodelmp where marketdatetime = @marketdatetime and finalyn = 'N'";
            mDeleteLmpCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mDeleteLmpCommand.Connection = VayuDbConn;
            //
            mSelectNodeTypeCommand = new SqlCommand();
            mSelectNodeTypeCommand.CommandText = "select nodetypekey from nodetype nolock where marketkey = 9 and label =  @label";
            mSelectNodeTypeCommand.Parameters.AddWithValue("@label", "label");
            mSelectNodeTypeCommand.Connection = VayuDbConn;
            //
            mInsertNodeCommand = new SqlCommand();
            mInsertNodeCommand.CommandText = "insert Vayu..node (NodeName,ExternalNodeID,MarketKey,NodeTypeKey,Zone,Longitude,Latitude) values (@nodename, 0, 9, @nodetypekey, '',NULL,NULL)";
            mInsertNodeCommand.Parameters.AddWithValue("@nodename", "nodename");
            mInsertNodeCommand.Parameters.AddWithValue("@nodetypekey", "nodetypekey");
            mInsertNodeCommand.Connection = VayuDbConn;
            //
            mSelectHourCommand = new SqlCommand();
            mSelectHourCommand.CommandText = "select count(*) from nodelmpmin nolock where marketdate = @marketdate and markethour = @markethour and nodekey = 57194";
            mSelectHourCommand.Parameters.AddWithValue("@marketdate", "marketdate");
            mSelectHourCommand.Parameters.AddWithValue("@markethour", "markethour");
            mSelectHourCommand.Connection = VayuDbConn;

            //
            mDeleteDownloadSspCommand = new SqlCommand();
            mDeleteDownloadSspCommand.CommandText = "truncate table NodeLMPMerge";
            mDeleteDownloadSspCommand.Connection = VayuDbConn;

            //
            mDeleteDownloadLMPCommand = new SqlCommand();
            mDeleteDownloadLMPCommand.CommandText = "truncate table NodeLmpMinMerge";
            mDeleteDownloadLMPCommand.Connection = VayuDbConn;
            //
            mDeleteCal15MinCommand = new SqlCommand();
            mDeleteCal15MinCommand.CommandText = "truncate table NodeLMPMerge";
            mDeleteCal15MinCommand.Connection = VayuDbConn;

            //
            mDeleteCalHourlyCommand = new SqlCommand();
            mDeleteCalHourlyCommand.CommandText = "truncate table NodeLmphtemp";
            mDeleteCalHourlyCommand.Connection = VayuDbConn;
        }

        private void FillNodeHash()
        {
            mNodeHash = new Dictionary<string, int>();
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = Convert.ToInt32(reader.GetValue(0));
                mNodeHash.Add(reader.GetString(1), nodekey);
            }
            reader.Close();
            VayuDbConn.Close();
        }

        public void StartTimer()
        {
            try
            {
                sTimer = new System.Timers.Timer();
                sTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
                sTimer.Interval = 5 * 60 * 1000; // 5 minutes
                OnTimerEvent(null, null);
                sTimer.Start();
                Thread.Sleep(10 * 1000);
                while (true) ;
                //{
                //    Thread.Sleep(1000); // 1 minute
                //}
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in StartTimer");
            }
        }
        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {

            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdServerCert");
            sTimer.Enabled = false;
          DateTime today = DateTime.Today.AddDays(0);
          //DateTime today = new DateTime(2026,05,09);


           DownloadLmp(today);  //  5 min download
           DownloadSsp(today);  //  15 min download
            //for (int i = 1; i < 24; i++)
            //{
            //    for (int j = 0; j <= 45;)
            //    {
            //        DateTime date = today.AddHours(i).AddMinutes(j);
            //        CalculateHourly(date);
            //        j = j + 15;
            //        Console.WriteLine(j);

            //    }
            //    Console.WriteLine("h" + i);
            //    CalculateHourly(today.AddHours(i));

            //}

            //today = new DateTime(2026, 04, 01);
            //today = new DateTime(2026, 04, 01, 01, 0, 0); // 2:00 PM
            CalculateHourly(today);
            sTimer.Enabled = true;
        }
        private void DownloadLmp(DateTime today)
        {
            SqlDataReader reader;
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            try
            {
                Log.Information("Starting DownloadLmp for {Today}", today);
                for (int hour = 0; hour < 24; hour++)
                {
                    if (VayuDbConn.State == ConnectionState.Open)
                    {
                        VayuDbConn.Close();
                    }
                    VayuDbConn.Open();
                    int num = 0;
                    mSelectHourCommand.Parameters["@marketdate"].Value = today;
                    mSelectHourCommand.Parameters["@markethour"].Value = hour;
                    reader = mSelectHourCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        num = reader.GetInt32(0);
                    }
                    reader.Close();
                    if (num > 11)
                    {
                        continue;
                    }

                    for (int min = 0; min < 60;)
                    {
                        mDtFiveMin.Clear();
                        DateTime compareDate = DateTime.Parse(today.ToShortDateString());
                        compareDate = compareDate.AddHours(hour);
                        compareDate = compareDate.AddMinutes(min);
                        if (compareDate > DateTime.Now)
                        {
                            VayuDbConn.Close();
                            break;
                        }
                        if (VayuDbConn.State == ConnectionState.Closed)
                        {
                            VayuDbConn.Open();
                        }
                        mSelectMaxDateMarketMinCommand.Parameters["@marketdate"].Value = DateTime.Parse(today.ToShortDateString());
                        mSelectMaxDateMarketMinCommand.Parameters["@markethour"].Value = hour;
                        mSelectMaxDateMarketMinCommand.Parameters["@marketmin"].Value = min;
                        reader = mSelectMaxDateMarketMinCommand.ExecuteReader();
                        bool found = false;
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                found = true;
                                break;
                            }
                        }
                        reader.Close();
                        if (found)
                        {
                            min += 5;
                            continue;
                        }
                        //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                       ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                        AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
                        System.ServiceModel.Channels.CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
                        ErcotNodalClient ercotclient = new ErcotNodalClient();
#if test

                        OperationsClient operationsClient = ercotclient.CreateErcotOperationsClientTest(myClientRequestBinding, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path);

#else

                         OperationsClient operationsClient = ercotclient.CreateErcotOperationsClient(myClientRequestBinding, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path);

#endif

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        RequestMessage myRequestMessage = new RequestMessage();
                        ResponseMessage respMessage = new ResponseMessage();
                        ReplayDetectionType myReplayDetection = new ReplayDetectionType();
                        RequestType myRequest = new RequestType();
                        HeaderType myHeader = new HeaderType();
                        //myHeader.Source = "QCOBA3";
                        //myHeader.UserID = "API_QCOBA3";
                        myHeader.Source = "QENJRE";  
                        // myHeader.Source = "QTALLR"; //old

                        // myHeader.UserID = "TALLER CUBE LLC(QSE)";

#if test
                        myHeader.UserID = "API_04122019PIYUSHD";

#else

                        myHeader.UserID = userCertificateDetails.UserName;

#endif
                        myHeader.Verb = HeaderTypeVerb.get;
                        myHeader.Noun = "LMPs";
                        myReplayDetection.Nonce = new EncodedString();
                        myReplayDetection.Nonce.Value = Convert.ToString(Guid.NewGuid());
                        myReplayDetection.Created = new AttributedDateTime();
                        myReplayDetection.Created.Value = Convert.ToString(string.Format("{0:s}", DateTime.Now));
                        myHeader.ReplayDetection = myReplayDetection;
                        myHeader.Revision = "1.0";
                        myRequestMessage.Header = myHeader;
                        myRequest.MarketType = RequestTypeMarketType.RTM;
                        myRequest.MarketTypeSpecified = true;
                        myRequest.StartTimeSpecified = true;
                        myRequest.EndTime = Convert.ToDateTime(DateTime.Now);
                        myRequest.EndTimeSpecified = true;
                        myRequestMessage.Request = myRequest;
                        string minStr = min < 10 ? "0" + min : min.ToString();
                        string hourStr = hour < 10 ? "0" + hour : hour.ToString();
                        bool isDST = curTimeZone.IsDaylightSavingTime(DateTime.Now);

                        DateTime endTime = isDST ? Convert.ToDateTime(today.Year + "-" + today.Month + "-" + today.Day + "T" + hourStr + ":" + minStr + ":00-05:00") :
                            Convert.ToDateTime(today.Year + "-" + today.Month + "-" + today.Day + "T" + hourStr + ":" + minStr + ":00-06:00");

                        myRequest.EndTime = endTime.AddMinutes(4).AddSeconds(0);//endTime.AddMinutes(5).AddSeconds(0);

                        myRequest.StartTime = endTime.AddMinutes(0).AddSeconds(0);//endTime.AddMinutes(0).AddSeconds(0);

                        respMessage = operationsClient.MarketInfo(myRequestMessage);
                        if (respMessage.Payload.Items != null)
                        {
                            if (respMessage.Payload.Items[0] != null)
                            {
                                XmlElement abc = (XmlElement)respMessage.Payload.Items[0];
                                foreach (XmlNode xmlNode in abc.GetElementsByTagName("ns1:LMP"))
                                {
                                    FillLmpDT(xmlNode);
                                }
                                if (VayuDbConn.State == ConnectionState.Closed)
                                {
                                    VayuDbConn.Open();
                                }
                                SqlTransaction transaction;
                                transaction = VayuDbConn.BeginTransaction();
                                mDeleteDownloadLMPCommand.Transaction = transaction;
                                mDeleteDownloadLMPCommand.ExecuteNonQuery();
                                using (SqlBulkCopy bk15Min = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                                {
                                    try
                                    {
                                        bk15Min.DestinationTableName = "[NodeLmpMinMerge]";
                                        bk15Min.ColumnMappings.Add("NodeKey", "NodeKey");
                                        bk15Min.ColumnMappings.Add("LMP", "LMP");
                                        bk15Min.ColumnMappings.Add("MarketDate", "MarketDate");
                                        bk15Min.ColumnMappings.Add("MarketHour", "MarketHour");
                                        bk15Min.ColumnMappings.Add("MarketMin", "MarketMin");
                                        bk15Min.ColumnMappings.Add("Second", "Second");
                                        bk15Min.WriteToServer(mDtFiveMin);
                                        
                                        SqlCommand updateNodeLMP = new SqlCommand("[UpMergeNodeLMPMinDown]", VayuDbConn, transaction);
                                        updateNodeLMP.CommandType = CommandType.StoredProcedure;
                                        updateNodeLMP.ExecuteNonQuery();
                                        transaction.Commit();
                                        Console.WriteLine("Insertig 5 min LMP Data ");
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        VayuDbConn.Close();
                                    }
                                }
                                VayuDbConn.Close();
                            }
                            min += 5;
                        }
                    }
                    Log.Information("Inserting 5 min LMP Data");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in DownloadLmp");
            }
            VayuDbConn.Close();
        }
        public void DownloadSsp(DateTime today)
        {
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            List<DateTime> marketDateTimeList = new List<DateTime>();
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            mSelectToday15MinCommand.Parameters["@start"].Value = DateTime.Parse(today.ToShortDateString());
            mSelectToday15MinCommand.Parameters["@end"].Value = DateTime.Parse(today.AddDays(1).ToShortDateString());
            SqlDataReader reader = mSelectToday15MinCommand.ExecuteReader();
            while (reader.Read())
            {
                marketDateTimeList.Add(reader.GetDateTime(0));
                Console.WriteLine(reader.GetDateTime(0));
            }
            reader.Close();
            VayuDbConn.Close();
            DateTime start = DateTime.Parse(today.ToShortDateString());
            //SP
            //start = start.AddHours(22).AddMinutes(0);

            while (start <= DateTime.Now)
            {
                if (marketDateTimeList.Contains(start))
                {
                    start = start.AddMinutes(15);
                    Console.WriteLine(start);
                    continue;
                }
                int minute = DateTime.Now.Minute;

                try
                {
                    Log.Information("Starting DownloadSSP for {Today}", today);
                    mDtLmp.Clear();
                    mNodeNameList = new List<string>();

                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                    AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
                    CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
                    ErcotNodalClient ercotclient = new ErcotNodalClient();
                    if (Directory.Exists(@"D:\ISOFiles\ErcotTrader\")) ;
                    else
                        Directory.CreateDirectory(@"D:\ISOFiles\ErcotTrader\");
                    OperationsClient operationsClient = ercotclient.CreateErcotOperationsClient(myClientRequestBinding, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path);
                    RequestMessage myRequestMessage = new RequestMessage();
                    ResponseMessage respMessage = new ResponseMessage();
                    ReplayDetectionType myReplayDetection = new ReplayDetectionType();
                    RequestType myRequest = new RequestType();
                    HeaderType myHeader = new HeaderType();

                    myHeader.Source = "QENJRE";
                    //myHeader.Source = "QTALLR";
                    myHeader.UserID = userCertificateDetails.UserName;
                    myHeader.Verb = HeaderTypeVerb.get;
                    myHeader.Noun = "SPPs";
                    myReplayDetection.Nonce = new EncodedString();
                    myReplayDetection.Nonce.Value = Convert.ToString(Guid.NewGuid());
                    myReplayDetection.Created = new AttributedDateTime();
                    myReplayDetection.Created.Value = Convert.ToString(string.Format("{0:s}", DateTime.Now));
                    myHeader.ReplayDetection = myReplayDetection;
                    myHeader.Revision = "1.0";
                    myRequestMessage.Header = myHeader;
                    myRequest.MarketType = RequestTypeMarketType.RTM;
                    myRequest.MarketTypeSpecified = true;
                    myRequest.StartTimeSpecified = true;
                    myRequest.EndTime = Convert.ToDateTime(DateTime.Now);
                    myRequest.EndTimeSpecified = true;
                    myRequestMessage.Request = myRequest;
                    int min = start.AddMinutes(14).Minute;
                    int hour = min == 0 ? start.Hour + 1 : start.Hour;
                    DateTime endDate = start;
                    if (hour == 24)
                    {
                        hour = 0;
                        endDate = endDate.AddDays(1);
                    }
                    string minStr = min < 10 ? "0" + min : min.ToString();
                    string hourStr = hour < 10 ? "0" + hour : hour.ToString();
                    bool isDST = curTimeZone.IsDaylightSavingTime(DateTime.Now);
                    DateTime endTime = isDST ? Convert.ToDateTime(today.Year + "-" + today.Month + "-" + today.Day + "T" + hourStr + ":" + minStr + ":00-05:00") :
                                        Convert.ToDateTime(today.Year + "-" + today.Month + "-" + today.Day + "T" + hourStr + ":" + minStr + ":00-06:00");
                    myRequest.EndTime = endTime.AddMinutes(0);//endTime.AddMinutes(0);
                    myRequest.StartTime = start.AddMinutes(0);//start.AddMinutes(0);

                    respMessage = operationsClient.MarketInfo(myRequestMessage);
                    if (respMessage.Payload.Items != null)
                    {
                        if (respMessage.Payload.Items[0] != null)
                        {
                            XmlElement abc = (XmlElement)respMessage.Payload.Items[0];
                            foreach (XmlNode xmlNode in abc.GetElementsByTagName("ns1:SPP"))
                            {
                                FillSppDt(xmlNode);
                            }
                            if (VayuDbConn.State == ConnectionState.Open)
                            {
                                VayuDbConn.Close();
                            }
                            SqlTransaction transaction;
                            if (VayuDbConn.State == ConnectionState.Closed)
                            {
                                VayuDbConn.Open();
                            }
                            mDeleteDownloadSspCommand.ExecuteNonQuery();
                            transaction = VayuDbConn.BeginTransaction();
                            using (SqlBulkCopy bkLmp = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                            {
                                try
                                {
                                    bkLmp.DestinationTableName = "[NodeLMPMerge]";
                                    bkLmp.ColumnMappings.Add("NodeKey", "NodeKey");
                                    bkLmp.ColumnMappings.Add("LMP", "LMP");
                                    bkLmp.ColumnMappings.Add("Congestion", "Congestion");
                                    bkLmp.ColumnMappings.Add("Loss", "Loss");
                                    bkLmp.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                                    bkLmp.ColumnMappings.Add("FinalYN", "FinalYN");
                                    bkLmp.WriteToServer(mDtLmp);
                                    SqlCommand updateNodeDALMPH = new SqlCommand("[UpMergeNodeLMP2]", VayuDbConn, transaction);
                                    updateNodeDALMPH.CommandType = CommandType.StoredProcedure;
                                    updateNodeDALMPH.ExecuteNonQuery();
                                    transaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    VayuDbConn.Close();
                                }
                            }
                            VayuDbConn.Close();
                        }
                        CalculateHourly(start);
                    }
                    Log.Information("Inserting 15 min LMP Data");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in DownloadSSP");
                }
                start = start.AddMinutes(15);
            }
        }
        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;

        }
        private void FillLmpDT(XmlNode xmlNode)
        {
            bool add = true;
            mDrFiveMin = mDtFiveMin.NewRow();
            string preString = "ns1";
            foreach (XmlNode lmpnode in xmlNode.ChildNodes)
            {
                if (lmpnode.Name == preString + ":time")
                {
                    DateTime dtMarketDate = Convert.ToDateTime(lmpnode.InnerText);
                    mDrFiveMin["MarketDate"] = dtMarketDate.Date;
                    mDrFiveMin["MarketHour"] = dtMarketDate.Hour;
                    mDrFiveMin["MarketMin"] = dtMarketDate.Minute;
                    mDrFiveMin["Second"] = dtMarketDate.Second;
                }
                if (lmpnode.Name == preString + ":value1")
                {
                    mDrFiveMin["Lmp"] = lmpnode.InnerText;
                }
                if (lmpnode.Name == preString + ":bus")
                {
                    if (mNodeHash.ContainsKey(lmpnode.InnerText))
                    {
                        mDrFiveMin["nodeKey"] = mNodeHash[lmpnode.InnerText];
                    }
                    else
                    {
                        add = false;
                    }
                }
            }
            if (add)
            {
                mDtFiveMin.Rows.Add(mDrFiveMin);
            }
        }
        private void FillSppDt(XmlNode xmlNode)
        {
            try
            {
                bool found = false;
                bool add = true;
                string nodeType = null;
                string nodeName = null;
                mDrLmp = mDtLmp.NewRow();
                string preString = "ns1";
                foreach (XmlNode lmpnode in xmlNode.ChildNodes)
                {
                    try
                    {
                        if (lmpnode.Name == preString + ":ending")
                        {
                            DateTime dtMarketDate = Convert.ToDateTime(lmpnode.InnerText);
                            mDrLmp["MarketDateTime"] = dtMarketDate;
                        }
                        if (lmpnode.Name == preString + ":value1")
                        {
                            mDrLmp["LMP"] = lmpnode.InnerText;
                        }
                        if (lmpnode.Name == preString + ":spType")
                        {
                            nodeType = lmpnode.InnerText;
                        }
                        if (lmpnode.Name == preString + ":sp")
                        {
                            nodeName = lmpnode.InnerText;
                            if (mNodeNameList.Contains(nodeName))
                            {
                                found = true;
                            }
                            else
                            {
                                mNodeNameList.Add(nodeName);
                                found = false;
                            }
                            if (mNodeHash.ContainsKey(nodeName))
                            {
                                mDrLmp["NodeKey"] = mNodeHash[nodeName];
                            }
                            else
                            {
                                add = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                mDrLmp["Congestion"] = 0;
                mDrLmp["Loss"] = 0;
                mDrLmp["FinalYN"] = "Y";
                if (!add)
                {
                    try
                    {
                        int nodeTypeKey = 0;
                        if (VayuDbConn.State == ConnectionState.Closed)
                        {
                            VayuDbConn.Open();
                        }
                        mSelectNodeTypeCommand.Parameters["@label"].Value = nodeType;
                        SqlDataReader reader = mSelectNodeTypeCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            nodeTypeKey = Convert.ToInt32(reader.GetValue(0));
                        }
                        reader.Close();
                        mInsertNodeCommand.Parameters["@nodename"].Value = nodeName;
                        mInsertNodeCommand.Parameters["@nodetypekey"].Value = nodeTypeKey;
                        mInsertNodeCommand.ExecuteNonQuery();
                        VayuDbConn.Close();
                        FillNodeHash();
                        mDrLmp["NodeKey"] = mNodeHash[nodeName];
                    }
                    catch (Exception ex)
                    {


                    }
                }
                if (!found)
                {
                    mDtLmp.Rows.Add(mDrLmp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void CalculateHourly(DateTime hourDate)
        {
            //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Date=" + hourDate.ToShortDateString());
            if (hourDate.Minute == 0)
            {
                hourDate = hourDate.AddHours(-1);
            }
            hourDate = DateTime.Parse(hourDate.Year + "/" + hourDate.Month + "/" + hourDate.Day + " " + hourDate.Hour + ":00");
            Dictionary<int, List<double>> nodeHash = new Dictionary<int, List<double>>();
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            mSelectNodeHourCommand.Parameters["@start"].Value = hourDate.AddMinutes(15);
            mSelectNodeHourCommand.Parameters["@end"].Value = hourDate.AddHours(1).AddMinutes(15);
            SqlDataReader reader = mSelectNodeHourCommand.ExecuteReader();
            while (reader.Read())
            {
                List<double> lmpList = new List<double>();
                int nodeKey = Convert.ToInt32(reader.GetValue(0));
                double lmp = Convert.ToDouble(reader.GetValue(1));
                if (nodeHash.ContainsKey(nodeKey))
                {
                    lmpList = nodeHash[nodeKey];
                    nodeHash.Remove(nodeKey);
                }
                lmpList.Add(lmp);
                nodeHash.Add(nodeKey, lmpList);
            }
            reader.Close();
            VayuDbConn.Close();
            mDtLmph.Clear();
            List<int> keyList = nodeHash.Keys.ToList<int>();
            foreach (int key in keyList)
            {
                if (key == 57216)
                {

                }
                List<double> lmpList = nodeHash[key];
                double lmpH = 0;
                foreach (double lmp in lmpList)
                {
                    lmpH += lmp;
                }
                lmpH = lmpH / (double)lmpList.Count;
                DataRow drLmph = mDtLmph.NewRow();
                drLmph["NodeKey"] = key;
                drLmph["LMP"] = lmpH;
                drLmph["Congestion"] = 0;
                drLmph["Loss"] = 0;
                drLmph["MarketDateTime"] = hourDate.AddHours(1);
                drLmph["LMPCalculated"] = lmpH;
                drLmph["CongestionCalculated"] = 0;
                drLmph["LossCalculated"] = 0;
                drLmph["FinalYN"] = "Y";
                mDtLmph.Rows.Add(drLmph);
            }
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            SqlTransaction transaction;
            transaction = VayuDbConn.BeginTransaction();
            mDeleteCalHourlyCommand.Transaction = transaction;
            mDeleteCalHourlyCommand.ExecuteNonQuery();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {

                    bkLmpH.DestinationTableName = "[NodeLmphtemp]";
                    mDeleteLmphTmpCommand.Transaction = transaction;
                    mDeleteLmphTmpCommand.ExecuteNonQuery();
                    bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                    bkLmpH.ColumnMappings.Add("LMP", "LMP");
                    bkLmpH.ColumnMappings.Add("Congestion", "Congestion");
                    bkLmpH.ColumnMappings.Add("Loss", "Loss");
                    bkLmpH.ColumnMappings.Add("LMPCalculated", "LMPCalculated");
                    bkLmpH.ColumnMappings.Add("CongestionCalculated", "CongestionCalculated");
                    bkLmpH.ColumnMappings.Add("LossCalculated", "LossCalculated");
                    bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                    bkLmpH.ColumnMappings.Add("FinalYN", "FinalYN");
                    bkLmpH.WriteToServer(mDtLmph);
                    SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeNodeLMPH]", VayuDbConn, transaction);
                    updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                    updateNodeLmpMin.CommandTimeout = 300000;
                    updateNodeLmpMin.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    //   mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                    transaction.Rollback();
                    VayuDbConn.Close();
                }
            }
            VayuDbConn.Close();
        }
    }
}
