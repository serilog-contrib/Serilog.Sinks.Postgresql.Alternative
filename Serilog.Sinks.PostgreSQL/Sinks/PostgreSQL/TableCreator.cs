using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace Serilog.Sinks.PostgreSQL
{
    public class TableCreator
    {
        public static int DefaultCharColumnsLength = 50;
        public static int DefaultVarcharColumnsLength = 50;
        public static int DefaultBitColumnsLength = 8;

        public static void CreateTable(NpgsqlConnection connection, string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = GetCreateTableQuery(tableName, columnsInfo);
                command.ExecuteNonQuery();
            }
        }


        private static string GetCreateTableQuery(string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
        {
            var builder = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
            builder.Append(tableName);
            builder.AppendLine(" (");

            builder.AppendLine(String.Join(",\n", columnsInfo.Select(r => $" {r.Key} {GetSqlTypeStr(r.Value.DbType)} ")));

            builder.AppendLine(")");

            return builder.ToString();
        }

        private static string GetSqlTypeStr(NpgsqlDbType dbType)
        {
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
                case NpgsqlDbType.TimestampTZ:
                    return "timestamp with time zone";
                case NpgsqlDbType.Interval:
                    return "interval";
                case NpgsqlDbType.TimeTZ:
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
                    throw new ArgumentOutOfRangeException(nameof(dbType), dbType, "Cannot atomatically create column of type " + dbType);
            }
        }
    }
}