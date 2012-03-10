//-----------------------------------------------------------------------
// <copyright file="IOpenIdMessageExtension.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
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
	[ContractClass(typeof(IOpenIdMessageExtensionContract))]
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

	/// <summary>
	/// Code contract class for the IOpenIdMessageExtension interface.
	/// </summary>
	[ContractClassFor(typeof(IOpenIdMessageExtension))]
	internal abstract class IOpenIdMessageExtensionContract : IOpenIdMessageExtension {
		/// <summary>
		/// Prevents a default instance of the <see cref="IOpenIdMessageExtensionContract"/> class from being created.
		/// </summary>
		private IOpenIdMessageExtensionContract() {
		}

		#region IOpenIdMessageExtension Members

		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		string IOpenIdMessageExtension.TypeUri {
			get {
				Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the additional TypeURIs that are supported by this extension, in preferred order.
		/// May be empty if none other than <see cref="IOpenIdMessageExtension.TypeUri"/> is supported, but
		/// should not be null.
		/// </summary>
		/// <remarks>
		/// Useful for reading in messages with an older version of an extension.
		/// The value in the <see cref="IOpenIdMessageExtension.TypeUri"/> property is always checked before
		/// trying this list.
		/// If you do support multiple versions of an extension using this method,
		/// consider adding a CreateResponse method to your request extension class
		/// so that the response can have the context it needs to remain compatible
		/// given the version of the extension in the request message.
		/// The <see cref="Extensions.SimpleRegistration.ClaimsRequest.CreateResponse"/> for an example.
		/// </remarks>
		IEnumerable<string> IOpenIdMessageExtension.AdditionalSupportedTypeUris {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this extension was
		/// signed by the sender.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the sender; otherwise, <c>false</c>.
		/// </value>
		bool IOpenIdMessageExtension.IsSignedByRemoteParty {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		#endregion

		#region IMessage Members

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		Version IMessage.Version {
			get {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		IDictionary<string, string> IMessage.ExtraData {
			get {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		void IMessage.EnsureValidMessage() {
			throw new NotImplementedException();
		}

		#endregion
	}
}
