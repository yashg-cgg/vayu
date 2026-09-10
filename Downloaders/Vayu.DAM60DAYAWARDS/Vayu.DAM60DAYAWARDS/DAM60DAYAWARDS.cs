using Vayu.CommonAccessLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;


namespace Vayu.DAM60DAYAWARDS
{
    class DAM60DAYAWARDS
    {

        SqlConnection  VayuDbConn;
       
        private DataTable stDAMDataTable = new DataTable();
        private DataRow stRowDAMDT = null;
        private X509Certificate2 mCert = new X509Certificate2();
        CertificateInfoLibrary.CertificateHeler userCertificateDetails = null;
        CertificateInfoLibrary.CertificateHeler serverCertificateDetails = null;
        SqlCommand mSelectNodeKey = new SqlCommand();
        SqlCommand mSelectParticipateKey = new SqlCommand();
        SqlCommand mInsertCommand = new SqlCommand();
        SqlCommand CmdGetParticipantID = new SqlCommand();


        public DAM60DAYAWARDS()
        {
            InitDB();
            InitCert();
            DateTime target = DateTime.Today.AddDays(-1);
            
            while (target < DateTime.Today)
            {
                DownloadAwards(target);
                target = target.AddDays(1);
            }
           
        }
        public void InitDB()
        {
             VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();
            
            userCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientAPI");
            serverCertificateDetails = CertificateInfoLibrary.CertificateInfo.GetCertificateDetails(9, "ProdClientCert");

            stDAMDataTable.Columns.Add("DeliveryDate", typeof(DateTime));//0
            stDAMDataTable.Columns.Add("Hour", typeof(int));//0
            stDAMDataTable.Columns.Add("ParticipantID", typeof(int));//2
            stDAMDataTable.Columns.Add("SourceKey", typeof(int));//3
            stDAMDataTable.Columns.Add("SinkKey", typeof(int));//4
            stDAMDataTable.Columns.Add("MW", typeof(decimal));//5
            stDAMDataTable.Columns.Add("Price", typeof(decimal));//6
            stDAMDataTable.Columns.Add("BidID", typeof(string));//6
            

            mSelectNodeKey = new SqlCommand();
            mSelectNodeKey.CommandText = "select distinct NodeName, Nodekey from Vayu..Node";
            mSelectNodeKey.Connection =  VayuDbConn;

            mSelectParticipateKey = new SqlCommand();
            mSelectParticipateKey.CommandText = "select  Distinct Participant, ParticipantID from Vayu..Company where Marketkey=9";
            mSelectParticipateKey.Connection =  VayuDbConn;

            mInsertCommand = new SqlCommand();
            mInsertCommand.CommandText = "Insert Vayu..Company values(Null,@Participant,9,Null)";
            mInsertCommand.Parameters.AddWithValue("@Participant", "Participant");
            mInsertCommand.Connection =  VayuDbConn;

            CmdGetParticipantID = new SqlCommand();
            CmdGetParticipantID.CommandText = "Select ParticipantID from Vayu..Company where Marketkey=9 and Participant= @Participant";
            CmdGetParticipantID.Parameters.AddWithValue("@Participant", "Participant");
            CmdGetParticipantID.Connection =  VayuDbConn;

        }

