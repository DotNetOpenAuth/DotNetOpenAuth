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

namespace DotNetOpenId.RegistrationExtension
{
	[Serializable()]
	public class ProfileFieldValues
	{
		internal static ProfileFieldValues Empty = new ProfileFieldValues();

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

		public override bool Equals(object obj)
		{
			ProfileFieldValues other = obj as ProfileFieldValues;
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