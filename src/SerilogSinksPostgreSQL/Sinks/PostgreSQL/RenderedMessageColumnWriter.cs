// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RenderedMessageColumnWriter.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write the message part.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using NpgsqlTypes;

    using Serilog.Events;

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     This class is used to write the message part.
    /// </summary>
    /// <seealso cref="ColumnWriterBase" />
    public class RenderedMessageColumnWriter : ColumnWriterBase
    {
        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="RenderedMessageColumnWriter" /> class.
        /// </summary>
        /// <param name="dbType">The column type.</param>
        /// <seealso cref="ColumnWriterBase" />
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public RenderedMessageColumnWriter(NpgsqlDbType dbType = NpgsqlDbType.Text)
            : base(dbType)
        {
        }

        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Gets the part of the log event to write to the column.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        ///     An object value.
        /// </returns>
        /// <seealso cref="ColumnWriterBase" />
        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            return logEvent.RenderMessage(formatProvider);
        }
    }
}