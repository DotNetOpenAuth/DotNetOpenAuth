using System;
using System.Collections.Specialized;
using System.Diagnostics;
using DotNetOpenId.Provider;
using ProviderCustomStore;

public partial class Server : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		var builder = new UriBuilder(Request.Url);
		builder.Query = null;
		builder.Fragment = null;
		Uri providerEndpoint = builder.Uri;
		NameValueCollection query = Request.RequestType == "GET" ? Request.QueryString : Request.Form;
		OpenIdProvider op = new OpenIdProvider(CustomStore.Instance, providerEndpoint, Request.Url, query);
		if (op.Request != null) {
			if (!op.Request.IsResponseReady) {
				var request = (IAuthenticationRequest)op.Request;
				if (request.IsDirectedIdentity) throw new NotSupportedException("This sample does not implement directed identity support.");
				request.IsAuthenticated = true;
			}
			Debug.Assert(op.Request.IsResponseReady);
			op.Request.Response.Send();
			Response.End();
		}
	}
}
