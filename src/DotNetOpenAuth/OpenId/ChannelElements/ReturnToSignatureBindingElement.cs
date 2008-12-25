//-----------------------------------------------------------------------
// <copyright file="ReturnToSignatureBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using System.Web;
	using System.Security.Cryptography;
	using System.Collections.Specialized;

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
		/// The optimal length for a private secret used for signing using the HMACSHA256 class.
		/// </summary>
		/// <remarks>
		/// The 64-byte length is optimized for highest security when used with HMACSHA256.
		/// See HMACSHA256.HMACSHA256(byte[]) documentation for more information.
		/// </remarks>
		internal static readonly int OptimalPrivateSecretLength = 64;

		private static readonly string ReturnToSignatureParameterName = "dnoi.return_to_sig";
	
		private HashAlgorithm signingHasher;

		internal ReturnToSignatureBindingElement(IPrivateSecretStore secretStore) {
			ErrorUtilities.VerifyArgumentNotNull(secretStore, "secretStore");
			ErrorUtilities.VerifyInternal(secretStore.PrivateSecret != null, "Private secret should have been set already.");

			if (secretStore.PrivateSecret.Length < OptimalPrivateSecretLength) {
				Logger.WarnFormat("For best security, the optimal length of a private signing secret is {0} bytes, but the secret we have is only {1} bytes.", OptimalPrivateSecretLength, secretStore.PrivateSecret.Length);
			}
		
			this.signingHasher = new HMACSHA256(secretStore.PrivateSecret);
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
				string signature = GetReturnToSignature(request.ReturnTo);
				request.AddReturnToArguments(ReturnToSignatureParameterName, signature);
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
				// This binding element tends to get "speedboated" to higher in the channel stack
				// by the SigningBindingElement.  Only do the work here if it hasn't been done yet
				// to avoid signing twice (once for the SigningBindingElement and once for the channel).
				if (!response.ReturnToParametersSignatureValidated) {
					// We can't use response.GetReturnToArgument(string) because that relies
					// on us already having validated this signature.
					NameValueCollection returnToParameters = HttpUtility.ParseQueryString(response.ReturnTo.Query);

					// Set the safety flag showing whether the return_to url had a valid signature.
					string expected = GetReturnToSignature(response.ReturnTo);
					string actual = returnToParameters[ReturnToSignatureParameterName];
					response.ReturnToParametersSignatureValidated = actual == expected;
					if (!response.ReturnToParametersSignatureValidated) {
						Logger.WarnFormat("The return_to signature failed verification.");
					}
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Gets the return to signature.
		/// </summary>
		/// <param name="returnTo">The return to.</param>
		/// <returns></returns>
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
			var returnToParameters = HttpUtility.ParseQueryString(returnTo.Query).ToDictionary();
			returnToParameters.Remove(ReturnToSignatureParameterName);
			var sortedReturnToParameters = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var pair in returnToParameters) {
				sortedReturnToParameters.Add(pair.Key, pair.Value);
			}

			Logger.DebugFormat("ReturnTo signed data: {0}", sortedReturnToParameters.ToStringDeferred());

			// Sign the parameters.
			byte[] bytesToSign = KeyValueFormEncoding.GetBytes(sortedReturnToParameters);
			byte[] signature = this.signingHasher.ComputeHash(bytesToSign);
			string signatureBase64 = Convert.ToBase64String(signature);
			return signatureBase64;
		}
	}
}
