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
    using System.Text;

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
        /// <param name="schemaName">The name of the schema.</param>
        /// <param name="tableName">The name of the table.</param>
        public void ClearTable(string schemaName, string tableName)
        {
            tableName = tableName.Replace("\"", string.Empty);
            schemaName = schemaName.Replace("\"", string.Empty);

            var builder = new StringBuilder();
            builder.Append("TRUNCATE ");

            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                builder.Append("\"");
                builder.Append(schemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(tableName);
            builder.Append("\";");

            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = builder.ToString();
            command.ExecuteNonQuery();
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

            var builder = new StringBuilder();
            builder.Append("SELECT count(*) FROM ");

            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                builder.Append("\"");
                builder.Append(schemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(tableName);
            builder.Append("\";");

            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = builder.ToString();
            var result = command.ExecuteScalar();
            return (long?)result ?? 0;
        }

        /// <summary>
        ///     Removes the table.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <param name="tableName">The name of the table.</param>
        public void RemoveTable(string schemaName, string tableName)
        {
            tableName = tableName.Replace("\"", string.Empty);
            schemaName = schemaName.Replace("\"", string.Empty);

            var builder = new StringBuilder();
            builder.Append("DROP TABLE IF EXISTS ");

            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                builder.Append("\"");
                builder.Append(schemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(tableName);
            builder.Append("\";");

            using var conn = new NpgsqlConnection(this.connectionString);
            conn.Open();
            using var command = conn.CreateCommand();
            command.CommandText = builder.ToString();
            command.ExecuteNonQuery();
        }
    }
}