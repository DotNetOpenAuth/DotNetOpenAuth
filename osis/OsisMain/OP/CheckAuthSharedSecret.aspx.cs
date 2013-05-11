using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId;
using System.ServiceModel;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.ChannelElements;
using System.Diagnostics;

public partial class OP_CheckAuthRejectsSharedAssociationHandles : System.Web.UI.Page {
	protected void beginButton_Click(object sender, EventArgs e) {
		if (!Page.IsValid) {
			return;
		}

		IdentifierDiscoveryResult endpoint;
		var rp = new OpenIdRelyingParty();
		try {
			Identifier identifier = identifierBox.Text;
			endpoint = rp.Discover(identifier).First();
		} catch (ProtocolException ex) {
			testResultDisplay.Pass = false;
			testResultDisplay.Details = ex.Message;
			return;
		}

		MultiView1.ActiveViewIndex = 1;
		testResultDisplay.ProviderEndpoint = endpoint.ProviderEndpoint;
		testResultDisplay.ProtocolVersion = endpoint.Version;

		if (ForceSHA1Association.Checked) {
			rp.SecuritySettings.MaximumHashBitLength = 160;
		}

		try {
			// Establish a shared association with that provider endpoint.
			Association association = rp.AssociationManager.CreateNewAssociation(endpoint);
			if (association == null) {
				throw new ApplicationException("Unable to establish an association with the Provider");
			}

			// Forge an assertion from the Provider.
			var assertion = new PositiveAssertionResponse(endpoint.Version, new Uri(Request.Url, Request.Url.AbsolutePath));
			assertion.ClaimedIdentifier = (string)endpoint.ClaimedIdentifier;     // These (string) casts change the "dnoahttp://" scheme to just "http://" in the OriginalString property...
			assertion.LocalIdentifier = (string)endpoint.ProviderLocalIdentifier; // ... which is very important for signing correctness.
			assertion.ProviderEndpoint = endpoint.ProviderEndpoint;

			// Sign it using the shared association.
			ITamperResistantOpenIdMessage signedAssertion = assertion;
			signedAssertion.AssociationHandle = association.Handle;
			var opStore = new StandardProviderApplicationStore();
			opStore.StoreAssociation(AssociationRelyingPartyType.Smart, association);
			var op = new OpenIdProvider(opStore);
			op.SecuritySettings.ProtectDownlevelReplayAttacks = false; // avoids the OP forcing a private association for OpenID 1.1 endpoints.
			op.Channel.PrepareResponse(assertion);
			if (signedAssertion.AssociationHandle != association.Handle) {
				throw new ApplicationException("Internal test error when preparing signed assertion.");
			}

			// Send the assertion via check_auth to the Provider for confirmation.
			var checkAuthRequest = new CheckAuthenticationRequest(assertion, rp.Channel);
			var opResponse = rp.Channel.Request<CheckAuthenticationResponse>(checkAuthRequest);

			if (opResponse.IsValid) {
				testResultDisplay.Pass = false;
				testResultDisplay.Details = "OP confirmed an assertion with a shared association.";
			} else {
				testResultDisplay.Pass = true;
				testResultDisplay.Details = "OP rejected the check_auth message with the shared association handle.";
			}
		} catch (ProtocolException ex) {
			testResultDisplay.Pass = false;
			testResultDisplay.Details = ex.Message;
		}
	}
}