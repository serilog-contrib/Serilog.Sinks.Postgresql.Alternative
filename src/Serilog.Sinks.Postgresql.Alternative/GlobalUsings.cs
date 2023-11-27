#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System;
global using System.Text;

global using Microsoft.Extensions.Configuration;

global using Npgsql;

global using NpgsqlTypes;

global using Serilog.Configuration;
global using Serilog.Core;
global using Serilog.Debugging;
global using Serilog.Events;
global using Serilog.Formatting.Json;
global using Serilog.Sinks.PeriodicBatching;
global using Serilog.Sinks.PostgreSQL;
global using Serilog.Sinks.PostgreSQL.ColumnWriters;
global using Serilog.Sinks.PostgreSQL.Configuration;
global using Serilog.Sinks.PostgreSQL.EventArgs;

global using SystemEventArgs = System.EventArgs;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.