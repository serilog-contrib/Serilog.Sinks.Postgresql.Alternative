// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimestampColumnWriterTest.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the <seealso cref="TimestampColumnWriter" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.Tests.ColumnWritersTests;

/// <summary>
///     This class is used to test the <seealso cref="TimestampColumnWriter" /> class.
/// </summary>
[TestClass]
public sealed class TimestampColumnWriterTest
{
    /// <summary>
    ///     This method is used to test the writer with a timestamp without time zone.
    /// </summary>
    [TestMethod]
    public void ByDefaultShouldReturnTimestampValueWithTimezone()
    {
        var writer = new TimestampColumnWriter();

        var timeStamp = new DateTimeOffset(2017, 8, 13, 11, 11, 11, new TimeSpan());

        var testEvent = new LogEvent(
            timeStamp,
            LogEventLevel.Debug,
            null,
            new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());

        var result = writer.GetValue(testEvent);

        Assert.AreEqual(timeStamp, result);
    }

    /// <summary>
    ///     This method is used to test the writer with valid timestamp.
    /// </summary>
    [TestMethod]
    public void DbTypeWithTimezoneSelectedShouldReturnTimestampValue()
    {
        var writer = new TimestampColumnWriter();

        var timeStamp = new DateTimeOffset(2017, 8, 13, 11, 11, 11, new TimeSpan());

        var testEvent = new LogEvent(
            timeStamp,
            LogEventLevel.Debug,
            null,
            new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());

        var result = writer.GetValue(testEvent);

        Assert.AreEqual(timeStamp, result);
    }
}
