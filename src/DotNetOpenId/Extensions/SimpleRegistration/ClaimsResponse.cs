/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Extensions.SimpleRegistration
{
#pragma warning disable 0659, 0661
	/// <summary>
	/// A struct storing Simple Registration field values describing an
	/// authenticating user.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals"), Serializable()]
	public sealed class ClaimsResponse : IExtensionResponse, IClientScriptExtensionResponse
	{
		string typeUriToUse;

		/// <summary>
		/// Creates an instance of the <see cref="ClaimsResponse"/> class.
		/// </summary>
		[Obsolete("Use ClaimsRequest.CreateResponse() instead.")]
		public ClaimsResponse() : this(Constants.sreg_ns) {
		}

		internal ClaimsResponse(string typeUriToUse) {
			if (string.IsNullOrEmpty(typeUriToUse)) throw new ArgumentNullException("typeUriToUse");
			this.typeUriToUse = typeUriToUse;
		}

		/// <summary>
		/// The nickname the user goes by.
		/// </summary>
		public string Nickname { get; set; }
		/// <summary>
		/// The user's email address.
		/// </summary>
		public string Email { get; set; }
		/// <summary>
		/// A combination of the user's full name and email address.
		/// </summary>
		public MailAddress MailAddress
		{
			get
			{
				if (string.IsNullOrEmpty(Email)) return null;
				if (string.IsNullOrEmpty(FullName))
					return new MailAddress(Email);
				else
					return new MailAddress(Email, FullName);
			}
		}
		/// <summary>
		/// The full name of a user as a single string.
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// The user's birthdate.
		/// </summary>
		public DateTime? BirthDate { get; set; }
		/// <summary>
		/// The gender of the user.
		/// </summary>
		public Gender? Gender { get; set; }
		/// <summary>
		/// The zip code / postal code of the user.
		/// </summary>
		public string PostalCode { get; set; }
		/// <summary>
		/// The country of the user.
		/// </summary>
		public string Country { get; set; }
		/// <summary>
		/// The primary/preferred language of the user.
		/// </summary>
		public string Language { get; set; }
		CultureInfo culture;
		/// <summary>
		/// A combination o the language and country of the user.
		/// </summary>
		[XmlIgnore]
		public CultureInfo Culture
		{
			get
			{
				if (culture == null && !string.IsNullOrEmpty(Language))
				{
					string cultureString = "";
					cultureString = Language;
					if (!string.IsNullOrEmpty(Country))
						cultureString += "-" + Country;
					culture = CultureInfo.GetCultureInfo(cultureString);
				}

				return culture;
			}
			set
			{
				culture = value;
				Language = (value != null) ? value.TwoLetterISOLanguageName : null;
				int indexOfHyphen = (value != null) ? value.Name.IndexOf('-') : -1;
				Country = indexOfHyphen > 0 ? value.Name.Substring(indexOfHyphen + 1) : null;
			}
		}
		/// <summary>
		/// The user's timezone.
		/// </summary>
		public string TimeZone { get; set; }

		#region IExtensionResponse Members
		string IExtension.TypeUri { get { return typeUriToUse; } }
		IEnumerable<string> IExtension.AdditionalSupportedTypeUris {
			get { return new string[0]; }
		}

		/// <summary>
		/// Adds the values of this struct to an authentication response being prepared
		/// by an OpenID Provider.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		IDictionary<string, string> IExtensionResponse.Serialize(Provider.IRequest authenticationRequest) {
			if (authenticationRequest == null) throw new ArgumentNullException("authenticationRequest");
			Dictionary<string, string> fields = new Dictionary<string, string>();
			if (BirthDate != null) {
				fields.Add(Constants.dob, BirthDate.ToString());
			}
			if (!String.IsNullOrEmpty(Country)) {
				fields.Add(Constants.country, Country);
			}
			if (Email != null) {
				fields.Add(Constants.email, Email.ToString());
			}
			if ((!String.IsNullOrEmpty(FullName))) {
				fields.Add(Constants.fullname, FullName);
			}
			if (Gender != null) {
				if (Gender == SimpleRegistration.Gender.Female) {
					fields.Add(Constants.gender, Constants.Genders.Female);
				} else {
					fields.Add(Constants.gender, Constants.Genders.Male);
				}
			}
			if (!String.IsNullOrEmpty(Language)) {
				fields.Add(Constants.language, Language);
			}
			if (!String.IsNullOrEmpty(Nickname)) {
				fields.Add(Constants.nickname, Nickname);
			}
			if (!String.IsNullOrEmpty(PostalCode)) {
				fields.Add(Constants.postcode, PostalCode);
			}
			if (!String.IsNullOrEmpty(TimeZone)) {
				fields.Add(Constants.timezone, TimeZone);
			}
			return fields;
		}

		bool IExtensionResponse.Deserialize(IDictionary<string, string> sreg, IAuthenticationResponse response, string typeUri) {
			if (sreg == null) return false;
			string nickname, email, fullName, dob, genderString, postalCode, country, language, timeZone;
			BirthDate = null;
			Gender = null;
			sreg.TryGetValue(Constants.nickname, out nickname);
			Nickname = nickname;
			sreg.TryGetValue(Constants.email, out email);
			Email = email;
			sreg.TryGetValue(Constants.fullname, out fullName);
			FullName = fullName;
			if (sreg.TryGetValue(Constants.dob, out dob)) {
				DateTime bd;
				if (DateTime.TryParse(dob, out bd))
					BirthDate = bd;
			}
			if (sreg.TryGetValue(Constants.gender, out genderString)) {
				switch (genderString) {
					case Constants.Genders.Male: Gender = SimpleRegistration.Gender.Male; break;
					case Constants.Genders.Female: Gender = SimpleRegistration.Gender.Female; break;
				}
			}
			sreg.TryGetValue(Constants.postcode, out postalCode);
			PostalCode = postalCode;
			sreg.TryGetValue(Constants.country, out country);
			Country = country;
			sreg.TryGetValue(Constants.language, out language);
			Language = language;
			sreg.TryGetValue(Constants.timezone, out timeZone);
			TimeZone = timeZone;

			return true;
		}

		#endregion

		#region IClientScriptExtension Members

		static string createAddFieldJS(string propertyName, string value) {
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1},",
				propertyName, Util.GetSafeJavascriptValue(value));
		}

		string IClientScriptExtensionResponse.InitializeJavaScriptData(IDictionary<string, string> sreg, IAuthenticationResponse response, string typeUri) {
			StringBuilder builder = new StringBuilder();
			builder.Append("{ ");
			
			string nickname, email, fullName, dob, genderString, postalCode, country, language, timeZone;
			if (sreg.TryGetValue(Constants.nickname, out nickname)) {
				builder.Append(createAddFieldJS(Constants.nickname, nickname));
			}
			if (sreg.TryGetValue(Constants.email, out email)) {
				builder.Append(createAddFieldJS(Constants.email, email));
			}
			if (sreg.TryGetValue(Constants.fullname, out fullName)) {
				builder.Append(createAddFieldJS(Constants.fullname, fullName));
			}
			if (sreg.TryGetValue(Constants.dob, out dob)) {
				builder.Append(createAddFieldJS(Constants.dob, dob));
			}
			if (sreg.TryGetValue(Constants.gender, out genderString)) {
				builder.Append(createAddFieldJS(Constants.gender, genderString));
			}
			if (sreg.TryGetValue(Constants.postcode, out postalCode)) {
				builder.Append(createAddFieldJS(Constants.postcode, postalCode));
			}
			if (sreg.TryGetValue(Constants.country, out country)) {
				builder.Append(createAddFieldJS(Constants.country, country));
			}
			if (sreg.TryGetValue(Constants.language, out language)) {
				builder.Append(createAddFieldJS(Constants.language, language));
			}
			if (sreg.TryGetValue(Constants.timezone, out timeZone)) {
				builder.Append(createAddFieldJS(Constants.timezone, timeZone));
			}
			if (builder[builder.Length - 1] == ',') builder.Length -= 1;
			builder.Append("}");
			return builder.ToString();
		}

		#endregion

		/// <summary>
		/// Tests equality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		public static bool operator ==(ClaimsResponse one, ClaimsResponse other) {
			if ((object)one == null && (object)other == null) return true;
			if ((object)one == null ^ (object)other == null) return false;
			return one.Equals(other);
		}
		/// <summary>
		/// Tests inequality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		public static bool operator !=(ClaimsResponse one, ClaimsResponse other) {
			return !(one == other);
		}
		/// <summary>
		/// Tests equality of two <see cref="ClaimsResponse"/> objects.
		/// </summary>
		public override bool Equals(object obj)
		{
			ClaimsResponse other = obj as ClaimsResponse;
			if (other == null) return false;

			return
				safeEquals(this.BirthDate, other.BirthDate) &&
				safeEquals(this.Country, other.Country) &&
				safeEquals(this.Language, other.Language) &&
				safeEquals(this.Email, other.Email) &&
				safeEquals(this.FullName, other.FullName) &&
				safeEquals(this.Gender, other.Gender) &&
				safeEquals(this.Nickname, other.Nickname) &&
				safeEquals(this.PostalCode, other.PostalCode) &&
				safeEquals(this.TimeZone, other.TimeZone);
		}
		static bool safeEquals(object one, object other)
		{
			if (one == null && other == null) return true;
			if (one == null ^ other == null) return false;
			return one.Equals(other);
		}
	}
}