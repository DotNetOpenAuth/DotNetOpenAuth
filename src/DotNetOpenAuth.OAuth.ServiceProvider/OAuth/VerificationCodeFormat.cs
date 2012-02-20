//-----------------------------------------------------------------------
// <copyright file="VerificationCodeFormat.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// The different formats a user authorization verifier code can take
	/// in order to be as secure as possible while being compatible with
	/// the type of OAuth Consumer requesting access.
	/// </summary>
	/// <remarks>
	/// Some Consumers may be set-top boxes, video games, mobile devies, etc.
	/// with very limited character entry support and no ability to receive
	/// a callback URI.  OAuth 1.0a requires that these devices operators
	/// must manually key in a verifier code, so in these cases it better
	/// be possible to do so given the input options on that device.
	/// </remarks>
	public enum VerificationCodeFormat {
		/// <summary>
		/// The strongest verification code.
		/// The best option for web consumers since a callback is usually an option.
		/// </summary>
		IncludedInCallback,

		/// <summary>
		/// A combination of upper and lowercase letters and numbers may be used,
		/// allowing a computer operator to easily read from the screen and key
		/// in the verification code.
		/// </summary>
		/// <remarks>
		/// Some letters and numbers will be skipped where they are visually similar
		/// enough that they can be difficult to distinguish when displayed with most fonts.
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Alikes", Justification = "Breaking change of existing API")]
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "AlphaNumeric", Justification = "Breaking change of existing API")]
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LookAlikes", Justification = "Breaking change of existing API")]
		AlphaNumericNoLookAlikes,

		/// <summary>
		/// Only uppercase letters will be used in the verification code.
		/// Verification codes are case-sensitive, so consumers with fixed
		/// keyboards with only one character case option may require this option.
		/// </summary>
		AlphaUpper,

		/// <summary>
		/// Only lowercase letters will be used in the verification code.
		/// Verification codes are case-sensitive, so consumers with fixed
		/// keyboards with only one character case option may require this option.
		/// </summary>
		AlphaLower,

		/// <summary>
		/// Only the numbers 0-9 will be used in the verification code.
		/// Must useful for consumers running on mobile phone devices.
		/// </summary>
		Numeric,
	}
}
