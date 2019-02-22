using System;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.PostgreSQL;
using Xunit;

namespace SerilogSinksPostgreSQL.Tests.ColumnWritersTests
{
    public class ExceptionColumnWriterTest
    {
        [Fact]
        public void ExceptionIsNull_ShouldReturnDbNullValue()
        {
            var writer = new ExceptionColumnWriter();

            var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(DBNull.Value, result);
        }

        [Fact]
        public void ExceptionIsPresent_ShouldReturnStringrepresentation()
        {
            var writer = new ExceptionColumnWriter();

            var exception = new Exception("Test exception");

            var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, exception,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(exception.ToString(), result);
        }
    }
}