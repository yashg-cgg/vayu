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
using Vayu.CommonAccessLibrary;
namespace Vayu.ERCOTOutagesDownload
{
    class ErcotOutageLoad
    {
         private SqlConnection Vayudbconnection;
        
        private SqlCommand mInsertErcotOutagesCommand;
        private SqlCommand mUpdateErcotOutagesCommand;
        private SqlCommand mSelectErcotOutagesCommand;
        private SqlCommand mSelectRemoveErcotOutagesCommand;        
        System.Timers.Timer mTimer = new System.Timers.Timer();
        private List<Outages> mOutageList = new List<Outages>();
        private X509Certificate2 mCert = new X509Certificate2();
        private string DestinationZipFilePath = @"D:\ISOFiles\ERCOTOutagesDownload\ERCOTOutages\Completed\";
        public ErcotOutageLoad()
        {
            InitDB();           
            OnTimedEvent(null, null);
            StartTimer();
        }
        public void StartTimer()
        {
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 60000;
            mTimer.Enabled = true;
            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') ;
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            DownloadOutages();
            mTimer.Enabled = true;
        }

        public void InitDB()
        {
            Vayudbconnection = new VayuDBConnection().GetInstance().GetSqlConnection();
                  
            mSelectRemoveErcotOutagesCommand= new SqlCommand();
            mSelectRemoveErcotOutagesCommand.CommandText = "select OutageIdentifier, PlannedStartDate, PlannedEndDate, ActualStartDate, ActualEndDate, OutageStatus, RequestorOrgName, EquipmentType, EquipmentName, EquipmentFromStationName, EquipmentToStationName, VoltageLevel,  SubmitTime, OutageType from ERCOT_OUTAGES_temp where RemovedDate is null or RemovedDate=''";                       
            mSelectRemoveErcotOutagesCommand.Connection = Vayudbconnection;
            //
            mSelectErcotOutagesCommand = new SqlCommand();
            mSelectErcotOutagesCommand.CommandText = "select top 1 * from ERCOT_OUTAGES_temp where OutageIdentifier=@OutageIdentifier and EquipmentName=@EquipmentName and PlannedStartDate=@PlannedStartDate and EquipmentType=@EquipmentType order by revisedDate desc";
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@OutageIdentifier", "OutageIdentifier");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@EquipmentName", "EquipmentName");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@PlannedStartDate", "PlannedStartDate");
            mSelectErcotOutagesCommand.Parameters.AddWithValue("@EquipmentType", "EquipmentType");            
            mSelectErcotOutagesCommand.Connection = Vayudbconnection;

