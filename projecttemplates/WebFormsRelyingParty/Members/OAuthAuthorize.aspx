<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
	CodeBehind="OAuthAuthorize.aspx.cs" Inherits="WebFormsRelyingParty.Members.OAuthAuthorize" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="server">
	<h2>
		Client authorization
	</h2>
	<asp:MultiView runat="server" ID="outerMultiView" ActiveViewIndex="0">
		<asp:View runat="server" ID="getPermissionView">
			<div style="background-color: Yellow">
				<b>Warning</b>: Never give your login credentials to another web site or application.
			</div>
			<p>
				The
				<asp:Label ID="consumerNameLabel" runat="server" Text="(app name)" />
				application is requesting to access the private data in your account here. Is that
				alright with you?
			</p>
			<p>
				If you grant access now, you can revoke it at any time by returning to this page.
			</p>
			<div style="display: none" id="responseButtonsDiv">
				<asp:Button ID="yesButton" runat="server" Text="Yes" OnClick="yesButton_Click" />
				<asp:Button ID="noButton" runat="server" Text="No" OnClick="noButton_Click" />
				<asp:HiddenField runat="server" ID="csrfCheck" EnableViewState="false" />
			</div>
			<div id="javascriptDisabled">
				<b>Javascript appears to be disabled in your browser. </b>This page requires Javascript
				to be enabled to better protect your security.
			</div>
			<asp:Panel runat="server" BackColor="Red" ForeColor="White" Font-Bold="true" Visible="false" ID="OAuth10ConsumerWarning">
				This website is registered with service_PROVIDER_DOMAIN_NAME to make authorization requests, but has not been configured to send requests securely. If you grant access but you did not initiate this request at consumer_DOMAIN_NAME, it may be possible for other users of consumer_DOMAIN_NAME to access your data. We recommend you deny access unless you are certain that you initiated this request directly with consumer_DOMAIN_NAME.
			</asp:Panel>

			<script language="javascript" type="text/javascript">
				//<![CDATA[
				// we use HTML to hide the action buttons and Javascript to show them
				// to protect against click-jacking in an iframe whose javascript is disabled.
				document.getElementById('responseButtonsDiv').style.display = 'block';
				document.getElementById('javascriptDisabled').style.display = 'none';

				// Frame busting code (to protect us from being hosted in an iframe).
				// This protects us from click-jacking.
				if (document.location !== window.top.location) {
					window.top.location = document.location;
				}
				//]]>
			</script>

		</asp:View>
		<asp:View ID="authorizationGrantedView" runat="server">
			<p>Authorization has been granted.</p>
			<asp:MultiView runat="server" ID="verifierMultiView" ActiveViewIndex="0">
				<asp:View ID="verificationCodeView" runat="server">
					<p>You must enter this verification code at the Consumer: <asp:Label runat="server"
						ID="verificationCodeLabel" /> </p>
				</asp:View>
				<asp:View ID="noCallbackView" runat="server">
					<p>You may now close this window and return to the Consumer. </p>
				</asp:View>
			</asp:MultiView>
		</asp:View>
		<asp:View ID="authorizationDeniedView" runat="server">
			<p>Authorization has been denied. You're free to do whatever now. </p>
		</asp:View>
	</asp:MultiView>
</asp:Content>
