using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vayu.NodePriceLibrary;

namespace Vayu.PowerMap
{
    #region placemark class and point class

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class point : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int? NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the LMP.
        /// </summary>
        /// <value>
        /// The LMP.
        /// </value>
        public double LMP { get; set; }
        /// <summary>
        /// Gets or sets the kv.
        /// </summary>
        /// <value>
        /// The kv.
        /// </value>
        public int? KV { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string FuelType { get; set; }
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }
        /// <summary>
        /// Gets or sets the market date time.
        /// </summary>
        /// <value>
        /// The market date time.
        /// </value>
        public DateTime MarketDateTime { get; set; }
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public placemark Parent { get; set; }

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
    public class placemark
    {
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double Longitude { get; set; }
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double Latitude { get; set; }
        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        public int NodeType { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the station.
        /// </summary>
        /// <value>
        /// The station.
        /// </value>
        public string Station { get; set; }
        /// <summary>
        /// Gets or sets the points.
        /// </summary>
        /// <value>
        /// The points.
        /// </value>
        public List<point> points { get; set; }
        /// <summary>
        /// Gets or sets the marketkey.
        /// </summary>
        /// <value>
        /// The marketkey.
        /// </value>
        public int marketkey { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class Node_Geo : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the node key.
        /// </summary>
        /// <value>
        /// The node key.
        /// </value>
        public int? NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double Longitude { get; set; }
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double Latitude { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        /// 
        public string FuelType { get; set; }
        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>

        public int NodeType { get; set; }
        /// <summary>
        /// Gets or sets the kv.
        /// </summary>
        /// <value>
        /// The kv.
        /// </value>
        public int? KV { get; set; }
        /// <summary>
        /// Gets or sets the PSS ename.
        /// </summary>
        /// <value>
        /// The PSS ename.
        /// </value>
        public string PSSEname { get; set; }
        /// <summary>
        /// Gets or sets the marketkey.
        /// </summary>
        /// <value>
        /// The marketkey.
        /// </value>
        public int Marketkey { get; set; }
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }
        /// <summary>
        /// Gets or sets the pnode identifier.
        /// </summary>
        /// <value>
        /// The pnode identifier.
        /// </value>
        public int? PnodeID { get; set; }
        //public Node Hour_Price { get; set; }
        /// <summary>
        /// Gets or sets the hour price.
        /// </summary>
        /// <value>
        /// The hour price.
        /// </value>
        public List<TimePrice> Hour_Price { get; set; }

        /// <summary>
        /// Shallows the copy.
        /// </summary>
        /// <returns></returns>
        public Node_Geo ShallowCopy()
        {
            return (Node_Geo)this.MemberwiseClone();
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

    #endregion
}
