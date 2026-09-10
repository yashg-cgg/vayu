using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vayu.CongestionVolatilityIndex.Model;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;

namespace Vayu.CongestionVolatilityIndex.ViewModels
{
    #region commned code
    //public class MainWindowViewModel : BindableBase
    //{

    //    private readonly IDataService _dataService;
    //    List<CongestionVolatility> CongestionVolatilityList = new List<CongestionVolatility>();
    //    private Dictionary<string, List<CongestionVolatility>> tempCongestionList;
    //    List<CongestionVolatility> tempdictdata = null;
    //    private Dictionary<string, OxyColor> mOxyColorList;
    //    List<string> mUptosPathList;
    //    private Dictionary<string, Vayu.DBLibrary.NodeDetail> mNodeHash = new Dictionary<string, Vayu.DBLibrary.NodeDetail>();
    //    private Dictionary<int, List<string>> mNodeTypeHash = new Dictionary<int, List<string>>();
    //    private Dictionary<int, List<string>> mZoneHash = new Dictionary<int, List<string>>();
    //    List<DateTime> dateList = new List<DateTime>();
    //    DateTime mdate = new DateTime();
    //    public DelegateCommand RefreshCommand { get; private set; }
    //    private bool hourlyChecked;

    //    public bool HourlyChecked
    //    {
    //        get { return hourlyChecked; }
    //        set
    //        {
    //            hourlyChecked = value;
    //            RaisePropertyChanged("HourlyChecked");
    //        }
    //    }

    //    private PlotModel plotDataModel;

    //    public PlotModel PlotDataModel
    //    {
    //        get { return plotDataModel; }
    //        set
    //        {
    //            plotDataModel = value; RaisePropertyChanged("PlotDataModel");
    //        }
    //    }

    //    private bool fourhourlyChecked;

    //    public bool FourhourlyChecked
    //    {
    //        get { return fourhourlyChecked; }
    //        set
    //        {
    //            fourhourlyChecked = value;
    //            RaisePropertyChanged("FourhourlyChecked");
    //        }
    //    }
    //    /// <summary>
    //    /// Set DailyChecked
    //    /// </summary>
    //    private bool dailyChecked;
    //    /// <summary>
    //    /// Get and Set DailyChecked
    //    /// </summary>
    //    public bool DailyChecked
    //    {
    //        get { return dailyChecked; }
    //        set { dailyChecked = value; RaisePropertyChanged("DailyChecked"); }
    //    }

    //    private DateTime fromSelectedDate = DateTime.Now.AddDays(-2);

    //    public DateTime FromSelectedDate
    //    {
    //        get { return fromSelectedDate; }
    //        set { fromSelectedDate = value; RaisePropertyChanged("FromSelectedDate"); }
    //    }
    //    public string[] MarketList { get; set; }
    //    private DateTime endSelectedDate;

    //    public DateTime EndSelectedDate
    //    {
    //        get { return endSelectedDate; }
    //        set { endSelectedDate = value; RaisePropertyChanged("EndSelectedDate"); }
    //    }
    //    private string marketSelected;

    //    public string MarketSelected
    //    {
    //        get { return marketSelected; }
    //        set { marketSelected = value; RaisePropertyChanged("MarketSelected"); }
    //    }

    //    private List<CongestionVolatility> congestionList;

    //    public List<CongestionVolatility> CongestionList
    //    {
    //        get { return congestionList; }
    //        set { congestionList = value; RaisePropertyChanged("CongestionList"); }
    //    }

    //    public MainWindowViewModel(IDataService dataService)
    //    {
    //        _dataService = dataService;
    //        MarketList = new string[] { "ERCOT" };
    //        MarketSelected = MarketList.FirstOrDefault();
    //        //FromSelectedDate = DateTime.Now.AddDays(-1);
    //        //EndSelectedDate = FromSelectedDate;
    //        EndSelectedDate = DateTime.Parse(DateTime.Now.AddDays(-1).ToString("MM-dd-yyyy"));
    //        RefreshCommand = new DelegateCommand(() => Refresh());
    //        HourlyChecked = true;
    //        // FillSourceSinkHash();
    //        Refresh();
    //    }
    //    private void Refresh()
    //    {
    //        dateList = new List<DateTime>();
    //        DateTime date = DateTime.Parse(FromSelectedDate.ToString("MM-dd-yyyy"));
    //        while (date <= EndSelectedDate)
    //        {
    //            dateList.Add(date);
    //            date = date.AddDays(1);
    //        }
    //        mdate = DateTime.Parse(FromSelectedDate.ToString("MM-dd-yyyy"));

    //        tempCongestionList = new Dictionary<string, List<CongestionVolatility>>();
    //        GetConstraints(GetMarketKey(), "RT");
    //        tempCongestionList.Add("RT", tempdictdata);
    //        GetConstraints(GetMarketKey(), "DA");
    //        tempCongestionList.Add("DA", tempdictdata);
    //        if (HourlyChecked)
    //        {
    //            SetTable();
    //            tempCongestionList.Add("LMP", tempdictdata);
    //        }
    //        FixCongestionData();
    //    }

