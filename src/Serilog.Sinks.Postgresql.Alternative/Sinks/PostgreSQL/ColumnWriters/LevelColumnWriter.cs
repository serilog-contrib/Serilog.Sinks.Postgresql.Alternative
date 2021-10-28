// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LevelColumnWriter.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to write the level.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.ColumnWriters
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using NpgsqlTypes;

    using Serilog.Events;

    /// <inheritdoc cref="ColumnWriterBase" />
    /// <summary>
    ///     This class is used to write the level.
    /// </summary>
    /// <seealso cref="ColumnWriterBase" />
    public class LevelColumnWriter : ColumnWriterBase
    {
        /// <summary>
        ///     A boolean value indicating whether the level is rendered as text.
        /// </summary>
        private readonly bool renderAsText;

        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="LevelColumnWriter" /> class.
        /// </summary>
        public LevelColumnWriter() : base(NpgsqlDbType.Integer, order: 0)
        {
            this.renderAsText = false;
        }

        /// <inheritdoc cref="ColumnWriterBase" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="LevelColumnWriter" /> class.
        /// </summary>
        /// <param name="renderAsText">if set to <c>true</c> [render as text].</param>
        /// <param name="dbType">The row type.</param>
        /// <param name="order">
        /// The order of the column writer if needed.
        /// Is used for sorting the columns as the writers are ordered alphabetically per default.
        /// </param>
        /// <seealso cref="ColumnWriterBase"/>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public LevelColumnWriter(bool renderAsText = false, NpgsqlDbType dbType = NpgsqlDbType.Integer, int? order = null)
            : base(dbType, order: order)
        {
            this.renderAsText = renderAsText;
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
        /// <seealso cref="ColumnWriterBase"/>
        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            if (this.renderAsText)
            {
                return logEvent.Level.ToString();
            }

            return (int)logEvent.Level;
        }
    }
}