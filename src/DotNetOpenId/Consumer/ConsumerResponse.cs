namespace DotNetOpenId.Consumer {
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Collections.Generic;

	public class ConsumerResponse {
		internal ConsumerResponse(Uri identityUrl, IDictionary<string, string> query, string signed) {
			IdentityUrl = identityUrl;
			signedArguments = new Dictionary<string, string>();
			foreach (string fieldNoPrefix in signed.Split(',')) {
				string fieldWithPrefix = QueryStringArgs.openid.Prefix + fieldNoPrefix;
				string val;
				if (!query.TryGetValue(fieldWithPrefix, out val)) val = string.Empty;
				signedArguments[fieldWithPrefix] = val;
			}
		}

		public Uri IdentityUrl { get; private set; }
		IDictionary<string, string> signedArguments;

		internal Uri ReturnTo {
			get { return new Uri(signedArguments[QueryStringArgs.openid.return_to]); }
		}

		/// <summary>
		/// Gets the key/value pairs of a provider's response for a given OpenID extension.
		/// </summary>
		/// <param name="extensionPrefix">
		/// The prefix used by the extension, not including the 'openid.' start.
		/// For example, simple registration key/values can be retrieved by passing 
		/// 'sreg' as this argument.
		/// </param>
		/// <returns>
		/// Returns key/value pairs where the keys do not include the 
		/// 'openid.' or the <paramref name="extensionPrefix"/>.
		/// </returns>
		public IDictionary<string, string> GetExtensionResponse(string extensionPrefix) {
			var response = new Dictionary<string, string>();
			extensionPrefix = QueryStringArgs.openid.Prefix + extensionPrefix + ".";
			int prefix_len = extensionPrefix.Length;
			foreach (var pair in this.signedArguments) {
				if (pair.Key.StartsWith(extensionPrefix, StringComparison.OrdinalIgnoreCase)) {
					string bareKey = pair.Key.Substring(prefix_len);
					response[bareKey] = pair.Value;
				}
			}

			return response;
		}

	}
}
