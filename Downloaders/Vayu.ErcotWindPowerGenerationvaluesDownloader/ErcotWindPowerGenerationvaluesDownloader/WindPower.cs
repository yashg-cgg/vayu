using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using Xml2CSharp;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using Vayu.CommonAccessLibrary;
using System.Security.Cryptography.X509Certificates;
using Vayu.CertificateInfoLibrary;
using Newtonsoft.Json;

namespace Vayu.ErcotWindPowerGenerationvaluesDownloader
{
    public class WindPower
    {
        SqlConnection VayuDBConnection;
        DataTable dtwindpower = new DataTable();
        DataRow drwindpower;
        private DataTable mERCOTLoadDT = new DataTable();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        private X509Certificate2 ErcotCert = new X509Certificate2();
        private Dictionary<string, int> mHashnode = new Dictionary<string, int>();
        private DataTable mDTConstraintGeo = new DataTable();
        private DataTable mERCOTRTLoadDT = new DataTable();
        private DataRow mERCOTRTLoadRow = null;
        private SqlConnection mVayuDBConnection;
        private SqlCommand mDeleteNodeHERCOTCommand;
        private SqlCommand mDeleteNodeHERCOTDACommand;
        private SqlCommand mDeleteERCOTGeoCommand;
        public WindPower()
        {
            InitDB();
            InitCert();
            DownloadWindPower();
        }
        void InitCert()
        {
            Console.WriteLine("Checking Authentication....");
            ErcotCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        private void LoadDB()
        {
            mVayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            mVayuDBConnection.Open();
        }
        public void InitDB()
        {
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            dtwindpower.Columns.Add("MarketDateTime", typeof(DateTime));
            dtwindpower.Columns.Add("Hour", typeof(int));
            dtwindpower.Columns.Add("RealTimevalue", typeof(decimal));
            dtwindpower.Columns.Add("RTSouth_Houston", typeof(decimal));
            dtwindpower.Columns.Add("RTWest", typeof(decimal));
            dtwindpower.Columns.Add("RTNorth", typeof(decimal));
            dtwindpower.Columns.Add("Forecastvalue", typeof(decimal));
            dtwindpower.Columns.Add("ForeCastSouth_Houston", typeof(decimal));
            dtwindpower.Columns.Add("ForeCastWest", typeof(decimal));
            dtwindpower.Columns.Add("ForeCastNorth", typeof(decimal));
            dtwindpower.Columns.Add("DSTFlag", typeof(string));
        }
        public void DownloadWindPower()
        {
            try
            {

                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloader\ErcotDownloader\");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
                
                string URL = "https://www.ercot.com/mp/data-products/data-product-details?id=NP4-732-CD";
                HttpWebRequest request =(HttpWebRequest)HttpWebRequest.Create(URL);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.ConnectionLimit = 1;
                request.CookieContainer = new CookieContainer();
                request.Method = "GET";
                request.ClientCertificates.Add(ErcotCert);
                request.Timeout = 1000000;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string windpower = sr.ReadToEnd();
                string strDaString = windpower.ToString();
                sr.Close();

                int k = strDaString.IndexOf("var reportTypeID");
                string typeID = strDaString.Substring(k + 20, 5);
                int l = strDaString.IndexOf("var reportListUrl");
                string listURL = strDaString.Substring(l + 21, 68);
                int m = strDaString.IndexOf("var reportDownloadUrl");
                string downloadURL = strDaString.Substring(m + 25, 67);
                HttpWebRequest request1 = (HttpWebRequest)HttpWebRequest.Create(listURL + typeID);
                request1.KeepAlive = false;
                request1.ProtocolVersion = HttpVersion.Version10;
                request1.ClientCertificates.Add(ErcotCert);
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
                string[] strArrayList= strListString.Split(new char[] { '\n'});
                string dataCSV = strArrayList[2];

                if (dataCSV.Contains("WPPHRLYAVGACTNP4732_xml.zip"))
                {
                    int dockIndex = dataCSV.IndexOf("DocID");
                    string dockID = dataCSV.Substring(dockIndex + 8, 10);
                    string FileName = @"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloader\ErcotDownloader\Ercot_" + dataCSV.Substring(71,9) + ".zip";

                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(downloadURL + dockID);

                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                    HttpWebResponse FileDownloadresponse1 = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string headers = FileDownloadresponse1.Headers["content-Disposition"];
                    headers = headers.Replace("attachment; filename=", "");
                    string name = headers.Replace("_xml_zip", ".zip");
                    string compare = name.Substring(0, 41);
                    string comparision = headers.Replace(".", "_");
                    //string fileDestName = @"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloader\ErcotDownloader"+ "WPPHRLYAVGACTNP4732_xml.zip";
                    string fileDestName = @"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloader\ErcotDownloader"+ name;
                   
                    using (BinaryReader reader = new BinaryReader(FileDownloadresponse1.GetResponseStream()))
                    {
                        using (FileStream fileStream = File.Open(FileName, FileMode.Create))
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
                    int hour; DateTime datetime;
                    string NewFileName = XML_UnZipFile(FileName, @"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloader\ErcotDownloader\ErcotWindPower\");
                    XmlSerializer serializer = new XmlSerializer(typeof(WindPowerProductionHourlyAverageActualForecastedValues));
                    WindPowerProductionHourlyAverageActualForecastedValues Windpowergeneration;
                    using (XmlReader reader = XmlReader.Create(NewFileName))
                    {
                        Windpowergeneration = (WindPowerProductionHourlyAverageActualForecastedValues)serializer.Deserialize(reader);
                    }
                    int totalcount = Windpowergeneration.WindPowerProductionHourlyAverageActualForecastedValue.Count;
                    dtwindpower.Clear();
                    foreach (var item in Windpowergeneration.WindPowerProductionHourlyAverageActualForecastedValue)
                    {
                        datetime = Convert.ToDateTime(item.DELIVERY_DATE);
                        hour = Convert.ToInt32(item.HOUR_ENDING);
                        drwindpower = dtwindpower.NewRow();
                        drwindpower["MarketDateTime"] = datetime;
                        drwindpower["Hour"] = hour;
                        drwindpower["RealTimevalue"] = Convert.ToDecimal(item.ACTUAL_SYSTEM_WIDE);
                        drwindpower["RTSouth_Houston"] = Convert.ToDecimal(item.ACTUAL_LZ_SOUTH_HOUSTON);
                        drwindpower["RTWest"] = Convert.ToDecimal(item.ACTUAL_LZ_WEST);
                        drwindpower["RTNorth"] = Convert.ToDecimal(item.ACTUAL_LZ_NORTH);
                        drwindpower["Forecastvalue"] = Convert.ToDecimal(item.STWPF_SYSTEM_WIDE);
                        drwindpower["ForeCastSouth_Houston"] = Convert.ToDecimal(item.STWPF_LZ_SOUTH_HOUSTON);
                        drwindpower["ForeCastWest"] = Convert.ToDecimal(item.STWPF_LZ_WEST);
                        drwindpower["ForeCastNorth"] = Convert.ToDecimal(item.STWPF_LZ_NORTH);
                        drwindpower["DSTFlag"] = Convert.ToString(item.DSTFlag);
                        dtwindpower.Rows.Add(drwindpower);
                    }
                    int rowcount = dtwindpower.Rows.Count;
                    InsertDB(dtwindpower);
                    File.Delete(FileName);
                    File.Delete(NewFileName);
                }

            }
            catch (Exception ex)
            {


            }
        }
        private List<DateTime> GetExistingDates()
        {
            List<DateTime> constraintDateList = new List<DateTime>();
            try
            {
                if (mVayuDBConnection.State == ConnectionState.Closed)
                {
                    mVayuDBConnection.Open();
                }
                SqlCommand cmd = mVayuDBConnection.CreateCommand();
                cmd.CommandText = "select distinct marketdatetime from ConstraintRT where MarketDateTime > '" + DateTime.Today.AddDays(-10).ToString() + "' order by MarketDateTime desc";
                cmd.Connection = mVayuDBConnection;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DateTime marketdatetime = Convert.ToDateTime(rdr.GetValue(0));
                    constraintDateList.Add(marketdatetime);
                }
                rdr.Close();
                mVayuDBConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return constraintDateList;
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

        public void InsertDB(DataTable winddt)
        {
            try
            {
                if (VayuDBConnection.State == ConnectionState.Closed)
                    VayuDBConnection.Open();
                SqlTransaction transaction = VayuDBConnection.BeginTransaction();

                using (SqlBulkCopy bKwind = new SqlBulkCopy(VayuDBConnection, SqlBulkCopyOptions.TableLock, transaction))
                {
                    bKwind.DestinationTableName = "Vayu..WindPowerGenerationValuetest";
                    bKwind.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                    bKwind.ColumnMappings.Add("Hour", "Hour");
                    bKwind.ColumnMappings.Add("RealTimevalue", "RealTimevalue");
                    bKwind.ColumnMappings.Add("RTSouth_Houston", "RTSouth_Houston");
                    bKwind.ColumnMappings.Add("RTWest", "RTWest");
                    bKwind.ColumnMappings.Add("RTNorth", "RTNorth");
                    bKwind.ColumnMappings.Add("Forecastvalue", "Forecastvalue");
                    bKwind.ColumnMappings.Add("ForeCastSouth_Houston", "ForeCastSouth_Houston");
                    bKwind.ColumnMappings.Add("ForeCastWest", "ForeCastWest");
                    bKwind.ColumnMappings.Add("ForeCastNorth", "ForeCastNorth");
                    bKwind.ColumnMappings.Add("DSTFlag", "DSTFlag");
                    bKwind.WriteToServer(winddt);
                    SqlCommand Updatewindpower = new SqlCommand("[dbo].[UpMergeErcotWindPower]", VayuDBConnection, transaction);
                    Updatewindpower.CommandType = CommandType.StoredProcedure;
                    Updatewindpower.CommandTimeout = 30000;
                    Updatewindpower.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    public class Document
    {
        public DateTime ExpiredDate { get; set; }
        public string ILMStatus { get; set; }
        public string SecurityStatus { get; set; }
        public string ContentSize { get; set; }
        public string Extension { get; set; }
        public string ReportTypeID { get; set; }
        public string Prefix { get; set; }
        public string FriendlyName { get; set; }
        public string ConstructedName { get; set; }
        public string DocID { get; set; }
        public DateTime PublishDate { get; set; }
        public string ReportName { get; set; }
        public string DUNS { get; set; }
        public string DocCount { get; set; }
    }

    public class DocumentList
    {
        public Document Document { get; set; }
    }

    public class ListDocsByRptTypeRes
    {
        public List<DocumentList> DocumentList { get; set; }
    }

    public class Root
    {
        public ListDocsByRptTypeRes ListDocsByRptTypeRes { get; set; }
    }
}
