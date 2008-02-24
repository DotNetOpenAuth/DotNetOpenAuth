using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DotNetOpenId.Provider;
using System.Diagnostics;
using DotNetOpenId;

/// <summary>
/// This is the primary page for this open-id server.
/// This page is responsible for handling all open-id compliant requests:
/// </summary>
/// <remarks>
/// CheckIdRequest:
///   - when openid.mode='checkid_immediate' or openid.mode='checkid_setup'
///   - this is the initial message request sent to the server via server side call from the server
///   - this is stored in session in State.Session.LastRequest
/// 
/// AssociateRequest
///   - when openid.mode='associate'
///   - this is optionally sent by the consumer who is implementing smart mode to obtain the shared secret before an actual CheckIDRequest
///   - this is sent via a HTTP server side call from the consumer
///  
/// CheckAuthRequest
///   - when open.mode='check_authentication'
///   - this is request from the consumer to authenticate the user 
///   - this is sent via a HTTP 302 redirect by the consumer
/// </remarks>
public partial class server : System.Web.UI.Page {
	protected void Page_Load(object src, System.EventArgs evt) {
		Provider openIDServer = new Provider();
		Request request = null;

		// determine what incoming message was received
		try {
			request = openIDServer.DecodeRequest(Request);
		} catch (ProtocolException e) {
			Util.GenerateHttpResponse(e);
			return;
		}
		if (request == null) {
			contentForWebBrowsers.Visible = true;
			return;
		}

		// process the incoming message appropriately and send the response
		WebResponse response = null;
		switch (request.RequestType) {
			case RequestType.CheckIdRequest:
				CheckIdRequest idrequest = (CheckIdRequest)request;
				if (idrequest.Immediate) {
					String s = Util.ExtractUserName(idrequest.IdentityUrl);
					bool allow = (s != User.Identity.Name);
					response = idrequest.Answer(allow, Util.ServerUri);
				} else {
					State.Session.LastRequest = idrequest;
					Response.Redirect("~/decide.aspx");
				}
				break;
			default:
				response = openIDServer.HandleRequest(request);
				break;
		}
		Util.GenerateHttpResponse(response);
	}
}