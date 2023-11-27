## Basic usage
```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Serilog;";

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

The project can be found on [nuget](https://www.nuget.org/packages/Serilog.Sinks.Postgresql.Alternative/).

## Hints
Since the sink uses PeriodicBatching, which queues the log events and uses a timer to dequeue and finally log the events, you need to call `Log.CloseAndFlush();` sometimes to create the table if it should be auto-created. Check out https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/50 for an example.

## Configuration options

|Parameter|Meaning|Example|Default value|
|-|-|-|-|
|connectionString|The connection string to connect to the PostgreSQL database.|`"User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Serilog;"`|None, is mandatory.|
|tableName|The table name to write the data to. Is case-sensitive!|`"logs"`|None, is mandatory.|
|period|The time to wait between checking for event batches.|`period: new TimeSpan(0, 0, 20)`|`00:00:05`|
|formatProvider|The `IFormatProvider` to use. Supplies culture-specific formatting information. Check https://docs.microsoft.com/en-us/dotnet/api/system.iformatprovider?view=netframework-4.8.|`new CultureInfo("de-DE")`|`null`|
|columnOptions|The column options to use.|See the examples under the [Full example](https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative#full-example) section below.|`null`|
|batchSizeLimit|The maximum number of events to include in a single batch.|`batchSizeLimit: 40`|`30`|
|queueLimit|Maximum number of events in the queue.|`queueLimit: 3000`|`int.MaxValue` or `2147483647`|
|useCopy|Enables the copy command to allow batch inserting instead of multiple `INSERT` commands.|`useCopy: true`|`true`|
|schemaName|The schema in which the table should be created.|`schemaName: "Logs"`|`string.Empty` which defaults to the PostgreSQL `public` schema.|
|needAutoCreateTable|Specifies whether the table should be auto-created if it does not already exist or not.|`needAutoCreateTable: true`|`false`|
|needAutoCreateSchema|Specifies whether the schema should be auto-created if it does not already exist or not.|`needAutoCreateSchema: true`|`false`|
|failureCallback|Adds an option to add a failure callback action.|`failureCallback: e => Console.WriteLine($"Sink error: {e.Message}")`|`null`|
|appConfiguration|The app configuration section. Required if the connection string is a name.|-|`null`|
|onCreateTableCallback|Adds an option to add a create table callback action. Setting this disables the table creation and allows you to add a custom behaviour.|`onCreateTableCallback: e => Console.WriteLine($"Create table called: {e.ToString()}")`|`null`|
|onCreateSchemaCallback|Adds an option to add a create schema callback action. Setting this disables the schema creation and allows you to add a custom behaviour.|`onCreateSchemaCallback: e => Console.WriteLine($"Create schema called: {e.ToString()}")`|`null`|

## Configuration via JSON file

The configuration via a JSON file allows the following `loggerColumnOptions`:

* Level: Stores the log level as `Integer`.
* LevelAsText: Stores the log level as `Text`.
* Timestamp: Stores the timestamp as `TimestampTz`.
* LogEvent: Stores the log event as `Jsonb`.
* Properties: Stores the properties as `Jsonb`.
* Message: Stores the message template as `Text`.
* RenderedMessage: Stores the rendered message as `Text`.
* Exception: Stores the exception as `Text`.
* IdAutoIncrement: Stores the identifier as `Bigint` with auto increment.

```json
{
    "Serilog": {
        "LevelSwitches": {
            "$controlSwitch": "Verbose"
        },
        "MinimumLevel": {
            "ControlledBy": "$controlSwitch"
        },
        "WriteTo": [
            {
                "Name": "PostgreSQL",
                "Args": {
                    "connectionString": "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Serilog;",
                    "tableName": "TestLogs",
                    "schemaName": null,
                    "needAutoCreateTable": true,
                    "loggerColumnOptions": {
                        "Id": {
                            "Name": "IdAutoIncrement"
                        },
                        "TimeStamp": {
                            "Name": "Timestamp"
                        },
                        "LogEvent": {
                            "Name": "LogEvent"
                        }
                    },
                    "loggerPropertyColumnOptions": {
                        "TestColumnName": {
                            "Name": "TestProperty",
                            "Format": "{0}",
                            "WriteMethod": "Raw",
                            "DbType": "Text"
                        }
                    },
                    "period": "0.00:00:30",
                    "batchSizeLimit": 50
                }
            }
        ]
    }
}
```

## Configuration via JSON file to use ordered columns

```json
{
    "Serilog": {
        "LevelSwitches": { "$controlSwitch": "Verbose" },
        "MinimumLevel": { "ControlledBy": "$controlSwitch" },
        "WriteTo": [
            {
                "Name": "PostgreSQL",
                "Args": {
                    "connectionString": "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Serilog;",
                    "tableName": "ConfigLogs3",
                    "schemaName": null,
                    "needAutoCreateTable": true,
                    "loggerColumnOptions": {
                        "Id": {
                            "Name": "IdAutoIncrement",
                            "Order": 0
                        },
                        "TimeStamp": {
                            "Name": "Timestamp",
                            "Order": 2
                        },
                        "LogEvent": {
                            "Name": "LogEvent",
                            "Order": 3
                        }
                    },
                    "loggerPropertyColumnOptions": {
                        "TestColumnName": {
                            "Format": "{0}",
                            "Name": "TestProperty",
                            "WriteMethod": "Raw",
                            "DbType": "Text",
                            "Order": 1
                        }
                    },
                    "period": "0.00:00:30",
                    "batchSizeLimit": 50
                }
            }
        ]
    }
}
```

## Configuration via JSON file to use named connection strings

```json
{
    "ConnectionStrings": {
        "DevTest": "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=Serilog;"
    },
    "Serilog": {
        "LevelSwitches": {
            "$controlSwitch": "Verbose"
        },
        "MinimumLevel": {
            "ControlledBy": "$controlSwitch"
        },
        "WriteTo": [
            {
                "Name": "PostgreSQL",
                "Args": {
                    "connectionString": "DevTest",
                    "tableName": "TestLogs",
                    "schemaName": null,
                    "needAutoCreateTable": true,
                    "loggerColumnOptions": {
                        "Id": {
                            "Name": "IdAutoIncrement"
                        },
                        "TimeStamp": {
                            "Name": "Timestamp"
                        },
                        "LogEvent": {
                            "Name": "Properties"
                        }
                    },
                    "loggerPropertyColumnOptions": {
                        "TestColumnName": {
                            "Format": "{0}",
                            "Name": "TestProperty",
                            "WriteMethod": "Raw",
                            "DbType": "Text"
                        }
                    },
                    "period": "0.00:00:30",
                    "batchSizeLimit": 50
                }
            }
        ]
    }
}
```

The option to use named connection strings can be used like this:

```csharp
 var configuration = new ConfigurationBuilder()
    .AddJsonFile(".\\MyConfiguration.json", false, true)
    .Build();

