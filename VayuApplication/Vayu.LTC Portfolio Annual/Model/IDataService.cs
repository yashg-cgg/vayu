using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vayu.DBLibrary;

namespace Vayu.LTC_PortfolioAnnual.Model
{
    public interface IDataService
    {
        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        void LoadDBCommands();
        /// <summary>
        /// Deletes the CRR.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        void DeleteCRR(int portfolioKey, int mKey);
        /// <summary>
        /// Gets the CRR data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="auction">The auction.</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="portfolioKey">The portfolio key.</param>
        string GetIsoAuctionName(string name);
        /// <summary>
        /// Gets the auction list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="chkNewPortFolio">if set to <c>true</c> [CHK new port folio].</param>
        /// <returns></returns>
        List<string> GetAuctionList(int marketKey, bool chkNewPortFolio);

        List<string> GetNewAuctionList(int Marketkey, bool chkNewPortFolio);
        /// <summary>
        /// Gets the CRR portfolio list.
        /// </summary>
        /// <param name="auction">The auction.</param>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        List<Portfolio> GetCRRPortfolioList(string auction, int marketKey);
        /// <summary>
        /// Gets the CRR auctions.
        /// </summary>
        /// <param name="isoCode">The iso code.</param>
        /// <returns></returns>
        List<CRRAuction> GetFtrAuctions(string isoCode);
        /// <summary>
        /// Gets the SPP miso portfolio list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        List<Tuple<int, string>> GetSppMisoPortfolioList(string marketKey);
        /// <summary>
        /// Gets the cost.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="sartDate">The sart date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="sourceKey">The source key.</param>
        /// <param name="sinkKey">The sink key.</param>
        /// <param name="isMustTake">if set to <c>true</c> [is must take].</param>
        /// <returns></returns>
        Dictionary<DateTime, Cost> GetCost(int marketKey, DateTime sartDate, DateTime endDate, int sourceKey, int sinkKey, bool isMustTake);
        /// <summary>
        /// Gets the cost.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="isMustTake">if set to <c>true</c> [is must take].</param>
        /// <param name="pathList">The path list.</param>
        /// <returns></returns>
        Dictionary<string, Dictionary<string, Dictionary<DateTime, Cost>>> GetCost(int marketKey, DateTime startDate, DateTime endDate, bool isMustTake, List<FTRBid> pathList, string period = "ALL", bool isRtNeeded = true);
        /// <summary>
        /// Gets the auction start date.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="auctionName">Name of the auction.</param>
        /// <returns></returns>
        DateTime GetAuctionStartDate(int marketKey, string auctionName);

        Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivity(DateTime startDate, DateTime endDate, bool IsDollar);

        Dictionary<int, Dictionary<int, Sensitivity>> GetSensitivity(DateTime startDate, DateTime endDate, bool IsDollar, string outageType, string nodeKeyString);


        List<ConstraintContingency> GetConstraintContingencyList(DateTime startDate, DateTime endDate, string predectedType, string market, string rtorda);

        Dictionary<int, Dictionary<int, double>> GetSensitivityInfo(DateTime startDate, DateTime endDate, string predectedType, string market, string rtorda);

        // Dictionary<DateTime, Dictionary<string, Cost>> GetOptionPrices(int marketkey, DateTime startDate, DateTime endDate, List<FTRBid> pathList, bool isAsbid, string period = "ALL");


        Dictionary<DateTime, Dictionary<string, Cost>> GetOptionPrices(int marketKey, DateTime startDate, DateTime endDate, List<FTRBid> pathList, bool asBidChecked, string AuctionSelectedItem, int ID);
        Dictionary<string, double> GetDBclrmwh(int marketkey, string selectedAuction, string selectedPortfolio);

        // Dictionary<string, string> GetClearedPath(int marketkey, string selectedAuction, string selectedPortfolio);

        Dictionary<DateTime, string> GetPeakYN(DateTime startDate, DateTime endDate);

        List<NodePriceHelper> GetallCosts(DateTime date, DateTime lastHistTime);

        Dictionary<string, string> getRTDAMinDate(string market, string sourcenodekeylist, string sinknodekeylist);
        void GetNodeCoordinate(Action<ObservableCollection<NodeCoordinate>, Exception> callback, List<int> sourcesinkNode);
    }
}
