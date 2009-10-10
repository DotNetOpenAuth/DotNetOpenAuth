<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebFormsRelyingParty.Login"
	MasterPageFile="~/Site.Master" ValidateRequest="false" %>

<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty"
	TagPrefix="rp" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.InfoCard" TagPrefix="ic" %>
<asp:Content runat="server" ContentPlaceHolderID="Body">
	<h2>Login</h2>
	<p>Login using an account you already use:</p>
	<div class="OpenIdButtons">
		<span class="OpenIdButton" onclick='clickChild(this)'><span>
			<rp:OpenIdButton runat="server" ImageUrl="~/images/google.gif" Text="Login with Google"
				Identifier="https://www.google.com/accounts/o8/id" PrecreateRequest="true" OnLoggedIn="openIdLogin1_LoggedIn" />
		</span></span><span class="OpenIdButton" onclick='clickChild(this)'><span>
			<rp:OpenIdButton runat="server" ImageUrl="~/images/yahoo.gif" Text="Login with Yahoo!"
				Identifier="https://me.yahoo.com/" PrecreateRequest="true" OnLoggedIn="openIdLogin1_LoggedIn" />
		</span></span><span class="OpenIdButton" id="InfoCardButton" style="display: none">
			<ic:InfoCardSelector ID="InfoCardSelector1" runat="server" ImageSize="Size92x64"
				OnReceivedToken="InfoCardSelector1_ReceivedToken" ToolTip="Log in with your Information Card">
				<ic:ClaimType Name="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier" />
			</ic:InfoCardSelector>
		</span>
	</div>
	<div class="OpenIdBox">
		<rp:OpenIdLogin runat="server" ID="openIdLogin1" RequestFullName="Request" RequestEmail="Request"
			OnLoggedIn="openIdLogin1_LoggedIn" TabIndex="1" RegisterVisible="false" />
	</div>
	<p>If you don't have an account with any of these services, you can <a href="https://www.myopenid.com/signup">
		create one</a> and then use it for this and many other web sites.<br />
		If you have logged into this site previously, click the same button you did last
		time. </p>

	<script type="text/javascript">
		function clickChild(parent, recursion) {
			if (recursion && parent.click) {
				parent.click();
			}
			for (var i = 0; i < parent.childNodes.length; i++) {
				clickChild(parent.childNodes[i], true);
			}
		};
		if (document.infoCard && document.infoCard.isSupported()) {
			document.getElementById('InfoCardButton').style.display = 'block';
		}
	</script>

</asp:Content>
