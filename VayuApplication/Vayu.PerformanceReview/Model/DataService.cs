using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Vayu.CommonAccessLibrary;
using Vayu.PerformanceReview.ViewModels;

namespace Vayu.PerformanceReview.Model
{
    public class DataService : IDataService
    {
        SqlConnection vAccountingDBConnection;
        SqlCommand vSelectPNLDBCommand;
        SqlCommand vSelectPJMPNLDBCommand;
        SqlCommand vSelectERCOTPNLDBCommand;
        SqlCommand vSelectPNLDBCommandFTR;
        SqlCommand vSelectPNLDBCommandCRR;
        SqlCommand vSelectMWDBCommandFTR;
        SqlCommand vSelectPNLStatementCommand;
        SqlCommand vSelectPJMPNLStatementCommand;
        SqlCommand vSelectERCOTPNLStatementCommand;
        SqlCommand vSelectPNLDailyDBCommand;
        SqlCommand vSelectPNLDailyDBCommandFTR;
        SqlCommand vSelectPNLDailyDBCommandCRR;
        SqlCommand vSelecMWDailyDBCommandFTR;
        SqlCommand vSelectPNLMonthlyUTCCommand;
        SqlCommand vSelectPNLMonthlyFTRCommand;
        SqlCommand vSelectPNLMonthlyFTRCreditCommand;
        SqlCommand vSelectPNLMonthlyCRRCreditCommand;
        SqlCommand vSelectPNLMonthlyFTRDBCommand;
        SqlCommand vSelectPNLMonthlyFTRDBCommandSTMT;
        SqlCommand vSelectMaxDate;
        SqlCommand vSelectPNLDailyACLCommand;
        SqlCommand vSelectPNLDailyACLCRRCommand;
        SqlCommand vSelectPNLMonthlyACLCRRCommand;
        SqlCommand vSelectPNLDailyACLMonthlyCommand;
        SqlCommand vSelectERCOTLDailyDBCommand;
        SqlCommand stSelectERCOTLDailyDAM;
        SqlCommand stSelectERCOTLDailyRTM;
        private void stLoadDBCommand()
        {
            //vAccountingDBConnection = new SqlConnection("Data Source = ; Initial Catalog = VayuAccounting; Persist Security Info = True; User Id = VayuAcc; password = Vayu@2023!; Connect Timeout = 100000; MultipleActiveResultSets = True");
            vAccountingDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            // vAccountingDBConnection.Open();

            vSelectPNLDBCommand = new SqlCommand();
            vSelectPNLDBCommand.CommandText = "select  sum(gross1), sum(fees1),  sum(DollarsCleared1),sum(CongestionUplift) as CongestionUplift  from (  " +
                                                 " select isnull( sum(b.gross),sum(pnl1) ) as gross1, isnull(sum(b.Fee), sum(fee1)) as fees1 ,  sum(DollarsCleared) as DollarsCleared1, sum(CongestionUplift) as CongestionUplift from (" +
                                                 "  select sum(pnl) as pnl1, sum(fee) as fee1, sum(mw) as mw , " +
                                                 "sum(DollarsCleared) as DollarsCleared, PnlDate as PnlDate1  from [Pnl] where " +
                                                 "pnldate>=@startdate and pnldate<=@enddate   group by PnlDate  ) a left join PNLAccounting   b on PnlDate1 " +
                                                 "=b.MarketDate and b.Product='utc' or b.MarketDate is null group by pnldate1) main";
            vSelectPNLDBCommand.Parameters.AddWithValue("@startdate", "pnldate");
            vSelectPNLDBCommand.Parameters.AddWithValue("@enddate", "pnldate");

            vSelectPJMPNLDBCommand = new SqlCommand();
            vSelectPJMPNLDBCommand.CommandText = "select  sum(gross1), sum(fees1),  sum(DollarsCleared1),sum(CongestionUplift) as CongestionUplift, isnull(sum(fees1)/sum(mws),0) as feemw, sum(IsoGross) as GrossIso, sum(ISOFee) as FeeISO,sum(mws) as mws  from (  " +
                                                 " select isnull( sum(b.gross),sum(pnl1) ) as gross1, isnull(sum(b.Fee), sum(fee1)) as fees1 ,  sum(DollarsCleared) as DollarsCleared1, sum(CongestionUplift) as CongestionUplift, sum(mw) as mws, sum(IsoMiscRev) as IsoGross, sum(IsoMiscExp) as ISOFee from (" +
                                                 "  select sum(pnl) as pnl1, sum(fee) as fee1, sum(mw) as mw , " +
                                                 "sum(DollarsCleared) as DollarsCleared, PnlDate as PnlDate1  from Pjm.Pnl where " +
                                                 "pnldate>=@startdate and pnldate<=@enddate   group by PnlDate  ) a left join Pjm.PNLAccounting   b on PnlDate1 " +
                                                 "=b.MarketDate and b.Product='utc' or b.MarketDate is null group by pnldate1) main";
            vSelectPJMPNLDBCommand.Parameters.AddWithValue("@startdate", "pnldate");
            vSelectPJMPNLDBCommand.Parameters.AddWithValue("@enddate", "pnldate");

            vSelectERCOTPNLDBCommand = new SqlCommand();
            //vSelectERCOTPNLDBCommand.CommandText = "select  sum(gross1), sum(fees1),  sum(DollarsCleared1),sum(CongestionUplift) as CongestionUplift, SUM(fees1)/Sum(mws), sum(mws)  from (  " +
            //                                     " select isnull( sum(b.gross),sum(pnl1) ) as gross1, isnull(sum(b.Fee), sum(fee1)) as fees1 ,  sum(DollarsCleared) as DollarsCleared1, sum(CongestionUplift) as CongestionUplift, SUM(mw) as mws from (" +
            //                                     "  select sum(pnl) as pnl1, sum(fee) as fee1, sum(mw) as mw , " +
            //                                     "sum(DollarsCleared) as DollarsCleared, PnlDate as PnlDate1  from Ercot.Pnl where " +
            //                                     "pnldate>=@startdate and pnldate<=@enddate   group by PnlDate  ) a left join Ercot.PNLAccounting   b on PnlDate1 " +
            //                                     "=b.MarketDate and b.Product='utc' or b.MarketDate is null group by pnldate1) main";

            vSelectERCOTPNLDBCommand.CommandText = " select sum(pnl) as gross1, sum(Fee) as Fees, sum(DollarsCleared) asDollarsCleared, sum(Fee) as CongestionUplift, SUM(Fee) / Sum(mw), sum(mw) from DailyPNL where pnldate>=@startdate and pnldate<=@enddate   ";
            vSelectERCOTPNLDBCommand.Parameters.AddWithValue("@startdate", "pnldate");
            vSelectERCOTPNLDBCommand.Parameters.AddWithValue("@enddate", "pnldate");


            vSelectPNLDBCommandFTR = new SqlCommand();
            vSelectPNLDBCommandFTR.CommandText = "select sum( ISNULL( b.gross, a.PNL)) gross,sum(ISNULL( b.Fee, 0)) Fee, sum( ISNULL( b.Credit, 0)) Credit,sum( ISNULL( b.CongestionUplift, 0)) ,sum(b.Fee)/nullif(sum(a.mw),0),sum( ISNULL( b.IsoMiscRev, 0)),sum( ISNULL( b.IsoMiscExp, 0)), sum (a.mw) " +
                                                    " from AccountingApp.PJM.FTRDailyPNLReport  a left join AccountingApp.PJM.PNLAccounting   b on b.MarketDate = a.Month where(a.Participant = 'iso1' and b.Product = 'ftr' or b.MarketDate is null)" +
                                                    " and a.Month >=@startdate and a.Month <= @enddate  and a.Participant = 'iso1'";

            vSelectPNLDBCommandFTR.Parameters.AddWithValue("@startdate", "Month");
            vSelectPNLDBCommandFTR.Parameters.AddWithValue("@enddate", "Month");

            vSelectPNLDBCommandCRR = new SqlCommand();
            // stSelectPNLDBCommandCRR.CommandText = "select sum(IsNULL(DAPrice, 0)), 0 as Fee, 0 as Credit, 0 as CongestionUplift, 0 as FeeperMW from AccountingApp.ERCOT.CRRDailyPNLReport where Month >=@startdate and Month <= @enddate  and Participant='xiso2'";
            //stSelectPNLDBCommandCRR.CommandText = "select sum(DAPrice) as Net, sum(b.Amount) as gross, sum(b.Amount)-sum(DAPrice) as fee from AccountingApp.ERCOT.CRRDailyPNLReport a"+
            //                                    " join AccountingApp.ERCOT.Crrpnlstatement b on a.Month = b.MarketDateTime where(a.Month >= @startdate and a.Month <= @enddate  and a.Participant = 'xiso2')";
            vSelectPNLDBCommandCRR.CommandText = "select sum( ISNULL( -1*a.Amount,0)) As NET,  sum( ISNULL(  b.DAPrice,-1*a.Amount)) As PNL, sum(ISNULL((-1*a.Amount)-b.DAPrice,0)) as Fee, sum(b.MW)as mw from CRRPNLstatement a right join " +
                                                    " CRRDailyPNLReport b on b.Month = a.MarketDateTime where(b.Month >= @startdate and b.Month <= @enddate  and b.Participant = 'Qenjre')";
            vSelectPNLDBCommandCRR.Parameters.AddWithValue("@startdate", "Month");
            vSelectPNLDBCommandCRR.Parameters.AddWithValue("@enddate", "Month");

            vSelectMWDBCommandFTR = new SqlCommand();
            vSelectMWDBCommandFTR.CommandText = "select  isnull(sum(mw),0) from PJM.PNL where PnlDate >=@startdate and PnlDate<=@enddate";

            vSelectMWDBCommandFTR.Parameters.AddWithValue("@startdate", "Month");
            vSelectMWDBCommandFTR.Parameters.AddWithValue("@enddate", "Month");

            vSelectPNLDailyDBCommand = new SqlCommand();
            vSelectPNLDailyDBCommand.CommandText = " select isnull( sum(b.gross),sum(pnl1) ), isnull(sum(b.Fee), sum(fee1)), isnull(sum(b.gross)+sum(b.Fee), " +
                                                      " sum(pnl1)+sum(fee1)) , sum(DollarsCleared) ,isnull(sum(b.CongestionUplift),0), PnlDate1, sum(fee1)/sum(mw),isnull(sum(b.IsoMiscRev),0),isnull(sum(b.IsoMiscExp),0), isnull(sum(mw),0) as mws from ( " +
                                                      "   select sum(pnl) as pnl1, sum(fee) as fee1, sum(mw) as mw ,  " +
                                                      " sum(DollarsCleared) as DollarsCleared, PnlDate as PnlDate1  from Pjm.Pnl where " +
                                                      " pnldate>=@startdate and pnldate<=@enddate   group by PnlDate  ) a left join Pjm.PNLAccounting   b on PnlDate1 " +
                                                      " =b.MarketDate and b.Product='utc' or b.MarketDate is null group by pnldate1  order by PnlDate1 ";


            vSelectPNLDailyDBCommand.Parameters.AddWithValue("@startdate", "pnldate");
            vSelectPNLDailyDBCommand.Parameters.AddWithValue("@enddate", "pnldate");


            vSelectPNLDailyDBCommandFTR = new SqlCommand();
            vSelectPNLDailyDBCommandFTR.CommandText = "select isnull( sum(b.gross+credit),sum(pnl1) ), isnull(sum(b.Fee), sum(fee1)), isnull(sum(b.gross)+sum(b.Fee), sum(pnl1)+sum(fee1))  ,PnlDate1," +
                                                      "sum(Credit)Credit,sum(CongestionUplift)CongestionUplift, sum(b.Fee)/nullif(sum(mw),0),sum(IsoMiscRev), sum(IsoMiscExp), sum(a.mw) from (select sum(pnl) as pnl1, 0 as fee1, sum(mw) as mw , 0 as DollarsCleared, MONTH  as PnlDate1  " +
                                                      "from AccountingApp.PJM.FTRDailyPNLReport where MONTH>=@startdate and MONTH<=@enddate  and Participant='iso1' group by MONTH  ) a left join AccountingApp.PJM.PNLAccounting   b" +
                                                      " on PnlDate1=b.MarketDate and b.Product='ftr' or b.MarketDate is null group by pnldate1";

            vSelectPNLDailyDBCommandFTR.Parameters.AddWithValue("@startdate", "Month");
            vSelectPNLDailyDBCommandFTR.Parameters.AddWithValue("@enddate", "Month");

            vSelectPNLDailyDBCommandCRR = new SqlCommand();
            //stSelectPNLDailyDBCommandCRR.CommandText = "select sum(ISNULL(DAPrice,0)), 0 as Fee, 0 as Credit, 0 as ConestionUplift, 0 as FeePerMW, MONTH from AccountingApp.ERCOT.CRRDailyPNLReport"+
            //                                            " where Month >= @startdate and Month <= @enddate  and Participant = 'xiso2'  group by Month";
            //stSelectPNLDailyDBCommandCRR.CommandText= "select sum(Net), sum(ISnull(b.Amount,a.pnl)) as gross, sum(ISnull(b.Amount,a.pnl))-sum(Net) as fee, date from" +
            //                                            " (select sum(DAPrice) as Net,sum(pnl)as pnl, MONTH as date from AccountingApp.ERCOT.CRRDailyPNLReport where Month >= @startdate and" +
            //                                            " Month <= @enddate  and Participant = 'xiso2'  group by Month) a left join AccountingApp.ERCOT.Crrpnlstatement b" +
            //                                               " on date= b.MarketDateTime group by date";

            vSelectPNLDailyDBCommandCRR.CommandText = "select sum( ISNULL( -1*a.Amount,0)) As NET,  sum( ISNULL(  b.DAPrice,-1*a.Amount)) As PNL, sum(ISNULL((-1*a.Amount)-b.DAPrice,0)) as Fee, b.Month, sum(b.MW) from CRRPNLstatement a right join " +
                                                        " CRRDailyPNLReport b on b.Month = a.MarketDateTime Where b.Month >= @startdate and b.Month <= @enddate Group by b.Month order by b.Month";

            vSelectPNLDailyDBCommandCRR.Parameters.AddWithValue("@startdate", "Month");
            vSelectPNLDailyDBCommandCRR.Parameters.AddWithValue("@enddate", "Month");

            vSelecMWDailyDBCommandFTR = new SqlCommand();
            vSelecMWDailyDBCommandFTR.CommandText = " select  isnull(sum(mw),0) from PJM.PNL where PnlDate =@startdate";

            vSelecMWDailyDBCommandFTR.Parameters.AddWithValue("@startdate", "Month");
            //stSelecMWDailyDBCommandFTR.Parameters.AddWithValue("@enddate", "Month");

            vSelectPNLStatementCommand = new SqlCommand();
            vSelectPNLStatementCommand.CommandText = " select PNL, MiscellaneousCharges from PNLFromStatement where startdate=@startdate and marketkey=1";
            vSelectPNLStatementCommand.Parameters.AddWithValue("@startdate", "startdate");
            //stSelectPNLStatementCommand.Connection = stDBConnection;

            vSelectPJMPNLStatementCommand = new SqlCommand();
            vSelectPJMPNLStatementCommand.CommandText = "select PNL, MiscellaneousCharges from Pjm.PNLFromStatement where startdate=@startdate and marketkey=1";
            vSelectPJMPNLStatementCommand.Parameters.AddWithValue("@startdate", "startdate");

            vSelectERCOTPNLStatementCommand = new SqlCommand();
            vSelectERCOTPNLStatementCommand.CommandText = "select PNL, MiscellaneousCharges from PNLFromStatement where startdate=@startdate and marketkey=1";
            vSelectERCOTPNLStatementCommand.Parameters.AddWithValue("@startdate", "startdate");

            vSelectPNLMonthlyUTCCommand = new SqlCommand();
            //stSelectPNLMonthlyUTCCommand.CommandText = "select MarketDate, Fee from Trident..PNLAccountingMonthly   where MarketDate>=@sDate and MarketDate<=@eDate  and Product='UTC' order by MarketDate";
            vSelectPNLMonthlyUTCCommand.CommandText = "select MarketDate, Fee from Pjm.PNLAccountingMonthly   where MarketDate >=@sDate and MarketDate<=@eDate  and Product = 'UTC' order by MarketDate";
            vSelectPNLMonthlyUTCCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLMonthlyUTCCommand.Parameters.AddWithValue("@eDate", "MarketDate");

            vSelectPNLDailyACLCommand = new SqlCommand();
            vSelectPNLDailyACLCommand.CommandText = " select businessdate, sum( transactionamount) from CollateralTransactionsSummary  where BusinessDate>=@sDate and BusinessDate<=@eDate and "
                        + " (Description Not like 'STL%' and Description Not like 'Cash%' and Description not like '' and Description not like '%deposit%' and Description Not like 'CRR%' and Description Not like '%return%') group by businessdate  order by BusinessDate";
            //stSelectPNLDailyACLCommand.CommandText = " select businessdate, sum( transactionamount) from Ercot.CollateralTransactionsSummary  where BusinessDate >=@sDate and BusinessDate<=@eDate'" +
            //                                     " and Description like '%Short Pay%' group by businessdate order by BusinessDate";
            vSelectPNLDailyACLCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLDailyACLCommand.Parameters.AddWithValue("@eDate", "MarketDate");

            vSelectPNLDailyACLCRRCommand = new SqlCommand();
            vSelectPNLDailyACLCRRCommand.CommandText = " select businessdate, sum( transactionamount) from CollateralTransactionsSummary where BusinessDate>=@sDate and BusinessDate<=@eDate and" +
                                                           "  Description like 'CRR%' group by businessdate order by BusinessDate";
            vSelectPNLDailyACLCRRCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLDailyACLCRRCommand.Parameters.AddWithValue("@eDate", "MarketDate");

            vSelectPNLMonthlyACLCRRCommand = new SqlCommand();
            vSelectPNLMonthlyACLCRRCommand.CommandText = "select sum( transactionamount) from CollateralTransactionsSummary where BusinessDate>=@sDate and BusinessDate<= @eDate and Description  like 'CRR%'";
            vSelectPNLMonthlyACLCRRCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLMonthlyACLCRRCommand.Parameters.AddWithValue("@eDate", "MarketDate");

            vSelectPNLDailyACLMonthlyCommand = new SqlCommand();
            //stSelectPNLDailyACLMonthlyCommand.CommandText = " select sum( transactionamount) from CollateralTransactionsSummary  where BusinessDate>=@sDate and BusinessDate<=@eDate and "
            //            + " Description like '%Short Pay%' ";
            vSelectPNLDailyACLMonthlyCommand.CommandText = "select sum(transactionamount) from CollateralTransactionsSummary  where BusinessDate >=@sDate and BusinessDate<= @eDate" +
                                         " and (Description Not like 'STL%' and Description Not like 'Cash%' and Description not like '' and Description not like '%deposit%' and Description Not like 'CRR%' and Description not like '%return%')";
            vSelectPNLDailyACLMonthlyCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLDailyACLMonthlyCommand.Parameters.AddWithValue("@eDate", "MarketDate");




            vSelectPNLMonthlyFTRCommand = new SqlCommand();
            //stSelectPNLMonthlyFTRCommand.CommandText = "select MarketDate, PNL from Trident..PNLAccountingMonthly   where MarketDate>=@sDate and MarketDate<= @eDate and Product='FTR' order by MarketDate";
            vSelectPNLMonthlyFTRCommand.CommandText = "select MarketDate, PNL from Pjm.PNLAccountingMonthly   where MarketDate >==@sDate and MarketDate<=@eDate and Product = 'FTR' order by MarketDate";

            vSelectPNLMonthlyFTRCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLMonthlyFTRCommand.Parameters.AddWithValue("@eDate", "MarketDate");
            vSelectPNLMonthlyFTRCommand.Connection = vAccountingDBConnection;

            vSelectPNLMonthlyCRRCreditCommand = new SqlCommand();
            //vSelectPNLMonthlyCRRCreditCommand.CommandText = "select -1 * sum(Amount) from ERCOT.CRRPNLstatement where MarketDateTime >=@sDate and MarketDateTime<=@eDate";
            vSelectPNLMonthlyCRRCreditCommand.CommandText = "select sum(pnl) from CRRPNL where StartDate >=@sDate and StartDate<=@eDate";
            vSelectPNLMonthlyCRRCreditCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLMonthlyCRRCreditCommand.Parameters.AddWithValue("@eDate", "MarketDate");
            vSelectPNLMonthlyCRRCreditCommand.Connection = vAccountingDBConnection;

            //changee
            vSelectPNLMonthlyFTRCreditCommand = new SqlCommand();
            // stSelectPNLMonthlyFTRCreditCommand.CommandText = "select -1*sum(Credit ), sum(gross), sum(fee) from Trident..PNLAccounting   where MarketDate>=@sDate and MarketDate<= @eDate and Product='FTR' ";
            vSelectPNLMonthlyFTRCreditCommand.CommandText = "select - 1 * sum(Credit), sum(gross), sum(fee) from Pjm.PNLAccounting   where MarketDate >=@sDate and MarketDate<=@eDate and Product = 'FTR'";
            vSelectPNLMonthlyFTRCreditCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLMonthlyFTRCreditCommand.Parameters.AddWithValue("@eDate", "MarketDate");
            vSelectPNLMonthlyFTRCreditCommand.Connection = vAccountingDBConnection;


            vSelectPNLMonthlyFTRDBCommand = new SqlCommand();
            //stSelectPNLMonthlyFTRDBCommand.CommandText = "select sum(pnl) from Trident..FTRDailyPNLReport where Participant ='iso1' and  Month>=@sDate and Month<= @eDate  ";
            vSelectPNLMonthlyFTRDBCommand.CommandText = "select sum(pnl) from Pjm.PNLAccounting where Product = 'ftr' and MarketDate>=@sDate and MarketDate<=@eDate";
            vSelectPNLMonthlyFTRDBCommand.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLMonthlyFTRDBCommand.Parameters.AddWithValue("@eDate", "MarketDate");
            vSelectPNLMonthlyFTRDBCommand.Connection = vAccountingDBConnection;

            vSelectPNLMonthlyFTRDBCommandSTMT = new SqlCommand();
            //stSelectPNLMonthlyFTRDBCommandSTMT.CommandText = "select gross-Credit, Fee,  MarketDate from Trident..PNLAccountingMonthly   where Product='ftr' order by MarketDate ";
            vSelectPNLMonthlyFTRDBCommandSTMT.CommandText = "select gross-Credit, Fee,  MarketDate from Pjm.PNLAccountingMonthly where Product = 'ftr' order by MarketDate";
            vSelectPNLMonthlyFTRDBCommandSTMT.Parameters.AddWithValue("@sDate", "MarketDate");
            vSelectPNLMonthlyFTRDBCommandSTMT.Parameters.AddWithValue("@eDate", "MarketDate");
            vSelectPNLMonthlyFTRDBCommandSTMT.Connection = vAccountingDBConnection;


            vSelectMaxDate = new SqlCommand();
            //stSelectMaxDate.CommandText = " select  max(MarketDate)  from PNLAccounting   where Product=@Product";
            vSelectMaxDate.CommandText = " select  max(MarketDate)  from Pjm.PNLAccounting   where Product=@Product";
            vSelectMaxDate.Parameters.AddWithValue("@Product", "Product");
            vSelectMaxDate.Connection = vAccountingDBConnection;

            //ERCOT

            vSelectERCOTLDailyDBCommand = new SqlCommand();

            //vSelectERCOTLDailyDBCommand.CommandText = " select  isnull( sum(b.gross),sum(pnl1) ), isnull(sum(b.Fee), sum(fee1)), isnull(sum(b.gross)+sum(b.Fee), " +
            //                                         " sum(pnl1)+sum(fee1)) , sum(DollarsCleared) ,isnull(sum(b.CongestionUplift),0), PnlDate1, isnull(sum(b.fee)/SUM(a.mw), sum(fee1)/sum(a.mw)), sum(a.mw) from ( " +
            //                                         "   select sum(pnl) as pnl1, sum(fee) as fee1, sum(mw) as mw ,  " +
            //                                         " sum(DollarsCleared) as DollarsCleared, PnlDate as PnlDate1  from ERCOT.Pnl where " +
            //                                         " pnldate>=@startdate and pnldate<=@enddate   group by PnlDate  ) a left join ERCOT.PNLAccounting   b on PnlDate1 " +
            //                                         " =b.MarketDate and b.Product='utc' or b.MarketDate is null group by pnldate1  order by PnlDate1 ";
            vSelectERCOTLDailyDBCommand.CommandText = " with cte1 as ( "
                                            + " select sum(a.pnl) as Gross,  sum(a.mw) as Mw, "
                                            + "   YEAR(a.StartDate) as Year, MONTH(a.StartDate) as Month, c.USERALIAS as USERALIAS, b.account  from CRRPNL a "
                                            + "   join  PORTFOLIO b on a.PortfolioKey = b.PORTFOLIO_ID "
                                            + "   join END_USER c on b.ENDUSER_KEY = c.ENDUSER_KEY "
                                            + "     where a.StartDate >= @startdate and a.StartDate <= @enddate "
                                            + "     group by YEAR(a.StartDate), MONTH(a.StartDate),  c.USERALIAS, b.account "
                                            + "     ), "
                                            + "   cte2 as ( "
                                            + "   select sum(a.pnl) as Gross, sum(a.fee) as Fee, sum(a.mw) as Mw, sum(a.dollarscleared) as DollarsCleared, "
                                            + "   YEAR(a.PnlDate) as Year, MONTH(a.PnlDate) as Month, c.USERALIAS as USERALIAS, b.account  from dailypnl a "
                                            + "   join PORTFOLIO b on a.PortfolioKey = b.PORTFOLIO_ID "
                                            + "   join END_USER c on b.ENDUSER_KEY = c.ENDUSER_KEY "
                                            + "   where a.PnlDate >= @startdate and a.PnlDate <= @enddate "
                                            + "   group by YEAR(a.PnlDate), MONTH(a.PnlDate),  c.USERALIAS, b.account "
                                            + "   ) "
                                            + " select cte1.Gross as CrrGross, cte1.Mw as CrrMw, cte2.Gross as PtpGross, cte2.Fee as PtpFee, "
                                            + " cte2.Mw as PtpMw, cte2.DollarsCleared,coalesce(cte2.Year, cte1.Year) as Year, coalesce(cte2.Month, cte1.Month) as month, "
                                            + " coalesce(cte2.USERALIAS, cte1.USERALIAS) as USERALIAS , coalesce( cte2.account, cte1.account) account from cte1 full outer "
                                            + "                                                       join cte2  on "
                                            + " cte1.Year = cte2.Year "
                                            + " and cte1.Month = cte2.Month and "
                                            + " cte1.USERALIAS = cte2.USERALIAS "
                                            + " order by Year, Month, cte2.USERALIAS";
            vSelectERCOTLDailyDBCommand.Parameters.AddWithValue("@startdate", "pnldate");
            vSelectERCOTLDailyDBCommand.Parameters.AddWithValue("@enddate", "pnldate");

            stSelectERCOTLDailyDAM = new SqlCommand();
            stSelectERCOTLDailyDAM.CommandText = "select isnull(Amount,0) from PNLstatement where Type like'%DAM%' and MarketDateTime =@startdate";
            stSelectERCOTLDailyDAM.Parameters.AddWithValue("@startdate", "pnldate");
            stSelectERCOTLDailyDAM.Connection = vAccountingDBConnection;

            stSelectERCOTLDailyRTM = new SqlCommand();
            stSelectERCOTLDailyRTM.CommandText = "select isnull(Amount,0) from PNLstatement where Type like'%RTM%' and MarketDateTime = @startdate";
            stSelectERCOTLDailyRTM.Parameters.AddWithValue("@startdate", "pnldate");
            stSelectERCOTLDailyRTM.Connection = vAccountingDBConnection;


        }

