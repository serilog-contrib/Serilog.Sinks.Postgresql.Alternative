// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSQLAuditSink.cs" company="TerumoBCT">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is the main class and contains all options for the PostgreSQL audit sink.
//   This class is based on the existing PostgreSqlSink class
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
   using Serilog.Sinks.PostgreSQL.ColumnWriters;
   using Serilog.Core;

   /// <summary>
   /// Writes log events as rows in a table of PostgreSQL database using Audit logic, meaning that each row is synchronously committed
   /// and any errors that occur are propagated to the caller.
   /// </summary>
   public class PostgreSqlAuditSink : ILogEventSink, IDisposable
   {
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
      ///  The failure callback.
      /// </summary>
      private readonly Action<Exception> failureCallback;

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

      /// <inheritdoc cref="PeriodicBatchingSink" />
      /// <summary>
      ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
      /// </summary>
      /// <param name="connectionString">The connection string.</param>
      /// <param name="tableName">Name of the table.</param>
      /// <param name="formatProvider">The format provider.</param>
      /// <param name="columnOptions">The column options.</param>
      /// <param name="schemaName">Name of the schema.</param>
      /// <param name="needAutoCreateTable">Specifies whether the table should be auto-created if it does not already exist or not.</param>
      /// <param name="needAutoCreateSchema">Specifies whether the schema should be auto-created if it does not already exist or not.</param>
      /// <param name="failureCallback">The failure callback.</param>
      public PostgreSqlAuditSink(
          string connectionString,
          string tableName,
          IFormatProvider formatProvider = null,
          IDictionary<string, ColumnWriterBase> columnOptions = null,
          string schemaName = "",
          bool needAutoCreateTable = false,
          bool needAutoCreateSchema = false,
          Action<Exception> failureCallback = null)
      {
         this.connectionString = connectionString;

         this.schemaName = schemaName.Replace("\"", string.Empty);
         this.tableName = tableName.Replace("\"", string.Empty);

         this.formatProvider = formatProvider;

         this.columnOptions = columnOptions ?? ColumnOptions.Default;

         this.ClearQuotationMarksFromColumnOptions();

         this.isTableCreated = !needAutoCreateTable;
         this.isSchemaCreated = !needAutoCreateSchema;

         this.failureCallback = failureCallback;
      }

      /// <summary>
      /// Emit the provided log event to the sink.
      /// </summary>
      /// <param name="logEvent"> a log event to emit </param>
      public void Emit(LogEvent logEvent)
      {
         using var connection = new NpgsqlConnection(this.connectionString);
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

         this.ProcessEventsByInsertStatement(logEvent, connection);
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
      ///     Processes the event by insert statement.
      /// </summary>
      /// <param name="logEvent">The event.</param>
      /// <param name="connection">The connection.</param>
      private void ProcessEventsByInsertStatement(LogEvent logEvent, NpgsqlConnection connection)
      {
         using var command = connection.CreateCommand();
         command.CommandText = this.GetInsertQuery();

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

      /// <summary>
      /// The columns names without skipped columns.
      /// </summary>
      /// <returns>The list of column names for the INSERT query.</returns>
      private IEnumerable<string> ColumnNamesWithoutSkipped() =>
          this.columnOptions
              .Where(c => !c.Value.SkipOnInsert)
              .Select(c => c.Key);
      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// Releases the unmanaged resources used by the Serilog.Sinks.MSSqlServer.MSSqlServerAuditSink and optionally
      /// releases the managed resources.
      /// </summary>
      /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
      protected virtual void Dispose(bool disposing)
      {
         // This class needn't to dispose anything. This is just here for sink interface compatibility.
      }
   }
}