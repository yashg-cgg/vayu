using Microsoft.Maps.MapControl.WPF;
using System.ComponentModel;

namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class BranchModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The tobranch
        /// </summary>
        private string _tobranch;
        /// <summary>
        /// The branch
        /// </summary>
        private string _branch;
        /// <summary>
        /// The kv
        /// </summary>
        private int _kv;
        /// <summary>
        /// The tokv
        /// </summary>
        private int _tokv;
        /// <summary>
        /// The branchname
        /// </summary>
        private string _branchname;
        /// <summary>
        /// The devicetype
        /// </summary>
        private string _devicetype;

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
        /// Gets or sets the kv.
        /// </summary>
        /// <value>
        /// The kv.
        /// </value>
        public int kv
        {
            get { return _kv; }
            set
            {
                _kv = value;
                RaisePropertyChanged("kv");
            }
        }

        /// <summary>
        /// Gets or sets the tokv.
        /// </summary>
        /// <value>
        /// The tokv.
        /// </value>
        public int tokv
        {
            get { return _tokv; }
            set
            {
                _tokv = value;
                RaisePropertyChanged("tokv");
            }
        }

        /// <summary>
        /// Gets or sets the branchname.
        /// </summary>
        /// <value>
        /// The branchname.
        /// </value>
        public string branchname
        {
            get { return _branchname; }
            set
            {
                _branchname = value;
                RaisePropertyChanged("branchname");
            }
        }

        /// <summary>
        /// Gets or sets the branchzone.
        /// </summary>
        /// <value>
        /// The branchzone.
        /// </value>
        public string branchzone { get; set; }

        /// <summary>
        /// Gets or sets the tobranchzone.
        /// </summary>
        /// <value>
        /// The tobranchzone.
        /// </value>
        public string tobranchzone { get; set; }

        /// <summary>
        /// Gets or sets the devicetype.
        /// </summary>
        /// <value>
        /// The devicetype.
        /// </value>
        public string devicetype
        {
            get { return _devicetype; }
            set
            {
                _devicetype = value;
                RaisePropertyChanged("devicetype");
            }
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
        /// Gets or sets the marketkey.
        /// </summary>
        /// <value>
        /// The marketkey.
        /// </value>
        public int marketkey { get; set; }

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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class StationModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The branch
        /// </summary>
        private string _branch;
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
        /// Gets or sets the branch location.
        /// </summary>
        /// <value>
        /// The branch location.
        /// </value>
        public Location branch_location { get; set; }

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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.Maps.MapControl.WPF.MapPolyline" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class MyPolyline : MapPolyline, INotifyPropertyChanged
    {
        /// <summary>
        /// The branch
        /// </summary>
        private BranchModel _branch;

        /// <summary>
        /// Gets or sets the branch.
        /// </summary>
        /// <value>
        /// The branch.
        /// </value>
        public BranchModel branch
        {
            get { return _branch; }
            set
            {
                _branch = value;
                RaisePropertyChanged("branch");
            }
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
