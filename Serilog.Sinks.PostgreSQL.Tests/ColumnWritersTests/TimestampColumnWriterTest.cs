using System;
using System.Linq;
using Xunit;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.PostgreSQL.Tests
{
    public class TimestampColumnWriterTest
    {
        private TimestampColumnWriter _writer;

        public TimestampColumnWriterTest()
        {
            _writer = new TimestampColumnWriter();
        }

        [Fact]
        public void ByDefault_ShouldReturnTimestampValue()
        {
            var timeStamp = new DateTimeOffset(2017,8,13,11,11,11, new TimeSpan());

            var testEvent = new LogEvent(timeStamp, LogEventLevel.Debug, null, new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

            var result = _writer.GetValue(testEvent);

            Assert.Equal(timeStamp, result);
        }
    }
}
