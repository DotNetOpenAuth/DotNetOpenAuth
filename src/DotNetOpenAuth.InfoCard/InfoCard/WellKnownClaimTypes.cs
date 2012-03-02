//-----------------------------------------------------------------------
// <copyright file="WellKnownClaimTypes.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Well known claims that may be included in an Information Card.
	/// </summary>
	public class WellKnownClaimTypes {
		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous" claim.
		/// </summary>
		public const string Anonymous = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication" claim.
		/// </summary>
		public const string Authentication = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision" claim.
		/// </summary>
		public const string AuthorizationDecision = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision";

		/// <summary>
		/// The date of birth of a subject in a form allowed by the xs:date data type.
		/// </summary>
		/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth</value>
		public const string DateOfBirth = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid" claim.
		/// </summary>
		public const string DenyOnlySid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns" claim.
		/// </summary>
		public const string Dns = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash" claim.
		/// </summary>
		public const string Hash = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" claim.
		/// </summary>
		public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

		/// <summary>
		/// A private personal identifier (PPID) that identifies the subject to a relying party. 
		/// </summary>
		/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier</value>
		/// <remarks>
		/// The word “private” is used in the sense that the subject identifier is 
		/// specific to a given relying party and hence private to that relying party. 
		/// A subject's PPID at one relying party cannot be correlated with the subject's 
		/// PPID at another relying party.
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ppid", Justification = "By design")]
		public const string Ppid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa" claim.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rsa", Justification = "By design")]
		public const string Rsa = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid" claim.
		/// </summary>
		public const string Sid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn" claim.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Spn", Justification = "By design")]
		public const string Spn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system" claim.
		/// </summary>
		public const string System = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint" claim.
		/// </summary>
		public const string Thumbprint = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" claim.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Upn", Justification = "By design")]
		public const string Upn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri" claim.
		/// </summary>
		public const string Uri = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname" claim.
		/// </summary>
		public const string X500DistinguishedName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname";

		/// <summary>
		/// Prevents a default instance of the <see cref="WellKnownClaimTypes"/> class from being created.
		/// </summary>
		private WellKnownClaimTypes() {
		}

		/// <summary>
		/// Inherent attributes about a personality such as gender and bio.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Person {
			/// <summary>
			/// Gender of a subject.
			/// </summary>
			/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender</value>
			/// <remarks>
			/// The value of the claim can have any of these exact string values –
			/// 0 (unspecified) or
			/// 1 (Male) or
			/// 2 (Female). Using these values allows them to be language neutral.
			/// </remarks>
			public const string Gender = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender";
		}

		/// <summary>
		/// Various ways to contact a person.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
		public static class Contact {
			/// <summary>
			/// Preferred address for the “To:” field of email to be sent to the subject.
			/// </summary>
			/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress</value>
			/// <remarks>
			/// (mail in inetOrgPerson) Usually of the form @. According to inetOrgPerson using RFC 1274: “This attribute type specifies an electronic mailbox attribute following the syntax specified in RFC 822.”
			/// </remarks>
			public const string Email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

			/// <summary>
			/// Various types of phone numbers.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class Phone {
				/// <summary>
				/// Primary or home telephone number of a subject. 
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone</value>
				/// <remarks>
				/// According to inetOrgPerson using RFC 1274:
				/// “This attribute type specifies 
				/// a home telephone number associated with a person.” Attribute values 
				/// should follow the agreed format for international telephone numbers, 
				/// e.g. +44 71 123 4567.
				/// </remarks>
				public const string HomePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone";

				/// <summary>
				/// Mobile telephone number of a subject. 
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone</value>
				/// <remarks>
				/// (mobile in inetOrgPerson) According to inetOrgPerson using RFC 1274: “This attribute type specifies a mobile telephone number associated with a person.” Attribute values should follow the agreed format for international telephone numbers, e.g. +44 71 123 4567.
				/// </remarks>
				public const string MobilePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";

				/// <summary>
				/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone" claim.
				/// </summary>
				public const string OtherPhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone";
			}

			/// <summary>
			/// The many fields that make up an address.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class Address {
				/// <summary>
				/// Street address component of a subject's address information. 
				/// According to RFC 2256: 
				/// “This attribute contains the physical address of the object to which 
				/// the entry corresponds, such as an address for package delivery.” 
				/// Its content is arbitrary, but typically given as a PO Box number or 
				/// apartment/house number followed by a street name, e.g. 303 Mulberry St.
				/// (street in RFC 2256) 
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress</value>
				public const string StreetAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress";

				/// <summary>
				/// Locality component of a subject's address information.
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality</value>
				/// <remarks>
				/// According to RFC 2256: “This attribute contains the name of a locality, such as a city, county or other geographic region.” e.g. Redmond.
				/// </remarks>
				public const string City = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality";

				/// <summary>
				/// Abbreviation for state or province name of a subject's address information. 
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince</value>
				/// <remarks>
				/// According to RFC 2256: “This attribute contains the full name of a state or province. The values should be coordinated on a national level and if well-known shortcuts exist - like the two-letter state abbreviations in the US – these abbreviations are preferred over longer full names.” e.g. WA.
				/// </remarks>
				public const string StateOrProvince = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince";

				/// <summary>
				/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode" claim.
				/// </summary>
				public const string PostalCode = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode";

				/// <summary>
				/// Country of a subject. 
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country</value>
				/// <remarks>
				/// (c in RFC 2256) According to RFC 2256: “This attribute contains a two-letter ISO 3166 country code.”
				/// </remarks>
				public const string Country = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country";
			}

			/// <summary>
			/// The names a person goes by.
			/// </summary>
			[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class Name {
				/// <summary>
				/// Preferred name or first name of a subject. According to RFC 2256: “This attribute is used to hold the part of a person’s name which is not their surname nor middle name.”
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname</value>
				/// <remarks>
				/// (givenName in RFC 2256)
				/// </remarks>
				public const string GivenName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";

				/// <summary>
				/// Surname or family name of a subject. 
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname</value>
				/// <remarks>
				/// According to RFC 2256: “This is the X.500 surname attribute which contains the family name of a person.”
				/// </remarks>
				public const string Surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
			}

			/// <summary>
			/// Various web addresses connected with this personality.
			/// </summary>
			[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "By design"), SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Required for desired autocompletion.")]
			public static class Web {
				/// <summary>
				/// The Web page of a subject expressed as a URL.
				/// </summary>
				/// <value>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage</value>
				public const string Homepage = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage";
			}
		}
	}
}