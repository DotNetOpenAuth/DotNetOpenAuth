//-----------------------------------------------------------------------
// <copyright file="OAuth2ChannelBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	using Validation;

	/// <summary>
	/// The base messaging channel used by OAuth 2.0 parties.
	/// </summary>
	internal abstract class OAuth2ChannelBase : StandardMessageFactoryChannel {
		/// <summary>
		/// The protocol versions supported by this channel.
		/// </summary>
		private static readonly Version[] Versions = Protocol.AllVersions.Select(v => v.Version).ToArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ChannelBase" /> class.
		/// </summary>
		/// <param name="messageTypes">The message types that are received by this channel.</param>
		/// <param name="channelBindingElements">The binding elements to use in sending and receiving messages.
		/// The order they are provided is used for outgoing messgaes, and reversed for incoming messages.</param>
		/// <param name="hostFactories">The host factories.</param>
		internal OAuth2ChannelBase(Type[] messageTypes, IChannelBindingElement[] channelBindingElements = null, IHostFactories hostFactories = null)
			: base(Requires.NotNull(messageTypes, "messageTypes"), Versions, hostFactories ?? new OAuth.DefaultOAuthHostFactories(), channelBindingElements ?? new IChannelBindingElement[0]) {
		}

		/// <summary>
		/// Allows preprocessing and validation of message data before an appropriate message type is
		/// selected or deserialized.
		/// </summary>
		/// <param name="fields">The received message data.</param>
		protected override void FilterReceivedFields(IDictionary<string, string> fields) {
			base.FilterReceivedFields(fields);

			// Apply the OAuth 2.0 section 2.1 requirement:
			// Parameters sent without a value MUST be treated as if they were omitted from the request.
			// The authorization server SHOULD ignore unrecognized request parameters.
			var emptyKeys = from pair in fields
							where string.IsNullOrEmpty(pair.Value)
							select pair.Key;
			foreach (string emptyKey in emptyKeys.ToList()) {
				fields.Remove(emptyKey);
			}
		}
	}
}
