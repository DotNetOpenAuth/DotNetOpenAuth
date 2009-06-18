//-----------------------------------------------------------------------
// <copyright file="AnonymousRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Provides access to a host Provider to read an incoming extension-only checkid request message,
	/// and supply extension responses or a cancellation message to the RP.
	/// </summary>
	[Serializable]
	internal class AnonymousRequest : HostProcessedRequest, IAnonymousRequest {
		/// <summary>
		/// The extension-response message to send, if the host site chooses to send it.
		/// </summary>
		private readonly IndirectSignedResponse positiveResponse;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnonymousRequest"/> class.
		/// </summary>
		/// <param name="provider">The provider that received the request.</param>
		/// <param name="request">The incoming authentication request message.</param>
		internal AnonymousRequest(OpenIdProvider provider, SignedResponseRequest request)
			: base(provider, request) {
			Contract.Requires(provider != null);
			Contract.Requires(!(request is CheckIdRequest), "Instantiate " + typeof(AuthenticationRequest).Name + " to handle this kind of message.");
			ErrorUtilities.VerifyInternal(!(request is CheckIdRequest), "Instantiate {0} to handle this kind of message.", typeof(AuthenticationRequest).Name);

			this.positiveResponse = new IndirectSignedResponse(request);
		}

		#region IAnonymousRequest Members

		/// <summary>
		/// Gets or sets a value indicating whether the user approved sending any data to the relying party.
		/// </summary>
		/// <value><c>true</c> if approved; otherwise, <c>false</c>.</value>
		public bool? IsApproved { get; set; }

		#endregion

		#region Request members

		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		public override bool IsResponseReady {
			get { return this.IsApproved.HasValue; }
		}

		/// <summary>
		/// Gets the response message, once <see cref="IsResponseReady"/> is <c>true</c>.
		/// </summary>
		protected override IProtocolMessage ResponseMessage {
			get {
				if (this.IsApproved.HasValue) {
					return this.IsApproved.Value ? (IProtocolMessage)this.positiveResponse : this.NegativeResponse;
				} else {
					return null;
				}
			}
		}

		#endregion
	}
}
