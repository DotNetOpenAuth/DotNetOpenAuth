//-----------------------------------------------------------------------
// <copyright file="AliasManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Manages a fast, two-way mapping between type URIs and their aliases.
	/// </summary>
	internal class AliasManager {
		/// <summary>
		/// The format of auto-generated aliases.
		/// </summary>
		private const string AliasFormat = "alias{0}";

		/// <summary>
		/// Tracks extension Type URIs and aliases assigned to them.
		/// </summary>
		private Dictionary<string, string> typeUriToAliasMap = new Dictionary<string, string>();

		/// <summary>
		/// Tracks extension aliases and Type URIs assigned to them.
		/// </summary>
		private Dictionary<string, string> aliasToTypeUriMap = new Dictionary<string, string>();

		/// <summary>
		/// Gets the aliases that have been set.
		/// </summary>
		public IEnumerable<string> Aliases {
			get { return this.aliasToTypeUriMap.Keys; }
		}

		/// <summary>
		/// Gets an alias assigned for a given Type URI.  A new alias is assigned if necessary.
		/// </summary>
		/// <param name="typeUri">The type URI.</param>
		/// <returns>The alias assigned to this type URI.  Never null.</returns>
		public string GetAlias(string typeUri) {
			Requires.NotNullOrEmpty(typeUri, "typeUri");
			string alias;
			return this.typeUriToAliasMap.TryGetValue(typeUri, out alias) ? alias : this.AssignNewAlias(typeUri);
		}

		/// <summary>
		/// Sets an alias and the value that will be returned by <see cref="ResolveAlias"/>.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="typeUri">The type URI.</param>
		public void SetAlias(string alias, string typeUri) {
			Requires.NotNullOrEmpty(alias, "alias");
			Requires.NotNullOrEmpty(typeUri, "typeUri");
			this.aliasToTypeUriMap.Add(alias, typeUri);
			this.typeUriToAliasMap.Add(typeUri, alias);
		}

		/// <summary>
		/// Takes a sequence of type URIs and assigns aliases for all of them.
		/// </summary>
		/// <param name="typeUris">The type URIs to create aliases for.</param>
		/// <param name="preferredTypeUriToAliases">An optional dictionary of URI/alias pairs that suggest preferred aliases to use if available for certain type URIs.</param>
		public void AssignAliases(IEnumerable<string> typeUris, IDictionary<string, string> preferredTypeUriToAliases) {
			Requires.NotNull(typeUris, "typeUris");

			// First go through the actually used type URIs and see which ones have matching preferred aliases.
			if (preferredTypeUriToAliases != null) {
				foreach (string typeUri in typeUris) {
					if (this.typeUriToAliasMap.ContainsKey(typeUri)) {
						// this Type URI is already mapped to an alias.
						continue;
					}

					string preferredAlias;
					if (preferredTypeUriToAliases.TryGetValue(typeUri, out preferredAlias) && !this.IsAliasUsed(preferredAlias)) {
						this.SetAlias(preferredAlias, typeUri);
					}
				}
			}

			// Now go through the whole list again and assign whatever is left now that the preferred ones
			// have gotten their picks where available.
			foreach (string typeUri in typeUris) {
				if (this.typeUriToAliasMap.ContainsKey(typeUri)) {
					// this Type URI is already mapped to an alias.
					continue;
				}

				this.AssignNewAlias(typeUri);
			}
		}

		/// <summary>
		/// Sets up aliases for any Type URIs in a dictionary that do not yet have aliases defined,
		/// and where the given preferred alias is still available.
		/// </summary>
		/// <param name="preferredTypeUriToAliases">A dictionary of type URI keys and alias values.</param>
		public void SetPreferredAliasesWhereNotSet(IDictionary<string, string> preferredTypeUriToAliases) {
			Requires.NotNull(preferredTypeUriToAliases, "preferredTypeUriToAliases");

			foreach (var pair in preferredTypeUriToAliases) {
				if (this.typeUriToAliasMap.ContainsKey(pair.Key)) {
					// type URI is already mapped
					continue;
				}

				if (this.aliasToTypeUriMap.ContainsKey(pair.Value)) {
					// alias is already mapped
					continue;
				}

				// The type URI and alias are as yet unset, so go ahead and assign them.
				this.SetAlias(pair.Value, pair.Key);
			}
		}

		/// <summary>
		/// Gets the Type Uri encoded by a given alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>The Type URI.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the given alias does not have a matching TypeURI.</exception>
		public string ResolveAlias(string alias) {
			Requires.NotNullOrEmpty(alias, "alias");
			string typeUri = this.TryResolveAlias(alias);
			if (typeUri == null) {
				throw new ArgumentOutOfRangeException("alias");
			}
			return typeUri;
		}

		/// <summary>
		/// Gets the Type Uri encoded by a given alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>The Type URI for the given alias, or null if none for that alias exist.</returns>
		public string TryResolveAlias(string alias) {
			Requires.NotNullOrEmpty(alias, "alias");
			string typeUri = null;
			this.aliasToTypeUriMap.TryGetValue(alias, out typeUri);
			return typeUri;
		}

		/// <summary>
		/// Returns a value indicating whether an alias has already been assigned to a type URI.
		/// </summary>
		/// <param name="alias">The alias in question.</param>
		/// <returns>True if the alias has already been assigned.  False otherwise.</returns>
		public bool IsAliasUsed(string alias) {
			Requires.NotNullOrEmpty(alias, "alias");
			return this.aliasToTypeUriMap.ContainsKey(alias);
		}

		/// <summary>
		/// Determines whether a given TypeURI has an associated alias assigned to it.
		/// </summary>
		/// <param name="typeUri">The type URI.</param>
		/// <returns>
		/// 	<c>true</c> if the given type URI already has an alias assigned; <c>false</c> otherwise.
		/// </returns>
		public bool IsAliasAssignedTo(string typeUri) {
			Requires.NotNull(typeUri, "typeUri");
			return this.typeUriToAliasMap.ContainsKey(typeUri);
		}

		/// <summary>
		/// Assigns a new alias to a given Type URI.
		/// </summary>
		/// <param name="typeUri">The type URI to assign a new alias to.</param>
		/// <returns>The newly generated alias.</returns>
		private string AssignNewAlias(string typeUri) {
			Requires.NotNullOrEmpty(typeUri, "typeUri");
			ErrorUtilities.VerifyInternal(!this.typeUriToAliasMap.ContainsKey(typeUri), "Oops!  This type URI already has an alias!");
			string alias = string.Format(CultureInfo.InvariantCulture, AliasFormat, this.typeUriToAliasMap.Count + 1);
			this.typeUriToAliasMap.Add(typeUri, alias);
			this.aliasToTypeUriMap.Add(alias, typeUri);
			return alias;
		}
	}
}
