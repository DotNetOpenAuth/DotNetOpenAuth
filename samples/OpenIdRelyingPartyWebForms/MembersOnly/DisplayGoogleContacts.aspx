<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="DisplayGoogleContacts.aspx.cs"
	Inherits="OpenIdRelyingPartyWebForms.MembersOnly.DisplayGoogleContacts" Async="true" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			<p>Obtain an access token by <asp:HyperLink NavigateUrl="~/loginPlusOAuth.aspx" runat="server"
				Text="logging in at our OpenID+OAuth hybrid login page" />. </p>
			<p>If you've already done that, then you might have inadvertently clicked "Allow [this
				site] to remember me", which causes Google to stop sending the access token that
				this sample doesn't save. If you did check it, you can restore this sample&#39;s
				functionality by <a href="https://www.google.com/accounts/IssuedAuthSubTokens">revoking
					access</a> to this site from your Google Account. </p>
		</asp:View>
		<asp:View ID="View2" runat="server">
			<h2>Address book</h2>
			<p>These are the contacts for Google Account: <asp:Label ID="emailLabel" runat="server"
				Font-Bold="True" /> and OpenID <asp:Label ID="claimedIdLabel" runat="server" Font-Bold="True" /></p>
			<asp:PlaceHolder ID="resultsPlaceholder" runat="server" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
