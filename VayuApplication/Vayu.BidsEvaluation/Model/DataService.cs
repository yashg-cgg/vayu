using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.BidsEvaluation.ViewModels;
using Vayu.CommonAccessLibrary;

namespace Vayu.BidsEvaluation.Model
{
    public class DataService : IDataService
    {

        SqlConnection vAccountingDBConnection;
        SqlCommand vSelectERCOTLDailyDBCommand;
        SqlCommand vSelectERCOTLMonthlyDBCommand;
        private void stLoadDBCommand()
        {
            //vAccountingDBConnection = new SqlConnection("Data Source = ; Initial Catalog = VayuAccounting; Persist Security Info = True; User Id = VayuAcc; password = Vayu@2023!; Connect Timeout = 100000; MultipleActiveResultSets = True");
            vAccountingDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            // vAccountingDBConnection.Open();

            //ERCOT

            vSelectERCOTLDailyDBCommand = new SqlCommand();
            vSelectERCOTLDailyDBCommand.CommandText = " select PortfolioName,SubmittedCount,ClearedCount,TotalUnclearedBids,RequestedMW,ClearedMW,TotalUnclearedMW,PercentageMWCleared,MarketDate   "
                                + " from ercotbids "
                                + " where marketdate >= @startdate and marketdate<= @enddate order by PercentageMWCleared desc";
            vSelectERCOTLDailyDBCommand.Parameters.AddWithValue("@startdate", "marketdate");
            vSelectERCOTLDailyDBCommand.Parameters.AddWithValue("@enddate", "marketdate");

            vSelectERCOTLMonthlyDBCommand = new SqlCommand();
            vSelectERCOTLMonthlyDBCommand.CommandText = "select PortfolioName,sum(SubmittedCount) as SubmittedCount,sum(ClearedCount) as ClearedCount,sum(TotalUnclearedBids) as TotalUnclearedBids, "
                            + " sum(RequestedMW) as RequestedMW,Sum(ClearedMW) as ClearedMW,Sum(TotalUnclearedMW) as TotalUnclearedMW, "
                            + " avg(PercentageMWCleared) as PercentageMWCleared,Month(MarketDate) as Month, Year(MarketDate) as Year "
                            + " from ercotbids "
                            + " where marketdate >= @startdate and marketdate<= @enddate "
                            + " group by Month(MarketDate),Year(MarketDate),PortfolioName "
                            + " order by Month(MarketDate),Year(MarketDate),PortfolioName ";
            vSelectERCOTLMonthlyDBCommand.Parameters.AddWithValue("@startdate", "marketdate");
            vSelectERCOTLMonthlyDBCommand.Parameters.AddWithValue("@enddate", "marketdate");


        }

        public List<BidsEvaluationDLY> GetBidsListDailyMonthly(DateTime startDate, DateTime endDate, int marketkey)
        {
            // List<BidsEvaluationMTLY> stPNLMonthlyList = GetReconcilationList(startDate, endDate, marketkey);
            DateTime sdate = startDate;
            DateTime edate = endDate;
            List<BidsEvaluationDLY> listReconcilation = new List<BidsEvaluationDLY>();
            Dictionary<DateTime, double> stUTCErcotDic = new System.Collections.Generic.Dictionary<DateTime, double>();
            stLoadDBCommand();
            if (marketkey == 9)
            {
                if (vAccountingDBConnection.State == ConnectionState.Open)
                {
                    vAccountingDBConnection.Close();
                }
                vAccountingDBConnection.Open();

                vSelectERCOTLMonthlyDBCommand.Parameters["@startdate"].Value = new DateTime(sdate.Year, sdate.Month, 01);
                vSelectERCOTLMonthlyDBCommand.Connection = vAccountingDBConnection;
                vSelectERCOTLMonthlyDBCommand.Parameters["@enddate"].Value = edate;
                SqlDataReader reader1 = vSelectERCOTLMonthlyDBCommand.ExecuteReader();
                double? runningtotal = 0;
                while (reader1.Read())
                {
                    try
                    {
                        BidsEvaluationDLY stReconcilation = new BidsEvaluationDLY();
                        stReconcilation.PortfolioName = reader1.GetString(0).ToString();
                        stReconcilation.SubmittedCount = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(1)) ? 0 : reader1.GetValue(1)), 2);
                        stReconcilation.ClearedCount = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(2)) ? 0 : reader1.GetValue(2)), 2);
                        stReconcilation.TotalUnclearedBids = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(3)) ? 0 : reader1.GetValue(3)), 2);// + iso;
                        stReconcilation.RequestedMW = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(4)) ? 0 : reader1.GetValue(4)), 2);// + iso;
                        stReconcilation.ClearedMW = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(5)) ? 0 : reader1.GetValue(5)), 2);
                        stReconcilation.TotalUnclearedMW = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(6)) ? 0 : reader1.GetValue(6)), 2);
                        stReconcilation.PercentageMWCleared = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(7)) ? 0 : reader1.GetValue(7)), 2);
                        stReconcilation.MarketDate = new DateTime(Convert.ToInt32(reader1.GetValue(9)), Convert.ToInt32(reader1.GetValue(8)), 01);
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


        public List<BidsEvaluationDLY> GetBidsListDaily(DateTime startDate, DateTime endDate, int marketkey)
        {
            // List<BidsEvaluationMTLY> stPNLMonthlyList = GetReconcilationList(startDate, endDate, marketkey);
            DateTime sdate = startDate;
            DateTime edate = endDate;
            List<BidsEvaluationDLY> listReconcilation = new List<BidsEvaluationDLY>();
            Dictionary<DateTime, double> stUTCErcotDic = new System.Collections.Generic.Dictionary<DateTime, double>();
            stLoadDBCommand();
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
                double? runningtotal = 0;
                while (reader1.Read())
                {
                    try
                    {//select PortfolioName,SubmittedCount,ClearedCount,TotalUnclearedBids,RequestedMW,ClearedMW,TotalUnclearedMW,PercentageMWCleared,MarketDate   "
                        BidsEvaluationDLY stReconcilation = new BidsEvaluationDLY();
                        stReconcilation.PortfolioName = reader1.GetString(0).ToString();
                        stReconcilation.SubmittedCount = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(1)) ? 0 : reader1.GetValue(1)), 2);
                        stReconcilation.ClearedCount = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(2)) ? 0 : reader1.GetValue(2)), 2);
                        stReconcilation.TotalUnclearedBids = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(3)) ? 0 : reader1.GetValue(3)), 2);// + iso;
                        stReconcilation.RequestedMW = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(4)) ? 0 : reader1.GetValue(4)), 2);// + iso;
                        stReconcilation.ClearedMW = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(5)) ? 0 : reader1.GetValue(5)), 2);
                        stReconcilation.TotalUnclearedMW = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(6)) ? 0 : reader1.GetValue(6)), 2);
                        stReconcilation.PercentageMWCleared = Math.Round(Convert.ToDouble(Convert.IsDBNull(reader1.GetValue(7)) ? 0 : reader1.GetValue(7)), 2);
                        stReconcilation.MarketDate = reader1.GetDateTime(8);
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

    }
}
