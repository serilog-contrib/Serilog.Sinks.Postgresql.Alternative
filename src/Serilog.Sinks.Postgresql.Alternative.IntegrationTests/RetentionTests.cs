// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RetentionTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the deletion of outdated log entries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test the deletion of outdated log entries.
/// </summary>
[TestClass]
public sealed class RetentionTests : BaseTests
{
    /// <summary>
    ///     The retention time used in the tests.
    /// </summary>
    private static readonly TimeSpan RetentionTime = TimeSpan.FromDays(1);

    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test the retention with the default column names, where the timestamp column is
    ///     called "Timestamp" and therefore only found again if the delete query quotes the column name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task RetentionTimeShouldDeleteOldEntriesWithDefaultColumnNames()
    {
        const string TableName = "LogsRetention1";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var columnProps = ColumnOptions.Default;

        this.WriteSingleLogEvent(TableName, columnProps);

        var outdatedTimestamp = DateTimeOffset.UtcNow - RetentionTime - TimeSpan.FromDays(1);
        await this.databaseHelper.InsertTimestampOnlyRow(string.Empty, TableName, DefaultColumnNames.Timestamp, outdatedTimestamp);

        var outdatedRowsBefore = await this.databaseHelper.GetTableRowsCountOlderThan(
            string.Empty,
            TableName,
            DefaultColumnNames.Timestamp,
            DateTimeOffset.UtcNow - RetentionTime);
        Assert.AreEqual(1L, outdatedRowsBefore);

        this.WriteSingleLogEvent(TableName, columnProps);

        var outdatedRowsAfter = await this.databaseHelper.GetTableRowsCountOlderThan(
            string.Empty,
            TableName,
            DefaultColumnNames.Timestamp,
            DateTimeOffset.UtcNow - RetentionTime);
        Assert.AreEqual(0L, outdatedRowsAfter);
    }

    /// <summary>
    ///     This method is used to test that the retention time is also honored by the overload that takes the
    ///     logger column options of the JSON configuration together with logger property column options.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task RetentionTimeShouldDeleteOldEntriesWithPropertyColumnOptions()
    {
        const string TableName = "LogsRetention2";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        this.WriteSingleLogEventWithPropertyColumnOptions(TableName);

        var outdatedTimestamp = DateTimeOffset.UtcNow - RetentionTime - TimeSpan.FromDays(1);
        await this.databaseHelper.InsertTimestampOnlyRow(string.Empty, TableName, DefaultColumnNames.Timestamp, outdatedTimestamp);

        var outdatedRowsBefore = await this.databaseHelper.GetTableRowsCountOlderThan(
            string.Empty,
            TableName,
            DefaultColumnNames.Timestamp,
            DateTimeOffset.UtcNow - RetentionTime);
        Assert.AreEqual(1L, outdatedRowsBefore);

        this.WriteSingleLogEventWithPropertyColumnOptions(TableName);

        var outdatedRowsAfter = await this.databaseHelper.GetTableRowsCountOlderThan(
            string.Empty,
            TableName,
            DefaultColumnNames.Timestamp,
            DateTimeOffset.UtcNow - RetentionTime);
        Assert.AreEqual(0L, outdatedRowsAfter);
    }

    /// <summary>
    ///     Writes a single log event and disposes the logger, so that the batch is flushed before the method returns.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnProps">The column options.</param>
    private void WriteSingleLogEvent(string tableName, IDictionary<string, ColumnWriterBase> columnProps)
    {
        using var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            tableName,
            columnProps,
            needAutoCreateTable: true,
            useCopy: false,
            retentionTime: RetentionTime).CreateLogger();

        logger.Information("Test");
    }

    /// <summary>
    ///     Writes a single log event through the overload that takes logger property column options and disposes the
    ///     logger, so that the batch is flushed before the method returns.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    private void WriteSingleLogEventWithPropertyColumnOptions(string tableName)
    {
        var loggerColumnOptions = new Dictionary<string, DefaultColumnWriter>
        {
            { DefaultColumnNames.Timestamp, new DefaultColumnWriter { Name = "Timestamp" } }
        };

        var loggerPropertyColumnOptions = new Dictionary<string, SinglePropertyColumnWriter>
        {
            { "TestColumn", new SinglePropertyColumnWriter("TestProperty") }
        };

        using var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            tableName,
            loggerColumnOptions,
            loggerPropertyColumnOptions,
            needAutoCreateTable: true,
            useCopy: false,
            retentionTime: RetentionTime).CreateLogger();

        logger.Information("Test {TestProperty}", "TestValue");
    }
}
