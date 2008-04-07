<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Import Namespace="DotNetOpenId.Provider" %>
<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId" TagPrefix="openid" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
	protected void sendAssertionButton_Click(object sender, EventArgs e) {
		TextBox relyingPartySite = (TextBox)loginView.FindControl("relyingPartySite");
		Uri providerEndpoint = new Uri(Request.Url, Page.ResolveUrl("~/server.aspx"));
		OpenIdProvider op = new OpenIdProvider(OpenIdProvider.HttpApplicationStore,
			providerEndpoint, Request.Url, Request.QueryString);
		try {
			op.PrepareUnsolicitedAssertion(relyingPartySite.Text, Util.BuildIdentityUrl(), Util.BuildIdentityUrl()).Send();
		} catch (OpenIdException ex) {
			Label errorLabel = (Label)loginView.FindControl("errorLabel");
			errorLabel.Visible = true;
			errorLabel.Text = ex.Message;
		}
	}
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<openid:XrdsPublisher runat="server" XrdsUrl="~/op_xrds.aspx" />
	<title>OpenID Provider, by DotNetOpenId</title>
</head>
<body>
	<form id="form1" runat="server">
	<h1>
		OpenID Provider
	</h1>
	<h2>
		Provided by <a href="http://dotnetopenid.googlecode.com">DotNetOpenId</a>
	</h2>
	<p>
		Welcome. This site doesn't do anything more than simple authentication of users.
		Start the authentication process on the Relying Party sample site, or log in here
		and send an unsolicited assertion.
	</p>
	<asp:LoginView runat="server" ID="loginView">
		<LoggedInTemplate>
			<asp:Panel runat="server" DefaultButton="sendAssertionButton">
				Since you're logged in, try sending an unsolicited assertion to an OpenID 2.0 relying
				party site. Just type in the URL to the site's home page. This could be the sample
				relying party web site.
				<br />
				<asp:TextBox runat="server" ID="relyingPartySite" Columns="40" />
				<asp:Button runat="server" ID="sendAssertionButton" Text="Send assertion" OnClick="sendAssertionButton_Click" />
				<asp:RequiredFieldValidator runat="server" ControlToValidate="relyingPartySite" Text="Specify relying party site first" />
				<br />
				An unsolicited assertion is a way to log in to a relying party site directly from
				your OpenID Provider.
				<p>
					<asp:Label runat="server" EnableViewState="false" Visible="false" ID="errorLabel"
						ForeColor="Red" />
				</p>
			</asp:Panel>
		</LoggedInTemplate>
	</asp:LoginView>
	<asp:LoginStatus runat="server" />
	</form>
</body>
</html>
