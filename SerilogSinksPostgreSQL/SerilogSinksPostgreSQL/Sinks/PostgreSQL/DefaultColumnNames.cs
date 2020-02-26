// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultColumnNames.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license
// </copyright>
// <summary>
//   This class contains the default column names.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///     This class contains the default column names.
    /// </summary>
    public static class DefaultColumnNames
    {
        /// <summary>
        ///     The exception.
        /// </summary>
        public const string Exception = "Exception";

        /// <summary>
        ///     The level.
        /// </summary>
        public const string Level = "Level";

        /// <summary>
        ///     The log event serialized.
        /// </summary>
        public const string LogEventSerialized = "LogEvent";

        /// <summary>
        ///     The message template.
        /// </summary>
        public const string MessageTemplate = "MessageTemplate";

        /// <summary>
        ///     The rendered message.
        /// </summary>
        public const string RenderedMessage = "Message";

        /// <summary>
        ///     The timestamp.
        /// </summary>
        public const string Timestamp = "Timestamp";
    }
}