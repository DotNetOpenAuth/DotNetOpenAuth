//-----------------------------------------------------------------------
// <copyright file="OAuthHttpMethodBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Sets the HTTP Method property on a signed message before the signing module gets to it.
	/// </summary>
	internal class OAuthHttpMethodBindingElement : IChannelBindingElement {
		#region IChannelBindingElement Members

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		public MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False otherwise.
		/// </returns>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			var oauthMessage = message as ITamperResistantOAuthMessage;

			if (oauthMessage != null) {
				HttpDeliveryMethods transmissionMethod = oauthMessage.HttpMethods;
				if ((transmissionMethod & HttpDeliveryMethods.PostRequest) != 0) {
					oauthMessage.HttpMethod = "POST";
				} else if ((transmissionMethod & HttpDeliveryMethods.GetRequest) != 0) {
					oauthMessage.HttpMethod = "GET";
				} else {
					return false;
				}

				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False if the operation did not apply to this message.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			return false;
		}

		#endregion
	}
}