        private System.Collections.Generic.Dictionary<DateTime, double> GetMonthlyUTCData(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> keyList = new Dictionary<DateTime, double>();

            stLoadDBCommand();
            if (vAccountingDBConnection.State == ConnectionState.Open)
            {
                vAccountingDBConnection.Close();
            }
            vAccountingDBConnection.Open();
            vSelectPNLMonthlyUTCCommand.Parameters["@sDate"].Value = startDate;
            vSelectPNLMonthlyUTCCommand.Parameters["@eDate"].Value = endDate;
            vSelectPNLMonthlyUTCCommand.Connection = vAccountingDBConnection;
            SqlDataReader reader2 = vSelectPNLMonthlyUTCCommand.ExecuteReader();
            while (reader2.Read())
            {
                keyList.Add(reader2.GetDateTime(0), Math.Round(Convert.ToDouble(reader2.GetValue(1)), 2));
            }
            reader2.Close();

            return keyList;
        }
        private System.Collections.Generic.Dictionary<DateTime, double> GetMonthlyACLData(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> keyList = new Dictionary<DateTime, double>();

            stLoadDBCommand();
            DateTime sdate = startDate;
            DateTime edate = endDate;
            while (startDate <= edate)
            {
                if (vAccountingDBConnection.State == ConnectionState.Open)
                {
                    vAccountingDBConnection.Close();
                }
                vAccountingDBConnection.Open();
                vSelectPNLDailyACLMonthlyCommand.Parameters["@sDate"].Value = startDate;
                vSelectPNLDailyACLMonthlyCommand.Parameters["@eDate"].Value = startDate.AddMonths(1).AddDays(-1);
                vSelectPNLDailyACLMonthlyCommand.Connection = vAccountingDBConnection;
                SqlDataReader reader2 = vSelectPNLDailyACLMonthlyCommand.ExecuteReader();
                while (reader2.Read())
                {
                    if (reader2.IsDBNull(0))
                    {
                        keyList.Add(startDate, 0);
                    }
                    else
                    {
                        keyList.Add(startDate, Math.Round(Convert.ToDouble(reader2.GetValue(0)), 2));
                    }
                }
                reader2.Close();
                vAccountingDBConnection.Close();
                startDate = startDate.AddMonths(1);
            }
            return keyList;
        }
        //changee
        private System.Collections.Generic.Dictionary<DateTime, double> GetMonthlyACLCRRData(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> keyList = new Dictionary<DateTime, double>();

            stLoadDBCommand();
            DateTime sdate = startDate;
            DateTime edate = endDate;
            while (startDate <= edate)
            {
                if (vAccountingDBConnection.State == ConnectionState.Open)
                {
                    vAccountingDBConnection.Close();
                }
                vAccountingDBConnection.Open();
                vSelectPNLMonthlyACLCRRCommand.Parameters["@sDate"].Value = startDate;
                vSelectPNLMonthlyACLCRRCommand.Parameters["@eDate"].Value = startDate.AddMonths(1).AddDays(-1);
                vSelectPNLMonthlyACLCRRCommand.Connection = vAccountingDBConnection;
                SqlDataReader reader2 = vSelectPNLMonthlyACLCRRCommand.ExecuteReader();
                while (reader2.Read())
                {
                    if (reader2.IsDBNull(0))
                    {
                        keyList.Add(startDate, 0);
                    }
                    else
                    {
                        keyList.Add(startDate, Math.Round(Convert.ToDouble(reader2.GetValue(0)), 2));
                    }
                }
                reader2.Close();
                vAccountingDBConnection.Close();
                startDate = startDate.AddMonths(1);

            }
            return keyList;
        }

