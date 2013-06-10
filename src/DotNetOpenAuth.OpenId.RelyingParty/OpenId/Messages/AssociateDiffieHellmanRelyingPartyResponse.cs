//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanRelyingPartyResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using Org.Mentalis.Security.Cryptography;

	/// <summary>
	/// The successful Diffie-Hellman association response message.
	/// </summary>
	/// <remarks>
	/// Association response messages are described in OpenID 2.0 section 8.2.  This type covers section 8.2.3.
	/// </remarks>
	internal class AssociateDiffieHellmanRelyingPartyResponse : AssociateDiffieHellmanResponse, IAssociateSuccessfulResponseRelyingParty {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateDiffieHellmanRelyingPartyResponse"/> class.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="originatingRequest">The originating request.</param>
		internal AssociateDiffieHellmanRelyingPartyResponse(Version responseVersion, AssociateDiffieHellmanRequest originatingRequest)
			: base(responseVersion, originatingRequest) {
		}

		/// <summary>
		/// Creates the association at relying party side after the association response has been received.
		/// </summary>
		/// <param name="request">The original association request that was already sent and responded to.</param>
		/// <returns>The newly created association.</returns>
		/// <remarks>
		/// The resulting association is <i>not</i> added to the association store and must be done by the caller.
		/// </remarks>
		public Association CreateAssociationAtRelyingParty(AssociateRequest request) {
			var diffieHellmanRequest = request as AssociateDiffieHellmanRequest;
			ErrorUtilities.VerifyArgument(diffieHellmanRequest != null, OpenIdStrings.DiffieHellmanAssociationRequired);

			HashAlgorithm hasher = DiffieHellmanUtilities.Lookup(Protocol, this.SessionType);
			byte[] associationSecret = DiffieHellmanUtilities.SHAHashXorSecret(hasher, diffieHellmanRequest.Algorithm, this.DiffieHellmanServerPublic, this.EncodedMacKey);

			Association association = HmacShaAssociation.Create(Protocol, this.AssociationType, this.AssociationHandle, associationSecret, TimeSpan.FromSeconds(this.ExpiresIn));
			return association;
		}
	}
}
