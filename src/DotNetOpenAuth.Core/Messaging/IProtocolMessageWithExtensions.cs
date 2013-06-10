//-----------------------------------------------------------------------
// <copyright file="IProtocolMessageWithExtensions.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A protocol message that supports adding extensions to the payload for transmission.
	/// </summary>
	public interface IProtocolMessageWithExtensions : IProtocolMessage {
		/// <summary>
		/// Gets the list of extensions that are included with this message.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		IList<IExtensionMessage> Extensions { get; }
	}
}
