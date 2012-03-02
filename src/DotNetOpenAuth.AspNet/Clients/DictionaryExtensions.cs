//-----------------------------------------------------------------------
// <copyright file="DictionaryExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Xml.Linq;

	internal static class DictionaryExtensions {
		/// <summary>
		/// Adds a key/value pair to the specified dictionary if the value is not null or empty.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public static void AddItemIfNotEmpty(this IDictionary<string, string> dictionary, string key, string value) {
			if (key == null) {
				throw new ArgumentNullException("key");
			}

			if (!String.IsNullOrEmpty(value)) {
				dictionary[key] = value;
			}
		}

		/// <summary>
		/// Adds the value from an XDocument with the specified element name if it's not empty.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="document">The document.</param>
		/// <param name="elementName">Name of the element.</param>
		public static void AddDataIfNotEmpty(
			this Dictionary<string, string> dictionary,
			XDocument document,
			string elementName) {
			var element = document.Root.Element(elementName);
			if (element != null) {
				dictionary.AddItemIfNotEmpty(elementName, element.Value);
			}
		}
	}
}
