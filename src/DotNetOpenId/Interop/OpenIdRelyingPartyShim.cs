using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DotNetOpenId.RelyingParty;
using System.Web;

namespace DotNetOpenId.Interop {
	[Guid("00462F34-21BE-456c-B986-B6DDE4DC5CA8")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IOpenIdRelyingParty {
		string CreateRequest(string userSuppliedIdentifier, string realm, string returnToUrl);
		AuthenticationResponseShim ProcessAuthentication(string url, string form);
	}

	[Guid("4D6FB236-1D66-4311-B761-972C12BB85E8")]
	[ProgId("DotNetOpenId.RelyingParty.OpenIdRelyingParty")]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComSourceInterfaces(typeof(IOpenIdRelyingParty))]
	public class OpenIdRelyingPartyShim : IOpenIdRelyingParty {
		public string CreateRequest(string userSuppliedIdentifier, string realm, string returnToUrl) {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null, null);
			Response response = (Response)rp.CreateRequest(userSuppliedIdentifier, realm, new Uri(returnToUrl)).RedirectingResponse;
			return response.IndirectMessageAsRequestUri.AbsoluteUri;
		}

		public AuthenticationResponseShim ProcessAuthentication(string url, string form) {
			Uri uri = new Uri(url);
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, uri, HttpUtility.ParseQueryString(uri.Query));
			if (rp.Response != null) {
				return new AuthenticationResponseShim(rp.Response);
			}

			return null;
		}
	}
}
