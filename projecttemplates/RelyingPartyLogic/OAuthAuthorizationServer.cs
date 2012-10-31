//-----------------------------------------------------------------------
// <copyright file="OAuthAuthorizationServer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Provides OAuth 2.0 authorization server information to DotNetOpenAuth.
	/// </summary>
	public class OAuthAuthorizationServer : IAuthorizationServerHost {
		private static readonly RSACryptoServiceProvider SigningKey = new RSACryptoServiceProvider();

		private readonly INonceStore nonceStore = new NonceDbStore();

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthAuthorizationServer"/> class.
		/// </summary>
		public OAuthAuthorizationServer() {
			this.CryptoKeyStore = new RelyingPartyApplicationDbStore();
		}

		#region IAuthorizationServerHost Members

		public ICryptoKeyStore CryptoKeyStore { get; private set; }

		/// <summary>
		/// Gets the authorization code nonce store to use to ensure that authorization codes can only be used once.
		/// </summary>
		/// <value>The authorization code nonce store.</value>
		public INonceStore NonceStore {
			get { return this.nonceStore; }
		}

		/// <summary>
		/// Gets the crypto service provider with the asymmetric private key to use for signing access tokens.
		/// </summary>
		/// <value>
		/// Must not be null, and must contain the private key.
		/// </value>
		/// <returns>A crypto service provider instance that contains the private key.</returns>
		public RSACryptoServiceProvider AccessTokenSigningKey {
			get { return SigningKey; }
		}

		/// <summary>
		/// Obtains parameters to go into the formulation of an access token.
		/// </summary>
		/// <param name="accessTokenRequestMessage">Details regarding the resources that the access token will grant access to, and the identity of the client
		/// that will receive that access.
		/// Based on this information the receiving resource server can be determined and the lifetime of the access
		/// token can be set based on the sensitivity of the resources.</param>
		/// <returns>
		/// A non-null parameters instance that DotNetOpenAuth will dispose after it has been used.
		/// </returns>
		public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage) {
			var accessToken = new AuthorizationServerAccessToken() {
				// For this sample, we assume just one resource server.
				// If this authorization server needs to mint access tokens for more than one resource server,
				// we'd look at the request message passed to us and decide which public key to return.
				ResourceServerEncryptionKey = OAuthResourceServer.CreateRSA(),
			};

			var result = new AccessTokenResult(accessToken);
			return result;
		}

		/// <summary>
		/// Gets the client with a given identifier.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client registration.  Never null.</returns>
		/// <exception cref="ArgumentException">Thrown when no client with the given identifier is registered with this authorization server.</exception>
		public IClientDescription GetClient(string clientIdentifier) {
			try {
				return Database.DataContext.Clients.First(c => c.ClientIdentifier == clientIdentifier);
			} catch (InvalidOperationException ex) {
				throw new ArgumentOutOfRangeException("No client by that identifier.", ex);
			}
		}

		/// <summary>
		/// Determines whether a described authorization is (still) valid.
		/// </summary>
		/// <param name="authorization">The authorization.</param>
		/// <returns>
		/// 	<c>true</c> if the original authorization is still valid; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// 	<para>When establishing that an authorization is still valid,
		/// it's very important to only match on recorded authorizations that
		/// meet these criteria:</para>
		/// 1) The client identifier matches.
		/// 2) The user account matches.
		/// 3) The scope on the recorded authorization must include all scopes in the given authorization.
		/// 4) The date the recorded authorization was issued must be <em>no later</em> that the date the given authorization was issued.
		/// <para>One possible scenario is where the user authorized a client, later revoked authorization,
		/// and even later reinstated authorization.  This subsequent recorded authorization
		/// would not satisfy requirement #4 in the above list.  This is important because the revocation
		/// the user went through should invalidate all previously issued tokens as a matter of
		/// security in the event the user was revoking access in order to sever authorization on a stolen
		/// account or piece of hardware in which the tokens were stored. </para>
		/// </remarks>
		public bool IsAuthorizationValid(IAuthorizationDescription authorization) {
			return this.IsAuthorizationValid(authorization.Scope, authorization.ClientIdentifier, authorization.UtcIssued, authorization.User);
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
		public AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest) {
			// This web site delegates user authentication to OpenID Providers, and as such no users have local passwords with this server.
			throw new NotSupportedException();
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
		public AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest) {
			throw new NotImplementedException();
		}

		#endregion

		public bool CanBeAutoApproved(EndUserAuthorizationRequest authorizationRequest) {
			if (authorizationRequest == null) {
				throw new ArgumentNullException("authorizationRequest");
			}

			// NEVER issue an auto-approval to a client that would end up getting an access token immediately
			// (without a client secret), as that would allow ANY client to spoof an approved client's identity
			// and obtain unauthorized access to user data.
			if (authorizationRequest.ResponseType == EndUserAuthorizationResponseType.AuthorizationCode) {
				// Never issue auto-approval if the client secret is blank, since that too makes it easy to spoof
				// a client's identity and obtain unauthorized access.
				var requestingClient = Database.DataContext.Clients.First(c => c.ClientIdentifier == authorizationRequest.ClientIdentifier);
				if (!string.IsNullOrEmpty(requestingClient.ClientSecret)) {
					return this.IsAuthorizationValid(
						authorizationRequest.Scope,
						authorizationRequest.ClientIdentifier,
						DateTime.UtcNow,
						HttpContext.Current.User.Identity.Name);
				}
			}

			// Default to not auto-approving.
			return false;
		}

		private bool IsAuthorizationValid(HashSet<string> requestedScopes, string clientIdentifier, DateTime issuedUtc, string username) {
			var grantedScopeStrings = from auth in Database.DataContext.ClientAuthorizations
									  where
										auth.Client.ClientIdentifier == clientIdentifier &&
										auth.CreatedOnUtc <= issuedUtc &&
										(!auth.ExpirationDateUtc.HasValue || auth.ExpirationDateUtc.Value >= DateTime.UtcNow) &&
										auth.User.AuthenticationTokens.Any(token => token.ClaimedIdentifier == username)
										select auth.Scope;

			if (!grantedScopeStrings.Any()) {
				// No granted authorizations prior to the issuance of this token, so it must have been revoked.
				// Even if later authorizations restore this client's ability to call in, we can't allow
				// access tokens issued before the re-authorization because the revoked authorization should
				// effectively and permanently revoke all access and refresh tokens.
				return false;
			}

			var grantedScopes = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
			foreach (string scope in grantedScopeStrings) {
				grantedScopes.UnionWith(OAuthUtilities.SplitScopes(scope));
			}

			return requestedScopes.IsSubsetOf(grantedScopes);
		}
	}
}
