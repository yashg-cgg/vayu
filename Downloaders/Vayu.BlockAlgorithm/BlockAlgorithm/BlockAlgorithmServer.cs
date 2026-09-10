//#define HISTORY
#define ERCOT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vayu.BlockAlgorithmLibraryNamespace;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Vayu.LMP;
using Vayu.NodePriceLibrary;
using System.Configuration;
using Vayu.CommonAccessLibrary;

namespace Vayu.BlockAlgorithmNamespace
{
    [ServiceBehavior(MaxItemsInObjectGraph = int.MaxValue)]
    class BlockAlgorithmServer : IBlockAlgorithm
    {

#if HISTORY
        private DateTime LastScanDate = DateTime.Now.Date;
#endif
        private Dictionary<int, string> nodeHash;
        private SqlConnection SigmaDbConnection;
        private SqlCommand mSelectPathsCommand;

        public const string dateFormat = "yyyy-MM-dd";
        public const string completedDate = "completedDate";
        public string tableName = Program.ProductionTable;

        public BlockAlgorithmServer()
        {
            //SigmaDbConnection = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
            SigmaDbConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            nodeHash = GetNodeHash();
        }

        private void InitDB()
        {
            mSelectPathsCommand = new SqlCommand();
#if ERCOT
            mSelectPathsCommand.CommandText = "SELECT DISTINCT SourceNodeKey, SinkNodeKey from eroct..EESPathList  order by sourcenodekey, sinknodekey";
#else
            mSelectPathsCommand.CommandText = "SELECT DISTINCT SourceNodeKey, SinkNodeKey from EESPathList where marketkey = 1 order by sourcenodekey, sinknodekey";
#endif
            mSelectPathsCommand.Connection = SigmaDbConnection;
        }

        public Dictionary<int, string> GetNodeHash()
        {
            Dictionary<int, string> mNodeHash = new Dictionary<int, string>();
            SqlCommand mSelectNodeCommand = SigmaDbConnection.CreateCommand();
#if ERCOT
            mSelectNodeCommand.CommandText = "select nodekey, nodename from node where marketkey = 9";
#else

            mSelectNodeCommand.CommandText = "select nodekey, nodename from node where marketkey = 1";
#endif
            if (SigmaDbConnection.State != ConnectionState.Open)
                SigmaDbConnection.Open();

            try
            {
                SqlDataReader reader = mSelectNodeCommand.ExecuteReader();

                while (reader.Read())
                    mNodeHash.Add(Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[0]).GetValueOrDefault(), reader.GetString(1));

                reader.Close();
            }
            catch { }
            finally
            {
                if (SigmaDbConnection.State != ConnectionState.Closed)
                    SigmaDbConnection.Close();
            }

