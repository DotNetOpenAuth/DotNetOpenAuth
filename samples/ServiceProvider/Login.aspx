<%@ Page Title="Login" Language="C#" MasterPageFile="~/MasterPage.master" %>
<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="rp" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" Runat="Server">
	<rp:OpenIdLogin runat="server" TabIndex='1' />
</asp:Content>
