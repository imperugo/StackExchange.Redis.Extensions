using System.Collections;
using System.Collections.Generic;

namespace StackExchange.Redis.Extensions.Core.ServerIteration
{
    public class ServerEnumerable : IEnumerable<IServer>
    {
        private readonly ConnectionMultiplexer multiplexer;
        private readonly ServerEnumerationStrategy.TargetRoleOptions targetRole;
        private readonly ServerEnumerationStrategy.UnreachableServerActionOptions unreachableServerAction;

        public ServerEnumerable(
            ConnectionMultiplexer multiplexer,
            ServerEnumerationStrategy.TargetRoleOptions targetRole,
            ServerEnumerationStrategy.UnreachableServerActionOptions unreachableServerAction)
        {
            this.multiplexer = multiplexer;
            this.targetRole = targetRole;
            this.unreachableServerAction = unreachableServerAction;
        }

        public IEnumerator<IServer> GetEnumerator()
        {
            foreach (var endPoint in multiplexer.GetEndPoints())
            {
                var server = multiplexer.GetServer(endPoint);
                if (targetRole == ServerEnumerationStrategy.TargetRoleOptions.PreferSlave)
                {
                    if (!server.IsSlave)
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
}
