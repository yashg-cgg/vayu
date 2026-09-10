using Vayu.CommonAccessLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.DBLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class DBAccess
    {
        #region Value Assignment to Private Variable
        /// <summary>
        /// The s user
        /// </summary>
        private static string sUser = Environment.UserName;

        /// <summary>
        /// The s bid identifier
        /// </summary>
        private static int sBidId = 1;

        #endregion

        #region Sql Connection for connecting DB
        /// <summary>
        /// The Vayu database connection
        /// </summary>
        private static SqlConnection VayuDBConnection;
        private static SqlCommand cmdSelectAllPath;
        #endregion

        #region Sql Commands for Database Query Execution
        /// <summary>
        /// The command select name date range
        /// </summary>
        private static SqlCommand cmdSelectNameDateRange;
        /// <summary>
        /// The command select date date range
        /// </summary>
        private static SqlCommand cmdSelectDateDateRange;
        /// <summary>
        /// The command select PJM constraint Sensitivity
        /// </summary>
        private static SqlCommand cmdSelectPJMConstraintSensitivity;
        /// <summary>
        /// The command select miso constraint Sensitivity
        /// </summary>
        private static SqlCommand cmdSelectMISOConstraintSensitivity;
        /// <summary>
        /// The command select contingency Sensitivity
        /// </summary>
        private static SqlCommand cmdSelectContingencySensitivity;
        /// <summary>
        /// The command select miso contingency Sensitivity
        /// </summary>
        private static SqlCommand cmdSelectNodeFromExternalCommand;

        private static SqlCommand cmdSelectMISOContingencySensitivity;
        /// <summary>
        /// The command select Sensitivity
        /// </summary>
        private static SqlCommand cmdSelectSensitivity;
        /// <summary>
        /// The command select miso Sensitivity
        /// </summary>
        private static SqlCommand cmdSelectMISOSensitivity;
        /// <summary>
        /// The command select node
        /// </summary>
        private static SqlCommand cmdSelectNode;
        /// <summary>
        /// The command select node from name
        /// </summary>
        private static SqlCommand cmdSelectNodeFromName;
        /// <summary>
        /// The command select node from external
        /// </summary>
        private static SqlCommand cmdSelectNodeFromExternal;
        /// <summary>
        /// The command select portfolio name
        /// </summary>
        private static SqlCommand cmdSelectPortfolioName;
        private static SqlCommand cmdSelectExternalPortfolioName;
        /// <summary>
        /// The command select portfolio
        /// </summary>
        private static SqlCommand cmdSelectPortfolio;
        /// <summary>
        /// The command select portfolio in ercot ees bids
        /// </summary>
        private static SqlCommand cmdSelectPortfolioInErcotEESBids;
        /// <summary>
        /// The command select portfolio in ees bids
        /// </summary>
        private static SqlCommand cmdSelectPortfolioInEESBids;
        /// <summary>
        /// The command select portfolio in virtual bids
        /// </summary>
        private static SqlCommand cmdSelectPortfolioInVirtualBids;
        /// <summary>
        /// The command select portfolio in virtual bids holder
        /// </summary>
        private static SqlCommand cmdSelectPortfolioInVirtualBidsHolder;
        /// <summary>
        /// The command select portfolio file virtual bids by user
        /// </summary>
        private static SqlCommand cmdSelectPortfolioFileVirtualBidsByUser;
        /// <summary>
        /// The command select portfolio file virtual bids
        /// </summary>
        private static SqlCommand cmdSelectPortfolioFileVirtualBids;
        /// <summary>
        /// The command select PJM uptos bid
        /// </summary>
        private static SqlCommand cmdSelectPjmUptosBid;
        /// <summary>
        /// The command select deenergized nodes
        /// </summary>
        private static SqlCommand cmdSelectDeenergizedNodes;
        /// <summary>
        /// The command select ercot ees bids
        /// </summary>
        private static SqlCommand cmdSelectErcotEESBids;
        /// <summary>
        /// The command select virtual bids
        /// </summary>
        private static SqlCommand cmdSelectVirtualBids;
        /// <summary>
        /// The command select miso virtual bids
        /// </summary>
        private static SqlCommand cmdSelectMisoVirtualBids;
        /// <summary>
        /// The command select load
        /// </summary>
        private static SqlCommand cmdSelectLoad;
        /// <summary>
        /// The command select weather
        /// </summary>
        private static SqlCommand cmdSelectWeather;
        /// <summary>
        /// The command select load RTH
        /// </summary>
        private static SqlCommand cmdSelectLoadRth;
        /// <summary>
        /// The command select load forecast
        /// </summary>
        private static SqlCommand cmdSelectLoadForecast;
        /// <summary>
        /// The command select weather forecast
        /// </summary>
        private static SqlCommand cmdSelectWeatherForecast;
        /// <summary>
        /// The command insert ees bids
        /// </summary>
        private static SqlCommand cmdInsertEESBids;
        /// <summary>
        /// The command insert ercot ees bids
        /// </summary>
        private static SqlCommand cmdInsertErcotEESBids;
        /// <summary>
        /// The command delete ees bids command
        /// </summary>
        private static SqlCommand cmdDeleteEESBidsCommand;
        /// <summary>
        /// The command delete ees bids
        /// </summary>
        private static SqlCommand cmdDeleteEESBids;
        /// <summary>
        /// The command delete ercot ees bids
        /// </summary>
        private static SqlCommand cmdDeleteErcotEESBids;
        /// <summary>
        /// The command delete ees bids by identifier
        /// </summary>
        private static SqlCommand cmdDeleteEESBidsById;
        /// <summary>
        /// The command delete file virtual bids
        /// </summary>
        private static SqlCommand cmdDeleteFileVirtualBids;
        /// <summary>
        /// The command delete ercot ees bids command
        /// </summary>
        private static SqlCommand cmdDeleteErcotEESBidsCommand;
        /// <summary>
        /// The command delete ercot ees bids by identifier
        /// </summary>
        private static SqlCommand cmdDeleteErcotEESBidsById;
        /// <summary>
        /// The command select source sink
        /// </summary>
        private static SqlCommand cmdSelectSourceSink;
        /// <summary>
        /// The command select source
        /// </summary>
        private static SqlCommand cmdSelectSource;
        /// <summary>
        /// The command select sink command
        /// </summary>
        private static SqlCommand cmdSelectSinkCommand;
        /// <summary>
        /// The command select source sink node
        /// </summary>
        private static SqlCommand cmdSelectSourceSinkNode;
        /// <summary>
        /// The command select weather cities
        /// </summary>
        private static SqlCommand cmdSelectWeatherCities;
        /// <summary>
        /// The command select load difference list
        /// </summary>
        private static SqlCommand cmdSelectLoadDiffList;
        /// <summary>
        /// The command select maximum bid identifier command
        /// </summary>
        private static SqlCommand cmdSelectMaxBidIdCommand;
        /// <summary>
        /// The command insert virtual bids
        /// </summary>
        private static SqlCommand cmdInsertVirtualBids;
        /// <summary>
        /// The command delete virtual bids
        /// </summary>
        private static SqlCommand cmdDeleteVirtualBids;
        /// <summary>
        /// The command delete valid virtual bids
        /// </summary>
        private static SqlCommand cmdDeleteValidVirtualBids;
        /// <summary>
        /// The command delete load threshold
        /// </summary>
        private static SqlCommand cmdDeleteLoadThreshold;
        /// <summary>
        /// The command insert load threshold
        /// </summary>
        private static SqlCommand cmdInsertLoadThreshold;
        /// <summary>
        /// The command select load threshold
        /// </summary>
        private static SqlCommand cmdSelectLoadThreshold;
        /// <summary>
        /// The command update submit virtual portfolio
        /// </summary>
        private static SqlCommand cmdUpdateSubmitVirtualPortfolio;
        /// <summary>
        /// The command select mw
        /// </summary>
        private static SqlCommand cmdSelectMW;
        /// <summary>
        /// The command select PNL fee
        /// </summary>
        private static SqlCommand cmdSelectPnlFee;
        /// <summary>
        /// The command insert FTR bid command
        /// </summary>
        private static SqlCommand cmdInsertPJMFtrBidCommand;
        /// <summary>
        /// The command select last fee
        /// </summary>
        private static SqlCommand cmdSelectLastFee;
        /// <summary>
        /// The command delete FTR bid
        /// </summary>
        private static SqlCommand cmdDeleteFtrBid;
        /// <summary>
        /// The command select FTR transaction
        /// </summary>
        private static SqlCommand cmdSelectFtrTransaction;
        /// <summary>
        /// The command select node type
        /// </summary>
        private static SqlCommand cmdSelectNodeType;
        /// <summary>
        /// The command select coordinate
        /// </summary>
        private static SqlCommand cmdSelectCoordinate;
        /// <summary>
        /// The command insert virtual bids holder
        /// </summary>
        private static SqlCommand cmdInsertVirtualBidsHolder;
        /// <summary>
        /// The command select FTR bid
        /// </summary>
        private static SqlCommand cmdSelectFtrBid;
        /// <summary>
        /// The CMS selectftr bidspeakoffpeak
        /// </summary>
        private static SqlCommand cmsSelectftrBidspeakoffpeak;
        private static SqlCommand cmsSelectftrBidspeakoffpeakErcot;
        private static SqlCommand cmsSelectftrPortfilio;
        /// <summary>
        /// The command select virtual bids node holder
        /// </summary>
        private static SqlCommand cmdSelectVirtualBidsNodeHolder;
        /// <summary>
        /// The command select virtual bids holder
        /// </summary>
        private static SqlCommand cmdSelectVirtualBidsHolder;
        /// <summary>
        /// The command select PJM rt families
        /// </summary>
        private static SqlCommand cmdSelectPjmRtFamilies;
        private static SqlCommand cmdSelectErcotRTFamilies;
        private static SqlCommand cmdSelectErcotDAFamilies;
        /// <summary>
        /// The command select PJM rt constraint numbers
        /// </summary>
        private static SqlCommand cmdSelectPjmRtConstraintNumbers;
        /// <summary>
        /// The command select similar load day
        /// </summary>
        private static SqlCommand cmdSelectSimilarLoadDay;
        /// <summary>
        /// The command select risk constraints
        /// </summary>
        private static SqlCommand cmdSelectRiskConstraints;
        /// <summary>
        /// The command delete virtual bids for update
        /// </summary>
        private static SqlCommand cmdDeleteVirtualBidsForUpdate;
        /// <summary>
        /// The command select uptos forfeiture
        /// </summary>
        private static SqlCommand cmdSelectUptosForfeiture;
        /// <summary>
        /// The command select virtual forfeiture
        /// </summary>
        private static SqlCommand cmdSelectVirtualForfeiture;
        /// <summary>
        /// The command select virtual cleared
        /// </summary>
        private static SqlCommand cmdSelectVirtualCleared;
        /// <summary>
        /// The command select upto cleared
        /// </summary>
        private static SqlCommand cmdSelectUptoCleared;
        private static SqlCommand cmdErcotSelectUptoCleared;
        private static SqlCommand cmdErcotExternalSelectUptoCleared;
        /// <summary>
        /// The command select portfolio from account
        /// </summary>
        private static SqlCommand cmdSelectPortfolioFromAccount;
        /// <summary>
        /// The command select all nodes
        /// </summary>
        private static SqlCommand cmdSelectAllNodes;

        private static SqlCommand cmdSelectAllErcotNodes;
        /// <summary>
        /// The command select current bids
        /// </summary>
        private static SqlCommand cmdSelectCurrentBids;
        /// <summary>
        /// The command select current uptos
        /// </summary>
        private static SqlCommand cmdSelectCurrentUptos;
        /// <summary>
        /// The command select virtual nodes
        /// </summary>
        private static SqlCommand cmdSelectVirtualNodes;
        /// <summary>
        /// The command select account by user
        /// </summary>
        private static SqlCommand cmdSelectAccountByUser;
        /// <summary>
        /// The command select account
        /// </summary>
        private static SqlCommand cmdSelectAccount;
        /// <summary>
        /// The command select portfolio cleared bids
        /// </summary>
        private static SqlCommand cmdSelectPortfolioClearedBids;
        /// <summary>
        /// The command select portfolio count
        /// </summary>
        private static SqlCommand cmdSelectPortfolioCount;
        /// <summary>
        /// The command select portfolio current bids
        /// </summary>
        private static SqlCommand cmdSelectPortfolioCurrentBids;
        /// <summary>
        /// The command select portfolio source uptos
        /// </summary>
        private static SqlCommand cmdSelectPortfolioSourceUptos;
        /// <summary>
        /// The command select portfolio sink uptos
        /// </summary>
        private static SqlCommand cmdSelectPortfolioSinkUptos;
        /// <summary>
        /// The command select user
        /// </summary>
        private static SqlCommand cmdSelectUser;
        /// <summary>
        /// The command select PJM user
        /// </summary>
        private static SqlCommand cmdSelectPJMUser;
        /// <summary>
        /// The command select caiso user
        /// </summary>
        private static SqlCommand cmdSelectCAISOUser;
        private static SqlCommand cmdSelectERCOTUser;
        /// <summary>
        /// The command select PJM valid virtual nodes
        /// </summary>
        private static SqlCommand cmdSelectPJMValidVirtualNodes;
        private static SqlCommand cmdSelectFTRPortfolio;
        private static SqlCommand cmdSelectPortfolioInFTRBids;
        private static SqlCommand cmdDeleteFtrBidErcot;
        private static SqlCommand cmdInsertFtrBidCommandErcot;
        #endregion

        /// <summary>
        /// The s forfeiture virtual list
        /// </summary>
        private static Tuple<List<string>, List<string>> sForfeitureVirtualList = null;
        #region Instantiate Collection
        /// <summary>
        /// The s bid identifier list
        /// </summary>
        private static List<string> sBidIdList = new List<string>();
        /// <summary>
        /// The LST forfeiture uptos list
        /// </summary>
        private static List<string> lstForfeitureUptosList = new List<string>();

        private static Dictionary<int, string> dictExternalPortfolioHash = new Dictionary<int, string>();
        /// <summary>
        /// The dictionary node type hash
        /// </summary>
        private static Dictionary<int, string> dictNodeTypeHash = new Dictionary<int, string>();
        /// <summary>
        /// The dictionary node hash
        /// </summary>
        private static Dictionary<long, PricingNode> dictPJMNodeHash = new Dictionary<long, PricingNode>();
        private static Dictionary<long, PricingNode> dictErcotNodeHash = new Dictionary<long, PricingNode>();
        /// <summary>
        /// The dictionary node name hash
        /// </summary>
        private static Dictionary<string, int> dictNodeNameHash = new Dictionary<string, int>();
        /// <summary>
        /// The dictionary node ext hash
        /// </summary>
        private static Dictionary<long, int> dictNodeExtHash = new Dictionary<long, int>();
        /// <summary>
        /// The dictionary coordinate hash
        /// </summary>
        private static Dictionary<int, Coordinate> dictCoordinateHash = new Dictionary<int, Coordinate>();
        /// <summary>
        /// The dictionary portfolio hash
        /// </summary>
        private static Dictionary<int, string> dictPortfolioHash = new Dictionary<int, string>();
        /// <summary>
        /// The dictionary SPP location hash
        /// </summary>
        private static Dictionary<int, string> dictSppLocationHash = new Dictionary<int, string>();
        /// <summary>
        /// The dictionary market node hash
        /// </summary>
        private static Dictionary<int, Dictionary<string, PricingNode>> dictMarketNodeHash = new Dictionary<int, Dictionary<string, PricingNode>>();

        #endregion

        /// <summary>
        /// Initialization of Database Connection and Commands
        /// </summary>
        private static void LoadDB()
        {
            try
            {
                 
                VayuDBConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
                //
                cmdSelectUptosForfeiture = new SqlCommand();
                cmdSelectUptosForfeiture.CommandText = "select distinct sourcename_utc, sinkname_utc from fftlist where isocode = 'PJM' and monthfft = @monthfft order by sourcename_utc, sinkname_utc";
                cmdSelectUptosForfeiture.Parameters.AddWithValue("@monthfft", "monthfft");
                cmdSelectUptosForfeiture.Connection = VayuDBConnection;
                //
                cmdSelectAllNodes = new SqlCommand();
                cmdSelectAllNodes.CommandText = "select NodeName, NodeKey, zone, NodeType.Label, externalnodeid, Node.nodeTypeKey from Node, NodeType where Node.MarketKey = @MarketKey " +
                                                                    "AND Node.NodeTypeKey=nodetype.NodeTypeKey order by Node.NodeName";
                cmdSelectAllNodes.Parameters.AddWithValue("@MarketKey", "MarketKey");
                cmdSelectAllNodes.Connection = VayuDBConnection;
                //
                cmdSelectAllErcotNodes = new SqlCommand();
                cmdSelectAllErcotNodes.CommandText = "select NodeName, NodeKey, zone, NodeType.Label, externalnodeid, Node.nodeTypeKey from Vayu..Node,Vayu..NodeType where Node.MarketKey = @MarketKey " +
                                                                    "AND Node.NodeTypeKey=nodetype.NodeTypeKey order by Node.NodeName";
                cmdSelectAllErcotNodes.Parameters.AddWithValue("@MarketKey", "MarketKey");
                cmdSelectAllErcotNodes.Connection = VayuDBConnection;
                //
                cmdSelectVirtualForfeiture = new SqlCommand();
                cmdSelectVirtualForfeiture.CommandText = "select nodename, inc_dec from FFT_BidNodeList where isocode = 'PJM' and monthfft = @monthfft order by nodename";
                cmdSelectVirtualForfeiture.Parameters.AddWithValue("@monthfft", "monthfft");
                cmdSelectVirtualForfeiture.Connection = VayuDBConnection;
                //
                cmdSelectNameDateRange = new SqlCommand();
                cmdSelectNameDateRange.CommandText = "select distinct datename from daterange where trader = @trader order by datename";
                cmdSelectNameDateRange.Parameters.AddWithValue("@trader", "trader");
                cmdSelectNameDateRange.Connection = VayuDBConnection;
                //
                cmdSelectDateDateRange = new SqlCommand();
                cmdSelectDateDateRange.CommandText = "select marketdate from daterange where trader = @trader and datename = @datename order by marketdate";
                cmdSelectDateDateRange.Parameters.AddWithValue("@trader", "trader");
                cmdSelectDateDateRange.Parameters.AddWithValue("@datename", "datename");
                cmdSelectDateDateRange.Connection = VayuDBConnection;
                //
                cmdSelectNodeFromExternalCommand = new SqlCommand();
                cmdSelectNodeFromExternalCommand.CommandText = "Select nodekey from node ExternalNodeID=@ExternalNodeID";
                cmdSelectNodeFromExternalCommand.Parameters.AddWithValue("@ExternalNodeID", "ExternalNodeID");
                cmdSelectNodeFromExternalCommand.Connection = VayuDBConnection;

                cmdSelectCurrentBids = new SqlCommand();
                cmdSelectCurrentBids.CommandText = "select NodeKey, ClearedMW, PortfolioKey, IncDec, marketdatetime from clearedbids where MarketDateTime > convert(date, GETDATE()) and " +
                    "portfoliokey = @portfoliokey order by marketdatetime";
                cmdSelectCurrentBids.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectCurrentBids.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioCurrentBids = new SqlCommand();
                cmdSelectPortfolioCurrentBids.CommandText = "select distinct NodeKey  from clearedbids where MarketDateTime > @startDate and MarketDateTime <=@endDate and " +
                    "portfoliokey in (@portfoliokey)";
                cmdSelectPortfolioCurrentBids.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectPortfolioCurrentBids.Parameters.AddWithValue("@startDate", "startDate");
                cmdSelectPortfolioCurrentBids.Parameters.AddWithValue("@endDate", "endDate");
                cmdSelectPortfolioCurrentBids.Connection = VayuDBConnection;
                //
                cmdSelectCurrentUptos = new SqlCommand();
                cmdSelectCurrentUptos.CommandText = "select SourceNodeKey, SinkNodeKey, ClearedMW, PortfolioKey, MarketDateTime from clearedees where MarketDateTime > convert(date, GETDATE()) and portfoliokey = @portfoliokey " +
                   "| order by marketdatetime";
                cmdSelectCurrentUptos.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectCurrentUptos.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioSourceUptos = new SqlCommand();
                cmdSelectPortfolioSourceUptos.CommandText = "select distinct SourceNodeKey from clearedees where MarketDateTime > @startDate and MarketDateTime <=@endDate and " +
                    "portfoliokey in ( @portfoliokey )";
                cmdSelectPortfolioSourceUptos.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectPortfolioSourceUptos.Parameters.AddWithValue("@startDate", "startDate");
                cmdSelectPortfolioSourceUptos.Parameters.AddWithValue("@endDate", "endDate");
                cmdSelectPortfolioSourceUptos.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioSinkUptos = new SqlCommand();
                cmdSelectPortfolioSinkUptos.CommandText = "select distinct SinkNodeKey from clearedees where MarketDateTime > @startDate and MarketDateTime <=@endDate and portfoliokey = @portfoliokey ";

                cmdSelectPortfolioSinkUptos.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectPortfolioSinkUptos.Parameters.AddWithValue("@startDate", "startDate");
                cmdSelectPortfolioSinkUptos.Parameters.AddWithValue("@endDate", "endDate");
                cmdSelectPortfolioSinkUptos.Connection = VayuDBConnection;
                //
                cmdSelectPJMConstraintSensitivity = new SqlCommand();
                cmdSelectPJMConstraintSensitivity.CommandText = "select distinct MonitoredText from RTMasterConstraint order by MonitoredText";
                cmdSelectPJMConstraintSensitivity.Connection = VayuDBConnection;
                //                
                cmdSelectMISOConstraintSensitivity = new SqlCommand();
                cmdSelectMISOConstraintSensitivity.CommandText = "select distinct MonitoredText from MISO.RTMasterConstraint order by MonitoredText";
                cmdSelectMISOConstraintSensitivity.Connection = VayuDBConnection;
                //
                cmdSelectContingencySensitivity = new SqlCommand();
                cmdSelectContingencySensitivity.CommandText = "select distinct ContingencyText from RTMasterConstraint where MonitoredText = @constraintname order by ContingencyText";
                cmdSelectContingencySensitivity.Parameters.AddWithValue("@constraintname", "constraintname");
                cmdSelectContingencySensitivity.Connection = VayuDBConnection;
                //
                cmdSelectMISOContingencySensitivity = new SqlCommand();
                cmdSelectMISOContingencySensitivity.CommandText = "select distinct ContingencyText from MISO.RTMasterConstraint where MonitoredText = @constraintname order by ContingencyText";
                cmdSelectMISOContingencySensitivity.Parameters.AddWithValue("@constraintname", "constraintname");
                cmdSelectMISOContingencySensitivity.Connection = VayuDBConnection;
                //
                cmdSelectSensitivity = new SqlCommand();
                cmdSelectSensitivity.CommandText = "select nodekey, sensitivity from RTMasterConstraint r join RTMasterVector v on v.ConstraintRTNum = r.ConstraintRTNum where r.MonitoredText = @constraintname and @contingency = r.ContingencyText and sensitivity <> 0 order by sensitivity";
                cmdSelectSensitivity.Parameters.AddWithValue("@constraintname", "constraintname");
                cmdSelectSensitivity.Parameters.AddWithValue("@contingency", "contingency");
                cmdSelectSensitivity.Connection = VayuDBConnection;
                //
                cmdSelectMISOSensitivity = new SqlCommand();
                cmdSelectMISOSensitivity.CommandText = "select nodekey, sensitivity from MISO.RTMasterConstraint r join MISO.RTMasterSensitivity v on v.ConstraintRTNum = r.ConstraintRTNum where r.MonitoredText = @constraintname and @contingency = r.ContingencyText and sensitivity <> 0 order by sensitivity";
                cmdSelectMISOSensitivity.Parameters.AddWithValue("@constraintname", "constraintname");
                cmdSelectMISOSensitivity.Parameters.AddWithValue("@contingency", "contingency");
                cmdSelectMISOSensitivity.Connection = VayuDBConnection;
                //
                cmdSelectNode = new SqlCommand();
                cmdSelectNode.CommandText = "select nodekey, nodename, externalnodeid, marketkey, zone, nodetypekey from node";
                cmdSelectNode.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioName = new SqlCommand();
                cmdSelectPortfolioName.CommandText = "select strip from portfolio where portfolio_id = @portfolio_id";
                cmdSelectPortfolioName.Parameters.AddWithValue("@portfolio_id", "portfolio_id");
                cmdSelectPortfolioName.Connection = VayuDBConnection;
                //
                cmdSelectExternalPortfolioName = new SqlCommand();
                cmdSelectExternalPortfolioName.CommandText = "select Participant from Vayu..Company where Marketkey=9 and ParticipantID = @ParticipantID";
                cmdSelectExternalPortfolioName.Parameters.AddWithValue("@ParticipantID", "ParticipantID");
                cmdSelectExternalPortfolioName.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioFileVirtualBidsByUser = new SqlCommand();
                cmdSelectPortfolioFileVirtualBidsByUser.CommandText = "select distinct saved_name from VIRTUAL_BIDS where trader=@trader and bid_date = @bid_date order by saved_name";
                cmdSelectPortfolioFileVirtualBidsByUser.Parameters.AddWithValue("@trader", "trader");
                cmdSelectPortfolioFileVirtualBidsByUser.Parameters.AddWithValue("@bid_date", "bid_date");
                cmdSelectPortfolioFileVirtualBidsByUser.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioFileVirtualBids = new SqlCommand();
                cmdSelectPortfolioFileVirtualBids.CommandText = "select distinct saved_name from VIRTUAL_BIDS where bid_date = @bid_date order by saved_name";
                cmdSelectPortfolioFileVirtualBids.Parameters.AddWithValue("@bid_date", "bid_date");
                cmdSelectPortfolioFileVirtualBids.Connection = VayuDBConnection;
                //                
                cmdSelectPortfolio = new SqlCommand();
                cmdSelectPortfolio.CommandText = "select STRIP, Portfolio_ID from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = @HUB and A.flag_deprecated=0"
                                         + "and product = @product and ACTIVE = 'Y'   and  PORTFOLIO_id in ("
                                         + "select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in "
                                         + "(select enduser_key from END_USER where ad_login=@ad_login)) order by STRIP";

                cmdSelectPortfolio.Parameters.AddWithValue("@HUB", "HUB");
                cmdSelectPortfolio.Parameters.AddWithValue("@product", "product");
                cmdSelectPortfolio.Parameters.AddWithValue("@ad_login", "ad_login");
                cmdSelectPortfolio.Connection = VayuDBConnection;

                //                
                cmdSelectFTRPortfolio = new SqlCommand();
                cmdSelectFTRPortfolio.CommandText = "select STRIP, Portfolio_ID from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = @HUB and A.flag_deprecated=0"
                                         + "and product = @product and ACTIVE = 'Y'   and  PORTFOLIO_id in ("
                                         + "select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in "
                                         + "(select enduser_key from END_USER where ad_login=@ad_login)) order by STRIP";

                cmdSelectFTRPortfolio.Parameters.AddWithValue("@HUB", "HUB");
                cmdSelectFTRPortfolio.Parameters.AddWithValue("@product", "product");
                cmdSelectFTRPortfolio.Parameters.AddWithValue("@ad_login", "ad_login");
                cmdSelectFTRPortfolio.Connection = VayuDBConnection;

                //
                cmdSelectPortfolioFromAccount = new SqlCommand();
                cmdSelectPortfolioFromAccount.CommandText = "select STRIP, Portfolio_ID, hub, product from PORTFOLIO where account = @account";
                cmdSelectPortfolioFromAccount.Parameters.AddWithValue("@account", "account");
                cmdSelectPortfolioFromAccount.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioInErcotEESBids = new SqlCommand();
                cmdSelectPortfolioInErcotEESBids.CommandText = "select count(*) from Vayu..ErcotPTPBids where PortfolioKey=@PortfolioKey and ENDMARKETDATETIME > @START AND ENDMARKETDATETIME <= @END";
                cmdSelectPortfolioInErcotEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdSelectPortfolioInErcotEESBids.Parameters.AddWithValue("@START", "ENDMARKETDATETIME");
                cmdSelectPortfolioInErcotEESBids.Parameters.AddWithValue("@END", "ENDMARKETDATETIME");
                cmdSelectPortfolioInErcotEESBids.Connection = VayuDBConnection;//' //VayuDBConnection;
                //
                cmdSelectMaxBidIdCommand = new SqlCommand();
                cmdSelectMaxBidIdCommand.CommandText = "select max(bidid) from VirtualBids";
                cmdSelectMaxBidIdCommand.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioInEESBids = new SqlCommand();
                cmdSelectPortfolioInEESBids.CommandText = "select COUNT(*) from EESBIDS where PortfolioKey=@PortfolioKey and ENDMARKETDATETIME > @START AND ENDMARKETDATETIME <= @END";
                cmdSelectPortfolioInEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdSelectPortfolioInEESBids.Parameters.AddWithValue("@START", "ENDMARKETDATETIME");
                cmdSelectPortfolioInEESBids.Parameters.AddWithValue("@END", "ENDMARKETDATETIME");
                cmdSelectPortfolioInEESBids.Connection = VayuDBConnection;

                //
                cmdSelectPortfolioInFTRBids = new SqlCommand();
                cmdSelectPortfolioInFTRBids.CommandText = "select COUNT(*) from FtrBids where PortfolioKey=@PortfolioKey and Auction = @Auction";
                cmdSelectPortfolioInFTRBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdSelectPortfolioInFTRBids.Parameters.AddWithValue("@Auction", "Auction");
                cmdSelectPortfolioInFTRBids.Connection = VayuDBConnection;

                //
                cmdSelectPortfolioInVirtualBids = new SqlCommand();
                cmdSelectPortfolioInVirtualBids.CommandText = "select COUNT(*) from VirtualBids where PortfolioKey=@PortfolioKey and marketdate = @START";
                cmdSelectPortfolioInVirtualBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdSelectPortfolioInVirtualBids.Parameters.AddWithValue("@START", "marketdate");
                cmdSelectPortfolioInVirtualBids.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioInVirtualBidsHolder = new SqlCommand();
                cmdSelectPortfolioInVirtualBidsHolder.CommandText = "select COUNT(*) from VirtualBidsHolder where PortfolioKey=@PortfolioKey and marketdate = @START";
                cmdSelectPortfolioInVirtualBidsHolder.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdSelectPortfolioInVirtualBidsHolder.Parameters.AddWithValue("@START", "marketdate");
                cmdSelectPortfolioInVirtualBidsHolder.Connection = VayuDBConnection;
                //
                cmdSelectPjmUptosBid = new SqlCommand();
                cmdSelectPjmUptosBid.CommandText = "select SourceNodeKey, SinkNodeKey, RequestedMW, Price, EndMarketDateTime, bidstatus, scheduleid, comments  " +
                                                        "  from EESBids a  where EndMarketDateTime " +
                                                        "> @start and EndMarketDateTime <= @end and PortfolioKey = @portfoliokey order by source, sink, EndMarketDateTime";
                cmdSelectPjmUptosBid.Parameters.AddWithValue("@start", "EndMarketDateTime");
                cmdSelectPjmUptosBid.Parameters.AddWithValue("@end", "EndMarketDateTime");
                cmdSelectPjmUptosBid.Parameters.AddWithValue("@portfoliokey", "PortfolioKey");
                cmdSelectPjmUptosBid.Connection = VayuDBConnection;
                //
                cmdSelectErcotEESBids = new SqlCommand();
                //cmdSelectErcotEESBids.CommandText = "SELECT SOURCE, SINK, REQUESTEDMW, PRICE, ENDMARKETDATETIME, bidstatus, bidid,comments FROM Vayu..ercotptpbids WHERE ENDMARKETDATETIME > @start AND " +
                //                                            "ENDMARKETDATETIME <= @end AND PORTFOLIOKEY = @portfoliokey order by ENDMARKETDATETIME";
                cmdSelectErcotEESBids.CommandText = "select SourceNodeKey, SinkNodeKey, RequestedMW, Price, EndMarketDateTime, bidstatus, scheduleid, comments  " +
                                        "  from Vayu..ErcotPTPBids a  where EndMarketDateTime " +
                                        "> @start and EndMarketDateTime <= @end and PortfolioKey = @portfoliokey order by source, sink, EndMarketDateTime";

                cmdSelectErcotEESBids.Parameters.AddWithValue("@start", "EndMarketDateTime");
                cmdSelectErcotEESBids.Parameters.AddWithValue("@end", "EndMarketDateTime");
                cmdSelectErcotEESBids.Parameters.AddWithValue("@portfoliokey", "PortfolioKey");
                cmdSelectErcotEESBids.Connection = VayuDBConnection; //VayuDBConnection;
                //
                cmdSelectMisoVirtualBids = new SqlCommand();
                cmdSelectMisoVirtualBids.CommandText = "SELECT nodekey, mw, price, he, incdec, status from VirtualBids where marketdate = @marketdate and portfoliokey = @portfoliokey";
                cmdSelectMisoVirtualBids.Parameters.AddWithValue("@marketdate", "marketdate");
                cmdSelectMisoVirtualBids.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectMisoVirtualBids.Connection = VayuDBConnection;
                //
                cmdSelectVirtualBids = new SqlCommand();
                cmdSelectVirtualBids.CommandText = "SELECT nodekey, mw, price, he, incdec, status, bidid, segment,comments from VirtualBids where marketdate = @marketdate and portfoliokey = @portfoliokey " +
                                                        "and price is not null and status <> @status";
                //   cmdSelectVirtualBids.CommandText = "SELECT nodekey, mw, price, he, incdec, status, bidid, segment,comments from VirtualBids where marketdate = @marketdate and portfoliokey = @portfoliokey " +
                //                                       " and price is not null and status = @status";
                cmdSelectVirtualBids.Parameters.AddWithValue("@marketdate", "marketdate");
                cmdSelectVirtualBids.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectVirtualBids.Parameters.AddWithValue("@status", "status");
                cmdSelectVirtualBids.Connection = VayuDBConnection;
                //
                cmdSelectVirtualBidsHolder = new SqlCommand();
                cmdSelectVirtualBidsHolder.CommandText = "SELECT nodekey, mw, price, he, incdec, status, bidid, segment,comments from VirtualBidsHolder where marketdate = @marketdate and portfoliokey = @portfoliokey";
                cmdSelectVirtualBidsHolder.Parameters.AddWithValue("@marketdate", "marketdate");
                cmdSelectVirtualBidsHolder.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectVirtualBidsHolder.Connection = VayuDBConnection;
                //
                cmdSelectNodeFromExternal = new SqlCommand();
                cmdSelectNodeFromExternal.CommandText = "select nodekey from node where ExternalNodeID = @ExternalNodeID";
                cmdSelectNodeFromExternal.Parameters.AddWithValue("@ExternalNodeID", "ExternalNodeID");
                cmdSelectNodeFromExternal.Connection = VayuDBConnection;
                //
                cmdSelectNodeFromName = new SqlCommand();
                cmdSelectNodeFromName.CommandText = "select nodekey from node where nodename = @nodename and marketkey = @marketKey";
                cmdSelectNodeFromName.Parameters.AddWithValue("@nodename", "nodename");
                cmdSelectNodeFromName.Parameters.AddWithValue("@marketkey", "marketkey");
                //cmdSelectNodeFromName.Connection = VayuDBConnection;
                //
                cmdSelectLoad = new SqlCommand();
                cmdSelectLoad.CommandText = "select distinct loadsname, loadskey from loads where MarketKey = @marketkey order by loadsname";
                cmdSelectLoad.Parameters.AddWithValue("@marketkey", "marketkey");
                cmdSelectLoad.Connection = VayuDBConnection;
                //
                cmdSelectLoadForecast = new SqlCommand();
                cmdSelectLoadForecast.CommandText = "select marketdatetime, mw from loadrth where loadskey = @loadskey and marketdatetime > @start and marketdatetime <= @end order by marketdatetime";
                cmdSelectLoadForecast.Parameters.AddWithValue("@loadskey", "loadskey");
                cmdSelectLoadForecast.Parameters.AddWithValue("@start", "marketdatetime");
                cmdSelectLoadForecast.Parameters.AddWithValue("@end", "marketdatetime");
                cmdSelectLoadForecast.Connection = VayuDBConnection;
                //
                cmdSelectLoadRth = new SqlCommand();
                cmdSelectLoadRth.CommandText = "select marketdatetime from loadrth where loadskey = @loadskey and marketdatetime > @start and marketdatetime <= @end and (mw < @min or mw > @max)";
                cmdSelectLoadRth.Parameters.AddWithValue("@loadskey", "loadskey");
                cmdSelectLoadRth.Parameters.AddWithValue("@start", "marketdatetime");
                cmdSelectLoadRth.Parameters.AddWithValue("@end", "marketdatetime");
                cmdSelectLoadRth.Parameters.AddWithValue("@min", "mw");
                cmdSelectLoadRth.Parameters.AddWithValue("@max", "mw");
                cmdSelectLoadRth.Connection = VayuDBConnection;
                //
                cmdSelectWeather = new SqlCommand();
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(temperatures < @min or temperatures > @max)";
                cmdSelectWeather.Parameters.AddWithValue("@city", "city");
                cmdSelectWeather.Parameters.AddWithValue("@start", "marketdate");
                cmdSelectWeather.Parameters.AddWithValue("@end", "marketdate");
                cmdSelectWeather.Parameters.AddWithValue("@min", "temperatures");
                cmdSelectWeather.Parameters.AddWithValue("@max", "temperatures");
                cmdSelectWeather.Connection = VayuDBConnection;
                //
                cmdSelectWeatherForecast = new SqlCommand();
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(temperatures < @min or temperatures > @max)";
                cmdSelectWeatherForecast.Parameters.AddWithValue("@city", "city");
                cmdSelectWeatherForecast.Parameters.AddWithValue("@start", "marketdate");
                cmdSelectWeatherForecast.Parameters.AddWithValue("@end", "marketdate");
                cmdSelectWeatherForecast.Connection = VayuDBConnection;
                //
                cmdInsertEESBids = new SqlCommand();
                cmdInsertEESBids.CommandText = "Insert into EESBids values (@scheduleid,@BidStatus,0,@EndMarketDateTime,@RequestedMW,@ClearedMW,@EndUserKey,@SourceNodeKey, " +
                                                                                                "@SinkNodeKey,null, null, @Source,@Sink,@Price,@PortfolioKey,@SubmittedDateTime,@comments)";
                cmdInsertEESBids.Parameters.AddWithValue("@BidStatus", "BidStatus");
                cmdInsertEESBids.Parameters.AddWithValue("@scheduleid", "scheduleid");
                cmdInsertEESBids.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
                cmdInsertEESBids.Parameters.AddWithValue("@RequestedMW", "RequestedMW");
                cmdInsertEESBids.Parameters.AddWithValue("@ClearedMW", "ClearedMW");
                cmdInsertEESBids.Parameters.AddWithValue("@EndUserKey", "EndUserKey");
                cmdInsertEESBids.Parameters.AddWithValue("@SourceNodeKey", "SourceNodeKey");
                cmdInsertEESBids.Parameters.AddWithValue("@SinkNodeKey", "SinkNodeKey");
                cmdInsertEESBids.Parameters.AddWithValue("@Source", "Source");
                cmdInsertEESBids.Parameters.AddWithValue("@Sink", "Sink");
                cmdInsertEESBids.Parameters.AddWithValue("@Price", "Price");
                cmdInsertEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdInsertEESBids.Parameters.AddWithValue("@SubmittedDateTime", "SubmittedDateTime");
                cmdInsertEESBids.Parameters.AddWithValue("@comments", "comments");
                cmdInsertEESBids.Connection = VayuDBConnection;
                //
                cmdInsertErcotEESBids = new SqlCommand();
                //cmdInsertErcotEESBids.CommandText = "Insert into Vayu..ercotptpbids values (@bidid,@Source,@Sink,@Price,@EndMarketDateTime,@RequestedMW,@BidStatus,@EndUserKey,@PortfolioKey,@SubmittedDateTime,@comments)";
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@bidid", "bidid");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@Source", "Source");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@Sink", "Sink");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@Price", "Price");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@RequestedMW", "RequestedMW");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@BidStatus", "BidStatus");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@EndUserKey", "EndUserKey");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@SubmittedDateTime", "SubmittedDateTime");
                //cmdInsertErcotEESBids.Parameters.AddWithValue("@comments", "comments");
                //cmdInsertErcotEESBids.Connection = VayuDBConnection;
                //
                cmdInsertErcotEESBids.CommandText = "Insert into Vayu..ercotptpbids values (@scheduleid,@BidStatus,0,@EndMarketDateTime,@RequestedMW,@ClearedMW,@EndUserKey,@SourceNodeKey, " +
                                                                                "@SinkNodeKey,null, null, @Source,@Sink,@Price,@PortfolioKey,@SubmittedDateTime,@comments)";
                cmdInsertErcotEESBids.Parameters.AddWithValue("@BidStatus", "BidStatus");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@scheduleid", "scheduleid");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@EndMarketDateTime", "EndMarketDateTime");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@RequestedMW", "RequestedMW");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@ClearedMW", "ClearedMW");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@EndUserKey", "EndUserKey");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@SourceNodeKey", "SourceNodeKey");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@SinkNodeKey", "SinkNodeKey");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@Source", "Source");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@Sink", "Sink");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@Price", "Price");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@SubmittedDateTime", "SubmittedDateTime");
                cmdInsertErcotEESBids.Parameters.AddWithValue("@comments", "comments");
                cmdInsertErcotEESBids.Connection = VayuDBConnection;

                //
                cmdDeleteEESBidsCommand = new SqlCommand();
                cmdDeleteEESBidsCommand.CommandText = "delete EESBIDS where portfoliokey = @portfoliokey and bidstatus <> 'Valid' and endmarketdatetime > @start and endmarketdatetime <= @end";
                cmdDeleteEESBidsCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdDeleteEESBidsCommand.Parameters.AddWithValue("@start", "EndMarketDateTime");
                cmdDeleteEESBidsCommand.Parameters.AddWithValue("@end", "EndMarketDateTime");
                cmdDeleteEESBidsCommand.Connection = VayuDBConnection;
                //
                cmdDeleteEESBidsById = new SqlCommand();
                cmdDeleteEESBidsById.CommandText = "delete EESBIDS where scheduleid = @scheduleid and bidstatus <> 'Valid' and endmarketdatetime > @start and endmarketdatetime <= @end";
                cmdDeleteEESBidsById.Parameters.AddWithValue("@scheduleid", "scheduleid");
                cmdDeleteEESBidsById.Parameters.AddWithValue("@start", "EndMarketDateTime");
                cmdDeleteEESBidsById.Parameters.AddWithValue("@end", "EndMarketDateTime");
                cmdDeleteEESBidsById.Connection = VayuDBConnection;
                //
                cmdDeleteFileVirtualBids = new SqlCommand();
                cmdDeleteFileVirtualBids.CommandText = "delete virtual_bids where bid_date = @bid_date and trader = @trader and saved_name = @saved_name";
                cmdDeleteFileVirtualBids.Parameters.AddWithValue("@bid_date", "bid_date");
                cmdDeleteFileVirtualBids.Parameters.AddWithValue("@trader", "trader");
                cmdDeleteFileVirtualBids.Parameters.AddWithValue("@saved_name", "saved_name");
                cmdDeleteFileVirtualBids.Connection = VayuDBConnection;
                //
                cmdDeleteErcotEESBidsCommand = new SqlCommand();
                cmdDeleteErcotEESBidsCommand.CommandText = "delete Vayu..ercotptpbids where portfoliokey = @portfoliokey and bidstatus <> 'Valid' and endmarketdatetime > @start and endmarketdatetime <= @end";
                cmdDeleteErcotEESBidsCommand.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdDeleteErcotEESBidsCommand.Parameters.AddWithValue("@start", "EndMarketDateTime");
                cmdDeleteErcotEESBidsCommand.Parameters.AddWithValue("@end", "EndMarketDateTime");
                cmdDeleteErcotEESBidsCommand.Connection = VayuDBConnection;
                //
                cmdDeleteErcotEESBidsById = new SqlCommand();
                cmdDeleteErcotEESBidsById.CommandText = "delete Vayu..ercotptpbids where bidid = @bidid and bidstatus <> 'Valid' and endmarketdatetime > @start and endmarketdatetime <= @end";
                cmdDeleteErcotEESBidsById.Parameters.AddWithValue("@bidid", "bidid");
                cmdDeleteErcotEESBidsById.Parameters.AddWithValue("@start", "EndMarketDateTime");
                cmdDeleteErcotEESBidsById.Parameters.AddWithValue("@end", "EndMarketDateTime");
                cmdDeleteErcotEESBidsById.Connection = VayuDBConnection;
                //
                cmdSelectSourceSink = new SqlCommand();
                cmdSelectSourceSink.CommandText = "select src.SourceNodeKey, src.SourceName, n.ExternalNodeId as SourceExternalNodeID, n.NodeTypeKey as SourceNodeTypeKey, "
                                                        + "n.Zone as SourceNodeZone, sink.SinkNodeKey, sink.SinkName, n2.ExternalNodeID as SinkExternalNodeID, "
                                                        + "n2.NodeTypeKey as SinkNodeTypeKey, n2.Zone as SinkNodeZone "
                                                        + "from EESPathList src inner join Node n on n.NodeKey = src.SourceNodeKey "
                                                        + "inner join EESPathList sink on sink.SinkNodeKey = src.SinkNodeKey and sink.EESPathListKey = src.EESPathListKey "
                                                        + "inner join Node n2 on n2.NodeKey = sink.SinkNodeKey where n.MarketKey = @market and n2.MarketKey = @market and src.MarketKey = @market "
                                                        + "and sink.MarketKey = @market order by n.NodeName ";
                cmdSelectSourceSink.Parameters.AddWithValue("@market", "market");
                cmdSelectSourceSink.Connection = VayuDBConnection;
                //

                //
                cmdSelectSource = new SqlCommand();
                cmdSelectSource.CommandText = "select  distinct src.SourceNodeKey, src.SourceName, n.ExternalNodeId as SourceExternalNodeID, n.NodeTypeKey as SourceNodeTypeKey, n.Zone" +
                                                     " as SourceNodeZone from EESPathList src inner join Node n on n.NodeKey = src.SourceNodeKey where n.MarketKey = @market and src.MarketKey = @market";
                cmdSelectSource.Parameters.AddWithValue("@market", "market");
                cmdSelectSource.Connection = VayuDBConnection;

                //
                cmdSelectSinkCommand = new SqlCommand();
                cmdSelectSinkCommand.CommandText = "select  distinct sink.SinkNodeKey, sink.SinkName, n2.ExternalNodeID as SinkExternalNodeID, n2.NodeTypeKey as SinkNodeTypeKey, n2.Zone " +
                                                  "as SinkNodeZone from EESPathList sink inner join Node n2 on n2.NodeKey = sink.SinkNodeKey where n2.marketkey= @market and sink.MarketKey = @market";
                cmdSelectSinkCommand.Parameters.AddWithValue("@market", "market");
                cmdSelectSinkCommand.Connection = VayuDBConnection;
                //

                cmdSelectSourceSinkNode = new SqlCommand();
                cmdSelectSourceSinkNode.CommandText = "select NodeKey, NodeName, ExternalNodeId, NodeTypeKey, Zone from node where MarketKey = @market" +
                                                          " and nodekey  in (select nodekey from pjm.VirtualValidNodes) order by NodeName";
                //cmdSelectSourceSinkNode.CommandText = "select NodeKey, NodeName, ExternalNodeId, NodeTypeKey, Zone from Node where MarketKey = @market order by NodeName ";
                cmdSelectSourceSinkNode.Parameters.AddWithValue("@market", "market");
                cmdSelectSourceSinkNode.Connection = VayuDBConnection;
                //
                cmdSelectVirtualNodes = VayuDBConnection.CreateCommand();
                cmdSelectVirtualNodes.CommandText = "select Nodekey,nodename,ExternalNodeKey,OriginalNodeKey,OriginalExternalNodeKey,NodeType from NYISOVirtualNodes";
                //
                cmdSelectWeatherCities = new SqlCommand();
                cmdSelectWeatherCities.CommandText = "select distinct city from Wsi_Weather order by city";
                cmdSelectWeatherCities.Connection = VayuDBConnection;
                //
                cmdInsertVirtualBids = new SqlCommand();
                cmdInsertVirtualBids.CommandText = "INSERT VirtualBids VALUES (@PortfolioKey, @NodeKey, @Segment, @HE, @Price, @MW, @MarketDate, @TranTime, @Trader, @IncDec, " +
                                                            "@FileName, @Market, @status, null, @bidid, @comments)";
                cmdInsertVirtualBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdInsertVirtualBids.Parameters.AddWithValue("@NodeKey", "NodeKey");
                cmdInsertVirtualBids.Parameters.AddWithValue("@Segment", "Segment");
                cmdInsertVirtualBids.Parameters.AddWithValue("@MarketDate", "MarketDate");
                cmdInsertVirtualBids.Parameters.AddWithValue("@HE", "HE");
                cmdInsertVirtualBids.Parameters.AddWithValue("@MW", "MW");
                cmdInsertVirtualBids.Parameters.AddWithValue("@Price", "Price");
                cmdInsertVirtualBids.Parameters.AddWithValue("@IncDec", "IncDec");
                cmdInsertVirtualBids.Parameters.AddWithValue("@FileName", "FileName");
                cmdInsertVirtualBids.Parameters.AddWithValue("@TranTime", "TranTime");
                cmdInsertVirtualBids.Parameters.AddWithValue("@Market", "Market");
                cmdInsertVirtualBids.Parameters.AddWithValue("@Trader", "Trader");
                cmdInsertVirtualBids.Parameters.AddWithValue("@status", "status");
                cmdInsertVirtualBids.Parameters.AddWithValue("@bidid", "bidid");
                cmdInsertVirtualBids.Parameters.AddWithValue("@comments", "comments");
                cmdInsertVirtualBids.Connection = VayuDBConnection;
                //
                //DD: Removed coz we will maintain one Key for one portfolio. Else traders have to co-ordinate.
                cmdDeleteVirtualBids = new SqlCommand();
                cmdDeleteVirtualBids.CommandText = "delete VirtualBids where portfoliokey=@portfoliokey and marketdate=@marketdate and status <> 'Valid'";
                cmdDeleteVirtualBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdDeleteVirtualBids.Parameters.AddWithValue("@MarketDate", "MarketDate");
                //cmdDeleteVirtualBids.Parameters.AddWithValue("@Trader", "Trader");
                cmdDeleteVirtualBids.Connection = VayuDBConnection;
                //
                cmdDeleteValidVirtualBids = new SqlCommand();
                cmdDeleteValidVirtualBids.CommandText = "delete VirtualBids where portfoliokey=@portfoliokey and marketdate=@marketdate and trader = @trader and status = 'Valid' " +
                                                                "and nodekey = @nodekey and incdec = @incdec";
                cmdDeleteValidVirtualBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdDeleteValidVirtualBids.Parameters.AddWithValue("@MarketDate", "MarketDate");
                cmdDeleteValidVirtualBids.Parameters.AddWithValue("@Trader", "Trader");
                cmdDeleteValidVirtualBids.Parameters.AddWithValue("@nodekey", "nodekey");
                cmdDeleteValidVirtualBids.Parameters.AddWithValue("@incdec", "incdec");
                cmdDeleteValidVirtualBids.Connection = VayuDBConnection;
                //
                cmdDeleteVirtualBidsForUpdate = VayuDBConnection.CreateCommand();
                cmdDeleteVirtualBidsForUpdate.CommandText = "delete VirtualBids where bidid=@bidid";
                cmdDeleteVirtualBidsForUpdate.Parameters.AddWithValue("@bidid", "bidid");
                //
                cmdSelectLoadDiffList = new SqlCommand();
                cmdSelectLoadDiffList.CommandText = "select loadsname, loadskey from Loads where MarketKey = 1 or (marketkey = 2 and loadsname = 'Total Load') order by LoadsName";
                cmdSelectLoadDiffList.Connection = VayuDBConnection;
                //
                cmdSelectAllPath = new SqlCommand();
                cmdSelectAllPath.CommandText = "select distinct STL_PNT from Vayu..DAMPTPObligation order by STL_PNT";
                cmdSelectAllPath.Connection = VayuDBConnection;
                //
                cmdInsertLoadThreshold = new SqlCommand();
                cmdInsertLoadThreshold.CommandText = "insert load_threshold values (@loads_user, @zone, @threshold)";
                cmdInsertLoadThreshold.Parameters.AddWithValue("@loads_user", "loads_user");
                cmdInsertLoadThreshold.Parameters.AddWithValue("@zone", "zone");
                cmdInsertLoadThreshold.Parameters.AddWithValue("@threshold", "threshold");
                cmdInsertLoadThreshold.Connection = VayuDBConnection;
                //
                cmdDeleteLoadThreshold = new SqlCommand();
                cmdDeleteLoadThreshold.CommandText = "delete load_threshold where loads_user = @loads_user and zone = @zone";
                cmdDeleteLoadThreshold.Parameters.AddWithValue("@loads_user", "loads_user");
                cmdDeleteLoadThreshold.Parameters.AddWithValue("@zone", "zone");
                cmdDeleteLoadThreshold.Connection = VayuDBConnection;
                //
                cmdSelectLoadThreshold = new SqlCommand();
                cmdSelectLoadThreshold.CommandText = "select zone, threshold from load_threshold where loads_user = @loads_user";
                cmdSelectLoadThreshold.Parameters.AddWithValue("@loads_user", "loads_user");
                cmdSelectLoadThreshold.Connection = VayuDBConnection;
                //
                cmdUpdateSubmitVirtualPortfolio = new SqlCommand();
                cmdUpdateSubmitVirtualPortfolio.CommandText = "update VirtualBids set marketdate = @newmarketdate, trantime = @trantime, status = @status,comments = @comments where nodekey = @nodekey and he = @he and price = @price " +
                                                            "and mw = @mw and marketdate = @marketdate and portfoliokey = @portfoliokey and incdec = @incdec and status <> 'VALID'";
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@newmarketdate", "marketdate");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@trantime", "trantime");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@nodekey", "nodekey");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@he", "he");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@price", "price");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@mw", "mw");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@marketdate", "marketdate");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@incdec", "incdec");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@status", "status");
                cmdUpdateSubmitVirtualPortfolio.Parameters.AddWithValue("@comments", "comments");
                cmdUpdateSubmitVirtualPortfolio.Connection = VayuDBConnection;
                //
                cmdSelectFtrBid = new SqlCommand();
                //cmdSelectFtrBid.CommandText = "select  source, sink, mw1, price1, mw2, price2, mw3, price3, mw4, price4, mw5, price5, mw6, price6, classtype, " +
                //                                    "tradetype, hedgetype, periodhours, periodname,sourceextid,sinkextid,ftrbidskey, Status, tcrid, periodkey from ftrbids (nolock) where portfoliokey = @portfoliokey and " +
                //                                    "auction = @auction and status = 'IMPORTED'";
                cmdSelectFtrBid.CommandText = "select FB.source,FB.sink,FB. mw1,FB. price1,FB. mw2,FB. price2,FB. mw3,FB. price3,FB. mw4,FB. price4,FB. mw5,FB. price5,FB. mw6,FB. price6,FB.classtype,FB.tradetype, " +
                                              " FB.hedgetype,FB.periodhours,FB.periodname,FB.sourceextid,FB.sinkextid,FB.ftrbidskey,FB.Status,FB.tcrid,FB.periodkey,SourceZone.Zone as SourceZone, " +
                                              " SinkZone.Zone as SinkZone from ftrbids  (nolock) as FB Join Node As SourceZone on FB.Source=SourceZone.NodeName Join Node AS SinkZone on FB.Sink=SinkZone.NodeName " +
                                              " where FB.portfoliokey = @portfoliokey and FB.auction = @auction and FB.status = 'IMPORTED'  and SourceZone.MarketKey=1 and SinkZone.MarketKey=1";
                cmdSelectFtrBid.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectFtrBid.Parameters.AddWithValue("@auction", "auction");

                //
                cmsSelectftrBidspeakoffpeak = new SqlCommand();
                cmsSelectftrBidspeakoffpeak.CommandText = " Select FB.source,FB.sink,FB. mw1,FB. price1,FB. mw2,FB. price2,FB. mw3,FB. price3,FB. mw4,FB. price4,FB. mw5,FB. price5,FB. mw6,FB. price6,FB.classtype,FB.tradetype, " +
                                                          " FB.hedgetype,FB.periodhours,FB.periodname,FB.sourceextid,FB.sinkextid,FB.ftrbidskey,FB.Status,FB.tcrid,FB.periodkey, " +
                                                          " SourceZone.Zone As SourceZone,SinkZone.Zone As SinkZone,FB.PortfolioKey from ftrbids  (nolock) as FB  " +
                                                          " join Node As SourceZone ON FB.Source=SourceZone.NodeName join Node As SinkZone ON FB.Sink=SinkZone.NodeName " +
                                                          " where FB.portfoliokey =@portfoliokey  and FB.auction =@auction";
                cmsSelectftrBidspeakoffpeak.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmsSelectftrBidspeakoffpeak.Parameters.AddWithValue("@auction", "auction");
                cmsSelectftrBidspeakoffpeak.Connection = VayuDBConnection;
                //

                cmsSelectftrBidspeakoffpeakErcot = new SqlCommand();
                cmsSelectftrBidspeakoffpeakErcot.CommandText = " Select FB.source,FB.sink,FB. mw1,FB. price1,FB. mw2,FB. price2,FB. mw3,FB. price3,FB. mw4,FB. price4,FB. mw5,FB. price5,FB. mw6,FB. price6,FB.classtype,FB.tradetype, " +
                                                          " FB.hedgetype,FB.periodhours,FB.periodname,FB.sourceextid,FB.sinkextid,FB.crrbidskey,FB.Status,FB.tcrid,FB.periodkey, " +
                                                          " SourceZone.Zone As SourceZone,SinkZone.Zone As SinkZone,FB.PortfolioKey from crrbids  (nolock) as FB  " +
                                                          " join Node As SourceZone ON FB.Source=SourceZone.NodeName join Node As SinkZone ON FB.Sink=SinkZone.NodeName " +
                                                          " where FB.portfoliokey =@portfoliokey  and FB.auction =@auction";
                cmsSelectftrBidspeakoffpeakErcot.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmsSelectftrBidspeakoffpeakErcot.Parameters.AddWithValue("@auction", "auction");
                cmsSelectftrBidspeakoffpeakErcot.Connection = VayuDBConnection;


                cmsSelectftrPortfilio = new SqlCommand();
                cmsSelectftrPortfilio.CommandText = "Select STRIP,PORTFOLIO_ID from PORTFOLIO where HUB=@HUB and PRODUCT='FTR'";
                cmsSelectftrPortfilio.Parameters.AddWithValue("@HUB", "HUB");
                cmsSelectftrPortfilio.Connection = VayuDBConnection;
                //
                cmdInsertPJMFtrBidCommand = new SqlCommand();
                cmdInsertPJMFtrBidCommand.CommandText = "insert FTRBids (Participant,Month,Auction,PeriodType,ClassType,TradeType,HedgeType,MW1,Price1,MW2,Price2,MW3,Price3,MW4," +
                    "Price4,MW5,Price5,MW6,Price6,PortfolioKey,MarketKey,PeriodHours,PeriodName,Source,SourceExtId,Sink,SinkExtId,PeriodKey,status,TcrId) " +
                    " values(@Participant, @month, @auction, @periodtype, @classtype, @tradetype, @hedgetype," +
                    "@mw1,@price1,@mw2,@price2,@mw3,@price3,@mw4,@price4,@mw5,@price5,@mw6,@price6,@portfoliokey,@MarketKey,@periodhours,@periodname," +
                                                     "@source,@sourceextid,@sink,@sinkextid,@PeriodKey,'IMPORTED',@TcrId)";
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@PeriodKey", "PeriodKey");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@Participant", "Participant");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@MarketKey", "MarketKey");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@month", "month");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@auction", "auction");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@periodtype", "periodtype");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@classtype", "classtype");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@tradetype", "tradetype");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@hedgetype", "hedgetype");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@mw1", "mw1");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@price1", "price1");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@mw2", "mw2");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@price2", "price2");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@mw3", "mw3");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@price3", "price3");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@mw4", "mw4");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@price4", "price4");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@mw5", "mw5");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@price5", "price5");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@mw6", "mw6");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@price6", "price6");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@portfoliokey", "portfiliokey");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@periodhours", "periodhours");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@periodname", "periodname");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@source", "source");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@sourceextid", "sourceextid");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@sink", "sink");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@sinkextid", "sinkextid");
                cmdInsertPJMFtrBidCommand.Parameters.AddWithValue("@TcrId", "TcrId");
                cmdInsertPJMFtrBidCommand.Connection = VayuDBConnection;

                cmdInsertFtrBidCommandErcot = new SqlCommand();
                cmdInsertFtrBidCommandErcot.CommandText = "insert CrrBids (Participant,Month,Auction,PeriodType,ClassType,TradeType,HedgeType,MW1,Price1,MW2,Price2,MW3,Price3,MW4," +
                    "Price4,MW5,Price5,MW6,Price6,PortfolioKey,MarketKey,PeriodHours,PeriodName,Source,SourceExtId,Sink,SinkExtId,PeriodKey,status,TcrId) " +
                    " values(@Participant, @month, @auction, @periodtype, @classtype, @tradetype, @hedgetype," +
                    "@mw1,@price1,@mw2,@price2,@mw3,@price3,@mw4,@price4,@mw5,@price5,@mw6,@price6,@portfoliokey,@MarketKey,@periodhours,@periodname," +
                                                     "@source,@sourceextid,@sink,@sinkextid,@PeriodKey,'IMPORTED',@TcrId)";
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@PeriodKey", "PeriodKey");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@Participant", "Participant");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@MarketKey", "MarketKey");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@month", "month");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@auction", "auction");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@periodtype", "periodtype");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@classtype", "classtype");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@tradetype", "tradetype");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@hedgetype", "hedgetype");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@mw1", "mw1");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@price1", "price1");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@mw2", "mw2");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@price2", "price2");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@mw3", "mw3");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@price3", "price3");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@mw4", "mw4");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@price4", "price4");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@mw5", "mw5");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@price5", "price5");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@mw6", "mw6");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@price6", "price6");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@portfoliokey", "portfiliokey");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@periodhours", "periodhours");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@periodname", "periodname");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@source", "source");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@sourceextid", "sourceextid");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@sink", "sink");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@sinkextid", "sinkextid");
                cmdInsertFtrBidCommandErcot.Parameters.AddWithValue("@TcrId", "TcrId");
                cmdInsertFtrBidCommandErcot.Connection = VayuDBConnection;

                //
                cmdDeleteFtrBid = new SqlCommand();
                cmdDeleteFtrBid.CommandText = "delete ftrbids where auction = @auction and portfoliokey = @portfoliokey and ( status <> 'valid' or Status  is null)";
                cmdDeleteFtrBid.Parameters.AddWithValue("@auction", "auction");
                cmdDeleteFtrBid.Parameters.AddWithValue("@portfoliokey", "portfiliokey");
                cmdDeleteFtrBid.Connection = VayuDBConnection;

                //
                cmdDeleteFtrBidErcot = new SqlCommand();
                cmdDeleteFtrBidErcot.CommandText = "delete crrbids where auction = @auction and portfoliokey = @portfoliokey and ( status <> 'valid' or Status  is null)";
                cmdDeleteFtrBidErcot.Parameters.AddWithValue("@auction", "auction");
                cmdDeleteFtrBidErcot.Parameters.AddWithValue("@portfoliokey", "portfiliokey");
                cmdDeleteFtrBidErcot.Connection = VayuDBConnection;


                //
                cmdSelectNodeType = new SqlCommand();
                cmdSelectNodeType.CommandText = "select nodetypekey, label from nodetype";
                cmdSelectNodeType.Connection = VayuDBConnection;
                //
                cmdSelectCoordinate = new SqlCommand();
                cmdSelectCoordinate.CommandText = "select longitude, latitude from nodegeoimport where nodekey=@nodekey";
                cmdSelectCoordinate.Parameters.AddWithValue("@nodekey", "nodekey");
                cmdSelectCoordinate.Connection = VayuDBConnection;
                //
                cmdInsertVirtualBidsHolder = new SqlCommand();
                cmdInsertVirtualBidsHolder.CommandText = "insert virtualbidsholder select * from VirtualBids where portfoliokey = " +
                                                        "@portfoliokey and marketdate = @marketdate and status = 'Valid' and nodekey = @nodekey and incdec = @incdec";
                cmdInsertVirtualBidsHolder.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdInsertVirtualBidsHolder.Parameters.AddWithValue("@marketdate", "marketdate");
                cmdInsertVirtualBidsHolder.Parameters.AddWithValue("@trader", "trader");
                cmdInsertVirtualBidsHolder.Parameters.AddWithValue("@nodekey", "nodekey");
                cmdInsertVirtualBidsHolder.Parameters.AddWithValue("@incdec", "incdec");
                cmdInsertVirtualBidsHolder.Connection = VayuDBConnection;
                //
                cmdSelectVirtualBidsNodeHolder = new SqlCommand();
                cmdSelectVirtualBidsNodeHolder.CommandText = "select distinct nodekey from virtualbidsholder where portfoliokey = @portfoliokey " +
                                                                "and trader = @trader and marketdate = @marketdate";
                cmdSelectVirtualBidsNodeHolder.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectVirtualBidsNodeHolder.Parameters.AddWithValue("@trader", "trader");
                cmdSelectVirtualBidsNodeHolder.Parameters.AddWithValue("@marketdate", "marketdate");
                cmdSelectVirtualBidsNodeHolder.Connection = VayuDBConnection;
                //
                cmdSelectPjmRtFamilies = new SqlCommand();
                cmdSelectPjmRtFamilies.CommandText = "select distinct monitoredtext from RTFamily";
                cmdSelectPjmRtFamilies.Connection = VayuDBConnection;
                //
                cmdSelectPjmRtConstraintNumbers = new SqlCommand();
                cmdSelectPjmRtConstraintNumbers.CommandText = "select distinct constraintrtnum from RTMasterConstraint";
                cmdSelectPjmRtConstraintNumbers.Connection = VayuDBConnection;
                //
                cmdSelectSimilarLoadDay = new SqlCommand();
                cmdSelectSimilarLoadDay.CommandText = "select similarDate from SimilarLoadDay where tradedate = @tradeDate";
                cmdSelectSimilarLoadDay.Parameters.AddWithValue("@tradeDate", "tradeDate");
                cmdSelectSimilarLoadDay.Connection = VayuDBConnection;
                //
                cmdSelectRiskConstraints = new SqlCommand();
                cmdSelectRiskConstraints.CommandText = "select ConstraintRTNum from RiskConstraints where Date=@similarDate";
                cmdSelectRiskConstraints.Parameters.AddWithValue("@similarDate", "similarDate");
                cmdSelectRiskConstraints.Connection = VayuDBConnection;
                //
                cmdSelectVirtualCleared = new SqlCommand();
                cmdSelectVirtualCleared.CommandText = "select nodekey, clearedmw, marketdatetime, incdec from clearedbids where portfoliokey = @portfoliokey and marketdatetime > @startdate " +
                                                            " and marketdatetime <= @enddate";
                cmdSelectVirtualCleared.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectVirtualCleared.Parameters.AddWithValue("@startdate", "startdate");
                cmdSelectVirtualCleared.Parameters.AddWithValue("@enddate", "enddate");
                cmdSelectVirtualCleared.Connection = VayuDBConnection;
                //
                cmdSelectUptoCleared = new SqlCommand();
                cmdSelectUptoCleared.CommandText = "select sourcenodekey, clearedmw, marketdatetime, sinknodekey from ClearedEES where marketdatetime > @startdate and portfoliokey = @portfoliokey" +
                                                            " and marketdatetime <= @enddate";
                cmdSelectUptoCleared.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectUptoCleared.Parameters.AddWithValue("@startdate", "startdate");
                cmdSelectUptoCleared.Parameters.AddWithValue("@enddate", "enddate");
                cmdSelectUptoCleared.Connection = VayuDBConnection;
                //
                //Ercot

                cmdSelectErcotRTFamilies = new SqlCommand();
                cmdSelectErcotRTFamilies.CommandText = "select distinct monitoredtext from Vayu..RTMasterConstraint";
                cmdSelectErcotRTFamilies.Connection = VayuDBConnection;


                cmdSelectErcotDAFamilies = new SqlCommand();
                cmdSelectErcotDAFamilies.CommandText = "select distinct monitoredtext from Vayu..RTMasterConstraint";
                cmdSelectErcotDAFamilies.Connection = VayuDBConnection;


                cmdErcotSelectUptoCleared = new SqlCommand();
                cmdErcotSelectUptoCleared.CommandText = "select sourcenodekey, clearedmw, marketdatetime, sinknodekey from Vayu..ClearedEES where marketdatetime > @startdate and portfoliokey = @portfoliokey" +
                                                               " and marketdatetime <= @enddate";
                cmdErcotSelectUptoCleared.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdErcotSelectUptoCleared.Parameters.AddWithValue("@startdate", "startdate");
                cmdErcotSelectUptoCleared.Parameters.AddWithValue("@enddate", "enddate");
                cmdErcotSelectUptoCleared.Connection = VayuDBConnection;

                //
                cmdErcotExternalSelectUptoCleared = new SqlCommand();
                cmdErcotExternalSelectUptoCleared.CommandText = "select SourceKey, MW, DeliveryDate,SinkKey from Vayu..DAM60DAYPTPAWARDS where DeliveryDate > @startdate and ParticipantID = @portfoliokey and DeliveryDate <= @enddate";
                cmdErcotExternalSelectUptoCleared.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdErcotExternalSelectUptoCleared.Parameters.AddWithValue("@startdate", "startdate");
                cmdErcotExternalSelectUptoCleared.Parameters.AddWithValue("@enddate", "enddate");
                cmdErcotExternalSelectUptoCleared.Connection = VayuDBConnection;
                //
                cmdSelectMW = new SqlCommand();
                cmdSelectMW.CommandText = "select marketdatetime, clearedmw from clearedbids where marketdatetime between @start and @end and portfoliokey = @portfoliokey";
                cmdSelectMW.Parameters.AddWithValue("@start", "marketdatetime");
                cmdSelectMW.Parameters.AddWithValue("@end", "marketdatetime");
                cmdSelectMW.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectMW.Connection = VayuDBConnection;
                //
                cmdSelectPnlFee = new SqlCommand();
                cmdSelectPnlFee.CommandText = "select pnldate, fee, pnl from virtual_pnl where pnldate between @start and @end and portfoliokey = @portfoliokey";
                cmdSelectPnlFee.Parameters.AddWithValue("@start", "pnldate");
                cmdSelectPnlFee.Parameters.AddWithValue("@end", "pnldate");
                cmdSelectPnlFee.Parameters.AddWithValue("@portfoliokey", "portfoliokey");
                cmdSelectPnlFee.Connection = VayuDBConnection;
                //
                cmdSelectLastFee = new SqlCommand();
                cmdSelectLastFee.CommandText = "select PortfolioKey, PnlDate, fee from VIRTUAL_PNL where PnlDate = (select MAX(pnldate) from VIRTUAL_PNL where Fee <> 0 and PortfolioKey in " +
                                                    "(select portfoliokey from Portfolio where tradetype = @tradetype and market = @market)) and fee <> 0 and " +
                                                    "PortfolioKey  in (select portfoliokey from Portfolio where tradetype = @tradetype and market = @market)";
                cmdSelectLastFee.Parameters.AddWithValue("@tradetype", "tradetype");
                cmdSelectLastFee.Parameters.AddWithValue("@market", "market");
                cmdSelectLastFee.Connection = VayuDBConnection;
                //
                cmdDeleteEESBids = new SqlCommand();
                cmdDeleteEESBids.CommandText = "delete from EESBids where EndMarketDateTime between @startdate and @endate and Source=@source and Sink= @sink and Price=@price and RequestedMW=@mw and PortfolioKey=@portfoliokey";
                cmdDeleteEESBids.Parameters.AddWithValue("@startdate", "EndMarketDateTime");
                cmdDeleteEESBids.Parameters.AddWithValue("@endate", "EndMarketDateTime");
                cmdDeleteEESBids.Parameters.AddWithValue("@source", "Source");
                cmdDeleteEESBids.Parameters.AddWithValue("@sink", "Sink");
                cmdDeleteEESBids.Parameters.AddWithValue("@price", "Price");
                cmdDeleteEESBids.Parameters.AddWithValue("@mw", "RequestedMW");
                cmdDeleteEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdDeleteEESBids.Connection = VayuDBConnection;
                //
                cmdDeleteErcotEESBids = new SqlCommand();
                cmdDeleteErcotEESBids.CommandText = "delete from Vayu..ercotptpbids where EndMarketDateTime between @startdate and @endate and Source=@source and Sink= @sink and Price=@price and RequestedMW=@mw and PortfolioKey=@portfoliokey";
                cmdDeleteErcotEESBids.Parameters.AddWithValue("@startdate", "EndMarketDateTime");
                cmdDeleteErcotEESBids.Parameters.AddWithValue("@endate", "EndMarketDateTime");
                cmdDeleteErcotEESBids.Parameters.AddWithValue("@source", "Source");
                cmdDeleteErcotEESBids.Parameters.AddWithValue("@sink", "Sink");
                cmdDeleteErcotEESBids.Parameters.AddWithValue("@price", "Price");
                cmdDeleteErcotEESBids.Parameters.AddWithValue("@mw", "RequestedMW");
                cmdDeleteErcotEESBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdDeleteErcotEESBids.Connection = VayuDBConnection;
                //
                //
                cmdSelectAccountByUser = new SqlCommand();
                cmdSelectAccountByUser.CommandText = "select trader, account_id, STRIP,Portfolio_ID from PORTFOLIO p, ACCOUNT a where a.ACCOUNT_ID = p.ACCOUNT and " +
                    "HUB = @HUB and ACCOUNT in (select ACCOUNT_ID from END_USER_ACCOUNT where ENDUSER_KEY = (select ENDUSER_KEY from END_USER where Replace(AD_LOGIN,' ','') " +
                    "like '%' + Replace(@AD_LOGIN,' ','') + ',%' OR Replace(AD_LOGIN,' ','') like '%,' + Replace(@AD_LOGIN,' ','') + ',%' OR Replace(AD_LOGIN,' ','') " +
                    "like '%,' + Replace(@AD_LOGIN,' ','') + '%' OR AD_LOGIN = @AD_LOGIN)) and FLAG_DEPRECATED = 0 and PRODUCT = @product";
                cmdSelectAccountByUser.Parameters.AddWithValue("@HUB", "HUB");
                cmdSelectAccountByUser.Parameters.AddWithValue("@product", "product");
                cmdSelectAccountByUser.Parameters.AddWithValue("@AD_LOGIN", "AD_LOGIN");
                cmdSelectAccountByUser.Connection = VayuDBConnection;
                //
                cmdSelectAccount = new SqlCommand();
                cmdSelectAccount.CommandText = "select TRADER, ACCOUNT_ID, STRIP, Portfolio_ID from PORTFOLIO p, account a where a.account_id = p.account and HUB = @HUB " +
                                                    "and product = @product  and FLAG_DEPRECATED = 0 order by ACCOUNT_ID";
                cmdSelectAccount.Parameters.AddWithValue("@HUB", "HUB");
                cmdSelectAccount.Parameters.AddWithValue("@product", "product");
                cmdSelectAccount.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioCount = new SqlCommand();
                cmdSelectPortfolioCount.CommandText = "select COUNT(*) from ClearedBids where  MarketDateTime > @StartDate AND MarketDateTime <= @EndDate";
                // cmdSelectPortfolioCount.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdSelectPortfolioCount.Parameters.AddWithValue("@StartDate", "@StartDate");
                cmdSelectPortfolioCount.Parameters.AddWithValue("@EndDate", "@EndDate");
                cmdSelectPortfolioCount.Connection = VayuDBConnection;
                //
                cmdSelectPortfolioClearedBids = new SqlCommand();
                cmdSelectPortfolioClearedBids.CommandText = "SELECT ClearedBids.nodekey, node.NodeName, ClearedMW,MarketDateTime, IncDec  from ClearedBids INNER JOIN node ON ClearedBids.nodekey=node.nodekey  where PortfolioKey=@PortfolioKey and MarketDateTime >= @StartDate AND MarketDateTime <= @EndDate order by MarketDateTime";
                cmdSelectPortfolioClearedBids.Parameters.AddWithValue("@PortfolioKey", "PortfolioKey");
                cmdSelectPortfolioClearedBids.Parameters.AddWithValue("@StartDate", "@StartDate");
                cmdSelectPortfolioClearedBids.Parameters.AddWithValue("@EndDate", "@EndDate");
                cmdSelectPortfolioClearedBids.Connection = VayuDBConnection;

                //
                cmdSelectDeenergizedNodes = new SqlCommand();
                cmdSelectDeenergizedNodes.CommandText = "select NodeKey, NodeName, ExternalNodeId, NodeTypeKey, Zone from Node where MarketKey = 1 and nodekey not in (select nodekey from PJM.DeenergizedNodes) order by NodeName";
                cmdSelectDeenergizedNodes.Connection = VayuDBConnection;

                cmdSelectUser = new SqlCommand();
                cmdSelectUser.CommandText = "select ad_login from END_USER where user_type='admin' and activeyn='Y'";
                cmdSelectUser.Connection = VayuDBConnection;


                cmdSelectPJMUser = new SqlCommand();
                cmdSelectPJMUser.CommandText = "select distinct ad_login from END_USER a join END_USER_MARKET_PORTFOLIO b on a.enduser_key=b.end_user_key and b.marketkey=1";
                cmdSelectPJMUser.Connection = VayuDBConnection;


                cmdSelectCAISOUser = new SqlCommand();
                cmdSelectCAISOUser.CommandText = "select distinct ad_login from END_USER a join END_USER_MARKET_PORTFOLIO b on a.enduser_key=b.end_user_key and b.marketkey=7";
                cmdSelectCAISOUser.Connection = VayuDBConnection;

                cmdSelectERCOTUser = new SqlCommand();
                cmdSelectERCOTUser.CommandText = "select distinct ad_login from END_USER a join END_USER_MARKET_PORTFOLIO b on a.enduser_key=b.end_user_key and b.marketkey=9";
                cmdSelectERCOTUser.Connection = VayuDBConnection;
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Gets the market key related with Hub.
        /// </summary>
        /// <param name="hub">The hub.</param>
        /// <returns></returns>
        private static int GetMarket(string hub)
        {
            if (hub == "PJM")
            {
                return 1;
            }

            if (hub == "ERCOT")
            {
                return 9;
            }

            return 12;
        }
        #region Public Static Methods
        /// <summary>
        /// Gets the current bids.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns></returns>
        public static List<Bid> GetCurrentBids(string account)
        {
            List<Bid> bidList = new List<Bid>();
            LoadDB();
            VayuDBConnection.Open();
            cmdSelectPortfolioFromAccount.Parameters["@account"].Value = account;
            SqlDataReader reader = cmdSelectPortfolioFromAccount.ExecuteReader();
            Dictionary<int, int> portfolioHash = new Dictionary<int, int>();
            while (reader.Read())
            {
                int portfolioKey = reader.GetInt32(1);
                string hub = reader.GetString(2);
                portfolioHash.Add(portfolioKey, GetMarket(hub));
            }
            reader.Close();
            VayuDBConnection.Close();
            List<int> portfolioKeyList = portfolioHash.Keys.ToList<int>();
            foreach (int portfolioKey in portfolioKeyList)
            {
                int marketKey = portfolioHash[portfolioKey];
                cmdSelectCurrentBids.Parameters["@portfoliokey"].Value = portfolioKey;
                reader = cmdSelectCurrentBids.ExecuteReader();
                while (reader.Read())
                {
                    Bid bid = new Bid();
                    bid.Source = (int)reader.GetDecimal(0);
                    bid.MW = (double)reader.GetDecimal(1);
                    bid.PortfolioKey = (int)reader.GetDecimal(2);
                    bid.MW = (reader.GetString(3) == "INC" || reader.GetString(3) == "I") ? bid.MW * -1 : bid.MW;
                    bid.MarketDateTime = reader.GetDateTime(4);
                    bid.IsUptos = false;
                    bid.Market = marketKey;
                    bidList.Add(bid);
                }
                reader.Close();
                cmdSelectCurrentUptos.Parameters["@portfoliokey"].Value = portfolioKey;
                reader = cmdSelectCurrentUptos.ExecuteReader();
                while (reader.Read())
                {
                    Bid bid = new Bid();
                    bid.Source = (int)reader.GetDecimal(0);
                    bid.Sink = (int)reader.GetDecimal(1);
                    bid.MW = (double)reader.GetDecimal(2);
                    bid.PortfolioKey = (int)reader.GetDecimal(3);
                    bid.MarketDateTime = reader.GetDateTime(4);
                    bid.Market = marketKey;
                    bid.IsUptos = true;
                    bidList.Add(bid);
                }
                reader.Close();
            }
            VayuDBConnection.Close();
            return bidList;
        }
        /// <summary>
        /// Gets the Trader Account Name and ID.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="product">The product.</param>
        /// <param name="market">The market.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /* public static List<Account> GetAccount(string user, string product, string market)
         {
             LoadDB();
             List<Account> accountList = new List<Account>();
             string usertype = "";

             if (VayuDBConnection.State == ConnectionState.Closed)
             {
                 VayuDBConnection.Open();
             }
             SqlCommand cmdusertype = VayuDBConnection.CreateCommand();
             cmdusertype.CommandText = "select user_type from END_USER where ad_login=" + "'" + user + "' ";
             cmdusertype.Connection = VayuDBConnection;
             SqlDataReader cmdusertypedr = cmdusertype.ExecuteReader();
             while (cmdusertypedr.Read())
             {
                 usertype = cmdusertypedr.GetValue(0).ToString();
             }
             SqlCommand cmd = VayuDBConnection.CreateCommand();
             if (usertype == "user")
                 cmd.CommandText = "select trader, ACCOUNT_ID from Account a where flag_deprecated = 0 and ACCOUNT_ID in(select distinct account from PORTFOLIO where  HUB = '" + market + "' and product =" + "'" + product + "' and PORTFOLIO_id in (select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in (select enduser_key from END_USER where ad_login =" + "'" + user + "' ))) order by trader";
             else
                 cmd.CommandText = "select trader, ACCOUNT_ID from Account a where flag_deprecated = 0 and ACCOUNT_ID in(select distinct account from PORTFOLIO where  HUB = '" + market + "' and product =" + "'" + product + "' ) order by trader";
             cmd.Connection = VayuDBConnection;
             SqlDataReader rdr = cmd.ExecuteReader();
             while (rdr.Read())
             {
                 string trader = rdr.GetValue(0).ToString();
                 string accountId = rdr.GetValue(1).ToString();
                 Account account = new Account();
                 account.ID = accountId;
                 account.Trader = trader;
                 accountList.Add(account);
             }
             rdr.Close();
             SqlCommand cmd1 = VayuDBConnection.CreateCommand();
             foreach (Account account in accountList)
             {
                 if (VayuDBConnection.State == ConnectionState.Closed)
                 {
                     VayuDBConnection.Open();
                 }
                 List<Portfolio> portfolioList = new List<Portfolio>();
                 // cmd.CommandText = "select portfolio_id , strip from Portfolio where ACCOUNT = '" + account.ID + "' and product =" + "'" + product + "'  and active='y' order by strip";
                 if (product == "EES/PTP")
                 {
                     if (usertype == "user")
                         cmd.CommandText = "select Portfolio_ID,STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = " + "'" + market + "' and A.flag_deprecated=0 and ACCOUNT = '" + account.ID + "' and product = " + "'" + product + "' and ACTIVE = 'Y'   and  PORTFOLIO_id in (select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in (select enduser_key from END_USER where ad_login=" + "'" + user + "')) order by STRIP";
                     else
                         cmd.CommandText = "select  Portfolio_ID,STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where ACCOUNT = '" + account.ID + "'  and HUB =  " + "'" + market + "'  and A.flag_deprecated=0and product = " + "'" + product + "' and ACTIVE = 'Y' order by STRIP"; // PJM  'EES/PTP'

                 }
                 else
                 {
                     if (usertype == "user")
                         cmd.CommandText = "select Portfolio_ID, STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = " + "'" + market + "' and A.flag_deprecated=0 and ACCOUNT = '" + account.ID + "' and product = " + "'" + product + "' and ACTIVE = 'Y'   and  PORTFOLIO_id in (select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in (select enduser_key from END_USER where ad_login=" + "'" + user + "')) order by STRIP";
                     else
                         cmd.CommandText = "select  Portfolio_ID, STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where ACCOUNT = '" + account.ID + "'  and HUB =" + "'" + market + "'  and A.flag_deprecated=0and product = " + "'" + product + "'  and ACTIVE = 'Y' order by STRIP"; //PJM  virtual

                 }
                 cmd.Connection = VayuDBConnection;
                 SqlDataReader rdr1 = cmd.ExecuteReader();
                 while (rdr1.Read())
                 {
                     int port = Convert.ToInt32(rdr1.GetValue(0));
                     string strip = rdr1.GetValue(1).ToString();
                     Portfolio portfolio = new Portfolio();
                     if (product.ToUpper().Equals("EES/PTP"))
                     {
                         portfolio.IsUptos = true;
                     }
                     else
                     {
                         portfolio.IsUptos = false;
                     }
                     if (market == "PJM")
                     {
                         portfolio.Market = "PJM";
                         portfolio.MarketKey = 1;
                         portfolio.Name = strip;
                         portfolio.ID = port;
                         portfolioList.Add(portfolio);
                     }
                     else if (market == "ERCOT")
                     {
                         portfolio.Market = "ERCOT";
                         portfolio.MarketKey = 9;
                         portfolio.Name = strip;
                         portfolio.ID = port;
                         portfolioList.Add(portfolio);
                     }
                 }
                 rdr1.Close();
                 if (VayuDBConnection.State == ConnectionState.Open)
                 {
                     VayuDBConnection.Close();
                 }
                 account.PortfolioList = portfolioList.ToList();
             }
             return accountList;
         }*/

        public static List<Account> GetAccount(string user, string product, string market)
        {
            LoadDB();
            List<Account> accountList = new List<Account>();
            if (market != "ERCOT External")
            {
                string usertype = "";

                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                SqlCommand cmdusertype = VayuDBConnection.CreateCommand();
                cmdusertype.CommandText = "select user_type from END_USER where ad_login=" + "'" + user + "' ";
                cmdusertype.Connection = VayuDBConnection;
                SqlDataReader cmdusertypedr = cmdusertype.ExecuteReader();
                while (cmdusertypedr.Read())
                {
                    usertype = cmdusertypedr.GetValue(0).ToString();
                }
                SqlCommand cmd = VayuDBConnection.CreateCommand();
                if (usertype == "user")
                    cmd.CommandText = "select trader, ACCOUNT_ID from Account a where flag_deprecated = 0 and ACCOUNT_ID in(select distinct account from PORTFOLIO where  HUB = '" + market + "' and product =" + "'" + product + "' and PORTFOLIO_id in (select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in (select enduser_key from END_USER where ad_login =" + "'" + user + "' ))) order by trader";
                else
                    cmd.CommandText = "select trader, ACCOUNT_ID from Account a where flag_deprecated = 0 and ACCOUNT_ID in(select distinct account from PORTFOLIO where  HUB = '" + market + "' and product =" + "'" + product + "' ) order by trader";
                cmd.Connection = VayuDBConnection;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string trader = rdr.GetValue(0).ToString();
                    string accountId = rdr.GetValue(1).ToString();
                    Account account = new Account();
                    account.ID = accountId;
                    account.Trader = trader;
                    accountList.Add(account);
                }
                rdr.Close();
                SqlCommand cmd1 = VayuDBConnection.CreateCommand();
                foreach (Account account in accountList)
                {
                    if (VayuDBConnection.State == ConnectionState.Closed)
                    {
                        VayuDBConnection.Open();
                    }
                    List<Portfolio> portfolioList = new List<Portfolio>();
                    // cmd.CommandText = "select portfolio_id , strip from Portfolio where ACCOUNT = '" + account.ID + "' and product =" + "'" + product + "'  and active='y' order by strip";
                    if (product == "EES/PTP")
                    {
                        if (usertype == "user")
                            cmd.CommandText = "select Portfolio_ID,STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = " + "'" + market + "' and A.flag_deprecated=0 and ACCOUNT = '" + account.ID + "' and product = " + "'" + product + "' and ACTIVE = 'Y'   and  PORTFOLIO_id in (select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in (select enduser_key from END_USER where ad_login=" + "'" + user + "')) order by STRIP";
                        else
                            cmd.CommandText = "select  Portfolio_ID,STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where ACCOUNT = '" + account.ID + "'  and HUB =  " + "'" + market + "'  and A.flag_deprecated=0and product = " + "'" + product + "' and ACTIVE = 'Y' order by STRIP"; // PJM  'EES/PTP'

                    }
                    else
                    {
                        if (usertype == "user")
                            cmd.CommandText = "select Portfolio_ID, STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = " + "'" + market + "' and A.flag_deprecated=0 and ACCOUNT = '" + account.ID + "' and product = " + "'" + product + "' and ACTIVE = 'Y'   and  PORTFOLIO_id in (select distinct portfoliokey from END_USER_MARKET_PORTFOLIO where end_user_key in (select enduser_key from END_USER where ad_login=" + "'" + user + "')) order by STRIP";
                        else
                            cmd.CommandText = "select  Portfolio_ID, STRIP from PORTFOLIO P inner join account A on P.account=A.account_id where ACCOUNT = '" + account.ID + "'  and HUB =" + "'" + market + "'  and A.flag_deprecated=0and product = " + "'" + product + "'  and ACTIVE = 'Y' order by STRIP"; //PJM  virtual

                    }
                    cmd.Connection = VayuDBConnection;
                    SqlDataReader rdr1 = cmd.ExecuteReader();
                    while (rdr1.Read())
                    {
                        int port = Convert.ToInt32(rdr1.GetValue(0));
                        string strip = rdr1.GetValue(1).ToString();
                        Portfolio portfolio = new Portfolio();
                        if (product.ToUpper().Equals("EES/PTP"))
                        {
                            portfolio.IsUptos = true;
                        }
                        else
                        {
                            portfolio.IsUptos = false;
                        }
                        if (market == "PJM")
                        {
                            portfolio.Market = "PJM";
                            portfolio.MarketKey = 1;
                            portfolio.Name = strip;
                            portfolio.ID = port;
                            portfolioList.Add(portfolio);
                        }
                        else if (market == "ERCOT")
                        {
                            portfolio.Market = "ERCOT";
                            portfolio.MarketKey = 9;
                            portfolio.Name = strip;
                            portfolio.ID = port;
                            portfolioList.Add(portfolio);
                        }
                    }
                    rdr1.Close();
                    if (VayuDBConnection.State == ConnectionState.Open)
                    {
                        VayuDBConnection.Close();
                    }
                    account.PortfolioList = portfolioList.ToList();
                }
            }
            if (market == "ERCOT External")
            {
                Account account = new Account();
                account.ID = "1";
                account.Trader = "Ercot External";
                accountList.Add(account);
                foreach (Account account1 in accountList)
                {
                    if (VayuDBConnection.State == ConnectionState.Closed)
                    {
                        VayuDBConnection.Open();
                    }
                    SqlCommand cmd = VayuDBConnection.CreateCommand();
                    List<Portfolio> portfolioList = new List<Portfolio>();
                    if (product == "EES/PTP")
                    {
                        cmd.CommandText = "select distinct  b.ParticipantID,b.Participant from Vayu..DAM60DAYPTPAWARDS a join Vayu..Company b ON a.ParticipantID=b.ParticipantID where b.Marketkey=9 and b.Participant not in ('QTALLR' ,'QISO2') order by b.Participant";
                        cmd.Connection = VayuDBConnection;
                        SqlDataReader rdr1 = cmd.ExecuteReader();
                        while (rdr1.Read())
                        {
                            int port = Convert.ToInt32(rdr1.GetValue(0));
                            string strip = rdr1.GetValue(1).ToString();
                            Portfolio portfolio = new Portfolio();
                            if (product.ToUpper().Equals("EES/PTP"))
                            {
                                portfolio.IsUptos = true;
                            }
                            else
                            {
                                portfolio.IsUptos = false;
                            }
                            portfolio.Market = "ERCOT External";
                            portfolio.MarketKey = 9;
                            portfolio.Name = strip;
                            portfolio.ID = port;
                            portfolioList.Add(portfolio);
                        }
                        rdr1.Close();
                        if (VayuDBConnection.State == ConnectionState.Open)
                        {
                            VayuDBConnection.Close();
                        }
                        account.PortfolioList = portfolioList.ToList();
                    }


                }

            }
            return accountList;
        }

        /// <summary>
        /// Gets all node releated data on the basis of market key.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <param name="nodeTypeHash">The node type hash.</param>
        /// <param name="zoneHash">The zone hash.</param>
        /// <returns></returns>
        public static Dictionary<string, NodeDetail> GetAllNodes(int marketKey, Dictionary<int, List<string>> nodeTypeHash, Dictionary<int, List<string>> zoneHash)
        {
            LoadDB();
            VayuDBConnection.Open();
            SqlDataReader reader = null;
            if (marketKey == 1)
            {
                cmdSelectAllNodes.Parameters["@MarketKey"].Value = marketKey;
                reader = cmdSelectAllNodes.ExecuteReader();
            }
            else
            {
                cmdSelectAllErcotNodes.Parameters["@MarketKey"].Value = marketKey;
                reader = cmdSelectAllErcotNodes.ExecuteReader();
            }
            Dictionary<string, NodeDetail> nodeHash = new Dictionary<string, NodeDetail>();
            while (reader.Read())
            {
                NodeDetail nodeDetail = new NodeDetail();
                string nodeName = reader.GetString(0);
                //int nodeKey = (int)reader.GetDecimal(1);
                int nodeKey = reader.GetInt32(1);
                string nodeType = reader.GetString(3);
                string zone = null;
                if (!reader.IsDBNull(2))
                {
                    zone = reader.GetString(2);
                }
                long externalNodeId = 0;
                if (!reader.IsDBNull(4))
                {
                    externalNodeId = (Int64)reader.GetDecimal(4);
                }
                string nodeTypeKey = reader.GetString(5);
                nodeDetail.Name = nodeName;
                nodeDetail.ID = nodeKey;
                nodeDetail.Zone = zone;
                nodeDetail.Type = nodeType;
                nodeDetail.ExternalID = externalNodeId;
                nodeDetail.TypeKey = Convert.ToInt32(nodeTypeKey);
                if (!nodeHash.ContainsKey(nodeName))
                    nodeHash.Add(nodeName, nodeDetail);
                List<string> nodeTypeList = new List<string>();
                if (nodeTypeHash.ContainsKey(marketKey))
                {
                    nodeTypeList = nodeTypeHash[marketKey];
                    nodeTypeHash.Remove(marketKey);
                }
                nodeTypeHash.Add(marketKey, nodeTypeList);
                if (!nodeTypeList.Contains(nodeType) && nodeType.Trim().Length > 0)
                {
                    nodeTypeList.Add(nodeType);
                }
                List<string> zoneList = new List<string>();
                if (zoneHash.ContainsKey(marketKey))
                {
                    zoneList = zoneHash[marketKey];
                    zoneHash.Remove(marketKey);
                }
                if (zone != null && !zoneList.Contains(zone) && zone.Trim().Length > 0)
                {
                    zoneList.Add(zone);
                }
                string key = nodeName + marketKey;
                if (!dictNodeNameHash.ContainsKey(key))
                {
                    dictNodeNameHash.Add(key, nodeKey);
                }
                zoneHash.Add(marketKey, zoneList);
            }
            reader.Close();
            VayuDBConnection.Close();
            return nodeHash;
        }
        /// <summary>
        /// Gets the name of the portfolio.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <returns></returns>
        public static string GetPortfolioName(int portfolioKey)
        {
            if (dictPortfolioHash.ContainsKey(portfolioKey))
            {
                return dictPortfolioHash[portfolioKey];
            }
            LoadDB();
            VayuDBConnection.Open();
            cmdSelectPortfolioName.Parameters["@portfolio_id"].Value = portfolioKey;
            SqlDataReader reader = cmdSelectPortfolioName.ExecuteReader();
            string account = null;
            while (reader.Read())
            {
                account = reader.GetString(0);
            }
            reader.Close();
            VayuDBConnection.Close();
            if (account != null)
            {
                dictPortfolioHash.Add(portfolioKey, account);
            }
            return account;
        }

        public static string GetExternalPortfolioName(int portfoliokey)
        {
            if (dictExternalPortfolioHash.ContainsKey(portfoliokey))
            {
                return dictExternalPortfolioHash[portfoliokey];
            }
            LoadDB(); //cmdSelectExternalPortfolioName
            VayuDBConnection.Open();
            cmdSelectExternalPortfolioName.Parameters["@ParticipantID"].Value = portfoliokey;
            SqlDataReader reader = cmdSelectExternalPortfolioName.ExecuteReader();
            string account = null;
            while (reader.Read())
            {
                account = reader.GetString(0);
            }
            reader.Close();
            VayuDBConnection.Close();
            if (account != null)
            {
                dictExternalPortfolioHash.Add(portfoliokey, account);
            }
            return account;
        }
        /// <summary>
        /// Gets the cleareds.
        /// </summary>
        /// <param name="isUpto">if set to <c>true</c> [is upto].</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /* public static List<Bid> GetCleareds(bool isUpto, int marketKey, int portfolioKey, DateTime startDate, DateTime endDate)
         {
             LoadDB();
             List<Bid> bidList = new List<Bid>();
             SqlDataReader reader = null;
             VayuDBConnection.Open();
             if (isUpto)
             {

                 if (marketKey == 1)
                 {
                     cmdSelectUptoCleared.Parameters["@portfoliokey"].Value = portfolioKey;
                     cmdSelectUptoCleared.Parameters["@startdate"].Value = startDate;
                     cmdSelectUptoCleared.Parameters["@enddate"].Value = endDate.AddDays(1);
                     reader = cmdSelectUptoCleared.ExecuteReader();
                 }
                 if (marketKey == 9)
                 {
                     cmdErcotSelectUptoCleared.Parameters["@portfoliokey"].Value = portfolioKey;
                     cmdErcotSelectUptoCleared.Parameters["@startdate"].Value = startDate;
                     cmdErcotSelectUptoCleared.Parameters["@enddate"].Value = endDate.AddDays(1);
                     reader = cmdErcotSelectUptoCleared.ExecuteReader();
                 }
             }
             else
             {
                 cmdSelectVirtualCleared.Parameters["@portfoliokey"].Value = portfolioKey;
                 cmdSelectVirtualCleared.Parameters["@startdate"].Value = startDate;
                 cmdSelectVirtualCleared.Parameters["@enddate"].Value = endDate.AddDays(1);
                 reader = cmdSelectVirtualCleared.ExecuteReader();
             }
             while (reader.Read())
             {
                 Bid bid = new Bid();
                 bid.PortfolioKey = portfolioKey;
                 bid.IsUptos = isUpto;
                 bid.Market = marketKey;
                 bid.Source = (int)reader.GetDecimal(0);
                 bid.MarketDateTime = reader.GetDateTime(2);
                 if (isUpto)
                 {
                     bid.Sink = (int)reader.GetDecimal(3);
                     bid.MW = (double)reader.GetDecimal(1);
                 }
                 else
                 {
                     string incDec = reader.GetString(3);
                     bid.MW = (incDec.ToUpper() == "I" || incDec.ToUpper() == "INC") ? -(double)reader.GetDecimal(1) : (double)reader.GetDecimal(1);
                 }
                 bidList.Add(bid);
             }
             reader.Close();
             VayuDBConnection.Close();
             return bidList;
         } */

        public static List<Bid> GetCleareds(bool isUpto, int marketKey, int portfolioKey, DateTime startDate, DateTime endDate)
        {
            LoadDB();
            List<Bid> bidList = new List<Bid>();
            SqlDataReader reader = null;
            VayuDBConnection.Open();
            if (isUpto)
            {

                if (marketKey == 1)
                {
                    cmdSelectUptoCleared.Parameters["@portfoliokey"].Value = portfolioKey;
                    cmdSelectUptoCleared.Parameters["@startdate"].Value = startDate;
                    cmdSelectUptoCleared.Parameters["@enddate"].Value = endDate.AddDays(1);
                    reader = cmdSelectUptoCleared.ExecuteReader();
                }
                if (marketKey == 9)
                {
                    cmdErcotSelectUptoCleared.Parameters["@portfoliokey"].Value = portfolioKey;
                    cmdErcotSelectUptoCleared.Parameters["@startdate"].Value = startDate;
                    cmdErcotSelectUptoCleared.Parameters["@enddate"].Value = endDate.AddDays(1);
                    reader = cmdErcotSelectUptoCleared.ExecuteReader();
                }
                if (marketKey == 10)
                {
                    string sDate = startDate.AddMonths(-2).ToString("yyyy-MM-dd");
                    string eDate = endDate.AddMonths(-2).AddDays(1).ToString("yyyy-MM-dd");

                    cmdErcotExternalSelectUptoCleared.Parameters["@portfoliokey"].Value = portfolioKey;
                    cmdErcotExternalSelectUptoCleared.Parameters["@startdate"].Value = sDate;
                    cmdErcotExternalSelectUptoCleared.Parameters["@enddate"].Value = eDate;
                    reader = cmdErcotExternalSelectUptoCleared.ExecuteReader();

                }
            }
            else
            {
                cmdSelectVirtualCleared.Parameters["@portfoliokey"].Value = portfolioKey;
                cmdSelectVirtualCleared.Parameters["@startdate"].Value = startDate;
                cmdSelectVirtualCleared.Parameters["@enddate"].Value = endDate.AddDays(1);
                reader = cmdSelectVirtualCleared.ExecuteReader();
            }
            while (reader.Read())
            {
                Bid bid = new Bid();
                if (marketKey == 10)
                {
                    bid.PortfolioKey = portfolioKey;
                    bid.IsUptos = isUpto;
                    bid.Market = 9;
                    bid.Source = (int)reader.GetValue(0);
                    bid.MarketDateTime = reader.GetDateTime(2);
                    bid.Sink = (int)reader.GetValue(3);
                    bid.MW = (double)reader.GetDecimal(1);


                }
                else
                {
                    bid.PortfolioKey = portfolioKey;
                    bid.IsUptos = isUpto;
                    bid.Market = marketKey;
                    bid.Source = (int)reader.GetDecimal(0);
                    bid.MarketDateTime = reader.GetDateTime(2);
                    if (isUpto)
                    {
                        bid.Sink = (int)reader.GetDecimal(3);
                        bid.MW = (double)reader.GetDecimal(1);
                    }
                    else
                    {
                        string incDec = reader.GetString(3);
                        bid.MW = (incDec.ToUpper() == "I" || incDec.ToUpper() == "INC") ? -(double)reader.GetDecimal(1) : (double)reader.GetDecimal(1);
                    }


                }
                bidList.Add(bid);
            }
            reader.Close();
            VayuDBConnection.Close();
            return bidList;
        }
        /// <summary>
        /// Gets the forfeiture virtual list.
        /// </summary>
        /// <returns></returns>
        public static Tuple<List<string>, List<string>> GetForfeitureVirtualList()
        {
            LoadDB();
            if (sForfeitureVirtualList == null)
            {
                List<string> decList = new List<string>();
                List<string> incList = new List<string>();
                VayuDBConnection.Open();
                cmdSelectVirtualForfeiture.Parameters["@monthfft"].Value = DateTime.Parse(DateTime.Today.Month + "/1/" + DateTime.Today.Year);
                SqlDataReader reader = cmdSelectVirtualForfeiture.ExecuteReader();
                while (reader.Read())
                {
                    string node = reader.GetString(0);
                    string incDec = reader.GetString(1);
                    if (incDec.IndexOf("DEC") != -1)
                    {
                        if (!decList.Contains(node))
                        {
                            decList.Add(node);
                        }
                    }
                    if (incDec.IndexOf("INC") != -1)
                    {
                        if (!incList.Contains(node))
                        {
                            incList.Add(node);
                        }
                    }

                }
                VayuDBConnection.Close();
                sForfeitureVirtualList = new Tuple<List<string>, List<string>>(decList, incList);
            }
            return sForfeitureVirtualList;
        }
        /// <summary>
        /// Gets the forfeiture uptos list.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetForfeitureUptosList()
        {
            LoadDB();
            if (lstForfeitureUptosList.Count == 0)
            {
                VayuDBConnection.Open();
                cmdSelectUptosForfeiture.Parameters["@monthfft"].Value = DateTime.Parse(DateTime.Today.Month + "/1/" + DateTime.Today.Year);
                SqlDataReader reader = cmdSelectUptosForfeiture.ExecuteReader();
                while (reader.Read())
                {
                    string source = reader.GetString(0);
                    string sink = reader.GetString(1);
                    lstForfeitureUptosList.Add(source + "?" + sink);
                }
                VayuDBConnection.Close();
            }
            return lstForfeitureUptosList;
        }
        /// <summary>
        /// Gets the virtual bids holders.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <param name="marketDate">The market date.</param>
        /// <param name="trader">The trader.</param>
        /// <returns></returns>
        public static List<int> GetVirtualBidsHolders(int portfolioKey, DateTime marketDate, string trader)
        {
            List<int> bidList = new List<int>();
            VayuDBConnection.Open();
            cmdSelectVirtualBidsNodeHolder.Parameters["@portfoliokey"].Value = portfolioKey;
            cmdSelectVirtualBidsNodeHolder.Parameters["@marketDate"].Value = marketDate;
            cmdSelectVirtualBidsNodeHolder.Parameters["@trader"].Value = trader;
            SqlDataReader reader = cmdSelectVirtualBidsNodeHolder.ExecuteReader();
            while (reader.Read())
            {
                bidList.Add(reader.GetInt32(0));
            }
            reader.Close();
            VayuDBConnection.Close();
            return bidList;
        }
        /// <summary>
        /// Moves to virtual bid holder.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <param name="marketDate">The market date.</param>
        /// <param name="trader">The trader.</param>
        /// <param name="nodeIncDecList">The node inc decimal list.</param>
        public static void MoveToVirtualBidHolder(int portfolioKey, DateTime marketDate, string trader, List<Tuple<int, string>> nodeIncDecList)
        {
            VayuDBConnection.Open();
            foreach (Tuple<int, string> nodeIncDecTuple in nodeIncDecList)
            {
                cmdInsertVirtualBidsHolder.Parameters["@portfoliokey"].Value = portfolioKey;
                cmdInsertVirtualBidsHolder.Parameters["@marketdate"].Value = marketDate;
                cmdInsertVirtualBidsHolder.Parameters["@trader"].Value = trader;
                cmdInsertVirtualBidsHolder.Parameters["@nodekey"].Value = nodeIncDecTuple.Item1;
                cmdInsertVirtualBidsHolder.Parameters["@incdec"].Value = nodeIncDecTuple.Item2;
                cmdInsertVirtualBidsHolder.ExecuteNonQuery();
                cmdDeleteValidVirtualBids.Parameters["@portfoliokey"].Value = portfolioKey;
                cmdDeleteValidVirtualBids.Parameters["@marketdate"].Value = marketDate;
                cmdDeleteValidVirtualBids.Parameters["@trader"].Value = trader;
                cmdDeleteValidVirtualBids.Parameters["@nodekey"].Value = nodeIncDecTuple.Item1;
                cmdDeleteValidVirtualBids.Parameters["@incdec"].Value = nodeIncDecTuple.Item2;
                cmdDeleteValidVirtualBids.ExecuteNonQuery();
            }
            VayuDBConnection.Close();
        }
        /// <summary>
        /// Gets the fee per mw.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="market">The market.</param>
        /// <param name="product">The product.</param>
        /// <param name="portfolioKeyList">The portfolio key list.</param>
        /// <returns></returns>
        public static double GetFeePerMW(DateTime startDate, DateTime endDate, int market, string product, List<int> portfolioKeyList)
        {
            Dictionary<DateTime, double> mwHash = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> feeHash = new Dictionary<DateTime, double>();
            VayuDBConnection.Open();
            if (product == "UPTO")
            {
                cmdSelectMW.CommandText = "select marketdatetime, clearedmw from clearedees where marketdatetime between @start and @end and portfoliokey = @portfoliokey";
            }
            else
            {
                cmdSelectMW.CommandText = "select marketdatetime, clearedmw from clearedbids where marketdatetime between @start and @end"; //and portfoliokey = @portfoliokey";
            }
            foreach (int portfolioKey in portfolioKeyList)
            {
                cmdSelectMW.Parameters["@start"].Value = startDate;
                cmdSelectMW.Parameters["@end"].Value = endDate;
                cmdSelectMW.Parameters["@portfoliokey"].Value = portfolioKey;
                SqlDataReader reader = cmdSelectMW.ExecuteReader();
                DateTime feesDate = DateTime.Today;
                while (reader.Read())
                {
                    DateTime date = reader.GetDateTime(0);
                    feesDate = date;
                    double mw = Math.Abs((double)reader.GetDecimal(1));
                    date = date.Hour == 0 ? date.AddDays(1).Date : date.Date;
                    if (mwHash.ContainsKey(date))
                    {
                        mw += mwHash[date];
                        mwHash.Remove(date);
                    }
                    mwHash.Add(date, mw);
                }
                reader.Close();
                //  cmdSelectPnlFee.Parameters["@start"].Value = startDate;
                //   cmdSelectPnlFee.Parameters["@end"].Value = endDate;
                // cmdSelectPnlFee.Parameters["@portfoliokey"].Value = portfolioKey;
                // reader = cmdSelectPnlFee.ExecuteReader();
                //   while (reader.Read())
                {
                    DateTime date = feesDate;
                    double fee = 0.06;
                    if (feeHash.ContainsKey(date))
                    {
                        fee = feeHash[date];
                        feeHash.Remove(date);
                    }
                    feeHash.Add(date, fee);
                }
                //   reader.Close();
            }
            List<DateTime> dateList = mwHash.Keys.ToList<DateTime>();
            double totalMw = 0;
            double totalFees = 0;
            foreach (DateTime date in dateList)
            {
                if (mwHash.ContainsKey(date) && feeHash.ContainsKey(date))
                {
                    totalMw += mwHash[date];
                    totalFees += feeHash[date];
                }
            }
            double feeRate = 0;
            if (totalFees != 0)
            {
                feeRate = totalFees / totalMw;
            }
            //else
            //{
            //    cmdSelectLastFee.Parameters["@tradetype"].Value = product == "Virtual" ? "Virtual" : "EES/PTP";
            //    cmdSelectLastFee.Parameters["@market"].Value = market;
            //    SqlDataReader reader = cmdSelectLastFee.ExecuteReader();
            //    int portfolioKey = 0;
            //    DateTime pnlDate = DateTime.Today;
            //    double lastFee = 0;
            //    while (reader.Read())
            //    {
            //        double fee = (double)reader.GetDecimal(2);
            //        if (lastFee == 0 || Math.Abs(lastFee) < Math.Abs(fee))
            //        {
            //            portfolioKey = Int32.Parse(reader.GetString(0));
            //            pnlDate = reader.GetDateTime(1);
            //            lastFee = fee;
            //        }
            //    }
            //    reader.Close();
            //    cmdSelectMW.Parameters["@start"].Value = pnlDate;
            //    cmdSelectMW.Parameters["@end"].Value = pnlDate.AddDays(1);
            //    cmdSelectMW.Parameters["@portfoliokey"].Value = portfolioKey;
            //    reader = cmdSelectMW.ExecuteReader();
            //    double mw = 0;
            //    while (reader.Read())
            //    {
            //        mw += Math.Abs((double)reader.GetDecimal(1));
            //    }
            //    reader.Close();
            //    feeRate = mw == 0 ? 0 : lastFee / mw;
            //}
            VayuDBConnection.Close();
            //return Math.Abs(0.06);
            return Math.Abs(0.08);
        }
        /// <summary>
        /// Gets the coordinate.
        /// </summary>
        /// <param name="nodeKey">The node key.</param>
        /// <returns></returns>
        public static Coordinate GetCoordinate(int nodeKey)
        {
            Coordinate coordinate = new Coordinate();
            if (!dictCoordinateHash.ContainsKey(nodeKey))
            {
                VayuDBConnection.Open();
                cmdSelectCoordinate.Parameters["@nodekey"].Value = nodeKey;
                SqlDataReader reader = cmdSelectCoordinate.ExecuteReader();
                while (reader.Read())
                {
                    coordinate.Longitude = (double)reader.GetDecimal(0);
                    coordinate.Latitude = (double)reader.GetDecimal(1);
                    dictCoordinateHash.Add(nodeKey, coordinate);
                    break;
                }
                reader.Close();
                VayuDBConnection.Close();
            }
            if (dictCoordinateHash.ContainsKey(nodeKey))
            {
                coordinate = dictCoordinateHash[nodeKey];
            }
            return coordinate;
        }
        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        /// <param name="nodeTypeKey">The node type key.</param>
        /// <returns></returns>
        public static string GetNodeType(int nodeTypeKey)
        {
            LoadDB();
            if (dictNodeTypeHash.Count == 0)
            {
                if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                SqlDataReader reader = cmdSelectNodeType.ExecuteReader();
                while (reader.Read())
                {
                    int tempNodeKey = (int)reader.GetDecimal(0);
                    string label = reader.GetString(1);
                    dictNodeTypeHash.Add(tempNodeKey, label);
                }
                reader.Close();
            }
            if (dictNodeTypeHash.ContainsKey(nodeTypeKey))
            {
                return dictNodeTypeHash[nodeTypeKey];
            }
            return null;
        }
        /// <summary>
        /// Gets the FTR transactions.
        /// </summary>
        /// <param name="auction">The auction.</param>
        /// <returns></returns>
        public static List<FTRTransaction> GetFtrTransactions(string auction, int marketkey)
        {
            LoadDB();
            //
            cmdSelectFtrTransaction = new SqlCommand();
            if (marketkey == 1)
            {
                cmdSelectFtrTransaction.CommandText = "select transaction_id,round, ftr_type, ftr_date from ftr_transaction where market = @market order by ftr_date desc ";
                cmdSelectFtrTransaction.Parameters.AddWithValue("@market", "market");
                cmdSelectFtrTransaction.Connection = VayuDBConnection;
            }
            else if (marketkey == 9)
            {
                cmdSelectFtrTransaction.CommandText = "select transaction_id,round, ftr_type, ftr_date from crr_transaction where market = @market order by ftr_date desc ";
                cmdSelectFtrTransaction.Parameters.AddWithValue("@market", "market");
                cmdSelectFtrTransaction.Connection = VayuDBConnection;
            }

            List<FTRTransaction> transactions = new List<FTRTransaction>();
            VayuDBConnection.Open();
            VayuDBConnection.Open();
            cmdSelectFtrTransaction.Parameters["@market"].Value = auction;
            try
            {
                SqlDataReader reader = cmdSelectFtrTransaction.ExecuteReader();
                while (reader.Read())
                {
                    FTRTransaction transaction = new FTRTransaction();
                    transaction.ID = reader.GetString(0);
                    transaction.Round = reader.GetInt32(1);
                    transaction.Name = reader.GetString(2);
                    transaction.Date = reader.GetDateTime(3);
                    transaction.Market = auction;
                    transactions.Add(transaction);
                }
                reader.Close();
                VayuDBConnection.Close();
                VayuDBConnection.Close();
            }
            catch (Exception ex)
            {

            }
            return transactions;

        }
        /// <summary>
        /// Gets the FTR bid list.
        /// </summary>
        /// <param name="portfoliokey">The portfoliokey.</param>
        /// <param name="auction">The auction.</param>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        /// <param name="marketKey">The market key.</param>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public static List<FTRBid> GetFtrBidList(int portfoliokey, string auction, bool isValid, int marketKey, List<string> ids)
        {
            LoadDB();
            int count = isValid ? 2 : 1;
            List<FTRBid> ftrBidList = new List<FTRBid>();

            if (string.IsNullOrEmpty(auction))
                return ftrBidList;

            string idStr = string.Empty;
            if (ids != null && ids.Count > 0)
            {
                foreach (var item in ids)
                    idStr += "'" + item + "',";

                idStr = idStr.Trim(',');
            }

            VayuDBConnection.Open();
            VayuDBConnection.Open();
            if (marketKey == 1)
            {
                //cmdSelectFtrBid.CommandText = "select FB.source,FB.sink,FB. mw1,FB. price1,FB. mw2,FB. price2,FB. mw3,FB. price3,FB. mw4,FB. price4,FB. mw5,FB. price5,FB. mw6,FB. price6,FB.classtype,FB.tradetype, " +
                //                              " FB.hedgetype,FB.periodhours,FB.periodname,FB.sourceextid,FB.sinkextid,FB.ftrbidskey,FB.Status,FB.tcrid,FB.periodkey,SourceZone.Zone as SourceZone," +
                //                              " SinkZone.Zone as SinkZone from ftrbids  (nolock) as FB  Join Node As SourceZone on FB.Source=SourceZone.NodeName Join Node AS SinkZone on FB.Sink=SinkZone.NodeName " +
                //                              //" where FB.portfoliokey = @portfoliokey and FB.auction =@auction and FB.status <> 'IMPORTED'  and SourceZone.MarketKey=1 and SinkZone.MarketKey=1";
                //                              " where FB.portfoliokey = @portfoliokey and FB.auction =@auction   and SourceZone.MarketKey=1 and SinkZone.MarketKey=1";
                //cmdSelectFtrBid.Connection = VayuDBConnection;

                cmdSelectFtrBid.CommandText = "select FB.source,FB.sink,FB. mw1,FB. price1,FB. mw2,FB. price2,FB. mw3,FB. price3,FB. mw4,FB. price4,FB. mw5,FB. price5,FB. mw6,FB. price6,FB.classtype,FB.tradetype, " +
                                             " FB.hedgetype,FB.periodhours,FB.periodname,FB.sourceextid,FB.sinkextid,FB.ftrbidskey,FB.Status,FB.tcrid,FB.periodkey,SourceZone.Zone as SourceZone," +
                                             " SinkZone.Zone as SinkZone,SourceZone.Nodekey as SourceNodekey, SinkZone.Nodekey as SinkNodeKey from ftrbids  (nolock) as FB  Join Node As SourceZone on FB.Source=SourceZone.NodeName Join Node AS SinkZone on FB.Sink=SinkZone.NodeName " +
                                             //" where FB.portfoliokey = @portfoliokey and FB.auction =@auction and FB.status <> 'IMPORTED'  and SourceZone.MarketKey=1 and SinkZone.MarketKey=1";
                                             " where FB.portfoliokey = @portfoliokey and FB.auction =@auction   and SourceZone.MarketKey=1 and SinkZone.MarketKey=1";
                cmdSelectFtrBid.Connection = VayuDBConnection;
            }
            else if (marketKey == 9)
            {
                //cmdSelectFtrBid.CommandText = "select FB.source,FB.sink,FB. mw1,FB. price1,FB. mw2,FB. price2,FB. mw3,FB. price3,FB. mw4,FB. price4,FB. mw5,FB. price5,FB. mw6,FB. price6,FB.classtype,FB.tradetype, " +
                //                             " FB.hedgetype,FB.periodhours,FB.periodname,FB.sourceextid,FB.sinkextid,FB.CRRBidsKey,FB.Status,FB.tcrid,FB.periodkey,SourceZone.Zone as SourceZone," +
                //                             " SinkZone.Zone as SinkZone from crrbids  (nolock) as FB  Join Node As SourceZone on FB.Source=SourceZone.NodeName Join Node AS SinkZone on FB.Sink=SinkZone.NodeName " +
                //                             // " where FB.portfoliokey = @portfoliokey and FB.auction =@auction and FB.status <> 'IMPORTED'  and SourceZone.MarketKey=9 and SinkZone.MarketKey=9";
                //                             " where FB.portfoliokey = @portfoliokey and FB.auction =@auction and SourceZone.MarketKey=9 and SinkZone.MarketKey=9";
                //cmdSelectFtrBid.Connection = VayuErcotDBConnection;

                cmdSelectFtrBid.CommandText = "select FB.source,FB.sink,FB. mw1,FB. price1,FB. mw2,FB. price2,FB. mw3,FB. price3,FB. mw4,FB. price4,FB. mw5,FB. price5,FB. mw6,FB. price6,FB.classtype,FB.tradetype, " +
                                            " FB.hedgetype,FB.periodhours,FB.periodname,FB.sourceextid,FB.sinkextid,FB.CRRBidsKey,FB.Status,FB.tcrid,FB.periodkey,SourceZone.Zone as SourceZone," +
                                            " SinkZone.Zone as SinkZone,SourceZone.Nodekey as SourceNodekey, SinkZone.Nodekey as SinkNodeKey from crrbids  (nolock) as FB  Join Node As SourceZone on FB.Source=SourceZone.NodeName Join Node AS SinkZone on FB.Sink=SinkZone.NodeName " +
                                            // " where FB.portfoliokey = @portfoliokey and FB.auction =@auction and FB.status <> 'IMPORTED'  and SourceZone.MarketKey=9 and SinkZone.MarketKey=9";
                                            " where FB.portfoliokey = @portfoliokey and FB.auction =@auction and SourceZone.MarketKey=9 and SinkZone.MarketKey=9";
                cmdSelectFtrBid.Connection = VayuDBConnection;
            }
            for (int i = 0; i < count; i++)
            {
                if (i == 1)
                {
                    //cmdSelectFtrBid.CommandText = "select source, sink, mw1, price1, mw2, price2, mw3, price3, mw4, price4, mw5, price5, mw6, price6, classtype, " +
                    //                               "tradetype, hedgetype, periodhours, periodname, sourceextid, sinkextid, ftrbidskey, Status, tcrid, periodkey from ftrbids  (nolock) where " +
                    //                               "portfoliokey = @portfoliokey and auction = @auction and status <> 'IMPORTED' ";

                    if (!string.IsNullOrEmpty(idStr))
                        cmdSelectFtrBid.CommandText += " and TransactionId in (" + idStr + ")";
                }
                cmdSelectFtrBid.Parameters["@portfoliokey"].Value = portfoliokey;
                cmdSelectFtrBid.Parameters["@auction"].Value = auction;

                SqlDataReader reader = cmdSelectFtrBid.ExecuteReader();
                while (reader.Read())
                {
                    FTRBid ftrBid = new FTRBid();
                    ftrBid.ID = Convert.ToInt32(reader.GetValue(21));
                    ftrBid.Source = reader.GetString(0);
                    ftrBid.Sink = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                    {
                        ftrBid.MW1 = reader.GetDouble(2);
                    }
                    if (!reader.IsDBNull(3))
                    {
                        ftrBid.Price1 = reader.GetDouble(3);
                    }
                    if (!reader.IsDBNull(4))
                    {
                        ftrBid.MW2 = reader.GetDouble(4);
                    }
                    if (!reader.IsDBNull(5))
                    {
                        ftrBid.Price2 = reader.GetDouble(5);
                    }
                    if (!reader.IsDBNull(6))
                    {
                        ftrBid.MW3 = reader.GetDouble(6);
                    }
                    if (!reader.IsDBNull(7))
                    {
                        ftrBid.Price3 = reader.GetDouble(7);
                    }
                    if (!reader.IsDBNull(8))
                    {
                        ftrBid.MW4 = reader.GetDouble(8);
                    }
                    if (!reader.IsDBNull(9))
                    {
                        ftrBid.Price4 = reader.GetDouble(9);
                    }
                    if (!reader.IsDBNull(10))
                    {
                        ftrBid.MW5 = reader.GetDouble(10);
                    }
                    if (!reader.IsDBNull(11))
                    {
                        ftrBid.Price5 = reader.GetDouble(11);
                    }
                    if (!reader.IsDBNull(12))
                    {
                        ftrBid.MW6 = reader.GetDouble(12);
                    }
                    if (!reader.IsDBNull(13))
                    {
                        ftrBid.Price6 = reader.GetDouble(13);
                    }
                    ftrBid.ClassType = reader.GetString(14);
                    ftrBid.TradeType = reader.GetString(15);
                    if (ftrBid.TradeType == "SELL" && (marketKey == 12 || marketKey == 2))
                    {
                        ftrBid.ID = reader.GetInt32(23);
                    }
                    ftrBid.HedgeType = reader.GetString(16);
                    ftrBid.PeriodHours = reader.GetInt32(17);
                    ftrBid.PeriodName = reader.GetString(18);
                    ftrBid.Status = reader.GetString(22);
                    ftrBid.PeriodKey = GetInt(reader[24]);

                    ftrBid.SourceZone = reader.IsDBNull(25) ? "" : reader.GetString(25);
                    ftrBid.SinkZone = reader.IsDBNull(26) ? "" : reader.GetString(26);

                    ftrBid.SourceExternalid = reader.IsDBNull(19) ? 0 : Convert.ToInt64(reader.GetValue(19));
                    ftrBid.SinkExternalid = reader.IsDBNull(20) ? 0 : Convert.ToInt64(reader.GetValue(20));
                    ftrBid.SourceNodekey = reader.IsDBNull(27) ? 0 : Convert.ToInt64(reader.GetValue(27));
                    ftrBid.SinkNodekey = reader.IsDBNull(28) ? 0 : Convert.ToInt64(reader.GetValue(28));

                    ftrBidList.Add(ftrBid);
                }
                reader.Close();
            }
            VayuDBConnection.Close();
            VayuDBConnection.Close();
            return ftrBidList;
        }

        public static List<FTRBid> GetFtrBids(int portfoliokey, string auction, int MarketKey)
        {
            Dictionary<int, string> dictPortfolio = GetFtrPortfolio(MarketKey);
            List<FTRBid> ftrBidList = new List<FTRBid>();
            if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
            {
                VayuDBConnection.Open();
            }

            if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
            {
                VayuDBConnection.Open();
            }
            SqlDataReader reader = null;
            if (MarketKey == 1)
            {
                cmsSelectftrBidspeakoffpeak.Parameters["@portfoliokey"].Value = portfoliokey;
                cmsSelectftrBidspeakoffpeak.Parameters["@auction"].Value = auction;
                reader = cmsSelectftrBidspeakoffpeak.ExecuteReader();
            }
            else if (MarketKey == 9)
            {
                cmsSelectftrBidspeakoffpeakErcot.Parameters["@portfoliokey"].Value = portfoliokey;
                cmsSelectftrBidspeakoffpeakErcot.Parameters["@auction"].Value = auction;
                reader = cmsSelectftrBidspeakoffpeakErcot.ExecuteReader();
            }
            while (reader.Read())
            {
                FTRBid ftrBid = new FTRBid();
                ftrBid.ID = Convert.ToInt32(reader.GetValue(21));
                ftrBid.Source = reader.GetString(0);
                ftrBid.Sink = reader.GetString(1);
                // if (!reader.IsDBNull(2))
                {
                    ftrBid.MW1 = reader.IsDBNull(2) ? 00 : reader.GetDouble(2);
                }
                if (!reader.IsDBNull(3))
                {
                    ftrBid.Price1 = reader.GetDouble(3);
                }
                // if (!reader.IsDBNull(4))
                {
                    ftrBid.MW2 = reader.IsDBNull(4) ? 00 : reader.GetDouble(4);
                }
                if (!reader.IsDBNull(5))
                {
                    ftrBid.Price2 = reader.GetDouble(5);
                }
                //  if (!reader.IsDBNull(6))
                {
                    ftrBid.MW3 = reader.IsDBNull(6) ? 00 : reader.GetDouble(6);
                }
                if (!reader.IsDBNull(7))
                {
                    ftrBid.Price3 = reader.GetDouble(7);
                }
                //if (!reader.IsDBNull(8))
                {
                    ftrBid.MW4 = reader.IsDBNull(8) ? 00 : reader.GetDouble(8);
                }
                if (!reader.IsDBNull(9))
                {
                    ftrBid.Price4 = reader.GetDouble(9);
                }
                // if (!reader.IsDBNull(10))
                {
                    ftrBid.MW5 = reader.IsDBNull(10) ? 00 : reader.GetDouble(10);
                }
                if (!reader.IsDBNull(11))
                {
                    ftrBid.Price5 = reader.GetDouble(11);
                }
                // if (!reader.IsDBNull(12))
                {
                    ftrBid.MW6 = reader.IsDBNull(12) ? 00 : reader.GetDouble(12);
                }
                if (!reader.IsDBNull(13))
                {
                    ftrBid.Price6 = reader.GetDouble(13);
                }
                ftrBid.ClassType = reader.GetString(14);
                ftrBid.TradeType = reader.GetString(15);
                ftrBid.HedgeType = reader.GetString(16);
                ftrBid.PeriodHours = reader.GetInt32(17);
                ftrBid.PeriodName = reader.GetString(18);
                ftrBid.Status = reader.GetString(22);
                ftrBid.PeriodKey = GetInt(reader[23]);
                ftrBid.SourceZone = reader.IsDBNull(25) ? "" : reader.GetString(25);
                ftrBid.SinkZone = reader.IsDBNull(26) ? "" : reader.GetString(26);
                int portfilioId = GetInt(reader[27]);
                ftrBid.PortfolioName = Convert.ToString(dictPortfolio[portfilioId]);
                ftrBidList.Add(ftrBid);
            }
            VayuDBConnection.Close();
            VayuDBConnection.Close();
            return ftrBidList.ToList();
        }
        public static Dictionary<int, string> GetFtrPortfolio(int MarketKey)
        {
            Dictionary<int, string> dictPortfolio = new Dictionary<int, string>();
            if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
            {
                VayuDBConnection.Open();
            }
            string market = "";
            if (MarketKey == 1)
                market = "PJM";
            else
                market = "Ercot";
            cmsSelectftrPortfilio.Parameters["@Hub"].Value = market;
            SqlDataReader reader = cmsSelectftrPortfilio.ExecuteReader();
            while (reader.Read())
            {
                string Name = reader.GetString(0);
                int portfolioId = GetInt(reader[1]);
                if (!dictPortfolio.ContainsKey(portfolioId))
                    dictPortfolio.Add(portfolioId, Name);
            }
            VayuDBConnection.Close();
            return dictPortfolio;
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private static int GetInt(object obj)
        {
            int i = 0;
            int.TryParse((obj ?? "").ToString(), out i);
            return i;
        }
        /// <summary>
        /// Gets the thershold.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public static Dictionary<string, double> GetThershold(string user)
        {
            Dictionary<string, double> thresholdHash = new Dictionary<string, double>();
            LoadDB();
            VayuDBConnection.Open();
            cmdSelectLoadThreshold.Parameters["@loads_user"].Value = user;
            SqlDataReader reader = cmdSelectLoadThreshold.ExecuteReader();
            while (reader.Read())
            {
                string zone = reader.GetString(0);
                double threshold = reader.GetDouble(1);
                thresholdHash.Add(zone, threshold);
            }
            reader.Close();
            VayuDBConnection.Close();
            return thresholdHash;
        }
        /// <summary>
        /// Saves the threshold.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="load">The load.</param>
        /// <param name="threshold">The threshold.</param>
        public static void SaveThreshold(string user, Load load, double threshold)
        {
            LoadDB();
            VayuDBConnection.Open();
            cmdDeleteLoadThreshold.Parameters["@loads_user"].Value = user;
            cmdDeleteLoadThreshold.Parameters["@zone"].Value = load.Name;
            cmdDeleteLoadThreshold.ExecuteNonQuery();
            if (threshold != 0)
            {
                cmdInsertLoadThreshold.Parameters["@loads_user"].Value = user;
                cmdInsertLoadThreshold.Parameters["@zone"].Value = load.Name;
                cmdInsertLoadThreshold.Parameters["@threshold"].Value = Math.Abs(threshold);
                cmdInsertLoadThreshold.ExecuteNonQuery();
            }
            VayuDBConnection.Close();
        }
        /// <summary>
        /// Gets the load difference list.
        /// </summary>
        /// <returns></returns>
        public static List<Load> GetLoadDiffList()
        {
            LoadDB();
            List<Load> loadList = new List<Load>();
            VayuDBConnection.Open();
            try
            {
                SqlDataReader reader = cmdSelectLoadDiffList.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0).Trim();
                    int id = (int)reader.GetDecimal(1);
                    if (name == "Total Load")
                    {
                        name = "Total Load";
                    }
                    Load load = new Load();
                    load.Key = id;
                    load.Name = name;
                    loadList.Add(load);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
            }
            VayuDBConnection.Close();
            return loadList;
        }
        /// <summary>
        /// Gets the weather cities.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetWeatherCities()
        {
            List<string> weatherCityList = new List<string>();
            LoadDB();
            VayuDBConnection.Open();
            SqlDataReader reader = cmdSelectWeatherCities.ExecuteReader();
            while (reader.Read())
            {
                weatherCityList.Add(reader.GetString(0));
            }
            reader.Close();
            VayuDBConnection.Close();
            return weatherCityList;
        }
        /// <summary>
        /// Gets the source sink list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public static List<SourceSinkData> GetSourceSinkList(int marketKey)
        {
            LoadDB();
            List<SourceSinkData> sourceSinkDataList = new List<SourceSinkData>();
            if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
            {
                VayuDBConnection.Open();
            }
            cmdSelectSourceSink.Parameters["@market"].Value = marketKey;
            SqlDataReader reader = cmdSelectSourceSink.ExecuteReader();
            while (reader.Read())
            {
                SourceSinkData sourceSinkData = new SourceSinkData();
                PricingNode sourceNode = new PricingNode();
                sourceNode.NodeKey = Convert.ToInt32(reader.GetDecimal(0));
                sourceNode.NodeName = reader.GetString(1);
                sourceNode.ExternalNodeId = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetDecimal(2));
                sourceNode.NodeTypeKey = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetDecimal(3));
                sourceNode.Zone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                sourceNode.MarketKey = marketKey;
                PricingNode sinkNode = new PricingNode();
                sinkNode.NodeKey = Convert.ToInt32(reader.GetDecimal(5));
                sinkNode.NodeName = reader.GetString(6);
                sinkNode.ExternalNodeId = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetDecimal(7));
                sinkNode.NodeTypeKey = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetDecimal(8));
                sinkNode.Zone = reader.IsDBNull(9) ? "" : reader.GetString(9);
                sinkNode.MarketKey = marketKey;
                sourceSinkData.Source = sourceNode;
                sourceSinkData.Sink = sinkNode;
                sourceSinkDataList.Add(sourceSinkData);
            }
            reader.Close();
            VayuDBConnection.Close();
            return sourceSinkDataList;
        }
        /// <summary>
        /// Gets the source sink node list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="market">The market.</param>
        /// <param name="product">The product.</param>
        public static void GetSourceSinkNodeList(Action<Tuple<List<PricingNode>, List<PricingNode>>, Exception> callback, string market, string product)
        {
            LoadDB();
            //if (market == "PJM")
            {
                if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
            }
            //else
            {
                if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                    VayuDBConnection.Open();
            }
            int marketKey = 1;
            if (market == "PJM")
            {
                marketKey = 1;
            }
            else if (market == "PJM ")
            {
                marketKey = 1;
            }
            else if (market == "ERCOT")
            {
                marketKey = 9;
            }

            else
            {
                marketKey = 7;
            }
            List<PricingNode> sourceNodeList = new List<PricingNode>();
            List<PricingNode> sinkNodeList = new List<PricingNode>();
            SqlDataReader reader;
            if (product == "BOTH" || product == "VIRTUAL" || market == "PJM " || product == "FTR")
            {
                if (marketKey == 3)
                {
                    reader = cmdSelectVirtualNodes.ExecuteReader();
                    while (reader.Read())
                    {
                        sourceNodeList.Add(new PricingNode
                        {
                            NodeKey = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
                            ExternalNodeId = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                            NodeName = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString(),
                            MarketKey = marketKey
                        });
                        if (product == "BOTH")
                        {
                            sinkNodeList.Add(new PricingNode
                            {
                                NodeKey = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
                                ExternalNodeId = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                                NodeName = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString(),
                                MarketKey = marketKey
                            });
                        }
                    }
                }

                else
                {
                    if (product == "FTR" && market == "PJM")
                    {
                        cmdSelectSourceSinkNode.CommandText = "select NodeKey, NodeName, ExternalNodeId, NodeTypeKey, Zone from Node where MarketKey = @market order by NodeName ";

                    }
                    if (market == "ERCOT")
                    {
                        cmdSelectSourceSinkNode.CommandText = "select NodeKey, NodeName, ExternalNodeId, NodeTypeKey, Zone from Vayu..Node where MarketKey = 9 and NodeKey in (select distinct nodekey from Vayu..NodeDALMPH)";
                        //cmdSelectSourceSinkNode.Connection = VayuErcotDBConnection;
                    }
                    if (product == "VIRTUAL" && market == "PJM")
                    {
                        cmdSelectSourceSinkNode.CommandText = "select NodeKey, NodeName, ExternalNodeId, NodeTypeKey, Zone from Node where MarketKey = @market order by NodeName ";
                    }
                    cmdSelectSourceSinkNode.Parameters["@market"].Value = marketKey;
                    reader = cmdSelectSourceSinkNode.ExecuteReader();
                    while (reader.Read())
                    {
                        PricingNode sourceNode = new PricingNode();
                        sourceNode.NodeKey = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[0]).GetValueOrDefault();
                        sourceNode.NodeName = reader.GetString(1);
                        sourceNode.ExternalNodeId = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[2]).GetValueOrDefault();
                        sourceNode.NodeTypeKey = Vayu.CommonAccessLibrary.CommonDataConversions.GetInt(reader[3]).GetValueOrDefault();
                        sourceNode.Zone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        sourceNode.MarketKey = marketKey;
                        sourceNodeList.Add(sourceNode);
                        sinkNodeList.Add(sourceNode);
                    }
                }
            }

            else
            {
                if (market == "ERCOT")
                {
                    cmdSelectSource.CommandText = "select  distinct src.SourceNodeKey, src.SourceName, n.ExternalNodeId as SourceExternalNodeID, n.NodeTypeKey as SourceNodeTypeKey, n.Zone" +
                                                   " as SourceNodeZone from Vayu..EESPathList src inner join Vayu..Node n on n.NodeKey = src.SourceNodeKey where n.MarketKey = @market and src.MarketKey = @market";

                }

                cmdSelectSource.Parameters["@market"].Value = marketKey;
                reader = cmdSelectSource.ExecuteReader();
                while (reader.Read())
                {
                    PricingNode sourceNode = new PricingNode();
                    sourceNode.NodeKey = CommonDataConversions.GetInt(reader[0]).GetValueOrDefault();
                    sourceNode.NodeName = reader.GetString(1);
                    sourceNode.ExternalNodeId = CommonDataConversions.GetInt(reader[2]).GetValueOrDefault();
                    sourceNode.NodeTypeKey = CommonDataConversions.GetInt(reader[3]).GetValueOrDefault();
                    sourceNode.Zone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    sourceNode.MarketKey = marketKey;
                    sourceNodeList.Add(sourceNode);
                }
                reader.Close();
                if (market == "ERCOT")
                {

                    cmdSelectSinkCommand.CommandText = "select  distinct sink.SinkNodeKey, sink.SinkName, n2.ExternalNodeID as SinkExternalNodeID, n2.NodeTypeKey as SinkNodeTypeKey, n2.Zone " +
                                                      "as SinkNodeZone from Vayu..EESPathList sink inner join Vayu..Node n2 on n2.NodeKey = sink.SinkNodeKey where n2.marketkey= @market and sink.MarketKey = @market";

                }
                cmdSelectSinkCommand.Parameters["@market"].Value = marketKey;
                reader = cmdSelectSinkCommand.ExecuteReader();
                while (reader.Read())
                {
                    PricingNode sinkNode = new PricingNode();
                    sinkNode.NodeKey = CommonDataConversions.GetInt(reader[0]).GetValueOrDefault();
                    sinkNode.NodeName = reader.GetString(1);
                    sinkNode.ExternalNodeId = CommonDataConversions.GetInt(reader[2]).GetValueOrDefault();
                    sinkNode.NodeTypeKey = CommonDataConversions.GetInt(reader[3]).GetValueOrDefault();
                    sinkNode.Zone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    sinkNode.MarketKey = marketKey;
                    sinkNodeList.Add(sinkNode);
                }
            }
            reader.Close();
            VayuDBConnection.Close();
            sourceNodeList = sourceNodeList.GroupBy(node => node.NodeKey).Select(g => g.First()).OrderBy(x => x.NodeName).ToList();
            if (product == "VIRTUAL")
            {
                sinkNodeList = null;
            }
            else
            {
                sinkNodeList = sinkNodeList.GroupBy(cust => cust.NodeKey).Select(g => g.First()).OrderBy(x => x.NodeName).ToList();
            }
            callback(new Tuple<List<PricingNode>, List<PricingNode>>(sourceNodeList, sinkNodeList), null);
        }
        /// <summary>
        /// Updates the date virtual bids.
        /// </summary>
        /// <param name="portfoliokey">The portfoliokey.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="bids">The bids.</param>
        /// <param name="status">The status.</param>
        public static void UpdateDateVirtualBids(int portfoliokey, DateTime toDate, List<Bid> bids, string status, int market)
        {

            LoadDB();
            VayuDBConnection.Open();
            foreach (Bid bid in bids)
            {
                long source = 0;
                if (market == 1)
                    source = bid.SourcePnodeId;
                else
                    source = (Int64)bid.Source;
                cmdUpdateSubmitVirtualPortfolio.Parameters["@newmarketdate"].Value = toDate;
                cmdUpdateSubmitVirtualPortfolio.Parameters["@trantime"].Value = DateTime.Now;
                cmdUpdateSubmitVirtualPortfolio.Parameters["@nodekey"].Value = bid.Source;
                DateTime marketDate = bid.MarketDateTime.Hour == 0 ? bid.MarketDateTime.Date.AddDays(-1) : bid.MarketDateTime.Date;
                cmdUpdateSubmitVirtualPortfolio.Parameters["@he"].Value = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                cmdUpdateSubmitVirtualPortfolio.Parameters["@price"].Value = Math.Round(bid.Price, 2);
                cmdUpdateSubmitVirtualPortfolio.Parameters["@mw"].Value = Math.Abs(Math.Round(bid.MW, 1));
                cmdUpdateSubmitVirtualPortfolio.Parameters["@marketdate"].Value = marketDate;
                cmdUpdateSubmitVirtualPortfolio.Parameters["@portfoliokey"].Value = portfoliokey;
                cmdUpdateSubmitVirtualPortfolio.Parameters["@incdec"].Value = bid.MW < 0 ? "I" : "D";
                cmdUpdateSubmitVirtualPortfolio.Parameters["@status"].Value = status;
                int count = cmdUpdateSubmitVirtualPortfolio.ExecuteNonQuery();
            }
            VayuDBConnection.Close();
        }
        /// <summary>
        /// Saves the FTR bids.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <param name="auction">The auction.</param>
        /// <param name="bids">The bids.</param>
        public static void SaveFtrBids(int portfolioKey, string auction, List<FTRBid> bids)
        {
            LoadDB();
            VayuDBConnection.Open();
            cmdDeleteFtrBid.Parameters["@auction"].Value = auction;
            cmdDeleteFtrBid.Parameters["@portfoliokey"].Value = portfolioKey;
            cmdDeleteFtrBid.ExecuteNonQuery();
            foreach (FTRBid bid in bids)
            {
                cmdInsertPJMFtrBidCommand.Parameters["@PeriodKey"].Value = bid.PeriodKey;
                cmdInsertPJMFtrBidCommand.Parameters["@Participant"].Value = bid.mParticipant;
                cmdInsertPJMFtrBidCommand.Parameters["@MarketKey"].Value = bid.mMarketKey;
                cmdInsertPJMFtrBidCommand.Parameters["@hedgetype"].Value = bid.HedgeType;

                cmdInsertPJMFtrBidCommand.Parameters["@month"].Value = bid.month;
                cmdInsertPJMFtrBidCommand.Parameters["@auction"].Value = auction;
                cmdInsertPJMFtrBidCommand.Parameters["@classtype"].Value = bid.ClassType;
                cmdInsertPJMFtrBidCommand.Parameters["@tradetype"].Value = bid.TradeType;
                cmdInsertPJMFtrBidCommand.Parameters["@hedgetype"].Value = bid.HedgeType;

                cmdInsertPJMFtrBidCommand.Parameters["@portfoliokey"].Value = portfolioKey;
                cmdInsertPJMFtrBidCommand.Parameters["@periodhours"].Value = bid.PeriodHours;
                cmdInsertPJMFtrBidCommand.Parameters["@periodname"].Value = bid.PeriodName;
                cmdInsertPJMFtrBidCommand.Parameters["@periodtype"].Value = bid.PeriodName;
                cmdInsertPJMFtrBidCommand.Parameters["@source"].Value = bid.Source;
                cmdInsertPJMFtrBidCommand.Parameters["@sourceextid"].Value = bid.SourceExt;
                cmdInsertPJMFtrBidCommand.Parameters["@sink"].Value = bid.Sink;
                cmdInsertPJMFtrBidCommand.Parameters["@sinkextid"].Value = bid.SinkExt;

                if (bid.TCRID == 0)
                    cmdInsertPJMFtrBidCommand.Parameters["@TcrId"].Value = DBNull.Value;
                else
                    cmdInsertPJMFtrBidCommand.Parameters["@TcrId"].Value = bid.TCRID;

                cmdInsertPJMFtrBidCommand.Parameters["@mw1"].Value = bid.MW1;
                cmdInsertPJMFtrBidCommand.Parameters["@price1"].Value = bid.Price1;
                if (bid.MW2 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw2"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw2"].Value = bid.MW2;
                }
                if (bid.Price2 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price2"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price2"].Value = bid.Price2;
                }
                if (bid.MW3 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw3"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw3"].Value = bid.MW3;
                }
                if (bid.Price3 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price3"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price3"].Value = bid.Price3;
                }
                if (bid.MW4 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw4"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw4"].Value = bid.MW4;
                }
                if (bid.Price4 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price4"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price4"].Value = bid.Price4;
                }
                if (bid.MW5 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw5"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw5"].Value = bid.MW5;
                }
                if (bid.Price5 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price5"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price5"].Value = bid.Price5;
                }
                if (bid.MW6 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw6"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@mw6"].Value = bid.MW6;
                }
                if (bid.Price6 == null)
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price6"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertPJMFtrBidCommand.Parameters["@price6"].Value = bid.Price6;
                }

                try
                {
                    cmdInsertPJMFtrBidCommand.ExecuteNonQuery();
                }
                catch (Exception ex) { }
            }
            VayuDBConnection.Close();
        }

        public static List<string> GetLatestValidFTRNodes()
        {
            List<string> nodeList = new List<string>();
            try
            {
                using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();
                        cmd.CommandText = "select distinct NodeName from pjm.FTRAuctionNodePrice a join node n1 on a.NodeKey = n1.NodeKey  where PeriodKey  in (select MAX(PeriodKey) from pjm.FTRAuctionNodePrice) ";
                        cmd.Connection = con;
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string nodeName = rdr.GetValue(0).ToString();
                            if (!nodeList.Contains(nodeName))
                                nodeList.Add(nodeName);
                        }
                        rdr.Close();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return nodeList;
        }
        /// <summary>
        /// Deletes the path.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="bidId">The bid identifier.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="isUptos">if set to <c>true</c> [is uptos].</param>
        public static void DeletePath(int market, string bidId, DateTime toDate, bool isUptos)
        {
            LoadDB();
            VayuDBConnection.Open();
            if (isUptos)
            {
                if (market == 1)
                {
                    cmdDeleteEESBidsById.Parameters["@scheduleid"].Value = bidId;
                    cmdDeleteEESBidsById.Parameters["@start"].Value = toDate;
                    cmdDeleteEESBidsById.Parameters["@end"].Value = toDate.AddDays(1);
                    cmdDeleteEESBidsById.ExecuteNonQuery();
                }
                else
                {
                    cmdDeleteErcotEESBidsById.Parameters["@bidid"].Value = bidId;
                    cmdDeleteErcotEESBidsById.Parameters["@start"].Value = toDate;
                    cmdDeleteErcotEESBidsById.Parameters["@end"].Value = toDate.AddDays(1);
                    cmdDeleteErcotEESBidsById.ExecuteNonQuery();
                }
            }
            else
            {
                try
                {
                    cmdDeleteVirtualBidsForUpdate.Parameters[0].Value = bidId;
                    cmdDeleteVirtualBidsForUpdate.ExecuteNonQuery();
                }
                catch
                {
                }
            }
            VayuDBConnection.Close();
        }
        /// <summary>
        /// Deletes the path asynchronous.
        /// </summary>
        /// <param name="portfolioDate">The portfolio date.</param>
        /// <param name="source">The source.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="price">The price.</param>
        /// <param name="mw">The mw.</param>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <param name="isUptos">if set to <c>true</c> [is uptos].</param>
        /// <param name="market">The market.</param>
        public static void DeletePathAsync(DateTime portfolioDate, string source, string sink, double price, double mw, int portfolioKey, bool isUptos, int market)
        {
            LoadDB();
            using (SqlConnection con = new SqlConnection())
            {
                if (isUptos)
                {
                    if (market == 1)
                    {

                        con.ConnectionString = cmdDeleteEESBids.Connection.ConnectionString;
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = cmdDeleteEESBids.CommandText;
                            cmd.Parameters.AddWithValue("@startdate", portfolioDate);
                            cmd.Parameters.AddWithValue("@endate", portfolioDate.AddDays(1));
                            cmd.Parameters.AddWithValue("@source", source);
                            cmd.Parameters.AddWithValue("@sink", sink);
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.Parameters.AddWithValue("@mw", mw);
                            cmd.Parameters.AddWithValue("@PortfolioKey", portfolioKey);
                            cmd.Connection.Open();
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();

                        }


                    }
                    else
                    {
                        con.ConnectionString = cmdDeleteErcotEESBids.Connection.ConnectionString;
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = cmdDeleteErcotEESBids.CommandText;
                            cmd.Parameters.AddWithValue("@startdate", portfolioDate);
                            cmd.Parameters.AddWithValue("@endate", portfolioDate.AddDays(1));
                            cmd.Parameters.AddWithValue("@source", source);
                            cmd.Parameters.AddWithValue("@sink", sink);
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.Parameters.AddWithValue("@mw", mw);
                            cmd.Parameters.AddWithValue("@PortfolioKey", portfolioKey);
                            cmd.Connection.Open();
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                    }
                }
                else
                {
                    try
                    {
                        //con.ConnectionString = cmdDeleteVirtualBidsForUpdate.Connection.ConnectionString;
                        //using (SqlCommand cmd = con.CreateCommand())
                        //{
                        //    cmd.CommandText = cmdDeleteVirtualBidsForUpdate.CommandText;
                        //    cmd.Parameters.AddWithValue("@bidid", bidId);
                        //    cmd.Connection.Open();
                        //    cmd.ExecuteNonQuery();
                        //    cmd.Connection.Close();
                        //}
                    }
                    catch
                    {
                    }
                }
            }
        }
        /// <summary>
        /// Deletes the portfolio.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="toDate">To date.</param>
        public static void DeletePortfolio(string user, Portfolio portfolio, DateTime toDate)
        {
            LoadDB();
            VayuDBConnection.Open();
            if (!portfolio.IsUptos)
            {
                cmdDeleteVirtualBids.Parameters["@PortfolioKey"].Value = portfolio.ID;
                cmdDeleteVirtualBids.Parameters["@MarketDate"].Value = toDate;
                //cmdDeleteVirtualBids.Parameters["@Trader"].Value = user;
                cmdDeleteVirtualBids.ExecuteNonQuery();
            }
            else
            {
                if (portfolio.Market == "PJM")
                {
                    //cmdDeleteEESBidsCommand.Parameters["@portfoliokey"].Value = portfolio.ID;
                    //cmdDeleteEESBidsCommand.Parameters["@start"].Value = toDate;
                    //cmdDeleteEESBidsCommand.Parameters["@end"].Value = toDate.AddDays(1);
                    //cmdDeleteEESBidsCommand.ExecuteNonQuery();
                }
                else
                {
                    cmdDeleteErcotEESBidsCommand.Parameters["@portfoliokey"].Value = portfolio.ID;
                    cmdDeleteErcotEESBidsCommand.Parameters["@start"].Value = toDate;
                    cmdDeleteErcotEESBidsCommand.Parameters["@end"].Value = toDate.AddDays(1);
                    cmdDeleteErcotEESBidsCommand.ExecuteNonQuery();
                }
            }
            VayuDBConnection.Close();
        }
        /// <summary>
        /// Saves the bids.
        /// </summary>
        /// <param name="bids">The bids.</param>
        /// <param name="mUser">The m user.</param>
        public static void SaveBids(List<Bid> bids, string mUser)
        {
            if (bids == null || bids.Count == 0)
            {
                return;
            }
            LoadDB();
            VayuDBConnection.Open();
            int bidId = 1;
            if (!bids[0].IsUptos)
            {
                SqlDataReader reader = cmdSelectMaxBidIdCommand.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        bidId = CommonDataConversions.GetInt(reader[0]).GetValueOrDefault() + 1;
                    }
                }
                reader.Close();
            }
            Dictionary<string, int> bidHash = new Dictionary<string, int>();
            foreach (Bid bid in bids)
            {
                if (bid.IsUptos)
                {

                    if (bid.Market == 1)
                    {
                        PricingNode sourceNode = GetNode(bid.Source,1);
                        PricingNode sinkNode = GetNode(bid.Sink,1);
                        cmdInsertEESBids.Parameters["@BidStatus"].Value = "IMPORTED";
                        cmdInsertEESBids.Parameters["@scheduleid"].Value = bid.BidId;
                        cmdInsertEESBids.Parameters["@EndMarketDateTime"].Value = bid.MarketDateTime;
                        cmdInsertEESBids.Parameters["@RequestedMW"].Value = Math.Round(bid.MW, 1);
                        cmdInsertEESBids.Parameters["@ClearedMW"].Value = 0;
                        cmdInsertEESBids.Parameters["@EndUserKey"].Value = "28";
                        cmdInsertEESBids.Parameters["@SourceNodeKey"].Value = bid.Source;
                        cmdInsertEESBids.Parameters["@SinkNodeKey"].Value = bid.Sink;
                        cmdInsertEESBids.Parameters["@Source"].Value = sourceNode.NodeName;
                        cmdInsertEESBids.Parameters["@Sink"].Value = sinkNode.NodeName;
                        cmdInsertEESBids.Parameters["@Price"].Value = Math.Round(bid.Price, 2);
                        cmdInsertEESBids.Parameters["@PortfolioKey"].Value = bid.PortfolioKey;
                        cmdInsertEESBids.Parameters["@SubmittedDateTime"].Value = DateTime.Now;
                        cmdInsertEESBids.Parameters["@comments"].Value = bid.Comments == null ? string.Empty : bid.Comments;
                        cmdInsertEESBids.ExecuteNonQuery();
                    }
                    else
                    {
                        PricingNode sourceNode = GetNode(bid.Source, 9);
                        PricingNode sinkNode = GetNode(bid.Sink, 9);
                        //cmdInsertErcotEESBids.Parameters["@bidid"].Value = bid.BidId;
                        //cmdInsertErcotEESBids.Parameters["@Source"].Value = sourceNode.NodeName;
                        //cmdInsertErcotEESBids.Parameters["@Sink"].Value = sinkNode.NodeName;
                        //cmdInsertErcotEESBids.Parameters["@Price"].Value = Math.Round(bid.Price, 2);
                        //cmdInsertErcotEESBids.Parameters["@EndMarketDateTime"].Value = bid.MarketDateTime;
                        //cmdInsertErcotEESBids.Parameters["@RequestedMW"].Value = Math.Round(bid.MW, 1);
                        //cmdInsertErcotEESBids.Parameters["@BidStatus"].Value = "IMPORTED";
                        //cmdInsertErcotEESBids.Parameters["@EndUserKey"].Value = 5;
                        //cmdInsertErcotEESBids.Parameters["@PortfolioKey"].Value = bid.PortfolioKey;
                        //cmdInsertErcotEESBids.Parameters["@SubmittedDateTime"].Value = DateTime.Now;
                        //cmdInsertErcotEESBids.Parameters["@comments"].Value = bid.Comments == null ? string.Empty : bid.Comments;
                        cmdInsertErcotEESBids.CommandTimeout = 300000;
                        cmdInsertErcotEESBids.Parameters["@BidStatus"].Value = "IMPORTED";
                        cmdInsertErcotEESBids.Parameters["@scheduleid"].Value = bid.BidId;
                        cmdInsertErcotEESBids.Parameters["@EndMarketDateTime"].Value = bid.MarketDateTime;
                        cmdInsertErcotEESBids.Parameters["@RequestedMW"].Value = Math.Round(bid.MW, 1);
                        cmdInsertErcotEESBids.Parameters["@ClearedMW"].Value = 0;
                        cmdInsertErcotEESBids.Parameters["@EndUserKey"].Value = "28";
                        cmdInsertErcotEESBids.Parameters["@SourceNodeKey"].Value = bid.Source;
                        cmdInsertErcotEESBids.Parameters["@SinkNodeKey"].Value = bid.Sink;
                        cmdInsertErcotEESBids.Parameters["@Source"].Value = sourceNode.NodeName;
                        cmdInsertErcotEESBids.Parameters["@Sink"].Value = sinkNode.NodeName;
                        cmdInsertErcotEESBids.Parameters["@Price"].Value = Math.Round(bid.Price, 2);
                        cmdInsertErcotEESBids.Parameters["@PortfolioKey"].Value = bid.PortfolioKey;
                        cmdInsertErcotEESBids.Parameters["@SubmittedDateTime"].Value = DateTime.Now;
                        cmdInsertErcotEESBids.Parameters["@comments"].Value = bid.Comments == null ? string.Empty : bid.Comments;

                        cmdInsertErcotEESBids.ExecuteNonQuery();
                    }
                }
                else
                {
                    string key = bid.Source.ToString() + bid.MW + bid.Price;
                    int tempBid = bidId;
                    if (bidHash.ContainsKey(key))
                    {
                        tempBid = bidHash[key];
                    }
                    else
                    {
                        bidHash.Add(key, bidId);
                        bidId++;
                    }
                    cmdInsertVirtualBids.Parameters["@PortfolioKey"].Value = bid.PortfolioKey;
                    cmdInsertVirtualBids.Parameters["@Market"].Value = bid.Market;
                    DateTime marketDate = bid.MarketDateTime.Hour == 0 ? bid.MarketDateTime.Date.AddDays(-1) : bid.MarketDateTime.Date;
                    cmdInsertVirtualBids.Parameters["@MarketDate"].Value = marketDate;
                    cmdInsertVirtualBids.Parameters["@TranTime"].Value = DateTime.Now;
                    cmdInsertVirtualBids.Parameters["@Trader"].Value = mUser;
                    cmdInsertVirtualBids.Parameters["@FileName"].Value = "";
                    cmdInsertVirtualBids.Parameters["@status"].Value = bid.Status;
                    cmdInsertVirtualBids.Parameters["@IncDec"].Value = bid.MW > 0 ? "D" : "I";
                    cmdInsertVirtualBids.Parameters["@NodeKey"].Value = bid.Source;
                    cmdInsertVirtualBids.Parameters["@Segment"].Value = bid.Segment;
                    int hour = bid.MarketDateTime.Hour == 0 ? 24 : bid.MarketDateTime.Hour;
                    cmdInsertVirtualBids.Parameters["@HE"].Value = hour;
                    cmdInsertVirtualBids.Parameters["@Price"].Value = Math.Round(bid.Price, 2);
                    cmdInsertVirtualBids.Parameters["@MW"].Value = Math.Abs(Math.Round(bid.MW, 1));
                    cmdInsertVirtualBids.Parameters["@bidid"].Value = tempBid;
                    cmdInsertVirtualBids.Parameters["@comments"].Value = bid.Comments == null ? string.Empty : bid.Comments;
                    cmdInsertVirtualBids.ExecuteNonQuery();
                }
            }
            VayuDBConnection.Close();
        }
        /// <summary>
        /// Gets the invalid weather hours list.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="city">The city.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public static List<DateTime> GetInvalidWeatherHoursList(string type, string city, DateTime start, DateTime end, double min, double max)
        {
            LoadDB();
            if (type == "Temp")
            {
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(temperature < @min or temperature > @max)";
            }
            if (type == "Cloud Cover")
            {
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(CloudCover < @min or CloudCover > @max)";
            }
            if (type == "Dew Point")
            {
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(DewPoint < @min or DewPoint > @max)";
            }
            if (type == "Precip")
            {
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(Precip < @min or Precip > @max)";
            }
            if (type == "Wind Speed")
            {
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(WindSpeed < @min or WindSpeed > @max)";
            }
            if (type == "Rel. Hum.")
            {
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(RelativeHumidity < @min or RelativeHumidity > @max)";
            }
            if (type == "Wind Dir.")
            {
                cmdSelectWeather.CommandText = "select marketdate, markettime from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end and " +
                                                    "(WindDirection < @min or WindDirection > @max)";
            }
            List<DateTime> dateTimeList = new List<DateTime>();
            VayuDBConnection.Open();
            cmdSelectWeather.Parameters["@city"].Value = city;
            cmdSelectWeather.Parameters["@start"].Value = start;
            cmdSelectWeather.Parameters["@end"].Value = end;
            cmdSelectWeather.Parameters["@min"].Value = min;
            cmdSelectWeather.Parameters["@max"].Value = max;
            SqlDataReader reader = cmdSelectWeather.ExecuteReader();
            while (reader.Read())
            {
                DateTime marketDate = reader.GetDateTime(0);
                int hour = reader.GetInt32(1);
                dateTimeList.Add(marketDate.AddHours(hour));
            }
            reader.Close();
            VayuDBConnection.Close();
            return dateTimeList;
        }
        /// <summary>
        /// Gets the weather date mw.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="city">The city.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static Dictionary<DateTime, List<double>> GetWeatherDateMW(string type, string city, DateTime start, DateTime end)
        {
            LoadDB();
            if (type == "Temp")
            {
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime, temperature from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end";
            }
            if (type == "Cloud Cover")
            {
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime, cloudcover from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end";
            }
            if (type == "Dew Point")
            {
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime, dewpoint from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end";
            }
            if (type == "Precip")
            {
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime, precip from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end";
            }
            if (type == "Wind Speed")
            {
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime, windspeed from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end";
            }
            if (type == "Rel. Hum.")
            {
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime, relativehumidity from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end";
            }
            if (type == "Wind Dir.")
            {
                cmdSelectWeatherForecast.CommandText = "select marketdate, markettime, WindDirection from Wsi_Weather where city = @city and marketdate >= @start and marketdate <= @end";
            }
            VayuDBConnection.Open();
            cmdSelectWeatherForecast.Parameters["@city"].Value = city;
            cmdSelectWeatherForecast.Parameters["@start"].Value = start;
            cmdSelectWeatherForecast.Parameters["@end"].Value = end;
            SqlDataReader reader = cmdSelectWeatherForecast.ExecuteReader();
            Dictionary<DateTime, List<double>> dateMWHash = new Dictionary<DateTime, List<double>>();
            while (reader.Read())
            {
                DateTime marketDate = reader.GetDateTime(0);
                int hour = reader.GetInt32(1);
                string typeName = reader.GetDataTypeName(2);
                double mw = 0;
                if (typeName == "int")
                {
                    mw = reader.GetInt32(2);
                }
                else
                {
                    mw = reader.GetDouble(2);
                }
                DateTime date = marketDate.AddHours(hour);
                List<double> mwList = new List<double>();
                if (dateMWHash.ContainsKey(date))
                {
                    mwList = dateMWHash[date];
                    dateMWHash.Remove(date);
                }
                mwList.Add(mw);
                dateMWHash.Add(date, mwList);
            }
            reader.Close();
            VayuDBConnection.Close();
            return dateMWHash;
        }
        /// <summary>
        /// Gets the load date mw.
        /// </summary>
        /// <param name="loadKey">The load key.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static Dictionary<DateTime, List<double>> GetLoadDateMW(int loadKey, DateTime start, DateTime end)
        {
            LoadDB();
            Dictionary<DateTime, List<double>> dateMWHash = new Dictionary<DateTime, List<double>>();
            VayuDBConnection.Open();
            cmdSelectLoadForecast.Parameters["@loadskey"].Value = loadKey;
            cmdSelectLoadForecast.Parameters["@start"].Value = start;
            cmdSelectLoadForecast.Parameters["@end"].Value = end;
            SqlDataReader reader = cmdSelectLoadForecast.ExecuteReader();
            while (reader.Read())
            {
                DateTime marketDateTime = reader.GetDateTime(0);
                double mw = CommonDataConversions.GetDouble(reader[1]).GetValueOrDefault();
                DateTime date = marketDateTime.Hour == 0 ? marketDateTime.AddDays(-1).Date : marketDateTime.Date;
                List<double> mwList = new List<double>();
                if (dateMWHash.ContainsKey(date))
                {
                    mwList = dateMWHash[date];
                    dateMWHash.Remove(date);
                }
                mwList.Add(mw);
                dateMWHash.Add(date, mwList);
            }
            reader.Close();
            VayuDBConnection.Close();
            return dateMWHash;
        }
        /// <summary>
        /// Gets the invalid load hours list.
        /// </summary>
        /// <param name="loadKey">The load key.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public static List<DateTime> GetInvalidLoadHoursList(int loadKey, DateTime start, DateTime end, double min, double max)
        {
            LoadDB();
            List<DateTime> dateTimeList = new List<DateTime>();
            VayuDBConnection.Open();
            cmdSelectLoadRth.Parameters["@loadskey"].Value = loadKey;
            cmdSelectLoadRth.Parameters["@start"].Value = start;
            cmdSelectLoadRth.Parameters["@end"].Value = end;
            cmdSelectLoadRth.Parameters["@min"].Value = min;
            cmdSelectLoadRth.Parameters["@max"].Value = max;
            SqlDataReader reader = cmdSelectLoadRth.ExecuteReader();
            while (reader.Read())
            {
                DateTime marketDateTime = reader.GetDateTime(0);
                dateTimeList.Add(marketDateTime);
            }
            reader.Close();
            VayuDBConnection.Close();
            return dateTimeList;
        }
        /// <summary>
        /// Gets the loads.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public static List<Load> GetLoads(string market)
        {
            LoadDB();
            int marketKey = 1;
            if (market == "ERCOT")
            {
                marketKey = 9;
            }

            List<Load> loadList = new List<Load>();
            VayuDBConnection.Open();
            cmdSelectLoad.Parameters["@marketkey"].Value = marketKey;
            SqlDataReader reader = cmdSelectLoad.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                int key = CommonDataConversions.GetInt(reader[1]).GetValueOrDefault();
                Load load = new Load();
                load.Name = name;
                load.Key = key;
                loadList.Add(load);
            }
            reader.Close();
            VayuDBConnection.Close();
            return loadList;
        }
        /// <summary>
        /// Gets the date names.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDateNames()
        {
            List<string> dateNameList = new List<string>();
            LoadDB();
            VayuDBConnection.Open();
            cmdSelectNameDateRange.Parameters["@trader"].Value = sUser;
            SqlDataReader reader = cmdSelectNameDateRange.ExecuteReader();
            while (reader.Read())
            {
                dateNameList.Add(reader.GetString(0));
            }
            VayuDBConnection.Close();
            return dateNameList;
        }
        /// <summary>
        /// Gets the constraints.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public static List<VectorKeyInfo> GetConstraints(string market = "PJM")
        {
            List<VectorKeyInfo> constraintList = new List<VectorKeyInfo>();
            LoadDB();
            VayuDBConnection.Open();
            SqlDataReader reader = null;

            reader = cmdSelectPJMConstraintSensitivity.ExecuteReader();

            while (reader.Read())
            {
                VectorKeyInfo info = new VectorKeyInfo();
                info.DBText = reader.GetString(0);
                info.DisplayText = (info.DBText ?? "").Replace("Monitor", "").Replace("COMPANY", "").Replace("Actual", "").TrimStart();
                constraintList.Add(info);
            }
            VayuDBConnection.Close();
            return constraintList;
        }
        /// <summary>
        /// Gets the families.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFamilies()
        {
            List<string> familyList = new List<string>();
            LoadDB();
            VayuDBConnection.Open();
            SqlDataReader reader = cmdSelectPjmRtFamilies.ExecuteReader();
            while (reader.Read())
            {
                familyList.Add(reader.GetString(0));
            }
            VayuDBConnection.Close();
            return familyList;
        }

        public static List<string> GetERCOTFamilies(string RTORDA)
        {
            List<string> familyList = new List<string>();
            SqlDataReader reader = null;
            LoadDB();
            VayuDBConnection.Open();

            if (RTORDA == "DA")
                reader = cmdSelectErcotDAFamilies.ExecuteReader();
            if (RTORDA == "RT")
                reader = cmdSelectErcotRTFamilies.ExecuteReader();

            while (reader.Read())
            {
                familyList.Add(reader.GetString(0));
            }
            VayuDBConnection.Close();
            return familyList;
        }

        public static List<string> GetAllPath()
        {
            List<string> AllPathList = new List<string>();
            LoadDB();
            VayuDBConnection.Open();
            SqlDataReader reader = cmdSelectAllPath.ExecuteReader();
            while (reader.Read())
            {
                AllPathList.Add(reader.GetString(0));
            }
            VayuDBConnection.Close();
            return AllPathList;
        }
        /// <summary>
        /// Gets the constraint numbers.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetConstraintNumbers()
        {
            List<string> conNumList = new List<string>();
            LoadDB();
            VayuDBConnection.Open();
            SqlDataReader reader = cmdSelectPjmRtConstraintNumbers.ExecuteReader();
            while (reader.Read())
            {
                conNumList.Add(Convert.ToString(reader.GetValue(0)));
            }
            VayuDBConnection.Close();
            return conNumList;
        }

        /// <summary>
        /// Gets the current bids.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="portfolioIDs">The portfolio i ds.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="isUptos">if set to <c>true</c> [is uptos].</param>
        /// <returns></returns>
        public static List<int> GetCurrentBids(string market, IEnumerable<int> portfolioIDs, DateTime startDate, bool isUptos)
        {
            if (string.IsNullOrEmpty(market) || portfolioIDs == null || portfolioIDs.Count() == 0)
                return null;

            string portList = string.Empty;
            foreach (var item in portfolioIDs)
                portList += item + ",";

            portList = portList.Trim(' ', ',');
            HashSet<int> bidList = new HashSet<int>();
            LoadDB();

            if (!isUptos)
            {
                cmdSelectPortfolioCurrentBids.CommandText = "select distinct NodeKey  from clearedbids where MarketDateTime > @startDate and MarketDateTime <=@endDate and " +
                    "portfoliokey in (" + portList + ")";
                //cmdSelectPortfolioCurrentBids.Parameters["@portfoliokey"].Value = portList;
                cmdSelectPortfolioCurrentBids.Parameters["@startdate"].Value = startDate;
                cmdSelectPortfolioCurrentBids.Parameters["@enddate"].Value = startDate.AddDays(1);

                if (cmdSelectPortfolioCurrentBids.Connection.State != ConnectionState.Open)
                    cmdSelectPortfolioCurrentBids.Connection.Open();

                SqlDataReader reader = cmdSelectPortfolioCurrentBids.ExecuteReader();
                while (reader.Read())
                    bidList.Add(CommonDataConversions.GetInt(reader[0]).GetValueOrDefault());

                reader.Close();
                if (cmdSelectPortfolioCurrentBids.Connection.State != ConnectionState.Closed)
                    cmdSelectPortfolioCurrentBids.Connection.Close();
            }
            else
            {
                cmdSelectPortfolioSourceUptos.CommandText = "select distinct SourceNodeKey from clearedees where MarketDateTime > @startDate and MarketDateTime <=@endDate and " +
                    "portfoliokey in ( " + portList + " )";
                cmdSelectPortfolioSourceUptos.Parameters["@portfoliokey"].Value = portList;
                cmdSelectPortfolioSourceUptos.Parameters["@startdate"].Value = startDate;
                cmdSelectPortfolioSourceUptos.Parameters["@enddate"].Value = startDate.AddDays(1);
                if (cmdSelectPortfolioSourceUptos.Connection.State != ConnectionState.Open)
                    cmdSelectPortfolioSourceUptos.Connection.Open();

                SqlDataReader reader = cmdSelectPortfolioSourceUptos.ExecuteReader();
                while (reader.Read())
                    bidList.Add(CommonDataConversions.GetInt(reader[0]).GetValueOrDefault());

                reader.Close();
                if (cmdSelectPortfolioSourceUptos.Connection.State != ConnectionState.Closed)
                    cmdSelectPortfolioSourceUptos.Connection.Close();


                cmdSelectPortfolioSinkUptos.CommandText = "select distinct SinkNodeKey from clearedees where MarketDateTime > @startDate and MarketDateTime <=@endDate and " +
                    "portfoliokey in ( " + portList + " )";
                cmdSelectPortfolioSinkUptos.Parameters["@portfoliokey"].Value = portList;
                cmdSelectPortfolioSinkUptos.Parameters["@startdate"].Value = startDate;
                cmdSelectPortfolioSinkUptos.Parameters["@enddate"].Value = startDate.AddDays(1);

                if (cmdSelectPortfolioSinkUptos.Connection.State != ConnectionState.Open)
                    cmdSelectPortfolioSinkUptos.Connection.Open();

                SqlDataReader reader1 = cmdSelectPortfolioSinkUptos.ExecuteReader();
                while (reader1.Read())
                    bidList.Add(CommonDataConversions.GetInt(reader1[0]).GetValueOrDefault());

                reader1.Close();
                if (cmdSelectPortfolioSinkUptos.Connection.State != ConnectionState.Closed)
                    cmdSelectPortfolioSinkUptos.Connection.Close();
            }

            if (market == "PJM" && bidList.Count != 0)
            {
                List<int> latestbidList = new List<int>();
                string ids = string.Empty;
                foreach (var item in bidList.Distinct())
                    ids += item + ",";

                ids = ids.Trim(',');
                SqlCommand sCmd = new SqlCommand("select NodeKey from node  where ExternalNodeID in(select distinct ExternalNodeID from node  where nodekey  in (" + ids + "))", VayuDBConnection);

                if (sCmd.Connection.State != ConnectionState.Open)
                    sCmd.Connection.Open();

                SqlDataReader reader12 = sCmd.ExecuteReader();
                while (reader12.Read())
                    latestbidList.Add(CommonDataConversions.GetInt(reader12[0]).GetValueOrDefault());

                reader12.Close();
                if (sCmd.Connection.State != ConnectionState.Closed)
                    sCmd.Connection.Close();

                bidList = new HashSet<int>(latestbidList);
            }

            return bidList.ToList();
        }

        /// <summary>
        /// Gets the bids.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="savedName">Name of the saved.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="isUptos">if set to <c>true</c> [is uptos].</param>
        /// <param name="inStatus">The in status.</param>
        /// <returns></returns>
        public static List<Bid> GetBids(string market, int portfolio, string savedName, DateTime start, DateTime end, bool isUptos, string inStatus)
        {
            sBidIdList = new List<string>();
            List<Bid> bidList = new List<Bid>();
            if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
            {
                VayuDBConnection.Open();
            }
            if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
            {
                VayuDBConnection.Open();
            }
            SqlDataReader reader = null;
            if (isUptos)
            {
                if (market == "PJM")
                {
                    cmdSelectPjmUptosBid.Parameters["@start"].Value = start;
                    cmdSelectPjmUptosBid.Parameters["@end"].Value = end;
                    cmdSelectPjmUptosBid.Parameters["@portfoliokey"].Value = portfolio;
                    reader = cmdSelectPjmUptosBid.ExecuteReader();
                }
                else
                {
                    cmdSelectErcotEESBids.Parameters["@start"].Value = start;
                    cmdSelectErcotEESBids.Parameters["@end"].Value = end;
                    cmdSelectErcotEESBids.Parameters["@portfoliokey"].Value = portfolio;
                    reader = cmdSelectErcotEESBids.ExecuteReader();
                }
            }
            if (!isUptos && start <= DateTime.Today)
            {
                if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
                {
                    VayuDBConnection.Open();
                }
                //if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
                //{
                //    VayuDBConnection.Open();
                //}
                cmdSelectVirtualBidsHolder.Parameters["@marketdate"].Value = start;
                cmdSelectVirtualBidsHolder.Parameters["@portfoliokey"].Value = portfolio;
                reader = cmdSelectVirtualBidsHolder.ExecuteReader();
                ReadBidList(reader, isUptos, start, savedName, portfolio, market, bidList, false);
            }
            if (!isUptos)
            {
                if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
                {
                    VayuDBConnection.Open();
                }
                //if (VayuDBConnection.State.Equals(System.Data.ConnectionState.Closed))
                //{
                //    VayuDBConnection.Open();
                //}
                cmdSelectVirtualBids.Parameters["@marketdate"].Value = start;
                cmdSelectVirtualBids.Parameters["@portfoliokey"].Value = portfolio;
                cmdSelectVirtualBids.Parameters["@status"].Value = inStatus;
                reader = cmdSelectVirtualBids.ExecuteReader();
            }
            ReadBidList(reader, isUptos, start, savedName, portfolio, market, bidList, true);
            VayuDBConnection.Close();
            VayuDBConnection.Close();
            return bidList;
        }
        /// <summary>
        /// Reads the bid list.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="isUptos">if set to <c>true</c> [is uptos].</param>
        /// <param name="start">The start.</param>
        /// <param name="savedName">Name of the saved.</param>
        /// <param name="portfolio">The portfolio.</param>
        /// <param name="market">The market.</param>
        /// <param name="bidList">The bid list.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        private static void ReadBidList(SqlDataReader reader, bool isUptos, DateTime start, string savedName, int portfolio, string market, List<Bid> bidList, bool check)
        {
            int marketId = 1;

            if (market == "ERCOT")
            {
                marketId = 9;
            }

            while (reader.Read())
            {
                int source = 0;
                int sink = 0;
                DateTime marketDateTime = start;
                double mw = savedName == null ? (double)reader.GetDecimal(2) : reader.GetDouble(1);

                mw = Math.Round(mw, 1);

                double price = savedName == null ? (double)reader.GetDecimal(3) : reader.GetDouble(2);

                price = Math.Round(price, 2);

                if (isUptos)
                {
                    if (market == "PJM")
                    {
                        source = (int)reader.GetDecimal(0);
                        sink = (int)reader.GetDecimal(1);
                    }
                    else
                    {
                        //  PricingNode sourceNode = GetNodeFromName(reader.GetString(0), 9);
                        // PricingNode sinkNode = GetNodeFromName(reader.GetString(1), 9);
                        source = (int)reader.GetDecimal(0);
                        sink = (int)reader.GetDecimal(1);
                    }
                    marketDateTime = reader.GetDateTime(4);
                }
                else
                {
                    source = reader.GetInt32(0);
                    marketDateTime = start.AddHours(reader.GetInt32(3));
                    string incDec = reader.GetString(4);
                    if (incDec == "I")
                    {
                        mw *= -1;
                    }
                    string key = source + incDec;
                    if (!check && !sBidIdList.Contains(key))
                    {
                        sBidIdList.Add(key);
                    }
                    if (check && sBidIdList.Contains(key))
                    {
                        continue;
                    }
                }
                string status = reader.GetString(5);
                string bidId = isUptos ? reader.GetString(6) : reader.GetValue(6).ToString();
                int segment = isUptos ? 1 : reader.GetInt32(7);
                Bid bid = new Bid();
                PricingNode sourcePricingNode = GetNode(source, marketId);
                PricingNode sinkPricingNode = GetNode(source, marketId);
                bid.Source = source;
                bid.Sink = sink;
                bid.MW = mw;
                bid.Price = price;
                bid.MarketDateTime = marketDateTime;
                bid.BidId = bidId;
                bid.Status = status;
                bid.PortfolioKey = portfolio;
                bid.Market = marketId;
                bid.File = savedName;
                bid.IsUptos = isUptos;
                bid.Segment = segment;
                if (isUptos)
                {
                    bid.Comments = reader.GetValue(7).ToString();
                }
                else
                {
                    bid.Comments = reader.IsDBNull(8) ? "" : reader.GetValue(8).ToString();
                }
                bidList.Add(bid);
                if (savedName != null)
                {
                    sBidId++;
                }
            }
            reader.Close();
        }
        /// <summary>
        /// Gets the portfolios from account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns></returns>
        public static List<Portfolio> GetPortfoliosFromAccount(string account)
        {
            LoadDB();
            List<Portfolio> portfolioList = new List<Portfolio>();
            VayuDBConnection.Open();
            cmdSelectPortfolioFromAccount.Parameters["@account"].Value = account;
            SqlDataReader reader = cmdSelectPortfolioFromAccount.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                int id = reader.GetInt32(1);
                string market = reader.GetString(2);
                string product = reader.GetString(3);
                Portfolio portfolio = new Portfolio();
                portfolio.ID = id;
                portfolio.Name = name;
                portfolio.Market = market;
                portfolio.IsUptos = product == "EES/PTP";
                portfolioList.Add(portfolio);
            }
            reader.Close();
            VayuDBConnection.Close();
            return portfolioList;
        }
        /// <summary>
        /// Gets the trading portfolio by key.
        /// </summary>
        /// <param name="portfolioKey">The portfolio key.</param>
        /// <returns></returns>
        public static Portfolio GetTradingPortfolioByKey(int portfolioKey)
        {
            SqlCommand portfolioByID = new SqlCommand();
            portfolioByID.CommandText = "select * from PORTFOLIO where portfoliokey = @portfoliokey";
            portfolioByID.Parameters.AddWithValue("@portfoliokey", portfolioKey);
            portfolioByID.Connection = new VayuDBConnection().GetInstance().GetSqlConnection();
            Portfolio portfolio = null;

            try
            {
                portfolioByID.Connection.Open();
                SqlDataReader reader = portfolioByID.ExecuteReader();

                if (reader.Read())
                {
                    portfolio = new Portfolio();
                    portfolio.ID = (int)reader.GetDecimal(0);
                    portfolio.Name = reader.GetString(1);
                    portfolio.MarketKey = (int)reader.GetDecimal(3);
                    portfolio.TradeType = reader.GetString(4);
                    reader.Close();
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                portfolioByID.Connection.Close();
            }

            return portfolio;
        }
        /// <summary>
        /// Gets the user portfolio list.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="user">The user.</param>
        /// <param name="product">The product.</param>
        /// <param name="market">The market.</param>
        public static void GetUserPortfolioList(Action<List<Portfolio>, Exception> callback, string user, string product, string market)
        {
            if (market == null)
            {
                return;
            }
            string usertype = "";
            LoadDB();
            VayuDBConnection.Open();
            List<Portfolio> portfolioList = new List<Portfolio>();
            SqlDataReader reader = null;
            SqlCommand cmdusertype = VayuDBConnection.CreateCommand();
            cmdusertype.CommandText = "select user_type from END_USER where ad_login=" + "'" + user + "' ";
            cmdusertype.Connection = VayuDBConnection;
            SqlDataReader cmdusertypedr = cmdusertype.ExecuteReader();
            while (cmdusertypedr.Read())
            {
                usertype = cmdusertypedr.GetValue(0).ToString();
            }
            if (product == "EES/PTP")
            {
                if (market == "ERCOT")
                {
                    if (usertype == "user")
                    {
                        cmdSelectPortfolio.Parameters["@HUB"].Value = market;
                        cmdSelectPortfolio.Parameters["@product"].Value = product;
                        cmdSelectPortfolio.Parameters["@ad_login"].Value = user;
                    }
                    else
                        cmdSelectPortfolio.CommandText = "select STRIP, Portfolio_ID from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = 'ERCOT' and A.flag_deprecated=0and product = 'EES/PTP' and ACTIVE = 'Y' order by STRIP";

                }
                else
                {
                    SqlCommand cmd = VayuDBConnection.CreateCommand();
                    if (usertype == "user")
                    {
                        cmdSelectPortfolio.Parameters["@HUB"].Value = market;
                        cmdSelectPortfolio.Parameters["@product"].Value = product;
                        cmdSelectPortfolio.Parameters["@ad_login"].Value = user;
                    }
                    else
                        cmdSelectPortfolio.CommandText = "select STRIP, Portfolio_ID from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = 'PJM' and A.flag_deprecated=0and product = 'EES/PTP' and ACTIVE = 'Y' order by STRIP";

                }
            }
            else if (product == "Virtual")
            {
                SqlCommand cmd = VayuDBConnection.CreateCommand();
                if (usertype == "user")
                {
                    cmdSelectPortfolio.Parameters["@HUB"].Value = market;
                    cmdSelectPortfolio.Parameters["@product"].Value = product;
                    cmdSelectPortfolio.Parameters["@ad_login"].Value = user;
                }
                else
                    cmdSelectPortfolio.CommandText = "select STRIP, Portfolio_ID from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = 'PJM' and A.flag_deprecated=0and product = 'Virtual' and ACTIVE = 'Y' order by STRIP";

            }
            reader = cmdSelectPortfolio.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                int id = reader.GetInt32(1);
                Portfolio portfolio = new Portfolio();
                portfolio.ID = id;
                portfolio.Name = name;
                portfolio.Market = market;
                portfolio.IsUptos = product == "EES/PTP";
                portfolioList.Add(portfolio);
            }
            reader.Close();
            VayuDBConnection.Close();
            callback(portfolioList, null);
        }
        /// <summary>
        /// Gets the portfolio.
        /// </summary>
        /// <param name="bidDate">The bid date.</param>
        /// <param name="user">The user.</param>
        /// <param name="product">The product.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public static List<Portfolio> GetPortfolio(DateTime bidDate, string user, string product, string market)
        {
            if (bidDate.Year == 1)
            {
                return null;
            }
            LoadDB();
            #region new by sangram
            SqlDataReader portfilioreader = null;
            List<int> portfolioList_all = new List<int>();
            if (VayuDBConnection.State == ConnectionState.Closed)
            {
                VayuDBConnection.Open();
            }
            SqlCommand cmdgetallPortfolios = VayuDBConnection.CreateCommand();
            cmdgetallPortfolios.CommandText = "select distinct(PortfolioKey),count(*) from Vayu..ErcotPTPBids where ENDMARKETDATETIME > @START " +
                "AND ENDMARKETDATETIME <= @END group by PortfolioKey ";
            cmdgetallPortfolios.Parameters.AddWithValue("@START", "ENDMARKETDATETIME");
            cmdgetallPortfolios.Parameters.AddWithValue("@END", "ENDMARKETDATETIME");
            cmdgetallPortfolios.Connection = VayuDBConnection;

            cmdgetallPortfolios.Parameters["@START"].Value = bidDate;
            cmdgetallPortfolios.Parameters["@END"].Value = bidDate.AddDays(1);
            portfilioreader = cmdgetallPortfolios.ExecuteReader();
            while(portfilioreader.Read())
            {
                int portfolioId= portfilioreader.GetInt32(0);
                int count= portfilioreader.GetInt32(1);
                if(count > 0)
                {
                    portfolioList_all.Add(portfolioId);
                }
            }
            cmdgetallPortfolios.Connection.Close();
            #endregion
            List<Portfolio> portfolioList = new List<Portfolio>();
            VayuDBConnection.Open();
            VayuDBConnection.Open();
            SqlDataReader reader = null;
            string usertype = "";
            SqlCommand cmdusertype = VayuDBConnection.CreateCommand();
            cmdusertype.CommandText = "select user_type from END_USER where ad_login=" + "'" + user + "' ";
            cmdusertype.Connection = VayuDBConnection;
            SqlDataReader cmdusertypedr = cmdusertype.ExecuteReader();
            while (cmdusertypedr.Read())
            {
                usertype = cmdusertypedr.GetValue(0).ToString();
            }


            {
                if (usertype == "user")
                {
                    cmdSelectPortfolio.Parameters["@HUB"].Value = market;
                    cmdSelectPortfolio.Parameters["@product"].Value = product;
                    cmdSelectPortfolio.Parameters["@ad_login"].Value = user;
                }
                else
                {
                        cmdSelectPortfolio.CommandText = "select STRIP, Portfolio_ID from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = 'ERCOT' and A.flag_deprecated=0and product = 'EES/PTP' and ACTIVE = 'Y' order by STRIP";
                }
                reader = cmdSelectPortfolio.ExecuteReader();
                while (reader.Read())
                {
                    int count = 0;
                    string name = reader.GetString(0);
                    int id = reader.GetInt32(1);
                    Portfolio portfolio = new Portfolio();
                    portfolio.ID = id;
                    portfolio.Name = name;
                    portfolio.MarketKey = GetMarket(market);
                    //if (market == "ERCOT")
                    //{
                    //    cmdSelectPortfolioInErcotEESBids.CommandTimeout = 30000;
                    //    cmdSelectPortfolioInErcotEESBids.Parameters["@PortfolioKey"].Value = reader.GetValue(1);
                    //    cmdSelectPortfolioInErcotEESBids.Parameters["@START"].Value = bidDate;
                    //    cmdSelectPortfolioInErcotEESBids.Parameters["@END"].Value = bidDate.AddDays(1);
                    //    try
                    //    {
                    //        if (VayuErcotDBConnection.State == ConnectionState.Closed)
                    //        {
                    //            VayuErcotDBConnection.Open();
                    //        }
                    //        count = (int)cmdSelectPortfolioInErcotEESBids.ExecuteScalar();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //    }
                    //}
                    if (portfolioList_all.Contains(portfolio.ID))
                    {
                        if ( !portfolioList.Contains(portfolio))
                        {
                            portfolioList.Add(portfolio);
                        }
                    }
                }
                reader.Close();
            }
            VayuDBConnection.Close();
            VayuDBConnection.Close();
            return portfolioList;
        }
        /// <summary>
        /// Gets the vectors.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <param name="contingency">The contingency.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public static List<Vector> GetVectors(string constraint, string contingency, string market = "PJM")
        {
            List<Vector> vectorList = new List<Vector>();
            LoadDB();
            VayuDBConnection.Open();
            SqlCommand cmd = null;


            cmd = cmdSelectSensitivity;

            cmd.Parameters["@constraintname"].Value = constraint;
            cmd.Parameters["@contingency"].Value = contingency;
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Vector vector = new Vector();
                vector.NodeKey = (int)reader.GetDecimal(0);
                vector.Sensitivity = (double)reader.GetDecimal(1);
                vectorList.Add(vector);
            }
            VayuDBConnection.Close();
            return vectorList;
        }
        /// <summary>
        /// Gets the constingencies.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public static List<VectorKeyInfo> GetConstingencies(string constraint, string market = "PJM")
        {
            List<VectorKeyInfo> contingencyList = new List<VectorKeyInfo>();
            if (string.IsNullOrEmpty(constraint))
                return contingencyList;

            LoadDB();
            VayuDBConnection.Open();
            SqlCommand sql = null;

            sql = cmdSelectContingencySensitivity;

            sql.Parameters["@constraintname"].Value = constraint;
            SqlDataReader reader = sql.ExecuteReader();
            while (reader.Read())
            {
                VectorKeyInfo info = new VectorKeyInfo();
                info.DBText = reader.GetString(0);
                info.DisplayText = (info.DBText ?? "").Replace("Contingency", "").TrimStart();
                contingencyList.Add(info);
            }
            VayuDBConnection.Close();
            return contingencyList;
        }
      
        
        public static PricingNode GetNodeFromeExtId(long extNodeId,int MarketKey) 
        {
            int nodekey = 0;
            if (dictNodeExtHash.ContainsKey(extNodeId))
            {
                nodekey = dictNodeExtHash[extNodeId];
            }
            else
            {
                bool close = true;
                if (VayuDBConnection == null) 
                {
                    LoadDB();
                }
                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();

                }
                else
                {
                    close = false;
                }
                cmdSelectNodeFromExternalCommand.Parameters["@ExternalNodeID"].Value = extNodeId;
                if(MarketKey==9)
                  cmdSelectNodeFromExternalCommand.CommandText="Select nodekey from Vayu..node where nodekey ="+ extNodeId +"";
                SqlDataReader reader = cmdSelectNodeFromExternalCommand.ExecuteReader();

                while(reader.Read())
                {
                    nodekey = (int)reader.GetValue(0);
                    if (!dictNodeExtHash.ContainsKey(extNodeId))
                    {
                        dictNodeExtHash.Add(extNodeId, nodekey);
                    }
                }reader.Close();
            }
            return null;
        }
        /// <summary>
        /// Gets the date range.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static List<DateTime> GetDateRange(string name)
        {
            List<DateTime> dateTimeList = new List<DateTime>();
            if (name != null)
            {
                LoadDB();
                VayuDBConnection.Open();
                cmdSelectDateDateRange.Parameters["@trader"].Value = sUser;
                cmdSelectDateDateRange.Parameters["@datename"].Value = name;
                SqlDataReader reader = cmdSelectDateDateRange.ExecuteReader();
                while (reader.Read())
                {
                    dateTimeList.Add(reader.GetDateTime(0));
                }
                VayuDBConnection.Close();
            }
            return dateTimeList;
        }
        /// <summary>
        /// Gets the node frome ext identifier.
        /// </summary>
        /// <param name="extNodeId">The ext node identifier.</param>
        /// <returns></returns>

        /// <summary>
        /// Gets the name of the node from.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="marketkey">The marketkey.</param>
        /// <returns></returns>
        public static PricingNode GetNodeFromName(string name, int marketkey)
        {
            try
            {
                LoadDB();
            }
            catch (Exception ex)
            {

            }
            int nodeKey = 0;
            string key = name + marketkey;
            if (dictNodeNameHash.ContainsKey(key))
            {
                nodeKey = dictNodeNameHash[key];
            }
            else
            {
                if (marketkey == 1)
                {
                    if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                    {
                        VayuDBConnection.Open();
                    }
                    cmdSelectNodeFromName.Connection = VayuDBConnection;
                }
                else
                {
                    if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                    {
                        VayuDBConnection.Open();
                    }
                    cmdSelectNodeFromName.Connection = VayuDBConnection;
                }
                cmdSelectNodeFromName.Parameters["@nodename"].Value = name;
                cmdSelectNodeFromName.Parameters["@marketkey"].Value = marketkey;
                SqlDataReader reader = cmdSelectNodeFromName.ExecuteReader();
                while (reader.Read())
                {
                    nodeKey = CommonDataConversions.GetInt(reader[0]).GetValueOrDefault();
                    if (!dictNodeNameHash.ContainsKey(key))
                    {
                        dictNodeNameHash.Add(key, nodeKey);
                    }
                }
                reader.Close();
                VayuDBConnection.Close();
                VayuDBConnection.Close();
            }
            if (nodeKey != 0 && marketkey == 1)
            {
                return GetNode(nodeKey,1);
            }
            else if (nodeKey != 0 && marketkey == 9)
            {
                return GetNode(nodeKey, 9);
            }
            return null;
        }
        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <param name="externalId">The external identifier.</param>
        /// <returns></returns>
        public static PricingNode GetNode(long externalId, int marketKey=1)
        {
            int nodeKey = 0;
            if (marketKey == 1)
            {
                if (dictPJMNodeHash.Count == 0)
                {
                    LoadDB();

                    bool close = false;
                    if (marketKey == 9)
                    {
                        if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                        {
                            cmdSelectNode.Connection = VayuDBConnection;
                            VayuDBConnection.Open();
                            close = true;
                        }
                    }
                    else
                    {
                        if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                        {
                            VayuDBConnection.Open();
                            close = true;
                        }
                    }
                    SqlDataReader reader = cmdSelectNode.ExecuteReader();
                    while (reader.Read())
                    {
                        int key = (int)reader.GetValue(0);
                        string name = reader.GetString(1);
                        long externalID = reader.IsDBNull(2) ? 0 : Convert.ToInt64(reader.GetValue(2));
                        int market = (int)reader.GetValue(3);
                        string zone = reader.IsDBNull(4) ? null : reader.GetString(4);
                        int nodeTypeKey = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5));
                        PricingNode node = new PricingNode();
                        node.NodeKey = key;
                        nodeKey = key;
                        node.NodeName = name;
                        node.ExternalNodeId = externalID;
                        node.MarketKey = market;
                        node.Zone = zone;
                        node.NodeTypeKey = nodeTypeKey;
                        dictPJMNodeHash.Add(key, node);


                    }
                    if (close)
                    {
                        if (marketKey == 9)
                            VayuDBConnection.Close();
                        else
                            VayuDBConnection.Close();
                    }
                }
                if (dictPJMNodeHash.ContainsKey(externalId))
                {
                    return dictPJMNodeHash[externalId];
                }
            }
            else if (marketKey == 9)
            {
                if (dictErcotNodeHash.Count == 0)
                {
                    LoadDB();

                    bool close = false;
                    if (marketKey == 9)
                    {
                        if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                        {
                            cmdSelectNode.Connection = VayuDBConnection;
                            VayuDBConnection.Open();
                            close = true;
                        }
                    }
                    else
                    {
                        if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
                        {
                            VayuDBConnection.Open();
                            close = true;
                        }
                    }
                    SqlDataReader reader = cmdSelectNode.ExecuteReader();
                    while (reader.Read())
                    {
                        int key = (int)reader.GetValue(0);
                        string name = reader.GetString(1);
                        long externalID = reader.IsDBNull(2) ? 0 : Convert.ToInt64(reader.GetValue(2));
                        int market = (int)reader.GetValue(3);
                        string zone = reader.IsDBNull(4) ? null : reader.GetString(4);
                        int nodeTypeKey = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5));
                        PricingNode node = new PricingNode();
                        node.NodeKey = key;
                        nodeKey = key;
                        node.NodeName = name;
                        node.ExternalNodeId = externalID;
                        node.MarketKey = market;
                        node.Zone = zone;
                        node.NodeTypeKey = nodeTypeKey;
                        dictErcotNodeHash.Add(key, node);


                    }
                    if (close)
                    {
                        if (marketKey == 9)
                            VayuDBConnection.Close();
                        else
                            VayuDBConnection.Close();
                    }
                }
                if (dictErcotNodeHash.ContainsKey(externalId))
                {
                    return dictErcotNodeHash[externalId];
                }
            }
            return null;
        }
      
        /// <summary>
        /// Gets the similar date.
        /// </summary>
        /// <param name="tradeDate">The trade date.</param>
        /// <returns></returns>
        public static DateTime GetSimilarDate(DateTime tradeDate)
        {
            DateTime SimilarDate = DateTime.Today;
            LoadDB();
            bool close = false;
            if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
            {
                VayuDBConnection.Open();
                close = true;
            }
            cmdSelectSimilarLoadDay.Parameters["@tradeDate"].Value = tradeDate;
            SqlDataReader dr = cmdSelectSimilarLoadDay.ExecuteReader();
            while (dr.Read())
            {
                SimilarDate = dr.GetDateTime(0);
            }
            if (close)
            {
                VayuDBConnection.Close();
            }
            return SimilarDate;
        }
        /// <summary>
        /// Gets the risk constraints.
        /// </summary>
        /// <param name="similarDate">The similar date.</param>
        /// <returns></returns>
        public static List<int> GetRiskConstraints(DateTime similarDate)
        {
            List<int> ConstraintList = new List<int>();
            LoadDB();
            bool close = false;
            if (VayuDBConnection.State == System.Data.ConnectionState.Closed)
            {
                VayuDBConnection.Open();
                close = true;
            }
            cmdSelectRiskConstraints.Parameters["@similarDate"].Value = similarDate;
            SqlDataReader drConstraintReader = cmdSelectRiskConstraints.ExecuteReader();
            while (drConstraintReader.Read())
            {
                ConstraintList.Add(Convert.ToInt32(drConstraintReader.GetValue(0)));
            }
            drConstraintReader.Close();
            if (close)
            {
                VayuDBConnection.Close();
            }
            return ConstraintList;
        }

        /// <summary>
        /// Gets the SPP locations.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> GetSppLocations()
        {
            if (dictSppLocationHash.Count == 0)
            {
                if (VayuDBConnection == null)
                {
                    LoadDB();
                }
                using (SqlConnection con = new SqlConnection(VayuDBConnection.ConnectionString))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = "select distinct n.NodeKey,loc.SettlementLocationName from SPP.settlementlocationname loc join Node n on n.NodeName =loc.SettlementLocationName where n.MarketKey=12";
                            cmd.Connection.Open();
                            IDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    int nodekey = reader.IsDBNull(0) ? -1 : Convert.ToInt32(reader.GetValue(0));
                                    string nodeName = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString();
                                    if (nodekey > 0 && nodeName != "")
                                    {
                                        if (dictSppLocationHash.ContainsKey(nodekey))
                                        {
                                            dictSppLocationHash.Remove(nodekey);
                                        }
                                        dictSppLocationHash.Add(nodekey, nodeName);
                                    }
                                }
                                catch
                                {
                                }
                            }
                            reader.Close();
                            cmd.Connection.Close();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return dictSppLocationHash;
        }
        /// <summary>
        /// Gets the bid identifier.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="p">if set to <c>true</c> [p].</param>
        /// <param name="segment">The segment.</param>
        /// <param name="submitDate">The submit date.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public static string GetBidId(string location, int hour, bool p, int segment, DateTime submitDate, int market)
        {
            string bidid = string.Empty;
            using (SqlConnection con = new SqlConnection(VayuDBConnection.ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select distinct BIDID from VirtualBids where NodeKey in (select top(1)NodeKey from Node where MarketKey=@mkt and NodeName =@nodename) and  MarketDate=@marketdate and Market=@mkt and HE=@hour and incdec=@incdec";
                    cmd.Parameters.AddWithValue("nodename", location);
                    cmd.Parameters.AddWithValue("marketdate", submitDate.ToShortDateString());
                    cmd.Parameters.AddWithValue("@mkt", market);
                    cmd.Parameters.AddWithValue("hour", hour);
                    cmd.Parameters.AddWithValue("incdec", p ? "I" : "D");
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        bidid = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString();
                        if (bidid != "")
                        {
                            break;
                        }
                    }
                    reader.Close();
                    con.Close();
                }
            }
            return bidid;
        }
        /// <summary>
        /// Gets the pricing nodes for market.
        /// </summary>
        /// <param name="marketkey">The marketkey.</param>
        /// <returns></returns>
        public static Dictionary<string, PricingNode> GetPricingNodesForMarket(int marketkey)
        {
            if (dictMarketNodeHash.ContainsKey(marketkey))
                return dictMarketNodeHash[marketkey];
            else
            {
                Dictionary<string, PricingNode> nodeHash = new Dictionary<string, PricingNode>();
                if (marketkey == 1)
                {
                    using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "select nodekey, nodename, externalnodeid, zone, nodetypekey from node where marketkey = " + marketkey + ";";
                            cmd.Connection.Open();
                            IDataReader reader1 = cmd.ExecuteReader();
                            while (reader1.Read())
                            {
                                string nodename = reader1.IsDBNull(1) ? "" : reader1.GetValue(1).ToString();
                                if (nodename != "" && !nodeHash.ContainsKey(nodename))
                                {
                                    nodeHash.Add(nodename, new PricingNode
                                    {
                                        NodeName = nodename,
                                        NodeKey = reader1.IsDBNull(0) ? -1 : Convert.ToInt32(reader1.GetValue(0)),
                                        ExternalNodeId = reader1.IsDBNull(2) ? -1 : Convert.ToInt64(reader1.GetValue(2)),
                                        MarketKey = marketkey,
                                        Zone = reader1.IsDBNull(3) ? "" : reader1.GetValue(3).ToString(),
                                        NodeTypeKey = reader1.IsDBNull(4) ? -1 : Convert.ToInt32(reader1.GetValue(4)),
                                    });
                                }
                            }
                            reader1.Close();
                            cmd.Connection.Close();
                        }
                    }
                }
                else if (marketkey == 9)
                {
                    using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "select nodekey, nodename, externalnodeid, zone, nodetypekey from node where marketkey = " + marketkey + ";";
                            cmd.Connection.Open();
                            IDataReader reader1 = cmd.ExecuteReader();
                            while (reader1.Read())
                            {
                                string nodename = reader1.IsDBNull(1) ? "" : reader1.GetValue(1).ToString();
                                if (nodename != "" && !nodeHash.ContainsKey(nodename))
                                {
                                    nodeHash.Add(nodename, new PricingNode
                                    {
                                        NodeName = nodename,
                                        NodeKey = reader1.IsDBNull(0) ? -1 : Convert.ToInt32(reader1.GetValue(0)),
                                        ExternalNodeId = reader1.IsDBNull(2) ? -1 : Convert.ToInt64(reader1.GetValue(2)),
                                        MarketKey = marketkey,
                                        Zone = reader1.IsDBNull(3) ? "" : reader1.GetValue(3).ToString(),
                                        NodeTypeKey = reader1.IsDBNull(4) ? -1 : Convert.ToInt32(reader1.GetValue(4)),
                                    });
                                }
                            }
                            reader1.Close();
                            cmd.Connection.Close();
                        }
                    }
                }
                dictMarketNodeHash.Add(marketkey, nodeHash);
                return nodeHash;
            }
        }

        public static Dictionary<DateTime, string> GetPeakOPeakHrsForPeriod(DateTime sDate, DateTime eDate, int marketKey)
        {
            Dictionary<DateTime, string> peakOffPeakHrsDict = new Dictionary<DateTime, string>();
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = "select MarketDateTime  , PeakYN from markettime where MarketKey = 1  and MarketDateTime between @StartDate and @EndDate order by MarketDateTime";
                    cmd.Parameters.AddWithValue("@StartDate", sDate.Date);
                    cmd.Parameters.AddWithValue("@EndDate", eDate.Date);
                    cmd.Connection = con;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        DateTime dt = Convert.ToDateTime(rdr.GetValue(0));
                        string peakOffPeak = rdr.GetValue(1).ToString();
                        peakOffPeakHrsDict.Add(dt, peakOffPeak);
                    }
                    rdr.Close();
                    con.Close();
                }
            }
            return peakOffPeakHrsDict;
        }
        /// <summary>
        /// Gets the user list.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<string> GetUserList()
        {
            LoadDB();
            VayuDBConnection.Open();
            cmdSelectUser.CommandText = "select ad_login from END_USER where user_type='admin' and activeyn='Y'";
            SqlDataReader reader = cmdSelectUser.ExecuteReader();
            ObservableCollection<string> UserList = new ObservableCollection<string>();
            while (reader.Read())
            {
                UserList.Add(reader.GetString(0));
            }
            reader.Close();
            VayuDBConnection.Close();
            return UserList;
        }
        public static ObservableCollection<string> GetERCOTUserList()
        {
            LoadDB();
            VayuDBConnection.Open();
            cmdSelectERCOTUser.CommandText = "select distinct ad_login from END_USER a join END_USER_MARKET_PORTFOLIO b on a.enduser_key=b.end_user_key and b.marketkey=9";
            SqlDataReader reader = cmdSelectERCOTUser.ExecuteReader();
            ObservableCollection<string> ERCOTUserList = new ObservableCollection<string>();
            while (reader.Read())
            {
                ERCOTUserList.Add(reader.GetString(0));
            }
            reader.Close();
            VayuDBConnection.Close();
            return ERCOTUserList;
        }

        public static Dictionary<int, string> GetNodeFromNames(int marketkey)
        {
            Dictionary<int, string> nodeDict = new Dictionary<int, string>();
           
            using (SqlConnection con = new VayuDBConnection().GetInstance().GetSqlConnection())
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "select NodeKey , nodename from Node where MarketKey = " + marketkey.ToString();
                    cmd.Connection = con;
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int nodeKey = Convert.ToInt32(rdr.GetValue(0));
                        string nodeName = rdr.GetValue(1).ToString();
                        if (!nodeDict.ContainsKey(nodeKey))
                            nodeDict.Add(nodeKey, nodeName);
                    }
                    rdr.Close();
                    con.Close();
                }
            }
            return nodeDict;
        }


        /// <summary>
        /// Gets the PJM valid virtual nodes.
        /// </summary>
        /// <param name="PNodeId">The p node identifier.</param>
        /// <returns></returns>
        public static bool GetPJMValidVirtualNodes(long PNodeId)
        {
            LoadDB();
            VayuDBConnection.Open();

            cmdSelectPJMValidVirtualNodes = VayuDBConnection.CreateCommand();
            cmdSelectPJMValidVirtualNodes.CommandText = "select distinct pnodeid from pjm.virtualvalidnodes where pnodeid = " + PNodeId;

            object obj = cmdSelectPJMValidVirtualNodes.ExecuteScalar();
            int result = 0;

            if (obj != null)
            {
                result = int.Parse(cmdSelectPJMValidVirtualNodes.ExecuteScalar().ToString());
            }
            VayuDBConnection.Close();

            if (result > 0)
                return false;
            else
                return true;
        }
        #endregion
        public static List<Portfolio> GetFTRPortfolio(DateTime bidDate, string user, string product, string market, string auctionname)
        {
            if (bidDate.Year == 1)
            {
                return null;
            }
            LoadDB();
            string usertype = "";
            List<Portfolio> portfolioList = new List<Portfolio>();
            VayuDBConnection.Open();
            SqlDataReader reader = null;

            SqlCommand cmdusertype = VayuDBConnection.CreateCommand();
            cmdusertype.CommandText = "select user_type from END_USER where ad_login=" + "'" + user + "' ";
            cmdusertype.Connection = VayuDBConnection;
            SqlDataReader cmdusertypedr = cmdusertype.ExecuteReader();
            while (cmdusertypedr.Read())
            {
                usertype = cmdusertypedr.GetValue(0).ToString();
            }
            SqlCommand cmd = VayuDBConnection.CreateCommand();
            if (usertype == "user")
            {
                cmdSelectFTRPortfolio.Parameters["@HUB"].Value = market;
                cmdSelectFTRPortfolio.Parameters["@product"].Value = product;
                cmdSelectFTRPortfolio.Parameters["@ad_login"].Value = user;
            }
            else
                cmdSelectFTRPortfolio.CommandText = "select STRIP, Portfolio_ID from PORTFOLIO P inner join account A on P.account=A.account_id where HUB = '" + market + "' and A.flag_deprecated=0and product = 'FTR' and ACTIVE = 'Y' order by STRIP";
            reader = cmdSelectFTRPortfolio.ExecuteReader();
            while (reader.Read())
            {
                int count = 0;
                string name = reader.GetString(0);
                int id = reader.GetInt32(1);
                Portfolio portfolio = new Portfolio();
                portfolio.ID = id;
                portfolio.Name = name;
                if (market == "PJM")
                    portfolio.MarketKey = 1;
                else
                    portfolio.MarketKey = 9;
                cmdSelectPortfolioInFTRBids.Parameters["@PortfolioKey"].Value = reader.GetValue(1);
                cmdSelectPortfolioInFTRBids.Parameters["@Auction"].Value = auctionname;
                try
                {
                    if (VayuDBConnection.State == ConnectionState.Closed)
                    {
                        VayuDBConnection.Open();
                    }
                    count = (int)cmdSelectPortfolioInFTRBids.ExecuteScalar();
                }
                catch (Exception ex)
                {
                }
                if (count > 0 && !portfolioList.Contains(portfolio))
                {
                    portfolioList.Add(portfolio);
                }
                if (count <= 0)
                {
                    portfolioList.Add(portfolio);

                    if (usertype == "user")
                        portfolioList.RemoveAll(x => x.Name == "Vayu_FTR_STRAT");
                    if (user == "gojira")
                    {
                        Portfolio addportfolio = new Portfolio();
                        addportfolio.ID = 674;
                        addportfolio.Name = "Vayu_FTR_STRAT";
                        portfolioList.Add(addportfolio);
                    }
                }
            }
            reader.Close();
            VayuDBConnection.Close();
            if (usertype == "user")
                portfolioList.RemoveAll(x => x.Name == "Vayu_FTR_STRAT");
            if (user == "gojira")
            {
                Portfolio addportfolio = new Portfolio();
                addportfolio.ID = 674;
                addportfolio.Name = "Vayu_FTR_STRAT";
                portfolioList.Add(addportfolio);
            }
            return portfolioList.OrderBy(x => x.Name).ToList<Portfolio>();
        }
        public static List<Portfolio> GetExterFTRPortfolio(DateTime bidDate, string user, string product, string market, string auctionname)
        {
            List<Portfolio> externalportfolioList = new List<Portfolio>();
            try
            {
                LoadDB();
                if (VayuDBConnection.State == ConnectionState.Closed)
                {
                    VayuDBConnection.Open();
                }
                SqlCommand selectexternalportfolio = VayuDBConnection.CreateCommand();
                selectexternalportfolio.CommandText = "Select * from pjm.FTRExternalPortfolios";
                selectexternalportfolio.Connection = VayuDBConnection;
                SqlDataReader reader = selectexternalportfolio.ExecuteReader();
                while (reader.Read())
                {
                    Portfolio portfolio = new Portfolio();
                    portfolio.ID = reader.GetInt32(0);
                    portfolio.Name = reader.GetString(1);
                    externalportfolioList.Add(portfolio);
                }
            }
            catch
            {

            }
            return externalportfolioList.ToList();
        }


        public static void SaveFtrErcotBids(int portfolioKey, string auction, List<FTRBid> bids)
        {
            LoadDB();
            VayuDBConnection.Open();
            cmdDeleteFtrBidErcot.Parameters["@auction"].Value = auction;
            cmdDeleteFtrBidErcot.Parameters["@portfoliokey"].Value = portfolioKey;
            cmdDeleteFtrBidErcot.ExecuteNonQuery();
            foreach (FTRBid bid in bids)
            {
                cmdInsertFtrBidCommandErcot.Parameters["@PeriodKey"].Value = bid.PeriodKey;
                cmdInsertFtrBidCommandErcot.Parameters["@Participant"].Value = bid.mParticipant;
                cmdInsertFtrBidCommandErcot.Parameters["@MarketKey"].Value = bid.mMarketKey;
                cmdInsertFtrBidCommandErcot.Parameters["@hedgetype"].Value = bid.HedgeType;

                cmdInsertFtrBidCommandErcot.Parameters["@month"].Value = bid.month;
                cmdInsertFtrBidCommandErcot.Parameters["@auction"].Value = auction;
                cmdInsertFtrBidCommandErcot.Parameters["@classtype"].Value = bid.ClassType;
                cmdInsertFtrBidCommandErcot.Parameters["@tradetype"].Value = bid.TradeType;
                cmdInsertFtrBidCommandErcot.Parameters["@hedgetype"].Value = bid.HedgeType;

                cmdInsertFtrBidCommandErcot.Parameters["@portfoliokey"].Value = portfolioKey;
                cmdInsertFtrBidCommandErcot.Parameters["@periodhours"].Value = bid.PeriodHours;
                cmdInsertFtrBidCommandErcot.Parameters["@periodname"].Value = bid.PeriodName;
                cmdInsertFtrBidCommandErcot.Parameters["@periodtype"].Value = bid.PeriodName;
                cmdInsertFtrBidCommandErcot.Parameters["@source"].Value = bid.Source;
                cmdInsertFtrBidCommandErcot.Parameters["@sourceextid"].Value = bid.SourceExt;
                cmdInsertFtrBidCommandErcot.Parameters["@sink"].Value = bid.Sink;
                cmdInsertFtrBidCommandErcot.Parameters["@sinkextid"].Value = bid.SinkExt;

                if (bid.TCRID == 0)
                    cmdInsertFtrBidCommandErcot.Parameters["@TcrId"].Value = DBNull.Value;
                else
                    cmdInsertFtrBidCommandErcot.Parameters["@TcrId"].Value = bid.TCRID;

                cmdInsertFtrBidCommandErcot.Parameters["@mw1"].Value = bid.MW1;
                cmdInsertFtrBidCommandErcot.Parameters["@price1"].Value = bid.Price1;
                if (bid.MW2 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw2"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw2"].Value = bid.MW2;
                }
                if (bid.Price2 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price2"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price2"].Value = bid.Price2;
                }
                if (bid.MW3 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw3"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw3"].Value = bid.MW3;
                }
                if (bid.Price3 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price3"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price3"].Value = bid.Price3;
                }
                if (bid.MW4 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw4"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw4"].Value = bid.MW4;
                }
                if (bid.Price4 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price4"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price4"].Value = bid.Price4;
                }
                if (bid.MW5 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw5"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw5"].Value = bid.MW5;
                }
                if (bid.Price5 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price5"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price5"].Value = bid.Price5;
                }
                if (bid.MW6 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw6"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@mw6"].Value = bid.MW6;
                }
                if (bid.Price6 == null)
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price6"].Value = DBNull.Value;
                }
                else
                {
                    cmdInsertFtrBidCommandErcot.Parameters["@price6"].Value = bid.Price6;
                }

                try
                {
                    cmdInsertFtrBidCommandErcot.ExecuteNonQuery();
                }
                catch (Exception ex) { }
            }
            VayuDBConnection.Close();
        }
    }
}
