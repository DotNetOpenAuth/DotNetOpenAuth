using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Test.Mocks {
	/// <summary>
	/// Performs similar to an ordinary <see cref="Identifier"/>, but when called upon
	/// to perform discovery, it returns a preset list of sevice endpoints to avoid
	/// having a dependency on a hosted web site to actually perform discovery on.
	/// </summary>
	class MockIdentifier : Identifier {
		IEnumerable<ServiceEndpoint> endpoints;
		Identifier wrappedIdentifier;

		public MockIdentifier(Identifier wrappedIdentifier, IEnumerable<ServiceEndpoint> endpoints) {
			if (wrappedIdentifier == null) throw new ArgumentNullException("wrappedIdentifier");
			if (endpoints == null) throw new ArgumentNullException("endpoints");
			this.wrappedIdentifier = wrappedIdentifier;
			this.endpoints = endpoints;
		}

		internal override IEnumerable<ServiceEndpoint> Discover() {
			return endpoints;
		}

		internal override Identifier TrimFragment() {
			return this;
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
