using System.Configuration;

namespace DotNetOpenId.RelyingParty {
	public sealed class RelyingPartyConfigurationSectionHandler : ConfigurationSection {
		public RelyingPartyConfigurationSectionHandler() {
		}

		const string securitySettingsConfigName = "security";
		[ConfigurationProperty(securitySettingsConfigName)]
		public RelyingPartySecuritySettingsConfigurationElement SecuritySettings {
			get { return (RelyingPartySecuritySettingsConfigurationElement)this[securitySettingsConfigName]; }
			set { this[securitySettingsConfigName] = value; }
		}
	}
}
