namespace StackExchange.Redis.Extensions.Core.Models;

/// <summary>
/// A class that contains redis info.
/// </summary>
public class InfoDetail
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InfoDetail"/> class.
    /// </summary>
    /// <param name="category">The category name.</param>
    /// <param name="key">The redis key.</param>
    /// <param name="infoValue">The informations</param>
    public InfoDetail(string category, string key, string infoValue)
    {
        Category = category;
        Key = key;
        InfoValue = infoValue;
    }

    /// <summary>
    /// Gets or sets the category name
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets or sets the redis key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets the informations.
    /// </summary>
    public string InfoValue { get; }
}
