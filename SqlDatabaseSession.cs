using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AU.DocumentGenerationService.GeneratorIntegration.CommandQueues
{
    public class SqlDatabaseSession : IDatabaseSession, IDisposable
    {
        public SqlDatabaseSession(string connectStringName)
        {
            if (string.IsNullOrEmpty(connectStringName))
                throw new ArgumentNullException("connectStringName");

            _connectStringName = connectStringName;
        }

        // Copy ctor to enable new sessions
        public SqlDatabaseSession(SqlDatabaseSession sess)
        {
            _connectStringName = sess._connectStringName;
        }

        private readonly string _connectStringName;
        private IDbConnection _conn;

        public SqlConnection SqlConnection()
        {
            return Connection() as SqlConnection;
        }

        public IDbConnection Connection()
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                return _conn;
            }

            // Get connection string from app.config
            var connStr = ConfigurationManager.ConnectionStrings[_connectStringName];
            if (connStr == null)
                throw new ConfigurationErrorsException("Can't loaded ConnectionString named: " + _connectStringName);

            _conn = new SqlConnection(connStr.ConnectionString);
            _conn.Open();

            return _conn;
        }

        public void CloseConnection(IDbConnection conn)
        {
            if (conn != null)
                conn.Close();
        }

        public void Dispose()
        {
            if (_conn == null) 
                return;

            _conn.Close();
            _conn.Dispose();
            _conn = null;
        }

        public IDataParameter CreateParam(string name, object value)
        {
            return new SqlParameter(name, value);
        }
    }
}
