using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Timers;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotHourlyOutagesDownload
{
    class ErctHourlyOutagesDownload
    {
        SqlConnection Vayudbconn;

        X509Certificate2 ErcotCert = new X509Certificate2();

        //SqlCommand selectCommand; NodeKey INT, MarketDate DATETIME, Hour INT, SourceVolume NUMERIC(18,3), SinkVolume

        DataTable dtErcotHourlyOutages = new DataTable();
        System.Timers.Timer mTimer = new System.Timers.Timer();
        Dictionary<string, int> dictHashNode; // = new Dictionary<string, int>();

        void LoadDB()
        {
            Vayudbconn = new VayuDBConnection().GetInstance().GetSqlConnection();
            if (Vayudbconn.State == System.Data.ConnectionState.Closed)
            {
                Vayudbconn.Open();
            }
        }

        void AddColumnsToDT()
        {
            
            dtErcotHourlyOutages.Columns.Add("Date", typeof(DateTime));
            dtErcotHourlyOutages.Columns.Add("HourEnding", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalResourceMWZoneSouth", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalResourceMWZoneNorth", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalResourceMWZoneWest", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalResourceMWZoneHouston", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalIRRMWZoneSouth", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalIRRMWZoneNorth", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalIRRMWZoneWest", typeof(int));
            dtErcotHourlyOutages.Columns.Add("TotalIRRMWZoneHouston", typeof(int));
            
        }

        public ErctHourlyOutagesDownload()
        {
            
            LoadDB();
            //InitCert();
            mTimer = new System.Timers.Timer();
            mTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            mTimer.Interval = 20 * 60 * 1000;
            OnTimerEvent(null, null);
            mTimer.Start();
          //  while (true) ;
            
        }

        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DateTime today = DateTime.Today.AddDays(0);
            Download();
            mTimer.Enabled = true;
        }
        void InitCert()
        {
            SqlCommand selectUserPassCommand = Vayudbconn.CreateCommand();
            selectUserPassCommand.CommandText = " select CertificateName, Password from Vayu..Certificate where MarketKey = 9 and Application_Name = 'ProdClientAPI' ";
            //selectUserPassCommand.Parameters.AddWithValue("@Password", "Password");

            string certFileName = "D:\\ISOFiles\\ErcotTradedVolumesDownload\\Certificates\\Prod\\";
            string certPass = "";

            if (Vayudbconn.State == ConnectionState.Open)
            {
                Vayudbconn.Close();
            }
            Vayudbconn.Open();

            SqlDataReader rdr = selectUserPassCommand.ExecuteReader();
            while (rdr.Read())
            {
                certFileName = certFileName + rdr.GetString(0);
                certPass = rdr.GetString(1);
            }
            rdr.Close();
            Vayudbconn.Close();

            if (certPass == "")
            {
                return;
            }

            ErcotCert.Import(certFileName, certPass, X509KeyStorageFlags.MachineKeySet);
        }

        void FillHashNode()
        {
            dictHashNode = new Dictionary<string, int>();

            LoadDB();

            SqlCommand selectNodesCommand = Vayudbconn.CreateCommand();
            selectNodesCommand.CommandText = " select distinct nodekey, nodename from node where MarketKey = 9 ";

            if (Vayudbconn.State == ConnectionState.Open)
            {
                Vayudbconn.Close();
            }
            Vayudbconn.Open();

            SqlDataReader reader = selectNodesCommand.ExecuteReader();
            while (reader.Read())
            {
                dictHashNode.Add(reader.GetString(1), (int)reader.GetInt32(0));
            }
            reader.Close();
            Vayudbconn.Close();
        }

        void Download()
        {
            try
            {
                //string URL = "http://mis.ercot.com/misapp/GetReports.do?reportTypeId=13042&reportTitle=DAM%20PTP%20Obligation%20and%20Option%20Results%20by%20Settlement%20Point&showHTMLView=&mimicKey";
                string URL = "https://www.ercot.com/mp/data-products/data-product-details?id=NP3-233-CD";

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ClientCertificates.Add(ErcotCert);
                request.ServicePoint.ConnectionLimit = 1;
                request.Method = "GET";
                request.Timeout = 100000;
                HttpWebResponse FileDownloadResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(FileDownloadResponse.GetResponseStream(), Encoding.UTF8);
                string str = sr.ReadToEnd();
                str = str.ToString();
                sr.Close();

                //SPChange
                int s = str.IndexOf("var reportTypeID");
                string typeID = str.Substring(s + 20, 5);
                int l = str.IndexOf("var reportListUrl");
                string listURL = str.Substring(l + 21, 68);
                int m = str.IndexOf("var reportDownloadUrl");
                string downloadURL = str.Substring(m + 25, 67);
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
                string[] strArrayList = strListString.Split(new char[] { '\n' });
                //SpchangeEnd
                List<DateTime> dateTimes = new List<DateTime>();
                AddColumnsToDT();
                for (int i = strArrayList.Count()-1; i > 0; i--)
                {
                    string dataCsv = strArrayList[i];//DAMPTPOBLSPNP4194_csv.zip
                    if (dataCsv.Contains("HRLYRESOUTCAPNP3233_csv.zip"))
                    {
                        int dateIndex = dataCsv.IndexOf(".0000000000000000.");
                        string dateString = dataCsv.Substring(dateIndex + 18, 13);
                        string dateString1 = dataCsv.Substring(dateIndex + 18, 9);
                        dateString = dateString.Replace(".", "");
                        dateString1 = dateString1.Replace(".", "");
                        string formatString = "yyyyMMddHHmm";
                        string formatString1 = "yyyyMMdd";
                        DateTime filedate = DateTime.ParseExact(dateString, formatString, null);
                        DateTime filedate1 = DateTime.ParseExact(dateString1, formatString1, null);
                        
                        
                            if (filedate1 > DateTime.Today.AddDays(-10))
                            {
                                int dockIndex = dataCsv.IndexOf("DocID");
                                string dockID = dataCsv.Substring(dockIndex + 8, 10);
                                string FileName = "D:\\ISOFiles\\ErcotHourlyOutages\\isodata\\Ercot_" + dataCsv.Substring(71, 9) + ".zip";
                                HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(downloadURL + dockID);
                                FileDownloadRequest.Timeout = 100000;
                                FileDownloadRequest.Method = "GET";
                                HttpWebResponse FileDownloadresponse1 = (HttpWebResponse)FileDownloadRequest.GetResponse();
                                string headers = FileDownloadresponse1.Headers["Content-Disposition"];
                                if (headers != null)
                                {
                                    headers = headers.Replace("attachment; filename=", "");
                                    string name = headers.Replace("_csv_zip", ".zip");
                                    string compare = name.Substring(0, 41);
                                    string comparision = headers.Replace(".", "_");
                                    comparision = comparision.Replace("_csv_zip", ".csv");

                                    string fileDestName = @"D:\\ISOFiles\\ErcotHourlyOutages\\ERCOTTesting\\HourlyOutages\\" + name;
                                    string filedestName = @"D:\\ISOFiles\\ErcotHourlyOutages\\ERCOTTesting\HourlyOutages\\Imported\\" + name;
                                    string folderFilename = @"D:\\ISOFiles\\ErcotHourlyOutages\ISOData\\";
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

                                    string returnrtfilename = CSVUnZipFile(fileDestName, folderFilename);

                                    

                                    using (StreamReader streamReader = new StreamReader(returnrtfilename))
                                    {
                                        dtErcotHourlyOutages.Clear();

                                        string data = streamReader.ReadToEnd();
                                        data = data.Replace("\"", "");
                                        data = data.Replace("\r", "");
                                        data = data.Trim();
                                        string[] split1 = data.Split(new char[] { '\n' });

                                        for (int k = 1; k < split1.Length; k++)
                                        {
                                            string[] split2 = split1[k].Split(',');
                                            //int count = 1;
                                            string MarketDate = split2[0].Trim();

                                            string Hour = split2[1].Trim();


                                            DateTime MarketDateTime = Convert.ToDateTime(MarketDate);
                                            DataRow dr = dtErcotHourlyOutages.NewRow();
                                            dr["Date"] = MarketDateTime;
                                            dr["HourEnding"] = Hour;
                                            dr["TotalResourceMWZoneSouth"] = split2[2].Trim();
                                            dr["TotalResourceMWZoneNorth"] = split2[3].Trim();
                                            dr["TotalResourceMWZoneWest"] = split2[4].Trim();
                                            dr["TotalResourceMWZoneHouston"] = split2[5].Trim();
                                            dr["TotalIRRMWZoneSouth"] = split2[6].Trim();
                                            dr["TotalIRRMWZoneNorth"] = split2[7].Trim();
                                            dr["TotalIRRMWZoneWest"] = split2[8].Trim();
                                            dr["TotalIRRMWZoneHouston"] = split2[9].Trim();

                                            dtErcotHourlyOutages.Rows.Add(dr);

                                        }
                                    }

                                    if (Vayudbconn.State == ConnectionState.Open)
                                    {
                                        Vayudbconn.Close();
                                    }
                                    Vayudbconn.Open();
                                    SqlTransaction transaction = Vayudbconn.BeginTransaction();
                                    using (SqlBulkCopy bkTradedVolume = new SqlBulkCopy(Vayudbconn, SqlBulkCopyOptions.TableLock, transaction))
                                    {
                                        try
                                        {
                                            bkTradedVolume.DestinationTableName = "dbo.HourlyoutagesTest";
                                            bkTradedVolume.ColumnMappings.Add("Date", "Date");
                                            bkTradedVolume.ColumnMappings.Add("HourEnding", "HourEnding");
                                            bkTradedVolume.ColumnMappings.Add("TotalResourceMWZoneSouth", "TotalResourceMWZoneSouth");
                                            bkTradedVolume.ColumnMappings.Add("TotalResourceMWZoneNorth", "TotalResourceMWZoneNorth");
                                            bkTradedVolume.ColumnMappings.Add("TotalResourceMWZoneWest", "TotalResourceMWZoneWest");
                                            bkTradedVolume.ColumnMappings.Add("TotalResourceMWZoneHouston", "TotalResourceMWZoneHouston");
                                            bkTradedVolume.ColumnMappings.Add("TotalIRRMWZoneSouth", "TotalIRRMWZoneSouth");
                                            bkTradedVolume.ColumnMappings.Add("TotalIRRMWZoneNorth", "TotalIRRMWZoneNorth");
                                            bkTradedVolume.ColumnMappings.Add("TotalIRRMWZoneWest", "TotalIRRMWZoneWest");
                                            bkTradedVolume.ColumnMappings.Add("TotalIRRMWZoneHouston", "TotalIRRMWZoneHouston");
                                            bkTradedVolume.WriteToServer(dtErcotHourlyOutages);
                                            SqlCommand updateTradedVolume = new SqlCommand("dbo.UpMergeErcotHourlyoutages", Vayudbconn, transaction);
                                            updateTradedVolume.CommandType = CommandType.StoredProcedure;
                                            updateTradedVolume.CommandTimeout = 30000;
                                            updateTradedVolume.ExecuteNonQuery();
                                            transaction.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            transaction.Rollback();
                                        }
                                    }

                                    Vayudbconn.Close();
                                    File.Delete(fileDestName);
                                    File.Delete(returnrtfilename);

                                }
                            Console.WriteLine("Data inserted for " + filedate);

                            }
                        
                    }
                }


            }
            catch (Exception ex)
            {
                
            }
        }

        public static string CSVUnZipFile(string InputPathOfZipFile, string FolderFilename)
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
    }
}
