// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableCreator.cs" company="Haemmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to create the tables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    using Npgsql;

    using NpgsqlTypes;

    /// <summary>
    ///     This class is used to create the tables.
    /// </summary>
    public static class TableCreator
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
        ///     Creates the table.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnsInfo">The columns information.</param>
        public static void CreateTable(
            NpgsqlConnection connection,
            string tableName,
            IDictionary<string, ColumnWriterBase> columnsInfo)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = GetCreateTableQuery(tableName, columnsInfo);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Gets the create table query.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnsInfo">The columns information.</param>
        /// <returns>The create table query string.</returns>
        private static string GetCreateTableQuery(string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
        {
            var builder = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
            builder.Append(tableName);
            builder.AppendLine(" (");

            builder.AppendLine(
                string.Join(",\n", columnsInfo.Select(r => $" \"{r.Key}\" {GetSqlTypeStr(r.Value.DbType)} ")));

            builder.AppendLine(")");

            return builder.ToString();
        }

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
        private static string GetSqlTypeStr(NpgsqlDbType dbType)
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