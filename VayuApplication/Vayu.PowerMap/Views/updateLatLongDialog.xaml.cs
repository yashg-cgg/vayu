
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using Vayu.CommonAccessLibrary;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// Interaction logic for updateLatLongDialog.xaml
    /// </summary>
    public partial class updateLatLongDialog : Window
    {
        /// <summary>
        /// The Sigma database connection
        /// </summary>
        private SqlConnection SigmaDbConn;
        /// <summary>
        /// The m update lat long command
        /// </summary>
        private SqlCommand mUpdateLatLongCommand;

        /// <summary>
        /// Gets or sets the node identifier.
        /// </summary>
        /// <value>
        /// The node identifier.
        /// </value>
        public int NodeId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="updateLatLongDialog"/> class.
        /// </summary>
        /// <param name="NodeKey">The node key.</param>
        /// <param name="NodeName">Name of the node.</param>
        /// <param name="Zone">The zone.</param>
        /// <param name="latutide">The latutide.</param>
        /// <param name="longitude">The longitude.</param>
        public updateLatLongDialog(int NodeKey, string NodeName, string Zone, double? latutide, double? longitude)
        {
            InitializeComponent();
            initDB();
            NodeId = NodeKey;
            nodeNameTextBox.Text = NodeName;
            nodeZoneTextBox.Text = Zone;
            oldLatitudeTextBox.Text = latutide.ToString();
            oldLongitudeTextBox.Text = longitude.ToString();
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        void initDB()
        {
            SigmaDbConn = new VayuDBConnection().GetInstance().GetSqlConnection();

            mUpdateLatLongCommand = new SqlCommand();
            mUpdateLatLongCommand.CommandText = "Update dbo.NodeGeoImport Set Latitude = @Latitude, Longitude = @Longitude Where NodeKey = @NodeKey";
            mUpdateLatLongCommand.CommandType = CommandType.Text;
            mUpdateLatLongCommand.Parameters.AddWithValue("@Latitude", "Latitude");
            mUpdateLatLongCommand.Parameters.AddWithValue("@Longitude", "Longitude");
            mUpdateLatLongCommand.Parameters.AddWithValue("@NodeKey", "NodeKey");
            mUpdateLatLongCommand.Connection = SigmaDbConn;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (newLatitudeTextBox.Equals("") || newLongitudeTextBox.Equals(""))
                return;

            try
            {
                double latitude = double.Parse(newLatitudeTextBox.Text);
                double longitude = double.Parse(newLongitudeTextBox.Text);
                mUpdateLatLongCommand.Connection.Open();
                mUpdateLatLongCommand.Parameters["@Latitude"].Value = latitude;
                mUpdateLatLongCommand.Parameters["@Longitude"].Value = longitude;
                mUpdateLatLongCommand.Parameters["@NodeKey"].Value = NodeId;
                mUpdateLatLongCommand.ExecuteNonQuery();
                mUpdateLatLongCommand.Connection.Close();
                //DialogResult = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Latitude or Longitude are invalid!! ");
                return;
            }
        }
    }
}
