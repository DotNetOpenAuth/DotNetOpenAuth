//-----------------------------------------------------------------------
// <copyright file="WellKnownAttributes.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Attribute types defined at http://www.axschema.org/types/.
	/// </summary>
	/// <remarks>
	/// If you don't see what you need here, check that URL to see if any have been added.
	/// You can use new ones directly without adding them to this class, and can even make
	/// up your own if you expect the other end to understand what you make up.
	/// </remarks>
	[SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1630:DocumentationTextMustContainWhitespace", Justification = "The samples are string literals.")]
	[SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1631:DocumentationMustMeetCharacterPercentage", Justification = "The samples are string literals.")]
	public static class WellKnownAttributes {
		/// <summary>
		/// Inherent attributes about a personality such as gender and bio.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Person {
			/// <summary>Gender, either "M" or "F"</summary>
			/// <example>"M", "F"</example>
			public const string Gender = "http://axschema.org/person/gender";

			/// <summary>Biography (text)</summary>
			/// <example>"I am the very model of a modern Major General."</example>
			public const string Biography = "http://axschema.org/media/biography";
		}

		/// <summary>
		/// Preferences such as language and timezone.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Preferences {
			/// <summary>Preferred language, as per RFC4646</summary>
			/// <example>"en-US"</example>
			public const string Language = "http://axschema.org/pref/language";

			/// <summary>Home time zone information (as specified in <a href="http://en.wikipedia.org/wiki/List_of_tz_zones_by_name">zoneinfo</a>)</summary>
			/// <example>"America/Pacific"</example>
			public const string TimeZone = "http://axschema.org/pref/timezone";
		}

		/// <summary>
		/// The names a person goes by.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Name {
			/// <summary>Subject's alias or "screen" name</summary>
			/// <example>"Johnny5"</example>
			public const string Alias = "http://axschema.org/namePerson/friendly";

			/// <summary>Full name of subject</summary>
			/// <example>"John Doe"</example>
			public const string FullName = "http://axschema.org/namePerson";

			/// <summary>Honorific prefix for the subject's name</summary>
			/// <example>"Mr.", "Mrs.", "Dr."</example>
			public const string Prefix = "http://axschema.org/namePerson/prefix";

			/// <summary>First or given name of subject</summary>
			/// <example>"John"</example>
			public const string First = "http://axschema.org/namePerson/first";

			/// <summary>Last name or surname of subject</summary>
			/// <example>"Smith"</example>
			public const string Last = "http://axschema.org/namePerson/last";

			/// <summary>Middle name(s) of subject</summary>
			/// <example>"Robert"</example>
			public const string Middle = "http://axschema.org/namePerson/middle";

			/// <summary>Suffix of subject's name</summary>
			/// <example>"III", "Jr."</example>
			public const string Suffix = "http://axschema.org/namePerson/suffix";
		}

		/// <summary>
		/// Business affiliation.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Company {
			/// <summary>Company name (employer)</summary>
			/// <example>"Springfield Power"</example>
			public const string CompanyName = "http://axschema.org/company/name";

			/// <summary>Employee title</summary>
			/// <example>"Engineer"</example>
			public const string JobTitle = "http://axschema.org/company/title";
		}

		/// <summary>
		/// Information about a person's birthdate.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class BirthDate {
			/// <summary>Date of birth.</summary>
			/// <example>"1979-01-01"</example>
			public const string WholeBirthDate = "http://axschema.org/birthDate";

			/// <summary>Year of birth (four digits)</summary>
			/// <example>"1979"</example>
			public const string Year = "http://axschema.org/birthDate/birthYear";

			/// <summary>Month of birth (1-12)</summary>
			/// <example>"05"</example>
			public const string Month = "http://axschema.org/birthDate/birthMonth";

			/// <summary>Day of birth</summary>
			/// <example>"31"</example>
			public const string DayOfMonth = "http://axschema.org/birthDate/birthday";
		}

		/// <summary>
		/// Various ways to contact a person.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Contact {
			/// <summary>Internet SMTP email address as per RFC2822</summary>
			/// <example>"jsmith@isp.example.com"</example>
			public const string Email = "http://axschema.org/contact/email";

			/// <summary>
			/// Various types of phone numbers.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class Phone {
				/// <summary>Main phone number (preferred)</summary>
				/// <example>+1-800-555-1234</example>
				public const string Preferred = "http://axschema.org/contact/phone/default";

				/// <summary>Home phone number</summary>
				/// <example>+1-800-555-1234</example>
				public const string Home = "http://axschema.org/contact/phone/home";

				/// <summary>Business phone number</summary>
				/// <example>+1-800-555-1234</example>
				public const string Work = "http://axschema.org/contact/phone/business";

				/// <summary>Cellular (or mobile) phone number</summary>
				/// <example>+1-800-555-1234</example>
				public const string Mobile = "http://axschema.org/contact/phone/cell";

				/// <summary>Fax number</summary>
				/// <example>+1-800-555-1234</example>
				public const string Fax = "http://axschema.org/contact/phone/fax";
			}

			/// <summary>
			/// The many fields that make up an address.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class HomeAddress {
				/// <summary>Home postal address: street number, name and apartment number</summary>
				/// <example>"#42 135 East 1st Street"</example>
				public const string StreetAddressLine1 = "http://axschema.org/contact/postalAddress/home";

				/// <summary>"#42 135 East 1st Street"</summary>
				/// <example>"Box 67"</example>
				public const string StreetAddressLine2 = "http://axschema.org/contact/postalAddressAdditional/home";

				/// <summary>Home city name</summary>
				/// <example>"Vancouver"</example>
				public const string City = "http://axschema.org/contact/city/home";

				/// <summary>Home state or province name</summary>
				/// <example>"BC"</example>
				public const string State = "http://axschema.org/contact/state/home";

				/// <summary>Home country code in ISO.3166.1988 (alpha 2) format</summary>
				/// <example>"CA"</example>
				public const string Country = "http://axschema.org/contact/country/home";

				/// <summary>Home postal code; region specific format</summary>
				/// <example>"V5A 4B2"</example>
				public const string PostalCode = "http://axschema.org/contact/postalCode/home";
			}

			/// <summary>
			/// The many fields that make up an address.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class WorkAddress {
				/// <summary>Business postal address: street number, name and apartment number</summary>
				/// <example>"#42 135 East 1st Street"</example>
				public const string StreetAddressLine1 = "http://axschema.org/contact/postalAddress/business";

				/// <summary>"#42 135 East 1st Street"</summary>
				/// <example>"Box 67"</example>
				public const string StreetAddressLine2 = "http://axschema.org/contact/postalAddressAdditional/business";

				/// <summary>Business city name</summary>
				/// <example>"Vancouver"</example>
				public const string City = "http://axschema.org/contact/city/business";

				/// <summary>Business state or province name</summary>
				/// <example>"BC"</example>
				public const string State = "http://axschema.org/contact/state/business";

				/// <summary>Business country code in ISO.3166.1988 (alpha 2) format</summary>
				/// <example>"CA"</example>
				public const string Country = "http://axschema.org/contact/country/business";

				/// <summary>Business postal code; region specific format</summary>
				/// <example>"V5A 4B2"</example>
				public const string PostalCode = "http://axschema.org/contact/postalCode/business";
			}

			/// <summary>
			/// Various handles for instant message clients.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class IM {
				/// <summary>AOL instant messaging service handle</summary>
				/// <example>"jsmith421234"</example>
				[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "AOL", Justification = "By design")]
				public const string AOL = "http://axschema.org/contact/IM/AIM";

				/// <summary>ICQ instant messaging service handle</summary>
				/// <example>"1234567"</example>
				[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ICQ", Justification = "By design")]
				public const string ICQ = "http://axschema.org/contact/IM/ICQ";

				/// <summary>MSN instant messaging service handle</summary>
				/// <example>"jsmith42@hotmail.com"</example>
				[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MSN", Justification = "By design")]
				public const string MSN = "http://axschema.org/contact/IM/MSN";

				/// <summary>Yahoo! instant messaging service handle</summary>
				/// <example>"jsmith421234"</example>
				public const string Yahoo = "http://axschema.org/contact/IM/Yahoo";

				/// <summary>Jabber instant messaging service handle</summary>
				/// <example>"jsmith@jabber.example.com"</example>
				public const string Jabber = "http://axschema.org/contact/IM/Jabber";

				/// <summary>Skype instant messaging service handle</summary>
				/// <example>"jsmith42"</example>
				[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Skype", Justification = "By design")]
				public const string Skype = "http://axschema.org/contact/IM/Skype";
			}

			/// <summary>
			/// Various web addresses connected with this personality.
			/// </summary>
			[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "By design"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class Web {
				/// <summary>Web site URL</summary>
				/// <example>"http://example.com/~jsmith/"</example>
				public const string Homepage = "http://axschema.org/contact/web/default";

				/// <summary>Blog home page URL</summary>
				/// <example>"http://example.com/jsmith_blog/"</example>
				public const string Blog = "http://axschema.org/contact/web/blog";

				/// <summary>LinkedIn URL</summary>
				/// <example>"http://www.linkedin.com/pub/1/234/56"</example>
				public const string LinkedIn = "http://axschema.org/contact/web/Linkedin";

				/// <summary>Amazon URL</summary>
				/// <example>"http://www.amazon.com/gp/pdp/profile/A24DLKJ825"</example>
				public const string Amazon = "http://axschema.org/contact/web/Amazon";

				/// <summary>Flickr URL</summary>
				/// <example>"http://flickr.com/photos/jsmith42/"</example>
				[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Flickr", Justification = "By design")]
				public const string Flickr = "http://axschema.org/contact/web/Flickr";

				/// <summary>del.icio.us URL</summary>
				/// <example>"http://del.icio.us/jsmith42"</example>
				public const string Delicious = "http://axschema.org/contact/web/Delicious";
			}
		}

		/// <summary>
		/// Audio and images of this personality.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "By design"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Media {
			/// <summary>Spoken name (web URL)</summary>
			/// <example>"http://example.com/~jsmith/john_smith.wav"</example>
			public const string SpokenName = "http://axschema.org/media/spokenname";

			/// <summary>Audio greeting (web URL)</summary>
			/// <example>"http://example.com/~jsmith/i_greet_you.wav"</example>
			public const string AudioGreeting = "http://axschema.org/media/greeting/audio";

			/// <summary>Video greeting (web URL)</summary>
			/// <example>"http://example.com/~jsmith/i_greet_you.mov"</example>
			public const string VideoGreeting = "http://axschema.org/media/greeting/video";

			/// <summary>
			/// Images of this personality.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class Images {
				/// <summary>Image (web URL); unspecified dimension</summary>
				/// <example>"http://example.com/~jsmith/image.jpg"</example>
				public const string Default = "http://axschema.org/media/image/default";

				/// <summary>Image (web URL) with equal width and height</summary>
				/// <example>"http://example.com/~jsmith/image.jpg"</example>
				public const string Aspect11 = "http://axschema.org/media/image/aspect11";

				/// <summary>Image (web URL) 4:3 aspect ratio - landscape</summary>
				/// <example>"http://example.com/~jsmith/image.jpg"</example>
				public const string Aspect43 = "http://axschema.org/media/image/aspect43";

				/// <summary>Image (web URL) 4:3 aspect ratio - landscape</summary>
				/// <example>"http://example.com/~jsmith/image.jpg"</example>
				public const string Aspect34 = "http://axschema.org/media/image/aspect34";

				/// <summary>Image (web URL); favicon format as per FAVICON-W3C. The format for the image must be 16x16 pixels or 32x32 pixels, using either 8-bit or 24-bit colors. The format of the image must be one of PNG (a W3C standard), GIF, or ICO.</summary>
				/// <example>"http://example.com/~jsmith/image.jpg"</example>
				[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fav", Justification = "By design")]
				public const string FavIcon = "http://axschema.org/media/image/favicon";
			}
		}
	}
}
