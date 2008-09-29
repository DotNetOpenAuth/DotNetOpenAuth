//-----------------------------------------------------------------------
// <copyright file="OAuthHttpMethodBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	internal class OAuthHttpMethodBindingElement : IChannelBindingElement {

		#region IChannelBindingElement Members

		public MessageProtection Protection {
			get { return MessageProtection.None; }
		}

		public bool PrepareMessageForSending(IProtocolMessage message) {
			var oauthMessage = message as ITamperResistantOAuthMessage;

			if (oauthMessage != null) {
				HttpDeliveryMethod transmissionMethod = oauthMessage.HttpMethods;
				if ((transmissionMethod & HttpDeliveryMethod.AuthorizationHeaderRequest) != 0) {
					oauthMessage.HttpMethod = "GET";
				} else if ((transmissionMethod & HttpDeliveryMethod.PostRequest) != 0) {
					oauthMessage.HttpMethod = "POST";
				} else if ((transmissionMethod & HttpDeliveryMethod.GetRequest) != 0) {
					oauthMessage.HttpMethod = "GET";
				} else {
					return false;
				}

				return true;
			} else {
				return false;
			}
		}

		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			return false;
		}

		#endregion
	}
}
