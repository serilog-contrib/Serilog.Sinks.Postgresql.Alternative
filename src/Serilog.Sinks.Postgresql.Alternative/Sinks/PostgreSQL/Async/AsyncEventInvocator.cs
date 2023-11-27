// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncEventInvocator.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The asynchronous event invocator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.Async;

/// <summary>
/// The asynchronous event invocator.
/// </summary>
/// <typeparam name="TEventArgs">The event args type.</typeparam>
public readonly struct AsyncEventInvocator<TEventArgs>
{
    /// <summary>
    /// The handler.
    /// </summary>
    private readonly Action<TEventArgs>? handler;

    /// <summary>
    /// The asynchronous handler.
    /// </summary>
    private readonly Func<TEventArgs, Task>? asyncHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncEventInvocator{TEventArgs}"/> struct.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="asyncHandler">The asynchronous handler.</param>
    public AsyncEventInvocator(Action<TEventArgs>? handler, Func<TEventArgs, Task>? asyncHandler)
    {
        this.handler = handler;
        this.asyncHandler = asyncHandler;
    }

    /// <summary>
    /// Wraps the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>Gets a value indicating whether the handler is this handler.</returns>
    public bool WrapsHandler(Action<TEventArgs> handler)
    {
        // Do not use ReferenceEquals! It will not work with delegates.
        return handler == this.handler;
    }

    /// <summary>
    /// Wraps the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <returns>Gets a value indicating whether the handler is this handler.</returns>
    public bool WrapsHandler(Func<TEventArgs, Task> handler)
    {
        // Do not use ReferenceEquals! It will not work with delegates.
        return handler == this.asyncHandler;
    }

    /// <summary>
    /// Invokes the handler.
    /// </summary>
    /// <param name="eventArgs">The event args.</param>
    /// <exception cref="InvalidOperationException">Thrown if no handler is configured.</exception>
    public Task Invoke(TEventArgs eventArgs)
    {
        if (this.handler is not null)
        {
            this.handler.Invoke(eventArgs);
            return Task.CompletedTask;
        }

        if (this.asyncHandler is not null)
        {
            return this.asyncHandler.Invoke(eventArgs);
        }

        throw new InvalidOperationException();
    }
}