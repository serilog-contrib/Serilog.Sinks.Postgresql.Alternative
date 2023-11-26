// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MicrosoftExtensionsConnectionStringProvider.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   A class to read named connection strings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.Configuration;

/// <inheritdoc cref="IMicrosoftExtensionsConnectionStringProvider"/>
/// <summary>
/// A class to read named connection strings.
/// </summary>
/// <seealso cref="IMicrosoftExtensionsConnectionStringProvider"/>
internal sealed class MicrosoftExtensionsConnectionStringProvider : IMicrosoftExtensionsConnectionStringProvider
{
    /// <inheritdoc cref="IMicrosoftExtensionsConnectionStringProvider"/>
    /// <summary>
    /// Reads the connection string.
    /// </summary>
    /// <param name="nameOrConnectionString">The name of a named connection string or the connection string.</param>
    /// <param name="appConfiguration">The app configuration.</param>
    /// <returns>The connection <see cref="string"/>.</returns>
    /// <seealso cref="IMicrosoftExtensionsConnectionStringProvider"/>
    public string GetConnectionString(string nameOrConnectionString, IConfiguration appConfiguration)
    {
        // If there is a `=`, we assume this is a raw connection string, not a named value.
        // If there are no `=`, attempt to pull the named value from config.
        if (nameOrConnectionString.IndexOf("=", StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return nameOrConnectionString;
        }

        var result = appConfiguration?.GetConnectionString(nameOrConnectionString);

        if (string.IsNullOrWhiteSpace(result))
        {
            SelfLog.WriteLine($"The value {nameOrConnectionString} is not found in the `ConnectionStrings` settings and does not appear to be a raw connection string.");
        }

        return result ?? string.Empty;
    }
}
