//-----------------------------------------------------------------------
// <copyright file="SuggestedStringsConverter.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.Collections;
	using System.ComponentModel.Design.Serialization;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using Validation;

	/// <summary>
	/// A type that generates suggested strings for Intellisense,
	/// but doesn't actually convert between strings and other types.
	/// </summary>
	public abstract class SuggestedStringsConverter : ConverterBase<string> {
		/// <summary>
		/// Initializes a new instance of the <see cref="SuggestedStringsConverter"/> class.
		/// </summary>
		protected SuggestedStringsConverter() {
		}

		/// <summary>
		/// Gets the type to reflect over for the well known values.
		/// </summary>
		[Pure]
		protected abstract Type WellKnownValuesType { get; }

		/// <summary>
		/// Gets the values of public static fields and properties on a given type.
		/// </summary>
		/// <param name="type">The type to reflect over.</param>
		/// <returns>A collection of values.</returns>
		internal static ICollection GetStandardValuesForCacheShared(Type type) {
			Requires.NotNull(type, "type");

			var fields = from field in type.GetFields(BindingFlags.Static | BindingFlags.Public)
						 select field.GetValue(null);
			var properties = from prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public)
							 select prop.GetValue(null, null);
			return fields.Concat(properties).ToArray();
		}

		/// <summary>
		/// Converts a value from its string representation to its strongly-typed object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The strongly-typed object.</returns>
		[Pure]
		protected override string ConvertFrom(string value) {
			return value;
		}

		/// <summary>
		/// Creates the reflection instructions for recreating an instance later.
		/// </summary>
		/// <param name="value">The value to recreate later.</param>
		/// <returns>
		/// The description of how to recreate an instance.
		/// </returns>
		[Pure]
		protected override InstanceDescriptor CreateFrom(string value) {
			// No implementation necessary since we're only dealing with strings.
			throw new NotImplementedException();
		}

		/// <summary>
		/// Converts the strongly-typed value to a string.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The string representation of the object.</returns>
		[Pure]
		protected override string ConvertToString(string value) {
			return value;
		}

		/// <summary>
		/// Gets the standard values to suggest with Intellisense in the designer.
		/// </summary>
		/// <returns>A collection of the standard values.</returns>
		[Pure]
		protected override ICollection GetStandardValuesForCache() {
			return GetStandardValuesForCacheShared(this.WellKnownValuesType);
		}
	}
}
