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
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
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
		/// Gets the original request URL, as seen from the browser before any URL rewrites on the server if any.
		/// Cookieless session directory (if applicable) is also included.
		/// </summary>
		/// <returns>The URL in the user agent's Location bar.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "The Uri merging requires use of a string value.")]
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive call should not be a property.")]
		public static Uri GetRequestUrlFromContext() {
			HttpContext context = HttpContext.Current;
			if (context == null) {
				throw new InvalidOperationException(MessagingStrings.CurrentHttpContextRequired);
			}

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
		/// Gets the query data from the original request (before any URL rewriting has occurred.)
		/// </summary>
		/// <returns>A <see cref="NameValueCollection"/> containing all the parameters in the query string.</returns>
		public static NameValueCollection GetQueryFromContextNVC() {
			ErrorUtilities.VerifyOperation(HttpContext.Current != null, MessagingStrings.HttpContextRequired);

			HttpRequest request = HttpContext.Current.Request;

			// This request URL may have been rewritten by the host site.
			// For openid protocol purposes, we really need to look at 
			// the original query parameters before any rewriting took place.
			if (request.Url.PathAndQuery == request.RawUrl) {
				// No rewriting has taken place.
				return request.QueryString;
			} else {
				// Rewriting detected!  Recover the original request URI.
				return HttpUtility.ParseQueryString(GetRequestUrlFromContext().Query);
			}
		}

		/// <summary>
		/// Strips any and all URI query parameters that start with some prefix.
		/// </summary>
		/// <param name="uri">The URI that may have a query with parameters to remove.</param>
		/// <param name="prefix">The prefix for parameters to remove.</param>
		/// <returns>Either a new Uri with the parameters removed if there were any to remove, or the same Uri instance if no parameters needed to be removed.</returns>
		public static Uri StripQueryArgumentsWithPrefix(this Uri uri, string prefix) {
			ErrorUtilities.VerifyArgumentNotNull(uri, "uri");

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
			newRequest.Accept = request.Accept;
			newRequest.AllowAutoRedirect = request.AllowAutoRedirect;
			newRequest.AllowWriteStreamBuffering = request.AllowWriteStreamBuffering;
			newRequest.AuthenticationLevel = request.AuthenticationLevel;
			newRequest.AutomaticDecompression = request.AutomaticDecompression;
			newRequest.CachePolicy = request.CachePolicy;
			newRequest.ClientCertificates = request.ClientCertificates;
			newRequest.ConnectionGroupName = request.ConnectionGroupName;
			if (request.ContentLength >= 0) {
				newRequest.ContentLength = request.ContentLength;
			}
			newRequest.ContentType = request.ContentType;
			newRequest.ContinueDelegate = request.ContinueDelegate;
			newRequest.CookieContainer = request.CookieContainer;
			newRequest.Credentials = request.Credentials;
			newRequest.Expect = request.Expect;
			newRequest.IfModifiedSince = request.IfModifiedSince;
			newRequest.ImpersonationLevel = request.ImpersonationLevel;
			newRequest.KeepAlive = request.KeepAlive;
			newRequest.MaximumAutomaticRedirections = request.MaximumAutomaticRedirections;
			newRequest.MaximumResponseHeadersLength = request.MaximumResponseHeadersLength;
			newRequest.MediaType = request.MediaType;
			newRequest.Method = request.Method;
			newRequest.Pipelined = request.Pipelined;
			newRequest.PreAuthenticate = request.PreAuthenticate;
			newRequest.ProtocolVersion = request.ProtocolVersion;
			newRequest.Proxy = request.Proxy;
			newRequest.ReadWriteTimeout = request.ReadWriteTimeout;
			newRequest.Referer = request.Referer;
			newRequest.SendChunked = request.SendChunked;
			newRequest.Timeout = request.Timeout;
			newRequest.TransferEncoding = request.TransferEncoding;
			newRequest.UnsafeAuthenticatedConnectionSharing = request.UnsafeAuthenticatedConnectionSharing;
			newRequest.UseDefaultCredentials = request.UseDefaultCredentials;
			newRequest.UserAgent = request.UserAgent;

			// We copy headers last, and only those that do not yet exist as a result
			// of setting these properties, so as to avoid exceptions thrown because 
			// there are properties .NET wants us to use rather than direct headers.
			foreach (string header in request.Headers) {
				if (string.IsNullOrEmpty(newRequest.Headers[header])) {
					newRequest.Headers.Add(header, request.Headers[header]);
				}
			}

			return newRequest;
		}

		/// <summary>
		/// Tests whether two arrays are equal in length and contents.
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
		/// transmission.  No ? is prefixed to the string.
		/// </summary>
		/// <param name="args">The dictionary of key/values to read from.</param>
		/// <returns>The formulated querystring style string.</returns>
		internal static string CreateQueryString(IEnumerable<KeyValuePair<string, string>> args) {
			ErrorUtilities.VerifyArgumentNotNull(args, "args");
			if (args.Count() == 0) {
				return string.Empty;
			}
			StringBuilder sb = new StringBuilder(args.Count() * 10);

			foreach (var p in args) {
				ErrorUtilities.VerifyArgument(p.Key != null, MessagingStrings.UnexpectedNullKey);
				ErrorUtilities.VerifyArgument(p.Value != null, MessagingStrings.UnexpectedNullValue, p.Key);
				sb.Append(HttpUtility.UrlEncode(p.Key));
				sb.Append('=');
				sb.Append(HttpUtility.UrlEncode(p.Value));
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
		internal static void AppendQueryArgs(this UriBuilder builder, IEnumerable<KeyValuePair<string, string>> args) {
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}

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
		/// Extracts the recipient from an HttpRequestInfo.
		/// </summary>
		/// <param name="request">The request to get recipient information from.</param>
		/// <returns>The recipient.</returns>
		internal static MessageReceivingEndpoint GetRecipient(this HttpRequestInfo request) {
			return new MessageReceivingEndpoint(request.Url, request.HttpMethod == "GET" ? HttpDeliveryMethods.GetRequest : HttpDeliveryMethods.PostRequest);
		}

		/// <summary>
		/// Copies some extra parameters into a message.
		/// </summary>
		/// <param name="message">The message to copy the extra data into.</param>
		/// <param name="extraParameters">The extra data to copy into the message.  May be null to do nothing.</param>
		internal static void AddExtraParameters(this IMessage message, IDictionary<string, string> extraParameters) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			if (extraParameters != null) {
				MessageDictionary messageDictionary = new MessageDictionary(message);
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
		internal static Dictionary<string, string> ToDictionary(this NameValueCollection nvc) {
			if (nvc == null) {
				return null;
			}

			var dictionary = new Dictionary<string, string>();
			foreach (string key in nvc) {
				dictionary.Add(key, nvc[key]);
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
