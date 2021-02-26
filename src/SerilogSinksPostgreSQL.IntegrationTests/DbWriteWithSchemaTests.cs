// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbWriteWithSchemaTests.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <copyright file="DbWriteWithSchemaTests.cs" company="TerumoBCT">
// The project is licensed under the MIT license.
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
    using Serilog.Sinks.PostgreSQL.ColumnWriters;

    using SerilogSinksPostgreSQL.IntegrationTests.Objects;

    using Xunit;

    /// <summary>
    ///     This class is used to test the writing of data to the database with schemas.
    /// </summary>
    public class DbWriteWithSchemaTests : BaseTests
    {
        /// <summary>
        ///     The schema name. This needs to be present in the database, e.g. create it manually.
        /// </summary>
        private const string SchemaName = "Logs";

        /// <summary>
        ///     The table name.
        /// </summary>
        private const string TableName = "LogsWithSchema";

        /// <summary>
        ///     The database helper.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private readonly DbHelper dbHelper = new DbHelper(ConnectionString);

        /// <summary>
        ///     This method is used to test the auto create table function with a schema name.
        ///     The schema name needs to be present in the database, e.g. create it manually.
        /// </summary>
        [Fact]
        public void AutoCreateTableIsTrueShouldCreateTable()
        {
            this.dbHelper.RemoveTable(SchemaName, TableName);

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

            var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
                ConnectionString,
                TableName,
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

            var actualRowsCount = this.dbHelper.GetTableRowsCount(SchemaName, TableName);

            Assert.Equal(RowsCount, actualRowsCount);
        }

        /// <summary>
        ///     This method is used to write 50 log events to the database.
        ///     The schema name needs to be present in the database, e.g. create it manually.
        /// </summary>
        [Fact]
        public void Write50EventsShouldInsert50EventsToDb()
        {
            this.dbHelper.ClearTable(SchemaName, TableName);

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
                                      { "MachineName", new SinglePropertyColumnWriter("MachineName") }
                                  };

            var logger = new LoggerConfiguration().WriteTo
                .PostgreSQL(ConnectionString, TableName, columnProps, schemaName: SchemaName).Enrich.WithMachineName()
                .CreateLogger();

            for (var i = 0; i < 50; i++)
            {
                logger.Information("Test{testNo}: {@testObject} test2: {@testObj2}", i, testObject, testObj2);
            }

            logger.Dispose();

            var rowsCount = this.dbHelper.GetTableRowsCount(SchemaName, TableName);

            Assert.Equal(50, rowsCount);
        }


      /// <summary>
      ///     This method is used to test AuditSink insert command with a schema name.
      ///     The schema name needs to be present in the database, e.g. create it manually.
      /// </summary>
      [Fact]
      public void PostgreSQLAuditSink_WithSchema_ShouldInsertLog()
      {
         // Arrange
         this.dbHelper.RemoveTable(SchemaName, TableName);

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

         var logger = new LoggerConfiguration().AuditTo.PostgreSQL(
             ConnectionString,
             TableName,
             columnProps,
             schemaName: SchemaName,
             needAutoCreateTable: true).Enrich.WithMachineName().CreateLogger();

         // Act
         const int RowsCount = 10;
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

         // Assert
         var actualRowsCount = this.dbHelper.GetTableRowsCount(SchemaName, TableName);
         Assert.Equal(RowsCount, actualRowsCount);
      }

      /// <summary>
      ///     This method is used to test AuditSink log throws exception with incorrect DB connection string (with schema name case).
      /// </summary>
      [Fact]
      public void PostgreSQLAuditSink_IncorrectDBConnectionStringWithSchema_LogShouldThrowException()
      {
         // Arrange
         var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

         var testObj2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

         var columnProps = new Dictionary<string, ColumnWriterBase>
         {
         
         };

         string invalidConnectionString = ConnectionString.Replace("Port=", "Port=1");
         var logger = new LoggerConfiguration().AuditTo.PostgreSQL(
             invalidConnectionString,
             TableName,
             columnProps,
             schemaName: SchemaName,
             needAutoCreateTable: true).Enrich.WithMachineName().CreateLogger();

         // Act & Assert
         var exception = Assert.Throws<AggregateException>(() =>
            logger.Information(
                   "Test{testNo}: {@testObject} test2: {@testObj2} testStr: {@testStr:l}",
                   1,
                   testObject,
                   testObj2,
                   "stringValue")
         );        

         logger.Dispose();

         // Assert
         Assert.Contains("Failed to emit a log event", exception.Message);
      }
   }
}