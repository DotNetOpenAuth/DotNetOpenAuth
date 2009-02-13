//-----------------------------------------------------------------------
// <copyright file="ReturnToSignatureBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Security.Cryptography;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// This binding element signs a Relying Party's openid.return_to parameter
	/// so that upon return, it can verify that it hasn't been tampered with.
	/// </summary>
	/// <remarks>
	/// <para>Since Providers can send unsolicited assertions, not all openid.return_to
	/// values will be signed.  But those that are signed will be validated, and
	/// any invalid or missing signatures will cause this library to not trust
	/// the parameters in the return_to URL.</para>
	/// <para>In the messaging stack, this binding element looks like an ordinary
	/// transform-type of binding element rather than a protection element,
	/// due to its required order in the channel stack and that it doesn't sign
	/// anything except a particular message part.</para>
	/// </remarks>
	internal class ReturnToSignatureBindingElement : IChannelBindingElement {
		/// <summary>
		/// The name of the callback parameter we'll tack onto the return_to value
		/// to store our signature on the return_to parameter.
		/// </summary>
		private const string ReturnToSignatureParameterName = "dnoi.return_to_sig";

		/// <summary>
		/// The name of the callback parameter we'll tack onto the return_to value
		/// to store the handle of the association we use to sign the return_to parameter.
		/// </summary>
		private const string ReturnToSignatureHandleParameterName = "dnoi.return_to_sig_handle";

		/// <summary>
		/// The hashing algorithm used to generate the private signature on the return_to parameter.
		/// </summary>
		private PrivateSecretManager secretManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReturnToSignatureBindingElement"/> class.
		/// </summary>
		/// <param name="secretStore">The secret store from which to retrieve the secret used for signing.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal ReturnToSignatureBindingElement(IAssociationStore<Uri> secretStore, RelyingPartySecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(secretStore, "secretStore");

			this.secretManager = new PrivateSecretManager(securitySettings, secretStore);
		}

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel { get; set; }

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.None"/></value>
		public MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			SignedResponseRequest request = message as SignedResponseRequest;
			if (request != null) {
				request.AddReturnToArguments(ReturnToSignatureHandleParameterName, this.secretManager.CurrentHandle);
				request.AddReturnToArguments(ReturnToSignatureParameterName, this.GetReturnToSignature(request.ReturnTo));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False if the operation did not apply to this message.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			IndirectSignedResponse response = message as IndirectSignedResponse;

			if (response != null) {
				// We can't use response.GetReturnToArgument(string) because that relies
				// on us already having validated this signature.
				NameValueCollection returnToParameters = HttpUtility.ParseQueryString(response.ReturnTo.Query);

				// Only check the return_to signature if one is present.
				if (returnToParameters[ReturnToSignatureHandleParameterName] != null) {
					// Set the safety flag showing whether the return_to url had a valid signature.
					string expected = this.GetReturnToSignature(response.ReturnTo);
					string actual = returnToParameters[ReturnToSignatureParameterName];
					actual = OpenIdUtilities.FixDoublyUriDecodedBase64String(actual);
					response.ReturnToParametersSignatureValidated = actual == expected;
					if (!response.ReturnToParametersSignatureValidated) {
						Logger.WarnFormat("The return_to signature failed verification.");
					}

					return true;
				}
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Gets the return to signature.
		/// </summary>
		/// <param name="returnTo">The return to.</param>
		/// <returns>The generated signature.</returns>
		/// <remarks>
		/// Only the parameters in the return_to URI are signed, rather than the base URI
		/// itself, in order that OPs that might change the return_to's implicit port :80 part
		/// or other minor changes do not invalidate the signature.
		/// </remarks>
		private string GetReturnToSignature(Uri returnTo) {
			ErrorUtilities.VerifyArgumentNotNull(returnTo, "returnTo");

			// Assemble the dictionary to sign, taking care to remove the signature itself
			// in order to accurately reproduce the original signature (which of course didn't include
			// the signature).
			// Also we need to sort the dictionary's keys so that we sign in the same order as we did
			// the last time.
			var returnToParameters = HttpUtility.ParseQueryString(returnTo.Query);
			returnToParameters.Remove(ReturnToSignatureParameterName);
			var sortedReturnToParameters = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (string key in returnToParameters) {
				sortedReturnToParameters.Add(key, returnToParameters[key]);
			}

			Logger.DebugFormat("ReturnTo signed data: {0}{1}", Environment.NewLine, sortedReturnToParameters.ToStringDeferred());

			// Sign the parameters.
			byte[] bytesToSign = KeyValueFormEncoding.GetBytes(sortedReturnToParameters);
			byte[] signature;
			try {
				signature = this.secretManager.Sign(bytesToSign, returnToParameters[ReturnToSignatureHandleParameterName]);
			} catch (ProtocolException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.MaximumAuthenticationTimeExpired);
			}
			string signatureBase64 = Convert.ToBase64String(signature);
			return signatureBase64;
		}
	}
}
