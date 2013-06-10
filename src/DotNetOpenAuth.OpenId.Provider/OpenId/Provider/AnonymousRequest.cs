//-----------------------------------------------------------------------
// <copyright file="AnonymousRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

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
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Contracts.__ContractsRuntime.Requires<System.ArgumentException>(System.Boolean,System.String,System.String)", Justification = "Code contracts"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AuthenticationRequest", Justification = "Type name"), SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Code contracts require it.")]
		internal AnonymousRequest(OpenIdProvider provider, SignedResponseRequest request)
			: base(provider, request) {
			Requires.NotNull(provider, "provider");
			Requires.That(!(request is CheckIdRequest), "request", "request cannot be CheckIdRequest");

			this.positiveResponse = new IndirectSignedResponse(request);
		}

		#region HostProcessedRequest members

		/// <summary>
		/// Gets or sets the provider endpoint.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// </value>
		public override Uri ProviderEndpoint {
			get { return this.positiveResponse.ProviderEndpoint; }
			set { this.positiveResponse.ProviderEndpoint = value; }
		}

		#endregion

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
		/// Gets the response message, once <see cref="IsResponseReady" /> is <c>true</c>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response message.</returns>
		protected override async Task<IProtocolMessage> GetResponseMessageAsync(CancellationToken cancellationToken) {
				if (this.IsApproved.HasValue) {
					return this.IsApproved.Value ? (IProtocolMessage)this.positiveResponse : await this.GetNegativeResponseAsync();
				} else {
					return null;
				}
		}

		#endregion
	}
}
