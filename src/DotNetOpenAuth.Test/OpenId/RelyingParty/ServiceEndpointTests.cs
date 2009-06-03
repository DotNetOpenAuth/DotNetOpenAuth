//-----------------------------------------------------------------------
// <copyright file="ServiceEndpointTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ServiceEndpointTests : OpenIdTestBase {
		private UriIdentifier claimedId = new UriIdentifier("http://claimedid.justatest.com");
		private XriIdentifier claimedXri = new XriIdentifier("=!9B72.7DD1.50A9.5CCD");
		private XriIdentifier userSuppliedXri = new XriIdentifier("=Arnot");
		private Uri providerEndpoint = new Uri("http://someprovider.com");
		private Identifier localId = "http://localid.someprovider.com";
		private string[] v20TypeUris = { Protocol.V20.ClaimedIdentifierServiceTypeURI };
		private string[] v11TypeUris = { Protocol.V11.ClaimedIdentifierServiceTypeURI };
		private int servicePriority = 10;
		private int uriPriority = 10;

		[TestMethod]
		public void Ctor() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreEqual(this.claimedId, se.ClaimedIdentifier);
			Assert.AreSame(this.providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(this.localId, se.ProviderLocalIdentifier);
			CollectionAssert<string>.AreEquivalent(this.v20TypeUris, se.ProviderSupportedServiceTypeUris);
			Assert.AreEqual(this.servicePriority, ((IXrdsProviderEndpoint)se).ServicePriority);
		}

		[TestMethod]
		public void CtorImpliedLocalIdentifier() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, null, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreEqual(this.claimedId, se.ClaimedIdentifier);
			Assert.AreSame(this.providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(this.claimedId, se.ProviderLocalIdentifier);
			CollectionAssert<string>.AreEquivalent(this.v20TypeUris, se.ProviderSupportedServiceTypeUris);
		}

		[TestMethod]
		public void ProtocolDetection() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreSame(Protocol.V20, se.Protocol);
			se = ServiceEndpoint.CreateForClaimedIdentifier(
				this.claimedId,
				this.localId,
				new ProviderEndpointDescription(this.providerEndpoint, new[] { Protocol.V20.OPIdentifierServiceTypeURI }),
				this.servicePriority,
				this.uriPriority);
			Assert.AreSame(Protocol.V20, se.Protocol);
			se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v11TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreSame(Protocol.V11, se.Protocol);
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void ProtocolDetectionWithoutClues() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(
				this.claimedId,
				this.localId,
				new ProviderEndpointDescription(this.providerEndpoint, new[] { Protocol.V20.HtmlDiscoveryLocalIdKey }), // random type URI irrelevant to detection
				this.servicePriority,
				this.uriPriority);
		}

		[TestMethod]
		public void SerializationWithUri() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb)) {
				se.Serialize(sw);
			}
			using (StringReader sr = new StringReader(sb.ToString())) {
				ServiceEndpoint se2 = ServiceEndpoint.Deserialize(sr);
				Assert.AreEqual(se, se2);
				Assert.AreEqual(se.Protocol.Version, se2.Protocol.Version, "Particularly interested in this, since type URIs are not serialized but version info is.");
				Assert.AreEqual(se.UserSuppliedIdentifier, se2.UserSuppliedIdentifier);
				Assert.AreEqual(se.FriendlyIdentifierForDisplay, se2.FriendlyIdentifierForDisplay);
			}
		}

		[TestMethod]
		public void SerializationWithXri() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb)) {
				se.Serialize(sw);
			}
			using (StringReader sr = new StringReader(sb.ToString())) {
				ServiceEndpoint se2 = ServiceEndpoint.Deserialize(sr);
				Assert.AreEqual(se, se2);
				Assert.AreEqual(se.Protocol.Version, se2.Protocol.Version, "Particularly interested in this, since type URIs are not serialized but version info is.");
				Assert.AreEqual(se.UserSuppliedIdentifier, se2.UserSuppliedIdentifier);
				Assert.AreEqual(se.FriendlyIdentifierForDisplay, se2.FriendlyIdentifierForDisplay);
			}
		}

		[TestMethod]
		public void EqualsTests() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			ServiceEndpoint se2 = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), (int?)null, (int?)null);
			Assert.AreEqual(se2, se);
			Assert.AreNotEqual(se, null);
			Assert.AreNotEqual(null, se);

			ServiceEndpoint se3 = ServiceEndpoint.CreateForClaimedIdentifier(new UriIdentifier(this.claimedId + "a"), this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(new Uri(this.providerEndpoint.AbsoluteUri + "a"), this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId + "a", new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v11TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);

			// make sure that Collection<T>.Contains works as desired.
			List<ServiceEndpoint> list = new List<ServiceEndpoint>();
			list.Add(se);
			Assert.IsTrue(list.Contains(se2));
		}

		[TestMethod]
		public void FriendlyIdentifierForDisplay() {
			Uri providerEndpoint = new Uri("http://someprovider");
			Identifier localId = "someuser";
			string[] serviceTypeUris = new string[] {
				Protocol.V20.ClaimedIdentifierServiceTypeURI,
			};
			ServiceEndpoint se;

			// strip of protocol and fragment
			se = ServiceEndpoint.CreateForClaimedIdentifier(
				"http://someprovider.somedomain.com:79/someuser#frag",
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("someprovider.somedomain.com:79/someuser", se.FriendlyIdentifierForDisplay);

			// unescape characters
			Uri foreignUri = new Uri("http://server崎/村");
			se = ServiceEndpoint.CreateForClaimedIdentifier(foreignUri, localId, new ProviderEndpointDescription(providerEndpoint, serviceTypeUris), null, null);
			Assert.AreEqual("server崎/村", se.FriendlyIdentifierForDisplay);

			// restore user supplied identifier to XRIs
			se = ServiceEndpoint.CreateForClaimedIdentifier(
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				new XriIdentifier("=Arnott崎村"),
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("=Arnott崎村", se.FriendlyIdentifierForDisplay);

			// If UserSuppliedIdentifier is the same as the ClaimedIdentifier, don't display it twice...
			se = ServiceEndpoint.CreateForClaimedIdentifier(
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("=!9B72.7DD1.50A9.5CCD", se.FriendlyIdentifierForDisplay);
		}

		[TestMethod]
		public void IsTypeUriPresent() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.IsTrue(se.IsTypeUriPresent(Protocol.Default.ClaimedIdentifierServiceTypeURI));
			Assert.IsFalse(se.IsTypeUriPresent("http://someother"));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void IsTypeUriPresentNull() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			se.IsTypeUriPresent(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void IsTypeUriPresentEmpty() {
			ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			se.IsTypeUriPresent(string.Empty);
		}
	}
}
