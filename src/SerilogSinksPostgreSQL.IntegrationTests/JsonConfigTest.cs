using Microsoft.Extensions.Configuration;
using Serilog;
using SerilogSinksPostgreSQL.IntegrationTests.Objects;
using Xunit;

namespace SerilogSinksPostgreSQL.IntegrationTests
{
    /// <summary>Tests for creating PostgreSql logger from JSON config </summary>
    public class JsonConfigTest
    {
        private const string ConnectionString =
            "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Serilog";;

        private const string TableName = "TestLogs";

        private readonly DbHelper dbHelper = new DbHelper(ConnectionString);

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
