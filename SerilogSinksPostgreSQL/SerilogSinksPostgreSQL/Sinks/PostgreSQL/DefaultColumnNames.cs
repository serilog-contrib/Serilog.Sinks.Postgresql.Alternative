namespace Serilog.Sinks.PostgreSQL
{
    public static class DefaultColumnNames
    {
        public const string RenderedMesssage = "message";

        public const string MessageTemplate = "message_template";

        public const string Level = "level";

        public const string Timestamp = "timestamp";

        public const string Exception = "exception";

        public const string LogEventSerialized = "log_event";
    }
}