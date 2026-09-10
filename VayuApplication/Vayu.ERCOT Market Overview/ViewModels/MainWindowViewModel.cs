using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using Vayu.ERCOT_Market_Overview.Model;
using Vayu.NodePriceLibrary;

namespace Vayu.ERCOT_Market_Overview.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Declaration

        private readonly IDataService _dataService;
        List<string> mNodeListAll = new List<string>();
        public DelegateCommand RetrieveOldCommand { private set; get; }
        public DelegateCommand RunExportCSVCommand { private set; get; }
        public DelegateCommand AddNodeCommand { private set; get; }
        public DelegateCommand RemoveNodeCommand { private set; get; }
        public DelegateCommand RemoveAllNodeCommand { private set; get; }


        private string mUser = Environment.UserName;


        public List<DAVolumes> FinalList;
        private List<DAVolumes> mPathList;
        public List<DAVolumes> PathList
        {
            get
            {
                return mPathList;
            }
            set
            {
                mPathList = value;
                RaisePropertyChanged("PathList");
            }
        }
        private DateTime mStartDate;
        public DateTime StartDate
        {
            get
            {
                return mStartDate;
            }
            set
            {
                mStartDate = value.Date;
                // SetAccountList();
                //ResetValues(true);
                RaisePropertyChanged("StartDate");
            }
        }


        private DateTime mEndDate;
        public DateTime EndDate
        {
            get
            {
                return mEndDate;
            }
            set
            {
                mEndDate = value.Date;
                // SetAccountList();
                //ResetValues(true);
                RaisePropertyChanged("EndDate");
            }
        }
        private bool mDateRangeCheckBoxChecked;
        /// <summary>
        /// Gets or sets a value indicating whether [filter day comparison total checked].
        /// </summary>
        /// <value>
        /// <c>true</c> if [filter day comparison total checked]; otherwise, <c>false</c>.
        /// </value>
        public bool DateRangeCheckBoxChecked
        {
            get
            {
                return mDateRangeCheckBoxChecked;
            }
            set
            {
                mDateRangeCheckBoxChecked = value;
                RaisePropertyChanged("DateRangeCheckBoxChecked");

            }
        }
        private List<string> mNodeList;
        public List<string> NodeList
        {
            get
            {
                return mNodeList;
            }
            set
            {
                mNodeList = value;
                RaisePropertyChanged("NodeList");
            }
        }

        private List<string> mFamilyList;
        /// <summary>
        /// Gets or sets the family list.
        /// </summary>
        /// <value>
        /// The family list.
        /// </value>
        public List<string> FamilyList
        {
            get
            {
                return mFamilyList;
            }
            set
            {
                mFamilyList = value;
                RaisePropertyChanged("FamilyList");
            }
        }

        private string mSelectedNodeList;
        /// <summary>
        /// Gets or sets the family list.
        /// </summary>
        /// <value>
        /// The family list.
        /// </value>
        public string SelectedNodeList
        {
            get
            {
                return mSelectedNodeList;
            }
            set
            {
                mSelectedNodeList = value;
                RaisePropertyChanged("SelectedNodeList");
            }
        }

        private string mNodeDataSelected;
        /// <summary>
        /// Gets or sets the family list.
        /// </summary>
        /// <value>
        /// The family list.
        /// </value>
        public string NodeDataSelected
        {
            get
            {
                return mNodeDataSelected;
            }
            set
            {
                mNodeDataSelected = value;
                RaisePropertyChanged("NodeDataSelected");
            }
        }


        private double? mTotalMW;
        public double? TotalMW
        {
            get
            {
                return mTotalMW;
            }
            set
            {
                mTotalMW = value;
                RaisePropertyChanged("TotalMW");
            }
        }
        private double? mTotalDA;

        public double? TotalDA
        {
            get
            {
                return mTotalDA;
            }
            set
            {
                mTotalDA = value;
                RaisePropertyChanged("TotalDA");
            }
        }
        private double? mTotalRT;

        public double? TotalRT
        {
            get
            {
                return mTotalRT;
            }
            set
            {
                mTotalRT = value;
                RaisePropertyChanged("TotalRT");
            }
        }

        private double? mTotalDART;

        public double? TotalDART
        {
            get
            {
                return mTotalDART;
            }
            set
            {
                mTotalDART = value;
                RaisePropertyChanged("TotalDART");
            }
        }
        private double? mDAPerMW;

        public double? DAPerMW
        {
            get
            {
                return mDAPerMW;
            }
            set
            {
                mDAPerMW = value;
                RaisePropertyChanged("DAPerMW");
            }
        }
        private double? mRTPerMW;

        public double? RTPerMW
        {
            get
            {
                return mRTPerMW;
            }
            set
            {
                mRTPerMW = value;
                RaisePropertyChanged("RTPerMW");
            }
        }
        private double? mDARTPerMW;

        public double? DARTPerMW
        {
            get
            {
                return mDARTPerMW;
            }
            set
            {
                mDARTPerMW = value;
                RaisePropertyChanged("DARTPerMW");
            }
        }


        #endregion

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            RetrieveOldCommand = new DelegateCommand(RetrieveOld);

            RunExportCSVCommand = new DelegateCommand(ExportToCSVCommand);
            AddNodeCommand = new DelegateCommand(AddNodeButton);
            RemoveNodeCommand = new DelegateCommand(RemoveNodeButton);
            RemoveAllNodeCommand = new DelegateCommand(RemoveAllNodeButton);
            FamilyList = DBAccess.GetAllPath();

        }
        public void SetFamilies()
        {
            FamilyList = null;
            FamilyList = DBAccess.GetAllPath();
        }

        public void AddNodeButton()
        {
            try
            {
                if (SelectedNodeList != null)
                {
                    if (!mNodeListAll.Contains(SelectedNodeList))
                    {
                        NodeList = null;
                        mNodeListAll.Add(SelectedNodeList);
                        NodeList = mNodeListAll;
                    }
                    SelectedNodeList = null;
                }

            }
            catch (Exception e)
            {

            }
        }
        public void RemoveNodeButton()
        {
            try
            {
                if (mNodeListAll.Contains(NodeDataSelected))
                {

                    mNodeListAll.Remove(NodeDataSelected);
                    NodeList = null;
                    NodeList = mNodeListAll;
                    PathList = null;
                    TotalMW = null;
                    TotalDA = null;
                    TotalRT = null;
                    TotalDART = null;
                    DAPerMW = null;
                    RTPerMW = null;
                    DARTPerMW = null;


                }

            }
            catch (Exception e)
            {

            }
        }
        public void RemoveAllNodeButton()
        {
            try
            {

                mNodeListAll = null;
                NodeList = mNodeListAll;
                PathList = null;
                TotalMW = null;
                TotalDA = null;
                TotalRT = null;
                TotalDART = null;
                DAPerMW = null;
                RTPerMW = null;
                DARTPerMW = null;
            }
            catch (Exception e)
            {

            }
        }
        private void RetrieveOld()
        {
            if (DateRangeCheckBoxChecked == false)
            {

                Dictionary<int, Node> mDAHash = new Dictionary<int, Node>();
                Dictionary<int, Node> mRTHash = new Dictionary<int, Node>();
                List<DAVolumes> listDAVolumes = _dataService.GetDAVolumes(StartDate);

                
                List<string> validPTPs = _dataService.GetValidPTPList(StartDate);

                // Get distinct STLPNTs from retrieved volumes
                var retrievedSTLPNTs = listDAVolumes.Select(v => v.STLPNT).Distinct();

                // Compare and get missing STLPNTs
                var invalidSTLPNTs = retrievedSTLPNTs.Where(stlpnt => !validPTPs.Contains(stlpnt)).ToList();

                if (invalidSTLPNTs.Any())
                {
                    string message = "\n" +
                                     string.Join(", ", invalidSTLPNTs);
                    System.Windows.MessageBox.Show(message, "New nodes has been found ", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                //if (invalidSTLPNTs.Any())
                //{
                //    string message = "The following Nodes are not found in the ErcotValidPTP table:\n" +
                //                     string.Join("\n", invalidSTLPNTs);
                //    System.Windows.MessageBox.Show(message, "This Nodes Are Not Available", MessageBoxButton.OK, MessageBoxImage.Warning);
                //}

                List<DataItem> rtEnergyList = _dataService.GetRTEnergy(StartDate);
                List<DataItem> daEnergyList = _dataService.GetDAEnergy(StartDate);
                List<SourceSinkData> SourceSinkList = new List<SourceSinkData>();


                Dictionary<int, DataItem> rtEnergyDic = new Dictionary<int, DataItem>();
                Dictionary<int, DataItem> daEnergyDic = new Dictionary<int, DataItem>();
                foreach (DataItem dataItem in rtEnergyList)
                {
                    if (!rtEnergyDic.ContainsKey(dataItem.EnergyHourEnding))
                    {
                        rtEnergyDic.Add(dataItem.EnergyHourEnding, dataItem);
                    }
                }
                foreach (DataItem dataItem in daEnergyList)
                {
                    if (!daEnergyDic.ContainsKey(dataItem.EnergyHourEnding))
                    {
                        daEnergyDic.Add(dataItem.EnergyHourEnding, dataItem);
                    }
                }
                foreach (DAVolumes item in listDAVolumes)
                {
                    SourceSinkData sourcesSinkData = new SourceSinkData();
                    PricingNode priceNode = new PricingNode();
                    priceNode = DBAccess.GetNodeFromName(item.STLPNT, 9);
                    sourcesSinkData.Source = priceNode;
                    SourceSinkList.Add(sourcesSinkData);
                }
                //SourceSinkList=
                List<Node> rtList = new List<Node>();
                List<Node> daList = new List<Node>();
                List<PricingNode> sourceSinkNodeList = new List<PricingNode>();
                foreach (SourceSinkData sourceSinkData in SourceSinkList)
                {
                    if (!sourceSinkNodeList.Contains(sourceSinkData.Source))
                    {
                        sourceSinkNodeList.Add(sourceSinkData.Source);
                    }
                    if (sourceSinkData.Sink != null && !sourceSinkNodeList.Contains(sourceSinkData.Sink))
                    {
                        sourceSinkNodeList.Add(sourceSinkData.Sink);
                    }
                }
                foreach (PricingNode priceNode in sourceSinkNodeList)
                {
                    Node node = new Node();
                    node.Market = priceNode.MarketKey;
                    node.NodeId = priceNode.NodeKey;
                    node.NodeName = priceNode.NodeName;
                    node.PNodeId = priceNode.ExternalNodeId;
                    rtList.Add(node);
                    Node node1 = new Node();
                    node1.Market = node.Market;
                    node1.NodeId = node.NodeId;
                    node1.NodeName = node.NodeName;
                    node1.PNodeId = node.PNodeId;
                    daList.Add(node1);
                }




                DARTNode.GetDART(rtList, daList, StartDate, 1, false, true);
                mDAHash = new Dictionary<int, Node>();
                foreach (Node node in daList)
                {
                    if (!mDAHash.ContainsKey(node.NodeId))
                    {
                        mDAHash.Add(node.NodeId, node);
                    }
                }

                mRTHash = new Dictionary<int, Node>();
                foreach (Node node in rtList)
                {
                    if (!mRTHash.ContainsKey(node.NodeId))
                    {
                        mRTHash.Add(node.NodeId, node);
                    }
                }

                foreach (DAVolumes sourceSinkData in listDAVolumes)
                {
                    Node daSourceNode = mDAHash[sourceSinkData.NodeKey];
                    DataItem rtEnergy = null;
                    DataItem daEnergy = null;
                    if (rtEnergyDic.ContainsKey(sourceSinkData.HourEnding))
                    {
                        rtEnergy = rtEnergyDic[sourceSinkData.HourEnding];
                    }
                    if (daEnergyDic.ContainsKey(sourceSinkData.HourEnding))
                    {
                        daEnergy = daEnergyDic[sourceSinkData.HourEnding];
                    }

                    if (daSourceNode.LmpTimePriceList[(sourceSinkData.HourEnding - 1)].Lmp != null)
                    {
                        sourceSinkData.DA = Math.Round(daSourceNode.LmpTimePriceList[sourceSinkData.HourEnding - 1].Lmp.Price, 2);
                    }
                    Node rtSourceNode = mRTHash[sourceSinkData.NodeKey];
                    if (rtSourceNode.LmpTimePriceList[sourceSinkData.HourEnding - 1].Lmp != null)
                    {
                        sourceSinkData.RT = Math.Round(rtSourceNode.LmpTimePriceList[sourceSinkData.HourEnding - 1].Lmp.Price, 2);
                    }
                    if (rtEnergy != null)
                    {
                        sourceSinkData.RT_Energy = Math.Round(rtEnergy.SystemLambda, 2);
                    }
                    else
                    {
                        sourceSinkData.RT_Energy = double.NaN;

                    }
                    if (daEnergy != null)
                    {
                        sourceSinkData.DA_Energy = Math.Round(daEnergy.SystemLambda, 2);
                    }
                    else
                    {
                        sourceSinkData.DA_Energy = double.NaN;

                    }

                    sourceSinkData.RT_Cong = Math.Round(sourceSinkData.RT - sourceSinkData.RT_Energy, 2);

                    sourceSinkData.DA_Cong = Math.Round(sourceSinkData.DA - sourceSinkData.DA_Energy, 2);

                    sourceSinkData.DART_Cong = Math.Round(sourceSinkData.RT_Cong - sourceSinkData.DA_Cong, 2);

                    sourceSinkData.NetVolume = Math.Round(sourceSinkData.TOTAL_PTP_OBL_AWARDED_SINK - sourceSinkData.TOTAL_PTP_OBL_AWARDED_SOURCE, 2);
                    sourceSinkData.DART = Math.Round(sourceSinkData.RT - sourceSinkData.DA, 2);
                    sourceSinkData.TotalDART = Math.Round(sourceSinkData.NetVolume * sourceSinkData.DART, 2);
                    sourceSinkData.DARTSource = Math.Round(sourceSinkData.TOTAL_PTP_OBL_AWARDED_SOURCE * sourceSinkData.DART, 2);
                    sourceSinkData.DARTSink = Math.Round(sourceSinkData.TOTAL_PTP_OBL_AWARDED_SINK * sourceSinkData.DART, 2);
                    sourceSinkData.SUM = Math.Round(sourceSinkData.DARTSource + sourceSinkData.DARTSink, 2);
                    sourceSinkData.DA_Node = Math.Round(sourceSinkData.NetVolume * sourceSinkData.DA, 2);
                    sourceSinkData.RT_Node = Math.Round(sourceSinkData.NetVolume * sourceSinkData.RT, 2);
                    sourceSinkData.DART_Node = Math.Round(sourceSinkData.RT_Node - sourceSinkData.DA_Node, 2);

                }
                if (listDAVolumes.Count > 0)
                {
                    PathList = listDAVolumes;

                    TotalMW = Math.Round(listDAVolumes.Sum(x => x.TOTAL_PTP_OBL_AWARDED_SOURCE), 2);
                    TotalDA = Math.Round(listDAVolumes.Sum(x => x.DA_Node), 2);
                    TotalRT = Math.Round(listDAVolumes.Sum(x => Double.IsNaN(x.RT_Node) ? 0 : x.RT_Node), 2);
                    double totaldart = Math.Round(listDAVolumes.Sum(x => Double.IsNaN(x.DART_Node) ? 0 : x.DART_Node), 2);
                    double mw = Math.Round(listDAVolumes.Sum(x => x.TOTAL_PTP_OBL_AWARDED_SOURCE), 2);
                    double da = Math.Round(listDAVolumes.Sum(x => Double.IsNaN(x.DA_Node) ? 0 : x.DA_Node), 2);
                    double rt = Math.Round(listDAVolumes.Sum(x => Double.IsNaN(x.RT_Node) ? 0 : x.RT_Node), 2);
                    double todart = totaldart / mw;
                    double tort = rt / mw;
                    double toda = da / mw;
                    TotalDART = Math.Round(listDAVolumes.Sum(x => Double.IsNaN(x.DART_Node) ? 0 : x.DART_Node), 2);
                    DAPerMW = Math.Round(toda, 2);
                    RTPerMW = Math.Round(tort, 2);
                    DARTPerMW = Math.Round(todart, 2);


                }


            }
            if (DateRangeCheckBoxChecked == true)
            {
                if (NodeList == null)
                {
                    System.Windows.MessageBox.Show("Please Select Node");
                }
                if (NodeList != null)
                {
                    Dictionary<int, Node> mDAHash = new Dictionary<int, Node>();
                    Dictionary<int, Node> mRTHash = new Dictionary<int, Node>();
                    List<BetweenDataItem> daEnergyList = _dataService.GetDAEnergyBetween(StartDate, EndDate);
                    List<BetweenDataItem> rtEnergyList = _dataService.GetRTEnergyBetween(StartDate, EndDate);
                    List<DAVolumes> listDAVolumes = new List<DAVolumes>(); ;
                    List<DAVolumes> listDAVolumes2 = null;
                    List<SourceSinkData> SourceSinkList = new List<SourceSinkData>();
                    Dictionary<DateTime, BetweenDataItem> daEnergyDic = new Dictionary<DateTime, BetweenDataItem>();
                    Dictionary<DateTime, BetweenDataItem> rtEnergyDic = new Dictionary<DateTime, BetweenDataItem>();
                    foreach (BetweenDataItem dataItem in daEnergyList)
                    {
                        if (!daEnergyDic.ContainsKey(dataItem.daDate))
                        {
                            daEnergyDic.Add(dataItem.daDate, dataItem);
                        }
                    }
                    foreach (BetweenDataItem dataItem in rtEnergyList)
                    {
                        if (!rtEnergyDic.ContainsKey(dataItem.daDate))
                        {
                            rtEnergyDic.Add(dataItem.daDate, dataItem);
                        }
                    }
                    for (int i = 0; i < NodeList.Count; i++)
                    {
                        string Stlpnt = NodeList[i];
                        listDAVolumes2 = _dataService.GetDAVolumesBetween(StartDate, EndDate, Stlpnt);
                        listDAVolumes.AddRange(listDAVolumes2);

                    }

                    foreach (DAVolumes item in listDAVolumes)
                    {
                        SourceSinkData sourcesSinkData = new SourceSinkData();
                        PricingNode priceNode = new PricingNode();
                        priceNode = DBAccess.GetNodeFromName(item.STLPNT, 9);
                        sourcesSinkData.Source = priceNode;
                        SourceSinkList.Add(sourcesSinkData);
                    }
                    //SourceSinkList=
                    List<Node> rtList = new List<Node>();
                    List<Node> daList = new List<Node>();
                    List<PricingNode> sourceSinkNodeList = new List<PricingNode>();
                    foreach (SourceSinkData sourceSinkData in SourceSinkList)
                    {
                        if (!sourceSinkNodeList.Contains(sourceSinkData.Source))
                        {
                            sourceSinkNodeList.Add(sourceSinkData.Source);
                        }
                        if (sourceSinkData.Sink != null && !sourceSinkNodeList.Contains(sourceSinkData.Sink))
                        {
                            sourceSinkNodeList.Add(sourceSinkData.Sink);
                        }
                    }
                    foreach (PricingNode priceNode in sourceSinkNodeList)
                    {
                        Node node = new Node();
                        node.Market = priceNode.MarketKey;
                        node.NodeId = priceNode.NodeKey;
                        node.NodeName = priceNode.NodeName;
                        node.PNodeId = priceNode.ExternalNodeId;
                        rtList.Add(node);
                        Node node1 = new Node();
                        node1.Market = node.Market;
                        node1.NodeId = node.NodeId;
                        node1.NodeName = node.NodeName;
                        node1.PNodeId = node.PNodeId;
                        daList.Add(node1);
                    }
                    DARTNode.GetDART(rtList, daList, StartDate, 1, false, true);
                    mDAHash = new Dictionary<int, Node>();
                    foreach (Node node in daList)
                    {
                        if (!mDAHash.ContainsKey(node.NodeId))
                        {
                            mDAHash.Add(node.NodeId, node);
                        }
                    }

                    mRTHash = new Dictionary<int, Node>();
                    foreach (Node node in rtList)
                    {
                        if (!mRTHash.ContainsKey(node.NodeId))
                        {
                            mRTHash.Add(node.NodeId, node);
                        }
                    }

                    foreach (DAVolumes sourceSinkData in listDAVolumes)
                    {
                        DateTime tempDate = sourceSinkData.DeliveryYDate;
                        if (sourceSinkData.HourEnding != 24)
                        {
                            tempDate = tempDate.AddHours(sourceSinkData.HourEnding);
                        }
                        BetweenDataItem daEnergy = null;
                        BetweenDataItem rtEnergy = null;
                        if (daEnergyDic.ContainsKey(tempDate))
                        {
                            daEnergy = daEnergyDic[tempDate];
                        }
                        if (rtEnergyDic.ContainsKey(tempDate))
                        {
                            rtEnergy = rtEnergyDic[tempDate];
                        }
                        Node daSourceNode = mDAHash[sourceSinkData.NodeKey];
                        if (daSourceNode.LmpTimePriceList[(sourceSinkData.HourEnding - 1)].Lmp != null)
                        {
                            sourceSinkData.DA = Math.Round(daSourceNode.LmpTimePriceList[sourceSinkData.HourEnding - 1].Lmp.Price, 2);
                        }
                        Node rtSourceNode = mRTHash[sourceSinkData.NodeKey];
                        if (rtSourceNode.LmpTimePriceList[sourceSinkData.HourEnding - 1].Lmp != null)
                        {
                            sourceSinkData.RT = Math.Round(rtSourceNode.LmpTimePriceList[sourceSinkData.HourEnding - 1].Lmp.Price, 2);
                        }
                        if (rtEnergy != null)
                        {
                            sourceSinkData.RT_Energy = Math.Round(rtEnergy.SystemLambda, 2);
                        }
                        else
                        {
                            sourceSinkData.RT_Energy = double.NaN;

                        }
                        if (daEnergy != null)
                        {
                            sourceSinkData.DA_Energy = Math.Round(daEnergy.SystemLambda, 2);
                        }
                        else
                        {
                            sourceSinkData.DA_Energy = double.NaN;

                        }
                        sourceSinkData.RT_Cong = Math.Round(sourceSinkData.RT - sourceSinkData.RT_Energy, 2);
                        sourceSinkData.DA_Cong = Math.Round(sourceSinkData.DA - sourceSinkData.DA_Energy, 2);
                        sourceSinkData.DART_Cong = Math.Round(sourceSinkData.RT_Cong - sourceSinkData.DA_Cong, 2);

                        sourceSinkData.NetVolume = Math.Round(sourceSinkData.TOTAL_PTP_OBL_AWARDED_SINK - sourceSinkData.TOTAL_PTP_OBL_AWARDED_SOURCE, 2);
                        sourceSinkData.DART = Math.Round(sourceSinkData.RT - sourceSinkData.DA, 2);
                        sourceSinkData.TotalDART = Math.Round(sourceSinkData.NetVolume * sourceSinkData.DART, 2);
                        sourceSinkData.DARTSource = Math.Round(sourceSinkData.TOTAL_PTP_OBL_AWARDED_SOURCE * sourceSinkData.DART, 2);
                        sourceSinkData.DARTSink = Math.Round(sourceSinkData.TOTAL_PTP_OBL_AWARDED_SINK * sourceSinkData.DART, 2);
                        sourceSinkData.SUM = Math.Round(sourceSinkData.DARTSource + sourceSinkData.DARTSink, 2);
                        sourceSinkData.DA_Node = Math.Round(sourceSinkData.NetVolume * sourceSinkData.DA, 2);
                        sourceSinkData.RT_Node = Math.Round(sourceSinkData.NetVolume * sourceSinkData.RT, 2);
                        sourceSinkData.DART_Node = Math.Round(sourceSinkData.RT_Node - sourceSinkData.DA_Node, 2);

                    }
                    if (listDAVolumes.Count > 0)
                    {
                        PathList = listDAVolumes;
                        TotalMW = Math.Round(listDAVolumes.Sum(x => x.TOTAL_PTP_OBL_AWARDED_SOURCE), 2);
                        TotalDA = Math.Round(listDAVolumes.Sum(x => x.DA_Node), 2);
                        TotalRT = Math.Round(listDAVolumes.Sum(x => x.RT_Node), 2);
                        double totaldart = Math.Round(listDAVolumes.Sum(x => x.DART_Node), 2);
                        double mw = Math.Round(listDAVolumes.Sum(x => x.TOTAL_PTP_OBL_AWARDED_SOURCE), 2);
                        double da = Math.Round(listDAVolumes.Sum(x => x.DA_Node), 2);
                        double rt = Math.Round(listDAVolumes.Sum(x => x.RT_Node), 2);
                        double todart = totaldart / mw;
                        double tort = rt / mw;
                        double toda = da / mw;
                        TotalDART = Math.Round(listDAVolumes.Sum(x => x.DART_Node), 2);
                        DAPerMW = Math.Round(toda, 2);
                        RTPerMW = Math.Round(tort, 2);
                        DARTPerMW = Math.Round(todart, 2);
                    }
                }
            }
        }



        public void ExportToCSVCommand()
        {
            Task.Factory.StartNew(() => { ExportToCSVThreadedOld(); });
        }

        private void ExportToCSVThreadedOld()
        {
            try
            {
                if (PathList == null || PathList.Count == 0)
                {
                    System.Windows.MessageBox.Show("No data to export to");
                    return;
                }
                SaveFileDialog dialog = new SaveFileDialog { Filter = "csv|CSV" };
                dialog.FileName = "DAVolumes_" + StartDate.ToString("yyyy-MM-dd") + ".csv";
                if ((bool)dialog.ShowDialog())
                {
                    if (dialog.FileName != "")
                    {
                        if (PathList != null && PathList.Count > 0)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (PropertyInfo item in PathList[0].GetType().GetProperties())
                            {

                                if (item.Name == "DeliveryYDate" || item.Name == "HourEnding" || item.Name == "STLPNT" || item.Name == "TOTAL_PTP_OBL_AWARDED_SOURCE"
                                    || item.Name == "TOTAL_PTP_OBL_AWARDED_SINK" || item.Name == "NetVolume" ||
item.Name == "RT" || item.Name == "DA" || item.Name == "DART" || item.Name == "RT_Node" || item.Name == "DA_Node" || item.Name == "DART_Node" || item.Name == "RT_Energy" || item.Name == "DA_Energy" || item.Name == "RT_Cong" || item.Name == "DA_Cong" || item.Name == "DART_Cong" || item.Name == "Zone" || item.Name == "Fuelsource")
                                {
                                    builder.Append(item.Name + ",");
                                }
                            }
                            builder.AppendLine();
                            foreach (DAVolumes item in PathList)
                            {
                                foreach (PropertyInfo propItem in item.GetType().GetProperties())
                                {
                                    if (propItem.Name == "DeliveryYDate" || propItem.Name == "HourEnding" || propItem.Name == "STLPNT" || propItem.Name == "TOTAL_PTP_OBL_AWARDED_SOURCE"
                                        || propItem.Name == "TOTAL_PTP_OBL_AWARDED_SINK" || propItem.Name == "NetVolume" ||
    propItem.Name == "RT" || propItem.Name == "DA" || propItem.Name == "DART" || propItem.Name == "RT_Node" || propItem.Name == "DA_Node" || propItem.Name == "DART_Node" || propItem.Name == "RT_Energy" || propItem.Name == "DA_Energy" || propItem.Name == "RT_Cong" || propItem.Name == "DA_Cong" || propItem.Name == "DART_Cong" || propItem.Name == "Zone" || propItem.Name == "Fuelsource")
                                    {
                                        builder.Append(propItem.GetValue(item) + ",");
                                    }
                                }
                                builder.AppendLine();
                            }
                            if (builder.Length > 0)
                            {
                                using (TextWriter str = new StreamWriter(dialog.FileName, false))
                                {
                                    str.Write(builder.ToString());
                                    str.Flush();
                                    str.Close();
                                    str.Dispose();
                                }
                                if (File.Exists(dialog.FileName))
                                {
                                    System.Windows.MessageBox.Show("Successfully saved the file " + dialog.FileName);
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Couldnot save the file");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
