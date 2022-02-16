// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMicrosoftExtensionsConnectionStringProvider.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   An interface to read named connection strings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.Configuration;

/// <summary>
/// An interface to read named connection strings.
/// </summary>
internal interface IMicrosoftExtensionsConnectionStringProvider
{
    /// <summary>
    /// Reads the connection string.
    /// </summary>
    /// <param name="nameOrConnectionString">The name of a named connection string or the connection string.</param>
    /// <param name="appConfiguration">The app configuration.</param>
    /// <returns>The connection <see cref="string"/>.</returns>
    string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration);
}
