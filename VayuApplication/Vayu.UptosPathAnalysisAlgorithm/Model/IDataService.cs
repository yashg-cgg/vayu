using System;
using System.Collections.Generic;

namespace Vayu.UptosPathAnalysisAlgorithm.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Loads the database commands.
        /// </summary>
        void loadDBCommands();
        /// <summary>
        /// Gets the ees robot list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="RobotType">Type of the robot.</param>
        /// <param name="date">The date.</param>
        void GetEESRobotList(Action<List<RobotTypeList>, Exception> callback, string RobotType, DateTime? date);
        /// <summary>
        /// Gets the c3 po maximum date.
        /// </summary>
        /// <returns></returns>
        DateTime? GetBlockMaxDate();
    }

    /// <summary>
    /// 
    /// </summary>
    public class RobotTypeList
    {
        /// <summary>
        /// Gets or sets the name of the source.
        /// </summary>
        /// <value>
        /// The name of the source.
        /// </value>
        public string SourceName { get; set; }
        /// <summary>
        /// Gets or sets the name of the sink.
        /// </summary>
        /// <value>
        /// The name of the sink.
        /// </value>
        public string SinkName { get; set; }
        /// <summary>
        /// Gets or sets the source zone.
        /// </summary>
        /// <value>
        /// The source zone.
        /// </value>
        public string AnalysisType { get; set; }
        /// <summary>
        /// Gets or sets the analysis type int.
        /// </summary>
        /// <value>
        /// The analysis type int.
        /// </value>
        public int AnalysisTypeInt { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price { get; set; }
        //public double Price { get; set; }
        /// <summary>
        /// Gets or sets the sum value.
        /// </summary>
        /// <value>
        /// The sum value.
        /// </value>
        public double SumValue { get; set; }
        // public string Hour { get; set; }
        //public double Price { get; set; }
        public double Correlation { get; set; }

        public string SourceZone { get; set; }
        /// <summary>
        /// Gets or sets the sink zone.
        /// </summary>
        /// <value>
        /// The sink zone.
        /// </value>
        public string SinkZone { get; set; }
        /// <summary>
        /// Gets or sets the type of the source node.
        /// </summary>
        /// <value>
        /// The type of the source node.
        /// </value>
        public string SourceNodeType { get; set; }
        /// <summary>
        /// Gets or sets the type of the sink node.
        /// </summary>
        /// <value>
        /// The type of the sink node.
        /// </value>
        public string SinkNodeType { get; set; }
        //public string Zone { get; set; }
        //public string NodeType { get; set; }
        /// <summary>
        /// Gets or sets the type of the analysis.
        /// </summary>
        /// <value>
        /// The type of the analysis.
        /// </value>
       // public string AnalysisType { get; set; }
        /// <summary>
        /// Gets or sets the analysis type int.
        /// </summary>
        /// <value>
        /// The analysis type int.
        /// </value>
       // public int AnalysisTypeInt { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
      //  public double Price { get; set; }
        //public double Price { get; set; }
        /// <summary>
        /// Gets or sets the sum value.
        /// </summary>
        /// <value>
        /// The sum value.
        /// </value>
      //  public double SumValue { get; set; }
        /// <summary>
        /// Gets or sets the maximum win.
        /// </summary>
        /// <value>
        /// The maximum win.
        /// </value>
        public double MaxWin { get; set; }
        /// <summary>
        /// Gets or sets the maximum loss.
        /// </summary>
        /// <value>
        /// The maximum loss.
        /// </value>
        public double MaxLoss { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the risk reward.
        /// </summary>
        /// <value>
        /// The risk reward.
        /// </value>
        public double RiskReward { get; set; }
        /// <summary>
        /// Gets or sets the count days.
        /// </summary>
        /// <value>
        /// The count days.
        /// </value>
        public double CountDays { get; set; }
        /// <summary>
        /// Gets or sets the count cleared.
        /// </summary>
        /// <value>
        /// The count cleared.
        /// </value>
        public double CountCleared { get; set; }
        /// <summary>
        /// Gets or sets the PCT win.
        /// </summary>
        /// <value>
        /// The PCT win.
        /// </value>
        public double PctWin { get; set; }
        /// <summary>
        /// Gets or sets the yearly up side.
        /// </summary>
        /// <value>
        /// The yearly up side.
        /// </value>
        public double YearlyUpSide { get; set; }
        /// <summary>
        /// Gets or sets the yearly risk reward.
        /// </summary>
        /// <value>
        /// The yearly risk reward.
        /// </value>
        public double YearlyRiskReward { get; set; }
        /// <summary>
        /// Gets or sets the average da.
        /// </summary>
        /// <value>
        /// The average da.
        /// </value>
        public double AvgDA { get; set; }
        /// <summary>
        /// Gets or sets the calculate number.
        /// </summary>
        /// <value>
        /// The calculate number.
        /// </value>
        public double CalcNumber { get; set; }
        /// <summary>
        /// Gets or sets the saved time.
        /// </summary>
        /// <value>
        /// The saved time.
        /// </value>
        public DateTime SavedTime { get; set; }
        public decimal RTMaxWinter { get; set; }
        public decimal RTMinWinter { get; set; }
        public decimal RTMaxSpring { get; set; }
        public decimal RTMinSpring { get; set; }
        public decimal RTMaxSummer { get; set; }
        public decimal RTMinSummer { get; set; }
        public decimal RTMaxFall { get; set; }
        public decimal RTMinFall { get; set; }
        public decimal RTMaxAll { get; set; }
        public decimal RTMinAll { get; set; }
        public DateTime PathMinRT { get; set; }
        public string SourceFuel { get; set; }
        public string SinkFuel { get; set; }
        /// <summary>
        /// Gets or sets the must take sum.
        /// </summary>
        /// <value>
        /// The must take sum.
        /// </value>
        public double MustTakeSum { get; set; }
        /// <summary>
        /// Gets or sets a must take sum.
        /// </summary>
        /// <value>
        /// a must take sum.
        /// </value>
        public double AMustTakeSum { get; set; }
        /// <summary>
        /// Gets or sets the daily must take minimum.
        /// </summary>
        /// <value>
        /// The daily must take minimum.
        /// </value>
        public double DailyMustTakeMin { get; set; }
        /// <summary>
        /// Gets or sets a daily must take minimum.
        /// </summary>
        /// <value>
        /// a daily must take minimum.
        /// </value>
        public double ADailyMustTakeMin { get; set; }
        /// <summary>
        /// Gets or sets a sum.
        /// </summary>
        /// <value>
        /// a sum.
        /// </value>
        public double ASum { get; set; }
        /// <summary>
        /// Gets or sets a average.
        /// </summary>
        /// <value>
        /// a average.
        /// </value>
        public double AAvg { get; set; }
        /// <summary>
        /// Gets or sets a minimum.
        /// </summary>
        /// <value>
        /// a minimum.
        /// </value>
        public double AMin { get; set; }
        /// <summary>
        /// Gets or sets a maximum.
        /// </summary>
        /// <value>
        /// a maximum.
        /// </value>
        public double AMax { get; set; }
        /// <summary>
        /// Gets or sets a standard dev.
        /// </summary>
        /// <value>
        /// a standard dev.
        /// </value>
        public double AStdDev { get; set; }
        /// <summary>
        /// Gets or sets a win PCT.
        /// </summary>
        /// <value>
        /// a win PCT.
        /// </value>
        public double AWinPct { get; set; }
        /// <summary>
        /// Gets or sets a clear PCT.
        /// </summary>
        /// <value>
        /// a clear PCT.
        /// </value>
        public double AClearPct { get; set; }
        /// <summary>
        /// Gets or sets the daily minimum.
        /// </summary>
        /// <value>
        /// The daily minimum.
        /// </value>
        public double DailyMin { get; set; }
        /// <summary>
        /// Gets or sets the daily maximum.
        /// </summary>
        /// <value>
        /// The daily maximum.
        /// </value>
        public double DailyMax { get; set; }
        /// <summary>
        /// Gets or sets the daily average.
        /// </summary>
        /// <value>
        /// The daily average.
        /// </value>
        public double DailyAvg { get; set; }
        /// <summary>
        /// Gets or sets a daily minimum.
        /// </summary>
        /// <value>
        /// a daily minimum.
        /// </value>
        public double ADailyMin { get; set; }
        /// <summary>
        /// Gets or sets a daily maximum.
        /// </summary>
        /// <value>
        /// a daily maximum.
        /// </value>
        public double ADailyMax { get; set; }
        /// <summary>
        /// Gets or sets a daily average.
        /// </summary>
        /// <value>
        /// a daily average.
        /// </value>
        public double ADailyAvg { get; set; }
        /// <summary>
        /// Gets or sets the sum to maximum.
        /// </summary>
        /// <value>
        /// The sum to maximum.
        /// </value>
        public double SumToMax { get; set; }
        /// <summary>
        /// Gets or sets the sharpe.
        /// </summary>
        /// <value>
        /// The sharpe.
        /// </value>
        public double Sharpe { get; set; }
        /// <summary>
        /// Gets or sets the skew.
        /// </summary>
        /// <value>
        /// The skew.
        /// </value>
        public double Skew { get; set; }
        /// <summary>
        /// Gets or sets the kurtosis.
        /// </summary>
        /// <value>
        /// The kurtosis.
        /// </value>
        public double Kurtosis { get; set; }
        /// <summary>
        /// Gets or sets the dollar per mw.
        /// </summary>
        /// <value>
        /// The dollar per mw.
        /// </value>
        public double DollarPerMW { get; set; }
        //
        /// <summary>
        /// Gets or sets the weekly sum value.
        /// </summary>
        /// <value>
        /// The weekly sum value.
        /// </value>
        public double WeeklySumValue { get; set; }
        /// <summary>
        /// Gets or sets the weekly maximum win.
        /// </summary>
        /// <value>
        /// The weekly maximum win.
        /// </value>
        public double WeeklyMaxWin { get; set; }
        /// <summary>
        /// Gets or sets the weekly maximum loss rt.
        /// </summary>
        /// <value>
        /// The weekly maximum loss rt.
        /// </value>
        public double WeeklyMaxLossRt { get; set; }
        /// <summary>
        /// Gets or sets the weekly PCT win.
        /// </summary>
        /// <value>
        /// The weekly PCT win.
        /// </value>
        public double WeeklyPctWin { get; set; }
        /// <summary>
        /// Gets or sets the weekly count cleared.
        /// </summary>
        /// <value>
        /// The weekly count cleared.
        /// </value>
        public int WeeklyCountCleared { get; set; }
        /// <summary>
        /// Gets or sets the annual maximum loss rt.
        /// </summary>
        /// <value>
        /// The annual maximum loss rt.
        /// </value>
        public double AnnualMaxLossRt { get; set; }
        /// <summary>
        /// Gets or sets the monthly maximum loss rt.
        /// </summary>
        /// <value>
        /// The monthly maximum loss rt.
        /// </value>
        public double MonthlyMaxLossRt { get; set; }
        //
        /// <summary>
        /// Gets or sets the inc decimal.
        /// </summary>
        /// <value>
        /// The inc decimal.
        /// </value>
        public string IncDec { get; set; }
        //
        /// <summary>
        /// Gets or sets the yearly down side.
        /// </summary>
        /// <value>
        /// The yearly down side.
        /// </value>
        public double YearlyDownSide { get; set; }
        /// <summary>
        /// Gets or sets the source node key.
        /// </summary>
        /// <value>
        /// The source node key.
        /// </value>
        public double SourceNodeKey { get; set; }
        /// <summary>
        /// Gets or sets the sink node key.
        /// </summary>
        /// <value>
        /// The sink node key.
        /// </value>
        public double SinkNodeKey { get; set; }
        /// <summary>
        /// Gets or sets the market date.
        /// </summary>
        /// <value>
        /// The market date.
        /// </value>
        public DateTime MarketDate { get; set; }
        /// <summary>
        /// Gets or sets the hours cleared.
        /// </summary>
        /// <value>
        /// The hours cleared.
        /// </value>
        public double HoursCleared { get; set; }

        /// <summary>
        /// Gets or sets the weekly win.
        /// </summary>
        /// <value>
        /// The weekly win.
        /// </value>
        public int WeeklyWin { get; set; }
        /// <summary>
        /// Gets or sets the standard dev.
        /// </summary>
        /// <value>
        /// The standard dev.
        /// </value>
        public double StdDev { get; set; }

        /// <summary>
        /// Gets or sets the constraint text.
        /// </summary>
        /// <value>
        /// The constraint text.
        /// </value>
        public string ConstraintText { get; set; }
        /// <summary>
        /// Gets or sets the contingency text.
        /// </summary>
        /// <value>
        /// The contingency text.
        /// </value>
        public string ContingencyText { get; set; }

        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public string family { get; set; }
        /// <summary>
        /// Gets or sets the sensitivity.
        /// </summary>
        /// <value>
        /// The sensitivity.
        /// </value>
        public float Sensitivity { get; set; }

        public double RTMedian { get; set; }
        public double DAMedian { get; set; }
        public double RTStdDev { get; set; }
        public double DAStdDev { get; set; }
        public double AvgRtLoss { get; set; }
        //avgRtCong
        public double AvgRtCong { get; set; }
        //


        public double CorrelationAsBid { get; set; }
        public double RTMedianAsBid { get; set; }
        public double DAMedianAsBid { get; set; }
        public double RTStdDevAsBid { get; set; }
        public double DAStdDevAsBid { get; set; }
        public double DARTAsBid { get; set; }
        public double DollarPerMWAsBid { get; set; }
    }
    public class RobotTypeListCorrelations
    {
        public string SourceName { get; set; }
        public string SinkName { get; set; }
        public string SourceZone { get; set; }
        public string SinkZone { get; set; }
        public string SourceNodeType { get; set; }
        public string SinkNodeType { get; set; }
        public int Hour { get; set; }
        public double Correlation { get; set; }
        public double RTMedian { get; set; }
        public double DAMedian { get; set; }
        public double RTStdDev { get; set; }
        public double DAStdDev { get; set; }
        public DateTime SavedTime { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class classAlgorithmList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="classAlgorithmList"/> class.
        /// </summary>
        /// <param name="algorithmList">The algorithm list.</param>
        public classAlgorithmList(string algorithmList)
        {
            this.AlgorithmList = algorithmList;
        }

        /// <summary>
        /// Gets or sets the algorithm list.
        /// </summary>
        /// <value>
        /// The algorithm list.
        /// </value>
        public string AlgorithmList { get; set; }
    }
}
