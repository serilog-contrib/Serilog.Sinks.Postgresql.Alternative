// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbWriteTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <copyright file="DbWriteTests.cs" company="TerumoBCT">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test the writing to the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test the writing to the database.
/// </summary>
[TestClass]
public sealed class DbWriteTests : BaseTests
{
    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test the auto creation of the tables and adds data with the insert command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task AutoCreateTableIsTrueShouldCreateTableInsert()
    {
        const string TableName = "Logs1";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

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
            needAutoCreateTable: true,
            useCopy: false).Enrich.WithMachineName().CreateLogger();

        const long RowsCount = 1;

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
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
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
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

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

        var logger = new LoggerConfiguration().WriteTo
            .PostgreSQL(ConnectionString, TableName, columnProps, needAutoCreateTable: true).Enrich
            .WithMachineName().CreateLogger();

        const long RowsCount = 1;

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
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
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
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "RaiseDate", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new LogEventSerializedColumnWriter() },
            { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) },
            { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
        };

        await this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

        var logger = new LoggerConfiguration().WriteTo
            .PostgreSQL(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

        logger.Information("Test: {@TestObject} testStr: {@TestStr:l}", testObject, "stringValue");

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
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
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "\"MessageTemplate\"", new MessageTemplateColumnWriter() },
            { "\"Level\"", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "RaiseDate", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new LogEventSerializedColumnWriter() },
            { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) }
        };

        await this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

        var logger = new LoggerConfiguration().WriteTo
            .PostgreSQL(ConnectionString, TableName, columnProps, useCopy: false).CreateLogger();

        logger.Information("Test: {@TestObject} testStr: {@TestStr:l}", testObject, "stringValue");

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
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
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

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
            { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l") }
        };

        await this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

        var logger = new LoggerConfiguration().WriteTo.PostgreSQL(ConnectionString, TableName, columnProps).Enrich
            .WithMachineName().CreateLogger();

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
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to write an event with zero code character in json to the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task WriteEventWithZeroCodeCharInJsonShouldInsertEventToDb()
    {
        const string TableName = "Logs6";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test\\u0000" };

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "RaiseDate", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new LogEventSerializedColumnWriter() },
            { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text) }
        };

        await this.databaseHelper.CreateTable(string.Empty, TableName, columnProps);

        var logger = new LoggerConfiguration().WriteTo.PostgreSQL(ConnectionString, TableName, columnProps)
            .CreateLogger();

        logger.Information("Test: {@TestObject} testStr: {@TestStr:l}", testObject, "stringValue");

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(1, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test AuditSink insert command without schema name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task AuditSinkNoSchemaShouldInsertLog()
    {
        const string TableName = "Logs7";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

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

        var logger = new LoggerConfiguration().AuditTo
            .PostgreSQL(ConnectionString, TableName, columnProps, needAutoCreateTable: true).Enrich
            .WithMachineName().CreateLogger();

        const long RowsCount = 10;

        for (var i = 0; i < RowsCount; i++)
        {
            logger.Information(
                "Test{TestNo}: {@TestObject} test2: {@TestObject2} testStr: {@TestStr:l}",
                10,
                testObject,
                testObject2,
                "stringValue");
        }

        Log.CloseAndFlush();
        await Task.Delay(1000);
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test AuditSink log throws exception with incorrect DB connection string (without schema name case).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task IncorrectDatabaseConnectionStringNoSchemaLogShouldThrowException()
    {
        const string TableName = "Logs8";

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };
        var testObject2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

        var columnProps = new Dictionary<string, ColumnWriterBase>();

        var invalidConnectionString = ConnectionString.Replace("Database=", "Database=A");
        var logger = new LoggerConfiguration().AuditTo
            .PostgreSQL(invalidConnectionString, TableName, columnProps, needAutoCreateTable: true).Enrich
            .WithMachineName().CreateLogger();

        logger.Information(
            "Test{TestNo}: {@TestObject} test2: {@TestObject2} testStr: {@TestStr:l}",
            1,
            testObject,
            testObject2,
            "stringValue");
        Log.CloseAndFlush();
        await Assert.ThrowsExceptionAsync<AggregateException>(async () => await Task.Delay(1000));
    }

    /// <summary>
    ///     This method is used to test the auto creation of the tables and adds data with the insert command and ordered columns.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task AutoCreateTableIsTrueShouldCreateTableInsertWithOrders()
    {
        const string TableName = "Logs9";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var testObject = new TestObjectType1 { IntProp = 42, StringProp = "Test" };
        var testObject2 = new TestObjectType2 { DateProp = DateTime.Now, NestedProp = testObject };

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter(order: 8) },
            { "MessageTemplate", new MessageTemplateColumnWriter(order: 1) },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text, 2) },
            { "RaiseDate", new TimestampColumnWriter(order: 3) },
            { "Exception", new ExceptionColumnWriter(order: 4) },
            { "Properties", new LogEventSerializedColumnWriter(order: 5) },
            { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text, 6) },
            {
                "IntPropertyTest",
                new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer, order: 7)
            },
            { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l", order: 0) }
        };

        var logger = new LoggerConfiguration().WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            columnProps,
            needAutoCreateTable: true,
            useCopy: false).Enrich.WithMachineName().CreateLogger();

        const long RowsCount = 1;

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
        var actualRowsCount = await this.databaseHelper.GetTableRowsCount(string.Empty, TableName);
        Assert.AreEqual(RowsCount, actualRowsCount);
    }

    /// <summary>
    ///     This method is used to test the issue of https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/32.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task TestIssue32()
    {
        const string TableName = "Logs10";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "TimeStamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
            { "Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            { "UserName", new SinglePropertyColumnWriter("UserName", PropertyWriteMethod.ToString, NpgsqlDbType.Text) },
        };

        var logger = new LoggerConfiguration()
          .Enrich.FromLogContext()
          .WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            columnProps,
            needAutoCreateTable: true,
            needAutoCreateSchema: true,
            failureCallback: e => Console.WriteLine($"Sink error: {e.Message}")
          ).CreateLogger();

        LogContext.PushProperty("UserName", "Hans");

        logger.Information("A test error occured.");

        Log.CloseAndFlush();
        await Task.Delay(1000);
    }

    /// <summary>
    ///     This method is used to test the issue of https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/52.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task TestIssue52()
    {
        const string TableName = "Logs11";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var columnProps = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
            { "TimeStamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
            { "Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            { "EnumTest", new SinglePropertyColumnWriter("EnumTest", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) }
        };

        var logger = new LoggerConfiguration()
          .Enrich.FromLogContext()
          .WriteTo.PostgreSQL(
            ConnectionString,
            TableName,
            columnProps,
            needAutoCreateTable: true,
            needAutoCreateSchema: true,
            failureCallback: e => Console.WriteLine($"Sink error: {e.Message}")
          ).CreateLogger();

        LogContext.PushProperty("EnumTest", DummyEnum.Test1);

        logger.Information("A test error occured.");

        Log.CloseAndFlush();
        await Task.Delay(1000);
    }
}
