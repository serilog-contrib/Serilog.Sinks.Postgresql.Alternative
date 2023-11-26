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
public class DefaultColumnWriter
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
}
