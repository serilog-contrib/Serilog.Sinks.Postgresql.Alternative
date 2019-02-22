namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.IO;
    using System.Text;

    using NpgsqlTypes;

    using Serilog.Events;
    using Serilog.Formatting.Json;

    /// <summary>
    ///     Write single event property
    /// </summary>
    public class SinglePropertyColumnWriter : ColumnWriterBase
    {
        public SinglePropertyColumnWriter(
            string propertyName,
            PropertyWriteMethod writeMethod = PropertyWriteMethod.ToString,
            NpgsqlDbType dbType = NpgsqlDbType.Text,
            string format = null)
            : base(dbType)
        {
            this.Name = propertyName;
            this.WriteMethod = writeMethod;
            this.Format = format;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Format { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Name { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public PropertyWriteMethod WriteMethod { get; }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (!logEvent.Properties.ContainsKey(this.Name))
                return DBNull.Value;

            switch (this.WriteMethod)
            {
                case PropertyWriteMethod.Raw:
                    return this.GetPropertyValue(logEvent.Properties[this.Name]);
                case PropertyWriteMethod.Json:
                    var valuesFormatter = new JsonValueFormatter();

                    var sb = new StringBuilder();

                    using (var writer = new StringWriter(sb))
                    {
                        valuesFormatter.Format(logEvent.Properties[this.Name], writer);
                    }

                    return sb.ToString();

                default:
                    return logEvent.Properties[this.Name].ToString(this.Format, formatProvider);
            }
        }

        private object GetPropertyValue(LogEventPropertyValue logEventProperty)
        {
            // TODO: Add support for arrays
            if (logEventProperty is ScalarValue scalarValue)
                return scalarValue.Value;

            return logEventProperty;
        }
    }
}