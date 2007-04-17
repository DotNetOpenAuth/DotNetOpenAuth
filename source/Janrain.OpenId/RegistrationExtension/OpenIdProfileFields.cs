using System;
using System.Globalization;
using System.Net.Mail;
using Janrain.OpenId.RegistrationExtension;
using System.Xml.Serialization;

namespace Janrain.OpenId.RegistrationExtension
{
	[Serializable()]
	public class OpenIdProfileFields
	{
		internal static OpenIdProfileFields Empty = new OpenIdProfileFields();

		public OpenIdProfileFields()
		{

		}

		private string nickname;
		public string Nickname
		{
			get { return nickname; }
			set { nickname = value; }
		}

		private string email;
		public string Email
		{
			get { return email; }
			set { email = value; }
		}

		public MailAddress MailAddress
		{
			get
			{
				if (Email == null) return null;
				if (string.IsNullOrEmpty(Fullname))
					return new MailAddress(Email);
				else
					return new MailAddress(Email, Fullname);
			}
		}

		private string fullname;
		public string Fullname
		{
			get { return fullname; }
			set { fullname = value; }
		}

		private DateTime? birthdate;
		public DateTime? Birthdate
		{
			get { return birthdate; }
			set { birthdate = value; }
		}

		private Gender? gender;
		public Gender? Gender
		{
			get { return gender; }
			set { gender = value; }
		}

		private string postalCode;
		public string PostalCode
		{
			get { return postalCode; }
			set { postalCode = value; }
		}

		private string country;
		public string Country
		{
			get { return country; }
			set { country = value; }
		}

		private string language;
		public string Language
		{
			get { return language; }
			set { language = value; }
		}

		private CultureInfo culture;
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

		private string timeZone;
		public string TimeZone
		{
			get { return timeZone; }
			set { timeZone = value; }
		}

		public override bool Equals(object obj)
		{
			OpenIdProfileFields other = obj as OpenIdProfileFields;
			if (other == null) return false;

			return
				safeEquals(this.Birthdate, other.Birthdate) &&
				safeEquals(this.Country, other.Country) &&
				safeEquals(this.Language, other.Language) &&
				safeEquals(this.Email, other.Email) &&
				safeEquals(this.Fullname, other.Fullname) &&
				safeEquals(this.Gender, other.Gender) &&
				safeEquals(this.Nickname, other.Nickname) &&
				safeEquals(this.PostalCode, other.PostalCode) &&
				safeEquals(this.TimeZone, other.TimeZone);
		}

		bool safeEquals(object one, object other)
		{
			if (one == null && other == null) return true;
			if (one == null ^ other == null) return false;
			return one.Equals(other);
		}
	}
}