//-----------------------------------------------------------------------
// <copyright file="ScopeEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Encodes or decodes a set of scopes into the OAuth 2.0 scope message part.
	/// </summary>
	internal class ScopeEncoder : IMessagePartEncoder {
		/// <summary>
		/// Initializes a new instance of the <see cref="ScopeEncoder"/> class.
		/// </summary>
		public ScopeEncoder() {
		}

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		public string Encode(object value) {
			var scopes = (IEnumerable<string>)value;
			ErrorUtilities.VerifyProtocol(!scopes.Any(scope => scope.Contains(" ")), OAuthStrings.ScopesMayNotContainSpaces);
			return (scopes != null && scopes.Any()) ? string.Join(" ", scopes.ToArray()) : null;
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
			return OAuthUtilities.SplitScopes(value);
		}
	}
}
