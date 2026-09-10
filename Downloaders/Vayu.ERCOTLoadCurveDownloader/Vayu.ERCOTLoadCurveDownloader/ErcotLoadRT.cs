using System;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Xml2CSharp;
using System.Threading;
using System.Timers;
using System.Globalization;
using Vayu.CommonAccessLibrary;

namespace Vayu.ERCOTLoadCurveDownloader
{
    public class ErcotLoadRT
    {
        private SqlConnection VayuDbConn;
        SqlCommand mInsertLoadRT;
        SqlCommand mUpdateLoadRT;
        DataTable dtloadsRT = new DataTable();
        DataRow drloadsRT;
        DateTime startdate = DateTime.Today;
        int Hour = DateTime.Now.Hour;
        decimal MW = 0;
        System.Timers.Timer mTimer = new System.Timers.Timer();
        SqlCommand selectRTCommand;
        X509Certificate2 ErcotCert = new X509Certificate2();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;

        void InitCert()
        {
            ErcotCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        
        public ErcotLoadRT()
        { 
            InitDB();
            MW = GetRealtimeValue(startdate.AddHours(Hour));
            Console.WriteLine("mw" + MW);
            
            if (MW == 0)
            {
                mTimer = new System.Timers.Timer();
                mTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
                mTimer.Interval = 5 * 60 * 1000;
                OnTimerEvent(null, null);
                mTimer.Start();
                Console.WriteLine("Wait for 5 min ...");
                while (true)
                {
                    Thread.Sleep(10 * 1000);
                }
            }
        }
        public void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            Console.WriteLine("Accesising DownloadRT Funcation");
            InitCert();
            DownloadRT();
            mTimer.Enabled = true;
        }
        public void InitDB()
        {
            //userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ErcotSubmission");
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");

            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            mUpdateLoadRT = new SqlCommand();
            mUpdateLoadRT.CommandText = "Update Vayu..LoadRT set MW=@MW where LoadsKey=@LoadsKey and MarketDateTime=@MarketDateTime and FileDateTime=@FileDateTime";
            mUpdateLoadRT.Parameters.AddWithValue("@LoadsKey", "LoadsKey");
            mUpdateLoadRT.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mUpdateLoadRT.Parameters.AddWithValue("@MW", "MW");
            mUpdateLoadRT.Parameters.AddWithValue("@FileDateTime", "FileDateTime");

            //
            mInsertLoadRT = new SqlCommand();
            mInsertLoadRT.CommandText = "insert into Vayu..LoadRT(LoadsKey, MarketDateTime, MW,FileDateTime) Values (@LoadsKey, @MarketDateTime, @MW, @FileDateTime)";
            mInsertLoadRT.Parameters.AddWithValue("@LoadsKey", "LoadsKey");
            mInsertLoadRT.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertLoadRT.Parameters.AddWithValue("@MW", "MW");
            mInsertLoadRT.Parameters.AddWithValue("@FileDateTime", "FileDateTime");
            //
            selectRTCommand = new SqlCommand();
            selectRTCommand.CommandText = "select MW from Vayu..LoadRT where MarketDatetime=@MarketDatetime";
            selectRTCommand.Parameters.AddWithValue("@MarketDatetime", "MarketDatetime");
            selectRTCommand.Connection = VayuDbConn;
            //
            dtloadsRT.Columns.Add("Date", typeof(DateTime));
            dtloadsRT.Columns.Add("LoadsKey", typeof(int));
            dtloadsRT.Columns.Add("MW", typeof(decimal));
            dtloadsRT.Columns.Add("FileDateTime", typeof(DateTime));
        }
        
