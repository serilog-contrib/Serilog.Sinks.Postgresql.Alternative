using System;
using System.Diagnostics.CodeAnalysis;
using NpgsqlTypes;

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    /// The helper for getting types for  PostSQL query
    /// </summary>
    public static class SqlTypeHelper
    {
        /// <summary>
        ///     The default bit columns length.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public const int DefaultBitColumnsLength = 8;

        /// <summary>
        ///     The default character columns length.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public const int DefaultCharColumnsLength = 50;

        /// <summary>
        ///     The default varchar columns length.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public const int DefaultVarcharColumnsLength = 50;

        /// <summary>
        ///     Gets the SQL type string.
        /// </summary>
        /// <param name="dbType">The column type.</param>
        /// <returns>The SQL type string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">dbType - Cannot automatically create column of type " + dbType</exception>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public static string GetSqlTypeStr(NpgsqlDbType dbType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (dbType)
            {
                case NpgsqlDbType.Bigint:
                    return "bigint";
                case NpgsqlDbType.Double:
                    return "double precision";
                case NpgsqlDbType.Integer:
                    return "integer";
                case NpgsqlDbType.Numeric:
                    return "numeric";
                case NpgsqlDbType.Real:
                    return "real";
                case NpgsqlDbType.Smallint:
                    return "smallint";
                case NpgsqlDbType.Boolean:
                    return "boolean";
                case NpgsqlDbType.Money:
                    return "money";
                case NpgsqlDbType.Char:
                    return $"character({DefaultCharColumnsLength})";
                case NpgsqlDbType.Text:
                    return "text";
                case NpgsqlDbType.Varchar:
                    return $"character varying({DefaultVarcharColumnsLength})";
                case NpgsqlDbType.Bytea:
                    return "bytea";
                case NpgsqlDbType.Date:
                    return "date";
                case NpgsqlDbType.Time:
                    return "time";
                case NpgsqlDbType.Timestamp:
                    return "timestamp";
                case NpgsqlDbType.TimestampTz:
                    return "timestamp with time zone";
                case NpgsqlDbType.Interval:
                    return "interval";
                case NpgsqlDbType.TimeTz:
                    return "time with time zone";
                case NpgsqlDbType.Inet:
                    return "inet";
                case NpgsqlDbType.Cidr:
                    return "cidr";
                case NpgsqlDbType.MacAddr:
                    return "macaddr";
                case NpgsqlDbType.Bit:
                    return $"bit({DefaultBitColumnsLength})";
                case NpgsqlDbType.Varbit:
                    return $"bit varying({DefaultBitColumnsLength})";
                case NpgsqlDbType.Uuid:
                    return "uuid";
                case NpgsqlDbType.Xml:
                    return "xml";
                case NpgsqlDbType.Json:
                    return "json";
                case NpgsqlDbType.Jsonb:
                    return "jsonb";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(dbType),
                        dbType,
                        "Cannot automatically create column of type " + dbType);
            }
        }
    }
}
