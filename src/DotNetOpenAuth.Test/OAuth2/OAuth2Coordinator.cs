//-----------------------------------------------------------------------
// <copyright file="OAuth2Coordinator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.Test.Mocks;

	internal class OAuth2Coordinator<TClient> : CoordinatorBase<TClient, AuthorizationServer>
		where TClient : ClientBase {
		private readonly AuthorizationServerDescription serverDescription;
		private readonly IAuthorizationServerHost authServerHost;
		private readonly TClient client;

		internal OAuth2Coordinator(
			AuthorizationServerDescription serverDescription,
			IAuthorizationServerHost authServerHost,
			TClient client,
			Action<TClient> clientAction,
			Action<AuthorizationServer> authServerAction)
			: base(clientAction, authServerAction) {
			Requires.NotNull(serverDescription, "serverDescription");
			Requires.NotNull(authServerHost, "authServerHost");
			Requires.NotNull(client, "client");

			this.serverDescription = serverDescription;
			this.authServerHost = authServerHost;
			this.client = client;

			this.client.ClientIdentifier = OAuth2TestBase.ClientId;
			this.client.ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(OAuth2TestBase.ClientSecret);
		}

		internal override void Run() {
			var authServer = new AuthorizationServer(this.authServerHost);

			var rpCoordinatingChannel = new CoordinatingOAuth2ClientChannel(this.client.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			var opCoordinatingChannel = new CoordinatingOAuth2AuthServerChannel(authServer.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			rpCoordinatingChannel.RemoteChannel = opCoordinatingChannel;
			opCoordinatingChannel.RemoteChannel = rpCoordinatingChannel;

			this.client.Channel = rpCoordinatingChannel;
			authServer.Channel = opCoordinatingChannel;

			this.RunCore(this.client, authServer);
		}

		private static Action<WebServerClient> WrapAction(Action<WebServerClient> action) {
			Requires.NotNull(action, "action");

			return client => {
				action(client);
				((CoordinatingChannel)client.Channel).Close();
			};
		}

		private static Action<AuthorizationServer> WrapAction(Action<AuthorizationServer> action) {
			Requires.NotNull(action, "action");

			return authServer => {
				action(authServer);
				((CoordinatingChannel)authServer.Channel).Close();
			};
		}
	}
}
