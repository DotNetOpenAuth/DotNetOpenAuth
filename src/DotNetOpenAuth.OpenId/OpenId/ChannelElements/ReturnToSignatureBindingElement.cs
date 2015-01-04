//-----------------------------------------------------------------------
// <copyright file="ReturnToSignatureBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Security.Cryptography;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

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
		/// A reusable pre-completed task that may be returned multiple times to reduce GC pressure.
		/// </summary>
		private static readonly Task<MessageProtections?> NullTask = Task.FromResult<MessageProtections?>(null);

		/// <summary>
		/// A reusable pre-completed task that may be returned multiple times to reduce GC pressure.
		/// </summary>
		private static readonly Task<MessageProtections?> NoneTask =
			Task.FromResult<MessageProtections?>(MessageProtections.None);

		/// <summary>
		/// The name of the callback parameter we'll tack onto the return_to value
		/// to store our signature on the return_to parameter.
		/// </summary>
		private const string ReturnToSignatureParameterName = OpenIdUtilities.CustomParameterPrefix + "return_to_sig";

		/// <summary>
		/// The name of the callback parameter we'll tack onto the return_to value
		/// to store the handle of the association we use to sign the return_to parameter.
		/// </summary>
		private const string ReturnToSignatureHandleParameterName = OpenIdUtilities.CustomParameterPrefix + "return_to_sig_handle";

		/// <summary>
		/// The URI to use for private associations at this RP.
		/// </summary>
		private static readonly Uri SecretUri = new Uri("https://localhost/dnoa/secret");

		/// <summary>
		/// The key store used to generate the private signature on the return_to parameter.
		/// </summary>
		private ICryptoKeyStore cryptoKeyStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReturnToSignatureBindingElement"/> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store.</param>
		internal ReturnToSignatureBindingElement(ICryptoKeyStore cryptoKeyStore) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");

			this.cryptoKeyStore = cryptoKeyStore;
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
		/// <remarks>
		/// No message protection is reported because this binding element
		/// does not protect the entire message -- only a part.
		/// </remarks>
		public MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			SignedResponseRequest request = message as SignedResponseRequest;
			if (request != null && request.ReturnTo != null && request.SignReturnTo) {
				var cryptoKeyPair = this.cryptoKeyStore.GetCurrentKey(SecretUri.AbsoluteUri, OpenIdElement.Configuration.MaxAuthenticationTime);
				request.AddReturnToArguments(ReturnToSignatureHandleParameterName, cryptoKeyPair.Key);
				string signature = Convert.ToBase64String(this.GetReturnToSignature(request.ReturnTo, cryptoKeyPair.Value));
				request.AddReturnToArguments(ReturnToSignatureParameterName, signature);

				// We return none because we are not signing the entire message (only a part).
				return NoneTask;
			}

			return NullTask;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			IndirectSignedResponse response = message as IndirectSignedResponse;

			if (response != null) {
				// We can't use response.GetReturnToArgument(string) because that relies
				// on us already having validated this signature.
				NameValueCollection returnToParameters = HttpUtility.ParseQueryString(response.ReturnTo.Query);

				// Only check the return_to signature if one is present.
				if (returnToParameters[ReturnToSignatureHandleParameterName] != null) {
					// Set the safety flag showing whether the return_to url had a valid signature.
					byte[] expectedBytes = this.GetReturnToSignature(response.ReturnTo);
					string actual = returnToParameters[ReturnToSignatureParameterName];
					actual = OpenIdUtilities.FixDoublyUriDecodedBase64String(actual);
					byte[] actualBytes = Convert.FromBase64String(actual);
					response.ReturnToParametersSignatureValidated = MessagingUtilities.AreEquivalentConstantTime(actualBytes, expectedBytes);
					if (!response.ReturnToParametersSignatureValidated) {
						Logger.Bindings.WarnFormat("The return_to signature failed verification.");
					}

					return NoneTask;
				}
			}

			return NullTask;
		}

		#endregion

		/// <summary>
		/// Gets the return to signature.
		/// </summary>
		/// <param name="returnTo">The return to.</param>
		/// <param name="cryptoKey">The crypto key.</param>
		/// <returns>
		/// The generated signature.
		/// </returns>
		/// <remarks>
		/// Only the parameters in the return_to URI are signed, rather than the base URI
		/// itself, in order that OPs that might change the return_to's implicit port :80 part
		/// or other minor changes do not invalidate the signature.
		/// </remarks>
		private byte[] GetReturnToSignature(Uri returnTo, CryptoKey cryptoKey = null) {
			Requires.NotNull(returnTo, "returnTo");

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

			Logger.Bindings.DebugFormat("ReturnTo signed data: {0}{1}", Environment.NewLine, sortedReturnToParameters.ToStringDeferred());

			// Sign the parameters.
			byte[] bytesToSign = KeyValueFormEncoding.GetBytes(sortedReturnToParameters);
			byte[] signature;
			try {
				if (cryptoKey == null) {
					cryptoKey = this.cryptoKeyStore.GetKey(SecretUri.AbsoluteUri, returnToParameters[ReturnToSignatureHandleParameterName]);
					ErrorUtilities.VerifyProtocol(
						cryptoKey != null,
						MessagingStrings.MissingDecryptionKeyForHandle,
						SecretUri.AbsoluteUri,
						returnToParameters[ReturnToSignatureHandleParameterName]);
				}

				using (var signer = HmacAlgorithms.Create(HmacAlgorithms.HmacSha256, cryptoKey.Key)) {
					signature = signer.ComputeHash(bytesToSign);
				}
			} catch (ProtocolException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.MaximumAuthenticationTimeExpired);
			}

			return signature;
		}
	}
}
