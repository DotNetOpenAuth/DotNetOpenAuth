//-----------------------------------------------------------------------
// <copyright file="UriConverter.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.ComponentModel.Design.Serialization;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// A design-time helper to allow controls to have properties
	/// of type <see cref="Uri"/>.
	/// </summary>
	public class UriConverter : ConverterBase<Uri> {
		/// <summary>
		/// Initializes a new instance of the UriConverter class.
		/// </summary>
		protected UriConverter() {
		}

		/// <summary>
		/// Gets the type to reflect over to extract the well known values.
		/// </summary>
		protected virtual Type WellKnownValuesType {
			get { return null; }
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
			Uri uriValue;
			string stringValue;
			if ((uriValue = value as Uri) != null) {
				return uriValue.IsAbsoluteUri;
			} else if ((stringValue = value as string) != null) {
				Uri result;
				return stringValue.Length == 0 || Uri.TryCreate(stringValue, UriKind.Absolute, out result);
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
			return string.IsNullOrEmpty(value) ? null : new Uri(value);
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
			if (value == null) {
				return null;
			}

			MemberInfo uriCtor = typeof(Uri).GetConstructor(new Type[] { typeof(string) });
			return CreateInstanceDescriptor(uriCtor, new object[] { value.AbsoluteUri });
		}

		/// <summary>
		/// Converts the strongly-typed value to a string.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The string representation of the object.</returns>
		[Pure]
		protected override string ConvertToString(Uri value) {
			if (value == null) {
				return null;
			}

			return value.AbsoluteUri;
		}

		/// <summary>
		/// Gets the standard claim type URIs known to the library.
		/// </summary>
		/// <returns>An array of the standard claim types.</returns>
		[Pure]
		protected override ICollection GetStandardValuesForCache() {
			if (this.WellKnownValuesType != null) {
				var fields = from field in this.WellKnownValuesType.GetFields(BindingFlags.Static | BindingFlags.Public)
							 let value = (string)field.GetValue(null)
							 where value != null
							 select new Uri(value);
				var properties = from prop in this.WellKnownValuesType.GetProperties(BindingFlags.Static | BindingFlags.Public)
								 let value = (string)prop.GetValue(null, null)
								 where value != null
								 select new Uri(value);
				return fields.Concat(properties).ToArray();
			} else {
				return new Uri[0];
			}
		}
	}
}
