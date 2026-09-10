#define FAST
#define ERCOT
//#define WithoutFilters
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using Vayu.BlockAlgorithmLibraryNamespace;

namespace Vayu.BlockAlgorithmNamespace
{
    public class BlockAlgoThread
    {
        private int mNumPaths;
        private SourceSink[] mCalculateNodeList;
        private DateTime mStartDate;
        private DateTime mEndDate;
        private DateTime mSavedDate;
        private Dictionary<int, string> mNodeHash;
        private ConcurrentDictionary<string, double> mRtHash;
        private ConcurrentDictionary<string, double> mDaHash;

#if Blocking
#else

        public string ResultTable { get; set; }
#endif

        public BlockAlgoThread(int numPaths, SourceSink[] calculateNodeList, DateTime startDate, DateTime endDate, ConcurrentDictionary<string, double> rtHash,
            ConcurrentDictionary<string, double> daHash, DateTime savedDate, Dictionary<int, string> dicNode)
        {
            mNumPaths = numPaths;
            mCalculateNodeList = calculateNodeList;
            mStartDate = startDate;
            mEndDate = endDate;
            mRtHash = rtHash;
            mDaHash = daHash;
            mSavedDate = savedDate;
            mNodeHash = dicNode;
        }

        public void Run()
        {
            string machineName = Environment.MachineName;
            int numCompleted = 0;
            ConcurrentDictionary<string, Dictionary<int, Dictionary<double, Result>>> resultHash = new ConcurrentDictionary<string, Dictionary<int, Dictionary<double, Result>>>();
            ConcurrentDictionary<string, Dictionary<int, Dictionary<double, Result>>> result30Hash = new ConcurrentDictionary<string, Dictionary<int, Dictionary<double, Result>>>();
            PriceHash priceHash = new PriceHash();
#if FAST
            Parallel.ForEach(mCalculateNodeList, new ParallelOptions { MaxDegreeOfParallelism = 7 }, sourceSink =>
#else
            foreach (SourceSink sourceSink in mCalculateNodeList)
#endif
            {
                
                try
                {
                    #region NodeLoop
                    string sourceSinkKey = sourceSink.SourceNodeKey + "?" + sourceSink.SinkNodeKey;
                    priceHash.AddParent(sourceSinkKey);
                    if(sourceSink.SourceNodeKey== 57194 && sourceSink.SinkNodeKey== 72073)
                    {

                    }
                    DateTime tempDate1 = mStartDate;
                    while (tempDate1 <= mEndDate)
                    {
                        int dayCount = (mEndDate - tempDate1).Days;
                        bool isMonthly = (dayCount <= 31);
                        bool isWeekly = (dayCount <= 7);

                        #region Year Loop
                        for (int row = 0; row < 22; row++) //SPChnage row = 0; 
                        {
                            #region HourLoop

                            double min = -1;//-1
                            double[] testDartSpreads = new double[3];
                            double[] testDaSpreads = new double[3];
                            double mSum = 0;
                            int hourCount = 0;
                            double minRT = double.MaxValue;
                            #region DART Min Finder Loop
                            for (int i = 0; i < 3; i++)
                            {
                                int hour = i + row + 1;
                                try
                                {
                                    DateTime tempDate = hour == 24 ? tempDate1.AddDays(1) : tempDate1.AddHours(hour);
                                    string sourceKey = tempDate.ToString() + sourceSink.SourceNodeKey.ToString();
                                    string sinkKey = tempDate.ToString() + sourceSink.SinkNodeKey.ToString();
                                    if (mRtHash.ContainsKey(sourceKey) && mDaHash.ContainsKey(sourceKey) &&
                                        mRtHash.ContainsKey(sinkKey) && mDaHash.ContainsKey(sinkKey) &&
                                        !double.IsNaN(mRtHash[sinkKey]) && !double.IsNaN(mDaHash[sinkKey]) &&
                                        !double.IsNaN(mRtHash[sourceKey]) && !double.IsNaN(mDaHash[sourceKey]))
                                    {
                                        //Console.WriteLine("something");
                                        double sourceDa = mDaHash[sourceKey];
                                        double sourceRt = mRtHash[sourceKey];
                                        double sinkDa = mDaHash[sinkKey];
                                        double sinkRt = mRtHash[sinkKey];
                                        double daSpread = (sinkDa - sourceDa);
                                        double rtSpread = (sinkRt - sourceRt);
                                        if (rtSpread < minRT)
                                            minRT = rtSpread;
                                        double dartHash = ((sinkRt - sourceRt) - (sinkDa - sourceDa));
                                        mSum += dartHash;
                                        testDartSpreads[i] = dartHash;
                                        testDaSpreads[i] = daSpread;
                                        hourCount++;
                                        while (daSpread < min)
                                        {
                                            min--;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }

                            if (hourCount != 3)
                                continue;

                           // Console.WriteLine("Hour count 3 " + sourceSinkKey);
                            #endregion

                            priceHash.AddUpdateHourData(sourceSinkKey, row, mSum, isMonthly, minRT, isWeekly);
#if ERCOT
                            for (double bid = min; bid <= 60; bid += 0.1)//30
                            {
                                //SPCH 
                                //if (bid > 10)
                                //    bid += 0.1;
                                //
#else
                            for (double bid = min; bid <= 20; bid += 0.1)
                                {
#endif

                                if (bid > -10)
                                {

                                    #region BidLoop

                                    bid = Math.Round(bid, 2);
                                    double da = 0;
                                    double dart = 0;
                                    double num = 0;
                                    int winCount = 0;
                                    double[] dartSpreads = new double[3];
                                    double loopDart = 0;
                                    double squareSum = 0;

                                    #region SummationLooop
                                    for (int i = 0; i < 3; i++)
                                    {
                                        loopDart = testDartSpreads[i];

                                        if (testDaSpreads[i] < bid)
                                        {
                                            //Console.WriteLine("Bid price" + bid+" Da Price"+ testDaSpreads[i]);
                                            if (loopDart > 0)
                                                winCount++;

                                            dart += loopDart;
                                            da += testDaSpreads[i];
                                            dartSpreads[i] = loopDart;
                                            squareSum += (loopDart * loopDart);
                                            num++;
                                        }
                                    }

                                    #endregion

                                    if (num == 0)
                                        continue;

                                    da = da / num;

                                    if (da >= bid)
                                        continue;

                                    if (isMonthly)
                                        PerformDayAction(result30Hash, sourceSinkKey,
                                            row, bid, dartSpreads, dart, da, winCount, num, isWeekly, squareSum);

                                    PerformDayAction(resultHash, sourceSinkKey,
                                        row, bid, dartSpreads, dart, da, winCount, num, isWeekly, squareSum);
                                    #endregion
                                }


                            }
                            #endregion
                        }
                        tempDate1 = tempDate1.AddDays(1);
                        // Console.WriteLine(tempDate1);
                        #endregion
                    }
                    numCompleted++;
                    //BlockAlgoHelperClass.BlockAlgoHelper.ConsoleCollection.Add("Completed " + sourceSinkKey + " " + (mNumPaths + numCompleted));

                    #endregion
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
#if FAST
            });
#else
            }
#endif
                 List<string> sourceSinkList = resultHash.Keys.ToList<string>();

            #region SaveResult
#if FAST
            Parallel.ForEach(sourceSinkList, new ParallelOptions { MaxDegreeOfParallelism = 7 }, sourceSink =>
#else
            foreach (string sourceSink in sourceSinkList)
#endif
            {
                
                try
                {
                    string[] sourceSinks = sourceSink.Split('?');
                    if (resultHash.ContainsKey(sourceSink) && result30Hash.ContainsKey(sourceSink))
                    {

                        Dictionary<int, Dictionary<double, Result>> rowHash = resultHash[sourceSink];
                        Dictionary<int, Dictionary<double, Result>> row30Hash = result30Hash[sourceSink];
                        List<int> rowList = rowHash.Keys.ToList<int>();
                        foreach (int row in rowList)
                        {
                            #region hourloop
                            Dictionary<double, Result> bidHash = rowHash[row];
                            List<double> bidList = bidHash.Keys.ToList<double>();
                            bidList.Sort();
                            double maxSum = double.MinValue;
                            double maxBid = double.MinValue;
                            Result maxResult = null;

                            #region MaxBidFinder
                            foreach (double bid in bidList)
                            {
                                Result resultChild = bidHash[bid];
#if WithoutFilters
#else
                                if (maxSum < resultChild.Sum && resultChild.Min > -25)
#endif
                                {
                                    maxSum = resultChild.Sum;
                                    maxBid = bid;
                                    maxResult = resultChild;
                                    //double riskRewardLoop = resultChild.Min == double.MinValue ? 1000 : Math.Abs(resultChild.Max / resultChild.Min);
                                }
                            }
                            #endregion

                            if (!row30Hash.ContainsKey(row))
                                continue;

                            Dictionary<double, Result> bid30Hash = row30Hash[row];
                            //BlockAlgoHelperClass.BlockAlgoHelper.ConsoleCollection.Add("Max Bid " + maxBid);
                            if (!bid30Hash.ContainsKey(maxBid))
                                continue;

                            Result result = bid30Hash[maxBid];
                            // double riskReward = result.Min == double.MinValue ? 1000 : Math.Abs(result.Max / result.Min);
                            double pctWin = result.HoursWin / result.HoursCleared;
                            double riskReward = (pctWin * result.Max) / ((1 - pctWin) * Math.Abs(result.Min));

                            if (Math.Abs(result.Min) < 0.01)
                                riskReward = 30000;

                            double calc = ((double)result.WinCount / (double)result.CountCleared) * riskReward;
                            double sumOfMax = result.Sum / result.Max;
                            double da = result.DA / result.CountCleared;
                            //BlockAlgoHelperClass.BlockAlgoHelper.ConsoleCollection.Add("calc 1.2 : " + calc + " sumOfMax 2 : " + sumOfMax + " riskReward 2.5 : " + riskReward + " Result min -25 : " + result.Min);
#if WithoutFilters
#else
                            if (calc >= 1.2 && sumOfMax >= 2 && riskReward > 2.5 && result.Min >= -25)
#endif
                            {
                                string sourcekey = sourceSinks[0];
                                string sinkkey = sourceSinks[1];
                                MustTakeStub stub = priceHash.GetStub(sourceSink, row);

                                result.SourceName = mNodeHash[Int32.Parse(sourcekey)];
                                result.SinkName = mNodeHash[Int32.Parse(sinkkey)];
                                result.SourceKey = sourcekey;
                                result.SinkKey = sinkkey;
                                result.MaxBid = maxBid;

                                if (calc > 100000.0)
                                    result.CalcNum = calc;
                                else
                                    result.CalcNum = 100000.0;
                                
                                result.DANum = da;
                                result.MustTakeSum = stub.MonthlyDARTSum;
                                result.MustTakeMin = stub.MonthlyDARTMin; // .AMustTakeSum;
                                maxResult.MustTakeSum = stub.YearDARTSum;// //MustTakeMin;
                                maxResult.MustTakeMin = stub.YearDARTMin;// //.AMustTakeMin;
                                maxResult.AClearPct = maxResult.HoursCleared / stub.YearCount;
                                //priceHash.GetTotalAnnualHourCount(sourcekey, sinkkey, row);

                                if (double.IsNaN(maxResult.AClearPct) || double.IsInfinity(maxResult.AClearPct))
                                    maxResult.AClearPct = 0;

                                result.Analysistype = (row + 1);
                                result.SavedDate = mSavedDate;
                                result.EndDate = mEndDate;
                                //
                                result.WeeklyPctWin = (double)result.WeekluWinCount / result.WeeklyCountCleared;
                                result.WeeklyMaxLossRt = stub.WeeklyMinRT;
                                result.MonthlyMinRT = stub.MonthlyMinRT;
                                maxResult.YearlyMinRt = stub.YearlyMinRt;

                                BlockAlgoHelperClass.BlockAlgoHelper.IncrementValidPathCount();
                                BlockAlgoHelperClass.BlockAlgoHelper.ResultCollection.Add(new KeyValuePair<Result, Result>(result, maxResult));
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    BlockAlgoHelperClass.AppendFileLog("Exception Produced " + ex.Message);
                    Console.WriteLine(ex);
                }
#if FAST
            });
#else
            }
#endif
            #endregion
        }

        private void PerformDayAction(ConcurrentDictionary<string, Dictionary<int, Dictionary<double, Result>>> hashResult,
            string sourceSinkKey, int row, double bid, double[] dartSpreads, double dart, double da, int winCount, double num, bool isWeekly, double squareSum)
        {
            try
            {
                Dictionary<int, Dictionary<double, Result>> hashRow = null;
                Dictionary<double, Result> hashBid = null;
                Result resultRec = null;
                double dayAvg = (dart / num);

                hashResult.TryGetValue(sourceSinkKey, out hashRow);
                if (hashRow == null)
                {
                    hashRow = new Dictionary<int, Dictionary<double, Result>>();
                    hashResult.TryAdd(sourceSinkKey, hashRow);
                }

                hashRow.TryGetValue(row, out hashBid);
                if (hashBid == null)
                {
                    hashBid = new Dictionary<double, Result>();
                    hashRow.Add(row, hashBid);
                }

                hashBid.TryGetValue(bid, out resultRec);
                if (resultRec == null)
                {
                    resultRec = new Result();
                    hashBid.Add(bid, resultRec);
                }

                if (dart > 0)
                {
                    resultRec.WinCount++;
                    if (isWeekly)
                        resultRec.IsWithinWeek = true;
                }

                if (resultRec.Max < dart)
                    resultRec.Max = dart;

                if (resultRec.Min > dart && dart < 0)
                    resultRec.Min = dart;

                foreach (var item in dartSpreads)
                {
                    if (item > resultRec.HourlyMax)
                        resultRec.HourlyMax = item;
                }

                foreach (var item in dartSpreads)
                {
                    if (item < resultRec.HourlyMin)
                        resultRec.HourlyMin = item;
                }
                if (isWeekly)
                {
                    resultRec.WeeklySum += dart;
                    resultRec.WeeklyCountCleared++;
                    if (dart > resultRec.WeeklyMaxWin)
                        resultRec.WeeklyMaxWin = dart;
                    if (dart > 0)
                        resultRec.WeekluWinCount++;
                }

                resultRec.CountCleared++;
                resultRec.Sum += dart;
                resultRec.HoursWin += winCount;
                resultRec.DA += da;
                resultRec.HoursCleared += num;
                resultRec.DailyAverage += dayAvg;
                resultRec.SumOfSquareDART += squareSum;
            }
            catch (Exception e)
            {
                BlockAlgoHelperClass.AppendFileLog(e.Message);
            }
        }
    }
}