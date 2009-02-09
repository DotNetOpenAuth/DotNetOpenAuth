//-----------------------------------------------------------------------
// <copyright file="IndirectResponseBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A common base class from which indirect response messages should derive.
	/// </summary>
	internal class IndirectResponseBase : RequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectResponseBase"/> class.
		/// </summary>
		/// <param name="request">The request that caused this response message to be constructed.</param>
		/// <param name="mode">The value of the openid.mode parameter.</param>
		protected IndirectResponseBase(SignedResponseRequest request, string mode)
			: base(GetVersion(request), GetReturnTo(request), mode, MessageTransport.Indirect) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			this.OriginatingRequest = request;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectResponseBase"/> class
		/// for unsolicited assertion scenarios.
		/// </summary>
		/// <param name="version">The OpenID version supported at the Relying Party.</param>
		/// <param name="relyingPartyReturnTo">
		/// The URI at which the Relying Party receives OpenID indirect messages.
		/// </param>
		/// <param name="mode">The value to use for the openid.mode parameter.</param>
		protected IndirectResponseBase(Version version, Uri relyingPartyReturnTo, string mode)
			: base(version, relyingPartyReturnTo, mode, MessageTransport.Indirect) {
		}

		/// <summary>
		/// Gets the originating request message, if applicable.
		/// </summary>
		protected SignedResponseRequest OriginatingRequest { get; private set; }

		/// <summary>
		/// Gets the <see cref="IMessage.Version"/> property of a message.
		/// </summary>
		/// <param name="message">The message to fetch the protocol version from.</param>
		/// <returns>The value of the <see cref="IMessage.Version"/> property.</returns>
		/// <remarks>
		/// This method can be used by a constructor to throw an <see cref="ArgumentNullException"/>
		/// instead of a <see cref="NullReferenceException"/>.
		/// </remarks>
		internal static Version GetVersion(IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			return message.Version;
		}

		/// <summary>
		/// Gets the <see cref="SignedResponseRequest.ReturnTo"/> property of a message.
		/// </summary>
		/// <param name="message">The message to fetch the ReturnTo from.</param>
		/// <returns>The value of the <see cref="SignedResponseRequest.ReturnTo"/> property.</returns>
		/// <remarks>
		/// This method can be used by a constructor to throw an <see cref="ArgumentNullException"/>
		/// instead of a <see cref="NullReferenceException"/>.
		/// </remarks>
		private static Uri GetReturnTo(SignedResponseRequest message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			ErrorUtilities.VerifyProtocol(message.ReturnTo != null, OpenIdStrings.ReturnToRequiredForResponse);
			return message.ReturnTo;
		}
	}
}
