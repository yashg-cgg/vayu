/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "result")]
    public class Result
    {
        [XmlElement(ElementName = "CRR_ID")]
        public string CRR_ID { get; set; }
        [XmlElement(ElementName = "ORI_CRR_ID")]
        public string ORI_CRR_ID { get; set; }
        [XmlElement(ElementName = "accountHolder")]
        public string AccountHolder { get; set; }
        [XmlElement(ElementName = "hedgeType")]
        public string HedgeType { get; set; }
        [XmlElement(ElementName = "bidType")]
        public string BidType { get; set; }
        [XmlElement(ElementName = "CRRType")]
        public string CRRType { get; set; }
        [XmlElement(ElementName = "source")]
        public string Source { get; set; }
        [XmlElement(ElementName = "sink")]
        public string Sink { get; set; }
        [XmlElement(ElementName = "startDate")]
        public string StartDate { get; set; }
        [XmlElement(ElementName = "endDate")]
        public string EndDate { get; set; }
        [XmlElement(ElementName = "timeOfUse")]
        public string TimeOfUse { get; set; }
        [XmlElement(ElementName = "bid24Hour")]
        public string Bid24Hour { get; set; }
        [XmlElement(ElementName = "MW")]
        public string MW { get; set; }
        [XmlElement(ElementName = "shadowPrice")]
        public string ShadowPrice { get; set; }
    }

    [XmlRoot(ElementName = "marketResults")]
    public class MarketResults
    {
        [XmlElement(ElementName = "result")]
        public List<Result> Result { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string NoNamespaceSchemaLocation { get; set; }
    }

}
