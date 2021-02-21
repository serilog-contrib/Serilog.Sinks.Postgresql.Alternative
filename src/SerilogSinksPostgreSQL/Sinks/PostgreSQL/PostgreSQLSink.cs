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

    using Events;
    using PeriodicBatching;

    using Serilog.Debugging;

    /// <inheritdoc cref="PeriodicBatchingSink" />
    /// <summary>
    ///     This class is the main class and contains all options for the PostgreSQL sink.
    /// </summary>
    /// <seealso cref="PeriodicBatchingSink" />
    public class PostgreSqlSink : PeriodicBatchingSink
    {
        /// <summary>
        /// The PostgreSQL options.
        /// </summary>
        private readonly PostgreSqlOptions sinkOptions;

        /// <summary>
        ///     A boolean value indicating whether the table is created or not.
        /// </summary>
        private bool isTableCreated;

        /// <summary>
        ///     A boolean value indicating whether the schema is created or not.
        /// </summary>
        private bool isSchemaCreated;

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
        /// </summary>
        /// <param name="options">The sink options.</param>
        public PostgreSqlSink(PostgreSqlOptions options) : base(options.BatchSizeLimit, options.Period)
        {
            this.sinkOptions = options;
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
            try
            {
                using var connection = new NpgsqlConnection(this.sinkOptions.ConnectionString);
                connection.Open();

                if (!this.isSchemaCreated && !string.IsNullOrWhiteSpace(this.sinkOptions.SchemaName))
                {
                    SchemaCreator.CreateSchema(connection, this.sinkOptions.SchemaName);
                    this.isSchemaCreated = true;
                }

                if (!this.isTableCreated)
                {
                    TableCreator.CreateTable(connection, this.sinkOptions.SchemaName, this.sinkOptions.TableName, this.sinkOptions.ColumnOptions);
                    this.isTableCreated = true;
                }

                if (this.sinkOptions.UseCopy)
                {
                    this.ProcessEventsByCopyCommand(events, connection);
                }
                else
                {
                    this.ProcessEventsByInsertStatements(events, connection);
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"{ex.Message} {ex.StackTrace}");
                this.sinkOptions.FailureCallback?.Invoke(ex);
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
        ///     Gets the copy command.
        /// </summary>
        /// <returns>A SQL string with the copy command.</returns>
        private string GetCopyCommand()
        {
            var columns = "\"" + string.Join("\", \"", this.ColumnNamesWithoutSkipped()) + "\"";
            var builder = new StringBuilder();
            builder.Append("COPY ");

            if (!string.IsNullOrWhiteSpace(this.sinkOptions.SchemaName))
            {
                builder.Append("\"");
                builder.Append(this.sinkOptions.SchemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(this.sinkOptions.SchemaName);
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

            if (!string.IsNullOrWhiteSpace(this.sinkOptions.SchemaName))
            {
                builder.Append("\"");
                builder.Append(this.sinkOptions.SchemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(this.sinkOptions.SchemaName);
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
            using var binaryCopyWriter = connection.BeginBinaryImport(this.GetCopyCommand());
            this.WriteToStream(binaryCopyWriter, events);
            binaryCopyWriter.Complete();
        }

        /// <summary>
        ///     Processes the events by insert statements.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="connection">The connection.</param>
        private void ProcessEventsByInsertStatements(IEnumerable<LogEvent> events, NpgsqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = this.GetInsertQuery();

            foreach (var logEvent in events)
            {
                command.Parameters.Clear();
                foreach (var columnKey in this.ColumnNamesWithoutSkipped())
                {
                    command.Parameters.AddWithValue(
                        ClearColumnNameForParameterName(columnKey),
                        this.sinkOptions.ColumnOptions[columnKey].DbType,
                        this.sinkOptions.ColumnOptions[columnKey].GetValue(logEvent, this.sinkOptions.FormatProvider));
                }

                command.ExecuteNonQuery();
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
                        this.sinkOptions.ColumnOptions[columnKey].GetValue(entity, this.sinkOptions.FormatProvider),
                        this.sinkOptions.ColumnOptions[columnKey].DbType);
                }
            }
        }

        /// <summary>
        /// The columns names without skipped columns.
        /// </summary>
        /// <returns>The list of column names for the INSERT query.</returns>
        private IEnumerable<string> ColumnNamesWithoutSkipped() =>
            this.sinkOptions.ColumnOptions
                .Where(c => !c.Value.SkipOnInsert)
                .Select(c => c.Key);
    }
}