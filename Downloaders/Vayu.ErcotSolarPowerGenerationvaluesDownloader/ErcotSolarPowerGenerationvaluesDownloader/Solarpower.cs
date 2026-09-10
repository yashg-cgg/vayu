using System;
using System.Text;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using System.Xml.Serialization;
using Xml2CSharp;
using System.Data;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotSolarPowerGenerationvaluesDownloader
{
    public class Solarpower
    {
        private SqlConnection VayuDBConnection;
        private SqlCommand selectUserPasswordCommand;
        X509Certificate2 Ercotcer = new X509Certificate2();
        DataTable dtsolarpower = new DataTable();
        DataRow drsolarpower; 
        private DataTable mERCOTRTLoadDT = new DataTable();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;

        #region Public method
        public Solarpower()
        {
            InitDB();
            //InitCert();
            DownloadSolarpower();
        }
        public void InitCert()
        {

            Ercotcer.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }
       
        public void InitDB()
        {
            VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            selectUserPasswordCommand = new SqlCommand();
            selectUserPasswordCommand.CommandText = "select CertificateName,Password from Vayu..Certificate where MarketKey=9 and Application_Name='ProdClientAPI'";
            selectUserPasswordCommand.Connection = VayuDBConnection;

            dtsolarpower.Columns.Add("MarketDateTime", typeof(DateTime));
            dtsolarpower.Columns.Add("Hour", typeof(int));
            dtsolarpower.Columns.Add("RealTimevalue", typeof(decimal));
            dtsolarpower.Columns.Add("Forecastvalue", typeof(decimal));
            dtsolarpower.Columns.Add("COPHSLvalue", typeof(decimal));
            dtsolarpower.Columns.Add("PVGRPPvalue", typeof(decimal));
            dtsolarpower.Columns.Add("DSTFlag", typeof(string));
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
        public void InsertDB(DataTable dtSolar)
        {
            try
            {
                if (VayuDBConnection.State == ConnectionState.Closed)
                    VayuDBConnection.Open();
                SqlTransaction transaction = VayuDBConnection.BeginTransaction();
                using (SqlBulkCopy bkSolar = new SqlBulkCopy(VayuDBConnection, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkSolar.DestinationTableName = "Vayu..SolarPowerGenerationValuetest";
                        bkSolar.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                        bkSolar.ColumnMappings.Add("Hour", "Hour");
                        bkSolar.ColumnMappings.Add("RealTimevalue", "RealTimevalue");
                        bkSolar.ColumnMappings.Add("Forecastvalue", "Forecastvalue");
                        bkSolar.ColumnMappings.Add("COPHSLvalue", "COPHSLvalue");
                        bkSolar.ColumnMappings.Add("PVGRPPvalue", "PVGRPPvalue");
                        bkSolar.ColumnMappings.Add("DSTFlag", "DSTFlag");
                        bkSolar.WriteToServer(dtSolar);
                        SqlCommand UpdateSolarPower = new SqlCommand("[dbo].[UpMergeErcotSolarPower]", VayuDBConnection, transaction);
                        UpdateSolarPower.CommandType = CommandType.StoredProcedure;
                        UpdateSolarPower.CommandTimeout = 30000;
                        UpdateSolarPower.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
                transaction.Dispose();
                VayuDBConnection.Close();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Downloader 
        public void DownloadSolarpower()
        {
            try
            {
                #region old Code http site 
                //string URL = "http://mis.ercot.com/misapp/GetReports.do?reportTypeId=13483&reportTitle=Solar%20Power%20Production%20-";
                //request.Method = WebRequestMethods.Http.Post;
                //request.Timeout = 100000;
                //request.ContentLength = 0;
                //request.Credentials = CredentialCache.DefaultCredentials;

                //HttpWebResponse responce = (HttpWebResponse)request.GetResponse();
                //StreamReader sr = default(StreamReader);
                //sr = new StreamReader(responce.GetResponseStream(), Encoding.UTF8);
                //string strsolarpower = sr.ReadToEnd();
                //strsolarpower = strsolarpower.Substring(strsolarpower.IndexOf("_xml"), 1000);
                //string FileURL = strsolarpower.Substring(strsolarpower.IndexOf("_xml"), strsolarpower.IndexOf("'>zip") - strsolarpower.IndexOf("_xml"));
                //FileURL = FileURL.Substring(FileURL.IndexOf("misdownload/servlets/mirDownload?mimic_duns=000000000&doclookupId="), FileURL.Length - FileURL.IndexOf("misdownload/servlets/mirDownload?mimic_duns=000000000&doclookupId="));
                //FileURL = "http://mis.ercot.com/" + FileURL;
                //string FileName = @"D:\ISOFiles\ErcotSolarPowerGenerationvaluesDownloader\ErcotDownloader\" + FileURL.Substring(FileURL.IndexOf("Id=") + 3, FileURL.Length - (FileURL.IndexOf("Id=") + 3)) + ".zip";
                //HttpWebRequest Wrequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                //Wrequest.Method = WebRequestMethods.Http.Post;
                //Wrequest.ContentType = "application/zip";
                //Wrequest.ContentLength = 0;
                //Wrequest.Timeout = 100000;
                //HttpWebResponse Wresponce = (HttpWebResponse)Wrequest.GetResponse();
                //Wresponce.Headers.Add("Content-disposition", FileName);
                //using (BinaryReader reader = new BinaryReader(Wresponce.GetResponseStream()))
                //{
                //    using (FileStream fileStream = File.Open(FileName, FileMode.Create))
                //    {
                //        using (BinaryWriter writer = new BinaryWriter(fileStream))
                //        {
                //            byte[] buffer = new byte[2048];
                //            int count = reader.Read(buffer, 0, buffer.Length);
                //            while (count != 0)
                //            {
                //                writer.Write(buffer, 0, count);
                //                writer.Flush();
                //                count = reader.Read(buffer, 0, buffer.Length);
                //            }
                //            writer.Close();
                //            reader.Close();
                //        }
                //    }
                //}
                //int hour; DateTime datetime;
                //string NewFileName = XML_UnZipFile(FileName, @"D:\ISOFiles\ErcotSolarPowerGenerationvaluesDownloader\ErcotSolarPower\");
                //XmlSerializer serializer = new XmlSerializer(typeof(SolarPowerProductionHourlyAverageActualForecastedValues));
                //SolarPowerProductionHourlyAverageActualForecastedValues SolarPowerProductionHourly;
                //using (XmlReader reader = XmlReader.Create(NewFileName))
                //{
                //    SolarPowerProductionHourly = (SolarPowerProductionHourlyAverageActualForecastedValues)serializer.Deserialize(reader);
                //}
                //int totalcount = SolarPowerProductionHourly.SolarPowerProductionHourlyAverageActualForecastedValue.Count;
                //foreach (var item in SolarPowerProductionHourly.SolarPowerProductionHourlyAverageActualForecastedValue)
                //{
                //    hour = Convert.ToInt32(item.HOUR_ENDING);
                //    datetime = Convert.ToDateTime(item.DELIVERY_DATE);
                //    drsolarpower = dtsolarpower.NewRow();
                //    drsolarpower["MarketDateTime"] = datetime;
                //    drsolarpower["Hour"] = hour;
                //    drsolarpower["RealTimevalue"] = Convert.ToDecimal(item.ACTUAL_SYSTEM_WIDE);
                //    drsolarpower["Forecastvalue"] = Convert.ToDecimal(item.STPPF_SYSTEM_WIDE);
                //    drsolarpower["COPHSLvalue"] = Convert.ToDecimal(item.COP_HSL_SYSTEM_WIDE);
                //    drsolarpower["PVGRPPvalue"] = Convert.ToDecimal(item.PVGRPP_SYSTEM_WIDE);
                //    drsolarpower["DSTFlag"] = Convert.ToString(item.DSTFlag);
                //    dtsolarpower.Rows.Add(drsolarpower);
                //}

                //int dtcount = dtsolarpower.Rows.Count;
                //InsertDB(dtsolarpower);
                //File.Delete(FileName);
                //File.Delete(NewFileName);
                #endregion

                //Updated 10/17/2022
                #region New code https Site 
                string URL = "https://www.ercot.com/mp/data-products/data-product-details?id=NP4-737-CD";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.KeepAlive = false;
                request.ClientCertificates.Add(Ercotcer);
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.ConnectionLimit = 1;
                request.CookieContainer = new CookieContainer();
                request.Method = "POST";
                request.ClientCertificates.Add(Ercotcer);
                request.Timeout = 1000000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string strsolarpower = sr.ReadToEnd();
                string strDaString = strsolarpower.ToString();
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
                bool historicalConstraint = false;
                for (int p = 1; p <= strArrayList.Length; p++)
                {
                    mERCOTRTLoadDT = new DataTable();
                    mERCOTRTLoadDT.Columns.Add("MarketDateTime", typeof(DateTime));
                    mERCOTRTLoadDT.Columns.Add("ConstraintDescription", typeof(string));
                    mERCOTRTLoadDT.Columns.Add("ContingencyText", typeof(string));
                    mERCOTRTLoadDT.Columns.Add("ConstraintText", typeof(string));
                    mERCOTRTLoadDT.Columns.Add("ShadowPrice", typeof(decimal));
                    mERCOTRTLoadDT.Columns.Add("MaxShadowPrice", typeof(decimal));
                    string dataXml = strArrayList[p];
                    if (dataXml.Contains("PVGRHRLYAVGACTNP4737_xml.zip"))
                    {
                        int dateIndex = dataXml.IndexOf(".0000000000000000.");
                        string dateString = dataXml.Substring(dateIndex + 18, 13);
                        string dateString1 = dataXml.Substring(dateIndex + 18, 9);
                        dateString = dateString.Replace(".", "");
                        dateString1 = dateString1.Replace(".", "");
                        string formatString = "yyyyMMddHHmm";
                        string formatString1 = "yyyyMMdd";
                        DateTime filedate = DateTime.ParseExact(dateString, formatString, null);
                        DateTime filedate1 = DateTime.ParseExact(dateString1, formatString1, null);
                        if (filedate1 == DateTime.Today)
                        {
                            int dockIndex = dataXml.IndexOf("DocID");
                            string dockID = dataXml.Substring(dockIndex + 8, 10);
                            HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(downloadURL + dockID);
                            FileDownloadRequest.Timeout = 100000;
                            FileDownloadRequest.Method = "GET";
                            HttpWebResponse FileDownloadresponse1 = (HttpWebResponse)FileDownloadRequest.GetResponse();
                            string headers = FileDownloadresponse1.Headers["Content-Disposition"];
                            if (headers != null)
                            {
                                headers = headers.Replace("attachment; filename=", "");
                                string name = headers.Replace("_xml_zip", ".xml");
                                string compare = name.Substring(0, 41);
                                string comparision = headers.Replace(".", "_");
                                comparision = comparision.Replace("_csv_zip", ".xml");
                                string fileDestName = @"D:\\ISOFiles\\ErcotSolarPowerGenerationvaluesDownloader\\ErcotSolarPower\\" + name;
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
                                int hour; DateTime datetime;
                                string NewFileName = XML_UnZipFile(fileDestName, @"D:\ISOFiles\ErcotSolarPowerGenerationvaluesDownloader\ErcotSolarPower\");
                                XmlSerializer serializer = new XmlSerializer(typeof(SolarPowerProductionHourlyAverageActualForecastedValues));
                                SolarPowerProductionHourlyAverageActualForecastedValues SolarPowerProductionHourly;
                                using (XmlReader reader = XmlReader.Create(NewFileName))
                                {
                                    SolarPowerProductionHourly = (SolarPowerProductionHourlyAverageActualForecastedValues)serializer.Deserialize(reader);
                                }
                                int totalcount = SolarPowerProductionHourly.SolarPowerProductionHourlyAverageActualForecastedValue.Count;
                                foreach (var item in SolarPowerProductionHourly.SolarPowerProductionHourlyAverageActualForecastedValue)
                                {
                                    hour = Convert.ToInt32(item.HOUR_ENDING);
                                    datetime = Convert.ToDateTime(item.DELIVERY_DATE);
                                    drsolarpower = dtsolarpower.NewRow();
                                    drsolarpower["MarketDateTime"] = datetime;
                                    drsolarpower["Hour"] = hour;
                                    drsolarpower["RealTimevalue"] = Convert.ToDecimal(item.ACTUAL_SYSTEM_WIDE);
                                    drsolarpower["Forecastvalue"] = Convert.ToDecimal(item.STPPF_SYSTEM_WIDE);
                                    drsolarpower["COPHSLvalue"] = Convert.ToDecimal(item.COP_HSL_SYSTEM_WIDE);
                                    drsolarpower["PVGRPPvalue"] = Convert.ToDecimal(item.PVGRPP_SYSTEM_WIDE);
                                    drsolarpower["DSTFlag"] = Convert.ToString(item.DSTFlag);
                                    dtsolarpower.Rows.Add(drsolarpower);
                                }

                                int dtcount = dtsolarpower.Rows.Count;
                                InsertDB(dtsolarpower);
                                File.Delete(fileDestName);
                                File.Delete(NewFileName);
                                
                            }
                        }


                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
