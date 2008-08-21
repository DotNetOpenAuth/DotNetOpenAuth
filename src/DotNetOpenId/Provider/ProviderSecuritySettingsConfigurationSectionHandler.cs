using System.Configuration;

namespace DotNetOpenId.Provider {
	sealed class ProviderSecuritySettingsConfigurationSectionHandler : ConfigurationSection {
		public ProviderSecuritySettings SecuritySettings { get; private set; }

		public ProviderSecuritySettingsConfigurationSectionHandler() {
			SecuritySettings = new ProviderSecuritySettings();
		}

		[ConfigurationProperty("minimumHashBitLength", DefaultValue = DotNetOpenId.SecuritySettings.minimumHashBitLengthDefault)]
		public int MinimumHashBitLength {
			get { return SecuritySettings.MinimumHashBitLength; }
			set { SecuritySettings.MinimumHashBitLength = value; }
		}

		[ConfigurationProperty("maximumHashBitLength", DefaultValue = DotNetOpenId.SecuritySettings.maximumHashBitLengthOPDefault)]
		public int MaximumHashBitLength {
			get { return SecuritySettings.MaximumHashBitLength; }
			set { SecuritySettings.MaximumHashBitLength = value; }
		}

		[ConfigurationProperty("protectDownlevelReplayAttacks", DefaultValue = false)]
		public bool ProtectDownlevelReplayAttacks {
			get { return SecuritySettings.ProtectDownlevelReplayAttacks; }
			set { SecuritySettings.ProtectDownlevelReplayAttacks = value; }
		}
	}
}
