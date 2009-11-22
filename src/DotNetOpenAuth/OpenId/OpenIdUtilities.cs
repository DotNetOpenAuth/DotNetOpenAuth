//-----------------------------------------------------------------------
// <copyright file="OpenIdUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.DiscoveryServices;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// A set of utilities especially useful to OpenID.
	/// </summary>
	public static class OpenIdUtilities {
		/// <summary>
		/// The prefix to designate this library's proprietary parameters added to the protocol.
		/// </summary>
		internal const string CustomParameterPrefix = "dnoa.";

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <typeparam name="T">The extension whose support is being queried.</typeparam>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No parameter at all.")]
		public static bool IsExtensionSupported<T>(this IProviderEndpoint providerEndpoint) where T : IOpenIdMessageExtension, new() {
			Contract.Requires(providerEndpoint != null);
			T extension = new T();
			return IsExtensionSupported(providerEndpoint, extension);
		}

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="extensionType">The extension whose support is being queried.</param>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		public static bool IsExtensionSupported(this IProviderEndpoint providerEndpoint, Type extensionType) {
			Contract.Requires(providerEndpoint != null);
			Contract.Requires(extensionType != null);
			Contract.Requires<ArgumentException>(typeof(IOpenIdMessageExtension).IsAssignableFrom(extensionType));
			var extension = (IOpenIdMessageExtension)Activator.CreateInstance(extensionType);
			return IsExtensionSupported(providerEndpoint, extension);
		}

		/// <summary>
		/// Determines whether a given extension is supported by this endpoint.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="extension">An instance of the extension to check support for.</param>
		/// <returns>
		/// 	<c>true</c> if the extension is supported by this endpoint; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsExtensionSupported(this IProviderEndpoint providerEndpoint, IOpenIdMessageExtension extension) {
			Contract.Requires(providerEndpoint != null);
			Contract.Requires<ArgumentNullException>(extension != null);

			// Consider the primary case.
			if (providerEndpoint.IsTypeUriPresent(extension.TypeUri)) {
				return true;
			}

			// Consider the secondary cases.
			if (extension.AdditionalSupportedTypeUris != null) {
				if (extension.AdditionalSupportedTypeUris.Any(typeUri => providerEndpoint.IsTypeUriPresent(typeUri))) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the OpenID protocol instance for the version in a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The OpenID protocol instance.</returns>
		internal static Protocol GetProtocol(this IProtocolMessage message) {
			Contract.Requires<ArgumentNullException>(message != null);
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
				Logger.OpenId.Error("Deserializing a corrupted token.  The OpenID Provider may have inappropriately decoded the return_to URL before sending it back to us.");
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
		/// <param name="requestContext">The request context.</param>
		/// <returns>The fully-qualified realm.</returns>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DotNetOpenAuth.OpenId.Realm", Justification = "Using ctor for validation.")]
		internal static UriBuilder GetResolvedRealm(Page page, string realm, HttpRequestInfo requestContext) {
			Contract.Requires<ArgumentNullException>(page != null);
			Contract.Requires<ArgumentNullException>(requestContext != null);

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
				new Uri(requestContext.UrlBeforeRewriting, page.ResolveUrl(realmNoWildcard)));

			if (foundWildcard) {
				fullyQualifiedRealm.Host = "*." + fullyQualifiedRealm.Host;
			}

			// Is it valid?
			new Realm(fullyQualifiedRealm); // throws if not valid

			return fullyQualifiedRealm;
		}

		/// <summary>
		/// Gets the extension factories from the extension aggregator on an OpenID channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <returns>The list of factories that will be used to generate extension instances.</returns>
		/// <remarks>
		/// This is an extension method on <see cref="Channel"/> rather than an instance
		/// method on <see cref="OpenIdChannel"/> because the <see cref="OpenIdRelyingParty"/>
		/// and <see cref="OpenIdProvider"/> classes don't strong-type to <see cref="OpenIdChannel"/>
		/// to allow flexibility in the specific type of channel the user (or tests)
		/// can plug in.
		/// </remarks>
		internal static IList<IOpenIdExtensionFactory> GetExtensionFactories(this Channel channel) {
			Contract.Requires<ArgumentNullException>(channel != null);

			var extensionsBindingElement = channel.BindingElements.OfType<ExtensionsBindingElement>().SingleOrDefault();
			ErrorUtilities.VerifyOperation(extensionsBindingElement != null, OpenIdStrings.UnsupportedChannelConfiguration);
			IOpenIdExtensionFactory factory = extensionsBindingElement.ExtensionFactory;
			var aggregator = factory as OpenIdExtensionFactoryAggregator;
			ErrorUtilities.VerifyOperation(aggregator != null, OpenIdStrings.UnsupportedChannelConfiguration);
			return aggregator.Factories;
		}

		/// <summary>
		/// Gets the OpenID protocol used by the Provider.
		/// </summary>
		/// <param name="providerDescription">The provider description.</param>
		/// <returns>The OpenID protocol.</returns>
		internal static Protocol GetProtocol(this IProviderEndpoint providerDescription) {
			return Protocol.Lookup(providerDescription.Version);
		}

		/// <summary>
		/// Gets the value for the <see cref="IAuthenticationResponse.FriendlyIdentifierForDisplay"/> property.
		/// </summary>
		/// <param name="discoveryResult">The discovery result.</param>
		/// <returns>A human-readable, abbreviated (but not secure) identifier the user MAY recognize as his own.</returns>
		internal static string GetFriendlyIdentifierForDisplay(this IIdentifierDiscoveryResult discoveryResult) {
			Contract.Requires(discoveryResult != null);
			XriIdentifier xri = discoveryResult.ClaimedIdentifier as XriIdentifier;
			UriIdentifier uri = discoveryResult.ClaimedIdentifier as UriIdentifier;
			if (xri != null) {
				if (discoveryResult.UserSuppliedIdentifier == null || String.Equals(discoveryResult.UserSuppliedIdentifier, discoveryResult.ClaimedIdentifier, StringComparison.OrdinalIgnoreCase)) {
					return discoveryResult.ClaimedIdentifier;
				} else {
					return discoveryResult.UserSuppliedIdentifier;
				}
			} else if (uri != null) {
				if (uri != discoveryResult.ProviderEndpoint.GetProtocol().ClaimedIdentifierForOPIdentifier) {
					string displayUri = uri.Uri.Host + uri.Uri.AbsolutePath;
					displayUri = displayUri.TrimEnd('/');

					// Multi-byte unicode characters get encoded by the Uri class for transit.
					// Since discoveryResult is for display purposes, we want to reverse discoveryResult and display a readable
					// representation of these foreign characters.  
					return Uri.UnescapeDataString(displayUri);
				}
			} else {
				return discoveryResult.ClaimedIdentifier;
			}

			return null;
		}

		/// <summary>
		/// Determines whether a given type URI is present on the specified provider endpoint.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="typeUri">The type URI.</param>
		/// <returns>
		/// 	<c>true</c> if the type URI is present on the specified provider endpoint; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsTypeUriPresent(this IProviderEndpoint providerEndpoint, string typeUri) {
			Contract.Requires(providerEndpoint != null);
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(typeUri));
			return providerEndpoint.Capabilities.Contains(typeUri);
		}
	}
}