    //    private void GetConstraints(int p, string Type)
    //    {
    //        CongestionVolatilityList = new List<CongestionVolatility>();
    //        _dataService.GetCongestionVolatility((a, e) =>
    //        {
    //            if (e == null)
    //            {
    //                tempdictdata = a;
    //            }
    //            else
    //            {
    //                System.Windows.MessageBox.Show(e.Message);
    //            }

    //        }, p, mdate, EndSelectedDate, Type, HourlyChecked, FourhourlyChecked, DailyChecked);
    //    }


    //    private void SetTable()
    //    {
    //        try
    //        {
    //            List<string> UptosNodeList = _dataService.GetUptosNode(GetMarketKey());
    //            tempdictdata = new List<CongestionVolatility>();
    //            string name = null;
    //            string sink = null;
    //            mNodeHash = DBAccess.GetAllNodes(GetMarketKey(), mNodeTypeHash, mZoneHash);
    //            DARTNode.GetAllDartsAndLMP(GetMarketKey(), DateTime.Parse(FromSelectedDate.ToString("MM-dd-yyyy")), EndSelectedDate);
    //            List<CongestionVolatility> templist = new List<CongestionVolatility>();
    //            List<string> nodeNameList = mNodeHash.Keys.ToList<string>();
    //            List<string> nameList = new List<string>();
    //            if (GetMarketKey() == 1)
    //                nameList = nodeNameList;//mUptosPathList;
    //            else
    //                nameList = nodeNameList;
    //            foreach (DateTime date in dateList)
    //            {
    //                foreach (var item in nameList)
    //                {
    //                    string[] tokens = item.Split('?');
    //                    name = tokens[0];
    //                    //if (GetMarketKey() == 1)
    //                    //    sink = tokens[1];

    //                    CongestionVolatility nodeData = new CongestionVolatility();

    //                    for (int i = 0; i < 24; i++)
    //                    {
    //                        double rtPrice = double.NaN;

    //                        if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
    //                        {
    //                            continue;
    //                        }
    //                        string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
    //                        string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
    //                        if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Price))
    //                        {
    //                            if (double.IsNaN(rtPrice))
    //                            {
    //                                if (UptosNodeList.Contains(name))
    //                                {
    //                                    rtPrice = DARTNode.sRTLmpHash[sourceKey].Price;
    //                                    #region AllNodes
    //                                    //if (sinkKey != null)
    //                                    //{
    //                                    //    if (sinkKey != null)
    //                                    //    {
    //                                    //        double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
    //                                    //        rtPrice = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
    //                                    //    }
    //                                    //}
    //                                    #endregion AllNodes
    //                                    nodeData = SetNodeData(i, nodeData, rtPrice);
    //                                    nodeData.GetType().GetProperty("HE" + (i + 1)).SetValue(nodeData, null, null);
    //                                    nodeData.ConstraintDate = date;
    //                                    templist.Add(nodeData);
    //                                }
    //                            }
    //                        }
    //                        else
    //                            continue;
    //                    }


    //                }

