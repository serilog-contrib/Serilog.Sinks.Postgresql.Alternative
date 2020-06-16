using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.PostgreSQL;
using Xunit;

namespace SerilogSinksPostgreSQL.Tests.ColumnWritersTests
{
    public class IdAutoincrementColumnWriterTest
    {
        [Fact]

        public void GetValueShouldThrowException()
        {
            var writer = new IdAutoincrementColumnWriter();

            var testEvent = new LogEvent(
                DateTime.Now,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = 

            Assert.Throws<Exception>(() => writer.GetValue(testEvent));
        }

        /// <summary>
        ///     This method is used to test the writer with default values.
        /// </summary>
        [Fact]

        public void WriterShouldBeSkippedOnInsert()
        {
            var writer = new IdAutoincrementColumnWriter();

            bool result = writer.SkipOnInsert;

            Assert.True(result);
        }
    }
}
