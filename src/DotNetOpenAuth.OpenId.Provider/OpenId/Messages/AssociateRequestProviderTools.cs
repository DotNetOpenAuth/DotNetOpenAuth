//-----------------------------------------------------------------------
// <copyright file="AssociateRequestProviderTools.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using Validation;

	/// <summary>
	/// OpenID Provider tools for receiving association requests.
	/// </summary>
	internal static class AssociateRequestProviderTools {
		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
		/// <param name="requestMessage">The request message.</param>
		/// <param name="associationStore">The association store.</param>
		/// <param name="securitySettings">The security settings on the Provider.</param>
		/// <returns>
		/// The appropriate association response that is ready to be sent back to the Relying Party.
		/// </returns>
		/// <remarks>
		///   <para>If an association is created, it will be automatically be added to the provided
		/// association store.</para>
		///   <para>Successful association response messages will derive from <see cref="AssociateSuccessfulResponse"/>.
		/// Failed association response messages will derive from <see cref="AssociateUnsuccessfulResponse"/>.</para>
		/// </remarks>
		internal static IProtocolMessage CreateResponse(IAssociateRequestProvider requestMessage, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Requires.NotNull(requestMessage, "requestMessage");
			Requires.NotNull(associationStore, "associationStore");
			Requires.NotNull(securitySettings, "securitySettings");

			AssociateRequest request = (AssociateRequest)requestMessage;
			IProtocolMessage response;
			var protocol = requestMessage.GetProtocol();
			if (securitySettings.IsAssociationInPermittedRange(protocol, request.AssociationType) &&
				HmacShaAssociation.IsDHSessionCompatible(protocol, request.AssociationType, request.SessionType)) {
				response = requestMessage.CreateResponseCore();

				// Create and store the association if this is a successful response.
				var successResponse = response as IAssociateSuccessfulResponseProvider;
				if (successResponse != null) {
					OpenIdProviderUtilities.CreateAssociation(request, successResponse, associationStore, securitySettings);
				}
			} else {
				response = CreateUnsuccessfulResponse(requestMessage, securitySettings);
			}

			return response;
		}

		/// <summary>
		/// Creates a response that notifies the Relying Party that the requested
		/// association type is not supported by this Provider, and offers
		/// an alternative association type, if possible.
		/// </summary>
		/// <param name="requestMessage">The request message.</param>
		/// <param name="securitySettings">The security settings that apply to this Provider.</param>
		/// <returns>
		/// The response to send to the Relying Party.
		/// </returns>
		private static AssociateUnsuccessfulResponse CreateUnsuccessfulResponse(IAssociateRequestProvider requestMessage, ProviderSecuritySettings securitySettings) {
			Requires.NotNull(requestMessage, "requestMessage");
			Requires.NotNull(securitySettings, "securitySettings");

			var unsuccessfulResponse = new AssociateUnsuccessfulResponse(requestMessage.Version, (AssociateRequest)requestMessage);

			// The strategy here is to suggest that the RP try again with the lowest
			// permissible security settings, giving the RP the best chance of being
			// able to match with a compatible request.
			bool unencryptedAllowed = requestMessage.Recipient.IsTransportSecure();
			bool useDiffieHellman = !unencryptedAllowed;
			var request = (AssociateRequest)requestMessage;
			var protocol = requestMessage.GetProtocol();
			string associationType, sessionType;
			if (HmacShaAssociation.TryFindBestAssociation(protocol, false, securitySettings, useDiffieHellman, out associationType, out sessionType)) {
				ErrorUtilities.VerifyInternal(request.AssociationType != associationType, "The RP asked for an association that should have been allowed, but the OP is trying to suggest the same one as an alternative!");
				unsuccessfulResponse.AssociationType = associationType;
				unsuccessfulResponse.SessionType = sessionType;
				Logger.OpenId.InfoFormat(
					"Association requested of type '{0}' and session '{1}', which the Provider does not support.  Sending back suggested alternative of '{0}' with session '{1}'.",
					request.AssociationType,
					request.SessionType,
					unsuccessfulResponse.AssociationType,
					unsuccessfulResponse.SessionType);
			} else {
				Logger.OpenId.InfoFormat("Association requested of type '{0}' and session '{1}', which the Provider does not support.  No alternative association type qualified for suggesting back to the Relying Party.", request.AssociationType, request.SessionType);
			}

			return unsuccessfulResponse;
		}
	}
}
