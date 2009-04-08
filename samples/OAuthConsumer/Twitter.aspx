<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="Twitter.aspx.cs" Inherits="Twitter" %>

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
			<p>Ok, Twitter has authorized us to download your feeds. Click 'Get updates' to download
				updates to this sample. Notice how we never asked you for your Twitter username
				or password. </p>
			<asp:Button ID="downloadUpdates" runat="server" Text="Get updates" OnClick="downloadUpdates_Click" />
			<asp:PlaceHolder runat="server" ID="resultsPlaceholder" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
