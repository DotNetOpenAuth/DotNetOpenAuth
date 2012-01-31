<%@ Page Title="Login" Language="C#" MasterPageFile="~/MasterPage.master" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="rp" %>
<script runat="server">
	protected void Page_Load(object sender, EventArgs e) {
		OpenIdLogin1.Focus();
	}
</script>
<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<rp:OpenIdLogin runat="server" TabIndex='1' ID="OpenIdLogin1" />
</asp:Content>
