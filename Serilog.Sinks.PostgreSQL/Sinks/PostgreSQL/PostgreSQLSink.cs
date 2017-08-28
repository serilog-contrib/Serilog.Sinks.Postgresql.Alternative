using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.PostgreSQL
{
    public class PostgreSQLSink : PeriodicBatchingSink
    {
        private readonly string _connectionString;

        private readonly string _tableName;
        private readonly IDictionary<string, ColumnWriterBase> _columnOptions;
        private readonly IFormatProvider _formatProvider;
        private readonly bool _useCopy;

        private readonly string _schemaName;

        public const int DefaultBatchSizeLimit = 30;
        public const int DefaultQueueLimit = Int32.MaxValue;


        public PostgreSQLSink(string connectionString,
            string tableName,
            TimeSpan period,
            IFormatProvider formatProvider = null,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            bool useCopy = true,
            string schemaName = "") : base(batchSizeLimit, period)
        {
            _connectionString = connectionString;
            _tableName = tableName;

            _schemaName = schemaName;

            _formatProvider = formatProvider;
            _useCopy = useCopy;

            _columnOptions = columnOptions ?? ColumnOptions.Default;
        }


        public PostgreSQLSink(string connectionString,
            string tableName,
            TimeSpan period,
            IFormatProvider formatProvider = null,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            int queueLimit = DefaultQueueLimit,
            bool useCopy = true,
            string schemaName = "") : base(batchSizeLimit, period, queueLimit)
        {
            _connectionString = connectionString;
            _tableName = tableName;

            _schemaName = schemaName;

            _formatProvider = formatProvider;
            _useCopy = useCopy;

            _columnOptions = columnOptions ?? ColumnOptions.Default;
        }


        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                if (_useCopy)
                {
                    ProcessEventsByCopyCommand(events, connection);
                }
                else
                {
                    ProcessEventsByInsertStatements(events, connection);
                }
            }
        }

        private void ProcessEventsByInsertStatements(IEnumerable<LogEvent> events, NpgsqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = GetInsertQuery();


                foreach (var logEvent in events)
                {
                    //TODO: Init once
                    command.Parameters.Clear();
                    foreach (var columnOption in _columnOptions)
                    {
                        command.Parameters.AddWithValue(columnOption.Key, columnOption.Value.DbType,
                            columnOption.Value.GetValue(logEvent, _formatProvider));
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        private void ProcessEventsByCopyCommand(IEnumerable<LogEvent> events, NpgsqlConnection connection)
        {
            using (var binaryCopyWriter = connection.BeginBinaryImport(GetCopyCommand()))
            {
                WriteToStream(binaryCopyWriter, events);
            }
        }

        private string GetCopyCommand()
        {
            string schemaPrefix = GetSchemaPrefix();

            var columns = String.Join(", ", _columnOptions.Keys);

            return $"COPY {schemaPrefix}{_tableName}({columns}) FROM STDIN BINARY;";

        }

        private string GetInsertQuery()
        {
            string schemaPrefix = GetSchemaPrefix();

            var columns = String.Join(", ", _columnOptions.Keys);

            var parameters = String.Join(", ", _columnOptions.Keys.Select(cn => ":" + cn));

            return $@"INSERT INTO {schemaPrefix}{_tableName} ({columns})
                                        VALUES ({parameters})";
        }

        private string GetSchemaPrefix()
        {
            if (!String.IsNullOrEmpty(_schemaName))
            {
                return _schemaName + ".";
            }

            return String.Empty;
        }


        private void WriteToStream(NpgsqlBinaryImporter writer, IEnumerable<LogEvent> entities)
        {
            foreach (var entity in entities)
            {
                writer.StartRow();

                foreach (var columnKey in _columnOptions.Keys)
                {
                    writer.Write(_columnOptions[columnKey].GetValue(entity, _formatProvider), _columnOptions[columnKey].DbType);
                }
            }
        }
    }
}