            return mNodeHash;
        }

//        public void Connect()
//        {
//            while (true)
//            {
//                InitDB();
//                tableName = DecideTable();
//                BlockAlgoHelperClass.SetTableName(tableName);
//                DateTime today = GetScanDate();

//#if HISTORY
//                if (today < new DateTime(2014, 1, 1))
//                    break;

//                if (tableName == Program.TemporaryTable)
//                {
//                    LastScanDate = LastScanDate.AddDays(-1);
//                    continue;
//                }
//#endif

//                BlockAlgoHelperClass.AppendFileLog("Start time: " + DateTime.Now.ToString());
//                try
//                {
//                    List<SourceSink> nodeList = new List<SourceSink>();
//                    List<int> uniqueList = new List<int>();
//                    Dictionary<int, Node> rtHash = new Dictionary<int, Node>();
//                    Dictionary<int, Node> daHash = new Dictionary<int, Node>();
//                    SigmaDbConnection.Open();
//                    SqlDataReader reader = mSelectPathsCommand.ExecuteReader();
//                    while (reader.Read())
//                    {
//                        SourceSink mSourceSink = new SourceSink();
//                        mSourceSink.SourceNodeKey = Convert.ToInt32(reader.GetValue(0));
//                        mSourceSink.SinkNodeKey = Convert.ToInt32(reader.GetValue(1));
//                        nodeList.Add(mSourceSink);
//                        if (!uniqueList.Contains(mSourceSink.SourceNodeKey))
//                        {
//                            uniqueList.Add(mSourceSink.SourceNodeKey);
//                        }
//                        if (!uniqueList.Contains(mSourceSink.SinkNodeKey))
//                        {
//                            uniqueList.Add(mSourceSink.SinkNodeKey);
//                        }
//                    }
//                    reader.Close();
//                    SigmaDbConnection.Close();
//                    List<int> dartList = new List<int>();
//                    foreach (SourceSink sourceSink in nodeList)
//                    {
//                        if (!dartList.Contains(sourceSink.SourceNodeKey))
//                        {
//                            dartList.Add(sourceSink.SourceNodeKey);
//                        }
//                        if (!dartList.Contains(sourceSink.SinkNodeKey))
//                        {
//                            dartList.Add(sourceSink.SinkNodeKey);
//                        }
//                    }

//                    DateTime startDate = today.AddYears(-1);
//                    DateTime endDate = today;
//                    DateTime tempDate1 = startDate;
//                    while (tempDate1 < startDate.AddDays(368))
//                    {
//#if ERCOT
//                        DARTNode.GetDartsForUptos(null, null, tempDate1, tempDate1.AddDays(15), true, 1);
//                        tempDate1 = tempDate1.AddDays(15);
//                        BlockAlgoHelperClass.AppendConsole("Date " + tempDate1.ToShortDateString());
//#else
//                        DARTNode.GetDartsForUptos(null, null, tempDate1, tempDate1.AddDays(15), true, 1);
//                        tempDate1 = tempDate1.AddDays(15);
//                        BlockAlgoHelperClass.AppendConsole("Date " + tempDate1.ToShortDateString());
//#endif
//                    }

//                    bool SingleMachine = IsSingleMachine();
//                    SourceSink[] calculateNodeList = null;
//                    int numPaths = 0;
//                    int count = GetSessionCount();
//                    DateTime savedDate = DateTime.Now;
//                    int blockAlgoPathCount = 0;

//                    if (SingleMachine)
//                        BlockAlgoHelperClass.AppendFileLog("Single Machine Mode");
//                    else
//                        BlockAlgoHelperClass.AppendFileLog("Multi Machine Mode");

//                    while (true)
//                    {
//                        if ((numPaths + count) >= nodeList.Count)
//                            break;

//                        calculateNodeList = new SourceSink[count];
//                        nodeList.CopyTo(numPaths, calculateNodeList, 0, count);

//                        if (nodeList.Count - numPaths < count)
//                            count = nodeList.Count - numPaths - 1;
//                        else
//                            numPaths += count;

//                        if (count < 0)
//                            break;

//                        Calculate(numPaths + 1, calculateNodeList, startDate, endDate, DARTNode.dictRTHash, DARTNode.dictDAHash, savedDate);
//                    }
//                }
//                catch (Exception Exception)
//                {
//                    BlockAlgoHelperClass.AppendFileLog(Exception.Message);
//                }
//                finally
//                {
//                    BlockAlgoHelperClass.AppendFileLog("Complete time: " + DateTime.Now.ToString());
//                    RunPostCompleteTask(today);
//#if HISTORY
//                    LastScanDate = LastScanDate.AddDays(-1);
//#endif
//                }
//            }
//        }