var logger = new LoggerConfiguration()
    .WriteTo.PostgreSQL(
        ConnectionString,
        TableName,
        null,
        LogEventLevel.Verbose,
        null,
        null,
        30,
        null,
        false,
        string.Empty,
        true,
        false,
        null,
        configuration)
    .CreateLogger();
```

## Example for usage via JSON file

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile(".\\YourJsonConfiguration.json", false, true)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

logger.Information(
    "{@LogEvent} {TestProperty}",
    objectToLog,
    "TestValue");
```

## Full example

```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Serilog;";

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

## Using the sink with NodaTime in .Net Core 2.2+
For the use with [NodaTime](https://nodatime.org/) in .Net Core 2.2, you need to add a new column writer class for the `DateTimeOffset` values.
Check the issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/10, too.

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

You can change column sizes by setting the values in the `SqlTypeHelper` class:
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

## Order of columns
With version 3.3.10+, column names can be ordered according to a custom oder. This can be achieved as follows.
Default is alphabetic sorting / C# default sorting (Order is set to `null` per default).
Sorting will only work if all `order` values are set to an integer value other than `null`.

```csharp
var columnProps = new Dictionary<string, ColumnWriterBase>
{
    { "Message", new RenderedMessageColumnWriter(order: 8) },
    { "MessageTemplate", new MessageTemplateColumnWriter(order: 1) },
    { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar, 2) },
    { "RaiseDate", new TimestampColumnWriter(order: 3) },
    { "Exception", new ExceptionColumnWriter(order: 4) },
    { "Properties", new LogEventSerializedColumnWriter(order: 5) },
    { "PropertyTest", new PropertiesColumnWriter(NpgsqlDbType.Text, order: 6) },
    {
        "IntPropertyTest",
        new SinglePropertyColumnWriter("testNo", PropertyWriteMethod.Raw, NpgsqlDbType.Integer, order: 7)
    },
    { "MachineName", new SinglePropertyColumnWriter("MachineName", format: "l", order: 0) }
};
```

## Notes on column writers
One difference between `Serilog.Sinks.PostgreSQL.ColumnWriters.LogEventSerializedColumnWriter` and `PropertiesColumnWriter` is that `LogEventSerializedColumnWriter` contains metadata while `PropertiesColumnWriter` only contains properties.

Example of `LogEventSerializedColumnWriter`:

```json
{
   "Level": "Information",
   "Timestamp": "2022-03-04T13:42:29.5201398+07:00",
   "Properties": {
      "Elapsed": 289.4962,
      "ActionId": "67d1f404-0ec4-40a2-8ea6-b7cdea75597a",
      "RequestId": "0HMFTL8J7NNHJ:00000001",
      "ActionName": "/Index",
      "StatusCode": 200,
      "RequestPath": "/",
      "ConnectionId": "0HMFTL8J7NNHJ",
      "RequestMethod": "GET",
      "SourceContext": "Serilog.AspNetCore.RequestLoggingMiddleware"
   },
   "Renderings": {
      "Elapsed": [
         {
            "Format": "0.0000",
            "Rendering": "289.4962"
         }
      ]
   },
   "MessageTemplate": "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms"
}
```

And `PropertiesColumnWriter` for the same event:

```json
{
   "Elapsed": 289.4962,
   "ActionId": "67d1f404-0ec4-40a2-8ea6-b7cdea75597a",
   "RequestId": "0HMFTL8J7NNHJ:00000001",
   "ActionName": "/Index",
   "StatusCode": 200,
   "RequestPath": "/",
   "ConnectionId": "0HMFTL8J7NNHJ",
   "RequestMethod": "GET",
   "SourceContext": "Serilog.AspNetCore.RequestLoggingMiddleware"
}
```

If there is an exception, `LogEventSerializedColumnWriter` will have a property called `Exception` containing the stack trace, the same stack trace inside `ExceptionColumnWriter`.
