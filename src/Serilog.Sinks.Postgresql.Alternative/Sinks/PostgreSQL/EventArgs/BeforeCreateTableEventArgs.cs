// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BeforeCreateTableEventArgs.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The before create table event args.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.EventArgs;

/// <inheritdoc cref="SystemEventArgs" />
/// <summary>
/// The before create table event args.
/// </summary>
/// <seealso cref="SystemEventArgs" />
public class BeforeCreateTableEventArgs : SystemEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeCreateTableEventArgs"/> class.
    /// </summary>
    public BeforeCreateTableEventArgs()
    {
    }
}