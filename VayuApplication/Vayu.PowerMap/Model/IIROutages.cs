using System;

namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    public class IIROutages
    {
        /// <summary>
        /// The outage identifier
        /// </summary>
        private string outageID;
        /// <summary>
        /// The unit name
        /// </summary>
        private string unitName;
        /// <summary>
        /// The unit identifier
        /// </summary>
        private string unitID;
        /// <summary>
        /// The owner name
        /// </summary>
        private string ownerName;
        /// <summary>
        /// The plant identifier
        /// </summary>
        private string plantID;
        /// <summary>
        /// The plant name
        /// </summary>
        private string plantName;
        /// <summary>
        /// The n ercr region
        /// </summary>
        private string nERCRRegion;
        /// <summary>
        /// The secondary fuel
        /// </summary>
        private string secondaryFuel;
        /// <summary>
        /// The fuel group
        /// </summary>
        private string fuelGroup;
        /// <summary>
        /// The output capacity
        /// </summary>
        private string outputCapacity;
        /// <summary>
        /// The o duration
        /// </summary>
        private int oDuration;
        /// <summary>
        /// The precision
        /// </summary>
        private string precision;
        /// <summary>
        /// The status
        /// </summary>
        private string status;
        /// <summary>
        /// The cap offline
        /// </summary>
        private double capOffline;
        /// <summary>
        /// The start date
        /// </summary>
        private DateTime? startDate;
        /// <summary>
        /// The end date
        /// </summary>
        private DateTime? endDate;
        /// <summary>
        /// The primary fuel
        /// </summary>
        private string primaryFuel;
        /// <summary>
        /// The power usage
        /// </summary>
        private string powerUsage;
        /// <summary>
        /// The type
        /// </summary>
        private string type;
        /// <summary>
        /// The delivery date
        /// </summary>
        private DateTime? deliveryDate;
        /// <summary>
        /// The n erc sub region
        /// </summary>
        private string nERCSubRegion;
        /// <summary>
        /// The unit type
        /// </summary>
        private string unitType;
        /// <summary>
        /// The heat rate
        /// </summary>
        private string heatRate;
        /// <summary>
        /// The ultimate owner identifier
        /// </summary>
        private string ultimateOwnerID;
        /// <summary>
        /// The ultimate owner
        /// </summary>
        private string ultimateOwner;
        /// <summary>
        /// The electric connection
        /// </summary>
        private string electricConnection;
        /// <summary>
        /// The trade region
        /// </summary>
        private string tradeRegion;
        /// <summary>
        /// The outage cause
        /// </summary>
        private string outageCause;
        /// <summary>
        /// The lattitude
        /// </summary>
        private double lattitude;
        /// <summary>
        /// The longitude
        /// </summary>
        private double longitude;
        /// <summary>
        /// The plant unit name
        /// </summary>
        private string plantUnitName;
        /// <summary>
        /// Gets or sets the outage identifier.
        /// </summary>
        /// <value>
        /// The outage identifier.
        /// </value>
        public string OutageID
        {
            get
            {
                return outageID;
            }
            set
            {
                outageID = value;
            }
        }
        /// <summary>
        /// Gets or sets the name of the unit.
        /// </summary>
        /// <value>
        /// The name of the unit.
        /// </value>
        public string UnitName
        {
            get
            {
                return unitName;
            }
            set
            {
                unitName = value;
            }
        }
        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        /// <value>
        /// The unit identifier.
        /// </value>
        public string UnitID
        {
            get
            {
                return unitID;
            }
            set
            {
                unitID = value;
            }
        }
        /// <summary>
        /// Gets or sets the name of the owner.
        /// </summary>
        /// <value>
        /// The name of the owner.
        /// </value>
        public string OwnerName
        {
            get
            {
                return ownerName;
            }
            set
            {
                ownerName = value;
            }
        }
        /// <summary>
        /// Gets or sets the plant identifier.
        /// </summary>
        /// <value>
        /// The plant identifier.
        /// </value>
        public string PlantID
        {
            get
            {
                return plantID;
            }
            set
            {
                plantID = value;
            }
        }
        /// <summary>
        /// Gets or sets the name of the plant.
        /// </summary>
        /// <value>
        /// The name of the plant.
        /// </value>
        public string PlantName
        {
            get
            {
                return plantName;
            }
            set
            {
                plantName = value;
            }
        }
        /// <summary>
        /// Gets or sets the nercr region.
        /// </summary>
        /// <value>
        /// The nercr region.
        /// </value>
        public string NERCRRegion
        {
            get
            {
                return nERCRRegion;
            }
            set
            {
                nERCRRegion = value;
            }
        }
        /// <summary>
        /// Gets or sets the primary fuel.
        /// </summary>
        /// <value>
        /// The primary fuel.
        /// </value>
        public string PrimaryFuel
        {
            get
            {
                return primaryFuel;
            }
            set
            {
                primaryFuel = value;
            }
        }
        /// <summary>
        /// Gets or sets the secondary fuel.
        /// </summary>
        /// <value>
        /// The secondary fuel.
        /// </value>
        public string SecondaryFuel
        {
            get
            {
                return secondaryFuel;
            }
            set
            {
                secondaryFuel = value;
            }
        }
        /// <summary>
        /// Gets or sets the fuel group.
        /// </summary>
        /// <value>
        /// The fuel group.
        /// </value>
        public string FuelGroup
        {
            get
            {
                return fuelGroup;
            }
            set
            {
                fuelGroup = value;
            }
        }
        /// <summary>
        /// Gets or sets the output capacity.
        /// </summary>
        /// <value>
        /// The output capacity.
        /// </value>
        public string OutputCapacity
        {
            get
            {
                return outputCapacity;
            }
            set
            {
                outputCapacity = value;
            }
        }
        /// <summary>
        /// Gets or sets the duration of the o.
        /// </summary>
        /// <value>
        /// The duration of the o.
        /// </value>
        public int ODuration
        {
            get
            {
                return oDuration;
            }
            set
            {
                oDuration = value;
            }
        }
        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>
        /// The precision.
        /// </value>
        public string Precision
        {
            get
            {
                return precision;
            }
            set
            {
                precision = value;
            }
        }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
        /// <summary>
        /// Gets or sets the nerc sub region.
        /// </summary>
        /// <value>
        /// The nerc sub region.
        /// </value>
        public string NERCSubRegion
        {
            get
            {
                return nERCSubRegion;
            }
            set
            {
                nERCSubRegion = value;
            }
        }
        /// <summary>
        /// Gets or sets the type of the unit.
        /// </summary>
        /// <value>
        /// The type of the unit.
        /// </value>
        public string UnitType
        {
            get
            {
                return unitType;
            }
            set
            {
                unitType = value;
            }
        }
        /// <summary>
        /// Gets or sets the heat rate.
        /// </summary>
        /// <value>
        /// The heat rate.
        /// </value>
        public string HeatRate
        {
            get
            {
                return heatRate;
            }
            set
            {
                heatRate = value;
            }
        }
        /// <summary>
        /// Gets or sets the ultimate owner identifier.
        /// </summary>
        /// <value>
        /// The ultimate owner identifier.
        /// </value>
        public string UltimateOwnerID
        {
            get
            {
                return ultimateOwnerID;
            }
            set
            {
                ultimateOwnerID = value;
            }
        }
        /// <summary>
        /// Gets or sets the ultimate owner.
        /// </summary>
        /// <value>
        /// The ultimate owner.
        /// </value>
        public string UltimateOwner
        {
            get
            {
                return ultimateOwner;
            }
            set
            {
                ultimateOwner = value;
            }
        }
        /// <summary>
        /// Gets or sets the electric connection.
        /// </summary>
        /// <value>
        /// The electric connection.
        /// </value>
        public string ElectricConnection
        {
            get
            {
                return electricConnection;
            }
            set
            {
                electricConnection = value;
            }
        }
        /// <summary>
        /// Gets or sets the trade region.
        /// </summary>
        /// <value>
        /// The trade region.
        /// </value>
        public string TradeRegion
        {
            get
            {
                return tradeRegion;
            }
            set
            {
                tradeRegion = value;
            }
        }
        /// <summary>
        /// Gets or sets the outage cause.
        /// </summary>
        /// <value>
        /// The outage cause.
        /// </value>
        public string OutageCause
        {
            get
            {
                return outageCause;
            }
            set
            {
                outageCause = value;
            }
        }
        /// <summary>
        /// Gets or sets the cap offline.
        /// </summary>
        /// <value>
        /// The cap offline.
        /// </value>
        public double CapOffline
        {
            get
            {
                return capOffline;
            }
            set
            {
                capOffline = value;
            }
        }
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                endDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the delivery date.
        /// </summary>
        /// <value>
        /// The delivery date.
        /// </value>
        public DateTime? DeliveryDate
        {
            get
            {
                return deliveryDate;
            }
            set
            {
                deliveryDate = value;
            }
        }
        /// <summary>
        /// Gets or sets the power usage.
        /// </summary>
        /// <value>
        /// The power usage.
        /// </value>
        public string PowerUsage
        {
            get
            {
                return powerUsage;
            }
            set
            {
                powerUsage = value;
            }
        }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        /// <summary>
        /// Gets or sets the lattitude.
        /// </summary>
        /// <value>
        /// The lattitude.
        /// </value>
        public double Lattitude
        {
            get
            {
                return lattitude;
            }
            set
            {
                lattitude = value;
            }
        }
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                longitude = value;
            }
        }
        /// <summary>
        /// Gets or sets the name of the plant unit.
        /// </summary>
        /// <value>
        /// The name of the plant unit.
        /// </value>
        public string PlantUnitName
        {
            get
            {
                return plantUnitName;
            }
            set
            {
                plantUnitName = value;
            }
        }

    }
}
