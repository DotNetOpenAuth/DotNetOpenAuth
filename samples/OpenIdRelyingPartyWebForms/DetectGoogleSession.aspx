<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DetectGoogleSession.aspx.cs"
	Inherits="OpenIdRelyingPartyWebForms.DetectGoogleSession" ValidateRequest="false"
	MasterPageFile="~/Site.Master" Async="true" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.Extensions.SimpleRegistration"
	TagPrefix="sreg" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<asp:Label Text="We've detected that you're logged into Google!" runat="server" Visible="false"
		ID="YouAreLoggedInLabel" />
	<asp:Label Text="We've detected that you're logged into Google because your Google account trusts this site!" runat="server" Visible="false"
		ID="YouTrustUsLabel" />
	<asp:Label Text="We've detected that you're NOT logged into Google!" runat="server" Visible="false"
		ID="YouAreNotLoggedInLabel" />
</asp:Content>
