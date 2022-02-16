// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbHelper.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used as helper class for the database connection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used as helper class for the database connection.
/// </summary>
public class DbHelper
{
    /// <summary>
    ///     The connection string.
    /// </summary>
    private readonly string connectionString;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbHelper" /> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public DbHelper(string connectionString)
    {
        this.connectionString = connectionString;
    }

    /// <summary>
    ///     Clears the table.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    public async Task ClearTable(string schemaName, string tableName)
    {
        schemaName = schemaName.Replace("\"", string.Empty);
        tableName = tableName.Replace("\"", string.Empty);

        var builder = new StringBuilder();
        builder.Append("TRUNCATE ");

        if (!string.IsNullOrWhiteSpace(schemaName))
        {
            builder.Append('"');
            builder.Append(schemaName);
            builder.Append("\".");
        }

        builder.Append("\"");
        builder.Append(tableName);
        builder.Append("\";");

        using var connection = new NpgsqlConnection(this.connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = builder.ToString();
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///     Gets the table rows count.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>The table row count.</returns>
    public async Task<long> GetTableRowsCount(string schemaName, string tableName)
    {
        schemaName = schemaName.Replace("\"", string.Empty);
        tableName = tableName.Replace("\"", string.Empty);

        var builder = new StringBuilder();
        builder.Append("SELECT count(*) FROM ");

        if (!string.IsNullOrWhiteSpace(schemaName))
        {
            builder.Append('"');
            builder.Append(schemaName);
            builder.Append("\".");
        }

        builder.Append('"');
        builder.Append(tableName);
        builder.Append("\";");

        using var connection = new NpgsqlConnection(this.connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = builder.ToString();
        var result = await command.ExecuteScalarAsync();
        return (long?)result ?? 0;
    }

    /// <summary>
    ///     Removes the table.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    public async Task RemoveTable(string schemaName, string tableName)
    {
        schemaName = schemaName.Replace("\"", string.Empty);
        tableName = tableName.Replace("\"", string.Empty);

        var builder = new StringBuilder();
        builder.Append("DROP TABLE IF EXISTS ");

        if (!string.IsNullOrWhiteSpace(schemaName))
        {
            builder.Append('"');
            builder.Append(schemaName);
            builder.Append("\".");
        }

        builder.Append('"');
        builder.Append(tableName);
        builder.Append("\";");

        using var connection = new NpgsqlConnection(this.connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = builder.ToString();
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///     Creates the table.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnsInfo">The columns information.</param>
    public async Task CreateTable(string schemaName, string tableName, IDictionary<string, ColumnWriterBase> columnsInfo)
    {
        schemaName = schemaName.Replace("\"", string.Empty);
        tableName = tableName.Replace("\"", string.Empty);
        using var connection = new NpgsqlConnection(this.connectionString);
        connection.Open();
        await TableCreator.CreateTable(connection, schemaName, tableName, ClearQuotationMarksFromColumnOptions(columnsInfo));
    }

    /// <summary>
    ///     Clears the quotation marks from the column options.
    /// </summary>
    private static IDictionary<string, ColumnWriterBase> ClearQuotationMarksFromColumnOptions(
        IDictionary<string, ColumnWriterBase> columnOptions)
    {
        var result = new Dictionary<string, ColumnWriterBase>(columnOptions);

        foreach (var keyValuePair in columnOptions)
        {
            if (!keyValuePair.Key.Contains('"'))
            {
                continue;
            }

            result.Remove(keyValuePair.Key);
            result[keyValuePair.Key.Replace("\"", string.Empty)] = keyValuePair.Value;
        }

        return result;
    }
}
