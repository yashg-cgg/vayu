using System;
using System.Collections.Generic;
using Vayu.DBLibrary;

namespace Vayu.Admin.Model
{
    public interface IDataService
    {/// <summary>
     /// Gets all portfolios.
     /// </summary>
     /// <returns></returns>
        List<Portfolio> GetAllPortfolios(string Market, string Products);
        /// <summary>
        /// Gets the portfolios for gojira.
        /// </summary>
        /// <returns></returns>
        List<Portfolio> GetPortfoliosForgojira(string Market, string Products, string User);

        /// <summary>
        /// Gets the admin data.
        /// </summary>
        /// <returns></returns>
        List<AdminData> GetAdminData(string Market, string Product);
        /// <summary>
        /// Gets the admin data for gojira.
        /// </summary>
        /// <returns></returns>
        List<AdminData> GetAdminDataForgojira(string Market, string Product);

        /// <summary>
        /// Checks if Portfolio exist or not in DB.
        /// </summary>
        /// <param name="PortfolioID">The portfolio identifier.</param>
        /// <returns></returns>
        List<AdminData> PortfolioExistOrNot(int PortfolioID);

        List<SubmittedData> GetSubmittedPortfolios(DateTime date);

        /// <summary>
        /// Adds the factor.
        /// </summary>
        /// <param name="objAdmin">The object admin.</param>
        /// <returns></returns>
        bool AddFactor(AdminData objAdmin);
        /// <summary>
        /// Deletes the factor.
        /// </summary>
        /// <param name="objAdmin">The object admin.</param>
        /// <returns></returns>
        bool DeleteFactor(AdminData objAdmin);

        /// <summary>
        /// Gets the account for portfolio.
        /// </summary>
        /// <param name="PortfolioID">The portfolio identifier.</param>
        /// <returns></returns>
        string GetAccountForPortfolio(string PortfolioID);
    }

    /// <summary>
    /// 
    /// </summary>
    public class AdminData
    {
        /// <summary>
        /// Gets or sets the portfolio identifier.
        /// </summary>
        /// <value>
        /// The portfolio identifier.
        /// </value>
        public string PortfolioID { get; set; }
        /// <summary>
        /// Gets or sets the name of the portfolio.
        /// </summary>
        /// <value>
        /// The name of the portfolio.
        /// </value>
        public string PortfolioName { get; set; }
        /// <summary>
        /// Gets or sets the factor.
        /// </summary>
        /// <value>
        /// The factor.
        /// </value>
        public decimal Factor { get; set; }
        /// <summary>
        /// Gets or sets the account2.
        /// </summary>
        /// <value>
        /// The account2.
        /// </value>
        public int Account2 { get; set; }
        /// <summary>
        /// Gets or sets the Market.
        /// </summary>
        /// <value>
        /// The Marktet.
        /// </value>
        public int Market { get; set; }
        /// <summary>
        /// Gets or sets the Product.
        /// </summary>
        /// <value>
        /// The account2.
        /// </value>
        public string Product { get; set; }

    }
    public class SubmittedData
    {

        public double? Risk1MW { get; set; }
        public double? SubmittedMW { get; set; }
        public int SubmittedCount { get; set; }
        public int ExpectedCount { get; set; }

        public string Status { get; set; }

    }

}
