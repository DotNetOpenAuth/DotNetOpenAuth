//-----------------------------------------------------------------------
// <copyright file="ConverterBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.ComponentModel.Design.Serialization;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Reflection;
	using System.Security;
	using System.Security.Permissions;

	/// <summary>
	/// A design-time helper to allow Intellisense to aid typing
	/// ClaimType URIs.
	/// </summary>
	/// <typeparam name="T">The strong-type of the property this class is affixed to.</typeparam>
	public abstract class ConverterBase<T> : TypeConverter {
		/// <summary>
		/// A cache of the standard claim types known to the application.
		/// </summary>
		private StandardValuesCollection standardValues;

		/// <summary>
		/// Initializes a new instance of the ConverterBase class.
		/// </summary>
		protected ConverterBase() {
		}

		/// <summary>
		/// Gets a cache of the standard values to suggest.
		/// </summary>
		private StandardValuesCollection StandardValueCache {
			get {
				if (this.standardValues == null) {
					this.standardValues = new StandardValuesCollection(this.GetStandardValuesForCache());
				}

				return this.standardValues;
			}
		}

		/// <summary>
		/// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <returns>
		/// true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> should be called to find a common set of values the object supports; otherwise, false.
		/// </returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
			return this.StandardValueCache.Count > 0;
		}

		/// <summary>
		/// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
		/// <returns>
		/// A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"/> that holds a standard set of valid values, or null if the data type does not support a standard set of values.
		/// </returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
			return this.StandardValueCache;
		}

		/// <summary>
		/// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> is an exclusive list of possible values, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <returns>
		/// true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"/> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"/> is an exhaustive list of possible values; false if other values are possible.
		/// </returns>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
			return false;
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			return sourceType == typeof(string)
				|| base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="T:System.Type"/> that represents the type you want to convert to.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			return destinationType == typeof(string)
				|| destinationType == typeof(InstanceDescriptor)
				|| base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			string stringValue = value as string;
			if (stringValue != null) {
				return this.ConvertFrom(stringValue);
			} else {
				return base.ConvertFrom(context, culture, value);
			}
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="destinationType"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Contracts.__ContractsRuntime.Assume(System.Boolean,System.String,System.String)", Justification = "No localization required.")]
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			Assumes.True(destinationType != null, "Missing contract.");
			if (destinationType.IsInstanceOfType(value)) {
				return value;
			}

			T typedValue = (T)value;
			if (destinationType == typeof(string)) {
				return this.ConvertToString(typedValue);
			} else if (destinationType == typeof(InstanceDescriptor)) {
				return this.CreateFrom(typedValue);
			} else {
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}

		/// <summary>
		/// Creates an <see cref="InstanceDescriptor"/> instance, protecting against the LinkDemand.
		/// </summary>
		/// <param name="memberInfo">The member info.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns>A <see cref="InstanceDescriptor"/>, or <c>null</c> if sufficient permissions are unavailable.</returns>
		protected static InstanceDescriptor CreateInstanceDescriptor(MemberInfo memberInfo, ICollection arguments) {
			try {
				return CreateInstanceDescriptorPrivate(memberInfo, arguments);
			} catch (SecurityException) {
				return null;
			}
		}

		/// <summary>
		/// Gets the standard values to suggest with Intellisense in the designer.
		/// </summary>
		/// <returns>A collection of the standard values.</returns>
		[Pure]
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Potentially expensive call.")]
		protected virtual ICollection GetStandardValuesForCache() {
			return new T[0];
		}

		/// <summary>
		/// Converts a value from its string representation to its strongly-typed object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The strongly-typed object.</returns>
		[Pure]
		protected abstract T ConvertFrom(string value);

		/// <summary>
		/// Creates the reflection instructions for recreating an instance later.
		/// </summary>
		/// <param name="value">The value to recreate later.</param>
		/// <returns>The description of how to recreate an instance.</returns>
		[Pure]
		protected abstract InstanceDescriptor CreateFrom(T value);

		/// <summary>
		/// Converts the strongly-typed value to a string.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The string representation of the object.</returns>
		[Pure]
		protected abstract string ConvertToString(T value);

		/// <summary>
		/// Creates an <see cref="InstanceDescriptor"/> instance, protecting against the LinkDemand.
		/// </summary>
		/// <param name="memberInfo">The member info.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns>A <see cref="InstanceDescriptor"/>.</returns>
		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		private static InstanceDescriptor CreateInstanceDescriptorPrivate(MemberInfo memberInfo, ICollection arguments) {
			return new InstanceDescriptor(memberInfo, arguments);
		}
	}
}
