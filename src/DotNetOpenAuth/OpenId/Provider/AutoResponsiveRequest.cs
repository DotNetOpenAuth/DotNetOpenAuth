//-----------------------------------------------------------------------
// <copyright file="AutoResponsiveRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Handles messages coming into an OpenID Provider for which the entire
	/// response message can be automatically determined without help from
	/// the hosting web site.
	/// </summary>
	internal class AutoResponsiveRequest : Request {
		/// <summary>
		/// The response message to send.
		/// </summary>
		private readonly IProtocolMessage response;

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoResponsiveRequest"/> class.
		/// </summary>
		/// <param name="provider">The provider that received the request message.</param>
		/// <param name="request">The request message.</param>
		/// <param name="response">The response that is ready for transmittal.</param>
		internal AutoResponsiveRequest(OpenIdProvider provider, IDirectedProtocolMessage request, IProtocolMessage response)
			: base(provider, request) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			this.response = response;
		}

		public override bool IsResponseReady {
			get { return true; }
		}

		protected override IProtocolMessage ResponseMessage {
			get { return this.response; }
		}
	}
}