            //
            mInsertErcotOutagesCommand = new SqlCommand();
            mInsertErcotOutagesCommand.CommandText = "insert into ERCOT_OUTAGES_temp (OutageIdentifier,PlannedStartDate,PlannedEndDate,ActualStartDate,ActualEndDate,OutageStatus,RequestorOrgName,EquipmentType,EquipmentName,EquipmentFromStationName,EquipmentToStationName,VoltageLevel,SubmitTime,OutageType,RemovedDate,RevisedDate) values(@OutageIdentifier,@PlannedStartDate,@PlannedEndDate,@ActualStartDate,@ActualEndDate,@OutageStatus,@RequestorOrgName,@EquipmentType,@EquipmentName,@EquipmentFromStationName,@EquipmentToStationName,@VoltageLevel,@SubmitTime,@OutageType, @RemovedDate,@RevisedDate)";
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@OutageIdentifier", "OutageIdentifier");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@PlannedStartDate", "PlannedStartDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@PlannedEndDate", "PlannedEndDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@ActualStartDate", "ActualStartDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@ActualEndDate", "ActualEndDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@OutageStatus", "OutageStatus");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@RequestorOrgName", "RequestorOrgName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentType", "EquipmentType");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentName", "EquipmentName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentFromStationName", "EquipmentFromStationName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@EquipmentToStationName", "EquipmentToStationName");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@VoltageLevel", "VoltageLevel");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@SubmitTime", "SubmitTime");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@OutageType", "OutageType");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@RemovedDate", "RemovedDate");
            mInsertErcotOutagesCommand.Parameters.AddWithValue("@RevisedDate", "RevisedDate");            
            mInsertErcotOutagesCommand.Connection = Vayudbconnection;
            //         
         
        }
        public void DownloadOutages()
        {
            try
            {
               
                string FileDestFolder = @"D:\ISOFiles\ERCOTOutagesDownload\ERCOTOutages\";
                string[] filePaths = Directory.GetFiles(FileDestFolder);
                for (int k = 0; k < filePaths.Length; k++)
                {
                    FillList();
                    string FolderFilename = filePaths[k].Substring(16, (filePaths[k].IndexOf(".zip") - 16));
                    string DestinationFolderFilename = FileDestFolder + FolderFilename;
                    Directory.CreateDirectory(DestinationFolderFilename);
                    XML_UnZipFile(filePaths[k], FolderFilename);                
                    string[] DestinationFolderfilePaths = Directory.GetFiles(DestinationFolderFilename);
                    foreach (string FileName in DestinationFolderfilePaths)
                    {
                        using (StreamReader streamreader = new StreamReader(FileName))
                            {

                                List<Outages> outagesList = new List<Outages>();
                                string data = streamreader.ReadToEnd();
                                data = data.Replace("\"", "");
                                data = data.Trim();
                                DateTime plannedStartDate = Convert.ToDateTime("2012-05-01");
                                DateTime plannedEndDate = Convert.ToDateTime("2012-05-01");
                                DateTime actualStartDate = Convert.ToDateTime("2012-05-01");
                                DateTime actualEndDate = Convert.ToDateTime("2012-05-01");

                                DateTime submitTime = Convert.ToDateTime("2012-05-01");

                                string[] split1 = data.Split(new char[] { '\n' });
                                for (int i = 1; i < split1.Length; i++)
                                {
                                    Outages outage = new ERCOTOutagesDownload.Outages();
                                    string[] split2 = split1[i].Split(new char[] { ',' });
                                        outage.OutageIdentifier = Convert.ToString(split2[0]).TrimStart();
                                        outage.PlannedStartDate = Convert.ToDateTime(split2[1]);
                                        outage.PlannedEndDate = Convert.ToDateTime(split2[2]);

                                        if (!(Convert.ToString(split2[3]) == "" || Convert.ToString(split2[3]) == " "))
                                        {
                                            outage.ActualStartDate = Convert.ToDateTime(split2[3]);
                                        }
                                        if (!(Convert.ToString(split2[4]) == "" || Convert.ToString(split2[4]) == " "))
                                        {
                                            outage.ActualEndDate = Convert.ToDateTime(split2[4]);
                                        }
                                        outage.OutageStatus = Convert.ToString(split2[5]).TrimStart();
                                        outage.RequestorOrgName = Convert.ToString(split2[6]).TrimStart();
                                        outage.EquipmentType = Convert.ToString(split2[7]).TrimStart();
                                        outage.EquipmentName = Convert.ToString(split2[8]).TrimStart();
                                        outage.EquipmentFromStationName = Convert.ToString(split2[9]).TrimStart();
                                        if (Convert.ToString(split2[10]) == "")
                                        {
                                            outage.EquipmentToStationName = "0";
                                        }
                                        else
                                        {
                                            outage.EquipmentToStationName = Convert.ToString(split2[10]).TrimStart();
                                        }
                                        outage.VoltageLevel = Convert.ToDecimal(split2[11]);
                                        outage.SubmitTime = Convert.ToDateTime(split2[12]);
                                        outage.OutageType = Convert.ToString(split2[13]).TrimStart();
                                    
                                    outagesList.Add(outage);
                                }
                                try
                                {
                                    foreach (Outages outage in outagesList)
                                    {
                                        Vayudbconnection.Open();
                                        mSelectErcotOutagesCommand.Parameters["@OutageIdentifier"].Value = outage.OutageIdentifier;
                                        mSelectErcotOutagesCommand.Parameters["@EquipmentName"].Value = outage.EquipmentName;
                                        mSelectErcotOutagesCommand.Parameters["@PlannedStartDate"].Value = outage.PlannedStartDate;
                                        mSelectErcotOutagesCommand.Parameters["@EquipmentType"].Value = outage.EquipmentType;
                                        SqlDataReader drSelectErcotOutagesCommand = mSelectErcotOutagesCommand.ExecuteReader();
                                        if (drSelectErcotOutagesCommand.HasRows)
                                        {
                                            while (drSelectErcotOutagesCommand.Read())
                                            {
                                                bool IsPresent = true;
                                                if (drSelectErcotOutagesCommand.GetValue(0).ToString() != outage.OutageIdentifier.ToString())
                                                {
                                                    IsPresent = false;
                                                }
                                                if (drSelectErcotOutagesCommand.GetValue(2).ToString() != outage.PlannedEndDate.ToString())
                                                {
                                                    IsPresent = false;
                                                }
                                                if (drSelectErcotOutagesCommand.GetValue(3).ToString() != outage.ActualStartDate.ToString())
                                                {
                                                    IsPresent = false;
                                                }
                                                if (drSelectErcotOutagesCommand.GetValue(4).ToString() != outage.ActualEndDate.ToString())
                                                {
                                                    if (drSelectErcotOutagesCommand.GetValue(4).ToString() != "")
                                                    {
                                                        IsPresent = false;
                                                    }
                                                }
                                                if (drSelectErcotOutagesCommand.GetValue(11).ToString().TrimEnd('0').TrimEnd('.') != outage.VoltageLevel.ToString())
                                                {
                                                    IsPresent = false;
                                                }

                                                if (!IsPresent)
                                                {
                                                    Vayudbconnection.Open();
                                                    mInsertErcotOutagesCommand.Parameters["@OutageIdentifier"].Value = outage.OutageIdentifier;
                                                    mInsertErcotOutagesCommand.Parameters["@PlannedStartDate"].Value = outage.PlannedStartDate;
                                                    mInsertErcotOutagesCommand.Parameters["@PlannedEndDate"].Value = outage.PlannedEndDate;
                                                    if (!outage.ActualStartDate.HasValue)
                                                    {
                                                        mInsertErcotOutagesCommand.Parameters["@ActualStartDate"].Value = DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        mInsertErcotOutagesCommand.Parameters["@ActualStartDate"].Value = outage.ActualStartDate;
                                                    }

                                                    if (!outage.ActualEndDate.HasValue)
                                                    {
                                                        mInsertErcotOutagesCommand.Parameters["@ActualEndDate"].Value = DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        mInsertErcotOutagesCommand.Parameters["@ActualEndDate"].Value = outage.ActualEndDate;
                                                    }
                                                    mInsertErcotOutagesCommand.Parameters["@OutageStatus"].Value = outage.OutageStatus;
                                                    mInsertErcotOutagesCommand.Parameters["@RequestorOrgName"].Value = outage.RequestorOrgName;
                                                    mInsertErcotOutagesCommand.Parameters["@EquipmentType"].Value = outage.EquipmentType;
                                                    mInsertErcotOutagesCommand.Parameters["@EquipmentName"].Value = outage.EquipmentName;
                                                    mInsertErcotOutagesCommand.Parameters["@EquipmentFromStationName"].Value = outage.EquipmentFromStationName;
                                                    mInsertErcotOutagesCommand.Parameters["@EquipmentToStationName"].Value = outage.EquipmentToStationName;
                                                    mInsertErcotOutagesCommand.Parameters["@VoltageLevel"].Value = outage.VoltageLevel;
                                                    mInsertErcotOutagesCommand.Parameters["@SubmitTime"].Value = outage.SubmitTime;
                                                    mInsertErcotOutagesCommand.Parameters["@OutageType"].Value = outage.OutageType;
                                                    mInsertErcotOutagesCommand.Parameters["@RemovedDate"].Value = DBNull.Value;
                                                    mInsertErcotOutagesCommand.Parameters["@RevisedDate"].Value = DateTime.Now;
                                                    mInsertErcotOutagesCommand.ExecuteNonQuery();
                                                    Vayudbconnection.Close();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Vayudbconnection.Open();
                                            mInsertErcotOutagesCommand.Parameters["@OutageIdentifier"].Value = outage.OutageIdentifier;
                                            mInsertErcotOutagesCommand.Parameters["@PlannedStartDate"].Value = outage.PlannedStartDate;
                                            mInsertErcotOutagesCommand.Parameters["@PlannedEndDate"].Value = outage.PlannedEndDate;
                                            if (!outage.ActualStartDate.HasValue)
                                            {
                                                mInsertErcotOutagesCommand.Parameters["@ActualStartDate"].Value = DBNull.Value;
                                            }
                                            else
                                            {
                                                mInsertErcotOutagesCommand.Parameters["@ActualStartDate"].Value = outage.ActualStartDate;
                                            }

                                            if (!outage.ActualEndDate.HasValue)
                                            {
                                                mInsertErcotOutagesCommand.Parameters["@ActualEndDate"].Value = DBNull.Value;
                                            }
                                            else
                                            {
                                                mInsertErcotOutagesCommand.Parameters["@ActualEndDate"].Value = outage.ActualEndDate;
                                            }
                                            mInsertErcotOutagesCommand.Parameters["@OutageStatus"].Value = outage.OutageStatus;
                                            mInsertErcotOutagesCommand.Parameters["@RequestorOrgName"].Value = outage.RequestorOrgName;
                                            mInsertErcotOutagesCommand.Parameters["@EquipmentType"].Value = outage.EquipmentType;
                                            mInsertErcotOutagesCommand.Parameters["@EquipmentName"].Value = outage.EquipmentName;
                                            mInsertErcotOutagesCommand.Parameters["@EquipmentFromStationName"].Value = outage.EquipmentFromStationName;
                                            mInsertErcotOutagesCommand.Parameters["@EquipmentToStationName"].Value = outage.EquipmentToStationName;
                                            mInsertErcotOutagesCommand.Parameters["@VoltageLevel"].Value = outage.VoltageLevel;
                                            mInsertErcotOutagesCommand.Parameters["@SubmitTime"].Value = outage.SubmitTime;
                                            mInsertErcotOutagesCommand.Parameters["@OutageType"].Value = outage.OutageType;
                                            mInsertErcotOutagesCommand.Parameters["@RemovedDate"].Value = DBNull.Value;
                                            mInsertErcotOutagesCommand.Parameters["@RevisedDate"].Value = DBNull.Value;
                                            mInsertErcotOutagesCommand.ExecuteNonQuery();
                                            Vayudbconnection.Close();
                                        }
                                        Vayudbconnection.Close();
                                    }


                                    List<string> OutageKeyList = new List<string>();
                                    foreach (Outages newOutage in outagesList)
                                    {
                                        OutageKeyList.Add(newOutage.OutageIdentifier + newOutage.PlannedStartDate + newOutage.PlannedEndDate + newOutage.EquipmentType + newOutage.EquipmentName);
                                    }

                                    foreach (Outages outages1 in mOutageList)
                                    {
                                        try
                                        {
                                            if (!OutageKeyList.Contains(outages1.OutageIdentifier + outages1.PlannedStartDate + outages1.PlannedEndDate + outages1.EquipmentType + outages1.EquipmentName))
                                            {
                                                Vayudbconnection.Open();
                                                mInsertErcotOutagesCommand.Parameters["@OutageIdentifier"].Value = outages1.OutageIdentifier;
                                                mInsertErcotOutagesCommand.Parameters["@PlannedStartDate"].Value = outages1.PlannedStartDate;
                                                mInsertErcotOutagesCommand.Parameters["@PlannedEndDate"].Value = outages1.PlannedEndDate;
                                                if (!outages1.ActualStartDate.HasValue)
                                                {
                                                    mInsertErcotOutagesCommand.Parameters["@ActualStartDate"].Value = DBNull.Value;
                                                }
                                                else
                                                {
                                                    mInsertErcotOutagesCommand.Parameters["@ActualStartDate"].Value = outages1.ActualStartDate;
                                                }

                                                if (!outages1.ActualEndDate.HasValue)
                                                {
                                                    mInsertErcotOutagesCommand.Parameters["@ActualEndDate"].Value = DBNull.Value;
                                                }
                                                else
                                                {
                                                    mInsertErcotOutagesCommand.Parameters["@ActualEndDate"].Value = outages1.ActualEndDate;
                                                }
                                                mInsertErcotOutagesCommand.Parameters["@OutageStatus"].Value = outages1.OutageStatus;
                                                mInsertErcotOutagesCommand.Parameters["@RequestorOrgName"].Value = outages1.RequestorOrgName;
                                                mInsertErcotOutagesCommand.Parameters["@EquipmentType"].Value = outages1.EquipmentType;
                                                mInsertErcotOutagesCommand.Parameters["@EquipmentName"].Value = outages1.EquipmentName;
                                                mInsertErcotOutagesCommand.Parameters["@EquipmentFromStationName"].Value = outages1.EquipmentFromStationName;
                                                mInsertErcotOutagesCommand.Parameters["@EquipmentToStationName"].Value = outages1.EquipmentToStationName;
                                                mInsertErcotOutagesCommand.Parameters["@VoltageLevel"].Value = outages1.VoltageLevel;
                                                mInsertErcotOutagesCommand.Parameters["@SubmitTime"].Value = outages1.SubmitTime;
                                                mInsertErcotOutagesCommand.Parameters["@OutageType"].Value = outages1.OutageType;
                                                mInsertErcotOutagesCommand.Parameters["@RemovedDate"].Value = DateTime.Now;
                                                mInsertErcotOutagesCommand.Parameters["@RevisedDate"].Value = DBNull.Value;
                                                mInsertErcotOutagesCommand.ExecuteNonQuery();
                                                Vayudbconnection.Close();
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                   
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        
                    }
                    
                    File.Move(filePaths[k], DestinationZipFilePath + FolderFilename+".zip");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void FillList()
        {
            mOutageList = new List<Outages>();
            if (Vayudbconnection.State == ConnectionState.Open)
            {
                Vayudbconnection.Close();
            }
            Vayudbconnection.Open();
            SqlDataReader reader = mSelectRemoveErcotOutagesCommand.ExecuteReader();
            while (reader.Read())
            {
                Outages outage = new Outages();
                outage.OutageIdentifier = reader.GetValue(0).ToString();
                outage.PlannedStartDate = Convert.ToDateTime(reader.GetValue(1));
                outage.PlannedEndDate = Convert.ToDateTime(reader.GetValue(2));
                if (reader.IsDBNull(3))
                {
                    outage.ActualStartDate = null;
                }
                else
                {
                    outage.ActualStartDate = Convert.ToDateTime(reader.GetValue(3));
                }
                if(reader.IsDBNull(4))
                {
                    outage.ActualEndDate = null;
                }
                else
                {
                   outage.ActualEndDate= Convert.ToDateTime(reader.GetValue(4));
                }
                outage.OutageStatus = reader.GetValue(5).ToString();
                outage.RequestorOrgName = reader.GetValue(6).ToString();
                outage.EquipmentType = reader.GetValue(7).ToString();
                outage.EquipmentName = reader.GetValue(8).ToString();
                outage.EquipmentFromStationName = reader.GetValue(9).ToString();
                outage.EquipmentToStationName = reader.GetValue(10).ToString();
                outage.VoltageLevel =Convert.ToDecimal(reader.GetValue(11));
                outage.SubmitTime = Convert.ToDateTime(reader.GetValue(12).ToString());
                outage.OutageType = reader.GetValue(13).ToString();
                mOutageList.Add(outage);
            }
            reader.Close();
            Vayudbconnection.Close();
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
                                  //  strNewFile = FolderFilename;
                                    strNewFile = @"" + baseDirectory + @"\" + FolderFilename + @"\" + theEntry.Name;
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
