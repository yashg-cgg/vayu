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
using System.Xml;

namespace Vayu.DAMPTPObligationResultsbySettlementPoint
{
    class DAMPTPObligationResultsbySettlementPoint
    {
        SqlConnection VayuDbConn;
        
        private X509Certificate2 mCert = new X509Certificate2();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        private SqlCommand mSelectNodeCommand;
        private Dictionary<string, int> mNodeHash = new Dictionary<string, int>();
        private SqlCommand mDeleteNodeHErcotCommand;
        DataTable mERCOTDAMPTPOBLSPDT;
        DataRow mERCOTDAMPTPOBLSPDT1;
        public DAMPTPObligationResultsbySettlementPoint()
        {
            InitDB();
            FillNodeHash();
            InitCert();
            Download();
        }
        public void InitDB()
        {
            VayuDbConn = new SqlConnection(DBConnectionCredentials.GetERCOTDBConnection());
            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ProdClientCert");

            //userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ErcotSubmission");
            //serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ErcotDownloadersServer");

            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select NodeKey, nodename from Vayu..Node where MarketKey=9";
            mSelectNodeCommand.Connection = VayuDbConn;

            mDeleteNodeHErcotCommand = new SqlCommand();
            mDeleteNodeHErcotCommand.CommandText = "Truncate table DAMPTPObligationTest";
            mDeleteNodeHErcotCommand.Connection = VayuDbConn;

        }
        
        private void Download()
        {
            try
            {
                Console.WriteLine("Downloading File.....");
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\DAMPTPObligationResultsbySettlementPoint\");
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
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=13042");
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
                strnodedalmp = strnodedalmp.Substring(strnodedalmp.IndexOf("<b>DAM PTP Obligation Results by Settlement Point</b></"));
                string[] splinelitRow = strnodedalmp.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);

                for (int k = 6; k < splinelitRow.Length; k = k + 6)
                {
                    sr.Close();
                    string FileURL = "https://mis.ercot.com/" + splinelitRow[k].Substring(10, 80);
                    string FileName = "D:\\ISOFiles\\DAMPTPObligationResultsbySettlementPoint\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                    FileDownloadRequest.ClientCertificates.Add(mCert);
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                    HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string fileDestName = FileName;
                    string folderFilename = @"D:\ISOFiles\DAMPTPObligationResultsbySettlementPoint\";
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
                    mERCOTDAMPTPOBLSPDT = new DataTable();
                    mERCOTDAMPTPOBLSPDT.Columns.Add("NodeKey", typeof(int));
                    mERCOTDAMPTPOBLSPDT.Columns.Add("DELIVERYDATE", typeof(DateTime));
                    mERCOTDAMPTPOBLSPDT.Columns.Add("HOURENDING", typeof(int));
                    mERCOTDAMPTPOBLSPDT.Columns.Add("STL_PNT", typeof(string));
                    mERCOTDAMPTPOBLSPDT.Columns.Add("TOTAL_PTP_OBL_AWARDED_SOURCE", typeof(decimal));
                    mERCOTDAMPTPOBLSPDT.Columns.Add("TOTAL_PTP_OBL_AWARDED_SINK", typeof(decimal));
                    mERCOTDAMPTPOBLSPDT.Columns.Add("DSTFlag", typeof(char));
                    StreamReader XMLreader = new StreamReader(destinationFolderfilePaths[0]);
                    string filestr = XMLreader.ReadToEnd();
                    XMLreader.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(filestr);
                    mERCOTDAMPTPOBLSPDT.Clear();
                    foreach (XmlNode node in xmlParser.ChildNodes)
                    {
                        foreach (XmlNode childnode in node)
                        {
                            mERCOTDAMPTPOBLSPDT1 = mERCOTDAMPTPOBLSPDT.NewRow();
                            int nodeKey = 0;
                            if (!mNodeHash.ContainsKey(childnode.ChildNodes[2].InnerXml))
                            {
                                if (VayuDbConn.State == System.Data.ConnectionState.Open)
                                {
                                    VayuDbConn.Close();
                                }
                                VayuDbConn.Open();
                                mSelectNodeCommand.Parameters["@nodename"].Value = childnode.ChildNodes[2].InnerXml;
                                mSelectNodeCommand.ExecuteNonQuery();
                                VayuDbConn.Close();
                                FillNodeHash();
                            }
                            nodeKey = mNodeHash[(childnode.ChildNodes[2].InnerXml)];
                            mERCOTDAMPTPOBLSPDT1["NodeKey"] = nodeKey;
                            mERCOTDAMPTPOBLSPDT1["DELIVERYDATE"] = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml);
                            mERCOTDAMPTPOBLSPDT1["HOURENDING"] =Convert.ToInt32(childnode.ChildNodes[1].InnerXml.Replace(":00",""));
                            mERCOTDAMPTPOBLSPDT1["STL_PNT"] = childnode.ChildNodes[2].InnerXml;
                            mERCOTDAMPTPOBLSPDT1["TOTAL_PTP_OBL_AWARDED_SOURCE"] = childnode.ChildNodes[3].InnerXml;
                            mERCOTDAMPTPOBLSPDT1["TOTAL_PTP_OBL_AWARDED_SINK"] = childnode.ChildNodes[4].InnerXml;
                            mERCOTDAMPTPOBLSPDT1["DSTFlag"] = childnode.ChildNodes[5].InnerXml;
                            mERCOTDAMPTPOBLSPDT.Rows.Add(mERCOTDAMPTPOBLSPDT1);
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
                            bkLmpH.DestinationTableName = "[dbo].[DAMPTPObligationTest]";
                            bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                            bkLmpH.ColumnMappings.Add("DELIVERYDATE", "DELIVERYDATE");
                            bkLmpH.ColumnMappings.Add("HOURENDING", "HOURENDING");
                            bkLmpH.ColumnMappings.Add("STL_PNT", "STL_PNT");
                            bkLmpH.ColumnMappings.Add("TOTAL_PTP_OBL_AWARDED_SOURCE", "TOTAL_PTP_OBL_AWARDED_SOURCE");
                            bkLmpH.ColumnMappings.Add("TOTAL_PTP_OBL_AWARDED_SINK", "TOTAL_PTP_OBL_AWARDED_SINK");
                            bkLmpH.ColumnMappings.Add("DSTFlag", "DSTFlag");
                            bkLmpH.WriteToServer(mERCOTDAMPTPOBLSPDT);
                            SqlCommand updateNodeLmpMin = new SqlCommand("[DAMPTPObligationResultsbySettlementPoint]", VayuDbConn, transaction);
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

            }
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
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }
        private void FillNodeHash()
        {
            mNodeHash = new Dictionary<string, int>();
            if (VayuDbConn.State == ConnectionState.Closed)
                VayuDbConn.Open();
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = Convert.ToInt32(reader.GetValue(0));
                mNodeHash.Add(reader.GetString(1), nodekey);
            }
            reader.Close();
            VayuDbConn.Close();
        }
    }
}
