using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace DotNetOpenId.RelyingParty {
	internal class StoreConfigurationElement : ConfigurationElement {
		public StoreConfigurationElement() { }

		const string customStoreTypeConfigName = "type";
		[ConfigurationProperty(customStoreTypeConfigName)]
		//[SubclassTypeValidator(typeof(IRelyingPartyApplicationStore))]
		public string TypeName {
			get { return (string)this[customStoreTypeConfigName]; }
			set { this[customStoreTypeConfigName] = value; }
		}

		public Type CustomStoreType {
			get { return string.IsNullOrEmpty(TypeName) ? null : Type.GetType(TypeName); }
		}
	}
}
