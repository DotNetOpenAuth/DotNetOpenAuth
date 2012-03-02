//-----------------------------------------------------------------------
// <copyright file="MessageProtections.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;

	/// <summary>
	/// Categorizes the various types of channel binding elements so they can be properly ordered.
	/// </summary>
	/// <remarks>
	/// The order of these enum values is significant.  
	/// Each successive value requires the protection offered by all the previous values
	/// in order to be reliable.  For example, message expiration is meaningless without
	/// tamper protection to prevent a user from changing the timestamp on a message.
	/// </remarks>
	[Flags]
	public enum MessageProtections {
		/// <summary>
		/// No protection.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// A binding element that signs a message before sending and validates its signature upon receiving.
		/// </summary>
		TamperProtection = 0x1,

		/// <summary>
		/// A binding element that enforces a maximum message age between sending and processing on the receiving side.
		/// </summary>
		Expiration = 0x2,

		/// <summary>
		/// A binding element that prepares messages for replay detection and detects replayed messages on the receiving side.
		/// </summary>
		ReplayProtection = 0x4,

		/// <summary>
		/// All forms of protection together.
		/// </summary>
		All = TamperProtection | Expiration | ReplayProtection,
	}
}