using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.DAMImpact.Model
{
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;
        private SqlCommand SelectNotification;
        private SqlCommand SelectSubmissionNotification;
        private SqlCommand SelectSearchSubmissionNotification;
        private SqlCommand SelectMessages;
        private SqlCommand SelectSearchMessages;
        private SqlCommand selectSourceDataForClearedMW;
        private SqlCommand SelectSinkDataForClearedMW;
        private SqlCommand SelectSourceDataForFullMarketAquired;
        private SqlCommand SelectSinkDataForFullMarketAquired;

        public DataService()
        {
            loadDBCommands();
        }

        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            SelectNotification = VayuConnection.CreateCommand();
            SelectNotification.CommandText = "SELECT * FROM ErcotListenerMesages WHERE IssuedTime BETWEEN @StartDate AND @EndDate";
            SelectNotification.Parameters.AddWithValue("@StartDate", "IssuedTime");
            SelectNotification.Parameters.AddWithValue("@EndDate", "IssuedTime");

            SelectSubmissionNotification = VayuConnection.CreateCommand();
            SelectSubmissionNotification.CommandText = " SELECT * from ErcotListenerSubmissionMesages where MarketDate  BETWEEN @StartDate AND @EndDate";
            SelectSubmissionNotification.Parameters.AddWithValue("@StartDate", "SubmittedDateTime");
            SelectSubmissionNotification.Parameters.AddWithValue("@EndDate", "SubmittedDateTime");

            SelectSearchSubmissionNotification = VayuConnection.CreateCommand();
            SelectSearchSubmissionNotification.CommandText = "SELECT * from ErcotListenerSubmissionMesages where MarketDate  BETWEEN @StartDate AND @EndDate and Status=@Status";
            SelectSubmissionNotification.Parameters.AddWithValue("@StartDate", "SubmittedDateTime");
            SelectSubmissionNotification.Parameters.AddWithValue("@EndDate", "SubmittedDateTime");
            SelectSubmissionNotification.Parameters.AddWithValue("@Status", "Status");


            SelectMessages = new SqlCommand();
            SelectMessages.CommandText = "SELECT * FROM OperationMessages_OR WHERE CreateDate BETWEEN @StartDate AND @EndDate order by CreateDate desc";
            SelectMessages.Parameters.AddWithValue("@StartDate", "CreateDate");
            SelectMessages.Parameters.AddWithValue("@EndDate", "CreateDate");
            SelectMessages.Connection = VayuConnection;

            //SelectSearchMessages = VayuDBConnection.CreateCommand();
            SelectSearchMessages = new SqlCommand();
            SelectSearchMessages.CommandText = "SELECT * from OperationMessages_OR where CreateDate  BETWEEN @StartDate AND @EndDate and Status=@Status order by CreateDate desc";
            SelectSearchMessages.Parameters.AddWithValue("@StartDate", "CreateDate");
            SelectSearchMessages.Parameters.AddWithValue("@EndDate", "CreateDate");
            SelectSearchMessages.Parameters.AddWithValue("@Status", "Status");
            SelectSearchMessages.Connection = VayuConnection;

            selectSourceDataForClearedMW = new SqlCommand();
            selectSourceDataForClearedMW.CommandText = "SELECT Pivottable.*, b.NodeName " +
                                                         "FROM (" +
                                                         "    SELECT SourceNodeKey, ClearedMW, DATEPART(Hour, MarketDateTime) AS hours " +
                                                         "    FROM ClearedEES " +
                                                         "    WHERE MarketDateTime > @StartDate AND MarketDateTime <= DATEADD(DAY, 1, @StartDate)" +
                                                         " ) AS SourceTable " +
                                                         "PIVOT (" +
                                                         "    SUM(ClearedMW) " +
                                                         "    FOR hours IN([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[0])" +
                                                         ") AS Pivottable " +
                                                         "JOIN node b ON b.NodeKey = Pivottable.SourceNodeKey " +
                                                         "ORDER BY b.NodeName";
            selectSourceDataForClearedMW.Parameters.AddWithValue("@StartDate", "MarketDateTime");

            selectSourceDataForClearedMW.Connection = VayuConnection;

            SelectSinkDataForClearedMW = new SqlCommand();
            SelectSinkDataForClearedMW.CommandText = "SELECT Pivottable.*, b.NodeName FROM (" +
                                                    "SELECT SinkNodeKey, ClearedMW, DATEPART(Hour, MarketDateTime) AS hours " +
                                                    "FROM ClearedEES " +
                                                    "WHERE MarketDateTime > @StartDate AND MarketDateTime <= DATEADD(DAY, 1, @StartDate)" +
                                                    ") AS SourceTable " +
                                                    "PIVOT (" +
                                                    "SUM(ClearedMW) FOR hours IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[0])" +
                                                    ") AS Pivottable " +
                                                    "JOIN node b ON b.NodeKey = Pivottable.SinkNodeKey " +
                                                    "ORDER BY b.NodeName";
            SelectSinkDataForClearedMW.Parameters.AddWithValue("@StartDate", "MarketDateTime");

            SelectSinkDataForClearedMW.Connection = VayuConnection;

            SelectSourceDataForFullMarketAquired = new SqlCommand();
            SelectSourceDataForFullMarketAquired.CommandText = "WITH cte1 AS (" +
                                                    "   SELECT Pivottable.*, b.NodeName FROM (" +
                                                    "       SELECT NodeKey, TOTAL_PTP_OBL_AWARDED_Source, HOURENDING FROM DAMPTPObligation WHERE DELIVERYDATE =@StartDate " +
                                                    "   ) AS SourceTable" +
                                                    "   PIVOT (" +
                                                    "       SUM(TOTAL_PTP_OBL_AWARDED_Source) FOR HOURENDING IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[24])" +
                                                    "   ) AS Pivottable" +
                                                    "   JOIN node b ON b.NodeKey = Pivottable.NodeKey" +
                                                    "), cte2 AS (" +
                                                    "   SELECT Pivottable.*, b.NodeName FROM (" +
                                                    "       SELECT SourceNodeKey, ClearedMW, DATEPART(Hour, MarketDateTime) AS hours FROM ClearedEES WHERE MarketDateTime > @StartDate AND MarketDateTime <= DATEADD(DAY, 1, @StartDate)" +
                                                    "   ) AS SourceTable" +
                                                    "   PIVOT (" +
                                                    "       SUM(ClearedMW) FOR hours IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[0])" +
                                                    "   ) AS Pivottable" +
                                                    "   JOIN node b ON b.NodeKey = Pivottable.SourceNodeKey" +
                                                    ") SELECT cte1.nodename, dbo.[CompareNumbers](cte1.[1], cte2.[1]) AS [1]," +
                                                    "   dbo.[CompareNumbers](cte1.[2], cte2.[2]) AS [2]," +
                                                    "   dbo.[CompareNumbers](cte1.[3], cte2.[3]) AS [3]," +
                                                    "   dbo.[CompareNumbers](cte1.[4], cte2.[4]) AS [4]," +
                                                    "   dbo.[CompareNumbers](cte1.[5], cte2.[5]) AS [5]," +
                                                    "   dbo.[CompareNumbers](cte1.[6], cte2.[6]) AS [6]," +
                                                    "   dbo.[CompareNumbers](cte1.[7], cte2.[7]) AS [7]," +
                                                    "   dbo.[CompareNumbers](cte1.[8], cte2.[8]) AS [8]," +
                                                    "   dbo.[CompareNumbers](cte1.[9], cte2.[9]) AS [9]," +
                                                    "   dbo.[CompareNumbers](cte1.[10], cte2.[10]) AS [10]," +
                                                    "   dbo.[CompareNumbers](cte1.[11], cte2.[11]) AS [11]," +
                                                    "   dbo.[CompareNumbers](cte1.[12], cte2.[12]) AS [12]," +
                                                    "   dbo.[CompareNumbers](cte1.[13], cte2.[13]) AS [13]," +
                                                    "   dbo.[CompareNumbers](cte1.[14], cte2.[14]) AS [14]," +
                                                    "   dbo.[CompareNumbers](cte1.[15], cte2.[15]) AS [15]," +
                                                    "   dbo.[CompareNumbers](cte1.[16], cte2.[16]) AS [16]," +
                                                    "   dbo.[CompareNumbers](cte1.[17], cte2.[17]) AS [17]," +
                                                    "   dbo.[CompareNumbers](cte1.[18], cte2.[18]) AS [18]," +
                                                    "   dbo.[CompareNumbers](cte1.[19], cte2.[19]) AS [19]," +
                                                    "   dbo.[CompareNumbers](cte1.[20], cte2.[20]) AS [20]," +
                                                    "   dbo.[CompareNumbers](cte1.[21], cte2.[21]) AS [21]," +
                                                    "   dbo.[CompareNumbers](cte1.[22], cte2.[22]) AS [22]," +
                                                    "   dbo.[CompareNumbers](cte1.[23], cte2.[23]) AS [23]," +
                                                    "   dbo.[CompareNumbers](cte1.[24], cte2.[0]) AS [24]" +
                                                    " FROM cte1 JOIN cte2 ON cte1.NodeName = cte2.NodeName" +
                                                    " ORDER BY cte1.NodeName";
            SelectSourceDataForFullMarketAquired.Parameters.AddWithValue("@StartDate", "MarketDateTime");

            SelectSourceDataForFullMarketAquired.Connection = VayuConnection;

            SelectSinkDataForFullMarketAquired = new SqlCommand();
            SelectSinkDataForFullMarketAquired.CommandText = "WITH cte1 AS (" +
                                                            "   SELECT Pivottable.*, b.NodeName FROM (" +
                                                            "       SELECT NodeKey, TOTAL_PTP_OBL_AWARDED_Sink, HOURENDING FROM DAMPTPObligation WHERE DELIVERYDATE =@StartDate" +
                                                            "   ) AS SinkTable" +
                                                            "   PIVOT (" +
                                                            "       SUM(TOTAL_PTP_OBL_AWARDED_Sink) FOR HOURENDING IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[24])" +
                                                            "   ) AS Pivottable" +
                                                            "   JOIN node b ON b.NodeKey = Pivottable.NodeKey" +
                                                            "), cte2 AS (" +
                                                            "   SELECT Pivottable.*, b.NodeName FROM (" +
                                                            "       SELECT SinkNodeKey, ClearedMW, DATEPART(Hour, MarketDateTime) AS hours FROM ClearedEES WHERE MarketDateTime > @StartDate AND MarketDateTime <= DATEADD(DAY, 1, @StartDate)" +
                                                            "   ) AS SinkTable" +
                                                            "   PIVOT (" +
                                                            "       SUM(ClearedMW) FOR hours IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[0])" +
                                                            "   ) AS Pivottable" +
                                                            "   JOIN node b ON b.NodeKey = Pivottable.SinkNodeKey" +
                                                            ") SELECT cte1.nodename, dbo.[CompareNumbers](cte1.[1], cte2.[1]) AS [1]," +
                                                            "   dbo.[CompareNumbers](cte1.[2], cte2.[2]) AS [2]," +
                                                            "   dbo.[CompareNumbers](cte1.[3], cte2.[3]) AS [3]," +
                                                            "   dbo.[CompareNumbers](cte1.[4], cte2.[4]) AS [4]," +
                                                            "   dbo.[CompareNumbers](cte1.[5], cte2.[5]) AS [5]," +
                                                            "   dbo.[CompareNumbers](cte1.[6], cte2.[6]) AS [6]," +
                                                            "   dbo.[CompareNumbers](cte1.[7], cte2.[7]) AS [7]," +
                                                            "   dbo.[CompareNumbers](cte1.[8], cte2.[8]) AS [8]," +
                                                            "   dbo.[CompareNumbers](cte1.[9], cte2.[9]) AS [9]," +
                                                            "   dbo.[CompareNumbers](cte1.[10], cte2.[10]) AS [10]," +
                                                            "   dbo.[CompareNumbers](cte1.[11], cte2.[11]) AS [11]," +
                                                            "   dbo.[CompareNumbers](cte1.[12], cte2.[12]) AS [12]," +
                                                            "   dbo.[CompareNumbers](cte1.[13], cte2.[13]) AS [13]," +
                                                            "   dbo.[CompareNumbers](cte1.[14], cte2.[14]) AS [14]," +
                                                            "   dbo.[CompareNumbers](cte1.[15], cte2.[15]) AS [15]," +
                                                            "   dbo.[CompareNumbers](cte1.[16], cte2.[16]) AS [16]," +
                                                            "   dbo.[CompareNumbers](cte1.[17], cte2.[17]) AS [17]," +
                                                            "   dbo.[CompareNumbers](cte1.[18], cte2.[18]) AS [18]," +
                                                            "   dbo.[CompareNumbers](cte1.[19], cte2.[19]) AS [19]," +
                                                            "   dbo.[CompareNumbers](cte1.[20], cte2.[20]) AS [20]," +
                                                            "   dbo.[CompareNumbers](cte1.[21], cte2.[21]) AS [21]," +
                                                            "   dbo.[CompareNumbers](cte1.[22], cte2.[22]) AS [22]," +
                                                            "   dbo.[CompareNumbers](cte1.[23], cte2.[23]) AS [23]," +
                                                            "   dbo.[CompareNumbers](cte1.[24], cte2.[0]) AS [24]" +
                                                            " FROM cte1 JOIN cte2 ON cte1.NodeName = cte2.NodeName" +
                                                            " ORDER BY cte1.NodeName";
            SelectSinkDataForFullMarketAquired.Parameters.AddWithValue("@StartDate", "MarketDateTime");

            SelectSinkDataForFullMarketAquired.Connection = VayuConnection;
        }

        public (List<RTImpactModel>, int) GetRTImpactsource(DateTime StartDate)
        {
            List<RTImpactModel> lstRTImapact = new List<RTImpactModel>();
            int totalcount = 0;
            try
            {
                RTImpactModel RTList = null;
                int nonNullCount = 0;

                using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();

                        cmd.CommandText = selectSourceDataForClearedMW.CommandText;
                        cmd.Parameters.Add("@StartDate", StartDate);


                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            RTList = new RTImpactModel();

                            for (int i = 1; i <= 24; i++)
                            {
                                decimal? hourValue = reader.IsDBNull(i) ? (decimal?)null : reader.GetDecimal(i);
                                string hourString = hourValue.HasValue ? hourValue.Value.ToString() : string.Empty;
                                if (hourValue.HasValue)
                                {
                                    nonNullCount++;
                                }
                                // Assuming SetHourValue expects a string
                                SetHourValue(RTList, i, hourString);

                            }

                            RTList.NodeName = reader.GetString(25);
                            lstRTImapact.Add(RTList);

                        }
                        totalcount = nonNullCount;


                    }
                }

            }
            catch (Exception ex)
            {
            }
            return (lstRTImapact.ToList(), totalcount);
        }
        private void SetHourValue(RTImpactModel rtList, int hourNumber, string value)
        {
            switch (hourNumber)
            {
                case 1: rtList.Hour1 = value; break;
                case 2: rtList.Hour2 = value; break;
                case 3: rtList.Hour3 = value; break;
                case 4: rtList.Hour4 = value; break;
                case 5: rtList.Hour5 = value; break;
                case 6: rtList.Hour6 = value; break;
                case 7: rtList.Hour7 = value; break;
                case 8: rtList.Hour8 = value; break;
                case 9: rtList.Hour9 = value; break;
                case 10: rtList.Hour10 = value; break;
                case 11: rtList.Hour11 = value; break;
                case 12: rtList.Hour12 = value; break;
                case 13: rtList.Hour13 = value; break;
                case 14: rtList.Hour14 = value; break;
                case 15: rtList.Hour15 = value; break;
                case 16: rtList.Hour16 = value; break;
                case 17: rtList.Hour17 = value; break;
                case 18: rtList.Hour18 = value; break;
                case 19: rtList.Hour19 = value; break;
                case 20: rtList.Hour20 = value; break;
                case 21: rtList.Hour21 = value; break;
                case 22: rtList.Hour22 = value; break;
                case 23: rtList.Hour23 = value; break;
                case 24: rtList.Hour24 = value; break;
                default: break; // Handle unexpected hour numbers
            }
        }

        public (List<RTImpactModel>, int) GetRTImpactSink(DateTime StartDate)
        {
            List<RTImpactModel> lstRTImapactSink = new List<RTImpactModel>();
            int nonNullCount = 0;
            int totalcount = 0;
            try
            {
                RTImpactModel RTList = null;

                using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();
                        cmd.CommandText = SelectSinkDataForClearedMW.CommandText;
                        cmd.Parameters.Add("@StartDate", StartDate);


                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            RTList = new RTImpactModel();

                            for (int i = 1; i <= 24; i++)
                            {
                                decimal? hourValue = reader.IsDBNull(i) ? (decimal?)null : reader.GetDecimal(i);
                                string hourString = hourValue.HasValue ? hourValue.Value.ToString() : string.Empty;
                                if (hourValue.HasValue)
                                {
                                    nonNullCount++;
                                }
                                // Assuming SetHourValue expects a string
                                SetHourValue(RTList, i, hourString);
                            }

                            RTList.NodeName = reader.GetString(25);
                            lstRTImapactSink.Add(RTList);
                        }
                        totalcount = nonNullCount;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return (lstRTImapactSink.ToList(), totalcount);
        }

        public (List<RTImpactModel>, int) GetRTImpactSourceFMA(DateTime StartDate)
        {
            List<RTImpactModel> lstRTImapactSourceFMA = new List<RTImpactModel>();
            int sumOfCounts = 0;
            try
            {
                RTImpactModel RTList = null;

                using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();
                        cmd.CommandText = SelectSourceDataForFullMarketAquired.CommandText;
                        cmd.Parameters.Add("@StartDate", StartDate);


                        SqlDataReader reader = cmd.ExecuteReader();
                        int[] totalCountOfOnes = new int[24];

                        while (reader.Read())
                        {

                            RTList = new RTImpactModel();
                            for (int i = 1; i <= 24; i++)
                            {
                                string hourValue = reader.GetString(i);
                                string hourString = hourValue == "-" ? string.Empty : hourValue.ToString();
                                int countOfOnes = hourString.Count(c => c == '1');
                                // Assuming SetHourValue expects a string
                                SetHourValue(RTList, i, hourString);
                                totalCountOfOnes[i - 1] += countOfOnes;
                            }
                            RTList.NodeName = reader.GetString(0);
                            lstRTImapactSourceFMA.Add(RTList);
                        }
                        sumOfCounts = totalCountOfOnes.Sum();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return (lstRTImapactSourceFMA.ToList(), sumOfCounts);
        }

        public (List<RTImpactModel>, int) GetRTImpactSinkFMA(DateTime StartDate)
        {
            List<RTImpactModel> lstRTImapactSinkFMA = new List<RTImpactModel>();
            int sumOfCounts = 0;
            try
            {
                RTImpactModel RTList = null;

                using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();
                        cmd.CommandText = SelectSinkDataForFullMarketAquired.CommandText;
                        cmd.Parameters.Add("@StartDate", StartDate);

                        int[] totalCountOfOnes = new int[24];
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            RTList = new RTImpactModel();
                            for (int i = 1; i <= 24; i++)
                            {
                                string hourValue = reader.GetString(i);
                                string hourString = hourValue == "-" ? string.Empty : hourValue.ToString();
                                int countOfOnes = hourString.Count(c => c == '1');
                                // Assuming SetHourValue expects a string
                                SetHourValue(RTList, i, hourString);
                                totalCountOfOnes[i - 1] += countOfOnes;
                            }
                            RTList.NodeName = reader.GetString(0);
                            lstRTImapactSinkFMA.Add(RTList);
                        }
                        sumOfCounts = totalCountOfOnes.Sum();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return (lstRTImapactSinkFMA.ToList(), sumOfCounts);
        }

    }
}
