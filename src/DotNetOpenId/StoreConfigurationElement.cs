using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace DotNetOpenId {
	internal class StoreConfigurationElement<T> : ConfigurationElement {
		public StoreConfigurationElement() { }

		const string customStoreTypeConfigName = "type";
		[ConfigurationProperty(customStoreTypeConfigName)]
		//[SubclassTypeValidator(typeof(T))]
		public string TypeName {
			get { return (string)this[customStoreTypeConfigName]; }
			set { this[customStoreTypeConfigName] = value; }
		}

		public Type CustomStoreType {
			get { return string.IsNullOrEmpty(TypeName) ? null : Type.GetType(TypeName); }
		}

		public T CreateInstanceOfStore(T defaultValue) {
			return CustomStoreType != null ? (T)Activator.CreateInstance(CustomStoreType) : defaultValue;
		}
	}
}
