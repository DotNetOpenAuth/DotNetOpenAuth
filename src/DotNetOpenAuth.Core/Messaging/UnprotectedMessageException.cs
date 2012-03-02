//-----------------------------------------------------------------------
// <copyright file="UnprotectedMessageException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Globalization;

	/// <summary>
	/// An exception thrown when messages cannot receive all the protections they require.
	/// </summary>
	[Serializable]
	internal class UnprotectedMessageException : ProtocolException {
		/// <summary>
		/// Initializes a new instance of the <see cref="UnprotectedMessageException"/> class.
		/// </summary>
		/// <param name="faultedMessage">The message whose protection requirements could not be met.</param>
		/// <param name="appliedProtection">The protection requirements that were fulfilled.</param>
		internal UnprotectedMessageException(IProtocolMessage faultedMessage, MessageProtections appliedProtection)
			: base(string.Format(CultureInfo.CurrentCulture, MessagingStrings.InsufficientMessageProtection, faultedMessage.GetType().Name, faultedMessage.RequiredProtection, appliedProtection), faultedMessage) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnprotectedMessageException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected UnprotectedMessageException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
