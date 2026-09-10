using System;

namespace Vayu.Outage_Constraint_History.Model
{
    public class Daily
    {
        public string Outages { get; set; }
        public string Constraint { get; set; }
        public int OutageCurrentMonth { get; set; }
        public int ConstraintNextDay { get; set; }
        public int ConstraintNext10Day { get; set; }
        public int ConstraintNext30Day { get; set; }
    }
    public class OutagesDetails
    {
        public string Outages { get; set; }
        public DateTime MarketDateTime { get; set; }
    }
    public class ConstraintDetails
    {
        public string Constraint { get; set; }
        public DateTime MarketDateTime { get; set; }
    }
    public class ConstraintOutages
    {
        public string Constraint { get; set; }
        public string Outages { get; set; }
    }
}
