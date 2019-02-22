using System;
using System.IO;
using System.Text;
using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///     Writes all event properties as json
    /// </summary>
    public class PropertiesColumnWriter : ColumnWriterBase
    {
        public PropertiesColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Jsonb) : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return PropertiesToJson(logEvent);
        }

        private object PropertiesToJson(LogEvent logEvent)
        {
            if (logEvent.Properties.Count == 0)
                return "{}";

            var valuesFormatter = new JsonValueFormatter();

            var sb = new StringBuilder();

            sb.Append("{");

            using (var writer = new StringWriter(sb))
            {
                foreach (var logEventProperty in logEvent.Properties)
                {
                    sb.Append($"\"{logEventProperty.Key}\":");

                    valuesFormatter.Format(logEventProperty.Value, writer);

                    sb.Append(", ");
                }
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");

            return sb.ToString();
        }
    }
}