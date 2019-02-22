namespace SerilogSinksPostgreSQL.IntegrationTests.Objects
{
    using System;

    public class TestObjectType2
    {
        public DateTime DateProp1 { get; set; }

        public TestObjectType1 NestedProp { get; set; }
    }
}