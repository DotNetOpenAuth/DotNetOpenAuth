//-----------------------------------------------------------------------
// <copyright file="NistAssuranceLevel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;

	/// <summary>
	/// Descriptions for NIST-defined levels of assurance that a credential
	/// has not been compromised and therefore the extent to which an
	/// authentication assertion can be trusted.
	/// </summary>
	/// <remarks>
	/// <para>One using this enum should review the following publication for details
	/// before asserting or interpreting what these levels signify, notwithstanding
	/// the brief summaries attached to each level in DotNetOpenAuth documentation.
	/// http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf</para>
	/// <para>
	/// See PAPE spec Appendix A.1.2 (NIST Assurance Levels) for high-level example classifications of authentication methods within the defined levels.
	/// </para>
	/// </remarks>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nist", Justification = "By design")]
	public enum NistAssuranceLevel {
		/// <summary>
		/// Not an assurance level defined by NIST, but rather SHOULD be used to 
		/// signify that the OP recognizes the parameter and the End User 
		/// authentication did not meet the requirements of Level 1.
		/// </summary>
		InsufficientForLevel1 = 0,

		/// <summary>
		/// See this document for a thorough description:
		/// http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
		/// </summary>
		Level1 = 1,

		/// <summary>
		/// See this document for a thorough description:
		/// http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
		/// </summary>
		Level2 = 2,

		/// <summary>
		/// See this document for a thorough description:
		/// http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
		/// </summary>
		Level3 = 3,

		/// <summary>
		/// See this document for a thorough description:
		/// http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
		/// </summary>
		Level4 = 4,
	}
}
