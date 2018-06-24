using Npgsql;

namespace Serilog.Sinks.PostgreSQL.IntegrationTests
{
    public class DbHelper
    {
        private string _connectionString;

        public DbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void RemoveTable(string tableName)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"DROP TABLE IF EXISTS {tableName}";

                    command.ExecuteNonQuery();
                }
            }
        }

        public void ClearTable(string tableName)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
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

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;

                    return (long) command.ExecuteScalar();
                }
            }
        }
    }
}