using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;

namespace DotNetOpenId.Configuration {
	internal class WhiteBlackListCollection : ConfigurationElementCollection {
		public WhiteBlackListCollection() { }

		protected override ConfigurationElement CreateNewElement() {
			return new WhiteBlackListElement();
		}

		protected override object GetElementKey(ConfigurationElement element) {
			return ((WhiteBlackListElement)element).Name;
		}

		internal IEnumerable<string> KeysAsStrings {
			get {
				foreach (WhiteBlackListElement element in this) {
					yield return element.Name;
				}
			}
		}

		internal IEnumerable<Regex> KeysAsRegexs {
			get {
				foreach (WhiteBlackListElement element in this) {
					yield return new Regex(element.Name);
				}
			}
		}
	}
}
