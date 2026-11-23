// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesColumnWriterTest.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the <seealso cref="PropertiesColumnWriter" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.Tests.ColumnWritersTests;

/// <summary>
///     This class is used to test the <seealso cref="PropertiesColumnWriter" /> class.
/// </summary>
[TestClass]
public sealed class PropertiesColumnWriterTest
{
    /// <summary>
    ///     This method is used to test the writer with empty properties.
    /// </summary>
    [TestMethod]
    public void NoPropertiesShouldReturnEmptyJsonObject()
    {
        var writer = new PropertiesColumnWriter();

        var testEvent = new LogEvent(
            DateTime.Now,
            LogEventLevel.Debug,
            null,
            new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());

        var result = writer.GetValue(testEvent);

        Assert.AreEqual("{}", result);
    }

    /// <summary>
    ///     This method is used to test that the properties are written as JSON object.
    /// </summary>
    [TestMethod]
    public void PropertiesShouldBeWrittenAsJsonObject()
    {
        var writer = new PropertiesColumnWriter();

        var result = writer.GetValue(CreateLogEvent());

        Assert.AreEqual("{\"Application\":\"WeatherApp\", \"Secret\":\"1234\", \"Other\":42}", result);
    }

    /// <summary>
    ///     This method is used to test that the excluded properties are left out.
    ///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/8.
    /// </summary>
    [TestMethod]
    public void ExcludedPropertiesShouldBeLeftOut()
    {
        var writer = new PropertiesColumnWriter { ExcludedProperties = ["Secret"] };

        var result = writer.GetValue(CreateLogEvent());

        Assert.AreEqual("{\"Application\":\"WeatherApp\", \"Other\":42}", result);
    }

    /// <summary>
    ///     This method is used to test that an empty JSON object is written if all properties are excluded.
    /// </summary>
    [TestMethod]
    public void AllPropertiesExcludedShouldReturnEmptyJsonObject()
    {
        var writer = new PropertiesColumnWriter { ExcludedProperties = ["Application", "Secret", "Other"] };

        var result = writer.GetValue(CreateLogEvent());

        Assert.AreEqual("{}", result);
    }

    /// <summary>
    ///     This method is used to test that the properties with their own column in the same sink are left out, but
    ///     only if the option is set.
    ///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/8.
    /// </summary>
    [TestMethod]
    public void ExcludeSinglePropertyColumnsShouldLeaveOutPropertiesWithOwnColumn()
    {
        var excludingWriter = new PropertiesColumnWriter { ExcludeSinglePropertyColumns = true };
        var keepingWriter = new PropertiesColumnWriter();

        _ = new SinkHelper(new PostgreSqlOptions
        {
            ColumnOptions = new Dictionary<string, ColumnWriterBase>
            {
                { "Properties", excludingWriter },
                { "AllProperties", keepingWriter },
                { "Application", new SinglePropertyColumnWriter("Application") }
            }
        });

        Assert.AreEqual("{\"Secret\":\"1234\", \"Other\":42}", excludingWriter.GetValue(CreateLogEvent()));
        Assert.AreEqual("{\"Application\":\"WeatherApp\", \"Secret\":\"1234\", \"Other\":42}", keepingWriter.GetValue(CreateLogEvent()));
    }

    /// <summary>
    ///     This method is used to test that a property name with a quotation mark still gives valid JSON.
    /// </summary>
    [TestMethod]
    public void PropertyNameWithQuotationMarkShouldBeEscaped()
    {
        var writer = new PropertiesColumnWriter();

        var testEvent = new LogEvent(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Test"),
            [new LogEventProperty("Say \"Hi\"", new ScalarValue(1))]);

        var result = (string)writer.GetValue(testEvent);

        using var document = JsonDocument.Parse(result);
        Assert.AreEqual(1, document.RootElement.GetProperty("Say \"Hi\"").GetInt32());
    }

    /// <summary>
    ///     Creates a log event with three properties.
    /// </summary>
    /// <returns>The log event.</returns>
    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Test"),
            [
                new LogEventProperty("Application", new ScalarValue("WeatherApp")),
                new LogEventProperty("Secret", new ScalarValue("1234")),
                new LogEventProperty("Other", new ScalarValue(42))
            ]);
    }
}
