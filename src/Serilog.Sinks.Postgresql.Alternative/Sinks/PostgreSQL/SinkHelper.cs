// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SinkHelper.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The sink helper class to not duplicate the code in the audit sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Npgsql;

    using Serilog.Events;

    /// <summary>
    /// The sink helper class to not duplicate the code in the audit sink.
    /// </summary>
    public class SinkHelper
    {
        /// <summary>
        ///     A boolean value indicating whether the table is created or not.
        /// </summary>
        private bool isTableCreated;

        /// <summary>
        ///     A boolean value indicating whether the schema is created or not.
        /// </summary>
        private bool isSchemaCreated;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
        /// </summary>
        /// <param name="options">The sink options.</param>
        public SinkHelper(PostgreSqlOptions options)
        {
            this.SinkOptions = options;
            this.isSchemaCreated = !options.NeedAutoCreateSchema;
            this.isTableCreated = !options.NeedAutoCreateTable;
        }

        /// <summary>
        /// Gets or sets the PostgreSQL options.
        /// </summary>
        public PostgreSqlOptions SinkOptions { get; set; }

        /// <summary>
        /// Emits the events.
        /// </summary>
        /// <param name="events">The events.</param>
        public void Emit(IEnumerable<LogEvent> events)
        {
            using var connection = new NpgsqlConnection(this.SinkOptions.ConnectionString);
            connection.Open();

            if (this.SinkOptions.NeedAutoCreateSchema && !this.isSchemaCreated && !string.IsNullOrWhiteSpace(this.SinkOptions.SchemaName))
            {
                SchemaCreator.CreateSchema(connection, this.SinkOptions.SchemaName);
                this.isSchemaCreated = true;
            }

            if (this.SinkOptions.NeedAutoCreateTable && !this.isTableCreated && !string.IsNullOrWhiteSpace(this.SinkOptions.TableName))
            {
                if (this.SinkOptions.ColumnOptions.All(c => c.Value.Order != null))
                {
                    var columnOptions = this.SinkOptions.ColumnOptions.OrderBy(c => c.Value.Order)
                        .ToDictionary(c => c.Key, x => x.Value);
                    TableCreator.CreateTable(connection, this.SinkOptions.SchemaName, this.SinkOptions.TableName, columnOptions);
                }
                else
                {
                    TableCreator.CreateTable(connection, this.SinkOptions.SchemaName, this.SinkOptions.TableName, this.SinkOptions.ColumnOptions);
                }
                
                this.isTableCreated = true;
            }

            if (this.SinkOptions.UseCopy)
            {
                this.ProcessEventsByCopyCommand(events, connection);
            }
            else
            {
                this.ProcessEventsByInsertStatements(events, connection);
            }
        }

        /// <summary>
        ///     Processes the events by the copy command.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="connection">The connection.</param>
        public void ProcessEventsByCopyCommand(IEnumerable<LogEvent> events, NpgsqlConnection connection)
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
        public void ProcessEventsByInsertStatements(IEnumerable<LogEvent> events, NpgsqlConnection connection)
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
                        this.SinkOptions.ColumnOptions[columnKey].DbType,
                        this.SinkOptions.ColumnOptions[columnKey].GetValue(logEvent, this.SinkOptions.FormatProvider));
                }

                command.ExecuteNonQuery();
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

            if (!string.IsNullOrWhiteSpace(this.SinkOptions.SchemaName))
            {
                builder.Append("\"");
                builder.Append(this.SinkOptions.SchemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(this.SinkOptions.TableName);
            builder.Append("\"");

            builder.Append(" (");
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

            if (!string.IsNullOrWhiteSpace(this.SinkOptions.SchemaName))
            {
                builder.Append("\"");
                builder.Append(this.SinkOptions.SchemaName);
                builder.Append("\".");
            }

            builder.Append("\"");
            builder.Append(this.SinkOptions.TableName);
            builder.Append("\"");

            builder.Append(" (");
            builder.Append(columns);
            builder.Append(") VALUES (");
            builder.Append(parameters);
            builder.Append(");");
            return builder.ToString();
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
                        this.SinkOptions.ColumnOptions[columnKey].GetValue(entity, this.SinkOptions.FormatProvider),
                        this.SinkOptions.ColumnOptions[columnKey].DbType);
                }
            }
        }

        /// <summary>
        /// The columns names without skipped columns.
        /// </summary>
        /// <returns>The list of column names for the INSERT query.</returns>
        private IEnumerable<string> ColumnNamesWithoutSkipped() =>
            this.SinkOptions.ColumnOptions
                .Where(c => !c.Value.SkipOnInsert)
                .Select(c => c.Key);
    }
}