using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId.RegistrationExtension;
using System.Diagnostics;
using System.Collections.Generic;

namespace DotNetOpenId.Server {
	/// <summary>
	/// A request to confirm the identity of a user.
	/// </summary>
	/// <remarks>
	/// This class handles requests for openid modes checkid_immediate and checkid_setup.
	/// </remarks>
	public class CheckIdRequest : AssociatedRequest {

		ProfileRequest requestNicknameDefault = ProfileRequest.NoRequest;
		ProfileRequest requestEmailDefault = ProfileRequest.NoRequest;
		ProfileRequest requestFullNameDefault = ProfileRequest.NoRequest;
		ProfileRequest requestBirthdateDefault = ProfileRequest.NoRequest;
		ProfileRequest requestGenderDefault = ProfileRequest.NoRequest;
		ProfileRequest requestPostalCodeDefault = ProfileRequest.NoRequest;
		ProfileRequest requestCountryDefault = ProfileRequest.NoRequest;
		ProfileRequest requestLanguageDefault = ProfileRequest.NoRequest;
		ProfileRequest requestTimeZoneDefault = ProfileRequest.NoRequest;

		public ProfileRequest RequestNicknameDefault {
			get { return requestNicknameDefault; }
		}
		public ProfileRequest RequestEmailDefault {
			get { return requestEmailDefault; }
		}
		public ProfileRequest RequestFullNameDefault {
			get { return requestFullNameDefault; }
		}
		public ProfileRequest RequestBirthdateDefault {
			get { return requestBirthdateDefault; }
		}
		public ProfileRequest RequestGenderDefault {
			get { return requestGenderDefault; }
		}
		public ProfileRequest RequestPostalCodeDefault {
			get { return requestPostalCodeDefault; }
		}
		public ProfileRequest RequestCountryDefault {
			get { return requestCountryDefault; }
		}
		public ProfileRequest RequestLanguageDefault {
			get { return requestLanguageDefault; }
		}
		public ProfileRequest RequestTimeZoneDefault {
			get { return requestTimeZoneDefault; }
		}

