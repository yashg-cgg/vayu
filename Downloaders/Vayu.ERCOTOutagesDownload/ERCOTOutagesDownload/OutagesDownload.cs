using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Timers;
using System.Threading;
using Vayu.CommonAccessLibrary;

namespace Vayu.ERCOTOutagesDownload
{
  public  class OutagesDownload
    {
        private SqlConnection Vayudbconnection;
        private SqlConnection mConnection901;
        private SqlCommand mInsertErcotOutagesCommand;
        private SqlCommand mUpdateErcotOutagesCommand;
        private SqlCommand mSelectErcotOutagesCommand;
        private SqlCommand mSelectRemoveErcotOutagesCommand;
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private List<Outages> mOutageList = new List<Outages>();
        private X509Certificate2 mCert = new X509Certificate2();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        DataTable mErcotOutagesdt = new DataTable();
        DataRow mErcotOutagesdr;
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }

        public OutagesDownload()
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

        [Obsolete]
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadOutages();
            mTimer.Enabled = true;
        }


        public void InitDB()
        {
            Vayudbconnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");
            mSelectRemoveErcotOutagesCommand = new SqlCommand();
            mSelectRemoveErcotOutagesCommand.CommandText = "select OutageIdentifier, PlannedStartDate, PlannedEndDate, ActualStartDate, ActualEndDate, OutageStatus, RequestorOrgName, EquipmentType, EquipmentName, EquipmentFromStationName, EquipmentToStationName, VoltageLevel,  SubmitTime, OutageType from ERCOT_OUTAGES where RemovedDate is null or RemovedDate=''";
            mSelectRemoveErcotOutagesCommand.Connection = Vayudbconnection;
            //
            mSelectErcotOutagesCommand = new SqlCommand();
            mSelectErcotOutagesCommand.CommandText = "select top 1 * from Vayu..Ercot_RT_Outages where OutageIdentifier=@OutageIdentifier and EquipmentName=@EquipmentName and PlannedStartDate=@PlannedStartDate and EquipmentType=@EquipmentType order by revisedDate desc";
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@OutageIdentifier", "OutageIdentifier");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@EquipmentName", "EquipmentName");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@PlannedStartDate", "PlannedStartDate");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@EquipmentType", "EquipmentType");
            mSelectErcotOutagesCommand.Connection = Vayudbconnection;
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

            mUpdateErcotOutagesCommand.Connection = Vayudbconnection;
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
            mInsertErcotOutagesCommand.Connection = Vayudbconnection;
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

        }

        [Obsolete]
        public void DownloadOutages()
        {
            //string certPath = "C:\\Certificates\\QSE COBALT_2014.pfx";
            //mCert.Import(certPath, "westoaks", X509KeyStorageFlags.MachineKeySet);

            string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ERCOTOutagesDownload");
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
            }
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=13446&mimic_duns=0809369482000");
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
                        #region OLD
                        // data = data.Replace("\"", "");
                        // data = data.Trim();
                        //// DateTime plannedStartDate = Convert.ToDateTime("2012-05-01");
                        //// DateTime plannedEndDate = Convert.ToDateTime("2012-05-01");
                        //// DateTime actualStartDate = Convert.ToDateTime("2012-05-01");
                        //// DateTime actualEndDate = Convert.ToDateTime("2012-05-01");

                        //// DateTime submitTime = Convert.ToDateTime("2012-05-01");

                        // string[] split1 = data.Split(new char[] { '\n' });

                        // for (int i = 2; i < split1.Length; i++)
                        // {
                        //     Outages outage = new ERCOTOutagesDownload.Outages();
                        //     string[] split2 = split1[i].Split(new char[] { '>' });
                        //     outage.OutageIdentifier = Convert.ToString(split1[3]).TrimStart();
                        //     string PlannedStartDate = Convert.ToString(split1[4]);
                        //     //outage.PlannedStartDate = Convert.ToDateTime(split1[4]);
                        //     outage.PlannedEndDate = Convert.ToDateTime(split1[5]);

                        //     if (!(Convert.ToString(split2[3]) == "" || Convert.ToString(split2[3]) == " "))
                        //     {
                        //         outage.ActualStartDate = Convert.ToDateTime(split2[3]);
                        //     }
                        //     if (!(Convert.ToString(split2[4]) == "" || Convert.ToString(split2[4]) == " "))
                        //     {
                        //         outage.ActualEndDate = Convert.ToDateTime(split2[4]);
                        //     }
                        //     outage.OutageStatus = Convert.ToString(split2[5]).TrimStart();
                        //     outage.RequestorOrgName = Convert.ToString(split2[6]).TrimStart();
                        //     outage.EquipmentType = Convert.ToString(split2[7]).TrimStart();
                        //     outage.EquipmentName = Convert.ToString(split2[8]).TrimStart();
                        //     outage.EquipmentFromStationName = Convert.ToString(split2[9]).TrimStart();
                        //     if (Convert.ToString(split2[10]) == "")
                        //     {
                        //         outage.EquipmentToStationName = "0";
                        //     }
                        //     else
                        //     {
                        //         outage.EquipmentToStationName = Convert.ToString(split2[10]).TrimStart();
                        //     }
                        //     outage.VoltageLevel = Convert.ToDecimal(split2[11]);
                        //     outage.SubmitTime = Convert.ToDateTime(split2[12]);
                        //     outage.OutageType = Convert.ToString(split2[13]).TrimStart();
                        //     outagesList.Add(outage);
                        // }
                        #endregion OLD
                        XmlDataDocument xmlParser = new XmlDataDocument();
                        xmlParser.LoadXml(data);
                        foreach (XmlNode node in xmlParser.ChildNodes)
                        {
                            foreach (XmlNode childnode in node)
                            {
                                mErcotOutagesdr = mErcotOutagesdt.NewRow();
                                mErcotOutagesdr["OutageIdentifier"] = childnode["OutageIdentifier"].InnerText.ToString();
                                mErcotOutagesdr["PlannedStartDate"] = Convert.ToDateTime(childnode["PlannedStartDate"].InnerXml);
                                mErcotOutagesdr["PlannedEndtDate"] = Convert.ToDateTime(childnode["PlannedEndDate"].InnerXml);
                                mErcotOutagesdr["OutageStatus"] = childnode["OutageStatus"].InnerText.ToString();
                                mErcotOutagesdr["RequestorOrgName"] = childnode["RequestorOrgName"].InnerText.ToString();
                                mErcotOutagesdr["RequestorLongName"] = Convert.ToString(childnode["RequestorLongName"].InnerText);
                                mErcotOutagesdr["EquipmentType"] = childnode["EquipmentType"].InnerText.ToString();
                                mErcotOutagesdr["EquipmentName"] = childnode["EquipmentName"].InnerText.ToString();
                                mErcotOutagesdr["EquipmentFromStationName"] = childnode["EquipmentFromStationName"].InnerText.ToString();
                                mErcotOutagesdr["NatureOfWork"] = Convert.ToString(childnode["NatureOfWork"].InnerText);
                                mErcotOutagesdr["VoltageLevel"] = Convert.ToDecimal(childnode["VoltageLevel"].InnerText);
                                mErcotOutagesdr["SubmitTime"] = Convert.ToDateTime(childnode["SubmitTime"].InnerText);
                                mErcotOutagesdr["OutageType"] = Convert.ToString(childnode["OutageType"].InnerText);
                                mErcotOutagesdr["TEID"] = Convert.ToInt32(childnode["TEID"].InnerText);
                                mErcotOutagesdr["RestorationTime"] = Convert.ToInt32(childnode["RestorationTime"].InnerText);
                                if (Convert.ToString(childnode["EquipmentToStationName"]) == string.Empty) { mErcotOutagesdr["EquipmentToStationName"] = DBNull.Value; }
                                else { mErcotOutagesdr["EquipmentToStationName"] = Convert.ToString(childnode["EquipmentToStationName"].InnerText); }

                                if (Convert.ToString(childnode["ReasonForCancellation"]) == string.Empty)
                                {
                                    mErcotOutagesdr["ReasonForCancellation"] = DBNull.Value;
                                }
                                else { mErcotOutagesdr["ReasonForCancellation"] = Convert.ToString(childnode["ReasonForCancellation"].InnerText); }

                                if (Convert.ToString(childnode["CancellationDate"]) == string.Empty)
                                {
                                    mErcotOutagesdr["CancellationDate"] = DBNull.Value;
                                }
                                else { mErcotOutagesdr["CancellationDate"] = Convert.ToDateTime(childnode["CancellationDate"].InnerText); }

                                if (Convert.ToString(childnode["BreakerSwitchNormalStatus"]) == string.Empty)
                                {
                                    mErcotOutagesdr["BreakerSwitchNormalStatus"] = DBNull.Value;
                                }
                                else { mErcotOutagesdr["BreakerSwitchNormalStatus"] = Convert.ToString(childnode["BreakerSwitchNormalStatus"].InnerText); }

                                if (Convert.ToString(childnode["BreakerSwitchOutageStatus"]) == string.Empty)
                                {
                                    mErcotOutagesdr["BreakerSwitchOutageStatus"] = DBNull.Value;
                                }
                                else { mErcotOutagesdr["BreakerSwitchOutageStatus"] = Convert.ToString(childnode["BreakerSwitchOutageStatus"].InnerText); }

                                if (Convert.ToString(childnode["ActualStartDate"]) == string.Empty)
                                {
                                    mErcotOutagesdr["ActualStartDate"] = DBNull.Value;
                                }
                                else { mErcotOutagesdr["ActualStartDate"] = Convert.ToDateTime(childnode["ActualStartDate"].InnerText); }

                                if (Convert.ToString(childnode["ActualEndDate"]) == string.Empty)
                                {
                                    mErcotOutagesdr["ActualEndDate"] = DBNull.Value;
                                }
                                else { mErcotOutagesdr["ActualEndDate"] = Convert.ToDateTime(childnode["ActualEndDate"].InnerText); }

                                if (Convert.ToString(childnode["GroupLabel"]) == string.Empty)
                                {
                                    mErcotOutagesdr["GroupLabel"] = DBNull.Value;
                                }
                                else { mErcotOutagesdr["GroupLabel"] = Convert.ToString(childnode["GroupLabel"].InnerText); }

                                mErcotOutagesdt.Rows.Add(mErcotOutagesdr);
                            }
                        }
                        try
                        {
                            InsertDB(mErcotOutagesdt);
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

        public void InsertDB(DataTable mErcotdt)
        {
            if (Vayudbconnection.State == ConnectionState.Open)
            {
                Vayudbconnection.Close();
            }
            Vayudbconnection.Open();
            SqlTransaction transaction = Vayudbconnection.BeginTransaction();
            using (SqlBulkCopy bkoutage = new SqlBulkCopy(Vayudbconnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkoutage.DestinationTableName = "Ercot_RT_Outagestemp";
                    bkoutage.ColumnMappings.Add("OutageIdentifier", "OutageIdentifier");
                    bkoutage.ColumnMappings.Add("PlannedStartDate", "PlannedStartDate");
                    bkoutage.ColumnMappings.Add("PlannedEndtDate", "PlannedEndtDate");
                    bkoutage.ColumnMappings.Add("BreakerSwitchNormalStatus", "BreakerSwitchNormalStatus");
                    bkoutage.ColumnMappings.Add("BreakerSwitchOutageStatus", "BreakerSwitchOutageStatus");
                    bkoutage.ColumnMappings.Add("OutageStatus", "OutageStatus");
                    bkoutage.ColumnMappings.Add("RequestorOrgName", "RequestorOrgName");
                    bkoutage.ColumnMappings.Add("RequestorLongName", "RequestorLongName");
                    bkoutage.ColumnMappings.Add("EquipmentType", "EquipmentType");
                    bkoutage.ColumnMappings.Add("EquipmentName", "EquipmentName");
                    bkoutage.ColumnMappings.Add("EquipmentFromStationName", "EquipmentFromStationName");
                    bkoutage.ColumnMappings.Add("NatureOfWork", "NatureOfWork");
                    bkoutage.ColumnMappings.Add("VoltageLevel", "VoltageLevel");
                    bkoutage.ColumnMappings.Add("SubmitTime", "SubmitTime");
                    bkoutage.ColumnMappings.Add("OutageType", "OutageType");
                    bkoutage.ColumnMappings.Add("TEID", "TEID");
                    bkoutage.ColumnMappings.Add("RestorationTime", "RestorationTime");
                    bkoutage.ColumnMappings.Add("ReasonForCancellation", "ReasonForCancellation");
                    bkoutage.ColumnMappings.Add("CancellationDate", "CancellationDate");
                    bkoutage.ColumnMappings.Add("ActualStartDate", "ActualStartDate");
                    bkoutage.ColumnMappings.Add("ActualEndDate", "ActualEndDate");
                    bkoutage.ColumnMappings.Add("GroupLabel", "GroupLabel");
                    bkoutage.ColumnMappings.Add("EquipmentToStationName", "EquipmentToStationName");
                    bkoutage.WriteToServer(mErcotdt);
                    SqlCommand updateNodeDALMPH = new SqlCommand("[UPMergeErcotOutages]", Vayudbconnection, transaction); 
                    updateNodeDALMPH.CommandType = CommandType.StoredProcedure;
                    updateNodeDALMPH.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            Vayudbconnection.Close();
        }
    }
}
