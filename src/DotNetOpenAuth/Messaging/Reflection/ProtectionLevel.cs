//-----------------------------------------------------------------------
// <copyright file="ProtectionLevel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
#if SILVERLIGHT // full desktop defines this in the Framework
	/// <summary>
	/// Indicates the security services requested for an authenticated stream.
	/// </summary>
	public enum ProtectionLevel {
		/// <summary>
		/// Authentication only.
		/// </summary>
		None = 0,

		/// <summary>
		/// Sign data to help ensure the integrity of transmitted data.
		/// </summary>
		Sign = 1,

		/// <summary>
		/// Encrypt and sign data to help ensure the confidentiality and integrity of transmitted data.
		/// </summary>
		EncryptAndSign = 2,
	}
#endif
}
