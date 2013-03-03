namespace OAuth2ProtectedWebApi.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using OAuth2ProtectedWebApi.Code;

	/// <summary>
	/// Provides application-specific policy and persistence for OAuth 2.0 authorization servers.
	/// </summary>
	public class AuthorizationServerHost : IAuthorizationServerHost {
		/// <summary>
		/// Storage for the cryptographic keys used to protect authorization codes, refresh tokens and access tokens.
		/// </summary>
		/// <remarks>
		/// A single, hard-coded symmetric key is hardly adequate. Applications that rely on decent security should
		/// replace this implementation with one that actually stores and retrieves keys in some persistent store
		/// (e.g. a database). DotNetOpenAuth will automatically take care of generating, rotating, and expiring keys
		/// if you provide a real implementation of this interface.
		/// TODO: Consider replacing use of <see cref="HardCodedKeyCryptoKeyStore"/> with a real persisted database table.
		/// </remarks>
		internal static readonly ICryptoKeyStore HardCodedCryptoKeyStore = new HardCodedKeyCryptoKeyStore("p7J1L24Qj4KGYUOrnfENF0XAhqn6rZc5dx4nxvI22Kg=");

		/// <summary>
		/// Gets the store for storing crypto keys used to symmetrically encrypt and sign authorization codes and refresh tokens.
		/// </summary>
		/// <remarks>
		/// This store should be kept strictly confidential in the authorization server(s)
		/// and NOT shared with the resource server.  Anyone with these secrets can mint
		/// tokens to essentially grant themselves access to anything they want.
		/// </remarks>
		public ICryptoKeyStore CryptoKeyStore {
			get { return HardCodedCryptoKeyStore; }
		}

		/// <summary>
		/// Gets the authorization code nonce store to use to ensure that authorization codes can only be used once.
		/// </summary>
		/// <value>
		/// The authorization code nonce store.
		/// </value>
		public INonceStore NonceStore {
			get {
				// TODO: Consider implementing a nonce store to mitigate replay attacks on authorization codes.
				return null;
			}
		}

		/// <summary>
		/// Acquires the access token and related parameters that go into the formulation of the token endpoint's response to a client.
		/// </summary>
		/// <param name="accessTokenRequestMessage">Details regarding the resources that the access token will grant access to, and the identity of the client
		/// that will receive that access.
		/// Based on this information the receiving resource server can be determined and the lifetime of the access
		/// token can be set based on the sensitivity of the resources.</param>
		/// <returns>
		/// A non-null parameters instance that DotNetOpenAuth will dispose after it has been used.
		/// </returns>
		public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage) {
			// If your resource server and authorization server are different web apps,
			// consider using asymmetric keys instead of symmetric ones by setting different
			// properties on the access token below.
			var accessToken = new AuthorizationServerAccessToken {
				Lifetime = TimeSpan.FromHours(1),
				SymmetricKeyStore = this.CryptoKeyStore,
			};
			var result = new AccessTokenResult(accessToken);
			return result;
		}

		/// <summary>
		/// Gets the client with a given identifier.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>
		/// The client registration.  Never null.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown when no client with the given identifier is registered with this authorization server.</exception>
		public IClientDescription GetClient(string clientIdentifier) {
			// TODO: Consider adding a clients table in your database to track actual client accounts
			//       with authenticating secrets.
			// For now, just allow all clients regardless of ID, and consider them "Public" clients.
			return new AnyCallbackClient();
		}

		/// <summary>
		/// Determines whether a described authorization is (still) valid.
		/// </summary>
		/// <param name="authorization">The authorization.</param>
		/// <returns>
		///   <c>true</c> if the original authorization is still valid; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		///   <para>When establishing that an authorization is still valid,
		/// it's very important to only match on recorded authorizations that
		/// meet these criteria:</para>
		/// 1) The client identifier matches.
		/// 2) The user account matches.
		/// 3) The scope on the recorded authorization must include all scopes in the given authorization.
		/// 4) The date the recorded authorization was issued must be <em>no later</em> that the date the given authorization was issued.
		///   <para>One possible scenario is where the user authorized a client, later revoked authorization,
		/// and even later reinstated authorization.  This subsequent recorded authorization
		/// would not satisfy requirement #4 in the above list.  This is important because the revocation
		/// the user went through should invalidate all previously issued tokens as a matter of
		/// security in the event the user was revoking access in order to sever authorization on a stolen
		/// account or piece of hardware in which the tokens were stored. </para>
		/// </remarks>
		public bool IsAuthorizationValid(IAuthorizationDescription authorization) {
			// If your application supports access revocation (highly recommended),
			// this method should return false if the specified authorization is not
			// discovered in your current authorizations table.
			//// TODO: code here

			return true;
		}

		/// <summary>
		/// Determines whether a given set of resource owner credentials is valid based on the authorization server's user database
		/// and if so records an authorization entry such that subsequent calls to <see cref="IsAuthorizationValid" /> would
		/// return <c>true</c>.
		/// </summary>
		/// <param name="userName">Username on the account.</param>
		/// <param name="password">The user's password.</param>
		/// <param name="accessRequest">The access request the credentials came with.
		/// This may be useful if the authorization server wishes to apply some policy based on the client that is making the request.</param>
		/// <returns>
		/// A value that describes the result of the authorization check.
		/// </returns>
		/// <exception cref="System.NotSupportedException"></exception>
		public AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest) {
			// TODO: Consider only accepting resource owner credential grants from specific clients
			//       based on accessRequest.ClientIdentifier and accessRequest.ClientAuthenticated.
			if (Membership.ValidateUser(userName, password)) {
				// Add an entry to your authorization table to record that access was granted so that
				// you can conditionally return true from IsAuthorizationValid when the row is discovered.
				//// TODO: code here

				// Inform DotNetOpenAuth that it may proceed to issue an access token.
				return new AutomatedUserAuthorizationCheckResponse(accessRequest, true, Membership.GetUser(userName).UserName);
			} else {
				return new AutomatedUserAuthorizationCheckResponse(accessRequest, false, null);
			}
		}

		/// <summary>
		/// Determines whether an access token request given a client credential grant should be authorized
		/// and if so records an authorization entry such that subsequent calls to <see cref="IsAuthorizationValid" /> would
		/// return <c>true</c>.
		/// </summary>
		/// <param name="accessRequest">The access request the credentials came with.
		/// This may be useful if the authorization server wishes to apply some policy based on the client that is making the request.</param>
		/// <returns>
		/// A value that describes the result of the authorization check.
		/// </returns>
		/// <exception cref="System.NotSupportedException"></exception>
		public AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest) {
			// TODO: Consider implementing this if your application should support clients that access data that
			//       doesn't belong to specific people, or clients that have elevated privileges and can access other
			//       people's data.
			if (accessRequest.ClientAuthenticated) {
				// Before returning a positive response, be *very careful* to validate the requested access scope
				// to make sure it is appropriate for the requesting client.
				throw new NotSupportedException();
			} else {
				// Only authenticated clients should be given access.
				return new AutomatedAuthorizationCheckResponse(accessRequest, false);
			}
		}

		private class AnyCallbackClient : ClientDescription {
			public override bool IsCallbackAllowed(Uri callback) {
				return true;
			}
		}
	}
}