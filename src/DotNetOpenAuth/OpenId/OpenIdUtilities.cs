//-----------------------------------------------------------------------
// <copyright file="OpenIdUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web.UI;
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
		/// Corrects any URI decoding the Provider may have inappropriately done
		/// to our return_to URL, resulting in an otherwise corrupted base64 encoded value.
		/// </summary>
		/// <param name="value">The base64 encoded value.  May be null.</param>
		/// <returns>
		/// The value; corrected if corruption had occurred.
		/// </returns>
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
			if (value == null) {
				return null;
			}

			if (value.Contains(" ")) {
				Logger.Error("Deserializing a corrupted token.  The OpenID Provider may have inappropriately decoded the return_to URL before sending it back to us.");
				value = value.Replace(' ', '+'); // Undo any extra decoding the Provider did
			}

			return value;
		}

		/// <summary>
		/// Rounds the given <see cref="DateTime"/> downward to the whole second.
		/// </summary>
		/// <param name="dateTime">The DateTime object to adjust.</param>
		/// <returns>The new <see cref="DateTime"/> value.</returns>
		internal static DateTime CutToSecond(DateTime dateTime) {
			return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
		}

		/// <summary>
		/// Gets the fully qualified Realm URL, given a Realm that may be relative to a particular page.
		/// </summary>
		/// <param name="page">The hosting page that has the realm value to resolve.</param>
		/// <param name="realm">The realm, which may begin with "*." or "~/".</param>
		/// <returns>The fully-qualified realm.</returns>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenAuth.OpenId.Realm", Justification = "Using ctor for validation.")]
		internal static UriBuilder GetResolvedRealm(Page page, string realm) {
			ErrorUtilities.VerifyArgumentNotNull(page, "page");

			// Allow for *. realm notation, as well as ASP.NET ~/ shortcuts.

			// We have to temporarily remove the *. notation if it's there so that
			// the rest of our URL manipulation will succeed.
			bool foundWildcard = false;

			// Note: we don't just use string.Replace because poorly written URLs
			// could potentially have multiple :// sequences in them.
			MatchEvaluator matchDelegate = delegate(Match m) {
				foundWildcard = true;
				return m.Groups[1].Value;
			};
			string realmNoWildcard = Regex.Replace(realm, @"^(\w+://)\*\.", matchDelegate);

			UriBuilder fullyQualifiedRealm = new UriBuilder(
				new Uri(MessagingUtilities.GetRequestUrlFromContext(), page.ResolveUrl(realmNoWildcard)));

			if (foundWildcard) {
				fullyQualifiedRealm.Host = "*." + fullyQualifiedRealm.Host;
			}

			// Is it valid?
			new Realm(fullyQualifiedRealm); // throws if not valid

			return fullyQualifiedRealm;
		}
	}
}
