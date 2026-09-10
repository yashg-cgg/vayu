using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using Vayu.CommonAccessLibrary;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class EnergyMapHelper
    {
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

        /// <summary>
        /// Runs the action in thread.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static Task RunActionInThread(Action action)
        {
            Task task = Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception)
                {
                }
            });
            return task;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PortfolioBid
    {
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NodeDetail
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int ID { get; set; }
        /// <summary>
        /// Gets or sets the external identifier.
        /// </summary>
        /// <value>
        /// The external identifier.
        /// </value>
        public int ExternalID { get; set; }
        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the type key.
        /// </summary>
        /// <value>
        /// The type key.
        /// </value>
        public int TypeKey { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PortfolioDetails
    {
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the name of the source.
        /// </summary>
        /// <value>
        /// The name of the source.
        /// </value>
        public string SourceName { get; set; }
        /// <summary>
        /// Gets or sets the sink.
        /// </summary>
        /// <value>
        /// The sink.
        /// </value>
        public string Sink { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public double Price { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PortfolioDetails1
    {
        /// <summary>
        /// Gets or sets the mw.
        /// </summary>
        /// <value>
        /// The mw.
        /// </value>
        public double MW { get; set; }
        /// <summary>
        /// Gets or sets the nodename.
        /// </summary>
        /// <value>
        /// The nodename.
        /// </value>
        public string Nodename { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CategoryEquipmentType
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }
        /// <summary>
        /// Gets or sets the name of the equipment type.
        /// </summary>
        /// <value>
        /// The name of the equipment type.
        /// </value>
        public string EquipmentTypeName { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ComparisonConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : System.Windows.Data.Binding.DoNothing;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class WaitCursor : IDisposable
    {
        /// <summary>
        /// The saved cursor
        /// </summary>
        private System.Windows.Forms.Cursor _SavedCursor;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitCursor"/> class.
        /// </summary>
        public WaitCursor()
        {
            _SavedCursor = System.Windows.Forms.Cursor.Current;
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            System.Windows.Forms.Cursor.Current = _SavedCursor;
        }
    }
}