		public bool Immediate { get; private set; }
		public string TrustRoot { get; private set; }
		public Uri IdentityUrl { get; private set; }
		public Uri ReturnTo { get; private set; }
		public Uri PolicyUrl { get; private set; }
		internal override string Mode {
			get { return Immediate ? QueryStringArgs.Modes.checkid_immediate : QueryStringArgs.Modes.checkid_setup; }
		}
		public override RequestType RequestType {
			get { return RequestType.CheckIdRequest; }
		}
		public bool IsAnySimpleRegistrationFieldsRequestedOrRequired {
			get {
				return (!(this.requestBirthdateDefault == ProfileRequest.NoRequest
						  && this.requestCountryDefault == ProfileRequest.NoRequest
						  && this.requestEmailDefault == ProfileRequest.NoRequest
						  && this.requestFullNameDefault == ProfileRequest.NoRequest
						  && this.requestGenderDefault == ProfileRequest.NoRequest
						  && this.requestLanguageDefault == ProfileRequest.NoRequest
						  && this.requestNicknameDefault == ProfileRequest.NoRequest
						  && this.requestPostalCodeDefault == ProfileRequest.NoRequest
						  && this.requestTimeZoneDefault == ProfileRequest.NoRequest));
			}
		}
		public bool IsTrustRootValid {
			get {
				Debug.Assert(TrustRoot != null, "The constructor should have guaranteed this.");

				try {
					return new TrustRoot(TrustRoot).ValidateUrl(ReturnTo);
				} catch (MalformedTrustRootException) {
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

				return new Uri(builder.ToString());
			}
		}

		internal CheckIdRequest(Uri identity, Uri return_to, string trust_root, bool immediate, string assoc_handle) {
			this.AssocHandle = assoc_handle;

			IdentityUrl = identity;
			ReturnTo = return_to;
			TrustRoot = trust_root ?? return_to.AbsoluteUri;
			Immediate = immediate;

			if (!this.IsTrustRootValid)
				throw new UntrustedReturnUrlException(null, ReturnTo, TrustRoot);
		}

		internal CheckIdRequest(NameValueCollection query) {
			// handle the mandatory protocol fields
			string mode = getRequiredField(query, QueryStringArgs.openid.mode);
			if (QueryStringArgs.Modes.checkid_immediate.Equals(mode, StringComparison.Ordinal)) {
				Immediate = true;
			} else if (QueryStringArgs.Modes.checkid_setup.Equals(mode, StringComparison.Ordinal)) {
				Immediate = false; // implied
			} else {
				throw new ProtocolException(query, QueryStringArgs.openid.mode + " does not have any expected value: " + mode);
			}

			try {
				IdentityUrl = new Uri(getRequiredField(query, QueryStringArgs.openid.identity));
			} catch (UriFormatException) {
				throw new ProtocolException(query, QueryStringArgs.openid.identity + " not a valid url: " + query[QueryStringArgs.openid.identity]);
			}

			try {
				ReturnTo = new Uri(getRequiredField(query, QueryStringArgs.openid.return_to));
			} catch (UriFormatException) {
				throw new MalformedReturnUrlException(query, query[QueryStringArgs.openid.return_to]);
			}

			TrustRoot = query[QueryStringArgs.openid.trust_root] ?? ReturnTo.AbsoluteUri;
			AssocHandle = query[QueryStringArgs.openid.assoc_handle];

			if (!IsTrustRootValid)
				throw new UntrustedReturnUrlException(query, ReturnTo, TrustRoot);

			// Handle the optional Simple Registration extension fields
			string policyUrl = query[QueryStringArgs.openid.sreg.policy_url];
			if (!String.IsNullOrEmpty(policyUrl)) {
				PolicyUrl = new Uri(policyUrl);
			}

			string optionalFields = query[QueryStringArgs.openid.sreg.optional];
			if (!String.IsNullOrEmpty(optionalFields)) {
				string[] splitOptionalFields = optionalFields.Split(',');
				setSimpleRegistrationExtensionFields(splitOptionalFields, ProfileRequest.Request);
			}

			string requiredFields = query[QueryStringArgs.openid.sreg.required];
			if (!String.IsNullOrEmpty(requiredFields)) {
				string[] splitRrequiredFields = requiredFields.Split(',');
				setSimpleRegistrationExtensionFields(splitRrequiredFields, ProfileRequest.Require);
			}
		}

		string getRequiredField(NameValueCollection query, string field) {
			string value = query[field];

			if (value == null)
				throw new ProtocolException(query, "Missing required field " + field);

			return value;
		}

		void setSimpleRegistrationExtensionFields(string[] fields, ProfileRequest request) {
			foreach (string field in fields) {
				switch (field) {
					case QueryStringArgs.openidnp.sregnp.nickname:
						this.requestNicknameDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.email:
						this.requestEmailDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.fullname:
						this.requestFullNameDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.dob:
						this.requestBirthdateDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.gender:
						this.requestGenderDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.postcode:
						this.requestPostalCodeDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.country:
						this.requestCountryDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.language:
						this.requestLanguageDefault = request;
						break;
					case QueryStringArgs.openidnp.sregnp.timezone:
						this.requestTimeZoneDefault = request;
						break;
				}
			}
		}

		/// <summary>
		/// Respond to this request.
		/// </summary>
		/// <param name="allow">Allow this user to claim this identity, and allow the consumer to have this information?</param>
		/// <param name="server_url"></param>
		/// <returns></returns>
		public Response Answer(bool allow, Uri server_url) {
			return Answer(allow, server_url, null);
		}

		/// <summary>
		/// Respond to this request.
		/// </summary>
		/// <param name="allow">Allow this user to claim this identity, and allow the consumer to have this information?</param>
		/// <param name="server_url"></param>
		/// <param name="openIdProfileFields"></param>
		/// <returns></returns>
		public Response Answer(bool allow, Uri server_url, OpenIdProfileFields openIdProfileFields) {
			string mode;

			if (allow || Immediate)
				mode = QueryStringArgs.Modes.id_res;
			else
				mode = QueryStringArgs.Modes.cancel;

			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("Start processing Response for CheckIdRequest");
				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ServerTrace(String.Format("mode = '{0}',  server_url = '{1}", mode, server_url.ToString()));
					if (openIdProfileFields != null) {
						TraceUtil.ServerTrace("Simple registration fields follow: ");
						TraceUtil.ServerTrace(openIdProfileFields);
					} else {
						TraceUtil.ServerTrace("No simple registration fields have been supplied.");
					}

				}
			}