        private System.Collections.Generic.Dictionary<DateTime, double> GetMonthlyACLCRRDaily(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> keyList = new Dictionary<DateTime, double>();

            stLoadDBCommand();
            if (vAccountingDBConnection.State == ConnectionState.Open)
            {
                vAccountingDBConnection.Close();
            }
            vAccountingDBConnection.Open();
            vSelectPNLDailyACLCRRCommand.Parameters["@sDate"].Value = startDate;
            vSelectPNLDailyACLCRRCommand.Parameters["@eDate"].Value = endDate;
            vSelectPNLDailyACLCRRCommand.Connection = vAccountingDBConnection;
            SqlDataReader reader2 = vSelectPNLDailyACLCRRCommand.ExecuteReader();
            while (reader2.Read())
            {
                keyList.Add(reader2.GetDateTime(0), Math.Round(Convert.ToDouble(reader2.GetValue(1)), 2));
            }
            reader2.Close();


            vAccountingDBConnection.Close();
            return keyList;
        }
        private System.Collections.Generic.Dictionary<DateTime, double> GetMonthlyACLDaily(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> keyList = new Dictionary<DateTime, double>();

            stLoadDBCommand();
            if (vAccountingDBConnection.State == ConnectionState.Open)
            {
                vAccountingDBConnection.Close();
            }
            vAccountingDBConnection.Open();
            vSelectPNLDailyACLCommand.Parameters["@sDate"].Value = startDate;
            vSelectPNLDailyACLCommand.Parameters["@eDate"].Value = endDate;
            vSelectPNLDailyACLCommand.Connection = vAccountingDBConnection;
            //SqlDataReader reader2 = vSelectPNLDailyACLCommand.ExecuteReader();
            //while (reader2.Read())
            //{
            //    keyList.Add(reader2.GetDateTime(0), Math.Round(Convert.ToDouble(reader2.GetValue(1)), 2));
            //}
            //reader2.Close();
            //vAccountingDBConnection.Close();
            return keyList;
        }

