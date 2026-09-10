using System;
using System.Collections.Generic;
using Vayu.DBLibrary;

namespace Vayu.CRRAnalysis.Model
{
    public interface IDataService
    {
        List<string> GetAuctionList(bool chkNewPortFolio);

        List<string> GetNewAuctionList(bool chkNewPortFolio);

        List<CRRAuction> GetFtrAuctions(string isoCode);
        List<Tuple<int, string>> GetPortfolioList(string marketKey);
        Dictionary<int, string> GetPrevKeys(string Auction);

        DateTime GetAuctionStartDate(int marketKey, string auctionName);

        Dictionary<string, double> GetPrevData(int Key);

        Dictionary<string, double> GetPrevNodeData(int Key, string nodenames);
        Dictionary<string, double> GetPrevOptionData(int Key);


        Dictionary<string, double> GetPrevOptionsData(DateTime date);

        Dictionary<string, double> GetSubmittedOptionData(int Key);

        Dictionary<string, double> GetClearedOptionData(int Key);
        Dictionary<string, double> GetMin();
        Dictionary<string, double> GetMax();
        Dictionary<string, double> GetMedian45();
    }
}
