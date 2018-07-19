using System;
using System.Text;
using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.PostgreSQL
{
    public abstract class ColumnWriterBase
    {
        /// <summary>
        /// Column type
        /// </summary>
        public NpgsqlDbType DbType { get; }

        protected ColumnWriterBase(NpgsqlDbType dbType)
        {
            DbType = dbType;
        }

        /// <summary>
        /// Gets part of log event to write to the column
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public abstract object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null);

    }

    /// <summary>
    /// Writes timestamp part
    /// </summary>
    public class TimestampColumnWriter : ColumnWriterBase
    {
        public TimestampColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Timestamp) : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (DbType == NpgsqlDbType.Timestamp)
            {
                return logEvent.Timestamp.DateTime;
            }

            return logEvent.Timestamp;
        }
    }

    /// <summary>
    /// Writes message part
    /// </summary>
    public class RenderedMessageColumnWriter : ColumnWriterBase
    {
        public RenderedMessageColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text) : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.RenderMessage(formatProvider);
        }
    }

    /// <summary>
    /// Writes non rendered message
    /// </summary>
    public class MessageTemplateColumnWriter : ColumnWriterBase
    {
        public MessageTemplateColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text) : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.MessageTemplate.Text;
        }
    }

    /// <summary>
    /// Writes log level
    /// </summary>
    public class LevelColumnWriter : ColumnWriterBase
    {
        private readonly bool _renderAsText;

        public LevelColumnWriter(bool renderAsText = false, NpgsqlDbType dbType = NpgsqlDbType.Integer) : base(dbType)
        {
            _renderAsText = renderAsText;
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (_renderAsText)
            {
                return logEvent.Level.ToString();
            }

            return (int)logEvent.Level;
        }
    }

    /// <summary>
    /// Writes exception (just it ToString())
    /// </summary>
    public class ExceptionColumnWriter : ColumnWriterBase
    {
        public ExceptionColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text) : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.Exception == null ? (object)DBNull.Value : logEvent.Exception.ToString();
        }
    }

    /// <summary>
    /// Writes all event properties as json
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

            using (var writer = new System.IO.StringWriter(sb))
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

    /// <summary>
    /// Writes log event as json
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
            using (var writer = new System.IO.StringWriter(sb))
                jsonFormatter.Format(logEvent, writer);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Write single event property
    /// </summary>
    public class SinglePropertyColumnWriter : ColumnWriterBase
    {
        public string Name { get; }
        public PropertyWriteMethod WriteMethod { get; }
        public string Format { get; }

        public SinglePropertyColumnWriter(string propertyName, PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString, 
                                            NpgsqlDbType dbType = NpgsqlDbType.Text, string format = null) : base(dbType)
        {
            Name = propertyName;
            WriteMethod = writeMethod;
            Format = format;
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (!logEvent.Properties.ContainsKey(Name))
            {
                return DBNull.Value;
            }

            switch (WriteMethod)
            {
                case PropertyWriteMethod.Raw:
                    return GetPropertyValue(logEvent.Properties[Name]);
                case PropertyWriteMethod.Json:
                    var valuesFormatter = new JsonValueFormatter();

                    var sb = new StringBuilder();

                    using (var writer = new System.IO.StringWriter(sb))
                    {
                        valuesFormatter.Format(logEvent.Properties[Name], writer);
                    }

                    return sb.ToString();

                default:
                    return logEvent.Properties[Name].ToString(Format, formatProvider);
            }

        }

        private object GetPropertyValue(LogEventPropertyValue logEventProperty)
        {
            //TODO: Add support for arrays
            if (logEventProperty is ScalarValue scalarValue)
            {
                return scalarValue.Value;
            }

            return logEventProperty;
        }
    }

    public enum PropertyWriteMethod
    {
        Raw,
        ToString,
        Json
    }
}