using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.RegistrationExtension;
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
	public class CheckIdRequest : AssociatedRequest {
		public ProfileRequestFields RequestedProfileFields { get; private set; }

		bool? isAuthenticated;
		/// <summary>
		/// Gets/sets whether the provider has determined that the 
		/// <see cref="IdentityUrl"/> belongs to the currently logged in user
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
		public Uri ServerUrl {
			get { return serverUrl; }
			set {
				serverUrl = value;
				InvalidateResponse();
			}
		}
		ProfileFieldValues profileFields;
		/// <summary>
		/// The simple registration profile field values to send to the consumer upon
		/// successful authentication.
		/// </summary>
		public ProfileFieldValues ProfileFields {
			get { return profileFields; }
			set {
				profileFields = value;
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
		public TrustRoot TrustRoot { get; private set; }
		/// <summary>
		/// The claimed OpenId URL of the user attempting to authenticate.
		/// </summary>
		public Uri IdentityUrl { get; private set; }
		/// <summary>
		/// The URL to redirect the user agent to after the authentication attempt.
		/// This must fall "under" the TrustRoot URL.
		/// </summary>
		internal Uri ReturnTo { get; private set; }
		/// <summary>
		/// The URL the consumer site provides for the authenticating user to review
		/// for how his claims will be used by the consumer web site.
		/// </summary>
		public Uri PolicyUrl { get; private set; }
		internal override string Mode {
			get { return Immediate ? QueryStringArgs.Modes.checkid_immediate : QueryStringArgs.Modes.checkid_setup; }
		}
		/// <summary>
		/// Indicates whether this request has all the information necessary to formulate a response.
		/// </summary>
		public override bool IsResponseReady {
			get { return IsAuthenticated.HasValue && ServerUrl != null; }
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
				args.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.cancel);
				UriUtil.AppendQueryArgs(builder, args);

				return builder.Uri;
			}
		}

		internal CheckIdRequest(OpenIdProvider server, NameValueCollection query) : base(server) {
			RequestedProfileFields = new ProfileRequestFields();
			
			// handle the mandatory protocol fields
			string mode = getRequiredField(query, QueryStringArgs.openid.mode);
			if (QueryStringArgs.Modes.checkid_immediate.Equals(mode, StringComparison.Ordinal)) {
				Immediate = true;
			} else if (QueryStringArgs.Modes.checkid_setup.Equals(mode, StringComparison.Ordinal)) {
				Immediate = false; // implied
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue, QueryStringArgs.openid.mode, mode), query);
			}

			try {
				IdentityUrl = new Uri(getRequiredField(query, QueryStringArgs.openid.identity));
			} catch (UriFormatException) {
				throw new OpenIdException(QueryStringArgs.openid.identity + " not a valid url: " + query[QueryStringArgs.openid.identity], query);
			}

			try {
				ReturnTo = new Uri(getRequiredField(query, QueryStringArgs.openid.return_to));
			} catch (UriFormatException ex) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture, 
					"'{0}' is not a valid OpenID return_to URL.", query[QueryStringArgs.openid.return_to]),
					IdentityUrl, query, ex);
			}

			try {
				TrustRoot = new TrustRoot(query[QueryStringArgs.openid.trust_root] ?? ReturnTo.AbsoluteUri);
			} catch (UriFormatException ex) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue, QueryStringArgs.openid.trust_root,
					query[QueryStringArgs.openid.trust_root]), ex);
			}
			AssociationHandle = query[QueryStringArgs.openid.assoc_handle];

			if (!TrustRoot.IsUrlWithinTrustRoot(ReturnTo)) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ReturnToNotUnderTrustRoot, ReturnTo.AbsoluteUri, TrustRoot), query);
			}

			// Handle the optional Simple Registration extension fields
			string policyUrl = query[QueryStringArgs.openid.sreg.policy_url];
			if (!String.IsNullOrEmpty(policyUrl)) {
				PolicyUrl = new Uri(policyUrl);
			}

			string optionalFields = query[QueryStringArgs.openid.sreg.optional];
			if (!String.IsNullOrEmpty(optionalFields)) {
				RequestedProfileFields.SetProfileRequestFromList(optionalFields.Split(','), ProfileRequest.Request);
			}

			string requiredFields = query[QueryStringArgs.openid.sreg.required];
			if (!String.IsNullOrEmpty(requiredFields)) {
				RequestedProfileFields.SetProfileRequestFromList(requiredFields.Split(','), ProfileRequest.Require);
			}
		}

		static string getRequiredField(NameValueCollection query, string field) {
			string value = query[field];

			if (value == null)
				throw new OpenIdException("Missing required field " + field, query);

			return value;
		}

		internal override EncodableResponse CreateResponse() {
			string mode = (IsAuthenticated.Value || Immediate) ?
				QueryStringArgs.Modes.id_res : QueryStringArgs.Modes.cancel;

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start processing Response for CheckIdRequest");
				if (TraceUtil.Switch.TraceVerbose) {
					Trace.TraceInformation("mode = '{0}',  server_url = '{1}", mode, ServerUrl);
					if (ProfileFields != null) {
						Trace.TraceInformation("Simple registration fields: {0}",
							TraceUtil.ToString(ProfileFields));
					} else {
						Trace.TraceInformation("No simple registration fields have been supplied.");
					}

				}
			}

			EncodableResponse response = new EncodableResponse(this);

			if (IsAuthenticated.Value) {
				var fields = new Dictionary<string, string>();

				fields.Add(QueryStringArgs.openidnp.mode, mode);
				fields.Add(QueryStringArgs.openidnp.identity, IdentityUrl.AbsoluteUri);
				fields.Add(QueryStringArgs.openidnp.return_to, ReturnTo.AbsoluteUri);

				if (ProfileFields != null) {
					ProfileFields.SendWithAuthenticationResponse(this);
				}

				response.AddFields(null, fields, true);
			}
			response.AddField(null, QueryStringArgs.openidnp.mode, mode, false);
			if (Immediate) {
				if (ServerUrl == null) {
					throw new ArgumentNullException("serverUrl", "serverUrl is required for allow=False in immediate mode.");
				}

				response.AddField(null, "user_setup_url", SetupUrl.AbsoluteUri, false);
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
				var q = new Dictionary<string, string>();

				q.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.checkid_setup);
				q.Add(QueryStringArgs.openid.identity, IdentityUrl.AbsoluteUri);
				q.Add(QueryStringArgs.openid.return_to, ReturnTo.AbsoluteUri);

				if (TrustRoot != null)
					q.Add(QueryStringArgs.openid.trust_root, TrustRoot.Url);

				if (this.AssociationHandle != null)
					q.Add(QueryStringArgs.openid.assoc_handle, this.AssociationHandle);

				UriBuilder builder = new UriBuilder(ServerUrl);
				UriUtil.AppendQueryArgs(builder, q);

				return builder.Uri;
			}
		}

		public override string ToString() {
			string returnString = @"
CheckIdRequest.Immediate = '{0}'
CheckIdRequest.TrustRoot = '{1}'
CheckIdRequest.Identity = '{2}' 
CheckIdRequest._mode = '{3}' 
CheckIdRequest.ReturnTo = '{4}' 
CheckIdRequest._policyUrl = '{5}'
{6}
";

			return base.ToString() + string.Format(CultureInfo.CurrentUICulture,
				returnString, Immediate, TrustRoot, IdentityUrl, Mode, ReturnTo,
				PolicyUrl, RequestedProfileFields);
		}
	}
}