    //            }
    //            SetLmpHour(templist, dateList);
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }

    //    private CongestionVolatility SetNodeData(int i, CongestionVolatility nodeData, double? rt)
    //    {
    //        if (i == 0)
    //        {
    //            nodeData.HE = rt;
    //        }
    //        if (i == 1)
    //        {
    //            nodeData.HE1 = rt;
    //        }
    //        if (i == 2)
    //        {
    //            nodeData.HE2 = rt;
    //        }
    //        if (i == 3)
    //        {
    //            nodeData.HE3 = rt;
    //        }
    //        if (i == 4)
    //        {
    //            nodeData.HE4 = rt;
    //        }
    //        if (i == 5)
    //        {
    //            nodeData.HE5 = rt;
    //        }
    //        if (i == 6)
    //        {
    //            nodeData.HE6 = rt;
    //        }
    //        if (i == 7)
    //        {
    //            nodeData.HE7 = rt;
    //        }
    //        if (i == 8)
    //        {
    //            nodeData.HE8 = rt;
    //        }
    //        if (i == 9)
    //        {
    //            nodeData.HE9 = rt;
    //        }
    //        if (i == 10)
    //        {
    //            nodeData.HE10 = rt;
    //        }
    //        if (i == 11)
    //        {
    //            nodeData.HE11 = rt;
    //        }
    //        if (i == 12)
    //        {
    //            nodeData.HE12 = rt;
    //        }
    //        if (i == 13)
    //        {
    //            nodeData.HE13 = rt;
    //        }
    //        if (i == 14)
    //        {
    //            nodeData.HE14 = rt;
    //        }
    //        if (i == 15)
    //        {
    //            nodeData.HE15 = rt;
    //        }
    //        if (i == 16)
    //        {
    //            nodeData.HE16 = rt;
    //        }
    //        if (i == 17)
    //        {
    //            nodeData.HE17 = rt;
    //        }
    //        if (i == 18)
    //        {
    //            nodeData.HE18 = rt;
    //        }
    //        if (i == 19)
    //        {
    //            nodeData.HE19 = rt;
    //        }
    //        if (i == 20)
    //        {
    //            nodeData.HE20 = rt;
    //        }
    //        if (i == 21)
    //        {
    //            nodeData.HE21 = rt;
    //        }
    //        if (i == 22)
    //        {
    //            nodeData.HE22 = rt;
    //        }
    //        if (i == 23)
    //        {
    //            nodeData.HE23 = rt;
    //        }
    //        return nodeData;
    //    }

    //    public void SetLmpHour(List<CongestionVolatility> AllList, List<DateTime> ListDate)
    //    {
    //        try
    //        {
    //            tempdictdata = new List<CongestionVolatility>();
    //            foreach (var item in ListDate)
    //            {
    //                CongestionVolatility congestion = new CongestionVolatility();

    //                congestion.value = 00;
    //                congestion.ConstraintDate = item.AddHours(0);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h1Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE);
    //                double h1Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE);
    //                double h1Total = (h1Max - h1Min);
    //                congestion.value = h1Total;
    //                congestion.ConstraintDate = item.AddHours(1);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h2Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE1);
    //                double h2Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE1);
    //                double h2Total = (h2Max - h2Min);
    //                congestion.value = h2Total;
    //                congestion.ConstraintDate = item.AddHours(2);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h3Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE2);
    //                double h3Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE2);
    //                double h3Total = (h3Max - h3Min);
    //                congestion.value = h3Total;
    //                congestion.ConstraintDate = item.AddHours(3);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h4Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE3);
    //                double h4Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE3);
    //                double h4Total = (h4Max - h4Min);
    //                congestion.value = h4Total;
    //                congestion.ConstraintDate = item.AddHours(4);
    //                tempdictdata.Add(congestion);


    //                congestion = new CongestionVolatility();
    //                double h5Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE4);
    //                double h5Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE4);
    //                double h5Total = (h5Max - h5Min);
    //                congestion.value = h5Total;
    //                congestion.ConstraintDate = item.AddHours(5);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h6Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE5);
    //                double h6Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE5);
    //                double h6Total = (h6Max - h6Min);
    //                congestion.value = h6Total;
    //                congestion.ConstraintDate = item.AddHours(6);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h7Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE6);
    //                double h7Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE6);
    //                double h7Total = (h7Max - h7Min);
    //                congestion.value = h7Total;
    //                congestion.ConstraintDate = item.AddHours(7);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h8Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE7);
    //                double h8Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE7);
    //                double h8Total = (h8Max - h8Min);
    //                congestion.value = h8Total;
    //                congestion.ConstraintDate = item.AddHours(8);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h9Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE8);
    //                double h9Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE8);
    //                double h9Total = (h9Max - h9Min);
    //                congestion.value = h9Total;
    //                congestion.ConstraintDate = item.AddHours(9);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h10Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE9);
    //                double h10Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE9);
    //                double h10Total = (h10Max - h10Min);
    //                congestion.value = h10Total;
    //                congestion.ConstraintDate = item.AddHours(10);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h11Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE10);
    //                double h11Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE10);
    //                double h11Total = (h11Max - h11Min);
    //                congestion.value = h11Total;
    //                congestion.ConstraintDate = item.AddHours(11);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h12Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE11);
    //                double h12Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE11);
    //                double h12Total = (h12Max - h12Min);
    //                congestion.value = h12Total;
    //                congestion.ConstraintDate = item.AddHours(12);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h13Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE12);
    //                double h13Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE12);
    //                double h13Total = (h13Max - h13Min);
    //                congestion.value = h13Total;
    //                congestion.ConstraintDate = item.AddHours(13);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h14Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE13);
    //                double h14Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE13);
    //                double h14Total = (h14Max - h14Min);
    //                congestion.value = h14Total;
    //                congestion.ConstraintDate = item.AddHours(14);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h15Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE14);
    //                double h15Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE14);
    //                double h15Total = (h15Max - h15Min);
    //                congestion.value = h15Total;
    //                congestion.ConstraintDate = item.AddHours(15);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h16Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE15);
    //                double h16Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE15);
    //                double h16Total = (h16Max - h16Min);
    //                congestion.value = h16Total;
    //                congestion.ConstraintDate = item.AddHours(16);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h17Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE16);
    //                double h17Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE16);
    //                double h17Total = (h17Max - h17Min);
    //                congestion.value = h17Total;
    //                congestion.ConstraintDate = item.AddHours(17);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h18Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE17);
    //                double h18Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE17);
    //                double h18Total = (h18Max - h18Min);
    //                congestion.value = h18Total;
    //                congestion.ConstraintDate = item.AddHours(18);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h19Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE18);
    //                double h19Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE18);
    //                double h19Total = (h19Max - h19Min);
    //                congestion.value = h19Total;
    //                congestion.ConstraintDate = item.AddHours(19);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h20Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE19);
    //                double h20Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE10);
    //                double h20Total = (h20Max - h20Min);
    //                congestion.value = h20Total;
    //                congestion.ConstraintDate = item.AddHours(20);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h21Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE20);
    //                double h21Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE20);
    //                double h21Total = (h21Max - h21Min);
    //                congestion.value = h21Total;
    //                congestion.ConstraintDate = item.AddHours(21);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h22Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE21);
    //                double h22Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE21);
    //                double h22Total = (h22Max - h22Min);
    //                congestion.value = h22Total;
    //                congestion.ConstraintDate = item.AddHours(22);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h23Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE22);
    //                double h23Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE22);
    //                double h23Total = (h23Max - h23Min);
    //                congestion.value = h23Total;
    //                congestion.ConstraintDate = item.AddHours(23);
    //                tempdictdata.Add(congestion);

    //                congestion = new CongestionVolatility();
    //                double h24Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE23);
    //                double h24Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE23);
    //                double h24Total = (h24Max - h24Min);
    //                congestion.value = h24Total;
    //                congestion.ConstraintDate = item.AddHours(24);
    //                tempdictdata.Add(congestion);
    //            }
    //        }
    //        catch
    //        {

    //        }

    //    }

    //    private void FixCongestionData()
    //    {
    //        PlotDataModel = new PlotModel();
    //        DateTime from = FromSelectedDate;
    //        DateTime to = EndSelectedDate;
    //        PlotModel tempModel = new PlotModel();
    //        tempModel.Axes.Add(new LinearAxis
    //        {
    //            Key = "YAxis",
    //            MajorGridlineStyle = LineStyle.Solid,
    //            MinorGridlineStyle = LineStyle.Dot,
    //            MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
    //            MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
    //            IsPanEnabled = false,
    //            IsZoomEnabled = true,
    //            MaximumPadding = 0.5,
    //            MinimumPadding = 0.1,
    //            TextColor = OxyColors.White,
    //            TitleColor = OxyColors.WhiteSmoke,
    //            EndPosition = 1,
    //            AxisTickToLabelDistance = 1,
    //            Position = AxisPosition.Left,
    //            Title = "Shadow Price ----->",
    //            AxisTitleDistance = 1,
    //        });
    //        if (HourlyChecked || FourhourlyChecked)
    //            tempModel.Axes.Add(new DateTimeAxis
    //            {
    //                Title = "Market Date ---->",
    //                Position = AxisPosition.Bottom,
    //                TextColor = OxyColors.White,
    //                TitleColor = OxyColors.WhiteSmoke,
    //                AxisTitleDistance = 0,
    //                StringFormat = "HH\n dd", //MMM-dd
    //                MajorGridlineStyle = LineStyle.Solid,
    //                AxislineThickness = 3,
    //                AxisTickToLabelDistance = 1,
    //                MajorStep = 1.01 / 24,
    //                MinorGridlineStyle = LineStyle.Dot,
    //                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
    //                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
    //                Angle = 360,
    //            });
    //        if (DailyChecked)
    //            tempModel.Axes.Add(new DateTimeAxis
    //            {
    //                Title = "Market Date ---->",
    //                Position = AxisPosition.Bottom,
    //                TextColor = OxyColors.White,
    //                TitleColor = OxyColors.WhiteSmoke,
    //                AxisTitleDistance = 0,
    //                StringFormat = "HH\n dd", //MMM-dd
    //                MajorGridlineStyle = LineStyle.Solid,
    //                AxislineThickness = 3,
    //                AxisTickToLabelDistance = 1,
    //                MajorStep = 1,
    //                MinorGridlineStyle = LineStyle.Dot,
    //                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
    //                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
    //                Angle = 360,
    //            });
    //        try
    //        {
    //            if (mOxyColorList == null)
    //            {
    //                FillColorList();
    //            }
    //            foreach (var item in tempCongestionList.Keys)
    //            {
    //                tempModel.Series.Add(CreateSeries(tempCongestionList[item].OrderBy(j => j.ConstraintDate).ToList(), item));
    //            }
    //            var l = new Legend
    //            {
    //                LegendOrientation = LegendOrientation.Horizontal,
    //                LegendPlacement = LegendPlacement.Outside,
    //                LegendPosition = LegendPosition.RightTop,
    //                LegendTextColor = OxyColors.White,
    //            };

    //            tempModel.Legends.Add(l);

    //            tempModel.TitlePadding = 1;
    //            PlotDataModel = tempModel;
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //    }

    //    private void FillColorList()
    //    {
    //        mOxyColorList = new Dictionary<string, OxyColor>();
    //        mOxyColorList.Add("RT", OxyColors.Yellow);
    //        mOxyColorList.Add("DA", OxyColors.Red);
    //        mOxyColorList.Add("LMP", OxyColors.Blue);
    //    }

    //    private LineSeries CreateSeries(List<CongestionVolatility> list, string key)
    //    {
    //        //list.RemoveAll(a => a.value == 0);
    //        List<GraphItem> grpList = new List<GraphItem>();
    //        try
    //        {
    //            list.ForEach(dItem =>
    //            {
    //                grpList.Add(new GraphItem
    //                {
    //                    X = dItem.ConstraintDate,
    //                    Y = dItem.value
    //                });
    //            });
    //        }
    //        catch (Exception ex)
    //        {
    //        }
    //        LineSeries series = new LineSeries()
    //        {
    //            CanTrackerInterpolatePoints = false,
    //            DataFieldX = "X",
    //            DataFieldY = "Y",
    //            ItemsSource = grpList,
    //            MarkerType = MarkerType.Diamond,
    //            TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " Price",
    //            MarkerSize = 1,
    //            TextColor = OxyColors.Black,
    //            MarkerStrokeThickness = 3,
    //            Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
    //            LineStyle = GetLineStyle(key),
    //            MarkerStroke = OxyColors.White,
    //            Title = key.ToUpper(),
    //        };

    //        // grpList.ToList().ForEach(d => series.Points.Add(new DataPoint(Convert.ToDouble(d.X.Hour), Convert.ToDouble(d.Y))));
    //        return series;

    //    }

    //    public class GraphItem
    //    {
    //        public DateTime X { get; set; }
    //        public double? Y { get; set; }
    //    }
    //    private LineStyle GetLineStyle(string key)
    //    {
    //        if (key.ToUpper().StartsWith(key))
    //        {
    //            return LineStyle.Dot;
    //        }
    //        else
    //        {
    //            return LineStyle.Solid;
    //        }
    //    }

    //    public int GetMarketKey()
    //    {
    //        switch (MarketSelected)
    //        {
    //            case "PJM": return 1;
    //            case "ERCOT": return 9;
    //            default: return 0;
    //        }
    //    }
    //}
    #endregion
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataService _dataService;
        List<CongestionVolatility> CongestionVolatilityList = new List<CongestionVolatility>();
        private Dictionary<string, List<CongestionVolatility>> tempCongestionList;
        List<CongestionVolatility> tempdictdata = null;
        private Dictionary<string, OxyColor> mOxyColorList;
        List<string> mUptosPathList;
        private Dictionary<string, Vayu.DBLibrary.NodeDetail> mNodeHash = new Dictionary<string, Vayu.DBLibrary.NodeDetail>();
        private Dictionary<int, List<string>> mNodeTypeHash = new Dictionary<int, List<string>>();
        private Dictionary<int, List<string>> mZoneHash = new Dictionary<int, List<string>>();
        List<DateTime> dateList = new List<DateTime>();
        DateTime mdate = new DateTime();
        #region Properties

        public DelegateCommand RefreshCommand { get; private set; }

        public string[] MarketList { get; set; }

        private OxyPlot.PlotModel mPlotDataModel;
        public OxyPlot.PlotModel PlotDataModel
        {
            get
            {
                return mPlotDataModel;
            }
            set
            {
                mPlotDataModel = value; RaisePropertyChanged("PlotDataModel");
            }
        }
        private DateTime mFromSelectedDate = DateTime.Now.AddDays(-2);
        public DateTime FromSelectedDate
        {
            get
            {
                return mFromSelectedDate;
            }
            set
            {
                mFromSelectedDate = value; RaisePropertyChanged("FromSelectedDate");
            }
        }

        private DateTime mEndSelectedDate;
        public DateTime EndSelectedDate
        {
            get
            {
                return mEndSelectedDate;
            }
            set
            {
                mEndSelectedDate = value;
                RaisePropertyChanged("EndSelectedDate");
                if (value != null)
                {
                    mEndSelectedDate = mEndSelectedDate.AddHours(23).AddMinutes(55);
                }
            }
        }
        private bool mHourlyChecked;
        public bool HourlyChecked
        {
            get { return mHourlyChecked; }
            set
            {
                mHourlyChecked = value;
                RaisePropertyChanged("HourlyChecked");
            }
        }
        private bool mFourhourlyChecked;
        public bool FourhourlyChecked
        {
            get { return mFourhourlyChecked; }
            set
            {
                mFourhourlyChecked = value;
                RaisePropertyChanged("FourhourlyChecked");
            }
        }

        private bool mDailyChecked;
        public bool DailyChecked
        {
            get { return mDailyChecked; }
            set
            {
                mDailyChecked = value;
                RaisePropertyChanged("DailyChecked");
            }
        }
        private string mMarketSelected;
        public string MarketSelected
        {
            get { return mMarketSelected; }
            set
            {
                RaisePropertyChanged("MarketSelected");
                mMarketSelected = value;
            }
        }

        #endregion
        private List<CongestionVolatility> mCongestionList;

        public List<CongestionVolatility> CongestionList
        {
            get { return mCongestionList; }
            set
            {
                mCongestionList = value;
                RaisePropertyChanged("CongestionList");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;
            MarketList = new string[] { "ERCOT" };
            MarketSelected = MarketList.FirstOrDefault();
            EndSelectedDate = DateTime.Parse(DateTime.Now.AddDays(-1).ToString("MM-dd-yyyy"));
            RefreshCommand = new DelegateCommand(() => Refresh());
            HourlyChecked = true;
            // FillSourceSinkHash();
            Refresh();
        }

        private void Refresh()
        {
            dateList = new List<DateTime>();
            DateTime date = DateTime.Parse(FromSelectedDate.ToString("MM-dd-yyyy"));
            while (date <= EndSelectedDate)
            {
                dateList.Add(date);
                date = date.AddDays(1);
            }
            mdate = DateTime.Parse(FromSelectedDate.ToString("MM-dd-yyyy"));

            tempCongestionList = new Dictionary<string, List<CongestionVolatility>>();
            GetConstraints(GetMarketKey(), "RT");
            tempCongestionList.Add("RT", tempdictdata);
            GetConstraints(GetMarketKey(), "DA");
            tempCongestionList.Add("DA", tempdictdata);
            if (HourlyChecked)
            {
                SetTable();
                tempCongestionList.Add("LMP", tempdictdata);
            }
            FixCongestionData();
        }

        private void GetConstraints(int p, string Type)
        {
            CongestionVolatilityList = new List<CongestionVolatility>();
            _dataService.GetCongestionVolatility((a, e) =>
            {
                if (e == null)
                {
                    tempdictdata = a;
                }
                else
                {
                    System.Windows.MessageBox.Show(e.Message);
                }

            }, p, mdate, EndSelectedDate, Type, HourlyChecked, FourhourlyChecked, DailyChecked);
        }


        private void SetTable()
        {
            try
            {
                List<string> UptosNodeList = _dataService.GetUptosNode(GetMarketKey());
                tempdictdata = new List<CongestionVolatility>();
                string name = null;
                string sink = null;
                mNodeHash = DBAccess.GetAllNodes(GetMarketKey(), mNodeTypeHash, mZoneHash);
                DARTNode.GetAllDartsAndLMP(GetMarketKey(), DateTime.Parse(FromSelectedDate.ToString("MM-dd-yyyy")), EndSelectedDate);
                List<CongestionVolatility> templist = new List<CongestionVolatility>();
                List<string> nodeNameList = mNodeHash.Keys.ToList<string>();
                List<string> nameList = new List<string>();
                if (GetMarketKey() == 1)
                    nameList = nodeNameList;//mUptosPathList;
                else
                    nameList = nodeNameList;
                foreach (DateTime date in dateList)
                {
                    foreach (var item in nameList)
                    {
                        string[] tokens = item.Split('?');
                        name = tokens[0];
                        //if (GetMarketKey() == 1)
                        //    sink = tokens[1];

                        CongestionVolatility nodeData = new CongestionVolatility();

                        for (int i = 0; i < 24; i++)
                        {
                            double rtPrice = double.NaN;

                            if (!mNodeHash.ContainsKey(name) || (sink != null && !mNodeHash.ContainsKey(sink)))
                            {
                                continue;
                            }
                            string sourceKey = date.AddHours(i + 1).ToString() + mNodeHash[name].ID.ToString();
                            string sinkKey = sink == null ? null : date.AddHours(i + 1).ToString() + mNodeHash[sink].ID.ToString();
                            if (DARTNode.sRTLmpHash.ContainsKey(sourceKey) && !double.IsNaN(DARTNode.sRTLmpHash[sourceKey].Price))
                            {
                                if (double.IsNaN(rtPrice))
                                {
                                    if (UptosNodeList.Contains(name))
                                    {
                                        rtPrice = DARTNode.sRTLmpHash[sourceKey].Price;
                                        #region AllNodes
                                        //if (sinkKey != null)
                                        //{
                                        //    if (sinkKey != null)
                                        //    {
                                        //        double sinkRt = (DARTNode.sRTLmpHash.ContainsKey(sinkKey) && !double.IsNaN(DARTNode.sRTLmpHash[sinkKey].Price)) ? DARTNode.sRTLmpHash[sinkKey].Price : 0;
                                        //        rtPrice = sinkRt - DARTNode.sRTLmpHash[sourceKey].Price;
                                        //    }
                                        //}
                                        #endregion AllNodes
                                        nodeData = SetNodeData(i, nodeData, rtPrice);
                                        nodeData.GetType().GetProperty("HE" + (i + 1)).SetValue(nodeData, null, null);
                                        nodeData.ConstraintDate = date;
                                        templist.Add(nodeData);
                                    }
                                }
                            }
                            else
                                continue;
                        }


                    }

                }
                SetLmpHour(templist, dateList);
            }
            catch (Exception ex)
            {

            }
        }

        private CongestionVolatility SetNodeData(int i, CongestionVolatility nodeData, double? rt)
        {
            if (i == 0)
            {
                nodeData.HE = rt;
            }
            if (i == 1)
            {
                nodeData.HE1 = rt;
            }
            if (i == 2)
            {
                nodeData.HE2 = rt;
            }
            if (i == 3)
            {
                nodeData.HE3 = rt;
            }
            if (i == 4)
            {
                nodeData.HE4 = rt;
            }
            if (i == 5)
            {
                nodeData.HE5 = rt;
            }
            if (i == 6)
            {
                nodeData.HE6 = rt;
            }
            if (i == 7)
            {
                nodeData.HE7 = rt;
            }
            if (i == 8)
            {
                nodeData.HE8 = rt;
            }
            if (i == 9)
            {
                nodeData.HE9 = rt;
            }
            if (i == 10)
            {
                nodeData.HE10 = rt;
            }
            if (i == 11)
            {
                nodeData.HE11 = rt;
            }
            if (i == 12)
            {
                nodeData.HE12 = rt;
            }
            if (i == 13)
            {
                nodeData.HE13 = rt;
            }
            if (i == 14)
            {
                nodeData.HE14 = rt;
            }
            if (i == 15)
            {
                nodeData.HE15 = rt;
            }
            if (i == 16)
            {
                nodeData.HE16 = rt;
            }
            if (i == 17)
            {
                nodeData.HE17 = rt;
            }
            if (i == 18)
            {
                nodeData.HE18 = rt;
            }
            if (i == 19)
            {
                nodeData.HE19 = rt;
            }
            if (i == 20)
            {
                nodeData.HE20 = rt;
            }
            if (i == 21)
            {
                nodeData.HE21 = rt;
            }
            if (i == 22)
            {
                nodeData.HE22 = rt;
            }
            if (i == 23)
            {
                nodeData.HE23 = rt;
            }
            return nodeData;
        }

        public void SetLmpHour(List<CongestionVolatility> AllList, List<DateTime> ListDate)
        {
            try
            {
                tempdictdata = new List<CongestionVolatility>();
                foreach (var item in ListDate)
                {
                    CongestionVolatility congestion = new CongestionVolatility();

                    congestion.value = 00;
                    congestion.ConstraintDate = item.AddHours(0);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h1Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE);
                    double h1Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE);
                    double h1Total = (h1Max - h1Min);
                    congestion.value = h1Total;
                    congestion.ConstraintDate = item.AddHours(1);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h2Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE1);
                    double h2Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE1);
                    double h2Total = (h2Max - h2Min);
                    congestion.value = h2Total;
                    congestion.ConstraintDate = item.AddHours(2);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h3Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE2);
                    double h3Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE2);
                    double h3Total = (h3Max - h3Min);
                    congestion.value = h3Total;
                    congestion.ConstraintDate = item.AddHours(3);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h4Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE3);
                    double h4Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE3);
                    double h4Total = (h4Max - h4Min);
                    congestion.value = h4Total;
                    congestion.ConstraintDate = item.AddHours(4);
                    tempdictdata.Add(congestion);


                    congestion = new CongestionVolatility();
                    double h5Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE4);
                    double h5Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE4);
                    double h5Total = (h5Max - h5Min);
                    congestion.value = h5Total;
                    congestion.ConstraintDate = item.AddHours(5);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h6Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE5);
                    double h6Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE5);
                    double h6Total = (h6Max - h6Min);
                    congestion.value = h6Total;
                    congestion.ConstraintDate = item.AddHours(6);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h7Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE6);
                    double h7Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE6);
                    double h7Total = (h7Max - h7Min);
                    congestion.value = h7Total;
                    congestion.ConstraintDate = item.AddHours(7);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h8Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE7);
                    double h8Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE7);
                    double h8Total = (h8Max - h8Min);
                    congestion.value = h8Total;
                    congestion.ConstraintDate = item.AddHours(8);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h9Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE8);
                    double h9Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE8);
                    double h9Total = (h9Max - h9Min);
                    congestion.value = h9Total;
                    congestion.ConstraintDate = item.AddHours(9);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h10Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE9);
                    double h10Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE9);
                    double h10Total = (h10Max - h10Min);
                    congestion.value = h10Total;
                    congestion.ConstraintDate = item.AddHours(10);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h11Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE10);
                    double h11Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE10);
                    double h11Total = (h11Max - h11Min);
                    congestion.value = h11Total;
                    congestion.ConstraintDate = item.AddHours(11);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h12Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE11);
                    double h12Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE11);
                    double h12Total = (h12Max - h12Min);
                    congestion.value = h12Total;
                    congestion.ConstraintDate = item.AddHours(12);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h13Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE12);
                    double h13Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE12);
                    double h13Total = (h13Max - h13Min);
                    congestion.value = h13Total;
                    congestion.ConstraintDate = item.AddHours(13);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h14Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE13);
                    double h14Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE13);
                    double h14Total = (h14Max - h14Min);
                    congestion.value = h14Total;
                    congestion.ConstraintDate = item.AddHours(14);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h15Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE14);
                    double h15Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE14);
                    double h15Total = (h15Max - h15Min);
                    congestion.value = h15Total;
                    congestion.ConstraintDate = item.AddHours(15);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h16Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE15);
                    double h16Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE15);
                    double h16Total = (h16Max - h16Min);
                    congestion.value = h16Total;
                    congestion.ConstraintDate = item.AddHours(16);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h17Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE16);
                    double h17Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE16);
                    double h17Total = (h17Max - h17Min);
                    congestion.value = h17Total;
                    congestion.ConstraintDate = item.AddHours(17);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h18Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE17);
                    double h18Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE17);
                    double h18Total = (h18Max - h18Min);
                    congestion.value = h18Total;
                    congestion.ConstraintDate = item.AddHours(18);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h19Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE18);
                    double h19Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE18);
                    double h19Total = (h19Max - h19Min);
                    congestion.value = h19Total;
                    congestion.ConstraintDate = item.AddHours(19);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h20Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE19);
                    double h20Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE10);
                    double h20Total = (h20Max - h20Min);
                    congestion.value = h20Total;
                    congestion.ConstraintDate = item.AddHours(20);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h21Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE20);
                    double h21Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE20);
                    double h21Total = (h21Max - h21Min);
                    congestion.value = h21Total;
                    congestion.ConstraintDate = item.AddHours(21);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h22Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE21);
                    double h22Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE21);
                    double h22Total = (h22Max - h22Min);
                    congestion.value = h22Total;
                    congestion.ConstraintDate = item.AddHours(22);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h23Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE22);
                    double h23Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE22);
                    double h23Total = (h23Max - h23Min);
                    congestion.value = h23Total;
                    congestion.ConstraintDate = item.AddHours(23);
                    tempdictdata.Add(congestion);

                    congestion = new CongestionVolatility();
                    double h24Max = (double)AllList.Where(a => a.ConstraintDate == item).Max(a => a.HE23);
                    double h24Min = (double)AllList.Where(a => a.ConstraintDate == item).Min(a => a.HE23);
                    double h24Total = (h24Max - h24Min);
                    congestion.value = h24Total;
                    congestion.ConstraintDate = item.AddHours(24);
                    tempdictdata.Add(congestion);
                }
            }
            catch
            {

            }

        }

        private void FixCongestionData()
        {
            PlotDataModel = new PlotModel();
            DateTime from = FromSelectedDate;
            DateTime to = EndSelectedDate;
            PlotModel tempModel = new PlotModel();
            tempModel.Axes.Add(new LinearAxis
            {
                Key = "YAxis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                IsPanEnabled = false,
                IsZoomEnabled = true,
                MaximumPadding = 0.5,
                MinimumPadding = 0.1,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.WhiteSmoke,
                EndPosition = 1,
                AxisTickToLabelDistance = 1,
                Position = AxisPosition.Left,
                Title = "Shadow Price ----->",
                AxisTitleDistance = 1,
            });
            if (HourlyChecked || FourhourlyChecked)
                tempModel.Axes.Add(new DateTimeAxis
                {
                    Title = "Market Date ---->",
                    Position = AxisPosition.Bottom,
                    TextColor = OxyColors.White,
                    TitleColor = OxyColors.WhiteSmoke,
                    AxisTitleDistance = 0,
                    StringFormat = "HH\n dd", //MMM-dd
                    MajorGridlineStyle = LineStyle.Solid,
                    AxislineThickness = 3,
                    AxisTickToLabelDistance = 1,
                    MajorStep = 1.01 / 24,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                    MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                    Angle = 360,
                });
            if (DailyChecked)
                tempModel.Axes.Add(new DateTimeAxis
                {
                    Title = "Market Date ---->",
                    Position = AxisPosition.Bottom,
                    TextColor = OxyColors.White,
                    TitleColor = OxyColors.WhiteSmoke,
                    AxisTitleDistance = 0,
                    StringFormat = "HH\n dd", //MMM-dd
                    MajorGridlineStyle = LineStyle.Solid,
                    AxislineThickness = 3,
                    AxisTickToLabelDistance = 1,
                    MajorStep = 1,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255),
                    MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255),
                    Angle = 360,
                });
            try
            {
                if (mOxyColorList == null)
                {
                    FillColorList();
                }
                foreach (var item in tempCongestionList.Keys)
                {
                    tempModel.Series.Add(CreateSeries(tempCongestionList[item].OrderBy(j => j.ConstraintDate).ToList(), item));
                }
                var l = new Legend
                {
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.RightTop,
                    LegendTextColor = OxyColors.White,

                };
                tempModel.Legends.Add(l);
                tempModel.TitlePadding = 1;
                PlotDataModel = tempModel;
            }
            catch (Exception ex)
            {
            }
        }

        private void FillColorList()
        {
            mOxyColorList = new Dictionary<string, OxyColor>();
            mOxyColorList.Add("RT", OxyColors.Yellow);
            mOxyColorList.Add("DA", OxyColors.Red);
            mOxyColorList.Add("LMP", OxyColors.Blue);
        }

        private LineSeries CreateSeries(List<CongestionVolatility> list, string key)
        {
            //list.RemoveAll(a => a.value == 0);
            List<GraphItem> grpList = new List<GraphItem>();
            try
            {
                list.ForEach(dItem =>
                {
                    grpList.Add(new GraphItem
                    {
                        X = dItem.ConstraintDate,
                        Y = dItem.value
                    });
                });
            }
            catch (Exception ex)
            {
            }
            LineSeries series = new LineSeries()
            {
                CanTrackerInterpolatePoints = false,
                DataFieldX = "X",
                DataFieldY = "Y",
                ItemsSource = grpList,
                MarkerType = MarkerType.Diamond,
                TrackerFormatString = "{0}\n{X:MM-dd HH:mm}\n{Y:0.###}" + " Price",
                MarkerSize = 1,
                TextColor = OxyColors.Black,
                MarkerStrokeThickness = 3,
                Color = mOxyColorList.ContainsKey(key) ? mOxyColorList[key] : OxyColors.MediumPurple,
                LineStyle = GetLineStyle(key),
                MarkerStroke = OxyColors.White,
                Title = key.ToUpper(),
            };

            // grpList.ToList().ForEach(d => series.Points.Add(new DataPoint(Convert.ToDouble(d.X.Hour), Convert.ToDouble(d.Y))));
            return series;

        }

        public class GraphItem
        {
            public DateTime X { get; set; }
            public double? Y { get; set; }
        }
        private LineStyle GetLineStyle(string key)
        {
            if (key.ToUpper().StartsWith(key))
            {
                return LineStyle.Dot;
            }
            else
            {
                return LineStyle.Solid;
            }
        }

        public int GetMarketKey()
        {
            switch (MarketSelected)
            {
                case "PJM": return 1;
                case "ERCOT": return 9;
                default: return 0;
            }
        }
    }
}
