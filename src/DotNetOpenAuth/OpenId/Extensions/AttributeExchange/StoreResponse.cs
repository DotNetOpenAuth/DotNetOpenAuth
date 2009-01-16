//-----------------------------------------------------------------------
// <copyright file="StoreResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Globalization;
	using System.Diagnostics;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The Attribute Exchange Store message, response leg.
	/// </summary>
	public sealed class StoreResponse : ExtensionBase {
		/// <summary>
		/// The value of the mode parameter used to express a successful store operation.
		/// </summary>
		private const string SuccessMode = "store_response_success";

		/// <summary>
		/// The value of the mode parameter used to express a store operation failure.
		/// </summary>
		private const string FailureMode = "store_response_failure";

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreResponse"/> class.
		/// </summary>
		public StoreResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets or sets a value indicating whether the storage request succeeded.
		/// </summary>
		public bool Succeeded {
			get { return this.mode == SuccessMode; }
			set { this.mode = value ? SuccessMode : FailureMode; }
		}

		/// <summary>
		/// Gets or sets the reason for the failure, if applicable.
		/// </summary>
		[MessagePart("error", IsRequired = false)]
		public string FailureReason { get; set; }

		/// <summary>
		/// Gets or sets the mode argument.
		/// </summary>
		/// <value>One of 'store_response_success' or 'store_response_failure'.</value>
		[MessagePart("mode", IsRequired = true)]
		private string mode { get; set; }

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
				this.mode == SuccessMode || this.mode == FailureMode,
				MessagingStrings.UnexpectedMessagePartValue, "mode", this.mode);
		}
	}
}
