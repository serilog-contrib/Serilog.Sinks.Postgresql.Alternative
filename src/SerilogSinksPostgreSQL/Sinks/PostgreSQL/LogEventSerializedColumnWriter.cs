// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogEventSerializedColumnWriter.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write the log event as JSON.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    using NpgsqlTypes;

    using Events;
    using Formatting.Json;

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     This class is used to write the log event as JSON.
    /// </summary>
    /// <seealso cref="ColumnWriterBase" />
    public class LogEventSerializedColumnWriter : ColumnWriterBase
    {
        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogEventSerializedColumnWriter" /> class.
        /// </summary>
        /// <param name="dbType">The column type.</param>
        /// <seealso cref="ColumnWriterBase" />
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public LogEventSerializedColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Jsonb)
            : base(dbType)
        {
        }

        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Gets the part of the log event to write to the column.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        ///     An object value.
        /// </returns>
        /// <seealso cref="ColumnWriterBase" />
        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return LogEventToJson(logEvent, formatProvider);
        }

        /// <summary>
        ///     Converts the log event to json.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>The log event as json string.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static object LogEventToJson(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var jsonFormatter = new JsonFormatter(formatProvider: formatProvider);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                jsonFormatter.Format(logEvent, writer);
            }

            return sb.ToString();
        }
    }
}