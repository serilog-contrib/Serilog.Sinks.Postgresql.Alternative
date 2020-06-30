namespace SerilogSinksPostgreSQL.IntegrationTests
{
    /// <summary>
    /// Common properties for Tests
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
