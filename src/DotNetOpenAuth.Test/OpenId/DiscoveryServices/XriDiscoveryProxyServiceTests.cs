//-----------------------------------------------------------------------
// <copyright file="XriDiscoveryProxyServiceTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.DiscoveryServices {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class XriDiscoveryProxyServiceTests : OpenIdTestBase {
		[Test]
		public async Task Discover() {
			string xrds = @"<?xml version='1.0' encoding='UTF-8'?>
<XRD version='2.0' xmlns='xri://$xrd*($v*2.0)'>
 <Query>*Arnott</Query>
 <Status ceid='off' cid='verified' code='100'/>
 <Expires>2008-07-14T02:03:24.000Z</Expires>
 <ProviderID>xri://=</ProviderID>
 <LocalID>!9b72.7dd1.50a9.5ccd</LocalID>
 <CanonicalID>=!9B72.7DD1.50A9.5CCD</CanonicalID>

 <Service priority='10'>
  <ProviderID>xri://!!1008</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='default' select='false'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null' select='false'/>
  <URI append='qxri' priority='1'>http://1id.com/contact/</URI>

 </Service>
 <Service priority='10'>
  <ProviderID>xri://!!1008</ProviderID>
  <Type select='true'>xri://+i-service*(+forwarding)*($v*1.0)</Type>
  <Type match='null' select='false'/>
  <URI append='qxri' priority='1'>http://1id.com/</URI>
 </Service>

 <Service priority='10'>
  <ProviderID>xri://!!1008</ProviderID>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <URI append='none' priority='10'>http://1id.com/sso</URI>
 </Service>
</XRD>";
			var mocks = new Dictionary<string, string> {
				{ "https://xri.net/=Arnott?_xrd_r=application/xrd%2Bxml;sep=false", xrds },
				{ "https://xri.net/=!9B72.7DD1.50A9.5CCD?_xrd_r=application/xrd%2Bxml;sep=false", xrds },
			};
			this.RegisterMockXrdsResponses(mocks);

			string expectedCanonicalId = "=!9B72.7DD1.50A9.5CCD";
			IdentifierDiscoveryResult se = await this.VerifyCanonicalIdAsync("=Arnott", expectedCanonicalId);
			Assert.AreEqual(Protocol.V10, Protocol.Lookup(se.Version));
			Assert.AreEqual("http://1id.com/sso", se.ProviderEndpoint.ToString());
			Assert.AreEqual(se.ClaimedIdentifier, se.ProviderLocalIdentifier);
			Assert.AreEqual("=Arnott", se.FriendlyIdentifierForDisplay);
		}

		[Test]
		public async Task DiscoverCommunityInameCanonicalIDs() {
			string llliResponse = @"<?xml version='1.0' encoding='UTF-8'?>
<XRD version='2.0' xmlns='xri://$xrd*($v*2.0)'>
 <Query>*llli</Query>
 <Status ceid='off' cid='verified' code='100'/>
 <Expires>2008-07-14T02:21:06.000Z</Expires>
 <ProviderID>xri://@</ProviderID>
 <LocalID>!72cd.a072.157e.a9c6</LocalID>
 <CanonicalID>@!72CD.A072.157E.A9C6</CanonicalID>
 <Service priority='10'>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <URI append='none' priority='1'>https://login.llli.org/server/</URI>
 </Service>
 <Service priority='1'>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type match='null' select='false'/>
  <Type select='true'>xri://+i-service*(+forwarding)*($v*1.0)</Type>
  <Path match='default'/>
  <Path>(+index)</Path>
  <URI append='qxri' priority='1'>http://linksafe-forward.ezibroker.net/forwarding/</URI>
 </Service>
 <Service priority='10'>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://$res*auth*($v*2.0)</Type>
  <MediaType>application/xrds+xml;trust=none</MediaType>
  <URI priority='10'>http://resolve.ezibroker.net/resolve/@llli/</URI>
 </Service>
 <Service priority='10'>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null'/>
  <URI append='authority' priority='1'>http://linksafe-contact.ezibroker.net/contact/</URI>
 </Service>
</XRD>
";
			string llliAreaResponse = @"<?xml version='1.0' encoding='UTF-8'?>
<XRD xmlns='xri://$xrd*($v*2.0)'>
 <Query>*area</Query>
 <Status cid='verified' code='100'>SUCCESS</Status>
 <ServerStatus code='100'>SUCCESS</ServerStatus>
 <Expires>2008-07-15T01:21:07.000Z</Expires>
 <ProviderID>xri://!!1003</ProviderID>
 <LocalID>0000.0000.3B9A.CA0C</LocalID>
 <CanonicalID>@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C</CanonicalID>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <URI append='none' priority='1'>https://login.llli.org/server/</URI>
 </Service>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null'/>
  <URI append='authority' priority='1'>http://linksafe-contact.ezibroker.net/contact/</URI>
 </Service>
 <Service priority='1'>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://+i-service*(+forwarding)*($v*1.0)</Type>
  <Type match='null' select='false'/>
  <Path>(+index)</Path>
  <Path match='default'/>
  <URI append='qxri' priority='1'>http://linksafe-forward.ezibroker.net/forwarding/</URI>
 </Service>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://$res*auth*($v*2.0)</Type>
  <MediaType>application/xrds+xml;trust=none</MediaType>
  <URI>http://resolve.ezibroker.net/resolve/@llli*area/</URI>
 </Service>
</XRD>";
			string llliAreaCanadaUnattachedResponse = @"<?xml version='1.0' encoding='UTF-8'?>
<XRD xmlns='xri://$xrd*($v*2.0)'>
 <Query>*canada.unattached</Query>
 <Status cid='verified' code='100'>SUCCESS</Status>
 <ServerStatus code='100'>SUCCESS</ServerStatus>
 <Expires>2008-07-15T01:21:08.000Z</Expires>
 <ProviderID>xri://!!1003</ProviderID>
 <LocalID>0000.0000.3B9A.CA41</LocalID>
 <CanonicalID>@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C!0000.0000.3B9A.CA41</CanonicalID>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <URI append='none' priority='1'>https://login.llli.org/server/</URI>
 </Service>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null'/>
  <URI append='authority' priority='1'>http://linksafe-contact.ezibroker.net/contact/</URI>
 </Service>
 <Service priority='1'>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://+i-service*(+forwarding)*($v*1.0)</Type>
  <Type match='null' select='false'/>
  <Path>(+index)</Path>
  <Path match='default'/>
  <URI append='qxri' priority='1'>http://linksafe-forward.ezibroker.net/forwarding/</URI>
 </Service>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://$res*auth*($v*2.0)</Type>
  <MediaType>application/xrds+xml;trust=none</MediaType>
  <URI>http://resolve.ezibroker.net/resolve/@llli*area*canada.unattached/</URI>
 </Service>
</XRD>";
			string llliAreaCanadaUnattachedAdaResponse = @"<?xml version='1.0' encoding='UTF-8'?>
<XRD xmlns='xri://$xrd*($v*2.0)'>
 <Query>*ada</Query>
 <Status cid='verified' code='100'>SUCCESS</Status>
 <ServerStatus code='100'>SUCCESS</ServerStatus>
 <Expires>2008-07-15T01:21:10.000Z</Expires>
 <ProviderID>xri://!!1003</ProviderID>
 <LocalID>0000.0000.3B9A.CA01</LocalID>
 <CanonicalID>@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C!0000.0000.3B9A.CA41!0000.0000.3B9A.CA01</CanonicalID>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <URI append='none' priority='1'>https://login.llli.org/server/</URI>
 </Service>
 <Service>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null'/>
  <URI append='authority' priority='1'>http://linksafe-contact.ezibroker.net/contact/</URI>
 </Service>
 <Service priority='1'>
  <ProviderID>xri://!!1003!103</ProviderID>
  <Type select='true'>xri://+i-service*(+forwarding)*($v*1.0)</Type>
  <Type match='null' select='false'/>
  <Path>(+index)</Path>
  <Path match='default'/>
  <URI append='qxri' priority='1'>http://linksafe-forward.ezibroker.net/forwarding/</URI>
 </Service>
</XRD>";
			string webResponse = @"<?xml version='1.0' encoding='UTF-8'?>
<XRD version='2.0' xmlns='xri://$xrd*($v*2.0)'>
 <Query>*Web</Query>
 <Status ceid='off' cid='verified' code='100'/>
 <Expires>2008-07-14T02:21:12.000Z</Expires>
 <ProviderID>xri://=</ProviderID>
 <LocalID>!91f2.8153.f600.ae24</LocalID>
 <CanonicalID>=!91F2.8153.F600.AE24</CanonicalID>
 <Service priority='10'>
  <Type select='true'>xri://+i-service*(+locator)*($v*1.0)</Type>
  <Path select='true'>(+locator)</Path>
  <MediaType match='default' select='false'/>
  <URI append='qxri'>http://locator.fullxri.com/locator/</URI>
 </Service>
 <Service priority='10'>
  <ProviderID>xri://=web</ProviderID>
  <Type select='true'>xri://$res*auth*($v*2.0)</Type>
  <Type select='true'>xri://$res*auth*($v*2.0)</Type>
  <MediaType select='true'>application/xrds+xml</MediaType>
  <URI append='qxri' priority='1'>https://resolve.freexri.com/ns/=web/</URI>
  <URI append='qxri' priority='2'>http://resolve.freexri.com/ns/=web/</URI>
 </Service>
 <Service priority='10'>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <Type select='true'>http://specs.openid.net/auth/2.0/signon</Type>
  <Path select='true'>(+login)</Path>
  <Path match='default' select='false'/>
  <MediaType match='default' select='false'/>
  <URI append='none' priority='2'>http://authn.fullxri.com/authentication/</URI>
  <URI append='none' priority='1'>https://authn.fullxri.com/authentication/</URI>
 </Service>
 <Service priority='10'>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null' select='false'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null' select='false'/>
  <MediaType match='default' select='false'/>
  <URI append='qxri'>http://contact.fullxri.com/contact/</URI>
 </Service>
 <KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'>
  <X509Data>
   <X509Certificate>
MIIExzCCA6+gAwIBAgIJAM+MlFr0Sth6MA0GCSqGSIb3DQEBBQUAMIGdMR8wHQYD
VQQDExZTdXBlcnZpbGxhaW46IFRoZSBSb290MQswCQYDVQQGEwJVUzERMA8GA1UE
CBMITmV3IFlvcmsxDzANBgNVBAcTBkdvdGhhbTEgMB4GA1UEChMXU3VwZXJ2aWxs
YWluIFVuaXZlcnNpdHkxJzAlBgkqhkiG9w0BCQEWGHBlbmd1aW5Ac3VwZXJ2aWxs
YWluLmVkdTAeFw0wNjA4MTcxOTU5NTNaFw0xMTA4MTYxOTU5NTNaMIGdMR8wHQYD
VQQDExZTdXBlcnZpbGxhaW46IFRoZSBSb290MQswCQYDVQQGEwJVUzERMA8GA1UE
CBMITmV3IFlvcmsxDzANBgNVBAcTBkdvdGhhbTEgMB4GA1UEChMXU3VwZXJ2aWxs
YWluIFVuaXZlcnNpdHkxJzAlBgkqhkiG9w0BCQEWGHBlbmd1aW5Ac3VwZXJ2aWxs
YWluLmVkdTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAL6uFqas4dK6
A2wTZL0viRQNJrPyFnFBDSZGib/2ijhgzed/vvmZIBM9sFpwahcuR5hvyKUe37/c
/RSZXoNDi/eiNOx4qb0l9UB6bd8qvc4V1PnLE7L+ZYcmwrvTKm4x8qXMgEv1wca2
FPsreHNPdLiTUZ8v0tDTWi3Mgi7y47VTzJaTkcfmO1nL6xAtln5sLdH0PbMM3LAp
T1d3nwI3VdbhqqZ+6+OKEuC8gk5iH4lfrbr6C9bYS6vzIKrotHpZ3N2aIC3NMjJD
PMw/mfCuADfRNlHXgZW+0zyUkwGTMDea8qgsoAMWJGdeTIw8I1I3RhnbgLzdsNQl
b/1ZXx1uJRUCAwEAAaOCAQYwggECMB0GA1UdDgQWBBQe+xSjYTrlfraJARjMxscb
j36jvDCB0gYDVR0jBIHKMIHHgBQe+xSjYTrlfraJARjMxscbj36jvKGBo6SBoDCB
nTEfMB0GA1UEAxMWU3VwZXJ2aWxsYWluOiBUaGUgUm9vdDELMAkGA1UEBhMCVVMx
ETAPBgNVBAgTCE5ldyBZb3JrMQ8wDQYDVQQHEwZHb3RoYW0xIDAeBgNVBAoTF1N1
cGVydmlsbGFpbiBVbml2ZXJzaXR5MScwJQYJKoZIhvcNAQkBFhhwZW5ndWluQHN1
cGVydmlsbGFpbi5lZHWCCQDPjJRa9ErYejAMBgNVHRMEBTADAQH/MA0GCSqGSIb3
DQEBBQUAA4IBAQC4SPBDGYAxfbXd8N5OvG0drM7a5hjXfcCZpiILlPSRpxp79yh7
I5vVWxBxUfolwbei7PTBVy7CE27SUbSICeqWjcDCfjNjiZk6mLS80rm/TdLrHSyM
+Ujlw9MGcBGaLI+sdziDUMtTQDpeAyQTaGVbh1mx5874Hlo1VXqGYNo0RwR+iLfs
x48VuO6GbWVyxtktkE2ypz1KLWiyI056YynydRvuBCBHeRqGUixPlH9CrmeSCP2S
sfbiKnMOGXjIYbvbsTAMdW2iqg6IWa/fgxhvZoAXChM9bkhisJQc0qD0J5TJQwgr
uEyb50RJ7DWmXctSC0b3eymZ2lSXxAWNOsNy
			</X509Certificate>
  </X509Data>
 </KeyInfo>
</XRD>";
			this.RegisterMockXrdsResponses(new Dictionary<string, string> {
				{ "https://xri.net/@llli?_xrd_r=application/xrd%2Bxml;sep=false", llliResponse },
				{ "https://xri.net/@llli*area?_xrd_r=application/xrd%2Bxml;sep=false", llliAreaResponse },
				{ "https://xri.net/@llli*area*canada.unattached?_xrd_r=application/xrd%2Bxml;sep=false", llliAreaCanadaUnattachedResponse },
				{ "https://xri.net/@llli*area*canada.unattached*ada?_xrd_r=application/xrd%2Bxml;sep=false", llliAreaCanadaUnattachedAdaResponse },
				{ "https://xri.net/=Web?_xrd_r=application/xrd%2Bxml;sep=false", webResponse },
			});
			await this.VerifyCanonicalIdAsync("@llli", "@!72CD.A072.157E.A9C6");
			await this.VerifyCanonicalIdAsync("@llli*area", "@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C");
			await this.VerifyCanonicalIdAsync("@llli*area*canada.unattached", "@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C!0000.0000.3B9A.CA41");
			await this.VerifyCanonicalIdAsync("@llli*area*canada.unattached*ada", "@!72CD.A072.157E.A9C6!0000.0000.3B9A.CA0C!0000.0000.3B9A.CA41!0000.0000.3B9A.CA01");
			await this.VerifyCanonicalIdAsync("=Web", "=!91F2.8153.F600.AE24");
		}

		[Test]
		public async Task DiscoveryCommunityInameDelegateWithoutCanonicalID() {
			this.RegisterMockXrdsResponses(new Dictionary<string, string> {
				{ "https://xri.net/=Web*andrew.arnott?_xrd_r=application/xrd%2Bxml;sep=false", @"<?xml version='1.0' encoding='UTF-8'?>
<XRD xmlns='xri://$xrd*($v*2.0)'>
 <Query>*andrew.arnott</Query>
 <Status cid='absent' code='100'>Success</Status>
 <ServerStatus code='100'>Success</ServerStatus>
 <Expires>2008-07-14T03:30:59.722Z</Expires>
 <ProviderID>=!91F2.8153.F600.AE24</ProviderID>
 <Service>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <Path select='true'>(+login)</Path>
  <Path match='default'/>
  <MediaType match='default'/>
  <URI append='none' priority='2'>http://www.myopenid.com/server</URI>
  <openid:Delegate xmlns:openid='http://openid.net/xmlns/1.0'>http://blog.nerdbank.net</openid:Delegate>
 </Service>
 <Service>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null'/>
  <MediaType match='default'/>
  <URI append='qxri'>http://contact.freexri.com/contact/</URI>
 </Service>
 <Service>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>xri://+i-service*(+forwarding)*($v*1.0)</Type>
  <Path select='true'>(+index)</Path>
  <Path match='default'/>
  <MediaType match='default'/>
  <URI append='qxri'>http://forwarding.freexri.com/forwarding/</URI>
 </Service>
 <Service>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <Path select='true'>(+login)</Path>
  <Path match='default'/>
  <MediaType match='default'/>
  <URI append='none' priority='2'>http://authn.freexri.com/authentication/</URI>
  <URI append='none' priority='1'>https://authn.freexri.com/authentication/</URI>
 </Service>
 <ServedBy>OpenXRI</ServedBy>
</XRD>" },
				{ "https://xri.net/@id*andrewarnott?_xrd_r=application/xrd%2Bxml;sep=false", @"<?xml version='1.0' encoding='UTF-8'?>
<XRD xmlns='xri://$xrd*($v*2.0)'>
 <Query>*andrewarnott</Query>
 <Status cid='absent' code='100'>Success</Status>
 <ServerStatus code='100'>Success</ServerStatus>
 <Expires>2008-07-14T03:31:00.466Z</Expires>
 <ProviderID>@!B1E8.C27B.E41C.25C3</ProviderID>
 <Service>
  <Type select='true'>http://openid.net/signon/1.0</Type>
  <Path select='true'>(+login)</Path>
  <Path match='default'/>
  <MediaType match='default'/>
  <URI append='none' priority='2'>http://www.myopenid.com/server</URI>
  <openid:Delegate xmlns:openid='http://openid.net/xmlns/1.0'>http://blog.nerdbank.net</openid:Delegate>
 </Service>
 <Service>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>xri://+i-service*(+contact)*($v*1.0)</Type>
  <Type match='null'/>
  <Path select='true'>(+contact)</Path>
  <Path match='null'/>
  <MediaType match='default'/>
  <URI append='qxri'>http://contact.freexri.com/contact/</URI>
 </Service>
 <Service>
  <ProviderID>@!7F6F.F50.A4E4.1133</ProviderID>
  <Type select='true'>xri://+i-service*(+forwarding)*($v*1.0)</Type>
  <Path select='true'>(+index)</Path>
  <Path match='default'/>
  <MediaType match='default'/>
  <URI append='qxri'>http://forwarding.freexri.com/forwarding/</URI>
 </Service>
 <ServedBy>OpenXRI</ServedBy>
</XRD>" },
			});
			// Consistent with spec section 7.3.2.3, we do not permit
			// delegation on XRI discovery when there is no CanonicalID present.
			await this.VerifyCanonicalIdAsync("=Web*andrew.arnott", null);
			await this.VerifyCanonicalIdAsync("@id*andrewarnott", null);
		}

		private async Task<IdentifierDiscoveryResult> VerifyCanonicalIdAsync(Identifier iname, string expectedClaimedIdentifier) {
			var se = (await this.DiscoverAsync(iname)).FirstOrDefault();
			if (expectedClaimedIdentifier != null) {
				Assert.IsNotNull(se);
				Assert.AreEqual(expectedClaimedIdentifier, se.ClaimedIdentifier.ToString(), "i-name {0} discovery resulted in unexpected CanonicalId", iname);
				Assert.IsTrue(se.Capabilities.Count > 0);
			} else {
				Assert.IsNull(se);
			}
			return se;
		}
	}
}
