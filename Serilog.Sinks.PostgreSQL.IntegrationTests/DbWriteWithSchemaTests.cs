using System;
using System.Collections.Generic;
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL.IntegrationTests.Objects;
using Xunit;

namespace Serilog.Sinks.PostgreSQL.IntegrationTests
{
    public class DbWriteWithSchemaTests
    {
        private const string _connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=serilog_logs";

        private const string _tableName = "logs_with_schema";
        private const string _schemaName = "logs";

        private readonly string _tableFullName = $"{_schemaName}.{_tableName}";

        private readonly DbHelper _dbHelper = new DbHelper(_connectionString);

        [Fact]
        public void Write50Events_ShouldInsert50EventsToDb()
        {
            _dbHelper.ClearTable(_tableFullName);


            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

            var columnProps = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter() },
                {"message_template", new MessageTemplateColumnWriter() },
                {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"raise_date", new TimestampColumnWriter() },
                {"exception", new ExceptionColumnWriter() },
                {"properties", new LogEventSerializedColumnWriter() },
                {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                {"machine_name", new SinglePropertyColumnWriter("MachineName") }
            };

            var logger =
                new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, _tableName, columnProps, schemaName: _schemaName)
                    .Enrich.WithMachineName()
                    .CreateLogger();

            for (int i = 0; i < 50; i++)
            {
                logger.Information("Test{testNo}: {@testObject} test2: {@testObj2}", i, testObject, testObj2);
            }

            logger.Dispose();

            var rowsCount = _dbHelper.GetTableRowsCount(_tableFullName);

            Assert.Equal(50, rowsCount);

        }

        [Fact]
        public void AutoCreateTableIsTrue_ShouldCreateTable()
        {
            var tableName = "logs_auto_created_w_schema";

            var fullTableName = $"{_schemaName}.{tableName}";
            _dbHelper.RemoveTable(fullTableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

            var columnProps = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter() },
                {"message_template", new MessageTemplateColumnWriter() },
                {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"raise_date", new TimestampColumnWriter() },
                {"exception", new ExceptionColumnWriter() },
                {"properties", new LogEventSerializedColumnWriter() },
                {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                {"int_prop_test", new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
                {"machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
            };

            var logger =
                new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, tableName, columnProps,schemaName: _schemaName, needAutoCreateTable: true)
                    .Enrich.WithMachineName()
                    .CreateLogger();

            int rowsCount = 50;
            for (int i = 0; i < rowsCount; i++)
            {
                logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
            }

            logger.Dispose();

            var actualRowsCount = _dbHelper.GetTableRowsCount(fullTableName);

            Assert.Equal(rowsCount, actualRowsCount);
        }
    }
}