			#endregion

			Response response = new Response(this);

			if (allow) {
				var fields = new Dictionary<string, string>();

				fields.Add(QueryStringArgs.openidnp.mode, mode);
				fields.Add(QueryStringArgs.openidnp.identity, IdentityUrl.AbsoluteUri);
				fields.Add(QueryStringArgs.openidnp.return_to, ReturnTo.AbsoluteUri);

				if (openIdProfileFields != null) {
					if (openIdProfileFields.Birthdate != null) {
						fields.Add(QueryStringArgs.openidnp.sreg.dob, openIdProfileFields.Birthdate.ToString());
					}
					if (!String.IsNullOrEmpty(openIdProfileFields.Country)) {
						fields.Add(QueryStringArgs.openidnp.sreg.country, openIdProfileFields.Country);
					}
					if (openIdProfileFields.Email != null) {
						fields.Add(QueryStringArgs.openidnp.sreg.email, openIdProfileFields.Email.ToString());
					}
					if ((!String.IsNullOrEmpty(openIdProfileFields.Fullname))) {
						fields.Add(QueryStringArgs.openidnp.sreg.fullname, openIdProfileFields.Fullname);
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
				if (server_url == null) { throw new ApplicationException("setup_url is required for allow=False in immediate mode."); }

				CheckIdRequest setup_request = new CheckIdRequest(IdentityUrl, ReturnTo, TrustRoot, false, this.AssocHandle);

				Uri setup_url = setup_request.encodeToUrl(server_url);

				response.AddField(null, "user_setup_url", setup_url.AbsoluteUri, false);
			}

			#region  Trace
			if (TraceUtil.Switch.TraceInfo) {
				TraceUtil.ServerTrace("CheckIdRequest response successfully created. ");
				if (TraceUtil.Switch.TraceVerbose) {
					TraceUtil.ServerTrace("Response follows. ");
					TraceUtil.ServerTrace(response.ToString());
				}
			}

			#endregion

			return response;
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

			if (this.AssocHandle != null)
				q.Add(QueryStringArgs.openid.assoc_handle, this.AssocHandle);

			UriBuilder builder = new UriBuilder(server_url);
			UriUtil.AppendQueryArgs(builder, q);

			return new Uri(builder.ToString());
		}

		public override string ToString() {
			string returnString = @"
CheckIdRequest.Immediate = '{0}'
CheckIdRequest.TrustRoot = '{1}'
CheckIdRequest.Identity = '{2}' 
CheckIdRequest._mode = '{3}' 
CheckIdRequest.ReturnTo = '{4}' 
CheckIdRequest._policyUrl = '{5}' 
CheckIdRequest.requestNicknameDefault = '{6}' 
CheckIdRequest.requestEmailDefault = '{7}' 
CheckIdRequest.requestFullNameDefault = '{8}' 
CheckIdRequest.requestBirthdateDefault = '{9}'
CheckIdRequest.requestGenderDefault = '{10}'
CheckIdRequest.requestPostalCodeDefault = '{11}'
CheckIdRequest.requestCountryDefault = '{12}'
CheckIdRequest.requestLanguageDefault = '{13}'
CheckIdRequest.requestTimeZoneDefault = '{14}'";

			return base.ToString() + string.Format(
				returnString, Immediate, TrustRoot, IdentityUrl, Mode, ReturnTo,
				PolicyUrl, requestNicknameDefault, requestEmailDefault,
				requestFullNameDefault, requestBirthdateDefault, requestGenderDefault,
				requestPostalCodeDefault, requestCountryDefault, requestLanguageDefault,
				requestTimeZoneDefault);
		}
	}
}