<%@ Page Title="" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false"
	ValidateRequest="false" %>

<%@ Import Namespace="DotNetOpenAuth.InfoCard" %>

<script runat="server">
	Protected Sub InfoCardSelector1_ReceivedToken(ByVal sender As Object, ByVal e As ReceivedTokenEventArgs) Handles InfoCardSelector1.ReceivedToken
		Session("SiteSpecificID") = e.Token.SiteSpecificId
		FormsAuthentication.RedirectFromLoginPage(e.Token.UniqueId, False)
	End Sub
	
	Protected Sub InfoCardSelector1_TokenProcessingError(ByVal sender As Object, ByVal e As TokenProcessingErrorEventArgs) Handles InfoCardSelector1.TokenProcessingError
		errorLabel.Text = HttpUtility.HtmlEncode(e.Exception.Message)
		errorLabel.Visible = True
	End Sub
</script>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<asp:Content ID="Content2" ContentPlaceHolderID="Main" runat="Server">
	<p>This login page demonstrates logging in using the InfoCard selector. Click the InfoCard
		image below to login. </p>
	<ic:InfoCardSelector runat="server" ID="InfoCardSelector1">
		<ClaimsRequested>
			<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier" />
		</ClaimsRequested>
		<UnsupportedTemplate>
			<p>You're using a browser that doesn't seem to have Information Card Selector support.
			</p>
			<p>In a real web application you might want to put an alternate login method here, or
				a link to find out how to enable the user's browser to use InfoCards. </p>
		</UnsupportedTemplate>
	</ic:InfoCardSelector>
	<asp:Label runat="server" EnableViewState="false" Visible="false" ForeColor="Red" ID="errorLabel" />
</asp:Content>
