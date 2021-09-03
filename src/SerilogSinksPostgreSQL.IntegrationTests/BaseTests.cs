// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used as a base class for the integration tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.IntegrationTests
{
    /// <summary>
    /// This class is used as a base class for the integration tests.
    /// </summary>
    public abstract class BaseTests
    {
        /// <summary>
        ///     The connection string.
        /// </summary>
        protected const string ConnectionString =
            "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Serilog";
    }
}
