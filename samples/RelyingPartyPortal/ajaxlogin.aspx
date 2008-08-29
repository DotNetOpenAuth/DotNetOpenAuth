<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ajaxlogin.aspx.cs" Inherits="ConsumerPortal.ajaxlogin"
	ValidateRequest="false" MasterPageFile="~/Site.Master" %>

<%@ Register Assembly="DotNetOpenId" Namespace="DotNetOpenId.RelyingParty" TagPrefix="cc1" %>
<asp:Content runat="server" ContentPlaceHolderID="Main">
	<asp:MultiView runat="server" ID="multiView" ActiveViewIndex='0'>
		<asp:View runat="server" ID="commentSubmission">
			<table>
				<tr>
					<td>
						OpenID
					</td>
					<td>
						<cc1:OpenIdAjaxTextBox ID="OpenIdAjaxTextBox1" runat="server" 
							OnLoggingIn="OpenIdAjaxTextBox1_LoggingIn" 
							OnLoggedIn="OpenIdAjaxTextBox1_LoggedIn"
							OnClientAssertionReceived="alert('Demonstration of page-injected javascript handling authentication:\nClaimed Identifier is ' + sender.getClaimedIdentifier())" />
					</td>
				</tr>
				<tr>
					<td>
						Email
					</td>
					<td>
						<asp:TextBox runat="server" ID="emailAddressBox" />
					</td>
				</tr>
				<tr>
					<td>
						Comments
					</td>
					<td>
						<asp:TextBox runat="server" ID="commentsBox" />
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
