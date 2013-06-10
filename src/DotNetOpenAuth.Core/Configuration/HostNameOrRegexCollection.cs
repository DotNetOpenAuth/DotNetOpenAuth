//-----------------------------------------------------------------------
// <copyright file="HostNameOrRegexCollection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Collections.Generic;
	using System.Configuration;
	using System.Text.RegularExpressions;
	using Validation;

	/// <summary>
	/// Represents a collection of child elements that describe host names either as literal host names or regex patterns.
	/// </summary>
	internal class HostNameOrRegexCollection : ConfigurationElementCollection {
		/// <summary>
		/// Initializes a new instance of the <see cref="HostNameOrRegexCollection"/> class.
		/// </summary>
		public HostNameOrRegexCollection() {
		}

		/// <summary>
		/// Gets all the members of the collection assuming they are all literal host names.
		/// </summary>
		internal IEnumerable<string> KeysAsStrings {
			get {
				foreach (HostNameElement element in this) {
					yield return element.Name;
				}
			}
		}

		/// <summary>
		/// Gets all the members of the collection assuming they are all host names regex patterns.
		/// </summary>
		internal IEnumerable<Regex> KeysAsRegexs {
			get {
				foreach (HostNameElement element in this) {
					if (element.Name != null) {
						yield return new Regex(element.Name);
					}
				}
			}
		}

		/// <summary>
		/// Creates a new child host name element.
		/// </summary>
		/// <returns>
		/// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement() {
			return new HostNameElement();
		}

		/// <summary>
		/// Gets the element key for a specified configuration element.
		/// </summary>
		/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element) {
			Requires.NotNull(element, "element");
			return ((HostNameElement)element).Name ?? string.Empty;
		}
	}
}
