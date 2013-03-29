//-----------------------------------------------------------------------
// <copyright file="ProtectionLevel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	/// <summary>
	/// Indicates the security services requested for an authenticated stream.
	/// </summary>
	public enum ProtectionLevel {
		/// <summary>
		/// No protection.
		/// </summary>
		None,

		/// <summary>
		/// Messages are signed.
		/// </summary>
		Sign,

		/// <summary>
		/// Messages are encrypted and signed.
		/// </summary>
		EncryptAndSign,
	}
}