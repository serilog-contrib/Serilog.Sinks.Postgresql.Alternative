// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateSchemaEventArgs.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The create schema event args.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.EventArgs;

/// <inheritdoc cref="SystemEventArgs" />
/// <summary>
/// The create schema event args.
/// </summary>
/// <seealso cref="SystemEventArgs" />
public class CreateSchemaEventArgs : SystemEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSchemaEventArgs"/> class.
    /// </summary>
    public CreateSchemaEventArgs()
    {
    }
}