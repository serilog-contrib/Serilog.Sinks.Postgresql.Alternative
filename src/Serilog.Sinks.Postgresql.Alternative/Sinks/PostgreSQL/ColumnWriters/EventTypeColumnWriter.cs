// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventTypeColumnWriter.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write the event type, a hash of the message template.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.ColumnWriters;

/// <inheritdoc cref="ColumnWriterBase" />
/// <summary>
///     This class is used to write the event type, a hash of the message template. Events with the same message
///     template get the same event type. The hash is the Jenkins one-at-a-time hash that Serilog.Formatting.Compact
///     writes as event id <c>@i</c>, so the values match those. A <c>Bigint</c> column gets the hash as number, every
///     other column type gets it as eight lower case hexadecimal digits.
/// </summary>
/// <seealso cref="ColumnWriterBase" />
public sealed class EventTypeColumnWriter : ColumnWriterBase
{
    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventTypeColumnWriter" /> class.
    /// </summary>
    public EventTypeColumnWriter() : base(NpgsqlDbType.Text, order: 0)
    {
    }

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventTypeColumnWriter" /> class.
    /// </summary>
    /// <param name="dbType">The column type, <c>Bigint</c> for the hash as number, otherwise a text type.</param>
    /// <param name="order">
    /// The order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </param>
    /// <seealso cref="ColumnWriterBase" />
    public EventTypeColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text, int? order = null)
        : base(dbType, order: order)
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
    public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
    {
        var hash = ComputeHash(logEvent.MessageTemplate.Text);

        if (this.DbType == NpgsqlDbType.Bigint)
        {
            return (long)hash;
        }

        return hash.ToString("x8", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Computes the Jenkins one-at-a-time hash of the message template over its UTF-16 code units, the same way as
    ///     Serilog.Formatting.Compact does.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <returns>The hash.</returns>
    private static uint ComputeHash(string messageTemplate)
    {
        unchecked
        {
            uint hash = 0;

            foreach (var character in messageTemplate)
            {
                hash += character;
                hash += hash << 10;
                hash ^= hash >> 6;
            }

            hash += hash << 3;
            hash ^= hash >> 11;
            hash += hash << 15;
            return hash;
        }
    }
}
