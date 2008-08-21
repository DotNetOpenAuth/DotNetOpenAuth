using System.Configuration;

namespace DotNetOpenId.Provider {
	internal class ProviderSection : ConfigurationSection {
		public ProviderSection() {
		}

		const string securitySettingsConfigName = "security";
		[ConfigurationProperty(securitySettingsConfigName)]
		public ProviderSecuritySettingsElement SecuritySettings {
			get { return (ProviderSecuritySettingsElement)this[securitySettingsConfigName]; }
			set { this[securitySettingsConfigName] = value; }
		}
	}
}
