using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Vayu.BlockAlgorithmNamespace
{
    public class Result
    {
        public double WeeklySum;
        public double WeeklyMaxWin;
        public double WeeklyMaxLossRt;
        public double WeeklyPctWin;
        public int WeeklyCountCleared;
        public double YearlyMinRt;
        public double WeekluWinCount;
        public double MonthlyMinRT;
        //
        public double Max;
        public double WeeklyMax;
        public double Min;
        public double WeeklyMin;
        public double DA;
        public int Count;
        public int WinCount;
        public int CountCleared;

        public double Sum;
        public string IncDec;

        public double DailyAverage;
        public double HourlyMin;
        public double HourlyMax;
        public double HoursCleared;
        public double HoursWin;
        public double MustTakeMin;
        public double MustTakeSum;
        public bool IsWithinWeek;
        public double SumOfSquareDART;
        public double CubeOfSquareDART;

        public double StdDeviation;
        public double Sharpe;
        public double Skew;
        public double Kurtosis;

        public string SourceName;
        public string SinkName;
        public int Analysistype;
        public double MaxBid;
        public string SourceKey;
        public string SinkKey;
        public double CalcNum;
        public double DANum;
        public DateTime SavedDate;
        public DateTime EndDate;
        public double AClearPct;

        public double SumToMax;
        public double RiskReward;

        public Result()
        {
            HourlyMin = double.MaxValue;
            HourlyMax = double.MinValue;
            MustTakeMin = double.MaxValue;
            Max = double.MinValue;
            Min = double.MaxValue;
        }

        public void SetValues()
        {
            if (HoursCleared == 0)
                return;

            double step1 = (Sum * Sum) / HoursCleared;
            double step2 = (SumOfSquareDART - step1) / HoursCleared;
            if (step2 > 0)
                StdDeviation = Math.Sqrt(step2);
            else
                return;

            //Sharpe
            Sharpe = (Sum / HoursCleared) / StdDeviation;

            //Skew

            //Kurtosis


            //SumToMax
            SumToMax = Math.Round(Sum / Max, 4);
            if (double.IsInfinity(SumToMax) || double.IsNaN(SumToMax))
                SumToMax = 0;

            //Risk reward
            //RiskReward = Math.Round(Min == double.MaxValue ? 0 : Math.Abs(Max / Min), 2);
            double pctWin =HoursWin/ HoursCleared;
            RiskReward = (pctWin * Max) / ((1 - pctWin) * Math.Abs(Min));
            if (double.IsInfinity(RiskReward) || double.IsNaN(RiskReward))
                RiskReward = 0;

            if (30000.0 < RiskReward)
                RiskReward = 30000.0;
        }

        public void SetValues(IEnumerable<double> clearedDART)
        {
            if (HoursCleared == 0)
                return;

            double HourlyAverage = Sum / HoursCleared;
            double average = clearedDART.Average();
            double sumOfSquaresOfDifferences = clearedDART.Select(val => (val - average) * (val - average)).Sum();

            if (sumOfSquaresOfDifferences > 0)
                StdDeviation = Math.Sqrt(sumOfSquaresOfDifferences / clearedDART.Count());
            else
                return;

            //Sharpe
            Sharpe = (Sum / HoursCleared) / StdDeviation;

            //Skew
            {
                double step2 = 0;
                double step3 = HoursCleared - 1;
                step3 *= StdDeviation * StdDeviation * StdDeviation;

                foreach (var item in clearedDART)
                {
                    double step1 = item - HourlyAverage;
                    step1 = step1 * step1 * step1;
                    step2 += step1;
                }

                if (step3 != 0)
                    Skew = step2 / step3;
            }

            //Kurtosis
            {
                double step2 = 0;
                double step3 = HoursCleared - 1;
                step3 *= StdDeviation * StdDeviation * StdDeviation * StdDeviation;

                foreach (var item in clearedDART)
                {
                    double step1 = item - HourlyAverage;
                    step1 = step1 * step1 * step1 * step1;
                    step2 += step1;
                }

                if (step3 != 0)
                    Kurtosis = step2 / step3;

                Kurtosis -= 3;
            }
        }
    }
}
