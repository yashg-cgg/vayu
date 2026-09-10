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
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotWindPowerGenerationvaluesDownloader
{
    public class WindPowerZones
    {
        SqlConnection VayuDBConnection;
        SqlCommand mDeleteCommand;
        DataTable dtwindpower = new DataTable();
        DataRow drwindpower;
        private X509Certificate2 ErcotCert = new X509Certificate2();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        public WindPowerZones()
        {
            InitDB();
            DownloadWindPower();
        }
        
        public void InitDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            dtwindpower.Columns.Add("MarketDateTime", typeof(DateTime));
            dtwindpower.Columns.Add("Hour", typeof(int));
            dtwindpower.Columns.Add("RealTimevalue", typeof(decimal));
            dtwindpower.Columns.Add("Forecastvalue", typeof(decimal));
            dtwindpower.Columns.Add("RTPANHANDLE", typeof(decimal));
            dtwindpower.Columns.Add("ForecastPANHANDLE", typeof(decimal));
            dtwindpower.Columns.Add("RTCOASTAL", typeof(decimal));
            dtwindpower.Columns.Add("ForecastCOASTAL", typeof(decimal));
            dtwindpower.Columns.Add("RTSouth", typeof(decimal));
            dtwindpower.Columns.Add("ForecastSouth", typeof(decimal));
            dtwindpower.Columns.Add("RTWest", typeof(decimal));
            dtwindpower.Columns.Add("ForecastWest", typeof(decimal));
            dtwindpower.Columns.Add("RTNorth", typeof(decimal));
            dtwindpower.Columns.Add("ForecastNorth", typeof(decimal));
            dtwindpower.Columns.Add("DSTFlag", typeof(string));

            mDeleteCommand = new SqlCommand();
            mDeleteCommand.CommandText = "truncate table WindPowerGenerationValueRegionTest";
            mDeleteCommand.Connection = VayuDBConnection;
        }
        public void DownloadWindPower()
        {
            try
            {
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloaderZone\");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
                //string URL = "http://mis.ercot.com/misapp/GetReports.do?reportTypeId=14787&reportTitle=Wind%20Power%20Production%20-%20Hourly%20Averaged%20Actual%20and%20Forecasted%20Values%20by%20Geographical%20Region&showHTMLView=&mimicKey";
                string URL = "https://www.ercot.com/misapp/servlets/IceDocListJsonWS?reportTypeId=14787";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.Method = WebRequestMethods.Http.Post;
                request.Timeout = 100000;
                request.ContentLength = 0;
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse responce = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(responce.GetResponseStream(), Encoding.UTF8);
                string windpower = sr.ReadToEnd();
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(windpower);
                myDeserializedClass.ListDocsByRptTypeRes.DocumentList.Reverse();
                foreach (var document in myDeserializedClass.ListDocsByRptTypeRes.DocumentList)
                {
                    if (document.Document.FriendlyName.Contains("xml"))
                    {
                        string FileURL = "https://www.ercot.com/misdownload/servlets/mirDownload?doclookupId=" + document.Document.DocID + "";
                        string FileName = @"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloaderZone\"+document.Document.DocID+".zip";
                        HttpWebRequest wrequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                        wrequest.Method = WebRequestMethods.Http.Post;
                        wrequest.Timeout = 100000;
                        wrequest.ContentLength = 0;
                        wrequest.ContentType = "application/Zip";
                        HttpWebResponse wresponce = (HttpWebResponse)wrequest.GetResponse();
                        wresponce.Headers.Add("Content-disposition", FileName);
                        using (BinaryReader reader = new BinaryReader(wresponce.GetResponseStream()))
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
                        string NewFileName = XML_UnZipFile(FileName, @"D:\ISOFiles\ErcotWindPowerGenerationvaluesDownloaderZone\");
                        XmlSerializer serializer = new XmlSerializer(typeof(WindPowerProductionHourlyAverageActualForecastedGeoRegionValues));
                        WindPowerProductionHourlyAverageActualForecastedGeoRegionValues Windpowergeneration;
                        using (XmlReader reader = XmlReader.Create(NewFileName))
                        {
                            Windpowergeneration = (WindPowerProductionHourlyAverageActualForecastedGeoRegionValues)serializer.Deserialize(reader);
                        }
                        int totalcount = Windpowergeneration.WindPowerProductionHourlyAverageActualForecastedGeoRegionValue.Count;
                        dtwindpower.Clear();
                        foreach (var item in Windpowergeneration.WindPowerProductionHourlyAverageActualForecastedGeoRegionValue)
                        {
                            datetime = Convert.ToDateTime(item.DELIVERY_DATE);
                            hour = Convert.ToInt32(item.HOUR_ENDING);
                            drwindpower = dtwindpower.NewRow();
                            drwindpower["MarketDateTime"] = datetime;
                            drwindpower["Hour"] = hour;
                            drwindpower["RealTimevalue"] = Convert.ToDecimal(item.ACTUAL_SYSTEM_WIDE);
                            drwindpower["Forecastvalue"] = Convert.ToDecimal(item.COP_HSL_SYSTEM_WIDE);
                            drwindpower["RTPANHANDLE"] = Convert.ToDecimal(item.ACTUAL_PANHANDLE);
                            drwindpower["ForecastPANHANDLE"] = Convert.ToDecimal(item.COP_HSL_PANHANDLE);
                            drwindpower["RTCOASTAL"] = Convert.ToDecimal(item.ACTUAL_COASTAL);
                            drwindpower["ForecastCOASTAL"] = Convert.ToDecimal(item.COP_HSL_COASTAL);
                            drwindpower["RTSouth"] = Convert.ToDecimal(item.ACTUAL_SOUTH);
                            drwindpower["ForecastSouth"] = Convert.ToDecimal(item.COP_HSL_SOUTH);
                            drwindpower["RTWest"] = Convert.ToDecimal(item.ACTUAL_WEST);
                            drwindpower["ForecastWest"] = Convert.ToDecimal(item.COP_HSL_WEST);
                            drwindpower["RTNorth"] = Convert.ToDecimal(item.ACTUAL_NORTH);
                            drwindpower["ForecastNorth"] = Convert.ToDecimal(item.COP_HSL_NORTH);
                            drwindpower["DSTFlag"] = Convert.ToString(item.DSTFlag);
                            dtwindpower.Rows.Add(drwindpower);
                        }
                        int rowcount = dtwindpower.Rows.Count;
                        InsertDB(dtwindpower);
                        File.Delete(FileName);
                        File.Delete(NewFileName);
                    }
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

        public void InsertDB(DataTable winddt)
        {
            try
            {
                if (VayuDBConnection.State == ConnectionState.Closed)
                    VayuDBConnection.Open();
                mDeleteCommand.ExecuteNonQuery();
                SqlTransaction transaction = VayuDBConnection.BeginTransaction();
                using (SqlBulkCopy bKwind = new SqlBulkCopy(VayuDBConnection, SqlBulkCopyOptions.TableLock, transaction))
                {
                    bKwind.DestinationTableName = "Vayu..WindPowerGenerationValueRegionTest";
                    bKwind.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                    bKwind.ColumnMappings.Add("Hour", "Hour");
                    bKwind.ColumnMappings.Add("RealTimevalue", "RealTimevalue");
                    bKwind.ColumnMappings.Add("Forecastvalue", "Forecastvalue");
                    bKwind.ColumnMappings.Add("RTPANHANDLE", "RTPANHANDLE");
                    bKwind.ColumnMappings.Add("ForecastPANHANDLE", "ForecastPANHANDLE");
                    bKwind.ColumnMappings.Add("RTCOASTAL", "RTCOASTAL");
                    bKwind.ColumnMappings.Add("ForecastCOASTAL", "ForecastCOASTAL");
                    bKwind.ColumnMappings.Add("RTSouth", "RTSouth");
                    bKwind.ColumnMappings.Add("ForecastSouth", "ForecastSouth");
                    bKwind.ColumnMappings.Add("RTWest", "RTWest");
                    bKwind.ColumnMappings.Add("ForecastWest", "ForecastWest");
                    bKwind.ColumnMappings.Add("RTNorth", "RTNorth");
                    bKwind.ColumnMappings.Add("ForecastNorth", "ForecastNorth");
                    bKwind.ColumnMappings.Add("DSTFlag", "DSTFlag");
                    bKwind.WriteToServer(winddt);
                    SqlCommand Updatewindpower = new SqlCommand("[dbo].[UpMergeErcotWindPowerRegion]", VayuDBConnection, transaction);
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
}
