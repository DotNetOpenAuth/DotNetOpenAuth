//-----------------------------------------------------------------------
// <copyright file="SignedResponseRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// An indirect request from a Relying Party to a Provider where the response
	/// is expected to be signed.
	/// </summary>
	[Serializable]
	internal class SignedResponseRequest : RequestBase, IProtocolMessageWithExtensions {
		/// <summary>
		/// Backing store for the <see cref="Extensions"/> property.
		/// </summary>
		private IList<IExtensionMessage> extensions = new List<IExtensionMessage>();

		/// <summary>
		/// Initializes a new instance of the <see cref="SignedResponseRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version to use.</param>
		/// <param name="providerEndpoint">The Provider endpoint that receives this message.</param>
		/// <param name="mode">
		/// <see cref="AuthenticationRequestMode.Immediate"/> for asynchronous javascript clients;
		/// <see cref="AuthenticationRequestMode.Setup"/>  to allow the Provider to interact with the user in order to complete authentication.
		/// </param>
		internal SignedResponseRequest(Version version, Uri providerEndpoint, AuthenticationRequestMode mode) :
			base(version, providerEndpoint, GetMode(version, mode), DotNetOpenAuth.Messaging.MessageTransport.Indirect) {
		}

		#region IProtocolMessageWithExtensions Members

		/// <summary>
		/// Gets the list of extensions that are included with this message.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public IList<IExtensionMessage> Extensions {
			get { return this.extensions; }
		}

		#endregion

		/// <summary>
		/// Gets a value indicating whether the Provider is allowed to interact with the user
		/// as part of authentication.
		/// </summary>
		/// <value><c>true</c> if using OpenID immediate mode; otherwise, <c>false</c>.</value>
		internal bool Immediate {
			get { return string.Equals(this.Mode, Protocol.Args.Mode.checkid_immediate, StringComparison.Ordinal); }
		}

		/// <summary>
		/// Gets or sets the handle of the association the RP would like the Provider
		/// to use for signing a positive assertion in the response message.
		/// </summary>
		/// <value>A handle for an association between the Relying Party and the OP 
		/// that SHOULD be used to sign the response. </value>
		/// <remarks>
		/// If no association handle is sent, the transaction will take place in Stateless Mode
		/// (Verifying Directly with the OpenID Provider). 
		/// </remarks>
		[MessagePart("openid.assoc_handle", IsRequired = false, AllowEmpty = false)]
		internal string AssociationHandle { get; set; }

		/// <summary>
		/// Gets or sets the URL the Provider should redirect the user agent to following
		/// the authentication attempt.
		/// </summary>
		/// <value>URL to which the OP SHOULD return the User-Agent with the response 
		/// indicating the status of the request.</value>
		/// <remarks>
		/// <para>If this value is not sent in the request it signifies that the Relying Party 
		/// does not wish for the end user to be returned. </para>
		/// <para>The return_to URL MAY be used as a mechanism for the Relying Party to attach 
		/// context about the authentication request to the authentication response. 
		/// This document does not define a mechanism by which the RP can ensure that query 
		/// parameters are not modified by outside parties; such a mechanism can be defined 
		/// by the RP itself. </para>
		/// </remarks>
		[MessagePart("openid.return_to", IsRequired = true, AllowEmpty = false)]
		[MessagePart("openid.return_to", IsRequired = false, AllowEmpty = false, MinVersion = "2.0")]
		internal Uri ReturnTo { get; set; }

		/// <summary>
		/// Gets or sets the Relying Party discovery URL the Provider may use to verify the
		/// source of the authentication request.
		/// </summary>
		/// <value>
		/// URL pattern the OP SHOULD ask the end user to trust. See Section 9.2 (Realms). 
		/// This value MUST be sent if openid.return_to is omitted. 
		/// Default: The <see cref="ReturnTo"/> URL.
		/// </value>
		[MessagePart("openid.trust_root", IsRequired = false, AllowEmpty = false)]
		[MessagePart("openid.realm", IsRequired = false, AllowEmpty = false, MinVersion = "2.0")]
		internal Realm Realm { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the return_to value should be signed.
		/// </summary>
		internal bool SignReturnTo { get; set; }

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
		public override void EnsureValidMessage() {
			base.EnsureValidMessage();

			if (this.Realm == null) {
				// Set the default Realm per the spec if it is not explicitly given.
				this.Realm = this.ReturnTo;
			} else if (this.ReturnTo != null) {
				// Verify that the realm and return_to agree.
				ErrorUtilities.VerifyProtocol(this.Realm.Contains(this.ReturnTo), OpenIdStrings.ReturnToNotUnderRealm, this.ReturnTo, this.Realm);
			}
		}

		/// <summary>
		/// Adds parameters to the return_to querystring.
		/// </summary>
		/// <param name="keysValues">The keys=value pairs to add to the return_to query string.</param>
		/// <remarks>
		/// This method is useful if the Relying Party wants to recall some value
		/// when and if a positive assertion comes back from the Provider.
		/// </remarks>
		internal void AddReturnToArguments(IEnumerable<KeyValuePair<string, string>> keysValues) {
			Requires.NotNull(keysValues, "keysValues");
			ErrorUtilities.VerifyOperation(this.ReturnTo != null, OpenIdStrings.ReturnToRequiredForOperation);
			UriBuilder returnToBuilder = new UriBuilder(this.ReturnTo);
			returnToBuilder.AppendAndReplaceQueryArgs(keysValues);
			this.ReturnTo = returnToBuilder.Uri;
		}

		/// <summary>
		/// Adds a parameter to the return_to querystring.
		/// </summary>
		/// <param name="key">The name of the parameter.</param>
		/// <param name="value">The value of the argument.</param>
		/// <remarks>
		/// This method is useful if the Relying Party wants to recall some value
		/// when and if a positive assertion comes back from the Provider.
		/// </remarks>
		internal void AddReturnToArguments(string key, string value) {
			var pair = new KeyValuePair<string, string>(key, value);
			this.AddReturnToArguments(new[] { pair });
		}

		/// <summary>
		/// Gets the value of the openid.mode parameter based on the protocol version and immediate flag.
		/// </summary>
		/// <param name="version">The OpenID version to use.</param>
		/// <param name="mode">
		/// <see cref="AuthenticationRequestMode.Immediate"/> for asynchronous javascript clients;
		/// <see cref="AuthenticationRequestMode.Setup"/>  to allow the Provider to interact with the user in order to complete authentication.
		/// </param>
		/// <returns>checkid_immediate or checkid_setup</returns>
		private static string GetMode(Version version, AuthenticationRequestMode mode) {
			Requires.NotNull(version, "version");

			Protocol protocol = Protocol.Lookup(version);
			return mode == AuthenticationRequestMode.Immediate ? protocol.Args.Mode.checkid_immediate : protocol.Args.Mode.checkid_setup;
		}
	}
}
