using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using System.Diagnostics;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class ServiceEndpointTests {
		UriIdentifier claimedId = new UriIdentifier("http://claimedid.justatest.com");
		Uri providerEndpoint = new Uri("http://someprovider.com");
		Identifier localId = "http://localid.someprovider.com";
		string[] v20TypeUris = { Protocol.v20.ClaimedIdentifierServiceTypeURI };
		string[] v11TypeUris = { Protocol.v11.ClaimedIdentifierServiceTypeURI };
		int servicePriority = 10;
		int uriPriority = 10;

		[Test]
		public void Ctor() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, servicePriority, uriPriority);
			Assert.AreSame(claimedId, se.ClaimedIdentifier);
			Assert.AreSame(providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(localId, se.ProviderLocalIdentifier);
			Assert.AreSame(v20TypeUris, se.ProviderSupportedServiceTypeUris);
			Assert.AreEqual(servicePriority, ((IXrdsProviderEndpoint)se).ServicePriority);
		}

		[Test]
		public void CtorImpliedLocalIdentifier() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, null, v20TypeUris, servicePriority, uriPriority);
			Assert.AreSame(claimedId, se.ClaimedIdentifier);
			Assert.AreSame(providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(claimedId, se.ProviderLocalIdentifier);
			Assert.AreSame(v20TypeUris, se.ProviderSupportedServiceTypeUris);
		}

		[Test]
		public void ProtocolDetection() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, servicePriority, uriPriority);
			Assert.AreSame(Protocol.v20, se.Protocol);
			se = new ServiceEndpoint(claimedId, providerEndpoint, localId,
				new[] { Protocol.v20.OPIdentifierServiceTypeURI }, servicePriority, uriPriority);
			Assert.AreSame(Protocol.v20, se.Protocol);
			se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v11TypeUris, servicePriority, uriPriority);
			Assert.AreSame(Protocol.v11, se.Protocol);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ProtocolDetectionWithoutClues() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId,
				new[] { Protocol.v20.HtmlDiscoveryLocalIdKey }, servicePriority, uriPriority); // random type URI irrelevant to detection
			Protocol p = se.Protocol;
		}

		[Test]
		public void Serialization() {
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, servicePriority, uriPriority);
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
			ServiceEndpoint se = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, servicePriority, uriPriority);
			ServiceEndpoint se2 = new ServiceEndpoint(claimedId, providerEndpoint, localId, v20TypeUris, (int?)null, (int?)null);
			Assert.AreEqual(se2, se);
			Assert.AreNotEqual(se, null);
			Assert.AreNotEqual(null, se);

			ServiceEndpoint se3 = new ServiceEndpoint(new UriIdentifier(claimedId + "a"), providerEndpoint, localId, v20TypeUris, servicePriority, uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = new ServiceEndpoint(claimedId, new Uri(providerEndpoint.AbsoluteUri + "a"), localId, v20TypeUris, servicePriority, uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = new ServiceEndpoint(claimedId, providerEndpoint, localId + "a", v20TypeUris, servicePriority, uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = new ServiceEndpoint(claimedId, providerEndpoint, localId, v11TypeUris, servicePriority, uriPriority);
			Assert.AreNotEqual(se, se3);

			// make sure that Collection<T>.Contains works as desired.
			List<ServiceEndpoint> list = new List<ServiceEndpoint>();
			list.Add(se);
			Assert.IsTrue(list.Contains(se2));
		}

		[Test]
		public void FriendlyIdentifierForDisplay() {
			Uri providerEndpoint= new Uri("http://someprovider");
			Identifier localId = "someuser";
			string[] serviceTypeUris = new string[0];
			ServiceEndpoint se;

			// strip of protocol and fragment
			se = new ServiceEndpoint(new UriIdentifier("http://someprovider.somedomain.com:79/someuser#frag"),
				providerEndpoint, localId, serviceTypeUris, null, null);
			Assert.AreEqual("someprovider.somedomain.com:79/someuser", se.FriendlyIdentifierForDisplay);

			// unescape characters
			Uri foreignUri = new Uri("http://server崎/村");
			se = new ServiceEndpoint(foreignUri, providerEndpoint, localId, serviceTypeUris, null, null);
			Assert.AreEqual("server崎/村", se.FriendlyIdentifierForDisplay);
			
			// restore user supplied identifier to XRIs
			se = new ServiceEndpoint(new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				new XriIdentifier("=Arnott崎村"), providerEndpoint, localId, serviceTypeUris, null, null);
			Assert.AreEqual("=!9B72.7DD1.50A9.5CCD (=Arnott崎村)", se.FriendlyIdentifierForDisplay);
		}
	}
}
