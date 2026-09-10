using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;
using Vayu.DBLibrary;

namespace Vayu.CRRAnalysis.Model
{
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;
        private SqlCommand mSelectFtrErcotAuctionCommand;
        private SqlCommand mSelectNewPortfolioErcotFtrAuctionCommand;
        private SqlCommand mSelectNewPortfolioFtrAuctionCommand;
        private SqlCommand mSelectDistinctErcotFTRCommand;
        private SqlCommand mSelectPortfolioCommand;
        private SqlCommand mSelectAuctionKeysCommand;
        private SqlCommand mSelectAuctionCommand;
        private SqlCommand mSelectOptionsCommand;
        private SqlCommand mSelectAuctionNodesCommand;
        private SqlCommand mSelectAuctionOptionCommand;
        public void LoadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            mSelectNewPortfolioFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioFtrAuctionCommand.CommandText = "select distinct(Auction) from ftrbids where MarketKey = @Marketkey and Auction in (select FTRAuctionName from PJM.FTRAuction where convert(date, GETDATE()) between DATEADD(DD, -1, AuctionStartDate) and AuctionEndDate)";
            mSelectNewPortfolioFtrAuctionCommand.Parameters.AddWithValue("@Marketkey", "MarketKey");
            mSelectNewPortfolioFtrAuctionCommand.Connection = VayuConnection;

            //
            mSelectNewPortfolioErcotFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioErcotFtrAuctionCommand.CommandText = "select distinct(Auction) from crrbids where MarketKey=@marketkey and Auction in(select CRRAuctionName from crrAuction where convert(date, GETDATE()) between AuctionStartDate and AuctionEndDate )";
            mSelectNewPortfolioErcotFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNewPortfolioErcotFtrAuctionCommand.Connection = VayuConnection;

            //
            mSelectNewPortfolioErcotFtrAuctionCommand = new SqlCommand();
            mSelectNewPortfolioErcotFtrAuctionCommand.CommandText = "select distinct(Auction) from crrbids where MarketKey=@marketkey and Auction in(select CRRAuctionName from crrAuction where convert(date, GETDATE()) between AuctionStartDate and AuctionEndDate )";
            mSelectNewPortfolioErcotFtrAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
            mSelectNewPortfolioErcotFtrAuctionCommand.Connection = VayuConnection;

            mSelectDistinctErcotFTRCommand = new SqlCommand();
            mSelectDistinctErcotFTRCommand.CommandText = "select CRRAuctionKey, CRRAuctionName from CRRAuction where auctionenddate >= convert(date,getdate())";

            //mSelectDistinctErcotFTRCommand.CommandText = "select CRRAuctionKey, CRRAuctionName from CRRAuction where auctionenddate >= '2020-12-01'";
            mSelectDistinctErcotFTRCommand.Parameters.AddWithValue("@isocode", "isocode");
            mSelectDistinctErcotFTRCommand.Connection = VayuConnection;


            //
            mSelectPortfolioCommand = new SqlCommand();
            mSelectPortfolioCommand.CommandText = "select Portfolio_ID, Strip from Portfolio where Product = 'FTR' and Active = 'Y' And PORTFOLIO_ID<>3333 and hub=@hub  ";
            mSelectPortfolioCommand.Parameters.AddWithValue("@hub", "hub");
            mSelectPortfolioCommand.Connection = VayuConnection;

            mSelectAuctionKeysCommand = new SqlCommand();
            mSelectAuctionKeysCommand.CommandText = "select top 2 CRRAuctionKey, CRRAuctionName from CRRAuction where auctionenddate<(select AuctionEndDate from " +
                                        "CRRAuction where CRRAuctionName = @CRRAuctionName) and CRRAuctionType = 'Monthly' " +
                                        "ORDER BY auctionenddate DESC";
            mSelectAuctionKeysCommand.Parameters.AddWithValue("@CRRAuctionName", "CRRAuctionName");
            mSelectAuctionKeysCommand.Connection = VayuConnection;

            mSelectAuctionCommand = new SqlCommand();
            mSelectAuctionCommand.CommandText = "select  SourceName, SinkName, Hedge, TimeUse, ShadowPrice from CRRAuctionResults where CRRAuctionkey = @CRRAuctionkey";
            mSelectAuctionCommand.Parameters.AddWithValue("@CRRAuctionkey", "CRRAuctionkey");
            mSelectAuctionCommand.Connection = VayuConnection;

            mSelectAuctionNodesCommand = new SqlCommand();