        private System.Collections.Generic.Dictionary<DateTime, double> GetMonthlyFTRData(DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> keyList = new Dictionary<DateTime, double>();

            stLoadDBCommand();
            if (vAccountingDBConnection.State == ConnectionState.Open)
            {
                vAccountingDBConnection.Close();
            }
            vAccountingDBConnection.Open();
            vSelectPNLMonthlyFTRCommand.Parameters["@sDate"].Value = startDate;
            vSelectPNLMonthlyFTRCommand.Parameters["@eDate"].Value = endDate;
            vSelectPNLMonthlyFTRCommand.Connection = vAccountingDBConnection;
            SqlDataReader reader2 = vSelectPNLMonthlyFTRCommand.ExecuteReader();
            while (reader2.Read())
            {
                keyList.Add(reader2.GetDateTime(0), Math.Round(Convert.ToDouble(reader2.GetValue(1)), 2));
            }
            reader2.Close();
            vAccountingDBConnection.Close();
            return keyList;
        }
        //private System.Collections.Generic.Dictionary<DateTime, ReconcilationMTLY> GetMonthlyFTRCreditData(DateTime startDate, DateTime endDate)
        //{
        //    Dictionary<DateTime, ReconcilationMTLY> keyList = new Dictionary<DateTime, ReconcilationMTLY>();

