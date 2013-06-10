//-----------------------------------------------------------------------
// <copyright file="AssociateDiffieHellmanProviderResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Provider;
	using Org.Mentalis.Security.Cryptography;

	/// <summary>
	/// The successful Diffie-Hellman association response message.
	/// </summary>
	/// <remarks>
	/// Association response messages are described in OpenID 2.0 section 8.2.  This type covers section 8.2.3.
	/// </remarks>
	internal class AssociateDiffieHellmanProviderResponse : AssociateDiffieHellmanResponse, IAssociateSuccessfulResponseProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateDiffieHellmanProviderResponse"/> class.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="originatingRequest">The originating request.</param>
		internal AssociateDiffieHellmanProviderResponse(Version responseVersion, AssociateDiffieHellmanRequest originatingRequest)
			: base(responseVersion, originatingRequest) {
		}

		/// <summary>
		/// Gets or sets the lifetime, in seconds, of this association. The Relying Party MUST NOT use the association after this time has passed.
		/// </summary>
		/// <value>
		/// An integer, represented in base 10 ASCII.
		/// </value>
		long IAssociateSuccessfulResponseProvider.ExpiresIn {
			get { return this.ExpiresIn; }
			set { this.ExpiresIn = value; }
		}

		/// <summary>
		/// Gets or sets the association handle is used as a key to refer to this association in subsequent messages.
		/// </summary>
		/// <value>
		/// A string 255 characters or less in length. It MUST consist only of ASCII characters in the range 33-126 inclusive (printable non-whitespace characters).
		/// </value>
		string IAssociateSuccessfulResponseProvider.AssociationHandle {
			get { return this.AssociationHandle; }
			set { this.AssociationHandle = value; }
		}

		/// <summary>
		/// Creates the association at the provider side after the association request has been received.
		/// </summary>
		/// <param name="request">The association request.</param>
		/// <param name="associationStore">The OpenID Provider's association store or handle encoder.</param>
		/// <param name="securitySettings">The security settings of the Provider.</param>
		/// <returns>
		/// The newly created association.
		/// </returns>
		/// <remarks>
		/// The response message is updated to include the details of the created association by this method,
		/// but the resulting association is <i>not</i> added to the association store and must be done by the caller.
		/// </remarks>
		public Association CreateAssociationAtProvider(AssociateRequest request, IProviderAssociationStore associationStore, ProviderSecuritySettings securitySettings) {
			var diffieHellmanRequest = request as AssociateDiffieHellmanRequest;
			ErrorUtilities.VerifyInternal(diffieHellmanRequest != null, "Expected a DH request type.");

			this.SessionType = this.SessionType ?? request.SessionType;

			// Go ahead and create the association first, complete with its secret that we're about to share.
			Association association = HmacShaAssociationProvider.Create(this.Protocol, this.AssociationType, AssociationRelyingPartyType.Smart, associationStore, securitySettings);

			// We now need to securely communicate the secret to the relying party using Diffie-Hellman.
			// We do this by performing a DH algorithm on the secret and setting a couple of properties
			// that will be transmitted to the Relying Party.  The RP will perform an inverse operation
			// using its part of a DH secret in order to decrypt the shared secret we just invented 
			// above when we created the association.
			using (DiffieHellman dh = new DiffieHellmanManaged(
				diffieHellmanRequest.DiffieHellmanModulus ?? AssociateDiffieHellmanRequest.DefaultMod,
				diffieHellmanRequest.DiffieHellmanGen ?? AssociateDiffieHellmanRequest.DefaultGen,
				AssociateDiffieHellmanRequest.DefaultX)) {
				HashAlgorithm hasher = DiffieHellmanUtilities.Lookup(this.Protocol, this.SessionType);
				this.DiffieHellmanServerPublic = DiffieHellmanUtilities.EnsurePositive(dh.CreateKeyExchange());
				this.EncodedMacKey = DiffieHellmanUtilities.SHAHashXorSecret(hasher, dh, diffieHellmanRequest.DiffieHellmanConsumerPublic, association.SecretKey);
			}
			return association;
		}
	}
}
