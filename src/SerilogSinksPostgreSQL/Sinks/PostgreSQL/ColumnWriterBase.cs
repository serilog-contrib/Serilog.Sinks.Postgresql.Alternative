// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnWriterBase.cs" company="Haemmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the column writer base methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using NpgsqlTypes;

    using Serilog.Events;

    /// <summary>
    ///     This class contains the column writer base methods.
    /// </summary>
    public abstract class ColumnWriterBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnWriterBase" /> class.
        /// </summary>
        /// <param name="dbType">The column type.</param>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        protected ColumnWriterBase(NpgsqlDbType dbType)
        {
            this.DbType = dbType;
        }

        /// <summary>
        ///     Gets or sets the column type.
        /// </summary>
        /// <value>
        ///     The column type.
        /// </value>
        public NpgsqlDbType DbType { get; set; }

        /// <summary>
        ///     Gets the part of the log event to write to the column.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>An object value.</returns>
        public abstract object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null);
    }
}