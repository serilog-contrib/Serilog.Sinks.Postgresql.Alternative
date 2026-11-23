// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogEventSerializedColumnWriterTest.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the <seealso cref="LogEventSerializedColumnWriter" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.Tests.ColumnWritersTests;

/// <summary>
///     This class is used to test the <seealso cref="LogEventSerializedColumnWriter" /> class.
///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/67.
/// </summary>
[TestClass]
public sealed class LogEventSerializedColumnWriterTest
{
    /// <summary>
    ///     The timestamp of the test events, with an offset of minus six hours.
    /// </summary>
    private static readonly DateTimeOffset Timestamp = new(2024, 3, 1, 12, 7, 27, 251, TimeSpan.FromHours(-6));

    /// <summary>
    ///     This method is used to test that the timestamp keeps its offset by default.
    /// </summary>
    [TestMethod]
    public void DefaultShouldKeepTimestampOffset()
    {
        var writer = new LogEventSerializedColumnWriter();

        var json = (string)writer.GetValue(CreateLogEvent());

        Assert.AreEqual("2024-03-01T12:07:27.2510000-06:00", GetTimestamp(json));
    }

    /// <summary>
    ///     This method is used to test that the timestamp is written in UTC and still denotes the same point in time.
    /// </summary>
    [TestMethod]
    public void UseUtcTimestampShouldWriteTimestampInUtc()
    {
        var writer = new LogEventSerializedColumnWriter(true);

        var json = (string)writer.GetValue(CreateLogEvent());

        Assert.AreEqual("2024-03-01T18:07:27.2510000+00:00", GetTimestamp(json));
        Assert.AreEqual(Timestamp, DateTimeOffset.Parse(GetTimestamp(json), CultureInfo.InvariantCulture));
    }

    /// <summary>
    ///     This method is used to test that the conversion to UTC changes nothing but the timestamp.
    /// </summary>
    [TestMethod]
    public void UseUtcTimestampShouldKeepEverythingElse()
    {
        var logEvent = CreateLogEvent();

        var local = (string)new LogEventSerializedColumnWriter().GetValue(logEvent);
        var utc = (string)new LogEventSerializedColumnWriter(true).GetValue(logEvent);

        using var localDocument = JsonDocument.Parse(local);
        using var utcDocument = JsonDocument.Parse(utc);
        var localProperties = localDocument.RootElement.EnumerateObject().Where(p => p.Name != "Timestamp").Select(p => $"{p.Name}={p.Value.GetRawText()}").ToList();
        var utcProperties = utcDocument.RootElement.EnumerateObject().Where(p => p.Name != "Timestamp").Select(p => $"{p.Name}={p.Value.GetRawText()}").ToList();

        CollectionAssert.AreEqual(localProperties, utcProperties);
        CollectionAssert.Contains(utcProperties, $"TraceId=\"{logEvent.TraceId}\"");
        CollectionAssert.Contains(utcProperties, $"SpanId=\"{logEvent.SpanId}\"");
    }

    /// <summary>
    ///     Creates a log event with a trace id, a span id, a property and an exception.
    /// </summary>
    /// <returns>The log event.</returns>
    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            Timestamp,
            LogEventLevel.Information,
            new InvalidOperationException("Test exception"),
            new MessageTemplateParser().Parse("Request {RequestId}"),
            [new LogEventProperty("RequestId", new ScalarValue("97a1fe41"))],
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom());
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
