// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventTypeTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the event type column writer against the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test the event type column writer against the database.
///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/77.
/// </summary>
[TestClass]
public sealed class EventTypeTests : BaseTests
{
    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test that the event type is written to a text and a bigint column, equal for the same
    ///     message template and different for another one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task EventTypeShouldBeWrittenAsTextAndNumber()
    {
        const string TableName = "LogsEventType1";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "EventType", new EventTypeColumnWriter() },
            { "EventTypeNumber", new EventTypeColumnWriter(NpgsqlDbType.Bigint) }
        };

        using (var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            columnProps,
            needAutoCreateTable: true).CreateLogger())
        {
            logger.Information("User {Name} logged in", "A");
            logger.Information("User {Name} logged in", "B");
            logger.Information("User {Name} logged out", "A");
        }

        var rows = await this.databaseHelper.GetRows(string.Empty, TableName, "Message", "EventType", "EventTypeNumber");
        Assert.AreEqual(3, rows.Count);

        var loggedIn = rows.Where(r => ((string)r[0]).EndsWith("logged in", StringComparison.Ordinal)).Select(r => (string)r[1]).Distinct().ToList();
        var loggedOut = rows.Where(r => ((string)r[0]).EndsWith("logged out", StringComparison.Ordinal)).Select(r => (string)r[1]).Distinct().ToList();
        Assert.AreEqual(1, loggedIn.Count);
        Assert.AreEqual(1, loggedOut.Count);
        Assert.AreNotEqual(loggedIn[0], loggedOut[0]);

        foreach (var row in rows)
        {
            Assert.AreEqual(8, ((string)row[1]).Length);
            Assert.AreEqual(Convert.ToInt64((string)row[1], 16), (long)row[2]);
        }
    }

    /// <summary>
    ///     This method is used to test that the name <c>EventType</c> in the logger column options of the JSON
    ///     configuration adds the event type column.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task EventTypeFromLoggerColumnOptionsShouldBeWritten()
    {
        const string TableName = "LogsEventType2";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var loggerColumnOptions = new Dictionary<string, DefaultColumnWriter>
        {
            { "Message", new DefaultColumnWriter { Name = "RenderedMessage" } },
            { "EventType", new DefaultColumnWriter { Name = "EventType" } }
        };

        using (var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            loggerColumnOptions,
            needAutoCreateTable: true).CreateLogger())
        {
            logger.Information("User {Name} logged in", "A");
        }

        var rows = await this.databaseHelper.GetRows(string.Empty, TableName, "EventType");
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual(8, ((string)rows[0][0]).Length);
    }
}
