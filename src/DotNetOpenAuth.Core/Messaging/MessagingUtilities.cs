//-----------------------------------------------------------------------
// <copyright file="MessagingUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Runtime.Serialization.Json;
	using System.Security;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using Validation;

	/// <summary>
	/// A grab-bag of utility methods useful for the channel stack of the protocol.
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Utility class touches lots of surface area")]
	public static class MessagingUtilities {
		/// <summary>
		/// The cryptographically strong random data generator used for creating secrets.
		/// </summary>
		/// <remarks>The random number generator is thread-safe.</remarks>
		internal static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

		/// <summary>
		/// The uppercase alphabet.
		/// </summary>
		internal const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		/// <summary>
		/// The lowercase alphabet.
		/// </summary>
		internal const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

		/// <summary>
		/// The set of base 10 digits.
		/// </summary>
		internal const string Digits = "0123456789";

		/// <summary>
		/// The set of digits and alphabetic letters (upper and lowercase).
		/// </summary>
		internal const string AlphaNumeric = UppercaseLetters + LowercaseLetters + Digits;

		/// <summary>
		/// All the characters that are allowed for use as a base64 encoding character.
		/// </summary>
		internal const string Base64Characters = AlphaNumeric + "+" + "/";

		/// <summary>
		/// All the characters that are allowed for use as a base64 encoding character
		/// in the "web safe" context.
		/// </summary>
		internal const string Base64WebSafeCharacters = AlphaNumeric + "-" + "_";

		/// <summary>
		/// The set of digits, and alphabetic letters (upper and lowercase) that are clearly
		/// visually distinguishable.
		/// </summary>
		internal const string AlphaNumericNoLookAlikes = "23456789abcdefghjkmnpqrstwxyzABCDEFGHJKMNPQRSTWXYZ";

		/// <summary>
		/// The length of private symmetric secret handles.
		/// </summary>
		/// <remarks>
		/// This value needn't be high, as we only expect to have a small handful of unexpired secrets at a time,
		/// and handle recycling is permissible.
		/// </remarks>
		private const int SymmetricSecretHandleLength = 4;

		/// <summary>
		/// A pre-completed task.
		/// </summary>
		private static readonly Task CompletedTaskField = Task.FromResult<object>(null);

		/// <summary>
		/// The default lifetime of a private secret.
		/// </summary>
		private static readonly TimeSpan SymmetricSecretKeyLifespan = Configuration.DotNetOpenAuthSection.Messaging.PrivateSecretMaximumAge;

		/// <summary>
		/// A character array containing just the = character.
		/// </summary>
		private static readonly char[] EqualsArray = new char[] { '=' };

		/// <summary>
		/// A character array containing just the , character.
		/// </summary>
		private static readonly char[] CommaArray = new char[] { ',' };

		/// <summary>
		/// A character array containing just the " character.
		/// </summary>
		private static readonly char[] QuoteArray = new char[] { '"' };

		/// <summary>
		/// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
		/// </summary>
		private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

		/// <summary>
		/// A set of escaping mappings that help secure a string from javscript execution.
		/// </summary>
		/// <remarks>
		/// The characters to escape here are inspired by 
		/// http://code.google.com/p/doctype/wiki/ArticleXSSInJavaScript
		/// </remarks>
		private static readonly Dictionary<string, string> javascriptStaticStringEscaping = new Dictionary<string, string> {
			{ "\\", @"\\" }, // this WAS just above the & substitution but we moved it here to prevent double-escaping
			{ "\t", @"\t" },
			{ "\n", @"\n" },
			{ "\r", @"\r" },
			{ "\u0085", @"\u0085" },
			{ "\u2028", @"\u2028" },
			{ "\u2029", @"\u2029" },
			{ "'", @"\x27" },
			{ "\"", @"\x22" },
			{ "&", @"\x26" },
			{ "<", @"\x3c" },
			{ ">", @"\x3e" },
			{ "=", @"\x3d" },
		};

		/// <summary>
		/// The available compression algorithms.
		/// </summary>
		internal enum CompressionMethod {
			/// <summary>
			/// The Deflate algorithm.
			/// </summary>
			Deflate,

			/// <summary>
			/// The GZip algorithm.
			/// </summary>
			Gzip,
		}

		/// <summary>
		/// Gets a pre-completed task.
		/// </summary>
		internal static Task CompletedTask {
			get { return CompletedTaskField; }
		}

		/// <summary>
		/// Gets a random number generator for use on the current thread only.
		/// </summary>
		internal static Random NonCryptoRandomDataGenerator {
			get { return ThreadSafeRandom.RandomNumberGenerator; }
		}

		/// <summary>
		/// Gets the original request URL, as seen from the browser before any URL rewrites on the server if any.
		/// Cookieless session directory (if applicable) is also included.
		/// </summary>
		/// <returns>The URL in the user agent's Location bar.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "The Uri merging requires use of a string value.")]
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive call should not be a property.")]
		public static Uri GetRequestUrlFromContext() {
			RequiresEx.ValidState(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			return new HttpRequestWrapper(HttpContext.Current.Request).GetPublicFacingUrl();
		}

		/// <summary>
		/// Strips any and all URI query parameters that start with some prefix.
		/// </summary>
		/// <param name="uri">The URI that may have a query with parameters to remove.</param>
		/// <param name="prefix">The prefix for parameters to remove.  A period is NOT automatically appended.</param>
		/// <returns>Either a new Uri with the parameters removed if there were any to remove, or the same Uri instance if no parameters needed to be removed.</returns>
		public static Uri StripQueryArgumentsWithPrefix(this Uri uri, string prefix) {
			Requires.NotNull(uri, "uri");
			Requires.NotNullOrEmpty(prefix, "prefix");

			NameValueCollection queryArgs = HttpUtility.ParseQueryString(uri.Query);
			var matchingKeys = queryArgs.Keys.OfType<string>().Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
			if (matchingKeys.Count > 0) {
				UriBuilder builder = new UriBuilder(uri);
				foreach (string key in matchingKeys) {
					queryArgs.Remove(key);
				}
				builder.Query = CreateQueryString(queryArgs.ToDictionary());
				return builder.Uri;
			} else {
				return uri;
			}
		}

		/// <summary>
		/// Assembles a message comprised of the message on a given exception and all inner exceptions.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <returns>The assembled message.</returns>
		public static string ToStringDescriptive(this Exception exception) {
			// The input being null is probably bad, but since this method is called
			// from a catch block, we don't really want to throw a new exception and
			// hide the details of this one.  
			if (exception == null) {
				Logger.Messaging.Error("MessagingUtilities.GetAllMessages called with null input.");
			}

			StringBuilder message = new StringBuilder();
			while (exception != null) {
				message.Append(exception.Message);
				exception = exception.InnerException;
				if (exception != null) {
					message.Append("  ");
				}
			}

			return message.ToString();
		}

		/// <summary>
		/// Flattens the specified sequence of sequences.
		/// </summary>
		/// <typeparam name="T">The type of element contained in the sequence.</typeparam>
		/// <param name="sequence">The sequence of sequences to flatten.</param>
		/// <returns>A sequence of the contained items.</returns>
		[Obsolete("Use Enumerable.SelectMany instead.")]
		public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> sequence) {
			ErrorUtilities.VerifyArgumentNotNull(sequence, "sequence");

			foreach (IEnumerable<T> subsequence in sequence) {
				foreach (T item in subsequence) {
					yield return item;
				}
			}
		}

		/// <summary>
		/// Cuts off precision beyond a second on a DateTime value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A DateTime with a 0 millisecond component.</returns>
		public static DateTime CutToSecond(this DateTime value) {
			return value - TimeSpan.FromMilliseconds(value.Millisecond);
		}

		/// <summary>
		/// Adds a name-value pair to the end of a given URL
		/// as part of the querystring piece.  Prefixes a ? or &amp; before
		/// first element as necessary.
		/// </summary>
		/// <param name="builder">The UriBuilder to add arguments to.</param>
		/// <param name="name">The name of the parameter to add.</param>
		/// <param name="value">The value of the argument.</param>
		/// <remarks>
		/// If the parameters to add match names of parameters that already are defined
		/// in the query string, the existing ones are <i>not</i> replaced.
		/// </remarks>
		public static void AppendQueryArgument(this UriBuilder builder, string name, string value) {
			AppendQueryArgs(builder, new[] { new KeyValuePair<string, string>(name, value) });
		}

		/// <summary>
		/// Adds a set of values to a collection.
		/// </summary>
		/// <typeparam name="T">The type of value kept in the collection.</typeparam>
		/// <param name="collection">The collection to add to.</param>
		/// <param name="values">The values to add to the collection.</param>
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values) {
			Requires.NotNull(collection, "collection");
			Requires.NotNull(values, "values");

			foreach (var value in values) {
				collection.Add(value);
			}
		}

		/// <summary>
		/// Tests whether two timespans are within reasonable approximation of each other.
		/// </summary>
		/// <param name="self">One TimeSpan.</param>
		/// <param name="other">The other TimeSpan.</param>
		/// <param name="marginOfError">The allowable margin of error.</param>
		/// <returns><c>true</c> if the two TimeSpans are within <paramref name="marginOfError"/> of each other.</returns>
		public static bool Equals(this TimeSpan self, TimeSpan other, TimeSpan marginOfError) {
			return TimeSpan.FromMilliseconds(Math.Abs((self - other).TotalMilliseconds)) < marginOfError;
		}

		/// <summary>
		/// Compares to string values for ordinal equality in such a way that its execution time does not depend on how much of the value matches.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>A value indicating whether the two strings share ordinal equality.</returns>
		/// <remarks>
		/// In signature equality checks, a difference in execution time based on how many initial characters match MAY
		/// be used as an attack to figure out the expected signature.  It is therefore important to make a signature
		/// equality check's execution time independent of how many characters match the expected value.
		/// See http://codahale.com/a-lesson-in-timing-attacks/ for more information.
		/// </remarks>
		public static bool EqualsConstantTime(string value1, string value2) {
			// If exactly one value is null, they don't match.
			if (value1 == null ^ value2 == null) {
				return false;
			}

			// If both values are null (since if one is at this point then they both are), it's a match.
			if (value1 == null) {
				return true;
			}

			if (value1.Length != value2.Length) {
				return false;
			}

			// This looks like a pretty crazy way to compare values, but it provides a constant time equality check,
			// and is more resistant to compiler optimizations than simply setting a boolean flag and returning the boolean after the loop.
			int result = 0;
			for (int i = 0; i < value1.Length; i++) {
				result |= value1[i] ^ value2[i];
			}

			return result == 0;
		}

		/// <summary>
		/// Gets the public facing URL for the given incoming HTTP request.
		/// </summary>
		/// <param name="request">The incoming request.  Cannot be <c>null</c>.</param>
		/// <param name="serverVariables">The server variables to consider part of the request.  Cannot be <c>null</c>.</param>
		/// <returns>
		/// The URI that the outside world used to create this request.
		/// </returns>
		/// <remarks>
		/// Although the <paramref name="serverVariables"/> value can be obtained from
		/// <see cref="HttpRequest.ServerVariables"/>, it's useful to be able to pass them
		/// in so we can simulate injected values from our unit tests since the actual property
		/// is a read-only kind of <see cref="NameValueCollection"/>.
		/// </remarks>
		public static Uri GetPublicFacingUrl(this HttpRequestBase request, NameValueCollection serverVariables) {
			Requires.NotNull(request, "request");
			Requires.NotNull(serverVariables, "serverVariables");

			// Due to URL rewriting, cloud computing (i.e. Azure)
			// and web farms, etc., we have to be VERY careful about what
			// we consider the incoming URL.  We want to see the URL as it would
			// appear on the public-facing side of the hosting web site.
			// HttpRequest.Url gives us the internal URL in a cloud environment,
			// So we use a variable that (at least from what I can tell) gives us
			// the public URL:
			string httpHost;
			try {
				httpHost = serverVariables["HTTP_HOST"];
			} catch (NullReferenceException) {
				// The VS dev web server can throw this. :(
				httpHost = null;
			}

			if (httpHost != null) {
				ErrorUtilities.VerifySupported(request.Url.Scheme == Uri.UriSchemeHttps || request.Url.Scheme == Uri.UriSchemeHttp, "Only HTTP and HTTPS are supported protocols.");
				string scheme = serverVariables["HTTP_X_FORWARDED_PROTO"] ??
					(string.Equals(serverVariables["HTTP_FRONT_END_HTTPS"], "on", StringComparison.OrdinalIgnoreCase) ? Uri.UriSchemeHttps : request.Url.Scheme);
				Uri hostAndPort = new Uri(scheme + Uri.SchemeDelimiter + serverVariables["HTTP_HOST"]);
				UriBuilder publicRequestUri = new UriBuilder(request.Url);
				publicRequestUri.Scheme = scheme;
				publicRequestUri.Host = hostAndPort.Host;
				publicRequestUri.Port = hostAndPort.Port; // CC missing Uri.Port contract that's on UriBuilder.Port
				return publicRequestUri.Uri;
			} else {
				// Failover to the method that works for non-web farm enviroments.
				// We use Request.Url for the full path to the server, and modify it
				// with Request.RawUrl to capture both the cookieless session "directory" if it exists
				// and the original path in case URL rewriting is going on.  We don't want to be
				// fooled by URL rewriting because we're comparing the actual URL with what's in
				// the return_to parameter in some cases.
				// Response.ApplyAppPathModifier(builder.Path) would have worked for the cookieless
				// session, but not the URL rewriting problem.
				return new Uri(request.Url, request.RawUrl);
			}
		}

		/// <summary>
		/// Gets the public facing URL for the given incoming HTTP request.
		/// </summary>
		/// <param name="request">The incoming request.  Cannot be <c>null</c>.  Server variables are read from this request.</param>
		/// <returns>The URI that the outside world used to create this request.</returns>
		public static Uri GetPublicFacingUrl(this HttpRequestBase request) {
			Requires.NotNull(request, "request");
			return GetPublicFacingUrl(request, request.ServerVariables);
		}

		/// <summary>
		/// Gets the public facing URL for the given incoming HTTP request.
		/// </summary>
		/// <returns>The URI that the outside world used to create this request.</returns>
		public static Uri GetPublicFacingUrl() {
			ErrorUtilities.VerifyHttpContext();
			return GetPublicFacingUrl(new HttpRequestWrapper(HttpContext.Current.Request));
		}

		/// <summary>
		/// Wraps an instance of <see cref="HttpRequestBase"/> as an <see cref="HttpRequestMessage"/> instance.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>An instance of <see cref="HttpRequestMessage"/></returns>
		public static HttpRequestMessage AsHttpRequestMessage(this HttpRequestBase request) {
			Requires.NotNull(request, "request");

			Uri publicFacingUrl = request.GetPublicFacingUrl();
			var httpRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), publicFacingUrl);

			if (request.Form != null) {
				// Avoid a request message that will try to read the request stream twice for already parsed data.
				httpRequest.Content = new FormUrlEncodedContent(request.Form.AsKeyValuePairs());
			} else if (request.InputStream != null) {
				httpRequest.Content = new StreamContent(request.InputStream);
			}

			httpRequest.CopyHeadersFrom(request);
			return httpRequest;
		}

		/// <summary>
		/// Sends a response message to the HTTP client.
		/// </summary>
		/// <param name="response">The response message.</param>
		/// <param name="context">The HTTP context to send the response with.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public static async Task SendAsync(this HttpResponseMessage response, HttpContextBase context = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(response, "response");
			if (context == null) {
				ErrorUtilities.VerifyHttpContext();
				context = new HttpContextWrapper(HttpContext.Current);
			}

			var responseContext = context.Response;
			responseContext.StatusCode = (int)response.StatusCode;
			responseContext.StatusDescription = response.ReasonPhrase;
			foreach (var header in response.Headers) {
				foreach (var value in header.Value) {
					responseContext.AddHeader(header.Key, value);
				}
			}

			if (response.Content != null) {
				await response.Content.CopyToAsync(responseContext.OutputStream).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Disposes a value if it is not null.
		/// </summary>
		/// <param name="disposable">The disposable value.</param>
		internal static void DisposeIfNotNull(this IDisposable disposable) {
			if (disposable != null) {
				disposable.Dispose();
			}
		}

		/// <summary>
		/// Clones the specified <see cref="HttpRequestMessage"/> so it can be re-sent.
		/// </summary>
		/// <param name="original">The original message.</param>
		/// <returns>The cloned message</returns>
		/// <remarks>
		/// This is useful when an HTTP request fails, and after a little tweaking should be resent.
		/// Since <see cref="HttpRequestMessage"/> remembers it was already sent, it will not permit being
		/// sent a second time. This method clones the message so its contents are identical but allows
		/// re-sending.
		/// </remarks>
		internal static HttpRequestMessage Clone(this HttpRequestMessage original) {
			Requires.NotNull(original, "original");

			var clone = new HttpRequestMessage(original.Method, original.RequestUri);
			clone.Content = original.Content;
			foreach (var header in original.Headers) {
				clone.Headers.Add(header.Key, header.Value);
			}

			foreach (var property in original.Properties) {
				clone.Properties[property.Key] = property.Value;
			}

			clone.Version = original.Version;
			return clone;
		}

		/// <summary>
		/// Gets the URL to the root of a web site, which may include a virtual directory path.
		/// </summary>
		/// <returns>An absolute URI.</returns>
		internal static Uri GetWebRoot() {
			HttpRequestBase requestInfo = new HttpRequestWrapper(HttpContext.Current.Request);
			UriBuilder realmUrl = new UriBuilder(requestInfo.GetPublicFacingUrl());
			realmUrl.Path = HttpContext.Current.Request.ApplicationPath;
			realmUrl.Query = null;
			realmUrl.Fragment = null;
			return realmUrl.Uri;
		}

		/// <summary>
		/// Creates the XML reader settings to use for reading XML from untrusted sources.
		/// </summary>
		/// <returns>
		/// The new instance of <see cref="XmlReaderSettings"/>.
		/// </returns>
		/// <remarks>
		/// The default values set here are based on recommendations from
		/// http://msdn.microsoft.com/en-us/magazine/ee335713.aspx
		/// </remarks>
		internal static XmlReaderSettings CreateUntrustedXmlReaderSettings() {
			return new XmlReaderSettings {
				MaxCharactersFromEntities = 1024,
				XmlResolver = null,
				DtdProcessing = DtdProcessing.Prohibit,
			};
		}

		/// <summary>
		/// Clears any existing elements in a collection and fills the collection with a given set of values.
		/// </summary>
		/// <typeparam name="T">The type of value kept in the collection.</typeparam>
		/// <param name="collection">The collection to modify.</param>
		/// <param name="values">The new values to fill the collection.</param>
		internal static void ResetContents<T>(this ICollection<T> collection, IEnumerable<T> values) {
			Requires.NotNull(collection, "collection");

			collection.Clear();
			if (values != null) {
				AddRange(collection, values);
			}
		}

		/// <summary>
		/// Strips any and all URI query parameters that serve as parts of a message.
		/// </summary>
		/// <param name="uri">The URI that may contain query parameters to remove.</param>
		/// <param name="messageDescription">The message description whose parts should be removed from the URL.</param>
		/// <returns>A cleaned URL.</returns>
		internal static Uri StripMessagePartsFromQueryString(this Uri uri, MessageDescription messageDescription) {
			Requires.NotNull(uri, "uri");
			Requires.NotNull(messageDescription, "messageDescription");

			NameValueCollection queryArgs = HttpUtility.ParseQueryString(uri.Query);
			var matchingKeys = queryArgs.Keys.OfType<string>().Where(key => messageDescription.Mapping.ContainsKey(key)).ToList();
			if (matchingKeys.Count > 0) {
				var builder = new UriBuilder(uri);
				foreach (string key in matchingKeys) {
					queryArgs.Remove(key);
				}
				builder.Query = CreateQueryString(queryArgs.ToDictionary());
				return builder.Uri;
			} else {
				return uri;
			}
		}

		/// <summary>
		/// Assembles the content of the HTTP Authorization or WWW-Authenticate header.
		/// </summary>
		/// <param name="fields">The fields to include.</param>
		/// <returns>
		/// A value prepared for an HTTP header.
		/// </returns>
		internal static string AssembleAuthorizationHeader(IEnumerable<KeyValuePair<string, string>> fields) {
			Requires.NotNull(fields, "fields");

			var authorization = new StringBuilder();
			foreach (var pair in fields) {
				string key = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Key);
				string value = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Value);
				authorization.Append(key);
				authorization.Append("=\"");
				authorization.Append(value);
				authorization.Append("\",");
			}
			authorization.Length--; // remove trailing comma
			return authorization.ToString();
		}

		/// <summary>
		/// Assembles the content of the HTTP Authorization or WWW-Authenticate header.
		/// </summary>
		/// <param name="scheme">The scheme.</param>
		/// <param name="fields">The fields to include.</param>
		/// <returns>A value prepared for an HTTP header.</returns>
		internal static string AssembleAuthorizationHeader(string scheme, IEnumerable<KeyValuePair<string, string>> fields) {
			Requires.NotNullOrEmpty(scheme, "scheme");
			Requires.NotNull(fields, "fields");

			var authorization = new StringBuilder();
			authorization.Append(scheme);
			authorization.Append(" ");
			authorization.Append(AssembleAuthorizationHeader(fields));
			return authorization.ToString();
		}

		/// <summary>
		/// Parses the authorization header.
		/// </summary>
		/// <param name="scheme">The scheme.  Must not be null or empty.</param>
		/// <param name="authorizationHeader">The authorization header.  May be null or empty.</param>
		/// <returns>A sequence of key=value pairs discovered in the header.  Never null, but may be empty.</returns>
		internal static IEnumerable<KeyValuePair<string, string>> ParseAuthorizationHeader(string scheme, AuthenticationHeaderValue authorizationHeader) {
			Requires.NotNullOrEmpty(scheme, "scheme");

			if (authorizationHeader != null && authorizationHeader.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase)) { // RFC 2617 says this is case INsensitive
				return from element in authorizationHeader.Parameter.Split(CommaArray)
					   let parts = element.Trim().Split(EqualsArray, 2)
					   let key = Uri.UnescapeDataString(parts[0])
					   let value = Uri.UnescapeDataString(parts[1].Trim(QuoteArray))
					   select new KeyValuePair<string, string>(key, value);
			}

			return Enumerable.Empty<KeyValuePair<string, string>>();
		}

		/// <summary>
		/// Encodes a symmetric key handle and the blob that is encrypted/signed with that key into a single string
		/// that can be decoded by <see cref="ExtractKeyHandleAndPayload"/>.
		/// </summary>
		/// <param name="handle">The cryptographic key handle.</param>
		/// <param name="payload">The encrypted/signed blob.</param>
		/// <returns>The combined encoded value.</returns>
		internal static string CombineKeyHandleAndPayload(string handle, string payload) {
			Requires.NotNullOrEmpty(handle, "handle");
			Requires.NotNullOrEmpty(payload, "payload");

			return handle + "!" + payload;
		}

		/// <summary>
		/// Extracts the key handle and encrypted blob from a string previously returned from <see cref="CombineKeyHandleAndPayload"/>.
		/// </summary>
		/// <param name="messagePart">The message part. May be null if not applicable.</param>
		/// <param name="keyHandleAndBlob">The value previously returned from <see cref="CombineKeyHandleAndPayload"/>.</param>
		/// <param name="handle">The crypto key handle.</param>
		/// <param name="dataBlob">The encrypted/signed data.</param>
		internal static void ExtractKeyHandleAndPayload(string messagePart, string keyHandleAndBlob, out string handle, out string dataBlob) {
			Requires.NotNullOrEmpty(keyHandleAndBlob, "keyHandleAndBlob");

			int privateHandleIndex = keyHandleAndBlob.IndexOf('!');
			ErrorUtilities.VerifyProtocol(privateHandleIndex > 0, MessagingStrings.UnexpectedMessagePartValue, messagePart ?? "<unknown>", keyHandleAndBlob);
			handle = keyHandleAndBlob.Substring(0, privateHandleIndex);
			dataBlob = keyHandleAndBlob.Substring(privateHandleIndex + 1);
		}

		/// <summary>
		/// Gets a buffer of random data (not cryptographically strong).
		/// </summary>
		/// <param name="length">The length of the sequence to generate.</param>
		/// <returns>The generated values, which may contain zeros.</returns>
		internal static byte[] GetNonCryptoRandomData(int length) {
			byte[] buffer = new byte[length];
			NonCryptoRandomDataGenerator.NextBytes(buffer);
			return buffer;
		}

		/// <summary>
		/// Gets a cryptographically strong random sequence of values.
		/// </summary>
		/// <param name="length">The length of the sequence to generate.</param>
		/// <returns>The generated values, which may contain zeros.</returns>
		internal static byte[] GetCryptoRandomData(int length) {
			byte[] buffer = new byte[length];
			CryptoRandomDataGenerator.GetBytes(buffer);
			return buffer;
		}

		/// <summary>
		/// Gets a cryptographically strong random string of base64 characters.
		/// </summary>
		/// <param name="binaryLength">The length of the byte sequence to generate.</param>
		/// <returns>A base64 encoding of the generated random data, 
		/// whose length in characters will likely be greater than <paramref name="binaryLength"/>.</returns>
		internal static string GetCryptoRandomDataAsBase64(int binaryLength) {
			byte[] uniq_bytes = GetCryptoRandomData(binaryLength);
			string uniq = Convert.ToBase64String(uniq_bytes);
			return uniq;
		}

		/// <summary>
		/// Gets a NON-cryptographically strong random string of base64 characters.
		/// </summary>
		/// <param name="binaryLength">The length of the byte sequence to generate.</param>
		/// <param name="useWeb64">A value indicating whether web64 encoding is used to avoid the need to escape characters.</param>
		/// <returns>
		/// A base64 encoding of the generated random data,
		/// whose length in characters will likely be greater than <paramref name="binaryLength" />.
		/// </returns>
		internal static string GetNonCryptoRandomDataAsBase64(int binaryLength, bool useWeb64 = false) {
			byte[] uniq_bytes = GetNonCryptoRandomData(binaryLength);
			string uniq = useWeb64 ? ConvertToBase64WebSafeString(uniq_bytes) : Convert.ToBase64String(uniq_bytes);
			return uniq;
		}

		/// <summary>
		/// Gets a random string made up of a given set of allowable characters.
		/// </summary>
		/// <param name="length">The length of the desired random string.</param>
		/// <param name="allowableCharacters">The allowable characters.</param>
		/// <returns>A random string.</returns>
		internal static string GetRandomString(int length, string allowableCharacters) {
			Requires.Range(length >= 0, "length");
			Requires.That(allowableCharacters != null && allowableCharacters.Length >= 2, "allowableCharacters", "At least two allowable characters required.");

			char[] randomString = new char[length];
			var random = NonCryptoRandomDataGenerator;
			for (int i = 0; i < length; i++) {
				randomString[i] = allowableCharacters[random.Next(allowableCharacters.Length)];
			}

			return new string(randomString);
		}

		/// <summary>
		/// Computes the hash of a string.
		/// </summary>
		/// <param name="algorithm">The hash algorithm to use.</param>
		/// <param name="value">The value to hash.</param>
		/// <param name="encoding">The encoding to use when converting the string to a byte array.</param>
		/// <returns>A base64 encoded string.</returns>
		internal static string ComputeHash(this HashAlgorithm algorithm, string value, Encoding encoding = null) {
			Requires.NotNull(algorithm, "algorithm");
			Requires.NotNull(value, "value");

			encoding = encoding ?? Encoding.UTF8;
			byte[] bytesToHash = encoding.GetBytes(value);
			byte[] hash = algorithm.ComputeHash(bytesToHash);
			string base64Hash = Convert.ToBase64String(hash);
			return base64Hash;
		}

		/// <summary>
		/// Computes the hash of a sequence of key=value pairs.
		/// </summary>
		/// <param name="algorithm">The hash algorithm to use.</param>
		/// <param name="data">The data to hash.</param>
		/// <param name="encoding">The encoding to use when converting the string to a byte array.</param>
		/// <returns>A base64 encoded string.</returns>
		internal static string ComputeHash(this HashAlgorithm algorithm, IDictionary<string, string> data, Encoding encoding = null) {
			Requires.NotNull(algorithm, "algorithm");
			Requires.NotNull(data, "data");

			// Assemble the dictionary to sign, taking care to remove the signature itself
			// in order to accurately reproduce the original signature (which of course didn't include
			// the signature).
			// Also we need to sort the dictionary's keys so that we sign in the same order as we did
			// the last time.
			var sortedData = new SortedDictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
			return ComputeHash(algorithm, (IEnumerable<KeyValuePair<string, string>>)sortedData, encoding);
		}

		/// <summary>
		/// Computes the hash of a sequence of key=value pairs.
		/// </summary>
		/// <param name="algorithm">The hash algorithm to use.</param>
		/// <param name="sortedData">The data to hash.</param>
		/// <param name="encoding">The encoding to use when converting the string to a byte array.</param>
		/// <returns>A base64 encoded string.</returns>
		internal static string ComputeHash(this HashAlgorithm algorithm, IEnumerable<KeyValuePair<string, string>> sortedData, Encoding encoding = null) {
			Requires.NotNull(algorithm, "algorithm");
			Requires.NotNull(sortedData, "sortedData");

			return ComputeHash(algorithm, CreateQueryString(sortedData), encoding);
		}

		/// <summary>
		/// Encrypts a byte buffer.
		/// </summary>
		/// <param name="buffer">The buffer to encrypt.</param>
		/// <param name="key">The symmetric secret to use to encrypt the buffer.  Allowed values are 128, 192, or 256 bytes in length.</param>
		/// <returns>The encrypted buffer</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		internal static byte[] Encrypt(byte[] buffer, byte[] key) {
			using (SymmetricAlgorithm crypto = CreateSymmetricAlgorithm(key)) {
				using (var ms = new MemoryStream()) {
					var binaryWriter = new BinaryWriter(ms);
					binaryWriter.Write((byte)1); // version of encryption algorithm
					binaryWriter.Write(crypto.IV);
					binaryWriter.Flush();

					var cryptoStream = new CryptoStream(ms, crypto.CreateEncryptor(), CryptoStreamMode.Write);
					cryptoStream.Write(buffer, 0, buffer.Length);
					cryptoStream.FlushFinalBlock();

					return ms.ToArray();
				}
			}
		}

		/// <summary>
		/// Decrypts a byte buffer.
		/// </summary>
		/// <param name="buffer">The buffer to decrypt.</param>
		/// <param name="key">The symmetric secret to use to decrypt the buffer.  Allowed values are 128, 192, and 256.</param>
		/// <returns>The encrypted buffer</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		internal static byte[] Decrypt(byte[] buffer, byte[] key) {
			using (SymmetricAlgorithm crypto = CreateSymmetricAlgorithm(key)) {
				using (var ms = new MemoryStream(buffer)) {
					var binaryReader = new BinaryReader(ms);
					int algorithmVersion = binaryReader.ReadByte();
					ErrorUtilities.VerifyProtocol(algorithmVersion == 1, MessagingStrings.UnsupportedEncryptionAlgorithm);
					crypto.IV = binaryReader.ReadBytes(crypto.IV.Length);

					// Allocate space for the decrypted buffer.  We don't know how long it will be yet,
					// but it will never be larger than the encrypted buffer.
					var decryptedBuffer = new byte[buffer.Length];
					int actualDecryptedLength;

					using (var cryptoStream = new CryptoStream(ms, crypto.CreateDecryptor(), CryptoStreamMode.Read)) {
						actualDecryptedLength = cryptoStream.Read(decryptedBuffer, 0, decryptedBuffer.Length);
					}

					// Create a new buffer with only the decrypted data.
					var finalDecryptedBuffer = new byte[actualDecryptedLength];
					Array.Copy(decryptedBuffer, finalDecryptedBuffer, actualDecryptedLength);
					return finalDecryptedBuffer;
				}
			}
		}

		/// <summary>
		/// Encrypts a string.
		/// </summary>
		/// <param name="plainText">The text to encrypt.</param>
		/// <param name="key">The symmetric secret to use to encrypt the buffer.  Allowed values are 128, 192, and 256.</param>
		/// <returns>The encrypted buffer</returns>
		internal static string Encrypt(string plainText, byte[] key) {
			byte[] buffer = Encoding.UTF8.GetBytes(plainText);
			byte[] cipher = Encrypt(buffer, key);
			return Convert.ToBase64String(cipher);
		}

		/// <summary>
		/// Decrypts a string previously encrypted with <see cref="Encrypt(string, byte[])"/>.
		/// </summary>
		/// <param name="cipherText">The text to decrypt.</param>
		/// <param name="key">The symmetric secret to use to decrypt the buffer.  Allowed values are 128, 192, and 256.</param>
		/// <returns>The encrypted buffer</returns>
		internal static string Decrypt(string cipherText, byte[] key) {
			byte[] cipher = Convert.FromBase64String(cipherText);
			byte[] plainText = Decrypt(cipher, key);
			return Encoding.UTF8.GetString(plainText);
		}

		/// <summary>
		/// Performs asymmetric encryption of a given buffer.
		/// </summary>
		/// <param name="crypto">The asymmetric encryption provider to use for encryption.</param>
		/// <param name="buffer">The buffer to encrypt.</param>
		/// <returns>The encrypted data.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		internal static byte[] EncryptWithRandomSymmetricKey(this RSACryptoServiceProvider crypto, byte[] buffer) {
			Requires.NotNull(crypto, "crypto");
			Requires.NotNull(buffer, "buffer");

			using (var symmetricCrypto = SymmetricAlgorithm.Create()) {
				symmetricCrypto.Mode = CipherMode.CBC;

				using (var encryptedStream = new MemoryStream()) {
					var encryptedStreamWriter = new BinaryWriter(encryptedStream);

					byte[] prequel = new byte[symmetricCrypto.Key.Length + symmetricCrypto.IV.Length];
					Array.Copy(symmetricCrypto.Key, prequel, symmetricCrypto.Key.Length);
					Array.Copy(symmetricCrypto.IV, 0, prequel, symmetricCrypto.Key.Length, symmetricCrypto.IV.Length);
					byte[] encryptedPrequel = crypto.Encrypt(prequel, false);

					encryptedStreamWriter.Write(encryptedPrequel.Length);
					encryptedStreamWriter.Write(encryptedPrequel);
					encryptedStreamWriter.Flush();

					var cryptoStream = new CryptoStream(encryptedStream, symmetricCrypto.CreateEncryptor(), CryptoStreamMode.Write);
					cryptoStream.Write(buffer, 0, buffer.Length);
					cryptoStream.FlushFinalBlock();

					return encryptedStream.ToArray();
				}
			}
		}

		/// <summary>
		/// Performs asymmetric decryption of a given buffer.
		/// </summary>
		/// <param name="crypto">The asymmetric encryption provider to use for decryption.</param>
		/// <param name="buffer">The buffer to decrypt.</param>
		/// <returns>The decrypted data.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		internal static byte[] DecryptWithRandomSymmetricKey(this RSACryptoServiceProvider crypto, byte[] buffer) {
			Requires.NotNull(crypto, "crypto");
			Requires.NotNull(buffer, "buffer");

			using (var encryptedStream = new MemoryStream(buffer)) {
				var encryptedStreamReader = new BinaryReader(encryptedStream);

				byte[] encryptedPrequel = encryptedStreamReader.ReadBuffer(4096);
				byte[] prequel = crypto.Decrypt(encryptedPrequel, false);

				using (var symmetricCrypto = SymmetricAlgorithm.Create()) {
					symmetricCrypto.Mode = CipherMode.CBC;

					byte[] symmetricKey = new byte[symmetricCrypto.Key.Length];
					byte[] symmetricIV = new byte[symmetricCrypto.IV.Length];
					Array.Copy(prequel, symmetricKey, symmetricKey.Length);
					Array.Copy(prequel, symmetricKey.Length, symmetricIV, 0, symmetricIV.Length);
					symmetricCrypto.Key = symmetricKey;
					symmetricCrypto.IV = symmetricIV;

					// Allocate space for the decrypted buffer.  We don't know how long it will be yet,
					// but it will never be larger than the encrypted buffer.
					var decryptedBuffer = new byte[encryptedStream.Length - encryptedStream.Position];
					int actualDecryptedLength;

					using (var cryptoStream = new CryptoStream(encryptedStream, symmetricCrypto.CreateDecryptor(), CryptoStreamMode.Read)) {
						actualDecryptedLength = cryptoStream.Read(decryptedBuffer, 0, decryptedBuffer.Length);
					}

					// Create a new buffer with only the decrypted data.
					var finalDecryptedBuffer = new byte[actualDecryptedLength];
					Array.Copy(decryptedBuffer, finalDecryptedBuffer, actualDecryptedLength);
					return finalDecryptedBuffer;
				}
			}
		}

		/// <summary>
		/// Gets a key from a given bucket with the longest remaining life, or creates a new one if necessary.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store.</param>
		/// <param name="bucket">The bucket where the key should be found or stored.</param>
		/// <param name="minimumRemainingLife">The minimum remaining life required on the returned key.</param>
		/// <param name="keySize">The required size of the key, in bits.</param>
		/// <returns>
		/// A key-value pair whose key is the secret's handle and whose value is the cryptographic key.
		/// </returns>
		internal static KeyValuePair<string, CryptoKey> GetCurrentKey(this ICryptoKeyStore cryptoKeyStore, string bucket, TimeSpan minimumRemainingLife, int keySize = 256) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
			Requires.NotNullOrEmpty(bucket, "bucket");
			Requires.That(keySize % 8 == 0, "keySize", "Key size must be a multiple of 8.");

			var cryptoKeyPair = cryptoKeyStore.GetKeys(bucket).FirstOrDefault(pair => pair.Value.Key.Length == keySize / 8);
			if (cryptoKeyPair.Value == null || cryptoKeyPair.Value.ExpiresUtc < DateTime.UtcNow + minimumRemainingLife) {
				// No key exists with enough remaining life for the required purpose.  Create a new key.
				if (cryptoKeyPair.Value == null) {
					Logger.Messaging.InfoFormat("{0}.GetKeys returned no keys for bucket \"{1}\" with the required key length of {2} bits.  A new key will be created", typeof(ICryptoKeyStore), bucket, keySize);
				} else {
					Logger.Messaging.InfoFormat("The first key returned by {0}.GetKeys for bucket \"{1}\" with the required key length of {2} bits was too near expiry to use.  A new key will be created", typeof(ICryptoKeyStore), bucket, keySize);
				}

				ErrorUtilities.VerifyHost(minimumRemainingLife <= SymmetricSecretKeyLifespan, "Unable to create a new symmetric key with the required lifespan of {0} because it is beyond the limit of {1}.", minimumRemainingLife, SymmetricSecretKeyLifespan);
				byte[] secret = GetCryptoRandomData(keySize / 8);
				DateTime expires = DateTime.UtcNow + SymmetricSecretKeyLifespan;
				var cryptoKey = new CryptoKey(secret, expires);

				// Store this key so we can find and use it later.
				int failedAttempts = 0;
			tryAgain:
				try {
					string handle = GetRandomString(SymmetricSecretHandleLength, Base64WebSafeCharacters);
					cryptoKeyPair = new KeyValuePair<string, CryptoKey>(handle, cryptoKey);
					cryptoKeyStore.StoreKey(bucket, handle, cryptoKey);
				} catch (CryptoKeyCollisionException) {
					ErrorUtilities.VerifyInternal(++failedAttempts < 3, "Unable to derive a unique handle to a private symmetric key.");
					Logger.Messaging.Warn("A randomly generated crypto key handle collided with an existing handle.  Another randomly generated handle will be attempted till the retry count is met.");
					goto tryAgain;
				}
			}

			return cryptoKeyPair;
		}

		/// <summary>
		/// Compresses a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to compress.</param>
		/// <param name="method">The compression algorithm to use.</param>
		/// <returns>The compressed data.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		internal static byte[] Compress(byte[] buffer, CompressionMethod method = CompressionMethod.Deflate) {
			Requires.NotNull(buffer, "buffer");

			using (var ms = new MemoryStream()) {
				Stream compressingStream = null;
				try {
					switch (method) {
						case CompressionMethod.Deflate:
							compressingStream = new DeflateStream(ms, CompressionMode.Compress, true);
							break;
						case CompressionMethod.Gzip:
							compressingStream = new GZipStream(ms, CompressionMode.Compress, true);
							break;
						default:
							Requires.Range(false, "method");
							break;
					}

					compressingStream.Write(buffer, 0, buffer.Length);
					return ms.ToArray();
				} finally {
					if (compressingStream != null) {
						compressingStream.Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Decompresses a given buffer.
		/// </summary>
		/// <param name="buffer">The buffer to decompress.</param>
		/// <param name="method">The compression algorithm used.</param>
		/// <returns>The decompressed data.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		internal static byte[] Decompress(byte[] buffer, CompressionMethod method = CompressionMethod.Deflate) {
			Requires.NotNull(buffer, "buffer");

			using (var compressedDataStream = new MemoryStream(buffer)) {
				using (var decompressedDataStream = new MemoryStream()) {
					Stream decompressingStream = null;
					try {
						switch (method) {
							case CompressionMethod.Deflate:
								decompressingStream = new DeflateStream(compressedDataStream, CompressionMode.Decompress, true);
								break;
							case CompressionMethod.Gzip:
								decompressingStream = new GZipStream(compressedDataStream, CompressionMode.Decompress, true);
								break;
							default:
								Requires.Range(false, "method");
								break;
						}

						decompressingStream.CopyTo(decompressedDataStream);
					} finally {
						if (decompressingStream != null) {
							decompressingStream.Dispose();
						}
					}

					return decompressedDataStream.ToArray();
				}
			}
		}

		/// <summary>
		/// Converts to data buffer to a base64-encoded string, using web safe characters and with the padding removed.
		/// </summary>
		/// <param name="data">The data buffer.</param>
		/// <returns>A web-safe base64-encoded string without padding.</returns>
		internal static string ConvertToBase64WebSafeString(byte[] data) {
			var builder = new StringBuilder(Convert.ToBase64String(data));

			// Swap out the URL-unsafe characters, and trim the padding characters.
			builder.Replace('+', '-').Replace('/', '_');
			while (builder[builder.Length - 1] == '=') { // should happen at most twice.
				builder.Length -= 1;
			}

			return builder.ToString();
		}

		/// <summary>
		/// Decodes a (web-safe) base64-string back to its binary buffer form.
		/// </summary>
		/// <param name="base64WebSafe">The base64-encoded string.  May be web-safe encoded.</param>
		/// <returns>A data buffer.</returns>
		internal static byte[] FromBase64WebSafeString(string base64WebSafe) {
			Requires.NotNullOrEmpty(base64WebSafe, "base64WebSafe");

			// Restore the padding characters and original URL-unsafe characters.
			int missingPaddingCharacters;
			switch (base64WebSafe.Length % 4) {
				case 3:
					missingPaddingCharacters = 1;
					break;
				case 2:
					missingPaddingCharacters = 2;
					break;
				case 0:
					missingPaddingCharacters = 0;
					break;
				default:
					throw new ProtocolException(MessagingStrings.DataCorruptionDetected, new ArgumentException("No more than two padding characters should be present for base64."));
			}
			var builder = new StringBuilder(base64WebSafe, base64WebSafe.Length + missingPaddingCharacters);
			builder.Replace('-', '+').Replace('_', '/');
			builder.Append('=', missingPaddingCharacters);

			return Convert.FromBase64String(builder.ToString());
		}

		/// <summary>
		/// Adds a set of HTTP headers to an <see cref="HttpResponse"/> instance,
		/// taking care to set some headers to the appropriate properties of
		/// <see cref="HttpResponse" />
		/// </summary>
		/// <param name="headers">The headers to add.</param>
		/// <param name="response">The <see cref="HttpResponse"/> instance to set the appropriate values to.</param>
		internal static void ApplyHeadersToResponse(WebHeaderCollection headers, HttpResponseBase response) {
			Requires.NotNull(headers, "headers");
			Requires.NotNull(response, "response");

			foreach (string headerName in headers) {
				switch (headerName) {
					case "Content-Type":
						response.ContentType = headers[HttpResponseHeader.ContentType];
						break;

					// Add more special cases here as necessary.
					default:
						response.AddHeader(headerName, headers[headerName]);
						break;
				}
			}
		}

		/// <summary>
		/// Copies the contents of one stream to another.
		/// </summary>
		/// <param name="copyFrom">The stream to copy from, at the position where copying should begin.</param>
		/// <param name="copyTo">The stream to copy to, at the position where bytes should be written.</param>
		/// <param name="maximumBytesToCopy">The maximum bytes to copy.</param>
		/// <returns>The total number of bytes copied.</returns>
		/// <remarks>
		/// Copying begins at the streams' current positions.
		/// The positions are NOT reset after copying is complete.
		/// </remarks>
		internal static int CopyUpTo(this Stream copyFrom, Stream copyTo, int maximumBytesToCopy) {
			Requires.NotNull(copyFrom, "copyFrom");
			Requires.NotNull(copyTo, "copyTo");
			Requires.That(copyFrom.CanRead, "copyFrom", MessagingStrings.StreamUnreadable);
			Requires.That(copyTo.CanWrite, "copyTo", MessagingStrings.StreamUnwritable);

			byte[] buffer = new byte[1024];
			int readBytes;
			int totalCopiedBytes = 0;
			while ((readBytes = copyFrom.Read(buffer, 0, Math.Min(1024, maximumBytesToCopy))) > 0) {
				int writeBytes = Math.Min(maximumBytesToCopy, readBytes);
				copyTo.Write(buffer, 0, writeBytes);
				totalCopiedBytes += writeBytes;
				maximumBytesToCopy -= writeBytes;
			}

			return totalCopiedBytes;
		}

		/// <summary>
		/// Creates a snapshot of some stream so it is seekable, and the original can be closed.
		/// </summary>
		/// <param name="copyFrom">The stream to copy bytes from.</param>
		/// <returns>A seekable stream with the same contents as the original.</returns>
		internal static Stream CreateSnapshot(this Stream copyFrom) {
			Requires.NotNull(copyFrom, "copyFrom");
			Requires.That(copyFrom.CanRead, "copyFrom", MessagingStrings.StreamUnreadable);

			MemoryStream copyTo = new MemoryStream(copyFrom.CanSeek ? (int)copyFrom.Length : 4 * 1024);
			try {
				copyFrom.CopyTo(copyTo);
				copyTo.Position = 0;
				return copyTo;
			} catch {
				copyTo.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Clones an <see cref="HttpWebRequest" /> in order to send it again.
		/// </summary>
		/// <param name="message">The message to set headers on.</param>
		/// <param name="request">The request with headers to clone.</param>
		internal static void CopyHeadersFrom(this HttpRequestMessage message, HttpRequestBase request) {
			Requires.NotNull(request, "request");
			Requires.NotNull(message, "message");

			foreach (string headerName in request.Headers) {
				string[] headerValues = request.Headers.GetValues(headerName);
				if (headerName == "Authorization" && headerValues.Length > 1) {
					// The TryAddWithoutValidation doesn't do this quite right, so do it by hand.
					message.Headers.Authorization = AuthenticationHeaderValue.Parse(string.Join(",", headerValues));
				} else {
					if (!message.Headers.TryAddWithoutValidation(headerName, headerValues)) {
						message.Content.Headers.TryAddWithoutValidation(headerName, headerValues);
					}
				}
			}
		}

		/// <summary>
		/// Tests whether two arrays are equal in contents and ordering.
		/// </summary>
		/// <typeparam name="T">The type of elements in the arrays.</typeparam>
		/// <param name="first">The first array in the comparison.  May be null.</param>
		/// <param name="second">The second array in the comparison.  May be null.</param>
		/// <returns>True if the arrays equal; false otherwise.</returns>
		internal static bool AreEquivalent<T>(T[] first, T[] second) {
			if (first == null && second == null) {
				return true;
			}

			if (first == null || second == null) {
				return false;
			}

			if (first.Length != second.Length) {
				return false;
			}
			for (int i = 0; i < first.Length; i++) {
				if (!first[i].Equals(second[i])) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Tests whether two arrays are equal in contents and ordering,
		/// guaranteeing roughly equivalent execution time regardless of where a signature mismatch may exist.
		/// </summary>
		/// <param name="first">The first array in the comparison.  May not be null.</param>
		/// <param name="second">The second array in the comparison. May not be null.</param>
		/// <returns>True if the arrays equal; false otherwise.</returns>
		/// <remarks>
		/// Guaranteeing equal execution time is useful in mitigating against timing attacks on a signature
		/// or other secret.
		/// </remarks>
		internal static bool AreEquivalentConstantTime(byte[] first, byte[] second) {
			Requires.NotNull(first, "first");
			Requires.NotNull(second, "second");
			if (first.Length != second.Length) {
				return false;
			}

			int result = 0;
			for (int i = 0; i < first.Length; i++) {
				result |= first[i] ^ second[i];
			}
			return result == 0;
		}

		/// <summary>
		/// Tests two sequences for same contents and ordering.
		/// </summary>
		/// <typeparam name="T">The type of elements in the arrays.</typeparam>
		/// <param name="sequence1">The first sequence in the comparison.  May not be null.</param>
		/// <param name="sequence2">The second sequence in the comparison. May not be null.</param>
		/// <returns>True if the arrays equal; false otherwise.</returns>
		internal static bool AreEquivalent<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2) {
			if (sequence1 == null && sequence2 == null) {
				return true;
			}
			if ((sequence1 == null) ^ (sequence2 == null)) {
				return false;
			}

			IEnumerator<T> iterator1 = sequence1.GetEnumerator();
			IEnumerator<T> iterator2 = sequence2.GetEnumerator();
			bool movenext1, movenext2;
			while (true) {
				movenext1 = iterator1.MoveNext();
				movenext2 = iterator2.MoveNext();
				if (!movenext1 || !movenext2) { // if we've reached the end of at least one sequence
					break;
				}
				object obj1 = iterator1.Current;
				object obj2 = iterator2.Current;
				if (obj1 == null && obj2 == null) {
					continue; // both null is ok
				}
				if (obj1 == null ^ obj2 == null) {
					return false; // exactly one null is different
				}
				if (!obj1.Equals(obj2)) {
					return false; // if they're not equal to each other
				}
			}

			return movenext1 == movenext2; // did they both reach the end together?
		}

		/// <summary>
		/// Tests two unordered collections for same contents.
		/// </summary>
		/// <typeparam name="T">The type of elements in the collections.</typeparam>
		/// <param name="first">The first collection in the comparison.  May not be null.</param>
		/// <param name="second">The second collection in the comparison. May not be null.</param>
		/// <returns>True if the collections have the same contents; false otherwise.</returns>
		internal static bool AreEquivalentUnordered<T>(ICollection<T> first, ICollection<T> second) {
			if (first == null && second == null) {
				return true;
			}
			if ((first == null) ^ (second == null)) {
				return false;
			}

			if (first.Count != second.Count) {
				return false;
			}

			foreach (T value in first) {
				if (!second.Contains(value)) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Tests whether two dictionaries are equal in length and contents.
		/// </summary>
		/// <typeparam name="TKey">The type of keys in the dictionaries.</typeparam>
		/// <typeparam name="TValue">The type of values in the dictionaries.</typeparam>
		/// <param name="first">The first dictionary in the comparison.  May not be null.</param>
		/// <param name="second">The second dictionary in the comparison. May not be null.</param>
		/// <returns>True if the arrays equal; false otherwise.</returns>
		internal static bool AreEquivalent<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second) {
			Requires.NotNull(first, "first");
			Requires.NotNull(second, "second");
			return AreEquivalent(first.ToArray(), second.ToArray());
		}

		/// <summary>
		/// Concatenates a list of name-value pairs as key=value&amp;key=value,
		/// taking care to properly encode each key and value for URL
		/// transmission according to RFC 3986.  No ? is prefixed to the string.
		/// </summary>
		/// <param name="args">The dictionary of key/values to read from.</param>
		/// <returns>The formulated querystring style string.</returns>
		internal static string CreateQueryString(IEnumerable<KeyValuePair<string, string>> args) {
			Requires.NotNull(args, "args");

			if (!args.Any()) {
				return string.Empty;
			}
			StringBuilder sb = new StringBuilder(args.Count() * 10);

			foreach (var p in args) {
				ErrorUtilities.VerifyArgument(!string.IsNullOrEmpty(p.Key), MessagingStrings.UnexpectedNullOrEmptyKey);
				ErrorUtilities.VerifyArgument(p.Value != null, MessagingStrings.UnexpectedNullValue, p.Key);
				sb.Append(EscapeUriDataStringRfc3986(p.Key));
				sb.Append('=');
				sb.Append(EscapeUriDataStringRfc3986(p.Value));
				sb.Append('&');
			}
			sb.Length--; // remove trailing &

			return sb.ToString();
		}

		/// <summary>
		/// Adds a set of name-value pairs to the end of a given URL
		/// as part of the querystring piece.  Prefixes a ? or &amp; before
		/// first element as necessary.
		/// </summary>
		/// <param name="builder">The UriBuilder to add arguments to.</param>
		/// <param name="args">
		/// The arguments to add to the query.  
		/// If null, <paramref name="builder"/> is not changed.
		/// </param>
		/// <remarks>
		/// If the parameters to add match names of parameters that already are defined
		/// in the query string, the existing ones are <i>not</i> replaced.
		/// </remarks>
		internal static void AppendQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args) {
			Requires.NotNull(builder, "builder");

			if (args != null && args.Count() > 0) {
				StringBuilder sb = new StringBuilder(50 + (args.Count() * 10));
				if (!string.IsNullOrEmpty(builder.Query)) {
					sb.Append(builder.Query.Substring(1));
					sb.Append('&');
				}
				sb.Append(CreateQueryString(args));

				builder.Query = sb.ToString();
			}
		}

		/// <summary>
		/// Adds a set of name-value pairs to the end of a given URL
		/// as part of the fragment piece.  Prefixes a # or &amp; before
		/// first element as necessary.
		/// </summary>
		/// <param name="builder">The UriBuilder to add arguments to.</param>
		/// <param name="args">
		/// The arguments to add to the query.  
		/// If null, <paramref name="builder"/> is not changed.
		/// </param>
		/// <remarks>
		/// If the parameters to add match names of parameters that already are defined
		/// in the fragment, the existing ones are <i>not</i> replaced.
		/// </remarks>
		internal static void AppendFragmentArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args) {
			Requires.NotNull(builder, "builder");

			if (args != null && args.Count() > 0) {
				StringBuilder sb = new StringBuilder(50 + (args.Count() * 10));
				if (!string.IsNullOrEmpty(builder.Fragment)) {
					sb.Append(builder.Fragment);
					sb.Append('&');
				}
				sb.Append(CreateQueryString(args));

				builder.Fragment = sb.ToString();
			}
		}

		/// <summary>
		/// Adds parameters to a query string, replacing parameters that
		/// match ones that already exist in the query string.
		/// </summary>
		/// <param name="builder">The UriBuilder to add arguments to.</param>
		/// <param name="args">
		/// The arguments to add to the query.  
		/// If null, <paramref name="builder"/> is not changed.
		/// </param>
		internal static void AppendAndReplaceQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args) {
			Requires.NotNull(builder, "builder");

			if (args != null && args.Count() > 0) {
				NameValueCollection aggregatedArgs = HttpUtility.ParseQueryString(builder.Query);
				foreach (var pair in args) {
					aggregatedArgs[pair.Key] = pair.Value;
				}

				builder.Query = CreateQueryString(aggregatedArgs.ToDictionary());
			}
		}

		/// <summary>
		/// Extracts the recipient from an HttpRequestInfo.
		/// </summary>
		/// <param name="request">The request to get recipient information from.</param>
		/// <returns>The recipient.</returns>
		/// <exception cref="ArgumentException">Thrown if the HTTP request is something we can't handle.</exception>
		internal static MessageReceivingEndpoint GetRecipient(this HttpRequestMessage request) {
			return new MessageReceivingEndpoint(request.RequestUri, GetHttpDeliveryMethod(request.Method.Method));
		}

		/// <summary>
		/// Gets the <see cref="HttpDeliveryMethods"/> enum value for a given HTTP verb.
		/// </summary>
		/// <param name="httpVerb">The HTTP verb.</param>
		/// <returns>A <see cref="HttpDeliveryMethods"/> enum value that is within the <see cref="HttpDeliveryMethods.HttpVerbMask"/>.</returns>
		/// <exception cref="ArgumentException">Thrown if the HTTP request is something we can't handle.</exception>
		internal static HttpDeliveryMethods GetHttpDeliveryMethod(string httpVerb) {
			if (httpVerb == "GET") {
				return HttpDeliveryMethods.GetRequest;
			} else if (httpVerb == "POST") {
				return HttpDeliveryMethods.PostRequest;
			} else if (httpVerb == "PUT") {
				return HttpDeliveryMethods.PutRequest;
			} else if (httpVerb == "DELETE") {
				return HttpDeliveryMethods.DeleteRequest;
			} else if (httpVerb == "HEAD") {
				return HttpDeliveryMethods.HeadRequest;
			} else if (httpVerb == "PATCH") {
				return HttpDeliveryMethods.PatchRequest;
			} else if (httpVerb == "OPTIONS") {
				return HttpDeliveryMethods.OptionsRequest;
			} else {
				throw ErrorUtilities.ThrowArgumentNamed("httpVerb", MessagingStrings.UnsupportedHttpVerb, httpVerb);
			}
		}

		/// <summary>
		/// Gets the HTTP verb to use for a given <see cref="HttpDeliveryMethods"/> enum value.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <returns>An HTTP verb, such as GET, POST, PUT, DELETE, PATCH, or OPTION.</returns>
		internal static HttpMethod GetHttpVerb(HttpDeliveryMethods httpMethod) {
			if ((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.GetRequest) {
				return HttpMethod.Get;
			} else if ((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.PostRequest) {
				return HttpMethod.Post;
			} else if ((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.PutRequest) {
				return HttpMethod.Put;
			} else if ((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.DeleteRequest) {
				return HttpMethod.Delete;
			} else if ((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.HeadRequest) {
				return HttpMethod.Head;
			} else if ((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.PatchRequest) {
				return new HttpMethod("PATCH");
			} else if ((httpMethod & HttpDeliveryMethods.HttpVerbMask) == HttpDeliveryMethods.OptionsRequest) {
				return HttpMethod.Options;
			} else if ((httpMethod & HttpDeliveryMethods.AuthorizationHeaderRequest) != 0) {
				return HttpMethod.Get; // if AuthorizationHeaderRequest is specified without an explicit HTTP verb, assume GET.
			} else {
				throw ErrorUtilities.ThrowArgumentNamed("httpMethod", MessagingStrings.UnsupportedHttpVerb, httpMethod);
			}
		}

		/// <summary>
		/// Copies some extra parameters into a message.
		/// </summary>
		/// <param name="messageDictionary">The message to copy the extra data into.</param>
		/// <param name="extraParameters">The extra data to copy into the message.  May be null to do nothing.</param>
		internal static void AddExtraParameters(this MessageDictionary messageDictionary, IDictionary<string, string> extraParameters) {
			Requires.NotNull(messageDictionary, "messageDictionary");

			if (extraParameters != null) {
				foreach (var pair in extraParameters) {
					try {
						messageDictionary.Add(pair);
					} catch (ArgumentException ex) {
						throw ErrorUtilities.Wrap(ex, MessagingStrings.ExtraParameterAddFailure, pair.Key, pair.Value);
					}
				}
			}
		}

		/// <summary>
		/// Gets the URI that contains the entire payload that would be sent by the browser for the specified redirect-based request message.
		/// </summary>
		/// <param name="response">The redirecting response message.</param>
		/// <returns>The absolute URI that could be retrieved to send the same message the browser would.</returns>
		/// <exception cref="System.NotSupportedException">Thrown if the message is not a redirect message.</exception>
		internal static Uri GetDirectUriRequest(this HttpResponseMessage response) {
			Requires.NotNull(response, "response");
			Requires.Argument(
				response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.RedirectKeepVerb
				|| response.StatusCode == HttpStatusCode.RedirectMethod || response.StatusCode == HttpStatusCode.TemporaryRedirect,
				"response",
				"Redirecting response expected.");

			if (response.Headers.Location != null) {
				return response.Headers.Location;
			} else {
				// Some responses are so large that they're HTML/JS self-posting pages.
				// We can't create long URLs for those, at present.
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Collects a sequence of key=value pairs into a dictionary.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="sequence">The sequence.</param>
		/// <returns>A dictionary.</returns>
		internal static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> sequence) {
			Requires.NotNull(sequence, "sequence");
			return sequence.ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		/// <summary>
		/// Enumerates all members of the collection as key=value pairs.
		/// </summary>
		/// <param name="nvc">The collection to enumerate.</param>
		/// <returns>A sequence of pairs.</returns>
		internal static IEnumerable<KeyValuePair<string, string>> AsKeyValuePairs(this NameValueCollection nvc) {
			Requires.NotNull(nvc, "nvc");

			foreach (string key in nvc) {
				foreach (string value in nvc.GetValues(key)) {
					yield return new KeyValuePair<string, string>(key, value);
				}
			}
		}

		/// <summary>
		/// Converts a <see cref="NameValueCollection"/> to an IDictionary&lt;string, string&gt;.
		/// </summary>
		/// <param name="nvc">The NameValueCollection to convert.  May be null.</param>
		/// <returns>The generated dictionary, or null if <paramref name="nvc"/> is null.</returns>
		/// <remarks>
		/// If a <c>null</c> key is encountered, its value is ignored since
		/// <c>Dictionary&lt;string, string&gt;</c> does not allow null keys.
		/// </remarks>
		internal static Dictionary<string, string> ToDictionary(this NameValueCollection nvc) {
			return ToDictionary(nvc, false);
		}

		/// <summary>
		/// Converts a <see cref="NameValueCollection"/> to an IDictionary&lt;string, string&gt;.
		/// </summary>
		/// <param name="nvc">The NameValueCollection to convert.  May be null.</param>
		/// <param name="throwOnNullKey">
		/// A value indicating whether a null key in the <see cref="NameValueCollection"/> should be silently skipped since it is not a valid key in a Dictionary.  
		/// Use <c>true</c> to throw an exception if a null key is encountered.
		/// Use <c>false</c> to silently continue converting the valid keys.
		/// </param>
		/// <returns>The generated dictionary, or null if <paramref name="nvc"/> is null.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="throwOnNullKey"/> is <c>true</c> and a null key is encountered.</exception>
		internal static Dictionary<string, string> ToDictionary(this NameValueCollection nvc, bool throwOnNullKey) {
			if (nvc == null) {
				return null;
			}

			var dictionary = new Dictionary<string, string>();
			foreach (string key in nvc) {
				// NameValueCollection supports a null key, but Dictionary<K,V> does not.
				if (key == null) {
					if (throwOnNullKey) {
						throw new ArgumentException(MessagingStrings.UnexpectedNullKey);
					} else {
						// Only emit a warning if there was a non-empty value.
						if (!string.IsNullOrEmpty(nvc[key])) {
							Logger.OpenId.WarnFormat("Null key with value {0} encountered while translating NameValueCollection to Dictionary.", nvc[key]);
						}
					}
				} else {
					dictionary.Add(key, nvc[key]);
				}
			}

			return dictionary;
		}

		/// <summary>
		/// Converts a dictionary to a <see cref="NameValueCollection"/>
		/// </summary>
		/// <param name="data">The existing dictionary.</param>
		/// <returns>The new collection.</returns>
		internal static NameValueCollection ToNameValueCollection(this IDictionary<string, string> data) {
			Requires.NotNull(data, "data");

			var nvc = new NameValueCollection();
			foreach (var entry in data) {
				nvc.Add(entry.Key, entry.Value);
			}

			return nvc;
		}

		/// <summary>
		/// Sorts the elements of a sequence in ascending order by using a specified comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A sequence of values to order.</param>
		/// <param name="keySelector">A function to extract a key from an element.</param>
		/// <param name="comparer">A comparison function to compare keys.</param>
		/// <returns>An System.Linq.IOrderedEnumerable&lt;TElement&gt; whose elements are sorted according to a key.</returns>
		internal static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Comparison<TKey> comparer) {
			Requires.NotNull(source, "source");
			Requires.NotNull(comparer, "comparer");
			Requires.NotNull(keySelector, "keySelector");
			return System.Linq.Enumerable.OrderBy<TSource, TKey>(source, keySelector, new ComparisonHelper<TKey>(comparer));
		}

		/// <summary>
		/// Determines whether the specified message is a request (indirect message or direct request).
		/// </summary>
		/// <param name="message">The message in question.</param>
		/// <returns>
		/// 	<c>true</c> if the specified message is a request; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// Although an <see cref="IProtocolMessage"/> may implement the <see cref="IDirectedProtocolMessage"/>
		/// interface, it may only be doing that for its derived classes.  These objects are only requests
		/// if their <see cref="IDirectedProtocolMessage.Recipient"/> property is non-null.
		/// </remarks>
		internal static bool IsRequest(this IDirectedProtocolMessage message) {
			Requires.NotNull(message, "message");
			return message.Recipient != null;
		}

		/// <summary>
		/// Determines whether the specified message is a direct response.
		/// </summary>
		/// <param name="message">The message in question.</param>
		/// <returns>
		/// 	<c>true</c> if the specified message is a direct response; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// Although an <see cref="IProtocolMessage"/> may implement the 
		/// <see cref="IDirectResponseProtocolMessage"/> interface, it may only be doing 
		/// that for its derived classes.  These objects are only requests if their 
		/// <see cref="IDirectResponseProtocolMessage.OriginatingRequest"/> property is non-null.
		/// </remarks>
		internal static bool IsDirectResponse(this IDirectResponseProtocolMessage message) {
			Requires.NotNull(message, "message");
			return message.OriginatingRequest != null;
		}

		/// <summary>
		/// Writes a buffer, prefixed with its own length.
		/// </summary>
		/// <param name="writer">The binary writer.</param>
		/// <param name="buffer">The buffer.</param>
		internal static void WriteBuffer(this BinaryWriter writer, byte[] buffer) {
			Requires.NotNull(writer, "writer");
			Requires.NotNull(buffer, "buffer");
			writer.Write(buffer.Length);
			writer.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Reads a buffer that is prefixed with its own length.
		/// </summary>
		/// <param name="reader">The binary reader positioned at the buffer length.</param>
		/// <param name="maxBufferSize">
		/// The maximum size of the buffer that should be permitted. 
		/// Although the stream will indicate the size of the buffer, this mitigates data corruption
		/// or DoS attacks causing the web server to allocate too much memory for a small data packet.
		/// </param>
		/// <returns>The read buffer.</returns>
		internal static byte[] ReadBuffer(this BinaryReader reader, int maxBufferSize) {
			Requires.NotNull(reader, "reader");
			Requires.Range(maxBufferSize > 0 && maxBufferSize < 1024 * 1024, "maxBufferSize");
			int length = reader.ReadInt32();
			ErrorUtilities.VerifyProtocol(length <= maxBufferSize, MessagingStrings.DataCorruptionDetected);
			byte[] buffer = new byte[length];
			ErrorUtilities.VerifyProtocol(reader.Read(buffer, 0, length) == length, MessagingStrings.UnexpectedBufferLength);
			return buffer;
		}

		/// <summary>
		/// Constructs a Javascript expression that will create an object
		/// on the user agent when assigned to a variable.
		/// </summary>
		/// <param name="namesAndValues">The untrusted names and untrusted values to inject into the JSON object.</param>
		/// <param name="valuesPreEncoded">if set to <c>true</c> the values will NOT be escaped as if it were a pure string.</param>
		/// <returns>The Javascript JSON object as a string.</returns>
		internal static string CreateJsonObject(IEnumerable<KeyValuePair<string, string>> namesAndValues, bool valuesPreEncoded) {
			StringBuilder builder = new StringBuilder();
			builder.Append("{ ");

			foreach (var pair in namesAndValues) {
				builder.Append(MessagingUtilities.GetSafeJavascriptValue(pair.Key));
				builder.Append(": ");
				builder.Append(valuesPreEncoded ? pair.Value : MessagingUtilities.GetSafeJavascriptValue(pair.Value));
				builder.Append(",");
			}

			if (builder[builder.Length - 1] == ',') {
				builder.Length -= 1;
			}
			builder.Append("}");
			return builder.ToString();
		}

		/// <summary>
		/// Serializes the given message as a JSON string.
		/// </summary>
		/// <param name="message">The message to serialize.</param>
		/// <param name="messageDescriptions">The cached message descriptions to use for reflection.</param>
		/// <returns>A JSON string.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		internal static string SerializeAsJson(IMessage message, MessageDescriptionCollection messageDescriptions) {
			Requires.NotNull(message, "message");
			Requires.NotNull(messageDescriptions, "messageDescriptions");

			var encoding = Encoding.UTF8;
			var bytes = SerializeAsJsonBytes(message, messageDescriptions, encoding);
			string json = encoding.GetString(bytes);
			return json;
		}

		/// <summary>
		/// Serializes the given message as a JSON string.
		/// </summary>
		/// <param name="message">The message to serialize.</param>
		/// <param name="messageDescriptions">The cached message descriptions to use for reflection.</param>
		/// <param name="encoding">The encoding to use.  Defaults to <see cref="Encoding.UTF8"/></param>
		/// <returns>A JSON string.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This Dispose is safe.")]
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		internal static byte[] SerializeAsJsonBytes(IMessage message, MessageDescriptionCollection messageDescriptions, Encoding encoding = null) {
			Requires.NotNull(message, "message");
			Requires.NotNull(messageDescriptions, "messageDescriptions");

			encoding = encoding ?? Encoding.UTF8;
			MessageDictionary messageDictionary = messageDescriptions.GetAccessor(message);
			using (var memoryStream = new MemoryStream()) {
				using (var jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(memoryStream, encoding)) {
					MessageSerializer.Serialize(messageDictionary, jsonWriter);
					jsonWriter.Flush();
				}

				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Deserializes a JSON object into a message.
		/// </summary>
		/// <param name="jsonBytes">The buffer containing the JSON string.</param>
		/// <param name="receivingMessage">The message to deserialize the object into.</param>
		/// <param name="messageDescriptions">The cache of message descriptions.</param>
		/// <param name="encoding">The encoding that the JSON bytes are in.</param>
		internal static void DeserializeFromJson(byte[] jsonBytes, IMessage receivingMessage, MessageDescriptionCollection messageDescriptions, Encoding encoding = null) {
			Requires.NotNull(jsonBytes, "jsonBytes");
			Requires.NotNull(receivingMessage, "receivingMessage");
			Requires.NotNull(messageDescriptions, "messageDescriptions");

			encoding = encoding ?? Encoding.UTF8;
			MessageDictionary messageDictionary = messageDescriptions.GetAccessor(receivingMessage);
			using (var jsonReader = JsonReaderWriterFactory.CreateJsonReader(jsonBytes, 0, jsonBytes.Length, encoding, Channel.DefaultUntrustedXmlDictionaryReaderQuotas, null)) {
				MessageSerializer.Deserialize(messageDictionary, jsonReader);
			}
		}

		/// <summary>
		/// Prepares what SHOULD be simply a string value for safe injection into Javascript
		/// by using appropriate character escaping.
		/// </summary>
		/// <param name="value">The untrusted string value to be escaped to protected against XSS attacks.  May be null.</param>
		/// <returns>The escaped string, surrounded by single-quotes.</returns>
		internal static string GetSafeJavascriptValue(string value) {
			if (value == null) {
				return "null";
			}

			// We use a StringBuilder because we have potentially many replacements to do,
			// and we don't want to create a new string for every intermediate replacement step.
			StringBuilder builder = new StringBuilder(value);
			foreach (var pair in javascriptStaticStringEscaping) {
				builder.Replace(pair.Key, pair.Value);
			}
			builder.Insert(0, '\'');
			builder.Append('\'');
			return builder.ToString();
		}

		/// <summary>
		/// Escapes a string according to the URI data string rules given in RFC 3986.
		/// </summary>
		/// <param name="value">The value to escape.</param>
		/// <returns>The escaped value.</returns>
		/// <remarks>
		/// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
		/// RFC 3986 behavior if certain elements are present in a .config file.  Even if this
		/// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
		/// host actually having this configuration element present.
		/// </remarks>
		internal static string EscapeUriDataStringRfc3986(string value) {
			Requires.NotNull(value, "value");

			// fast path for empty values.
			if (value.Length == 0) {
				return value;
			}

			// Start with RFC 2396 escaping by calling the .NET method to do the work.
			// This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
			// If it does, the escaping we do that follows it will be a no-op since the
			// characters we search for to replace can't possibly exist in the string.
			StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));

			// Upgrade the escaping to RFC 3986, if necessary.
			for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++) {
				escaped.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
			}

			// Return the fully-RFC3986-escaped string.
			return escaped.ToString();
		}

		/// <summary>
		/// Ensures that UTC times are converted to local times.  Unspecified kinds are unchanged.
		/// </summary>
		/// <param name="value">The date-time to convert.</param>
		/// <returns>The date-time in local time.</returns>
		internal static DateTime ToLocalTimeSafe(this DateTime value) {
			if (value.Kind == DateTimeKind.Unspecified) {
				return value;
			}

			return value.ToLocalTime();
		}

		/// <summary>
		/// Ensures that local times are converted to UTC times.  Unspecified kinds are unchanged.
		/// </summary>
		/// <param name="value">The date-time to convert.</param>
		/// <returns>The date-time in UTC time.</returns>
		internal static DateTime ToUniversalTimeSafe(this DateTime value) {
			if (value.Kind == DateTimeKind.Unspecified) {
				return value;
			}

			return value.ToUniversalTime();
		}

		/// <summary>
		/// Gets the query data from the original request (before any URL rewriting has occurred.)
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// A <see cref="NameValueCollection"/> containing all the parameters in the query string.
		/// </returns>
		internal static NameValueCollection GetQueryStringBeforeRewriting(this HttpRequestBase request) {
			// This request URL may have been rewritten by the host site.
			// For openid protocol purposes, we really need to look at 
			// the original query parameters before any rewriting took place.
			Uri beforeRewriting = GetPublicFacingUrl(request);
			if (beforeRewriting == request.Url) {
				// No rewriting has taken place.
				return request.QueryString;
			} else {
				// Rewriting detected!  Recover the original request URI.
				ErrorUtilities.VerifyInternal(beforeRewriting != null, "UrlBeforeRewriting is null, so the query string cannot be determined.");
				return HttpUtility.ParseQueryString(beforeRewriting.Query);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the request's URL was rewritten by ASP.NET
		/// or some other module.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>A value indicating whether there is evidence that the URL of the request has been changed to some internal server (farm) representation.</returns>
		/// <value>
		///   <c>true</c> if this request's URL was rewritten; otherwise, <c>false</c>.
		/// </value>
		internal static bool GetIsUrlRewritten(this HttpRequestBase request) {
			return request.Url != GetPublicFacingUrl(request);
		}

		/// <summary>
		/// Gets the query or form data from the original request (before any URL rewriting has occurred.)
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// A set of name=value pairs.
		/// </returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive call")]
		internal static NameValueCollection GetQueryOrForm(this HttpRequestBase request) {
			Requires.NotNull(request, "request");
			return request.HttpMethod == "GET" ? GetQueryStringBeforeRewriting(request) : request.Form;
		}

		/// <summary>
		/// Creates a symmetric algorithm for use in encryption/decryption.
		/// </summary>
		/// <param name="key">The symmetric key to use for encryption/decryption.</param>
		/// <returns>A symmetric algorithm.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		private static SymmetricAlgorithm CreateSymmetricAlgorithm(byte[] key) {
			SymmetricAlgorithm result = null;
			try {
				result = SymmetricAlgorithm.Create();
				result.Mode = CipherMode.CBC;
				result.Key = key;
				return result;
			} catch {
				IDisposable disposableResult = result;
				if (disposableResult != null) {
					disposableResult.Dispose();
				}

				throw;
			}
		}

		/// <summary>
		/// A thread-safe, non-crypto random number generator.
		/// </summary>
		private static class ThreadSafeRandom {
			/// <summary>
			/// The initializer of all new <see cref="Random"/> instances.
			/// </summary>
			private static readonly Random threadRandomInitializer = new Random();

			/// <summary>
			/// A thread-local instance of <see cref="Random"/>
			/// </summary>
			[ThreadStatic]
			private static Random threadRandom;

			/// <summary>
			/// Gets a random number generator for use on the current thread only.
			/// </summary>
			public static Random RandomNumberGenerator {
				get {
					if (threadRandom == null) {
						lock (threadRandomInitializer) {
							threadRandom = new Random(threadRandomInitializer.Next());
						}
					}

					return threadRandom;
				}
			}
		}

		/// <summary>
		/// A class to convert a <see cref="Comparison&lt;T&gt;"/> into an <see cref="IComparer&lt;T&gt;"/>.
		/// </summary>
		/// <typeparam name="T">The type of objects being compared.</typeparam>
		private class ComparisonHelper<T> : IComparer<T> {
			/// <summary>
			/// The comparison method to use.
			/// </summary>
			private Comparison<T> comparison;

			/// <summary>
			/// Initializes a new instance of the ComparisonHelper class.
			/// </summary>
			/// <param name="comparison">The comparison method to use.</param>
			internal ComparisonHelper(Comparison<T> comparison) {
				Requires.NotNull(comparison, "comparison");

				this.comparison = comparison;
			}

			#region IComparer<T> Members

			/// <summary>
			/// Compares two instances of <typeparamref name="T"/>.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>Any of -1, 0, or 1 according to standard comparison rules.</returns>
			public int Compare(T x, T y) {
				return this.comparison(x, y);
			}

			#endregion
		}
	}
}
