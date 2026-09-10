using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.BlockAlgorithmNamespace
{
    public class PriceHash : ConcurrentDictionary<string, ConcurrentDictionary<int, MustTakeStub>>
    {
        public void AddParent(string sourceSinkKey)
        {
            if (string.IsNullOrEmpty(sourceSinkKey))
                return;

            if (!this.ContainsKey(sourceSinkKey))
            {
                this.TryAdd(sourceSinkKey, new ConcurrentDictionary<int, MustTakeStub>());

                for (int row = 0; row < 22; row++)
                    this[sourceSinkKey].TryAdd(row, new MustTakeStub());
            }
        }

        public void AddUpdateHourData(string sourceSink, int hour, double dartSpread, bool isMonthly, double minRtSpread, bool isWeekly)
        {
            try
            {
                this[sourceSink][hour].Accept(dartSpread, isMonthly, minRtSpread, isWeekly);
            }
            catch { }
        }

        public MustTakeStub GetStub(string sourceSink, int hour)
        {
            return this[sourceSink][hour];
        }
    }

    public class MustTakeStub
    {
        public double YearDARTSum;
        public double MonthlyDARTSum;
        public double YearDARTMin = double.MaxValue;
        public double MonthlyDARTMin = double.MaxValue;
        public double WeeklyMinDART = double.MaxValue;
        public double WeeklyMaxDART = double.MinValue;
        public int YearCount;
        public double YearlyMinRt = double.MaxValue;
        public double MonthlyMinRT = double.MaxValue;
        public double WeeklyMinRT = double.MaxValue;

        public void Accept(double dartSpread, bool isMonthly, double minRtSpread, bool isWeekly)
        {
            YearDARTSum += dartSpread;
            YearCount += 3;

            if (YearDARTMin > dartSpread)
                YearDARTMin = dartSpread;

            if (minRtSpread < YearlyMinRt)
                YearlyMinRt = minRtSpread;

            if (isMonthly)
            {
                MonthlyDARTSum += dartSpread;
                if (MonthlyDARTMin > dartSpread)
                    MonthlyDARTMin = dartSpread;
                //
                if (minRtSpread < MonthlyMinRT)
                    MonthlyMinRT = minRtSpread;
            }

            if (isWeekly)
            {
                if (minRtSpread < WeeklyMinRT)
                    WeeklyMinRT = minRtSpread;
            }

        }
    }
}