        public bool IsSingleMachine()
        {
            ConfigurationManager.RefreshSection("appSettings");
            string strValue = ConfigurationManager.AppSettings.Get("SingleMachine");
            if (!string.IsNullOrEmpty(strValue) && "true".Equals(strValue, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public int GetSessionCount()
        {
            ConfigurationManager.RefreshSection("appSettings");
            string strValue = (ConfigurationManager.AppSettings.Get("SessionCount") ?? "");
            int count = 64;

            if (!int.TryParse(strValue, out count))
                count = 64;

            return count;
        }

        public bool CanExecute()
        {
            DateTime scanDate = GetScanDate();
            SqlCommand cmd = SigmaDbConnection.CreateCommand();
            cmd.CommandText = "select max(marketdate) from " + tableName;

            SqlCommand deleteCmd = SigmaDbConnection.CreateCommand();
            deleteCmd.CommandText = "delete " + tableName + " where marketdate = '" + scanDate.ToString(dateFormat) + "'";

            if (SigmaDbConnection.State != ConnectionState.Open)
                SigmaDbConnection.Open();

            try
            {
                DateTime? date = cmd.ExecuteScalar() as DateTime?;
                if (!date.GetValueOrDefault().Date.Equals(scanDate.Date))
                    return true;

                DateTime configDate;
                string dateString = ConfigurationManager.AppSettings.Get(completedDate);
                DateTime.TryParseExact(dateString, dateFormat, null, System.Globalization.DateTimeStyles.None, out configDate);

                if (configDate.Date.Equals(scanDate))
                    return false;

                deleteCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            { }
            finally
            {
                if (SigmaDbConnection.State != ConnectionState.Closed)
                    SigmaDbConnection.Close();
            }

            return true;
        }

        public string DecideTable()
        {
            string tabName = string.Empty;
            DateTime scanDate = GetScanDate();
            SqlCommand cmd = SigmaDbConnection.CreateCommand();

            /*
             * If main table is not having date then execute it on main table.
             * else delete the test table and then execute on it.
             * if executed on test table .. delete for that date in main table and copy data from test to main
             * */

            tabName = Program.ProductionTable;
            cmd.CommandText = "select count(*) from " + Program.ProductionTable + " where marketdate = '" + scanDate.ToString("yyyy-MM-dd") + "'";

            if (SigmaDbConnection.State != ConnectionState.Open)
                SigmaDbConnection.Open();

            try
            {
                //DateTime? date = cmd.ExecuteScalar() as DateTime?;
                int? count = cmd.ExecuteScalar() as int?;
                if (count.HasValue && count.Value > 0)
                    tabName = Program.TemporaryTable;
                else
                    tabName = Program.ProductionTable;

            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
            finally
            {
                if (SigmaDbConnection.State != ConnectionState.Closed)
                    SigmaDbConnection.Close();
            }

            return tabName;
        }

        public void RunPostCompleteTask(DateTime dat)
        {
            string tableNameString = string.Empty;

            string table = Program.TemporaryTable;
            string storedProc = "exec " + Program.MergeProcedure;
            BlockAlgoHelperClass.StopDBCollection();
            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //config.AppSettings.Settings["completedDate"].Value = date.ToString(dateFormat);
            //config.Save();

            if (table.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
            {
                storedProc = "exec  " + Program.MergeProcedure;
                tableNameString = Program.TemporaryTable;
            }

            if (tableNameString.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    SqlCommand mergeCmd = SigmaDbConnection.CreateCommand();
                    mergeCmd.CommandText = storedProc;
                    // mergeCmd.Connection = new SqlConnection(Vayu.CommonAccessLibrary.DBConnectionCredentials.GetERCOTDBConnection());
                    mergeCmd.Connection = new VayuDBConnection().GetInstance().GetSqlConnection();
                    mergeCmd.Connection.Open();
                    mergeCmd.ExecuteNonQuery();
                    mergeCmd.Connection.Close();
                }
                catch { }
            }
        }


        public void Calculate(int numPaths, SourceSink[] calculateNodeList, DateTime startDate, DateTime endDate,
                            ConcurrentDictionary<string, double> rtHash, ConcurrentDictionary<string, double> daHash, DateTime savedDate)
        {

            BlockAlgoThread blockAlgo = new BlockAlgoThread(numPaths, calculateNodeList, startDate, endDate, rtHash, daHash, savedDate, nodeHash);
            blockAlgo.Run();
        }

        public DateTime GetScanDate()
        {
            ConfigurationManager.RefreshSection("appSettings");
            string strValue = ConfigurationManager.AppSettings.Get("ScanDate");
            DateTime scanDate;

#if HISTORY
            if (!DateTime.TryParse(strValue, out scanDate))
                scanDate = LastScanDate.AddDays(-1);            
#else
            if (!DateTime.TryParse(strValue, out scanDate))
                scanDate = DateTime.Now.Date;
#endif

            BlockAlgoHelperClass.AppendFileLog("Produced Scan Date: " + scanDate.ToShortDateString());
            return scanDate;
        }
    }
}