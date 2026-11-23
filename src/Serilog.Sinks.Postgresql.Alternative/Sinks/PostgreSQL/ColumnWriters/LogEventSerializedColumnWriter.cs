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
        return LogEventToJson(this.useUtcTimestamp ? WithUtcTimestamp(logEvent) : logEvent, formatProvider);
    }

    /// <summary>
    ///     Creates a copy of the log event with the timestamp converted to UTC. The JSON formatter writes the timestamp
    ///     with its offset and has no option for UTC.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <returns>The log event with the timestamp in UTC.</returns>
    private static LogEvent WithUtcTimestamp(LogEvent logEvent)
    {
        return new LogEvent(
            logEvent.Timestamp.ToUniversalTime(),
            logEvent.Level,
            logEvent.Exception,
            logEvent.MessageTemplate,
            logEvent.Properties.Select(p => new LogEventProperty(p.Key, p.Value)),
            logEvent.TraceId ?? default,
            logEvent.SpanId ?? default);
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
