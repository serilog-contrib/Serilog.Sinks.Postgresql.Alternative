using System;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Serilog.Sinks.PostgreSQL.Tests
{
    public class PropertiesColumnWriterTest
    {
        [Fact]
        public void NoProperties_ShouldReturnEmptyJsonObject()
        {
            var writer = new PropertiesColumnWriter();

            var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, null, new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal("{}", result);
        }
    }
}