using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId.Provider;

/// <remarks>
/// The attack vector that requires return_to verification to thwart
/// is a redirected positive assertion.  That is, an attacker can
/// intercept a valid assertion going to a.com, and send it instead 
/// to b.com.  Provided the assertion is signed by an OP's private
/// association, b.com would accept the assertion and the attacker
/// would be granted access as a spoofed identity.
/// return_to verification prevents b.com from accepting an assertion
/// directed at a.com.
/// In this simulation, since many RPs don't yet support unsolicited
/// positive assertions, which the above scenario describes,
/// in order to isolate the return_to verification test, 
/// we actually have "b.com" solicit an assertion, but then in the 
/// positive assertion response we change it to make it look like
/// it was targeted for "a.com".  This way the response is just
/// perfect for b.com's consumption, except that it will fail
/// if b.com is performing return_to verification.
/// </remarks>
public partial class RP_VerifyReturnTo : System.Web.UI.Page {
	/// <summary>
	/// The various ways in which a return_to can be changed that should
	/// trigger a failed authentication at the RP.
	/// </summary>
	private enum ReturnToChangeMethod {
		Scheme = 1,
		HostName,
		Port,
		PathSignificant,
		PathCapitalization,
		ExtraQueryParameter,
		PostReturnToArgsInEntity,
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

					// The POST return_to args in entity test is meaningless without callback args from the RP.
					var opAuthReq = (AuthenticationRequest)ViewState["PendingAuth"];
					if (opAuthReq.positiveResponse.Recipient.Query == null || opAuthReq.positiveResponse.Recipient.Query.Length <= 1) {
						postArgsButton.Enabled = false;
						postArgsButton.ToolTip = "This test is not available because the RP did not send any return_to callback arguments.";
					}
				} else {
					op.SendResponse(req);
				}
			}
		}
	}

	protected void CompleteAuthentication_Click(object sender, EventArgs e) {
		Button sendingButton = (Button)sender;
		ReturnToChangeMethod method = (ReturnToChangeMethod)int.Parse(sendingButton.CommandArgument);
		OpenIdProvider op = new OpenIdProvider();

		// We need to change the return_to URL here before
		// sending back the assertion.
		var opAuthReq = (AuthenticationRequest)ViewState["PendingAuth"];
		opAuthReq.positiveResponse = new PositiveAssertionResponseNoCheck((CheckIdRequest)opAuthReq.RequestMessage);
		opAuthReq.IsAuthenticated = true;

		// One special test forces use of POST, and moves all callback arguments from
		// the GET query string to the POST entity.
		if (method == ReturnToChangeMethod.PostReturnToArgsInEntity) {
			// force POST
			op.Channel.IndirectMessageGetToPostThreshold = 1;

			// Copy the callback args to the payload of the message
			NameValueCollection callbackArgs = HttpUtility.ParseQueryString(opAuthReq.positiveResponse.Recipient.Query);
			foreach (string key in callbackArgs) {
				opAuthReq.positiveResponse.ExtraData[key] = callbackArgs[key];
			}

			// Clear the args from the query string.
			UriBuilder newRecipient = new UriBuilder(opAuthReq.positiveResponse.Recipient);
			newRecipient.Query = null;
			opAuthReq.positiveResponse.Recipient = newRecipient.Uri;
		} else {
			opAuthReq.positiveResponse.ReturnTo = GetAlteredReturnTo(opAuthReq.positiveResponse.ReturnTo, method);
		}

		op.SendResponse(opAuthReq);
	}

	private static Uri GetAlteredReturnTo(Uri originalReturnTo, ReturnToChangeMethod method) {
		UriBuilder hackedReturnTo = new UriBuilder(originalReturnTo);
		switch (method) {
			case ReturnToChangeMethod.Scheme:
				hackedReturnTo.Scheme = originalReturnTo.Scheme == "http" ? "https" : "http";
				break;
			case ReturnToChangeMethod.HostName:
				hackedReturnTo.Host = originalReturnTo.Host + "a";
				break;
			case ReturnToChangeMethod.Port:
				hackedReturnTo.Port += 1;
				break;
			case ReturnToChangeMethod.PathSignificant:
				hackedReturnTo.Path = originalReturnTo.AbsolutePath + "a";
				break;
			case ReturnToChangeMethod.PathCapitalization:
				string locase = originalReturnTo.AbsolutePath.ToLowerInvariant();
				string upcase = originalReturnTo.AbsolutePath.ToUpperInvariant();
				if (locase != originalReturnTo.AbsolutePath) {
					hackedReturnTo.Path = locase;
				} else if (upcase != originalReturnTo.AbsolutePath) {
					hackedReturnTo.Path = upcase;
				} else {
					// There are no characters in the path whose case can change!
				}
				break;
			case ReturnToChangeMethod.ExtraQueryParameter:
				hackedReturnTo.AppendQueryArgs(new Dictionary<string, string> {
						{ "extraKey", "extraValue" },
					});
				break;
			default:
				throw new ArgumentException();
		}

		return hackedReturnTo.Uri;
	}
}
