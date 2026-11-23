// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultColumnWriter.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   The default column writer, needed for JSON configuration of the default columns.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL.ColumnWriters;

/// <summary>
/// The default column writer, needed for JSON configuration of the default columns.
/// </summary>
public sealed class DefaultColumnWriter
{
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of the column writer if needed.
    /// Is used for sorting the columns as the writers are ordered alphabetically per default.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Gets or sets the names of the properties that the <c>Properties</c>, <c>LogEvent</c> and <c>LogEventUtc</c>
    /// columns leave out of their JSON.
    /// </summary>
    public List<string>? ExcludedProperties { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the <c>Properties</c>, <c>LogEvent</c> and <c>LogEventUtc</c> columns
    /// leave the properties out of their JSON that have their own column in the <c>loggerPropertyColumnOptions</c>.
    /// </summary>
    public bool ExcludeSinglePropertyColumns { get; set; }
}
