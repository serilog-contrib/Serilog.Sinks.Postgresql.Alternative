// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbHelper.cs" company="Haemmer Electronics">
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
        /// <param name="tableName">Name of the table.</param>
        public void ClearTable(string tableName)
        {
            try
            {
                using var conn = new NpgsqlConnection(this.connectionString);
                conn.Open();
                using var command = conn.CreateCommand();
                command.CommandText = tableName.Contains("\"") ? $"TRUNCATE {tableName}" : $"TRUNCATE \"{tableName}\"";
                command.ExecuteNonQuery();

            }
            catch (PostgresException ex)
            {
                if (!ex.Message.Contains("does not exist"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     Gets the table rows count.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The table row count.</returns>
        public long GetTableRowsCount(string tableName)
        {
            try
            { 
                var sql = tableName.Contains("\"") ? $"SELECT count(*) FROM {tableName}" : $"SELECT count(*) FROM \"{tableName}\"";
                using var conn = new NpgsqlConnection(this.connectionString);
                conn.Open();
                using var command = conn.CreateCommand();
                command.CommandText = sql;
                var result = command.ExecuteScalar();
                return (long?)result ?? 0;

            }
            catch (PostgresException ex)
            {
                if (ex.Message.Contains("does not exist"))
                {
                    return 0;
                }

                throw;
            }
        }

        /// <summary>
        ///     Removes the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void RemoveTable(string tableName)
        {
            try
            {
                using var conn = new NpgsqlConnection(this.connectionString);
                conn.Open();
                using var command = conn.CreateCommand();
                command.CommandText = tableName.Contains("\"")
                                          ? $"DROP TABLE IF EXISTS {tableName}"
                                          : $"DROP TABLE IF EXISTS \"{tableName}\"";
                command.ExecuteNonQuery();
            }
            catch (PostgresException ex)
            {
                if (!ex.Message.Contains("does not exist"))
                {
                    throw;
                }
            }
        }
    }
}