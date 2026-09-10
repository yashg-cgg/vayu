using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Vayu.CommonAccessLibrary;

namespace Vayu.CommonControls
{
    /// <summary>
    /// 
    /// </summary>
    public class ZoneModel
    {
        /// <summary>
        /// Gets the zone list.
        /// </summary>
        /// <param name="marketKey">The market key.</param>
        /// <returns></returns>
        public static List<ZoneInfo> GetZoneList(int marketKey)
        {
            List<ZoneInfo> list = new List<ZoneInfo>();
            List<ZoneInfo> zList = new List<ZoneInfo>();
            SqlCommand mSelectZones = new SqlCommand();
            mSelectZones.CommandText = "select Zone , ColorString , Latitude , Longitude , Sequence , regionID from ZoneInfo where MarketKey = " + marketKey;
            mSelectZones.Connection = new VayuDBConnection().GetInstance().GetSqlConnection();

            try
            {
                mSelectZones.Connection.Open();
                SqlDataReader reader = mSelectZones.ExecuteReader();
                while (reader.Read())
                {
                    ZoneInfo inf = new ZoneInfo();
                    
                    inf.Name = reader[0].ToString();
                    inf.StringColor = reader[1].ToString();

                    double.TryParse(reader[2].ToString(), out inf.latitude);
                    double.TryParse(reader[3].ToString(), out inf.longitude);
                    int.TryParse(reader[4].ToString(), out inf.sequence);
                    int.TryParse(reader[5].ToString(), out inf.regionID);

                    zList.Add(inf);
                }

                reader.Close();

                foreach (var item0 in zList.GroupBy(x => x.Name))
                {
                    ZoneInfo inf = new ZoneInfo();
                    inf.Name = item0.Key;
                    inf.StringColor = item0.First().StringColor;

                    foreach (var item1 in item0.GroupBy(x=>x.regionID))
                    {
                        RegionInfo rinf = new RegionInfo();
                        rinf.RegionID = item1.Key;
                        rinf.Name = item0.Key;
                        string[] cols = item1.First().StringColor.Split(',');
                        byte r, g, b;
                        byte.TryParse(cols[0], out r);
                        byte.TryParse(cols[1], out g);
                        byte.TryParse(cols[2], out b);
                        Color color = Color.FromRgb(r, g, b);
                        rinf.FillColor = new SolidColorBrush(color);
                        inf.FillColor = new SolidColorBrush(color);
                        LocationCollection col = new LocationCollection();

                        foreach (var itemF in item1.OrderBy(x => x.sequence))
                            col.Add(new Location(itemF.latitude, itemF.longitude));

                        rinf.Locations = col;
                        inf.RegionInfoList.Add(rinf);
                    }

                    list.Add(inf);
                }
            }
            catch (Exception ex) { }
            finally
            {
                if (mSelectZones.Connection.State != System.Data.ConnectionState.Open)
                    mSelectZones.Connection.Close();
            }

            return list;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class ZoneInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// The is selected
        /// </summary>
        private bool isSelected;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; Notify(); }
        }

        /// <summary>
        /// The string color
        /// </summary>
        private string stringColor;
        /// <summary>
        /// Gets or sets the color of the string.
        /// </summary>
        /// <value>
        /// The color of the string.
        /// </value>
        public string StringColor
        {
            get { return stringColor; }
            set { stringColor = value; Notify(); }
        }

        /// <summary>
        /// Gets or sets the color of the fill.
        /// </summary>
        /// <value>
        /// The color of the fill.
        /// </value>
        public SolidColorBrush FillColor { get; set; }

        /// <summary>
        /// The name
        /// </summary>
        private string name;
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return name; }
            set { name = value; Notify(); }
        }

        /// <summary>
        /// Gets or sets the region information list.
        /// </summary>
        /// <value>
        /// The region information list.
        /// </value>
        public List<RegionInfo> RegionInfoList { get; set; }

        /// <summary>
        /// The latitude
        /// </summary>
        public double latitude;
        /// <summary>
        /// The longitude
        /// </summary>
        public double longitude;
        /// <summary>
        /// The region identifier
        /// </summary>
        public int regionID;
        /// <summary>
        /// The sequence
        /// </summary>
        public int sequence;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notifies the specified property name.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        private void Notify([CallerMemberName] string propName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneInfo"/> class.
        /// </summary>
        public ZoneInfo()
        {
            RegionInfoList = new List<RegionInfo>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RegionInfo
    {
        /// <summary>
        /// Gets or sets the region identifier.
        /// </summary>
        /// <value>
        /// The region identifier.
        /// </value>
        public int RegionID { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the color of the fill.
        /// </summary>
        /// <value>
        /// The color of the fill.
        /// </value>
        public SolidColorBrush FillColor { get; set; }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        public LocationCollection Locations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionInfo"/> class.
        /// </summary>
        public RegionInfo()
        {
            Locations = new LocationCollection();
        }
    }
}
