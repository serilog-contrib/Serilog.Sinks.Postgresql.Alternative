// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyExclusionTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used to test leaving properties out of the JSON columns against the database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
///     This class is used to test leaving properties out of the JSON columns against the database.
///     Test for issue https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/8.
/// </summary>
[TestClass]
public sealed class PropertyExclusionTests : BaseTests
{
    /// <summary>
    ///     The database helper.
    /// </summary>
    private readonly DbHelper databaseHelper = new(ConnectionString);

    /// <summary>
    ///     This method is used to test that <c>ExcludedProperties</c> and <c>ExcludeSinglePropertyColumns</c> from a
    ///     JSON configuration leave the properties out of the <c>Properties</c> and <c>LogEvent</c> columns.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    [TestMethod]
    public async Task PropertyExclusionsFromJsonConfigurationShouldBeLeftOut()
    {
        const string TableName = "LogsPropertyExclusion1";
        await this.databaseHelper.RemoveTable(string.Empty, TableName);

        var json = $$"""
            {
              "Serilog": {
                "Using": [ "Serilog.Sinks.Postgresql.Alternative" ],
                "WriteTo": [
                  {
                    "Name": "PostgreSQL",
                    "Args": {
                      "connectionString": "{{ConnectionString}}",
                      "tableName": "{{TableName}}",
                      "needAutoCreateTable": true,
                      "loggerColumnOptions": {
                        "Properties": { "Name": "Properties", "ExcludedProperties": [ "Secret" ] },
                        "LogEvent": { "Name": "LogEvent", "ExcludeSinglePropertyColumns": true }
                      },
                      "loggerPropertyColumnOptions": {
                        "Application": { "Name": "Application", "Format": "{0}", "WriteMethod": "Raw", "DbType": "Text" }
                      }
                    }
                  }
                ]
              }
            }
            """;

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build();

        using (var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger())
        {
            logger.Information("Request from {Application} with {Secret} and {Other}", "WeatherApp", "1234", 42);
        }

        var rows = await this.databaseHelper.GetRows(string.Empty, TableName, "Properties", "LogEvent", "Application");
        Assert.AreEqual(1, rows.Count);
        CollectionAssert.AreEquivalent(new[] { "Application", "Other" }, GetPropertyNames((string)rows[0][0]));
        CollectionAssert.AreEquivalent(new[] { "Other", "Secret" }, GetPropertyNames((string)rows[0][1], "Properties"));
        Assert.AreEqual("WeatherApp", rows[0][2]);
    }

    /// <summary>
    ///     Gets the names of the properties of a JSON object.
    /// </summary>
    /// <param name="json">The JSON.</param>
    /// <param name="childName">The name of the child object to look into, or null for the root object.</param>
    /// <returns>The property names.</returns>
    private static string[] GetPropertyNames(string json, string? childName = null)
    {
        using var document = JsonDocument.Parse(json);
        var element = childName is null ? document.RootElement : document.RootElement.GetProperty(childName);
        return [.. element.EnumerateObject().Select(p => p.Name)];
    }
}
