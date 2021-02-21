// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbWriteTests.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
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
    using Serilog.Sinks.PostgreSQL.ColumnWriters;

    using SerilogSinksPostgreSQL.IntegrationTests.Objects;

    using Xunit;

    /// <summary>
    ///     This class is used to test the writing to the database.
    /// </summary>
    public class DbWriteTests : BaseTests
    {
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
        ///     This method is used to test the auto creation of the tables and adds data with the insert command.
        /// </summary>
        [Fact]
        public void AutoCreateTableIsTrueShouldCreateTableInsert()
        {
            this.dbHelper.RemoveTable(string.Empty, TableName);

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
                { "IntPropertyTest", new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
                { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
            };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSQL(ConnectionString, TableName, columnProps, needAutoCreateTable: true, useCopy: false).Enrich
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

            Log.CloseAndFlush();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(string.Empty, TableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to test the auto creation of the tables and adds data with the copy command.
        /// </summary>
        [Fact]
        public void AutoCreateTableIsTrueShouldCreateTableCopy()
        {
            this.dbHelper.RemoveTable(string.Empty, TableName);

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
                { "IntPropertyTest", new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
                { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
            };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSQL(ConnectionString, TableName, columnProps, needAutoCreateTable: true).Enrich
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

            Log.CloseAndFlush();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(string.Empty, TableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to test the behavior if the single property column writer does not exist.
        /// </summary>
        [Fact]
        public void PropertyForSinglePropertyColumnWriterDoesNotExistsWithInsertStatementsShouldInsertEventToDb()
        {
            this.dbHelper.ClearTable(string.Empty, TableName);

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
                .PostgreSQL(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            Log.CloseAndFlush();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(string.Empty, TableName);

            Assert.Equal(1, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to check if quoted column names are inserted.
        /// </summary>
        [Fact]
        public void QuotedColumnNamesWithInsertStatementsShouldInsertEventToDb()
        {
            this.dbHelper.ClearTable(string.Empty, TableName);

            var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

            var columnProps = new Dictionary<string, ColumnWriterBase>
            {
                { "Message", new RenderedMessageColumnWriter() },
                { "\"MessageTemplate\"", new MessageTemplateColumnWriter() },
                { "\"Level\"", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                { "RaiseDate", new TimestampColumnWriter() },
                { "Exception", new ExceptionColumnWriter() },
                { "Properties", new LogEventSerializedColumnWriter() },
                { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) }
            };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSQL(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            Log.CloseAndFlush();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(string.Empty, TableName);

            Assert.Equal(1, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write 50 events to the database.
        /// </summary>
        [Fact]
        public void Write50EventsShouldInsert50EventsToDb()
        {
            this.dbHelper.ClearTable(string.Empty, TableName);

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

            var logger = new LoggerConfiguration().WriteTo.PostgreSQL(ConnectionString, TableName, columnProps).Enrich
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

            Log.CloseAndFlush();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(string.Empty, TableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write an event with zero code character in json to the database.
        /// </summary>
        [Fact]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public void WriteEventWithZeroCodeCharInJsonShouldInsertEventToDb()
        {
            this.dbHelper.ClearTable(string.Empty, TableName);

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

            var logger = new LoggerConfiguration().WriteTo.PostgreSQL(ConnectionString, TableName, columnProps)
                .CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            Log.CloseAndFlush();

            var actualRowsCount = this.dbHelper.GetTableRowsCount(string.Empty, TableName);

            Assert.Equal(1, actualRowsCount);
        }
    }
}