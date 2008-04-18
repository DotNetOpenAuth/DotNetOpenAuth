/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.Globalization;
using System.Net.Mail;
using DotNetOpenId.Extensions;
using System.Xml.Serialization;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Provider;
using System.Collections.Generic;

namespace DotNetOpenId.Extensions
{
#pragma warning disable 0659, 0661
	/// <summary>
	/// A struct storing Simple Registration field values describing an
	/// authenticating user.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals"), Serializable()]
	public class SimpleRegistrationFieldValues : IExtensionResponse
	{
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
				if (Email == null) return null;
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

		/// <summary>
		/// Reads a Provider's response for Simple Registration values and returns
		/// an instance of this struct with the values.
		/// </summary>
		public static SimpleRegistrationFieldValues ReadFromResponse(IAuthenticationResponse response) {
			var obj = new SimpleRegistrationFieldValues();
			return ((IExtensionResponse)obj).ReadFromResponse(response) ? obj : null;
		}

		#region IExtensionResponse Members
		string IExtensionResponse.TypeUri { get { return Constants.sreg.sreg_ns; } }

		/// <summary>
		/// Adds the values of this struct to an authentication response being prepared
		/// by an OpenID Provider.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public void AddToResponse(Provider.IRequest authenticationRequest) {
			if (authenticationRequest == null) throw new ArgumentNullException("authenticationRequest");
			Dictionary<string, string> fields = new Dictionary<string, string>();
			if (BirthDate != null) {
				fields.Add(Constants.sreg.dob, BirthDate.ToString());
			}
			if (!String.IsNullOrEmpty(Country)) {
				fields.Add(Constants.sreg.country, Country);
			}
			if (Email != null) {
				fields.Add(Constants.sreg.email, Email.ToString());
			}
			if ((!String.IsNullOrEmpty(FullName))) {
				fields.Add(Constants.sreg.fullname, FullName);
			}
			if (Gender != null) {
				if (Gender == DotNetOpenId.Extensions.Gender.Female) {
					fields.Add(Constants.sreg.gender, Constants.sreg.Genders.Female);
				} else {
					fields.Add(Constants.sreg.gender, Constants.sreg.Genders.Male);
				}
			}
			if (!String.IsNullOrEmpty(Language)) {
				fields.Add(Constants.sreg.language, Language);
			}
			if (!String.IsNullOrEmpty(Nickname)) {
				fields.Add(Constants.sreg.nickname, Nickname);
			}
			if (!String.IsNullOrEmpty(PostalCode)) {
				fields.Add(Constants.sreg.postcode, PostalCode);
			}
			if (!String.IsNullOrEmpty(TimeZone)) {
				fields.Add(Constants.sreg.timezone, TimeZone);
			}
			authenticationRequest.AddExtensionArguments(Constants.sreg.sreg_ns, fields);
		}

		bool IExtensionResponse.ReadFromResponse(IAuthenticationResponse response) {
			var sreg = response.GetExtensionArguments(Constants.sreg.sreg_ns);
			if (sreg == null) return false;
			string nickname, email, fullName, dob, genderString, postalCode, country, language, timeZone;
			BirthDate = null;
			Gender = null;
			sreg.TryGetValue(Constants.sreg.nickname, out nickname);
			Nickname = nickname;
			sreg.TryGetValue(Constants.sreg.email, out email);
			Email = email;
			sreg.TryGetValue(Constants.sreg.fullname, out fullName);
			FullName = fullName;
			if (sreg.TryGetValue(Constants.sreg.dob, out dob)) {
				DateTime bd;
				if (DateTime.TryParse(dob, out bd))
					BirthDate = bd;
			}
			if (sreg.TryGetValue(Constants.sreg.gender, out genderString)) {
				switch (genderString) {
					case Constants.sreg.Genders.Male: Gender = DotNetOpenId.Extensions.Gender.Male; break;
					case Constants.sreg.Genders.Female: Gender = DotNetOpenId.Extensions.Gender.Female; break;
				}
			}
			sreg.TryGetValue(Constants.sreg.postcode, out postalCode);
			PostalCode = postalCode;
			sreg.TryGetValue(Constants.sreg.country, out country);
			Country = country;
			sreg.TryGetValue(Constants.sreg.language, out language);
			Language = language;
			sreg.TryGetValue(Constants.sreg.timezone, out timeZone);
			TimeZone = timeZone;

			return true;
		}

		#endregion

		/// <summary>
		/// Tests equality of two <see cref="SimpleRegistrationFieldValues"/> objects.
		/// </summary>
		public static bool operator ==(SimpleRegistrationFieldValues one, SimpleRegistrationFieldValues other) {
			if ((object)one == null && (object)other == null) return true;
			if ((object)one == null ^ (object)other == null) return false;
			return one.Equals(other);
		}
		/// <summary>
		/// Tests inequality of two <see cref="SimpleRegistrationFieldValues"/> objects.
		/// </summary>
		public static bool operator !=(SimpleRegistrationFieldValues one, SimpleRegistrationFieldValues other) {
			return !(one == other);
		}
		/// <summary>
		/// Tests equality of two <see cref="SimpleRegistrationFieldValues"/> objects.
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is SimpleRegistrationFieldValues)) return false;
			SimpleRegistrationFieldValues other = (SimpleRegistrationFieldValues)obj;

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