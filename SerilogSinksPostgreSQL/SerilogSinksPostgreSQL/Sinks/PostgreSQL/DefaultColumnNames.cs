namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///  This class contains the default column names.
    /// </summary>
    public static class DefaultColumnNames
    {
        /// <summary>
        /// The exception.
        /// </summary>
        public const string Exception = "exception";

        /// <summary>
        /// The level.
        /// </summary>
        public const string Level = "level";

        /// <summary>
        /// The log event serialized.
        /// </summary>
        public const string LogEventSerialized = "log_event";

        /// <summary>
        /// The message template.
        /// </summary>
        public const string MessageTemplate = "message_template";

        /// <summary>
        /// The rendered message.
        /// </summary>
        public const string RenderedMessage = "message";

        /// <summary>
        /// The timestamp.
        /// </summary>
        public const string Timestamp = "timestamp";
    }
}