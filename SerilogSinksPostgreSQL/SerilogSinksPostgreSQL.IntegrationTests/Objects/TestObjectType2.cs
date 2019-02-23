namespace SerilogSinksPostgreSQL.IntegrationTests.Objects
{
    using System;

    /// <summary>
    /// This class is used as an example test object.
    /// </summary>
    public class TestObjectType2
    {
        /// <summary>
        /// Gets or sets the date property.
        /// </summary>
        /// <value>
        /// The date property.
        /// </value>
        public DateTime DateProp { get; set; }

        /// <summary>
        /// Gets or sets the nested property.
        /// </summary>
        /// <value>
        /// The nested property.
        /// </value>
        public TestObjectType1 NestedProp { get; set; }
    }
}