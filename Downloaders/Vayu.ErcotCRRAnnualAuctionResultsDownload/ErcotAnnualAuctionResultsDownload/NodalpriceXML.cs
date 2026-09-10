/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "shadowPrice")]
    public class ShadowPriceElement
    {
        [XmlElement(ElementName = "sourceSink")]
        public string SourceSink { get; set; }
        [XmlElement(ElementName = "calendarPeriod")]
        public string CalendarPeriod { get; set; }
        [XmlElement(ElementName = "tou")]
        public string Tou { get; set; }
        [XmlElement(ElementName = "shadowPrice")]
        public string ShadowPrice { get; set; }
    }

    [XmlRoot(ElementName = "shadowPrices")]
    public class ShadowPrices
    {
        [XmlElement(ElementName = "shadowPrice")]
        public List<ShadowPriceElement> ShadowPrice { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string NoNamespaceSchemaLocation { get; set; }
    }

}
