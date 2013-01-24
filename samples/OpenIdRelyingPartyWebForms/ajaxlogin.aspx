<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ajaxlogin.aspx.cs" Inherits="OpenIdRelyingPartyWebForms.ajaxlogin"
	ValidateRequest="false" MasterPageFile="~/Site.Master" Async="true" %>

<%@ Register Assembly="DotNetOpenAuth.OpenId.RelyingParty.UI" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="openid" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
<script>
//	window.openid_visible_iframe = true; // causes the hidden iframe to show up
//	window.openid_trace = true; // causes lots of messages
</script>
<style type="text/css">
.textbox
{
	width: 200px;
}
input.openidtextbox
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
	function onauthenticated(sender, e) {
		var emailBox = document.getElementById('<%= emailAddressBox.ClientID %>');
		emailBox.disabled = false;
		emailBox.title = null; // remove tooltip describing why the box was disabled.
		// the sreg response may not always be included.
		if (e && e.sreg) {
			// and the email field may not always be included in the sreg response.
			if (e.sreg.email) { emailBox.value = e.sreg.email; }
		}
	}
</script>

	<asp:MultiView runat="server" ID="multiView" ActiveViewIndex='0'>
		<asp:View runat="server" ID="commentSubmission">
			<p>
				The scenario here is that you've just read a blog post and you want to comment on
				that post. You're <b>not</b> actually logging into the web site by entering your
				OpenID here, but your OpenID <i>will</i> be verified before the comment is posted.
			</p>
			<table>
				<tr>
					<td>
						OpenID
					</td>
					<td>
						<openid:OpenIdAjaxTextBox ID="OpenIdAjaxTextBox1" runat="server" CssClass="openidtextbox"
							OnLoggingIn="OpenIdAjaxTextBox1_LoggingIn" 
							OnLoggedIn="OpenIdAjaxTextBox1_LoggedIn"
							OnClientAssertionReceived="onauthenticated(sender, e)"
							OnUnconfirmedPositiveAssertion="OpenIdAjaxTextBox1_UnconfirmedPositiveAssertion" />
						<asp:RequiredFieldValidator ID="openidRequiredValidator" runat="server" 
							ControlToValidate="OpenIdAjaxTextBox1" ValidationGroup="openidVG"
							ErrorMessage="The OpenID field is required." SetFocusOnError="True">
							<asp:Image runat="server" ImageUrl="~/images/attention.png" ToolTip="This is a required field" />
						</asp:RequiredFieldValidator>
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
					<td></td>
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
