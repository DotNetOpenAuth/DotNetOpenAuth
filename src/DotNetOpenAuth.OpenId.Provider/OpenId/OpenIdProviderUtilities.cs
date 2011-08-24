//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderUtilities.cs" company="Andrew Arnott">
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
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// Utility methods for OpenID Providers.
	/// </summary>
	internal static class OpenIdProviderUtilities {
		/// <summary>
		/// Called to create the Association based on a request previously given by the Relying Party.
		/// </summary>
		/// <param name="request">The prior request for an association.</param>
		/// <param name="response">The response.</param>
		/// <param name="associationStore">The Provider's association store.</param>
		/// <param name="securitySettings">The security settings for the Provider.  Should be <c>null</c> for Relying Parties.</param>
		/// <returns>
		/// The created association.
		/// </returns>
		/// <remarks>
		/// The response message is updated to include the details of the created association by this method.
		/// This method is called by both the Provider and the Relying Party, but actually performs
		/// quite different operations in either scenario.
		/// </remarks>
		internal static Association CreateAssociation(AssociateRequest request, IAssociateSuccessfulResponseProvider response, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentNullException>(response != null, "response");
			Contract.Requires<ArgumentNullException>(securitySettings != null, "securitySettings");

			// We need to initialize some common properties based on the created association.
			var association = response.CreateAssociationAtProvider(request, associationStore, securitySettings);
			response.ExpiresIn = association.SecondsTillExpiration;
			response.AssociationHandle = association.Handle;

			return association;
		}

		/// <summary>
		/// Determines whether the association with the specified handle is (still) valid.
		/// </summary>
		/// <param name="associationStore">The association store.</param>
		/// <param name="containingMessage">The OpenID message that referenced this association handle.</param>
		/// <param name="isPrivateAssociation">A value indicating whether a private association is expected.</param>
		/// <param name="handle">The association handle.</param>
		/// <returns>
		///   <c>true</c> if the specified containing message is valid; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsValid(this IProviderAssociationStore associationStore, IProtocolMessage containingMessage, bool isPrivateAssociation, string handle) {
			Contract.Requires<ArgumentNullException>(associationStore != null);
			Contract.Requires<ArgumentNullException>(containingMessage != null);
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(handle));
			try {
				return associationStore.Deserialize(containingMessage, isPrivateAssociation, handle) != null;
			} catch (ProtocolException) {
				return false;
			}
		}
	}
}
