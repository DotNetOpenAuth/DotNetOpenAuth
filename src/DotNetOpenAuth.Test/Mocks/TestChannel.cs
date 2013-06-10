//-----------------------------------------------------------------------
// <copyright file="TestChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;

	internal class TestChannel : Channel {
		internal TestChannel(IHostFactories hostFactories = null)
			: this(new TestMessageFactory(), new IChannelBindingElement[0], hostFactories ?? new DefaultOpenIdHostFactories()) {
		}

		internal TestChannel(MessageDescriptionCollection messageDescriptions, IHostFactories hostFactories = null)
			: this(hostFactories) {
			this.MessageDescriptions = messageDescriptions;
		}

		internal TestChannel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements, IHostFactories hostFactories)
			: base(messageTypeProvider, bindingElements, hostFactories) {
		}

		/// <summary>
		/// Deserializes a dictionary of values into a message.
		/// </summary>
		/// <param name="fields">The dictionary of values that were read from an HTTP request or response.</param>
		/// <param name="recipient">Information about where the message was directed.  Null for direct response messages.</param>
		/// <returns>
		/// The deserialized message, or null if no message could be recognized in the provided data.
		/// </returns>
		/// <remarks>
		/// This internal method exposes Receive directly to unit tests for easier deserialization of custom (possibly malformed) messages.
		/// </remarks>
		internal new IProtocolMessage Receive(Dictionary<string, string> fields, MessageReceivingEndpoint recipient) {
			return base.Receive(fields, recipient);
		}

		protected override Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
			throw new NotImplementedException("ReadFromResponseInternal");
		}

		protected override HttpRequestMessage CreateHttpRequest(IDirectedProtocolMessage request) {
			throw new NotImplementedException("CreateHttpRequest");
		}

		protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response) {
			throw new NotImplementedException("SendDirectMessageResponse");
		}
	}
}
