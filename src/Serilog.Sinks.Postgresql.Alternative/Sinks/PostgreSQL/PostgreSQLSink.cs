// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSQLSink.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is the main class and contains all options for the PostgreSQL sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL;

/// <inheritdoc cref="IBatchedLogEventSink" />
/// <summary>
///     This class is the main class and contains all options for the PostgreSQL sink.
/// </summary>
/// <seealso cref="IBatchedLogEventSink" />
public sealed class PostgreSqlSink : IBatchedLogEventSink
{
    /// <summary>
    /// The sink helper.
    /// </summary>
    private readonly SinkHelper sinkHelper;

    /// <inheritdoc cref="IBatchedLogEventSink" />
    /// <summary>
    ///     Initializes a new instance of the <see cref="PostgreSqlSink" /> class.
    /// </summary>
    /// <param name="options">The sink options.</param>
    public PostgreSqlSink(PostgreSqlOptions options)
    {
        this.sinkHelper = new SinkHelper(options);
    }

    /// <inheritdoc cref="IBatchedLogEventSink" />
    /// <summary>
    /// Emit a batch of log events, running asynchronously.
    /// </summary>
    /// <param name="events">The events to emit.</param>
    /// <returns></returns>
    /// <exception cref="LoggingFailedException">Received failed result {result.StatusCode} when posting events to Microsoft Teams</exception>
    /// <remarks>
    /// Override either <see cref="M:Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink.EmitBatch(System.Collections.Generic.IReadOnlyCollection{Serilog.Events.LogEvent})" /> or <see cref="M:Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink.EmitBatchAsync(System.Collections.Generic.IReadOnlyCollection{Serilog.Events.LogEvent})" />,
    /// not both. Overriding EmitBatch() is preferred.
    /// </remarks>
    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> events)
    {
        try
        {
            await this.sinkHelper.Emit(events);
        }
        catch (Exception ex)
        {
            // Todo: Remove this in next version!
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            this.sinkHelper.SinkOptions.FailureCallback?.Invoke(ex);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
            throw;
        }
    }

    /// <inheritdoc cref="IBatchedLogEventSink" />
    /// <summary>
    /// Allows sinks to perform periodic work without requiring additional threads or
    /// timers (thus avoiding additional flush/shut-down complexity).   
    /// </summary>
    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}
