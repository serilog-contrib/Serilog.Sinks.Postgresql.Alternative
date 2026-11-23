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
        // PostgreSQL checks the create privilege on the database before it evaluates IF NOT EXISTS, so a user that may
        // only write into an existing schema would fail here although there is nothing to create.
        if (await SchemaExists(connection, schemaName).ConfigureAwait(false))
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = GetCreateTableQuery(schemaName);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     Checks whether the schema exists.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns><c>true</c> if the schema exists, <c>false</c> otherwise.</returns>
    private static async Task<bool> SchemaExists(NpgsqlConnection connection, string schemaName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = @schemaName);";
        command.Parameters.AddWithValue("schemaName", NpgsqlDbType.Text, schemaName);
        return await command.ExecuteScalarAsync().ConfigureAwait(false) is true;
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
