//#define TEST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using ICSharpCode.SharpZipLib.Zip;
using Vayu;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Concurrent;
using Vayu.CommonAccessLibrary;

namespace Vayu.ErcotConstraintsDownload
{
    class ErcotConstraintDownload
    {
        private DataTable mERCOTLoadDT = new DataTable();
        private SqlCommand mSelectNodeCommand;
        private DataRow mERCOTLoadRow = null;
        X509Certificate2 Ercotcer = new X509Certificate2();
        private DataTable mERCOTRTLoadDT = new DataTable();
        private DataRow mERCOTRTLoadRow = null;
        private SqlConnection mVayuDBConnection;
        private SqlCommand mDeleteNodeHERCOTCommand;
        private SqlCommand mDeleteNodeHERCOTDACommand;
        private SqlCommand mDeleteERCOTGeoCommand;
        private SqlCommand mDeleteNodeHERCOTRTCommand;
        private Dictionary<string, int> mHashnode = new Dictionary<string, int>();
        private DateTime mDate;
        private DataTable mDTConstraintGeo = new DataTable();
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private StreamWriter mFile = new StreamWriter("log.txt");
        private bool mIsDa;
        private string mDaString;
        private string he;
        Vayu.CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        Vayu.CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        private void LoadDB()
        {
            mVayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            
        }

        private void LoadDBNewTrading()
        {
            mVayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            
        }

