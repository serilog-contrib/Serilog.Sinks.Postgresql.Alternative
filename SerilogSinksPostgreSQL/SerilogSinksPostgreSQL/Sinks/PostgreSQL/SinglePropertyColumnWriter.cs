using System;
using System.IO;
using System.Text;
using NpgsqlTypes;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///     Write single event property
    /// </summary>
    public class SinglePropertyColumnWriter : ColumnWriterBase
    {
        public SinglePropertyColumnWriter(string propertyName,
            PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString,
            NpgsqlDbType dbType = NpgsqlDbType.Text, string format = null) : base(dbType)
        {
            Name = propertyName;
            WriteMethod = writeMethod;
            Format = format;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Name { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public PropertyWriteMethod WriteMethod { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Format { get; }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (!logEvent.Properties.ContainsKey(Name))
                return DBNull.Value;

            switch (WriteMethod)
            {
                case PropertyWriteMethod.Raw:
                    return GetPropertyValue(logEvent.Properties[Name]);
                case PropertyWriteMethod.Json:
                    var valuesFormatter = new JsonValueFormatter();

                    var sb = new StringBuilder();

                    using (var writer = new StringWriter(sb))
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
                return scalarValue.Value;

            return logEventProperty;
        }
    }
}