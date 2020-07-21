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

    using Serilog.Configuration;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Sinks.PostgreSQL;

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
        ///     Default time to wait between checking for event batches.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

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
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once UnusedMember.Global
        public static LoggerConfiguration PostgreSql(
            this LoggerSinkConfiguration sinkConfiguration,
            string connectionString,
            string tableName,
            IDictionary<string, ColumnWriterBase> columnOptions = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            int batchSizeLimit = PostgreSqlSink.DefaultBatchSizeLimit,
            LoggingLevelSwitch levelSwitch = null,
            bool useCopy = true,
            string schemaName = "",
            bool needAutoCreateTable = false)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            period = period ?? DefaultPeriod;

            return sinkConfiguration.Sink(
                new PostgreSqlSink(
                    connectionString,
                    tableName,
                    period.Value,
                    formatProvider,
                    columnOptions,
                    batchSizeLimit,
                    useCopy,
                    schemaName,
                    needAutoCreateTable),
                restrictedToMinimumLevel,
                levelSwitch);
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
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration PostgreSql(
            this LoggerSinkConfiguration sinkConfiguration,
            string connectionString,
            string tableName,
            IDictionary<string, string> loggerColumnOptions = null,
            IDictionary<string, SinglePropertyColumnWriter> loggerPropertyColumnOptions = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            int batchSizeLimit = PostgreSqlSink.DefaultBatchSizeLimit,
            LoggingLevelSwitch levelSwitch = null,
            bool useCopy = true,
            string schemaName = "",
            bool needAutoCreateTable = false)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

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
                return sinkConfiguration.PostgreSql(
                    connectionString,
                    tableName,
                    columns,
                    restrictedToMinimumLevel,
                    period,
                    formatProvider,
                    batchSizeLimit,
                    levelSwitch,
                    useCopy,
                    schemaName,
                    needAutoCreateTable);
            }

            columns = columns ?? new Dictionary<string, ColumnWriterBase>();

            foreach (var columnOption in loggerPropertyColumnOptions)
            {
                columns.Add(columnOption.Key, columnOption.Value);
            }

            return sinkConfiguration.PostgreSql(
                connectionString,
                tableName,
                columns,
                restrictedToMinimumLevel,
                period,
                formatProvider,
                batchSizeLimit,
                levelSwitch,
                useCopy,
                schemaName,
                needAutoCreateTable);
        }
    }
}