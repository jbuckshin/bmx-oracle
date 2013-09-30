using System;
using System.Collections.Generic;
using System.Data;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.Database;
using Inedo.BuildMaster.Web;
using Oracle.DataAccess.Client;

namespace Inedo.BuildMasterExtensions.Oracle
{
    /// <summary>
    /// Connects to an Oracle database and provides change script functionality.
    /// </summary>
    [ProviderProperties("Oracle", "Supports Oracle 9i and later; requires Oracle Data Access Components (ODAC) installed.", RequiresTransparentProxy = true)]
    [CustomEditor(typeof(OracleDatabaseProviderEditor))]
    public sealed class OracleDatabaseProvider : DatabaseProviderBase, IChangeScriptProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDatabaseProvider"/> class.
        /// </summary>
        public OracleDatabaseProvider()
        {
        }

        /// <summary>
        /// When implemented by a derived class, runs each query in the provided array.
        /// </summary>
        /// <param name="queries">An array of query text</param>
        public override void ExecuteQueries(string[] queries)
        {
            if (queries == null || queries.Length == 0)
                return;

            using (var conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand(string.Empty, conn))
                {
                    for (int i = 0; i < queries.Length; i++)
                    {
                        var query = queries[i];
                        this.LogDebug("Executing Query {0}...", i+1);
                        cmd.CommandText = query.Replace("\r", "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        /// <summary>
        /// When implemented by a derived class, runs the specified query.
        /// </summary>
        /// <param name="query">The database query to execute</param>
        public override void ExecuteQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
                return;

            var queries = new List<string>(ScriptSplitter.Process(query)).ToArray();
            this.LogDebug("Split into {0} queries.", queries.Length);
            this.ExecuteQueries(queries);
        }
        /// <summary>
        /// When implemented in a derived class, indicates whether the provider
        /// is installed and available for use in the current execution context
        /// </summary>
        /// <returns></returns>
        public override bool IsAvailable()
        {
            try
            {
                return IsAvailable2();
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// When implemented in a derived class, attempts to connect with the
        /// current configuration and, if not successful, throws a
        /// <see cref="ConnectionException"/>
        /// </summary>
        public override void ValidateConnection()
        {
            ExecuteQuery("SELECT 1 FROM dual");
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            try
            {
                return ToString2();
            }
            catch { /* GULP */ }
            return "Oracle";
        }

        /// <summary>
        /// When implemented by a derived class, initializes the database by installing metadata tables
        /// for tracking change scripts and version numbers
        /// </summary>
        public void InitializeDatabase()
        {
            if (IsDatabaseInitialized())
                throw new InvalidOperationException("Database is already initialized.");

            ExecuteQueries(new[]
            {
                Properties.Resources.Initialize1,
                Properties.Resources.Initialize2,
                Properties.Resources.Initialize3
            });
        }
        /// <summary>
        /// When implemented by a derived class, indicates whether the database has been initialized
        /// </summary>
        /// <returns></returns>
        public bool IsDatabaseInitialized()
        {
            ValidateConnection();

            int count = ExecuteScalar("SELECT COUNT(*) FROM tabs WHERE \"TABLE_NAME\" = '__BuildMaster_DbSchemaChanges'");
            return count > 0;
        }
        /// <summary>
        /// When implemented by a derived class, retrieves the changes that occurred in the
        /// specified database in a table that matches the <see cref="TableDefs.DatabaseChangeHistory"/> schema
        /// </summary>
        /// <returns></returns>
        public ChangeScript[] GetChangeHistory()
        {
            if (!IsDatabaseInitialized())
                throw new InvalidOperationException("Database is not initialized.");

            var table = ExecuteDataTable("SELECT * FROM \"__BuildMaster_DbSchemaChanges\"");
            var scripts = new OracleChangeScript[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
                scripts[i] = new OracleChangeScript(table.Rows[i]);

            return scripts;
        }
        /// <summary>
        /// When implemented in a derived class, retrieves the numeric release number of the
        /// database
        /// </summary>
        /// <returns></returns>
        public long GetSchemaVersion()
        {
            if (!IsDatabaseInitialized())
                throw new InvalidOperationException("Database is not initialized.");

            return Convert.ToInt64(ExecuteDataTable(
                "SELECT COALESCE(MAX(\"Numeric_Release_Number\"),0) FROM \"__BuildMaster_DbSchemaChanges\""
                ).Rows[0][0]);
        }
        /// <summary>
        /// When implemented by a derived class, executes the specified script provided that the
        /// specified script has not already been executed, and returns a boolean indicating whether
        /// the script was skipped as a result of being executed.
        /// </summary>
        /// <param name="numericReleaseNumber">Release number for the specified script name</param>
        /// <param name="scriptId"></param>
        /// <param name="scriptName">name of the script to be executed</param>
        /// <param name="scriptText">script text to be run</param>
        /// <returns>
        /// a boolean indicating whether the script was skipped
        /// </returns>
        public ExecutionResult ExecuteChangeScript(long numericReleaseNumber, int scriptId, string scriptName, string scriptText)
        {
            if (!this.IsDatabaseInitialized())
                throw new InvalidOperationException("Database is not initialized.");

            var tables = this.ExecuteDataTable("SELECT * FROM \"__BuildMaster_DbSchemaChanges\"");
            if (tables.Select("Script_Id=" + scriptId.ToString()).Length > 0)
                return new ExecutionResult(ExecutionResult.Results.Skipped, scriptName + " already executed.");

            Exception ex = null;
            try { this.ExecuteQuery(scriptText); }
            catch (Exception _ex) { ex = _ex; }

            this.ExecuteQuery(string.Format(
                "INSERT INTO \"__BuildMaster_DbSchemaChanges\" "
                + " (\"Numeric_Release_Number\", \"Script_Id\", \"Script_Sequence\", \"Script_Name\", \"Executed_Date\", \"Success_Indicator\") "
                + "VALUES "
                + "({0}, {1}, \"__BuildMaster_DbSchema_Seq\".nextval, '{2}', CURRENT_DATE, '{3}')",
                numericReleaseNumber,
                scriptId,
                scriptName.Replace("'", "''"),
                ex == null ? "Y" : "N"));

            if (ex == null)
                return new ExecutionResult(ExecutionResult.Results.Success, scriptName + " executed successfully.");
            else
                return new ExecutionResult(ExecutionResult.Results.Failed, scriptName + " execution failed:" + ex.Message);
        }

        /// <summary>
        /// Executes a query and returns a data table.
        /// </summary>
        /// <param name="sqlCommand">The SQL query to execute.</param>
        /// <returns>The result of the query as a data table.</returns>
        private DataTable ExecuteDataTable(string sqlCommand)
        {
            using (var conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand(sqlCommand, conn))
                {
                    var table = new DataTable();
                    table.Load(cmd.ExecuteReader());
                    return table;
                }
            }
        }
        /// <summary>
        /// Executes a query and returns a scalar Int32 value.
        /// </summary>
        /// <param name="sqlCommand">The SQL query to execute.</param>
        /// <returns>The result of the query as an Int32.</returns>
        private int ExecuteScalar(string sqlCommand)
        {
            using (var conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand(sqlCommand, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        private string ToString2()
        {
            var csb = new OracleConnectionStringBuilder(this.ConnectionString);
            return string.Format("Oracle on server \"{0}\"", csb.DataSource);
        }
        private bool IsAvailable2()
        {
            try
            {
                new OracleConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
