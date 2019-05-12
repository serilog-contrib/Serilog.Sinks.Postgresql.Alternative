SerilogSinkForPostgreSQL
====================================

SerilogSinkForPostgreSQL is a library to save logging information from [Serilog](https://github.com/serilog/serilog) to [PostgreSQL](https://www.postgresql.org/).
The assembly was written and tested in .Net Framework 4.8 and .Net Standard 2.0.

[![Build status](https://ci.appveyor.com/api/projects/status/0ggd9vc0fw9gc92c?svg=true)](https://ci.appveyor.com/project/SeppPenner/serilogsinkforpostgresql)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/stargazers)
[![GitHub license](https://img.shields.io/badge/license-AGPL-blue.svg)](https://raw.githubusercontent.com/SeppPenner/SerilogSinkForPostgreSQL/master/License.txt)
[![Nuget](https://img.shields.io/badge/SerilogSinkForPostgreSQL-Nuget-brightgreen.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL/)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/SerilogSinkForPostgreSQL/badge.svg)](https://snyk.io/test/github/SeppPenner/SerilogSinkForPostgreSQL)

## Available for
* NetFramework 4.5
* NetFramework 4.6
* NetFramework 4.6.2
* NetFramework 4.7
* NetFramework 4.7.2
* NetFramework 4.8
* NetStandard 2.0

## Basic usage:
```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Logs";

string tableName = "logs";

IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
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

## Table auto creation:
If you set parameter `needAutoCreateTable` to `true`, the sink will automatically create the table.

```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=Logs";

string tableName = "logs";

IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
    { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
};

var logger = new LoggerConfiguration()
	.WriteTo.PostgreSQL(connectionString, tableName, columnWriters, needAutoCreateTable: true)
	.CreateLogger();
```

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

* **Version 1.0.1.0 (2019-05-08)** : Updated documentation, added documentation to the nuget package and all classes, added option to allow upper case table and column names.
Simplified building and packing scripts. Added support for NetFramework 4.6, NetFramework 4.6.2, NetFramework 4.7 and NetFramework 4.8.
* **Version 1.0.0.0 (2019-02-22)** : 1.0 release.