using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Data.SqlTypes;
using Vayu.CertificateInfoLibrary;

using System.Timers;
using System.Globalization;
namespace Vayu.ErcotnodeDAlmpDownload
{
    class FiveMinsLmpDownload
    {
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private SqlConnection VayuDbConnErcot;
        private SqlConnection VayuDbConn;
        private Dictionary<string, int> mNodeHash = new Dictionary<string, int>();
        private DataTable mDr15Min;
        private DataTable mDtLmph = new DataTable();
        // private static ApplicationLog.ApplicationLog mApplicationLog;
        private SqlCommand mDeleteNodeHErcotCommand;
        private SqlCommand mSelectNodeCommand;
        private SqlCommand mSelectNodeHourCommand;
        DataTable mERCOTLoadResourcesDT = new DataTable();
        DataRow mERCOTLoadResourcesDT1;
        private SqlCommand mInsertNodeCommand;
        private X509Certificate2 mCert = new X509Certificate2();
        private SqlCommand mSelectMaxDateCommand;
        private SqlCommand mDeleteCalHourlyCommand;
        private SqlCommand mDeleteLmphTmpCommand;
        public FiveMinsLmpDownload()
        {
            // mApplicationLog = new ApplicationLog.ApplicationLog(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name, "");
            InitDB();
            FillNodeHash();

            CertificateInfoLibrary.CertificateHeler certDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetrails(9,"ProdClientAPI");
            InitCert(certDetails);
            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 5 * 60 * 1000; //60000;
            mTimer.Enabled = true;
            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') ;
        }
        private void FillNodeHash()
        {
            mNodeHash = new Dictionary<string, int>();
            if (VayuDbConnErcot.State == System.Data.ConnectionState.Open)
            {
                VayuDbConnErcot.Close();
            }
            VayuDbConnErcot.Open();
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = Convert.ToInt32(reader.GetValue(0));
                mNodeHash.Add(reader.GetString(1), nodekey);
            }
            reader.Close();
            VayuDbConnErcot.Close();
        }
        public void InitCert(Vayu.CertificateInfoLibrary.CertificateHeler certDetails)
        {

            mCert.Import(certDetails.Path, certDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadLoadResources();
            mTimer.Enabled = true;
        }
        public void InitDB()
        {
            VayuDbConnErcot = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            VayuDbConn = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            //

            mERCOTLoadResourcesDT.Columns.Add("nodeKey", typeof(int));
            mERCOTLoadResourcesDT.Columns.Add("Lmp", typeof(double));
            mERCOTLoadResourcesDT.Columns.Add("MarketDate", typeof(DateTime));
            mERCOTLoadResourcesDT.Columns.Add("MarketHour", typeof(int));
            mERCOTLoadResourcesDT.Columns.Add("MarketMin", typeof(Int32));
            mERCOTLoadResourcesDT.Columns.Add("Second", typeof(Int32));
            //
            mSelectNodeHourCommand = new SqlCommand();
            mSelectNodeHourCommand.CommandText = "select nodekey, lmp from nodelmp where marketdatetime >= @start and marketdatetime < @end";
            mSelectNodeHourCommand.Parameters.AddWithValue("@start", "MarketDateTime");
            mSelectNodeHourCommand.Parameters.AddWithValue("@end", "MarketDateTime");
            mSelectNodeHourCommand.Connection = VayuDbConnErcot;
            //
            mDeleteNodeHErcotCommand = new SqlCommand();
            mDeleteNodeHErcotCommand.CommandText = "Truncate table NodeLmpMinMerge";
            mDeleteNodeHErcotCommand.Connection = VayuDbConnErcot;
            //
            mSelectMaxDateCommand = new SqlCommand();
            mSelectMaxDateCommand.CommandText = "select max(MarketDateTime) from NodeDALMPH (nolock) ";
            mSelectMaxDateCommand.Connection = VayuDbConnErcot;

            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select NodeKey, nodename from Node where MarketKey=9";
            mSelectNodeCommand.Connection = VayuDbConnErcot;

            //

            mDeleteLmphTmpCommand = new SqlCommand();
            mDeleteLmphTmpCommand.CommandText = "truncate table nodelmphtemp";
            mDeleteLmphTmpCommand.Connection = VayuDbConnErcot;
            //

            mDeleteCalHourlyCommand = new SqlCommand();
            mDeleteCalHourlyCommand.CommandText = "truncate table NodeLmphtemp";
            mDeleteCalHourlyCommand.Connection = VayuDbConnErcot;
            //

            //mDr15Min = new DataTable();
            //mERCOTLoadResourcesDT.Columns.Add("NodeKey");
            //mERCOTLoadResourcesDT.Columns.Add("LMP");
            //mERCOTLoadResourcesDT.Columns.Add("Congestion");
            //mERCOTLoadResourcesDT.Columns.Add("Loss");
            //mERCOTLoadResourcesDT.Columns.Add("FinalYN");
        }
        public void DownloadLoadResources()
        {
            try
            {
                mTimer.Enabled = false;
                //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Start Saving DA");
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\Ercot 5 Min\");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
                string uri = "https://mis.ercot.com/misapp/GetReports.do?reportTypeId=12300&noOfDaysofArchive=3&reportTitle=LMPs by Resource Nodes, Load Zones and Trading Hubs&showHTMLView=undefined&mimicKey";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.CookieContainer = new CookieContainer();
                request.ServicePoint.ConnectionLimit = 1;
                request.Method = "GET";
                request.ClientCertificates.Add(mCert);
                request.Timeout = 100000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string strnodedalmp = sr.ReadToEnd();
                strnodedalmp = strnodedalmp.Replace("\r\n", "").Trim();
                strnodedalmp = strnodedalmp.Substring(strnodedalmp.IndexOf("<b>LMPs by Resource Nodes, Load Zones and Trading Hubs</b></"));
                string[] splinelitRow = strnodedalmp.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 6; k < splinelitRow.Length; k = k + 6)
                {
                    sr.Close();
                    string FileURL = "https://mis.ercot.com/" + splinelitRow[k].Substring(10, 79);
                    string FileName = "D:\\ISOFiles\\ErcotnodeDAlmpDownload\\ERCOTDAResources\\Ercot 5 Min\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                    FileDownloadRequest.ClientCertificates.Add(mCert);
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                    HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string fileDestName = FileName;
                    string folderFilename = @"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\Ercot 5 Min\";
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
                    string[] fileDate = destinationFolderfilePaths[0].Split('.');
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    string format = "yyyyMMdd";
                    DateTime marketDateTime = DateTime.ParseExact(fileDate[3].ToString(), format, provider);
                    VayuDbConnErcot.Open();
                    DateTime? maxMarketDateTime = (DateTime?)mSelectMaxDateCommand.ExecuteScalar();
                    VayuDbConnErcot.Close();
                    //if (DateTime.Today == marketDateTime && maxMarketDateTime.Value.AddHours(-1) > marketDateTime.AddDays(1))
                    //{
                    //    // mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Exit");
                    //    Environment.Exit(0);
                    //}
                    if (marketDateTime >= DateTime.Parse("2019-05-02"))
                    {
                        // continue;
                    }
                    if (marketDateTime < DateTime.Parse("2019-05-01"))
                    {
                        // continue;
                    }

                    StreamReader XMLreader = new StreamReader(destinationFolderfilePaths[0]);
                    string filestr = XMLreader.ReadToEnd();
                    XMLreader.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(filestr);
                    mERCOTLoadResourcesDT.Clear();
                    foreach (XmlNode node in xmlParser.ChildNodes)
                    {
                        foreach (XmlNode childnode in node)
                        {
                            mERCOTLoadResourcesDT1 = mERCOTLoadResourcesDT.NewRow();
                            int nodeKey = 0;
                            string data = childnode.ChildNodes[0].InnerText;

                            DateTime dtMarketDate = Convert.ToDateTime(childnode.ChildNodes[0].InnerText);
                            mERCOTLoadResourcesDT1["MarketDate"] = dtMarketDate.Date;
                            mERCOTLoadResourcesDT1["MarketHour"] = dtMarketDate.Hour;
                            mERCOTLoadResourcesDT1["MarketMin"] = dtMarketDate.Minute;
                            mERCOTLoadResourcesDT1["Second"] = dtMarketDate.Second;
                            if (childnode.ChildNodes[3].InnerXml == "")
                            {
                                mERCOTLoadResourcesDT1["LMP"] = DBNull.Value;
                            }
                            else
                            {
                                mERCOTLoadResourcesDT1["LMP"] = childnode.ChildNodes[3].InnerXml;
                            }
                            if (mNodeHash.ContainsKey(childnode.ChildNodes[2].InnerXml))
                            {
                                mERCOTLoadResourcesDT1["nodeKey"] = mNodeHash[childnode.ChildNodes[2].InnerXml];
                            }



                            //if (!mNodeHash.ContainsKey((childnode.ChildNodes[2].InnerXml)))
                            //{
                            //    continue;
                            //}
                            //nodeKey = mNodeHash[(childnode.ChildNodes[2].InnerXml)];
                            //mERCOTLoadResourcesDT1["NodeKey"] = nodeKey;
                            //if (childnode.ChildNodes[3].InnerXml == "")
                            //{
                            //    mERCOTLoadResourcesDT1["LMP"] = DBNull.Value;
                            //}
                            //else
                            //{
                            //    mERCOTLoadResourcesDT1["LMP"] = childnode.ChildNodes[3].InnerXml;
                            //}
                            //mERCOTLoadResourcesDT1["Congestion"] = DBNull.Value;
                            //mERCOTLoadResourcesDT1["Loss"] = DBNull.Value;
                            //mERCOTLoadResourcesDT1["MarketDateTime"] = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml);//.AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));

                            //mERCOTLoadResourcesDT1["FinalYN"] = childnode.ChildNodes[1].InnerXml; ;
                            mERCOTLoadResourcesDT.Rows.Add(mERCOTLoadResourcesDT1);
                        }
                    }
                    if (VayuDbConnErcot.State == ConnectionState.Open)
                    {
                        VayuDbConnErcot.Close();
                    }
                    VayuDbConnErcot.Open();
                    mDeleteNodeHErcotCommand.ExecuteNonQuery();
                    SqlTransaction transaction = VayuDbConnErcot.BeginTransaction();
                    using (SqlBulkCopy bkLmp = new SqlBulkCopy(VayuDbConnErcot, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        try
                        {
                            bkLmp.DestinationTableName = "[NodeLmpMinMerge]";
                            bkLmp.ColumnMappings.Add("NodeKey", "NodeKey");
                            bkLmp.ColumnMappings.Add("LMP", "LMP");
                            bkLmp.ColumnMappings.Add("MarketDate", "MarketDate");
                            bkLmp.ColumnMappings.Add("MarketHour", "MarketHour");
                            bkLmp.ColumnMappings.Add("MarketMin", "MarketMin");
                            bkLmp.ColumnMappings.Add("Second", "Second");
                            bkLmp.WriteToServer(mERCOTLoadResourcesDT);
                            SqlCommand updateNodeLMP = new SqlCommand("[UpMergeNodeLMPMinDown]", VayuDbConnErcot, transaction);
                            updateNodeLMP.CommandType = CommandType.StoredProcedure;
                            updateNodeLMP.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                        }
                    }
                    File.Delete(FileName);
                    File.Delete(destinationFolderfilePaths[0]);
                    VayuDbConnErcot.Close();
                    //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "End Saving DA");
                }
                //foreach (DateTime date in marketDateList)
                //{
                //    CalculateHourly(date);
                //}
            }
            catch (Exception ex)
            {
                //  mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            mTimer.Enabled = true;
        }

        private void CalculateHourly(DateTime hourDate)
        {
            //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Date=" + hourDate.ToShortDateString());
            if (hourDate.Minute == 0)
            {
                hourDate = hourDate.AddHours(-1);
            }
            hourDate = DateTime.Parse(hourDate.Year + "/" + hourDate.Month + "/" + hourDate.Day + " " + hourDate.Hour + ":00");
            Dictionary<int, List<double>> nodeHash = new Dictionary<int, List<double>>();
            if (VayuDbConnErcot.State == ConnectionState.Closed)
            {
                VayuDbConnErcot.Open();
            }
            mSelectNodeHourCommand.Parameters["@start"].Value = hourDate.AddMinutes(15);
            mSelectNodeHourCommand.Parameters["@end"].Value = hourDate.AddHours(1).AddMinutes(15);
            SqlDataReader reader = mSelectNodeHourCommand.ExecuteReader();
            while (reader.Read())
            {
                List<double> lmpList = new List<double>();
                int nodeKey = (int)reader.GetDecimal(0);
                double lmp = (double)reader.GetDecimal(1);
                if (nodeHash.ContainsKey(nodeKey))
                {
                    lmpList = nodeHash[nodeKey];
                    nodeHash.Remove(nodeKey);
                }
                lmpList.Add(lmp);
                nodeHash.Add(nodeKey, lmpList);
            }
            reader.Close();
            VayuDbConnErcot.Close();
            mDtLmph.Clear();
            List<int> keyList = nodeHash.Keys.ToList<int>();
            foreach (int key in keyList)
            {
                List<double> lmpList = nodeHash[key];
                double lmpH = 0;
                foreach (double lmp in lmpList)
                {
                    lmpH += lmp;
                }
                lmpH = lmpH / (double)lmpList.Count;
                DataRow drLmph = mDtLmph.NewRow();
                drLmph["NodeKey"] = key;
                drLmph["LMP"] = lmpH;
                drLmph["Congestion"] = 0;
                drLmph["Loss"] = 0;
                drLmph["MarketDateTime"] = hourDate.AddHours(1);
                drLmph["LMPCalculated"] = lmpH;
                drLmph["CongestionCalculated"] = 0;
                drLmph["LossCalculated"] = 0;
                drLmph["FinalYN"] = "Y";
                mDtLmph.Rows.Add(drLmph);
            }
            if (VayuDbConnErcot.State == ConnectionState.Closed)
            {
                VayuDbConnErcot.Open();
            }
            SqlTransaction transaction;
            transaction = VayuDbConnErcot.BeginTransaction();
            mDeleteCalHourlyCommand.Transaction = transaction;
            mDeleteCalHourlyCommand.ExecuteNonQuery();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConnErcot, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {

                    bkLmpH.DestinationTableName = "[NodeLmphtemp]";
                    mDeleteLmphTmpCommand.Transaction = transaction;
                    mDeleteLmphTmpCommand.ExecuteNonQuery();
                    bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                    bkLmpH.ColumnMappings.Add("LMP", "LMP");
                    bkLmpH.ColumnMappings.Add("Congestion", "Congestion");
                    bkLmpH.ColumnMappings.Add("Loss", "Loss");
                    bkLmpH.ColumnMappings.Add("LMPCalculated", "LMPCalculated");
                    bkLmpH.ColumnMappings.Add("CongestionCalculated", "CongestionCalculated");
                    bkLmpH.ColumnMappings.Add("LossCalculated", "LossCalculated");
                    bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                    bkLmpH.ColumnMappings.Add("FinalYN", "FinalYN");
                    bkLmpH.WriteToServer(mDtLmph);
                    SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeNodeLMPH]", VayuDbConnErcot, transaction);
                    updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                    updateNodeLmpMin.CommandTimeout = 300000;
                    updateNodeLmpMin.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    //   mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                    transaction.Rollback();
                    VayuDbConnErcot.Close();
                }
            }
            VayuDbConnErcot.Close();
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
