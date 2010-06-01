//-----------------------------------------------------------------------
// <copyright file="DataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuthWrap.Messages;

	internal abstract class DataBag : MessageBase {
		private const int NonceLength = 6;

		private readonly bool signed;

		private readonly INonceStore decodeOnceOnly;

		private readonly TimeSpan? maximumAge;

		private readonly bool encrypted;

		private readonly bool compressed;

		protected DataBag(OAuthWrapAuthorizationServerChannel channel, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(Protocol.Default.Version) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentException>(channel.AuthorizationServer != null);
			Contract.Requires<ArgumentException>(signed || decodeOnceOnly == null, "A signature must be applied if this data is meant to be decoded only once.");
			Contract.Requires<ArgumentException>(maximumAge.HasValue || decodeOnceOnly == null, "A maximum age must be given if a message can only be decoded once.");

			this.Hasher = new HMACSHA256(channel.AuthorizationServer.Secret);
			this.Channel = channel;
			this.signed = signed;
			this.maximumAge = maximumAge;
			this.decodeOnceOnly = decodeOnceOnly;
			this.encrypted = encrypted;
			this.compressed = compressed;
		}

		protected OAuthWrapAuthorizationServerChannel Channel { get; set; }

		protected HashAlgorithm Hasher { get; set; }

		[MessagePart("sig")]
		private string Signature { get; set; }

		[MessagePart]
		internal string Nonce { get; set; }

		[MessagePart("timestamp", IsRequired = true, Encoder = typeof(TimestampEncoder))]
		internal DateTime UtcCreationDate { get; set; }

		internal virtual string Encode() {
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			this.UtcCreationDate = DateTime.UtcNow;

			if (decodeOnceOnly != null) {
				this.Nonce = Convert.ToBase64String(MessagingUtilities.GetNonCryptoRandomData(NonceLength));
			}

			if (signed) {
				this.Signature = this.CalculateSignature();
			}

			var fields = this.Channel.MessageDescriptions.GetAccessor(this);
			string value = Uri.EscapeDataString(this.BagTypeName) + "&" + MessagingUtilities.CreateQueryString(fields);

			byte[] encoded = Encoding.UTF8.GetBytes(value);

			if (compressed) {
				encoded = MessagingUtilities.Compress(encoded);
			}

			if (encrypted) {
				encoded = MessagingUtilities.Encrypt(encoded, this.Channel.AuthorizationServer.Secret);
			}

			return Convert.ToBase64String(encoded);
		}

		protected virtual void Decode(string value, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");

			byte[] encoded = Convert.FromBase64String(value);

			if (encrypted) {
				encoded = MessagingUtilities.Decrypt(encoded, this.Channel.AuthorizationServer.Secret);
			}

			if (compressed) {
				encoded = MessagingUtilities.Decompress(encoded);
			}

			value = Encoding.UTF8.GetString(encoded);

			// Deserialize into this newly created instance.
			var fields = this.Channel.MessageDescriptions.GetAccessor(this);
			string[] halves = value.Split(new char[] { '&' }, 2);
			ErrorUtilities.VerifyProtocol(string.Equals(halves[0], Uri.EscapeDataString(this.BagTypeName), StringComparison.Ordinal), "Unexpected type of message while decoding.");
			value = halves[1];

			var nvc = HttpUtility.ParseQueryString(value);
			foreach (string key in nvc) {
				fields[key] = nvc[key];
			}

			if (signed) {
				// Verify that the verification code was issued by this authorization server.
				ErrorUtilities.VerifyProtocol(string.Equals(this.Signature, this.CalculateSignature(), StringComparison.Ordinal), Protocol.bad_verification_code);
			}

			if (maximumAge.HasValue) {
				// Has this verification code expired?
				DateTime expirationDate = this.UtcCreationDate + this.maximumAge.Value;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, containingMessage);
				}
			}

			// Has this verification code already been used to obtain an access/refresh token?
			if (decodeOnceOnly != null) {
				ErrorUtilities.VerifyInternal(this.maximumAge.HasValue, "Oops!  How can we validate a nonce without a maximum message age?");
				string context = "{" + GetType().FullName + "}";
				if (!this.decodeOnceOnly.StoreNonce(context, this.Nonce, this.UtcCreationDate)) {
					Logger.OpenId.ErrorFormat("Replayed nonce detected ({0} {1}).  Rejecting message.", this.Nonce, this.UtcCreationDate);
					throw new ReplayedMessageException(containingMessage);
				}
			}
		}

		private string BagTypeName {
			get { return this.GetType().Name; }
		}

		/// <summary>
		/// Calculates the signature for the data in this verification code.
		/// </summary>
		/// <returns>The calculated signature.</returns>
		private string CalculateSignature() {
			// Sign the data, being sure to avoid any impact of the signature field itself.
			var fields = this.Channel.MessageDescriptions.GetAccessor(this);
			var fieldsCopy = fields.ToDictionary();
			fieldsCopy.Remove("sig");
			return this.Hasher.ComputeHash(fieldsCopy);
		}
	}
}
