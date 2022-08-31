// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SinglePropertyColumnWriter.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write a single event property.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.ColumnWriters;

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
    public SinglePropertyColumnWriter() : base(NpgsqlDbType.Text, order: 0)
    {
    }

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="SinglePropertyColumnWriter" /> class.
    /// </summary>
    /// <param name="order">
    /// The order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </param>
    /// <seealso cref="ColumnWriterBase" />
    public SinglePropertyColumnWriter(int? order = null) : base(NpgsqlDbType.Text, order: order)
    {
    }

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="SinglePropertyColumnWriter" /> class.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="writeMethod">The write method.</param>
    /// <param name="dbType">Type of the database.</param>
    /// <param name="format">The format.</param>
    /// <param name="order">
    /// The order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </param>
    /// <seealso cref="ColumnWriterBase" />
    public SinglePropertyColumnWriter(
        string propertyName,
        PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString,
        NpgsqlDbType dbType = NpgsqlDbType.Text,
        string? format = null,
        int? order = null)
        : base(dbType, order: order)
    {
        this.Name = propertyName;
        this.WriteMethod = writeMethod;
        this.Format = format;
    }

    /// <summary>
    ///     Gets or sets the format.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the write method.
    /// </summary>
    public PropertyWriteMethod WriteMethod { get; set; }

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
    public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
    {
        if (!logEvent.Properties.ContainsKey(this.Name))
        {
            return DBNull.Value;
        }

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
            // In case of an enum and write method "raw", write it as integer.
            if (scalarValue.Value is Enum enumValue)
            {
                return Convert.ToInt32(enumValue);
            }

            return scalarValue.Value;
        }

        return logEventProperty;
    }
}
