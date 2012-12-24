//-----------------------------------------------------------------------
// <copyright file="AccessTokenRequestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A message sent from the client to the authorization server to exchange a previously obtained grant for an access token.
	/// </summary>
	public abstract class AccessTokenRequestBase : AuthenticatedClientRequestBase, IAccessTokenRequestInternal {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenRequestBase"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		protected AccessTokenRequestBase(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		HashSet<string> IAccessTokenRequest.Scope {
			get { return this.RequestedScope; }
		}

		/// <summary>
		/// Gets a value indicating whether the client requesting the access token has authenticated itself.
		/// </summary>
		/// <value>
		/// Always true, because of our base class.
		/// </value>
		public bool ClientAuthenticated { get; internal set; }

		/// <summary>
		/// Gets or sets the result of calling the authorization server host's access token creation method.
		/// </summary>
		IAccessTokenResult IAccessTokenRequestInternal.AccessTokenResult { get; set; }

		/// <summary>
		/// Gets the username of the authorizing user, when applicable.
		/// </summary>
		/// <value>
		/// A non-empty string; or <c>null</c> when no user has authorized this access token.
		/// </value>
		public virtual string UserName {
			get {
				IAccessTokenRequestInternal request = this;
				if (request.AccessTokenResult != null && request.AccessTokenResult.AccessToken != null) {
					return request.AccessTokenResult.AccessToken.User;
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		[MessagePart(Protocol.grant_type, IsRequired = true, Encoder = typeof(GrantTypeEncoder))]
		internal abstract GrantType GrantType { get; }

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		protected abstract HashSet<string> RequestedScope { get; }

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
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();
			ErrorUtilities.VerifyProtocol(
				DotNetOpenAuthSection.Messaging.RelaxSslRequirements || this.Recipient.IsTransportSecure(),
				OAuthStrings.HttpsRequired);
		}
	}
}
