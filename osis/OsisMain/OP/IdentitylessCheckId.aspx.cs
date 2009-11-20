using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

public partial class OP_IdentitylessCheckId : System.Web.UI.Page {
	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			var rp = new OpenIdRelyingParty();
			try {
				var response = rp.GetResponse();
				if (response != null) {
					MultiView1.ActiveViewIndex = 1;
					if (response.Status == AuthenticationStatus.ExtensionsOnly) {
						if (response.GetExtension<ClaimsResponse>() != null || response.GetExtension<FetchResponse>() != null) {
							testResultDisplay.Pass = true;
						} else {
							testResultDisplay.Pass = false;
							testResultDisplay.Details = "No sreg or AX extension response was present in the response.";
						}
					} else if (response.Status == AuthenticationStatus.Canceled) {
						testResultDisplay.Pass = true;
						testResultDisplay.Details = "OP sent a 'cancel' response, which may be acceptable depending on the Provider.";
					} else {
						testResultDisplay.Pass = false;
						testResultDisplay.Details = "Expected an identity-less id_res message but got a " + response.Status + " response instead.";
					}

					testResultDisplay.ProviderEndpoint = new Uri(response.GetCallbackArgument("op_endpoint"));
					testResultDisplay.ProtocolVersion = new Version(response.GetCallbackArgument("version"));
				}
			} catch (ProtocolException ex) {
				testResultDisplay.Pass = false;
				testResultDisplay.Details = ex.Message;
			}
		}
	}

	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		var rp = new OpenIdRelyingParty();
		try {
			var request = rp.CreateRequest(identifierBox.Text);
			request.IsExtensionOnly = true;

			request.AddCallbackArguments("op_endpoint", request.Provider.Uri.AbsoluteUri);
			request.AddCallbackArguments("version", request.Provider.Version.ToString());

			// Add both sreg and ax extensions with lots of requests to try to generate
			// some return data...
			request.AddExtension(new ClaimsRequest() {
				FullName = DemandLevel.Request,
				Email = DemandLevel.Request,
				PostalCode = DemandLevel.Request,
			});

			var ax = new FetchRequest();
			ax.Attributes.AddOptional(WellKnownAttributes.Contact.Email);
			ax.Attributes.AddOptional(WellKnownAttributes.Name.FullName);
			ax.Attributes.AddOptional(WellKnownAttributes.Contact.HomeAddress.PostalCode);
			request.AddExtension(ax);

			request.RedirectToProvider();
		} catch (ProtocolException ex) {
			errorLabel.Text = ex.Message;
			errorLabel.Visible = true;
		}
	}
}
