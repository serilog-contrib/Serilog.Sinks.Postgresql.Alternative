// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbWriteWithSchemaTests.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   This class is used to test the writing of data to the database with schemas.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using NpgsqlTypes;

    using Serilog;
    using Serilog.Sinks.PostgreSQL;

    using SerilogSinksPostgreSQL.IntegrationTests.Objects;

    using Xunit;

    /// <summary>
    ///     This class is used to test the writing of data to the database with schemas.
    /// </summary>
    public class DbWriteWithSchemaTests
    {
        /// <summary>
        ///     The connection string.
        /// </summary>
        private const string ConnectionString =
            "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=serilog_logs";

        /// <summary>
        ///     The schema name.
        /// </summary>
        private const string SchemaName = "logs";

        /// <summary>
        ///     The table name.
        /// </summary>
        private const string TableName = "logs_with_schema";

        /// <summary>
        ///     The database helper.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private readonly DbHelper dbHelper = new DbHelper(ConnectionString);

        /// <summary>
        ///     The table full name.
        /// </summary>
        private readonly string tableFullName = $"{SchemaName}.{TableName}";

        /// <summary>
        ///     This method is used to test the auto create table function.
        /// </summary>
        [Fact]
        public void AutoCreateTableIsTrueShouldCreateTable()
        {
            const string LocalTableName = "logs_auto_created_w_schema";

            var fullTableName = $"{SchemaName}.{LocalTableName}";
            this.dbHelper.RemoveTable(fullTableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

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
                LocalTableName,
                columnProps,
                schemaName: SchemaName,
                needAutoCreateTable: true).Enrich.WithMachineName().CreateLogger();

            const int RowsCount = 50;
            for (var i = 0; i < RowsCount; i++)
            {
                logger.Information(
                    "Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}",
                    i,
                    testObject,
                    testObj2,
                    "stringValue");
            }

            logger.Dispose();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(fullTableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write 50 log events to the database.
        /// </summary>
        [Fact]
        public void Write50EventsShouldInsert50EventsToDb()
        {
            this.dbHelper.ClearTable(this.tableFullName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

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
            {
                logger.Information("Test{testNo}: {@testObject} test2: {@testObj2}", i, testObject, testObj2);
            }

            logger.Dispose();

            var rowsCount = this.dbHelper.GetTableRowsCount(this.tableFullName);

            Assert.Equal(50, rowsCount);
        }
    }
}