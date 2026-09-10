using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.ErcotACLSummaryReport.ViewModel;

namespace Vayu.ErcotACLSummaryReport.Model
{
    public class DataService : IDataService
    {


        SqlConnection stVayuAccountingAppDBConnection;
        SqlCommand stAccountDataCommand;
        SqlCommand stAccountCRRDataCommand;
        SqlCommand stAccountDataMulDayCommand;
        SqlCommand stAccountCRRDataMulDayCommand;
        SqlCommand stAccountdamDataMulDayCommand;
        SqlCommand stAccountDAMDataCommand;
        SqlCommand stAccountclaclDataMulDayCommand;
        SqlCommand stAccountcltranDataCommand;
        private void stLoadDBCommand()
        {
            try
            {

                //stVayuAccountingAppDBConnection = new SqlConnection("Data Source = ; Initial Catalog = VayuAccounting; Persist Security Info = True; User Id = VayuAcc; password = Vayu@2023!; Connect Timeout = 100000; MultipleActiveResultSets = True");
                //stVayuAccountingAppDBConnection = new SqlConnection("Data Source = ; Initial Catalog = Vayu; Persist Security Info = True; User Id = ; password = ; Connect Timeout = 100000; MultipleActiveResultSets=True");
                stVayuAccountingAppDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
                // stVayuAccountingAppDBConnection.Open();

                stAccountDataCommand = new SqlCommand();
                stAccountDataCommand.CommandText = "select BusinessDate, Cash, TotalFincSc, TPES, IndependentA,RmndCl, LetterOfCdt  "
                           + " , SuretyBond,  ApdCRRBTrades, CRRLockedACL "
                           + " from ClSummary where BusinessDate = @BusinessDate order by BusinessDate ";
                stAccountDataCommand.Parameters.AddWithValue("@BusinessDate", "BusinessDate");
                stAccountDataCommand.Connection = stVayuAccountingAppDBConnection;

                stAccountCRRDataCommand = new SqlCommand();
                stAccountCRRDataCommand.CommandText = "select BusinessDate,TPES,IndependentA,TPESasPerOfCl,CRRACL,AdjustedCRRACL,ACLSentToCRR,ApdBT,ACLLkdforCRR,TPESinExsOfCl,OutstandingClRqt,AdditionalClRequired " +
                                                    " from TPESCRRACLSummary where BusinessDate = @BusinessDate order by BusinessDate desc";
                stAccountCRRDataCommand.Parameters.AddWithValue("@BusinessDate", "BusinessDate");
                stAccountCRRDataCommand.Connection = stVayuAccountingAppDBConnection;

                stAccountCRRDataMulDayCommand = new SqlCommand();
                stAccountCRRDataMulDayCommand.CommandText = "select BusinessDate,TPES,IndependentA,TPESasPerOfCl,CRRACL,AdjustedCRRACL,ACLSentToCRR,ApdBT,ACLLkdforCRR, TPESinExsOfCl,OutstandingClRqt,AdditionalClRequired " +
                             " from TPESCRRACLSummary where  BusinessDate >= @FromSelectedDate and "
                            + " BusinessDate <= @FromSelectedEndDate order by BusinessDate desc";
                stAccountCRRDataMulDayCommand.Parameters.AddWithValue("@FromSelectedDate", "Date");
                stAccountCRRDataMulDayCommand.Parameters.AddWithValue("@FromSelectedEndDate", "Date");
                stAccountCRRDataMulDayCommand.Connection = stVayuAccountingAppDBConnection;
                stVayuAccountingAppDBConnection.Close();

                stAccountDataMulDayCommand = new SqlCommand();
                stAccountDataMulDayCommand.CommandText = "select BusinessDate, Cash, TotalFincSc, TPES, IndependentA,RmndCl, LetterOfCdt  "
                           + " , SuretyBond, ApdCRRBTrades, CRRLockedACL "
                           + " from ClSummary where  BusinessDate >= @FromSelectedDate and "
                            + " BusinessDate <= @FromSelectedEndDate order by BusinessDate desc";
                stAccountDataMulDayCommand.Parameters.AddWithValue("@FromSelectedDate", "Date");
                stAccountDataMulDayCommand.Parameters.AddWithValue("@FromSelectedEndDate", "Date");
                stAccountDataMulDayCommand.Connection = stVayuAccountingAppDBConnection;
                stVayuAccountingAppDBConnection.Close();


                stAccountdamDataMulDayCommand = new SqlCommand();
                stAccountdamDataMulDayCommand.CommandText = " select  BusinessDate,TPEA,RmndCl,TPEAasPerOfAnyCdtLimit,DAMACL,AdjustedDAMACL,ACLSenttoDAM,TPEAinExsOfRmndCdt,OutstandingAnyClRqt,AdditionalAnyClRequired  " +
                                            " from TPEADAMACLSummary where  BusinessDate >= @FromSelectedDate and "
                            + " BusinessDate <= @FromSelectedEndDate order by BusinessDate desc";
                stAccountdamDataMulDayCommand.Parameters.AddWithValue("@FromSelectedDate", "Date");
                stAccountdamDataMulDayCommand.Parameters.AddWithValue("@FromSelectedEndDate", "Date");
                stAccountdamDataMulDayCommand.Connection = stVayuAccountingAppDBConnection;
                stVayuAccountingAppDBConnection.Close();

                stAccountDAMDataCommand = new SqlCommand();
                stAccountDAMDataCommand.CommandText = "select  BusinessDate,TPEA,RmndCl,TPEAasPerOfAnyCdtLimit,DAMACL,AdjustedDAMACL,ACLSenttoDAM,TPEAinExsOfRmndCdt,OutstandingAnyClRqt,AdditionalAnyClRequired  "
                       + " from TPEADAMACLSummary where  BusinessDate = @BusinessDate  order by BusinessDate desc";
                stAccountDAMDataCommand.Parameters.AddWithValue("@BusinessDate", "BusinessDate");
                stAccountDAMDataCommand.Connection = stVayuAccountingAppDBConnection;
                stVayuAccountingAppDBConnection.Close();

                stAccountclaclDataMulDayCommand = new SqlCommand();
                stAccountclaclDataMulDayCommand.CommandText = " select BusinessDate,ClType,BeginningBalance,TransactionDate,TransactionAmount,EndingBalance,Description from ClTransactionsSummary where  BusinessDate >= @FromSelectedDate and "
                             + " BusinessDate <= @FromSelectedEndDate order by BusinessDate desc";
                stAccountclaclDataMulDayCommand.Parameters.AddWithValue("@FromSelectedDate", "Date");
                stAccountclaclDataMulDayCommand.Parameters.AddWithValue("@FromSelectedEndDate", "Date");
                stAccountclaclDataMulDayCommand.Connection = stVayuAccountingAppDBConnection;
                stVayuAccountingAppDBConnection.Close();

                stAccountcltranDataCommand = new SqlCommand();
                stAccountcltranDataCommand.CommandText = "select BusinessDate,ClType,BeginningBalance,TransactionDate,TransactionAmount,EndingBalance,Description from ClTransactionsSummary where  BusinessDate = @BusinessDate  order by BusinessDate desc";
                stAccountcltranDataCommand.Parameters.AddWithValue("@BusinessDate", "BusinessDate");
                stAccountcltranDataCommand.Connection = stVayuAccountingAppDBConnection;
                stVayuAccountingAppDBConnection.Close();

            }
            catch (Exception)
            {
            }
        }
        public ObservableCollection<ACLData> GetAllData(DateTime FromSelectedDate)
        {
            ObservableCollection<ACLData> ACLDataDataList = new ObservableCollection<ACLData>();
            DateTime sdate = FromSelectedDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountDataCommand.Parameters["@BusinessDate"].Value = sdate;

                SqlDataReader reader = stAccountDataCommand.ExecuteReader();
                int i = 1;
                while (reader.Read())
                {
                    ACLData stACLData = new ACLData();
                    stACLData.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLData.Cash = reader.IsDBNull(1) ? Double.NaN : Convert.ToDouble(reader.GetValue(1));
                    stACLData.TotalSecuredCollateral = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));
                    stACLData.TPES = reader.IsDBNull(3) ? Double.NaN : Convert.ToDouble(reader.GetValue(3));
                    stACLData.IndependentAmount = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLData.RemainderCollateral = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLData.LetterOfCredit = reader.IsDBNull(6) ? Double.NaN : Convert.ToDouble(reader.GetValue(6));
                    stACLData.SuretyBond = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    // stACLData.Guarantee = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    //stACLData.UnsecuredCreditLimit = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    stACLData.ApprovedCRRBilateralTrades = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLData.CRRLockedACL = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    ACLDataDataList.Add(stACLData);
                }
            }
            catch (Exception)
            {
            }

            return ACLDataDataList;
        }
        public ObservableCollection<ACLData> GetMultipleDaysData(DateTime FromSelectedDate, DateTime FromSelectedEndDate)
        {
            ObservableCollection<ACLData> ACLDataDataList = new ObservableCollection<ACLData>();
            DateTime sDate = FromSelectedDate;
            DateTime eDate = FromSelectedEndDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountDataMulDayCommand.Parameters["@FromSelectedDate"].Value = sDate;
                stAccountDataMulDayCommand.Parameters["@FromSelectedEndDate"].Value = eDate;
                SqlDataReader reader = stAccountDataMulDayCommand.ExecuteReader();
                while (reader.Read())
                {
                    ACLData stACLData = new ACLData();
                    stACLData.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLData.Cash = reader.IsDBNull(1) ? Double.NaN : Convert.ToDouble(reader.GetValue(1));
                    stACLData.TotalSecuredCollateral = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));
                    stACLData.TPES = reader.IsDBNull(3) ? Double.NaN : Convert.ToDouble(reader.GetValue(3));
                    stACLData.IndependentAmount = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLData.RemainderCollateral = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLData.LetterOfCredit = reader.IsDBNull(6) ? Double.NaN : Convert.ToDouble(reader.GetValue(6));
                    stACLData.SuretyBond = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    // stACLData.Guarantee = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    // stACLData.UnsecuredCreditLimit = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    stACLData.ApprovedCRRBilateralTrades = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLData.CRRLockedACL = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    ACLDataDataList.Add(stACLData);
                }
            }
            catch (Exception)
            {
            }
            return ACLDataDataList;
        }
        public ObservableCollection<CRRACLData> GetCRRAllData(DateTime FromSelectedDate)
        {
            ObservableCollection<CRRACLData> crrDataDataList = new ObservableCollection<CRRACLData>();
            DateTime sdate = FromSelectedDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountCRRDataCommand.Parameters["@BusinessDate"].Value = sdate;
                SqlDataReader reader = stAccountCRRDataCommand.ExecuteReader();
                while (reader.Read())
                {
                    CRRACLData stACLcrrData = new CRRACLData();
                    stACLcrrData.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLcrrData.TPES = reader.IsDBNull(1) ? Double.NaN : Convert.ToDouble(reader.GetValue(1));
                    stACLcrrData.IndependentAmount = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));

                    stACLcrrData.TPESAsPerOfSecuredCollateral = reader.IsDBNull(3) ? Double.NaN : Convert.ToDouble(reader.GetValue(3));
                    stACLcrrData.CRRACL = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLcrrData.AdjustedCRRACL = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLcrrData.ACLSentToCRR = reader.IsDBNull(6) ? Double.NaN : Convert.ToDouble(reader.GetValue(6));

                    stACLcrrData.ApprovedBilateralTrades = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    stACLcrrData.ACLLockedForCRR = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLcrrData.TPESInExcessOfSecuredCollateral = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    stACLcrrData.OutstandingSecuredCollateralRequest = reader.IsDBNull(10) ? Double.NaN : Convert.ToDouble(reader.GetValue(10));
                    stACLcrrData.AdditionalSecuredCollateralRequired = reader.IsDBNull(11) ? Double.NaN : Convert.ToDouble(reader.GetValue(11));
                    crrDataDataList.Add(stACLcrrData);
                }
            }
            catch (Exception)
            {
            }
            return crrDataDataList;
        }
        public ObservableCollection<CRRACLData> GetCRRMultipleDaysData(DateTime FromSelectedDate, DateTime FromSelectedEndDate)
        {
            ObservableCollection<CRRACLData> crrDataDataList = new ObservableCollection<CRRACLData>();
            DateTime sDate = FromSelectedDate;
            DateTime eDate = FromSelectedEndDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountCRRDataMulDayCommand.Parameters["@FromSelectedDate"].Value = sDate;
                stAccountCRRDataMulDayCommand.Parameters["@FromSelectedEndDate"].Value = eDate;
                SqlDataReader reader = stAccountCRRDataMulDayCommand.ExecuteReader();
                while (reader.Read())
                {
                    CRRACLData stACLcrrData = new CRRACLData();
                    stACLcrrData.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLcrrData.TPES = reader.IsDBNull(1) ? Double.NaN : Convert.ToDouble(reader.GetValue(1));
                    stACLcrrData.IndependentAmount = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));

                    stACLcrrData.TPESAsPerOfSecuredCollateral = reader.IsDBNull(3) ? Double.NaN : Convert.ToDouble(reader.GetValue(3));
                    stACLcrrData.CRRACL = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLcrrData.AdjustedCRRACL = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLcrrData.ACLSentToCRR = reader.IsDBNull(6) ? Double.NaN : Convert.ToDouble(reader.GetValue(6));

                    stACLcrrData.ApprovedBilateralTrades = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    stACLcrrData.ACLLockedForCRR = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLcrrData.TPESInExcessOfSecuredCollateral = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    stACLcrrData.OutstandingSecuredCollateralRequest = reader.IsDBNull(10) ? Double.NaN : Convert.ToDouble(reader.GetValue(10));
                    stACLcrrData.AdditionalSecuredCollateralRequired = reader.IsDBNull(11) ? Double.NaN : Convert.ToDouble(reader.GetValue(11));
                    crrDataDataList.Add(stACLcrrData);
                }
            }
            catch (Exception)
            {
            }
            return crrDataDataList;
        }

        public ObservableCollection<DAMACLData> GetDAMAllData(DateTime FromSelectedDate)
        {
            ObservableCollection<DAMACLData> damDataDataList = new ObservableCollection<DAMACLData>();
            DateTime sdate = FromSelectedDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountDAMDataCommand.Parameters["@BusinessDate"].Value = sdate;
                SqlDataReader reader = stAccountDAMDataCommand.ExecuteReader();
                while (reader.Read())
                {
                    DAMACLData stACLcrrData = new DAMACLData();
                    stACLcrrData.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLcrrData.TPEA = reader.IsDBNull(1) ? Double.NaN : Convert.ToDouble(reader.GetValue(1));
                    stACLcrrData.RemainderCollateral = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));

                    stACLcrrData.TPEAAsPerOfAnyCollateralAndUnsecuredCreditLimit = reader.IsDBNull(3) ? Double.NaN : Convert.ToDouble(reader.GetValue(3));
                    stACLcrrData.DAMACL = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLcrrData.AdjustedDAMACL = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLcrrData.ACLSentToDAM = reader.IsDBNull(6) ? Double.NaN : Convert.ToDouble(reader.GetValue(6));

                    //stACLcrrData.Guarantee = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    //stACLcrrData.UnsecuredCreditLimit = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLcrrData.TPEAInExcessOfRemainderCollateralAndUnsecuredCredit = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    stACLcrrData.OutstandingAnyCollateralRequest = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLcrrData.AdditionalAnyCollateralRequired = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    damDataDataList.Add(stACLcrrData);
                }
            }
            catch (Exception ex)
            {
            }
            return damDataDataList;
        }
        public ObservableCollection<DAMACLData> GetDAMMultipleDaysData(DateTime FromSelectedDate, DateTime FromSelectedEndDate)
        {
            ObservableCollection<DAMACLData> damDataDataList = new ObservableCollection<DAMACLData>();
            DateTime sDate = FromSelectedDate;
            DateTime eDate = FromSelectedEndDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountdamDataMulDayCommand.Parameters["@FromSelectedDate"].Value = sDate;
                stAccountdamDataMulDayCommand.Parameters["@FromSelectedEndDate"].Value = eDate;
                SqlDataReader reader = stAccountdamDataMulDayCommand.ExecuteReader();
                while (reader.Read())
                {
                    DAMACLData stACLcrrData = new DAMACLData();
                    stACLcrrData.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLcrrData.TPEA = reader.IsDBNull(1) ? Double.NaN : Convert.ToDouble(reader.GetValue(1));
                    stACLcrrData.RemainderCollateral = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));

                    stACLcrrData.TPEAAsPerOfAnyCollateralAndUnsecuredCreditLimit = reader.IsDBNull(3) ? Double.NaN : Convert.ToDouble(reader.GetValue(3));
                    stACLcrrData.DAMACL = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLcrrData.AdjustedDAMACL = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLcrrData.ACLSentToDAM = reader.IsDBNull(6) ? Double.NaN : Convert.ToDouble(reader.GetValue(6));

                    // stACLcrrData.Guarantee = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    //stACLcrrData.UnsecuredCreditLimit = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLcrrData.TPEAInExcessOfRemainderCollateralAndUnsecuredCredit = reader.IsDBNull(7) ? Double.NaN : Convert.ToDouble(reader.GetValue(7));
                    stACLcrrData.OutstandingAnyCollateralRequest = reader.IsDBNull(8) ? Double.NaN : Convert.ToDouble(reader.GetValue(8));
                    stACLcrrData.AdditionalAnyCollateralRequired = reader.IsDBNull(9) ? Double.NaN : Convert.ToDouble(reader.GetValue(9));
                    damDataDataList.Add(stACLcrrData);
                }
            }
            catch (Exception ex)
            {
            }
            return damDataDataList;
        }


        public ObservableCollection<ClTranACLData> GetClTRanAllData(DateTime FromSelectedDate)
        {
            ObservableCollection<ClTranACLData> clDataDataList = new ObservableCollection<ClTranACLData>();
            DateTime sdate = FromSelectedDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountcltranDataCommand.Parameters["@BusinessDate"].Value = sdate;
                SqlDataReader reader = stAccountcltranDataCommand.ExecuteReader();
                while (reader.Read())
                {
                    ClTranACLData stACLclata = new ClTranACLData();
                    stACLclata.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLclata.CollateralType = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    stACLclata.BeginningBalance = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));
                    stACLclata.TransactionDate = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    stACLclata.TransactionAmount = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLclata.EndingBalance = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLclata.Description = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    clDataDataList.Add(stACLclata);
                }
            }
            catch (Exception ex)
            {
            }
            return clDataDataList;
        }
        public ObservableCollection<ClTranACLData> GetClTranMultipleDaysData(DateTime FromSelectedDate, DateTime FromSelectedEndDate)
        {
            ObservableCollection<ClTranACLData> clDataDataList = new ObservableCollection<ClTranACLData>();
            DateTime sDate = FromSelectedDate;
            DateTime eDate = FromSelectedEndDate;
            try
            {
                stLoadDBCommand();
                if (stVayuAccountingAppDBConnection.State == System.Data.ConnectionState.Open)
                {
                    stVayuAccountingAppDBConnection.Close();
                }
                stVayuAccountingAppDBConnection.Open();
                stAccountclaclDataMulDayCommand.Parameters["@FromSelectedDate"].Value = sDate;
                stAccountclaclDataMulDayCommand.Parameters["@FromSelectedEndDate"].Value = eDate;
                SqlDataReader reader = stAccountclaclDataMulDayCommand.ExecuteReader();
                while (reader.Read())
                {
                    ClTranACLData stACLclData = new ClTranACLData();
                    stACLclData.BusinessDate = Convert.ToDateTime(reader.GetValue(0));
                    stACLclData.CollateralType = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    stACLclData.BeginningBalance = reader.IsDBNull(2) ? Double.NaN : Convert.ToDouble(reader.GetValue(2));
                    stACLclData.TransactionDate = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    stACLclData.TransactionAmount = reader.IsDBNull(4) ? Double.NaN : Convert.ToDouble(reader.GetValue(4));
                    stACLclData.EndingBalance = reader.IsDBNull(5) ? Double.NaN : Convert.ToDouble(reader.GetValue(5));
                    stACLclData.Description = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    clDataDataList.Add(stACLclData);
                }
            }
            catch (Exception ex)
            {
            }
            return clDataDataList;
        }
    }
}