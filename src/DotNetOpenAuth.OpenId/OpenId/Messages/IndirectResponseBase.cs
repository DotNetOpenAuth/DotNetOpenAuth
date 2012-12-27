//-----------------------------------------------------------------------
// <copyright file="IndirectResponseBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A common base class from which indirect response messages should derive.
	/// </summary>
	[Serializable]
	internal class IndirectResponseBase : RequestBase, IProtocolMessageWithExtensions {
		/// <summary>
		/// Backing store for the <see cref="Extensions"/> property.
		/// </summary>
		private IList<IExtensionMessage> extensions = new List<IExtensionMessage>();

		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectResponseBase"/> class.
		/// </summary>
		/// <param name="request">The request that caused this response message to be constructed.</param>
		/// <param name="mode">The value of the openid.mode parameter.</param>
		protected IndirectResponseBase(SignedResponseRequest request, string mode)
			: base(GetVersion(request), GetReturnTo(request), mode, MessageTransport.Indirect) {
			Requires.NotNull(request, "request");

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
		/// Gets the signed extensions on this message.
		/// </summary>
		internal IEnumerable<IOpenIdMessageExtension> SignedExtensions {
			get { return this.extensions.OfType<IOpenIdMessageExtension>().Where(ext => ext.IsSignedByRemoteParty); }
		}

		/// <summary>
		/// Gets the unsigned extensions on this message.
		/// </summary>
		internal IEnumerable<IOpenIdMessageExtension> UnsignedExtensions {
			get { return this.extensions.OfType<IOpenIdMessageExtension>().Where(ext => !ext.IsSignedByRemoteParty); }
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
			Requires.NotNull(message, "message");
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
			Requires.NotNull(message, "message");
			ErrorUtilities.VerifyProtocol(message.ReturnTo != null, OpenIdStrings.ReturnToRequiredForResponse);
			return message.ReturnTo;
		}
	}
}
