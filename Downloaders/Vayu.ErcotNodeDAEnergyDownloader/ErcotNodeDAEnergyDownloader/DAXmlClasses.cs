/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "DAMDeEnergizedStlPnt", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
    public class DAMDeEnergizedStlPnt
    {
        [XmlElement(ElementName = "DeliveryDate", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DeliveryDate { get; set; }
        [XmlElement(ElementName = "HourEnding", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string HourEnding { get; set; }
        [XmlElement(ElementName = "SettlementPoint", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string SettlementPoint { get; set; }
        [XmlElement(ElementName = "DSTFlag", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DSTFlag { get; set; }
    }

    [XmlRoot(ElementName = "DAMDeEnergizedStlPnts", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
    public class DAMDeEnergizedStlPnts
    {
        [XmlElement(ElementName = "DAMDeEnergizedStlPnt", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public List<DAMDeEnergizedStlPnt> DAMDeEnergizedStlPnt { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

}