            //mSelectAuctionNodesCommand.Parameters.AddWithValue("@CRRAuctionkey", "CRRAuctionkey");
            //mSelectAuctionNodesCommand.Parameters.AddWithValue("@NodeName", "NodeName");
            mSelectAuctionNodesCommand.Connection = VayuConnection;



            mSelectOptionsCommand = new SqlCommand();
            mSelectOptionsCommand.CommandText = "select  Source, Sink, HedgeType, TimeOfUse, sum(MW) as mw from CRROwnershipofRecord where startdate<= @startdate and  startdate<= @enddate " +
                "   and   enddate >=@enddate group by Source, Sink, HedgeType, TimeOfUse";

            mSelectOptionsCommand.Parameters.AddWithValue("@startdate", "startdate");
            mSelectOptionsCommand.Parameters.AddWithValue("@enddate", "enddate");
            mSelectOptionsCommand.Connection = VayuConnection;

        }
        public List<string> GetAuctionList(bool chkNewPortFolio)
        {
            LoadDBCommands();
            List<string> auctionList = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand mSelectFtrAuctionCommandFinal = new SqlCommand();
            SqlDataReader reader = null;
            if (!chkNewPortFolio)
            {
                mSelectFtrErcotAuctionCommand = new SqlCommand();
                mSelectFtrErcotAuctionCommand.Connection = VayuConnection;
                mSelectFtrErcotAuctionCommand.CommandText = "select distinct auction,[Month],a.CRRAuctionStartDate from CRRBids f join " +
                    "CRRAuction a on f.Auction = a.CRRAuctionName where marketkey = @marketkey order by a.CRRAuctionStartDate desc";
                mSelectFtrErcotAuctionCommand.Parameters.AddWithValue("@marketkey", "marketkey");
                mSelectFtrErcotAuctionCommand.Parameters["@marketkey"].Value = 9;
                mSelectFtrAuctionCommandFinal = mSelectFtrErcotAuctionCommand;
                reader = mSelectFtrAuctionCommandFinal.ExecuteReader();
            }
            else
            {
                mSelectNewPortfolioErcotFtrAuctionCommand.Parameters["@marketkey"].Value = 9;
                reader = mSelectNewPortfolioErcotFtrAuctionCommand.ExecuteReader();
            }
            while (reader.Read())
            {
                auctionList.Add(reader.GetString(0));
            }
            reader.Close();
            VayuConnection.Close();
            return auctionList.Distinct().ToList();
        }

        public DateTime GetAuctionStartDate(int marketKey, string auctionName)
        {
            LoadDBCommands();
            DateTime strStartDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1);
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlCommand cmd = new SqlCommand(); // mSelectIsoCommand.Parameters["@auctionname"].Value = name;
            if (marketKey == 9)
            {
                cmd = VayuConnection.CreateCommand();
                cmd.CommandText = "select CRRAuctionStartDate from CRRAuction where CRRAuctionName = '" + auctionName + "'";
            }
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                strStartDate = reader.GetDateTime(0);
            }
            reader.Close();
            VayuConnection.Close();
            return strStartDate;
        }
        public List<string> GetNewAuctionList(bool chkNewPortFolio)
        {
            LoadDBCommands();
            List<string> auctionList = new List<string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlDataReader reader = null;
            if (chkNewPortFolio)
            {
                mSelectNewPortfolioFtrAuctionCommand.Parameters["@MarketKey"].Value = 9;
                reader = mSelectNewPortfolioFtrAuctionCommand.ExecuteReader();
                while (reader.Read())
                {
                    auctionList.Add(reader.GetString(0));
                }
                VayuConnection.Close();
            }
            return auctionList.Distinct().ToList();
        }

        public List<Tuple<int, string>> GetPortfolioList(string marketKey)
        {
            LoadDBCommands();
            List<Tuple<int, string>> portfolioList = new List<Tuple<int, string>>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectPortfolioCommand.Parameters["@hub"].Value = "Ercot";
            SqlDataReader reader = mSelectPortfolioCommand.ExecuteReader();
            while (reader.Read())
            {
                Tuple<int, string> portfolioTuple = new Tuple<int, string>((int)reader.GetValue(0), reader.GetString(1));
                portfolioList.Add(portfolioTuple);
            }
            reader.Close();
            VayuConnection.Close();
            return portfolioList;
        }

        public List<CRRAuction> GetFtrAuctions(string isoCode)
        {
            LoadDBCommands();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            SqlDataReader reader = null;
            List<CRRAuction> periodList = new List<CRRAuction>();

            mSelectDistinctErcotFTRCommand.Parameters["@isocode"].Value = isoCode;
            reader = mSelectDistinctErcotFTRCommand.ExecuteReader();

            while (reader.Read())
            {
                CRRAuction auction = new CRRAuction();
                auction.Key = (int)reader.GetDecimal(0);
                auction.Name = reader.GetString(1);
                periodList.Add(auction);
            }
            reader.Close();
            VayuConnection.Close();
            return periodList;
        }

        public Dictionary<int, string> GetPrevKeys(string Auction)
        {
            LoadDBCommands();
            Dictionary<int, string> keyList = new Dictionary<int, string>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectAuctionKeysCommand.Parameters["@CRRAuctionName"].Value = Auction;
            SqlDataReader reader = mSelectAuctionKeysCommand.ExecuteReader();
            while (reader.Read())
            {
                keyList.Add((int)reader.GetDecimal(0), reader.GetString(1));
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }

        public Dictionary<string, double> GetPrevData(int Key)
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectAuctionCommand.Parameters["@CRRAuctionkey"].Value = Key;
            SqlDataReader reader = mSelectAuctionCommand.ExecuteReader();
            //SourceName, SinkName, Hedge, TimeUse, ShadowPrice
            while (reader.Read())
            {
                string key = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(2).Trim() + reader.GetString(3).Trim()).ToLower();
                if (!keyList.ContainsKey(key))
                    keyList.Add(key, Convert.ToDouble(reader.GetValue(4)));
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }
        public Dictionary<string, double> GetPrevNodeData(int Key, string nodenames)
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            //mSelectAuctionNodesCommand.Parameters["@CRRAuctionkey"].Value = Key;
            //mSelectAuctionNodesCommand.Parameters["@NodeName"].Value = nodenames;
            mSelectAuctionNodesCommand.CommandText = "select  c.NodeName, d.NodeName, f.LMPOnPeak, f.LMPOffPeak, f.PeakWE from (select  a.NodeKey as SourceNodeKey, b.nodekey as SinkNodeKey,( a.LMPOnPeak-b.LMPOnPeak ) as LMPOnPeak, " +
             "( a.LMPOffPeak -b.LMPOffPeak ) as LMPOffPeak, ( a.PeakWE -b.PeakWE ) as PeakWE from CRRAuctionNodePrice a, CRRAuctionNodePrice b where a.CRRAuctionKey=" + Key + "  and b.CRRAuctionKey=" + Key + " and a.NodeKey!=b.NodeKey) f join node c on" +
             " f.SourceNodeKey=c.NodeKey join node d on f.SinkNodeKey=d.NodeKey where c.NodeName in(" + nodenames + ") or d.NodeName in(" + nodenames + ") ";
            SqlDataReader reader = mSelectAuctionNodesCommand.ExecuteReader();
            //SourceName, SinkName, Hedge, TimeUse, ShadowPrice
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "OBL" + "PeakWD").ToLower();
                string keyoffpeak = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "OBL" + "Off-peak").ToLower();
                string keywe = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "OBL" + "PeakWE").ToLower();
                if (!keyList.ContainsKey(keywd))
                    keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));

                if (!keyList.ContainsKey(keyoffpeak))
                    keyList.Add(keyoffpeak, Convert.ToDouble(reader.GetValue(3)));

                if (!keyList.ContainsKey(keywe))
                    keyList.Add(keywe, Convert.ToDouble(reader.GetValue(4)));

            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }


        public Dictionary<string, double> GetPrevOptionData(int Key)
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            //mSelectAuctionNodesCommand.Parameters["@CRRAuctionkey"].Value = Key;
            //mSelectAuctionNodesCommand.Parameters["@NodeName"].Value = nodenames;
            mSelectAuctionOptionCommand = new SqlCommand();
            mSelectAuctionOptionCommand.Connection = VayuConnection;
            mSelectAuctionOptionCommand.CommandText = "select b.NodeName, c.NodeName, a.PeakWD, a.OffPeak, a.PeakWE from CRRAuctionOptionPrices a join node b on a.Sourcekey=b.NodeKey join node c on a.Sinkkey=c.NodeKey where CRRAuctionKey=" + Key;
            SqlDataReader reader = mSelectAuctionOptionCommand.ExecuteReader();
            //SourceName, SinkName, Hedge, TimeUse, ShadowPrice
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "OPT" + "PeakWD").ToLower();
                string keyoffpeak = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "OPT" + "Off-peak").ToLower();
                string keywe = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "OPT" + "PeakWE").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                }

                if (!keyList.ContainsKey(keyoffpeak))
                {
                    if (!reader.IsDBNull(3))
                        keyList.Add(keyoffpeak, Convert.ToDouble(reader.GetValue(3)));
                }

                if (!keyList.ContainsKey(keywe))
                {
                    if (!reader.IsDBNull(4))
                        keyList.Add(keywe, Convert.ToDouble(reader.GetValue(4)));
                }

            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }

        public Dictionary<string, double> GetPrevOptionsData(DateTime date)
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectOptionsCommand.Parameters["@startdate"].Value = date;
            mSelectOptionsCommand.Parameters["@enddate"].Value = date.AddMonths(1).AddDays(-1);
            SqlDataReader reader = mSelectOptionsCommand.ExecuteReader();
            //SourceName, SinkName, Hedge, TimeUse, ShadowPrice
            while (reader.Read())
            {
                string key = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(2).Trim() + reader.GetString(3).Trim()).ToLower();
                if (!keyList.ContainsKey(key))
                    keyList.Add(key, Convert.ToDouble(reader.GetValue(4)));
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }

        public Dictionary<string, double> GetClearedOptionData(int Key)
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectAuctionOptionCommand = new SqlCommand();
            mSelectAuctionOptionCommand.Connection = VayuConnection;
            mSelectAuctionOptionCommand.CommandText = "   select b.NodeName, c.NodeName, sum(a.MWWD), a.HedgeType from CRRAuctionOptionPrices " +
                "a join node b on a.Sourcekey = b.NodeKey join node c on a.Sinkkey = c.NodeKey where CRRAuctionKey =  " + Key + ""
            + "and a.PeakWD < a.BidPricePeakWD group by b.NodeName, c.NodeName, HedgeType";

            SqlDataReader reader = mSelectAuctionOptionCommand.ExecuteReader();

            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(3).Trim() + "PeakWD").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                }
            }
            reader.Close();
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            mSelectAuctionOptionCommand = new SqlCommand();
            mSelectAuctionOptionCommand.Connection = VayuConnection;
            mSelectAuctionOptionCommand.CommandText = " select b.NodeName, c.NodeName, sum(a.MWOffP),   a.HedgeType from CRRAuctionOptionPrices "
           + " a join node b on a.Sourcekey = b.NodeKey join node c on a.Sinkkey = c.NodeKey where CRRAuctionKey =  " + Key + ""
            + " and a.OffPeak < a.BidPriceOffpeak  group by b.NodeName, c.NodeName, HedgeType";
            reader = mSelectAuctionOptionCommand.ExecuteReader();

            while (reader.Read())
            {
                string keyoffpeak = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(3).Trim() + "Off-peak").ToLower();
                if (!keyList.ContainsKey(keyoffpeak))
                {
                    if (!reader.IsDBNull(2))
                        keyList.Add(keyoffpeak, Convert.ToDouble(reader.GetValue(2)));
                }
            }
            reader.Close();

            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            mSelectAuctionOptionCommand = new SqlCommand();
            mSelectAuctionOptionCommand.Connection = VayuConnection;
            mSelectAuctionOptionCommand.CommandText = "select b.NodeName, c.NodeName, sum(a.MWWE),   a.HedgeType from CRRAuctionOptionPrices "
            + " a join node b on a.Sourcekey = b.NodeKey join node c on a.Sinkkey = c.NodeKey where CRRAuctionKey = " + Key + ""
             + " and a.PeakWE < a.BidPricePeakWE  group by b.NodeName, c.NodeName, HedgeType";

            reader = mSelectAuctionOptionCommand.ExecuteReader();

            while (reader.Read())
            {
                string keywe = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(3).Trim() + "PeakWE").ToLower();
                if (!keyList.ContainsKey(keywe))
                {
                    if (!reader.IsDBNull(2))
                        keyList.Add(keywe, Convert.ToDouble(reader.GetValue(2)));
                }
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }


        public Dictionary<string, double> GetSubmittedOptionData(int Key)
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            mSelectAuctionOptionCommand = new SqlCommand();
            mSelectAuctionOptionCommand.Connection = VayuConnection;
            mSelectAuctionOptionCommand.CommandText = "select distinct b.NodeName, c.NodeName, (a.PeakWDMW), (a.OffPeakMW), (a.PeakWEMW) ,a.HedgeType" +
                " from CRRAuctionOptionPrices a join node b on a.Sourcekey = b.NodeKey join node c on a.Sinkkey = c.NodeKey where CRRAuctionKey = " + Key + "";
            //  " group by b.NodeName, c.NodeName,a.HedgeType";
            SqlDataReader reader = mSelectAuctionOptionCommand.ExecuteReader();
            //SourceName, SinkName, Hedge, TimeUse, ShadowPrice
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(5).Trim() + "PeakWD").ToLower();
                string keyoffpeak = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(5).Trim() + "Off-peak").ToLower();
                string keywe = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(5).Trim() + "PeakWE").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                    {
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
                if (!keyList.ContainsKey(keyoffpeak))
                {
                    if (!reader.IsDBNull(3))
                    {
                        keyList.Add(keyoffpeak, Convert.ToDouble(reader.GetValue(3)));
                    }
                }
                if (!keyList.ContainsKey(keywe))
                {
                    if (!reader.IsDBNull(4))
                    {
                        keyList.Add(keywe, Convert.ToDouble(reader.GetValue(4)));
                    }
                }
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }

        public Dictionary<string, double> GetMin()
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            SqlCommand mSelectMinDACommand = new SqlCommand();
            mSelectMinDACommand.Connection = VayuConnection;
            mSelectMinDACommand.CommandText = "select SourceName,SinkName,MinDA from ErcotNodeMinMax where TimeOfUse='Peakwd' ";
            SqlDataReader reader = mSelectMinDACommand.ExecuteReader();
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "PeakWD").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                    {
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
            }

            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            mSelectMinDACommand = new SqlCommand();
            mSelectMinDACommand.Connection = VayuConnection;
            mSelectMinDACommand.CommandText = "select SourceName,SinkName,MinDA from ErcotNodeMinMax where TimeOfUse='off-peak' ";
            reader = mSelectMinDACommand.ExecuteReader();
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "Off-peak").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                    {
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
            }
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            mSelectMinDACommand = new SqlCommand();
            mSelectMinDACommand.Connection = VayuConnection;
            mSelectMinDACommand.CommandText = "select SourceName,SinkName,MinDA from ErcotNodeMinMax where TimeOfUse='Peakwe' ";
            reader = mSelectMinDACommand.ExecuteReader();
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "PeakWE").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                    {
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }

        public Dictionary<string, double> GetMax()
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }

            SqlCommand mSelectMinDACommand = new SqlCommand();
            mSelectMinDACommand.Connection = VayuConnection;
            mSelectMinDACommand.CommandText = "select SourceName,SinkName,MaxDA from ErcotNodeMinMax where TimeOfUse='Peakwd' ";
            SqlDataReader reader = mSelectMinDACommand.ExecuteReader();
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "PeakWD").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                    {
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
            }

            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            mSelectMinDACommand = new SqlCommand();
            mSelectMinDACommand.Connection = VayuConnection;
            mSelectMinDACommand.CommandText = "select SourceName,SinkName,MaxDA from ErcotNodeMinMax where TimeOfUse='OFF-PEAK' ";
            reader = mSelectMinDACommand.ExecuteReader();
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "Off-peak").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                    {
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
            }
            if (VayuConnection.State == ConnectionState.Open)
            {
                VayuConnection.Close();
            }
            VayuConnection.Open();
            mSelectMinDACommand = new SqlCommand();
            mSelectMinDACommand.Connection = VayuConnection;
            mSelectMinDACommand.CommandText = "select SourceName,SinkName,MaxDA from ErcotNodeMinMax where TimeOfUse='Peakwe' ";
            reader = mSelectMinDACommand.ExecuteReader();
            while (reader.Read())
            {
                string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + "PeakWE").ToLower();
                if (!keyList.ContainsKey(keywd))
                {
                    if (!reader.IsDBNull(2))
                    {
                        keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }

        public Dictionary<string, double> GetMedian45()
        {
            LoadDBCommands();
            Dictionary<string, double> keyList = new Dictionary<string, double>();
            if (VayuConnection.State == ConnectionState.Closed)
            {
                VayuConnection.Open();
            }
            mSelectAuctionOptionCommand = new SqlCommand();
            mSelectAuctionOptionCommand.Connection = VayuConnection;
            mSelectAuctionOptionCommand.CommandText = "select SourceName,SinkName,Median45,TimeOfUse from ErcotNodeMedian45 ";
            SqlDataReader reader = mSelectAuctionOptionCommand.ExecuteReader();
            //SourceName, SinkName, Hedge, TimeUse, ShadowPrice
            while (reader.Read())
            {
                try
                {
                    string keywd = (reader.GetString(0).Trim() + reader.GetString(1).Trim() + reader.GetString(3)).ToLower();
                    if (!keyList.ContainsKey(keywd))
                    {
                        if (!reader.IsDBNull(2))
                            keyList.Add(keywd, Convert.ToDouble(reader.GetValue(2)));
                    }
                }
                catch (Exception ex)
                {

                }
            }
            reader.Close();
            VayuConnection.Close();
            return keyList;
        }

    }
}
