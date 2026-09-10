using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vayu.NodePriceLibrary;
using Vayu.DBLibrary;
using Vayu.EnergyLMP;
using System.Data;
using System.Threading;
using System.Timers;

namespace Vayu.ErcotLambdaPNLCalculation
{
    class CalcualtePNL
    {
        public List<DAVolumes> PathList;
        DataService dataservice = new DataService();
        DataTable mDTEmoPnl = new DataTable();
        System.Timers.Timer mTimer = new System.Timers.Timer();

        public CalcualtePNL()
        {
            mDTEmoPnl.Columns.Add("DeliveryDate", typeof(DateTime));
            mDTEmoPnl.Columns.Add("HourEnding", typeof(int));
            mDTEmoPnl.Columns.Add("NodeKey", typeof(int));

            mDTEmoPnl.Columns.Add("A_MW", typeof(decimal));
            mDTEmoPnl.Columns.Add("A_DA", typeof(decimal));
            mDTEmoPnl.Columns.Add("A_RT", typeof(decimal));
            mDTEmoPnl.Columns.Add("A_DART", typeof(decimal));
            mDTEmoPnl.Columns.Add("A_COST", typeof(decimal));
            mDTEmoPnl.Columns.Add("A_REV", typeof(decimal));
            mDTEmoPnl.Columns.Add("A_PNL", typeof(decimal));

            mDTEmoPnl.Columns.Add("B_MW", typeof(decimal));
            mDTEmoPnl.Columns.Add("B_DA",typeof(decimal));
            mDTEmoPnl.Columns.Add("B_RT", typeof(decimal));
            mDTEmoPnl.Columns.Add("B_DART", typeof(decimal));
            mDTEmoPnl.Columns.Add("B_COST", typeof(decimal));
            mDTEmoPnl.Columns.Add("B_REV", typeof(decimal));
            mDTEmoPnl.Columns.Add("B_PNL", typeof(decimal));
            mDTEmoPnl.Columns.Add("TotalPNL", typeof(decimal));
            mDTEmoPnl.Columns.Add("TotalCost", typeof(decimal));
            mDTEmoPnl.Columns.Add("TotalRev", typeof(decimal));



            OnTimedEvent(null, null);
            mTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            mTimer.Interval = 15 * 60 * 1000;
            mTimer.Enabled = true;
            bool flag = true;
            //while (flag)
            //{
            //    Thread.Sleep(60*60 * 1000);
            //    flag = false;

            //}

        }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            mTimer.Enabled = false;
            RetrieveData();
            mTimer.Enabled = true;
        }
        public void RetrieveData()
        {
            DateTime maxDateSave = dataservice.getMaxDate();
            int cnt = 0;
            DateTime StartDate = DateTime.Now.Date.AddDays(-5);

            TimeSpan difference = DateTime.Now.Date - maxDateSave;

          
            int daysDifference = difference.Days;

            if (daysDifference == 0)
                StartDate = maxDateSave;
            else
                StartDate = maxDateSave;
            



            DateTime EndDate = DateTime.Now.Date;
            
            try
            {
                while (EndDate >= StartDate)
                {
                    Console.WriteLine("Calculating EMO PNL For Date " + StartDate);
                    if (!false)
                    {
                        List<PNLData> PNLDataList = new List<PNLData>();
                        Dictionary<int, Node> mDAHash = new Dictionary<int, Node>();
                        Dictionary<int, Node> mRTHash = new Dictionary<int, Node>();
                        List<DAVolumes> listDAVolumes = dataservice.GetDAVolumes(StartDate);
                        List<DataItem> rtEnergyList = dataservice.GetRTEnergy(StartDate);
                        List<DataItem> daEnergyList = dataservice.GetDAEnergy(StartDate);
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
                        cnt = 0;
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

                            foreach (DAVolumes path in PathList)
                            {


                                cnt = cnt + 1;                               
                                PNLData pnlData = new PNLData();
                                DateTime CurrentDateTime = DateTime.Now;
                                TimeSpan Currentdifference = CurrentDateTime.Date - path.DeliveryYDate;
                                int currentDiff = Currentdifference.Days;


                                pnlData.date = path.DeliveryYDate;
                                pnlData.hour = path.HourEnding;
                                pnlData.NodeKey = path.NodeKey;

                                pnlData.A_Source = "LAMBDA";
                                pnlData.A_Sink = path.STLPNT;



                                pnlData.B_Source = path.STLPNT;
                                pnlData.B_Sink = "LAMBDA";


                                pnlData.A_MW = path.TOTAL_PTP_OBL_AWARDED_SINK;
                                pnlData.A_DA = path.DA - path.DA_Energy;
                                pnlData.A_RT = path.RT - path.RT_Energy;
                                pnlData.A_DART = pnlData.A_RT - pnlData.A_DA;
                                pnlData.A_COST = pnlData.A_MW * pnlData.A_DA;
                                pnlData.A_REV = pnlData.A_MW * pnlData.A_RT;
                                pnlData.A_PNL = pnlData.A_REV - pnlData.A_COST;


                                pnlData.B_MW = path.TOTAL_PTP_OBL_AWARDED_SOURCE;
                                pnlData.B_DA = path.DA_Energy - path.DA;
                                pnlData.B_RT = path.RT_Energy - path.RT;
                                pnlData.B_DART = pnlData.B_RT - pnlData.B_DA;
                                pnlData.B_COST = pnlData.B_MW * pnlData.B_DA;
                                pnlData.B_REV = pnlData.B_MW * pnlData.B_RT;
                                pnlData.B_PNL = pnlData.B_REV - pnlData.B_COST;

                                pnlData.Total_PNL = pnlData.A_PNL + pnlData.B_PNL;
                                pnlData.Total_COST = pnlData.A_COST + pnlData.B_COST;
                                pnlData.Total_REV = pnlData.A_REV + pnlData.B_REV;

                                PNLDataList.Add(pnlData);


                            }



                        }
                        cnt = 0;
                        if (PNLDataList.Count > 0)
                        {

                            foreach (var item in PNLDataList)
                            {
                                cnt = cnt + 1;
                                if (cnt == 3554)
                                { }
                                DataRow dataRow = mDTEmoPnl.NewRow();
                                dataRow["DeliveryDate"] = item.date;
                                dataRow["HourEnding"] = item.hour;
                                dataRow["NodeKey"] = item.NodeKey;

                                dataRow["A_MW"] = item.A_MW;
                                dataRow["A_DA"] = item.A_DA;
                                dataRow["A_COST"] = item.A_COST;


                                if (item.A_RT == null || Double.IsNaN((double)item.A_RT))
                                    dataRow["A_RT"] = DBNull.Value;
                                else
                                    dataRow["A_RT"] = item.A_RT;

                                if (item.A_DART == null || Double.IsNaN((double)item.A_DART))
                                    dataRow["A_DART"] = DBNull.Value;
                                else
                                    dataRow["A_DART"] = item.A_DART;
                               

                                if (item.A_REV == null || Double.IsNaN((double)item.A_REV))
                                    dataRow["A_REV"] = DBNull.Value;
                                else
                                    dataRow["A_REV"] = item.A_REV;


                                if (item.A_PNL == null || Double.IsNaN((double)item.A_PNL))
                                    dataRow["A_PNL"] = DBNull.Value;
                                else
                                    dataRow["A_PNL"] = item.A_PNL;




                                dataRow["B_MW"] = item.B_MW;
                                dataRow["B_DA"] = item.B_DA;
                                dataRow["B_COST"] = item.B_COST;

                                


                                if (item.B_RT == null || Double.IsNaN((double)item.B_RT))
                                    dataRow["B_RT"] = DBNull.Value;
                                else
                                    dataRow["B_RT"] = item.B_RT;


                                if (item.B_DART == null || Double.IsNaN((double)item.B_DART))
                                    dataRow["B_DART"] = DBNull.Value;
                                else
                                    dataRow["B_DART"] = item.B_DART;



                                if (item.B_REV == null || Double.IsNaN((double)item.B_REV))
                                    dataRow["B_REV"] = DBNull.Value;
                                else
                                    dataRow["B_REV"] = item.B_REV;


                                if (item.B_PNL == null || Double.IsNaN((double)item.B_PNL))
                                    dataRow["B_PNL"] = DBNull.Value;
                                else
                                    dataRow["B_PNL"] = item.B_PNL;



                                if (item.Total_PNL == null || Double.IsNaN((double)item.Total_PNL))
                                    dataRow["TotalPNL"] = DBNull.Value;
                                else
                                    dataRow["TotalPNL"] = item.Total_PNL;




                                if (item.Total_REV == null || Double.IsNaN((double)item.Total_REV))
                                    dataRow["TotalRev"] = DBNull.Value;
                                else
                                    dataRow["TotalRev"] = item.Total_REV;


                                dataRow["TotalCost"] = item.Total_COST;
                               

                                mDTEmoPnl.Rows.Add(dataRow);



                            }

                        }
                        var distinctRows = mDTEmoPnl.AsEnumerable()
                             .GroupBy(r => new { DeliveryDate = r["DeliveryDate"], HourEnding = r["HourEnding"], NodeKey = r["NodeKey"] })
                             .Select(g => g.First())
                             .CopyToDataTable();
                       // DataTable distinctTable = mDTEmoPnl.DefaultView.ToTable(true);
                        if (distinctRows.Rows.Count > 0)
                        {

                            dataservice.SavePNL(distinctRows);
                            //distinctTable.Clear();
                            mDTEmoPnl.Clear();
                            StartDate = StartDate.AddDays(1);
                        }



                    }//if
                    
                }//Endate
            }
            catch (Exception ae)
            { 
              
            }
        }
    }
}
