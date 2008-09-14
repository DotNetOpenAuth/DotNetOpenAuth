//-----------------------------------------------------------------------
// <copyright file="StandardExpirationBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Bindings {
	using System;

	/// <summary>
	/// A message expiration enforcing binding element that supports messages
	/// implementing the <see cref="IExpiringProtocolMessage"/> interface.
	/// </summary>
	internal class StandardExpirationBindingElement : IChannelBindingElement {
		/// <summary>
		/// The default maximum message age to use if the default constructor is called.
		/// </summary>
		internal static readonly TimeSpan DefaultMaximumMessageAge = TimeSpan.FromMinutes(13);

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardExpirationBindingElement"/> class
		/// with a default maximum message lifetime of 13 minutes.
		/// </summary>
		internal StandardExpirationBindingElement()
			: this(DefaultMaximumMessageAge) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardExpirationBindingElement"/> class.
		/// </summary>
		/// <param name="maximumAge">
		/// <para>The maximum age a message implementing the 
		/// <see cref="IExpiringProtocolMessage"/> interface can be before
		/// being discarded as too old.</para>
		/// <para>This time limit should take into account expected time skew for servers
		/// across the Internet.  For example, if a server could conceivably have its
		/// clock d = 5 minutes off UTC time, then any two servers could have
		/// their clocks disagree by as much as 2*d = 10 minutes.
		/// If a message should live for at least t = 3 minutes, 
		/// this property should be set to (2*d + t) = 13 minutes.</para>
		/// </param>
		internal StandardExpirationBindingElement(TimeSpan maximumAge) {
			this.MaximumMessageAge = maximumAge;
		}

		#region IChannelBindingElement Properties

		/// <summary>
		/// Gets the protection offered by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtection.Expiration"/></value>
		MessageProtection IChannelBindingElement.Protection {
			get { return MessageProtection.Expiration; }
		}

		#endregion

		/// <summary>
		/// Gets the maximum age a message implementing the 
		/// <see cref="IExpiringProtocolMessage"/> interface can be before
		/// being discarded as too old.
		/// </summary>
		protected internal TimeSpan MaximumMessageAge {
			get;
			private set;
		}

		#region IChannelBindingElement Methods

		/// <summary>
		/// Sets the timestamp on an outgoing message.
		/// </summary>
		/// <param name="message">The outgoing message.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False otherwise.
		/// </returns>
		bool IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			IExpiringProtocolMessage expiringMessage = message as IExpiringProtocolMessage;
			if (expiringMessage != null) {
				expiringMessage.UtcCreationDate = DateTime.UtcNow;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Reads the timestamp on a message and throws an exception if the message is too old.
		/// </summary>
		/// <param name="message">The incoming message.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False if the operation did not apply to this message.
		/// </returns>
		/// <exception cref="ExpiredMessageException">Thrown if the given message has already expired.</exception>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		bool IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			IExpiringProtocolMessage expiringMessage = message as IExpiringProtocolMessage;
			if (expiringMessage != null) {
				// Yes the UtcCreationDate is supposed to always be in UTC already,
				// but just in case a given message failed to guarantee that, we do it here.
				DateTime expirationDate = expiringMessage.UtcCreationDate.ToUniversalTime() + this.MaximumMessageAge;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, expiringMessage);
				}

				return true;
			}

			return false;
		}

		#endregion
	}
}
