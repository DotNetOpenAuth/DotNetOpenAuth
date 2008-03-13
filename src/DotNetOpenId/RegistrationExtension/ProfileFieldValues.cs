/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.Globalization;
using System.Net.Mail;
using DotNetOpenId.RegistrationExtension;
using System.Xml.Serialization;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Provider;
using System.Collections.Generic;

namespace DotNetOpenId.RegistrationExtension
{
	[Serializable()]
	public struct ProfileFieldValues
	{
		public static readonly ProfileFieldValues Empty = new ProfileFieldValues();

		public string Nickname { get; set; }
		public string Email { get; set; }
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
		public string FullName { get; set; }
		public DateTime? BirthDate { get; set; }
		public Gender? Gender { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public string Language { get; set; }
		CultureInfo culture;
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
		public string TimeZone { get; set; }

		public void AddToResponse(Provider.IAuthenticationRequest authenticationRequest) {
			if (authenticationRequest == null) throw new ArgumentNullException("authenticationRequest");
			Dictionary<string, string> fields = new Dictionary<string, string>();
			if (BirthDate != null) {
				fields.Add(QueryStringArgs.openidnp.sregnp.dob, BirthDate.ToString());
			}
			if (!String.IsNullOrEmpty(Country)) {
				fields.Add(QueryStringArgs.openidnp.sregnp.country, Country);
			}
			if (Email != null) {
				fields.Add(QueryStringArgs.openidnp.sregnp.email, Email.ToString());
			}
			if ((!String.IsNullOrEmpty(FullName))) {
				fields.Add(QueryStringArgs.openidnp.sregnp.fullname, FullName);
			}
			if (Gender != null) {
				if (Gender == DotNetOpenId.RegistrationExtension.Gender.Female) {
					fields.Add(QueryStringArgs.openidnp.sregnp.gender, QueryStringArgs.Genders.Female);
				} else {
					fields.Add(QueryStringArgs.openidnp.sregnp.gender, QueryStringArgs.Genders.Male);
				}
			}
			if (!String.IsNullOrEmpty(Language)) {
				fields.Add(QueryStringArgs.openidnp.sregnp.language, Language);
			}
			if (!String.IsNullOrEmpty(Nickname)) {
				fields.Add(QueryStringArgs.openidnp.sregnp.nickname, Nickname);
			}
			if (!String.IsNullOrEmpty(PostalCode)) {
				fields.Add(QueryStringArgs.openidnp.sregnp.postcode, PostalCode);
			}
			if (!String.IsNullOrEmpty(TimeZone)) {
				fields.Add(QueryStringArgs.openidnp.sregnp.timezone, TimeZone);
			}
			authenticationRequest.AddExtensionArguments(QueryStringArgs.openidnp.sreg.Prefix.TrimEnd('.'),
				fields);
		}
		public static ProfileFieldValues ReadFromResponse(IAuthenticationResponse response) {
			var sreg = response.GetExtensionArguments(QueryStringArgs.openidnp.sreg.Prefix.TrimEnd('.'));
			string nickname, email, fullName, dob, genderString, postalCode, country, language, timeZone;
			DateTime? birthDate = null;
			Gender? gender = null;
			sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.nickname, out nickname);
			sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.email, out email);
			sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.fullname, out fullName);
			if (sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.dob, out dob)) {
				DateTime bd;
				if (DateTime.TryParse(dob, out bd))
					birthDate = bd;
			}
			if (sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.gender, out genderString)) {
				switch (genderString) {
					case QueryStringArgs.Genders.Male: gender = DotNetOpenId.RegistrationExtension.Gender.Male; break;
					case QueryStringArgs.Genders.Female: gender = DotNetOpenId.RegistrationExtension.Gender.Female; break;
				}
			}
			sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.postcode, out postalCode);
			sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.country, out country);
			sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.language, out language);
			sreg.TryGetValue(QueryStringArgs.openidnp.sregnp.timezone, out timeZone);

			return new ProfileFieldValues() {
				Nickname = nickname,
				Email = email,
				FullName = fullName,
				BirthDate = birthDate,
				Gender = gender,
				PostalCode = postalCode,
				Country = country,
				Language = language,
				TimeZone = timeZone,
			};
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ProfileFieldValues)) return false;
			ProfileFieldValues other = (ProfileFieldValues)obj;

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