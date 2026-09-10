/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "sourceSink", Namespace = "http://crr.ercot.org/download/xml")]
    public class SourceSinkData
    {
        [XmlElement(ElementName = "sourceSink", Namespace = "http://crr.ercot.org/download/xml")]
        public string SourceSink { get; set; }
        [XmlElement(ElementName = "calendarPeriod", Namespace = "http://crr.ercot.org/download/xml")]
        public string CalendarPeriod { get; set; }
        [XmlElement(ElementName = "tou", Namespace = "http://crr.ercot.org/download/xml")]
        public string Tou { get; set; }
        [XmlElement(ElementName = "shadowPrice", Namespace = "http://crr.ercot.org/download/xml")]
        public string ShadowPrice { get; set; }
    }

    [XmlRoot(ElementName = "shadowPrices", Namespace = "http://crr.ercot.org/download/xml")]
    public class HistoricalShadowPrices
    {
        [XmlElement(ElementName = "sourceSink", Namespace = "http://crr.ercot.org/download/xml")]
        public List<SourceSinkData> SourceSink { get; set; }
        [XmlAttribute(AttributeName = "CRRDownload", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string CRRDownload { get; set; }
    }

}
