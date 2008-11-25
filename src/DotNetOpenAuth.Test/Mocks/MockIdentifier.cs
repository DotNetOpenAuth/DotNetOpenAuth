//-----------------------------------------------------------------------
// <copyright file="MockIdentifier.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Performs similar to an ordinary <see cref="Identifier"/>, but when called upon
	/// to perform discovery, it returns a preset list of sevice endpoints to avoid
	/// having a dependency on a hosted web site to actually perform discovery on.
	/// </summary>
	class MockIdentifier : Identifier {
		IEnumerable<ServiceEndpoint> endpoints;
		MockHttpRequest mockHttpRequest;
		Identifier wrappedIdentifier;

		public MockIdentifier(Identifier wrappedIdentifier, MockHttpRequest mockHttpRequest, IEnumerable<ServiceEndpoint> endpoints)
			: base(false) {
			ErrorUtilities.VerifyArgumentNotNull(wrappedIdentifier, "wrappedIdentifier");
			ErrorUtilities.VerifyArgumentNotNull(mockHttpRequest, "mockHttpRequest");
			ErrorUtilities.VerifyArgumentNotNull(endpoints, "endpoints");

			this.wrappedIdentifier = wrappedIdentifier;
			this.endpoints = endpoints;
			this.mockHttpRequest = mockHttpRequest;

			// Register a mock HTTP response to enable discovery of this identifier within the RP
			// without having to host an ASP.NET site within the test.
			mockHttpRequest.RegisterMockXrdsResponse(new Uri(wrappedIdentifier.ToString()), endpoints);
		}

		internal override IEnumerable<ServiceEndpoint> Discover(IDirectSslWebRequestHandler requestHandler) {
			return endpoints;
		}

		internal override Identifier TrimFragment() {
			return this;
		}

		internal override bool TryRequireSsl(out Identifier secureIdentifier) {
			// We take special care to make our wrapped identifier secure, but still
			// return a mocked (secure) identifier.
			Identifier secureWrappedIdentifier;
			bool result = wrappedIdentifier.TryRequireSsl(out secureWrappedIdentifier);
			secureIdentifier = new MockIdentifier(secureWrappedIdentifier, this.mockHttpRequest, this.endpoints);
			return result;
		}

		public override string ToString() {
			return wrappedIdentifier.ToString();
		}

		public override bool Equals(object obj) {
			return wrappedIdentifier.Equals(obj);
		}

		public override int GetHashCode() {
			return wrappedIdentifier.GetHashCode();
		}
	}
}
