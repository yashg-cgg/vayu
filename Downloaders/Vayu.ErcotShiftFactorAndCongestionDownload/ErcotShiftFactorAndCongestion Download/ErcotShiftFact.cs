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
using System.Timers;
using System.Globalization;
using System.Threading;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotShiftFactorAndCongestionDownload
{
    public class ErcotShftFact
    {
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private SqlConnection SigmaDbConnErcot;
        private SqlConnection SigmaDbConn;
        private Dictionary<string, int> mNodeHash = new Dictionary<string, int>();
        private SqlCommand mDeleteNodeHErcotCommand;
        private SqlCommand mSelectMasterCommand;
        private SqlCommand mSelectMasterRTCommand;
        private SqlCommand mSelectConstraintCommand;
        private SqlCommand mSelectNodeCommand;
        private SqlCommand mDeleteErcotShftFactCommand;
        DataTable mERCOTLoadResourcesDT = new DataTable();
        DataTable mErcotShiftFactorLoadResourcesDT = new DataTable();
        DateTime mDate = DateTime.Today;
        private SqlCommand mInsertConstraintCommand;
        private X509Certificate2 mCert = new X509Certificate2();
        private SqlCommand mSelectMaxDateCommand;
        private SqlCommand mDeleteErcotVectorCommand;
        private SqlCommand mInsertErcotVectorCommand;
        private SqlCommand mSelectMaxMinCommand;
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        DataTable mDtErcotShftFact = new DataTable();
        Dictionary<DateTime, MinMax> dictMinMaxDict = new Dictionary<DateTime, MinMax>();
        Dictionary<DateTime, List<ConstraintHelper>> mConstraintDict = new Dictionary<DateTime, List<ConstraintHelper>>();
        SqlCommand deleteHourlyImpactCommand;
        SqlCommand insertHourlyImpactCommand;
        SqlCommand updateHourlyImpactCommand;
        public ErcotShftFact()
        {
            InitDB();
            FillNodeHash();
            InitCert();
            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 1 * 60 * 1000;
            mTimer.Enabled = true;
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
            Console.WriteLine("Press \'q\' to quit.");
        }
        private void FillNodeHash()
        {
            mNodeHash = new Dictionary<string, int>();
            if (SigmaDbConn.State == System.Data.ConnectionState.Open)
            {
                SigmaDbConn.Close();
            }
            SigmaDbConn.Open();
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = Convert.ToInt32(reader.GetValue(0));
                mNodeHash.Add(reader.GetString(1), nodekey);
            }
            reader.Close();
            SigmaDbConn.Close();
        }
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadSensitivity();
            SaveHourlyImpact(DateTime.Today.AddDays(-2), DateTime.Today.AddDays(1));
            mTimer.Enabled = true;
        }
        public void InitDB()
        {
            SigmaDbConnErcot = new VayuDBConnection().GetInstance().GetSqlConnection();
            SigmaDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");
            //
            mDeleteNodeHErcotCommand = new SqlCommand();
            mDeleteNodeHErcotCommand.CommandText = "Truncate table NodeLmpMinMerge";
            mDeleteNodeHErcotCommand.Connection = SigmaDbConnErcot;
            //
            mSelectMaxDateCommand = new SqlCommand();
            mSelectMaxDateCommand.CommandText = "select max(MarketDateTime) from ConstraintRT (nolock) ";
            mSelectMaxDateCommand.Connection = SigmaDbConnErcot;
            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select NodeKey, nodename from Vayu..Node where MarketKey=9";
            mSelectNodeCommand.Connection = SigmaDbConn;
            //mDr15Min = new DataTable();
            mERCOTLoadResourcesDT.Columns.Add("NodeKey");
            mERCOTLoadResourcesDT.Columns.Add("LMP");
            mERCOTLoadResourcesDT.Columns.Add("MarketDate");
            mERCOTLoadResourcesDT.Columns.Add("MarketHour");
            mERCOTLoadResourcesDT.Columns.Add("MarketMin");
            //
            mSelectMasterCommand = new SqlCommand();
            mSelectMasterCommand.Parameters.AddWithValue("@sdate", "MarketDateTime");
            mSelectMasterCommand.Parameters.AddWithValue("@edate", "MarketDateTime");
            //mSelectMasterCommand.CommandText = "select b.ConstraintRTNum from Vayu..ConstraintRT a join rtmasterconstraint b on "+
            //   " a.ConstraintText = b.MonitoredText and a.ContingencyText = b.ContingencyText " 
            //+"where a.MarketDateTime > @sdate and a.MarketDateTime <= @edate and a.ConstraintText like '@ConstraintText%'  and a.contingencytext = @contingencytext ";
            //mSelectMasterCommand.Parameters.AddWithValue("@sdate", "MarketDateTime");
            //mSelectMasterCommand.Parameters.AddWithValue("@edate", "MarketDateTime");
            //mSelectMasterCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
            //mSelectMasterCommand.Parameters.AddWithValue("@ConstraintText", "ConstraintText");
            //mSelectMasterCommand.Connection = SigmaDbConnErcot;
            //
            mDeleteErcotVectorCommand = new SqlCommand();
            mDeleteErcotVectorCommand.CommandText = "delete rtmastervector where constraintrtnum = @constraintrtnum and nodekey = @nodekey";
            mDeleteErcotVectorCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mDeleteErcotVectorCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mDeleteErcotVectorCommand.Connection = SigmaDbConnErcot;
            //
            mInsertErcotVectorCommand = new SqlCommand();
            mInsertErcotVectorCommand.CommandText = "insert rtmastervector values (@constraintrtnum, @nodekey, @sensitivity, 0, null, null)";
            mInsertErcotVectorCommand.Parameters.AddWithValue("@constraintrtnum", "constraintrtnum");
            mInsertErcotVectorCommand.Parameters.AddWithValue("@nodekey", "nodekey");
            mInsertErcotVectorCommand.Parameters.AddWithValue("@sensitivity", "sensitivity");
            mInsertErcotVectorCommand.Connection = SigmaDbConnErcot;
            //
            mDtErcotShftFact = new DataTable();
            //
            mDtErcotShftFact.Columns.Add("ConstraintRTNum", typeof(int));
            mDtErcotShftFact.Columns.Add("NodeKey", typeof(int));
            mDtErcotShftFact.Columns.Add("Sensitivity", typeof(double));
            //
            mDeleteErcotShftFactCommand = new SqlCommand();
            mDeleteErcotShftFactCommand.CommandText = "truncate table rtmastervector_test ";
            mDeleteErcotShftFactCommand.Connection = SigmaDbConnErcot;
            //
            mSelectMasterRTCommand = new SqlCommand();
            mSelectMasterRTCommand.CommandText = "select constraintrtnum from rtmasterconstraint where monitoredtext like @monitoredtext and " +
                "contingencytext = @contingencytext";
            mSelectMasterRTCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
            mSelectMasterRTCommand.Connection = SigmaDbConnErcot;
            //
            mSelectMaxMinCommand = new SqlCommand();
            mSelectMaxMinCommand.CommandText = "select lmp from NodeLMPMin where marketdate = @marketdate and " +
                "markethour = @markethour and marketmin = @marketmin and second = @second";
            mSelectMaxMinCommand.Parameters.AddWithValue("@marketdate", "marketdate");
            mSelectMaxMinCommand.Parameters.AddWithValue("@markethour", "markethour");
            mSelectMaxMinCommand.Parameters.AddWithValue("@marketmin", "marketmin");
            mSelectMaxMinCommand.Parameters.AddWithValue("@second", "second");
            mSelectMaxMinCommand.Connection = SigmaDbConnErcot;
            //
            mSelectConstraintCommand = new SqlCommand();
            mSelectConstraintCommand.CommandText = "select constrainttext, contingencytext, shadowprice from ConstraintRT where " +
                                                                        "marketdatetime = @marketdatetime and shadowprice <> 0";
            mSelectConstraintCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mSelectConstraintCommand.Connection = SigmaDbConnErcot;
            //
            mInsertConstraintCommand = new SqlCommand();
            mInsertConstraintCommand.CommandText = "insert RTMasterConstraint values (@MarketDateTime, @MonitoredText, " +
                "@ContingencyText, @ShadowPrice, @NumberConstraints, @ShiftFactor, 0, 1, @DollarImpact, @UpdateTime, 0)";
            mInsertConstraintCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertConstraintCommand.Parameters.AddWithValue("@MonitoredText", "MonitoredText");
            mInsertConstraintCommand.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
            mInsertConstraintCommand.Parameters.AddWithValue("@ShadowPrice", "ShadowPrice");
            mInsertConstraintCommand.Parameters.AddWithValue("@NumberConstraints", "NumberConstraints");
            mInsertConstraintCommand.Parameters.AddWithValue("@ShiftFactor", "ShiftFactor");
            mInsertConstraintCommand.Parameters.AddWithValue("@DollarImpact", "DollarImpact");
            mInsertConstraintCommand.Parameters.AddWithValue("@UpdateTime", "UpdateTime");
            mInsertConstraintCommand.Connection = this.SigmaDbConnErcot;
        }
        private string ExtractFile(string splitRow)
        {
            string token = "a href='";
            int startIndex = splitRow.IndexOf(token);
            string temp = splitRow.Substring(startIndex + token.Length);
            temp = temp.Substring(0, temp.IndexOf("'"));
            string fileURL = "https://mis.ercot.com" + temp;
            //string fileURL = "https://mis.ercot.com/misdownload/servlets/mirDownload?mimic_duns=0809369482000&doclookupId=767344472";
            string fileName = splitRow.Substring(0, splitRow.IndexOf("<"));
            string filePath = "D:\\ISOFiles\\ERCOTDAResources\\ErcotRealTimeShiftFactor\\";
            fileName = filePath + fileName;
            HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(fileURL);
            FileDownloadRequest.ClientCertificates.Add(mCert);
            FileDownloadRequest.Timeout = 100000;
            FileDownloadRequest.Method = "GET";
            HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
            using (BinaryReader reader = new BinaryReader(FileDownloadresponse.GetResponseStream()))
            {
                using (FileStream fileStream = File.Open(fileName, FileMode.Create))
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
            return XMLUnZipFile(fileName, filePath);
        }
        private void SaveSensitivity(DataRow[] rows)
        {
            DateTime date = new DateTime();
            if (rows.Count()>0)
            {
                 date =Convert.ToDateTime( rows[0].ItemArray[0]).Date;
            }
           
            Dictionary<string, decimal> masterHash = new Dictionary<string, decimal>();
            Dictionary<string, decimal> sensitivityHash = new Dictionary<string, decimal>();
            Dictionary<string, decimal> dateHash = new Dictionary<string, decimal>();
            if (SigmaDbConnErcot.State == ConnectionState.Closed)
            {
                SigmaDbConnErcot.Open();
            }
            mDtErcotShftFact.Clear();
            foreach (DataRow row in rows)
            {
                string sensitivityKey = row[3].ToString() + row[4].ToString() + row[5].ToString();
                if (sensitivityHash.ContainsKey(sensitivityKey))
                {
                    continue;
                }
                string key = row[3].ToString() + row[4].ToString();
                decimal constraintId = 0;
                int nodeKey = mNodeHash[row[5].ToString()];
                //NCARBI_SEADRF1_1/NCARBIDE-SEADRFTC/138-138  BENTS_FRTER_1C_1/S_MISSIN-RAILROAD/138-138
                if (row[3].ToString().Contains("BENTS") )
                {

                }
                if (masterHash.ContainsKey(key))
                {
                    constraintId = masterHash[key];
                }
                else
                {
                    // mSelectMasterCommand.CommandText = mSelectMasterCommand.CommandText.Replace("@ConstraintText", "'" + row[3].ToString() + "%'");
                    //mSelectMasterCommand.Parameters["@ConstraintText"].Value = "'" + row[3].ToString() + "%'";
                    mSelectMasterCommand.CommandText = "select distinct b.ConstraintRTNum from Vayu..ConstraintRT a join rtmasterconstraint b on " +
              " a.ConstraintText = b.MonitoredText and a.ContingencyText = b.ContingencyText "
           + "where a.MarketDateTime > @sdate and a.MarketDateTime <= @edate and a.ConstraintText like '" + row[3].ToString() + "%'  and a.contingencytext ='" + row[4].ToString()+"'";
                   
                    //mSelectMasterCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
                    //mSelectMasterCommand.Parameters.AddWithValue("@ConstraintText", "ConstraintText");
                    mSelectMasterCommand.Connection = SigmaDbConnErcot;

                    //mSelectMasterCommand.Parameters["@ConstraintText"].Value =  row[3].ToString() ;
                    //mSelectMasterCommand.Parameters["@contingencytext"].Value = row[4].ToString();
                    mSelectMasterCommand.Parameters["@sdate"].Value = date;
                    mSelectMasterCommand.Parameters["@edate"].Value = date.AddDays(1);

                    SqlDataReader reader = mSelectMasterCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        constraintId = reader.GetDecimal(0);

                    }
                    reader.Close();
                    if (constraintId != 0)
                    {
                        masterHash.Add(key, constraintId);
                    }
                    //mSelectMasterCommand.CommandText = "select constraintrtnum from rtmasterconstraint where monitoredtext like @monitoredtext and " +
                                        //"contingencytext = @contingencytext";
                }
                if (constraintId == 0)
                {
                    if (dateHash.ContainsKey(row[0].ToString()))
                    {
                        continue;
                    }
                    DateTime dateTime = DateTime.Parse(row[0].ToString());
                    mSelectMaxMinCommand.Parameters["@marketdate"].Value = dateTime.Date;
                    mSelectMaxMinCommand.Parameters["@markethour"].Value = dateTime.Hour;
                    mSelectMaxMinCommand.Parameters["@marketmin"].Value = dateTime.Minute;
                    mSelectMaxMinCommand.Parameters["@second"].Value = dateTime.Second;
                    bool found = false;
                    List<decimal> lmpList = new List<decimal>();
                    SqlDataReader reader = mSelectMaxMinCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        lmpList.Add(reader.GetDecimal(0));
                        found = true;
                    }
                    reader.Close();
                    if (!found)
                    {
                        dateHash.Add(row[0].ToString(), constraintId);
                        continue;
                    }
                    decimal min = lmpList.Min();
                    decimal max = lmpList.Max();
                    decimal num = 0;
                    decimal sum = 0;
                    string constraint = null;
                    decimal shadowPrice = 0;
                    mSelectConstraintCommand.Parameters["@marketdatetime"].Value = dateTime;
                    reader = mSelectConstraintCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        num++;
                        sum += reader.GetDecimal(2);
                        if (reader.GetString(0).StartsWith(row[3].ToString()) && reader.GetString(1) == row[4].ToString())
                        {
                            shadowPrice = reader.GetDecimal(2);
                            constraint = reader.GetString(0);
                        }
                    }
                    reader.Close();
                    decimal impact = (Math.Abs(max - min) / sum) * (shadowPrice / sum);
                    mInsertConstraintCommand.Parameters["@MarketDateTime"].Value = dateTime;
                    mInsertConstraintCommand.Parameters["@MonitoredText"].Value = constraint;
                    mInsertConstraintCommand.Parameters["@ContingencyText"].Value = row[4].ToString();
                    mInsertConstraintCommand.Parameters["@ShadowPrice"].Value = shadowPrice;
                    mInsertConstraintCommand.Parameters["@NumberConstraints"].Value = num;
                    mInsertConstraintCommand.Parameters["@ShiftFactor"].Value = impact;
                    mInsertConstraintCommand.Parameters["@DollarImpact"].Value = impact * shadowPrice;
                    mInsertConstraintCommand.Parameters["@UpdateTime"].Value = DateTime.Now;
                    int a= mInsertConstraintCommand.ExecuteNonQuery();
                    mSelectMasterCommand.Connection = SigmaDbConnErcot;

                    //mSelectMasterCommand.Parameters["@ConstraintText"].Value =  row[3].ToString() ;
                    //mSelectMasterCommand.Parameters["@contingencytext"].Value = row[4].ToString();
                    mSelectMasterCommand.Parameters["@sdate"].Value = date;
                    mSelectMasterCommand.Parameters["@edate"].Value = date.AddDays(1);

                    reader = mSelectMasterCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        constraintId = reader.GetDecimal(0);
                    }
                    reader.Close();
                    if (constraintId != 0)
                    {
                        masterHash.Add(key, constraintId);
                    }
                    //mSelectMasterCommand.CommandText = "select constraintrtnum from rtmasterconstraint where monitoredtext like @monitoredtext and " +
                                        //"contingencytext = @contingencytext";
                }
                DataRow drErcotShftFact = mDtErcotShftFact.NewRow();
                drErcotShftFact["ConstraintRTNum"] = constraintId;
                if(constraintId==1897)
                {

                }
                drErcotShftFact["NodeKey"] = nodeKey;
                decimal sensitivity = 0;
                try
                {
                    if(row[6].ToString().Contains("E"))
                    {
                        sensitivity =0.0000001m * decimal.Parse(row[6].ToString().Replace("E-7", ""));
                    }
                    else
                    {
                        sensitivity = decimal.Parse(row[6].ToString().Replace("E-7", ""));

                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                

               
                drErcotShftFact["Sensitivity"] = -sensitivity;
               // if (constraintId == 762)
                {
                    mDtErcotShftFact.Rows.Add(drErcotShftFact);
                }
                sensitivityHash.Add(sensitivityKey, constraintId);
            }
            SigmaDbConnErcot.Close();
            if (SigmaDbConnErcot.State == ConnectionState.Closed)
            {
                SigmaDbConnErcot.Open();
            }
            SqlTransaction transaction = SigmaDbConnErcot.BeginTransaction();
            mDeleteErcotShftFactCommand.Transaction = transaction;
            mDeleteErcotShftFactCommand.ExecuteNonQuery();
            using (SqlBulkCopy bk15Min = new SqlBulkCopy(SigmaDbConnErcot, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bk15Min.DestinationTableName = "[rtmastervector_test]";
                    bk15Min.BatchSize = 1000;
                    bk15Min.BulkCopyTimeout = 10000;
                    bk15Min.ColumnMappings.Add("ConstraintRTNum", "ConstraintRTNum");
                    bk15Min.ColumnMappings.Add("NodeKey", "NodeKey");
                    bk15Min.ColumnMappings.Add("Sensitivity", "Sensitivity");
                    bk15Min.WriteToServer(mDtErcotShftFact);
                    SqlCommand updateNodeLMP = new SqlCommand("[UpMergeVector]", SigmaDbConnErcot, transaction);
                    updateNodeLMP.CommandTimeout = 10000;
                    updateNodeLMP.CommandType = CommandType.StoredProcedure;
                    updateNodeLMP.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SigmaDbConnErcot.Close();
                }
            }
            SigmaDbConnErcot.Close();
        }
        public void DownloadSensitivity()
        {
            string[] files = Directory.GetFiles("D:\\ISOFiles\\ERCOTDAResources\\ErcotRealTimeShiftFactor\\");
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            Console.WriteLine("Downloading File");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=16013");
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.ConnectionLimit = 1;
            request.CookieContainer = new CookieContainer();
            request.Method = "GET";
            request.ClientCertificates.Add(mCert);
            request.Timeout = 1000000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = default(StreamReader);
            sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string strnodedalmp = sr.ReadToEnd();
            DataSet dataset;
            sr.Close();
            string[] splinelitRow1 = strnodedalmp.Split(new string[] { "<td class='labelOptional_ind'>" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> rowList = new List<string>();
            foreach (string splitRow in splinelitRow1.Reverse<string>())
            {
                if (splitRow.IndexOf("xml") == -1)
                {
                    continue;
                }
                rowList.Add(splitRow);
            }
            int count = 0;
            foreach (string splitRow in rowList)
            {
                if (count > rowList.Count -15)
                {
                    try
                    {
                        string fileName = ExtractFile(splitRow);
                        Console.WriteLine(fileName);
                        XmlDataDocument xmlParser = new XmlDataDocument();
                        mERCOTLoadResourcesDT.Clear();
                        System.IO.FileStream fsReadXml = new System.IO.FileStream(fileName, System.IO.FileMode.Open);
                        dataset = new DataSet();
                        dataset.ReadXml(fsReadXml);
                        DataTable SensDt = dataset.Tables[0];
                        DataRow[] rows = SensDt.Select().ToArray();
                        SaveSensitivity(rows);
                        Console.WriteLine(DateTime.Now + " " + "Completed Inserting Sensitivities");
                        fsReadXml.Close();
                        fsReadXml.Dispose();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                count++;
            }
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
                Console.WriteLine(ex);
            }
            return strNewFile;
        }
        private void SaveHourlyImpact(DateTime startDate, DateTime endDate)
        {

            if (SigmaDbConn.State == ConnectionState.Open)
            {
                SigmaDbConn.Close();
            }
            SigmaDbConn.Open();

            while (startDate < endDate)
            {
                Console.WriteLine(DateTime.Now + " saving impact " + startDate);
                DateTime date = startDate.Date;
                int hour = startDate.AddHours(1).Hour == 0 ? 24 : startDate.AddHours(1).Hour;
                //
                deleteHourlyImpactCommand = new SqlCommand();
                deleteHourlyImpactCommand.CommandText = "delete Vayu..RTImpact where date =  Convert(Date, @Time) and hour = @Hour";
                deleteHourlyImpactCommand.Parameters.AddWithValue("@Time", "Time");
                deleteHourlyImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
                deleteHourlyImpactCommand.Connection = SigmaDbConn;
                deleteHourlyImpactCommand.Parameters["@Time"].Value = date;
                deleteHourlyImpactCommand.Parameters["@Hour"].Value = hour;
                int a= deleteHourlyImpactCommand.ExecuteNonQuery();
                //
                insertHourlyImpactCommand = new SqlCommand();
                insertHourlyImpactCommand.CommandText = "INSERT INTO Vayu..RTImpact(ConstraintRTNum, Date, Hour , ShadowPrice) " +
                "SELECT B.ConstraintRTNum, Convert(Date, @start) as Date, @Hour as Hour, abs(sum(A.ShadowPrice)) as ShadowPrice from Vayu..ConstraintRT as A inner join " +
                "Vayu..RTMasterConstraint as B on A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  where A.MarketDateTime >= @start and " +
                "A.MarketDateTime < @end and ConstraintText <> 'None' group by ConstraintText, A.ContingencyText, B.ConstraintRTNum";
                insertHourlyImpactCommand.Parameters.AddWithValue("@start", "a.marketdatetime");
                insertHourlyImpactCommand.Parameters.AddWithValue("@end", "a.marketdatetime");
                insertHourlyImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
                insertHourlyImpactCommand.Connection = SigmaDbConn;
                insertHourlyImpactCommand.Parameters["@start"].Value = date.AddHours(hour - 1);
                insertHourlyImpactCommand.Parameters["@Hour"].Value = hour;
                insertHourlyImpactCommand.Parameters["@end"].Value = date.AddHours(hour);
                insertHourlyImpactCommand.ExecuteNonQuery();
                //
                updateHourlyImpactCommand = new SqlCommand();
                updateHourlyImpactCommand.CommandText = "Update Vayu..RTImpact set impact = A.shadowprice*B.shiftfactor from Vayu..RTImpact A inner join Vayu..RTMasterConstraint B on A.ConstraintRTNum = B.ConstraintRTNum " +
                "where date = Convert(Date, @Time) and hour = @Hour and B.Shiftfactor is not null ";
                updateHourlyImpactCommand.Parameters.AddWithValue("@Time", "Time");
                updateHourlyImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
                updateHourlyImpactCommand.Connection = SigmaDbConn;
                updateHourlyImpactCommand.Parameters["@Time"].Value = date;
                updateHourlyImpactCommand.Parameters["@Hour"].Value = hour;
                updateHourlyImpactCommand.ExecuteNonQuery();
                startDate = startDate.AddHours(1);
            }

            SigmaDbConn.Close();
        }
    }
    public class MinMax
    {
        public double Min { get; set; }
        public double Max { get; set; }

    }
    class ConstraintHelper
    {
        public string ConstraintName { get; set; }
        public string ContingencyName { get; set; }
        public double ShadowPrice { get; set; }
        public int ConstraintCount { get; set; }
        public double TotalSP { get; set; }
    }
}

