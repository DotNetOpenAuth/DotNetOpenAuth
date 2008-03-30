namespace DotNetOpenId.RelyingParty {
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Collections.Generic;
	using System.Web;
	using System.Globalization;
	using System.Diagnostics;

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
		internal AuthenticationResponse(AuthenticationStatus status, ServiceEndpoint provider, IDictionary<string, string> query) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (query == null) throw new ArgumentNullException("query");
			Status = status;
			Provider = provider;
			signedArguments = new Dictionary<string, string>();
			string signed;
			if (query.TryGetValue(Provider.Protocol.openid.signed, out signed)) {
				foreach (string fieldNoPrefix in signed.Split(',')) {
					string fieldWithPrefix = Provider.Protocol.openid.Prefix + fieldNoPrefix;
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
		public Identifier ClaimedIdentifier {
			get { return Provider.ClaimedIdentifier; }
		}
		/// <summary>
		/// The discovered endpoint information.
		/// </summary>
		internal ServiceEndpoint Provider { get; private set; }
		/// <summary>
		/// The arguments returned from the OP that were signed.
		/// </summary>
		IDictionary<string, string> signedArguments;
		/// <summary>
		/// Gets the set of arguments that the Provider included as extensions.
		/// </summary>
		public ExtensionArgumentsManager IncomingExtensions { get; private set; }

		internal Uri ReturnTo {
			get { return new Uri(Util.GetRequiredArg(signedArguments, Provider.Protocol.openid.return_to)); }
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

		internal static AuthenticationResponse Parse(IDictionary<string, string> query,
			IRelyingPartyApplicationStore store, Uri requestUrl) {
			if (query == null) throw new ArgumentNullException("query");
			if (requestUrl == null) throw new ArgumentNullException("requestUrl");
			ServiceEndpoint tokenEndpoint = null;
			string token = Util.GetOptionalArg(query, Token.TokenKey);
			if (token != null && store != null) {
				tokenEndpoint = Token.Deserialize(token, store).Endpoint;
			}

			Protocol protocol = Protocol.Detect(query);
			string mode = Util.GetRequiredArg(query, protocol.openid.mode);
			if (mode.Equals(protocol.Args.Mode.cancel, StringComparison.Ordinal)) {
				return new AuthenticationResponse(AuthenticationStatus.Canceled, tokenEndpoint, query);
			} else if (mode.Equals(protocol.Args.Mode.setup_needed, StringComparison.Ordinal)) {
				return new AuthenticationResponse(AuthenticationStatus.SetupRequired, tokenEndpoint, query);
			} else if (mode.Equals(protocol.Args.Mode.error, StringComparison.Ordinal)) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					"The provider returned an error: {0}", query[protocol.openid.error]));
			} else if (mode.Equals(protocol.Args.Mode.id_res, StringComparison.Ordinal)) {
				// We allow unsolicited assertions (that won't have our own token on it)
				// only for OpenID 2.0 providers.
				ServiceEndpoint responseEndpoint = null;
				if (protocol.Version.Major < 2) {
					if (tokenEndpoint == null)
						throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
							Strings.MissingInternalQueryParameter, Token.TokenKey));
				} else {
					// 2.0 OPs provide enough information to assemble the entire endpoint info
					responseEndpoint = ServiceEndpoint.ParseFromAuthResponse(query);
					// If this is a solicited assertion, we'll have a token with endpoint data too,
					// which we can use to more quickly confirm the validity of the claimed
					// endpoint info.
				}
				// At this point, we are guaranteed to have tokenEndpoint ?? responseEndpoint
				// set to endpoint data (one or the other or both).  
				// tokenEndpoint is known good data, whereas responseEndpoint must still be
				// verified.
				// For the error-handling and cancellation cases, the info does not have to
				// be verified, so we'll use whichever one is available.
				ServiceEndpoint unverifiedEndpoint = tokenEndpoint ?? responseEndpoint;

				return parseIdResResponse(query, tokenEndpoint, responseEndpoint, store, requestUrl);
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue,
					protocol.openid.mode, mode));
			}
		}

		static AuthenticationResponse parseIdResResponse(IDictionary<string, string> query,
			ServiceEndpoint tokenEndpoint, ServiceEndpoint responseEndpoint,
			IRelyingPartyApplicationStore store, Uri requestUrl) {
			// Use responseEndpoint if it is available so we get the
			// Claimed Identifer correct in the AuthenticationResponse.
			ServiceEndpoint unverifiedEndpoint = responseEndpoint ?? tokenEndpoint;
			if (unverifiedEndpoint.Protocol.Version.Major < 2) {
				string user_setup_url = Util.GetOptionalArg(query, unverifiedEndpoint.Protocol.openid.user_setup_url);
				if (user_setup_url != null) {
					return new AuthenticationResponse(AuthenticationStatus.SetupRequired, unverifiedEndpoint, query);
				}
			}

			verifyReturnTo(query, unverifiedEndpoint, requestUrl);
			verifyDiscoveredInfoMatchesAssertedInfo(query, tokenEndpoint, responseEndpoint);
			verifyNonceUnused(query, unverifiedEndpoint, store);
			verifySignature(query, unverifiedEndpoint, store);

			return new AuthenticationResponse(AuthenticationStatus.Authenticated, unverifiedEndpoint, query);
		}

		/// <summary>
		/// Verifies that the openid.return_to field matches the URL of the actual HTTP request.
		/// </summary>
		/// <remarks>
		/// From OpenId Authentication 2.0 section 11.1:
		/// To verify that the "openid.return_to" URL matches the URL that is processing this assertion:
		///  * The URL scheme, authority, and path MUST be the same between the two URLs.
		///  * Any query parameters that are present in the "openid.return_to" URL MUST 
		///    also be present with the same values in the URL of the HTTP request the RP received.
		/// </remarks>
		static void verifyReturnTo(IDictionary<string, string> query, ServiceEndpoint endpoint, Uri requestUrl) {
			Debug.Assert(query != null);
			Debug.Assert(endpoint != null);
			Debug.Assert(requestUrl != null);

			Uri return_to = new Uri(Util.GetRequiredArg(query, endpoint.Protocol.openid.return_to));
			if (return_to.Scheme != requestUrl.Scheme ||
				return_to.Authority != requestUrl.Authority ||
				return_to.AbsolutePath != requestUrl.AbsolutePath)
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ReturnToParamDoesNotMatchRequestUrl, endpoint.Protocol.openid.return_to));

			NameValueCollection returnToArgs = HttpUtility.ParseQueryString(return_to.Query);
			NameValueCollection requestArgs = HttpUtility.ParseQueryString(requestUrl.Query);
			foreach (string paramName in returnToArgs) {
				if (requestArgs[paramName] != returnToArgs[paramName])
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.ReturnToParamDoesNotMatchRequestUrl, endpoint.Protocol.openid.return_to));
			}
		}

		/// <remarks>
		/// This is documented in OpenId Authentication 2.0 section 11.2.
		/// </remarks>
		static void verifyDiscoveredInfoMatchesAssertedInfo(IDictionary<string, string> query, 
			ServiceEndpoint tokenEndpoint, ServiceEndpoint responseEndpoint) {
			if ((tokenEndpoint ?? responseEndpoint).Protocol.Version.Major < 2) {
				Debug.Assert(tokenEndpoint != null, "Our OpenID 1.x implementation requires an RP token.  And this should have been verified by our caller.");
				// For 1.x OPs, we only need to verify that the OP Local Identifier 
				// hasn't changed since we made the request.
				if (tokenEndpoint.ProviderLocalIdentifier !=
					Util.GetRequiredArg(query, tokenEndpoint.Protocol.openid.identity)) {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.TamperingDetected, tokenEndpoint.Protocol.openid.identity,
						tokenEndpoint.ProviderLocalIdentifier,
						Util.GetRequiredArg(query, tokenEndpoint.Protocol.openid.identity)));
				}
			} else {
				// In 2.0, we definitely have a responseEndpoint, but may not have a 
				// tokenEndpoint. If we don't have a tokenEndpoint or if the user 
				// gave us an OP Identifier originally, we need to perform discovery on
				// the responseEndpoint.ClaimedIdentifier to verify the OP has authority
				// to speak for it.
				if (tokenEndpoint == null ||
					tokenEndpoint.ClaimedIdentifier == tokenEndpoint.Protocol.ClaimedIdentifierForOPIdentifier) {
					Identifier claimedIdentifier = Util.GetRequiredArg(query, responseEndpoint.Protocol.openid.claimed_id);
					ServiceEndpoint claimedEndpoint = claimedIdentifier.Discover();
					// Compare the two ServiceEndpoints to make sure they are the same.
					if (responseEndpoint != claimedEndpoint)
						throw new OpenIdException(Strings.IssuedAssertionFailsIdentifierDiscovery);
				} else {
					// Check that the assertion matches the service endpoint we know about.
					if (responseEndpoint != tokenEndpoint)
						throw new OpenIdException(Strings.IssuedAssertionFailsIdentifierDiscovery);
				}
			}
		}

		static void verifyNonceUnused(IDictionary<string, string> query, ServiceEndpoint endpoint, IRelyingPartyApplicationStore store) {
			if (endpoint.Protocol.Version.Major < 2) return; // nothing to validate
			if (store == null) return; // we'll pass verifying the nonce responsibility to the OP
			var nonce = new Nonce(Util.GetRequiredArg(query, endpoint.Protocol.openid.response_nonce), true);
			nonce.Consume(store);
		}

		static void verifySignature(IDictionary<string, string> query, ServiceEndpoint endpoint, IRelyingPartyApplicationStore store) {
			string signed = Util.GetRequiredArg(query, endpoint.Protocol.openid.signed);
			string[] signedFields = signed.Split(',');

			// Check that all fields that are required to be signed are indeed signed
			if (endpoint.Protocol.Version.Major >= 2) {
				verifyFieldsAreSigned(signedFields,
					endpoint.Protocol.openidnp.op_endpoint,
					endpoint.Protocol.openidnp.return_to,
					endpoint.Protocol.openidnp.response_nonce,
					endpoint.Protocol.openidnp.assoc_handle);
				if (query.ContainsKey(endpoint.Protocol.openid.claimed_id))
					verifyFieldsAreSigned(signedFields,
						endpoint.Protocol.openidnp.claimed_id,
						endpoint.Protocol.openidnp.identity);
			} else {
				verifyFieldsAreSigned(signedFields,
					endpoint.Protocol.openidnp.identity,
					endpoint.Protocol.openidnp.return_to);
			}

			// Now actually validate the signature itself.
			string assoc_handle = Util.GetRequiredArg(query, endpoint.Protocol.openid.assoc_handle);
			Association assoc = store != null ? store.GetAssociation(endpoint.ProviderEndpoint, assoc_handle) : null;

			if (assoc == null) {
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				verifySignatureByProvider(query, endpoint, store);
			} else {
				if (assoc.IsExpired)
					throw new OpenIdException(String.Format(CultureInfo.CurrentUICulture,
						"Association with {0} expired", endpoint.ProviderEndpoint), endpoint.ClaimedIdentifier);

				verifySignatureByAssociation(query, endpoint.Protocol, signedFields, assoc);
			}
		}

		/// <summary>
		/// Checks that fields that must be signed are in fact signed.
		/// </summary>
		static void verifyFieldsAreSigned(string[] fieldsThatAreSigned, params string[] fieldsThatShouldBeSigned) {
			Debug.Assert(fieldsThatAreSigned != null);
			Debug.Assert(fieldsThatShouldBeSigned != null);
			foreach (string field in fieldsThatShouldBeSigned) {
				if (Array.IndexOf(fieldsThatAreSigned, field) < 0)
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.FieldMustBeSigned, field));
			}
		}

		/// <summary>
		/// Verifies that a query is signed and that the signed fields have not been tampered with.
		/// </summary>
		/// <exception cref="OpenIdException">Thrown when the signature is missing or the query has been tampered with.</exception>
		static void verifySignatureByAssociation(IDictionary<string, string> query, Protocol protocol, string[] signedFields, Association assoc) {
			string sig = Util.GetRequiredArg(query, protocol.openid.sig);

			string v_sig = Convert.ToBase64String(assoc.Sign(query, signedFields, protocol.openid.Prefix));

			if (v_sig != sig)
				throw new OpenIdException(Strings.InvalidSignature);
		}

		/// <summary>
		/// Performs a dumb-mode authentication verification by making an extra
		/// request to the provider after the user agent was redirected back
		/// to the consumer site with an authenticated status.
		/// </summary>
		/// <returns>Whether the authentication is valid.</returns>
		static void verifySignatureByProvider(IDictionary<string, string> query, ServiceEndpoint provider, IRelyingPartyApplicationStore store) {
			var request = CheckAuthRequest.Create(provider, query);
			if (request.Response.InvalidatedAssociationHandle != null && store != null)
				store.RemoveAssociation(provider.ProviderEndpoint, request.Response.InvalidatedAssociationHandle);
			if (!request.Response.IsAuthenticationValid)
				throw new OpenIdException(Strings.InvalidSignature);
		}
	}
}
