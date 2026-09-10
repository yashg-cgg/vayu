using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRAuction.Model
{
    public class DataService : IDataService
    {
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("Welcome to MVVM Light");
            callback(item, null);
        }

        /// <summary>
        /// Gets the auction data.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="market">The market.</param>
        public void GetAuctionData(Action<System.Collections.Generic.List<FtrAuctionType>, Exception> callback, string market)
        {

            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            List<FtrAuctionType> auctionList = new List<FtrAuctionType>();

            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (market.ToUpper() == "ERCOT")
                        {
                            cmd.CommandText = "Select CRRAuctionKey,CRRAuctionName,AuctionStartDate,AuctionEndDate,AuctionRound,ResultsPostedDate,CreditPostedDate,CRRAuctionType from CRRAuction where crrauctionkey  not in (select distinct crrauctionkey from CRRAuctionNodePrice ) and ResultsPostedDate is not null  order by resultsposteddate";
                        }

                        IDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            try
                            {
                                auctionList.Add(new FtrAuctionType
                                {
                                    FTRAuctionKey = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)),
                                    FTRAuctionName = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetValue(1)),
                                    AuctionStartDate = reader.IsDBNull(2) ? new DateTime() : Convert.ToDateTime(reader.GetValue(2)),
                                    AuctionEndDate = reader.IsDBNull(3) ? new DateTime() : Convert.ToDateTime(reader.GetValue(3)),
                                    AuctionRound = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                                    ResultPostedDate = reader.IsDBNull(5) ? new DateTime() : Convert.ToDateTime(reader.GetValue(5)),
                                    CreditPostedDate = reader.IsDBNull(6) ? new DateTime() : Convert.ToDateTime(reader.GetValue(6)),
                                    FTRAuctionPeriod = reader.IsDBNull(7) ? "" : Convert.ToString(reader.GetValue(7)),
                                    Market = market,
                                });
                            }
                            catch
                            {
                            }
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                }
                System.Threading.Tasks.Parallel.ForEach(auctionList, a =>
                {
                    a.IsOld = a.ResultPostedDate < DateTime.Today;
                });
                callback(auctionList, null);
            }
            catch (Exception ex)
            {
                callback(null, ex);
            }

        }

        /// <summary>
        /// Gets the market.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        private string GetMarket(int i)
        {
            switch (i)
            {
                case 9: return "ERCOT";
                default: return "";
            }

        }
    }
}
