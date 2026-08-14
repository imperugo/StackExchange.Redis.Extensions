// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Models;

/// <summary>
/// A class that contains redis info.
/// </summary>
/// <param name="category">The category name.</param>
/// <param name="key">The redis key.</param>
/// <param name="infoValue">The information</param>
public class InfoDetail(string category, string key, string infoValue)
{
    /// <summary>
    /// Gets or sets the category name
    /// </summary>
    public string Category { get; } = category;

    /// <summary>
    /// Gets or sets the redis key.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// Gets or sets the informations.
    /// </summary>
    public string InfoValue { get; } = infoValue;

    internal static InfoDetail[] ParseFrom(string? info)
    {
        var rows = InfoDetailsParser.ParseAsFlatRows(info);
        if (rows.Count < 1)
            return [];

        var details = new InfoDetail[rows.Count];
        for (var i = 0; i < rows.Count; i++)
        {
            var (category, key, val) = rows[i];
            details[i] = new(category, key, val);
        }

        return details;
    }
}
