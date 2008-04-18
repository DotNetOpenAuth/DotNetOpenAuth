using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.Extensions {
	class AliasManager {
		readonly string aliasFormat = "alias{0}";
		/// <summary>
		/// Tracks extension Type URIs and aliases assigned to them.
		/// </summary>
		Dictionary<string, string> typeUriToAliasMap = new Dictionary<string, string>();
		/// <summary>
		/// Tracks extension aliases and Type URIs assigned to them.
		/// </summary>
		Dictionary<string, string> aliasToTypeUriMap = new Dictionary<string, string>();

		/// <summary>
		/// Gets an alias assigned for a given Type URI.  A new alias is assigned if necessary.
		/// </summary>
		public string GetAlias(string typeUri) {
			if (string.IsNullOrEmpty(typeUri)) throw new ArgumentNullException("typeUri");
			string alias;
			if (typeUriToAliasMap.TryGetValue(typeUri, out alias))
				return alias;
			else
				return assignNewAlias(typeUri);
		}

		/// <summary>
		/// Sets an alias and the value that will be returned by <see cref="ResolveAlias"/>.
		/// </summary>
		public void SetAlias(string alias, string typeUri) {
			if (string.IsNullOrEmpty(alias)) throw new ArgumentNullException("alias");
			if (string.IsNullOrEmpty(typeUri)) throw new ArgumentNullException("typeUri");
			aliasToTypeUriMap.Add(alias, typeUri);
			typeUriToAliasMap.Add(typeUri, alias);
		}

		/// <summary>
		/// Gets the Type Uri encoded by a given alias.
		/// </summary>
		public string ResolveAlias(string alias) {
			if (string.IsNullOrEmpty(alias)) throw new ArgumentNullException("alias");
			string typeUri;
			if (!aliasToTypeUriMap.TryGetValue(alias, out typeUri))
				throw new ArgumentOutOfRangeException("alias");
			return typeUri;
		}

		public IEnumerable<string> Aliases {
			get { return aliasToTypeUriMap.Keys; }
		}

		string assignNewAlias(string typeUri) {
			Debug.Assert(!string.IsNullOrEmpty(typeUri));
			Debug.Assert(!typeUriToAliasMap.ContainsKey(typeUri));
			string alias = string.Format(CultureInfo.InvariantCulture, aliasFormat, typeUriToAliasMap.Count + 1);
			typeUriToAliasMap.Add(typeUri, alias);
			aliasToTypeUriMap.Add(alias, typeUri);
			return alias;
		}
	}
}
