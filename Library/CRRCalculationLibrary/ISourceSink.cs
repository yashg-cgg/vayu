using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Description;

namespace Vayu.CRRCalculationLibrary
{
    
    [ServiceContract]
    public interface ISourceSink
    { 
        [OperationContract()]
        List<SourceSink> GetFTR(int marketkey, string account, DateTime period, bool gethourlypnl = false);
         
        [OperationContract()]
        List<SourceSink> GetFTRs(int marketkey, List<string> accounts, DateTime period, bool gethourlypnl = false);

        [OperationContract()]
        List<SourceSink> Get6MonthCRRs(int marketKey, List<string> accounts, DateTime period, string auctRound);

        [OperationContract()]
        List<SourceSink> GetFTRsFromSourceSinks(int marketkey, List<SourceSink> sourceSinks, DateTime period, bool gethourlypnl = false);

        [OperationContract()]
        Dictionary<int, Dictionary<int, Tuple<DailyValues, Dictionary<int, DailyValues>>>> FillFTRPathData(int Marketkey, long SourceId, long SinkId, DateTime startDate, DateTime endDate, string periodtype, string hedgetype,  int ftrQuarterMonth = 0, string auctionType = "Monthly");

        [OperationContract()]
        //Dictionary<int, Dictionary<int, Tuple<MonthlyValues, Dictionary<int, MonthlyValues>>>> FillFTRPathData1(int Marketkey, long SourceId, long SinkId, DateTime startDate, DateTime endDate, List<int> selectedMonths, string hedgetype,string classType);

        List<MonthlyValues> FillFTRPathData1(int Marketkey, long SourceId, long SinkId, DateTime startDate, DateTime endDate, List<int> selectedMonths, string hedgetype, string classType);


        [OperationContract()]
        Dictionary<DateTime, string> GetPeakYN_daterange(DateTime startDate, DateTime endDate);


    }
}
