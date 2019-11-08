dotnet build SerilogSinksPostgreSQL.sln -c Release
xcopy /s .\SerilogSinksPostgreSQL\bin\Release ..\Nuget\Source\
pause