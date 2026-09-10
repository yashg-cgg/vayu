using System;
using System.Collections.Generic;
using Vayu.DBLibrary;
using Vayu.WorkbookStatistics.ViewModels;

namespace Vayu.WorkbookStatistics.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        void loadDBCommands();
        /// <summary>
        /// Gets the risk limit.
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        /// <returns></returns>
        RiskLimit GetRiskLimit(int portfolio);
        /// <summary>
        /// Moves the trade.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="traderList">The trader list.</param>
        /// <param name="submitDate">The submit date.</param>
        /// <param name="forfeitureList">The forfeiture list.</param>
        void MoveTrade(Action<List<Path>, List<string>> callback, List<Portfolio> traderList, DateTime submitDate, List<string> forfeitureList);
        /// <summary>
        /// Gets the sensitivity.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="IsDollar">if set to <c>true</c> [is dollar].</param>
        /// <returns></returns>
        Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivity(DateTime startDate, DateTime endDate, bool IsDollar, string market, bool isDA);
        /// <summary>
        /// Gets the sensitivity by constraint i ds.
        /// </summary>
        /// <param name="ConstraintDate">The constraint date.</param>
        /// <returns></returns>
        Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivityByConstraintIDs(DateTime ConstraintDate);

        Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivityByConstraintIDsForNewConstraints(DateTime ConstraintDate);

        /// <summary>
        /// Gets the sensitivity on start checked.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="IsDollar">if set to <c>true</c> [is dollar].</param>
        /// <param name="StartDateChecked">if set to <c>true</c> [start date checked].</param>
        /// <param name="OutageChecked">if set to <c>true</c> [outage checked].</param>
        /// <param name="ShadowPrice">The shadow price.</param>
        /// <param name="PathList">The path list.</param>
        /// <param name="MarketKey">The market key.</param>
        /// <param name="ConstraintRTNumList">The constraint rt number list.</param>
        /// <returns></returns>
        Dictionary<int, Dictionary<int, Dictionary<int, Sensitivity>>> GetSensitivityOnStartChecked(DateTime startDate, DateTime endDate, bool IsDollar, bool StartDateChecked, bool OutageChecked, string ShadowPrice, List<Path> PathList, int MarketKey, out List<string> ConstraintRTNumList);
        /// <summary>
        /// Gets the constraint not exist.
        /// </summary>
        /// <param name="ConstraintDate">The constraint date.</param>
        /// <returns></returns>
        List<Sensitivity> GetConstraintNotExist(DateTime ConstraintDate);

        List<Portfolio> GetExternalPortfolio(DateTime sDate, DateTime eDate);
        /// <summary>
        /// Gets the maximum load.
        /// </summary>
        /// <param name="similarDate">The similar date.</param>
        /// <returns></returns>
        double GetMaxLoad(DateTime similarDate);

        int CheckSubmittedPortfolio(DateTime SubmitDate, int portfolio);
        /// <summary>
        /// Sets the trade identifier.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        string SetTradeId(string portfolioKey, string market);
        /// <summary>
        /// Saves the preference.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="preference">The preference.</param>
        void SavePreference(string user, string preference);
        /// <summary>
        /// Deletes the uptos PTP bids.
        /// </summary>
        /// <param name="bidId">The bid identifier.</param>
        /// <param name="submitDate">The submit date.</param>
        /// <param name="market">The market.</param>
        void DeleteUptosPtpBids(string bidId, DateTime submitDate, string market);
        /// <summary>
        /// Gets the bid identifier.
        /// </summary>
        /// <param name="portfolio">The portfolio.</param>
        /// <returns></returns>
        System.Numerics.BigInteger GetBidId(int portfolio);
        /// <summary>
        /// Gets the preference.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        string GetPreference(string user);
        /// <summary>
        /// Gets the database bids.
        /// </summary>
        /// <param name="marketDateTime">The market date time.</param>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <returns></returns>
        List<Bid> GetDBBids(DateTime marketDateTime, int portfolioKey);

        Dictionary<DateTime, Dictionary<int, double>> GetHourlyImpact(DateTime startDate, DateTime endDate, string market, bool isDA);

        List<DeenergizedNode> GetDeenergizedNodes();

        Dictionary<string, string> GetFuelSource();
        void UpdateScaleNumber(double num, Portfolio TraderPortfolioSelected, DateTime StartDate, DateTime EndDate);

        List<string> GetStrDeenergizedNodes();
    }
}
