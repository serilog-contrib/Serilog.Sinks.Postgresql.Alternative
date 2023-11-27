// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncEvent.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The asynchronous event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.Async;

/// <summary>
/// The asynchronous event.
/// </summary>
/// <typeparam name="TEventArgs">The event args type.</typeparam>
public sealed class AsyncEvent<TEventArgs> where TEventArgs : SystemEventArgs
{
    /// <summary>
    /// The handlers.
    /// </summary>
    private readonly List<AsyncEventInvocator<TEventArgs>> handlers = new();

    /// <summary>
    /// The handlers to invoke.
    /// </summary>
    private ICollection<AsyncEventInvocator<TEventArgs>> handlersForInvoke;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncEvent{TEventArgs}"/> class.
    /// </summary>
    public AsyncEvent()
    {
        this.handlersForInvoke = this.handlers;
    }

    /// <summary>
    /// Checks whether the event has handlers. Track the existence of handlers in a separate field so that checking it
    /// all the time will not require locking the actual list (handlers).
    /// </summary>
    public bool HasHandlers { get; private set; }

    /// <summary>
    /// Adds a handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <exception cref="ArgumentNullException">Thrown if the handler is null.</exception>
    public void AddHandler(Func<TEventArgs, Task>? handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        lock (this.handlers)
        {
            this.handlers.Add(new AsyncEventInvocator<TEventArgs>(null, handler));
            this.HasHandlers = true;
            this.handlersForInvoke = new List<AsyncEventInvocator<TEventArgs>>(this.handlers);
        }
    }

    /// <summary>
    /// Adds a handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <exception cref="ArgumentNullException">Thrown if the handler is null.</exception>
    public void AddHandler(Action<TEventArgs>? handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        lock (this.handlers)
        {
            this.handlers.Add(new AsyncEventInvocator<TEventArgs>(handler, null));
            this.HasHandlers = true;
            this.handlersForInvoke = new List<AsyncEventInvocator<TEventArgs>>(this.handlers);
        }
    }

    /// <summary>
    /// Removes a handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <exception cref="ArgumentNullException">Thrown if the handler is null.</exception>
    public void RemoveHandler(Func<TEventArgs, Task>? handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        lock (this.handlers)
        {
            this.handlers.RemoveAll(h => h.WrapsHandler(handler));
            this.HasHandlers = this.handlers.Count > 0;
            this.handlersForInvoke = new List<AsyncEventInvocator<TEventArgs>>(this.handlers);
        }
    }

    /// <summary>
    /// Removes a handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <exception cref="ArgumentNullException">Thrown if the handler is null.</exception>
    public void RemoveHandler(Action<TEventArgs> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        lock (this.handlers)
        {
            this.handlers.RemoveAll(h => h.WrapsHandler(handler));
            this.HasHandlers = this.handlers.Count > 0;
            this.handlersForInvoke = new List<AsyncEventInvocator<TEventArgs>>(this.handlers);
        }
    }

    /// <summary>
    /// Invokes the event.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    public async Task Invoke(TEventArgs eventArgs)
    {
        if (!this.HasHandlers)
        {
            return;
        }

        // Adding or removing handlers will produce a new list instance all the time.
        // So locking here is not required since only the reference to an immutable list
        // of handlers is used.
        var handlers = this.handlersForInvoke;
        foreach (var handler in handlers)
        {
            await handler.Invoke(eventArgs).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Tries to invoke the event.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown if the event args or the logger is null.</exception>
    public async Task TryInvokeAsync(TEventArgs eventArgs, ILogger logger)
    {
        if (eventArgs is null)
        {
            throw new ArgumentNullException(nameof(eventArgs));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        try
        {
            await this.Invoke(eventArgs).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.Warning(exception, "Error while invoking event with arguments of type {Type}", typeof(TEventArgs));
        }
    }
}
