using System;
using System.Globalization;
using System.Net.Mail;
using Janrain.OpenId.RegistrationExtension;

namespace Janrain.OpenId.RegistrationExtension
{
    [Serializable()]
    public class OpenIdProfileFields
    {
        internal static OpenIdProfileFields Empty = new OpenIdProfileFields();

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
        public CultureInfo Culture
        {
            get { return culture; }
            set { culture = value; }
        }

        private string timeZone;
        public string TimeZone
        {
            get { return timeZone; }
            set { timeZone = value; }
        }
    }
}