//-----------------------------------------------------------------------
// <copyright file="UIRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.UI {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// OpenID User Interface extension 1.0 request message.
	/// </summary>
	/// <remarks>
	/// 	<para>Implements the extension described by: http://wiki.openid.net/f/openid_ui_extension_draft01.html </para>
	/// 	<para>This extension only applies to checkid_setup requests, since checkid_immediate requests display
	/// no UI to the user. </para>
	/// 	<para>For rules about how the popup window should be displayed, please see the documentation of
	/// <see cref="UIModes.Popup"/>. </para>
	/// 	<para>An RP may determine whether an arbitrary OP supports this extension (and thereby determine
	/// whether to use a standard full window redirect or a popup) via the
	/// <see cref="IProviderEndpoint.IsExtensionSupported"/> method on the <see cref="IAuthenticationRequest.Provider"/>
	/// object.</para>
	/// </remarks>
	public sealed class UIRequest : IOpenIdMessageExtension, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == UITypeUri && isProviderRole) {
				return new UIRequest();
			}

			return null;
		};

		/// <summary>
		/// The type URI associated with this extension.
		/// </summary>
		private const string UITypeUri = "http://specs.openid.net/extensions/ui/1.0";

		/// <summary>
		/// Backing store for <see cref="ExtraData"/>.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="UIRequest"/> class.
		/// </summary>
		public UIRequest() {
			this.LanguagePreference = CultureInfo.CurrentUICulture;
		}

		/// <summary>
		/// Gets or sets the user's preferred language.
		/// </summary>
		/// <value>The default is the <see cref="CultureInfo.CurrentUICulture"/> of the thread that created this instance.</value>
		/// <remarks>
		/// The user's preferred language, reusing the Language Tag format used by the [Language Preference Attribute] (axschema.org, “Language Preference Attribute,” .)  for [OpenID Attribute Exchange] (Hardt, D., Bufu, J., and J. Hoyt, “OpenID Attribute Exchange 1.0,” .)  and defined in [RFC4646] (Phillips, A. and M. Davis, “Tags for Identifying Languages,” .). For example "en-US" represents the English language as spoken in the United States, and "fr-CA" represents the French language spoken in Canada. 
		/// </remarks>
		[MessagePart("lang", AllowEmpty = false)]
		public CultureInfo LanguagePreference { get; set; }

		/// <summary>
		/// Gets the style of UI that the RP is hosting the OP's authentication page in.
		/// </summary>
		/// <value>Some value from the <see cref="UIModes"/> class.  Defaults to <see cref="UIModes.Popup"/>.</value>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Design is to allow this later to be changable when more than one value exists.")]
		[MessagePart("mode", AllowEmpty = false, IsRequired = true)]
		public string Mode { get { return UIModes.Popup; } }

		#region IOpenIdMessageExtension Members

		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		/// <value></value>
		public string TypeUri { get { return UITypeUri; } }

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
		public IEnumerable<string> AdditionalSupportedTypeUris { get { return Enumerable.Empty<string>(); } }

		/// <summary>
		/// Gets or sets a value indicating whether this extension was
		/// signed by the sender.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the sender; otherwise, <c>false</c>.
		/// </value>
		public bool IsSignedByRemoteParty { get; set; }

		#endregion

		#region IMessage Members

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		/// <value>The value 1.0.</value>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public Version Version {
			get { return new Version(1, 0); }
		}

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
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
		public void EnsureValidMessage() {
		}

		#endregion

		#region IMessageWithEvents Members

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		public void OnSending() {
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		public void OnReceiving() {
			if (this.LanguagePreference != null) {
				// TODO: see if we can change the CultureInfo.CurrentUICulture somehow
			}
		}

		#endregion
	}
}
