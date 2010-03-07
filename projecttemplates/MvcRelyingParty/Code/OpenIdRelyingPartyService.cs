namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Messaging;

	public interface IOpenIdRelyingParty {
		Channel Channel { get; }

		IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo);

		IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo);

		IAuthenticationResponse GetResponse();

		IAuthenticationResponse GetResponse(HttpRequestInfo request);
	}

	/// <summary>
	/// A wrapper around the standard <see cref="OpenIdRelyingParty"/> class.
	/// </summary>
	public class OpenIdRelyingPartyService : IOpenIdRelyingParty {
		/// <summary>
		/// The OpenID relying party to use for logging users in.
		/// </summary>
		/// <remarks>
		/// This is static because it is thread-safe and is more expensive
		/// to create than we want to run through for every single page request.
		/// </remarks>
		private static OpenIdRelyingParty relyingParty = new OpenIdRelyingParty();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyService"/> class.
		/// </summary>
		public OpenIdRelyingPartyService() {
		}

		#region IOpenIdRelyingParty Members

		public Channel Channel {
			get { return relyingParty.Channel; }
		}

		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo) {
			return relyingParty.CreateRequest(userSuppliedIdentifier, realm, returnTo);
		}

		public IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnTo) {
			return relyingParty.CreateRequests(userSuppliedIdentifier, realm, returnTo);
		}

		public IAuthenticationResponse GetResponse() {
			return relyingParty.GetResponse();
		}

		public IAuthenticationResponse GetResponse(HttpRequestInfo request) {
			return relyingParty.GetResponse(request);
		}

		#endregion
	}
}
