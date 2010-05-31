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
			this.Channel = new OAuthWrapAuthorizationServerChannel(authorizationServer);
		}

		public Channel Channel { get; set; }

		internal OAuthWrapAuthorizationServerChannel OAuthChannel {
			get { return (OAuthWrapAuthorizationServerChannel)this.Channel; }
		}

		public IAuthorizationServer AuthorizationServer { get; set; }
	}
}
