// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesColumnWriterTest.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   This class is used to test the PropertiesColumnWriter class.
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
    ///     This class is used to test the <seealso cref="PropertiesColumnWriter" /> class.
    /// </summary>
    public class PropertiesColumnWriterTest
    {
        /// <summary>
        ///     This method is used to test the writer with empty properties.
        /// </summary>
        [Fact]
        public void NoPropertiesShouldReturnEmptyJsonObject()
        {
            var writer = new PropertiesColumnWriter();

            var testEvent = new LogEvent(
                DateTime.Now,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal("{}", result);
        }
    }
}