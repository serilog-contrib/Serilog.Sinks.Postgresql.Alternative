// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableCreator.cs" company="Hämmer Electronics">
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
                string.Join(",\n", columnsInfo.Select(r => $" \"{r.Key}\" {r.Value.GetSqlType()} ")));

            builder.AppendLine(")");

            return builder.ToString();
        }
    }
}