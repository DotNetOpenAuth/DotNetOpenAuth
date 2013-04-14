//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http.Headers;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
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
		/// The web.config file-specified provider of web resource URLs.
		/// </summary>
		private static IEmbeddedResourceRetrieval embeddedResourceRetrieval = MessagingElement.Configuration.EmbeddedResourceRetrievalProvider.CreateInstance(null, false, null);

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
		/// Validates that a URL will be resolvable at runtime.
		/// </summary>
		/// <param name="page">The page hosting the control that receives this URL as a property.</param>
		/// <param name="designMode">If set to <c>true</c> the page is in design-time mode rather than runtime mode.</param>
		/// <param name="value">The URI to check.</param>
		/// <exception cref="UriFormatException">Thrown if the given URL is not a valid, resolvable URI.</exception>
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "Just to throw an exception on invalid input.")]
		internal static void ValidateResolvableUrl(Page page, bool designMode, string value) {
			if (string.IsNullOrEmpty(value)) {
				return;
			}

			if (page != null && !designMode) {
				Assumes.True(page.Request != null);

				// Validate new value by trying to construct a Realm object based on it.
				string relativeUrl = page.ResolveUrl(value);
				Assumes.True(page.Request.Url != null);
				Assumes.True(relativeUrl != null);
				new Uri(page.Request.Url, relativeUrl); // throws an exception on failure.
			} else {
				// We can't fully test it, but it should start with either ~/ or a protocol.
				if (Regex.IsMatch(value, @"^https?://")) {
					new Uri(value); // make sure it's fully-qualified, but ignore wildcards
				} else if (value.StartsWith("~/", StringComparison.Ordinal)) {
					// this is valid too
				} else {
					throw new UriFormatException();
				}
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
	}
}
