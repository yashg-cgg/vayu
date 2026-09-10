using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vayu.CommonAccessLibrary;
using Xml2CSharp;

namespace Vayu.ErcotNodeDAEnergyDownloader
{
    public class DAEnergyDownload
    {
        private SqlConnection VayuDBConnection;
        X509Certificate2 Ercotcer = new X509Certificate2();
        DataTable dtDAEnergy = new DataTable();
        DataRow drDAEnergy;
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        public DAEnergyDownload()
        {
            DBInit();
            InitCert();
            DADownload();
            DADownloadHistorical();
        }
        public void DBInit()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
            {
                VayuDBConnection.Open();
            }

            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");

            dtDAEnergy.Columns.Add("Name", typeof(string));
            dtDAEnergy.Columns.Add("MarketDate", typeof(DateTime));
            dtDAEnergy.Columns.Add("Hours", typeof(int));
            dtDAEnergy.Columns.Add("Flag", typeof(string));
        }
        public void InitCert()
        {
            

            Ercotcer.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);

        }
         
        public void DADownload()
        {
            try
            {
                string dir = @"D:\ISOFiles\ErcotNodeDAEnergyDownloader\DAEnergy";
                string[] filePaths = Directory.GetFiles(dir);
                for (int i = 0; i < filePaths.Length; i++)
                {
                    File.Delete(filePaths[i]);
                    continue;
                }

                //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://mis.ercot.com/misapp/GetReports.do?reportTypeId=13063&reportTitle=DAM%20Electrically%20Similar%20Settlement%20Points&showHTMLView=&mimicKey");
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.ercot.com/mp/data-products/data-product-details?id=NP4-200-CD");
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ClientCertificates.Add(Ercotcer);
                request.ServicePoint.ConnectionLimit = 1;
                request.Method = "GET";
                request.Timeout = 100000;
                HttpWebResponse fileDownloadrespose = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(fileDownloadrespose.GetResponseStream(), Encoding.UTF8);
                string strLoadDA = sr.ReadToEnd();
                string strDaString = strLoadDA.ToString();
                sr.Close();
                //Spchange
                int k = strDaString.IndexOf("var reportTypeID");
                string typeID = strDaString.Substring(k + 20, 5);
                int l = strDaString.IndexOf("var reportListUrl");
                string listURL = strDaString.Substring(l + 21, 68);
                int m = strDaString.IndexOf("var reportDownloadUrl");
                string downloadURL = strDaString.Substring(m + 25, 67);
                HttpWebRequest request1 = (HttpWebRequest)HttpWebRequest.Create(listURL + typeID);
                request1.KeepAlive = false;
                request1.ProtocolVersion = HttpVersion.Version10;
                request1.ClientCertificates.Add(Ercotcer);
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
                string dataCsv = strArrayList[2];
                if (dataCsv.Contains("DAMDEENERGUZEDSTLPNTNP4200_xml.zip"))
                {
                    int dockIndex = dataCsv.IndexOf("DocID");
                    string dockID = dataCsv.Substring(dockIndex + 8, 10);
                    string FileName = @"D:\ISOFiles\ErcotNodeDAEnergyDownloader\DAEnergy\\Ercot_" + dataCsv.Substring(71, 9) + ".zip";
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(downloadURL + dockID);
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                    HttpWebResponse FileDownloadresponse1 = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string headers = FileDownloadresponse1.Headers["Content-Disposition"];
                    headers = headers.Replace("attachment; filename=", "");
                    string name = headers.Replace("_xml_zip", ".zip");
                    string compare = name.Substring(0, 41);
                    string comparision = headers.Replace(".", "_");
                    string fileDestName = @"D:\ISOFiles\ErcotNodeDAEnergyDownloader\DAEnergy\" + name;
                    using (BinaryReader reader = new BinaryReader(FileDownloadresponse1.GetResponseStream()))
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
                    string Newfilename = XML_UnZipFile(fileDestName, @"D:\ISOFiles\ErcotNodeDAEnergyDownloader\DAEnergy\");
                    XmlSerializer serializer = new XmlSerializer(typeof(DAMDeEnergizedStlPnts));
                    DAMDeEnergizedStlPnts objDAMDeEnergizedStlPnts;
                    using (XmlReader reader = XmlReader.Create(Newfilename))
                    {
                        objDAMDeEnergizedStlPnts = (DAMDeEnergizedStlPnts)serializer.Deserialize(reader);
                    }
                    int count1 = objDAMDeEnergizedStlPnts.DAMDeEnergizedStlPnt.Count;
                    foreach (var item in objDAMDeEnergizedStlPnts.DAMDeEnergizedStlPnt)
                    {
                        string hour = item.HourEnding;
                        string[] arr = hour.Split(':');
                        drDAEnergy = dtDAEnergy.NewRow();
                        drDAEnergy["Name"] = Convert.ToString(item.SettlementPoint.ToString());
                        drDAEnergy["MarketDate"] = DateTime.Parse(item.DeliveryDate);
                        drDAEnergy["Hours"] = Convert.ToInt32(arr[0]);
                        drDAEnergy["Flag"] = Convert.ToString(item.DSTFlag);
                        dtDAEnergy.Rows.Add(drDAEnergy);
                    }
                    SaveDB(dtDAEnergy);
                    File.Delete(Newfilename);
                    File.Delete(fileDestName);
                }


            }
            catch (Exception ex)
            {

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

                                    strNewFile = FolderFilename + @"\" + theEntry.Name;
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
        public void SaveDB(DataTable DAEnergydt)
        {
            if (VayuDBConnection.State == ConnectionState.Closed)
            {
                VayuDBConnection.Open();
            }
            SqlTransaction transaction = VayuDBConnection.BeginTransaction();
            using (SqlBulkCopy bkDAEnergy = new SqlBulkCopy(VayuDBConnection, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkDAEnergy.DestinationTableName = "DeenergizedNodes_test";  
                    bkDAEnergy.ColumnMappings.Add("Name", "Name");
                    bkDAEnergy.ColumnMappings.Add("MarketDate", "MarketDate");
                    bkDAEnergy.ColumnMappings.Add("Hours", "Hours");
                    bkDAEnergy.ColumnMappings.Add("Flag", "Flag");
                    bkDAEnergy.WriteToServer(DAEnergydt);
                    SqlCommand cmdUpdateDAEnergy = new SqlCommand("[UpMergeNodeDAEnergy]", VayuDBConnection, transaction);
                    cmdUpdateDAEnergy.CommandType = CommandType.StoredProcedure;
                    cmdUpdateDAEnergy.CommandTimeout = 300000;
                    cmdUpdateDAEnergy.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            VayuDBConnection.Close();
        }
        public void DADownloadHistorical()
        {
            string dir = @"D:\ISOFiles\ErcotNodeDAEnergyDownloader\DAEnergy";
            string[] filepath = Directory.GetFiles(dir);
            for (int i = 0; i < filepath.Length; i++)
            {
                try
                {
                    string fileDestName = filepath[i];
                    string NewFileName = XML_UnZipFile(fileDestName, @"D:\ISOFiles\ErcotNodeDAEnergyDownloader\DAEnergy\");
                    XmlSerializer serializer = new XmlSerializer(typeof(DAMDeEnergizedStlPnts));
                    DAMDeEnergizedStlPnts objDAMDeEnergizedStlPnts;
                    using (XmlReader reader = XmlReader.Create(NewFileName))
                    {
                        objDAMDeEnergizedStlPnts = (DAMDeEnergizedStlPnts)serializer.Deserialize(reader);
                    }
                    int count = objDAMDeEnergizedStlPnts.DAMDeEnergizedStlPnt.Count;
                    dtDAEnergy.Clear();
                    foreach (var item in objDAMDeEnergizedStlPnts.DAMDeEnergizedStlPnt)
                    {
                        string hour = item.HourEnding;
                        string[] arr = hour.Split(':');
                        drDAEnergy = dtDAEnergy.NewRow();
                        drDAEnergy["Name"] = Convert.ToString(item.SettlementPoint);
                        drDAEnergy["MarketDate"] = DateTime.Parse(item.DeliveryDate);
                        drDAEnergy["Hours"] = Convert.ToInt32(arr[0]);
                        drDAEnergy["Flag"] = Convert.ToString(item.DSTFlag);
                        dtDAEnergy.Rows.Add(drDAEnergy);
                    }
                    SaveDB(dtDAEnergy);
                    File.Delete(fileDestName);
                    File.Delete(NewFileName);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
