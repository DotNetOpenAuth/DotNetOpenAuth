//-----------------------------------------------------------------------
// <copyright file="AliasManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Diagnostics;
	using System.Globalization;

	internal class AliasManager {
		private readonly string aliasFormat = "alias{0}";
		
		/// <summary>
		/// Tracks extension Type URIs and aliases assigned to them.
		/// </summary>
		private Dictionary<string, string> typeUriToAliasMap = new Dictionary<string, string>();
	
		/// <summary>
		/// Tracks extension aliases and Type URIs assigned to them.
		/// </summary>
		private Dictionary<string, string> aliasToTypeUriMap = new Dictionary<string, string>();

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
		/// Takes a sequence of type URIs and assigns aliases for all of them.
		/// </summary>
		/// <param name="typeUris">The type URIs to create aliases for.</param>
		/// <param name="preferredTypeUriToAliases">An optional dictionary of URI/alias pairs that suggest preferred aliases to use if available for certain type URIs.</param>
		public void AssignAliases(IEnumerable<string> typeUris, IDictionary<string, string> preferredTypeUriToAliases) {
			// First go through the actually used type URIs and see which ones have matching preferred aliases.
			if (preferredTypeUriToAliases != null) {
				foreach (string typeUri in typeUris) {
					if (typeUriToAliasMap.ContainsKey(typeUri)) {
						// this Type URI is already mapped to an alias.
						continue;
					}

					string preferredAlias;
					if (preferredTypeUriToAliases.TryGetValue(typeUri, out preferredAlias) && !IsAliasUsed(preferredAlias)) {
						SetAlias(preferredAlias, typeUri);
					}
				}
			}

			// Now go through the whole list again and assign whatever is left now that the preferred ones
			// have gotten their picks where available.
			foreach (string typeUri in typeUris) {
				if (typeUriToAliasMap.ContainsKey(typeUri)) {
					// this Type URI is already mapped to an alias.
					continue;
				}

				assignNewAlias(typeUri);
			}
		}
		
		/// <summary>
		/// Sets up aliases for any Type URIs in a dictionary that do not yet have aliases defined,
		/// and where the given preferred alias is still available.
		/// </summary>
		/// <param name="preferredTypeUriToAliases">A dictionary of type URI keys and alias values.</param>
		public void SetPreferredAliasesWhereNotSet(IDictionary<string, string> preferredTypeUriToAliases) {
			if (preferredTypeUriToAliases == null) throw new ArgumentNullException("preferredTypeUriToAliases");

			foreach (var pair in preferredTypeUriToAliases) {
				if (typeUriToAliasMap.ContainsKey(pair.Key)) {
					// type URI is already mapped
					continue;
				}

				if (aliasToTypeUriMap.ContainsKey(pair.Value)) {
					// alias is already mapped
					continue;
				}

				// The type URI and alias are as yet unset, so go ahead and assign them.
				SetAlias(pair.Value, pair.Key);
			}
		}

		/// <summary>
		/// Gets the Type Uri encoded by a given alias.
		/// </summary>
		public string ResolveAlias(string alias) {
			string typeUri = TryResolveAlias(alias);
			if (typeUri == null)
				throw new ArgumentOutOfRangeException("alias");
			return typeUri;
		}
		
		public string TryResolveAlias(string alias) {
			if (string.IsNullOrEmpty(alias)) throw new ArgumentNullException("alias");
			string typeUri = null;
			aliasToTypeUriMap.TryGetValue(alias, out typeUri);
			return typeUri;
		}

		public IEnumerable<string> Aliases {
			get { return aliasToTypeUriMap.Keys; }
		}
		
		/// <summary>
		/// Returns a value indicating whether an alias has already been assigned to a type URI.
		/// </summary>
		/// <param name="alias">The alias in question.</param>
		/// <returns>True if the alias has already been assigned.  False otherwise.</returns>
		public bool IsAliasUsed(string alias) {
			if (string.IsNullOrEmpty(alias)) throw new ArgumentNullException("alias");
			return aliasToTypeUriMap.ContainsKey(alias);
		}
		
		public bool IsAliasAssignedTo(string typeUri) {
			if (string.IsNullOrEmpty("typeUri")) throw new ArgumentNullException("typeUri");
			return typeUriToAliasMap.ContainsKey(typeUri);
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
