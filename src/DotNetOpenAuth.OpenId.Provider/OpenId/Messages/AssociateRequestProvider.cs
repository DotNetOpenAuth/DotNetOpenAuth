namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal abstract class AssociateRequestProvider : AssociateRequest {
		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
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
		internal IProtocolMessage CreateResponse(IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(associationStore != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);

			IProtocolMessage response;
			if (securitySettings.IsAssociationInPermittedRange(Protocol, this.AssociationType) &&
				HmacShaAssociation.IsDHSessionCompatible(Protocol, this.AssociationType, this.SessionType)) {
				response = this.CreateResponseCore();

				// Create and store the association if this is a successful response.
				var successResponse = response as AssociateSuccessfulResponse;
				if (successResponse != null) {
					successResponse.CreateAssociation(this, associationStore, securitySettings);
				}
			} else {
				response = this.CreateUnsuccessfulResponse(securitySettings);
			}

			return response;
		}

		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
		/// <returns>
		/// The appropriate association response message.
		/// </returns>
		/// <remarks>
		/// <para>If an association can be successfully created, the 
		/// <see cref="AssociateSuccessfulResponse.CreateAssociation"/> method must not be
		/// called by this method.</para>
		/// <para>Successful association response messages will derive from <see cref="AssociateSuccessfulResponse"/>.
		/// Failed association response messages will derive from <see cref="AssociateUnsuccessfulResponse"/>.</para>
		/// </remarks>
		protected abstract IProtocolMessage CreateResponseCore();

		/// <summary>
		/// Creates a response that notifies the Relying Party that the requested
		/// association type is not supported by this Provider, and offers
		/// an alternative association type, if possible.
		/// </summary>
		/// <param name="securitySettings">The security settings that apply to this Provider.</param>
		/// <returns>The response to send to the Relying Party.</returns>
		private AssociateUnsuccessfulResponse CreateUnsuccessfulResponse(ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(securitySettings != null);

			var unsuccessfulResponse = new AssociateUnsuccessfulResponse(this.Version, this);

			// The strategy here is to suggest that the RP try again with the lowest
			// permissible security settings, giving the RP the best chance of being
			// able to match with a compatible request.
			bool unencryptedAllowed = this.Recipient.IsTransportSecure();
			bool useDiffieHellman = !unencryptedAllowed;
			string associationType, sessionType;
			if (HmacShaAssociation.TryFindBestAssociation(Protocol, false, securitySettings, useDiffieHellman, out associationType, out sessionType)) {
				ErrorUtilities.VerifyInternal(this.AssociationType != associationType, "The RP asked for an association that should have been allowed, but the OP is trying to suggest the same one as an alternative!");
				unsuccessfulResponse.AssociationType = associationType;
				unsuccessfulResponse.SessionType = sessionType;
				Logger.OpenId.InfoFormat(
					"Association requested of type '{0}' and session '{1}', which the Provider does not support.  Sending back suggested alternative of '{0}' with session '{1}'.",
					this.AssociationType,
					this.SessionType,
					unsuccessfulResponse.AssociationType,
					unsuccessfulResponse.SessionType);
			} else {
				Logger.OpenId.InfoFormat("Association requested of type '{0}' and session '{1}', which the Provider does not support.  No alternative association type qualified for suggesting back to the Relying Party.", this.AssociationType, this.SessionType);
			}

			return unsuccessfulResponse;
		}

	}
}
