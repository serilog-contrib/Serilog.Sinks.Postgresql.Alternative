// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InsertOnlyUserTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the auto creation options with a user that may not create anything.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test the auto creation options with a user that may not create anything.
///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/93.
/// </summary>
[TestClass]
public sealed class InsertOnlyUserTests : BaseTests
{
    /// <summary>
    ///     The schema used in the tests, the insert only user may use it, but not create anything in it.
    /// </summary>
    private const string SchemaName = "LogsInsertOnly";

    /// <summary>
    ///     The name and the password of the insert only user.
    /// </summary>
    private const string UserName = "SerilogInsertOnly";

    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test that an existing table is written to with <c>needAutoCreateTable</c> set, although
    ///     the user may not create a table.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task AutoCreateTableIsTrueWithExistingTableShouldInsertWithoutCreatePrivilege()
    {
        const string TableName = "LogsInsertOnly1";
        await this.PrepareTable(TableName);

        WriteSingleLogEvent(TableName, false);

        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(SchemaName, TableName);
        Assert.AreEqual(1L, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test that an existing schema and table are written to with <c>needAutoCreateSchema</c>
    ///     and <c>needAutoCreateTable</c> set, although the user may not create a schema or a table.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task AutoCreateSchemaIsTrueWithExistingSchemaShouldInsertWithoutCreatePrivilege()
    {
        const string TableName = "LogsInsertOnly2";
        await this.PrepareTable(TableName);

        WriteSingleLogEvent(TableName, true);

        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(SchemaName, TableName);
        Assert.AreEqual(1L, actualRowsCount);
    }

    /// <summary>
    ///     Gets the column options used in the tests.
    /// </summary>
    /// <returns>The column options.</returns>
    private static Dictionary<string, ColumnWriterBase> GetColumnOptions()
    {
        return new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "Timestamp", new TimestampColumnWriter() }
        };
    }

    /// <summary>
    ///     Writes a single log event as the insert only user and disposes the logger, so that the batch is flushed before
    ///     the method returns.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="needAutoCreateSchema">A value indicating whether the schema should be auto created.</param>
    private static void WriteSingleLogEvent(string tableName, bool needAutoCreateSchema)
    {
        var connectionString = new NpgsqlConnectionStringBuilder(ConnectionString)
        {
            Username = UserName,
            Password = UserName
        }.ConnectionString;

        using var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            connectionString,
            tableName,
            GetColumnOptions(),
            schemaName: SchemaName,
            needAutoCreateTable: true,
            needAutoCreateSchema: needAutoCreateSchema).CreateLogger();

        logger.Information("Test");
    }

    /// <summary>
    ///     Creates the schema, the table and the insert only user as the database owner.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    private async Task PrepareTable(string tableName)
    {
        await this.databaseHelper.CreateSchema(SchemaName);
        await this.databaseHelper.RemoveTable(SchemaName, tableName);
        await this.databaseHelper.CreateTable(SchemaName, tableName, GetColumnOptions());
        await this.databaseHelper.CreateInsertOnlyUser(UserName, UserName, SchemaName, tableName);
    }
}
