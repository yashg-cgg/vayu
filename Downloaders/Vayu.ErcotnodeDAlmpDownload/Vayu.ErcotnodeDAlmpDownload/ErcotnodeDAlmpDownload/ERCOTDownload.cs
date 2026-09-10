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
using Vayu.CertificateInfoLibrary;
namespace Vayu.ErcotnodeDAlmpDownload
{
    public class ERCOTNodeDALmpDownload
    {
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private SqlConnection VayuDbConnErcot;
        private SqlConnection VayuDbConn;
        private Dictionary<string, int> mNodeHash = new Dictionary<string, int>();
        // private static ApplicationLog.ApplicationLog mApplicationLog;
        private SqlCommand mDeleteNodeHErcotCommand;
        private SqlCommand mSelectNodeCommand;
        DataTable mERCOTLoadResourcesDT;
        DataRow mERCOTLoadResourcesDT1;
        private SqlCommand mInsertNodeCommand;
        private X509Certificate2 mCert = new X509Certificate2();
        private SqlCommand mSelectMaxDateCommand;
        public ERCOTNodeDALmpDownload()
        {
            // mApplicationLog = new ApplicationLog.ApplicationLog(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name, "");
            InitDB();
            FillNodeHash();
            CertificateHeler certDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdClientAPI");
            InitCert(certDetails);
            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 60000;
            mTimer.Enabled = true;
            Console.WriteLine("Press \'q\' to quit.");
            //  while (Console.Read() != 'q') ;
        }
        private void FillNodeHash()
        {
            mNodeHash = new Dictionary<string, int>();
            if (VayuDbConnErcot.State == System.Data.ConnectionState.Open)
            {
                VayuDbConnErcot.Close();
            }
            VayuDbConnErcot.Open();
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = Convert.ToInt32(reader.GetValue(0));
                mNodeHash.Add(reader.GetString(1), nodekey);
            }
            reader.Close();
            VayuDbConnErcot.Close();
        }
        public void InitCert(Vayu.CertificateInfoLibrary.CertificateHeler certDetails)
        {
           
            mCert.Import(certDetails.Path, certDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadLoadResources();
            //   DownloadLoadResourcesHist();
            mTimer.Enabled = true;
        }
        public void InitDB()
        {
            VayuDbConnErcot = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            VayuDbConn = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            //
       
            //
            mDeleteNodeHErcotCommand = new SqlCommand();
            // mDeleteNodeHErcotCommand.CommandText = "Truncate table NodeDALMPHTemp";
            mDeleteNodeHErcotCommand.CommandText = "Truncate table NodeDALMPHTest";
            mDeleteNodeHErcotCommand.Connection = VayuDbConnErcot;
            //
            mSelectMaxDateCommand = new SqlCommand();
            mSelectMaxDateCommand.CommandText = "select max(MarketDateTime) from NodeDALMPH_History (nolock) ";
            mSelectMaxDateCommand.Connection = VayuDbConnErcot;

            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select NodeKey, nodename from Vayu_ercot..Node where MarketKey=9";
            mSelectNodeCommand.Connection = VayuDbConnErcot;

            //
            mInsertNodeCommand = new SqlCommand();
            mInsertNodeCommand.CommandText = "if not exists (select * from Vayu_ercot..node where nodename=@nodename and marketkey=9 )" +
                                                    "insert Vayu_ercot..node values (@nodename,  0, 9, 32, 'SOUTH' , NULL , NULL)";
            mInsertNodeCommand.Parameters.AddWithValue("@nodename", "nodename");
            //mInsertNodeCommand.Connection = VayuDbConnErcot;
        }

        [Obsolete]
        public void DownloadLoadResources()
        {
            try
            {
                mTimer.Enabled = false;
                //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Start Saving DA");
<<<<<<< HEAD
<<<<<<< HEAD
                string[] filePaths = Directory.GetFiles(@"H:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\");
=======
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\");
>>>>>>> d095c189a148182b96c6c34129c140cacf917da5
=======
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\");
>>>>>>> d095c189a148182b96c6c34129c140cacf917da5
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=12331&noOfDaysofArchive=30&reportTitle=DAM Settlement Point Prices&showHTMLView=undefined&mimicKey=");
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.ConnectionLimit = 1;
                request.CookieContainer = new CookieContainer();
                request.Method = "GET";
                request.ClientCertificates.Add(mCert);
                request.Timeout = 100000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string strnodedalmp = sr.ReadToEnd();
                strnodedalmp = strnodedalmp.Replace("\r\n", "").Trim();
                strnodedalmp = strnodedalmp.Substring(strnodedalmp.IndexOf("<b>DAM Settlement Point Prices</b></"));
                string[] splinelitRow = strnodedalmp.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 6; k < splinelitRow.Length; k = k + 6)
                {
                    sr.Close();
                    string FileURL = "https://mis.ercot.com/" + splinelitRow[k].Substring(10, 79);
<<<<<<< HEAD
<<<<<<< HEAD
                    string FileName = "H:\\ISOFiles\\ErcotnodeDAlmpDownload\\ERCOTDAResources\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
=======
                    string FileName = "D:\\ISOFiles\\ErcotnodeDAlmpDownload\\ERCOTDAResources\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
>>>>>>> d095c189a148182b96c6c34129c140cacf917da5
=======
                    string FileName = "D:\\ISOFiles\\ErcotnodeDAlmpDownload\\ERCOTDAResources\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
>>>>>>> d095c189a148182b96c6c34129c140cacf917da5
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                    FileDownloadRequest.ClientCertificates.Add(mCert);
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                  
                    HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string fileDestName = FileName;
<<<<<<< HEAD
<<<<<<< HEAD
                    string folderFilename = @"H:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\";
=======
                    string folderFilename = @"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\";
>>>>>>> d095c189a148182b96c6c34129c140cacf917da5
=======
                    string folderFilename = @"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\";
>>>>>>> d095c189a148182b96c6c34129c140cacf917da5
                    using (BinaryReader reader = new BinaryReader(FileDownloadresponse.GetResponseStream()))
                    {
                        using (FileStream fileStream = File.Open(fileDestName, FileMode.Create))
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
                    }
                    XMLUnZipFile(fileDestName, folderFilename);
                    string[] destinationFolderfilePaths = Directory.GetFiles(folderFilename);
                    string[] fileDate = destinationFolderfilePaths[0].Split('.');
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    string format = "yyyyMMdd";
                    DateTime marketDateTime = DateTime.ParseExact(fileDate[3].ToString(), format, provider);
                    VayuDbConnErcot.Open();
                    
                   // DateTime? maxMarketDateTime = (DateTime?)mSelectMaxDateCommand.ExecuteScalar();
                    VayuDbConnErcot.Close();
                    //if (DateTime.Today == marketDateTime && maxMarketDateTime.Value.AddHours(-1) > marketDateTime.AddDays(1))
                    //{
                    //    // mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Exit");
                    //    Environment.Exit(0);
                    //}
                    mERCOTLoadResourcesDT = new DataTable();
                    mERCOTLoadResourcesDT.Columns.Add("NodeKey", typeof(int));
                    mERCOTLoadResourcesDT.Columns.Add("LMP", typeof(decimal));
                    mERCOTLoadResourcesDT.Columns.Add("Congestion", typeof(decimal));
                    mERCOTLoadResourcesDT.Columns.Add("Loss", typeof(decimal));
                    mERCOTLoadResourcesDT.Columns.Add("MarketDateTime", typeof(DateTime));
                    mERCOTLoadResourcesDT.Columns.Add("FinalYN", typeof(char));
                    StreamReader XMLreader = new StreamReader(destinationFolderfilePaths[0]);
                    string filestr = XMLreader.ReadToEnd();
                    XMLreader.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(filestr);
                    mERCOTLoadResourcesDT.Clear();
                    foreach (XmlNode node in xmlParser.ChildNodes)
                    {
                        foreach (XmlNode childnode in node)
                        {
                            mERCOTLoadResourcesDT1 = mERCOTLoadResourcesDT.NewRow();
                            int nodeKey = 0;
                            if (!mNodeHash.ContainsKey((childnode.ChildNodes[2].InnerXml)))
                            {
                                if (VayuDbConn.State == System.Data.ConnectionState.Open)
                                {
                                    VayuDbConn.Close();
                                }
                                VayuDbConn.Open();
                                mInsertNodeCommand.Parameters["@nodename"].Value = childnode.ChildNodes[2].InnerXml;
                                mInsertNodeCommand.ExecuteNonQuery();
                                VayuDbConn.Close();
                                FillNodeHash();
                            }
                            nodeKey = mNodeHash[(childnode.ChildNodes[2].InnerXml)];
                            mERCOTLoadResourcesDT1["NodeKey"] = nodeKey;
                            if (childnode.ChildNodes[3].InnerXml == "")
                            {
                                mERCOTLoadResourcesDT1["LMP"] = DBNull.Value;
                            }
                            else
                            {
                                mERCOTLoadResourcesDT1["LMP"] = childnode.ChildNodes[3].InnerXml;
                            }
                            mERCOTLoadResourcesDT1["Congestion"] = DBNull.Value;
                            mERCOTLoadResourcesDT1["Loss"] = DBNull.Value;
                            mERCOTLoadResourcesDT1["MarketDateTime"] = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml).AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                            mERCOTLoadResourcesDT1["FinalYN"] = "Y";
                            mERCOTLoadResourcesDT.Rows.Add(mERCOTLoadResourcesDT1);
                        }
                    }
                    if (VayuDbConnErcot.State == ConnectionState.Open)
                    {
                        VayuDbConnErcot.Close();
                    }
                    VayuDbConnErcot.Open();
                    mDeleteNodeHErcotCommand.ExecuteNonQuery();
                    SqlTransaction transaction = VayuDbConnErcot.BeginTransaction();
                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConnErcot, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        try
                        {
                            // bkLmpH.DestinationTableName = "[dbo].[NodeDALMPHTemp]";                            
                            bkLmpH.DestinationTableName = "[dbo].[NodeDALMPHTest]";
                            bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                            bkLmpH.ColumnMappings.Add("LMP", "LMP");
                            bkLmpH.ColumnMappings.Add("Congestion", "Congestion");
                            bkLmpH.ColumnMappings.Add("Loss", "Loss");
                            bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                            //  bkLmpH.ColumnMappings.Add("FinalYN", "FinalYN");
                            bkLmpH.WriteToServer(mERCOTLoadResourcesDT);
                            SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeNodeDALMPHErcot]", VayuDbConnErcot, transaction);
                            updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                            updateNodeLmpMin.CommandTimeout = 300000;
                            updateNodeLmpMin.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                        }
                    }
                    File.Delete(FileName);
                    File.Delete(destinationFolderfilePaths[0]);
                    VayuDbConnErcot.Close();
                    //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "End Saving DA");
                }
            }
            catch (Exception ex)
            {
                //  mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            mTimer.Enabled = true;
        }
        public static string XMLUnZipFile(string InputPathOfZipFile, string FolderFilename)
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
                                    strNewFile = @"" + baseDirectory + @"\" + theEntry.Name; //FolderFilename;
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
                                            {
                                                streamWriter.Write(data, 0, size);
                                            }
                                            else
                                            {
                                                break;
                                            }
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
                //mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return strNewFile;
        }

        public void DownloadLoadResourcesHist()
        {
            DateTime mDate = DateTime.Parse("2018/07/20");
            // string dir = "C:\\ErcothistoricData\\DALMP\\cdr.00012331.0000000000000000.20180507.123324773.DAMSPNP4190_xml\\";
            string dir = "D:\\ISOFiles\\ErcotnodeDAlmpDownload\\ErcothistoricData\\DALMP\\cdr.00013044.0000000000000000.20180723.150558169.pricecorrection_DAM_SPP_20180720_to_20180720_NP4196_xml\\";

            string[] filePaths = Directory.GetFiles(dir);

            for (int i = 0; i < filePaths.Length; i++)
            {
                ///  string fileDestName = "C:\\ErcothistoricData\\DALMP\\rpt.00013239.0000809369482000.20180328.114552806.DataRqst_1044607_DAM_SPP_2013_20180327\\Extracted";
                //    string folderFilename = "C:\\ErcothistoricData\\DALMP\\rpt.00013239.0000809369482000.20180328.114552806.DataRqst_1044607_DAM_SPP_2013_20180327\\Extracted";
                //    string FileName = "C:\\ErcothistoricData\\DALMP\\rpt.00013239.0000809369482000.20180328.114552806.DataRqst_1044607_DAM_SPP_2013_20180327\\DAM_SPP_2018_thru_0327\\cdr.00012331.0000000000000000." + mDate.ToString("yyyyMMdd") + ".130907.DAMSPNP4190_xml.zip";
                if (!filePaths[i].Contains("_xml"))
                {

                    File.Delete(filePaths[i]);
                    continue;
                }

                //   else if (filePaths[i].Contains("_csv"))
                //   File.Delete(filePaths[i]);
                string fileDestName = filePaths[i];
                string FileName = fileDestName;
                string folderFilename = dir;
                //   string fileDestName = FileName;
                XMLUnZipFile(fileDestName, folderFilename);
                string[] destinationFolderfilePaths = Directory.GetFiles(folderFilename);
                string[] fileDate = destinationFolderfilePaths[0].Split('.');
                CultureInfo provider = CultureInfo.InvariantCulture;
                string format = "yyyyMMdd";
                DateTime marketDateTime = DateTime.ParseExact(fileDate[3].ToString(), format, provider);
                VayuDbConnErcot.Open();
                //   DateTime? maxMarketDateTime = (DateTime?)mSelectMaxDateCommand.ExecuteScalar();
                VayuDbConnErcot.Close();
                //    if (DateTime.Today == marketDateTime && maxMarketDateTime.Value.AddHours(-1) > marketDateTime.AddDays(1))
                {
                    // mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Exit");
                    //  Environment.Exit(0);
                }
                StreamReader XMLreader = new StreamReader(destinationFolderfilePaths[0]);
                string filestr = XMLreader.ReadToEnd();
                XMLreader.Close();
                XmlDataDocument xmlParser = new XmlDataDocument();
                xmlParser.LoadXml(filestr);
                mERCOTLoadResourcesDT.Clear();
                foreach (XmlNode node in xmlParser.ChildNodes)
                {
                    foreach (XmlNode childnode in node)
                    {
                        mERCOTLoadResourcesDT1 = mERCOTLoadResourcesDT.NewRow();
                        int nodeKey = 0;
                        if (!mNodeHash.ContainsKey((childnode.ChildNodes[2].InnerXml)))
                        {
                            if (VayuDbConn.State == System.Data.ConnectionState.Open)
                            {
                                VayuDbConn.Close();
                            }
                            VayuDbConn.Open();
                            mInsertNodeCommand.Parameters["@nodename"].Value = childnode.ChildNodes[2].InnerXml;
                            mInsertNodeCommand.ExecuteNonQuery();
                            VayuDbConn.Close();
                            FillNodeHash();
                        }
                        nodeKey = mNodeHash[(childnode.ChildNodes[2].InnerXml)];
                        mERCOTLoadResourcesDT1["NodeKey"] = nodeKey;

                        if (childnode.ChildNodes[3].InnerXml == "")
                        {
                            mERCOTLoadResourcesDT1["LMP"] = DBNull.Value;
                        }
                        else
                        {
                            mERCOTLoadResourcesDT1["LMP"] = childnode.ChildNodes[3].InnerXml;
                        }

                        mERCOTLoadResourcesDT1["Congestion"] = DBNull.Value;
                        mERCOTLoadResourcesDT1["Loss"] = DBNull.Value;

                        if (childnode.ChildNodes[1].InnerXml.Length == 1)
                        {
                            mERCOTLoadResourcesDT1["MarketDateTime"] = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml).AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 1)));
                        }
                        else if (childnode.ChildNodes[1].InnerXml.Length == 2)
                        {
                            mERCOTLoadResourcesDT1["MarketDateTime"] = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml).AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                        }
                        else
                        {
                            mERCOTLoadResourcesDT1["MarketDateTime"] = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml).AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                        }

                        mERCOTLoadResourcesDT1["FinalYN"] = "Y";
                        mERCOTLoadResourcesDT.Rows.Add(mERCOTLoadResourcesDT1);
                    }
                }
                if (VayuDbConnErcot.State == ConnectionState.Open)
                {
                    VayuDbConnErcot.Close();
                }
                VayuDbConnErcot.Open();
                //mDeleteNodeHErcotCommand.ExecuteNonQuery();
                //SqlTransaction transaction = VayuDbConnErcot.BeginTransaction();
                //using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConnErcot, SqlBulkCopyOptions.TableLock, transaction))
                //{
                //    try
                //    {
                //        bkLmpH.DestinationTableName = "[dbo].[NodeDALMPH_Temp]";
                //        bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                //        bkLmpH.ColumnMappings.Add("LMP", "LMP");
                //        bkLmpH.ColumnMappings.Add("Congestion", "Congestion");
                //        bkLmpH.ColumnMappings.Add("Loss", "Loss");
                //        bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                //        //  bkLmpH.ColumnMappings.Add("FinalYN", "FinalYN");
                //        bkLmpH.WriteToServer(mERCOTLoadResourcesDT);
                //        SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeNodeLMPH_History]", VayuDbConnErcot, transaction);
                //        updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                //        updateNodeLmpMin.ExecuteNonQuery();
                //        transaction.Commit();
                //    }
                //    catch (Exception ex)
                //    {
                //        transaction.Rollback();
                //        // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                //    }
                //}

                mDeleteNodeHErcotCommand.ExecuteNonQuery();
                SqlTransaction transaction = VayuDbConnErcot.BeginTransaction();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConnErcot, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkLmpH.DestinationTableName = "[dbo].[NodeDALMPH_Temp]";
                        bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                        bkLmpH.ColumnMappings.Add("LMP", "LMP");
                        bkLmpH.ColumnMappings.Add("Congestion", "Congestion");
                        bkLmpH.ColumnMappings.Add("Loss", "Loss");
                        bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                        //  bkLmpH.ColumnMappings.Add("FinalYN", "FinalYN");
                        bkLmpH.WriteToServer(mERCOTLoadResourcesDT);
                        SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeNodeDALMPHErcot]", VayuDbConnErcot, transaction);
                        updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                        updateNodeLmpMin.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                    }
                }
                File.Delete(FileName);
                File.Delete(destinationFolderfilePaths[0]);
                VayuDbConnErcot.Close();
            }


        }
    }
}