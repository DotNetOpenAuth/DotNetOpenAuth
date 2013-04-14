//-----------------------------------------------------------------------
// <copyright file="PortableUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using Validation;

	/// <summary>
	/// Common utility methods that can be compiled in a portable library.
	/// </summary>
	public static class PortableUtilities {
		/// <summary>
		/// A character array containing just the &amp; character.
		/// </summary>
		private static readonly char[] AmperstandArray = new char[] { '&' };

		/// <summary>
		/// A character array containing just the = character.
		/// </summary>
		private static readonly char[] EqualsArray = new char[] { '=' };

		/// <summary>
		/// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
		/// </summary>
		private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

		/// <summary>
		/// Gets a random number generator for use on the current thread only.
		/// </summary>
		internal static Random NonCryptoRandomDataGenerator {
			get { return ThreadSafeRandom.RandomNumberGenerator; }
		}

		/// <summary>
		/// Gets the assembly file version of the executing assembly, otherwise falls back to the assembly version.
		/// </summary>
		internal static string AssemblyFileVersion {
			get { return ThisAssembly.AssemblyInformationalVersion; }
		}

		/// <summary>
		/// Gets a human-readable description of the library name and version, including
		/// whether the build is an official or private one.
		/// </summary>
		internal static string LibraryVersion {
			get { return ThisAssembly.AssemblyProduct + " " + ThisAssembly.AssemblyInformationalVersion; }
		}

		/// <summary>
		/// Gets an HTTP header that can be included in outbound requests.
		/// </summary>
		internal static ProductInfoHeaderValue LibraryVersionHeader {
			get { return new ProductInfoHeaderValue(ThisAssembly.AssemblyProduct, AssemblyFileVersion); }
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
		/// Parses a query string into a sequence of key=value pairs.
		/// </summary>
		/// <param name="query">The query string. It may be null, empty or begin with a ? symbol.</param>
		/// <returns>A non-null sequence of pairs. Keys may be null if a "pair" is missing the = symbol.</returns>
		public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string query) {
			if (!string.IsNullOrEmpty(query)) {
				// Trim leading '?' if there is one.
				if (query[0] == '?') {
					query = query.Substring(1);
				}

				foreach (var pair in query.Split(AmperstandArray, StringSplitOptions.RemoveEmptyEntries)) {
					var keyValue = pair.Split(EqualsArray, 2);
					for (int i = 0; i < keyValue.Length; i++) {
						keyValue[i] = WebUtility.UrlDecode(keyValue[i]);
					}

					if (keyValue.Length == 1) {
						yield return new KeyValuePair<string, string>(null, keyValue[0]);
					} else {
						yield return new KeyValuePair<string, string>(keyValue[0], keyValue[1]);
					}
				}
			}
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

			var builder = new UriBuilder(uri);
			builder.Query =
				CreateQueryString(
					ParseQueryString(uri.Query).Where(p => p.Key == null || !p.Key.StartsWith(prefix, StringComparison.Ordinal)));
			return builder.Uri;
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
		/// Returns a read only instance of the specified list.
		/// </summary>
		/// <typeparam name="T">The type of elements in the list.</typeparam>
		/// <param name="list">The list.</param>
		/// <returns>The readonly list.</returns>
		internal static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list) {
			Requires.NotNull(list, "list");
			return list as IReadOnlyList<T> ?? new ReadOnlyCollection<T>(list);
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
				escaped.Replace(UriRfc3986CharsToEscape[i], HexEscape(UriRfc3986CharsToEscape[i][0]));
			}

			// Return the fully-RFC3986-escaped string.
			return escaped.ToString();
		}

		/// <summary>
		/// Provides equivalent behavior to Uri.HexEscape, which is missing from portable libraries.
		/// </summary>
		/// <param name="value">The character to convert.</param>
		/// <returns>A 3-character sequence beginning with the % sign, followed by two hexadecimal characters.</returns>
		internal static string HexEscape(char value) {
			return string.Format(CultureInfo.InvariantCulture, "%{0:X2}", (int)value);
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

			var sb = new StringBuilder(args.Count() * 10);
			foreach (var pair in args) {
				if (pair.Key != null) {
					sb.Append(EscapeUriDataStringRfc3986(pair.Key));
					sb.Append('=');
				}

				sb.Append(EscapeUriDataStringRfc3986(pair.Value));
				sb.Append('&');
			}

			sb.Length--; // remove trailing &

			return sb.ToString();
		}

		/// <summary>
		/// Initializes a new dictionary based on the contents of the specified key=value sequence.
		/// Entries with null keys are dropped. Duplicate keys are handled with last-one-wins policy.
		/// </summary>
		/// <param name="value">The sequence of key=value pairs..</param>
		/// <returns>The new dictionary.</returns>
		internal static Dictionary<string, string> ToDictionaryDropNullKeys(this IEnumerable<KeyValuePair<string, string>> value) {
			var dictionary = new Dictionary<string, string>();

			foreach (var pair in value) {
				if (pair.Key != null) {
					dictionary[pair.Key] = pair.Value;
				}
			}

			return dictionary;
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

			if (args != null && args.Any()) {
				var aggregatedArgs = ParseQueryString(builder.Query).ToDictionaryDropNullKeys();
				foreach (var pair in args) {
					aggregatedArgs[pair.Key] = pair.Value;
				}

				builder.Query = CreateQueryString(aggregatedArgs);
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
		/// Prepares a dictionary for printing as a string.
		/// </summary>
		/// <typeparam name="K">The type of the key.</typeparam>
		/// <typeparam name="V">The type of the value.</typeparam>
		/// <param name="pairs">The dictionary or sequence of name-value pairs.</param>
		/// <returns>An object whose ToString method will perform the actual work of generating the string.</returns>
		/// <remarks>
		/// The work isn't done until (and if) the
		/// <see cref="Object.ToString"/> method is actually called, which makes it great
		/// for logging complex objects without being in a conditional block.
		/// </remarks>
		internal static object ToStringDeferred<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs) {
			return new DelayedToString<IEnumerable<KeyValuePair<K, V>>>(
				pairs,
				p => {
					Requires.NotNull(pairs, "pairs");
					var dictionary = pairs as IDictionary<K, V>;
					var messageDictionary = pairs as MessageDictionary;
					StringBuilder sb = new StringBuilder(dictionary != null ? dictionary.Count * 40 : 200);
					foreach (var pair in pairs) {
						var key = pair.Key.ToString();
						string value = pair.Value.ToString();
						if (messageDictionary != null && messageDictionary.Description.Mapping.ContainsKey(key) && messageDictionary.Description.Mapping[key].IsSecuritySensitive) {
							value = "********";
						}

						sb.AppendFormat("\t{0}: {1}{2}", key, value, Environment.NewLine);
					}
					return sb.ToString();
				});
		}

		/// <summary>
		/// Offers deferred ToString processing for a list of elements, that are assumed
		/// to generate just a single-line string.
		/// </summary>
		/// <typeparam name="T">The type of elements contained in the list.</typeparam>
		/// <param name="list">The list of elements.</param>
		/// <returns>An object whose ToString method will perform the actual work of generating the string.</returns>
		internal static object ToStringDeferred<T>(this IEnumerable<T> list) {
			return ToStringDeferred<T>(list, false);
		}

		/// <summary>
		/// Offers deferred ToString processing for a list of elements.
		/// </summary>
		/// <typeparam name="T">The type of elements contained in the list.</typeparam>
		/// <param name="list">The list of elements.</param>
		/// <param name="multiLineElements">if set to <c>true</c>, special formatting will be applied to the output to make it clear where one element ends and the next begins.</param>
		/// <returns>An object whose ToString method will perform the actual work of generating the string.</returns>
		internal static object ToStringDeferred<T>(this IEnumerable<T> list, bool multiLineElements) {
			return new DelayedToString<IEnumerable<T>>(
				list,
				l => {
					// Code contracts not allowed in generator methods.
					ErrorUtilities.VerifyArgumentNotNull(l, "l");

					string newLine = Environment.NewLine;
					////Assumes.True(newLine != null && newLine.Length > 0);
					StringBuilder sb = new StringBuilder();
					if (multiLineElements) {
						sb.AppendLine("[{");
						foreach (T obj in l) {
							// Prepare the string repersentation of the object
							string objString = obj != null ? obj.ToString() : "<NULL>";

							// Indent every line printed
							objString = objString.Replace(newLine, Environment.NewLine + "\t");
							sb.Append("\t");
							sb.Append(objString);

							if (!objString.EndsWith(Environment.NewLine, StringComparison.Ordinal)) {
								sb.AppendLine();
							}
							sb.AppendLine("}, {");
						}
						if (sb.Length > 2 + Environment.NewLine.Length) { // if anything was in the enumeration
							sb.Length -= 2 + Environment.NewLine.Length; // trim off the last ", {\r\n"
						} else {
							sb.Length -= 1 + Environment.NewLine.Length; // trim off the opening {
						}
						sb.Append("]");
						return sb.ToString();
					} else {
						sb.Append("{");
						foreach (T obj in l) {
							sb.Append(obj != null ? obj.ToString() : "<NULL>");
							sb.AppendLine(",");
						}
						if (sb.Length > 1) {
							sb.Length -= 1;
						}
						sb.Append("}");
						return sb.ToString();
					}
				});
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
		/// Manages an individual deferred ToString call.
		/// </summary>
		/// <typeparam name="T">The type of object to be serialized as a string.</typeparam>
		private class DelayedToString<T> {
			/// <summary>
			/// The object that will be serialized if called upon.
			/// </summary>
			private readonly T obj;

			/// <summary>
			/// The method used to serialize <see cref="obj"/> to string form.
			/// </summary>
			private readonly Func<T, string> toString;

			/// <summary>
			/// Initializes a new instance of the DelayedToString class.
			/// </summary>
			/// <param name="obj">The object that may be serialized to string form.</param>
			/// <param name="toString">The method that will serialize the object if called upon.</param>
			public DelayedToString(T obj, Func<T, string> toString) {
				Requires.NotNull(toString, "toString");

				this.obj = obj;
				this.toString = toString;
			}

			/// <summary>
			/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
			/// </returns>
			public override string ToString() {
				return this.toString(this.obj) ?? string.Empty;
			}
		}
	}
}
