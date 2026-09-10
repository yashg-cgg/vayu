using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Data.SqlTypes;
using System.Timers;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Vayu.CommonAccessLibrary;

namespace Vayu.ERCOTOutagesDownload
{
    class OutagesUpdate
    {
        private SqlConnection mConnection90;
        private SqlConnection mConnection901;
        private SqlCommand mInsertErcotOutagesCommand;
        private SqlCommand mUpdateErcotOutagesCommand;
        private SqlCommand mSelectErcotOutagesCommand;
        private SqlCommand mSelectRemoveErcotOutagesCommand;
        private SqlCommand mDeleteIDCommand;
        private SqlCommand mUpdateActualEndDateCommand;
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private List<Outages> mOutageList = new List<Outages>();
        private X509Certificate2 mCert = new X509Certificate2();
        DataTable mErcotOutagesdt = new DataTable();
        DataRow mErcotOutagesdr;
        DataTable mErcotOutagesIDdt = new DataTable();
        DataRow mErcotOutagesIDdr;
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        public void InitCert()
        {
            Console.WriteLine("Checking Authentication....");
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }

        //public void InitCert()
        //{

        //    SqlCommand selectUserPasswordCommand = new SqlCommand();
        //    selectUserPasswordCommand.CommandText = "select CertificateName,Password from Vayu..Certificate where MarketKey=9 and Application_Name='ProdClientAPI'";
        //    selectUserPasswordCommand.Parameters.AddWithValue("@Password", "Password");
        //    selectUserPasswordCommand.Connection = mConnection90;
        //    string certFileName = "D:\\Q1Certificate\\Digital\\";
        //    string certPasswd = "";
        //    mConnection90.Open();

        //    SqlDataReader reader = selectUserPasswordCommand.ExecuteReader();
        //    while (reader.Read())
        //    {
        //        certFileName = certFileName + reader.GetString(0) + ".pfx";
        //        certPasswd = reader.GetString(1);
        //    }
        //    reader.Close();
        //    mConnection90.Close();
        //    if (certPasswd == "")
        //    {
        //        return;
        //    }

        //    mCert.Import(certFileName, certPasswd, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        //}


        public OutagesUpdate()
        {
            InitDB();
            InitCert();
            OnTimedEvent(null, null);
            StartTimer();
        }
        public void StartTimer()
        {
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 60000;
            mTimer.Enabled = true;
            Console.WriteLine("Press \'q\' to quit.");
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            UpdateOutages();
            mTimer.Enabled = true;
        }


        public void InitDB()
        {
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            mConnection90 = new VayuDBConnection().GetInstance().GetSqlConnection();           
            mSelectRemoveErcotOutagesCommand = new SqlCommand();
            mSelectRemoveErcotOutagesCommand.CommandText = "select OutageIdentifier, PlannedStartDate, PlannedEndDate, ActualStartDate, ActualEndDate, OutageStatus, RequestorOrgName, EquipmentType, EquipmentName, EquipmentFromStationName, EquipmentToStationName, VoltageLevel,  SubmitTime, OutageType from ERCOT_OUTAGES where RemovedDate is null or RemovedDate=''";
            mSelectRemoveErcotOutagesCommand.Connection = mConnection90;
            //
            mSelectErcotOutagesCommand = new SqlCommand();
            mSelectErcotOutagesCommand.CommandText = "select top 1 * from Vayu..Ercot_RT_Outages where OutageIdentifier=@OutageIdentifier and EquipmentName=@EquipmentName and PlannedStartDate=@PlannedStartDate and EquipmentType=@EquipmentType order by revisedDate desc";
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@OutageIdentifier", "OutageIdentifier");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@EquipmentName", "EquipmentName");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@PlannedStartDate", "PlannedStartDate");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@EquipmentType", "EquipmentType");
            mSelectErcotOutagesCommand.Connection = mConnection90;
            //

