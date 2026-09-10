using Microsoft.Win32;
using Vayu.CommonAccessLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Vayu.ErcotNotification
{
    class NewErcotNotification
    {
        private SqlCommand cmd;
        System.Timers.Timer sTimer = new System.Timers.Timer();
        private DateTime mDate = DateTime.Today.AddDays(0);
        private DataTable dtErcotMessages = new DataTable();
        private DataRow drStErcotMessageRow = null;
        SqlConnection VayuDbConn;
        public NewErcotNotification()
        {
            LoadDB();
            sTimer = new System.Timers.Timer();
            OnTimerEvent(null, null);
            sTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            sTimer.Interval = 300000;
            sTimer.Start();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        private void OnTimerEvent(object source, ElapsedEventArgs e)
        {
            sTimer.Enabled = false;
            try
            {
                DownloadOperationMessages();
            }
            catch (Exception ex)
            {
            }
            sTimer.Enabled = true;
        }
        public void DownloadOperationMessages()
        {
            try
            {
                string URL = "http://www.ercot.com/services/comm/mkt_notices/opsmessages";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.ConnectionLimit = 1;
                request.Method = "GET";
                request.Timeout = 100000;
                HttpWebResponse FileDownloadresponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = default(StreamReader);
                sr = new StreamReader(FileDownloadresponse.GetResponseStream(), Encoding.UTF8);
                string strLoadMessages = sr.ReadToEnd();
                string strDaString = strLoadMessages.ToString();
                strLoadMessages = strLoadMessages.Replace("\n", "").Trim();
                ParseHtml(strLoadMessages);
                sr.Close();

            }
            catch (Exception ex)
            {

            }
        }
        public void ParseHtml(string htmlData)
        {
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(htmlData);
            var table = htmlDocument.DocumentNode.Descendants("tr");
            dtErcotMessages.Clear();
            foreach (var row in table.Skip(1))
            {
                drStErcotMessageRow = dtErcotMessages.NewRow();
                drStErcotMessageRow["Date"] = row.ChildNodes[1].InnerText;
                drStErcotMessageRow["Message"] = row.ChildNodes[3].InnerText;
                drStErcotMessageRow["Type"] = row.ChildNodes[5].InnerText;
                drStErcotMessageRow["Status"] = row.ChildNodes[7].InnerText;
                drStErcotMessageRow["CreateDate"] = DateTime.Now;
                dtErcotMessages.Rows.Add(drStErcotMessageRow);
            }
            var x = dtErcotMessages.Rows.Count;
            SaveInDB(dtErcotMessages);
        }

        private void SaveInDB(DataTable msgData)
        {
            if (VayuDbConn.State == ConnectionState.Open)
            {
                VayuDbConn.Close();
            }
            VayuDbConn.Open();
            SqlTransaction transaction = VayuDbConn.BeginTransaction();
            using (SqlBulkCopy bkLoadsH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction))
            {
                try
                {
                    bkLoadsH.DestinationTableName = "OperationMessages_TP";
                    bkLoadsH.ColumnMappings.Add("Date", "Date");
                    bkLoadsH.ColumnMappings.Add("Message", "Message");
                    bkLoadsH.ColumnMappings.Add("Type", "Type");
                    bkLoadsH.ColumnMappings.Add("Status", "Status");
                    bkLoadsH.ColumnMappings.Add("CreateDate", "CreateDate");
                    bkLoadsH.BulkCopyTimeout = 30000;
                    bkLoadsH.WriteToServer(msgData);                    
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                }
            }

            SqlTransaction transaction2 = VayuDbConn.BeginTransaction();
            using (SqlBulkCopy bkLoadsH = new SqlBulkCopy(VayuDbConn, SqlBulkCopyOptions.TableLock, transaction2))
            {
                try
                {
                    SqlCommand UpMergeWindForecastCmd = new SqlCommand("[dbo].[UpMergeOperationMessageTemp]", VayuDbConn, transaction2);
                    UpMergeWindForecastCmd.CommandType = CommandType.StoredProcedure;
                    UpMergeWindForecastCmd.CommandTimeout = 40000;
                    UpMergeWindForecastCmd.ExecuteNonQuery();
                    transaction2.Commit();
                }
                catch (Exception ex)
                {
                }
            }
            VayuDbConn.Close();

        }

        private void LoadDB()
        {
            VayuDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();

            //RegistryKey localKey;
            //if (Environment.Is64BitOperatingSystem)
            //    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            //else
            //    localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            //string getconnstring = localKey.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run").GetValue("Ercot").ToString();
            ////string getconnstring = GetErcotDBConnection();

            //conn = new SqlConnection(getconnstring);

            dtErcotMessages.Columns.Add("Date", typeof(string));
            dtErcotMessages.Columns.Add("Message", typeof(string));
            dtErcotMessages.Columns.Add("Type", typeof(string));
            dtErcotMessages.Columns.Add("Status", typeof(string));
            dtErcotMessages.Columns.Add("CreateDate", typeof(DateTime));

        }
    }
}
