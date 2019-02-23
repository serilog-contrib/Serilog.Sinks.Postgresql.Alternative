namespace SerilogSinksPostgreSQL.Tests.ColumnWritersTests
{
    using System;
    using System.Linq;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.PostgreSQL;

    using Xunit;

    /// <summary>
    /// This class is used to test the <seealso cref="ExceptionColumnWriter"/> class.
    /// </summary>
    public class ExceptionColumnWriterTest
    {
        /// <summary>
        /// This method is used to test the writer with empty exceptions.
        /// </summary>
        [Fact]
        public void ExceptionIsNullShouldReturnDbNullValue()
        {
            var writer = new ExceptionColumnWriter();

            var testEvent = new LogEvent(
                DateTime.Now,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(DBNull.Value, result);
        }

        /// <summary>
        /// This method is used to test the writer with valid exceptions.
        /// </summary>
        [Fact]
        public void ExceptionIsPresentShouldReturnStringRepresentation()
        {
            var writer = new ExceptionColumnWriter();

            var exception = new Exception("Test exception");

            var testEvent = new LogEvent(
                DateTime.Now,
                LogEventLevel.Debug,
                exception,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(exception.ToString(), result);
        }
    }
}