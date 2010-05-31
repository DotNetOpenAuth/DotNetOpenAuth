using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Web;
using DotNetOpenAuth.Messaging;

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuthWrap.Messages;

	internal class VerificationCode : MessageBase, IMessageWithEvents {
		private HashAlgorithm hasher;

		private const int NonceLength = 6;

		/// <summary>
		/// Initializes a new instance of the <see cref="VerificationCode"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="callback">The callback.</param>
		internal VerificationCode(OAuthWrapAuthorizationServerChannel channel, Uri callback, string scope, string username)
			: this(channel) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentNullException>(callback != null, "callback");

			this.CallbackHash = this.CalculateCallbackHash(callback);
			this.Scope = scope;
			this.User = username;
			this.CreationDateUtc = DateTime.UtcNow;
			this.Nonce = Convert.ToBase64String(MessagingUtilities.GetNonCryptoRandomData(NonceLength));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VerificationCode"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		private VerificationCode(OAuthWrapAuthorizationServerChannel channel)
			: base(Protocol.Default.Version) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			this.Channel = channel;
			this.hasher = new HMACSHA256(this.Channel.AuthorizationServer.Secret);
		}

		/// <summary>
		/// Gets or sets the channel.
		/// </summary>
		public OAuthWrapAuthorizationServerChannel Channel { get; set; }

		[MessagePart("cb")]
		private string CallbackHash { get; set; }

		[MessagePart]
		internal string Scope { get; set; }

		[MessagePart]
		internal string User { get; set; }

		[MessagePart]
		internal string Nonce { get; set; }

		[MessagePart("timestamp", Encoder = typeof(TimestampEncoder))]
		internal DateTime CreationDateUtc { get; set; }

		[MessagePart("sig")]
		private string Signature { get; set; }

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnSending() {
			// Encrypt the authorizing username so as to not expose unintended private user data
			// to the client or any eavesdropping third party.
			if (this.User != null) {
				// TODO: code here
			}

			this.Signature = CalculateSignature();
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			// Verify that the verification code was issued by this authorization server.
			ErrorUtilities.VerifyProtocol(string.Equals(this.Signature, this.CalculateSignature(), StringComparison.Ordinal), Protocol.bad_verification_code);

			// Decrypt the authorizing username.
			if (this.User != null) {
				// TODO: code here
			}
		}

		internal static VerificationCode Decode(OAuthWrapAuthorizationServerChannel channel, string value) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Ensures(Contract.Result<VerificationCode>() != null);

			// Construct a new instance of this type.
			VerificationCode self = new VerificationCode(channel);
			var fields = channel.MessageDescriptions.GetAccessor(self);

			// Deserialize into this newly created instance.
			var nvc = HttpUtility.ParseQueryString(value);
			foreach (string key in nvc) {
				fields[key] = nvc[key];
			}

			((IMessageWithEvents)self).OnReceiving();

			return self;
		}

		internal string Encode() {
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			((IMessageWithEvents)this).OnSending();

			var fields = this.Channel.MessageDescriptions.GetAccessor(this);
			return MessagingUtilities.CreateQueryString(fields);
		}

		internal void VerifyCallback(Uri callback) {
			ErrorUtilities.VerifyProtocol(string.Equals(this.CallbackHash, this.CalculateCallbackHash(callback), StringComparison.Ordinal), Protocol.redirect_uri_mismatch);
		}

		private string CalculateCallbackHash(Uri callback) {
			return this.hasher.ComputeHash(callback.AbsoluteUri);
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
			return this.hasher.ComputeHash(fieldsCopy);
		}
	}
}
