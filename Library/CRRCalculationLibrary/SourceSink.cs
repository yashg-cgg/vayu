using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;

namespace Vayu.CRRCalculationLibrary
{
    /// <seealso cref="System.Attribute" />
    [DataContract]
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false,
    AllowMultiple = false)]
    [Serializable()]
    public class SourceSink : Attribute
    {
        
        /// </value>
        [DataMember]
        public string AuctionName { get; set; }
        
        [DataMember]
        public int AuctionKey { get; set; }
         
        [DataMember]
        public string Month { get; set; }
         
        [DataMember]
        public string Source { get; set; }
         
        [DataMember]
        public string Sink { get; set; }
         
        [DataMember]
        public string ClassType { get; set; }
        
        [DataMember]
        public string PeriodType { get; set; }
        
        [DataMember]
        public string TradeType { get; set; }
         
        [DataMember]
        public string HedgeType { get; set; }
        
        [DataMember]
        public double MW { get; set; }
         
        [DataMember]
        public double Obligation { get; set; }
        
        [DataMember]
        public double Option { get; set; }
         
        [DataMember]
        public string Period { get; set; }
        
        [DataMember]
        public long Ftrid { get; set; }
        
        [DataMember]
        public int PeriodKey { get; set; }
      
        [DataMember]
        public long CRR_ID { get; set; }                 //for ercot
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public string Category { get; set; }            //for ercot
        /// <summary>
        /// Gets or sets the timeof use.
        /// </summary>
        /// <value>
        /// The timeof use.
        /// </value>
        [DataMember]
        public string TimeofUse { get; set; }           //for ercot
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        public DateTime StartDate { get; set; }         //for ercot
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        public DateTime EndDate { get; set; }           //for ercot
        /// <summary>
        /// Gets or sets the shadow price.
        /// </summary>
        /// <value>
        /// The shadow price.
        /// </value>
        [DataMember]
        public double ShadowPrice { get; set; }         //for ercot
        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        [DataMember]
        public int Hours { get; set; }                  //for ercot
        /// <summary>
        /// Gets or sets the participant.
        /// </summary>
        /// <value>
        /// The participant.
        /// </value>
        [DataMember]
        public string Participant { get; set; }
        /// <summary>
        /// Gets or sets the source node identifier.
        /// </summary>
        /// <value>
        /// The source node identifier.
        /// </value>
        [DataMember]
        public long SourceNodeId { get; set; }
        /// <summary>
        /// Gets or sets the sink node identifier.
        /// </summary>
        /// <value>
        /// The sink node identifier.
        /// </value>
        [DataMember]
        public long SinkNodeId { get; set; }
        /// <summary>
        /// Gets or sets the source zone.
        /// </summary>
        /// <value>
        /// The source zone.
        /// </value>
        [DataMember]
        public string SourceZone { get; set; }
        /// <summary>
        /// Gets or sets the sink zone.
        /// </summary>
        /// <value>
        /// The sink zone.
        /// </value>
        [DataMember]
        public string SinkZone { get; set; }
        /// <summary>
        /// Gets or sets the daily PNL.
        /// </summary>
        /// <value>
        /// The daily PNL.
        /// </value>
        [DataMember]
        public Dictionary<DateTime, double> dailyPNL { get; set; }
        /// <summary>
        /// Gets or sets the daily da price.
        /// </summary>
        /// <value>
        /// The daily da price.
        /// </value>
        [DataMember]
        public Dictionary<DateTime, double> dailyDAPrice { get; set; }
        /// <summary>
        /// Gets or sets the daily rt price.
        /// </summary>
        /// <value>
        /// The daily rt price.
        /// </value>
        [DataMember]
        public Dictionary<DateTime, double> dailyRTPrice { get; set; }
        /// <summary>
        /// Gets or sets the daily cost.
        /// </summary>
        /// <value>
        /// The daily cost.
        /// </value>
        [DataMember]
        public Dictionary<DateTime, double> dailyCost { get; set; }
        /// <summary>
        /// Gets or sets the PNL.
        /// </summary>
        /// <value>
        /// The PNL.
        /// </value>
        [DataMember]
        public Dictionary<DateTime, double> PNL { get; set; }
        /// <summary>
        /// Gets or sets the da price.
        /// </summary>
        /// <value>
        /// The da price.
        /// </value>
        [DataMember]
        public Dictionary<DateTime, double> DAPrice { get; set; }
        
        [DataMember]
        public Dictionary<DateTime, double> RTPrice { get; set; }
        
        [DataMember]
        public Dictionary<DateTime, double> Cost { get; set; }
        
        [DataMember]
        public double PNLmonthlytotal { get; set; }
        
        [DataMember]
        public double DAmonthlytotal { get; set; }
       
        [DataMember]
        public double RTmonthlytotal { get; set; }
       
        [DataMember]
        public double Costmonthlytotal { get; set; }
    }

    [DataContract]
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false,
    AllowMultiple = false)]
    [Serializable()]
    public class DailyValues : Attribute
    {
        [DataMember]
        public double? DA24Cong { get; set; }
        [DataMember]
        public double? DAPeakCong { get; set; }
        [DataMember]
        public double? DAOffPeakCong { get; set; }
        [DataMember]
        public double? DAPeakWECong { get; set; }
        [DataMember]
        public double? RT24Cong { get; set; }
        [DataMember]
        public double? RTPeakCong { get; set; }
        [DataMember]
        public double? RTOffPeakCong { get; set; }
        [DataMember]
        public double? RTPeakWECong { get; set; }
        [DataMember]
        public double? Price24 { get; set; }
        [DataMember]
        public double? PricePeak { get; set; }
        [DataMember]
        public double? PriceOffPeak { get; set; }
        [DataMember]
        public int PeakHours { get; set; }
        [DataMember]
        public int OffPeakHours { get; set; }
        [DataMember]
        public int PeakWEHours { get; set; }
        [DataMember]
        public int PeriodKey { get; set; }
        [DataMember]
        public double? PriceWE { get; set; }
        [DataMember]
        public int PeakWE { get; set; }
    }

    public class MonthlyValues : Attribute
    {
        public DateTime StartDate { get; set; } 
        [DataMember]
        public double? PricePeak { get; set; }
        [DataMember]
        public double? PriceOffPeak { get; set; }
        [DataMember]
        public int PeakHours { get; set; }
        [DataMember]
        public int OffPeakHours { get; set; }
        [DataMember]
        public int PeakWEHours { get; set; }
        [DataMember]
        public int PeriodKey { get; set; }
        [DataMember]
        public double? PriceWE { get; set; }
        [DataMember]
        public int PeakWE { get; set; }
        [DataMember]
        public double? DA24Cong { get; set; }
        [DataMember]
        public double? DAPeakCong { get; set; }
        [DataMember]
        public double? DAOffPeakCong { get; set; }
        [DataMember]
        public double? DAPeakWECong { get; set; }
        public double? DACrr { get; set; }

        public double? PricePeakYear1Month1 { get; set; }
        public double? PricePeakYear1Month2 { get; set; }
        public double? PricePeakYear1Month3 { get; set; }
        public double? PricePeakYear2Month1 { get; set; }
        public double? PricePeakYear2Month2 { get; set; }
        public double? PricePeakYear2Month3 { get; set; }
        public double? PricePeakYear3Month1 { get; set; }
        public double? PricePeakYear3Month2 { get; set; }
        public double? PricePeakYear3Month3 { get; set; }

        public double? PriceOffPeakYear1Month1 { get; set; }
        public double? PriceOffPeakYear1Month2 { get; set; }
        public double? PriceOffPeakYear1Month3 { get; set; }
        public double? PriceOffPeakYear2Month1 { get; set; }
        public double? PriceOffPeakYear2Month2 { get; set; }
        public double? PriceOffPeakYear2Month3 { get; set; }
        public double? PriceOffPeakYear3Month1 { get; set; }
        public double? PriceOffPeakYear3Month2 { get; set; }
        public double? PriceOffPeakYear3Month3 { get; set; }

        public double? PriceWEYear1Month1 { get; set; }
        public double? PriceWEYear1Month2 { get; set; }
        public double? PriceWEYear1Month3 { get; set; }
        public double? PriceWEYear2Month1 { get; set; }
        public double? PriceWEYear2Month2 { get; set; }
        public double? PriceWEYear2Month3 { get; set; }
        public double? PriceWEYear3Month1 { get; set; }
        public double? PriceWEYear3Month2 { get; set; }
        public double? PriceWEYear3Month3 { get; set; }

        public double? DAPeakCongYear1Month1 { get; set; }
        public double? DAPeakCongYear1Month2 { get; set; }
        public double? DAPeakCongYear1Month3 { get; set; }
        public double? DAPeakCongYear2Month1 { get; set; }
        public double? DAPeakCongYear2Month2 { get; set; }
        public double? DAPeakCongYear2Month3 { get; set; }
        public double? DAPeakCongYear3Month1 { get; set; }
        public double? DAPeakCongYear3Month2 { get; set; }
        public double? DAPeakCongYear3Month3 { get; set; }

        public double? DAOffPeakCongYear1Month1 { get; set; }
        public double? DAOffPeakCongYear1Month2 { get; set; }
        public double? DAOffPeakCongYear1Month3 { get; set; }
        public double? DAOffPeakCongYear2Month1 { get; set; }
        public double? DAOffPeakCongYear2Month2 { get; set; }
        public double? DAOffPeakCongYear2Month3 { get; set; }
        public double? DAOffPeakCongYear3Month1 { get; set; }
        public double? DAOffPeakCongYear3Month2 { get; set; }
        public double? DAOffPeakCongYear3Month3 { get; set; }

        public double? DAPeakWECongYear1Month1 { get; set; }
        public double? DAPeakWECongYear1Month2 { get; set; }
        public double? DAPeakWECongYear1Month3 { get; set; }
        public double? DAPeakWECongYear2Month1 { get; set; }
        public double? DAPeakWECongYear2Month2 { get; set; }
        public double? DAPeakWECongYear2Month3 { get; set; }
        public double? DAPeakWECongYear3Month1 { get; set; }
        public double? DAPeakWECongYear3Month2 { get; set; }
        public double? DAPeakWECongYear3Month3 { get; set; }

        
    }
}
