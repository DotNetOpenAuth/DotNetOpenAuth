//-----------------------------------------------------------------------
// <copyright file="EndUserAuthorizationResponseTypeEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Encodes/decodes the OAuth 2.0 response_type argument.
	/// </summary>
	internal class EndUserAuthorizationResponseTypeEncoder : IMessagePartEncoder {
		/// <summary>
		/// Initializes a new instance of the <see cref="EndUserAuthorizationResponseTypeEncoder"/> class.
		/// </summary>
		public EndUserAuthorizationResponseTypeEncoder() {
		}

		#region IMessagePartEncoder Members

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		public string Encode(object value) {
			var responseType = (EndUserAuthorizationResponseType)value;
			switch (responseType)
			{
				case EndUserAuthorizationResponseType.AccessToken:
					return Protocol.ResponseTypes.Token;
				case EndUserAuthorizationResponseType.AuthorizationCode:
					return Protocol.ResponseTypes.Code;
				default:
					throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.response_type, value);
			}
		}

		/// <summary>
		/// Decodes the specified value.
		/// </summary>
		/// <param name="value">The string value carried by the transport.  Guaranteed to never be null, although it may be empty.</param>
		/// <returns>
		/// The deserialized form of the given string.
		/// </returns>
		/// <exception cref="FormatException">Thrown when the string value given cannot be decoded into the required object type.</exception>
		public object Decode(string value) {
			switch (value) {
				case Protocol.ResponseTypes.Token:
					return EndUserAuthorizationResponseType.AccessToken;
				case Protocol.ResponseTypes.Code:
					return EndUserAuthorizationResponseType.AuthorizationCode;
				default:
					throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.response_type, value);
			}
		}

		#endregion
	}
}
