
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Configuration;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Vayu.CRRCalculationLibrary;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRPNLDownload
{
   
    class CRRPNLdownloadMain
    {
        private SqlConnection mConnection90;
        private SqlCommand mInsertVirtualPnlCommand;
        private SqlCommand mDeleteVirtualPnlCommand;
        private SqlCommand mSelectPortfolioCommand;
        private SqlCommand mSelectPJMMarketParticipantsCommand;
        private SqlCommand mSelectMISOMarketParticipantsCommand;
        private SqlCommand mUpdateVirtualPnlCommandCommand;
        private SqlCommand mSelectERCOTParicipantsCommand;
        private DataTable mPositionPnlDT = new DataTable();
        private string mEndPoint = Vayu.CommonAccessLibrary.ServiceConnections.GetFTRService();
        private ISourceSink mFTRCalculationProxy = null;        
        private int mERCOTKey = 9;        
        #region OurParticipant
        private List<string> mERCOTAccount = new List<string> { "" };
        private List<string> mERCOTAllAccount = new List<string>();
        #endregion

        public void CRRPNLCalculationMain()
        {
            InitDB();
            Connect();
            DownloadCRRPNL();
        }
        //Connect to FTRCalculation Server
        private void Connect()
        {
           
            TcpTransportBindingElement transport = new TcpTransportBindingElement();
            transport.TransferMode = TransferMode.Streamed;
            BinaryMessageEncodingBindingElement encoder = new BinaryMessageEncodingBindingElement();
            CustomBinding binding = new CustomBinding(encoder, transport);
            NetTcpBinding myBinding = new NetTcpBinding();
            myBinding.CloseTimeout = new TimeSpan(0, 30, 0);
            myBinding.OpenTimeout = new TimeSpan(0, 30, 0);
            myBinding.SendTimeout = new TimeSpan(0, 30, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 30, 0);
            myBinding.TransactionFlow = false;
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferPoolSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;
            myBinding.Security.Mode = SecurityMode.None;
            myBinding.TransferMode = TransferMode.Buffered;
            myBinding.ReaderQuotas.MaxArrayLength = 5000000;

            //myBinding.MaxReceivedMessageSize = int.MaxValue;
            ChannelFactory<ISourceSink> pipeFactory = new ChannelFactory<ISourceSink>(myBinding, new EndpointAddress(mEndPoint));
            foreach (OperationDescription op in pipeFactory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior =
                            op.Behaviors.Find<DataContractSerializerOperationBehavior>()
                            as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
            try
            {
                mFTRCalculationProxy = pipeFactory.CreateChannel();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + ex.Message);
            }
        }
        //Initiatize Database configuration
        private void InitDB()
        {
            mConnection90 = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mPositionPnlDT.Columns.Add("Date", typeof(DateTime));
            mPositionPnlDT.Columns.Add("FTRID", typeof(int));
            mPositionPnlDT.Columns.Add("DA", typeof(decimal));
            mPositionPnlDT.Columns.Add("Cost", typeof(decimal));
            mPositionPnlDT.Columns.Add("PNL", typeof(decimal));
            mPositionPnlDT.Columns.Add("FTRAuctionKey", typeof(int));
            mPositionPnlDT.Columns.Add("PeriodKey", typeof(int));
            //
           
            //
           
            //
            //
            //
            mSelectPortfolioCommand = new SqlCommand();
            mSelectPortfolioCommand.CommandText = "select portfolio_id from portfolio where strip = @strip and hub=@hub and product='CRR'";
            mSelectPortfolioCommand.Parameters.AddWithValue("@strip", "strip");
            mSelectPortfolioCommand.Parameters.AddWithValue("@hub", "hub");
            mSelectPortfolioCommand.Connection = mConnection90;
            //
            mDeleteVirtualPnlCommand = new SqlCommand();
            mDeleteVirtualPnlCommand.CommandText = "delete VIRTUAL_PNL where portfoliokey = @portfoliokey and pnldate = @pnldate";
            mDeleteVirtualPnlCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mDeleteVirtualPnlCommand.Parameters.AddWithValue("@pnldate", "pnldate");
            mDeleteVirtualPnlCommand.Connection = mConnection90;
            //
            mInsertVirtualPnlCommand = new SqlCommand();
            mInsertVirtualPnlCommand.CommandText = "insert VIRTUAL_PNL values (@portfoliokey, @pnldate, @pnl, 0, @pnlUpdateTime, null)";
            mInsertVirtualPnlCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
            mInsertVirtualPnlCommand.Parameters.AddWithValue("@pnldate", "pnldate");
            mInsertVirtualPnlCommand.Parameters.AddWithValue("@pnl", "pnl");
            mInsertVirtualPnlCommand.Parameters.AddWithValue("@pnlUpdateTime", "DateTimecurrent");
            mInsertVirtualPnlCommand.Connection = mConnection90;
            
            //
            mUpdateVirtualPnlCommandCommand = new SqlCommand();
            mUpdateVirtualPnlCommandCommand.CommandText = "update VIRTUAL_PNL set Fee=@Fee , feeUpdateTime=@feeUpdateTime where PortfolioKey=@PortfolioKey and PnlDate=@PnlDate";
            mUpdateVirtualPnlCommandCommand.Parameters.AddWithValue("@Fee", "Fee");
            mUpdateVirtualPnlCommandCommand.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
            mUpdateVirtualPnlCommandCommand.Parameters.AddWithValue("@PnlDate", "PnlDate");
            mUpdateVirtualPnlCommandCommand.Parameters.AddWithValue("@feeUpdateTime", "feeUpdateTime");
            mUpdateVirtualPnlCommandCommand.Connection = mConnection90;
            //
            mSelectPJMMarketParticipantsCommand = new SqlCommand();
            mSelectPJMMarketParticipantsCommand.CommandText = "select distinct Participant from pjm.ftrauctionresults order by Participant";
            mSelectPJMMarketParticipantsCommand.Connection = mConnection90;
            //
            mSelectMISOMarketParticipantsCommand = new SqlCommand();
            mSelectMISOMarketParticipantsCommand.CommandText = "select distinct Participant from miso.FTRAuctionResults order by Participant";
            mSelectMISOMarketParticipantsCommand.Connection = mConnection90;
            //
            mSelectERCOTParicipantsCommand = new SqlCommand();
            mSelectERCOTParicipantsCommand.CommandText = "select distinct(AccountHolder) from CRRAuctionResults";
                                                                                
            mSelectERCOTParicipantsCommand.Connection = mConnection90;
            //
        }
        private void SaveInternalList(List<SourceSink> sourceSinks, int market)
        {
            List<SourceSink> internalList = new List<SourceSink>();
            var itemnew = sourceSinks.Where(a => a.Cost != null);
            CRRDailyCalculation(sourceSinks, market);
            internalList.AddRange(sourceSinks);
            Dictionary<string, Dictionary<DateTime, double>> participantHash = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (SourceSink sourceSink in internalList)
            {
                Dictionary<DateTime, double> pnlHash = new Dictionary<DateTime, double>();
                if (participantHash.ContainsKey(sourceSink.Participant))
                {
                    pnlHash = participantHash[sourceSink.Participant];
                    participantHash.Remove(sourceSink.Participant);
                }
                foreach (KeyValuePair<DateTime, double> item in sourceSink.dailyPNL)
                {
                    if (!pnlHash.ContainsKey(item.Key))
                    {
                        pnlHash.Add(item.Key, item.Value);
                    }
                    else
                    {
                        pnlHash[item.Key] += item.Value;
                    }
                }
                participantHash.Add(sourceSink.Participant, pnlHash);
            }
            LoadDB(participantHash, GetMarket(market));
        }
        private string GetMarket(int market)
        {
            switch (market)
            {
                
                case 9: return "ERCOT";
                default: return "";
            }
        }
        private void CalculateCRRPnls(int marketKey, DateTime period)
        {
            List<List<string>> splitList = new List<List<string>>();
            List<SourceSink> sourceSinks = new List<SourceSink>();

            if (marketKey == 9)
            {
                Console.WriteLine("Loading ERCOT " + period.ToShortDateString());
                marketKey = mERCOTKey;
                try
                {
                   // sourceSinks = mFTRCalculationProxy.GetFTRs(mERCOTKey, mERCOTAccount, period);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                for (int i = 0; i < mERCOTAllAccount.Count; i = i + 30)
                {
                    splitList.Add(mERCOTAllAccount.GetRange(i, Math.Min(30, mERCOTAllAccount.Count - i)));
                }
            }
           
            IEnumerable sourceSinkFiltered = null;
            if (sourceSinks != null && sourceSinks.Count > 0)
            {
                sourceSinkFiltered = sourceSinks.Where(a => a.Cost != null);
                SaveInternalList(sourceSinks, marketKey);
            }
            for (int n = 0; n < splitList.Count; n++)
            {
                Connect();
                try
                {
                    Console.WriteLine("Getting Data from Service..");
                    sourceSinks = mFTRCalculationProxy.GetFTRs(marketKey, splitList[n], period);
                    if (sourceSinks != null && sourceSinks.Count > 0)
                    {
                        sourceSinkFiltered = sourceSinks.Where(a => a.Cost != null);
                        CRRDailyCalculation(sourceSinks, marketKey);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Connect();
                    foreach (string account in splitList[n])
                    {
                        List<string> accounts = new List<string>() { account };
                        try
                        {
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("Loading account " + account);
                            sourceSinks = mFTRCalculationProxy.GetFTRs(marketKey, accounts, period);
                            sourceSinkFiltered = sourceSinks.Where(a => a.Cost != null);
                            CRRDailyCalculation(sourceSinks, marketKey);
                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine(ex1);
                            Connect();
                            Console.WriteLine(account + " " + ex1);
                        }
                    }                   
                }
            }
        }
        private void DownloadCRRPNL()
        {
            DateTime startDate = AppConfigStartDate();
            DateTime endDate = AppConfigEndDate();
            if (endDate == DateTime.MinValue)
            {
                endDate = DateTime.Today.AddDays(1);
            }
            if (startDate == DateTime.MinValue)
            {
                startDate = new DateTime(2023, 08, 01);//DateTime(endDate.AddMonths(0).Year, endDate.AddMonths(0).Month-1, 1) ;//DateTime.Parse(endDate.AddMonths(-3).Month + "/1/" + endDate.AddMonths(-3).Year);
                if(endDate.Month==1)
                {
                    startDate = new DateTime(endDate.AddMonths(-1).Year, endDate.AddMonths(-1).Month, 1);
                }
                else
                startDate = new DateTime(endDate.AddMonths(0).Year, endDate.AddMonths(0).Month-1, 1) ;//DateTime.Parse(endDate.AddMonths(-3).Month + "/1/" + endDate.AddMonths(-3).Year);
            }
            PrepareAllAccounts();           
#if PRODUCTION
            Parallel.For(0, 3, k =>
            //for (int i = 1; i < 23; i++)
#else
            for (int k = 0; k <= 0; k++)
#endif
            {
                for (DateTime period = startDate; period <= endDate; period = period.AddMonths(1))
                {
                    CalculateCRRPnls(mERCOTKey, period);
                }
#if PRODUCTION
            });
#else
            }
#endif
        }

        private DateTime AppConfigEndDate()
        {
            try
            {
                DateTime endDate = Convert.ToDateTime(ConfigurationSettings.AppSettings["EndDate"]);
                return endDate;
            }
            catch (Exception ex)
            {
                return DateTime.MinValue;
            }
        }

        private DateTime AppConfigStartDate()
        {
            try
            {
                DateTime startDate = Convert.ToDateTime(ConfigurationSettings.AppSettings["StartDate"]);
                return startDate;
            }
            catch (Exception ex)
            {
                return DateTime.MinValue;
            }
        }
        private void PrepareAllAccounts()
        {
            Console.WriteLine("Getting All Account..");
            for (int i = 1; i <= 1; i++)
            {
                using (var con = new SqlConnection(mConnection90.ConnectionString))
                {
                    using (var cmd = con.CreateCommand())
                    {
                        con.Open();
                        if (i == 1)
                        {
                            cmd.CommandText = mSelectERCOTParicipantsCommand.CommandText;
                        }                        
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string account = reader.GetString(0);                            
                            if (i == 1)
                            {
                                if (!mERCOTAccount.Contains(account))
                                {
                                    mERCOTAllAccount.Add(account);
                                }
                            }                            
                        }
                        reader.Close();
                    }
                }
            }
        }
        private void CRRDailyCalculation(List<SourceSink> sourceSinkList, int marketKey)
        {
            try
            {
                Dictionary<string, Tuple<double, Dictionary<DateTime, DailyPnlHelper>>> dailyHash = GetFormattedSourceSink(sourceSinkList);
                Console.WriteLine("Calculated Results for marketkey " + marketKey + " " + sourceSinkList.Count);
                if (sourceSinkList != null && sourceSinkList.Count > 0)
                {
                    DataTable dt = CreateDataTable(dailyHash, marketKey, sourceSinkList[0].Month);
                    dailyHash = null;
                    if (dt.Rows.Count > 0)
                    {
                        Console.WriteLine("Insert into DB..");
                        InsertTableToDb(dt);
                    }
                    dt.Dispose();
                    dt = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void InsertTableToDb(DataTable tempDataTable)
        {
            try
            {
                SqlConnection connection90 = new VayuDBConnection().GetInstance().GetSqlConnection();
              //  LogWriter.writeLog("Inserting bulkdata into db" + tempDataTable.Rows.Count);
                var df = tempDataTable.DefaultView.ToTable(true, (from a in tempDataTable.Columns.Cast<DataColumn>() select a.ColumnName).ToArray());
                using (SqlConnection con = new SqlConnection(connection90.ConnectionString))
                {
                    con.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con) { DestinationTableName = "CRRDailyPNLReport" })
                    {
                        bulkCopy.ColumnMappings.Add("Participant", "Participant");
                        bulkCopy.ColumnMappings.Add("Month", "Month");
                        bulkCopy.ColumnMappings.Add("MW", "MW");
                        bulkCopy.ColumnMappings.Add("Cost", "Cost");
                        bulkCopy.ColumnMappings.Add("PNL", "PNL");
                        bulkCopy.ColumnMappings.Add("DAPrice", "DAPrice");
                        bulkCopy.ColumnMappings.Add("Hours", "Hours");
                        bulkCopy.ColumnMappings.Add("MarketKey", "MarketKey");
                        bulkCopy.ColumnMappings.Add("ReportDate", "ReportDate");
                        bulkCopy.ColumnMappings.Add("Monthly", "Monthly");
                        bulkCopy.ColumnMappings.Add("Q1", "Q1");
                        bulkCopy.ColumnMappings.Add("Q2", "Q2");
                        bulkCopy.ColumnMappings.Add("Q3", "Q3");
                        bulkCopy.ColumnMappings.Add("Q4", "Q4");
                        bulkCopy.ColumnMappings.Add("Annual", "Annual");
                        bulkCopy.ColumnMappings.Add("YR1", "YR1");
                        bulkCopy.ColumnMappings.Add("YR2", "YR2");
                        bulkCopy.ColumnMappings.Add("YR3", "YR3");
                        bulkCopy.ColumnMappings.Add("YRALL", "YRALL");
                        bulkCopy.WriteToServer(df);
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);               
            }
        }
        private Dictionary<string, Tuple<double, Dictionary<DateTime, DailyPnlHelper>>> GetFormattedSourceSink(List<SourceSink> sourcesinks)
        {
            Dictionary<string, Tuple<double, Dictionary<DateTime, DailyPnlHelper>>> dailyHash = new Dictionary<string, Tuple<double, Dictionary<DateTime, DailyPnlHelper>>>();
            Dictionary<string, double> mwHash = new Dictionary<string, double>();
            Dictionary<string, double> costHash = new Dictionary<string, double>();
            Console.WriteLine("Formating Source and Sink List..");
            foreach (SourceSink item in sourcesinks)
            {
                try
                {
                    if (mwHash.ContainsKey(item.Participant))
                    {
                        mwHash[item.Participant] += item.MW;
                    }
                    else
                    {
                        mwHash.Add(item.Participant, item.MW);
                    }
                    if (costHash.ContainsKey(item.Participant))
                    {
                        costHash[item.Participant] += item.Costmonthlytotal;
                    }
                    else
                    {
                        costHash.Add(item.Participant, item.Costmonthlytotal);
                    }
                    Dictionary<DateTime, DailyPnlHelper> tempDailyHash = new Dictionary<DateTime, DailyPnlHelper>();
                    if (dailyHash.ContainsKey(item.Participant))
                    {
                        tempDailyHash = dailyHash[item.Participant].Item2;
                        dailyHash.Remove(item.Participant);
                    }
                    foreach (var innerItem in item.dailyPNL)
                    {
                        DailyPnlHelper helperTemp = null;
                        if (tempDailyHash.ContainsKey(innerItem.Key))
                        {
                            helperTemp = tempDailyHash[innerItem.Key] as DailyPnlHelper;
                            tempDailyHash.Remove(innerItem.Key);
                        }
                        else
                        {
                            helperTemp = new DailyPnlHelper();
                        }
                        helperTemp.PNL += innerItem.Value;
                        if (item.PeriodType.ToUpper() == "ALL")
                        {
                            helperTemp.Annual += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "Q1" || item.PeriodType.ToUpper().StartsWith("SUM"))
                        {
                            helperTemp.Q1 += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "Q2" || item.PeriodType.ToUpper().StartsWith("FAL"))
                        {
                            helperTemp.Q2 += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "Q3" || item.PeriodType.ToUpper().StartsWith("WIN"))
                        {
                            helperTemp.Q3 += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "Q4" || item.PeriodType.ToUpper().StartsWith("SPR"))
                        {
                            helperTemp.Q4 += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "YR1")
                        {
                            helperTemp.YR1 += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "YR2")
                        {
                            helperTemp.YR2 += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "YR3")
                        {
                            helperTemp.YR3 += innerItem.Value;
                        }
                        else if (item.PeriodType.ToUpper() == "YRALL")
                        {
                            helperTemp.YRALL += innerItem.Value;
                        }
                        else
                        {
                            helperTemp.Monthly += innerItem.Value;
                        }
                        helperTemp.Cost = costHash.ContainsKey(item.Participant) ? costHash[item.Participant] : 0;
                        helperTemp.Price += item.dailyDAPrice.ContainsKey(innerItem.Key) ? item.dailyDAPrice[innerItem.Key] : 0;
                        helperTemp.Hours = item.Hours;
                        tempDailyHash.Add(innerItem.Key, helperTemp);
                    }
                    double mw = mwHash.ContainsKey(item.Participant) ? mwHash[item.Participant] : 0;
                    Tuple<double, Dictionary<DateTime, DailyPnlHelper>> mwTuple = new Tuple<double, Dictionary<DateTime, DailyPnlHelper>>(mw, tempDailyHash);
                    dailyHash.Add(item.Participant, mwTuple);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);                   
                }
            }            
            return dailyHash;
        }

        private DataTable CreateDataTable(Dictionary<string, Tuple<double, Dictionary<DateTime, DailyPnlHelper>>> dailyHash, int marketKey, string month)
        {
            DataTable tempDataTable = new DataTable();
            try
            {
                tempDataTable.Columns.Add("Participant");
                tempDataTable.Columns.Add("Month");
                tempDataTable.Columns.Add("MW");
                tempDataTable.Columns.Add("Cost");
                tempDataTable.Columns.Add("PNL");
                tempDataTable.Columns.Add("DAPrice");
                tempDataTable.Columns.Add("Hours");
                tempDataTable.Columns.Add("MarketKey");
                tempDataTable.Columns.Add("ReportDate");
                tempDataTable.Columns.Add("Monthly");
                tempDataTable.Columns.Add("Q1");
                tempDataTable.Columns.Add("Q2");
                tempDataTable.Columns.Add("Q3");
                tempDataTable.Columns.Add("Q4");
                tempDataTable.Columns.Add("Annual");
                tempDataTable.Columns.Add("YR1");
                tempDataTable.Columns.Add("YR2");
                tempDataTable.Columns.Add("YR3");
                tempDataTable.Columns.Add("YRALL");
                foreach (var item in dailyHash)
                {
                    using (var con = new SqlConnection(mConnection90.ConnectionString))
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "delete from  CRRDailyPNLReport where marketkey=" + marketKey + " and participant = '" + item.Key + "' and month between '" + DateTime.Parse(month).ToString("yyyy-MM-dd") + "' and '" + DateTime.Parse(GetLastDayOfMonth(DateTime.Parse(month))).ToString("yyyy-MM-dd") + "'";
                            try
                            {
                                con.Open();
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                    foreach (var innerItem in item.Value.Item2)
                    {
                        var dr = tempDataTable.NewRow();
                        dr[0] = item.Key;
                        dr[1] = innerItem.Key;
                        dr[2] = Math.Round(item.Value.Item1, 2);
                        dr[3] = Math.Round(innerItem.Value.Cost, 2);
                        dr[4] = Math.Round(innerItem.Value.PNL, 2);
                        dr[5] = Math.Round(innerItem.Value.Price, 2);
                        dr[6] = innerItem.Value.Hours;
                        dr[7] = marketKey;
                        dr[8] = DateTime.Now;
                        dr[9] = Math.Round(innerItem.Value.Monthly, 2);
                        dr[10] = Math.Round(innerItem.Value.Q1, 2);
                        dr[11] = Math.Round(innerItem.Value.Q2, 2);
                        dr[12] = Math.Round(innerItem.Value.Q3, 2);
                        dr[13] = Math.Round(innerItem.Value.Q4, 2);
                        dr[14] = Math.Round(innerItem.Value.Annual, 2);
                        dr[15] = Math.Round(innerItem.Value.YR1, 2);
                        dr[16] = Math.Round(innerItem.Value.YR2, 2);
                        dr[17] = Math.Round(innerItem.Value.YR3, 2);
                        dr[18] = Math.Round(innerItem.Value.YRALL, 2);
                        tempDataTable.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return tempDataTable;
        }
        private string GetLastDayOfMonth(DateTime dateTime)
        {
            return dateTime.AddDays(DateTime.DaysInMonth(dateTime.Year, dateTime.Month) - 1).ToString();
        }
        private void LoadDB(Dictionary<string, Dictionary<DateTime, double>> participantHash, string market)
        {
            foreach (string participant in participantHash.Keys.ToList())
            {
                try
                {
                    int portfolioId = 0;
                    using (SqlConnection con = new SqlConnection(mConnection90.ConnectionString))
                    {
                        con.Open();
                        mSelectPortfolioCommand.Connection = con;
                        mSelectPortfolioCommand.Parameters["@strip"].Value = participant;
                        mSelectPortfolioCommand.Parameters["@hub"].Value = market;
                        SqlDataReader reader = mSelectPortfolioCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            portfolioId = reader.GetInt32(0);
                        }
                        reader.Close();
                    }
                    Dictionary<DateTime, double> pnlHash = participantHash[participant];
                    List<DateTime> dateList = pnlHash.Keys.ToList<DateTime>();
                    foreach (DateTime date in dateList)
                    {
                        double pnl = pnlHash[date];
                        //Delete
                        using (var con1 = new SqlConnection(mConnection90.ConnectionString))
                        {
                            try
                            {
                                con1.Open();

                                //mDeleteVirtualPnlCommand.Connection = con1;
                                //mDeleteVirtualPnlCommand.Parameters["@portfoliokey"].Value = portfolioId;
                                //mDeleteVirtualPnlCommand.Parameters["@pnldate"].Value = date;
                                //mDeleteVirtualPnlCommand.ExecuteNonQuery();
                                ////Insert
                                //mInsertVirtualPnlCommand.Connection = con1;
                                //mInsertVirtualPnlCommand.Parameters["@portfoliokey"].Value = portfolioId;
                                //mInsertVirtualPnlCommand.Parameters["@pnldate"].Value = date;
                                //mInsertVirtualPnlCommand.Parameters["@pnl"].Value = pnl;
                                //mInsertVirtualPnlCommand.Parameters["@pnlUpdateTime"].Value = DateTime.Now;
                                //mInsertVirtualPnlCommand.ExecuteNonQuery();
                                //LogWriter.writeLog("Deleting and inserting into db portfolioKey" + portfolioId + "\t" + date + "\t" + pnl); 
                                con1.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                // sApplicationLog.UpdateInfo(MethodInfo.GetCurrentMethod().Name, ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //  sApplicationLog.UpdateError(MethodInfo.GetCurrentMethod().Name, ex.Message);
                }
            }
        }
    }
    //public class DailyPnlHelper
    //{
    //    public double PNL { get; set; }
    //    public double Monthly { get; set; }
    //    public double Annual { get; set; }
    //    public double Q1 { get; set; }
    //    public double Q2 { get; set; }
    //    public double Q3 { get; set; }
    //    public double Q4 { get; set; }
    //    public double YR1 { get; set; }
    //    public double YR2 { get; set; }
    //    public double YR3 { get; set; }
    //    public double YRALL { get; set; }
    //    public double Cost { get; set; }
    //    public double Price { get; set; }
    //    public double MW { get; set; }
    //    public int Hours { get; set; }
    //    public int MarketKey { get; set; }
    //}
}
