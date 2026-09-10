//#define TEST
#define ERCOT
using Vayu.BlockAlgorithmLibraryNamespace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.LMP;
using Vayu.CommonAccessLibrary;
using System.Data;

namespace Vayu.BlockAlgorithmNamespace
{
    public class Program
    {
#if ERCOT
       public const string ProductionTable = "Vayu..BlockAlgorithmResults"; //BlockAlgorithmResults_New
       // public const string ProductionTable = "Vayu..BlockAlgorithmResults";
        public const string TemporaryTable = "Vayu..BlockAlgorithmResults_Temp";//"EESAnalysisRobotFilteredResults3HR1_history_temp";
        public const string MergeProcedure = "Vayu..[MergeBlockAlgorithmData]";
    
#else
        //public const string ProductionTable = "BlockAlgorithmResults";// "EESAnalysisRobotFilteredResults3HR1_history";
        public const string ProductionTable = "BlockAlgorithmResults_new";
        public const string TemporaryTable = "BlockAlgorithmResults_Temp_new";//"EESAnalysisRobotFilteredResults3HR1_history_temp";
        public const string MergeProcedure = "Ercot.[dbo].[MergeBlockAlgorithmData_new]";
#endif

        public Program()
        {
        }

#if TEST
        static void MainProduction(string[] args)
#else
        static void MainSeasonal(string[] args)
#endif
        {
            CalculateMinMaxRTWinter mCalculateMinMaxRTWinter = new CalculateMinMaxRTWinter();
            // CalculateMinMaxRTAll mCalculateMinMaxRTAll = new CalculateMinMaxRTAll();

        }

#if TEST
        static void Main(string[] args)
#else
        public static void Main(string[] args)
#endif
        {

            BlockAlgoHelperClass.StartBlockAlgoHelper();

            Program pg = new Program();
            
            pg.RunTest();

            BlockAlgoHelperClass.StopBlockAlgoHelper();
        }

        public void RunTest()
        {
#if ERCOT

            DeleteOldRecords(9);
           // BlockAlgoHelperClass.SetTableName("Vayu..BlockAlgorithmResults_SP");
           // BlockAlgoHelperClass.SetTableName("Vayu..BlockAlgorithmResults_New");
            BlockAlgoHelperClass.SetTableName("Vayu..BlockAlgorithmResults");

#endif

            List<SourceSink> nodeList = GetTestSourceSinkList();
            int[] nodeIDs = nodeList.Select(x => x.SinkNodeKey).Concat(nodeList.Select(y => y.SourceNodeKey)).ToArray();
            List<int> dartList = new List<int>();

            foreach (SourceSink sourceSink in nodeList)
            {
                if (!dartList.Contains(sourceSink.SourceNodeKey))
                    dartList.Add(sourceSink.SourceNodeKey);
                if (!dartList.Contains(sourceSink.SinkNodeKey))
                    dartList.Add(sourceSink.SinkNodeKey);
                //}


            }
                      

            DateTime today = DateTime.Today;
           //DateTime startDate = today.AddYears(-1);
            DateTime startDate = today.AddDays(-62);
            DateTime endDate = today;
            DateTime tempDate1 = startDate;

            while (tempDate1 < startDate.AddDays(66)) //SPCh 368.0
            {
                //#if TEST
#if ERCOT
                DARTNode.GetDartsForUptos(null, null, tempDate1, tempDate1.AddDays(15), true, 9);

#endif
                //#endif
                tempDate1 = tempDate1.AddDays(15);
                BlockAlgoHelperClass.AppendConsole("Date " + tempDate1.ToShortDateString());
            }

            DateTime now = DateTime.Now;
            Dictionary<int, string> nodeHash = new BlockAlgorithmServer().GetNodeHash();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (nodeList.Count > 0)
            {
                List<SourceSink> list = nodeList.Take<SourceSink>(50).ToList<SourceSink>();
                BlockAlgoThread blockAlgoThread = new BlockAlgoThread(0, list.ToArray(), startDate, endDate, DARTNode.dictRTHash, DARTNode.dictDAHash, now, nodeHash);
                try
                {
                    blockAlgoThread.Run();
                }
                catch (Exception ex)
                {
                }
                foreach (SourceSink sourceSink in list)
                {
                    SourceSink path = sourceSink;
                    if (nodeList.Exists((Predicate<SourceSink>)(a => a.SourceNodeKey == path.SourceNodeKey && a.SinkNodeKey == path.SinkNodeKey)))
                        nodeList.Remove(path);
                }
                GC.Collect();
            }
            stopwatch.Stop();
            Debug.WriteLine("Time taken :" + stopwatch.Elapsed.ToString());
            Console.WriteLine("Time taken : {0}", stopwatch.Elapsed);
            BlockAlgoHelperClass.StopBlockAlgoHelper();
        }

        public void DeleteOldRecords(int marketKey)
        {
            try
            {
                if (marketKey == 9)
                {
                    using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {

//                          select* from Vayu..BlockAlgorithmResults_New where marketdate< (select DATEADD(DD,-5,max(marketdate)) from Vayu..BlockAlgorithmResults_New)
                            //con.Open();
                            Console.WriteLine("Deleting old Data");
                            cmd.CommandText = "delete from Vayu..BlockAlgorithmResults where marketdate < (select DATEADD(DD,-3,max(marketdate)) from Vayu..BlockAlgorithmResults) ";
                            //cmd.CommandText = "delete from Vayu..BlockAlgorithmResults where marketdate < '" + DateTime.Today.Date.AddDays(-2).ToString() + "' ";
                            cmd.CommandTimeout = 30000;
                            cmd.Connection = con;
                            cmd.ExecuteNonQuery();
                            con.Close();
                            Console.WriteLine("Deleting Done");
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private List<SourceSink> GetTestSourceSinkList()
        {
            List<SourceSink> nodeList = new List<SourceSink>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                //if (con.State == ConnectionState.Open)
                //{
                //    con.Close();
                //}
                using (SqlCommand cmd = con.CreateCommand())
                {
                  

                    //con.Open();
                    cmd.Connection = con;
#if ERCOT
                    //cmd.CommandText = "select distinct SourceNodeKey , SinkNodeKey from Vayu..EESPathList where SourceNodeKey < 57484 order by SourceNodeKey  ";
                     cmd.CommandText = "select distinct SourceNodeKey , SinkNodeKey from Vayu..EESPathList order by SourceNodeKey  ";
                    //cmd.CommandText = "select distinct SourceNodeKey , SinkNodeKey from Vayu..EESPathList where  SourceNodeKey=57219"; // and SinkNodeKey=57716 ";  //57286
                    

#else
                    cmd.CommandText = "select distinct SourceNodeKey , SinkNodeKey from EESPathList";
#endif
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        SourceSink path = new SourceSink();
                        path.SourceNodeKey = Convert.ToInt32(rdr.GetValue(0));
                        path.SinkNodeKey = Convert.ToInt32(rdr.GetValue(1));
                        nodeList.Add(path);
                    }
                    rdr.Close();
                    con.Close();
                }
            }

            //SQLColumn Issue           
            return nodeList;
        }
    }
}

