using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ServiceModel;
using Vayu.CommonAccessLibrary;

namespace Vayu.LatestConstraintsInformationLibrary
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="LatestConstraintsInformationLibrary.IConstraintInfoCallback" />
    public class ConstraintHelper : IConstraintInfoCallback
    {
        #region Public Methods


        public void SetConstraints(List<LatestConstraint> constraintList)
        {
            //Do nothing
        }


        public static List<LatestConstraint> GetLatestConstraint(MarketType market, DateTime date, bool isAllDay = false)
        {
            return GetLatestConstraint((int)market, date, isAllDay);
        }

        /// <summary>
        /// Gets the latest constraint.
        /// </summary>
        /// <param name="market">The market.</param>
        /// <param name="date">The date.</param>
        /// <param name="isAllDay">if set to <c>true</c> [is all day].</param>
        /// <returns></returns>
        public static List<LatestConstraint> GetLatestConstraint(int market, DateTime date, bool isAllDay = false)
        {
            ConstraintHelper helper = new ConstraintHelper();
            IConstraintInfoProvider infoProvider = helper.GetInstance();
            List<LatestConstraint> constraints = infoProvider.GetLatestConstraint((int)market, date, isAllDay);
            return constraints;
        }

        public IConstraintInfoProvider GetInstance()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            binding.OpenTimeout = new TimeSpan(0, 30, 0);
            binding.CloseTimeout = new TimeSpan(0, 12, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 12, 0);
            binding.SendTimeout = new TimeSpan(0, 12, 0);
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.MaxBufferSize = int.MaxValue;
            binding.TransferMode = TransferMode.Buffered;
            binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            binding.TransactionFlow = false;
            DuplexChannelFactory<IConstraintInfoProvider> mConstraintFactory = new DuplexChannelFactory<IConstraintInfoProvider>
                        (new InstanceContext(new ConstraintHelper()), binding, Vayu.CommonAccessLibrary.ServiceConnections.GetLatestConstraintService());
            IConstraintInfoProvider channel = mConstraintFactory.CreateChannel() as IConstraintInfoProvider;
            mConstraintFactory.Faulted += OnChannelFactory_Faulted;

            return channel;
        }

        #endregion

        /// <summary>
        /// Called when [channel factory faulted].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnChannelFactory_Faulted(object sender, EventArgs e)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NodeInfo
    {
        #region Properties

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
        public int NodeKey { get; set; }
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double? Latitude { get; set; }
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double? Longitude { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <param name="mkType">Type of the mk.</param>
        /// <returns></returns>
        public static Hashtable GetHash(MarketType mkType)
        {
            return GetHash((int)mkType);
        }

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <param name="mkType">Type of the mk.</param>
        /// <returns></returns>
        public static Hashtable GetHash(int mkType)
        {
            Hashtable hash = new Hashtable();
            SqlConnection VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();
            SqlCommand command = VayuConnection.CreateCommand();
            command.CommandText = "select NodeKey,NodeName,Longitude,Latitude from Node where MarketKey = " + (int)mkType;

            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                NodeInfo info = new NodeInfo();
                info.NodeKey = GetInt(reader[0]);
                info.NodeName = reader[1].ToString();
                info.Longitude = GetNullDouble(reader[2]);
                info.Latitude = GetNullDouble(reader[3]);

                hash[info.NodeKey] = info;
            }

            reader.Close();
            command.Connection.Close();
            return hash;
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static int GetInt(object intValue)
        {
            int val = 0;
            int.TryParse((intValue ?? "").ToString(), out val);
            return val;
        }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static DateTime GetDate(object intValue)
        {
            DateTime val;
            DateTime.TryParse((intValue ?? "").ToString(), out val);
            return val;
        }

        /// <summary>
        /// Gets the double.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static double GetDouble(object intValue)
        {
            double val;
            double.TryParse((intValue ?? "").ToString(), out val);
            return val;
        }

        /// <summary>
        /// Gets the null double.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static double? GetNullDouble(object intValue)
        {
            double val;

            if (!double.TryParse((intValue ?? "").ToString(), out val))
                return null;

            return val;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum MarketType
    {
        /// <summary>
        /// The none
        /// </summary>
        NONE = 0,
        /// <summary>
        /// The PJM
        /// </summary>
        PJM = 1,
        /// <summary>
        /// The ercot
        /// </summary>
        ERCOT = 9,

    }
}
