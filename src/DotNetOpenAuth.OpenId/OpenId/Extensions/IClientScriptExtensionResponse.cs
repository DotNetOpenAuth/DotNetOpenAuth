//-----------------------------------------------------------------------
// <copyright file="IClientScriptExtensionResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// An interface that OpenID extensions can implement to allow authentication response
	/// messages with included extensions to be processed by Javascript on the user agent.
	/// </summary>
	public interface IClientScriptExtensionResponse : IExtensionMessage {
		/// <summary>
		/// Reads the extension information on an authentication response from the provider.
		/// </summary>
		/// <param name="response">The incoming OpenID response carrying the extension.</param>
		/// <returns>
		/// A Javascript snippet that when executed on the user agent returns an object with
		/// the information deserialized from the extension response.
		/// </returns>
		/// <remarks>
		/// This method is called <b>before</b> the signature on the assertion response has been
		/// verified.  Therefore all information in these fields should be assumed unreliable
		/// and potentially falsified.
		/// </remarks>
		string InitializeJavaScriptData(IProtocolMessageWithExtensions response);
	}
}
