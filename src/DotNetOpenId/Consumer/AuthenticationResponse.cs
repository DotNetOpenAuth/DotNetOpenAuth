namespace DotNetOpenId.Consumer {
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Collections.Generic;
	using System.Web;
	using System.Globalization;

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

	class AuthenticationResponse : IAuthenticationResponse {
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
		/// <param name="extensionTypeUri">
		/// The Type URI of the OpenID extension whose arguments are being sought.
		/// </param>
		/// <returns>
		/// Returns key/value pairs for this extension.
		/// </returns>
		public IDictionary<string, string> GetExtensionArguments(string extensionTypeUri) {
			if (string.IsNullOrEmpty(extensionTypeUri)) throw new ArgumentNullException("extensionTypeUri");
			var response = new Dictionary<string, string>();
			string alias = findAliasForExtension(extensionTypeUri);
			if (alias == null) {
				// for OpenID 1.x compatibility, guess the sreg alias.
				if (extensionTypeUri == QueryStringArgs.sreg_ns &&
					!isExtensionAliasDefined(QueryStringArgs.sreg_compatibility_alias))
					alias = QueryStringArgs.sreg_compatibility_alias;
				else
					return response;
			}

			string extensionPrefix = QueryStringArgs.openid.Prefix + alias + ".";
			foreach (var pair in signedArguments) {
				if (pair.Key.StartsWith(extensionPrefix, StringComparison.OrdinalIgnoreCase)) {
					string bareKey = pair.Key.Substring(extensionPrefix.Length);
					response[bareKey] = pair.Value;
				}
			}

			return response;
		}

		bool isExtensionAliasDefined(string alias) {
			string aliasPrefix = QueryStringArgs.openid.ns + "." + alias;
			foreach (var arg in signedArguments) {
				if (arg.Key.Equals(aliasPrefix, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}

		string findAliasForExtension(string extensionTypeUri) {
			string aliasPrefix = QueryStringArgs.openid.ns + ".";
			foreach (var arg in signedArguments) {
				if (arg.Key.StartsWith(aliasPrefix, StringComparison.OrdinalIgnoreCase) &&
					arg.Value.Equals(extensionTypeUri, StringComparison.Ordinal)) {
					return arg.Key.Substring(aliasPrefix.Length);
				}
			}
			return null;
		}

		internal static AuthenticationResponse Parse(IDictionary<string, string> query, IConsumerApplicationStore store) {
			string mode;
			if (!query.TryGetValue(QueryStringArgs.openid.mode, out mode))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingOpenIdQueryParameter, QueryStringArgs.openid.mode));

			string tokenString;
			if (!query.TryGetValue(Token.TokenKey, out tokenString))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingInternalQueryParameter, Token.TokenKey));
			Token token = Token.Deserialize(tokenString, store);

			switch (mode) {
				case QueryStringArgs.Modes.cancel:
					return new AuthenticationResponse(AuthenticationStatus.Canceled, token.IdentityUrl, query);
				case QueryStringArgs.Modes.error:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						"The provider returned an error: {0}", query[QueryStringArgs.openid.error],
						token.IdentityUrl));
				case QueryStringArgs.Modes.id_res:
					return parseIdResResponse(query, token, store);
				default:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValue,
						QueryStringArgs.openid.mode, mode), token.IdentityUrl);
			}
		}

		static AuthenticationResponse parseIdResResponse(IDictionary<string, string> query, Token token, IConsumerApplicationStore store) {
			string user_setup_url;
			if (query.TryGetValue(QueryStringArgs.openid.user_setup_url, out user_setup_url))
				return new AuthenticationResponse(AuthenticationStatus.SetupRequired, token.IdentityUrl, query);

			string assoc_handle = getRequiredField(query, QueryStringArgs.openid.assoc_handle);

			Association assoc = store.GetAssociation(token.ServerUrl, assoc_handle);
			AuthenticationResponse response;

			if (assoc == null) {
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				if (!verifyByProvider(query, token.ServerUrl, store))
					throw new OpenIdException("check_authentication failed", token.IdentityUrl);

				response = new AuthenticationResponse(AuthenticationStatus.Authenticated, token.IdentityUrl, query);
			} else {
				if (assoc.IsExpired)
					throw new OpenIdException(String.Format(CultureInfo.CurrentUICulture,
						"Association with {0} expired", token.ServerUrl), token.IdentityUrl);

				verifyBySignature(query, assoc);

				response = new AuthenticationResponse(AuthenticationStatus.Authenticated, token.IdentityUrl, query);
			}

			// Just a little extra something to make sure that what's signed in return_to
			// and doubled in the actual returned arguments is the same.
			detectAlteredArguments(response, query, Token.TokenKey);

			return response;
		}

		static string getRequiredField(IDictionary<string, string> query, string key) {
			string val;
			if (!query.TryGetValue(key, out val))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingOpenIdQueryParameter, key));

			return val;
		}

		/// <summary>
		/// Verifies that a query is signed and that the signed fields have not been tampered with.
		/// </summary>
		/// <exception cref="OpenIdException">Thrown when the signature is missing or the query has been tampered with.</exception>
		static void verifyBySignature(IDictionary<string, string> query, Association assoc) {
			string sig = getRequiredField(query, QueryStringArgs.openid.sig);
			string signed = getRequiredField(query, QueryStringArgs.openid.signed);
			string[] signed_array = signed.Split(',');

			string v_sig = CryptUtil.ToBase64String(assoc.Sign(query, signed_array, QueryStringArgs.openid.Prefix));

			if (v_sig != sig)
				throw new OpenIdException(Strings.InvalidSignature);
		}

		/// <summary>
		/// Performs a dumb-mode authentication verification by making an extra
		/// request to the provider after the user agent was redirected back
		/// to the consumer site with an authenticated status.
		/// </summary>
		/// <returns>Whether the authentication is valid.</returns>
		static bool verifyByProvider(IDictionary<string, string> query, Uri serverUrl, IConsumerApplicationStore store) {
			var request = CheckAuthRequest.Create(serverUrl, query);
			if (request.Response == null)
				return false;
			if (request.Response.InvalidatedAssociationHandle != null)
				store.RemoveAssociation(serverUrl, request.Response.InvalidatedAssociationHandle);
			return request.Response.IsAuthenticationValid;
		}

		static void detectAlteredArguments(AuthenticationResponse response,
			IDictionary<string, string> query, params string[] argumentNames) {

			NameValueCollection return_to = HttpUtility.ParseQueryString(response.ReturnTo.Query);

			foreach (string arg in argumentNames) {
				string queryArg;
				query.TryGetValue(arg, out queryArg);
				if (queryArg != return_to[arg])
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ReturnToArgDifferentFromQueryArg, arg, return_to[arg], query[arg]));
			}
		}

	}
}