        //    stLoadDBCommand();
        //    while (startDate <= endDate)
        //    {
        //        if (vAccountingDBConnection.State == ConnectionState.Open)
        //        {
        //            vAccountingDBConnection.Close();
        //        }
        //        vAccountingDBConnection.Open();
        //        vSelectPNLMonthlyFTRCreditCommand.Parameters["@sDate"].Value = startDate;
        //        vSelectPNLMonthlyFTRCreditCommand.Parameters["@eDate"].Value = startDate.AddMonths(1).AddDays(-1);
        //        vSelectPNLMonthlyFTRCreditCommand.Connection = vAccountingDBConnection;
        //        SqlDataReader reader2 = vSelectPNLMonthlyFTRCreditCommand.ExecuteReader();
        //        while (reader2.Read())
        //        {
        //            ReconcilationMTLY oReconcilationMTLY = new ReconcilationMTLY();
        //            double credit = reader2.IsDBNull(0) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(0)), 2);
        //            double gross = reader2.IsDBNull(1) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(1)), 2);
        //            double fee = reader2.IsDBNull(2) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(2)), 2);
        //            oReconcilationMTLY.Gross = gross;
        //            oReconcilationMTLY.Fee = fee;
        //            oReconcilationMTLY.PNL = credit;
        //            keyList.Add(startDate, oReconcilationMTLY);
        //        }
        //        reader2.Close();
        //        vAccountingDBConnection.Close();
        //        startDate = startDate.AddMonths(1);
        //    }
        //    return keyList;
        //}

        //private System.Collections.Generic.Dictionary<DateTime, ReconcilationMTLY> GetMonthlyCRRCreditData(DateTime startDate, DateTime endDate)
        //{
        //    Dictionary<DateTime, ReconcilationMTLY> keyList = new Dictionary<DateTime, ReconcilationMTLY>();

        //    stLoadDBCommand();
        //    while (startDate <= endDate)
        //    {
        //        if (vAccountingDBConnection.State == ConnectionState.Open)
        //        {
        //            vAccountingDBConnection.Close();
        //        }
        //        vAccountingDBConnection.Open();
        //        vSelectPNLMonthlyCRRCreditCommand.Parameters["@sDate"].Value = startDate;
        //        vSelectPNLMonthlyCRRCreditCommand.Parameters["@eDate"].Value = startDate.AddMonths(1).AddDays(-1);
        //        vSelectPNLMonthlyCRRCreditCommand.Connection = vAccountingDBConnection;
        //        SqlDataReader reader2 = vSelectPNLMonthlyCRRCreditCommand.ExecuteReader();
        //        while (reader2.Read())
        //        {
        //            ReconcilationMTLY oReconcilationMTLY = new ReconcilationMTLY();
        //            double credit = reader2.IsDBNull(0) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(0)), 2);
        //            oReconcilationMTLY.PNL = credit;
        //            keyList.Add(startDate, oReconcilationMTLY);
        //        }
        //        reader2.Close();
        //        vAccountingDBConnection.Close();
        //        startDate = startDate.AddMonths(1);
        //    }
        //    return keyList;
        //}
        //private System.Collections.Generic.Dictionary<DateTime, double> GetMonthlyFTRCreditDataDB(DateTime startDate, DateTime endDate)
        //{
        //    Dictionary<DateTime, double> keyList = new Dictionary<DateTime, double>();

        //    stLoadDBCommand();
        //    while (startDate <= endDate)
        //    {
        //        if (vAccountingDBConnection.State == ConnectionState.Open)
        //        {
        //            vAccountingDBConnection.Close();
        //        }
        //        vAccountingDBConnection.Open();
        //        vSelectPNLMonthlyFTRDBCommand.Parameters["@sDate"].Value = startDate;
        //        vSelectPNLMonthlyFTRDBCommand.Parameters["@eDate"].Value = startDate.AddMonths(1).AddDays(-1);
        //        vSelectPNLMonthlyFTRDBCommand.Connection = vAccountingDBConnection;
        //        SqlDataReader reader2 = vSelectPNLMonthlyFTRDBCommand.ExecuteReader();
        //        while (reader2.Read())
        //        {
        //            double credit = reader2.IsDBNull(0) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(0)), 2);
        //            keyList.Add(startDate, credit);
        //        }
        //        reader2.Close();
        //        vAccountingDBConnection.Close();
        //        startDate = startDate.AddMonths(1);
        //    }
        //    return keyList;
        //}
        //private System.Collections.Generic.Dictionary<DateTime, ReconcilationMTLY> GetMonthlyFTRCreditDataSTMT(DateTime startDate, DateTime endDate)
        //{
        //    Dictionary<DateTime, ReconcilationMTLY> keyList = new Dictionary<DateTime, ReconcilationMTLY>();

        //    stLoadDBCommand();
        //    //while (startDate <= endDate)
        //    {
        //        if (vAccountingDBConnection.State == ConnectionState.Open)
        //        {
        //            vAccountingDBConnection.Close();
        //        }
        //        vAccountingDBConnection.Open();
        //        vSelectPNLMonthlyFTRDBCommandSTMT.Connection = vAccountingDBConnection;
        //        SqlDataReader reader2 = vSelectPNLMonthlyFTRDBCommandSTMT.ExecuteReader();
        //        while (reader2.Read())
        //        {
        //            ReconcilationMTLY reconcilationMTLY = new ReconcilationMTLY();
        //            reconcilationMTLY.PNL = reader2.IsDBNull(0) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(0)), 2);
        //            reconcilationMTLY.Fee = reader2.IsDBNull(1) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(1)), 2);
        //            // double credit = reader2.IsDBNull(0) ? 0 : Math.Round(Convert.ToDouble(reader2.GetValue(0)), 2);
        //            keyList.Add(reader2.GetDateTime(2), reconcilationMTLY);
        //        }
        //        reader2.Close();
        //        vAccountingDBConnection.Close();
        //        // startDate = startDate.AddMonths(1);
        //    }
        //    return keyList;
        //}

