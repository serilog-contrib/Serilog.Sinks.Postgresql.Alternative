// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimestampColumnWriterTest.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   This class is used to test the TimestampColumnWriter class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.Tests.ColumnWritersTests
{
    using System;
    using System.Linq;

    using NpgsqlTypes;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.PostgreSQL;

    using Xunit;

    /// <summary>
    ///     This class is used to test the <seealso cref="TimestampColumnWriter" /> class.
    /// </summary>
    public class TimestampColumnWriterTest
    {
        /// <summary>
        ///     This method is used to test the writer with a timestamp without time zone.
        /// </summary>
        [Fact]
        public void ByDefaultShouldReturnTimestampValueWithTimezone()
        {
            var writer = new TimestampColumnWriter();

            var timeStamp = new DateTimeOffset(2017, 8, 13, 11, 11, 11, new TimeSpan());

            var testEvent = new LogEvent(
                timeStamp,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(timeStamp, result);
        }

        /// <summary>
        ///     This method is used to test the writer with valid timestamp.
        /// </summary>
        [Fact]
        public void DbTypeWithTimezoneSelectedShouldReturnTimestampValue()
        {
            var writer = new TimestampColumnWriter(NpgsqlDbType.TimestampTz);

            var timeStamp = new DateTimeOffset(2017, 8, 13, 11, 11, 11, new TimeSpan());

            var testEvent = new LogEvent(
                timeStamp,
                LogEventLevel.Debug,
                null,
                new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());

            var result = writer.GetValue(testEvent);

            Assert.Equal(timeStamp, result);
        }
    }
}