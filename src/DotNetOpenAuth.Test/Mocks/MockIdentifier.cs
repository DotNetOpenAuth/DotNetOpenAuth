//-----------------------------------------------------------------------
// <copyright file="MockIdentifier.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Performs similar to an ordinary <see cref="Identifier"/>, but when called upon
	/// to perform discovery, it returns a preset list of sevice endpoints to avoid
	/// having a dependency on a hosted web site to actually perform discovery on.
	/// </summary>
	internal class MockIdentifier : Identifier {
		private IEnumerable<IdentifierDiscoveryResult> endpoints;

		private MockHttpRequest mockHttpRequest;

		private Identifier wrappedIdentifier;

		public MockIdentifier(Identifier wrappedIdentifier, MockHttpRequest mockHttpRequest, IEnumerable<IdentifierDiscoveryResult> endpoints)
			: base(wrappedIdentifier.OriginalString, false) {
			Requires.NotNull(wrappedIdentifier, "wrappedIdentifier");
			Requires.NotNull(mockHttpRequest, "mockHttpRequest");
			Requires.NotNull(endpoints, "endpoints");

			this.wrappedIdentifier = wrappedIdentifier;
			this.endpoints = endpoints;
			this.mockHttpRequest = mockHttpRequest;

			// Register a mock HTTP response to enable discovery of this identifier within the RP
			// without having to host an ASP.NET site within the test.
			mockHttpRequest.RegisterMockXrdsResponse(new Uri(wrappedIdentifier.ToString()), endpoints);
		}

		internal IEnumerable<IdentifierDiscoveryResult> DiscoveryEndpoints {
			get { return this.endpoints; }
		}

		public override string ToString() {
			return this.wrappedIdentifier.ToString();
		}

		public override bool Equals(object obj) {
			return this.wrappedIdentifier.Equals(obj);
		}

		public override int GetHashCode() {
			return this.wrappedIdentifier.GetHashCode();
		}

		internal override Identifier TrimFragment() {
			return this;
		}

		internal override bool TryRequireSsl(out Identifier secureIdentifier) {
			// We take special care to make our wrapped identifier secure, but still
			// return a mocked (secure) identifier.
			Identifier secureWrappedIdentifier;
			bool result = this.wrappedIdentifier.TryRequireSsl(out secureWrappedIdentifier);
			secureIdentifier = new MockIdentifier(secureWrappedIdentifier, this.mockHttpRequest, this.endpoints);
			return result;
		}
	}
}
