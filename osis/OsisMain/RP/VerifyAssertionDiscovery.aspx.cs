using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.ChannelElements;

public partial class RP_VerifyAssertionDiscovery : System.Web.UI.Page {
	/// <summary>
	/// The various ways in which a positive assertion can be changed that should
	/// trigger a failed authentication at the RP.
	/// </summary>
	private enum AssertionVariance {
		ClaimedIdentifierPathSignificant = 1,
		ClaimedIdentifierPathCapitalization,
		ClaimedIdentifierHost,
		ClaimedIdentifierPort,
		OPLocalIdentifierSignificant,
		OPLocalIdentifierCapitalization,
		ProviderEndpointSignificant,
		ProviderEndpointCapitalization,
		OpenIdVersion,

		// these should NOT invalidate the assertion
		ClaimedIdentifierFragment,
		ClaimedIdentifierInsignificantQuery, // discovery on our identifier doesn't change due to just a small query parameter
	}

	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdProvider op = new OpenIdProvider();
			
			IRequest req = op.GetRequest();
			if (req != null) {
				var authReq = req as IAuthenticationRequest;
				if (authReq != null) {
					ViewState["PendingAuth"] = authReq;
					AuthPanel.Visible = true;
				} else {
					op.SendResponse(req);
				}
			} else {
				// Normalize the Claimed Identifier's capitalization so that
				// tampering with its capitalization in the positive assertion
				// is a significant difference.
				if (Request.Url.AbsolutePath != Page.ResolveUrl("~/RP/VerifyAssertionDiscovery.aspx")) {
					Response.Redirect("~/RP/VerifyAssertionDiscovery.aspx");
				}
			}
		}
	}

	protected void CompleteAuthentication_Click(object sender, EventArgs e) {
		Button sendingButton = (Button)sender;
		AssertionVariance method = (AssertionVariance)int.Parse(sendingButton.CommandArgument);
		OpenIdProvider op = new OpenIdProvider();

		// We need to change the assertion before sending it back.
		var opAuthReq = (AuthenticationRequest)ViewState["PendingAuth"];
		opAuthReq.positiveResponse = new PositiveAssertionResponseNoCheck((CheckIdRequest)opAuthReq.RequestMessage);
		opAuthReq.IsAuthenticated = true;
	
		// Force use of a private association so changing the OP endpoint
		// doesn't knock out the signature at the RP.
		((ITamperResistantOpenIdMessage)opAuthReq.positiveResponse).AssociationHandle = null;

		// Tamper with the assertion according to the user's selection.
		AlterAssertion(opAuthReq, method);
		op.Channel.Send(opAuthReq.positiveResponse);
	}

	private void AlterAssertion(AuthenticationRequest authRequest, AssertionVariance method) {
		var assertion = authRequest.positiveResponse;
		switch (method) {
			case AssertionVariance.ClaimedIdentifierPathSignificant:
				UriBuilder claimedId = new UriBuilder(assertion.ClaimedIdentifier);
				claimedId.Path += "a";
				assertion.ClaimedIdentifier = claimedId.Uri;
				break;
			case AssertionVariance.ClaimedIdentifierPathCapitalization:
				assertion.ClaimedIdentifier = assertion.ClaimedIdentifier.ToString().ToUpperInvariant();
				break;
			case AssertionVariance.ClaimedIdentifierHost:
				claimedId = new UriBuilder(assertion.ClaimedIdentifier);
				claimedId.Host = "some.randomhost.net";
				assertion.ClaimedIdentifier = claimedId.Uri;
				break;
			case AssertionVariance.ClaimedIdentifierPort:
				claimedId = new UriBuilder(assertion.ClaimedIdentifier);
				claimedId.Port = 777;
				assertion.ClaimedIdentifier = claimedId.Uri;
				break;
			case AssertionVariance.OPLocalIdentifierSignificant:
				assertion.LocalIdentifier = assertion.LocalIdentifier + "a";
				break;
			case AssertionVariance.OPLocalIdentifierCapitalization:
				assertion.LocalIdentifier = assertion.LocalIdentifier.ToString().ToUpperInvariant();
				break;
			case AssertionVariance.ProviderEndpointSignificant:
				assertion.ProviderEndpoint = new Uri(assertion.ProviderEndpoint.AbsoluteUri + "a");
				break;
			case AssertionVariance.ProviderEndpointCapitalization:
				assertion.ProviderEndpoint = new Uri(assertion.ProviderEndpoint.AbsoluteUri.ToUpperInvariant());
				break;
			case AssertionVariance.OpenIdVersion:
				assertion.OpenIdNamespace = null; // suppress the openid.ns parameter
				break;
			case AssertionVariance.ClaimedIdentifierFragment:
				authRequest.SetClaimedIdentifierFragment("someFragment");
				break;
			case AssertionVariance.ClaimedIdentifierInsignificantQuery:
				claimedId = new UriBuilder(assertion.ClaimedIdentifier);
				claimedId.AppendQueryArgs(new Dictionary<string, string> {
					{"a","1"},
				});
				assertion.ClaimedIdentifier = claimedId.Uri;
				break;
			default:
				break;
		}
	}
}
