//-----------------------------------------------------------------------
// <copyright file="MessagingUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Security;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// A grab-bag of utility methods useful for the channel stack of the protocol.
	/// </summary>
	public static class MessagingUtilities {
		/// <summary>
		/// The cryptographically strong random data generator used for creating secrets.
		/// </summary>
		/// <remarks>The random number generator is thread-safe.</remarks>
		internal static readonly RandomNumberGenerator CryptoRandomDataGenerator = new RNGCryptoServiceProvider();

		/// <summary>
		/// A pseudo-random data generator (NOT cryptographically strong random data)
		/// </summary>
		internal static readonly Random NonCryptoRandomDataGenerator = new Random();

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
		/// The set of digits, and alphabetic letters (upper and lowercase) that are clearly
		/// visually distinguishable.
		/// </summary>
		internal const string AlphaNumericNoLookAlikes = "23456789abcdefghjkmnpqrstwxyzABCDEFGHJKMNPQRSTWXYZ";

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
		/// Transforms an OutgoingWebResponse to an MVC-friendly ActionResult.
		/// </summary>
		/// <param name="response">The response to send to the uesr agent.</param>
		/// <returns>The <see cref="ActionResult"/> instance to be returned by the Controller's action method.</returns>
		public static ActionResult AsActionResult(this OutgoingWebResponse response) {
			Contract.Requires(response != null);
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			return new OutgoingWebResponseActionResult(response);
		}

		/// <summary>
		/// Gets the original request URL, as seen from the browser before any URL rewrites on the server if any.
		/// Cookieless session directory (if applicable) is also included.
		/// </summary>
		/// <returns>The URL in the user agent's Location bar.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "The Uri merging requires use of a string value.")]
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive call should not be a property.")]
		public static Uri GetRequestUrlFromContext() {
			ErrorUtilities.VerifyHttpContext();
			HttpContext context = HttpContext.Current;

			// We use Request.Url for the full path to the server, and modify it
			// with Request.RawUrl to capture both the cookieless session "directory" if it exists
			// and the original path in case URL rewriting is going on.  We don't want to be
			// fooled by URL rewriting because we're comparing the actual URL with what's in
			// the return_to parameter in some cases.
			// Response.ApplyAppPathModifier(builder.Path) would have worked for the cookieless
			// session, but not the URL rewriting problem.
			return new Uri(context.Request.Url, context.Request.RawUrl);
		}

		/// <summary>
		/// Strips any and all URI query parameters that start with some prefix.
		/// </summary>
		/// <param name="uri">The URI that may have a query with parameters to remove.</param>
		/// <param name="prefix">The prefix for parameters to remove.  A period is NOT automatically appended.</param>
		/// <returns>Either a new Uri with the parameters removed if there were any to remove, or the same Uri instance if no parameters needed to be removed.</returns>
		public static Uri StripQueryArgumentsWithPrefix(this Uri uri, string prefix) {
			ErrorUtilities.VerifyArgumentNotNull(uri, "uri");
			ErrorUtilities.VerifyNonZeroLength(prefix, "prefix");

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
		internal static string GetAllMessages(this Exception exception) {
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
		/// Gets a cryptographically strong random sequence of values.
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
		/// Gets a random string made up of a given set of allowable characters.
		/// </summary>
		/// <param name="length">The length of the desired random string.</param>
		/// <param name="allowableCharacters">The allowable characters.</param>
		/// <returns>A random string.</returns>
		internal static string GetRandomString(int length, string allowableCharacters) {
			Contract.Requires(length >= 0);
			Contract.Requires(allowableCharacters != null && allowableCharacters.Length >= 2);

			char[] randomString = new char[length];
			for (int i = 0; i < length; i++) {
				randomString[i] = allowableCharacters[NonCryptoRandomDataGenerator.Next(allowableCharacters.Length)];
			}

			return new string(randomString);
		}

		/// <summary>
		/// Adds a set of HTTP headers to an <see cref="HttpResponse"/> instance,
		/// taking care to set some headers to the appropriate properties of
		/// <see cref="HttpResponse" />
		/// </summary>
		/// <param name="headers">The headers to add.</param>
		/// <param name="response">The <see cref="HttpResponse"/> instance to set the appropriate values to.</param>
		internal static void ApplyHeadersToResponse(WebHeaderCollection headers, HttpResponse response) {
			ErrorUtilities.VerifyArgumentNotNull(headers, "headers");
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

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
		/// Adds a set of HTTP headers to an <see cref="HttpResponse"/> instance,
		/// taking care to set some headers to the appropriate properties of
		/// <see cref="HttpResponse" />
		/// </summary>
		/// <param name="headers">The headers to add.</param>
		/// <param name="response">The <see cref="HttpListenerResponse"/> instance to set the appropriate values to.</param>
		internal static void ApplyHeadersToResponse(WebHeaderCollection headers, HttpListenerResponse response) {
			ErrorUtilities.VerifyArgumentNotNull(headers, "headers");
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

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
		/// <returns>The total number of bytes copied.</returns>
		/// <remarks>
		/// Copying begins at the streams' current positions.
		/// The positions are NOT reset after copying is complete.
		/// </remarks>
		internal static int CopyTo(this Stream copyFrom, Stream copyTo) {
			return CopyTo(copyFrom, copyTo, int.MaxValue);
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
		internal static int CopyTo(this Stream copyFrom, Stream copyTo, int maximumBytesToCopy) {
			ErrorUtilities.VerifyArgumentNotNull(copyFrom, "copyFrom");
			ErrorUtilities.VerifyArgumentNotNull(copyTo, "copyTo");
			ErrorUtilities.VerifyArgument(copyFrom.CanRead, MessagingStrings.StreamUnreadable);
			ErrorUtilities.VerifyArgument(copyTo.CanWrite, MessagingStrings.StreamUnwritable, "copyTo");

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
			ErrorUtilities.VerifyArgumentNotNull(copyFrom, "copyFrom");

			MemoryStream copyTo = new MemoryStream(copyFrom.CanSeek ? (int)copyFrom.Length : 4 * 1024);
			copyFrom.CopyTo(copyTo);
			copyTo.Position = 0;
			return copyTo;
		}

		/// <summary>
		/// Clones an <see cref="HttpWebRequest"/> in order to send it again.
		/// </summary>
		/// <param name="request">The request to clone.</param>
		/// <returns>The newly created instance.</returns>
		internal static HttpWebRequest Clone(this HttpWebRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			return Clone(request, request.RequestUri);
		}

		/// <summary>
		/// Clones an <see cref="HttpWebRequest"/> in order to send it again.
		/// </summary>
		/// <param name="request">The request to clone.</param>
		/// <param name="newRequestUri">The new recipient of the request.</param>
		/// <returns>The newly created instance.</returns>
		internal static HttpWebRequest Clone(this HttpWebRequest request, Uri newRequestUri) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifyArgumentNotNull(newRequestUri, "newRequestUri");

			var newRequest = (HttpWebRequest)WebRequest.Create(newRequestUri);

			// First copy headers.  Only set those that are explicitly set on the original request,
			// because some properties (like IfModifiedSince) activate special behavior when set,
			// even when set to their "original" values.
			foreach (string headerName in request.Headers) {
				switch (headerName) {
					case "Accept": newRequest.Accept = request.Accept; break;
					case "Connection": break; // Keep-Alive controls this
					case "Content-Length": newRequest.ContentLength = request.ContentLength; break;
					case "Content-Type": newRequest.ContentType = request.ContentType; break;
					case "Expect": newRequest.Expect = request.Expect; break;
					case "Host": break; // implicitly copied as part of the RequestUri
					case "If-Modified-Since": newRequest.IfModifiedSince = request.IfModifiedSince; break;
					case "Keep-Alive": newRequest.KeepAlive = request.KeepAlive; break;
					case "Proxy-Connection": break; // no property equivalent?
					case "Referer": newRequest.Referer = request.Referer; break;
					case "Transfer-Encoding": newRequest.TransferEncoding = request.TransferEncoding; break;
					case "User-Agent": newRequest.UserAgent = request.UserAgent; break;
					default: newRequest.Headers[headerName] = request.Headers[headerName]; break;
				}
			}

			newRequest.AllowAutoRedirect = request.AllowAutoRedirect;
			newRequest.AllowWriteStreamBuffering = request.AllowWriteStreamBuffering;
			newRequest.AuthenticationLevel = request.AuthenticationLevel;
			newRequest.AutomaticDecompression = request.AutomaticDecompression;
			newRequest.CachePolicy = request.CachePolicy;
			newRequest.ClientCertificates = request.ClientCertificates;
			newRequest.ConnectionGroupName = request.ConnectionGroupName;
			newRequest.ContinueDelegate = request.ContinueDelegate;
			newRequest.CookieContainer = request.CookieContainer;
			newRequest.Credentials = request.Credentials;
			newRequest.ImpersonationLevel = request.ImpersonationLevel;
			newRequest.MaximumAutomaticRedirections = request.MaximumAutomaticRedirections;
			newRequest.MaximumResponseHeadersLength = request.MaximumResponseHeadersLength;
			newRequest.MediaType = request.MediaType;
			newRequest.Method = request.Method;
			newRequest.Pipelined = request.Pipelined;
			newRequest.PreAuthenticate = request.PreAuthenticate;
			newRequest.ProtocolVersion = request.ProtocolVersion;
			newRequest.ReadWriteTimeout = request.ReadWriteTimeout;
			newRequest.SendChunked = request.SendChunked;
			newRequest.Timeout = request.Timeout;
			newRequest.UseDefaultCredentials = request.UseDefaultCredentials;

			try {
				newRequest.Proxy = request.Proxy;
				newRequest.UnsafeAuthenticatedConnectionSharing = request.UnsafeAuthenticatedConnectionSharing;
			} catch (SecurityException) {
				Logger.Messaging.Warn("Unable to clone some HttpWebRequest properties due to partial trust.");
			}

			return newRequest;
		}

		/// <summary>
		/// Tests whether two arrays are equal in contents and ordering.
		/// </summary>
		/// <typeparam name="T">The type of elements in the arrays.</typeparam>
		/// <param name="first">The first array in the comparison.  May not be null.</param>
		/// <param name="second">The second array in the comparison. May not be null.</param>
		/// <returns>True if the arrays equal; false otherwise.</returns>
		internal static bool AreEquivalent<T>(T[] first, T[] second) {
			ErrorUtilities.VerifyArgumentNotNull(first, "first");
			ErrorUtilities.VerifyArgumentNotNull(second, "second");
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
			Contract.Requires(args != null);
			Contract.Ensures(Contract.Result<string>() != null);
			ErrorUtilities.VerifyArgumentNotNull(args, "args");
			if (args.Count() == 0) {
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
			ErrorUtilities.VerifyArgumentNotNull(builder, "builder");

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
		/// Adds parameters to a query string, replacing parameters that
		/// match ones that already exist in the query string.
		/// </summary>
		/// <param name="builder">The UriBuilder to add arguments to.</param>
		/// <param name="args">
		/// The arguments to add to the query.  
		/// If null, <paramref name="builder"/> is not changed.
		/// </param>
		internal static void AppendAndReplaceQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args) {
			ErrorUtilities.VerifyArgumentNotNull(builder, "builder");

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
		internal static MessageReceivingEndpoint GetRecipient(this HttpRequestInfo request) {
			return new MessageReceivingEndpoint(request.UrlBeforeRewriting, request.HttpMethod == "GET" ? HttpDeliveryMethods.GetRequest : HttpDeliveryMethods.PostRequest);
		}

		/// <summary>
		/// Copies some extra parameters into a message.
		/// </summary>
		/// <param name="messageDictionary">The message to copy the extra data into.</param>
		/// <param name="extraParameters">The extra data to copy into the message.  May be null to do nothing.</param>
		internal static void AddExtraParameters(this MessageDictionary messageDictionary, IDictionary<string, string> extraParameters) {
			ErrorUtilities.VerifyArgumentNotNull(messageDictionary, "messageAccessor");

			if (extraParameters != null) {
				foreach (var pair in extraParameters) {
					messageDictionary.Add(pair);
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
			Contract.Ensures((nvc != null && Contract.Result<Dictionary<string, string>>() != null) || (nvc == null && Contract.Result<Dictionary<string, string>>() == null));
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
			Contract.Ensures((nvc != null && Contract.Result<Dictionary<string, string>>() != null) || (nvc == null && Contract.Result<Dictionary<string, string>>() == null));
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
						Logger.OpenId.WarnFormat("Null key with value {0} encountered while translating NameValueCollection to Dictionary.", nvc[key]);
					}
				} else {
					dictionary.Add(key, nvc[key]);
				}
			}

			return dictionary;
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
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
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
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			return message.OriginatingRequest != null;
		}

		/// <summary>
		/// Constructs a Javascript expression that will create an object
		/// on the user agent when assigned to a variable.
		/// </summary>
		/// <param name="namesAndValues">The untrusted names and untrusted values to inject into the JSON object.</param>
		/// <returns>The Javascript JSON object as a string.</returns>
		internal static string CreateJsonObject(IEnumerable<KeyValuePair<string, string>> namesAndValues) {
			StringBuilder builder = new StringBuilder();
			builder.Append("{ ");

			foreach (var pair in namesAndValues) {
				builder.Append(MessagingUtilities.GetSafeJavascriptValue(pair.Key));
				builder.Append(": ");
				builder.Append(MessagingUtilities.GetSafeJavascriptValue(pair.Value));
				builder.Append(",");
			}

			if (builder[builder.Length - 1] == ',') {
				builder.Length -= 1;
			}
			builder.Append("}");
			return builder.ToString();
		}

		/// <summary>
		/// Prepares what SHOULD be simply a string value for safe injection into Javascript
		/// by using appropriate character escaping.
		/// </summary>
		/// <param name="value">The untrusted string value to be escaped to protected against XSS attacks.  May be null.</param>
		/// <returns>The escaped string.</returns>
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
				if (comparison == null) {
					throw new ArgumentNullException("comparison");
				}

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
