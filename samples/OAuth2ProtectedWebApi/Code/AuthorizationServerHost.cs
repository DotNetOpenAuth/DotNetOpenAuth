namespace OAuth2ProtectedWebApi {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using OAuth2ProtectedWebApi.Code;

	public class AuthorizationServerHost : IAuthorizationServerHost {
		private static ICryptoKeyStore cryptoKeyStore = MemoryCryptoKeyStore.Instance;

		public ICryptoKeyStore CryptoKeyStore {
			get { return cryptoKeyStore; }
		}

		public INonceStore NonceStore {
			get {
				// Implementing a nonce store is a good idea as it mitigates replay attacks.
				return null;
			}
		}

		public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage) {
			var accessToken = new AuthorizationServerAccessToken();
			accessToken.Lifetime = TimeSpan.FromHours(1);
			accessToken.SymmetricKeyStore = this.CryptoKeyStore;
			var result = new AccessTokenResult(accessToken);
			return result;
		}

		public IClientDescription GetClient(string clientIdentifier) {
			return new ClientDescription("b", new Uri("http://www.microsoft.com/en-us/default.aspx"), ClientType.Confidential);
		}

		public bool IsAuthorizationValid(IAuthorizationDescription authorization) {
			return true;
		}

		public AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest) {
			throw new NotSupportedException();
		}

		public AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest) {
			throw new NotSupportedException();
		}
	}
}