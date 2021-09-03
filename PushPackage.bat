dotnet nuget push "src\Serilog.Sinks.Postgresql.Alternative\bin\Release\Serilog.Sinks.Postgresql.Alternative.*.nupkg" -s "github" --skip-duplicate
dotnet nuget push "src\Serilog.Sinks.Postgresql.Alternative\bin\Release\Serilog.Sinks.Postgresql.Alternative.*.nupkg" -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
PAUSE