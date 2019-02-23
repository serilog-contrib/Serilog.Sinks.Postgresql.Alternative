namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    /// This enumeration contains the property write method.
    /// </summary>
    public enum PropertyWriteMethod
    {
        /// <summary>
        /// The raw method.
        /// </summary>
        Raw,

        /// <summary>
        /// The to string method.
        /// </summary>
        ToString,

        /// <summary>
        /// The json method.
        /// </summary>
        Json
    }
}