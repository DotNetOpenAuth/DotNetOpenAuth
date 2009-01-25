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
		/// The base namespace for this library from which all other namespaces derive.
		/// </summary>
		internal const string DefaultNamespace = "DotNetOpenAuth";

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
					var dictionary = pairs as IDictionary<K, V>;
					StringBuilder sb = new StringBuilder(dictionary != null ? dictionary.Count * 40 : 200);
					foreach (var pair in pairs) {
						sb.AppendFormat("\t{0}: {1}{2}", pair.Key, pair.Value, Environment.NewLine);
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
		/// Manages an individual deferred ToString call.
		/// </summary>
		/// <typeparam name="T">The type of object to be serialized as a string.</typeparam>
		private class DelayedToString<T> {
			/// <summary>
			/// The object that will be serialized if called upon.
			/// </summary>
			private T obj;

			/// <summary>
			/// The method used to serialize <see cref="obj"/> to string form.
			/// </summary>
			private Func<T, string> toString;

			/// <summary>
			/// Initializes a new instance of the DelayedToString class.
			/// </summary>
			/// <param name="obj">The object that may be serialized to string form.</param>
			/// <param name="toString">The method that will serialize the object if called upon.</param>
			public DelayedToString(T obj, Func<T, string> toString) {
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
				return this.toString(this.obj);
			}
		}
	}
}
