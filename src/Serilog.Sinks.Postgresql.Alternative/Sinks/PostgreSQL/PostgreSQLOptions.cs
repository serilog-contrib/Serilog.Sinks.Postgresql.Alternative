// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgreSqlOptions.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   A options class for the PostgreSQL sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.PostgreSQL;

/// <summary>
/// A options class for the PostgreSql sink.
/// </summary>
public sealed class PostgreSqlOptions
{
    /// <summary>
    ///     Gets or sets the connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the format provider.
    /// </summary>
    public IFormatProvider? FormatProvider { get; set; }

    /// <summary>
    ///     Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the copy command is used or not.
    /// </summary>
    public bool UseCopy { get; set; }

    /// <summary>
    ///     Gets or sets the column options.
    /// </summary>
    public IDictionary<string, ColumnWriterBase> ColumnOptions { get; set; } = new Dictionary<string, ColumnWriterBase>();

    /// <summary>
    /// Gets or sets the batching period.
    /// </summary>
    public TimeSpan Period { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the table should be auto-created if it does not already exist or not.
    /// </summary>
    public bool NeedAutoCreateTable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the schema should be auto-created if it does not already exist or not.
    /// </summary>
    public bool NeedAutoCreateSchema { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of events to include in a single batch.
    /// </summary>
    public int BatchSizeLimit { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of events that should be stored in the batching queue.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    ///  Gets or sets the create table callback.
    /// </summary>
    public Action<CreateTableEventArgs>? OnCreateTable { get; set; }

    /// <summary>
    ///  Gets or sets the create schema callback.
    /// </summary>
    public Action<CreateSchemaEventArgs>? OnCreateSchema { get; set; }

    /// <summary>
    /// Gets or sets the retention time of the log entries in the database.
    /// </summary>
    public TimeSpan? RetentionTime { get; set; }
}
