//-----------------------------------------------------------------------
// <copyright file="InfoCardErrorUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Validation;

	/// <summary>
	/// Error reporting methods specific to InfoCard validation.
	/// </summary>
	internal static class InfoCardErrorUtilities {
		/// <summary>
		/// Checks a condition and throws an <see cref="InfoCard.InformationCardException"/> 
		/// if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <param name="args">The formatting arguments.</param>
		/// <exception cref="InfoCard.InformationCardException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyInfoCard(bool condition, string errorMessage, params object[] args) {
			Requires.NotNull(args, "args");
			Assumes.True(errorMessage != null);
			if (!condition) {
				errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, args);
				throw new InfoCard.InformationCardException(errorMessage);
			}
		}
	}
}