        public decimal GetRealtimeValue(DateTime date)
        {
            decimal MW = 0;
            try
            {
                if (VayuDbConn.State == ConnectionState.Closed)
                    VayuDbConn.Open();
                selectRTCommand.Parameters["@MarketDatetime"].Value = date;
                SqlDataReader reader = selectRTCommand.ExecuteReader();
                while (reader.Read())
                {
                    MW = reader.IsDBNull(0) ? decimal.Zero : reader.GetDecimal(0);
                }
            }
            catch
            {
            }
            return MW;
        }
        public void DownloadRT()
        {
            Console.WriteLine("Accessing ERCOT  website");
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
            string FileName = @"D:\ISOFiles\ErcotLoadCurveDownloader\RT\" + fileName;
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
            
            string Newfilename = XML_UnZipFile(FileName, @"D:\ISOFiles\ErcotLoadCurveDownloader\RT\");
            if (Newfilename != "")
            {
                //string fileNameOnly = Path.GetFileNameWithoutExtension(Newfilename); // Extract file name without extension
                //int startIndex = fileNameOnly.LastIndexOf('.') + 1;
                //int endIndex = fileNameOnly.LastIndexOf('.');
                //string dateTimePart = fileNameOnly.Substring(startIndex, endIndex - startIndex);
                //DateTime fileNameDateTime = DateTime.ParseExact(dateTimePart, "yyyyMMdd.HHmmss", CultureInfo.InvariantCulture);
                string fileNameOnly = Path.GetFileNameWithoutExtension(Newfilename); // Extract file name without extension
                int startIndex = fileNameOnly.IndexOf('.') + 1; // Get index after the first dot
                startIndex = fileNameOnly.IndexOf('.', startIndex) + 1; // Get index after the second dot
                startIndex = fileNameOnly.IndexOf('.', startIndex) + 1; // Get index after the third dot
                int endIndex = fileNameOnly.LastIndexOf('.'); // Get index of the last dot
                string dateTimePart = fileNameOnly.Substring(startIndex, endIndex - startIndex);
                DateTime fileNameDateTime = DateTime.ParseExact(dateTimePart, "yyyyMMdd.HHmmss", CultureInfo.InvariantCulture);
                StreamReader readerFile = new StreamReader(Newfilename);
                string filestr = readerFile.ReadToEnd();
                readerFile.Close();
                #region New
                XmlSerializer serializer = new XmlSerializer(typeof(LoadForecastVsActuals));
                LoadForecastVsActuals objLoadForecastVsActuals;
                using (XmlReader reader = XmlReader.Create(Newfilename))
                {
                    objLoadForecastVsActuals = (LoadForecastVsActuals)serializer.Deserialize(reader);
                }
                int countt = objLoadForecastVsActuals.LFvsActualReport.Count;
                int hr = 0;
                DateTime dt = DateTime.Now;
                foreach (var item in objLoadForecastVsActuals.LFvsActualReport)
                {
                    //  if (StartDate == item.DeliveryDate)
                    {
                        hr = Convert.ToInt32(item.HourEnding);
                        dt = Convert.ToDateTime(item.DeliveryDate);
                        drloadsRT = dtloadsRT.NewRow();
                        drloadsRT["Date"] = dt.AddHours(hr);
                        drloadsRT["MW"] = Convert.ToDecimal(item.ActualLoad);
                        drloadsRT["FileDateTime"] = fileNameDateTime;
                        drloadsRT["LoadsKey"] = 2213;
                        dtloadsRT.Rows.Add(drloadsRT);
                        Console.WriteLine(countt);
                    }
                }

                for (int i = 0; i < dtloadsRT.Rows.Count; i++)
                {
                    mUpdateLoadRT.Connection = VayuDbConn;
                    if (VayuDbConn.State == ConnectionState.Open)
                    {
                        VayuDbConn.Close();
                    }
                    VayuDbConn.Open();
                    mUpdateLoadRT.Parameters["@MarketdateTime"].Value = dtloadsRT.Rows[i]["Date"];
                    mUpdateLoadRT.Parameters["@MW"].Value = dtloadsRT.Rows[i]["MW"];
                    mUpdateLoadRT.Parameters["@FileDateTime"].Value = dtloadsRT.Rows[i]["FileDateTime"];
                    mUpdateLoadRT.Parameters["@LoadsKey"].Value = dtloadsRT.Rows[i]["LoadsKey"];
                    if (Convert.ToDecimal(dtloadsRT.Rows[i]["MW"]) != 0)
                    {
                        int z = mUpdateLoadRT.ExecuteNonQuery();
                        if (z <= 0)
                        {
                            mInsertLoadRT.Connection = VayuDbConn;
                            mInsertLoadRT.Parameters["@MarketdateTime"].Value = dtloadsRT.Rows[i]["Date"];
                            mInsertLoadRT.Parameters["@MW"].Value = dtloadsRT.Rows[i]["MW"];
                            mInsertLoadRT.Parameters["@LoadsKey"].Value = dtloadsRT.Rows[i]["LoadsKey"];
                            mInsertLoadRT.Parameters["@FileDateTime"].Value = dtloadsRT.Rows[i]["FileDateTime"];
                            if (Convert.ToDecimal(dtloadsRT.Rows[i]["MW"]) != 0)
                            {
                                mInsertLoadRT.ExecuteNonQuery();
                                Console.WriteLine("inserting data");
                            }
                        }
                    }
                    VayuDbConn.Close();
                }
                dtloadsRT.Clear();
                #endregion New
                File.Delete(fileName);
                File.Delete(Newfilename);
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
