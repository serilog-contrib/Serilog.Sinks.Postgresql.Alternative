dotnet nuget push "src\SerilogSinksPostgreSQL\bin\Release\HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL.*.nupkg" -s "github" --skip-duplicate
dotnet nuget push "src\SerilogSinksPostgreSQL\bin\Release\HaemmerElectronics.SeppPenner.SerilogSinkForPostgreSQL.*.nupkg" -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
PAUSE