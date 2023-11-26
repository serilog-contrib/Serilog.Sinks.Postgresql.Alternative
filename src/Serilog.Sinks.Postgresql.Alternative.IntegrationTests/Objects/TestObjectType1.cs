// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObjectType1.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used as an example test object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests.Objects;

/// <summary>
///     This class is used as an example test object.
/// </summary>
public sealed class TestObjectType1
{
    /// <summary>
    ///     Gets or sets the int property.
    /// </summary>
    /// <value>
    ///     The int property.
    /// </value>
    public int IntProp { get; set; }

    /// <summary>
    ///     Gets or sets the string property.
    /// </summary>
    /// <value>
    ///     The string property.
    /// </value>
    public string StringProp { get; set; } = string.Empty;
}
