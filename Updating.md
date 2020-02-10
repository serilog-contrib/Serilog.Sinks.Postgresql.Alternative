## How to update the package

1. Add your changes.
2. Update versions in `SerilogSinksPostgreSQL.csproj`.
3. Update the `global.json` dotnet version.
4. Run `build.bat` to build the project and copy the files to the `Nuget` folder.
5. Adjust the `package.nuspec`.
6. Copy the `Readme.md` file to the `Nuget` folder.
7. Run `pack.bat` to build the Nuget package.