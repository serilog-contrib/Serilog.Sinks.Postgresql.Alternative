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
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NpgsqlTypes;

    using Serilog;
    using Serilog.Sinks.PostgreSQL;
    using Serilog.Sinks.PostgreSQL.ColumnWriters;

    using SerilogSinksPostgreSQL.IntegrationTests.Objects;

    /// <summary>
    ///     This class is used to test the writing to the database.
    /// </summary>
    [TestClass]
    public class DbWriteTests : BaseTests
    {
        /// <summary>
        ///     The database helper.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private readonly DbHelper databaseHelper = new DbHelper(ConnectionString);

        /// <summary>
        ///     This method is used to test the auto creation of the tables and adds data with the insert command.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [TestMethod]
        public async Task AutoCreateTableIsTrueShouldCreateTableInsert()
        {
            const string TableName = "Logs1";
            this.databaseHelper.RemoveTable(string.Empty, TableName);
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

            const long RowsCount = 1;

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
            await Task.Delay(1000);
            var actualRowsCount = this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
            Assert.AreEqual(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to test the auto creation of the tables and adds data with the copy command.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [TestMethod]
        public async Task AutoCreateTableIsTrueShouldCreateTableCopy()
        {
            const string TableName = "Logs2";
            this.databaseHelper.RemoveTable(string.Empty, TableName);
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

            const long RowsCount = 1;

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
            await Task.Delay(1000);
            var actualRowsCount = this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
            Assert.AreEqual(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to test the behavior if the single property column writer does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [TestMethod]
        public async Task PropertyForSinglePropertyColumnWriterDoesNotExistsWithInsertStatementsShouldInsertEventToDb()
        {
            const string TableName = "Logs3";
            this.databaseHelper.RemoveTable(string.Empty, TableName);
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

            this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSQL(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            Log.CloseAndFlush();
            await Task.Delay(1000);
            var actualRowsCount = this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
            Assert.AreEqual(1, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to check if quoted column names are inserted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [TestMethod]
        public async Task QuotedColumnNamesWithInsertStatementsShouldInsertEventToDb()
        {
            const string TableName = "Logs4";
            this.databaseHelper.RemoveTable(string.Empty, TableName);
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

            this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSQL(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            Log.CloseAndFlush();
            await Task.Delay(1000);
            var actualRowsCount = this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
            Assert.AreEqual(1, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write 50 events to the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [TestMethod]
        public async Task Write50EventsShouldInsert50EventsToDb()
        {
            const string TableName = "Logs5";
            this.databaseHelper.RemoveTable(string.Empty, TableName);
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

            this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

            var logger = new LoggerConfiguration().WriteTo.PostgreSQL(ConnectionString, TableName, columnProps).Enrich
                .WithMachineName().CreateLogger();

            const long RowsCount = 50;

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
            await Task.Delay(1000);
            var actualRowsCount = this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
            Assert.AreEqual(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write an event with zero code character in json to the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        [TestMethod]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public async Task WriteEventWithZeroCodeCharInJsonShouldInsertEventToDb()
        {
            const string TableName = "Logs6";
            this.databaseHelper.RemoveTable(string.Empty, TableName);
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

            this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

            var logger = new LoggerConfiguration().WriteTo.PostgreSQL(ConnectionString, TableName, columnProps)
                .CreateLogger();

            logger.Information("Test: {@testObject} testStr: {@testStr:l}", testObject, "stringValue");

            Log.CloseAndFlush();
            await Task.Delay(1000);
            var actualRowsCount = this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
            Assert.AreEqual(1, actualRowsCount);
        }
    }
}