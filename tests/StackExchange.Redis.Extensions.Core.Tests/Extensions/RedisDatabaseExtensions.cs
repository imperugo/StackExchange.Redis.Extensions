// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace StackExchange.Redis.Extensions.Tests.Extensions;

internal static class RedisDatabaseExtensions
{
    public static void FlushDatabase(this IDatabase db)
    {
        foreach (var endpoint in db.Multiplexer.GetEndPoints())
        {
            var server = db.Multiplexer.GetServer(endpoint);

            server.FlushDatabase();
        }
    }
}
