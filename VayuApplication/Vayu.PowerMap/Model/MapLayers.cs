using Microsoft.Maps.MapControl.WPF;
using System.Collections.Generic;
using System.ComponentModel;

namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class Top_Layer : INotifyPropertyChanged
    {
        /// <summary>
        /// The childlayer
        /// </summary>
        private List<Mid_Layer> childlayer = new List<Mid_Layer>();
        /// <summary>
        /// The name
        /// </summary>
        private string name;
        /// <summary>
        /// The key
        /// </summary>
        private int key;
        /// <summary>
        /// The on off
        /// </summary>
        private bool On_Off;
        /// <summary>
        /// The top maplayer
        /// </summary>
        public MapLayer top_maplayer = new MapLayer();

        /// <summary>
        /// Gets or sets a value indicating whether [on off].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on off]; otherwise, <c>false</c>.
        /// </value>
        public bool ON_OFF
        {
            get { return this.On_Off; }
            set
            {
                this.On_Off = value;
                RaisePropertyChanged("ON_OFF");
            }
        }

        /// <summary>
        /// Gets the childlayer.
        /// </summary>
        /// <value>
        /// The childlayer.
        /// </value>
        public List<Mid_Layer> Childlayer
        {
            get { return this.childlayer; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public int Key
        {
            get { return this.key; }
            set { this.key = value; }
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == "ON_OFF")
            {
                if (this.Childlayer.Count != 0 && this.On_Off == false)
                {
                    CheckedChild(this.Childlayer, this.On_Off);
                }
            }
        }

        /// <summary>
        /// Checkeds the child.
        /// </summary>
        /// <param name="children">The children.</param>
        /// <param name="on_off">if set to <c>true</c> [on off].</param>
        private void CheckedChild(List<Mid_Layer> children, bool on_off)
        {
            foreach (Mid_Layer child in children)
            {
                child.On_Off = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Mid_Layer
    {
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public string parent { get; set; }
        /// <summary>
        /// Gets or sets the parentkey.
        /// </summary>
        /// <value>
        /// The parentkey.
        /// </value>
        public int parentkey { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public int key { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [on off].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on off]; otherwise, <c>false</c>.
        /// </value>
        public bool On_Off { get; set; }
        /// <summary>
        /// Gets or sets the parentlayer.
        /// </summary>
        /// <value>
        /// The parentlayer.
        /// </value>
        public List<Top_Layer> parentlayer { get; set; }
        /// <summary>
        /// Gets or sets the childlayer.
        /// </summary>
        /// <value>
        /// The childlayer.
        /// </value>
        public List<Third_Layer> childlayer { get; set; }
        /// <summary>
        /// The mid maplayer
        /// </summary>
        public MapLayer mid_maplayer = new MapLayer();
        /// <summary>
        /// Gets or sets the childyescount.
        /// </summary>
        /// <value>
        /// The childyescount.
        /// </value>
        public int childyescount { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class Third_Layer : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public string parent { get; set; }
        /// <summary>
        /// Gets or sets the parentkey.
        /// </summary>
        /// <value>
        /// The parentkey.
        /// </value>
        public int parentkey { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public int key { get; set; }
        /// <summary>
        /// The on off
        /// </summary>
        private bool On_Off;
        /// <summary>
        /// Gets or sets the parentlayer.
        /// </summary>
        /// <value>
        /// The parentlayer.
        /// </value>
        public List<Third_Layer> parentlayer { get; set; }
        /// <summary>
        /// The third maplayer
        /// </summary>
        public MapLayer third_maplayer = new MapLayer();
        /// <summary>
        /// The map object collection
        /// </summary>
        public Dictionary<string, placemarkbject> map_obj_collection = new Dictionary<string, placemarkbject>();

        /// <summary>
        /// Gets or sets a value indicating whether [on off].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on off]; otherwise, <c>false</c>.
        /// </value>
        public bool ON_OFF
        {
            get { return this.On_Off; }
            set
            {
                this.On_Off = value;
                RaisePropertyChanged("ON_OFF");
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
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName == "ON_OFF")
            {
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class placemarkbject
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }
        /// <summary>
        /// Gets or sets the descripton.
        /// </summary>
        /// <value>
        /// The descripton.
        /// </value>
        public string descripton { get; set; }
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Location location { get; set; }
    }
}
