//-----------------------------------------------------------------------
// <copyright file="RequiresEx.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Argument validation checks that throw some kind of ArgumentException when they fail (unless otherwise noted).
	/// </summary>
	internal static class RequiresEx {
		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="InvalidOperationException"/>.</param>
		[Pure, DebuggerStepThrough]
		internal static void ValidState(bool condition) {
			if (!condition) {
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="InvalidOperationException"/>.</param>
		/// <param name="message">The message to include with the exception.</param>
		[Pure, DebuggerStepThrough]
		internal static void ValidState(bool condition, string message) {
			if (!condition) {
				throw new InvalidOperationException(message);
			}
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="InvalidOperationException"/>.</param>
		/// <param name="unformattedMessage">The unformatted message.</param>
		/// <param name="args">Formatting arguments.</param>
		[Pure, DebuggerStepThrough]
		internal static void ValidState(bool condition, string unformattedMessage, params object[] args) {
			if (!condition) {
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, unformattedMessage, args));
			}
		}

		/// <summary>
		/// Validates that some argument describes a type that is or derives from a required type.
		/// </summary>
		/// <typeparam name="T">The type that the argument must be or derive from.</typeparam>
		/// <param name="type">The type given in the argument.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		[Pure, DebuggerStepThrough]
		internal static void NotNullSubtype<T>(Type type, string parameterName) {
			Requires.NotNull(type, parameterName);
			Requires.That(typeof(T).IsAssignableFrom(type), parameterName, MessagingStrings.UnexpectedType, typeof(T).FullName, type.FullName);
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="FormatException"/>.</param>
		/// <param name="message">The message.</param>
		[Pure, DebuggerStepThrough]
		internal static void Format(bool condition, string message) {
			if (!condition) {
				throw new FormatException(message);
			}
		}

		/// <summary>
		/// Throws an <see cref="NotSupportedException"/> if a condition does not evaluate to <c>true</c>.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="NotSupportedException"/>.</param>
		/// <param name="message">The message.</param>
		[Pure, DebuggerStepThrough]
		internal static void Support(bool condition, string message) {
			if (!condition) {
				throw new NotSupportedException(message);
			}
		}
	}
}
