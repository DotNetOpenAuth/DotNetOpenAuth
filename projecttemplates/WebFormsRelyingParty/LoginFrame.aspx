<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginFrame.aspx.cs" Inherits="WebFormsRelyingParty.LoginFrame"
	EnableViewState="false" ValidateRequest="false" %>

<%@ Register Assembly="DotNetOpenAuth.OpenID.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<%@ Register Assembly="DotNetOpenAuth.OpenID" Namespace="DotNetOpenAuth.OpenId.Extensions.SimpleRegistration" TagPrefix="sreg" %>
<%@ Register Assembly="DotNetOpenAuth.InfoCard.UI" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<%@ Register Assembly="DotNetOpenAuth.OpenIdInfoCard.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="rpic" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!-- COPYRIGHT (C) 2011 Outercurve Foundation.  All rights reserved. -->
<!-- LICENSE: Microsoft Public License available at http://opensource.org/licenses/ms-pl.html -->
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<link rel="stylesheet" type="text/css" href="theme/ui.all.css" />
	<link rel="stylesheet" type="text/css" href="styles/loginpopup.css" />
</head>
<body>
<% if (Request.Url.IsLoopback) { %>
	<script type="text/javascript" src="scripts/jquery-1.3.1.js"></script>
	<script type="text/javascript" src="scripts/jquery-ui-personalized-1.6rc6.js"></script>
<% } else { %>
	<script type="text/javascript" language="javascript" src="http://www.google.com/jsapi"></script>
	<script type="text/javascript" language="javascript">
		google.load("jquery", "1.3.2");
		google.load("jqueryui", "1.7.2");
	</script>
<% } %>

	<script type="text/javascript" src="scripts/jquery.cookie.js"></script>

	<form runat="server" id="form1" target="_top">
	<div class="wrapper">
		<p>
			Login with an account you already use!
		</p>
		<rpic:OpenIdInfoCardSelector runat="server" ID="openIdSelector" OnLoggedIn="openIdSelector_LoggedIn"
			OnFailed="openIdSelector_Failed" OnCanceled="openIdSelector_Failed" OnReceivedToken="openIdSelector_ReceivedToken"
			OnTokenProcessingError="openIdSelector_TokenProcessingError">
			<Buttons>
				<rp:SelectorProviderButton OPIdentifier="https://me.yahoo.com/" Image="images/yahoo.gif" />
				<rp:SelectorProviderButton OPIdentifier="https://www.google.com/accounts/o8/id" Image="images/google.gif" />
				<rpic:SelectorInfoCardButton>
					<InfoCardSelector Issuer="">
						<ClaimsRequested>
							<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" IsOptional="false" />
							<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" IsOptional="true" />
							<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" IsOptional="true" />
						</ClaimsRequested>
					</InfoCardSelector>
				</rpic:SelectorInfoCardButton>
				<rp:SelectorOpenIdButton Image="images/openid.png" />
			</Buttons>
			<Extensions>
				<sreg:ClaimsRequest Email="Require" FullName="Request" />
			</Extensions>
		</rpic:OpenIdInfoCardSelector>
		<asp:HiddenField runat="server" ID="topWindowUrl" />
		<asp:Panel ID="errorPanel" runat="server" EnableViewState="false" Visible="false" ForeColor="Red">
			Oops. Something went wrong while logging you in. Trying again may work. <a href="#" onclick="$('#errorMessage').show()">
				What went wrong?</a>
			<span id="errorMessage" style="display: none">
				<asp:Label ID="errorMessageLabel" runat="server" Text="Login canceled." />
			</span>
		</asp:Panel>
		<div class="helpDoc">
			<p>
				If you have logged in previously, click the same button you did last time.
			</p>
			<p>
				If you don't have an account with any of these services, just pick Google. They'll
				help you set up an account.
			</p>
		</div>
	</div>
	</form>
</body>
</html>
