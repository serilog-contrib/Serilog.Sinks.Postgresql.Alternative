// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnOptions.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the column options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL;

/// <summary>
///     This class contains the column options.
/// </summary>
public static class ColumnOptions
{
    /// <summary>
    ///     Gets the default column options.
    /// </summary>
    /// <value>
    ///     The default column options.
    /// </value>
    public static IDictionary<string, ColumnWriterBase> Default =>
        new Dictionary<string, ColumnWriterBase>
        {
                { DefaultColumnNames.RenderedMessage, new RenderedMessageColumnWriter() },
                { DefaultColumnNames.MessageTemplate, new MessageTemplateColumnWriter() },
                { DefaultColumnNames.Level, new LevelColumnWriter() },
                { DefaultColumnNames.Timestamp, new TimestampColumnWriter() },
                { DefaultColumnNames.Exception, new ExceptionColumnWriter() },
                { DefaultColumnNames.LogEventSerialized, new LogEventSerializedColumnWriter() }
        };
}
