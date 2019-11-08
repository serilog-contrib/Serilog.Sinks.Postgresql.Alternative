SerilogSinkForPostgreSQL
====================================

SerilogSinkForPostgreSQL is a library to save logging information from [Serilog](https://github.com/serilog/serilog) to [PostgreSQL](https://www.postgresql.org/).
The assembly was written and tested in .Net Framework 4.8 and .Net Standard 2.0.

[![Build status](https://ci.appveyor.com/api/projects/status/0ggd9vc0fw9gc92c?svg=true)](https://ci.appveyor.com/project/SeppPenner/serilogsinkforpostgresql)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/SeppPenner/SerilogSinkForPostgreSQL/master/License.txt)
[![Nuget](https://img.shields.io/badge/SerilogSinkForPostgreSQL-Nuget-brightgreen.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL/)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/SerilogSinkForPostgreSQL/badge.svg)](https://snyk.io/test/github/SeppPenner/SerilogSinkForPostgreSQL)
[![Gitter](https://badges.gitter.im/SerilogSinkForPostgreSQL/community.svg)](https://gitter.im/SerilogSinkForPostgreSQL/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Available for
* NetFramework 4.5
* NetFramework 4.6
* NetFramework 4.6.2
* NetFramework 4.7
* NetFramework 4.7.2
* NetFramework 4.8
* NetStandard 2.0
* NetStandard 2.1
* NetCore 2.2
* NetCore 3.0

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

## Further information:
This project is a fork of https://github.com/b00ted/serilog-sinks-postgresql but is maintained.
Do not hesitate to create [issues](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/issues) or [pull requests](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/pulls).

Change history
--------------

* **Version 1.0.4.0 (2019-11-08)** : Updated nuget packages.
* **Version 1.0.3.0 (2019-06-23)** : Added icon to the nuget package.
* **Version 1.0.2.0 (2019-05-13)** : Updated documentation, fixed some tests.
* **Version 1.0.1.0 (2019-05-08)** : Updated documentation, added documentation to the nuget package and all classes, added option to allow upper case table and column names.
Simplified building and packing scripts. Added support for NetFramework 4.6, NetFramework 4.6.2, NetFramework 4.7 and NetFramework 4.8.
* **Version 1.0.0.0 (2019-02-22)** : 1.0 release.
