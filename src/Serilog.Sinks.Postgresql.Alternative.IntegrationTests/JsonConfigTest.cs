// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonConfigTest.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Tests for creating PostgreSql logger from a JSON configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
/// Tests for creating PostgreSql logger from a JSON configuration.
/// </summary>
[TestClass]
public class JsonConfigTest : BaseTests
{
    /// <summary>
    /// The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test the logger creation from the configuration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task ShouldCreateLoggerFromConfig()
    {
        const string TableName = "ConfigLogs1";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(".\\PostgreSinkConfiguration.json", false, true)
            .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        const long RowsCount = 2;

        for (var i = 0; i < RowsCount; i++)
        {
            logger.Information(
                "{@LogEvent} {TestProperty}",
                testObject,
                "TestValue");
        }

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test the logger creation from the configuration with the level column as text.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task ShouldCreateLoggerFromConfigWithLevelAsText()
    {
        const string TableName = "ConfigLogs2";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(".\\PostgreSinkConfiguration.Level.json", false, true)
            .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        const long RowsCount = 2;

        for (var i = 0; i < RowsCount; i++)
        {
            logger.Information(
                "{@LogEvent} {TestProperty}",
                testObject,
                "TestValue");
        }

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test the logger creation from the configuration with ordered columns.
    ///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/57.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task ShouldCreateLoggerFromConfigWithOrderedColumns()
    {
        const string TableName = "ConfigLogs3";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(".\\PostgreSinkConfigurationOrderedColumns.json", false, true)
            .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        const long RowsCount = 2;

        for (var i = 0; i < RowsCount; i++)
        {
            logger.Information(
                "{@LogEvent} {TestProperty}",
                testObject,
                "TestValue");
        }

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }
}
