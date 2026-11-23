// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogEventSerializedColumnWriter.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write the log event as JSON.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.ColumnWriters;

/// <inheritdoc cref="ColumnWriterBase" />
/// <summary>
///     This class is used to write the log event as JSON.
/// </summary>
/// <seealso cref="ColumnWriterBase" />
public sealed class LogEventSerializedColumnWriter : ColumnWriterBase
{
    /// <summary>
    ///     A value indicating whether the timestamp is converted to UTC before the log event is serialized.
    /// </summary>
    private readonly bool useUtcTimestamp;

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="LogEventSerializedColumnWriter" /> class.
    /// </summary>
    public LogEventSerializedColumnWriter() : base(NpgsqlDbType.Jsonb, order: 0)
    {
    }

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="LogEventSerializedColumnWriter" /> class.
    /// </summary>
    /// <param name="dbType">The column type.</param>
    /// <param name="order">
    /// The order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </param>
    /// <seealso cref="ColumnWriterBase" />
    public LogEventSerializedColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Jsonb, int? order = null)
        : base(dbType, order: order)
    {
    }

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="LogEventSerializedColumnWriter" /> class.
    /// </summary>
    /// <param name="useUtcTimestamp">
    /// If set to <c>true</c>, the timestamp is converted to UTC before the log event is serialized, so that it ends
    /// in <c>+00:00</c> instead of the local offset. The point in time stays the same.
    /// </param>
    /// <param name="dbType">The column type.</param>
    /// <param name="order">
    /// The order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </param>
    /// <seealso cref="ColumnWriterBase" />
    public LogEventSerializedColumnWriter(bool useUtcTimestamp, NpgsqlDbType dbType = NpgsqlDbType.Jsonb, int? order = null)
        : base(dbType, order: order)
    {
        this.useUtcTimestamp = useUtcTimestamp;
    }

    /// <summary>
    ///     Gets or sets the names of the properties that are left out of the <c>Properties</c> of the JSON. The names
    ///     are case-sensitive.
    /// </summary>
    public ICollection<string> ExcludedProperties { get; set; } = [];

    /// <summary>
    ///     Gets or sets a value indicating whether the properties that the same sink already writes to their own column
    ///     with a <see cref="SinglePropertyColumnWriter"/> are left out of the <c>Properties</c> of the JSON.
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
        return LogEventToJson(this.PrepareLogEvent(logEvent), formatProvider);
    }

    /// <summary>
    ///     Gets the log event to serialize. If the timestamp is converted to UTC or properties are left out, this is a
    ///     copy, because the same log event is handed to the other columns and sinks as well. The JSON formatter writes
    ///     the timestamp with its offset and has no option for UTC.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <returns>The log event to serialize.</returns>
    private LogEvent PrepareLogEvent(LogEvent logEvent)
    {
        var excludesProperties = this.ExcludedProperties.Count > 0 || this.ExcludeSinglePropertyColumns;

        if (!this.useUtcTimestamp && !excludesProperties)
        {
            return logEvent;
        }

        return new LogEvent(
            this.useUtcTimestamp ? logEvent.Timestamp.ToUniversalTime() : logEvent.Timestamp,
            logEvent.Level,
            logEvent.Exception,
            logEvent.MessageTemplate,
            logEvent.Properties.Where(p => !this.IsExcluded(p.Key)).Select(p => new LogEventProperty(p.Key, p.Value)),
            logEvent.TraceId ?? default,
            logEvent.SpanId ?? default);
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

    /// <summary>
    ///     Converts the log event to json.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>The log event as json string.</returns>
    private static object LogEventToJson(LogEvent logEvent, IFormatProvider? formatProvider)
    {
        var jsonFormatter = new JsonFormatter(formatProvider: formatProvider);

        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        jsonFormatter.Format(logEvent, writer);

        return sb.ToString();
    }
}
