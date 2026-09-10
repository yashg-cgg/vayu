using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Net.Security;
using Vayu.MarketLibraryErcot;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Xml;
using System.Timers;
using Vayu.MarketLibraryErcot.ERCOTNodalService;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotEnergyPriceDownload
{
    class Program
    {


        static void Main(string[] args)
        {
            EneryDOwnloader download = new EneryDOwnloader();
            download.StartTimer();
        }

    }
    public class EneryDOwnloader
    {
        private SqlConnection VayuDbConn;
        private SqlCommand mDeleteNodeHErcotCommand;
        private SqlCommand cmdSelectNodeLmpCommand;
        private SqlCommand cmdTruncTempTable;
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;

        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        System.Timers.Timer mTimer = new System.Timers.Timer();
        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
        }
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        public void StartTimer()
        {

            mTimer = new System.Timers.Timer();
            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 60000;
            mTimer.Start();
            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') { }
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdServerCert");
            mTimer.Enabled = false;
            DateTime date = DateTime.Today.Date;
            while (date > DateTime.Today.AddDays(-4))
            {
                DownloadEnergy(date);
                date = date.AddDays(-1);
            }
            CalculateHourly();
            mTimer.Enabled = true;
        }

        private void CalculateHourly()
        {
            InitDB();
            DateTime today = DateTime.Now;
            List<DateTime> hourList = new List<DateTime>();
            DataTable dtLmph = new DataTable();
            dtLmph.Columns.Add("NodeKey");
            dtLmph.Columns.Add("Congestion");
            dtLmph.Columns.Add("MarketDateTime");
            dtLmph.Columns.Add("LMP");
            dtLmph.Columns.Add("Loss");
            dtLmph.Columns.Add("LMPCalculated");
            dtLmph.Columns.Add("CongestionCalculated");
            dtLmph.Columns.Add("LossCalculated");

            if (VayuDbConn.State == System.Data.ConnectionState.Open)
            {
                VayuDbConn.Close();
            }
            VayuDbConn.Open();
            DateTime start = DateTime.MinValue;
            string day = today.ToString("dddd").ToLower();
            if ((today.ToString("dddd").ToLower() == "saturday") || (today.ToString("dddd").ToLower() == "sunday"))
                start = today.AddDays(-2);
            else
                start = today.AddDays(-1);
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            //  DateTime endDate = curTimeZone.IsDaylightSavingTime(start) ? start : start.AddHours(1);
            DateTime endDate = DateTime.Now;
            while (start <= endDate)
            {

                DateTime end = start.AddHours(1);
                cmdSelectNodeLmpCommand = new SqlCommand();
                cmdSelectNodeLmpCommand.CommandText = "select NodeKey , AVG(congestion) from NodeLMPMin " +
                                                            "where MarketDate = '" + start.Date.ToString("yyyy-MM-dd") + "' and marketHour = " + start.Hour.ToString() + " and congestion is not null group by NodeKey ";

                cmdSelectNodeLmpCommand.Connection = VayuDbConn;
                cmdSelectNodeLmpCommand.CommandTimeout = 30000;
                if (VayuDbConn.State == ConnectionState.Open)
                {
                    VayuDbConn.Close();
                }
                VayuDbConn.Open();
                var reader = cmdSelectNodeLmpCommand.ExecuteReader();
                while (reader.Read())
                {
                    DataRow drLmph = dtLmph.NewRow();
                    drLmph["NodeKey"] = Convert.ToInt32(reader.GetValue(0));
                    drLmph["Congestion"] = Math.Round(Convert.ToDouble(reader.GetValue(1)), 2);
                    drLmph["MarketDateTime"] = start;
                    drLmph["LMP"] = 0;
                    dtLmph.Rows.Add(drLmph);
                }
                reader.Close();


                if (VayuDbConn.State == ConnectionState.Open)
                {
                    VayuDbConn.Close();
                }

                VayuDbConn.Open();
                cmdTruncTempTable = new SqlCommand();
                cmdTruncTempTable.CommandText = "truncate table [NodelmphCong_Test]";
                cmdTruncTempTable.Connection = VayuDbConn;
                cmdTruncTempTable.ExecuteNonQuery();
                SqlTransaction transaction = VayuDbConn.BeginTransaction();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        if (dtLmph.Rows.Count > 0)
                        {
                            bkLmpH.DestinationTableName = "[NodelmphCong_Test]";
                            bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                            bkLmpH.ColumnMappings.Add("LMP", "LMP");
                            bkLmpH.ColumnMappings.Add("Congestion", "Congestion");
                            bkLmpH.ColumnMappings.Add("Loss", "Loss");
                            bkLmpH.ColumnMappings.Add("LMPCalculated", "LMPCalculated");
                            bkLmpH.ColumnMappings.Add("CongestionCalculated", "CongestionCalculated");
                            bkLmpH.ColumnMappings.Add("LossCalculated", "LossCalculated");
                            bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                            //   bkLmpH.ColumnMappings.Add("FinalYN", "FinalYN");
                            bkLmpH.WriteToServer(dtLmph);
                            SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeHourlyCong]", VayuDbConn, transaction);
                            updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                            updateNodeLmpMin.CommandTimeout = 30000;
                            updateNodeLmpMin.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                    finally
                    {
                        //transaction.Commit();
                        VayuDbConn.Close();
                    }
                    //  dt.Clear();
                }
                start = start.AddHours(1);
            }
        }
        public void DownloadEnergy(DateTime today)
        {
            InitDB();
            TimeZone curTimeZone = TimeZone.CurrentTimeZone;
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            List<DateTime> marketDateTimeList = new List<DateTime>();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = VayuDbConn;
            cmd.CommandText = "select distinct MarketDate , MarketHour , MarketMin    from NodeLMPMin where   MarketDate = '" + today.Date.ToString() + "' and congestion is null   order by MarketDate desc, MarketHour desc, MarketMin desc";

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DateTime mktDate = Convert.ToDateTime(reader.GetValue(0));
                int hour = Convert.ToInt32(reader.GetValue(1));
                int min = Convert.ToInt32(reader.GetValue(2));
                mktDate = mktDate.AddHours(hour).AddMinutes(min);
                marketDateTimeList.Add(mktDate);
            }
            reader.Close();
            VayuDbConn.Close();
            try
            {
                for (int hour = 0; hour < 24; hour++)
                { 
                    if (VayuDbConn.State == ConnectionState.Open)
                    {
                        VayuDbConn.Close();
                    }
                    VayuDbConn.Open();
                    int num = 0;

                    if (num > 11)
                    {
                        continue;
                    }

                    for (int min = 0; min < 60; )
                    {

                        DateTime compareDate = today;
                        compareDate = compareDate.AddHours(hour);
                        compareDate = compareDate.AddMinutes(min);
                        if (!marketDateTimeList.Contains(compareDate))
                        {
                            min += 5;
                            continue;
                        }
                        if (compareDate > DateTime.Now)
                        {
                            VayuDbConn.Close();
                            break;
                        }
                        if (VayuDbConn.State == ConnectionState.Closed)
                        {
                            VayuDbConn.Open();
                        }

                        ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                        AsymmetricCustomBinding acb = new AsymmetricCustomBinding();
                        CustomBinding myClientRequestBinding = acb.CreateAsymmetricBinding();
                        ErcotNodalClient ercotclient = new ErcotNodalClient();
                        OperationsClient operationsClient = ercotclient.CreateErcotOperationsClient(myClientRequestBinding, userCertificateDetails.Path, userCertificateDetails.Password, serverCertificateDetails.Path);
                        RequestMessage myRequestMessage = new RequestMessage();
                        ResponseMessage respMessage = new ResponseMessage();
                        ReplayDetectionType myReplayDetection = new ReplayDetectionType();
                        RequestType myRequest = new RequestType();
                        HeaderType myHeader = new HeaderType(); 
                        myHeader.Source = "QENJRE"; 
                        myHeader.UserID = userCertificateDetails.UserName;
                        myHeader.Verb = HeaderTypeVerb.get;
                        myHeader.Noun = "SCEDORDCPriceAdders";
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


                        DateTime endTime = isDST ? Convert.ToDateTime(compareDate.Year + "-" + compareDate.Month + "-" + compareDate.Day + "T" + hourStr + ":" + minStr + ":00-05:00") :
                    Convert.ToDateTime(compareDate.Year + "-" + compareDate.Month + "-" + compareDate.Day + "T" + hourStr + ":" + minStr + ":00-06:00");
                         
                        myRequest.EndTime = endTime.AddMinutes(5); 
                        myRequest.StartTime = endTime; 
                        respMessage = operationsClient.MarketInfo(myRequestMessage);
                        double energy = double.MinValue;
                        DateTime respDate = DateTime.MinValue;
                        if (respMessage.Payload.Items != null && respMessage.Payload.Items[0] != null)
                        {
                            DataTable congestionDt = new DataTable();
                            congestionDt.Columns.Add("NodeKey");
                            congestionDt.Columns.Add("LMP");
                            congestionDt.Columns.Add("MarketDate");
                            congestionDt.Columns.Add("MarketHour");
                            congestionDt.Columns.Add("MarketMin");
                            congestionDt.Columns.Add("Congestion");
                            congestionDt.Columns.Add("Second");
                            congestionDt.Columns.Add("Energy");
                            XmlElement abc = (XmlElement)respMessage.Payload.Items[0];
                            foreach (XmlNode xmlNode in abc.GetElementsByTagName("ns1:SCEDTimestamp"))
                            { 
                                respDate = Convert.ToDateTime(xmlNode.InnerText);
                            }
                            foreach (XmlNode xmlNode in abc.GetElementsByTagName("ns1:SystemLambda"))
                            { 
                                energy = Convert.ToDouble(xmlNode.InnerText);
                            }

                            if (energy != double.MinValue && respDate != DateTime.MinValue)
                            {
                                SqlCommand cmdGetUnknownCongestionNodes = VayuDbConn.CreateCommand();
                                cmdGetUnknownCongestionNodes.CommandText = " select * from NodeLMPMin where MarketDate = '" + respDate.Date + "' and MarketHour = " + respDate.Hour
                                                                            + " and MarketMin = " + respDate.Minute + " and Second = " + respDate.Second;

                                SqlDataReader rdr = cmdGetUnknownCongestionNodes.ExecuteReader();
                                while (rdr.Read())
                                {
                                    if (Convert.ToInt32(rdr.GetValue(0)) == 57667)
                                    {

                                    }
                                    DataRow congDr = congestionDt.NewRow();
                                    congDr["NodeKey"] = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                                    double lmp = rdr.IsDBNull(1) ? 0 : Convert.ToDouble(rdr.GetValue(1));
                                    congDr["LMP"] = lmp;
                                    congDr["MarketDate"] = rdr.IsDBNull(2) ? DateTime.Today : Convert.ToDateTime(rdr.GetValue(2));
                                    congDr["MarketHour"] = rdr.IsDBNull(3) ? 0 : Convert.ToInt32(rdr.GetValue(3));
                                    congDr["MarketMin"] = rdr.IsDBNull(4) ? 0 : Convert.ToInt32(rdr.GetValue(4));
                                    congDr["Congestion"] = Math.Round(lmp - energy, 2);
                                    congDr["Second"] = rdr.IsDBNull(6) ? 0 : Convert.ToInt32(rdr.GetValue(6)); 
                                    congestionDt.Rows.Add(congDr); 
                                }
                                rdr.Close();

                                if (VayuDbConn.State == ConnectionState.Closed)
                                {
                                    VayuDbConn.Open();
                                }
                                mDeleteNodeHErcotCommand = VayuDbConn.CreateCommand();
                                mDeleteNodeHErcotCommand.CommandText = "Truncate table NodeLMPCong_temp";
                                mDeleteNodeHErcotCommand.ExecuteNonQuery();
                                SqlTransaction transaction;
                                transaction = VayuDbConn.BeginTransaction();


                                using (SqlBulkCopy bk15Min = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                                {
                                    try
                                    {
                                        bk15Min.DestinationTableName = "[NodeLMPCong_temp]";
                                        bk15Min.ColumnMappings.Add("NodeKey", "NodeKey");
                                        bk15Min.ColumnMappings.Add("LMP", "LMP");
                                        bk15Min.ColumnMappings.Add("MarketDate", "MarketDate");
                                        bk15Min.ColumnMappings.Add("MarketHour", "MarketHour");
                                        bk15Min.ColumnMappings.Add("MarketMin", "MarketMin");
                                        bk15Min.ColumnMappings.Add("Congestion", "Congestion");
                                        bk15Min.ColumnMappings.Add("Second", "Second");
                                        bk15Min.WriteToServer(congestionDt);
                                        SqlCommand updateNodeLMP = new SqlCommand("[UpMergeNodeLMPMinDownCong]", VayuDbConn, transaction);
                                        updateNodeLMP.CommandType = CommandType.StoredProcedure;
                                        updateNodeLMP.ExecuteNonQuery();
                                        transaction.Commit();
                                    }
                                    catch (Exception ex)
                                    { 
                                        transaction.Rollback();
                                        VayuDbConn.Close();
                                    }
                                }
                            }

                        }
                        min += 5;
                    }
                }
            }
            catch (Exception ex)
            { 
            }
            VayuDbConn.Close(); 
        }
    }
}
