namespace DotNetOpenId.Consumer
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using Org.Mentalis.Security.Cryptography;
	using DotNetOpenId;
	using System.Web;
	using System.IO;
	using System.Diagnostics;
	using System.Globalization;

	internal class GenericConsumer
	{
		static TimeSpan minimumUsefulAssociationLifetime {
			get { return OpenIdConsumer.MaximumUserAgentAuthenticationTime; }
		}

		IConsumerApplicationStore store;

		public GenericConsumer(IConsumerApplicationStore store)
		{
			this.store = store;
		}

		public AuthenticationRequest Begin(ServiceEndpoint service_endpoint,
			TrustRoot trustRoot, Uri returnToUrl)
		{
			string token = new Token(service_endpoint).Serialize(store);

			Association assoc = this.getAssociation(service_endpoint.ServerUrl);

			AuthenticationRequest request = new AuthenticationRequest(token, assoc, service_endpoint,
				trustRoot, returnToUrl);

			return request;
		}

		public AuthenticationResponse Complete(IDictionary<string, string> query)
		{
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
					return doIdRes(query, token);
				default:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValue,
						QueryStringArgs.openid.mode, mode), token.IdentityUrl);
			}
		}

		/// <summary>
		/// Performs a dumb-mode authentication verification by making an extra
		/// request to the provider after the user agent was redirected back
		/// to the consumer site with an authenticated status.
		/// </summary>
		/// <returns>Whether the authentication is valid.</returns>
		bool checkAuth(IDictionary<string, string> query, Uri serverUrl)
		{
			var request = CheckAuthRequest.Create(serverUrl, query);
			if (request.Response == null)
				return false;
			if (request.Response.InvalidatedAssociationHandle != null)
				store.RemoveAssociation(serverUrl, request.Response.InvalidatedAssociationHandle);
			return request.Response.IsAuthenticationValid;
		}

		public void detectAlteredArguments(AuthenticationResponse response, 
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

		string getRequiredField(IDictionary<string, string> query, string key) {
			string val;
			if (!query.TryGetValue(key, out val))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingOpenIdQueryParameter, key));

			return val;
		}

		AuthenticationResponse doIdRes(IDictionary<string, string> query, Token token)
		{
			string user_setup_url;
			if (query.TryGetValue(QueryStringArgs.openid.user_setup_url, out user_setup_url))
				return new AuthenticationResponse(AuthenticationStatus.SetupRequired, token.IdentityUrl, query);

			string assoc_handle = getRequiredField(query, QueryStringArgs.openid.assoc_handle);

			// TODO: I'm pretty sure this check doesn't accomplish anything. (ALA)
			if (token.ServerId.AbsoluteUri != token.ServerId.ToString())
				throw new OpenIdException("Provider ID (delegate) mismatch", token.IdentityUrl);

			Association assoc = store.GetAssociation(token.ServerUrl, assoc_handle);
			AuthenticationResponse response;

			if (assoc == null) {
				// It's not an association we know about.  Dumb mode is our
				// only possible path for recovery.
				if (!checkAuth(query, token.ServerUrl))
					throw new OpenIdException("check_authentication failed", token.IdentityUrl);

				response = new AuthenticationResponse(AuthenticationStatus.Authenticated, token.IdentityUrl, query);
			} else {
				if (assoc.IsExpired)
					throw new OpenIdException(String.Format(CultureInfo.CurrentUICulture,
						"Association with {0} expired", token.ServerUrl), token.IdentityUrl);

				verifySignature(query, assoc);

				response = new AuthenticationResponse(AuthenticationStatus.Authenticated, token.IdentityUrl, query);
			}

			// Just a little extra something to make sure that what's signed in return_to
			// and doubled in the actual returned arguments is the same.
			detectAlteredArguments(response, query, Token.TokenKey);

			return response;
		}

		/// <summary>
		/// Verifies that a query is signed and that the signed fields have not been tampered with.
		/// </summary>
		/// <exception cref="OpenIdException">Thrown when the signature is missing or the query has been tampered with.</exception>
		void verifySignature(IDictionary<string, string> query, Association assoc) {
			string sig = getRequiredField(query, QueryStringArgs.openid.sig);
			string signed = getRequiredField(query, QueryStringArgs.openid.signed);
			string[] signed_array = signed.Split(',');

			string v_sig = CryptUtil.ToBase64String(assoc.Sign(query, signed_array, QueryStringArgs.openid.Prefix));

			if (v_sig != sig)
				throw new OpenIdException(Strings.InvalidSignature);
		}

		Association getAssociation(Uri serverUrl)
		{
			Association assoc = store.GetAssociation(serverUrl);

			if (assoc == null || assoc.SecondsTillExpiration < minimumUsefulAssociationLifetime.TotalSeconds)
			{
				var req = AssociateRequest.Create(serverUrl);
				if (req.Response != null) {
					assoc = req.Response.Association;
					store.StoreAssociation(serverUrl, assoc);
				}
			}

			return assoc;
		}

	}
}
