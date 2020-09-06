// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSQLSink.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is the main class and contains all options for the PostgreSQL sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Npgsql;

    using Serilog.Events;
    using Serilog.Sinks.PeriodicBatching;

    /// <inheritdoc cref="PeriodicBatchingSink" />
    /// <summary>
    ///     This class is the main class and contains all options for the PostgreSQL sink.
    /// </summary>
    /// <seealso cref="PeriodicBatchingSink" />
    public class PostgreSqlSink : PeriodicBatchingSink
    {
        /// <summary>
        ///     The default batch size limit.
        /// </summary>
        public const int DefaultBatchSizeLimit = 30;

        /// <summary>
        ///     The default queue limit.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public const int DefaultQueueLimit = int.MaxValue;

        /// <summary>
        ///     The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        ///     The format provider.
        /// </summary>
        private readonly IFormatProvider formatProvider;

        /// <summary>
        ///     The table name.
        /// </summary>
        private readonly string tableName;

        /// <summary>
        ///     The schema name.
        /// </summary>
        private readonly string schemaName;

        /// <summary>
        ///     A boolean value indicating if the copy is used.
        /// </summary>
        private readonly bool useCopy;

        /// <summary>
        ///     The column options.
        /// </summary>
        private IDictionary<string, ColumnWriterBase> columnOptions;

        /// <summary>
        ///     A boolean value indicating whether the table is created or not.
        /// </summary>
        private bool isTableCreated;

        /// <summary>
        ///     A boolean value indicating whether the schema is created or not.
        /// </summary>
        private bool isSchemaCreated;

        /// <summary>
        ///     A boolean value indicating whether the log level table is created or not.
        /// </summary>
        private bool isLogLevelTableCreated;

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="columnOptions">The column options.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
        /// <param name="useCopy">Enables the copy command to allow batch inserting instead of multiple INSERT commands.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="needAutoCreateTable">Specifies whether the table should be auto-created if it does not already exist or not.</param>
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
            this.connectionString = connectionString;

            this.schemaName = schemaName.Replace("\"", string.Empty);
            this.tableName = tableName.Replace("\"", string.Empty);

            this.formatProvider = formatProvider;
            this.useCopy = useCopy;

            this.columnOptions = columnOptions ?? ColumnOptions.Default;

            this.ClearQuotationMarksFromColumnOptions();

            this.isTableCreated = !needAutoCreateTable;
            this.isSchemaCreated = false;
        }

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="columnOptions">The column options.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
        /// <param name="queueLimit">Maximum number of events in the queue.</param>
        /// <param name="useCopy">Enables the copy command to allow batch inserting instead of multiple INSERT commands.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="needAutoCreateTable">Specifies whether the table should be auto-created if it does not already exist or not.</param>
        // ReSharper disable once UnusedMember.Global
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
            this.connectionString = connectionString;

            this.schemaName = schemaName.Replace("\"", string.Empty);
            this.tableName = tableName.Replace("\"", string.Empty);

            this.formatProvider = formatProvider;
            this.useCopy = useCopy;

            this.columnOptions = columnOptions ?? ColumnOptions.Default;

            this.isTableCreated = !needAutoCreateTable;
            this.isSchemaCreated = false;
        }

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Emit a batch of log events, running to completion synchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <remarks>
        ///     Override either
        ///     <see
        ///         cref="M:Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink.EmitBatch(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent})" />
        ///     or
        ///     <see
        ///         cref="M:Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink.EmitBatchAsync(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent})" />
        ///     ,
        ///     not both.
        /// </remarks>
        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            using (var connection = new NpgsqlConnection(this.connectionString))
            {
                connection.Open();

                if (!this.isSchemaCreated && !string.IsNullOrWhiteSpace(this.schemaName))
                {
                    SchemaCreator.CreateSchema(connection, this.schemaName);
                    this.isSchemaCreated = true;
                }

                if (!this.isTableCreated)
                {
                    TableCreator.CreateTable(connection, this.schemaName, this.tableName, this.columnOptions);
                    this.isTableCreated = true;
                }

                if (!this.isLogLevelTableCreated)
                {
                    TableCreator.CreateLogLevelTable(connection);
                    this.isLogLevelTableCreated = true;
                }

                if (this.useCopy)
                {
                    this.ProcessEventsByCopyCommand(events, connection);
                }
                else
                {
                    this.ProcessEventsByInsertStatements(events, connection);
                }
            }
        }

        /// <summary>
        ///     Clears the name of the column name for parameter.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The cleared column name.</returns>
        private static string ClearColumnNameForParameterName(string columnName)
        {
            return columnName?.Replace("\"", string.Empty);
        }

        /// <summary>
        ///     Clears the quotation marks from the column options.
        /// </summary>
        private void ClearQuotationMarksFromColumnOptions()
        {
            var result = new Dictionary<string, ColumnWriterBase>(this.columnOptions);

            foreach (var keyValuePair in this.columnOptions)
            {
                if (!keyValuePair.Key.Contains("\""))
                {
                    continue;
                }

                result.Remove(keyValuePair.Key);
                result[keyValuePair.Key.Replace("\"", string.Empty)] = keyValuePair.Value;
            }

            this.columnOptions = result;
        }

        /// <summary>
        ///     Gets the copy command.
        /// </summary>
        /// <returns>A SQL string with the copy command.</returns>
        private string GetCopyCommand()
        {
            var columns = "\"" + string.Join("\", \"", this.ColumnNamesWithoutSkipped()) + "\"";
            var builder = new StringBuilder();
            builder.Append("COPY ");

            if (!string.IsNullOrWhiteSpace(this.schemaName))
            {
                builder.Append("\"");
                builder.Append(this.schemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(this.tableName);
            builder.Append("\"(");
            builder.Append(columns);
            builder.Append(") FROM STDIN BINARY;");
            return builder.ToString();
        }

        /// <summary>
        ///     Gets the insert query.
        /// </summary>
        /// <returns>A SQL string with the insert query.</returns>
        private string GetInsertQuery()
        {
            var columns = "\"" + string.Join("\", \"", this.ColumnNamesWithoutSkipped()) + "\"";

            var parameters = string.Join(
                ", ",
                this.ColumnNamesWithoutSkipped().Select(cn => "@" + ClearColumnNameForParameterName(cn)));

            var builder = new StringBuilder();
            builder.Append("INSERT INTO ");

            if (!string.IsNullOrWhiteSpace(this.schemaName))
            {
                builder.Append("\"");
                builder.Append(this.schemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(this.tableName);
            builder.Append("\"(");
            builder.Append(columns);
            builder.Append(") VALUES (");
            builder.Append(parameters);
            builder.Append(");");
            return builder.ToString();
        }

        /// <summary>
        ///     Processes the events by the copy command.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="connection">The connection.</param>
        private void ProcessEventsByCopyCommand(IEnumerable<LogEvent> events, NpgsqlConnection connection)
        {
            using (var binaryCopyWriter = connection.BeginBinaryImport(this.GetCopyCommand()))
            {
                this.WriteToStream(binaryCopyWriter, events);
                binaryCopyWriter.Complete();
            }
        }

        /// <summary>
        ///     Processes the events by insert statements.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="connection">The connection.</param>
        private void ProcessEventsByInsertStatements(IEnumerable<LogEvent> events, NpgsqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = this.GetInsertQuery();

                foreach (var logEvent in events)
                {
                    command.Parameters.Clear();
                    foreach (var columnKey in this.ColumnNamesWithoutSkipped())
                    {
                        command.Parameters.AddWithValue(
                            ClearColumnNameForParameterName(columnKey),
                            this.columnOptions[columnKey].DbType,
                            this.columnOptions[columnKey].GetValue(logEvent, this.formatProvider));
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Writes to the stream.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="entities">The entities.</param>
        private void WriteToStream(NpgsqlBinaryImporter writer, IEnumerable<LogEvent> entities)
        {
            foreach (var entity in entities)
            {
                writer.StartRow();

                foreach (var columnKey in this.ColumnNamesWithoutSkipped())
                {
                    writer.Write(
                        this.columnOptions[columnKey].GetValue(entity, this.formatProvider),
                        this.columnOptions[columnKey].DbType);
                }
            }
        }

        /// <summary>
        /// The columns names without skipped columns.
        /// </summary>
        /// <returns>The list of column names for the INSERT query.</returns>
        private IEnumerable<string> ColumnNamesWithoutSkipped() =>
            this.columnOptions
                .Where(c => !c.Value.SkipOnInsert)
                .Select(c => c.Key);
    }
}