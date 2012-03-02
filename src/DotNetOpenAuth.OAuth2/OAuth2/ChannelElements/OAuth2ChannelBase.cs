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

	/// <summary>
	/// The base messaging channel used by OAuth 2.0 parties.
	/// </summary>
	internal abstract class OAuth2ChannelBase : StandardMessageFactoryChannel {
		/// <summary>
		/// The messages receivable by this channel.
		/// </summary>
		private static readonly Type[] MessageTypes = new Type[] {
			typeof(AccessTokenRefreshRequest),
			typeof(AccessTokenAuthorizationCodeRequest),
			typeof(AccessTokenResourceOwnerPasswordCredentialsRequest),
			typeof(AccessTokenClientCredentialsRequest),
			typeof(AccessTokenSuccessResponse),
			typeof(AccessTokenFailedResponse),
			typeof(EndUserAuthorizationRequest),
			typeof(EndUserAuthorizationImplicitRequest),
			typeof(EndUserAuthorizationSuccessAuthCodeResponse),
			typeof(EndUserAuthorizationSuccessAccessTokenResponse),
			typeof(EndUserAuthorizationFailedResponse),
			typeof(UnauthorizedResponse),
		};

		/// <summary>
		/// The protocol versions supported by this channel.
		/// </summary>
		private static readonly Version[] Versions = Protocol.AllVersions.Select(v => v.Version).ToArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ChannelBase"/> class.
		/// </summary>
		/// <param name="channelBindingElements">The channel binding elements.</param>
		internal OAuth2ChannelBase(params IChannelBindingElement[] channelBindingElements)
			: base(MessageTypes, Versions, channelBindingElements) {
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
							where String.IsNullOrEmpty(pair.Value)
							select pair.Key;
			foreach (string emptyKey in emptyKeys.ToList()) {
				fields.Remove(emptyKey);
			}
		}
	}
}
