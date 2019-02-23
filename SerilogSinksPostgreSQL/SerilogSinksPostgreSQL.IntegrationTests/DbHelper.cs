namespace SerilogSinksPostgreSQL.IntegrationTests
{
    using Npgsql;

    /// <summary>
    /// This class is used as helper class for the database connection.
    /// </summary>
    public class DbHelper
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbHelper"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DbHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Clears the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void ClearTable(string tableName)
        {
            using (var conn = new NpgsqlConnection(this.connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"TRUNCATE {tableName}";

                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Gets the table rows count.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The table row count.</returns>
        public long GetTableRowsCount(string tableName)
        {
            var sql = $@"SELECT count(*)
                         FROM {tableName}";

            using (var conn = new NpgsqlConnection(this.connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;

                    return (long)command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Removes the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void RemoveTable(string tableName)
        {
            using (var conn = new NpgsqlConnection(this.connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"DROP TABLE IF EXISTS {tableName}";

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}