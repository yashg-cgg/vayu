using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Vayu.CommonAccessLibrary;

namespace Vayu.UptosPathAnalysisAlgorithm.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        private SqlConnection VayuConnection;
        private SqlCommand mSelectEESRobotR2Command;
        private SqlCommand mSelectEESRobotAnalysisC3Command;
        private SqlCommand mSelectEESRobotAnalysisC3B2Command;
        private SqlCommand mSelectEESRobotAnalysisC3B3Command;
        private SqlCommand mSelectErcotAnalysisC3Command;
        private SqlCommand mSelectErcotAnalysisC3B1Command;
        private SqlCommand mSelectEESRobotAnalysisJabbaCommand;
        private Dictionary<string, string> mNodeTypeHash = new Dictionary<string, string>();
        private Dictionary<string, string> mZoneHash = new Dictionary<string, string>();
        private SqlCommand mSelectNodeZoneTypeCommand;
        private SqlCommand mSelectVirtualBlockresultsCommand;
        private SqlCommand mSelectUptosShortAlgoCommand;
        private SqlCommand mSelectVirtualShortAlgoCommand;
        private SqlCommand mSelectOutageAlgoCommand;
        private SqlCommand mSelectCorrelationsAlgoCommand;
        private SqlCommand mSelectVirtualPathlistAlgoCommand;
        private SqlCommand mSelectErcotCorrelationCommand;
        private SqlCommand mSelectNegativeCorrelationCommand;
        private SqlCommand mSelectErcotNegativeCorrelationCommand;
        #endregion

        #region Public Methods

        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            //VirtualShortBlockAlgoResults
            mSelectVirtualShortAlgoCommand = VayuConnection.CreateCommand();
            mSelectVirtualShortAlgoCommand.CommandText = "select  n1.nodename as Source, n1.zone as SourceZone,   AnalysisType  , nt.Label  from robot.VirtualShortBlockAlgoResults a  " +
                                                         " join node n1 on a.NodeKey = n1.nodekey   join nodeType nt on n1.nodetypeKey  = nt.nodetypeKey " +
                                                         "where marketDate = @marketDate";
            mSelectVirtualShortAlgoCommand.Parameters.AddWithValue("@marketDate", "marketDate");

            //EESAnalysisRobotFilteredResults
            mSelectEESRobotR2Command = new SqlCommand();
#if NEW
            mSelectEESRobotR2Command.CommandText = " select  SourceName,Sinkname, AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , " +
            " PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber, SavedTime ,si.Zone,sint.Label  ,c.MustTakeSum,c.AMustTakeSum, " +
            " c.DailyMustTakeMin,c.ADailyMustTakeMin,c.YearlySumValue,c.AAvg,c.AMin,c.AMax  , 0 as AStdDev,c.YearlyCountDaysWin,c.YearlyCountDaysCleared,c.MaxLoss, " +
            " c.MaxWin,c.AvgValue,c.YearlyDownside,c.YearlyUpside,c.YearlyAvgValue  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW , " +
            " so.Zone,sont.Label   ,c.WeeklySumValue ,c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , " +
            " c.AnnualMaxLossRT, c.MonthlyMaxLossRT   From Robot.PeakOffPeakResults c with (nolock)   " +
            " inner join Node so with (nolock)  on c.SourceNodeKey = so.NodeKey and so.MarketKey = 1   inner join node si " +
            " with (nolock) on c.SinkNodeKey = si.NodeKey And si.MarketKey = 1   left join NodeType sint " +
            " with (nolock) on si.NodeTypeKey=sint.NodeTypeKey   left join NodeType sont with (nolock) on " +
            " so.NodeTypeKey=sont.NodeTypeKey  where CONVERT(date,c.SavedTime) = CONVERT(date,@date)";
            mSelectEESRobotR2Command.Parameters.AddWithValue("@date", "date");
#else
            mSelectEESRobotR2Command.CommandText = "select  SourceName,Sinkname, AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared, PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber, SavedTime ,b.Zone,nt.Label" +
                                                   " From Robot.EESAnalysisRobotFilteredResults inner join Node on SourceName = Node.NodeName and Node.MarketKey = 1 " +
                                                   "inner join node b on SinkName=b.NodeName inner join NodeType nt on b.NodeTypeKey=nt.NodeTypeKey where RiskReward=1000 and PctWin=1";
            //mSelectEESRobotR2Command.Parameters.AddWithValue("@RiskReward", "RiskReward");
