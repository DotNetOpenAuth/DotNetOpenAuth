//-----------------------------------------------------------------------
// <copyright file="StandardMessageExpirationBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;

	/// <summary>
	/// A message expiration enforcing binding element that supports messages
	/// implementing the <see cref="IExpiringProtocolMessage"/> interface.
	/// </summary>
	internal class StandardMessageExpirationBindingElement : IChannelBindingElement {
		/// <summary>
		/// The default maximum message age to use if the default constructor is called.
		/// </summary>
		internal static readonly TimeSpan DefaultMaximumMessageAge = TimeSpan.FromMinutes(13);

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardMessageExpirationBindingElement"/> class
		/// with a default maximum message lifetime of 13 minutes.
		/// </summary>
		internal StandardMessageExpirationBindingElement()
			: this(DefaultMaximumMessageAge) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardMessageExpirationBindingElement"/> class.
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
		internal StandardMessageExpirationBindingElement(TimeSpan maximumAge) {
			this.MaximumMessageAge = maximumAge;
		}

		#region IChannelBindingElement Properties

		/// <summary>
		/// Gets the protection offered by this binding element.
		/// </summary>
		/// <value><see cref="ChannelProtection.Expiration"/></value>
		ChannelProtection IChannelBindingElement.Protection {
			get { return ChannelProtection.Expiration; }
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
		void IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			IExpiringProtocolMessage expiringMessage = message as IExpiringProtocolMessage;
			if (expiringMessage != null) {
				expiringMessage.UtcCreationDate = DateTime.UtcNow;
			}
		}

		/// <summary>
		/// Reads the timestamp on a message and throws an exception if the message is too old.
		/// </summary>
		/// <param name="message">The incoming message.</param>
		/// <exception cref="ExpiredMessageException">Thrown if the given message has already expired.</exception>
		void IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			IExpiringProtocolMessage expiringMessage = message as IExpiringProtocolMessage;
			if (expiringMessage != null) {
				// Yes the UtcCreationDate is supposed to always be in UTC already,
				// but just in case a given message failed to guarantee that, we do it here.
				DateTime expirationDate = expiringMessage.UtcCreationDate.ToUniversalTime() + this.MaximumMessageAge;
				if (expirationDate < DateTime.UtcNow) {
					throw new ExpiredMessageException(expirationDate, expiringMessage);
				}
			}
		}

		#endregion
	}
}
