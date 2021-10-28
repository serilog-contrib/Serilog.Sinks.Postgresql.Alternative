Change history
--------------

* **Version 3.3.11.0 (2021-10-28)** : Updated nuget packages, fixed bug where configuration from a JSON file was broken (https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/31).
* **Version 3.3.10.0 (2021-10-14)** : Updated nuget packages, added custom order for table columns.
* **Version 3.3.9.0 (2021-09-12)** : Smaller changes due to switch to serilog-contrib, updated NuGet packages, updated icon.
* **Version 3.3.8.0 (2021-09-03)** : Updated license to fit the new owning repository, updated readme and so on to fit new package name.
* **Version 3.3.7.0 (2021-08-29)** : Added missing `queueLimit` parameter, updated nuget packages.
* **Version 3.3.6.0 (2021-08-09)** : Removed support for soon deprecated NetCore 2.1.
* **Version 3.3.5.0 (2021-07-25)** : Updated nuget packages, enabled source linking for debugging.
* **Version 3.3.4.0 (2021-06-04)** : Updated nuget packages.
* **Version 3.3.3.0 (2021-04-29)** : Updated nuget packages.
* **Version 3.3.2.0 (2021-03-04)** : Fixed create schema bug.
* **Version 3.3.1.0 (2021-03-03)** : Updated nuget packages, added support for audit sink (Thanks to [@bliusb](https://github.com/bliusb)), smaller cleanups.
* **Version 3.3.0.0 (2021-02-21)** : Updated nuget packages, cleaned up code, removed `PostgreSql` extension methods (Use `PostgreSQL` instead), added support for named connection strings.
* **Version 3.2.3.0 (2021-01-14)** : Added option to disable schema creation: `needAutoCreateSchema`.
* **Version 3.2.2.0 (2021-01-04)** : Added failure callback option, made `PostgreSql` call obsolete, use `PostgreSQL` now.
* **Version 3.2.1.0 (2021-01-03)** : Formatted json example, added override for the old sink naming `PostgreSQL` instead of the new one `PostgreSql` to avoid confusions with the original sink.
* **Version 3.2.0.0 (2020-12-26)** : Updated Nuget packages.
* **Version 3.1.0.0 (2020-11-16)** : Updated Nuget packages.
* **Version 3.0.0.0 (2020-11-11)** : Added support for .Net5.0, updated Nuget packages.
* **Version 2.5.0.0 (2020-09-06)** : Added option to store level as text, updated nuget packages, removed lookup table for log event levels.
* **Version 2.4.0.0 (2020-09-06)** : Added option to store log levels in table to be able to do a join query.
* **Version 2.3.0.0 (2020-09-06)** : Updated nuget packages.
* **Version 2.2.0.0 (2020-07-25)** : Updated nuget packages, changed to attach correct column writer to exception column (Thanks to [@GZidar](https://github.com/GZidar)).
* **Version 2.1.0.0 (2020-06-30)** : Updated nuget packages, allowing single columns within the JSON configuration (Thanks to [@artiomponkratov](https://github.com/artiomponkratov)).
* **Version 2.0.0.0 (2020-06-16)** : Fixed bugs with schema creation.
* **Version 1.9.0.0 (2020-06-16)** : Updated nuget packages, added option to configure the sink via a JSON file (Thanks to [@artiomponkratov](https://github.com/artiomponkratov)).
* **Version 1.8.1.0 (2020-06-05)** : Updated nuget packages, adjusted build to Visual Studio, moved changelog to extra file.
* **Version 1.8.0.0 (2020-06-04)** : Updated nuget packages, updated license year information.
* **Version 1.7.0.0 (2020-05-10)** : Updated nuget packages.
* **Version 1.6.0.0 (2020-04-26)** : Updated nuget packages.
* **Version 1.5.0.0 (2020-02-09)** : Updated nuget packages, updated available versions.
* **Version 1.4.0.0 (2019-11-08)** : Updated nuget packages.
* **Version 1.0.3.0 (2019-06-23)** : Added icon to the nuget package.
* **Version 1.0.2.0 (2019-05-13)** : Updated documentation, fixed some tests.
* **Version 1.0.1.0 (2019-05-08)** : Updated documentation, added documentation to the nuget package and all classes, added option to allow upper case table and column names.
Simplified building and packing scripts. Added support for NetFramework 4.6, NetFramework 4.6.2, NetFramework 4.7 and NetFramework 4.8.
* **Version 1.0.0.0 (2019-02-22)** : 1.0 release.