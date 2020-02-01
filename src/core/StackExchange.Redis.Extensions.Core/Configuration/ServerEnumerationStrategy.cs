namespace StackExchange.Redis.Extensions.Core.Configuration
{
    /// <summary>
    /// This class represent the Server enumeration strategy used in the operations 
    /// that require to iterate all the redis servers
    /// </summary>
	public partial class ServerEnumerationStrategy
    {
        /// <summary>
        /// The iteration mode.
        /// </summary>
        public enum ModeOptions
        {
            /// <summary>Will execute the operation on a all nodes.</summary>
            All = 0,

            /// <summary>Will execute the operation on a single node.</summary>
            Single
        }

        /// <summary>
        /// The target role.
        /// </summary>
        public enum TargetRoleOptions
        {
            /// <summary>Any kind of server.</summary>
            Any = 0,

            /// <summary>Prefer the slave.</summary>
            PreferSlave
        }

        /// <summary>
        /// The Unreachable server action.
        /// </summary>
        public enum UnreachableServerActionOptions
        {
            /// <summary>Will throw an exception in case the server is unreachable.</summary>
            Throw = 0,

            /// <summary>Will NOT throw an exception in case the server is unreachable.</summary>
            IgnoreIfOtherAvailable
        }

        //[ConfigurationProperty("mode", IsRequired = false, DefaultValue = "All")]

        /// <summary>
        /// Specify the strategy mode
        /// </summary>
        /// <value>
        ///   Default value All.
        /// </value>
        public ModeOptions Mode { get; set; }

        /// <summary>
        /// Specify the target role
        /// </summary>
        /// <value>
        ///   Default value Any.
        /// </value>
        public TargetRoleOptions TargetRole { get; set; }

        /// <summary>
        /// Specify the unreachable server action
        /// </summary>
        /// <value>
        ///   Default value Throw.
        /// </value>
        public UnreachableServerActionOptions UnreachableServerAction { get; set; }
    }
}
