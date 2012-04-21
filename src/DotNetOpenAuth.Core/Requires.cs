//-----------------------------------------------------------------------
// <copyright file="Requires.cs" company="Outercurve Foundation">
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

	/// <summary>
	/// Argument validation checks that throw some kind of ArgumentException when they fail (unless otherwise noted).
	/// </summary>
	internal static class Requires {
		/// <summary>
		/// Validates that a given parameter is not null.
		/// </summary>
		/// <typeparam name="T">The type of the parameter</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>The tested value, guaranteed to not be null.</returns>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static T NotNull<T>(T value, string parameterName) where T : class {
			if (value == null) {
				throw new ArgumentNullException(parameterName);
			}

			Contract.EndContractBlock();
			return value;
		}

		/// <summary>
		/// Validates that a parameter is not null or empty.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>The validated value.</returns>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static string NotNullOrEmpty(string value, string parameterName) {
			NotNull(value, parameterName);
			True(value.Length > 0, parameterName, Strings.EmptyStringNotAllowed);
			Contract.Ensures(Contract.Result<string>() == value);
			Contract.EndContractBlock();
			return value;
		}

		/// <summary>
		/// Validates that an array is not null or empty.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="parameterName">Name of the parameter.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void NotNullOrEmpty<T>(IEnumerable<T> value, string parameterName) {
			NotNull(value, parameterName);
			True(value.Any(), parameterName, Strings.InvalidArgument);
			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates that an argument is either null or is a sequence with no null elements.
		/// </summary>
		/// <typeparam name="T">The type of elements in the sequence.</typeparam>
		/// <param name="sequence">The sequence.</param>
		/// <param name="parameterName">Name of the parameter.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void NullOrWithNoNullElements<T>(IEnumerable<T> sequence, string parameterName) where T : class {
			if (sequence != null) {
				if (sequence.Any(e => e == null)) {
					throw new ArgumentException(MessagingStrings.SequenceContainsNullElement, parameterName);
				}
			}
		}

		/// <summary>
		/// Validates some expression describing the acceptable range for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="ArgumentOutOfRangeException"/>.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message to include with the exception.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void InRange(bool condition, string parameterName, string message = null) {
			if (!condition) {
				throw new ArgumentOutOfRangeException(parameterName, message);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="ArgumentException"/>.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message to include with the exception.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void True(bool condition, string parameterName = null, string message = null) {
			if (!condition) {
				throw new ArgumentException(message ?? Strings.InvalidArgument, parameterName);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="ArgumentException"/>.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="unformattedMessage">The unformatted message.</param>
		/// <param name="args">Formatting arguments.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void True(bool condition, string parameterName, string unformattedMessage, params object[] args) {
			if (!condition) {
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, unformattedMessage, args), parameterName);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="InvalidOperationException"/>.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void ValidState(bool condition) {
			if (!condition) {
				throw new InvalidOperationException();
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="InvalidOperationException"/>.</param>
		/// <param name="message">The message to include with the exception.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void ValidState(bool condition, string message) {
			if (!condition) {
				throw new InvalidOperationException(message);
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="InvalidOperationException"/>.</param>
		/// <param name="unformattedMessage">The unformatted message.</param>
		/// <param name="args">Formatting arguments.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void ValidState(bool condition, string unformattedMessage, params object[] args) {
			if (!condition) {
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, unformattedMessage, args));
			}

			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates that some argument describes a type that is or derives from a required type.
		/// </summary>
		/// <typeparam name="T">The type that the argument must be or derive from.</typeparam>
		/// <param name="type">The type given in the argument.</param>
		/// <param name="parameterName">Name of the parameter.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void NotNullSubtype<T>(Type type, string parameterName) {
			NotNull(type, parameterName);
			True(typeof(T).IsAssignableFrom(type), parameterName, MessagingStrings.UnexpectedType, typeof(T).FullName, type.FullName);

			Contract.EndContractBlock();
		}

		/// <summary>
		/// Validates some expression describing the acceptable condition for an argument evaluates to true.
		/// </summary>
		/// <param name="condition">The expression that must evaluate to true to avoid an <see cref="FormatException"/>.</param>
		/// <param name="message">The message.</param>
#if !CLR4
		[ContractArgumentValidator]
#endif
		[Pure, DebuggerStepThrough]
		internal static void Format(bool condition, string message) {
			if (!condition) {
				throw new FormatException(message);
			}

			Contract.EndContractBlock();
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

		/// <summary>
		/// Throws an <see cref="ArgumentException"/>
		/// </summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message.</param>
		[Pure, DebuggerStepThrough]
		internal static void Fail(string parameterName, string message) {
			throw new ArgumentException(message, parameterName);
		}
	}
}
