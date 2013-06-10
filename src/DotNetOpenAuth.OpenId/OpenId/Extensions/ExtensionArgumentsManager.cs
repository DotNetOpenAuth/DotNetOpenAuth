//-----------------------------------------------------------------------
// <copyright file="ExtensionArgumentsManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Manages the processing and construction of OpenID extensions parts.
	/// </summary>
	internal class ExtensionArgumentsManager {
		/// <summary>
		/// This contains a set of aliases that we must be willing to implicitly
		/// match to namespaces for backward compatibility with other OpenID libraries.
		/// </summary>
		private static readonly Dictionary<string, string> typeUriToAliasAffinity = new Dictionary<string, string> {
			{ Extensions.SimpleRegistration.Constants.TypeUris.Standard, Extensions.SimpleRegistration.Constants.sreg_compatibility_alias },
			{ Extensions.ProviderAuthenticationPolicy.Constants.TypeUri, Extensions.ProviderAuthenticationPolicy.Constants.CompatibilityAlias },
		};

		/// <summary>
		/// The version of OpenID that the message is using.
		/// </summary>
		private Protocol protocol;

		/// <summary>
		/// Whether extensions are being read or written.
		/// </summary>
		private bool isReadMode;

		/// <summary>
		/// The alias manager that will track Type URI to alias mappings.
		/// </summary>
		private AliasManager aliasManager = new AliasManager();

		/// <summary>
		/// A complex dictionary where the key is the Type URI of the extension,
		/// and the value is another dictionary of the name/value args of the extension.
		/// </summary>
		private Dictionary<string, IDictionary<string, string>> extensions = new Dictionary<string, IDictionary<string, string>>();

		/// <summary>
		/// Prevents a default instance of the <see cref="ExtensionArgumentsManager"/> class from being created.
		/// </summary>
		private ExtensionArgumentsManager() { }

		/// <summary>
		/// Gets a value indicating whether the extensions are being read (as opposed to written).
		/// </summary>
		internal bool ReadMode {
			get { return this.isReadMode; }
		}

		/// <summary>
		/// Creates a <see cref="ExtensionArgumentsManager"/> instance to process incoming extensions.
		/// </summary>
		/// <param name="query">The parameters in the OpenID message.</param>
		/// <returns>The newly created instance of <see cref="ExtensionArgumentsManager"/>.</returns>
		public static ExtensionArgumentsManager CreateIncomingExtensions(IDictionary<string, string> query) {
			Requires.NotNull(query, "query");
			var mgr = new ExtensionArgumentsManager();
			mgr.protocol = Protocol.Detect(query);
			mgr.isReadMode = true;
			string aliasPrefix = mgr.protocol.openid.ns + ".";

			// First pass looks for namespace aliases
			foreach (var pair in query) {
				if (pair.Key.StartsWith(aliasPrefix, StringComparison.Ordinal)) {
					mgr.aliasManager.SetAlias(pair.Key.Substring(aliasPrefix.Length), pair.Value);
				}
			}

			// For backwards compatibility, add certain aliases if they aren't defined.
			if (mgr.protocol.Version.Major < 2) {
				foreach (var pair in typeUriToAliasAffinity) {
					if (!mgr.aliasManager.IsAliasAssignedTo(pair.Key) &&
						!mgr.aliasManager.IsAliasUsed(pair.Value)) {
						mgr.aliasManager.SetAlias(pair.Value, pair.Key);
					}
				}
			}

			// Second pass looks for extensions using those aliases
			foreach (var pair in query) {
				if (!pair.Key.StartsWith(mgr.protocol.openid.Prefix, StringComparison.Ordinal)) {
					continue;
				}
				string possibleAlias = pair.Key.Substring(mgr.protocol.openid.Prefix.Length);
				int periodIndex = possibleAlias.IndexOf(".", StringComparison.Ordinal);
				if (periodIndex >= 0) {
					possibleAlias = possibleAlias.Substring(0, periodIndex);
				}
				string typeUri;
				if ((typeUri = mgr.aliasManager.TryResolveAlias(possibleAlias)) != null) {
					if (!mgr.extensions.ContainsKey(typeUri)) {
						mgr.extensions[typeUri] = new Dictionary<string, string>();
					}
					string key = periodIndex >= 0 ? pair.Key.Substring(mgr.protocol.openid.Prefix.Length + possibleAlias.Length + 1) : string.Empty;
					mgr.extensions[typeUri].Add(key, pair.Value);
				}
			}
			return mgr;
		}

		/// <summary>
		/// Creates a <see cref="ExtensionArgumentsManager"/> instance to prepare outgoing extensions.
		/// </summary>
		/// <param name="protocol">The protocol version used for the outgoing message.</param>
		/// <returns>
		/// The newly created instance of <see cref="ExtensionArgumentsManager"/>.
		/// </returns>
		public static ExtensionArgumentsManager CreateOutgoingExtensions(Protocol protocol) {
			var mgr = new ExtensionArgumentsManager();
			mgr.protocol = protocol;

			// Affinity for certain alias for backwards compatibility
			foreach (var pair in typeUriToAliasAffinity) {
				mgr.aliasManager.SetAlias(pair.Value, pair.Key);
			}
			return mgr;
		}

		/// <summary>
		/// Adds query parameters for OpenID extensions to the request directed
		/// at the OpenID provider.
		/// </summary>
		/// <param name="extensionTypeUri">The extension type URI.</param>
		/// <param name="arguments">The arguments for this extension to add to the message.</param>
		public void AddExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments) {
			RequiresEx.ValidState(!this.ReadMode);
			Requires.NotNullOrEmpty(extensionTypeUri, "extensionTypeUri");
			Requires.NotNull(arguments, "arguments");
			if (arguments.Count == 0) {
				return;
			}

			IDictionary<string, string> extensionArgs;
			if (!this.extensions.TryGetValue(extensionTypeUri, out extensionArgs)) {
				this.extensions.Add(extensionTypeUri, extensionArgs = new Dictionary<string, string>(arguments.Count));
			}

			ErrorUtilities.VerifyProtocol(extensionArgs.Count == 0, OpenIdStrings.ExtensionAlreadyAddedWithSameTypeURI, extensionTypeUri);
			foreach (var pair in arguments) {
				extensionArgs.Add(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// Gets the actual arguments to add to a querystring or other response,
		/// where type URI, alias, and actual key/values are all defined.
		/// </summary>
		/// <param name="includeOpenIdPrefix">
		/// <c>true</c> if the generated parameter names should include the 'openid.' prefix.
		/// This should be <c>true</c> for all but direct response messages.
		/// </param>
		/// <returns>A dictionary of key=value pairs to add to the message to carry the extension.</returns>
		internal IDictionary<string, string> GetArgumentsToSend(bool includeOpenIdPrefix) {
			RequiresEx.ValidState(!this.ReadMode);
			Dictionary<string, string> args = new Dictionary<string, string>();
			foreach (var typeUriAndExtension in this.extensions) {
				string typeUri = typeUriAndExtension.Key;
				var extensionArgs = typeUriAndExtension.Value;
				if (extensionArgs.Count == 0) {
					continue;
				}
				string alias = this.aliasManager.GetAlias(typeUri);

				// send out the alias declaration
				string openidPrefix = includeOpenIdPrefix ? this.protocol.openid.Prefix : string.Empty;
				args.Add(openidPrefix + this.protocol.openidnp.ns + "." + alias, typeUri);
				string prefix = openidPrefix + alias;
				foreach (var pair in extensionArgs) {
					string key = prefix;
					if (pair.Key.Length > 0) {
						key += "." + pair.Key;
					}
					args.Add(key, pair.Value);
				}
			}
			return args;
		}

		/// <summary>
		/// Gets the fields carried by a given OpenId extension.
		/// </summary>
		/// <param name="extensionTypeUri">The type URI of the extension whose fields are being queried for.</param>
		/// <returns>
		/// The fields included in the given extension, or null if the extension is not present.
		/// </returns>
		internal IDictionary<string, string> GetExtensionArguments(string extensionTypeUri) {
			Requires.NotNullOrEmpty(extensionTypeUri, "extensionTypeUri");
			RequiresEx.ValidState(this.ReadMode);

			IDictionary<string, string> extensionArgs;
			this.extensions.TryGetValue(extensionTypeUri, out extensionArgs);
			return extensionArgs;
		}

		/// <summary>
		/// Gets whether any arguments for a given extension are present.
		/// </summary>
		/// <param name="extensionTypeUri">The extension Type URI in question.</param>
		/// <returns><c>true</c> if this extension is present; <c>false</c> otherwise.</returns>
		internal bool ContainsExtension(string extensionTypeUri) {
			return this.extensions.ContainsKey(extensionTypeUri);
		}

		/// <summary>
		/// Gets the type URIs of all discovered extensions in the message.
		/// </summary>
		/// <returns>A sequence of the type URIs.</returns>
		internal IEnumerable<string> GetExtensionTypeUris() {
			return this.extensions.Keys;
		}
	}
}
