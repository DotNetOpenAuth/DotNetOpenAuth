//-----------------------------------------------------------------------
// <copyright file="TestUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System.Collections.Specialized;

	using DotNetOpenAuth.Logging;

	using Validation;

	/// <summary>
	/// An assortment of methods useful for testing.
	/// </summary>
	internal static class TestUtilities {
		/// <summary>
		/// The logger that tests should use.
		/// </summary>
		internal static readonly ILog TestLogger = LogProvider.GetLogger("DotNetOpenAuth.Test");

		internal static void ApplyTo(this NameValueCollection source, NameValueCollection target) {
			Requires.NotNull(source, "source");
			Requires.NotNull(target, "target");

			foreach (string header in source) {
				target[header] = source[header];
			}
		}

		internal static T Clone<T>(this T source) where T : NameValueCollection, new() {
			Requires.NotNull(source, "source");

			var result = new T();
			ApplyTo(source, result);
			return result;
		}
	}
}
