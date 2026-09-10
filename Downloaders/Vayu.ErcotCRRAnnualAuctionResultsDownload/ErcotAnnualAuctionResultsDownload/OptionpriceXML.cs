/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "crr")]
    public class Crr
    {
        [XmlElement(ElementName = "source")]
        public string Source { get; set; }
        [XmlElement(ElementName = "sink")]
        public string Sink { get; set; }
        [XmlElement(ElementName = "bidType")]
        public string BidType { get; set; }
        [XmlElement(ElementName = "startDate")]
        public string StartDate { get; set; }
        [XmlElement(ElementName = "endDate")]
        public string EndDate { get; set; }
        [XmlElement(ElementName = "hedgeType")]
        public string HedgeType { get; set; }
        [XmlElement(ElementName = "tou")]
        public string Tou { get; set; }
        [XmlElement(ElementName = "MW")]
        public string MW { get; set; }
        [XmlElement(ElementName = "BidPrice")]
        public string BidPrice { get; set; }
        [XmlElement(ElementName = "shadowPrice")]
        public string ShadowPrice { get; set; }
    }

    [XmlRoot(ElementName = "auctionCRRs")]
    public class AuctionCRRs
    {
        [XmlElement(ElementName = "crr")]
        public List<Crr> Crr { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string NoNamespaceSchemaLocation { get; set; }
    }

}


///* 
//  Licensed under the Apache License, Version 2.0

//  http://www.apache.org/licenses/LICENSE-2.0
//  */
//using System;
//using System.Xml.Serialization;
//using System.Collections.Generic;
//namespace Xml2CSharp
//{
//	[XmlRoot(ElementName = "crr", Namespace = "http://crr.ercot.org/download/xml")]
//	public class Crr
//	{
//		[XmlElement(ElementName = "category", Namespace = "http://crr.ercot.org/download/xml")]
//		public string Category { get; set; }
//		[XmlElement(ElementName = "source", Namespace = "http://crr.ercot.org/download/xml")]
//		public string Source { get; set; }
//		[XmlElement(ElementName = "sink", Namespace = "http://crr.ercot.org/download/xml")]
//		public string Sink { get; set; }
//		[XmlElement(ElementName = "flowgate", Namespace = "http://crr.ercot.org/download/xml")]
//		public string Flowgate { get; set; }
//		[XmlElement(ElementName = "type", Namespace = "http://crr.ercot.org/download/xml")]
//		public string Type { get; set; }
//		[XmlElement(ElementName = "startDate", Namespace = "http://crr.ercot.org/download/xml")]
//		public string StartDate { get; set; }
//		[XmlElement(ElementName = "endDate", Namespace = "http://crr.ercot.org/download/xml")]
//		public string EndDate { get; set; }
//		[XmlElement(ElementName = "hedgeType", Namespace = "http://crr.ercot.org/download/xml")]
//		public string HedgeType { get; set; }
//		[XmlElement(ElementName = "tou", Namespace = "http://crr.ercot.org/download/xml")]
//		public string Tou { get; set; }
//		[XmlElement(ElementName = "MW", Namespace = "http://crr.ercot.org/download/xml")]
//		public string MW { get; set; }
//		[XmlElement(ElementName = "BidPrice", Namespace = "http://crr.ercot.org/download/xml")]
//		public string BidPrice { get; set; }
//		[XmlElement(ElementName = "shadowPrice", Namespace = "http://crr.ercot.org/download/xml")]
//		public string ShadowPrice { get; set; }
//	}

//	[XmlRoot(ElementName = "auctionCRRs", Namespace = "http://crr.ercot.org/download/xml")]
//	public class AuctionCRRs
//	{
//		[XmlElement(ElementName = "crr", Namespace = "http://crr.ercot.org/download/xml")]
//		public List<Crr> Crr { get; set; }
//		[XmlAttribute(AttributeName = "CRRDownload", Namespace = "http://www.w3.org/2000/xmlns/")]
//		public string CRRDownload { get; set; }
//	}

//}
