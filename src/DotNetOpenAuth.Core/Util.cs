//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http.Headers;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.UI;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using Validation;

	/// <summary>
	/// A grab-bag utility class.
	/// </summary>
	internal static class Util {
		/// <summary>
		/// The base namespace for this library from which all other namespaces derive.
		/// </summary>
		internal const string DefaultNamespace = "DotNetOpenAuth";

		/// <summary>
		/// A lazily-assembled string that describes the version of the library.
		/// </summary>
		private static readonly Lazy<string> libraryVersionLazy = new Lazy<string>(delegate {
			var assembly = Assembly.GetExecutingAssembly();
			string assemblyFullName = assembly.FullName;
			bool official = assemblyFullName.Contains("PublicKeyToken=d0acff3d13b42a9d");
			assemblyFullName = assemblyFullName.Replace(assembly.GetName().Version.ToString(), AssemblyFileVersion);

			// We use InvariantCulture since this is used for logging.
			return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", assemblyFullName, official ? "official" : "private");
		});

		/// <summary>
		/// A lazily-assembled string that describes the version of the library.
		/// </summary>
		private static readonly Lazy<ProductInfoHeaderValue> libraryVersionHeaderLazy = new Lazy<ProductInfoHeaderValue>(delegate {
			var assemblyName = Assembly.GetExecutingAssembly().GetName();
			return new ProductInfoHeaderValue(assemblyName.Name, AssemblyFileVersion);
		});

		/// <summary>
		/// The web.config file-specified provider of web resource URLs.
		/// </summary>
		private static IEmbeddedResourceRetrieval embeddedResourceRetrieval = MessagingElement.Configuration.EmbeddedResourceRetrievalProvider.CreateInstance(null, false, null);

		/// <summary>
		/// Gets a human-readable description of the library name and version, including
		/// whether the build is an official or private one.
		/// </summary>
		internal static string LibraryVersion {
			get { return libraryVersionLazy.Value; }
		}

		/// <summary>
		/// Gets an HTTP header that can be included in outbound requests.
		/// </summary>
		internal static ProductInfoHeaderValue LibraryVersionHeader {
			get { return libraryVersionHeaderLazy.Value; }
		}

		/// <summary>
		/// Gets the assembly file version of the executing assembly, otherwise falls back to the assembly version.
		/// </summary>
		internal static string AssemblyFileVersion {
			get {
				var assembly = Assembly.GetExecutingAssembly();
				var attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
				if (attributes.Length == 1) {
					var fileVersionAttribute = (AssemblyFileVersionAttribute)attributes[0];
					return fileVersionAttribute.Version;
				}

				return assembly.GetName().Version.ToString();
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
		/// Gets the web resource URL from a Page or <see cref="IEmbeddedResourceRetrieval"/> object.
		/// </summary>
		/// <param name="someTypeInResourceAssembly">Some type in resource assembly.</param>
		/// <param name="manifestResourceName">Name of the manifest resource.</param>
		/// <returns>An absolute URL</returns>
		internal static string GetWebResourceUrl(Type someTypeInResourceAssembly, string manifestResourceName) {
			Page page;
			IEmbeddedResourceRetrieval retrieval;

			if (embeddedResourceRetrieval != null) {
				Uri url = embeddedResourceRetrieval.GetWebResourceUrl(someTypeInResourceAssembly, manifestResourceName);
				return url != null ? url.AbsoluteUri : null;
			} else if ((page = HttpContext.Current.CurrentHandler as Page) != null) {
				return page.ClientScript.GetWebResourceUrl(someTypeInResourceAssembly, manifestResourceName);
			} else if ((retrieval = HttpContext.Current.CurrentHandler as IEmbeddedResourceRetrieval) != null) {
				return retrieval.GetWebResourceUrl(someTypeInResourceAssembly, manifestResourceName).AbsoluteUri;
			} else {
				throw new InvalidOperationException(
					string.Format(
						CultureInfo.CurrentCulture,
						Strings.EmbeddedResourceUrlProviderRequired,
						string.Join(", ", new string[] { typeof(Page).FullName, typeof(IEmbeddedResourceRetrieval).FullName })));
			}
		}

		/// <summary>
		/// Creates a dictionary of a sequence of elements and the result of an asynchronous transform,
		/// allowing the async work to proceed concurrently.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="transform">The transform.</param>
		/// <returns>A dictionary populated with the results of the transforms.</returns>
		internal static async Task<Dictionary<TSource, TResult>> ToDictionaryAsync<TSource, TResult>(
			this IEnumerable<TSource> source, Func<TSource, Task<TResult>> transform) {
			var taskResults = source.ToDictionary(s => s, transform);
			await Task.WhenAll(taskResults.Values);
			return taskResults.ToDictionary(p => p.Key, p => p.Value.Result);
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
