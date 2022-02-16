// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlTypeHelper.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The helper for getting types for PostgreSQL queries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL;

/// <summary>
/// The helper for getting types for PostgreSQL queries.
/// </summary>
public static class SqlTypeHelper
{
    /// <summary>
    ///     The default bit columns length.
    /// </summary>
    public const int DefaultBitColumnsLength = 8;

    /// <summary>
    ///     The default character columns length.
    /// </summary>
    public const int DefaultCharColumnsLength = 50;

    /// <summary>
    ///     The default varchar columns length.
    /// </summary>
    public const int DefaultVarcharColumnsLength = 50;

    /// <summary>
    ///     Gets the SQL type string.
    /// </summary>
    /// <param name="dbType">The column type.</param>
    /// <returns>The SQL type string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">dbType - Cannot automatically create column of type " + dbType</exception>
    public static string GetSqlTypeString(NpgsqlDbType dbType)
    {
        return dbType switch
        {
            NpgsqlDbType.Bigint => "bigint",
            NpgsqlDbType.Double => "double precision",
            NpgsqlDbType.Integer => "integer",
            NpgsqlDbType.Numeric => "numeric",
            NpgsqlDbType.Real => "real",
            NpgsqlDbType.Smallint => "smallint",
            NpgsqlDbType.Boolean => "boolean",
            NpgsqlDbType.Money => "money",
            NpgsqlDbType.Char => $"character({DefaultCharColumnsLength})",
            NpgsqlDbType.Text => "text",
            NpgsqlDbType.Varchar => $"character varying({DefaultVarcharColumnsLength})",
            NpgsqlDbType.Bytea => "bytea",
            NpgsqlDbType.Date => "date",
            NpgsqlDbType.Time => "time",
            NpgsqlDbType.Timestamp => "timestamp",
            NpgsqlDbType.TimestampTz => "timestamp with time zone",
            NpgsqlDbType.Interval => "interval",
            NpgsqlDbType.TimeTz => "time with time zone",
            NpgsqlDbType.Inet => "inet",
            NpgsqlDbType.Cidr => "cidr",
            NpgsqlDbType.MacAddr => "macaddr",
            NpgsqlDbType.Bit => $"bit({DefaultBitColumnsLength})",
            NpgsqlDbType.Varbit => $"bit varying({DefaultBitColumnsLength})",
            NpgsqlDbType.Uuid => "uuid",
            NpgsqlDbType.Xml => "xml",
            NpgsqlDbType.Json => "json",
            NpgsqlDbType.Jsonb => "jsonb",
            _ => throw new ArgumentOutOfRangeException(
                     nameof(dbType),
                     dbType,
                     $"Cannot automatically create column of type {dbType}.")
        };
    }
}
