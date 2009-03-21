//-----------------------------------------------------------------------
// <copyright file="WellKnownClaimTypes.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country" claim.
		/// </summary>
		public const string Country = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth" claim.
		/// </summary>
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
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" claim.
		/// </summary>
		public const string Email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender" claim.
		/// </summary>
		public const string Gender = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" claim.
		/// </summary>
		public const string GivenName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash" claim.
		/// </summary>
		public const string Hash = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone" claim.
		/// </summary>
		public const string HomePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality" claim.
		/// </summary>
		public const string Locality = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone" claim.
		/// </summary>
		public const string MobilePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" claim.
		/// </summary>
		public const string Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" claim.
		/// </summary>
		public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone" claim.
		/// </summary>
		public const string OtherPhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode" claim.
		/// </summary>
		public const string PostalCode = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier" claim.
		/// </summary>
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
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince" claim.
		/// </summary>
		public const string StateOrProvince = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress" claim.
		/// </summary>
		public const string StreetAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" claim.
		/// </summary>
		public const string Surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";

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
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage" claim.
		/// </summary>
		public const string Webpage = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage";

		/// <summary>
		/// The "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname" claim.
		/// </summary>
		public const string X500DistinguishedName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname";

		/// <summary>
		/// Prevents a default instance of the <see cref="WellKnownClaimTypes"/> class from being created.
		/// </summary>
		private WellKnownClaimTypes() {
		}
	}
}