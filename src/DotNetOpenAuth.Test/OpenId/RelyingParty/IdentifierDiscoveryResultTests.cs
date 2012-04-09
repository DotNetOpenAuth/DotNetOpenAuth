//-----------------------------------------------------------------------
// <copyright file="IdentifierDiscoveryResultTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	using NUnit.Framework;

	[TestFixture]
	public class IdentifierDiscoveryResultTests : OpenIdTestBase {
		private UriIdentifier claimedId = new UriIdentifier("http://claimedid.justatest.com");
		private XriIdentifier claimedXri = new XriIdentifier("=!9B72.7DD1.50A9.5CCD");
		private XriIdentifier userSuppliedXri = new XriIdentifier("=Arnot");
		private Uri providerEndpoint = new Uri("http://someprovider.com");
		private Identifier localId = "http://localid.someprovider.com";
		private string[] v20TypeUris = { Protocol.V20.ClaimedIdentifierServiceTypeURI };
		private string[] v11TypeUris = { Protocol.V11.ClaimedIdentifierServiceTypeURI };
		private int servicePriority = 10;
		private int uriPriority = 10;

		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		[Test]
		public void Ctor() {
			IdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreEqual(this.claimedId, se.ClaimedIdentifier);
			Assert.AreSame(this.providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(this.localId, se.ProviderLocalIdentifier);
			CollectionAssert<string>.AreEquivalent(this.v20TypeUris, se.Capabilities);
			Assert.AreEqual(this.servicePriority, se.ServicePriority);
		}

		[Test]
		public void CtorImpliedLocalIdentifier() {
			IdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, null, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreEqual(this.claimedId, se.ClaimedIdentifier);
			Assert.AreSame(this.providerEndpoint, se.ProviderEndpoint);
			Assert.AreSame(this.claimedId, se.ProviderLocalIdentifier);
			CollectionAssert<string>.AreEquivalent(this.v20TypeUris, se.Capabilities);
		}

		[Test]
		public void ProtocolDetection() {
			IdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreSame(Protocol.V20, se.Protocol);
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				this.claimedId,
				this.localId,
				new ProviderEndpointDescription(this.providerEndpoint, new[] { Protocol.V20.OPIdentifierServiceTypeURI }),
				this.servicePriority,
				this.uriPriority);
			Assert.AreSame(Protocol.V20, se.Protocol);
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v11TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreSame(Protocol.V11, se.Protocol);
		}

		[Test]
		public void EqualsTests() {
			IdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			IdentifierDiscoveryResult se2 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), (int?)null, (int?)null);
			Assert.AreEqual(se2, se);
			Assert.AreNotEqual(se, null);
			Assert.AreNotEqual(null, se);

			IdentifierDiscoveryResult se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(new UriIdentifier(this.claimedId + "a"), this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(new Uri(this.providerEndpoint.AbsoluteUri + "a"), this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId + "a", new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);
			se3 = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedId, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v11TypeUris), this.servicePriority, this.uriPriority);
			Assert.AreNotEqual(se, se3);

			// make sure that Collection<T>.Contains works as desired.
			var list = new List<IdentifierDiscoveryResult>();
			list.Add(se);
			Assert.IsTrue(list.Contains(se2));
		}

		[Test]
		public void GetFriendlyIdentifierForDisplay() {
			Uri providerEndpoint = new Uri("http://someprovider");
			Identifier localId = "someuser";
			string[] serviceTypeUris = new string[] {
				Protocol.V20.ClaimedIdentifierServiceTypeURI,
			};
			IdentifierDiscoveryResult se;

			// strip of protocol, port, query and fragment
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				"http://someprovider.somedomain.com:79/someuser?query#frag",
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("someprovider.somedomain.com/someuser", se.FriendlyIdentifierForDisplay);

			// unescape characters
			Uri foreignUri = new Uri("http://server崎/村");
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(foreignUri, localId, new ProviderEndpointDescription(providerEndpoint, serviceTypeUris), null, null);
			Assert.AreEqual("server崎/村", se.FriendlyIdentifierForDisplay);

			// restore user supplied identifier to XRIs
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				new XriIdentifier("=Arnott崎村"),
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("=Arnott崎村", se.FriendlyIdentifierForDisplay);

			// If UserSuppliedIdentifier is the same as the ClaimedIdentifier, don't display it twice...
			se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				new XriIdentifier("=!9B72.7DD1.50A9.5CCD"),
				localId,
				new ProviderEndpointDescription(providerEndpoint, serviceTypeUris),
				null,
				null);
			Assert.AreEqual("=!9B72.7DD1.50A9.5CCD", se.FriendlyIdentifierForDisplay);
		}

		[Test]
		public void IsTypeUriPresent() {
			IdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			Assert.IsTrue(se.IsTypeUriPresent(Protocol.Default.ClaimedIdentifierServiceTypeURI));
			Assert.IsFalse(se.IsTypeUriPresent("http://someother"));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void IsTypeUriPresentNull() {
			IdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			se.IsTypeUriPresent(null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void IsTypeUriPresentEmpty() {
			IdentifierDiscoveryResult se = IdentifierDiscoveryResult.CreateForClaimedIdentifier(this.claimedXri, this.userSuppliedXri, this.localId, new ProviderEndpointDescription(this.providerEndpoint, this.v20TypeUris), this.servicePriority, this.uriPriority);
			se.IsTypeUriPresent(string.Empty);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void IsExtensionSupportedNullType() {
			var se = IdentifierDiscoveryResult.CreateForProviderIdentifier(OPUri, new ProviderEndpointDescription(OPUri, this.v20TypeUris), null, null);
			se.IsExtensionSupported((Type)null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void IsTypeUriPresentNullString() {
			var se = IdentifierDiscoveryResult.CreateForProviderIdentifier(OPUri, new ProviderEndpointDescription(OPUri, this.v20TypeUris), null, null);
			se.IsTypeUriPresent((string)null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void IsTypeUriPresentEmptyString() {
			var se = IdentifierDiscoveryResult.CreateForProviderIdentifier(OPUri, new ProviderEndpointDescription(OPUri, this.v20TypeUris), null, null);
			se.IsTypeUriPresent(string.Empty);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void IsExtensionSupportedNullExtension() {
			var se = IdentifierDiscoveryResult.CreateForProviderIdentifier(OPUri, new ProviderEndpointDescription(OPUri, this.v20TypeUris), null, null);
			se.IsExtensionSupported((IOpenIdMessageExtension)null);
		}

		[Test]
		public void IsExtensionSupported() {
			var se = IdentifierDiscoveryResult.CreateForProviderIdentifier(OPUri, new ProviderEndpointDescription(OPUri, this.v20TypeUris), null, null);
			Assert.IsFalse(se.IsExtensionSupported<ClaimsRequest>());
			Assert.IsFalse(se.IsExtensionSupported(new ClaimsRequest()));
			Assert.IsFalse(se.IsTypeUriPresent("http://someextension/typeuri"));

			se = IdentifierDiscoveryResult.CreateForProviderIdentifier(
				OPUri,
				new ProviderEndpointDescription(OPUri, new[] { Protocol.V20.ClaimedIdentifierServiceTypeURI, "http://someextension", Constants.TypeUris.Standard }),
				null,
				null);
			Assert.IsTrue(se.IsExtensionSupported<ClaimsRequest>());
			Assert.IsTrue(se.IsExtensionSupported(new ClaimsRequest()));
			Assert.IsTrue(se.IsTypeUriPresent("http://someextension"));
		}
	}
}
