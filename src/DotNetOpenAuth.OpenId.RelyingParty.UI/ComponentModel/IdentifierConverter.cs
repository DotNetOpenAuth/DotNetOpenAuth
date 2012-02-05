//-----------------------------------------------------------------------
// <copyright file="IdentifierConverter.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.Collections;
	using System.ComponentModel.Design.Serialization;
	using System.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// A design-time helper to give an OpenID Identifier property an auto-complete functionality
	/// listing the OP Identifiers in the <see cref="WellKnownProviders"/> class.
	/// </summary>
	public class IdentifierConverter : ConverterBase<Identifier> {
		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierConverter"/> class.
		/// </summary>
		[Obsolete("This class is meant for design-time use within an IDE, and not meant to be used directly by runtime code.")]
		public IdentifierConverter() {
		}

		/// <summary>
		/// Converts a value from its string representation to its strongly-typed object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The strongly-typed object.</returns>
		protected override Identifier ConvertFrom(string value) {
			return value;
		}

		/// <summary>
		/// Creates the reflection instructions for recreating an instance later.
		/// </summary>
		/// <param name="value">The value to recreate later.</param>
		/// <returns>
		/// The description of how to recreate an instance.
		/// </returns>
		protected override InstanceDescriptor CreateFrom(Identifier value) {
			if (value == null) {
				return null;
			}

			MemberInfo identifierParse = typeof(Identifier).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
			return CreateInstanceDescriptor(identifierParse, new object[] { value.ToString() });
		}

		/// <summary>
		/// Converts the strongly-typed value to a string.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The string representation of the object.</returns>
		protected override string ConvertToString(Identifier value) {
			return value;
		}

		/// <summary>
		/// Gets the standard values to suggest with Intellisense in the designer.
		/// </summary>
		/// <returns>A collection of the standard values.</returns>
		protected override ICollection GetStandardValuesForCache() {
			return SuggestedStringsConverter.GetStandardValuesForCacheShared(typeof(WellKnownProviders));
		}
	}
}
