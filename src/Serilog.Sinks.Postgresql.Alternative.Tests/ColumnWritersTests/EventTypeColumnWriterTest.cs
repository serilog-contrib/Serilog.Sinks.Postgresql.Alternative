// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventTypeColumnWriterTest.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the <seealso cref="EventTypeColumnWriter" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.Tests.ColumnWritersTests;

/// <summary>
///     This class is used to test the <seealso cref="EventTypeColumnWriter" /> class.
/// </summary>
[TestClass]
public sealed class EventTypeColumnWriterTest
{
    /// <summary>
    ///     This method is used to test that the event type matches the event id <c>@i</c> that
    ///     Serilog.Formatting.Compact writes for the same message template.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    [TestMethod]
    [DataRow("Hello, {Name}!")]
    [DataRow("Test{TestNo}: {@TestObject} test2: {@TestObject2} testStr: {@TestStr:l}")]
    [DataRow("")]
    [DataRow("Gr\u00fc\u00dfe \uD83D\uDE00 {Value}")]
    public void EventTypeShouldMatchCompactJsonEventId(string messageTemplate)
    {
        var writer = new EventTypeColumnWriter();
        var testEvent = CreateLogEvent(messageTemplate);

        var result = writer.GetValue(testEvent);

        Assert.AreEqual(GetCompactJsonEventId(testEvent), result);
    }

    /// <summary>
    ///     This method is used to test that a <c>Bigint</c> column gets the same hash as number.
    /// </summary>
    [TestMethod]
    public void BigintColumnShouldReturnHashAsNumber()
    {
        var writer = new EventTypeColumnWriter(NpgsqlDbType.Bigint);
        var testEvent = CreateLogEvent("Hello, {Name}!");

        var result = writer.GetValue(testEvent);

        Assert.AreEqual(Convert.ToInt64(GetCompactJsonEventId(testEvent), 16), result);
    }

    /// <summary>
    ///     This method is used to test that events with the same message template get the same event type and events
    ///     with another message template get another one.
    /// </summary>
    [TestMethod]
    public void SameMessageTemplateShouldReturnSameEventType()
    {
        var writer = new EventTypeColumnWriter();

        var first = writer.GetValue(CreateLogEvent("User {Name} logged in", new LogEventProperty("Name", new ScalarValue("A"))));
        var second = writer.GetValue(CreateLogEvent("User {Name} logged in", new LogEventProperty("Name", new ScalarValue("B"))));
        var other = writer.GetValue(CreateLogEvent("User {Name} logged out", new LogEventProperty("Name", new ScalarValue("A"))));

        Assert.AreEqual(first, second);
        Assert.AreNotEqual(first, other);
    }

    /// <summary>
    ///     Creates a log event with the given message template.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="properties">The properties of the log event.</param>
    /// <returns>The log event.</returns>
    private static LogEvent CreateLogEvent(string messageTemplate, params LogEventProperty[] properties)
    {
        return new LogEvent(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse(messageTemplate),
            properties);
    }

    /// <summary>
    ///     Gets the event id <c>@i</c> that Serilog.Formatting.Compact writes for the log event.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <returns>The event id.</returns>
    private static string? GetCompactJsonEventId(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        new RenderedCompactJsonFormatter().Format(logEvent, writer);
        using var document = JsonDocument.Parse(writer.ToString());
        return document.RootElement.GetProperty("@i").GetString();
    }
}
