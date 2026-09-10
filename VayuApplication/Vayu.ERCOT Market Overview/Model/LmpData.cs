using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Vayu.ERCOT_Market_Overview.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class LmpData
    {
        /// <summary>
        /// Gets or sets whether node is deenergized.
        /// </summary>
        /// <value>
        /// The value of energized.
        /// </value>
        public bool NodeDeenergized { get; set; }
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the LMP.
        /// </summary>
        /// <value>
        /// The LMP.
        /// </value>
        public double LMP { get; set; }
        /// <summary>
        /// Gets or sets the LMP.
        /// </summary>
        /// <value>
        /// The LMP.
        /// </value>
        public double DALMP { get; set; }
        /// <summary>
        /// Gets or sets the congestion.
        /// </summary>
        /// <value>
        /// The congestion.
        /// </value>
        public double Congestion { get; set; }
        /// <summary>
        /// Gets or sets the loss.
        /// </summary>
        /// <value>
        /// The loss.
        /// </value>     
        public double Loss { get; set; }
        /// Gets or sets the DaCongestion Price.
        /// </summary>
        /// <value>
        /// The da congestion .
        /// </value>
        /// 
        public double DACongestion { get; set; }
        /// <summary>
        /// Gets or sets the RT congestion - DA Congestion.
        /// </summary>
        /// <value>
        /// The DART consetion.
        /// </value>
        /// 

        public double DART { get; set; }


        /// <summary>
        /// Gets or sets the energy Price.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        /// 
        public double EnergyPrice { get; set; }

        /// <summary>
        /// Gets or sets the DAenergy Price.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        /// 
        public double DAEnergyPrice { get; set; }

        /// Gets or sets the date time.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        /// 

        public string DateTime { get; set; }
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int NodeKey { get; set; }

        public string Zone { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LmpData"/> class.
        /// </summary>
        public LmpData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LmpData"/> class.
        /// </summary>
        /// <param name="lmpTimePrice">The LMP time price.</param>
        /// <param name="nodekey">The nodekey.</param>
        /// <param name="nodename">The nodename.</param>
        public LmpData(Vayu.NodePriceLibrary.LmpTimePrice lmpTimePrice, int nodekey, string nodename, string Zones, bool isdeenergized)
        {
            if (nodekey == 57231)
            {

            }
            DateTime = lmpTimePrice.MarketTime.ToString("MM/dd/yyyy HH:mm:ss tt");
            LMP = lmpTimePrice.Lmp.Price;
            DALMP = lmpTimePrice.Lmp.DAPrice;
            Congestion = lmpTimePrice.Lmp.Congestion;
            Loss = lmpTimePrice.Lmp.Loss;
            EnergyPrice = lmpTimePrice.Lmp.EnergyPrice;
            DAEnergyPrice = lmpTimePrice.Lmp.DAEnergyPrice;
            Congestion = LMP - EnergyPrice;
            DACongestion = DALMP - DAEnergyPrice;

            DART = Congestion - DACongestion;
            NodeName = nodename;
            NodeKey = nodekey;
            Zone = Zones;
            NodeDeenergized = isdeenergized;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{Vayu.MarketViewNameSpace.Model.LmpData}" />
    public class LMPDataList : ObservableCollection<LmpData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LMPDataList"/> class.
        /// </summary>
        public LMPDataList() { }

        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }

        /// <summary>
        /// Gets or sets the original list.
        /// </summary>
        /// <value>
        /// The original list.
        /// </value>
        public IEnumerable<LmpData> OriginalList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LMPDataList"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="origionalList">The origional list.</param>
        public LMPDataList(IEnumerable<LmpData> list, IEnumerable<LmpData> origionalList = null)
            : base(list)
        {
            if (origionalList == null)
            {
                OriginalList = list;
                //OriginalList = list.OrderBy(x =>x.Zone);
            }

            else
            {
                OriginalList = origionalList;
                //OriginalList = origionalList.OrderBy(x => x.Zone);
            }

        }
    }
}
