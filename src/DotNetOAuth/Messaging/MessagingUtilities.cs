//-----------------------------------------------------------------------
// <copyright file="MessagingUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;
	using DotNetOAuth.Messaging.Reflection;

	/// <summary>
	/// A grab-bag of utility methods useful for the channel stack of the protocol.
	/// </summary>
	internal static class MessagingUtilities {
		/// <summary>
		/// Adds a set of HTTP headers to an <see cref="HttpResponse"/> instance,
		/// taking care to set some headers to the appropriate properties of
		/// <see cref="HttpResponse" />
		/// </summary>
		/// <param name="headers">The headers to add.</param>
		/// <param name="response">The <see cref="HttpResponse"/> instance to set the appropriate values to.</param>
		internal static void ApplyHeadersToResponse(WebHeaderCollection headers, HttpResponse response) {
			if (headers == null) {
				throw new ArgumentNullException("headers");
			}
			if (response == null) {
				throw new ArgumentNullException("response");
			}
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
		/// <remarks>
		/// Copying begins at the streams' current positions.
		/// The positions are NOT reset after copying is complete.
		/// </remarks>
		internal static void CopyTo(this Stream copyFrom, Stream copyTo) {
			if (copyFrom == null) {
				throw new ArgumentNullException("copyFrom");
			}
			if (copyTo == null) {
				throw new ArgumentNullException("copyTo");
			}
			if (!copyFrom.CanRead) {
				throw new ArgumentException(MessagingStrings.StreamUnreadable, "copyFrom");
			}
			if (!copyTo.CanWrite) {
				throw new ArgumentException(MessagingStrings.StreamUnwritable, "copyTo");
			}

			byte[] buffer = new byte[1024];
			int readBytes;
			while ((readBytes = copyFrom.Read(buffer, 0, 1024)) > 0) {
				copyTo.Write(buffer, 0, readBytes);
			}
		}

		/// <summary>
		/// Concatenates a list of name-value pairs as key=value&amp;key=value,
		/// taking care to properly encode each key and value for URL
		/// transmission.  No ? is prefixed to the string.
		/// </summary>
		/// <param name="args">The dictionary of key/values to read from.</param>
		/// <returns>The formulated querystring style string.</returns>
		internal static string CreateQueryString(IDictionary<string, string> args) {
			if (args == null) {
				throw new ArgumentNullException("args");
			}
			if (args.Count == 0) {
				return string.Empty;
			}
			StringBuilder sb = new StringBuilder(args.Count * 10);

			foreach (var p in args) {
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
		internal static void AppendQueryArgs(UriBuilder builder, IDictionary<string, string> args) {
			if (builder == null) {
				throw new ArgumentNullException("builder");
			}

			if (args != null && args.Count > 0) {
				StringBuilder sb = new StringBuilder(50 + (args.Count * 10));
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
			return new MessageReceivingEndpoint(request.Url, request.HttpMethod == "GET" ? HttpDeliveryMethod.GetRequest : HttpDeliveryMethod.PostRequest);
		}

		/// <summary>
		/// Copies some extra parameters into a message.
		/// </summary>
		/// <param name="message">The message to copy the extra data into.</param>
		/// <param name="extraParameters">The extra data to copy into the message.  May be null to do nothing.</param>
		internal static void AddExtraFields(this IProtocolMessage message, IDictionary<string, string> extraParameters) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

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
