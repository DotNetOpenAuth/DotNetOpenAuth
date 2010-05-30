//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using ChannelElements;
	using DotNetOpenAuth.Messaging;
	using OAuth.ChannelElements;

	public abstract class AuthorizationServerBase {
		protected AuthorizationServerBase(IAuthorizationServer authorizationServer) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
			this.AuthorizationServer = authorizationServer;
		}

		public Channel Channel { get; set; }

		internal OAuthWrapAuthorizationServerChannel OAuthChannel {
			get { return (OAuthWrapAuthorizationServerChannel)this.Channel; }
		}

		public IAuthorizationServer AuthorizationServer { get; set; }

		protected IConsumerDescription GetClient(string clientIdentifier) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Ensures(Contract.Result<IConsumerDescription>() != null);

			try {
				return this.AuthorizationServer.GetClient(clientIdentifier);
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, OAuth.OAuthStrings.ConsumerOrTokenSecretNotFound);
			}
		}
	}
}
