//-----------------------------------------------------------------------
// <copyright file="ExtensionsInteropHelper.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
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
	internal static class ExtensionsInteropHelper {
		/// <summary>
		/// Transforms an AX attribute type URI from the axschema.org format into a given format.
		/// </summary>
		/// <param name="axSchemaOrgFormatTypeUri">The ax schema org format type URI.</param>
		/// <param name="targetFormat">The target format.  Only one flag should be set.</param>
		/// <returns>The AX attribute type URI in the target format.</returns>
		internal static string TransformAXFormatTestHook(string axSchemaOrgFormatTypeUri, AXAttributeFormats targetFormat) {
			return OpenIdExtensionsInteropHelper.TransformAXFormat(axSchemaOrgFormatTypeUri, targetFormat);
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
			Requires.NotNull(request, "request");

			var req = (Provider.HostProcessedRequest)request;
			var sreg = req.GetExtension<ClaimsRequest>();
			if (sreg != null) {
				return sreg;
			}

			var ax = req.GetExtension<FetchRequest>();
			if (ax != null) {
				sreg = new ClaimsRequest(DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.Constants.TypeUris.Standard);
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		/// <remarks>
		/// If the original attribute request came in as AX, the Simple Registration extension is converted
		/// to an AX response and then the Simple Registration extension is removed from the response.
		/// </remarks>
		internal static async Task ConvertSregToMatchRequestAsync(this Provider.IHostProcessedRequest request, CancellationToken cancellationToken) {
			var req = (Provider.HostProcessedRequest)request;
			var protocolMessage = await req.GetResponseAsync(cancellationToken);
			var response = protocolMessage as IProtocolMessageWithExtensions; // negative responses don't support extensions.
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

					AXAttributeFormats format = OpenIdExtensionsInteropHelper.DetectAXFormat(axRequest.Attributes.Select(att => att.TypeUri));
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
						AddAXAttributeValue(axResponse, WellKnownAttributes.Person.Gender, format, OpenIdExtensionsInteropHelper.GenderEncoder.Encode(sregResponse.Gender));
					}
				}
			}
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
				string targetTypeUri = OpenIdExtensionsInteropHelper.TransformAXFormat(typeUri, format);
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
			Requires.NotNull(ax, "ax");
			Requires.NotNullOrEmpty(typeUri, "typeUri");

			foreach (AXAttributeFormats format in OpenIdExtensionsInteropHelper.ForEachFormat(AXAttributeFormats.All)) {
				string typeUriInFormat = OpenIdExtensionsInteropHelper.TransformAXFormat(typeUri, format);
				if (ax.Attributes.Contains(typeUriInFormat)) {
					return ax.Attributes[typeUriInFormat].IsRequired ? DemandLevel.Require : DemandLevel.Request;
				}
			}

			return DemandLevel.NoRequest;
		}
	}
}
