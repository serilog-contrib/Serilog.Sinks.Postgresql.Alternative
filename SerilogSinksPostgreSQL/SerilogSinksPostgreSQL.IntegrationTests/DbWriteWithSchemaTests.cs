namespace SerilogSinksPostgreSQL.IntegrationTests
{
    using System;
    using System.Collections.Generic;

    using NpgsqlTypes;

    using Serilog;
    using Serilog.Sinks.PostgreSQL;

    using SerilogSinksPostgreSQL.IntegrationTests.Objects;

    using Xunit;

    public class DbWriteWithSchemaTests
    {
        private const string ConnectionString =
            "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=serilog_logs";

        private const string SchemaName = "logs";

        private const string TableName = "logs_with_schema";

        private readonly DbHelper _dbHelper = new DbHelper(ConnectionString);

        private readonly string _tableFullName = $"{SchemaName}.{TableName}";

        [Fact]
        public void AutoCreateTableIsTrue_ShouldCreateTable()
        {
            var tableName = "logs_auto_created_w_schema";

            var fullTableName = $"{SchemaName}.{tableName}";
            this._dbHelper.RemoveTable(fullTableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

            var columnProps = new Dictionary<string, ColumnWriterBase>
                                  {
                                      { "message", new RenderedMessageColumnWriter() },
                                      { "message_template", new MessageTemplateColumnWriter() },
                                      { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                                      { "raise_date", new TimestampColumnWriter() },
                                      { "exception", new ExceptionColumnWriter() },
                                      { "properties", new LogEventSerializedColumnWriter() },
                                      { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                                      {
                                          "int_prop_test",
                                          new SinglePropertyColumnWriter(
                                              "testNo",
                                              PropertyWriteMethod.Raw,
                                              NpgsqlDbType.Integer)
                                      },
                                      { "machine_name", new SinglePropertyColumnWriter("MachineName", format: "l") }
                                  };

            var logger = new LoggerConfiguration().WriteTo.PostgreSql(
                ConnectionString,
                tableName,
                columnProps,
                schemaName: SchemaName,
                needAutoCreateTable: true).Enrich.WithMachineName().CreateLogger();

            var rowsCount = 50;
            for (var i = 0; i < rowsCount; i++)
                logger.Information(
                    "Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}",
                    i,
                    testObject,
                    testObj2,
                    "stringValue");

            logger.Dispose();

            var actualRowsCount = this._dbHelper.GetTableRowsCount(fullTableName);

            Assert.Equal(rowsCount, actualRowsCount);
        }

        [Fact]
        public void Write50Events_ShouldInsert50EventsToDb()
        {
            this._dbHelper.ClearTable(this._tableFullName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp1 = DateTime.Now, NestedProp = testObject };

            var columnProps = new Dictionary<string, ColumnWriterBase>
                                  {
                                      { "message", new RenderedMessageColumnWriter() },
                                      { "message_template", new MessageTemplateColumnWriter() },
                                      { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                                      { "raise_date", new TimestampColumnWriter() },
                                      { "exception", new ExceptionColumnWriter() },
                                      { "properties", new LogEventSerializedColumnWriter() },
                                      { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                                      { "machine_name", new SinglePropertyColumnWriter("MachineName") }
                                  };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSql(ConnectionString, TableName, columnProps, schemaName: SchemaName).Enrich.WithMachineName()
                .CreateLogger();

            for (var i = 0; i < 50; i++)
                logger.Information("Test{testNo}: {@testObject} test2: {@testObj2}", i, testObject, testObj2);

            logger.Dispose();

            var rowsCount = this._dbHelper.GetTableRowsCount(this._tableFullName);

            Assert.Equal(50, rowsCount);
        }
    }
}