        public List<Performance> GetReconcilationListDaily(DateTime startDate, DateTime endDate, int marketkey)
        {
            // List<ReconcilationMTLY> stPNLMonthlyList = GetReconcilationList(startDate, endDate, marketkey);
            DateTime sdate = startDate;
            DateTime edate = endDate;
            List<Performance> listReconcilation = new List<Performance>();
            Dictionary<DateTime, double> stUTCErcotDic = new System.Collections.Generic.Dictionary<DateTime, double>();
            stLoadDBCommand();
            if (marketkey == 9)
            {
                //stUTCErcotDic = GetMonthlyACLDaily(startDate, endDate);
            }
            if (marketkey == 1)
            {
                if (vAccountingDBConnection.State == ConnectionState.Open)
                {
                    vAccountingDBConnection.Close();
                }
                vAccountingDBConnection.Open();
                vSelectPNLDailyDBCommand.Connection = vAccountingDBConnection;
                vSelectPNLDailyDBCommand.Parameters["@startdate"].Value = sdate;
                vSelectPNLDailyDBCommand.Parameters["@enddate"].Value = endDate;
                SqlDataReader reader = vSelectPNLDailyDBCommand.ExecuteReader();
                double? runningtotal = 0;
                int M = 1;
                double a = 0;
                while (reader.Read())
                {
                    // int N = M + 4;
                    Performance stReconcilation = new Performance();
                    stReconcilation.QuantName = reader.GetDateTime(5).ToString();
                    //stReconcilation.DateFormat = reader.GetDateTime(5);
                    //   DateTime comparedate = new DateTime(stReconcilation.DateFormat.Year, stReconcilation.DateFormat.Month, 01);
                    stReconcilation.CRRFee = Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);
                    stReconcilation.CRRNet = Math.Round(Convert.ToDouble(reader.GetValue(7)), 2);
                    stReconcilation.CRRMw = Math.Round(Convert.ToDouble(reader.GetValue(1)), 2);
                    stReconcilation.CompanyMW = Math.Round(Convert.ToDouble(reader.GetValue(9)), 2);
                    stReconcilation.PTPGross = Math.Round(Convert.ToDouble(reader.GetValue(8)), 2);
                    stReconcilation.PTPNet = stReconcilation.CRRNet + stReconcilation.PTPGross;
                    // stReconcilation.ISOMiscellaneousCharges = Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
                    //stReconcilation.DailyFeePerMW = Math.Round(Convert.ToDouble(reader.GetValue(6)), 2);
                    List<Performance> removelist = new List<Performance>();
                    foreach (var m in listReconcilation)
                    {
                        removelist.Add(m);
                    }
                    DateTime YesterdayDate = endDate.AddDays(-1);
                    DateTime YesterdayMonth = endDate.AddMonths(-1);
                    DateTime NextYear = YesterdayDate.AddYears(-1);
                    //if (stReconcilation.CRRMw == 0 && stReconcilation.DateFormat.Year == NextYear.Year)
                    //{
                    //    YesterdayDate = NextYear;
                    //}

                    //if (stReconcilation.CRRMw == 0 && stReconcilation.DateFormat.Year == YesterdayDate.Year)//||(stReconcilation.DailyFee == 0 && stReconcilation.DateFormat.Month == YesterdayMonth.Month))
                    //{

                    //    if (M == 1)
                    //    {
                    //        List<ReconcilationDLY> templist = new List<ReconcilationDLY>();
                    //        for (int i = 1; i <= 5; i++)
                    //        {

                    //            var x = removelist.LastOrDefault();
                    //            templist.Add(x);
                    //            removelist.Remove(x);

                    //        }
                    //        a = (double)templist.Sum(x => x.CompanyFee);
                    //    }
                    //    M++;

                    //    double b = a / 5;
                    //    stReconcilation.CompanyFee = b;

                    //}
                    //else
                    //{
                    //    if (!reader.IsDBNull(6))
                    //        stReconcilation.CompanyFee = Math.Round(Convert.ToDouble(reader.GetValue(6)), 4);
                    //}

                    runningtotal = Math.Round(Convert.ToDouble(runningtotal), 2) + stReconcilation.CRRFee + stReconcilation.CRRMw + stReconcilation.PTPNet;
                    stReconcilation.PTPFee = Math.Round(Convert.ToDouble(reader.GetValue(2)), 2) + Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);// stReconcilation.ISOMiscellaneousCharges;
                    stReconcilation.PTPMW = Math.Round(Convert.ToDouble(runningtotal), 2);
                    stReconcilation.CompanyNet = Math.Round(Convert.ToDouble(reader.GetValue(3)), 2);
                    stReconcilation.CompanyGross = 0;
                    stReconcilation.PTPDollarsCleared = 0;
                    //stReconcilation.RunningTotal1 = 0;

                    listReconcilation.Add(stReconcilation);
                }
                reader.Close();
                vAccountingDBConnection.Close();
            }
            else
            if (marketkey == 9)
            {
                if (vAccountingDBConnection.State == ConnectionState.Open)
                {
                    vAccountingDBConnection.Close();
                }
                vAccountingDBConnection.Open();

                vSelectERCOTLDailyDBCommand.Parameters["@startdate"].Value = sdate;
                vSelectERCOTLDailyDBCommand.Connection = vAccountingDBConnection;
                vSelectERCOTLDailyDBCommand.Parameters["@enddate"].Value = endDate;
                SqlDataReader reader1 = vSelectERCOTLDailyDBCommand.ExecuteReader();
                while (reader1.Read())
                {
                    try
                    {
                        Performance stReconcilation = new Performance();
                        //CrrGross	CrrMw	PtpGross	PtpFee	PtpMw	DollarsCleared	Year	month	USERALIAS
                        //stReconcilation.DateFormat = reader1.GetDateTime(4);
                        stReconcilation.CRRGross = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(0)) ? 0 : reader1.GetValue(0)), 2);
                        stReconcilation.CRRMw = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(1)) ? 0 : reader1.GetValue(1)), 2);// + iso;
                        stReconcilation.PTPGross = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(2)) ? 0 : reader1.GetValue(2)), 2);
                        stReconcilation.PTPFee = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(3)) ? 0 : reader1.GetValue(3)), 2);
                        stReconcilation.PTPMW = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(4)) ? 0 : reader1.GetValue(4)), 2);
                        stReconcilation.PTPDollarsCleared = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(5)) ? 0 : reader1.GetValue(5)), 2);
                        stReconcilation.Year = Convert.ToInt32(Convert.IsDBNull(reader1.GetValue(6)) ? 0 : reader1.GetValue(6));
                        stReconcilation.Month = DateTimeFormatInfo.CurrentInfo.GetMonthName(Convert.ToInt32(Convert.IsDBNull(reader1.GetValue(7)) ? 0 : reader1.GetValue(7)));
                        stReconcilation.QuantName = reader1.GetString(8).ToString();
                        stReconcilation.CRRFee = 0;
                        stReconcilation.PTPNet = stReconcilation.PTPGross + stReconcilation.PTPFee;
                        stReconcilation.CRRNet = stReconcilation.CRRGross + stReconcilation.CRRFee;

                        stReconcilation.CompanyGross = stReconcilation.PTPGross + stReconcilation.CRRGross;
                        stReconcilation.CompanyFee = stReconcilation.PTPFee + stReconcilation.CRRFee;
                        stReconcilation.CompanyMW = stReconcilation.PTPMW + stReconcilation.CRRMw;
                        stReconcilation.CompanyNet = stReconcilation.CompanyGross + stReconcilation.CompanyFee;
                        stReconcilation.AccountName = reader1.GetString(9).ToString();
                        listReconcilation.Add(stReconcilation);
                    }
                    catch (Exception ex)
                    {
                    }

                }
                reader1.Close();
                vAccountingDBConnection.Close();
            }
            return listReconcilation;
        }



        //public List<ReconcilationMTLY> GetMonthlyFTR(DateTime startDate, DateTime endDate, int marketkey)
        //{
        //    DateTime sdate = startDate;
        //    DateTime edate = endDate;
        //    DateTime monthedate;
        //    List<ReconcilationMTLY> listReconcilation = new List<ReconcilationMTLY>();
        //    Dictionary<DateTime, ReconcilationMTLY> stFTRDic = new System.Collections.Generic.Dictionary<DateTime, ReconcilationMTLY>();
        //    Dictionary<DateTime, ReconcilationMTLY> stFTRDic1 = new System.Collections.Generic.Dictionary<DateTime, ReconcilationMTLY>();
        //    Dictionary<DateTime, double> stFTRDicgrossdb = new System.Collections.Generic.Dictionary<DateTime, double>();
        //    stFTRDic = new Dictionary<DateTime, ReconcilationMTLY>();
        //    stFTRDicgrossdb = new Dictionary<DateTime, double>();
        //    stFTRDic1 = GetMonthlyFTRCreditDataSTMT(startDate, endDate);
        //    stFTRDic = GetMonthlyFTRCreditData(startDate, endDate);
        //    stFTRDicgrossdb = GetMonthlyFTRCreditDataDB(startDate, endDate);
        //    double? runningtotal = 0;
        //    while (sdate <= edate)
        //    {
        //        stLoadDBCommand();
        //        if (marketkey == 1)
        //        {

        //            if (vAccountingDBConnection.State == ConnectionState.Open)
        //            {
        //                vAccountingDBConnection.Close();
        //            }
        //            vAccountingDBConnection.Open();
        //            vSelectPNLDBCommandFTR.Connection = vAccountingDBConnection;
        //        }
        //        vSelectPNLDBCommandFTR.Parameters["@startdate"].Value = sdate;

        //        if (endDate <= sdate.AddMonths(1).AddDays(-1))
        //        {
        //            monthedate = endDate;
        //            vSelectPNLDBCommandFTR.Parameters["@enddate"].Value = endDate;
        //        }
        //        else
        //        {
        //            monthedate = sdate.AddMonths(1).AddDays(-1);
        //            vSelectPNLDBCommandFTR.Parameters["@enddate"].Value = sdate.AddMonths(1).AddDays(-1);
        //        }
        //        SqlDataReader reader = vSelectPNLDBCommandFTR.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            ReconcilationMTLY stReconcilation = new ReconcilationMTLY();
        //            DateTime sdate1 = sdate;
        //            stReconcilation.Gross = Convert.ToDouble(reader.IsDBNull(0) ? 0 : reader.GetValue(0)) - stFTRDic[sdate1].PNL;
        //            stReconcilation.DateFormat = sdate1;
        //            stReconcilation.Month = sdate1.ToString("MMM") + " " + sdate1.Year;

        //            // stReconcilation.Fee = Convert.ToDouble(reader.IsDBNull(1) ? 0 : reader.GetValue(1));
        //            stReconcilation.Fee = reader.IsDBNull(1) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(1)), 2);
        //            stReconcilation.FeePerMW = reader.IsDBNull(4) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(4)), 2);
        //            stReconcilation.ISOMiscellaneousCharges = 0;
        //            stReconcilation.MWs = reader.IsDBNull(7) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(7)), 2);
        //            if (marketkey == 1)
        //            {
        //                // 
        //                // if (stFTRDic.ContainsKey(sdate1))
        //                {
        //                    // if (stFTRDic1.ContainsKey(sdate1))
        //                    {
        //                        //stReconcilation.ISOMiscellaneousCharges = stFTRDic1[sdate1].PNL - stReconcilation.Gross;
        //                        //double isochanrges = Convert.ToDouble(reader.IsDBNull(3) ? 0 : reader.GetValue(3));
        //                        stReconcilation.MonthGrossISO = Convert.ToDouble(reader.IsDBNull(5) ? 0 : reader.GetValue(5));
        //                        stReconcilation.MonthFeeISO = Convert.ToDouble(reader.IsDBNull(6) ? 0 : reader.GetValue(6));
        //                        stReconcilation.ISOMiscellaneousCharges = stReconcilation.MonthGrossISO + stReconcilation.MonthFeeISO;
        //                        // stReconcilation.ISOMiscellaneousCharges = Convert.ToDouble(reader.IsDBNull(3) ? 0 : reader.GetValue(3));
        //                        stReconcilation.Gross = stReconcilation.Gross;// + isochanrges;
        //                        stReconcilation.PNL = stReconcilation.Gross + stReconcilation.Fee + stReconcilation.ISOMiscellaneousCharges;
        //                    }
        //                    //else
        //                    //{
        //                    //    stReconcilation.ISOMiscellaneousCharges = 0;
        //                    //    stReconcilation.PNL = stReconcilation.Gross + stReconcilation.Fee + stReconcilation.ISOMiscellaneousCharges;
        //                    //}

        //                }
        //            }
        //            // stReconcilation.PNL = stReconcilation.Gross + stReconcilation.Fee;
        //            runningtotal = stReconcilation.PNL;
        //            stReconcilation.MonthlyRunningTotal1 = runningtotal;
        //            stReconcilation.DAM = 0;
        //            stReconcilation.RTM = 0;
        //            listReconcilation.Add(stReconcilation);
        //        }


        //        reader.Close();

        //        string month = DateTime.Today.ToString("MMM") + " " + DateTime.Today.Year;
        //        bool boolvar = (listReconcilation.Exists(x => x.Month == month));
        //        sdate = sdate.AddMonths(1);
        //    }
        //    return listReconcilation;
        //}
        ////public List<ReconcilationDLY> GetDailyFTR(DateTime startDate, DateTime endDate, int marketkey)
        //{
        //    List<ReconcilationMTLY> stPNLMonthlyList = GetMonthlyFTR(startDate, endDate, marketkey);
        //    DateTime sdate = startDate;
        //    DateTime edate = endDate;
        //    List<ReconcilationDLY> listReconcilation = new List<ReconcilationDLY>();

        //    stLoadDBCommand();
        //    if (marketkey == 1)
        //    {

        //        if (vAccountingDBConnection.State == ConnectionState.Open)
        //        {
        //            vAccountingDBConnection.Close();
        //        }
        //        vAccountingDBConnection.Open();

        //        vSelectPNLDailyDBCommandFTR.Connection = vAccountingDBConnection;
        //        vSelectPNLDailyDBCommandFTR.Parameters["@startdate"].Value = sdate;
        //        vSelectPNLDailyDBCommandFTR.Parameters["@enddate"].Value = endDate;
        //        SqlDataReader reader = vSelectPNLDailyDBCommandFTR.ExecuteReader();
        //        double? runningtotal = 0;
        //        int M = 1;
        //        double a = 0;
        //        while (reader.Read())
        //        {


        //            ReconcilationDLY stReconcilation = new ReconcilationDLY();
        //            stReconcilation.QuantName = reader.GetDateTime(3).ToString();
        //            //stReconcilation.DateFormat = reader.GetDateTime(3);
        //            //stReconcilation.DailyFeePerMW= reader.IsDBNull(6) ? 0 : (Math.Round(Convert.ToDouble(reader.GetValue(6)), 2));

        //            stReconcilation.CRRMw = reader.IsDBNull(1) ? 0 : (Math.Round(Convert.ToDouble(reader.GetValue(1)), 2));

        //            List<ReconcilationDLY> removelist = new List<ReconcilationDLY>();
        //            foreach (var m in listReconcilation)
        //            {
        //                removelist.Add(m);
        //            }
        //            DateTime YesterdayDate = endDate.AddDays(-1);
        //            DateTime YesterdayMonth = endDate.AddMonths(-1);
        //            DateTime NextYear = YesterdayDate.AddYears(-1);
        //            //if (stReconcilation.CRRMw == 0 && stReconcilation.DateFormat.Year == NextYear.Year)
        //            //{
        //            //    YesterdayDate = NextYear;
        //            //}

        //            //if (stReconcilation.CRRMw == 0 && stReconcilation.DateFormat.Year == YesterdayDate.Year)//||(stReconcilation.DailyFee == 0 && stReconcilation.DateFormat.Month == YesterdayMonth.Month))
        //            //{

        //            //    if (M == 1)
        //            //    {
        //            //        List<ReconcilationDLY> templist = new List<ReconcilationDLY>();
        //            //        for (int i = 1; i <= 5; i++)
        //            //        {

        //            //            var x = removelist.LastOrDefault();
        //            //            templist.Add(x);
        //            //            removelist.Remove(x);

        //            //        }
        //            //        a = (double)templist.Sum(x => x.CompanyFee);
        //            //    }
        //            //    M++;

        //            //    double b = a / 5;
        //            //    stReconcilation.CompanyFee = b;

        //            //}
        //            //else
        //            //{
        //            //    if (!reader.IsDBNull(6))
        //            //        stReconcilation.CompanyFee = Math.Round(Convert.ToDouble(reader.GetValue(6)), 4);
        //            //}
        //            //// runningtotal = Math.Round(Convert.ToDouble(runningtotal), 2) + Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);

        //           // DateTime comparedate = new DateTime(stReconcilation.DateFormat.Year, stReconcilation.DateFormat.Month, 01);

        //            // stReconcilation.DailyGross = -1 * (Math.Round(Convert.ToDouble(reader.GetValue(0)), 2) + (reader.IsDBNull(4) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(4)), 2)));
        //            stReconcilation.CRRNet = reader.IsDBNull(7) ? 0 : (Math.Round(Convert.ToDouble(reader.GetValue(7)), 2));
        //            stReconcilation.PTPGross = reader.IsDBNull(8) ? 0 : (Math.Round(Convert.ToDouble(reader.GetValue(8)), 2));
        //            stReconcilation.PTPNet = stReconcilation.CRRNet + stReconcilation.PTPGross;// 0;
        //            //stReconcilation.ISOMiscellaneousCharges = reader.IsDBNull(5) ? 0 : (Math.Round(Convert.ToDouble(reader.GetValue(5)), 2));// 0;
        //            //double isocharges = Math.Round(Convert.ToDouble(reader.GetValue(5)), 2);
        //            stReconcilation.CRRFee = Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);// + isocharges;
        //            stReconcilation.PTPFee = stReconcilation.CRRFee + stReconcilation.CRRMw + stReconcilation.PTPNet;
        //            runningtotal = Math.Round(Convert.ToDouble(runningtotal), 2) + stReconcilation.PTPFee;
        //            //if (stReconcilation.DateFormat <= dtmaxdate)
        //            {
        //                stReconcilation.PTPFee = stReconcilation.CRRFee + stReconcilation.CRRMw + stReconcilation.PTPNet;
        //                runningtotal = Math.Round(Convert.ToDouble(runningtotal), 2) + stReconcilation.PTPFee;
        //                stReconcilation.PTPMW = Math.Round(Convert.ToDouble(runningtotal), 2);
        //                stReconcilation.CompanyGross = 0;
        //                stReconcilation.PTPDollarsCleared = 0;
        //                stReconcilation.CompanyMW = reader.IsDBNull(9) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(9)), 2);
        //            }
        //            //stReconcilation.DailyFee = Math.Round(Convert.ToDouble(reader.GetValue(1)), 2);
        //            listReconcilation.Add(stReconcilation);
        //        }
        //        reader.Close();

        //    }
        //    return listReconcilation;
        //}

        //change
        //public List<ReconcilationMTLY> GetMonthlyCRR(DateTime startDate, DateTime endDate, int marketkey)
        //{
        //    DateTime sdate = startDate;
        //    DateTime edate = endDate;
        //    DateTime monthedate;

        //    Dictionary<DateTime, double> stCRRErcotDic = new System.Collections.Generic.Dictionary<DateTime, double>();
        //    Dictionary<DateTime, ReconcilationMTLY> stCRRErcotDic1 = new System.Collections.Generic.Dictionary<DateTime, ReconcilationMTLY>();
        //    List<ReconcilationMTLY> listReconcilation = new List<ReconcilationMTLY>();

        //    if (marketkey == 9)
        //    {
        //        //stCRRErcotDic = GetMonthlyACLCRRData(startDate, endDate);
        //       // stCRRErcotDic1 = GetMonthlyCRRCreditData(startDate, endDate);
        //    }
        //    double? runningtotal = 0;
        //    while (sdate <= edate)
        //    {
        //        stLoadDBCommand();
        //        if (marketkey == 9)
        //        {

        //            if (vAccountingDBConnection.State == ConnectionState.Open)
        //            {
        //                vAccountingDBConnection.Close();
        //            }
        //            vAccountingDBConnection.Open();
        //            vSelectPNLDBCommandCRR.Connection = vAccountingDBConnection;
        //        }
        //        vSelectPNLDBCommandCRR.Parameters["@startdate"].Value = sdate;

        //        if (endDate <= sdate.AddMonths(1).AddDays(-1))
        //        {
        //            monthedate = endDate;
        //            vSelectPNLDBCommandCRR.Parameters["@enddate"].Value = endDate;
        //        }
        //        else
        //        {
        //            monthedate = sdate.AddMonths(1).AddDays(-1);
        //            vSelectPNLDBCommandCRR.Parameters["@enddate"].Value = sdate.AddMonths(1).AddDays(-1);
        //        }
        //        SqlDataReader reader = vSelectPNLDBCommandCRR.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            //ReconcilationMTLY stReconcilation = new ReconcilationMTLY();
        //            //DateTime sdate1 = sdate;
        //            //stReconcilation.Gross = Convert.ToDouble(reader.IsDBNull(1) ? 0 : reader.GetValue(1));// - stFTRDic[sdate1].PNL;
        //            //stReconcilation.DateFormat = sdate1;
        //            //stReconcilation.Month = sdate1.ToString("MMM") + " " + sdate1.Year;

        //            //// stReconcilation.Fee = Convert.ToDouble(reader.IsDBNull(1) ? 0 : reader.GetValue(1));
        //            //stReconcilation.Fee = reader.IsDBNull(2) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(2)), 2);
        //            //stReconcilation.MWs = reader.IsDBNull(2) ? 0 : Math.Round(Convert.ToDouble(reader.GetValue(3)), 2);
        //            //stReconcilation.FeePerMW = 0;
        //            ////stReconcilation.ISOMiscellaneousCharges = 0;
        //            //if (stCRRErcotDic.ContainsKey(sdate))
        //            //{
        //            //    // iso = stUTCErcotDic[sdate];
        //            //    stReconcilation.ISOMiscellaneousCharges = stCRRErcotDic[sdate];
        //            //    stReconcilation.MonthFeeISO = stReconcilation.ISOMiscellaneousCharges;
        //            //}
        //            //else
        //            //{
        //            //    stReconcilation.ISOMiscellaneousCharges = 0;
        //            //    stReconcilation.MonthFeeISO = stReconcilation.ISOMiscellaneousCharges;
        //            //}
        //            //stReconcilation.MonthGrossISO = 0;
        //            //stReconcilation.PNL = stReconcilation.Gross + stReconcilation.Fee + stReconcilation.ISOMiscellaneousCharges;
        //            //// stReconcilation.PNL = stReconcilation.Gross + stReconcilation.Fee;
        //            //runningtotal = stReconcilation.PNL;
        //            //stReconcilation.MonthlyRunningTotal1 = runningtotal;
        //            //stReconcilation.DAM = 0;
        //            //stReconcilation.RTM = 0;
        //            listReconcilation.Add(stReconcilation);
        //        }
        //        reader.Close();
        //        sdate = sdate.AddMonths(1);
        //    }
        //    return listReconcilation;
        //}
        public List<Performance> GetDailyCRR(DateTime startDate, DateTime endDate, int marketkey)
        {
            DateTime sdate = startDate;
            DateTime edate = endDate;
            List<Performance> listReconcilation = new List<Performance>();
            Dictionary<DateTime, double> stCRRErcotDic = new System.Collections.Generic.Dictionary<DateTime, double>();
            stLoadDBCommand();
            if (marketkey == 9)
            {
                stCRRErcotDic = GetMonthlyACLCRRDaily(startDate, endDate);

                if (vAccountingDBConnection.State == ConnectionState.Open)
                {
                    vAccountingDBConnection.Close();
                }
                vAccountingDBConnection.Open();
                vSelectPNLDailyDBCommandCRR.Connection = vAccountingDBConnection;
                vSelectPNLDailyDBCommandCRR.Parameters["@startdate"].Value = sdate;
                vSelectPNLDailyDBCommandCRR.Parameters["@enddate"].Value = endDate;
                SqlDataReader reader = vSelectPNLDailyDBCommandCRR.ExecuteReader();
                double? runningtotal = 0;
                int M = 1;
                double a = 0;
                while (reader.Read())
                {

                    Performance stReconcilation = new Performance();
                    stReconcilation.QuantName = reader.GetDateTime(3).ToString();
                    // stReconcilation.DateFormat = reader.GetDateTime(3);
                    stReconcilation.CompanyMW = Math.Round(Convert.ToDouble(reader.GetValue(4)), 3);
                    //stReconcilation.DailyFeePerMW= reader.IsDBNull(6) ? 0 : (Math.Round(Convert.ToDouble(reader.GetValue(6)), 2));
                    stReconcilation.CRRMw = reader.IsDBNull(2) ? 0 : (Math.Round(Convert.ToDouble(reader.GetValue(2)), 2));
                    List<Performance> removelist = new List<Performance>();
                    {
                        stReconcilation.CompanyFee = 0;
                    }
                    //DateTime comparedate = new DateTime(stReconcilation.DateFormat.Year, stReconcilation.DateFormat.Month, 01);
                    //if (stCRRErcotDic.ContainsKey(stReconcilation.DateFormat))
                    //{
                    //    // iso = stUTCErcotDic[sdate];
                    //    stReconcilation.PTPNet = stCRRErcotDic[stReconcilation.DateFormat];
                    //    stReconcilation.PTPGross = stReconcilation.PTPNet;
                    //}
                    //else
                    //{
                    //    stReconcilation.PTPNet = 0;

                    //    stReconcilation.PTPGross = stReconcilation.PTPNet;
                    //}
                    stReconcilation.CRRFee = Math.Round(Convert.ToDouble(reader.GetValue(1)), 2);// + isocharges;
                    stReconcilation.PTPFee = stReconcilation.CRRFee + stReconcilation.CRRMw + stReconcilation.PTPNet;
                    runningtotal = Math.Round(Convert.ToDouble(runningtotal), 2) + stReconcilation.PTPFee;
                    //if (stReconcilation.DateFormat <= dtmaxdate)
                    {
                        // stReconcilation.DailyPNL = stReconcilation.DailyGross + stReconcilation.DailyFee + stReconcilation.ISOMiscellaneousCharges;
                        // runningtotal = Math.Round(Convert.ToDouble(runningtotal), 2) + stReconcilation.DailyPNL;
                        stReconcilation.PTPMW = Math.Round(Convert.ToDouble(runningtotal), 2);
                        stReconcilation.CompanyGross = 0;
                        stReconcilation.PTPDollarsCleared = 0;
                    }
                    //stReconcilation.DailyFee = Math.Round(Convert.ToDouble(reader.GetValue(1)), 2);
                    listReconcilation.Add(stReconcilation);
                }
                reader.Close();
            }


            return listReconcilation;
        }

        public DateTime GetMaxDate(string product)
        {
            DateTime keyList = new DateTime();

            stLoadDBCommand();
            if (vAccountingDBConnection.State == ConnectionState.Open)
            {
                vAccountingDBConnection.Close();
            }
            vAccountingDBConnection.Open();
            vSelectMaxDate.Parameters["@Product"].Value = product;
            vSelectMaxDate.Connection = vAccountingDBConnection;
            SqlDataReader reader2 = vSelectMaxDate.ExecuteReader();
            while (reader2.Read())
            {
                keyList = reader2.GetDateTime(0);
            }
            reader2.Close();
            vAccountingDBConnection.Close();
            return keyList;
        }
    }

}
