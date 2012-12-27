//-----------------------------------------------------------------------
// <copyright file="UIRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using DotNetOpenAuth.Xrds;

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
	/// <see cref="IdentifierDiscoveryResult.IsExtensionSupported&lt;T&gt;()"/> method.</para>
	/// </remarks>
	[Serializable]
	public class UIRequest : IOpenIdMessageExtension, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == UIConstants.UITypeUri && isProviderRole) {
				return new UIRequest();
			}

			return null;
		};

		/// <summary>
		/// Additional type URIs that this extension is sometimes known by remote parties.
		/// </summary>
		private static readonly string[] additionalTypeUris = new string[] {
			UIConstants.LangPrefSupported,
			UIConstants.PopupSupported,
			UIConstants.IconSupported,
		};

		/// <summary>
		/// Backing store for <see cref="ExtraData"/>.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="UIRequest"/> class.
		/// </summary>
		public UIRequest() {
			this.LanguagePreference = new[] { CultureInfo.CurrentUICulture };
			this.Mode = UIModes.Popup;
		}

		/// <summary>
		/// Gets or sets the list of user's preferred languages, sorted in decreasing preferred order.
		/// </summary>
		/// <value>The default is the <see cref="CultureInfo.CurrentUICulture"/> of the thread that created this instance.</value>
		/// <remarks>
		/// The user's preferred languages as a [BCP 47] language priority list, represented as a comma-separated list of BCP 47 basic language ranges in descending priority order. For instance, the value "fr-CA,fr-FR,en-CA" represents the preference for French spoken in Canada, French spoken in France, followed by English spoken in Canada.
		/// </remarks>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design.")]
		[MessagePart("lang", AllowEmpty = false)]
		public CultureInfo[] LanguagePreference { get; set; }

		/// <summary>
		/// Gets or sets the style of UI that the RP is hosting the OP's authentication page in.
		/// </summary>
		/// <value>Some value from the <see cref="UIModes"/> class.  Defaults to <see cref="UIModes.Popup"/>.</value>
		[MessagePart("mode", AllowEmpty = false, IsRequired = true)]
		public string Mode { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Relying Party has an icon
		/// it would like the Provider to display to the user while asking them
		/// whether they would like to log in.
		/// </summary>
		/// <value><c>true</c> if the Provider should display an icon; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// By default, the Provider displays the relying party's favicon.ico.
		/// </remarks>
		[MessagePart("icon", AllowEmpty = false, IsRequired = false)]
		public bool? Icon { get; set; }

		#region IOpenIdMessageExtension Members

		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		/// <value></value>
		public string TypeUri { get { return UIConstants.UITypeUri; } }

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
		public IEnumerable<string> AdditionalSupportedTypeUris { get { return additionalTypeUris; } }

		/// <summary>
		/// Gets or sets a value indicating whether this extension was
		/// signed by the sender.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the sender; otherwise, <c>false</c>.
		/// </value>
		public bool IsSignedByRemoteParty { get; set; }

		#endregion

		#region IMessage Properties

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

		#endregion

		#region IMessage methods

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
