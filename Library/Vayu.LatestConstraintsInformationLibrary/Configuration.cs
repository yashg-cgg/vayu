using System;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.LatestConstraintsInformationLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The constraint endpoint
        /// </summary>
        public const string ConstraintEndpoint = "net.tcp://localhost:7003/ISubscribe";

        #region Public Methods

        /// <summary>
        /// Gets the trading database connection.
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetERCOTDBConnection()
        {
            SqlConnection conn = new VayuDBConnection().GetInstance().GetSqlConnection();
            return conn;
        }

        /// <summary>
        /// Gets the trading database command.
        /// </summary>
        /// <returns></returns>
        public static SqlCommand GetTradingDBCommand()
        {
            SqlConnection conn = GetERCOTDBConnection();
            return conn.CreateCommand();
        }
        public static SqlCommand GetErcotDBCommand()
        {
            SqlConnection conn = GetERCOTDBConnection();
            return conn.CreateCommand();
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
        /// Gets the double na n.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static double GetDoubleNaN(object intValue)
        {
            double val = double.NaN;
            if (double.TryParse((intValue ?? "").ToString(), out val))
                return val;

            return double.NaN;
        }

        /// <summary>
        /// Gets the null double.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static double? GetNullDouble(object intValue)
        {
            double val;
            if (double.TryParse((intValue ?? "").ToString(), out val))
                return val;

            return null;
        }

        /// <summary>
        /// Gets the null int.
        /// </summary>
        /// <param name="intValue">The int value.</param>
        /// <returns></returns>
        public static int? GetNullInt(object intValue)
        {
            int val = 0;
            if (int.TryParse((intValue ?? "").ToString(), out val))
                return val;

            return null;
        }

        /// <summary>
        /// Gets the absolute date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime GetAbsoluteDate(DateTime date)
        {
            DateTime dt = new DateTime(date.Ticks, DateTimeKind.Unspecified);
            return dt;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class KeyMaker
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <param name="contingency">The contingency.</param>
        /// <returns></returns>
        public static KeyStruct GetKey(string constraint, string contingency)
        {
            return new KeyStruct() { Constraint = constraint, Contingency = contingency };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct KeyStruct
    {
        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public string Constraint { get; set; }
        /// <summary>
        /// Gets or sets the contingency.
        /// </summary>
        /// <value>
        /// The contingency.
        /// </value>
        public string Contingency { get; set; }
    }
}
