using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using ICSharpCode.SharpZipLib.Zip;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Vayu.CommonAccessLibrary;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Vayu.ErcotConstraintsDownload
{
    public class ActiveConstraint
    {
        SqlConnection DBConnection = new SqlConnection();
        private SqlCommand mSelectConstraintCommand;
        SqlCommand mDeleteCommand = new SqlCommand();
        SqlCommand mDeleteERCOTGeoCommand = new SqlCommand();
        private Dictionary<string, List<int>> constraintHash = new Dictionary<string, List<int>>();
        Timer mTimer = new Timer();
        X509Certificate2 Ercotcer = new X509Certificate2();
        DataTable constraintDT = new DataTable();
        private DataTable mDTConstraintGeo = new DataTable();
        DataSet dataSet = new DataSet();
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        public void LoadDB()
        {
     
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdServerCert");
            DBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mDeleteCommand = new SqlCommand();
            mDeleteCommand.CommandText = "Truncate table ActiveConstraintsTemp";
            mDeleteCommand.Connection = DBConnection;

            mDeleteERCOTGeoCommand = new SqlCommand();
            mDeleteERCOTGeoCommand.CommandText = "Truncate table Vayu..[ConstraintGeoTest]";

            mSelectConstraintCommand = new SqlCommand();
            mSelectConstraintCommand.CommandText = " select distinct RT.ConstraintText,RT.ContingencyText,GE.SourceNodeKey,GE.SinkNodeKey from ConstraintRT RT " +
                                             " join ConstraintGeo GE on RT.ConstraintText=GE.ConstraintText and RT.ContingencyText=GE.ContingencyText ";
            mSelectConstraintCommand.Connection = DBConnection;

            constraintDT.Columns.Add("MarketDateTime", typeof(DateTime));
            constraintDT.Columns.Add("MonitoredText", typeof(string));
            constraintDT.Columns.Add("Contingency", typeof(string));
            constraintDT.Columns.Add("MonitoredElementType", typeof(string));
            constraintDT.Columns.Add("ContingencyDesc", typeof(string));
            constraintDT.Columns.Add("FromStation", typeof(string));
            constraintDT.Columns.Add("ToStation", typeof(string));
            constraintDT.Columns.Add("FromKV", typeof(int));
            constraintDT.Columns.Add("ToKV", typeof(int));
            constraintDT.Columns.Add("MonitoredID1", typeof(string));
            constraintDT.Columns.Add("MonitoredID2", typeof(string));
            constraintDT.Columns.Add("RatingType", typeof(string));
            constraintDT.Columns.Add("RatingMW", typeof(decimal));
            constraintDT.Columns.Add("PostCTGFlowMW", typeof(decimal));
            constraintDT.Columns.Add("PercentViolation", typeof(decimal));
            constraintDT.Columns.Add("DSTFlag", typeof(string));

            mDTConstraintGeo.Columns.Add("ContingencyText", typeof(string));
            mDTConstraintGeo.Columns.Add("ConstraintText", typeof(string));
            mDTConstraintGeo.Columns.Add("SourceNodeKey", typeof(int));
            mDTConstraintGeo.Columns.Add("SinkNodeKey", typeof(int));
        }
        public ActiveConstraint()
        {
            LoadDB();
            InitCert();
            HashConstraint();
            mTimer = new Timer();
            mTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            mTimer.Interval = 1 * 60 * 1000;
            OnTimerEvent(null, null);
            mTimer.Start();
            Console.WriteLine("Wait for 5 min ...");
            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }
        public void InitCert()
        {
            Console.WriteLine("Checking Authentication....");
            Ercotcer.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        public void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadActConstraint();
            mTimer.Enabled = true;
        }
        public void DownloadActConstraint()
        {
            try
            {
                Console.WriteLine("Downloading Files...");
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\\ActiveConstraint\");
                for (int m = 0; m < filePaths.Length; m++)
                {
                    File.Delete(filePaths[m]);
                }
                string URL = "https://mis.ercot.com/misapp/GetReports.do?reportTypeId=12305";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.ConnectionLimit = 1;
                request.CookieContainer = new CookieContainer();
                request.Method = "GET";
                request.ClientCertificates.Add(Ercotcer);
                request.Timeout = 1000000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string strActConstraint = sr.ReadToEnd();
                strActConstraint = strActConstraint.Replace("\r\n", "").Trim();
                strActConstraint = strActConstraint.Substring(strActConstraint.IndexOf("<b>NSA Active Constraints</b></"));
                string[] splinelitRow = strActConstraint.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);
                bool isBreak = false;
                for (int i = 3; i < splinelitRow.Length; i = i + 3)
                {
                    if (isBreak)
                        break;
                    string FileURL = "https://mis.ercot.com/" + splinelitRow[i].Substring(10, 80);
                    string FileName = "D:\\ISOFiles\\ErcotConstraintsDownload\\Ercot_" + FileURL.Substring(65, 14) + ".zip";
                    HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                    FileDownloadRequest.ClientCertificates.Add(Ercotcer);
                    FileDownloadRequest.Timeout = 100000;
                    FileDownloadRequest.Method = "GET";
                    HttpWebResponse FileDownloadRespose = (HttpWebResponse)FileDownloadRequest.GetResponse();
                    string fileDestName = FileName;
                    string folderFilename = @"D:\ISOFiles\ErcotConstraintsDownload";
                    constraintDT.Clear();
                    mDTConstraintGeo.Clear();
                    using (BinaryReader reader = new BinaryReader(FileDownloadRespose.GetResponseStream()))
                    {
                        using (FileStream filestream = File.Open(FileName, FileMode.Create))
                        {
                            using (BinaryWriter writer = new BinaryWriter(filestream))
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
                    Console.WriteLine("Extracting Unzip file....");
                    bool isFound = false;
                    XMLUnzipFile(fileDestName, folderFilename);
                    DateTime marketDateTime = DateTime.Now;
                    string[] destinationFolderfilePaths = Directory.GetFiles(folderFilename);
                    int fileInd = 0;
                    filePaths = destinationFolderfilePaths;
                    string csvFileName = filePaths[0];
                    for (int j = 0; j < filePaths.Length; j++)
                    {
                        if (filePaths[j].Contains("xml"))
                        {
                            string[] fileDate = destinationFolderfilePaths[j].Split('.');
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            string format = "yyyyMMdd";
                            marketDateTime = DateTime.ParseExact(fileDate[3].ToString(), format, provider);
                            fileInd = i;
                            File.Delete(filePaths[0]);
                            File.Delete(filePaths[0 + 1]);
                            isFound = true;
                            break;
                        }
                    }
                    if (isFound)
                        continue;
                    try
                    {
                        using (StreamReader streamreader = new StreamReader(csvFileName))
                        {
                            string data = streamreader.ReadToEnd();
                            data = data.Replace("\"", "");
                            data = data.Replace("\r", "");
                            data = data.Trim();
                            string[] split1 = data.Split(new char[] { '\n' });
                            Console.WriteLine("Fill into Data Table...");
                            for (int k = 0; k < split1.Length; k++)
                            {
                                if (k == 0)
                                    continue;

                                string data1 = split1[k].ToString();
                                string[] splitData = data1.Split(new char[] { ',' });
                                DataRow drGeoConstraints = mDTConstraintGeo.NewRow();
                                DataRow drConstraint = constraintDT.NewRow();
                                marketDateTime = Convert.ToDateTime(splitData[0].Trim());
                                string[] arryconstaint = splitData[3].Trim().Split(new char[] { '@' });
                                string constraintText = string.Empty;
                                string Contingency = Convert.ToString(splitData[1]);
                                if (marketDateTime > DateTime.Now.AddMinutes(-30))//go back 30 Mins
                                {
                                    bool IsDisgit;
                                    string strchar = arryconstaint[0].Trim();
                                    char lastchar = strchar.Last();
                                    IsDisgit = Char.IsDigit(lastchar);
                                    string lastStr = strchar.Substring(strchar.Length - 2);
                                    if (IsDisgit)
                                        constraintText = arryconstaint[0].Trim() + "_1/" + splitData[5].Trim() + "-" + splitData[6].Trim() + "/" + (splitData[7].Trim() == string.Empty ? "0" : splitData[7].Trim()) + "-" +
                                                               (splitData[8].Trim() == string.Empty ? "0" : splitData[8].Trim());
                                    else if (lastStr.Contains("_"))
                                    {
                                        string[] arrStr = strchar.Split(new char[] { '_' });
                                        constraintText = arrStr[0].Trim() + "__" + arrStr[1].Trim() + "/" + splitData[5].Trim() + "-" + splitData[6].Trim() + "/" + (splitData[7].Trim() == string.Empty ? "0" : splitData[7].Trim()) + "-" +
                                                                  (splitData[8].Trim() == string.Empty ? "0" : splitData[8].Trim());
                                    }
                                    else
                                        constraintText = arryconstaint[0].Trim() + "/" + splitData[5].Trim() + "-" + splitData[6].Trim() + "/" + (splitData[7].Trim() == string.Empty ? "0" : splitData[7].Trim()) + "-" +
                                                                   (splitData[8].Trim() == string.Empty ? "0" : splitData[8].Trim());
                                    string[] constraintTextArray = Regex.Split(constraintText, "/");
                                    string[] ConstraintSourceSinkArray = Regex.Split(constraintTextArray[1], "-");
                                    drConstraint["MarketDateTime"] = Convert.ToDateTime(marketDateTime);
                                    drConstraint["MonitoredText"] = constraintText.Trim();
                                    drConstraint["Contingency"] = Contingency.Trim();
                                    drConstraint["MonitoredElementType"] = Convert.ToString(splitData[4]);
                                    drConstraint["ContingencyDesc"] = Convert.ToString(splitData[2]);
                                    drConstraint["FromStation"] = Convert.ToString(splitData[5]);
                                    drConstraint["ToStation"] = Convert.ToString(splitData[6]);
                                    drConstraint["FromKV"] = Convert.ToInt32((splitData[7].Trim() == string.Empty ? "0" : splitData[7].Trim()));
                                    drConstraint["ToKV"] = Convert.ToInt32(splitData[8].Trim() == string.Empty ? "0" : splitData[8].Trim());
                                    drConstraint["MonitoredID1"] = Convert.ToString(splitData[9]);
                                    drConstraint["MonitoredID2"] = Convert.ToString(splitData[10]);
                                    drConstraint["RatingType"] = Convert.ToString(splitData[11]);
                                    drConstraint["RatingMW"] = Convert.ToDecimal(splitData[12]);
                                    drConstraint["PostCTGFlowMW"] = Convert.ToDecimal(splitData[15]);//13
                                    drConstraint["PercentViolation"] = Convert.ToDecimal(splitData[17]);//16
                                    drConstraint["DSTFlag"] = Convert.ToString(splitData[18]);//17
                                    constraintDT.Rows.Add(drConstraint);

                                    drGeoConstraints["ContingencyText"] = Contingency;
                                    drGeoConstraints["ConstraintText"] = constraintText;
                                    if (constraintHash.ContainsKey(constraintText))
                                    {
                                        List<int> Sourcesink = constraintHash[constraintText];
                                        drGeoConstraints["SourceNodeKey"] = Sourcesink[0];
                                        drGeoConstraints["SinkNodeKey"] = Sourcesink[1];
                                        mDTConstraintGeo.Rows.Add(drGeoConstraints);
                                    }
                                    else
                                    {
                                        SqlCommand SelectNodeCommand = new SqlCommand();
                                        SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[0].Trim() + "%'";
                                        SelectNodeCommand.Connection = DBConnection;
                                        if (DBConnection.State == ConnectionState.Closed)
                                            DBConnection.Open();
                                        SqlDataReader reader1 = SelectNodeCommand.ExecuteReader();
                                        int sourcenodekey = 0;
                                        int sinknodekey = 0;
                                        while (reader1.Read())
                                        {
                                            sourcenodekey = (int)reader1.GetValue(0);
                                            break;
                                        }
                                        reader1.Close();
                                        SelectNodeCommand = new SqlCommand();
                                        SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[1].Trim() + "%'";
                                        SelectNodeCommand.Connection = DBConnection;

                                        if (DBConnection.State == ConnectionState.Closed)
                                            DBConnection.Open();
                                        SqlDataReader reader2 = SelectNodeCommand.ExecuteReader();
                                        while (reader2.Read())
                                        {
                                            sinknodekey = (int)reader2.GetValue(0);
                                            break;
                                        }
                                        reader2.Close();
                                        if (sourcenodekey > 0 && sinknodekey > 0)
                                        {
                                            drGeoConstraints["SourceNodeKey"] = sourcenodekey;
                                            drGeoConstraints["SinkNodeKey"] = sinknodekey;
                                            mDTConstraintGeo.Rows.Add(drGeoConstraints);
                                        }
                                    }
                                }
                                else
                                {
                                    isBreak = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    if (constraintDT.Rows.Count > 0)
                    {
                        Console.WriteLine("Insert NSA Constraint into DB...." + marketDateTime);
                        InsertDB(constraintDT);
                    }
                    if (mDTConstraintGeo.Rows.Count > 0)
                        InsertGeo(mDTConstraintGeo);
                    Console.WriteLine("Deleting Zip File...." + marketDateTime);
                    File.Delete(fileDestName);
                    File.Delete(csvFileName);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static string XMLUnzipFile(string InputPathOfZipFile, string FolderFilename)
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
        public void InsertDB(DataTable DTconstraint)
        {
            try
            {
                if (DBConnection.State == ConnectionState.Closed)
                    DBConnection.Open();
                mDeleteCommand.Connection = DBConnection;
                mDeleteCommand.ExecuteNonQuery();
                SqlTransaction transaction = DBConnection.BeginTransaction();
                using (SqlBulkCopy bkconstraint = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkconstraint.DestinationTableName = "ActiveConstraintsTemp";
                        bkconstraint.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                        bkconstraint.ColumnMappings.Add("MonitoredText", "MonitoredText");
                        bkconstraint.ColumnMappings.Add("Contingency", "Contingency");
                        bkconstraint.ColumnMappings.Add("MonitoredElementType", "MonitoredElementType");
                        bkconstraint.ColumnMappings.Add("ContingencyDesc", "ContingencyDesc");
                        bkconstraint.ColumnMappings.Add("FromStation", "FromStation");
                        bkconstraint.ColumnMappings.Add("ToStation", "ToStation");
                        bkconstraint.ColumnMappings.Add("FromKV", "FromKV");
                        bkconstraint.ColumnMappings.Add("ToKV", "ToKV");
                        bkconstraint.ColumnMappings.Add("MonitoredID1", "MonitoredID1");
                        bkconstraint.ColumnMappings.Add("MonitoredID2", "MonitoredID2");
                        bkconstraint.ColumnMappings.Add("RatingType", "RatingType");
                        bkconstraint.ColumnMappings.Add("RatingMW", "RatingMW");
                        bkconstraint.ColumnMappings.Add("PostCTGFlowMW", "PostCTGFlowMW");
                        bkconstraint.ColumnMappings.Add("PercentViolation", "PercentViolation");
                        bkconstraint.ColumnMappings.Add("DSTFlag", "DSTFlag");
                        bkconstraint.WriteToServer(DTconstraint);
                        SqlCommand UpdateConstraint = new SqlCommand("[dbo].[UpMergeErcotNSAActiveConstraint]", DBConnection, transaction);
                        UpdateConstraint.CommandType = CommandType.StoredProcedure;
                        UpdateConstraint.CommandTimeout = 30000;
                        UpdateConstraint.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                    }
                }
            }
            catch
            {

            }
            finally { DBConnection.Close(); }
        }
        public void InsertGeo(DataTable mDTConstraintGeo)
        {
            try
            {
                if (DBConnection.State == ConnectionState.Closed)
                    DBConnection.Open();
                mDeleteERCOTGeoCommand.Connection = DBConnection;
                mDeleteERCOTGeoCommand.ExecuteNonQuery();
                SqlTransaction transaction = DBConnection.BeginTransaction();
                using (SqlBulkCopy bkGeoconstraint = new SqlBulkCopy(DBConnection, SqlBulkCopyOptions.TableLock, transaction))
                {
                    try
                    {
                        bkGeoconstraint.DestinationTableName = "[dbo].[ConstraintGeoTest]";
                        bkGeoconstraint.ColumnMappings.Add("ContingencyText", "ContingencyText");
                        bkGeoconstraint.ColumnMappings.Add("ConstraintText", "ConstraintText");
                        bkGeoconstraint.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                        bkGeoconstraint.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                        bkGeoconstraint.WriteToServer(mDTConstraintGeo);
                        SqlCommand updatenodelmph = new SqlCommand("[dbo].[UpMergeConstraintERCOTRTGeo]", DBConnection, transaction);
                        updatenodelmph.CommandType = CommandType.StoredProcedure;
                        updatenodelmph.CommandTimeout = 30000;
                        updatenodelmph.ExecuteNonQuery();
                        transaction.Commit();
                        Console.WriteLine("GEO Constarint Updated Successfully...");
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
            catch
            {

            }
        }
        private void HashConstraint()
        {
            Console.WriteLine("Filling Constraint Hash...");
            constraintHash = new Dictionary<string, List<int>>();
            if (DBConnection.State == ConnectionState.Closed)
                DBConnection.Open();
            SqlDataReader reader = mSelectConstraintCommand.ExecuteReader();
            while (reader.Read())
            {
                string Constraint = reader.GetString(0);
                string Contigency = reader.GetString(1);
                if (!constraintHash.ContainsKey(Constraint))
                {
                    List<int> listNodekey = new List<int>();
                    listNodekey.Add(reader.GetInt32(2));
                    listNodekey.Add(reader.GetInt32(3));
                    constraintHash.Add(Constraint, listNodekey);
                }
            }
            reader.Close();
            DBConnection.Close();
        }
    }
}
