using System;
using System.Collections.Generic;

using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Extensions;

internal static class SpanExtensions
{
    public static void EnumerateLines(this ReadOnlySpan<char> span, ref List<InfoDetail> data, ref string category)
    {
        var start = 0;

        while (start < span.Length)
        {
            var end = span[start..].IndexOf('\n');
            ReadOnlySpan<char> line;

            if (end == -1)
            {
                line = span[start..].Trim();
                start = span.Length; // Termina il loop
            }
            else
            {
                line = span[start..(start + end)].Trim();
                start += end + 1;
            }

            // Gestisci ogni riga
            if (line.IsEmpty)
                continue;

            if (line[0] == '#')
            {
                category = line[1..].Trim().ToString();
                continue;
            }

            var idx = line.IndexOf(':');
            if (idx > 0)
            {
                var key = line[..idx].Trim();
                var infoValue = line[(idx + 1)..].Trim();

                data.Add(new InfoDetail(category, key.ToString(), infoValue.ToString()));
            }
        }
    }
}
