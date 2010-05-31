//-----------------------------------------------------------------------
// <copyright file="RefreshAccessTokenBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.Messages;

	internal class RefreshAccessTokenBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
		/// </remarks>
		public override MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		public override MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			return null;
		}

		public override MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			var request = message as RefreshAccessTokenRequest;
			if (request != null) {
				// Decode and validate the refresh token
				//request.RefreshToken

				// Fill in the authorized access scope from the refresh token and fill in the property
				// on the message so that others can read it later.
				//request.Scope =

				return MessageProtections.None;
			}

			return null;
		}
	}
}
