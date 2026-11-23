// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesColumnWriter.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write all event properties.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.ColumnWriters;

/// <inheritdoc cref="ColumnWriterBase" />
/// <summary>
///     This class is used to write all event properties.
/// </summary>
/// <seealso cref="ColumnWriterBase" />
public sealed class PropertiesColumnWriter : ColumnWriterBase
{
    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertiesColumnWriter" /> class.
    /// </summary>
    public PropertiesColumnWriter() : base(NpgsqlDbType.Jsonb, order: 0)
    {
    }

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertiesColumnWriter" /> class.
    /// </summary>
    /// <param name="dbType">The column type.</param>
    /// <param name="order">
    /// The order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </param>
    /// <seealso cref="ColumnWriterBase" />
    public PropertiesColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Jsonb, int? order = null)
        : base(dbType, order: order)
    {
    }

    /// <summary>
    ///     Gets or sets the names of the properties that are left out of the JSON. The names are case-sensitive.
    /// </summary>
    public ICollection<string> ExcludedProperties { get; set; } = [];

    /// <summary>
    ///     Gets or sets a value indicating whether the properties that the same sink already writes to their own column
    ///     with a <see cref="SinglePropertyColumnWriter"/> are left out of the JSON.
    /// </summary>
    public bool ExcludeSinglePropertyColumns { get; set; }

    /// <summary>
    ///     Gets or sets the property names of the <see cref="SinglePropertyColumnWriter"/> columns of the sink. The sink
    ///     sets them when it is created, they are only used if <see cref="ExcludeSinglePropertyColumns"/> is set.
    /// </summary>
    internal ICollection<string> SinglePropertyColumnNames { get; set; } = [];

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
        return this.PropertiesToJson(logEvent);
    }

    /// <summary>
    ///     Converts the properties that are not excluded to json.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <returns>The properties as json object.</returns>
    private string PropertiesToJson(LogEvent logEvent)
    {
        var valuesFormatter = new JsonValueFormatter();
        using var writer = new StringWriter();
        var delimiter = string.Empty;

        writer.Write('{');

        foreach (var keyValuePair in logEvent.Properties)
        {
            if (this.IsExcluded(keyValuePair.Key))
            {
                continue;
            }

            writer.Write(delimiter);
            delimiter = ", ";
            JsonValueFormatter.WriteQuotedJsonString(keyValuePair.Key, writer);
            writer.Write(':');
            valuesFormatter.Format(keyValuePair.Value, writer);
        }

        writer.Write('}');
        return writer.ToString();
    }

    /// <summary>
    ///     Checks whether the property is left out of the JSON.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns><c>true</c> if the property is left out, <c>false</c> otherwise.</returns>
    private bool IsExcluded(string propertyName)
    {
        return this.ExcludedProperties.Contains(propertyName)
            || (this.ExcludeSinglePropertyColumns && this.SinglePropertyColumnNames.Contains(propertyName));
    }
}
