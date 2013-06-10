//-----------------------------------------------------------------------
// <copyright file="IOpenIdMessageExtension.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The contract any OpenID extension for DotNetOpenAuth must implement.
	/// </summary>
	/// <remarks>
	/// Classes that implement this interface should be marked as
	/// [<see cref="SerializableAttribute"/>] to allow serializing state servers
	/// to cache messages, particularly responses.
	/// </remarks>
	public interface IOpenIdMessageExtension : IExtensionMessage {
		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		string TypeUri { get; }

		/// <summary>
		/// Gets the additional TypeURIs that are supported by this extension, in preferred order.
		/// May be empty if none other than <see cref="TypeUri"/> is supported, but
		/// should not be null.
		/// </summary>
		/// <remarks>
		/// Useful for reading in messages with an older version of an extension.
		/// The value in the <see cref="TypeUri"/> property is always checked before
		/// trying this list.
		/// If you do support multiple versions of an extension using this method,
		/// consider adding a CreateResponse method to your request extension class
		/// so that the response can have the context it needs to remain compatible
		/// given the version of the extension in the request message.
		/// The <see cref="Extensions.SimpleRegistration.ClaimsRequest.CreateResponse"/> for an example.
		/// </remarks>
		IEnumerable<string> AdditionalSupportedTypeUris { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this extension was 
		/// signed by the sender.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the sender; otherwise, <c>false</c>.
		/// </value>
		bool IsSignedByRemoteParty { get; set; }
	}
}
