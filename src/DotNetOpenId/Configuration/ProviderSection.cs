using System.Configuration;
using DotNetOpenId.Provider;
using IProviderAssociationStore = DotNetOpenId.IAssociationStore<DotNetOpenId.AssociationRelyingPartyType>;

namespace DotNetOpenId.Configuration {
	internal class ProviderSection : ConfigurationSection {
		public ProviderSection() {
		}

		const string securitySettingsConfigName = "security";
		[ConfigurationProperty(securitySettingsConfigName)]
		public ProviderSecuritySettingsElement SecuritySettings {
			get { return (ProviderSecuritySettingsElement)this[securitySettingsConfigName]; }
			set { this[securitySettingsConfigName] = value; }
		}

		const string storeConfigName = "store";
		[ConfigurationProperty(storeConfigName)]
		public StoreConfigurationElement<IProviderAssociationStore> Store {
			get { return (StoreConfigurationElement<IProviderAssociationStore>)this[storeConfigName]; }
			set { this[storeConfigName] = value; }
		}
	}
}
