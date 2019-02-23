// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyWriteMethod.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   This enumeration contains the property write method.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL
{
    /// <summary>
    ///     This enumeration contains the property write method.
    /// </summary>
    public enum PropertyWriteMethod
    {
        /// <summary>
        ///     The raw method.
        /// </summary>
        Raw,

        /// <summary>
        ///     The to string method.
        /// </summary>
        ToString,

        /// <summary>
        ///     The json method.
        /// </summary>
        Json
    }
}