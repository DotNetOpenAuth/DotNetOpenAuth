//-----------------------------------------------------------------------
// <copyright file="AuthTicketRoles.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OpenIdWebRingSsoRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Security;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An authentication module that utilizes the forms auth ticket cookie
	/// as a cache for the users' roles, since those roles are determined by
	/// the OpenID Provider and we don't have a local user-roles cache at this
	/// RP since those relationships are always managed by the Provider.
	/// </summary>
	public class AuthTicketRoles : IHttpModule {
		#region IHttpModule Members

		/// <summary>
		/// Initializes a module and prepares it to handle requests.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
		public void Init(HttpApplication context) {
			context.AuthenticateRequest += this.application_AuthenticateRequest;
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
		/// </summary>
		public void Dispose() {
		}

		#endregion

		private void application_AuthenticateRequest(object sender, EventArgs e) {
			if (HttpContext.Current.User != null) {
				var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
				if (cookie != null) {
					var ticket = FormsAuthentication.Decrypt(cookie.Value);
					if (!string.IsNullOrEmpty(ticket.UserData)) {
						string[] roles = ticket.UserData.Split(';');
						HttpContext.Current.User = new GenericPrincipal(HttpContext.Current.User.Identity, roles);
					}
				}
			}
		}
	}
}
