<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" Inherits="OAuthServiceProvider.Authorize" Codebehind="Authorize.aspx.cs" Async="true" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:MultiView runat="server" ActiveViewIndex="0" ID="multiView">
		<asp:View runat="server">
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
				<b>Javascript appears to be disabled in your browser. </b>This page requires Javascript
				to be enabled to better protect your security.
			</div>
			<p>If you grant access now, you can revoke it at any time by returning to this page.
			</p>
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
		<asp:View runat="server">
			<p>Authorization has been granted.</p>
			<asp:MultiView runat="server" ID="verifierMultiView" ActiveViewIndex="0">
				<asp:View runat="server">
					<p>You must enter this verification code at the Consumer: <asp:Label runat="server"
						ID="verificationCodeLabel" /> </p>
				</asp:View>
				<asp:View ID="View1" runat="server">
					<p>You may now close this window and return to the Consumer. </p>
				</asp:View>
			</asp:MultiView>
		</asp:View>
		<asp:View runat="server">
			<p>Authorization has been denied. You're free to do whatever now. </p>
		</asp:View>
	</asp:MultiView>
</asp:Content>
