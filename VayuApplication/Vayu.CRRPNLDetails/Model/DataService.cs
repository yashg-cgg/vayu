using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.CRRCalculationLibrary;

namespace Vayu.CRRPNLDetails.Model
{
    public class DataService : IDataService
    {
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private SqlConnection VayuConnection;
        private SqlCommand mSelectDaysCommand;

        private readonly WcfClientWrapper<ISourceSink> _client;
        #region SQL Commands

        /// <summary>
        /// The m select ercot external mp
        /// </summary>
        private SqlCommand mSelectERCOTExternalMP;
        /// <summary>
        /// The m select external mp
        /// </summary>
        private SqlCommand mSelectExternalMP;
        private SqlCommand sSelect6MonthDate;
        private SqlCommand sSelect6MonthAuctionKey;
        private SqlCommand mSelectErcotMonthCommand;

        #endregion

        /// <summary>
        /// Initializes the database commands.
        /// </summary>
        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            //
            mSelectERCOTExternalMP = new SqlCommand();
            //mSelectERCOTExternalMP.CommandText = "select distinct(AccountHolder) from ERCOT.CRRAnnualAuctionResults union select distinct(AccountHolder) from ERCOT.CRRMonthlyAuctionResults";
            //mSelectERCOTExternalMP.CommandText = "select distinct(AccountHolder) from CRRAuctionResults union select distinct(AccountHolder) from CRRAuctionResults";
            mSelectERCOTExternalMP.CommandText = "select distinct(AccountHolder) from CRRAnnualAuctionResult union select distinct(AccountHolder) from CRRAuctionResults";
            mSelectERCOTExternalMP.Connection = VayuConnection;
            //
            mSelectExternalMP = new SqlCommand();
            mSelectExternalMP.CommandText = "select distinct AccountHolder from CRRAuctionResults order by AccountHolder";
            //mSelectExternalMP.Parameters.Add("@MarketKey", SqlDbType.Int);
            mSelectExternalMP.Connection = VayuConnection;
            //sSelect6MonthDate
            sSelect6MonthDate = new SqlCommand();
            sSelect6MonthDate.CommandText = "select distinct(AuctionName) from CRRAnnualAuction where AuctionName like '%@year%' order by AuctionName";
            sSelect6MonthDate.Parameters.AddWithValue("@year", "year");
            sSelect6MonthDate.Connection = VayuConnection;
            //sSelect6MonthAuctionKey
            sSelect6MonthAuctionKey = new SqlCommand();
            sSelect6MonthAuctionKey.CommandText = "select CRRAuctionKey from crrauction where  CRRAuctionName like '%@year%' and CRRAuctionType='Annual' and AuctionRound in ('@round')";
            sSelect6MonthAuctionKey.Parameters.AddWithValue("@year", "year");
            sSelect6MonthAuctionKey.Connection = VayuConnection;
            //
            mSelectErcotMonthCommand = new SqlCommand();
            mSelectErcotMonthCommand.CommandText = "select distinct periodyear from period order by periodyear";
            mSelectErcotMonthCommand.Connection = VayuConnection;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        public DataService( )
        {
            _client = new WcfClientWrapper<ISourceSink>(Vayu.CommonAccessLibrary.ServiceConnections.GetCRRPNLService());
            loadDBCommands();
        }

        public List<SourceSink> GetFTRs(int marketKey,List<string> accounts,DateTime period)
        {
            return _client.Execute(c =>
                  c.GetFTRs(marketKey, accounts, period));
        }

