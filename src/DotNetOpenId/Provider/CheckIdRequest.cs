using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.RegistrationExtension;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// A request to confirm the identity of a user.
	/// </summary>
	/// <remarks>
	/// This class handles requests for openid modes checkid_immediate and checkid_setup.
	/// </remarks>
	public class CheckIdRequest : AssociatedRequest {
		public ProfileRequestFields RequestedProfileFields { get; private set; }

		/// <summary>
		/// Whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		public bool Immediate { get; private set; }
		/// <summary>
		/// The URL the consumer site claims to use as its 'base' address.
		/// </summary>
		public string TrustRoot { get; private set; }
		/// <summary>
		/// The claimed OpenId URL of the user attempting to authenticate.
		/// </summary>
		public Uri IdentityUrl { get; private set; }
		/// <summary>
		/// The URL to redirect the user agent to after the authentication attempt.
		/// This must fall "under" the TrustRoot URL.
		/// </summary>
		public Uri ReturnTo { get; private set; }
		/// <summary>
		/// The URL the consumer site provides for the authenticating user to review
		/// for how his claims will be used by the consumer web site.
		/// </summary>
		public Uri PolicyUrl { get; private set; }
		internal override string Mode {
			get { return Immediate ? QueryStringArgs.Modes.checkid_immediate : QueryStringArgs.Modes.checkid_setup; }
		}
		public override RequestType RequestType {
			get { return RequestType.CheckIdRequest; }
		}
		public bool IsTrustRootValid {
			get {
				Debug.Assert(TrustRoot != null, "The constructor should have guaranteed this.");

				try {
					return new TrustRoot(TrustRoot).IsUrlWithinTrustRoot(ReturnTo);
				} catch (UriFormatException) {
					return false;
				}
			}
		}
		/// <summary>
		/// Get the URL to cancel this request.
		/// </summary>
		public Uri CancelUrl {
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

		internal CheckIdRequest(OpenIdProvider server, Uri identity, Uri return_to, string trust_root, 
			bool immediate, string assoc_handle) : base(server) {
			RequestedProfileFields = new ProfileRequestFields();
			
			AssociationHandle = assoc_handle;
			IdentityUrl = identity;
			ReturnTo = return_to;
			TrustRoot = trust_root ?? return_to.AbsoluteUri;
			Immediate = immediate;

			if (!this.IsTrustRootValid)
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture, 
					Strings.ReturnToNotUnderTrustRoot, ReturnTo.AbsoluteUri, TrustRoot));
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
				throw new OpenIdException(QueryStringArgs.openid.mode + " does not have any expected value: " + mode, query);
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

			TrustRoot = query[QueryStringArgs.openid.trust_root] ?? ReturnTo.AbsoluteUri;
			AssociationHandle = query[QueryStringArgs.openid.assoc_handle];

			if (!IsTrustRootValid)
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture, 
					Strings.ReturnToNotUnderTrustRoot, ReturnTo.AbsoluteUri, TrustRoot), query);

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

		/// <summary>
		/// Respond to this request.
		/// </summary>
		/// <param name="allow">Allow this user to claim this identity, and allow the consumer to have this information?</param>
		public Response Answer(bool allow, Uri serverUrl) {
			return Answer(allow, serverUrl, null);
		}

		/// <summary>
		/// Respond to this request.
		/// </summary>
		/// <param name="allow">Allow this user to claim this identity, and allow the consumer to have this information?</param>
		public Response Answer(bool allow, Uri serverUrl, ProfileFieldValues openIdProfileFields) {
			string mode = (allow || Immediate) ? QueryStringArgs.Modes.id_res : QueryStringArgs.Modes.cancel;

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("Start processing Response for CheckIdRequest");
				if (TraceUtil.Switch.TraceVerbose) {
					Trace.TraceInformation("mode = '{0}',  server_url = '{1}", mode, serverUrl);
					if (openIdProfileFields != null) {
						Trace.TraceInformation("Simple registration fields: {0}",
							TraceUtil.ToString(openIdProfileFields));
					} else {
						Trace.TraceInformation("No simple registration fields have been supplied.");
					}

				}
			}

			EncodableResponse response = new EncodableResponse(this);

			if (allow) {
				var fields = new Dictionary<string, string>();

				fields.Add(QueryStringArgs.openidnp.mode, mode);
				fields.Add(QueryStringArgs.openidnp.identity, IdentityUrl.AbsoluteUri);
				fields.Add(QueryStringArgs.openidnp.return_to, ReturnTo.AbsoluteUri);

				if (openIdProfileFields != null) {
					if (openIdProfileFields.BirthDate != null) {
						fields.Add(QueryStringArgs.openidnp.sreg.dob, openIdProfileFields.BirthDate.ToString());
					}
					if (!String.IsNullOrEmpty(openIdProfileFields.Country)) {
						fields.Add(QueryStringArgs.openidnp.sreg.country, openIdProfileFields.Country);
					}
					if (openIdProfileFields.Email != null) {
						fields.Add(QueryStringArgs.openidnp.sreg.email, openIdProfileFields.Email.ToString());
					}
					if ((!String.IsNullOrEmpty(openIdProfileFields.FullName))) {
						fields.Add(QueryStringArgs.openidnp.sreg.fullname, openIdProfileFields.FullName);
					}

					if (openIdProfileFields.Gender != null) {
						if (openIdProfileFields.Gender == Gender.Female) {
							fields.Add(QueryStringArgs.openidnp.sreg.gender, QueryStringArgs.Genders.Female);
						} else {
							fields.Add(QueryStringArgs.openidnp.sreg.gender, QueryStringArgs.Genders.Male);
						}
					}

					if (!String.IsNullOrEmpty(openIdProfileFields.Language)) {
						fields.Add(QueryStringArgs.openidnp.sreg.language, openIdProfileFields.Language);
					}

					if (!String.IsNullOrEmpty(openIdProfileFields.Nickname)) {
						fields.Add(QueryStringArgs.openidnp.sreg.nickname, openIdProfileFields.Nickname);
					}

					if (!String.IsNullOrEmpty(openIdProfileFields.PostalCode)) {
						fields.Add(QueryStringArgs.openidnp.sreg.postcode, openIdProfileFields.PostalCode);
					}

					if (!String.IsNullOrEmpty(openIdProfileFields.TimeZone)) {
						fields.Add(QueryStringArgs.openidnp.sreg.timezone, openIdProfileFields.TimeZone);
					}
				}

				response.AddFields(null, fields, true);
			}
			response.AddField(null, QueryStringArgs.openidnp.mode, mode, false);
			if (Immediate) {
				if (serverUrl == null) {
					throw new ArgumentNullException("serverUrl", "serverUrl is required for allow=False in immediate mode.");
				}

				CheckIdRequest setup_request = new CheckIdRequest(Server, IdentityUrl, ReturnTo, TrustRoot, false, this.AssociationHandle);

				Uri setup_url = setup_request.encodeToUrl(serverUrl);

				response.AddField(null, "user_setup_url", setup_url.AbsoluteUri, false);
			}

			if (TraceUtil.Switch.TraceInfo) {
				Trace.TraceInformation("CheckIdRequest response successfully created. ");
				if (TraceUtil.Switch.TraceVerbose) {
					Trace.TraceInformation("Response follows: {0}", response);
				}
			}

			return Server.EncodeResponse(response);
		}

		protected override Response CreateResponse() {
			throw new NotSupportedException("Call Answer method instead.");
		}

		/// <summary>
		/// Encode this request as a URL to GET.
		/// </summary>
		/// <param name="server_url">The URL of the OpenID server to make this request of. </param>
		Uri encodeToUrl(Uri server_url) {
			var q = new Dictionary<string, string>();

			q.Add(QueryStringArgs.openid.mode, Mode);
			q.Add(QueryStringArgs.openid.identity, IdentityUrl.AbsoluteUri);
			q.Add(QueryStringArgs.openid.return_to, ReturnTo.AbsoluteUri);

			if (TrustRoot != null)
				q.Add(QueryStringArgs.openid.trust_root, TrustRoot);

			if (this.AssociationHandle != null)
				q.Add(QueryStringArgs.openid.assoc_handle, this.AssociationHandle);

			UriBuilder builder = new UriBuilder(server_url);
			UriUtil.AppendQueryArgs(builder, q);

			return builder.Uri;
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