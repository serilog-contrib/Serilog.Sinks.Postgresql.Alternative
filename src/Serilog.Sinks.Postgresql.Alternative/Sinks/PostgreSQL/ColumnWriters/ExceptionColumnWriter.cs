// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionColumnWriter.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write the exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.ColumnWriters;

/// <inheritdoc cref="ColumnWriterBase" />
/// <summary>
///     This class is used to write the exception.
/// </summary>
/// <seealso cref="ColumnWriterBase" />
public class ExceptionColumnWriter : ColumnWriterBase
{
    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExceptionColumnWriter" /> class.
    /// </summary>
    public ExceptionColumnWriter() : base(NpgsqlDbType.Text, order: 0)
    {
    }

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExceptionColumnWriter" /> class.
    /// </summary>
    /// <param name="dbType">The column type.</param>
    /// <param name="order">
    /// The order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </param>
    /// <seealso cref="ColumnWriterBase"/>
    public ExceptionColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text, int? order = null)
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
    /// <seealso cref="ColumnWriterBase"/>
    public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
    {
        return logEvent.Exception?.ToString() ?? (object)DBNull.Value;
    }
}
