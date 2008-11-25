//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Net;
	using System.Reflection;
	using System.Text;

	/// <summary>
	/// A grab-bag utility class.
	/// </summary>
	internal static class Util {
		/// <summary>
		/// Gets a human-readable description of the library name and version, including
		/// whether the build is an official or private one.
		/// </summary>
		public static string LibraryVersion {
			get {
				string assemblyFullName = Assembly.GetExecutingAssembly().FullName;
				bool official = assemblyFullName.Contains("PublicKeyToken=2780ccd10d57b246");

				// We use InvariantCulture since this is used for logging.
				return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", assemblyFullName, official ? "official" : "private");
			}
		}

		/// <summary>
		/// Tests for equality between two objects.  Safely handles the case where one or both are null.
		/// </summary>
		/// <typeparam name="T">The type of objects been checked for equality.</typeparam>
		/// <param name="first">The first object.</param>
		/// <param name="second">The second object.</param>
		/// <returns><c>true</c> if the two objects are equal; <c>false</c> otherwise.</returns>
		internal static bool EqualsNullSafe<T>(this T first, T second) where T : class {
			// If one is null and the other is not...
			if (object.ReferenceEquals(first, null) ^ object.ReferenceEquals(second, null)) {
				return false;
			}

			// If both are null... (we only check one because we already know both are either null or non-null)
			if (object.ReferenceEquals(first, null)) {
				return true;
			}

			// Neither are null.  Delegate to the Equals method.
			return first.Equals(second);
		}

		/// <summary>
		/// Prepares a dictionary for printing as a string.
		/// </summary>
		/// <remarks>
		/// The work isn't done until (and if) the 
		/// <see cref="Object.ToString"/> method is actually called, which makes it great
		/// for logging complex objects without being in a conditional block.
		/// </remarks>
		internal static object ToStringDeferred<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs) {
			return new DelayedToString<IEnumerable<KeyValuePair<K, V>>>(
				pairs,
				p => {
					var dictionary = pairs as IDictionary<K, V>;
					StringBuilder sb = new StringBuilder(dictionary != null ? dictionary.Count * 40 : 200);
					foreach (var pair in pairs) {
						sb.AppendFormat("\t{0}: {1}{2}", pair.Key, pair.Value, Environment.NewLine);
					}
					return sb.ToString();
				});
		}

		internal static object ToStringDeferred<T>(this IEnumerable<T> list) {
			return ToStringDeferred<T>(list, false);
		}

		internal static object ToStringDeferred<T>(this IEnumerable<T> list, bool multiLineElements) {
			return new DelayedToString<IEnumerable<T>>(
				list,
				l => {
					StringBuilder sb = new StringBuilder();
					if (multiLineElements) {
						sb.AppendLine("[{");
						foreach (T obj in l) {
							// Prepare the string repersentation of the object
							string objString = obj != null ? obj.ToString() : "<NULL>";

							// Indent every line printed
							objString = objString.Replace(Environment.NewLine, Environment.NewLine + "\t");
							sb.Append("\t");
							sb.Append(objString);

							if (!objString.EndsWith(Environment.NewLine)) {
								sb.AppendLine();
							}
							sb.AppendLine("}, {");
						}
						if (sb.Length > 2) { // if anything was in the enumeration
							sb.Length -= 2 + Environment.NewLine.Length; // trim off the last ", {\r\n"
						} else {
							sb.Length -= 1; // trim off the opening {
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

		internal static HttpWebRequest CreatePostRequest(Uri requestUri, string body) {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = body.Length;
			request.Method = "POST";
			return request;
		}

		private class DelayedToString<T> {
			private T obj;

			private Func<T, string> toString;

			public DelayedToString(T obj, Func<T, string> toString) {
				this.obj = obj;
				this.toString = toString;
			}

			public override string ToString() {
				return this.toString(this.obj);
			}
		}
	}
}
