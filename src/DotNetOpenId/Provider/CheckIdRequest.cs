using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to confirm the identity of a user.
	/// </summary>
	/// <remarks>
	/// This class handles requests for openid modes checkid_immediate and checkid_setup.
	/// </remarks>
	class CheckIdRequest : AssociatedRequest, IAuthenticationRequest {
		bool? isAuthenticated;
		/// <summary>
		/// Gets/sets whether the provider has determined that the 
		/// <see cref="ClaimedIdentifier"/> belongs to the currently logged in user
		/// and wishes to share this information with the consumer.
		/// </summary>
		public bool? IsAuthenticated {
			get { return isAuthenticated; }
			set {
				isAuthenticated = value;
				InvalidateResponse();
			}
		}
		Uri serverUrl = tryGetServerUrl();
		/// <summary>
		/// The provider URL that responds to OpenID requests.
		/// </summary>
		/// <remarks>
		/// An auto-detect attempt is made if an ASP.NET HttpContext is available.
		/// </remarks>
		public Uri ProviderEndpoint {
			get { return serverUrl; }
			set {
				serverUrl = value;
				InvalidateResponse();
			}
		}

		/// <summary>
		/// Whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		public bool Immediate { get; private set; }
		/// <summary>
		/// The URL the consumer site claims to use as its 'base' address.
		/// </summary>
		public Realm Realm { get; private set; }
		/// <summary>
		/// The claimed OpenId URL of the user attempting to authenticate.
		/// </summary>
		public Identifier ClaimedIdentifier { get; private set; }
		/// <summary>
		/// The URL to redirect the user agent to after the authentication attempt.
		/// This must fall "under" the realm URL.
		/// </summary>
		internal Uri ReturnTo { get; private set; }
		internal override string Mode {
			get { return Immediate ? Protocol.Args.Mode.checkid_immediate : Protocol.Args.Mode.checkid_setup; }
		}
		/// <summary>
		/// Indicates whether this request has all the information necessary to formulate a response.
		/// </summary>
		public override bool IsResponseReady {
			get { return IsAuthenticated.HasValue && ProviderEndpoint != null; }
		}
		/// <summary>
		/// Get the URL to cancel this request.
		/// </summary>
		internal Uri CancelUrl {
			get {
				if (Immediate)
					throw new InvalidOperationException("Cancel is not an appropriate response to immediate mode requests.");

				UriBuilder builder = new UriBuilder(ReturnTo);
				var args = new Dictionary<string, string>();
				args.Add(Protocol.openid.mode, Protocol.Args.Mode.cancel);
				UriUtil.AppendQueryArgs(builder, args);

				return builder.Uri;
			}
		}

		internal CheckIdRequest(OpenIdProvider server) : base(server) {
			// handle the mandatory protocol fields
			string mode = Util.GetRequiredArg(Query, Protocol.openid.mode);
			if (Protocol.Args.Mode.checkid_immediate.Equals(mode, StringComparison.Ordinal)) {
				Immediate = true;
			} else if (Protocol.Args.Mode.checkid_setup.Equals(mode, StringComparison.Ordinal)) {
				Immediate = false; // implied
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue, Protocol.openid.mode, mode), Query);
			}

			try {
				ClaimedIdentifier = Util.GetRequiredArg(Query, Protocol.openid.identity);
			} catch (UriFormatException) {
				throw new OpenIdException(Protocol.openid.identity + " not a valid url: " + Util.GetRequiredArg(Query, Protocol.openid.identity), Query);
			}

			try {
				ReturnTo = new Uri(Util.GetRequiredArg(Query, Protocol.openid.return_to));
			} catch (UriFormatException ex) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture, 
					"'{0}' is not a valid OpenID return_to URL.", Util.GetRequiredArg(Query, Protocol.openid.return_to)),
					ClaimedIdentifier, Query, ex);
			}

			try {
				Realm = new Realm(Util.GetOptionalArg(Query, Protocol.openid.Realm) ?? ReturnTo.AbsoluteUri);
			} catch (UriFormatException ex) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue, Protocol.openid.Realm,
					Util.GetOptionalArg(Query, Protocol.openid.Realm)), ex);
			}
			AssociationHandle = Util.GetOptionalArg(Query, Protocol.openid.assoc_handle);

			if (!Realm.Contains(ReturnTo)) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ReturnToNotUnderRealm, ReturnTo.AbsoluteUri, Realm), Query);
			}
		}

		internal override IEncodable CreateResponse() {
			string mode = (IsAuthenticated.Value || Immediate) ?
				Protocol.Args.Mode.id_res : Protocol.Args.Mode.cancel;

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start processing Response for CheckIdRequest");
				if (TraceUtil.Switch.TraceVerbose) {
					Trace.TraceInformation("mode = '{0}',  server_url = '{1}", mode, ProviderEndpoint);
				}
			}

			EncodableResponse response = new EncodableResponse(this);

			// Always send the openid.mode, and sign it only if authentication succeeded.
			response.AddField(null, Protocol.openidnp.mode, mode, IsAuthenticated.Value);

			if (IsAuthenticated.Value) {
				// Add additional signed fields
				var fields = new Dictionary<string, string>();
				fields.Add(Protocol.openidnp.identity, ClaimedIdentifier.ToString());
				fields.Add(Protocol.openidnp.return_to, ReturnTo.AbsoluteUri);
				response.AddFields(null, fields, true);
			}
			if (Immediate && !IsAuthenticated.Value) {
				response.AddField(null, Protocol.openidnp.user_setup_url, SetupUrl.AbsoluteUri, false);
			}

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("CheckIdRequest response successfully created. ");
				if (TraceUtil.Switch.TraceVerbose) {
					Trace.TraceInformation("Response follows: {0}", response);
				}
			}

			return response;
		}

		static Uri tryGetServerUrl() {
			if (HttpContext.Current == null) return null;
			UriBuilder builder = new UriBuilder(HttpContext.Current.Request.Url);
			builder.Query = null;
			builder.Fragment = null;
			return builder.Uri;
		}

		/// <summary>
		/// Encode this request as a URL to GET.
		/// </summary>
		internal Uri SetupUrl {
			get {
				if (ProviderEndpoint == null) {
					throw new InvalidOperationException("ProviderEndpoint is required for failed authentication in immediate mode.");
				}

				var q = new Dictionary<string, string>();

				q.Add(Protocol.openid.mode, Protocol.Args.Mode.checkid_setup);
				q.Add(Protocol.openid.identity, ClaimedIdentifier.ToString());
				q.Add(Protocol.openid.return_to, ReturnTo.AbsoluteUri);

				if (Realm != null)
					q.Add(Protocol.openid.Realm, Realm);

				if (this.AssociationHandle != null)
					q.Add(Protocol.openid.assoc_handle, this.AssociationHandle);

				UriBuilder builder = new UriBuilder(ProviderEndpoint);
				UriUtil.AppendQueryArgs(builder, q);

				return builder.Uri;
			}
		}

		public override string ToString() {
			string returnString = @"
CheckIdRequest.Immediate = '{0}'
CheckIdRequest.Realm = '{1}'
CheckIdRequest.Identity = '{2}' 
CheckIdRequest._mode = '{3}' 
CheckIdRequest.ReturnTo = '{4}' 
";

			return base.ToString() + string.Format(CultureInfo.CurrentUICulture,
				returnString, Immediate, Realm, ClaimedIdentifier, Mode, ReturnTo);
		}
	}
}