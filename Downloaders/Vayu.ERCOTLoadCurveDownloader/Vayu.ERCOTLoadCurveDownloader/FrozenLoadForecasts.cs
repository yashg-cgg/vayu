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
using Vayu.CommonAccessLibrary;

namespace Vayu.ERCOTLoadCurveDownloader
{
    public class FrozenLoadForecasts
    {
        private SqlConnection VayuDbConn;
        SqlCommand mInsertFrozonLoadForecast;
        SqlCommand mUpdateLoadForecast;
        DataTable dtFrozonloadsforecast = new DataTable();
        DataRow drFrozonloadsforecast;
        X509Certificate2 ErcotCert = new X509Certificate2();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        public FrozenLoadForecasts()
        {
            InitDB();
            DownloadFrozenLoadsForecast();
        }
        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
           // userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9, "ErcotSubmission");
            mInsertFrozonLoadForecast = new SqlCommand();
            mInsertFrozonLoadForecast.CommandText = "if not exists(Select * from Vayu..FrozenLoadForecasts where MarketDateTime=@MarketDateTime and LoadForecastTypeKey=@LoadForecastTypeKey) insert into Vayu..FrozenLoadForecasts(LoadForecastTypeKey, MarketDateTime, MW) Values (@LoadForecastTypeKey, @MarketDateTime, @MW)";
            mInsertFrozonLoadForecast.Parameters.AddWithValue("@LoadForecastTypeKey", "LoadForecastTypeKey");
            mInsertFrozonLoadForecast.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertFrozonLoadForecast.Parameters.AddWithValue("@MW", "MW");

            //
            dtFrozonloadsforecast.Columns.Add("Date", typeof(DateTime));
            dtFrozonloadsforecast.Columns.Add("LoadForeCastTypeKey", typeof(decimal));
            dtFrozonloadsforecast.Columns.Add("MW", typeof(decimal));
        }
        void InitCert()
        {
            ErcotCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }

        [Obsolete]
        public void DownloadFrozenLoadsForecast()
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
                //if (count < splinelitRow1.Count() - 60)
                {
                    if (item.Contains("LFCCONGESTNP3560_xml"))
                    {
                        string[] split = item.Split(':');
                        string[] stringSeparators = new string[] { "\"" };
                        string[] result = split[13].Split(stringSeparators, StringSplitOptions.None);
                        string[] splitslash = result[1].Split(',');
                        string[] resultfile = split[12].Split(stringSeparators, StringSplitOptions.None);
                        string[] splitfile = result[1].Split(',');
                        if (result[1] != "DocumentList")
                        {
                            string reportDownloadUrl = "https://www.ercot.com/misdownload/servlets/mirDownload?doclookupId=" + result[1];
                            DownloadXML(reportDownloadUrl, resultfile[1]);
                        }
                    }
                }
                count++;
            }
             
        }

        [Obsolete]
        private void DownloadXML(string reportDownloadUrl, string fileName)
        {
            string FileName = @"D:\ISOFiles\ErcotLoadCurveDownloader\Frozen\" + fileName;
            HttpWebRequest Wrequest = (HttpWebRequest)HttpWebRequest.Create(reportDownloadUrl);
            Wrequest.Method = WebRequestMethods.Http.Post;
            Wrequest.ContentType = "application/zip";
            Wrequest.ContentLength = 0;
            Wrequest.Timeout = 100000;
            Wrequest.Timeout = 100000;
            Wrequest.Method = "GET";
            Wrequest.ClientCertificates.Add(ErcotCert);
            HttpWebResponse Wresponse = (HttpWebResponse)Wrequest.GetResponse();
            Wresponse.Headers.Add("Content-disposition", FileName);
            using (BinaryReader reader = new BinaryReader(Wresponse.GetResponseStream()))
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
            string Newfilename = XML_UnZipFile(FileName, @"D:\ISOFiles\ErcotLoadCurveDownloader\Frozen\");
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
                    foreach (XmlNode node in xmlParser.ChildNodes)
                    {
                        if (node.Name == "LoadForecasts")
                        {
                            foreach (XmlNode ChildNode in node.ChildNodes)
                            {
                                foreach (XmlNode Cnode in ChildNode.ChildNodes)
                                {
                                    if (Cnode.Name == "East")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[3].InnerText);
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 33;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }

                                    if (Cnode.Name == "FarWest")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[4].InnerText);
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 30;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }

                                    if (Cnode.Name == "North")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[2].InnerText);//5
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 25;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }

                                    if (Cnode.Name == "NorthCentral")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[6].InnerText);
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 31;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }

                                    if (Cnode.Name == "SouthCentral")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[7].InnerText);
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 32;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }

                                    if (Cnode.Name == "Southern")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[8].InnerText);
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 26;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }

                                    if (Cnode.Name == "West")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[4].InnerText);//9
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 27;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }
                                    if (Cnode.Name == "SystemTotal")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[6].InnerText);//10
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 34;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }
                                    if (Cnode.Name == "South")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[3].InnerText);//10
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 26;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }
                                    if (Cnode.Name == "Houston")
                                    {
                                        drFrozonloadsforecast = dtFrozonloadsforecast.NewRow();
                                        drFrozonloadsforecast["Date"] = Convert.ToDateTime(ChildNode.ChildNodes[0].InnerText).AddHours(Convert.ToInt16(ChildNode.ChildNodes[1].InnerText.Remove(ChildNode.ChildNodes[1].InnerText.IndexOf(":"), 3)));
                                        drFrozonloadsforecast["MW"] = Convert.ToDecimal(ChildNode.ChildNodes[5].InnerText);//10
                                        drFrozonloadsforecast["LoadForeCastTypeKey"] = 28;
                                        dtFrozonloadsforecast.Rows.Add(drFrozonloadsforecast);
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < dtFrozonloadsforecast.Rows.Count; i++)
                    {
 
                        if (VayuDbConn.State == ConnectionState.Open)
                        {
                            VayuDbConn.Close();
                        }
                        VayuDbConn.Open();
                        {
                            mInsertFrozonLoadForecast.Connection = VayuDbConn;
                            mInsertFrozonLoadForecast.Parameters["@MarketdateTime"].Value = dtFrozonloadsforecast.Rows[i]["Date"];
                            mInsertFrozonLoadForecast.Parameters["@MW"].Value = dtFrozonloadsforecast.Rows[i]["MW"];
                            mInsertFrozonLoadForecast.Parameters["@LoadForecastTypeKey"].Value = dtFrozonloadsforecast.Rows[i]["LoadForeCastTypeKey"];
                            mInsertFrozonLoadForecast.ExecuteNonQuery();
                        }
                        VayuDbConn.Close();
                    }
                    dtFrozonloadsforecast.Clear();
                    File.Delete(fileName);
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
