# Project rules for Claude

## What this is

Serilog.Sinks.Postgresql.Alternative is a Serilog sink that writes log events into a PostgreSQL
table. It is published as the NuGet package
[Serilog.Sinks.Postgresql.Alternative](https://www.nuget.org/packages/Serilog.Sinks.Postgresql.Alternative/),
so `GeneratePackageOnBuild` is on and every build drops a `.nupkg` and a `.snupkg` into
`src/Serilog.Sinks.Postgresql.Alternative/bin/Release`. The project is a maintained fork of
[b00ted/serilog-sinks-postgresql](https://github.com/b00ted/serilog-sinks-postgresql).

One solution `src/Serilog.Sinks.Postgresql.Alternative.sln` with exactly three projects:

- `src/Serilog.Sinks.Postgresql.Alternative/Serilog.Sinks.Postgresql.Alternative.csproj`, the
  library and the only packed project.
- `src/Serilog.Sinks.Postgresql.Alternative.Tests/...csproj`, MSTest, unit tests that need no
  database.
- `src/Serilog.Sinks.Postgresql.Alternative.IntegrationTests/...csproj`, MSTest, tests that write
  into a real PostgreSQL instance.

Layout inside `src/Serilog.Sinks.Postgresql.Alternative`:

- `LoggerConfigurationPostgreSQLExtensions.cs`: the public entry point, four `PostgreSQL` extension
  methods (two for `LoggerSinkConfiguration`, two for `LoggerAuditSinkConfiguration`) plus the
  internal `GetOptions` and `ClearQuotationMarksFromColumnOptions`. Everything a user configures
  ends up in a `PostgreSqlOptions`.
- `Sinks/PostgreSQL/PostgreSQLSink.cs`: `IBatchedLogEventSink`, hands batches to the sink helper.
  `Sinks/PostgreSQL/PostgreSQLAuditSink.cs`: `ILogEventSink`, writes single events.
- `Sinks/PostgreSQL/SinkHelper.cs`: the actual work, shared by both sinks. Opens the connection,
  creates schema and table on demand, writes via `COPY` or `INSERT`, deletes old rows when a
  retention time is set. The query builders `GetCopyCommand`, `GetInsertQuery` and `GetDeleteQuery`
  each do one thing, keep new SQL in that shape.
- `Sinks/PostgreSQL/SchemaCreator.cs` and `TableCreator.cs`: the `CREATE SCHEMA` and `CREATE TABLE`
  statements. `SqlTypeHelper.cs`: `NpgsqlDbType` to SQL type string.
- `Sinks/PostgreSQL/ColumnWriters/`: `ColumnWriterBase` plus one writer per column kind.
  `DefaultColumnWriter` is not a writer at all, it is the DTO that JSON configuration binds to.
- `Sinks/PostgreSQL/ColumnOptions.cs`: the default column set. `DefaultColumnNames.cs`: its keys.
- `Sinks/PostgreSQL/Configuration/`: reads named connection strings out of an `IConfiguration`.
- `Sinks/PostgreSQL/EventArgs/`: the two callback argument types.
- `Sinks/PostgreSQL/Async/`: `AsyncEvent` and `AsyncEventInvocator`, currently unused, see below.
- `GlobalUsings.cs`: all usings of the project, including the alias `SystemEventArgs`.

Layout inside the test projects:

- `Serilog.Sinks.Postgresql.Alternative.Tests/ColumnWritersTests/`: one test class per column
  writer, 12 tests in total, no database and no network needed.
- `Serilog.Sinks.Postgresql.Alternative.IntegrationTests/`: `DbWriteTests`, `DbWriteWithSchemaTests`,
  the two `JsonConfigTest*` classes, the helper `DbHelper` and `BaseTests` with the connection
  string. The four `PostgreSinkConfiguration*.json` files are copied to the output directory with
  `CopyToOutputDirectory=Always`.

Repository root: `README.md` (badges, target frameworks, links), `HowToUse.md` (the actual user
documentation with all configuration options), `Changelog.md`, `Updating.md` (the five release
steps), `License.txt` (MIT), `Icon.png`, `BuildAndPushPackage.bat`, `Delete-BIN-OBJ-Folders.bat`,
`.all-contributorsrc`, `.gitattributes` and `.gitignore`. There is no `.github` folder and no
pipeline file.

## Build

```powershell
dotnet build src/Serilog.Sinks.Postgresql.Alternative.sln
```

```powershell
dotnet test src/Serilog.Sinks.Postgresql.Alternative.Tests/Serilog.Sinks.Postgresql.Alternative.Tests.csproj
```

- The library multi-targets `net8.0;net9.0`. Both test projects are single target `net9.0`.
- **Restore needs nuget.org.** A private feed is configured globally on this machine and answers
  404 or refuses the connection for public packages, so a plain `dotnet build` fails with `NU1301`
  and, because warnings are errors, additionally with `NU1900`. Always build with an explicit
  source:
  `dotnet build src/Serilog.Sinks.Postgresql.Alternative.sln -c Release --source https://api.nuget.org/v3/index.json`.
- `TreatWarningsAsErrors` is enabled in all three projects, so every warning breaks the build,
  NuGet warnings (`NU****`) from restore included. A clean build reports zero warnings, keep it that
  way.
- `NU1803` (HTTP source usage during restore) is the one warning suppressed via `NoWarn`. Fix
  warnings instead of extending that list. `NuGetAudit` and `NuGetAuditMode=all` are on, so a
  vulnerable transitive package fails the build too.
- `src/Directory.Build.props` exists but sets exactly one property,
  `GenerateDocumentationFile`. Every other build property is written out in each of the three
  `.csproj` files and duplicated there. Do not assume a property is inherited, check the csproj.
- Versions come from GitVersion.MsBuild out of the git tags, for example `4.2.1-1` for the first
  commit after tag `4.2.0`. Never edit a version property or an assembly version by hand.
- `dotnet test` on the **solution** also runs the 21 integration tests, which need a PostgreSQL
  server on `localhost:5432` with user `postgres`, password `postgres` and an existing database
  `Serilog` (see `IntegrationTests/BaseTests.cs`). The schemas `Logs2`, `Logs3` and `Logs4` have to
  exist in that database as well, the test comments say so and only `Logs1` is created by the test
  that uses `needAutoCreateSchema`. Without them two tests fail with `3F000`. For a quick check run
  the unit test project alone, that is the 12 tests above. Never claim a test run happened without
  running it.
- `dotnet list package --outdated` ignores `--source` for its own restore step and therefore dies
  on the private feed. Query `https://api.nuget.org/v3-flatcontainer/<id>/index.json` instead.

## Code conventions

Follow the surrounding code, it is consistent throughout every file:

- File header comment block with `<copyright file="..." company="SeppPenner and the Serilog
  contributors">` and a `<summary>`, then the file-scoped namespace.
- XML doc comments on every type and every member, private members included, no exceptions.
  Implementations of an interface member additionally carry `<inheritdoc cref="..."/>` and
  `<seealso cref="..."/>` pointing at that interface. Overrides of `ColumnWriterBase` do the same
  against the base class.
- `Nullable`, `ImplicitUsings` and `LangVersion latest` are enabled.
- New `using` directives go into the `GlobalUsings.cs` of the respective project, inside the
  existing `#pragma warning disable IDE0065` block, never at the top of a file. The editorconfig
  requires usings inside the namespace (`csharp_using_directive_placement=inside_namespace:warning`),
  which global usings cannot satisfy, that is what the pragma is for. Do not add other pragmas. The
  comment text in that block is German because Visual Studio generated it, leave it alone.
- Fields, properties, methods and events are always accessed with `this.` qualification
  (`dotnet_style_qualification_for_*` at severity `warning`).
- `src/.editorconfig` also enforces braces everywhere, no multiple blank lines, four spaces, CRLF,
  UTF-8, file scoped namespaces, `System` usings sorted first and `IDE0005` as warning. Analyzer
  warnings are fixed, not silenced.
- Indentation inside the `.csproj` files is four spaces, unlike the two spaces of
  `Directory.Build.props`.

## Known quirks

Do not silently "clean up" these, they are existing behaviour:

- **Three spellings of the same product.** The package and assembly are
  `Serilog.Sinks.Postgresql.Alternative`, the code namespace is `Serilog.Sinks.PostgreSQL` with
  capital `SQL`, and the test namespaces are `Serilog.Sinks.Postgresql.Alternative.Tests` and
  `...IntegrationTests`. The `RootNamespace` of the library is plain `Serilog` so that
  `LoggerConfigurationPostgreSqlExtensions` lands in the `Serilog` namespace and
  `WriteTo.PostgreSQL(...)` works without an extra using.
- **File names do not match the type names.** `PostgreSQLSink.cs` holds `PostgreSqlSink`,
  `PostgreSQLOptions.cs` holds `PostgreSqlOptions`, `LoggerConfigurationPostgreSQLExtensions.cs`
  holds `LoggerConfigurationPostgreSqlExtensions`, `IdAutoincrementColumnWriter.cs` holds
  `IdAutoIncrementColumnWriter`. Some `<copyright file="...">` attributes use the type spelling
  rather than the real file name. This is the published public API, renaming files is churn and
  renaming types is a breaking change.
- **Two copyright headers.** `LoggerConfigurationPostgreSQLExtensions.cs` carries a `SeppPenner and
  the Serilog contributors` block and a `TerumoBCT` block, `PostgreSQLAuditSink.cs` carries only the
  `TerumoBCT` one. That records who contributed the audit sink, leave it.
- **`Async/AsyncEvent.cs` and `Async/AsyncEventInvocator.cs` are public and unused.** They are
  leftovers of the `failureCallback` that version 4.2.0.0 deprecated and the commit
  "Removed failurecallback option." took out. Nothing in the library references them any more.
  They are public API, so deleting them is a breaking change and needs its own release note.
- **The audit sink blocks on purpose and every library await is `ConfigureAwait(false)`.**
  `PostgreSqlAuditSink.Emit` implements the synchronous `ILogEventSink.Emit` and therefore waits on
  `SinkHelper.Emit` with `GetAwaiter().GetResult()`, which is what lets an error reach the caller as
  Serilog's `AggregateException`. That only stays deadlock free because `SinkHelper`, `SchemaCreator`
  and `TableCreator` never resume on a captured synchronization context. Any new `await` in the
  library needs `ConfigureAwait(false)` for the same reason. Until version 4.3.0.0 the method was
  `async void`, which turned every failed write into an unhandled exception on the thread pool and
  took the whole process down.
- **The audit sink never uses `COPY`.** Both audit overloads pass `useCopy: false` and
  `period: TimeSpan.Zero` to `GetOptions`, the batched overloads default to `useCopy: true`. That is
  the point of the audit sink, one row per event, committed synchronously.
- **`TimestampColumnWriter` ignores the `dbType` you pass.** The parameterized constructor takes a
  `dbType` and then overwrites `this.DbType` with `NpgsqlDbType.TimestampTz` on purpose, see
  https://github.com/npgsql/npgsql/issues/2470. `LevelColumnWriter` does the same for
  `NpgsqlDbType.Text` when `renderAsText` is set. `HowToUse.md` still shows
  `new LevelColumnWriter(true, NpgsqlDbType.Varchar)`, which therefore writes `text`.
- **Identifiers are quoted, so table, schema and column names are case-sensitive.** `TableCreator`,
  `GetCopyCommand`, `GetInsertQuery` and `GetDeleteQuery` all wrap the names in double quotes, and
  `ClearQuotationMarksFromColumnOptions` plus the `Replace("\"", string.Empty)` calls in
  `GetOptions` strip quotes the user typed so they are not doubled. Any new query builder must quote
  the same way, an unquoted name is folded to lower case by PostgreSQL and then does not exist.
- **The default column names do not match the writer names.** `DefaultColumnNames.RenderedMessage`
  is `"Message"` and `DefaultColumnNames.LogEventSerialized` is `"LogEvent"`. `ColumnOptions.Default`
  is an expression-bodied property, so every call hands out a fresh dictionary.
- **`IdAutoIncrementColumnWriter.GetValue` throws by design.** Its `SkipOnInsert` is `true`, so the
  column never reaches an insert and `GetValue` is unreachable in normal operation. Its `GetSqlType`
  returns `SERIAL PRIMARY KEY` instead of a plain type.
- **The two `PostgreSQL` overloads per sink kind differ only in their dictionary type.** One takes
  `IDictionary<string, ColumnWriterBase>`, the other takes `IDictionary<string, DefaultColumnWriter>`
  plus `IDictionary<string, SinglePropertyColumnWriter>` for JSON configuration. A bare `null`
  argument is ambiguous, callers have to name the parameter. `HowToUse.md` shows a positional
  example that relies on this.
- **JSON configuration silently drops unknown column writers.** The `switch` over
  `columnOption.Value.Name` in both JSON overloads has no `default` branch, so a typo in the config
  produces a missing column, not an error. The accepted names are listed in `HowToUse.md`.
- **`SqlTypeHelper.DefaultBitColumnsLength`, `DefaultCharColumnsLength` and
  `DefaultVarcharColumnsLength` are `const`.** They cannot be changed at runtime, only recompiled.
  The "Adjusting column sizes" section of `HowToUse.md` shows assignments to `TableCreator.Default*`,
  which is wrong on both counts, wrong class and not settable.
- **A missing timestamp column only fails at the first flush.** `GetDeleteQuery` throws
  `ArgumentException("No timestamp column found.")` when a retention time is configured but no
  `TimestampColumnWriter` is in the column options. That happens inside `Emit`, not at
  configuration time.
- **The origin remote is the old repository name.** `git remote -v` points at
  `https://github.com/SeppPenner/SerilogSinkForPostgreSQL` while every link in the README, the
  changelog and the csproj uses `serilog-contrib/Serilog.Sinks.Postgresql.Alternative`. GitHub
  redirects, do not "fix" one side alone.
- **`PackageReleaseNotes` duplicates the newest `Changelog.md` entry.** Both have to be updated for
  a release, see `Updating.md`.
- **`README.md` contains generated blocks.** The `ALL-CONTRIBUTORS-BADGE` and
  `ALL-CONTRIBUTORS-LIST` sections and `.all-contributorsrc` belong to the all-contributors bot, do
  not hand-edit them.
- **`src/Serilog.Sinks.Postgresql.Alternative.sln.DotSettings`** is tracked and holds nothing but a
  ReSharper user dictionary (`bytea`, `H_00E4mmer`, `Npgsql`, `Terumo`, ...). Leave it alone.
- **`.gitattributes` is the Visual Studio template with everything commented out** except
  `* text=auto`. There is no binary file in the repository that would need its own rule.

## Releasing

1. Make the change.
2. Add an entry at the top of `Changelog.md` in the existing format:
   `* **Version 4.2.0.0 (2025-03-24)** : Short description.`
3. Update `PackageReleaseNotes` in
   `src/Serilog.Sinks.Postgresql.Alternative/Serilog.Sinks.Postgresql.Alternative.csproj` to the
   same text in its own format:
   `Version 4.2.0.0 (2025-03-24): Short description.`
4. Update the `## Available for` list in `README.md` if the target frameworks changed.
5. Commit that.
6. Tag the commit with the plain version number, no `v` prefix (`4.2.0`, `4.1.3`, ...). The existing
   tags are lightweight tags, create new ones the same way. Tag **before** building the package, so
   that GitVersion does not bake a prerelease version into the shipped assembly.
7. Push the commits and the tag.
8. `BuildAndPushPackage.bat` deletes all `bin` and `obj` folders, builds `-c Release` and pushes
   `*.nupkg` and `*.snupkg` to nuget.org with `%NUGET_API_KEY%`. It runs `dotnet restore` without an
   explicit source, so it needs the private feed to be reachable or removed from the machine
   configuration. Publishing to nuget.org is irreversible, only run it on request.

The version in `Changelog.md` and `PackageReleaseNotes` has four parts (`4.2.0.0`), the tag has
three (`4.2.0`). GitVersion turns the tag into the assembly and package version, so an untagged
commit produces something like `4.2.1-1`.

## Git

- **Never amend a commit.** No `git commit --amend`, not for a typo in the message, not to add a
  forgotten file, not even when the commit is still local. Write a follow-up commit instead. The
  release versions come from tags on exact commits, an amended commit leaves its tag pointing at a
  commit that no longer exists in the branch.

## Writing style

- Commit messages are written **in English only**: short, precise subject line, explanatory body
  when needed.
- Code comments and comments in project files such as `.csproj` are **always English**, regardless
  of the language used in the conversation.
- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments nor
  documentation. Use a regular hyphen, comma, colon, parentheses or a separate sentence.
- German texts (documentation, chat replies) always use real umlauts and ß, never ASCII
  transliterations such as `ae`, `oe`, `ue` or `ss`. Identifiers, file names and configuration keys
  stay unchanged where umlauts are technically undesirable.
