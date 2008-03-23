using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using DotNetOpenId.Provider;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// Carries the request/require/none demand state of the simple registration fields.
	/// </summary>
#pragma warning disable 0659, 0661
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals")]
	public struct SimpleRegistrationRequestFields {
		public static readonly SimpleRegistrationRequestFields None = new SimpleRegistrationRequestFields();

		public SimpleRegistrationRequest Nickname { get; set; }
		public SimpleRegistrationRequest Email { get; set; }
		public SimpleRegistrationRequest FullName { get; set; }
		public SimpleRegistrationRequest BirthDate { get; set; }
		public SimpleRegistrationRequest Gender { get; set; }
		public SimpleRegistrationRequest PostalCode { get; set; }
		public SimpleRegistrationRequest Country { get; set; }
		public SimpleRegistrationRequest Language { get; set; }
		public SimpleRegistrationRequest TimeZone { get; set; }

		/// <summary>
		/// The URL the consumer site provides for the authenticating user to review
		/// for how his claims will be used by the consumer web site.
		/// </summary>
		public Uri PolicyUrl { get; set; }

		/// <summary>
		/// Sets the profile request properties according to a list of
		/// field names that might have been passed in the OpenId query dictionary.
		/// </summary>
		/// <param name="fieldNames">
		/// The list of field names that should receive a given 
		/// <paramref name="requestLevel"/>.  These field names should match 
		/// the OpenId specification for field names, omitting the 'openid.sreg' prefix.
		/// </param>
		/// <param name="requestLevel">The none/request/require state of the listed fields.</param>
		internal void SetProfileRequestFromList(ICollection<string> fieldNames, SimpleRegistrationRequest requestLevel) {
			foreach (string field in fieldNames) {
				switch (field) {
					case Protocol.Constants.openidnp.sregnp.nickname:
						Nickname = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.email:
						Email = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.fullname:
						FullName = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.dob:
						BirthDate = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.gender:
						Gender = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.postcode:
						PostalCode = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.country:
						Country = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.language:
						Language = requestLevel;
						break;
					case Protocol.Constants.openidnp.sregnp.timezone:
						TimeZone = requestLevel;
						break;
					default:
						Trace.TraceWarning("OpenIdProfileRequest.SetProfileRequestFromList: Unrecognized field name '{0}'.", field);
						break;
				}
			}
		}
		string[] assembleProfileFields(SimpleRegistrationRequest level) {
			List<string> fields = new List<string>(10);
			if (Nickname == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.nickname);
			if (Email == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.email);
			if (FullName == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.fullname);
			if (BirthDate == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.dob);
			if (Gender == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.gender);
			if (PostalCode == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.postcode);
			if (Country == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.country);
			if (Language == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.language);
			if (TimeZone == level)
				fields.Add(Protocol.Constants.openidnp.sregnp.timezone);

			return fields.ToArray();
		}
		/// <summary>
		/// Reads the sreg extension information on an authentication request to the provider
		/// and returns information on what profile fields the consumer is requesting/requiring.
		/// </summary>
		public static SimpleRegistrationRequestFields ReadFromRequest(IRequest request) {
			SimpleRegistrationRequestFields fields = new SimpleRegistrationRequestFields();
			var args = request.GetExtensionArguments(Protocol.Constants.sreg_ns);
			if (args == null) return fields;

			string policyUrl;
			if (args.TryGetValue(Protocol.Constants.openidnp.sregnp.policy_url, out policyUrl)
				&& !string.IsNullOrEmpty(policyUrl)) {
				fields.PolicyUrl = new Uri(policyUrl);
			}

			string optionalFields;
			if (args.TryGetValue(Protocol.Constants.openidnp.sregnp.optional, out optionalFields)) {
				fields.SetProfileRequestFromList(optionalFields.Split(','), SimpleRegistrationRequest.Request);
			}

			string requiredFields;
			if (args.TryGetValue(Protocol.Constants.openidnp.sregnp.required, out requiredFields)) {
				fields.SetProfileRequestFromList(requiredFields.Split(','), SimpleRegistrationRequest.Require);
			}

			return fields;
		}
		public void AddToRequest(RelyingParty.IAuthenticationRequest request) {
			var fields = new Dictionary<string, string>();
			if (PolicyUrl != null)
				fields.Add(Protocol.Constants.openidnp.sregnp.policy_url, PolicyUrl.AbsoluteUri);

			fields.Add(Protocol.Constants.openidnp.sregnp.required, string.Join(",", assembleProfileFields(SimpleRegistrationRequest.Require)));
			fields.Add(Protocol.Constants.openidnp.sregnp.optional, string.Join(",", assembleProfileFields(SimpleRegistrationRequest.Request)));

			request.AddExtensionArguments(Protocol.Constants.sreg_ns, fields);
		}

		public override string ToString() {
			return string.Format(CultureInfo.CurrentUICulture, @"Nickname = '{0}' 
Email = '{1}' 
FullName = '{2}' 
Birthdate = '{3}'
Gender = '{4}'
PostalCode = '{5}'
Country = '{6}'
Language = '{7}'
TimeZone = '{8}'", Nickname, Email, FullName, BirthDate, Gender, PostalCode, Country, Language, TimeZone);
		}
		public static bool operator ==(SimpleRegistrationRequestFields one, SimpleRegistrationRequestFields other) {
			return one.Equals(other);
		}
		public static bool operator !=(SimpleRegistrationRequestFields one, SimpleRegistrationRequestFields other) {
			return !one.Equals(other);
		}
		public override bool Equals(object obj) {
			if (!(obj is SimpleRegistrationRequestFields)) return false;
			SimpleRegistrationRequestFields other = (SimpleRegistrationRequestFields)obj;

			return
				safeEquals(this.BirthDate, other.BirthDate) &&
				safeEquals(this.Country, other.Country) &&
				safeEquals(this.Language, other.Language) &&
				safeEquals(this.Email, other.Email) &&
				safeEquals(this.FullName, other.FullName) &&
				safeEquals(this.Gender, other.Gender) &&
				safeEquals(this.Nickname, other.Nickname) &&
				safeEquals(this.PostalCode, other.PostalCode) &&
				safeEquals(this.TimeZone, other.TimeZone) &&
				safeEquals(this.PolicyUrl, other.PolicyUrl);
		}
		static bool safeEquals(object one, object other) {
			if (one == null && other == null) return true;
			if (one == null ^ other == null) return false;
			return one.Equals(other);
		}
	}
}
