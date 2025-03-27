// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonConfigTestNamedConnectionString.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Tests for creating PostgreSql logger from a JSON configuration with named connection strings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
/// Tests for creating PostgreSql logger from a JSON configuration with named connection strings.
/// </summary>
[TestClass]
public sealed class JsonConfigTestNamedConnectionString : BaseTests
{
    /// <summary>
    /// The test logs.
    /// </summary>
    private const string TableName = "TestLogsNamedConnectionString";

    /// <summary>
    /// The database helper.
    /// </summary>
    private readonly DbHelper dbHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test the logger creation from the configuration.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task ShouldCreateLoggerFromConfig()
    {
        await this.dbHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(".\\PostgreSinkConfigurationConnectionString.json", false, true)
            .Build();

        var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            null,
            LogEventLevel.Verbose,
            null,
            null,
            30,
            1000,
            null,
            false,
            string.Empty,
            true,
            false,
            configuration).CreateLogger();

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
        var actualRowsCount = await this.dbHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }
}
