## Basic usage:
```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Logs";

string tableName = "logs";

IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
    { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
};

var logger = new LoggerConfiguration()
	.WriteTo.PostgreSQL(connectionString, tableName, columnWriters)
	.CreateLogger();
```

The project can be found on [nuget](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL/).

## Configuration options:

|Parameter|Meaning|Example|Default value|
|-|-|-|-|
|connectionString|The connection string to connect to the PostgreSQL database.|`"User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Logs"`|None, is mandatory.|
|tableName|The table name to write the data to. Is case-sensitive!|`"logs"`|None, is mandatory.|
|period|The time to wait between checking for event batches.|`period: new TimeSpan(0, 0, 20)`|`00:00:05`|
|formatProvider|The `IFormatProvider` to use. Supplies culture-specific formatting information. Check https://docs.microsoft.com/en-us/dotnet/api/system.iformatprovider?view=netframework-4.8.|`new CultureInfo("de-DE")`|`null`|
|columnOptions|The column options to use.|See the examples under the [Full example](https://github.com/SeppPenner/SerilogSinkForPostgreSQL#full-example) section below.|`null`|
|batchSizeLimit|The maximum number of events to include in a single batch.|`batchSizeLimit: 40`|`30`|
|useCopy|Enables the copy command to allow batch inserting instead of multiple `INSERT` commands.|`useCopy: true`|`true`|
|schemaName|The schema in which the table should be created.|`schemaName: "Logs"`|`string.Empty` which defaults to the PostgreSQL `public` schema.|
|needAutoCreateTable|Specifies whether the table should be auto-created if it does not already exist or not.|`needAutoCreateTable: true`|`false`|
|queueLimit|Maximum number of events in the queue.|`queueLimit: 3000`|`int.MaxValue` or `2147483647`|

## Configuration via JSON options

```json
{
    "Serilog": {
        "Using": [ "Serilog.Sinks.PostgreSql" ],
        "MinimumLevel": {
            "Default": "Warning"
        },
        "WriteTo": [
            {
                "Name": "PostgreSql",
                "Args": {
                    "connectionString": "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Logs",
                    "tableName": "logs"
                }
            }
        ]
    }
}
```

## Full example

```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Logs";

string tableName = "logs";

IDictionary<string, ColumnWriterBase> columnOptions = new Dictionary<string, ColumnWriterBase>
{
    { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
    { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
};

var logger = new LoggerConfiguration()
	.WriteTo.PostgreSQL(connectionString, tableName, columnOptions, needAutoCreateTable: true, schemaName: "LoggingSchema", useCopy: true, queueLimit: 3000, batchSizeLimit: 40, period: new TimeSpan(0, 0, 10), formatProvider: null)
	.CreateLogger();
```

## Using the sink with NodaTime in .Net Core 2.2
For the use with [NodaTime](https://nodatime.org/) in .Net Core 2.2, you need to add a new column writer class for the `DateTimeOffset` values.
Check the issue https://github.com/SeppPenner/SerilogSinkForPostgreSQL/issues/10, too.

```csharp
public class OffsetDateTimeColumnWriterBase : ColumnWriterBase
{
    public OffsetDateTimeColumnWriterBase(NpgsqlDbType dbType = NpgsqlDbType.TimestampTz): base(dbType)
    {
        this.DbType = NpgsqlDbType.TimestampTz;
    }

    public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
    {
        return OffsetDateTime.FromDateTimeOffset(logEvent.Timestamp);
    }
}
```

## Adjusting column sizes

You can change column sizes by setting the values in the `TableCreator` class:
```csharp
// Sets size of all BIT and BIT VARYING columns to 20
TableCreator.DefaultBitColumnsLength = 20;

// Sets size of all CHAR columns to 30
TableCreator.DefaultCharColumnsLength = 30;

// Sets size of all VARCHAR columns to 50
TableCreator.DefaultVarcharColumnsLength = 50;
```

## Upper or lower case table or column names
Table or column names are always case-sensitive!
