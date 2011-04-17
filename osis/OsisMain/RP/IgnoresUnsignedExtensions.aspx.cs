using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

public partial class RP_IgnoresUnsignedExtensions : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			var op = new OpenIdProvider();
			op.SecuritySettings.SignOutgoingExtensions = false;

			IRequest req = op.GetRequest();
			if (req != null) {
				var hostReq = req as IHostProcessedRequest;
				if (hostReq != null) {
					var fetchResponse = new FetchResponse();
					var fetchRequest = hostReq.GetExtension<FetchRequest>();
					if (fetchRequest != null) {
						if (fetchRequest.Attributes.Contains(WellKnownAttributes.Contact.Email)) {
							fetchResponse.Attributes.Add(WellKnownAttributes.Contact.Email, "test-failed@if-you-see-this.com");
						}

						if (fetchRequest.Attributes.Contains(WellKnownAttributes.Name.FullName)) {
							fetchResponse.Attributes.Add(WellKnownAttributes.Name.FullName, "TestFailed IfYouSeeThis");
						}

						if (fetchRequest.Attributes.Contains(WellKnownAttributes.Name.First)) {
							fetchResponse.Attributes.Add(WellKnownAttributes.Name.First, "TestFailed");
						}

						if (fetchRequest.Attributes.Contains(WellKnownAttributes.Name.Last)) {
							fetchResponse.Attributes.Add(WellKnownAttributes.Name.Last, "IfYouSeeThis");
						}

						hostReq.AddResponseExtension(fetchResponse);
					}

					if (fetchResponse.Attributes.Count == 0) {
						MissingOrUnsupportedExtensionRequest.Visible = true;
						return;
					}

					var authReq = req as IAuthenticationRequest;
					if (authReq != null) {
						authReq.IsAuthenticated = true;
					}

					var anonReq = req as IAnonymousRequest;
					if (anonReq != null) {
						anonReq.IsApproved = true;
					}
				}

				op.SendResponse(req);
			}
		}

	}
}