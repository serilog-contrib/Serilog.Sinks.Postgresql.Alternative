#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System.Text;

global using Npgsql;
global using NpgsqlTypes;

global using Microsoft.Extensions.Configuration;
global using Microsoft.VisualStudio.TestTools.UnitTesting;

global using Serilog.Context;
global using Serilog.Events;
global using Serilog.Sinks.PostgreSQL;
global using Serilog.Sinks.PostgreSQL.ColumnWriters;
global using Serilog.Sinks.Postgresql.Alternative.IntegrationTests.Objects;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.