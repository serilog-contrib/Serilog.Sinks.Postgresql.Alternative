// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the sinks that open their connections from a data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test the sinks that open their connections from a data source.
///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/86.
/// </summary>
[TestClass]
public sealed class DataSourceTests : BaseTests
{
    /// <summary>
    ///     The schema used in the password rotation test.
    /// </summary>
    private const string SchemaName = "LogsDataSource";

    /// <summary>
    ///     The name of the role whose password is rotated.
    /// </summary>
    private const string UserName = "SerilogRotatedPassword";

    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test that the batched sink writes through a data source.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task DataSourceShouldInsertEventsToDb()
    {
        const string TableName = "LogsDataSource1";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        using var dataSource = NpgsqlDataSource.Create(ConnectionString);

        using (var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            dataSource,
            TableName,
            GetColumnOptions(),
            needAutoCreateTable: true).CreateLogger())
        {
            logger.Information("First");
            logger.Information("Second");
        }

        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(2L, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test that a running audit sink picks up a rotated password through the password
    ///     provider of the data source.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task RotatedPasswordShouldBeUsedForNewConnections()
    {
        const string TableName = "LogsDataSource2";
        await this.databaseHelper.CreateSchema(SchemaName);
        await this.databaseHelper.RemoveTable(SchemaName, TableName);
        await this.databaseHelper.CreateTable(SchemaName, TableName, GetColumnOptions());
        await this.databaseHelper.CreateInsertOnlyUser(UserName, "FirstPassword", SchemaName, TableName);
        await this.databaseHelper.SetPassword(UserName, "FirstPassword");

        var password = "FirstPassword";

        // Without pooling, every write opens a new physical connection and therefore asks the password provider again.
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
        dataSourceBuilder.ConnectionStringBuilder.Username = UserName;
        dataSourceBuilder.ConnectionStringBuilder.Password = null;
        dataSourceBuilder.ConnectionStringBuilder.Pooling = false;
        dataSourceBuilder.UsePasswordProvider(_ => password, (_, _) => ValueTask.FromResult(password));
        using var dataSource = dataSourceBuilder.Build();

        using var logger = new LoggerConfiguration().AuditTo.PostgreSQL(
            dataSource,
            TableName,
            GetColumnOptions(),
            schemaName: SchemaName).CreateLogger();

        logger.Information("Written with the first password");

        await this.databaseHelper.SetPassword(UserName, "SecondPassword");
        password = "SecondPassword";

        logger.Information("Written with the second password");

        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(SchemaName, TableName);
        Assert.AreEqual(2L, actualRowsCount);
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
}
