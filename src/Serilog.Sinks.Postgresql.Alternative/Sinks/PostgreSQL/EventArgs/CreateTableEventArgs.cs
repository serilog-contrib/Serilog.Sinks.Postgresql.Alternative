// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateTableEventArgs.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The create table event args.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.EventArgs;

/// <inheritdoc cref="SystemEventArgs" />
/// <summary>
/// The create table event args.
/// </summary>
/// <seealso cref="SystemEventArgs" />
public class CreateTableEventArgs : SystemEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTableEventArgs"/> class.
    /// </summary>
    public CreateTableEventArgs()
    {
    }
}