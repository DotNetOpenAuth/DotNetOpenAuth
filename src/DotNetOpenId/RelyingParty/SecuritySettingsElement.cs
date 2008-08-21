using System.Configuration;

namespace DotNetOpenId.RelyingParty {
	internal class SecuritySettingsElement : ConfigurationElement {
		public SecuritySettingsElement() { }

		public RelyingPartySecuritySettings CreateSecuritySettings() {
			RelyingPartySecuritySettings settings = new RelyingPartySecuritySettings();
			settings.RequireSsl = RequireSsl;
			settings.MinimumRequiredOpenIdVersion = MinimumRequiredOpenIdVersion;
			settings.MinimumHashBitLength = MinimumHashBitLength;
			settings.MaximumHashBitLength = MaximumHashBitLength;
			return settings;
		}

		const string requireSslConfigName = "requireSsl";
		[ConfigurationProperty(requireSslConfigName, DefaultValue = false)]
		public bool RequireSsl {
			get { return (bool)this[requireSslConfigName]; }
			set { this[requireSslConfigName] = value; }
		}

		const string minimumRequiredOpenIdVersionConfigName = "minimumRequiredOpenIdVersion";
		[ConfigurationProperty(minimumRequiredOpenIdVersionConfigName, DefaultValue = "V10")]
		public ProtocolVersion MinimumRequiredOpenIdVersion {
			get { return (ProtocolVersion)this[minimumRequiredOpenIdVersionConfigName]; }
			set { this[minimumRequiredOpenIdVersionConfigName] = value; }
		}

		const string minimumHashBitLengthConfigName = "minimumHashBitLength";
		[ConfigurationProperty(minimumHashBitLengthConfigName, DefaultValue = DotNetOpenId.SecuritySettings.minimumHashBitLengthDefault)]
		public int MinimumHashBitLength {
			get { return (int)this[minimumHashBitLengthConfigName]; }
			set { this[minimumHashBitLengthConfigName] = value; }
		}

		const string maximumHashBitLengthConfigName = "maximumHashBitLength";
		[ConfigurationProperty(maximumHashBitLengthConfigName, DefaultValue = DotNetOpenId.SecuritySettings.maximumHashBitLengthRPDefault)]
		public int MaximumHashBitLength {
			get { return (int)this[maximumHashBitLengthConfigName]; }
			set { this[maximumHashBitLengthConfigName] = value; }
		}
	}
}
