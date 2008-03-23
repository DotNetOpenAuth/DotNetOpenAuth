namespace DotNetOpenId.RelyingParty {
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
		internal AuthenticationResponse(AuthenticationStatus status, Identifier claimedIdentifier, IDictionary<string, string> query) {
			Status = status;
			ClaimedIdentifier = claimedIdentifier;
			signedArguments = new Dictionary<string, string>();
			string signed;
			if (query.TryGetValue(Protocol.Constants.openid.signed, out signed)) {
				foreach (string fieldNoPrefix in signed.Split(',')) {
					string fieldWithPrefix = Protocol.Constants.openid.Prefix + fieldNoPrefix;
					string val;
					if (!query.TryGetValue(fieldWithPrefix, out val)) val = string.Empty;
					signedArguments[fieldWithPrefix] = val;
				}
			}
			// Only read extensions from signed argument list.
			IncomingExtensions = ExtensionArgumentsManager.CreateIncomingExtensions(signedArguments);
		}

		/// <summary>
		/// The detailed success or failure status of the authentication attempt.
		/// </summary>
		public AuthenticationStatus Status { get; private set; }
		/// <summary>
		/// An Identifier that the end user claims to own.
		/// </summary>
		public Identifier ClaimedIdentifier { get; private set; }
		IDictionary<string, string> signedArguments;
		/// <summary>
		/// Gets the set of arguments that the Provider included as extensions.
		/// </summary>
		public ExtensionArgumentsManager IncomingExtensions { get; private set; }

		internal Uri ReturnTo {
			get { return new Uri(signedArguments[Protocol.Constants.openid.return_to]); }
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
			return IncomingExtensions.GetExtensionArguments(extensionTypeUri);
		}

		internal static AuthenticationResponse Parse(IDictionary<string, string> query, IRelyingPartyApplicationStore store) {
			string mode = Util.GetRequiredArg(query, Protocol.Constants.openid.mode);
			string tokenString = Util.GetRequiredArg(query, Token.TokenKey);
			Token token = Token.Deserialize(tokenString, store);

			switch (mode) {
				case Protocol.Constants.Modes.cancel:
					return new AuthenticationResponse(AuthenticationStatus.Canceled, token.ClaimedIdentifier, query);
				case Protocol.Constants.Modes.error:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						"The provider returned an error: {0}", query[Protocol.Constants.openid.error],
						token.ClaimedIdentifier));
				case Protocol.Constants.Modes.id_res:
					return parseIdResResponse(query, token, store);
				default:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValue,
						Protocol.Constants.openid.mode, mode), token.ClaimedIdentifier);
			}
		}

		static AuthenticationResponse parseIdResResponse(IDictionary<string, string> query, Token token, IRelyingPartyApplicationStore store) {
			string user_setup_url;
			if (query.TryGetValue(Protocol.Constants.openid.user_setup_url, out user_setup_url))
				return new AuthenticationResponse(AuthenticationStatus.SetupRequired, token.ClaimedIdentifier, query);

			string assoc_handle = Util.GetRequiredArg(query, Protocol.Constants.openid.assoc_handle);

			Association assoc = store.GetAssociation(token.ProviderEndpoint, assoc_handle);
			AuthenticationResponse response;

			if (assoc == null) {
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				if (!verifyByProvider(query, token.ProviderEndpoint, store))
					throw new OpenIdException("check_authentication failed", token.ClaimedIdentifier);
			} else {
				if (assoc.IsExpired)
					throw new OpenIdException(String.Format(CultureInfo.CurrentUICulture,
						"Association with {0} expired", token.ProviderEndpoint), token.ClaimedIdentifier);

				verifyBySignature(query, assoc);
			}

			response = new AuthenticationResponse(AuthenticationStatus.Authenticated, token.ClaimedIdentifier, query);

			// Just a little extra something to make sure that what's signed in return_to
			// and doubled in the actual returned arguments is the same.
			detectAlteredArguments(response, query, Token.TokenKey);

			return response;
		}

		/// <summary>
		/// Verifies that a query is signed and that the signed fields have not been tampered with.
		/// </summary>
		/// <exception cref="OpenIdException">Thrown when the signature is missing or the query has been tampered with.</exception>
		static void verifyBySignature(IDictionary<string, string> query, Association assoc) {
			string sig = Util.GetRequiredArg(query, Protocol.Constants.openid.sig);
			string signed = Util.GetRequiredArg(query, Protocol.Constants.openid.signed);
			string[] signed_array = signed.Split(',');

			string v_sig = CryptUtil.ToBase64String(assoc.Sign(query, signed_array, Protocol.Constants.openid.Prefix));

			if (v_sig != sig)
				throw new OpenIdException(Strings.InvalidSignature);
		}

		/// <summary>
		/// Performs a dumb-mode authentication verification by making an extra
		/// request to the provider after the user agent was redirected back
		/// to the consumer site with an authenticated status.
		/// </summary>
		/// <returns>Whether the authentication is valid.</returns>
		static bool verifyByProvider(IDictionary<string, string> query, Uri serverUrl, IRelyingPartyApplicationStore store) {
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
