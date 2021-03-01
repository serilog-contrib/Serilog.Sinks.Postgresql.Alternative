// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdAutoincrementColumnWriterTest.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the <seealso cref="IdAutoIncrementColumnWriter" /> class.
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
    /// This class is used to test the <seealso cref="IdAutoIncrementColumnWriter" /> class.
    /// </summary>
    public class IdAutoIncrementColumnWriterTest
    {
        /// <summary>
        ///     This method is used to test the <see cref="IdAutoIncrementColumnWriter"/> with empty values.
        /// </summary>
        [Fact]
        public void GetValueShouldThrowException()
        {
            var writer = new IdAutoIncrementColumnWriter();

            var testEvent = new LogEvent(
                DateTime.Now,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            Assert.Throws<Exception>(() => writer.GetValue(testEvent));
        }

        /// <summary>
        ///     This method is used to test the <see cref="IdAutoIncrementColumnWriter"/> with default values.
        /// </summary>
        [Fact]

        public void WriterShouldBeSkippedOnInsert()
        {
            var writer = new IdAutoIncrementColumnWriter();
            var result = writer.SkipOnInsert;
            Assert.True(result);
        }
    }
}
