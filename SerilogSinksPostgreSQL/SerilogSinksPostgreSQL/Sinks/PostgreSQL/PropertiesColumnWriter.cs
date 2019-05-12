// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesColumnWriter.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the PropertiesColumnWriter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    using NpgsqlTypes;

    using Serilog.Events;
    using Serilog.Formatting.Json;

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     This class is used to write all event properties.
    /// </summary>
    /// <seealso cref="ColumnWriterBase" />
    public class PropertiesColumnWriter : ColumnWriterBase
    {
        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertiesColumnWriter" /> class.
        /// </summary>
        /// <param name="dbType">The column type.</param>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public PropertiesColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Jsonb)
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
        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return PropertiesToJson(logEvent);
        }

        /// <summary>
        ///     Converts the properties to json.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>The properties as json object.</returns>
        private static object PropertiesToJson(LogEvent logEvent)
        {
            if (logEvent.Properties.Count == 0)
            {
                return "{}";
            }

            var valuesFormatter = new JsonValueFormatter();

            var sb = new StringBuilder();

            sb.Append("{");

            using (var writer = new StringWriter(sb))
            {
                foreach (var logEventProperty in logEvent.Properties)
                {
                    sb.Append($"\"{logEventProperty.Key}\":");
                    valuesFormatter.Format(logEventProperty.Value, writer);
                    sb.Append(", ");
                }
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");

            return sb.ToString();
        }
    }
}