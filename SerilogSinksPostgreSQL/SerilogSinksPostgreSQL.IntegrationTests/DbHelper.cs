namespace SerilogSinksPostgreSQL.IntegrationTests
{
    using Npgsql;

    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public void ClearTable(string tableName)
        {
            using (var conn = new NpgsqlConnection(this._connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"TRUNCATE {tableName}";

                    command.ExecuteNonQuery();
                }
            }
        }

        public long GetTableRowsCount(string tableName)
        {
            var sql = $@"SELECT count(*)
                         FROM {tableName}";

            using (var conn = new NpgsqlConnection(this._connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;

                    return (long)command.ExecuteScalar();
                }
            }
        }

        public void RemoveTable(string tableName)
        {
            using (var conn = new NpgsqlConnection(this._connectionString))
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