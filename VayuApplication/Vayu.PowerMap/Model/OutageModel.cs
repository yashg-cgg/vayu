using Microsoft.Maps.MapControl.WPF;
using System;
using System.ComponentModel;

namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class Outage : INotifyPropertyChanged
    {
        /// <summary>
        /// The enddate
        /// </summary>
        private DateTime? _enddate = null;
        /// <summary>
        /// The plannedstart
        /// </summary>
        private DateTime? _plannedstart = null;
        /// <summary>
        /// The plannedend
        /// </summary>
        private DateTime? _plannedend = null;
        /// <summary>
        /// The lastrevised
        /// </summary>
        private DateTime? _lastrevised = null;
        /// <summary>
        /// The startdate
        /// </summary>
        private DateTime? _startdate = null;
        /// <summary>
        /// The openclose
        /// </summary>
        public int? _openclose = null;
        /// <summary>
        /// The outagestatus
        /// </summary>
        public string _outagestatus = null;
        /// <summary>
        /// The outage type
        /// </summary>
        public string _outageType = null;
        /// <summary>
        /// The causes
        /// </summary>
        public string _causes = null;
        /// <summary>
        /// The type
        /// </summary>
        private string _type;
        /// <summary>
        /// The equipment
        /// </summary>
        private string _equipment;
        /// <summary>
        /// The voltage
        /// </summary>
        private int _voltage;
        /// <summary>
        /// The equipment type
        /// </summary>
        private string _equipmentType;
        /// <summary>
        /// The zone
        /// </summary>
        private string _zone;
        /// <summary>
        /// The tobranch
        /// </summary>
        private string _tobranch;
        /// <summary>
        /// The branch
        /// </summary>
        private string _branch;
        /// <summary>
        /// The ticket identifier
        /// </summary>
        private string _ticketId;
        /// <summary>
        /// The plannedduration
        /// </summary>
        private int _plannedduration;

        /// <summary>
        /// Gets or sets the ticket identifier.
        /// </summary>
        /// <value>
        /// The ticket identifier.
        /// </value>
        public string ticketId
        {
            get { return _ticketId; }
            set { _ticketId = value; }
        }

        /// <summary>
        /// Gets or sets the branch.
        /// </summary>
        /// <value>
        /// The branch.
        /// </value>
        public string branch
        {
            get { return _branch; }
            set
            {
                _branch = value;
                RaisePropertyChanged("branch");
            }
        }

        /// <summary>
        /// Gets or sets the tobranch.
        /// </summary>
        /// <value>
        /// The tobranch.
        /// </value>
        public string tobranch
        {
            get { return _tobranch; }
            set
            {
                _tobranch = value;
                RaisePropertyChanged("tobranch");
            }
        }

        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                RaisePropertyChanged("zone");
            }
        }

        //public int facilityID { get; set; }
        /// <summary>
        /// Gets or sets the type of the equipment.
        /// </summary>
        /// <value>
        /// The type of the equipment.
        /// </value>
        public string equipmentType
        {
            get { return _equipmentType; }
            set { _equipmentType = value; }
        }

        /// <summary>
        /// Gets or sets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        public int voltage
        {
            get { return _voltage; }
            set { _voltage = value; }
        }

        /// <summary>
        /// Gets or sets the equipment.
        /// </summary>
        /// <value>
        /// The equipment.
        /// </value>
        public string equipment
        {
            get { return _equipment; }
            set { _equipment = value; }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the startdate.
        /// </summary>
        /// <value>
        /// The startdate.
        /// </value>
        public DateTime? startdate
        {
            get { return _startdate; }
            set { _startdate = value; }
        }

        /// <summary>
        /// Gets or sets the enddate.
        /// </summary>
        /// <value>
        /// The enddate.
        /// </value>
        public DateTime? enddate { get { return _enddate; } set { _enddate = value; } }

        /// <summary>
        /// Gets or sets the plannedstart.
        /// </summary>
        /// <value>
        /// The plannedstart.
        /// </value>
        public DateTime? plannedstart { get { return _plannedstart; } set { _plannedstart = value; } }

        /// <summary>
        /// Gets or sets the plannedend.
        /// </summary>
        /// <value>
        /// The plannedend.
        /// </value>
        public DateTime? plannedend { get { return _plannedend; } set { _plannedend = value; } }

        /// <summary>
        /// Gets or sets the openclose.
        /// </summary>
        /// <value>
        /// The openclose.
        /// </value>
        public int? openclose { get; set; }

        /// <summary>
        /// Gets or sets the outagestatus.
        /// </summary>
        /// <value>
        /// The outagestatus.
        /// </value>
        public string outagestatus
        {
            get { return _outagestatus; }
            set { _outagestatus = value; }
        }

        /// <summary>
        /// Gets or sets the lastrevised.
        /// </summary>
        /// <value>
        /// The lastrevised.
        /// </value>
        public DateTime? lastrevised { get { return _lastrevised; } set { _lastrevised = value; } }

        /// <summary>
        /// Gets or sets the type of the outage.
        /// </summary>
        /// <value>
        /// The type of the outage.
        /// </value>
        public string outageType
        {
            get { return _outageType; }
            set { _outageType = value; }
        }

        /// <summary>
        /// Gets or sets the causes.
        /// </summary>
        /// <value>
        /// The causes.
        /// </value>
        public string causes
        {
            get { return _causes; }
            set { _causes = value; }
        }

        /// <summary>
        /// Gets or sets the branch location.
        /// </summary>
        /// <value>
        /// The branch location.
        /// </value>
        public Location branch_location { get; set; }

        /// <summary>
        /// Gets or sets the tobranch location.
        /// </summary>
        /// <value>
        /// The tobranch location.
        /// </value>
        public Location tobranch_location { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string status { get; set; }

        /// <summary>
        /// Gets or sets the removeddate.
        /// </summary>
        /// <value>
        /// The removeddate.
        /// </value>
        public DateTime removeddate { get; set; }

        /// <summary>
        /// Gets or sets the plannedduration.
        /// </summary>
        /// <value>
        /// The plannedduration.
        /// </value>
        public int plannedduration
        {
            get { return _plannedduration; }
            set { _plannedduration = value; }
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
