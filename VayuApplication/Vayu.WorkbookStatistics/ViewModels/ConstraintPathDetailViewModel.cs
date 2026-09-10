using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vayu.DBLibrary;
using Vayu.WorkbookStatistics.Model;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class ConstraintPathDetailViewModel : BindableBase
    {
        #region Declaration And Properties

        /// <summary>
        /// The m user
        /// </summary>
        private string mUser = Environment.UserName;
        /// <summary>
        /// The s portfolio details hash
        /// </summary>
        private static Dictionary<int, string> sPortfolioDetailsHash = new Dictionary<int, string>();

        /// <summary>
        /// The path full list
        /// </summary>
        private List<Path> pathFullList;
        /// <summary>
        /// The path list
        /// </summary>
        private List<Path> pathList;
        /// <summary>
        /// Gets or sets the path list.
        /// </summary>
        /// <value>
        /// The path list.
        /// </value>
        public List<Path> PathList
        {
            get
            {
                return pathList;
            }
            set
            {
                pathList = value;
                RaisePropertyChanged("PathList");
            }
        }
        /// <summary>
        /// The path selected list
        /// </summary>
        private List<Path> pathSelectedList;
        /// <summary>
        /// The m parent model
        /// </summary>
        public MainWindowViewModel mParentModel;
        /// <summary>
        /// The selected constraint path value
        /// </summary>
        private Model.Exposure SelectedConstraintPathValue;
        /// <summary>
        /// Gets or sets the path selected list.
        /// </summary>
        /// <value>
        /// The path selected list.
        /// </value>
        public List<Path> PathSelectedList
        {
            get
            {
                return pathSelectedList;
            }
            set
            {
                pathSelectedList = value;
                RaisePropertyChanged("PathSelectedList");
            }
        }
        /// <summary>
        /// The m constraint check view model
        /// </summary>
        private ConstraintCheckViewModel mConstraintCheckViewModel;
        /// <summary>
        /// The m path detail view model
        /// </summary>
        private PathDetailViewModel mPathDetailViewModel;
        /// <summary>
        /// Gets the delete paths command.
        /// </summary>
        /// <value>
        /// The delete paths command.
        /// </value>
        public DelegateCommand DeletePathsCommand { get; private set; }

        #endregion
        public ConstraintPathDetailViewModel()
        {
            DeletePathsCommand = new DelegateCommand(() => DeleteSelectedPaths());
        }
        public ConstraintPathDetailViewModel(MainWindowViewModel mParentModel1, ConstraintCheckViewModel constraintCheckViewModel, PathDetailViewModel pathDetailViewModel, List<Path> list, Model.Exposure SelectedConstraintPathValue1)
        {
            DeletePathsCommand = new DelegateCommand(() => DeleteSelectedPaths());
            this.mParentModel = mParentModel1;
            this.mConstraintCheckViewModel = constraintCheckViewModel;
            this.pathFullList = list;
            this.mPathDetailViewModel = pathDetailViewModel;
            SelectedConstraintPathValue = SelectedConstraintPathValue1;
            PathList = pathFullList.Where(a => a.Source == SelectedConstraintPathValue.Constraint && a.Sink == SelectedConstraintPathValue.Contingency).ToList();
        }
        #region Private Methods


        private static string GetPortfolioName(Bid bid)
        {
            if (!sPortfolioDetailsHash.ContainsKey(bid.PortfolioKey))
            {
                sPortfolioDetailsHash.Add(bid.PortfolioKey, DBAccess.GetPortfolioName(bid.PortfolioKey));
            }
            return sPortfolioDetailsHash[bid.PortfolioKey];
        }

        private string GetMarket(int p)
        {
            switch (p)
            {
                //  case 1: return "PJM";
                //case 2: return "MISO";
                //case 3: return "NYISO";
                // case 7: return "CAISO";
                //case 9: return "ERCOT";
                default: return "";
            }
        }

        private void Retrieve(Path editedRow, string savedName)
        {
            Dictionary<string, Path> pathHash = new Dictionary<string, Path>();
            List<string> pathComboList = new List<string>();
            List<Bid> retrievedBidList = new List<Bid>();
            try
            {
                if (editedRow == null && PathList.Count > 0)
                {
                    retrievedBidList = DBAccess.GetBids(GetMarket(PathList[0].Market), PathList[0].PortfolioKey, null, PathList[0].PortfolioDate, PathList[0].PortfolioDate.AddDays(1), true, "MOVED");
                }
                else
                {
                    retrievedBidList = DBAccess.GetBids(GetMarket(editedRow.Market), editedRow.PortfolioKey, null, editedRow.PortfolioDate, editedRow.PortfolioDate.AddDays(1), true, "MOVED");
                }
                foreach (Bid bid in retrievedBidList)
                {
                    Path path = new Path();
                    PricingNode sourcePricingNode = DBAccess.GetNode(bid.Source, bid.Market);
                    string pathName = sourcePricingNode.NodeName;
                    path.Source = sourcePricingNode.NodeName;
                    path.SourcePNodeId = sourcePricingNode.ExternalNodeId;
                    path.SourceZone = sourcePricingNode.Zone;
                    if (bid.Sink != 0)
                    {
                        PricingNode sinkPricingNode = DBAccess.GetNode(bid.Sink, bid.Market);
                        path.Sink = sinkPricingNode.NodeName;
                        path.SinkZone = sinkPricingNode.Zone;
                        path.SinkPNodeId = sinkPricingNode.ExternalNodeId;
                        pathName = sourcePricingNode.NodeName + "->" + sinkPricingNode.NodeName;
                    }
                    else
                    {
                        path.Sink = "";
                        path.SinkZone = "";
                    }
                    if (!pathComboList.Contains(pathName))
                    {
                        pathComboList.Add(pathName);
                    }
                    path.MarketDateTime = bid.MarketDateTime;
                    if (bid.MarketDateTime.Hour == 0)
                    {
                        path.AnalysisType = "24";
                    }
                    else
                    {
                        path.AnalysisType = bid.MarketDateTime.Hour.ToString();
                    }
                    path.Price = bid.Price;
                    path.MW = bid.MW;
                    path.Status = bid.Status;
                    path.BidId = bid.BidId;
                    path.IsUptos = bid.IsUptos;
                    path.Portfolio = GetPortfolioName(bid);
                    path.PortfolioKey = bid.PortfolioKey;
                    path.Submit = true;
                    path.Market = bid.Market;
                    path.RiskPath = false;
                    path.PortfolioDate = editedRow == null ? PathList[0].PortfolioDate : editedRow.PortfolioDate;
                    path.Comments = bid.Comments;
                    if (pathHash.ContainsKey(bid.BidId))
                    {
                        Path savedPath = pathHash[bid.BidId];
                        savedPath.AnalysisType = savedPath.AnalysisType + "." + path.AnalysisType;
                    }
                    else
                    {
                        pathHash.Add(bid.BidId, path);
                    }
                }
                if (mParentModel != null)
                {
                    mParentModel.PathList = pathHash.Values.ToList();
                    if (mConstraintCheckViewModel != null)
                    {
                        List<Exposure> exposureList = mParentModel.GetExposureChecked(false, false);
                        mConstraintCheckViewModel.ExposureConstraintList = exposureList;
                        mPathDetailViewModel.PathDetailList = exposureList;
                    }
                    else
                    {
                        mParentModel.CheckExposure(true, false, false, DateTime.Today);
                    }
                    // code for updating constraints
                }
                PathList = pathHash.Values.Where(a => a.Source == SelectedConstraintPathValue.Constraint && a.Sink == SelectedConstraintPathValue.Contingency).ToList();
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Deletes the selected paths.
        /// </summary>
        private void DeleteSelectedPaths()
        {
            try
            {
                if (PathSelectedList != null)
                {
                    PathSelectedList.ForEach(a =>
                    {
                        DBAccess.DeletePath(a.Market, a.BidId, a.PortfolioDate, a.IsUptos);
                    });
                }
            }
            catch
            {
            }
            Retrieve(null, "");
        }

        #endregion


        internal void UpdatePath(Path editedRow, string column, string price)
        {
            if (editedRow != null)
            {
                try
                {
                    string savedName = editedRow.IsUptos ? null : editedRow.Portfolio;
                    DateTime? start = editedRow.PortfolioDate;
                    PricingNode sourceNode = DBAccess.GetNodeFromName(editedRow.Source, editedRow.Market);
                    PricingNode sinkNode = DBAccess.GetNodeFromName(editedRow.Sink, editedRow.Market);
                    DBAccess.DeletePath(editedRow.Market, editedRow.BidId, start.Value, editedRow.IsUptos);
                    List<Bid> bidList = new List<Bid>();
                    string[] hourTokens = editedRow.AnalysisType.Split('.');
                    double mw = editedRow.MW;
                    double price1 = editedRow.Price;
                    if (column.Equals("MW"))
                    {
                        mw = Convert.ToDouble(price);
                    }
                    else if (column.Equals("Price"))
                    {
                        price1 = Convert.ToDouble(price);
                    }
                    else if (column.Equals("AnalysisType"))
                    {
                        hourTokens = price.Split('.');
                    }
                    foreach (string hour in hourTokens)
                    {
                        Bid bid = new Bid();
                        bid.Source = sourceNode.NodeKey;
                        bid.Sink = sinkNode == null ? -1 : sinkNode.NodeKey;
                        bid.MW = mw;
                        bid.Price = price1;
                        bid.Market = editedRow.Market;
                        bid.MarketDateTime = start.Value.AddHours(Int16.Parse(hour));
                        bid.PortfolioKey = editedRow.PortfolioKey;
                        bid.BidId = editedRow.BidId;
                        bid.IsUptos = editedRow.IsUptos;
                        bid.Status = editedRow.Status;
                        bid.Comments = editedRow.Comments;
                        bidList.Add(bid);
                    }
                    DBAccess.SaveBids(bidList, mUser);
                    Retrieve(editedRow, savedName);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
