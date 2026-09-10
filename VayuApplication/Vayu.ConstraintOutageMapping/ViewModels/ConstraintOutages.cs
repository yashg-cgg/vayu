using System;

namespace Vayu.ConstraintOutageMapping.ViewModels
{
    public class ConstraintOutages
    {
        public string ConstraintName { get; set; }

        public string Contingency { get; set; }
        /// <summary>
        /// Gets or sets the name of the outage.
        /// </summary>
        /// <value>
        /// The name of the outage.
        /// </value>
        public string OutageName { get; set; }
        /// <summary>
        /// Gets or sets the ticket identifier.
        /// </summary>
        /// <value>
        /// The ticket identifier.
        /// </value>
        public string TicketID { get; set; }
        /// <summary>
        /// Gets or sets the branch.
        /// </summary>
        /// <value>
        /// The branch.
        /// </value>
        public string Branch { get; set; }
        /// <summary>
        /// Gets or sets to branch.
        /// </summary>
        /// <value>
        /// To branch.
        /// </value>
        public string ToBranch { get; set; }
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }
        /// <summary>
        /// Gets or sets the type of the equipment.
        /// </summary>
        /// <value>
        /// The type of the equipment.
        /// </value>
        public string EquipmentType { get; set; }
        /// <summary>
        /// Gets or sets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        public Decimal Voltage { get; set; }
        /// <summary>
        /// Gets or sets the outage status.
        /// </summary>
        /// <value>
        /// The outage status.
        /// </value>
        public string OutageStatus { get; set; }
        /// <summary>
        /// Gets or sets the startdate.
        /// </summary>
        /// <value>
        /// The startdate.
        /// </value>
        public DateTime? Startdate { get; set; }
        /// <summary>
        /// Gets or sets the enddate.
        /// </summary>
        /// <value>
        /// The enddate.
        /// </value>
        public DateTime? Enddate { get; set; }
        /// <summary>
        /// Gets or sets the duration of the outage.
        /// </summary>
        /// <value>
        /// The duration of the outage.
        /// </value>
        public int OutageDuration { get; set; }
        /// <summary>
        /// Gets or sets the type of the outage.
        /// </summary>
        /// <value>
        /// The type of the outage.
        /// </value>
        public string OutageType { get; set; }


    }
}
