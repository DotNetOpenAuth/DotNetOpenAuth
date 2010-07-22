namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;

	internal class OAuth2AuthorizationServer : IAuthorizationServer {
		internal static readonly RSAParameters AsymmetricKey;

		private static readonly byte[] secret;

		private readonly INonceStore nonceStore = new DatabaseNonceStore();

		static OAuth2AuthorizationServer() {
			// For this sample, we just generate random secrets.
			RandomNumberGenerator crypto = new RNGCryptoServiceProvider();
			secret = new byte[16];
			crypto.GetBytes(secret);

			AsymmetricKey = new RSACryptoServiceProvider().ExportParameters(true);
		}

		#region Implementation of IAuthorizationServer

		public byte[] Secret {
			get { return secret; }
		}

		public INonceStore VerificationCodeNonceStore {
			get { return this.nonceStore; }
		}

		public RSAParameters AccessTokenSigningPrivateKey {
			get { return AsymmetricKey; }
		}

		public IConsumerDescription GetClient(string clientIdentifier) {
			var consumerRow = Global.DataContext.Clients.SingleOrDefault(
				consumerCandidate => consumerCandidate.ClientIdentifier == clientIdentifier);
			if (consumerRow == null) {
				throw new ArgumentOutOfRangeException("clientIdentifier");
			}

			return consumerRow;
		}

		#endregion

		public bool IsAuthorizationValid(IAuthorizationDescription authorization) {
			return this.IsAuthorizationValid(authorization.Scope, authorization.ClientIdentifier, authorization.UtcIssued, authorization.User);
		}

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
				var requestingClient = Global.DataContext.Clients.First(c => c.ClientIdentifier == authorizationRequest.ClientIdentifier);
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
			var grantedScopeStrings = from auth in Global.DataContext.ClientAuthorizations
									  where
										auth.Client.ClientIdentifier == clientIdentifier &&
										auth.CreatedOnUtc <= issuedUtc &&
										(!auth.ExpirationDateUtc.HasValue || auth.ExpirationDateUtc.Value >= DateTime.UtcNow) &&
										auth.User.OpenIDClaimedIdentifier == username
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