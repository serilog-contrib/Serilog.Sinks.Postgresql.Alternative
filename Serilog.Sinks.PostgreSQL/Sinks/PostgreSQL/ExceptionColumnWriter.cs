using System;
using NpgsqlTypes;
using Serilog.Events;

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///     Writes exception (just it ToString())
    /// </summary>
    public class ExceptionColumnWriter : ColumnWriterBase
    {
        public ExceptionColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text) : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.Exception == null ? (object) DBNull.Value : logEvent.Exception.ToString();
        }
    }
}