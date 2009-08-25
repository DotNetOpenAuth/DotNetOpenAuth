<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="rp" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<rp:OpenIdLogin ID="OpenIdLogin1" runat="server" TabIndex='1' />
</asp:Content>
