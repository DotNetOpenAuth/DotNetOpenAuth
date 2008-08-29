<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ajaxlogin.aspx.cs" Inherits="ConsumerPortal.ajaxlogin"
	ValidateRequest="false" MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="openid" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
<style type="text/css">
.textbox
{
	width: 200px;
}
.openidtextbox
{
	width: 185px;
}
td
{
	vertical-align: top;
}
</style>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="Main">
<script type="text/javascript">
	function onauthenticated(sender) {
		var emailBox = document.getElementById('ctl00_Main_emailAddressBox');
		emailBox.disabled = false;
		emailBox.title = null; // remove tooltip describing why the box was disabled.
		// the sreg response may not always be included.
		if (sender.sreg) {
			// and the email field may not always be included in the sreg response.
			if (sender.sreg.email) { emailBox.value = sender.sreg.email; }
		}
	}
</script>

	<asp:MultiView runat="server" ID="multiView" ActiveViewIndex='0'>
		<asp:View runat="server" ID="commentSubmission">
			<table>
				<tr>
					<td>
						OpenID
					</td>
					<td>
						<openid:OpenIdAjaxTextBox ID="OpenIdAjaxTextBox1" runat="server" CssClass="openidtextbox"
							OnLoggingIn="OpenIdAjaxTextBox1_LoggingIn" 
							OnLoggedIn="OpenIdAjaxTextBox1_LoggedIn"
							OnClientAssertionReceived="onauthenticated(sender)"
							OnUnconfirmedPositiveAssertion="OpenIdAjaxTextBox1_UnconfirmedPositiveAssertion" />
					</td>
				</tr>
				<tr>
					<td>
						Email
					</td>
					<td>
						<asp:TextBox runat="server" ID="emailAddressBox" Enabled="false" CssClass="textbox" ToolTip="This field will be enabled after you log in with your OpenID." />
					</td>
				</tr>
				<tr>
					<td>
						Comments
					</td>
					<td>
						<asp:TextBox runat="server" ID="commentsBox" TextMode="MultiLine" Rows="5" CssClass="textbox" />
					</td>
				</tr>
				<tr>
					<td />
					<td>
						<asp:Button runat="server" Text="Submit" ID="submitButton" OnClick="submitButton_Click" />
					</td>
				</tr>
			</table>
		</asp:View>
		<asp:View runat="server" ID="commentSubmitted">
			<p>Congratulations,
				<asp:Label runat="server" ID="emailLabel" />! Your comment was received (and discarded...
				this is just a demo after all).</p>
			<asp:LinkButton runat="server" Text="Go back and change something in the comment"
				OnClick="editComment_Click" />
		</asp:View>
		<asp:View runat="server" ID="commentFailed">
			<p>Your comment submission failed.</p>
		</asp:View>
	</asp:MultiView>
</asp:Content>
