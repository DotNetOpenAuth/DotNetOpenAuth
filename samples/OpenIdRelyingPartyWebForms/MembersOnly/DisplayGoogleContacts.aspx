<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="DisplayGoogleContacts.aspx.cs"
	Inherits="OpenIdRelyingPartyWebForms.MembersOnly.DisplayGoogleContacts" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="Main">
	<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
		<asp:View ID="View1" runat="server">
			Obtain an access token by <asp:HyperLink NavigateUrl="~/loginPlusOAuth.aspx" runat="server"
				Text="logging in at our OpenID+OAuth hybrid login page" />.
		</asp:View>
		<asp:View ID="View2" runat="server">
			<h2>Address book</h2>
			<p>These are the contacts for Google Account: <asp:Label ID="emailLabel" runat="server"
				Font-Bold="True" /> and OpenID <asp:Label ID="claimedIdLabel" runat="server" Font-Bold="True" /></p>
			<asp:PlaceHolder ID="resultsPlaceholder" runat="server" />
		</asp:View>
	</asp:MultiView>
</asp:Content>
