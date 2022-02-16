// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSQLAuditSink.cs" company="TerumoBCT">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is the main class and contains all options for the PostgreSQL audit sink.
//   This class is based on the existing PostgreSqlSink class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL;

/// <inheritdoc cref="ILogEventSink"/>
/// <inheritdoc cref="IDisposable"/>
/// <summary>
/// Writes log events as rows in a table of PostgreSQL database using Audit logic, meaning that each row is synchronously committed
/// and any errors that occur are propagated to the caller.
/// </summary>
public class PostgreSqlAuditSink : ILogEventSink, IDisposable
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
    public PostgreSqlAuditSink(PostgreSqlOptions options)
    {
        this.sinkHelper = new SinkHelper(options);
    }

    /// <summary>
    /// Emit the provided log event to the sink.
    /// </summary>
    /// <param name="logEvent"> a log event to emit </param>
    public async void Emit(LogEvent logEvent)
    {
        await this.sinkHelper.Emit(new List<LogEvent> { logEvent });
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the sink.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources, <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        // This class doesn't need to dispose anything. This is just here for sink interface compatibility.
    }
}
