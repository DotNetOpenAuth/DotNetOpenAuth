<%@ Page Title="" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false"
	ValidateRequest="false" %>
<%@ Import Namespace="DotNetOpenAuth.InfoCard" %>

<script runat="server">
	Protected Sub InfoCardSelector1_ReceivedToken(ByVal sender As Object, ByVal e As ReceivedTokenEventArgs) Handles InfoCardSelector1.ReceivedToken
		Session("SiteSpecificID") = e.Token.SiteSpecificId
		FormsAuthentication.RedirectFromLoginPage(e.Token.UniqueId, False)
	End Sub
</script>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<asp:Content ID="Content2" ContentPlaceHolderID="Main" runat="Server">
	<p>This login page demonstrates logging in using the InfoCard selector. Click the InfoCard
		image below to login. </p>
	<ic:InfoCardSelector runat="server" ID="InfoCardSelector1">
		<ClaimTypes>
			<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa" />
		</ClaimTypes>
		<UnsupportedTemplate>
			<p>You're using a browser that doesn't seem to have Information Card Selector support.
			</p>
			<p>In a real web application you might want to put an alternate login method here, or
				a link to find out how to enable the user's browser to use InfoCards. </p>
		</UnsupportedTemplate>
	</ic:InfoCardSelector>
</asp:Content>
