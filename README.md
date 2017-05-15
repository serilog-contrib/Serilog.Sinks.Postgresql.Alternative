# Serilog.Sinks.Postgresql
A [Serilog](https://github.com/serilog/serilog) sink that writes to PostgreSQL

**Package** - [Serilog.Sinks.PostgreSQL](https://www.nuget.org/packages/Serilog.Sinks.PostgreSQL/)
| **Platforms** - .NET 4.5, .NET Standart 1.3

#### Code

```csharp
string connectionstring = "User ID=serilog;Password=serilog;Host=localhost;Port=5432;Database=logs";

string tableName = "logs";

//Used columns (Key is a column name)
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    {"message", new RenderedMessageColumnWriter() },
    {"message_template", new MessageTemplateColumnWriter() },
    {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    {"raise_date", new TimeStampColumnWriter() },
    {"exception", new ExceptionColumnWriter() },
    {"properties", new LogEventSerializedColumnWriter() },
    {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Text) },
    {"machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.Raw) }
};

var logger = new LoggerConfiguration()
			        .WriteTo.PostgreSQL(connectionstring, tableName, columnWriters)
			        .CreateLogger();
```