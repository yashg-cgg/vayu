using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;

namespace Vayu.Admin.Model
{
    public class DataService : IDataService
    {
        #region Declaration

        /// <summary>
        /// The Vayu DB connection
        /// </summary>
        SqlConnection VayuConnection;
        /// <summary>
        /// The command get all portfolios
        /// </summary>
        SqlCommand cmdGetAllPortfolios;
        SqlCommand cmdGetSubmittedPortfolios;
        SqlCommand cmdGetExpectedPortfolios;
        /// <summary>
        /// The command get portfolios for gojira
        /// </summary>
        //SqlCommand cmdGetPortfoliosForgojira;
        /// <summary>
        /// The command get admin data
        /// </summary>
        SqlCommand cmdGetAdminData;
        /// <summary>
        /// The command get admin data for gojira
        /// </summary>
        SqlCommand cmdGetAdminDataForgojira;
        /// <summary>
        /// The command portfolio exist
        /// </summary>
        SqlCommand cmdPortfolioExist;
        /// <summary>
        /// The command insert factor
        /// </summary>
        SqlCommand cmdInsertFactor;
        /// <summary>
        /// The command delete factor
        /// </summary>
        SqlCommand cmdDeleteFactor;
        /// <summary>
        /// The command get portfolio account
        /// </summary>
        SqlCommand cmdGetPortfolioAccount;

        /// <summary>
        /// The portfolio list
        /// </summary>
        List<Portfolio> PortfolioList;
        /// <summary>
        /// The portfolio object
        /// </summary>
        Portfolio objPortfolio;

        #endregion

        /// <summary>
        /// Loads the database commands.
        /// </summary>
        void LoadDB()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            cmdGetAllPortfolios = VayuConnection.CreateCommand();
            cmdGetAllPortfolios.CommandText = "SELECT DISTINCT PORTFOLIO_ID, STRIP FROM PORTFOLIO WHERE HUB = @Market AND PRODUCT = @Products AND ACTIVE = 'Y' ORDER BY STRIP";
            cmdGetAllPortfolios.Parameters.AddWithValue("@Market", "Market");
            cmdGetAllPortfolios.Parameters.AddWithValue("@Products", "Products");
            //// cmdGetSubmittedPortfolios  cmdGetExpectedPortfolios
            ///
            cmdGetSubmittedPortfolios = VayuConnection.CreateCommand();
            //cmdGetSubmittedPortfolios.CommandText = "select distinct PortfolioKey, STRIP from SubmittedBidsErcotPTP join PORTFOLIO on PortfolioKey=PORTFOLIO_ID where EndMarketDateTime>= @StartDate and EndMarketDateTime< @EndDate ";
            cmdGetSubmittedPortfolios.CommandText = "select SUM(CAST(RequestedMW AS decimal(18,1))) AS reqestedmw, count(BidID) from SubmittedBidsErcotPTP_test where " + "EndMarketDateTime>@StartDate and EndMarketDateTime<=@EndDate ";
            cmdGetSubmittedPortfolios.Parameters.AddWithValue("@StartDate", "StartDate");
            cmdGetSubmittedPortfolios.Parameters.AddWithValue("@EndDate", "EndDate");
            ///

