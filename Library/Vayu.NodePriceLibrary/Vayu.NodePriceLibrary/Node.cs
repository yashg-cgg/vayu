using System;
using System.Collections.Generic;

namespace Vayu.NodePriceLibrary
{
    public class Node
    {
        public int Market { get; set; }

        public int NodeId { get; set; }

        public long PNodeId { get; set; }
        public string NodeName { get; set; }
        public List<TimePrice> TimePriceList { get; set; }

        public List<LmpTimePrice> LmpTimePriceList { get; set; }

        public Node()
        {

        }
        public Node(Node fromNode)
        {
            Market = fromNode.Market;
            NodeId = fromNode.NodeId;
            PNodeId = fromNode.PNodeId;
            NodeName = fromNode.NodeName;
            if (fromNode.TimePriceList != null)
            {
                List<TimePrice> tpList = new List<TimePrice>();
                foreach (TimePrice tp in fromNode.TimePriceList)
                {
                    TimePrice tpTemp = new TimePrice();
                    tpTemp.MarketTime = tp.MarketTime;
                    tpTemp.Price = tp.Price;

                    tpList.Add(tpTemp);
                }
                TimePriceList = tpList;
            }
            if (fromNode.LmpTimePriceList != null)
            {
                List<LmpTimePrice> tpList = new List<LmpTimePrice>();

                foreach (LmpTimePrice tp in fromNode.LmpTimePriceList)
                {
                    LmpTimePrice tpTemp = new LmpTimePrice();
                    tpTemp.Lmp = new LMP();
                    tpTemp.MarketTime = tp.MarketTime;
                    tpTemp.Lmp.Price = tp.Lmp.Price;

                    tpTemp.Lmp.Congestion = tp.Lmp.Congestion;
                    tpTemp.Lmp.Loss = tp.Lmp.Loss;
                    tpList.Add(tpTemp);
                }
                LmpTimePriceList = tpList;
            }
        }

    }

    public class LMP
    {
        public double Price = double.NaN;
        public double DAPrice = double.NaN;
        public double Congestion = double.NaN;
        public double Loss = double.NaN;
        public double Energy = double.NaN;
        public double EnergyPrice = double.NaN;
        public double DAEnergyPrice = double.NaN;
        public double DACongestion = double.NaN;

    }

    public class LmpTimePrice
    {
        public DateTime MarketTime { get; set; }

        public LMP Lmp { get; set; }

    }
    public class TimePrice
    {
        public DateTime MarketTime { get; set; }

        public double Price = double.NaN;

    }
}
