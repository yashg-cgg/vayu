/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "result", Namespace = "http://crr.ercot.org/download/xml")]
    public class AuctionResult
    {
        [XmlElement(ElementName = "CRR_ID", Namespace = "http://crr.ercot.org/download/xml")]
        public string CRR_ID { get; set; }
        [XmlElement(ElementName = "ORI_CRR_ID", Namespace = "http://crr.ercot.org/download/xml")]
        public string ORI_CRR_ID { get; set; }
        [XmlElement(ElementName = "accountHolder", Namespace = "http://crr.ercot.org/download/xml")]
        public string AccountHolder { get; set; }
        [XmlElement(ElementName = "category", Namespace = "http://crr.ercot.org/download/xml")]
        public string Category { get; set; }
        [XmlElement(ElementName = "hedgeType", Namespace = "http://crr.ercot.org/download/xml")]
        public string HedgeType { get; set; }
        [XmlElement(ElementName = "class", Namespace = "http://crr.ercot.org/download/xml")]
        public string Class { get; set; }
        [XmlElement(ElementName = "CRRType", Namespace = "http://crr.ercot.org/download/xml")]
        public string CRRType { get; set; }
        [XmlElement(ElementName = "source", Namespace = "http://crr.ercot.org/download/xml")]
        public string Source { get; set; }
        [XmlElement(ElementName = "sink", Namespace = "http://crr.ercot.org/download/xml")]
        public string Sink { get; set; }
        [XmlElement(ElementName = "flowgate", Namespace = "http://crr.ercot.org/download/xml")]
        public string Flowgate { get; set; }
        [XmlElement(ElementName = "startDate", Namespace = "http://crr.ercot.org/download/xml")]
        public string StartDate { get; set; }
        [XmlElement(ElementName = "endDate", Namespace = "http://crr.ercot.org/download/xml")]
        public string EndDate { get; set; }
        [XmlElement(ElementName = "timeOfUse", Namespace = "http://crr.ercot.org/download/xml")]
        public string TimeOfUse { get; set; }
        [XmlElement(ElementName = "bid24Hour", Namespace = "http://crr.ercot.org/download/xml")]
        public string Bid24Hour { get; set; }
        [XmlElement(ElementName = "MW", Namespace = "http://crr.ercot.org/download/xml")]
        public string MW { get; set; }
        [XmlElement(ElementName = "shadowPrice", Namespace = "http://crr.ercot.org/download/xml")]
        public string ShadowPrice { get; set; }
    }

    [XmlRoot(ElementName = "marketResults", Namespace = "http://crr.ercot.org/download/xml")]
    public class AuctionMarketResults
    {
        [XmlElement(ElementName = "result", Namespace = "http://crr.ercot.org/download/xml")]
        public List<AuctionResult> Result { get; set; }
        [XmlAttribute(AttributeName = "CRRDownload", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string CRRDownload { get; set; }
    }

}
