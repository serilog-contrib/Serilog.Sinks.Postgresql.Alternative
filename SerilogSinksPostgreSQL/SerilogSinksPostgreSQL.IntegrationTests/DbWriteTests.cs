// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbWriteTests.cs" company="H�mmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   This class is used to test the writing to the database.
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
    ///     This class is used to test the writing to the database.
    /// </summary>
    public class DbWriteTests
    {
        /// <summary>
        ///     The connection string.
        /// </summary>
        private const string ConnectionString =
            "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Serilog";

        /// <summary>
        ///     The table name.
        /// </summary>
        private const string TableName = "Logs";

        /// <summary>
        ///     The database helper.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private readonly DbHelper dbHelper = new DbHelper(ConnectionString);

        /// <summary>
        ///     This method is used to test the auto creation of the tables.
        /// </summary>
        [Fact]
        public void AutoCreateTableIsTrueShouldCreateTable()
        {
            this.dbHelper.RemoveTable(TableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

            var columnProps = new Dictionary<string, ColumnWriterBase>
                                  {
                                      { "Message", new RenderedMessageColumnWriter() },
                                      { "MessageTemplate", new MessageTemplateColumnWriter() },
                                      { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                                      { "RaiseDate", new TimestampColumnWriter() },
                                      { "Exception", new ExceptionColumnWriter() },
                                      { "Properties", new LogEventSerializedColumnWriter() },
                                      { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                                      {
                                          "IntPropertyTest",
                                          new SinglePropertyColumnWriter(
                                              "testNo",
                                              PropertyWriteMethod.Raw,
                                              NpgsqlDbType.Integer)
                                      },
                                      { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
                                  };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSql(ConnectionString, TableName, columnProps, needAutoCreateTable: true, useCopy: false).Enrich
                .WithMachineName().CreateLogger();

            const int RowsCount = 1;
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

            var actualRowsCount = this.dbHelper.GetTableRowsCount(TableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to test the behavior if the single property column writer does not exist.
        /// </summary>
        [Fact]
        public void PropertyForSinglePropertyColumnWriterDoesNotExistsWithInsertStatementsShouldInsertEventToDb()
        {
            this.dbHelper.ClearTable(TableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var columnProps = new Dictionary<string, ColumnWriterBase>
                                  {
                                      { "Message", new RenderedMessageColumnWriter() },
                                      { "MessageTemplate", new MessageTemplateColumnWriter() },
                                      { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                                      { "RaiseDate", new TimestampColumnWriter() },
                                      { "Exception", new ExceptionColumnWriter() },
                                      { "Properties", new LogEventSerializedColumnWriter() },
                                      { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                                      { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
                                  };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSql(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            logger.Dispose();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(TableName);

            Assert.Equal(1, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to check if quoted column names are inserted.
        /// </summary>
        [Fact]
        public void QuotedColumnNamesWithInsertStatementsShouldInsertEventToDb()
        {
            this.dbHelper.ClearTable(TableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var columnProps = new Dictionary<string, ColumnWriterBase>
                                  {
                                      { "Message", new RenderedMessageColumnWriter() },
                                      { "\"Message_template\"", new MessageTemplateColumnWriter() },
                                      { "\"Level\"", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                                      { "RaiseDate", new TimestampColumnWriter() },
                                      { "Exception", new ExceptionColumnWriter() },
                                      { "Properties", new LogEventSerializedColumnWriter() },
                                      { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) }
                                  };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSql(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            logger.Dispose();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(TableName);

            Assert.Equal(1, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write 50 events to the database.
        /// </summary>
        [Fact]
        public void Write50EventsShouldInsert50EventsToDb()
        {
            this.dbHelper.ClearTable(TableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var testObj2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

            var columnProps = new Dictionary<string, ColumnWriterBase>
                                  {
                                      { "Message", new RenderedMessageColumnWriter() },
                                      { "MessageTemplate", new MessageTemplateColumnWriter() },
                                      { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                                      { "RaiseDate", new TimestampColumnWriter() },
                                      { "Exception", new ExceptionColumnWriter() },
                                      { "Properties", new LogEventSerializedColumnWriter() },
                                      { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
                                      { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
                                  };

            var logger = new LoggerConfiguration().WriteTo.PostgreSql(ConnectionString, TableName, columnProps).Enrich
                .WithMachineName().CreateLogger();

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

            var actualRowsCount = this.dbHelper.GetTableRowsCount(TableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write an event with zero code character in json to the database.
        /// </summary>
        [Fact]
        public void WriteEventWithZeroCodeCharInJsonShouldInsertEventToDb()
        {
            this.dbHelper.ClearTable(TableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test\\u0000" };

            var columnProps = new Dictionary<string, ColumnWriterBase>
                                  {
                                      { "Message", new RenderedMessageColumnWriter() },
                                      { "MessageTemplate", new MessageTemplateColumnWriter() },
                                      { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                                      { "RaiseDate", new TimestampColumnWriter() },
                                      { "Exception", new ExceptionColumnWriter() },
                                      { "Properties", new LogEventSerializedColumnWriter() },
                                      { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) }
                                  };

            var logger = new LoggerConfiguration().WriteTo.PostgreSql(ConnectionString, TableName, columnProps)
                .CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            logger.Dispose();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(TableName);

            Assert.Equal(1, actualRowsCount);
        }
    }
}