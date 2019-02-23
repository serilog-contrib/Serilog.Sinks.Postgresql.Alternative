// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSQLSink.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the PostgreSqlSink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        ///     The column options.
        /// </summary>
        private readonly IDictionary<string, ColumnWriterBase> columnOptions;

        /// <summary>
        ///     The connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        ///     The format provider.
        /// </summary>
        private readonly IFormatProvider formatProvider;

        /// <summary>
        ///     The full table name.
        /// </summary>
        private readonly string fullTableName;

        /// <summary>
        ///     A boolean value indicating if the copy is used.
        /// </summary>
        private readonly bool useCopy;

        /// <summary>
        ///     A boolean value indicating whether the table is created or not.
        /// </summary>
        private bool isTableCreated;

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="period">The period.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="columnOptions">The column options.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="useCopy">if set to <c>true</c> [use copy].</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="needAutoCreateTable">if set to <c>true</c> [need automatic create table].</param>
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

            this.fullTableName = GetFullTableName(tableName, schemaName);

            this.formatProvider = formatProvider;
            this.useCopy = useCopy;

            this.columnOptions = columnOptions ?? ColumnOptions.Default;

            this.isTableCreated = !needAutoCreateTable;
        }

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="period">The period.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="columnOptions">The column options.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="queueLimit">The queue limit.</param>
        /// <param name="useCopy">if set to <c>true</c> [use copy].</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="needAutoCreateTable">if set to <c>true</c> [need automatic create table].</param>
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

            this.fullTableName = GetFullTableName(tableName, schemaName);

            this.formatProvider = formatProvider;
            this.useCopy = useCopy;

            this.columnOptions = columnOptions ?? ColumnOptions.Default;

            this.isTableCreated = !needAutoCreateTable;
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

                if (!this.isTableCreated)
                {
                    TableCreator.CreateTable(connection, this.fullTableName, this.columnOptions);
                    this.isTableCreated = true;
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
        ///     Gets the full name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns>The full table name.</returns>
        private static string GetFullTableName(string tableName, string schemaName)
        {
            var schemaPrefix = string.Empty;
            if (!string.IsNullOrEmpty(schemaName))
            {
                schemaPrefix = $"{schemaName}.";
            }

            return schemaPrefix + tableName;
        }

        /// <summary>
        ///     Gets the copy command.
        /// </summary>
        /// <returns>A SQL string with the copy command.</returns>
        private string GetCopyCommand()
        {
            var columns = string.Join(", ", this.columnOptions.Keys);

            return $"COPY {this.fullTableName}({columns}) FROM STDIN BINARY;";
        }

        /// <summary>
        ///     Gets the insert query.
        /// </summary>
        /// <returns>A SQL string with the insert query.</returns>
        private string GetInsertQuery()
        {
            var columns = string.Join(", ", this.columnOptions.Keys);

            var parameters = string.Join(
                ", ",
                this.columnOptions.Keys.Select(cn => ":" + ClearColumnNameForParameterName(cn)));

            return $@"INSERT INTO {this.fullTableName} ({columns})
                                        VALUES ({parameters})";
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
                    // TODO: Init once
                    command.Parameters.Clear();
                    foreach (var columnOption in this.columnOptions)
                    {
                        command.Parameters.AddWithValue(
                            ClearColumnNameForParameterName(columnOption.Key),
                            columnOption.Value.DbType,
                            columnOption.Value.GetValue(logEvent, this.formatProvider));
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

                foreach (var columnKey in this.columnOptions.Keys)
                {
                    writer.Write(
                        this.columnOptions[columnKey].GetValue(entity, this.formatProvider),
                        this.columnOptions[columnKey].DbType);
                }
            }
        }
    }
}