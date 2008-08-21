using System.Configuration;

namespace DotNetOpenId.Provider {
	sealed class ProviderConfigurationSectionHandler : ConfigurationSection {
		public ProviderConfigurationSectionHandler() {
		}

		const string securitySettingsConfigName = "security";
		[ConfigurationProperty(securitySettingsConfigName)]
		public ProviderSecuritySettingsConfigurationElement SecuritySettings {
			get { return (ProviderSecuritySettingsConfigurationElement)this[securitySettingsConfigName]; }
			set { this[securitySettingsConfigName] = value; }
		}
	}
}
