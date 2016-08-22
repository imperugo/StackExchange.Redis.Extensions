using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.ServerIteration
{
    public class ServerEnumerationStrategy
    {
        public ModeOptions Mode { get; set; }

        public TargetRoleOptions TargetRole { get; set; }

        public UnreachableServerActionOptions UnreachableServerAction { get; set; }

        public enum ModeOptions
        {
            All = 0,
            Single
        }

        public enum TargetRoleOptions
        {
            Any = 0,
            PreferSlave
        }

        public enum UnreachableServerActionOptions
        {
            Throw = 0,
            IgnoreIfOtherAvailable
        }
    }
}
