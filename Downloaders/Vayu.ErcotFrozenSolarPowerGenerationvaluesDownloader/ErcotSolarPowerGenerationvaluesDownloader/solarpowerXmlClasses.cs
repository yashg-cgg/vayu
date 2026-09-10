/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "SolarPowerProductionHourlyAverageActualForecastedValue", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
    public class SolarPowerProductionHourlyAverageActualForecastedValue
    {
        [XmlElement(ElementName = "DELIVERY_DATE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DELIVERY_DATE { get; set; }
        [XmlElement(ElementName = "HOUR_ENDING", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string HOUR_ENDING { get; set; }
        [XmlElement(ElementName = "ACTUAL_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string ACTUAL_SYSTEM_WIDE { get; set; }
        [XmlElement(ElementName = "COP_HSL_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string COP_HSL_SYSTEM_WIDE { get; set; }
        [XmlElement(ElementName = "STPPF_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string STPPF_SYSTEM_WIDE { get; set; }
        [XmlElement(ElementName = "PVGRPP_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string PVGRPP_SYSTEM_WIDE { get; set; }
        [XmlElement(ElementName = "DSTFlag", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public string DSTFlag { get; set; }
    }

    [XmlRoot(ElementName = "SolarPowerProductionHourlyAverageActualForecastedValues", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
    public class SolarPowerProductionHourlyAverageActualForecastedValues
    {
        [XmlElement(ElementName = "SolarPowerProductionHourlyAverageActualForecastedValue", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
        public List<SolarPowerProductionHourlyAverageActualForecastedValue> SolarPowerProductionHourlyAverageActualForecastedValue { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

}
