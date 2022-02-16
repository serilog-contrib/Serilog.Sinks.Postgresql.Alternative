// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchemaCreator.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to create the tables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL;

/// <summary>
///     This class is used to create the schemas.
/// </summary>
public static class SchemaCreator
{
    /// <summary>
    ///     Creates the schema.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="schemaName">The name of the schema.</param>
    public static async Task CreateSchema(NpgsqlConnection connection, string schemaName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = GetCreateTableQuery(schemaName);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///     Gets the create schema query.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>The create table query string.</returns>
    private static string GetCreateTableQuery(string schemaName)
    {
        var builder = new StringBuilder("CREATE SCHEMA IF NOT EXISTS ");
        builder.Append('"');
        builder.Append(schemaName);
        builder.Append("\";");
        return builder.ToString();
    }
}
