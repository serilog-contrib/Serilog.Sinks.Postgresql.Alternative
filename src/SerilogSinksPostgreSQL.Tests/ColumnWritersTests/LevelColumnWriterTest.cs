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

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.PostgreSQL.ColumnWriters;

    /// <summary>
    ///     This class is used to test the <seealso cref="LevelColumnWriter" /> class.
    /// </summary>
    [TestClass]
    public class LevelColumnWriterTest
    {
        /// <summary>
        ///     This method is used to test the writer with default values.
        /// </summary>
        [TestMethod]
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

            Assert.AreEqual(1, result);
        }

        /// <summary>
        ///     This method is used to test the writer with the write as text property.
        /// </summary>
        [TestMethod]
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

            Assert.AreEqual(nameof(LogEventLevel.Debug), result);
        }
    }
}