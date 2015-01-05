//-----------------------------------------------------------------------
// <copyright file="OpenIdUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Cache;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.UI;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.RelyingParty;

	using Org.Mentalis.Security.Cryptography;
	using Validation;

	/// <summary>
	/// A set of utilities especially useful to OpenID.
	/// </summary>
	public static class OpenIdUtilities {
		/// <summary>
		/// The prefix to designate this library's proprietary parameters added to the protocol.
		/// </summary>
		internal const string CustomParameterPrefix = "dnoa.";

		/// <summary>
		/// A static variable that carries the results of a check for the presence of
		/// assemblies that are required for the Diffie-Hellman algorithm.
		/// </summary>
		private static bool? diffieHellmanPresent;

		/// <summary>
		/// Gets a value indicating whether Diffie Hellman is available in this installation.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if Diffie-Hellman functionality is present; otherwise, <c>false</c>.
		/// </value>
		internal static bool IsDiffieHellmanPresent {
			get {
				if (!diffieHellmanPresent.HasValue) {
					try {
						LoadDiffieHellmanTypes();
						diffieHellmanPresent = true;
					} catch (FileNotFoundException) {
						diffieHellmanPresent = false;
					} catch (TypeLoadException) {
						diffieHellmanPresent = false;
					}

					if (diffieHellmanPresent.Value) {
						Logger.OpenId.Info("Diffie-Hellman supporting assemblies found and loaded.");
					} else {
						Logger.OpenId.Warn("Diffie-Hellman supporting assemblies failed to load.  Only associations with HTTPS OpenID Providers will be supported.");
					}
				}

				return diffieHellmanPresent.Value;
			}
		}

		/// <summary>
		/// Creates a random association handle.
		/// </summary>
		/// <returns>The association handle.</returns>
		public static string GenerateRandomAssociationHandle() {
			// Generate the handle.  It must be unique, and preferably unpredictable,
			// so we use a time element and a random data element to generate it.
			string uniq = MessagingUtilities.GetCryptoRandomDataAsBase64(4);
			return string.Format(CultureInfo.InvariantCulture, "{{{0}}}{{{1}}}", DateTime.UtcNow.Ticks, uniq);
		}

		/// <summary>
		/// Immediately sends a redirect response to the browser to initiate an authentication request.
		/// </summary>
		/// <param name="authenticationRequest">The authentication request to send via redirect.</param>
		/// <param name="context">The context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public static async Task RedirectToProviderAsync(this IAuthenticationRequest authenticationRequest, HttpContextBase context = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(authenticationRequest, "authenticationRequest");
			Verify.Operation(context != null || HttpContext.Current != null, MessagingStrings.HttpContextRequired);

			context = context ?? new HttpContextWrapper(HttpContext.Current);
			var response = await authenticationRequest.GetRedirectingResponseAsync(cancellationToken);
			await response.SendAsync(context, cancellationToken);
		}

		/// <summary>
		/// Gets the OpenID protocol instance for the version in a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The OpenID protocol instance.</returns>
		internal static Protocol GetProtocol(this IProtocolMessage message) {
			Requires.NotNull(message, "message");
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
		internal static UriBuilder GetResolvedRealm(Page page, string realm, HttpRequestBase requestContext) {
			Requires.NotNull(page, "page");
			Requires.NotNull(requestContext, "requestContext");

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
				new Uri(requestContext.GetPublicFacingUrl(), page.ResolveUrl(realmNoWildcard)));

			if (foundWildcard) {
				fullyQualifiedRealm.Host = "*." + fullyQualifiedRealm.Host;
			}

			// Is it valid?
			new Realm(fullyQualifiedRealm); // throws if not valid

			return fullyQualifiedRealm;
		}

		/// <summary>
		/// Creates a new HTTP client for use by OpenID relying parties and providers.
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="requireSsl">if set to <c>true</c> [require SSL].</param>
		/// <param name="cachePolicy">The cache policy.</param>
		/// <returns>An HttpClient instance with appropriate caching policies set for OpenID operations.</returns>
		internal static HttpClient CreateHttpClient(this IHostFactories hostFactories, bool requireSsl, RequestCachePolicy cachePolicy = null) {
			Requires.NotNull(hostFactories, "hostFactories");

			var rootHandler = hostFactories.CreateHttpMessageHandler();
			var handler = rootHandler;
			bool sslRequiredSet = false, cachePolicySet = false;
			do {
				var webRequestHandler = handler as WebRequestHandler;
				var untrustedHandler = handler as UntrustedWebRequestHandler;
				var delegatingHandler = handler as DelegatingHandler;
				if (webRequestHandler != null) {
					if (cachePolicy != null) {
						webRequestHandler.CachePolicy = cachePolicy;
						cachePolicySet = true;
					}
				} else if (untrustedHandler != null) {
					untrustedHandler.IsSslRequired = requireSsl;
					sslRequiredSet = true;
				}

				if (delegatingHandler != null) {
					handler = delegatingHandler.InnerHandler;
				} else {
					break;
				}
			}
			while (true);

			if (cachePolicy != null && !cachePolicySet) {
				Logger.OpenId.Warn(
					"Unable to set cache policy due to HttpMessageHandler instances not being of type WebRequestHandler.");
			}

			ErrorUtilities.VerifyProtocol(!requireSsl || sslRequiredSet, "Unable to set RequireSsl on message handler because no HttpMessageHandler was of type {0}.", typeof(UntrustedWebRequestHandler).FullName);

			return hostFactories.CreateHttpClient(rootHandler);
		}

		/// <summary>
		/// Gets the extension factories from the extension aggregator on an OpenID channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <returns>The list of factories that will be used to generate extension instances.</returns>
		/// <remarks>
		/// This is an extension method on <see cref="Channel"/> rather than an instance
		/// method on <see cref="OpenIdChannel"/> because the OpenIdRelyingParty
		/// and OpenIdProvider classes don't strong-type to <see cref="OpenIdChannel"/>
		/// to allow flexibility in the specific type of channel the user (or tests)
		/// can plug in.
		/// </remarks>
		internal static IList<IOpenIdExtensionFactory> GetExtensionFactories(this Channel channel) {
			Requires.NotNull(channel, "channel");

			var extensionsBindingElement = channel.BindingElements.OfType<ExtensionsBindingElement>().SingleOrDefault();
			ErrorUtilities.VerifyOperation(extensionsBindingElement != null, OpenIdStrings.UnsupportedChannelConfiguration);
			IOpenIdExtensionFactory factory = extensionsBindingElement.ExtensionFactory;
			var aggregator = factory as OpenIdExtensionFactoryAggregator;
			ErrorUtilities.VerifyOperation(aggregator != null, OpenIdStrings.UnsupportedChannelConfiguration);
			return aggregator.Factories;
		}

		/// <summary>
		/// Loads the Diffie-Hellman assemblies.
		/// </summary>
		/// <exception cref="FileNotFoundException">Thrown if the DH assemblies are missing.</exception>
		private static void LoadDiffieHellmanTypes() {
			// This seeming no-op instruction is enough for the CLR to throw a FileNotFoundException
			// If the assemblies are missing.
			new DiffieHellmanManaged();
		}
	}
}
