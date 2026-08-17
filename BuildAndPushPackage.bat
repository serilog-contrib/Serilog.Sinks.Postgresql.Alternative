cd .\src

@ECHO off
cls

FOR /d /r . %%d in (bin,obj) DO (
	IF EXIST "%%d" (		 	 
		ECHO %%d | FIND /I "\node_modules\" > Nul && ( 
			ECHO.Skipping: %%d
		) || (
			ECHO.Deleting: %%d
			rd /s/q "%%d"
		)
	)
)

@ECHO.Building solution...
@dotnet restore
@dotnet build -c Release
@cd .\Serilog.Sinks.Postgresql.Alternative\bin\Release
@ECHO.Build successful.

REM Command echoing stays off on purpose. With ECHO ON, cmd prints every command line before it
REM runs it, and the key variable is already expanded at that point, so the key would land in the
REM console and in every redirected log. The @ in front of the push lines keeps it out of the
REM output even if echoing is switched back on above.
@IF "%NUGET_API_KEY%"=="" (
	ECHO.NUGET_API_KEY is not set, nothing was pushed.
	PAUSE
	EXIT /B 1
)

@dotnet nuget push *.nupkg -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
@dotnet nuget push *.snupkg -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
@ECHO.Upload success. Press any key to exit.
PAUSE