//-----------------------------------------------------------------------
// <copyright file="GrantTypeEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Encodes/decodes the OAuth 2.0 grant_type argument.
	/// </summary>
	internal class GrantTypeEncoder : IMessagePartEncoder {
		/// <summary>
		/// Initializes a new instance of the <see cref="GrantTypeEncoder"/> class.
		/// </summary>
		public GrantTypeEncoder() {
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
			var responseType = (GrantType)value;
			switch (responseType)
			{
				case GrantType.ClientCredentials:
					return Protocol.GrantTypes.ClientCredentials;
				case GrantType.AuthorizationCode:
					return Protocol.GrantTypes.AuthorizationCode;
					case GrantType.RefreshToken:
					return Protocol.GrantTypes.RefreshToken;
				case GrantType.Password:
					return Protocol.GrantTypes.Password;
				case GrantType.Assertion:
					return Protocol.GrantTypes.Assertion;
				default:
					throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.grant_type, value);
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
				case Protocol.GrantTypes.ClientCredentials:
					return GrantType.ClientCredentials;
				case Protocol.GrantTypes.Assertion:
					return GrantType.Assertion;
				case Protocol.GrantTypes.Password:
					return GrantType.Password;
				case Protocol.GrantTypes.RefreshToken:
					return GrantType.RefreshToken;
				case Protocol.GrantTypes.AuthorizationCode:
					return GrantType.AuthorizationCode;
				default:
					throw ErrorUtilities.ThrowFormat(MessagingStrings.UnexpectedMessagePartValue, Protocol.grant_type, value);
			}
		}

		#endregion
	}
}
