// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogEventUtcTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the serialized log event with a UTC timestamp against the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test the serialized log event with a UTC timestamp against the database.
///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/67.
/// </summary>
[TestClass]
public sealed class LogEventUtcTests : BaseTests
{
    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test that the name <c>LogEventUtc</c> in the logger column options of the JSON
    ///     configuration writes the log event with its timestamp in UTC, while <c>LogEvent</c> keeps the offset.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task LogEventUtcFromLoggerColumnOptionsShouldWriteTimestampInUtc()
    {
        const string TableName = "LogsLogEventUtc1";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var loggerColumnOptions = new Dictionary<string, DefaultColumnWriter>
        {
            { "LogEvent", new DefaultColumnWriter { Name = "LogEvent" } },
            { "LogEventUtc", new DefaultColumnWriter { Name = "LogEventUtc" } }
        };

        var timestamp = new DateTimeOffset(2024, 3, 1, 12, 7, 27, 251, TimeSpan.FromHours(-6));

        using (var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            loggerColumnOptions,
            needAutoCreateTable: true).CreateLogger())
        {
            logger.Write(new LogEvent(
                timestamp,
                LogEventLevel.Information,
                null,
                new MessageTemplateParser().Parse("Test"),
                []));
        }

        var rows = await this.databaseHelper.GetRows(string.Empty, TableName, "LogEvent", "LogEventUtc");
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual("2024-03-01T12:07:27.2510000-06:00", GetTimestamp((string)rows[0][0]));
        Assert.AreEqual("2024-03-01T18:07:27.2510000+00:00", GetTimestamp((string)rows[0][1]));
    }

    /// <summary>
    ///     Gets the timestamp from the serialized log event.
    /// </summary>
    /// <param name="json">The serialized log event.</param>
    /// <returns>The timestamp as written.</returns>
    private static string GetTimestamp(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("Timestamp").GetString() ?? string.Empty;
    }
}
