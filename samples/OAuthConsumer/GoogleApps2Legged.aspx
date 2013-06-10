<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPage.master"CodeBehind="GoogleApps2Legged.aspx.cs" Inherits="OAuthConsumer.GoogleApps2Legged" Async="true" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View runat="server">
			<h2>Google setup</h2>
			<p>A Google client app must be endorsed by a Google user. </p>
			<ol>
				<li><a target="_blank" href="https://www.google.com/accounts/ManageDomains">Visit Google
					and create a client app</a>. </li>
				<li>Modify your web.config file to include your consumer key and consumer secret.
				</li>
			</ol>
		</asp:View>
		<asp:View runat="server">
			<h2>Updates</h2>
			<p>Ok, Google has authorized us to download your contacts. Click &#39;Get address book&#39;
				to download the first 5 contacts to this sample. Notice how we never asked you
				for your Google username or password. </p>
			<asp:Button ID="getAddressBookButton" runat="server" OnClick="getAddressBookButton_Click"
				Text="Get address book" />
			<asp:PlaceHolder ID="resultsPlaceholder" runat="server" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
