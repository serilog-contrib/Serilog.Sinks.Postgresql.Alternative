// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableCreator.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to create the tables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL;

/// <summary>
///     This class is used to create the tables.
/// </summary>
public static class TableCreator
{
    /// <summary>
    ///     Creates the table.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnsInfo">The columns information.</param>
    public static async Task CreateTable(
        NpgsqlConnection connection,
        string schemaName,
        string tableName,
        IDictionary<string, ColumnWriterBase> columnsInfo)
    {
        // PostgreSQL checks the create privilege on the schema before it evaluates IF NOT EXISTS, so a user that may
        // only write into an existing table would fail here although there is nothing to create.
        if (await TableExists(connection, schemaName, tableName).ConfigureAwait(false))
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = GetCreateTableQuery(schemaName, tableName, columnsInfo);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Checks whether the table exists. Without a schema name, the table is looked up through the search path, like
    ///     the insert and copy commands do.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <returns><c>true</c> if the table exists, <c>false</c> otherwise.</returns>
    private static async Task<bool> TableExists(NpgsqlConnection connection, string schemaName, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT to_regclass(@tableName) IS NOT NULL;";
        command.Parameters.AddWithValue("tableName", NpgsqlDbType.Text, GetQuotedTableName(schemaName, tableName));
        return await command.ExecuteScalarAsync().ConfigureAwait(false) is true;
    }

    /// <summary>
    ///     Gets the create table query.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnsInfo">The columns information.</param>
    /// <returns>The create table query string.</returns>
    private static string GetCreateTableQuery(string schemaName, string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
    {
        var builder = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
        builder.Append(GetQuotedTableName(schemaName, tableName));
        builder.AppendLine(" (");

        builder.AppendLine(
            string.Join(",\n", columnsInfo.Select(r => $" \"{r.Key}\" {r.Value.GetSqlType()}")));

        builder.AppendLine(");");

        return builder.ToString();
    }

    /// <summary>
    ///     Gets the quoted table name, qualified with the quoted schema name if one is given.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>The quoted table name.</returns>
    private static string GetQuotedTableName(string schemaName, string tableName)
    {
        schemaName = schemaName.Replace("\"", string.Empty);
        tableName = tableName.Replace("\"", string.Empty);

        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(schemaName))
        {
            builder.Append('"');
            builder.Append(schemaName);
            builder.Append("\".");
        }

        builder.Append('"');
        builder.Append(tableName);
        builder.Append('"');
        return builder.ToString();
    }
}