            cmdGetExpectedPortfolios = VayuConnection.CreateCommand();
            cmdGetExpectedPortfolios.CommandText = "select Sum(RequestedMW),count(ScheduleID) from ErcotPTPBids where " + "EndMarketDateTime>@StartDate and EndMarketDateTime<=@EndDate and PortfolioKey in(3022) ";
            cmdGetExpectedPortfolios.Parameters.AddWithValue("@StartDate", "StartDate");
            cmdGetExpectedPortfolios.Parameters.AddWithValue("@EndDate", "EndDate");
            ///
            cmdGetAdminData = VayuConnection.CreateCommand();
            cmdGetAdminData.CommandText = "SELECT a.ACCOUNT, b.STRIP, MULTIPLIER FROM FirmAccount a "
                                            + "INNER JOIN PORTFOLIO b ON b.PORTFOLIO_ID = a.ACCOUNT WHERE b.HUB=@Market AND b.PRODUCT= @Product"
                                            + " ORDER BY STRIP";
            cmdGetAdminData.Parameters.AddWithValue("@Market", "Market");
            cmdGetAdminData.Parameters.AddWithValue("@Product", "Product");
            //
            cmdGetAdminDataForgojira = VayuConnection.CreateCommand();
            cmdGetAdminDataForgojira.CommandText = "SELECT a.ACCOUNT, b.STRIP, MULTIPLIER FROM FirmAccount a "
                                            + "INNER JOIN PORTFOLIO b ON b.PORTFOLIO_ID = a.ACCOUNT "
                                            + "WHERE  HUB=@Market AND b.PRODUCT=@Product AND PORTFOLIO_ID IN (SELECT PORTFOLIOKEY FROM END_USER_MARKET_PORTFOLIO WHERE END_USER_KEY = 21) "
                                            + "ORDER BY STRIP";
            cmdGetAdminDataForgojira.Parameters.AddWithValue("@Market", "Market");
            cmdGetAdminDataForgojira.Parameters.AddWithValue("@Product", "Product");
            //
            cmdPortfolioExist = VayuConnection.CreateCommand();
            cmdPortfolioExist.CommandText = "SELECT * FROM FirmAccount WHERE ACCOUNT = @ACCOUNT";
            cmdPortfolioExist.Parameters.AddWithValue("@ACCOUNT", "ACCOUNT");
            //
            cmdInsertFactor = VayuConnection.CreateCommand();
            cmdInsertFactor.CommandText = "INSERT INTO FirmAccount(ACCOUNT,MULTIPLIER,Account2,marketkey,product) VALUES (@ACCOUNT, @MULTIPLIER, @Account2,@Market,@Product)";
            cmdInsertFactor.Parameters.AddWithValue("@ACCOUNT", "ACCOUNT");
            cmdInsertFactor.Parameters.AddWithValue("@MULTIPLIER", "MULTIPLIER");
            cmdInsertFactor.Parameters.AddWithValue("@Account2", "Account2");
            cmdInsertFactor.Parameters.AddWithValue("@Market", "Market");
            cmdInsertFactor.Parameters.AddWithValue("@Product", "Product");
            //
            cmdDeleteFactor = VayuConnection.CreateCommand();
            cmdDeleteFactor.CommandText = "DELETE FROM FirmAccount "
                                            + "WHERE ACCOUNT = @ACCOUNT AND MULTIPLIER = @MULTIPLIER AND Marketkey=@Market AND Product=@Product";
            cmdDeleteFactor.Parameters.AddWithValue("@ACCOUNT", "ACCOUNT");
            cmdDeleteFactor.Parameters.AddWithValue("@MULTIPLIER", "MULTIPLIER");
            cmdDeleteFactor.Parameters.AddWithValue("@Market", "Market");
            cmdDeleteFactor.Parameters.AddWithValue("@Product", "Product");
            //
            cmdGetPortfolioAccount = VayuConnection.CreateCommand();
            cmdGetPortfolioAccount.CommandText = "SELECT ACCOUNT FROM PORTFOLIO WHERE PORTFOLIO_ID = @PORTFOLIO_ID";
            cmdGetPortfolioAccount.Parameters.AddWithValue("@PORTFOLIO_ID", "PORTFOLIO_ID");
            //
        }

        #region Public Methods

        /// <summary>
        /// Gets all portfolios.
        /// </summary>
        /// <returns></returns>
        public List<Portfolio> GetAllPortfolios(string Market, string Products)
        {
            LoadDB();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            PortfolioList = new List<Portfolio>();
            cmdGetAllPortfolios.Parameters["@Market"].Value = Market;
            cmdGetAllPortfolios.Parameters["@Products"].Value = Products;
            SqlDataReader reader = cmdGetAllPortfolios.ExecuteReader();
            while (reader.Read())
            {
                objPortfolio = new Portfolio();
                objPortfolio.ID = int.Parse(reader.GetValue(0).ToString());
                objPortfolio.Name = reader.GetString(1);
                PortfolioList.Add(objPortfolio);
                objPortfolio = null;
            }
            reader.Close();
            VayuConnection.Close();
            return PortfolioList;
        }

