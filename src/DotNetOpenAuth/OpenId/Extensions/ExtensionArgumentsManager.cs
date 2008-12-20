//-----------------------------------------------------------------------
// <copyright file="ExtensionArgumentsManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;

	internal class ExtensionArgumentsManager : IIncomingExtensions, IOutgoingExtensions {
		private Protocol protocol;

		/// <summary>
		/// Whether extensions are being read or written.
		/// </summary>
		private bool isReadMode;

		private Extensions.AliasManager aliasManager = new Extensions.AliasManager();

		/// <summary>
		/// A complex dictionary where the key is the Type URI of the extension,
		/// and the value is another dictionary of the name/value args of the extension.
		/// </summary>
		private Dictionary<string, IDictionary<string, string>> extensions = new Dictionary<string, IDictionary<string, string>>();

		/// <summary>
		/// This contains a set of aliases that we must be willing to implicitly
		/// match to namespaces for backward compatibility with other OpenID libraries.
		/// </summary>
		private static readonly Dictionary<string, string> typeUriToAliasAffinity = new Dictionary<string, string> {
			// TODO: re-enable these lines.
			////{ Extensions.SimpleRegistration.Constants.sreg_ns, Extensions.SimpleRegistration.Constants.sreg_compatibility_alias },
			////{ Extensions.ProviderAuthenticationPolicy.Constants.TypeUri, Extensions.ProviderAuthenticationPolicy.Constants.pape_compatibility_alias },
		};

		private ExtensionArgumentsManager() { }

		public static ExtensionArgumentsManager CreateIncomingExtensions(IDictionary<string, string> query) {
			if (query == null) throw new ArgumentNullException("query");
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
			foreach (var pair in typeUriToAliasAffinity) {
				if (!mgr.aliasManager.IsAliasAssignedTo(pair.Key) &&
					!mgr.aliasManager.IsAliasUsed(pair.Value)) {
					mgr.aliasManager.SetAlias(pair.Value, pair.Key);
				}
			}
			// Second pass looks for extensions using those aliases
			foreach (var pair in query) {
				if (!pair.Key.StartsWith(mgr.protocol.openid.Prefix, StringComparison.Ordinal)) continue;
				string possibleAlias = pair.Key.Substring(mgr.protocol.openid.Prefix.Length);
				int periodIndex = possibleAlias.IndexOf(".", StringComparison.Ordinal);
				if (periodIndex >= 0) possibleAlias = possibleAlias.Substring(0, periodIndex);
				string typeUri;
				if ((typeUri = mgr.aliasManager.TryResolveAlias(possibleAlias)) != null) {
					if (!mgr.extensions.ContainsKey(typeUri))
						mgr.extensions[typeUri] = new Dictionary<string, string>();
					string key = periodIndex >= 0 ? pair.Key.Substring(mgr.protocol.openid.Prefix.Length + possibleAlias.Length + 1) : string.Empty;
					mgr.extensions[typeUri].Add(key, pair.Value);
				}
			}
			return mgr;
		}

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
		/// Gets the actual arguments to add to a querystring or other response,
		/// where type URI, alias, and actual key/values are all defined.
		/// </summary>
		public IDictionary<string, string> GetArgumentsToSend(bool includeOpenIdPrefix) {
			if (isReadMode) throw new InvalidOperationException();
			Dictionary<string, string> args = new Dictionary<string, string>();
			foreach (var typeUriAndExtension in extensions) {
				string typeUri = typeUriAndExtension.Key;
				var extensionArgs = typeUriAndExtension.Value;
				if (extensionArgs.Count == 0) continue;
				string alias = aliasManager.GetAlias(typeUri);
				// send out the alias declaration
				string openidPrefix = includeOpenIdPrefix ? protocol.openid.Prefix : string.Empty;
				args.Add(openidPrefix + protocol.openidnp.ns + "." + alias, typeUri);
				string prefix = openidPrefix + alias;
				foreach (var pair in extensionArgs) {
					string key = prefix;
					if (pair.Key.Length > 0) key += "." + pair.Key;
					args.Add(key, pair.Value);
				}
			}
			return args;
		}

		public void AddExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments) {
			if (isReadMode) {
				throw new InvalidOperationException();
			}
			ErrorUtilities.VerifyNonZeroLength(extensionTypeUri, "extensionTypeUri");
			ErrorUtilities.VerifyArgumentNotNull(arguments, "arguments");
			if (arguments.Count == 0) return;

			IDictionary<string, string> extensionArgs;
			if (!extensions.TryGetValue(extensionTypeUri, out extensionArgs)) {
				extensions.Add(extensionTypeUri, extensionArgs = new Dictionary<string, string>());
			}

			ErrorUtilities.VerifyProtocol(extensionArgs.Count == 0, OpenIdStrings.ExtensionAlreadyAddedWithSameTypeURI, extensionTypeUri);
			foreach (var pair in arguments) {
				extensionArgs.Add(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// Gets the fields carried by a given OpenId extension.
		/// </summary>
		/// <returns>The fields included in the given extension, or null if the extension is not present.</returns>
		public IDictionary<string, string> GetExtensionArguments(string extensionTypeUri) {
			if (!isReadMode) throw new InvalidOperationException();
			if (string.IsNullOrEmpty(extensionTypeUri)) throw new ArgumentNullException("extensionTypeUri");
			IDictionary<string, string> extensionArgs;
			extensions.TryGetValue(extensionTypeUri, out extensionArgs);
			return extensionArgs;
		}

		public bool ContainsExtension(string extensionTypeUri) {
			if (!isReadMode) throw new InvalidOperationException();
			return extensions.ContainsKey(extensionTypeUri);
		}
	}
}
