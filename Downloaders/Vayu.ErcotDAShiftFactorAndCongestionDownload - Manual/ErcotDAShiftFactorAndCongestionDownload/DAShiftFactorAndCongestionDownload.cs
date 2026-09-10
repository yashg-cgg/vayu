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

namespace Vayu.ErcotDAShiftFactorAndCongestionDownload
{
    public class DAShiftFactorAndCongestionDownload
    {
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private SqlConnection VayuDbConn;
        private Dictionary<string, int> mNodeHash = new Dictionary<string, int>();
        private SqlCommand mSelectMasterCommand;
        private SqlCommand mSelectConstraintCommand;
        private SqlCommand mUpdateErcotConstraintCommand;
        private SqlCommand mInsertErcotConstraintCommand;
        private SqlCommand mSelectNodeCommand;
        private SqlCommand mDeleteErcotShftFactCommand;
        private SqlCommand mInsertConstraintCommand;
        private X509Certificate2 mCert = new X509Certificate2();
        private SqlCommand mSelectMaxMinCommand;
        private SqlCommand mInsertImpactCommand;
        private SqlCommand mUpdateImpactCommand;
        DataTable mDtDAConstraints = new DataTable();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        DataTable mDtErcotShftFact = new DataTable();
        Dictionary<DateTime, MinMax> dictMinMaxDict = new Dictionary<DateTime, MinMax>();
        Dictionary<DateTime, List<ConstraintHelper>> mConstraintDict = new Dictionary<DateTime, List<ConstraintHelper>>();
        DataTable mDtDACong = new DataTable();
        Dictionary<string, DAConstraint> mMasterDAConstraintHash = new Dictionary<string, DAConstraint>();
        private SqlCommand mSelectMasterDAConstraintCommand;
        private SqlCommand mDeleteNodeDALMPHCommand;
        private SqlCommand mDeleteShiftFactorsCommand;
        private SqlCommand mDeleteImpactCommand;
        public DAShiftFactorAndCongestionDownload()
        {
            InitDB();
            FillNodeHash();
            InitCert();
            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 15 * 60 * 1000;
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
            if (VayuDbConn.State == System.Data.ConnectionState.Open)
            {
                VayuDbConn.Close();
            }
            VayuDbConn.Open();
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                int nodekey = Convert.ToInt32(reader.GetValue(0));
                string a = reader.GetString(1);
                mNodeHash.Add(reader.GetString(1), nodekey);
            }
            reader.Close();
            VayuDbConn.Close();
        }
        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadSensitivity();
            SaveImpact(DateTime.Today.AddDays(-5), DateTime.Today.AddDays(1));
            mTimer.Enabled = true;
        }
        public void InitDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");

            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select NodeKey, nodename from Vayu..Node where MarketKey=9";
            mSelectNodeCommand.Connection = VayuDbConn;
            
            //
            mSelectMasterCommand = new SqlCommand();
            mSelectMasterCommand.CommandText = "select constraintrtnum from DAMasterConstraint where monitoredtext like @monitoredtext and " +
                "contingencytext = @contingencytext";
            mSelectMasterCommand.Parameters.AddWithValue("@contingencytext", "contingencytext");
            mSelectMasterCommand.Connection = VayuDbConn;
            //
            mDtErcotShftFact = new DataTable();
            //
            mDtErcotShftFact.Columns.Add("ConstraintRTNum", typeof(int));
            mDtErcotShftFact.Columns.Add("NodeKey", typeof(int));
            mDtErcotShftFact.Columns.Add("Sensitivity", typeof(double));
            //
            mDeleteErcotShftFactCommand = new SqlCommand();
            mDeleteErcotShftFactCommand.CommandText = "truncate table DAMasterVector_test ";
            mDeleteErcotShftFactCommand.Connection = VayuDbConn;
            //
            mSelectMaxMinCommand = new SqlCommand();
            mSelectMaxMinCommand.CommandText = "select lmp from Vayu..NodeDALMPH where MarketDateTime = @MarketDateTime ";
            mSelectMaxMinCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mSelectMaxMinCommand.Connection = VayuDbConn;
            //
            mSelectConstraintCommand = new SqlCommand();
            mSelectConstraintCommand.CommandText = "select constrainttext, contingencytext, shadowprice from Constraintda where " +
                                                                        "marketdatetime = @marketdatetime and shadowprice <> 0";
            mSelectConstraintCommand.Parameters.AddWithValue("@marketdatetime", "marketdatetime");
            mSelectConstraintCommand.Connection = VayuDbConn;
            //
            mInsertConstraintCommand = new SqlCommand();
            mInsertConstraintCommand.CommandText = "insert DAMasterConstraint values (@MarketDateTime, @MonitoredText, " +
                "@ContingencyText, @ShadowPrice, @NumberConstraints, @ShiftFactor, 0, 1, @DollarImpact, @UpdateTime, 0)";
            mInsertConstraintCommand.Parameters.AddWithValue("@MarketDateTime", "MarketDateTime");
            mInsertConstraintCommand.Parameters.AddWithValue("@MonitoredText", "MonitoredText");
            mInsertConstraintCommand.Parameters.AddWithValue("@ContingencyText", "ContingencyText");
            mInsertConstraintCommand.Parameters.AddWithValue("@ShadowPrice", "ShadowPrice");
            mInsertConstraintCommand.Parameters.AddWithValue("@NumberConstraints", "NumberConstraints");
            mInsertConstraintCommand.Parameters.AddWithValue("@ShiftFactor", "ShiftFactor");
            mInsertConstraintCommand.Parameters.AddWithValue("@DollarImpact", "DollarImpact");
            mInsertConstraintCommand.Parameters.AddWithValue("@UpdateTime", "UpdateTime");
            mInsertConstraintCommand.Connection = this.VayuDbConn;

            mDtDAConstraints.Columns.Add("MarketDateTime", typeof(DateTime));
            mDtDAConstraints.Columns.Add("ConstraintText", typeof(string));
            mDtDAConstraints.Columns.Add("ContingencyText", typeof(string));
            mDtDAConstraints.Columns.Add("ShadowPrice", typeof(double));

            mDtDACong.Columns.Add("NodeKey", typeof(int));
            mDtDACong.Columns.Add("Congestion", typeof(double));
            mDtDACong.Columns.Add("MarketDateTime", typeof(DateTime));

            mSelectMasterDAConstraintCommand = new SqlCommand();
            mSelectMasterDAConstraintCommand.CommandText = "select  MarketDateTime,ContingencyText,ConstraintText,ShadowPrice from Vayu..ConstraintDA where MarketDateTime>@SDate and MarketDateTime<=@Edate order by MarketDateTime";
            mSelectMasterDAConstraintCommand.Parameters.AddWithValue("@SDate", "MarketDateTime");
            mSelectMasterDAConstraintCommand.Parameters.AddWithValue("@Edate", "MarketDateTime");
            mSelectMasterDAConstraintCommand.Connection = VayuDbConn;

            mDeleteNodeDALMPHCommand = new SqlCommand();
            mDeleteNodeDALMPHCommand.CommandText = "truncate table NodeDALMPH_ShiftFactor";
            mDeleteNodeDALMPHCommand.Connection = VayuDbConn;

            mDeleteShiftFactorsCommand = new SqlCommand();
            mDeleteShiftFactorsCommand.CommandText = "truncate table ConstraintDA_Factor";
            mDeleteShiftFactorsCommand.Connection = VayuDbConn;
        }
        private string ExtractFile(string splitRow)
        {
            string token = "a href='";
            int startIndex = splitRow.IndexOf(token);
            string temp = splitRow.Substring(startIndex + token.Length);
            temp = temp.Substring(0, temp.IndexOf("'"));
            string fileURL = "https://mis.ercot.com" + temp;
            //string fileURL = "https://mis.ercot.com/misdownload/servlets/mirDownload?mimic_duns=0809369482000&doclookupId=747326585";
            string fileName = splitRow.Substring(0, splitRow.IndexOf("<"));


            string filePath = "D:\\ISOFiles\\ErcotDAShiftFactorAndCongestion\\";

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
            Dictionary<string, decimal> masterHash = new Dictionary<string, decimal>();
            Dictionary<string, decimal> sensitivityHash = new Dictionary<string, decimal>();
            Dictionary<string, decimal> dateHash = new Dictionary<string, decimal>();
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            mDtErcotShftFact.Clear();
            foreach (DataRow row in rows)
            {
                string sensitivityKey = row[4].ToString().Trim() + row[3].ToString().Trim() + row[11].ToString().Trim();

                //if (sensitivityKey.Contains("AMIHY_HAMRD_1B_1"))
                //{ }
                //else
                //    continue;


                if (sensitivityHash.ContainsKey(sensitivityKey))
                {
                    continue;
                }
                string key = row[4].ToString().Trim() + row[3].ToString().Trim()+ row[5].ToString().Trim();
                { 
                }
                decimal constraintId = 0;
                bool cfound = false;
                int nodeKey = mNodeHash[row[11].ToString()];
                if (masterHash.ContainsKey(key))
                {
                    constraintId = masterHash[key];
                }
                else
                {
                    mSelectMasterCommand.CommandText = mSelectMasterCommand.CommandText.Replace("@monitoredtext", "'" + row[4].ToString().Trim() + "%'" + "'" + row[5].ToString().Trim() + "%'");
                    mSelectMasterCommand.Parameters["@contingencytext"].Value = row[3].ToString().Trim();
                    SqlDataReader reader = mSelectMasterCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        constraintId = reader.GetDecimal(0);
                    }
                    reader.Close();
                    if(constraintId== 1011)
                    {

                    }
                    if (constraintId != 0)
                    {
                        masterHash.Add(key, constraintId);
                    }
                    mSelectMasterCommand.CommandText = "select constraintrtnum from DAMasterConstraint where monitoredtext like @monitoredtext and " +
                                        "contingencytext = @contingencytext";
                }
                if (constraintId == 0)
                {
                    string[] time = row[1].ToString().Split(':');
                    DateTime dateTime = DateTime.Parse(row[0].ToString()).AddHours(int.Parse(time[0]));
                    if (dateHash.ContainsKey(dateTime.ToString()))
                    {
                        continue;
                    }
                    mSelectMaxMinCommand.Parameters["@MarketDateTime"].Value = dateTime;
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
                        dateHash.Add(dateTime.ToString(), constraintId);
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
                        if (reader.GetString(0).StartsWith(row[4].ToString().Trim()) && reader.GetString(1) == row[3].ToString().Trim())
                        {
                            shadowPrice = reader.GetDecimal(2);
                            constraint = reader.GetString(0);
                        }
                    }
                    reader.Close();
                    decimal impact = (Math.Abs(max - min) / sum) * (shadowPrice / sum);

                    mUpdateErcotConstraintCommand = new SqlCommand();
                    mUpdateErcotConstraintCommand.Connection = this.VayuDbConn; 
                    mUpdateErcotConstraintCommand.CommandText = "update Vayu..DAMasterConstraint set marketdatetime = @marketdatetime, " +
                        "shadowprice = @shadowprice, numberconstraints = @numberconstraints, " +
                        "shiftfactor = @shiftfactor, dollarimpact = @dollarimpact, updatetime = @updatetime where monitoredtext =" +
                        " @MonitoredText and contingencytext = @ContingencyText";
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@marketdatetime", dateTime);
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shadowprice", shadowPrice);
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@numberconstraints", num);
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@shiftfactor", impact);
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@dollarimpact", impact * shadowPrice);
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@updatetime", DateTime.Now);
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@MonitoredText", constraint.Trim());
                    mUpdateErcotConstraintCommand.Parameters.AddWithValue("@ContingencyText", row[3].ToString().Trim());
                    Console.WriteLine("Updating ERCOT DAMasterConstraint ");
                    if (VayuDbConn.State == ConnectionState.Closed)
                        VayuDbConn.Open();
                    int Updatecount = mUpdateErcotConstraintCommand.ExecuteNonQuery();
                    if (Updatecount == 0)
                    {                     
                        mInsertConstraintCommand.Parameters["@MarketDateTime"].Value = dateTime;
                        mInsertConstraintCommand.Parameters["@MonitoredText"].Value = constraint.Trim();
                        mInsertConstraintCommand.Parameters["@ContingencyText"].Value = row[3].ToString().Trim();
                        mInsertConstraintCommand.Parameters["@ShadowPrice"].Value = shadowPrice;
                        mInsertConstraintCommand.Parameters["@NumberConstraints"].Value = num;
                        mInsertConstraintCommand.Parameters["@ShiftFactor"].Value = impact;
                        mInsertConstraintCommand.Parameters["@DollarImpact"].Value = impact * shadowPrice;
                        mInsertConstraintCommand.Parameters["@UpdateTime"].Value = DateTime.Now;
                        mInsertConstraintCommand.ExecuteNonQuery();
                        Console.WriteLine("Inserting Data ERCOT DAMasterConstraint ");


                    }

                    //if (row[4].ToString().Trim().Contains("AMIHY_HAMRD_1B_1"))
                    //{ }
                    //else
                    //    continue;

                    string row6, row8;
                    row6 = row8 = "";

                    if (row[6].ToString().Trim() == "")
                        row6 = "0";
                    else
                        row6 = row[6].ToString().Trim();

                    if (row[8].ToString().Trim() == "")
                        row8 = "0";
                    else
                        row8 = row[8].ToString().Trim();

                    string monitortextToSerach = row[4].ToString().Trim() + "/" + row[5].ToString().Trim() + "-" + row[7].ToString().Trim()+"/"+ row6.ToString().Trim()+"-"+ row8.ToString().Trim();

                    //mSelectMasterCommand.CommandText = mSelectMasterCommand.CommandText.Replace("@monitoredtext", "'" + row[4].ToString().Trim() + "%'");
                    mSelectMasterCommand.CommandText = mSelectMasterCommand.CommandText.Replace("@monitoredtext", "'" + monitortextToSerach + "%'");
                    mSelectMasterCommand.Parameters["@contingencytext"].Value = row[3].ToString().Trim();
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


                    mSelectMasterCommand.CommandText = "select constraintrtnum from DAMasterConstraint where monitoredtext like @monitoredtext and " +
                                        "contingencytext = @contingencytext";

                }
                DataRow drErcotShftFact = mDtErcotShftFact.NewRow();
                drErcotShftFact["ConstraintRTNum"] = constraintId;
                if(constraintId== 1168330)
                {

                }
                drErcotShftFact["NodeKey"] = nodeKey;
                decimal sensitivity = 0;
                try
                {
                    sensitivity = decimal.Parse(row[12].ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                drErcotShftFact["Sensitivity"] = -sensitivity;
                mDtErcotShftFact.Rows.Add(drErcotShftFact);
                sensitivityHash.Add(sensitivityKey, constraintId);
            }
            VayuDbConn.Close();
            if (VayuDbConn.State == ConnectionState.Closed)
            {
                VayuDbConn.Open();
            }
            SqlTransaction transaction = VayuDbConn.BeginTransaction();
            mDeleteErcotShftFactCommand.Transaction = transaction;
            mDeleteErcotShftFactCommand.ExecuteNonQuery();
            using (SqlBulkCopy bk15Min = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bk15Min.DestinationTableName = "[DAMasterVector_test]";
                    bk15Min.BatchSize = 1000;
                    bk15Min.BulkCopyTimeout = 10000;
                    bk15Min.ColumnMappings.Add("ConstraintRTNum", "ConstraintRTNum");
                    bk15Min.ColumnMappings.Add("NodeKey", "NodeKey");
                    bk15Min.ColumnMappings.Add("Sensitivity", "Sensitivity");
                    bk15Min.WriteToServer(mDtErcotShftFact);
                    SqlCommand updateNodeLMP = new SqlCommand("[UpMergeVectorDA]", VayuDbConn, transaction);
                    updateNodeLMP.CommandTimeout = 10000;
                    updateNodeLMP.CommandType = CommandType.StoredProcedure;
                    updateNodeLMP.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    VayuDbConn.Close();
                }
            }
            VayuDbConn.Close();
        }
        public void DownloadSensitivity()
        {

            string[] files = Directory.GetFiles("D:\\ISOFiles\\ErcotDAShiftFactorAndCongestion\\");

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
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=13089");
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://www.ercot.com/misapp/servlets/IceDocListJsonWS?reportTypeId=13499");

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
            int count = rowList.Count;
            rowList.Reverse();

            foreach (string splitRow in rowList)
            {
                //if (splitRow.Contains("cdr.00013089.0000000000000000.20240714.123602280.DAMSHIFTFACTORSNP4793"))
                //{ 
                //}
                //else
                //    continue;
                //cdr.00013089.0000000000000000.20240625.123832974.DAMSHIFTFACTORSNP4793_csv.zip
                if (count > rowList.Count-15)
                {
                    try
                    {
                        string fileName = ExtractFile(splitRow);
                        Console.WriteLine(fileName);
                        XmlDataDocument xmlParser = new XmlDataDocument();
                        System.IO.FileStream fsReadXml = new System.IO.FileStream(fileName, System.IO.FileMode.Open);
                        dataset = new DataSet();
                        dataset.ReadXml(fsReadXml);
                        SaveVector(dataset.Tables[0].Select());
                        SaveLmpCong(dataset.Tables[0].Select());
                        DataTable SensDt = dataset.Tables[0];
                        DataRow[] rows = SensDt.Select().ToArray();
                        SaveSensitivity(rows);
                        Console.WriteLine(DateTime.Now + " " + "Completed Inserting Sensitivities");
                    }
                    catch (Exception ex)
                    {

                    }
                }
                count--;
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
        private void SaveVector(DataRow[] xmlDataRows)
        {
            mDtDAConstraints.Clear();
            foreach (var row in xmlDataRows)
            {
                DateTime date = Convert.ToDateTime(row[0]);
                GetConstraintDA(date);
                break;
            }
            Dictionary<string, DAConstraint> dicda = new Dictionary<string, DAConstraint>();
            foreach (var row in xmlDataRows)
            {
                string split1 = (string)row[6].ToString().Trim();
                string split2 = (string)row[8].ToString().Trim();
                if (split1 == "")
                {
                    split1 = "0";
                }
                if (split2 == "")
                {
                    split2 = "0";
                }
                string con = (string)row.ItemArray[4].ToString().Trim() + '/' + (string)row[5].ToString().Trim() + '-' + (string)row[7].ToString().Trim() + '/' + split1 + '-' + split2;
                string key = con + (string)row.ItemArray[3].ToString().Trim();
                if (!mMasterDAConstraintHash.ContainsKey(key))
                {
                    DataRow drLmph = mDtDAConstraints.NewRow();
                    DateTime date = new DateTime();
                    if (row.ItemArray[1].ToString().Contains("24"))
                    {
                        date = Convert.ToDateTime(row[0]).AddDays(1);
                    }
                    else
                    {
                        string time = " " + row.ItemArray[1].ToString();
                        string ti = row[0] + time;
                        date = Convert.ToDateTime(row[0] + time);
                    }
                    DAConstraint mDAConstraint = new DAConstraint();
                    mDAConstraint.MarketDateTime = date;
                    mDAConstraint.ConstraintText = con;
                    mDAConstraint.ContingencyText = (string)row.ItemArray[3].ToString().Trim();
                    mDAConstraint.ShadowPrice = Convert.ToDouble(row.ItemArray[10]);

                    drLmph["MarketDateTime"] = date;
                    drLmph["ConstraintText"] = con;
                    drLmph["ContingencyText"] = (string)row.ItemArray[3].ToString().Trim();
                    drLmph["ShadowPrice"] = Convert.ToDouble(row.ItemArray[10]);

                    if (!dicda.ContainsKey(key))
                    {
                        dicda.Add(key, mDAConstraint);
                        mDtDAConstraints.Rows.Add(drLmph);
                    }
                }
            }
            if (mDtDAConstraints.Rows.Count > 0)
            {
                if (VayuDbConn.State == ConnectionState.Open)
                {
                    VayuDbConn.Close();
                }
                VayuDbConn.Open();
                SqlTransaction transaction;
                transaction = VayuDbConn.BeginTransaction();
                mDeleteShiftFactorsCommand.Transaction = transaction;
                mDeleteShiftFactorsCommand.ExecuteNonQuery();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {

                        bkLmpH.DestinationTableName = "[ConstraintDA_Factor]";
                        bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                        bkLmpH.ColumnMappings.Add("ContingencyText", "ContingencyText");
                        bkLmpH.ColumnMappings.Add("ConstraintText", "ConstraintText");
                        bkLmpH.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                        bkLmpH.WriteToServer(mDtDAConstraints);
                        SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeShiftFactor]", VayuDbConn, transaction);
                        updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                        updateNodeLmpMin.CommandTimeout = 300000;
                        updateNodeLmpMin.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        VayuDbConn.Close();
                    }
                }
                VayuDbConn.Close();
            }

        }
        public void GetConstraintDA(DateTime date)
        {
            mMasterDAConstraintHash = new Dictionary<string, DAConstraint>();
            if (VayuDbConn.State == ConnectionState.Open)
            {
                VayuDbConn.Close();
            }
            VayuDbConn.Open();
            mSelectMasterDAConstraintCommand.Parameters["@SDate"].Value = date;
            mSelectMasterDAConstraintCommand.Parameters["@EDate"].Value = date.AddDays(1);
            SqlDataReader rdr = mSelectMasterDAConstraintCommand.ExecuteReader();
            while (rdr.Read())
            {
                DAConstraint daobj = new DAConstraint();
                daobj.MarketDateTime = rdr.GetDateTime(0);
                daobj.ConstraintText = rdr.GetString(2).ToString().Trim();
                daobj.ContingencyText = rdr.GetString(1).ToString().Trim();
                daobj.ShadowPrice = Convert.ToDouble(rdr.GetValue(3));
                string key = daobj.ConstraintText.Trim() + daobj.ContingencyText.Trim();
                if (!mMasterDAConstraintHash.ContainsKey(key))
                    mMasterDAConstraintHash.Add(key, daobj);
            }
            rdr.Close();
            VayuDbConn.Close();
        }
        private void SaveLmpCong(DataRow[] xmlDataRows)
        {
            mDtDACong.Clear();
            Dictionary<string, double> dictCong = new Dictionary<string, double>();
            foreach (var row in xmlDataRows)
            {
                string nodename = (string)row[11].ToString().Trim();
                DateTime date = new DateTime();
                if (row.ItemArray[1].ToString().Contains("24"))
                {
                    date = Convert.ToDateTime(row[0]).AddDays(1);
                }
                else
                {
                    string time = " " + row.ItemArray[1].ToString();
                    string ti = row[0] + time;
                    date = Convert.ToDateTime(row[0] + time);
                }
                string key = nodename + "?" + date;
                if (mNodeHash.ContainsKey(nodename))
                {
                    double congSum = Math.Round(Convert.ToDouble(row.ItemArray[10]) * Convert.ToDouble(row.ItemArray[12]), 2);

                    if (dictCong.ContainsKey(key))
                    {
                        congSum = dictCong[key] + Math.Round(Convert.ToDouble(row.ItemArray[10]) * Convert.ToDouble(row.ItemArray[12]), 2);
                        dictCong.Remove(key);
                    }
                    dictCong.Add(key, congSum);
                }
            }
            foreach (var row in dictCong)
            {
                string[] splitrow = row.Key.Split('?');
                DataRow drLmph = mDtDACong.NewRow();
                drLmph["NodeKey"] = (mNodeHash[splitrow[0]]);
                drLmph["Congestion"] = (-1)*Math.Round(row.Value, 2);
                drLmph["MarketDateTime"] = Convert.ToDateTime(splitrow[1]); ;
                mDtDACong.Rows.Add(drLmph);
            }
            if (mDtDACong.Rows.Count > 0)
            {
                if (VayuDbConn.State == ConnectionState.Open)
                {
                    VayuDbConn.Close();
                }
                VayuDbConn.Open();
                SqlTransaction transaction;
                transaction = VayuDbConn.BeginTransaction();
                mDeleteNodeDALMPHCommand.Transaction = transaction;
                mDeleteNodeDALMPHCommand.ExecuteNonQuery();
                using (SqlBulkCopy bkLmpH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkLmpH.DestinationTableName = "[NodeDALMPH_ShiftFactor]";
                        bkLmpH.ColumnMappings.Add("NodeKey", "NodeKey");
                        bkLmpH.ColumnMappings.Add("Congestion", "Congestion");
                        bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                        bkLmpH.WriteToServer(mDtDACong);
                        SqlCommand updateNodeLmpMin = new SqlCommand("[UpMergeDACong]", VayuDbConn, transaction);
                        updateNodeLmpMin.CommandType = CommandType.StoredProcedure;
                        updateNodeLmpMin.CommandTimeout = 300000;
                        updateNodeLmpMin.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        VayuDbConn.Close();
                    }
                }
                VayuDbConn.Close();
            }
        }

        private void SaveImpact(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (VayuDbConn.State == ConnectionState.Closed)
                    VayuDbConn.Open();
                while (startDate < endDate)
                {
                    Console.WriteLine(DateTime.Now + " Saving Impact...." + startDate);
                    DateTime date = startDate.Date;
                    int hour = startDate.AddHours(1).Hour == 0 ? 24 : startDate.AddHours(1).Hour;

                    mDeleteImpactCommand = new SqlCommand();
                    mDeleteImpactCommand.CommandText = "Delete Vayu..DAImpact where date=Convert(Date, @Time) and hour = @Hour";
                    mDeleteImpactCommand.Parameters.AddWithValue("@Time", "Time");
                    mDeleteImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
                    mDeleteImpactCommand.Connection = VayuDbConn;
                    mDeleteImpactCommand.Parameters["@Time"].Value = date;
                    mDeleteImpactCommand.Parameters["@Hour"].Value = hour;
                    mDeleteImpactCommand.ExecuteNonQuery();

                    mInsertImpactCommand = new SqlCommand();
                    mInsertImpactCommand.CommandText = "INSERT INTO Vayu..DAImpact(ConstraintRTNum, Date, Hour , ShadowPrice) " +
                                                        "SELECT B.ConstraintRTNum, Convert(Date, @start) as Date, @Hour as Hour, abs(sum(A.ShadowPrice)) as ShadowPrice from Vayu..ConstraintDA as A inner join " +
                                                        "Vayu..DAMasterConstraint as B on A.ConstraintText = B.MonitoredText and A.ContingencyText = B.ContingencyText  where A.MarketDateTime >= @start and " +
                                                        "A.MarketDateTime < @end and ConstraintText <> 'None' group by ConstraintText, A.ContingencyText, B.ConstraintRTNum";
                    mInsertImpactCommand.Parameters.AddWithValue("@start", "a.marketdatetime");
                    mInsertImpactCommand.Parameters.AddWithValue("@end", "a.marketdatetime");
                    mInsertImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
                    mInsertImpactCommand.Connection = VayuDbConn;
                    mInsertImpactCommand.Parameters["@start"].Value = date.AddHours(hour - 1);
                    mInsertImpactCommand.Parameters["@Hour"].Value = hour;
                    mInsertImpactCommand.Parameters["@end"].Value = date.AddHours(hour);
                    mInsertImpactCommand.ExecuteNonQuery();

                    mUpdateImpactCommand = new SqlCommand();
                    mUpdateImpactCommand.CommandText = "Update Vayu..DAImpact set impact = A.shadowprice*B.shiftfactor from Vayu..DAImpact A inner join Vayu..DAMasterConstraint B on A.ConstraintRTNum = B.ConstraintRTNum " +
                                                       "where date = Convert(Date, @Time) and hour = @Hour and B.Shiftfactor is not null ";
                    mUpdateImpactCommand.Parameters.AddWithValue("@Time", "Time");
                    mUpdateImpactCommand.Parameters.AddWithValue("@Hour", "Hour");
                    mUpdateImpactCommand.Connection = VayuDbConn;
                    mUpdateImpactCommand.Parameters["@Time"].Value = date;
                    mUpdateImpactCommand.Parameters["@Hour"].Value = hour;
                    mUpdateImpactCommand.ExecuteNonQuery();
                    startDate = startDate.AddHours(1);
                }
            }
            catch
            {

            }
            finally
            {
                VayuDbConn.Close();
            }
        }
    }
    public class DAShiftFactor
    {
        public int ConstraintRTNum { get; set; }
        public int numberconstraints { get; set; }
        public double shiftfactor { get; set; }
        public string MonitoredText { get; set; }
        public string ContingencyText { get; set; }

    }
    public class MinMax
    {
        public double Min { get; set; }
        public double Max { get; set; }

    }
    public class DAConstraint
    {
        public DateTime MarketDateTime { get; set; }
        public string ContingencyText { get; set; }
        public string ConstraintText { get; set; }
        public double ShadowPrice { get; set; }

    }
    //class ConstraintHelper
    //{
    //    public string ConstraintName { get; set; }
    //    public string ContingencyName { get; set; }
    //    public double ShadowPrice { get; set; }
    //    public int ConstraintCount { get; set; }
    //    public double TotalSP { get; set; }
    //}
}

