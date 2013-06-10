//-----------------------------------------------------------------------
// <copyright file="UntrustedWebRequestElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Configuration;

	/// <summary>
	/// Represents the section of a .config file where security policies regarding web requests
	/// to user-provided, untrusted servers is controlled.
	/// </summary>
	internal class UntrustedWebRequestElement : ConfigurationElement {
		#region Attribute names

		/// <summary>
		/// Gets the name of the @timeout attribute.
		/// </summary>
		private const string TimeoutConfigName = "timeout";

		/// <summary>
		/// Gets the name of the @readWriteTimeout attribute.
		/// </summary>
		private const string ReadWriteTimeoutConfigName = "readWriteTimeout";

		/// <summary>
		/// Gets the name of the @maximumBytesToRead attribute.
		/// </summary>
		private const string MaximumBytesToReadConfigName = "maximumBytesToRead";

		/// <summary>
		/// Gets the name of the @maximumRedirections attribute.
		/// </summary>
		private const string MaximumRedirectionsConfigName = "maximumRedirections";

		/// <summary>
		/// Gets the name of the @whitelistHosts attribute.
		/// </summary>
		private const string WhitelistHostsConfigName = "whitelistHosts";

		/// <summary>
		/// Gets the name of the @whitelistHostsRegex attribute.
		/// </summary>
		private const string WhitelistHostsRegexConfigName = "whitelistHostsRegex";

		/// <summary>
		/// Gets the name of the @blacklistHosts attribute.
		/// </summary>
		private const string BlacklistHostsConfigName = "blacklistHosts";

		/// <summary>
		/// Gets the name of the @blacklistHostsRegex attribute.
		/// </summary>
		private const string BlacklistHostsRegexConfigName = "blacklistHostsRegex";

		#endregion

		/// <summary>
		/// Gets or sets the read/write timeout after which an HTTP request will fail.
		/// </summary>
		[ConfigurationProperty(ReadWriteTimeoutConfigName, DefaultValue = "00:00:01.500")]
		[PositiveTimeSpanValidator]
		public TimeSpan ReadWriteTimeout {
			get { return (TimeSpan)this[ReadWriteTimeoutConfigName]; }
			set { this[ReadWriteTimeoutConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the timeout after which an HTTP request will fail.
		/// </summary>
		[ConfigurationProperty(TimeoutConfigName, DefaultValue = "00:00:10")]
		[PositiveTimeSpanValidator]
		public TimeSpan Timeout {
			get { return (TimeSpan)this[TimeoutConfigName]; }
			set { this[TimeoutConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum bytes to read from an untrusted web server.
		/// </summary>
		[ConfigurationProperty(MaximumBytesToReadConfigName, DefaultValue = 1024 * 1024)]
		[IntegerValidator(MinValue = 2048)]
		public int MaximumBytesToRead {
			get { return (int)this[MaximumBytesToReadConfigName]; }
			set { this[MaximumBytesToReadConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum redirections that will be followed before an HTTP request fails.
		/// </summary>
		[ConfigurationProperty(MaximumRedirectionsConfigName, DefaultValue = 10)]
		[IntegerValidator(MinValue = 0)]
		public int MaximumRedirections {
			get { return (int)this[MaximumRedirectionsConfigName]; }
			set { this[MaximumRedirectionsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the collection of hosts on the whitelist.
		/// </summary>
		[ConfigurationProperty(WhitelistHostsConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(HostNameOrRegexCollection))]
		public HostNameOrRegexCollection WhitelistHosts {
			get { return (HostNameOrRegexCollection)this[WhitelistHostsConfigName] ?? new HostNameOrRegexCollection(); }
			set { this[WhitelistHostsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the collection of hosts on the blacklist.
		/// </summary>
		[ConfigurationProperty(BlacklistHostsConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(HostNameOrRegexCollection))]
		public HostNameOrRegexCollection BlacklistHosts {
			get { return (HostNameOrRegexCollection)this[BlacklistHostsConfigName] ?? new HostNameOrRegexCollection(); }
			set { this[BlacklistHostsConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the collection of regular expressions that describe hosts on the whitelist.
		/// </summary>
		[ConfigurationProperty(WhitelistHostsRegexConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(HostNameOrRegexCollection))]
		public HostNameOrRegexCollection WhitelistHostsRegex {
			get { return (HostNameOrRegexCollection)this[WhitelistHostsRegexConfigName] ?? new HostNameOrRegexCollection(); }
			set { this[WhitelistHostsRegexConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the collection of regular expressions that describe hosts on the blacklist.
		/// </summary>
		[ConfigurationProperty(BlacklistHostsRegexConfigName, IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(HostNameOrRegexCollection))]
		public HostNameOrRegexCollection BlacklistHostsRegex {
			get { return (HostNameOrRegexCollection)this[BlacklistHostsRegexConfigName] ?? new HostNameOrRegexCollection(); }
			set { this[BlacklistHostsRegexConfigName] = value; }
		}
	}
}
