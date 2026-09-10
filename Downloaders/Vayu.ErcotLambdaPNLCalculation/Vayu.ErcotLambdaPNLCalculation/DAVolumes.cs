using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.NodePriceLibrary
{
    public class DAVolumes
    {
        public int NodeKey { get; set; }
        public DateTime DeliveryYDate { get; set; }
        public int HourEnding { get; set; }
        public string STLPNT { get; set; }
        public double TOTAL_PTP_OBL_AWARDED_SOURCE { get; set; }
        public double TOTAL_PTP_OBL_AWARDED_SINK { get; set; }
        public string DSTFlag { get; set; }
        public double NetVolume { get; set; }
        public double RT { get; set; }
        public double DA { get; set; }
        public double DART { get; set; }
        public double TotalDART { get; set; }
        public double DARTSource { get; set; }
        public double DARTSink { get; set; }
        public double SUM { get; set; }
        public double RT_Node { get; set; }
        public double DA_Node { get; set; }

        public double DART_Node { get; set; }

        public double RT_Energy { get; set; }
        public double DA_Energy { get; set; }
        public double RT_Cong { get; set; }
        public double DA_Cong { get; set; }
        public double DART_Cong { get; set; }
        public string Zone { get; set; }
        public string Fuelsource { get; set; }




    }
}
