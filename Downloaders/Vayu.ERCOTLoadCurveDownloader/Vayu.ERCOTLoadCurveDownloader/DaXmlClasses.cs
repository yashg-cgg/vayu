/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "LFvsActualReport", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
    public class LFvsActualReport
    {
        [XmlElement(ElementName = "DeliveryDate", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DeliveryDate { get; set; }
        [XmlElement(ElementName = "HourEnding", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string HourEnding { get; set; }
        [XmlElement(ElementName = "CurrentDayForecast", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string CurrentDayForecast { get; set; }
        [XmlElement(ElementName = "DayAheadForecast", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DayAheadForecast { get; set; }
        [XmlElement(ElementName = "ActualLoad", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string ActualLoad { get; set; }
        [XmlElement(ElementName = "DayAheadHSL", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DayAheadHSL { get; set; }
        [XmlElement(ElementName = "CurrentDayHSL", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string CurrentDayHSL { get; set; }
        [XmlElement(ElementName = "DSTFlag", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DSTFlag { get; set; }
    }

    [XmlRoot(ElementName = "LoadForecastVsActuals", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
    public class LoadForecastVsActuals
    {
        [XmlElement(ElementName = "LFvsActualReport", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public List<LFvsActualReport> LFvsActualReport { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

}
