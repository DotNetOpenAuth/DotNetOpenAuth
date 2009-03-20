<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" %>

<%@ Import Namespace="OpenIdProviderWebForms.Code" %>
<%@ Import Namespace="DotNetOpenAuth.OpenId.Provider" %>
<%@ Import Namespace="DotNetOpenAuth.Messaging" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId" TagPrefix="openid" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth" TagPrefix="openauth" %>

<script runat="server">
	protected void sendAssertionButton_Click(object sender, EventArgs e) {
		TextBox relyingPartySite = (TextBox)loginView.FindControl("relyingPartySite");
		Uri providerEndpoint = new Uri(Request.Url, Page.ResolveUrl("~/server.aspx"));
		OpenIdProvider op = new OpenIdProvider();
		try {
			op.PrepareUnsolicitedAssertion(providerEndpoint, relyingPartySite.Text, Util.BuildIdentityUrl(), Util.BuildIdentityUrl()).Send();
		} catch (ProtocolException ex) {
			Label errorLabel = (Label)loginView.FindControl("errorLabel");
			errorLabel.Visible = true;
			errorLabel.Text = ex.Message;
		}
	}
</script>

<asp:Content runat="server" ContentPlaceHolderID="head">
	<openauth:XrdsPublisher runat="server" XrdsUrl="~/op_xrds.aspx" />
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Main">
	<h2>Provider </h2>
	<p>Welcome. This site doesn't do anything more than simple authentication of users.
		Start the authentication process on the Relying Party sample site, or log in here
		and send an unsolicited assertion. </p>
	<asp:LoginView runat="server" ID="loginView">
		<LoggedInTemplate>
			<asp:Panel runat="server" DefaultButton="sendAssertionButton">
			<p>You're logged in as <b><%= HttpUtility.HtmlEncode(User.Identity.Name) %></b> </p>
			<p>Your claimed identifier is <b><%= HttpUtility.HtmlEncode(Util.BuildIdentityUrl()) %></b> </p>
				<p>Since you're logged in, try sending an unsolicited assertion to an OpenID 2.0 relying
					party site. Just type in the URL to the site's home page. This could be the sample
					relying party web site. </p>
				<div>
					<asp:TextBox runat="server" ID="relyingPartySite" Columns="40" />
					<asp:Button runat="server" ID="sendAssertionButton" Text="Send assertion" OnClick="sendAssertionButton_Click" />
					<asp:RequiredFieldValidator runat="server" ControlToValidate="relyingPartySite" Text="Specify relying party site first" />
				</div>
				<p>An unsolicited assertion is a way to log in to a relying party site directly from
					your OpenID Provider. </p>
				<p><asp:Label runat="server" EnableViewState="false" Visible="false" ID="errorLabel"
					ForeColor="Red" /> </p>
			</asp:Panel>
		</LoggedInTemplate>
	</asp:LoginView>
	<asp:LoginStatus runat="server" />
</asp:Content>
