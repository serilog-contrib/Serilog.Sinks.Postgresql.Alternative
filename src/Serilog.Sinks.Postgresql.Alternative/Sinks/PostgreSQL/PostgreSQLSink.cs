// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSQLSink.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is the main class and contains all options for the PostgreSQL sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    using System;
    using System.Collections.Generic;

    using Events;
    using PeriodicBatching;

    using Serilog.Debugging;

    /// <inheritdoc cref="PeriodicBatchingSink" />
    /// <summary>
    ///     This class is the main class and contains all options for the PostgreSQL sink.
    /// </summary>
    /// <seealso cref="PeriodicBatchingSink" />
    public class PostgreSqlSink : PeriodicBatchingSink
    {
        /// <summary>
        /// The sink helper.
        /// </summary>
        private readonly SinkHelper sinkHelper;

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
        /// </summary>
        /// <param name="options">The sink options.</param>
        public PostgreSqlSink(PostgreSqlOptions options) : base(options.BatchSizeLimit, options.Period, options.QueueLimit)
        {
            this.sinkHelper = new SinkHelper(options);
        }

        /// <inheritdoc cref="PeriodicBatchingSink" />
        /// <summary>
        ///     Emit a batch of log events, running to completion synchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <remarks>
        ///     Override either
        ///     <see
        ///         cref="M:Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink.EmitBatch(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent})" />
        ///     or
        ///     <see
        ///         cref="M:Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink.EmitBatchAsync(System.Collections.Generic.IEnumerable{Serilog.Events.LogEvent})" />
        ///     ,
        ///     not both.
        /// </remarks>
        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            try
            {
                this.sinkHelper.Emit(events);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"{ex.Message} {ex.StackTrace}");
                this.sinkHelper.SinkOptions.FailureCallback?.Invoke(ex);
            }
        }
    }
}