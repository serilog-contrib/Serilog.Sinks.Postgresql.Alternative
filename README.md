# Serilog.Sinks.Postgresql
A [Serilog](https://github.com/serilog/serilog) sink that writes to PostgreSQL

**Package** - [Serilog.Sinks.PostgreSQL](https://www.nuget.org/packages/Serilog.Sinks.PostgreSQL/)
| **Platforms** - .NET 4.5, .NET Standard 2.0

#### Code

```csharp
string connectionstring = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=logs";

string tableName = "logs";

//Used columns (Key is a column name) 
//Column type is writer's constructor parameter
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    {"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    {"raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
    {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    {"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    {"machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
};

var logger = new LoggerConfiguration()
			        .WriteTo.PostgreSQL(connectionstring, tableName, columnWriters)
			        .CreateLogger();
```


##### Table auto creation
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
