//-----------------------------------------------------------------------
// <copyright file="ExtensionsInteropHelper.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// A set of methods designed to assist in improving interop across different
	/// OpenID implementations and their extensions.
	/// </summary>
	public static class ExtensionsInteropHelper {
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
		/// <paramref name="attributeFormats"/> argument and accomodate the Provider to minimize
		/// the size of the request.</para>
		/// 	<para>If the request does not carry an sreg extension, the method logs a warning but
		/// otherwise quietly returns doing nothing.</para>
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Abbreviation")]
		public static void SpreadSregToAX(this RelyingParty.IAuthenticationRequest request, AXAttributeFormats attributeFormats) {
			Requires.NotNull(request, "request");

			var req = (RelyingParty.AuthenticationRequest)request;
			var sreg = req.AppliedExtensions.OfType<ClaimsRequest>().SingleOrDefault();
			if (sreg == null) {
				Logger.OpenId.Debug("No Simple Registration (ClaimsRequest) extension present in the request to spread to AX.");
				return;
			}

			if (req.DiscoveryResult.IsExtensionSupported<ClaimsRequest>()) {
				Logger.OpenId.Debug("Skipping generation of AX request because the Identifier advertises the Provider supports the Sreg extension.");
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
				Logger.OpenId.Debug("Detected OP support for AX but not for Sreg.  Removing Sreg extension request and using AX instead.");
				attributeFormats = detectedFormat;
				req.Extensions.Remove(sreg);
			} else {
				Logger.OpenId.Debug("Could not determine whether OP supported Sreg or AX.  Using both extensions.");
			}

			foreach (AXAttributeFormats format in OpenIdExtensionsInteropHelper.ForEachFormat(attributeFormats)) {
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.BirthDate.WholeBirthDate, sreg.BirthDate);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Contact.HomeAddress.Country, sreg.Country);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Contact.Email, sreg.Email);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Name.FullName, sreg.FullName);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Person.Gender, sreg.Gender);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Preferences.Language, sreg.Language);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Name.Alias, sreg.Nickname);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Contact.HomeAddress.PostalCode, sreg.PostalCode);
				OpenIdExtensionsInteropHelper.FetchAttribute(ax, format, WellKnownAttributes.Preferences.TimeZone, sreg.TimeZone);
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
			Requires.NotNull(response, "response");

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
					sreg.Gender = (Gender)OpenIdExtensionsInteropHelper.GenderEncoder.Decode(gender);
				}
			}

			return sreg;
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
			return OpenIdExtensionsInteropHelper.ForEachFormat(formats).Select(format => fetchResponse.GetAttributeValue(OpenIdExtensionsInteropHelper.TransformAXFormat(typeUri, format))).FirstOrDefault(s => s != null);
		}

		/// <summary>
		/// Tries to find the exact format of AX attribute Type URI supported by the Provider.
		/// </summary>
		/// <param name="request">The authentication request.</param>
		/// <param name="attributeFormat">The attribute formats the RP will try if this discovery fails.</param>
		/// <returns>The AX format(s) to use based on the Provider's advertised AX support.</returns>
		private static bool TryDetectOPAttributeFormat(RelyingParty.IAuthenticationRequest request, out AXAttributeFormats attributeFormat) {
			Requires.NotNull(request, "request");
			attributeFormat = OpenIdExtensionsInteropHelper.DetectAXFormat(request.DiscoveryResult.Capabilities);
			return attributeFormat != AXAttributeFormats.None;
		}
	}
}
