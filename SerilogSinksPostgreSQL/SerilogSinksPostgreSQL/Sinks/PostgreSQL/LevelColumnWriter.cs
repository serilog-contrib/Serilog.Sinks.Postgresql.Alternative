namespace Serilog.Sinks.PostgreSQL
{
    using System;

    using NpgsqlTypes;

    using Serilog.Events;

    /// <summary>
    ///     Writes log level
    /// </summary>
    public class LevelColumnWriter : ColumnWriterBase
    {
        private readonly bool _renderAsText;

        public LevelColumnWriter(bool renderAsText = false, NpgsqlDbType dbType = NpgsqlDbType.Integer)
            : base(dbType)
        {
            this._renderAsText = renderAsText;
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (this._renderAsText)
                return logEvent.Level.ToString();

            return (int)logEvent.Level;
        }
    }
}