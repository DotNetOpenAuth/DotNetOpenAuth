//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.Contracts;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	public abstract class AuthorizationServerBase {
		protected AuthorizationServerBase(IAuthorizationServer authorizationServer) {
			Contract.Requires<ArgumentNullException>(authorizationServer != null, "authorizationServer");
			this.AuthorizationServer = authorizationServer;
		}

		public Channel Channel { get; set; }

		public IAuthorizationServer AuthorizationServer { get; set; }

		protected IConsumerDescription GetClient(string clientIdentifier) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Ensures(Contract.Result<IConsumerDescription>() != null);

			try {
				return this.AuthorizationServer.GetClient(clientIdentifier);
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, DotNetOpenAuth.OAuth.OAuthStrings.ConsumerOrTokenSecretNotFound);
			}
		}
	}
}
