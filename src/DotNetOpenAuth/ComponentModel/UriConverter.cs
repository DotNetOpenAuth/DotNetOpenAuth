//-----------------------------------------------------------------------
// <copyright file="UriConverter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.ComponentModel;
	using System.ComponentModel.Design.Serialization;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using System.Collections;

	/// <summary>
	/// A design-time helper to allow controls to have properties
	/// of type <see cref="Uri"/>.
	/// </summary>
	public class UriConverter<WellKnownValues> : ConverterBase<Uri> {
		/// <summary>
		/// Initializes a new instance of the UriConverter&lt;WellKnownValues&gt; class.
		/// </summary>
		[Obsolete("This class is meant for design-time use within an IDE, and not meant to be used directly by runtime code.")]
		public UriConverter() {
		}

		/// <summary>
		/// Returns whether the given value object is valid for this type and for the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to test for validity.</param>
		/// <returns>
		/// true if the specified value is valid for this object; otherwise, false.
		/// </returns>
		public override bool IsValid(ITypeDescriptorContext context, object value) {
			if (value is Uri) {
				return ((Uri)value).IsAbsoluteUri;
			} else if (value is string) {
				Uri result;
				return Uri.TryCreate((string)value, UriKind.Absolute, out result);
			} else {
				return false;
			}
		}

		/// <summary>
		/// Converts a value from its string representation to its strongly-typed object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The strongly-typed object.</returns>
		[Pure]
		protected override Uri ConvertFrom(string value) {
			return new Uri(value);
		}

		/// <summary>
		/// Creates the reflection instructions for recreating an instance later.
		/// </summary>
		/// <param name="value">The value to recreate later.</param>
		/// <returns>
		/// The description of how to recreate an instance.
		/// </returns>
		[Pure]
		protected override InstanceDescriptor CreateFrom(Uri value) {
			MemberInfo uriCtor = typeof(Uri).GetConstructor(new Type[] { typeof(string) });
			return new InstanceDescriptor(uriCtor, new object[] { value.AbsoluteUri });
		}

		/// <summary>
		/// Converts the strongly-typed value to a string.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The string representation of the object.</returns>
		[Pure]
		protected override string ConvertToString(Uri value) {
			return value.AbsoluteUri;
		}

		/// <summary>
		/// Gets the standard claim type URIs known to the library.
		/// </summary>
		/// <returns>An array of the standard claim types.</returns>
		[Pure]
		protected override ICollection GetStandardValuesForCache() {
			return (from field in typeof(WellKnownValues).GetFields(BindingFlags.Static | BindingFlags.Public)
					select new Uri((string)field.GetValue(null))).ToArray();
		}
	}
}
