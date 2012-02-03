<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.master"
	CodeBehind="Yammer.aspx.cs" Inherits="OAuthConsumer.Yammer" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="ClientRegistrationRequiredView" runat="server">
			<h2>
				Yammer setup</h2>
			<p>
				A Yammer client app must be registered.
			</p>
			<ol>
				<li><a target="_blank" href="https://www.yammer.com/client_applications/new">Visit Yammer
					and register a client app</a>. </li>
				<li>Modify your web.config file to include your consumer key and consumer secret.
				</li>
			</ol>
		</asp:View>
		<asp:View ID="BeginAuthorizationView" runat="server">
			<asp:Label Text="An error occurred in authorization.  You may try again." EnableViewState="false" Visible="false" ForeColor="Red" ID="authorizationErrorLabel" runat="server" />
			<asp:Button Text="Obtain authorization now" runat="server" ID="obtainAuthorizationButton"
				OnClick="obtainAuthorizationButton_Click" />
		</asp:View>
		<asp:View ID="CompleteAuthorizationView" runat="server">
			After you have authorized Yammer to share your information, please enter the code
			Yammer gives you here:
			<asp:TextBox runat="server" ID="yammerUserCode" EnableViewState="false" />
			<asp:RequiredFieldValidator ErrorMessage="*" ControlToValidate="yammerUserCode" runat="server" />
			<asp:Button Text="Finish" runat="server" ID="finishAuthorizationButton" OnClick="finishAuthorizationButton_Click" />
		</asp:View>
		<asp:View ID="AuthorizationCompleteView" runat="server">
			<h2>
				Updates
			</h2>
			<p>The access token we have obtained is: 
				<asp:Label ID="accessTokenLabel" runat="server" />
			</p>
			<p>
				Ok, Yammer has authorized us to download your messages. Click &#39;Get messages&#39;
				to download the latest few messages to this sample. Notice how we never asked you
				for your Yammer username or password.
			</p>
			<asp:Button ID="getYammerMessagesButton" runat="server" OnClick="getYammerMessages_Click"
				Text="Get address book" />
			<asp:PlaceHolder ID="resultsPlaceholder" runat="server" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
