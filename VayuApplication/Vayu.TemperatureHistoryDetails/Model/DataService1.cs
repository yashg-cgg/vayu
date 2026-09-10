using System;
using System.Data.SqlClient;
using Vayu.CommonAccessLibrary;

namespace Vayu.TemperatureHistoryDetails.Model
{
    public class MySqlCommand
    {
        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public SqlConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public SqlCommand Command { get; set; }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        /// <value>
        /// The command text.
        /// </value>
        public string CommandText
        {
            get { return Command.CommandText; }
            set { Command.CommandText = value; }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public SqlParameterCollection Parameters
        {
            get { return Command.Parameters; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlCommand"/> class.
        /// </summary>
        public MySqlCommand()
        {
            Command = new SqlCommand();
            Command.Connection = new VayuDBConnection().GetInstance().GetSqlConnection();
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <returns></returns>

    }

    /// <summary>
    /// 
    /// </summary>

    public class DataService1 : IDataService
    {
        public void loadDBCommands()
        {
            throw new NotImplementedException();
        }
    }
}
