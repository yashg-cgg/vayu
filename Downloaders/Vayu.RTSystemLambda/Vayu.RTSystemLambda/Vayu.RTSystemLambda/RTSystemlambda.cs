using ICSharpCode.SharpZipLib.Zip;
using Vayu.CommonAccessLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace Vayu.RTSystemLambda
{
    class RTSystemlambda
    {

        SqlConnection VayuDbConn;

        private X509Certificate2 mCert = new X509Certificate2();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        private SqlCommand mDeleteNodeHErcotCommand;
        DataTable mERCOTDAMSystemLambdaDT;
        DataRow mERCOTDAMSystemLambdaDT1;
        System.Timers.Timer sTimer = new System.Timers.Timer();

        [Obsolete]
        public RTSystemlambda()
        {
            InitDB();
            InitCert();

            sTimer = new System.Timers.Timer();
            OnTimedEvent(null, null);
            sTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            sTimer.Interval =  60000;//60sec - 1min
            sTimer.Start();
            while (true)
            {
                Thread.Sleep(1000 * 10); // 10 sec
            }

        }

        [Obsolete]
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            try
            {
                Download();
            }
            catch (Exception ex)
            {
            }
            sTimer.Enabled = true;
        }

        [Obsolete]
        private void Download()
        {

            try
            {
                Console.WriteLine("Deleting Old Files ....");

                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\RTsystemLambda");
                try
                {
                    foreach (string item in filePaths)
                    {
                        File.Delete(item);
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(10000);
                    return;
                }

                Console.WriteLine("Downloading New Files ....");


                //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/secure/data-products/markets/real-time-market?id=NP6-322-CD");
               // HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://sa.ercot.com/misapp/GetReports.do?reportTypeId=13114&reportTitle=SCED%20System%20Lambda&showHTMLView=&mimicKey");
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=13114");
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
                strnodedalmp = strnodedalmp.Substring(strnodedalmp.IndexOf("<b>SCED System Lambda</b></"));
                string[] splinelitRow = strnodedalmp.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);

                //for (int k = 6; k < splinelitRow.Length; k = k + 6)
                for (int k =1800; k < 7000; k = k + 6)
                {
                    Console.WriteLine(k);
                    sr.Close();
                    string FileURL = "https://mis.ercot.com/" + splinelitRow[k].Substring(10, 80);
                    string FileName = "D:\\ISOFiles\\RTsystemLambda\\Ercot_" + FileURL.Substring(75, 9) + ".zip";
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                    FileDownloadRequest.ClientCertificates.Add(mCert);
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";

                    HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string fileDestName = FileName;
                    string folderFilename = @"D:\ISOFiles\RTsystemLambda\";
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

                    mERCOTDAMSystemLambdaDT = new DataTable();
                    mERCOTDAMSystemLambdaDT.Columns.Add("DeliveryDate", typeof(DateTime));
                    mERCOTDAMSystemLambdaDT.Columns.Add("SystemLambda", typeof(decimal));
                    mERCOTDAMSystemLambdaDT.Columns.Add("DSTFlag", typeof(char));
                    StreamReader XMLreader = new StreamReader(destinationFolderfilePaths[0]);
                    string filestr = XMLreader.ReadToEnd();
                    XMLreader.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(filestr);
                    mERCOTDAMSystemLambdaDT.Clear();
                    foreach (XmlNode node in xmlParser.ChildNodes)
                    {
                        foreach (XmlNode childnode in node)
                        {
                            mERCOTDAMSystemLambdaDT1 = mERCOTDAMSystemLambdaDT.NewRow();
                            mERCOTDAMSystemLambdaDT1["DeliveryDate"] = childnode.ChildNodes[0].InnerXml;
                            mERCOTDAMSystemLambdaDT1["SystemLambda"] = childnode.ChildNodes[2].InnerXml;
                            mERCOTDAMSystemLambdaDT1["DSTFlag"] = childnode.ChildNodes[1].InnerXml;
                            mERCOTDAMSystemLambdaDT.Rows.Add(mERCOTDAMSystemLambdaDT1);
                        }
                    }

                    if (VayuDbConn.State == ConnectionState.Open)
                    {
                        VayuDbConn.Close();
                    }
                    VayuDbConn.Open();
                    mDeleteNodeHErcotCommand.ExecuteNonQuery();
                    SqlTransaction transaction = VayuDbConn.BeginTransaction();
                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        try
                        {
                            bkLmpH.DestinationTableName = "RTSystemLambdaTest";
                            bkLmpH.ColumnMappings.Add("DeliveryDate", "DeliveryDate");
                            bkLmpH.ColumnMappings.Add("SystemLambda", "SystemLambda");
                            bkLmpH.ColumnMappings.Add("DSTFlag", "DSTFlag");
                            bkLmpH.WriteToServer(mERCOTDAMSystemLambdaDT);
                            SqlCommand updateNodeLmpMin = new SqlCommand("[RTSystemLambdaP]", VayuDbConn, transaction);
                            updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                            updateNodeLmpMin.CommandTimeout = 300000;
                            updateNodeLmpMin.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                        }
                    }
                    File.Delete(FileName);
                    File.Delete(destinationFolderfilePaths[0]);
                    VayuDbConn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();

            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");

            mDeleteNodeHErcotCommand = new SqlCommand();
            mDeleteNodeHErcotCommand.CommandText = "Truncate table RTSystemLambdaTest";
            mDeleteNodeHErcotCommand.Connection = VayuDbConn;

        }
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
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

            }
            return strNewFile;
        }

    }
}
