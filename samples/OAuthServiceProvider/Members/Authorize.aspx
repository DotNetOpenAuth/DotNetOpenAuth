<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="Authorize.aspx.cs" Inherits="Authorize" %>

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
			<div>
				<asp:Button ID="allowAccessButton" runat="server" Text="Yes" OnClick="allowAccessButton_Click" />
				<asp:Button ID="denyAccessButton" runat="server" Text="No" OnClick="denyAccessButton_Click" />
			</div>
			<p>If you grant access now, you can revoke it at any time by returning to this page.
			</p>
			<asp:Panel runat="server" BackColor="Red" ForeColor="White" Font-Bold="true" Visible="false" ID="OAuth10ConsumerWarning">
				This website is registered with service_PROVIDER_DOMAIN_NAME to make authorization requests, but has not been configured to send requests securely. If you grant access but you did not initiate this request at consumer_DOMAIN_NAME, it may be possible for other users of consumer_DOMAIN_NAME to access your data. We recommend you deny access unless you are certain that you initiated this request directly with consumer_DOMAIN_NAME.
			</asp:Panel>
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