        public void initdb()
        {
            userCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = Vayu.CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdServerCert");


            mERCOTLoadDT.Columns.Add("MarketDateTime", typeof(DateTime));
            mERCOTLoadDT.Columns.Add("ConstraintDescription", typeof(string));
            mERCOTLoadDT.Columns.Add("ContingencyText", typeof(string));
            mERCOTLoadDT.Columns.Add("ConstraintText", typeof(string));
            mERCOTLoadDT.Columns.Add("ShadowPrice", typeof(decimal));

            //
#if TEST
            mDeleteNodeHERCOTCommand = new SqlCommand();
            mDeleteNodeHERCOTCommand.CommandText = "delete [dbo].[ConstraintRTTemp_Test]";
#else
            mDeleteNodeHERCOTCommand = new SqlCommand();
            mDeleteNodeHERCOTCommand.CommandText = "delete [dbo].[ConstraintRTTest]";
#endif
            //
            mDeleteNodeHERCOTDACommand = new SqlCommand();
            mDeleteNodeHERCOTDACommand.CommandText = "delete [dbo].[ConstraintGeoTest]";

            //
            mDeleteERCOTGeoCommand = new SqlCommand();
            mDeleteERCOTGeoCommand.CommandText = "delete [dbo].[ConstraintGeoTest]";
            //


            //
            //mDeleteNodeHERCOTRTCommand = new SqlCommand();
            //mDeleteNodeHERCOTRTCommand.CommandText = "delete [TradingData].[dbo].[ConstraintRTERCOTTemp]";

            //
            mDTConstraintGeo.Columns.Add("ContingencyText", typeof(string));
            mDTConstraintGeo.Columns.Add("ConstraintText", typeof(string));
            mDTConstraintGeo.Columns.Add("SourceNodeKey", typeof(int));
            mDTConstraintGeo.Columns.Add("SinkNodeKey", typeof(int));

            //
            mSelectNodeCommand = new SqlCommand();
            mSelectNodeCommand.CommandText = "select distinct nodekey, nodename from node where MarketKey=9";

        }
        public void InitCert()
        {
            #region OLD
            //// ConcurrentDictionary<string, string> connection = DBConnectionHelper.ConnectionHelper.ConnectDB();

            ////mConnection90 = new SqlConnection(connection["TradingData"]);

            //LoadDBNewTrading();
            //SqlCommand selectUserPasswordCommand = new SqlCommand();
            //selectUserPasswordCommand.CommandText = " select CertificateName,Password from Certificate where MarketKey=9 and Application_Name='ErcotDownloadersProd'";
            //selectUserPasswordCommand.Parameters.AddWithValue("@Password", "Password");
            //selectUserPasswordCommand.Connection = mVayuDBConnection;
            //string certFileName = "C:\\Certificates\\Prod\\";
            //string certPasswd = "";
            //if (mVayuDBConnection.State == ConnectionState.Open)
            //{
            //    mVayuDBConnection.Close();
            //}
            //mVayuDBConnection.Open();

            //SqlDataReader reader = selectUserPasswordCommand.ExecuteReader();
            //while (reader.Read())
            //{
            //    certFileName = certFileName + reader.GetString(0);
            //    certPasswd = reader.GetString(1);
            //}
            //reader.Close();
            //mVayuDBConnection.Close();
            //if (certPasswd == "")
            //{
            //    return;
            //}           
            //Ercotcer.Import(certFileName,certPasswd, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
            #endregion OLD
            Ercotcer.Import(userCertificateDetails.Path, userCertificateDetails.Password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);
        }
        public ErcotConstraintDownload(bool isda)
        {
            initdb();
            InitCert();
            HashNode();
            mIsDa = isda;
            
            if (mIsDa)
            {
                DownloadDA();
            }
            else
            {
                mTimer = new System.Timers.Timer();
                mTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
                mTimer.Interval = 20 * 60 * 1000;
                OnTimerEvent(null, null);
                mTimer.Start();
                //while (true)
                //{
                //    Thread.Sleep(10 * 1000);
                //}
                DownloadRT();
            }
        }

        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {

            mTimer.Enabled = false;
            DownloadRT();
            mTimer.Enabled = true;
        }
        private void HashNode()
        {
            mHashnode = new Dictionary<string, int>();
            
            LoadDB();
            mSelectNodeCommand.Connection = mVayuDBConnection;
            if (mVayuDBConnection.State == ConnectionState.Open)
            {
                mVayuDBConnection.Close();
            }
            mVayuDBConnection.Open();
            SqlDataReader reader = mSelectNodeCommand.ExecuteReader();
            while (reader.Read())
            {
                mHashnode.Add(reader.GetString(1), (int)reader.GetInt32(0));
            }
            reader.Close();
            mVayuDBConnection.Close();
        }
        public void DownloadRT()
        {
            try
            {
                string[] filePaths = Directory.GetFiles(@"D:\\ISOFiles\\ERCOTTesting\\RealTimeShadowPrices\\");
                try
                {
                    foreach (string item in filePaths)
                    {
                        File.Delete(item);
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(10000);
                    return;
                }
                //string URL = "http://mis.ercot.com/misapp/GetReports.do?reportTypeId=12302&reportTitle=SCED%20Shadow%20Prices%20and%20Binding%20Transmission%20Constraints&showHTMLView=&mimicKey";
                string URL = "https://www.ercot.com/mp/data-products/data-product-details?id=NP6-86-CD";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.KeepAlive = false;

                request.ProtocolVersion = HttpVersion.Version10;
                request.ClientCertificates.Add(Ercotcer);
                request.ServicePoint.ConnectionLimit = 1;
                request.Method = "GET";
                request.Timeout = 100000;
                HttpWebResponse FileDownloadresponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(FileDownloadresponse.GetResponseStream(), Encoding.UTF8);
                string strLoadForecast = sr.ReadToEnd();
                string strDaString = strLoadForecast.ToString();
                sr.Close();
                //SPChange
                int k = strDaString.IndexOf("var reportTypeID");
                string typeID = strDaString.Substring(k + 20, 5);
                int l = strDaString.IndexOf("var reportListUrl");
                string listURL = strDaString.Substring(l + 21, 68);
                int m = strDaString.IndexOf("var reportDownloadUrl");
                string downloadURL = strDaString.Substring(m + 25, 67);
                HttpWebRequest request1 = (HttpWebRequest)HttpWebRequest.Create(listURL + typeID);
                request1.KeepAlive = false;
                request1.ProtocolVersion = HttpVersion.Version10;
                request1.ClientCertificates.Add(Ercotcer);
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
                bool historicalConstraint = false;
                //bool historicalConstraint = true;
                List<DateTime> constraintDateList = GetExistingDates();
                #region new code
                //SPchange
                for (int p = 1; p <= strArrayList.Length; p++)
                {
                    mERCOTRTLoadDT = new DataTable();
                    mERCOTRTLoadDT.Columns.Add("MarketDateTime", typeof(DateTime));
                    mERCOTRTLoadDT.Columns.Add("ConstraintDescription", typeof(string));
                    mERCOTRTLoadDT.Columns.Add("ContingencyText", typeof(string));
                    mERCOTRTLoadDT.Columns.Add("ConstraintText", typeof(string));
                    mERCOTRTLoadDT.Columns.Add("ShadowPrice", typeof(decimal));
                    mERCOTRTLoadDT.Columns.Add("MaxShadowPrice", typeof(decimal));
                    string dataCsv = strArrayList[p];
                    if (dataCsv.Contains("SCEDBTCNP686_csv.zip"))
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
                        if (filedate1 == DateTime.Today.AddDays(0))
                        {
                            int dockIndex = dataCsv.IndexOf("DocID");
                            string dockID = dataCsv.Substring(dockIndex + 8, 10);
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
                                comparision = comparision.Replace("_csv_zip", ".csv");//D:\ISOFiles\ErcotConstraintsDownload
                                string fileDestName = @"D:\\ISOFiles\\ERCOTTesting\\RealTimeShadowPrices\\" + name;
                                string filedestName = @"D:\\ISOFiles\\ERCOTTesting\\RealTimeShadowPrices\\Imported\\" + name;
                                string folderFilename = @"D:\\ISOFiles\\ERCOTTesting\\RealTimeShadowPrices\\";
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
                                try
                                {
                                    using (StreamReader streamreader = new StreamReader(returnrtfilename))
                                    {
                                        mERCOTLoadDT.Clear();
                                        mDTConstraintGeo.Clear();
                                        string data = streamreader.ReadToEnd();
                                        data = data.Replace("\"", "");
                                        data = data.Replace("\r", "");
                                        data = data.Trim();
                                        string[] split1 = data.Split(new char[] { '\n' });
                                        string constraintText = "";
                                        string contig = "";
                                        for (int i = 1; i < split1.Length; i++)
                                        {
                                            string[] split2 = split1[i].Split(',');
                                            int count = 1;
                                            constraintText = split2[3].Trim() + '/' + split2[10].Trim() + '-' + split2[11].Trim() + '/' + split2[12].Trim() + '-' + split2[13].Trim(); ;
                                            contig = split2[4].Trim();

                                            if (constraintText.Contains("/"))
                                            {
                                                DataRow drConstraints = mDTConstraintGeo.NewRow();
                                                string[] constraintTextArray = Regex.Split(constraintText, "/");
                                                string[] ConstraintSourceSinkArray = Regex.Split(constraintTextArray[1], "-");
                                                drConstraints["ContingencyText"] = contig;
                                                drConstraints["ConstraintText"] = constraintText;
                                                if ((mHashnode.ContainsKey(ConstraintSourceSinkArray[0].Trim())) && (mHashnode.ContainsKey(ConstraintSourceSinkArray[1].Trim())))
                                                {
                                                    drConstraints["SourceNodeKey"] = mHashnode[ConstraintSourceSinkArray[0].Trim()];
                                                    drConstraints["SinkNodeKey"] = mHashnode[ConstraintSourceSinkArray[1].Trim()];
                                                    mDTConstraintGeo.Rows.Add(drConstraints);
                                                }
                                                else
                                                {
                                                    SqlCommand SelectNodeCommand = new SqlCommand();
                                                    SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[0].Trim() + "%'";
                                                    SelectNodeCommand.Connection = mVayuDBConnection;

                                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                                    {
                                                        mVayuDBConnection.Close();
                                                    }
                                                    mVayuDBConnection.Open();
                                                    SqlDataReader reader1 = SelectNodeCommand.ExecuteReader();
                                                    int sourcenodekey = 0;
                                                    int sinknodekey = 0;
                                                    while (reader1.Read())
                                                    {
                                                        sourcenodekey = (int)reader1.GetValue(0);
                                                        break;
                                                    }
                                                    reader1.Close();
                                                    mVayuDBConnection.Close();
                                                    SelectNodeCommand = new SqlCommand();
                                                    SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[1].Trim() + "%'";
                                                    SelectNodeCommand.Connection = mVayuDBConnection;

                                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                                    {
                                                        mVayuDBConnection.Close();
                                                    }
                                                    mVayuDBConnection.Open();
                                                    SqlDataReader reader2 = SelectNodeCommand.ExecuteReader();
                                                    while (reader2.Read())
                                                    {
                                                        sinknodekey = (int)reader2.GetValue(0);
                                                        break;
                                                    }
                                                    reader2.Close();
                                                    mVayuDBConnection.Close();
                                                    if (sourcenodekey > 0 && sinknodekey > 0)
                                                    {
                                                        drConstraints["SourceNodeKey"] = sourcenodekey;
                                                        drConstraints["SinkNodeKey"] = sinknodekey;
                                                        mDTConstraintGeo.Rows.Add(drConstraints);
                                                    }
                                                }
                                            }
                                            mERCOTRTLoadRow = mERCOTRTLoadDT.NewRow();
                                            DateTime dt = Convert.ToDateTime(split2[0]);
                                            decimal shadowPrice = Convert.ToDecimal(split2[5]);
                                            decimal maxSHadowPrice = Convert.ToDecimal(split2[6]);
                                            // dt = dt.AddSeconds(-dt.Second);

                                            {
                                                mERCOTRTLoadRow["MarketDateTime"] = dt;
                                                mERCOTRTLoadRow["ConstraintDescription"] = "";
                                                mERCOTRTLoadRow["ContingencyText"] = contig;
                                                mERCOTRTLoadRow["ConstraintText"] = constraintText;
                                                mERCOTRTLoadRow["ShadowPrice"] = shadowPrice;
                                                mERCOTRTLoadRow["MaxShadowPrice"] = maxSHadowPrice;
                                                mERCOTRTLoadDT.Rows.Add(mERCOTRTLoadRow);
                                            }
                                            count++;
                                        }
                                    }
                                    LoadDB();
                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                    {
                                        mVayuDBConnection.Close();
                                    }
                                    mVayuDBConnection.Open();
                                    SqlTransaction transaction = mVayuDBConnection.BeginTransaction();
                                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mVayuDBConnection, SqlBulkCopyOptions.TableLock, transaction))
                                    {  //
                                        try
                                        {
#if TEST
                                            bkLmpH.DestinationTableName = "[dbo].[ConstraintRTTemp_Test]";
                                            bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                                            bkLmpH.ColumnMappings.Add("ContingencyText", "ContingencyText");
                                            bkLmpH.ColumnMappings.Add("ConstraintText", "ConstraintText");
                                            bkLmpH.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                                            bkLmpH.ColumnMappings.Add("MaxShadowPrice", "MaxShadowPrice");
                                            bkLmpH.WriteToServer(mERCOTRTLoadDT);
                                            SqlCommand updatenodelmph = new SqlCommand("[dbo].[UpMergeERCOTMarginalRT_New_test]", mVayuDBConnection, transaction);

                                            updatenodelmph.CommandType = CommandType.StoredProcedure;
                                            updatenodelmph.CommandTimeout = 30000;
                                            updatenodelmph.ExecuteNonQuery();
                                            transaction.Commit();
#else
                                            bkLmpH.DestinationTableName = "[dbo].[ConstraintRTTest]";
                                            bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                                            bkLmpH.ColumnMappings.Add("ContingencyText", "ContingencyText");
                                            bkLmpH.ColumnMappings.Add("ConstraintText", "ConstraintText");
                                            bkLmpH.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                                            bkLmpH.ColumnMappings.Add("MaxShadowPrice", "MaxShadowPrice");
                                            bkLmpH.WriteToServer(mERCOTRTLoadDT);
                                            SqlCommand updatenodelmph = new SqlCommand("[dbo].[UpMergeERCOTMarginalRT_New]", mVayuDBConnection, transaction);

                                            updatenodelmph.CommandType = CommandType.StoredProcedure;
                                            updatenodelmph.CommandTimeout = 30000;
                                            updatenodelmph.ExecuteNonQuery();
                                            transaction.Commit();
#endif
                                        }
                                        catch (Exception ex)
                                        {
                                            transaction.Rollback();
                                        }
                                    }
                                    mVayuDBConnection.Close();
                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                    {
                                        mVayuDBConnection.Close();
                                    }
                                    mVayuDBConnection.Open();
                                    mDeleteERCOTGeoCommand.Connection = mVayuDBConnection;
                                    mDeleteERCOTGeoCommand.ExecuteNonQuery();
                                    SqlTransaction transaction1 = mVayuDBConnection.BeginTransaction();
                                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mVayuDBConnection, SqlBulkCopyOptions.TableLock, transaction1))
                                    {
                                        try
                                        {

                                            bkLmpH.DestinationTableName = "[dbo].[ConstraintGeoTest]";
                                            bkLmpH.ColumnMappings.Add("ContingencyText", "ContingencyText");
                                            bkLmpH.ColumnMappings.Add("ConstraintText", "ConstraintText");
                                            bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                                            bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                                            bkLmpH.WriteToServer(mDTConstraintGeo);
                                            SqlCommand updatenodelmph = new SqlCommand("[dbo].[UpMergeConstraintERCOTRTGeo]", mVayuDBConnection, transaction1);
                                            updatenodelmph.CommandType = CommandType.StoredProcedure;
                                            updatenodelmph.CommandTimeout = 30000;
                                            updatenodelmph.ExecuteNonQuery();
                                            transaction1.Commit();

                                        }
                                        catch (Exception ex)
                                        {
                                            transaction1.Rollback();
                                        }
                                    }
                                    mVayuDBConnection.Close();
                                    Console.WriteLine(fileDestName);
                                    File.Delete(fileDestName);
                                    File.Delete(returnrtfilename);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }


                    }
                }
                #endregion

               
            }

            catch (Exception ex)
            {
            }
        }

        private List<DateTime> GetExistingDates()
        {
            List<DateTime> constraintDateList = new List<DateTime>();
            try
            {
                if (mVayuDBConnection.State == ConnectionState.Closed)
                {
                    mVayuDBConnection.Open();
                }
                SqlCommand cmd = mVayuDBConnection.CreateCommand();
                cmd.CommandText = "select distinct marketdatetime from ConstraintRT where MarketDateTime > '" + DateTime.Today.AddDays(-10).ToString() + "' order by MarketDateTime desc";
                cmd.Connection = mVayuDBConnection;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DateTime marketdatetime = Convert.ToDateTime(rdr.GetValue(0));
                    constraintDateList.Add(marketdatetime);
                }
                rdr.Close();
                mVayuDBConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return constraintDateList;
        }
        public void InsertGeo()
        {
            List<ConstraintContigency> list = new List<ConstraintContigency>();
            SqlCommand SelectCommand = new SqlCommand();
            SelectCommand.CommandText = "select distinct ConstraintText, ContingencyText from Constraintda";
            SelectCommand.Connection = mVayuDBConnection;

            if (mVayuDBConnection.State == ConnectionState.Open)
            {
                mVayuDBConnection.Close();
            }
            mVayuDBConnection.Open();
            SqlDataReader reader = SelectCommand.ExecuteReader();
            while (reader.Read())
            {
                ConstraintContigency mConstraintContigency = new ConstraintContigency();
                mConstraintContigency.Constraints = reader.GetString(0);
                mConstraintContigency.Contigency = reader.GetString(1);
                list.Add(mConstraintContigency);
            }
            reader.Close();

            foreach (ConstraintContigency item in list)
            {


                if (item.Constraints.Contains("/"))
                {
                    DataRow drConstraints = mDTConstraintGeo.NewRow();
                    string[] constraintTextArray = Regex.Split(item.Constraints, "/");
                    string[] ConstraintSourceSinkArray = Regex.Split(constraintTextArray[1], "-");
                    drConstraints["ContingencyText"] = item.Contigency;
                    drConstraints["ConstraintText"] = item.Constraints;
                    if ((mHashnode.ContainsKey(ConstraintSourceSinkArray[0].Trim())) && (mHashnode.ContainsKey(ConstraintSourceSinkArray[1].Trim())))
                    {
                        drConstraints["SourceNodeKey"] = mHashnode[ConstraintSourceSinkArray[0].Trim()];
                        drConstraints["SinkNodeKey"] = mHashnode[ConstraintSourceSinkArray[1].Trim()];
                        mDTConstraintGeo.Rows.Add(drConstraints);
                    }
                    else
                    {
                        SqlCommand SelectNodeCommand = new SqlCommand();
                        SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[0].Trim() + "%'";
                        SelectNodeCommand.Connection = mVayuDBConnection;

                        if (mVayuDBConnection.State == ConnectionState.Open)
                        {
                            mVayuDBConnection.Close();
                        }
                        mVayuDBConnection.Open();
                        SqlDataReader reader1 = SelectNodeCommand.ExecuteReader();
                        int sourcenodekey = 0;
                        int sinknodekey = 0;
                        while (reader1.Read())
                        {
                            sourcenodekey = (int)reader1.GetValue(0);
                            break;
                        }
                        reader1.Close();
                        mVayuDBConnection.Close();
                        SelectNodeCommand = new SqlCommand();
                        SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[1].Trim() + "%'";
                        SelectNodeCommand.Connection = mVayuDBConnection;

                        if (mVayuDBConnection.State == ConnectionState.Open)
                        {
                            mVayuDBConnection.Close();
                        }
                        mVayuDBConnection.Open();
                        SqlDataReader reader2 = SelectNodeCommand.ExecuteReader();
                        while (reader2.Read())
                        {
                            sinknodekey = (int)reader2.GetValue(0);
                            break;
                        }
                        reader2.Close();
                        mVayuDBConnection.Close();
                        if (sourcenodekey > 0 && sinknodekey > 0)
                        {
                            drConstraints["SourceNodeKey"] = sourcenodekey;
                            drConstraints["SinkNodeKey"] = sinknodekey;
                            mDTConstraintGeo.Rows.Add(drConstraints);
                        }
                    }
                }
            }
            if (mVayuDBConnection.State == ConnectionState.Open)
            {
                mVayuDBConnection.Close();
            }
            mVayuDBConnection.Open();
            SqlTransaction transaction1 = mVayuDBConnection.BeginTransaction();
            using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mVayuDBConnection, SqlBulkCopyOptions.TableLock, transaction1))
            {
                try
                {

                    bkLmpH.DestinationTableName = "[dbo].[ConstraintGeoTest]";
                    bkLmpH.ColumnMappings.Add("ContingencyText", "ContingencyText");
                    bkLmpH.ColumnMappings.Add("ConstraintText", "ConstraintText");
                    bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                    bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                    bkLmpH.WriteToServer(mDTConstraintGeo);
                    SqlCommand updatenodelmph = new SqlCommand("[dbo].[UpMergeConstraintERCOTRTGeo]", mVayuDBConnection, transaction1);
                    updatenodelmph.CommandType = CommandType.StoredProcedure;
                    updatenodelmph.CommandTimeout = 30000;
                    updatenodelmph.ExecuteNonQuery();
                    transaction1.Commit();

                }
                catch (Exception ex)
                {
                    transaction1.Rollback();
                }
            }
            mVayuDBConnection.Close();
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
        public void DownloadDA()
        {
            try
            {
                //string URL = "http://mis.ercot.com/misapp/GetReports.do?reportTypeId=12332&reportTitle=DAM%20Shadow%20Prices&showHTMLView=&mimicKey";
                string URL = "https://www.ercot.com/mp/data-products/data-product-details?id=NP4-191-CD";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ClientCertificates.Add(Ercotcer);
                request.ServicePoint.ConnectionLimit = 1;
                request.Method = "GET";
                request.Timeout = 100000;
                HttpWebResponse FileDownloadresponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(FileDownloadresponse.GetResponseStream(), Encoding.UTF8);
                string strLoadForecast = sr.ReadToEnd();
                string strDaString = strLoadForecast.ToString();
                sr.Close();
                //SPChange
                int k = strDaString.IndexOf("var reportTypeID");
                string typeID = strDaString.Substring(k + 20, 5);
                int l = strDaString.IndexOf("var reportListUrl");
                string listURL = strDaString.Substring(l + 21, 68);
                int m = strDaString.IndexOf("var reportDownloadUrl");
                string downloadURL = strDaString.Substring(m + 25, 67);
                HttpWebRequest request1 = (HttpWebRequest)HttpWebRequest.Create(listURL + typeID);
                request1.KeepAlive = false;
                request1.ProtocolVersion = HttpVersion.Version10;
                request1.ClientCertificates.Add(Ercotcer);
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
                #region new code
                //SPchange
                for (int p = 1; p < strArrayList.Length; p++)
                {
                    string dataCsv = strArrayList[p];
                    if (dataCsv.Contains("DASPBCNP4191_csv.zip"))
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
                        if (filedate1 == DateTime.Today.AddDays(-1))
                        {
                            int dockIndex = dataCsv.IndexOf("DocID");
                            string dockID = dataCsv.Substring(dockIndex + 8, 10);
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
                                string fileDestName = @"D:\ISOFiles\ERCOTTesting\DayAheadShadowPrices\" + name;
                                string filedestName = "D:\\ISOFiles\\ERCOTTesting\\DayAheadShadowPrices\\Imported\\" + name;
                                string folderFilename = @"D:\ISOFiles\ERCOTTesting\DayAheadShadowPrices\";
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
                                string returnfilename = CSVUnZipFile(fileDestName, folderFilename);
                                try
                                {
                                    using (StreamReader streamreader = new StreamReader(returnfilename))
                                    {
                                        mERCOTLoadDT.Clear();
                                        mDTConstraintGeo.Clear();
                                        string data = streamreader.ReadToEnd();
                                        data = data.Replace("\"", "");
                                        data = data.Replace("\r", "");
                                        data = data.Trim();
                                        string constraintname = "";
                                        string conting = "";
                                        string[] split1 = data.Split(new char[] { '\n' });
                                        for (int i = 1; i < split1.Length; i++)
                                        {
                                            string[] split2 = split1[i].Split(',');
                                            int count = 1;
                                            DataRow mERCOTLoadRow = mERCOTLoadDT.NewRow();
                                            constraintname = split2[3].Trim() + '/' + split2[9].Trim() + '-' + split2[10].Trim() + '/' + split2[11].Trim() + '-' + split2[12].Trim(); ;
                                            conting = split2[4].Trim();
                                            mERCOTLoadRow["ContingencyText"] = conting;
                                            mERCOTLoadRow["ConstraintText"] = constraintname;
                                            mERCOTLoadRow["ConstraintDescription"] = constraintname;
                                            if (constraintname.Contains("/"))
                                            {
                                                DataRow drConstraints = mDTConstraintGeo.NewRow();
                                                string[] constraintTextArray = Regex.Split(constraintname, "/");
                                                string[] ConstraintSourceSinkArray = Regex.Split(constraintTextArray[1], "-");
                                                drConstraints["ContingencyText"] = conting;
                                                drConstraints["ConstraintText"] = constraintname;
                                                if ((mHashnode.ContainsKey(ConstraintSourceSinkArray[0].Trim())) && (mHashnode.ContainsKey(ConstraintSourceSinkArray[1].Trim())))
                                                {
                                                    drConstraints["SourceNodeKey"] = mHashnode[ConstraintSourceSinkArray[0].Trim()];
                                                    drConstraints["SinkNodeKey"] = mHashnode[ConstraintSourceSinkArray[1].Trim()];
                                                    mDTConstraintGeo.Rows.Add(drConstraints);
                                                }
                                                else
                                                {
                                                    SqlCommand SelectNodeCommand = new SqlCommand();
                                                    SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[0].Trim() + "%'";
                                                    SelectNodeCommand.Connection = mVayuDBConnection;

                                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                                    {
                                                        mVayuDBConnection.Close();
                                                    }
                                                    mVayuDBConnection.Open();
                                                    SqlDataReader reader1 = SelectNodeCommand.ExecuteReader();
                                                    int sourcenodekey = 0;
                                                    int sinknodekey = 0;
                                                    while (reader1.Read())
                                                    {
                                                        sourcenodekey = (int)reader1.GetValue(0);
                                                        break;
                                                    }
                                                    reader1.Close();
                                                    mVayuDBConnection.Close();
                                                    SelectNodeCommand = new SqlCommand();
                                                    SelectNodeCommand.CommandText = " select nodekey from node where MarketKey=9 and nodename like " + "'%" + ConstraintSourceSinkArray[1].Trim() + "%'";
                                                    SelectNodeCommand.Connection = mVayuDBConnection;

                                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                                    {
                                                        mVayuDBConnection.Close();
                                                    }
                                                    mVayuDBConnection.Open();
                                                    SqlDataReader reader2 = SelectNodeCommand.ExecuteReader();
                                                    while (reader2.Read())
                                                    {
                                                        sinknodekey = (int)reader2.GetValue(0);
                                                        break;
                                                    }
                                                    reader2.Close();
                                                    mVayuDBConnection.Close();
                                                    if (sourcenodekey > 0 && sinknodekey > 0)
                                                    {
                                                        drConstraints["SourceNodeKey"] = sourcenodekey;
                                                        drConstraints["SinkNodeKey"] = sinknodekey;
                                                        mDTConstraintGeo.Rows.Add(drConstraints);
                                                    }
                                                }
                                            }
                                            mERCOTLoadRow["ShadowPrice"] = Convert.ToDecimal(split2[8]);
                                            var hr = split2[1].ToString();
                                            int hour = Convert.ToInt32(hr.Substring(0, 2));
                                            mERCOTLoadRow["MarketDateTime"] = Convert.ToDateTime(split2[0]).AddHours(hour);
                                            mERCOTLoadDT.Rows.Add(mERCOTLoadRow);
                                            count++;
                                        }
                                    }


                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                    {
                                        mVayuDBConnection.Close();
                                    }
                                    LoadDB();
                                    mDeleteNodeHERCOTDACommand.Connection = mVayuDBConnection;
                                    mDeleteNodeHERCOTDACommand.ExecuteNonQuery();
                                    SqlTransaction transaction = mVayuDBConnection.BeginTransaction();
                                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mVayuDBConnection, SqlBulkCopyOptions.TableLock, transaction))
                                    {
                                        try
                                        {
                                            bkLmpH.DestinationTableName = "[dbo].[ConstraintDATest]";
                                            bkLmpH.ColumnMappings.Add("MarketDateTime", "MarketDateTime");
                                            bkLmpH.ColumnMappings.Add("ContingencyText", "ContingencyText");
                                            bkLmpH.ColumnMappings.Add("ConstraintText", "ConstraintText");
                                            bkLmpH.ColumnMappings.Add("ShadowPrice", "ShadowPrice");
                                            bkLmpH.WriteToServer(mERCOTLoadDT);
                                            SqlCommand updatenodelmph = new SqlCommand("[dbo].[UpMergeERCOTMarginalDA_new]", mVayuDBConnection, transaction);
                                            updatenodelmph.CommandType = CommandType.StoredProcedure;
                                            updatenodelmph.CommandTimeout = 30000;
                                            updatenodelmph.ExecuteNonQuery();
                                            transaction.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            transaction.Rollback();
                                            // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                                        }
                                    }
                                    mVayuDBConnection.Close();
                                    if (mVayuDBConnection.State == ConnectionState.Open)
                                    {
                                        mVayuDBConnection.Close();
                                    }
                                    mVayuDBConnection.Open();
                                    mDeleteERCOTGeoCommand.Connection = mVayuDBConnection;
                                    mDeleteERCOTGeoCommand.ExecuteNonQuery();
                                    SqlTransaction transaction1 = mVayuDBConnection.BeginTransaction();
                                    using (SqlBulkCopy bkLmpH = new SqlBulkCopy(mVayuDBConnection, SqlBulkCopyOptions.TableLock, transaction1))
                                    {
                                        try
                                        {
                                            bkLmpH.DestinationTableName = "[dbo].[ConstraintGeoTest]";
                                            bkLmpH.ColumnMappings.Add("ContingencyText", "ContingencyText");
                                            bkLmpH.ColumnMappings.Add("ConstraintText", "ConstraintText");
                                            bkLmpH.ColumnMappings.Add("SourceNodeKey", "SourceNodeKey");
                                            bkLmpH.ColumnMappings.Add("SinkNodeKey", "SinkNodeKey");
                                            bkLmpH.WriteToServer(mDTConstraintGeo);
                                            SqlCommand updatenodelmph = new SqlCommand("[dbo].[UpMergeConstraintERCOTRTGeo]", mVayuDBConnection, transaction1);
                                            updatenodelmph.CommandType = CommandType.StoredProcedure;
                                            updatenodelmph.CommandTimeout = 30000;
                                            updatenodelmph.ExecuteNonQuery();
                                            transaction1.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            transaction1.Rollback();
                                            //  mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                                        }
                                    }
                                    mVayuDBConnection.Close();
                                    File.Delete(fileDestName);
                                    File.Delete(returnfilename);
                                }
                                //
                                catch (Exception ex)
                                {
                                    // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
                                }
                            }
                        }
                    }

                }
                #endregion
            }
            catch (Exception ex)
            {
                // mApplicationLog.UpdateError(System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }
    }
    public class ConstraintContigency
    {
        public string Constraints { get; set; }
        public string Contigency { get; set; }
    }
}

