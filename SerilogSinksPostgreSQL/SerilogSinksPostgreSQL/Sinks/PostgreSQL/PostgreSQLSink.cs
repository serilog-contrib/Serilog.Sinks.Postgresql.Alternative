namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Npgsql;

    using Serilog.Events;
    using Serilog.Sinks.PeriodicBatching;

    public class PostgreSqlSink : PeriodicBatchingSink
    {
        public const int DefaultBatchSizeLimit = 30;

        // ReSharper disable once MemberCanBePrivate.Global
        public const int DefaultQueueLimit = int.MaxValue;

        private readonly IDictionary<string, ColumnWriterBase> _columnOptions;

        private readonly string _connectionString;

        private readonly IFormatProvider _formatProvider;

        private readonly string _fullTableName;

        private readonly bool _useCopy;

        private bool _isTableCreated;

        public PostgreSqlSink(
            string connectionString,
            string tableName,
            TimeSpan period,
            IFormatProvider formatProvider = null,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            bool useCopy = true,
            string schemaName = "",
            bool needAutoCreateTable = false)
            : base(batchSizeLimit, period)
        {
            this._connectionString = connectionString;

            this._fullTableName = this.GetFullTableName(tableName, schemaName);

            this._formatProvider = formatProvider;
            this._useCopy = useCopy;

            this._columnOptions = columnOptions ?? ColumnOptions.Default;

            this._isTableCreated = !needAutoCreateTable;
        }

        public PostgreSqlSink(
            string connectionString,
            string tableName,
            TimeSpan period,
            IFormatProvider formatProvider = null,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            int queueLimit = DefaultQueueLimit,
            bool useCopy = true,
            string schemaName = "",
            bool needAutoCreateTable = false)
            : base(batchSizeLimit, period, queueLimit)
        {
            this._connectionString = connectionString;

            this._fullTableName = this.GetFullTableName(tableName, schemaName);

            this._formatProvider = formatProvider;
            this._useCopy = useCopy;

            this._columnOptions = columnOptions ?? ColumnOptions.Default;

            this._isTableCreated = !needAutoCreateTable;
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            using (var connection = new NpgsqlConnection(this._connectionString))
            {
                connection.Open();

                if (!this._isTableCreated)
                {
                    TableCreator.CreateTable(connection, this._fullTableName, this._columnOptions);
                    this._isTableCreated = true;
                }

                if (this._useCopy) this.ProcessEventsByCopyCommand(events, connection);
                else this.ProcessEventsByInsertStatements(events, connection);
            }
        }

        private static string ClearColumnNameForParameterName(string columnName)
        {
            return columnName?.Replace("\"", string.Empty);
        }

        private string GetCopyCommand()
        {
            var columns = string.Join(", ", this._columnOptions.Keys);

            return $"COPY {this._fullTableName}({columns}) FROM STDIN BINARY;";
        }

        private string GetFullTableName(string tableName, string schemaName)
        {
            var schemaPrefix = string.Empty;
            if (!string.IsNullOrEmpty(schemaName))
                schemaPrefix = schemaName + ".";

            return schemaPrefix + tableName;
        }

        private string GetInsertQuery()
        {
            var columns = string.Join(", ", this._columnOptions.Keys);

            var parameters = string.Join(
                ", ",
                this._columnOptions.Keys.Select(cn => ":" + ClearColumnNameForParameterName(cn)));

            return $@"INSERT INTO {this._fullTableName} ({columns})
                                        VALUES ({parameters})";
        }

        private void ProcessEventsByCopyCommand(IEnumerable<LogEvent> events, NpgsqlConnection connection)
        {
            using (var binaryCopyWriter = connection.BeginBinaryImport(this.GetCopyCommand()))
            {
                this.WriteToStream(binaryCopyWriter, events);
                binaryCopyWriter.Complete();
            }
        }

        private void ProcessEventsByInsertStatements(IEnumerable<LogEvent> events, NpgsqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = this.GetInsertQuery();

                foreach (var logEvent in events)
                {
                    // TODO: Init once
                    command.Parameters.Clear();
                    foreach (var columnOption in this._columnOptions)
                        command.Parameters.AddWithValue(
                            ClearColumnNameForParameterName(columnOption.Key),
                            columnOption.Value.DbType,
                            columnOption.Value.GetValue(logEvent, this._formatProvider));

                    command.ExecuteNonQuery();
                }
            }
        }

        private void WriteToStream(NpgsqlBinaryImporter writer, IEnumerable<LogEvent> entities)
        {
            foreach (var entity in entities)
            {
                writer.StartRow();

                foreach (var columnKey in this._columnOptions.Keys)
                    writer.Write(
                        this._columnOptions[columnKey].GetValue(entity, this._formatProvider),
                        this._columnOptions[columnKey].DbType);
            }
        }
    }
}