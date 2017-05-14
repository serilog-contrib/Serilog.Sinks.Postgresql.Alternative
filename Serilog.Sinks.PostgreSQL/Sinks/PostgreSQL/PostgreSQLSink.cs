using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Serilog.Debugging;
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


        public const int DefaultBatchSizeLimit = 30;
        public const int DefaultQueueLimit = 30;


        public PostgreSQLSink(string connectionString,
            string tableName,
            TimeSpan period,
            IFormatProvider formatProvider = null,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            int queueLimit = DefaultQueueLimit,
            bool useCopy = true) : base(batchSizeLimit, period, queueLimit)
        {
            _connectionString = connectionString;
            _tableName = tableName;

            _formatProvider = formatProvider;
            _useCopy = useCopy;

            _columnOptions = columnOptions ?? ColumnOptions.Default;
        }


        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            try
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
            catch (Exception e)
            {
                SelfLog.WriteLine("Unable to write {0} log events to the database due to following error: {1}", events.Count(), e.Message);
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
            var columns = String.Join(", ", _columnOptions.Keys);

            return $"COPY {_tableName}({columns}) FROM STDIN BINARY;";

        }

        private string GetInsertQuery()
        {
            var columns = String.Join(", ", _columnOptions.Keys);

            var parameters = String.Join(", ", _columnOptions.Keys.Select(cn => ":" + cn));

            return $@"INSERT INTO {_tableName} ({columns})
                                        VALUES ({parameters})";
        }


        private void WriteToStream(NpgsqlBinaryImporter writer, IEnumerable<LogEvent> entities)
        {
            foreach (var entity in entities)
            {
                writer.StartRow();

                foreach (var columnKey in _columnOptions.Keys)
                {
                    writer.Write(_columnOptions[columnKey].GetValue(entity), _columnOptions[columnKey].DbType);
                }
            }
        }
    }
}