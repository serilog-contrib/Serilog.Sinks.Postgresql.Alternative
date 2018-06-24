using System;
using System.Collections.Generic;
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL.IntegrationTests.Objects;
using Xunit;

namespace Serilog.Sinks.PostgreSQL.IntegrationTests
{
    public class DbWriteTests
    {
        private const string _connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=serilog_logs";

        private const string _tableName = "logs";


        private DbHelper _dbHelper = new DbHelper(_connectionString);

        [Fact]
        public void Write50Events_ShouldInsert50EventsToDb()
        {
            _dbHelper.ClearTable(_tableName);


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
                {"machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
            };

            var logger =
                new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, _tableName, columnProps)
                    .Enrich.WithMachineName()
                    .CreateLogger();

            int rowsCount = 50;
            for (int i = 0; i < rowsCount; i++)
            {
                logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
            }

            logger.Dispose();

            var actualRowsCount = _dbHelper.GetTableRowsCount(_tableName);

            Assert.Equal(rowsCount, actualRowsCount);

        }

        [Fact]
        public void WriteEventWithZeroCodeCharInJson_ShouldInsertEventToDb()
        {
            _dbHelper.ClearTable(_tableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test\u0000" };

            var columnProps = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter() },
                {"message_template", new MessageTemplateColumnWriter() },
                {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"raise_date", new TimestampColumnWriter() },
                {"exception", new ExceptionColumnWriter() },
                {"properties", new LogEventSerializedColumnWriter() },
                {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) }
            };

            var logger =
                new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, _tableName, columnProps)
                    .CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            logger.Dispose();

            var actualRowsCount = _dbHelper.GetTableRowsCount(_tableName);

            Assert.Equal(1, actualRowsCount);
        }

        [Fact]
        public void QuotedColumnNamesWithInsertStatements_ShouldInsertEventToDb()
        {
            _dbHelper.ClearTable(_tableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var columnProps = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter() },
                {"\"message_template\"", new MessageTemplateColumnWriter() },
                {"\"level\"", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"raise_date", new TimestampColumnWriter() },
                {"exception", new ExceptionColumnWriter() },
                {"properties", new LogEventSerializedColumnWriter() },
                {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) }
            };

            var logger =
                new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, _tableName, columnProps, useCopy: false)
                    .CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            logger.Dispose();

            var actualRowsCount = _dbHelper.GetTableRowsCount(_tableName);

            Assert.Equal(1, actualRowsCount);
        }

        [Fact]
        public void PropertyForSinglePropertyColumnWriterDoesNotExistsWithInsertStatements_ShouldInsertEventToDb()
        {
            _dbHelper.ClearTable(_tableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var columnProps = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter() },
                {"message_template", new MessageTemplateColumnWriter() },
                {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"raise_date", new TimestampColumnWriter() },
                {"exception", new ExceptionColumnWriter() },
                {"properties", new LogEventSerializedColumnWriter() },
                {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                {"machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
            };

            var logger =
                new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, _tableName, columnProps, useCopy: false)
                    .CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            logger.Dispose();

            var actualRowsCount = _dbHelper.GetTableRowsCount(_tableName);

            Assert.Equal(1, actualRowsCount);
        }

        [Fact]
        public void AutoCreateTableIsTrue_ShouldCreateTable()
        {
            var tableName = "logs_auto_created";

            _dbHelper.RemoveTable(tableName);

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
                {"machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
            };

            var logger =
                new LoggerConfiguration().WriteTo.PostgreSQL(_connectionString, tableName, columnProps, needAutoCreateTable: true)
                    .Enrich.WithMachineName()
                    .CreateLogger();

            int rowsCount = 50;
            for (int i = 0; i < rowsCount; i++)
            {
                logger.Information("Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}", i, testObject, testObj2, "stringValue");
            }

            logger.Dispose();

            var actualRowsCount = _dbHelper.GetTableRowsCount(tableName);

            Assert.Equal(rowsCount, actualRowsCount);
        }
    }
}
