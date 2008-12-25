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

		internal static void MoveTo<T>(this IList<T> list, int position, T value) {
			ErrorUtilities.VerifyInternal(list.Remove(value), "Unable to find element in list.");
			list.Insert(position, value);
		}

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
	}
}
