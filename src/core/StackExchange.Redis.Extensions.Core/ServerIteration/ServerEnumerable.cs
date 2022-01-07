// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.ServerIteration;

/// <summary>
/// The class that allows you to enumerate all the redis servers.
/// </summary>
public class ServerEnumerable : IEnumerable<IServer>
{
    private readonly IConnectionMultiplexer multiplexer;
    private readonly ServerEnumerationStrategy.TargetRoleOptions targetRole;
    private readonly ServerEnumerationStrategy.UnreachableServerActionOptions unreachableServerAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEnumerable"/> class.
    /// </summary>
    /// <param name="multiplexer">The redis connection.</param>
    /// <param name="targetRole">The target role.</param>
    /// <param name="unreachableServerAction">The unreachable server strategy.</param>
    public ServerEnumerable(
        IConnectionMultiplexer multiplexer,
        ServerEnumerationStrategy.TargetRoleOptions targetRole,
        ServerEnumerationStrategy.UnreachableServerActionOptions unreachableServerAction)
    {
        this.multiplexer = multiplexer;
        this.targetRole = targetRole;
        this.unreachableServerAction = unreachableServerAction;
    }

    /// <summary>
    /// Return the enumerator of the Redis servers
    /// </summary>
    public IEnumerator<IServer> GetEnumerator()
    {
        foreach (var endPoint in multiplexer.GetEndPoints())
        {
            var server = multiplexer.GetServer(endPoint);
            if (targetRole == ServerEnumerationStrategy.TargetRoleOptions.PreferSlave)
            {
                if (!server.IsReplica)
                    continue;
            }

            if (unreachableServerAction == ServerEnumerationStrategy.UnreachableServerActionOptions.IgnoreIfOtherAvailable)
            {
                if (!server.IsConnected || !server.Features.Scan)
                    continue;
            }

            yield return server;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
