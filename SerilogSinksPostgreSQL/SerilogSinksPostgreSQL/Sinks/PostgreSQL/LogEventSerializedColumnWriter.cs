using System;
using System.IO;
using System.Text;
using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///     Writes log event as json
    /// </summary>
    public class LogEventSerializedColumnWriter : ColumnWriterBase
    {
        public LogEventSerializedColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Jsonb) : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return LogEventToJson(logEvent, formatProvider);
        }

        private object LogEventToJson(LogEvent logEvent, IFormatProvider formatProvider)
        {
            var jsonFormatter = new JsonFormatter(formatProvider: formatProvider);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                jsonFormatter.Format(logEvent, writer);
            }
            return sb.ToString();
        }
    }
}