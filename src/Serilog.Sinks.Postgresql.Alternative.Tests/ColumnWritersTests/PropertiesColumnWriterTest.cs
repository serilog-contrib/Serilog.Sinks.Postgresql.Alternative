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
public class PropertiesColumnWriterTest
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
}
