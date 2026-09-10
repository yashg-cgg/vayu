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
using System.Timers;
using System.Globalization;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotnodeDAlmpDownload
{
    class Ercot15MinsDownload
    {
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private SqlConnection VayuDbConn;
        private Dictionary<string, int> mNodeHash = new Dictionary<string, int>();
        private DataTable mDr15Min; 
        private SqlCommand mDeleteNodeHErcotCommand;
        private SqlCommand mSelectNodeCommand;
        private SqlCommand mSelectNodeHourCommand;
        private SqlCommand mDeleteCalHourlyCommand;
        private SqlCommand mDeleteLmphTmpCommand;
        DataTable mERCOTLoadResourcesDT = new DataTable();
        DataRow mERCOTLoadResourcesDT1;
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        DataTable mDtLmph = new DataTable();
        private SqlCommand mInsertNodeCommand;
        private X509Certificate2 mCert = new X509Certificate2();
        private SqlCommand mSelectMaxDateCommand;
        public Ercot15MinsDownload()
        {
            // mApplicationLog = new ApplicationLog.ApplicationLog(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name, "");
            InitDB();
            FillNodeHash();
             InitCert();
            
            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 5 * 60 * 1000;// 60000;
            mTimer.Enabled = true;
            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') ;
        }
        private void FillNodeHash()
        {
            mNodeHash = new Dictionary<string, int>();
            if (VayuDbConn.State == System.Data.ConnectionState.Open)
            {
                VayuDbConn.Close();
            }
            VayuDbConn.Open();
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = Convert.ToInt32(reader.GetValue(0));
                mNodeHash.Add(reader.GetString(1), nodekey);
            }
            reader.Close();
            VayuDbConn.Close();
        }

        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadLoadResources();
            mTimer.Enabled = true;
        }
        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");
            //mERCOTLoadResourcesDT.Columns.Add("NodeKey", typeof(int));
            //mERCOTLoadResourcesDT.Columns.Add("LMP", typeof(decimal));
            //mERCOTLoadResourcesDT.Columns.Add("Congestion", typeof(decimal));
            //mERCOTLoadResourcesDT.Columns.Add("Loss", typeof(decimal));
            //mERCOTLoadResourcesDT.Columns.Add("MarketDateTime", typeof(DateTime));
            //mERCOTLoadResourcesDT.Columns.Add("FinalYN", typeof(char));
            //
            mDeleteNodeHErcotCommand = new SqlCommand();
            mDeleteNodeHErcotCommand.CommandText = "Truncate table NodeLMPMerge";
            mDeleteNodeHErcotCommand.Connection = VayuDbConn;
            //
            mSelectMaxDateCommand = new SqlCommand();
            mSelectMaxDateCommand.CommandText = "select max(MarketDateTime) from NodeDALMPH (nolock) ";
            mSelectMaxDateCommand.Connection = VayuDbConn;

            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select NodeKey, nodename from Node where MarketKey=9";
            mSelectNodeCommand.Connection = VayuDbConn;

            //
            mSelectNodeHourCommand = new SqlCommand();
            mSelectNodeHourCommand.CommandText = "select nodekey, lmp from nodelmp where marketdatetime >= @start and marketdatetime < @end";
            mSelectNodeHourCommand.Parameters.AddWithValue("@start", "MarketDateTime");
            mSelectNodeHourCommand.Parameters.AddWithValue("@end", "MarketDateTime");
            mSelectNodeHourCommand.Connection = VayuDbConn;

            //
            mDeleteCalHourlyCommand = new SqlCommand();
            mDeleteCalHourlyCommand.CommandText = "truncate table NodeLmphtemp";
            mDeleteCalHourlyCommand.Connection = VayuDbConn;
            //
            //
            mDeleteLmphTmpCommand = new SqlCommand();
            mDeleteLmphTmpCommand.CommandText = "truncate table nodelmphtemp";
            mDeleteLmphTmpCommand.Connection = VayuDbConn;
            //

            mDtLmph.Columns.Add("NodeKey", typeof(int));
            mDtLmph.Columns.Add("LMP", typeof(double));
            mDtLmph.Columns.Add("Congestion", typeof(double));
            mDtLmph.Columns.Add("Loss", typeof(double));
            mDtLmph.Columns.Add("MarketDateTime", typeof(DateTime));
            mDtLmph.Columns.Add("LMPCalculated", typeof(double));
            mDtLmph.Columns.Add("CongestionCalculated", typeof(double));
            mDtLmph.Columns.Add("LossCalculated", typeof(double));
            mDtLmph.Columns.Add("FinalYN", typeof(char));
            //

            //mDr15Min = new DataTable();   mERCOTLoadResourcesDT
            mERCOTLoadResourcesDT.Columns.Add("Congestion", typeof(double));
            mERCOTLoadResourcesDT.Columns.Add("Loss", typeof(double));
            mERCOTLoadResourcesDT.Columns.Add("MarketDateTime", typeof(DateTime));
            mERCOTLoadResourcesDT.Columns.Add("NodeKey", typeof(int));
            mERCOTLoadResourcesDT.Columns.Add("LMP", typeof(double));
            mERCOTLoadResourcesDT.Columns.Add("FinalYN", typeof(char));
        }
        public void DownloadLoadResources()
        {
            try
            {
                mTimer.Enabled = false;
                //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Start Saving DA");
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=12301&noOfDaysofArchive=3&reportTitle=Settlement Point Prices at Resource Nodes, Hubs and Load Zones&showHTMLView=undefined&mimicKey");
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
                strnodedalmp = strnodedalmp.Substring(strnodedalmp.IndexOf("<b>Settlement Point Prices at Resource Nodes, Hubs and Load Zones</b></"));
                string[] splinelitRow = strnodedalmp.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 6; k < splinelitRow.Length; k = k + 6)
                {
                    sr.Close();
                    string FileURL = "https://mis.ercot.com/" + splinelitRow[k].Substring(10, 80);
                    string FileName = "D:\\ISOFiles\\ErcotnodeDAlmpDownload\\ERCOTDAResources\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                    FileDownloadRequest.ClientCertificates.Add(mCert);
                    FileDownloadRequest.CookieContainer = new CookieContainer();
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                    HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string fileDestName = FileName;
                    string folderFilename = @"D:\ISOFiles\ErcotnodeDAlmpDownload\ERCOTDAResources\";
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

                    VayuDbConn.Open();
                    DateTime? maxMarketDateTime = (DateTime?)mSelectMaxDateCommand.ExecuteScalar();
                    VayuDbConn.Close();
                    //if (marketDateTime >= DateTime.Parse("2019-05-15"))
                    //{
                    //    continue;
                    //}
                    //if ( marketDateTime < DateTime.Parse("2019-05-01"))
                    //{
                    //    continue;
                    //}
                    if (DateTime.Today == marketDateTime && maxMarketDateTime.Value.AddHours(-1) > marketDateTime.AddDays(1))
                    {
                        // mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "Exit");
                        Environment.Exit(0);
                    }
                    StreamReader XMLreader = new StreamReader(destinationFolderfilePaths[0]);
                    string filestr = XMLreader.ReadToEnd();
                    XMLreader.Close();
                    XmlDataDocument xmlParser = new XmlDataDocument();
                    xmlParser.LoadXml(filestr);
                    mERCOTLoadResourcesDT.Clear();
                    DateTime datehour = DateTime.Today;
                    foreach (XmlNode node in xmlParser.ChildNodes)
                    {
                        foreach (XmlNode childnode in node)
                        {
                            mERCOTLoadResourcesDT1 = mERCOTLoadResourcesDT.NewRow();
                            int nodeKey = 0;
                            if (!mNodeHash.ContainsKey((childnode.ChildNodes[3].InnerXml)))
                            {
                                continue;
                            }
                            nodeKey = mNodeHash[(childnode.ChildNodes[3].InnerXml)];
                            mERCOTLoadResourcesDT1["NodeKey"] = nodeKey;
                            if (childnode.ChildNodes[5].InnerXml == "")
                            {
                                mERCOTLoadResourcesDT1["LMP"] = DBNull.Value;
                            }
                            else
                            {
                                mERCOTLoadResourcesDT1["LMP"] = childnode.ChildNodes[5].InnerXml;
                            }
                            mERCOTLoadResourcesDT1["Congestion"] = 0;
                            mERCOTLoadResourcesDT1["Loss"] = 0;
                            mERCOTLoadResourcesDT1["FinalYN"] = "Y";
                            int hour = 0;
                            int min = 0;
                            DateTime date = DateTime.MaxValue;
                            if (Convert.ToInt32(childnode.ChildNodes[2].InnerXml) != 4)
                            {
                                hour = Convert.ToInt32(childnode.ChildNodes[1].InnerXml) - 1;
                                date = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml);//.AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                                datehour = date;
                            }
                            if (childnode.ChildNodes[2].InnerXml == "1")
                            {
                                min = 15;
                                hour = Convert.ToInt32(childnode.ChildNodes[1].InnerXml) - 1;
                                mERCOTLoadResourcesDT1["MarketDateTime"] = date.AddHours(hour).AddMinutes(min);//.AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                                datehour = date.AddHours(hour).AddMinutes(min);
                            }
                            if (childnode.ChildNodes[2].InnerXml == "2")
                            {
                                min = 30;
                                hour = Convert.ToInt32(childnode.ChildNodes[1].InnerXml) - 1;
                                mERCOTLoadResourcesDT1["MarketDateTime"] = date.AddHours(hour).AddMinutes(min);//.AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                                datehour = date.AddHours(hour).AddMinutes(min);
                            }
                            if (childnode.ChildNodes[2].InnerXml == "3")
                            {
                                min = 45;
                                hour = Convert.ToInt32(childnode.ChildNodes[1].InnerXml) - 1;
                                mERCOTLoadResourcesDT1["MarketDateTime"] = date.AddHours(hour).AddMinutes(min); //.AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                                datehour = date.AddHours(hour).AddMinutes(min); ;
                            }
                            if (childnode.ChildNodes[2].InnerXml == "4")
                            {
                                date = Convert.ToDateTime(childnode.ChildNodes[0].InnerXml);
                                min = 0;
                                hour = Convert.ToInt32(childnode.ChildNodes[1].InnerXml);
                                mERCOTLoadResourcesDT1["MarketDateTime"] = date.AddHours(hour).AddMinutes(min); //.AddHours(Convert.ToDouble(childnode.ChildNodes[1].InnerXml.Substring(0, 2)));
                                datehour = date.AddHours(hour).AddMinutes(min);
                            }
                            // mERCOTLoadResourcesDT1["FinalYN"] = "Y";
                            mERCOTLoadResourcesDT.Rows.Add(mERCOTLoadResourcesDT1);
                        }
                    }
                    if (VayuDbConn.State == ConnectionState.Open)
                    {
                        VayuDbConn.Close();
                    }
                    VayuDbConn.Open();
                    mDeleteNodeHErcotCommand.ExecuteNonQuery();
                    SqlTransaction transaction = VayuDbConn.BeginTransaction();
                    using (SqlBulkCopy bk15Min = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        try
                        {
                            //mDeleteLmpCommand.Transaction = transaction;
                            //mDeleteLmpCommand.Parameters["@marketdatetime"].Value = start;
                            //mDeleteLmpCommand.ExecuteNonQuery();
                            bk15Min.DestinationTableName = "[NodeLMPMerge]";
                            bk15Min.ColumnMappings.Add("LMP", "LMP");
                            bk15Min.ColumnMappings.Add("Congestion", "Congestion");
                            bk15Min.ColumnMappings.Add("Loss", "Loss");
                            bk15Min.ColumnMappings.Add("NodeKey", "NodeKey");
                            bk15Min.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                            bk15Min.ColumnMappings.Add("FinalYN", "FinalYN");
                            bk15Min.WriteToServer(mERCOTLoadResourcesDT);
                            SqlCommand updateNodeDALMPH = new SqlCommand("[UpMergeNodeLMP2]", VayuDbConn, transaction);
                            updateNodeDALMPH.CommandType = CommandType.StoredProcedure;
                            updateNodeDALMPH.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                        }
                    }
                    CalculateHourly(datehour);
                    File.Delete(FileName);
                    File.Delete(destinationFolderfilePaths[0]);
                    VayuDbConn.Close();
                    //   mApplicationLog.UpdateInfo(System.Reflection.MethodBase.GetCurrentMethod().Name, "End Saving DA");
                }
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
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            mSelectNodeHourCommand.Parameters["@start"].Value = hourDate.AddMinutes(15);
            mSelectNodeHourCommand.Parameters["@end"].Value = hourDate.AddHours(1).AddMinutes(15);
            SqlDataReader reader = mSelectNodeHourCommand.ExecuteReader();
            while (reader.Read())
            {
                List<double> lmpList = new List<double>();
                int nodeKey = Convert.ToInt32(reader.GetValue(0));
                double lmp = Convert.ToDouble(reader.GetValue(1));
                if (nodeHash.ContainsKey(nodeKey))
                {
                    lmpList = nodeHash[nodeKey];
                    nodeHash.Remove(nodeKey);
                }
                lmpList.Add(lmp);
                nodeHash.Add(nodeKey, lmpList);
            }
            reader.Close();
            VayuDbConn.Close();
            mDtLmph.Clear();
            List<int> keyList = nodeHash.Keys.ToList<int>();
            foreach (int key in keyList)
            {
                if (key == 57216)
                {

                }
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
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            SqlTransaction transaction;
            transaction = VayuDbConn.BeginTransaction();
            mDeleteCalHourlyCommand.Transaction = transaction;
            mDeleteCalHourlyCommand.ExecuteNonQuery();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
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
                    SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeNodeLMPH]", VayuDbConn, transaction);
                    updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                    updateNodeLmpMin.CommandTimeout = 300000;
                    updateNodeLmpMin.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    //   mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                    transaction.Rollback();
                    VayuDbConn.Close();
                }
            }
            VayuDbConn.Close();
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
