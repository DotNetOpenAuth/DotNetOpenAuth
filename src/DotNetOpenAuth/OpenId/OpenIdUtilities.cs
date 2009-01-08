//-----------------------------------------------------------------------
// <copyright file="OpenIdUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// A set of utilities especially useful to OpenID.
	/// </summary>
	internal static class OpenIdUtilities {
		/// <summary>
		/// Gets the OpenID protocol instance for the version in a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The OpenID protocol instance.</returns>
		internal static Protocol GetProtocol(this IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			return Protocol.Lookup(message.Version);
		}

		/// <summary>
		/// Changes the position of some element in a list.
		/// </summary>
		/// <typeparam name="T">The type of elements stored in the list.</typeparam>
		/// <param name="list">The list to be modified.</param>
		/// <param name="position">The new position for the given element.</param>
		/// <param name="value">The element to move within the list.</param>
		/// <exception cref="InternalErrorException">Thrown if the element does not already exist in the list.</exception>
		internal static void MoveTo<T>(this IList<T> list, int position, T value) {
			ErrorUtilities.VerifyInternal(list.Remove(value), "Unable to find element in list.");
			list.Insert(position, value);
		}

		/// <summary>
		/// Initializes the private secret if it has not yet been set.
		/// </summary>
		/// <param name="secretStore">The secret store.</param>
		internal static void InitializeSecretIfUnset(this IPrivateSecretStore secretStore) {
			ErrorUtilities.VerifyArgumentNotNull(secretStore, "secretStore");

			if (secretStore.PrivateSecret == null) {
				secretStore.PrivateSecret = MessagingUtilities.GetCryptoRandomData(ReturnToSignatureBindingElement.OptimalPrivateSecretLength);

				// Log that we created a new private secret.
				// If this happens frequently, it's a sign that the store for this secret is not
				// properly saving the value, and the result will be slower performance for 
				// Relying Parties (at best) and failed authentications (at worst).
				Logger.Info("Generated and saved private secret.  This should generally happen only at web application initialization time.");
			}
		}

		/// <summary>
		/// Corrects any URI decoding the Provider may have inappropriately done
		/// to our return_to URL, resulting in an otherwise corrupted base64 token.
		/// </summary>
		/// <param name="token">The token, which MAY have been corrupted by an extra URI decode.</param>
		/// <returns>The token; corrected if corruption had occurred.</returns>
		/// <remarks>
		/// AOL may have incorrectly URI-decoded the token for us in the return_to, 
		/// resulting in a token URI-decoded twice by the time we see it, and no
		/// longer being a valid base64 string.
		/// It turns out that the only symbols from base64 that is also encoded
		/// in URI encoding rules are the + and / characters.
		/// AOL decodes the %2b sequence to the + character 
		/// and the %2f sequence to the / character (it shouldn't decode at all).
		/// When we do our own URI decoding, the + character becomes a space (corrupting base64)
		/// but the / character remains a /, so no further corruption happens to this character.
		/// So to correct this we just need to change any spaces we find in the token
		/// back to + characters.
		/// </remarks>
		internal static string FixDoublyUriDecodedBase64String(string value) {
			ErrorUtilities.VerifyArgumentNotNull(value, "value");

			if (value.Contains(" ")) {
				Logger.Error("Deserializing a corrupted token.  The OpenID Provider may have inappropriately decoded the return_to URL before sending it back to us.");
				value = value.Replace(' ', '+'); // Undo any extra decoding the Provider did
			}

			return value;
		}
	}
}
