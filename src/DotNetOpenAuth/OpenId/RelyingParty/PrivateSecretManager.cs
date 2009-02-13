//-----------------------------------------------------------------------
// <copyright file="PrivateSecretManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Manages signing at the RP using private secrets.
	/// </summary>
	internal class PrivateSecretManager {
		/// <summary>
		/// The optimal length for a private secret used for signing using the HMACSHA256 class.
		/// </summary>
		/// <remarks>
		/// The 64-byte length is optimized for highest security when used with HMACSHA256.
		/// See HMACSHA256.HMACSHA256(byte[]) documentation for more information.
		/// </remarks>
		private const int OptimalPrivateSecretLength = 64;

		/// <summary>
		/// The URI to use for private associations at this RP.
		/// </summary>
		private static readonly Uri SecretUri = new Uri("https://localhost/dnoa/secret");

		/// <summary>
		/// The security settings that apply to this Relying Party.
		/// </summary>
		private RelyingPartySecuritySettings securitySettings;

		/// <summary>
		/// The association store
		/// </summary>
		private IAssociationStore<Uri> store;

		/// <summary>
		/// Initializes a new instance of the <see cref="PrivateSecretManager"/> class.
		/// </summary>
		/// <param name="securitySettings">The security settings.</param>
		/// <param name="store">The association store.</param>
		internal PrivateSecretManager(RelyingPartySecuritySettings securitySettings, IAssociationStore<Uri> store) {
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");
			ErrorUtilities.VerifyArgumentNotNull(store, "store");

			this.securitySettings = securitySettings;
			this.store = store;
		}

		/// <summary>
		/// Gets the handle of the association to use for private signatures.
		/// </summary>
		/// <returns>
		/// An string made up of plain ASCII characters.
		/// </returns>
		internal string CurrentHandle {
			get {
				Association association = this.GetAssociation();
				return association.Handle;
			}
		}

		/// <summary>
		/// Used to verify a signature previously written.
		/// </summary>
		/// <param name="buffer">The data whose signature is to be verified.</param>
		/// <param name="handle">The handle to the private association used to sign the data.</param>
		/// <returns>
		/// The signature for the given buffer using the provided handle.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when an association with the given handle could not be found.
		/// This most likely happens if the association was near the end of its life and the user took too long to log in.</exception>
		internal byte[] Sign(byte[] buffer, string handle) {
			ErrorUtilities.VerifyArgumentNotNull(buffer, "buffer");
			ErrorUtilities.VerifyNonZeroLength(handle, "handle");

			Association association = this.store.GetAssociation(SecretUri, handle);
			ErrorUtilities.VerifyProtocol(association != null, OpenIdStrings.PrivateRPSecretNotFound, handle);
			return association.Sign(buffer);
		}

		/// <summary>
		/// Gets an association to use for signing new data.
		/// </summary>
		/// <returns>
		/// The association, which may have previously existed or
		/// may have been created as a result of this call.
		/// </returns>
		private Association GetAssociation() {
			Association privateAssociation = this.store.GetAssociation(SecretUri, this.securitySettings);
			if (privateAssociation == null || !privateAssociation.HasUsefulLifeRemaining) {
				int secretLength = HmacShaAssociation.GetSecretLength(Protocol.Default, Protocol.Default.Args.SignatureAlgorithm.Best);
				byte[] secret = MessagingUtilities.GetCryptoRandomData(secretLength);
				privateAssociation = HmacShaAssociation.Create(this.CreateNewAssociationHandle(), secret, this.securitySettings.PrivateSecretMaximumAge);
				if (!privateAssociation.HasUsefulLifeRemaining) {
					Logger.WarnFormat(
						"Brand new private association has a shorter lifespan ({0}) than the maximum allowed authentication time for a user ({1}).  This may lead to login failures.",
						this.securitySettings.PrivateSecretMaximumAge,
						DotNetOpenAuthSection.Configuration.OpenId.MaxAuthenticationTime);
				}

				this.store.StoreAssociation(SecretUri, privateAssociation);
			}

			return privateAssociation;
		}

		/// <summary>
		/// Creates the new association handle.
		/// </summary>
		/// <returns>The ASCII-encoded handle name.</returns>
		private string CreateNewAssociationHandle() {
			string uniq = MessagingUtilities.GetCryptoRandomDataAsBase64(4);
			string handle = "{" + DateTime.UtcNow.Ticks + "}{" + uniq + "}";
			return handle;
		}
	}
}
