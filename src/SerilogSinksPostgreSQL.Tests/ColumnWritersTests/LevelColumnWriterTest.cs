// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LevelColumnWriterTest.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the <seealso cref="LevelColumnWriter" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.Tests.ColumnWritersTests
{
    using System;
    using System.Linq;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.PostgreSQL;
    using Xunit;

    /// <summary>
    ///     This class is used to test the <seealso cref="LevelColumnWriter" /> class.
    /// </summary>
    public class LevelColumnWriterTest
    {
        /// <summary>
        ///     This method is used to test the writer with default values.
        /// </summary>
        [Fact]
        public void ByDefaultShouldWriteLevelNo()
        {
            var writer = new LevelColumnWriter();

            var testEvent = new LogEvent(
                DateTime.Now,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(1, result);
        }

        /// <summary>
        ///     This method is used to test the writer with the write as text property.
        /// </summary>
        [Fact]
        public void WriteAsTextIsTrueShouldWriteLevelName()
        {
            var writer = new LevelColumnWriter(true);

            var testEvent = new LogEvent(
                DateTime.Now,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(nameof(LogEventLevel.Debug), result);
        }
    }
}