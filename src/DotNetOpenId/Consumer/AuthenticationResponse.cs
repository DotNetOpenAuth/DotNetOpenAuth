namespace DotNetOpenId.Consumer {
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Collections.Generic;

	public enum AuthenticationStatus {
		/// <summary>
		/// The authentication was canceled by the user agent while at the provider.
		/// </summary>
		Canceled,
		/// <summary>
		/// The authentication failed because the provider refused to send
		/// authentication approval.
		/// </summary>
		Failed,
		/// <summary>
		/// The Provider responded to a request for immediate authentication approval
		/// with a message stating that additional user agent interaction is required
		/// before authentication can be completed.
		/// </summary>
		SetupRequired,
		/// <summary>
		/// Authentication is completed successfully.
		/// </summary>
		Authenticated,
	}

	public class AuthenticationResponse {
		internal AuthenticationResponse(AuthenticationStatus status, Uri identityUrl, IDictionary<string, string> query) {
			Status = status;
			IdentityUrl = identityUrl;
			signedArguments = new Dictionary<string, string>();
			string signed;
			if (query.TryGetValue(QueryStringArgs.openid.signed, out signed)) {
				foreach (string fieldNoPrefix in signed.Split(',')) {
					string fieldWithPrefix = QueryStringArgs.openid.Prefix + fieldNoPrefix;
					string val;
					if (!query.TryGetValue(fieldWithPrefix, out val)) val = string.Empty;
					signedArguments[fieldWithPrefix] = val;
				}
			}
		}

		/// <summary>
		/// The detailed success or failure status of the authentication attempt.
		/// </summary>
		public AuthenticationStatus Status { get; private set; }
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
		public IDictionary<string, string> GetExtensionArguments(string extensionPrefix) {
			if (string.IsNullOrEmpty(extensionPrefix)) throw new ArgumentNullException("extensionPrefix");
			if (extensionPrefix.StartsWith(".", StringComparison.Ordinal) ||
				extensionPrefix.EndsWith(".", StringComparison.Ordinal))
				throw new ArgumentException(Strings.PrefixWithoutPeriodsExpected, "extensionPrefix");

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
