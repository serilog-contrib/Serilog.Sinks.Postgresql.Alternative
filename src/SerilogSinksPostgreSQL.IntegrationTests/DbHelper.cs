// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbHelper.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used as helper class for the database connection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.IntegrationTests
{
    using Npgsql;

    /// <summary>
    ///     This class is used as helper class for the database connection.
    /// </summary>
    public class DbHelper
    {
        /// <summary>
        ///     The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbHelper" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DbHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        ///     Clears the table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public void ClearTable(string tableName)
        {
            tableName = tableName.Replace("\"", string.Empty);
            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = $"TRUNCATE \"{tableName}\";";
            command.ExecuteNonQuery();
        }

        /// <summary>
        ///     Clears the table.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <param name="tableName">The name of the table.</param>
        public void ClearTable(string schemaName, string tableName)
        {
            tableName = tableName.Replace("\"", string.Empty);
            schemaName = schemaName.Replace("\"", string.Empty);
            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = $"TRUNCATE \"{schemaName}\".\"{tableName}\";";
            command.ExecuteNonQuery();
        }

        /// <summary>
        ///     Gets the table rows count.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The table row count.</returns>
        public long GetTableRowsCount(string tableName)
        {
            tableName = tableName.Replace("\"", string.Empty);
            var sql = $"SELECT count(*) FROM \"{tableName}\";";
            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = sql;
            var result = command.ExecuteScalar();
            return (long?)result ?? 0;
        }

        /// <summary>
        ///     Gets the table rows count.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The table row count.</returns>
        public long GetTableRowsCount(string schemaName, string tableName)
        {
            tableName = tableName.Replace("\"", string.Empty);
            schemaName = schemaName.Replace("\"", string.Empty);
            var sql = $"SELECT count(*) FROM \"{schemaName}\".\"{tableName}\";";
            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = sql;
            var result = command.ExecuteScalar();
            return (long?)result ?? 0;
        }

        /// <summary>
        ///     Removes the table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public void RemoveTable(string tableName)
        {
            tableName = tableName.Replace("\"", string.Empty);
            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = $"DROP TABLE IF EXISTS \"{tableName}\";";
            command.ExecuteNonQuery();
        }

        /// <summary>
        ///     Removes the schema.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        public void RemoveSchema(string schemaName)
        {
            schemaName = schemaName.Replace("\"", string.Empty);
            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = $"DROP SCHEMA IF EXISTS \"{schemaName}\";";
            command.ExecuteNonQuery();
        }
    }
}