// Decompiled with JetBrains decompiler
// Type: BlockAlgorithmNamespace.BlockAlgoHelperClass
// Assembly: BlockAlgorithm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5EEEACC4-ABC5-4C61-B304-4E2FBBDDD6CC
// Assembly location: C:\Users\piyush\Desktop\BlockDLLs\BlockAlgorithm.exe

using Vayu.CommonAccessLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vayu.BlockAlgorithmNamespace
{
    public class BlockAlgoHelperClass
    {
        private Dictionary<string, string> nameTypeDictionary = new Dictionary<string, string>()
    {
      {
        "SourceName",
        "s"
      },
      {
        "SinkName",
        "s"
      },
      {
        "AnalysisType",
        "d"
      },
      {
        "Price",
        "d"
      },
      {
        "SumValue",
        "d"
      },
      {
        "MaxWin",
        "d"
      },
      {
        "MaxLoss",
        "d"
      },
      {
        "MW",
        "d"
      },
      {
        "RiskReward",
        "d"
      },
      {
        "CountDays",
        "d"
      },
      {
        "CountCleared",
        "d"
      },
      {
        "PctWin",
        "d"
      },
      {
        "CalcNumber",
        "d"
      },
      {
        "YearlyDownside",
        "d"
      },
      {
        "YearlyRiskReward",
        "d"
      },
      {
        "SourceNodeKey",
        "d"
      },
      {
        "SinkNodeKey",
        "d"
      },
      {
        "AvgDA",
        "d"
      },
      {
        "SavedTime",
        "t"
      },
      {
        "marketdate",
        "t"
      },
      {
        "HoursCleared",
        "d"
      },
      {
        "MustTakeSum",
        "d"
      },
      {
        "AMustTakeSum",
        "d"
      },
      {
        "DailyMustTakeMin",
        "d"
      },
      {
        "ADailyMustTakeMin",
        "d"
      },
      {
        "ASum",
        "d"
      },
      {
        "AAvg",
        "d"
      },
      {
        "AMin",
        "d"
      },
      {
        "AMax",
        "d"
      },
      {
        "AStdDev",
        "d"
      },
      {
        "AWinPct",
        "d"
      },
      {
        "AClearPct",
        "d"
      },
      {
        "DailyMin",
        "d"
      },
      {
        "DailyMax",
        "d"
      },
      {
        "DailyAvg",
        "d"
      },
      {
        "ADailyMin",
        "d"
      },
      {
        "ADailyMax",
        "d"
      },
      {
        "ADailyAvg",
        "d"
      },
      {
        "SumToMax",
        "d"
      },
      {
        "Sharpe",
        "d"
      },
      {
        "Skew",
        "d"
      },
      {
        "Kurtosis",
        "d"
      },
      {
        "DollarPerMW",
        "d"
      },
      {
        "WeeklyWin",
        "i"
      },
      {
        "StdDev",
        "d"
      },
      {
        "WeeklySumValue",
        "d"
      },
      {
        "WeeklyMaxWin",
        "d"
      },
      {
        "WeeklyMaxLossRT",
        "d"
      },
      {
        "WeeklyPctWin",
        "d"
      },
      {
        "AnnualMaxLossRT",
        "d"
      },
      {
        "WeeklyCountCleared",
        "i"
      },
      {
        "MonthlyMaxLossRT",
        "d"
      }
    };
        private static BlockAlgoHelperClass blockAlgoHelperClass;
        private BlockingCollection<string> logCollection;
        private BlockingCollection<string> consoleCollection;
        private BlockingCollection<KeyValuePair<Result, Result>> resultCollection;
        private Task taskResultLog;
        private Task taskFileLog;
        private Task taskConsoleLog;
        private string tableName;
        private SqlConnection SigmaDbConnection;
        private SqlCommand mInsertResultsCommand;
        private int validCount;

        public static BlockAlgoHelperClass BlockAlgoHelper
        {
            get
            {
                return BlockAlgoHelperClass.blockAlgoHelperClass;
            }
        }

        public BlockingCollection<string> FileLogCollection
        {
            get
            {
                return this.logCollection;
            }
        }

        public BlockingCollection<string> ConsoleCollection
        {
            get
            {
                return this.consoleCollection;
            }
        }

        public BlockingCollection<KeyValuePair<Result, Result>> ResultCollection
        {
            get
            {
                return this.resultCollection;
            }
        }

        public BlockAlgoHelperClass()
        {
            this.logCollection = new BlockingCollection<string>();
            this.consoleCollection = new BlockingCollection<string>();
            this.resultCollection = new BlockingCollection<KeyValuePair<Result, Result>>();
        }

        public void InitDB()
        {
            this.InitConnection();
            this.mInsertResultsCommand = this.SigmaDbConnection.CreateCommand();
            this.mInsertResultsCommand.Parameters.AddWithValue("@sourcename", (object)"sourcename");
            this.mInsertResultsCommand.Parameters.AddWithValue("@sinkname", (object)"sinkname");
            this.mInsertResultsCommand.Parameters.AddWithValue("@analysistype", (object)"analysistype");
            this.mInsertResultsCommand.Parameters.AddWithValue("@price", (object)"price");
            this.mInsertResultsCommand.Parameters.AddWithValue("@sumvalue", (object)"sumvalue");
            this.mInsertResultsCommand.Parameters.AddWithValue("@maxwin", (object)"maxwin");
            this.mInsertResultsCommand.Parameters.AddWithValue("@maxloss", (object)"maxloss");
            this.mInsertResultsCommand.Parameters.AddWithValue("@riskreward", (object)"riskreward");
            this.mInsertResultsCommand.Parameters.AddWithValue("@countdays", (object)"countdays");
            this.mInsertResultsCommand.Parameters.AddWithValue("@countcleared", (object)"countcleared");
            this.mInsertResultsCommand.Parameters.AddWithValue("@pctwin", (object)"pctwin");
            this.mInsertResultsCommand.Parameters.AddWithValue("@calcnumber", (object)"calcnumber");
            this.mInsertResultsCommand.Parameters.AddWithValue("@yearlydownside", (object)"yearlydownside");
            this.mInsertResultsCommand.Parameters.AddWithValue("@yearlyriskreward", (object)"yearlyriskreward");
            this.mInsertResultsCommand.Parameters.AddWithValue("@sourcenodekey", (object)"sourcenodekey");
            this.mInsertResultsCommand.Parameters.AddWithValue("@sinknodekey", (object)"sinknodekey");
            this.mInsertResultsCommand.Parameters.AddWithValue("@avgda", (object)"avgda");
            this.mInsertResultsCommand.Parameters.AddWithValue("@savedtime", (object)"savedtime");
            this.mInsertResultsCommand.Parameters.AddWithValue("@marketdate", (object)"marketdate");
            this.mInsertResultsCommand.Parameters.AddWithValue("@hourCleared", (object)"hour");
            this.mInsertResultsCommand.Parameters.AddWithValue("@MustTakeSum", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@AMustTakeSum", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@DailyMustTakeMin", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@ADailyMustTakeMin", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@ASum", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@AAvg", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@Amin", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@Amax", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@AStdDev", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@AWinPct", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@AClearPct", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@DailyMin", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@DailyMax", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@DailyAvg", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@ADailyMin", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@ADailyMax", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@ADailyAvg", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@SumToMax", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@Sharpe", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@Skew", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@Kurtosis", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@DollarPerMW", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@WeeklyWin", (object)0);
            this.mInsertResultsCommand.Parameters.AddWithValue("@StdDev", (object)0);
        }

        public void InitConnection()
        {
            //this.SigmaDbConnection = new SqlConnection(DBConnectionCredentials.GetERCOTDBConnection());
            this.SigmaDbConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
        }

        public void IncrementValidPathCount()
        {
            ++BlockAlgoHelperClass.blockAlgoHelperClass.validCount;
            BlockAlgoHelperClass.blockAlgoHelperClass.ConsoleCollection.Add("Valid Path count: " + BlockAlgoHelperClass.blockAlgoHelperClass.validCount.ToString());
        }

        public static void StartBlockAlgoHelper()
        {
            BlockAlgoHelperClass.blockAlgoHelperClass = new BlockAlgoHelperClass();
            BlockAlgoHelperClass.blockAlgoHelperClass.StartBlockingCollections();
        }

        public static void SetTableName(string tabName)
        {
            BlockAlgoHelperClass.blockAlgoHelperClass.resultCollection = new BlockingCollection<KeyValuePair<Result, Result>>();
            BlockAlgoHelperClass.blockAlgoHelperClass.tableName = tabName;
            BlockAlgoHelperClass.blockAlgoHelperClass.InitDB();
            BlockAlgoHelperClass.blockAlgoHelperClass.RunDBCollection();
        }

        public static void StopBlockAlgoHelper()
        {
            if (BlockAlgoHelperClass.blockAlgoHelperClass == null)
                return;
            BlockAlgoHelperClass.StopDBCollection();
            BlockAlgoHelperClass.blockAlgoHelperClass.StopBlockingLogCollections();
        }

        public static void StopDBCollection()
        {
            if (BlockAlgoHelperClass.blockAlgoHelperClass.resultCollection == null)
                return;
            BlockAlgoHelperClass.blockAlgoHelperClass.resultCollection.CompleteAdding();
            Task.WaitAll(BlockAlgoHelperClass.blockAlgoHelperClass.taskResultLog);
        }

        private void StopBlockingLogCollections()
        {
            this.logCollection.CompleteAdding();
            this.consoleCollection.CompleteAdding();
            Task.WaitAll(this.taskFileLog, this.taskConsoleLog);
        }

        private void StartBlockingCollections()
        {
            this.taskFileLog = new Task((Action)(() => this.AppendLogFromCollection()));
            this.taskFileLog.Start();
            this.taskConsoleLog = new Task((Action)(() => this.AppendConsoleFromCollection()));
            this.taskConsoleLog.Start();
        }

        public static void AppendFileLog(string logLine)
        {
            try
            {
                BlockAlgoHelperClass.blockAlgoHelperClass.logCollection.Add(logLine);
            }
            catch
            {
            }
        }

        public static void AppendConsole(string logLine)
        {
            try
            {
                BlockAlgoHelperClass.blockAlgoHelperClass.consoleCollection.Add(logLine);
            }
            catch
            {
            }
        }

        public static void AddResults(Result result30, Result result360)
        {
            try
            {
                KeyValuePair<Result, Result> keyValuePair = new KeyValuePair<Result, Result>(result30, result360);
                BlockAlgoHelperClass.blockAlgoHelperClass.resultCollection.Add(keyValuePair);
            }
            catch
            {
            }
        }

        public void AppendLogFromCollection()
        {
            string path = Environment.CurrentDirectory + "\\log.txt";
            try
            {
                foreach (string consuming in this.logCollection.GetConsumingEnumerable())
                {
                    StreamWriter streamWriter = new StreamWriter(path, true);
                    streamWriter.WriteLine(consuming);
                    streamWriter.Close();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
        }

        public void AppendConsoleFromCollection()
        {
            try
            {
                foreach (string consuming in this.consoleCollection.GetConsumingEnumerable())
                    Console.WriteLine(consuming);
            }
            catch
            {
            }
        }

        public void RunDBCollection()
        {
            this.validCount = 0;
            List<KeyValuePair<Result, Result>> resultList = new List<KeyValuePair<Result, Result>>();
            Action action = (Action)(() =>
            {
                try
                {
                    foreach (KeyValuePair<Result, Result> consuming in this.resultCollection.GetConsumingEnumerable())
                    {
                        try
                        {
                            resultList.Add(new KeyValuePair<Result, Result>(consuming.Key, consuming.Value));
                            if (resultList.Count > 50)
                            {
                                this.RunBulkInsert(resultList);
                                resultList.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            KeyValuePair<Result, Result> keyValuePair = resultList.LastOrDefault<KeyValuePair<Result, Result>>();
                            string str = string.Empty;
                            if (keyValuePair.Key != null)
                                str = " Source : " + keyValuePair.Key.SourceKey + " Sink : " + keyValuePair.Key.SinkKey;
                            BlockAlgoHelperClass.AppendFileLog(ex.Message + " " + str);
                        }
                    }
                    this.RunBulkInsert(resultList);
                    resultList.Clear();
                }
                catch (Exception ex)
                {
                    KeyValuePair<Result, Result> keyValuePair = resultList.LastOrDefault<KeyValuePair<Result, Result>>();
                    string str = string.Empty;
                    if (keyValuePair.Key != null)
                        str = " Source : " + keyValuePair.Key.SourceKey + " Sink : " + keyValuePair.Key.SinkKey;
                    BlockAlgoHelperClass.AppendFileLog(ex.Message + " " + str);
                }
            });
            this.taskResultLog = new Task((Action)(() => action()));
            this.taskResultLog.Start();
        }

        public void RunDBCollection2()
        {
            if (this.SigmaDbConnection.State != ConnectionState.Open)
                this.SigmaDbConnection.Open();
            Action action = (Action)(() =>
            {
                try
                {
                    foreach (KeyValuePair<Result, Result> consuming in this.resultCollection.GetConsumingEnumerable())
                    {
                        Result key = consuming.Key;
                        Result result = consuming.Value;
                        double num = key.HoursWin / key.HoursCleared;
                        BlockAlgoHelperClass.AppendConsole("Trying to Insert");
                        this.mInsertResultsCommand.Parameters["@sourcename"].Value = (object)key.SourceName;
                        this.mInsertResultsCommand.Parameters["@sinkname"].Value = (object)key.SinkName;
                        this.mInsertResultsCommand.Parameters["@analysistype"].Value = (object)key.Analysistype.ToString();
                        this.mInsertResultsCommand.Parameters["@price"].Value = (object)key.MaxBid;
                        this.mInsertResultsCommand.Parameters["@sumvalue"].Value = (object)key.Sum;
                        this.mInsertResultsCommand.Parameters["@maxwin"].Value = (object)key.Max;
                        this.mInsertResultsCommand.Parameters["@maxloss"].Value = (object)(key.Min == double.MaxValue ? 0.0 : key.Min);
                        this.mInsertResultsCommand.Parameters["@riskreward"].Value = (object)(num * key.Max / ((1.0 - num) * Math.Abs(key.Min)));
                        this.mInsertResultsCommand.Parameters["@countdays"].Value = (object)30;
                        this.mInsertResultsCommand.Parameters["@countcleared"].Value = (object)key.CountCleared;
                        if (key.HoursCleared != 0.0)
                            this.mInsertResultsCommand.Parameters["@pctwin"].Value = (object)(key.HoursWin / key.HoursCleared);
                        this.mInsertResultsCommand.Parameters["@calcnumber"].Value = (object)key.CalcNum;
                        this.mInsertResultsCommand.Parameters["@yearlydownside"].Value = (object)result.Min;
                        this.mInsertResultsCommand.Parameters["@yearlyriskreward"].Value = (object)(result.Min == double.MaxValue ? 0.0 : Math.Abs(result.Max / result.Min));
                        this.mInsertResultsCommand.Parameters["@sourcenodekey"].Value = (object)key.SourceKey;
                        this.mInsertResultsCommand.Parameters["@sinknodekey"].Value = (object)key.SinkKey;
                        this.mInsertResultsCommand.Parameters["@avgda"].Value = (object)key.DANum;
                        this.mInsertResultsCommand.Parameters["@savedtime"].Value = (object)key.SavedDate;
                        this.mInsertResultsCommand.Parameters["@marketdate"].Value = (object)key.EndDate;
                        this.mInsertResultsCommand.Parameters["@hourCleared"].Value = (object)key.HoursCleared;
                        this.mInsertResultsCommand.Parameters["@MustTakeSum"].Value = (object)key.MustTakeSum;
                        this.mInsertResultsCommand.Parameters["@AMustTakeSum"].Value = (object)result.MustTakeSum;
                        this.mInsertResultsCommand.Parameters["@DailyMustTakeMin"].Value = (object)key.MustTakeMin;
                        this.mInsertResultsCommand.Parameters["@ADailyMustTakeMin"].Value = (object)result.MustTakeMin;
                        this.mInsertResultsCommand.Parameters["@ASum"].Value = (object)result.Sum;
                        this.mInsertResultsCommand.Parameters["@AAvg"].Value = (object)(result.Sum / result.HoursCleared);
                        this.mInsertResultsCommand.Parameters["@Amin"].Value = (object)result.HourlyMin;
                        this.mInsertResultsCommand.Parameters["@Amax"].Value = (object)result.HourlyMax;
                        this.mInsertResultsCommand.Parameters["@AStdDev"].Value = (object)result.StdDeviation;
                        this.mInsertResultsCommand.Parameters["@AWinPct"].Value = (object)(result.HoursWin / result.HoursCleared);
                        this.mInsertResultsCommand.Parameters["@AClearPct"].Value = (object)result.AClearPct;
                        this.mInsertResultsCommand.Parameters["@DailyMin"].Value = (object)key.Min;
                        this.mInsertResultsCommand.Parameters["@DailyMax"].Value = (object)key.Max;
                        this.mInsertResultsCommand.Parameters["@DailyAvg"].Value = (object)key.DailyAverage;
                        this.mInsertResultsCommand.Parameters["@ADailyMin"].Value = (object)result.Min;
                        this.mInsertResultsCommand.Parameters["@ADailyMax"].Value = (object)result.Max;
                        this.mInsertResultsCommand.Parameters["@ADailyAvg"].Value = (object)result.DailyAverage;
                        this.mInsertResultsCommand.Parameters["@SumToMax"].Value = (object)(key.Sum / key.Max);
                        this.mInsertResultsCommand.Parameters["@Sharpe"].Value = (object)key.Sharpe;
                        this.mInsertResultsCommand.Parameters["@Skew"].Value = (object)key.Skew;
                        this.mInsertResultsCommand.Parameters["@Kurtosis"].Value = (object)key.Kurtosis;
                        this.mInsertResultsCommand.Parameters["@DollarPerMW"].Value = (object)(key.Sum / key.HoursCleared);
                        this.mInsertResultsCommand.Parameters["@WeeklyWin"].Value = (object)key.IsWithinWeek;
                        this.mInsertResultsCommand.Parameters["@StdDev"].Value = (object)key.StdDeviation;
                        this.mInsertResultsCommand.ExecuteNonQuery();
                        BlockAlgoHelperClass.AppendConsole("Data Inserted " + key.SourceName + "->" + key.SinkName);
                    }
                }
                catch
                {
                }
                finally
                {
                    if (this.SigmaDbConnection.State != ConnectionState.Closed)
                        this.SigmaDbConnection.Close();
                }
            });
            this.taskResultLog = new Task((Action)(() => action()));
            this.taskResultLog.Start();
        }

        private DataTable GetHistoryDataTable()
        {
            DataTable dataTable = new DataTable();
            foreach (KeyValuePair<string, string> nameType in this.nameTypeDictionary)
            {
                if (nameType.Value == "s")
                    dataTable.Columns.Add(nameType.Key, typeof(string));
                else if (nameType.Value == "d")
                    dataTable.Columns.Add(nameType.Key, typeof(Decimal));
                else if (nameType.Value == "t")
                    dataTable.Columns.Add(nameType.Key, typeof(DateTime));
                else if (nameType.Value == "i")
                    dataTable.Columns.Add(nameType.Key, typeof(int));
            }
            return dataTable;
        }

        private void RunBulkInsert(List<KeyValuePair<Result, Result>> resultList)
        {
            BlockAlgoHelperClass.AppendConsole("Trying to Insert");
            DataTable historyDataTable1 = this.GetHistoryDataTable();
            Action<KeyValuePair<Result, Result>, DataTable> addAction = (Action<KeyValuePair<Result, Result>, DataTable>)((item, table) =>
            {
                DataRow row = table.NewRow();
                Result key = item.Key;
                Result result = item.Value;
                result.SetValues();
                key.SetValues();
                row["sourcename"] = (object)key.SourceName;
                row["sinkname"] = (object)key.SinkName;
                row["analysistype"] = (object)key.Analysistype.ToString();
                row["price"] = (object)this.DoubleRound(key.MaxBid, 2);
                row["sumvalue"] = (object)this.DoubleRound(key.Sum, 2);
                row["maxwin"] = (object)this.DoubleRound(key.Max, 2);
                row["maxloss"] = (object)this.DoubleRound(key.Min == double.MaxValue ? 0.0 : key.Min, 2);
                row["riskreward"] = (object)key.RiskReward;
                row["countdays"] = (object)30;
                row["countcleared"] = (object)key.CountCleared;
                row["mw"] = (object)25;
                if (key.HoursCleared != 0.0)
                    row["pctwin"] = (object)this.DoubleRound(key.HoursWin / key.HoursCleared, 4);
                row["calcnumber"] = (object)this.DoubleRound(key.CalcNum, 4);
                // row["yearlydownside"] = (object)this.DoubleRound(result.Min, 2);
                row["yearlydownside"] = (object)this.DoubleRound(result.Min == double.MaxValue ? 0.0 : result.Min, 2);
                row["yearlyriskreward"] = (object)result.RiskReward;
                row["sourcenodekey"] = (object)key.SourceKey;
                row["sinknodekey"] = (object)key.SinkKey;
                row["avgda"] = (object)this.DoubleRound(key.DANum, 4);
                row["savedtime"] = (object)key.SavedDate;
                row["marketdate"] = (object)key.EndDate;
                row["hoursCleared"] = (object)key.HoursCleared;
                row["MustTakeSum"] = (object)this.DoubleRound(key.MustTakeSum, 2);
                row["AMustTakeSum"] = (object)this.DoubleRound(result.MustTakeSum, 2);
                row["DailyMustTakeMin"] = (object)this.DoubleRound(key.MustTakeMin, 2);
                row["ADailyMustTakeMin"] = (object)this.DoubleRound(result.MustTakeMin, 2);
                row["ASum"] = (object)this.DoubleRound(result.Sum, 2);
                row["AAvg"] = (object)this.DoubleRound(result.Sum / result.HoursCleared, 4);
                row["Amin"] = (object)this.DoubleRound(result.HourlyMin, 2);
                row["Amax"] = (object)this.DoubleRound(result.HourlyMax, 2);
                row["AStdDev"] = (object)this.DoubleRound(result.StdDeviation, 4);
                row["AWinPct"] = (object)this.DoubleRound(result.HoursWin / result.HoursCleared, 2);
                row["AClearPct"] = (object)this.DoubleRound(result.AClearPct, 4);
                row["DailyMin"] = (object)this.DoubleRound(key.Min == double.MaxValue ? 0.0 : key.Min, 2);
                row["DailyMax"] = (object)this.DoubleRound(key.Max, 2);
                row["DailyAvg"] = (object)this.DoubleRound(key.DailyAverage, 4);
                // row["ADailyMin"] = (object)this.DoubleRound(result.Min, 2);
                row["ADailyMin"] = (object)this.DoubleRound(result.Min == double.MaxValue ? 0.0 : result.Min, 2);
                row["ADailyMax"] = (object)this.DoubleRound(result.Max, 2);
                row["ADailyAvg"] = (object)this.DoubleRound(result.DailyAverage, 4);
                row["SumToMax"] = (object)key.SumToMax;
                row["Sharpe"] = (object)this.DoubleRound(key.Sharpe, 4);
                row["Skew"] = (object)this.DoubleRound(key.Skew, 2);
                row["Kurtosis"] = (object)this.DoubleRound(key.Kurtosis, 4);
                row["DollarPerMW"] = (object)this.DoubleRound(key.Sum / key.HoursCleared, 4);
                row["WeeklyWin"] = (object)key.IsWithinWeek;
                row["StdDev"] = (object)this.DoubleRound(key.StdDeviation, 4);
                row["WeeklySumValue"] = (object)this.DoubleRound(key.WeeklySum, 4);
                row["WeeklyMaxWin"] = (object)this.DoubleRound(key.WeeklyMaxWin, 4);
                row["WeeklyMaxLossRT"] = double.MaxValue == key.WeeklyMaxLossRt ? (object)0 : (object)this.DoubleRound(key.WeeklyMaxLossRt, 4);
                row["WeeklyPctWin"] = (object)this.DoubleRound(key.WeeklyPctWin * 100.0, 4);
                row["WeeklyCountCleared"] = (object)key.WeeklyCountCleared;
                row["AnnualMaxLossRT"] = (object)this.DoubleRound(result.YearlyMinRt, 4);
                row["MonthlyMaxLossRT"] = (object)this.DoubleRound(key.MonthlyMinRT, 4);
                table.Rows.Add(row);
            });
            Action<DataTable> bulkInsert = (Action<DataTable>)(table =>
            {
                try
                {
                    if (this.SigmaDbConnection.State != ConnectionState.Open)
                        this.SigmaDbConnection.Open();
                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(this.SigmaDbConnection);
                    sqlBulkCopy.DestinationTableName = this.tableName;
                    foreach (KeyValuePair<string, string> nameType in this.nameTypeDictionary)
                        sqlBulkCopy.ColumnMappings.Add(nameType.Key, nameType.Key);
                    sqlBulkCopy.WriteToServer(table);
                }
                catch
                {
                }
            });
            Action action = (Action)(() =>
            {
                foreach (KeyValuePair<Result, Result> result in resultList)
                {
                    try
                    {
                        DataTable historyDataTable2 = this.GetHistoryDataTable();
                        addAction(result, historyDataTable2);
                        bulkInsert(historyDataTable2);
                    }
                    catch (Exception ex)
                    {
                        string str = string.Empty;
                        if (result.Key != null)
                            str = " Source : " + result.Key.SourceKey + " Sink : " + result.Key.SinkKey;
                        BlockAlgoHelperClass.AppendFileLog(ex.Message + " " + str);
                        BlockAlgoHelperClass.AppendFileLog("Inserting record by record");
                    }
                }
            });
            try
            {
                foreach (KeyValuePair<Result, Result> result in resultList)
                    addAction(result, historyDataTable1);
            }
            catch
            {
                if (this.SigmaDbConnection.State != ConnectionState.Closed)
                    this.SigmaDbConnection.Close();
                this.InitConnection();
                action();
                return;
            }
            try
            {
                bulkInsert(historyDataTable1);
            }
            catch (Exception ex)
            {
                if (this.SigmaDbConnection.State != ConnectionState.Closed)
                    this.SigmaDbConnection.Close();
                this.InitConnection();
                KeyValuePair<Result, Result> keyValuePair = resultList.LastOrDefault<KeyValuePair<Result, Result>>();
                string str = string.Empty;
                if (keyValuePair.Key != null)
                    str = " Source : " + keyValuePair.Key.SourceKey + " Sink : " + keyValuePair.Key.SinkKey;
                BlockAlgoHelperClass.AppendFileLog(ex.Message + " " + str);
                BlockAlgoHelperClass.AppendFileLog("Inserting record by record");
                action();
            }
            finally
            {
                if (this.SigmaDbConnection.State != ConnectionState.Closed)
                    this.SigmaDbConnection.Close();
            }
        }

        private double DoubleRound(double dValue, int round)
        {
            if (double.IsInfinity(dValue) || double.IsNaN(dValue))
                dValue = 0.0;
            return Math.Round(dValue, round);
        }
    }
}
