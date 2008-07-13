using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class ServiceEndpointTests {
		Identifier claimedId = "http://claimedid.justatest.com";
		Uri providerEndpoint = new Uri("http://someprovider.com");
		Identifier localId = "http://localid.someprovider.com";
		string[] v20TypeUris = { Protocol.v20.ClaimedIdentifierServiceTypeURI };
		string[] v11TypeUris = { Protocol.v11.ClaimedIdentifierServiceTypeURI };
		int priority = 10;

		[Test]
		public void Ctor() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, priority);
			Assert.AreSame(claimedId, se.ClaimedIdentifier);
			Assert.AreSame(providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(localId, se.ProviderLocalIdentifier);
			Assert.AreSame(v20TypeUris, se.ProviderSupportedServiceTypeUris);
			Assert.AreEqual(priority, ((IXrdsProviderEndpoint)se).Priority);
		}

		[Test]
		public void CtorImpliedLocalIdentifier() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, null, v20TypeUris, priority);
			Assert.AreSame(claimedId, se.ClaimedIdentifier);
			Assert.AreSame(providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(claimedId, se.ProviderLocalIdentifier);
			Assert.AreSame(v20TypeUris, se.ProviderSupportedServiceTypeUris);
		}

		[Test]
		public void ProtocolDetection() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, priority);
			Assert.AreSame(Protocol.v20, se.Protocol);
			se = new ServiceEndpoint(claimedId, providerEndpoint, localId,
				new[] { Protocol.v20.OPIdentifierServiceTypeURI }, priority);
			Assert.AreSame(Protocol.v20, se.Protocol);
			se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v11TypeUris, priority);
			Assert.AreSame(Protocol.v11, se.Protocol);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ProtocolDetectionWithoutClues() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId,
				new[] { Protocol.v20.HtmlDiscoveryLocalIdKey }, priority); // random type URI irrelevant to detection
			Protocol p = se.Protocol;
		}

		[Test]
		public void Serialization() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, priority);
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb)) {
				se.Serialize(sw);
			}
			using (StringReader sr = new StringReader(sb.ToString())) {
				ServiceEndpoint se2 = ServiceEndpoint.Deserialize(sr);
				Assert.AreEqual(se, se2);
				Assert.AreEqual(se.Protocol.Version, se2.Protocol.Version, "Particularly interested in this, since type URIs are not serialized but version info is.");
			}
		}

		[Test]
		public void EqualsTests() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, priority);
			ServiceEndpoint se2 = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, priority);
			Assert.AreEqual(se2, se);
			Assert.AreNotEqual(se, null);
			Assert.AreNotEqual(null, se);

			ServiceEndpoint se3 = new ServiceEndpoint(claimedId + "a", providerEndpoint, localId, v20TypeUris, priority);
			Assert.AreNotEqual(se, se3);
			se3 = new ServiceEndpoint(claimedId, new Uri(providerEndpoint.AbsoluteUri + "a"), localId, v20TypeUris, priority);
			Assert.AreNotEqual(se, se3);
			se3 = new ServiceEndpoint(claimedId, providerEndpoint, localId + "a", v20TypeUris, priority);
			Assert.AreNotEqual(se, se3);
			se3 = new ServiceEndpoint(claimedId, providerEndpoint, localId, v11TypeUris, priority);
			Assert.AreNotEqual(se, se3);
			se3 = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, priority + 1);
			Assert.AreNotEqual(se, se3);

			// make sure that Collection<T>.Contains works as desired.
			List<ServiceEndpoint> list = new List<ServiceEndpoint>();
			list.Add(se);
			Assert.IsTrue(list.Contains(se2));
		}
	}
}
