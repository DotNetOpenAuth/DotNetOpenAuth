//-----------------------------------------------------------------------
// <copyright file="CheckAuthenticationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using Validation;

	/// <summary>
	/// A message a Relying Party sends to a Provider to confirm the validity
	/// of a positive assertion that was signed by a Provider-only secret.
	/// </summary>
	/// <remarks>
	/// The significant payload of this message depends entirely upon the
	/// assertion message, and therefore is all in the 
	/// <see cref="DotNetOpenAuth.Messaging.IMessage.ExtraData"/> property bag.
	/// </remarks>
	internal class CheckAuthenticationRequest : RequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckAuthenticationRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		internal CheckAuthenticationRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint, GetProtocolConstant(version, p => p.Args.Mode.check_authentication), MessageTransport.Direct) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CheckAuthenticationRequest"/> class
		/// based on the contents of some signed message whose signature must be verified.
		/// </summary>
		/// <param name="message">The message whose signature should be verified.</param>
		/// <param name="channel">The channel.  This is used only within the constructor and is not stored in a field.</param>
		internal CheckAuthenticationRequest(IndirectSignedResponse message, Channel channel)
			: base(message.Version, message.ProviderEndpoint, GetProtocolConstant(message.Version, p => p.Args.Mode.check_authentication), MessageTransport.Direct) {
			Requires.NotNull(channel, "channel");

			// Copy all message parts from the id_res message into this one,
			// except for the openid.mode parameter.
			MessageDictionary checkPayload = channel.MessageDescriptions.GetAccessor(message, true);
			MessageDictionary thisPayload = channel.MessageDescriptions.GetAccessor(this);
			foreach (var pair in checkPayload) {
				if (!string.Equals(pair.Key, this.Protocol.openid.mode)) {
					thisPayload[pair.Key] = pair.Value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the signature being verified by this request
		/// is in fact valid.
		/// </summary>
		/// <value><c>true</c> if the signature is valid; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// This property is automatically set as the message is received by the channel's
		/// signing binding element.
		/// </remarks>
		internal bool IsValid { get; set; }

		/// <summary>
		/// Gets or sets the ReturnTo that existed in the original signed message.
		/// </summary>
		/// <remarks>
		/// This exists strictly for convenience in recreating the <see cref="IndirectSignedResponse"/>
		/// message.
		/// </remarks>
		[MessagePart("openid.return_to", IsRequired = true, AllowEmpty = false, Encoder = typeof(OriginalStringUriEncoder))]
		[MessagePart("openid.return_to", IsRequired = false, AllowEmpty = false, MinVersion = "2.0", Encoder = typeof(OriginalStringUriEncoder))]
		internal Uri ReturnTo { get; set; }
	}
}