        public List<SubmittedData> GetSubmittedPortfolios(DateTime date)
        {
            LoadDB();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            List<SubmittedData> SPortfolioList = new List<SubmittedData>();
            SubmittedData obj = new SubmittedData();
            cmdGetSubmittedPortfolios.Parameters["@StartDate"].Value = date;
            cmdGetSubmittedPortfolios.Parameters["@EndDate"].Value = date.AddDays(1);
            SqlDataReader reader = cmdGetSubmittedPortfolios.ExecuteReader();
            while(reader.Read())
            {
                obj.SubmittedMW = Convert.ToDouble(reader.GetValue(0));
                obj.SubmittedCount = Convert.ToInt32(reader.GetValue(1));
            }
            reader.Close();
            cmdGetExpectedPortfolios.Parameters["@StartDate"].Value = date;
            cmdGetExpectedPortfolios.Parameters["@EndDate"].Value = date.AddDays(1);
            SqlDataReader reader1 = cmdGetExpectedPortfolios.ExecuteReader();
            while (reader1.Read())
            {
                obj.Risk1MW = Convert.ToDouble(reader1.GetValue(0));
                obj.ExpectedCount = Convert.ToInt32(reader1.GetValue(1));
            }
            if(obj.Risk1MW==obj.SubmittedMW && obj.SubmittedCount==obj.ExpectedCount)
            {
                obj.Status = "Success";
            }
            else if(obj.Risk1MW == obj.SubmittedMW)
            {
                obj.Status = "Error- Count Not Matching";
            }
            else if (obj.SubmittedCount == obj.ExpectedCount)
            {
                obj.Status = "Error- MW Not Matching";
            }
            else
            {
                obj.Status = "Error- MW And Count Not Matching";
            }
            SPortfolioList.Add(obj);
            reader1.Close();
            VayuConnection.Close();
            return SPortfolioList;
        }

        /// <summary>
        /// Gets the portfolios for gojira.
        /// </summary>
        /// <returns></returns>
        public List<Portfolio> GetPortfoliosForgojira(string Market, string Products, string User)
        {
            LoadDB();
            PortfolioList = new List<Portfolio>();
            SqlCommand cmdGetPortfoliosForgojira = new SqlCommand();
            if (Market == "PJM")
            {
                cmdGetPortfoliosForgojira = VayuConnection.CreateCommand();
                cmdGetPortfoliosForgojira.CommandText = "SELECT DISTINCT PORTFOLIOKEY, STRIP FROM END_USER a "
                                            + "INNER JOIN END_USER_MARKET_PORTFOLIO b ON b.END_USER_KEY = a.ENDUSER_KEY "
                                            + "INNER JOIN PORTFOLIO c ON c.PORTFOLIO_ID = b.PORTFOLIOKEY "
                                            + "WHERE a.AD_LOGIN = 'gojira' AND c.HUB='" + Market + "' AND c.PRODUCT='" + Products + "' ORDER BY STRIP";
            }
            else if (Market == "ERCOT")
            {
                cmdGetPortfoliosForgojira = VayuConnection.CreateCommand();
                cmdGetPortfoliosForgojira.CommandText = "SELECT DISTINCT PORTFOLIOKEY, STRIP FROM END_USER a "
                                         + "INNER JOIN END_USER_MARKET_PORTFOLIO b ON b.END_USER_KEY = a.ENDUSER_KEY "
                                         + "INNER JOIN PORTFOLIO c ON c.PORTFOLIO_ID = b.PORTFOLIOKEY "
                                         + "WHERE c.HUB='" + Market + "' AND c.PRODUCT='" + Products + "' and  a.AD_LOGIN='" + User + "' ORDER BY STRIP";
            }
            SqlDataReader reader = cmdGetPortfoliosForgojira.ExecuteReader();
            while (reader.Read())
            {
                objPortfolio = new Portfolio();
                objPortfolio.ID = int.Parse(reader.GetValue(0).ToString());
                objPortfolio.Name = reader.GetString(1);
                PortfolioList.Add(objPortfolio);
                objPortfolio = null;
            }
            reader.Close();
            VayuConnection.Close();
            return PortfolioList;
        }

        /// <summary>
        /// Gets the admin data.
        /// </summary>
        /// <returns></returns>
        public List<AdminData> GetAdminData(string Market, string Product)
        {
            LoadDB();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            List<AdminData> lstAdminData = new List<AdminData>();
            cmdGetAdminData.Parameters["@Market"].Value = Market;
            cmdGetAdminData.Parameters["@Product"].Value = Product;
            SqlDataReader rdr = cmdGetAdminData.ExecuteReader();
            while (rdr.Read())
            {
                lstAdminData.Add(new AdminData
                {
                    PortfolioID = rdr.IsDBNull(0) ? "" : Convert.ToString(rdr.GetValue(0)),
                    PortfolioName = rdr.IsDBNull(0) ? "" : Convert.ToString(rdr.GetValue(1)),
                    Factor = rdr.IsDBNull(1) ? 0 : Convert.ToDecimal(rdr.GetValue(2))
                });
            }
            return lstAdminData;
        }

