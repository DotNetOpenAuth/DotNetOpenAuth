//-----------------------------------------------------------------------
// <copyright file="ExtensionsInteropHelper.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// A set of methods designed to assist in improving interop across different
	/// OpenID implementations and their extensions.
	/// </summary>
	public static class ExtensionsInteropHelper {
		/// <summary>
		/// The gender decoder to translate AX genders to Sreg.
		/// </summary>
		private static GenderEncoder genderEncoder = new GenderEncoder();

		/// <summary>
		/// Adds an Attribute Exchange (AX) extension to the authentication request
		/// that asks for the same attributes as the Simple Registration (sreg) extension
		/// that is already applied.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <param name="attributeFormats">The attribute formats to use in the AX request.</param>
		/// <remarks>
		/// 	<para>If discovery on the user-supplied identifier yields hints regarding which
		/// extensions and attribute formats the Provider supports, this method MAY ignore the
		/// <paramref name="attributeFormat"/> argument and accomodate the Provider to minimize
		/// the size of the request.</para>
		/// 	<para>If the request does not carry an sreg extension, the method logs a warning but
		/// otherwise quietly returns doing nothing.</para>
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Abbreviation")]
		public static void SpreadSregToAX(this RelyingParty.IAuthenticationRequest request, AXAttributeFormats attributeFormats) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			var req = (RelyingParty.AuthenticationRequest)request;
			var sreg = req.AppliedExtensions.OfType<ClaimsRequest>().SingleOrDefault();
			if (sreg == null) {
				Logger.OpenId.Warn("No Simple Registration (ClaimsRequest) extension present in the request to spread to AX.");
				return;
			}

			if (req.Provider.IsExtensionSupported<ClaimsRequest>()) {
				Logger.OpenId.Info("Skipping generation of AX request because the Identifier advertises the Provider supports the Sreg extension.");
				return;
			}

			var ax = req.AppliedExtensions.OfType<FetchRequest>().SingleOrDefault();
			if (ax == null) {
				ax = new FetchRequest();
				req.AddExtension(ax);
			}

			// Try to use just one AX Type URI format if we can figure out which type the OP accepts.
			AXAttributeFormats detectedFormat;
			if (TryDetectOPAttributeFormat(request, out detectedFormat)) {
				Logger.OpenId.Info("Detected OP support for AX but not for Sreg.  Removing Sreg extension request and using AX instead.");
				attributeFormats = detectedFormat;
				req.Extensions.Remove(sreg);
			} else {
				Logger.OpenId.Info("Could not determine whether OP supported Sreg or AX.  Using both extensions.");
			}

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

		/// <summary>
		/// Looks for Simple Registration and Attribute Exchange (all known formats)
		/// response extensions and returns them as a Simple Registration extension.
		/// </summary>
		/// <param name="response">The authentication response.</param>
		/// <param name="allowUnsigned">if set to <c>true</c> unsigned extensions will be included in the search.</param>
		/// <returns>
		/// The Simple Registration response if found, 
		/// or a fabricated one based on the Attribute Exchange extension if found,
		/// or just an empty <see cref="ClaimsResponse"/> if there was no data.
		/// Never <c>null</c>.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Abbreviation")]
		public static ClaimsResponse UnifyExtensionsAsSreg(this RelyingParty.IAuthenticationResponse response, bool allowUnsigned) {
			Contract.Requires(response != null);
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			var resp = (RelyingParty.IAuthenticationResponse)response;
			var sreg = allowUnsigned ? resp.GetUntrustedExtension<ClaimsResponse>() : resp.GetExtension<ClaimsResponse>();
			if (sreg != null) {
				return sreg;
			}

			AXAttributeFormats formats = AXAttributeFormats.All;
			sreg = new ClaimsResponse();
			var fetchResponse = allowUnsigned ? resp.GetUntrustedExtension<FetchResponse>() : resp.GetExtension<FetchResponse>();
			if (fetchResponse != null) {
				((IOpenIdMessageExtension)sreg).IsSignedByRemoteParty = fetchResponse.IsSignedByProvider;
				sreg.BirthDateRaw = fetchResponse.GetAttributeValue(WellKnownAttributes.BirthDate.WholeBirthDate, formats);
				sreg.Country = fetchResponse.GetAttributeValue(WellKnownAttributes.Contact.HomeAddress.Country, formats);
				sreg.PostalCode = fetchResponse.GetAttributeValue(WellKnownAttributes.Contact.HomeAddress.PostalCode, formats);
				sreg.Email = fetchResponse.GetAttributeValue(WellKnownAttributes.Contact.Email, formats);
				sreg.FullName = fetchResponse.GetAttributeValue(WellKnownAttributes.Name.FullName, formats);
				sreg.Language = fetchResponse.GetAttributeValue(WellKnownAttributes.Preferences.Language, formats);
				sreg.Nickname = fetchResponse.GetAttributeValue(WellKnownAttributes.Name.Alias, formats);
				sreg.TimeZone = fetchResponse.GetAttributeValue(WellKnownAttributes.Preferences.TimeZone, formats);
				string gender = fetchResponse.GetAttributeValue(WellKnownAttributes.Person.Gender, formats);
				if (gender != null) {
					sreg.Gender = (Gender)genderEncoder.Decode(gender);
				}
			}

			return sreg;
		}

		/// <summary>
		/// Looks for Simple Registration and Attribute Exchange (all known formats)
		/// request extensions and returns them as a Simple Registration extension,
		/// and adds the new extension to the original request message if it was absent.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <returns>
		/// The Simple Registration request if found, 
		/// or a fabricated one based on the Attribute Exchange extension if found,
		/// or <c>null</c> if no attribute extension request is found.</returns>
		internal static ClaimsRequest UnifyExtensionsAsSreg(this Provider.IHostProcessedRequest request) {
			Contract.Requires(request != null);
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			var req = (Provider.HostProcessedRequest)request;
			var sreg = req.GetExtension<ClaimsRequest>();
			if (sreg != null) {
				return sreg;
			}

			var ax = req.GetExtension<FetchRequest>();
			if (ax != null) {
				sreg = new ClaimsRequest(SimpleRegistration.Constants.sreg_ns);
				sreg.Synthesized = true;
				((IProtocolMessageWithExtensions)req.RequestMessage).Extensions.Add(sreg);
				sreg.BirthDate = GetDemandLevelFor(ax, WellKnownAttributes.BirthDate.WholeBirthDate);
				sreg.Country = GetDemandLevelFor(ax, WellKnownAttributes.Contact.HomeAddress.Country);
				sreg.Email = GetDemandLevelFor(ax, WellKnownAttributes.Contact.Email);
				sreg.FullName = GetDemandLevelFor(ax, WellKnownAttributes.Name.FullName);
				sreg.Gender = GetDemandLevelFor(ax, WellKnownAttributes.Person.Gender);
				sreg.Language = GetDemandLevelFor(ax, WellKnownAttributes.Preferences.Language);
				sreg.Nickname = GetDemandLevelFor(ax, WellKnownAttributes.Name.Alias);
				sreg.PostalCode = GetDemandLevelFor(ax, WellKnownAttributes.Contact.HomeAddress.PostalCode);
				sreg.TimeZone = GetDemandLevelFor(ax, WellKnownAttributes.Preferences.TimeZone);
			}

			return sreg;
		}

		/// <summary>
		/// Converts the Simple Registration extension response to whatever format the original
		/// attribute request extension came in.
		/// </summary>
		/// <param name="request">The authentication request with the response extensions already added.</param>
		/// <remarks>
		/// If the original attribute request came in as AX, the Simple Registration extension is converted
		/// to an AX response and then the Simple Registration extension is removed from the response.
		/// </remarks>
		internal static void ConvertSregToMatchRequest(this Provider.IHostProcessedRequest request) {
			var req = (Provider.HostProcessedRequest)request;
			var response = req.Response as IProtocolMessageWithExtensions; // negative responses don't support extensions.
			var sregRequest = request.GetExtension<ClaimsRequest>();
			if (sregRequest != null && response != null) {
				if (sregRequest.Synthesized) {
					var axRequest = request.GetExtension<FetchRequest>();
					ErrorUtilities.VerifyInternal(axRequest != null, "How do we have a synthesized Sreg request without an AX request?");

					var sregResponse = response.Extensions.OfType<ClaimsResponse>().SingleOrDefault();
					if (sregResponse == null) {
						// No Sreg response to copy from.
						return;
					}

					// Remove the sreg response since the RP didn't ask for it.
					response.Extensions.Remove(sregResponse);

					AXAttributeFormats format = DetectAXFormat(axRequest.Attributes.Select(att => att.TypeUri));
					if (format == AXAttributeFormats.None) {
						// No recognized AX attributes were requested.
						return;
					}

					var axResponse = response.Extensions.OfType<FetchResponse>().SingleOrDefault();
					if (axResponse == null) {
						axResponse = new FetchResponse();
						response.Extensions.Add(axResponse);
					}

					AddAXAttributeValue(axResponse, WellKnownAttributes.BirthDate.WholeBirthDate, format, sregResponse.BirthDateRaw);
					AddAXAttributeValue(axResponse, WellKnownAttributes.Contact.HomeAddress.Country, format, sregResponse.Country);
					AddAXAttributeValue(axResponse, WellKnownAttributes.Contact.HomeAddress.PostalCode, format, sregResponse.PostalCode);
					AddAXAttributeValue(axResponse, WellKnownAttributes.Contact.Email, format, sregResponse.Email);
					AddAXAttributeValue(axResponse, WellKnownAttributes.Name.FullName, format, sregResponse.FullName);
					AddAXAttributeValue(axResponse, WellKnownAttributes.Name.Alias, format, sregResponse.Nickname);
					AddAXAttributeValue(axResponse, WellKnownAttributes.Preferences.TimeZone, format, sregResponse.TimeZone);
					AddAXAttributeValue(axResponse, WellKnownAttributes.Preferences.Language, format, sregResponse.Language);
					if (sregResponse.Gender.HasValue) {
						AddAXAttributeValue(axResponse, WellKnownAttributes.Person.Gender, format, genderEncoder.Encode(sregResponse.Gender));
					}
				}
			}
		}

		/// <summary>
		/// Gets the attribute value if available.
		/// </summary>
		/// <param name="fetchResponse">The AX fetch response extension to look for the attribute value.</param>
		/// <param name="typeUri">The type URI of the attribute, using the axschema.org format of <see cref="WellKnownAttributes"/>.</param>
		/// <param name="formats">The AX type URI formats to search.</param>
		/// <returns>
		/// The first value of the attribute, if available.
		/// </returns>
		internal static string GetAttributeValue(this FetchResponse fetchResponse, string typeUri, AXAttributeFormats formats) {
			return ForEachFormat(formats).Select(format => fetchResponse.GetAttributeValue(TransformAXFormat(typeUri, format))).FirstOrDefault(s => s != null);
		}

		/// <summary>
		/// Adds the AX attribute value to the response if it is non-empty.
		/// </summary>
		/// <param name="ax">The AX Fetch response to add the attribute value to.</param>
		/// <param name="typeUri">The attribute type URI in axschema.org format.</param>
		/// <param name="format">The target format of the actual attribute to write out.</param>
		/// <param name="value">The value of the attribute.</param>
		private static void AddAXAttributeValue(FetchResponse ax, string typeUri, AXAttributeFormats format, string value) {
			if (!string.IsNullOrEmpty(value)) {
				string targetTypeUri = TransformAXFormat(typeUri, format);
				if (!ax.Attributes.Contains(targetTypeUri)) {
					ax.Attributes.Add(targetTypeUri, value);
				}
			}
		}

		/// <summary>
		/// Gets the demand level for an AX attribute.
		/// </summary>
		/// <param name="ax">The AX fetch request to search for the attribute.</param>
		/// <param name="typeUri">The type URI of the attribute in axschema.org format.</param>
		/// <returns>The demand level for the attribute.</returns>
		private static DemandLevel GetDemandLevelFor(FetchRequest ax, string typeUri) {
			Contract.Requires(ax != null);
			Contract.Requires(!String.IsNullOrEmpty(typeUri));

			foreach (AXAttributeFormats format in ForEachFormat(AXAttributeFormats.All)) {
				string typeUriInFormat = TransformAXFormat(typeUri, format);
				if (ax.Attributes.Contains(typeUriInFormat)) {
					return ax.Attributes[typeUriInFormat].IsRequired ? DemandLevel.Require : DemandLevel.Request;
				}
			}

			return DemandLevel.NoRequest;
		}

		/// <summary>
		/// Tries to find the exact format of AX attribute Type URI supported by the Provider.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <param name="attributeFormat">The attribute formats the RP will try if this discovery fails.</param>
		/// <returns>The AX format(s) to use based on the Provider's advertised AX support.</returns>
		private static bool TryDetectOPAttributeFormat(RelyingParty.IAuthenticationRequest request, out AXAttributeFormats attributeFormat) {
			Contract.Requires(request != null);
			var provider = (RelyingParty.ServiceEndpoint)request.Provider;
			attributeFormat = DetectAXFormat(provider.ProviderDescription.Capabilities);
			return attributeFormat != AXAttributeFormats.None;
		}

		/// <summary>
		/// Detects the AX attribute type URI format from a given sample.
		/// </summary>
		/// <param name="typeURIs">The type URIs to scan for recognized formats.</param>
		/// <returns>The first AX type URI format recognized in the list.</returns>
		private static AXAttributeFormats DetectAXFormat(IEnumerable<string> typeURIs) {
			Contract.Requires(typeURIs != null);

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
		/// <param name="ax">The AX request to add the attribute request to.</param>
		/// <param name="format">The format of the attribute's Type URI to use.</param>
		/// <param name="axSchemaOrgFormatAttribute">The attribute in axschema.org format.</param>
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
