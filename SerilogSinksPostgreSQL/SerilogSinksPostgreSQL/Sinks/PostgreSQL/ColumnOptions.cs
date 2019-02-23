namespace Serilog.Sinks.PostgreSQL
{
    using System.Collections.Generic;

    /// <summary>
    /// This class contains the column options.
    /// </summary>
    public static class ColumnOptions
    {
        /// <summary>
        /// Gets the default column options.
        /// </summary>
        /// <value>
        /// The default column options.
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
}