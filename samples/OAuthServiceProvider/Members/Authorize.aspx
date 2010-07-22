<%@ Page Title="Authorize Access" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeBehind="Authorize.aspx.cs" Inherits="OAuthServiceProvider.Members.Authorize2" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="server">
	<asp:MultiView runat="server" ActiveViewIndex="0" ID="multiView">
		<asp:View ID="AuthRequest" runat="server">
			<div style="background-color: Yellow">
				<b>Warning</b>: Never give your login credentials to another web site or application.
			</div>
			<asp:HiddenField runat="server" ID="OAuthAuthorizationSecToken" EnableViewState="false" />
			<p>The client web site or application <asp:Label ID="consumerLabel" Font-Bold="true"
				runat="server" Text="[consumer]" /> wants access to your <asp:Label ID="desiredAccessLabel"
					Font-Bold="true" runat="server" Text="[protected resource]" />. </p>
			<p>Do you want to allow this? </p>
			<div style="display: none" id="responseButtonsDiv">
				<asp:Button ID="allowAccessButton" runat="server" Text="Yes" OnClick="allowAccessButton_Click" />
				<asp:Button ID="denyAccessButton" runat="server" Text="No" OnClick="denyAccessButton_Click" />
			</div>
			<div id="javascriptDisabled">
				<b>JavaScript appears to be disabled in your browser. </b>This page requires Javascript
				to be enabled to better protect your security.
			</div>
			<p>If you grant access now, you can revoke it at any time by returning to this page.
			</p>
			<script language="javascript" type="text/javascript">
				//<![CDATA[
				// we use HTML to hide the action buttons and JavaScript to show them
				// to protect against click-jacking in an iframe whose JavaScript is disabled.
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
		<asp:View ID="AuthGranted" runat="server">
			<p>Authorization has been granted.</p>
			<asp:MultiView runat="server" ID="verifierMultiView" ActiveViewIndex="0">
				<asp:View ID="View3" runat="server">
					<p>You must enter this verification code at the Consumer: <asp:Label runat="server"
						ID="verificationCodeLabel" /> </p>
				</asp:View>
				<asp:View ID="View4" runat="server">
					<p>You may now close this window and return to the Consumer. </p>
				</asp:View>
			</asp:MultiView>
		</asp:View>
	</asp:MultiView>
</asp:Content>
