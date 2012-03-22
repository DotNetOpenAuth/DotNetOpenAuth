//-----------------------------------------------------------------------
// <copyright file="TestBadChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
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

		internal new IProtocolMessage ReadFromRequest(HttpRequestBase request) {
			return base.ReadFromRequest(request);
		}

		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
			throw new NotImplementedException();
		}

		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			throw new NotImplementedException();
		}
	}
}
