// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdAutoincrementColumnWriterTest.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the <seealso cref="IdAutoIncrementColumnWriter" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.Tests.ColumnWritersTests;

/// <summary>
/// This class is used to test the <seealso cref="IdAutoIncrementColumnWriter" /> class.
/// </summary>
[TestClass]
public sealed class IdAutoIncrementColumnWriterTest
{
    /// <summary>
    ///     This method is used to test the <see cref="IdAutoIncrementColumnWriter"/> with empty values.
    /// </summary>
    [TestMethod]
    public void GetValueShouldThrowException()
    {
        var writer = new IdAutoIncrementColumnWriter();

        var testEvent = new LogEvent(
            DateTime.Now,
            LogEventLevel.Debug,
            null,
            new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());

        Assert.ThrowsException<Exception>(() => writer.GetValue(testEvent));
    }

    /// <summary>
    ///     This method is used to test the <see cref="IdAutoIncrementColumnWriter"/> with default values.
    /// </summary>
    [TestMethod]

    public void WriterShouldBeSkippedOnInsert()
    {
        var writer = new IdAutoIncrementColumnWriter();
        var result = writer.SkipOnInsert;
        Assert.IsTrue(result);
    }
}
