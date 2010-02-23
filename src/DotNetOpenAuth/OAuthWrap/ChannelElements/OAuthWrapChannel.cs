//-----------------------------------------------------------------------
// <copyright file="OAuthWrapChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// The channel for the OAuth WRAP protocol.
	/// </summary>
	internal class OAuthWrapChannel : Channel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthWrapChannel"/> class.
		/// </summary>
		protected internal OAuthWrapChannel()
			: base(new StandardMessageFactory()) {
			((StandardMessageFactory)this.MessageFactory).AddMessageTypes(GetWrapMessageDescriptions(this.MessageDescriptions));
		}

		/// <summary>
		/// Gets or sets the message descriptions.
		/// </summary>
		internal override MessageDescriptionCollection MessageDescriptions {
			get {
				return base.MessageDescriptions;
			}

			set {
				base.MessageDescriptions = value;

				// We must reinitialize the message factory so it can use the new message descriptions.
				var factory = new StandardMessageFactory();
				factory.AddMessageTypes(GetWrapMessageDescriptions(value));
				this.MessageFactory = factory;
			}
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <returns>
		/// The pending user agent redirect based message to be sent as an HttpResponse.
		/// </returns>
		/// <remarks>
		/// This method implements spec OAuth V1.0 section 5.3.
		/// </remarks>
		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the message types that come standard with OAuth WRAP.
		/// </summary>
		/// <param name="descriptionsCache">The descriptions cache from which to draw.</param>
		/// <returns>A collection of WRAP message types.</returns>
		private static IEnumerable<MessageDescription> GetWrapMessageDescriptions(MessageDescriptionCollection descriptionsCache) {
			Contract.Requires<ArgumentNullException>(descriptionsCache != null);
			Contract.Ensures(Contract.Result<IEnumerable<MessageDescription>>() != null);

			var messageTypes = new Type[] {
				typeof(Messages.RefreshAccessTokenRequest),
				typeof(Messages.RefreshAccessTokenSuccessResponse),
				typeof(Messages.RefreshAccessTokenFailedResponse),
				typeof(Messages.UnauthorizedResponse),
				typeof(Messages.AssertionRequest),
				typeof(Messages.AssertionSuccessResponse),
				typeof(Messages.AssertionFailedResponse),
				typeof(Messages.ClientAccountUsernamePasswordRequest),
				typeof(Messages.ClientAccountUsernamePasswordSuccessResponse),
				typeof(Messages.ClientAccountUsernamePasswordFailedResponse),
				typeof(Messages.RichAppRequest),
				typeof(Messages.RichAppResponse),
				typeof(Messages.RichAppAccessTokenRequest),
				typeof(Messages.RichAppAccessTokenSuccessResponse),
				typeof(Messages.RichAppAccessTokenFailedResponse),
				typeof(Messages.UserNamePasswordRequest),
				typeof(Messages.UserNamePasswordSuccessResponse),
				typeof(Messages.UserNamePasswordVerificationResponse),
				typeof(Messages.UserNamePasswordFailedResponse),
				typeof(Messages.UsernamePasswordCaptchaResponse),
				typeof(Messages.WebAppRequest),
				typeof(Messages.WebAppSuccessResponse),
				typeof(Messages.WebAppFailedResponse),
				typeof(Messages.WebAppAccessTokenRequest),
				typeof(Messages.WebAppAccessTokenSuccessResponse),
				typeof(Messages.WebAppAccessTokenBadClientResponse),
				typeof(Messages.WebAppAccessTokenFailedResponse),
			};

			// Get all the MessageDescription objects through the standard cache,
			// so that perhaps it will be a quick lookup, or at least it will be
			// stored there for a quick lookup later.
			var messageDescriptions = new List<MessageDescription>(messageTypes.Length * Protocol.AllVersions.Count);
			foreach (Protocol protocol in Protocol.AllVersions) {
				foreach (Type messageType in messageTypes) {
					messageDescriptions.Add(descriptionsCache.Get(messageType, protocol.Version));
				}
			}

			return messageDescriptions;
		}
	}
}