#endif
            mSelectEESRobotR2Command.Connection = VayuConnection;

            //EESAnalysisRobotFilteredResults3HR
            mSelectEESRobotAnalysisC3Command = new SqlCommand();
            #region OLD
            //           mSelectEESRobotAnalysisC3Command.CommandText = "select top 1000  c.SourceName,c.SinkName, wint.RTMax,wint.RTMin, spr.RTMax,spr.RTMin, summ.RTMax,summ.RTMin, fall.RTMax,fall.RTMin, rt.RTMax,rt.RTMin, " +
            //"AnalysisType,Price, SumValue, MaxWin, MaxLoss, " +
            //"MW, RiskReward,CountDays, CountCleared  , PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  " +
            //"SavedTime ,si.Zone,sint.Label  ,c.MustTakeSum,c.AMustTakeSum,c.DailyMustTakeMin,c.ADailyMustTakeMin, " +
            //"c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin,c.DailyMax,c.DailyAvg,c.ADailyMin, " +
            //"c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW ,so.Zone,sont.Label ,c.WeeklySumValue , " +
            //"c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared ,c.AnnualMaxLossRT,c.MonthlyMaxLossRT   " +
            //"From BlockAlgorithmResults c with (nolock)   inner join Node so with (nolock) on c.SourceNodeKey = so.NodeKey  " +
            //"and so.MarketKey = 1   inner join node si with (nolock) on c.SinkNodeKey = si.NodeKey And si.MarketKey = 1   " +
            //"left join BlockAlgoAll RT on RT.SourceNodeKey=c.SourceNodeKey and rt.SinkNodeKey=c.SinkNodeKey left join  " +
            //"NodeType sint with (nolock) on si.NodeTypeKey=sint.NodeTypeKey  left join NodeType sont with (nolock) on  " +
            //"so.NodeTypeKey=sont.NodeTypeKey left join BlockAlgoFall fall with (nolock) on fall.SourceNodeKey=c.SourceNodeKey and fall.SinkNodeKey=c.SinkNodeKey " +
            //"left join NodeType sinf with(nolock) on sinf.NodeTypeKey=si.NodeTypeKey left join NodeType sonf with (nolock) on sonf.NodeTypeKey=so.NodeTypeKey  " +
            //"left join BlockAlgoSpring spr with (nolock) on spr.SourceNodeKey=c.SourceNodeKey and spr.SinkNodeKey=c.SinkNodeKey " +
            //"left join NodeType sins with(nolock) on sins.NodeTypeKey=si.NodeTypeKey left join NodeType sons with (nolock) on sons.NodeTypeKey=so.NodeTypeKey  " +
            //"left join BlockAlgoWinter wint with (nolock) on wint.SourceNodeKey=c.SourceNodeKey and wint.SinkNodeKey=c.SinkNodeKey " +
            //"left join NodeType sinw with(nolock) on sinw.NodeTypeKey=si.NodeTypeKey left join NodeType sonw with (nolock) on sonw.NodeTypeKey=so.NodeTypeKey  " +
            //"left join BlockAlgoSummer summ with (nolock) on summ.SourceNodeKey=c.SourceNodeKey and summ.SinkNodeKey=c.SinkNodeKey " +
            //"left join NodeType sinsu with(nolock) on sinsu.NodeTypeKey=si.NodeTypeKey left join NodeType sonsu with (nolock) on sonsu.NodeTypeKey=so.NodeTypeKey   where c.marketdate = CONVERT(date,@date)";
            #endregion OLD
            mSelectEESRobotAnalysisC3Command.CommandText = "select  c.SourceName,c.SinkName, wint.RTMax,wint.RTMin, spr.RTMax,spr.RTMin, summ.RTMax,summ.RTMin, fall.RTMax,fall.RTMin, rt.RTMax,rt.RTMin, " +
            "AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,   " +
            "SavedTime ,si.Zone,sint.Label  ,c.MustTakeSum,c.AMustTakeSum,c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,  " +
            "c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin,c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,  " +
            "c.Kurtosis,c.DollarPerMW ,so.Zone,sont.Label ,c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared ,  " +
            "c.AnnualMaxLossRT,c.MonthlyMaxLossRT   From BlockAlgorithmResults c with (nolock)    " +
            "inner join Node so with (nolock) on c.SourceNodeKey = so.NodeKey  and so.MarketKey = 1   " +
            "inner join node si with (nolock) on c.SinkNodeKey = si.NodeKey And si.MarketKey = 1     " +
            "left join BlockAlgoAll RT on RT.SourceNodeKey=c.SourceNodeKey and rt.SinkNodeKey=c.SinkNodeKey   " +
            "left join  NodeType sint with (nolock) on si.NodeTypeKey=sint.NodeTypeKey    " +
            "left join NodeType sont with (nolock) on  so.NodeTypeKey=sont.NodeTypeKey  " +
            "left join BlockAlgoFallUptos fall with (nolock) on fall.SourceNodeKey=c.SourceNodeKey and fall.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sinf with(nolock) on sinf.NodeTypeKey=si.NodeTypeKey  " +
            "left join NodeType sonf with (nolock) on sonf.NodeTypeKey=so.NodeTypeKey   " +
            "left join BlockAlgoSpringUptos spr with (nolock) on spr.SourceNodeKey=c.SourceNodeKey and spr.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sins with(nolock) on sins.NodeTypeKey=si.NodeTypeKey   " +
            "left join NodeType sons with (nolock) on sons.NodeTypeKey=so.NodeTypeKey   " +
            "left join BlockAlgoWinterUptos wint with (nolock) on wint.SourceNodeKey=c.SourceNodeKey and wint.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sinw with(nolock) on sinw.NodeTypeKey=si.NodeTypeKey  " +
            "left join NodeType sonw with (nolock) on sonw.NodeTypeKey=so.NodeTypeKey   " +
            "left join BlockAlgoSummerUptos summ with (nolock) on summ.SourceNodeKey=c.SourceNodeKey and summ.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sinsu with(nolock) on sinsu.NodeTypeKey=si.NodeTypeKey   " +
            "left join NodeType sonsu with (nolock) on sonsu.NodeTypeKey=so.NodeTypeKey   where c.marketdate = CONVERT(date,@date)";
            mSelectEESRobotAnalysisC3Command.Parameters.AddWithValue("@date", "date");
            //mSelectEESRobotAnalysisC3Command.Parameters.AddWithValue("@RiskReward", "RiskReward");
            mSelectEESRobotAnalysisC3Command.Connection = VayuConnection;
            //
            mSelectEESRobotAnalysisC3B2Command = new SqlCommand();
            mSelectEESRobotAnalysisC3B2Command.CommandText = "select c.SourceName,c.SinkName, wint.RTMax,wint.RTMin, spr.RTMax,spr.RTMin, summ.RTMax,summ.RTMin, fall.RTMax,fall.RTMin, rt.RTMax,rt.RTMin, " +
            "AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,   " +
            "SavedTime ,si.Zone,sint.Label  ,c.MustTakeSum,c.AMustTakeSum,c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,  " +
            "c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin,c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,  " +
            "c.Kurtosis,c.DollarPerMW ,so.Zone,sont.Label ,c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared ,  " +
            "c.AnnualMaxLossRT,c.MonthlyMaxLossRT   From BlockAlgorithmResults_old c with (nolock)    " +
            "inner join Node so with (nolock) on c.SourceNodeKey = so.NodeKey  and so.MarketKey = 1   " +
            "inner join node si with (nolock) on c.SinkNodeKey = si.NodeKey And si.MarketKey = 1     " +
            "left join BlockAlgoAll RT on RT.SourceNodeKey=c.SourceNodeKey and rt.SinkNodeKey=c.SinkNodeKey   " +
            "left join  NodeType sint with (nolock) on si.NodeTypeKey=sint.NodeTypeKey    " +
            "left join NodeType sont with (nolock) on  so.NodeTypeKey=sont.NodeTypeKey  " +
            "left join BlockAlgoFallUptos fall with (nolock) on fall.SourceNodeKey=c.SourceNodeKey and fall.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sinf with(nolock) on sinf.NodeTypeKey=si.NodeTypeKey  " +
            "left join NodeType sonf with (nolock) on sonf.NodeTypeKey=so.NodeTypeKey   " +
            "left join BlockAlgoSpringUptos spr with (nolock) on spr.SourceNodeKey=c.SourceNodeKey and spr.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sins with(nolock) on sins.NodeTypeKey=si.NodeTypeKey   " +
            "left join NodeType sons with (nolock) on sons.NodeTypeKey=so.NodeTypeKey   " +
            "left join BlockAlgoWinterUptos wint with (nolock) on wint.SourceNodeKey=c.SourceNodeKey and wint.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sinw with(nolock) on sinw.NodeTypeKey=si.NodeTypeKey  " +
            "left join NodeType sonw with (nolock) on sonw.NodeTypeKey=so.NodeTypeKey   " +
            "left join BlockAlgoSummerUptos summ with (nolock) on summ.SourceNodeKey=c.SourceNodeKey and summ.SinkNodeKey=c.SinkNodeKey   " +
            "left join NodeType sinsu with(nolock) on sinsu.NodeTypeKey=si.NodeTypeKey   " +
            "left join NodeType sonsu with (nolock) on sonsu.NodeTypeKey=so.NodeTypeKey   where c.marketdate = CONVERT(date,@date)";
            mSelectEESRobotAnalysisC3B2Command.Parameters.AddWithValue("@date", "date");
            //mSelectEESRobotAnalysisC3B2Command.Parameters.AddWithValue("@RiskReward", "RiskReward");
            mSelectEESRobotAnalysisC3B2Command.Connection = VayuConnection;

            //
            mSelectEESRobotAnalysisC3B3Command = new SqlCommand();
            mSelectEESRobotAnalysisC3B3Command.CommandText = "select c.SourceName,c.SinkName, wint.RTMax,wint.RTMin, spr.RTMax,spr.RTMin, summ.RTMax,summ.RTMin, fall.RTMax,fall.RTMin, rt.RTMax,rt.RTMin, " +
                                       " AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  " +
                                       " SavedTime ,si.Zone,sint.Label  ,c.MustTakeSum,c.AMustTakeSum,c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,  " +
                                       " c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin,c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,  " +
                                       " c.Kurtosis,c.DollarPerMW ,so.Zone,sont.Label ,c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , " +
                                       " c.AnnualMaxLossRT,c.MonthlyMaxLossRT   From BlockAlgorithmResults_b3 c with (nolock)    " +
                                       " inner join Node so with (nolock) on c.SourceNodeKey = so.NodeKey  and so.MarketKey = 1  " +
                                       " inner join node si with (nolock) on c.SinkNodeKey = si.NodeKey And si.MarketKey = 1    " +
                                       " left join BlockAlgoAll RT on RT.SourceNodeKey=c.SourceNodeKey and rt.SinkNodeKey=c.SinkNodeKey   " +
                                       " left join  NodeType sint with (nolock) on si.NodeTypeKey=sint.NodeTypeKey    " +
                                       " left join NodeType sont with (nolock) on  so.NodeTypeKey=sont.NodeTypeKey " +
                                       " left join BlockAlgoFallUptos fall with (nolock) on fall.SourceNodeKey=c.SourceNodeKey and fall.SinkNodeKey=c.SinkNodeKey " +
                                       " left join NodeType sinf with(nolock) on sinf.NodeTypeKey=si.NodeTypeKey  " +
                                       " left join NodeType sonf with (nolock) on sonf.NodeTypeKey=so.NodeTypeKey   " +
                                       " left join BlockAlgoSpringUptos spr with (nolock) on spr.SourceNodeKey=c.SourceNodeKey and spr.SinkNodeKey=c.SinkNodeKey  " +
                                       " left join NodeType sins with(nolock) on sins.NodeTypeKey=si.NodeTypeKey  " +
                                       " left join NodeType sons with (nolock) on sons.NodeTypeKey=so.NodeTypeKey   " +
                                       " left join BlockAlgoWinterUptos wint with (nolock) on wint.SourceNodeKey=c.SourceNodeKey and wint.SinkNodeKey=c.SinkNodeKey   " +
                                       " left join NodeType sinw with(nolock) on sinw.NodeTypeKey=si.NodeTypeKey  " +
                                       " left join NodeType sonw with (nolock) on sonw.NodeTypeKey=so.NodeTypeKey   " +
                                       " left join BlockAlgoSummerUptos summ with (nolock) on summ.SourceNodeKey=c.SourceNodeKey and summ.SinkNodeKey=c.SinkNodeKey  " +
                                       " left join NodeType sinsu with(nolock) on sinsu.NodeTypeKey=si.NodeTypeKey  " +
                                       " left join NodeType sonsu with (nolock) on sonsu.NodeTypeKey=so.NodeTypeKey   where c.marketdate = CONVERT(date,@date)";
            mSelectEESRobotAnalysisC3B3Command.Parameters.AddWithValue("@date", "date");
            mSelectEESRobotAnalysisC3B3Command.Connection = VayuConnection;
            //
            mSelectErcotAnalysisC3Command = new SqlCommand();
            mSelectErcotAnalysisC3Command.CommandText = "select c.SourceName,c.SinkName,  AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , " +
                       " PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  SavedTime ,c.MustTakeSum,c.AMustTakeSum,  " +
                       " c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct, " +
                       " c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.DollarPerMW , " +
                       " c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , " +
                       " c.AnnualMaxLossRT,c.MonthlyMaxLossRT,MAX(rg.maxlmp)MAXLMP,MIN(rg.minlmp)MINLMP ,N1.Zone as sourceZone,N2.Zone as sinkZone, " +
                       " Fall.MinLMP as RTFallMin,Fall.MaxLMP as RTFallMax,Spr.MinLMP as RTSpringMin,Spr.MaxLMP as RTSpringMax , " +
                       " Summ.MinLMP as RTSummerMin,Summ.MaxLMP as RTSummerMax,Win.MinLMP as RTWinterMin,Win.MaxLMP as RTWinterMax,utc.PathMinRT,srFuel.FuelSource sourcefuel,siFuel.FuelSource sinkfuel from  BlockAlgoResults c  " +
                       " join  UTCPathHistoricData AS utc ON c.SourceNodeKey=utc. SourceNodeKey and c.SinkNodeKey=utc.SinkNodeKey " +
                         "  join  Node AS N1 on c.SourceName=N1.NodeName " +
                       "  join  Node AS N2 on c.SinkName=N2.NodeName  " +
                       " join   ErcotNodeFuelSource srFuel on c.SourceNodeKey=srFuel.NodeKey " +
                       " join   ErcotNodeFuelSource siFuel on c.SinkNodeKey=siFuel.NodeKey  " +
                       " left join  RTRange rg on c.SourceName = rg.SourceName AND c.SinkName= rg.SinkName  " +
                        " left join  RTRangeFall AS Fall on Fall.SourceName=c.SourceName and Fall.SinkName=c.SinkName " +
                       " left join  RTRangeSpring As Spr on Spr.SourceName=c.SourceName and Spr.SinkName=c.SinkName " +
                       " left join  RTRangeSummer AS Summ on Summ.SourceName=c.SourceName and Summ.SinkName=c.SinkName " +
                       "  left join  RTRangeWinter AS Win on Win.SourceName=c.SourceName and Win.SinkName=c.SinkName " +
                       " where c.marketdate =@date  " +
                       " group by c.SourceName,c.SinkName,  AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , " +
                       " PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  SavedTime ,c.MustTakeSum,c.AMustTakeSum,  " +
                       " c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct, " +
                       " c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.DollarPerMW ,  " +
                       " c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared ,  " +
                       " c.AnnualMaxLossRT,c.MonthlyMaxLossRT,N1.Zone,N2.Zone,Fall.MinLMP ,Fall.MaxLMP,Spr.MinLMP,Spr.MaxLMP,Summ.MinLMP,Summ.MaxLMP,Win.MinLMP,Win.MaxLMP,utc.PathMinRT,srFuel.FuelSource,siFuel.FuelSource";
            mSelectErcotAnalysisC3Command.Parameters.AddWithValue("@date", "date");
            mSelectErcotAnalysisC3Command.Connection = VayuConnection;
            mSelectErcotAnalysisC3Command.CommandTimeout = 60000;
            //

            mSelectErcotAnalysisC3B1Command = new SqlCommand();
            mSelectErcotAnalysisC3B1Command.CommandText = "select c.SourceName,c.SinkName,  AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , " +
                       " PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  SavedTime ,c.MustTakeSum,c.AMustTakeSum,  " +
                       " c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin, " +
                       " c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW , " +
                       " c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , " +
                       " c.AnnualMaxLossRT,c.MonthlyMaxLossRT,MAX(rg.maxlmp)MAXLMP,MIN(rg.minlmp)MINLMP ,N1.Zone as sourceZone,N2.Zone as sinkZone, " +
                       " Fall.MinLMP as RTFallMin,Fall.MaxLMP as RTFallMax,Spr.MinLMP as RTSpringMin,Spr.MaxLMP as RTSpringMax , " +
                       " Summ.MinLMP as RTSummerMin,Summ.MaxLMP as RTSummerMax,Win.MinLMP as RTWinterMin,Win.MaxLMP as RTWinterMax,utc.PathMinRT,srFuel.FuelSource sourcefuel,siFuel.FuelSource sinkfuel from  BlockAlgorithmResults c  " +
                       " join  UTCPathHistoricData AS utc ON c.SourceNodeKey=utc. SourceNodeKey and c.SinkNodeKey=utc.SinkNodeKey " +
                         "  join  Node AS N1 on c.SourceName=N1.NodeName " +
                       "  join  Node AS N2 on c.SinkName=N2.NodeName  " +
                       " left join   ErcotNodeFuelSource srFuel on c.SourceNodeKey=srFuel.NodeKey " +
                       " left join   ErcotNodeFuelSource siFuel on c.SinkNodeKey=siFuel.NodeKey  " +
                       " left join  RTRange rg on c.SourceName = rg.SourceName AND c.SinkName= rg.SinkName  " +
                        " left join  RTRangeFall AS Fall on Fall.SourceName=c.SourceName and Fall.SinkName=c.SinkName " +
                       " left join  RTRangeSpring As Spr on Spr.SourceName=c.SourceName and Spr.SinkName=c.SinkName " +
                       " left join  RTRangeSummer AS Summ on Summ.SourceName=c.SourceName and Summ.SinkName=c.SinkName " +
                       "  left join  RTRangeWinter AS Win on Win.SourceName=c.SourceName and Win.SinkName=c.SinkName " +
                       " where c.marketdate =@date " +
                       " group by c.SourceName,c.SinkName,  AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , " +
                       " PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  SavedTime ,c.MustTakeSum,c.AMustTakeSum,  " +
                       " c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin, " +
                       " c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW ,  " +
                       " c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared ,  " +
                       " c.AnnualMaxLossRT,c.MonthlyMaxLossRT,N1.Zone,N2.Zone,Fall.MinLMP ,Fall.MaxLMP,Spr.MinLMP,Spr.MaxLMP,Summ.MinLMP,Summ.MaxLMP,Win.MinLMP,Win.MaxLMP,utc.PathMinRT,srFuel.FuelSource,siFuel.FuelSource";
            mSelectErcotAnalysisC3B1Command.Parameters.AddWithValue("@date", "date");
            mSelectErcotAnalysisC3B1Command.Connection = VayuConnection;
            mSelectErcotAnalysisC3B1Command.CommandTimeout = 60000;
            //
            mSelectErcotCorrelationCommand = new SqlCommand();
            mSelectErcotCorrelationCommand.CommandText = " select distinct c.SourceName ,c.SinkName, source.zone as SourceZone, sink.zone SinkZone,'','',  Hour,Correlation,RTMedian,DAMedian,RTStdDev,DAStdDev,SavedTime," +
                                                         " price,  wint.MaxLMP as WintMaxLmp,wint.MinLMP as WintMinLmp, spr.MaxLMP as SprMaxLmp,spr.MinLMP as SprMinLmp, summ.MaxLMP as SummMaxLmp, " +
                                                         " summ.MinLMP as SummMinLmp, fall.MaxLMP as FallMaxLmp,fall.MinLMP as FallMinLmp,dart,DollarPerMW , AvgRtLoss , AvgRtCong , CorrelationAsBid , " +
                                                         " RTMedianAsBid , DAMedianAsBid ,  RTStdDevAsBid , DAStdDevAsBid , DARTAsBid , DollarPerMWAsBid,c.CountDays,c.CountCleared,c.PctWin,utc.PathMinRT, srcf.FuelSource as SourceFuel,sinkf.FuelSource as SinkFuel " +
                                                         " from  HighestCorrelationsAlgo as c  " +
                                                         " join  UTCPathHistoricData AS utc ON c.SourceNodeKey=utc. SourceNodeKey and c.SinkNodeKey=utc.SinkNodeKey " +
                                                         " join  RTRangeFall fall with (nolock) on fall.SourceName=c.SourceName and fall.SinkName=c.SinkName    " +
                                                         " join  RTRangeSpring spr with (nolock) on spr.SourceName=c.SourceName and spr.SinkName=c.SinkName   " +
                                                         " join  RTRangeWinter wint with (nolock) on wint.SourceName=c.SourceName and wint.SinkName=c.SinkName  " +
                                                         " join  RTRangeSummer summ with (nolock) on summ.SourceName=c.SourceName and summ.SinkName=c.SinkName   " +
                                                         " join  Node source on c.SourceName = source.NodeName   " +
                                                         " join  node sink on c.SinkName = sink.Nodename " +
                                                         " left join   ErcotNodeFuelSource srcf on c.SourceNodeKey=srcf.NodeKey " +
                                                         " left join   ErcotNodeFuelSource sinkf on c.SinkNodeKey=sinkf.NodeKey " +
                                                         " where  SavedTime=@date and DollarPerMW>0";
            mSelectErcotCorrelationCommand.Parameters.AddWithValue("@date", "date");
            mSelectErcotCorrelationCommand.Connection = VayuConnection;
            //PJM Negative Corelation Algo
            mSelectNegativeCorrelationCommand = new SqlCommand();
            mSelectNegativeCorrelationCommand.CommandText = "select NCA.[SourceName] ,NCA.[SinkName],N1.Zone As SourceZone,N2.Zone As SinkZone,'','' ,NCA.[Hour] " + //--,NT1.Label AS SourceNodeType,NT2.Label AS SinkNodeType 
                                                           " ,NCA.[Correlation],NCA.[RTMedian],NCA.[DAMedian],NCA.[RTStdDev],NCA.[DAStdDev],NCA.[SavedTime],NCA.[Price], " +
                                                           " wint.RTMax,wint.RTMin, spr.RTMax,spr.RTMin, summ.RTMax,summ.RTMin, fall.RTMax,fall.RTMin,   rt.RTMax,rt.RTMin, " +
                                                           " NCA.[DART],NCA.[DollarPerMW],NCA.[AvgRtLoss],NCA.[AvgRtCong] " +
                                                           " ,NCA.[CorrelationAsBid],NCA.[RTMedianAsBid],NCA.[DAMedianAsBid],NCA.[RTStdDevAsBid],NCA.[DAStdDevAsBid],NCA.[DARTAsBid] " +
                                                           " ,NCA.[DollarPerMWAsBid] from pjm.[NegativeCorrelationsAlgo] as NCA " +
                                                           " left join BlockAlgoAll RT on RT.SourceNodeKey=NCA.SourceNodeKey and rt.SinkNodeKey=NCA.SinkNodeKey  left join BlockAlgoFallUptos fall with (nolock) on fall.SourceNodeKey=NCA.SourceNodeKey and fall.SinkNodeKey=NCA.SinkNodeKey   left join BlockAlgoSpringUptos spr with (nolock) on spr.SourceNodeKey=NCA.SourceNodeKey and spr.SinkNodeKey=NCA.SinkNodeKey  " +
                                                           " left join BlockAlgoWinterUptos wint with (nolock) on wint.SourceNodeKey=NCA.SourceNodeKey and wint.SinkNodeKey=NCA.SinkNodeKey   left join BlockAlgoSummerUptos summ with (nolock) on summ.SourceNodeKey=NCA.SourceNodeKey and summ.SinkNodeKey=NCA.SinkNodeKey " +
                                                           " INNER JOIN Node As N1 ON N1.NodeName=NCA.SourceName " +
                                                           " INNER JOIN Node AS N2 ON N2.NodeName=NCA.SinkName " +
                                                           " INNER JOIN NodeType AS NT1 ON NT1.NodeTypeKey=N1.NodeTypeKey " +
                                                           " INNER JOIN NodeType AS NT2 ON NT2.NodeTypeKey=N2.NodeTypeKey " +
                                                           " where SavedTime=@date";
            mSelectNegativeCorrelationCommand.Parameters.AddWithValue("@date", "date");
            mSelectNegativeCorrelationCommand.Connection = VayuConnection;
            //Ercot Negative Corelation
            mSelectErcotNegativeCorrelationCommand = new SqlCommand();
            mSelectErcotNegativeCorrelationCommand.CommandText = "  select c.SourceName ,c.SinkName, source.zone as SourceZone, sink.zone SinkZone,'','',  Hour,Correlation,RTMedian,DAMedian,RTStdDev,DAStdDev,SavedTime, " +
                                                                  " price,   wint.MaxLMP as WintMaxLmp,wint.MinLMP as WintMinLmp, spr.MaxLMP as SprMaxLmp,spr.MinLMP as SprMinLmp, summ.MaxLMP as SummMaxLmp," +
                                                                  " summ.MinLMP as SummMinLmp, fall.MaxLMP as FallMaxLmp,fall.MinLMP as FallMinLmp,   dart,DollarPerMW , AvgRtLoss , AvgRtCong , CorrelationAsBid , " +
                                                                  " RTMedianAsBid , DAMedianAsBid ,   RTStdDevAsBid , DAStdDevAsBid , DARTAsBid , DollarPerMWAsBid,c.CountDays,c.CountCleared,c.PctWin,utc.PathMinRT,srcf.FuelSource as SourceFuel,sinkf.FuelSource as SinkFuel " +
                                                                  " from Ercot..NegativeCorrelationsAlgo as c   " +
                                                                  " join Ercot..UTCPathHistoricData AS utc ON c.SourceNodeKey=utc. SourceNodeKey and c.SinkNodeKey=utc.SinkNodeKey " +
                                                                  " join Ercot..RTRangeFall fall with (nolock) on fall.SourceName=c.SourceName and fall.SinkName=c.SinkName   " +
                                                                  " join Ercot..RTRangeSpring spr with (nolock) on spr.SourceName=c.SourceName and spr.SinkName=c.SinkName   " +
                                                                  " join Ercot..RTRangeWinter wint with (nolock) on wint.SourceName=c.SourceName and wint.SinkName=c.SinkName  " +
                                                                  " join Ercot..RTRangeSummer summ with (nolock) on summ.SourceName=c.SourceName and summ.SinkName=c.SinkName   " +
                                                                  " join Ercot..Node source on c.SourceName = source.NodeName   " +
                                                                  " join Ercot..node sink on c.SinkName = sink.Nodename  " +
                                                                  " left join ERCOT.. ErcotNodeFuelSource srcf on c.SourceNodeKey=srcf.NodeKey " +
                                                                  " left join ERCOT.. ErcotNodeFuelSource sinkf on c.SinkNodeKey=sinkf.NodeKey " +
                                                                  " Where SavedTime=@date and DollarPerMW>0";

            mSelectErcotNegativeCorrelationCommand.Parameters.AddWithValue("@date", "date");
            mSelectErcotNegativeCorrelationCommand.Connection = VayuConnection;

            #region OLD
            //mSelectErcotAnalysisC3Command.CommandText ="select  c.SourceName,c.SinkName,  AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , "+
            //" PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  SavedTime ,c.MustTakeSum,c.AMustTakeSum, "+
            //" c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin, "+
            //" c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW , "+
            //" c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , "+
            //" c.AnnualMaxLossRT,c.MonthlyMaxLossRT,MAX(rg.maxlmp)MAXLMP,MIN(rg.minlmp)MINLMP ,N1.Zone as sourceZone,N2.Zone as sinkZone from ERCOT..BlockAlgorithmResults c " +
            //" join ERCOT..RTRange rg on c.SourceName = rg.SourceName AND c.SinkName= rg.SinkName " +
            //" join NewTrading..Node AS N1 on c.SourceName=N1.NodeName "+
            //" join NewTrading..Node AS N2 on c.SinkName=N2.NodeName "+
            //" where c.marketdate =@date " +
            //" group by c.SourceName,c.SinkName,  AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  ,  "+
            //" PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  SavedTime ,c.MustTakeSum,c.AMustTakeSum, "+
            //" c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin, "+
            //" c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW , "+
            //" c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , "+
            //" c.AnnualMaxLossRT,c.MonthlyMaxLossRT,N1.Zone,N2.Zone";

            //OLD
            //mSelectErcotAnalysisC3Command.CommandText = "select c.SourceName,c.SinkName,  AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , "+
            //" PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  SavedTime ,c.MustTakeSum,c.AMustTakeSum, "+
            //" c.DailyMustTakeMin,c.ADailyMustTakeMin, c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin, "+
            //" c.DailyMax,c.DailyAvg,c.ADailyMin, c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW , "+
            //" c.WeeklySumValue , c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , "+
            //" c.AnnualMaxLossRT,c.MonthlyMaxLossRT,N1.Zone AS SourceZone,N2.Zone AS SinkZone From ercot..BlockAlgorithmResults c with (nolock) "+
            //" INNER JOIN ercot..Node AS N1 ON c.SourceNodeKey=N1.NodeKey "+
            //" INNER JOIN ercot..Node AS N2 ON c.SinkNodeKey=N2.NodeKey" + 
            //" where c.marketdate = CONVERT(date,@date)";
            #endregion OLD

            //
            mSelectVirtualBlockresultsCommand = new SqlCommand();
            mSelectVirtualBlockresultsCommand.CommandText = " select   c.NodeName, AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared "
                                                          + ", PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber, SavedTime"
                                                          + " ,c.MustTakeSum,c.AMustTakeSum,c.DailyMustTakeMin,c.ADailyMustTakeMin,c.ASum,c.AAvg,c.AMin,c.AMax"
                                                          + " ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin,c.DailyMax,c.DailyAvg,c.ADailyMin,c.ADailyMax,c.ADailyAvg "
                                                          + ",c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW ,so.Zone,sont.Label"
                                                          + " ,c.WeeklySumValue ,c.WeeklyMaxWin ,c.WeeklyMaxLoss ,c.WeeklyPctWin ,c.WeeklyCountCleared ,c.AnnualMaxLoss , IncDec"
                                                          + " From robot.VirtualBlockAlgorithmResults c with (nolock) inner join Node so with (nolock)  on c.NodeKey = so.NodeKey and so.MarketKey = 1 "
                                                          + "left join NodeType sont with (nolock) on so.NodeTypeKey=sont.NodeTypeKey where CONVERT(date,c.marketdate) = CONVERT(date,@date)";
            mSelectVirtualBlockresultsCommand.Connection = VayuConnection;
            mSelectVirtualBlockresultsCommand.Parameters.AddWithValue("@date", "date");
            //

            //EESAnalysisRobotFilteredResultsJabba
            mSelectEESRobotAnalysisJabbaCommand = new SqlCommand();
