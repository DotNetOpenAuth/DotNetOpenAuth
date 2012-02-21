//-----------------------------------------------------------------------
// <copyright file="OAuth2Coordinator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.Test.Mocks;

	internal class OAuth2Coordinator : CoordinatorBase<WebServerClient, AuthorizationServer> {
		private readonly AuthorizationServerDescription serverDescription;
		private readonly IAuthorizationServer authServerHost;

		internal OAuth2Coordinator(AuthorizationServerDescription serverDescription, IAuthorizationServer authServerHost, Action<WebServerClient> clientAction, Action<AuthorizationServer> authServerAction)
			: base(clientAction, authServerAction) {
			Requires.NotNull(serverDescription, "serverDescription");
			Requires.NotNull(authServerHost, "authServerHost");
			this.serverDescription = serverDescription;
			this.authServerHost = authServerHost;
		}

		internal override void Run() {
			var client = new WebServerClient(this.serverDescription) {
				ClientIdentifier = OAuth2TestBase.ClientId,
				ClientSecret = OAuth2TestBase.ClientSecret,
			};
			var authServer = new AuthorizationServer(this.authServerHost);

			var rpCoordinatingChannel = new CoordinatingChannel(client.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			var opCoordinatingChannel = new CoordinatingOAuth2AuthServerChannel(authServer.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			rpCoordinatingChannel.RemoteChannel = opCoordinatingChannel;
			opCoordinatingChannel.RemoteChannel = rpCoordinatingChannel;

			client.Channel = rpCoordinatingChannel;
			authServer.Channel = opCoordinatingChannel;

			this.RunCore(client, authServer);
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
