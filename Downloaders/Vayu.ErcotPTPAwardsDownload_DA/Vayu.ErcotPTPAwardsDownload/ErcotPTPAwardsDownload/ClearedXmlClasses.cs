/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "AwardedPTPObligation", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
    public class AwardedPTPObligation
    {
        [XmlElement(ElementName = "qse", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string Qse { get; set; }
        [XmlElement(ElementName = "startTime", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string StartTime { get; set; }
        [XmlElement(ElementName = "endTime", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string EndTime { get; set; }
        [XmlElement(ElementName = "tradingDate", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string TradingDate { get; set; }
        [XmlElement(ElementName = "awardedMW", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string AwardedMW { get; set; }
        [XmlElement(ElementName = "source", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string Source { get; set; }
        [XmlElement(ElementName = "sink", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string Sink { get; set; }
        [XmlElement(ElementName = "price", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string Price { get; set; }
        [XmlElement(ElementName = "bidId", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string BidId { get; set; }
    }

    [XmlRoot(ElementName = "AwardSet", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
    public class AwardSet
    {
        [XmlElement(ElementName = "tradingDate", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string TradingDate { get; set; }
        [XmlElement(ElementName = "AwardedPTPObligation", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public List<AwardedPTPObligation> AwardedPTPObligation { get; set; }
        [XmlAttribute(AttributeName = "ns0", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ns0 { get; set; }
    }

    [XmlRoot(ElementName = "root", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
    public class Root
    {
        [XmlElement(ElementName = "AwardSet", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public AwardSet AwardSet { get; set; }
        [XmlElement(ElementName = "Status", Namespace = "http://www.ercot.com/schema/2007-06/nodal/ews")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "ns1", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ns1 { get; set; }
        [XmlAttribute(AttributeName = "ns0", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ns0 { get; set; }
    }

}
