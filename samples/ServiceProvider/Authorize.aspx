<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="Authorize.aspx.cs" Inherits="Authorize" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<div style="background-color: Yellow">
		<b>Warning</b>: Never give your login credentials to another web site or application.
	</div>
	<p>The client web site or application
		<asp:Label ID="consumerLabel" Font-Bold="true" runat="server" Text="[consumer]" />
		wants access to your
		<asp:Label ID="desiredAccessLabel" Font-Bold="true" runat="server" Text="[protected resource]" />.
	</p>
	<p>Do you want to allow this? </p>
	<div>
		<asp:Button ID="allowAccessButton" runat="server" Text="Yes" />
		<asp:Button ID="denyAccessButton" runat="server" Text="No" />
	</div>
	<p>If you grant access now, you can revoke it at any time by returning to this page.
	</p>
</asp:Content>
