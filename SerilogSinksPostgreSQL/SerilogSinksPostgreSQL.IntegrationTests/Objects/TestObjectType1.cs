// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObjectType1.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license
// </copyright>
// <summary>
//   This class is used as an example test object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.IntegrationTests.Objects
{
    /// <summary>
    ///     This class is used as an example test object.
    /// </summary>
    public class TestObjectType1
    {
        /// <summary>
        ///     Gets or sets the int property.
        /// </summary>
        /// <value>
        ///     The int property.
        /// </value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int IntProp { get; set; }

        /// <summary>
        ///     Gets or sets the string property.
        /// </summary>
        /// <value>
        ///     The string property.
        /// </value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string StringProp { get; set; }
    }
}