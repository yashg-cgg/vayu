using Microsoft.Maps.MapControl.WPF;
using System;
using System.ComponentModel;
using Vayu.LatestConstraintsInformationLibrary;

namespace Vayu.PowerMap
{
    public class ConstraintInfo : INotifyPropertyChanged
    {
        private string _mcontingency;
        public string ContingencyName
        {
            get { return _mcontingency; }
            set
            {
                _mcontingency = value;
                RaisePropertyChanged("ContingencyName");
            }
        }

        private string _mconstraint;
        public string ConstraintName
        {
            get { return _mconstraint; }
            set
            {
                _mconstraint = value;
                RaisePropertyChanged("ConstraintName");
            }
        }

        private string mConstraintStationFrom;
        public string StationFrom
        {
            get
            {
                return mConstraintStationFrom;
            }
            set
            {
                mConstraintStationFrom = value;
                RaisePropertyChanged("StationFrom");
            }
        }

        private string mConstraintStationTo;
        public string StationTo
        {
            get
            {
                return mConstraintStationTo;
            }
            set
            {
                mConstraintStationTo = value;
                RaisePropertyChanged("StationTo");
            }
        }

        private double? mConstraintKV;
        public double? Constraint_KV
        {
            get
            {
                return mConstraintKV;
            }
            set
            {
                mConstraintKV = value;
                RaisePropertyChanged("constraint_KV");
            }
        }

        public int MarketKey { get; set; }
        public DateTime Marketdatetime { get; set; }
        public double Shadowprice { get; set; }

        private string mType;
        public string Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
                RaisePropertyChanged("Type");
            }
        }

        private Location _branch_location;
        public Location Fromlocation
        {
            get { return _branch_location; }
            set
            {
                _branch_location = value;
                RaisePropertyChanged("Fromlocation");
            }
        }

        private Location _tobranch_location;
        public Location ToLocation
        {
            get { return _tobranch_location; }
            set
            {
                _tobranch_location = value;
                RaisePropertyChanged("ToLocation");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConstraintInfo()
        {

        }

        public ConstraintInfo(LatestConstraint myConstraintobj) // to enable easier deep copy
        {
            ContingencyName = myConstraintobj.ContigencyText;
            ConstraintName = myConstraintobj.ConstraintText;
            //StationFrom = myConstraintobj.constraint_stationFrom;
            //StationTo = myConstraintobj.constraint_stationTo;
            //Constraint_KV = myConstraintobj.constraint_KV;
            //MarketKey = myConstraintobj.MarketKey;
            Marketdatetime = myConstraintobj.MarketDate;
            Shadowprice = myConstraintobj.ShadowPrice;
            //Type = myConstraintobj.GetType
            //Fromlocation = myConstraintobj.branch_location;
            //ToLocation = myConstraintobj.tobranch_location;
        }
    }

    public class Constraintobj : INotifyPropertyChanged
    {
        private string _mcontingency;
        public string contingency
        {
            get { return _mcontingency; }
            set
            {
                _mcontingency = value;
                RaisePropertyChanged("_contingency");
            }
        }
        private string _mconstraint;
        public string constraint
        {
            get { return _mconstraint; }
            set
            {
                _mconstraint = value;
                RaisePropertyChanged("constraint");
            }
        }
        private string mConstraintStationFrom;
        public string constraint_stationFrom
        {
            get
            {
                return mConstraintStationFrom;
            }
            set
            {
                mConstraintStationFrom = value;
                RaisePropertyChanged("constraint_stationFrom");
            }
        }
        private string mConstraintStationTo;
        public string constraint_stationTo
        {
            get
            {
                return mConstraintStationTo;
            }
            set
            {
                mConstraintStationTo = value;
                RaisePropertyChanged("constraint_stationTo");
            }
        }
        private double? mConstraintKV;
        public double? constraint_KV
        {
            get
            {
                return mConstraintKV;
            }
            set
            {
                mConstraintKV = value;
                RaisePropertyChanged("constraint_KV");
            }
        }
        public int MarketKey { get; set; }
        public DateTime marketdatetime { get; set; }
        public double shadowprice { get; set; }
        private string mType;
        public string type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
                RaisePropertyChanged("type");
            }
        }

        private Location _branch_location;
        public Location branch_location
        {
            get { return _branch_location; }
            set
            {
                _branch_location = value;
                RaisePropertyChanged("branch_location");
            }
        }
        private Location _tobranch_location;
        public Location tobranch_location
        {
            get { return _tobranch_location; }
            set
            {
                _tobranch_location = value;
                RaisePropertyChanged("tobranch_location");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Constraintobj()
        {

        }

        public Constraintobj(Constraintobj myConstraintobj) // to enable easier deep copy
        {
            contingency = myConstraintobj.contingency;
            constraint = myConstraintobj.constraint;
            constraint_stationFrom = myConstraintobj.constraint_stationFrom;
            constraint_stationTo = myConstraintobj.constraint_stationTo;
            constraint_KV = myConstraintobj.constraint_KV;
            MarketKey = myConstraintobj.MarketKey;
            marketdatetime = myConstraintobj.marketdatetime;
            shadowprice = myConstraintobj.shadowprice;
            type = myConstraintobj.type;
            branch_location = myConstraintobj.branch_location;
            tobranch_location = myConstraintobj.tobranch_location;

        }

        public Constraintobj(LatestConstraint myConstraintobj)
        {
            contingency = myConstraintobj.ContigencyText;
            constraint = myConstraintobj.ConstraintText;
            marketdatetime = myConstraintobj.MarketDate;
            shadowprice = myConstraintobj.ShadowPrice;
            type = myConstraintobj.ConstraintType;
        }
    }
}
