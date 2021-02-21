// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerConfigurationPostgreSQLExtensions.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the PostgreSQL logger configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Configuration;

    using Core;

    using Events;

    using Microsoft.Extensions.Configuration;

    using NpgsqlTypes;

    using Serilog.Sinks.PostgreSQL.ColumnWriters;
    using Serilog.Sinks.PostgreSQL.Configuration;

    using Sinks.PostgreSQL;

    /// <summary>
    ///     This class contains the PostgreSQL logger configuration.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once UnusedMember.Global
    public static class LoggerConfigurationPostgreSqlExtensions
    {
        /// <summary>
        ///     The default batch size limit.
        /// </summary>
        private const int DefaultBatchSizeLimit = 30;

        /// <summary>
        ///     Default time to wait between checking for event batches.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The Microsoft extensions connection string provider.
        /// </summary>
        private static readonly IMicrosoftExtensionsConnectionStringProvider MicrosoftExtensionsConnectionStringProvider
            = new MicrosoftExtensionsConnectionStringProvider();

        /// <summary>
        ///     Adds a sink which writes to the PostgreSQL table.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="columnOptions">The column options.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include to single batch.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <param name="useCopy">If true inserts data via COPY command, otherwise uses INSERT INTO statement.</param>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="needAutoCreateTable">A <seealso cref="bool"/> value indicating whether the table should be auto created or not.</param>
        /// <param name="needAutoCreateSchema">Specifies whether the schema should be auto-created if it does not already exist or not.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="appConfiguration">The app configuration section. Required if the connection string is a name.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once InconsistentNaming
        public static LoggerConfiguration PostgreSQL(
            this LoggerSinkConfiguration sinkConfiguration,
            string connectionString,
            string tableName,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            LoggingLevelSwitch levelSwitch = null,
            bool useCopy = true,
            string schemaName = "",
            bool needAutoCreateTable = false,
            bool needAutoCreateSchema = false,
            Action<Exception> failureCallback = null,
            IConfiguration appConfiguration = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (appConfiguration != null)
            {
                connectionString =
                    MicrosoftExtensionsConnectionStringProvider.GetConnectionString(connectionString, appConfiguration);
            }

            period ??= DefaultPeriod;

            var optionsLocal = GetOptions(
                connectionString,
                tableName,
                columnOptions,
                period.Value,
                formatProvider,
                batchSizeLimit,
                useCopy,
                schemaName,
                needAutoCreateTable,
                needAutoCreateSchema,
                failureCallback);

            return sinkConfiguration.Sink(new PostgreSqlSink(optionsLocal), restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>
        /// Adds a sink which writes to the PostgreSQL table. The configuration for the sink can be taken from the JSON file.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="loggerColumnOptions">The logger column options.</param>
        /// <param name="loggerPropertyColumnOptions">The logger property column options.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include to single batch.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <param name="useCopy">If true inserts data via COPY command, otherwise uses INSERT INTO statement.</param>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="needAutoCreateTable">A <seealso cref="bool"/> value indicating whether the table should be auto created or not.</param>
        /// <param name="needAutoCreateSchema">Specifies whether the schema should be auto-created if it does not already exist or not.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="appConfiguration">The app configuration section. Required if the connection string is a name.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        // ReSharper disable once InconsistentNaming
        public static LoggerConfiguration PostgreSQL(
            this LoggerSinkConfiguration sinkConfiguration,
            string connectionString,
            string tableName,
            IDictionary<string, string> loggerColumnOptions = null,
            IDictionary<string, SinglePropertyColumnWriter> loggerPropertyColumnOptions = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            int batchSizeLimit = DefaultBatchSizeLimit,
            LoggingLevelSwitch levelSwitch = null,
            bool useCopy = true,
            string schemaName = "",
            bool needAutoCreateTable = false,
            bool needAutoCreateSchema = false,
            Action<Exception> failureCallback = null,
            IConfiguration appConfiguration = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (appConfiguration != null)
            {
                connectionString =
                    MicrosoftExtensionsConnectionStringProvider.GetConnectionString(connectionString, appConfiguration);
            }

            period ??= DefaultPeriod;

            IDictionary<string, ColumnWriterBase> columns = null;

            if (loggerColumnOptions != null)
            {
                columns = new Dictionary<string, ColumnWriterBase>();

                foreach (var columnOption in loggerColumnOptions)
                {
                    switch (columnOption.Value)
                    {
                        case "Level":
                            columns.Add(columnOption.Key, new LevelColumnWriter());
                            break;
                        case "LevelAsText":
                            columns.Add(columnOption.Key, new LevelColumnWriter(true, NpgsqlDbType.Text));
                            break;
                        case "Timestamp":
                            columns.Add(columnOption.Key, new TimestampColumnWriter());
                            break;
                        case "LogEvent":
                            columns.Add(columnOption.Key, new LogEventSerializedColumnWriter());
                            break;
                        case "Properties":
                            columns.Add(columnOption.Key, new PropertiesColumnWriter());
                            break;
                        case "Message":
                            columns.Add(columnOption.Key, new MessageTemplateColumnWriter());
                            break;
                        case "RenderedMessage":
                            columns.Add(columnOption.Key, new RenderedMessageColumnWriter());
                            break;
                        case "Exception":
                            columns.Add(columnOption.Key, new ExceptionColumnWriter());
                            break;
                        case "IdAutoIncrement":
                            columns.Add(columnOption.Key, new IdAutoIncrementColumnWriter());
                            break;
                    }
                }
            }

            if (loggerPropertyColumnOptions == null)
            {
                var optionsLocal = GetOptions(
                    connectionString,
                    tableName,
                    columns,
                    period.Value,
                    formatProvider,
                    batchSizeLimit,
                    useCopy,
                    schemaName,
                    needAutoCreateTable,
                    needAutoCreateSchema,
                    failureCallback);

                return sinkConfiguration.Sink(new PostgreSqlSink(optionsLocal), restrictedToMinimumLevel, levelSwitch);
            }

            columns ??= new Dictionary<string, ColumnWriterBase>();

            foreach (var columnOption in loggerPropertyColumnOptions)
            {
                columns.Add(columnOption.Key, columnOption.Value);
            }

            var optionsLocal2 = GetOptions(
                connectionString,
                tableName,
                columns,
                period.Value,
                formatProvider,
                batchSizeLimit,
                useCopy,
                schemaName,
                needAutoCreateTable,
                needAutoCreateSchema,
                failureCallback);

            return sinkConfiguration.Sink(new PostgreSqlSink(optionsLocal2), restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>
        ///     Clears the quotation marks from the column options.
        /// </summary>
        internal static IDictionary<string, ColumnWriterBase> ClearQuotationMarksFromColumnOptions(
            IDictionary<string, ColumnWriterBase> columnOptions)
        {
            var result = new Dictionary<string, ColumnWriterBase>(columnOptions);

            foreach (var keyValuePair in columnOptions)
            {
                if (!keyValuePair.Key.Contains("\""))
                {
                    continue;
                }

                result.Remove(keyValuePair.Key);
                result[keyValuePair.Key.Replace("\"", string.Empty)] = keyValuePair.Value;
            }

            return result;
        }

        /// <summary>
        /// Gets the column options.
        /// </summary>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="columnOptions">The current column options.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="batchSizeLimit">The maximum number of events to include to single batch.</param>
        /// <param name="useCopy">If true inserts data via COPY command, otherwise uses INSERT INTO statement.</param>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="needAutoCreateTable">A <seealso cref="bool"/> value indicating whether the table should be auto created or not.</param>
        /// <param name="needAutoCreateSchema">Specifies whether the schema should be auto-created if it does not already exist or not.</param>
        /// <param name="failureCallback">The failure callback.</param>
        internal static PostgreSqlOptions GetOptions(
            string connectionString,
            string tableName,
            IDictionary<string, ColumnWriterBase> columnOptions,
            TimeSpan period,
            IFormatProvider formatProvider,
            int batchSizeLimit,
            bool useCopy,
            string schemaName,
            bool needAutoCreateTable,
            bool needAutoCreateSchema,
            Action<Exception> failureCallback)
        {
            var columnOptionsLocal =
                ClearQuotationMarksFromColumnOptions(columnOptions ?? DefaultColumnOptions.Default);

            return new PostgreSqlOptions
            {
                ConnectionString = connectionString,
                TableName = tableName.Replace("\"", string.Empty),
                Period = period,
                FormatProvider = formatProvider,
                ColumnOptions = columnOptionsLocal,
                BatchSizeLimit = batchSizeLimit,
                UseCopy = useCopy,
                SchemaName = schemaName.Replace("\"", string.Empty),
                NeedAutoCreateTable = !needAutoCreateTable,
                NeedAutoCreateSchema = !needAutoCreateSchema,
                FailureCallback = failureCallback
            };
        }
    }
}