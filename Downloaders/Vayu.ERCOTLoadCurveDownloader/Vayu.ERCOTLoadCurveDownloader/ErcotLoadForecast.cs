
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Concurrent;
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
using Vayu.CommonAccessLibrary;

namespace Vayu.ERCOTLoadCurveDownloader
{
    public class ErcotLoadForecast
    {
        private SqlConnection vayuDbConn;
        SqlCommand mInsertLoadForecast;
        SqlCommand mUpdateLoadForecast;
        DataTable dtloadsforecast = new DataTable();
        DataRow drloadsforecast;
        X509Certificate2 ErcotCert = new X509Certificate2();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;

        [Obsolete]
        public ErcotLoadForecast()
        {
            InitDB();
            DownloadLoadsForecast();
        }
        public void InitDB()
        {
            vayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            
            mUpdateLoadForecast = new SqlCommand();
            mUpdateLoadForecast.CommandText = "Update Vayu..loadforecasts set MW=@MW , UpdatedDateTime=@UpdatedDateTime where LoadForecastTypeKey=@LoadForecastTypeKey and MarketDateTime=@MarketDateTime";
            mUpdateLoadForecast.Parameters.AddWithValue("@LoadForecastTypeKey", "LoadForecastTypeKey");
            mUpdateLoadForecast.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mUpdateLoadForecast.Parameters.AddWithValue("@MW", "MW");
            mUpdateLoadForecast.Parameters.AddWithValue("@UpdatedDateTime", "UpdatedDateTime");

            //
            mInsertLoadForecast = new SqlCommand();
            mInsertLoadForecast.CommandText = "insert into Vayu..loadforecasts(LoadForecastTypeKey, MarketDateTime, MW,UpdatedDateTime) Values (@LoadForecastTypeKey, @MarketDateTime, @MW,@UpdatedDateTime)";
            mInsertLoadForecast.Parameters.AddWithValue("@LoadForecastTypeKey", "LoadForecastTypeKey");
            mInsertLoadForecast.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertLoadForecast.Parameters.AddWithValue("@MW", "MW");
            mInsertLoadForecast.Parameters.AddWithValue("@UpdatedDateTime", "UpdatedDateTime");

            //
            dtloadsforecast.Columns.Add("Date", typeof(DateTime));
            dtloadsforecast.Columns.Add("LoadForeCastTypeKey", typeof(decimal));
            dtloadsforecast.Columns.Add("MW", typeof(decimal));
            dtloadsforecast.Columns.Add("UpdatedDateTime", typeof(DateTime));
        }

        [Obsolete]
        public void DownloadLoadsForecast()
        {
            InitCert();
            //https://www.ercot.com/mp/data-products/data-product-details?id=NP3-560-CD
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.ercot.com/misapp/servlets/IceDocListJsonWS?reportTypeId=12311");
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ClientCertificates.Add(ErcotCert);
            request.ServicePoint.ConnectionLimit = 1;
            request.Method = WebRequestMethods.Http.Post;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.CookieContainer = new CookieContainer();
            request.Timeout = 1000000;
            HttpWebResponse FileDownloadResponse = (HttpWebResponse)request.GetResponse();
            StreamReader sr = default(StreamReader);
            sr = new StreamReader(FileDownloadResponse.GetResponseStream(), Encoding.UTF8);
            string str = sr.ReadToEnd();
            str = str.ToString();
            sr.Close();
            string[] splinelitRow1 = str.Split(new string[] { "ExpiredDate" }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            foreach (var item in splinelitRow1)
            {
                if (count < splinelitRow1.Count() - 60)
                {
                    if (item.Contains("LFCCONGESTNP3560_xml"))
                    {
                        int publishdate = item.IndexOf("PublishDate");
                        string pDate = item.Substring(publishdate + 14, 19);
                        pDate = pDate.Replace("-", "");
                        pDate = pDate.Replace(":", "");
                        pDate = pDate.Replace("T", "");
                        string PdateFormat = "yyyyMMddHHmmss";
                        DateTime publishedDate = DateTime.ParseExact(pDate, PdateFormat, null);
                        string[] split = item.Split(':');
                        string[] stringSeparators = new string[] { "\"" };
                        string[] result = split[13].Split(stringSeparators, StringSplitOptions.None);
                        string[] splitslash = result[1].Split(',');
                        string[] resultfile = split[12].Split(stringSeparators, StringSplitOptions.None);
                        string[] splitfile = result[1].Split(',');
                        if (result[1] != "DocumentList")
                        {
                            string reportDownloadUrl = "https://www.ercot.com/misdownload/servlets/mirDownload?doclookupId=" + result[1];
                            DownloadXML(reportDownloadUrl, resultfile[1], publishedDate);
                            return;
                        }
                    }
                }
                count++;
            }
        }
        void InitCert()
        {
            ErcotCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }

        [Obsolete]
        private void DownloadXML(string reportDownloadUrl, string fileName, DateTime pDate)
        {
            string FileName = @"D:\ISOFiles\ErcotLoadCurveDownloader\Forecast\" + fileName;
            HttpWebRequest Wrequest = (HttpWebRequest)HttpWebRequest.Create(reportDownloadUrl);
            Wrequest.Method = WebRequestMethods.Http.Post;
            Wrequest.ContentType = "application/zip";
            Wrequest.ContentLength = 0;
            Wrequest.Timeout = 100000;
            Wrequest.Timeout = 100000;
            Wrequest.Method = "GET";
            Wrequest.ClientCertificates.Add(ErcotCert);
            HttpWebResponse Rresponse = (HttpWebResponse)Wrequest.GetResponse();
            Rresponse.Headers.Add("Content-disposition", FileName);
            using (BinaryReader reader = new BinaryReader(Rresponse.GetResponseStream()))
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
            string Newfilename = XML_UnZipFile(FileName, @"D:\ISOFiles\ErcotLoadCurveDownloader\Forecast\");
            if (Newfilename != "")
            {
                if (!Newfilename.Contains(".csv"))
                {
                    StreamReader readerFile = new StreamReader(Newfilename);
                    string filestr = readerFile.ReadToEnd();
                    readerFile.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(filestr);
                    File.Delete(FileName);
                    foreach (XmlNode node in xmlParser)
                    {
                        if (node.Name == "LoadForecasts")
                        {
                            foreach (XmlNode ChildNode in node.ChildNodes)
                            {
                                foreach (XmlNode Cnode in ChildNode.ChildNodes)
                                {
                                    if (Cnode.Name == "East")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[3].InnerText);
                                        drloadsforecast["LoadForeCastTypeKey"] = 33;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }

                                    if (Cnode.Name == "FarWest")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[4].InnerText);
                                        drloadsforecast["LoadForeCastTypeKey"] = 30;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }

                                    if (Cnode.Name == "North")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[2].InnerText);//5
                                        drloadsforecast["LoadForeCastTypeKey"] = 25;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }

                                    if (Cnode.Name == "NorthCentral")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[6].InnerText);
                                        drloadsforecast["LoadForeCastTypeKey"] = 31;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }

                                    if (Cnode.Name == "SouthCentral")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[7].InnerText);
                                        drloadsforecast["LoadForeCastTypeKey"] = 32;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }

