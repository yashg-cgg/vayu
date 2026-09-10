using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;

namespace Vayu.ERCOT_Market_Overview.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ImportHelper
    {
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        protected SqlConnection VayuDbConnection;
        /// <summary>
        /// The dic period
        /// </summary>
        protected Dictionary<string, Period> dicPeriod;
        protected Dictionary<string, Period> dicPeriodErcot;
        /// <summary>
        /// The dic results
        /// </summary>
        protected Dictionary<string, int> dicResults;
        protected Dictionary<string, int> dicResultsErcot;
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public abstract int MarketKey { get; }
        /// <summary>
        /// Gets the participant.
        /// </summary>
        /// <value>
        /// The participant.
        /// </value>
        public abstract string Participant { get; }
        /// <summary>
        /// Gets the select period command.
        /// </summary>
        /// <value>
        /// The select period command.
        /// </value>
        public abstract string SelectPeriodCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportHelper"/> class.
        /// </summary>
        public ImportHelper()
        {

            VayuDbConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            dicPeriod = new Dictionary<string, Period>();
            dicResults = new Dictionary<string, int>();

            dicPeriodErcot = new Dictionary<string, Period>();
            dicResultsErcot = new Dictionary<string, int>();
        }

        #region Public Methods

        /// <summary>
        /// Gets the name of the period by.
        /// </summary>
        /// <param name="periodName">Name of the period.</param>
        /// <returns></returns>
        public virtual Period GetPeriodByName(string periodName)
        {
            if (dicPeriod.ContainsKey(periodName))
                return dicPeriod[periodName];

            Period p = null;
            SqlCommand cmd = VayuDbConnection.CreateCommand();
            try
            {
                cmd.CommandText = SelectPeriodCommand;
                cmd.Parameters.AddWithValue("@PeriodName", periodName);
                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    p = new Period();
                    p.PeriodKey = (int)reader.GetInt32(0);
                    p.PeriodName = reader[1].ToString();
                    p.PeakHours = (int)reader.GetInt32(2);

                    p.OffPeakHours = (int)reader.GetInt32(3);
                    p.Date = reader.GetDateTime(4);
                }
                reader.Close();

            }
            catch { }
            finally { cmd.Connection.Close(); }

            if (p != null)
                dicPeriod.Add(periodName, p);

            return p;
        }
        public virtual Period GetPeriodErcotByName(string periodName)
        {
            if (dicPeriodErcot.ContainsKey(periodName))
                return dicPeriodErcot[periodName];

            Period p = null;
            SqlCommand cmd = VayuDbConnection.CreateCommand();
            try
            {
                cmd.CommandText = SelectPeriodCommand;
                cmd.Parameters.AddWithValue("@PeriodName", periodName);
                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    p = new Period();
                    p.PeriodKey = (int)reader.GetInt32(0);
                    p.PeriodName = reader[1].ToString();
                    p.PeakHours = (int)reader.GetInt32(2);
                    p.OffPeakHours = (int)reader.GetInt32(3);
                    p.Date = reader.GetDateTime(4);
                    p.PeakWEHours = (int)reader.GetInt32(5);
                }
                reader.Close();

            }
            catch { }
            finally { cmd.Connection.Close(); }

            if (p != null)
                dicPeriodErcot.Add(periodName, p);

            return p;
        }


        /// <summary>
        /// Gets the external identifier.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        public virtual long GetExternalID(string nodeName) { return 0; }
        /// <summary>
        /// Fills the TCR hash.
        /// </summary>
        /// <param name="month">The month.</param>
        public virtual void FillTCRHash(DateTime month) { }
        /// <summary>
        /// Constructs the by market.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="month">The month.</param>
        /// <returns></returns>
        public static ImportHelper ConstructByMarket(string market, DateTime month)
        {
            if (market.ToUpper() == "SPP")
            {
                SPPImport spp = new SPPImport();
                spp.FillTCRHash(month);
                return spp;
            }
            else if (market.ToUpper() == "MISO")
                return new MISOImport();
            else if (market.ToUpper() == "CAISO")
                return new CAISOImport();
            else if (market.ToUpper() == "ERCOT")
                return new ERCOTImport();
            else
                return new PJMImport();
        }
        /// <summary>
        /// Gets the tcrid.
        /// </summary>
        /// <param name="bid">The bid.</param>
        /// <returns></returns>


        public virtual int GetTCRErcotID(int portfolio)
        {
            DateTime now = DateTime.Now;


            //string key = BigInteger.Parse(portfolio.ToString() + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() +
            //now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + now.Millisecond.ToString());
            //if (!dicPeriodErcot.ContainsKey(key))
            //    return 0;

            //return dicPeriodErcot[key];
            int key = int.Parse(portfolio + now.Day.ToString() + now.Millisecond.ToString());
            return key;

        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.FTRWorkbookStatistics.ViewModel.ImportHelper" />
    public class CAISOImport : ImportHelper
    {
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public override int MarketKey
        {
            get
            {
                return 7;
            }
        }

        /// <summary>
        /// Gets the participant.
        /// </summary>
        /// <value>
        /// The participant.
        /// </value>
        public override string Participant
        {
            get
            {
                return "SIGMA";
            }
        }

        /// <summary>
        /// Gets the select period command.
        /// </summary>
        /// <value>
        /// The select period command.
        /// </value>
        public override string SelectPeriodCommand
        {
            get
            {
                return "select top 1 PeriodKey, PeriodName,PeakHrs,OffPeakHrs,StartDate from Period where MarketKey = 7 and PeriodName = @PeriodName and StartDate > GETDATE() order by StartDate ";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.FTRWorkbookStatistics.ViewModel.ImportHelper" />
    public class MISOImport : ImportHelper
    {
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public override int MarketKey
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets the participant.
        /// </summary>
        /// <value>
        /// The participant.
        /// </value>
        public override string Participant
        {
            get
            {
                return "SIGMA";
            }
        }

        /// <summary>
        /// Gets the select period command.
        /// </summary>
        /// <value>
        /// The select period command.
        /// </value>
        public override string SelectPeriodCommand
        {
            get
            {
                return "select top 1 PeriodKey, PeriodName,PeakHrs,OffPeakHrs,StartDate from Period where MarketKey = 2 and PeriodName = @PeriodName and StartDate > GETDATE() order by StartDate ";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.FTRWorkbookStatistics.ViewModel.ImportHelper" />
    public class PJMImport : ImportHelper
    {
        public override int MarketKey
        {
            get { return 1; }
        }

        public override string Participant
        {
            get { return "SIGMA"; }
        }

        public override string SelectPeriodCommand
        {
            get
            {
                return "select top 1 PeriodKey, PeriodName,PeakHrs,OffPeakHrs,StartDate from Period where MarketKey = 1 and PeriodName = @PeriodName and StartDate > GETDATE() order by StartDate ";
            }
        }

        /// <summary>
        /// Gets the external identifier.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        public override long GetExternalID(string nodeName)
        {
            PricingNode node = DBAccess.GetNodeFromName(nodeName, 1);
            if (node != null)
                return node.ExternalNodeId;
            else
                return 0;
        }
    }
    public class ERCOTImport : ImportHelper
    {
        public override int MarketKey
        {
            get
            {
                return 9;
            }
        }

        public override string Participant
        {
            get
            {
                return "SIGMAERCOT";
            }
        }

        public override string SelectPeriodCommand
        {
            get
            {
                return "select top 1 PeriodKey, PeriodName,PeakHrs,OffPeakHrs,StartDate , PeakWE from Period where MarketKey = 9 and PeriodName = @PeriodName and StartDate > GETDATE() order by StartDate ";
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.FTRWorkbookStatistics.ViewModel.ImportHelper" />
    public class SPPImport : ImportHelper
    {
        /// <summary>
        /// Gets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public override int MarketKey
        {
            get
            {
                return 12;
            }
        }

        /// <summary>
        /// Gets the participant.
        /// </summary>
        /// <value>
        /// The participant.
        /// </value>
        public override string Participant
        {
            get
            {
                return "SIGMA";
            }
        }

        /// <summary>
        /// Gets the select period command.
        /// </summary>
        /// <value>
        /// The select period command.
        /// </value>
        public override string SelectPeriodCommand
        {
            get
            {
                return "select top 1 PeriodKey, PeriodName,PeakHrs,OffPeakHrs,StartDate from Period where MarketKey = 12 and PeriodName = @PeriodName and StartDate > GETDATE() order by StartDate ";
            }
        }

        /// <summary>
        /// Fills the TCR hash.
        /// </summary>
        /// <param name="month">The month.</param>
        public override void FillTCRHash(DateTime month)
        {
            int ftrKey = 0;
            SqlCommand selectMaxFTR = VayuDbConnection.CreateCommand();
            try
            {
                selectMaxFTR.CommandText = "select distinct  top 1 a.FTRAuctionKey , a.FTRAuctionStartDate, a.AuctionRound from SPP.FTRAuctionResults r join SPP.FTRAuction a on " +
                    " r.FTRAuctionKey = a.FTRAuctionKey where a.FTRAuctionType = 'annual' order by a.FTRAuctionStartDate desc, a.AuctionRound desc ";
                selectMaxFTR.Connection.Open();
                SqlDataReader reader = selectMaxFTR.ExecuteReader();
                if (reader.Read())
                {
                    ftrKey = (int)reader.GetDecimal(0);
                }
                reader.Close();
                selectMaxFTR.Connection.Close();

                if (ftrKey == 0)
                {
                    System.Windows.MessageBox.Show("Cannot find annual auction");
                    return;
                }

                selectMaxFTR.Connection.Open();

                Action<string> fill = (calssType) =>
                {
                    SqlDataReader resultsreader = selectMaxFTR.ExecuteReader();
                    while (resultsreader.Read())
                    {
                        string key = resultsreader[1].ToString() + resultsreader[2].ToString() + calssType;
                        int ftrID = 0;
                        if (int.TryParse(resultsreader[0].ToString(), out ftrID))
                            dicResults.Add(key, ftrID);
                    }
                    resultsreader.Close();
                };

                selectMaxFTR.CommandText = "select FTRID, SourceNode, SinkNode, ClassType from SPP.FTRAuctionResults r join " +
                    " Period p on r.PeriodKey = p.PeriodKey where FTRAuctionKey = " + ftrKey + " and Participant = 'Sigma' and TradeType = 'buy' and ClassType = 'peak' " +
                    " and p.StartDate <= '" + month.ToString("yyyy-MM-dd") + "' and p.EndDate >= '" + month.ToString("yyyy-MM-dd") + "'";
                fill("PEAK");

                selectMaxFTR.CommandText = "select FTRID, SourceNode, SinkNode, ClassType from SPP.FTRAuctionResults r join " +
                    " Period p on r.PeriodKey = p.PeriodKey where FTRAuctionKey = " + ftrKey + " and Participant = 'Sigma' and TradeType = 'buy' and ClassType <> 'peak' " +
                    " and p.StartDate <= '" + month.ToString("yyyy-MM-dd") + "' and p.EndDate >= '" + month.ToString("yyyy-MM-dd") + "'";
                fill("OFFPEAK");
            }
            catch (Exception ex) { }
            finally
            {
                selectMaxFTR.Connection.Close();
            }
        }


    }

    /// <summary>
    /// 
    /// </summary>
    public class Period
    {
        /// <summary>
        /// Gets or sets the period key.
        /// </summary>
        /// <value>
        /// The period key.
        /// </value>
        public int PeriodKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the period.
        /// </summary>
        /// <value>
        /// The name of the period.
        /// </value>
        public string PeriodName { get; set; }
        /// <summary>
        /// Gets or sets the peak hours.
        /// </summary>
        /// <value>
        /// The peak hours.
        /// </value>
        public int PeakHours { get; set; }
        /// <summary>
        /// Gets or sets the off peak hours.
        /// </summary>
        /// <value>
        /// The off peak hours.
        /// </value>
        public int OffPeakHours { get; set; }
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public int PeakWEHours { get; set; }
        public DateTime Date { get; set; }
    }
}
