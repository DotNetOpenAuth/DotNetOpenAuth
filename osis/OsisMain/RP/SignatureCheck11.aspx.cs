using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.ChannelElements;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.Messaging.Reflection;
using System.Net.Security;
using DotNetOpenAuth.OpenId;

public partial class RP_SignatureCheck11 : System.Web.UI.Page {
	/// <summary>
	/// The various ways in which a signature can be changed that should
	/// trigger a failed authentication at the RP.
	/// </summary>
	private enum SignatureVariance {
		InvalidSignatureShared = 1,
		InvalidSignaturePrivate,
		MissingReturnTo,
		MissingIdentity,
	}

	protected void Page_Load(object sender, EventArgs e) {
		if (!IsPostBack) {
			OpenIdProvider op = new OpenIdProvider();

			IRequest req = op.GetRequest();
			if (req != null) {
				var authReq = req as IAuthenticationRequest;
				if (authReq != null) {
					var opAuthReq = (AuthenticationRequest)authReq;
					ViewState["PendingAuth"] = authReq;
					AuthPanel.Visible = true;
					if (((ITamperResistantOpenIdMessage)opAuthReq.positiveResponse).AssociationHandle == null) {
						invalidSignatureSharedButton.Enabled = false;
						invalidSignatureSharedButton.ToolTip = "Invalid test since RP is using stateless mode.";
					}
				} else {
					op.SendResponse(req);
				}
			}
		}
	}

	protected void CompleteAuthentication_Click(object sender, EventArgs e) {
		Button sendingButton = (Button)sender;
		SignatureVariance method = (SignatureVariance)int.Parse(sendingButton.CommandArgument);
		OpenIdProvider op = new OpenIdProvider();
		var signatureTamperer = new SignatureTamperingBindingElement();
		signatureTamperer.Channel = op.Channel;
		op.Channel.outgoingBindingElements.Add(signatureTamperer);

		// We need to change the assertion before sending it back.
		var opAuthReq = (AuthenticationRequest)ViewState["PendingAuth"];
		opAuthReq.IsAuthenticated = true;

		// Tamper with the assertion according to the user's selection.
		AlterAssertion(op.Channel, opAuthReq, method, signatureTamperer);
		op.Channel.Send(opAuthReq.positiveResponse);
	}

	private void AlterAssertion(Channel channel, AuthenticationRequest authRequest, SignatureVariance method, SignatureTamperingBindingElement signatureTamperer) {
		var assertion = authRequest.positiveResponse;
		
		// Ensure the channel has its own descriptions collection so we don't corrupt
		// the generally reusable ones.
		channel.MessageDescriptions = new MessageDescriptionCollection();
		MessageDescription description = channel.MessageDescriptions.Get(assertion);
		Protocol protocol = Protocol.Lookup(assertion.Version);
		string skipSignOnPart = null;
		switch (method) {
			case SignatureVariance.InvalidSignatureShared:
				signatureTamperer.InvalidateSignature = true;
				break;
			case SignatureVariance.InvalidSignaturePrivate:
				((ITamperResistantOpenIdMessage)assertion).AssociationHandle = null;
				signatureTamperer.InvalidateSignature = true;
				break;
			case SignatureVariance.MissingReturnTo:
				skipSignOnPart = protocol.openid.return_to;
				break;
			case SignatureVariance.MissingIdentity:
				skipSignOnPart = protocol.openid.identity;
				break;
			default:
				throw new ArgumentException();
		}

		if (skipSignOnPart != null) {
			description.Mapping[skipSignOnPart].RequiredProtection = ProtectionLevel.None;
		}
	}
}