        private void DownloadAwards(DateTime date)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(@"D:\ISOFiles\DAM60DAYAWARDS\");
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
               // DateTime date = DateTime.Today.AddDays(-1);
                string strDate = date.ToString("yyyyMMdd");
                //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=13113&reportTitle=DAM%20System%20Lambda&showHTMLView=&mimicKey");
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://mis.ercot.com/misapp/GetReports.do?reportTypeId=13051");
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.ConnectionLimit = 1;
                request.CookieContainer = new CookieContainer();
                request.Method = "GET";
                request.ClientCertificates.Add(mCert);
                request.Timeout = 100000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string strnodedalmp = sr.ReadToEnd();
                strnodedalmp = strnodedalmp.Substring(strnodedalmp.IndexOf("<b>60-Day DAM Disclosure Reports</b></"));
                string[] splinelitRow = strnodedalmp.Split(new string[] { "class='labelOptional'><div align='center'>" }, StringSplitOptions.RemoveEmptyEntries);
                // string[] splinelitRow = strnodedalmp.Split(new string[] { "<td class='labelOptional_ind'>ext" }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < splinelitRow.Length; j=j+3)
                {
                    if (splinelitRow[j].Contains(strDate))
                    {
                        string FileURL = "https://mis.ercot.com/" + splinelitRow[3+j].Substring(10, 80);
                        string FileName = "D:\\ISOFiles\\DAM60DAYAWARDS\\Ercot_" + FileURL.Substring(71, 9) + ".zip";
                        HttpWebRequest FileDownloadRequest = (HttpWebRequest)HttpWebRequest.Create(FileURL);
                        FileDownloadRequest.ClientCertificates.Add(mCert);
                        FileDownloadRequest.Timeout = 100000;
                        FileDownloadRequest.Method = "GET";

                        HttpWebResponse FileDownloadresponse = (HttpWebResponse)FileDownloadRequest.GetResponse();
                        string fileDestName = FileName;
                        string folderFilename = @"D:\ISOFiles\DAM60DAYAWARDS\";
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
                        int k = 0;
                        Dictionary<string, int> NodeKeys = new Dictionary<string, int>();
                        Dictionary<string, int> ParticipantIDs = new Dictionary<string, int>();
                        
                        string returnfilename = CSVUnZipFile(fileDestName, folderFilename);
                         
                        try
                        {
                            stDAMDataTable.Clear();
                            NodeKeys = GetSourceSinkKey();
                            using (StreamReader streamreader = new StreamReader(returnfilename))
                            {
                                
                                string data = streamreader.ReadToEnd();
                                data = data.Replace("\"", "");
                                data = data.Replace("\r", "");
                                string[] split1 = data.Split(new char[] { '\n' });
                                int count = 0;
                                for (int i = 1; i < split1.Length; i++)
                                {
                                    k = i;
                                  if(k == 173291) 
                                    { }
                                    if (count == 0)
                                    {
                                        ParticipantIDs = GetParticipantID();

                                    }
 
                                    string[] split2 = split1[i].Split(',');
                                    if (split2.Length > 1)
                                    {

                                        DateTime Date = Convert.ToDateTime(split2[0]);
                                        int Hour = Convert.ToInt32(split2[1]);
                                        Date = Date.AddHours(Hour);
                                        string Participant = split2[2];
                                        string Source = split2[3];
                                        string Sink = split2[4];
                                        decimal MW = Convert.ToDecimal(split2[5]);
                                        decimal Price = Convert.ToDecimal(split2[6]);
                                        int SourceKey = 0;
                                        int SinkKey = 0;
                                        int ParticipantID = 0;
                                        if (NodeKeys.ContainsKey(Source))
                                        {
                                            SourceKey = NodeKeys[Source];
                                        }
                                        if (NodeKeys.ContainsKey(Sink))
                                        {
                                            SinkKey = NodeKeys[Sink];
                                        }
                                        if (split2[7] == "AZ02" && stDAMDataTable.Rows.Count > 84300)
                                        {

                                        }
                                        if (ParticipantIDs.ContainsKey(Participant))
                                        {
                                            ParticipantID = ParticipantIDs[Participant];
                                            count = 1;
                                        }
                                        else
                                        {
                                            if (VayuDbConn.State == ConnectionState.Open)
                                            {
                                                VayuDbConn.Close();
                                            }
                                            VayuDbConn.Open();
                                            mInsertCommand.Parameters["@Participant"].Value = Participant;
                                            mInsertCommand.ExecuteNonQuery();

                                            CmdGetParticipantID.Parameters["@Participant"].Value = Participant;
                                            SqlDataReader reader = CmdGetParticipantID.ExecuteReader();
                                            while (reader.Read())
                                            {
                                                ParticipantID = reader.GetInt32(0);
                                            }
                                            reader.Close();
                                            VayuDbConn.Close();
                                            count = 0;
                                        }
                                        stRowDAMDT = stDAMDataTable.NewRow();
                                        stRowDAMDT["DeliveryDate"] = Date;
                                        stRowDAMDT["Hour"] = Hour;
                                        stRowDAMDT["ParticipantID"] = ParticipantID;
                                        stRowDAMDT["SourceKey"] = SourceKey;
                                        stRowDAMDT["SinkKey"] = SinkKey;
                                        stRowDAMDT["MW"] = MW;
                                        stRowDAMDT["Price"] = Price;
                                        stRowDAMDT["BidID"] = split2[7];
                                        if (MW > 0)
                                        {
                                            stDAMDataTable.Rows.Add(stRowDAMDT);
                                        } 
                                    }
                                }

                                InsertDB(stDAMDataTable);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(k);
                        }
                        
                    }
                   
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void InsertDB(DataTable  DAMTable)
        {
            if ( VayuDbConn.State == ConnectionState.Open)
            {
                 VayuDbConn.Close();
            }
             VayuDbConn.Open();
            SqlTransaction transaction =  VayuDbConn.BeginTransaction();
            using(SqlBulkCopy bkDam = new SqlBulkCopy( VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    int count = DAMTable.Rows.Count;
                    Console.WriteLine("data count ", +count);
                    bkDam.DestinationTableName= "dbo.DAM60DAYPTPAWARDS_Test";
                    bkDam.ColumnMappings.Add("DeliveryDate", "DeliveryDate");
                    bkDam.ColumnMappings.Add("Hour", "Hour");
                    bkDam.ColumnMappings.Add("ParticipantID", "ParticipantID");
                    bkDam.ColumnMappings.Add("SourceKey", "SourceKey");
                    bkDam.ColumnMappings.Add("SinkKey", "SinkKey");
                    bkDam.ColumnMappings.Add("MW", "MW");
                    bkDam.ColumnMappings.Add("Price", "Price");
                    bkDam.ColumnMappings.Add("BidID", "BidID");
                    bkDam.WriteToServer(DAMTable);
                    transaction.Commit();

                }
                catch(Exception ex)
                {
                    transaction.Rollback();

                }
            }
             VayuDbConn.Close();

            if ( VayuDbConn.State == ConnectionState.Open)
            {
                 VayuDbConn.Close();
            }
            try
            {
                 VayuDbConn.Open();
                SqlTransaction Upmerge =  VayuDbConn.BeginTransaction();
                using (SqlBulkCopy bkData = new SqlBulkCopy( VayuDbConn, SqlBulkCopyOptions.TableLock, Upmerge))
                {
                    try
                    {
                        if (DAMTable.Rows.Count > 0)
                        {
                            SqlCommand mUpmarge = new SqlCommand("[UpMergeDAM60DayPTP]",  VayuDbConn, Upmerge);
                            mUpmarge.CommandType = CommandType.StoredProcedure;
                            mUpmarge.CommandTimeout = 50000;
                            mUpmarge.ExecuteNonQuery();
                            Upmerge.Commit();


                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            catch(Exception ex)
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
                            if (theEntry.Name.Contains("60d_DAM_PTPObligationBidAwards"))
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



       

        private Dictionary<string, int>GetSourceSinkKey()
        {
            Dictionary<string, int> KeyDic = new Dictionary<string, int>();
           // InitDB();
            if( VayuDbConn.State==ConnectionState.Open)
            {
                 VayuDbConn.Close();
            }
             VayuDbConn.Open();
            mSelectNodeKey.Connection =  VayuDbConn;
            SqlDataReader reader = mSelectNodeKey.ExecuteReader();
            while (reader.Read())
            {
                string Nodename = reader.GetValue(0).ToString();
                int Nodekey = reader.GetInt32(1);
                KeyDic.Add(Nodename, Nodekey);
            }



            return KeyDic;
        }
        private Dictionary<string, int>GetParticipantID()
        {
            Dictionary<string, int> KeyDict = new Dictionary<string, int>();
            try
            {
                if ( VayuDbConn.State == ConnectionState.Open)
                {
                     VayuDbConn.Close();
                }
                 VayuDbConn.Open();
                mSelectParticipateKey.Connection =  VayuDbConn;
                SqlDataReader reader = mSelectParticipateKey.ExecuteReader();
                while (reader.Read())
                {
                    string Participant = reader.GetValue(0).ToString();
                    int ID = reader.GetInt32(1);
                    KeyDict.Add(Participant, ID);

                }
            }
            catch(Exception ex)
            {

            }

            return KeyDict;
        }

        public void InitCert()
        {
            mCert.Import(userCertificateDetails.Path, userCertificateDetails.Password, X509KeyStorageFlags.MachineKeySet);
        }
    }
}
