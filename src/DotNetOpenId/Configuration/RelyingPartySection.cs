using System.Configuration;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Configuration {
	internal class RelyingPartySection : ConfigurationSection {
		internal static RelyingPartySection Configuration {
			get { return (RelyingPartySection)ConfigurationManager.GetSection("dotNetOpenId/relyingParty"); }
		}

		public RelyingPartySection() {
		}

		const string securitySettingsConfigName = "security";
		[ConfigurationProperty(securitySettingsConfigName)]
		public RelyingPartySecuritySettingsElement SecuritySettings {
			get { return (RelyingPartySecuritySettingsElement)this[securitySettingsConfigName]; }
			set { this[securitySettingsConfigName] = value; }
		}

		const string storeConfigName = "store";
		[ConfigurationProperty(storeConfigName)]
		public StoreConfigurationElement<IRelyingPartyApplicationStore> Store {
			get { return (StoreConfigurationElement<IRelyingPartyApplicationStore>)this[storeConfigName]; }
			set { this[storeConfigName] = value; }
		}
	}
}
