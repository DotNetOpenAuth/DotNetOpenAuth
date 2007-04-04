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

        private MailAddress email;
        [XmlIgnore]
        public MailAddress Email
        {
            get { return email; }
            set { email = value; }
        }

        [XmlElement("Email")]
        string emailString
        {
            get { return Email == null ? null : Email.Address; }
            set { Email = (string.IsNullOrEmpty(value) ? null : new MailAddress(value)); }
        }

        private string fullName;
        public string Fullname
        {
            get { return fullName; }
            set { fullName = value; }
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
    }
}