<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" Inherits="OAuthConsumer.Twitter" Codebehind="Twitter.aspx.cs" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<h2>Twitter setup</h2>
			<p>A Twitter client app must be endorsed by a Twitter user. </p>
			<ol>
				<li><a target="_blank" href="https://twitter.com/oauth_clients">Visit Twitter and create
					a client app</a>. </li>
				<li>Modify your web.config file to include your consumer key and consumer secret.</li>
			</ol>
		</asp:View>
		<asp:View runat="server">
			<h2>Updates</h2>
			<p>Ok, Twitter has authorized us to download your feeds. Notice how we never asked 
				you for your Twitter username or password. </p>
			<p>
				Upload a new profile photo:
				<asp:FileUpload ID="profilePhoto" runat="server" />
				&nbsp;<asp:Button ID="uploadProfilePhotoButton" runat="server" 
					onclick="uploadProfilePhotoButton_Click" Text="Upload photo" />
				&nbsp;<asp:Label ID="photoUploadedLabel" runat="server" EnableViewState="False" 
					Text="Done!" Visible="False"></asp:Label>
			</p>
			<p>
				Click &#39;Get updates&#39; to download updates to this sample.
			</p>
			<asp:Button ID="downloadUpdates" runat="server" Text="Get updates" OnClick="downloadUpdates_Click" />
			<asp:PlaceHolder runat="server" ID="resultsPlaceholder" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
