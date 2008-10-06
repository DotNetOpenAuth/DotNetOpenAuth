<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
	CodeFile="Authorize.aspx.cs" Inherits="Authorize" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Body" runat="Server">
	<asp:MultiView runat="server" ActiveViewIndex="0" ID="multiView">
		<asp:View runat="server">
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
				<asp:Button ID="allowAccessButton" runat="server" Text="Yes" OnClick="allowAccessButton_Click" />
				<asp:Button ID="denyAccessButton" runat="server" Text="No" 
					onclick="denyAccessButton_Click" />
			</div>
			<p>If you grant access now, you can revoke it at any time by returning to this page.
			</p>
		</asp:View>
		<asp:View runat="server">
			<p>Authorization has been granted. Please inform the consumer application or web site
				of this. </p>
		</asp:View>
		<asp:View runat="server">
			<p>Authorization has been denied. You're free to do whatever now. </p>
		</asp:View>
	</asp:MultiView>
</asp:Content>
