dotnet build SerilogSinksPostgreSQL.sln -c Release
xcopy /s .\SerilogSinksPostgreSQL\bin\Release ..\Nuget\Source\
xcopy /s .\Help ..\Nuget\Documentation\
pause