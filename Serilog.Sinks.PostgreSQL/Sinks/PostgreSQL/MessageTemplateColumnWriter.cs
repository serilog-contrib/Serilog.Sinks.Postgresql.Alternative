using System;
using NpgsqlTypes;
using Serilog.Events;

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///     Writes non rendered message
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
}