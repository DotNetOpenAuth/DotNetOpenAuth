//-----------------------------------------------------------------------
// <copyright file="ExtensionBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// A handy base class for built-in extensions.
	/// </summary>
	[Serializable]
	public class ExtensionBase : IOpenIdMessageExtension {
		/// <summary>
		/// Backing store for the <see cref="IOpenIdMessageExtension.TypeUri"/> property.
		/// </summary>
		private string typeUri;

		/// <summary>
		/// Backing store for the <see cref="IOpenIdMessageExtension.AdditionalSupportedTypeUris"/> property.
		/// </summary>
		private IEnumerable<string> additionalSupportedTypeUris;

		/// <summary>
		/// Backing store for the <see cref="IMessage.ExtraData"/> property.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionBase"/> class.
		/// </summary>
		/// <param name="version">The version of the extension.</param>
		/// <param name="typeUri">The type URI to use in the OpenID message.</param>
		/// <param name="additionalSupportedTypeUris">The additional supported type URIs by which this extension might be recognized.  May be null.</param>
		protected ExtensionBase(Version version, string typeUri, IEnumerable<string> additionalSupportedTypeUris) {
			this.Version = version;
			this.typeUri = typeUri;
			this.additionalSupportedTypeUris = additionalSupportedTypeUris ?? EmptyList<string>.Instance;
		}

		#region IOpenIdProtocolMessageExtension Members

		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		string IOpenIdMessageExtension.TypeUri {
			get { return this.TypeUri; }
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
		/// The <see cref="SimpleRegistration.ClaimsRequest.CreateResponse"/> for an example.
		/// </remarks>
		IEnumerable<string> IOpenIdMessageExtension.AdditionalSupportedTypeUris {
			get { return this.AdditionalSupportedTypeUris; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this extension was
		/// signed by the OpenID Provider.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the provider; otherwise, <c>false</c>.
		/// </value>
		bool IOpenIdMessageExtension.IsSignedByRemoteParty {
			get { return this.IsSignedByRemoteParty; }
			set { this.IsSignedByRemoteParty = value; }
		}

		#endregion

		#region IMessage Properties

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		IDictionary<string, string> IMessage.ExtraData {
			get { return this.ExtraData; }
		}

		#endregion

		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		protected string TypeUri {
			get { return this.typeUri; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this extension was
		/// signed by the OpenID Provider.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the provider; otherwise, <c>false</c>.
		/// </value>
		protected bool IsSignedByRemoteParty { get; set; }

		/// <summary>
		/// Gets the additional TypeURIs that are supported by this extension, in preferred order.
		/// May be empty if none other than <see cref="IOpenIdMessageExtension.TypeUri"/> is supported, but
		/// should not be null.
		/// </summary>
		/// <value></value>
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
		protected IEnumerable<string> AdditionalSupportedTypeUris {
			get { return this.additionalSupportedTypeUris; }
		}

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		protected IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		#region IMessage Methods

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
			this.EnsureValidMessage();
		}

		#endregion

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
		protected virtual void EnsureValidMessage() {
		}
	}
}
