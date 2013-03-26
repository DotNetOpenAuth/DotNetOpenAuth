//-----------------------------------------------------------------------
// <copyright file="RelyingPartySecurityOptions.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Helps ensure compliance to some properties in the <see cref="RelyingPartySecuritySettings"/>.
	/// </summary>
	internal class RelyingPartySecurityOptions : IChannelBindingElement {
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
		/// The security settings that are active on the relying party.
		/// </summary>
		private RelyingPartySecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartySecurityOptions"/> class.
		/// </summary>
		/// <param name="securitySettings">The security settings.</param>
		internal RelyingPartySecurityOptions(RelyingPartySecuritySettings securitySettings) {
			this.securitySettings = securitySettings;
		}

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel { get; set; }

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
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
			var positiveAssertion = message as PositiveAssertionResponse;
			if (positiveAssertion != null) {
				ErrorUtilities.VerifyProtocol(
					!this.securitySettings.RejectDelegatingIdentifiers ||
					positiveAssertion.LocalIdentifier == positiveAssertion.ClaimedIdentifier,
					OpenIdStrings.DelegatingIdentifiersNotAllowed);

				return NoneTask;
			}

			return NullTask;
		}

		#endregion
	}
}