        public List<SourceSink> Get6MonthsCRRs(int key,List<string> accounts,DateTime period,string rounds)
        {
            return _client.Execute(c => c.Get6MonthCRRs(key, accounts, period, rounds));
        }
        /// <summary>
        /// Gets the ercot account holders.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetERCOTAccountHolders(Action<List<string>, Exception> callback)
        {
            try
            {
                // loadDBCommands();
                List<string> holdersList = new List<string>();
                if (VayuConnection.State == System.Data.ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                SqlDataReader reader = mSelectERCOTExternalMP.ExecuteReader();
                while (reader.Read())
                {
                    holdersList.Add(reader.GetString(0));
                }
                reader.Close();
                // VayuConnection.Close();
                callback(holdersList, null);
            }
            catch (Exception ex)
            {
            }
        }



        public PeriodHours GetPeriodHours(DateTime startDate, DateTime endDate, int? Key)
        {
            loadDBCommands();
            PeriodHours periodHours = new PeriodHours();
            if (Key != 9)
            {
                using (SqlConnection con = VayuConnection)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = " select PeakHrs , OffPeakHrs from Period where PeriodType = 'Monthly' and StartDate = '" + startDate.ToString() + "' and EndDate = '" + endDate.ToString() + "'";
                        cmd.Connection = con;
                        con.Open();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            periodHours.peakHours = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                            periodHours.offpeakHours = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(1));
                        }
                        rdr.Close();
                        con.Close();
                    }
                }
            }
            else
            {
                using (SqlConnection con = VayuConnection)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = " select PeakHrs , OffPeakHrs, PeakWE from Period where PeriodType = 'Monthly' and StartDate = '" + startDate.ToString() + "' and EndDate = '" + endDate.ToString() + "'";
                        cmd.Connection = con;
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            periodHours.peakHours = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(0));
                            periodHours.offpeakHours = rdr.IsDBNull(0) ? 0 : Convert.ToInt32(rdr.GetValue(1));
                            periodHours.peakWEHours = rdr.IsDBNull(2) ? 0 : Convert.ToInt32(rdr.GetValue(2));
                        }
                        rdr.Close();
                        con.Close();
                    }
                }
            }
            return periodHours;
        }

        public Dictionary<int, Dictionary<string, List<ExposureHelper>>> GetExposures(List<int> nodeList)
        {
            // loadDBCommands();
            Dictionary<int, Dictionary<string, List<ExposureHelper>>> exposureDict = new Dictionary<int, Dictionary<string, List<ExposureHelper>>>();
            string nodeKeyString = string.Empty;
            int count = 0;
            foreach (int nodeKey in nodeList)
            {
                count++;
                if (count < nodeList.Count)
                {
                    nodeKeyString = nodeKeyString + nodeKey.ToString() + ',';
                }
                else
                    nodeKeyString = nodeKeyString + nodeKey.ToString();
            }
            using (SqlConnection con = VayuConnection)
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select distinct b.ConstraintRTNum , ConstraintText , a.ContingencyText , v.NodeKey , v.Sensitivity , b.ShiftFactor , b.DollarImpact"
                        + " from  ConstraintRT a  join RTMasterConstraint b on a.ConstraintText = b.MonitoredText join RTMasterVector_new v on b.ConstraintRTNum = v.ConstraintRTNum " +
                        " and a.ContingencyText = b.ContingencyText and v.NodeKey in (" + nodeKeyString + ") where a.MarketDateTime between  '" + DateTime.Today.AddDays(-(DateTime.Today.Day - 1)).ToString() + "' and  '" + DateTime.Today.ToString() + "'";
                    cmd.Connection = con;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    cmd.CommandTimeout = 30 * 1000;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int constraintId = Convert.ToInt32(rdr.GetValue(0));
                        string constraint = rdr.GetValue(1).ToString();
                        string contingency = rdr.GetValue(2).ToString();
                        int nodeKey = Convert.ToInt32(rdr.GetValue(3));
                        double sensitivity = Convert.ToDouble(rdr.GetValue(4));
                        double shiftFactor = Convert.ToDouble(rdr.GetValue(5));
                        double dollarImpact = Convert.ToDouble(rdr.GetValue(6));
                        if (!exposureDict.ContainsKey(constraintId))
                        {
                            Dictionary<string, List<ExposureHelper>> tempDict = new Dictionary<string, List<ExposureHelper>>();
                            ExposureHelper exp = new ExposureHelper();
                            exp.nodeKey = nodeKey;
                            exp.Sensitivity = sensitivity;
                            exp.ShiftFactor = shiftFactor;
                            exp.DollarImpact = dollarImpact;
                            List<ExposureHelper> exposureList = new List<ExposureHelper>();
                            exposureList.Add(exp);
                            tempDict.Add(constraint + '?' + contingency, exposureList);
                            exposureDict.Add(constraintId, tempDict);
                        }
                        else
                        {
                            Dictionary<string, List<ExposureHelper>> tempDict = exposureDict[constraintId];
                            List<ExposureHelper> exposureList = tempDict[constraint + '?' + contingency];
                            bool exists = exposureList.Exists(a => a.nodeKey == nodeKey);
                            if (!exists)
                            {
                                ExposureHelper exp = new ExposureHelper();
                                exp.nodeKey = nodeKey;
                                exp.Sensitivity = sensitivity;
                                exp.ShiftFactor = shiftFactor;
                                exp.DollarImpact = dollarImpact;
                                exposureList.Add(exp);
                            }

                        }
                    }
                    rdr.Close();
                    con.Close();
                }
            }
            return exposureDict;
        }
        /// <summary>
        /// Gets the market participants.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="marketKey">The market key.</param>
        public void GetMarketParticipants(Action<List<string>, Exception> callback, int marketKey)
        {
            try
            {
                loadDBCommands();
                List<string> participantsList = new List<string>();
                IDataReader reader = null;
                if (marketKey == 9)
                {
                    if (VayuConnection.State == System.Data.ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    //mSelectExternalMP.Parameters["@marketkey"].Value = marketKey;
                    reader = mSelectExternalMP.ExecuteReader();
                    while (reader.Read())
                    {
                        participantsList.Add(reader.GetString(0));
                    }
                    reader.Close();
                }
                callback(participantsList, null);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                try
                {
                    VayuConnection.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public List<string> Get6MonthAuctionDate(string year)
        {
            List<string> list = new List<string>();
            try
            {

                loadDBCommands();
                IDataReader reader = null;
                // if (marketKey == 9)
                {
                    if (VayuConnection.State == System.Data.ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    sSelect6MonthDate.CommandText = "select distinct(AuctionName) from CRRAnnualAuction where AuctionName like '%" + year + "%' order by AuctionName";
                    //sSelect6MonthDate.Parameters["@year"].Value = year;
                    reader = sSelect6MonthDate.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(reader.GetString(0));
                    }
                    reader.Close();
                }

            }
            catch (Exception ex)
            {
            }
            finally
            {
                try
                {
                    VayuConnection.Close();
                }
                catch (Exception ex)
                {
                }
            }
            return list;
        }

        public List<int> GetAnnualAuctionKey(string round, string year)
        {
            List<int> list = new List<int>();
            try
            {

                loadDBCommands();
                IDataReader reader = null;
                // if (marketKey == 9)
                {
                    if (VayuConnection.State == System.Data.ConnectionState.Closed)
                    {
                        VayuConnection.Open();
                    }
                    sSelect6MonthAuctionKey.CommandText = "select CRRAuctionKey from crrauction where  CRRAuctionName like '%" + year + "%' and CRRAuctionType='Annual' and AuctionRound in (" + round + ")";
                    //sSelect6MonthDate.Parameters["@year"].Value = year;
                    reader = sSelect6MonthAuctionKey.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)));
                    }
                    reader.Close();
                }

            }
            catch (Exception ex)
            {
            }
            finally
            {
                try
                {
                    VayuConnection.Close();
                }
                catch (Exception ex)
                {
                }
            }
            return list;
        }
        /// <summary>
        /// Gets the date list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public void GetDateList(Action<List<DateTime>, Exception> callback, int marketKey)
        {
            try
            {
                loadDBCommands();
                List<DateTime> monthList = new List<DateTime>();
                string strip;
                if (VayuConnection.State == System.Data.ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                IDataReader reader = null;
                if (marketKey == 9)
                {
                    reader = mSelectErcotMonthCommand.ExecuteReader();
                }

                while (reader.Read())
                {
                    strip = "1/1/" + reader.GetDecimal(0).ToString(); ;
                    monthList.Add(DateTime.Parse(strip));
                }
                reader.Close();
                //VayuConnection.Close();
                callback(monthList, null);
            }
            catch (Exception ex)
            {
            }
        }

        public Dictionary<int, PeriodDays> GetPeriodDays(string SelectedMarket)
        {
            loadDBCommands();
            Dictionary<int, PeriodDays> listperdiodays = new Dictionary<int, PeriodDays>();
            mSelectDaysCommand = new SqlCommand();
            if (SelectedMarket == "ERCOT")
                mSelectDaysCommand.CommandText = "select periodKey, peakhrs/16, offpeakhrs/8, PeakWE/16 from Period ";
            mSelectDaysCommand.Connection = VayuConnection;
            try
            {

                if (VayuConnection.State == System.Data.ConnectionState.Closed)
                {
                    VayuConnection.Open();
                }
                SqlDataReader reader = mSelectDaysCommand.ExecuteReader();
                while (reader.Read())
                {
                    PeriodDays objPeriodDays = new PeriodDays();
                    int key = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                    objPeriodDays.PeakDay = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
                    objPeriodDays.OffPeakDay = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));
                    if (SelectedMarket == "ERCOT")
                        objPeriodDays.PeakWEDay = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
                    listperdiodays.Add(key, objPeriodDays);
                }
                reader.Close();
                // VayuConnection.Close();

            }
            catch (Exception ex)
            {
            }

            return listperdiodays;
        }
    }

    public class FTRExposure
    {
        public int ConstraintId { get; set; }
        public string Constraint { get; set; }
        public string Contingency { get; set; }
        public double TotalMWExposure { get; set; }
        public double TotalDollarExposure { get; set; }
        public double PeakMWExposure { get; set; }
        public double OffPeakMWExposure { get; set; }
        public double PeakDollarExposure { get; set; }
        public double OffPeakDollarExposure { get; set; }
    }
    public class PathExposure
    {
        public string Source { get; set; }
        public string Sink { get; set; }
        public string ClassType { get; set; }
        public double MW { get; set; }
        public double TotalMWExposure { get; set; }
        public double TotalDollarExposure { get; set; }

        public string Participant { get; set; }

        public string Auction { get; set; }

        public string Period { get; set; }

    }

    public class ExposureHelper
    {
        public string ConstraintName { get; set; }
        public string ContingencyName { get; set; }

        public int nodeKey { get; set; }
        public double Sensitivity { get; set; }
        public double ShiftFactor { get; set; }
        public double DollarImpact { get; set; }
    }

    public class PeriodHours
    {
        public int peakHours { get; set; }
        public int offpeakHours { get; set; }
        public int peakWEHours { get; set; }
    }
    public class PeriodDays
    {
        public int PeakDay { get; set; }
        public int OffPeakDay { get; set; }
        public int PeakWEDay { get; set; }
    }
}