#if NEW
            mSelectEESRobotAnalysisJabbaCommand.CommandText = " select   SourceName,Sinkname, AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared  , " +
            " PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber, SavedTime ,si.Zone,sint.Label  ,c.MustTakeSum,c.AMustTakeSum, " +
            " c.DailyMustTakeMin,c.ADailyMustTakeMin,c.YearlySumValue,c.AAvg,c.AMin,c.AMax  , 0 as AStdDev,c.YearlyCountDaysWin,c.YearlyCountDaysCleared,c.MaxLoss, " +
            " c.MaxWin,c.AvgValue,c.YearlyDownside,c.YearlyUpside,c.YearlyAvgValue  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW , " +
            " so.Zone,sont.Label   ,c.WeeklySumValue ,c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared , " +
            " c.AnnualMaxLossRT, c.MonthlyMaxLossRT  From Robot.RampResults c with (nolock)   " +
            " inner join Node so with (nolock)  on c.SourceNodeKey = so.NodeKey and so.MarketKey = 1   inner join node si " +
            " with (nolock) on c.SinkNodeKey = si.NodeKey And si.MarketKey = 1   left join NodeType sint " +
            " with (nolock) on si.NodeTypeKey=sint.NodeTypeKey   left join NodeType sont with (nolock) on " +
            " so.NodeTypeKey=sont.NodeTypeKey where CONVERT(date,c.SavedTime) = CONVERT(date,@date)";
            mSelectEESRobotAnalysisJabbaCommand.Parameters.AddWithValue("@date", "date");
