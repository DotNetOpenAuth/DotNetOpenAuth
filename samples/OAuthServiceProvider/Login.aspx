<%@ Page Title="Login" Language="C#" MasterPageFile="~/MasterPage.master" Async="true" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="rp" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<rp:OpenIdLogin runat="server" TabIndex='1' />
</asp:Content>
