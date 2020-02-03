using System.Configuration;

namespace StackExchange.Redis.Extensions.LegacyConfiguration.Configuration
{
	/// <inheritdoc/>
	public class ServerEnumerationStrategyConfiguration : ConfigurationElement
	{

		[ConfigurationProperty("mode", IsRequired = false, DefaultValue = "All")]
		public Core.Configuration.ServerEnumerationStrategy.ModeOptions Mode
		{
			get { return (Core.Configuration.ServerEnumerationStrategy.ModeOptions)base["mode"]; }
			set { base["mode"] = value; }
		}

		[ConfigurationProperty("targetRole", IsRequired = false, DefaultValue = "Any")]
		public Core.Configuration.ServerEnumerationStrategy.TargetRoleOptions TargetRole
		{
			get { return (Core.Configuration.ServerEnumerationStrategy.TargetRoleOptions)base["targetRole"]; }
			set { base["targetRole"] = value; }
		}

		[ConfigurationProperty("unreachableServerAction", IsRequired = false, DefaultValue = "Throw")]
		public Core.Configuration.ServerEnumerationStrategy.UnreachableServerActionOptions UnreachableServerAction
		{
			get { return (Core.Configuration.ServerEnumerationStrategy.UnreachableServerActionOptions)base["unreachableServerAction"]; }
			set { base["unreachableServerAction"] = value; }
		}
	}
}