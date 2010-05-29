using DotNetOpenAuth.OAuth.ChannelElements;

namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuthWrap;

	internal class OAuth2AuthorizationServer : IAuthorizationServer {
		#region Implementation of IAuthorizationServer

		public IConsumerDescription GetClient(string clientIdentifier)
		{
			throw new NotImplementedException();
		}

		#endregion

		private class ConsumerDescription : IConsumerDescription {
			public string Key {
				get { throw new NotImplementedException(); }
			}

			public string Secret {
				get { throw new NotImplementedException(); }
			}

			public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate {
				get { throw new NotImplementedException(); }
			}

			public Uri Callback {
				get { throw new NotImplementedException(); }
			}

			public DotNetOpenAuth.OAuth.VerificationCodeFormat VerificationCodeFormat {
				get { throw new NotImplementedException(); }
			}

			public int VerificationCodeLength {
				get { throw new NotImplementedException(); }
			}
		}

	}
}