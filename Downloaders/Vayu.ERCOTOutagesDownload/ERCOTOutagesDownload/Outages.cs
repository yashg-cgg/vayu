using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vayu.ERCOTOutagesDownload
{
    class Outages
    {
        public string OutageIdentifier { get; set;}
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; } 
        public DateTime? ActualEndDate { get; set; }
        public string OutageStatus { get; set; }
        public string RequestorOrgName { get; set; }
        public string EquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentFromStationName { get; set; }
        public string EquipmentToStationName { get; set; }
        public decimal VoltageLevel { get; set; }
        public DateTime? SubmitTime { get; set; }
        public string OutageType { get; set; }
        public DateTime? RemovedDate { get; set; }
        public DateTime? RevisedDate { get; set; }
        //
        public string BreakerSwitchNormalStatus { get; set; }
        public string BreakerSwitchOutageStatus { get; set; }
         public string RequestorLongName { get; set; }
        public string NatureOfWork { get; set; }
        public int TEID { get; set; }
        public int RestorationTime { get; set; }
        public string ReasonForCancellation { get; set; }
        public DateTime? CancellationDate  { get; set; }
        public string GroupLabel { get; set; }


    }
}
