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

namespace Vayu.CRROwnershipofRecord
{
    class CRROwnershipofRecord
    {
        SqlConnection VayuDbConn;
        private X509Certificate2 mCert = new X509Certificate2();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        private SqlCommand mDeleteNodeHErcotCommand;
        DataTable mERCOTCRROwnershipDT;
        DataRow mERCOTCRROwnershipDT1;
        public CRROwnershipofRecord()
        {
            InitDB();
            InitCert();
            Download();
        }
        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();


            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");

            mDeleteNodeHErcotCommand = new SqlCommand();           
            mDeleteNodeHErcotCommand.CommandText = "Truncate table CRROwnershipofRecord";
            mDeleteNodeHErcotCommand.Connection = VayuDbConn;

        }
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }

        [Obsolete]
        private void Download()
        {
                try
                {
                    Console.WriteLine("Downloading File.....");
                    string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\CRROwnershipofRecord\");
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
                    //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://mis.ercot.com/misapp/GetReports.do?reportTypeId=11206&reportTitle=CRR%20Ownership%20of%20Record&showHTMLView=&mimicKey");
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.ercot.com/mp/data-products/data-product-details?id=NP7-157-SG");
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
                    sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    string str = sr.ReadToEnd();
                    str = str.ToString();
                    sr.Close();

                    //SpChange
                    int s = str.IndexOf("var reportTypeID");
                    string typeID = str.Substring(s + 20, 5);
                    int l = str.IndexOf("var reportListUrl");
                    string listURL = str.Substring(l + 21, 68);
                    int m = str.IndexOf("var reportDownloadUrl");
                    string downloadURL = str.Substring(m + 25, 67);
                    HttpWebRequest request1 = (HttpWebRequest)HttpWebRequest.Create(listURL + typeID);
                    request1.KeepAlive = false;
                    request1.ProtocolVersion = HttpVersion.Version10;
                    request1.ClientCertificates.Add(mCert);
                    request1.ServicePoint.ConnectionLimit = 1;
                    request1.Method = "GET";
                    request1.Timeout = 100000;
                    HttpWebResponse FileDownloadresponseList = (HttpWebResponse)request1.GetResponse();
                    StreamReader sr1 = default(StreamReader);
                    sr1 = new StreamReader(FileDownloadresponseList.GetResponseStream(), Encoding.UTF8);
                    string strList = sr1.ReadToEnd();
                    string strListString = strList.ToString();
                    sr1.Close();
                    strListString = strListString.Replace("ExpiredDate", "\n");
                    string[] strArrayList = strListString.Split(new char[] { '\n' });
                    //SpChangeEnd
                    string dataCsv = strArrayList[1];
                   
                        int dockIndex = dataCsv.IndexOf("DocID");
                        string dockID = dataCsv.Substring(dockIndex + 8, 10);
                        //string FileURL = "http://mis.ercot.com/misdownload/servlets/mirDownload?mimic_duns=000000000&doclookupId=751359640";
                            string FileName = "D:\\ISOFiles\\CRROwnershipofRecord\\Ercot_" + dataCsv.Substring(71, 9) + ".zip";
                        HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(downloadURL + dockID);
                        FileDownloadRequest.ClientCertificates.Add(mCert);
                        FileDownloadRequest.Timeout = 100000;
                        FileDownloadRequest.Method = "GET";

                        HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                        string fileDestName = FileName;
                        string folderFilename = @"D:\ISOFiles\CRROwnershipofRecord\";
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
                        string[] fileDate = destinationFolderfilePaths[1].Split('.');

                        mERCOTCRROwnershipDT = new DataTable();
                        mERCOTCRROwnershipDT.Columns.Add("CRR_ID", typeof(int));
                        mERCOTCRROwnershipDT.Columns.Add("SegmentID", typeof(int));
                        mERCOTCRROwnershipDT.Columns.Add("AccountHolder", typeof(string));
                        mERCOTCRROwnershipDT.Columns.Add("HedgeType", typeof(string));
                        mERCOTCRROwnershipDT.Columns.Add("CRRType", typeof(string));
                        mERCOTCRROwnershipDT.Columns.Add("Source", typeof(string));
                        mERCOTCRROwnershipDT.Columns.Add("Sink", typeof(string));
                        mERCOTCRROwnershipDT.Columns.Add("StartDate", typeof(DateTime));
                        mERCOTCRROwnershipDT.Columns.Add("EndDate", typeof(DateTime));
                        mERCOTCRROwnershipDT.Columns.Add("TimeOfUse", typeof(string));
                        mERCOTCRROwnershipDT.Columns.Add("MW", typeof(decimal));

                        StreamReader XMLreader = new StreamReader(destinationFolderfilePaths[1]);
                        string filestr = XMLreader.ReadToEnd();
                        XMLreader.Close();
                        XmlDataDocument xmlParser = new XmlDataDocument();
                        xmlParser.LoadXml(filestr);
                        mERCOTCRROwnershipDT.Clear();
                        foreach (XmlNode node in xmlParser.ChildNodes)
                    {
                        foreach (XmlNode childnode in node)
                        {
                            mERCOTCRROwnershipDT1 = mERCOTCRROwnershipDT.NewRow();
                            mERCOTCRROwnershipDT1["CRR_ID"] = childnode.ChildNodes[0].InnerXml;
                            mERCOTCRROwnershipDT1["SegmentID"] = childnode.ChildNodes[1].InnerXml;
                            mERCOTCRROwnershipDT1["AccountHolder"] = childnode.ChildNodes[2].InnerXml;
                            mERCOTCRROwnershipDT1["HedgeType"] = childnode.ChildNodes[3].InnerXml;
                            mERCOTCRROwnershipDT1["CRRType"] = childnode.ChildNodes[4].InnerXml;
                            mERCOTCRROwnershipDT1["Source"] = childnode.ChildNodes[5].InnerXml;
                            mERCOTCRROwnershipDT1["Sink"] = childnode.ChildNodes[6].InnerXml;
                            mERCOTCRROwnershipDT1["StartDate"] = Convert.ToDateTime(childnode.ChildNodes[7].InnerXml);
                            mERCOTCRROwnershipDT1["EndDate"] = Convert.ToDateTime(childnode.ChildNodes[8].InnerXml);
                            mERCOTCRROwnershipDT1["TimeOfUse"] = childnode.ChildNodes[9].InnerXml;
                            mERCOTCRROwnershipDT1["MW"] = childnode.ChildNodes[10].InnerXml;

                            mERCOTCRROwnershipDT.Rows.Add(mERCOTCRROwnershipDT1);
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
                            bkLmpH.DestinationTableName = "[dbo].[CRROwnershipofRecord]";
                            bkLmpH.BulkCopyTimeout = 300000;
                            bkLmpH.BatchSize = 3000;

                            bkLmpH.ColumnMappings.Add("CRR_ID", "CRR_ID");
                            bkLmpH.ColumnMappings.Add("SegmentID", "SegmentID");
                            bkLmpH.ColumnMappings.Add("AccountHolder", "AccountHolder");
                            bkLmpH.ColumnMappings.Add("HedgeType", "HedgeType");
                            bkLmpH.ColumnMappings.Add("CRRType", "CRRType");
                            bkLmpH.ColumnMappings.Add("Source", "Source");
                            bkLmpH.ColumnMappings.Add("Sink", "Sink");
                            bkLmpH.ColumnMappings.Add("StartDate", "StartDate");
                            bkLmpH.ColumnMappings.Add("EndDate", "EndDate");
                            bkLmpH.ColumnMappings.Add("TimeOfUse", "TimeOfUse");
                            bkLmpH.ColumnMappings.Add("MW", "MW");
                            bkLmpH.WriteToServer(mERCOTCRROwnershipDT);
                       
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                        }
                    }

                        string[] delallfile = Directory.GetFiles(@"D:\ISOFiles\CRROwnershipofRecord\");
                        foreach (string item in delallfile)
                    {
                        File.Delete(item);
                    }
                        VayuDbConn.Close();
                    
                        
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
                //mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return strNewFile;
        }
    }
}
