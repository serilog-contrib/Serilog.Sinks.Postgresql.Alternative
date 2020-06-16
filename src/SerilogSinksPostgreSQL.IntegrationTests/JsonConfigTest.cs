// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonConfigTest.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   Tests for creating PostgreSql logger from JSON config.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.IntegrationTests
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Extensions.Configuration;

    using Serilog;

    using SerilogSinksPostgreSQL.IntegrationTests.Objects;

    using Xunit;

    /// <summary>
    /// Tests for creating PostgreSql logger from JSON config.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class JsonConfigTest
    {
        /// <summary>
        /// The connection string.
        /// </summary>
        private const string ConnectionString = "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Serilog;";

        /// <summary>
        /// The test logs.
        /// </summary>
        private const string TableName = "TestLogs";

        /// <summary>
        /// The database helper.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        private readonly DbHelper dbHelper = new DbHelper(ConnectionString);

        /// <summary>
        ///     This method is used to test the logger creation from the configuration.
        /// </summary>
        [Fact]
        public void ShouldCreateLoggerFromConfig()
        {
            this.dbHelper.RemoveTable(TableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(".\\PostgreSinkConfiguration.json", false, true)
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            const int RowsCount = 2;
            for (var i = 0; i < RowsCount; i++)
            {
                logger.Information(
                    "{@LogEvent} {TestProperty}",
                    testObject,
                    "TestValue");
            }

            logger.Dispose();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(TableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }
    }
}
