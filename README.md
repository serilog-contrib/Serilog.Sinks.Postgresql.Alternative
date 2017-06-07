# Serilog.Sinks.Postgresql
A [Serilog](https://github.com/serilog/serilog) sink that writes to PostgreSQL

**Package** - [Serilog.Sinks.PostgreSQL](https://www.nuget.org/packages/Serilog.Sinks.PostgreSQL/)
| **Platforms** - .NET 4.5, .NET Standard 1.3

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
    {"raise_date", new TimeStampColumnWriter(NpgsqlDbType.Timestamp) },
    {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    {"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    {"machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.Raw, NpgsqlDbType.Text) }
};

var logger = new LoggerConfiguration()
			        .WriteTo.PostgreSQL(connectionstring, tableName, columnWriters)
			        .CreateLogger();
```