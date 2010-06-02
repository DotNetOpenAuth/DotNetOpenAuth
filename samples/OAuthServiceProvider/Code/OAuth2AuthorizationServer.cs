namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuthWrap;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	internal class OAuth2AuthorizationServer : IAuthorizationServer {
		private static readonly byte[] secret;

		private static readonly RSAParameters asymmetricKey;

		private readonly INonceStore nonceStore = new DatabaseNonceStore();

		static OAuth2AuthorizationServer() {
			// For this sample, we just generate random secrets.
			RandomNumberGenerator crypto = new RNGCryptoServiceProvider();
			secret = new byte[16];
			crypto.GetBytes(secret);

			asymmetricKey = new RSACryptoServiceProvider().ExportParameters(true);
		}

		#region Implementation of IAuthorizationServer

		public byte[] Secret {
			get { return secret; }
		}

		public DotNetOpenAuth.Messaging.Bindings.INonceStore VerificationCodeNonceStore {
			get { return this.nonceStore; }
		}

		public RSAParameters? AccessTokenSigningPrivateKey {
			get { return asymmetricKey; }
		}

		public IConsumerDescription GetClient(string clientIdentifier) {
			var consumerRow = Global.DataContext.OAuthConsumers.SingleOrDefault(
				consumerCandidate => consumerCandidate.ConsumerKey == clientIdentifier);
			if (consumerRow == null) {
				throw new ArgumentOutOfRangeException("clientIdentifier");
			}

			return consumerRow;
		}

		#endregion

		public bool IsAuthorizationValid(IAuthorizationDescription authorization) {
			// We don't support revoking tokens yet.
			return true;
		}
	}
}