namespace StackExchange.Redis.Extensions.Core.Configuration
{
	public partial class ServerEnumerationStrategy
	{
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
