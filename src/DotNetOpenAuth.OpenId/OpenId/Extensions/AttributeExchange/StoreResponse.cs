//-----------------------------------------------------------------------
// <copyright file="StoreResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The Attribute Exchange Store message, response leg.
	/// </summary>
	[Serializable]
	public sealed class StoreResponse : ExtensionBase {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUri && !isProviderRole) {
				string mode;
				if (data.TryGetValue("mode", out mode) && (mode == SuccessMode || mode == FailureMode)) {
					return new StoreResponse();
				}
			}

			return null;
		};

		/// <summary>
		/// The value of the mode parameter used to express a successful store operation.
		/// </summary>
		private const string SuccessMode = "store_response_success";

		/// <summary>
		/// The value of the mode parameter used to express a store operation failure.
		/// </summary>
		private const string FailureMode = "store_response_failure";

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreResponse"/> class
		/// to represent a successful store operation.
		/// </summary>
		public StoreResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
			this.Succeeded = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreResponse"/> class
		/// to represent a failed store operation.
		/// </summary>
		/// <param name="failureReason">The reason for failure.</param>
		public StoreResponse(string failureReason)
			: this() {
			this.Succeeded = false;
			this.FailureReason = failureReason;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the storage request succeeded.
		/// </summary>
		/// <value>Defaults to <c>true</c>.</value>
		public bool Succeeded {
			get { return this.Mode == SuccessMode; }
			set { this.Mode = value ? SuccessMode : FailureMode; }
		}

		/// <summary>
		/// Gets or sets the reason for the failure, if applicable.
		/// </summary>
		[MessagePart("error", IsRequired = false)]
		public string FailureReason { get; set; }

		/// <summary>
		/// Gets a value indicating whether this extension is signed by the Provider.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is signed by the Provider; otherwise, <c>false</c>.
		/// </value>
		public bool IsSignedByProvider {
			get { return this.IsSignedByRemoteParty; }
		}

		/// <summary>
		/// Gets or sets the mode argument.
		/// </summary>
		/// <value>One of 'store_response_success' or 'store_response_failure'.</value>
		[MessagePart("mode", IsRequired = true)]
		private string Mode { get; set; }

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			var other = obj as StoreResponse;
			if (other == null) {
				return false;
			}

			if (this.Version != other.Version) {
				return false;
			}

			if (this.Succeeded != other.Succeeded) {
				return false;
			}

			if (this.FailureReason != other.FailureReason) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = this.Version.GetHashCode();
				hashCode += this.Succeeded ? 0 : 1;
				if (this.FailureReason != null) {
					hashCode += this.FailureReason.GetHashCode();
				}

				return hashCode;
			}
		}

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			ErrorUtilities.VerifyProtocol(
				this.Mode == SuccessMode || this.Mode == FailureMode,
				MessagingStrings.UnexpectedMessagePartValue,
				"mode",
				this.Mode);
		}
	}
}
