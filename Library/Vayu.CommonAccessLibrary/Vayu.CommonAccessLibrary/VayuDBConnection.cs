using Microsoft.Extensions.ObjectPool;
using System;
using System.Data.SqlClient;

namespace Vayu.CommonAccessLibrary
{
    public class VayuDBConnection
    {
        private VayuDBConnection instance;

        //string secret = null;

        private static readonly object lockObject = new object();

        private static readonly string ConnectionStr = DatabaseConnection.GetConnectionStringFromAzure();
        //private static readonly string ConnectionStr = DatabaseConnection.VayuDbConnection();
        private static ObjectPool<SqlConnection> connectionPool;
        public VayuDBConnection()
        {
            InitializeConnectionPool();
        }

        public VayuDBConnection GetInstance()
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new VayuDBConnection();
                    }
                }
            }
            return instance;
        }

        private void InitializeConnectionPool()
        {
            // Create connection pool
            connectionPool = new DefaultObjectPool<SqlConnection>(
                new DefaultPooledObjectPolicy<SqlConnection>(),
                Environment.ProcessorCount * 50); // You can adjust the pool size as per your requirements
        }

        public SqlConnection GetSqlConnection()
        {
            // Acquire connection from the pool
            var connection = connectionPool.Get();

            // Ensure that the connection is open
            if (connection.State != System.Data.ConnectionState.Open)
            {

                // Set the connection string if it's not set already
                if (string.IsNullOrEmpty(connection.ConnectionString))
                {
                    connection.ConnectionString = ConnectionStr;
                }

                // Open the connection
                connection.Open();


            }

            return connection;
        }
    }

    public class SqlConnectionPooledObjectPolicy : IPooledObjectPolicy<SqlConnection>
    {
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;

        public SqlConnectionPooledObjectPolicy(SqlConnectionStringBuilder connectionStringBuilder)
        {
            _connectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
        }

        public SqlConnection Create()
        {
            var connection = new SqlConnection(_connectionStringBuilder.ConnectionString);
            return connection;
        }

        public bool Return(SqlConnection obj)
        {
            // You can put any necessary reset logic here if needed
            return true;
        }
    }
}
