using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuthWrap;

	internal class OAuth2AuthorizationServer : IAuthorizationServer {
		private static readonly byte[] secret = new byte[] { 0x33, 0x55 }; // TODO: make this cryptographically strong and unique per app.
		private readonly INonceStore nonceStore = new DatabaseNonceStore();
		#region Implementation of IAuthorizationServer

		public byte[] Secret {
			get { return secret; }
		}

		public DotNetOpenAuth.Messaging.Bindings.INonceStore VerificationCodeNonceStore {
			get { return this.nonceStore; }
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
	}
}