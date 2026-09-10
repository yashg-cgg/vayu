/* 
 Licensed under the Apache License, Version 2.0
    
 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
	[XmlRoot(ElementName = "WindPowerProductionHourlyAverageActualForecastedValue", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
	public class WindPowerProductionHourlyAverageActualForecastedValue
	{
		[XmlElement(ElementName = "DELIVERY_DATE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string DELIVERY_DATE { get; set; }

		[XmlElement(ElementName = "HOUR_ENDING", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string HOUR_ENDING { get; set; }

		//[XmlElement(ElementName = "ACTUAL_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		[XmlElement(ElementName = "SYSTEM_WIDE_GEN", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_SYSTEM_WIDE { get; set; }

		[XmlElement(ElementName = "COP_HSL_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_SYSTEM_WIDE { get; set; }

		[XmlElement(ElementName = "STWPF_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_SYSTEM_WIDE { get; set; }

		[XmlElement(ElementName = "WGRPP_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_SYSTEM_WIDE { get; set; }

		//[XmlElement(ElementName = "ACTUAL_LZ_SOUTH_HOUSTON", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		[XmlElement(ElementName = "GEN_LZ_SOUTH_HOUSTON", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_LZ_SOUTH_HOUSTON { get; set; }

		[XmlElement(ElementName = "COP_HSL_LZ_SOUTH_HOUSTON", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_LZ_SOUTH_HOUSTON { get; set; }

		[XmlElement(ElementName = "STWPF_LZ_SOUTH_HOUSTON", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_LZ_SOUTH_HOUSTON { get; set; }

		[XmlElement(ElementName = "WGRPP_LZ_SOUTH_HOUSTON", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_LZ_SOUTH_HOUSTON { get; set; }

		//[XmlElement(ElementName = "ACTUAL_LZ_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		[XmlElement(ElementName = "GEN_LZ_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_LZ_WEST { get; set; }

		[XmlElement(ElementName = "COP_HSL_LZ_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_LZ_WEST { get; set; }

		[XmlElement(ElementName = "STWPF_LZ_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_LZ_WEST { get; set; }

		[XmlElement(ElementName = "WGRPP_LZ_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_LZ_WEST { get; set; }

		//[XmlElement(ElementName = "ACTUAL_LZ_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		[XmlElement(ElementName = "GEN_LZ_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_LZ_NORTH { get; set; }

		[XmlElement(ElementName = "COP_HSL_LZ_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_LZ_NORTH { get; set; }

		[XmlElement(ElementName = "STWPF_LZ_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_LZ_NORTH { get; set; }

		[XmlElement(ElementName = "WGRPP_LZ_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_LZ_NORTH { get; set; }

		[XmlElement(ElementName = "DSTFlag", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string DSTFlag { get; set; }
	}
	[XmlRoot(ElementName = "WindPowerProductionHourlyAverageActualForecastedValues", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
	public class WindPowerProductionHourlyAverageActualForecastedValues
	{
		[XmlElement(ElementName = "WindPowerProductionHourlyAverageActualForecastedValue", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public List<WindPowerProductionHourlyAverageActualForecastedValue> WindPowerProductionHourlyAverageActualForecastedValue { get; set; }
		[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }
	}

	[XmlRoot(ElementName = "WindPowerProductionHourlyAverageActualForecastedGeoRegionValue", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
	public class WindPowerProductionHourlyAverageActualForecastedGeoRegionValue
	{
		[XmlElement(ElementName = "ACTUAL_COASTAL", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_COASTAL { get; set; }
		[XmlElement(ElementName = "ACTUAL_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_NORTH { get; set; }
		[XmlElement(ElementName = "ACTUAL_PANHANDLE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_PANHANDLE { get; set; }
		[XmlElement(ElementName = "ACTUAL_SOUTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_SOUTH { get; set; }
		[XmlElement(ElementName = "ACTUAL_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_SYSTEM_WIDE { get; set; }
		[XmlElement(ElementName = "ACTUAL_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string ACTUAL_WEST { get; set; }
		[XmlElement(ElementName = "COP_HSL_COASTAL", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_COASTAL { get; set; }
		[XmlElement(ElementName = "COP_HSL_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_NORTH { get; set; }
		[XmlElement(ElementName = "COP_HSL_PANHANDLE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_PANHANDLE { get; set; }
		[XmlElement(ElementName = "COP_HSL_SOUTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_SOUTH { get; set; }
		[XmlElement(ElementName = "COP_HSL_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_SYSTEM_WIDE { get; set; }
		[XmlElement(ElementName = "COP_HSL_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string COP_HSL_WEST { get; set; }
		[XmlElement(ElementName = "DELIVERY_DATE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string DELIVERY_DATE { get; set; }
		[XmlElement(ElementName = "DSTFlag", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string DSTFlag { get; set; }
		[XmlElement(ElementName = "HOUR_ENDING", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string HOUR_ENDING { get; set; }
		[XmlElement(ElementName = "STWPF_COASTAL", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_COASTAL { get; set; }
		[XmlElement(ElementName = "STWPF_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_NORTH { get; set; }
		[XmlElement(ElementName = "STWPF_PANHANDLE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_PANHANDLE { get; set; }
		[XmlElement(ElementName = "STWPF_SOUTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_SOUTH { get; set; }
		[XmlElement(ElementName = "STWPF_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_SYSTEM_WIDE { get; set; }
		[XmlElement(ElementName = "STWPF_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string STWPF_WEST { get; set; }
		[XmlElement(ElementName = "WGRPP_COASTAL", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_COASTAL { get; set; }
		[XmlElement(ElementName = "WGRPP_NORTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_NORTH { get; set; }
		[XmlElement(ElementName = "WGRPP_PANHANDLE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_PANHANDLE { get; set; }
		[XmlElement(ElementName = "WGRPP_SOUTH", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_SOUTH { get; set; }
		[XmlElement(ElementName = "WGRPP_SYSTEM_WIDE", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_SYSTEM_WIDE { get; set; }
		[XmlElement(ElementName = "WGRPP_WEST", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public string WGRPP_WEST { get; set; }
	}

	[XmlRoot(ElementName = "WindPowerProductionHourlyAverageActualForecastedGeoRegionValues", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
	public class WindPowerProductionHourlyAverageActualForecastedGeoRegionValues
	{
		[XmlElement(ElementName = "WindPowerProductionHourlyAverageActualForecastedGeoRegionValue", Namespace = "http://www.ercot.com/schema/2009-01/nodal/cdr")]
		public List<WindPowerProductionHourlyAverageActualForecastedGeoRegionValue> WindPowerProductionHourlyAverageActualForecastedGeoRegionValue { get; set; }
		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }
		[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
	}


}
