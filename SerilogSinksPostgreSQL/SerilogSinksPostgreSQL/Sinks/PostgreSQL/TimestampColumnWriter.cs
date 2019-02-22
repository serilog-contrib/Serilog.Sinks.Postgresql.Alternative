namespace Serilog.Sinks.PostgreSQL
{
    using System;

    using NpgsqlTypes;

    using Serilog.Events;

    /// <summary>
    ///     Writes timestamp part
    /// </summary>
    public class TimestampColumnWriter : ColumnWriterBase
    {
        public TimestampColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Timestamp)
            : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (this.DbType == NpgsqlDbType.Timestamp)
                return logEvent.Timestamp.DateTime;

            return logEvent.Timestamp;
        }
    }
}