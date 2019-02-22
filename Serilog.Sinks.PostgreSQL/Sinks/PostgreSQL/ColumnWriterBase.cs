using System;
using NpgsqlTypes;
using Serilog.Events;

namespace Serilog.Sinks.PostgreSQL
{
    public abstract class ColumnWriterBase
    {
        protected ColumnWriterBase(NpgsqlDbType dbType)
        {
            DbType = dbType;
        }

        /// <summary>
        ///     Column type
        /// </summary>
        public NpgsqlDbType DbType { get; }

        /// <summary>
        ///     Gets part of log event to write to the column
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public abstract object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null);
    }
}