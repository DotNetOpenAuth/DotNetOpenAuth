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
	[Serializable]
	internal class InvalidSignatureException : ProtocolException {
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidSignatureException"/> class.
		/// </summary>
		/// <param name="faultedMessage">The message with the invalid signature.</param>
		public InvalidSignatureException(IProtocolMessage faultedMessage)
			: base(MessagingStrings.SignatureInvalid, faultedMessage) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidSignatureException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected InvalidSignatureException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
