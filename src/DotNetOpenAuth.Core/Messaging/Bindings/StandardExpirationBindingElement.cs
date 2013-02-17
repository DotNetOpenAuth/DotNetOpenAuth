//-----------------------------------------------------------------------
// <copyright file="StandardExpirationBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Configuration;

	/// <summary>
	/// A message expiration enforcing binding element that supports messages
	/// implementing the <see cref="IExpiringProtocolMessage"/> interface.
	/// </summary>
	internal class StandardExpirationBindingElement : IChannelBindingElement {
		/// <summary>
		/// A reusable pre-completed task that may be returned multiple times to reduce GC pressure.
		/// </summary>
		private static readonly Task<MessageProtections?> NullTask = Task.FromResult<MessageProtections?>(null);

		/// <summary>
		/// A reusable pre-completed task that may be returned multiple times to reduce GC pressure.
		/// </summary>
		private static readonly Task<MessageProtections?> CompletedExpirationTask = Task.FromResult<MessageProtections?>(MessageProtections.Expiration);

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardExpirationBindingElement"/> class.
		/// </summary>
		internal StandardExpirationBindingElement() {
		}

		#region IChannelBindingElement Properties

		/// <summary>
		/// Gets the protection offered by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.Expiration"/></value>
		MessageProtections IChannelBindingElement.Protection {
			get { return MessageProtections.Expiration; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		#endregion

		/// <summary>
		/// Gets the maximum age a message implementing the 
		/// <see cref="IExpiringProtocolMessage"/> interface can be before
		/// being discarded as too old.
		/// </summary>
		protected internal static TimeSpan MaximumMessageAge {
			get { return Configuration.DotNetOpenAuthSection.Messaging.MaximumMessageLifetime; }
		}

		#region IChannelBindingElement Methods

		/// <summary>
		/// Sets the timestamp on an outgoing message.
		/// </summary>
		/// <param name="message">The outgoing message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection" /> properties where applicable.
		/// </remarks>
		public Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			IExpiringProtocolMessage expiringMessage = message as IExpiringProtocolMessage;
			if (expiringMessage != null) {
				expiringMessage.UtcCreationDate = DateTime.UtcNow;
				return CompletedExpirationTask;
			}

			return NullTask;
		}

		/// <summary>
		/// Reads the timestamp on a message and throws an exception if the message is too old.
		/// </summary>
		/// <param name="message">The incoming message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="ExpiredMessageException">Thrown if the given message has already expired.</exception>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		public Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken) {
			IExpiringProtocolMessage expiringMessage = message as IExpiringProtocolMessage;
			if (expiringMessage != null) {
				// Yes the UtcCreationDate is supposed to always be in UTC already,
				// but just in case a given message failed to guarantee that, we do it here.
				DateTime creationDate = expiringMessage.UtcCreationDate.ToUniversalTimeSafe();
				DateTime expirationDate = creationDate + MaximumMessageAge;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, expiringMessage);
				}

				// Mitigate HMAC attacks (just guessing the signature until they get it) by 
				// disallowing post-dated messages.
				ErrorUtilities.VerifyProtocol(
					creationDate <= DateTime.UtcNow + DotNetOpenAuthSection.Messaging.MaximumClockSkew,
					MessagingStrings.MessageTimestampInFuture,
					creationDate);

				return CompletedExpirationTask;
			}

			return NullTask;
		}

		#endregion
	}
}
