//-----------------------------------------------------------------------
// <copyright file="TestBadChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A Channel derived type that passes null to the protected constructor.
	/// </summary>
	internal class TestBadChannel : Channel {
		internal TestBadChannel(bool badConstructorParam)
			: base(badConstructorParam ? null : new TestMessageFactory()) {
		}

		internal new void Create301RedirectResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields, bool payloadInFragment = false) {
			base.Create301RedirectResponse(message, fields, payloadInFragment);
		}

		internal new void CreateFormPostResponse(IDirectedProtocolMessage message, IDictionary<string, string> fields) {
			base.CreateFormPostResponse(message, fields);
		}

		internal new void PrepareIndirectResponse(IDirectedProtocolMessage message) {
			base.PrepareIndirectResponse(message);
		}

		internal new IProtocolMessage Receive(Dictionary<string, string> fields, MessageReceivingEndpoint recipient) {
			return base.Receive(fields, recipient);
		}

		internal new Task<IDirectedProtocolMessage> ReadFromRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			return base.ReadFromRequestAsync(request, cancellationToken);
		}

		protected override Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response) {
			throw new NotImplementedException();
		}
	}
}