        /// <summary>
        /// Gets the admin data for gojira.
        /// </summary>
        /// <returns></returns>
        public List<AdminData> GetAdminDataForgojira(string Market, string Product)
        {
            LoadDB();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            List<AdminData> lstAdminData = new List<AdminData>();
            cmdGetAdminDataForgojira.Parameters["@Market"].Value = Market;
            cmdGetAdminDataForgojira.Parameters["@Product"].Value = Product;
            SqlDataReader rdr = cmdGetAdminDataForgojira.ExecuteReader();
            while (rdr.Read())
            {
                lstAdminData.Add(new AdminData
                {
                    PortfolioID = rdr.IsDBNull(0) ? "" : Convert.ToString(rdr.GetValue(0)),
                    PortfolioName = rdr.IsDBNull(0) ? "" : Convert.ToString(rdr.GetValue(1)),
                    Factor = rdr.IsDBNull(1) ? 0 : Convert.ToDecimal(rdr.GetValue(2))
                });
            }
            return lstAdminData;
        }

        /// <summary>
        /// Checks if Portfolio exist or not in DB.
        /// </summary>
        /// <param name="PortfolioID">The portfolio identifier.</param>
        /// <returns></returns>
        public List<AdminData> PortfolioExistOrNot(int PortfolioID)
        {
            LoadDB();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            cmdPortfolioExist.Parameters["@ACCOUNT"].Value = PortfolioID;
            List<AdminData> lstAdminData = new List<AdminData>();
            SqlDataReader rdr = cmdPortfolioExist.ExecuteReader();
            while (rdr.Read())
            {
                lstAdminData.Add(new AdminData
                {
                    PortfolioID = rdr.IsDBNull(0) ? "" : Convert.ToString(rdr.GetValue(0)),
                    Factor = rdr.IsDBNull(1) ? 0 : Convert.ToDecimal(rdr.GetValue(1)),
                    Account2 = rdr.IsDBNull(2) ? 0 : Convert.ToInt32(rdr.GetValue(2))
                });
            }
            return lstAdminData;
        }

        /// <summary>
        /// Inserts Admin Data.
        /// </summary>
        /// <param name="objAdmin">The object admin.</param>
        /// <returns></returns>
        public bool AddFactor(AdminData objAdmin)
        {
            LoadDB();

            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            cmdInsertFactor.Parameters["@ACCOUNT"].Value = objAdmin.PortfolioID;
            cmdInsertFactor.Parameters["@MULTIPLIER"].Value = objAdmin.Factor;
            cmdInsertFactor.Parameters["@Account2"].Value = objAdmin.Account2;
            cmdInsertFactor.Parameters["@Market"].Value = objAdmin.Market;
            cmdInsertFactor.Parameters["@Product"].Value = objAdmin.Product;
            if (cmdInsertFactor.ExecuteNonQuery() >= 1)
            {
                return true;
            }
            VayuConnection.Close();
            return false;
        }

        /// <summary>
        /// Deletes Admin Data.
        /// </summary>
        /// <param name="objAdmin">The object admin.</param>
        /// <returns></returns>
        public bool DeleteFactor(AdminData objAdmin)
        {
            LoadDB();

            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            cmdDeleteFactor.Parameters["@ACCOUNT"].Value = objAdmin.PortfolioID;
            cmdDeleteFactor.Parameters["@MULTIPLIER"].Value = objAdmin.Factor;
            cmdDeleteFactor.Parameters["@Market"].Value = objAdmin.Market;
            cmdDeleteFactor.Parameters["@Product"].Value = objAdmin.Product;
            if (cmdDeleteFactor.ExecuteNonQuery() >= 1)
            {
                return true;
            }
            VayuConnection.Close();
            return false;
        }

        /// <summary>
        /// Gets the account for portfolio.
        /// </summary>
        /// <param name="PortfolioID">The portfolio identifier.</param>
        /// <returns></returns>
        public string GetAccountForPortfolio(string PortfolioID)
        {
            LoadDB();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            cmdGetPortfolioAccount.Parameters["@PORTFOLIO_ID"].Value = PortfolioID;
            string Account = string.Empty;
            SqlDataReader rdr = cmdGetPortfolioAccount.ExecuteReader();
            while (rdr.Read())
            {
                Account = rdr.IsDBNull(0) ? "" : Convert.ToString(rdr.GetValue(0));
            }
            return Account;
        }
        #endregion
    }
}
