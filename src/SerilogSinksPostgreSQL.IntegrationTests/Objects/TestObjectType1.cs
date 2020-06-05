// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObjectType1.cs" company="Haemmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used as an example test object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SerilogSinksPostgreSQL.IntegrationTests.Objects
{
    using System.Diagnostics.CodeAnalysis;

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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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