#else
            mSelectEESRobotAnalysisJabbaCommand.CommandText = "select  SourceName,Sinkname, AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared, PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber, SavedTime ,b.Zone,nt.Label " +
                                                                " From Robot.EESAnalysisRobotFilteredResultsJabba inner join Node  on SourceName =Node.NodeName and Node.MarketKey=1 " +
                                                                "inner join node b on SinkName=b.NodeName And b.MarketKey = 1 inner join NodeType nt on b.NodeTypeKey=nt.NodeTypeKey where RiskReward=1000 and PctWin=1";
#endif
            //mSelectEESRobotAnalysisJabbaCommand.Parameters.AddWithValue("@RiskReward", "RiskReward");
            mSelectEESRobotAnalysisJabbaCommand.Connection = VayuConnection;
            mSelectNodeZoneTypeCommand = VayuConnection.CreateCommand();
            mSelectNodeZoneTypeCommand.CommandText = "select  n.NodeName, n.Zone, nt.Label from Node n join NodeType nt on n.NodeTypeKey = nt.NodeTypeKey where n.MarketKey = 9";//first here was 1 date dont know changed it on the 13-04-2026 for testing
            //
            mSelectUptosShortAlgoCommand = VayuConnection.CreateCommand();
            mSelectUptosShortAlgoCommand.Connection = VayuConnection;
            mSelectUptosShortAlgoCommand.CommandText = "select  sourceNodeKey , SinkNodeKey ,n1.nodename as Source,n2.nodename as Sink,n1.zone as SourceZone,n2.zone as SinkZone," +
                                                        "nt.label as SourceNodeType,nt.label as sinknodetype, AnalysisType " +
                                                        "from ShortBlockAlgorithmResults a " +
                                                        "join node n1 on a.sourceNodeKey = n1.nodekey " +
                                                        "join node n2 on a.sinknodekey = n2.nodekey " +
                                                        "join nodeType nt on n1.nodetypeKey  = nt.nodetypeKey " +
                                                        "join nodeType nt1 on n2.nodetypeKey  = nt1.nodetypeKey where marketDate = @marketDate";
            mSelectUptosShortAlgoCommand.Parameters.AddWithValue("@marketDate", "marketDate");
            //
            mSelectOutageAlgoCommand = VayuConnection.CreateCommand();
            mSelectOutageAlgoCommand.Connection = VayuConnection;
            //mSelectOutageAlgoCommand.CommandText = "select  TOP 100 br.*, n.Zone AS SourceZone, n1.Zone AS SinkZone, a.ConstraintText, a.ContingencyText, c.MonitoredText AS family, a.Sensitivity" // local
            mSelectOutageAlgoCommand.CommandText = "select  br.*, n.Zone AS SourceZone, n1.Zone AS SinkZone, a.ConstraintText, a.ContingencyText, c.MonitoredText AS family, a.Sensitivity" // live
                                                    + " , nt.Label AS SourceNodeType, nt1.Label AS SinkNodeType"
                                                    + " FROM OutageAlgorithm  a"
                                                    + " JOIN RTMasterConstraint b ON a.ConstraintText = b.MonitoredText AND a.ContingencyText = b.ContingencyText"
                                                    + " JOIN RTFamily c ON c.ConstraintRTNum = b.ConstraintRTNum"
                                                    + " LEFT JOIN BlockAlgorithmResults br ON a.SourceNodeKey = br.SourceNodeKey AND a.SinkNodeKey = br.SinkNodeKey AND a.MarketDate = br.marketdate"
                                                    + " JOIN node n ON a.SourceNodeKey = n.NodeKey"
                                                    + " JOIN node n1 ON a.SinkNodeKey = n1.NodeKey"
                                                    + " JOIN NodeType nt ON nt.NodeTypeKey = n.NodeTypeKey AND nt.MarketKey = 1"
                                                    + " JOIN NodeType nt1 ON nt1.NodeTypeKey = n.NodeTypeKey AND nt1.MarketKey = 1"
                                                    + " WHERE a.MarketDate = @marketDate"
                                                    + " ORDER BY a.SourceNodeKey";
            mSelectOutageAlgoCommand.Parameters.AddWithValue("@marketDate", "marketDate");

            mSelectCorrelationsAlgoCommand = VayuConnection.CreateCommand();
            mSelectCorrelationsAlgoCommand.Connection = VayuConnection;
            //mSelectCorrelationsAlgoCommand.CommandText = "select  rt.SourceName,rt.SinkName,SourceZone,SinkZone,SourceNodeType,SinkNodeType,Hour,Correlation,RTMedian,DAMedian,RTStdDev,DAStdDev,SavedTime, price,rt.RTMaxAll,rt.RTMinAll,dart,DollarPerMW from PJM.HighestCorrelationsAlgo c left join BlockAlgoMinMaxAll RT on RT.SourceName=c.SourceName and rt.SinkName=c.SinkName where SavedTime=@SavedTime";
            mSelectCorrelationsAlgoCommand.CommandText = "select   c.SourceName ,c.SinkName , source.zone as SourceZone, sink.zone SinkZone, ntsource.Label as SourceNodeType, ntsink.Label SinkNodeType ,Hour,Correlation,RTMedian,DAMedian,RTStdDev,DAStdDev,SavedTime,    price, wint.RTMax,wint.RTMin, spr.RTMax,spr.RTMin, summ.RTMax,summ.RTMin, fall.RTMax,fall.RTMin,   rt.RTMax,rt.RTMin,dart,DollarPerMW , AvgRtLoss , AvgRtCong , CorrelationAsBid , RTMedianAsBid , DAMedianAsBid , RTStdDevAsBid , DAStdDevAsBid , DARTAsBid , DollarPerMWAsBid  " +
                       " from PJM.HighestCorrelationsAlgo_test c left join BlockAlgoAll RT on RT.SourceNodeKey=c.SourceNodeKey and    rt.SinkNodeKey=c.SinkNodeKey  left join BlockAlgoFallUptos fall with (nolock) on fall.SourceNodeKey=c.SourceNodeKey and fall.SinkNodeKey=c.SinkNodeKey   left join BlockAlgoSpringUptos spr with (nolock) on spr.SourceNodeKey=c.SourceNodeKey and spr.SinkNodeKey=c.SinkNodeKey " +
                       " left join BlockAlgoWinterUptos wint with (nolock) on wint.SourceNodeKey=c.SourceNodeKey and wint.SinkNodeKey=c.SinkNodeKey   left join BlockAlgoSummerUptos summ with (nolock) on summ.SourceNodeKey=c.SourceNodeKey and summ.SinkNodeKey=c.SinkNodeKey join Node source on c.SourceName = source.NodeName join node sink on c.SinkName = sink.Nodename  " +
                       "  join NodeType ntsource on   source.NodeTypeKey =  ntsource.NodeTypeKey join   NodeType ntsink on   sink.NodeTypeKey =  ntsink.NodeTypeKey     where  SavedTime=@SavedTime ";
            mSelectCorrelationsAlgoCommand.Parameters.AddWithValue("@SavedTime", "SavedTime");


            //mSelectVirtualPathlistAlgoCommand = new SqlCommand();
            //mSelectVirtualPathlistAlgoCommand.CommandText = " select  top 10 c.SourceName,c.SinkName,AnalysisType,Price, SumValue, MaxWin, MaxLoss, MW, RiskReward,CountDays, CountCleared "
            //                                              + " , PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber, SavedTime"
            //                                               + " ,c.MustTakeSum,c.AMustTakeSum,c.DailyMustTakeMin,c.ADailyMustTakeMin,c.ASum,c.AAvg,c.AMin,c.AMax"
            //                                               + " ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin,c.DailyMax,c.DailyAvg,c.ADailyMin,c.ADailyMax,c.ADailyAvg "
            //                                              + " ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW ,so.Zone,sont.Label"
            //                                              + "  ,c.WeeklySumValue ,c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared ,c.AnnualMaxLossRT" 
            //                                              + "  From BlockAlgorithmResults_VP c with (nolock) inner join Node so with (nolock)  on so.MarketKey = 1 "
            //                                              + " left join NodeType sont with (nolock) on so.NodeTypeKey=sont.NodeTypeKey where CONVERT(date,c.marketdate) = CONVERT(date,@date)";
            //mSelectVirtualPathlistAlgoCommand.Connection = SigmaDbConn;
            //mSelectVirtualPathlistAlgoCommand.Parameters.AddWithValue("@date", "date");


            mSelectVirtualPathlistAlgoCommand = new SqlCommand();
            mSelectVirtualPathlistAlgoCommand.CommandText = "select c.SourceName,c.SinkName, wint.RTMax,wint.RTMin, spr.RTMax,spr.RTMin, summ.RTMax,summ.RTMin, fall.RTMax,fall.RTMin, rt.RTMax,rt.RTMin, " +
 "AnalysisType,Price, SumValue, MaxWin, MaxLoss, " +
 "MW, RiskReward,CountDays, CountCleared  , PctWin,YearlyDownside, YearlyRiskReward,AvgDA,CalcNumber,  " +
 "SavedTime ,si.Zone,sint.Label  ,c.MustTakeSum,c.AMustTakeSum,c.DailyMustTakeMin,c.ADailyMustTakeMin, " +
 "c.ASum,c.AAvg,c.AMin,c.AMax  ,c.AStdDev,c.AWinPct,c.AClearPct,c.DailyMin,c.DailyMax,c.DailyAvg,c.ADailyMin, " +
 "c.ADailyMax,c.ADailyAvg  ,c.SumToMax,c.Sharpe,c.Skew,c.Kurtosis,c.DollarPerMW ,so.Zone,sont.Label ,c.WeeklySumValue , " +
 "c.WeeklyMaxWin ,c.WeeklyMaxLossRT ,c.WeeklyPctWin ,c.WeeklyCountCleared ,c.AnnualMaxLossRT,c.MonthlyMaxLossRT   " +
 "From BlockAlgorithmResults_VP c with (nolock)   inner join Node so with (nolock) on c.SourceNodeKey = so.NodeKey  " +
 "and so.MarketKey = 1   inner join node si with (nolock) on c.SinkNodeKey = si.NodeKey And si.MarketKey = 1   " +
 "left join BlockAlgoAll RT on RT.SourceNodeKey=c.SourceNodeKey and rt.SinkNodeKey=c.SinkNodeKey left join  " +
 "NodeType sint with (nolock) on si.NodeTypeKey=sint.NodeTypeKey  left join NodeType sont with (nolock) on  " +
 "so.NodeTypeKey=sont.NodeTypeKey left join BlockAlgoFall fall with (nolock) on fall.SourceNodeKey=c.SourceNodeKey and fall.SinkNodeKey=c.SinkNodeKey " +
 "left join NodeType sinf with(nolock) on sinf.NodeTypeKey=si.NodeTypeKey left join NodeType sonf with (nolock) on sonf.NodeTypeKey=so.NodeTypeKey  " +
 "left join BlockAlgoSpring spr with (nolock) on spr.SourceNodeKey=c.SourceNodeKey and spr.SinkNodeKey=c.SinkNodeKey " +
 "left join NodeType sins with(nolock) on sins.NodeTypeKey=si.NodeTypeKey left join NodeType sons with (nolock) on sons.NodeTypeKey=so.NodeTypeKey  " +
 "left join BlockAlgoWinter wint with (nolock) on wint.SourceNodeKey=c.SourceNodeKey and wint.SinkNodeKey=c.SinkNodeKey " +
 "left join NodeType sinw with(nolock) on sinw.NodeTypeKey=si.NodeTypeKey left join NodeType sonw with (nolock) on sonw.NodeTypeKey=so.NodeTypeKey  " +
 "left join BlockAlgoSummer summ with (nolock) on summ.SourceNodeKey=c.SourceNodeKey and summ.SinkNodeKey=c.SinkNodeKey " +
 "left join NodeType sinsu with(nolock) on sinsu.NodeTypeKey=si.NodeTypeKey left join NodeType sonsu with (nolock) on sonsu.NodeTypeKey=so.NodeTypeKey   where c.marketdate = CONVERT(date,@date)";
            mSelectVirtualPathlistAlgoCommand.Connection = VayuConnection;
            mSelectVirtualPathlistAlgoCommand.Parameters.AddWithValue("@date", "date");
            mSelectVirtualPathlistAlgoCommand.CommandTimeout = 9000;



        }

        public DateTime? GetBlockMaxDate()
        {
            try
            {
                SqlCommand selectMaxDate = new SqlCommand();
                selectMaxDate.Connection = new VayuDBConnection().GetInstance().GetSqlConnection();
                selectMaxDate.CommandText = "select  max(marketdate) from BlockAlgorithmResults with (nolock)";
                selectMaxDate.Connection.Open();
                DateTime? date = selectMaxDate.ExecuteScalar() as DateTime?;
                return date;
            }
            catch { }
            return DateTime.Now;
        }

        public void GetEESRobotList(Action<List<RobotTypeList>, Exception> callback, string RobotType, DateTime? date = null)
        {
            if (mNodeTypeHash.Count == 0 || mZoneHash.Count == 0)
            {
                FillZoneNodeHashValues();
            }
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            List<RobotTypeList> RobotList = new List<RobotTypeList>();


            Dictionary<int, string> mNodeHash = new Dictionary<int, string>();
            SqlDataReader reader = null;
            try
            {
                if (RobotType == "ErcotBlock_B2")
                {
                    mSelectErcotAnalysisC3Command.Parameters["@date"].Value = date.HasValue ? date.Value.AddDays(-1) : DateTime.Now.Date.AddDays(-1);
                    reader = mSelectErcotAnalysisC3Command.ExecuteReader();
                }
                else if (RobotType == "ErcotBlock_B1")
                {
                    mSelectErcotAnalysisC3B1Command.Parameters["@date"].Value = date.HasValue ? date.Value.AddDays(0) : DateTime.Now.Date.AddDays(0);
                    reader = mSelectErcotAnalysisC3B1Command.ExecuteReader();
                }
#if NEW
#else
                //if (RobotType == "Block")
#endif
                //if (RobotType == "Block")
                {
                    Debug.WriteLine("Start: " + DateTime.Now.ToString());

                    while (reader.Read())
                    {

                        if (RobotType == "ErcotBlock_B1")
                        {
                            RobotTypeList mRobotTypeList = new RobotTypeList();
                            mRobotTypeList.SourceName = reader.GetValue(0).ToString();
                            mRobotTypeList.SinkName = reader.GetValue(1).ToString();
                            mRobotTypeList.AnalysisType = reader.GetValue(2).ToString();
                            mRobotTypeList.Price = GetDouble(reader[3]);
                            mRobotTypeList.SumValue = GetDouble(reader[4]);
                            mRobotTypeList.MaxWin = GetDouble(reader[5]);
                            mRobotTypeList.MaxLoss = GetDouble(reader[6]);
                            mRobotTypeList.MW = GetDouble(reader[7]);
                            mRobotTypeList.RiskReward = GetDouble(reader[8]);
                            mRobotTypeList.CountDays = GetDouble(reader[9]);
                            mRobotTypeList.CountCleared = GetDouble(reader[10]);
                            mRobotTypeList.PctWin = GetDouble(reader[11]);
                            mRobotTypeList.YearlyDownSide = GetDouble(reader[12]);
                            mRobotTypeList.YearlyRiskReward = GetDouble(reader[13]);
                            mRobotTypeList.AvgDA = GetDouble(reader[14]);
                            mRobotTypeList.CalcNumber = GetDouble(reader[15]);
                            mRobotTypeList.SavedTime = reader.GetValue(16) == DBNull.Value ? new DateTime() : reader.GetDateTime(16);
                            mRobotTypeList.MustTakeSum = GetDouble(reader[17]);
                            mRobotTypeList.AMustTakeSum = GetDouble(reader[18]);
                            mRobotTypeList.DailyMustTakeMin = GetDouble(reader[19]);//
                            mRobotTypeList.ADailyMustTakeMin = GetDouble(reader[20]);
                            mRobotTypeList.ASum = GetDouble(reader[21]);
                            mRobotTypeList.AAvg = GetDouble(reader[22]);
                            mRobotTypeList.AMin = GetDouble(reader[23]);
                            mRobotTypeList.AMax = GetDouble(reader[24]);
                            mRobotTypeList.AStdDev = GetDouble(reader[25]);
                            mRobotTypeList.AWinPct = GetDouble(reader[26]);
                            mRobotTypeList.AClearPct = GetDouble(reader[27]);
                            mRobotTypeList.DailyMin = GetDouble(reader[28]);
                            mRobotTypeList.DailyMax = GetDouble(reader[29]);
                            mRobotTypeList.DailyAvg = GetDouble(reader[30]);
                            mRobotTypeList.ADailyMin = GetDouble(reader[31]);
                            mRobotTypeList.ADailyMax = GetDouble(reader[32]);
                            mRobotTypeList.ADailyAvg = GetDouble(reader[33]);
                            mRobotTypeList.SumToMax = GetDouble(reader[34]);
                            mRobotTypeList.Sharpe = GetDouble(reader[35]);
                            mRobotTypeList.Skew = GetDouble(reader[36]);
                            mRobotTypeList.Kurtosis = GetDouble(reader[37]);
                            mRobotTypeList.DollarPerMW = GetDouble(reader[38]);
                            mRobotTypeList.WeeklySumValue = GetDouble(reader[39]);
                            mRobotTypeList.WeeklyMaxWin = GetDouble(reader[40]);
                            mRobotTypeList.WeeklyMaxLossRt = GetDouble(reader[41]);
                            mRobotTypeList.WeeklyPctWin = GetDouble(reader[42]);
                            mRobotTypeList.WeeklyCountCleared = GetInt(reader.GetValue(43));
                            mRobotTypeList.AnnualMaxLossRt = GetDouble(reader[44]);
                            mRobotTypeList.MonthlyMaxLossRt = GetDouble(reader[45]);
                            //
                            // mRobotTypeList.RTMaxAll = Convert.ToDecimal(reader[46]);
                            // mRobotTypeList.RTMinAll = Convert.ToDecimal(reader[47]);
                            mRobotTypeList.SourceZone = reader.GetValue(48).ToString();
                            mRobotTypeList.SinkZone = reader.GetValue(49).ToString();
                            // mRobotTypeList.SourceNodeType = reader.GetValue(48).ToString();
                            //mRobotTypeList.SinkNodeType = reader.GetValue(49).ToString();
                            //                            
                            mRobotTypeList.RTMinFall = reader.IsDBNull(50) ? 0 : Convert.ToDecimal(reader[50]);
                            mRobotTypeList.RTMaxFall = reader.IsDBNull(51) ? 0 : Convert.ToDecimal(reader[51]);
                            mRobotTypeList.RTMinSpring = reader.IsDBNull(52) ? 0 : Convert.ToDecimal(reader[52]);
                            mRobotTypeList.RTMaxSpring = reader.IsDBNull(53) ? 0 : Convert.ToDecimal(reader[53]);
                            mRobotTypeList.RTMinSummer = reader.IsDBNull(54) ? 0 : Convert.ToDecimal(reader[54]);
                            mRobotTypeList.RTMaxSummer = reader.IsDBNull(55) ? 0 : Convert.ToDecimal(reader[55]);
                            mRobotTypeList.RTMinWinter = reader.IsDBNull(56) ? 0 : Convert.ToDecimal(reader[56]);
                            mRobotTypeList.RTMaxWinter = reader.IsDBNull(57) ? 0 : Convert.ToDecimal(reader[57]);
                            //
                            decimal min1 = Math.Min(mRobotTypeList.RTMinWinter, mRobotTypeList.RTMinSpring);
                            decimal min2 = Math.Min(mRobotTypeList.RTMinSummer, mRobotTypeList.RTMinFall);
                            decimal max1 = Math.Max(mRobotTypeList.RTMaxWinter, mRobotTypeList.RTMaxSpring);
                            decimal max2 = Math.Max(mRobotTypeList.RTMaxSummer, mRobotTypeList.RTMaxFall);
                            mRobotTypeList.RTMaxAll = Math.Max(max1, max2);
                            mRobotTypeList.RTMinAll = Math.Min(min1, min2);
                            mRobotTypeList.PathMinRT = reader.GetDateTime(58);
                            int anaType = 0;
                            if (int.TryParse(mRobotTypeList.AnalysisType, out anaType))
                                mRobotTypeList.AnalysisTypeInt = anaType;
                            mRobotTypeList.SourceFuel = reader.GetValue(59).ToString();
                            mRobotTypeList.SinkFuel = reader.GetValue(60).ToString();
                            RobotList.Add(mRobotTypeList);
                        }
                        if (RobotType == "ErcotBlock_B2")
                        {
                            RobotTypeList mRobotTypeList = new RobotTypeList();
                            mRobotTypeList.SourceName = reader.GetValue(0).ToString();
                            mRobotTypeList.SinkName = reader.GetValue(1).ToString();
                            mRobotTypeList.AnalysisType = reader.GetValue(2).ToString();
                            mRobotTypeList.Price = GetDouble(reader[3]);
                            mRobotTypeList.SumValue = GetDouble(reader[4]);
                            mRobotTypeList.MaxWin = GetDouble(reader[5]);
                            mRobotTypeList.MaxLoss = GetDouble(reader[6]);
                            mRobotTypeList.MW = GetDouble(reader[7]);
                            mRobotTypeList.RiskReward = GetDouble(reader[8]);
                            mRobotTypeList.CountDays = GetDouble(reader[9]);
                            mRobotTypeList.CountCleared = GetDouble(reader[10]);
                            mRobotTypeList.PctWin = GetDouble(reader[11]);
                            mRobotTypeList.YearlyDownSide = GetDouble(reader[12]);
                            mRobotTypeList.YearlyRiskReward = GetDouble(reader[13]);
                            mRobotTypeList.AvgDA = GetDouble(reader[14]);
                            mRobotTypeList.CalcNumber = GetDouble(reader[15]);
                            mRobotTypeList.SavedTime = reader.GetValue(16) == DBNull.Value ? new DateTime() : reader.GetDateTime(16);
                            mRobotTypeList.MustTakeSum = GetDouble(reader[17]);
                            mRobotTypeList.AMustTakeSum = GetDouble(reader[18]);
                            mRobotTypeList.DailyMustTakeMin = GetDouble(reader[19]);//
                            mRobotTypeList.ADailyMustTakeMin = GetDouble(reader[20]);
                            mRobotTypeList.ASum = GetDouble(reader[21]);
                            mRobotTypeList.AAvg = GetDouble(reader[22]);
                            mRobotTypeList.AMin = GetDouble(reader[23]);
                            mRobotTypeList.AMax = GetDouble(reader[24]);
                            mRobotTypeList.AStdDev = GetDouble(reader[25]);
                            mRobotTypeList.AWinPct = GetDouble(reader[26]);
                            mRobotTypeList.AClearPct = GetDouble(reader[27]);

                            mRobotTypeList.DailyAvg = GetDouble(reader[28]);
                            mRobotTypeList.ADailyMin = GetDouble(reader[29]);
                            mRobotTypeList.ADailyMax = GetDouble(reader[30]);
                            mRobotTypeList.ADailyAvg = GetDouble(reader[31]);
                            mRobotTypeList.SumToMax = GetDouble(reader[32]);
                            mRobotTypeList.Sharpe = GetDouble(reader[33]);
                            mRobotTypeList.DollarPerMW = GetDouble(reader[34]);
                            mRobotTypeList.WeeklySumValue = GetDouble(reader[35]);
                            mRobotTypeList.WeeklyMaxWin = GetDouble(reader[36]);
                            mRobotTypeList.WeeklyMaxLossRt = GetDouble(reader[37]);
                            mRobotTypeList.WeeklyPctWin = GetDouble(reader[38]);
                            mRobotTypeList.WeeklyCountCleared = GetInt(reader.GetValue(39));
                            mRobotTypeList.AnnualMaxLossRt = GetDouble(reader[40]);
                            mRobotTypeList.MonthlyMaxLossRt = GetDouble(reader[41]);
                            //
                            // mRobotTypeList.RTMaxAll = Convert.ToDecimal(reader[46]);
                            // mRobotTypeList.RTMinAll = Convert.ToDecimal(reader[47]);
                            mRobotTypeList.SourceZone = reader.GetValue(44).ToString();
                            mRobotTypeList.SinkZone = reader.GetValue(45).ToString();
                            // mRobotTypeList.SourceNodeType = reader.GetValue(48).ToString();
                            //mRobotTypeList.SinkNodeType = reader.GetValue(49).ToString();
                            //                            
                            mRobotTypeList.RTMinFall = reader.IsDBNull(46) ? 0 : Convert.ToDecimal(reader[46]);
                            mRobotTypeList.RTMaxFall = reader.IsDBNull(47) ? 0 : Convert.ToDecimal(reader[47]);
                            mRobotTypeList.RTMinSpring = reader.IsDBNull(48) ? 0 : Convert.ToDecimal(reader[48]);
                            mRobotTypeList.RTMaxSpring = reader.IsDBNull(49) ? 0 : Convert.ToDecimal(reader[49]);
                            mRobotTypeList.RTMinSummer = reader.IsDBNull(50) ? 0 : Convert.ToDecimal(reader[50]);
                            mRobotTypeList.RTMaxSummer = reader.IsDBNull(51) ? 0 : Convert.ToDecimal(reader[51]);
                            mRobotTypeList.RTMinWinter = reader.IsDBNull(52) ? 0 : Convert.ToDecimal(reader[52]);
                            mRobotTypeList.RTMaxWinter = reader.IsDBNull(53) ? 0 : Convert.ToDecimal(reader[53]);
                            //
                            decimal min1 = Math.Min(mRobotTypeList.RTMinWinter, mRobotTypeList.RTMinSpring);
                            decimal min2 = Math.Min(mRobotTypeList.RTMinSummer, mRobotTypeList.RTMinFall);
                            decimal max1 = Math.Max(mRobotTypeList.RTMaxWinter, mRobotTypeList.RTMaxSpring);
                            decimal max2 = Math.Max(mRobotTypeList.RTMaxSummer, mRobotTypeList.RTMaxFall);
                            mRobotTypeList.RTMaxAll = Math.Max(max1, max2);
                            decimal RTMin = Math.Min(min1, min2);
                            //if (RTMin > -1000)
                            {
                                mRobotTypeList.RTMinAll = Math.Min(min1, min2);
                                mRobotTypeList.PathMinRT = reader.GetDateTime(54);
                                mRobotTypeList.SourceFuel = reader.GetValue(55).ToString();
                                mRobotTypeList.SinkFuel = reader.GetValue(56).ToString();
                                int anaType = 0;
                                if (int.TryParse(mRobotTypeList.AnalysisType, out anaType))
                                    mRobotTypeList.AnalysisTypeInt = anaType;

                                RobotList.Add(mRobotTypeList);
                            }
                        }

                    }
                    Debug.WriteLine("Stop: " + DateTime.Now.ToString());
                }
#if NEW
#else
                //else
                //{
                //    while (reader.Read())
                //    {
                //        RobotTypeList mRobotTypeList = new RobotTypeList();
                //        mRobotTypeList.SourceName = reader.GetString(0);
                //        mRobotTypeList.SinkName = reader.GetString(1);
                //        mRobotTypeList.AnalysisType = reader.GetString(2);
                //        mRobotTypeList.Price = GetDouble(reader[3]);
                //        mRobotTypeList.SumValue = GetDouble(reader[4]);
                //        mRobotTypeList.MaxWin = GetDouble(reader[5]);
                //        mRobotTypeList.MaxLoss = GetDouble(reader[6]);
                //        mRobotTypeList.MW = GetDouble(reader[7]);
                //        mRobotTypeList.RiskReward = GetDouble(reader[8]);
                //        mRobotTypeList.CountDays = GetDouble(reader[9]);
                //        mRobotTypeList.CountCleared = GetDouble(reader[10]);
                //        mRobotTypeList.PctWin = GetDouble(reader[11]);
                //        mRobotTypeList.YearlyUpSide = GetDouble(reader[12]);
                //        mRobotTypeList.YearlyRiskReward = GetDouble(reader[13]);
                //        mRobotTypeList.AvgDA = GetDouble(reader[14]);
                //        mRobotTypeList.CalcNumber = GetDouble(reader[15]);
                //        mRobotTypeList.SavedTime = reader.GetValue(16) == DBNull.Value ? DateTime.Today : reader.GetDateTime(16);
                //        int anaType = 0;
                //        if (int.TryParse(mRobotTypeList.AnalysisType, out anaType))
                //            mRobotTypeList.AnalysisTypeInt = anaType;

                //        RobotList.Add(mRobotTypeList);
                //    }

                //    System.Threading.Tasks.Parallel.ForEach(RobotList, a =>
                //    {
                //        a.SourceNodeType = mNodeTypeHash.ContainsKey(a.SourceName) ? mNodeTypeHash[a.SourceName] : "";
                //        a.SinkNodeType = mNodeTypeHash.ContainsKey(a.SinkName) ? mNodeTypeHash[a.SinkName] : "";
                //        a.SourceZone = mZoneHash.ContainsKey(a.SourceName) ? mZoneHash[a.SourceName] : "";
                //        a.SinkZone = mZoneHash.ContainsKey(a.SinkName) ? mZoneHash[a.SinkName] : "";
                //    });
                //}
#endif
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                VayuConnection.Close();
            }

            callback(RobotList, null);

        }

        #endregion

        #region Private Methods

        private void FillNodeHash()
        {
            Dictionary<int, string> mNodehash = new Dictionary<int, string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand mSelectNodeCommand = VayuConnection.CreateCommand();
            mSelectNodeCommand.CommandText = "";
        }

        private Dictionary<int, string> GetNodeNames()
        {
            try
            {
                Dictionary<int, string> mNodeHash = new Dictionary<int, string>();
                SqlCommand mSelectNodeCommand = VayuConnection.CreateCommand();
                mSelectNodeCommand.CommandText = "select  nodekey , nodename from EESSourceSinkValidNodes";
                mSelectNodeCommand.Connection = VayuConnection;
                if (VayuConnection.State == ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                SqlDataReader rdr = mSelectNodeCommand.ExecuteReader();
                while (rdr.Read())
                {
                    string nodeName = rdr.GetValue(1).ToString();
                    int nodekay = Convert.ToInt32(rdr.GetValue(0));
                    if (!mNodeHash.ContainsKey(nodekay))
                    {
                        mNodeHash.Add(nodekay, nodeName);
                    }
                }
                rdr.Close();
                return mNodeHash;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private double GetDouble(object objValue)
        {
            double doubVal = 0;
            if (double.TryParse(objValue.ToString(), out doubVal))
                return doubVal;
            else
                return double.NaN;
        }

        private int GetInt(object objValue)
        {
            int doubVal = 0;
            if (int.TryParse(objValue.ToString(), out doubVal))
                return doubVal;
            else
                return 0;
        }

        private float GetFloat(object objValue)
        {
            float floatVal = 0;
            if (float.TryParse(objValue.ToString(), out floatVal))
            {
                return floatVal;
            }
            else
            {
                return 0;
            }
        }

        private void FillZoneNodeHashValues()
        {
            if (VayuConnection == null)
            {
                loadDBCommands();
            }
            try
            {
                if (mSelectNodeZoneTypeCommand.Connection.State.Equals(ConnectionState.Closed))
                {
                    mSelectNodeZoneTypeCommand.Connection.Open();
                }
                IDataReader reader = mSelectNodeZoneTypeCommand.ExecuteReader();
                while (reader.Read())
                {
                    string nodeName = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString();
                    string zone = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString();
                    string type = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString();
                    if (nodeName != "")
                    {
                        if (mZoneHash.ContainsKey(nodeName))
                        {
                            mZoneHash[nodeName] = zone;
                        }
                        else
                        {
                            mZoneHash.Add(nodeName, zone);
                        }

                        if (mNodeTypeHash.ContainsKey(nodeName))
                        {
                            mNodeTypeHash[nodeName] = type;
                        }
                        else
                        {
                            mNodeTypeHash.Add(nodeName, type);
                        }
                    }
                }
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (mSelectNodeZoneTypeCommand.Connection.State.Equals(ConnectionState.Open))
                {
                    mSelectNodeZoneTypeCommand.Connection.Close();
                }
            }
        }

        #endregion
    }
}
