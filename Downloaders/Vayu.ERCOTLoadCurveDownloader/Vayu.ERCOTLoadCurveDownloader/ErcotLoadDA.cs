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

namespace Vayu.ERCOTLoadCurveDownloader
{
    class ErcotLoadDA
    {
        private SqlConnection VayuDbConn;
        SqlCommand mInsertLoadDA;
        SqlCommand mUpdateLoadDA;
        DataTable dtloadsDA = new DataTable();
        DataRow drloadsDA;
        X509Certificate2 ErcotCert = new X509Certificate2();
        public ErcotLoadDA()
        {
            InitDB();
            DownloadDA();
        }
        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            mUpdateLoadDA = new SqlCommand();
            mUpdateLoadDA.CommandText = "Update Vayu..dayaheadload set Value=@MW where Name=@Name  and MarketDateTime=@MarketDateTime and Marketkey=@Marketkey";//dayaheadload   dayaheadload_temp
            mUpdateLoadDA.Parameters.AddWithValue("@Name", "Name");
            mUpdateLoadDA.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mUpdateLoadDA.Parameters.AddWithValue("@MW", "MW");
            mUpdateLoadDA.Parameters.AddWithValue("@Marketkey", "Marketkey");
            //
            mInsertLoadDA = new SqlCommand();
            mInsertLoadDA.CommandText = "insert into Vayu..dayaheadload(Name,Value, MarketDateTime,Marketkey) Values (@Name,@MW, @MarketDateTime,@Marketkey)";//dayaheadload
            mInsertLoadDA.Parameters.AddWithValue("@Name", "Name");
            mInsertLoadDA.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertLoadDA.Parameters.AddWithValue("@MW", "MW");
            mInsertLoadDA.Parameters.AddWithValue("@Marketkey", "Marketkey");

            //
            dtloadsDA.Columns.Add("Date", typeof(DateTime));
            dtloadsDA.Columns.Add("Name", typeof(string));
            dtloadsDA.Columns.Add("MW", typeof(decimal));
            dtloadsDA.Columns.Add("Marketkey", typeof(int));
        }
        
        public void DownloadDA()
        {
            Console.WriteLine("Accessing website");
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.ercot.com/mp/data-products/data-product-details?id=GEN-55-CD");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.ercot.com/misapp/servlets/IceDocListJsonWS?reportTypeId=13499");
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
            string[] splinelitRow1 = str.Split(new string[] { "DocID" }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            foreach (var item in splinelitRow1)
            {
                if (count < splinelitRow1.Count() - 60)
                {
                    if (item.Contains("HOURLYLFvsACTUALGEN55_csv"))
                    {
                        string[] split = item.Split(':');
                        string[] stringSeparators = new string[] { "\"" };
                        string[] result = split[1].Split(stringSeparators, StringSplitOptions.None);
                        string[] splitslash = result[1].Split(',');
                        string[] resultfile = split[15].Split(stringSeparators, StringSplitOptions.None);
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

        private void DownloadXML(string reportDownloadUrl, string fileName)
        {
            string FileName = @"D:\ISOFiles\ErcotLoadCurveDownloader\DA\" + fileName;
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
            int hr; DateTime dt;
            string Newfilename = XML_UnZipFile(FileName, @"D:\ISOFiles\ErcotLoadCurveDownloader\DA");
            string StartDate = System.DateTime.Now.ToString("MM/dd/yyyy");
            #region New
            if (Newfilename != "")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LoadForecastVsActuals));
                LoadForecastVsActuals objLoadForecastVsActuals;
                using (XmlReader reader = XmlReader.Create(Newfilename))
                {
                    objLoadForecastVsActuals = (LoadForecastVsActuals)serializer.Deserialize(reader);
                }
                int countt = objLoadForecastVsActuals.LFvsActualReport.Count;
                foreach (var item in objLoadForecastVsActuals.LFvsActualReport)
                {
                    if (StartDate == item.DeliveryDate)
                    {
                        hr = Convert.ToInt32(item.HourEnding);
                        dt = Convert.ToDateTime(item.DeliveryDate);
                        drloadsDA = dtloadsDA.NewRow();
                        drloadsDA["Date"] = dt.AddHours(hr);
                        drloadsDA["MW"] = Convert.ToDecimal(item.DayAheadForecast);
                        drloadsDA["Name"] = "Total";
                        drloadsDA["Marketkey"] = 9;
                        dtloadsDA.Rows.Add(drloadsDA);
                    }
                }

                for (int i = 0; i < dtloadsDA.Rows.Count; i++)
                {
                    mUpdateLoadDA.Connection = VayuDbConn;
                    if (VayuDbConn.State == ConnectionState.Open)
                    {
                        VayuDbConn.Close();
                    }
                    VayuDbConn.Open();
                    mUpdateLoadDA.Parameters["@MarketdateTime"].Value = dtloadsDA.Rows[i]["Date"];
                    mUpdateLoadDA.Parameters["@MW"].Value = dtloadsDA.Rows[i]["MW"];
                    mUpdateLoadDA.Parameters["@Name"].Value = dtloadsDA.Rows[i]["Name"];
                    mUpdateLoadDA.Parameters["@Marketkey"].Value = dtloadsDA.Rows[i]["Marketkey"];
                    if (Convert.ToDecimal(dtloadsDA.Rows[i]["MW"]) != 0)
                    {
                        int z = mUpdateLoadDA.ExecuteNonQuery();
                        if (z <= 0)
                        {
                            mInsertLoadDA.Connection = VayuDbConn;
                            mInsertLoadDA.Parameters["@MarketdateTime"].Value = dtloadsDA.Rows[i]["Date"];
                            mInsertLoadDA.Parameters["@MW"].Value = dtloadsDA.Rows[i]["MW"];
                            mInsertLoadDA.Parameters["@Name"].Value = dtloadsDA.Rows[i]["Name"];
                            mInsertLoadDA.Parameters["@Marketkey"].Value = dtloadsDA.Rows[i]["Marketkey"];
                            if (Convert.ToDecimal(dtloadsDA.Rows[i]["MW"]) != 0)
                            {
                                mInsertLoadDA.ExecuteNonQuery();
                            }
                        }
                    }
                    VayuDbConn.Close();
                }
            }
            dtloadsDA.Clear();
            #endregion New
            File.Delete(FileName);
            File.Delete(Newfilename);
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
