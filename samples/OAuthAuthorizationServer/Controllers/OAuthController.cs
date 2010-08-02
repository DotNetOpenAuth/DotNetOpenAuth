namespace OAuthAuthorizationServer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography;
	using System.Web;
	using System.Web.Mvc;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	using OAuthAuthorizationServer.Code;
	using OAuthAuthorizationServer.Models;

	public class OAuthController : Controller {
		private readonly AuthorizationServer authorizationServer = new AuthorizationServer(new OAuth2AuthorizationServer());

#if SAMPLESONLY
		/// <summary>
		/// This is the FOR SAMPLE ONLY hard-coded public key of the complementary OAuthResourceServer sample.
		/// </summary>
		/// <remarks>
		/// In a real app, the authorization server would need to determine which resource server the access token needs to be encoded for
		/// based on the authorization request.  It would then need to look up the public key for that resource server and use that in 
		/// preparing the access token for the client to use against that resource server.
		/// </remarks>
		private static readonly RSAParameters ResourceServerEncryptionPublicKey = new RSAParameters {
			Exponent = new byte[] { 1, 0, 1 },
			Modulus = new byte[] { 166, 175, 117, 169, 211, 251, 45, 215, 55, 53, 202, 65, 153, 155, 92, 219, 235, 243, 61, 170, 101, 250, 221, 214, 239, 175, 238, 175, 239, 20, 144, 72, 227, 221, 4, 219, 32, 225, 101, 96, 18, 33, 117, 176, 110, 123, 109, 23, 29, 85, 93, 50, 129, 163, 113, 57, 122, 212, 141, 145, 17, 31, 67, 165, 181, 91, 117, 23, 138, 251, 198, 132, 188, 213, 10, 157, 116, 229, 48, 168, 8, 127, 28, 156, 239, 124, 117, 36, 232, 100, 222, 23, 52, 186, 239, 5, 63, 207, 185, 16, 137, 73, 137, 147, 252, 71, 9, 239, 113, 27, 88, 255, 91, 56, 192, 142, 210, 21, 34, 81, 204, 239, 57, 60, 140, 249, 15, 101 },
		};
#else
		[Obsolete("You must use a real key for a real app.", true)]
		private static readonly RSAParameters ResourceServerEncryptionPublicKey = new RSAParameters();
#endif

		/// <summary>
		/// The OAuth 2.0 token endpoint.
		/// </summary>
		/// <returns>The response to the Client.</returns>
		public ActionResult Token() {
			var request = this.authorizationServer.ReadAccessTokenRequest();
			if (request != null) {
				// Just for the sake of the sample, we use a short-lived token.  This can be useful to mitigate the security risks
				// of access tokens that are used over standard HTTP.
				// But this is just the lifetime of the access token.  The client can still renew it using their refresh token until
				// the authorization itself expires.
				TimeSpan accessTokenLifetime = TimeSpan.FromMinutes(2);

				// Also take into account the remaining life of the authorization and artificially shorten the access token's lifetime
				// to account for that if necessary.
				// TODO: code here

				// Prepare the refresh and access tokens.
				var response = this.authorizationServer.PrepareAccessTokenResponse(request, ResourceServerEncryptionPublicKey, accessTokenLifetime);
				return this.authorizationServer.Channel.PrepareResponse(response).AsActionResult();
			}

			throw new HttpException((int)HttpStatusCode.BadRequest, "Missing OAuth 2.0 request message.");
		}

		/// <summary>
		/// Prompts the user to authorize a client to access the user's private data.
		/// </summary>
		/// <returns>The browser HTML response that prompts the user to authorize the client.</returns>
		[Authorize, AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		public ActionResult Authorize() {
			var pendingRequest = this.authorizationServer.ReadAuthorizationRequest();
			if (pendingRequest == null) {
				throw new HttpException((int)HttpStatusCode.BadRequest, "Missing authorization request.");
			}

			var requestingClient = MvcApplication.DataContext.Clients.First(c => c.ClientIdentifier == pendingRequest.ClientIdentifier);

			// Consider auto-approving if safe to do so.
			if (((OAuth2AuthorizationServer)this.authorizationServer.AuthorizationServerServices).CanBeAutoApproved(pendingRequest)) {
				var approval = this.authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, HttpContext.User.Identity.Name);
				return this.authorizationServer.Channel.PrepareResponse(approval).AsActionResult();
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
		public ActionResult AuthorizeResponse(bool isApproved) {
			var pendingRequest = this.authorizationServer.ReadAuthorizationRequest();
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

				// In this simple sample, the user either agrees to the entire scope requested by the client or none of it.  
				// But in a real app, you could grant a reduced scope of access to the client by passing a scope parameter to this method.
				response = this.authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
			} else {
				response = this.authorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
			}

			return this.authorizationServer.Channel.PrepareResponse(response).AsActionResult();
		}
	}
}
