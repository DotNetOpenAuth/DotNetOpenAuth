//-----------------------------------------------------------------------
// <copyright file="OpenIdExtensionsInteropHelper.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// A set of methods designed to assist in improving interop across different
	/// OpenID implementations and their extensions.
	/// </summary>
	internal static class OpenIdExtensionsInteropHelper {
		/// <summary>
		/// The gender decoder to translate AX genders to Sreg.
		/// </summary>
		private static GenderEncoder genderEncoder = new GenderEncoder();

		/// <summary>
		/// Gets the gender decoder to translate AX genders to Sreg.
		/// </summary>
		internal static GenderEncoder GenderEncoder {
			get { return genderEncoder; }
		}

		/// <summary>
		/// Splits the AX attribute format flags into individual values for processing.
		/// </summary>
		/// <param name="formats">The formats to split up into individual flags.</param>
		/// <returns>A sequence of individual flags.</returns>
		internal static IEnumerable<AXAttributeFormats> ForEachFormat(AXAttributeFormats formats) {
			if ((formats & AXAttributeFormats.AXSchemaOrg) != 0) {
				yield return AXAttributeFormats.AXSchemaOrg;
			}

			if ((formats & AXAttributeFormats.OpenIdNetSchema) != 0) {
				yield return AXAttributeFormats.OpenIdNetSchema;
			}

			if ((formats & AXAttributeFormats.SchemaOpenIdNet) != 0) {
				yield return AXAttributeFormats.SchemaOpenIdNet;
			}
		}

		/// <summary>
		/// Transforms an AX attribute type URI from the axschema.org format into a given format.
		/// </summary>
		/// <param name="axSchemaOrgFormatTypeUri">The ax schema org format type URI.</param>
		/// <param name="targetFormat">The target format.  Only one flag should be set.</param>
		/// <returns>The AX attribute type URI in the target format.</returns>
		internal static string TransformAXFormat(string axSchemaOrgFormatTypeUri, AXAttributeFormats targetFormat) {
			Requires.NotNullOrEmpty(axSchemaOrgFormatTypeUri, "axSchemaOrgFormatTypeUri");

			switch (targetFormat) {
				case AXAttributeFormats.AXSchemaOrg:
					return axSchemaOrgFormatTypeUri;
				case AXAttributeFormats.SchemaOpenIdNet:
					return axSchemaOrgFormatTypeUri.Replace("axschema.org", "schema.openid.net");
				case AXAttributeFormats.OpenIdNetSchema:
					return axSchemaOrgFormatTypeUri.Replace("axschema.org", "openid.net/schema");
				default:
					throw new ArgumentOutOfRangeException("targetFormat");
			}
		}

		/// <summary>
		/// Detects the AX attribute type URI format from a given sample.
		/// </summary>
		/// <param name="typeURIs">The type URIs to scan for recognized formats.</param>
		/// <returns>The first AX type URI format recognized in the list.</returns>
		internal static AXAttributeFormats DetectAXFormat(IEnumerable<string> typeURIs) {
			Requires.NotNull(typeURIs, "typeURIs");

			if (typeURIs.Any(uri => uri.StartsWith("http://axschema.org/", StringComparison.Ordinal))) {
				return AXAttributeFormats.AXSchemaOrg;
			}

			if (typeURIs.Any(uri => uri.StartsWith("http://schema.openid.net/", StringComparison.Ordinal))) {
				return AXAttributeFormats.SchemaOpenIdNet;
			}

			if (typeURIs.Any(uri => uri.StartsWith("http://openid.net/schema/", StringComparison.Ordinal))) {
				return AXAttributeFormats.OpenIdNetSchema;
			}

			return AXAttributeFormats.None;
		}

		/// <summary>
		/// Adds an attribute fetch request if it is not already present in the AX request.
		/// </summary>
		/// <param name="ax">The AX request to add the attribute request to.</param>
		/// <param name="format">The format of the attribute's Type URI to use.</param>
		/// <param name="axSchemaOrgFormatAttribute">The attribute in axschema.org format.</param>
		/// <param name="demandLevel">The demand level.</param>
		internal static void FetchAttribute(FetchRequest ax, AXAttributeFormats format, string axSchemaOrgFormatAttribute, DemandLevel demandLevel) {
			Requires.NotNull(ax, "ax");
			Requires.NotNullOrEmpty(axSchemaOrgFormatAttribute, "axSchemaOrgFormatAttribute");

			string typeUri = TransformAXFormat(axSchemaOrgFormatAttribute, format);
			if (!ax.Attributes.Contains(typeUri)) {
				switch (demandLevel) {
					case DemandLevel.Request:
						ax.Attributes.AddOptional(typeUri);
						break;
					case DemandLevel.Require:
						ax.Attributes.AddRequired(typeUri);
						break;
					default:
						break;
				}
			}
		}
	}
}
