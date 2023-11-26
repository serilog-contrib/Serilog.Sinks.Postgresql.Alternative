// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbWriteWithSchemaTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <copyright file="DbWriteWithSchemaTests.cs" company="TerumoBCT">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the writing of data to the database with schemas.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test the writing of data to the database with schemas.
/// </summary>
[TestClass]
public sealed class DbWriteWithSchemaTests : BaseTests
{
    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test the auto create table function with a schema name.
    ///     The schema name needs to be present in the database, e.g. create it manually.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task AutoCreateTableIsTrueShouldCreateTable()
    {
        const string SchemaName = "Logs1";
        const string TableName = "LogsWithSchema1";
        await this.databaseHelper.RemoveTable(SchemaName, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };
        var testObject2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "RaiseDate", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new LogEventSerializedColumnWriter() },
            { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
            {
                "IntPropertyTest",
                new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer)
            },
            { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
        };

        var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            columnProps,
            schemaName: SchemaName,
            needAutoCreateTable: true,
            needAutoCreateSchema: true).Enrich.WithMachineName().CreateLogger();

        const long RowsCount = 50;

        for (var i = 0; i < RowsCount; i++)
        {
            logger.Information(
                "Test{TestNo}: {@TestObject} test2: {@TestObject2} testStr: {@TestStr:l}",
                i,
                testObject,
                testObject2,
                "stringValue");
        }

        Log.CloseAndFlush();
        await Task.Delay(10000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(SchemaName, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to write 50 log events to the database.
    ///     The schema name needs to be present in the database, e.g. create it manually.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task Write50EventsShouldInsert50EventsToDb()
    {
        const string SchemaName = "Logs2";
        const string TableName = "LogsWithSchema2";
        await this.databaseHelper.RemoveTable(SchemaName, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };
        var testObject2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "RaiseDate", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new LogEventSerializedColumnWriter() },
            { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
            { "MachineName", new SinglePropertyColumnWriter("MachineName") }
        };

        await this.databaseHelper.CreateTable(SchemaName, TableName, columnProps);

        var logger = new LoggerConfiguration().WriteTo
            .PostgreSQL(ConnectionString, TableName, columnProps, schemaName: SchemaName).Enrich.WithMachineName()
            .CreateLogger();

        const long RowsCount = 50;

        for (var i = 0; i < RowsCount; i++)
        {
            logger.Information("Test{TestNo}: {@TestObject} test2: {@TestObject2}", i, testObject, testObject2);
        }

        Log.CloseAndFlush();
        await Task.Delay(10000);
        var rowsCount = await this.databaseHelper.GetTableRowsCount(SchemaName, TableName);
        Assert.AreEqual(RowsCount, rowsCount);
    }

    /// <summary>
    ///     This method is used to test AuditSink insert command with a schema name.
    ///     The schema name needs to be present in the database, e.g. create it manually.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task TestWithSchemaShouldInsertLog()
    {
        const string SchemaName = "Logs3";
        const string TableName = "LogsWithSchema3";
        await this.databaseHelper.RemoveTable(SchemaName, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };
        var testObject2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "RaiseDate", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new LogEventSerializedColumnWriter() },
            { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
            {
                "IntPropertyTest",
                new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer)
            },
            { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
        };

        var logger = new LoggerConfiguration().AuditTo.PostgreSQL(
            ConnectionString,
            TableName,
            columnProps,
            schemaName: SchemaName,
            needAutoCreateTable: true).Enrich.WithMachineName().CreateLogger();

        const long RowsCount = 10;

        for (var i = 0; i < RowsCount; i++)
        {
            logger.Information(
                "Test{TestNo}: {@TestObject} test2: {@TestObject2} testStr: {@TestStr:l}",
                i,
                testObject,
                testObject2,
                "stringValue");
        }

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(SchemaName, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test AuditSink log throws exception with incorrect DB connection string (with schema name case).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    [ExpectedException(typeof(AggregateException))]
    public async Task IncorrectDatabaseConnectionStringWithSchemaLogShouldThrowException()
    {
        const string SchemaName = "Logs4";
        const string TableName = "LogsWithSchema4";

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };
        var testObject2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

        var columnProps = new Dictionary<string, ColumnWriterBase>();

        var invalidConnectionString = ConnectionString.Replace("Port=", "Port=1");
        var logger = new LoggerConfiguration().AuditTo.PostgreSQL(
            invalidConnectionString,
            TableName,
            columnProps,
            schemaName: SchemaName,
            needAutoCreateTable: true).Enrich.WithMachineName().CreateLogger();

        logger.Information(
            "Test{TestNo}: {@TestObject} test2: {@TestObject2} testStr: {@TestStr:l}",
            1,
            testObject,
            testObject2,
            "stringValue");
        Log.CloseAndFlush();
        await Task.Delay(1000);
    }
}
