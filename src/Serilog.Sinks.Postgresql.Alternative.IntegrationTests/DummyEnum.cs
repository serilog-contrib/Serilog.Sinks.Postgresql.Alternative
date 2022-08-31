// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyEnum.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   A dummy enum for testing enum as integer writing logic.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.Postgresql.Alternative.IntegrationTests;

/// <summary>
/// A dummy enum for testing enum as integer writing logic.
/// </summary>
public enum DummyEnum
{
    /// <summary>
    /// Test 1.
    /// </summary>
    Test1 = 0,

    /// <summary>
    /// Test 2.
    /// </summary>
    Test2 = 1
}