                                    if (Cnode.Name == "Southern")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[8].InnerText);
                                        drloadsforecast["LoadForeCastTypeKey"] = 26;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }

                                    if (Cnode.Name == "West")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[4].InnerText);//9
                                        drloadsforecast["LoadForeCastTypeKey"] = 27;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }
                                    if (Cnode.Name == "SystemTotal")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[6].InnerText);//10
                                        drloadsforecast["LoadForeCastTypeKey"] = 34;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }
                                    if (Cnode.Name == "South")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[3].InnerText);//10
                                        drloadsforecast["LoadForeCastTypeKey"] = 26;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }
                                    if (Cnode.Name == "Houston")
                                    {
                                        drloadsforecast = dtloadsforecast.NewRow();
                                        drloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[5].InnerText);//10
                                        drloadsforecast["LoadForeCastTypeKey"] = 28;
                                        drloadsforecast["UpdatedDateTime"] = pDate;
                                        dtloadsforecast.Rows.Add(drloadsforecast);
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < dtloadsforecast.Rows.Count; i++)
                    {

                        mUpdateLoadForecast.Connection = vayuDbConn;
                        if (vayuDbConn.State == ConnectionState.Open)
                        {
                            vayuDbConn.Close();
                        }
                        vayuDbConn.Open();
                         
                        mUpdateLoadForecast.Parameters["@MarketdateTime"].Value = dtloadsforecast.Rows[i]["Date"];
                        mUpdateLoadForecast.Parameters["@MW"].Value = dtloadsforecast.Rows[i]["MW"];
                        mUpdateLoadForecast.Parameters["@LoadForecastTypeKey"].Value = dtloadsforecast.Rows[i]["LoadForeCastTypeKey"];
                        mUpdateLoadForecast.Parameters["@UpdatedDateTime"].Value = dtloadsforecast.Rows[i]["UpdatedDateTime"];

                        int z = mUpdateLoadForecast.ExecuteNonQuery();
                        if (z <= 0)
                        {
                            mInsertLoadForecast.Connection = vayuDbConn;
                            mInsertLoadForecast.Parameters["@MarketdateTime"].Value = dtloadsforecast.Rows[i]["Date"];
                            mInsertLoadForecast.Parameters["@MW"].Value = dtloadsforecast.Rows[i]["MW"];
                            mInsertLoadForecast.Parameters["@LoadForecastTypeKey"].Value = dtloadsforecast.Rows[i]["LoadForeCastTypeKey"];
                            mInsertLoadForecast.Parameters["@UpdatedDateTime"].Value = dtloadsforecast.Rows[i]["UpdatedDateTime"];
                            mInsertLoadForecast.ExecuteNonQuery();
                        }
                        vayuDbConn.Close();
                    }
                    dtloadsforecast.Clear();
                    File.Delete(Newfilename);
                }
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
    }


}

