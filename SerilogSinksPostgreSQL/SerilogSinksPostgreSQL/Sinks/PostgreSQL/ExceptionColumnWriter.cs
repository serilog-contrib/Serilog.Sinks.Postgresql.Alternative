namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using NpgsqlTypes;

    using Serilog.Events;

    /// <inheritdoc cref="ColumnWriterBase"/>
    /// <summary>
    ///    This class is used to write the exception.
    /// </summary>
    /// <seealso cref="ColumnWriterBase"/>
    public class ExceptionColumnWriter : ColumnWriterBase
    {
        /// <inheritdoc cref="ColumnWriterBase"/>
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionColumnWriter"/> class.
        /// </summary>
        /// <param name="dbType">The column type.</param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public ExceptionColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text)
            : base(dbType)
        {
        }

        /// <inheritdoc cref="ColumnWriterBase"/>
        /// <summary>
        /// Gets the part of the log event to write to the column.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// An object value.
        /// </returns>
        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.Exception?.ToString() ?? (object)DBNull.Value;
        }
    }
}