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

	internal class AutoResponsiveRequest : Request {
		private readonly IProtocolMessage response;

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
