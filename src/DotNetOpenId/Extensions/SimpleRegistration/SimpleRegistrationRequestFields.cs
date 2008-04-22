using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using DotNetOpenId.Provider;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Extensions.SimpleRegistration {
	/// <summary>
	/// Carries the request/require/none demand state of the simple registration fields.
	/// </summary>
#pragma warning disable 0659, 0661
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals")]
	public class SimpleRegistrationRequestFields : IExtensionRequest {
		/// <summary>
		/// The level of interest a relying party has in the nickname of the user.
		/// </summary>
		public SimpleRegistrationRequest Nickname { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the email of the user.
		/// </summary>
		public SimpleRegistrationRequest Email { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the full name of the user.
		/// </summary>
		public SimpleRegistrationRequest FullName { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the birthdate of the user.
		/// </summary>
		public SimpleRegistrationRequest BirthDate { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the gender of the user.
		/// </summary>
		public SimpleRegistrationRequest Gender { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the postal code of the user.
		/// </summary>
		public SimpleRegistrationRequest PostalCode { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the Country of the user.
		/// </summary>
		public SimpleRegistrationRequest Country { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the language of the user.
		/// </summary>
		public SimpleRegistrationRequest Language { get; set; }
		/// <summary>
		/// The level of interest a relying party has in the time zone of the user.
		/// </summary>
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
					case Constants.nickname:
						Nickname = requestLevel;
						break;
					case Constants.email:
						Email = requestLevel;
						break;
					case Constants.fullname:
						FullName = requestLevel;
						break;
					case Constants.dob:
						BirthDate = requestLevel;
						break;
					case Constants.gender:
						Gender = requestLevel;
						break;
					case Constants.postcode:
						PostalCode = requestLevel;
						break;
					case Constants.country:
						Country = requestLevel;
						break;
					case Constants.language:
						Language = requestLevel;
						break;
					case Constants.timezone:
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
				fields.Add(Constants.nickname);
			if (Email == level)
				fields.Add(Constants.email);
			if (FullName == level)
				fields.Add(Constants.fullname);
			if (BirthDate == level)
				fields.Add(Constants.dob);
			if (Gender == level)
				fields.Add(Constants.gender);
			if (PostalCode == level)
				fields.Add(Constants.postcode);
			if (Country == level)
				fields.Add(Constants.country);
			if (Language == level)
				fields.Add(Constants.language);
			if (TimeZone == level)
				fields.Add(Constants.timezone);

			return fields.ToArray();
		}

		#region IExtensionRequest Members
		string IExtension.TypeUri { get { return Constants.sreg_ns; } }

		bool IExtensionRequest.Deserialize(IDictionary<string, string> args, IRequest request) {
			if (args == null) return false;

			string policyUrl;
			if (args.TryGetValue(Constants.policy_url, out policyUrl)
				&& !string.IsNullOrEmpty(policyUrl)) {
				PolicyUrl = new Uri(policyUrl);
			}

			string optionalFields;
			if (args.TryGetValue(Constants.optional, out optionalFields)) {
				SetProfileRequestFromList(optionalFields.Split(','), SimpleRegistrationRequest.Request);
			}

			string requiredFields;
			if (args.TryGetValue(Constants.required, out requiredFields)) {
				SetProfileRequestFromList(requiredFields.Split(','), SimpleRegistrationRequest.Require);
			}

			return true;
		}

		/// <summary>
		/// Adds a description of the information the relying party site would like
		/// the Provider to include with a positive authentication assertion as an
		/// extension to an authentication request.
		/// </summary>
		IDictionary<string, string> IExtensionRequest.Serialize(RelyingParty.IAuthenticationRequest request) {
			var fields = new Dictionary<string, string>();
			if (PolicyUrl != null)
				fields.Add(Constants.policy_url, PolicyUrl.AbsoluteUri);

			fields.Add(Constants.required, string.Join(",", assembleProfileFields(SimpleRegistrationRequest.Require)));
			fields.Add(Constants.optional, string.Join(",", assembleProfileFields(SimpleRegistrationRequest.Request)));

			return fields;
		}
		#endregion

		/// <summary>
		/// Renders the requested information as a string.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return string.Format(CultureInfo.CurrentCulture, @"Nickname = '{0}' 
Email = '{1}' 
FullName = '{2}' 
Birthdate = '{3}'
Gender = '{4}'
PostalCode = '{5}'
Country = '{6}'
Language = '{7}'
TimeZone = '{8}'", Nickname, Email, FullName, BirthDate, Gender, PostalCode, Country, Language, TimeZone);
		}
		/// <summary>
		/// Tests equality between two <see cref="SimpleRegistrationRequestFields"/> structs.
		/// </summary>
		public static bool operator ==(SimpleRegistrationRequestFields one, SimpleRegistrationRequestFields other) {
			if ((object)one == null && (object)other == null) return true;
			if ((object)one == null ^ (object)other == null) return false;
			return one.Equals(other);
				}
		/// <summary>
		/// Tests inequality between two <see cref="SimpleRegistrationRequestFields"/> structs.
		/// </summary>
		public static bool operator !=(SimpleRegistrationRequestFields one, SimpleRegistrationRequestFields other) {
			return !(one == other);
		}
		/// <summary>
		/// Tests equality between two <see cref="SimpleRegistrationRequestFields"/> structs.
		/// </summary>
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
