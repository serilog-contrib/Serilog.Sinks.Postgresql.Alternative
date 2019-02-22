namespace Serilog.Sinks.PostgreSQL
{
    using System;

    using NpgsqlTypes;

    using Serilog.Events;

    /// <summary>
    ///     Writes message part
    /// </summary>
    public class RenderedMessageColumnWriter : ColumnWriterBase
    {
        public RenderedMessageColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text)
            : base(dbType)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.RenderMessage(formatProvider);
        }
    }
}