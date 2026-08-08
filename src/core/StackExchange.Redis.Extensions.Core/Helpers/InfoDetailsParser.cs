// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.Helpers;

/// <summary>
/// A helper class that provides methods to parse Redis info strings into structured data.
/// </summary>
/// <remarks>
/// See <see href="https://redis.io/docs/latest/commands/info/#return-information"/> for details about the format of the info command output.
/// </remarks>
internal static class InfoDetailsParser
{
    private const string LineSeparator = "\r\n";

    /// <summary>
    /// Parses the given info string into a list of sections, where each section contains a list of key-value pairs.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static List<List<KeyValuePair<string, string>>> ParseAsTreeRows(ReadOnlySpan<char> info)
    {
        List<List<KeyValuePair<string, string>>> sections = [];
#if NET9_0_OR_GREATER
        foreach (var r in info.Split(LineSeparator))
        {
            var line = info[r].Trim();
            if (line.IsEmpty)
                continue;

            if (TryParseSection(line, out var section))
                sections.Add([]);
            else if (TryParseDetail(line, out var detail))
                sections[^1].Add(detail);
        }
#else
        var start = 0;
        while (start < info.Length)
        {
            var end = info[start..].IndexOf(LineSeparator);
            ReadOnlySpan<char> line;
            if (end < 0)
            {
                line = info[start..].Trim();
                start = info.Length;
            }
            else
            {
                line = info[start..(start + end)].Trim();
                start += end + LineSeparator.Length;
            }

            if (line.IsEmpty)
                continue;

            if (TryParseSection(line, out var section))
                sections.Add([]);
            else if (TryParseDetail(line, out var detail))
                sections[^1].Add(detail);
        }

#endif
        return sections;
    }

    /// <summary>
    /// Parses the given info string into a flat list of tuples, where each tuple contains the section name, key, and value.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static List<(string Section, string Key, string Value)> ParseAsFlatRows(ReadOnlySpan<char> info)
    {
        List<(string Section, string Key, string Value)> rows = [];

        var currentSection = string.Empty;
#if NET9_0_OR_GREATER
        foreach (var r in info.Split(LineSeparator))
        {
            var line = info[r].Trim();
            if (line.IsEmpty)
                continue;

            if (TryParseSection(line, out var section))
                currentSection = section;
            else if (TryParseDetail(line, out var detail))
                rows.Add((currentSection, detail.Key, detail.Value));
        }
#else
        var start = 0;
        while (start < info.Length)
        {
            var end = info[start..].IndexOf(LineSeparator);
            ReadOnlySpan<char> line;
            if (end < 0)
            {
                line = info[start..].Trim();
                start = info.Length;
            }
            else
            {
                line = info[start..(start + end)].Trim();
                start += end + LineSeparator.Length;
            }

            if (line.IsEmpty)
                continue;

            if (TryParseSection(line, out var section))
                currentSection = section;
            else if (TryParseDetail(line, out var detail))
                rows.Add((currentSection, detail.Key, detail.Value));
        }

#endif
        return rows;
    }

    /// <summary>
    /// Parses the given info string into a dictionary of key-value pairs. Ingores section.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ParseAsDictionary(ReadOnlySpan<char> info)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);

        // No need to trim the line, as TryParseDetail will handle it
#if NET9_0_OR_GREATER
        foreach (var r in info.Split(LineSeparator))
        {
            var line = info[r];
            if (TryParseDetail(line, out var detail))
                dict.Add(detail.Key, detail.Value);
        }
#else
        var start = 0;
        while (start < info.Length)
        {
            var end = info[start..].IndexOf(LineSeparator);
            ReadOnlySpan<char> line;
            if (end < 0)
            {
                line = info[start..];
                start = info.Length;
            }
            else
            {
                line = info[start..(start + end)];
                start += end + LineSeparator.Length;
            }

            if (TryParseDetail(line, out var detail))
                dict.Add(detail.Key, detail.Value);
        }

#endif
        return dict;
    }

    private static bool TryParseSection(ReadOnlySpan<char> line, out string section)
    {
        section = string.Empty;

        if (line[0] != '#')
            return false;

        section = new(line[1..].Trim());
        return true;
    }

    private static bool TryParseDetail(ReadOnlySpan<char> line, out KeyValuePair<string, string> detail)
    {
        var idx = line.IndexOf(':');
        if (idx <= 0)
        {
            detail = default;
            return false;
        }

        var key = new string(line[..idx].Trim());
        var value = new string(line[(idx + 1)..].Trim());
        detail = new(key, value);
        return true;
    }
}
