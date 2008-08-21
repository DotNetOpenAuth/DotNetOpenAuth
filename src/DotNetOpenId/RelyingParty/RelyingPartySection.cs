using System.Configuration;

namespace DotNetOpenId.RelyingParty {
	internal class RelyingPartySection : ConfigurationSection {
		public RelyingPartySection() {
		}

		const string securitySettingsConfigName = "security";
		[ConfigurationProperty(securitySettingsConfigName)]
		public SecuritySettingsElement SecuritySettings {
			get { return (SecuritySettingsElement)this[securitySettingsConfigName]; }
			set { this[securitySettingsConfigName] = value; }
		}

		const string storeConfigName = "store";
		[ConfigurationProperty(storeConfigName)]
		public StoreConfigurationElement Store {
			get { return (StoreConfigurationElement)this[storeConfigName]; }
			set { this[storeConfigName] = value; }
		}
	}
}
