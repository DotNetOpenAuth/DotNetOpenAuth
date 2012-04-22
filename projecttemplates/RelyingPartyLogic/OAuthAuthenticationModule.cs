//-----------------------------------------------------------------------
// <copyright file="OAuthAuthenticationModule.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Security;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

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

			// Register an event that allows us to override roles for OAuth requests.
			var roleManager = (RoleManagerModule)this.application.Modules["RoleManager"];
			roleManager.GetRoles += this.roleManager_GetRoles;
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

			using (var crypto = OAuthResourceServer.CreateRSA()) {
				var tokenAnalyzer = new SpecialAccessTokenAnalyzer(crypto, crypto);
				var resourceServer = new ResourceServer(tokenAnalyzer);

				try {
					IPrincipal principal = resourceServer.GetPrincipal(new HttpRequestWrapper(this.application.Context.Request));
					this.application.Context.User = principal;
				} catch (ProtocolFaultResponseException ex) {
					ex.CreateErrorResponse().Send();
				}
			}
		}

		#endregion

		private bool IsOAuthControllerRequest() {
			return string.Equals(this.application.Context.Request.Url.AbsolutePath, "/OAuth.ashx", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Handles the GetRoles event of the roleManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Web.Security.RoleManagerEventArgs"/> instance containing the event data.</param>
		private void roleManager_GetRoles(object sender, RoleManagerEventArgs e) {
			if (this.application.User is DotNetOpenAuth.OAuth.ChannelElements.OAuthPrincipal) {
				e.RolesPopulated = true;
			}
		}
	}
}
