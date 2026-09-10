using System;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.CRRAnnAnalysis.Model
{
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;
        private SqlCommand mSelectSeqYearData;

        public DataTable GetSequenceYearData(
            int startYear,
            int endYear,
            int month, 
            string timeUse, 
            string hedge,
            int sourceKey,
            int sinkKey, 
            string auctionPattern)
        {
            DataTable dt = new DataTable();
            try
            {
                if (mSelectSeqYearData == null)
                    LoadDBCommands();

                //DataTable dt = new DataTable();
                mSelectSeqYearData.Parameters.Clear();

                mSelectSeqYearData.Parameters.AddWithValue("@StartYear", startYear);
                mSelectSeqYearData.Parameters.AddWithValue("@EndYear", endYear);
                mSelectSeqYearData.Parameters.AddWithValue("@SelectedMonth", month);
                mSelectSeqYearData.Parameters.AddWithValue("@TimeUse", timeUse);
                mSelectSeqYearData.Parameters.AddWithValue("@Hedge", hedge);
                mSelectSeqYearData.Parameters.AddWithValue("@SourceKey", sourceKey);
                mSelectSeqYearData.Parameters.AddWithValue("@SinkKey", sinkKey);
                mSelectSeqYearData.Parameters.AddWithValue("@AuctionPattern", auctionPattern);

                SqlDataAdapter adapter = new SqlDataAdapter(mSelectSeqYearData);
                adapter.Fill(dt);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return dt;
        }

        public DataTable GetMonthlyShadowPrices(int startYear,int endYear,string timeUse,string hedge,string sourceName,string sinkName)
        {
            DataTable dt = new DataTable();

            string query = @" SELECT YEAR(StartDate) AS AuctionYear, MONTH(StartDate) AS AuctionMonth,
                              MAX(ShadowPrice) AS ShadowPrice
FROM CRRAuctionResults
WHERE Hedge = @Hedge
AND SourceName = @SourceName
AND SinkName = @SinkName
AND TimeUse = @TimeUse
AND Bid = 'BUY'
AND Bid24Hour = 'No'
AND YEAR(StartDate) BETWEEN @StartYear AND @EndYear
GROUP BY YEAR(StartDate), MONTH(StartDate)
ORDER BY AuctionYear DESC, AuctionMonth";

            using(SqlCommand cmd = new SqlCommand(query,VayuConnection))
            {
                cmd.Parameters.AddWithValue("@StartYear", startYear);
                cmd.Parameters.AddWithValue("@EndYear", startYear);
                cmd.Parameters.AddWithValue("@TimeUse", timeUse);
                cmd.Parameters.AddWithValue("@Hedge", hedge);
                cmd.Parameters.AddWithValue("@SourceName", sourceName);
                cmd.Parameters.AddWithValue("@SinkName", sinkName);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);

            }
            return dt;


        }

        public void LoadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectSeqYearData = new SqlCommand();
            mSelectSeqYearData.Connection = VayuConnection;
            mSelectSeqYearData.CommandType = System.Data.CommandType.Text;

            mSelectSeqYearData.CommandText =
               @"WITH BaseData AS
              (
              SELECT
              YEAR(StartDate) AS AuctionYear,
              MONTH(StartDate) AS AuctionMonth,
              ShadowPrice,
              CASE
              WHEN AuctionName LIKE '%Seq1%' THEN 'Seq1'
              WHEN AuctionName LIKE '%Seq2%' THEN 'Seq2'
              WHEN AuctionName LIKE '%Seq3%' THEN 'Seq3'
              WHEN AuctionName LIKE '%Seq4%' THEN 'Seq4'
              WHEN AuctionName LIKE '%Seq5%' THEN 'Seq5'
              WHEN AuctionName LIKE '%Seq6%' THEN 'Seq6'
              END AS Seq
              FROM CRRAnnualAuctionResult
              WHERE TimeUse = @TimeUse
              AND Hedge = @Hedge
              AND SourceKey = @SourceKey
              AND SinkKey = @SinkKey
              AND YEAR(StartDate) BETWEEN @StartYear AND @EndYear
              AND MONTH(StartDate) = @SelectedMonth
              AND AuctionName LIKE @AuctionPattern
              )
              
              SELECT
              AuctionYear,
              MAX(CASE WHEN Seq = 'Seq6' THEN ShadowPrice END) AS Seq6,
              MAX(CASE WHEN Seq = 'Seq5' THEN ShadowPrice END) AS Seq5,
              MAX(CASE WHEN Seq = 'Seq4' THEN ShadowPrice END) AS Seq4,
              MAX(CASE WHEN Seq = 'Seq3' THEN ShadowPrice END) AS Seq3,
              MAX(CASE WHEN Seq = 'Seq2' THEN ShadowPrice END) AS Seq2,
              MAX(CASE WHEN Seq = 'Seq1' THEN ShadowPrice END) AS Seq1,
              MAX(ShadowPrice) AS HighestMonthShadowPrice
              FROM BaseData
              GROUP BY AuctionYear
              ORDER BY AuctionYear DESC";
        }
    }
}
