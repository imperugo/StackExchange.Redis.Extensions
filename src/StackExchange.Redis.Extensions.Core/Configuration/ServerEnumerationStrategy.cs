using System;
using System.Configuration;

namespace StackExchange.Redis.Extensions.Core.Configuration
{
	public class ServerEnumerationStrategy : ConfigurationElement
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
			Ignore
		}

		[ConfigurationProperty("mode", IsRequired = false, DefaultValue = "All")]
		public ModeOptions Mode
		{
			get { return (ModeOptions)base["mode"]; }
			set { base["mode"] = value; }
		}

		[ConfigurationProperty("targetRole", IsRequired = false, DefaultValue = "Any")]
		public TargetRoleOptions TargetRole
		{
			get { return (TargetRoleOptions)base["targetRole"]; }
			set { base["targetRole"] = value; }
		}

		[ConfigurationProperty("unreachableServerAction", IsRequired = false, DefaultValue = "Throw")]
		public UnreachableServerActionOptions UnreachableServerAction
		{
			get { return (UnreachableServerActionOptions)base["unreachableServerAction"]; }
			set { base["unreachableServerAction"] = value; }
		}
	}
}
