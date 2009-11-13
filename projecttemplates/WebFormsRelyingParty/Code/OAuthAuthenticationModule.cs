//-----------------------------------------------------------------------
// <copyright file="OAuthAuthenticationModule.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Principal;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public class OAuthAuthenticationModule : IHttpModule {
		private HttpApplication application;

		#region IHttpModule Members

		/// <summary>
		/// Initializes a module and prepares it to handle requests.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
		public void Init(HttpApplication context) {
			this.application = context;
			this.application.AuthenticateRequest += this.context_AuthenticateRequest;
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
		/// </summary>
		public void Dispose() {
		}

		/// <summary>
		/// Handles the AuthenticateRequest event of the HttpApplication.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void context_AuthenticateRequest(object sender, EventArgs e) {
			// Don't read OAuth messages directed at the OAuth controller or else we'll fail nonce checks.
			if (this.IsOAuthControllerRequest()) {
				return;
			}

			IDirectedProtocolMessage incomingMessage = OAuthServiceProvider.ServiceProvider.ReadRequest(new HttpRequestInfo(this.application.Context.Request));
			var authorization = incomingMessage as AccessProtectedResourceRequest;
			if (authorization != null) {
				this.application.Context.User = OAuthServiceProvider.ServiceProvider.CreatePrincipal(authorization);
			}
		}

		#endregion

		private bool IsOAuthControllerRequest() {
			return string.Equals(this.application.Context.Request.Url.AbsolutePath, "/OAuth.ashx", StringComparison.OrdinalIgnoreCase);
		}
	}
}
