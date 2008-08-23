using System;
using System.Configuration;

namespace DotNetOpenId.Configuration {
	internal class UntrustedWebRequestSection : ConfigurationSection {
		internal static UntrustedWebRequestSection Configuration {
			get { return (UntrustedWebRequestSection)ConfigurationManager.GetSection("dotNetOpenId/untrustedWebRequest") ?? new UntrustedWebRequestSection(); }
		}

		public UntrustedWebRequestSection() {
			SectionInformation.AllowLocation = false;
		}

		const string readWriteTimeoutConfigName = "readWriteTimeout";
		[ConfigurationProperty(readWriteTimeoutConfigName, DefaultValue = "00:00:00.800")]
		[PositiveTimeSpanValidator]
		public TimeSpan ReadWriteTimeout {
			get { return (TimeSpan)this[readWriteTimeoutConfigName]; }
			set { this[readWriteTimeoutConfigName] = value; }
		}

		const string timeoutConfigName = "timeout";
		[ConfigurationProperty(timeoutConfigName, DefaultValue = "00:00:10")]
		[PositiveTimeSpanValidator]
		public TimeSpan Timeout {
			get { return (TimeSpan)this[timeoutConfigName]; }
			set { this[timeoutConfigName] = value; }
		}

		const string maximumBytesToReadConfigName = "maximumBytesToRead";
		[ConfigurationProperty(maximumBytesToReadConfigName, DefaultValue = 1024 * 1024)]
		[IntegerValidator(MinValue = 2048)]
		public int MaximumBytesToRead {
			get { return (int)this[maximumBytesToReadConfigName]; }
			set { this[maximumBytesToReadConfigName] = value; }
		}

		const string maximumRedirectionsConfigName = "maximumRedirections";
		[ConfigurationProperty(maximumRedirectionsConfigName, DefaultValue = 10)]
		[IntegerValidator(MinValue = 0)]
		public int MaximumRedirections {
			get { return (int)this[maximumRedirectionsConfigName]; }
			set { this[maximumRedirectionsConfigName] = value; }
		}

		const string whitelistHostsConfigName = "whitelistHosts";
		[ConfigurationProperty(whitelistHostsConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(WhiteBlackListCollection))]
		public WhiteBlackListCollection WhitelistHosts {
			get { return (WhiteBlackListCollection)this[whitelistHostsConfigName] ?? new WhiteBlackListCollection(); }
			set { this[whitelistHostsConfigName] = value; }
		}

		const string blacklistHostsConfigName = "blacklistHosts";
		[ConfigurationProperty(blacklistHostsConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(WhiteBlackListCollection))]
		public WhiteBlackListCollection BlacklistHosts {
			get { return (WhiteBlackListCollection)this[blacklistHostsConfigName] ?? new WhiteBlackListCollection(); }
			set { this[blacklistHostsConfigName] = value; }
		}

		const string whitelistHostsRegexConfigName = "whitelistHostsRegex";
		[ConfigurationProperty(whitelistHostsRegexConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(WhiteBlackListCollection))]
		public WhiteBlackListCollection WhitelistHostsRegex {
			get { return (WhiteBlackListCollection)this[whitelistHostsRegexConfigName] ?? new WhiteBlackListCollection(); }
			set { this[whitelistHostsRegexConfigName] = value; }
		}

		const string blacklistHostsRegexConfigName = "blacklistHostsRegex";
		[ConfigurationProperty(blacklistHostsRegexConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(WhiteBlackListCollection))]
		public WhiteBlackListCollection BlacklistHostsRegex {
			get { return (WhiteBlackListCollection)this[blacklistHostsRegexConfigName] ?? new WhiteBlackListCollection(); }
			set { this[blacklistHostsRegexConfigName] = value; }
		}
	}
}
