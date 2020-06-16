// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnWriterBase.cs" company="Hämmer Electronics">
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
        /// <param name="skipOnInsert">A value indicating whether the column in the insert queries is skipped or not.</param>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        protected ColumnWriterBase(NpgsqlDbType dbType, bool skipOnInsert = false)
        {
            this.DbType = dbType;
            this.SkipOnInsert = skipOnInsert;
        }

        /// <summary>
        ///     Gets or sets the column type.
        /// </summary>
        /// <value>
        ///     The column type.
        /// </value>
        public NpgsqlDbType DbType { get; set; }

        /// <summary>
        /// Gets a value indicating whether the column in the insert queries is skipped or not.
        /// </summary>
        public bool SkipOnInsert { get; }

        /// <summary>
        ///     Gets the part of the log event to write to the column.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>An object value.</returns>
        public abstract object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null);

        /// <summary>
        /// Gets the type of the SQL query.
        /// </summary>
        /// <returns>The PostgreSql type for inserting it into the CREATE TABLE query.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public virtual string GetSqlType()
        {
            return SqlTypeHelper.GetSqlTypeStr(this.DbType);
        }
    }
}