            mUpdateErcotOutagesCommand = new SqlCommand();
            mUpdateErcotOutagesCommand.CommandText = "update Vayu..Ercot_RT_Outages set OutageIdentifier=@OutageIdentifier,PlannedStartDate=@PlannedStartDate,PlannedEndtDate=@PlannedEndDate,OutageStatus=@OutageStatus,RequestorOrgName=@RequestorOrgName,EquipmentType=@EquipmentType,EquipmentName=@EquipmentName,EquipmentFromStationName=@EquipmentFromStationName,VoltageLevel=@VoltageLevel,SubmitTime=@SubmitTime,OutageType=@OutageType " + //EquipmentToStationName=@EquipmentToStationName,
                ", BreakerSwitchNormalStatus=@BreakerSwitchNormalStatus , BreakerSwitchOutageStatus=@BreakerSwitchOutageStatus, RequestorLongName=@RequestorLongName ,NatureOfWork=@NatureOfWork ,TEID=@TEID ,RestorationTime=@RestorationTime ,ReasonForCancellation=@ReasonForCancellation,CancellationDate=@CancellationDate,ActualStartDate=@ActualStartDate,ActualEndDate=@ActualEndDate,GroupLabel=@GroupLabel,EquipmentToStationName=@EquipmentToStationName" +
                " where OutageIdentifier=@OutageIdentifier and EquipmentName=@EquipmentName ";// 
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@OutageIdentifier", "OutageIdentifier");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@PlannedStartDate", "PlannedStartDate");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@PlannedEndDate", "PlannedEndDate");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@OutageStatus", "OutageStatus");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@RequestorOrgName", "RequestorOrgName");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@EquipmentType", "EquipmentType");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@EquipmentName", "EquipmentName");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@EquipmentFromStationName", "EquipmentFromStationName");
            // mUpdateErcotOutagesCommand.Parameters.AddWithValue("@EquipmentToStationName", "EquipmentToStationName");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@VoltageLevel", "VoltageLevel");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@SubmitTime", "SubmitTime");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@OutageType", "OutageType");
            //
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@BreakerSwitchNormalStatus", "BreakerSwitchNormalStatus");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@BreakerSwitchOutageStatus", "BreakerSwitchOutageStatus");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@RequestorLongName", "RequestorLongName");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@NatureOfWork", "NatureOfWork");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@TEID", "TEID");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@RestorationTime", "RestorationTime");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@ReasonForCancellation", "ReasonForCancellation");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@CancellationDate", "CancellationDate");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@ActualStartDate", "ActualStartDate");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@ActualEndDate", "ActualEndDate");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@GroupLabel", "GroupLabel");
            mUpdateErcotOutagesCommand.Parameters.AddWithValue("@EquipmentToStationName", "EquipmentToStationName");

            mUpdateErcotOutagesCommand.Connection = mConnection90;
            //
            mInsertErcotOutagesCommand = new SqlCommand();
            mInsertErcotOutagesCommand.CommandText = "insert into Vayu..Ercot_RT_Outages (OutageIdentifier,PlannedStartDate,PlannedEndtDate,OutageStatus,RequestorOrgName,EquipmentType,EquipmentName,EquipmentFromStationName,VoltageLevel,SubmitTime,OutageType, " + //UpdateDateTime ,,EquipmentToStationName,
                "BreakerSwitchNormalStatus,BreakerSwitchOutageStatus,RequestorLongName,NatureOfWork,TEID,RestorationTime,ReasonForCancellation,CancellationDate,ActualStartDate,ActualEndDate,GroupLabel,EquipmentToStationName)" +
                                                                        "values(@OutageIdentifier,@PlannedStartDate,@PlannedEndDate,@OutageStatus,@RequestorOrgName,@EquipmentType,@EquipmentName,@EquipmentFromStationName,@VoltageLevel,@SubmitTime,@OutageType," + //,@UpdateDateTime.,@ActualStartDate,@ActualEndDate,@EquipmentToStationName,
                "@BreakerSwitchNormalStatus,@BreakerSwitchOutageStatus,@RequestorLongName,@NatureOfWork,@TEID,@RestorationTime,@ReasonForCancellation,@CancellationDate,@ActualStartDate,@ActualEndDate,@GroupLabel,@EquipmentToStationName)";
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@OutageIdentifier", "OutageIdentifier");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@PlannedStartDate", "PlannedStartDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@PlannedEndDate", "PlannedEndDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@OutageStatus", "OutageStatus");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@RequestorOrgName", "RequestorOrgName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentType", "EquipmentType");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentName", "EquipmentName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentFromStationName", "EquipmentFromStationName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@VoltageLevel", "VoltageLevel");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@SubmitTime", "SubmitTime");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@OutageType", "OutageType");
            //  mInsertErcotOutagesCommand.Parameters.AddWithValue("@UpdateDateTime", "UpdateDateTime");
            //
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@BreakerSwitchNormalStatus", "BreakerSwitchNormalStatus");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@BreakerSwitchOutageStatus", "BreakerSwitchOutageStatus");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@RequestorLongName", "RequestorLongName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@NatureOfWork", "NatureOfWork");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@TEID", "TEID");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@RestorationTime", "RestorationTime");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@ReasonForCancellation", "ReasonForCancellation");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@CancellationDate", "CancellationDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@ActualStartDate", "ActualStartDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@ActualEndDate", "ActualEndDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@GroupLabel", "GroupLabel");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentToStationName", "EquipmentToStationName");
            mInsertErcotOutagesCommand.Connection = mConnection90;
            //  
            mErcotOutagesdt = new DataTable();
            mErcotOutagesdt.Columns.Add("OutageIdentifier", typeof(string));
            mErcotOutagesdt.Columns.Add("PlannedStartDate", typeof(DateTime));
            mErcotOutagesdt.Columns.Add("PlannedEndtDate", typeof(DateTime));
            mErcotOutagesdt.Columns.Add("BreakerSwitchNormalStatus", typeof(string));
            mErcotOutagesdt.Columns.Add("BreakerSwitchOutageStatus", typeof(string));
            mErcotOutagesdt.Columns.Add("OutageStatus", typeof(string));
            mErcotOutagesdt.Columns.Add("RequestorOrgName", typeof(string));
            mErcotOutagesdt.Columns.Add("RequestorLongName", typeof(string));
            mErcotOutagesdt.Columns.Add("EquipmentType", typeof(string));
            mErcotOutagesdt.Columns.Add("EquipmentName", typeof(string));
            mErcotOutagesdt.Columns.Add("EquipmentFromStationName", typeof(string));
            mErcotOutagesdt.Columns.Add("NatureOfWork", typeof(string));
            mErcotOutagesdt.Columns.Add("VoltageLevel", typeof(decimal));
            mErcotOutagesdt.Columns.Add("SubmitTime", typeof(DateTime));
            mErcotOutagesdt.Columns.Add("OutageType", typeof(string));
            mErcotOutagesdt.Columns.Add("TEID", typeof(int));
            mErcotOutagesdt.Columns.Add("RestorationTime", typeof(int));
            mErcotOutagesdt.Columns.Add("ReasonForCancellation", typeof(string));
            mErcotOutagesdt.Columns.Add("CancellationDate", typeof(DateTime));
            mErcotOutagesdt.Columns.Add("ActualStartDate", typeof(DateTime));
            mErcotOutagesdt.Columns.Add("ActualEndDate", typeof(DateTime));
            mErcotOutagesdt.Columns.Add("GroupLabel", typeof(string));
            mErcotOutagesdt.Columns.Add("EquipmentToStationName", typeof(string));

            mErcotOutagesIDdt = new DataTable();
            mErcotOutagesIDdt.Columns.Add("OutageIdentifier", typeof(string));

            mDeleteIDCommand = new SqlCommand();
            mDeleteIDCommand.CommandText = "truncate table OutageID";
            mDeleteIDCommand.Connection = mConnection90;

            mUpdateActualEndDateCommand = new SqlCommand();
            mUpdateActualEndDateCommand.Connection = mConnection90;

        }
        public void UpdateOutages()
        {
            //string certPath = "C:\\Certificates\\QSE COBALT_2014.pfx";
            //mCert.Import(certPath, "westoaks", X509KeyStorageFlags.MachineKeySet);

            string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ERCOTOutagesDownload");
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=13446&mimic_duns=1189331712000");
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.ConnectionLimit = 1;
            request.Method = "GET";
            request.ClientCertificates.Add(mCert);
            request.CookieContainer = new CookieContainer();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 |
                                                   SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
            request.Timeout = 100000;
            //request.PreAuthenticate = true;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = default(StreamReader);
            sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string strOutages = sr.ReadToEnd();
            sr.Close();
            strOutages = strOutages.Remove(0, strOutages.IndexOf(".AATONP3754_csv") + 20);
            string Outages = "https://mis.ercot.com/misdownload" + strOutages.Substring(strOutages.IndexOf("/servlets/mirDownload?mimic_duns="), 69);
            HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(Outages);
            FileDownloadRequest.Method = "GET";
            FileDownloadRequest.ClientCertificates.Add(mCert);
            FileDownloadRequest.Timeout = 500000;
            HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
            string FileDestName = @"D:\\ISOFiles\\ERCOTOutagesDownload\\" + Outages.Substring(Outages.IndexOf("lookupId=") + 9, 9) + ".zip";
            string FolderFilename = @"D:\\ISOFiles\\ERCOTOutagesDownload\\" + Outages.Substring(Outages.IndexOf("lookupId=") + 9, 9) + ".csv";
            FileDownloadresponse.Headers.Add("Content-disposition", FileDestName);
            using (BinaryReader reader = new BinaryReader(FileDownloadresponse.GetResponseStream()))
            {
                using (FileStream fileStream = File.Open(FileDestName, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        byte[] buffer = new byte[2048];
                        int count = reader.Read(buffer, 0, buffer.Length);
                        while (count != 0)
                        {
                            writer.Write(buffer, 0, count);
                            writer.Flush();
                            count = reader.Read(buffer, 0, buffer.Length);
                        }
                        writer.Close();
                        reader.Close();
                    }
                }

                XML_UnZipFile(FileDestName, FolderFilename);
                if (FolderFilename.Contains(FolderFilename))
                {
                    using (StreamReader streamreader = new StreamReader(FolderFilename))
                    {

                        List<Outages> outagesList = new List<Outages>();
                        string data = streamreader.ReadToEnd();

                        XmlDataDocument xmlParser = new XmlDataDocument();
                        xmlParser.LoadXml(data);

                        mErcotOutagesIDdt.Clear();
                        mErcotOutagesIDdr = mErcotOutagesIDdt.NewRow();
                        foreach (XmlNode node in xmlParser.ChildNodes)
                        {
                            foreach (XmlNode childnode in node)
                            {
                                mErcotOutagesIDdr = mErcotOutagesIDdt.NewRow();
                                mErcotOutagesIDdr["OutageIdentifier"] = childnode["OutageIdentifier"].InnerText.ToString();
                                try
                                {
                                    mErcotOutagesIDdt.Rows.Add(mErcotOutagesIDdr);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        try
                        {
                            InsertIdentifierDB();
                            // InsertDB(mErcotOutagesdt);
                            // File.Delete(FolderFilename);
                            // File.Delete(FileDestName);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                }
            }
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }
        }

        public static string XML_UnZipFile(string InputPathOfZipFile, string FolderFilename)
        {
            string strNewFile = "";
            try
            {
                if (File.Exists(InputPathOfZipFile))
                {
                    string baseDirectory = Path.GetDirectoryName(InputPathOfZipFile);
                    using (ZipInputStream ZipStream = new ZipInputStream(File.OpenRead(InputPathOfZipFile)))
                    {
                        ZipEntry theEntry;
                        while ((theEntry = ZipStream.GetNextEntry()) != null)
                        {
                            if (theEntry.IsFile)
                            {
                                if (theEntry.Name != "")
                                {
                                    strNewFile = FolderFilename;
                                    if (File.Exists(strNewFile))
                                    {
                                        continue;
                                    }

                                    using (FileStream streamWriter = File.Create(strNewFile))
                                    {
                                        int size = 2048;
                                        byte[] data = new byte[2048];
                                        while (true)
                                        {
                                            size = ZipStream.Read(data, 0, data.Length);
                                            if (size > 0)
                                                streamWriter.Write(data, 0, size);
                                            else
                                                break;
                                        }
                                        streamWriter.Close();
                                    }
                                }
                            }
                            else if (theEntry.IsDirectory)
                            {
                                string strNewDirectory = @"" + baseDirectory + @"\" + theEntry.Name;
                                if (!Directory.Exists(strNewDirectory))
                                {
                                    Directory.CreateDirectory(strNewDirectory);
                                }
                            }
                        }
                        ZipStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            return strNewFile;
        }

        public void InsertIdentifierDB()
        {
            if (mConnection90.State == ConnectionState.Open)
            {
                mConnection90.Close();
            }
            mConnection90.Open();
            mDeleteIDCommand.ExecuteNonQuery();
            SqlTransaction transaction = mConnection90.BeginTransaction();
            using (SqlBulkCopy bkoutage = new SqlBulkCopy(mConnection90, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkoutage.DestinationTableName = "OutageID_test";
                    bkoutage.ColumnMappings.Add("OutageIdentifier", "OutageIdentifier");
                    bkoutage.WriteToServer(mErcotOutagesIDdt);
                    SqlCommand updateNodeDALMPH = new SqlCommand("[UPMergeErcotOutagesID]", mConnection90, transaction);
                    updateNodeDALMPH.CommandType = CommandType.StoredProcedure;
                    updateNodeDALMPH.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            mConnection90.Close();
            mUpdateActualEndDateCommand.CommandText = "update Ercot_rt_outages set   ActualEndDate=convert(varchar(10), getdate(),120) where ActualEndDate is null and OutageIdentifier not in  ( " +
                " select OutageIdentifier from OutageID)";

            if (mConnection90.State == ConnectionState.Open)
            {
                mConnection90.Close();
            }
            mConnection90.Open();
            mUpdateActualEndDateCommand.ExecuteNonQuery();
            mConnection90.Close();
            //string updateQuery = "update Ercot_rt_outages set status= 'Complete' and ActualEndDate= PlannedEndtDate where  (ActualEndDate is null) and (OutageIdentifier not in(" + identifierList + "))";


        }
        //public void InsertDB(DataTable mErcotdt)
        //{
        //    if (mConnection90.State == ConnectionState.Open)
        //    {
        //        mConnection90.Close();
        //    }
        //    mConnection90.Open();
        //    SqlTransaction transaction = mConnection90.BeginTransaction();
        //    using (SqlBulkCopy bkoutage = new SqlBulkCopy(mConnection90, SqlBulkCopyOptions.TableLock, transaction))
        //    {
        //        try
        //        {
        //            bkoutage.DestinationTableName = "Ercot_RT_Outagestemp";
        //            bkoutage.ColumnMappings.Add("OutageIdentifier", "OutageIdentifier");
        //            bkoutage.ColumnMappings.Add("PlannedStartDate", "PlannedStartDate");
        //            bkoutage.ColumnMappings.Add("PlannedEndtDate", "PlannedEndtDate");
        //            bkoutage.ColumnMappings.Add("BreakerSwitchNormalStatus", "BreakerSwitchNormalStatus");
        //            bkoutage.ColumnMappings.Add("BreakerSwitchOutageStatus", "BreakerSwitchOutageStatus");
        //            bkoutage.ColumnMappings.Add("OutageStatus", "OutageStatus");
        //            bkoutage.ColumnMappings.Add("RequestorOrgName", "RequestorOrgName");
        //            bkoutage.ColumnMappings.Add("RequestorLongName", "RequestorLongName");
        //            bkoutage.ColumnMappings.Add("EquipmentType", "EquipmentType");
        //            bkoutage.ColumnMappings.Add("EquipmentName", "EquipmentName");
        //            bkoutage.ColumnMappings.Add("EquipmentFromStationName", "EquipmentFromStationName");
        //            bkoutage.ColumnMappings.Add("NatureOfWork", "NatureOfWork");
        //            bkoutage.ColumnMappings.Add("VoltageLevel", "VoltageLevel");
        //            bkoutage.ColumnMappings.Add("SubmitTime", "SubmitTime");
        //            bkoutage.ColumnMappings.Add("OutageType", "OutageType");
        //            bkoutage.ColumnMappings.Add("TEID", "TEID");
        //            bkoutage.ColumnMappings.Add("RestorationTime", "RestorationTime");
        //            bkoutage.ColumnMappings.Add("ReasonForCancellation", "ReasonForCancellation");
        //            bkoutage.ColumnMappings.Add("CancellationDate", "CancellationDate");
        //            bkoutage.ColumnMappings.Add("ActualStartDate", "ActualStartDate");
        //            bkoutage.ColumnMappings.Add("ActualEndDate", "ActualEndDate");
        //            bkoutage.ColumnMappings.Add("GroupLabel", "GroupLabel");
        //            bkoutage.ColumnMappings.Add("EquipmentToStationName", "EquipmentToStationName");
        //            bkoutage.WriteToServer(mErcotdt);
        //            SqlCommand updateNodeDALMPH = new SqlCommand("[UPMergeErcotOutages]", mConnection90, transaction); 
        //            updateNodeDALMPH.CommandType = CommandType.StoredProcedure;
        //            updateNodeDALMPH.ExecuteNonQuery();
        //            transaction.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //        }
        //    }
        //    mConnection90.Close();
        //}
    }
}
