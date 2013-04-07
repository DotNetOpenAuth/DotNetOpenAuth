//-----------------------------------------------------------------------
// <copyright file="InvalidSignatureException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;

	/// <summary>
	/// An exception thrown when a signed message does not pass signature validation.
	/// </summary>
	internal class InvalidSignatureException : ProtocolException {
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidSignatureException"/> class.
		/// </summary>
		/// <param name="faultedMessage">The message with the invalid signature.</param>
		public InvalidSignatureException(IProtocolMessage faultedMessage)
			: base(MessagingStrings.SignatureInvalid, faultedMessage) { }
	}
}
