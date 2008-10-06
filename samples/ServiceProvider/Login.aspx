<%@ Page Title="Login" Language="C#" MasterPageFile="~/MasterPage.master" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="rp" %>

<script runat="server">
	private void Page_Load(object sender, EventArgs e) {
		// fake out login for offline use of sample.
		FormsAuthentication.RedirectFromLoginPage("=!9B72.7DD1.50A9.5CCD", false);
	}
</script>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<rp:OpenIdLogin runat="server" TabIndex='1' />
</asp:Content>
