//-----------------------------------------------------------------------
// <copyright file="ClientCredentialApplicator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	public abstract class ClientCredentialApplicator {
		protected ClientCredentialApplicator() {
		}

		public static ClientCredentialApplicator SecretParameter(string clientSecret) {
			Requires.NotNullOrEmpty(clientSecret, "clientSecret");
			return new SecretParameterApplicator(clientSecret);
		}

		public static ClientCredentialApplicator NetworkCredential(NetworkCredential credential) {
			Requires.NotNull(credential, "credential");
			return new NetworkCredentialApplicator(credential);
		}

		public static ClientCredentialApplicator NoSecret() {
			return null;
		}

		public virtual void ApplyClientCredential(string clientIdentifier, AuthenticatedClientRequestBase request) {
		}

		public virtual void ApplyClientCredential(string clientIdentifier, HttpWebRequest request) {
		}

		private class NetworkCredentialApplicator : ClientCredentialApplicator {
			private readonly NetworkCredential credential;

			internal NetworkCredentialApplicator(NetworkCredential credential) {
				Requires.NotNull(credential, "credential");
				this.credential = credential;
			}

			public override void ApplyClientCredential(string clientIdentifier, AuthenticatedClientRequestBase request) {
				// When using network credentials, the client authentication is not done as standard message parts.
				request.ClientIdentifier = null;
				request.ClientSecret = null;
				OAuthUtilities.ApplyHttpBasicAuth(request.Headers, clientIdentifier, this.credential.Password);
			}

			public override void ApplyClientCredential(string clientIdentifier, HttpWebRequest request) {
			}
		}

		private class SecretParameterApplicator : ClientCredentialApplicator {
			private readonly string secret;

			internal SecretParameterApplicator(string clientSecret) {
				Requires.NotNullOrEmpty(clientSecret, "clientSecret");
				this.secret = clientSecret;
			}

			public override void ApplyClientCredential(string clientIdentifier, AuthenticatedClientRequestBase request) {
				request.ClientSecret = this.secret;
			}

			public override void ApplyClientCredential(string clientIdentifier, HttpWebRequest request) {
			}
		}
	}
}
