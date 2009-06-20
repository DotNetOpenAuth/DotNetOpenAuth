//-----------------------------------------------------------------------
// <copyright file="InteropHelper.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	public static class InteropHelper {
		/// <summary>
		/// The various Type URI formats an AX attribute may use by various remote parties.
		/// </summary>
		[Flags]
		public enum AXAttributeFormats {
			/// <summary>
			/// AX attributes should use the Type URI format starting with <c>http://axschema.org/</c>.
			/// </summary>
			AXSchemaOrg,

			/// <summary>
			/// AX attributes should use the Type URI format starting with <c>http://schema.openid.net/</c>.
			/// </summary>
			SchemaOpenIdNet,

			/// <summary>
			/// AX attributes should use the Type URI format starting with <c>http://openid.net/schema/</c>.
			/// </summary>
			OpenIdNetSchema,
		}

		/// <summary>
		/// Adds an Attribute Exchange (AX) extension to the authentication request
		/// that asks for the same attributes as the Simple Registration (sreg) extension
		/// that is already applied.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <param name="attributeFormat">The attribute formats to include in the request.</param>
		/// <remarks>
		/// 	<para>If discovery on the user-supplied identifier yields hints regarding which
		/// extensions and attribute formats the Provider supports, this method MAY ignore the
		/// <paramref name="attributeFormat"/> argument and accomodate the Provider to minimize
		/// the size of the request.</para>
		/// 	<para>If the request does not carry an sreg extension, the method logs a warning but
		/// otherwise quietly returns doing nothing.</para>
		/// </remarks>
		public static void SpreadSregToAX(this RelyingParty.IAuthenticationRequest request, AXAttributeFormats attributeFormats) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			var req = (RelyingParty.AuthenticationRequest)request;
			var sreg = req.AppliedExtensions.OfType<ClaimsRequest>().SingleOrDefault();
			if (sreg == null) {
				Logger.OpenId.Warn("No Simple Registration (ClaimsRequest) extension present in the request to spread to AX.");
				return;
			}

			var ax = req.AppliedExtensions.OfType<FetchRequest>().SingleOrDefault();
			if (ax == null) {
				ax = new FetchRequest();
				req.AddExtension(ax);
			}

			if (req.Provider.IsExtensionSupported<ClaimsRequest>()) {
				Logger.OpenId.Info("Skipping generation of AX request because the Identifier advertises the Provider supports the Sreg extension.");
				return;
			}

			// Try to use just one AX Type URI format if we can figure out which type the OP accepts.
			attributeFormats = FocusAttributeFormat(request, attributeFormats);

			foreach (AXAttributeFormats format in ForEachFormat(attributeFormats)) {
				FetchAttribute(ax, format, WellKnownAttributes.BirthDate.WholeBirthDate, sreg.BirthDate);
				FetchAttribute(ax, format, WellKnownAttributes.Contact.HomeAddress.Country, sreg.Country);
				FetchAttribute(ax, format, WellKnownAttributes.Contact.Email, sreg.Email);
				FetchAttribute(ax, format, WellKnownAttributes.Name.FullName, sreg.FullName);
				FetchAttribute(ax, format, WellKnownAttributes.Person.Gender, sreg.Gender);
				FetchAttribute(ax, format, WellKnownAttributes.Preferences.Language, sreg.Language);
				FetchAttribute(ax, format, WellKnownAttributes.Name.Alias, sreg.Nickname);
				FetchAttribute(ax, format, WellKnownAttributes.Contact.HomeAddress.PostalCode, sreg.PostalCode);
				FetchAttribute(ax, format, WellKnownAttributes.Preferences.TimeZone, sreg.TimeZone);
			}
		}

		public static void UnifyExtensions(this RelyingParty.IAuthenticationResponse response) {
			Contract.Requires(response != null);
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			var resp = (RelyingParty.IAuthenticationResponse)response;
			throw new NotImplementedException();
		}

		public static void UnifyExtensions(this Provider.IAuthenticationRequest request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			var req = (Provider.AuthenticationRequest)request;
			throw new NotImplementedException();
		}

		/// <summary>
		/// Tries to find the exact format of AX attribute Type URI supported by the Provider.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <param name="attributeFormat">The attribute formats the RP will try if this discovery fails.</param>
		/// <returns>The AX format(s) to use based on the Provider's advertised AX support.</returns>
		private static AXAttributeFormats FocusAttributeFormat(RelyingParty.IAuthenticationRequest request, AXAttributeFormats attributeFormat) {
			Contract.Requires(request != null);

			var provider = (RelyingParty.ServiceEndpoint)request.Provider;

			if (provider.ProviderDescription.Capabilities.Any(uri => uri.StartsWith("http://axschema.org/", StringComparison.Ordinal))) {
				return AXAttributeFormats.AXSchemaOrg;
			}

			if (provider.ProviderDescription.Capabilities.Any(uri => uri.StartsWith("http://schema.openid.net/", StringComparison.Ordinal))) {
				return AXAttributeFormats.SchemaOpenIdNet;
			}

			if (provider.ProviderDescription.Capabilities.Any(uri => uri.StartsWith("http://openid.net/schema/", StringComparison.Ordinal))) {
				return AXAttributeFormats.OpenIdNetSchema;
			}

			return attributeFormat;
		}

		/// <summary>
		/// Transforms an AX attribute type URI from the axschema.org format into a given format.
		/// </summary>
		/// <param name="axSchemaOrgFormatTypeUri">The ax schema org format type URI.</param>
		/// <param name="targetFormat">The target format.  Only one flag should be set.</param>
		/// <returns>The AX attribute type URI in the target format.</returns>
		private static string TransformAXFormat(string axSchemaOrgFormatTypeUri, AXAttributeFormats targetFormat) {
			Contract.Requires(!String.IsNullOrEmpty(axSchemaOrgFormatTypeUri));

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
		/// Splits the AX attribute format flags into individual values for processing.
		/// </summary>
		/// <param name="formats">The formats to split up into individual flags.</param>
		/// <returns>A sequence of individual flags.</returns>
		private static IEnumerable<AXAttributeFormats> ForEachFormat(AXAttributeFormats formats) {
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
		/// Adds an attribute fetch request if it is not already present in the AX request.
		/// </summary>
		/// <param name="ax">The ax.</param>
		/// <param name="format">The format.</param>
		/// <param name="axSchemaOrgFormatAttribute">The ax schema org format attribute.</param>
		/// <param name="demandLevel">The demand level.</param>
		private static void FetchAttribute(FetchRequest ax, AXAttributeFormats format, string axSchemaOrgFormatAttribute, DemandLevel demandLevel) {
			Contract.Requires(ax != null);
			Contract.Requires(!String.IsNullOrEmpty(axSchemaOrgFormatAttribute));

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
