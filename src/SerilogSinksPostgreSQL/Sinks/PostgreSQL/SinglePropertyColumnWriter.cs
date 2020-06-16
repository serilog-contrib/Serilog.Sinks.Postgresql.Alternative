// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SinglePropertyColumnWriter.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Defines the SinglePropertyColumnWriter type.
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
    ///     This class is used to write a single event property.
    /// </summary>
    /// <seealso cref="ColumnWriterBase" />
    public class SinglePropertyColumnWriter : ColumnWriterBase
    {
        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="SinglePropertyColumnWriter" /> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="writeMethod">The write method.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="format">The format.</param>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public SinglePropertyColumnWriter(
            string propertyName,
            PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString,
            NpgsqlDbType dbType = NpgsqlDbType.Text,
            string format = null)
            : base(dbType)
        {
            this.Name = propertyName;
            this.WriteMethod = writeMethod;
            this.Format = format;
        }

        /// <summary>
        ///     Gets the format.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string Format { get; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string Name { get; }

        /// <summary>
        ///     Gets the write method.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public PropertyWriteMethod WriteMethod { get; }

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
            if (!logEvent.Properties.ContainsKey(this.Name))
            {
                return DBNull.Value;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (this.WriteMethod)
            {
                case PropertyWriteMethod.Raw:
                    return GetPropertyValue(logEvent.Properties[this.Name]);
                case PropertyWriteMethod.Json:
                    var valuesFormatter = new JsonValueFormatter();

                    var sb = new StringBuilder();

                    using (var writer = new StringWriter(sb))
                    {
                        valuesFormatter.Format(logEvent.Properties[this.Name], writer);
                    }

                    return sb.ToString();

                default:
                    return logEvent.Properties[this.Name].ToString(this.Format, formatProvider);
            }
        }

        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <param name="logEventProperty">The log event property.</param>
        /// <returns>The property value.</returns>
        private static object GetPropertyValue(LogEventPropertyValue logEventProperty)
        {
            // TODO: Add support for arrays
            if (logEventProperty is ScalarValue scalarValue)
            {
                return scalarValue.Value;
            }

            return logEventProperty;
        }
    }
}