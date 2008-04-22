namespace DotNetOpenId.Extensions.AttributeExchange {
	/// <summary>
	/// Attribute types defined at http://www.axschema.org/types/.
	/// </summary>
	/// <remarks>
	/// If you don't see what you need here, check that URL to see if any have been added.
	/// You can use new ones directly without adding them to this class, and can even make
	/// up your own if you expect the other end to understand what you make up.
	/// </remarks>
	public static class WellKnownAttributes {
		public static class Person {
			public const string Gender = "http://axschema.org/person/gender";
			public const string Biography = "http://axschema.org/media/biography";
		}

		public static class Preferences {
			public const string Language = "http://axschema.org/pref/language";
			public const string TimeZone = "http://axschema.org/pref/timezone";
		}

		public static class Name {
			public const string Alias = "http://axschema.org/namePerson/friendly";
			public const string FullName = "http://axschema.org/namePerson";
			public const string Prefix = "http://axschema.org/namePerson/prefix";
			public const string First = "http://axschema.org/namePerson/first";
			public const string Last = "http://axschema.org/namePerson/last";
			public const string Middle = "http://axschema.org/namePerson/middle";
			public const string Suffix = "http://axschema.org/namePerson/suffix";
		}

		public static class Company {
			public const string CompanyName = "http://axschema.org/company/name";
			public const string JobTitle = "http://axschema.org/company/title";
		}

		public static class BirthDate {
			/// <summary>Date of birth.  Example: 1979-01-01</summary>
			public const string Birthdate = "http://axschema.org/birthDate";
			public const string Year = "http://axschema.org/birthDate/birthYear";
			public const string Month = "http://axschema.org/birthDate/birthMonth";
			public const string DayOfMonth = "http://axschema.org/birthDate/birthday";
		}

		public static class Contact {
			public const string Email = "http://axschema.org/contact/email";

			public static class Phone {
				public const string Preferred = "http://axschema.org/contact/phone/default";
				public const string Home = "http://axschema.org/contact/phone/home";
				public const string Work = "http://axschema.org/contact/phone/business";
				public const string Mobile = "http://axschema.org/contact/phone/cell";
				public const string Fax = "http://axschema.org/contact/phone/fax";
			}

			public static class HomeAddress {
				public const string StreetAddressLine1 = "http://axschema.org/contact/postalAddress/home";
				public const string StreetAddressLine2 = "http://axschema.org/contact/postalAddressAdditional/home";
				public const string City = "http://axschema.org/contact/city/home";
				public const string State = "http://axschema.org/contact/state/home";
				public const string Country = "http://axschema.org/contact/country/home";
				public const string PostalCode = "http://axschema.org/contact/postalCode/home";
			}

			public static class WorkAddress {
				public const string StreetAddressLine1 = "http://axschema.org/contact/postalAddress/business";
				public const string StreetAddressLine2 = "http://axschema.org/contact/postalAddressAdditional/business";
				public const string City = "http://axschema.org/contact/city/business";
				public const string State = "http://axschema.org/contact/state/business";
				public const string Country = "http://axschema.org/contact/country/business";
				public const string PostalCode = "http://axschema.org/contact/postalCode/business";
			}

			public static class IM {
				public const string AOL = "http://axschema.org/contact/IM/AIM";
				public const string ICQ = "http://axschema.org/contact/IM/ICQ";
				public const string MSN = "http://axschema.org/contact/IM/MSN";
				public const string Yahoo = "http://axschema.org/contact/IM/Yahoo";
				public const string Jabber = "http://axschema.org/contact/IM/Jabber";
				public const string Skype = "http://axschema.org/contact/IM/Skype";
			}

			public static class Web {
				public const string WebPage = "http://axschema.org/contact/web/default";
				public const string Blog = "http://axschema.org/contact/web/blog";
				public const string LinkedIn = "http://axschema.org/contact/web/Linkedin";
				public const string Amazon = "http://axschema.org/contact/web/Amazon";
				public const string Flickr = "http://axschema.org/contact/web/Flickr";
				public const string Delicious = "http://axschema.org/contact/web/Delicious";
			}
		}

		public static class Media {
			public const string SpokenName = "http://axschema.org/media/spokenname";
			public const string AudiGreeting = "http://axschema.org/media/greeting/audio";
			public const string VideoGreeting = "http://axschema.org/media/greeting/video";

			public static class Images {
				public const string Default = "http://axschema.org/media/image/default";
				public const string Aspect11 = "http://axschema.org/media/image/aspect11";
				public const string Aspect43 = "http://axschema.org/media/image/aspect43";
				public const string Aspect34 = "http://axschema.org/media/image/aspect34";
				public const string FavIcon = "http://axschema.org/media/image/favicon";
			}
		}
	}
}
