using System;
using NpgsqlTypes;
using Serilog.Events;

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>Integer column with Autoincrement and primary key</summary>
    /// <seealso cref="ColumnWriterBase" />
    public class IdAutoincrementColumnWriter: ColumnWriterBase
    {
        /// <summary>Initializes a new instance of the <see cref="IdAutoincrementColumnWriter"/> class.</summary>
        public IdAutoincrementColumnWriter() : base(NpgsqlDbType.Bigint, true)
        {
        }

        /// <inheritdoc />
        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            // this method should not be called, because of the autoincrement
            throw new Exception("Auto-increment column should not have a value to be written!");
        }

        /// <inheritdoc />
        public override string GetSqlType()
        {
            return "SERIAL PRIMARY KEY";
        }
    }
}