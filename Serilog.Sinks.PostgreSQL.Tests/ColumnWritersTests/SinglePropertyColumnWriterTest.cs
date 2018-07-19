using System;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Serilog.Sinks.PostgreSQL.Tests
{
    public class SinglePropertyColumnWriterTest
    {
        [Fact]
        public void WithToStringSeleted_ShouldRespectFormatPassed()
        {
            string propertyName = "TestProperty";

            string propertyValue = "TestValue";

            var property = new LogEventProperty(propertyName, new ScalarValue(propertyValue));

            var writer = new SinglePropertyColumnWriter(propertyName, PropertyWriteMethod.ToString, format: "l");

            var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, null, new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), new []{ property });

            var result = writer.GetValue(testEvent);

            Assert.Equal(propertyValue, result);
        }

        [Fact]
        public void PropertyIsNotPeresent_ShouldReturnDbNullValue()
        {
            string propertyName = "TestProperty";

            string propertyValue = "TestValue";

            var property = new LogEventProperty(propertyName, new ScalarValue(propertyValue));

            var writer = new SinglePropertyColumnWriter(propertyName, PropertyWriteMethod.ToString, format: "l");

            var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, null, new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(DBNull.Value, result);
        }

        [Fact]
        public void RawSelectedForScalarProperty_ShouldReturnPropertyValue()
        {
            string propertyName = "TestProperty";

            int propertyValue = 42;

            var property = new LogEventProperty(propertyName, new ScalarValue(propertyValue));

            var writer = new SinglePropertyColumnWriter(propertyName, PropertyWriteMethod.Raw);

            var testEvent = new LogEvent(DateTime.Now, LogEventLevel.Debug, null, new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()), new[] { property });

            var result = writer.GetValue(testEvent);

            Assert.Equal(propertyValue, result);
        }
    }
}