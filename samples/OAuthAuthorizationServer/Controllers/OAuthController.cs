namespace OAuthAuthorizationServer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using OAuthAuthorizationServer.Code;
	using OAuthAuthorizationServer.Models;

	public class OAuthController : Controller {
		private readonly AuthorizationServer authorizationServer = new AuthorizationServer(new OAuth2AuthorizationServer());

		/// <summary>
		/// The OAuth 2.0 token endpoint.
		/// </summary>
		/// <returns>The response to the Client.</returns>
		public async Task<ActionResult> Token() {
			var request = await this.authorizationServer.HandleTokenRequestAsync(this.Request, this.Response.ClientDisconnectedToken);
			return request.AsActionResult();
		}

		/// <summary>
		/// Prompts the user to authorize a client to access the user's private data.
		/// </summary>
		/// <returns>The browser HTML response that prompts the user to authorize the client.</returns>
		[Authorize, AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		[HttpHeader("x-frame-options", "SAMEORIGIN")] // mitigates clickjacking
		public async Task<ActionResult> Authorize() {
			var pendingRequest = await this.authorizationServer.ReadAuthorizationRequestAsync(Request, Response.ClientDisconnectedToken);
			if (pendingRequest == null) {
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			var requestingClient = MvcApplication.DataContext.Clients.First(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);

			// Consider auto-approving if safe to do so.
			if (((OAuth2AuthorizationServer)this.authorizationServer.AuthorizationServerServices).CanBeAutoApproved(pendingRequest)) {
				var approval = this.authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, HttpContext.User.Identity.Name);
				var response = await this.authorizationServer.Channel.PrepareResponseAsync(approval, Response.ClientDisconnectedToken);
				return response.AsActionResult();
			}

			var model = new AccountAuthorizeModel {
				ClientApp = requestingClient.Name,
				Scope = pendingRequest.Scope,
				AuthorizationRequest = pendingRequest,
			};

			return View(model);
		}

		/// <summary>
		/// Processes the user's response as to whether to authorize a Client to access his/her private data.
		/// </summary>
		/// <param name="isApproved">if set to <c>true</c>, the user has authorized the Client; <c>false</c> otherwise.</param>
		/// <returns>HTML response that redirects the browser to the Client.</returns>
		[Authorize, HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> AuthorizeResponse(bool isApproved) {
			var pendingRequest = await this.authorizationServer.ReadAuthorizationRequestAsync(Request, Response.ClientDisconnectedToken);
			if (pendingRequest == null) {
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			IDirectedProtocolMessage response;
			if (isApproved) {
				// The authorization we file in our database lasts until the user explicitly revokes it.
				// You can cause the authorization to expire by setting the ExpirationDateUTC
				// property in the below created ClientAuthorization.
				var client = MvcApplication.DataContext.Clients.First(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);
				client.ClientAuthorizations.Add(
					new ClientAuthorization {
						Scope = OAuthUtilities.JoinScopes(pendingRequest.Scope),
						User = MvcApplication.LoggedInUser,
						CreatedOnUtc = DateTime.UtcNow,
					});
				MvcApplication.DataContext.SubmitChanges(); // submit now so that this new row can be retrieved later in this same HTTP request

				// In this simple sample, the user either agrees to the entire scope requested by the client or none of it.  
				// But in a real app, you could grant a reduced scope of access to the client by passing a scope parameter to this method.
				response = this.authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
			} else {
				response = this.authorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
			}

			var preparedResponse = await this.authorizationServer.Channel.PrepareResponseAsync(response, Response.ClientDisconnectedToken);
			return preparedResponse.AsActionResult();
		}
	}
}
