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
	using DotNetOpenAuth.OpenId.DiscoveryServices;
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
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreEqual(this.claimedId, se.ClaimedIdentifier);
			Assert.AreSame(this.providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(this.localId, se.ProviderLocalIdentifier);
			CollectionAssert<string>.AreEquivalent(this.v20TypeUris, se.ProviderEndpoint.Capabilities);
			Assert.AreEqual(this.servicePriority, ((IXrdsProviderEndpoint)se).ServicePriority);
		}

		[TestMethod]
		public void CtorImpliedLocalIdentifier() {
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, null, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreEqual(this.claimedId, se.ClaimedIdentifier);
			Assert.AreSame(this.providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(this.claimedId, se.ProviderLocalIdentifier);
			CollectionAssert<string>.AreEquivalent(this.v20TypeUris, se.ProviderEndpoint.Capabilities);
		}

		[TestMethod]
		public void ProtocolDetection() {
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreSame(Protocol.V20, se.ProviderEndpoint.GetProtocol());
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				this.claimedId,
				this.localId,
				new ProviderEndpointDescription(this.providerEndpoint, new[] { Protocol.V20.OPIdentifierServiceTypeURI }),
				this.servicePriority,
				this.uriPriority);
			Assert.AreSame(Protocol.V20, se.ProviderEndpoint.GetProtocol());
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v11TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreSame(Protocol.V11, se.ProviderEndpoint.GetProtocol());
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void ProtocolDetectionWithoutClues() {
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				this.claimedId,
				this.localId,
				new ProviderEndpointDescription(this.providerEndpoint, new[] { Protocol.V20.HtmlDiscoveryLocalIdKey }), // random type URI irrelevant to detection
				this.servicePriority,
				this.uriPriority);
		}

		[TestMethod]
		public void EqualsTests() {
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			IIdentifierDiscoveryResult se2 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), (int?)null, (int?)null);
			Assert.AreEqual(se2, se);
			Assert.AreNotEqual(se, null);
			Assert.AreNotEqual(null, se);

			IIdentifierDiscoveryResult se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(new UriIdentifier(this.claimedId + "a"), this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(new Uri(this.providerEndpoint.AbsoluteUri + "a"), this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId + "a", new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v11TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);

			// make sure that Collection<T>.Contains works as desired.
			var list = new List<IIdentifierDiscoveryResult>();
			list.Add(se);
			Assert.IsTrue(list.Contains(se2));
		}

		[TestMethod]
		public void GetFriendlyIdentifierForDisplay() {
			Uri providerEndpoint = new Uri("http://someprovider");
			Identifier localId = "someuser";
			string[] serviceTypeUris = new string[] {
				Protocol.V20.ClaimedIdentifierServiceTypeURI,
			};
			IIdentifierDiscoveryResult se;

			// strip of protocol, port, query and fragment
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				"http://someprovider.somedomain.com:79/someuser?query#frag",
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("someprovider.somedomain.com/someuser", se.GetFriendlyIdentifierForDisplay());

			// unescape characters
			Uri foreignUri = new Uri("http://server崎/村");
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(foreignUri, localId, new ProviderEndpointDescription(providerEndpoint, serviceTypeUris), null, null);
			Assert.AreEqual("server崎/村", se.GetFriendlyIdentifierForDisplay());

			// restore user supplied identifier to XRIs
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				new XriIdentifier("=Arnott崎村"),
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("=Arnott崎村", se.GetFriendlyIdentifierForDisplay());

			// If UserSuppliedIdentifier is the same as the ClaimedIdentifier, don't display it twice...
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("=!9B72.7DD1.50A9.5CCD", se.GetFriendlyIdentifierForDisplay());
		}

		[TestMethod]
		public void IsTypeUriPresent() {
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.IsTrue(se.ProviderEndpoint.IsTypeUriPresent(Protocol.Default.ClaimedIdentifierServiceTypeURI));
			Assert.IsFalse(se.ProviderEndpoint.IsTypeUriPresent("http://someother"));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void IsTypeUriPresentNull() {
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			se.ProviderEndpoint.IsTypeUriPresent(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void IsTypeUriPresentEmpty() {
			IIdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			se.ProviderEndpoint.IsTypeUriPresent(string.Empty);
		}
	}
}
