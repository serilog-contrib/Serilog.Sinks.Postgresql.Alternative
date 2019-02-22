SerilogSinkForPostgreSQL
====================================

SerilogSinkForPostgreSQL is a library to save logging information from [Serilog](https://github.com/serilog/serilog) to [PostgreSQL](https://www.postgresql.org/).
The assembly was written and tested in .Net Framework 4.7.2 and .Net Standard 2.0.

[![Build status](https://ci.appveyor.com/api/projects/status/0ggd9vc0fw9gc92c?svg=true)](https://ci.appveyor.com/project/SeppPenner/serilogsinkforpostgresql)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/SerilogSinkForPostgreSQL.svg)](https://github.com/SeppPenner/SerilogSinkForPostgreSQL/stargazers)
[![GitHub license](https://img.shields.io/badge/license-AGPL-blue.svg)](https://raw.githubusercontent.com/SeppPenner/SerilogSinkForPostgreSQL/master/License.txt)
[![Nuget](https://img.shields.io/badge/SerilogSinkForPostgreSQL-Nuget-brightgreen.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL/)

## Basic usage:
```csharp
string connectionString = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=logs";

string tableName = "logs";

//Used columns (Key is a column name) 
//Column type is writer's constructor parameter
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
If you set parameter `needAutoCreateTable` to `true` sink automatically create table.
You can change column sizes by setting values in `TableCreator` class:
```csharp
//Sets size of all BIT and BIT VARYING columns to 20
TableCreator.DefaultBitColumnsLength = 20;

//Sets size of all CHAR columns to 30
TableCreator.DefaultCharColumnsLength = 30;

//Sets size of all VARCHAR columns to 50
TableCreator.DefaultVarcharColumnsLength = 50;
```

Change history
--------------

* **Version 1.0.0.0 (2019-02-22)** : 1.0 release.